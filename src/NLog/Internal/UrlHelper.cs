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
    using System.Text;

    /// <summary>
    /// URL Encoding helper.
    /// </summary>
    internal class UrlHelper
    {
        private static string safeUrlPunctuation = ".()*-_!'";
        private static string hexChars = "0123456789abcdef";

        /// <summary>
        /// Url encode and URL
        /// </summary>
        /// <param name="str">URL to be encoded</param>
        /// <param name="spaceAsPlus">space as + or %20? <c>false</c> (%20) is the safe option.</param>
        /// <returns>Encoded url.</returns>
        internal static string UrlEncode(string str, bool spaceAsPlus)
        {
            StringBuilder result = new StringBuilder(str.Length + 20);
            for (int i = 0; i < str.Length; ++i)
            {
                char ch = str[i];

                if (ch == ' ' && spaceAsPlus)
                {
                    result.Append('+');
                }
                else if (IsSafeUrlCharacter(ch))
                {
                    result.Append(ch);
                }
                else if (ch < 256)
                {
                    result.Append('%');
                    result.Append(hexChars[(ch >> 4) & 0xF]);
                    result.Append(hexChars[(ch >> 0) & 0xF]);
                }
                else
                {
                    result.Append('%');
                    result.Append('u');
                    result.Append(hexChars[(ch >> 12) & 0xF]);
                    result.Append(hexChars[(ch >> 8) & 0xF]);
                    result.Append(hexChars[(ch >> 4) & 0xF]);
                    result.Append(hexChars[(ch >> 0) & 0xF]);
                }
            }

            return result.ToString();
        }

        /// <summary>
        /// Is this character safe in the URL?
        /// </summary>
        /// <param name="ch">char to test.</param>
        /// <returns><c>true</c> is safe.</returns>
        private static bool IsSafeUrlCharacter(char ch)
        {
            if (ch >= 'a' && ch <= 'z')
            {
                return true;
            }

            if (ch >= 'A' && ch <= 'Z')
            {
                return true;
            }

            if (ch >= '0' && ch <= '9')
            {
                return true;
            }

            return safeUrlPunctuation.IndexOf(ch) >= 0;
        }
    }
}
