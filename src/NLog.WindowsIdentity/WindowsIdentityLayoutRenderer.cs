// 
// Copyright (c) 2004-2021 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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

namespace NLog.LayoutRenderers
{
    using System.Security.Principal;
    using System.Text;

    /// <summary>
    /// Thread Windows identity information (username).
    /// </summary>
    [LayoutRenderer("windows-identity")]
    public class WindowsIdentityLayoutRenderer : LayoutRenderer
    {
        /// <summary>
        /// Gets or sets a value indicating whether domain name should be included.
        /// </summary>
        /// <docgen category='Layout Options' order='10' />
        public bool Domain { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether username should be included.
        /// </summary>
        /// <docgen category='Layout Options' order='10' />
        public bool UserName { get; set; } = true;

        /// <inheritdoc/>
        protected override void Append(StringBuilder builder, LogEventInfo logEvent)
        {
            var currentIdentity = GetValue();
            if (currentIdentity != null)
            {
                string output;

                if (UserName)
                {
                    output = Domain ? GetUserNameWithDomain(currentIdentity) : GetUserNameWithoutDomain(currentIdentity);
                }
                else
                {
                    // no username
                    if (!Domain)
                    {
                        // nothing to output
                        return;
                    }

                    output = GetDomainOnly(currentIdentity);
                }

                builder.Append(output);
            }
        }

        private static string GetDomainOnly(WindowsIdentity currentIdentity)
        {
            string output;
            int pos = currentIdentity.Name.IndexOf('\\');
            if (pos >= 0)
            {
                output = currentIdentity.Name.Substring(0, pos);
            }
            else
            {
                output = currentIdentity.Name;
            }

            return output;
        }

        private static string GetUserNameWithoutDomain(WindowsIdentity currentIdentity)
        {
            string output;
            int pos = currentIdentity.Name.LastIndexOf('\\');
            if (pos >= 0)
            {
                output = currentIdentity.Name.Substring(pos + 1);
            }
            else
            {
                output = currentIdentity.Name;
            }

            return output;
        }

        private static string GetUserNameWithDomain(WindowsIdentity currentIdentity)
        {
            return currentIdentity.Name;
        }

        private static WindowsIdentity GetValue()
        {
            WindowsIdentity currentIdentity = WindowsIdentity.GetCurrent();
            return currentIdentity;
        }
    }
}
