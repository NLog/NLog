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

namespace NLog.Internal.NetworkSenders
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net.Sockets;
    using NLog.Common;

    /// <summary>
    /// Sends messages over a TCP network connection.
    /// </summary>
    internal class TcpNetworkSender : NetworkSender
    {
        private readonly Queue<SocketAsyncEventArgs> _pendingRequests = new Queue<SocketAsyncEventArgs>();

        private ISocket _socket;
        private Exception _pendingError;
        private bool _asyncOperationInProgress;
        private AsyncContinuation _closeContinuation;
        private AsyncContinuation _flushContinuation;

        /// <summary>
        /// Initializes a new instance of the <see cref="TcpNetworkSender"/> class.
        /// </summary>
        /// <param name="url">URL. Must start with tcp://.</param>
        /// <param name="addressFamily">The address family.</param>
        public TcpNetworkSender(string url, AddressFamily addressFamily)
            : base(url)
        {
            AddressFamily = addressFamily;
        }

        internal AddressFamily AddressFamily { get; set; }

        internal int MaxQueueSize { get; set; }

#if !SILVERLIGHT
        internal System.Security.Authentication.SslProtocols SslProtocols { get; set; }
#endif

        /// <summary>
        /// Creates the socket with given parameters. 
        /// </summary>
        /// <param name="host">The host address.</param>
        /// <param name="addressFamily">The address family.</param>
        /// <param name="socketType">Type of the socket.</param>
        /// <param name="protocolType">Type of the protocol.</param>
        /// <returns>Instance of <see cref="ISocket" /> which represents the socket.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "This is a factory method")]
        protected internal virtual ISocket CreateSocket(string host, AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType)
        {
            var socketProxy = new SocketProxy(addressFamily, socketType, protocolType);
#if !NETSTANDARD1_0 && !SILVERLIGHT
            if (SslProtocols != System.Security.Authentication.SslProtocols.None)
            {
                return new SslSocketProxy(host, SslProtocols, socketProxy);
            }
#endif
            return socketProxy;
        }

        /// <summary>
        /// Performs sender-specific initialization.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Object is disposed in the event handler.")]
        protected override void DoInitialize()
        {
            var uri = new Uri(Address);
            var args = new MySocketAsyncEventArgs();
            args.RemoteEndPoint = ParseEndpointAddress(new Uri(Address), AddressFamily);
            args.Completed += SocketOperationCompleted;
            args.UserToken = null;

            _socket = CreateSocket(uri.Host, args.RemoteEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            _asyncOperationInProgress = true;

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
                SocketOperationCompleted(_socket, args);
            }
        }

        /// <summary>
        /// Closes the socket.
        /// </summary>
        /// <param name="continuation">The continuation.</param>
        protected override void DoClose(AsyncContinuation continuation)
        {
            lock (_pendingRequests)
            {
                if (_asyncOperationInProgress)
                {
                    _closeContinuation = continuation;
                }
                else
                {
                    CloseSocket(continuation);
                }
            }
        }

        /// <summary>
        /// Performs sender-specific flush.
        /// </summary>
        /// <param name="continuation">The continuation.</param>
        protected override void DoFlush(AsyncContinuation continuation)
        {
            lock (_pendingRequests)
            {
                if (!_asyncOperationInProgress && _pendingRequests.Count == 0)
                {
                    continuation(null);
                }
                else
                {
                    _flushContinuation = continuation;
                }
            }
        }

        /// <summary>
        /// Sends the specified text over the connected socket.
        /// </summary>
        /// <param name="bytes">The bytes to be sent.</param>
        /// <param name="offset">Offset in buffer.</param>
        /// <param name="length">Number of bytes to send.</param>
        /// <param name="asyncContinuation">The async continuation to be invoked after the buffer has been sent.</param>
        /// <remarks>To be overridden in inheriting classes.</remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Object is disposed in the event handler.")]
        protected override void DoSend(byte[] bytes, int offset, int length, AsyncContinuation asyncContinuation)
        {
            var args = new MySocketAsyncEventArgs();
            args.SetBuffer(bytes, offset, length);
            args.UserToken = asyncContinuation;
            args.Completed += SocketOperationCompleted;

            lock (_pendingRequests)
            {
                if (MaxQueueSize != 0 && _pendingRequests.Count >= MaxQueueSize)
                {
                    var dequeued = _pendingRequests.Dequeue();

                    if (dequeued != null)
                    {
                        dequeued.Dispose();
                    }
                }

                _pendingRequests.Enqueue(args);
            }

            ProcessNextQueuedItem();
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

        private void SocketOperationCompleted(object sender, SocketAsyncEventArgs e)
        {
            lock (_pendingRequests)
            {
                _asyncOperationInProgress = false;
                var asyncContinuation = e.UserToken as AsyncContinuation;

                if (e.SocketError != SocketError.Success)
                {
                    _pendingError = new IOException($"Error: " + e.SocketError);
                }

                e.Dispose();

                if (asyncContinuation != null)
                {
                    asyncContinuation(_pendingError);
                }
            }

            ProcessNextQueuedItem();
        }

        private void ProcessNextQueuedItem()
        {
            SocketAsyncEventArgs args;

            lock (_pendingRequests)
            {
                if (_asyncOperationInProgress)
                {
                    return;
                }

                if (_pendingError != null)
                {
                    while (_pendingRequests.Count != 0)
                    {
                        args = _pendingRequests.Dequeue();
                        var asyncContinuation = (AsyncContinuation)args.UserToken;
                        args.Dispose();
                        asyncContinuation(_pendingError);
                    }
                }

                if (_pendingRequests.Count == 0)
                {
                    var fc = _flushContinuation;
                    if (fc != null)
                    {
                        _flushContinuation = null;
                        fc(_pendingError);
                    }

                    var cc = _closeContinuation;
                    if (cc != null)
                    {
                        _closeContinuation = null;
                        CloseSocket(cc);
                    }

                    return;
                }

                args = _pendingRequests.Dequeue();

                _asyncOperationInProgress = true;

                bool asyncOperation = false;
                try
                {
                    asyncOperation = _socket.SendAsync(args);
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
                    SocketOperationCompleted(_socket, args);
                }
            }
        }

        public override void CheckSocket()
        {
            if (_socket == null)
            {
                DoInitialize();
            }
        }

        /// <summary>
        /// Facilitates mocking of <see cref="SocketAsyncEventArgs"/> class.
        /// </summary>
        internal class MySocketAsyncEventArgs : SocketAsyncEventArgs
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
