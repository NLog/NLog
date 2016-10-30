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

    /// <summary>
    /// URL Encoding helper.
    /// </summary>
    internal static class UrlHelper
    {
        private static readonly char[] hexUpperChars =
            { '0', '1', '2', '3', '4', '5', '6', '7',
            '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };
        private static readonly char[] hexLowerChars =
            { '0', '1', '2', '3', '4', '5', '6', '7',
            '8', '9', 'a', 'b', 'c', 'd', 'e', 'f' };

        [Flags]
        public enum EscapeEncodingFlag
        {
            /// <summary>Use UnreservedMarks instead of ReservedMarks, as specified by chosen RFC</summary>
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

        private const string RFC2396ReservedMarks = @";/?:@&=+$,";
        private const string RFC3986ReservedMarks = @":/?#[]@!$&'()*+,;=";
        private const string RFC2396UnreservedMarks = @"-_.!~*'()";
        private const string RFC3986UnreservedMarks = @"-._~";

        /// <summary>
        /// Url encode an URL
        /// </summary>
        /// <param name="str">URL to be encoded</param>
        /// <param name="spaceAsPlus">space as + or %20? <c>false</c> (%20) is the safe option.</param>
        /// <returns>Encoded url.</returns>
        public static string UrlEncode(string str, bool spaceAsPlus)
        {
            if (string.IsNullOrEmpty(str))
                return string.Empty;

            StringBuilder result = new StringBuilder(str.Length + 20);
            result.Append(str);
            EscapeEncodingFlag escapeFlags = EscapeEncodingFlag.NLogLegacy;
            if (spaceAsPlus)
                escapeFlags |= EscapeEncodingFlag.SpaceAsPlus;
            EscapeDataEncode(result, escapeFlags);
            return result.ToString();
        }

        /// <summary>
        /// Escape unicode string data for use in http-requests
        /// </summary>
        /// <param name="target">unicode string-data to be encoded</param>
        /// <param name="flags"><see cref="EscapeEncodingFlag"/>s for how to perform the encoding</param>
        public static void EscapeDataEncode(StringBuilder target, EscapeEncodingFlag flags)
        {
            if (target.Length == 0)
                return;

            bool isUriString = ((int)flags & (int)EscapeEncodingFlag.UriString) == (int)EscapeEncodingFlag.UriString;
            bool isLegacyRfc2396 = ((int)flags & (int)EscapeEncodingFlag.LegacyRfc2396) == (int)EscapeEncodingFlag.LegacyRfc2396;
            bool isLowerCaseHex = ((int)flags & (int)EscapeEncodingFlag.LowerCaseHex) == (int)EscapeEncodingFlag.LowerCaseHex;
            bool isSpaceAsPlus = ((int)flags & (int)EscapeEncodingFlag.SpaceAsPlus) == (int)EscapeEncodingFlag.SpaceAsPlus;
            bool isNLogLegacy = ((int)flags & (int)EscapeEncodingFlag.NLogLegacy) == (int)EscapeEncodingFlag.NLogLegacy;

            char[] charArray = null;
            byte[] byteArray = null;
            char[] hexChars = isLowerCaseHex ? hexLowerChars : hexUpperChars;

            for (int i = 0; i < target.Length; ++i)
            {
                char ch = target[i];
                if (ch >= 'a' && ch <= 'z')
                    continue;
                if (ch >= 'A' && ch <= 'Z')
                    continue;
                if (ch >= '0' && ch <= '9')
                    continue;
                if (isSpaceAsPlus && ch == ' ')
                {
                    target[i] = '+';
                    continue;
                }

                if (isUriString)
                {
                    if (!isLegacyRfc2396 && RFC3986UnreservedMarks.IndexOf(ch) >= 0)
                        continue;
                    if (isLegacyRfc2396 && RFC2396UnreservedMarks.IndexOf(ch) >= 0)
                        continue;
                }
                else
                {
                    if (!isLegacyRfc2396 && RFC3986ReservedMarks.IndexOf(ch) >= 0)
                        continue;
                    if (isLegacyRfc2396 && RFC2396ReservedMarks.IndexOf(ch) >= 0)
                        continue;
                }

                if (isNLogLegacy)
                {
                    if (ch > 255)
                    {
                        target.Insert(i, "%", 5);
                        target[i] = '%';
                        target[++i] = 'u';
                        target[++i] = hexChars[(ch >> 12) & 0xF];
                        target[++i] = hexChars[(ch >> 8) & 0xF];
                        target[++i] = hexChars[(ch >> 4) & 0xF];
                        target[++i] = hexChars[(ch >> 0) & 0xF];
                    }
                    else
                    {
                        target.Insert(i, "%", 2);
                        target[i] = '%';
                        target[++i] = hexChars[(ch >> 4) & 0xF];
                        target[++i] = hexChars[(ch >> 0) & 0xF];
                    }
                    continue;
                }

                if (charArray == null)
                    charArray = new char[1];
                charArray[0] = ch;

                if (byteArray == null)
                    byteArray = new byte[8];

                // Convert the wide-char into utf8-bytes, and then escape
                int byteCount = Encoding.UTF8.GetBytes(charArray, 0, 1, byteArray, 0);
                target.Insert(i, "%", byteCount * 3 - 1);
                --i;
                for (int j = 0; j < byteCount; ++j)
                {
                    byte byteCh = byteArray[j];
                    ++i;
                    target[++i] = hexChars[(byteCh & 0xf0) >> 4];
                    target[++i] = hexChars[byteCh & 0xf];
                }
            }
        }
    }
}
