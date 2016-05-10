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
    public static class StringSplitter
    {
        /// <summary>
        /// Split string with escape. The escape char is the same as the splitchar
        /// </summary>
        /// <param name="text"></param>
        /// <param name="splitChar">split char. escaped also with this char</param>
        /// <returns></returns>
        public static IEnumerable<string> SplitWithSelfEscape(this string text, char splitChar)
        {
            return SplitWithEscape(text, splitChar, splitChar);
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
            if (!string.IsNullOrEmpty(text))
            {
                var prevWasEscape = false;
                int i;
                var sb = new StringBuilder();
                if (splitChar == escapeChar)
                {
                    //if same, handle different
                    for (i = 0; i < text.Length; i++)
                    {
                        var c = text[i];
                        var isSplitChar = c == splitChar;
                        if (prevWasEscape)
                        {
                            if (isSplitChar)
                            {
                                prevWasEscape = false;
                            }
                            else
                            {
                                if (sb.Length > 0) sb.Length --;  
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
                            sb.Append(c);
                            if (isSplitChar)
                            {
                                prevWasEscape = true;
                            }
                        }
                    }
                }
                else
                {
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
                                prevWasEscape = false;
                                yield return part;
                            }
                            else
                            {
                                sb.Append(c);
                                prevWasEscape = c == escapeChar;
                            }
                        }
                    }
                }
                var length = sb.Length;
                if (length > 0)
                {
                    var lastPart = sb.ToString();
                    yield return lastPart;
                }
            }
        }
    }
}