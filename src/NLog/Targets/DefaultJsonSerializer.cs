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
using System.Globalization;
using System.Text;
using NLog.Internal;

namespace NLog.Targets
{
    /// <summary>
    /// Default class for serialization of values to JSON format.
    /// </summary>
#pragma warning disable 618
    public class DefaultJsonSerializer : IJsonConverter, IJsonSerializer
#pragma warning restore 618
    {
        private readonly ObjectReflectionCache _objectReflectionCache = new ObjectReflectionCache();
        private readonly MruCache<Enum, string> _enumCache = new MruCache<Enum, string>(1500);
        private readonly JsonSerializeOptions _serializeOptions = new JsonSerializeOptions();
        private readonly JsonSerializeOptions _exceptionSerializeOptions = new JsonSerializeOptions() { SanitizeDictionaryKeys = true };
        private readonly IFormatProvider _defaultFormatProvider = CreateFormatProvider();

        private const int MaxJsonLength = 512 * 1024;

        private static readonly DefaultJsonSerializer instance = new DefaultJsonSerializer();

        private static readonly IEqualityComparer<object> _referenceEqualsComparer = SingleItemOptimizedHashSet<object>.ReferenceEqualityComparer.Default;

        /// <summary>
        /// Singleton instance of the serializer.
        /// </summary>
        public static DefaultJsonSerializer Instance => instance;

        static DefaultJsonSerializer()
        {
        }

        /// <summary>
        /// Private. Use <see cref="Instance"/>
        /// </summary>
        private DefaultJsonSerializer()
        { }

        /// <summary>
        /// Returns a serialization of an object into JSON format.
        /// </summary>
        /// <param name="value">The object to serialize to JSON.</param>
        /// <returns>Serialized value.</returns>
        public string SerializeObject(object value)
        {
            return SerializeObject(value, _serializeOptions);
        }

        /// <summary>
        /// Returns a serialization of an object into JSON format.
        /// </summary>
        /// <param name="value">The object to serialize to JSON.</param>
        /// <param name="options">serialisation options</param>
        /// <returns>Serialized value.</returns>
        public string SerializeObject(object value, JsonSerializeOptions options)
        {
            if (value == null)
            {
                return "null";
            }
            else if (value is string str)
            {
                for (int i = 0; i < str.Length; ++i)
                {
                    if (RequiresJsonEscape(str[i], options.EscapeUnicode))
                    {
                        StringBuilder sb = new StringBuilder(str.Length + 4);
                        sb.Append('"');
                        AppendStringEscape(sb, str, options.EscapeUnicode);
                        sb.Append('"');
                        return sb.ToString();
                    }
                }
                return QuoteValue(str);
            }
            else
            {
                TypeCode objTypeCode = Convert.GetTypeCode(value);
                if (objTypeCode != TypeCode.Object && objTypeCode != TypeCode.Char && StringHelpers.IsNullOrWhiteSpace(options.Format) && options.FormatProvider == null)
                {
                    Enum enumValue;
                    if (!options.EnumAsInteger && IsNumericTypeCode(objTypeCode, false) && (enumValue = value as Enum) != null)
                    {
                        return QuoteValue(EnumAsString(enumValue));
                    }
                    else
                    {
                        string xmlStr = XmlHelper.XmlConvertToString(value, objTypeCode);
                        if (SkipQuotes(value, objTypeCode))
                        {
                            return xmlStr;
                        }
                        else
                        {
                            return QuoteValue(xmlStr);
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
        /// Serialization of the object in JSON format to the destination StringBuilder
        /// </summary>
        /// <param name="value">The object to serialize to JSON.</param>
        /// <param name="destination">Write the resulting JSON to this destination.</param>
        /// <returns>Object serialized succesfully (true/false).</returns>
        public bool SerializeObject(object value, StringBuilder destination)
        {
            return SerializeObject(value, destination, _serializeOptions);
        }

        /// <summary>
        /// Serialization of the object in JSON format to the destination StringBuilder
        /// </summary>
        /// <param name="value">The object to serialize to JSON.</param>
        /// <param name="destination">Write the resulting JSON to this destination.</param>
        /// <param name="options">serialisation options</param>
        /// <returns>Object serialized succesfully (true/false).</returns>
        public bool SerializeObject(object value, StringBuilder destination, JsonSerializeOptions options)
        {
            return SerializeObject(value, Convert.GetTypeCode(value), destination, options, default(SingleItemOptimizedHashSet<object>), 0);
        }

        /// <summary>
        /// Serialization of the object in JSON format to the destination StringBuilder
        /// </summary>
        /// <param name="value">The object to serialize to JSON.</param>
        /// <param name="objTypeCode">The TypeCode for the object to serialize.</param>
        /// <param name="destination">Write the resulting JSON to this destination.</param>
        /// <param name="options">serialisation options</param>
        /// <param name="objectsInPath">The objects in path (Avoid cyclic reference loop).</param>
        /// <param name="depth">The current depth (level) of recursion.</param>
        /// <returns>Object serialized succesfully (true/false).</returns>
        private bool SerializeObject(object value, TypeCode objTypeCode, StringBuilder destination, JsonSerializeOptions options,
                SingleItemOptimizedHashSet<object> objectsInPath, int depth)
        {
            if (objTypeCode == TypeCode.Object && objectsInPath.Contains(value))
            {
                return false; // detected reference loop, skip serialization
            }

            if (value == null)
            {
                destination.Append("null");
            }
            else if (objTypeCode == TypeCode.String)
            {
                destination.Append('"');
                AppendStringEscape(destination, value.ToString(), options.EscapeUnicode);
                destination.Append('"');
            }
            else if (objTypeCode != TypeCode.Object)
            {
                return SerializeWithTypeCode(value, objTypeCode, destination, options, ref objectsInPath, depth);
            }
            else if (value is IDictionary dict)
            {
                using (StartCollectionScope(ref objectsInPath, dict))
                {
                    SerializeDictionaryObject(dict, destination, options, objectsInPath, depth);
                }
            }
            else if (value is IDictionary<string, object> expando)
            {
                // Special case for Expando-objects
                using (StartCollectionScope(ref objectsInPath, expando))
                {
                    return SerializeObjectProperties(new ObjectReflectionCache.ObjectPropertyList(expando), destination, options, objectsInPath, depth);
                }
            }
            else if (value is IEnumerable enumerable)
            {
                using (StartCollectionScope(ref objectsInPath, value))
                {
                    SerializeCollectionObject(enumerable, destination, options, objectsInPath, depth);
                }
            }
            else
            {
                return SerializeWithTypeCode(value, objTypeCode, destination, options, ref objectsInPath, depth);
            }
            return true;
        }

        private bool SerializeWithTypeCode(object value, TypeCode objTypeCode, StringBuilder destination, JsonSerializeOptions options, ref SingleItemOptimizedHashSet<object> objectsInPath, int depth)
        {
            var hasFormat = !StringHelpers.IsNullOrWhiteSpace(options.Format);
            if ((options.FormatProvider != null || hasFormat) && (value is IFormattable formattable))
            {
                return SerializeWithFormatProvider(value, objTypeCode, destination, options, formattable, options.Format, hasFormat);
            }
            else
            {
                if (objTypeCode == TypeCode.Object)
                {
                    if (value is DateTimeOffset)
                    {
                        QuoteValue(destination, $"{value:yyyy-MM-dd HH:mm:ss zzz}");
                        return true;
                    }
                    else
                    {
                        return SerializeObjectWithProperties(value, destination, options, ref objectsInPath, depth);
                    }
                }
                else
                {
                    return SerializeSimpleTypeCodeValue(value, objTypeCode, destination, options);
                }
            }
        }

        private static SingleItemOptimizedHashSet<object>.SingleItemScopedInsert StartCollectionScope(ref SingleItemOptimizedHashSet<object> objectsInPath, object value)
        {
            return new SingleItemOptimizedHashSet<object>.SingleItemScopedInsert(value, ref objectsInPath, true, _referenceEqualsComparer);
        }

        private bool SerializeWithFormatProvider(object value, TypeCode objTypeCode, StringBuilder destination, JsonSerializeOptions options, IFormattable formattable, string format, bool hasFormat)
        {
            int originalLength = destination.Length;

            try
            {
                bool includeQuotes = !SkipQuotes(value, objTypeCode);
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
                    var str = formattable.ToString("", options.FormatProvider);
                    AppendStringEscape(destination, str, options.EscapeUnicode);
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

            int nextDepth = objectsInPath.Count <= 1 ? depth : (depth + 1);
            if (nextDepth > options.MaxRecursionLimit)
            {
                destination.Append("{}");
                return;
            }

            int originalLength;
            destination.Append('{');
            foreach (DictionaryEntry de in value)
            {
                originalLength = destination.Length;
                if (originalLength > MaxJsonLength)
                {
                    break;
                }

                if (!first)
                {
                    destination.Append(',');
                }

                var itemKey = de.Key;
                var itemKeyTypeCode = Convert.GetTypeCode(itemKey);
                if (options.QuoteKeys)
                {
                    if (!SerializeObjectAsString(itemKey, itemKeyTypeCode, destination, options))
                    {
                        destination.Length = originalLength;
                        continue;
                    }
                }
                else
                {
                    if (!SerializeObject(itemKey, itemKeyTypeCode, destination, options, objectsInPath, nextDepth))
                    {
                        destination.Length = originalLength;
                        continue;
                    }
                }

                if (options.SanitizeDictionaryKeys)
                {
                    int quoteSkipCount = options.QuoteKeys ? 1 : 0;
                    int keyEndIndex = destination.Length - quoteSkipCount;
                    int keyStartIndex = originalLength + (first ? 0 : 1) + quoteSkipCount;
                    if (!SanitizeDictionaryKey(destination, keyStartIndex, keyEndIndex - keyStartIndex))
                    {
                        destination.Length = originalLength;    // Empty keys are not allowed
                        continue;
                    }
                }

                destination.Append(':');

                //only serialize, if key and value are serialized without error (e.g. due to reference loop)
                var itemValue = de.Value;
                var itemValueTypeCode = Convert.GetTypeCode(itemValue);
                if (!SerializeObject(itemValue, itemValueTypeCode, destination, options, objectsInPath, nextDepth))
                {
                    destination.Length = originalLength;
                }
                else
                {
                    first = false;
                }
            }
            destination.Append('}');
        }

        private static bool SanitizeDictionaryKey(StringBuilder destination, int keyStartIndex, int keyLength)
        {
            if (keyLength == 0)
            {
                return false;   // Empty keys are not allowed
            }

            int keyEndIndex = keyStartIndex + keyLength;
            for (int i = keyStartIndex; i < keyEndIndex; ++i)
            {
                char keyChar = destination[i];
                if (keyChar == '_' || char.IsLetterOrDigit(keyChar))
                    continue;

                destination[i] = '_';
            }
            return true;
        }

        private void SerializeCollectionObject(IEnumerable value, StringBuilder destination, JsonSerializeOptions options, SingleItemOptimizedHashSet<object> objectsInPath, int depth)
        {
            bool first = true;

            int nextDepth = objectsInPath.Count <= 1 ? depth : (depth + 1); // Allow serialization of list-items 
            if (nextDepth > options.MaxRecursionLimit)
            {
                destination.Append("[]");
                return;
            }

            int originalLength;
            destination.Append('[');
            foreach (var val in value)
            {
                originalLength = destination.Length;
                if (originalLength > MaxJsonLength)
                {
                    break;
                }

                if (!first)
                {
                    destination.Append(',');
                }

                if (!SerializeObject(val, Convert.GetTypeCode(val), destination, options, objectsInPath, nextDepth))
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

        private bool SerializeObjectWithProperties(object value, StringBuilder destination, JsonSerializeOptions options, ref SingleItemOptimizedHashSet<object> objectsInPath, int depth)
        {
            int originalLength = destination.Length;
            if (originalLength > MaxJsonLength)
            {
                return false;
            }

            if (depth < options.MaxRecursionLimit)
            {
                try
                {
                    if (ReferenceEquals(options, instance._serializeOptions) && value is Exception)
                    {
                        // Exceptions are seldom under control, and can include random Data-Dictionary-keys, so we sanitize by default
                        options = instance._exceptionSerializeOptions;
                    }

                    var objectPropertyList = _objectReflectionCache.LookupObjectProperties(value);
                    using (new SingleItemOptimizedHashSet<object>.SingleItemScopedInsert(value, ref objectsInPath, false, _referenceEqualsComparer))
                    {
                        return SerializeObjectProperties(objectPropertyList, destination, options, objectsInPath, depth);
                    }
                }
                catch
                {
                    //nothing to add, so return is OK
                    destination.Length = originalLength;
                    return false;
                }
            }
            else
            {
                return SerializeObjectAsString(value, TypeCode.Object, destination, options);
            }
        }

        private bool SerializeSimpleTypeCodeValue(object value, TypeCode objTypeCode, StringBuilder destination, JsonSerializeOptions options, bool forceQuotes = false)
        {
            if (objTypeCode == TypeCode.String || objTypeCode == TypeCode.Char)
            {
                destination.Append('"');
                AppendStringEscape(destination, value.ToString(), options.EscapeUnicode);
                destination.Append('"');
            }
            else if (IsNumericTypeCode(objTypeCode, false))
            {
                if (!options.EnumAsInteger && value is Enum enumValue)
                {
                    QuoteValue(destination, EnumAsString(enumValue));
                }
                else
                {
                    if (forceQuotes)
                        destination.Append('"');
                    destination.AppendIntegerAsString(value, objTypeCode);
                    if (forceQuotes)
                        destination.Append('"');
                }
            }
            else
            {
                string str = XmlHelper.XmlConvertToString(value, objTypeCode);
                if (str == null)
                {
                    return false;
                }

                if (!forceQuotes && SkipQuotes(value, objTypeCode))
                {
                    destination.Append(str);
                }
                else
                {
                    QuoteValue(destination, str);
                }
            }
            return true;
        }

        private static CultureInfo CreateFormatProvider()
        {
#if SILVERLIGHT || NETSTANDARD1_0
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
        private static bool SkipQuotes(object value, TypeCode objTypeCode)
        {
            switch (objTypeCode)
            {
                case TypeCode.String: return false;
                case TypeCode.Char: return false;
                case TypeCode.DateTime: return false;
                case TypeCode.Empty: return true;
                case TypeCode.Boolean: return true;
                case TypeCode.Decimal: return true;
                case TypeCode.Double:
                    {
                        double dblValue = (double)value;
                        return !double.IsNaN(dblValue) && !double.IsInfinity(dblValue);
                    }
                case TypeCode.Single:
                    {
                        float floatValue = (float)value;
                        return !float.IsNaN(floatValue) && !float.IsInfinity(floatValue);
                    }
                default:
                    return IsNumericTypeCode(objTypeCode, false);
            }
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
        /// <param name="destination">Destination Builder</param>
        /// <param name="text">Input string</param>
        /// <param name="escapeUnicode">Should non-ascii characters be encoded</param>
        /// <returns>JSON escaped string</returns>
        internal static void AppendStringEscape(StringBuilder destination, string text, bool escapeUnicode)
        {
            if (string.IsNullOrEmpty(text))
                return;

            StringBuilder sb = null;

            for (int i = 0; i < text.Length; ++i)
            {
                char ch = text[i];
                if (!RequiresJsonEscape(ch, escapeUnicode))
                {
                    if (sb != null)
                        sb.Append(ch);
                    continue;
                }
                else if (sb == null)
                {
                    sb = destination;
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

            if (sb == null)
                destination.Append(text);   // Faster to make single Append
        }

        internal static bool RequiresJsonEscape(char ch, bool escapeUnicode)
        {
            if (!EscapeChar(ch, escapeUnicode))
            {
                switch (ch)
                {
                    case '"':
                    case '\\':
                    case '/':
                        return true;
                    default:
                        return false;
                }
            }
            return true;
        }

        private static bool EscapeChar(char ch, bool escapeUnicode)
        {
            if (ch < 32)
                return true;
            else
                return escapeUnicode && ch > 127;
        }

        private bool SerializeObjectProperties(ObjectReflectionCache.ObjectPropertyList objectPropertyList,StringBuilder destination, JsonSerializeOptions options,
            SingleItemOptimizedHashSet<object> objectsInPath, int depth)
        {
            if (objectPropertyList.Count == 0)
            { 
                //no props
                return SerializeObjectAsString(objectPropertyList.ToString(), TypeCode.Object, destination, options);
            }

            destination.Append('{');

            bool first = true;
            foreach (var propertyValue in objectPropertyList)
            {
                var originalLength = destination.Length;

                try
                {
                    if (propertyValue.Name != null && propertyValue.Value != null)
                    {
                        if (!first)
                        {
                            destination.Append(", ");
                        }

                        if (options.QuoteKeys)
                        {
                            QuoteValue(destination, propertyValue.Name);
                        }
                        else
                        {
                            destination.Append(propertyValue.Name);
                        }
                        destination.Append(':');

                        if (!SerializeObject(propertyValue.Value, propertyValue.TypeCode, destination, options, objectsInPath, depth + 1))
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

        private bool SerializeObjectAsString(object value, TypeCode objTypeCode, StringBuilder destination, JsonSerializeOptions options)
        {
            try
            {
                if (objTypeCode == TypeCode.Object)
                {
                    var str = Convert.ToString(value, CultureInfo.InvariantCulture);
                    destination.Append('"');
                    AppendStringEscape(destination, str, options.EscapeUnicode);
                    destination.Append('"');
                    return true;
                }
                else
                {
                    return SerializeSimpleTypeCodeValue(value, objTypeCode, destination, options, true);
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
