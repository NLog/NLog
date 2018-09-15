// 
// Copyright (c) 2004-2018 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Xunit;
    using NLog.Config;
    using NLog.Targets;

    public class AsyncTaskTargetTest : NLogTestBase
    {
        class AsyncTaskTestTarget : AsyncTaskTarget
        {
            internal Queue<string> Logs = new Queue<string>();
            internal int WriteTasks => _writeTasks;
            protected int _writeTasks;

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
                    await Task.Delay(10, token).ContinueWith((t) => Logs.Enqueue(RenderLogEvent(Layout, logEvent)), token).ContinueWith(async (t) => await Task.Delay(10).ConfigureAwait(false)).ConfigureAwait(false);
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

            var asyncTarget = new AsyncTaskTestTarget();
            asyncTarget.Layout = "${threadid}|${level}|${message}";

            SimpleConfigurator.ConfigureForTargetLogging(asyncTarget, LogLevel.Trace);
            NLog.Common.InternalLogger.LogLevel = LogLevel.Off;

            Assert.True(asyncTarget.Logs.Count == 0);
            logger.Trace("TTT");
            logger.Debug("DDD");
            logger.Info("III");
            logger.Warn("WWW");
            logger.Error("EEE");
            logger.Fatal("FFF");
            Thread.Sleep(75);
            Assert.True(asyncTarget.Logs.Count != 0);
            LogManager.Flush();
            Assert.True(asyncTarget.Logs.Count == 6);
            while (asyncTarget.Logs.Count > 0)
            {
                string logEventMessage = asyncTarget.Logs.Dequeue();
                Assert.Equal(0, logEventMessage.IndexOf(Thread.CurrentThread.ManagedThreadId.ToString() + "|"));
            }

            LogManager.Configuration = null;
        }

        [Fact]
        public void AsyncTaskTarget_TestAsyncException()
        {
            ILogger logger = LogManager.GetCurrentClassLogger();

            var asyncTarget = new AsyncTaskTestTarget();
            asyncTarget.Layout = "${level}";
            asyncTarget.RetryDelayMilliseconds = 50;

            SimpleConfigurator.ConfigureForTargetLogging(asyncTarget, LogLevel.Trace);
            Assert.True(asyncTarget.Logs.Count == 0);

            foreach (var logLevel in LogLevel.AllLoggingLevels)
                logger.Log(logLevel, logLevel == LogLevel.Debug ? "ASYNCEXCEPTION" : logLevel.Name.ToUpperInvariant());
            Thread.Sleep(75);
            Assert.True(asyncTarget.Logs.Count != 0);
            LogManager.Flush();
            Assert.Equal(LogLevel.MaxLevel.Ordinal, asyncTarget.Logs.Count);

            int ordinal = 0;
            while (asyncTarget.Logs.Count > 0)
            {
                string logEventMessage = asyncTarget.Logs.Dequeue();
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
            ILogger logger = LogManager.GetCurrentClassLogger();

            var asyncTarget = new AsyncTaskTestTarget();
            asyncTarget.Layout = "${level}";
            asyncTarget.TaskTimeoutSeconds = 1;

            SimpleConfigurator.ConfigureForTargetLogging(asyncTarget, LogLevel.Trace);
            Assert.True(asyncTarget.Logs.Count == 0);
            logger.Trace("TTT");
            logger.Debug("TIMEOUT");
            logger.Info("III");
            logger.Warn("WWW");
            logger.Error("EEE");
            logger.Fatal("FFF");
            Thread.Sleep(75);
            Assert.True(asyncTarget.Logs.Count != 0);
            LogManager.Flush();
            Assert.True(asyncTarget.Logs.Count == 5);
            while (asyncTarget.Logs.Count > 0)
            {
                string logEventMessage = asyncTarget.Logs.Dequeue();
                Assert.Equal(-1, logEventMessage.IndexOf("Debug|"));
            }

            LogManager.Configuration = null;
        }

        [Fact]
        public void AsyncTaskTarget_TestRetryAsyncException()
        {
            ILogger logger = LogManager.GetCurrentClassLogger();

            var asyncTarget = new AsyncTaskTestTarget();
            asyncTarget.Layout = "${level}";
            asyncTarget.RetryDelayMilliseconds = 10;
            asyncTarget.RetryCount = 3;

            SimpleConfigurator.ConfigureForTargetLogging(asyncTarget, LogLevel.Trace);
            Assert.True(asyncTarget.Logs.Count == 0);

            foreach (var logLevel in LogLevel.AllLoggingLevels)
                logger.Log(logLevel, logLevel == LogLevel.Debug ? "ASYNCEXCEPTION" : logLevel.Name.ToUpperInvariant());
            Thread.Sleep(75);
            Assert.True(asyncTarget.Logs.Count != 0);
            LogManager.Flush();
            Assert.Equal(LogLevel.MaxLevel.Ordinal, asyncTarget.Logs.Count);
            Assert.Equal(LogLevel.MaxLevel.Ordinal + 4, asyncTarget.WriteTasks);

            int ordinal = 0;
            while (asyncTarget.Logs.Count > 0)
            {
                string logEventMessage = asyncTarget.Logs.Dequeue();
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

            var asyncTarget = new AsyncTaskTestTarget();
            asyncTarget.Layout = "${level}";
            asyncTarget.RetryDelayMilliseconds = 10;
            asyncTarget.RetryCount = 3;

            SimpleConfigurator.ConfigureForTargetLogging(asyncTarget, LogLevel.Trace);
            Assert.True(asyncTarget.Logs.Count == 0);

            foreach (var logLevel in LogLevel.AllLoggingLevels)
                logger.Log(logLevel, logLevel == LogLevel.Debug ? "EXCEPTION" : logLevel.Name.ToUpperInvariant());
            Thread.Sleep(75);
            Assert.True(asyncTarget.Logs.Count != 0);
            LogManager.Flush();
            Assert.Equal(LogLevel.MaxLevel.Ordinal, asyncTarget.Logs.Count);
            Assert.Equal(LogLevel.MaxLevel.Ordinal + 4, asyncTarget.WriteTasks);

            int ordinal = 0;
            while (asyncTarget.Logs.Count > 0)
            {
                string logEventMessage = asyncTarget.Logs.Dequeue();
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

            var asyncTarget = new AsyncTaskBatchTestTarget();
            asyncTarget.Layout = "${level}";
            asyncTarget.BatchSize = 3;
            asyncTarget.TaskDelayMilliseconds = 10;

            SimpleConfigurator.ConfigureForTargetLogging(asyncTarget, LogLevel.Trace);
            Assert.True(asyncTarget.Logs.Count == 0);

            foreach (var logLevel in LogLevel.AllLoggingLevels)
                logger.Log(logLevel, logLevel.Name.ToUpperInvariant());
            Thread.Sleep(75);
            Assert.True(asyncTarget.Logs.Count != 0);
            LogManager.Flush();
            Assert.Equal(LogLevel.MaxLevel.Ordinal + 1, asyncTarget.Logs.Count);
            Assert.Equal(LogLevel.MaxLevel.Ordinal / 2, asyncTarget.WriteTasks);

            int ordinal = 0;
            while (asyncTarget.Logs.Count > 0)
            {
                string logEventMessage = asyncTarget.Logs.Dequeue();
                var logLevel = LogLevel.FromString(logEventMessage);
                Assert.Equal(ordinal++, logLevel.Ordinal);
            }

            LogManager.Configuration = null;
        }

        [Fact]
        public void AsyncTaskTarget_TestFakeBatchWriting()
        {
            ILogger logger = LogManager.GetCurrentClassLogger();

            var asyncTarget = new AsyncTaskTestTarget();
            asyncTarget.Layout = "${level}";
            asyncTarget.BatchSize = 3;
            asyncTarget.TaskDelayMilliseconds = 10;

            SimpleConfigurator.ConfigureForTargetLogging(asyncTarget, LogLevel.Trace);
            Assert.True(asyncTarget.Logs.Count == 0);

            foreach (var logLevel in LogLevel.AllLoggingLevels)
                logger.Log(logLevel, logLevel.Name.ToUpperInvariant());
            Thread.Sleep(75);
            Assert.True(asyncTarget.Logs.Count != 0);
            LogManager.Flush();
            Assert.Equal(LogLevel.MaxLevel.Ordinal + 1, asyncTarget.Logs.Count);
            Assert.Equal(LogLevel.MaxLevel.Ordinal + 1, asyncTarget.WriteTasks);

            int ordinal = 0;
            while (asyncTarget.Logs.Count > 0)
            {
                string logEventMessage = asyncTarget.Logs.Dequeue();
                var logLevel = LogLevel.FromString(logEventMessage);
                Assert.Equal(ordinal++, logLevel.Ordinal);
            }

            LogManager.Configuration = null;
        }
    }
#endif
}