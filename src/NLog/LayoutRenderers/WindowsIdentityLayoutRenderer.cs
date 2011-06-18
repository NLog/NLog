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

#if !NET_CF && !SILVERLIGHT

namespace NLog.LayoutRenderers
{
    using System.ComponentModel;
    using System.Security.Principal;
    using System.Text;

    /// <summary>
    /// Thread Windows identity information (username).
    /// </summary>
    [LayoutRenderer("windows-identity")]
    public class WindowsIdentityLayoutRenderer : LayoutRenderer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WindowsIdentityLayoutRenderer" /> class.
        /// </summary>
        public WindowsIdentityLayoutRenderer()
        {
            this.UserName = true;
            this.Domain = true;
        }

        /// <summary>
        /// Gets or sets a value indicating whether domain name should be included.
        /// </summary>
        /// <docgen category='Rendering Options' order='10' />
        [DefaultValue(true)]
        public bool Domain { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether username should be included.
        /// </summary>
        /// <docgen category='Rendering Options' order='10' />
        [DefaultValue(true)]
        public bool UserName { get; set; }

        /// <summary>
        /// Renders the current thread windows identity information and appends it to the specified <see cref="StringBuilder" />.
        /// </summary>
        /// <param name="builder">The <see cref="StringBuilder"/> to append the rendered data to.</param>
        /// <param name="logEvent">Logging event.</param>
        protected override void Append(StringBuilder builder, LogEventInfo logEvent)
        {
            WindowsIdentity currentIdentity = WindowsIdentity.GetCurrent();
            if (currentIdentity != null)
            {
                string output = string.Empty;

                if (this.UserName)
                {
                    if (this.Domain)
                    {
                        // username && domain
                        output = currentIdentity.Name;
                    }
                    else
                    {
                        // user name but no domain
                        int pos = currentIdentity.Name.LastIndexOf('\\');
                        if (pos >= 0)
                        {
                            output = currentIdentity.Name.Substring(pos + 1);
                        }
                        else
                        {
                            output = currentIdentity.Name;
                        }
                    }
                }
                else
                {
                    // no username
                    if (!this.Domain)
                    {
                        // nothing to output
                        return;
                    }

                    int pos = currentIdentity.Name.IndexOf('\\');
                    if (pos >= 0)
                    {
                        output = currentIdentity.Name.Substring(0, pos);
                    }
                    else
                    {
                        output = currentIdentity.Name;
                    }
                }

                builder.Append(output);
            }
        }
    }
}

#endif
