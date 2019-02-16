// 
// Copyright (c) 2004-2019 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Xml;
    using NLog.Internal;

    /// <summary>
    ///  Helper class for XML
    /// </summary>
    public static class XmlHelper
    {
        // found on https://stackoverflow.com/questions/397250/unicode-regex-invalid-xml-characters/961504#961504
        // filters control characters but allows only properly-formed surrogate sequences
#if NET3_5 || NETSTANDARD1_0
        private static readonly Regex InvalidXmlChars = new Regex(
            @"(?<![\uD800-\uDBFF])[\uDC00-\uDFFF]|[\uD800-\uDBFF](?![\uDC00-\uDFFF])|[\x00-\x08\x0B\x0C\x0E-\x1F\x7F-\x9F\uFEFF\uFFFE\uFFFF]",
            RegexOptions.Compiled);
#endif

        /// <summary>
        /// removes any unusual unicode characters that can't be encoded into XML
        /// </summary>
        private static string RemoveInvalidXmlChars(string text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

#if NET3_5 || NETSTANDARD1_0
            return InvalidXmlChars.Replace(text, "");
#else
            for (int i = 0; i < text.Length; ++i)
            {
                char ch = text[i];
                if (!XmlConvert.IsXmlChar(ch))
                {
                    return CreateValidXmlString(text);   // rare expensive case
                }
            }
            return text;
#endif
        }

#if !NET3_5 && !NETSTANDARD1_0
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
            TypeCode objTypeCode = Convert.GetTypeCode(value);
            return XmlConvertToString(value, objTypeCode, true);
        }

        /// <summary>
        /// Converts object value to invariant format (understood by JavaScript)
        /// </summary>
        /// <param name="value">Object value</param>
        /// <returns>Object value converted to string</returns>
        internal static string XmlConvertToString(object value)
        {
            TypeCode objTypeCode = Convert.GetTypeCode(value);
            return XmlConvertToString(value, objTypeCode, false);
        }

        /// <summary>
        /// XML elements must follow these naming rules:
        ///  - Element names are case-sensitive
        ///  - Element names must start with a letter or underscore
        ///  - Element names can contain letters, digits, hyphens, underscores, and periods
        ///  - Element names cannot contain spaces
        /// </summary>
        /// <param name="xmlElementName"></param>
        /// <param name="allowNamespace"></param>
        internal static string XmlConvertToElementName(string xmlElementName, bool allowNamespace)
        {
            if (string.IsNullOrEmpty(xmlElementName))
                return xmlElementName;

            xmlElementName = RemoveInvalidXmlChars(xmlElementName);

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
                    case ':':   // namespace-delimeter
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
                    sb = CreateStringBuilder(i);
                }
                sb.Append('_');
                if (includeChr)
                    sb.Append(chr);
            }

            sb?.TrimRight();
            return sb?.ToString() ?? xmlElementName;

            StringBuilder CreateStringBuilder(int i)
            {
                var sb2 = new StringBuilder(xmlElementName.Length);
                if (i > 0)
                    sb2.Append(xmlElementName, 0, i);
                return sb2;
            }
        }

        /// <summary>
        /// Converts object value to invariant format (understood by JavaScript)
        /// </summary>
        /// <param name="value">Object value</param>
        /// <param name="objTypeCode">Object TypeCode</param>
        /// <param name="safeConversion">Check and remove unusual unicode characters from the result string.</param>
        /// <returns>Object value converted to string</returns>
        internal static string XmlConvertToString(object value, TypeCode objTypeCode, bool safeConversion = false)
        {
            if (value == null)
            {
                return "null";
            }

            switch (objTypeCode)
            {
                case TypeCode.Boolean:
                    return XmlConvert.ToString((bool)value);   // boolean as lowercase
                case TypeCode.Byte:
                    return XmlConvert.ToString((byte)value);
                case TypeCode.SByte:
                    return XmlConvert.ToString((sbyte)value);
                case TypeCode.Int16:
                    return XmlConvert.ToString((short)value);
                case TypeCode.Int32:
                    return XmlConvert.ToString((int)value);
                case TypeCode.Int64:
                    return XmlConvert.ToString((long)value);
                case TypeCode.UInt16:
                    return XmlConvert.ToString((ushort)value);
                case TypeCode.UInt32:
                    return XmlConvert.ToString((uint)value);
                case TypeCode.UInt64:
                    return XmlConvert.ToString((ulong)value);
                case TypeCode.Single:
                    {
                        float singleValue = (float)value;
                        if (float.IsInfinity(singleValue))
                            return Convert.ToString(singleValue, System.Globalization.CultureInfo.InvariantCulture); // Infinity instead of INF
                        else
                            return XmlConvert.ToString(singleValue);    // 8 digits scale
                    }
                case TypeCode.Double:
                    {
                        double doubleValue = (double)value;
                        if (double.IsInfinity(doubleValue))
                            return Convert.ToString(doubleValue, System.Globalization.CultureInfo.InvariantCulture); // Infinity instead of INF
                        else
                            return XmlConvert.ToString(doubleValue);    // 16 digits scale
                    }
                case TypeCode.Decimal:
                    return XmlConvert.ToString((decimal)value);
                case TypeCode.DateTime:
                    return XmlConvert.ToString((DateTime)value, XmlDateTimeSerializationMode.Utc);
                case TypeCode.Char:
                    return XmlConvert.ToString((char)value);
                case TypeCode.String:
                    return safeConversion ? RemoveInvalidXmlChars((string)value) : (string)value;
                default:
                    try
                    {
                        string valueString = Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture);
                        return safeConversion ? RemoveInvalidXmlChars(valueString) : valueString;
                    }
                    catch
                    {
                        return null;
                    }
            }
        }

        /// <summary>
        /// Safe version of WriteAttributeString
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="prefix"></param>
        /// <param name="localName"></param>
        /// <param name="ns"></param>
        /// <param name="value"></param>
        public static void WriteAttributeSafeString(this XmlWriter writer, string prefix, string localName, string ns, string value)
        {
            writer.WriteAttributeString(RemoveInvalidXmlChars(prefix), RemoveInvalidXmlChars(localName), RemoveInvalidXmlChars(ns), RemoveInvalidXmlChars(value));
        }

        /// <summary>
        /// Safe version of WriteAttributeString
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="localName"></param>
        /// <param name="value"></param>
        public static void WriteAttributeSafeString(this XmlWriter writer, string localName, string value)
        {
            writer.WriteAttributeString(RemoveInvalidXmlChars(localName), RemoveInvalidXmlChars(value));
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
            writer.WriteElementString(RemoveInvalidXmlChars(prefix), RemoveInvalidXmlChars(localName), RemoveInvalidXmlChars(ns),
                                      RemoveInvalidXmlChars(value));
        }

        /// <summary>
        /// Safe version of WriteCData
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="text"></param>
        public static void WriteSafeCData(this XmlWriter writer, string text)
        {
            writer.WriteCData(RemoveInvalidXmlChars(text));
        }
    }
}
