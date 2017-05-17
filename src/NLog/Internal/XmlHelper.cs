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

namespace NLog.Internal
{
    using System;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Xml;

    /// <summary>
    ///  Helper class for XML
    /// </summary>
    public static class XmlHelper
    {
        // found on http://stackoverflow.com/questions/397250/unicode-regex-invalid-xml-characters/961504#961504
        // filters control characters but allows only properly-formed surrogate sequences
#if !SILVERLIGHT
        private static readonly Regex InvalidXmlChars = new Regex(
            @"(?<![\uD800-\uDBFF])[\uDC00-\uDFFF]|[\uD800-\uDBFF](?![\uDC00-\uDFFF])|[\x00-\x08\x0B\x0C\x0E-\x1F\x7F-\x9F\uFEFF\uFFFE\uFFFF]",
            RegexOptions.Compiled);
#else
		private static readonly Regex InvalidXmlChars = new Regex(
			@"(?<![\uD800-\uDBFF])[\uDC00-\uDFFF]|[\uD800-\uDBFF](?![\uDC00-\uDFFF])|[\x00-\x08\x0B\x0C\x0E-\x1F\x7F-\x9F\uFEFF\uFFFE\uFFFF]");
#endif

        /// <summary>
        /// removes any unusual unicode characters that can't be encoded into XML
        /// </summary>
        private static string RemoveInvalidXmlChars(string text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

#if NET3_5
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

#if !NET3_5
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

        /// <summary>
        /// Converts object value to invariant format, and strips any invalid xml-characters
        /// </summary>
        /// <param name="value">Object value</param>
        /// <returns>Object value converted to string</returns>
        internal static string XmlConvertToStringSafe(object value)
        {
            string valueString = XmlConvertToString(value);
            return RemoveInvalidXmlChars(valueString);
        }

        /// <summary>
        /// Converts object value to invariant format (understood by JavaScript)
        /// </summary>
        /// <param name="value">Object value</param>
        /// <returns>Object value converted to string</returns>
        internal static string XmlConvertToString(object value)
        {
            TypeCode objTypeCode = Convert.GetTypeCode(value);
            return XmlConvertToString(value, objTypeCode);
        }

        /// <summary>
        /// Converts object value to invariant format (understood by JavaScript)
        /// </summary>
        /// <param name="value">Object value</param>
        /// <param name="objTypeCode">Object TypeCode</param>
        /// <returns>Object value converted to string</returns>
        internal static string XmlConvertToString(object value, TypeCode objTypeCode)
        {
            if (value == null)
            {
                return "null";
            }

            switch (objTypeCode)
            {
                case TypeCode.Boolean:
                    return XmlConvert.ToString((Boolean)value);   // boolean as lowercase
                case TypeCode.Byte:
                    return XmlConvert.ToString((Byte)value);
                case TypeCode.SByte:
                    return XmlConvert.ToString((SByte)value);
                case TypeCode.Int16:
                    return XmlConvert.ToString((Int16)value);
                case TypeCode.Int32:
                    return XmlConvert.ToString((Int32)value);
                case TypeCode.Int64:
                    return XmlConvert.ToString((Int64)value);
                case TypeCode.UInt16:
                    return XmlConvert.ToString((UInt16)value);
                case TypeCode.UInt32:
                    return XmlConvert.ToString((UInt32)value);
                case TypeCode.UInt64:
                    return XmlConvert.ToString((UInt64)value);
                case TypeCode.Single:
                    {
                        float singleValue = (Single)value;
                        if (float.IsInfinity(singleValue))
                            return Convert.ToString(singleValue, System.Globalization.CultureInfo.InvariantCulture); // Infinity instead of INF
                        else
                            return XmlConvert.ToString(singleValue);    // 8 digits scale
                    }
                case TypeCode.Double:
                    {
                        double doubleValue = (Double)value;
                        if (double.IsInfinity(doubleValue))
                            return Convert.ToString(doubleValue, System.Globalization.CultureInfo.InvariantCulture); // Infinity instead of INF
                        else
                            return XmlConvert.ToString(doubleValue);    // 16 digits scale
                    }
                case TypeCode.Decimal:
                    return XmlConvert.ToString((Decimal)value);
                case TypeCode.DateTime:
                    return XmlConvert.ToString((DateTime)value, XmlDateTimeSerializationMode.Utc);
                case TypeCode.Char:
                    return XmlConvert.ToString((Char)value);
                case TypeCode.String:
                    return (string)value;
                default:
                    try
                    {
                        return Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture);
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
        /// <param name="thread"></param>
        /// <param name="localName"></param>
        public static void WriteAttributeSafeString(this XmlWriter writer, string thread, string localName)
        {
            writer.WriteAttributeString(RemoveInvalidXmlChars(thread), RemoveInvalidXmlChars(localName));
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
