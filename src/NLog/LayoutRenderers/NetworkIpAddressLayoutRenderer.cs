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
    [LayoutRenderer("networkip")]
    [ThreadAgnostic]
    [ThreadSafe]
    public class NetworkIpAddressLayoutRenderer : LayoutRenderer
    {
        private AddressFamily? _addressFamily;

        /// <summary>
        /// Get or set whether to prioritize IPv6 or IPv4 (default)
        /// </summary>
        public AddressFamily AddressFamily { get => _addressFamily ?? AddressFamily.InterNetwork; set => _addressFamily = value; }

        /// <inheritdoc/>
        protected override void Append(StringBuilder builder, LogEventInfo logEvent)
        {
            builder.Append(LookupIpAddress());
        }

        string LookupIpAddress()
        {
            string firstMatchAddress = string.Empty;
            string optimalIpAddress = string.Empty;

            try
            {
                foreach (var networkInterface in NetworkInterface.GetAllNetworkInterfaces())
                {
                    if (networkInterface.NetworkInterfaceType == NetworkInterfaceType.Loopback || networkInterface.NetworkInterfaceType == NetworkInterfaceType.Tunnel)
                        continue;

                    foreach (var networkAddress in networkInterface.GetIPProperties().UnicastAddresses)
                    {
                        var ipAddress = networkAddress.Address;
                        if (IPAddress.IsLoopback(ipAddress))
                            continue;

                        if (TryFindingMostOptimalIpAddress(networkInterface, ipAddress, ref firstMatchAddress, ref optimalIpAddress))
                            return optimalIpAddress;
                    }
                }

                return string.IsNullOrEmpty(optimalIpAddress) ? firstMatchAddress : optimalIpAddress;
            }
            catch (Exception ex)
            {
                InternalLogger.Warn(ex, "Failed to lookup NetworkInterface.GetAllNetworkInterfaces()");
                return string.IsNullOrEmpty(optimalIpAddress) ? firstMatchAddress : optimalIpAddress;
            }
        }

        private bool TryFindingMostOptimalIpAddress(NetworkInterface networkInterface, IPAddress ipAddress, ref string firstMatchAddress, ref string optimalIpAddress)
        {
            if (_addressFamily.HasValue && ipAddress.AddressFamily == _addressFamily.Value)
            {
                if (ValidateNetworkIpAddress(networkInterface, ipAddress, ref firstMatchAddress, ref optimalIpAddress))
                {
                    optimalIpAddress = ipAddress.ToString();
                    if (networkInterface.OperationalStatus == OperationalStatus.Up)
                        return true;
                }
            }
            else if (ipAddress.AddressFamily == AddressFamily.InterNetwork)
            {
                if (ValidateNetworkIpAddress(networkInterface, ipAddress, ref firstMatchAddress, ref optimalIpAddress))
                {
                    if (!_addressFamily.HasValue && networkInterface.OperationalStatus == OperationalStatus.Up)
                    {
                        optimalIpAddress = ipAddress.ToString();
                        return true;
                    }
                }
            }
            else if (ipAddress.AddressFamily == AddressFamily.InterNetworkV6)
            {
                ValidateNetworkIpAddress(networkInterface, ipAddress, ref firstMatchAddress, ref optimalIpAddress);
            }

            return false;
        }

        private static bool ValidateNetworkIpAddress(NetworkInterface networkInterface, IPAddress ipAddress, ref string firstMatchAddress, ref string optimalIpAddress)
        {
            const int minMacAddressLength = 12;
            if (networkInterface.GetPhysicalAddress()?.ToString()?.Length >= minMacAddressLength)
            {
                var ipAddressValue = ipAddress.ToString();
                if (string.IsNullOrEmpty(firstMatchAddress))
                    firstMatchAddress = ipAddressValue;

                if (networkInterface.OperationalStatus == OperationalStatus.Up)
                {
                    if (!string.IsNullOrEmpty(optimalIpAddress))
                        optimalIpAddress = ipAddressValue;
                }

                return true;
            }

            return false;
        }
    }
}

#endif
