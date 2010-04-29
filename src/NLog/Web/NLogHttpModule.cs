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

#if !NETCF

using System;
using System.Text;
using System.Web;

using NLog;
using NLog.Config;

namespace NLog.Web
{
    /// <summary>
    /// ASP.NET HttpModule that enables NLog to hook BeginRequest and EndRequest events easily.
    /// </summary>
    public class NLogHttpModule : IHttpModule
    {
        /// <summary>
        /// Event to be raised at the end of each HTTP Request.
        /// </summary>
        public static event EventHandler EndRequest;

        /// <summary>
        /// Event to be raised at the beginning of each HTTP Request.
        /// </summary>
        public static event EventHandler BeginRequest;

        /// <summary>
        /// Initializes the HttpModule
        /// </summary>
        /// <param name="application">ASP.NET application</param>
        public void Init(HttpApplication application)
        {
            application.BeginRequest += new EventHandler(this.BeginRequestHandler);
            application.EndRequest += new EventHandler(this.EndRequestHandler);
        }

        /// <summary>
        /// Disposes the module.
        /// </summary>
        public void Dispose()
        {
        }

        private void BeginRequestHandler(object sender, EventArgs args)
        {
            if (BeginRequest != null)
            {
                BeginRequest(sender, args);
            }
        }

        private void EndRequestHandler(object sender, EventArgs args)
        {
            if (EndRequest != null)
            {
                EndRequest(sender, args);
            }
        }
    }
}

#endif
