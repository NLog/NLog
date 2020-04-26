﻿// 
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
using NLog.Common;
using NLog.Config;
using NLog.Targets;
using Xunit;

namespace NLog.UnitTests.Config
{
    public class LogFactorySetupTests
    {
        [Fact]
        public void SetupBuilderGetCurrentClassLogger()
        {
            // Arrange
            var logFactory = new LogFactory();

            // Act
            var logger1 = logFactory.Setup().GetCurrentClassLogger();
            var logger2 = logFactory.GetCurrentClassLogger();

            // Assert
            Assert.Equal(typeof(LogFactorySetupTests).FullName, logger1.Name);
            Assert.Same(logger1, logger2);
        }

        [Fact]
        public void SetupBuilderGetLogger()
        {
            // Arrange
            var logFactory = new LogFactory();

            // Act
            var logger1 = logFactory.Setup().GetLogger(nameof(SetupBuilderGetCurrentClassLogger));
            var logger2 = logFactory.GetLogger(nameof(SetupBuilderGetCurrentClassLogger));
            
            // Assert
            Assert.Equal(nameof(SetupBuilderGetCurrentClassLogger), logger1.Name);
            Assert.Same(logger1, logger2);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void SetupExtensionsAutoLoadExtensionsTest(bool autoLoadAssemblies)
        {
            try
            {
                // Arrange
                var logFactory = new LogFactory();

                // Act
                logFactory.Setup().SetupExtensions(ext => ext.AutoLoadAssemblies(autoLoadAssemblies));
                Func<LogFactory, LoggingConfiguration> buildConfig = (f) => new XmlLoggingConfiguration(@"<nlog throwExceptions='true'>
                    <targets>
                        <target name='t' type='AutoLoadTarget' />
                    </targets>
                    <rules>
                      <logger name='*' writeTo='t'>
                      </logger>
                    </rules>
                </nlog>", null, f);

                // Assert
                if (autoLoadAssemblies)
                {
                    logFactory.Configuration = buildConfig(logFactory);
                    Assert.NotNull(logFactory.Configuration.FindTargetByName("t"));
                }
                else
                {
                    Assert.Throws<NLogConfigurationException>(() => buildConfig(logFactory));
                }
            }
            finally
            {
                ConfigurationItemFactory.Default = null;    // Restore global default
            }
        }

        [Fact]
        public void SetupExtensionsRegisterAssemblyNameTest()
        {
            try
            {
                // Arrange
                var logFactory = new LogFactory();

                // Act
                logFactory.Setup().SetupExtensions(ext => ext.AutoLoadAssemblies(false).RegisterAssembly("NLogAutoLoadExtension"));
                logFactory.Configuration = new XmlLoggingConfiguration(@"<nlog throwExceptions='true'>
                    <targets>
                        <target name='t' type='AutoLoadTarget' />
                    </targets>
                    <rules>
                      <logger name='*' writeTo='t'>
                      </logger>
                    </rules>
                </nlog>", null, logFactory);

                // Assert
                Assert.NotNull(logFactory.Configuration.FindTargetByName("t"));
            }
            finally
            {
                ConfigurationItemFactory.Default = null;    // Restore global default
            }
        }

        [Fact]
        public void SetupExtensionsRegisterAssemblyTest()
        {
            try
            {
                // Arrange
                var logFactory = new LogFactory();

                // Act
                logFactory.Setup().SetupExtensions(ext => ext.AutoLoadAssemblies(false).RegisterAssembly(typeof(MyExtensionNamespace.MyTarget).Assembly));
                logFactory.Configuration = new XmlLoggingConfiguration(@"<nlog throwExceptions='true'>
                    <targets>
                        <target name='t' type='MyTarget' />
                    </targets>
                    <rules>
                      <logger name='*' writeTo='t'>
                      </logger>
                    </rules>
                </nlog>", null, logFactory);

                // Assert
                Assert.NotNull(logFactory.Configuration.FindTargetByName<MyExtensionNamespace.MyTarget>("t"));
            }
            finally
            {
                ConfigurationItemFactory.Default = null;    // Restore global default
            }
        }

        [Fact]
        public void SetupExtensionsRegisterTargetTest()
        {
            try
            {
                // Arrange
                var logFactory = new LogFactory();

                // Act
                logFactory.Setup().SetupExtensions(ext => ext.AutoLoadAssemblies(false).RegisterTarget<MyExtensionNamespace.MyTarget>("MyTarget"));
                logFactory.Configuration = new XmlLoggingConfiguration(@"<nlog throwExceptions='true'>
                    <targets>
                        <target name='t' type='MyTarget' />
                    </targets>
                    <rules>
                      <logger name='*' writeTo='t'>
                      </logger>
                    </rules>
                </nlog>", null, logFactory);

                // Assert
                Assert.NotNull(logFactory.Configuration.FindTargetByName<MyExtensionNamespace.MyTarget>("t"));
            }
            finally
            {
                ConfigurationItemFactory.Default = null;    // Restore global default
            }
        }

        [Fact]
        public void SetupExtensionsRegisterTargetTypeTest()
        {
            try
            {
                // Arrange
                var logFactory = new LogFactory();

                // Act
                logFactory.Setup().SetupExtensions(ext => ext.AutoLoadAssemblies(false).RegisterTarget("MyTarget", typeof(MyExtensionNamespace.MyTarget)));
                logFactory.Configuration = new XmlLoggingConfiguration(@"<nlog throwExceptions='true'>
                    <targets>
                        <target name='t' type='MyTarget' />
                    </targets>
                    <rules>
                      <logger name='*' writeTo='t'>
                      </logger>
                    </rules>
                </nlog>", null, logFactory);

                // Assert
                Assert.NotNull(logFactory.Configuration.FindTargetByName<MyExtensionNamespace.MyTarget>("t"));
            }
            finally
            {
                ConfigurationItemFactory.Default = null;    // Restore global default
            }
        }

        [Fact]
        public void SetupExtensionsRegisterLayoutMethodTest()
        {
            try
            {
                // Arrange
                var logFactory = new LogFactory();

                // Act
                logFactory.Setup(b => b.SetupExtensions(ext => ext.RegisterLayoutRenderer("mylayout", (l) => "42")));
                logFactory.Configuration = new XmlLoggingConfiguration(@"<nlog throwExceptions='true'>
                    <targets>
                        <target name='debug' type='Debug' layout='${mylayout}' />
                    </targets>
                    <rules>
                      <logger name='*' writeTo='debug'>
                      </logger>
                    </rules>
                </nlog>", null, logFactory);
                logFactory.GetLogger("Hello").Info("World");

                // Assert
                Assert.Equal("42", logFactory.Configuration.FindTargetByName<DebugTarget>("debug").LastMessage);
            }
            finally
            {
                ConfigurationItemFactory.Default = null;    // Restore global default
            }
        }

        [Fact]
        public void SetupExtensionsRegisterLayoutRendererTest()
        {
            try
            {
                // Arrange
                var logFactory = new LogFactory();

                // Act
                logFactory.Setup().SetupExtensions(ext => ext.RegisterLayoutRenderer<MyExtensionNamespace.FooLayoutRenderer>("foo"));
                logFactory.Configuration = new XmlLoggingConfiguration(@"<nlog throwExceptions='true'>
                    <targets>
                        <target name='debug' type='Debug' layout='${foo}' />
                    </targets>
                    <rules>
                      <logger name='*' writeTo='debug'>
                      </logger>
                    </rules>
                </nlog>", null, logFactory);
                logFactory.GetLogger("Hello").Info("World");

                // Assert
                Assert.Equal("foo", logFactory.Configuration.FindTargetByName<DebugTarget>("debug").LastMessage);
            }
            finally
            {
                ConfigurationItemFactory.Default = null;    // Restore global default
            }
        }

        [Fact]
        public void SetupExtensionsRegisterLayoutRendererTypeTest()
        {
            try
            {
                // Arrange
                var logFactory = new LogFactory();

                // Act
                logFactory.Setup().SetupExtensions(ext => ext.RegisterLayoutRenderer("foo", typeof(MyExtensionNamespace.FooLayoutRenderer)));
                logFactory.Configuration = new XmlLoggingConfiguration(@"<nlog throwExceptions='true'>
                    <targets>
                        <target name='debug' type='Debug' layout='${foo}' />
                    </targets>
                    <rules>
                      <logger name='*' writeTo='debug'>
                      </logger>
                    </rules>
                </nlog>", null, logFactory);
                logFactory.GetLogger("Hello").Info("World");

                // Assert
                Assert.Equal("foo", logFactory.Configuration.FindTargetByName<DebugTarget>("debug").LastMessage);
            }
            finally
            {
                ConfigurationItemFactory.Default = null;    // Restore global default
            }
        }

        [Theory]
        [InlineData(nameof(LogLevel.Fatal))]
        [InlineData(nameof(LogLevel.Off))]
        public void SetupInternalLoggerSetLogLevelTest(string logLevelName)
        {
            try
            {
                // Arrange
                var logLevel = LogLevel.FromString(logLevelName);
                InternalLogger.Reset();
                var logFactory = new LogFactory();

                // Act
                logFactory.Setup().SetupInternalLogger(b => b.SetMinimumLogLevel(logLevel));

                // Assert
                Assert.Equal(logLevel, InternalLogger.LogLevel);
            }
            finally
            {
                InternalLogger.Reset();
            }
        }

        [Fact]
        public void SetupInternalLoggerLogToFileTest()
        {
            try
            {
                // Arrange
                var logFile = $"{nameof(SetupInternalLoggerLogToFileTest)}.txt";
                InternalLogger.Reset();
                var logFactory = new LogFactory();

                // Act
                logFactory.Setup().SetupInternalLogger(b => b.SetMinimumLogLevel(LogLevel.Fatal).LogToFile(logFile));

                // Assert
                Assert.Equal(logFile, InternalLogger.LogFile);
            }
            finally
            {
                InternalLogger.Reset();
            }
        }

        [Fact]
        public void SetupInternalLoggerLogToConsoleTest()
        {
            try
            {
                // Arrange
                InternalLogger.Reset();
                var logFactory = new LogFactory();

                // Act
                logFactory.Setup().SetupInternalLogger(b => b.SetMinimumLogLevel(LogLevel.Fatal).LogToConsole(true));

                // Assert
                Assert.True(InternalLogger.LogToConsole);
            }
            finally
            {
                InternalLogger.Reset();
            }
        }

        [Fact]
        public void SetupInternalLoggerLogToTraceTest()
        {
            try
            {
                // Arrange
                InternalLogger.Reset();
                var logFactory = new LogFactory();

                // Act
                logFactory.Setup().SetupInternalLogger(b => b.SetMinimumLogLevel(LogLevel.Fatal).LogToTrace(true));

                // Assert
                Assert.True(InternalLogger.LogToTrace);
            }
            finally
            {
                InternalLogger.Reset();
            }
        }

        [Fact]
        public void SetupInternalLoggerLogToWriterTest()
        {
            try
            {
                // Arrange
                InternalLogger.Reset();
                var logFactory = new LogFactory();

                // Act
                logFactory.Setup().SetupInternalLogger(b => b.SetMinimumLogLevel(LogLevel.Fatal).LogToWriter(Console.Out));

                // Assert
                Assert.Equal(Console.Out, InternalLogger.LogWriter);
            }
            finally
            {
                InternalLogger.Reset();
            }
        }

        [Fact]
        public void SetupExtensionsRegisterConditionMethodTest()
        {
            try
            {
                // Arrange
                var logFactory = new LogFactory();
                logFactory.Setup().SetupExtensions(s => s.RegisterConditionMethod("hasParameters", evt => evt.Parameters?.Length > 0));
                logFactory.Setup().SetupExtensions(s => s.RegisterConditionMethod("isProduction", () => false));
                logFactory.Setup().SetupExtensions(s => s.RegisterConditionMethod("isValid", typeof(Conditions.ConditionEvaluatorTests.MyConditionMethods).GetMethod(nameof(Conditions.ConditionEvaluatorTests.MyConditionMethods.IsValid))));

                // Act
                logFactory.Configuration = new XmlLoggingConfiguration(@"<nlog throwExceptions='true'>
                    <targets>
                        <target name='debug' type='Debug' layout='${message}' />
                    </targets>
                    <rules>
                      <logger name='*' writeTo='debug'>
			            <filters>
				            <when condition='hasParameters()' action='Ignore' />
                            <when condition='isProduction()' action='Ignore' />
                            <when condition='isValid()==false' action='Ignore' />
			            </filters>
                      </logger>
                    </rules>
                </nlog>", null, logFactory);
                logFactory.GetLogger("Hello").Info("World");
                logFactory.GetLogger("Hello").Info("{0}", "Earth");

                // Assert
                Assert.Equal("World", logFactory.Configuration.FindTargetByName<DebugTarget>("debug").LastMessage);
            }
            finally
            {
                ConfigurationItemFactory.Default = null;    // Restore global default
            }
        }

        [Fact]
        public void SetupBuilderLoadConfigurationTest()
        {
            // Arrange
            var logFactory = new LogFactory();
            var logConfig = new LoggingConfiguration(logFactory);

            // Act
            logFactory.Setup().LoadConfiguration(logConfig);

            // Assert
            Assert.Same(logConfig, logFactory.Configuration);
        }

        [Fact]
        public void SetupBuilderLoadConfigurationBuilderTest()
        {
            // Arrange
            var logFactory = new LogFactory();
            LoggingConfiguration logConfig = null;

            // Act
            logFactory.Setup().LoadConfiguration(b => logConfig = b.Configuration);

            // Assert
            Assert.Same(logConfig, logFactory.Configuration);
        }

        [Fact]
        public void SetupBuilderLoadConfigurationFromFileTest()
        {
            // Arrange
            var xmlFile = new System.IO.StringReader("<nlog autoshutdown='false'></nlog>");
            var appEnv = new Mocks.AppEnvironmentMock(f => true, f => System.Xml.XmlReader.Create(xmlFile));
            var configLoader = new LoggingConfigurationFileLoader(appEnv);
            var logFactory = new LogFactory(configLoader);

            // Act
            logFactory.Setup().LoadConfigurationFromFile();

            // Assert
            Assert.False(logFactory.AutoShutdown);
        }

        [Fact]
        public void SetupBuilderLoadNLogConfigFromFileNotExistsTest()
        {
            // Arrange
            var xmlFile = new System.IO.StringReader("<nlog autoshutdown='false'></nlog>");
            var appEnv = new Mocks.AppEnvironmentMock(f => false, f => System.Xml.XmlReader.Create(xmlFile));
            var configLoader = new LoggingConfigurationFileLoader(appEnv);
            var logFactory = new LogFactory(configLoader);

            // Act
            logFactory.Setup().LoadConfigurationFromFile("NLog.config", optional: true);

            // Assert
            Assert.Null(logFactory.Configuration);
        }

        [Fact]
        public void SetupBuilderLoadConfigurationFromFileOptionalFalseTest()
        {
            // Arrange
            var xmlFile = new System.IO.StringReader("<nlog autoshutdown='false'></nlog>");
            var appEnv = new Mocks.AppEnvironmentMock(f => false, f => System.Xml.XmlReader.Create(xmlFile));
            var configLoader = new LoggingConfigurationFileLoader(appEnv);
            var logFactory = new LogFactory(configLoader);

            // Act / Assert
            Assert.Throws<System.IO.FileNotFoundException>(() => logFactory.Setup().LoadConfigurationFromFile("NLog.config", optional: false));
        }

        [Fact]
        public void SetupBuilderLoadConfigurationFromXmlTest()
        {
            // Arrange
            var logFactory = new LogFactory();

            // Act
            logFactory.Setup().LoadConfigurationFromXml("<nlog autoshutdown='false'></nlog>");

            // Assert
            Assert.False(logFactory.AutoShutdown);
        }

        [Fact]
        public void SetupBuilderLoadConfigurationFromXmlPatchTest()
        {
            // Arrange
            var xmlFile = new System.IO.StringReader("<nlog autoshutdown='true'></nlog>");
            var appEnv = new Mocks.AppEnvironmentMock(f => true, f => System.Xml.XmlReader.Create(xmlFile));
            var configLoader = new LoggingConfigurationFileLoader(appEnv);
            var logFactory = new LogFactory(configLoader);

            // Act
            logFactory.Setup().
                LoadConfigurationFromXml("<nlog autoshutdown='false'></nlog>").
                LoadConfigurationFromFile().  // No effect, since config already loaded
                LoadConfiguration(b => { b.Configuration.Variables["Hello"] = "World"; });

            // Assert
            Assert.False(logFactory.AutoShutdown);
            Assert.Single(logFactory.Configuration.Variables);
        }
    }
}
