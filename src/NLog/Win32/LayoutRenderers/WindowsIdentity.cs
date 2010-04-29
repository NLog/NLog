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

#if !(NETCF)
using NLog.Config;
using System;
using System.Text;
using System.Security.Principal;

namespace NLog.Win32.LayoutRenderers
{
    /// <summary>
    /// Thread Windows identity information (username)
    /// </summary>
    [LayoutRenderer("windows-identity")]
    [SupportedRuntime(OS=RuntimeOS.Windows)]
    [SupportedRuntime(OS=RuntimeOS.WindowsNT)]
    public class WindowsIdentityLayoutRenderer: LayoutRenderer
    {
        private bool _includeDomain = true;
        private bool _includeUserName = true;

        /// <summary>
        /// Whether domain name should be included.
        /// </summary>
        [System.ComponentModel.DefaultValue(true)]
        public bool Domain
        {
            get { return _includeDomain; }
            set { _includeDomain = value; }
        }

        /// <summary>
        /// Whether username should be included.
        /// </summary>
        [System.ComponentModel.DefaultValue(true)]
        public bool UserName
        {
            get { return _includeUserName; }
            set { _includeUserName = value; }
        }

        /// <summary>
        /// Returns the estimated number of characters that are needed to
        /// hold the rendered value for the specified logging event.
        /// </summary>
        /// <param name="logEvent">Logging event information.</param>
        /// <returns>The number of characters.</returns>
        /// <remarks>
        /// If the exact number is not known or
        /// expensive to calculate this function should return a rough estimate
        /// that's big enough in most cases, but not too big, in order to conserve memory.
        /// </remarks>
        protected internal override int GetEstimatedBufferSize(LogEventInfo logEvent)
        {
            return 32;
        }

        /// <summary>
        /// Renders the current thread windows identity information and appends it to the specified <see cref="StringBuilder" />.
        /// </summary>
        /// <param name="builder">The <see cref="StringBuilder"/> to append the rendered data to.</param>
        /// <param name="logEvent">Logging event.</param>
        protected internal override void Append(StringBuilder builder, LogEventInfo logEvent)
        {
            WindowsIdentity currentIdentity = WindowsIdentity.GetCurrent();
            if (currentIdentity != null)
            {
                string output = "";

                if (UserName)
                {
                    if (Domain)
                    {
                        // username && domain

                        output = currentIdentity.Name;
                    }
                    else
                    {
                        // user name but no domain

                        int pos = currentIdentity.Name.LastIndexOf('\\');
                        if (pos >= 0)
                            output = currentIdentity.Name.Substring(pos + 1);
                        else
                            output = currentIdentity.Name;
                    }
                }
                else
                {
                    // no username

                    if (!Domain)
                    {
                        // nothing to output
                        return;
                    }

                    int pos = currentIdentity.Name.IndexOf('\\');
                    if (pos >= 0)
                        output = currentIdentity.Name.Substring(0, pos);
                    else
                        output = currentIdentity.Name;
                }
                builder.Append(ApplyPadding(output));
            }
        }
    }
}

#endif
