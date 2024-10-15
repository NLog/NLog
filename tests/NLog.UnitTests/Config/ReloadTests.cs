//
// Copyright (c) 2004-2024 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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

namespace NLog.UnitTests.Config
{
    using System;
    using System.IO;
    using System.Linq;
    using NLog.Config;
    using Xunit;

    public class ReloadTests : NLogTestBase
    {
        public ReloadTests()
        {
            if (LogManager.LogFactory != null)
            {
                LogManager.LogFactory.ResetLoggerCache();
            }
        }

        private static void SetLogManagerConfiguration(LogFactory logFactory, bool useExplicitFileLoading, string configFilePath)
        {
            if (useExplicitFileLoading)
            {
                logFactory.Configuration = new XmlLoggingConfiguration(configFilePath);
            }
            else
            {
                logFactory.Setup().LoadConfigurationFromFile(new string[] { configFilePath });
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void TestAutoReloadOnFileChange(bool useExplicitFileLoading)
        {
            string config1 = @"<nlog autoReload='true'>
                    <targets><target name='debug' type='Debug' layout='${message}' /></targets>
                    <rules><logger name='*' minlevel='Debug' writeTo='debug' /></rules>
                </nlog>";
            string config2 = @"<nlog autoReload='true'>
                    <targets><target name='debug' type='Debug' layout='[${message}]' /></targets>
                    <rules><logger name='*' minlevel='Debug' writeTo='debug' /></rules>
                </nlog>";
            string badConfig = @"<nlog autoReload='true'>
                    <targets><target name='debug' type='Debug' layout='(${message})' /></targets>
                    <rules><logger name='*' minlevel='Debug' writeTo='debug' /></rules>";

            string tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

            var logFactory = new LogFactory();

            try
            {
                Directory.CreateDirectory(tempDir);

                string configFilePath = Path.Combine(tempDir, "reload.nlog");
                WriteConfigFile(configFilePath, config1);

                SetLogManagerConfiguration(logFactory, useExplicitFileLoading, configFilePath);

                var logger = logFactory.GetCurrentClassLogger();
                logger.Debug("aaa");
                AssertDebugLastMessage("debug", "aaa", logFactory);
                Assert.True(((XmlLoggingConfiguration)logFactory.Configuration).AutoReload);
                Assert.Single(logFactory.Configuration.FileNamesToWatch);
                Assert.Equal(configFilePath, logFactory.Configuration.FileNamesToWatch.FirstOrDefault());

                WriteConfigFileAndReload(logFactory, configFilePath, badConfig);

                logger.Debug("bbb");
                // Assert that config1 is still loaded.
                AssertDebugLastMessage("debug", "bbb", logFactory);
                Assert.True(((XmlLoggingConfiguration)logFactory.Configuration).AutoReload);
                Assert.Single(logFactory.Configuration.FileNamesToWatch);
                Assert.Equal(configFilePath, logFactory.Configuration.FileNamesToWatch.FirstOrDefault());

                WriteConfigFileAndReload(logFactory, configFilePath, config2);

                logger.Debug("ccc");
                // Assert that config2 is loaded.
                AssertDebugLastMessage("debug", "[ccc]", logFactory);
                Assert.True(((XmlLoggingConfiguration)logFactory.Configuration).AutoReload);
                Assert.Single(logFactory.Configuration.FileNamesToWatch);
                Assert.Equal(configFilePath, logFactory.Configuration.FileNamesToWatch.FirstOrDefault());
            }
            finally
            {
                logFactory.Shutdown();

                if (Directory.Exists(tempDir))
                    Directory.Delete(tempDir, true);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]

        public void TestAutoReloadOnFileMove(bool useExplicitFileLoading)
        {
            string config1 = @"<nlog autoReload='true'>
                    <targets><target name='debug' type='Debug' layout='${message}' /></targets>
                    <rules><logger name='*' minlevel='Debug' writeTo='debug' /></rules>
                </nlog>";
            string config2 = @"<nlog autoReload='true'>
                    <targets><target name='debug' type='Debug' layout='[${message}]' /></targets>
                    <rules><logger name='*' minlevel='Debug' writeTo='debug' /></rules>
                </nlog>";

            string tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

            var logFactory = new LogFactory();

            try
            {
                Directory.CreateDirectory(tempDir);

                string configFilePath = Path.Combine(tempDir, "reload.nlog");
                WriteConfigFile(configFilePath, config1);
                string otherFilePath = Path.Combine(tempDir, "other.nlog");

                SetLogManagerConfiguration(logFactory, useExplicitFileLoading, configFilePath);

                var logger = logFactory.GetCurrentClassLogger();
                logger.Debug("aaa");
                AssertDebugLastMessage("debug", "aaa", logFactory);
                Assert.True(((XmlLoggingConfiguration)logFactory.Configuration).AutoReload);
                Assert.Single(logFactory.Configuration.FileNamesToWatch);
                Assert.Equal(configFilePath, logFactory.Configuration.FileNamesToWatch.FirstOrDefault());

                File.Move(configFilePath, otherFilePath);
                logFactory.Setup().ReloadConfiguration();
                logger.Debug("bbb");
                // Assert that config1 is still loaded.
                AssertDebugLastMessage("debug", "bbb", logFactory);
                Assert.True(((XmlLoggingConfiguration)logFactory.Configuration).AutoReload);
                Assert.Single(logFactory.Configuration.FileNamesToWatch);
                Assert.Equal(configFilePath, logFactory.Configuration.FileNamesToWatch.FirstOrDefault());

                WriteConfigFile(otherFilePath, config2);
                File.Move(otherFilePath, configFilePath);
                logFactory.Setup().ReloadConfiguration();

                logger.Debug("ccc");
                // Assert that config2 is loaded.
                AssertDebugLastMessage("debug", "[ccc]", logFactory);
                Assert.True(((XmlLoggingConfiguration)logFactory.Configuration).AutoReload);
                Assert.Single(logFactory.Configuration.FileNamesToWatch);
                Assert.Equal(configFilePath, logFactory.Configuration.FileNamesToWatch.FirstOrDefault());
            }
            finally
            {
                logFactory.Shutdown();

                if (Directory.Exists(tempDir))
                    Directory.Delete(tempDir, true);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void TestAutoReloadOnFileCopy(bool useExplicitFileLoading)
        {
            string config1 = @"<nlog autoReload='true'>
                    <targets><target name='debug' type='Debug' layout='${message}' /></targets>
                    <rules><logger name='*' minlevel='Debug' writeTo='debug' /></rules>
                </nlog>";
            string config2 = @"<nlog autoReload='true'>
                    <targets><target name='debug' type='Debug' layout='[${message}]' /></targets>
                    <rules><logger name='*' minlevel='Debug' writeTo='debug' /></rules>
                </nlog>";

            string tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

            var logFactory = new LogFactory();

            try
            {
                Directory.CreateDirectory(tempPath);

                string configFilePath = Path.Combine(tempPath, "reload.nlog");
                WriteConfigFile(configFilePath, config1);
                string otherFilePath = Path.Combine(tempPath, "other.nlog");

                SetLogManagerConfiguration(logFactory, useExplicitFileLoading, configFilePath);

                var logger = logFactory.GetCurrentClassLogger();
                logger.Debug("aaa");
                AssertDebugLastMessage("debug", "aaa", logFactory);
                Assert.True(((XmlLoggingConfiguration)logFactory.Configuration).AutoReload);
                Assert.Single(logFactory.Configuration.FileNamesToWatch);
                Assert.Equal(configFilePath, logFactory.Configuration.FileNamesToWatch.FirstOrDefault());

                File.Delete(configFilePath);
                logFactory.Setup().ReloadConfiguration();

                logger.Debug("bbb");
                // Assert that config1 is still loaded.
                AssertDebugLastMessage("debug", "bbb", logFactory);
                Assert.True(((XmlLoggingConfiguration)logFactory.Configuration).AutoReload);
                Assert.Single(logFactory.Configuration.FileNamesToWatch);
                Assert.Equal(configFilePath, logFactory.Configuration.FileNamesToWatch.FirstOrDefault());

                WriteConfigFile(otherFilePath, config2);
                File.Copy(otherFilePath, configFilePath);
                File.Delete(otherFilePath);
                logFactory.Setup().ReloadConfiguration();

                logger.Debug("ccc");
                // Assert that config2 is loaded.
                AssertDebugLastMessage("debug", "[ccc]", logFactory);
                Assert.True(((XmlLoggingConfiguration)logFactory.Configuration).AutoReload);
                Assert.Single(logFactory.Configuration.FileNamesToWatch);
                Assert.Equal(configFilePath, logFactory.Configuration.FileNamesToWatch.FirstOrDefault());
            }
            finally
            {
                logFactory.Shutdown();

                if (Directory.Exists(tempPath))
                    Directory.Delete(tempPath, true);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void TestIncludedConfigNoReload(bool useExplicitFileLoading)
        {
            string mainConfig1 = @"<nlog>
                  <include file='included.nlog' />
                  <rules><logger name='*' minlevel='Debug' writeTo='debug' /></rules>
                </nlog>";
            string mainConfig2 = @"<nlog>
                  <include file='included.nlog' />
                  <rules><logger name='*' minlevel='Info' writeTo='debug' /></rules>
                </nlog>";
            string includedConfig1 = @"<nlog>
                    <targets><target name='debug' type='Debug' layout='${message}' /></targets>
                </nlog>";
            string includedConfig2 = @"<nlog>
                    <targets><target name='debug' type='Debug' layout='[${message}]' /></targets>
                </nlog>";

            string tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

            var logFactory = new LogFactory();

            try
            {
                Directory.CreateDirectory(tempDir);

                string mainConfigFilePath = Path.Combine(tempDir, "main.nlog");
                WriteConfigFile(mainConfigFilePath, mainConfig1);

                string includedConfigFilePath = Path.Combine(tempDir, "included.nlog");
                WriteConfigFile(includedConfigFilePath, includedConfig1);

                SetLogManagerConfiguration(logFactory, useExplicitFileLoading, mainConfigFilePath);

                var logger = logFactory.GetCurrentClassLogger();
                logger.Debug("aaa");
                AssertDebugLastMessage("debug", "aaa", logFactory);
                Assert.False(((XmlLoggingConfiguration)logFactory.Configuration).AutoReload);
                Assert.Empty(logFactory.Configuration.FileNamesToWatch);

                WriteConfigFileAndReload(logFactory, mainConfigFilePath, mainConfig2);

                logger.Debug("bbb");
                // Assert that mainConfig2 has been loaded.
                AssertDebugLastMessage("debug", "", logFactory);
                Assert.False(((XmlLoggingConfiguration)logFactory.Configuration).AutoReload);
                Assert.Empty(logFactory.Configuration.FileNamesToWatch);
                logger.Info("bbb");
                AssertDebugLastMessage("debug", "bbb", logFactory);

                WriteConfigFile(mainConfigFilePath, mainConfig1);
                WriteConfigFileAndReload(logFactory, includedConfigFilePath, includedConfig2);

                logger.Debug("ccc");
                // Assert that includedConfig2 has been loaded.
                AssertDebugLastMessage("debug", "[ccc]", logFactory);
                Assert.False(((XmlLoggingConfiguration)logFactory.Configuration).AutoReload);
                Assert.Empty(logFactory.Configuration.FileNamesToWatch);
            }
            finally
            {
                logFactory.Shutdown();

                if (Directory.Exists(tempDir))
                    Directory.Delete(tempDir, true);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void TestIncludedConfigReload(bool useExplicitFileLoading)
        {
            string mainConfig1 = @"<nlog>
                  <include file='included.nlog' />
                  <rules><logger name='*' minlevel='Debug' writeTo='debug' /></rules>
                </nlog>";
            string includedConfig1 = @"<nlog autoReload='true'>
                    <targets><target name='debug' type='Debug' layout='${message}' /></targets>
                </nlog>";
            string includedConfig2 = @"<nlog autoReload='true'>
                    <targets><target name='debug' type='Debug' layout='[${message}]' /></targets>
                </nlog>";

            string tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

            var logFactory = new LogFactory();

            try
            {
                Directory.CreateDirectory(tempDir);

                string mainConfigFilePath = Path.Combine(tempDir, "main.nlog");
                WriteConfigFile(mainConfigFilePath, mainConfig1);

                string includedConfigFilePath = Path.Combine(tempDir, "included.nlog");
                WriteConfigFile(includedConfigFilePath, includedConfig1);

                SetLogManagerConfiguration(logFactory, useExplicitFileLoading, mainConfigFilePath);

                var logger = logFactory.GetCurrentClassLogger();
                logger.Debug("aaa");
                AssertDebugLastMessage("debug", "aaa", logFactory);
                Assert.True(((XmlLoggingConfiguration)logFactory.Configuration).AutoReload);
                Assert.Single(logFactory.Configuration.FileNamesToWatch);
                Assert.Equal(includedConfigFilePath, logFactory.Configuration.FileNamesToWatch.FirstOrDefault());

                WriteConfigFileAndReload(logFactory, includedConfigFilePath, includedConfig2);

                logger.Debug("ccc");
                // Assert that includedConfig2 is loaded.
                AssertDebugLastMessage("debug", "[ccc]", logFactory);
                Assert.True(((XmlLoggingConfiguration)logFactory.Configuration).AutoReload);
                Assert.Single(logFactory.Configuration.FileNamesToWatch);
                Assert.Equal(includedConfigFilePath, logFactory.Configuration.FileNamesToWatch.FirstOrDefault());
            }
            finally
            {
                logFactory.Shutdown();

                if (Directory.Exists(tempDir))
                    Directory.Delete(tempDir, true);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void TestMainConfigReload(bool useExplicitFileLoading)
        {
            string mainConfig1 = @"<nlog autoReload='true'>
                  <include file='included.nlog' />
                  <rules><logger name='*' minlevel='Debug' writeTo='debug' /></rules>
                </nlog>";
            string mainConfig2 = @"<nlog autoReload='true'>
                  <include file='included2.nlog' />
                  <rules><logger name='*' minlevel='Debug' writeTo='debug' /></rules>
                </nlog>";
            string included1Config = @"<nlog>
                    <targets><target name='debug' type='Debug' layout='${message}' /></targets>
                </nlog>";
            string included2Config1 = @"<nlog>
                    <targets><target name='debug' type='Debug' layout='[${message}]' /></targets>
                </nlog>";
            string included2Config2 = @"<nlog>
                    <targets><target name='debug' type='Debug' layout='(${message})' /></targets>
                </nlog>";

            string tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

            var logFactory = new LogFactory();

            try
            {
                Directory.CreateDirectory(tempDir);

                string mainConfigFilePath = Path.Combine(tempDir, "main.nlog");
                WriteConfigFile(mainConfigFilePath, mainConfig1);

                string included1ConfigFilePath = Path.Combine(tempDir, "included.nlog");
                WriteConfigFile(included1ConfigFilePath, included1Config);

                string included2ConfigFilePath = Path.Combine(tempDir, "included2.nlog");
                WriteConfigFile(included2ConfigFilePath, included2Config1);

                SetLogManagerConfiguration(logFactory, useExplicitFileLoading, mainConfigFilePath);

                var logger = logFactory.GetCurrentClassLogger();
                logger.Debug("aaa");
                AssertDebugLastMessage("debug", "aaa", logFactory);
                Assert.True(((XmlLoggingConfiguration)logFactory.Configuration).AutoReload);
                Assert.NotEmpty(logFactory.Configuration.FileNamesToWatch);
                Assert.Contains(mainConfigFilePath, logFactory.Configuration.FileNamesToWatch);
                Assert.Contains(included1ConfigFilePath, logFactory.Configuration.FileNamesToWatch);

                WriteConfigFileAndReload(logFactory, mainConfigFilePath, mainConfig2);

                logger.Debug("bbb");
                // Assert that mainConfig2 is loaded (which refers to included2.nlog).
                AssertDebugLastMessage("debug", "[bbb]", logFactory);
                Assert.True(((XmlLoggingConfiguration)logFactory.Configuration).AutoReload);
                Assert.NotEmpty(logFactory.Configuration.FileNamesToWatch);
                Assert.Contains(mainConfigFilePath, logFactory.Configuration.FileNamesToWatch);
                Assert.Contains(included2ConfigFilePath, logFactory.Configuration.FileNamesToWatch);

                WriteConfigFileAndReload(logFactory, included2ConfigFilePath, included2Config2);

                logger.Debug("ccc");
                // Assert that included2Config2 is loaded.
                AssertDebugLastMessage("debug", "(ccc)", logFactory);
                Assert.True(((XmlLoggingConfiguration)logFactory.Configuration).AutoReload);
                Assert.NotEmpty(logFactory.Configuration.FileNamesToWatch);
                Assert.Contains(mainConfigFilePath, logFactory.Configuration.FileNamesToWatch);
                Assert.Contains(included2ConfigFilePath, logFactory.Configuration.FileNamesToWatch);
            }
            finally
            {
                logFactory.Shutdown();

                if (Directory.Exists(tempDir))
                    Directory.Delete(tempDir, true);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void TestMainConfigReloadIncludedConfigNoReload(bool useExplicitFileLoading)
        {
            string mainConfig1 = @"<nlog autoReload='true'>
                  <include file='included.nlog' />
                  <rules><logger name='*' minlevel='Debug' writeTo='debug' /></rules>
                </nlog>";
            string mainConfig2 = @"<nlog autoReload='true'>
                  <include file='included2.nlog' />
                  <rules><logger name='*' minlevel='Debug' writeTo='debug' /></rules>
                </nlog>";
            string included1Config = @"<nlog>
                    <targets><target name='debug' type='Debug' layout='${message}' /></targets>
                </nlog>";
            string included2Config1 = @"<nlog autoReload='false'>
                    <targets><target name='debug' type='Debug' layout='[${message}]' /></targets>
                </nlog>";
            string included2Config2 = @"<nlog autoReload='false'>
                    <targets><target name='debug' type='Debug' layout='(${message})' /></targets>
                </nlog>";

            string tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

            var logFactory = new LogFactory();

            try
            {
                Directory.CreateDirectory(tempDir);

                string mainConfigFilePath = Path.Combine(tempDir, "main.nlog");
                WriteConfigFile(mainConfigFilePath, mainConfig1);

                string included1ConfigFilePath = Path.Combine(tempDir, "included.nlog");
                WriteConfigFile(included1ConfigFilePath, included1Config);

                string included2ConfigFilePath = Path.Combine(tempDir, "included2.nlog");
                WriteConfigFile(included2ConfigFilePath, included2Config1);

                SetLogManagerConfiguration(logFactory, useExplicitFileLoading, mainConfigFilePath);

                var logger = logFactory.GetCurrentClassLogger();
                logger.Debug("aaa");
                AssertDebugLastMessage("debug", "aaa", logFactory);
                Assert.True(((XmlLoggingConfiguration)logFactory.Configuration).AutoReload);
                Assert.NotEmpty(logFactory.Configuration.FileNamesToWatch);
                Assert.Contains(mainConfigFilePath, logFactory.Configuration.FileNamesToWatch);
                Assert.Contains(included1ConfigFilePath, logFactory.Configuration.FileNamesToWatch);

                WriteConfigFileAndReload(logFactory, mainConfigFilePath, mainConfig2);

                logger.Debug("bbb");
                // Assert that mainConfig2 is loaded (which refers to included2.nlog).
                AssertDebugLastMessage("debug", "[bbb]", logFactory);
                Assert.True(((XmlLoggingConfiguration)logFactory.Configuration).AutoReload);
                Assert.Single(logFactory.Configuration.FileNamesToWatch);
                Assert.Equal(mainConfigFilePath, logFactory.Configuration.FileNamesToWatch.FirstOrDefault());

                WriteConfigFileAndReload(logFactory, included2ConfigFilePath, included2Config2);

                logger.Debug("ccc");
                // Assert that included2Config2 has been loaded.
                AssertDebugLastMessage("debug", "(ccc)", logFactory);
                Assert.True(((XmlLoggingConfiguration)logFactory.Configuration).AutoReload);
                Assert.Single(logFactory.Configuration.FileNamesToWatch);
                Assert.Equal(mainConfigFilePath, logFactory.Configuration.FileNamesToWatch.FirstOrDefault());
            }
            finally
            {
                logFactory.Shutdown();

                if (Directory.Exists(tempDir))
                    Directory.Delete(tempDir, true);
            }
        }

        [Fact]
        public void TestKeepVariablesOnReload()
        {
            string config = @"<nlog autoReload='true' keepVariablesOnReload='true'>
                                <variable name='var1' value='' />
                                <variable name='var2' value='keep_value' />
                            </nlog>";

            var logFactory = new LogFactory();
            var configuration = XmlLoggingConfigurationMock.CreateFromXml(logFactory, config);
            logFactory.Configuration = configuration;
            logFactory.Configuration.Variables["var1"] = "new_value";
            logFactory.Configuration.Variables["var3"] = "new_value3";

            var nullEvent = LogEventInfo.CreateNullEvent();
            Assert.Equal("new_value", logFactory.Configuration.Variables["var1"].Render(nullEvent));
            Assert.Equal("keep_value", logFactory.Configuration.Variables["var2"].Render(nullEvent));
            Assert.Equal("new_value3", logFactory.Configuration.Variables["var3"].Render(nullEvent));

            logFactory.Setup().ReloadConfiguration();
            Assert.Equal("new_value", logFactory.Configuration.Variables["var1"].Render(nullEvent));
            Assert.Equal("keep_value", logFactory.Configuration.Variables["var2"].Render(nullEvent));
            Assert.Equal("new_value3", logFactory.Configuration.Variables["var3"].Render(nullEvent));
        }

        [Fact]
        public void TestKeepVariablesOnReloadAllowUpdate()
        {
            string config1 = @"<nlog autoReload='true' keepVariablesOnReload='true'>
                                <variable name='var1' value='' />
                                <variable name='var2' value='old_value2' />
                                <targets><target name='mem' type='memory' layout='${var:var2}' /></targets>
                                <rules><logger name='*' writeTo='mem' /></rules>
                            </nlog>";

            string config2 = @"<nlog autoReload='true' keepVariablesOnReload='true'>
                                <variable name='var1' value='' />
                                <variable name='var2' value='new_value2' />
                                <targets><target name='mem' type='memory' layout='${var:var2}' /></targets>
                                <rules><logger name='*' writeTo='mem' /></rules>
                            </nlog>";

            var logFactory = new LogFactory();
            var xmlConfig = XmlLoggingConfigurationMock.CreateFromXml(logFactory, config1);
            logFactory.Configuration = xmlConfig;

            // Act
            logFactory.Configuration.Variables.Remove("var1");
            logFactory.Configuration.Variables.Add("var3", "new_value3");
            xmlConfig.ConfigXml = config2;
            logFactory.Configuration = xmlConfig.Reload();

            // Assert
            var nullEvent = LogEventInfo.CreateNullEvent();
            Assert.Equal("", logFactory.Configuration.Variables["var1"].Render(nullEvent));
            Assert.Equal("new_value2", logFactory.Configuration.Variables["var2"].Render(nullEvent));
            Assert.Equal("new_value3", logFactory.Configuration.Variables["var3"].Render(nullEvent));
        }

        [Fact]
        public void TestResetVariablesOnReload()
        {
            string config = @"<nlog autoReload='true' keepVariablesOnReload='false'>
                                <variable name='var1' value='' />
                                <variable name='var2' value='keep_value' />
                            </nlog>";

            var logFactory = new LogFactory();
            var configuration = XmlLoggingConfigurationMock.CreateFromXml(logFactory, config);
            logFactory.Configuration = configuration;
            logFactory.Configuration.Variables["var1"] = "new_value";
            logFactory.Configuration.Variables["var3"] = "new_value3";
            
            LogEventInfo nullEvent = LogEventInfo.CreateNullEvent();
            Assert.Equal("new_value", logFactory.Configuration.Variables["var1"].Render(nullEvent));
            Assert.Equal("keep_value", logFactory.Configuration.Variables["var2"].Render(nullEvent));

            logFactory.Configuration = configuration.Reload();
            Assert.Equal("", logFactory.Configuration.Variables["var1"].Render(nullEvent));
            Assert.Equal("keep_value", logFactory.Configuration.Variables["var2"].Render(nullEvent));
        }

        [Fact]
        public void KeepVariablesOnReloadWithStaticMode()
        {
            // Arrange
            string config = @"<nlog autoReload='true'>
                                <variable name='maxArchiveDays' value='7' />
                                <targets>
                                    <target name='logfile' type='file' fileName='test.log' maxArchiveDays='${maxArchiveDays}' />
                                </targets>
                                <rules>
                                    <logger name='*' minLevel='Debug' writeTo='logfile' />
                                </rules>
                            </nlog>";
            var logFactory = new LogFactory();
            logFactory.Configuration = XmlLoggingConfigurationMock.CreateFromXml(logFactory, config);

            var fileTarget = logFactory.Configuration.AllTargets[0] as NLog.Targets.FileTarget;
            var beforeValue = fileTarget.MaxArchiveDays;

            // Act
            logFactory.Configuration.Variables["MaxArchiveDays"] = "42";
            logFactory.Configuration = logFactory.Configuration.Reload();
            fileTarget = logFactory.Configuration.AllTargets[0] as NLog.Targets.FileTarget;
            var afterValue = fileTarget.MaxArchiveDays;

            // Assert
            Assert.Equal(7, beforeValue);
            Assert.Equal(42, afterValue);
        }

        [Fact]
        public void ReloadConfigOnTimer_When_No_Exception_Raises_ConfigurationReloadedEvent()
        {
            var called = false;
            LoggingConfigurationChangedEventArgs arguments = null;
            object calledBy = null;

            var logFactory = new LogFactory();
            var loggingConfiguration = XmlLoggingConfigurationMock.CreateFromXml(logFactory, "<nlog></nlog>");
            logFactory.Configuration = loggingConfiguration;
            logFactory.ConfigurationChanged += (sender, args) => { called = true; calledBy = sender; arguments = args; };

            logFactory.Setup().ReloadConfiguration();

            Assert.True(called);
            Assert.Same(calledBy, logFactory);
            Assert.NotNull(arguments.ActivatedConfiguration);
        }

        [Fact]
        public void TestReloadingInvalidConfiguration()
        {
            var validXmlConfig = @"<nlog>
                    <targets><target name='debug' type='Debug' layout='${message}' /></targets>
                    <rules>
                        <logger name='*' minlevel='Debug' writeTo='debug' />
                    </rules>
                </nlog>";
            var invalidXmlConfig = @"<nlog autoReload='true' internalLogLevel='debug' internalLogLevel='error'>
                    <targets><target name='debug' type='Debug' layout='${message}' /></targets>
                </nlog>";

            string tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);

            try
            {
                using (new NoThrowNLogExceptions())
                {
                    var nlogConfigFile = Path.Combine(tempDir, "NLog.config");
                    LogFactory logFactory = new LogFactory();
                    logFactory.Setup().LoadConfigurationFromFile(new[] { nlogConfigFile });
                    var config = logFactory.Configuration;
                    Assert.Null(config);

                    WriteConfigFile(nlogConfigFile, invalidXmlConfig);
                    config = logFactory.Configuration;
                    Assert.NotNull(config);
                    Assert.Empty(config.AllTargets);        // Failed to load
                    Assert.Single(config.FileNamesToWatch); // But file-watcher is active

                    WriteConfigFile(nlogConfigFile, validXmlConfig);
                    config = logFactory.Configuration.Reload();
                    Assert.Single(config.AllTargets);
                }
            }
            finally
            {
                if (Directory.Exists(tempDir))
                {
                    Directory.Delete(tempDir, true);
                }
            }
        }

        [Fact]
        public void TestThrowExceptionWhenInvalidXml()
        {
            var invalidXmlConfig = @"<nlog throwExceptions='true' internalLogLevel='debug' internalLogLevel='error'>
                </nlog>";

            string tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);

            try
            {
                using (new NoThrowNLogExceptions())
                {
                    var nlogConfigFile = Path.Combine(tempDir, "NLog.config");
                    WriteConfigFile(nlogConfigFile, invalidXmlConfig);
                    LogFactory logFactory = new LogFactory();
                    Assert.Throws<NLogConfigurationException>(() => logFactory.Setup().LoadConfigurationFromFile(new[] { nlogConfigFile }).GetCurrentClassLogger());
                }
            }
            finally
            {
                if (Directory.Exists(tempDir))
                {
                    Directory.Delete(tempDir, true);
                }
            }
        }

        private static void WriteConfigFile(string configFilePath, string config)
        {
            using (StreamWriter writer = File.CreateText(configFilePath))
                writer.Write(config);
        }

        private static void WriteConfigFileAndReload(LogFactory logFactory, string configFilePath, string config)
        {
            using (StreamWriter writer = File.CreateText(configFilePath))
                writer.Write(config);

            try
            {
                logFactory.Setup().ReloadConfiguration();
            }
            catch
            {
                // Swallow issues from loading bad config
            }
        }
    }

    /// <summary>
    /// Xml config with reload without file-reads for performance
    /// </summary>
    public class XmlLoggingConfigurationMock : XmlLoggingConfiguration
    {
        public string ConfigXml { get; set; }

        private XmlLoggingConfigurationMock(LogFactory logFactory, string configXml)
            : base(logFactory)
        {
            ConfigXml = configXml;
        }

        public override LoggingConfiguration Reload()
        {
            var newConfig = new XmlLoggingConfigurationMock(LogFactory, ConfigXml);
            newConfig.PrepareForReload(this);
            newConfig.LoadFromXmlContent(ConfigXml, null);
            return newConfig;
        }

        public static XmlLoggingConfigurationMock CreateFromXml(LogFactory logFactory, string configXml)
        {
            var newConfig = new XmlLoggingConfigurationMock(logFactory, configXml);
            newConfig.LoadFromXmlContent(configXml, null);
            return newConfig;
        }
    }
}
