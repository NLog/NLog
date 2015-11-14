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
    using NLog.Common;
    using NLog.Targets;
    using NLog.Targets.Wrappers;
#if(__IOS__)
	using NUnit.Framework;
	using Assert = NUnit.Framework.NLog.Assert;
#else
    using Xunit;
#endif

    public class AutoFlushTargetWrapperTests : NLogTestBase
	{
        [Fact]
        public void AutoFlushTargetWrapperSyncTest1()
        {
            var myTarget = new MyTarget();
            var wrapper = new AutoFlushTargetWrapper
            {
                WrappedTarget = myTarget,
            };

            myTarget.Initialize(null);
            wrapper.Initialize(null);
            var logEvent = new LogEventInfo();
            Exception lastException = null;
            bool continuationHit = false;
            AsyncContinuation continuation =
                ex =>
                    {
                        lastException = ex;
                        continuationHit = true;
                    };

            wrapper.WriteAsyncLogEvent(logEvent.WithContinuation(continuation));

            Assert.True(continuationHit);
            Assert.Null(lastException);
            Assert.Equal(1, myTarget.FlushCount);
            Assert.Equal(1, myTarget.WriteCount);

            continuationHit = false;
            wrapper.WriteAsyncLogEvent(logEvent.WithContinuation(continuation));
            Assert.True(continuationHit);
            Assert.Null(lastException);
            Assert.Equal(2, myTarget.WriteCount);
            Assert.Equal(2, myTarget.FlushCount);
        }

        [Fact]
        public void AutoFlushTargetWrapperAsyncTest1()
        {
            var myTarget = new MyAsyncTarget();
            var wrapper = new AutoFlushTargetWrapper(myTarget);
            myTarget.Initialize(null);
            wrapper.Initialize(null);
            var logEvent = new LogEventInfo();
            Exception lastException = null;
            var continuationHit = new ManualResetEvent(false);
            AsyncContinuation continuation =
                ex =>
                {
                    lastException = ex;
                    continuationHit.Set();
                };

            wrapper.WriteAsyncLogEvent(logEvent.WithContinuation(continuation));

            continuationHit.WaitOne();
            Assert.Null(lastException);
            Assert.Equal(1, myTarget.FlushCount);
            Assert.Equal(1, myTarget.WriteCount);

            continuationHit.Reset();
            wrapper.WriteAsyncLogEvent(logEvent.WithContinuation(continuation));
            continuationHit.WaitOne();
            Assert.Null(lastException);
            Assert.Equal(2, myTarget.WriteCount);
            Assert.Equal(2, myTarget.FlushCount);
        }


        [Fact]
        public void AutoFlushTargetWrapperAsyncWithExceptionTest1()
        {
            var myTarget = new MyAsyncTarget
            {
                ThrowExceptions = true,
            };

            var wrapper = new AutoFlushTargetWrapper(myTarget);
            myTarget.Initialize(null);
            wrapper.Initialize(null);
            var logEvent = new LogEventInfo();
            Exception lastException = null;
            var continuationHit = new ManualResetEvent(false);
            AsyncContinuation continuation =
                ex =>
                {
                    lastException = ex;
                    continuationHit.Set();
                };

            wrapper.WriteAsyncLogEvent(logEvent.WithContinuation(continuation));

            continuationHit.WaitOne();
            Assert.NotNull(lastException);
            Assert.IsType(typeof(InvalidOperationException), lastException);

            // no flush on exception
            Assert.Equal(0, myTarget.FlushCount);
            Assert.Equal(1, myTarget.WriteCount);

            continuationHit.Reset();
            lastException = null;
            wrapper.WriteAsyncLogEvent(logEvent.WithContinuation(continuation));
            continuationHit.WaitOne();
            Assert.NotNull(lastException);
            Assert.IsType(typeof(InvalidOperationException), lastException);
            Assert.Equal(0, myTarget.FlushCount);
            Assert.Equal(2, myTarget.WriteCount);
        }

        class MyAsyncTarget : Target
        {
            public int FlushCount { get; private set; }
            public int WriteCount { get; private set; }

            protected override void Write(LogEventInfo logEvent)
            {
                throw new NotSupportedException();
            }

            protected override void Write(AsyncLogEventInfo logEvent)
            {
                Assert.True(this.FlushCount <= this.WriteCount);
                this.WriteCount++;
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

            protected override void FlushAsync(AsyncContinuation asyncContinuation)
            {
                this.FlushCount++;
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
