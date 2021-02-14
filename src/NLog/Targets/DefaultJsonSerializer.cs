// 
// Copyright (c) 2004-2020 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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
    public class DefaultJsonSerializer : IJsonConverter
    {
        private readonly ObjectReflectionCache _objectReflectionCache;
        private readonly MruCache<Enum, string> _enumCache = new MruCache<Enum, string>(2000);
        private readonly IFormatProvider _defaultFormatProvider = CreateFormatProvider();

        private const int MaxJsonLength = 512 * 1024;

        private static readonly IEqualityComparer<object> _referenceEqualsComparer = SingleItemOptimizedHashSet<object>.ReferenceEqualityComparer.Default;

        private static JsonSerializeOptions DefaultSerializerOptions = new JsonSerializeOptions();
        private static JsonSerializeOptions DefaultExceptionSerializerOptions = new JsonSerializeOptions() { SanitizeDictionaryKeys = true };

        /// <summary>
        /// Singleton instance of the serializer.
        /// </summary>
        [Obsolete("Instead use LogFactory.ServiceRepository.ResolveInstance(typeof(IJsonConverter)). Marked obsolete on NLog 5.0")]
        public static DefaultJsonSerializer Instance { get; } = new DefaultJsonSerializer(null);

        /// <summary>
        /// Private. Use <see cref="Instance"/>
        /// </summary>
        internal DefaultJsonSerializer(IServiceProvider serviceProvider)
        {
            _objectReflectionCache = new ObjectReflectionCache(serviceProvider);
        }

        /// <summary>
        /// Returns a serialization of an object into JSON format.
        /// </summary>
        /// <param name="value">The object to serialize to JSON.</param>
        /// <returns>Serialized value.</returns>
        public string SerializeObject(object value)
        {
            return SerializeObject(value, DefaultSerializerOptions);
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
                    if (RequiresJsonEscape(str[i], options))
                    {
                        StringBuilder sb = new StringBuilder(str.Length + 4);
                        sb.Append('"');
                        AppendStringEscape(sb, str, options);
                        sb.Append('"');
                        return sb.ToString();
                    }
                }
                return QuoteValue(str);
            }
            else
            {
                IConvertible convertibleValue = value as IConvertible;
                TypeCode objTypeCode = convertibleValue?.GetTypeCode() ?? TypeCode.Object;
                if (objTypeCode != TypeCode.Object && objTypeCode != TypeCode.Char && StringHelpers.IsNullOrWhiteSpace(options.Format) && options.FormatProvider == null)
                {
                    Enum enumValue;
                    if (!options.EnumAsInteger && IsNumericTypeCode(objTypeCode, false) && (enumValue = value as Enum) != null)
                    {
                        return QuoteValue(EnumAsString(enumValue));
                    }
                    else
                    {
                        string xmlStr = XmlHelper.XmlConvertToString(convertibleValue, objTypeCode);
                        if (SkipQuotes(convertibleValue, objTypeCode))
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
        /// <returns>Object serialized successfully (true/false).</returns>
        public bool SerializeObject(object value, StringBuilder destination)
        {
            return SerializeObject(value, destination, DefaultSerializerOptions);
        }

        /// <summary>
        /// Serialization of the object in JSON format to the destination StringBuilder
        /// </summary>
        /// <param name="value">The object to serialize to JSON.</param>
        /// <param name="destination">Write the resulting JSON to this destination.</param>
        /// <param name="options">serialisation options</param>
        /// <returns>Object serialized successfully (true/false).</returns>
        public bool SerializeObject(object value, StringBuilder destination, JsonSerializeOptions options)
        {
            return SerializeObject(value, destination, options, default(SingleItemOptimizedHashSet<object>), 0);
        }

        /// <summary>
        /// Serialization of the object in JSON format to the destination StringBuilder
        /// </summary>
        /// <param name="value">The object to serialize to JSON.</param>
        /// <param name="destination">Write the resulting JSON to this destination.</param>
        /// <param name="options">serialisation options</param>
        /// <param name="objectsInPath">The objects in path (Avoid cyclic reference loop).</param>
        /// <param name="depth">The current depth (level) of recursion.</param>
        /// <returns>Object serialized successfully (true/false).</returns>
        private bool SerializeObject(object value, StringBuilder destination, JsonSerializeOptions options, SingleItemOptimizedHashSet<object> objectsInPath, int depth)
        {
            int originalLength = destination.Length;

            try
            {
                if (SerializeSimpleObjectValue(value, destination, options))
                {
                    return true;
                }

                return SerializeObjectWithReflection(value, destination, options, ref objectsInPath, depth);
            }
            catch
            {
                destination.Length = originalLength;
                return false;
            }
        }

        private bool SerializeObjectWithReflection(object value, StringBuilder destination, JsonSerializeOptions options, ref SingleItemOptimizedHashSet<object> objectsInPath, int depth)
        {
            int originalLength = destination.Length;
            if (originalLength > MaxJsonLength)
            {
                return false;
            }

            if (objectsInPath.Contains(value))
            {
                return false;
            }

            if (value is IDictionary dict)
            {
                using (StartCollectionScope(ref objectsInPath, dict))
                {
                    SerializeDictionaryObject(dict, destination, options, objectsInPath, depth);
                    return true;
                }
            }

            if (value is IEnumerable enumerable)
            {
                if (_objectReflectionCache.TryLookupExpandoObject(value, out var propertyValues))
                {
                    if (propertyValues.ConvertToString || depth >= options.MaxRecursionLimit)
                    {
                        return SerializeObjectAsString(value, destination, options);
                    }
                    else
                    {
                        using (new SingleItemOptimizedHashSet<object>.SingleItemScopedInsert(value, ref objectsInPath, false, _referenceEqualsComparer))
                        {
                            return SerializeObjectProperties(propertyValues, destination, options, objectsInPath, depth);
                        }
                    }
                }

                using (StartCollectionScope(ref objectsInPath, value))
                {
                    SerializeCollectionObject(enumerable, destination, options, objectsInPath, depth);
                    return true;
                }
            }
            else
            {
                return SerializeObjectWithProperties(value, destination, options, ref objectsInPath, depth);
            }
        }

        private bool SerializeSimpleObjectValue(object value, StringBuilder destination, JsonSerializeOptions options, bool forceToString = false)
        {
            var convertibleValue = value as IConvertible;
            var objTypeCode = convertibleValue?.GetTypeCode() ?? (value == null ? TypeCode.Empty : TypeCode.Object);
            if (objTypeCode != TypeCode.Object)
            {
                SerializeSimpleTypeCodeValue(convertibleValue, objTypeCode, destination, options, forceToString);
                return true;
            }
            
            if (value is DateTimeOffset dateTimeOffset)
            {
                QuoteValue(destination, dateTimeOffset.ToString("yyyy-MM-dd HH:mm:ss zzz", CultureInfo.InvariantCulture));
                return true;
            }

            if (value is IFormattable formattable)
            {
                var hasFormat = !StringHelpers.IsNullOrWhiteSpace(options.Format);
                SerializeWithFormatProvider(formattable, true, destination, options, hasFormat);
                return true;
            }

            return false;   // Not simple
        }

        private static SingleItemOptimizedHashSet<object>.SingleItemScopedInsert StartCollectionScope(ref SingleItemOptimizedHashSet<object> objectsInPath, object value)
        {
            return new SingleItemOptimizedHashSet<object>.SingleItemScopedInsert(value, ref objectsInPath, true, _referenceEqualsComparer);
        }

        private void SerializeWithFormatProvider(IFormattable formattable, bool includeQuotes, StringBuilder destination, JsonSerializeOptions options, bool hasFormat)
        {
            if (includeQuotes)
            {
                destination.Append('"');
            }

            var formatProvider = options.FormatProvider ?? (hasFormat ? _defaultFormatProvider : null);
            var str = formattable.ToString(hasFormat ? options.Format : "", formatProvider);
            if (includeQuotes)
                AppendStringEscape(destination, str, options);
            else
                destination.Append(str);

            if (includeQuotes)
            {
                destination.Append('"');
            }
        }

        private void SerializeDictionaryObject(IDictionary dictionary, StringBuilder destination, JsonSerializeOptions options, SingleItemOptimizedHashSet<object> objectsInPath, int depth)
        {
            bool first = true;

            int nextDepth = objectsInPath.Count <= 1 ? depth : (depth + 1);
            if (nextDepth > options.MaxRecursionLimit)
            {
                destination.Append("{}");
                return;
            }

            destination.Append('{');
            foreach (var item in new DictionaryEntryEnumerable(dictionary))
            {
                var originalLength = destination.Length;
                if (originalLength > MaxJsonLength)
                {
                    break;
                }

                if (!first)
                {
                    destination.Append(',');
                }

                var itemKey = item.Key;
                if (options.QuoteKeys)
                {
                    if (!SerializeObjectAsString(itemKey, destination, options))
                    {
                        destination.Length = originalLength;
                        continue;
                    }
                }
                else
                {
                    if (!SerializeObject(itemKey, destination, options, objectsInPath, nextDepth))
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
                var itemValue = item.Value;
                if (!SerializeObject(itemValue, destination, options, objectsInPath, nextDepth))
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

                if (!SerializeObject(val, destination, options, objectsInPath, nextDepth))
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
            if (depth < options.MaxRecursionLimit)
            {
                var objectPropertyList = _objectReflectionCache.LookupObjectProperties(value);
                if (!objectPropertyList.ConvertToString)
                {
                    if (ReferenceEquals(options, DefaultSerializerOptions) && value is Exception)
                    {
                        // Exceptions are seldom under control, and can include random Data-Dictionary-keys, so we sanitize by default
                        options = DefaultExceptionSerializerOptions;
                    }

                    using (new SingleItemOptimizedHashSet<object>.SingleItemScopedInsert(value, ref objectsInPath, false, _referenceEqualsComparer))
                    {
                        return SerializeObjectProperties(objectPropertyList, destination, options, objectsInPath, depth);
                    }
                }
            }

            return SerializeObjectAsString(value, destination, options);
        }

        private void SerializeSimpleTypeCodeValue(IConvertible value, TypeCode objTypeCode, StringBuilder destination, JsonSerializeOptions options, bool forceToString = false)
        {
            if (value == null)
            {
                destination.Append(forceToString ? "\"\"" : "null");
            }
            else if (objTypeCode == TypeCode.String || objTypeCode == TypeCode.Char)
            {
                destination.Append('"');
                AppendStringEscape(destination, value.ToString(), options);
                destination.Append('"');
            }
            else
            {
                var hasFormat = !StringHelpers.IsNullOrWhiteSpace(options.Format);
                if ((options.FormatProvider != null || hasFormat) && (value is IFormattable formattable))
                {
                    bool includeQuotes = forceToString || objTypeCode == TypeCode.Object || !SkipQuotes(value, objTypeCode);
                    SerializeWithFormatProvider(formattable, includeQuotes, destination, options, hasFormat);
                }
                else
                {
                    SerializeSimpleTypeCodeValueNoEscape(value, objTypeCode, destination, options, forceToString);
                }
            }
        }

        private void SerializeSimpleTypeCodeValueNoEscape(IConvertible value, TypeCode objTypeCode, StringBuilder destination, JsonSerializeOptions options, bool forceToString)
        {
            if (IsNumericTypeCode(objTypeCode, false))
            {
                if (!options.EnumAsInteger && value is Enum enumValue)
                {
                    QuoteValue(destination, EnumAsString(enumValue));
                }
                else
                {
                    SerializeNumericValue(value, objTypeCode, destination, forceToString);
                }
            }
            else if (objTypeCode == TypeCode.DateTime)
            {
                destination.Append('"');
                destination.AppendXmlDateTimeRoundTrip(value.ToDateTime(CultureInfo.InvariantCulture));
                destination.Append('"');
            }
            else if (IsNumericTypeCode(objTypeCode, true) && SkipQuotes(value, objTypeCode))
            {
                SerializeNumericValue(value, objTypeCode, destination, forceToString);
            }
            else
            {
                string str = XmlHelper.XmlConvertToString(value, objTypeCode);
                if (!forceToString && !string.IsNullOrEmpty(str) && SkipQuotes(value, objTypeCode))
                {
                    destination.Append(str);
                }
                else
                {
                    QuoteValue(destination, str);
                }
            }
        }

        private void SerializeNumericValue(IConvertible value, TypeCode objTypeCode, StringBuilder destination, bool forceToString)
        {
            if (forceToString)
                destination.Append('"');
            destination.AppendNumericInvariant(value, objTypeCode);
            if (forceToString)
                destination.Append('"');
        }

        private static CultureInfo CreateFormatProvider()
        {
#if !NETSTANDARD1_3 && !NETSTANDARD1_5
            var culture = new CultureInfo("en-US", false);
#else
            var culture = new CultureInfo("en-US");            
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
        private static bool SkipQuotes(IConvertible value, TypeCode objTypeCode)
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
                        double dblValue = value.ToDouble(CultureInfo.InvariantCulture);
                        return !double.IsNaN(dblValue) && !double.IsInfinity(dblValue);
                    }
                case TypeCode.Single:
                    {
                        float floatValue = value.ToSingle(CultureInfo.InvariantCulture);
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
        /// <param name="options">all options</param>
        /// <returns>JSON escaped string</returns>
        private static void AppendStringEscape(StringBuilder destination, string text, JsonSerializeOptions options)
        {
            AppendStringEscape(destination, text, options.EscapeUnicode, options.EscapeForwardSlash);
        }

        /// <summary>
        /// Checks input string if it needs JSON escaping, and makes necessary conversion
        /// </summary>
        /// <param name="destination">Destination Builder</param>
        /// <param name="text">Input string</param>
        /// <param name="escapeUnicode">Should non-ascii characters be encoded</param>
        /// <param name="escapeForwardSlash"></param>
        /// <returns>JSON escaped string</returns>
        internal static void AppendStringEscape(StringBuilder destination, string text, bool escapeUnicode, bool escapeForwardSlash)
        {
            if (string.IsNullOrEmpty(text))
                return;

            StringBuilder sb = null;

            for (int i = 0; i < text.Length; ++i)
            {
                char ch = text[i];
                if (!RequiresJsonEscape(ch, escapeUnicode, escapeForwardSlash))
                {
                    sb?.Append(ch);
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

                    case '\b':
                        sb.Append("\\b");
                        break;

                    case '/':
                        if (escapeForwardSlash)
                        {
                            sb.Append("\\/");
                        }
                        else
                        {
                            sb.Append(ch);
                        }
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

        internal static bool RequiresJsonEscape(char ch, JsonSerializeOptions options)
        {
            return RequiresJsonEscape(ch, options.EscapeUnicode, options.EscapeForwardSlash);
        }

        internal static bool RequiresJsonEscape(char ch, bool escapeUnicode, bool escapeForwardSlash)
        {
            if (!EscapeChar(ch, escapeUnicode))
            {
                switch (ch)
                {
                    case '/': return escapeForwardSlash;
                    case '"':
                    case '\\':
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

        private bool SerializeObjectProperties(ObjectReflectionCache.ObjectPropertyList objectPropertyList, StringBuilder destination, JsonSerializeOptions options,
            SingleItemOptimizedHashSet<object> objectsInPath, int depth)
        {
            destination.Append('{');

            bool first = true;
            foreach (var propertyValue in objectPropertyList)
            {
                var originalLength = destination.Length;

                try
                {
                    if (!propertyValue.HasNameAndValue)
                        continue;

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

                    var objTypeCode = propertyValue.TypeCode;
                    if (objTypeCode != TypeCode.Object)
                    {
                        SerializeSimpleTypeCodeValue((IConvertible)propertyValue.Value, objTypeCode, destination, options);
                        first = false;
                    }
                    else
                    {
                        if (!SerializeObject(propertyValue.Value, destination, options, objectsInPath, depth + 1))
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
                    // skip single property
                    destination.Length = originalLength;
                }
            }

            destination.Append('}');
            return true;
        }

        private bool SerializeObjectAsString(object value, StringBuilder destination, JsonSerializeOptions options)
        {
            var originalLength = destination.Length;

            try
            {
                if (SerializeSimpleObjectValue(value, destination, options, true))
                {
                    return true;
                }

                var hasFormat = !StringHelpers.IsNullOrWhiteSpace(options.Format);
                if ((options.FormatProvider != null || hasFormat) && (value is IFormattable formattable))
                {
                    SerializeWithFormatProvider(formattable, true, destination, options, hasFormat);
                    return true;
                }

                var str = Convert.ToString(value, CultureInfo.InvariantCulture);
                destination.Append('"');
                AppendStringEscape(destination, str, options);
                destination.Append('"');
                return true;
            }
            catch
            {
                // skip bad object
                destination.Length = originalLength;
                return false;
            }
        }
    }
}
