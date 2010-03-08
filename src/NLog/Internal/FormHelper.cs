// 
// Copyright (c) 2004-2010 Jaroslaw Kowalski <jaak@jkowalski.net>
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

#if !MONO && !SILVERLIGHT

using System;
using System.Windows.Forms;

namespace NLog.Internal
{
    /// <summary>
    /// Form helper methods.
    /// </summary>
    internal class FormHelper
    {
#if !NET_CF
        /// <summary>
        /// Creates RichTextBox and docks in parentForm.
        /// </summary>
        /// <param name="name">Name of RichTextBox.</param>
        /// <param name="parentForm">Form to dock RichTextBox.</param>
        /// <returns>Created RichTextBox.</returns>
        internal static RichTextBox CreateRichTextBox(string name, Form parentForm)
        {
            RichTextBox rtb = new RichTextBox();
            rtb.Dock = System.Windows.Forms.DockStyle.Fill;
            rtb.Location = new System.Drawing.Point(0, 0);
            rtb.Name = name;
            rtb.Size = new System.Drawing.Size(parentForm.Width, parentForm.Height);
            parentForm.Controls.Add(rtb);
            return rtb;
        }
#endif
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
        /// <param name="name">Name of the control.</param>
        /// <param name="searchControl">Control in which we're searching for control.</param>
        /// <param name="controlType">Type of control to search for.</param>
        /// <returns>A value of null if no control has been found.</returns>
        internal static Control FindControl(string name, Control searchControl, Type controlType)
        {
            if ((searchControl.Name == name) && (searchControl.GetType() == controlType))
            {
                return searchControl;
            }

            foreach (Control childControl in searchControl.Controls)
            {
                Control foundControl = FindControl(name, childControl, controlType);

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
        /// <returns>Created form.</returns>
        internal static Form CreateForm(string name, int width, int height, bool show)
        {
            Form f = new Form();
            f.Name = name;
            f.Text = "NLog";

#if !Smartphone
            f.FormBorderStyle = FormBorderStyle.SizableToolWindow;
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
                f.Show();
            }

            return f;
        }
    }
}
#endif
