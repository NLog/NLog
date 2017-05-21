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
    public class DefaultJsonSerializer : IJsonSerializer
    {
        private const int MaxRecursionDepth = 10;

        private static readonly DefaultJsonSerializer _instance;

        /// <summary>
        /// Cache for property infos
        /// </summary>
        private static Dictionary<Type, PropertyInfo[]> _propsCache = new Dictionary<Type, PropertyInfo[]>();

        /// <summary>
        /// Singleton instance of the serializer.
        /// </summary>
        public static DefaultJsonSerializer Instance
        {
            get { return _instance; }
        }

        static DefaultJsonSerializer()
        {
            _instance = new DefaultJsonSerializer();
        }


        /// <summary>
        /// Private ctor so we use <see cref="Instance"/>
        /// </summary>
        private DefaultJsonSerializer()
        {
        }

        /// <summary>
        /// Returns a serialization of an object
        /// int JSON format.
        /// </summary>
        /// <param name="value">The object to serialize to JSON.</param>
        /// <returns>Serialized value.</returns>
        public string SerializeObject(object value)
        {
            return SerializeObject(value, new JsonSerializeOptions());
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
            return SerializeObject(value, options, null, 0);
        }

        /// <summary>
        /// Returns a serialization of an object
        /// int JSON format.
        /// </summary>
        /// <param name="value">The object to serialize to JSON.</param>
        /// <param name="options">serialisation options</param>
        /// <param name="objectsInPath">The objects in path.</param>
        /// <param name="depth">The current depth (level) of recursion.</param>
        /// <returns>
        /// Serialized value.
        /// </returns>
        private string SerializeObject(object value, JsonSerializeOptions options,
            HashSet<object> objectsInPath, int depth)
        {
            if (objectsInPath != null && objectsInPath.Contains(value))
            {
                return null; // detected reference loop, skip serialization
            }
            if (depth > MaxRecursionDepth)
            {
                return null; // reached maximum recursion level, no further serialization
            }

            IEnumerable enumerable;
            IDictionary dict;
            string str;
            string returnValue;

            if (value == null)
            {
                returnValue = "null";
            }
            else if ((str = value as string) != null)
            {
                returnValue = QuoteValue(EscapeString(str, options.EscapeUnicode));
            }
            else if ((dict = value as IDictionary) != null)
            {
                List<string> list = null;

                var set = AddToSet(objectsInPath, value);
                foreach (DictionaryEntry de in dict)
                {
                    var keyJson = SerializeObject(de.Key, options, set, depth + 1);
                    if (!string.IsNullOrEmpty(keyJson))
                    {

                        var valueJson = SerializeObject(de.Value, options, set, depth + 1);
                        if (valueJson != null)
                        {
                            list = list ?? new List<string>();
                            //only serialize, if key and value are serialized without error (e.g. due to reference loop)
                            list.Add(string.Concat(keyJson, ":", valueJson));
                        }
                    }
                }

                if (list != null)
                {
                    returnValue = string.Concat("{", string.Join(",", list.ToArray()), "}");
                }
                else
                {
                    returnValue = "{}";
                }
            }
            else if ((enumerable = value as IEnumerable) != null)
            {
                List<string> list = null;
                var set = AddToSet(objectsInPath, value);
                foreach (var val in enumerable)
                {
                    var valueJson = SerializeObject(val, options, set, depth + 1);
                    if (valueJson != null)
                    {
                        list = list ?? new List<string>();
                        list.Add(valueJson);
                    }
                }

                if (list != null)
                {
                    returnValue = string.Concat("[", string.Join(",", list.ToArray()), "]");
                }
                else
                {
                    returnValue = "[]";
                }
            }

            else
            {
                IFormattable formattable;
                var format = options.Format;
                var hasFormat = !StringHelpers.IsNullOrWhiteSpace(format);
                if ((options.FormatProvider != null || hasFormat) && (formattable = value as IFormattable) != null)
                {
                    TypeCode objTypeCode = Convert.GetTypeCode(value);

                    if (hasFormat)
                    {
                        if (options.FormatProvider == null)
                        {
                            var culture = CreateFormatProvider();

                            options.FormatProvider = culture;
                        }
                        returnValue = string.Format(options.FormatProvider, "{0:" + format + "}", value);

                    }
                    else
                    {
                        //format provider passed without FormatProvider
                        returnValue = formattable.ToString("", options.FormatProvider);
                    }
                    if (!SkipQuotes(objTypeCode))
                    {
                        returnValue = QuoteValue(returnValue);
                    }

                }
                else
                {
                    TypeCode objTypeCode = Convert.GetTypeCode(value);
                    if (objTypeCode == TypeCode.Object)
                    {
                        if (value is Guid || value is TimeSpan || value is DateTimeOffset)
                        {
                            //object without property, to string
                            return QuoteValue(Convert.ToString(value, CultureInfo.InvariantCulture));
                        }
                        try
                        {
                            var set = AddToSet(objectsInPath, value);
                            returnValue = SerializeProperties(value, options, set, depth);

                        }
                        catch
                        {
                            //nothing to add, so return is OK
                            return null;
                        }
                    }
                    else
                    {
                        returnValue = SerializePrimitive(value, objTypeCode, options.EscapeUnicode, options.EnumAsString);
                    }
                }
            }

            return returnValue;
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

        /// <summary>
        /// Converts object value into JSON escaped string
        /// </summary>
        /// <param name="value">Object value</param>
        /// <param name="objTypeCode">Object TypeCode</param>
        /// <param name="escapeUnicode">Should non-ascii characters be encoded</param>
        /// <param name="enumAsString">Enum as string value?</param>
        /// <returns>Object value converted to JSON escaped string</returns>
        internal static string SerializePrimitive(object value, TypeCode objTypeCode, bool escapeUnicode, bool enumAsString)
        {

            if (enumAsString && IsNumericTypeCode(objTypeCode) && value.GetType().IsEnum)
            {
                //enum as string
                return QuoteValue(Convert.ToString(value, CultureInfo.InvariantCulture));
            }

            string stringValue = Internal.XmlHelper.XmlConvertToString(value, objTypeCode);

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

        private string SerializeProperties(object value, JsonSerializeOptions options,
            HashSet<object> objectsInPath, int depth)
        {

            var props = GetProps(value);

            if (props.Length == 0)
            {
                //no props
                return QuoteValue(Convert.ToString(value, CultureInfo.InvariantCulture));
            }
            var sb = new StringBuilder();
            sb.Append('{');

            var isFirst = true;

            for (var i = 0; i < props.Length; i++)
            {
                var prop = props[i];

                try
                {
                    var propValue = prop.GetValue(value, null);
                    if (propValue != null)
                    {

                        var serializedProperty = SerializeObject(propValue, options, objectsInPath, depth + 1);

                        if (serializedProperty != null)
                        {
                            if (!isFirst)
                            {
                                sb.Append(", ");
                            }
                            isFirst = false;
                            if (options.QuoteKeys)
                            {
                                sb.Append("\"");
                                //no escape needed as properties don't have quotes
                                sb.Append(prop.Name);
                                sb.Append("\"");
                            }
                            else
                            {
                                sb.Append(prop.Name);
                            }

                            sb.Append(":");
                            sb.Append(serializedProperty);
                        }
                    }
                }
                catch
                {
                    //skip this property
                }

            }
            sb.Append('}');
            return sb.ToString();
        }

        /// <summary>
        /// Get properties, cached for a type
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private static PropertyInfo[] GetProps(object value)
        {
            var type = value.GetType();
            PropertyInfo[] props;
            if (!_propsCache.TryGetValue(type, out props))
            {
#if NETSTANDARD
                props = type.GetRuntimeProperties().ToArray();
#else
                props = type.GetProperties();
#endif
                _propsCache[type] = props;
            }

            return props;
        }

        private static HashSet<object> AddToSet(HashSet<object> objectsInPath, object value)
        {
            return new HashSet<object>(objectsInPath ?? (IEnumerable<object>)Internal.ArrayHelper.Empty<object>()) { value };
        }

    }

}