﻿// 
// Copyright (c) 2004-2016 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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
using System.Globalization;
using System.Reflection;
using System.Text;
using NLog.Internal;
using NLog.StructuredEvents.Serialization;

namespace NLog.Targets
{
    /// <summary>
    /// Default class for serialization of values to JSON format.
    /// </summary>
#pragma warning disable 618
    public class DefaultJsonSerializer : IJsonConverter, NLog.Targets.IJsonSerializer, ISerializer
#pragma warning restore 618
    {
        private readonly MruCache<Type, KeyValuePair<PropertyInfo[], ReflectionHelpers.LateBoundMethod[]>> _propsCache = new MruCache<Type, KeyValuePair<PropertyInfo[], ReflectionHelpers.LateBoundMethod[]>>(10000);
        private readonly MruCache<Enum, string> _enumCache = new MruCache<Enum, string>(10000);
        private readonly JsonSerializeOptions _serializeOptions = new JsonSerializeOptions();
        private readonly IFormatProvider _defaultFormatProvider = CreateFormatProvider();

        private const int MaxRecursionDepth = 10;

        private static readonly DefaultJsonSerializer instance;

        /// <summary>
        /// Singleton instance of the serializer.
        /// </summary>
        public static DefaultJsonSerializer Instance
        {
            get { return instance; }
        }

        static DefaultJsonSerializer()
        {
            instance = new DefaultJsonSerializer();
        }

        private DefaultJsonSerializer()
        { }

        /// <summary>
        /// Returns a serialization of an object
        /// int JSON format.
        /// </summary>
        /// <param name="value">The object to serialize to JSON.</param>
        /// <returns>Serialized value.</returns>
        public string SerializeObject(object value)
        {
            return SerializeObject(value, _serializeOptions);
        }

        /// <summary>
        /// Returns a serialization of an object
        /// int JSON format.
        /// </summary>
        /// <param name="value">The object to serialize to JSON.</param>
        /// <param name="options">Options</param>
        /// <returns>Serialized value.</returns>
        public string SerializeObject(object value, JsonSerializeOptions options)
        {
            string str;
            if (value == null)
            {
                return "null";
            }
            else if ((str = value as string) != null)
            {
                return QuoteValue(EscapeString(str, options.EscapeUnicode));
            }
            else
            {
                TypeCode objTypeCode = Convert.GetTypeCode(value);
                if (objTypeCode != TypeCode.Object && StringHelpers.IsNullOrWhiteSpace(options.Format) && options.FormatProvider == null)
                {
                    Enum enumValue;
                    if (!options.EnumAsInteger && IsNumericTypeCode(objTypeCode, false) && (enumValue = value as Enum) != null)
                    {
                        return QuoteValue(EnumAsString(enumValue));
                    }
                    else
                    {
                        str = XmlHelper.XmlConvertToString(value, objTypeCode);
                        if (SkipQuotes(objTypeCode))
                        {
                            return str;
                        }
                        else
                        {
                            return QuoteValue(str);
                        }
                    }
                }
                else
                {
                    StringBuilder sb = new StringBuilder();
                    if (!SerializeObject(value, sb, options))
                    {
                        return null;
                    }
                    return sb.ToString();
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="destination"></param>
        /// <returns></returns>
        public bool SerializeObject(object value, StringBuilder destination)
        {
            return SerializeObject(value, destination, _serializeOptions);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="destination"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public bool SerializeObject(object value, StringBuilder destination, JsonSerializeOptions options)
        {
            return SerializeObject(value, destination, options, default(SingleItemOptimizedHashSet<object>), 0);
        }

        /// <summary>
        /// Returns a serialization of an object
        /// int JSON format.
        /// </summary>
        /// <param name="value">The object to serialize to JSON.</param>
        /// <param name="destination">The object to serialize to JSON.</param>
        /// <param name="options">serialisation options</param>
        /// <param name="objectsInPath">The objects in path.</param>
        /// <param name="depth">The current depth (level) of recursion.</param>
        /// <returns>
        /// Serialized value.
        /// </returns>
        private bool SerializeObject(object value, StringBuilder destination, JsonSerializeOptions options,
                SingleItemOptimizedHashSet<object> objectsInPath, int depth)
        {
            if (objectsInPath.Contains(value))
            {
                return false; // detected reference loop, skip serialization
            }
            if (depth > MaxRecursionDepth)
            {
                return false; // reached maximum recursion level, no further serialization
            }

            IEnumerable enumerable;
            IDictionary dict;
            string str;

            if (value == null)
            {
                destination.Append("null");
            }
            else if ((str = value as string) != null)
            {
                QuoteValue(destination, EscapeString(str, options.EscapeUnicode));
            }
            else if ((dict = value as IDictionary) != null)
            {
                using (new SingleItemOptimizedHashSet<object>.SingleItemScopedInsert(dict, ref objectsInPath, true))
                {
                    SerializeDictionaryObject(dict, destination, options, objectsInPath, depth);
                }
            }
            else if ((enumerable = value as IEnumerable) != null)
            {
                using (new SingleItemOptimizedHashSet<object>.SingleItemScopedInsert(value, ref objectsInPath, true))
                {
                    SerializeCollectionObject(enumerable, destination, options, objectsInPath, depth);
                }
            }
            else
            {
                IFormattable formattable;
                var format = options.Format;
                var hasFormat = !StringHelpers.IsNullOrWhiteSpace(format);
                if ((options.FormatProvider != null || hasFormat) && (formattable = value as IFormattable) != null)
                {
                    if (!SerializeWithFormatProvider(value, destination, options, formattable, format, hasFormat))
                    {
                        return false;
                    }
                }
                else
                {
                    if (!SerializeTypeCodeValue(value, destination, options, objectsInPath, depth))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private bool SerializeWithFormatProvider(object value, StringBuilder destination, JsonSerializeOptions options, IFormattable formattable, string format, bool hasFormat)
        {
            int originalLength = destination.Length;

            try
            {
                TypeCode objTypeCode = Convert.GetTypeCode(value);
                bool includeQuotes = !SkipQuotes(objTypeCode);
                if (includeQuotes)
                {
                    destination.Append('"');
                }

                if (hasFormat)
                {
                    var formatProvider = options.FormatProvider ?? _defaultFormatProvider;
                    destination.AppendFormat(formatProvider, string.Concat("{0:", format, "}"), value);
                }
                else
                {
                    //format provider passed without FormatProvider
                    destination.Append(EscapeString(formattable.ToString("", options.FormatProvider), options.EscapeUnicode));
                }

                if (includeQuotes)
                {
                    destination.Append('"');
                }

                return true;
            }
            catch
            {
                destination.Length = originalLength;
                return false;
            }
        }

        private void SerializeDictionaryObject(IDictionary value, StringBuilder destination, JsonSerializeOptions options, SingleItemOptimizedHashSet<object> objectsInPath, int depth)
        {
            bool first = true;

            int originalLength;
            destination.Append('{');
            foreach (DictionaryEntry de in value)
            {
                originalLength = destination.Length;
                if (!first)
                {
                    destination.Append(',');
                }

                //only serialize, if key and value are serialized without error (e.g. due to reference loop)
                if (!SerializeObject(de.Key, destination, options, objectsInPath, depth + 1))
                {
                    destination.Length = originalLength;
                }
                else
                {
                    destination.Append(':');
                    if (!SerializeObject(de.Value, destination, options, objectsInPath, depth + 1))
                    {
                        destination.Length = originalLength;
                    }
                    else
                    {
                        first = false;
                    }
                }
            }
            destination.Append('}');
        }

        private void SerializeCollectionObject(IEnumerable value, StringBuilder destination, JsonSerializeOptions options, SingleItemOptimizedHashSet<object> objectsInPath, int depth)
        {
            bool first = true;

            int originalLength;
            destination.Append('[');
            foreach (var val in value)
            {
                originalLength = destination.Length;
                if (!first)
                {
                    destination.Append(',');
                }

                if (!SerializeObject(val, destination, options, objectsInPath, depth + 1))
                {
                    destination.Length = originalLength;
                }
                else
                {
                    first = false;
                }
            }
            destination.Append(']');
        }

        private bool SerializeTypeCodeValue(object value, StringBuilder destination, JsonSerializeOptions options, SingleItemOptimizedHashSet<object> objectsInPath, int depth)
        {
            TypeCode objTypeCode = Convert.GetTypeCode(value);
            if (objTypeCode == TypeCode.Object)
            {
                if (value is Guid || value is TimeSpan)
                {
                    //object without property, to string
                    QuoteValue(destination, Convert.ToString(value, CultureInfo.InvariantCulture));
                }
                else if (value is DateTimeOffset)
                {
                    QuoteValue(destination, string.Format("{0:yyyy-MM-dd HH:mm:ss zzz}", value));
                }
                else
                {
                    int originalLength = destination.Length;
                    try
                    {
                        using (new SingleItemOptimizedHashSet<object>.SingleItemScopedInsert(value, ref objectsInPath, false))
                        {
                            return SerializeProperties(value, destination, options, objectsInPath, depth);
                        }
                    }
                    catch
                    {
                        //nothing to add, so return is OK
                        destination.Length = originalLength;
                        return false;
                    }
                }
            }
            else
            {
                if (IsNumericTypeCode(objTypeCode, false))
                {
                    Enum enumValue;
                    if (!options.EnumAsInteger && (enumValue = value as Enum) != null)
                    {
                        QuoteValue(destination, EnumAsString(enumValue));
                    }
                    else
                    {
                        AppendIntegerAsString(destination, value, objTypeCode);
                    }
                }
                else
                {
                    string str = XmlHelper.XmlConvertToString(value, objTypeCode);
                    if (str == null)
                    {
                        return false;
                    }
                    if (SkipQuotes(objTypeCode))
                    {
                        destination.Append(str);
                    }
                    else
                    {
                        QuoteValue(destination, str);
                    }
                }
            }

            return true;
        }

        private static CultureInfo CreateFormatProvider()
        {
#if SILVERLIGHT
            var culture = new CultureInfo("en-US");
#else
            var culture = new CultureInfo("en-US", false);
#endif
            var numberFormat = culture.NumberFormat;
            numberFormat.NumberGroupSeparator = string.Empty;
            numberFormat.NumberDecimalSeparator = ".";
            numberFormat.NumberGroupSizes = new int[] { 0 };
            return culture;
        }

        private static string QuoteValue(string value)
        {
            return string.Concat("\"", value, "\"");
        }

        private static void QuoteValue(StringBuilder destination, string value)
        {
            destination.Append('"');
            destination.Append(value);
            destination.Append('"');
        }

        private static void AppendIntegerAsString(StringBuilder sb, object value, TypeCode objTypeCode)
        {
            switch (objTypeCode)
            {
                case TypeCode.Byte: sb.AppendInvariant((Byte)value); break;
                case TypeCode.SByte: sb.AppendInvariant((SByte)value); break;
                case TypeCode.Int16: sb.AppendInvariant((Int16)value); break;
                case TypeCode.Int32: sb.AppendInvariant((Int32)value); break;
                case TypeCode.Int64:
                    {
                        Int64 int64 = (Int64)value;
                        if (int64 < Int32.MaxValue && int64 > Int32.MinValue)
                            sb.AppendInvariant((Int32)int64);
                        else
                            sb.Append(int64);
                    } break;
                case TypeCode.UInt16: sb.AppendInvariant((UInt16)value); break;
                case TypeCode.UInt32: sb.AppendInvariant((UInt32)value); break;
                case TypeCode.UInt64:
                    {
                        UInt64 uint64 = (UInt64)value;
                        if (uint64 < UInt32.MaxValue)
                            sb.AppendInvariant((UInt32)uint64);
                        else
                            sb.Append(uint64);
                    } break;
                default:
                    sb.Append(XmlHelper.XmlConvertToString(value, objTypeCode));
                    break;
            }
        }

        private string EnumAsString(Enum value)
        {
            string textValue;
            if (!_enumCache.TryGetValue(value, out textValue))
            {
                textValue = Convert.ToString(value, CultureInfo.InvariantCulture);
                _enumCache.TryAddValue(value, textValue);
            }
            return textValue;
        }

        /// <summary>
        /// No quotes needed for this type?
        /// </summary>
        /// <param name="objTypeCode"></param>
        /// <returns></returns>
        private static bool SkipQuotes(TypeCode objTypeCode)
        {
            return objTypeCode != TypeCode.String && (objTypeCode == TypeCode.Empty  // Don't put quotes around null values
                || objTypeCode == TypeCode.Boolean
                || IsNumericTypeCode(objTypeCode, true));
        }

        /// <summary>
        /// Checks the object <see cref="TypeCode" /> if it is numeric
        /// </summary>
        /// <param name="objTypeCode">TypeCode for the object</param>
        /// <param name="includeDecimals">Accept fractional types as numeric type.</param>
        /// <returns></returns>
        private static bool IsNumericTypeCode(TypeCode objTypeCode, bool includeDecimals)
        {
            switch (objTypeCode)
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                    return true;
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Decimal:
                    return includeDecimals;
            }
            return false;
        }

        /// <summary>
        /// Checks input string if it needs JSON escaping, and makes necessary conversion
        /// </summary>
        /// <param name="text">Input string</param>
        /// <param name="escapeUnicode">Should non-ascii characters be encoded</param>
        /// <returns>JSON escaped string</returns>
        internal static string EscapeString(string text, bool escapeUnicode)
        {
            if (text == null)
                return null;

            StringBuilder sb = null;
            for (int i = 0; i < text.Length; ++i)
            {
                char ch = text[i];
                if (sb == null)
                {
                    // Check if we need to upgrade to StringBuilder
                    if (!EscapeChar(ch, escapeUnicode))
                    {
                        switch (ch)
                        {
                            case '"':
                            case '\\':
                            case '/':
                                break;

                            default:
                                continue; // StringBuilder not needed, yet
                        }
                    }

                    // StringBuilder needed
                    sb = new StringBuilder(text.Length + 4);
                    sb.Append(text, 0, i);
                }

                switch (ch)
                {
                    case '"':
                        sb.Append("\\\"");
                        break;

                    case '\\':
                        sb.Append("\\\\");
                        break;

                    case '/':
                        sb.Append("\\/");
                        break;

                    case '\b':
                        sb.Append("\\b");
                        break;

                    case '\r':
                        sb.Append("\\r");
                        break;

                    case '\n':
                        sb.Append("\\n");
                        break;

                    case '\f':
                        sb.Append("\\f");
                        break;

                    case '\t':
                        sb.Append("\\t");
                        break;

                    default:
                        if (EscapeChar(ch, escapeUnicode))
                        {
                            sb.AppendFormat(CultureInfo.InvariantCulture, "\\u{0:x4}", (int)ch);
                        }
                        else
                        {
                            sb.Append(ch);
                        }
                        break;
                }
            }

            if (sb != null)
                return sb.ToString();
            else
                return text;
        }

        private static bool EscapeChar(char ch, bool escapeUnicode)
        {
            if (ch < 32)
                return true;
            else
                return escapeUnicode && ch > 127;
        }

        private bool SerializeProperties(object value, StringBuilder destination, JsonSerializeOptions options,
            SingleItemOptimizedHashSet<object> objectsInPath, int depth)
        {
            var props = GetProps(value);
            if (props.Key.Length == 0)
            {
                try
                {
                    //no props
                    QuoteValue(destination, EscapeString(Convert.ToString(value, CultureInfo.InvariantCulture), options.EscapeUnicode));
                    return true;
                }
                catch
                {
                    return false;
                }
            }

            destination.Append('{');

            bool first = true;
            int originalLength = 0;
            bool useLateBoundMethods = props.Key.Length == props.Value.Length;

            for (var i = 0; i < props.Key.Length; i++)
            {
                originalLength = destination.Length;

                try
                {
                    var prop = props.Key[i];
                    var propValue = useLateBoundMethods ? props.Value[i](value, null) : prop.GetValue(value, null);
                    if (propValue != null)
                    {
                        if (!first)
                        {
                            destination.Append(", ");
                        }

                        if (options.QuoteKeys)
                        {
                            QuoteValue(destination, prop.Name);
                        }
                        else
                        {
                            destination.Append(prop.Name);
                        }
                        destination.Append(':');

                        if (!SerializeObject(propValue, destination, options, objectsInPath, depth + 1))
                        {
                            destination.Length = originalLength;
                        }
                        else
                        {
                            first = false;
                        }
                    }
                }
                catch
                {
                    //skip this property
                    destination.Length = originalLength;
                }
            }

            destination.Append('}');
            return true;
        }

        /// <summary>
        /// Get properties, cached for a type
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private KeyValuePair<PropertyInfo[], ReflectionHelpers.LateBoundMethod[]> GetProps(object value)
        {
            var type = value.GetType();
            KeyValuePair<PropertyInfo[],ReflectionHelpers.LateBoundMethod[]> props;
            if (_propsCache.TryGetValue(type, out props))
            {
                if (props.Key.Length != 0 && props.Value.Length == 0)
                {
                    var lateBoundMethods = new ReflectionHelpers.LateBoundMethod[props.Key.Length];
                    for(int i = 0; i < props.Key.Length; i++)
                    {
                        lateBoundMethods[i] = ReflectionHelpers.CreateLateBoundMethod(props.Key[i].GetGetMethod());
                    }
                    props = new KeyValuePair<PropertyInfo[], ReflectionHelpers.LateBoundMethod[]>(props.Key, lateBoundMethods);
                    _propsCache.TryAddValue(type, props);
                }
                return props;
            }

            PropertyInfo[] properties = null;

            try
            {
                properties = GetPropertyInfosNoCache(type);
                if (properties == null)
                {
                    properties = ArrayHelper.Empty<PropertyInfo>();
                }
            }
            catch (Exception ex)
            {
                properties = ArrayHelper.Empty<PropertyInfo>();
                NLog.Common.InternalLogger.Warn(ex, "Failed to get JSON properties for type: {0}", type);
            }

            props = new KeyValuePair<PropertyInfo[], ReflectionHelpers.LateBoundMethod[]>(properties, ArrayHelper.Empty<ReflectionHelpers.LateBoundMethod>());
            _propsCache.TryAddValue(type, props);
            return props;
        }

        private static PropertyInfo[] GetPropertyInfosNoCache(Type type)
        {
#if NETSTANDARD
            var props = type.GetRuntimeProperties().ToArray();
#else
            var props = type.GetProperties();
#endif
            return props;
        }


        #region Implementation of ISerializer

        /// <summary>Serialize an object</summary>
        /// <param name="sb">Add serialized value to this builder</param>
        /// <param name="value">Value to be serialized</param>
        /// <param name="formatProvider">format</param>
        public void SerializeObject(StringBuilder sb, object value, IFormatProvider formatProvider)
        {
            //todo inject options somewhere else
            SerializeObject(value, sb, new JsonSerializeOptions {FormatProvider = formatProvider,QuoteKeys = false});
        }

        #endregion
    }
}
