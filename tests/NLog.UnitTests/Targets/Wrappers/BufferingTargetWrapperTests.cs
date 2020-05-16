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

namespace NLog.UnitTests.Targets.Wrappers
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using NLog.Common;
    using NLog.Targets;
    using NLog.Targets.Wrappers;
    using Xunit;

    public class BufferingTargetWrapperTests : NLogTestBase
    {
        [Fact]
        public void BufferingTargetWrapperSyncTest1()
        {
            var myTarget = new MyTarget();
            var targetWrapper = new BufferingTargetWrapper
            {
                WrappedTarget = myTarget,
                BufferSize = 10,
            };

            InitializeTargets(myTarget, targetWrapper);

            const int totalEvents = 100;

            var continuationHit = new bool[totalEvents];
            var lastException = new Exception[totalEvents];
            var continuationThread = new Thread[totalEvents];
            var hitCount = 0;

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
            var eventCounter = 0;
            for (var i = 0; i < 9; ++i)
            {
                targetWrapper.WriteAsyncLogEvent(new LogEventInfo().WithContinuation(createAsyncContinuation(eventCounter++)));
            }

            Assert.Equal(0, hitCount);
            Assert.Equal(0, myTarget.WriteCount);

            // write one more event - everything will be flushed
            targetWrapper.WriteAsyncLogEvent(new LogEventInfo().WithContinuation(createAsyncContinuation(eventCounter++)));
            Assert.Equal(10, hitCount);
            Assert.Equal(1, myTarget.BufferedWriteCount);
            Assert.Equal(10, myTarget.BufferedTotalEvents);
            Assert.Equal(10, myTarget.WriteCount);
            for (var i = 0; i < hitCount; ++i)
            {
                Assert.Same(Thread.CurrentThread, continuationThread[i]);
                Assert.Null(lastException[i]);
            }

            // write 9 more events - they will all be buffered and no final continuation will be reached
            for (var i = 0; i < 9; ++i)
            {
                targetWrapper.WriteAsyncLogEvent(new LogEventInfo().WithContinuation(createAsyncContinuation(eventCounter++)));
            }

            // no change
            Assert.Equal(10, hitCount);
            Assert.Equal(1, myTarget.BufferedWriteCount);
            Assert.Equal(10, myTarget.BufferedTotalEvents);
            Assert.Equal(10, myTarget.WriteCount);

            Exception flushException = null;
            var flushHit = new ManualResetEvent(false);

            targetWrapper.Flush(
                ex =>
                {
                    flushException = ex;
                    flushHit.Set();
                });

            Assert.True(flushHit.WaitOne(5000), "Wait Flush Timeout");
            Assert.Null(flushException);

            // make sure remaining events were written
            Assert.Equal(19, hitCount);
            Assert.Equal(2, myTarget.BufferedWriteCount);
            Assert.Equal(19, myTarget.BufferedTotalEvents);
            Assert.Equal(19, myTarget.WriteCount);
            Assert.Equal(1, myTarget.FlushCount);

            // flushes happen on the same thread
            for (var i = 10; i < hitCount; ++i)
            {
                Assert.NotNull(continuationThread[i]);
                Assert.Same(Thread.CurrentThread, continuationThread[i]);
                Assert.Null(lastException[i]);
            }

            // flush again - should just invoke Flush() on the wrapped target
            flushHit.Reset();
            targetWrapper.Flush(
                ex =>
                {
                    flushException = ex;
                    flushHit.Set();
                });

            Assert.True(flushHit.WaitOne(5000), "Wait Again Flush Timeout");
            Assert.Equal(19, hitCount);
            Assert.Equal(2, myTarget.BufferedWriteCount);
            Assert.Equal(19, myTarget.BufferedTotalEvents);
            Assert.Equal(19, myTarget.WriteCount);
            Assert.Equal(2, myTarget.FlushCount);

            targetWrapper.Close();
            myTarget.Close();
        }

        [Fact]
        public void BufferingTargetWithFallbackGroupAndFirstTargetFails_Write_SecondTargetWritesEvents()
        {
            var myTarget = new MyTarget { FailCounter = 1 };
            var myTarget2 = new MyTarget();
            var fallbackGroup = new FallbackGroupTarget(myTarget, myTarget2);
            var targetWrapper = new BufferingTargetWrapper
            {
                WrappedTarget = fallbackGroup,
                BufferSize = 10,
            };

            InitializeTargets(myTarget, targetWrapper, myTarget2, fallbackGroup);

            const int totalEvents = 100;

            var continuationHit = new bool[totalEvents];
            var lastException = new Exception[totalEvents];
            var continuationThread = new Thread[totalEvents];

            CreateContinuationFunc createAsyncContinuation =
                eventNumber =>
                    ex =>
                    {
                        lastException[eventNumber] = ex;
                        continuationThread[eventNumber] = Thread.CurrentThread;
                        continuationHit[eventNumber] = true;
                    };

            using (new NoThrowNLogExceptions())
            {
                // write 9 events - they will all be buffered and no final continuation will be reached
                var eventCounter = 0;
                for (var i = 0; i < 9; ++i)
                {
                    targetWrapper.WriteAsyncLogEvent(new LogEventInfo().WithContinuation(createAsyncContinuation(eventCounter++)));
                }

                Assert.Equal(0, myTarget.WriteCount);

                // write one more event - everything will be flushed
                targetWrapper.WriteAsyncLogEvent(new LogEventInfo().WithContinuation(createAsyncContinuation(eventCounter++)));
                Assert.Equal(1, myTarget.WriteCount);
                Assert.Equal(10, myTarget2.WriteCount);

                targetWrapper.Close();
                myTarget.Close();
            }
        }

        [Fact]
        public void BufferingTargetWrapperSyncWithTimedFlushTest()
        {
            var myTarget = new MyTarget();
            var targetWrapper = new BufferingTargetWrapper
            {
                WrappedTarget = myTarget,
                BufferSize = 10,
                FlushTimeout = 50,
            };
            var writeHit = new ManualResetEvent(false);

            InitializeTargets(myTarget, targetWrapper);

            const int totalEvents = 100;

            var continuationHit = new bool[totalEvents];
            var lastException = new Exception[totalEvents];
            var continuationThread = new Thread[totalEvents];
            var hitCount = 0;

            CreateContinuationFunc createAsyncContinuation =
                eventNumber =>
                    ex =>
                    {
                        lastException[eventNumber] = ex;
                        continuationThread[eventNumber] = Thread.CurrentThread;
                        continuationHit[eventNumber] = true;
                        Interlocked.Increment(ref hitCount);
                        writeHit.Set();
                    };

            // write 9 events - they will all be buffered and no final continuation will be reached
            var eventCounter = 0;
            for (var i = 0; i < 9; ++i)
            {
                targetWrapper.WriteAsyncLogEvent(
                    new LogEventInfo().WithContinuation(createAsyncContinuation(eventCounter++)));
            }

            Assert.Equal(0, hitCount);
            Assert.Equal(0, myTarget.WriteCount);

            // sleep and wait for the trigger timer to flush all events
            Assert.True(writeHit.WaitOne(5000), "Wait Write Timeout");
            WaitAndAssertExpectedValue(ref hitCount, 9);

            Assert.Equal(1, myTarget.BufferedWriteCount);
            Assert.Equal(9, myTarget.BufferedTotalEvents);
            Assert.Equal(9, myTarget.WriteCount);
            for (var i = 0; i < hitCount; ++i)
            {
                Assert.NotSame(Thread.CurrentThread, continuationThread[i]);
                Assert.Null(lastException[i]);
            }

            // write 11 more events, 10 will be hit immediately because the buffer will fill up
            // 1 will be pending
            for (var i = 0; i < 11; ++i)
            {
                targetWrapper.WriteAsyncLogEvent(
                    new LogEventInfo().WithContinuation(createAsyncContinuation(eventCounter++)));
            }

            Assert.Equal(19, hitCount);
            Assert.Equal(2, myTarget.BufferedWriteCount);
            Assert.Equal(19, myTarget.BufferedTotalEvents);
            Assert.Equal(19, myTarget.WriteCount);

            // sleep and wait for the remaining one to be flushed
            WaitAndAssertExpectedValue(ref hitCount, 20);
            Assert.Equal(3, myTarget.BufferedWriteCount);
            Assert.Equal(20, myTarget.BufferedTotalEvents);
            Assert.Equal(20, myTarget.WriteCount);
        }

        [Fact]
        public void BufferingTargetWrapperAsyncTest1()
        {
            RetryingIntegrationTest(3, () =>
            {
                var myTarget = new MyAsyncTarget();
                var targetWrapper = new BufferingTargetWrapper
                {
                    WrappedTarget = myTarget,
                    BufferSize = 10,
                };
                var writeHit = new ManualResetEvent(false);

                InitializeTargets(myTarget, targetWrapper);

                const int totalEvents = 100;

                var continuationHit = new bool[totalEvents];
                var lastException = new Exception[totalEvents];
                var continuationThread = new Thread[totalEvents];
                var hitCount = 0;

                CreateContinuationFunc createAsyncContinuation =
                    eventNumber =>
                        ex =>
                        {
                            lastException[eventNumber] = ex;
                            continuationThread[eventNumber] = Thread.CurrentThread;
                            continuationHit[eventNumber] = true;
                            Interlocked.Increment(ref hitCount);
                            writeHit.Set();
                        };

                // write 9 events - they will all be buffered and no final continuation will be reached
                var eventCounter = 0;
                for (var i = 0; i < 9; ++i)
                {
                    targetWrapper.WriteAsyncLogEvent(
                        new LogEventInfo().WithContinuation(createAsyncContinuation(eventCounter++)));
                }

                Assert.Equal(0, hitCount);

                // write one more event - everything will be flushed
                targetWrapper.WriteAsyncLogEvent(
                    new LogEventInfo().WithContinuation(createAsyncContinuation(eventCounter++)));

                Assert.True(writeHit.WaitOne(5000), "Wait Write Timeout");
                WaitAndAssertExpectedValue(ref hitCount, 10);
                Assert.Equal(1, myTarget.BufferedWriteCount);
                Assert.Equal(10, myTarget.BufferedTotalEvents);
                for (var i = 0; i < hitCount; ++i)
                {
                    Assert.NotSame(Thread.CurrentThread, continuationThread[i]);
                    Assert.Null(lastException[i]);
                }

                // write 9 more events - they will all be buffered and no final continuation will be reached
                for (var i = 0; i < 9; ++i)
                {
                    targetWrapper.WriteAsyncLogEvent(
                        new LogEventInfo().WithContinuation(createAsyncContinuation(eventCounter++)));
                }

                // no change
                Assert.Equal(10, hitCount);
                Assert.Equal(1, myTarget.BufferedWriteCount);
                Assert.Equal(10, myTarget.BufferedTotalEvents);

                Exception flushException = null;
                var flushHit = new ManualResetEvent(false);

                targetWrapper.Flush(
                    ex =>
                    {
                        flushException = ex;
                        flushHit.Set();
                    });

                Assert.True(flushHit.WaitOne(5000), "Wait Flush Timeout");
                Assert.Null(flushException);

                // make sure remaining events were written
                Assert.Equal(19, hitCount);
                Assert.Equal(2, myTarget.BufferedWriteCount);
                Assert.Equal(19, myTarget.BufferedTotalEvents);

                // flushes happen on another thread
                for (var i = 10; i < hitCount; ++i)
                {
                    Assert.NotNull(continuationThread[i]);
                    Assert.NotSame(Thread.CurrentThread, continuationThread[i]);
                    Assert.Null(lastException[i]);
                }

                // flush again - should not do anything
                flushHit.Reset();
                targetWrapper.Flush(
                    ex =>
                    {
                        flushException = ex;
                        flushHit.Set();
                    });

                Assert.True(flushHit.WaitOne(5000), "Wait Again Flush Timeout");
                Assert.Equal(19, hitCount);
                Assert.Equal(2, myTarget.BufferedWriteCount);
                Assert.Equal(19, myTarget.BufferedTotalEvents);

                targetWrapper.Close();
                myTarget.Close();
            });
        }

        [Fact]
        public void BufferingTargetWrapperSyncWithTimedFlushNonSlidingTest()
        {
            RetryingIntegrationTest(3, () =>
            {

                var myTarget = new MyTarget();
                var targetWrapper = new BufferingTargetWrapper
                {
                    WrappedTarget = myTarget,
                    BufferSize = 10,
                    FlushTimeout = 400,
                    SlidingTimeout = false,
                };

                InitializeTargets(myTarget, targetWrapper);

                const int totalEvents = 100;

                var continuationHit = new bool[totalEvents];
                var lastException = new Exception[totalEvents];
                var continuationThread = new Thread[totalEvents];
                var hitCount = 0;

                var resetEvent = new ManualResetEvent(false);
                CreateContinuationFunc createAsyncContinuation =
                    eventNumber =>
                        ex =>
                        {
                            lastException[eventNumber] = ex;
                            continuationThread[eventNumber] = Thread.CurrentThread;
                            continuationHit[eventNumber] = true;
                            Interlocked.Increment(ref hitCount);
                            if (eventNumber > 0)
                            {
                                resetEvent.Set();
                            }
                        };

                var eventCounter = 0;
                targetWrapper.WriteAsyncLogEvent(
                    new LogEventInfo().WithContinuation(createAsyncContinuation(eventCounter++)));

                Assert.Equal(0, hitCount);
                Assert.Equal(0, myTarget.WriteCount);

                targetWrapper.WriteAsyncLogEvent(
                    new LogEventInfo().WithContinuation(createAsyncContinuation(eventCounter++)));
                Assert.True(resetEvent.WaitOne(5000), "Wait Write Timeout");

                Assert.Equal(2, hitCount);
                Assert.Equal(2, myTarget.WriteCount);

            });
        }

        [Fact]
        public void BufferingTargetWrapperSyncWithTimedFlushSlidingTest()
        {
            var myTarget = new MyTarget();
            var targetWrapper = new BufferingTargetWrapper
            {
                WrappedTarget = myTarget,
                BufferSize = 10,
                FlushTimeout = 400,
            };
            var writeEvent = new ManualResetEvent(false);

            InitializeTargets(myTarget, targetWrapper);

            const int totalEvents = 100;

            var continuationHit = new bool[totalEvents];
            var lastException = new Exception[totalEvents];
            var continuationThread = new Thread[totalEvents];
            var hitCount = 0;

            CreateContinuationFunc createAsyncContinuation =
                eventNumber =>
                    ex =>
                    {
                        lastException[eventNumber] = ex;
                        continuationThread[eventNumber] = Thread.CurrentThread;
                        continuationHit[eventNumber] = true;
                        Interlocked.Increment(ref hitCount);
                        writeEvent.Set();
                    };

            var eventCounter = 0;
            targetWrapper.WriteAsyncLogEvent(new LogEventInfo().WithContinuation(createAsyncContinuation(eventCounter++)));
            Thread.Sleep(100);

            Assert.Equal(0, hitCount);
            Assert.Equal(0, myTarget.WriteCount);

            targetWrapper.WriteAsyncLogEvent(new LogEventInfo().WithContinuation(createAsyncContinuation(eventCounter++)));
            Thread.Sleep(100);

            Assert.Equal(0, hitCount);
            Assert.Equal(0, myTarget.WriteCount);

            Assert.True(writeEvent.WaitOne(5000), "Wait Write Timeout");
            WaitAndAssertExpectedValue(ref hitCount, 2);
            Assert.Equal(2, myTarget.WriteCount);
        }

        [Fact]
        public void WhenWrappedTargetThrowsExceptionThisIsHandled()
        {
            using (new NoThrowNLogExceptions())
            {
                var myTarget = new MyTarget { ThrowException = true };
                var bufferingTargetWrapper = new BufferingTargetWrapper
                {
                    WrappedTarget = myTarget,
                    FlushTimeout = -1
                };

                InitializeTargets(myTarget, bufferingTargetWrapper);
                bufferingTargetWrapper.WriteAsyncLogEvent(new LogEventInfo().WithContinuation(_ => { }));

                var flushHit = new ManualResetEvent(false);
                bufferingTargetWrapper.Flush(ex => flushHit.Set());
                Assert.True(flushHit.WaitOne(5000), "Wait Flush Timeout");
                Assert.Equal(1, myTarget.FlushCount);
            }
        }

        [Fact]
        public void BufferingTargetWrapperSyncWithOverflowDiscardTest()
        {
            const int totalEvents = 15;
            const int bufferSize = 10;

            var myTarget = new MyTarget();
            var targetWrapper = new BufferingTargetWrapper
            {
                WrappedTarget = myTarget,
                BufferSize = bufferSize,
                OverflowAction = BufferingTargetWrapperOverflowAction.Discard
            };

            InitializeTargets(myTarget, targetWrapper);

            var continuationHit = new bool[totalEvents];
            var hitCount = 0;
            CreateContinuationFunc createAsyncContinuation =
                eventNumber =>
                    ex =>
                    {
                        continuationHit[eventNumber] = true;
                        Interlocked.Increment(ref hitCount);
                    };

            Assert.Equal(0, myTarget.WriteCount);

            for (int i = 0; i < totalEvents; i++)
            {
                targetWrapper.WriteAsyncLogEvent(new LogEventInfo().WithContinuation(createAsyncContinuation(i)));
            }

            // No events should be written to the wrapped target unless flushing manually.
            Assert.Equal(0, myTarget.WriteCount);
            Assert.Equal(0, myTarget.BufferedWriteCount);
            Assert.Equal(0, myTarget.BufferedTotalEvents);

            targetWrapper.Flush(e => { });
            Assert.Equal(bufferSize, hitCount);
            Assert.Equal(bufferSize, myTarget.WriteCount);
            Assert.Equal(1, myTarget.BufferedWriteCount);
            Assert.Equal(bufferSize, myTarget.BufferedTotalEvents);

            // Validate that we dropped the oldest events.
            Assert.False(continuationHit[totalEvents - bufferSize - 1]);
            Assert.True(continuationHit[totalEvents - bufferSize]);

            // Make sure the events do not stay in the buffer.
            targetWrapper.Flush(e => { });
            Assert.Equal(bufferSize, hitCount);
            Assert.Equal(bufferSize, myTarget.WriteCount);
            Assert.Equal(1, myTarget.BufferedWriteCount);
            Assert.Equal(bufferSize, myTarget.BufferedTotalEvents);

            // Make sure that events are discarded when closing target (config-reload + shutdown)
            targetWrapper.WriteAsyncLogEvent(new LogEventInfo().WithContinuation(createAsyncContinuation(totalEvents)));
            targetWrapper.Close();
            Assert.Equal(bufferSize, hitCount);
            Assert.Equal(bufferSize, myTarget.WriteCount);
            Assert.Equal(1, myTarget.BufferedWriteCount);
            Assert.Equal(bufferSize, myTarget.BufferedTotalEvents);
        }

        private static void InitializeTargets(params Target[] targets)
        {
            foreach (var target in targets)
            {
                target.Initialize(null);
            }
        }

        private class MyAsyncTarget : Target
        {
            private readonly NLog.Internal.AsyncOperationCounter _pendingWriteCounter = new NLog.Internal.AsyncOperationCounter();

            public int BufferedWriteCount { get; private set; }
            public int BufferedTotalEvents { get; private set; }

            protected override void Write(LogEventInfo logEvent)
            {
                throw new NotSupportedException();
            }

            protected override void Write(IList<AsyncLogEventInfo> logEvents)
            {
                _pendingWriteCounter.BeginOperation();

                BufferedWriteCount++;
                BufferedTotalEvents += logEvents.Count;

                for (int i = 0; i < logEvents.Count; ++i)
                {
                    var @event = logEvents[i];
                    ThreadPool.QueueUserWorkItem(
                        s =>
                        {
                            try
                            {
                                if (ThrowExceptions)
                                {
                                    @event.Continuation(new ApplicationException("Some problem!"));
                                }
                                else
                                {
                                    @event.Continuation(null);
                                }
                            }
                            finally
                            {
                                _pendingWriteCounter.CompleteOperation(null);
                            }
                        });
                }
            }

            protected override void FlushAsync(AsyncContinuation asyncContinuation)
            {
                var wrappedContinuation = _pendingWriteCounter.RegisterCompletionNotification(asyncContinuation);
                ThreadPool.QueueUserWorkItem(
                    s =>
                    {
                        wrappedContinuation(null);
                    });
            }

            public bool ThrowExceptions { get; set; }
        }

        private class MyTarget : Target
        {
            public int FlushCount { get; private set; }
            public int WriteCount { get; private set; }
            public int BufferedWriteCount { get; private set; }
            public int BufferedTotalEvents { get; private set; }
            public bool ThrowException { get; set; }
            public int FailCounter { get; set; }

            protected override void Write(IList<AsyncLogEventInfo> logEvents)
            {
                BufferedWriteCount++;
                BufferedTotalEvents += logEvents.Count;
                base.Write(logEvents);
            }

            protected override void Write(LogEventInfo logEvent)
            {
                Assert.True(FlushCount <= WriteCount);
                WriteCount++;
                if (ThrowException)
                {
                    throw new Exception("Target exception");
                }

                if (FailCounter > 0)
                {
                    FailCounter--;
                    throw new ApplicationException("Some failure.");
                }
            }

            protected override void FlushAsync(AsyncContinuation asyncContinuation)
            {
                FlushCount++;
                asyncContinuation(null);
            }
        }

        private delegate AsyncContinuation CreateContinuationFunc(int eventNumber);

        private static void WaitAndAssertExpectedValue(ref int hitCount, int expectedValue)
        {
            for (int i = 0; i < 100; ++i)
            {
                if (Thread.VolatileRead(ref hitCount) >= expectedValue)
                    break;  // Ready to assert

                Thread.Sleep(50);
            }
            Assert.Equal(expectedValue, hitCount);
        }
    }
}
