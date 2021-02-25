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

namespace NLog.Internal
{
    using System;
    using System.Globalization;
    using System.Text;
    using System.Xml;

    /// <summary>
    ///  Helper class for XML
    /// </summary>
    internal static class XmlHelper
    {
        // found on https://stackoverflow.com/questions/397250/unicode-regex-invalid-xml-characters/961504#961504
        // filters control characters but allows only properly-formed surrogate sequences
#if NET35 || NETSTANDARD1_3 || NETSTANDARD1_5
        private static readonly System.Text.RegularExpressions.Regex InvalidXmlChars = new System.Text.RegularExpressions.Regex(
            @"(?<![\uD800-\uDBFF])[\uDC00-\uDFFF]|[\uD800-\uDBFF](?![\uDC00-\uDFFF])|[\x00-\x08\x0B\x0C\x0E-\x1F\x7F-\x9F\uFEFF\uFFFE\uFFFF]",
            System.Text.RegularExpressions.RegexOptions.Compiled);
#endif

        /// <summary>
        /// removes any unusual unicode characters that can't be encoded into XML
        /// </summary>
        private static string RemoveInvalidXmlChars(string text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

#if !NET35 && !NETSTANDARD1_3 && !NETSTANDARD1_5
            int length = text.Length;
            for (int i = 0; i < length; ++i)
            {
                char ch = text[i];
                if (!XmlConvert.IsXmlChar(ch) && !(i + 1 < text.Length && XmlConvert.IsXmlSurrogatePair(text[i + 1], text[i])))
                {
                    return CreateValidXmlString(text);   // rare expensive case
                }
                else
                {
                    ++i;
                }
            }
            return text;
#else
            return InvalidXmlChars.Replace(text, "");
#endif
        }

#if !NET35 && !NETSTANDARD1_3 && !NETSTANDARD1_5
        /// <summary>
        /// Cleans string of any invalid XML chars found
        /// </summary>
        /// <param name="text">unclean string</param>
        /// <returns>string with only valid XML chars</returns>
        private static string CreateValidXmlString(string text)
        {
            var sb = new StringBuilder(text.Length);
            for (int i = 0; i < text.Length; ++i)
            {
                char ch = text[i];
                if (XmlConvert.IsXmlChar(ch))
                {
                    sb.Append(ch);
                }
            }
            return sb.ToString();
        }
#endif

        private static readonly char[] XmlEscapeChars = new char[] { '<', '>', '&', '\'', '"' };
        private static readonly char[] XmlEscapeNewlineChars = new char[] { '<', '>', '&', '\'', '"', '\r', '\n' };

        internal static string EscapeXmlString(string text, bool xmlEncodeNewlines, StringBuilder result = null)
        {
            if (result == null && SmallAndNoEscapeNeeded(text, xmlEncodeNewlines))
            {
                return text;
            }

            var sb = result ?? new StringBuilder(text.Length);
            for (int i = 0; i < text.Length; ++i)
            {
                switch (text[i])
                {
                    case '<':
                        sb.Append("&lt;");
                        break;

                    case '>':
                        sb.Append("&gt;");
                        break;

                    case '&':
                        sb.Append("&amp;");
                        break;

                    case '\'':
                        sb.Append("&apos;");
                        break;

                    case '"':
                        sb.Append("&quot;");
                        break;

                    case '\r':
                        if (xmlEncodeNewlines)
                            sb.Append("&#13;");
                        else
                            sb.Append(text[i]);
                        break;

                    case '\n':
                        if (xmlEncodeNewlines)
                            sb.Append("&#10;");
                        else
                            sb.Append(text[i]);
                        break;

                    default:
                        sb.Append(text[i]);
                        break;
                }
            }

            return result == null ? sb.ToString() : null;
        }

        /// <summary>
        /// Pretest, small text and not escape needed
        /// </summary>
        /// <param name="text"></param>
        /// <param name="xmlEncodeNewlines"></param>
        /// <returns></returns>
        private static bool SmallAndNoEscapeNeeded(string text, bool xmlEncodeNewlines)
        {
            return text.Length < 4096 && text.IndexOfAny(xmlEncodeNewlines ? XmlEscapeNewlineChars : XmlEscapeChars) < 0;
        }

        /// <summary>
        /// Converts object value to invariant format, and strips any invalid xml-characters
        /// </summary>
        /// <param name="value">Object value</param>
        /// <returns>Object value converted to string</returns>
        internal static string XmlConvertToStringSafe(object value)
        {
            return XmlConvertToString(value, true);
        }

        /// <summary>
        /// Converts object value to invariant format (understood by JavaScript)
        /// </summary>
        /// <param name="value">Object value</param>
        /// <returns>Object value converted to string</returns>
        internal static string XmlConvertToString(object value)
        {
            return XmlConvertToString(value, false);
        }

        internal static string XmlConvertToString(float value)
        {
            if (float.IsNaN(value))
                return XmlConvert.ToString(value);

            if (float.IsInfinity(value))
                return Convert.ToString(value, CultureInfo.InvariantCulture);

            return EnsureDecimalPlace(XmlConvert.ToString(value));
        }

        internal static string XmlConvertToString(double value)
        {
            if (double.IsNaN(value))
                return XmlConvert.ToString(value);

            if (double.IsInfinity(value))
                return Convert.ToString(value, CultureInfo.InvariantCulture);

            return EnsureDecimalPlace(XmlConvert.ToString(value));
        }

        internal static string XmlConvertToString(decimal value)
        {
            return EnsureDecimalPlace(XmlConvert.ToString(value));
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

            StringBuilder sb = null;
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

                if (sb == null)
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

        private static string XmlConvertToString(object value, bool safeConversion)
        {
            try
            {
                var convertibleValue = value as IConvertible;
                var objTypeCode = convertibleValue?.GetTypeCode() ?? (value == null ? TypeCode.Empty : TypeCode.Object);
                if (objTypeCode != TypeCode.Object)
                {
                    return XmlConvertToString(convertibleValue, objTypeCode, safeConversion);
                }

                return XmlConvertToStringInvariant(value, safeConversion);
            }
            catch
            {
                return safeConversion ? "" : null;
            }
        }

        private static string XmlConvertToStringInvariant(object value, bool safeConversion)
        {
            try
            {
                string valueString = Convert.ToString(value, CultureInfo.InvariantCulture);
                return safeConversion ? RemoveInvalidXmlChars(valueString) : valueString;
            }
            catch
            {
                return safeConversion ? "" : null;
            }
        }

        /// <summary>
        /// Converts object value to invariant format (understood by JavaScript)
        /// </summary>
        /// <param name="value">Object value</param>
        /// <param name="objTypeCode">Object TypeCode</param>
        /// <param name="safeConversion">Check and remove unusual unicode characters from the result string.</param>
        /// <returns>Object value converted to string</returns>
        internal static string XmlConvertToString(IConvertible value, TypeCode objTypeCode, bool safeConversion = false)
        {
            if (value == null)
            {
                return "null";
            }

            switch (objTypeCode)
            {
                case TypeCode.Boolean:
                    return XmlConvert.ToString(value.ToBoolean(CultureInfo.InvariantCulture));   // boolean as lowercase
                case TypeCode.Byte:
                    return XmlConvert.ToString(value.ToByte(CultureInfo.InvariantCulture));
                case TypeCode.SByte:
                    return XmlConvert.ToString(value.ToSByte(CultureInfo.InvariantCulture));
                case TypeCode.Int16:
                    return XmlConvert.ToString(value.ToInt16(CultureInfo.InvariantCulture));
                case TypeCode.Int32:
                    return XmlConvert.ToString(value.ToInt32(CultureInfo.InvariantCulture));
                case TypeCode.Int64:
                    return XmlConvert.ToString(value.ToInt64(CultureInfo.InvariantCulture));
                case TypeCode.UInt16:
                    return XmlConvert.ToString(value.ToUInt16(CultureInfo.InvariantCulture));
                case TypeCode.UInt32:
                    return XmlConvert.ToString(value.ToUInt32(CultureInfo.InvariantCulture));
                case TypeCode.UInt64:
                    return XmlConvert.ToString(value.ToUInt64(CultureInfo.InvariantCulture));
                case TypeCode.Single:
                    return XmlConvertToString(value.ToSingle(CultureInfo.InvariantCulture));
                case TypeCode.Double:
                    return XmlConvertToString(value.ToDouble(CultureInfo.InvariantCulture));
                case TypeCode.Decimal:
                    return XmlConvertToString(value.ToDecimal(CultureInfo.InvariantCulture));
                case TypeCode.DateTime:
                    return XmlConvert.ToString(value.ToDateTime(CultureInfo.InvariantCulture), XmlDateTimeSerializationMode.Utc);
                case TypeCode.Char:
                    return XmlConvert.ToString(value.ToChar(CultureInfo.InvariantCulture));
                case TypeCode.String:
                    return safeConversion ? RemoveInvalidXmlChars(value.ToString(CultureInfo.InvariantCulture)) : value.ToString(CultureInfo.InvariantCulture);
                default:
                    return XmlConvertToStringInvariant(value, safeConversion);
            }
        }

        /// <summary>
        /// Safe version of WriteAttributeString
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="localName"></param>
        /// <param name="value"></param>
        public static void WriteAttributeSafeString(this XmlWriter writer, string localName, string value)
        {
            writer.WriteAttributeString(localName, RemoveInvalidXmlChars(value));
        }

        /// <summary>
        /// Safe version of WriteElementSafeString
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="prefix"></param>
        /// <param name="localName"></param>
        /// <param name="ns"></param>
        /// <param name="value"></param>
        public static void WriteElementSafeString(this XmlWriter writer, string prefix, string localName, string ns, string value)
        {
            writer.WriteElementString(prefix, localName, ns, RemoveInvalidXmlChars(value));
        }

        /// <summary>
        /// Safe version of WriteCData
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="value"></param>
        public static void WriteSafeCData(this XmlWriter writer, string value)
        {
            writer.WriteCData(RemoveInvalidXmlChars(value));
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
    }
}
