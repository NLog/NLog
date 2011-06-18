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

namespace NLog.UnitTests.Targets.Wrappers
{
    using System;
    using System.Threading;
    using NUnit.Framework;

#if !NUNIT
    using SetUp = Microsoft.VisualStudio.TestTools.UnitTesting.TestInitializeAttribute;
    using TestFixture = Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute;
    using Test = Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute;
    using TearDown =  Microsoft.VisualStudio.TestTools.UnitTesting.TestCleanupAttribute;
#endif
    using NLog.Common;
    using NLog.Internal;
    using NLog.Targets;
    using NLog.Targets.Wrappers;

    [TestFixture]
    public class BufferingTargetWrapperTests : NLogTestBase
	{
        [Test]
        public void BufferingTargetWrapperSyncTest1()
        {
            var myTarget = new MyTarget();
            var targetWrapper = new BufferingTargetWrapper
            {
                WrappedTarget = myTarget,
                BufferSize = 10,
            };

            myTarget.Initialize(null);
            targetWrapper.Initialize(null);

            int totalEvents = 100;

            var continuationHit = new bool[totalEvents];
            var lastException = new Exception[totalEvents];
            var continuationThread = new Thread[totalEvents];
            int hitCount = 0;

            CreateContinuationFunc createAsyncContinuation = 
                eventNumber =>
                    ex =>
                    {
                        lastException[eventNumber] = ex;
                        continuationThread[eventNumber] = Thread.CurrentThread;
                        continuationHit[eventNumber] = true;
                        Interlocked.Increment(ref hitCount);
                    };

            // write 9 events - they will all be buffered and no final continuation will be reached
            int eventCounter = 0;
            for (int i = 0; i < 9; ++i)
            {
                targetWrapper.WriteAsyncLogEvent(new LogEventInfo().WithContinuation(createAsyncContinuation(eventCounter++)));
            }

            Assert.AreEqual(0, hitCount);
            Assert.AreEqual(0, myTarget.WriteCount);

            // write one more event - everything will be flushed
            targetWrapper.WriteAsyncLogEvent(new LogEventInfo().WithContinuation(createAsyncContinuation(eventCounter++)));
            Assert.AreEqual(10, hitCount);
            Assert.AreEqual(1, myTarget.BufferedWriteCount);
            Assert.AreEqual(10, myTarget.BufferedTotalEvents);
            Assert.AreEqual(10, myTarget.WriteCount);
            for (int i = 0; i < hitCount; ++i)
            {
                Assert.AreSame(Thread.CurrentThread, continuationThread[i]);
                Assert.IsNull(lastException[i]);
            }

            // write 9 more events - they will all be buffered and no final continuation will be reached
            for (int i = 0; i < 9; ++i)
            {
                targetWrapper.WriteAsyncLogEvent(new LogEventInfo().WithContinuation(createAsyncContinuation(eventCounter++)));
            }

            // no change
            Assert.AreEqual(10, hitCount);
            Assert.AreEqual(1, myTarget.BufferedWriteCount);
            Assert.AreEqual(10, myTarget.BufferedTotalEvents);
            Assert.AreEqual(10, myTarget.WriteCount);

            Exception flushException = null;
            var flushHit = new ManualResetEvent(false);

            targetWrapper.Flush(
                ex =>
                    {
                        flushException = ex;
                        flushHit.Set();
                    });

            Thread.Sleep(1000);

            flushHit.WaitOne();
            Assert.IsNull(flushException);

            // make sure remaining events were written
            Assert.AreEqual(19, hitCount);
            Assert.AreEqual(2, myTarget.BufferedWriteCount);
            Assert.AreEqual(19, myTarget.BufferedTotalEvents);
            Assert.AreEqual(19, myTarget.WriteCount);
            Assert.AreEqual(1, myTarget.FlushCount);

            // flushes happen on the same thread
            for (int i = 10; i < hitCount; ++i)
            {
                Assert.IsNotNull(continuationThread[i]);
                Assert.AreSame(Thread.CurrentThread, continuationThread[i], "Invalid thread #" + i);
                Assert.IsNull(lastException[i]);
            }

            // flush again - should just invoke Flush() on the wrapped target
            flushHit.Reset();
            targetWrapper.Flush(
                ex =>
                {
                    flushException = ex;
                    flushHit.Set();
                });

            flushHit.WaitOne();
            Assert.AreEqual(19, hitCount);
            Assert.AreEqual(2, myTarget.BufferedWriteCount);
            Assert.AreEqual(19, myTarget.BufferedTotalEvents);
            Assert.AreEqual(19, myTarget.WriteCount);
            Assert.AreEqual(2, myTarget.FlushCount);

            targetWrapper.Close();
            myTarget.Close();
        }

        [Test]
        public void BufferingTargetWrapperSyncWithTimedFlushTest()
        {
            var myTarget = new MyTarget();
            var targetWrapper = new BufferingTargetWrapper
            {
                WrappedTarget = myTarget,
                BufferSize = 10,
                FlushTimeout = 1000,
            };

            myTarget.Initialize(null);
            targetWrapper.Initialize(null);

            int totalEvents = 100;

            var continuationHit = new bool[totalEvents];
            var lastException = new Exception[totalEvents];
            var continuationThread = new Thread[totalEvents];
            int hitCount = 0;

            CreateContinuationFunc createAsyncContinuation =
                eventNumber =>
                    ex =>
                    {
                        lastException[eventNumber] = ex;
                        continuationThread[eventNumber] = Thread.CurrentThread;
                        continuationHit[eventNumber] = true;
                        Interlocked.Increment(ref hitCount);
                    };

            // write 9 events - they will all be buffered and no final continuation will be reached
            int eventCounter = 0;
            for (int i = 0; i < 9; ++i)
            {
                targetWrapper.WriteAsyncLogEvent(new LogEventInfo().WithContinuation(createAsyncContinuation(eventCounter++)));
            }

            Assert.AreEqual(0, hitCount);
            Assert.AreEqual(0, myTarget.WriteCount);

            // sleep 2 seconds, this will trigger the timer and flush all events
            Thread.Sleep(4000);
            Assert.AreEqual(9, hitCount);
            Assert.AreEqual(1, myTarget.BufferedWriteCount);
            Assert.AreEqual(9, myTarget.BufferedTotalEvents);
            Assert.AreEqual(9, myTarget.WriteCount);
            for (int i = 0; i < hitCount; ++i)
            {
                Assert.AreNotSame(Thread.CurrentThread, continuationThread[i]);
                Assert.IsNull(lastException[i]);
            }

            // write 11 more events, 10 will be hit immediately because the buffer will fill up
            // 1 will be pending
            for (int i = 0; i < 11; ++i)
            {
                targetWrapper.WriteAsyncLogEvent(new LogEventInfo().WithContinuation(createAsyncContinuation(eventCounter++)));
            }

            Assert.AreEqual(19, hitCount);
            Assert.AreEqual(2, myTarget.BufferedWriteCount);
            Assert.AreEqual(19, myTarget.BufferedTotalEvents);
            Assert.AreEqual(19, myTarget.WriteCount);

            // sleep 2 seonds and the last remaining one will be flushed
            Thread.Sleep(2000);
            Assert.AreEqual(20, hitCount);
            Assert.AreEqual(3, myTarget.BufferedWriteCount);
            Assert.AreEqual(20, myTarget.BufferedTotalEvents);
            Assert.AreEqual(20, myTarget.WriteCount);
        }

        [Test]
        public void BufferingTargetWrapperAsyncTest1()
        {
            var myTarget = new MyAsyncTarget();
            var targetWrapper = new BufferingTargetWrapper
            {
                WrappedTarget = myTarget,
                BufferSize = 10,
            };

            myTarget.Initialize(null);
            targetWrapper.Initialize(null);

            int totalEvents = 100;

            var continuationHit = new bool[totalEvents];
            var lastException = new Exception[totalEvents];
            var continuationThread = new Thread[totalEvents];
            int hitCount = 0;

            CreateContinuationFunc createAsyncContinuation =
                eventNumber =>
                    ex =>
                    {
                        lastException[eventNumber] = ex;
                        continuationThread[eventNumber] = Thread.CurrentThread;
                        continuationHit[eventNumber] = true;
                        Interlocked.Increment(ref hitCount);
                    };

            // write 9 events - they will all be buffered and no final continuation will be reached
            int eventCounter = 0;
            for (int i = 0; i < 9; ++i)
            {
                targetWrapper.WriteAsyncLogEvent(new LogEventInfo().WithContinuation(createAsyncContinuation(eventCounter++)));
            }

            Assert.AreEqual(0, hitCount);

            // write one more event - everything will be flushed
            targetWrapper.WriteAsyncLogEvent(new LogEventInfo().WithContinuation(createAsyncContinuation(eventCounter++)));

            while (hitCount < 10)
            {
                Thread.Sleep(10);
            }

            Assert.AreEqual(10, hitCount);
            Assert.AreEqual(1, myTarget.BufferedWriteCount);
            Assert.AreEqual(10, myTarget.BufferedTotalEvents);
            for (int i = 0; i < hitCount; ++i)
            {
                Assert.AreNotSame(Thread.CurrentThread, continuationThread[i]);
                Assert.IsNull(lastException[i]);
            }

            // write 9 more events - they will all be buffered and no final continuation will be reached
            for (int i = 0; i < 9; ++i)
            {
                targetWrapper.WriteAsyncLogEvent(new LogEventInfo().WithContinuation(createAsyncContinuation(eventCounter++)));
            }

            // no change
            Assert.AreEqual(10, hitCount);
            Assert.AreEqual(1, myTarget.BufferedWriteCount);
            Assert.AreEqual(10, myTarget.BufferedTotalEvents);

            Exception flushException = null;
            var flushHit = new ManualResetEvent(false);

            targetWrapper.Flush(
                ex =>
                {
                    flushException = ex;
                    flushHit.Set();
                });

            Thread.Sleep(1000);

            flushHit.WaitOne();
            Assert.IsNull(flushException);

            // make sure remaining events were written
            Assert.AreEqual(19, hitCount);
            Assert.AreEqual(2, myTarget.BufferedWriteCount);
            Assert.AreEqual(19, myTarget.BufferedTotalEvents);

            // flushes happen on another thread
            for (int i = 10; i < hitCount; ++i)
            {
                Assert.IsNotNull(continuationThread[i]);
                Assert.AreNotSame(Thread.CurrentThread, continuationThread[i], "Invalid thread #" + i);
                Assert.IsNull(lastException[i]);
            }

            // flush again - should not do anything
            flushHit.Reset();
            targetWrapper.Flush(
                ex =>
                {
                    flushException = ex;
                    flushHit.Set();
                });

            flushHit.WaitOne();
            Assert.AreEqual(19, hitCount);
            Assert.AreEqual(2, myTarget.BufferedWriteCount);
            Assert.AreEqual(19, myTarget.BufferedTotalEvents);

            targetWrapper.Close();
            myTarget.Close();
        }

        [Test]
        public void BufferingTargetWrapperSyncWithTimedFlushNonSlidingTest()
        {
            var myTarget = new MyTarget();
            var targetWrapper = new BufferingTargetWrapper
            {
                WrappedTarget = myTarget,
                BufferSize = 10,
                FlushTimeout = 400,
                SlidingTimeout = false,
            };

            myTarget.Initialize(null);
            targetWrapper.Initialize(null);

            int totalEvents = 100;

            var continuationHit = new bool[totalEvents];
            var lastException = new Exception[totalEvents];
            var continuationThread = new Thread[totalEvents];
            int hitCount = 0;

            CreateContinuationFunc createAsyncContinuation =
                eventNumber =>
                    ex =>
                    {
                        lastException[eventNumber] = ex;
                        continuationThread[eventNumber] = Thread.CurrentThread;
                        continuationHit[eventNumber] = true;
                        Interlocked.Increment(ref hitCount);
                    };

            int eventCounter = 0;
            targetWrapper.WriteAsyncLogEvent(new LogEventInfo().WithContinuation(createAsyncContinuation(eventCounter++)));
            Thread.Sleep(300);

            Assert.AreEqual(0, hitCount);
            Assert.AreEqual(0, myTarget.WriteCount);

            targetWrapper.WriteAsyncLogEvent(new LogEventInfo().WithContinuation(createAsyncContinuation(eventCounter++)));
            Thread.Sleep(300);

            Assert.AreEqual(2, hitCount);
            Assert.AreEqual(2, myTarget.WriteCount);
        }

        [Test]
        public void BufferingTargetWrapperSyncWithTimedFlushSlidingTest()
        {
            var myTarget = new MyTarget();
            var targetWrapper = new BufferingTargetWrapper
            {
                WrappedTarget = myTarget,
                BufferSize = 10,
                FlushTimeout = 400,
            };

            myTarget.Initialize(null);
            targetWrapper.Initialize(null);

            int totalEvents = 100;

            var continuationHit = new bool[totalEvents];
            var lastException = new Exception[totalEvents];
            var continuationThread = new Thread[totalEvents];
            int hitCount = 0;

            CreateContinuationFunc createAsyncContinuation =
                eventNumber =>
                    ex =>
                    {
                        lastException[eventNumber] = ex;
                        continuationThread[eventNumber] = Thread.CurrentThread;
                        continuationHit[eventNumber] = true;
                        Interlocked.Increment(ref hitCount);
                    };

            int eventCounter = 0;
            targetWrapper.WriteAsyncLogEvent(new LogEventInfo().WithContinuation(createAsyncContinuation(eventCounter++)));
            Thread.Sleep(300);

            Assert.AreEqual(0, hitCount);
            Assert.AreEqual(0, myTarget.WriteCount);

            targetWrapper.WriteAsyncLogEvent(new LogEventInfo().WithContinuation(createAsyncContinuation(eventCounter++)));
            Thread.Sleep(300);

            Assert.AreEqual(0, hitCount);
            Assert.AreEqual(0, myTarget.WriteCount);

            Thread.Sleep(200);
            Assert.AreEqual(2, hitCount);
            Assert.AreEqual(2, myTarget.WriteCount);
        }

        class MyAsyncTarget : Target
        {
            public int BufferedWriteCount { get; set; }
            public int BufferedTotalEvents { get; set; }

            protected override void Write(LogEventInfo logEvent)
            {
                throw new NotSupportedException();
            }

            protected override void Write(AsyncLogEventInfo[] logEvents)
            {
                this.BufferedWriteCount++;
                this.BufferedTotalEvents += logEvents.Length;

                for (int i = 0; i < logEvents.Length; ++i)
                {
                    var logEvent = logEvents[i];

                    ThreadPool.QueueUserWorkItem(
                        s =>
                            {
                                if (this.ThrowExceptions)
                                {
                                    logEvent.Continuation(new InvalidOperationException("Some problem!"));
                                    logEvent.Continuation(new InvalidOperationException("Some problem!"));
                                }
                                else
                                {
                                    logEvent.Continuation(null);
                                    logEvent.Continuation(null);
                                }
                            });
                }
            }

            protected override void FlushAsync(AsyncContinuation asyncContinuation)
            {
                ThreadPool.QueueUserWorkItem(
                    s => asyncContinuation(null));
            }

            public bool ThrowExceptions { get; set; }
        }

        class MyTarget : Target
        {
            public int FlushCount { get; set; }
            public int WriteCount { get; set; }
            public int BufferedWriteCount { get; set; }
            public int BufferedTotalEvents { get; set; }

            protected override void Write(AsyncLogEventInfo[] logEvents)
            {
                this.BufferedWriteCount++;
                this.BufferedTotalEvents += logEvents.Length;
                base.Write(logEvents);
            }

            protected override void Write(LogEventInfo logEvent)
            {
                Assert.IsTrue(this.FlushCount <= this.WriteCount);
                this.WriteCount++;
            }

            protected override void FlushAsync(AsyncContinuation asyncContinuation)
            {
                this.FlushCount++;
                asyncContinuation(null);
            }
        }

        private delegate AsyncContinuation CreateContinuationFunc(int eventNumber); 
    }
}
