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

namespace NLog.Internal.NetworkSenders
{
    using System;
    using System.IO;
    using System.Net;
    using System.Net.Sockets;
    using NLog.Common;

    /// <summary>
    /// Sends messages over the network as UDP datagrams.
    /// </summary>
    internal class UdpNetworkSender : NetworkSender
    {
        private readonly object _lockObject = new object();

        private ISocket _socket;
        private EndPoint _endpoint;

        /// <summary>
        /// Initializes a new instance of the <see cref="UdpNetworkSender"/> class.
        /// </summary>
        /// <param name="url">URL. Must start with udp://.</param>
        /// <param name="addressFamily">The address family.</param>
        public UdpNetworkSender(string url, AddressFamily addressFamily)
            : base(url)
        {
            AddressFamily = addressFamily;
        }

        internal AddressFamily AddressFamily { get; set; }

        /// <summary>
        /// Creates the socket.
        /// </summary>
        /// <param name="addressFamily">The address family.</param>
        /// <param name="socketType">Type of the socket.</param>
        /// <param name="protocolType">Type of the protocol.</param>
        /// <returns>Implementation of <see cref="ISocket"/> to use.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Socket is disposed elsewhere.")]
        protected internal virtual ISocket CreateSocket(AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType)
        {
            var proxy = new SocketProxy(addressFamily, socketType, protocolType);

            Uri uri;
            if (Uri.TryCreate(Address, UriKind.Absolute, out uri)
                && uri.Host.Equals(IPAddress.Broadcast.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                proxy.UnderlyingSocket.EnableBroadcast = true;
            }

            return proxy;
        }

        /// <summary>
        /// Performs sender-specific initialization.
        /// </summary>
        protected override void DoInitialize()
        {
            _endpoint = ParseEndpointAddress(new Uri(Address), AddressFamily);
            _socket = CreateSocket(_endpoint.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
        }

        /// <summary>
        /// Closes the socket.
        /// </summary>
        /// <param name="continuation">The continuation.</param>
        protected override void DoClose(AsyncContinuation continuation)
        {
            lock (_lockObject)
            {
                CloseSocket(continuation);
            }
        }

        private void CloseSocket(AsyncContinuation continuation)
        {
            try
            {
                var sock = _socket;
                _socket = null;

                if (sock != null)
                {
                    sock.Close();
                }

                continuation(null);
            }
            catch (Exception exception)
            {
                if (exception.MustBeRethrown())
                {
                    throw;
                }

                continuation(exception);
            }
        }

        /// <summary>
        /// Sends the specified text as a UDP datagram.
        /// </summary>
        /// <param name="bytes">The bytes to be sent.</param>
        /// <param name="offset">Offset in buffer.</param>
        /// <param name="length">Number of bytes to send.</param>
        /// <param name="asyncContinuation">The async continuation to be invoked after the buffer has been sent.</param>
        /// <remarks>To be overridden in inheriting classes.</remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Dispose() is called in the event handler.")]
        protected override void DoSend(byte[] bytes, int offset, int length, AsyncContinuation asyncContinuation)
        {
            lock (_lockObject)
            {
                var args = new SocketAsyncEventArgs();
                args.SetBuffer(bytes, offset, length);
                args.UserToken = asyncContinuation;
                args.Completed += SocketOperationCompleted;
                args.RemoteEndPoint = _endpoint;

                bool asyncOperation = false;
                try
                {
                    asyncOperation = _socket.SendToAsync(args);
                }
                catch (SocketException ex)
                {
                    InternalLogger.Error(ex, "NetworkTarget: Error sending udp request");
                    args.SocketError = ex.SocketErrorCode;
                }
                catch (Exception ex)
                {
                    InternalLogger.Error(ex, "NetworkTarget: Error sending udp request");
                    if (ex.InnerException is SocketException socketException)
                        args.SocketError = socketException.SocketErrorCode;
                    else
                        args.SocketError = SocketError.OperationAborted;
                }

                if (!asyncOperation)
                {
                    SocketOperationCompleted(_socket, args);
                }
            }
        }

        private void SocketOperationCompleted(object sender, SocketAsyncEventArgs e)
        {
            var asyncContinuation = e.UserToken as AsyncContinuation;

            Exception error = null;

            if (e.SocketError != SocketError.Success)
            {
                error = new IOException("Error: " + e.SocketError);
            }

            e.Dispose();

            if (asyncContinuation != null)
            {
                asyncContinuation(error);
            }
        }

        public override void CheckSocket()
        {
            if (_socket == null)
            {
                DoInitialize();
            }
        }
    }
}