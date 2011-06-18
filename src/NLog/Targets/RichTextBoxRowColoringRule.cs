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
    using NLog.Conditions;
    using NLog.Config;

    /// <summary>
    /// The row-coloring condition.
    /// </summary>
    [NLogConfigurationItem]
    public class RichTextBoxRowColoringRule
    {
        /// <summary>
        /// Initializes static members of the RichTextBoxRowColoringRule class.
        /// </summary>
        static RichTextBoxRowColoringRule()
        {
            Default = new RichTextBoxRowColoringRule();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RichTextBoxRowColoringRule" /> class.
        /// </summary>
        public RichTextBoxRowColoringRule()
            : this(null, "Empty", "Empty", FontStyle.Regular)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RichTextBoxRowColoringRule" /> class.
        /// </summary>
        /// <param name="condition">The condition.</param>
        /// <param name="fontColor">Color of the foregroung text.</param>
        /// <param name="backColor">Color of the background text.</param>
        /// <param name="fontStyle">The font style.</param>
        public RichTextBoxRowColoringRule(string condition, string fontColor, string backColor, FontStyle fontStyle)
        {
            this.Condition = condition;
            this.FontColor = fontColor;
            this.BackgroundColor = backColor;
            this.Style = fontStyle;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RichTextBoxRowColoringRule" /> class.
        /// </summary>
        /// <param name="condition">The condition.</param>
        /// <param name="fontColor">Color of the text.</param>
        /// <param name="backColor">Color of the background.</param>
        public RichTextBoxRowColoringRule(string condition, string fontColor, string backColor)
        {
            this.Condition = condition;
            this.FontColor = fontColor;
            this.BackgroundColor = backColor;
            this.Style = FontStyle.Regular;
        }

        /// <summary>
        /// Gets the default highlighting rule. Doesn't change the color.
        /// </summary>
        /// <docgen category='Rule Matching Options' order='10' />
        public static RichTextBoxRowColoringRule Default { get; private set; }
        
        /// <summary>
        /// Gets or sets the condition that must be met in order to set the specified font color.
        /// </summary>
        /// <docgen category='Rule Matching Options' order='10' />
        [RequiredParameter]
        public ConditionExpression Condition { get; set; }

        /// <summary>
        /// Gets or sets the font color.
        /// </summary>
        /// <remarks>
        /// Names are identical with KnownColor enum extended with Empty value which means that background color won't be changed.
        /// </remarks>
        /// <docgen category='Formatting Options' order='10' />
        [DefaultValue("Empty")]
        public string FontColor { get; set; }

        /// <summary>
        /// Gets or sets the background color.
        /// </summary>
        /// <remarks>
        /// Names are identical with KnownColor enum extended with Empty value which means that background color won't be changed.
        /// </remarks>
        /// <docgen category='Formatting Options' order='10' />
        [DefaultValue("Empty")]
        public string BackgroundColor { get; set; }

        /// <summary>
        /// Gets or sets the font style of matched text. 
        /// </summary>
        /// <remarks>
        /// Possible values are the same as in <c>FontStyle</c> enum in <c>System.Drawing</c>
        /// </remarks>
        /// <docgen category='Formatting Options' order='10' />
        public FontStyle Style { get; set; }

        /// <summary>
        /// Checks whether the specified log event matches the condition (if any).
        /// </summary>
        /// <param name="logEvent">
        /// Log event.
        /// </param>
        /// <returns>
        /// A value of <see langword="true"/> if the condition is not defined or 
        /// if it matches, <see langword="false"/> otherwise.
        /// </returns>
        public bool CheckCondition(LogEventInfo logEvent)
        {
            return true.Equals(this.Condition.Evaluate(logEvent));
        }
    }
}
#endif
