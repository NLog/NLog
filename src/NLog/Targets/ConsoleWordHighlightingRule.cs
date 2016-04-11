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

#if !SILVERLIGHT

namespace NLog.Targets
{
    using System;
    using System.ComponentModel;
    using System.Text;
    using System.Text.RegularExpressions;
    using NLog.Config;

    /// <summary>
    /// Highlighting rule for Win32 colorful console.
    /// </summary>
    [NLogConfigurationItem]
    public class ConsoleWordHighlightingRule
    {
        private Regex compiledRegex;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConsoleWordHighlightingRule" /> class.
        /// </summary>
        public ConsoleWordHighlightingRule()
        {
            this.BackgroundColor = ConsoleOutputColor.NoChange;
            this.ForegroundColor = ConsoleOutputColor.NoChange;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConsoleWordHighlightingRule" /> class.
        /// </summary>
        /// <param name="text">The text to be matched..</param>
        /// <param name="foregroundColor">Color of the foreground.</param>
        /// <param name="backgroundColor">Color of the background.</param>
        public ConsoleWordHighlightingRule(string text, ConsoleOutputColor foregroundColor, ConsoleOutputColor backgroundColor)
        {
            this.Text = text;
            this.ForegroundColor = foregroundColor;
            this.BackgroundColor = backgroundColor;
        }

        /// <summary>
        /// Gets or sets the regular expression to be matched. You must specify either <c>text</c> or <c>regex</c>.
        /// </summary>
        /// <docgen category='Rule Matching Options' order='10' />
        public string Regex { get; set; }

        /// <summary>
        /// Compile the <see cref="Regex"/>? This can improve the performance, but at the costs of more memory usage. If <c>false</c>, the Regex Cache is used.
        /// </summary>
        [DefaultValue(false)]
        public bool CompileRegex { get; set; }

        /// <summary>
        /// Gets or sets the text to be matched. You must specify either <c>text</c> or <c>regex</c>.
        /// </summary>
        /// <docgen category='Rule Matching Options' order='10' />
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to match whole words only.
        /// </summary>
        /// <docgen category='Rule Matching Options' order='10' />
        [DefaultValue(false)]
        public bool WholeWords { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to ignore case when comparing texts.
        /// </summary>
        /// <docgen category='Rule Matching Options' order='10' />
        [DefaultValue(false)]
        public bool IgnoreCase { get; set; }

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
        /// <remarks>Access this property will compile the Regex.</remarks>
        public Regex CompiledRegex
        {
            get
            {
                //compile regex on first usage.
                if (this.compiledRegex == null)
                {
                    var regexpression = GetRegexExpression();
                    if (regexpression == null)
                    {
                        //we can't build an empty regex
                        return null;
                    }

                    var regexOptions = GetRegexOptions(RegexOptions.Compiled);
                    this.compiledRegex = new Regex(regexpression, regexOptions);
                }

                return this.compiledRegex;
            }
        }

        /// <summary>
        /// Get regex options. 
        /// </summary>
        /// <param name="regexOptions">Default option to start with.</param>
        /// <returns></returns>
        private RegexOptions GetRegexOptions(RegexOptions regexOptions)
        {
            if (this.IgnoreCase)
            {
                regexOptions |= RegexOptions.IgnoreCase;
            }
            return regexOptions;
        }

        /// <summary>
        /// Get Expression for a <see cref="Regex"/>.
        /// </summary>
        /// <returns></returns>
        private string GetRegexExpression()
        {
            string regexpression = this.Regex;

            if (regexpression == null && this.Text != null)
            {
                regexpression = System.Text.RegularExpressions.Regex.Escape(this.Text);
                if (this.WholeWords)
                {
                    regexpression = "\\b" + regexpression + "\\b";
                }
            }
            return regexpression;
        }

        /// <summary>
        /// Replace regex result
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        private string MatchEvaluator(Match m)
        {
            StringBuilder result = new StringBuilder(m.Value.Length + 5);

            result.Append('\a');
            result.Append((char)((int)this.ForegroundColor + 'A'));
            result.Append((char)((int)this.BackgroundColor + 'A'));
            result.Append(m.Value);
            result.Append('\a');
            result.Append('X');

            return result.ToString();
        }


        internal string ReplaceWithEscapeSequences(string message)
        {
            if (CompileRegex)
            {
                var regex = this.CompiledRegex;
                if (regex == null)
                {
                    //empty regex so nothing todo
                    return message;
                }

                return regex.Replace(message, this.MatchEvaluator);
            }
            //use regex cache
            var expression = GetRegexExpression();
            if (expression != null)
            {
                RegexOptions regexOptions = GetRegexOptions(RegexOptions.None);
                //the static methods of Regex will cache the regex
                return System.Text.RegularExpressions.Regex.Replace(message, expression, this.MatchEvaluator, regexOptions);
            }
            return message;
        }
    }
}

#endif