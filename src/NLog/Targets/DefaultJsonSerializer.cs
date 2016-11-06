using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

namespace NLog.Targets
{
    /// <summary>
    /// Default class for serialization of values to JSON format.
    /// </summary>
    public class DefaultJsonSerializer : ICompactJsonSerializer
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

        private static readonly DefaultJsonSerializer _instance;

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

        private DefaultJsonSerializer()
        { }

        /// <summary>
        /// Returns a serialization of an object
        /// int compact JSON format.
        /// </summary>
        /// <param name="value">The object to serialize to compact JSON.</param>
        /// <returns>Serialized value.</returns>
        public string SerializeValue(object value)
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
                return string.Format("\"{0}\"", escapeString(str));
            }
            else if ((dict = value as IDictionary) != null)
            {
                var l = new List<string>();
                foreach(DictionaryEntry de in dict)
                {
                    l.Add(string.Format("\"{0}\":{1}", SerializeValue(de.Key), SerializeValue(de.Value)));
                }

                return string.Format("{{{0}}}", string.Join(",", l.ToArray()));
            }
            else if((enumerable = value as IEnumerable) != null)
            {
                var l = new List<string>();
                foreach (var val in enumerable)
                {
                    l.Add(SerializeValue(val));
                }

                return string.Format("[{0}]", string.Join(",", l));
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

        private static string escapeString(string str)
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
