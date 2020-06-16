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

#if !NETSTANDARD1_3

namespace NLog.Targets
{
    using System;
    using System.ComponentModel;
    using System.Text.RegularExpressions;
    using NLog.Conditions;
    using NLog.Config;
    using NLog.Internal;

    /// <summary>
    /// Highlighting rule for Win32 colorful console.
    /// </summary>
    [NLogConfigurationItem]
    public class ConsoleWordHighlightingRule
    {
        private readonly RegexHelper _regexHelper = new RegexHelper();

        /// <summary>
        /// Initializes a new instance of the <see cref="ConsoleWordHighlightingRule" /> class.
        /// </summary>
        public ConsoleWordHighlightingRule()
        {
            BackgroundColor = ConsoleOutputColor.NoChange;
            ForegroundColor = ConsoleOutputColor.NoChange;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConsoleWordHighlightingRule" /> class.
        /// </summary>
        /// <param name="text">The text to be matched..</param>
        /// <param name="foregroundColor">Color of the foreground.</param>
        /// <param name="backgroundColor">Color of the background.</param>
        public ConsoleWordHighlightingRule(string text, ConsoleOutputColor foregroundColor, ConsoleOutputColor backgroundColor)
        {
            _regexHelper.SearchText = text;
            ForegroundColor = foregroundColor;
            BackgroundColor = backgroundColor;
        }

        /// <summary>
        /// Gets or sets the regular expression to be matched. You must specify either <c>text</c> or <c>regex</c>.
        /// </summary>
        /// <docgen category='Rule Matching Options' order='10' />
        public string Regex
        {
            get => _regexHelper.RegexPattern;
            set => _regexHelper.RegexPattern = value;
        }

        /// <summary>
        /// Gets or sets the condition that must be met before scanning the row for highlight of words
        /// </summary>
        /// <docgen category='Rule Matching Options' order='10' />
        public ConditionExpression Condition { get; set; }

        /// <summary>
        /// Compile the <see cref="Regex"/>? This can improve the performance, but at the costs of more memory usage. If <c>false</c>, the Regex Cache is used.
        /// </summary>
        /// <docgen category='Rule Matching Options' order='10' />
        [DefaultValue(false)]
        public bool CompileRegex
        {
            get => _regexHelper.CompileRegex;
            set => _regexHelper.CompileRegex = value;
        }

        /// <summary>
        /// Gets or sets the text to be matched. You must specify either <c>text</c> or <c>regex</c>.
        /// </summary>
        /// <docgen category='Rule Matching Options' order='10' />
        public string Text
        {
            get => _regexHelper.SearchText;
            set => _regexHelper.SearchText = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether to match whole words only.
        /// </summary>
        /// <docgen category='Rule Matching Options' order='10' />
        [DefaultValue(false)]
        public bool WholeWords
        {
            get => _regexHelper.WholeWords;
            set => _regexHelper.WholeWords = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether to ignore case when comparing texts.
        /// </summary>
        /// <docgen category='Rule Matching Options' order='10' />
        [DefaultValue(false)]
        public bool IgnoreCase
        {
            get => _regexHelper.IgnoreCase;
            set => _regexHelper.IgnoreCase = value;
        }

        /// <summary>
        /// Gets or sets the foreground color.
        /// </summary>
        /// <docgen category='Formatting Options' order='10' />
        [DefaultValue("NoChange")]
        public ConsoleOutputColor ForegroundColor { get; set; }

        /// <summary>
        /// Gets or sets the background color.
        /// </summary>
        /// <docgen category='Formatting Options' order='10' />
        [DefaultValue("NoChange")]
        public ConsoleOutputColor BackgroundColor { get; set; }

        /// <summary>
        /// Gets the compiled regular expression that matches either Text or Regex property. Only used when <see cref="CompileRegex"/> is <c>true</c>.
        /// </summary>
        public Regex CompiledRegex => _regexHelper.Regex;

        internal MatchCollection Matches(LogEventInfo logEvent, string message)
        {
            if (Condition != null && false.Equals(Condition.Evaluate(logEvent)))
            {
                return null;
            }

            return _regexHelper.Matches(message);
        }
    }
}

#endif