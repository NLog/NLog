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
#if !NETCF

using System;
using System.Text;
using System.Web;

namespace NLog.LayoutAppenders
{
    [LayoutAppender("aspnet-request")]
    public class ASPNETSessioValueLayoutAppender : LayoutAppender
    {
        private string _queryStringKey;
        private string _formKey;
        private string _cookie;
        private string _item;
        private string _serverVariable;

        public string Item
        {
            get
            {
                return _item;
            }
            set
            {
                _item = value;
            }
        }

        public string QueryString
        {
            get
            {
                return _queryStringKey;
            }
            set
            {
                _queryStringKey = value;
            }
        }

        public string Form
        {
            get
            {
                return _formKey;
            }
            set
            {
                _formKey = value;
            }
        }

        public string Cookie
        {
            get
            {
                return _cookie;
            }
            set
            {
                _cookie = value;
            }
        }

        public string ServerVariable
        {
            get
            {
                return _serverVariable;
            }
            set
            {
                _serverVariable = value;
            }
        }

        protected internal override int GetEstimatedBufferSize(LogEventInfo ev)
        {
            return 64;
        }

        protected internal override void Append(StringBuilder builder, LogEventInfo ev)
        {
            HttpContext context = HttpContext.Current;
            if (context == null)
                return ;

            if (_queryStringKey != null)
            {
                builder.Append(context.Request.QueryString[_queryStringKey]);
            }
            else if (_formKey != null)
            {
                builder.Append(context.Request.Form[_formKey]);
            }
            else if (_cookie != null)
            {
                builder.Append(context.Request.Cookies[_cookie]);
            }
            else if (_serverVariable != null)
            {
                builder.Append(context.Request.ServerVariables[_cookie]);
            }
            else if (_item != null)
            {
                builder.Append(context.Request.Cookies[_item]);
            }
        }
    }
}

#endif
