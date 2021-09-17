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
            HappyPathTest(null, "msg1", "msg2", "msg3");
        }

        [Fact]
        public void HappyPathCRLFTest()
        {
            HappyPathTest(LineEndingMode.CRLF, "msg1", "msg2", "msg3");
        }

        [Fact]
        public void HappyPathLFTest()
        {
            HappyPathTest(LineEndingMode.LF, "msg1", "msg2", "msg3");
        }

        private void HappyPathTest(LineEndingMode lineEnding, params string[] messages)
        {
            var senderFactory = new MySenderFactory();
            var target = new NetworkTarget();
            target.Address = "tcp://someaddress/";
            target.SenderFactory = senderFactory;
            target.Layout = "${message}";
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

            Assert.True(mre.WaitOne(10000), "Network Write not completed");
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
            var eol = lineEnding?.NewLineCharacters ?? string.Empty;
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
            Assert.Equal(100, target.MaxConnections);
            Assert.Equal(10000, target.MaxQueueSize);
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

            Assert.True(mre.WaitOne(10000), "Network Write not completed");

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
            Assert.True(mre.WaitOne(10000), "Network Flush not completed");
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
            Assert.True(mre.WaitOne(10000), "Network Flush not completed");
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

            Assert.True(mre.WaitOne(10000), "Network Write not completed");
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

            Assert.True(mre.WaitOne(10000), "Network Write not completed");
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

            Assert.True(mre.WaitOne(10000), "Network Write not completed");
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

            Assert.True(mre.WaitOne(10000), "Network Write not completed");
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

            Assert.True(mre.WaitOne(10000), "Network Write not completed");
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

                Assert.True(writeCompleted.WaitOne(10000), "Network Writes did not complete");
                target.Close();
                foreach (var ex in exceptions)
                {
                    Assert.True(ex == null, $"Network Write Exception: {ex?.ToString()}");
                }
                Assert.True(receiveFinished.WaitOne(10000), "Network Receive did not complete");
                Assert.True(receiveException == null, $"Network Receive Exception: {receiveException?.ToString()}");
                string resultString = Encoding.UTF8.GetString(resultStream.GetBuffer(), 0, (int)resultStream.Length);
                Assert.Equal(expectedResult, resultString);
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void NetworkTargetUdpTest(bool splitMessage)
        {
            var target = new NetworkTarget()
            {
                Address = "udp://127.0.0.1:3002",
                Layout = "${message}\n",
                KeepConnection = true,
                MaxMessageSize = splitMessage ? 4 : short.MaxValue,
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

                const int toWrite = 50;
                int pendingWrites = toWrite;

                receivedDatagram = result =>
                    {
                        try
                        {
                            int got = listener.EndReceiveFrom(result, ref remoteEndPoint);
                            string message = Encoding.UTF8.GetString(receiveBuffer, 0, got);
                            lock (receivedMessages)
                            {
                                if (splitMessage)
                                {
                                    if (receivedMessages.Count > 0 && !receivedMessages[receivedMessages.Count - 1].Contains('\n'))
                                    {
                                        receivedMessages[receivedMessages.Count - 1] = receivedMessages[receivedMessages.Count - 1] + message;
                                    }
                                    else
                                    {
                                        receivedMessages.Add(message);
                                    }
                                }
                                else
                                {
                                    receivedMessages.Add(message);
                                }

                                if (receivedMessages.Count == toWrite && receivedMessages[receivedMessages.Count - 1].Contains('\n'))
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
                            receiveFinished.Set();
                        }
                    };

                remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
                listener.BeginReceiveFrom(receiveBuffer, 0, receiveBuffer.Length, SocketFlags.None, ref remoteEndPoint, receivedDatagram, null);

                target.Initialize(new LoggingConfiguration());

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
                    expectedResult += "message" + i + "\n";
                }

                Assert.True(writeCompleted.WaitOne(10000), "Network Write not completed");
                target.Close();
                foreach (var ex in exceptions)
                {
                    Assert.True(ex == null, $"Network Write Exception: {ex?.ToString()}");
                }
                Assert.True(receiveFinished.WaitOne(10000), $"Network Receive not completed. Count={receivedMessages.Count}, LastMsg={receivedMessages.LastOrDefault()}");
                Assert.True(receiveException == null, $"Network Receive Exception: {receiveException?.ToString()}");
                Assert.Equal(toWrite, receivedMessages.Count);
                for (int i = 0; i < toWrite; ++i)
                {
                    Assert.Equal(receivedMessages[i], $"message{i}\n");
                }
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

            var loggerTask = new NLog.Internal.AsyncHelpersTask(state =>
            {
                for (int i = 0; i < toWrite; ++i)
                {
                    var ev = new LogEventInfo(LogLevel.Info, "logger1", "message" + i).WithContinuation(writeFinished);
                    target.WriteAsyncLogEvent(ev);
                }
            });
            AsyncHelpers.StartAsyncTask(loggerTask, null);
            Assert.True(writeCompleted.WaitOne(10000), "Network Write not completed");

            var shutdownCompleted = new ManualResetEvent(false);
            var closeTask = new NLog.Internal.AsyncHelpersTask(state =>
            {
                // no exception
                target.Close();
                shutdownCompleted.Set();
            });
            AsyncHelpers.StartAsyncTask(closeTask, null);
            Assert.True(shutdownCompleted.WaitOne(10000), "Network Close not completed");

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

            Assert.True(mre.WaitOne(10000), "Network Write not completed");
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

        [Fact]
        public void AsyncConnectionFailureOnCloseShouldNotDeadlock()
        {
            const int total = 100;
            var senderFactory = new MyQueudSenderFactory()
            {
                FailCounter = total,
                AsyncMode = true,
            };

            var target = new NetworkTarget();
            target.Address = "tcp://${logger}.company.lan/";
            target.SenderFactory = senderFactory;
            target.Layout = "${message}";
            target.KeepConnection = true;
            target.Initialize(null);

            var exceptions = new List<Exception>();
            var writeCompleted = new ManualResetEvent(false);
            var shutdownCompleted = new ManualResetEvent(false);
            int remaining = total;
            AsyncContinuation asyncContinuation = ex =>
            {
                lock (exceptions)
                {
                    exceptions.Add(ex);
                    if (--remaining == 0)
                    {
                        writeCompleted.Set();
                    }
                }
            };

            var shutdownTask = new NLog.Internal.AsyncHelpersTask(state => {
                Thread.Sleep(10);
                target.Flush(ex => { });
                target.Close();
                shutdownCompleted.Set();
            });
            AsyncHelpers.StartAsyncTask(shutdownTask, null);

            var loggerTask = new NLog.Internal.AsyncHelpersTask(state =>
            {
                for (int i = 0; i < total; ++i)
                {
                    target.WriteAsyncLogEvent(new LogEventInfo(LogLevel.Info, $"logger{i}", i.ToString()).WithContinuation(asyncContinuation));
                    Thread.Sleep(1);
                }

                writeCompleted.Set();
            });
            AsyncHelpers.StartAsyncTask(loggerTask, null);

            Assert.True(writeCompleted.WaitOne(10000), "Network Write not completed");
            Assert.True(shutdownCompleted.WaitOne(10000), "Network Shutdown not completed");
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

            public NetworkSender Create(string url, int maxQueueSize, NetworkTargetQueueOverflowAction onQueueOverflow, int maxMessageSize, SslProtocols sslProtocols, TimeSpan keepAliveTime)
            {
                var sender = new MyNetworkSender(url, ++idCounter, Log, this);
                Senders.Add(sender);
                return sender;
            }

            public int FailCounter { get; set; }

            public bool AsyncMode { get; set; }
        }

        internal class MyNetworkSender : NetworkSender
        {
            private readonly int id;
            private readonly TextWriter log;
            private readonly MySenderFactory senderFactory;
            internal MemoryStream MemoryStream { get; }

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
                lock (log)
                {
                    log.WriteLine("{0}: connect {1}", id, Address);
                }
            }

            protected override void DoFlush(AsyncContinuation continuation)
            {
                if (senderFactory.AsyncMode)
                {
                    var asyncTask = new NLog.Internal.AsyncHelpersTask(state => { Thread.Sleep(1); FlushSync(continuation); });
                    AsyncHelpers.StartAsyncTask(asyncTask, null);
                }
                else
                {
                    FlushSync(continuation);
                }
            }

            private void FlushSync(AsyncContinuation continuation)
            {
                lock (log)
                {
                    log.WriteLine("{0}: flush", id);
                }
                continuation(null);
            }

            protected override void DoClose(AsyncContinuation continuation)
            {
                if (senderFactory.AsyncMode)
                {
                    var asyncTask = new NLog.Internal.AsyncHelpersTask(state => { Thread.Sleep(1); CloseSync(continuation); });
                    AsyncHelpers.StartAsyncTask(asyncTask, null);
                }
                else
                {
                    CloseSync(continuation);
                }
            }

            private void CloseSync(AsyncContinuation continuation)
            {
                lock (log)
                {
                    log.WriteLine("{0}: close", id);
                }
                continuation(null);
            }

            protected override void DoSend(byte[] bytes, int offset, int length, AsyncContinuation continuation)
            {
                if (senderFactory.AsyncMode)
                {
                    var asyncTask = new NLog.Internal.AsyncHelpersTask(state => { Thread.Sleep(1); SendSync(bytes, offset, length, continuation); });
                    AsyncHelpers.StartAsyncTask(asyncTask, null);
                }
                else
                {
                    SendSync(bytes, offset, length, continuation);
                }
            }

            private void SendSync(byte[] bytes, int offset, int length, AsyncContinuation continuation)
            {
                var failedException = CheckForFailedException();
                lock (log)
                {
                    log.WriteLine("{0}: send {1} {2}", id, offset, length);
                    MemoryStream.Write(bytes, offset, length);
                    if (failedException != null)
                    {
                        log.WriteLine("{0}: failed", id);
                    }
                }
                continuation(failedException);
            }

            private Exception CheckForFailedException()
            {
                lock (senderFactory)
                {
                    if (senderFactory.FailCounter > 0)
                    {
                        senderFactory.FailCounter--;
                        return new IOException("some IO error has occured");
                    }
                }

                return null;
            }
        }

        internal class MyQueudSenderFactory : INetworkSenderFactory
        {
            internal List<MyQueudNetworkSender> Senders = new List<MyQueudNetworkSender>();
            internal StringWriter Log = new StringWriter();
            private int idCounter;

            public NetworkSender Create(string url, int maxQueueSize, NetworkTargetQueueOverflowAction onQueueOverflow, int maxMessageSize, SslProtocols sslProtocols, TimeSpan keepAliveTime)
            {
                var sender = new MyQueudNetworkSender(url, ++idCounter, Log, this);
                Senders.Add(sender);
                return sender;
            }

            public int FailCounter { get; set; }

            public bool AsyncMode { get; set; }
        }

        internal class MyQueudNetworkSender : QueuedNetworkSender
        {
            private readonly int id;
            private readonly TextWriter log;
            private readonly MyQueudSenderFactory senderFactory;
            internal MemoryStream MemoryStream { get; }

            public MyQueudNetworkSender(string url, int id, TextWriter log, MyQueudSenderFactory senderFactory)
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
                lock (log)
                {
                    log.WriteLine("{0}: connect {1}", id, Address);
                }
            }

            protected override void DoFlush(AsyncContinuation continuation)
            {
                lock (log)
                {
                    log.WriteLine("{0}: flush", id);
                }

                base.DoFlush(continuation);
            }

            protected override void DoClose(AsyncContinuation continuation)
            {
                lock (log)
                {
                    log.WriteLine("{0}: close", id);
                }
                base.DoClose(continuation);
            }

            protected override void BeginRequest(NetworkRequestArgs eventArgs)
            {
                if (senderFactory.AsyncMode)
                {
                    var asyncTask = new NLog.Internal.AsyncHelpersTask(state => { Thread.Sleep(1); SendSync(eventArgs); });
                    AsyncHelpers.StartAsyncTask(asyncTask, null);
                }
                else
                {
                    SendSync(eventArgs);
                }
            }

            private void SendSync(NetworkRequestArgs eventArgs)
            {
                var failedException = CheckForFailedException();

                lock (log)
                {
                    log.WriteLine("{0}: send {1} {2}", id, eventArgs.RequestBufferOffset, eventArgs.RequestBufferLength);
                    MemoryStream.Write(eventArgs.RequestBuffer, eventArgs.RequestBufferOffset, eventArgs.RequestBufferLength);
                    if (failedException != null)
                    {
                        log.WriteLine("{0}: failed", id);
                    }
                }
                
                var nextRequest = EndRequest(eventArgs.AsyncContinuation, failedException);
                if (nextRequest.HasValue)
                {
                    BeginRequest(nextRequest.Value);
                }
            }

            private Exception CheckForFailedException()
            {
                lock (senderFactory)
                {
                    if (senderFactory.FailCounter > 0)
                    {
                        senderFactory.FailCounter--;
                        return new IOException("some IO error has occured");
                    }
                }

                return null;
            }
        }
    }
}
