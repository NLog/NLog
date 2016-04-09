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
    using System.IO;
    using System.Net;
    using System.Net.Sockets;
    using NLog.Common;

    /// <summary>
    /// Default implementation of <see cref="INetworkSenderFactory"/>.
    /// </summary>
    internal class NetworkSenderFactory : INetworkSenderFactory
    {
        public static readonly INetworkSenderFactory Default = new NetworkSenderFactory();

        /// <summary>
        /// Creates a new instance of the network sender based on a network URL:.
        /// </summary>
        /// <param name="url">
        /// URL that determines the network sender to be created.
        /// </param>
        /// <param name="maxQueueSize">
        /// The maximum queue size.
        /// </param>
        /// /// <returns>
        /// A newly created network sender.
        /// </returns>
        public NetworkSender Create(string url, int maxQueueSize)
        {
            if (url.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
            {
                return new HttpNetworkSender(url);
            }

            if (url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                return new HttpNetworkSender(url);
            }

            if (url.StartsWith("tcp://", StringComparison.OrdinalIgnoreCase))
            {
                return new TcpNetworkSender(url, AddressFamily.Unspecified)
                           {
                               MaxQueueSize = maxQueueSize
                           };
            }

            if (url.StartsWith("tcp4://", StringComparison.OrdinalIgnoreCase))
            {
                return new TcpNetworkSender(url, AddressFamily.InterNetwork)
                           {
                               MaxQueueSize = maxQueueSize
                           };
            }

            if (url.StartsWith("tcp6://", StringComparison.OrdinalIgnoreCase))
            {
                return new TcpNetworkSender(url, AddressFamily.InterNetworkV6)
                           {
                               MaxQueueSize = maxQueueSize
                           };
            }

#if !SILVERLIGHT
            if (url.StartsWith("udp://", StringComparison.OrdinalIgnoreCase))
            {
                return new UdpNetworkSender(url, AddressFamily.Unspecified);
            }

            if (url.StartsWith("udp4://", StringComparison.OrdinalIgnoreCase))
            {
                return new UdpNetworkSender(url, AddressFamily.InterNetwork);
            }

            if (url.StartsWith("udp6://", StringComparison.OrdinalIgnoreCase))
            {
                return new UdpNetworkSender(url, AddressFamily.InterNetworkV6);
            }
#endif

            throw new ArgumentException("Unrecognized network address", "url");
        }
    }
}
