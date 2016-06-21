// 
// Copyright (c) 2004-2016 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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
    using Xunit;

    public class LimitingTargetWrapperTests : NLogTestBase
    {
        [Fact]
        public void WriteMessagesLessThanMessageLimitWritesToWrappedTarget()
        {
            MyTarget wrappedTarget = new MyTarget();
            LimitingTargetWrapper wrapper = new LimitingTargetWrapper(wrappedTarget, 5, TimeSpan.FromSeconds(1));
            InitializeTargets(wrappedTarget, wrapper);

            Exception lastException = null;
            // Write limit number of messages should just write them to the wrappedTarget.
            for (int i = 0; i < 5; i++)
            {
                wrapper.WriteAsyncLogEvent(
                    new LogEventInfo(LogLevel.Debug, "test", $"Hello {i}").WithContinuation(ex => lastException = ex));
            }

            Assert.Equal(5, wrappedTarget.WriteCount);

            //Let the interval expire to start a new one.
            Thread.Sleep(1000);

            // Write limit number of messages should just write them to the wrappedTarget.
            for (int i = 5; i < 10; i++)
            {
                wrapper.WriteAsyncLogEvent(
                    new LogEventInfo(LogLevel.Debug, "test", $"Hello {i}").WithContinuation(ex => lastException = ex));
            }

            // We should have 10 messages (5 from first interval, 5 from second interval).
            Assert.Equal(10, wrappedTarget.WriteCount);
            Assert.Null(lastException);
        }

        [Fact]
        public void WriteMoreMessagesThanMessageLimitDiscardsExcessMessages()
        {
            MyTarget wrappedTarget = new MyTarget();
            LimitingTargetWrapper wrapper = new LimitingTargetWrapper(wrappedTarget, 5, TimeSpan.FromHours(1));
            InitializeTargets(wrappedTarget, wrapper);
            Exception lastException = null;

            // Write limit number of messages should just write them to the wrappedTarget.
            for (int i = 0; i < 5; i++)
            {
                wrapper.WriteAsyncLogEvent(
                    new LogEventInfo(LogLevel.Debug, "test", $"Hello {i}").WithContinuation(ex => lastException = ex));
            }
            Assert.Equal(5, wrappedTarget.WriteCount);

            //Additional messages will be discarded, but InternalLogger will write to trace.
            string internalLog = RunAndCaptureInternalLog(() =>
            {
                wrapper.WriteAsyncLogEvent(
                    new LogEventInfo(LogLevel.Debug, "test", $"Hello {5}").WithContinuation(ex => lastException = ex));
            }, LogLevel.Trace);

            Assert.Equal(5, wrappedTarget.WriteCount);
            Assert.True(internalLog.Contains("MessageLimit"));
            Assert.Null(lastException);
        }

        [Fact]
        public void WriteMessageAfterIntervalHasExpiredStartsNewInterval()
        {
            MyTarget wrappedTarget = new MyTarget();
            LimitingTargetWrapper wrapper = new LimitingTargetWrapper(wrappedTarget, 5, TimeSpan.FromSeconds(1));
            InitializeTargets(wrappedTarget, wrapper);
            Exception lastException = null;
            wrapper.WriteAsyncLogEvent(
                new LogEventInfo(LogLevel.Debug, "test", "first interval").WithContinuation(ex => lastException = ex));

            //Let the interval expire.
            Thread.Sleep(1000);

            //Writing a logEvent should start a new Interval. This should be written to InternalLogger.Debug.
            string internalLog = RunAndCaptureInternalLog(() =>
            {
                // We can write 5 messages again since a new interval started.
                for (int i = 0; i < 5; i++)
                {
                    wrapper.WriteAsyncLogEvent(
                        new LogEventInfo(LogLevel.Debug, "test", $"Hello {i}").WithContinuation(ex => lastException = ex));
                }
            }, LogLevel.Trace);

            //We should have written 6 messages (1 in first interval and 5 in second interval).
            Assert.Equal(6, wrappedTarget.WriteCount);
            Assert.True(internalLog.Contains("new interval"));
            Assert.Null(lastException);
        }

        [Fact]
        public void TestWritingMessagesOverMultipleIntervals()
        {
            MyTarget wrappedTarget = new MyTarget();
            LimitingTargetWrapper wrapper = new LimitingTargetWrapper(wrappedTarget, 5, TimeSpan.FromSeconds(1));
            InitializeTargets(wrappedTarget, wrapper);
            Exception lastException = null;

            for (int i = 0; i < 10; i++)
            {
                wrapper.WriteAsyncLogEvent(
                    new LogEventInfo(LogLevel.Debug, "test", $"Hello {i}").WithContinuation(ex => lastException = ex));
            }

            //Let the interval expire.
            Thread.Sleep(1000);

            Assert.Equal(5, wrappedTarget.WriteCount);
            Assert.Equal("Hello 4", wrappedTarget.LastWrittenMessage);

            for (int i = 10; i < 20; i++)
            {
                wrapper.WriteAsyncLogEvent(
                    new LogEventInfo(LogLevel.Debug, "test", $"Hello {i}").WithContinuation(ex => lastException = ex));
            }
            //We should have 10 messages (5 from first, 5 from second interval).
            Assert.Equal(10, wrappedTarget.WriteCount);
            Assert.Equal("Hello 14", wrappedTarget.LastWrittenMessage);

            //Let the interval expire.
            Thread.Sleep(2300);

            for (int i = 20; i < 30; i++)
            {
                wrapper.WriteAsyncLogEvent(
                    new LogEventInfo(LogLevel.Debug, "test", $"Hello {i}").WithContinuation(ex => lastException = ex));
            }

            //We should have 15 messages (5 from first, 5 from second, 5 from third interval).
            Assert.Equal(15, wrappedTarget.WriteCount);
            Assert.Equal("Hello 24", wrappedTarget.LastWrittenMessage);

            //Let the interval expire.
            Thread.Sleep(200);
            for (int i = 30; i < 40; i++)
            {
                wrapper.WriteAsyncLogEvent(
                    new LogEventInfo(LogLevel.Debug, "test", $"Hello {i}").WithContinuation(ex => lastException = ex));
            }

            //No more messages shouldve been written, since we are still in the third interval.
            Assert.Equal(15, wrappedTarget.WriteCount);
            Assert.Equal("Hello 24", wrappedTarget.LastWrittenMessage);
            Assert.Null(lastException);
        }

        [Fact]
        public void ConstructorWithNoParametersInitialisesDefaultsCorrectly()
        {
            MyTarget wrappedTarget = new MyTarget();
            LimitingTargetWrapper wrapper = new LimitingTargetWrapper();

            Assert.Equal(1000, wrapper.MessageLimit);
            Assert.Equal(TimeSpan.FromHours(1), wrapper.Interval);
        }

        [Fact]
        public void ConstructorWithTargetInitialisesDefaultsCorrectly()
        {
            MyTarget wrappedTarget = new MyTarget();
            LimitingTargetWrapper wrapper = new LimitingTargetWrapper(wrappedTarget);

            Assert.Equal(1000, wrapper.MessageLimit);
            Assert.Equal(TimeSpan.FromHours(1), wrapper.Interval);
        }

        [Fact]
        public void ConstructorWithNameInitialisesDefaultsCorrectly()
        {
            MyTarget wrappedTarget = new MyTarget();
            LimitingTargetWrapper wrapper = new LimitingTargetWrapper("Wrapper", wrappedTarget);

            Assert.Equal(1000, wrapper.MessageLimit);
            Assert.Equal(TimeSpan.FromHours(1), wrapper.Interval);
        }

        [Fact]
        public void InitializeThrowsNLogConfigurationExceptionIfMessageLimitIsSetToZero()
        {
            MyTarget wrappedTarget = new MyTarget();
            LimitingTargetWrapper wrapper = new LimitingTargetWrapper(wrappedTarget) {MessageLimit = 0};
            wrappedTarget.Initialize(null);
            LogManager.ThrowConfigExceptions = true;

            Assert.Throws<NLogConfigurationException>(() => wrapper.Initialize(null));
            LogManager.ThrowConfigExceptions = false;
        }

        [Fact]
        public void InitializeThrowsNLogConfigurationExceptionIfMessageLimitIsSmallerZero()
        {
            MyTarget wrappedTarget = new MyTarget();
            LimitingTargetWrapper wrapper = new LimitingTargetWrapper(wrappedTarget) {MessageLimit = -1};
            wrappedTarget.Initialize(null);
            LogManager.ThrowConfigExceptions = true;

            Assert.Throws<NLogConfigurationException>(() => wrapper.Initialize(null));
            LogManager.ThrowConfigExceptions = false;
        }

        [Fact]
        public void InitializeThrowsNLogConfigurationExceptionIfIntervalIsSmallerZero()
        {
            MyTarget wrappedTarget = new MyTarget();
            LimitingTargetWrapper wrapper = new LimitingTargetWrapper(wrappedTarget) {Interval = TimeSpan.MinValue};
            wrappedTarget.Initialize(null);
            LogManager.ThrowConfigExceptions = true;

            Assert.Throws<NLogConfigurationException>(() => wrapper.Initialize(null));
            LogManager.ThrowConfigExceptions = false;
        }

        [Fact]
        public void InitializeThrowsNLogConfigurationExceptionIfIntervalIsZero()
        {
            MyTarget wrappedTarget = new MyTarget();
            LimitingTargetWrapper wrapper = new LimitingTargetWrapper(wrappedTarget) {Interval = TimeSpan.Zero};
            wrappedTarget.Initialize(null);
            LogManager.ThrowConfigExceptions = true;

            Assert.Throws<NLogConfigurationException>(() => wrapper.Initialize(null));
            LogManager.ThrowConfigExceptions = false;
        }


        private static void InitializeTargets(params Target[] targets)
        {
            foreach (Target target in targets)
            {
                target.Initialize(null);
            }
        }

        private class MyTarget : Target
        {
            public int WriteCount { get; private set; }
            public string LastWrittenMessage { get; private set; }


            protected override void Write(AsyncLogEventInfo logEvent)
            {
                base.Write(logEvent);
                LastWrittenMessage = logEvent.LogEvent.FormattedMessage;
                WriteCount++;
            }

        }

    }
}