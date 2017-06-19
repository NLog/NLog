// 
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

namespace NLog.Targets
{
    /// <summary>
    /// Default class for serialization of values to JSON format.
    /// </summary>
    public class DefaultJsonSerializer : IJsonSerializer, IJsonSerializerV2
    {
        private readonly MruCache<Type, PropertyInfo[]> _propsCache = new MruCache<Type, PropertyInfo[]>(1000);
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
                    return SerializePrimitive(value, objTypeCode, options.EscapeUnicode, options.EnumAsInteger);
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
            return SerializeObject(value, destination, options, null, 0);
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
                HashSet<object> objectsInPath, int depth)
        {
            if (objectsInPath != null && objectsInPath.Contains(value))
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
                var set = AddToSet(objectsInPath, value);

                bool first = true;

                int originalLength = 0;
                destination.Append('{');
                foreach (DictionaryEntry de in dict)
                {
                    originalLength = destination.Length;
                    if (!first)
                    {
                        destination.Append(',');
                    }

                    //only serialize, if key and value are serialized without error (e.g. due to reference loop)
                    if (!SerializeObject(de.Key, destination, options, set, depth + 1))
                    {
                        destination.Length = originalLength;
                    }
                    else
                    {
                        destination.Append(':');
                        if (!SerializeObject(de.Value, destination, options, set, depth + 1))
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
            else if ((enumerable = value as IEnumerable) != null)
            {
                var set = AddToSet(objectsInPath, value);

                bool first = true;

                int originalLength = 0;
                destination.Append('[');
                foreach (var val in enumerable)
                {
                    originalLength = destination.Length;
                    if (!first)
                    {
                        destination.Append(',');
                    }

                    if (!SerializeObject(val, destination, options, set, depth + 1))
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
            else
            {
                IFormattable formattable;
                var format = options.Format;
                var hasFormat = !StringHelpers.IsNullOrWhiteSpace(format);
                if ((options.FormatProvider != null || hasFormat) && (formattable = value as IFormattable) != null)
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
                        destination.Append(formattable.ToString("", options.FormatProvider));
                    }

                    if (includeQuotes)
                    {
                        destination.Append('"');
                    }
                }
                else
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
                                var set = AddToSet(objectsInPath, value);
                                if (!SerializeProperties(value, destination, options, set, depth))
                                {
                                    destination.Length = originalLength;
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
                        destination.Append(SerializePrimitive(value, objTypeCode, options.EscapeUnicode, options.EnumAsInteger));
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

        /// <summary>
        /// Converts object value into JSON escaped string
        /// </summary>
        /// <param name="value">Object value</param>
        /// <param name="objTypeCode">Object TypeCode</param>
        /// <param name="escapeUnicode">Should non-ascii characters be encoded</param>
        /// <param name="enumAsInteger">Enum as integer value?</param>
        /// <returns>Object value converted to JSON escaped string</returns>
        internal static string SerializePrimitive(object value, TypeCode objTypeCode, bool escapeUnicode, bool enumAsInteger)
        {
            if (!enumAsInteger && IsNumericTypeCode(objTypeCode) && value.GetType().IsEnum)
            {
                //enum as string
                return QuoteValue(Convert.ToString(value, CultureInfo.InvariantCulture));
            }

            string stringValue = XmlHelper.XmlConvertToString(value, objTypeCode);

            if (stringValue == null)
            {
                return null;
            }

            if (SkipQuotes(objTypeCode))
            {
                return stringValue;
            }

            return QuoteValue(EscapeString(stringValue, escapeUnicode));
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
                || IsNumericTypeCode(objTypeCode));
        }

        private static bool IsNumericTypeCode(TypeCode objTypeCode)
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
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Decimal:
                    return true;
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
            HashSet<object> objectsInPath, int depth)
        {
            var props = GetProps(value);
            if (props.Length == 0)
            {
                //no props
                QuoteValue(destination, Convert.ToString(value, CultureInfo.InvariantCulture));
                return true;
            }

            destination.Append('{');

            bool first = true;
            int originalLength = 0;

            for (var i = 0; i < props.Length; i++)
            {
                var prop = props[i];

                originalLength = destination.Length;

                try
                {
                    var propValue = prop.GetValue(value, null);
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
                        destination.Append(":");

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
        private PropertyInfo[] GetProps(object value)
        {
            var type = value.GetType();
            PropertyInfo[] props;
            if (_propsCache.TryGetValue(type, out props))
            {
                return props;
            }

            try
            {
                props = GetPropertyInfosNoCache(type);
                if (props == null)
                {
                    props = ArrayHelper.Empty<PropertyInfo>();
                }
            }
            catch (Exception ex)
            {
                props = ArrayHelper.Empty<PropertyInfo>();
                NLog.Common.InternalLogger.Warn(ex, "Failed to get JSON properties for type: {0}", type);
            }

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

        private static HashSet<object> AddToSet(HashSet<object> objectsInPath, object value)
        {
            return new HashSet<object>(objectsInPath ?? (IEnumerable<object>)ArrayHelper.Empty<object>()) { value };
        }
    }
}
