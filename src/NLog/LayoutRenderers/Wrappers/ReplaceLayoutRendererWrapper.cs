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

namespace NLog.LayoutRenderers.Wrappers
{
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using NLog.Config;

    /// <summary>
    /// Replaces a string in the output of another layout with another string.
    /// </summary>
    /// <example>
    /// ${replace:searchFor=\\n+:replaceWith=-:regex=true:inner=${message}}
    /// </example>
    [LayoutRenderer("replace")]
    [AppDomainFixedOutput]
    [ThreadAgnostic]
    [ThreadSafe]
    public sealed class ReplaceLayoutRendererWrapper : WrapperLayoutRendererBase
    {
        private Regex _regex;

        /// <summary>
        /// Gets or sets the text to search for.
        /// </summary>
        /// <value>The text search for.</value>
        /// <docgen category='Search/Replace Options' order='10' />
        public string SearchFor { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether regular expressions should be used.
        /// </summary>
        /// <value>A value of <c>true</c> if regular expressions should be used otherwise, <c>false</c>.</value>
        /// <docgen category='Search/Replace Options' order='10' />
        public bool Regex { get; set; }

        /// <summary>
        /// Gets or sets the replacement string.
        /// </summary>
        /// <value>The replacement string.</value>
        /// <docgen category='Search/Replace Options' order='10' />
        public string ReplaceWith { get; set; }

        /// <summary>
        /// Gets or sets the group name to replace when using regular expressions.
        /// Leave null or empty to replace without using group name.
        /// </summary>
        /// <value>The group name.</value>
        /// <docgen category='Search/Replace Options' order='10' />
        public string ReplaceGroupName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to ignore case.
        /// </summary>
        /// <value>A value of <c>true</c> if case should be ignored when searching; otherwise, <c>false</c>.</value>
        /// <docgen category='Search/Replace Options' order='10' />
        public bool IgnoreCase { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to search for whole words.
        /// </summary>
        /// <value>A value of <c>true</c> if whole words should be searched for; otherwise, <c>false</c>.</value>
        /// <docgen category='Search/Replace Options' order='10' />
        public bool WholeWords { get; set; }

        /// <summary>
        /// Initializes the layout renderer.
        /// </summary>
        protected override void InitializeLayoutRenderer()
        {
            base.InitializeLayoutRenderer();
            string regexString = SearchFor;

            if (!Regex)
            {
                regexString = System.Text.RegularExpressions.Regex.Escape(regexString);
            }

#if SILVERLIGHT
            RegexOptions regexOptions = RegexOptions.None;
#else
            RegexOptions regexOptions = RegexOptions.Compiled;
#endif
            if (IgnoreCase)
            {
                regexOptions |= RegexOptions.IgnoreCase;
            }

            if (WholeWords)
            {
                regexString = string.Concat("\\b", regexString, "\\b");
            }

            _regex = new Regex(regexString, regexOptions);
        }

        /// <summary>
        /// Post-processes the rendered message. 
        /// </summary>
        /// <param name="text">The text to be post-processed.</param>
        /// <returns>Post-processed text.</returns>
        protected override string Transform(string text)
        {
            if (string.IsNullOrEmpty(ReplaceGroupName))
            {
                return _regex.Replace(text, ReplaceWith);
            }
            else
            {
                var replacer = new Replacer(text, ReplaceGroupName, ReplaceWith);
                return _regex.Replace(text, replacer.EvaluateMatch);
            }
        }

        /// <summary>
        /// This class was created instead of simply using a lambda expression so that the "ThreadAgnosticAttributeTest" will pass
        /// </summary>
        [ThreadAgnostic]
        public class Replacer
        {
            private readonly string _text;
            private readonly string _replaceGroupName;
            private readonly string _replaceWith;

            internal Replacer(string text, string replaceGroupName, string replaceWith)
            {
                _text = text;
                _replaceGroupName = replaceGroupName;
                _replaceWith = replaceWith;
            }

            internal string EvaluateMatch(Match match)
            {
                return ReplaceNamedGroup(_text, _replaceGroupName, _replaceWith, match);
            }
        }

        /// <summary>
        /// A match evaluator for Regular Expression based replacing
        /// </summary>
        /// <param name="input">Input string.</param>
        /// <param name="groupName">Group name in the regex.</param>
        /// <param name="replacement">Replace value.</param>
        /// <param name="match">Match from regex.</param>
        /// <returns>Groups replaced with <paramref name="replacement"/>.</returns>
        public static string ReplaceNamedGroup(string input, string groupName, string replacement, Match match)
        {
            var sb = new StringBuilder(input);
            var matchStart = match.Index;
            var matchLength = match.Length;

            var captures = match.Groups[groupName].Captures.OfType<Capture>().OrderByDescending(c => c.Index);
            foreach (var capt in captures)
            {
                if (capt == null)
                    continue;

                matchLength += replacement.Length - capt.Length;

                sb.Remove(capt.Index, capt.Length);
                sb.Insert(capt.Index, replacement);
            }

            var end = matchStart + matchLength;
            sb.Remove(end, sb.Length - end);
            sb.Remove(0, matchStart);

            return sb.ToString();
        }
    }
}
