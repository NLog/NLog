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

    public class PostFilteringTargetWrapperTests : NLogTestBase
	{
        [Fact]
        public void PostFilteringTargetWrapperUsingDefaultFilterTest()
        {
            var target = new MyTarget();
            var wrapper = new PostFilteringTargetWrapper()
            {
                WrappedTarget = target,
                Rules =
                {
                    // if we had any warnings, log debug too
                    new FilteringRule("level >= LogLevel.Warn", "level >= LogLevel.Debug"),

                    // when there is an error, emit everything
                    new FilteringRule
                    {
                        Exists = "level >= LogLevel.Error", 
                        Filter = "true",
                    },
                },

                // by default log info and above
                DefaultFilter = "level >= LogLevel.Info",
            };

            wrapper.Initialize(null);
            target.Initialize(null);

            var exceptions = new List<Exception>();
            
            var events = new []
            {
                new LogEventInfo(LogLevel.Debug, "Logger1", "Hello").WithContinuation(exceptions.Add),
                new LogEventInfo(LogLevel.Info, "Logger1", "Hello").WithContinuation(exceptions.Add),
                new LogEventInfo(LogLevel.Info, "Logger2", "Hello").WithContinuation(exceptions.Add),
                new LogEventInfo(LogLevel.Debug, "Logger1", "Hello").WithContinuation(exceptions.Add),
                new LogEventInfo(LogLevel.Trace, "Logger1", "Hello").WithContinuation(exceptions.Add),
                new LogEventInfo(LogLevel.Info, "Logger3", "Hello").WithContinuation(exceptions.Add),
            };

            wrapper.WriteAsyncLogEvents(events);

            // make sure all Info events went through
            Assert.Equal(3, target.Events.Count);
            Assert.Same(events[1].LogEvent, target.Events[0]);
            Assert.Same(events[2].LogEvent, target.Events[1]);
            Assert.Same(events[5].LogEvent, target.Events[2]);

            Assert.Equal(events.Length, exceptions.Count);
        }

        [Fact]
        public void PostFilteringTargetWrapperUsingDefaultNonFilterTest()
        {
            var target = new MyTarget();
            var wrapper = new PostFilteringTargetWrapper()
            {
                WrappedTarget = target,
                Rules =
                {
                    // if we had any warnings, log debug too
                    new FilteringRule("level >= LogLevel.Warn", "level >= LogLevel.Debug"),

                    // when there is an error, emit everything
                    new FilteringRule("level >= LogLevel.Error", "true"),
                },

                // by default log info and above
                DefaultFilter = "level >= LogLevel.Info",
            };

            wrapper.Initialize(null);
            target.Initialize(null);

            var exceptions = new List<Exception>();

            var events = new[]
            {
                new LogEventInfo(LogLevel.Debug, "Logger1", "Hello").WithContinuation(exceptions.Add),
                new LogEventInfo(LogLevel.Info, "Logger1", "Hello").WithContinuation(exceptions.Add),
                new LogEventInfo(LogLevel.Info, "Logger2", "Hello").WithContinuation(exceptions.Add),
                new LogEventInfo(LogLevel.Debug, "Logger1", "Hello").WithContinuation(exceptions.Add),
                new LogEventInfo(LogLevel.Trace, "Logger1", "Hello").WithContinuation(exceptions.Add),
                new LogEventInfo(LogLevel.Info, "Logger3", "Hello").WithContinuation(exceptions.Add),
                new LogEventInfo(LogLevel.Warn, "Logger1", "Hello").WithContinuation(exceptions.Add),
            };

            string result = RunAndCaptureInternalLog(() => wrapper.WriteAsyncLogEvents(events), LogLevel.Trace);
            Assert.True(result.IndexOf("Running on 7 events") != -1);
            Assert.True(result.IndexOf("Rule matched: (level >= Warn)") != -1);
            Assert.True(result.IndexOf("Filter to apply: (level >= Debug)") != -1);
            Assert.True(result.IndexOf("After filtering: 6 events.") != -1);
            Assert.True(result.IndexOf("Sending to MyTarget") != -1);

            // make sure all Debug,Info,Warn events went through
            Assert.Equal(6, target.Events.Count);
            Assert.Same(events[0].LogEvent, target.Events[0]);
            Assert.Same(events[1].LogEvent, target.Events[1]);
            Assert.Same(events[2].LogEvent, target.Events[2]);
            Assert.Same(events[3].LogEvent, target.Events[3]);
            Assert.Same(events[5].LogEvent, target.Events[4]);
            Assert.Same(events[6].LogEvent, target.Events[5]);

            Assert.Equal(events.Length, exceptions.Count);
        }

        [Fact]
        public void PostFilteringTargetWrapperUsingDefaultNonFilterTest2()
        {
            // in this case both rules would match, but first one is picked
            var target = new MyTarget();
            var wrapper = new PostFilteringTargetWrapper()
            {
                WrappedTarget = target,
                Rules =
                {
                    // when there is an error, emit everything
                    new FilteringRule("level >= LogLevel.Error", "true"),

                    // if we had any warnings, log debug too
                    new FilteringRule("level >= LogLevel.Warn", "level >= LogLevel.Debug"),
                },

                // by default log info and above
                DefaultFilter = "level >= LogLevel.Info",
            };

            wrapper.Initialize(null);
            target.Initialize(null);

            var exceptions = new List<Exception>();

            var events = new []
            {
                new LogEventInfo(LogLevel.Debug, "Logger1", "Hello").WithContinuation(exceptions.Add),
                new LogEventInfo(LogLevel.Info, "Logger1", "Hello").WithContinuation(exceptions.Add),
                new LogEventInfo(LogLevel.Info, "Logger2", "Hello").WithContinuation(exceptions.Add),
                new LogEventInfo(LogLevel.Debug, "Logger1", "Hello").WithContinuation(exceptions.Add),
                new LogEventInfo(LogLevel.Trace, "Logger1", "Hello").WithContinuation(exceptions.Add),
                new LogEventInfo(LogLevel.Info, "Logger3", "Hello").WithContinuation(exceptions.Add),
                new LogEventInfo(LogLevel.Error, "Logger1", "Hello").WithContinuation(exceptions.Add),
            };

            var result = RunAndCaptureInternalLog(() => wrapper.WriteAsyncLogEvents(events), LogLevel.Trace);
            Assert.True(result.IndexOf("Running on 7 events") != -1);
            Assert.True(result.IndexOf("Rule matched: (level >= Error)") != -1);
            Assert.True(result.IndexOf("Filter to apply: True") != -1);
            Assert.True(result.IndexOf("After filtering: 7 events.") != -1);
            Assert.True(result.IndexOf("Sending to MyTarget") != -1);

            // make sure all events went through
            Assert.Equal(7, target.Events.Count);
            Assert.Same(events[0].LogEvent, target.Events[0]);
            Assert.Same(events[1].LogEvent, target.Events[1]);
            Assert.Same(events[2].LogEvent, target.Events[2]);
            Assert.Same(events[3].LogEvent, target.Events[3]);
            Assert.Same(events[4].LogEvent, target.Events[4]);
            Assert.Same(events[5].LogEvent, target.Events[5]);
            Assert.Same(events[6].LogEvent, target.Events[6]);

            Assert.Equal(events.Length, exceptions.Count);
        }

        [Fact]
        public void PostFilteringTargetWrapperOnlyDefaultFilter()
        {
            var target = new MyTarget();
            var wrapper = new PostFilteringTargetWrapper()
            {
                WrappedTarget = target,
                DefaultFilter = "level >= LogLevel.Info",   // by default log info and above
            };

            wrapper.Initialize(null);
            target.Initialize(null);

            var exceptions = new List<Exception>();
            wrapper.WriteAsyncLogEvent(new LogEventInfo(LogLevel.Info, "Logger1", "Hello").WithContinuation(exceptions.Add));
            Assert.Single(target.Events);
            wrapper.WriteAsyncLogEvent(new LogEventInfo(LogLevel.Debug, "Logger1", "Hello").WithContinuation(exceptions.Add));
            Assert.Single(target.Events);
        }

        [Fact]
        public void PostFilteringTargetWrapperNoFiltersDefined()
        {
            var target = new MyTarget();
            var wrapper = new PostFilteringTargetWrapper()
            {
                WrappedTarget = target,
            };

            wrapper.Initialize(null);
            target.Initialize(null);

            var exceptions = new List<Exception>();

            var events = new[]
            {
                new LogEventInfo(LogLevel.Debug, "Logger1", "Hello").WithContinuation(exceptions.Add),
                new LogEventInfo(LogLevel.Info, "Logger1", "Hello").WithContinuation(exceptions.Add),
                new LogEventInfo(LogLevel.Info, "Logger2", "Hello").WithContinuation(exceptions.Add),
                new LogEventInfo(LogLevel.Debug, "Logger1", "Hello").WithContinuation(exceptions.Add),
                new LogEventInfo(LogLevel.Trace, "Logger1", "Hello").WithContinuation(exceptions.Add),
                new LogEventInfo(LogLevel.Info, "Logger3", "Hello").WithContinuation(exceptions.Add),
                new LogEventInfo(LogLevel.Error, "Logger1", "Hello").WithContinuation(exceptions.Add),
            };

            wrapper.WriteAsyncLogEvents(events);

            // make sure all events went through
            Assert.Equal(7, target.Events.Count);
            Assert.Same(events[0].LogEvent, target.Events[0]);
            Assert.Same(events[1].LogEvent, target.Events[1]);
            Assert.Same(events[2].LogEvent, target.Events[2]);
            Assert.Same(events[3].LogEvent, target.Events[3]);
            Assert.Same(events[4].LogEvent, target.Events[4]);
            Assert.Same(events[5].LogEvent, target.Events[5]);
            Assert.Same(events[6].LogEvent, target.Events[6]);

            Assert.Equal(events.Length, exceptions.Count);
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

            protected override void Write(LogEventInfo logEvent)
            {
                Events.Add(logEvent);
            }
        }
    }
}
