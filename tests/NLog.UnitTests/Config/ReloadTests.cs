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

#if !SILVERLIGHT && !MONO
namespace NLog.UnitTests.Config
{
    using NLog.Config;
    using System;
    using System.IO;
    using System.Threading;
    using Xunit;

    public class ReloadTests : NLogTestBase
    {
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
            
            string tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempPath);

            string configFilePath = Path.Combine(tempPath, "noreload.nlog");
            WriteConfigFile(configFilePath, config1);

            try
            {
                LogManager.Configuration = new XmlLoggingConfiguration(configFilePath);

                Assert.False(((XmlLoggingConfiguration)LogManager.Configuration).AutoReload);

                var logger = LogManager.GetLogger("A");
                logger.Debug("aaa");
                AssertDebugLastMessage("debug", "aaa");

                ChangeAndReloadConfigFile(configFilePath, config2, assertDidReload: false);

                logger.Debug("bbb");
                // Assert that config1 is still loaded.
                AssertDebugLastMessage("debug", "bbb");
            }
            finally
            {
                if (Directory.Exists(tempPath))
                    Directory.Delete(tempPath, true);
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
            
            string tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempPath);

            string configFilePath = Path.Combine(tempPath, "reload.nlog");
            WriteConfigFile(configFilePath, config1);

            try
            {
                LogManager.Configuration = new XmlLoggingConfiguration(configFilePath);

                Assert.True(((XmlLoggingConfiguration)LogManager.Configuration).AutoReload);

                var logger = LogManager.GetLogger("A");
                logger.Debug("aaa");
                AssertDebugLastMessage("debug", "aaa");

                ChangeAndReloadConfigFile(configFilePath, badConfig, assertDidReload: false);

                logger.Debug("bbb");
                // Assert that config1 is still loaded.
                AssertDebugLastMessage("debug", "bbb");

                ChangeAndReloadConfigFile(configFilePath, config2);
                
                logger.Debug("ccc");
                // Assert that config2 is loaded.
                AssertDebugLastMessage("debug", "[ccc]");
            }
            finally
            {
                if (Directory.Exists(tempPath))
                    Directory.Delete(tempPath, true);
            }
        }

        [Fact]
        public void TestAutoReloadOnFileMove()
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
            Directory.CreateDirectory(tempPath);

            string configFilePath = Path.Combine(tempPath, "reload.nlog");
            WriteConfigFile(configFilePath, config1);
            string otherFilePath = Path.Combine(tempPath, "other.nlog");

            try
            {
                LogManager.Configuration = new XmlLoggingConfiguration(configFilePath);

                var logger = LogManager.GetLogger("A");
                logger.Debug("aaa");
                AssertDebugLastMessage("debug", "aaa");
                
                using (var reloadWaiter = new ConfigurationReloadWaiter())
                {
                    File.Move(configFilePath, otherFilePath);
                    reloadWaiter.WaitForReload();
                }
                
                logger.Debug("bbb");
                // Assert that config1 is still loaded.
                AssertDebugLastMessage("debug", "bbb");

                WriteConfigFile(otherFilePath, config2);
                using (var reloadWaiter = new ConfigurationReloadWaiter())
                {
                    File.Move(otherFilePath, configFilePath);

                    reloadWaiter.WaitForReload();
                    Assert.True(reloadWaiter.DidReload);
                }

                logger.Debug("ccc");
                // Assert that config2 is loaded.
                AssertDebugLastMessage("debug", "[ccc]");
            }
            finally
            {
                if (Directory.Exists(tempPath))
                    Directory.Delete(tempPath, true);
            }
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
            Directory.CreateDirectory(tempPath);

            string configFilePath = Path.Combine(tempPath, "reload.nlog");
            WriteConfigFile(configFilePath, config1);
            string otherFilePath = Path.Combine(tempPath, "other.nlog");

            try
            {
                LogManager.Configuration = new XmlLoggingConfiguration(configFilePath);

                var logger = LogManager.GetLogger("A");
                logger.Debug("aaa");
                AssertDebugLastMessage("debug", "aaa");

                using (var reloadWaiter = new ConfigurationReloadWaiter())
                {
                    File.Delete(configFilePath);
                    reloadWaiter.WaitForReload();
                }

                logger.Debug("bbb");
                // Assert that config1 is still loaded.
                AssertDebugLastMessage("debug", "bbb");

                WriteConfigFile(otherFilePath, config2);
                using (var reloadWaiter = new ConfigurationReloadWaiter())
                {
                    File.Copy(otherFilePath, configFilePath);
                    File.Delete(otherFilePath);

                    reloadWaiter.WaitForReload();
                    Assert.True(reloadWaiter.DidReload);
                }

                logger.Debug("ccc");
                // Assert that config2 is loaded.
                AssertDebugLastMessage("debug", "[ccc]");
            }
            finally
            {
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
            
            string tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempPath);

            string mainConfigFilePath = Path.Combine(tempPath, "main.nlog");
            WriteConfigFile(mainConfigFilePath, mainConfig1);

            string includedConfigFilePath = Path.Combine(tempPath, "included.nlog");
            WriteConfigFile(includedConfigFilePath, includedConfig1);

            try
            {
                LogManager.Configuration = new XmlLoggingConfiguration(mainConfigFilePath);

                var logger = LogManager.GetLogger("A");
                logger.Debug("aaa");
                AssertDebugLastMessage("debug", "aaa");

                ChangeAndReloadConfigFile(mainConfigFilePath, mainConfig2, assertDidReload: false);

                logger.Debug("bbb");
                // Assert that mainConfig1 is still loaded.
                AssertDebugLastMessage("debug", "bbb");

                WriteConfigFile(mainConfigFilePath, mainConfig1);
                ChangeAndReloadConfigFile(includedConfigFilePath, includedConfig2, assertDidReload: false);

                logger.Debug("ccc");
                // Assert that includedConfig1 is still loaded.
                AssertDebugLastMessage("debug", "ccc");
            }
            finally
            {
                if (Directory.Exists(tempPath))
                    Directory.Delete(tempPath, true);
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
            
            string tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempPath);

            string mainConfigFilePath = Path.Combine(tempPath, "main.nlog");
            WriteConfigFile(mainConfigFilePath, mainConfig1);

            string includedConfigFilePath = Path.Combine(tempPath, "included.nlog");
            WriteConfigFile(includedConfigFilePath, includedConfig1);

            try
            {
                LogManager.Configuration = new XmlLoggingConfiguration(mainConfigFilePath);

                var logger = LogManager.GetLogger("A");
                logger.Debug("aaa");
                AssertDebugLastMessage("debug", "aaa");

                ChangeAndReloadConfigFile(mainConfigFilePath, mainConfig2, assertDidReload: false);
                
                logger.Debug("bbb");
                // Assert that mainConfig1 is still loaded.
                AssertDebugLastMessage("debug", "bbb");

                WriteConfigFile(mainConfigFilePath, mainConfig1);
                ChangeAndReloadConfigFile(includedConfigFilePath, includedConfig2);
                
                logger.Debug("ccc");
                // Assert that includedConfig2 is loaded.
                AssertDebugLastMessage("debug", "[ccc]");
            }
            finally
            {
                if (Directory.Exists(tempPath))
                    Directory.Delete(tempPath, true);
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
            
            string tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempPath);

            string mainConfigFilePath = Path.Combine(tempPath, "main.nlog");
            WriteConfigFile(mainConfigFilePath, mainConfig1);

            string included1ConfigFilePath = Path.Combine(tempPath, "included.nlog");
            WriteConfigFile(included1ConfigFilePath, included1Config);

            string included2ConfigFilePath = Path.Combine(tempPath, "included2.nlog");
            WriteConfigFile(included2ConfigFilePath, included2Config1);

            try
            {
                LogManager.Configuration = new XmlLoggingConfiguration(mainConfigFilePath);

                var logger = LogManager.GetLogger("A");
                logger.Debug("aaa");
                AssertDebugLastMessage("debug", "aaa");

                ChangeAndReloadConfigFile(mainConfigFilePath, mainConfig2);

                logger.Debug("bbb");
                // Assert that mainConfig2 is loaded (which refers to included2.nlog).
                AssertDebugLastMessage("debug", "[bbb]");

                ChangeAndReloadConfigFile(included2ConfigFilePath, included2Config2);

                logger.Debug("ccc");
                // Assert that included2Config2 is loaded.
                AssertDebugLastMessage("debug", "(ccc)");
            }
            finally
            {
                if (Directory.Exists(tempPath))
                    Directory.Delete(tempPath, true);
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
            
            string tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempPath);

            string mainConfigFilePath = Path.Combine(tempPath, "main.nlog");
            WriteConfigFile(mainConfigFilePath, mainConfig1);

            string included1ConfigFilePath = Path.Combine(tempPath, "included.nlog");
            WriteConfigFile(included1ConfigFilePath, included1Config);

            string included2ConfigFilePath = Path.Combine(tempPath, "included2.nlog");
            WriteConfigFile(included2ConfigFilePath, included2Config1);

            try
            {
                LogManager.Configuration = new XmlLoggingConfiguration(mainConfigFilePath);

                var logger = LogManager.GetLogger("A");
                logger.Debug("aaa");
                AssertDebugLastMessage("debug", "aaa");

                ChangeAndReloadConfigFile(mainConfigFilePath, mainConfig2);

                logger.Debug("bbb");
                // Assert that mainConfig2 is loaded (which refers to included2.nlog).
                AssertDebugLastMessage("debug", "[bbb]");

                ChangeAndReloadConfigFile(included2ConfigFilePath, included2Config2, assertDidReload: false);

                logger.Debug("ccc");
                // Assert that included2Config1 is still loaded.
                AssertDebugLastMessage("debug", "[ccc]");
            }
            finally
            {
               
                if (Directory.Exists(tempPath))
                    Directory.Delete(tempPath, true);
            }
        }


        private static void WriteConfigFile(string configFilePath, string config)
        {
            using (StreamWriter writer = File.CreateText(configFilePath))
                writer.Write(config);
        }

        private static void ChangeAndReloadConfigFile(string configFilePath, string config, bool assertDidReload = true)
        {
            using (var reloadWaiter = new ConfigurationReloadWaiter())
            {
                WriteConfigFile(configFilePath, config);
                reloadWaiter.WaitForReload();

                if (assertDidReload)
                    Assert.True(reloadWaiter.DidReload, "Config did not reload.");
            }
        }


        private class ConfigurationReloadWaiter : IDisposable
        {
            private CountdownEvent counterEvent = new CountdownEvent(1);

            public ConfigurationReloadWaiter()
            {
                LogManager.ConfigurationReloaded += SignalCounterEvent(counterEvent);
            }

            public bool DidReload { get { return counterEvent.CurrentCount == 0; } }

            public void Dispose()
            {
                LogManager.ConfigurationReloaded -= SignalCounterEvent(counterEvent);
            }

            public void WaitForReload()
            {
                counterEvent.Wait(2000);
            }

            private static EventHandler<LoggingConfigurationReloadedEventArgs> SignalCounterEvent(CountdownEvent counterEvent)
            {
                return (sender, e) =>
                {
                    if (counterEvent.CurrentCount > 0)
                        counterEvent.Signal();
                };
            }
        }
    }
}
#endif
