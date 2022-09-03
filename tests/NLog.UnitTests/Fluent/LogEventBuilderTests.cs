// 
// Copyright (c) 2004-2021 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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

namespace NLog.UnitTests.Fluent
{
    using System;
    using System.IO;
    using System.Collections.Generic;
    using NLog.Config;
    using NLog.Targets;
    using Xunit;

    public class LogEventBuilderTests : NLogTestBase
    {
        private static readonly Logger _logger = LogManager.GetLogger("logger1");

        private LogEventInfo _lastLogEventInfo;

        public LogEventBuilderTests()
        {
            var configuration = new LoggingConfiguration();

            var t1 = new MethodCallTarget("t1", (l, parms) => _lastLogEventInfo = l);
            t1.Parameters.Add(new MethodCallParameter("CallSite", "${callsite}"));
            var t2 = new DebugTarget { Name = "t2", Layout = "${message}" };
            configuration.AddTarget(t1);
            configuration.AddTarget(t2);
            configuration.LoggingRules.Add(new LoggingRule("*", LogLevel.Trace, t1));
            configuration.LoggingRules.Add(new LoggingRule("*", LogLevel.Trace, t2));

            LogManager.Configuration = configuration;
        }

        [Fact]
        public void TraceWrite()
        {
            LogWrite_internal(() => _logger.ForTraceEvent(), LogLevel.Trace);
        }

        [Fact]
        public void DebugWrite()
        {
            LogWrite_internal(() => _logger.ForDebugEvent(), LogLevel.Debug);
        }

        [Fact]
        public void InfoWrite()
        {
            LogWrite_internal(() => _logger.ForInfoEvent(), LogLevel.Info);
        }

        ///<remarks>
        /// func because 1 logbuilder creates 1 message
        /// 
        /// Caution: don't use overloading, that will break xUnit:
        /// CATASTROPHIC ERROR OCCURRED:
        /// System.ArgumentException: Ambiguous method named TraceWrite in type NLog.UnitTests.Fluent.LogBuilderTests
        /// </remarks>
        private void LogWrite_internal(Func<LogEventBuilder> logBuilder, LogLevel logLevel)
        {
            logBuilder()
                .Message("This is a test fluent message.")
                .Property("Test", "TraceWrite")
                .Log();

            var loggerName = "logger1";
            {
                var expectedEvent = new LogEventInfo(logLevel, loggerName, "This is a test fluent message.");
                expectedEvent.Properties["Test"] = "TraceWrite";
                AssertLastLogEventTarget(expectedEvent);
            }

            var ticks = DateTime.Now.Ticks;
            logBuilder()
                .Message("This is a test fluent message '{0}'.", ticks)
                .Property("Test", "TraceWrite")
                .Log();

            {
                var rendered = $"This is a test fluent message '{ticks}'.";
                var expectedEvent = new LogEventInfo(logLevel, loggerName, "This is a test fluent message '{0}'.");
                expectedEvent.Properties["Test"] = "TraceWrite";
                AssertLastLogEventTarget(expectedEvent);
                AssertDebugLastMessage("t2", rendered);
            }
        }

        [Fact]
        public void TraceWriteProperties()
        {
            var props = new Dictionary<string, object>
            {
                {"prop1", "1"},
                {"prop2", "2"},

            };

            _logger.ForTraceEvent()
                .Message("This is a test fluent message.")
                .Properties(props).Log();

            {
                var expectedEvent = new LogEventInfo(LogLevel.Trace, "logger1", "This is a test fluent message.");
                expectedEvent.Properties["prop1"] = "1";
                expectedEvent.Properties["prop2"] = "2";
                AssertLastLogEventTarget(expectedEvent);
            }
        }

        [Fact]
        public void WarnWriteProperties()
        {
            var props = new Dictionary<string, object>
            {
                {"prop1", "1"},
                {"prop2", "2"},

            };

            _logger.ForWarnEvent()
                .Message("This is a test fluent message.")
                .Properties(props).Log();

            {
                var expectedEvent = new LogEventInfo(LogLevel.Warn, "logger1", "This is a test fluent message.");
                expectedEvent.Properties["prop1"] = "1";
                expectedEvent.Properties["prop2"] = "2";
                AssertLastLogEventTarget(expectedEvent);
            }
        }

        [Fact]
        public void LogWriteProperties()
        {
            var props = new Dictionary<string, object>
            {
                {"prop1", "1"},
                {"prop2", "2"},
            };

            // Loop to verify caller-attribute-caching-lookup
            for (int i = 0; i < 2; ++i)
            {
                _logger.ForLogEvent(LogLevel.Fatal)
                    .Message("This is a test fluent message.")
                    .Properties(props).Log();

                var expectedEvent = new LogEventInfo(LogLevel.Fatal, "logger1", "This is a test fluent message.");
                expectedEvent.Properties["prop1"] = "1";
                expectedEvent.Properties["prop2"] = "2";
                AssertLastLogEventTarget(expectedEvent);

#if !NET35
                Assert.Equal(GetType().ToString(), _lastLogEventInfo.CallerClassName);
#endif
            }
        }

        [Fact]
        public void LogOffWriteProperties()
        {
            var props = new Dictionary<string, object>
            {
                {"prop1", "1"},
                {"prop2", "2"},

            };
            var props2 = new Dictionary<string, object>
            {
                {"prop1", "4"},
                {"prop2", "5"},
            };

            _logger.ForLogEvent(LogLevel.Fatal)
                .Message("This is a test fluent message.")
                .Properties(props).Log();

            _logger.ForLogEvent(LogLevel.Off)
                .Message("dont log this.")
                .Properties(props2).Log();

            {
                var expectedEvent = new LogEventInfo(LogLevel.Fatal, "logger1", "This is a test fluent message.");
                expectedEvent.Properties["prop1"] = "1";
                expectedEvent.Properties["prop2"] = "2";
                AssertLastLogEventTarget(expectedEvent);
            }
        }

        [Fact]
        public void TraceIfWrite()
        {
            _logger.ForTraceEvent()
                .Message("This is a test fluent message.1")
                .Property("Test", "TraceWrite")
                .Log();

            {
                var expectedEvent = new LogEventInfo(LogLevel.Trace, "logger1", "This is a test fluent message.1");
                expectedEvent.Properties["Test"] = "TraceWrite";
                AssertLastLogEventTarget(expectedEvent);
            }

            int v = 1;
            _logger.ForTraceEvent()
                .Message("This is a test fluent WriteIf message '{0}'.", DateTime.Now.Ticks)
                .Property("Test", "TraceWrite")
                .Log(v == 1 ? null : LogLevel.Off);


            {
                var expectedEvent = new LogEventInfo(LogLevel.Trace, "logger1", "This is a test fluent WriteIf message '{0}'.");
                expectedEvent.Properties["Test"] = "TraceWrite";
                AssertLastLogEventTarget(expectedEvent);
                AssertDebugLastMessageContains("t2", "This is a test fluent WriteIf message ");
            }

            _logger.ForTraceEvent()
                .Message("dont write this! '{0}'.", DateTime.Now.Ticks)
                .Property("Test", "TraceWrite")
                .Log(LogLevel.Off);

            {
                var expectedEvent = new LogEventInfo(LogLevel.Trace, "logger1", "This is a test fluent WriteIf message '{0}'.");
                expectedEvent.Properties["Test"] = "TraceWrite";
                AssertLastLogEventTarget(expectedEvent);
                AssertDebugLastMessageContains("t2", "This is a test fluent WriteIf message ");
            }

            _logger.ForTraceEvent()
                .Message("This is a test fluent WriteIf message '{0}'.", DateTime.Now.Ticks)
                .Property("Test", "TraceWrite")
                .Log(v == 1 ? null : LogLevel.Off);


            {
                var expectedEvent = new LogEventInfo(LogLevel.Trace, "logger1", "This is a test fluent WriteIf message '{0}'.");
                expectedEvent.Properties["Test"] = "TraceWrite";
                AssertLastLogEventTarget(expectedEvent);
                AssertDebugLastMessageContains("t2", "This is a test fluent WriteIf message ");
            }

            _logger.ForTraceEvent()
                .Message("Should Not WriteIf message '{0}'.", DateTime.Now.Ticks)
                .Property("Test", "TraceWrite")
                .Log(v > 1 ? null : LogLevel.Off);

            {
                //previous
                var expectedEvent = new LogEventInfo(LogLevel.Trace, "logger1", "This is a test fluent WriteIf message '{0}'.");
                expectedEvent.Properties["Test"] = "TraceWrite";
                AssertLastLogEventTarget(expectedEvent);
                AssertDebugLastMessageContains("t2", "This is a test fluent WriteIf message ");
            }
        }

        [Fact]
        public void FatalWrite()
        {
            LogWriteException_internal((ex) => _logger.ForFatalEvent().Exception(ex), LogLevel.Fatal);
        }

        [Fact]
        public void ErrorWrite()
        {
            LogWriteException_internal((ex) => _logger.ForErrorEvent().Exception(ex), LogLevel.Error);
        }

        [Fact]
        public void ExceptionWrite()
        {
            LogWriteException_internal((ex) => _logger.ForExceptionEvent(ex), LogLevel.Error);
        }

        [Fact]
        public void LogBuilder_null_lead_to_ArgumentNullException()
        {
            var logger = LogManager.GetLogger("a");
            Assert.Throws<ArgumentNullException>(() => new LogEventBuilder(null, LogLevel.Debug));
            Assert.Throws<ArgumentNullException>(() => new LogEventBuilder(null));
            Assert.Throws<ArgumentNullException>(() => new LogEventBuilder(logger, null));

            var logBuilder = new LogEventBuilder(logger);
            Assert.Throws<ArgumentNullException>(() => logBuilder.Properties(null));
            Assert.Throws<ArgumentNullException>(() => logBuilder.Property(null, "b"));
        }

        [Fact]
        public void LogBuilder_nLogEventInfo()
        {
            var d = new DateTime(2015, 01, 30, 14, 30, 5);
            var logEventInfo = new LogEventBuilder(LogManager.GetLogger("a"), LogLevel.Fatal).TimeStamp(d).LogEvent;

            Assert.Equal("a", logEventInfo.LoggerName);
            Assert.Equal(LogLevel.Fatal, logEventInfo.Level);
            Assert.Equal(d, logEventInfo.TimeStamp);
        }

        [Fact]
        public void LogBuilder_exception_only()
        {
            var ex = new Exception("Exception message1");

            _logger.ForErrorEvent()
                .Exception(ex)
                .Log();

            var expectedEvent = LogEventInfo.Create(LogLevel.Error, "logger1", null, ex);
            AssertLastLogEventTarget(expectedEvent);
        }

        [Fact]
        public void LogBuilder_message_overloadsTest()
        {
            LogManager.ThrowExceptions = true;

            _logger.ForDebugEvent();

            _logger.ForDebugEvent()
              .Message("Message with {0} arg", 1)
              .Log();
            AssertDebugLastMessage("t2", "Message with 1 arg");

            _logger.ForDebugEvent()
              .Message("Message with {0} args. {1}", 2, "YES")
              .Log();
            AssertDebugLastMessage("t2", "Message with 2 args. YES");

            _logger.ForDebugEvent()
              .Message("Message with {0} args. {1} {2}", 3, ":) ", 2)
              .Log();
            AssertDebugLastMessage("t2", "Message with 3 args. :)  2");

            _logger.ForDebugEvent()
              .Message("Message with {0} args. {1} {2}{3}", "more", ":) ", 2, "b")
              .Log();
            AssertDebugLastMessage("t2", "Message with more args. :)  2b");
        }

        [Fact]
        public void LogBuilder_message_cultureTest()
        {
            if (IsLinux())
            {
                Console.WriteLine("[SKIP] LogBuilderTests.LogBuilder_message_cultureTest because we are running in Travis");
                return;
            }

            LogManager.Configuration.DefaultCultureInfo = GetCultureInfo("en-US");

            _logger.ForDebugEvent()
             .Message("Message with {0} {1} {2} {3}", 4.1, 4.001, new DateTime(2016, 12, 31), true)
             .Log();
            AssertDebugLastMessage("t2", "Message with 4.1 4.001 12/31/2016 12:00:00 AM True");

            _logger.ForDebugEvent()
           .Message(GetCultureInfo("nl-nl"), "Message with {0} {1} {2} {3}", 4.1, 4.001, new DateTime(2016, 12, 31), true)
           .Log();
            AssertDebugLastMessage("t2", "Message with 4,1 4,001 31-12-2016 00:00:00 True");
        }

        [Fact]
        public void LogBuilder_Structured_Logging_Test()
        {
            var logEvent = _logger.ForInfoEvent().Property("Property1Key", "Property1Value").Message("{@message}", "My custom message").LogEvent;
            Assert.NotEmpty(logEvent.Properties);
            Assert.Contains("message", logEvent.Properties.Keys);
            Assert.Contains("Property1Key", logEvent.Properties.Keys);
        }

        [Fact]
        public void LogBuilder_Callsite_Test()
        {
            var logEvent = _logger.ForInfoEvent().Callsite(nameof(LogEventInfo.CallerClassName), nameof(LogEventInfo.CallerMemberName), nameof(LogEventInfo.CallerFilePath), 42).LogEvent;
            Assert.Equal(nameof(LogEventInfo.CallerClassName), logEvent.CallerClassName);
            Assert.Equal(nameof(LogEventInfo.CallerMemberName), logEvent.CallerMemberName);
            Assert.Equal(nameof(LogEventInfo.CallerFilePath), logEvent.CallerFilePath);
            Assert.Equal(42, logEvent.CallerLineNumber);
        }

        ///<remarks>
        /// func because 1 logbuilder creates 1 message
        /// 
        /// Caution: don't use overloading, that will break xUnit:
        /// CATASTROPHIC ERROR OCCURRED:
        /// System.ArgumentException: Ambiguous method named TraceWrite in type NLog.UnitTests.Fluent.LogBuilderTests
        /// </remarks>
        private void LogWriteException_internal(Func<Exception, LogEventBuilder> logBuilder, LogLevel logLevel)
        {
            Exception catchedException = null;
            string path = "blah.txt";

            try
            {
                string text = File.ReadAllText(path);
            }
            catch (Exception ex)
            {
                catchedException = ex;
                logBuilder(ex)
                    .Message("Error reading file '{0}'.", path)
                    .Property("Test", "ErrorWrite")
                    .Log();
            }

            var loggerName = "logger1";
            {
                var expectedEvent = new LogEventInfo(logLevel, loggerName, "Error reading file '{0}'.");
                expectedEvent.Properties["Test"] = "ErrorWrite";
                expectedEvent.Exception = catchedException;
                AssertLastLogEventTarget(expectedEvent);
                AssertDebugLastMessageContains("t2", "Error reading file '");
            }
            logBuilder(null)
                .Message("This is a test fluent message.")
                .Property("Test", "ErrorWrite")
                .Log();

            {
                var expectedEvent = new LogEventInfo(logLevel, loggerName, "This is a test fluent message.");
                expectedEvent.Properties["Test"] = "ErrorWrite";
                AssertLastLogEventTarget(expectedEvent);
            }

            logBuilder(null)
                .Message("This is a test fluent message '{0}'.", DateTime.Now.Ticks)
                .Property("Test", "ErrorWrite")
                .Log();

            {
                var expectedEvent = new LogEventInfo(logLevel, loggerName, "This is a test fluent message '{0}'.");
                expectedEvent.Properties["Test"] = "ErrorWrite";
                AssertLastLogEventTarget(expectedEvent);
                AssertDebugLastMessageContains("t2", "This is a test fluent message '");
            }
        }

        /// <summary>
        /// Test the written logevent
        /// </summary>
        /// <param name="expected">exptected event to be logged.</param>
        void AssertLastLogEventTarget(LogEventInfo expected)
        {
            Assert.NotNull(_lastLogEventInfo);
            Assert.Equal(expected.Message, _lastLogEventInfo.Message);

            Assert.NotNull(_lastLogEventInfo.Properties);

            Assert.Equal(expected.Properties, _lastLogEventInfo.Properties);
            Assert.Equal(expected.LoggerName, _lastLogEventInfo.LoggerName);
            Assert.Equal(expected.Level, _lastLogEventInfo.Level);
            Assert.Equal(expected.Exception, _lastLogEventInfo.Exception);
            Assert.Equal(expected.FormatProvider, _lastLogEventInfo.FormatProvider);
        }
    }
}
