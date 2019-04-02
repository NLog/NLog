// 
// Copyright (c) 2004-2019 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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
using System.Linq;
using System.Security.Authentication;
using NLog.Config;
using NLog.Internal.NetworkSenders;
using NLog.Targets;
using NLog.UnitTests.Mocks;
using Xunit;

namespace NLog.UnitTests.Internal.NetworkSenders
{
    public class HttpNetworkSenderTests : NLogTestBase
    {
        [Fact]
        public void HttpHappyPathTest()
        {
            // Arrange
            var networkTarget = new NetworkTarget("target1")
            {
                Address = "http://test.with.mock",
                Layout = "${logger}|${message}|${exception}"
            };
            var senderFactoryWithHttpMocks = new SenderFactoryWithHttpMocks();
            networkTarget.SenderFactory = senderFactoryWithHttpMocks;

            var logFactory = new LogFactory();
            var config = new LoggingConfiguration(logFactory);
            config.AddRuleForAllLevels(networkTarget);
            logFactory.Configuration = config;

            var logger = logFactory.GetLogger("HttpHappyPathTestLogger");

            // Act
            logger.Info("test message1");
            logFactory.Flush();

            // Assert
            var mock = senderFactoryWithHttpMocks.WebRequestMock;
            var requestedString = mock.GetRequestContentAsString();

            Assert.Equal("http://test.with.mock/", mock.RequestedAddress.ToString());
            Assert.Equal("HttpHappyPathTestLogger|test message1|",requestedString);
            Assert.Equal("POST",mock.Method);

        }

    }

    internal class SenderFactoryWithHttpMocks : INetworkSenderFactory
    {
        #region Implementation of INetworkSenderFactory

        /// <inheritdoc />
        public NetworkSender Create(string url, int maxQueueSize, SslProtocols sslProtocols, TimeSpan keepAliveTime)
        {
            return new HttpNetworkSender(url)
            {
                WebRequestFactory = new WebRequestFactoryMock(WebRequestMock)
            };
        }

        public WebRequestMock WebRequestMock { get; } = new WebRequestMock();

        #endregion
    }
}
