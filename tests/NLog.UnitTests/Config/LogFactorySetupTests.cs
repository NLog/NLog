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
using System.Linq;
using NLog.Common;
using NLog.Config;
using NLog.Layouts;
using NLog.LayoutRenderers;
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

        [Fact]
        public void SetupExtensionsSetTimeSourcAccurateUtcTest()
        {
            var currentTimeSource = NLog.Time.TimeSource.Current;
            try
            {
                // Arrange
                var logFactory = new LogFactory();

                // Act
                logFactory.Setup().SetupLogFactory(builder => builder.SetTimeSourcAccurateUtc());

                // Assert
                Assert.Same(NLog.Time.AccurateUtcTimeSource.Current, NLog.Time.TimeSource.Current);
            }
            finally
            {
                NLog.Time.TimeSource.Current = currentTimeSource;
            }
        }

        [Fact]
        public void SetupExtensionsSetTimeSourcAccurateLocalTest()
        {
            var currentTimeSource = NLog.Time.TimeSource.Current;
            try
            {
                // Arrange
                var logFactory = new LogFactory();

                // Act
                logFactory.Setup().SetupLogFactory(builder => builder.SetTimeSourcAccurateLocal());

                // Assert
                Assert.Same(NLog.Time.AccurateLocalTimeSource.Current, NLog.Time.TimeSource.Current);
            }
            finally
            {
                NLog.Time.TimeSource.Current = currentTimeSource;
            }
        }

        [Fact]
        public void SetupExtensionsSetGlobalContextPropertyTest()
        {
            // Arrange
            NLog.GlobalDiagnosticsContext.Clear();

            try
            {
                // Act
                var logFactory = new LogFactory();
                logFactory.Setup().SetupLogFactory(builder => builder.SetGlobalContextProperty(nameof(SetupExtensionsSetGlobalContextPropertyTest), "Yes"));

                // Assert
                Assert.Equal("Yes", NLog.GlobalDiagnosticsContext.Get(nameof(SetupExtensionsSetGlobalContextPropertyTest)));
            }
            finally
            {
                NLog.GlobalDiagnosticsContext.Clear();
            }
        }

        [Fact]
        public void SetupExtensionsSetAutoShutdownTest()
        {
            // Arrange
            var logFactory = new LogFactory();
            Assert.True(logFactory.AutoShutdown);

            // Act
            logFactory.Setup().SetupLogFactory(builder => builder.SetAutoShutdown(false));

            // Assert
            Assert.False(logFactory.AutoShutdown);
        }

        [Fact]
        public void SetupExtensionsSetDefaultCultureInfoTest()
        {
            // Arrange
            var logFactory = new LogFactory();
            Assert.Null(logFactory.DefaultCultureInfo);

            // Act
            logFactory.Setup().SetupLogFactory(builder => builder.SetDefaultCultureInfo(System.Globalization.CultureInfo.InvariantCulture));
            logFactory.Setup().LoadConfigurationFromXml("<nlog></nlog>");

            // Assert
            Assert.Same(System.Globalization.CultureInfo.InvariantCulture, logFactory.DefaultCultureInfo);
            Assert.Same(System.Globalization.CultureInfo.InvariantCulture, logFactory.Configuration.DefaultCultureInfo);
        }

        [Fact]
        public void SetupExtensionsSetGlobalThresholdTest()
        {
            // Arrange
            var logFactory = new LogFactory();
            Assert.Equal(LogLevel.Trace, logFactory.GlobalThreshold);

            // Act
            logFactory.Setup().SetupLogFactory(builder => builder.SetGlobalThreshold(LogLevel.Error));

            // Assert
            Assert.Equal(LogLevel.Error, logFactory.GlobalThreshold);
        }

        [Fact]
        public void SetupExtensionsSetThrowConfigExceptionsTest()
        {
            // Arrange
            var logFactory = new LogFactory();
            Assert.Equal(default(bool?), logFactory.ThrowConfigExceptions);

            // Act
            logFactory.Setup().SetupLogFactory(builder => builder.SetThrowConfigExceptions(true));

            // Assert
            Assert.True(logFactory.ThrowConfigExceptions);
        }

        [Fact]
        public void SetupExtensionsAutoLoadExtensionsTest()
        {
            // Arrange
            var logFactory = new LogFactory();

            // Act
            logFactory.Setup().SetupExtensions(ext => ext.AutoLoadExtensions());
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
            logFactory.Configuration = buildConfig(logFactory);
            Assert.NotNull(logFactory.Configuration.FindTargetByName("t"));
        }

        [Fact]
        public void SetupExtensionsRegisterAssemblyNameTest()
        {
            // Arrange
            var logFactory = new LogFactory();

            // Act
            logFactory.Setup().SetupExtensions(ext => ext.RegisterAssembly("NLogAutoLoadExtension"));
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

        [Fact]
        public void SetupExtensionsRegisterAssemblyTest()
        {
            // Arrange
            var logFactory = new LogFactory();

            // Act
            logFactory.Setup().SetupExtensions(ext => ext.RegisterAssembly(typeof(MyExtensionNamespace.MyTarget).Assembly));
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

        [Fact]
        public void SetupExtensionsRegisterTargetTest()
        {
            // Arrange
            var logFactory = new LogFactory();

            // Act
            logFactory.Setup().SetupExtensions(ext => ext.RegisterTarget<MyExtensionNamespace.MyTarget>("MyTarget"));
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

        [Fact]
        public void SetupExtensionsRegisterTargetTypeTest()
        {
            // Arrange
            var logFactory = new LogFactory();

            // Act
            logFactory.Setup().SetupExtensions(ext => ext.RegisterTarget<MyExtensionNamespace.MyTarget>());
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

        [Fact]
        public void SetupExtensionsRegisterLayoutTest()
        {
            // Arrange
            var logFactory = new LogFactory();

            // Act
            logFactory.Setup().SetupExtensions(ext => ext.RegisterLayout<MyExtensionNamespace.FooLayout>("FooLayout"));
            logFactory.Configuration = new XmlLoggingConfiguration(@"<nlog throwExceptions='true'>
                <targets>
                    <target name='debug' type='Debug'>
                        <layout type='foolayout' />
                    </target>
                </targets>
                <rules>
                    <logger name='*' writeTo='debug'>
                    </logger>
                </rules>
            </nlog>", null, logFactory);
            logFactory.GetLogger("Hello").Info("World");

            // Assert
            Assert.Equal("FooFoo0", logFactory.Configuration.FindTargetByName<DebugTarget>("debug").LastMessage);
        }

        [Fact]
        public void SetupExtensionsRegisterLayoutMethodTest()
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

        [Fact]
        public void SetupExtensionsRegisterLayoutMethodFluentTest()
        {
            // Arrange
            var logFactory = new LogFactory();

            // Act
            logFactory.Setup()
                .SetupExtensions(ext => ext.RegisterLayoutRenderer("mylayout", (l) => "42"))
                .LoadConfiguration(builder =>
                {
                    builder.ForLogger().WriteTo(new DebugTarget() { Layout = "${myLayout}" });
                });
            logFactory.GetLogger("Hello").Info("World");

            // Assert
            Assert.Equal("42", logFactory.Configuration.FindTargetByName<DebugTarget>("debug").LastMessage);
        }

        [Fact]
        public void SetupExtensionsRegisterLayoutMethodThreadUnsafeTest()
        {
            // Arrange
            var logFactory = new LogFactory();

            // Act
            logFactory.Setup(b => b.SetupExtensions(ext => ext.RegisterLayoutRenderer("mylayout", (l) => "42", LayoutRenderOptions.None)));
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

            ConfigurationItemFactory.Default.LayoutRendererFactory.TryCreateInstance("mylayout", out var layoutRenderer);
            var layout = new SimpleLayout(new LayoutRenderer[] { layoutRenderer }, "mylayout", ConfigurationItemFactory.Default);
            layout.Render(LogEventInfo.CreateNullEvent());

            // Assert
            Assert.Equal("42", logFactory.Configuration.FindTargetByName<DebugTarget>("debug").LastMessage);
            Assert.False(layout.ThreadAgnostic);
        }

        [Fact]
        public void SetupExtensionsRegisterLayoutMethodThreadAgnosticTest()
        {
            // Arrange
            var logFactory = new LogFactory();

            // Act
            logFactory.Setup(b => b.SetupExtensions(ext => ext.RegisterLayoutRenderer("mylayout", (l) => "42", LayoutRenderOptions.ThreadAgnostic)));
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

            ConfigurationItemFactory.Default.LayoutRendererFactory.TryCreateInstance("mylayout", out var layoutRenderer);
            var layout = new SimpleLayout(new LayoutRenderer[] { layoutRenderer }, "mylayout", ConfigurationItemFactory.Default);
            layout.Render(LogEventInfo.CreateNullEvent());

            // Assert
            Assert.Equal("42", logFactory.Configuration.FindTargetByName<DebugTarget>("debug").LastMessage);
            Assert.True(layout.ThreadAgnostic);
        }

        [Fact]
        public void SetupExtensionsRegisterLayoutRendererTest()
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

        [Fact]
        public void SetupExtensionsRegisterLayoutRendererTypeTest()
        {
            // Arrange
            var logFactory = new LogFactory();

            // Act
            logFactory.Setup().SetupExtensions(ext => ext.RegisterLayoutRenderer<MyExtensionNamespace.FooLayoutRenderer>());
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
                logFactory.Setup().SetupInternalLogger(b => b.LogToFile(logFile).SetMinimumLogLevel(LogLevel.Fatal));

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
#pragma warning disable CS0618 // Type or member is obsolete
                Assert.True(InternalLogger.LogToTrace);
#pragma warning restore CS0618 // Type or member is obsolete
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
        public void SetupInternalLoggerSetupFromEnvironmentVariablesTest()
        {
            try
            {
                // Arrange
                InternalLogger.Reset();
                var logFactory = new LogFactory();
                InternalLogger.IncludeTimestamp = false;

                // Act
                logFactory.Setup().SetupInternalLogger(b => b.SetupFromEnvironmentVariables().SetMinimumLogLevel(LogLevel.Fatal));

                // Assert
                Assert.True(InternalLogger.IncludeTimestamp);
                Assert.Equal(LogLevel.Fatal, InternalLogger.LogLevel);
            }
            finally
            {
                InternalLogger.Reset();
            }
        }

        [Fact]
        public void SetupExtensionsRegisterConditionMethodTest()
        {
            // Arrange
            var logFactory = new LogFactory();
            logFactory.Setup().SetupExtensions(s => s.RegisterConditionMethod("hasParameters", evt => evt.Parameters?.Length > 0));
            logFactory.Setup().SetupExtensions(s => s.RegisterConditionMethod("isProduction", () => false));
#pragma warning disable CS0618 // Type or member is obsolete
            logFactory.Setup().SetupExtensions(s => s.RegisterConditionMethod("isValid", typeof(Conditions.ConditionEvaluatorTests.MyConditionMethods).GetMethod(nameof(Conditions.ConditionEvaluatorTests.MyConditionMethods.IsValid))));
#pragma warning restore CS0618 // Type or member is obsolete

            // Act
            logFactory.Configuration = new XmlLoggingConfiguration(@"<nlog throwExceptions='true'>
                <targets>
                    <target name='debug' type='Debug' layout='${message}' />
                </targets>
                <rules>
                    <logger name='*' writeTo='debug'>
			        <filters defaultAction='Neutral'>
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
        public void SetupBuilderLoadConfigurationFromFileMissingButRequiredTest_IntegrationTest()
        {
            // Arrange
            var logFactory = new LogFactory();

            // Act
            Action act = () => logFactory.Setup().LoadConfigurationFromFile(optional: false);

            // Assert
            var ex = Assert.Throws<FileNotFoundException>(act);
            Assert.Contains("NLog.dll.nlog", ex.Message);
            Assert.Contains(Directory.GetCurrentDirectory(), ex.Message);
        }

        [Fact]
        public void SetupBuilderLoadConfigurationFromFileMissingTest()
        {
            // Arrange
            var appEnv = new Mocks.AppEnvironmentMock(f => false, f => null);
            var configLoader = new LoggingConfigurationFileLoader(appEnv);
            var logFactory = new LogFactory(configLoader);

            // Act
            logFactory.Setup().LoadConfigurationFromFile(optional: true);

            // Assert
            Assert.Null(logFactory.Configuration);
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

        [Fact]
        public void SetupBuilder_TimeSource()
        {
            // Arrange
            var originalTimeSource = NLog.Time.TimeSource.Current;

            try
            {
                // Act
                var logFactory = new LogFactory();
                logFactory.Setup().LoadConfiguration(builder => builder.SetTimeSource(new NLog.Time.AccurateUtcTimeSource()));

                // Assert
                Assert.Same(typeof(NLog.Time.AccurateUtcTimeSource), NLog.Time.TimeSource.Current.GetType());
            }
            finally
            {
                NLog.Time.TimeSource.Current = originalTimeSource;
            }
        }

        [Fact]
        public void SetupBuilder_GlobalDiagnosticContext()
        {
            // Arrange
            NLog.GlobalDiagnosticsContext.Clear();

            try
            {
                // Act
                var logFactory = new LogFactory();
                logFactory.Setup().LoadConfiguration(builder => builder.SetGlobalContextProperty(nameof(SetupBuilder_GlobalDiagnosticContext), "Yes"));

                // Assert
                Assert.Equal("Yes", NLog.GlobalDiagnosticsContext.Get(nameof(SetupBuilder_GlobalDiagnosticContext)));
            }
            finally
            {
                NLog.GlobalDiagnosticsContext.Clear();
            }
        }

        [Fact]
        public void SetupBuilder_FilterMinLevel()
        {
            var logFactory = new LogFactory();
            var logger = logFactory.Setup().LoadConfiguration(c => c.ForLogger().FilterMinLevel(LogLevel.Debug).WriteTo(new DebugTarget() { Layout = "${message}" })).GetCurrentClassLogger();
            var target = logFactory.Configuration.AllTargets.OfType<DebugTarget>().FirstOrDefault();
            Assert.Single(logFactory.Configuration.AllTargets);
            Assert.NotNull(target);

            logger.Info("Info Level");
            Assert.Equal("Info Level", target.LastMessage);

            logger.Fatal("Fatal Level");
            Assert.Equal("Fatal Level", target.LastMessage);

            logger.Trace("Trace Level");
            Assert.Equal("Fatal Level", target.LastMessage);
        }

        [Fact]
        public void SetupBuilder_FilterFinalMinLevel()
        {
            var logFactory = new LogFactory();
            var logger = logFactory.Setup().LoadConfiguration(c => {
                c.ForLogger("NoisyNameSpace.*").WriteToNil(LogLevel.Warn);
                c.ForLogger("NoisyNameSpace.GoodLogger").WriteToNil(LogLevel.Info);
                c.ForLogger(LogLevel.Debug).WriteTo(new DebugTarget() { Layout = "${message}" });
            }).GetCurrentClassLogger();
            var target = logFactory.Configuration.AllTargets.OfType<DebugTarget>().FirstOrDefault();
            Assert.Single(logFactory.Configuration.AllTargets);
            Assert.NotNull(target);

            logger.Info("Debug Level");
            Assert.Equal("Debug Level", target.LastMessage);

            logger.Info("Info Level");
            Assert.Equal("Info Level", target.LastMessage);

            logger.Fatal("Fatal Level");
            Assert.Equal("Fatal Level", target.LastMessage);

            logger.Trace("Trace Level");
            Assert.Equal("Fatal Level", target.LastMessage);

            var goodLogger = logFactory.GetLogger("NoisyNameSpace.GoodLogger");
            goodLogger.Info("Good Noise Info Level");
            Assert.Equal("Good Noise Info Level", target.LastMessage);

            goodLogger.Error("Good Noise Error Level");
            Assert.Equal("Good Noise Error Level", target.LastMessage);

            goodLogger.Fatal("Good Noise Fatal Level");
            Assert.Equal("Good Noise Fatal Level", target.LastMessage);

            goodLogger.Debug("Good Noise Debug Level");
            Assert.Equal("Good Noise Fatal Level", target.LastMessage);

            var noisyLogger = logFactory.GetLogger("NoisyNameSpace.BadLogger");
            noisyLogger.Error("Bad Noise Error Level");
            Assert.Equal("Bad Noise Error Level", target.LastMessage);

            noisyLogger.Fatal("Bad Noise Fatal Level");
            Assert.Equal("Bad Noise Fatal Level", target.LastMessage);

            noisyLogger.Info("Bad Noise Info Level");
            Assert.Equal("Bad Noise Fatal Level", target.LastMessage);

            noisyLogger.Debug("Bad Noise Debug Level");
            Assert.Equal("Bad Noise Fatal Level", target.LastMessage);
        }

        [Fact]
        public void SetupBuilder_FilterBlackHole()
        {
            var logFactory = new LogFactory();
            var logger = logFactory.Setup().LoadConfiguration(c =>
            {
                c.ForLogger().FilterMinLevel(LogLevel.Info).WriteTo(new DebugTarget() { Layout = "${message}" });
                c.ForLogger("*").TopRule().WriteToNil(LogLevel.Warn);
            }).GetCurrentClassLogger();
            var target = logFactory.Configuration.AllTargets.OfType<DebugTarget>().FirstOrDefault();
            Assert.Single(logFactory.Configuration.AllTargets);
            Assert.NotNull(target);

            logger.Fatal("Fatal Level");
            Assert.Equal("Fatal Level", target.LastMessage);

            logger.Info("Info Level");
            Assert.Equal("Fatal Level", target.LastMessage);
        }

        [Fact]
        public void SetupBuilder_FilterLevels()
        {
            var logFactory = new LogFactory();
            var logger = logFactory.Setup().LoadConfiguration(c => c.ForLogger().FilterLevels(LogLevel.Debug, LogLevel.Info).WriteTo(new DebugTarget() { Layout = "${message}" })).GetCurrentClassLogger();
            var target = logFactory.Configuration.AllTargets.OfType<DebugTarget>().FirstOrDefault();
            Assert.Single(logFactory.Configuration.AllTargets);
            Assert.NotNull(target);

            logger.Info("Info Level");
            Assert.Equal("Info Level", target.LastMessage);

            logger.Trace("Trace Level");
            Assert.Equal("Info Level", target.LastMessage);

            logger.Info("Debug Level");
            Assert.Equal("Debug Level", target.LastMessage);

            logger.Warn("Warn Level");
            Assert.Equal("Debug Level", target.LastMessage);
        }

        [Fact]
        public void SetupBuilder_FilterLevel()
        {
            var logFactory = new LogFactory();
            var logger = logFactory.Setup().LoadConfiguration(c => c.ForLogger().FilterLevel(LogLevel.Debug).WriteTo(new DebugTarget() { Layout = "${message}" })).GetCurrentClassLogger();
            var target = logFactory.Configuration.AllTargets.OfType<DebugTarget>().FirstOrDefault();
            Assert.Single(logFactory.Configuration.AllTargets);
            Assert.NotNull(target);

            logger.Debug("Debug Level");
            Assert.Equal("Debug Level", target.LastMessage);

            logger.Trace("Trace Level");
            Assert.Equal("Debug Level", target.LastMessage);

            logger.Trace("Error Level");
            Assert.Equal("Debug Level", target.LastMessage);
        }

        [Fact]
        void SetupBuilder_FilterDynamicMethod()
        {
            var logFactory = new LogFactory();
            var logger = logFactory.Setup().LoadConfiguration(c => c.ForLogger().FilterMinLevel(LogLevel.Debug).FilterDynamic(evt => evt.Properties.ContainsKey("Enabled") ? NLog.Filters.FilterResult.Log : NLog.Filters.FilterResult.Ignore).WriteTo(new DebugTarget() { Layout = "${message}" })).GetCurrentClassLogger();
            var target = logFactory.Configuration.AllTargets.OfType<DebugTarget>().FirstOrDefault();
            Assert.Single(logFactory.Configuration.AllTargets);
            Assert.NotNull(target);

            logger.Debug("Debug Level {Enabled:l}", "Yes");
            Assert.Equal("Debug Level Yes", target.LastMessage);

            logger.Info("Info Level No");
            Assert.Equal("Debug Level Yes", target.LastMessage);

            logger.Info("Info Level {Enabled:l}", "Yes");
            Assert.Equal("Info Level Yes", target.LastMessage);
        }

        [Fact]
        public void SetupBuilder_FilterDynamicLog()
        {
            var logFactory = new LogFactory();
            var logger = logFactory.Setup().LoadConfiguration(c => c.ForLogger().FilterDynamicLog(evt => evt.Properties.ContainsKey("Enabled")).WriteTo(new DebugTarget() { Layout = "${message}" })).GetCurrentClassLogger();
            var target = logFactory.Configuration.AllTargets.OfType<DebugTarget>().FirstOrDefault();
            Assert.Single(logFactory.Configuration.AllTargets);
            Assert.NotNull(target);

            logger.Debug("Debug Level {Enabled:l}", "Yes");
            Assert.Equal("Debug Level Yes", target.LastMessage);

            logger.Info("Info Level No");
            Assert.Equal("Debug Level Yes", target.LastMessage);

            logger.Info("Info Level {Enabled:l}", "Yes");
            Assert.Equal("Info Level Yes", target.LastMessage);
        }

        [Fact]
        public void SetupBuilder_FilterDynamicIgnore()
        {
            var logFactory = new LogFactory();
            var logger = logFactory.Setup().LoadConfiguration(c =>
            {
                c.ForLogger().FilterDynamicIgnore(evt => !evt.Properties.ContainsKey("Enabled")).WriteToNil();
                c.ForLogger().WriteTo(new DebugTarget() { Layout = "${message}" });
            }).GetCurrentClassLogger();
            var target = logFactory.Configuration.AllTargets.OfType<DebugTarget>().FirstOrDefault();
            Assert.NotNull(target);

            logger.Debug("Debug Level {Enabled:l}", "Yes");
            Assert.Equal("Debug Level Yes", target.LastMessage);

            logger.Info("Info Level No");
            Assert.Equal("Debug Level Yes", target.LastMessage);

            logger.Info("Info Level {Enabled:l}", "Yes");
            Assert.Equal("Info Level Yes", target.LastMessage);
        }

        [Fact]
        public void SetupBuilder_MultipleTargets()
        {
            string lastMessage = null;

            var logFactory = new LogFactory();
            var logger = logFactory.Setup().LoadConfiguration(c =>
            {
                c.ForLogger()
                    .WriteTo(new DebugTarget() { Layout = "${message}" })
                    .WriteToMethodCall((evt, args) => lastMessage = evt.FormattedMessage);
            }).GetCurrentClassLogger();

            var target = logFactory.Configuration.AllTargets.OfType<DebugTarget>().FirstOrDefault();
            Assert.Equal(2, logFactory.Configuration.AllTargets.Count);
            Assert.NotNull(target);

            logger.Debug("Debug Level");
            Assert.Equal("Debug Level", target.LastMessage);
            Assert.Equal("Debug Level", lastMessage);
        }

        [Fact]
        public void SetupBuilder_MultipleTargets2()
        {
            string lastMessage = null;

            var logFactory = new LogFactory();
            var logger = logFactory.Setup().LoadConfiguration(c =>
            {
                c.ForLogger().WriteTo(new DebugTarget() { Layout = "${message}" });
                c.ForLogger().WriteToMethodCall((evt, args) => lastMessage = evt.FormattedMessage);
            }).GetCurrentClassLogger();

            var target = logFactory.Configuration.AllTargets.OfType<DebugTarget>().FirstOrDefault();
            Assert.Equal(2, logFactory.Configuration.AllTargets.Count);
            Assert.NotNull(target);

            logger.Debug("Debug Level");
            Assert.Equal("Debug Level", target.LastMessage);
            Assert.Equal("Debug Level", lastMessage);
        }

        [Fact]
        public void SetupBuilder_MultipleTargets3()
        {
            var logFactory = new LogFactory();
            var logger = logFactory.Setup().LoadConfiguration(c =>
            {
                var debugTarget1 = new DebugTarget("debug1") { Layout = "${message}" };
                var debugTarget2 = new DebugTarget("debug2") { Layout = "${message}" };
                c.ForLogger().WriteTo(debugTarget1, debugTarget2).WithAsync();
            }).GetCurrentClassLogger();

            var target1 = logFactory.Configuration.FindTargetByName<DebugTarget>("debug1");
            var target2 = logFactory.Configuration.FindTargetByName<DebugTarget>("debug2");
            Assert.Equal(4, logFactory.Configuration.AllTargets.Count);
            Assert.NotNull(target1);
            Assert.NotNull(target2);

            logger.Debug("Debug Level");
            logFactory.Flush();
            Assert.Equal("Debug Level", target1.LastMessage);
            Assert.Equal("Debug Level", target2.LastMessage);
        }

        [Fact]
        public void SetupBuilder_ForTarget_WithName()
        {
            string lastMessage = null;
            var logFactory = new LogFactory();
            var logger = logFactory.Setup().LoadConfiguration(c =>
            {
                var methodTarget = c.ForTarget("mymethod").WriteToMethodCall((evt, args) => lastMessage = evt.FormattedMessage).WithAsync().FirstTarget<MethodCallTarget>();

                c.ForLogger().FilterLevel(LogLevel.Fatal).WriteTo(methodTarget);
                c.ForLogger("ErrorLogger").FilterLevel(LogLevel.Error).WriteTo(methodTarget);
            }).GetCurrentClassLogger();

            var target = logFactory.Configuration.FindTargetByName<MethodCallTarget>("mymethod");
            Assert.Equal(2, logFactory.Configuration.AllTargets.Count);
            Assert.NotNull(target);

            Assert.True(logger.IsFatalEnabled);
            Assert.False(logger.IsErrorEnabled);
            var errorLogger = logFactory.GetLogger("ErrorLogger");
            Assert.True(errorLogger.IsFatalEnabled);
            Assert.True(errorLogger.IsErrorEnabled);

            logger.Fatal("Debug Level");
            logFactory.Flush();
            Assert.Equal("Debug Level", lastMessage);
        }

        [Fact]
        public void SetupBuilder_ForTarget_Group()
        {
            string lastMessage = null;
            var logFactory = new LogFactory();
            var logger = logFactory.Setup().LoadConfiguration(c =>
            {
                var debugTargets = c.ForTarget().WriteToMethodCall((evt, args) => lastMessage = evt.FormattedMessage).WriteTo(new DebugTarget() { Layout = "${message}" }).WithAsync();
                c.ForLogger().WriteTo(debugTargets);
            }).GetCurrentClassLogger();

            var debugTarget = logFactory.Configuration.AllTargets.OfType<DebugTarget>().FirstOrDefault();
            var methodTarget = logFactory.Configuration.AllTargets.OfType<MethodCallTarget>().FirstOrDefault();
            Assert.Equal(4, logFactory.Configuration.AllTargets.Count);
            Assert.NotNull(debugTarget);
            Assert.NotNull(methodTarget);

            logger.Debug("Debug Level");
            logFactory.Flush();

            Assert.Equal("Debug Level", debugTarget.LastMessage);
            Assert.Equal("Debug Level", lastMessage);
        }

        [Fact]
        public void SetupBuilder_ForTargetWithName_ShouldFailForGroup()
        {
            var logFactory = new LogFactory();
            Assert.Throws<ArgumentException>(() =>
                logFactory.Setup().LoadConfiguration(c => c.ForTarget("OnlyOne").WriteTo(new DebugTarget() { Layout = "${message}" }).WriteTo(new DebugTarget() { Layout = "${message}" }))
            );
        }

        [Fact]
        public void SetupBuilder_WithWrapperFirst_ShouldFail()
        {
            var logFactory = new LogFactory();
            Assert.Throws<ArgumentException>(() =>
                logFactory.Setup().LoadConfiguration(c => c.ForLogger().WithAsync().WriteTo(new DebugTarget() { Layout = "${message}" }))
            );
        }

        [Fact]
        public void SetupBuilder_WriteToWithBuffering()
        {
            var logFactory = new LogFactory();
            var logger = logFactory.Setup().LoadConfiguration(c => c.ForLogger().WriteTo(new DebugTarget() { Layout = "${message}" }).WithBuffering()).GetCurrentClassLogger();
            var target = logFactory.Configuration.AllTargets.OfType<DebugTarget>().FirstOrDefault();
            Assert.Equal(2, logFactory.Configuration.AllTargets.Count);

            Assert.NotNull(target);
            logger.Debug("Debug Level");

            Assert.Equal("", target.LastMessage ?? string.Empty);

            logFactory.Flush();
            Assert.Equal("Debug Level", target.LastMessage);
        }

        [Fact]
        public void SetupBuilder_WriteToWithAutoFlush()
        {
            var logFactory = new LogFactory();
            var logger = logFactory.Setup().LoadConfiguration(c => c.ForLogger().WriteTo(new DebugTarget() { Layout = "${message}" }).WithBuffering().WithAutoFlush(evt => evt.Level == LogLevel.Error)).GetCurrentClassLogger();
            var target = logFactory.Configuration.AllTargets.OfType<DebugTarget>().FirstOrDefault();
            Assert.Equal(3, logFactory.Configuration.AllTargets.Count);

            Assert.NotNull(target);
            logger.Debug("Debug Level");

            Assert.Equal("", target.LastMessage ?? string.Empty);

            logFactory.Flush();
            Assert.Equal("Debug Level", target.LastMessage);

            logger.Error("Error Level");
            Assert.Equal("Error Level", target.LastMessage);
        }

        [Fact]
        public void SetupBuilder_WriteToWithAsync()
        {
            var logFactory = new LogFactory();
            var logger = logFactory.Setup().LoadConfiguration(c => c.ForLogger().FilterLevel(LogLevel.Debug).WriteTo(new DebugTarget() { Layout = "${message}" }).WithAsync()).GetCurrentClassLogger();
            var target = logFactory.Configuration.AllTargets.OfType<DebugTarget>().FirstOrDefault();
            Assert.NotNull(logFactory.Configuration);
            Assert.Equal(2, logFactory.Configuration.AllTargets.Count);

            Assert.NotNull(target);
            logger.Debug("Debug Level");

            logFactory.Flush();
            Assert.Equal("Debug Level", target.LastMessage);
        }

        [Fact]
        public void SetupBuilder_WriteToWithFallback()
        {
            bool exceptionWasThrown = false;

            var logFactory = new LogFactory();
            var logger = logFactory.Setup().LoadConfiguration(c =>
            {
                c.ForLogger()
                    .WriteToMethodCall((evt, args) => { exceptionWasThrown = true; throw new Exception("Abort"); })
                            .WithFallback(new DebugTarget() { Layout = "${message}" });
            }).GetCurrentClassLogger();
            var target = logFactory.Configuration.AllTargets.OfType<DebugTarget>().FirstOrDefault();
            Assert.NotNull(logFactory.Configuration);
            Assert.Equal(3, logFactory.Configuration.AllTargets.Count);

            Assert.NotNull(target);

            using (new NLogTestBase.NoThrowNLogExceptions())
            {
                logger.Debug("Debug Level");
                Assert.Equal("Debug Level", target.LastMessage);
                Assert.True(exceptionWasThrown);
            }
        }

        [Fact]
        public void SetupBuilder_WriteToWithRetry()
        {
            int methodCalls = 0;

            var logFactory = new LogFactory();
            var logger = logFactory.Setup().LoadConfiguration(c => c.ForLogger().WriteToMethodCall((evt, args) => { if (methodCalls++ > 0) return; throw new Exception("Abort"); }).WithRetry()).GetCurrentClassLogger();
            Assert.NotNull(logFactory.Configuration);
            Assert.Equal(2, logFactory.Configuration.AllTargets.Count);

            using (new NLogTestBase.NoThrowNLogExceptions())
            {
                logger.Debug("Debug Level");
                Assert.Equal(2, methodCalls);
            }
        }

        [Fact]
        public void SetupBuilder_LoadConfigEmbeddedResource()
        {
            var logFactory = new LogFactory();
            var config = logFactory.Setup().LoadConfigurationFromAssemblyResource(typeof(LogFactorySetupTests).Assembly, "NLog.UnitTests.config").LogFactory.Configuration;

            Assert.NotNull(logFactory.Configuration);
            Assert.NotEmpty(logFactory.Configuration.Variables);
        }
    }
}
