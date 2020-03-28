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

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Xunit;
using NLog.Common;
using System.Text;
using NLog.Time;
using Xunit.Extensions;

namespace NLog.UnitTests.Common
{
    public class InternalLoggerTests : NLogTestBase, IDisposable
    {
        /// <summary>
        /// Test the return values of all Is[Level]Enabled() methods.
        /// </summary>
        [Fact]
        public void IsEnabledTests()
        {
            // Setup LogLevel to minimum named level.
            InternalLogger.LogLevel = LogLevel.Trace;

            Assert.True(InternalLogger.IsTraceEnabled);
            Assert.True(InternalLogger.IsDebugEnabled);
            Assert.True(InternalLogger.IsInfoEnabled);
            Assert.True(InternalLogger.IsWarnEnabled);
            Assert.True(InternalLogger.IsErrorEnabled);
            Assert.True(InternalLogger.IsFatalEnabled);

            // Setup LogLevel to maximum named level.
            InternalLogger.LogLevel = LogLevel.Fatal;

            Assert.False(InternalLogger.IsTraceEnabled);
            Assert.False(InternalLogger.IsDebugEnabled);
            Assert.False(InternalLogger.IsInfoEnabled);
            Assert.False(InternalLogger.IsWarnEnabled);
            Assert.False(InternalLogger.IsErrorEnabled);
            Assert.True(InternalLogger.IsFatalEnabled);

            // Switch off the internal logging. 
            InternalLogger.LogLevel = LogLevel.Off;

            Assert.False(InternalLogger.IsTraceEnabled);
            Assert.False(InternalLogger.IsDebugEnabled);
            Assert.False(InternalLogger.IsInfoEnabled);
            Assert.False(InternalLogger.IsWarnEnabled);
            Assert.False(InternalLogger.IsErrorEnabled);
            Assert.False(InternalLogger.IsFatalEnabled);
        }

        [Fact]
        public void WriteToStringWriterTests()
        {
            try
            {
                // Expected result is the same for both types of method invocation.
                const string expected = "Warn WWW\nError EEE\nFatal FFF\nTrace TTT\nDebug DDD\nInfo III\n";

                InternalLogger.LogLevel = LogLevel.Trace;
                InternalLogger.IncludeTimestamp = false;
                {
                    StringWriter writer1 = new StringWriter()
                    {
                        NewLine = "\n"
                    };
                    InternalLogger.LogWriter = writer1;

                    // Named (based on LogLevel) public methods.
                    InternalLogger.Warn("WWW");
                    InternalLogger.Error("EEE");
                    InternalLogger.Fatal("FFF");
                    InternalLogger.Trace("TTT");
                    InternalLogger.Debug("DDD");
                    InternalLogger.Info("III");

                    TestWriter(expected, writer1);
                }
                {
                    //
                    // Reconfigure the LogWriter.

                    StringWriter writer2 = new StringWriter()
                    {
                        NewLine = "\n"
                    };
                    InternalLogger.LogWriter = writer2;

                    // Invoke Log(LogLevel, string) for every log level.
                    InternalLogger.Log(LogLevel.Warn, "WWW");
                    InternalLogger.Log(LogLevel.Error, "EEE");
                    InternalLogger.Log(LogLevel.Fatal, "FFF");
                    InternalLogger.Log(LogLevel.Trace, "TTT");
                    InternalLogger.Log(LogLevel.Debug, "DDD");
                    InternalLogger.Log(LogLevel.Info, "III");

                    TestWriter(expected, writer2);
                }
                {
                    //
                    // Reconfigure the LogWriter.

                    StringWriter writer2 = new StringWriter()
                    {
                        NewLine = "\n"
                    };
                    InternalLogger.LogWriter = writer2;

                    // Invoke Log(LogLevel, string) for every log level.
                    InternalLogger.Log(LogLevel.Warn, () => "WWW");
                    InternalLogger.Log(LogLevel.Error, () => "EEE");
                    InternalLogger.Log(LogLevel.Fatal, () => "FFF");
                    InternalLogger.Log(LogLevel.Trace, () => "TTT");
                    InternalLogger.Log(LogLevel.Debug, () => "DDD");
                    InternalLogger.Log(LogLevel.Info, () => "III");

                    TestWriter(expected, writer2);
                }
            }
            finally
            {
                InternalLogger.Reset();
            }
        }


        [Fact]
        public void WriteToStringWriterWithArgsTests()
        {
            try
            {
                // Expected result is the same for both types of method invocation.
                const string expected = "Warn WWW 0\nError EEE 0, 1\nFatal FFF 0, 1, 2\nTrace TTT 0, 1, 2\nDebug DDD 0, 1\nInfo III 0\n";

                InternalLogger.LogLevel = LogLevel.Trace;
                InternalLogger.IncludeTimestamp = false;
                {
                    StringWriter writer1 = new StringWriter()
                    {
                        NewLine = "\n"
                    };
                    InternalLogger.LogWriter = writer1;

                    // Named (based on LogLevel) public methods.
                    InternalLogger.Warn("WWW {0}", 0);
                    InternalLogger.Error("EEE {0}, {1}", 0, 1);
                    InternalLogger.Fatal("FFF {0}, {1}, {2}", 0, 1, 2);
                    InternalLogger.Trace("TTT {0}, {1}, {2}", 0, 1, 2);
                    InternalLogger.Debug("DDD {0}, {1}", 0, 1);
                    InternalLogger.Info("III {0}", 0);

                    TestWriter(expected, writer1);
                }
                {
                    //
                    // Reconfigure the LogWriter.

                    StringWriter writer2 = new StringWriter()
                    {
                        NewLine = "\n"
                    };
                    InternalLogger.LogWriter = writer2;

                    // Invoke Log(LogLevel, string) for every log level.
                    InternalLogger.Log(LogLevel.Warn, "WWW {0}", 0);
                    InternalLogger.Log(LogLevel.Error, "EEE {0}, {1}", 0, 1);
                    InternalLogger.Log(LogLevel.Fatal, "FFF {0}, {1}, {2}", 0, 1, 2);
                    InternalLogger.Log(LogLevel.Trace, "TTT {0}, {1}, {2}", 0, 1, 2);
                    InternalLogger.Log(LogLevel.Debug, "DDD {0}, {1}", 0, 1);
                    InternalLogger.Log(LogLevel.Info, "III {0}", 0);
                    TestWriter(expected, writer2);
                }
            }
            finally
            {
                InternalLogger.Reset();
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


        [Fact]
        public void WriteToConsoleOutTests()
        {
            // Expected result is the same for both types of method invocation.
            const string expected = "Warn WWW\nError EEE\nFatal FFF\nTrace TTT\nDebug DDD\nInfo III\n";

            using (var loggerScope = new InternalLoggerScope(true))
            {
                InternalLogger.LogLevel = LogLevel.Trace;
                InternalLogger.IncludeTimestamp = false;
                InternalLogger.LogToConsole = true;

                {
                    StringWriter consoleOutWriter1 = new StringWriter()
                    {
                        NewLine = "\n"
                    };

                    // Redirect the console output to a StringWriter.
                    loggerScope.SetConsoleOutput(consoleOutWriter1);

                    // Named (based on LogLevel) public methods.
                    InternalLogger.Warn("WWW");
                    InternalLogger.Error("EEE");
                    InternalLogger.Fatal("FFF");
                    InternalLogger.Trace("TTT");
                    InternalLogger.Debug("DDD");
                    InternalLogger.Info("III");

                    TestWriter(expected, consoleOutWriter1);
                }

                //
                // Redirect the console output to another StringWriter.
                {
                    StringWriter consoleOutWriter2 = new StringWriter()
                    {
                        NewLine = "\n"
                    };
                    loggerScope.SetConsoleOutput(consoleOutWriter2);

                    // Invoke Log(LogLevel, string) for every log level.
                    InternalLogger.Log(LogLevel.Warn, "WWW");
                    InternalLogger.Log(LogLevel.Error, "EEE");
                    InternalLogger.Log(LogLevel.Fatal, "FFF");
                    InternalLogger.Log(LogLevel.Trace, "TTT");
                    InternalLogger.Log(LogLevel.Debug, "DDD");
                    InternalLogger.Log(LogLevel.Info, "III");

                    TestWriter(expected, consoleOutWriter2);
                }

                //lambdas
                {
                    StringWriter consoleOutWriter1 = new StringWriter()
                    {
                        NewLine = "\n"
                    };

                    // Redirect the console output to a StringWriter.
                    loggerScope.SetConsoleOutput(consoleOutWriter1);

                    // Named (based on LogLevel) public methods.
                    InternalLogger.Warn(() => "WWW");
                    InternalLogger.Error(() => "EEE");
                    InternalLogger.Fatal(() => "FFF");
                    InternalLogger.Trace(() => "TTT");
                    InternalLogger.Debug(() => "DDD");
                    InternalLogger.Info(() => "III");

                    TestWriter(expected, consoleOutWriter1);
                }
            }
        }

        [Fact]
        public void WriteToConsoleErrorTests()
        {
            using (var loggerScope = new InternalLoggerScope(true))
            {
                // Expected result is the same for both types of method invocation.
                const string expected = "Warn WWW\nError EEE\nFatal FFF\nTrace TTT\nDebug DDD\nInfo III\n";

                InternalLogger.LogLevel = LogLevel.Trace;
                InternalLogger.IncludeTimestamp = false;
                InternalLogger.LogToConsoleError = true;

                {
                    StringWriter consoleWriter1 = new StringWriter()
                    {
                        NewLine = "\n"
                    };

                    // Redirect the console output to a StringWriter.
                    loggerScope.SetConsoleError(consoleWriter1);

                    // Named (based on LogLevel) public methods.
                    InternalLogger.Warn("WWW");
                    InternalLogger.Error("EEE");
                    InternalLogger.Fatal("FFF");
                    InternalLogger.Trace("TTT");
                    InternalLogger.Debug("DDD");
                    InternalLogger.Info("III");

                    TestWriter(expected, loggerScope.ConsoleErrorWriter);
                }

                {
                    //
                    // Redirect the console output to another StringWriter.

                    StringWriter consoleWriter2 = new StringWriter()
                    {
                        NewLine = "\n"
                    };
                    loggerScope.SetConsoleError(consoleWriter2);

                    // Invoke Log(LogLevel, string) for every log level.
                    InternalLogger.Log(LogLevel.Warn, "WWW");
                    InternalLogger.Log(LogLevel.Error, "EEE");
                    InternalLogger.Log(LogLevel.Fatal, "FFF");
                    InternalLogger.Log(LogLevel.Trace, "TTT");
                    InternalLogger.Log(LogLevel.Debug, "DDD");
                    InternalLogger.Log(LogLevel.Info, "III");
                    TestWriter(expected, loggerScope.ConsoleErrorWriter);
                }
            }
        }

        [Fact]
        public void WriteToFileTests()
        {
            string expected =
    "Warn WWW" + Environment.NewLine +
    "Error EEE" + Environment.NewLine +
    "Fatal FFF" + Environment.NewLine +
    "Trace TTT" + Environment.NewLine +
    "Debug DDD" + Environment.NewLine +
    "Info III" + Environment.NewLine;

            var tempFile = Path.GetTempFileName();

            try
            {
                InternalLogger.LogLevel = LogLevel.Trace;
                InternalLogger.IncludeTimestamp = false;
                InternalLogger.LogFile = tempFile;

                // Invoke Log(LogLevel, string) for every log level.
                InternalLogger.Log(LogLevel.Warn, "WWW");
                InternalLogger.Log(LogLevel.Error, "EEE");
                InternalLogger.Log(LogLevel.Fatal, "FFF");
                InternalLogger.Log(LogLevel.Trace, "TTT");
                InternalLogger.Log(LogLevel.Debug, "DDD");
                InternalLogger.Log(LogLevel.Info, "III");

                AssertFileContents(tempFile, expected, Encoding.UTF8);
            }
            finally
            {
                InternalLogger.Reset();
                if (File.Exists(tempFile))
                {
                    File.Delete(tempFile);
                }
            }
        }

        /// <summary>
        /// <see cref="TimeSource"/> that returns always the same time,
        /// passed into object constructor.
        /// </summary>
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

        [Fact]
        public void TimestampTests()
        {
            using (new InternalLoggerScope())
            {
                InternalLogger.LogLevel = LogLevel.Trace;
                InternalLogger.IncludeTimestamp = true;
                StringWriter consoleOutWriter = new StringWriter()
                {
                    NewLine = ";"
                };

                InternalLogger.LogWriter = consoleOutWriter;

                // Set fixed time source to test time output
                TimeSource.Current = new FixedTimeSource(DateTime.Now);

                // Named (based on LogLevel) public methods.
                InternalLogger.Warn("WWW");
                InternalLogger.Error("EEE");
                InternalLogger.Fatal("FFF");
                InternalLogger.Trace("TTT");
                InternalLogger.Debug("DDD");
                InternalLogger.Info("III");

                string expectedDateTime = TimeSource.Current.Time.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);

                var strings = consoleOutWriter.ToString().Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var str in strings)
                {
                    Assert.Contains(expectedDateTime + ".", str);
                }
            }
        }

        /// <summary>
        /// Test exception overloads
        /// </summary>
        [Fact]
        public void ExceptionTests()
        {
            using (new InternalLoggerScope())
            {
                InternalLogger.LogLevel = LogLevel.Trace;
                InternalLogger.IncludeTimestamp = false;

                var ex1 = new Exception("e1");
                var ex2 = new Exception("e2", new Exception("inner"));
                var ex3 = new NLogConfigurationException("config error");
                var ex4 = new NLogConfigurationException("config error", ex2);
                var ex5 = new PathTooLongException();
                ex5.Data["key1"] = "value1";
                Exception ex6 = null;

                const string prefix = " Exception: ";

                {
                    string expected =
                 "Warn WWW1" + prefix + ex1 + Environment.NewLine +
                 "Error EEE1" + prefix + ex2 + Environment.NewLine +
                 "Fatal FFF1" + prefix + ex3 + Environment.NewLine +
                 "Trace TTT1" + prefix + ex4 + Environment.NewLine +
                 "Debug DDD1" + prefix + ex5 + Environment.NewLine +
                 "Info III1" + Environment.NewLine;

                    StringWriter consoleOutWriter = new StringWriter()
                    {
                        NewLine = Environment.NewLine
                    };

                    // Redirect the console output to a StringWriter.
                    InternalLogger.LogWriter = consoleOutWriter;

                    // Named (based on LogLevel) public methods.

                    InternalLogger.Warn(ex1, "WWW1");
                    InternalLogger.Error(ex2, "EEE1");
                    InternalLogger.Fatal(ex3, "FFF1");
                    InternalLogger.Trace(ex4, "TTT1");
                    InternalLogger.Debug(ex5, "DDD1");
                    InternalLogger.Info(ex6, "III1");

                    consoleOutWriter.Flush();
                    var strings = consoleOutWriter.ToString();

                    Assert.Equal(expected, strings);
                }
                {
                    string expected =
           "Warn WWW2" + prefix + ex1 + Environment.NewLine +
           "Error EEE2" + prefix + ex2 + Environment.NewLine +
           "Fatal FFF2" + prefix + ex3 + Environment.NewLine +
           "Trace TTT2" + prefix + ex4 + Environment.NewLine +
           "Debug DDD2" + prefix + ex5 + Environment.NewLine +
           "Info III2" + Environment.NewLine;

                    StringWriter consoleOutWriter = new StringWriter()
                    {
                        NewLine = Environment.NewLine
                    };

                    // Redirect the console output to a StringWriter.
                    InternalLogger.LogWriter = consoleOutWriter;

                    // Named (based on LogLevel) public methods.

                    InternalLogger.Warn(ex1, () => "WWW2");
                    InternalLogger.Error(ex2, () => "EEE2");
                    InternalLogger.Fatal(ex3, () => "FFF2");
                    InternalLogger.Trace(ex4, () => "TTT2");
                    InternalLogger.Debug(ex5, () => "DDD2");
                    InternalLogger.Info(ex6, () => "III2");

                    consoleOutWriter.Flush();
                    var strings = consoleOutWriter.ToString();
                    Assert.Equal(expected, strings);
                }
                {

                    string expected =
           "Warn WWW3" + prefix + ex1 + Environment.NewLine +
           "Error EEE3" + prefix + ex2 + Environment.NewLine +
           "Fatal FFF3" + prefix + ex3 + Environment.NewLine +
           "Trace TTT3" + prefix + ex4 + Environment.NewLine +
           "Debug DDD3" + prefix + ex5 + Environment.NewLine +
           "Info III3" + Environment.NewLine;

                    StringWriter consoleOutWriter = new StringWriter()
                    {
                        NewLine = Environment.NewLine
                    };

                    // Redirect the console output to a StringWriter.
                    InternalLogger.LogWriter = consoleOutWriter;

                    // Named (based on LogLevel) public methods.

                    InternalLogger.Log(ex1, LogLevel.Warn, "WWW3");
                    InternalLogger.Log(ex2, LogLevel.Error, "EEE3");
                    InternalLogger.Log(ex3, LogLevel.Fatal, "FFF3");
                    InternalLogger.Log(ex4, LogLevel.Trace, "TTT3");
                    InternalLogger.Log(ex5, LogLevel.Debug, "DDD3");
                    InternalLogger.Log(ex6, LogLevel.Info, "III3");

                    consoleOutWriter.Flush();
                    var strings = consoleOutWriter.ToString();
                    Assert.Equal(expected, strings);
                }
                {
                    string expected =
           "Warn WWW4" + prefix + ex1 + Environment.NewLine +
           "Error EEE4" + prefix + ex2 + Environment.NewLine +
           "Fatal FFF4" + prefix + ex3 + Environment.NewLine +
           "Trace TTT4" + prefix + ex4 + Environment.NewLine +
           "Debug DDD4" + prefix + ex5 + Environment.NewLine +
           "Info III4" + Environment.NewLine;

                    StringWriter consoleOutWriter = new StringWriter()
                    {
                        NewLine = Environment.NewLine
                    };

                    // Redirect the console output to a StringWriter.
                    InternalLogger.LogWriter = consoleOutWriter;

                    // Named (based on LogLevel) public methods.

                    InternalLogger.Log(ex1, LogLevel.Warn, () => "WWW4");
                    InternalLogger.Log(ex2, LogLevel.Error, () => "EEE4");
                    InternalLogger.Log(ex3, LogLevel.Fatal, () => "FFF4");
                    InternalLogger.Log(ex4, LogLevel.Trace, () => "TTT4");
                    InternalLogger.Log(ex5, LogLevel.Debug, () => "DDD4");
                    InternalLogger.Log(ex6, LogLevel.Info, () => "III4");

                    var strings = consoleOutWriter.ToString();
                    Assert.Equal(expected, strings);
                }
            }
        }

        [Theory]
        [InlineData("trace", 6)]
        [InlineData("debug", 5)]
        [InlineData("info", 4)]
        [InlineData("warn", 3)]
        [InlineData("error", 2)]
        [InlineData("fatal", 1)]
        [InlineData("off", 0)]
        public void TestMinLevelSwitch_log(string rawLogLevel, int count)
        {
            Action log = () =>
            {
                InternalLogger.Log(LogLevel.Fatal, "L1");
                InternalLogger.Log(LogLevel.Error, "L2");
                InternalLogger.Log(LogLevel.Warn, "L3");
                InternalLogger.Log(LogLevel.Info, "L4");
                InternalLogger.Log(LogLevel.Debug, "L5");
                InternalLogger.Log(LogLevel.Trace, "L6");
            };

            TestMinLevelSwitch_inner(rawLogLevel, count, log);
        }

        [Theory]
        [InlineData("trace", 6)]
        [InlineData("debug", 5)]
        [InlineData("info", 4)]
        [InlineData("warn", 3)]
        [InlineData("error", 2)]
        [InlineData("fatal", 1)]
        [InlineData("off", 0)]
        public void TestMinLevelSwitch(string rawLogLevel, int count)
        {
            Action log = () =>
            {
                InternalLogger.Fatal("L1");
                InternalLogger.Error("L2");
                InternalLogger.Warn("L3");
                InternalLogger.Info("L4");
                InternalLogger.Debug("L5");
                InternalLogger.Trace("L6");
            };

            TestMinLevelSwitch_inner(rawLogLevel, count, log);
        }

        [Theory]
        [InlineData("trace", 6)]
        [InlineData("debug", 5)]
        [InlineData("info", 4)]
        [InlineData("warn", 3)]
        [InlineData("error", 2)]
        [InlineData("fatal", 1)]
        [InlineData("off", 0)]
        public void TestMinLevelSwitch_lambda(string rawLogLevel, int count)
        {
            Action log = () =>
            {
                InternalLogger.Fatal(() => "L1");
                InternalLogger.Error(() => "L2");
                InternalLogger.Warn(() => "L3");
                InternalLogger.Info(() => "L4");
                InternalLogger.Debug(() => "L5");
                InternalLogger.Trace(() => "L6");
            };

            TestMinLevelSwitch_inner(rawLogLevel, count, log);
        }

        private static void TestMinLevelSwitch_inner(string rawLogLevel, int count, Action log)
        {
            try
            {
                //set minimal
                InternalLogger.LogLevel = LogLevel.FromString(rawLogLevel);
                InternalLogger.IncludeTimestamp = false;

                StringWriter consoleOutWriter = new StringWriter()
                {
                    NewLine = ";"
                };

                InternalLogger.LogWriter = consoleOutWriter;

                var expected = "";
                var logLevel = LogLevel.Fatal.Ordinal;
                for (int i = 0; i < count; i++, logLevel--)
                {
                    expected += LogLevel.FromOrdinal(logLevel) + " L" + (i + 1) + ";";
                }

                log();

                var strings = consoleOutWriter.ToString();
                Assert.Equal(expected, strings);
            }
            finally
            {
                InternalLogger.Reset();
            }
        }

        [Theory]
        [InlineData("trace", true)]
        [InlineData("debug", true)]
        [InlineData("info", true)]
        [InlineData("warn", true)]
        [InlineData("error", true)]
        [InlineData("fatal", true)]
        [InlineData("off", false)]
        public void CreateDirectoriesIfNeededTests(string rawLogLevel, bool shouldCreateDirectory)
        {
            var tempPath = Path.GetTempPath();
            var tempFileName = Path.GetRandomFileName();
            var randomSubDirectory = Path.Combine(tempPath, Path.GetRandomFileName());
            string tempFile = Path.Combine(randomSubDirectory, tempFileName);

            try
            {
                InternalLogger.LogLevel = LogLevel.FromString(rawLogLevel);
                InternalLogger.IncludeTimestamp = false;

                if (Directory.Exists(randomSubDirectory))
                {
                    Directory.Delete(randomSubDirectory);
                }
                Assert.False(Directory.Exists(randomSubDirectory));

                // Set the log file, which will only create the needed directories
                InternalLogger.LogFile = tempFile;

                Assert.Equal(Directory.Exists(randomSubDirectory), shouldCreateDirectory);

                Assert.False(File.Exists(tempFile));

                InternalLogger.Log(LogLevel.FromString(rawLogLevel), "File and Directory created.");

                Assert.Equal(File.Exists(tempFile), shouldCreateDirectory);
            }
            finally
            {
                InternalLogger.Reset();

                if (File.Exists(tempFile))
                {
                    File.Delete(tempFile);
                }

                if (Directory.Exists(randomSubDirectory))
                {
                    Directory.Delete(randomSubDirectory);
                }
            }
        }

        [Fact]
        public void CreateFileInCurrentDirectoryTests()
        {
            string expected =
                    "Warn WWW" + Environment.NewLine +
                    "Error EEE" + Environment.NewLine +
                    "Fatal FFF" + Environment.NewLine +
                    "Trace TTT" + Environment.NewLine +
                    "Debug DDD" + Environment.NewLine +
                    "Info III" + Environment.NewLine;

            // Store off the previous log file
            string previousLogFile = InternalLogger.LogFile;

            var tempFileName = Path.GetRandomFileName();

            try
            {
                InternalLogger.LogLevel = LogLevel.Trace;
                InternalLogger.IncludeTimestamp = false;

                Assert.False(File.Exists(tempFileName));

                // Set the log file, which only has a filename
                InternalLogger.LogFile = tempFileName;

                Assert.False(File.Exists(tempFileName));

                // Invoke Log(LogLevel, string) for every log level.
                InternalLogger.Log(LogLevel.Warn, "WWW");
                InternalLogger.Log(LogLevel.Error, "EEE");
                InternalLogger.Log(LogLevel.Fatal, "FFF");
                InternalLogger.Log(LogLevel.Trace, "TTT");
                InternalLogger.Log(LogLevel.Debug, "DDD");
                InternalLogger.Log(LogLevel.Info, "III");

                AssertFileContents(tempFileName, expected, Encoding.UTF8);
                Assert.True(File.Exists(tempFileName));
            }
            finally
            {
                InternalLogger.Reset();
                if (File.Exists(tempFileName))
                {
                    File.Delete(tempFileName);
                }
            }
        }

        [Fact]
        public void TestReceivedLogEventTest()
        {
            using (var loggerScope = new InternalLoggerScope())
            {
                // Arrange
                var receivedArgs = new List<InternalLoggerMessageEventArgs>();
                var logFactory = new LogFactory();
                logFactory.Setup().SetupInternalLogger(s =>
                {
                    EventHandler<InternalLoggerMessageEventArgs> eventHandler = (sender, e) => receivedArgs.Add(e);
                    s.AddLogSubscription(eventHandler);
                    s.RemoveLogSubscription(eventHandler);
                    s.AddLogSubscription(eventHandler);
                });
                var exception = new Exception();

                // Act
                InternalLogger.Info(exception, "Hello {0}", "it's me!");

                // Assert
                Assert.Single(receivedArgs);
                var logEventArgs = receivedArgs.Single();
                Assert.Equal(LogLevel.Info, logEventArgs.Level);
                Assert.Equal(exception, logEventArgs.Exception);
                Assert.Equal("Hello it's me!", logEventArgs.Message);
            }
        }

        [Fact]
        public void TestReceivedLogEventThrowingTest()
        {
            using (var loggerScope = new InternalLoggerScope())
            {
                // Arrange
                var receivedArgs = new List<InternalLoggerMessageEventArgs>();
                InternalLogger.LogMessageReceived += (sender, e) =>
                {
                    receivedArgs.Add(e);
                    throw new ApplicationException("I'm a bad programmer");
                };
                var exception = new Exception();

                // Act
                InternalLogger.Info(exception, "Hello {0}", "it's me!");

                // Assert
                Assert.Single(receivedArgs);
                var logEventArgs = receivedArgs.Single();
                Assert.Equal(LogLevel.Info, logEventArgs.Level);
                Assert.Equal(exception, logEventArgs.Exception);
                Assert.Equal("Hello it's me!", logEventArgs.Message);
            }
        }

        [Fact]
        public void TestReceivedLogEventContextTest()
        {
            using (var loggerScope = new InternalLoggerScope())
            {
                // Arrange
                var targetContext = new NLog.Targets.DebugTarget() { Name = "Ugly" };
                var receivedArgs = new List<InternalLoggerMessageEventArgs>();
                InternalLogger.LogMessageReceived += (sender, e) =>
                {
                    receivedArgs.Add(e);
                };
                var exception = new Exception();

                // Act
                NLog.Internal.ExceptionHelper.MustBeRethrown(exception, targetContext);

                // Assert
                Assert.Single(receivedArgs);
                var logEventArgs = receivedArgs.Single();
                Assert.Equal(LogLevel.Error, logEventArgs.Level);
                Assert.Equal(exception, logEventArgs.Exception);
                Assert.Equal(targetContext.GetType(), logEventArgs.SenderType);
            }
        }

        public void Dispose()
        {
            TimeSource.Current = new FastLocalTimeSource();
            InternalLogger.Reset();
        }
    }
}
