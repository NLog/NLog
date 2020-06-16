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
using System.Collections.Generic;
using System.Text;

namespace NLog.Internal
{
    /// <summary>
    /// Split a string
    /// </summary>
    internal static class StringSplitter
    {
        /// <summary>
        /// Split a string, optional quoted value
        /// </summary>
        /// <param name="text">Text to split</param>
        /// <param name="splitChar">Character to split the <paramref name="text" /></param>
        /// <param name="quoteChar">Quote character</param>
        /// <param name="escapeChar">
        /// Escape for the <paramref name="quoteChar" />, not escape for the <paramref name="splitChar" />
        /// , use quotes for that.
        /// </param>
        public static IEnumerable<string> SplitQuoted(this string text, char splitChar, char quoteChar, char escapeChar)
        {
            if (!string.IsNullOrEmpty(text))
            {
                if (splitChar == quoteChar)
                {
                    throw new NotSupportedException("Quote character should different from split character");
                }

                if (splitChar == escapeChar)
                {
                    throw new NotSupportedException("Escape character should different from split character");
                }

                return SplitQuoted2(text, splitChar, quoteChar, escapeChar);
            }

            return ArrayHelper.Empty<string>();
        }

        /// <summary>
        /// Split a string, optional quoted value
        /// </summary>
        /// <param name="text">Text to split</param>
        /// <param name="splitChar">Character to split the <paramref name="text" /></param>
        /// <param name="quoteChar">Quote character</param>
        /// <param name="escapeChar">
        /// Escape for the <paramref name="quoteChar" />, not escape for the <paramref name="splitChar" />
        /// , use quotes for that.
        /// </param>
        private static IEnumerable<string> SplitQuoted2(string text, char splitChar, char quoteChar, char escapeChar)
        {
            bool inQuotedMode = false;
            bool prevEscape = false;
            bool prevQuote = false;
            bool doubleQuotesEscapes = escapeChar == quoteChar;  // Special mode

            var item = new StringBuilder();

            foreach (var c in text)
            {
                if (c == quoteChar)
                {
                    if (inQuotedMode)
                    {
                        if (prevEscape && !doubleQuotesEscapes)
                        {
                            item.Append(c); // Escaped quote-char in quoted-mode
                            prevEscape = false;
                            prevQuote = false;
                        }
                        else if (prevQuote && doubleQuotesEscapes)
                        {
                            // Double quote, means escaped quote, quoted-mode not real
                            item.Append(c);
                            inQuotedMode = false;
                            prevEscape = false;
                            prevQuote = false;
                        }
                        else if (item.Length > 0)
                        {
                            // quoted-mode ended with something to yield
                            inQuotedMode = false;
                            yield return item.ToString();
                            item.Length = 0;    // Start new item
                            prevEscape = false;
                            prevQuote = true;   // signal that item is empty, because it has just been yielded after quoted-mode
                        }
                        else
                        {
                            // quoted-mode ended without anything to yield
                            inQuotedMode = false;
                            prevEscape = false;
                            prevQuote = false;
                        }                       
                    }
                    else
                    {
                        if (item.Length != 0 || prevEscape)
                        {
                            // Quoted-mode can only be activated initially
                            item.Append(c);
                            prevEscape = false;
                            prevQuote = false;
                        }
                        else
                        {
                            // Quoted-mode is now activated
                            prevEscape = c == escapeChar;
                            prevQuote = true;
                            inQuotedMode = true;
                        }
                    }
                }
                else if (c == escapeChar)
                {
                    if (prevEscape)
                        item.Append(escapeChar);     // Escape-chars are only stripped in quoted-mode when placed before quote

                    prevEscape = true;
                    prevQuote = false;
                }
                else if (inQuotedMode)
                {
                    item.Append(c);
                    prevEscape = false;
                    prevQuote = false;
                }
                else if (c == splitChar)
                {
                    if (prevEscape)
                        item.Append(escapeChar);    // Escape-chars are only stripped in quoted-mode when placed before quote

                    if (item.Length > 0 || !prevQuote)
                    {
                        yield return item.ToString();
                        item.Length = 0;   // Start new item
                    }

                    prevEscape = false;
                    prevQuote = false;
                }
                else
                {
                    if (prevEscape)
                        item.Append(escapeChar);    // Escape-chars are only stripped in quoted-mode when placed before quote

                    item.Append(c);
                    prevEscape = false;
                    prevQuote = false;
                }
            }

            if (prevEscape && !doubleQuotesEscapes)
                item.Append(escapeChar);   // incomplete escape-sequence, means escape should be included

            if (inQuotedMode)
            {
                // incomplete quoted-mode, means quotes should be included
                if (prevQuote)
                {
                    item.Append(quoteChar);
                }
                else
                {
                    item.Insert(0, quoteChar);
                }
            }

            if (item.Length > 0 || !prevQuote)
                yield return item.ToString();
        }
    }
}