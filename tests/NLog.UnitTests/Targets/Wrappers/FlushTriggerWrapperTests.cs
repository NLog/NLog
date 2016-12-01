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
    using NLog.Common;
    using NLog.Targets;
    using NLog.Targets.Wrappers;
    using Xunit;

    public class FlushTriggerWrapperTests : NLogTestBase
    {
        [Fact]
        public void FlushTriggerWrapperConfigurationReadTest()
        {
            LogManager.Configuration = CreateConfigurationFromString(@"<nlog>
                <targets>
                    <target type='FlushTriggerWrapper' condition='level >= LogLevel.Debug' name='FlushOnError'>
                        <target name='d2' type='Debug' />
                    </target>
                </targets>

                <rules>
                    <logger name='*' level='Warn' writeTo='FlushOnError'>
                    </logger>
                </rules>
            </nlog>");
            var target = LogManager.Configuration.FindTargetByName("FlushOnError") as FlushTriggerWrapper;
            Assert.NotNull(target);
            Assert.NotNull(target.Condition);
            Assert.Equal("(level >= Debug)", target.Condition.ToString());
            Assert.Equal("d2", target.WrappedTarget.Name);
        }

        [Fact]
        public void FlushTriggerWrapperConditionMissingErrorTest()
        {
            var exception = Assert.Throws<NLogConfigurationException>(() =>
            {
                LogManager.Configuration = CreateConfigurationFromString(@"<nlog throwExceptions='true'>
                <targets>
                    <target type='FlushTriggerWrapper' name='FlushOnError'>
                        <target name='d2' type='Debug' />
                    </target>

                </targets>

                <rules>
                    <logger name='*' level='Warn' writeTo='FlushOnError'>
                    </logger>
                </rules>
            </nlog>");
            });

            Assert.Contains("Required parameter 'Condition'", exception.Message);
        }

        [Fact]
        public void FlushTriggerWrapperFlushOnConditionTest()
        {
            var testTarget = new TestTarget();
            var flushTriggerWrapper = new FlushTriggerWrapper(testTarget);
            flushTriggerWrapper.Condition = "level > LogLevel.Info";
            testTarget.Initialize(null);
            flushTriggerWrapper.Initialize(null);
            AsyncContinuation continuation = ex => { };
            flushTriggerWrapper.WriteAsyncLogEvent(LogEventInfo.Create(LogLevel.Info, "*", "test").WithContinuation(continuation));
            flushTriggerWrapper.WriteAsyncLogEvent(LogEventInfo.Create(LogLevel.Trace, "*", "test").WithContinuation(continuation));
            Assert.Equal(2, testTarget.WriteCount);
            Assert.Equal(0, testTarget.FlushCount);
            flushTriggerWrapper.WriteAsyncLogEvent(LogEventInfo.Create(LogLevel.Warn, "*", "test").WithContinuation(continuation));
            flushTriggerWrapper.WriteAsyncLogEvent(LogEventInfo.Create(LogLevel.Error, "*", "test").WithContinuation(continuation));
            Assert.Equal(4, testTarget.WriteCount);
            Assert.Equal(2, testTarget.FlushCount);
        }

        [Fact]
        public void FlushTriggerWrapperMultipleWrappersTest()
        {
            var testTarget = new TestTarget();
            var flushTriggerWrapperLevel = new FlushTriggerWrapper(testTarget);
            flushTriggerWrapperLevel.Condition = "level > LogLevel.Info";
            var flushTriggerWrapperMessage = new FlushTriggerWrapper(flushTriggerWrapperLevel);
            flushTriggerWrapperMessage.Condition = "contains('${message}','FlushThis')";
            testTarget.Initialize(null);
            flushTriggerWrapperLevel.Initialize(null);
            flushTriggerWrapperMessage.Initialize(null);

            AsyncContinuation continuation = ex => { };
            flushTriggerWrapperMessage.WriteAsyncLogEvent(LogEventInfo.Create(LogLevel.Trace, "*", "test").WithContinuation(continuation));
            Assert.Equal(1, testTarget.WriteCount);
            Assert.Equal(0, testTarget.FlushCount);
            flushTriggerWrapperMessage.WriteAsyncLogEvent(LogEventInfo.Create(LogLevel.Fatal, "*", "test").WithContinuation(continuation));
            Assert.Equal(2, testTarget.WriteCount);
            Assert.Equal(1, testTarget.FlushCount);
            flushTriggerWrapperMessage.WriteAsyncLogEvent(LogEventInfo.Create(LogLevel.Trace, "*", "Please FlushThis").WithContinuation(continuation));
            Assert.Equal(3, testTarget.WriteCount);
            Assert.Equal(2, testTarget.FlushCount);
        }


        class TestTarget : Target
        {
            public int FlushCount { get; set; }
            public int WriteCount { get; set; }

            protected override void Write(LogEventInfo logEvent)
            {
                this.WriteCount++;
            }

            protected override void FlushAsync(AsyncContinuation asyncContinuation)
            {
                this.FlushCount++;
                asyncContinuation(null);
            }
        }
    }
}