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
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using NLog.Internal;
    using NLog.Targets;
    using NLog.Targets.Wrappers;

    [TestClass]
    public class RepeatingTargetWrapperTests : NLogTestBase
	{
        [TestMethod]
        public void RepeatingTargetWrapperTest1()
        {
            var target = new MyTarget();
            var wrapper = new RepeatingTargetWrapper()
            {
                WrappedTarget = target,
                RepeatCount = 3,
            };
            ((ISupportsInitialize)wrapper).Initialize();
            ((ISupportsInitialize)target).Initialize();

            var events = new LogEventInfo[]
            {
                new LogEventInfo(LogLevel.Debug, "Logger1", "Hello"),
                new LogEventInfo(LogLevel.Info, "Logger1", "Hello"),
                new LogEventInfo(LogLevel.Info, "Logger2", "Hello"),
            };

            var exceptions = new List<Exception>();
            
            var continuations = new AsyncContinuation[events.Length];
            for (int i = 0; i < continuations.Length; ++i)
            {
                continuations[i] = exceptions.Add;
            }

            wrapper.WriteLogEvents(events, continuations);

            // make sure all events went through and were replicated 3 times
            Assert.AreEqual(9, target.Events.Count);
            Assert.AreSame(events[0], target.Events[0]);
            Assert.AreSame(events[0], target.Events[1]);
            Assert.AreSame(events[0], target.Events[2]);
            Assert.AreSame(events[1], target.Events[3]);
            Assert.AreSame(events[1], target.Events[4]);
            Assert.AreSame(events[1], target.Events[5]);
            Assert.AreSame(events[2], target.Events[6]);
            Assert.AreSame(events[2], target.Events[7]);
            Assert.AreSame(events[2], target.Events[8]);

            Assert.AreEqual(continuations.Length, exceptions.Count, "Some continuations were not invoked.");
        }

        [TestMethod]
        public void RepeatingTargetWrapperTest2()
        {
            var target = new MyTarget();
            target.ThrowExceptions = true;
            var wrapper = new RepeatingTargetWrapper()
            {
                WrappedTarget = target,
                RepeatCount = 3,
            };
            ((ISupportsInitialize)wrapper).Initialize();
            ((ISupportsInitialize)target).Initialize();

            var events = new LogEventInfo[]
            {
                new LogEventInfo(LogLevel.Debug, "Logger1", "Hello"),
                new LogEventInfo(LogLevel.Info, "Logger1", "Hello"),
                new LogEventInfo(LogLevel.Info, "Logger2", "Hello"),
            };

            var exceptions = new List<Exception>();

            var continuations = new AsyncContinuation[events.Length];
            for (int i = 0; i < continuations.Length; ++i)
            {
                continuations[i] = exceptions.Add;
            }

            wrapper.WriteLogEvents(events, continuations);

            // make sure all events went through but were registered only once
            // since repeating target wrapper will not repeat in case of exception.
            Assert.AreEqual(3, target.Events.Count);
            Assert.AreSame(events[0], target.Events[0]);
            Assert.AreSame(events[1], target.Events[1]);
            Assert.AreSame(events[2], target.Events[2]);

            Assert.AreEqual(continuations.Length, exceptions.Count, "Some continuations were not invoked.");
            foreach (var exception in exceptions)
            {
                Assert.IsNotNull(exception);
                Assert.AreEqual("Some exception has ocurred.", exception.Message);
            }
        }

        public class MyTarget : Target
        {
            public MyTarget()
            {
                this.Events = new List<LogEventInfo>();
            }

            public List<LogEventInfo> Events { get; set; }

            public bool ThrowExceptions { get; set; }

            protected override void Write(LogEventInfo logEvent)
            {
                this.Events.Add(logEvent);

                if (this.ThrowExceptions)
                {
                    throw new InvalidOperationException("Some exception has ocurred.");
                }
            }
        }
    }
}
