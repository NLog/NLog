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

namespace NLog.UnitTests.Internal.NetworkSenders
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading;
    using NLog.Internal.NetworkSenders;
    using Xunit;

    public class TcpNetworkSenderTests : NLogTestBase
    {
        [Fact]
        public void TcpHappyPathTest()
        {
            foreach (bool async in new[] { false, true })
            {
                var sender = new MyTcpNetworkSender("tcp://hostname:123", AddressFamily.Unspecified)
                {
                    Async = async,
                };

                sender.Initialize();
                byte[] buffer = Encoding.UTF8.GetBytes("quick brown fox jumps over the lazy dog");

                var exceptions = new List<Exception>();

                for (int i = 1; i < 8; i *= 2)
                {
                    sender.Send(
                        buffer, 0, i, ex =>
                        {
                            lock (exceptions) exceptions.Add(ex);
                        });
                }

                var mre = new ManualResetEvent(false);

                sender.FlushAsync(ex =>
                    {
                        lock (exceptions)
                        {
                            exceptions.Add(ex);
                        }

                        mre.Set();
                    });

                mre.WaitOne();

                var actual = sender.Log.ToString();
                Assert.True(actual.IndexOf("Parse endpoint address tcp://hostname:123/ Unspecified") != -1);
                Assert.True(actual.IndexOf("create socket 10000 Stream Tcp") != -1);
                Assert.True(actual.IndexOf("connect async to {mock end point: tcp://hostname:123/}") != -1);
                Assert.True(actual.IndexOf("send async 0 1 'q'") != -1);
                Assert.True(actual.IndexOf("send async 0 2 'qu'") != -1);
                Assert.True(actual.IndexOf("send async 0 4 'quic'") != -1);

                mre.Reset();
                for (int i = 1; i < 8; i *= 2)
                {
                    sender.Send(
                        buffer, 0, i, ex =>
                        {
                            lock (exceptions) exceptions.Add(ex);
                        });
                }

                sender.Close(ex =>
                    {
                        lock (exceptions)
                        {
                            exceptions.Add(ex);
                        }

                        mre.Set();
                    });

                mre.WaitOne();

                actual = sender.Log.ToString();
                
                Assert.True(actual.IndexOf("Parse endpoint address tcp://hostname:123/ Unspecified") != -1);
                Assert.True(actual.IndexOf("create socket 10000 Stream Tcp") != -1);
                Assert.True(actual.IndexOf("connect async to {mock end point: tcp://hostname:123/}") != -1);
                Assert.True(actual.IndexOf("send async 0 1 'q'") != -1);
                Assert.True(actual.IndexOf("send async 0 2 'qu'") != -1);
                Assert.True(actual.IndexOf("send async 0 4 'quic'") != -1);
                Assert.True(actual.IndexOf("send async 0 1 'q'") != -1);
                Assert.True(actual.IndexOf("send async 0 2 'qu'") != -1);
                Assert.True(actual.IndexOf("send async 0 4 'quic'") != -1);
                Assert.True(actual.IndexOf("close") != -1);

                foreach (var ex in exceptions)
                {
                    Assert.Null(ex);
                }
            }
        }

        [Fact]
        public void TcpProxyTest()
        {
            var sender = new TcpNetworkSender("tcp://foo:1234", AddressFamily.Unspecified);
            var socket = sender.CreateSocket("foo", AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            Assert.IsType<SocketProxy>(socket);
        }

        [Fact]
        public void TcpConnectFailureTest()
        {
            var sender = new MyTcpNetworkSender("tcp://hostname:123", AddressFamily.Unspecified)
            {
                ConnectFailure = 1,
                Async = true,
            };

            sender.Initialize();
            byte[] buffer = Encoding.UTF8.GetBytes("quick brown fox jumps over the lazy dog");

            var exceptions = new List<Exception>();
            var allSent = new ManualResetEvent(false);

            for (int i = 1; i < 8; i++)
            {
                sender.Send(
                    buffer, 0, i, ex =>
                    {
                        lock (exceptions)
                        {
                            exceptions.Add(ex);
                            if (exceptions.Count == 7)
                            {
                                allSent.Set();
                            }
                        }
                    });
            }

            Assert.True(allSent.WaitOne(3000, false));

            var mre = new ManualResetEvent(false);
            sender.FlushAsync(ex => mre.Set());
            mre.WaitOne(3000, false);

            var actual = sender.Log.ToString();

            Assert.True(actual.IndexOf("Parse endpoint address tcp://hostname:123/ Unspecified") != -1);
            Assert.True(actual.IndexOf("create socket 10000 Stream Tcp") != -1);
            Assert.True(actual.IndexOf("connect async to {mock end point: tcp://hostname:123/}") != -1);
            Assert.True(actual.IndexOf("failed") != -1);

            foreach (var ex in exceptions)
            {
                Assert.NotNull(ex);
            }
        }

        [Fact]
        public void TcpSendFailureTest()
        {
            var sender = new MyTcpNetworkSender("tcp://hostname:123", AddressFamily.Unspecified)
            {
                SendFailureIn = 3, // will cause failure on 3rd send
                Async = true,
            };

            sender.Initialize();
            byte[] buffer = Encoding.UTF8.GetBytes("quick brown fox jumps over the lazy dog");

            var exceptions = new Exception[9];

            var writeFinished = new ManualResetEvent(false);
            int remaining = exceptions.Length;

            for (int i = 1; i < 10; i++)
            {
                int pos = i - 1;

                sender.Send(
                    buffer, 0, i, ex =>
                    {
                        lock (exceptions)
                        {
                            exceptions[pos] = ex;
                            if (--remaining == 0)
                            {
                                writeFinished.Set();
                            }
                        }
                    });
            }

            var mre = new ManualResetEvent(false);
            writeFinished.WaitOne();
            sender.Close(ex => mre.Set());
            mre.WaitOne();

            var actual = sender.Log.ToString();
            Assert.True(actual.IndexOf("Parse endpoint address tcp://hostname:123/ Unspecified") != -1);
            Assert.True(actual.IndexOf("create socket 10000 Stream Tcp") != -1);
            Assert.True(actual.IndexOf("connect async to {mock end point: tcp://hostname:123/}") != -1);
            Assert.True(actual.IndexOf("send async 0 1 'q'") != -1);
            Assert.True(actual.IndexOf("send async 0 2 'qu'") != -1);
            Assert.True(actual.IndexOf("send async 0 3 'qui'") != -1);
            Assert.True(actual.IndexOf("failed") != -1);
            Assert.True(actual.IndexOf("close") != -1);

            for (int i = 0; i < exceptions.Length; ++i)
            {
                if (i < 2)
                {
                    Assert.Null(exceptions[i]);
                }
                else
                {
                    Assert.NotNull(exceptions[i]);
                }
            }
        }

        internal class MyTcpNetworkSender : TcpNetworkSender
        {
            public StringWriter Log { get; set; }

            public MyTcpNetworkSender(string url, AddressFamily addressFamily)
                : base(url, addressFamily)
            {
                Log = new StringWriter();
            }

            protected internal override ISocket CreateSocket(string host, AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType)
            {
                return new MockSocket(addressFamily, socketType, protocolType, this);
            }

            protected override EndPoint ParseEndpointAddress(Uri uri, AddressFamily addressFamily)
            {
                Log.WriteLine("Parse endpoint address {0} {1}", uri, addressFamily);
                return new MockEndPoint(uri);
            }

            public int ConnectFailure { get; set; }

            public bool Async { get; set; }

            public int SendFailureIn { get; set; }
        }

        internal class MockSocket : ISocket
        {
            private readonly MyTcpNetworkSender sender;
            private readonly StringWriter log;
            private bool faulted;

            public MockSocket(AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType, MyTcpNetworkSender sender)
            {
                this.sender = sender;
                log = sender.Log;
                log.WriteLine("create socket {0} {1} {2}", addressFamily, socketType, protocolType);
            }

            public bool ConnectAsync(SocketAsyncEventArgs args)
            {
                log.WriteLine("connect async to {0}", args.RemoteEndPoint);

                lock (this)
                {
                    if (sender.ConnectFailure > 0)
                    {
                        sender.ConnectFailure--;
                        faulted = true;
                        args.SocketError = SocketError.SocketError;
                        log.WriteLine("failed");
                    }
                }

                return InvokeCallback(args);
            }

            private bool InvokeCallback(SocketAsyncEventArgs args)
            {
                lock (this)
                {
                    var args2 = args as TcpNetworkSender.MySocketAsyncEventArgs;

                    if (sender.Async)
                    {
                        ThreadPool.QueueUserWorkItem(s =>
                            {
                                Thread.Sleep(10);
                                args2.RaiseCompleted();
                            });

                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            public void Close()
            {
                lock (this)
                {
                    log.WriteLine("close");
                }
            }

            public bool SendAsync(SocketAsyncEventArgs args)
            {
                lock (this)
                {
                    log.WriteLine("send async {0} {1} '{2}'", args.Offset, args.Count, Encoding.UTF8.GetString(args.Buffer, args.Offset, args.Count));
                    if (sender.SendFailureIn > 0)
                    {
                        sender.SendFailureIn--;
                        if (sender.SendFailureIn == 0)
                        {
                            faulted = true;
                        }
                    }

                    if (faulted)
                    {
                        log.WriteLine("failed");
                        args.SocketError = SocketError.SocketError;
                    }
                }

                return InvokeCallback(args);
            }

            public bool SendToAsync(SocketAsyncEventArgs args)
            {
                lock (this)
                {
                    log.WriteLine("sendto async {0} {1} '{2}' {3}", args.Offset, args.Count, Encoding.UTF8.GetString(args.Buffer, args.Offset, args.Count), args.RemoteEndPoint);
                    return InvokeCallback(args);
                }
            }
        }

        internal class MockEndPoint : EndPoint
        {
            private readonly Uri uri;

            public MockEndPoint(Uri uri)
            {
                this.uri = uri;
            }

            public override AddressFamily AddressFamily => (AddressFamily)10000;

            public override string ToString()
            {
                return "{mock end point: " + uri + "}";
            }
        }
    }
}
