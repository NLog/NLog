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
using NLog.StructuredEvents;

namespace NLog.Targets
{
    /// <summary>
    /// Default class for serialization of values to JSON format.
    /// </summary>
    public class DefaultJsonSerializer : IJsonSerializer, NLog.StructuredEvents.Serialization.ISerializer
    {
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
            return SerializeObject(value, false, null, 0, null);
        }

        private string SerializeObject(object value, IFormatProvider format)
        {
            return SerializeObject(value, false, null, 0, format);
        }

        /// <summary>
        /// Returns a serialization of an object
        /// int JSON format.
        /// </summary>
        /// <param name="value">The object to serialize to JSON.</param>
        /// <param name="escapeUnicode">Should non-ascii characters be encoded</param>
        /// <param name="objectsInPath">The objects in path.</param>
        /// <param name="depth">The current depth (level) of recursion.</param>
        /// <param name="format">format</param>
        /// <returns>
        /// Serialized value.
        /// </returns>
        private string SerializeObject(object value, bool escapeUnicode, HashSet<object> objectsInPath, int depth, IFormatProvider format)
        {
            if (objectsInPath != null && objectsInPath.Contains(value))
            {
                return null; // detected reference loop, skip serialization
            }

            IEnumerable enumerable = null;
            IDictionary dict = null;
            IFormattable formattable = null;
            string str = null;
            if (value == null)
            {
                return "null";
            }
            else if ((str = value as string) != null)
            {
                return string.Concat("\"", JsonStringEscape(str, escapeUnicode), "\"");
            }
            else if ((dict = value as IDictionary) != null)
            {
                if (depth == MaxRecursionDepth) return null; // reached maximum recursion level, no further serialization

                var list = new List<string>();
                var set = new HashSet<object>(objectsInPath ?? (IEnumerable<object>)Internal.ArrayHelper.Empty<object>()) { value };
                foreach (DictionaryEntry de in dict)
                {
                    var keyJson = SerializeObject(de.Key, escapeUnicode, set, depth + 1, format);
                    var valueJson = SerializeObject(de.Value, escapeUnicode, set, depth + 1, format);
                    if (!string.IsNullOrEmpty(keyJson) && valueJson != null)
                    {
                        //only serialize, if key and value are serialized without error (e.g. due to reference loop)
                        list.Add(string.Concat(keyJson, ":", valueJson));
                    }
                }

                return string.Concat("{", string.Join(",", list.ToArray()), "}");
            }
            else if ((enumerable = value as IEnumerable) != null)
            {
                if (depth == MaxRecursionDepth) return null; // reached maximum recursion level, no further serialization

                var list = new List<string>();
                var set = new HashSet<object>(objectsInPath ?? (IEnumerable<object>)Internal.ArrayHelper.Empty<object>()) { value };
                foreach (var val in enumerable)
                {
                    var valueJson = SerializeObject(val, escapeUnicode, set, depth + 1, format);
                    if (valueJson != null)
                    {
                        list.Add(valueJson);
                    }
                }

                return string.Concat("[", string.Join(",", list.ToArray()), "]");
            }
            else if ((formattable = value as IFormattable) != null)
            {
                return formattable.ToString("{0}", format);
            }
            else
            {
                TypeCode objTypeCode = value != null ? Convert.GetTypeCode(value) : TypeCode.Empty;
                if (objTypeCode == TypeCode.Object)
                {
                    try
                    {
                        return new ObjectSerializer(this).SerializeObjectProperties(value, objectsInPath, depth, format);
                    }
                    catch
                    {
                        return null;
                    }
                }
                else
                {
                    bool encodeStringValue;
                    string escapeXmlString = JsonStringEncode(value, objTypeCode, escapeUnicode, out encodeStringValue);
                    if (escapeXmlString != null && encodeStringValue)
                        return string.Concat("\"", escapeXmlString, "\"");
                    else
                        return escapeXmlString;
                }
            }
        }

        /// <summary>
        /// Converts object value into JSON escaped string
        /// </summary>
        /// <param name="value">Object value</param>
        /// <param name="objTypeCode">Object TypeCode</param>
        /// <param name="escapeUnicode">Should non-ascii characters be encoded</param>
        /// <param name="encodeString">Should string be JSON encoded with quotes</param>
        /// <returns>Object value converted to JSON escaped string</returns>
        internal static string JsonStringEncode(object value, TypeCode objTypeCode, bool escapeUnicode, out bool encodeString)
        {
            string stringValue = Internal.XmlHelper.XmlConvertToString(value, objTypeCode);
            if (objTypeCode != TypeCode.String || stringValue == null)
            {
                encodeString = false;
                if (stringValue == null)
                    return stringValue;
                else if (objTypeCode == TypeCode.Empty)
                    return stringValue; // Don't put quotes around null values
                else if (objTypeCode == TypeCode.Boolean)
                    return stringValue; // Don't put quotes around boolean values
                else if (IsNumericTypeCode(objTypeCode))
                    return stringValue; // Don't put quotes around numeric values
            }

            encodeString = true;
            return JsonStringEscape(stringValue, escapeUnicode);
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
        internal static string JsonStringEscape(string text, bool escapeUnicode)
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
                                continue;   // StringBuilder not needed, yet
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

        #region Implementation of ISerializer

        /// <summary>
        /// Serialize object
        /// </summary>
        /// <param name="sb"></param>
        /// <param name="value"></param>
        /// <param name="formatProvider"></param>
        public void SerializeObject(StringBuilder sb, object value, IFormatProvider formatProvider)
        {
            //todo formatProvider
            var result = SerializeObject(value, formatProvider);
            sb.Append(result);
        }

      

        #endregion

        private class ObjectSerializer
        {
            /// <summary>
            /// Cache for property infos
            /// </summary>
            private static Dictionary<Type, PropertyInfo[]> PropsCache = new Dictionary<Type, PropertyInfo[]>();
            private DefaultJsonSerializer defaultJsonSerializer;

            public ObjectSerializer(DefaultJsonSerializer defaultJsonSerializer)
            {
                this.defaultJsonSerializer = defaultJsonSerializer;
            }

            internal string SerializeObjectProperties(object value, HashSet<object> objectsInPath, int depth, IFormatProvider format)
            {
                var stringBuilder = new StringBuilder();
                SerializeObjectProperties(stringBuilder, value, format, objectsInPath, depth);
                return stringBuilder.ToString();
            }

            public void SerializeObjectProperties(StringBuilder sb, object value, IFormatProvider format, HashSet<object> objectsInPath, int depth)
            {
                var props = GetProps(value);

                sb.Append('{');

                var isFirst = true;

                foreach (var prop in props)
                {
                    
                    
                    var propValue = prop.GetValue(value, null);
                    //todo nest objects, be warn of infinite loops.
                    // ValueRenderer.AppendValue(sb, propValue, false, null, formatProvider);

                    if (propValue != null)
                    {
                        if (!isFirst)
                        {
                            sb.Append(", ");
                        }
                        isFirst = false;
                        //todo escape name? (e.g. spaces)

                        //todo escape value? (e.g quotes)
                        sb.Append(prop.Name);
                        sb.Append(":");
                        //todo nasty references to each other
                        //todo objectsInPath
                        var serializedProperty = defaultJsonSerializer.SerializeObject(propValue, false, objectsInPath, depth++, format);

                        sb.Append(serializedProperty);
                    }
                }
                sb.Append('}');

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
                if (!PropsCache.TryGetValue(type, out props))
                {
#if NETSTANDARD
                props = type.GetRuntimeProperties().ToArray();
#else
                    props = type.GetProperties();
#endif
                    PropsCache[type] = props;
                }

                return props;
            }
        }
    }
}
