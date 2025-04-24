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
    using System.Threading;
    using NLog.Config;
    using NLog.Targets;
    using Xunit;

    public class AutoReloadTests
    {
        public AutoReloadTests()
        {
            LogManager.ThrowExceptions = true;
        }

        [Fact]
        public void TestNoAutoReload()
        {
            string config1 = @"<nlog>
                    <targets><target name='debug' type='Debug' layout='${message}' /></targets>
                    <rules><logger name='*' minlevel='Debug' writeTo='debug' /></rules>
                </nlog>";
            string config2 = @"<nlog>
                        <targets><target name='debug' type='Debug' layout='[${message}]' /></targets>
                        <rules><logger name='*' minlevel='Debug' writeTo='debug' /></rules>
                    </nlog>";

            string tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

            var logFactory = new LogFactory();

            try
            {
                Directory.CreateDirectory(tempDir);

                string configFilePath = Path.Combine(tempDir, nameof(TestNoAutoReload) + ".nlog");
                WriteConfigFile(configFilePath, config1);

                var logger = logFactory.Setup().LoadConfigurationFromFile(configFilePath).GetCurrentClassLogger();

                Assert.False(((XmlLoggingConfiguration)logFactory.Configuration).AutoReload);

                logger.Debug("aaa");
                AssertDebugLastMessage("aaa", logFactory);

                ChangeAndReloadConfigFile(logFactory, configFilePath, config2, assertDidReload: false);

                logger.Debug("bbb");
                // Assert that config1 is still loaded.
                AssertDebugLastMessage("bbb", logFactory);
            }
            finally
            {
                logFactory.Shutdown();

                if (Directory.Exists(tempDir))
                    Directory.Delete(tempDir, true);
            }
        }

        [Fact]
        public void TestAutoReloadOnFileChange()
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

                string configFilePath = Path.Combine(tempDir, nameof(TestAutoReloadOnFileChange) + ".nlog");
                WriteConfigFile(configFilePath, config1);

                var logger = logFactory.Setup().LoadConfigurationFromFile(configFilePath).GetCurrentClassLogger();

                Assert.True(((XmlLoggingConfiguration)logFactory.Configuration).AutoReload);

                logger.Debug("aaa");
                AssertDebugLastMessage("aaa", logFactory);

                ChangeAndReloadConfigFile(logFactory, configFilePath, badConfig, assertDidReload: false);

                logger.Debug("bbb");
                // Assert that config1 is still loaded.
                AssertDebugLastMessage("bbb", logFactory);

                ChangeAndReloadConfigFile(logFactory, configFilePath, config2);

                logger.Debug("ccc");
                // Assert that config2 is loaded.
                AssertDebugLastMessage("[ccc]", logFactory);
            }
            finally
            {
                logFactory.Shutdown();

                if (Directory.Exists(tempDir))
                    Directory.Delete(tempDir, true);
            }
        }

        [Fact]
        public void TestAutoReloadOnFileMove()
        {
#if !NETFRAMEWORK || MONO
            if (IsLinux())
            {
                Console.WriteLine("[SKIP] AutoReloadTests.TestAutoReloadOnFileMove because we are running in Travis");
                return;
            }
#endif

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

                var logger = logFactory.Setup().LoadConfigurationFromFile(configFilePath).GetCurrentClassLogger();

                logger.Debug("aaa");
                AssertDebugLastMessage("aaa", logFactory);

                using (var reloadWaiter = new ConfigurationReloadWaiter(logFactory))
                {
                    File.Move(configFilePath, otherFilePath);
                    reloadWaiter.WaitForReload();
                }

                logger.Debug("bbb");
                // Assert that config1 is still loaded.
                AssertDebugLastMessage("bbb", logFactory);

                WriteConfigFile(otherFilePath, config2);
                using (var reloadWaiter = new ConfigurationReloadWaiter(logFactory))
                {
                    File.Move(otherFilePath, configFilePath);

                    reloadWaiter.WaitForReload();
                    Assert.True(reloadWaiter.DidReload);
                }

                logger.Debug("ccc");
                // Assert that config2 is loaded.
                AssertDebugLastMessage("[ccc]", logFactory);
            }
            finally
            {
                logFactory.Shutdown();

                if (Directory.Exists(tempDir))
                    Directory.Delete(tempDir, true);
            }
        }

        protected static bool IsLinux()
        {
            var val = Environment.GetEnvironmentVariable("WINDIR");
            return string.IsNullOrEmpty(val);
        }

        [Fact]
        public void TestAutoReloadOnFileCopy()
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

                var logger = logFactory.Setup().LoadConfigurationFromFile(configFilePath).GetCurrentClassLogger();

                logger.Debug("aaa");
                AssertDebugLastMessage("aaa", logFactory);

                using (var reloadWaiter = new ConfigurationReloadWaiter(logFactory))
                {
                    File.Delete(configFilePath);
                    reloadWaiter.WaitForReload();
                }

                logger.Debug("bbb");
                // Assert that config1 is still loaded.
                AssertDebugLastMessage("bbb", logFactory);

                WriteConfigFile(otherFilePath, config2);
                using (var reloadWaiter = new ConfigurationReloadWaiter(logFactory))
                {
                    File.Copy(otherFilePath, configFilePath);
                    File.Delete(otherFilePath);

                    reloadWaiter.WaitForReload();
                    Assert.True(reloadWaiter.DidReload);
                }

                logger.Debug("ccc");
                // Assert that config2 is loaded.
                AssertDebugLastMessage("[ccc]", logFactory);
            }
            finally
            {
                logFactory.Shutdown();

                if (Directory.Exists(tempPath))
                    Directory.Delete(tempPath, true);
            }
        }

        [Fact]
        public void TestIncludedConfigNoReload()
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

                var logger = logFactory.Setup().LoadConfigurationFromFile(mainConfigFilePath).GetCurrentClassLogger();

                logger.Debug("aaa");
                AssertDebugLastMessage("aaa", logFactory);

                ChangeAndReloadConfigFile(logFactory, mainConfigFilePath, mainConfig2, assertDidReload: false);

                logger.Debug("bbb");
                // Assert that mainConfig1 is still loaded.
                AssertDebugLastMessage("bbb", logFactory);

                WriteConfigFile(mainConfigFilePath, mainConfig1);
                ChangeAndReloadConfigFile(logFactory, includedConfigFilePath, includedConfig2, assertDidReload: false);

                logger.Debug("ccc");
                // Assert that includedConfig1 is still loaded.
                AssertDebugLastMessage("ccc", logFactory);
            }
            finally
            {
                logFactory.Shutdown();

                if (Directory.Exists(tempDir))
                    Directory.Delete(tempDir, true);
            }
        }

        [Fact]
        public void TestIncludedConfigReload()
        {
            string mainConfig1 = @"<nlog>
                  <include file='included.nlog' />
                  <rules><logger name='*' minlevel='Debug' writeTo='debug' /></rules>
                </nlog>";
            string mainConfig2 = @"<nlog>
                  <include file='included.nlog' />
                  <rules><logger name='*' minlevel='Info' writeTo='debug' /></rules>
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

                var logger = logFactory.Setup().LoadConfigurationFromFile(mainConfigFilePath).GetCurrentClassLogger();

                logger.Debug("aaa");
                AssertDebugLastMessage("aaa", logFactory);

                ChangeAndReloadConfigFile(logFactory, mainConfigFilePath, mainConfig2, assertDidReload: false);

                logger.Debug("bbb");
                // Assert that mainConfig1 is still loaded.
                AssertDebugLastMessage("bbb", logFactory);

                WriteConfigFile(mainConfigFilePath, mainConfig1);
                ChangeAndReloadConfigFile(logFactory, includedConfigFilePath, includedConfig2);

                logger.Debug("ccc");
                // Assert that includedConfig2 is loaded.
                AssertDebugLastMessage("[ccc]", logFactory);
            }
            finally
            {
                logFactory.Shutdown();

                if (Directory.Exists(tempDir))
                    Directory.Delete(tempDir, true);
            }
        }

        [Fact]
        public void TestMainConfigReload()
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

                var logger = logFactory.Setup().LoadConfigurationFromFile(mainConfigFilePath).GetCurrentClassLogger();

                logger.Debug("aaa");
                AssertDebugLastMessage("aaa", logFactory);

                ChangeAndReloadConfigFile(logFactory, mainConfigFilePath, mainConfig2);

                logger.Debug("bbb");
                // Assert that mainConfig2 is loaded (which refers to included2.nlog).
                AssertDebugLastMessage("[bbb]", logFactory);

                ChangeAndReloadConfigFile(logFactory, included2ConfigFilePath, included2Config2);

                logger.Debug("ccc");
                // Assert that included2Config2 is loaded.
                AssertDebugLastMessage("(ccc)", logFactory);
            }
            finally
            {
                logFactory.Shutdown();

                if (Directory.Exists(tempDir))
                    Directory.Delete(tempDir, true);
            }
        }

        [Fact]
        public void TestMainConfigReloadIncludedConfigNoReload()
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

                var logger = logFactory.Setup().LoadConfigurationFromFile(mainConfigFilePath).GetCurrentClassLogger();

                logger.Debug("aaa");
                AssertDebugLastMessage("aaa", logFactory);

                ChangeAndReloadConfigFile(logFactory, mainConfigFilePath, mainConfig2);

                logger.Debug("bbb");
                // Assert that mainConfig2 is loaded (which refers to included2.nlog).
                AssertDebugLastMessage("[bbb]", logFactory);

                ChangeAndReloadConfigFile(logFactory, included2ConfigFilePath, included2Config2, assertDidReload: false);

                logger.Debug("ccc");
                // Assert that included2Config1 is still loaded.
                AssertDebugLastMessage("[ccc]", logFactory);
            }
            finally
            {
                logFactory.Shutdown();

                if (Directory.Exists(tempDir))
                    Directory.Delete(tempDir, true);
            }
        }

        private static void WriteConfigFile(string configFilePath, string config)
        {
            using (StreamWriter writer = File.CreateText(configFilePath))
                writer.Write(config);
        }

        private static void ChangeAndReloadConfigFile(LogFactory logFactory, string configFilePath, string config, bool assertDidReload = true)
        {
            using (var reloadWaiter = new ConfigurationReloadWaiter(logFactory))
            {
                WriteConfigFile(configFilePath, config);
                reloadWaiter.WaitForReload();

                if (assertDidReload)
                    Assert.True(reloadWaiter.DidReload, $"Config '{configFilePath}' did not reload.");
            }
        }

        protected static void AssertDebugLastMessage(string msg, LogFactory logFactory)
        {
            var debugTarget = logFactory.Configuration.FindTargetByName<DebugTarget>("debug");
            Assert.Equal(msg, debugTarget.LastMessage);
        }

        private sealed class ConfigurationReloadWaiter : IDisposable
        {
            private readonly ManualResetEvent _counterEvent = new ManualResetEvent(false);
            private readonly LogFactory _logFactory;

            public ConfigurationReloadWaiter(LogFactory logFactory)
            {
                _logFactory = logFactory;
                _logFactory.ConfigurationChanged += SignalCounterEvent(_counterEvent);
            }

            public bool DidReload => _counterEvent.WaitOne(0);

            public void Dispose()
            {
                _logFactory.ConfigurationChanged -= SignalCounterEvent(_counterEvent);
            }

            public void WaitForReload()
            {
                _counterEvent.WaitOne(3000);    // Handle Timer-delay of 1 sec
            }

            private static EventHandler<LoggingConfigurationChangedEventArgs> SignalCounterEvent(ManualResetEvent counterEvent)
            {
                return (sender, e) =>
                {
                    counterEvent.Set();
                };
            }
        }
    }
}
