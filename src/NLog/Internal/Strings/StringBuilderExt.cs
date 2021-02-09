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
using System.Globalization;
using System.IO;
using System.Text;
using NLog.MessageTemplates;

namespace NLog.Internal
{
    /// <summary>
    /// Helpers for <see cref="StringBuilder"/>, which is used in e.g. layout renderers.
    /// </summary>
    internal static class StringBuilderExt
    {
        /// <summary>
        /// Renders the specified log event context item and appends it to the specified <see cref="StringBuilder" />.
        /// </summary>
        /// <param name="builder">append to this</param>
        /// <param name="value">value to be appended</param>
        /// <param name="format">format string. If @, then serialize the value with the Default JsonConverter.</param>
        /// <param name="formatProvider">provider, for example culture</param>
        /// <param name="valueFormatter">NLog string.Format interface</param>
        public static void AppendFormattedValue(this StringBuilder builder, object value, string format, IFormatProvider formatProvider, IValueFormatter valueFormatter)
        {
            string stringValue = value as string;
            if (stringValue != null && string.IsNullOrEmpty(format))
            {
                builder.Append(value);  // Avoid automatic quotes
            }
            else if (format == MessageTemplates.ValueFormatter.FormatAsJson)
            {
                valueFormatter.FormatValue(value, null, CaptureType.Serialize, formatProvider, builder);
            }
            else if (value != null)
            {
                valueFormatter.FormatValue(value, format, CaptureType.Normal, formatProvider, builder);
            }
        }

        /// <summary>
        /// Appends int without using culture, and most importantly without garbage
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="value">value to append</param>
        public static void AppendInvariant(this StringBuilder builder, int value)
        {
            // Deal with negative numbers
            if (value < 0)
            {
                builder.Append('-');
                uint uint_value = uint.MaxValue - ((uint)value) + 1; //< This is to deal with Int32.MinValue
                AppendInvariant(builder, uint_value);
            }
            else
            {
                AppendInvariant(builder, (uint)value);
            }
        }

        /// <summary>
        /// Appends uint without using culture, and most importantly without garbage
        /// 
        /// Credits Gavin Pugh  - https://www.gavpugh.com/2010/04/01/xnac-avoiding-garbage-when-working-with-stringbuilder/
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="value">value to append</param>
        public static void AppendInvariant(this StringBuilder builder, uint value)
        {
            if (value == 0)
            {
                builder.Append('0');
                return;
            }

            int digitCount = CalculateDigitCount(value);
            ApppendValueWithDigitCount(builder, value, digitCount);
        }

        private static int CalculateDigitCount(uint value)
        {
            // Calculate length of integer when written out
            int length = 0;
            uint length_calc = value;

            do
            {
                length_calc /= 10;
                length++;
            }
            while (length_calc > 0);
            return length;
        }

        private static void ApppendValueWithDigitCount(StringBuilder builder, uint value, int digitCount)
        {
            // Pad out space for writing.
            builder.Append('0', digitCount);

            int strpos = builder.Length;

            // We're writing backwards, one character at a time.
            while (digitCount > 0)
            {
                strpos--;

                // Lookup from static char array, to cover hex values too
                builder[strpos] = charToInt[value % 10];

                value /= 10;
                digitCount--;
            }
        }

        private static readonly char[] charToInt = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };

        /// <summary>
        /// Convert DateTime into UTC and format to yyyy-MM-ddTHH:mm:ss.fffffffZ - ISO 6801 date (Round-Trip-Time)
        /// </summary>
        public static void AppendXmlDateTimeRoundTrip(this StringBuilder builder, DateTime dateTime)
        {
            if (dateTime.Kind == DateTimeKind.Unspecified)
                dateTime = new DateTime(dateTime.Ticks, DateTimeKind.Utc);
            else
                dateTime = dateTime.ToUniversalTime();
            builder.Append4DigitsZeroPadded(dateTime.Year);
            builder.Append('-');
            builder.Append2DigitsZeroPadded(dateTime.Month);
            builder.Append('-');
            builder.Append2DigitsZeroPadded(dateTime.Day);
            builder.Append('T');
            builder.Append2DigitsZeroPadded(dateTime.Hour);
            builder.Append(':');
            builder.Append2DigitsZeroPadded(dateTime.Minute);
            builder.Append(':');
            builder.Append2DigitsZeroPadded(dateTime.Second);
            int fraction = (int)(dateTime.Ticks % 10000000);
            if (fraction > 0)
            {
                builder.Append('.');

                // Remove trailing zeros
                int max_digit_count = 7;
                while (fraction % 10 == 0)
                {
                    --max_digit_count;
                    fraction /= 10;
                }

                // Append the remaining fraction
                int digitCount = CalculateDigitCount((uint)fraction);
                if (max_digit_count > digitCount)
                    builder.Append('0', max_digit_count - digitCount);
                ApppendValueWithDigitCount(builder, (uint)fraction, digitCount);
            }
            builder.Append('Z');
        }

        /// <summary>
        /// Clears the provider StringBuilder
        /// </summary>
        /// <param name="builder"></param>
        public static void ClearBuilder(this StringBuilder builder)
        {
            try
            {
#if !NET35
                builder.Clear();
#else
                builder.Length = 0;
#endif
            }
            catch
            {
                // Default StringBuilder Clear() can cause the StringBuilder to re-allocate new internal char-array
                // This can fail in low memory conditions when StringBuilder is big, so instead try to clear the StringBuilder "gently"
                if (builder.Length > 1)
                    builder.Remove(0, builder.Length - 1);
                builder.Remove(0, builder.Length);
            }
        }

        /// <summary>
        /// Copies the contents of the StringBuilder to the MemoryStream using the specified encoding (Without BOM/Preamble)
        /// </summary>
        /// <param name="builder">StringBuilder source</param>
        /// <param name="ms">MemoryStream destination</param>
        /// <param name="encoding">Encoding used for converter string into byte-stream</param>
        /// <param name="transformBuffer">Helper char-buffer to minimize memory allocations</param>
        public static void CopyToStream(this StringBuilder builder, MemoryStream ms, Encoding encoding, char[] transformBuffer)
        {
            int charCount;
            int byteCount = encoding.GetMaxByteCount(builder.Length);
            ms.SetLength(ms.Position + byteCount);
            for (int i = 0; i < builder.Length; i += transformBuffer.Length)
            {
                charCount = Math.Min(builder.Length - i, transformBuffer.Length);
                builder.CopyTo(i, transformBuffer, 0, charCount);
                byteCount = encoding.GetBytes(transformBuffer, 0, charCount, ms.GetBuffer(), (int)ms.Position);
                ms.Position += byteCount;
            }
            if (ms.Position != ms.Length)
            {
                ms.SetLength(ms.Position);
            }
        }

        public static void CopyToBuffer(this StringBuilder builder, char[] destination, int destinationIndex)
        {
            builder.CopyTo(0, destination, destinationIndex, builder.Length);
        }

        /// <summary>
        /// Copies the contents of the StringBuilder to the destination StringBuilder
        /// </summary>
        /// <param name="builder">StringBuilder source</param>
        /// <param name="destination">StringBuilder destination</param>
        public static void CopyTo(this StringBuilder builder, StringBuilder destination)
        {
            int sourceLength = builder.Length;
            if (sourceLength > 0)
            {
                destination.EnsureCapacity(sourceLength + destination.Length);
                if (sourceLength < 8)
                {
                    // Skip char-buffer allocation for small strings
                    for (int i = 0; i < sourceLength; ++i)
                        destination.Append(builder[i]);
                }
                else if (sourceLength < 512)
                {
                    // Single char-buffer allocation through string-object
                    destination.Append(builder.ToString());
                }
                else
                {
                    // Reuse single char-buffer allocation for large StringBuilders
                    char[] buffer = new char[256];
                    for (int i = 0; i < sourceLength; i += buffer.Length)
                    {
                        int charCount = Math.Min(sourceLength - i, buffer.Length);
                        builder.CopyTo(i, buffer, 0, charCount);
                        destination.Append(buffer, 0, charCount);
                    }
                }
            }
        }

        /// <summary>
        /// Scans the StringBuilder for the position of needle character
        /// </summary>
        /// <param name="builder">StringBuilder source</param>
        /// <param name="needle">needle character to search for</param>
        /// <param name="startPos"></param>
        /// <returns>Index of the first occurrence (Else -1)</returns>
        public static int IndexOf(this StringBuilder builder, char needle, int startPos = 0)
        {
            for (int i = startPos; i < builder.Length; ++i)
                if (builder[i] == needle)
                    return i;
            return -1;
        }

        /// <summary>
        /// Scans the StringBuilder for the position of needle character
        /// </summary>
        /// <param name="builder">StringBuilder source</param>
        /// <param name="needles">needle characters to search for</param>
        /// <param name="startPos"></param>
        /// <returns>Index of the first occurrence (Else -1)</returns>
        public static int IndexOfAny(this StringBuilder builder, char[] needles, int startPos = 0)
        {
            for (int i = startPos; i < builder.Length; ++i)
            {
                if (CharArrayContains(builder[i], needles))
                    return i;
            }
            return -1;
        }

        private static bool CharArrayContains(char searchChar, char[] needles)
        {
            for (int i = 0; i < needles.Length; ++i)
            {
                if (needles[i] == searchChar)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Compares the contents of two StringBuilders
        /// </summary>
        /// <remarks>
        /// Correct implementation of <see cref="StringBuilder.Equals(StringBuilder)" /> that also works when <see cref="StringBuilder.Capacity"/> is not the same
        /// </remarks>
        /// <returns>True when content is the same</returns>
        public static bool EqualTo(this StringBuilder builder, StringBuilder other)
        {
            if (builder.Length != other.Length)
                return false;

            for (int x = 0; x < builder.Length; ++x)
            {
                if (builder[x] != other[x])
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Compares the contents of a StringBuilder and a String
        /// </summary>
        /// <returns>True when content is the same</returns>
        public static bool EqualTo(this StringBuilder builder, string other)
        {
            if (builder.Length != other.Length)
                return false;

            for (int i = 0; i < other.Length; ++i)
            {
                if (builder[i] != other[i])
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Append a number and pad with 0 to 2 digits
        /// </summary>
        /// <param name="builder">append to this</param>
        /// <param name="number">the number</param>
        internal static void Append2DigitsZeroPadded(this StringBuilder builder, int number)
        {
            builder.Append((char)((number / 10) + '0'));
            builder.Append((char)((number % 10) + '0'));
        }

        /// <summary>
        /// Append a number and pad with 0 to 4 digits
        /// </summary>
        /// <param name="builder">append to this</param>
        /// <param name="number">the number</param>
        internal static void Append4DigitsZeroPadded(this StringBuilder builder, int number)
        {
            builder.Append((char)(((number / 1000) % 10) + '0'));
            builder.Append((char)(((number / 100) % 10) + '0'));
            builder.Append((char)(((number / 10) % 10) + '0'));
            builder.Append((char)(((number / 1) % 10) + '0'));
        }

        /// <summary>
        /// Append a numeric type (byte, int, double, decimal) as string
        /// </summary>
        internal static void AppendNumericInvariant(this StringBuilder sb, IConvertible value, TypeCode objTypeCode)
        {
            switch (objTypeCode)
            {
                case TypeCode.Byte: sb.AppendInvariant(value.ToByte(CultureInfo.InvariantCulture)); break;
                case TypeCode.SByte: sb.AppendInvariant(value.ToSByte(CultureInfo.InvariantCulture)); break;
                case TypeCode.Int16: sb.AppendInvariant(value.ToInt16(CultureInfo.InvariantCulture)); break;
                case TypeCode.Int32: sb.AppendInvariant(value.ToInt32(CultureInfo.InvariantCulture)); break;
                case TypeCode.Int64:
                    {
                        long int64 = value.ToInt64(CultureInfo.InvariantCulture);
                        if (int64 < int.MaxValue && int64 > int.MinValue)
                            sb.AppendInvariant((int)int64);
                        else
                            sb.Append(int64);
                    }
                    break;
                case TypeCode.UInt16: sb.AppendInvariant(value.ToUInt16(CultureInfo.InvariantCulture)); break;
                case TypeCode.UInt32: sb.AppendInvariant(value.ToUInt32(CultureInfo.InvariantCulture)); break;
                case TypeCode.UInt64:
                    {
                        ulong uint64 = value.ToUInt64(CultureInfo.InvariantCulture);
                        if (uint64 < uint.MaxValue)
                            sb.AppendInvariant((uint)uint64);
                        else
                            sb.Append(uint64);
                    }
                    break;
                case TypeCode.Single:
                    {
                        float floatValue = value.ToSingle(CultureInfo.InvariantCulture);
                        AppendFloatInvariant(sb, floatValue);
                    }
                    break;
                case TypeCode.Double:
                    {
                        double doubleValue = value.ToDouble(CultureInfo.InvariantCulture);
                        AppendDoubleInvariant(sb, doubleValue);
                    }
                    break;
                case TypeCode.Decimal:
                    {
                        decimal decimalValue = value.ToDecimal(CultureInfo.InvariantCulture);
                        AppendDecimalInvariant(sb, decimalValue);
                    }
                    break;
                default:
                    sb.Append(XmlHelper.XmlConvertToString(value, objTypeCode));
                    break;
            }
        }

        private static void AppendDecimalInvariant(StringBuilder sb, decimal decimalValue)
        {
            if (Math.Truncate(decimalValue) == decimalValue && decimalValue > int.MinValue && decimalValue < int.MaxValue)
            {
                sb.AppendInvariant(Convert.ToInt32(decimalValue));
                sb.Append(".0");
            }
            else
            {
                sb.Append(XmlHelper.XmlConvertToString(decimalValue));
            }
        }

        private static void AppendDoubleInvariant(StringBuilder sb, double doubleValue)
        {
            if (double.IsNaN(doubleValue) || double.IsInfinity(doubleValue))
            {
                sb.Append(XmlHelper.XmlConvertToString(doubleValue));
            }
            else if (Math.Truncate(doubleValue) == doubleValue && doubleValue > int.MinValue && doubleValue < int.MaxValue)
            {
                sb.AppendInvariant(Convert.ToInt32(doubleValue));
                sb.Append(".0");
            }
            else
            {
                sb.Append(XmlHelper.XmlConvertToString(doubleValue));
            }
        }

        private static void AppendFloatInvariant(StringBuilder sb, float floatValue)
        {
            if (float.IsNaN(floatValue) || float.IsInfinity(floatValue))
            {
                sb.Append(XmlHelper.XmlConvertToString(floatValue));
            }
            else if (Math.Truncate(floatValue) == floatValue && floatValue > int.MinValue && floatValue < int.MaxValue)
            {
                sb.AppendInvariant(Convert.ToInt32(floatValue));
                sb.Append(".0");
            }
            else
            {
                sb.Append(XmlHelper.XmlConvertToString(floatValue));
            }
        }

        public static void TrimRight(this StringBuilder sb, int startPos = 0)
        {
            int i = sb.Length - 1;
            for (; i >= startPos; i--)
                if (!char.IsWhiteSpace(sb[i]))
                    break;

            if (i < sb.Length - 1)
                sb.Length = i + 1;
        }
    }
}
