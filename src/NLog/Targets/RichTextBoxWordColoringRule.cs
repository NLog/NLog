// 
// Copyright (c) 2004-2011 Jaroslaw Kowalski <jaak@jkowalski.net>
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

#if !NET_CF && !MONO && !SILVERLIGHT

namespace NLog.Targets
{
    using System.ComponentModel;
    using System.Drawing;
    using System.Text.RegularExpressions;
    using NLog.Config;

    /// <summary>
    /// Highlighting rule for Win32 colorful console.
    /// </summary>
    [NLogConfigurationItem]
    public class RichTextBoxWordColoringRule
    {
        private Regex compiledRegex;

        /// <summary>
        /// Initializes a new instance of the <see cref="RichTextBoxWordColoringRule" /> class.
        /// </summary>
        public RichTextBoxWordColoringRule()
        {
            this.FontColor = "Empty";
            this.BackgroundColor = "Empty";
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RichTextBoxWordColoringRule" /> class.
        /// </summary>
        /// <param name="text">The text to be matched..</param>
        /// <param name="fontColor">Color of the text.</param>
        /// <param name="backgroundColor">Color of the background.</param>
        public RichTextBoxWordColoringRule(string text, string fontColor, string backgroundColor)
        {
            this.Text = text;
            this.FontColor = fontColor;
            this.BackgroundColor = backgroundColor;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RichTextBoxWordColoringRule" /> class.
        /// </summary>
        /// <param name="text">The text to be matched..</param>
        /// <param name="textColor">Color of the text.</param>
        /// <param name="backgroundColor">Color of the background.</param>
        /// <param name="fontStyle">The font style.</param>
        public RichTextBoxWordColoringRule(string text, string textColor, string backgroundColor, FontStyle fontStyle)
        {
            this.Text = text;
            this.FontColor = textColor;
            this.BackgroundColor = backgroundColor;
            this.Style = fontStyle;
        }

        /// <summary>
        /// Gets or sets the regular expression to be matched. You must specify either <c>text</c> or <c>regex</c>.
        /// </summary>
        /// <docgen category='Rule Matching Options' order='10' />
        public string Regex { get; set; }

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
        /// Gets or sets the font style of matched text. 
        /// Possible values are the same as in <c>FontStyle</c> enum in <c>System.Drawing</c>.
        /// </summary>
        /// <docgen category='Formatting Options' order='10' />
        public FontStyle Style { get; set; }

        /// <summary>
        /// Gets the compiled regular expression that matches either Text or Regex property.
        /// </summary>
        public Regex CompiledRegex
        {
            get
            {
                if (this.compiledRegex == null)
                {
                    string regexpression = this.Regex;
                    if (regexpression == null && this.Text != null)
                    {
                        regexpression = System.Text.RegularExpressions.Regex.Escape(this.Text);
                        if (this.WholeWords)
                        {
                            regexpression = "\b" + regexpression + "\b";
                        }
                    }

                    RegexOptions regexOptions = RegexOptions.Compiled;
                    if (this.IgnoreCase)
                    {
                        regexOptions |= RegexOptions.IgnoreCase;
                    }

                    this.compiledRegex = new Regex(regexpression, regexOptions);
                }

                return this.compiledRegex;
            }
        }

        /// <summary>
        /// Gets or sets the font color.
        /// Names are identical with KnownColor enum extended with Empty value which means that font color won't be changed.
        /// </summary>
        /// <docgen category='Formatting Options' order='10' />
        [DefaultValue("Empty")]
        public string FontColor { get; set; }

        /// <summary>
        /// Gets or sets the background color. 
        /// Names are identical with KnownColor enum extended with Empty value which means that background color won't be changed.
        /// </summary>
        /// <docgen category='Formatting Options' order='10' />
        [DefaultValue("Empty")]
        public string BackgroundColor { get; set; }
    }
}
#endif
