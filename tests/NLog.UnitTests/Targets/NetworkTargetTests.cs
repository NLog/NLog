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

        private static void HappyPathTest(LineEndingMode lineEnding, params string[] messages)
        {
            var senderFactory = new MyQueudSenderFactory();
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
                Assert.True(ex == null, $"Network Write Exception: {ex?.ToString()}");
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
            var senderFactory = new MyQueudSenderFactory();
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
                Assert.True(ex == null, $"Network Write Exception: {ex?.ToString()}");
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
            var senderFactory = new MyQueudSenderFactory();
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
            var senderFactory = new MyQueudSenderFactory();
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
                Assert.True(ex == null, $"Network Write Exception: {ex?.ToString()}");
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
            var senderFactory = new MyQueudSenderFactory();
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
                Assert.True(ex == null, $"Network Write Exception: {ex?.ToString()}");
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
            var senderFactory = new MyQueudSenderFactory();
            var target = new NetworkTarget();
            target.Address = "tcp://${logger}.company.lan/";
            target.SenderFactory = senderFactory;
            target.Layout = "${message}";
            target.KeepConnection = true;
            target.MaxMessageSize = 10;
            target.OnOverflow = NetworkTargetOverflowAction.Discard;
            target.Initialize(null);

            int droppedLogs = 0;
            target.LogEventDropped += (sender, args) => droppedLogs++;

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
                Assert.True(ex == null, $"Network Write Exception: {ex?.ToString()}");
            }

            target.Close();

            string result = senderFactory.Log.ToString();
            Assert.True(result.IndexOf("1: connect tcp://logger1.company.lan/") != -1);
            Assert.True(result.IndexOf("1: send 0 7") != -1);
            Assert.True(result.IndexOf("2: connect tcp://logger2.company.lan/") != -1);
            Assert.True(result.IndexOf("2: send 0 5") != -1);
            Assert.True(result.IndexOf("1: close") != -1);
            Assert.True(result.IndexOf("2: close") != -1);
            Assert.Equal(1, droppedLogs);
        }

        [Fact]
        public void NetworkTargetMultipleConnectionsWithMessageErrorTest()
        {
            var senderFactory = new MyQueudSenderFactory()
            {
                AsyncMode = false,  // Disable async, because unit-test cannot handle it
            };
            var target = new NetworkTarget();
            target.Address = "tcp://${logger}.company.lan/";
            target.SenderFactory = senderFactory;
            target.Layout = "${message}";
            target.KeepConnection = true;
            target.MaxMessageSize = 10;
            target.OnOverflow = NetworkTargetOverflowAction.Error;
            target.Initialize(null);

            int droppedLogs = 0;
            target.LogEventDropped += (sender, args) => droppedLogs++;

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
            Assert.Equal("NetworkTarget: Discarded LogEvent because MessageSize=15 is above MaxMessageSize=10", exceptions[1].Message);
            Assert.Null(exceptions[2]);

            target.Close();

            string result = senderFactory.Log.ToString();
            Assert.True(result.IndexOf("1: connect tcp://logger1.company.lan/") != -1);
            Assert.True(result.IndexOf("1: send 0 7") != -1);
            Assert.True(result.IndexOf("1: close") != -1);
            Assert.True(result.IndexOf("2: connect tcp://logger2.company.lan/") != -1);
            Assert.True(result.IndexOf("2: send 0 5") != -1);
            Assert.True(result.IndexOf("2: close") != -1);

            Assert.Equal(1, droppedLogs);
        }

        [Fact]
        public void NetworkTargetSendFailureTests()
        {
            var senderFactory = new MyQueudSenderFactory()
            {
                FailCounter = 3, // first 3 sends will fail
                AsyncMode = false,  // Disable async, because unit-test cannot handle it
            };

            var target = new NetworkTarget();
            target.Address = "tcp://${logger}.company.lan/";
            target.SenderFactory = senderFactory;
            target.Layout = "${message}";
            target.KeepConnection = true;
            target.OnOverflow = NetworkTargetOverflowAction.Discard;

            int droppedLogsDueToSendFailure = 0;
            int droppedLogsOther = 0;
            target.LogEventDropped += (sender, args) =>
            {
                if (args.Reason == NetworkLogEventDroppedReason.NetworkError)
                {
                    droppedLogsDueToSendFailure++;
                }
                else
                {
                    droppedLogsOther++;
                }
            };

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

            Assert.Equal(3, droppedLogsDueToSendFailure);
            Assert.Equal(0, droppedLogsOther);
        }

        [Fact]
        public void NetworkTargetTcpTest()
        {
            var loopBackIP = Socket.OSSupportsIPv4 ? IPAddress.Loopback : IPAddress.IPv6Loopback;
            var tcpPrefix = loopBackIP.AddressFamily == AddressFamily.InterNetwork ? "tcp4" : "tcp6";
            var tcpPort = getNewNetworkPort();

            string expectedResult = string.Empty;

            using (var listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            using (var target = new NetworkTarget()
            {
                Address = $"{tcpPrefix}://{loopBackIP}:{tcpPort}",
                Layout = "${message}\n",
                KeepConnection = true,
            })
            {
                Exception receiveException = null;
                var resultStream = new MemoryStream();
                var receiveFinished = new ManualResetEvent(false);

                listener.Bind(new IPEndPoint(loopBackIP, tcpPort));
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

        [Fact]
        public void NetworkTargetUdpSplitEnabledTest()
        {
            RetryingIntegrationTest(3, () => NetworkTargetUdpTest(true));
        }

        [Fact]
        public void NetworkTargetUdpSplitDisabledTest()
        {
            RetryingIntegrationTest(3, () => NetworkTargetUdpTest(false));
        }

        private static int getNewNetworkPort()
        {
            return 9500 + System.Threading.Interlocked.Increment(ref _portOffset);
        }
        private static int _portOffset;

        private static void NetworkTargetUdpTest(bool splitMessage)
        {
            var loopBackIP = Socket.OSSupportsIPv4 ? IPAddress.Loopback : IPAddress.IPv6Loopback;
            var udpPrefix = loopBackIP.AddressFamily == AddressFamily.InterNetwork ? "udp4" : "udp6";
            var udpPort = getNewNetworkPort();

            string expectedResult = string.Empty;

            using (var listener = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
            using (var target = new NetworkTarget()
            {
                Address = $"{udpPrefix}://{loopBackIP}:{udpPort}",
                Layout = "${message}\n",
                KeepConnection = true,
                MaxMessageSize = splitMessage ? 4 : short.MaxValue,
            })
            {
                Exception receiveException = null;
                var receivedMessages = new List<string>();
                var receiveFinished = new ManualResetEvent(false);

                byte[] receiveBuffer = new byte[4096];

                listener.Bind(new IPEndPoint(loopBackIP, udpPort));
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
                Address = "tcp4://localhost:33415",
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
        public void NetworkTargetQueueDiscardTest()
        {
            var senderFactory = new MyQueudSenderFactory();
            var target = new NetworkTarget();
            target.MaxQueueSize = 1;
            target.Address = "tcp://someaddress/";
            target.SenderFactory = senderFactory;
            target.Layout = "${message}";
            target.KeepConnection = true;
            target.Initialize(null);

            int discardEventCalls = 0;
            target.LogEventDropped += (sender, args) => discardEventCalls++;

            int pendingWrites = 1;
            var exceptions = new List<Exception>();
            var mre = new ManualResetEvent(false);
            AsyncContinuation asyncContinuation = ex =>
            {
                lock (exceptions)
                {
                    exceptions.Add(ex);
                    if (--pendingWrites == 0)
                        mre.Set();
                }
            };

            target.WriteAsyncLogEvent(new LogEventInfo(LogLevel.Info, "logger", "msg0").WithContinuation(asyncContinuation));
            Assert.True(mre.WaitOne(10000), "Network Write not completed");
            Assert.Equal(1, senderFactory.BeginRequestCounter);
            Assert.Single(exceptions);

            foreach (var ex in exceptions)
            {
                Assert.True(ex == null, $"Network Write Exception: {ex?.ToString()}");
            }

            senderFactory.FailCounter = 1;
            senderFactory.AsyncSlowCounter = 1;

            pendingWrites = 5;  // 1st = Block, 2nd = Queue Pending
            mre.Reset();
            for (int i = 1; i <= 5 + 2; ++i)
            {
                target.WriteAsyncLogEvent(new LogEventInfo(LogLevel.Info, "logger", $"msg{i}").WithContinuation(asyncContinuation));
            }
            Assert.True(mre.WaitOne(10000), "Network Write not completed");
            Assert.True(exceptions.Count >= 6, $"Network write not completed: {exceptions.Count}");
            Assert.True(senderFactory.BeginRequestCounter <= 3, $"Network write not discarded: {senderFactory.BeginRequestCounter}");

            foreach (var ex in exceptions.Take(6))
            {
                Assert.True(ex == null, $"Network Write Exception: {ex?.ToString()}");
            }

            Assert.True(discardEventCalls >= 5, $"LogDropped not called: {discardEventCalls}");
        }

        [Fact]
        public void NetworkTargetQueueGrowTest()
        {
            var senderFactory = new MyQueudSenderFactory();
            var target = new NetworkTarget();
            target.MaxQueueSize = 1;
            target.OnQueueOverflow = NetworkTargetQueueOverflowAction.Grow;
            target.Address = "tcp://someaddress/";
            target.SenderFactory = senderFactory;
            target.Layout = "${message}";
            target.KeepConnection = true;
            target.Initialize(null);

            int pendingWrites = 1;
            var exceptions = new List<Exception>();
            var mre = new ManualResetEvent(false);
            AsyncContinuation asyncContinuation = ex =>
            {
                lock (exceptions)
                {
                    exceptions.Add(ex);
                    if (--pendingWrites == 0)
                        mre.Set();
                }
            };

            target.WriteAsyncLogEvent(new LogEventInfo(LogLevel.Info, "logger", "msg0").WithContinuation(asyncContinuation));
            Assert.True(mre.WaitOne(10000), "Network Write not completed");
            Assert.Equal(1, senderFactory.BeginRequestCounter);
            Assert.Single(exceptions);

            foreach (var ex in exceptions)
            {
                Assert.True(ex == null, $"Network Write Exception: {ex?.ToString()}");
            }

            senderFactory.AsyncSlowCounter = 1;

            pendingWrites = 5;
            mre.Reset();
            for (int i = 1; i <= 5; ++i)
            {
                target.WriteAsyncLogEvent(new LogEventInfo(LogLevel.Info, "logger", $"msg{i}").WithContinuation(asyncContinuation));
            }
            Assert.True(exceptions.Count < 4, $"Network write not growing: {exceptions.Count}");
            Assert.True(mre.WaitOne(10000), "Network Write not completed");
            Assert.Equal(6, exceptions.Count);
            Assert.Equal(6, senderFactory.BeginRequestCounter);

            foreach (var ex in exceptions)
            {
                Assert.True(ex == null, $"Network Write Exception: {ex?.ToString()}");
            }
        }

        [Fact]
        public void NetworkTargetQueueBlockTest()
        {
            var senderFactory = new MyQueudSenderFactory();
            var target = new NetworkTarget();
            target.MaxQueueSize = 1;
            target.OnQueueOverflow = NetworkTargetQueueOverflowAction.Block;
            target.Address = "tcp://someaddress/";
            target.SenderFactory = senderFactory;
            target.Layout = "${message}";
            target.KeepConnection = true;
            target.Initialize(null);

            int pendingWrites = 1;
            var exceptions = new List<Exception>();
            var mre = new ManualResetEvent(false);
            AsyncContinuation asyncContinuation = ex =>
            {
                lock (exceptions)
                {
                    exceptions.Add(ex);
                    if (--pendingWrites == 0)
                        mre.Set();
                }
            };

            target.WriteAsyncLogEvent(new LogEventInfo(LogLevel.Info, "logger", "msg0").WithContinuation(asyncContinuation));
            Assert.True(mre.WaitOne(10000), "Network Write not completed");
            Assert.Equal(1, senderFactory.BeginRequestCounter);
            Assert.Single(exceptions);

            foreach (var ex in exceptions)
            {
                Assert.True(ex == null, $"Network Write Exception: {ex?.ToString()}");
            }

            senderFactory.AsyncSlowCounter = 1;

            pendingWrites = 5;
            mre.Reset();
            for (int i = 1; i <= 5; ++i)
            {
                target.WriteAsyncLogEvent(new LogEventInfo(LogLevel.Info, "logger", $"msg{i}").WithContinuation(asyncContinuation));
            }
            Assert.True(exceptions.Count >= 4, $"Network write not blocking: {exceptions.Count}");
            Assert.True(mre.WaitOne(10000), "Network Write not completed");
            Assert.Equal(6, exceptions.Count);
            Assert.Equal(6, senderFactory.BeginRequestCounter);

            foreach (var ex in exceptions)
            {
                Assert.True(ex == null, $"Network Write Exception: {ex?.ToString()}");
            }
        }

        [Fact]
        public void NetworkTargetSendFailureWithoutKeepAliveTests()
        {
            var senderFactory = new MyQueudSenderFactory()
            {
                FailCounter = 3, // first 3 sends will fail
                AsyncMode = false,  // Disable async, because unit-test cannot handle it
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

            var logFactory = new LogFactory().Setup().LoadConfigurationFromXml($@"
            <nlog>
                <targets async='true'><target name='target1' type='network' layout='${{level}}|${{threadid}}|${{message}}' Address='tcp://127.0.0.1:50001' keepAliveTimeSeconds='{keepAliveTimeSeconds}' /></targets>
                <rules><logger name='*' minLevel='Trace' writeTo='target1'/></rules>
            </nlog>").LogFactory;

            var target = logFactory.Configuration.FindTargetByName<NetworkTarget>("target1");
            Assert.Equal(expected, target.KeepAliveTimeSeconds);

            var logger = logFactory.GetLogger("keepAliveTimeSeconds");
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

        [Fact]
        public void GzipCompressionTest()
        {
            var senderFactory = new MyQueudSenderFactory();
            var target = new NetworkTarget();
            target.Address = "tcp://someaddress/";
            target.SenderFactory = senderFactory;
            target.Layout = "${message}";
            target.KeepConnection = false;
            target.Compress = NetworkTargetCompressionType.GZip;
            target.CompressMinBytes = 15;
            target.LineEnding = LineEndingMode.CRLF;
            target.Initialize(null);

            var exceptions = new List<Exception>();
            var mre = new ManualResetEvent(false);
            AsyncContinuation asyncContinuation = ex =>
            {
                lock (exceptions)
                {
                    exceptions.Add(ex);
                    mre.Set();
                }
            };

            mre.Reset();
            target.WriteAsyncLogEvent(new LogEventInfo(LogLevel.Info, "logger", "smallmessage").WithContinuation(asyncContinuation));
            Assert.True(mre.WaitOne(10000), "Network Write not completed");
            var smallMessage = target.Encoding.GetString(senderFactory.Senders.Last().MemoryStream.GetBuffer(), 0, (int)senderFactory.Senders.Last().MemoryStream.Length);
            Assert.Equal("smallmessage\r\n", smallMessage);

            mre.Reset();
            target.WriteAsyncLogEvent(new LogEventInfo(LogLevel.Info, "logger", "superbigmessage").WithContinuation(asyncContinuation));
            Assert.True(mre.WaitOne(10000), "Network Write not completed");
            senderFactory.Senders.Last().MemoryStream.Position = 0;
            using (var unzipper = new System.IO.Compression.GZipStream(senderFactory.Senders.Last().MemoryStream, System.IO.Compression.CompressionMode.Decompress, true))
            using (var outstream = new MemoryStream())
            {
                unzipper.CopyTo(outstream);
                unzipper.Flush();
                var bigMessage = target.Encoding.GetString(outstream.GetBuffer(), 0, (int)outstream.Length);
                Assert.Equal("superbigmessage\r\n", bigMessage);
            } 
        }

        internal class MyQueudSenderFactory : INetworkSenderFactory
        {
            internal List<MyQueudNetworkSender> Senders = new List<MyQueudNetworkSender>();
            internal StringWriter Log = new StringWriter();
            private int _idCounter;

            public QueuedNetworkSender Create(string url, int maxQueueSize, NetworkTargetQueueOverflowAction onQueueOverflow, int maxMessageSize, SslProtocols sslProtocols, TimeSpan keepAliveTime)
            {
                var sender = new MyQueudNetworkSender(url, ++_idCounter, Log, this) { MaxQueueSize = maxQueueSize, OnQueueOverflow = onQueueOverflow };
                Senders.Add(sender);
                return sender;
            }

            public int BeginRequestCounter { get; set; }

            public int FailCounter { get; set; }

            public int AsyncSlowCounter { get; set; }

            public bool AsyncMode { get; set; } = true;
        }

        internal class MyQueudNetworkSender : QueuedNetworkSender
        {
            private readonly int _id;
            private readonly TextWriter _log;
            private readonly MyQueudSenderFactory _senderFactory;
            public MemoryStream MemoryStream { get; } = new MemoryStream();

            public MyQueudNetworkSender(string url, int id, TextWriter log, MyQueudSenderFactory senderFactory)
                : base(url)
            {
                _id = id;
                _log = log;
                _senderFactory = senderFactory;
            }

            protected override void DoInitialize()
            {
                base.DoInitialize();
                lock (_log)
                {
                    _log.WriteLine("{0}: connect {1}", _id, Address);
                }
            }

            protected override void DoFlush(AsyncContinuation continuation)
            {
                lock (_log)
                {
                    _log.WriteLine("{0}: flush", _id);
                }

                base.DoFlush(continuation);
            }

            protected override void DoClose(AsyncContinuation continuation)
            {
                lock (_log)
                {
                    _log.WriteLine("{0}: close", _id);
                }
                base.DoClose(continuation);
            }

            protected override void BeginRequest(NetworkRequestArgs eventArgs)
            {
                RegisterNextRequest();

                if (_senderFactory.AsyncMode)
                {
                    var asyncTask = new NLog.Internal.AsyncHelpersTask(state => { SendSync(eventArgs); });
                    AsyncHelpers.StartAsyncTask(asyncTask, null);
                }
                else
                {
                    SendSync(eventArgs);
                }
            }

            private void SendSync(NetworkRequestArgs? nextRequest)
            {
                while (nextRequest.HasValue)
                {
                    Thread.Sleep(CheckSendThrottleTimeMs());

                    var eventArgs = nextRequest.Value;

                    var failedException = CheckForFailedException();

                    lock (_log)
                    {
                        _log.WriteLine("{0}: send {1} {2}", _id, eventArgs.RequestBufferOffset, eventArgs.RequestBufferLength);
                        MemoryStream.Write(eventArgs.RequestBuffer, eventArgs.RequestBufferOffset, eventArgs.RequestBufferLength);
                        if (failedException != null)
                        {
                            _log.WriteLine("{0}: failed", _id);
                        }
                    }

                    nextRequest = EndRequest(eventArgs.AsyncContinuation, failedException);
                    if (nextRequest.HasValue)
                    {
                        RegisterNextRequest();
                    }
                } 
            }

            private void RegisterNextRequest()
            {
                lock (_senderFactory)
                {
                    _senderFactory.BeginRequestCounter++;
                }
            }

            private Exception CheckForFailedException()
            {
                lock (_senderFactory)
                {
                    if (_senderFactory.FailCounter > 0)
                    {
                        _senderFactory.FailCounter--;
                        return new IOException("some IO error has occured");
                    }
                }

                return null;
            }

            private int CheckSendThrottleTimeMs()
            {
                lock (_senderFactory)
                {
                    if (_senderFactory.AsyncSlowCounter > 0)
                    {
                        _senderFactory.AsyncSlowCounter--;
                        return 250;
                    }

                    return _senderFactory.AsyncMode ? 1 : 0;
                }
            }
        }
    }
}
