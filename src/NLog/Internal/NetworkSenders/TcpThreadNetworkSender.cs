// 
// Copyright (c) 2004-2021 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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
// Copyright (c) 2021 @pruiz <pruiz@evicertia.com>, @rvinaa <jespinosa@evicertia.com>
// 

#if !NET35 && !NETSTANDARD1_3 && !NETSTANDARD1_5
namespace NLog.Internal.NetworkSenders
{
    using System;
    using System.Threading;
    using System.Diagnostics;
    using System.Net.Sockets;
    using System.Collections.Concurrent;
    using System.Security.Authentication;
    using System.Runtime.CompilerServices;

    using NLog.Common;

    internal class TcpThreadNetworkSender : NetworkSender
    {
        #region Inner types

        private struct Item
        {
            public AsyncContinuation continuation;
            public byte[] bytes;
            public int length;
        }

        #endregion

        #region Common fields & properties

        private static bool? EnableKeepAliveSuccessful;
        private const int CLOSE_SIGNAL = -31337;
        private const int FLUSH_SIGNAL = -31338;
        private BlockingCollection<Item> _items;
        private Uri _uri;

        private readonly CancellationTokenSource _cancellator = new CancellationTokenSource();
        private readonly object _lock = new object();

        internal AddressFamily AddressFamily { get; set; }
        internal SslProtocols SslProtocols { get; set; }
        internal TimeSpan KeepAliveTime { get; set; }
        internal TimeSpan ConnectionTimeout { get; set; }
        internal int MaxQueueSize { get; set; }

        #endregion

        #region .ctors

        public TcpThreadNetworkSender(string url, AddressFamily addressFamily)
            : base(url)
        {
            ReleaseOnError = false; //< XXX: We do handle errors by ourselves (like reconnecting, etc.)
            AddressFamily = addressFamily;
        }

        #endregion

        #region NetworkSender methods

        protected override void DoInitialize()
        {
            if (_thread == null)
            {
                lock (_lock)
                {
                    if (_thread == null)
                    {
                        _uri = new Uri(Address);
                        _items = new BlockingCollection<Item>(MaxQueueSize);
                        _thread = new Thread(Main)
                        {
                            Name = $"{nameof(TcpThreadNetworkSender)} - {Address}"
                        };

                        _thread.Start();
                    }
                }
            }

            base.DoInitialize();
        }

        protected override void DoSend(byte[] bytes, int offset, int length, AsyncContinuation asyncContinuation)
        {
            if (!_items.IsCompleted)
                _items.Add(new Item { bytes = bytes, length = length, continuation = asyncContinuation }, _cancellator.Token);
        }

        protected override void DoFlush(AsyncContinuation continuation)
        {
            if (_items.IsCompleted)
            {
                continuation?.Invoke(null);
                return;
            }

            _items.Add(new Item { length = FLUSH_SIGNAL, continuation = continuation }, _cancellator.Token);
        }

        protected override void DoClose(AsyncContinuation continuation)
        {
            if (_items.IsCompleted)
            {
                continuation?.Invoke(null);
                return;
            }

            lock (_items)
            {
                continuation = ex => { _thread.Join(); continuation?.Invoke(ex); };
                _items.Add(new Item { length = CLOSE_SIGNAL, continuation = continuation });
                _cancellator.Cancel();
                _items.CompleteAdding();
            }
        }

        #endregion

        #region Thread methods

        private volatile Thread _thread;
        private ISocket _socket;

        private static readonly int[] DelayIntervals = new[] { 50, 100, 250, 500, 1000 };

        private static int GetDelay(uint step) => DelayIntervals[Math.Min(DelayIntervals.Length - 1, step)];

        private static bool TrySetSocketOption(Socket socket, SocketOptionLevel level, SocketOptionName name, object value)
        {
            try
            {
                socket.SetSocketOption(level, name, value);
                return true;
            }
            catch (Exception ex)
            {
                InternalLogger.Warn(ex, "Failed to configure socket option with level: {0}, name: {1}, value: {2}.", level, name, value);
                return false;
            }
        }

        private static bool TryEnableKeepAlive(Socket socket, int keepAliveTimeSeconds)
        {
            if (TrySetSocketOption(socket, SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true))
            {
                // SOCKET OPTION NAME CONSTANT
                // Ws2ipdef.h (Windows SDK)
                // #define    TCP_KEEPALIVE      3
                // #define    TCP_KEEPINTVL      17
                var TcpKeepAliveTime = (SocketOptionName)0x3;
                var TcpKeepAliveInterval = (SocketOptionName)0x11;

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

                if (TrySetSocketOption(socket, SocketOptionLevel.Tcp, TcpKeepAliveTime, keepAliveTimeSeconds))
                {
                    // Configure retransmission interval when missing acknowledge of keep-alive-probe
                    TrySetSocketOption(socket, SocketOptionLevel.Tcp, TcpKeepAliveInterval, 1); //< Default 1 sec on Windows (75 sec on Linux)
                    return true;
                }
            }

            return false;
        }

        protected internal virtual ISocket CreateSocket(string host, AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType)
        {
            var result = new SocketProxy(addressFamily, socketType, protocolType);
            var socket = result.UnderlyingSocket;

            if (KeepAliveTime.TotalSeconds >= 1.0 && EnableKeepAliveSuccessful != false)
                EnableKeepAliveSuccessful = TryEnableKeepAlive(socket, (int)KeepAliveTime.TotalSeconds);

            if (SslProtocols != SslProtocols.None)
                return new SslSocketProxy(host, SslProtocols, result);

            return result;
        }

        private void Connect()
        {
            _socket = CreateSocket(_uri.Host, AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            var asyncResult = _socket.BeginConnect(ParseEndpointAddress(new Uri(Address), AddressFamily), null, null);
            var signalReceived = asyncResult.AsyncWaitHandle.WaitOne(ConnectionTimeout, true);

            if (!signalReceived)
            {
                try
                {
                    Close(); //< XXX: Timeout reached? Close and throw.
                }
                catch (Exception)
                {
                    // Swallow the exception since this is considered a timeout..
                }

                throw new SocketException((int)SocketError.TimedOut);
            }

            _socket.EndConnect(asyncResult);
        }

        private void Send(Item item)
        {
            var left = item.length;

            while (left > 0)
            {
                try
                {
                    left -= _socket.Send(item.bytes, (item.length - left), left, SocketFlags.None);
                    item.continuation?.Invoke(null);
                }
                catch (Exception ex) when (item.continuation != null)
                {
                    item.continuation(ex);
                    throw;
                }
            }
        }

        private void Close()
        {
            var sock = _socket;
            _socket = null;

            if (sock?.Connected != null)
                sock.Close();

            sock?.Dispose();
        }

        private void Close(Item item)
        {

            try
            {
                Close();

                item.continuation?.Invoke(null);
            }
            catch (Exception exception)
            {
                item.continuation?.Invoke(exception);
            }
        }

#if !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private bool DoWork()
        {
            if (_socket?.Connected != true)
            {
                Close();
                Connect();
            }

            var item = _items.Take();
            if (item.length == CLOSE_SIGNAL)
            {
                Close(item);
                return false;
            }
            else if (item.length == FLUSH_SIGNAL)
            {
                item.continuation?.Invoke(null);
                return true;
            }

            Send(item);
            return true;
        }

        private void Main()
        {
            uint step = 0;
            var sw = new Stopwatch();

            while (true)
            {
                try
                {
                    if (!DoWork())
                        break;

                    step = 0;
                }
                catch (Exception ex)
                {
                    InternalLogger.Error(ex, "Unexpected error while sending log data to {0}", _uri);

                    var delay = GetDelay(step++);
                    var tmout = delay;

                    sw.Restart();

                    do
                    {
                        // XXX: Flush queue if sender is not connected..
                        while (_items.TryTake(out var item, tmout < 0 ? 0 : tmout))
                        {
                            if (item.length == CLOSE_SIGNAL)
                            {
                                Close(item);
                                goto done;
                            }

                            item.continuation?.Invoke(ex);
                            tmout = (int)(delay - sw.ElapsedMilliseconds);
                        }
                    }
                    while (sw.ElapsedMilliseconds < delay);

                done:
                    sw.Stop();
                }
            }
        }

        #endregion
    }
}
#endif