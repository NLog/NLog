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
    using System.Threading;
    using NLog.Common;
    using NLog.Targets;
    using NLog.Targets.Wrappers;
    using System.Collections.Generic;
    using Xunit;
    using NLog.Config;

    public class AsyncTargetWrapperTests : NLogTestBase
    {
        [Fact]
        public void AsyncTargetWrapperInitTest()
        {
            var myTarget = new MyTarget();
            var targetWrapper = new AsyncTargetWrapper(myTarget, 300, AsyncTargetWrapperOverflowAction.Grow);
            Assert.Equal(AsyncTargetWrapperOverflowAction.Grow, targetWrapper.OverflowAction);
            Assert.Equal(300, targetWrapper.QueueLimit);
            Assert.Equal(1, targetWrapper.TimeToSleepBetweenBatches);
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
            Assert.Equal(1, targetWrapper.TimeToSleepBetweenBatches);
            Assert.Equal(200, targetWrapper.BatchSize);
        }

        [Fact]
        public void AsyncTargetWrapperSyncTest_WithLock_WhenTimeToSleepBetweenBatchesIsEqualToZero()
        {
            AsyncTargetWrapperSyncTest_WhenTimeToSleepBetweenBatchesIsEqualToZero(true);
        }

        [Fact]
        public void AsyncTargetWrapperSyncTest_NoLock_WhenTimeToSleepBetweenBatchesIsEqualToZero()
        {
            AsyncTargetWrapperSyncTest_WhenTimeToSleepBetweenBatchesIsEqualToZero(false);
        }

        /// <summary>
        /// Test Fix for https://github.com/NLog/NLog/issues/1069
        /// </summary>
        private void AsyncTargetWrapperSyncTest_WhenTimeToSleepBetweenBatchesIsEqualToZero(bool forceLockingQueue)
        {
            LogManager.ThrowConfigExceptions = true;

            var myTarget = new MyTarget();
            var targetWrapper = new AsyncTargetWrapper() {
                WrappedTarget = myTarget,
                TimeToSleepBetweenBatches = 0,
#if !NET35 && !NET40
                ForceLockingQueue = forceLockingQueue,
#endif
                BatchSize = 3,
                QueueLimit = 5, // Will make it "sleep" between every second write
                FullBatchSizeWriteLimit = 1,
                OverflowAction = AsyncTargetWrapperOverflowAction.Block
            };
            targetWrapper.Initialize(null);
            myTarget.Initialize(null);

            try
            {
                int flushCounter = 0;
                AsyncContinuation flushHandler = (ex) => { ++flushCounter; };

                var itemPrepareList = new List<AsyncLogEventInfo>(500);
                var itemWrittenList = new List<int>(itemPrepareList.Capacity);
                for (int i = 0; i < itemPrepareList.Capacity; ++i)
                {
                    var logEvent = new LogEventInfo();
                    int sequenceID = logEvent.SequenceID;
                    bool blockConsumer = (itemPrepareList.Capacity / 2) == i;  // Force producers to get into blocking-mode
                    itemPrepareList.Add(logEvent.WithContinuation((ex) => { if (blockConsumer) Thread.Sleep(125); itemWrittenList.Add(sequenceID); }));
                }

                var eventProducer0 = new ManualResetEvent(false);
                var eventProducer1 = new ManualResetEvent(false);
                ParameterizedThreadStart producerMethod = (s) =>
                {
                    var eventProducer = (ManualResetEvent)s;
                    if (eventProducer != null)
                        eventProducer.Set();    // Signal we are ready

                    int partitionNo = ReferenceEquals(eventProducer, eventProducer1) ? 1 : 0;
                    for (int i = 0; i < itemPrepareList.Count; ++i)
                    {
                        if (i % 2 == partitionNo)
                            targetWrapper.WriteAsyncLogEvent(itemPrepareList[i]);
                    }
                };

                Thread producer0 = new Thread(producerMethod);
                producer0.IsBackground = true;
                Thread producer1 = new Thread(producerMethod);
                producer1.IsBackground = true;
                producer1.Start(eventProducer0);
                producer0.Start(eventProducer1);
                Assert.True(eventProducer0.WaitOne(5000), "Producer0 Start Timeout");
                Assert.True(eventProducer1.WaitOne(5000), "Producer1 Start Timeout");

                long startTicks = Environment.TickCount;

                Assert.True(producer0.Join(5000), "Producer0 Complete Timeout");  // Wait for producer0 to complete
                Assert.True(producer1.Join(5000), "Producer1 Complete Timeout");  // Wait for producer1 to complete

                long elapsedMilliseconds = Environment.TickCount - startTicks;

                targetWrapper.Flush(flushHandler);

                for (int i = 0; i < itemPrepareList.Count * 2 && itemWrittenList.Count != itemPrepareList.Count; ++i)
                    Thread.Sleep(1);

                Assert.Equal(itemPrepareList.Count, itemWrittenList.Count);

                int producer0sequenceID = 0;
                int producer1sequenceID = 0;
                for (int i = 1; i < itemWrittenList.Count; ++i)
                {
                    if (itemWrittenList[i] % 2 == 0)
                    {
                        Assert.True(producer0sequenceID < itemWrittenList[i], "Producer0 invalid sequence");
                        producer0sequenceID = itemWrittenList[i];
                    }
                    else
                    {
                        Assert.True(producer1sequenceID < itemWrittenList[i], "Producer1 invalid sequence");
                        producer1sequenceID = itemWrittenList[i];
                    }
                }

#if DEBUG
                if (!IsAppVeyor())  // Skip timing test when running within OpenCover.Console.exe
#endif
                    Assert.InRange(elapsedMilliseconds, 0, 975);

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
                Assert.True(continuationHit.WaitOne(5000));
                Assert.NotSame(continuationThread, Thread.CurrentThread);
                Assert.Null(lastException);
                Assert.Equal(1, myTarget.WriteCount);

                continuationHit.Reset();
                targetWrapper.WriteAsyncLogEvent(logEvent.WithContinuation(continuation));
                Assert.True(continuationHit.WaitOne(5000));
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

                Assert.True(continuationHit.WaitOne(5000));
                Assert.Null(lastException);
                Assert.Equal(1, myTarget.WriteCount);

                continuationHit.Reset();
                targetWrapper.WriteAsyncLogEvent(logEvent.WithContinuation(continuation));
                Assert.True(continuationHit.WaitOne(5000));
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

                Assert.True(continuationHit.WaitOne(5000));
                Assert.NotNull(lastException);
                Assert.IsType<InvalidOperationException>(lastException);

                // no flush on exception
                Assert.Equal(0, myTarget.FlushCount);
                Assert.Equal(1, myTarget.WriteCount);

                continuationHit.Reset();
                lastException = null;
                targetWrapper.WriteAsyncLogEvent(logEvent.WithContinuation(continuation));
                Assert.True(continuationHit.WaitOne(5000));
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
            RetryingIntegrationTest(3, () =>
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
                            Assert.True(mre.WaitOne(5000), InternalLogger.LogWriter?.ToString() ?? string.Empty);
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
            });
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

            using (new NoThrowNLogExceptions())
            {
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

                Assert.True(firstContinuationResetEvent.WaitOne(5000), nameof(firstContinuationResetEvent));
                Assert.True(secondContinuationResetEvent.WaitOne(5000), nameof(secondContinuationResetEvent));
                Assert.True(firstContinuationCalled);
                Assert.True(secondContinuationCalled);
            }
            finally
            {
                asyncTarget.Close();
            }
        }

        [Fact]
        public void LogEventDropped_OnRequestqueueOverflow()
        {
            int queueLimit = 2;
            int loggedEventCount = 5;
            int eventsCounter = 0;
            var myTarget = new MyTarget();

            var targetWrapper = new AsyncTargetWrapper()
            {
                WrappedTarget = myTarget,
                QueueLimit = queueLimit,
                TimeToSleepBetweenBatches = 500,    // Make it slow
                OverflowAction = AsyncTargetWrapperOverflowAction.Discard,
            };

            var logFactory = new LogFactory();
            var loggingConfig = new NLog.Config.LoggingConfiguration(logFactory);
            loggingConfig.AddRuleForAllLevels(targetWrapper);
            logFactory.Configuration = loggingConfig;
            var logger = logFactory.GetLogger("Test");

            try
            {
                targetWrapper.LogEventDropped += (o, e) => { eventsCounter++; };

                for (int i = 0; i < loggedEventCount; i++)
                {
                    logger.Info("Hello");
                }

                Assert.Equal(loggedEventCount - queueLimit, eventsCounter);
            }
            finally
            {
                logFactory.Configuration = null;
            }
        }

        [Fact]
        public void LogEventNotDropped_IfOverflowActionBlock()
        {
            int queueLimit = 2;
            int loggedEventCount = 5;
            int eventsCounter = 0;
            var myTarget = new MyTarget();

            var targetWrapper = new AsyncTargetWrapper()
            {
                WrappedTarget = myTarget,
                QueueLimit = queueLimit,
                OverflowAction = AsyncTargetWrapperOverflowAction.Block
            };

            var logFactory = new LogFactory();
            var loggingConfig = new NLog.Config.LoggingConfiguration(logFactory);
            loggingConfig.AddRuleForAllLevels(targetWrapper);
            logFactory.Configuration = loggingConfig;
            var logger = logFactory.GetLogger("Test");

            try
            {
                targetWrapper.LogEventDropped += (o, e) => { eventsCounter++; };

                for (int i = 0; i < loggedEventCount; i++)
                {
                    logger.Info("Hello");
                }
                
                Assert.Equal(0, eventsCounter);
            }
            finally
            {
                logFactory.Configuration = null;
            }
        }

        [Fact]
        public void LogEventNotDropped_IfOverflowActionGrow()
        {
            int queueLimit = 2;
            int loggedEventCount = 5;
            int eventsCounter = 0;
            var myTarget = new MyTarget();

            var targetWrapper = new AsyncTargetWrapper()
            {
                WrappedTarget = myTarget,
                QueueLimit = queueLimit,
                OverflowAction = AsyncTargetWrapperOverflowAction.Grow
            };

            var logFactory = new LogFactory();
            var loggingConfig = new NLog.Config.LoggingConfiguration(logFactory);
            loggingConfig.AddRuleForAllLevels(targetWrapper);
            logFactory.Configuration = loggingConfig;
            var logger = logFactory.GetLogger("Test");

            try
            {
                targetWrapper.LogEventDropped += (o, e) => { eventsCounter++; };

                for (int i = 0; i < loggedEventCount; i++)
                {
                    logger.Info("Hello");
                }
                
                Assert.Equal(0, eventsCounter);
            }
            finally
            {
                logFactory.Configuration = null;
            }
        }

        [Fact]
        public void EventQueueGrow_OnQueueGrow()
        {
            int queueLimit = 2;
            int loggedEventCount = 10;

            int expectedGrowingNumber = 3;

            int eventsCounter = 0;
            var myTarget = new MyTarget();

            var targetWrapper = new AsyncTargetWrapper()
            {
                WrappedTarget = myTarget,
                QueueLimit = queueLimit,
                TimeToSleepBetweenBatches = 500,    // Make it slow
                OverflowAction = AsyncTargetWrapperOverflowAction.Grow,
            };

            var logFactory = new LogFactory();
            var loggingConfig = new NLog.Config.LoggingConfiguration(logFactory);
            loggingConfig.AddRuleForAllLevels(targetWrapper);
            logFactory.Configuration = loggingConfig;
            var logger = logFactory.GetLogger("Test");

            try
            {
                targetWrapper.EventQueueGrow += (o, e) => { eventsCounter++; };

                for (int i = 0; i < loggedEventCount; i++)
                {
                    logger.Info("Hello");
                }
                
                Assert.Equal(expectedGrowingNumber, eventsCounter);
            }
            finally
            {
                logFactory.Configuration = null;
            }
        }

        [Fact]
        public void EnqueuQueueBlock_WithLock_OnClose_ReleasesWriters()
        {
            EnqueuQueueBlock_OnClose_ReleasesWriters(true);
        }

        [Fact]
        public void EnqueuQueueBlock_NoLock_OnClose_ReleasesWriters()
        {
            EnqueuQueueBlock_OnClose_ReleasesWriters(false);
        }

        private void EnqueuQueueBlock_OnClose_ReleasesWriters(bool forceLockingQueue)
        {
            // Arrange
            var slowTarget = new MethodCallTarget("slowTarget", (logEvent, parms) => System.Threading.Thread.Sleep(300));
            var targetWrapper = new AsyncTargetWrapper("asynSlowTarget", slowTarget)
            {
                OverflowAction = AsyncTargetWrapperOverflowAction.Block,
                QueueLimit = 3,
                ForceLockingQueue = forceLockingQueue,
            };

            var logFactory = new LogFactory();
            var loggingConfig = new NLog.Config.LoggingConfiguration(logFactory);
            loggingConfig.AddRuleForAllLevels(targetWrapper);
            logFactory.Configuration = loggingConfig;
            var logger = logFactory.GetLogger("Test");

            // Act
            long allTasksCompleted = 0;
            AsyncHelpers.ForEachItemInParallel(System.Linq.Enumerable.Range(1, 6), (ex) => Interlocked.Exchange(ref allTasksCompleted, 1), (value, cont) => { for (int i = 0; i < 100; ++i) logger.Info("Hello {0}", value); cont(null); });
            Thread.Sleep(150); // Let them get stuck
            Assert.Equal(0, Interlocked.Read(ref allTasksCompleted));

            targetWrapper.Close();  // Release those who are stuck, and discard the rest

            // Assert
            for (int i = 0; i < 100; i++)
            {
                if (Interlocked.Read(ref allTasksCompleted) == 1)
                    break;
                Thread.Sleep(10);
            }

            Assert.Equal(1, Interlocked.Read(ref allTasksCompleted));
        }

        [Fact]
        public void AsyncTargetWrapper_MissingDependency_EnqueueLogEvents()
        {
            using (new NoThrowNLogExceptions())
            {
                // Arrange
                var logFactory = new LogFactory();
                logFactory.ThrowConfigExceptions = true;
                var logConfig = new LoggingConfiguration(logFactory);
                var asyncTarget = new MyTarget() { Name = "asynctarget", RequiredDependency = typeof(IMisingDependencyClass) };
                logConfig.AddRuleForAllLevels(new AsyncTargetWrapper("wrapper", asyncTarget));
                logFactory.Configuration = logConfig;
                var logger = logFactory.GetLogger(nameof(AsyncTargetWrapper_MissingDependency_EnqueueLogEvents));

                // Act
                logger.Info("Hello World");
                Assert.False(asyncTarget.WaitForWriteEvent(50));
                logFactory.ServiceRepository.RegisterService(typeof(IMisingDependencyClass), new MisingDependencyClass());

                // Assert
                Assert.True(asyncTarget.WaitForWriteEvent());
            }
        }

        private interface IMisingDependencyClass
        {

        }

        private class MisingDependencyClass : IMisingDependencyClass
        {

        }

        private class MyAsyncTarget : Target
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

        private class MyTarget : Target
        {
            private readonly AutoResetEvent _writeEvent = new AutoResetEvent(false);

            public int FlushCount { get; set; }
            public int WriteCount { get; set; }

            public Type RequiredDependency { get; set; }

            public bool WaitForWriteEvent(int timeoutMilliseconds = 1000)
            {
                if (_writeEvent.WaitOne(TimeSpan.FromMilliseconds(timeoutMilliseconds)))
                {
                    Thread.Sleep(25);
                    return true;
                }
                return false;
            }

            protected override void InitializeTarget()
            {
                base.InitializeTarget();

                if (RequiredDependency != null)
                {
                    try
                    {
                        var resolveServiceMethod = typeof(Target).GetMethod(nameof(ResolveService), System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                        resolveServiceMethod = resolveServiceMethod.MakeGenericMethod(new[] { RequiredDependency });
                        resolveServiceMethod.Invoke(this, NLog.Internal.ArrayHelper.Empty<object>());
                    }
                    catch (System.Reflection.TargetInvocationException ex)
                    {
                        throw ex.InnerException;
                    }
                }
            }

            protected override void Write(LogEventInfo logEvent)
            {
                Assert.True(FlushCount <= WriteCount);
                WriteCount++;
                _writeEvent.Set();
            }

            protected override void FlushAsync(AsyncContinuation asyncContinuation)
            {
                FlushCount++;
                asyncContinuation(null);
            }
        }
    }
}
