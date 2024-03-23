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

namespace NLog.UnitTests.Common
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection.Emit;
    using System.Text;
    using NLog.Common;
    using NLog.Config;
    using NLog.Time;
    using Xunit;
    using Xunit.Sdk;

    public sealed class InternalLoggerLayoutTests : NLogTestBase, IDisposable
    {
        const string _timeStampFormat = "yyyy-MM-dd HH:mm:ss.ffff";

        //Test Level Field Formatting
        [Fact]
        public void TestLInternalLoggerLevelFieldFormattingInCapitalsAndBrackets() 
        {
            TimeSource.Current = new FixedTimeSource(DateTime.Now);
            var testTime = TimeSource.Current.Time.ToString(_timeStampFormat, CultureInfo.InvariantCulture);
            // Expected result is the same for both types of method invocation.
            // Expected result has timestamp
            // Expected results has 
            var expected = $"{testTime} [WARN  ] WWW\n{testTime} [ERROR ] EEE\n{testTime} [FATAL ] FFF\n{testTime} [TRACE ] TTT\n{testTime} [DEBUG ] DDD\n{testTime} [INFO  ] III\n";

            using (var loggerScope = new InternalLoggerScope(true))
            {
                InternalLogger.LogLevel = LogLevel.Trace;
                InternalLogger.IncludeTimestamp = true;
#pragma warning disable CS0618 // Type or member is obsolete
                InternalLogger.LogToConsole = true;
                StringWriter consoleOutWriter = new StringWriter()
                {
                    NewLine = ";"
                };
                InternalLogger.LogWriter = consoleOutWriter;

                {
                    // Named (based on LogLevel) public methods.
                    loggerScope.ConsoleOutputWriter.Flush();
                    loggerScope.ConsoleOutputWriter.GetStringBuilder().Length = 0;

                    InternalLogger.Warn("WWW");
                    InternalLogger.Error("EEE");
                    InternalLogger.Fatal("FFF");
                    InternalLogger.Trace("TTT");
                    InternalLogger.Debug("DDD");
                    InternalLogger.Info("III");

                    TestWriter(expected, loggerScope.ConsoleOutputWriter);
                }

                {
                    // Invoke Log(LogLevel, string) for every log level.
                    loggerScope.ConsoleOutputWriter.Flush();
                    loggerScope.ConsoleOutputWriter.GetStringBuilder().Length = 0;

                    InternalLogger.Log(LogLevel.Warn, "WWW");
                    InternalLogger.Log(LogLevel.Error, "EEE");
                    InternalLogger.Log(LogLevel.Fatal, "FFF");
                    InternalLogger.Log(LogLevel.Trace, "TTT");
                    InternalLogger.Log(LogLevel.Debug, "DDD");
                    InternalLogger.Log(LogLevel.Info, "III");

                    TestWriter(expected, loggerScope.ConsoleOutputWriter);
                }

                //lambdas
                {
                    // Named (based on LogLevel) public methods.
                    loggerScope.ConsoleOutputWriter.Flush();
                    loggerScope.ConsoleOutputWriter.GetStringBuilder().Length = 0;

#pragma warning disable CS0618 // Type or member is obsolete
                    InternalLogger.Warn(() => "WWW");
                    InternalLogger.Error(() => "EEE");
                    InternalLogger.Fatal(() => "FFF");
                    InternalLogger.Trace(() => "TTT");
                    InternalLogger.Debug(() => "DDD");
                    InternalLogger.Info(() => "III");
#pragma warning restore CS0618 // Type or member is obsolete

                    TestWriter(expected, loggerScope.ConsoleOutputWriter);
                }
            }
        }

        [Fact]
        //Test Trace, Debug Formatting
        public void TestInternalLoggerClassFieldShouldBeFullNameSpaceForTraceAndDebug() {
            //InternalLoggingConfigTest(LogLevel.Info, false, false, LogLevel.Trace, false, null, @"c:\temp\nlog\file3.txt", false, true);

            TimeSource.Current = new FixedTimeSource(DateTime.Now);
            var testTime = TimeSource.Current.Time.ToString(_timeStampFormat, CultureInfo.InvariantCulture);

            //test parameters
            var file = @"c:\temp\nlog\file_internallogger_tracedebug.txt";
            var logLevelString = LogLevel.Trace.ToString();
            var internalLogToConsoleString = false.ToString();
            var internalLogToConsoleErrorString = false.ToString().ToLower();
            var globalThresholdString = LogLevel.Trace.ToString();
            var throwExceptionsString = false.ToString().ToLower();
            var throwConfigExceptionsString = string.Empty;
            var logToTraceString = false.ToString().ToLower();
            var autoShutdownString = true.ToString().ToLower();

            string expected = "[NLog.Config.XmlLoggingConfiguration]";
            string notExpected = "[XmlLoggingConfiguration]";

            using (var loggerScope = new InternalLoggerScope(true))
            {

                InternalLogger.LogLevel = LogLevel.Trace;
                InternalLogger.IncludeTimestamp = true;
#pragma warning disable CS0618 // Type or member is obsolete
                InternalLogger.LogToConsole = true;
                StringWriter consoleOutWriter = new StringWriter()
                {
                    NewLine = ";"
                };

                XmlLoggingConfiguration.CreateFromXmlString($@"
<nlog internalLogFile='{file}' internalLogLevel='{logLevelString}' internalLogToConsole='{internalLogToConsoleString}' 
internalLogToConsoleError='{internalLogToConsoleErrorString}' globalThreshold='{globalThresholdString}' 
throwExceptions='{throwExceptionsString}' throwConfigExceptions='{throwConfigExceptionsString}' 
internalLogToTrace='{logToTraceString}' autoShutdown='{autoShutdownString}'>
</nlog>");
                
                WriterShouldContain(expected, loggerScope.ConsoleOutputWriter);
                WriterShouldNotContain(notExpected, loggerScope.ConsoleOutputWriter);
            }


        }

        [Fact]
        //Test Info and above Formatting
        public void TestInternalLoggerClassFieldShouldBeJustClassNamesForInfoAndAbove()
        {
            TimeSource.Current = new FixedTimeSource(DateTime.Now);
            var testTime = TimeSource.Current.Time.ToString(_timeStampFormat, CultureInfo.InvariantCulture);

            //test parameters
            var logLevelString = LogLevel.Info.ToString();
            var internalLogToConsoleString = false.ToString();
            var internalLogToConsoleErrorString = false.ToString().ToLower();
            var globalThresholdString = LogLevel.Trace.ToString();
            var throwExceptionsString = false.ToString().ToLower();
            var throwConfigExceptionsString = string.Empty;
            var logToTraceString = false.ToString().ToLower();
            var autoShutdownString = true.ToString().ToLower();

            string expected = "[LogManager]";
            string notExpected = "[NLog.LogManager]";

            using (var loggerScope = new InternalLoggerScope(true))
            {

                InternalLogger.LogLevel = LogLevel.Info;
                InternalLogger.IncludeTimestamp = true;
#pragma warning disable CS0618 // Type or member is obsolete
                InternalLogger.LogToConsole = true;
                StringWriter consoleOutWriter = new StringWriter()
                {
                    NewLine = ";"
                };

                Logger log = LogManager.GetCurrentClassLogger();
                log.Info("This is an Info Test message");

                WriterShouldContain(expected, loggerScope.ConsoleOutputWriter);
                WriterShouldNotContain(notExpected, loggerScope.ConsoleOutputWriter);
            }
        }

        /// <summary>
        /// Test output van een textwriter
        /// </summary>
        /// <param name="expected"></param>
        /// <param name="writer"></param>
        private static void TestWriter(string expected, StringWriter writer)
        {
            writer.Flush();
            var writerOutput = writer.ToString();
            Assert.Equal(expected, writerOutput);
        }

        private static void WriterShouldContain(string expected, StringWriter writer)
        {
            writer.Flush();
            var writerOutput = writer.ToString();
            Assert.Contains(expected, writerOutput);
        }

        private static void WriterShouldNotContain(string expected, StringWriter writer)
        {
            writer.Flush();
            var writerOutput = writer.ToString();
            Assert.DoesNotContain(expected, writerOutput);
        }

        private class FixedTimeSource : TimeSource
        {
            private readonly DateTime _time;

            public FixedTimeSource(DateTime time)
            {
                _time = time;
            }

            public override DateTime Time => _time;

            public override DateTime FromSystemTime(DateTime systemTime)
            {
                return _time;
            }
        }
        public void Dispose()
        {
            TimeSource.Current = new FastLocalTimeSource();
            InternalLogger.Reset();
        }

    }
}