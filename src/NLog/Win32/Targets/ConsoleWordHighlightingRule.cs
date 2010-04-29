// 
// Copyright (c) 2004-2006 Jaroslaw Kowalski <jaak@jkowalski.net>
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

#if !NETCF

using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;

using NLog.Config;
using NLog.Conditions;
using NLog.Targets;

namespace NLog.Win32.Targets
{
    /// <summary>
    /// Highlighting rule for Win32 colorful console.
    /// </summary>
    public class ConsoleWordHighlightingRule
    {
        private string _text;
        private string _regex;
        private bool _wholeWords = false;
        private bool _ignoreCase = false;
        private Regex _compiledRegex;
        private ConsoleOutputColor _backgroundColor = ConsoleOutputColor.NoChange;
        private ConsoleOutputColor _foregroundColor = ConsoleOutputColor.NoChange;

        /// <summary>
        /// The regular expression to be matched. You must specify either <c>text</c> or <c>regex</c>.
        /// </summary>
        public string Regex
        {
            get { return _regex; }
            set { _regex = value; }
        }

        /// <summary>
        /// The text to be matched. You must specify either <c>text</c> or <c>regex</c>.
        /// </summary>
        public string Text
        {
            get { return _text; }
            set { _text = value; }
        }

        /// <summary>
        /// Match whole words only.
        /// </summary>
        [System.ComponentModel.DefaultValue(false)]
        public bool WholeWords
        {
            get { return _wholeWords; }
            set { _wholeWords = value; }
        }

        /// <summary>
        /// Ignore case when comparing texts.
        /// </summary>
        [System.ComponentModel.DefaultValue(false)]
        public bool IgnoreCase
        {
            get { return _ignoreCase; }
            set { _ignoreCase = value; }
        }

        /// <summary>
        /// Compiled regular expression that matches either Text or Regex property.
        /// </summary>
        public Regex CompiledRegex
        {
            get
            {
                if (_compiledRegex == null)
                {
                    string regexpression = _regex;
                    if (regexpression == null && _text != null)
                    {
                        regexpression = System.Text.RegularExpressions.Regex.Escape(_text);
                        if (WholeWords)
                            regexpression = "\b" + regexpression + "\b";
                    }

                    RegexOptions regexOptions = RegexOptions.Compiled;
                    if (IgnoreCase)
                        regexOptions |= RegexOptions.IgnoreCase;

                    _compiledRegex = new Regex(regexpression, regexOptions);
                }

                return _compiledRegex;
            }
        }

        /// <summary>
        /// The foreground color.
        /// </summary>
        [System.ComponentModel.DefaultValue("NoChange")]
        public ConsoleOutputColor ForegroundColor
        {
            get { return _foregroundColor; }
            set { _foregroundColor = value; }
        }

        /// <summary>
        /// The background color.
        /// </summary>
        [System.ComponentModel.DefaultValue("NoChange")]
        public ConsoleOutputColor BackgroundColor
        {
            get { return _backgroundColor; }
            set { _backgroundColor = value; }
        }

        /// <summary>
        /// Creates a new instance of <see cref="ConsoleWordHighlightingRule"/>
        /// </summary>
        public ConsoleWordHighlightingRule()
        {
        }

        /// <summary>
        /// Creates a new instance of <see cref="ConsoleWordHighlightingRule"/>
        /// and sets Text, BackgroundColor and ForegroundColor properties.
        /// </summary>
        public ConsoleWordHighlightingRule(string text, ConsoleOutputColor foregroundColor, ConsoleOutputColor backgroundColor)
        {
            Text = text;
            ForegroundColor = foregroundColor;
            BackgroundColor = backgroundColor;
        }

        internal string MatchEvaluator(Match m)
        {
            StringBuilder result = new StringBuilder();
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
            return CompiledRegex.Replace(message, new MatchEvaluator(this.MatchEvaluator));
        }
    }
}

#endif