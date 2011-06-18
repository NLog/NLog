// 
// Copyright (c) 2004-2011 Jaroslaw Kowalski <jaak@jkowalski.net>
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

#if !WINDOWS_PHONE_7

namespace NLog.UnitTests.Internal.NetworkSenders
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading;
    using NUnit.Framework;

#if !NUNIT
    using SetUp = Microsoft.VisualStudio.TestTools.UnitTesting.TestInitializeAttribute;
    using TestFixture = Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute;
    using Test = Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute;
    using TearDown =  Microsoft.VisualStudio.TestTools.UnitTesting.TestCleanupAttribute;
#endif
    using NLog.Internal.NetworkSenders;

    [TestFixture]
    public class TcpNetworkSenderTests : NLogTestBase
    {
        [Test]
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
                string expectedLog = @"Parse endpoint address tcp://hostname:123/ Unspecified
create socket 10000 Stream Tcp
connect async to {mock end point: tcp://hostname:123/}
send async 0 1 'q'
send async 0 2 'qu'
send async 0 4 'quic'
";

                Assert.AreEqual(expectedLog, sender.Log.ToString());

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
                expectedLog = @"Parse endpoint address tcp://hostname:123/ Unspecified
create socket 10000 Stream Tcp
connect async to {mock end point: tcp://hostname:123/}
send async 0 1 'q'
send async 0 2 'qu'
send async 0 4 'quic'
send async 0 1 'q'
send async 0 2 'qu'
send async 0 4 'quic'
close
";

                Assert.AreEqual(expectedLog, sender.Log.ToString());
                foreach (var ex in exceptions)
                {
                    Assert.IsNull(ex);
                }
            }
        }

        [Test]
        public void TcpProxyTest()
        {
            var sender = new TcpNetworkSender("tcp://foo:1234", AddressFamily.Unspecified);
            var socket = sender.CreateSocket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            Assert.IsInstanceOfType(typeof(SocketProxy), socket);
        }

        [Test]
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

#if SILVERLIGHT
            Assert.IsTrue(allSent.WaitOne(3000));
#else
            Assert.IsTrue(allSent.WaitOne(3000, false));
#endif

            var mre = new ManualResetEvent(false);
            sender.FlushAsync(ex => mre.Set());
#if SILVERLIGHT
            mre.WaitOne(3000);
#else
            mre.WaitOne(3000, false);
#endif
            string expectedLog = @"Parse endpoint address tcp://hostname:123/ Unspecified
create socket 10000 Stream Tcp
connect async to {mock end point: tcp://hostname:123/}
failed
";

            Assert.AreEqual(expectedLog, sender.Log.ToString());
            foreach (var ex in exceptions)
            {
                Assert.IsNotNull(ex);
            }
        }

        [Test]
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
            string expectedLog = @"Parse endpoint address tcp://hostname:123/ Unspecified
create socket 10000 Stream Tcp
connect async to {mock end point: tcp://hostname:123/}
send async 0 1 'q'
send async 0 2 'qu'
send async 0 3 'qui'
failed
close
";

            Assert.AreEqual(expectedLog, sender.Log.ToString());
            for (int i = 0; i < exceptions.Length; ++i)
            {
                if (i < 2)
                {
                    Assert.IsNull(exceptions[i], "EXCEPTION: " + exceptions[i]);
                }
                else
                {
                    Assert.IsNotNull(exceptions[i]);
                }
            }
        }

        internal class MyTcpNetworkSender : TcpNetworkSender
        {
            public StringWriter Log { get; set; }

            public MyTcpNetworkSender(string url, AddressFamily addressFamily)
                : base(url, addressFamily)
            {
                this.Log = new StringWriter();
            }

            protected internal override ISocket CreateSocket(AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType)
            {
                return new MockSocket(addressFamily, socketType, protocolType, this);
            }

            protected override EndPoint ParseEndpointAddress(Uri uri, AddressFamily addressFamily)
            {
                this.Log.WriteLine("Parse endpoint address {0} {1}", uri, addressFamily);
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
            private bool faulted = false;

            public MockSocket(AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType, MyTcpNetworkSender sender)
            {
                this.sender = sender;
                this.log = sender.Log;
                this.log.WriteLine("create socket {0} {1} {2}", addressFamily, socketType, protocolType);
            }

            public bool ConnectAsync(SocketAsyncEventArgs args)
            {
                this.log.WriteLine("connect async to {0}", args.RemoteEndPoint);

                lock (this)
                {
                    if (this.sender.ConnectFailure > 0)
                    {
                        this.sender.ConnectFailure--;
                        this.faulted = true;
                        args.SocketError = SocketError.SocketError;
                        this.log.WriteLine("failed");
                    }
                }

                return InvokeCallback(args);
            }

            private bool InvokeCallback(SocketAsyncEventArgs args)
            {
                lock (this)
                {
                    var args2 = args as TcpNetworkSender.MySocketAsyncEventArgs;

                    if (this.sender.Async)
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
                    this.log.WriteLine("close");
                }
            }

            public bool SendAsync(SocketAsyncEventArgs args)
            {
                lock (this)
                {
                    this.log.WriteLine("send async {0} {1} '{2}'", args.Offset, args.Count, Encoding.UTF8.GetString(args.Buffer, args.Offset, args.Count));
                    if (this.sender.SendFailureIn > 0)
                    {
                        this.sender.SendFailureIn--;
                        if (this.sender.SendFailureIn == 0)
                        {
                            this.faulted = true;
                        }
                    }

                    if (this.faulted)
                    {
                        this.log.WriteLine("failed");
                        args.SocketError = SocketError.SocketError;
                    }
                }

                return InvokeCallback(args);
            }

            public bool SendToAsync(SocketAsyncEventArgs args)
            {
                lock (this)
                {
                    this.log.WriteLine("sendto async {0} {1} '{2}' {3}", args.Offset, args.Count, Encoding.UTF8.GetString(args.Buffer, args.Offset, args.Count), args.RemoteEndPoint);
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

            public override AddressFamily AddressFamily
            {
                get
                {
                    return (System.Net.Sockets.AddressFamily)10000;
                }
            }

            public override string ToString()
            {
                return "{mock end point: " + this.uri + "}";
            }
        }
    }
}

#endif