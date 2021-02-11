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

namespace NLog.UnitTests.Targets
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using System.Security.Authentication;
    using System.Text;
    using System.Threading;
    using NLog.Common;
    using NLog.Config;
    using NLog.Internal.NetworkSenders;
    using NLog.Targets;
    using Xunit;

    public class NetworkTargetTests : NLogTestBase
    {
        [Fact]
        public void HappyPathDefaultsTest()
        {
            HappyPathTest(false, LineEndingMode.CRLF, "msg1", "msg2", "msg3");
        }

        [Fact]
        public void HappyPathCRLFTest()
        {
            HappyPathTest(true, LineEndingMode.CRLF, "msg1", "msg2", "msg3");
        }

        [Fact]
        public void HappyPathLFTest()
        {
            HappyPathTest(true, LineEndingMode.LF, "msg1", "msg2", "msg3");
        }

        private void HappyPathTest(bool newLine, LineEndingMode lineEnding, params string[] messages)
        {
            var senderFactory = new MySenderFactory();
            var target = new NetworkTarget();
            target.Address = "tcp://someaddress/";
            target.SenderFactory = senderFactory;
            target.Layout = "${message}";
            target.NewLine = newLine;
            target.LineEnding = lineEnding;
            target.KeepConnection = true;
            target.Initialize(null);

            var exceptions = new List<Exception>();
            var mre = new ManualResetEvent(false);
            int remaining = 3;
            AsyncContinuation asyncContinuation = ex =>
                {
                    lock (exceptions)
                    {
                        exceptions.Add(ex);
                        if (--remaining == 0)
                        {
                            mre.Set();
                        }
                    }
                };

            target.WriteAsyncLogEvent(new LogEventInfo(LogLevel.Info, "logger", "msg1").WithContinuation(asyncContinuation));
            target.WriteAsyncLogEvent(new LogEventInfo(LogLevel.Info, "logger", "msg2").WithContinuation(asyncContinuation));
            target.WriteAsyncLogEvent(new LogEventInfo(LogLevel.Info, "logger", "msg3").WithContinuation(asyncContinuation));

            mre.WaitOne();
            foreach (var ex in exceptions)
            {
                if (ex != null)
                {
                    Assert.True(false, ex.ToString());
                }
            }

            Assert.Single(senderFactory.Senders);

            var sender = senderFactory.Senders[0];
            target.Close();

            // Get the length of all the messages and their line endings
            var eol = newLine ? lineEnding.NewLineCharacters : string.Empty;
            var eolLength = eol.Length;
            var length = messages.Sum(m => m.Length) + (eolLength * messages.Length);
            Assert.Equal(length, sender.MemoryStream.Length);
            Assert.Equal(string.Join(eol, messages) + eol, target.Encoding.GetString(sender.MemoryStream.GetBuffer(), 0, (int)sender.MemoryStream.Length));

            // we invoke the sender for each message, each time sending 4 bytes
            var actual = senderFactory.Log.ToString();

            Assert.True(actual.IndexOf("1: connect tcp://someaddress/") != -1);
            foreach (var message in messages)
            {
                Assert.True(actual.IndexOf($"1: send 0 {message.Length + eolLength}") != -1);
            }
            Assert.True(actual.IndexOf("1: close") != -1);
        }

        [Fact]
        public void NetworkTargetDefaultsTest()
        {
            var target = new NetworkTarget();

            Assert.True(target.KeepConnection);
            Assert.False(target.NewLine);
            Assert.Equal("\r\n", target.LineEnding.NewLineCharacters);
            Assert.Equal(65000, target.MaxMessageSize);
            Assert.Equal(5, target.ConnectionCacheSize);
            Assert.Equal(0, target.MaxConnections);
            Assert.Equal(0, target.MaxQueueSize);
            Assert.Equal(Encoding.UTF8, target.Encoding);
        }

        [Fact]
        public void NetworkTargetMultipleConnectionsTest()
        {
            var senderFactory = new MySenderFactory();
            var target = new NetworkTarget();
            target.Address = "tcp://${logger}.company.lan/";
            target.SenderFactory = senderFactory;
            target.Layout = "${message}";
            target.KeepConnection = true;
            target.Initialize(null);

            var exceptions = new List<Exception>();
            var mre = new ManualResetEvent(false);
            int remaining = 3;
            AsyncContinuation asyncContinuation = ex =>
            {
                lock (exceptions)
                {
                    exceptions.Add(ex);
                    if (--remaining == 0)
                    {
                        mre.Set();
                    }
                }
            };

            target.WriteAsyncLogEvent(new LogEventInfo(LogLevel.Info, "logger1", "msg1").WithContinuation(asyncContinuation));
            target.WriteAsyncLogEvent(new LogEventInfo(LogLevel.Info, "logger2", "msg2").WithContinuation(asyncContinuation));
            target.WriteAsyncLogEvent(new LogEventInfo(LogLevel.Info, "logger3", "msg3").WithContinuation(asyncContinuation));

            mre.WaitOne();
            foreach (var ex in exceptions)
            {
                if (ex != null)
                {
                    Assert.True(false, ex.ToString());
                }
            }

            mre.Reset();
            AsyncContinuation flushContinuation = ex =>
            {
                mre.Set();
            };

            target.Flush(flushContinuation);
            mre.WaitOne();
            target.Close();

            var actual = senderFactory.Log.ToString();
            Assert.True(actual.IndexOf("1: connect tcp://logger1.company.lan/") != -1);
            Assert.True(actual.IndexOf("1: send 0 4") != -1);
            Assert.True(actual.IndexOf("2: connect tcp://logger2.company.lan/") != -1);
            Assert.True(actual.IndexOf("2: send 0 4") != -1);
            Assert.True(actual.IndexOf("3: connect tcp://logger3.company.lan/") != -1);
            Assert.True(actual.IndexOf("3: send 0 4") != -1);
            Assert.True(actual.IndexOf("1: flush") != -1);
            Assert.True(actual.IndexOf("2: flush") != -1);
            Assert.True(actual.IndexOf("3: flush") != -1);
            Assert.True(actual.IndexOf("1: close") != -1);
            Assert.True(actual.IndexOf("2: close") != -1);
            Assert.True(actual.IndexOf("3: close") != -1);
        }

        [Fact]
        public void NothingToFlushTest()
        {
            var senderFactory = new MySenderFactory();
            var target = new NetworkTarget();
            target.Address = "tcp://${logger}.company.lan/";
            target.SenderFactory = senderFactory;
            target.Layout = "${message}";
            target.KeepConnection = true;
            target.Initialize(null);

            var mre = new ManualResetEvent(false);

            AsyncContinuation flushContinuation = ex =>
            {
                mre.Set();
            };

            target.Flush(flushContinuation);
            mre.WaitOne();
            target.Close();

            string expectedLog = @"";
            Assert.Equal(expectedLog, senderFactory.Log.ToString());
        }

        [Fact]
        public void NetworkTargetMultipleConnectionsWithCacheOverflowTest()
        {
            var senderFactory = new MySenderFactory();
            var target = new NetworkTarget();
            target.Address = "tcp://${logger}.company.lan/";
            target.SenderFactory = senderFactory;
            target.Layout = "${message}";
            target.KeepConnection = true;
            target.ConnectionCacheSize = 2;
            target.Initialize(null);

            var exceptions = new List<Exception>();
            var mre = new ManualResetEvent(false);
            int remaining = 6;
            AsyncContinuation asyncContinuation = ex =>
            {
                lock (exceptions)
                {
                    exceptions.Add(ex);
                    if (--remaining == 0)
                    {
                        mre.Set();
                    }
                }
            };

            // logger1 should be kept alive because it's being referenced frequently
            target.WriteAsyncLogEvent(new LogEventInfo(LogLevel.Info, "logger1", "msg1").WithContinuation(asyncContinuation));
            target.WriteAsyncLogEvent(new LogEventInfo(LogLevel.Info, "logger2", "msg2").WithContinuation(asyncContinuation));
            target.WriteAsyncLogEvent(new LogEventInfo(LogLevel.Info, "logger1", "msg3").WithContinuation(asyncContinuation));
            target.WriteAsyncLogEvent(new LogEventInfo(LogLevel.Info, "logger3", "msg1").WithContinuation(asyncContinuation));
            target.WriteAsyncLogEvent(new LogEventInfo(LogLevel.Info, "logger1", "msg2").WithContinuation(asyncContinuation));
            target.WriteAsyncLogEvent(new LogEventInfo(LogLevel.Info, "logger2", "msg3").WithContinuation(asyncContinuation));

            mre.WaitOne();
            foreach (var ex in exceptions)
            {
                if (ex != null)
                {
                    Assert.True(false, ex.ToString());
                }
            }

            target.Close();

            string result = senderFactory.Log.ToString();
            Assert.True(result.IndexOf("1: connect tcp://logger1.company.lan/") != -1);
            Assert.True(result.IndexOf("1: send 0 4") != -1);
            Assert.True(result.IndexOf("2: connect tcp://logger2.company.lan/") != -1);
            Assert.True(result.IndexOf("2: send 0 4") != -1);
            Assert.True(result.IndexOf("1: send 0 4") != -1);
            Assert.True(result.IndexOf("2: close") != -1);
            Assert.True(result.IndexOf("3: connect tcp://logger3.company.lan/") != -1);
            Assert.True(result.IndexOf("3: send 0 4") != -1);
            Assert.True(result.IndexOf("1: send 0 4") != -1);
            Assert.True(result.IndexOf("3: close") != -1);
            Assert.True(result.IndexOf("4: connect tcp://logger2.company.lan/") != -1);
            Assert.True(result.IndexOf("4: send 0 4") != -1);
            Assert.True(result.IndexOf("1: close") != -1);
            Assert.True(result.IndexOf("4: close") != -1);
        }

        [Fact]
        public void NetworkTargetMultipleConnectionsWithoutKeepAliveTest()
        {
            var senderFactory = new MySenderFactory();
            var target = new NetworkTarget();
            target.Address = "tcp://${logger}.company.lan/";
            target.SenderFactory = senderFactory;
            target.Layout = "${message}";
            target.KeepConnection = false;
            target.Initialize(null);

            var exceptions = new List<Exception>();
            var mre = new ManualResetEvent(false);
            int remaining = 6;
            AsyncContinuation asyncContinuation = ex =>
            {
                lock (exceptions)
                {
                    exceptions.Add(ex);
                    if (--remaining == 0)
                    {
                        mre.Set();
                    }
                }
            };

            target.WriteAsyncLogEvent(new LogEventInfo(LogLevel.Info, "logger1", "msg1").WithContinuation(asyncContinuation));
            target.WriteAsyncLogEvent(new LogEventInfo(LogLevel.Info, "logger2", "msg2").WithContinuation(asyncContinuation));
            target.WriteAsyncLogEvent(new LogEventInfo(LogLevel.Info, "logger1", "msg3").WithContinuation(asyncContinuation));
            target.WriteAsyncLogEvent(new LogEventInfo(LogLevel.Info, "logger3", "msg1").WithContinuation(asyncContinuation));
            target.WriteAsyncLogEvent(new LogEventInfo(LogLevel.Info, "logger1", "msg2").WithContinuation(asyncContinuation));
            target.WriteAsyncLogEvent(new LogEventInfo(LogLevel.Info, "logger2", "msg3").WithContinuation(asyncContinuation));

            mre.WaitOne();
            foreach (var ex in exceptions)
            {
                if (ex != null)
                {
                    Assert.True(false, ex.ToString());
                }
            }

            target.Close();

            string result = senderFactory.Log.ToString();
            Assert.True(result.IndexOf("1: connect tcp://logger1.company.lan/") != -1);
            Assert.True(result.IndexOf("1: send 0 4") != -1);
            Assert.True(result.IndexOf("1: close") != -1);
            Assert.True(result.IndexOf("2: connect tcp://logger2.company.lan/") != -1);
            Assert.True(result.IndexOf("2: send 0 4") != -1);
            Assert.True(result.IndexOf("2: close") != -1);
            Assert.True(result.IndexOf("3: connect tcp://logger1.company.lan/") != -1);
            Assert.True(result.IndexOf("3: send 0 4") != -1);
            Assert.True(result.IndexOf("3: close") != -1);
            Assert.True(result.IndexOf("4: connect tcp://logger3.company.lan/") != -1);
            Assert.True(result.IndexOf("4: send 0 4") != -1);
            Assert.True(result.IndexOf("4: close") != -1);
            Assert.True(result.IndexOf("5: connect tcp://logger1.company.lan/") != -1);
            Assert.True(result.IndexOf("5: send 0 4") != -1);
            Assert.True(result.IndexOf("5: close") != -1);
            Assert.True(result.IndexOf("6: connect tcp://logger2.company.lan/") != -1);
            Assert.True(result.IndexOf("6: send 0 4") != -1);
            Assert.True(result.IndexOf("6: close") != -1);
        }

        [Fact]
        public void NetworkTargetMultipleConnectionsWithMessageSplitTest()
        {
            var senderFactory = new MySenderFactory();
            var target = new NetworkTarget();
            target.Address = "tcp://${logger}.company.lan/";
            target.SenderFactory = senderFactory;
            target.Layout = "${message}";
            target.KeepConnection = true;
            target.MaxMessageSize = 9;
            target.OnOverflow = NetworkTargetOverflowAction.Split;
            target.Initialize(null);

            var exceptions = new List<Exception>();
            var mre = new ManualResetEvent(false);
            int remaining = 3;
            AsyncContinuation asyncContinuation = ex =>
            {
                lock (exceptions)
                {
                    exceptions.Add(ex);
                    if (--remaining == 0)
                    {
                        mre.Set();
                    }
                }
            };

            target.WriteAsyncLogEvent(new LogEventInfo(LogLevel.Info, "logger1", "012345678901234567890123456789").WithContinuation(asyncContinuation));
            target.WriteAsyncLogEvent(new LogEventInfo(LogLevel.Info, "logger1", "012345678901234").WithContinuation(asyncContinuation));
            target.WriteAsyncLogEvent(new LogEventInfo(LogLevel.Info, "logger2", "012345678901234567890123").WithContinuation(asyncContinuation));
            
            mre.WaitOne();
            foreach (var ex in exceptions)
            {
                if (ex != null)
                {
                    Assert.True(false, ex.ToString());
                }
            }

            target.Close();

            var result = senderFactory.Log.ToString();
            Assert.True(result.IndexOf("1: connect tcp://logger1.company.lan/") != -1);
            Assert.True(result.IndexOf("1: send 0 9") != -1);
            Assert.True(result.IndexOf("1: send 9 9") != -1);
            Assert.True(result.IndexOf("1: send 18 9") != -1);
            Assert.True(result.IndexOf("1: send 27 3") != -1);
            Assert.True(result.IndexOf("1: send 0 9") != -1);
            Assert.True(result.IndexOf("1: send 9 6") != -1);
            Assert.True(result.IndexOf("2: connect tcp://logger2.company.lan/") != -1);
            Assert.True(result.IndexOf("2: send 0 9") != -1);
            Assert.True(result.IndexOf("2: send 9 9") != -1);
            Assert.True(result.IndexOf("2: send 18 6") != -1);
            Assert.True(result.IndexOf("1: close") != -1);
            Assert.True(result.IndexOf("2: close") != -1);
        }

        [Fact]
        public void NetworkTargetMultipleConnectionsWithMessageDiscardTest()
        {
            var senderFactory = new MySenderFactory();
            var target = new NetworkTarget();
            target.Address = "tcp://${logger}.company.lan/";
            target.SenderFactory = senderFactory;
            target.Layout = "${message}";
            target.KeepConnection = true;
            target.MaxMessageSize = 10;
            target.OnOverflow = NetworkTargetOverflowAction.Discard;
            target.Initialize(null);

            var exceptions = new List<Exception>();
            var mre = new ManualResetEvent(false);
            int remaining = 3;
            AsyncContinuation asyncContinuation = ex =>
            {
                lock (exceptions)
                {
                    exceptions.Add(ex);
                    if (--remaining == 0)
                    {
                        mre.Set();
                    }
                }
            };

            target.WriteAsyncLogEvent(new LogEventInfo(LogLevel.Info, "logger1", "0123456").WithContinuation(asyncContinuation));
            target.WriteAsyncLogEvent(new LogEventInfo(LogLevel.Info, "logger1", "012345678901234").WithContinuation(asyncContinuation));
            target.WriteAsyncLogEvent(new LogEventInfo(LogLevel.Info, "logger2", "01234").WithContinuation(asyncContinuation));

            mre.WaitOne();
            foreach (var ex in exceptions)
            {
                if (ex != null)
                {
                    Assert.True(false, ex.ToString());
                }
            }

            target.Close();

            string result = senderFactory.Log.ToString();
            Assert.True(result.IndexOf("1: connect tcp://logger1.company.lan/") != -1);
            Assert.True(result.IndexOf("1: send 0 7") != -1);
            Assert.True(result.IndexOf("2: connect tcp://logger2.company.lan/") != -1);
            Assert.True(result.IndexOf("2: send 0 5") != -1);
            Assert.True(result.IndexOf("1: close") != -1);
            Assert.True(result.IndexOf("2: close") != -1);
        }

        [Fact]
        public void NetworkTargetMultipleConnectionsWithMessageErrorTest()
        {
            var senderFactory = new MySenderFactory();
            var target = new NetworkTarget();
            target.Address = "tcp://${logger}.company.lan/";
            target.SenderFactory = senderFactory;
            target.Layout = "${message}";
            target.KeepConnection = true;
            target.MaxMessageSize = 10;
            target.OnOverflow = NetworkTargetOverflowAction.Error;
            target.Initialize(null);

            var exceptions = new List<Exception>();
            var mre = new ManualResetEvent(false);
            int remaining = 3;
            AsyncContinuation asyncContinuation = ex =>
            {
                lock (exceptions)
                {
                    exceptions.Add(ex);
                    if (--remaining == 0)
                    {
                        mre.Set();
                    }
                }
            };

            target.WriteAsyncLogEvent(new LogEventInfo(LogLevel.Info, "logger1", "0123456").WithContinuation(asyncContinuation));
            target.WriteAsyncLogEvent(new LogEventInfo(LogLevel.Info, "logger1", "012345678901234").WithContinuation(asyncContinuation));
            target.WriteAsyncLogEvent(new LogEventInfo(LogLevel.Info, "logger2", "01234").WithContinuation(asyncContinuation));

            mre.WaitOne();
            Assert.Null(exceptions[0]);
            Assert.NotNull(exceptions[1]);
            Assert.Equal("Attempted to send a message larger than MaxMessageSize (10). Actual size was: 15. Adjust OnOverflow and MaxMessageSize parameters accordingly.", exceptions[1].Message);
            Assert.Null(exceptions[2]);

            target.Close();

            string result = senderFactory.Log.ToString();
            Assert.True(result.IndexOf("1: connect tcp://logger1.company.lan/") != -1);
            Assert.True(result.IndexOf("1: send 0 7") != -1);
            Assert.True(result.IndexOf("1: close") != -1);
            Assert.True(result.IndexOf("2: connect tcp://logger2.company.lan/") != -1);
            Assert.True(result.IndexOf("2: send 0 5") != -1);
            Assert.True(result.IndexOf("2: close") != -1);
        }

        [Fact]
        public void NetworkTargetSendFailureTests()
        {
            var senderFactory = new MySenderFactory()
            {
                FailCounter = 3, // first 3 sends will fail
            };

            var target = new NetworkTarget();
            target.Address = "tcp://${logger}.company.lan/";
            target.SenderFactory = senderFactory;
            target.Layout = "${message}";
            target.KeepConnection = true;
            target.OnOverflow = NetworkTargetOverflowAction.Discard;
            target.Initialize(null);

            var exceptions = new List<Exception>();
            var mre = new ManualResetEvent(false);
            int remaining = 5;
            AsyncContinuation asyncContinuation = ex =>
            {
                lock (exceptions)
                {
                    exceptions.Add(ex);
                    if (--remaining == 0)
                    {
                        mre.Set();
                    }
                }
            };

            target.WriteAsyncLogEvent(new LogEventInfo(LogLevel.Info, "logger1", "0123456").WithContinuation(asyncContinuation));
            target.WriteAsyncLogEvent(new LogEventInfo(LogLevel.Info, "logger1", "0123456").WithContinuation(asyncContinuation));
            target.WriteAsyncLogEvent(new LogEventInfo(LogLevel.Info, "logger1", "0123456").WithContinuation(asyncContinuation));
            target.WriteAsyncLogEvent(new LogEventInfo(LogLevel.Info, "logger1", "0123456").WithContinuation(asyncContinuation));
            target.WriteAsyncLogEvent(new LogEventInfo(LogLevel.Info, "logger1", "01234").WithContinuation(asyncContinuation));

            mre.WaitOne();
            Assert.NotNull(exceptions[0]);
            Assert.NotNull(exceptions[1]);
            Assert.NotNull(exceptions[2]);
            Assert.Null(exceptions[3]);
            Assert.Null(exceptions[4]);

            target.Close();

            var result = senderFactory.Log.ToString();
            Assert.True(result.IndexOf("1: connect tcp://logger1.company.lan/") != -1);
            Assert.True(result.IndexOf("1: send 0 7") != -1);
            Assert.True(result.IndexOf("1: failed") != -1);
            Assert.True(result.IndexOf("1: close") != -1);
            Assert.True(result.IndexOf("2: connect tcp://logger1.company.lan/") != -1);
            Assert.True(result.IndexOf("2: send 0 7") != -1);
            Assert.True(result.IndexOf("2: failed") != -1);
            Assert.True(result.IndexOf("2: close") != -1);
            Assert.True(result.IndexOf("3: connect tcp://logger1.company.lan/") != -1);
            Assert.True(result.IndexOf("3: send 0 7") != -1);
            Assert.True(result.IndexOf("3: failed") != -1);
            Assert.True(result.IndexOf("3: close") != -1);
            Assert.True(result.IndexOf("4: connect tcp://logger1.company.lan/") != -1);
            Assert.True(result.IndexOf("4: send 0 7") != -1);
            Assert.True(result.IndexOf("4: send 0 5") != -1);
            Assert.True(result.IndexOf("4: close") != -1);
        }

        [Fact]
        public void NetworkTargetTcpTest()
        {
            NetworkTarget target;

            target = new NetworkTarget()
            {
                Address = "tcp://127.0.0.1:3004",
                Layout = "${message}\n",
                KeepConnection = true,
            };

            string expectedResult = string.Empty;

            using (var listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                Exception receiveException = null;
                var resultStream = new MemoryStream();
                var receiveFinished = new ManualResetEvent(false);

                listener.Bind(new IPEndPoint(IPAddress.Loopback, 3004));
                listener.Listen(10);
                listener.BeginAccept(
                    result =>
                    {
                        try
                        {
                            byte[] buffer = new byte[4096];
                            using (Socket connectedSocket = listener.EndAccept(result))
                            {
                                int got;
                                while ((got = connectedSocket.Receive(buffer, 0, buffer.Length, SocketFlags.None)) > 0)
                                {
                                    resultStream.Write(buffer, 0, got);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Receive exception {0}", ex);
                            receiveException = ex;
                        }
                        finally
                        {
                            receiveFinished.Set();
                        }
                    }, null);

                target.Initialize(new LoggingConfiguration());

                int pendingWrites = 100;
                var writeCompleted = new ManualResetEvent(false);
                var exceptions = new List<Exception>();

                AsyncContinuation writeFinished =
                    ex =>
                    {
                        lock (exceptions)
                        {
                            // Console.WriteLine("{0} Write finished {1}", pendingWrites, ex);
                            exceptions.Add(ex);
                            pendingWrites--;
                            if (pendingWrites == 0)
                            {
                                writeCompleted.Set();
                            }
                        }
                    };

                int toWrite = pendingWrites;
                for (int i = 0; i < toWrite; ++i)
                {
                    var ev = new LogEventInfo(LogLevel.Info, "logger1", "messagemessagemessagemessagemessage" + i).WithContinuation(writeFinished);
                    target.WriteAsyncLogEvent(ev);
                    expectedResult += "messagemessagemessagemessagemessage" + i + "\n";
                }

                Assert.True(writeCompleted.WaitOne(10000, false), "Writes did not complete");
                target.Close();
                Assert.True(receiveFinished.WaitOne(10000, false), "Receive did not complete");
                string resultString = Encoding.UTF8.GetString(resultStream.GetBuffer(), 0, (int)resultStream.Length);
                Assert.Null(receiveException);
                Assert.Equal(expectedResult, resultString);
            }
        }

        [Fact]
        public void NetworkTargetUdpTest()
        {
            var target = new NetworkTarget()
            {
                Address = "udp://127.0.0.1:3002",
                Layout = "${message}\n",
                KeepConnection = true,
            };

            string expectedResult = string.Empty;

            using (var listener = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
            {
                Exception receiveException = null;
                var receivedMessages = new List<string>();
                var receiveFinished = new ManualResetEvent(false);

                byte[] receiveBuffer = new byte[4096];

                listener.Bind(new IPEndPoint(IPAddress.Loopback, 3002));
                EndPoint remoteEndPoint = null;
                AsyncCallback receivedDatagram = null;

                receivedDatagram = result =>
                    {
                        try
                        {
                            int got = listener.EndReceiveFrom(result, ref remoteEndPoint);
                            string message = Encoding.UTF8.GetString(receiveBuffer, 0, got);
                            lock (receivedMessages)
                            {
                                receivedMessages.Add(message);
                                if (receivedMessages.Count == 100)
                                {
                                    receiveFinished.Set();
                                }
                            }

                            remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
                            listener.BeginReceiveFrom(receiveBuffer, 0, receiveBuffer.Length, SocketFlags.None, ref remoteEndPoint, receivedDatagram, null);
                        }
                        catch (Exception ex)
                        {
                            receiveException = ex;
                        }
                    };

                remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
                listener.BeginReceiveFrom(receiveBuffer, 0, receiveBuffer.Length, SocketFlags.None, ref remoteEndPoint, receivedDatagram, null);

                target.Initialize(new LoggingConfiguration());

                int pendingWrites = 100;
                var writeCompleted = new ManualResetEvent(false);
                var exceptions = new List<Exception>();

                AsyncContinuation writeFinished =
                    ex =>
                    {
                        lock (exceptions)
                        {
                            exceptions.Add(ex);
                            pendingWrites--;
                            if (pendingWrites == 0)
                            {
                                writeCompleted.Set();
                            }
                        }
                    };

                int toWrite = pendingWrites;
                for (int i = 0; i < toWrite; ++i)
                {
                    var ev = new LogEventInfo(LogLevel.Info, "logger1", "message" + i).WithContinuation(writeFinished);
                    target.WriteAsyncLogEvent(ev);
                    expectedResult += "message" + i + "\n";
                }

                Assert.True(writeCompleted.WaitOne(10000, false));
                target.Close();
                Assert.True(receiveFinished.WaitOne(10000, false));
                Assert.Equal(toWrite, receivedMessages.Count);
                for (int i = 0; i < toWrite; ++i)
                {
                    Assert.True(receivedMessages.Contains("message" + i + "\n"), "Message #" + i + " not received.");
                }

                Assert.Null(receiveException);
            }
        }

        [Fact]
        public void NetworkTargetNotConnectedTest()
        {
            var target = new NetworkTarget()
            {
                Address = "tcp4://127.0.0.1:33415",
                Layout = "${message}\n",
                KeepConnection = true,
            };

            target.Initialize(new LoggingConfiguration());

            int toWrite = 10;
            int pendingWrites = toWrite;
            var writeCompleted = new ManualResetEvent(false);
            var exceptions = new List<Exception>();

            AsyncContinuation writeFinished =
                ex =>
                {
                    lock (exceptions)
                    {
                        exceptions.Add(ex);
                        pendingWrites--;
                        if (pendingWrites == 0)
                        {
                            writeCompleted.Set();
                        }
                    }
                };

            for (int i = 0; i < toWrite; ++i)
            {
                var ev = new LogEventInfo(LogLevel.Info, "logger1", "message" + i).WithContinuation(writeFinished);
                target.WriteAsyncLogEvent(ev);
            }

            writeCompleted.WaitOne();


            // no exception
            target.Close();

            Assert.Equal(toWrite, exceptions.Count);
            foreach (var ex in exceptions)
            {
                Assert.NotNull(ex);
            }
        }

        [Fact]
        public void NetworkTargetSendFailureWithoutKeepAliveTests()
        {
            var senderFactory = new MySenderFactory()
            {
                FailCounter = 3, // first 3 sends will fail
            };

            var target = new NetworkTarget();
            target.Address = "tcp://${logger}.company.lan/";
            target.SenderFactory = senderFactory;
            target.Layout = "${message}";
            target.KeepConnection = false;
            target.OnOverflow = NetworkTargetOverflowAction.Discard;
            target.Initialize(null);

            var exceptions = new List<Exception>();
            var mre = new ManualResetEvent(false);
            int remaining = 5;
            AsyncContinuation asyncContinuation = ex =>
            {
                lock (exceptions)
                {
                    exceptions.Add(ex);
                    if (--remaining == 0)
                    {
                        mre.Set();
                    }
                }
            };

            target.WriteAsyncLogEvent(new LogEventInfo(LogLevel.Info, "logger1", "0123456").WithContinuation(asyncContinuation));
            target.WriteAsyncLogEvent(new LogEventInfo(LogLevel.Info, "logger1", "0123456").WithContinuation(asyncContinuation));
            target.WriteAsyncLogEvent(new LogEventInfo(LogLevel.Info, "logger1", "0123456").WithContinuation(asyncContinuation));
            target.WriteAsyncLogEvent(new LogEventInfo(LogLevel.Info, "logger1", "0123456").WithContinuation(asyncContinuation));
            target.WriteAsyncLogEvent(new LogEventInfo(LogLevel.Info, "logger1", "01234").WithContinuation(asyncContinuation));

            mre.WaitOne();
            Assert.NotNull(exceptions[0]);
            Assert.NotNull(exceptions[1]);
            Assert.NotNull(exceptions[2]);
            Assert.Null(exceptions[3]);
            Assert.Null(exceptions[4]);

            target.Close();

            var result = senderFactory.Log.ToString();
            Assert.True(result.IndexOf("1: connect tcp://logger1.company.lan/") != -1);
            Assert.True(result.IndexOf("1: send 0 7") != -1);
            Assert.True(result.IndexOf("1: failed") != -1);
            Assert.True(result.IndexOf("1: close") != -1);
            Assert.True(result.IndexOf("2: connect tcp://logger1.company.lan/") != -1);
            Assert.True(result.IndexOf("2: send 0 7") != -1);
            Assert.True(result.IndexOf("2: failed") != -1);
            Assert.True(result.IndexOf("2: close") != -1);
            Assert.True(result.IndexOf("3: connect tcp://logger1.company.lan/") != -1);
            Assert.True(result.IndexOf("3: send 0 7") != -1);
            Assert.True(result.IndexOf("3: failed") != -1);
            Assert.True(result.IndexOf("3: close") != -1);
            Assert.True(result.IndexOf("4: connect tcp://logger1.company.lan/") != -1);
            Assert.True(result.IndexOf("4: send 0 7") != -1);
            Assert.True(result.IndexOf("4: close") != -1);
            Assert.True(result.IndexOf("5: connect tcp://logger1.company.lan/") != -1);
            Assert.True(result.IndexOf("5: send 0 5") != -1);
            Assert.True(result.IndexOf("5: close") != -1);
        }

        [Theory]
        [InlineData("none", SslProtocols.None)] //we can't set it on ""
        [InlineData("tls", SslProtocols.Tls)]
        [InlineData("tls11", SslProtocols.Tls11)]
        [InlineData("tls,tls11", SslProtocols.Tls11 | SslProtocols.Tls)]
        public void SslProtocolsConfigTest(string sslOptions, SslProtocols expected)
        {
            var config = XmlLoggingConfiguration.CreateFromXmlString($@"
            <nlog>
                <targets><target name='target1' type='network' layout='${{message}}' Address='tcp://127.0.0.1:50001' sslProtocols='{sslOptions}' /></targets>
               
            </nlog>");

            var target = config.FindTargetByName<NetworkTarget>("target1");
            Assert.Equal(expected, target.SslProtocols);
        }

        [Theory]
        [InlineData("0", 0)]
        [InlineData("30", 30)]
        public void KeepAliveTimeConfigTest(string keepAliveTimeSeconds, int expected)
        {
            if (IsLinux())
            {
                Console.WriteLine("[SKIP] NetworkTargetTests.KeepAliveTimeConfigTest because we are running in Travis");
                return;
            }

            var config = XmlLoggingConfiguration.CreateFromXmlString($@"
            <nlog>
                <targets async='true'><target name='target1' type='network' layout='${{level}}|${{threadid}}|${{message}}' Address='tcp://127.0.0.1:50001' keepAliveTimeSeconds='{keepAliveTimeSeconds}' /></targets>
                <rules><logger name='*' minLevel='Trace' writeTo='target1'/></rules>
            </nlog>");

            var target = config.FindTargetByName<NLog.Targets.Wrappers.AsyncTargetWrapper>("target1").WrappedTarget as NetworkTarget;
            Assert.Equal(expected, target.KeepAliveTimeSeconds);

            LogManager.Configuration = config;
            var logger = LogManager.GetLogger("keepAliveTimeSeconds");
            logger.Info("Hello");
        }

        [Fact]
        public void Bug3990StackOverflowWhenUsingNLogViewerTarget()
        {
            // this would fail because of stack overflow in the 
            // constructor of NLogViewerTarget
            var config = XmlLoggingConfiguration.CreateFromXmlString(@"
<nlog>
  <targets>
    <target name='viewer' type='NLogViewer' address='udp://127.0.0.1:9999' />
  </targets>
  <rules>
    <logger name='*' minlevel='Debug' writeTo='viewer' />
  </rules>
</nlog>");

            var target = config.LoggingRules[0].Targets[0] as NLogViewerTarget;
            Assert.NotNull(target);
        }

        internal class MySenderFactory : INetworkSenderFactory
        {
            internal List<MyNetworkSender> Senders = new List<MyNetworkSender>();
            internal StringWriter Log = new StringWriter();
            private int idCounter;

            public NetworkSender Create(string url, int maxQueueSize, SslProtocols sslProtocols, TimeSpan keepAliveTime)
            {
                var sender = new MyNetworkSender(url, ++idCounter, Log, this);
                Senders.Add(sender);
                return sender;
            }

            public int FailCounter { get; set; }
        }

        internal class MyNetworkSender : NetworkSender
        {
            private readonly int id;
            private readonly TextWriter log;
            private readonly MySenderFactory senderFactory;
            internal MemoryStream MemoryStream { get; set; }

            public MyNetworkSender(string url, int id, TextWriter log, MySenderFactory senderFactory)
                : base(url)
            {
                this.id = id;
                this.log = log;
                this.senderFactory = senderFactory;
                MemoryStream = new MemoryStream();
            }
            protected override void DoInitialize()
            {
                base.DoInitialize();
                log.WriteLine("{0}: connect {1}", id, Address);
            }

            protected override void DoFlush(AsyncContinuation continuation)
            {
                log.WriteLine("{0}: flush", id);
                continuation(null);
            }

            protected override void DoClose(AsyncContinuation continuation)
            {
                log.WriteLine("{0}: close", id);
                continuation(null);
            }

            protected override void DoSend(byte[] bytes, int offset, int length, AsyncContinuation asyncContinuation)
            {
                log.WriteLine("{0}: send {1} {2}", id, offset, length);
                MemoryStream.Write(bytes, offset, length);
                if (senderFactory.FailCounter > 0)
                {
                    log.WriteLine("{0}: failed", id);
                    senderFactory.FailCounter--;
                    asyncContinuation(new IOException("some IO error has occured"));
                }
                else
                {
                    asyncContinuation(null);
                }
            }
        }
    }
}
