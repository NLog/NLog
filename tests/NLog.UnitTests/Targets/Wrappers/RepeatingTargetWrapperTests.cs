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
    using NLog.Targets;
    using NLog.Targets.Wrappers;
    using Xunit;

    public class RepeatingTargetWrapperTests : NLogTestBase
	{
        [Fact]
        public void RepeatingTargetWrapperTest1()
        {
            var target = new MyTarget();
            var wrapper = new RepeatingTargetWrapper()
            {
                WrappedTarget = target,
                RepeatCount = 3,
            };
            wrapper.Initialize(null);
            target.Initialize(null);

            var exceptions = new List<Exception>();

            var events = new[]
            {
                new LogEventInfo(LogLevel.Debug, "Logger1", "Hello").WithContinuation(exceptions.Add),
                new LogEventInfo(LogLevel.Info, "Logger1", "Hello").WithContinuation(exceptions.Add),
                new LogEventInfo(LogLevel.Info, "Logger2", "Hello").WithContinuation(exceptions.Add),
            };

            wrapper.WriteAsyncLogEvents(events);

            // make sure all events went through and were replicated 3 times
            Assert.Equal(9, target.Events.Count);
            Assert.Same(events[0].LogEvent, target.Events[0]);
            Assert.Same(events[0].LogEvent, target.Events[1]);
            Assert.Same(events[0].LogEvent, target.Events[2]);
            Assert.Same(events[1].LogEvent, target.Events[3]);
            Assert.Same(events[1].LogEvent, target.Events[4]);
            Assert.Same(events[1].LogEvent, target.Events[5]);
            Assert.Same(events[2].LogEvent, target.Events[6]);
            Assert.Same(events[2].LogEvent, target.Events[7]);
            Assert.Same(events[2].LogEvent, target.Events[8]);

            Assert.Equal(events.Length, exceptions.Count);
        }

        [Fact]
        public void RepeatingTargetWrapperTest2()
        {
            using (new NoThrowNLogExceptions())
            {
                var target = new MyTarget();
                target.ThrowExceptions = true;
                var wrapper = new RepeatingTargetWrapper()
                {
                    WrappedTarget = target,
                    RepeatCount = 3,
                };
                wrapper.Initialize(null);
                target.Initialize(null);

                var exceptions = new List<Exception>();

                var events = new[]
                {
                    new LogEventInfo(LogLevel.Debug, "Logger1", "Hello").WithContinuation(exceptions.Add),
                    new LogEventInfo(LogLevel.Info, "Logger1", "Hello").WithContinuation(exceptions.Add),
                    new LogEventInfo(LogLevel.Info, "Logger2", "Hello").WithContinuation(exceptions.Add),
                };

                wrapper.WriteAsyncLogEvents(events);

                // make sure all events went through but were registered only once
                // since repeating target wrapper will not repeat in case of exception.
                Assert.Equal(3, target.Events.Count);
                Assert.Same(events[0].LogEvent, target.Events[0]);
                Assert.Same(events[1].LogEvent, target.Events[1]);
                Assert.Same(events[2].LogEvent, target.Events[2]);

                Assert.Equal(events.Length, exceptions.Count);
                foreach (var exception in exceptions)
                {
                    Assert.NotNull(exception);
                    Assert.Equal("Some exception has occurred.", exception.Message);
                }
            }
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

            public bool ThrowExceptions { get; set; }

            protected override void Write(LogEventInfo logEvent)
            {
                Events.Add(logEvent);

                if (ThrowExceptions)
                {
                    throw new ApplicationException("Some exception has occurred.");
                }
            }
        }
    }
}
