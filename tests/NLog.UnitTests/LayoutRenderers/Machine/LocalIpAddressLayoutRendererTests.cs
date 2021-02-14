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

using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using NLog.LayoutRenderers;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace NLog.UnitTests.LayoutRenderers
{
    using System;
    using NLog.Internal;

    public class LocalIpAddressLayoutRendererTests : NLogTestBase
    {
        private const string Mac1 = "F0-E1-D2-C3-B4-A5";

        /// <summary>
        /// Integration test
        /// </summary>
        [Fact]
        public void LocalIpAddress_CurrentMachine_NotEmpty()
        {
            var ipAddressRenderer = new LocalIpAddressLayoutRenderer();
            var result = ipAddressRenderer.Render(LogEventInfo.CreateNullEvent());
            Assert.NotEmpty(result);
        }

        [Fact]
        public void LocalIpAddress_RendersSuccessfulIp()
        {
            // Arrange
            var ipString = "10.0.1.2";

            var networkInterfaceRetrieverMock = new NetworkInterfaceRetrieverBuilder()
                .WithInterface(NetworkInterfaceType.Ethernet, Mac1)
                .WithIp(ipString)
                .Build();

            var ipAddressRenderer = new LocalIpAddressLayoutRenderer(networkInterfaceRetrieverMock);

            // Act
            var result = ipAddressRenderer.Render(LogEventInfo.CreateNullEvent());

            // Assert
            Assert.Equal(ipString, result);
        }

        [Fact]
        public void LocalIpAddress_OneInterfaceWithMultipleIps_RendersFirstIp()
        {
            // Arrange
            var networkInterfaceRetrieverMock = new NetworkInterfaceRetrieverBuilder()
                .WithInterface(NetworkInterfaceType.Ethernet, Mac1)
                .WithIp("10.0.1.1")
                .WithIp("10.0.1.2")
                .WithIp("10.0.1.3")
                .Build();

            var ipAddressRenderer = new LocalIpAddressLayoutRenderer(networkInterfaceRetrieverMock);

            // Act
            var result = ipAddressRenderer.Render(LogEventInfo.CreateNullEvent());

            // Assert
            Assert.Equal("10.0.1.1", result);
        }

        [Fact]
        public void LocalIpAddress_SkipsLoopback()
        {
            // Arrange
            var networkInterfaceRetrieverMock = new NetworkInterfaceRetrieverBuilder()
                .WithInterface(NetworkInterfaceType.Loopback, "F0-E0-D2-C3-B4-A5")
                .WithIp("1.2.3.4")
                .WithInterface(NetworkInterfaceType.Ethernet, Mac1)
                .WithIp("10.0.1.2")
                .Build();

            var ipAddressRenderer = new LocalIpAddressLayoutRenderer(networkInterfaceRetrieverMock);

            // Act
            var result = ipAddressRenderer.Render(LogEventInfo.CreateNullEvent());

            // Assert
            Assert.Equal("10.0.1.2", result);
        }

        [Fact]
        public void LocalIpAddress_Multiple_TakesFirst()
        {
            // Arrange

            var networkInterfaceRetrieverMock = new NetworkInterfaceRetrieverBuilder()
                .WithInterface(NetworkInterfaceType.Ethernet, "F0-E0-D2-C3-B4-A5")
                .WithIp("10.0.1.1")
                .WithInterface(NetworkInterfaceType.Ethernet, Mac1)
                .WithIp("10.0.1.2")
                .Build();

            var ipAddressRenderer = new LocalIpAddressLayoutRenderer(networkInterfaceRetrieverMock);

            // Act
            var result = ipAddressRenderer.Render(LogEventInfo.CreateNullEvent());

            // Assert
            Assert.Equal("10.0.1.1", result);
        }

        [Fact]
        public void LocalIpAddress_Multiple_TakesFirstUp()
        {
            // Arrange

            var networkInterfaceRetrieverMock = new NetworkInterfaceRetrieverBuilder()
                .WithInterface(NetworkInterfaceType.Ethernet, "F0-E0-D2-C3-B4-A5", OperationalStatus.Dormant)
                .WithIp("10.0.1.1")
                .WithInterface(NetworkInterfaceType.Ethernet, Mac1, OperationalStatus.Up)
                .WithIp("10.0.1.2")
                .WithInterface(NetworkInterfaceType.Ethernet, Mac1, OperationalStatus.Down)
                .WithIp("10.0.1.3", "10.0.1.0")
                .Build();

            var ipAddressRenderer = new LocalIpAddressLayoutRenderer(networkInterfaceRetrieverMock);

            // Act
            var result = ipAddressRenderer.Render(LogEventInfo.CreateNullEvent());

            // Assert
            Assert.Equal("10.0.1.2", result);
        }

        [Fact]
        public void LocalIpAddress_Multiple_TakesFirstWithGatewayUp()
        {
            // Arrange
            var networkInterfaceRetrieverMock = new NetworkInterfaceRetrieverBuilder()
                .WithInterface(NetworkInterfaceType.Ethernet, "F0-E0-D2-C3-B4-A5", OperationalStatus.Dormant)
                .WithIp("10.0.1.1", "10.0.1.0")
                .WithInterface(NetworkInterfaceType.Ethernet, Mac1, OperationalStatus.Up)
                .WithIp("10.0.1.2")
                .WithInterface(NetworkInterfaceType.Ethernet, Mac1, OperationalStatus.Up)
                .WithIp("10.0.1.3", "10.0.1.0")
                .Build();

            var ipAddressRenderer = new LocalIpAddressLayoutRenderer(networkInterfaceRetrieverMock);

            // Act
            var result = ipAddressRenderer.Render(LogEventInfo.CreateNullEvent());

            // Assert
            Assert.Equal("10.0.1.3", result);
        }

        [Fact]
        public void LocalIpAddress_Multiple_TakesFirstIpv4()
        {
            // Arrange
            var ipString = "10.0.1.2";

            var networkInterfaceRetrieverMock = new NetworkInterfaceRetrieverBuilder()
                .WithInterface(NetworkInterfaceType.Ethernet, "F0-E0-D2-C3-B4-A5")
                .WithIp("fe80:0:0:0:200:f8ff:fe21:67cf")
                .WithInterface(NetworkInterfaceType.Ethernet, Mac1)
                .WithIp(ipString)
                .Build();

            var ipAddressRenderer = new LocalIpAddressLayoutRenderer(networkInterfaceRetrieverMock);

            // Act
            var result = ipAddressRenderer.Render(LogEventInfo.CreateNullEvent());

            // Assert
            Assert.Equal(ipString, result);
        }

        [Fact]
        public void LocalIpAddress_Multiple_TakesFirstIpv6IfRequested()
        {
            // Arrange
            var ipv6 = "fe80::200:f8ff:fe21:67cf";

            var networkInterfaceRetrieverMock = new NetworkInterfaceRetrieverBuilder()
                .WithInterface(NetworkInterfaceType.Ethernet, "F0-E0-D2-C3-B4-A5")
                .WithIp("1.0.10.11")
                .WithInterface(NetworkInterfaceType.Ethernet, Mac1)
                .WithIp(ipv6)
                .Build();

            var ipAddressRenderer = new LocalIpAddressLayoutRenderer(networkInterfaceRetrieverMock) 
                {AddressFamily = AddressFamily.InterNetworkV6};

            // Act
            var result = ipAddressRenderer.Render(LogEventInfo.CreateNullEvent());

            // Assert
            Assert.Equal(ipv6, result);
        }

        [Fact]
        public void LocalIpAddress_RetrieverThrowsException_RenderEmptyString()
        {
            var networkInterfaceRetrieverMock = Substitute.For<INetworkInterfaceRetriever>();
            networkInterfaceRetrieverMock.AllNetworkInterfaces.Throws(new Exception("oops"));
            var ipAddressRenderer = new LocalIpAddressLayoutRenderer(networkInterfaceRetrieverMock);

            // Act
            var result = ipAddressRenderer.Render(LogEventInfo.CreateNullEvent());

            // Assert
            Assert.Equal(string.Empty, result);
        }
    }

    internal class NetworkInterfaceRetrieverBuilder
    {
        private readonly IDictionary<int, List<KeyValuePair<string, string>>> _ips = new Dictionary<int, List<KeyValuePair<string, string>>>();

        private IList<(NetworkInterfaceType networkInterfaceType, string mac, OperationalStatus status)> _networkInterfaces = new List<(NetworkInterfaceType networkInterfaceType, string mac, OperationalStatus status)>();
        private readonly INetworkInterfaceRetriever _networkInterfaceRetrieverMock;

        /// <inheritdoc />
        public NetworkInterfaceRetrieverBuilder()
        {
            _networkInterfaceRetrieverMock = Substitute.For<INetworkInterfaceRetriever>();
        }

        public NetworkInterfaceRetrieverBuilder WithInterface(NetworkInterfaceType networkInterfaceType, string mac, OperationalStatus status = OperationalStatus.Up)
        {
            _networkInterfaces.Add((networkInterfaceType, mac, status));
            return this;
        }

        /// <summary>
        /// One or more ips for an interface added with <see cref="WithInterface"/>
        /// </summary>
        public NetworkInterfaceRetrieverBuilder WithIp(string ip, string gateway = null)
        {
            if (_networkInterfaces.Count == 0)
            {
                throw new Exception("add interface first");
            }

            var key = _networkInterfaces.Count - 1;
            if (!_ips.ContainsKey(key))
            {
                _ips.Add(key, new List<KeyValuePair<string,string>>());
            }

            var list = _ips[key];
            list.Add(new KeyValuePair<string, string>(ip, gateway));
            return this;
        }

        public INetworkInterfaceRetriever Build()
        {
            var networkInterfaces = BuildAllNetworkInterfaces().ToArray();
            _networkInterfaceRetrieverMock.AllNetworkInterfaces.Returns(networkInterfaces);

            return _networkInterfaceRetrieverMock;
        }

        private IEnumerable<NetworkInterface> BuildAllNetworkInterfaces()
        {
            for (var i = 0; i < _networkInterfaces.Count; i++)
            {
                var networkInterface = _networkInterfaces[i];
                if (_ips.TryGetValue(i, out var ips))
                {
                    var networkInterfaceMock = BuildNetworkInterfaceMock(ips, networkInterface.mac, networkInterface.networkInterfaceType, networkInterface.status);
                    networkInterfaceMock.Id.Returns($"#{i}");
                    networkInterfaceMock.Description.Returns("ips: " + string.Join(";", ips.ToArray()));
                    yield return networkInterfaceMock;
                }
            }
        }

        private NetworkInterface BuildNetworkInterfaceMock(IEnumerable<KeyValuePair<string, string>> ips, string mac, NetworkInterfaceType type, OperationalStatus status)
        {
            var networkInterfaceMock = Substitute.For<NetworkInterface>();

            networkInterfaceMock.NetworkInterfaceType.Returns(type);
            networkInterfaceMock.OperationalStatus.Returns(status);
            networkInterfaceMock.GetPhysicalAddress().Returns(PhysicalAddress.Parse(mac));

            var unicastIpAddressInformations = new List<UnicastIPAddressInformation>(BuildUnicastInfoMocks(ips.Select(p => p.Key)));
            var gatewayIpAddressInformations = new List<GatewayIPAddressInformation>(BuildGatewayInfoMocks(ips.Select(p => p.Value)));

            var unicastIpAddressInformationCollection = Substitute.For<UnicastIPAddressInformationCollection>();
            unicastIpAddressInformationCollection.GetEnumerator().Returns(unicastIpAddressInformations.GetEnumerator());
            unicastIpAddressInformationCollection.Count.Returns(unicastIpAddressInformations.Count);

            var gatewayIpAddressInformationCollection = Substitute.For<GatewayIPAddressInformationCollection>();
            gatewayIpAddressInformationCollection.GetEnumerator().Returns(gatewayIpAddressInformations.GetEnumerator());
            gatewayIpAddressInformationCollection.Count.Returns(gatewayIpAddressInformations.Count);

            var interfacePropertiesMock = Substitute.For<IPInterfaceProperties>();
            interfacePropertiesMock.UnicastAddresses.Returns(unicastIpAddressInformationCollection);
            interfacePropertiesMock.GatewayAddresses.Returns(gatewayIpAddressInformationCollection);

            networkInterfaceMock.GetIPProperties().Returns(interfacePropertiesMock);
            return networkInterfaceMock;
        }

        private static IEnumerable<UnicastIPAddressInformation> BuildUnicastInfoMocks(IEnumerable<string> ips)
        {
            foreach (var ip in ips)
            {
                var ipInfoMock = Substitute.For<UnicastIPAddressInformation>();
                ipInfoMock.Address.Returns(IPAddress.Parse(ip));
                yield return ipInfoMock;
            }
        }

        private static IEnumerable<GatewayIPAddressInformation> BuildGatewayInfoMocks(IEnumerable<string> ips)
        {
            foreach (var ip in ips)
            {
                if (string.IsNullOrEmpty(ip))
                    continue;

                var ipInfoMock = Substitute.For<GatewayIPAddressInformation>();
                ipInfoMock.Address.Returns(IPAddress.Parse(ip));
                yield return ipInfoMock;
            }
        }

    }
}
