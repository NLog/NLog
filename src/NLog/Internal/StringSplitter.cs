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

#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#endregion

namespace NLog.Internal
{
    /// <summary>
    /// Split a string
    /// </summary>
    internal static class StringSplitter
    {
        /// <summary>
        /// Split string with escape. The escape char is the same as the splitchar
        /// </summary>
        /// <param name="text"></param>
        /// <param name="splitChar">split char. escaped also with this char</param>
        /// <returns></returns>
        public static IEnumerable<string> SplitWithSelfEscape(this string text, char splitChar)
        {
            return SplitWithSelfEscape2(text, splitChar);
        }

        /// <summary>
        /// Split string with escape
        /// </summary>
        /// <param name="text"></param>
        /// <param name="splitChar"></param>
        /// <param name="escapeChar"></param>
        /// <returns></returns>
        public static IEnumerable<string> SplitWithEscape(this string text, char splitChar, char escapeChar)
        {
            if (splitChar == escapeChar)
            {
                return SplitWithSelfEscape2(text, splitChar);
            }

            return SplitWithEscape2(text, splitChar, escapeChar);
        }


        private static IEnumerable<string> SplitWithEscape2(string text, char splitChar, char escapeChar)
        {
            if (!string.IsNullOrEmpty(text))
            {
                var prevWasEscape = false;
                int i;
                var sb = new StringBuilder();
                for (i = 0; i < text.Length; i++)
                {
                    var c = text[i];

                    //prev not escaped, then check splitchar
                    var isSplitChar = c == splitChar;
                    if (prevWasEscape)
                    {
                        if (isSplitChar)
                        {
                            //overwrite escapechar
                            if (sb.Length > 0)
                                sb.Length--;
                            sb.Append(c);
                            //if splitchar ==escapechar, then in this case it's used as split
                            prevWasEscape = false;
                        }
                        else
                        {
                            sb.Append(c);
                            prevWasEscape = c == escapeChar;
                        }
                    }
                    else
                    {
                        if (isSplitChar)
                        {
                            var part = sb.ToString();
                            //reset
                            sb.Length = 0;
                            yield return part;
                            if (text.Length - 1 == i)
                            {
                                //done
                                yield return string.Empty;
                                break;
                            }
                        }
                        else
                        {
                            sb.Append(c);
                            prevWasEscape = c == escapeChar;
                        }
                    }
                }
                var lastPart = GetLastPart(sb);
                if (lastPart != null)
                {
                    yield return lastPart;
                }
            }
        }

        private static IEnumerable<string> SplitWithSelfEscape2(string text, char splitAndEscapeChar)
        {
            if (!string.IsNullOrEmpty(text))
            {
                var prevWasEscape = false;
                int i;
                var sb = new StringBuilder();
                //if same, handle different
                for (i = 0; i < text.Length; i++)
                {
                    var c = text[i];
                    var isSplitAndEscape = c == splitAndEscapeChar;
                    var isLastChar = i == text.Length - 1;
                    if (prevWasEscape)
                    {
                        if (isSplitAndEscape)
                        {
                            prevWasEscape = false;
                        }
                        else
                        {
                            //if prevWasEscape, always appended so length >0
                            //if (sb.Length > 0) 
                            sb.Length--;
                            var part = sb.ToString();
                            //reset
                            sb.Length = 0;
                            prevWasEscape = false;
                            sb.Append(c);
                            yield return part;
                        }
                    }
                    else
                    {
                        if (isLastChar && isSplitAndEscape)
                        {
                            var part = sb.ToString();
                            sb.Length = 0;
                            yield return part;
                            yield return string.Empty;
                        }
                        else
                        {
                            sb.Append(c);
                            if (isSplitAndEscape)
                            {
                                prevWasEscape = true;
                            }
                        }
                    }
                }
                var lastPart = GetLastPart(sb);
                if (lastPart != null)
                {
                    yield return lastPart;
                }
            }
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
        /// <returns></returns>
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

                if (quoteChar == escapeChar)
                {
                    return SplitSelfQuoted2(text, splitChar, quoteChar);
                }


                return SplitQuoted2(text, splitChar, quoteChar, escapeChar);
            }
            return new List<string>();
        }

        private static IEnumerable<string> SplitSelfQuoted2(string text, char splitChar, char quoteAndEscapeChar)
        {
            var inQuotedMode = false;
            int i;
            var sb = new StringBuilder();
            var isNewPart = true;

            for (i = 0; i < text.Length; i++)
            {
                var c = text[i];

                //prev not escaped, then check splitchar
                var isSplitChar = c == splitChar;
                var isQuoteAndEscapeChar = c == quoteAndEscapeChar;
                var isLastChar = i == text.Length - 1;

                if (isNewPart)
                {
                    //now only quote for quotemode accepted
                    isNewPart = false;
                    isQuoteAndEscapeChar = c == quoteAndEscapeChar;

                    if (isQuoteAndEscapeChar)
                    {
                        //escape of the quote, if the quote is after this.
                        if (isLastChar)
                        {
                            //done
                            sb.Append(c);
                            break;
                        }
                        i++;
                        c = text[i];

                        if (c == quoteAndEscapeChar)
                        {
                            sb.Append(quoteAndEscapeChar);
                        }
                        else
                        {
                            sb.Append(c);
                            inQuotedMode = true;
                        }
                    }
                    else if (isSplitChar)
                    {
                        //end of part

                        var part = sb.ToString();
                        //reset
                        sb.Length = 0;
                        //  isInPart = false;
                        yield return part;

                        if (isLastChar)
                        {
                            //done
                            yield return string.Empty;
                            break;
                        }

                        isNewPart = true;
                    }
                    else
                    {
                        sb.Append(c);
                    }
                }

                else if (inQuotedMode)
                {
                    if (isQuoteAndEscapeChar)
                    {
                        //skip escapechar
                        i++;
                        //    isInPart = false;
                        inQuotedMode = false;
                        var part = sb.ToString();
                        //reset
                        sb.Length = 0;
                        yield return part;
                    }
                    else
                    {
                        sb.Append(c);
                    }
                }
                else
                {
                    if (isSplitChar)
                    {
                        //end of part

                        var part = sb.ToString();
                        //reset
                        sb.Length = 0;
                        //  isInPart = false;
                        yield return part;

                        if (isLastChar)
                        {
                            //done
                            yield return string.Empty;
                            break;
                        }

                        isNewPart = true;
                    }
                    else
                    {
                        sb.Append(c);
                    }
                }
            }

            var lastPart = GetLastPart(sb);
            if (inQuotedMode)
            {
                //append quote back
                lastPart = quoteAndEscapeChar + lastPart;
            }

            if (lastPart != null)
            {
                yield return lastPart;
            }
        }

        private static IEnumerable<string> SplitQuoted2(string text, char splitChar, char quoteChar, char escapeChar)
        {
            var inQuotedMode = false;
            int i;
            var sb = new StringBuilder();
            var isNewPart = true;

            var prevIsEscape = false;
            for (i = 0; i < text.Length; i++)
            {
                var c = text[i];

                //prev not escaped, then check splitchar
                var isSplitChar = c == splitChar;
                var isQuoteChar = c == quoteChar;
                var isEscapeChar = c == escapeChar;
                var isLastChar = i == text.Length - 1;


                if (isNewPart)
                {
                    isNewPart = false;
                    isQuoteChar = c == quoteChar;
                    isEscapeChar = c == escapeChar;

                    if (isEscapeChar)
                    {
                        //escape of the quote, if the quote is after this.

                        if (isLastChar)
                        {
                            //done
                            sb.Append(c);
                            break;
                        }

                        i++;

                        c = text[i];
                        if (c == quoteChar)
                        {
                            sb.Append(quoteChar);
                        }
                        else
                        {
                            sb.Append(escapeChar);
                            sb.Append(c);
                        }
                    }
                    else if (isSplitChar)
                    {
                        //end of part

                        var part = sb.ToString();
                        //reset
                        sb.Length = 0;
                        yield return part;

                        if (isLastChar)
                        {
                            //done
                            yield return string.Empty;
                            break;
                        }

                        isNewPart = true;
                    }

                    else if (isQuoteChar)
                    {
                        //skip quoteChar
                        if (sb.Length > 0)
                            sb.Length--;
                        //isInPart = true;
                        inQuotedMode = true;
                        //todo check escape quoteChar
                    }
                    else
                    {
                        sb.Append(c);
                    }
                }

                else if (inQuotedMode)
                {
                    if (isQuoteChar)
                    {
                        if (prevIsEscape)

                        {
                            //skip escapechar

                            if (sb.Length > 0)
                                sb.Length--;

                            //todo skip escape 
                            sb.Append(c);
                            break;
                        }


                        //skip quoteChar
                        i++;
                        //    isInPart = false;
                        inQuotedMode = false;
                        var part = sb.ToString();
                        //reset
                        sb.Length = 0;
                        yield return part;
                    }

                    else
                    {
                        prevIsEscape = isEscapeChar;

                        sb.Append(c);
                    }
                }
                else
                {
                    if (isSplitChar)
                    {
                        //end of part

                        var part = sb.ToString();
                        //reset
                        sb.Length = 0;
                        yield return part;

                        if (isLastChar)
                        {
                            //done
                            yield return string.Empty;
                            break;
                        }

                        isNewPart = true;
                    }
                    else
                    {
                        sb.Append(c);
                    }
                }
            }

            var lastPart = GetLastPart(sb);
            if (inQuotedMode)
            {
                //append quote back
                lastPart = quoteChar + lastPart;
            }

            if (lastPart != null)
            {
                yield return lastPart;
            }
        }

        private static string GetLastPart(StringBuilder sb)
        {
            var length = sb.Length;
            if (length > 0)
            {
                var lastPart = sb.ToString();
                return lastPart;
            }
            return null;
        }
    }
}