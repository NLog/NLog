﻿// 
// Copyright (c) 2004-2017 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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
using System.Text;
using NLog.Internal;

namespace NLog.MessageTemplates
{
    /// <summary>
    /// Convert Render or serialize a value, with optionnally backwardscompatible with <see cref="string.Format(System.IFormatProvider,string,object[])"/>
    /// </summary>
    internal class ValueSerializer : IValueSerializer
    {
        public static IValueSerializer Instance
        {
            get => _instance ?? (_instance = new ValueSerializer());
            set => _instance = value ?? new ValueSerializer();
        }
        private static IValueSerializer _instance;

        /// <summary>Singleton</summary>
        private ValueSerializer()
        {
        }

        private const int MaxRecursionDepth = 10;
        private const int MaxValueLength = 512 * 1024;
        private const string LiteralFormatSymbol = "l";

        private readonly MruCache<Enum, string> _enumCache = new MruCache<Enum, string>(1500);

        /// <inheritDoc/>
        public bool StringifyObject(object value, string format, IFormatProvider formatProvider, StringBuilder builder)
        {
            builder.Append('"');
            FormatToString(value, null, formatProvider, builder);
            builder.Append('"');
            return true;
        }

        /// <inheritDoc/>
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

        /// <inheritDoc/>
        public bool SerializeObject(object value, string format, IFormatProvider formatProvider, StringBuilder builder)
        {
            return Config.ConfigurationItemFactory.Default.JsonConverter.SerializeObject(value, builder);
        }

        private bool SerializeSimpleObject(object value, string format, IFormatProvider formatProvider, StringBuilder builder)
        {
            // todo support all scalar types: 

            // todo byte[] - hex?
            // todo datetime, timespan, datetimeoffset
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
                                AppendIntegerAsString(builder, value, objTypeCode);
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

        private static void AppendIntegerAsString(StringBuilder sb, object value, TypeCode objTypeCode)
        {
            switch (objTypeCode)
            {
                case TypeCode.Byte: sb.AppendInvariant((byte)value); break;
                case TypeCode.SByte: sb.AppendInvariant((sbyte)value); break;
                case TypeCode.Int16: sb.AppendInvariant((short)value); break;
                case TypeCode.Int32: sb.AppendInvariant((int)value); break;
                case TypeCode.Int64:
                    {
                        long int64 = (long)value;
                        if (int64 < int.MaxValue && int64 > int.MinValue)
                            sb.AppendInvariant((int)int64);
                        else
                            sb.Append(int64);
                    }
                    break;
                case TypeCode.UInt16: sb.AppendInvariant((ushort)value); break;
                case TypeCode.UInt32: sb.AppendInvariant((uint)value); break;
                case TypeCode.UInt64:
                    {
                        ulong uint64 = (ulong)value;
                        if (uint64 < uint.MaxValue)
                            sb.AppendInvariant((uint)uint64);
                        else
                            sb.Append(uint64);
                    }
                    break;
                default:
                    sb.Append(XmlHelper.XmlConvertToString(value, objTypeCode));
                    break;
            }
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
                using (new SingleItemOptimizedHashSet<object>.SingleItemScopedInsert(dictionary, ref objectsInPath, true))
                {
                    return SerializeDictionaryObject(dictionary, format, formatProvider, builder, objectsInPath, depth);
                }
            }

            using (new SingleItemOptimizedHashSet<object>.SingleItemScopedInsert(collection, ref objectsInPath, true))
            {
                return SerializeCollectionObject(collection, format, formatProvider, builder, objectsInPath, depth);
            }
        }

        private bool SerializeDictionaryObject(IDictionary dictionary, string format, IFormatProvider formatProvider, StringBuilder builder, SingleItemOptimizedHashSet<object> objectsInPath, int depth)
        {
            bool separator = false;
            foreach (DictionaryEntry item in dictionary)
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
