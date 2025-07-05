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

namespace NLog.Targets
{
    using System.Collections.Generic;
    using NLog.Config;
    using NLog.RegEx.Internal;

    /// <summary>
    /// Highlighting rule for Win32 colorful console.
    /// </summary>
    [NLogConfigurationItem]
    public class ConsoleWordHighlightingRuleRegex : ConsoleWordHighlightingRule
    {
        private readonly RegexHelper _regexHelper = new RegexHelper();

        /// <summary>
        /// Gets or sets the regular expression to be matched. You must specify either <c>text</c> or <c>regex</c>.
        /// </summary>
        /// <docgen category='Highlighting Rules' order='10' />
        public string? Regex
        {
            get => _regexHelper.RegexPattern;
            set => _regexHelper.RegexPattern = value;
        }

        /// <summary>
        /// Compile the <see cref="Regex"/>? This can improve the performance, but at the costs of more memory usage. If <see langword="false"/>, the Regex Cache is used.
        /// </summary>
        /// <docgen category='Highlighting Rules' order='10' />
        public bool CompileRegex
        {
            get => _regexHelper.CompileRegex;
            set => _regexHelper.CompileRegex = value;
        }

        private string _searchText = string.Empty;

        /// <inheritdoc/>
        protected override IEnumerable<KeyValuePair<int, int>>? GetWordsForHighlighting(string haystack)
        {
            if (!ReferenceEquals(_searchText, Text))
            {
                if (!string.IsNullOrEmpty(Text))
                    _regexHelper.SearchText = Text;
                _regexHelper.WholeWords = WholeWords;
                _regexHelper.IgnoreCase = IgnoreCase;
                _searchText = Text;
            }

            var matches = _regexHelper.Matches(haystack);
            if (matches is null || matches.Count == 0)
                return null;

            return YieldWordsForHighlighting(matches);
        }

        private static IEnumerable<KeyValuePair<int, int>> YieldWordsForHighlighting(System.Text.RegularExpressions.MatchCollection matches)
        {
            foreach (System.Text.RegularExpressions.Match match in matches)
                yield return new KeyValuePair<int, int>(match.Index, match.Length);
        }
    }
}
