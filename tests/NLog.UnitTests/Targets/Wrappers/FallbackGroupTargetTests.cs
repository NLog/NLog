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
    using System.Collections.Generic;
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
    public class FallbackGroupTargetTests : NLogTestBase
	{
        [Test]
        public void FirstTargetWorks_Write_AllEventsAreWrittenToFirstTarget()
        {
            var myTarget1 = new MyTarget();
            var myTarget2 = new MyTarget();
            var myTarget3 = new MyTarget();

            var wrapper = CreateAndInitializeFallbackGroupTarget(false, myTarget1, myTarget2, myTarget3);

            WriteAndAssertNoExceptions(wrapper);

            Assert.AreEqual(10, myTarget1.WriteCount);
            Assert.AreEqual(0, myTarget2.WriteCount);
            Assert.AreEqual(0, myTarget3.WriteCount);

            AssertNoFlushException(wrapper);
        }

        [Test]
        public void FirstTargetFails_Write_SecondTargetWritesAllEvents()
        {
            var myTarget1 = new MyTarget { FailCounter = 1 };
            var myTarget2 = new MyTarget();
            var myTarget3 = new MyTarget();

            var wrapper = CreateAndInitializeFallbackGroupTarget(false, myTarget1, myTarget2, myTarget3);

            WriteAndAssertNoExceptions(wrapper);

            Assert.AreEqual(1, myTarget1.WriteCount);
            Assert.AreEqual(10, myTarget2.WriteCount);
            Assert.AreEqual(0, myTarget3.WriteCount);

            AssertNoFlushException(wrapper);
        }

        [Test]
        public void FirstTwoTargetsFails_Write_ThirdTargetWritesAllEvents()
        {
            var myTarget1 = new MyTarget { FailCounter = 1 };
            var myTarget2 = new MyTarget { FailCounter = 1 };
            var myTarget3 = new MyTarget();

            var wrapper = CreateAndInitializeFallbackGroupTarget(false, myTarget1, myTarget2, myTarget3);

            WriteAndAssertNoExceptions(wrapper);

            Assert.AreEqual(1, myTarget1.WriteCount);
            Assert.AreEqual(1, myTarget2.WriteCount);
            Assert.AreEqual(10, myTarget3.WriteCount);

            AssertNoFlushException(wrapper);
        }

        [Test]
        public void ReturnToFirstOnSuccessAndSecondTargetSucceeds_Write_ReturnToFirstTargetOnSuccess()
        {
            var myTarget1 = new MyTarget { FailCounter = 1 };
            var myTarget2 = new MyTarget();
            var myTarget3 = new MyTarget();

            var wrapper = CreateAndInitializeFallbackGroupTarget(true, myTarget1, myTarget2, myTarget3);

            WriteAndAssertNoExceptions(wrapper);

            Assert.AreEqual(10, myTarget1.WriteCount);
            Assert.AreEqual(1, myTarget2.WriteCount);
            Assert.AreEqual(0, myTarget3.WriteCount);

            AssertNoFlushException(wrapper);
        }

        [Test]
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

            Assert.AreEqual(10, exceptions.Count);
            for (var i = 0; i < 10; ++i)
            {
                if (i < 3)
                {
                    Assert.IsNotNull(exceptions[i]);
                }
                else
                {
                    Assert.IsNull(exceptions[i]);
                }
            }

            Assert.AreEqual(10, myTarget1.WriteCount);
            Assert.AreEqual(3, myTarget2.WriteCount);
            Assert.AreEqual(3, myTarget3.WriteCount);

            AssertNoFlushException(wrapper);
        }

        [Test]
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

            Assert.AreEqual(10, exceptions.Count);
            for (var i = 0; i < 10; ++i)
            {
                if (i < 3)
                {
                    // for the first 3 rounds, no target is available
                    Assert.IsNotNull(exceptions[i]);
                    Assert.IsInstanceOfType(typeof(InvalidOperationException), exceptions[i]);
                    Assert.AreEqual("Some failure.", exceptions[i].Message);
                }
                else
                {
                    Assert.IsNull(exceptions[i], Convert.ToString(exceptions[i]));
                }
            }

            Assert.AreEqual(10, myTarget1.WriteCount);
            Assert.AreEqual(10, myTarget2.WriteCount);
            Assert.AreEqual(3, myTarget3.WriteCount);

            AssertNoFlushException(wrapper);

            Assert.AreEqual(1, myTarget1.FlushCount);
            Assert.AreEqual(1, myTarget2.FlushCount);
            Assert.AreEqual(1, myTarget3.FlushCount);
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

            Assert.AreEqual(10, exceptions.Count);
            foreach (var e in exceptions)
            {
                Assert.IsNull(e);
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
                Assert.Fail(flushException.ToString());
        }

        private class MyTarget : Target
        {
            public int FlushCount { get; set; }
            public int WriteCount { get; set; }
            public int FailCounter { get; set; }

            protected override void Write(LogEventInfo logEvent)
            {
                Assert.IsTrue(this.FlushCount <= this.WriteCount);
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
