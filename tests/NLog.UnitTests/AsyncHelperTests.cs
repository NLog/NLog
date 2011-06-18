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

namespace NLog.UnitTests
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

    [TestFixture]
    public class AsyncHelperTests : NLogTestBase
    {
        [Test]
        public void OneTimeOnlyTest1()
        {
            var exceptions = new List<Exception>();
            AsyncContinuation cont = exceptions.Add;
            cont = AsyncHelpers.PreventMultipleCalls(cont);

            // OneTimeOnly(OneTimeOnly(x)) == OneTimeOnly(x)
            var cont2 = AsyncHelpers.PreventMultipleCalls(cont);
#if NETCF2_0
            Assert.AreNotSame(cont, cont2);
#else
            Assert.AreSame(cont, cont2);
#endif

            var sampleException = new InvalidOperationException("some message");

            cont(null);
            cont(sampleException);
            cont(null);
            cont(sampleException);

            Assert.AreEqual(1, exceptions.Count);
            Assert.IsNull(exceptions[0]);
        }

        [Test]
        public void OneTimeOnlyTest2()
        {
            var exceptions = new List<Exception>();
            AsyncContinuation cont = exceptions.Add;
            cont = AsyncHelpers.PreventMultipleCalls(cont);

            var sampleException = new InvalidOperationException("some message");

            cont(sampleException);
            cont(null);
            cont(sampleException);
            cont(null);

            Assert.AreEqual(1, exceptions.Count);
            Assert.AreSame(sampleException, exceptions[0]);
        }

        [Test]
        public void OneTimeOnlyExceptionInHandlerTest()
        {
            var exceptions = new List<Exception>();
            var sampleException = new InvalidOperationException("some message");
            AsyncContinuation cont = ex => { exceptions.Add(ex); throw sampleException; };
            cont = AsyncHelpers.PreventMultipleCalls(cont);

            cont(null);
            cont(null);
            cont(null);

            Assert.AreEqual(1, exceptions.Count);
            Assert.IsNull(exceptions[0]);
        }

        [Test]
        public void ContinuationTimeoutTest()
        {
            var exceptions = new List<Exception>();

            // set up a timer to strike in 1 second
            var cont = AsyncHelpers.WithTimeout(ex => exceptions.Add(ex), TimeSpan.FromSeconds(1));

            // sleep 2 seconds to make sure
            Thread.Sleep(2000);

            // make sure we got timeout exception
            Assert.AreEqual(1, exceptions.Count);
            Assert.IsInstanceOfType(typeof(TimeoutException), exceptions[0]);
            Assert.AreEqual("Timeout.", exceptions[0].Message);

            // those will be ignored
            cont(null);
            cont(new InvalidOperationException("Some exception"));
            cont(null);
            cont(new InvalidOperationException("Some exception"));

            Assert.AreEqual(1, exceptions.Count);
        }

        [Test]
        public void ContinuationTimeoutNotHitTest()
        {
            var exceptions = new List<Exception>();

            // set up a timer to strike in 1 second
            var cont = AsyncHelpers.WithTimeout(AsyncHelpers.PreventMultipleCalls(exceptions.Add), TimeSpan.FromSeconds(1));

            // call success quickly, hopefully before the timer comes
            cont(null);

            // sleep 2 seconds to make sure timer event comes
            Thread.Sleep(2000);

            // make sure we got success, not a timer exception
            Assert.AreEqual(1, exceptions.Count);
            Assert.IsNull(exceptions[0]);

            // those will be ignored
            cont(null);
            cont(new InvalidOperationException("Some exception"));
            cont(null);
            cont(new InvalidOperationException("Some exception"));

            Assert.AreEqual(1, exceptions.Count);
            Assert.IsNull(exceptions[0]);
        }

        [Test]
        public void ContinuationErrorTimeoutNotHitTest()
        {
            var exceptions = new List<Exception>();

            // set up a timer to strike in 3 second
            var cont = AsyncHelpers.WithTimeout(AsyncHelpers.PreventMultipleCalls(exceptions.Add), TimeSpan.FromSeconds(1));

            var exception = new InvalidOperationException("Foo");
            // call success quickly, hopefully before the timer comes
            cont(exception);

            // sleep 2 seconds to make sure timer event comes
            Thread.Sleep(2000);

            // make sure we got success, not a timer exception
            Assert.AreEqual(1, exceptions.Count);
            Assert.IsNotNull(exceptions[0]);

            Assert.AreSame(exception, exceptions[0]);

            // those will be ignored
            cont(null);
            cont(new InvalidOperationException("Some exception"));
            cont(null);
            cont(new InvalidOperationException("Some exception"));

            Assert.AreEqual(1, exceptions.Count);
            Assert.IsNotNull(exceptions[0]);
        }

        [Test]
        public void RepeatTest1()
        {
            bool finalContinuationInvoked = false;
            Exception lastException = null;

            AsyncContinuation finalContinuation = ex =>
                {
                    finalContinuationInvoked = true;
                    lastException = ex;
                };

            int callCount = 0;

            AsyncHelpers.Repeat(10, finalContinuation, 
                cont =>
                    {
                        callCount++;
                        cont(null);
                    });

            Assert.IsTrue(finalContinuationInvoked);
            Assert.IsNull(lastException);
            Assert.AreEqual(10, callCount);
        }

        [Test]
        public void RepeatTest2()
        {
            bool finalContinuationInvoked = false;
            Exception lastException = null;
            Exception sampleException = new InvalidOperationException("Some message");

            AsyncContinuation finalContinuation = ex =>
            {
                finalContinuationInvoked = true;
                lastException = ex;
            };

            int callCount = 0;

            AsyncHelpers.Repeat(10, finalContinuation,
                cont =>
                {
                    callCount++;
                    cont(sampleException);
                    cont(sampleException);
                });

            Assert.IsTrue(finalContinuationInvoked);
            Assert.AreSame(sampleException, lastException);
            Assert.AreEqual(1, callCount);
        }

        [Test]
        public void RepeatTest3()
        {
            bool finalContinuationInvoked = false;
            Exception lastException = null;
            Exception sampleException = new InvalidOperationException("Some message");

            AsyncContinuation finalContinuation = ex =>
            {
                finalContinuationInvoked = true;
                lastException = ex;
            };

            int callCount = 0;

            AsyncHelpers.Repeat(10, finalContinuation,
                cont =>
                {
                    callCount++;
                    throw sampleException;
                });

            Assert.IsTrue(finalContinuationInvoked);
            Assert.AreSame(sampleException, lastException);
            Assert.AreEqual(1, callCount);
        }

        [Test]
        public void ForEachItemSequentiallyTest1()
        {
            bool finalContinuationInvoked = false;
            Exception lastException = null;

            AsyncContinuation finalContinuation = ex =>
            {
                finalContinuationInvoked = true;
                lastException = ex;
            };

            int sum = 0;
            var input = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, };

            AsyncHelpers.ForEachItemSequentially(input, finalContinuation,
                (i, cont) =>
                {
                    sum += i;
                    cont(null);
                    cont(null);
                });

            Assert.IsTrue(finalContinuationInvoked);
            Assert.IsNull(lastException);
            Assert.AreEqual(55, sum);
        }

        [Test]
        public void ForEachItemSequentiallyTest2()
        {
            bool finalContinuationInvoked = false;
            Exception lastException = null;
            Exception sampleException = new InvalidOperationException("Some message");

            AsyncContinuation finalContinuation = ex =>
            {
                finalContinuationInvoked = true;
                lastException = ex;
            };

            int sum = 0;
            var input = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, };

            AsyncHelpers.ForEachItemSequentially(input, finalContinuation,
                (i, cont) =>
                {
                    sum += i;
                    cont(sampleException);
                    cont(sampleException);
                });

            Assert.IsTrue(finalContinuationInvoked);
            Assert.AreSame(sampleException, lastException);
            Assert.AreEqual(1, sum);
        }

        [Test]
        public void ForEachItemSequentiallyTest3()
        {
            bool finalContinuationInvoked = false;
            Exception lastException = null;
            Exception sampleException = new InvalidOperationException("Some message");

            AsyncContinuation finalContinuation = ex =>
            {
                finalContinuationInvoked = true;
                lastException = ex;
            };

            int sum = 0;
            var input = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, };

            AsyncHelpers.ForEachItemSequentially(input, finalContinuation,
                (i, cont) =>
                {
                    sum += i;
                    throw sampleException;
                });

            Assert.IsTrue(finalContinuationInvoked);
            Assert.AreSame(sampleException, lastException);
            Assert.AreEqual(1, sum);
        }

        [Test]
        public void ForEachItemInParallelEmptyTest()
        {
            int[] items = new int[0];
            Exception lastException = null;
            bool finalContinuationInvoked = false;

            AsyncContinuation continuation = ex =>
                {
                    lastException = ex;
                    finalContinuationInvoked = true;
                };

            AsyncHelpers.ForEachItemInParallel(items, continuation, (i, cont) => { Assert.Fail("Will not be reached"); });
            Assert.IsTrue(finalContinuationInvoked);
            Assert.IsNull(lastException);
        }

        [Test]
        public void ForEachItemInParallelTest()
        {
            var finalContinuationInvoked = new ManualResetEvent(false);
            Exception lastException = null;

            AsyncContinuation finalContinuation = ex =>
            {
                lastException = ex;
                finalContinuationInvoked.Set();
            };

            int sum = 0;
            var input = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, };

            AsyncHelpers.ForEachItemInParallel(input, finalContinuation,
                (i, cont) =>
                {
                    lock (input)
                    {
                        sum += i;
                    }

                    cont(null);
                    cont(null);
                });

            finalContinuationInvoked.WaitOne();
            Assert.IsNull(lastException);
            Assert.AreEqual(55, sum);
        }

        [Test]
        public void ForEachItemInParallelSingleFailureTest()
        {
            using (new InternalLoggerScope())
            {
                InternalLogger.LogLevel = LogLevel.Trace;
                InternalLogger.LogToConsole = true;

                var finalContinuationInvoked = new ManualResetEvent(false);
                Exception lastException = null;

                AsyncContinuation finalContinuation = ex =>
                    {
                        lastException = ex;
                        finalContinuationInvoked.Set();
                    };

                int sum = 0;
                var input = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, };

                AsyncHelpers.ForEachItemInParallel(input, finalContinuation,
                    (i, cont) =>
                        {
                            Console.WriteLine("Callback on {0}", Thread.CurrentThread.ManagedThreadId);
                            lock (input)
                            {
                                sum += i;
                            }

                            if (i == 7)
                            {
                                throw new InvalidOperationException("Some failure.");
                            }

                            cont(null);
                        });

                finalContinuationInvoked.WaitOne();
                Assert.AreEqual(55, sum);
                Assert.IsNotNull(lastException);
                Assert.IsInstanceOfType(typeof(InvalidOperationException), lastException);
                Assert.AreEqual("Some failure.", lastException.Message);
            }
        }

        [Test]
        public void ForEachItemInParallelMultipleFailuresTest()
        {
            var finalContinuationInvoked = new ManualResetEvent(false);
            Exception lastException = null;

            AsyncContinuation finalContinuation = ex =>
            {
                lastException = ex;
                finalContinuationInvoked.Set();
            };

            int sum = 0;
            var input = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, };

            AsyncHelpers.ForEachItemInParallel(input, finalContinuation,
                (i, cont) =>
                {
                    lock (input)
                    {
                        sum += i;
                    }

                    throw new InvalidOperationException("Some failure.");
                });

            finalContinuationInvoked.WaitOne();
            Assert.AreEqual(55, sum);
            Assert.IsNotNull(lastException);
            Assert.IsInstanceOfType(typeof(NLogRuntimeException), lastException);
            Assert.IsTrue(lastException.Message.StartsWith("Got multiple exceptions:\r\n"));
        }

        [Test]
        public void PrecededByTest1()
        {
            int invokedCount1 = 0;
            int invokedCount2 = 0;
            int sequence = 7;
            int invokedCount1Sequence = 0;
            int invokedCount2Sequence = 0;

            AsyncContinuation originalContinuation = ex =>
            {
                invokedCount1++;
                invokedCount1Sequence = sequence++;
            };

            AsynchronousAction doSomethingElse = c =>
            {
                invokedCount2++;
                invokedCount2Sequence = sequence++;
                c(null);
                c(null);
            };

            AsyncContinuation cont = AsyncHelpers.PrecededBy(originalContinuation, doSomethingElse);
            cont(null);

            // make sure doSomethingElse was invoked first
            // then original continuation
            Assert.AreEqual(7, invokedCount2Sequence);
            Assert.AreEqual(8, invokedCount1Sequence);
            Assert.AreEqual(1, invokedCount1);
            Assert.AreEqual(1, invokedCount2);
        }

        [Test]
        public void PrecededByTest2()
        {
            int invokedCount1 = 0;
            int invokedCount2 = 0;
            int sequence = 7;
            int invokedCount1Sequence = 0;
            int invokedCount2Sequence = 0;

            AsyncContinuation originalContinuation = ex =>
            {
                invokedCount1++;
                invokedCount1Sequence = sequence++;
            };

            AsynchronousAction doSomethingElse = c =>
            {
                invokedCount2++;
                invokedCount2Sequence = sequence++;
                c(null);
                c(null);
            };

            AsyncContinuation cont = AsyncHelpers.PrecededBy(originalContinuation, doSomethingElse);
            var sampleException = new InvalidOperationException("Some message.");
            cont(sampleException);

            // make sure doSomethingElse was not invoked
            Assert.AreEqual(0, invokedCount2Sequence);
            Assert.AreEqual(7, invokedCount1Sequence);
            Assert.AreEqual(1, invokedCount1);
            Assert.AreEqual(0, invokedCount2);
        }
    }
}