// 
// Copyright (c) 2004-2020 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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

using System;
using System.IO;
using System.Linq;
using System.Net;
using NSubstitute;

namespace NLog.UnitTests.Mocks
{
    public class WebRequestMock : WebRequest, IDisposable
    {
        public Uri RequestedAddress { get; set; }

        public bool FirstRequestMustFail { get; set; }

        public MemoryStream RequestStream => _requestStream;
        private readonly ManualDisposableMemoryStream _requestStream = new ManualDisposableMemoryStream();

        /// <inheritdoc />
        public WebRequestMock()
        {
        }

        #region Overrides of WebRequest

        /// <inheritdoc />
        public override WebResponse EndGetResponse(IAsyncResult asyncResult)
        {
            if (FirstRequestMustFail)
            {
                FirstRequestMustFail = false;
                RequestStream.Position = 0;
                RequestStream.SetLength(0);
                System.Threading.Thread.Sleep(50);
                throw new ArgumentNullException("You are doomed");
            }

            var responseStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("new response 1"));
            var webResponseMock = Substitute.For<WebResponse>();
            webResponseMock.GetResponseStream().Returns(responseStream);
            return webResponseMock;
        }

        /// <inheritdoc />
        public override IAsyncResult BeginGetRequestStream(AsyncCallback callback, object state)
        {
            var result = CreateAsyncResultMock(state);
            System.Threading.Tasks.Task.Run(() => callback(result));
            return result;
        }

        /// <inheritdoc />
        public override Stream EndGetRequestStream(IAsyncResult asyncResult)
        {
            return RequestStream;
        }

        /// <inheritdoc />
        public override IAsyncResult BeginGetResponse(AsyncCallback callback, object state)
        {
            var result = CreateAsyncResultMock(state);
            callback(result);
            return result;
        }

        /// <inheritdoc />
        public override string Method { get; set; }

        #endregion

        public string GetRequestContentAsString()
        {
            var content = RequestStream.ToArray();
            var contentAsString = System.Text.Encoding.UTF8.GetString(content);
            return contentAsString;
        }

        private static IAsyncResult CreateAsyncResultMock(object state)
        {
            var asyncResult = Substitute.For<IAsyncResult>();
            asyncResult.AsyncState.Returns(state);
            return asyncResult;
        }

        #region IDisposable

        /// <inheritdoc />
        public void Dispose()
        {
            _requestStream?.RealDispose();
        }

        #endregion
    }
}
