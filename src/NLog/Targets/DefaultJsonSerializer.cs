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
using NLog.StructuredEvents;

namespace NLog.Targets
{
    /// <summary>
    /// Default class for serialization of values to JSON format.
    /// </summary>
    public class DefaultJsonSerializer : IJsonSerializer, NLog.StructuredEvents.Serialization.ISerializer
    {
        private const int MaxRecursionDepth = 10;

        private static HashSet<Type> NumericTypes = new HashSet<Type>
            {
                    typeof(int),
                    typeof(uint),
                    typeof(long),
                    typeof(ulong),
                    typeof(short),
                    typeof(ushort),
                    typeof(byte),
                    typeof(sbyte),
                    typeof(float),
                    typeof(double),
                    typeof(decimal),
            };

        private static Dictionary<char, string> CharacterMap = new Dictionary<char, string>()
        {
            { '"', "\\\"" },
            { '\\', "\\\\" },
            { '/', "\\/" },
            { '\b', "\\b" },
            { '\f', "\\f" },
            { '\n', "\\n" },
            { '\r', "\\r" },
            { '\t', "\\t" },
        };

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
            return SerializeObject(value, new HashSet<object>(), 0, null);
        }

        private string SerializeObject(object value, IFormatProvider format)
        {
            return SerializeObject(value, new HashSet<object>(), 0, format);
        }


        /// <summary>
        /// Returns a serialization of an object
        /// int JSON format.
        /// </summary>
        /// <param name="value">The object to serialize to JSON.</param>
        /// <param name="objectsInPath">The objects in path.</param>
        /// <param name="depth">The current depth (level) of recursion.</param>
        /// <param name="format">format</param>
        /// <returns>
        /// Serialized value.
        /// </returns>
        private string SerializeObject(object value, HashSet<object> objectsInPath, int depth, IFormatProvider format)
        {
            if (objectsInPath.Contains(value))
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
                return string.Format("\"{0}\"", EscapeString(str));
            }
            else if ((dict = value as IDictionary) != null)
            {
                if (depth == MaxRecursionDepth) return null; // reached maximum recursion level, no further serialization

                var list = new List<string>();
                var set = new HashSet<object>(objectsInPath) { value };
                foreach (DictionaryEntry de in dict)
                {
                    var keyJson = SerializeObject(de.Key, set, depth + 1, format);
                    var valueJson = SerializeObject(de.Value, set, depth + 1, format);
                    if (!string.IsNullOrEmpty(keyJson) && valueJson != null)
                    {
                        //only serialize, if key and value are serialized without error (e.g. due to reference loop)
                        list.Add(string.Format("{0}:{1}", keyJson, valueJson));
                    }
                }

                return string.Format("{{{0}}}", string.Join(",", list.ToArray()));
            }
            else if ((enumerable = value as IEnumerable) != null)
            {
                if (depth == MaxRecursionDepth) return null; // reached maximum recursion level, no further serialization

                var list = new List<string>();
                var set = new HashSet<object>(objectsInPath);
                set.Add(value);
                foreach (var val in enumerable)
                {
                    var valueJson = SerializeObject(val, set, depth + 1, format);
                    if (valueJson != null)
                    {
                        list.Add(valueJson);
                    }
                }

                return string.Format("[{0}]", string.Join(",", list.ToArray()));
            }
            else
            {
                var type = value.GetType();
                if (NumericTypes.Contains(type))
                {
#if SILVERLIGHT
                var culture = new CultureInfo("en-US").NumberFormat;
#else
                    var culture = new CultureInfo("en-US", false).NumberFormat;
#endif
                    culture.NumberGroupSeparator = string.Empty;
                    culture.NumberDecimalSeparator = ".";
                    culture.NumberGroupSizes = new int[] { 0 };
                    return string.Format(culture, "{0}", value);
                }
                if (type == typeof(bool))
                {
                    return value.ToString();
                }
                if (type == typeof(char))
                {
                    return "'" + value.ToString() + "'";
                }
                else if ((formattable = value as IFormattable) != null)
                {
                    if (depth == MaxRecursionDepth) return null; // reached maximum recursion level, no further serialization

                    return formattable.ToString("{0}", format);
                }
                else
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
            }
        }

        private static string EscapeString(string str)
        {
            var sb = new StringBuilder(str.Length);
            foreach (var c in str)
            {
                sb.Append(EscapeChar(c));
            }

            return sb.ToString();
        }

        private static string EscapeChar(char c)
        {
            string mapped;
            if (CharacterMap.TryGetValue(c, out mapped))
            {
                return mapped;
            }
            else if (c <= 0x1f)
            {
                return string.Format("\\u{0:x4}", (int)c);
            }
            else
            {
                return c.ToString();
            }
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
                    if (!isFirst)
                    {
                        sb.Append(", ");
                    }
                    isFirst = false;
                    //todo escape name? (e.g. spaces)
                    sb.Append(prop.Name);
                    sb.Append(":");
                    //todo escape value? (e.g quotes)
                    var propValue = prop.GetValue(value, null);
                    //todo nest objects, be warn of infinite loops.
                    // ValueRenderer.AppendValue(sb, propValue, false, null, formatProvider);

                    //todo nasty references to each other
                    //todo objectsInPath
                    var serializedProperty = defaultJsonSerializer.SerializeObject(propValue, objectsInPath, depth++, format);

                    sb.Append(serializedProperty);
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
