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

namespace NLog.UnitTests.Internal.NetworkSenders
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using NLog.Internal.NetworkSenders;

    [TestClass]
    public class TcpNetworkSenderTests : NLogTestBase
    {
        [TestMethod]
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
                    sender.Send(buffer, 0, i, exceptions.Add);
                }

                var mre = new ManualResetEvent(false);

                sender.FlushAsync(ex =>
                    {
                        exceptions.Add(ex);
                        mre.Set();
                    });

                mre.WaitOne();
                string expectedLog = @"Parse endpoint address tcp://hostname:123/ Unspecified
create socket 10000 Stream Tcp
connect async to {mock end point: tcp://hostname:123/}
async op completed
send async 0 1 'q'
async op completed
send async 0 2 'qu'
async op completed
send async 0 4 'quic'
async op completed
";

                Assert.AreEqual(expectedLog, sender.Log.ToString());

                mre.Reset();
                for (int i = 1; i < 8; i *= 2)
                {
                    sender.Send(buffer, 0, i, exceptions.Add);
                }

                sender.Close(ex =>
                    {
                        exceptions.Add(ex);
                        mre.Set();
                    });

                mre.WaitOne();
                expectedLog = @"Parse endpoint address tcp://hostname:123/ Unspecified
create socket 10000 Stream Tcp
connect async to {mock end point: tcp://hostname:123/}
async op completed
send async 0 1 'q'
async op completed
send async 0 2 'qu'
async op completed
send async 0 4 'quic'
async op completed
send async 0 1 'q'
async op completed
send async 0 2 'qu'
async op completed
send async 0 4 'quic'
async op completed
close
";

                Assert.AreEqual(expectedLog, sender.Log.ToString());
                foreach (var ex in exceptions)
                {
                    Assert.IsNull(ex);
                }
            }
        }

        [TestMethod]
        public void TcpProxyTest()
        {
            var sender = new TcpNetworkSender("tcp://foo:1234", AddressFamily.Unspecified);
            var socket = sender.CreateSocket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            Assert.IsInstanceOfType(socket, typeof(SocketProxy));
        }

        [TestMethod]
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

            for (int i = 1; i < 8; i *= 2)
            {
                sender.Send(buffer, 0, i, exceptions.Add);
            }

            var mre = new ManualResetEvent(false);

            sender.FlushAsync(ex => mre.Set());
            mre.WaitOne();
            string expectedLog = @"Parse endpoint address tcp://hostname:123/ Unspecified
create socket 10000 Stream Tcp
connect async to {mock end point: tcp://hostname:123/}
async op completed
";

            Assert.AreEqual(expectedLog, sender.Log.ToString());
            foreach (var ex in exceptions)
            {
                Assert.IsNotNull(ex);
            }
        }

        [TestMethod]
        public void TcpSendFailureTest()
        {
            var sender = new MyTcpNetworkSender("tcp://hostname:123", AddressFamily.Unspecified)
            {
                SendFailureIn = 3, // will cause failure on 3rd send
                Async = true,
            };

            sender.Initialize();
            byte[] buffer = Encoding.UTF8.GetBytes("quick brown fox jumps over the lazy dog");

            var exceptions = new List<Exception>();

            for (int i = 1; i < 10; i++)
            {
                sender.Send(buffer, 0, i, exceptions.Add);
            }

            var mre = new ManualResetEvent(false);

            sender.Close(ex => mre.Set());
            mre.WaitOne();
            string expectedLog = @"Parse endpoint address tcp://hostname:123/ Unspecified
create socket 10000 Stream Tcp
connect async to {mock end point: tcp://hostname:123/}
async op completed
send async 0 1 'q'
async op completed
send async 0 2 'qu'
async op completed
send async 0 3 'qui'
async op completed
close
";

            Assert.AreEqual(expectedLog, sender.Log.ToString());
            for (int i = 0; i < exceptions.Count; ++i)
            {
                if (i < 2)
                {
                    Assert.IsNull(exceptions[i]);
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

            public MockSocket(AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType, MyTcpNetworkSender sender)
            {
                this.sender = sender;
                this.log = sender.Log;
                this.log.WriteLine("create socket {0} {1} {2}", addressFamily, socketType, protocolType);
            }

            public bool ConnectAsync(SocketAsyncEventArgs args)
            {
                this.log.WriteLine("connect async to {0}", args.RemoteEndPoint);
                if (this.sender.ConnectFailure > 0)
                {
                    this.sender.ConnectFailure--;
                    args.SocketError = SocketError.SocketError;
                }

                return InvokeCallback(args);
            }

            private bool InvokeCallback(SocketAsyncEventArgs args)
            {
                var args2 = args as TcpNetworkSender.MySocketAsyncEventArgs;

                if (this.sender.Async)
                {
                    ThreadPool.QueueUserWorkItem(s =>
                        {
                            Thread.Sleep(10);
                            this.log.WriteLine("async op completed");

                            args2.RaiseCompleted();
                        });

                    return true;
                }
                else
                {
                    this.log.WriteLine("async op completed");
                    args2.RaiseCompleted();
                    return false;
                }
            }

            public void Close()
            {
                this.log.WriteLine("close");
            }

            public bool SendAsync(SocketAsyncEventArgs args)
            {
                this.log.WriteLine("send async {0} {1} '{2}'", args.Offset, args.Count, Encoding.UTF8.GetString(args.Buffer, args.Offset, args.Count));
                if (this.sender.SendFailureIn > 0)
                {
                    this.sender.SendFailureIn--;
                    if (this.sender.SendFailureIn == 0)
                    {
                        args.SocketError = SocketError.SocketError;
                    }
                }

                return InvokeCallback(args);
            }

            public bool SendToAsync(SocketAsyncEventArgs args)
            {
                this.log.WriteLine("sendto async {0} {1} '{2}' {3}", args.Offset, args.Count, Encoding.UTF8.GetString(args.Buffer, args.Offset, args.Count), args.RemoteEndPoint);
                return InvokeCallback(args);
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