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
using System;
using System.Text;
using System.Security.Principal;

namespace NLog.LayoutRenderers
{
    /// <summary>
    /// Thread identity information (name and authentication information)
    /// </summary>
    [LayoutRenderer("identity")]
    public class IdentityLayoutRenderer: LayoutRenderer
    {
        private bool _name = true;
        private bool _authType = true;
        private bool _isAuthenticated = true;
        private bool _fsNormalize = false;
        private string _separator = ":";
 
        /// <summary>
        /// The separator to be used when concatenating 
        /// parts of identity information.
        /// </summary>
        [System.ComponentModel.DefaultValue(":")]
        public string Separator
        {
            get { return _separator; }
            set { _separator = value; }
        }

        /// <summary>
        /// Render Thread.CurrentPrincipal.Identity.Name.
        /// </summary>
        [System.ComponentModel.DefaultValue(true)]
        public bool Name
        {
            get { return _name; }
            set { _name = value; }
        }

        /// <summary>
        /// Render Thread.CurrentPrincipal.Identity.AuthenticationType.
        /// </summary>
        [System.ComponentModel.DefaultValue(true)]
        public bool AuthType
        {
            get { return _authType; }
            set { _authType = value; }
        }

        /// <summary>
        /// Render Thread.CurrentPrincipal.Identity.IsAuthenticated.
        /// </summary>
        [System.ComponentModel.DefaultValue(true)]
        public bool IsAuthenticated
        {
            get { return _isAuthenticated; }
            set { _isAuthenticated = value; }
        }

        /// <summary>
        /// When true the output of this renderer is modified so it can be used as a part of file path
        /// (illegal characters are replaced with '_')
        /// </summary>
        [System.ComponentModel.DefaultValue(false)]
        public bool FSNormalize
        {
            get { return _fsNormalize; }
            set { _fsNormalize = value; }
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
        /// Renders the specified identity information and appends it to the specified <see cref="StringBuilder" />.
        /// </summary>
        /// <param name="builder">The <see cref="StringBuilder"/> to append the rendered data to.</param>
        /// <param name="logEvent">Logging event.</param>
        protected internal override void Append(StringBuilder builder, LogEventInfo logEvent)
        {
            IPrincipal principal = System.Threading.Thread.CurrentPrincipal;
            if (principal != null)
            {
                IIdentity identity = principal.Identity;
                if (identity != null)
                {
                    StringBuilder sb2 = builder;
                    if (Padding != 0)
                        sb2 = new StringBuilder();
                    int sbstart = sb2.Length;
                    bool first = true;

                    if (_isAuthenticated)
                    {
                        if (!first)
                        {
                            sb2.Append(_separator);
                        }
                        if (identity.IsAuthenticated)
                        {
                            sb2.Append("auth");
                        }
                        else
                        {
                            sb2.Append("notauth");
                        }
                        first = false;
                    }

                    if (_authType)
                    {
                        if (!first)
                        {
                            sb2.Append(_separator);
                        }
                        sb2.Append(identity.AuthenticationType);
                        first = false;
                    }

                    if (_name)
                    {
                        if (!first)
                        {
                            sb2.Append(_separator);
                        }
                        sb2.Append(identity.Name);
                        first = false;
                    }

                    if (_fsNormalize)
                    {
                        for (int i=sbstart; i<sb2.Length; i++)
                        {
                            char c = sb2[i];
                            if (!Char.IsLetterOrDigit(c) && c != '_' && c != '-' && c != '.') sb2[i] = '_';
                        }
                    }
                    
                    if (Padding != 0)
                        builder.Append(ApplyPadding(sb2.ToString()));
                }
            }
        }
    }
}

#endif
