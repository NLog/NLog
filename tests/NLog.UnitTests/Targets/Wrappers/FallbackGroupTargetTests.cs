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

        [Fact]
        public void FirstTwoTargetsFails_Write_ThirdTargetWritesAllEvents()
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

        [Fact]
        public void ReturnToFirstOnSuccessAndSecondTargetSucceeds_Write_ReturnToFirstTargetOnSuccess()
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

        [Fact]
        public void FallbackGroupTargetSyncTest5()
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

        [Fact]
        public void FallbackGroupTargetSyncTest6()
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
                    Assert.IsType(typeof(InvalidOperationException), exceptions[i]);
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

        private static FallbackGroupTarget CreateAndInitializeFallbackGroupTarget(bool returnToFirstOnSuccess, params Target[] targets)
        {
            var wrapper = new FallbackGroupTarget(targets)
                              {
                                  ReturnToFirstOnSuccess = returnToFirstOnSuccess,
                              };

            foreach (var target in targets)
            {
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

        private class MyTarget : Target
        {
            public int FlushCount { get; set; }
            public int WriteCount { get; set; }
            public int FailCounter { get; set; }

            protected override void Write(LogEventInfo logEvent)
            {
                Assert.True(this.FlushCount <= this.WriteCount);
                this.WriteCount++;

                if (this.FailCounter > 0)
                {
                    this.FailCounter--;
                    throw new InvalidOperationException("Some failure.");
                }
            }

            protected override void FlushAsync(AsyncContinuation asyncContinuation)
            {
                this.FlushCount++;
                asyncContinuation(null);
            }
        }
    }
}
