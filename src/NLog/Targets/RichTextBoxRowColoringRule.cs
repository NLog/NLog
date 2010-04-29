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

#if !NETCF && !MONO

using System;
using System.Text;
using System.Drawing;

using NLog.Conditions;
using NLog.Config;

namespace NLog.Targets
{
    /// <summary>
    /// The row-coloring condition.
    /// </summary>
    public class RichTextBoxRowColoringRule
    {
        private ConditionExpression _condition = null;
        private string _fontColor = "Empty";
        private string _backColor = "Empty";
        private FontStyle _style;
        /// <summary>
        /// Default highlighting rule. Doesn't change the color.
        /// </summary>
        public static readonly RichTextBoxRowColoringRule Default = new RichTextBoxRowColoringRule(null, "Empty", "Empty");
        
        /// <summary>
        /// The condition that must be met in order to set the specified font color.
        /// </summary>
        [AcceptsCondition]
        [RequiredParameter]
        public string Condition
        {
            get 
            { 
                if (_condition == null)
                    return null;
                else
                    return _condition.ToString();
            }
            set 
            { 
                if (value != null)
                    _condition = ConditionParser.ParseExpression(value);
                else
                    _condition = null;
            }
        }

        /// <summary>
        /// The font color.
        /// Names are identical with KnownColor enum extended with Empty value which means that background color won't be changed
        /// </summary>
        [System.ComponentModel.DefaultValue("Empty")]
        public string FontColor
        {
            get { return _fontColor; }
            set { _fontColor = value; }
        }

        /// <summary>
        /// The background color. 
        /// Names are identical with KnownColor enum extended with Empty value which means that background color won't be changed
        /// Background color will be set only in .net 2.0
        /// </summary>
        [System.ComponentModel.DefaultValue("Empty")]
        [SupportedRuntime(Framework = RuntimeFramework.DotNetFramework, MinRuntimeVersion = "2.0")]
        public string BackgroundColor
        {
            get { return _backColor; }
            set { _backColor = value; }
        }

        /// <summary>
        /// Font style of matched text. 
        /// Possible values are the same as in <c>FontStyle</c> enum in <c>System.Drawing</c>
        /// </summary>
        public FontStyle Style
        {
            get { return _style; }
            set { _style = value; }
        }

        /// <summary>
        /// Creates a new instance of <see cref="RichTextBoxRowColoringRule"/>
        /// </summary>
        public RichTextBoxRowColoringRule()
        {
        }

        /// <summary>
        /// Creates a new instance of <see cref="RichTextBoxRowColoringRule"/> and
        /// assigns Condition, FontColor and FontStyle properties.
        /// </summary>
        public RichTextBoxRowColoringRule(string condition, string fontColor, string backColor, FontStyle fontStyle)
        {
            Condition = condition;
            FontColor = fontColor;
            BackgroundColor = backColor;
            Style = fontStyle;
        }

        /// <summary>
        /// Creates a new instance of <see cref="RichTextBoxRowColoringRule"/> and
        /// assigns Condition and FontColor properties with regular style of font
        /// </summary>
        public RichTextBoxRowColoringRule(string condition, string fontColor, string backColor)
        {
            Condition = condition;
            FontColor = fontColor;
            BackgroundColor = backColor;
            Style = FontStyle.Regular;
        }

        /// <summary>
        /// Checks whether the specified log event matches the condition (if any)
        /// </summary>
        /// <param name="logEvent">log event</param>
        /// <returns><see langword="true"/> if the condition is not defined or 
        /// if it matches, <see langword="false"/> otherwise</returns>
        public bool CheckCondition(LogEventInfo logEvent)
        {
            if (_condition == null)
                return true;
            return true.Equals(_condition.Evaluate(logEvent));
        }
    }
}
#endif
