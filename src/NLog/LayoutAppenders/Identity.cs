// 
// Copyright (c) 2004 Jaroslaw Kowalski <jaak@polbox.com>
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
// * Neither the name of the Jaroslaw Kowalski nor the names of its 
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

namespace NLog.LayoutAppenders
{
    [LayoutAppender("identity")]
    public class IdentityLayoutAppender: LayoutAppender
    {
        private bool _name = true;
        private bool _authType = true;
        private bool _isAuthenticated = true;
        private string _separator = ":";

        public string Separator
        {
            get
            {
                return _separator;
            }
            set
            {
                _separator = value;
            }
        }

        public bool Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
            }
        }

        public bool AuthType
        {
            get
            {
                return _authType;
            }
            set
            {
                _authType = value;
            }
        }

        public bool IsAuthenticated
        {
            get
            {
                return _isAuthenticated;
            }
            set
            {
                _isAuthenticated = value;
            }
        }

        protected internal override int GetEstimatedBufferSize(LogEventInfo ev)
        {
            return 32;
        }

        protected internal override void Append(StringBuilder builder, LogEventInfo ev)
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

                    if (Padding != 0)
                        builder.Append(ApplyPadding(sb2.ToString()));
                }
            }
        }
    }
}

#endif
