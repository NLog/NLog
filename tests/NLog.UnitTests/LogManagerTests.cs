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
using System.IO;
using System.Xml;
using NUnit.Framework;

#if !NUNIT
    using SetUp = Microsoft.VisualStudio.TestTools.UnitTesting.TestInitializeAttribute;
    using TestFixture = Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute;
    using Test = Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute;
    using TearDown =  Microsoft.VisualStudio.TestTools.UnitTesting.TestCleanupAttribute;
#endif
using NLog.Common;
using NLog.Config;
using NLog.Targets;
using System.Diagnostics;
using System.Threading;

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
        public void GarbageCollectionTest()
        {
            string uniqueLoggerName = Guid.NewGuid().ToString();
            Logger loggerA1 = LogManager.GetLogger(uniqueLoggerName);
            GC.Collect();
            Logger loggerA2 = LogManager.GetLogger(uniqueLoggerName);
            Assert.AreSame(loggerA1, loggerA2);
        }

        static WeakReference GetWeakReferenceToTemporaryLogger()
        {
            string uniqueLoggerName = Guid.NewGuid ().ToString();
            return new WeakReference (LogManager.GetLogger(uniqueLoggerName));
        }

        [Test]
        public void GarbageCollection2Test()
        {
            WeakReference wr = GetWeakReferenceToTemporaryLogger();

            // nobody's holding a reference to this Logger anymore, so GC.Collect(2) should free it
            GC.Collect();
            Assert.IsFalse(wr.IsAlive);
        }

        [Test]
        public void NullLoggerTest()
        {
            Logger l = LogManager.CreateNullLogger();
            Assert.AreEqual("", l.Name);
        }

#if !SILVERLIGHT2 && !SILVERLIGHT3 && !WINDOWS_PHONE
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
#endif

        public void GlobalThresholdTest()
        {
            LogManager.Configuration = CreateConfigurationFromString(@"
                <nlog globalThreshold='Info'>
                    <targets><target name='debug' type='Debug' layout='${message}' /></targets>
                    <rules>
                        <logger name='*' minlevel='Debug' writeTo='debug' />
                    </rules>
                </nlog>");

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

#if !SILVERLIGHT && !NET_CF
        private int _reloadCounter = 0;

        private void WaitForConfigReload(int counter)
        {
            while (_reloadCounter < counter)
            {
                System.Threading.Thread.Sleep(100);
            }
        }

        private void OnConfigReloaded(object sender, LoggingConfigurationReloadedEventArgs e)
        {
            Console.WriteLine("OnConfigReloaded success={0}", e.Succeeded);
            _reloadCounter++;
        }

        private bool IsMacOsX ()
        {
#if MONO
            if (Directory.Exists("/Library/Frameworks/Mono.framework/"))
            {
                return true;
            }

#endif
            return false;
        }

        [Test]
        public void AutoReloadTest()
        {
            if (IsMacOsX())
            {
            	// skip this on Mac OS, since it requires root permissions for
            	// filesystem watcher
            	return;
            }
			
            using (new InternalLoggerScope())
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
        }
#endif
    }
}
