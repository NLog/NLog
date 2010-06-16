// 
// Copyright (c) 2004-2010 Jaroslaw Kowalski <jaak@jkowalski.net>
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
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using NLog.Common;
    using NLog.Internal;
    using NLog.Targets;
    using NLog.Targets.Wrappers;

    [TestClass]
    public class PostFilteringTargetWrapperTests : NLogTestBase
	{
        [TestMethod]
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

            wrapper.Initialize();
            target.Initialize();

            var events = new LogEventInfo[]
            {
                new LogEventInfo(LogLevel.Debug, "Logger1", "Hello"),
                new LogEventInfo(LogLevel.Info, "Logger1", "Hello"),
                new LogEventInfo(LogLevel.Info, "Logger2", "Hello"),
                new LogEventInfo(LogLevel.Debug, "Logger1", "Hello"),
                new LogEventInfo(LogLevel.Trace, "Logger1", "Hello"),
                new LogEventInfo(LogLevel.Info, "Logger3", "Hello"),
            };

            var exceptions = new List<Exception>();
            
            var continuations = new AsyncContinuation[events.Length];
            for (int i = 0; i < continuations.Length; ++i)
            {
                continuations[i] = exceptions.Add;
            }

            wrapper.WriteLogEvents(events, continuations);

            // make sure all Info events went through
            Assert.AreEqual(3, target.Events.Count);
            Assert.AreSame(events[1], target.Events[0]);
            Assert.AreSame(events[2], target.Events[1]);
            Assert.AreSame(events[5], target.Events[2]);

            Assert.AreEqual(continuations.Length, exceptions.Count, "Some continuations were not invoked.");
        }

        [TestMethod]
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

            wrapper.Initialize();
            target.Initialize();

            var events = new LogEventInfo[]
            {
                new LogEventInfo(LogLevel.Debug, "Logger1", "Hello"),
                new LogEventInfo(LogLevel.Info, "Logger1", "Hello"),
                new LogEventInfo(LogLevel.Info, "Logger2", "Hello"),
                new LogEventInfo(LogLevel.Debug, "Logger1", "Hello"),
                new LogEventInfo(LogLevel.Trace, "Logger1", "Hello"),
                new LogEventInfo(LogLevel.Info, "Logger3", "Hello"),
                new LogEventInfo(LogLevel.Warn, "Logger1", "Hello"),
            };

            var exceptions = new List<Exception>();

            var continuations = new AsyncContinuation[events.Length];
            for (int i = 0; i < continuations.Length; ++i)
            {
                continuations[i] = exceptions.Add;
            }

            string internalLogOutput = RunAndCaptureInternalLog(() => wrapper.WriteLogEvents(events, continuations), LogLevel.Trace);
            string expectedLogOutput = @"Trace Input: 7 events
Trace Rule matched: (level >= Warn)
Trace Filter to apply: (level >= Debug)
Trace After filtering: 6 events
";
            Assert.AreEqual(expectedLogOutput, internalLogOutput);

            // make sure all Debug,Info,Warn events went through
            Assert.AreEqual(6, target.Events.Count);
            Assert.AreSame(events[0], target.Events[0]);
            Assert.AreSame(events[1], target.Events[1]);
            Assert.AreSame(events[2], target.Events[2]);
            Assert.AreSame(events[3], target.Events[3]);
            Assert.AreSame(events[5], target.Events[4]);
            Assert.AreSame(events[6], target.Events[5]);

            Assert.AreEqual(continuations.Length, exceptions.Count, "Some continuations were not invoked.");
        }

        [TestMethod]
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

            wrapper.Initialize();
            target.Initialize();

            var events = new LogEventInfo[]
            {
                new LogEventInfo(LogLevel.Debug, "Logger1", "Hello"),
                new LogEventInfo(LogLevel.Info, "Logger1", "Hello"),
                new LogEventInfo(LogLevel.Info, "Logger2", "Hello"),
                new LogEventInfo(LogLevel.Debug, "Logger1", "Hello"),
                new LogEventInfo(LogLevel.Trace, "Logger1", "Hello"),
                new LogEventInfo(LogLevel.Info, "Logger3", "Hello"),
                new LogEventInfo(LogLevel.Error, "Logger1", "Hello"),
            };

            var exceptions = new List<Exception>();

            var continuations = new AsyncContinuation[events.Length];
            for (int i = 0; i < continuations.Length; ++i)
            {
                continuations[i] = exceptions.Add;
            }

            var internalLogOutput = RunAndCaptureInternalLog(() => wrapper.WriteLogEvents(events, continuations), LogLevel.Trace);
            string expectedLogOutput = @"Trace Input: 7 events
Trace Rule matched: (level >= Error)
Trace Filter to apply: True
Trace After filtering: 7 events
";

            Assert.AreEqual(expectedLogOutput, internalLogOutput);

            // make sure all events went through
            Assert.AreEqual(7, target.Events.Count);
            Assert.AreSame(events[0], target.Events[0]);
            Assert.AreSame(events[1], target.Events[1]);
            Assert.AreSame(events[2], target.Events[2]);
            Assert.AreSame(events[3], target.Events[3]);
            Assert.AreSame(events[4], target.Events[4]);
            Assert.AreSame(events[5], target.Events[5]);
            Assert.AreSame(events[6], target.Events[6]);

            Assert.AreEqual(continuations.Length, exceptions.Count, "Some continuations were not invoked.");
        }

        [TestMethod]
        public void PostFilteringTargetWrapperNoFiltersDefined()
        {
            var target = new MyTarget();
            var wrapper = new PostFilteringTargetWrapper()
            {
                WrappedTarget = target,
            };

            wrapper.Initialize();
            target.Initialize();

            var events = new LogEventInfo[]
            {
                new LogEventInfo(LogLevel.Debug, "Logger1", "Hello"),
                new LogEventInfo(LogLevel.Info, "Logger1", "Hello"),
                new LogEventInfo(LogLevel.Info, "Logger2", "Hello"),
                new LogEventInfo(LogLevel.Debug, "Logger1", "Hello"),
                new LogEventInfo(LogLevel.Trace, "Logger1", "Hello"),
                new LogEventInfo(LogLevel.Info, "Logger3", "Hello"),
                new LogEventInfo(LogLevel.Error, "Logger1", "Hello"),
            };

            var exceptions = new List<Exception>();

            var continuations = new AsyncContinuation[events.Length];
            for (int i = 0; i < continuations.Length; ++i)
            {
                continuations[i] = exceptions.Add;
            }

            wrapper.WriteLogEvents(events, continuations);

            // make sure all events went through
            Assert.AreEqual(7, target.Events.Count);
            Assert.AreSame(events[0], target.Events[0]);
            Assert.AreSame(events[1], target.Events[1]);
            Assert.AreSame(events[2], target.Events[2]);
            Assert.AreSame(events[3], target.Events[3]);
            Assert.AreSame(events[4], target.Events[4]);
            Assert.AreSame(events[5], target.Events[5]);
            Assert.AreSame(events[6], target.Events[6]);

            Assert.AreEqual(continuations.Length, exceptions.Count, "Some continuations were not invoked.");
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
