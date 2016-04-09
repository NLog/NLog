// 
// Copyright (c) 2004-2016 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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

namespace NLog.Internal.NetworkSenders
{
    using System;
    using System.Net;
    using NLog.Common;

    /// <summary>
    /// Network sender which uses HTTP or HTTPS POST.
    /// </summary>
    internal class HttpNetworkSender : NetworkSender
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HttpNetworkSender"/> class.
        /// </summary>
        /// <param name="url">The network URL.</param>
        public HttpNetworkSender(string url)
            : base(url)
        {
        }

        /// <summary>
        /// Actually sends the given text over the specified protocol.
        /// </summary>
        /// <param name="bytes">The bytes to be sent.</param>
        /// <param name="offset">Offset in buffer.</param>
        /// <param name="length">Number of bytes to send.</param>
        /// <param name="asyncContinuation">The async continuation to be invoked after the buffer has been sent.</param>
        /// <remarks>To be overridden in inheriting classes.</remarks>
        protected override void DoSend(byte[] bytes, int offset, int length, AsyncContinuation asyncContinuation)
        {
            var webRequest = WebRequest.Create(new Uri(this.Address));
            webRequest.Method = "POST";

            AsyncCallback onResponse =
                r =>
                {
                    try
                    {
                        using (var response = webRequest.EndGetResponse(r))
                        {
                        }

                        // completed fine
                        asyncContinuation(null);
                    }
                    catch (Exception ex)
                    {
                        if (ex.MustBeRethrown())
                        {
                            throw;
                        }

                        asyncContinuation(ex);
                    }
                };

            AsyncCallback onRequestStream =
                r =>
                {
                    try
                    {
                        using (var stream = webRequest.EndGetRequestStream(r))
                        {
                            stream.Write(bytes, offset, length);
                        }

                        webRequest.BeginGetResponse(onResponse, null);
                    }
                    catch (Exception ex)
                    {
                        if (ex.MustBeRethrown())
                        {
                            throw;
                        }

                        asyncContinuation(ex);
                    }
                };

            webRequest.BeginGetRequestStream(onRequestStream, null);
        }
    }
}