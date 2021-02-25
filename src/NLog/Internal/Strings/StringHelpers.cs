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
using System.Linq;
using System.Text;
using JetBrains.Annotations;

namespace NLog.Internal
{
    /// <summary>
    /// Helpers for <see cref="string"/>.
    /// </summary>
    internal static class StringHelpers
    {
        /// <summary>
        /// IsNullOrWhiteSpace, including for .NET 3.5
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        [ContractAnnotation("value:null => true")]
        internal static bool IsNullOrWhiteSpace(string value)
        {
#if !NET35
            return string.IsNullOrWhiteSpace(value);
#else
            if (value == null) return true;
            if (value.Length == 0) return true;
            return String.IsNullOrEmpty(value.Trim());
#endif
        }

        internal static string[] SplitAndTrimTokens(this string value, char delimiter)
        {
            if (IsNullOrWhiteSpace(value))
                return ArrayHelper.Empty<string>();

            if (value.IndexOf(delimiter) == -1)
            {
                return new[] { value.Trim() };
            }

            var result = value.Split(new char[] { delimiter }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < result.Length; ++i)
            {
                result[i] = result[i].Trim();
                if (string.IsNullOrEmpty(result[i]))
                    return result.Where(s => !IsNullOrWhiteSpace(s)).Select(s => s.Trim()).ToArray();
            }
            return result;
        }

        /// <summary>
        /// Replace string with <paramref name="comparison"/>
        /// </summary>
        /// <param name="str"></param>
        /// <param name="oldValue"></param>
        /// <param name="newValue"></param>
        /// <param name="comparison"></param>
        /// <returns>The same reference of nothing has been replaced.</returns>
        public static string Replace([NotNull] string str, [NotNull] string oldValue, string newValue, StringComparison comparison)
        {
            if (str == null)
            {
                throw new ArgumentNullException(nameof(str));
            }

            if (oldValue == null)
            {
                throw new ArgumentNullException(nameof(oldValue));
            }

            if (str.Length == 0)
            {
                //nothing to do
                return str;
            }

            StringBuilder sb = null;

            int previousIndex = 0;
            int index = str.IndexOf(oldValue, comparison);
            while (index != -1)
            {
                sb = sb ?? new StringBuilder(str.Length);

                if (previousIndex >= str.Length)
                {
                    // for cases that 2 chars is one symbol
                    break;
                }
                sb.Append(str.Substring(previousIndex, index - previousIndex));
                sb.Append(newValue);
                index += oldValue.Length;

                previousIndex = index;
                if (index >= str.Length)
                {
                    // for cases that 2 chars is one symbol
                    break;
                }

                index = str.IndexOf(oldValue, index, comparison);
            }

            if (sb == null)
            {
                //nothing replaced
                return str;
            }

            if (previousIndex < str.Length)
            {
                sb.Append(str.Substring(previousIndex));
            }
            return sb.ToString();

        }

        /// <summary>Concatenates all the elements of a string array, using the specified separator between each element. </summary>
        /// <param name="separator">The string to use as a separator. <paramref name="separator" /> is included in the returned string only if <paramref name="values" /> has more than one element.</param>
        /// <param name="values">An collection that contains the elements to concatenate. </param>
        /// <returns>A string that consists of the elements in <paramref name="values" /> delimited by the <paramref name="separator" /> string. If <paramref name="values" /> is an empty array, the method returns <see cref="F:System.String.Empty" />.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="values" /> is <see langword="null" />. </exception>
        internal static string Join(string separator, IEnumerable<string> values)
        {
#if !NET35
            return string.Join(separator, values);
#else
            return string.Join(separator, values.ToArray());
#endif
        }
    }
}
