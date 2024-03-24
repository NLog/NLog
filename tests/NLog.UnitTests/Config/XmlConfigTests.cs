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


using System;
using System.IO;
using NLog.Common;
using NLog.Config;
using NLog.Targets.Wrappers;
using Xunit;

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
                Assert.False(InternalLogger.LogToConsole);
                Assert.False(InternalLogger.LogToConsoleError);
                Assert.True(InternalLogger.IncludeTimestamp);
                Assert.Null(InternalLogger.LogWriter);
                Assert.Equal(LogLevel.Off, InternalLogger.LogLevel);
            }
        }

        [Fact]
        public void ParseNLogOptionsTest()
        {
            using (new InternalLoggerScope())
            {
                using (new NoThrowNLogExceptions())
                {
                    var xml = "<nlog logfile='test.txt' internalLogIncludeTimestamp='false' internalLogToConsole='true' internalLogToConsoleError='true'></nlog>";
                    var config = XmlLoggingConfiguration.CreateFromXmlString(xml);

                    Assert.False(config.AutoReload);
                    Assert.True(config.InitializeSucceeded);
                    Assert.Equal("", InternalLogger.LogFile);
                    Assert.True(InternalLogger.LogToConsole);
                    Assert.True(InternalLogger.LogToConsoleError);
                    Assert.False(InternalLogger.IncludeTimestamp);
                    Assert.Null(InternalLogger.LogWriter);
                    Assert.Equal(LogLevel.Info, InternalLogger.LogLevel);
                }
            }
        }

        [Fact]
        public void ParseNLogInternalLoggerPathTest()
        {
            using (new InternalLoggerScope())
            {
                var xml = "<nlog internalLogFile='${CurrentDir}test.txt'></nlog>";
                var config = XmlLoggingConfiguration.CreateFromXmlString(xml);
                Assert.Contains(System.IO.Directory.GetCurrentDirectory(), InternalLogger.LogFile);
            }

            using (new InternalLoggerScope())
            {
                var xml = "<nlog internalLogFile='${BaseDir}test.txt'></nlog>";
                var config = XmlLoggingConfiguration.CreateFromXmlString(xml);
                Assert.Contains(AppDomain.CurrentDomain.BaseDirectory, InternalLogger.LogFile);
            }

            using (new InternalLoggerScope())
            {
                var xml = "<nlog internalLogFile='${TempDir}test.txt'></nlog>";
                var config = XmlLoggingConfiguration.CreateFromXmlString(xml);
                Assert.Contains(System.IO.Path.GetTempPath(), InternalLogger.LogFile);
            }

#if !NETSTANDARD1_3
            using (new InternalLoggerScope())
            {
                var xml = "<nlog internalLogFile='${ProcessDir}test.txt'></nlog>";
                var config = XmlLoggingConfiguration.CreateFromXmlString(xml);
                Assert.Contains(Path.GetDirectoryName(CurrentProcessPath), InternalLogger.LogFile);
            }
#endif

#if !NETSTANDARD1_3 && !NETSTANDARD1_5
            using (new InternalLoggerScope())
            {
                var xml = "<nlog internalLogFile='${CommonApplicationDataDir}test.txt'></nlog>";
                var config = XmlLoggingConfiguration.CreateFromXmlString(xml);
                Assert.Contains(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), InternalLogger.LogFile);
            }

            using (new InternalLoggerScope())
            {
                var xml = "<nlog internalLogFile='${UserApplicationDataDir}test.txt'></nlog>";
                var config = XmlLoggingConfiguration.CreateFromXmlString(xml);
                Assert.Contains(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), InternalLogger.LogFile);
            }

            using (new InternalLoggerScope())
            {
                var xml = "<nlog internalLogFile='${UserLocalApplicationDataDir}test.txt'></nlog>";
                var config = XmlLoggingConfiguration.CreateFromXmlString(xml);
                Assert.Contains(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), InternalLogger.LogFile);
            }
#endif

            using (new InternalLoggerScope())
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
            using (new InternalLoggerScope())
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
            using (new InternalLoggerScope())
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

                var logFactory = new LogFactory();
                var config = XmlLoggingConfiguration.CreateFromXmlString(xml, logFactory);
                logFactory.Configuration = config;
                var logger = logFactory.GetLogger("InvalidInternalLogLevel_shouldNotBreakLogging");

                // Act
                logger.Debug("message 1");

                // Assert
                logFactory.AssertDebugLastMessage("message 1");
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
                        <logger name='*' minlevel='debug' appendto='debug' filterDefaultAction='ignore'>
                            <filters defaultAction='log'>
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
        [InlineData("xsi", false)]
        [InlineData("test", false)]
        [InlineData("xsi", true)]
        [InlineData("test", true)]
        public void XmlConfig_attributes_shouldNotLogWarningsToInternalLog(string @namespace, bool nestedConfig)
        {
            // Arrange
            var xml = $@"<?xml version=""1.0"" encoding=""utf-8""?>
{(nestedConfig ? "<configuration>" : "")}
<nlog xmlns=""http://www.nlog-project.org/schemas/NLog.xsd"" 
      xmlns:{@namespace}=""http://www.w3.org/2001/XMLSchema-instance"" 
      {@namespace}:schemaLocation=""somewhere"" 
      internalLogToConsole=""true"" internalLogLevel=""Warn"">
</nlog>
{(nestedConfig ? "</configuration>" : "")}";

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

        [Fact]
        public void RulesBeforeTargetsTest()
        {
            LoggingConfiguration c = XmlLoggingConfiguration.CreateFromXmlString(@"
            <nlog>
                <rules>
                    <logger name='*' minLevel='Info' writeTo='d1' />
                </rules>

                <targets>
                    <target name='d1' type='Debug' />
                </targets>
            </nlog>");

            Assert.Single(c.LoggingRules);
            var rule = c.LoggingRules[0];
            Assert.Equal("*", rule.LoggerNamePattern);
            Assert.Equal(4, rule.Levels.Count);
            Assert.Contains(LogLevel.Info, rule.Levels);
            Assert.Contains(LogLevel.Warn, rule.Levels);
            Assert.Contains(LogLevel.Error, rule.Levels);
            Assert.Contains(LogLevel.Fatal, rule.Levels);
            Assert.Single(rule.Targets);
            Assert.Same(c.FindTargetByName("d1"), rule.Targets[0]);
            Assert.False(rule.Final);
            Assert.Empty(rule.Filters);
        }

        [Fact]
        public void LowerCaseParserTest()
        {
            var logFactory = new LogFactory().Setup().LoadConfigurationFromXml(@"
            <nlog>
                <targets><target name='debug' type='debug' layout='${level}' /></targets>
                <rules>
                    <logger name='*' minlevel='info' appendto='debug'>
                        <filters defaultAction='log'>
                            <whencontains layout='${message}' substring='msg' action='ignore' />
                        </filters>
                    </logger>
                </rules>
            </nlog>").LogFactory;

            var logger = logFactory.GetLogger("A");

            logger.Fatal("message");
            logFactory.AssertDebugLastMessage(nameof(LogLevel.Fatal));

            logger.Error("message");
            logFactory.AssertDebugLastMessage(nameof(LogLevel.Error));

            logger.Warn("message");
            logFactory.AssertDebugLastMessage(nameof(LogLevel.Warn));

            logger.Info("message");
            logFactory.AssertDebugLastMessage(nameof(LogLevel.Info));

            logger.Debug("message");
            logger.Debug("msg");
            logger.Info("msg");
            logger.Warn("msg");
            logger.Error("msg");
            logger.Fatal("msg");
            logFactory.AssertDebugLastMessage(nameof(LogLevel.Info));
        }

        [Fact]
        public void UpperCaseParserTest()
        {
            var logFactory = new LogFactory().Setup().LoadConfigurationFromXml(@"
            <nlog throwExceptions='true'>
                <TARGETS><TARGET NAME='DEBUG' TYPE='DEBUG' LAYOUT='${LEVEL}' /></TARGETS>
                <RULES>
                    <LOGGER NAME='*' MINLEVEL='INFO' APPENDTO='DEBUG'>
                        <FILTERS DEFAULTACTION='LOG'>
                            <WHENCONTAINS LAYOUT='${MESSAGE}' SUBSTRING='msg' ACTION='IGNORE' />
                        </FILTERS>
                    </LOGGER>
                </RULES>
            </nlog>").LogFactory;

            var logger = logFactory.GetLogger("A");

            logger.Fatal("message");
            logFactory.AssertDebugLastMessage(nameof(LogLevel.Fatal));

            logger.Error("message");
            logFactory.AssertDebugLastMessage(nameof(LogLevel.Error));

            logger.Warn("message");
            logFactory.AssertDebugLastMessage(nameof(LogLevel.Warn));

            logger.Info("message");
            logFactory.AssertDebugLastMessage(nameof(LogLevel.Info));

            logger.Debug("message");
            logger.Debug("msg");
            logger.Info("msg");
            logger.Warn("msg");
            logger.Error("msg");
            logger.Fatal("msg");
            logFactory.AssertDebugLastMessage(nameof(LogLevel.Info));
        }

        [Fact]
        public void ShouldWriteLogsOnDuplicateAttributeTest()
        {
            using (new NoThrowNLogExceptions())
            {
                var logFactory = new LogFactory().Setup().LoadConfigurationFromXml(@"
                    <nlog>
                        <targets><target name='debug' type='debug' layout='${message}' /></targets>
                        <rules>
                            <logger name='*' minlevel='info' minLevel='info' appendto='debug'>
                               <filters defaultAction='log'>
                                    <whencontains layout='${message}' substring='msg' action='ignore' />
                                </filters>
                            </logger>
                        </rules>
                    </nlog>").LogFactory;

                var logger = logFactory.GetLogger("A");
                string expectedMesssage = "some message";
                logger.Info(expectedMesssage);
                logFactory.AssertDebugLastMessage(expectedMesssage);
            }
        }

        [Fact]
        public void ShoudThrowExceptionOnDuplicateAttributeWhenOptionIsEnabledTest()
        {
            Assert.Throws<NLogConfigurationException>(() =>
            {
                new LogFactory().Setup().LoadConfigurationFromXml(@"
                <nlog throwExceptions='true'>
                    <targets><target name='debug' type='debug' layout='${message}' /></targets>
                    <rules>
                        <logger name='*' minlevel='info' minLevel='info' appendto='debug'>
                           <filters defaultAction='log'>
                                <whencontains layout='${message}' substring='msg' action='ignore' />
                            </filters>
                        </logger>
                    </rules>
                </nlog>");
            });

            Assert.Throws<NLogConfigurationException>(() =>
            {
                new LogFactory().Setup().LoadConfigurationFromXml(@"
                <nlog throwConfigExceptions='true'>
                    <targets><target name='debug' type='debug' layout='${message}' /></targets>
                    <rules>
                        <logger name='*' minlevel='info' minLevel='info' appendto='debug'>
                           <filters defaultAction='log'>
                                <whencontains layout='${message}' substring='msg' action='ignore' />
                            </filters>
                        </logger>
                    </rules>
                </nlog>");
            });
        }
    }
}

