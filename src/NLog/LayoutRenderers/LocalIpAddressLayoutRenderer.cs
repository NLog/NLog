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

#if !NETSTANDARD1_0 && !SILVERLIGHT && !__IOS__ && !__ANDROID__

namespace NLog.LayoutRenderers
{
    using System;
    using System.Net;
    using System.Net.NetworkInformation;
    using System.Net.Sockets;
    using System.Text;
    using NLog.Common;
    using NLog.Config;

    /// <summary>
    /// The IP address from the network interface card (NIC) on the local machine
    /// </summary>
    /// <remarks>
    /// Skips loopback-adapters and tunnel-interfaces. Skips devices without any MAC-address
    /// </remarks>
    [LayoutRenderer("local-ip")]
    [ThreadAgnostic]
    [ThreadSafe]
    public class LocalIpAddressLayoutRenderer : LayoutRenderer
    {
        private AddressFamily? _addressFamily;
        private readonly INetworkInterfaceRetriever _networkInterfaceRetriever;

        /// <summary>
        /// Get or set whether to prioritize IPv6 or IPv4 (default)
        /// </summary>
        public AddressFamily AddressFamily { get => _addressFamily ?? AddressFamily.InterNetwork; set => _addressFamily = value; }

        /// <inheritdoc />
        public LocalIpAddressLayoutRenderer()
        {
            _networkInterfaceRetriever = new NetworkInterfaceRetriever();
        }

        /// <inheritdoc />
        internal LocalIpAddressLayoutRenderer(INetworkInterfaceRetriever networkInterfaceRetriever)
        {
            _networkInterfaceRetriever = networkInterfaceRetriever;
        }

        /// <inheritdoc/>
        protected override void Append(StringBuilder builder, LogEventInfo logEvent)
        {
            builder.Append(LookupIpAddress());
        }

        private string LookupIpAddress()
        {
            int currentNetworkScore = 0;
            string currentIpAddress = string.Empty;

            try
            {
                foreach (var networkInterface in _networkInterfaceRetriever.AllNetworkInterfaces)
                {
                    int networkScore = CalculateNetworkInterfaceScore(networkInterface);
                    if (networkScore == 0)
                        continue;

                    foreach (var networkAddress in networkInterface.GetIPProperties().UnicastAddresses)
                    {
                        int unicastScore = CalculateNetworkAddressScore(networkAddress);
                        if (unicastScore == 0)
                            continue;

                        if (CheckOptimalNetworkScore(networkAddress, networkScore + unicastScore, ref currentNetworkScore, ref currentIpAddress))
                        {
                            return currentIpAddress;
                        }
                    }
                }

                return currentIpAddress;
            }
            catch (Exception ex)
            {
                InternalLogger.Warn(ex, "Failed to lookup NetworkInterface.GetAllNetworkInterfaces()");
                return currentIpAddress;
            }
        }

        private bool CheckOptimalNetworkScore(UnicastIPAddressInformation networkAddress, int networkScore, ref int currentNetworkScore, ref string currentIpAddress)
        {
            const int greatNetworkScore = 16;   // 8 = Good Address Family + 4 = Good NetworkStatus + Extra Bonus Points

            if (networkScore > currentNetworkScore)
            {
                currentIpAddress = networkAddress.Address.ToString();
                currentNetworkScore = networkScore;
                if (currentNetworkScore >= greatNetworkScore)
                    return true;
            }

            return false;
        }

        private static int CalculateNetworkInterfaceScore(NetworkInterface networkInterface)
        {
            if (networkInterface.NetworkInterfaceType == NetworkInterfaceType.Loopback)
                return 0;

            if (networkInterface.NetworkInterfaceType == NetworkInterfaceType.Tunnel)
                return 0;

            const int macAddressMinLength = 12;
            if (networkInterface.GetPhysicalAddress()?.ToString()?.Length >= macAddressMinLength)
            {
                int currentScore = 1;

                if (networkInterface.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                    currentScore += 1;

                if (networkInterface.OperationalStatus == OperationalStatus.Up)
                    currentScore += 5;  // Better to have Ipv6 that is Up, than Ipv4 that is Down

                return currentScore;
            }

            return 0;
        }

        private int CalculateNetworkAddressScore(UnicastIPAddressInformation networkAddress)
        {
            var currentScore = CalculateIpAddressScore(networkAddress.Address);
            if (currentScore == 0)
                return 0;

            if (!networkAddress.IsDnsEligible)
                currentScore += 1;

            if (networkAddress.PrefixOrigin == PrefixOrigin.Dhcp)
                currentScore += 1;

            return currentScore;
        }

        private int CalculateIpAddressScore(IPAddress ipAddress)
        {
            if (IPAddress.IsLoopback(ipAddress))
                return 0;

            if (ipAddress.AddressFamily != AddressFamily.InterNetwork && ipAddress.AddressFamily != AddressFamily.InterNetworkV6 && ipAddress.AddressFamily != _addressFamily)
                return 0;

            var ipAddressValue = ipAddress.ToString();
            if (string.IsNullOrEmpty(ipAddressValue))
                return 0;

            int currentScore = 0;
            if (ipAddressValue != "127.0.0.1" && ipAddressValue != "0.0.0.0" && ipAddressValue != "::1")
                currentScore += 1;

            if (_addressFamily.HasValue)
            {
                if (ipAddress.AddressFamily == _addressFamily.Value)
                    currentScore += 8;
                else
                    currentScore += 2;
            }
            else if (ipAddress.AddressFamily == AddressFamily.InterNetwork)
            {
                currentScore += 8;
            }
            else
            {
                currentScore += 4;
            }

            return currentScore;
        }
    }
}

#endif
