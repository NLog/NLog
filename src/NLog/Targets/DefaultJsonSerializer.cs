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

namespace NLog.Targets
{
    /// <summary>
    /// Default class for serialization of values to JSON format.
    /// </summary>
    public class DefaultJsonSerializer : IJsonSerializer
    {
        private static List<Type> NumericTypes = new List<Type>
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

            IEnumerable enumerable = null;
            IDictionary dict = null;
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
                var l = new List<string>();
                foreach(DictionaryEntry de in dict)
                {
                    l.Add(string.Format("\"{0}\":{1}", SerializeObject(de.Key), SerializeObject(de.Value)));
                }

                return string.Format("{{{0}}}", string.Join(",", l.ToArray()));
            }
            else if((enumerable = value as IEnumerable) != null)
            {
                var l = new List<string>();
                foreach (var val in enumerable)
                {
                    l.Add(SerializeObject(val));
                }

                return string.Format("[{0}]", string.Join(",", l.ToArray()));
            }
            else if (NumericTypes.Contains(value.GetType()))
            {
#if SILVERLIGHT
                var nf = new CultureInfo("en-US").NumberFormat;
#else
                var nf = new CultureInfo("en-US", false).NumberFormat;
#endif
                nf.NumberGroupSeparator = string.Empty;
                nf.NumberDecimalSeparator = ".";
                nf.NumberGroupSizes = new int[] { 0 };
                return string.Format(nf, "{0}", value);
            }
            else
            {
                try
                {
                    return value.ToString();
                }
                catch 
                {
                    return null;
                }
             }
        }

        private static string EscapeString(string str)
        {
            return str.Replace("\\", "\\\\")
                .Replace("\"", "\\\"")
                .Replace("/", "\\/")
                .Replace("\b", "\\b")
                .Replace("\f", "\\f")
                .Replace("\n", "\\n")
                .Replace("\r", "\\r")
                .Replace("\t", "\\t");
        }
    }
}
