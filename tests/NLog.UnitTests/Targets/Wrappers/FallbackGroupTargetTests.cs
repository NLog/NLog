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

    public class FallbackGroupTargetTests : NLogTestBase
	{
        [Fact]
        public void FirstTargetWorks_Write_AllEventsAreWrittenToFirstTarget()
        {
            var myTarget1 = new MyTarget();
            var myTarget2 = new MyTarget();
            var myTarget3 = new MyTarget();

            var wrapper = CreateAndInitializeFallbackGroupTarget(false, myTarget1, myTarget2, myTarget3);

            WriteAndAssertNoExceptions(wrapper);

            Assert.Equal(10, myTarget1.WriteCount);
            Assert.Equal(0, myTarget2.WriteCount);
            Assert.Equal(0, myTarget3.WriteCount);

            AssertNoFlushException(wrapper);
        }

        [Fact]
        public void FirstTargetFails_Write_SecondTargetWritesAllEvents()
        {
            using (new NoThrowNLogExceptions())
            {
                var myTarget1 = new MyTarget { FailCounter = 1 };
                var myTarget2 = new MyTarget();
                var myTarget3 = new MyTarget();

                var wrapper = CreateAndInitializeFallbackGroupTarget(false, myTarget1, myTarget2, myTarget3);

                WriteAndAssertNoExceptions(wrapper);

                Assert.Equal(1, myTarget1.WriteCount);
                Assert.Equal(10, myTarget2.WriteCount);
                Assert.Equal(0, myTarget3.WriteCount);

                AssertNoFlushException(wrapper);
            }
        }

        [Fact]
        public void FirstTwoTargetsFails_Write_ThirdTargetWritesAllEvents()
        {
            using (new NoThrowNLogExceptions())
            {
                var myTarget1 = new MyTarget { FailCounter = 1 };
                var myTarget2 = new MyTarget { FailCounter = 1 };
                var myTarget3 = new MyTarget();

                var wrapper = CreateAndInitializeFallbackGroupTarget(false, myTarget1, myTarget2, myTarget3);

                WriteAndAssertNoExceptions(wrapper);

                Assert.Equal(1, myTarget1.WriteCount);
                Assert.Equal(1, myTarget2.WriteCount);
                Assert.Equal(10, myTarget3.WriteCount);

                AssertNoFlushException(wrapper);
            }
        }

        [Fact]
        public void ReturnToFirstOnSuccessAndSecondTargetSucceeds_Write_ReturnToFirstTargetOnSuccess()
        {
            using (new NoThrowNLogExceptions())
            {
                var myTarget1 = new MyTarget { FailCounter = 1 };
                var myTarget2 = new MyTarget();
                var myTarget3 = new MyTarget();

                var wrapper = CreateAndInitializeFallbackGroupTarget(true, myTarget1, myTarget2, myTarget3);

                WriteAndAssertNoExceptions(wrapper);

                Assert.Equal(10, myTarget1.WriteCount);
                Assert.Equal(1, myTarget2.WriteCount);
                Assert.Equal(0, myTarget3.WriteCount);

                AssertNoFlushException(wrapper);
            }
        }

        [Fact]
        public void FallbackGroupTargetSyncTest5()
        {
            using (new NoThrowNLogExceptions())
            {
                // fail once
                var myTarget1 = new MyTarget { FailCounter = 3 };
                var myTarget2 = new MyTarget { FailCounter = 3 };
                var myTarget3 = new MyTarget { FailCounter = 3 };

                var wrapper = CreateAndInitializeFallbackGroupTarget(true, myTarget1, myTarget2, myTarget3);

                var exceptions = new List<Exception>();

                // no exceptions
                for (var i = 0; i < 10; ++i)
                {
                    wrapper.WriteAsyncLogEvent(LogEventInfo.CreateNullEvent().WithContinuation(exceptions.Add));
                }

                Assert.Equal(10, exceptions.Count);
                for (var i = 0; i < 10; ++i)
                {
                    if (i < 3)
                    {
                        Assert.NotNull(exceptions[i]);
                    }
                    else
                    {
                        Assert.Null(exceptions[i]);
                    }
                }

                Assert.Equal(10, myTarget1.WriteCount);
                Assert.Equal(3, myTarget2.WriteCount);
                Assert.Equal(3, myTarget3.WriteCount);

                AssertNoFlushException(wrapper);
            }
        }

        [Fact]
        public void FallbackGroupTargetSyncTest6()
        {
            using (new NoThrowNLogExceptions())
            {
                // fail once
                var myTarget1 = new MyTarget { FailCounter = 10 };
                var myTarget2 = new MyTarget { FailCounter = 3 };
                var myTarget3 = new MyTarget { FailCounter = 3 };

                var wrapper = CreateAndInitializeFallbackGroupTarget(true, myTarget1, myTarget2, myTarget3);

                var exceptions = new List<Exception>();

                // no exceptions
                for (var i = 0; i < 10; ++i)
                {
                    wrapper.WriteAsyncLogEvent(LogEventInfo.CreateNullEvent().WithContinuation(exceptions.Add));
                }

                Assert.Equal(10, exceptions.Count);
                for (var i = 0; i < 10; ++i)
                {
                    if (i < 3)
                    {
                        // for the first 3 rounds, no target is available
                        Assert.NotNull(exceptions[i]);
                        Assert.IsType<ApplicationException>(exceptions[i]);
                        Assert.Equal("Some failure.", exceptions[i].Message);
                    }
                    else
                    {
                        Assert.Null(exceptions[i]);
                    }
                }

                Assert.Equal(10, myTarget1.WriteCount);
                Assert.Equal(10, myTarget2.WriteCount);
                Assert.Equal(3, myTarget3.WriteCount);

                AssertNoFlushException(wrapper);

                Assert.Equal(1, myTarget1.FlushCount);
                Assert.Equal(1, myTarget2.FlushCount);
                Assert.Equal(1, myTarget3.FlushCount);
            }
        }

        [Fact]
        public void FallbackGroupWithBufferingTargets_ReturnToFirstOnSuccess()
        {
            FallbackGroupWithBufferingTargets(true);
        }

        [Fact]
        public void FallbackGroupWithBufferingTargets_DoNotReturnToFirstOnSuccess()
        {
            FallbackGroupWithBufferingTargets(false);
        }

        private void FallbackGroupWithBufferingTargets(bool returnToFirstOnSuccess)
        {
            using (new NoThrowNLogExceptions())
            {
                const int totalEvents = 1000;

                var myTarget1 = new MyTarget { FailCounter = int.MaxValue }; // Always failing.
                var myTarget2 = new MyTarget();
                var myTarget3 = new MyTarget();

                var buffer1 = new BufferingTargetWrapper() { WrappedTarget = myTarget1, FlushTimeout = 100, SlidingTimeout = false };
                var buffer2 = new BufferingTargetWrapper() { WrappedTarget = myTarget2, FlushTimeout = 100, SlidingTimeout = false };
                var buffer3 = new BufferingTargetWrapper() { WrappedTarget = myTarget3, FlushTimeout = 100, SlidingTimeout = false };

                var wrapper = CreateAndInitializeFallbackGroupTarget(returnToFirstOnSuccess, buffer1, buffer2, buffer3);

                var allEventsDone = new ManualResetEvent(false);
                var exceptions = new List<Exception>();
                for (var i = 0; i < totalEvents; ++i)
                {
                    wrapper.WriteAsyncLogEvent(LogEventInfo.CreateNullEvent().WithContinuation(ex =>
                    {
                        lock (exceptions)
                        {
                            exceptions.Add(ex);
                            if (exceptions.Count >= totalEvents)
                                allEventsDone.Set();
                        }
                    }));
                }
                allEventsDone.WaitOne();                            // Wait for all events to be delivered.

                Assert.True(totalEvents >= myTarget1.WriteCount,    // Check events weren't delivered twice to myTarget1,
                    "Target 1 received " + myTarget1.WriteCount + " writes although only " + totalEvents + " events were written");
                Assert.Equal(totalEvents, myTarget2.WriteCount);    // were all delivered exactly once to myTarget2,
                Assert.Equal(0, myTarget3.WriteCount);              // with nothing delivered to myTarget3.

                Assert.Equal(totalEvents, exceptions.Count);
                foreach (var e in exceptions)
                {
                    Assert.Null(e);                                 // All events should have succeeded on myTarget2.
                }
            }
        }

        [Fact]
        public void FallbackGroupTargetAsyncTest()
        {
            using (new NoThrowNLogExceptions())
            {
                var myTarget1 = new MyTarget { FailCounter = int.MaxValue }; // Always failing.
                var myTarget1Async = new AsyncTargetWrapper(myTarget1) { TimeToSleepBetweenBatches = 0 }; // Always failing.
                var myTarget2 = new MyTarget() { Layout = "${ndlc}" };

                var wrapper = CreateAndInitializeFallbackGroupTarget(true, myTarget1Async, myTarget2);

                var exceptions = new List<Exception>();

                // no exceptions
                for (var i = 0; i < 10; ++i)
                {
                    using (ScopeContext.PushNestedState("Hello World"))
                    {
                        wrapper.WriteAsyncLogEvent(LogEventInfo.CreateNullEvent().WithContinuation(exceptions.Add));
                    }
                }

                ManualResetEvent resetEvent = new ManualResetEvent(false);
                myTarget1Async.Flush((ex) => { Assert.Null(ex); resetEvent.Set(); });
                resetEvent.WaitOne(1000);

                Assert.Equal(10, exceptions.Count);
                for (var i = 0; i < 10; ++i)
                {
                    Assert.Null(exceptions[i]);
                }

                Assert.Equal(10, myTarget2.WriteCount);

                AssertNoFlushException(wrapper);
            }
        }

        private static FallbackGroupTarget CreateAndInitializeFallbackGroupTarget(bool returnToFirstOnSuccess, params Target[] targets)
        {
            var wrapper = new FallbackGroupTarget(targets)
                              {
                                  ReturnToFirstOnSuccess = returnToFirstOnSuccess,
                              };

            foreach (var target in targets)
            {
                WrapperTargetBase wrapperTarget = target as WrapperTargetBase;
                if (wrapperTarget != null)
                    wrapperTarget.WrappedTarget.Initialize(null);

                target.Initialize(null);
            }

            wrapper.Initialize(null);

            return wrapper;
        }

        private static void WriteAndAssertNoExceptions(FallbackGroupTarget wrapper)
        {
            var exceptions = new List<Exception>();
            for (var i = 0; i < 10; ++i)
            {
                wrapper.WriteAsyncLogEvent(LogEventInfo.CreateNullEvent().WithContinuation(exceptions.Add));
            }

            Assert.Equal(10, exceptions.Count);
            foreach (var e in exceptions)
            {
                Assert.Null(e);
            }
        }

        private static void AssertNoFlushException(FallbackGroupTarget wrapper)
        {
            Exception flushException = null;
            var flushHit = new ManualResetEvent(false);
            wrapper.Flush(ex =>
                              {
                                  flushException = ex;
                                  flushHit.Set();
                              });

            flushHit.WaitOne();
            if (flushException != null)
                Assert.True(false, flushException.ToString());
        }

        private class MyTarget : TargetWithLayout
        {
            public int FlushCount { get; set; }
            public int WriteCount { get; set; }
            public int FailCounter { get; set; }

            protected override void Write(LogEventInfo logEvent)
            {
                if (Layout != null && string.IsNullOrEmpty(Layout.Render(logEvent)))
                {
                    throw new ApplicationException("Empty LogEvent.");
                }

                Assert.True(FlushCount <= WriteCount);
                WriteCount++;

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
    }
}
