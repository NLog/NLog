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
    using System.Collections.Generic;
    using System.Text;

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
            if (splitChar == quoteChar)
                throw new NotSupportedException("Quote character should be different from split character");

            if (splitChar == escapeChar)
                throw new NotSupportedException("Escape character should be different from split character");

            return string.IsNullOrEmpty(text)
                ? ArrayHelper.Empty<string>()
                : EnumerateSplitQuoted(text, splitChar, quoteChar, escapeChar);
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
        private static IEnumerable<string> EnumerateSplitQuoted(string text, char splitChar, char quoteChar, char escapeChar)
        {
            var item = new StringBuilder();
            bool inQuotedMode = false;
            bool skipNext = false;
            bool doubleQuotesEscapes = escapeChar == quoteChar;  // Special mode

            for (int i = 0; i < text.Length; i++)
            {
                if (skipNext)
                {
                    skipNext = false;
                    continue;
                }

                char c = text[i];
                if (c == quoteChar)
                {
                    if (doubleQuotesEscapes && i + 1 < text.Length && text[i + 1] == quoteChar)
                    {
                        if (inQuotedMode || item.Length == 0)
                        {
                            item.Append(quoteChar);
                            skipNext = true;
                            continue;
                        }
                    }

                    if (inQuotedMode)
                    {
                        if (i + 1 < text.Length && text[i + 1] != splitChar)
                        {
                            // Special legacy behavior of yielding multiple items, when dangling characters after closing quote
                            yield return item.ToString();
                            item.ClearBuilder();
                        }
                        inQuotedMode = false;
                        continue;
                    }

                    if (item.Length == 0)
                    {
                        // Quoted-mode can only be activated initially
                        inQuotedMode = true;
                        continue;
                    }

                    item.Append(c);
                    continue;
                }

                if (c == escapeChar)
                {
                    if (i + 1 < text.Length && text[i + 1] == quoteChar)
                    {
                        item.Append(quoteChar);
                        skipNext = true;
                    }
                    else if (!inQuotedMode || i + 1 >= text.Length)
                    {
                        // Escape-chars are kept when not in quoted-mode (or nothing to escape)
                        item.Append(c);
                    }
                    else if (i + 1 < text.Length && text[i + 1] == escapeChar)
                    {
                        // Escape-chars can escape themselves in quoted-mode
                        item.Append(escapeChar);
                        skipNext = true;
                    }
                    continue;
                }

                if (c == splitChar && !inQuotedMode)
                {
                    yield return item.ToString();
                    item.ClearBuilder();
                }
                else
                {
                    item.Append(c);
                }
            }

            if (inQuotedMode)
            {
                if (item.Length > 0 && item[item.Length - 1] == quoteChar)
                {
                    // incomplete quoted-mode, where ending quote was escaped, so replace with escape-char
                    item[item.Length - 1] = escapeChar;
                }
                else
                {
                    // incomplete quoted-mode, means starting quotes should be included
                    item.Insert(0, quoteChar);
                }
            }

            yield return item.ToString();
        }
    }
}
