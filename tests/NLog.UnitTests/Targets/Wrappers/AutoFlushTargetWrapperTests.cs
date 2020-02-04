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

using NLog.Config;

namespace NLog.UnitTests.Targets.Wrappers
{
    using System;
    using System.Threading;
    using NLog.Common;
    using NLog.Targets;
    using NLog.Targets.Wrappers;
    using Xunit;

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
        public void AutoFlushTargetWrapperAsyncTest2()
        {
            var myTarget = new MyAsyncTarget();
            var wrapper = new AutoFlushTargetWrapper(myTarget);
            myTarget.Initialize(null);
            wrapper.Initialize(null);
            var logEvent = new LogEventInfo();
            Exception lastException = null;

            for (int i = 0; i < 100; ++i)
            {
                wrapper.WriteAsyncLogEvent(logEvent.WithContinuation(ex => lastException = ex));
            }

            var continuationHit = new ManualResetEvent(false);
            AsyncContinuation continuation =
                ex =>
                {
                    continuationHit.Set();
                };

            wrapper.Flush(ex => { });
            Assert.Null(lastException);
            wrapper.Flush(continuation);
            Assert.Null(lastException);
            continuationHit.WaitOne();
            Assert.Null(lastException);
            wrapper.Flush(ex => { });   // Executed right away
            Assert.Null(lastException);
            Assert.Equal(100, myTarget.WriteCount);
            Assert.Equal(103, myTarget.FlushCount);
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
            Assert.IsType<InvalidOperationException>(lastException);

            // no flush on exception
            Assert.Equal(0, myTarget.FlushCount);
            Assert.Equal(1, myTarget.WriteCount);

            continuationHit.Reset();
            lastException = null;
            wrapper.WriteAsyncLogEvent(logEvent.WithContinuation(continuation));
            continuationHit.WaitOne();
            Assert.NotNull(lastException);
            Assert.IsType<InvalidOperationException>(lastException);
            Assert.Equal(0, myTarget.FlushCount);
            Assert.Equal(2, myTarget.WriteCount);
        }

        [Fact]
        public void AutoFlushConditionConfigurationTest()
        {
            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"<nlog>
                    <targets>
                        <target type='AutoFlushWrapper' condition='level >= LogLevel.Debug' name='FlushOnError'>
                    <target name='d2' type='Debug' />
                        </target>
                    </targets>
                    <rules>
                        <logger name='*' level='Warn' writeTo='FlushOnError'>
                        </logger>
                    </rules>
                  </nlog>");
            var target = LogManager.Configuration.FindTargetByName("FlushOnError") as AutoFlushTargetWrapper;
            Assert.NotNull(target);
            Assert.NotNull(target.Condition);
            Assert.Equal("(level >= Debug)", target.Condition.ToString());
            Assert.Equal("d2", target.WrappedTarget.Name);
        }

        [Fact]
        public void AutoFlushOnConditionTest()
        {
            var testTarget = new MyTarget();
            var autoFlushWrapper = new AutoFlushTargetWrapper(testTarget);
            autoFlushWrapper.Condition = "level > LogLevel.Info";
            testTarget.Initialize(null);
            autoFlushWrapper.Initialize(null);
            AsyncContinuation continuation = ex => { };
            autoFlushWrapper.WriteAsyncLogEvent(LogEventInfo.Create(LogLevel.Info, "*", "test").WithContinuation(continuation));
            autoFlushWrapper.WriteAsyncLogEvent(LogEventInfo.Create(LogLevel.Trace, "*", "test").WithContinuation(continuation));
            Assert.Equal(2, testTarget.WriteCount);
            Assert.Equal(0, testTarget.FlushCount);
            autoFlushWrapper.WriteAsyncLogEvent(LogEventInfo.Create(LogLevel.Warn, "*", "test").WithContinuation(continuation));
            autoFlushWrapper.WriteAsyncLogEvent(LogEventInfo.Create(LogLevel.Error, "*", "test").WithContinuation(continuation));
            Assert.Equal(4, testTarget.WriteCount);
            Assert.Equal(2, testTarget.FlushCount);
        }

        [Fact]
        public void MultipleConditionalAutoFlushWrappersTest()
        {
            var testTarget = new MyTarget();
            var autoFlushOnLevelWrapper = new AutoFlushTargetWrapper(testTarget);
            autoFlushOnLevelWrapper.Condition = "level > LogLevel.Info";
            var autoFlushOnMessageWrapper = new AutoFlushTargetWrapper(autoFlushOnLevelWrapper);
            autoFlushOnMessageWrapper.Condition = "contains('${message}','FlushThis')";
            testTarget.Initialize(null);
            autoFlushOnLevelWrapper.Initialize(null);
            autoFlushOnMessageWrapper.Initialize(null);

            AsyncContinuation continuation = ex => { };
            autoFlushOnMessageWrapper.WriteAsyncLogEvent(LogEventInfo.Create(LogLevel.Trace, "*", "test").WithContinuation(continuation));
            Assert.Equal(1, testTarget.WriteCount);
            Assert.Equal(0, testTarget.FlushCount);
            autoFlushOnMessageWrapper.WriteAsyncLogEvent(LogEventInfo.Create(LogLevel.Fatal, "*", "test").WithContinuation(continuation));
            Assert.Equal(2, testTarget.WriteCount);
            Assert.Equal(1, testTarget.FlushCount);
            autoFlushOnMessageWrapper.WriteAsyncLogEvent(LogEventInfo.Create(LogLevel.Trace, "*", "Please FlushThis").WithContinuation(continuation));
            Assert.Equal(3, testTarget.WriteCount);
            Assert.Equal(2, testTarget.FlushCount);
        }

        [Fact]
        public void BufferingAutoFlushWrapperTest()
        {
            var testTarget = new MyTarget();
            var bufferingTargetWrapper = new BufferingTargetWrapper(testTarget, 100);
            var autoFlushOnLevelWrapper = new AutoFlushTargetWrapper(bufferingTargetWrapper);
            autoFlushOnLevelWrapper.Condition = "level > LogLevel.Info";
            testTarget.Initialize(null);
            bufferingTargetWrapper.Initialize(null);
            autoFlushOnLevelWrapper.Initialize(null);

            AsyncContinuation continuation = ex => { };
            autoFlushOnLevelWrapper.WriteAsyncLogEvent(LogEventInfo.Create(LogLevel.Trace, "*", "test").WithContinuation(continuation));
            Assert.Equal(0, testTarget.WriteCount);
            Assert.Equal(0, testTarget.FlushCount);
            autoFlushOnLevelWrapper.WriteAsyncLogEvent(LogEventInfo.Create(LogLevel.Fatal, "*", "test").WithContinuation(continuation));
            Assert.Equal(2, testTarget.WriteCount);
            Assert.Equal(1, testTarget.FlushCount);
            autoFlushOnLevelWrapper.WriteAsyncLogEvent(LogEventInfo.Create(LogLevel.Trace, "*", "Please do not FlushThis").WithContinuation(continuation));
            Assert.Equal(2, testTarget.WriteCount);
            Assert.Equal(1, testTarget.FlushCount);
            autoFlushOnLevelWrapper.Flush(continuation);
            Assert.Equal(3, testTarget.WriteCount);
            Assert.Equal(2, testTarget.FlushCount);
        }

        [Fact]
        public void IgnoreExplicitAutoFlushWrapperTest()
        {
            var testTarget = new MyTarget();
            var bufferingTargetWrapper = new BufferingTargetWrapper(testTarget, 100);
            var autoFlushOnLevelWrapper = new AutoFlushTargetWrapper(bufferingTargetWrapper);
            autoFlushOnLevelWrapper.Condition = "level > LogLevel.Info";
            autoFlushOnLevelWrapper.FlushOnConditionOnly = true;
            testTarget.Initialize(null);
            bufferingTargetWrapper.Initialize(null);
            autoFlushOnLevelWrapper.Initialize(null);

            AsyncContinuation continuation = ex => { };
            autoFlushOnLevelWrapper.WriteAsyncLogEvent(LogEventInfo.Create(LogLevel.Trace, "*", "test").WithContinuation(continuation));
            Assert.Equal(0, testTarget.WriteCount);
            Assert.Equal(0, testTarget.FlushCount);
            autoFlushOnLevelWrapper.WriteAsyncLogEvent(LogEventInfo.Create(LogLevel.Fatal, "*", "test").WithContinuation(continuation));
            Assert.Equal(2, testTarget.WriteCount);
            Assert.Equal(1, testTarget.FlushCount);
            autoFlushOnLevelWrapper.WriteAsyncLogEvent(LogEventInfo.Create(LogLevel.Trace, "*", "Please do not FlushThis").WithContinuation(continuation));
            Assert.Equal(2, testTarget.WriteCount);
            Assert.Equal(1, testTarget.FlushCount);
            autoFlushOnLevelWrapper.Flush(continuation);
            Assert.Equal(2, testTarget.WriteCount);
            Assert.Equal(1, testTarget.FlushCount);
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

            protected override void Write(LogEventInfo logEvent)
            {
                Assert.True(FlushCount <= WriteCount);
                WriteCount++;
            }

            protected override void FlushAsync(AsyncContinuation asyncContinuation)
            {
                FlushCount++;
                asyncContinuation(null);
            }
        }
    }
}
