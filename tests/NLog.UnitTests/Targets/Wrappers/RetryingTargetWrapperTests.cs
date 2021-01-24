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
    using NLog.Common;
    using NLog.Targets;
    using NLog.Targets.Wrappers;
    using Xunit;

    public class RetryingTargetWrapperTests : NLogTestBase
	{
        [Fact]
        public void RetryingTargetWrapperTest1()
        {
            var target = new MyTarget();
            var wrapper = new RetryingTargetWrapper()
            {
                WrappedTarget = target,
                RetryCount = 10,
                RetryDelayMilliseconds = 1,
            };

            wrapper.Initialize(null);
            target.Initialize(null);

            var exceptions = new List<Exception>();

            var events = new []
            {
                new LogEventInfo(LogLevel.Debug, "Logger1", "Hello").WithContinuation(exceptions.Add),
                new LogEventInfo(LogLevel.Info, "Logger1", "Hello").WithContinuation(exceptions.Add),
                new LogEventInfo(LogLevel.Info, "Logger2", "Hello").WithContinuation(exceptions.Add),
            };

            wrapper.WriteAsyncLogEvents(events);

            // make sure all events went through
            Assert.Equal(3, target.Events.Count);
            Assert.Same(events[0].LogEvent, target.Events[0]);
            Assert.Same(events[1].LogEvent, target.Events[1]);
            Assert.Same(events[2].LogEvent, target.Events[2]);

            Assert.Equal(events.Length, exceptions.Count);

            // make sure there were no exception
            foreach (var ex in exceptions)
            {
                Assert.Null(ex);
            }
        }

        [Fact]
        public void RetryingTargetWrapperTest2()
        {
            var target = new MyTarget()
            {
                ThrowExceptions = 6,
            };

            var wrapper = new RetryingTargetWrapper()
            {
                WrappedTarget = target,
                RetryCount = 4,
                RetryDelayMilliseconds = 1,
            };

            wrapper.Initialize(null);
            target.Initialize(null);

            var exceptions = new List<Exception>();

            var events = new []
            {
                new LogEventInfo(LogLevel.Debug, "Logger1", "Hello").WithContinuation(exceptions.Add),
                new LogEventInfo(LogLevel.Info, "Logger1", "Hello").WithContinuation(exceptions.Add),
                new LogEventInfo(LogLevel.Info, "Logger2", "Hello").WithContinuation(exceptions.Add),
            };

            var result = RunAndCaptureInternalLog(() => wrapper.WriteAsyncLogEvents(events), LogLevel.Trace);
            Assert.True(result.IndexOf("Error while writing to 'MyTarget'. Try 1/4") != -1);
            Assert.True(result.IndexOf("Error while writing to 'MyTarget'. Try 2/4") != -1);
            Assert.True(result.IndexOf("Error while writing to 'MyTarget'. Try 3/4") != -1);
            Assert.True(result.IndexOf("Error while writing to 'MyTarget'. Try 4/4") != -1);
            Assert.True(result.IndexOf("Too many retries. Aborting.") != -1);
            Assert.True(result.IndexOf("Error while writing to 'MyTarget'. Try 1/4") != -1);
            Assert.True(result.IndexOf("Error while writing to 'MyTarget'. Try 2/4") != -1);

            // first event does not get to wrapped target because of too many attempts.
            // second event gets there in 3rd retry
            // and third event gets there immediately
            Assert.Equal(2, target.Events.Count);
            Assert.Same(events[1].LogEvent, target.Events[0]);
            Assert.Same(events[2].LogEvent, target.Events[1]);

            Assert.Equal(events.Length, exceptions.Count);

            Assert.NotNull(exceptions[0]);
            Assert.Equal("Some exception has occurred.", exceptions[0].Message);
            Assert.Null(exceptions[1]);
            Assert.Null(exceptions[2]);
        }

#if MONO
        [Fact(Skip="Not working under MONO - Premature abort seems to fail, and instead it just waits until finished")]
#else
        [Fact]
#endif
        public void RetryingTargetWrapperBlockingCloseTest()
        {
            RetryingIntegrationTest(3, () =>
            {
                var target = new MyTarget()
                {
                    ThrowExceptions = 5,
                };
                var wrapper = new RetryingTargetWrapper()
                {
                    WrappedTarget = target,
                    RetryCount = 10,
                    RetryDelayMilliseconds = 5000,
                };
                var asyncWrapper = new AsyncTargetWrapper(wrapper) {TimeToSleepBetweenBatches = 1};

                asyncWrapper.Initialize(null);
                wrapper.Initialize(null);
                target.Initialize(null);

                var exceptions = new List<Exception>();

                var events = new[]
                {
                    new LogEventInfo(LogLevel.Debug, "Logger1", "Hello").WithContinuation(exceptions.Add),
                    new LogEventInfo(LogLevel.Info, "Logger1", "Hello").WithContinuation(exceptions.Add),
                    new LogEventInfo(LogLevel.Info, "Logger2", "Hello").WithContinuation(exceptions.Add),
                };

                // Attempt to write LogEvents that will take forever to retry
                asyncWrapper.WriteAsyncLogEvents(events);
                // Wait a little for the AsyncWrapper to start writing
                System.Threading.Thread.Sleep(50);
                // Close down the AsyncWrapper while busy writing
                asyncWrapper.Close();
                // Close down the RetryingWrapper while busy retrying
                wrapper.Close();
                // Close down the actual target while busy writing
                target.Close();
                // Wait a little for the RetryingWrapper to detect that it has been closed down
                System.Threading.Thread.Sleep(200);
                // The premature abort, causes the exception to be logged
                Assert.NotNull(exceptions[0]);
            });
        }

        public class MyTarget : Target
        {
            public MyTarget()
            {
                Events = new List<LogEventInfo>();
            }

            public MyTarget(string name) : this()
            {
                Name = name;
            }

            public List<LogEventInfo> Events { get; set; }

            public int ThrowExceptions { get; set; }

            protected override void Write(AsyncLogEventInfo logEvent)
            {
                if (ThrowExceptions-- > 0)
                {
                    logEvent.Continuation(new ApplicationException("Some exception has occurred."));
                    return;
                }

                Events.Add(logEvent.LogEvent);
                logEvent.Continuation(null);
            }

            protected override void Write(LogEventInfo logEvent)
            {
            }
        }
    }
}
