// 
// Copyright (c) 2004-2009 Jaroslaw Kowalski <jaak@jkowalski.net>
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
using System.Xml;
using System.Globalization;

using NLog;
using NLog.Common;
using NLog.Config;

using NUnit.Framework;
using NLog.Targets;
using System.IO;
using System.Threading;

using NLog.Internal;

namespace NLog.UnitTests
{
    [TestFixture]
    public class LogManagerTests : NLogTestBase
    {
        [Test]
        public void GetLoggerTest()
        {
            Logger loggerA = LogManager.GetLogger("A");
            Logger loggerA2 = LogManager.GetLogger("A");
            Logger loggerB = LogManager.GetLogger("B");
            Assert.AreSame(loggerA, loggerA2);
            Assert.AreNotSame(loggerA, loggerB);
            Assert.AreEqual("A", loggerA.Name);
            Assert.AreEqual("B", loggerB.Name);
        }

        [Test]
        public void NullLoggerTest()
        {
            Logger l = LogManager.CreateNullLogger();
            Assert.AreEqual("", l.Name);
        }

        [Test]
        public void ThrowExceptionsTest()
        {
            FileTarget ft = new FileTarget();
            ft.FileName = ""; // invalid file name
            SimpleConfigurator.ConfigureForTargetLogging(ft);
            LogManager.ThrowExceptions = false;
            LogManager.GetLogger("A").Info("a");
            LogManager.ThrowExceptions = true;
            try
            {
                LogManager.GetLogger("A").Info("a");
                Assert.Fail("Should not be reached.");
            }
            catch
            {
                Assert.IsTrue(true);
            }
            LogManager.ThrowExceptions = false;
        }

        public void GlobalThresholdTest()
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(@"
                <nlog globalThreshold='Info'>
                    <targets><target name='debug' type='Debug' layout='${message}' /></targets>
                    <rules>
                        <logger name='*' minlevel='Debug' writeTo='debug' />
                    </rules>
                </nlog>");

            LogManager.Configuration = new XmlLoggingConfiguration(doc.DocumentElement, null);

            Assert.AreEqual(LogLevel.Info, LogManager.GlobalThreshold);

            // nothing gets logged because of globalThreshold
            LogManager.GetLogger("A").Debug("xxx");
            AssertDebugLastMessage("debug", "");

            // lower the threshold
            LogManager.GlobalThreshold = LogLevel.Trace;

            LogManager.GetLogger("A").Debug("yyy");
            AssertDebugLastMessage("debug", "yyy");

            // raise the threshold
            LogManager.GlobalThreshold = LogLevel.Info;

            // this should be yyy, meaning that the target is in place
            // only rules have been modified.

            LogManager.GetLogger("A").Debug("zzz");
            AssertDebugLastMessage("debug", "yyy");
        }

        private int _reloadCounter = 0;

        private void OnConfigReloaded(object sender, LoggingConfigurationReloadedEventArgs e)
        {
            Console.WriteLine("OnConfigReloaded success={0}", e.Succeeded);
            _reloadCounter++;
        }

        private void WaitForConfigReload(int counter)
        {
            while (_reloadCounter < counter)
            {
                System.Threading.Thread.Sleep(100);
            }
        }

        [Test]
        [Category("LongRunning")]
        public void AutoReloadTest()
        {
            string fileName = Path.GetTempFileName();
            try
            {
                _reloadCounter = 0;
                LogManager.ConfigurationReloaded += OnConfigReloaded;
                using (StreamWriter fs = File.CreateText(fileName))
                {
                    fs.Write(@"<nlog autoReload='true'>
                    <targets><target name='debug' type='Debug' layout='${message}' /></targets>
                    <rules>
                        <logger name='*' minlevel='Debug' writeTo='debug' />
                    </rules>
                </nlog>");
                }
                LogManager.Configuration = new XmlLoggingConfiguration(fileName);
                AssertDebugCounter("debug", 0);
                Logger logger = LogManager.GetLogger("A");
                logger.Debug("aaa");
                AssertDebugLastMessage("debug", "aaa");

                InternalLogger.Info("Rewriting test file...");

                // now write the file again
                using (StreamWriter fs = File.CreateText(fileName))
                {
                    fs.Write(@"<nlog autoReload='true'>
                    <targets><target name='debug' type='Debug' layout='xxx ${message}' /></targets>
                    <rules>
                        <logger name='*' minlevel='Debug' writeTo='debug' />
                    </rules>
                </nlog>");
                }

                InternalLogger.Info("Rewritten.");
                WaitForConfigReload(1);

                logger.Debug("aaa");
                AssertDebugLastMessage("debug", "xxx aaa");

                // write the file again, this time make an error
                using (StreamWriter fs = File.CreateText(fileName))
                {
                    fs.Write(@"<nlog autoReload='true'>
                    <targets><tar get name='debug' type='Debug' layout='xxx ${message}' /></targets>
                    <rules>
                        <logger name='*' minlevel='Debug' writeTo='debug' />
                    </rules>
                </nlog>");
                }

                WaitForConfigReload(2);
                logger.Debug("bbb");
                AssertDebugLastMessage("debug", "xxx bbb");

                // write the corrected file again
                using (StreamWriter fs = File.CreateText(fileName))
                {
                    fs.Write(@"<nlog autoReload='true'>
                    <targets><target name='debug' type='Debug' layout='zzz ${message}' /></targets>
                    <rules>
                        <logger name='*' minlevel='Debug' writeTo='debug' />
                    </rules>
                </nlog>");
                }
                WaitForConfigReload(3);
                logger.Debug("ccc");
                AssertDebugLastMessage("debug", "zzz ccc");

            }
            finally
            {
                LogManager.ConfigurationReloaded -= OnConfigReloaded;
                LogManager.Configuration = null;
                if (File.Exists(fileName))
                    File.Delete(fileName);
            }
        }

        [Test]
        public void IncludeTest()
        {
            string tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempPath);
            try
            {
                using (StreamWriter fs = File.CreateText(Path.Combine(tempPath, "included.nlog")))
                {
                    fs.Write(@"<nlog>
                        <targets><target name='debug' type='Debug' layout='${message}' /></targets>
                </nlog>");
                }

                using (StreamWriter fs = File.CreateText(Path.Combine(tempPath, "main.nlog")))
                {
                    fs.Write(@"<nlog>
                    <include file='included.nlog' />
                    <rules>
                        <logger name='*' minlevel='Debug' writeTo='debug' />
                    </rules>
                </nlog>");
                }

                LogManager.Configuration = new XmlLoggingConfiguration(Path.Combine(tempPath, "main.nlog"));
                LogManager.GetLogger("A").Debug("aaa");
                AssertDebugLastMessage("debug", "aaa");
            }
            finally
            {
                LogManager.Configuration = null;
                if (Directory.Exists(tempPath))
                    Directory.Delete(tempPath, true);
            }
        }

        [Test]
        [ExpectedException(typeof(NLogConfigurationException))]
        public void IncludeNotExistingTest()
        {
            string tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempPath);

            try
            {
                using (StreamWriter fs = File.CreateText(Path.Combine(tempPath, "main.nlog")))
                {
                    fs.Write(@"<nlog>
                    <include file='included.nlog' />
                </nlog>");
                }

                LogManager.Configuration = new XmlLoggingConfiguration(Path.Combine(tempPath, "main.nlog"));
            }
            finally
            {
                LogManager.Configuration = null;
                if (Directory.Exists(tempPath))
                    Directory.Delete(tempPath, true);
            }
        }

        [Test]
        [ExpectedException(typeof(NLogConfigurationException))]
        public void IncludeNotExistingIgnoredTest()
        {
            string tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempPath);

            try
            {
                using (StreamWriter fs = File.CreateText(Path.Combine(tempPath, "main.nlog")))
                {
                    fs.Write(@"<nlog>
                    <include file='included-notpresent.nlog' />
                    <targets><target name='debug' type='Debug' layout='${message}' /></targets>
                    <rules>
                        <logger name='*' minlevel='Debug' writeTo='debug' />
                    </rules>
                </nlog>");
                }

                LogManager.Configuration = new XmlLoggingConfiguration(Path.Combine(tempPath, "main.nlog"));
                LogManager.GetLogger("A").Debug("aaa");
                AssertDebugLastMessage("debug", "aaa");
            }
            finally
            {
                LogManager.Configuration = null;
                if (Directory.Exists(tempPath))
                    Directory.Delete(tempPath, true);
            }
        }
    }
}
