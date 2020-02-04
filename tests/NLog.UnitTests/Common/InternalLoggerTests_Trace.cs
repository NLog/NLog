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

#define DEBUG

using NLog.Config;

namespace NLog.UnitTests.Common
{

    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using NLog.Common;
    using Xunit;
    using Xunit.Extensions;

    public class InternalLoggerTests_Trace : NLogTestBase
    {

        [Theory]
        [InlineData(null, null)]
        [InlineData(false, null)]
        [InlineData(null, false)]
        [InlineData(false, false)]
        public void ShouldNotLogInternalWhenLogToTraceIsDisabled(bool? internalLogToTrace, bool? logToTrace)
        {
            var mockTraceListener = SetupTestConfiguration<MockTraceListener>(LogLevel.Trace, internalLogToTrace, logToTrace);

            InternalLogger.Trace("Logger1 Hello");

            Assert.Empty(mockTraceListener.Messages);
        }

        [Theory]
        [InlineData(null, null)]
        [InlineData(false, null)]
        [InlineData(null, false)]
        [InlineData(false, false)]
        [InlineData(true, null)]
        [InlineData(null, true)]
        [InlineData(true, true)]
        public void ShouldNotLogInternalWhenLogLevelIsOff(bool? internalLogToTrace, bool? logToTrace)
        {
            var mockTraceListener = SetupTestConfiguration<MockTraceListener>(LogLevel.Off, internalLogToTrace, logToTrace);

            InternalLogger.Trace("Logger1 Hello");

            Assert.Empty(mockTraceListener.Messages);
        }

        [Theory]
        [InlineData(true, null)]
        [InlineData(null, true)]
        [InlineData(true, true)]
        public void VerifyInternalLoggerLevelFilter(bool? internalLogToTrace, bool? logToTrace)
        {
            foreach (LogLevel logLevelConfig in LogLevel.AllLevels)
            {
                var mockTraceListener = SetupTestConfiguration<MockTraceListener>(logLevelConfig, internalLogToTrace, logToTrace);

                List<string> expected = new List<string>();

                string input = "Logger1 Hello";
                expected.Add(input);
                InternalLogger.Trace(input);
                InternalLogger.Debug(input);
                InternalLogger.Info(input);
                InternalLogger.Warn(input);
                InternalLogger.Error(input);
                InternalLogger.Fatal(input);
                input += "No.{0}";
                expected.Add(string.Format(input, 1));
                InternalLogger.Trace(input, 1);
                InternalLogger.Debug(input, 1);
                InternalLogger.Info(input, 1);
                InternalLogger.Warn(input, 1);
                InternalLogger.Error(input, 1);
                InternalLogger.Fatal(input, 1);
                input += ", We come in {1}";
                expected.Add(string.Format(input, 1, "Peace"));
                InternalLogger.Trace(input, 1, "Peace");
                InternalLogger.Debug(input, 1, "Peace");
                InternalLogger.Info(input, 1, "Peace");
                InternalLogger.Warn(input, 1, "Peace");
                InternalLogger.Error(input, 1, "Peace");
                InternalLogger.Fatal(input, 1, "Peace");
                input += " and we are {2} to god";
                expected.Add(string.Format(input, 1, "Peace", true));
                InternalLogger.Trace(input, 1, "Peace", true);
                InternalLogger.Debug(input, 1, "Peace", true);
                InternalLogger.Info(input, 1, "Peace", true);
                InternalLogger.Warn(input, 1, "Peace", true);
                InternalLogger.Error(input, 1, "Peace", true);
                InternalLogger.Fatal(input, 1, "Peace", true);
                input += ", Please don't {3}";
                expected.Add(string.Format(input, 1, "Peace", true, null));
                InternalLogger.Trace(input, 1, "Peace", true, null);
                InternalLogger.Debug(input, 1, "Peace", true, null);
                InternalLogger.Info(input, 1, "Peace", true, null);
                InternalLogger.Warn(input, 1, "Peace", true, null);
                InternalLogger.Error(input, 1, "Peace", true, null);
                InternalLogger.Fatal(input, 1, "Peace", true, null);
                input += " the {4}";
                expected.Add(string.Format(input, 1, "Peace", true, null, "Messenger"));
                InternalLogger.Trace(input, 1, "Peace", true, null, "Messenger");
                InternalLogger.Debug(input, 1, "Peace", true, null, "Messenger");
                InternalLogger.Info(input, 1, "Peace", true, null, "Messenger");
                InternalLogger.Warn(input, 1, "Peace", true, null, "Messenger");
                InternalLogger.Error(input, 1, "Peace", true, null, "Messenger");
                InternalLogger.Fatal(input, 1, "Peace", true, null, "Messenger");
                Assert.Equal(expected.Count * (LogLevel.Fatal.Ordinal - logLevelConfig.Ordinal + 1), mockTraceListener.Messages.Count);
                for (int i = 0; i < expected.Count; ++i)
                {
                    int msgCount = LogLevel.Fatal.Ordinal - logLevelConfig.Ordinal + 1;
                    for (int j = 0; j < msgCount; ++j)
                    {
                        Assert.Contains(expected[i], mockTraceListener.Messages.First());
                        mockTraceListener.Messages.RemoveAt(0);
                    }
                }
                Assert.Empty(mockTraceListener.Messages);
            }
        }

        [Theory]
        [InlineData(true, null)]
        [InlineData(null, true)]
        [InlineData(true, true)]
        public void ShouldLogToTraceWhenInternalLogToTraceIsOnAndLogLevelIsTrace(bool? internalLogToTrace, bool? logToTrace)
        {
            var mockTraceListener = SetupTestConfiguration<MockTraceListener>(LogLevel.Trace, internalLogToTrace, logToTrace);

            InternalLogger.Trace("Logger1 Hello");

            Assert.Single(mockTraceListener.Messages);
            Assert.Equal("NLog: Trace Logger1 Hello" + Environment.NewLine, mockTraceListener.Messages.First());
        }

        [Theory]
        [InlineData(true, null)]
        [InlineData(null, true)]
        [InlineData(true, true)]
        public void ShouldLogToTraceWhenInternalLogToTraceIsOnAndLogLevelIsDebug(bool? internalLogToTrace, bool? logToTrace)
        {
            var mockTraceListener = SetupTestConfiguration<MockTraceListener>(LogLevel.Debug, internalLogToTrace, logToTrace);

            InternalLogger.Debug("Logger1 Hello");

            Assert.Single(mockTraceListener.Messages);
            Assert.Equal("NLog: Debug Logger1 Hello" + Environment.NewLine, mockTraceListener.Messages.First());
        }

        [Theory]
        [InlineData(true, null)]
        [InlineData(null, true)]
        [InlineData(true, true)]
        public void ShouldLogToTraceWhenInternalLogToTraceIsOnAndLogLevelIsInfo(bool? internalLogToTrace, bool? logToTrace)
        {
            var mockTraceListener = SetupTestConfiguration<MockTraceListener>(LogLevel.Info, internalLogToTrace, logToTrace);

            InternalLogger.Info("Logger1 Hello");

            Assert.Single(mockTraceListener.Messages);
            Assert.Equal("NLog: Info Logger1 Hello" + Environment.NewLine, mockTraceListener.Messages.First());
        }

        [Theory]
        [InlineData(true, null)]
        [InlineData(null, true)]
        [InlineData(true, true)]
        public void ShouldLogToTraceWhenInternalLogToTraceIsOnAndLogLevelIsWarn(bool? internalLogToTrace, bool? logToTrace)
        {
            var mockTraceListener = SetupTestConfiguration<MockTraceListener>(LogLevel.Warn, internalLogToTrace, logToTrace);

            InternalLogger.Warn("Logger1 Hello");

            Assert.Single(mockTraceListener.Messages);
            Assert.Equal("NLog: Warn Logger1 Hello" + Environment.NewLine, mockTraceListener.Messages.First());
        }

        [Theory]
        [InlineData(true, null)]
        [InlineData(null, true)]
        [InlineData(true, true)]
        public void ShouldLogToTraceWhenInternalLogToTraceIsOnAndLogLevelIsError(bool? internalLogToTrace, bool? logToTrace)
        {
            var mockTraceListener = SetupTestConfiguration<MockTraceListener>(LogLevel.Error, internalLogToTrace, logToTrace);

            InternalLogger.Error("Logger1 Hello");

            Assert.Single(mockTraceListener.Messages);
            Assert.Equal("NLog: Error Logger1 Hello" + Environment.NewLine, mockTraceListener.Messages.First());
        }

        [Theory]
        [InlineData(true, null)]
        [InlineData(null, true)]
        [InlineData(true, true)]
        public void ShouldLogToTraceWhenInternalLogToTraceIsOnAndLogLevelIsFatal(bool? internalLogToTrace, bool? logToTrace)
        {
            var mockTraceListener = SetupTestConfiguration<MockTraceListener>(LogLevel.Fatal, internalLogToTrace, logToTrace);

            InternalLogger.Fatal("Logger1 Hello");

            Assert.Single(mockTraceListener.Messages);
            Assert.Equal("NLog: Fatal Logger1 Hello" + Environment.NewLine, mockTraceListener.Messages.First());
        }

#if !NETSTANDARD1_5
        [Fact(Skip = "This test's not working - explenation is in documentation: https://msdn.microsoft.com/pl-pl/library/system.stackoverflowexception(v=vs.110).aspx#Anchor_5. To clarify if StackOverflowException should be thrown.")]
        public void ShouldThrowStackOverFlowExceptionWhenUsingNLogTraceListener()
        {
            SetupTestConfiguration<NLogTraceListener>(LogLevel.Trace, true, null);

            Assert.Throws<StackOverflowException>(() => Trace.WriteLine("StackOverFlowException"));
        }
#endif

        /// <summary>
        /// Helper method to setup tests configuration
        /// </summary>
        /// <param name="logLevel">The <see cref="NLog.LogLevel"/> for the log event.</param>
        /// <param name="internalLogToTrace">internalLogToTrace XML attribute value. If <c>null</c> attribute is omitted.</param>
        /// <param name="logToTrace">Value of <see cref="InternalLogger.LogToTrace"/> property. If <c>null</c> property is not set.</param>
        /// <returns><see cref="TraceListener"/> instance.</returns>
        private T SetupTestConfiguration<T>(LogLevel logLevel, bool? internalLogToTrace, bool? logToTrace) where T : TraceListener
        {
            var internalLogToTraceAttribute = "";
            if (internalLogToTrace.HasValue)
            {
                internalLogToTraceAttribute = $" internalLogToTrace='{internalLogToTrace.Value}'";
            }

            var xmlConfiguration = string.Format(XmlConfigurationFormat, logLevel, internalLogToTraceAttribute);

            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(xmlConfiguration);

            InternalLogger.IncludeTimestamp = false;

            if (logToTrace.HasValue)
            {
                InternalLogger.LogToTrace = logToTrace.Value;
            }

            T traceListener;
            if (typeof (T) == typeof (MockTraceListener))
            {
                traceListener = CreateMockTraceListener() as T;
            }
            else
            {
                traceListener = CreateNLogTraceListener() as T;
            }

            Trace.Listeners.Clear();

            if (traceListener == null)
            {
                return null;
            }

            Trace.Listeners.Add(traceListener);

            return traceListener;
        }

        private const string XmlConfigurationFormat = @"<nlog internalLogLevel='{0}'{1}>
    <targets>
        <target name='debug' type='Debug' layout='${{logger}} ${{level}} ${{message}}'/>
    </targets>
    <rules>
        <logger name='*' level='{0}' writeTo='debug'/>
    </rules>
</nlog>";

        /// <summary>
        /// Creates <see cref="MockTraceListener"/> instance.
        /// </summary>
        /// <returns><see cref="MockTraceListener"/> instance.</returns>
        private static MockTraceListener CreateMockTraceListener()
        {
            return new MockTraceListener();
        }

        /// <summary>
        /// Creates <see cref="NLogTraceListener"/> instance.
        /// </summary>
        /// <returns><see cref="NLogTraceListener"/> instance.</returns>
        private static TraceListener CreateNLogTraceListener()
        {
#if !NETSTANDARD1_5
            return new NLogTraceListener {Name = "Logger1", ForceLogLevel = LogLevel.Trace};
#else
            return null;
#endif
        }

        private class MockTraceListener : TraceListener
        {

            internal readonly List<string> Messages = new List<string>();

            /// <summary>
            /// When overridden in a derived class, writes the specified message to the listener you create in the derived class.
            /// </summary>
            /// <param name="message">A message to write. </param>
            public override void Write(string message)
            {
                Messages.Add(message);
            }

            /// <summary>
            /// When overridden in a derived class, writes a message to the listener you create in the derived class, followed by a line terminator.
            /// </summary>
            /// <param name="message">A message to write. </param>
            public override void WriteLine(string message)
            {
                Messages.Add(message + Environment.NewLine);
            }

        }

    }

}
