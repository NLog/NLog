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
    using System.IO;
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
    public class PostFilteringTargetWrapperTests : NLogTestBase
	{
        [Test]
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
            Assert.AreEqual(3, target.Events.Count);
            Assert.AreSame(events[1].LogEvent, target.Events[0]);
            Assert.AreSame(events[2].LogEvent, target.Events[1]);
            Assert.AreSame(events[5].LogEvent, target.Events[2]);

            Assert.AreEqual(events.Length, exceptions.Count, "Some continuations were not invoked.");
        }

        [Test]
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

            string internalLogOutput = RunAndCaptureInternalLog(() => wrapper.WriteAsyncLogEvents(events), LogLevel.Trace);
            string expectedLogOutput = @"Trace Running PostFilteringWrapper Target[(unnamed)](MyTarget) on 7 events
Trace Rule matched: (level >= Warn)
Trace Filter to apply: (level >= Debug)
Trace After filtering: 6 events.
Trace Sending to MyTarget
";
            Assert.AreEqual(expectedLogOutput, internalLogOutput);

            // make sure all Debug,Info,Warn events went through
            Assert.AreEqual(6, target.Events.Count);
            Assert.AreSame(events[0].LogEvent, target.Events[0]);
            Assert.AreSame(events[1].LogEvent, target.Events[1]);
            Assert.AreSame(events[2].LogEvent, target.Events[2]);
            Assert.AreSame(events[3].LogEvent, target.Events[3]);
            Assert.AreSame(events[5].LogEvent, target.Events[4]);
            Assert.AreSame(events[6].LogEvent, target.Events[5]);

            Assert.AreEqual(events.Length, exceptions.Count, "Some continuations were not invoked.");
        }

        [Test]
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

            var internalLogOutput = RunAndCaptureInternalLog(() => wrapper.WriteAsyncLogEvents(events), LogLevel.Trace);
            string expectedLogOutput = @"Trace Running PostFilteringWrapper Target[(unnamed)](MyTarget) on 7 events
Trace Rule matched: (level >= Error)
Trace Filter to apply: True
Trace After filtering: 7 events.
Trace Sending to MyTarget
";

            Assert.AreEqual(expectedLogOutput, internalLogOutput);

            // make sure all events went through
            Assert.AreEqual(7, target.Events.Count);
            Assert.AreSame(events[0].LogEvent, target.Events[0]);
            Assert.AreSame(events[1].LogEvent, target.Events[1]);
            Assert.AreSame(events[2].LogEvent, target.Events[2]);
            Assert.AreSame(events[3].LogEvent, target.Events[3]);
            Assert.AreSame(events[4].LogEvent, target.Events[4]);
            Assert.AreSame(events[5].LogEvent, target.Events[5]);
            Assert.AreSame(events[6].LogEvent, target.Events[6]);

            Assert.AreEqual(events.Length, exceptions.Count, "Some continuations were not invoked.");
        }

        [Test]
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
            Assert.AreEqual(7, target.Events.Count);
            Assert.AreSame(events[0].LogEvent, target.Events[0]);
            Assert.AreSame(events[1].LogEvent, target.Events[1]);
            Assert.AreSame(events[2].LogEvent, target.Events[2]);
            Assert.AreSame(events[3].LogEvent, target.Events[3]);
            Assert.AreSame(events[4].LogEvent, target.Events[4]);
            Assert.AreSame(events[5].LogEvent, target.Events[5]);
            Assert.AreSame(events[6].LogEvent, target.Events[6]);

            Assert.AreEqual(events.Length, exceptions.Count, "Some continuations were not invoked.");
        }

        public class MyTarget : Target
        {
            public MyTarget()
            {
                this.Events = new List<LogEventInfo>();
            }

            public List<LogEventInfo> Events { get; set; }

            protected override void Write(LogEventInfo logEvent)
            {
                this.Events.Add(logEvent);
            }
        }
    }
}
