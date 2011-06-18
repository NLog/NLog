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

#if !MONO && !SILVERLIGHT && !NET_CF

namespace NLog.Internal
{
    using System;
    using System.Drawing;
    using System.Windows.Forms;

    /// <summary>
    /// Form helper methods.
    /// </summary>
    internal class FormHelper
    {
        /// <summary>
        /// Creates RichTextBox and docks in parentForm.
        /// </summary>
        /// <param name="name">Name of RichTextBox.</param>
        /// <param name="parentForm">Form to dock RichTextBox.</param>
        /// <returns>Created RichTextBox.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Objects are disposed elsewhere")]
        internal static RichTextBox CreateRichTextBox(string name, Form parentForm)
        {
            var rtb = new RichTextBox();
            rtb.Dock = System.Windows.Forms.DockStyle.Fill;
            rtb.Location = new System.Drawing.Point(0, 0);
            rtb.Name = name;
            rtb.Size = new System.Drawing.Size(parentForm.Width, parentForm.Height);
            parentForm.Controls.Add(rtb);
            return rtb;
        }

        /// <summary>
        /// Finds control embedded on searchControl.
        /// </summary>
        /// <param name="name">Name of the control.</param>
        /// <param name="searchControl">Control in which we're searching for control.</param>
        /// <returns>A value of null if no control has been found.</returns>
        internal static Control FindControl(string name, Control searchControl)
        {
            if (searchControl.Name == name)
            {
                return searchControl;
            }

            foreach (Control childControl in searchControl.Controls)
            {
                Control foundControl = FindControl(name, childControl);
                if (foundControl != null)
                {
                    return foundControl;
                }
            }

            return null;
        }

        /// <summary>
        /// Finds control of specified type embended on searchControl.
        /// </summary>
        /// <typeparam name="TControl">The type of the control.</typeparam>
        /// <param name="name">Name of the control.</param>
        /// <param name="searchControl">Control in which we're searching for control.</param>
        /// <returns>
        /// A value of null if no control has been found.
        /// </returns>
        internal static TControl FindControl<TControl>(string name, Control searchControl)
            where TControl : Control
        {
            if (searchControl.Name == name)
            {
                TControl foundControl = searchControl as TControl;
                if (foundControl != null)
                {
                    return foundControl;
                }
            }

            foreach (Control childControl in searchControl.Controls)
            {
                TControl foundControl = FindControl<TControl>(name, childControl);

                if (foundControl != null)
                {
                    return foundControl;
                }
            }

            return null;
        }

        /// <summary>
        /// Creates a form.
        /// </summary>
        /// <param name="name">Name of form.</param>
        /// <param name="width">Width of form.</param>
        /// <param name="height">Height of form.</param>
        /// <param name="show">Auto show form.</param>
        /// <param name="showMinimized">If set to <c>true</c> the form will be minimized.</param>
        /// <param name="toolWindow">If set to <c>true</c> the form will be created as tool window.</param>
        /// <returns>Created form.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "System.Windows.Forms.Control.set_Text(System.String)", Justification = "Does not need to be localized.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Objects are disposed elsewhere")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", Justification = "Using property names in message.")]
        internal static Form CreateForm(string name, int width, int height, bool show, bool showMinimized, bool toolWindow)
        {
            var f = new Form
            {
                Name = name,
                Text = "NLog",
                Icon = GetNLogIcon()
            };

#if !Smartphone
            if (toolWindow)
            {
                f.FormBorderStyle = FormBorderStyle.SizableToolWindow;
            }
#endif
            if (width > 0)
            {
                f.Width = width;
            }

            if (height > 0)
            {
                f.Height = height;
            }

            if (show)
            {
                if (showMinimized)
                {
                    f.WindowState = FormWindowState.Minimized;
                    f.Show();
                }
                else
                {
                    f.Show();
                }
            }

            return f;
        }

        private static Icon GetNLogIcon()
        {
            using (var stream = typeof(FormHelper).Assembly.GetManifestResourceStream("NLog.Resources.NLog.ico"))
            {
                return new Icon(stream);
            }
        }
    }
}
#endif
