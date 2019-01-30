// 
// Copyright (c) 2004-2018 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
// 
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without 
// modification, are permitted provided that the following conditions 
// are met:
// 
// * Redistributions of source code must retain the above copyright notice, 
//   this list of conditions and the following disclaimer. 
// 
// * Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution. 
// 
// * Neither the name of Jaroslaw Kowalski nor the names of its 
//   contributors may be used to endorse or promote products derived from this
//   software without specific prior written permission. 
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE 
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE 
// ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE 
// LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR 
// CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
// SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS 
// INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN 
// CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) 
// ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF 
// THE POSSIBILITY OF SUCH DAMAGE.
// 

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using NLog.Config;
using NLog.Internal;

namespace NLog.MessageTemplates
{
    /// <summary>
    /// Convert Render or serialize a value, with optionnally backwardscompatible with <see cref="string.Format(System.IFormatProvider,string,object[])"/>
    /// </summary>
    internal class ValueFormatter : IValueFormatter
    {
        public static IValueFormatter Instance
        {
            get => _instance ?? (_instance = new ValueFormatter());
            set => _instance = value ?? new ValueFormatter();
        }
        private static IValueFormatter _instance;
        private static readonly IEqualityComparer<object> _referenceEqualsComparer = SingleItemOptimizedHashSet<object>.ReferenceEqualityComparer.Default;

        /// <summary>Singleton</summary>
        private ValueFormatter()
        {
        }

        private const int MaxRecursionDepth = 2;
        private const int MaxValueLength = 512 * 1024;
        private const string LiteralFormatSymbol = "l";

        private readonly MruCache<Enum, string> _enumCache = new MruCache<Enum, string>(1500);

        public const string FormatAsJson = "@";
        public const string FormatAsString = "$";

        /// <summary>
        /// Serialization of an object, e.g. JSON and append to <paramref name="builder"/>
        /// </summary>
        /// <param name="value">The object to serialize to string.</param>
        /// <param name="format">Parameter Format</param>
        /// <param name="captureType">Parameter CaptureType</param>
        /// <param name="formatProvider">An object that supplies culture-specific formatting information.</param>
        /// <param name="builder">Output destination.</param>
        /// <returns>Serialize succeeded (true/false)</returns>
        public bool FormatValue(object value, string format, CaptureType captureType, IFormatProvider formatProvider, StringBuilder builder)
        {
            switch (captureType)
            {
                case CaptureType.Serialize:
                    {
                        return ConfigurationItemFactory.Default.JsonConverter.SerializeObject(value, builder);
                    }
                case CaptureType.Stringify:
                    {
                        builder.Append('"');
                        FormatToString(value, null, formatProvider, builder);
                        builder.Append('"');
                        return true;
                    }
                default:
                    {
                        return FormatObject(value, format, formatProvider, builder);
                    }
            }
        }

        /// <summary>
        /// Format an object to a readable string, or if it's an object, serialize
        /// </summary>
        /// <param name="value">The value to convert</param>
        /// <param name="format"></param>
        /// <param name="formatProvider"></param>
        /// <param name="builder"></param>
        /// <returns></returns>
        public bool FormatObject(object value, string format, IFormatProvider formatProvider, StringBuilder builder)
        {
            if (SerializeSimpleObject(value, format, formatProvider, builder))
            {
                return true;
            }

            IEnumerable collection = value as IEnumerable;
            if (collection != null)
            {
                return SerializeWithoutCyclicLoop(collection, format, formatProvider, builder, default(SingleItemOptimizedHashSet<object>), 0);
            }

            builder.Append(Convert.ToString(value, formatProvider));
            return true;
        }

        /// <summary>
        /// Try serialising a scalar (string, int, NULL) or simple type (IFormattable)
        /// </summary>
        /// <param name="value"></param>
        /// <param name="format"></param>
        /// <param name="formatProvider"></param>
        /// <param name="builder"></param>
        /// <returns></returns>
        private bool SerializeSimpleObject(object value, string format, IFormatProvider formatProvider, StringBuilder builder)
        {
            // todo support all scalar types: 

            // todo byte[] - hex?
            // todo nullables correct?

            if (value is string stringValue)
            {
                bool includeQuotes = format != LiteralFormatSymbol;
                if (includeQuotes) builder.Append('"');
                builder.Append(stringValue);
                if (includeQuotes) builder.Append('"');
                return true;
            }

            if (value == null)
            {
                builder.Append("NULL");
                return true;
            }

            IFormattable formattable;
            if (!string.IsNullOrEmpty(format) && (formattable = value as IFormattable) != null)
            {
                builder.Append(formattable.ToString(format, formatProvider));
                return true;
            }
            else
            {
                // Optimize for types that are pretty much invariant in all cultures when no format-string
                TypeCode objTypeCode = Convert.GetTypeCode(value);
                switch (objTypeCode)
                {
                    case TypeCode.Boolean:
                        {
                            builder.Append(((bool)value) ? "true" : "false");
                            return true;
                        }
                    case TypeCode.Char:
                        {
                            bool includeQuotes = format != LiteralFormatSymbol;
                            if (includeQuotes) builder.Append('"');
                            builder.Append((char)value);
                            if (includeQuotes) builder.Append('"');
                            return true;
                        }

                    case TypeCode.Byte:
                    case TypeCode.SByte:
                    case TypeCode.Int16:
                    case TypeCode.Int32:
                    case TypeCode.Int64:
                    case TypeCode.UInt16:
                    case TypeCode.UInt32:
                    case TypeCode.UInt64:
                        {
                            Enum enumValue;
                            if ((enumValue = value as Enum) != null)
                            {
                                AppendEnumAsString(builder, enumValue);
                            }
                            else
                            {
                                builder.AppendIntegerAsString(value, objTypeCode);
                            }
                        }
                        return true;

                    case TypeCode.Object:   // Guid, TimeSpan, DateTimeOffset
                    default:                // Single, Double, Decimal, etc.
                        break;
                }
            }

            return false;
        }

        private void AppendEnumAsString(StringBuilder sb, Enum value)
        {
            string textValue;
            if (!_enumCache.TryGetValue(value, out textValue))
            {
                textValue = value.ToString();
                _enumCache.TryAddValue(value, textValue);
            }
            sb.Append(textValue);
        }

        private bool SerializeWithoutCyclicLoop(IEnumerable collection, string format, IFormatProvider formatProvider, StringBuilder builder,
                SingleItemOptimizedHashSet<object> objectsInPath, int depth)
        {
            if (objectsInPath.Contains(collection))
            {
                return false; // detected reference loop, skip serialization
            }
            if (depth > MaxRecursionDepth)
            {
                return false; // reached maximum recursion level, no further serialization
            }

            IDictionary dictionary = collection as IDictionary;
            if (dictionary != null)
            {
                using (new SingleItemOptimizedHashSet<object>.SingleItemScopedInsert(dictionary, ref objectsInPath, true, _referenceEqualsComparer))
                {
                    return SerializeDictionaryObject(dictionary, format, formatProvider, builder, objectsInPath, depth);
                }
            }

            using (new SingleItemOptimizedHashSet<object>.SingleItemScopedInsert(collection, ref objectsInPath, true, _referenceEqualsComparer))
            {
                return SerializeCollectionObject(collection, format, formatProvider, builder, objectsInPath, depth);
            }
        }

        /// <summary>
        /// Serialize Dictionary as JSON like structure, without { and }
        /// </summary>
        /// <example>
        /// "FirstOrder"=true, "Previous login"=20-12-2017 14:55:32, "number of tries"=1
        /// </example>
        /// <param name="dictionary"></param>
        /// <param name="format">formatstring of an item</param>
        /// <param name="formatProvider"></param>
        /// <param name="builder"></param>
        /// <param name="objectsInPath"></param>
        /// <param name="depth"></param>
        /// <returns></returns>
        private bool SerializeDictionaryObject(IDictionary dictionary, string format, IFormatProvider formatProvider, StringBuilder builder, SingleItemOptimizedHashSet<object> objectsInPath, int depth)
        {
            bool separator = false;
            foreach (var item in new DictionaryEntryEnumerable(dictionary))
            {
                if (builder.Length > MaxValueLength)
                    return false;

                if (separator) builder.Append(", ");

                if (item.Key is string || !(item.Key is IEnumerable))
                    FormatObject(item.Key, format, formatProvider, builder);
                else
                    SerializeWithoutCyclicLoop((IEnumerable)item.Key, format, formatProvider, builder, objectsInPath, depth + 1);
                builder.Append("=");
                if (item.Value is string || !(item.Value is IEnumerable))
                    FormatObject(item.Value, format, formatProvider, builder);
                else
                    SerializeWithoutCyclicLoop((IEnumerable)item.Value, format, formatProvider, builder, objectsInPath, depth + 1);
                separator = true;
            }
            return true;
        }

        private bool SerializeCollectionObject(IEnumerable collection, string format, IFormatProvider formatProvider, StringBuilder builder, SingleItemOptimizedHashSet<object> objectsInPath, int depth)
        {
            bool separator = false;
            foreach (var item in collection)
            {
                if (builder.Length > MaxValueLength)
                    return false;

                if (separator) builder.Append(", ");

                if (item is string || !(item is IEnumerable))
                    FormatObject(item, format, formatProvider, builder);
                else
                    SerializeWithoutCyclicLoop((IEnumerable)item, format, formatProvider, builder, objectsInPath, depth + 1);

                separator = true;
            }
            return true;
        }

        /// <summary>
        /// Convert a value to a string with format and append to <paramref name="builder"/>.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <param name="format">Format sting for the value.</param>
        /// <param name="formatProvider">Format provider for the value.</param>
        /// <param name="builder">Append to this</param>
        public static void FormatToString(object value, string format, IFormatProvider formatProvider, StringBuilder builder)
        {
            var stringValue = value as string;
            if (stringValue != null)
            {
                builder.Append(stringValue);
            }
            else
            {
                var formattable = value as IFormattable;
                if (formattable != null)
                {
                    builder.Append(formattable.ToString(format, formatProvider));
                }
                else
                {
                    builder.Append(Convert.ToString(value, formatProvider));
                }
            }
        }
    }
}
