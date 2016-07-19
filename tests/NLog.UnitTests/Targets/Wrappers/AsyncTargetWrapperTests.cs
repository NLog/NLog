// 
// Copyright (c) 2004-2016 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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
    using System.Diagnostics;

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
            Assert.Equal(100, targetWrapper.BatchSize);
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
            Assert.Equal(100, targetWrapper.BatchSize);
        }

        /// <summary>
        /// Test for https://github.com/NLog/NLog/issues/1069
        /// </summary>
        [Fact]
        public void AsyncTargetWrapperInitTest_WhenTimeToSleepBetweenBatchesIsEqualToZero_ShouldThrowNLogConfigurationException() {
            LogManager.ThrowConfigExceptions = true;

            var myTarget = new MyTarget();
            var targetWrapper = new AsyncTargetWrapper() {
                WrappedTarget = myTarget,
                TimeToSleepBetweenBatches = 0,
            };
            Assert.Throws<NLogConfigurationException>(() => targetWrapper.Initialize(null));
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
                Assert.IsType(typeof(InvalidOperationException), lastException);

                // no flush on exception
                Assert.Equal(0, myTarget.FlushCount);
                Assert.Equal(1, myTarget.WriteCount);

                continuationHit.Reset();
                lastException = null;
                targetWrapper.WriteAsyncLogEvent(logEvent.WithContinuation(continuation));
                continuationHit.WaitOne();
                Assert.NotNull(lastException);
                Assert.IsType(typeof(InvalidOperationException), lastException);
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
        public void AsyncTargetWrapperSingleTest()
        {
            InternalLogger.LogToConsole = true;
            InternalLogger.IncludeTimestamp = true;
            InternalLogger.LogLevel = LogLevel.Trace;
            var myTarget = new MyAsyncTarget
            {
                ThrowExceptions = true
            };


            var targetWrapper = new AsyncTargetWrapper(myTarget)
            {
                Name = "AsyncTargetWrapperFlushTest_Wrapper",
                OverflowAction = AsyncTargetWrapperOverflowAction.Grow,
                TimeToSleepBetweenBatches = 3
            };

            targetWrapper.Initialize(null);
            myTarget.Initialize(null);

            try
            {
                List<Exception> exceptions = new List<Exception>();
                long missingEvents = 1;


                targetWrapper.WriteAsyncLogEvent(LogEventInfo.CreateNullEvent().WithContinuation(
                    ex =>
                    {
                        try
                        {
                            lock (exceptions)
                            {
                                exceptions.Add(ex);
                            }
                            Interlocked.Decrement(ref missingEvents);
                        }
                        catch (Exception e)
                        {
                            InternalLogger.Trace("Error in callback", e);
                        }
                    }));
               

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
                                DateTime start = DateTime.Now;

                                    // We have to spin until all events are done being written by the above code, otherwise on
                                    // slow computers the flush will be called before all events has been pushed to the event queue.
                                    // causing the below assertions to fail.
                                    if (missingEvents > 0)
                                {
                                    InternalLogger.Trace("Still missing {0} events, exceptions captured:{1}", missingEvents, exceptions.Count);
                                }
                                while (missingEvents > 0)
                                {
                                    InternalLogger.Trace("Still missing {0} events, exceptions captured:{1}", missingEvents, exceptions.Count);
                                    Thread.Sleep(50);
                                    if (DateTime.Now - start > TimeSpan.FromSeconds(2000))
                                    {
                                        Assert.False(true, string.Format("threads did not manage to enqueue their messages within time limit, still missing:{0}events, exceptions captured:{1}", missingEvents, exceptions.Count));
                                    }
                                }

                                    // by this time all continuations should be completed
                                    Assert.Equal(1, exceptions.Count);

                                    // We have to use interlocked, otherwise there are no guarantee that we get the correct value
                                    // with  just 1 flush of the target
                                    int flushCount = Interlocked.CompareExchange(ref myTarget.FlushCount, 0, 1);

                                Assert.Equal(1, flushCount);

                                int writeCount = Interlocked.CompareExchange(ref myTarget.WriteCount, 0, 1);
                                    // and all writes should be accounted for
                                    Assert.Equal(1, writeCount);
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
        public void AsyncTargetWrapperFlushTest()
        {
            InternalLogger.LogToConsole = true;
            InternalLogger.IncludeTimestamp = true;
            InternalLogger.LogLevel = LogLevel.Trace;
            var myTarget = new MyAsyncTarget
            {
                ThrowExceptions = true
            };
            

            var targetWrapper = new AsyncTargetWrapper(myTarget)
            {
                Name = "AsyncTargetWrapperFlushTest_Wrapper",
                OverflowAction = AsyncTargetWrapperOverflowAction.Grow,
                TimeToSleepBetweenBatches = 3
            };

            targetWrapper.Initialize(null);
            myTarget.Initialize(null);

            try
            {
                List<Exception> exceptions = new List<Exception>();
#if !SILVERLIGHT
                int eventCount = Environment.Is64BitProcess ? 5000 : 500;
                long missingEvents = Environment.Is64BitProcess ? 5000 : 500;
#else
                int eventCount = 500;
                long missingEvents = 500;
#endif
                for (int i = 0; i < eventCount; ++i)
                {
                    targetWrapper.WriteAsyncLogEvent(LogEventInfo.CreateNullEvent().WithContinuation(
                        ex =>
                            {
                                try
                                {
                                    lock (exceptions)
                                    {
                                        exceptions.Add(ex);
                                    }
                                    Interlocked.Decrement(ref missingEvents);
                                }
                                catch (Exception e)
                                {
                                    InternalLogger.Trace("Error in callback",e);
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
                                    DateTime start = DateTime.Now;

                                    // We have to spin until all events are done being written by the above code, otherwise on
                                    // slow computers the flush will be called before all events has been pushed to the event queue.
                                    // causing the below assertions to fail.
                                    if (missingEvents > 0)
                                    {
                                        InternalLogger.Trace("Still missing {0} events, exceptions captured:{1}", missingEvents, exceptions.Count);
                                    }
                                    while (missingEvents > 0)
                                    {
                                        InternalLogger.Trace("Still missing {0} events, exceptions captured:{1}", missingEvents, exceptions.Count);
                                        Thread.Sleep(50);
                                        if (DateTime.Now - start > TimeSpan.FromSeconds(20))
                                        {
                                            Assert.False( true,string.Format("threads did not manage to enqueue their messages within time limit, still missing:{0}events, exceptions captured:{1}", missingEvents, exceptions.Count));
                                        }
                                    }

                                    // by this time all continuations should be completed
                                    Assert.Equal(eventCount, exceptions.Count);

                                    // We have to use interlocked, otherwise there are no guarantee that we get the correct value
                                    // with  just 1 flush of the target
                                    int flushCount = Interlocked.CompareExchange(ref myTarget.FlushCount, 0, 1);

                                    Assert.Equal(1, flushCount);

                                    int writeCount = Interlocked.CompareExchange(ref myTarget.WriteCount, 0, eventCount);
                                    // and all writes should be accounted for
                                    Assert.Equal(eventCount, writeCount);
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

            Assert.True(internalLog.Contains("AsyncWrapper 'AsyncTargetWrapperExceptionTest_Wrapper': WrappedTarget is NULL"), internalLog);
        }

        [Fact]
        public void FlushingMultipleTimesSimultaneous()
        {
            var asyncTarget = new AsyncTargetWrapper
            {
                TimeToSleepBetweenBatches = 2000,
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
            public int FlushCount;
            public int WriteCount;

            protected override void Write(LogEventInfo logEvent)
            {
                throw new NotSupportedException();
            }

            protected override void Write(AsyncLogEventInfo logEvent)
            {
                // This assertion is flawed.
                // If threads run slow, then AsyncTargetWrapper will flush multiple times.
                // We cannot expect FlushCount to be lower than WriteCount, since Flush run on a timer thread, whereas
                // Write run on a threadpool thread.
                //Assert.True(this.FlushCount <= this.WriteCount);
                Interlocked.Increment(ref this.WriteCount);
                if (this.WriteCount % 100 == 0)
                {
                    InternalLogger.Trace("{0} -  Writen 100", DateTime.UtcNow.ToString("yyyy-MM-dd hh:mm:ss.ffff"));
                }
            
                ThreadPool.QueueUserWorkItem(
                    s =>
                        {
                            try
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
                            }
                            catch (Exception e)
                            {
                                InternalLogger.Trace("Unexopected Exception", e);
                                logEvent.Continuation(e);
                            }
                        
                        });
            }

            protected override void FlushAsync(AsyncContinuation asyncContinuation)
            {
                Interlocked.Increment(ref this.FlushCount);
                ThreadPool.QueueUserWorkItem(
                                    s => asyncContinuation(null));
            }

            public bool ThrowExceptions { get; set; }
        }

        class MyTarget : Target
        {
            public int FlushCount { get; set; }
            public int WriteCount { get; set; }

            protected override void Write(LogEventInfo logEvent)
            {
                Assert.True(this.FlushCount <= this.WriteCount);
                this.WriteCount++;
            }

            protected override void FlushAsync(AsyncContinuation asyncContinuation)
            {
                this.FlushCount++;
                asyncContinuation(null);
            }
        }
    }
}
