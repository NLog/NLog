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

    public class RoundRobinGroupTargetTests : NLogTestBase
	{
        [Fact]
        public void RoundRobinGroupTargetSyncTest1()
        {
            var myTarget1 = new MyTarget();
            var myTarget2 = new MyTarget();
            var myTarget3 = new MyTarget();

            var wrapper = new RoundRobinGroupTarget()
            {
                Targets = { myTarget1, myTarget2, myTarget3 },
            };

            myTarget1.Initialize(null);
            myTarget2.Initialize(null);
            myTarget3.Initialize(null);
            wrapper.Initialize(null);

            List<Exception> exceptions = new List<Exception>();

            // no exceptions
            for (int i = 0; i < 10; ++i)
            {
                wrapper.WriteAsyncLogEvent(LogEventInfo.CreateNullEvent().WithContinuation(exceptions.Add));
            }

            Assert.Equal(10, exceptions.Count);
            foreach (var e in exceptions)
            {
                Assert.Null(e);
            }

            Assert.Equal(4, myTarget1.WriteCount);
            Assert.Equal(3, myTarget2.WriteCount);
            Assert.Equal(3, myTarget3.WriteCount);

            Exception flushException = null;
            var flushHit = new ManualResetEvent(false);
            wrapper.Flush(ex => { flushException = ex; flushHit.Set(); });

            flushHit.WaitOne();
            if (flushException != null)
            {
                Assert.True(false, flushException.ToString());
            }

            Assert.Equal(1, myTarget1.FlushCount);
            Assert.Equal(1, myTarget2.FlushCount);
            Assert.Equal(1, myTarget3.FlushCount);
        }

        [Fact]
        public void RoundRobinGroupTargetSyncTest2()
        {
            var wrapper = new RoundRobinGroupTarget()
            {
                // empty target list
            };

            wrapper.Initialize(null);

            List<Exception> exceptions = new List<Exception>();

            // no exceptions
            for (int i = 0; i < 10; ++i)
            {
                wrapper.WriteAsyncLogEvent(LogEventInfo.CreateNullEvent().WithContinuation(exceptions.Add));
            }

            Assert.Equal(10, exceptions.Count);
            foreach (var e in exceptions)
            {
                Assert.Null(e);
            }

            Exception flushException = null;
            var flushHit = new ManualResetEvent(false);
            wrapper.Flush(ex => { flushException = ex; flushHit.Set(); });

            flushHit.WaitOne();
            if (flushException != null)
            {
                Assert.True(false, flushException.ToString());
            }
        }

        public class MyAsyncTarget : Target
        {
            public int FlushCount { get; private set; }
            public int WriteCount { get; private set; }

            public MyAsyncTarget() : base()
            {
            }

            public MyAsyncTarget(string name) : this()
            {
                Name = name;
            }

            protected override void Write(LogEventInfo logEvent)
            {
                throw new NotSupportedException();
            }

            protected override void Write(AsyncLogEventInfo logEvent)
            {
                Assert.True(FlushCount <= WriteCount);
                WriteCount++;
                ThreadPool.QueueUserWorkItem(
                    s =>
                        {
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
                        });
            }

            protected override void FlushAsync(AsyncContinuation asyncContinuation)
            {
                FlushCount++;
                ThreadPool.QueueUserWorkItem(
                    s => asyncContinuation(null));
            }

            public bool ThrowExceptions { get; set; }
        }

        class MyTarget : Target
        {
            public int FlushCount { get; set; }
            public int WriteCount { get; set; }
            public int FailCounter { get; set; }

            public MyTarget() : base()
            {
            }

            public MyTarget(string name) : this()
            {
                Name = name;
            }

            protected override void Write(LogEventInfo logEvent)
            {
                Assert.True(FlushCount <= WriteCount);
                WriteCount++;

                if (FailCounter > 0)
                {
                    FailCounter--;
                    throw new InvalidOperationException("Some failure.");
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
