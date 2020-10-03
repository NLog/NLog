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

namespace NLog.UnitTests
{
    using System;
    using System.Globalization;
    using NLog.Targets;
    using NLog.Config;
    using Xunit;
    using NLog;

    public class LoggerExtensionsTests : NLogTestBase
    {
        private CultureInfo NLCulture = GetCultureInfo("nl-nl");

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void TraceTest(bool levelEnabled)
        {
            var logLevel = LogLevel.Trace;
            ArrangeLogFactory(logLevel, levelEnabled, out var logFactory, out var debugTarget);
            ILog logger = logFactory.GetLogger(logLevel.ToString() + "Test");

            logger.Trace(2.3);
            if (levelEnabled)
                Assert.Equal($"{logLevel}|2.3|", debugTarget.LastMessage);
            else
                Assert.Equal("", debugTarget.LastMessage);

            logger.Trace(new ApplicationException("Galactic Failure"));
            if (levelEnabled)
                Assert.Equal($"{logLevel}|System.ApplicationException: Galactic Failure|Galactic Failure", debugTarget.LastMessage);
            else
                Assert.Equal("", debugTarget.LastMessage);

            logger.Trace(LogEventInfo.Create(LogLevel.Trace, null, "Hello World"));
            if (levelEnabled)
                Assert.Equal($"{logLevel}|Hello World|", debugTarget.LastMessage);
            else
                Assert.Equal("", debugTarget.LastMessage);

            logger.Trace(NLCulture, 2.3);
            if (levelEnabled)
                Assert.Equal($"{logLevel}|2,3|", debugTarget.LastMessage);
            else
                Assert.Equal("", debugTarget.LastMessage);

            logger.Trace("Hello");
            if (levelEnabled)
                Assert.Equal($"{logLevel}|Hello|", debugTarget.LastMessage);
            else
                Assert.Equal("", debugTarget.LastMessage);

            logger.Trace("Hello {0}", "World");
            if (levelEnabled)
                Assert.Equal($"{logLevel}|Hello World|", debugTarget.LastMessage);
            else
                Assert.Equal("", debugTarget.LastMessage);

            logger.Trace("Hello {0} and Goodbye {1}", "Sun", "Moon");
            if (levelEnabled)
                Assert.Equal($"{logLevel}|Hello Sun and Goodbye Moon|", debugTarget.LastMessage);
            else
                Assert.Equal("", debugTarget.LastMessage);

            logger.Trace("Hello {0} and {1} {2}", "Sun", "Goodbye", "Moon");
            if (levelEnabled)
                Assert.Equal($"{logLevel}|Hello Sun and Goodbye Moon|", debugTarget.LastMessage);
            else
                Assert.Equal("", debugTarget.LastMessage);

            logger.Trace("Hello {0} {1} {2} {3}", "Sun", "and", "Goodbye", "Moon");
            if (levelEnabled)
                Assert.Equal($"{logLevel}|Hello Sun and Goodbye Moon|", debugTarget.LastMessage);
            else
                Assert.Equal("", debugTarget.LastMessage);

            logger.Trace(NLCulture, "Multiply {0} with {1}", 4.2, 10);
            if (levelEnabled)
                Assert.Equal($"{logLevel}|Multiply 4,2 with 10|", debugTarget.LastMessage);
            else
                Assert.Equal("", debugTarget.LastMessage);

            logger.Trace(new ApplicationException("Galactic Failure"), NLCulture, "Multiply {0} with {1}", 4.2, 10);
            if (levelEnabled)
                Assert.Equal($"{logLevel}|Multiply 4,2 with 10|Galactic Failure", debugTarget.LastMessage);
            else
                Assert.Equal("", debugTarget.LastMessage);

            logger.Trace(() => "Hello World");
            if (levelEnabled)
                Assert.Equal($"{logLevel}|Hello World|", debugTarget.LastMessage);
            else
                Assert.Equal("", debugTarget.LastMessage);

            logger.Trace(new ApplicationException("Galactic Failure"), () => "Hello World");
            if (levelEnabled)
                Assert.Equal($"{logLevel}|Hello World|Galactic Failure", debugTarget.LastMessage);
            else
                Assert.Equal("", debugTarget.LastMessage);

            if (levelEnabled)
            {
                LogMessageGenerator nullMessageGenerator = null;
                Assert.Throws<ArgumentNullException>(() => logger.Trace(nullMessageGenerator));
                Assert.Throws<ArgumentNullException>(() => logger.Trace(new ApplicationException("Galactic Failure"), nullMessageGenerator));
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void DebugTest(bool levelEnabled)
        {
            var logLevel = LogLevel.Debug;
            ArrangeLogFactory(logLevel, levelEnabled, out var logFactory, out var debugTarget);
            ILog logger = logFactory.GetLogger(logLevel.ToString() + "Test");

            logger.Debug(2.3);
            if (levelEnabled)
                Assert.Equal($"{logLevel}|2.3|", debugTarget.LastMessage);
            else
                Assert.Equal("", debugTarget.LastMessage);

            logger.Debug(new ApplicationException("Galactic Failure"));
            if (levelEnabled)
                Assert.Equal($"{logLevel}|System.ApplicationException: Galactic Failure|Galactic Failure", debugTarget.LastMessage);
            else
                Assert.Equal("", debugTarget.LastMessage);

            logger.Debug(LogEventInfo.Create(LogLevel.Debug, null, "Hello World"));
            if (levelEnabled)
                Assert.Equal($"{logLevel}|Hello World|", debugTarget.LastMessage);
            else
                Assert.Equal("", debugTarget.LastMessage);

            logger.Debug(NLCulture, 2.3);
            if (levelEnabled)
                Assert.Equal($"{logLevel}|2,3|", debugTarget.LastMessage);
            else
                Assert.Equal("", debugTarget.LastMessage);

            logger.Debug("Hello");
            if (levelEnabled)
                Assert.Equal($"{logLevel}|Hello|", debugTarget.LastMessage);
            else
                Assert.Equal("", debugTarget.LastMessage);

            logger.Debug("Hello {0}", "World");
            if (levelEnabled)
                Assert.Equal($"{logLevel}|Hello World|", debugTarget.LastMessage);
            else
                Assert.Equal("", debugTarget.LastMessage);

            logger.Debug("Hello {0} and Goodbye {1}", "Sun", "Moon");
            if (levelEnabled)
                Assert.Equal($"{logLevel}|Hello Sun and Goodbye Moon|", debugTarget.LastMessage);
            else
                Assert.Equal("", debugTarget.LastMessage);

            logger.Debug("Hello {0} and {1} {2}", "Sun", "Goodbye", "Moon");
            if (levelEnabled)
                Assert.Equal($"{logLevel}|Hello Sun and Goodbye Moon|", debugTarget.LastMessage);
            else
                Assert.Equal("", debugTarget.LastMessage);

            logger.Debug("Hello {0} {1} {2} {3}", "Sun", "and", "Goodbye", "Moon");
            if (levelEnabled)
                Assert.Equal($"{logLevel}|Hello Sun and Goodbye Moon|", debugTarget.LastMessage);
            else
                Assert.Equal("", debugTarget.LastMessage);

            logger.Debug(NLCulture, "Multiply {0} with {1}", 4.2, 10);
            if (levelEnabled)
                Assert.Equal($"{logLevel}|Multiply 4,2 with 10|", debugTarget.LastMessage);
            else
                Assert.Equal("", debugTarget.LastMessage);

            logger.Debug(new ApplicationException("Galactic Failure"), NLCulture, "Multiply {0} with {1}", 4.2, 10);
            if (levelEnabled)
                Assert.Equal($"{logLevel}|Multiply 4,2 with 10|Galactic Failure", debugTarget.LastMessage);
            else
                Assert.Equal("", debugTarget.LastMessage);

            logger.Debug(() => "Hello World");
            if (levelEnabled)
                Assert.Equal($"{logLevel}|Hello World|", debugTarget.LastMessage);
            else
                Assert.Equal("", debugTarget.LastMessage);

            logger.Debug(new ApplicationException("Galactic Failure"), () => "Hello World");
            if (levelEnabled)
                Assert.Equal($"{logLevel}|Hello World|Galactic Failure", debugTarget.LastMessage);
            else
                Assert.Equal("", debugTarget.LastMessage);

            if (levelEnabled)
            {
                LogMessageGenerator nullMessageGenerator = null;
                Assert.Throws<ArgumentNullException>(() => logger.Debug(nullMessageGenerator));
                Assert.Throws<ArgumentNullException>(() => logger.Debug(new ApplicationException("Galactic Failure"), nullMessageGenerator));
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void InfoTest(bool levelEnabled)
        {
            var logLevel = LogLevel.Info;
            ArrangeLogFactory(logLevel, levelEnabled, out var logFactory, out var debugTarget);
            ILog logger = logFactory.GetLogger(logLevel.ToString() + "Test");

            logger.Info(2.3);
            if (levelEnabled)
                Assert.Equal($"{logLevel}|2.3|", debugTarget.LastMessage);
            else
                Assert.Equal("", debugTarget.LastMessage);

            logger.Info(new ApplicationException("Galactic Failure"));
            if (levelEnabled)
                Assert.Equal($"{logLevel}|System.ApplicationException: Galactic Failure|Galactic Failure", debugTarget.LastMessage);
            else
                Assert.Equal("", debugTarget.LastMessage);

            logger.Info(LogEventInfo.Create(LogLevel.Info, null, "Hello World"));
            if (levelEnabled)
                Assert.Equal($"{logLevel}|Hello World|", debugTarget.LastMessage);
            else
                Assert.Equal("", debugTarget.LastMessage);

            logger.Info(NLCulture, 2.3);
            if (levelEnabled)
                Assert.Equal($"{logLevel}|2,3|", debugTarget.LastMessage);
            else
                Assert.Equal("", debugTarget.LastMessage);

            logger.Info("Hello");
            if (levelEnabled)
                Assert.Equal($"{logLevel}|Hello|", debugTarget.LastMessage);
            else
                Assert.Equal("", debugTarget.LastMessage);

            logger.Info("Hello {0}", "World");
            if (levelEnabled)
                Assert.Equal($"{logLevel}|Hello World|", debugTarget.LastMessage);
            else
                Assert.Equal("", debugTarget.LastMessage);

            logger.Info("Hello {0} and Goodbye {1}", "Sun", "Moon");
            if (levelEnabled)
                Assert.Equal($"{logLevel}|Hello Sun and Goodbye Moon|", debugTarget.LastMessage);
            else
                Assert.Equal("", debugTarget.LastMessage);

            logger.Info("Hello {0} and {1} {2}", "Sun", "Goodbye", "Moon");
            if (levelEnabled)
                Assert.Equal($"{logLevel}|Hello Sun and Goodbye Moon|", debugTarget.LastMessage);
            else
                Assert.Equal("", debugTarget.LastMessage);

            logger.Info("Hello {0} {1} {2} {3}", "Sun", "and", "Goodbye", "Moon");
            if (levelEnabled)
                Assert.Equal($"{logLevel}|Hello Sun and Goodbye Moon|", debugTarget.LastMessage);
            else
                Assert.Equal("", debugTarget.LastMessage);

            logger.Info(NLCulture, "Multiply {0} with {1}", 4.2, 10);
            if (levelEnabled)
                Assert.Equal($"{logLevel}|Multiply 4,2 with 10|", debugTarget.LastMessage);
            else
                Assert.Equal("", debugTarget.LastMessage);

            logger.Info(new ApplicationException("Galactic Failure"), NLCulture, "Multiply {0} with {1}", 4.2, 10);
            if (levelEnabled)
                Assert.Equal($"{logLevel}|Multiply 4,2 with 10|Galactic Failure", debugTarget.LastMessage);
            else
                Assert.Equal("", debugTarget.LastMessage);

            logger.Info(() => "Hello World");
            if (levelEnabled)
                Assert.Equal($"{logLevel}|Hello World|", debugTarget.LastMessage);
            else
                Assert.Equal("", debugTarget.LastMessage);

            logger.Info(new ApplicationException("Galactic Failure"), () => "Hello World");
            if (levelEnabled)
                Assert.Equal($"{logLevel}|Hello World|Galactic Failure", debugTarget.LastMessage);
            else
                Assert.Equal("", debugTarget.LastMessage);

            if (levelEnabled)
            {
                LogMessageGenerator nullMessageGenerator = null;
                Assert.Throws<ArgumentNullException>(() => logger.Info(nullMessageGenerator));
                Assert.Throws<ArgumentNullException>(() => logger.Info(new ApplicationException("Galactic Failure"), nullMessageGenerator));
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void WarnTest(bool levelEnabled)
        {
            var logLevel = LogLevel.Warn;
            ArrangeLogFactory(logLevel, levelEnabled, out var logFactory, out var debugTarget);
            ILog logger = logFactory.GetLogger(logLevel.ToString() + "Test");

            logger.Warn(2.3);
            if (levelEnabled)
                Assert.Equal($"{logLevel}|2.3|", debugTarget.LastMessage);
            else
                Assert.Equal("", debugTarget.LastMessage);

            logger.Warn(new ApplicationException("Galactic Failure"));
            if (levelEnabled)
                Assert.Equal($"{logLevel}|System.ApplicationException: Galactic Failure|Galactic Failure", debugTarget.LastMessage);
            else
                Assert.Equal("", debugTarget.LastMessage);

            logger.Warn(LogEventInfo.Create(LogLevel.Warn, null, "Hello World"));
            if (levelEnabled)
                Assert.Equal($"{logLevel}|Hello World|", debugTarget.LastMessage);
            else
                Assert.Equal("", debugTarget.LastMessage);

            logger.Warn(NLCulture, 2.3);
            if (levelEnabled)
                Assert.Equal($"{logLevel}|2,3|", debugTarget.LastMessage);
            else
                Assert.Equal("", debugTarget.LastMessage);

            logger.Warn("Hello");
            if (levelEnabled)
                Assert.Equal($"{logLevel}|Hello|", debugTarget.LastMessage);
            else
                Assert.Equal("", debugTarget.LastMessage);

            logger.Warn("Hello {0}", "World");
            if (levelEnabled)
                Assert.Equal($"{logLevel}|Hello World|", debugTarget.LastMessage);
            else
                Assert.Equal("", debugTarget.LastMessage);

            logger.Warn("Hello {0} and Goodbye {1}", "Sun", "Moon");
            if (levelEnabled)
                Assert.Equal($"{logLevel}|Hello Sun and Goodbye Moon|", debugTarget.LastMessage);
            else
                Assert.Equal("", debugTarget.LastMessage);

            logger.Warn("Hello {0} and {1} {2}", "Sun", "Goodbye", "Moon");
            if (levelEnabled)
                Assert.Equal($"{logLevel}|Hello Sun and Goodbye Moon|", debugTarget.LastMessage);
            else
                Assert.Equal("", debugTarget.LastMessage);

            logger.Warn("Hello {0} {1} {2} {3}", "Sun", "and", "Goodbye", "Moon");
            if (levelEnabled)
                Assert.Equal($"{logLevel}|Hello Sun and Goodbye Moon|", debugTarget.LastMessage);
            else
                Assert.Equal("", debugTarget.LastMessage);

            logger.Warn(NLCulture, "Multiply {0} with {1}", 4.2, 10);
            if (levelEnabled)
                Assert.Equal($"{logLevel}|Multiply 4,2 with 10|", debugTarget.LastMessage);
            else
                Assert.Equal("", debugTarget.LastMessage);

            logger.Warn(new ApplicationException("Galactic Failure"), NLCulture, "Multiply {0} with {1}", 4.2, 10);
            if (levelEnabled)
                Assert.Equal($"{logLevel}|Multiply 4,2 with 10|Galactic Failure", debugTarget.LastMessage);
            else
                Assert.Equal("", debugTarget.LastMessage);

            logger.Warn(() => "Hello World");
            if (levelEnabled)
                Assert.Equal($"{logLevel}|Hello World|", debugTarget.LastMessage);
            else
                Assert.Equal("", debugTarget.LastMessage);

            logger.Warn(new ApplicationException("Galactic Failure"), () => "Hello World");
            if (levelEnabled)
                Assert.Equal($"{logLevel}|Hello World|Galactic Failure", debugTarget.LastMessage);
            else
                Assert.Equal("", debugTarget.LastMessage);

            if (levelEnabled)
            {
                LogMessageGenerator nullMessageGenerator = null;
                Assert.Throws<ArgumentNullException>(() => logger.Warn(nullMessageGenerator));
                Assert.Throws<ArgumentNullException>(() => logger.Warn(new ApplicationException("Galactic Failure"), nullMessageGenerator));
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ErrorTest(bool levelEnabled)
        {
            var logLevel = LogLevel.Error;
            ArrangeLogFactory(logLevel, levelEnabled, out var logFactory, out var debugTarget);
            ILog logger = logFactory.GetLogger(logLevel.ToString() + "Test");

            logger.Error(2.3);
            if (levelEnabled)
                Assert.Equal($"{logLevel}|2.3|", debugTarget.LastMessage);
            else
                Assert.Equal("", debugTarget.LastMessage);

            logger.Error(new ApplicationException("Galactic Failure"));
            if (levelEnabled)
                Assert.Equal($"{logLevel}|System.ApplicationException: Galactic Failure|Galactic Failure", debugTarget.LastMessage);
            else
                Assert.Equal("", debugTarget.LastMessage);

            logger.Error(LogEventInfo.Create(LogLevel.Error, null, "Hello World"));
            if (levelEnabled)
                Assert.Equal($"{logLevel}|Hello World|", debugTarget.LastMessage);
            else
                Assert.Equal("", debugTarget.LastMessage);

            logger.Error(NLCulture, 2.3);
            if (levelEnabled)
                Assert.Equal($"{logLevel}|2,3|", debugTarget.LastMessage);
            else
                Assert.Equal("", debugTarget.LastMessage);

            logger.Error("Hello");
            if (levelEnabled)
                Assert.Equal($"{logLevel}|Hello|", debugTarget.LastMessage);
            else
                Assert.Equal("", debugTarget.LastMessage);

            logger.Error("Hello {0}", "World");
            if (levelEnabled)
                Assert.Equal($"{logLevel}|Hello World|", debugTarget.LastMessage);
            else
                Assert.Equal("", debugTarget.LastMessage);

            logger.Error("Hello {0} and Goodbye {1}", "Sun", "Moon");
            if (levelEnabled)
                Assert.Equal($"{logLevel}|Hello Sun and Goodbye Moon|", debugTarget.LastMessage);
            else
                Assert.Equal("", debugTarget.LastMessage);

            logger.Error("Hello {0} and {1} {2}", "Sun", "Goodbye", "Moon");
            if (levelEnabled)
                Assert.Equal($"{logLevel}|Hello Sun and Goodbye Moon|", debugTarget.LastMessage);
            else
                Assert.Equal("", debugTarget.LastMessage);

            logger.Error("Hello {0} {1} {2} {3}", "Sun", "and", "Goodbye", "Moon");
            if (levelEnabled)
                Assert.Equal($"{logLevel}|Hello Sun and Goodbye Moon|", debugTarget.LastMessage);
            else
                Assert.Equal("", debugTarget.LastMessage);

            logger.Error(NLCulture, "Multiply {0} with {1}", 4.2, 10);
            if (levelEnabled)
                Assert.Equal($"{logLevel}|Multiply 4,2 with 10|", debugTarget.LastMessage);
            else
                Assert.Equal("", debugTarget.LastMessage);

            logger.Error(new ApplicationException("Galactic Failure"), NLCulture, "Multiply {0} with {1}", 4.2, 10);
            if (levelEnabled)
                Assert.Equal($"{logLevel}|Multiply 4,2 with 10|Galactic Failure", debugTarget.LastMessage);
            else
                Assert.Equal("", debugTarget.LastMessage);

            logger.Error(() => "Hello World");
            if (levelEnabled)
                Assert.Equal($"{logLevel}|Hello World|", debugTarget.LastMessage);
            else
                Assert.Equal("", debugTarget.LastMessage);

            logger.Error(new ApplicationException("Galactic Failure"), () => "Hello World");
            if (levelEnabled)
                Assert.Equal($"{logLevel}|Hello World|Galactic Failure", debugTarget.LastMessage);
            else
                Assert.Equal("", debugTarget.LastMessage);

            if (levelEnabled)
            {
                LogMessageGenerator nullMessageGenerator = null;
                Assert.Throws<ArgumentNullException>(() => logger.Error(nullMessageGenerator));
                Assert.Throws<ArgumentNullException>(() => logger.Error(new ApplicationException("Galactic Failure"), nullMessageGenerator));
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void FatalTest(bool levelEnabled)
        {
            var logLevel = LogLevel.Fatal;
            ArrangeLogFactory(logLevel, levelEnabled, out var logFactory, out var debugTarget);
            ILog logger = logFactory.GetLogger(logLevel.ToString() + "Test");

            logger.Fatal(2.3);
            if (levelEnabled)
                Assert.Equal($"{logLevel}|2.3|", debugTarget.LastMessage);
            else
                Assert.Equal("", debugTarget.LastMessage);

            logger.Fatal(new ApplicationException("Galactic Failure"));
            if (levelEnabled)
                Assert.Equal($"{logLevel}|System.ApplicationException: Galactic Failure|Galactic Failure", debugTarget.LastMessage);
            else
                Assert.Equal("", debugTarget.LastMessage);

            logger.Fatal(LogEventInfo.Create(LogLevel.Fatal, null, "Hello World"));
            if (levelEnabled)
                Assert.Equal($"{logLevel}|Hello World|", debugTarget.LastMessage);
            else
                Assert.Equal("", debugTarget.LastMessage);

            logger.Fatal(NLCulture, 2.3);
            if (levelEnabled)
                Assert.Equal($"{logLevel}|2,3|", debugTarget.LastMessage);
            else
                Assert.Equal("", debugTarget.LastMessage);

            logger.Fatal("Hello");
            if (levelEnabled)
                Assert.Equal($"{logLevel}|Hello|", debugTarget.LastMessage);
            else
                Assert.Equal("", debugTarget.LastMessage);

            logger.Fatal("Hello {0}", "World");
            if (levelEnabled)
                Assert.Equal($"{logLevel}|Hello World|", debugTarget.LastMessage);
            else
                Assert.Equal("", debugTarget.LastMessage);

            logger.Fatal("Hello {0} and Goodbye {1}", "Sun", "Moon");
            if (levelEnabled)
                Assert.Equal($"{logLevel}|Hello Sun and Goodbye Moon|", debugTarget.LastMessage);
            else
                Assert.Equal("", debugTarget.LastMessage);

            logger.Fatal("Hello {0} and {1} {2}", "Sun", "Goodbye", "Moon");
            if (levelEnabled)
                Assert.Equal($"{logLevel}|Hello Sun and Goodbye Moon|", debugTarget.LastMessage);
            else
                Assert.Equal("", debugTarget.LastMessage);

            logger.Fatal("Hello {0} {1} {2} {3}", "Sun", "and", "Goodbye", "Moon");
            if (levelEnabled)
                Assert.Equal($"{logLevel}|Hello Sun and Goodbye Moon|", debugTarget.LastMessage);
            else
                Assert.Equal("", debugTarget.LastMessage);

            logger.Fatal(NLCulture, "Multiply {0} with {1}", 4.2, 10);
            if (levelEnabled)
                Assert.Equal($"{logLevel}|Multiply 4,2 with 10|", debugTarget.LastMessage);
            else
                Assert.Equal("", debugTarget.LastMessage);

            logger.Fatal(new ApplicationException("Galactic Failure"), NLCulture, "Multiply {0} with {1}", 4.2, 10);
            if (levelEnabled)
                Assert.Equal($"{logLevel}|Multiply 4,2 with 10|Galactic Failure", debugTarget.LastMessage);
            else
                Assert.Equal("", debugTarget.LastMessage);

            logger.Fatal(() => "Hello World");
            if (levelEnabled)
                Assert.Equal($"{logLevel}|Hello World|", debugTarget.LastMessage);
            else
                Assert.Equal("", debugTarget.LastMessage);

            logger.Fatal(new ApplicationException("Galactic Failure"), () => "Hello World");
            if (levelEnabled)
                Assert.Equal($"{logLevel}|Hello World|Galactic Failure", debugTarget.LastMessage);
            else
                Assert.Equal("", debugTarget.LastMessage);

            if (levelEnabled)
            {
                LogMessageGenerator nullMessageGenerator = null;
                Assert.Throws<ArgumentNullException>(() => logger.Fatal(nullMessageGenerator));
                Assert.Throws<ArgumentNullException>(() => logger.Fatal(new ApplicationException("Galactic Failure"), nullMessageGenerator));
            }
        }

        private static void ArrangeLogFactory(LogLevel logLevel, bool levelEnabled, out LogFactory logFactory, out DebugTarget debugTarget)
        {
            logFactory = new LogFactory();
            var logConfig = new LoggingConfiguration(logFactory);
            logConfig.DefaultCultureInfo = CultureInfo.InvariantCulture;
            debugTarget = new DebugTarget("debug") { Layout = "${level}|${message}|${exception:format=message}" };
            var loggingRule = new LoggingRule("*", debugTarget);
            if (levelEnabled)
            {
                loggingRule.EnableLoggingForLevel(logLevel);
            }
            else
            {
                loggingRule.EnableLoggingForLevels(LogLevel.MinLevel, LogLevel.MaxLevel);
                loggingRule.DisableLoggingForLevel(logLevel);
            }
            logConfig.LoggingRules.Add(loggingRule);
            logFactory.Configuration = logConfig;
        }
    }
}
