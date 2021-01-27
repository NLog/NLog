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
    using System.Linq;
    using System.Threading;
    using NLog.Common;
    using NLog.Targets;
    using NLog.Targets.Wrappers;
    using Xunit;

    public class SplitGroupTargetTests : NLogTestBase
    {
        [Fact]
        public void NoTargets_SplitGroupTarget_IsWorking()
        {
            SplitGroupTarget_Exercise(new MyTarget[] { });
        }

        [Fact]
        public void SingleTarget_SplitGroupTarget_IsWorking()
        {
            SplitGroupTarget_Exercise(new []{ new MyTarget() });
        }

        [Fact]
        public void MultipleTargets_SplitGroupTarget_IsWorking()
        {
            SplitGroupTarget_Exercise(new[] { new MyTarget(), new MyTarget(), new MyTarget() });
        }

        [Fact]
        public void FirstTargetFails_SplitGroupTarget_WritesToAll()
        {
            using (new NoThrowNLogExceptions())
            {
                int logEventFailCount = 2;
                var failingTarget = new MyTarget() { FailCounter = logEventFailCount };
                SplitGroupTarget_Exercise(new[] { failingTarget, new MyTarget(), new MyTarget() }, logEventFailCount);
            }
        }

        [Fact]
        public void AsyncOutOfOrder_SplitGroupTarget_IsWorking()
        {
            var targets = Enumerable.Range(0, 3).Select(i => new AsyncTargetWrapper(i.ToString(), new MyTarget()) { TimeToSleepBetweenBatches = i * 10 }).ToArray();
            targets.ToList().ForEach(t => t.WrappedTarget.Initialize(null));
            Func<Target, MyTarget> lookupTarget = t => (MyTarget)((AsyncTargetWrapper)t).WrappedTarget;
            SplitGroupTarget_Exercise(targets, 0, lookupTarget);
        }

        private static void SplitGroupTarget_Exercise(Target[] targets, int logEventFailCount = 0, Func<Target, MyTarget> lookupTarget = null)
        {
            // Arrange
            var wrapper = new SplitGroupTarget(targets);
            foreach (var target in targets)
                target.Initialize(null);
            wrapper.Initialize(null);
            List<Exception> exceptions = new List<Exception>();
            var flushComplete = new ManualResetEvent(false);
            lookupTarget = lookupTarget ?? new Func<Target, MyTarget>(t => (MyTarget)t);
            const int LogEventBatchSize = 2;
            const int LogEventWriteCount = LogEventBatchSize + 2;

            // Act
            wrapper.WriteAsyncLogEvent(LogEventInfo.Create(LogLevel.Info, "", null, 1).WithContinuation(exceptions.Add));
            wrapper.WriteAsyncLogEvents(new[] { LogEventInfo.Create(LogLevel.Info, "", null, 2).WithContinuation(exceptions.Add) });
            wrapper.WriteAsyncLogEvents(Enumerable.Range(3, LogEventBatchSize).Select(i => LogEventInfo.Create(LogLevel.Info, "", null, i).WithContinuation(exceptions.Add)).ToArray());
            wrapper.Flush(ex => { exceptions.Add(ex); flushComplete.Set(); });
            flushComplete.WaitOne(5000);

            // Assert
            Assert.Equal(LogEventWriteCount + 1, exceptions.Count); // +1 because of flush
            Assert.Equal(LogEventWriteCount + 1 - logEventFailCount, exceptions.Count(ex => ex == null));
            foreach (var target in targets)
            {
                var myTarget = lookupTarget(target);
                Assert.Equal(LogEventWriteCount, myTarget.WrittenEvents.Count);
                Assert.Equal(1, myTarget.FlushCount);
                Assert.Equal(myTarget.WrittenEvents, myTarget.WrittenEvents.OrderBy(l => l.FormattedMessage).ToList());
            }
        }

        [Fact]
        public void SplitGroupToStringTest()
        {
            var myTarget1 = new MyTarget();
            var myTarget2 = new FileTarget("file1");
            var myTarget3 = new ConsoleTarget("Console2");

            var wrapper = new SplitGroupTarget()
            {
                Targets = { myTarget1, myTarget2, myTarget3 },
            };

            Assert.Equal("SplitGroup[MyTarget, FileTarget(Name=file1), ConsoleTarget(Name=Console2)]", wrapper.ToString());
        }

        public class MyTarget : Target
        {
            public MyTarget()
            {
                WrittenEvents = new List<LogEventInfo>();
            }

            public MyTarget(string name) : this()
            {
                Name = name;
            }

            public int FlushCount { get; set; }
            public int WriteCount { get; set; }
            public int FailCounter { get; set; }
            public List<LogEventInfo> WrittenEvents { get; }

            protected override void Write(LogEventInfo logEvent)
            {
                Assert.True(FlushCount <= WriteCount);
                lock (WrittenEvents)
                {
                    WriteCount++;
                    WrittenEvents.Add(logEvent);
                }

                if (FailCounter > 0)
                {
                    FailCounter--;
                    throw new ApplicationException("Some failure.");
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
