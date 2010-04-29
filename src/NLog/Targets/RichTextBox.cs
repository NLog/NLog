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
using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

using System.Windows.Forms;
using System.Drawing;

using NLog.Config;
using NLog.Internal;

namespace NLog.Targets
{
    /// <summary>
    /// Log text to Text property of  RichTextBox of specified Name
    /// </summary>
    /// <example>
    /// <p>
    /// To set up the target in the <a href="config.html">configuration file</a>, 
    /// use the following syntax:
    /// </p>
    /// <code lang="XML" src="examples/targets/Configuration File/RichTextBox/Simple/NLog.config" />
    /// <p>
    /// The result is:
    /// </p>
    /// <img src="examples/targets/Screenshots/RichTextBox/Simple.gif" />
    /// <p>
    /// To set up the target with coloring rules in the <a href="config.html">configuration file</a>, 
    /// use the following syntax:
    /// </p>
    /// <code lang="XML" src="examples/targets/Configuration File/RichTextBox/RowColoring/NLog.config" />
    /// <code lang="XML" src="examples/targets/Configuration File/RichTextBox/WordColoring/NLog.config" />
    /// <p>
    /// The result is:
    /// </p>
    /// <img src="examples/targets/Screenshots/RichTextBox/RowColoring.gif" />
    /// <img src="examples/targets/Screenshots/RichTextBox/WordColoring.gif" />
    /// <p>
    /// To set up the log target programmatically similar to above use code like this:
    /// </p>
    /// <code lang="C#" src="examples/targets/Configuration API/RichTextBox/Simple/Form1.cs" />,
    /// <code lang="C#" src="examples/targets/Configuration API/RichTextBox/RowColoring/Form1.cs" /> for RowColoring,
    /// <code lang="C#" src="examples/targets/Configuration API/RichTextBox/WordColoring/Form1.cs" /> for WordColoring
    /// </example>
    [Target("RichTextBox")]
    [SupportedRuntime(Framework = RuntimeFramework.DotNetFramework, MinRuntimeVersion = "1.1")]
    public sealed class RichTextBoxTarget : TargetWithLayout
    {
        private string _controlName;
        private string _formName;
        private bool _useDefaultRowColoringRules = false;
        private RichTextBoxRowColoringRuleCollection _richTextBoxRowColoringRules = new RichTextBoxRowColoringRuleCollection();
        private RichTextBoxWordColoringRuleCollection _richTextBoxWordColoringRules = new RichTextBoxWordColoringRuleCollection();
        private static RichTextBoxRowColoringRuleCollection _defaultRichTextBoxRowColoringRules = new RichTextBoxRowColoringRuleCollection();

        static RichTextBoxTarget()
        {
            _defaultRichTextBoxRowColoringRules.Add(new RichTextBoxRowColoringRule("level == LogLevel.Fatal", "White", "Red", FontStyle.Bold));
            _defaultRichTextBoxRowColoringRules.Add(new RichTextBoxRowColoringRule("level == LogLevel.Error", "Red", "Empty", FontStyle.Bold | FontStyle.Italic));
            _defaultRichTextBoxRowColoringRules.Add(new RichTextBoxRowColoringRule("level == LogLevel.Warn", "Orange", "Empty", FontStyle.Underline));
            _defaultRichTextBoxRowColoringRules.Add(new RichTextBoxRowColoringRule("level == LogLevel.Info", "Black", "Empty"));
            _defaultRichTextBoxRowColoringRules.Add(new RichTextBoxRowColoringRule("level == LogLevel.Debug", "Gray", "Empty"));
            _defaultRichTextBoxRowColoringRules.Add(new RichTextBoxRowColoringRule("level == LogLevel.Trace", "DarkGray", "Empty", FontStyle.Italic));
        }

        /// <summary>
        /// Name of RichTextBox to which Nlog will log
        /// </summary>
        [RequiredParameter]
        public string ControlName
        {
            get { return _controlName; }
            set { _controlName = value; }
        }

        /// <summary>
        /// Name of the Form on which the control is located. 
        /// If there is no open form of a specified name than NLog will create a new one.
        /// </summary>
        public string FormName
        {
            get { return _formName; }
            set { _formName = value; }
        }
        
        /// <summary>
        /// Use default coloring rules
        /// </summary>
        [System.ComponentModel.DefaultValue(false)]
        public bool UseDefaultRowColoringRules
        {
            get { return _useDefaultRowColoringRules; }
            set { _useDefaultRowColoringRules = value; }
        }

        /// <summary>
        /// Row coloring rules.
        /// </summary>
        [ArrayParameter(typeof(RichTextBoxRowColoringRule), "row-coloring")]
        public RichTextBoxRowColoringRuleCollection RowColoringRules
        {
            get { return _richTextBoxRowColoringRules; }
        }

        /// <summary>
        /// Word highlighting rules.
        /// </summary>
        [ArrayParameter(typeof(RichTextBoxWordColoringRule), "word-coloring")]
        public RichTextBoxWordColoringRuleCollection WordColoringRules
        {
            get { return _richTextBoxWordColoringRules; }
        }
        
        /// <summary>
        /// Log message to RichTextBox
        /// </summary>
        /// <param name="logEvent">The logging event.</param>
        protected internal override void Write(LogEventInfo logEvent)
        {
            RichTextBoxRowColoringRule matchingRule = null;

            foreach (RichTextBoxRowColoringRule rr in RowColoringRules)
            {
                if (rr.CheckCondition(logEvent))
                {
                    matchingRule = rr;
                    break;
                }
            }

            if (UseDefaultRowColoringRules && matchingRule == null)
            {
                foreach (RichTextBoxRowColoringRule rr in _defaultRichTextBoxRowColoringRules)
                {
                    if (rr.CheckCondition(logEvent))
                    {
                        matchingRule = rr;
                        break;
                    }
                }
            }

            if (matchingRule == null)
                matchingRule = RichTextBoxRowColoringRule.Default;
            
            string logMessage = CompiledLayout.GetFormattedMessage(logEvent);

            FindRichTextBoxAndSendTheMessage(logMessage, matchingRule);
        }

        private void FindRichTextBoxAndSendTheMessage(string logMessage, RichTextBoxRowColoringRule rule)
        {
            Form form = null;
            bool createdForm = false;

            if (Form.ActiveForm != null && Form.ActiveForm.Name == FormName)
            {
                form = Form.ActiveForm;
            }

#if DOTNET_2_0
            if (form == null && Application.OpenForms[FormName] != null)
                form = Application.OpenForms[FormName];
#endif
            if (form == null)
            {
                form = FormHelper.CreateForm(FormName, 0, 0, true);
                createdForm = true;
            }

            RichTextBox rtbx = (RichTextBox)FormHelper.FindControl(ControlName, form, typeof(RichTextBox));

            if (rtbx == null && createdForm)
                rtbx = FormHelper.CreateRichTextBox(ControlName, form);
            else if (rtbx == null && !createdForm)
                return;

            rtbx.Invoke(new DelSendTheMessageToRichTextBox(SendTheMessageToRichTextBox), new object[] { rtbx, logMessage, rule });
        }

        private delegate void DelSendTheMessageToRichTextBox(RichTextBox rtbx, string logMessage, RichTextBoxRowColoringRule rule);

        private void SendTheMessageToRichTextBox(RichTextBox rtbx, string logMessage, RichTextBoxRowColoringRule rule)
        {
            int startIndex = rtbx.Text.Length;
            rtbx.SelectionStart = startIndex;
#if DOTNET_2_0
            rtbx.SelectionBackColor = GetColorFromString(rule.BackgroundColor, rtbx.BackColor);
#endif
            rtbx.SelectionColor = GetColorFromString(rule.FontColor, rtbx.ForeColor);
            rtbx.SelectionFont = new Font(rtbx.SelectionFont, rtbx.SelectionFont.Style ^ rule.Style);
            rtbx.AppendText(logMessage + "\n");
            rtbx.SelectionLength = rtbx.Text.Length - rtbx.SelectionStart;

            // find word to color
            foreach (RichTextBoxWordColoringRule wordRule in WordColoringRules)
            {
                MatchCollection mc = wordRule.CompiledRegex.Matches(rtbx.Text, startIndex);
                foreach (Match m in mc)
                {
                    rtbx.SelectionStart = m.Index;
                    rtbx.SelectionLength = m.Length;
#if DOTNET_2_0
                    rtbx.SelectionBackColor = GetColorFromString(wordRule.BackgroundColor, rtbx.BackColor);
#endif
                    rtbx.SelectionColor = GetColorFromString(wordRule.FontColor, rtbx.ForeColor);
                    rtbx.SelectionFont = new Font(rtbx.SelectionFont, rtbx.SelectionFont.Style ^ wordRule.Style);
                }
            }
        }

        private Color GetColorFromString(string color, Color defaultColor)
        {
            if (color == "Empty") return defaultColor;
            
            Color c = Color.FromName(color);
            if (c == Color.Empty) return defaultColor;
            
            return c;
        }
    }
}
#endif
