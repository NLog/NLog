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
using System.IO;
using NLog.Common;
using NLog.Config;
using NLog.Targets.Wrappers;
using Xunit;
using Xunit.Extensions;

namespace NLog.UnitTests.Config
{
    public class XmlConfigTests : NLogTestBase
    {
        [Fact]
        public void ParseNLogOptionsDefaultTest()
        {
            using (new InternalLoggerScope())
            {
                var xml = "<nlog></nlog>";
                var config = XmlLoggingConfiguration.CreateFromXmlString(xml);

                Assert.False(config.AutoReload);
                Assert.True(config.InitializeSucceeded);
                Assert.Equal("", InternalLogger.LogFile);
                Assert.True(InternalLogger.IncludeTimestamp);
                Assert.False(InternalLogger.LogToConsole);
                Assert.False(InternalLogger.LogToConsoleError);
                Assert.Null(InternalLogger.LogWriter);
                Assert.Equal(LogLevel.Off, InternalLogger.LogLevel);
            }
        }

        [Fact]
        public void ParseNLogOptionsTest()
        {
            using (new InternalLoggerScope(true))
            {
                using (new NoThrowNLogExceptions())
                {
                    var xml = "<nlog logfile='test.txt' internalLogIncludeTimestamp='false' internalLogToConsole='true' internalLogToConsoleError='true'></nlog>";
                    var config = XmlLoggingConfiguration.CreateFromXmlString(xml);

                    Assert.False(config.AutoReload);
                    Assert.True(config.InitializeSucceeded);
                    Assert.Equal("", InternalLogger.LogFile);
                    Assert.False(InternalLogger.IncludeTimestamp);
                    Assert.True(InternalLogger.LogToConsole);
                    Assert.True(InternalLogger.LogToConsoleError);
                    Assert.Null(InternalLogger.LogWriter);
                    Assert.Equal(LogLevel.Info, InternalLogger.LogLevel);
                }
            }
        }

        [Fact]
        public void ParseNLogInternalLoggerPathTest()
        {
            using (new InternalLoggerScope(true))
            {
                var xml = "<nlog internalLogFile='${CurrentDir}test.txt'></nlog>";
                var config = XmlLoggingConfiguration.CreateFromXmlString(xml);
                Assert.Contains(System.IO.Directory.GetCurrentDirectory(), InternalLogger.LogFile);
            }

            using (new InternalLoggerScope(true))
            {
                var xml = "<nlog internalLogFile='${BaseDir}test.txt'></nlog>";
                var config = XmlLoggingConfiguration.CreateFromXmlString(xml);
                Assert.Contains(AppDomain.CurrentDomain.BaseDirectory, InternalLogger.LogFile);
            }

            using (new InternalLoggerScope(true))
            {
                var xml = "<nlog internalLogFile='${TempDir}test.txt'></nlog>";
                var config = XmlLoggingConfiguration.CreateFromXmlString(xml);
                Assert.Contains(System.IO.Path.GetTempPath(), InternalLogger.LogFile);
            }

#if !NETSTANDARD1_3
            using (new InternalLoggerScope(true))
            {
                var xml = "<nlog internalLogFile='${ProcessDir}test.txt'></nlog>";
                var config = XmlLoggingConfiguration.CreateFromXmlString(xml);
                Assert.Contains(Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess()?.MainModule?.FileName), InternalLogger.LogFile);
            }
#endif

            using (new InternalLoggerScope(true))
            {
                var userName = Environment.GetEnvironmentVariable("USERNAME") ?? string.Empty;
                var xml = "<nlog internalLogFile='%USERNAME%_test.txt'></nlog>";
                var config = XmlLoggingConfiguration.CreateFromXmlString(xml);
                if (!string.IsNullOrEmpty(userName))
                    Assert.Contains(userName, InternalLogger.LogFile);
            }
        }

        [Theory]
        [InlineData("0:0:0:1", 1)]
        [InlineData("0:0:1", 1)]
        [InlineData("0:1", 60)] //1 minute
        [InlineData("0:1:0", 60)]
        [InlineData("00:00:00:1", 1)]
        [InlineData("000:0000:000:001", 1)]
        [InlineData("0:0:1:1", 61)]
        [InlineData("1:0:0", 3600)] // 1 hour
        [InlineData("2:3:4", 7384)]
        [InlineData("1:0:0:0", 86400)] //1 day
        public void SetTimeSpanFromXmlTest(string interval, int seconds)
        {
            var config = XmlLoggingConfiguration.CreateFromXmlString($@"
            <nlog throwExceptions='true'>
                <targets>
                    <wrapper-target name='limiting' type='LimitingWrapper' messagelimit='5'  interval='{interval}'>
                        <target name='debug' type='Debug' layout='${{message}}' />
                    </wrapper-target>
                </targets>
                <rules>
                    <logger name='*' level='Debug' writeTo='limiting' />
                </rules>
            </nlog>");

            var target = config.FindTargetByName<LimitingTargetWrapper>("limiting");
            Assert.NotNull(target);
            Assert.Equal(TimeSpan.FromSeconds(seconds), target.Interval);
        }

        [Fact]
        public void InvalidInternalLogLevel_shouldNotSetLevel()
        {
            using (new InternalLoggerScope(true))
            using (new NoThrowNLogExceptions())
            {
                // Arrange
                InternalLogger.LogLevel = LogLevel.Error;
                var xml = @"<nlog  internalLogLevel='bogus' >
                    </nlog>";

                // Act
                XmlLoggingConfiguration.CreateFromXmlString(xml);

                // Assert
                Assert.Equal(LogLevel.Error, InternalLogger.LogLevel);
            }
        }

        [Fact]
        public void InvalidNLogAttributeValues_shouldNotBreakLogging()
        {
            using (new InternalLoggerScope(true))
            using (new NoThrowNLogExceptions())
            {
                // Arrange
                var xml = @"<nlog internalLogLevel='oops' globalThreshold='noooos'>
                        <targets>
                            <target name='debug' type='Debug' layout='${message}' />
                        </targets>
                        <rules>
                            <logger name='*' minlevel='debug' appendto='debug' />
                         </rules>
                    </nlog>";
                var config = XmlLoggingConfiguration.CreateFromXmlString(xml);
                LogManager.Configuration = config;
                var logger = LogManager.GetLogger("InvalidInternalLogLevel_shouldNotBreakLogging");

                // Act
                logger.Debug("message 1");

                // Assert
                var message = GetDebugLastMessage("debug");
                Assert.Equal("message 1", message);
            }
        }

        [Fact]
        public void XmlConfig_ParseUtf8Encoding_WithoutHyphen()
        {
            // Arrange
            var xml = @"<nlog>
                    <targets>
                        <target name='file' type='File' encoding='utf8' layout='${message}' fileName='hello.txt' />
                    </targets>
                    <rules>
                        <logger name='*' minlevel='debug' appendto='file' />
                    </rules>
                </nlog>";
            var config = XmlLoggingConfiguration.CreateFromXmlString(xml);

            Assert.Single(config.AllTargets);
            Assert.Equal(System.Text.Encoding.UTF8, (config.AllTargets[0] as NLog.Targets.FileTarget)?.Encoding);
        }

        [Fact]
        public void XmlConfig_ParseFilter_WithoutAttributes()
        {
            // Arrange
            var xml = @"<nlog>
                    <targets>
                        <target name='debug' type='Debug' layout='${message}' />
                    </targets>
                    <rules>
                        <logger name='*' minlevel='debug' appendto='debug'>
                            <filters defaultFilterResult='ignore'>
                                <whenContains />
                            </filters>
                        </logger>
                    </rules>
                </nlog>";

            var config = XmlLoggingConfiguration.CreateFromXmlString(xml);
            Assert.Single(config.LoggingRules);
            Assert.Single(config.LoggingRules[0].Filters);
        }

        [Theory]
        [InlineData("xsi")]
        [InlineData("test")]
        public void XmlConfig_attributes_shouldNotLogWarningsToInternalLog(string @namespace)
        {
            // Arrange
            var xml = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<nlog xmlns=""http://www.nlog-project.org/schemas/NLog.xsd"" 
      xmlns:{@namespace}=""http://www.w3.org/2001/XMLSchema-instance"" 
      {@namespace}:schemaLocation=""somewhere"" 
      internalLogToConsole=""true"" internalLogLevel=""Warn"">
</nlog>";

            try
            {
                TextWriter textWriter = new StringWriter();
                InternalLogger.LogWriter = textWriter;
                InternalLogger.IncludeTimestamp = false;

                // Act
                XmlLoggingConfiguration.CreateFromXmlString(xml);

                // Assert
                InternalLogger.LogWriter.Flush();

                var warning = textWriter.ToString();
                Assert.Equal("", warning);
            }
            finally
            {
                // cleanup
                InternalLogger.LogWriter = null;
            }
        }
    }
}

