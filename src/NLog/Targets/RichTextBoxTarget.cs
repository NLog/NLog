// 
// Copyright (c) 2004-2009 Jaroslaw Kowalski <jaak@jkowalski.net>
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

using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using NLog.Config;
using NLog.Internal;

namespace NLog.Targets
{
    /// <summary>
    /// Log text to Text property of  RichTextBox of specified Name.
    /// </summary>
    /// <example>
    /// <p>
    /// To set up the target in the <a href="config.html">configuration file</a>, 
    /// use the following syntax:
    /// </p><code lang="XML" source="examples/targets/Configuration File/RichTextBox/Simple/NLog.config">
    /// </code>
    /// <p>
    /// The result is:
    /// </p><img source="examples/targets/Screenshots/RichTextBox/Simple.gif"/><p>
    /// To set up the target with coloring rules in the <a href="config.html">configuration file</a>, 
    /// use the following syntax:
    /// </p><code lang="XML" source="examples/targets/Configuration File/RichTextBox/RowColoring/NLog.config">
    /// </code>
    /// <code lang="XML" source="examples/targets/Configuration File/RichTextBox/WordColoring/NLog.config">
    /// </code>
    /// <p>
    /// The result is:
    /// </p><img source="examples/targets/Screenshots/RichTextBox/RowColoring.gif"/><img source="examples/targets/Screenshots/RichTextBox/WordColoring.gif"/><p>
    /// To set up the log target programmatically similar to above use code like this:
    /// </p><code lang="C#" source="examples/targets/Configuration API/RichTextBox/Simple/Form1.cs">
    /// </code>
    /// ,
    /// <code lang="C#" source="examples/targets/Configuration API/RichTextBox/RowColoring/Form1.cs">
    /// </code>
    /// for RowColoring,
    /// <code lang="C#" source="examples/targets/Configuration API/RichTextBox/WordColoring/Form1.cs">
    /// </code>
    /// for WordColoring
    /// </example>
    [Target("RichTextBox")]
    public sealed class RichTextBoxTarget : TargetWithLayout
    {
        private static ICollection<RichTextBoxRowColoringRule> defaultRichTextBoxRowColoringRules = new List<RichTextBoxRowColoringRule>();

        /// <summary>
        /// Initializes static members of the RichTextBoxTarget class.
        /// </summary>
        /// <remarks>
        /// The default value of the layout is: <code>${longdate}|${level:uppercase=true}|${logger}|${message}</code>
        /// </remarks>
        static RichTextBoxTarget()
        {
            defaultRichTextBoxRowColoringRules.Add(new RichTextBoxRowColoringRule("level == LogLevel.Fatal", "White", "Red", FontStyle.Bold));
            defaultRichTextBoxRowColoringRules.Add(new RichTextBoxRowColoringRule("level == LogLevel.Error", "Red", "Empty", FontStyle.Bold | FontStyle.Italic));
            defaultRichTextBoxRowColoringRules.Add(new RichTextBoxRowColoringRule("level == LogLevel.Warn", "Orange", "Empty", FontStyle.Underline));
            defaultRichTextBoxRowColoringRules.Add(new RichTextBoxRowColoringRule("level == LogLevel.Info", "Black", "Empty"));
            defaultRichTextBoxRowColoringRules.Add(new RichTextBoxRowColoringRule("level == LogLevel.Debug", "Gray", "Empty"));
            defaultRichTextBoxRowColoringRules.Add(new RichTextBoxRowColoringRule("level == LogLevel.Trace", "DarkGray", "Empty", FontStyle.Italic));
        }

        /// <summary>
        /// Initializes a new instance of the RichTextBoxTarget class.
        /// </summary>
        /// <remarks>
        /// The default value of the layout is: <code>${longdate}|${level:uppercase=true}|${logger}|${message}</code>
        /// </remarks>
        public RichTextBoxTarget()
        {
            this.WordColoringRules = new List<RichTextBoxWordColoringRule>();
            this.RowColoringRules = new List<RichTextBoxRowColoringRule>();
        }

        private delegate void DelSendTheMessageToRichTextBox(RichTextBox rtbx, string logMessage, RichTextBoxRowColoringRule rule);

        /// <summary>
        /// Gets or sets the Name of RichTextBox to which Nlog will write.
        /// </summary>
        [RequiredParameter]
        public string ControlName { get; set; }

        /// <summary>
        /// Gets or sets the name of the Form on which the control is located. 
        /// If there is no open form of a specified name than NLog will create a new one.
        /// </summary>
        public string FormName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to use default coloring rules.
        /// </summary>
        [DefaultValue(false)]
        public bool UseDefaultRowColoringRules { get; set; }

        /// <summary>
        /// Gets the row coloring rules.
        /// </summary>
        [ArrayParameter(typeof(RichTextBoxRowColoringRule), "row-coloring")]
        public ICollection<RichTextBoxRowColoringRule> RowColoringRules { get; private set; }

        /// <summary>
        /// Gets the word highlighting rules.
        /// </summary>
        [ArrayParameter(typeof(RichTextBoxWordColoringRule), "word-coloring")]
        public ICollection<RichTextBoxWordColoringRule> WordColoringRules { get; private set; }

        /// <summary>
        /// Log message to RichTextBox.
        /// </summary>
        /// <param name="logEvent">The logging event.</param>
        protected internal override void Write(LogEventInfo logEvent)
        {
            RichTextBoxRowColoringRule matchingRule = null;

            foreach (RichTextBoxRowColoringRule rr in this.RowColoringRules)
            {
                if (rr.CheckCondition(logEvent))
                {
                    matchingRule = rr;
                    break;
                }
            }

            if (this.UseDefaultRowColoringRules && matchingRule == null)
            {
                foreach (RichTextBoxRowColoringRule rr in defaultRichTextBoxRowColoringRules)
                {
                    if (rr.CheckCondition(logEvent))
                    {
                        matchingRule = rr;
                        break;
                    }
                }
            }

            if (matchingRule == null)
            {
                matchingRule = RichTextBoxRowColoringRule.Default;
            }
            
            string logMessage = this.Layout.GetFormattedMessage(logEvent);

            this.FindRichTextBoxAndSendTheMessage(logMessage, matchingRule);
        }

        private void FindRichTextBoxAndSendTheMessage(string logMessage, RichTextBoxRowColoringRule rule)
        {
            Form form = null;
            bool createdForm = false;

            if (Form.ActiveForm != null && Form.ActiveForm.Name == this.FormName)
            {
                form = Form.ActiveForm;
            }

            if (form == null && Application.OpenForms[this.FormName] != null)
            {
                form = Application.OpenForms[this.FormName];
            }

            if (form == null)
            {
                form = FormHelper.CreateForm(this.FormName, 0, 0, true);
                createdForm = true;
            }

            RichTextBox rtbx = (RichTextBox)FormHelper.FindControl(this.ControlName, form, typeof(RichTextBox));

            if (rtbx == null && createdForm)
            {
                rtbx = FormHelper.CreateRichTextBox(this.ControlName, form);
            }
            else if (rtbx == null && !createdForm)
            {
                return;
            }

            rtbx.Invoke(new DelSendTheMessageToRichTextBox(this.SendTheMessageToRichTextBox), new object[] { rtbx, logMessage, rule });
        }

        private void SendTheMessageToRichTextBox(RichTextBox rtbx, string logMessage, RichTextBoxRowColoringRule rule)
        {
            int startIndex = rtbx.Text.Length;
            rtbx.SelectionStart = startIndex;
            rtbx.SelectionBackColor = this.GetColorFromString(rule.BackgroundColor, rtbx.BackColor);
            rtbx.SelectionColor = this.GetColorFromString(rule.FontColor, rtbx.ForeColor);
            rtbx.SelectionFont = new Font(rtbx.SelectionFont, rtbx.SelectionFont.Style ^ rule.Style);
            rtbx.AppendText(logMessage + "\n");
            rtbx.SelectionLength = rtbx.Text.Length - rtbx.SelectionStart;

            // find word to color
            foreach (RichTextBoxWordColoringRule wordRule in this.WordColoringRules)
            {
                MatchCollection mc = wordRule.CompiledRegex.Matches(rtbx.Text, startIndex);
                foreach (Match m in mc)
                {
                    rtbx.SelectionStart = m.Index;
                    rtbx.SelectionLength = m.Length;
                    rtbx.SelectionBackColor = this.GetColorFromString(wordRule.BackgroundColor, rtbx.BackColor);
                    rtbx.SelectionColor = this.GetColorFromString(wordRule.FontColor, rtbx.ForeColor);
                    rtbx.SelectionFont = new Font(rtbx.SelectionFont, rtbx.SelectionFont.Style ^ wordRule.Style);
                }
            }
        }

        private Color GetColorFromString(string color, Color defaultColor)
        {
            if (color == "Empty")
            {
                return defaultColor;
            }
            
            Color c = Color.FromName(color);
            if (c == Color.Empty)
            {
                return defaultColor;
            }
            
            return c;
        }
    }
}
#endif
