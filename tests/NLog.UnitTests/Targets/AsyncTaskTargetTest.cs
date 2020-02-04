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

namespace NLog.UnitTests.Targets
{
#if !NET3_5
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using NLog.Config;
    using NLog.Targets;
    using Xunit;

    public class AsyncTaskTargetTest : NLogTestBase
    {
        [Target("AsyncTaskTest")]
        class AsyncTaskTestTarget : AsyncTaskTarget
        {
            private readonly AutoResetEvent _writeEvent = new AutoResetEvent(false);
            internal readonly ConcurrentQueue<string> Logs = new ConcurrentQueue<string>();
            internal int WriteTasks => _writeTasks;
            protected int _writeTasks;

            public bool WaitForWriteEvent(int timeoutMilliseconds = 1000)
            {
                if (_writeEvent.WaitOne(TimeSpan.FromMilliseconds(timeoutMilliseconds)))
                {
                    Thread.Sleep(25);
                    return true;
                }
                return false;
            }

            protected override Task WriteAsyncTask(LogEventInfo logEvent, CancellationToken token)
            {
                Interlocked.Increment(ref _writeTasks);
                return WriteLogQueue(logEvent, token);
            }

            protected async Task WriteLogQueue(LogEventInfo logEvent, CancellationToken token)
            {
                if (logEvent.Message == "EXCEPTION")
                    throw new InvalidOperationException("AsyncTaskTargetTest Failure");
                else if (logEvent.Message == "ASYNCEXCEPTION")
                    await Task.Delay(10, token).ContinueWith((t) => { throw new InvalidOperationException("AsyncTaskTargetTest Async Failure"); }).ConfigureAwait(false);
                else if (logEvent.Message == "TIMEOUT")
                    await Task.Delay(15000, token).ConfigureAwait(false);
                else
                {
                    if (logEvent.Message == "SLEEP")
                        Task.Delay(5000, token).GetAwaiter().GetResult();
                    await Task.Delay(10, token).ContinueWith((t) => Logs.Enqueue(RenderLogEvent(Layout, logEvent)), token).ContinueWith(async (t) => await Task.Delay(10).ConfigureAwait(false)).ConfigureAwait(false);
                }
                _writeEvent.Set();
            }
        }

        class AsyncTaskBatchTestTarget : AsyncTaskTestTarget
        {
            protected override async Task WriteAsyncTask(IList<LogEventInfo> logEvents, CancellationToken cancellationToken)
            {
                Interlocked.Increment(ref _writeTasks);
                for (int i = 0; i < logEvents.Count; ++i)
                    await WriteLogQueue(logEvents[i], cancellationToken);
            }

            protected override Task WriteAsyncTask(LogEventInfo logEvent, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }
        }

        [Fact]
        public void AsyncTaskTarget_TestLogging()
        {
            ILogger logger = LogManager.GetCurrentClassLogger();

            var asyncTarget = new AsyncTaskTestTarget { Layout = "${threadid}|${level}|${message}|${mdlc:item=Test}" };

            SimpleConfigurator.ConfigureForTargetLogging(asyncTarget, LogLevel.Trace);
            NLog.Common.InternalLogger.LogLevel = LogLevel.Off;

            int managedThreadId = 0;
            Task task;
            using (MappedDiagnosticsLogicalContext.SetScoped("Test", 42))
            {
                task = Task.Run(() =>
                {
                    managedThreadId = Thread.CurrentThread.ManagedThreadId;
                    logger.Trace("TTT");
                    logger.Debug("DDD");
                    logger.Info("III");
                    logger.Warn("WWW");
                    logger.Error("EEE");
                    logger.Fatal("FFF");
                });
            }
            Assert.True(asyncTarget.WaitForWriteEvent());
            Assert.NotEmpty(asyncTarget.Logs);
            task.Wait();
            LogManager.Flush();
            Assert.Equal(6, asyncTarget.Logs.Count);
            while (asyncTarget.Logs.TryDequeue(out var logEventMessage))
            {
                Assert.Equal(0, logEventMessage.IndexOf(managedThreadId.ToString() + "|"));
                Assert.EndsWith("|42", logEventMessage);
            }

            LogManager.Configuration = null;
        }

        [Fact]
        public void AsyncTaskTarget_SkipAsyncTargetWrapper()
        {
            try
            {
                ConfigurationItemFactory.Default.RegisterType(typeof(AsyncTaskTestTarget), null);
                LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
            <nlog throwExceptions='true'>
            <targets async='true'>
                <target name='asyncDebug' type='AsyncTaskTest' />
                <target name='debug' type='Debug' />
            </targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>");

                Assert.NotNull(LogManager.Configuration.FindTargetByName<AsyncTaskTestTarget>("asyncDebug"));
                Assert.NotNull(LogManager.Configuration.FindTargetByName<NLog.Targets.Wrappers.AsyncTargetWrapper>("debug"));
            }
            finally
            {
                ConfigurationItemFactory.Default = null;
            }
        }

        [Fact]
        public void AsyncTaskTarget_SkipDefaultAsyncWrapper()
        {
            try
            {
                ConfigurationItemFactory.Default.RegisterType(typeof(AsyncTaskTestTarget), null);
                LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
            <nlog throwExceptions='true'>
            <targets>
                <default-wrapper type='AsyncWrapper' />
                <target name='asyncDebug' type='AsyncTaskTest' />
                <target name='debug' type='Debug' />
            </targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>");

                Assert.NotNull(LogManager.Configuration.FindTargetByName<AsyncTaskTestTarget>("asyncDebug"));
                Assert.NotNull(LogManager.Configuration.FindTargetByName<NLog.Targets.Wrappers.AsyncTargetWrapper>("debug"));
            }
            finally
            {
                ConfigurationItemFactory.Default = null;
            }
        }

        [Fact]
        public void AsyncTaskTarget_AllowDefaultBufferWrapper()
        {
            try
            {
                ConfigurationItemFactory.Default.RegisterType(typeof(AsyncTaskTestTarget), null);
                LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
            <nlog throwExceptions='true'>
            <targets>
                <default-wrapper type='BufferingWrapper' />
                <target name='asyncDebug' type='AsyncTaskTest' />
                <target name='debug' type='Debug' />
            </targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>");

                Assert.NotNull(LogManager.Configuration.FindTargetByName<NLog.Targets.Wrappers.BufferingTargetWrapper>("asyncDebug"));
                Assert.NotNull(LogManager.Configuration.FindTargetByName<NLog.Targets.Wrappers.BufferingTargetWrapper>("debug"));
            }
            finally
            {
                ConfigurationItemFactory.Default = null;
            }
        }

        [Fact]
        public void AsyncTaskTarget_TestAsyncException()
        {
            ILogger logger = LogManager.GetCurrentClassLogger();

            var asyncTarget = new AsyncTaskTestTarget
            {
                Layout = "${level}",
                RetryDelayMilliseconds = 50
            };

            SimpleConfigurator.ConfigureForTargetLogging(asyncTarget, LogLevel.Trace);

            foreach (var logLevel in LogLevel.AllLoggingLevels)
                logger.Log(logLevel, logLevel == LogLevel.Debug ? "ASYNCEXCEPTION" : logLevel.Name.ToUpperInvariant());
            Assert.True(asyncTarget.WaitForWriteEvent());
            Assert.NotEmpty(asyncTarget.Logs);
            LogManager.Flush();
            Assert.Equal(LogLevel.MaxLevel.Ordinal, asyncTarget.Logs.Count);

            int ordinal = 0;
            while (asyncTarget.Logs.TryDequeue(out var logEventMessage))
            {
                var logLevel = LogLevel.FromString(logEventMessage);
                Assert.NotEqual(LogLevel.Debug, logLevel);
                Assert.Equal(ordinal++, logLevel.Ordinal);
                if (ordinal == LogLevel.Debug.Ordinal)
                    ++ordinal;
            }

            LogManager.Configuration = null;
        }

        [Fact]
        public void AsyncTaskTarget_TestTimeout()
        {
            RetryingIntegrationTest(3, () =>
            {
                ILogger logger = LogManager.GetCurrentClassLogger();

                var asyncTarget = new AsyncTaskTestTarget
                {
                    Layout = "${level}",
                    TaskTimeoutSeconds = 1
                };

                SimpleConfigurator.ConfigureForTargetLogging(asyncTarget, LogLevel.Trace);

                logger.Trace("TTT");
                logger.Debug("TIMEOUT");
                logger.Info("III");
                logger.Warn("WWW");
                logger.Error("EEE");
                logger.Fatal("FFF");
                Assert.True(asyncTarget.WaitForWriteEvent());
                Assert.NotEmpty(asyncTarget.Logs);
                LogManager.Flush();
                Assert.Equal(5, asyncTarget.Logs.Count);
                while (asyncTarget.Logs.TryDequeue(out var logEventMessage))
                {
                    Assert.Equal(-1, logEventMessage.IndexOf("Debug|"));
                }

                LogManager.Configuration = null;
            });
        }

        [Fact]
        public void AsyncTaskTarget_TestRetryAsyncException()
        {
            ILogger logger = LogManager.GetCurrentClassLogger();

            var asyncTarget = new AsyncTaskTestTarget
            {
                Layout = "${level}",
                RetryDelayMilliseconds = 10,
                RetryCount = 3
            };

            SimpleConfigurator.ConfigureForTargetLogging(asyncTarget, LogLevel.Trace);

            foreach (var logLevel in LogLevel.AllLoggingLevels)
                logger.Log(logLevel, logLevel == LogLevel.Debug ? "ASYNCEXCEPTION" : logLevel.Name.ToUpperInvariant());
            Assert.True(asyncTarget.WaitForWriteEvent());
            Assert.NotEmpty(asyncTarget.Logs);
            LogManager.Flush();
            Assert.Equal(LogLevel.MaxLevel.Ordinal, asyncTarget.Logs.Count);
            Assert.Equal(LogLevel.MaxLevel.Ordinal + 4, asyncTarget.WriteTasks);

            int ordinal = 0;
            while (asyncTarget.Logs.TryDequeue(out var logEventMessage))
            {
                var logLevel = LogLevel.FromString(logEventMessage);
                Assert.NotEqual(LogLevel.Debug, logLevel);
                Assert.Equal(ordinal++, logLevel.Ordinal);
                if (ordinal == LogLevel.Debug.Ordinal)
                    ++ordinal;
            }

            LogManager.Configuration = null;
        }

        [Fact]
        public void AsyncTaskTarget_TestRetryException()
        {
            ILogger logger = LogManager.GetCurrentClassLogger();

            var asyncTarget = new AsyncTaskTestTarget
            {
                Layout = "${level}",
                RetryDelayMilliseconds = 10,
                RetryCount = 3
            };

            SimpleConfigurator.ConfigureForTargetLogging(asyncTarget, LogLevel.Trace);

            foreach (var logLevel in LogLevel.AllLoggingLevels)
                logger.Log(logLevel, logLevel == LogLevel.Debug ? "EXCEPTION" : logLevel.Name.ToUpperInvariant());
            Assert.True(asyncTarget.WaitForWriteEvent());
            Assert.NotEmpty(asyncTarget.Logs);
            LogManager.Flush();
            Assert.Equal(LogLevel.MaxLevel.Ordinal, asyncTarget.Logs.Count);
            Assert.Equal(LogLevel.MaxLevel.Ordinal + 4, asyncTarget.WriteTasks);

            int ordinal = 0;
            while (asyncTarget.Logs.TryDequeue(out var logEventMessage))
            {
                var logLevel = LogLevel.FromString(logEventMessage);
                Assert.NotEqual(LogLevel.Debug, logLevel);
                Assert.Equal(ordinal++, logLevel.Ordinal);
                if (ordinal == LogLevel.Debug.Ordinal)
                    ++ordinal;
            }

            LogManager.Configuration = null;
        }

        [Fact]
        public void AsyncTaskTarget_TestBatchWriting()
        {
            ILogger logger = LogManager.GetCurrentClassLogger();

            var asyncTarget = new AsyncTaskBatchTestTarget
            {
                Layout = "${level}",
                BatchSize = 3,
                TaskDelayMilliseconds = 10
            };

            SimpleConfigurator.ConfigureForTargetLogging(asyncTarget, LogLevel.Trace);

            foreach (var logLevel in LogLevel.AllLoggingLevels)
                logger.Log(logLevel, logLevel.Name.ToUpperInvariant());
            Assert.True(asyncTarget.WaitForWriteEvent());
            Assert.NotEmpty(asyncTarget.Logs);
            LogManager.Flush();
            Assert.Equal(LogLevel.MaxLevel.Ordinal + 1, asyncTarget.Logs.Count);
            Assert.Equal(LogLevel.MaxLevel.Ordinal / 2, asyncTarget.WriteTasks);

            int ordinal = 0;
            while (asyncTarget.Logs.TryDequeue(out var logEventMessage))
            {
                var logLevel = LogLevel.FromString(logEventMessage);
                Assert.Equal(ordinal++, logLevel.Ordinal);
            }

            LogManager.Configuration = null;
        }

        [Fact]
        public void AsyncTaskTarget_TestFakeBatchWriting()
        {
            ILogger logger = LogManager.GetCurrentClassLogger();

            var asyncTarget = new AsyncTaskTestTarget
            {
                Layout = "${level}",
                BatchSize = 3,
                TaskDelayMilliseconds = 10
            };

            SimpleConfigurator.ConfigureForTargetLogging(asyncTarget, LogLevel.Trace);

            foreach (var logLevel in LogLevel.AllLoggingLevels)
                logger.Log(logLevel, logLevel.Name.ToUpperInvariant());

            Assert.True(asyncTarget.WaitForWriteEvent());
            Assert.NotEmpty(asyncTarget.Logs);
            LogManager.Flush();
            Assert.Equal(LogLevel.MaxLevel.Ordinal + 1, asyncTarget.Logs.Count);
            Assert.Equal(LogLevel.MaxLevel.Ordinal + 1, asyncTarget.WriteTasks);

            int ordinal = 0;
            while (asyncTarget.Logs.TryDequeue(out var logEventMessage))
            {
                var logLevel = LogLevel.FromString(logEventMessage);
                Assert.Equal(ordinal++, logLevel.Ordinal);
            }

            LogManager.Configuration = null;
        }

        [Fact]
        public void AsyncTaskTarget_TestSlowBatchWriting()
        {
            ILogger logger = LogManager.GetCurrentClassLogger();

            var asyncTarget = new AsyncTaskBatchTestTarget
            {
                Layout = "${level}",
                TaskDelayMilliseconds = 200
            };

            SimpleConfigurator.ConfigureForTargetLogging(asyncTarget, LogLevel.Trace);

            DateTime utcNow = DateTime.UtcNow;

            logger.Log(LogLevel.Info, LogLevel.Info.ToString().ToUpperInvariant());
            logger.Log(LogLevel.Fatal, "SLEEP");
            Assert.True(asyncTarget.WaitForWriteEvent());
            Assert.Single(asyncTarget.Logs);
            logger.Log(LogLevel.Error, LogLevel.Error.ToString().ToUpperInvariant());

            asyncTarget.Dispose();  // Trigger fast shutdown
            LogManager.Configuration = null;

            TimeSpan shutdownTime = DateTime.UtcNow - utcNow;
            Assert.True(shutdownTime < TimeSpan.FromSeconds(4), $"Shutdown took {shutdownTime.TotalMilliseconds} msec");
        }

        [Fact]
        public void AsyncTaskTarget_TestThrottleOnTaskDelay()
        {
            ILogger logger = LogManager.GetCurrentClassLogger();

            var asyncTarget = new AsyncTaskBatchTestTarget
            {
                Layout = "${level}",
                TaskDelayMilliseconds = 50,
                BatchSize = 10,
            };

            SimpleConfigurator.ConfigureForTargetLogging(asyncTarget, LogLevel.Trace);

            for (int i = 0; i < 5; ++i)
            {
                for (int j = 0; j < 10; ++j)
                {
                    logger.Log(LogLevel.Info, i.ToString());
                    Thread.Sleep(20);
                }
                Assert.True(asyncTarget.WaitForWriteEvent());
            }

            Assert.True(asyncTarget.Logs.Count > 25, $"{asyncTarget.Logs.Count} LogEvents are too few after {asyncTarget.WriteTasks} writes");
            Assert.True(asyncTarget.WriteTasks < 20, $"{asyncTarget.WriteTasks} writes are too many.");
        }

        [Fact]
        public void AsynTaskTarget_AutoFlushWrapper()
        {
            ILogger logger = LogManager.GetCurrentClassLogger();

            var asyncTarget = new AsyncTaskBatchTestTarget
            {
                Layout = "${level}",
                TaskDelayMilliseconds = 5000,
                BatchSize = 10,
            };
            var autoFlush = new NLog.Targets.Wrappers.AutoFlushTargetWrapper("autoflush", asyncTarget);
            autoFlush.Condition =  "level > LogLevel.Warn";

            SimpleConfigurator.ConfigureForTargetLogging(autoFlush, LogLevel.Trace);

            logger.Info("Hello World");
            Assert.Empty(asyncTarget.Logs);
            logger.Error("Goodbye World");
            Assert.True(asyncTarget.WaitForWriteEvent());
            Assert.NotEmpty(asyncTarget.Logs);
        }
    }
#endif
}