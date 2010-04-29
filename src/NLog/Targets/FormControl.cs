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

using System.Windows.Forms;

using NLog.Config;
using NLog.Internal;

namespace NLog.Targets
{
    /// <summary>
    /// Log text to Windows.Forms.Control.Text property control of specified Name
    /// </summary>
    /// <example>
    /// <p>
    /// To set up the target in the <a href="config.html">configuration file</a>, 
    /// use the following syntax:
    /// </p>
    /// <code lang="XML" src="examples/targets/Configuration File/FormControl/NLog.config" />
    /// <p>
    /// The result is:
    /// </p>
    /// <img src="examples/targets/Screenshots/FormControl/FormControl.gif" />
    /// <p>
    /// To set up the log target programmatically similar to above use code like this:
    /// </p>
    /// <code lang="C#" src="examples/targets/Configuration API/FormControl/Form1.cs" />,
    /// </example>
    [Target("FormControl")]
    [SupportedRuntime(Framework=RuntimeFramework.DotNetFramework, MinRuntimeVersion="1.1")]
    public sealed class FormControlTarget : TargetWithLayout
    {
        private string _controlName;
        private bool _append = true;
        private string _formName;

        /// <summary>
        /// Name of control to which Nlog will log
        /// </summary>
        [RequiredParameter]
        public string ControlName
        {
            get { return _controlName; }
            set { _controlName = value; }
        }

        /// <summary>
        /// Setting to tell to append or overwrite the Text property of control
        /// </summary>
        public bool Append
        {
            get { return _append; }
            set { _append = value; }
        }

        /// <summary>
        /// Name of the Form on which the control is located.
        /// </summary>
        public string FormName
        {
            get { return _formName; }
            set { _formName = value; }
        }

        /// <summary>
        /// Log message to control
        /// </summary>
        /// <param name="logEvent">The logging event.</param>
        protected internal override void Write(LogEventInfo logEvent)
        {
            string logMessage = CompiledLayout.GetFormattedMessage(logEvent);
            
            FindControlAndSendTheMessage(logMessage);
        }

        private void FindControlAndSendTheMessage(string logMessage)
        {
            Form form = null;

            if (Form.ActiveForm != null)
                form = Form.ActiveForm;

#if DOTNET_2_0
            if (Application.OpenForms[FormName] != null)
                form = Application.OpenForms[FormName];
#endif
            if (form == null)
                return;

            Control ctrl = FormHelper.FindControl(ControlName, form);

            if (ctrl == null)
                return;

            ctrl.Invoke(new DelSendTheMessageToFormControl(SendTheMessageToFormControl), new object[] { ctrl, logMessage });
        }

        private delegate void DelSendTheMessageToFormControl(Control ctrl, string logMessage);

        private void SendTheMessageToFormControl(Control ctrl, string logMessage)
        {
            if (Append)
                ctrl.Text += logMessage;
            else
                ctrl.Text = logMessage;
        }
    }
}
#endif
