// 
// Copyright (c) 2004-2011 Jaroslaw Kowalski <jaak@jkowalski.net>
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
using System.Globalization;
using System.IO;
using System.Linq;
using Xunit;
using NLog.Common;
using System.Text;

namespace NLog.UnitTests.Common
{
    public class InternalLoggerTests : NLogTestBase
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
            // Expected result is the same for both types of method invocation.
            const string expected = "Warn WWW\nError EEE\nFatal FFF\nTrace TTT\nDebug DDD\nInfo III\n";

            InternalLogger.LogLevel = LogLevel.Trace;
            InternalLogger.IncludeTimestamp = false;

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

            Assert.True(writer1.ToString() == expected);

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

            Assert.True(writer2.ToString() == expected);
        }

        [Fact]
        public void WriteToStringWriterWithArgsTests()
        {
            // Expected result is the same for both types of method invocation.
            const string expected = "Warn WWW 0\nError EEE 0, 1\nFatal FFF 0, 1, 2\nTrace TTT 0, 1, 2\nDebug DDD 0, 1\nInfo III 0\n";

            InternalLogger.LogLevel = LogLevel.Trace;
            InternalLogger.IncludeTimestamp = false;

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

            Assert.True(writer1.ToString() == expected);

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

            Assert.True(writer2.ToString() == expected);
        }

#if !SILVERLIGHT
        [Fact]
        public void WriteToConsoleOutTests()
        {
            // Expected result is the same for both types of method invocation.
            const string expected = "Warn WWW\nError EEE\nFatal FFF\nTrace TTT\nDebug DDD\nInfo III\n";

            InternalLogger.LogLevel = LogLevel.Trace;
            InternalLogger.IncludeTimestamp = false;
            InternalLogger.LogToConsole = true;

            StringWriter consoleOutWriter1 = new StringWriter()
                                        {
                                            NewLine = "\n"
                                        };

            // Redirect the console output to a StringWriter.
            Console.SetOut(consoleOutWriter1);

            // Named (based on LogLevel) public methods.
            InternalLogger.Warn("WWW");
            InternalLogger.Error("EEE");
            InternalLogger.Fatal("FFF");
            InternalLogger.Trace("TTT");
            InternalLogger.Debug("DDD");
            InternalLogger.Info("III");

            Assert.True(consoleOutWriter1.ToString() == expected);

            //
            // Redirect the console output to another StringWriter.

            StringWriter consoleOutWriter2 = new StringWriter()
                                        {
                                            NewLine = "\n"
                                        };
            Console.SetOut(consoleOutWriter2);

            // Invoke Log(LogLevel, string) for every log level.
            InternalLogger.Log(LogLevel.Warn, "WWW");
            InternalLogger.Log(LogLevel.Error, "EEE");
            InternalLogger.Log(LogLevel.Fatal, "FFF");
            InternalLogger.Log(LogLevel.Trace, "TTT");
            InternalLogger.Log(LogLevel.Debug, "DDD");
            InternalLogger.Log(LogLevel.Info, "III");

            Assert.True(consoleOutWriter2.ToString() == expected);
        }

        [Fact]
        public void WriteToConsoleErrorTests()
        {
            // Expected result is the same for both types of method invocation.
            const string expected = "Warn WWW\nError EEE\nFatal FFF\nTrace TTT\nDebug DDD\nInfo III\n";

            InternalLogger.LogLevel = LogLevel.Trace;
            InternalLogger.IncludeTimestamp = false;
            InternalLogger.LogToConsoleError = true;

            StringWriter consoleWriter1 = new StringWriter()
            {
                NewLine = "\n"
            };

            // Redirect the console output to a StringWriter.
            Console.SetError(consoleWriter1);

            // Named (based on LogLevel) public methods.
            InternalLogger.Warn("WWW");
            InternalLogger.Error("EEE");
            InternalLogger.Fatal("FFF");
            InternalLogger.Trace("TTT");
            InternalLogger.Debug("DDD");
            InternalLogger.Info("III");

            Assert.True(consoleWriter1.ToString() == expected);

            //
            // Redirect the console output to another StringWriter.

            StringWriter consoleWriter2 = new StringWriter()
            {
                NewLine = "\n"
            };
            Console.SetError(consoleWriter2);

            // Invoke Log(LogLevel, string) for every log level.
            InternalLogger.Log(LogLevel.Warn, "WWW");
            InternalLogger.Log(LogLevel.Error, "EEE");
            InternalLogger.Log(LogLevel.Fatal, "FFF");
            InternalLogger.Log(LogLevel.Trace, "TTT");
            InternalLogger.Log(LogLevel.Debug, "DDD");
            InternalLogger.Log(LogLevel.Info, "III");

            Assert.True(consoleWriter2.ToString() == expected);
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

            InternalLogger.LogLevel = LogLevel.Trace;
            InternalLogger.IncludeTimestamp = false;
            InternalLogger.LogFile = tempFile;

            try
            {
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
                if (File.Exists(tempFile))
                {
                    File.Delete(tempFile);
                }
            }
        }

        [Fact]
        public void TimestampTests()
        {
            InternalLogger.LogLevel = LogLevel.Trace;
            InternalLogger.IncludeTimestamp = true;
            InternalLogger.LogToConsole = true;

            StringWriter consoleOutWriter = new StringWriter()
            {
                NewLine = "\n"
            };

            // Redirect the console output to a StringWriter.
            Console.SetOut(consoleOutWriter);            

            // Named (based on LogLevel) public methods.
            InternalLogger.Warn("WWW");
            InternalLogger.Error("EEE");
            InternalLogger.Fatal("FFF");
            InternalLogger.Trace("TTT");
            InternalLogger.Debug("DDD");
            InternalLogger.Info("III");

            string expectedDateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);

            var strings = consoleOutWriter.ToString().Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var str in strings)
            {
                Assert.Contains(expectedDateTime + ".", str);
            }
        }

        [Fact]
        public void CreateDirectoriesIfNeededTests()
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

            var tempPath = Path.GetTempPath();
            var tempFileName = Path.GetRandomFileName();
            var randomSubDirectory = Path.Combine(tempPath, Path.GetRandomFileName());
            string tempFile = Path.Combine(randomSubDirectory, tempFileName);

            InternalLogger.LogLevel = LogLevel.Trace;
            InternalLogger.IncludeTimestamp = false;

            Assert.False(Directory.Exists(randomSubDirectory));

            // Set the log file, which will only create the needed directories
            InternalLogger.LogFile = tempFile;

            Assert.True(Directory.Exists(randomSubDirectory));

            try
            {
                Assert.False(File.Exists(tempFile));

                // Invoke Log(LogLevel, string) for every log level.
                InternalLogger.Log(LogLevel.Warn, "WWW");
                InternalLogger.Log(LogLevel.Error, "EEE");
                InternalLogger.Log(LogLevel.Fatal, "FFF");
                InternalLogger.Log(LogLevel.Trace, "TTT");
                InternalLogger.Log(LogLevel.Debug, "DDD");
                InternalLogger.Log(LogLevel.Info, "III");

                AssertFileContents(tempFile, expected, Encoding.UTF8);
                Assert.True(File.Exists(tempFile));
            }
            finally
            {
                // Reset LogFile to the previous value
                InternalLogger.LogFile = previousLogFile;

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
#endif
    }
}
