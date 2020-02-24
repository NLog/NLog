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
    using System.Text;

    /// <summary>
    /// URL Encoding helper.
    /// </summary>
    internal static class UrlHelper
    {
        [Flags]
        public enum EscapeEncodingOptions
        {
            None = 0,
            /// <summary>Allow UnreservedMarks instead of ReservedMarks, as specified by chosen RFC</summary>
            UriString = 1,
            /// <summary>Use RFC2396 standard (instead of RFC3986)</summary>
            LegacyRfc2396 = 2,
            /// <summary>Should use lowercase when doing HEX escaping of special characters</summary>
            LowerCaseHex = 4,
            /// <summary>Replace space ' ' with '+' instead of '%20'</summary>
            SpaceAsPlus = 8,
            /// <summary>Skip UTF8 encoding, and prefix special characters with '%u'</summary>
            NLogLegacy = 16 | LegacyRfc2396 | LowerCaseHex | UriString,
        };

        /// <summary>
        /// Escape unicode string data for use in http-requests
        /// </summary>
        /// <param name="source">unicode string-data to be encoded</param>
        /// <param name="target">target for the encoded result</param>
        /// <param name="options"><see cref="EscapeEncodingOptions"/>s for how to perform the encoding</param>
        public static void EscapeDataEncode(string source, StringBuilder target, EscapeEncodingOptions options)
        {
            if (string.IsNullOrEmpty(source))
                return;

          
            bool isLowerCaseHex = Contains(options, EscapeEncodingOptions.LowerCaseHex);
            bool isSpaceAsPlus = Contains(options, EscapeEncodingOptions.SpaceAsPlus);
            bool isNLogLegacy = Contains(options, EscapeEncodingOptions.NLogLegacy);

            char[] charArray = null;
            byte[] byteArray = null;
            char[] hexChars = isLowerCaseHex ? hexLowerChars : hexUpperChars;

            for (int i = 0; i < source.Length; ++i)
            {
                char ch = source[i];
                target.Append(ch);
                if (IsSimpleCharOrNumber(ch))
                    continue;
               
                if (isSpaceAsPlus && ch == ' ')
                {
                    target[target.Length - 1] = '+';
                    continue;
                }

                if (IsAllowedChar(options, ch))
                {
                    continue;
                }

                if (isNLogLegacy)
                {
                    HandleLegacyEncoding(target, ch, hexChars);
                    continue;
                }

                if (charArray == null)
                    charArray = new char[1];
                charArray[0] = ch;

                if (byteArray == null)
                    byteArray = new byte[8];

                
                WriteWideChars(target, charArray, byteArray, hexChars);
            }
        }

        private static bool Contains(EscapeEncodingOptions options, EscapeEncodingOptions option)
        {
            return (options & option) == option;
        }

        /// <summary>
        /// Convert the wide-char into utf8-bytes, and then escape
        /// </summary>
        /// <param name="target"></param>
        /// <param name="charArray"></param>
        /// <param name="byteArray"></param>
        /// <param name="hexChars"></param>
        private static void WriteWideChars(StringBuilder target, char[] charArray, byte[] byteArray, char[] hexChars)
        {
            int byteCount = Encoding.UTF8.GetBytes(charArray, 0, 1, byteArray, 0);
            for (int j = 0; j < byteCount; ++j)
            {
                byte byteCh = byteArray[j];
                if (j == 0)
                    target[target.Length - 1] = '%';
                else
                    target.Append('%');
                target.Append(hexChars[(byteCh & 0xf0) >> 4]);
                target.Append(hexChars[byteCh & 0xf]);
            }
        }

        private static void HandleLegacyEncoding(StringBuilder target, char ch, char[] hexChars)
        {
            if (ch < 256)
            {
                target[target.Length - 1] = '%';
                target.Append(hexChars[(ch >> 4) & 0xF]);
                target.Append(hexChars[(ch >> 0) & 0xF]);
            }
            else
            {
                target[target.Length - 1] = '%';
                target.Append('u');
                target.Append(hexChars[(ch >> 12) & 0xF]);
                target.Append(hexChars[(ch >> 8) & 0xF]);
                target.Append(hexChars[(ch >> 4) & 0xF]);
                target.Append(hexChars[(ch >> 0) & 0xF]);
            }
        }

        /// <summary>
        /// Is allowed?
        /// </summary>
        /// <param name="options"></param>
        /// <param name="ch"></param>
        /// <returns></returns>
        private static bool IsAllowedChar(EscapeEncodingOptions options, char ch)
        {
            bool isUriString = (options & EscapeEncodingOptions.UriString) == EscapeEncodingOptions.UriString;
            bool isLegacyRfc2396 = (options & EscapeEncodingOptions.LegacyRfc2396) == EscapeEncodingOptions.LegacyRfc2396;
            if (isUriString)
            {
                if (!isLegacyRfc2396 && RFC3986UnreservedMarks.IndexOf(ch) >= 0)
                    return true;
                if (isLegacyRfc2396 && RFC2396UnreservedMarks.IndexOf(ch) >= 0)
                    return true;
            }
            else
            {
                if (!isLegacyRfc2396 && RFC3986ReservedMarks.IndexOf(ch) >= 0)
                    return true;
                if (isLegacyRfc2396 && RFC2396ReservedMarks.IndexOf(ch) >= 0)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Is a-z / A-Z / 0-9
        /// </summary>
        /// <param name="ch"></param>
        /// <returns></returns>
        private static bool IsSimpleCharOrNumber(char ch)
        {
            return ch >= 'a' && ch <= 'z' || ch >= 'A' && ch <= 'Z' || ch >= '0' && ch <= '9';
        }

        private const string RFC2396ReservedMarks = @";/?:@&=+$,";
        private const string RFC3986ReservedMarks = @":/?#[]@!$&'()*+,;=";
        private const string RFC2396UnreservedMarks = @"-_.!~*'()";
        private const string RFC3986UnreservedMarks = @"-._~";

        private static readonly char[] hexUpperChars =
            { '0', '1', '2', '3', '4', '5', '6', '7',
            '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };
        private static readonly char[] hexLowerChars =
            { '0', '1', '2', '3', '4', '5', '6', '7',
            '8', '9', 'a', 'b', 'c', 'd', 'e', 'f' };

        public static EscapeEncodingOptions GetUriStringEncodingFlags(bool escapeDataNLogLegacy, bool spaceAsPlus, bool escapeDataRfc3986)
        {
            EscapeEncodingOptions encodingOptions = EscapeEncodingOptions.UriString;
            if (escapeDataNLogLegacy)
                encodingOptions |= EscapeEncodingOptions.LowerCaseHex | EscapeEncodingOptions.NLogLegacy;
            else if (!escapeDataRfc3986)
                encodingOptions |= EscapeEncodingOptions.LowerCaseHex | EscapeEncodingOptions.LegacyRfc2396;
            if (spaceAsPlus)
                encodingOptions |= EscapeEncodingOptions.SpaceAsPlus;
            return encodingOptions;
        }
    }
}
