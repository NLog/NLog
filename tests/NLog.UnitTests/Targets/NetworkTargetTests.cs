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

namespace NLog.UnitTests.Targets
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
    using NLog.Common;
    using NLog.Config;
    using NLog.Internal.NetworkSenders;
    using NLog.Targets;

    [TestFixture]
    public class NetworkTargetTests : NLogTestBase
    {
        [Test]
        public void NetworkTargetHappyPathTest()
        {
            var senderFactory = new MySenderFactory();
            var target = new NetworkTarget();
            target.Address = "tcp://someaddress/";
            target.SenderFactory = senderFactory;
            target.Layout = "${message}";
            target.NewLine = true;
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
                    Assert.Fail(ex.ToString());
                }
            }

            Assert.AreEqual(1, senderFactory.Senders.Count);

            var sender = senderFactory.Senders[0];
            target.Close();

            Assert.AreEqual(18L, sender.MemoryStream.Length);
            Assert.AreEqual("msg1\r\nmsg2\r\nmsg3\r\n", target.Encoding.GetString(sender.MemoryStream.GetBuffer(), 0, (int)sender.MemoryStream.Length));

            // we invoke the sender 3 times, each time sending 4 bytes
            string expectedLog = @"1: connect tcp://someaddress/
1: send 0 6
1: send 0 6
1: send 0 6
1: close
";
            Assert.AreEqual(expectedLog, senderFactory.Log.ToString());
        }

        [Test]
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
                    Assert.Fail(ex.ToString());
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

            string expectedLog = @"1: connect tcp://logger1.company.lan/
1: send 0 4
2: connect tcp://logger2.company.lan/
2: send 0 4
3: connect tcp://logger3.company.lan/
3: send 0 4
1: flush
2: flush
3: flush
1: close
2: close
3: close
";
            Assert.AreEqual(expectedLog, senderFactory.Log.ToString());
        }

        [Test]
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
            Assert.AreEqual(expectedLog, senderFactory.Log.ToString());
        }

        [Test]
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
                    Assert.Fail(ex.ToString());
                }
            }

            target.Close();

            string expectedLog = @"1: connect tcp://logger1.company.lan/
1: send 0 4
2: connect tcp://logger2.company.lan/
2: send 0 4
1: send 0 4
2: close
3: connect tcp://logger3.company.lan/
3: send 0 4
1: send 0 4
3: close
4: connect tcp://logger2.company.lan/
4: send 0 4
1: close
4: close
";
            Assert.AreEqual(expectedLog, senderFactory.Log.ToString());
        }

        [Test]
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
                    Assert.Fail(ex.ToString());
                }
            }

            target.Close();

            string expectedLog = @"1: connect tcp://logger1.company.lan/
1: send 0 4
1: close
2: connect tcp://logger2.company.lan/
2: send 0 4
2: close
3: connect tcp://logger1.company.lan/
3: send 0 4
3: close
4: connect tcp://logger3.company.lan/
4: send 0 4
4: close
5: connect tcp://logger1.company.lan/
5: send 0 4
5: close
6: connect tcp://logger2.company.lan/
6: send 0 4
6: close
";
            Assert.AreEqual(expectedLog, senderFactory.Log.ToString());
        }

        [Test]
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
                    Assert.Fail(ex.ToString());
                }
            }

            target.Close();

            string expectedLog = @"1: connect tcp://logger1.company.lan/
1: send 0 9
1: send 9 9
1: send 18 9
1: send 27 3
1: send 0 9
1: send 9 6
2: connect tcp://logger2.company.lan/
2: send 0 9
2: send 9 9
2: send 18 6
1: close
2: close
";
            Assert.AreEqual(expectedLog, senderFactory.Log.ToString());
        }

        [Test]
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
                    Assert.Fail(ex.ToString());
                }
            }

            target.Close();

            string expectedLog = @"1: connect tcp://logger1.company.lan/
1: send 0 7
2: connect tcp://logger2.company.lan/
2: send 0 5
1: close
2: close
";
            Assert.AreEqual(expectedLog, senderFactory.Log.ToString());
        }

        [Test]
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
            Assert.IsNull(exceptions[0]);
            Assert.IsNotNull(exceptions[1]);
            Assert.AreEqual("Attempted to send a message larger than MaxMessageSize (10). Actual size was: 15. Adjust OnOverflow and MaxMessageSize parameters accordingly.", exceptions[1].Message);
            Assert.IsNull(exceptions[2]);

            target.Close();

            string expectedLog = @"1: connect tcp://logger1.company.lan/
1: send 0 7
1: close
2: connect tcp://logger2.company.lan/
2: send 0 5
2: close
";
            Assert.AreEqual(expectedLog, senderFactory.Log.ToString());
        }

        [Test]
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
            Assert.IsNotNull(exceptions[0]);
            Assert.IsNotNull(exceptions[1]);
            Assert.IsNotNull(exceptions[2]);
            Assert.IsNull(exceptions[3]);
            Assert.IsNull(exceptions[4]);

            target.Close();

            string expectedLog = @"1: connect tcp://logger1.company.lan/
1: send 0 7
1: failed
1: close
2: connect tcp://logger1.company.lan/
2: send 0 7
2: failed
2: close
3: connect tcp://logger1.company.lan/
3: send 0 7
3: failed
3: close
4: connect tcp://logger1.company.lan/
4: send 0 7
4: send 0 5
4: close
";
            Assert.AreEqual(expectedLog, senderFactory.Log.ToString());
        }

#if !SILVERLIGHT
        [Test]
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
                            // Console.WriteLine("Accepting...");
                            byte[] buffer = new byte[4096];
                            using (Socket connectedSocket = listener.EndAccept(result))
                            {
                                // Console.WriteLine("Accepted...");
                                int got;
                                while ((got = connectedSocket.Receive(buffer, 0, buffer.Length, SocketFlags.None)) > 0)
                                {
                                    resultStream.Write(buffer, 0, got);
                                }
                                // Console.WriteLine("Closing connection...");
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
                            Console.WriteLine("{0} Write finished {1}", pendingWrites, ex);
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

                Assert.IsTrue(writeCompleted.WaitOne(10000, false), "Writes did not complete");
                target.Close();
                Assert.IsTrue(receiveFinished.WaitOne(10000, false), "Receive did not complete");
                string resultString = Encoding.UTF8.GetString(resultStream.GetBuffer(), 0, (int)resultStream.Length);
                Assert.IsNull(receiveException, "Receive exception: " + receiveException);
                Assert.AreEqual(expectedResult, resultString);
            }
        }

        [Test]
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

                Assert.IsTrue(writeCompleted.WaitOne(10000, false));
                target.Close();
                Assert.IsTrue(receiveFinished.WaitOne(10000, false));
                Assert.AreEqual(toWrite, receivedMessages.Count);
                for (int i = 0; i < toWrite; ++i)
                {
                    Assert.IsTrue(receivedMessages.Contains("message" + i + "\n"), "Message #" + i + " not received.");
                }

                Assert.IsNull(receiveException, "Receive exception: " + receiveException);
            }
        }

        [Test]
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
                        Console.WriteLine("Write finished. Pending {0}", pendingWrites);
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

            Console.WriteLine("Waiting for completion...");
            writeCompleted.WaitOne();

            Console.WriteLine("Closing...");

            // no exception
            target.Close();

            Assert.AreEqual(toWrite, exceptions.Count);
            foreach (var ex in exceptions)
            {
                Assert.IsNotNull(ex);
            }

            Thread.Sleep(1000);
        }
#endif

        [Test]
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
            Assert.IsNotNull(exceptions[0]);
            Assert.IsNotNull(exceptions[1]);
            Assert.IsNotNull(exceptions[2]);
            Assert.IsNull(exceptions[3]);
            Assert.IsNull(exceptions[4]);

            target.Close();

            string expectedLog = @"1: connect tcp://logger1.company.lan/
1: send 0 7
1: failed
1: close
2: connect tcp://logger1.company.lan/
2: send 0 7
2: failed
2: close
3: connect tcp://logger1.company.lan/
3: send 0 7
3: failed
3: close
4: connect tcp://logger1.company.lan/
4: send 0 7
4: close
5: connect tcp://logger1.company.lan/
5: send 0 5
5: close
";
            Assert.AreEqual(expectedLog, senderFactory.Log.ToString());
        }

        internal class MySenderFactory : INetworkSenderFactory
        {
            internal List<MyNetworkSender> Senders = new List<MyNetworkSender>();
            internal StringWriter Log = new StringWriter();
            private int idCounter;

            public NetworkSender Create(string url)
            {
                var sender = new MyNetworkSender(url, ++this.idCounter, this.Log, this);
                this.Senders.Add(sender);
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
                this.MemoryStream = new MemoryStream();
            }
            protected override void DoInitialize()
            {
                base.DoInitialize();
                this.log.WriteLine("{0}: connect {1}", this.id, this.Address);
            }

            protected override void DoFlush(AsyncContinuation continuation)
            {
                this.log.WriteLine("{0}: flush", this.id);
                continuation(null);
            }

            protected override void DoClose(AsyncContinuation continuation)
            {
                this.log.WriteLine("{0}: close", this.id);
                continuation(null);
            }

            protected override void DoSend(byte[] bytes, int offset, int length, AsyncContinuation asyncContinuation)
            {
                this.log.WriteLine("{0}: send {1} {2}", this.id, offset, length);
                this.MemoryStream.Write(bytes, offset, length);
                if (this.senderFactory.FailCounter > 0)
                {
                    this.log.WriteLine("{0}: failed", this.id);
                    this.senderFactory.FailCounter--;
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

#endif