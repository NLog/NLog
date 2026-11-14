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

using System;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace NLog.Conditions
{
    /// <summary>
    /// A bunch of utility methods (mostly predicates) which can be used in
    /// condition expressions. Partially inspired by XPath 1.0.
    /// </summary>
    [ConditionMethods]
    public static class RegexConditionMethods
    {
        /// <summary>
        /// Indicates whether the specified regular expression finds a match in the specified input string.
        /// </summary>
        /// <param name="input">The string to search for a match.</param>
        /// <param name="pattern">The regular expression pattern to match.</param>
        /// <param name="options">A string consisting of the desired options for the test. The possible values are those of the <see cref="RegexOptions"/> separated by commas.</param>
        /// <returns><see langword="true"/> if the regular expression finds a match; otherwise, <see langword="false"/>.</returns>
        [ConditionMethod("regex-matches")]
        public static bool RegexMatches(string input, string pattern, [Optional, DefaultParameterValue("")] string options)
        {
            RegexOptions regexOpts = ParseRegexOptions(options);
            return Regex.IsMatch(input, pattern, regexOpts);
        }

        /// <summary>
        /// Counts the number of matches for the specified regular expression in the specified input string.
        /// </summary>
        /// <param name="input">The string to search for a match.</param>
        /// <param name="pattern">The regular expression pattern to match.</param>
        /// <param name="options">A string consisting of the desired options for the test. The possible values are those of the <see cref="RegexOptions"/> separated by commas.</param>
        /// <returns>The number of matches.</returns>
        [ConditionMethod("regex-count")]
        public static int RegexCount(string input, string pattern, [Optional, DefaultParameterValue("")] string options)
        {
            RegexOptions regexOpts = ParseRegexOptions(options);
            int count = 0;
            for (var match = Regex.Match(input, pattern, regexOpts); match.Success; match = match.NextMatch())
                ++count;
            return count;
        }

        private static RegexOptions ParseRegexOptions(string options)
        {
            if (string.IsNullOrEmpty(options))
            {
                return RegexOptions.None;
            }

            return (RegexOptions)Enum.Parse(typeof(RegexOptions), options, true);
        }
    }
}
