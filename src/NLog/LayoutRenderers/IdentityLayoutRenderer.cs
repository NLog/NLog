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

#if !NET_CF && !SILVERLIGHT

namespace NLog.LayoutRenderers
{
    using System;
    using System.ComponentModel;
    using System.Security.Principal;
    using System.Text;

    /// <summary>
    /// Thread identity information (name and authentication information).
    /// </summary>
    [LayoutRenderer("identity")]
    public class IdentityLayoutRenderer : LayoutRenderer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IdentityLayoutRenderer" /> class.
        /// </summary>
        public IdentityLayoutRenderer()
        {
            this.Name = true;
            this.AuthType = true;
            this.IsAuthenticated = true;
            this.FSNormalize = false;
            this.Separator = ":";
        }

        /// <summary>
        /// Gets or sets the separator to be used when concatenating 
        /// parts of identity information.
        /// </summary>
        [DefaultValue(":")]
        public string Separator { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to render Thread.CurrentPrincipal.Identity.Name.
        /// </summary>
        [DefaultValue(true)]
        public bool Name { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to render Thread.CurrentPrincipal.Identity.AuthenticationType.
        /// </summary>
        [DefaultValue(true)]
        public bool AuthType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to render Thread.CurrentPrincipal.Identity.IsAuthenticated.
        /// </summary>
        [DefaultValue(true)]
        public bool IsAuthenticated { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to modify the output of this renderer so it can be used as a part of file path
        /// (illegal characters are replaced with '_').
        /// </summary>
        [DefaultValue(false)]
        public bool FSNormalize { get; set; }

        /// <summary>
        /// Renders the specified identity information and appends it to the specified <see cref="StringBuilder" />.
        /// </summary>
        /// <param name="builder">The <see cref="StringBuilder"/> to append the rendered data to.</param>
        /// <param name="logEvent">Logging event.</param>
        protected override void Append(StringBuilder builder, LogEventInfo logEvent)
        {
            IPrincipal principal = System.Threading.Thread.CurrentPrincipal;
            if (principal != null)
            {
                IIdentity identity = principal.Identity;
                if (identity != null)
                {
                    int sbstart = builder.Length;
                    string separator = string.Empty;

                    if (this.IsAuthenticated)
                    {
                        builder.Append(separator);
                        separator = this.Separator;

                        if (identity.IsAuthenticated)
                        {
                            builder.Append("auth");
                        }
                        else
                        {
                            builder.Append("notauth");
                        }
                    }

                    if (this.AuthType)
                    {
                        builder.Append(separator);
                        separator = this.Separator;
                        builder.Append(identity.AuthenticationType);
                    }

                    if (this.Name)
                    {
                        builder.Append(separator);
                        separator = this.Separator;
                        builder.Append(identity.Name);
                    }

                    if (this.FSNormalize)
                    {
                        for (int i = sbstart; i < builder.Length; i++)
                        {
                            char c = builder[i];
                            if (!Char.IsLetterOrDigit(c) && c != '_' && c != '-' && c != '.')
                            {
                                builder[i] = '_';
                            }
                        }
                    }
                }
            }
        }
    }
}

#endif
