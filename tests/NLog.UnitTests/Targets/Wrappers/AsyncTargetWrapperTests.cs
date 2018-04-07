// 
// Copyright (c) 2004-2018 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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
    using NLog.Common;
    using NLog.Targets;
    using NLog.Targets.Wrappers;
    using System.Collections.Generic;
    using Xunit;

    public class AsyncTargetWrapperTests : NLogTestBase
    {
        [Fact]
        public void AsyncTargetWrapperInitTest()
        {
            var myTarget = new MyTarget();
            var targetWrapper = new AsyncTargetWrapper(myTarget, 300, AsyncTargetWrapperOverflowAction.Grow);
            Assert.Equal(AsyncTargetWrapperOverflowAction.Grow, targetWrapper.OverflowAction);
            Assert.Equal(300, targetWrapper.QueueLimit);
            Assert.Equal(50, targetWrapper.TimeToSleepBetweenBatches);
            Assert.Equal(200, targetWrapper.BatchSize);
        }

        [Fact]
        public void AsyncTargetWrapperInitTest2()
        {
            var myTarget = new MyTarget();
            var targetWrapper = new AsyncTargetWrapper()
            {
                WrappedTarget = myTarget,
            };

            Assert.Equal(AsyncTargetWrapperOverflowAction.Discard, targetWrapper.OverflowAction);
            Assert.Equal(10000, targetWrapper.QueueLimit);
            Assert.Equal(50, targetWrapper.TimeToSleepBetweenBatches);
            Assert.Equal(200, targetWrapper.BatchSize);
        }

        /// <summary>
        /// Test Fix for https://github.com/NLog/NLog/issues/1069
        /// </summary>
        [Fact]
        public void AsyncTargetWrapperSyncTest_WhenTimeToSleepBetweenBatchesIsEqualToZero()
        {
            LogManager.ThrowConfigExceptions = true;

            var myTarget = new MyTarget();
            var targetWrapper = new AsyncTargetWrapper() {
                WrappedTarget = myTarget,
                TimeToSleepBetweenBatches = 0,
                BatchSize = 4,
                QueueLimit = 2, // Will make it "sleep" between every second write
                OverflowAction = AsyncTargetWrapperOverflowAction.Block
            };
            targetWrapper.Initialize(null);
            myTarget.Initialize(null);

            try
            {
                int flushCounter = 0;
                AsyncContinuation flushHandler = (ex) => { ++flushCounter; };

                List<KeyValuePair<LogEventInfo, AsyncContinuation>> itemPrepareList = new List<KeyValuePair<LogEventInfo, AsyncContinuation>>(500);
                List<int> itemWrittenList = new List<int>(itemPrepareList.Capacity);
                for (int i = 0; i< itemPrepareList.Capacity; ++i)
                {
                    var logEvent = new LogEventInfo();
                    int sequenceID = logEvent.SequenceID;
                    itemPrepareList.Add(new KeyValuePair<LogEventInfo, AsyncContinuation>(logEvent, (ex) => itemWrittenList.Add(sequenceID)));
                }

                long startTicks = Environment.TickCount;
                for (int i = 0; i < itemPrepareList.Count; ++i)
                {
                    var logEvent = itemPrepareList[i].Key;
                    targetWrapper.WriteAsyncLogEvent(logEvent.WithContinuation(itemPrepareList[i].Value));
                }

                targetWrapper.Flush(flushHandler);

                for (int i = 0; i < itemPrepareList.Count * 2 && itemWrittenList.Count != itemPrepareList.Count; ++i)
                    Thread.Sleep(1);

                long elapsedMilliseconds = Environment.TickCount - startTicks;

                Assert.Equal(itemPrepareList.Count, itemWrittenList.Count);
                int prevSequenceID = 0;
                for (int i = 0; i < itemWrittenList.Count; ++i)
                {
                    Assert.True(prevSequenceID < itemWrittenList[i]);
                    prevSequenceID = itemWrittenList[i];
                }

#if NET4_5
                if (!IsAppVeyor())  // Skip timing test when running within OpenCover.Console.exe
#endif
                    Assert.True(elapsedMilliseconds < 950);

                targetWrapper.Flush(flushHandler);
                for (int i = 0; i < 2000 && flushCounter != 2; ++i)
                    Thread.Sleep(1);
                Assert.Equal(2, flushCounter);
            }
            finally
            {
                myTarget.Close();
                targetWrapper.Close();
            }
        }

        [Fact]
        public void AsyncTargetWrapperSyncTest1()
        {
            var myTarget = new MyTarget();
            var targetWrapper = new AsyncTargetWrapper
            {
                WrappedTarget = myTarget,
                Name = "AsyncTargetWrapperSyncTest1_Wrapper",
            };
            targetWrapper.Initialize(null);
            myTarget.Initialize(null);

            try
            {
                var logEvent = new LogEventInfo();
                Exception lastException = null;
                ManualResetEvent continuationHit = new ManualResetEvent(false);
                Thread continuationThread = null;
                AsyncContinuation continuation =
                    ex =>
                        {
                            lastException = ex;
                            continuationThread = Thread.CurrentThread;
                            continuationHit.Set();
                        };

                targetWrapper.WriteAsyncLogEvent(logEvent.WithContinuation(continuation));

                // continuation was not hit 
                Assert.True(continuationHit.WaitOne(2000));
                Assert.NotSame(continuationThread, Thread.CurrentThread);
                Assert.Null(lastException);
                Assert.Equal(1, myTarget.WriteCount);

                continuationHit.Reset();
                targetWrapper.WriteAsyncLogEvent(logEvent.WithContinuation(continuation));
                continuationHit.WaitOne();
                Assert.NotSame(continuationThread, Thread.CurrentThread);
                Assert.Null(lastException);
                Assert.Equal(2, myTarget.WriteCount);
            }
            finally
            {
                myTarget.Close();
                targetWrapper.Close();
            }
        }

        [Fact]
        public void AsyncTargetWrapperAsyncTest1()
        {
            var myTarget = new MyAsyncTarget();
            var targetWrapper = new AsyncTargetWrapper(myTarget) { Name = "AsyncTargetWrapperAsyncTest1_Wrapper" };
            targetWrapper.Initialize(null);
            myTarget.Initialize(null);
            try
            {
                var logEvent = new LogEventInfo();
                Exception lastException = null;
                var continuationHit = new ManualResetEvent(false);
                AsyncContinuation continuation =
                    ex =>
                    {
                        lastException = ex;
                        continuationHit.Set();
                    };

                targetWrapper.WriteAsyncLogEvent(logEvent.WithContinuation(continuation));

                Assert.True(continuationHit.WaitOne());
                Assert.Null(lastException);
                Assert.Equal(1, myTarget.WriteCount);

                continuationHit.Reset();
                targetWrapper.WriteAsyncLogEvent(logEvent.WithContinuation(continuation));
                continuationHit.WaitOne();
                Assert.Null(lastException);
                Assert.Equal(2, myTarget.WriteCount);
            }
            finally
            {
                myTarget.Close();
                targetWrapper.Close();
            }
        }

        [Fact]
        public void AsyncTargetWrapperAsyncWithExceptionTest1()
        {
            var myTarget = new MyAsyncTarget
            {
                ThrowExceptions = true,
       
            };

            var targetWrapper = new AsyncTargetWrapper(myTarget) {Name = "AsyncTargetWrapperAsyncWithExceptionTest1_Wrapper"};
            targetWrapper.Initialize(null);
            myTarget.Initialize(null);
            try
            {
                var logEvent = new LogEventInfo();
                Exception lastException = null;
                var continuationHit = new ManualResetEvent(false);
                AsyncContinuation continuation =
                    ex =>
                    {
                        lastException = ex;
                        continuationHit.Set();
                    };

                targetWrapper.WriteAsyncLogEvent(logEvent.WithContinuation(continuation));

                Assert.True(continuationHit.WaitOne());
                Assert.NotNull(lastException);
                Assert.IsType<InvalidOperationException>(lastException);

                // no flush on exception
                Assert.Equal(0, myTarget.FlushCount);
                Assert.Equal(1, myTarget.WriteCount);

                continuationHit.Reset();
                lastException = null;
                targetWrapper.WriteAsyncLogEvent(logEvent.WithContinuation(continuation));
                continuationHit.WaitOne();
                Assert.NotNull(lastException);
                Assert.IsType<InvalidOperationException>(lastException);
                Assert.Equal(0, myTarget.FlushCount);
                Assert.Equal(2, myTarget.WriteCount);
            }
            finally
            {
                myTarget.Close();
                targetWrapper.Close();
            }
        }

        [Fact]
        public void AsyncTargetWrapperFlushTest()
        {
            var myTarget = new MyAsyncTarget
            {
                ThrowExceptions = true
            };

            var targetWrapper = new AsyncTargetWrapper(myTarget)
            {
                Name = "AsyncTargetWrapperFlushTest_Wrapper",
                OverflowAction = AsyncTargetWrapperOverflowAction.Grow
            };

            targetWrapper.Initialize(null);
            myTarget.Initialize(null);

            try
            {
                List<Exception> exceptions = new List<Exception>();

                int eventCount = 5000;

                for (int i = 0; i < eventCount; ++i)
                {
                    targetWrapper.WriteAsyncLogEvent(LogEventInfo.CreateNullEvent().WithContinuation(
                        ex =>
                        {
                            lock (exceptions)
                            {
                                exceptions.Add(ex);
                            }
                        }));
                }

                Exception lastException = null;
                ManualResetEvent mre = new ManualResetEvent(false);

                string internalLog = RunAndCaptureInternalLog(
                    () =>
                    {
                        targetWrapper.Flush(
                            cont =>
                            {
                                try
                                {
                                    // by this time all continuations should be completed
                                    Assert.Equal(eventCount, exceptions.Count);

                                    // with just 1 flush of the target
                                    Assert.Equal(1, myTarget.FlushCount);

                                    // and all writes should be accounted for
                                    Assert.Equal(eventCount, myTarget.WriteCount);
                                }
                                catch (Exception ex)
                                {
                                    lastException = ex;
                                }
                                finally
                                {
                                    mre.Set();
                                }
                            });
                        Assert.True(mre.WaitOne());
                    },
                    LogLevel.Trace);

                if (lastException != null)
                {
                    Assert.True(false, lastException.ToString() + "\r\n" + internalLog);
                }
            }
            finally
            {
                myTarget.Close();
                targetWrapper.Close();
            }
        }

        [Fact]
        public void AsyncTargetWrapperCloseTest()
        {
            var myTarget = new MyAsyncTarget
            {
                ThrowExceptions = true
            };

            var targetWrapper = new AsyncTargetWrapper(myTarget)
            {
                OverflowAction = AsyncTargetWrapperOverflowAction.Grow,
                TimeToSleepBetweenBatches = 1000,
                Name = "AsyncTargetWrapperCloseTest_Wrapper",
            };

            targetWrapper.Initialize(null);
            myTarget.Initialize(null);

            targetWrapper.WriteAsyncLogEvent(LogEventInfo.CreateNullEvent().WithContinuation(ex => { }));

            // quickly close the target before the timer elapses
            targetWrapper.Close();
        }

        [Fact]
        public void AsyncTargetWrapperExceptionTest()
        {
            var targetWrapper = new AsyncTargetWrapper
            {
                OverflowAction = AsyncTargetWrapperOverflowAction.Grow,
                TimeToSleepBetweenBatches = 500,
                WrappedTarget = new DebugTarget(),
                Name = "AsyncTargetWrapperExceptionTest_Wrapper"
            };

            LogManager.ThrowExceptions = false;

            targetWrapper.Initialize(null);

            // null out wrapped target - will cause exception on the timer thread
            targetWrapper.WrappedTarget = null;

            string internalLog = RunAndCaptureInternalLog(
                () =>
                {
                    targetWrapper.WriteAsyncLogEvent(LogEventInfo.CreateNullEvent().WithContinuation(ex => { }));
                    targetWrapper.Close();
                },
                LogLevel.Trace);

            Assert.True(internalLog.Contains("WrappedTarget is NULL"), internalLog);
        }

        [Fact]
        public void FlushingMultipleTimesSimultaneous()
        {
            var asyncTarget = new AsyncTargetWrapper
            {
                TimeToSleepBetweenBatches = 1000,
                WrappedTarget = new DebugTarget(),
                Name = "FlushingMultipleTimesSimultaneous_Wrapper"
            };
            asyncTarget.Initialize(null);

            try
            {
                asyncTarget.WriteAsyncLogEvent(LogEventInfo.CreateNullEvent().WithContinuation(ex => { }));

                var firstContinuationCalled = false;
                var secondContinuationCalled = false;
                var firstContinuationResetEvent = new ManualResetEvent(false);
                var secondContinuationResetEvent = new ManualResetEvent(false);
                asyncTarget.Flush(ex =>
                {
                    firstContinuationCalled = true;
                    firstContinuationResetEvent.Set();
                });
                asyncTarget.Flush(ex =>
                {
                    secondContinuationCalled = true;
                    secondContinuationResetEvent.Set();
                });

                firstContinuationResetEvent.WaitOne();
                secondContinuationResetEvent.WaitOne();
                Assert.True(firstContinuationCalled);
                Assert.True(secondContinuationCalled);
            }
            finally
            {
                asyncTarget.Close();
            }
        }

        class MyAsyncTarget : Target
        {
            private readonly NLog.Internal.AsyncOperationCounter pendingWriteCounter = new NLog.Internal.AsyncOperationCounter();

            public int FlushCount;
            public int WriteCount;

            protected override void Write(LogEventInfo logEvent)
            {
                throw new NotSupportedException();
            }

            protected override void Write(AsyncLogEventInfo logEvent)
            {
                Assert.True(FlushCount <= WriteCount);

                pendingWriteCounter.BeginOperation();
                ThreadPool.QueueUserWorkItem(
                    s =>
                        {
                            try
                            {
                                Interlocked.Increment(ref WriteCount);
                                if (ThrowExceptions)
                                {
                                    logEvent.Continuation(new InvalidOperationException("Some problem!"));
                                    logEvent.Continuation(new InvalidOperationException("Some problem!"));
                                }
                                else
                                {
                                    logEvent.Continuation(null);
                                    logEvent.Continuation(null);
                                }
                            }
                            finally
                            {
                                pendingWriteCounter.CompleteOperation(null);
                            }
                        });
            }

            protected override void FlushAsync(AsyncContinuation asyncContinuation)
            {
                Interlocked.Increment(ref FlushCount);
                var wrappedContinuation = pendingWriteCounter.RegisterCompletionNotification(asyncContinuation);
                ThreadPool.QueueUserWorkItem(
                    s =>
                    {
                        wrappedContinuation(null);
                    });
            }

            public bool ThrowExceptions { get; set; }
        }

        class MyTarget : Target
        {
            public int FlushCount { get; set; }
            public int WriteCount { get; set; }

            protected override void Write(LogEventInfo logEvent)
            {
                Assert.True(FlushCount <= WriteCount);
                WriteCount++;
            }

            protected override void FlushAsync(AsyncContinuation asyncContinuation)
            {
                FlushCount++;
                asyncContinuation(null);
            }
        }
    }
}
