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

using System.Collections.Generic;
using NLog.Config;
using NLog.Targets;
using NLog.UnitTests.Common;

namespace NLog.UnitTests.Fluent
{
    using System;
    using System.IO;
    using Xunit;
    using NLog.Fluent;

    public class LogBuilderTests : NLogTestBase
    {
        private static readonly ILogger _logger = LogManager.GetLogger("logger1");

        public LogBuilderTests()
        {
            var configuration = new LoggingConfiguration();

            var t1 = new LastLogEventListTarget { Name = "t1" };
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
            TraceWrite_internal(() => _logger.Trace());
        }

#if NET4_5
        [Fact]
        public void TraceWrite_static_builder()
        {
            TraceWrite_internal(() => Log.Trace(), true);
        }
#endif


        ///<remarks>
        /// func because 1 logbuilder creates 1 message
        /// 
        /// Caution: don't use overloading, that will break xUnit:
        /// CATASTROPHIC ERROR OCCURRED:
        /// System.ArgumentException: Ambiguous method named TraceWrite in type NLog.UnitTests.Fluent.LogBuilderTests
        /// </remarks>
        private void TraceWrite_internal(Func<LogBuilder> logBuilder, bool isStatic = false)
        {
            logBuilder()
                .Message("This is a test fluent message.")
                .Property("Test", "TraceWrite")
                .Write();


            var loggerName = isStatic ? "LogBuilderTests" : "logger1";
            {
                var expectedEvent = new LogEventInfo(LogLevel.Trace, loggerName, "This is a test fluent message.");
                expectedEvent.Properties["Test"] = "TraceWrite";
                AssertLastLogEventTarget(expectedEvent);
            }

            var ticks = DateTime.Now.Ticks;
            logBuilder()
                .Message("This is a test fluent message '{0}'.", ticks)
                .Property("Test", "TraceWrite")
                .Write();

            {
                var rendered = string.Format("This is a test fluent message '{0}'.", ticks);
                var expectedEvent = new LogEventInfo(LogLevel.Trace, loggerName, "This is a test fluent message '{0}'.");
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

            _logger.Trace()
                .Message("This is a test fluent message.")
                .Properties(props).Write();

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

            _logger.Warn()
                .Message("This is a test fluent message.")
                .Properties(props).Write();

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

            _logger.Log(LogLevel.Fatal)
                .Message("This is a test fluent message.")
                .Properties(props).Write();

            {
                var expectedEvent = new LogEventInfo(LogLevel.Fatal, "logger1", "This is a test fluent message.");
                expectedEvent.Properties["prop1"] = "1";
                expectedEvent.Properties["prop2"] = "2";
                AssertLastLogEventTarget(expectedEvent);
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

            _logger.Log(LogLevel.Fatal)
                .Message("This is a test fluent message.")
                .Properties(props).Write();

            _logger.Log(LogLevel.Off)
          .Message("dont log this.")
          .Properties(props2).Write();

            {
                var expectedEvent = new LogEventInfo(LogLevel.Fatal, "logger1", "This is a test fluent message.");
                expectedEvent.Properties["prop1"] = "1";
                expectedEvent.Properties["prop2"] = "2";
                AssertLastLogEventTarget(expectedEvent);
            }

        }

#if NET4_5
        [Fact]
        public void LevelWriteProperties()
        {
            var props = new Dictionary<string, object>
            {
                {"prop1", "1"},
                {"prop2", "2"},

            };

            Log.Level(LogLevel.Fatal)
                .Message("This is a test fluent message.")
                .Properties(props).Write();

            {
                var expectedEvent = new LogEventInfo(LogLevel.Fatal, "LogBuilderTests", "This is a test fluent message.");
                expectedEvent.Properties["prop1"] = "1";
                expectedEvent.Properties["prop2"] = "2";
                AssertLastLogEventTarget(expectedEvent);
            }

        }
#endif

        [Fact]
        public void TraceIfWrite()
        {
            _logger.Trace()
                .Message("This is a test fluent message.1")
                .Property("Test", "TraceWrite")
                .Write();

            {
                var expectedEvent = new LogEventInfo(LogLevel.Trace, "logger1", "This is a test fluent message.1");
                expectedEvent.Properties["Test"] = "TraceWrite";
                AssertLastLogEventTarget(expectedEvent);
            }

            int v = 1;
            _logger.Trace()
                .Message("This is a test fluent WriteIf message '{0}'.", DateTime.Now.Ticks)
                .Property("Test", "TraceWrite")
                .WriteIf(() => v == 1);


            {
                var expectedEvent = new LogEventInfo(LogLevel.Trace, "logger1", "This is a test fluent WriteIf message '{0}'.");
                expectedEvent.Properties["Test"] = "TraceWrite";
                AssertLastLogEventTarget(expectedEvent);
                AssertDebugLastMessageContains("t2", "This is a test fluent WriteIf message ");
            }

            _logger.Trace()
                .Message("dont write this! '{0}'.", DateTime.Now.Ticks)
                .Property("Test", "TraceWrite")
                .WriteIf(() => { return false; });

            {
                var expectedEvent = new LogEventInfo(LogLevel.Trace, "logger1", "This is a test fluent WriteIf message '{0}'.");
                expectedEvent.Properties["Test"] = "TraceWrite";
                AssertLastLogEventTarget(expectedEvent);
                AssertDebugLastMessageContains("t2", "This is a test fluent WriteIf message ");
            }

            _logger.Trace()
                .Message("This is a test fluent WriteIf message '{0}'.", DateTime.Now.Ticks)
                .Property("Test", "TraceWrite")
                .WriteIf(v == 1);


            {
                var expectedEvent = new LogEventInfo(LogLevel.Trace, "logger1", "This is a test fluent WriteIf message '{0}'.");
                expectedEvent.Properties["Test"] = "TraceWrite";
                AssertLastLogEventTarget(expectedEvent);
                AssertDebugLastMessageContains("t2", "This is a test fluent WriteIf message ");
            }

            _logger.Trace()
                .Message("Should Not WriteIf message '{0}'.", DateTime.Now.Ticks)
                .Property("Test", "TraceWrite")
                .WriteIf(v > 1);


            {
                //previous
                var expectedEvent = new LogEventInfo(LogLevel.Trace, "logger1", "This is a test fluent WriteIf message '{0}'.");
                expectedEvent.Properties["Test"] = "TraceWrite";
                AssertLastLogEventTarget(expectedEvent);
                AssertDebugLastMessageContains("t2", "This is a test fluent WriteIf message ");
            }

        }

        [Fact]
        public void InfoWrite()
        {
            InfoWrite_internal(() => _logger.Info());
        }

#if NET4_5
        [Fact]
        public void InfoWrite_static_builder()
        {
            InfoWrite_internal(() => Log.Info(), true);
        }
#endif

        ///<remarks>
        /// func because 1 logbuilder creates 1 message
        /// 
        /// Caution: don't use overloading, that will break xUnit:
        /// CATASTROPHIC ERROR OCCURRED:
        /// System.ArgumentException: Ambiguous method named TraceWrite in type NLog.UnitTests.Fluent.LogBuilderTests
        /// </remarks>
        private void InfoWrite_internal(Func<LogBuilder> logBuilder, bool isStatic = false)
        {
            logBuilder()
                .Message("This is a test fluent message.")
                .Property("Test", "InfoWrite")
                .Write();

            var loggerName = isStatic ? "LogBuilderTests" : "logger1";
            {
                //previous
                var expectedEvent = new LogEventInfo(LogLevel.Info, loggerName, "This is a test fluent message.");
                expectedEvent.Properties["Test"] = "InfoWrite";
                AssertLastLogEventTarget(expectedEvent);
            }

            logBuilder()
                .Message("This is a test fluent message '{0}'.", DateTime.Now.Ticks)
                .Property("Test", "InfoWrite")
                .Write();

            {
                //previous
                var expectedEvent = new LogEventInfo(LogLevel.Info, loggerName, "This is a test fluent message '{0}'.");
                expectedEvent.Properties["Test"] = "InfoWrite";
                AssertLastLogEventTarget(expectedEvent);
                AssertDebugLastMessageContains("t2", "This is a test fluent message '");
            }
        }

        [Fact]
        public void DebugWrite()
        {
            ErrorWrite_internal(() => _logger.Debug(), LogLevel.Debug);
        }

#if NET4_5
        [Fact]
        public void DebugWrite_static_builder()
        {
            ErrorWrite_internal(() => Log.Debug(), LogLevel.Debug, true);
        }
#endif

        [Fact]
        public void FatalWrite()
        {
            ErrorWrite_internal(() => _logger.Fatal(), LogLevel.Fatal);
        }

#if NET4_5
        [Fact]
        public void FatalWrite_static_builder()
        {
            ErrorWrite_internal(() => Log.Fatal(), LogLevel.Fatal, true);
        }
#endif

        [Fact]
        public void ErrorWrite()
        {
            ErrorWrite_internal(() => _logger.Error(), LogLevel.Error);
        }

#if NET4_5
        [Fact]
        public void ErrorWrite_static_builder()
        {
            ErrorWrite_internal(() => Log.Error(), LogLevel.Error, true);
        }
#endif

        [Fact]
        public void LogBuilder_null_lead_to_ArgumentNullException()
        {
            var logger = LogManager.GetLogger("a");
            Assert.Throws<ArgumentNullException>(() => new LogBuilder(null, LogLevel.Debug));
            Assert.Throws<ArgumentNullException>(() => new LogBuilder(null));
            Assert.Throws<ArgumentNullException>(() => new LogBuilder(logger, null));

            var logBuilder = new LogBuilder(logger);
            Assert.Throws<ArgumentNullException>(() => logBuilder.Properties(null));
            Assert.Throws<ArgumentNullException>(() => logBuilder.Property(null, "b"));

        }

        [Fact]
        public void LogBuilder_nLogEventInfo()
        {
            var d = new DateTime(2015, 01, 30, 14, 30, 5);
            var logEventInfo = new LogBuilder(LogManager.GetLogger("a")).LoggerName("b").Level(LogLevel.Fatal).TimeStamp(d).LogEventInfo;

            Assert.Equal("b", logEventInfo.LoggerName);
            Assert.Equal(LogLevel.Fatal, logEventInfo.Level);
            Assert.Equal(d, logEventInfo.TimeStamp);
        }

        ///<remarks>
        /// func because 1 logbuilder creates 1 message
        /// 
        /// Caution: don't use overloading, that will break xUnit:
        /// CATASTROPHIC ERROR OCCURRED:
        /// System.ArgumentException: Ambiguous method named TraceWrite in type NLog.UnitTests.Fluent.LogBuilderTests
        /// </remarks>
        private void ErrorWrite_internal(Func<LogBuilder> logBuilder, LogLevel logLevel, bool isStatic = false)
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
                logBuilder()
                    .Message("Error reading file '{0}'.", path)
                    .Exception(ex)
                    .Property("Test", "ErrorWrite")
                    .Write();
            }

            var loggerName = isStatic ? "LogBuilderTests" : "logger1";
            {
                var expectedEvent = new LogEventInfo(logLevel, loggerName, "Error reading file '{0}'.");
                expectedEvent.Properties["Test"] = "ErrorWrite";
                expectedEvent.Exception = catchedException;
                AssertLastLogEventTarget(expectedEvent);
                AssertDebugLastMessageContains("t2", "Error reading file '");
            }
            logBuilder()
                .Message("This is a test fluent message.")
                .Property("Test", "ErrorWrite")
                .Write();

            {
                var expectedEvent = new LogEventInfo(logLevel, loggerName, "This is a test fluent message.");
                expectedEvent.Properties["Test"] = "ErrorWrite";
                AssertLastLogEventTarget(expectedEvent);
            }

            logBuilder()
                .Message("This is a test fluent message '{0}'.", DateTime.Now.Ticks)
                .Property("Test", "ErrorWrite")
                .Write();

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
        static void AssertLastLogEventTarget(LogEventInfo expected)
        {
            var target = LogManager.Configuration.FindTargetByName<LastLogEventListTarget>("t1");
            Assert.NotNull(target);

            var lastLogEvent = target.LastLogEvent;
            Assert.NotNull(lastLogEvent);
            Assert.Equal(expected.Message, lastLogEvent.Message);

            Assert.NotNull(lastLogEvent.Properties);
            //remove caller as they are also removed from the alleventrenders.
            lastLogEvent.Properties.Remove("CallerMemberName");
            lastLogEvent.Properties.Remove("CallerLineNumber");
            lastLogEvent.Properties.Remove("CallerFilePath");


            Assert.Equal(expected.Properties, lastLogEvent.Properties);
            Assert.Equal(expected.LoggerName, lastLogEvent.LoggerName);
            Assert.Equal(expected.Level, lastLogEvent.Level);
            Assert.Equal(expected.Exception, lastLogEvent.Exception);
            Assert.Equal(expected.FormatProvider, lastLogEvent.FormatProvider);
        }
    }
}
