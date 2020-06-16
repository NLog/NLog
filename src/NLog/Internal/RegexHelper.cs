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
using System.Text.RegularExpressions;

namespace NLog.Internal
{
    internal class RegexHelper
    {
        private Regex _regex;
        private string _searchText;
        private string _regexPattern;
        private bool _wholeWords;
        private bool _ignoreCase;
        private bool _simpleSearchText;

        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                _regexPattern = null;
                ResetRegex();
            }
        }

        public string RegexPattern
        {
            get => _regexPattern;
            set
            {
                _regexPattern = value;
                _searchText = null;
                ResetRegex();
            }
        }

        /// <summary>
        /// Compile the <see cref="Regex"/>? This can improve the performance, but at the costs of more memory usage. If <c>false</c>, the Regex Cache is used.
        /// </summary>
        /// <docgen category='Rule Matching Options' order='10' />
        public bool CompileRegex { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to match whole words only.
        /// </summary>
        /// <docgen category='Rule Matching Options' order='10' />
        public bool WholeWords
        {
            get => _wholeWords;
            set
            {
                if (_wholeWords != value)
                {
                    _wholeWords = value;
                    ResetRegex();
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to ignore case when comparing texts.
        /// </summary>
        /// <docgen category='Rule Matching Options' order='10' />
        public bool IgnoreCase
        {
            get => _ignoreCase;
            set
            {
                if (_ignoreCase != value)
                {
                    _ignoreCase = value;
                    ResetRegex();
                }
            }
        }

        public Regex Regex
        {
            get
            {
                if (_regex != null)
                    return _regex;

                var regexpression = RegexPattern;
                if (string.IsNullOrEmpty(regexpression))
                    return null;

                return _regex = new Regex(regexpression, GetRegexOptions());
            }
        }

        private void ResetRegex()
        {
            _simpleSearchText = !WholeWords && !IgnoreCase && !string.IsNullOrEmpty(SearchText);
            if (!string.IsNullOrEmpty(SearchText))
            {
                _regexPattern = Regex.Escape(SearchText);
            }

            if (WholeWords && !string.IsNullOrEmpty(_regexPattern))
            {
                _regexPattern = string.Concat("\\b", _regexPattern, "\\b");
            }

            _regex = null;
        }

        private RegexOptions GetRegexOptions()
        {
            RegexOptions regexOptions = RegexOptions.None;
            if (IgnoreCase)
            {
                regexOptions |= RegexOptions.IgnoreCase;
            }
            if (CompileRegex)
            {
                regexOptions |= RegexOptions.Compiled;
            }
            return regexOptions;
        }

        public string Replace(string input, string replacement)
        {
            if (_simpleSearchText)
            {
                return input.Replace(SearchText, replacement);
            }
            else if (CompileRegex)
            {
                return Regex?.Replace(input, replacement) ?? input;
            }
            else
            {
                return _regexPattern != null ? Regex.Replace(input, _regexPattern, replacement, GetRegexOptions()) : input;
            }
        }

        public MatchCollection Matches(string input)
        {
            if (CompileRegex)
            {
                return Regex?.Matches(input);
            }
            else
            {
                return _regexPattern != null ? Regex.Matches(input, _regexPattern, GetRegexOptions()) : null;
            }
        }
    }
}
