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

using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace NLog.Internal.NetworkSenders
{
    /// <summary>
    /// Sends messages over a TCP network connection.
    /// </summary>
	public class TcpNetworkSender : NetworkSender
	{
        private Socket _socket;

        /// <summary>
        /// Creates a new instance of <see cref="TcpNetworkSender"/> and initializes
        /// it with the specified URL. Connects to the server specified in the URL.
        /// </summary>
        /// <param name="url">URL. Must start with tcp://</param>
        public TcpNetworkSender(string url) : base(url)
        {
            // tcp://hostname:port

            Uri parsedUri = new Uri(url);
#if NET_2_API
            IPHostEntry host = Dns.GetHostEntry(parsedUri.Host);
#else
            IPHostEntry host = Dns.GetHostByName(parsedUri.Host);
#endif
            int port = parsedUri.Port;

#if NETCF_1_0
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
#else
            _socket = new Socket(host.AddressList[0].AddressFamily, SocketType.Stream, ProtocolType.Tcp);
#endif
            _socket.Connect(new IPEndPoint(host.AddressList[0], port));
        }

        /// <summary>
        /// Sends the specified text over the connected socket.
        /// </summary>
        /// <param name="bytes">The bytes to be sent.</param>
        /// <param name="offset">Offset in buffer</param>
        /// <param name="length">Number of bytes to send</param>
        /// <remarks>To be overridden in inheriting classes.</remarks>
        protected override void DoSend(byte[] bytes, int offset, int length)
        {
            lock (this)
            {
                _socket.Send(bytes, offset, length, SocketFlags.None);
            }
        }

        /// <summary>
        /// Closes the socket.
        /// </summary>
        public override void Close()
        {
            lock (this)
            {
                try
                {
                    _socket.Close();
                }
                catch (Exception)
                {
                    // ignore errors
                }
                _socket = null;
            }
        }
    }
}
