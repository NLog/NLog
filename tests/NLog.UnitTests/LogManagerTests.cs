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

using JetBrains.Annotations;

namespace NLog.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Xunit;
    using NLog.Common;
    using NLog.Config;
    using NLog.Targets;
    using System.Threading.Tasks;

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
            GC.Collect(2, GCCollectionMode.Forced, true);
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

            using (new NoThrowNLogExceptions())
            {
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
            }
        }

        [Fact(Skip="Side effects to other unit tests.")]
        public void GlobalThresholdTest()
        {
            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
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
            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(LoggerConfig);

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
            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(LoggerConfig);

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

        private int _reloadCounter;

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

        [Fact]
        public void AutoReloadTest()
        {
#if NETSTANDARD || MONO
            if (IsLinux())
            {
                Console.WriteLine("[SKIP] LogManagerTests.AutoReloadTest because we are running in Travis");
                return;
            }
#endif

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

        [Fact]
        public void GivenCurrentClass_WhenGetCurrentClassLogger_ThenLoggerShouldBeCurrentClass()
        {
            var logger = LogManager.GetCurrentClassLogger();

            Assert.Equal(GetType().FullName, logger.Name);
        }

        private static class ImAStaticClass
        {
            [UsedImplicitly]
            public static readonly Logger Logger = LogManager.GetCurrentClassLogger();

            static ImAStaticClass() { }

            public static void DummyToInvokeInitializers() { }

        }

        [Fact]
        public void GetCurrentClassLogger_static_class()
        {
            ImAStaticClass.DummyToInvokeInitializers();
            Assert.Equal(typeof(ImAStaticClass).FullName, ImAStaticClass.Logger.Name);
        }

        private abstract class ImAAbstractClass
        {
            public Logger Logger { get; private set; }
            public Logger LoggerType { get; private set; }

            public string BaseName => typeof(ImAAbstractClass).FullName;

            /// <summary>
            /// Initializes a new instance of the <see cref="T:System.Object"/> class.
            /// </summary>
            protected ImAAbstractClass()
            {
                Logger = LogManager.GetCurrentClassLogger();
                LoggerType = LogManager.GetCurrentClassLogger(typeof(Logger));
            }

            protected ImAAbstractClass(string param1, Func<string> param2)
            {
                Logger = LogManager.GetCurrentClassLogger();
                LoggerType = LogManager.GetCurrentClassLogger(typeof(Logger));
            }
        }

        private class InheritedFromAbstractClass : ImAAbstractClass
        {
            public Logger LoggerInherited = LogManager.GetCurrentClassLogger();
            public Logger LoggerTypeInherited = LogManager.GetCurrentClassLogger(typeof(Logger));

            public string InheritedName => GetType().FullName;

            public InheritedFromAbstractClass()
                : base()
            {
            }

            public InheritedFromAbstractClass(string param1, Func<string> param2)
                : base(param1, param2)
            {
            }
        }

        /// <summary>
        /// Creating instance in a abstract ctor should not be a problem
        /// </summary>
        [Fact]
        public void GetCurrentClassLogger_abstract_class()
        {
            var instance = new InheritedFromAbstractClass();
            Assert.Equal(instance.BaseName, instance.Logger.Name);
            Assert.Equal(instance.BaseName, instance.LoggerType.Name);
            Assert.Equal(instance.InheritedName, instance.LoggerInherited.Name);
            Assert.Equal(instance.InheritedName, instance.LoggerTypeInherited.Name);
        }

        /// <summary>
        /// Creating instance in a abstract ctor should not be a problem
        /// </summary>
        [Fact]
        public void GetCurrentClassLogger_abstract_class_with_parameter()
        {
            var instance = new InheritedFromAbstractClass("Hello", null);
            Assert.Equal(instance.BaseName, instance.Logger.Name);
            Assert.Equal(instance.BaseName, instance.LoggerType.Name);
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
        public void GetLogger_wrong_loggertype_should_continue()
        {
            using (new NoThrowNLogExceptions())
            {
                var instance = LogManager.GetLogger("a", typeof(ImNotALogger));
                Assert.NotNull(instance);
            }
        }

        /// <summary>
        /// ImNotALogger inherits not from Logger , but should not throw an exception
        /// </summary>
        [Fact]
        public void GetLogger_wrong_loggertype_should_continue_even_if_class_is_static()
        {
            using (new NoThrowNLogExceptions())
            {
                var instance = LogManager.GetLogger("a", typeof(ImAStaticClass));
                Assert.NotNull(instance);
            }
        }

        [Fact]
        public void GivenLazyClass_WhenGetCurrentClassLogger_ThenLoggerNameShouldBeCurrentClass()
        {
            var logger = new Lazy<ILogger>(LogManager.GetCurrentClassLogger);

            Assert.Equal(GetType().FullName, logger.Value.Name);
        }

        [Fact]
        public void ThreadSafe_Shutdown()
        {
            LogManager.Configuration = new LoggingConfiguration();
            LogManager.ThrowExceptions = true;
            LogManager.Configuration.AddTarget("memory", new NLog.Targets.Wrappers.BufferingTargetWrapper(new MemoryTarget() { MaxLogsCount = 500 }, 5, 1));
            LogManager.Configuration.LoggingRules.Add(new LoggingRule("*", LogLevel.Debug, LogManager.Configuration.FindTargetByName("memory")));
            LogManager.Configuration.AddTarget("memory2", new NLog.Targets.Wrappers.BufferingTargetWrapper(new MemoryTarget() { MaxLogsCount = 500 }, 5, 1));
            LogManager.Configuration.LoggingRules.Add(new LoggingRule("*", LogLevel.Debug, LogManager.Configuration.FindTargetByName("memory2")));
            var stopFlag = false;
            var exceptionThrown = false;
            Task.Run(() => { try { var logger = LogManager.GetLogger("Hello"); while (!stopFlag) { logger.Debug("Hello World"); System.Threading.Thread.Sleep(1); } } catch { exceptionThrown = true; } });
            Task.Run(() => { try { var logger = LogManager.GetLogger("Hello"); while (!stopFlag) { logger.Debug("Hello World"); System.Threading.Thread.Sleep(1); } } catch { exceptionThrown = true; } });
            System.Threading.Thread.Sleep(20);
            LogManager.Shutdown();  // Shutdown active LoggingConfiguration
            System.Threading.Thread.Sleep(20);
            stopFlag = true;
            System.Threading.Thread.Sleep(20);
            Assert.False(exceptionThrown);
        }

        /// <summary>
        /// Note: THe problem  can be reproduced when: debugging the unittest + "break when exception is thrown" checked in visual studio.
        /// 
        /// https://github.com/NLog/NLog/issues/500
        /// </summary>
        [Fact]
        public void ThreadSafe_getCurrentClassLogger_test()
        {
            MemoryTarget mTarget = new MemoryTarget() { Name = "memory", MaxLogsCount = 1000 };
            MemoryTarget mTarget2 = new MemoryTarget() { Name = "memory2", MaxLogsCount = 1000 };

            var task1 = Task.Run(() =>
            {
                //need for init
                LogManager.Configuration = new LoggingConfiguration();

                LogManager.Configuration.AddTarget(mTarget.Name, mTarget);
                LogManager.Configuration.AddRuleForAllLevels(mTarget.Name);
                System.Threading.Thread.Sleep(1);
                LogManager.ReconfigExistingLoggers();
                System.Threading.Thread.Sleep(1);
                mTarget.Layout = @"${date:format=HH\:mm\:ss}|${level:uppercase=true}|${message} ${exception:format=tostring}";
            });

            var task2 = task1.ContinueWith((t) =>
            {
                LogManager.Configuration.AddTarget(mTarget2.Name, mTarget2);
                LogManager.Configuration.AddRuleForAllLevels(mTarget2.Name);
                System.Threading.Thread.Sleep(1);
                LogManager.ReconfigExistingLoggers();
                System.Threading.Thread.Sleep(1);
                mTarget2.Layout = @"${date:format=HH\:mm\:ss}|${level:uppercase=true}|${message} ${exception:format=tostring}";
            });

            System.Threading.Thread.Sleep(1);

            Parallel.For(0, 8, new ParallelOptions() { MaxDegreeOfParallelism = 8 }, (e) =>
            {
                bool task1Complete = false, task2Complete = false;
                for (int i = 0; i < 100; ++i)
                {
                    if (i > 25 && !task1Complete)
                    {
                        task1.Wait(5000);
                        task1Complete = true;
                    }
                    if (i > 75 && !task2Complete)
                    {
                        task2.Wait(5000);
                        task2Complete = true;
                    }

                    // Multiple threads initializing new loggers while configuration is changing
                    var loggerA = LogManager.GetLogger(e + "A" + i);
                    loggerA.Info("Hi there {0}", e);
                    var loggerB = LogManager.GetLogger(e + "B" + i);
                    loggerB.Info("Hi there {0}", e);
                    var loggerC = LogManager.GetLogger(e + "C" + i);
                    loggerC.Info("Hi there {0}", e);
                    var loggerD = LogManager.GetLogger(e + "D" + i);
                    loggerD.Info("Hi there {0}", e);
                };
            });

            Assert.NotEqual(0, mTarget.Logs.Count + mTarget2.Logs.Count);
        }

        [Fact]
        public void RemovedTargetShouldNotLog()
        {
            var config = new LoggingConfiguration();
            var targetA = new MemoryTarget("TargetA") { Layout = "A | ${message}", MaxLogsCount = 1 };
            var targetB = new MemoryTarget("TargetB") { Layout = "B | ${message}", MaxLogsCount = 1 };
            config.AddRule(LogLevel.Debug, LogLevel.Fatal, targetA);
            config.AddRule(LogLevel.Debug, LogLevel.Fatal, targetB);

            LogManager.Configuration = config;

            Assert.Equal(new[] { "TargetA", "TargetB" }, LogManager.Configuration.ConfiguredNamedTargets.Select(target => target.Name));

            Assert.NotNull(LogManager.Configuration.FindTargetByName("TargetA"));
            Assert.NotNull(LogManager.Configuration.FindTargetByName("TargetB"));

            var logger = LogManager.GetCurrentClassLogger();
            logger.Info("Hello World");

            Assert.Equal("A | Hello World", targetA.Logs.LastOrDefault());
            Assert.Equal("B | Hello World", targetB.Logs.LastOrDefault());

            // Remove the first target from the configuration
            LogManager.Configuration.RemoveTarget("TargetA");

            Assert.Equal(new[] { "TargetB" }, LogManager.Configuration.ConfiguredNamedTargets.Select(target => target.Name));

            Assert.Null(LogManager.Configuration.FindTargetByName("TargetA"));
            Assert.NotNull(LogManager.Configuration.FindTargetByName("TargetB"));

            logger.Info("Goodbye World");

            Assert.Equal("A | Hello World", targetA.Logs.LastOrDefault());  // Flushed and closed
            Assert.Equal("B | Goodbye World", targetB.Logs.LastOrDefault());
        }
    }
}
