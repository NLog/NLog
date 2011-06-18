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
    using System.Windows.Forms;
    using NLog.Config;
    using NLog.Internal;

    /// <summary>
    /// Logs text to Windows.Forms.Control.Text property control of specified Name.
    /// </summary>
    /// <example>
    /// <p>
    /// To set up the target in the <a href="config.html">configuration file</a>, 
    /// use the following syntax:
    /// </p>
    /// <code lang="XML" source="examples/targets/Configuration File/FormControl/NLog.config" />
    /// <p>
    /// The result is:
    /// </p>
    /// <img src="examples/targets/Screenshots/FormControl/FormControl.gif" />
    /// <p>
    /// To set up the log target programmatically similar to above use code like this:
    /// </p>
    /// <code lang="C#" source="examples/targets/Configuration API/FormControl/Form1.cs" />,
    /// </example>
    [Target("FormControl")]
    public sealed class FormControlTarget : TargetWithLayout
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FormControlTarget" /> class.
        /// </summary>
        /// <remarks>
        /// The default value of the layout is: <code>${longdate}|${level:uppercase=true}|${logger}|${message}</code>
        /// </remarks>
        public FormControlTarget()
        {
            this.Append = true;
        }

        private delegate void DelSendTheMessageToFormControl(Control ctrl, string logMessage);

        /// <summary>
        /// Gets or sets the name of control to which NLog will log write log text.
        /// </summary>
        /// <docgen category='Form Options' order='10' />
        [RequiredParameter]
        public string ControlName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether log text should be appended to the text of the control instead of overwriting it. </summary>
        /// <docgen category='Form Options' order='10' />
        [DefaultValue(true)]
        public bool Append { get; set; }

        /// <summary>
        /// Gets or sets the name of the Form on which the control is located.
        /// </summary>
        /// <docgen category='Form Options' order='10' />
        public string FormName { get; set; }

        /// <summary>
        /// Log message to control.
        /// </summary>
        /// <param name="logEvent">
        /// The logging event.
        /// </param>
        protected override void Write(LogEventInfo logEvent)
        {
            string logMessage = this.Layout.Render(logEvent);
            
            this.FindControlAndSendTheMessage(logMessage);
        }

        private void FindControlAndSendTheMessage(string logMessage)
        {
            Form form = null;

            if (Form.ActiveForm != null)
            {
                form = Form.ActiveForm;
            }

            if (Application.OpenForms[this.FormName] != null)
            {
                form = Application.OpenForms[this.FormName];
            }

            if (form == null)
            {
                return;
            }

            Control ctrl = FormHelper.FindControl(this.ControlName, form);

            if (ctrl == null)
            {
                return;
            }

            ctrl.Invoke(new DelSendTheMessageToFormControl(this.SendTheMessageToFormControl), new object[] { ctrl, logMessage });
        }

        private void SendTheMessageToFormControl(Control ctrl, string logMessage)
        {
            if (this.Append)
            {
                ctrl.Text += logMessage;
            }
            else
            {
                ctrl.Text = logMessage;
            }
        }
    }
}
#endif
