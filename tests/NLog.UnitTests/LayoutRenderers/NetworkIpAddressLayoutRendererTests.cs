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

using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using NSubstitute.ExceptionExtensions;

namespace NLog.UnitTests.LayoutRenderers
{
    using System;
    using NLog.LayoutRenderers;
    using Xunit;
    using NSubstitute;

    public class NetworkIpAddressLayoutRendererTests : NLogTestBase
    {
        /// <summary>
        /// Integration test
        /// </summary>
        [Fact]
        public void NetworkIpAddress_CurrentMachine_NotEmpty()
        {
            var ipAddressRenderer = new NetworkIpAddressLayoutRenderer();
            var result = ipAddressRenderer.Render(LogEventInfo.CreateNullEvent());
            Assert.NotEmpty(result);
        }

        [Fact]
        public void NetworkIpAddress_RetrieverThrowsException_RenderEmptyString()
        {
            var networkInterfaceRetrieverMock = Substitute.For<INetworkInterfaceRetriever>();
            networkInterfaceRetrieverMock.GetAllNetworkInterfaces().Throws(new Exception("oops"));
            var ipAddressRenderer = new NetworkIpAddressLayoutRenderer(networkInterfaceRetrieverMock);

            // Act
            var result = ipAddressRenderer.Render(LogEventInfo.CreateNullEvent());

            // Assert
            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public void NetworkIpAddress_RendersSuccessfulIp()
        {
            var ipString = "10.0.1.2";
            var mac = "F0-E1-D2-C3-B4-A5";

            var ipAddressRenderer = CreateNetworkIpAddressLayoutRenderer(ipString, mac);

            // Act
            var result = ipAddressRenderer.Render(LogEventInfo.CreateNullEvent());

            // Assert
            Assert.Equal(ipString, result);
        }

        private static NetworkIpAddressLayoutRenderer CreateNetworkIpAddressLayoutRenderer(string ipString, string mac)
        {
            var interfacePropertiesMock = Substitute.For<IPInterfaceProperties>();
            var unicastIpAddressInformationCollection = Substitute.For<UnicastIPAddressInformationCollection>();
            var networkInterfaceRetrieverMock = Substitute.For<INetworkInterfaceRetriever>();
            var networkInterfaceMock = Substitute.For<NetworkInterface>();
            var ipInfoMock = Substitute.For<UnicastIPAddressInformation>();

            interfacePropertiesMock.UnicastAddresses.Returns(unicastIpAddressInformationCollection);

            networkInterfaceRetrieverMock.GetAllNetworkInterfaces().Returns(new List<NetworkInterface> { networkInterfaceMock });

            ipInfoMock.Address.Returns(IPAddress.Parse(ipString));

            networkInterfaceMock.GetIPProperties().Returns(interfacePropertiesMock);
            networkInterfaceMock.NetworkInterfaceType.Returns(NetworkInterfaceType.Ethernet);
            networkInterfaceMock.GetPhysicalAddress().Returns(PhysicalAddress.Parse(mac));

            var unicastIpAddressInformations = new List<UnicastIPAddressInformation> { ipInfoMock };

            unicastIpAddressInformationCollection.GetEnumerator().Returns(unicastIpAddressInformations.GetEnumerator());
            unicastIpAddressInformationCollection.Count.Returns(unicastIpAddressInformations.Count);

            return new NetworkIpAddressLayoutRenderer(networkInterfaceRetrieverMock);
        }
    }

}