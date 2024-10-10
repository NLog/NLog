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

namespace NLog.Targets.Internal
{
    using System;
    using System.Globalization;
    using System.Text;
    using System.Xml;

    internal static class XmlHelpers
    {
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

        public static string XmlConvertToStringSafe(object value)
        {
            try
            {
                var convertibleValue = value as IConvertible;
                var objTypeCode = convertibleValue?.GetTypeCode() ?? (value is null ? TypeCode.Empty : TypeCode.Object);
                if (objTypeCode != TypeCode.Object)
                {
                    return XmlConvertToString(convertibleValue, objTypeCode);
                }

                return XmlConvertToStringInvariant(value);
            }
            catch
            {
                return string.Empty;
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
            if (objTypeCode == TypeCode.Empty || value is null)
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
                    return XmlConvert.ToString(value.ToSingle(CultureInfo.InvariantCulture));
                case TypeCode.Double:
                    return XmlConvert.ToString(value.ToDouble(CultureInfo.InvariantCulture));
                case TypeCode.Decimal:
                    return XmlConvert.ToString(value.ToDecimal(CultureInfo.InvariantCulture));
                case TypeCode.DateTime:
                    return XmlConvert.ToString(value.ToDateTime(CultureInfo.InvariantCulture), XmlDateTimeSerializationMode.Utc);
                case TypeCode.Char:
                    return RemoveInvalidXmlChars(value.ToString(CultureInfo.InvariantCulture));
                case TypeCode.String:
                    return RemoveInvalidXmlChars(value.ToString(CultureInfo.InvariantCulture));
                default:
                    return XmlConvertToStringInvariant(value);
            }
        }

        private static string XmlConvertToStringInvariant(object value)
        {
            try
            {
                string valueString = Convert.ToString(value, CultureInfo.InvariantCulture);
                return RemoveInvalidXmlChars(valueString);
            }
            catch
            {
                return string.Empty;
            }
        }


        /// <summary>
        /// removes any unusual unicode characters that can't be encoded into XML
        /// </summary>
        public static string RemoveInvalidXmlChars(string text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            int length = text.Length;
            for (int i = 0; i < length; ++i)
            {
                char ch = text[i];
                if (!XmlConvert.IsXmlChar(ch))
                {
                    if (i + 1 < text.Length && XmlConvert.IsXmlSurrogatePair(text[i + 1], ch))
                    {
                        ++i;
                    }
                    else
                    {
                        return CreateValidXmlString(text);   // rare expensive case
                    }
                }
            }
            return text;
        }

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
    }
}
