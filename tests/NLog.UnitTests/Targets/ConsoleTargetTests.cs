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

namespace NLog.UnitTests.Targets
{
    using System;
    using System.IO;
    using NLog.Targets;
    using System.Collections.Generic;
    using Xunit;
    using System.Threading.Tasks;

    public class ConsoleTargetTests : NLogTestBase
    {
        [Fact]
        public void ConsoleOutWriteLineTest()
        {
            ConsoleOutTest(false);
        }

        [Fact]
        public void ConsoleOutWriteBufferTest()
        {
            ConsoleOutTest(true);
        }

        private void ConsoleOutTest(bool writeBuffer)
        {
            var target = new ConsoleTarget()
            {
                Header = "-- header --",
                Layout = "${logger} ${message}",
                Footer = "-- footer --",
                WriteBuffer = writeBuffer,
            };

            var consoleOutWriter = new StringWriter();
            TextWriter oldConsoleOutWriter = Console.Out;
            Console.SetOut(consoleOutWriter);

            try
            {
                var exceptions = new List<Exception>();
                target.Initialize(null);
                target.WriteAsyncLogEvent(new LogEventInfo(LogLevel.Info, "Logger1", "message1").WithContinuation(exceptions.Add));
                target.WriteAsyncLogEvent(new LogEventInfo(LogLevel.Info, "Logger1", "message2").WithContinuation(exceptions.Add));
                target.WriteAsyncLogEvents(
                    new LogEventInfo(LogLevel.Info, "Logger1", "message3").WithContinuation(exceptions.Add),
                    new LogEventInfo(LogLevel.Info, "Logger2", "message4").WithContinuation(exceptions.Add),
                    new LogEventInfo(LogLevel.Info, "Logger2", "message5").WithContinuation(exceptions.Add),
                    new LogEventInfo(LogLevel.Info, "Logger1", "message6").WithContinuation(exceptions.Add));
                Assert.Equal(6, exceptions.Count);
                target.Flush((ex) => { });
                target.Close();
            }
            finally
            {
                Console.SetOut(oldConsoleOutWriter);
            }

            var actual = consoleOutWriter.ToString();

            Assert.True(actual.IndexOf("-- header --") != -1);
            Assert.True(actual.IndexOf("Logger1 message1") != -1);
            Assert.True(actual.IndexOf("Logger1 message2") != -1);
            Assert.True(actual.IndexOf("Logger1 message3") != -1);
            Assert.True(actual.IndexOf("Logger2 message4") != -1);
            Assert.True(actual.IndexOf("Logger2 message5") != -1);
            Assert.True(actual.IndexOf("Logger1 message6") != -1);
            Assert.True(actual.IndexOf("-- footer --") != -1);
        }

        [Fact]
        public void ConsoleErrorTest()
        {
            var target = new ConsoleTarget()
            {
                Header = "-- header --",
                Layout = "${logger} ${message}",
                Footer = "-- footer --",
                Error = true,
            };

            var consoleErrorWriter = new StringWriter();
            TextWriter oldConsoleErrorWriter = Console.Error;
            Console.SetError(consoleErrorWriter);

            try
            {
                var exceptions = new List<Exception>();
                target.Initialize(null);
                target.WriteAsyncLogEvent(new LogEventInfo(LogLevel.Info, "Logger1", "message1").WithContinuation(exceptions.Add));
                target.WriteAsyncLogEvent(new LogEventInfo(LogLevel.Info, "Logger1", "message2").WithContinuation(exceptions.Add));
                target.WriteAsyncLogEvents(
                    new LogEventInfo(LogLevel.Info, "Logger1", "message3").WithContinuation(exceptions.Add),
                    new LogEventInfo(LogLevel.Info, "Logger2", "message4").WithContinuation(exceptions.Add),
                    new LogEventInfo(LogLevel.Info, "Logger2", "message5").WithContinuation(exceptions.Add),
                    new LogEventInfo(LogLevel.Info, "Logger1", "message6").WithContinuation(exceptions.Add));
                Assert.Equal(6, exceptions.Count);
                target.Flush((ex) => { });
                target.Close();
            }
            finally
            {
                Console.SetError(oldConsoleErrorWriter);
            }

            string expectedResult = string.Format("-- header --{0}Logger1 message1{0}Logger1 message2{0}Logger1 message3{0}Logger2 message4{0}Logger2 message5{0}Logger1 message6{0}-- footer --{0}", Environment.NewLine);
            Assert.Equal(expectedResult, consoleErrorWriter.ToString());
        }

#if !MONO
        [Fact]
        public void ConsoleEncodingTest()
        {
            var consoleOutputEncoding = Console.OutputEncoding;

            var target = new ConsoleTarget()
            {
                Header = "-- header --",
                Layout = "${logger} ${message}",
                Footer = "-- footer --",
                Encoding = System.Text.Encoding.UTF8
            };

            Assert.Equal(System.Text.Encoding.UTF8, target.Encoding);

            var consoleOutWriter = new StringWriter();
            TextWriter oldConsoleOutWriter = Console.Out;
            Console.SetOut(consoleOutWriter);

            try
            {
                var exceptions = new List<Exception>();
                target.Initialize(null);
                // Not really testing whether Console.OutputEncoding works, but just that it is configured without breaking ConsoleTarget
                Assert.Equal(System.Text.Encoding.UTF8, Console.OutputEncoding);
                Assert.Equal(System.Text.Encoding.UTF8, target.Encoding);
                target.WriteAsyncLogEvent(new LogEventInfo(LogLevel.Info, "Logger1", "message1").WithContinuation(exceptions.Add));
                target.WriteAsyncLogEvent(new LogEventInfo(LogLevel.Info, "Logger1", "message2").WithContinuation(exceptions.Add));
                Assert.Equal(2, exceptions.Count);
                target.Encoding = consoleOutputEncoding;
                Assert.Equal(consoleOutputEncoding, Console.OutputEncoding);
                target.Close();
            }
            finally
            {
                Console.OutputEncoding = consoleOutputEncoding;
                Console.SetOut(oldConsoleOutWriter);
            }

            string expectedResult = string.Format("-- header --{0}Logger1 message1{0}Logger1 message2{0}-- footer --{0}", Environment.NewLine);
            Assert.Equal(expectedResult, consoleOutWriter.ToString());
        }

#endif

#if !MONO
        [Fact]
        public void ConsoleRaceCondtionIgnoreTest()
        {
            var configXml = @"
            <nlog throwExceptions='true'>
                <targets>
                  <target name='console' type='console' layout='${message}' />
                  <target name='consoleError' type='console' layout='${message}'  error='true' />
                </targets>
                <rules>
                  <logger name='*' minlevel='Trace' writeTo='console,consoleError' />
                </rules>
            </nlog>";

            ConsoleRaceCondtionIgnoreInnerTest(configXml);
        }

        internal static void ConsoleRaceCondtionIgnoreInnerTest(string configXml)
        {
            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(configXml);

            //   Console.Out.Writeline / Console.Error.Writeline could throw 'IndexOutOfRangeException', which is a bug. 
            // See https://stackoverflow.com/questions/33915790/console-out-and-console-error-race-condition-error-in-a-windows-service-written
            // and https://connect.microsoft.com/VisualStudio/feedback/details/2057284/console-out-probable-i-o-race-condition-issue-in-multi-threaded-windows-service
            //             
            // Full error: 
            //   Error during session close: System.IndexOutOfRangeException: Probable I/ O race condition detected while copying memory.
            //   The I/ O package is not thread safe by default.In multithreaded applications, 
            //   a stream must be accessed in a thread-safe way, such as a thread - safe wrapper returned by TextReader's or 
            //   TextWriter's Synchronized methods.This also applies to classes like StreamWriter and StreamReader.

            var oldOut = Console.Out;
            var oldError = Console.Error;

            try
            {
                Console.SetOut(StreamWriter.Null);
                Console.SetError(StreamWriter.Null);


                LogManager.ThrowExceptions = true;
                var logger = LogManager.GetCurrentClassLogger();


                Parallel.For(0, 10, new ParallelOptions() { MaxDegreeOfParallelism = 10 }, (_) =>
                  {
                      for (int i = 0; i < 100; i++)
                      {
                          logger.Trace("test message to the out and error stream");
                      }
                  });
            }
            finally
            {
                Console.SetOut(oldOut);
                Console.SetError(oldError);
            }
        }
#endif
    }
}