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
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using NLog.LayoutRenderers;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace NLog.UnitTests.LayoutRenderers
{
    using System;

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
        public void NetworkIpAddress_RendersSuccessfulIp()
        {
            var ipString = "10.0.1.2";

            var builder = new NetworkInterfaceRetrieverBuilder()
                .WithInterface(NetworkInterfaceType.Ethernet, "F0-E1-D2-C3-B4-A5")
                .WithIp(ipString);
            var networkInterfaceRetrieverMock = builder.Build();

            var ipAddressRenderer = new NetworkIpAddressLayoutRenderer(networkInterfaceRetrieverMock);

            // Act
            var result = ipAddressRenderer.Render(LogEventInfo.CreateNullEvent());

            // Assert
            Assert.Equal(ipString, result);
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
    }

    public class NetworkInterfaceRetrieverBuilder
    {
        private readonly IDictionary<int, List<string>> _ips = new Dictionary<int, List<string>>();

        private IList<(NetworkInterfaceType networkInterfaceType, string mac)> _networkInterfaces = new List<(NetworkInterfaceType networkInterfaceType, string mac)>();
        private readonly IPInterfaceProperties interfacePropertiesMock;
        private readonly INetworkInterfaceRetriever networkInterfaceRetrieverMock;
        private readonly UnicastIPAddressInformationCollection unicastIpAddressInformationCollection;

        /// <inheritdoc />
        public NetworkInterfaceRetrieverBuilder()
        {
            interfacePropertiesMock = Substitute.For<IPInterfaceProperties>();
            unicastIpAddressInformationCollection = Substitute.For<UnicastIPAddressInformationCollection>();
            networkInterfaceRetrieverMock = Substitute.For<INetworkInterfaceRetriever>();
        }

        public NetworkInterfaceRetrieverBuilder WithInterface(NetworkInterfaceType networkInterfaceType, string mac)
        {
            _networkInterfaces.Add((networkInterfaceType, mac));
            return this;
        }

        /// <summary>
        /// One or more ips for an interface
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        public NetworkInterfaceRetrieverBuilder WithIp(string ip)
        {
            if (_networkInterfaces.Count == 0)
            {
                throw new Exception("add interface first");
            }

            var key = _networkInterfaces.Count - 1;
            if (!_ips.ContainsKey(key))
            {
                _ips.Add(key, new List<string>());
            }

            var list = _ips[key];
            list.Add(ip);

            return this;
        }

        public INetworkInterfaceRetriever Build()
        {
            var allNetworkInterfaces = BuildAllNetworkInterfaces();
            var networkInterfaces = new List<NetworkInterface>(allNetworkInterfaces);
            networkInterfaceRetrieverMock.GetAllNetworkInterfaces().Returns(networkInterfaces);

            return networkInterfaceRetrieverMock;
        }

        private IEnumerable<NetworkInterface> BuildAllNetworkInterfaces()
        {
            for (var i = 0; i < _networkInterfaces.Count; i++)
            {
                var networkInterface = _networkInterfaces[i];
                if (_ips.TryGetValue(i, out var ips))
                {
                    var networkInterfaceMock = BuildNetworkInterfaceMock(ips, networkInterface.mac, networkInterface.networkInterfaceType);
                    yield return networkInterfaceMock;
                }
            }
        }

        private NetworkInterface BuildNetworkInterfaceMock(IEnumerable<string> ips, string mac, NetworkInterfaceType intrefaceType)
        {
            var networkInterfaceMock = Substitute.For<NetworkInterface>();


            networkInterfaceMock.NetworkInterfaceType.Returns(intrefaceType);
            networkInterfaceMock.GetPhysicalAddress().Returns(PhysicalAddress.Parse(mac));

            var unicastIpAddressInformations = new List<UnicastIPAddressInformation>(BuildIpInfoMocks(ips));

            networkInterfaceMock.GetIPProperties().Returns(interfacePropertiesMock);

            interfacePropertiesMock.UnicastAddresses.Returns(unicastIpAddressInformationCollection);
            unicastIpAddressInformationCollection.GetEnumerator().Returns(unicastIpAddressInformations.GetEnumerator());
            unicastIpAddressInformationCollection.Count.Returns(unicastIpAddressInformations.Count);
            return networkInterfaceMock;
        }

        private static IEnumerable<UnicastIPAddressInformation> BuildIpInfoMocks(IEnumerable<string> ips)
        {
            foreach (var ip in ips)
            {
                var ipInfoMock = Substitute.For<UnicastIPAddressInformation>();
                ipInfoMock.Address.Returns(IPAddress.Parse(ip));
                yield return ipInfoMock;
            }
        }
    }
}