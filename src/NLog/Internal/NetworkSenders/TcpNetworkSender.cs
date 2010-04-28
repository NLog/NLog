// 
// Copyright (c) 2004-2010 Jaroslaw Kowalski <jaak@jkowalski.net>
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

#if SILVERLIGHT
using System.Threading;
#endif

    /// <summary>
    /// Sends messages over a TCP network connection.
    /// </summary>
    internal class TcpNetworkSender : NetworkSender
    {
        private Socket socket;
#if SILVERLIGHT
        private AutoResetEvent syncHandle = new AutoResetEvent(false);
        private SocketError lastError;
#endif

        /// <summary>
        /// Initializes a new instance of the <see cref="TcpNetworkSender" /> class.
        /// </summary>
        /// <param name="url">URL. Must start with tcp://.</param>
        public TcpNetworkSender(string url)
            : base(url)
        {
            // tcp://hostname:port
            Uri parsedUri = new Uri(url);
#if SILVERLIGHT
            SocketAsyncEventArgs sea = new SocketAsyncEventArgs();
            sea.RemoteEndPoint = new DnsEndPoint(parsedUri.Host, parsedUri.Port);
            sea.Completed += this.AsyncCompleted;
            this.socket.ConnectAsync(sea);
            this.syncHandle.WaitOne();
            if (this.lastError != SocketError.Success)
            {
                throw new IOException("Cannot connect to host " + url);
            }
#else
            IPHostEntry host = Dns.GetHostEntry(parsedUri.Host);
            int port = parsedUri.Port;

            this.socket = new Socket(host.AddressList[0].AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            this.socket.Connect(new IPEndPoint(host.AddressList[0], port));
#endif
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
                    this.socket.Close();
                }
                catch (Exception)
                {
                    // ignore errors
                }

                this.socket = null;
            }
        }

        /// <summary>
        /// Sends the specified text over the connected socket.
        /// </summary>
        /// <param name="bytes">The bytes to be sent.</param>
        /// <param name="offset">Offset in buffer.</param>
        /// <param name="length">Number of bytes to send.</param>
        /// <remarks>To be overridden in inheriting classes.</remarks>
        protected override void DoSend(byte[] bytes, int offset, int length)
        {
            lock (this)
            {
#if SILVERLIGHT
                SocketAsyncEventArgs sea = new SocketAsyncEventArgs();
                sea.SetBuffer(bytes, offset, length);
                sea.Completed += this.AsyncCompleted;
                this.socket.SendAsync(sea);
                this.syncHandle.WaitOne();
                if (this.lastError != SocketError.Success)
                {
                    throw new IOException("Network send error");
                }
#else
                this.socket.Send(bytes, offset, length, SocketFlags.None);
#endif
            }
        }

#if SILVERLIGHT
        private void AsyncCompleted(object sender, SocketAsyncEventArgs e)
        {
            this.syncHandle.Set();
            this.lastError = e.SocketError;
        }
#endif
    }
}
