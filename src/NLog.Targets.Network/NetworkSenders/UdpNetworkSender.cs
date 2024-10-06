//
// Copyright (c) 2004-2024 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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
    internal class UdpNetworkSender : QueuedNetworkSender
    {
        private ISocket _socket;
        private EndPoint _endpoint;
        private readonly EventHandler<SocketAsyncEventArgs> _socketOperationCompletedAsync;
        System.Threading.WaitCallback _asyncBeginRequest;

        /// <summary>
        /// Initializes a new instance of the <see cref="UdpNetworkSender"/> class.
        /// </summary>
        /// <param name="url">URL. Must start with udp://.</param>
        /// <param name="addressFamily">The address family.</param>
        public UdpNetworkSender(string url, AddressFamily addressFamily)
            : base(url)
        {
            AddressFamily = addressFamily;
            _socketOperationCompletedAsync = SocketOperationCompletedAsync;
        }

        internal AddressFamily AddressFamily { get; set; }

        internal int MaxMessageSize { get; set; }

        /// <summary>
        /// Creates the socket.
        /// </summary>
        /// <param name="ipAddress">The IP address.</param>
        /// <returns>Implementation of <see cref="ISocket"/> to use.</returns>
        protected internal virtual ISocket CreateSocket(IPAddress ipAddress)
        {
            var proxy = new SocketProxy(ipAddress.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
            if (ipAddress.AddressFamily != AddressFamily.InterNetworkV6 && ipAddress.Equals(IPAddress.Broadcast))
            {
                proxy.UnderlyingSocket.EnableBroadcast = true;
            }
            return proxy;
        }

        protected override void DoInitialize()
        {
            var uri = new Uri(Address);
            var address = ResolveIpAddress(uri, AddressFamily);
            _endpoint = new IPEndPoint(address, uri.Port);
            _socket = CreateSocket(address);
        }

        protected override void DoClose(AsyncContinuation continuation)
        {
            base.DoClose(ex => CloseSocket(continuation, ex));
        }

        private void CloseSocket(AsyncContinuation continuation, Exception pendingException)
        {
            try
            {
                var sock = _socket;
                _socket = null;
                sock?.Close();

                continuation(pendingException);
            }
            catch (Exception exception)
            {
                if (LogManager.ThrowExceptions)
                {
                    throw;
                }

                continuation(exception);
            }
        }

        protected override void BeginRequest(NetworkRequestArgs eventArgs)
        {
            var socketEventArgs = new SocketAsyncEventArgs();
            socketEventArgs.Completed += _socketOperationCompletedAsync;
            socketEventArgs.RemoteEndPoint = _endpoint;
            SetSocketNetworkRequest(socketEventArgs, eventArgs);

            // Schedule async network operation to avoid blocking socket-operation (Allow adding more request)
            if (_asyncBeginRequest is null)
                _asyncBeginRequest = BeginRequestAsync;
            System.Threading.ThreadPool.QueueUserWorkItem(_asyncBeginRequest, socketEventArgs);
        }

        private void BeginRequestAsync(object state)
        {
            BeginSocketRequest((SocketAsyncEventArgs)state);
        }

        private void BeginSocketRequest(SocketAsyncEventArgs args)
        {
            bool asyncOperation = false;

            do
            {
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

                args = asyncOperation ? null : SocketOperationCompleted(args);
            }
            while (args != null);
        }

        private void SetSocketNetworkRequest(SocketAsyncEventArgs socketEventArgs, NetworkRequestArgs networkRequest)
        {
            var messageLength = MaxMessageSize > 0 ? Math.Min(networkRequest.RequestBufferLength, MaxMessageSize) : networkRequest.RequestBufferLength;
            socketEventArgs.SetBuffer(networkRequest.RequestBuffer, networkRequest.RequestBufferOffset, messageLength);
            socketEventArgs.UserToken = networkRequest.AsyncContinuation;
        }

        private void SocketOperationCompletedAsync(object sender, SocketAsyncEventArgs args)
        {
            var nextRequest = SocketOperationCompleted(args);
            if (nextRequest != null)
            {
                BeginSocketRequest(nextRequest);
            }
        }

        private SocketAsyncEventArgs SocketOperationCompleted(SocketAsyncEventArgs args)
        {
            Exception socketException = null;
            if (args.SocketError != SocketError.Success)
            {
                socketException = new IOException($"Error: {args.SocketError.ToString()}, Address: {Address}");
            }

            if (socketException is null && (args.Buffer.Length - args.Offset) > MaxMessageSize && MaxMessageSize > 0)
            {
                var messageLength = Math.Min(args.Buffer.Length - args.Offset - MaxMessageSize, MaxMessageSize);
                args.SetBuffer(args.Buffer, args.Offset + MaxMessageSize, messageLength);
                return args;
            }

            var asyncContinuation = args.UserToken as AsyncContinuation;
            var nextRequest = EndRequest(asyncContinuation, socketException);
            if (nextRequest.HasValue)
            {
                SetSocketNetworkRequest(args, nextRequest.Value);
                return args;
            }
            else
            {
                args.Completed -= _socketOperationCompletedAsync;
                args.Dispose();
                return null;
            }
        }

        public override ISocket CheckSocket()
        {
            if (_socket is null)
            {
                DoInitialize();
            }
            return _socket;
        }
    }
}
