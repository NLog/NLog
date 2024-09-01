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
    using System.Net.Sockets;
    using NLog.Common;

    /// <summary>
    /// Sends messages over a TCP network connection.
    /// </summary>
    internal class TcpNetworkSender : QueuedNetworkSender
    {
        private static bool? EnableKeepAliveSuccessful;
        private ISocket _socket;
        private readonly EventHandler<SocketAsyncEventArgs> _socketOperationCompletedAsync;
        private AsyncHelpersTask? _asyncBeginRequest;

        /// <summary>
        /// Initializes a new instance of the <see cref="TcpNetworkSender"/> class.
        /// </summary>
        /// <param name="url">URL. Must start with tcp://.</param>
        /// <param name="addressFamily">The address family.</param>
        public TcpNetworkSender(string url, AddressFamily addressFamily)
            : base(url)
        {
            AddressFamily = addressFamily;
            _socketOperationCompletedAsync = SocketOperationCompletedAsync;
        }

        internal AddressFamily AddressFamily { get; set; }

        internal System.Security.Authentication.SslProtocols SslProtocols { get; set; }

        internal TimeSpan KeepAliveTime { get; set; }

        internal TimeSpan SendTimeout { get; set; }

        /// <summary>
        /// Creates the socket with given parameters.
        /// </summary>
        /// <param name="host">The host address.</param>
        /// <param name="addressFamily">The address family.</param>
        /// <param name="socketType">Type of the socket.</param>
        /// <param name="protocolType">Type of the protocol.</param>
        /// <param name="sendTimeout">Socket SendTimeout.</param>
        /// <returns>Instance of <see cref="ISocket" /> which represents the socket.</returns>
        protected internal virtual ISocket CreateSocket(string host, AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType, TimeSpan sendTimeout)
        {
            var socketProxy = new SocketProxy(addressFamily, socketType, protocolType);

            if (KeepAliveTime.TotalSeconds >= 1.0 && EnableKeepAliveSuccessful != false)
            {
                EnableKeepAliveSuccessful = TryEnableKeepAlive(socketProxy.UnderlyingSocket, (int)KeepAliveTime.TotalSeconds);
            }

            if (sendTimeout > TimeSpan.Zero)
            {
                socketProxy.UnderlyingSocket.SendTimeout = (int)sendTimeout.TotalMilliseconds;
            }

#if !NETSTANDARD1_3 && !NETSTANDARD1_5
            if (SslProtocols != System.Security.Authentication.SslProtocols.None)
            {
                return new SslSocketProxy(host, SslProtocols, socketProxy);
            }
#endif
            return socketProxy;
        }

        private static bool TryEnableKeepAlive(Socket underlyingSocket, int keepAliveTimeSeconds)
        {
            if (TrySetSocketOption(underlyingSocket, SocketOptionName.KeepAlive, true))
            {
                // SOCKET OPTION NAME CONSTANT
                // Ws2ipdef.h (Windows SDK)
                // #define    TCP_KEEPALIVE      3
                // #define    TCP_KEEPINTVL      17
                SocketOptionName TcpKeepAliveTime = (SocketOptionName)0x3;
                SocketOptionName TcpKeepAliveInterval = (SocketOptionName)0x11;

                if (PlatformDetector.CurrentOS == RuntimeOS.Linux)
                {
                    // https://github.com/torvalds/linux/blob/v4.16/include/net/tcp.h
                    // #define    TCP_KEEPIDLE            4              /* Start keepalives after this period */
                    // #define    TCP_KEEPINTVL           5              /* Interval between keepalives */
                    TcpKeepAliveTime = (SocketOptionName)0x4;
                    TcpKeepAliveInterval = (SocketOptionName)0x5;
                }
                else if (PlatformDetector.CurrentOS == RuntimeOS.MacOSX)
                {
                    // https://opensource.apple.com/source/xnu/xnu-4570.41.2/bsd/netinet/tcp.h.auto.html
                    // #define    TCP_KEEPALIVE      0x10                      /* idle time used when SO_KEEPALIVE is enabled */
                    // #define    TCP_KEEPINTVL      0x101                     /* interval between keepalives */
                    TcpKeepAliveTime = (SocketOptionName)0x10;
                    TcpKeepAliveInterval = (SocketOptionName)0x101;
                }

                if (TrySetTcpOption(underlyingSocket, TcpKeepAliveTime, keepAliveTimeSeconds))
                {
                    // Configure retransmission interval when missing acknowledge of keep-alive-probe
                    TrySetTcpOption(underlyingSocket, TcpKeepAliveInterval, 1); // Default 1 sec on Windows (75 sec on Linux)
                    return true;
                }
            }

            return false;
        }

        private static bool TrySetSocketOption(Socket underlyingSocket, SocketOptionName socketOption, bool value)
        {
            try
            {
                underlyingSocket.SetSocketOption(SocketOptionLevel.Socket, socketOption, value);
                return true;
            }
            catch (Exception ex)
            {
                InternalLogger.Warn(ex, "NetworkTarget: Failed to configure Socket-option {0} = {1}", socketOption, value);
                return false;
            }
        }

        private static bool TrySetTcpOption(Socket underlyingSocket, SocketOptionName socketOption, int value)
        {
            try
            {
                underlyingSocket.SetSocketOption(SocketOptionLevel.Tcp, socketOption, value);
                return true;
            }
            catch (Exception ex)
            {
                InternalLogger.Warn(ex, "NetworkTarget: Failed to configure TCP-option {0} = {1}", socketOption, value);
                return false;
            }
        }

        protected override void DoInitialize()
        {
            var uri = new Uri(Address);
            var args = new MySocketAsyncEventArgs();
            var ipAddress = ResolveIpAddress(uri, AddressFamily);
            args.RemoteEndPoint = new System.Net.IPEndPoint(ipAddress, uri.Port);
            args.Completed += _socketOperationCompletedAsync;
            args.UserToken = null;

            _socket = CreateSocket(uri.Host, args.RemoteEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp, SendTimeout);
            base.BeginInitialize();

            bool asyncOperation = false;
            try
            {
                asyncOperation = _socket.ConnectAsync(args);
            }
            catch (SocketException ex)
            {
                args.SocketError = ex.SocketErrorCode;
            }
            catch (Exception ex)
            {
                if (ex.InnerException is SocketException socketException)
                    args.SocketError = socketException.SocketErrorCode;
                else
                    args.SocketError = SocketError.OperationAborted;
            }

            if (!asyncOperation)
            {
                SocketOperationCompletedAsync(_socket, args);
            }
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
                if (exception.MustBeRethrown())
                {
                    throw;
                }

                continuation(exception);
            }
        }

        protected override void BeginRequest(NetworkRequestArgs eventArgs)
        {
            var socketEventArgs = new MySocketAsyncEventArgs();
            socketEventArgs.Completed += _socketOperationCompletedAsync;
            SetSocketNetworkRequest(socketEventArgs, eventArgs);

            // Schedule async network operation to avoid blocking socket-operation (Allow adding more request)
            if (_asyncBeginRequest is null)
                _asyncBeginRequest = new AsyncHelpersTask(BeginRequestAsync);
            AsyncHelpers.StartAsyncTask(_asyncBeginRequest.Value, socketEventArgs);
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
                    asyncOperation = _socket.SendAsync(args);
                }
                catch (SocketException ex)
                {
                    InternalLogger.Error(ex, "NetworkTarget: Error sending tcp request");
                    args.SocketError = ex.SocketErrorCode;
                }
                catch (Exception ex)
                {
                    InternalLogger.Error(ex, "NetworkTarget: Error sending tcp request");
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
            socketEventArgs.SetBuffer(networkRequest.RequestBuffer, networkRequest.RequestBufferOffset, networkRequest.RequestBufferLength);
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

        /// <summary>
        /// Facilitates mocking of <see cref="SocketAsyncEventArgs"/> class.
        /// </summary>
        internal sealed class MySocketAsyncEventArgs : SocketAsyncEventArgs
        {
            /// <summary>
            /// Raises the Completed event.
            /// </summary>
            public void RaiseCompleted()
            {
                OnCompleted(this);
            }
        }
    }
}
