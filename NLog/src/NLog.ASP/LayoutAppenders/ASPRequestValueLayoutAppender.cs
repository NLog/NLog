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

using System;
using System.Text;
using System.Runtime.InteropServices;

using NLog.LayoutAppenders;

namespace NLog.ASP.LayoutAppenders
{
    [LayoutAppender("asp-request")]
    public class ASPRequestValueLayoutAppender : LayoutAppender
    {
        private string _queryStringKey;
        private string _formKey;
        private string _cookie;
        private string _item;
        private string _serverVariable;

        public string Item
        {
            get { return _item; }
            set { _item = value; }
        }

        public string QueryString
        {
            get { return _queryStringKey; }
            set { _queryStringKey = value; }
        }

        public string Form
        {
            get { return _formKey; }
            set { _formKey = value; }
        }

        public string Cookie
        {
            get { return _cookie; }
            set { _cookie = value; }
        }

        public string ServerVariable
        {
            get { return _serverVariable; }
            set { _serverVariable = value; }
        }

        protected override int GetEstimatedBufferSize(LogEventInfo ev)
        {
            return 64;
        }

        private string GetItem(ASPHelper.IRequestDictionary dict, string key) {
            object retVal = null;
            object o = dict.GetItem(key);
            ASPHelper.IStringList sl = o as ASPHelper.IStringList;
            if (sl != null) {
                if (sl.GetCount() > 0) {
                    retVal = sl.GetItem(1);
                }
                Marshal.ReleaseComObject(sl);
            }
            else return o.GetType().ToString();
            return Convert.ToString(retVal);
        }
        
        protected override void Append(StringBuilder builder, LogEventInfo ev)
        {
            ASPHelper.IRequest request = ASPHelper.GetRequestObject();
            if (request != null) {
                if (_queryStringKey != null) {
                    builder.Append(GetItem(request.GetQueryString(), _queryStringKey));
                } else if (_formKey != null) {
                    builder.Append(GetItem(request.GetForm(), _formKey));
                } else if (_cookie != null) {
                    builder.Append(GetItem(request.GetCookies(), _cookie));
                } else if (_serverVariable != null) {
                    builder.Append(GetItem(request.GetServerVariables(), _serverVariable));
                } else if (_item != null) {
                    ASPHelper.IDispatch o = request.GetItem(_item);
                    ASPHelper.IStringList sl = o as ASPHelper.IStringList;
                    if (sl != null) {
                        if (sl.GetCount() > 0) {
                            builder.Append(sl.GetItem(1));
                        }
                        Marshal.ReleaseComObject(sl);
                    }
                }

                Marshal.ReleaseComObject(request);
            }
        }
    }
}
