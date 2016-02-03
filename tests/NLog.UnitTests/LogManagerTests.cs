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

using JetBrains.Annotations;

namespace NLog.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Diagnostics;
    using System.IO;
    using Xunit;
    using NLog.Common;
    using NLog.Config;
    using NLog.Layouts;
    using NLog.Targets;

#if NET4_5
    using System.Threading.Tasks;
    using Microsoft.Practices.Unity;
#endif

    public class LogManagerTests : NLogTestBase
    {
        [Fact]
        public void GetLoggerTest()
        {
            ILogger loggerA = LogManager.GetLogger("A");
            ILogger loggerA2 = LogManager.GetLogger("A");
            ILogger loggerB = LogManager.GetLogger("B");
            Assert.Same(loggerA, loggerA2);
            Assert.NotSame(loggerA, loggerB);
            Assert.Equal("A", loggerA.Name);
            Assert.Equal("B", loggerB.Name);
        }

        [Fact]
        public void GarbageCollectionTest()
        {
            string uniqueLoggerName = Guid.NewGuid().ToString();
            ILogger loggerA1 = LogManager.GetLogger(uniqueLoggerName);
            GC.Collect();
            ILogger loggerA2 = LogManager.GetLogger(uniqueLoggerName);
            Assert.Same(loggerA1, loggerA2);
        }

        static WeakReference GetWeakReferenceToTemporaryLogger()
        {
            string uniqueLoggerName = Guid.NewGuid().ToString();
            return new WeakReference(LogManager.GetLogger(uniqueLoggerName));
        }

        [Fact]
        public void GarbageCollection2Test()
        {
            WeakReference wr = GetWeakReferenceToTemporaryLogger();

            // nobody's holding a reference to this Logger anymore, so GC.Collect(2) should free it
            GC.Collect();
            Assert.False(wr.IsAlive);
        }

        [Fact]
        public void NullLoggerTest()
        {
            ILogger l = LogManager.CreateNullLogger();
            Assert.Equal(String.Empty, l.Name);
        }

        [Fact]
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
                Assert.True(false, "Should not be reached.");
            }
            catch
            {
                Assert.True(true);
            }
            LogManager.ThrowExceptions = false;
        }

        //[Fact(Skip="Side effects to other unit tests.")]
        public void GlobalThresholdTest()
        {
            LogManager.Configuration = CreateConfigurationFromString(@"
                <nlog globalThreshold='Info'>
                    <targets><target name='debug' type='Debug' layout='${message}' /></targets>
                    <rules>
                        <logger name='*' minlevel='Debug' writeTo='debug' />
                    </rules>
                </nlog>");

            Assert.Equal(LogLevel.Info, LogManager.GlobalThreshold);

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

            LogManager.Shutdown();
            LogManager.Configuration = null;
        }

        [Fact]
        public void DisableLoggingTest_UsingStatement()
        {
            const string LoggerConfig = @"
                <nlog>
                    <targets><target name='debug' type='Debug' layout='${message}' /></targets>
                    <rules>
                        <logger name='DisableLoggingTest_UsingStatement_A' levels='Trace' writeTo='debug' />
                        <logger name='DisableLoggingTest_UsingStatement_B' levels='Error' writeTo='debug' />
                    </rules>
                </nlog>";

            // Disable/Enable logging should affect ALL the loggers.
            ILogger loggerA = LogManager.GetLogger("DisableLoggingTest_UsingStatement_A");
            ILogger loggerB = LogManager.GetLogger("DisableLoggingTest_UsingStatement_B");
            LogManager.Configuration = CreateConfigurationFromString(LoggerConfig);

            // The starting state for logging is enable.
            Assert.True(LogManager.IsLoggingEnabled());

            loggerA.Trace("TTT");
            AssertDebugLastMessage("debug", "TTT");

            loggerB.Error("EEE");
            AssertDebugLastMessage("debug", "EEE");

            loggerA.Trace("---");
            AssertDebugLastMessage("debug", "---");

            using (LogManager.DisableLogging())
            {
                Assert.False(LogManager.IsLoggingEnabled());

                // The last of LastMessage outside using statement should be returned.

                loggerA.Trace("TTT");
                AssertDebugLastMessage("debug", "---");

                loggerB.Error("EEE");
                AssertDebugLastMessage("debug", "---");
            }

            Assert.True(LogManager.IsLoggingEnabled());

            loggerA.Trace("TTT");
            AssertDebugLastMessage("debug", "TTT");

            loggerB.Error("EEE");
            AssertDebugLastMessage("debug", "EEE");

            LogManager.Shutdown();
            LogManager.Configuration = null;
        }

        [Fact]
        public void DisableLoggingTest_WithoutUsingStatement()
        {
            const string LoggerConfig = @"
                <nlog>
                    <targets><target name='debug' type='Debug' layout='${message}' /></targets>
                    <rules>
                        <logger name='DisableLoggingTest_WithoutUsingStatement_A' levels='Trace' writeTo='debug' />
                        <logger name='DisableLoggingTest_WithoutUsingStatement_B' levels='Error' writeTo='debug' />
                    </rules>
                </nlog>";

            // Disable/Enable logging should affect ALL the loggers.
            ILogger loggerA = LogManager.GetLogger("DisableLoggingTest_WithoutUsingStatement_A");
            ILogger loggerB = LogManager.GetLogger("DisableLoggingTest_WithoutUsingStatement_B");
            LogManager.Configuration = CreateConfigurationFromString(LoggerConfig);

            // The starting state for logging is enable.
            Assert.True(LogManager.IsLoggingEnabled());

            loggerA.Trace("TTT");
            AssertDebugLastMessage("debug", "TTT");

            loggerB.Error("EEE");
            AssertDebugLastMessage("debug", "EEE");

            loggerA.Trace("---");
            AssertDebugLastMessage("debug", "---");

            LogManager.DisableLogging();
            Assert.False(LogManager.IsLoggingEnabled());

            // The last value of LastMessage before DisableLogging() should be returned.

            loggerA.Trace("TTT");
            AssertDebugLastMessage("debug", "---");

            loggerB.Error("EEE");
            AssertDebugLastMessage("debug", "---");

            LogManager.EnableLogging();

            Assert.True(LogManager.IsLoggingEnabled());

            loggerA.Trace("TTT");
            AssertDebugLastMessage("debug", "TTT");

            loggerB.Error("EEE");
            AssertDebugLastMessage("debug", "EEE");

            LogManager.Shutdown();
            LogManager.Configuration = null;
        }

#if !SILVERLIGHT
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

        private bool IsMacOsX()
        {
#if MONO
            if (Directory.Exists("/Library/Frameworks/Mono.framework/"))
            {
                return true;
            }

#endif
            return false;
        }

        [Fact]
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
                    ILogger logger = LogManager.GetLogger("A");
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
                    <targets><target name='debug' type='Debug' layout='xxx ${message}' /></targets>
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
                    if (File.Exists(fileName))
                        File.Delete(fileName);
                }
            }
        }
#endif

        [Fact]
        public void GivenCurrentClass_WhenGetCurrentClassLogger_ThenLoggerShouldBeCurrentClass()
        {
            var logger = LogManager.GetCurrentClassLogger();

            Assert.Equal(this.GetType().FullName, logger.Name);
        }

        private static class ImAStaticClass
        {
            [UsedImplicitly]
            private static readonly Logger Logger = NLog.LogManager.GetCurrentClassLogger();

            static ImAStaticClass() { }

            public static void DummyToInvokeInitializers() { }

        }

        [Fact]
        void GetCurrentClassLogger_static_class()
        {
            ImAStaticClass.DummyToInvokeInitializers();
        }

        private abstract class ImAAbstractClass
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="T:System.Object"/> class.
            /// </summary>
            protected ImAAbstractClass()
            {
                Logger Logger = NLog.LogManager.GetCurrentClassLogger();
            }
        }

        private class InheritedFromAbstractClass : ImAAbstractClass
        {

        }

        /// <summary>
        /// Creating instance in a static ctor should not be a problm
        /// </summary>
        [Fact]
        void GetCurrentClassLogger_abstract_class()
        {
            var instance = new InheritedFromAbstractClass();
        }

        /// <summary>
        /// I'm a class which isn't inhereting from Logger
        /// </summary>
        private class ImNotALogger
        {

        }



        /// <summary>
        /// ImNotALogger inherits not from Logger , but should not throw an exception
        /// </summary>
        [Fact]
        void GetLogger_wrong_loggertype_should_continue()
        {
            var instance = LogManager.GetLogger("a", typeof(ImNotALogger));
            Assert.NotNull(instance);

        }

        /// <summary>
        /// ImNotALogger inherits not from Logger , but should not throw an exception
        /// </summary>
        [Fact]
        void GetLogger_wrong_loggertype_should_continue_even_if_class_is_static()
        {
            var instance = LogManager.GetLogger("a", typeof(ImAStaticClass));
            Assert.NotNull(instance);

        }


#if NET4_0 || NET4_5
        [Fact]
        public void GivenLazyClass_WhenGetCurrentClassLogger_ThenLoggerNameShouldBeCurrentClass()
        {
            var logger = new Lazy<ILogger>(LogManager.GetCurrentClassLogger);

            Assert.Equal(this.GetType().FullName, logger.Value.Name);
        }
#endif
#if NET4_5

        /// <summary>
        /// target for <see cref="ThreadSafe_getCurrentClassLogger_test"/>
        /// </summary>
        private static MemoryQueueTarget mTarget = new MemoryQueueTarget(500);
        /// <summary>
        /// target for <see cref="ThreadSafe_getCurrentClassLogger_test"/>
        /// </summary>
        private static MemoryQueueTarget mTarget2 = new MemoryQueueTarget(500);

        /// <summary>
        /// Note: THe problem  can be reproduced when: debugging the unittest + "break when exception is thrown" checked in visual studio.
        /// 
        /// https://github.com/NLog/NLog/issues/500
        /// </summary>
        [Fact]
        public void ThreadSafe_getCurrentClassLogger_test()
        {
            using (var c = new UnityContainer())
            {
                var r = Enumerable.Range(1, 100); //reported with 10.
                Task.Run(() =>
                {
                    //need for init
                    LogManager.Configuration = new LoggingConfiguration();
                    LogManager.Configuration.AddTarget("memory", mTarget);
                    LogManager.Configuration.LoggingRules.Add(new LoggingRule("*", LogLevel.Debug, mTarget));
                    LogManager.Configuration.AddTarget("memory2", mTarget2);
                    LogManager.Configuration.LoggingRules.Add(new LoggingRule("*", LogLevel.Debug, mTarget2));
                    LogManager.ReconfigExistingLoggers();
                });

                Parallel.ForEach(r, a =>
                {
                    var res = c.Resolve<ClassA>();
                });
                mTarget.Layout = @"${date:format=HH\:mm\:ss}|${level:uppercase=true}|${message} ${exception:format=tostring}";
                mTarget2.Layout = @"${date:format=HH\:mm\:ss}|${level:uppercase=true}|${message} ${exception:format=tostring}";
            }
        }

        /// <summary>
        /// target for <see cref="ThreadSafe_getCurrentClassLogger_test"/>
        /// </summary>
        [Target("Memory")]
        public sealed class MemoryQueueTarget : TargetWithLayout
        {
            private int maxSize;
            public MemoryQueueTarget(int size)
            {
                this.Logs = new Queue<string>();
                this.maxSize = size;
            }

            public Queue<string> Logs { get; private set; }

            protected override void Write(LogEventInfo logEvent)
            {
                string msg = this.Layout.Render(logEvent);
                if (msg.Length > 100)
                    msg = msg.Substring(0, 100) + "...";

                this.Logs.Enqueue(msg);
                while (this.Logs.Count > maxSize)
                {
                    Logs.Dequeue();
                }
            }
        }

        /// <summary>
        /// class for <see cref="ThreadSafe_getCurrentClassLogger_test"/>
        /// </summary>
        public class ClassA
        {
            private static Logger logger = LogManager.GetCurrentClassLogger();
            public ClassA(ClassB dd)
            {
                logger.Info("Hi there A");

            }
        }
        /// <summary>
        /// class for <see cref="ThreadSafe_getCurrentClassLogger_test"/>
        /// </summary>
        public class ClassB
        {
            private static Logger logger = LogManager.GetCurrentClassLogger();
            public ClassB(ClassC dd)
            {
                logger.Info("Hi there B");
            }
        }
        /// <summary>
        /// class for <see cref="ThreadSafe_getCurrentClassLogger_test"/>
        /// </summary>
        public class ClassC
        {
            private static Logger logger = LogManager.GetCurrentClassLogger();
            public ClassC(ClassD dd)
            {
                logger.Info("Hi there C");

            }
        }
        /// <summary>
        /// class for <see cref="ThreadSafe_getCurrentClassLogger_test"/>
        /// </summary>
        public class ClassD
        {
            private static Logger logger = LogManager.GetCurrentClassLogger();
            public ClassD()
            {
                logger.Info("Hi there D");
            }
        }
#endif
    }
}
