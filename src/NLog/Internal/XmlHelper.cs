//
// Copyright (c) 2004-2024 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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

namespace NLog.Internal
{
    using System;
    using System.Globalization;
    using System.Text;

    /// <summary>
    ///  Helper class for XML
    /// </summary>
    internal static class XmlHelper
    {
        const char HIGH_SURROGATE_START = '\ud800';
        const char HIGH_SURROGATE_END = '\udbff';

        const char LOW_SURROGATE_START = '\udc00';
        const char LOW_SURROGATE_END = '\udfff';

        internal static bool XmlConvertIsXmlChar(char chr)
        {
            return (chr > '\u001f' && chr < HIGH_SURROGATE_START) || ExoticIsXmlChar(chr);
        }

        private static bool ExoticIsXmlChar(char chr)
        {
            if (chr < '\u0020')
                return chr == '\u0009' || chr == '\u000a' || chr == '\u000d';

            if (XmlConvertIsHighSurrogate(chr) || XmlConvertIsLowSurrogate(chr))
                return false;

            if (chr == '\ufffe' || chr == '\uffff')
                return false;

            return true;
        }

        public static bool XmlConvertIsHighSurrogate(char chr)
        {
            return chr >= HIGH_SURROGATE_START && chr <= HIGH_SURROGATE_END;
        }

        public static bool XmlConvertIsLowSurrogate(char chr)
        {
            return chr >= LOW_SURROGATE_START && chr <= LOW_SURROGATE_END;
        }

        public static bool XmlConvertIsXmlSurrogatePair(char lowChar, char highChar)
        {
            return XmlConvertIsHighSurrogate(highChar) && XmlConvertIsLowSurrogate(lowChar);
        }

        /// <summary>
        /// removes any unusual unicode characters that can't be encoded into XML
        /// </summary>
        private static string RemoveInvalidXmlChars(string text, StringBuilder? builder = null)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            int length = text.Length;
            for (int i = 0; i < length; ++i)
            {
                char ch = text[i];
                if (!XmlConvertIsXmlChar(ch))
                {
                    if (i + 1 < text.Length && XmlConvertIsXmlSurrogatePair(text[i + 1], ch))
                    {
                        ++i;
                    }
                    else
                    {
                        return CreateValidXmlString(text, i, builder);   // rare expensive case
                    }
                }
            }

            builder?.Append(text);
            return text;
        }

        /// <summary>
        /// Cleans string of any invalid XML chars found
        /// </summary>
        /// <returns>string with only valid XML chars</returns>
        private static string CreateValidXmlString(string text, int startIndex = 0, StringBuilder? builder = null)
        {
            var sb = builder ?? new StringBuilder(text.Length);
            sb.Append(text, 0, startIndex);
            for (int i = startIndex; i < text.Length; ++i)
            {
                char ch = text[i];
                if (XmlConvertIsXmlChar(ch))
                {
                    sb.Append(ch);
                }
                else if (i + 1 < text.Length && XmlConvertIsXmlSurrogatePair(text[i + 1], ch))
                {
                    sb.Append(ch);
                    sb.Append(text[++i]);
                }
            }
            return builder is null ? sb.ToString() : string.Empty;
        }

        internal static void EscapeXmlWhenNeeded(StringBuilder builder, int startPos, bool xmlEncodeNewlines)
        {
            var builderLength = builder.Length;
            for (int i = startPos; i < builderLength; ++i)
            {
                var chr = builder[i];
                if (!XmlConvertIsXmlChar(chr) || RequiresXmlEscape(chr, xmlEncodeNewlines))
                {
                    var text = builder.ToString(i, builderLength - i);
                    builder.Length = i;
                    EscapeXmlString(text, xmlEncodeNewlines, builder);
                    break;
                }
            }
        }

        internal static string EscapeXmlWhenNeeded(string text, bool xmlEncodeNewlines, StringBuilder? builder = null)
        {
            var startIndex = text.IndexOfAny(xmlEncodeNewlines ? XmlEscapeNewlineChars : XmlEscapeChars);
            if (startIndex >= 0)
            {
                var sb = builder ?? new StringBuilder(text.Length + 16);
                EscapeXmlString(text, xmlEncodeNewlines, sb);
                return builder is null ? sb.ToString() : string.Empty;
            }
            else
            {
                return RemoveInvalidXmlChars(text, builder);
            }
        }

        private static bool RequiresXmlEscape(char chr, bool xmlEncodeNewlines)
        {
            switch (chr)
            {
                case '<':
                case '>':
                case '&':
                case '\'':
                case '"':
                    return true;
                case '\r':
                case '\n':
                    return xmlEncodeNewlines;
            }

            return false;
        }

        private static readonly char[] XmlEscapeChars = new char[] { '<', '>', '&', '\'', '"' };
        private static readonly char[] XmlEscapeNewlineChars = new char[] { '<', '>', '&', '\'', '"', '\r', '\n' };

        private static void EscapeXmlString(string text, bool xmlEncodeNewlines, StringBuilder destination)
        {
            for (int i = 0; i < text.Length; ++i)
            {
                char chr = text[i];
                switch (chr)
                {
                    case '<':
                        destination.Append("&lt;");
                        break;

                    case '>':
                        destination.Append("&gt;");
                        break;

                    case '&':
                        destination.Append("&amp;");
                        break;

                    case '\'':
                        destination.Append("&apos;");
                        break;

                    case '"':
                        destination.Append("&quot;");
                        break;

                    case '\r':
                        if (xmlEncodeNewlines)
                            destination.Append("&#13;");
                        else
                            destination.Append(chr);
                        break;

                    case '\n':
                        if (xmlEncodeNewlines)
                            destination.Append("&#10;");
                        else
                            destination.Append(chr);
                        break;

                    default:
                        if (XmlConvertIsXmlChar(chr))
                        {
                            destination.Append(chr);
                        }
                        else if (i + 1 < text.Length && XmlConvertIsXmlSurrogatePair(text[i + 1], chr))
                        {
                            destination.Append(chr);
                            destination.Append(text[++i]);
                        }                        
                        break;
                }

            }
        }

        /// <summary>
        /// Converts object value to invariant format, and strips any invalid xml-characters
        /// </summary>
        /// <param name="value">Object value</param>
        /// <returns>Object value converted to string</returns>
        internal static string XmlConvertToStringSafe(object? value)
        {
            return XmlConvertToString(value, true);
        }

        /// <summary>
        /// Converts object value to invariant format (understood by JavaScript)
        /// </summary>
        /// <param name="value">Object value</param>
        /// <returns>Object value converted to string</returns>
        internal static string XmlConvertToString(object? value)
        {
            return value is string stringValue ? stringValue : XmlConvertToString(value, false);
        }

        internal static string XmlConvertToString(float value)
        {
            if (float.IsInfinity(value) || float.IsNaN(value))
                return Convert.ToString(value, CultureInfo.InvariantCulture);
            else
                return EnsureDecimalPlace(value.ToString("R", NumberFormatInfo.InvariantInfo));
        }

        internal static string XmlConvertToString(double value)
        {
            if (double.IsInfinity(value) || double.IsNaN(value))
                return Convert.ToString(value, CultureInfo.InvariantCulture);
            else
                return EnsureDecimalPlace(value.ToString("R", NumberFormatInfo.InvariantInfo));
        }

        internal static string XmlConvertToString(decimal value)
        {
            return EnsureDecimalPlace(value.ToString(null, NumberFormatInfo.InvariantInfo));
        }

        /// <summary>
        /// Converts DateTime to ISO 8601 format in UTC timezone.
        /// </summary>
        internal static string XmlConvertToString(DateTime value)
        {
            if (value.Kind == DateTimeKind.Unspecified)
                value = new DateTime(value.Ticks, DateTimeKind.Utc);
            else
                value = value.ToUniversalTime();
            return value.ToString("yyyy-MM-ddTHH:mm:ss.FFFFFFFK", CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Converts object value to invariant format (understood by JavaScript)
        /// </summary>
        /// <param name="value">Object value</param>
        /// <param name="objTypeCode">Object TypeCode</param>
        /// <param name="safeConversion">Check and remove unusual unicode characters from the result string.</param>
        /// <returns>Object value converted to string</returns>
        internal static string XmlConvertToString(IConvertible? value, TypeCode objTypeCode, bool safeConversion = false)
        {
            if (objTypeCode == TypeCode.Empty || value is null)
            {
                return "null";
            }

            switch (objTypeCode)
            {
                case TypeCode.Boolean:
                    return value.ToBoolean(CultureInfo.InvariantCulture) ? "true" : "false";   // boolean as lowercase
                case TypeCode.Byte:
                    return value.ToByte(CultureInfo.InvariantCulture).ToString(null, NumberFormatInfo.InvariantInfo);
                case TypeCode.SByte:
                    return value.ToSByte(CultureInfo.InvariantCulture).ToString(null, NumberFormatInfo.InvariantInfo);
                case TypeCode.Int16:
                    return value.ToInt16(CultureInfo.InvariantCulture).ToString(null, NumberFormatInfo.InvariantInfo);
                case TypeCode.Int32:
                    return value.ToInt32(CultureInfo.InvariantCulture).ToString(null, NumberFormatInfo.InvariantInfo);
                case TypeCode.Int64:
                    return value.ToInt64(CultureInfo.InvariantCulture).ToString(null, NumberFormatInfo.InvariantInfo);
                case TypeCode.UInt16:
                    return value.ToUInt16(CultureInfo.InvariantCulture).ToString(null, NumberFormatInfo.InvariantInfo);
                case TypeCode.UInt32:
                    return value.ToUInt32(CultureInfo.InvariantCulture).ToString(null, NumberFormatInfo.InvariantInfo);
                case TypeCode.UInt64:
                    return value.ToUInt64(CultureInfo.InvariantCulture).ToString(null, NumberFormatInfo.InvariantInfo);
                case TypeCode.Single:
                    return XmlConvertToString(value.ToSingle(CultureInfo.InvariantCulture));
                case TypeCode.Double:
                    return XmlConvertToString(value.ToDouble(CultureInfo.InvariantCulture));
                case TypeCode.Decimal:
                    return XmlConvertToString(value.ToDecimal(CultureInfo.InvariantCulture));
                case TypeCode.DateTime:
                    return XmlConvertToString(value.ToDateTime(CultureInfo.InvariantCulture));
                case TypeCode.Char:
                    return safeConversion ? RemoveInvalidXmlChars(value.ToString(CultureInfo.InvariantCulture)) : value.ToString(CultureInfo.InvariantCulture);
                case TypeCode.String:
                    return safeConversion ? RemoveInvalidXmlChars(value.ToString(CultureInfo.InvariantCulture)) : value.ToString(CultureInfo.InvariantCulture);
                default:
                    return XmlConvertToStringInvariant(value, safeConversion);
            }
        }

        private static string XmlConvertToString(object? value, bool safeConversion)
        {
            try
            {
                var convertibleValue = value as IConvertible;
                var objTypeCode = convertibleValue?.GetTypeCode() ?? (value is null ? TypeCode.Empty : TypeCode.Object);
                if (objTypeCode != TypeCode.Object)
                {
                    return XmlConvertToString(convertibleValue, objTypeCode, safeConversion);
                }

                return XmlConvertToStringInvariant(value, safeConversion);
            }
            catch
            {
                return string.Empty;
            }
        }

        private static string XmlConvertToStringInvariant(object? value, bool safeConversion)
        {
            try
            {
                var valueString = Convert.ToString(value, CultureInfo.InvariantCulture);
                if (valueString is null)
                    return string.Empty;
                return safeConversion ? RemoveInvalidXmlChars(valueString) : valueString;
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// XML elements must follow these naming rules:
        ///  - Element names are case-sensitive
        ///  - Element names must start with a letter or underscore
        ///  - Element names can contain letters, digits, hyphens, underscores, and periods
        ///  - Element names cannot contain spaces
        /// </summary>
        /// <param name="xmlElementName"></param>
        internal static string XmlConvertToElementName(string xmlElementName)
        {
            if (string.IsNullOrEmpty(xmlElementName))
                return xmlElementName;

            xmlElementName = RemoveInvalidXmlChars(xmlElementName);

            bool allowNamespace = true;

            StringBuilder? sb = null;
            for (int i = 0; i < xmlElementName.Length; ++i)
            {
                char chr = xmlElementName[i];
                if (char.IsLetter(chr))
                {
                    sb?.Append(chr);
                    continue;
                }

                bool includeChr = false;
                switch (chr)
                {
                    case ':':   // namespace-delimiter
                        if (i != 0 && allowNamespace)
                        {
                            allowNamespace = false;
                            sb?.Append(chr);
                            continue;
                        }
                        break;
                    case '_':
                        sb?.Append(chr);
                        continue;
                    case '0':
                    case '1':
                    case '2':
                    case '3':
                    case '4':
                    case '5':
                    case '6':
                    case '7':
                    case '8':
                    case '9':
                    case '-':
                    case '.':
                        {
                            if (i != 0)
                            {
                                sb?.Append(chr);
                                continue;
                            }
                            includeChr = true;
                            break;
                        }
                }

                if (sb is null)
                {
                    sb = CreateStringBuilder(xmlElementName, i);
                }
                sb.Append('_');
                if (includeChr)
                    sb.Append(chr);
            }

            sb?.TrimRight();
            return sb?.ToString() ?? xmlElementName;

            StringBuilder CreateStringBuilder(string orgValue, int i)
            {
                var sb2 = new StringBuilder(orgValue.Length);
                if (i > 0)
                    sb2.Append(orgValue, 0, i);
                return sb2;
            }
        }

        private static string EnsureDecimalPlace(string text)
        {
            if (text.IndexOf('.') != -1 || text.IndexOfAny(DecimalScientificExponent) != -1)
            {
                return text;
            }
            else if (text.Length == 1)
            {
                switch (text[0])
                {
                    case '0': return "0.0";
                    case '1': return "1.0";
                    case '2': return "2.0";
                    case '3': return "3.0";
                    case '4': return "4.0";
                    case '5': return "5.0";
                    case '6': return "6.0";
                    case '7': return "7.0";
                    case '8': return "8.0";
                    case '9': return "9.0";
                }
            }
            return text + ".0";
        }

        private static readonly char[] DecimalScientificExponent = new[] { 'e', 'E' };


        public static void EscapeCDataWhenNeeded(StringBuilder builder, int orgLength)
        {
            var builderLength = builder.Length;
            char chr0 = '\0';
            char chr1 = chr0;
            char chr2 = chr0;

            for (int i = orgLength; i < builderLength; ++i)
            {
                chr0 = chr1;
                chr1 = chr2;
                chr2 = builder[i];

                if (!XmlConvertIsXmlChar(chr2) || (chr0 == ']' && chr1 == ']' && chr2 == '>'))
                {
                    var text = builder.ToString(i - 2, builderLength - i + 2);
                    builder.Length = i - 2;
                    text = RemoveInvalidXmlChars(text);
                    text = text.Replace("]]>", "]]]]><![CDATA[>");
                    builder.Append(text);
                    break;
                }
            }
        }

        public static string EscapeCData(string text, StringBuilder? builder = null)
        {
            if (string.IsNullOrEmpty(text))
            {
                var emptyCData = "<![CDATA[]]>";
                builder?.Append(emptyCData);
                return emptyCData;
            }

            builder?.Append("<![CDATA[");
            int orgLength = builder?.Length ?? 0;
            var validText = RemoveInvalidXmlChars(text, builder);
            if (ReferenceEquals(validText, text) || builder is null)
            {
                if (validText.IndexOf("]]>", StringComparison.Ordinal) >= 0)
                {
                    validText = validText.Replace("]]>", "]]]]><![CDATA[>");
                    if (builder != null)
                    {
                        builder.Length = orgLength;
                        builder.Append(validText);
                    }
                }
            }
            else
            {
                EscapeCDataWhenNeeded(builder, orgLength);
            }
            builder?.Append("]]>");

            return builder is null ? $"<![CDATA[{validText}]]>" : string.Empty;
        }
    }
}
