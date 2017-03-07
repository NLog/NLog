// 
// Copyright (c) 2004-2016 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using NLog.Config;
using NLog.Layouts;
using NLog.Targets;

namespace NLog.UnitTests.Targets
{
#if !NET3_5 && !NET4_0
    public class AsyncTaskTargetTest : NLogTestBase
    {
        class AsyncTaskTestTarget : AsyncTaskTarget
        {
            public Layout Layout { get; set; }

            internal Queue<string> Logs = new Queue<string>();

            protected override Task WriteAsyncTask(LogEventInfo logEvent, CancellationToken token)
            {
                return WriteLogQueue(logEvent, token);
            }

            private async Task WriteLogQueue(LogEventInfo logEvent, CancellationToken token)
            {
                if (logEvent.Message == "EXCEPTION")
                    await Task.Delay(10, token).ContinueWith((t) => { throw new InvalidOperationException("AsyncTaskTargetTest Failed"); }).ConfigureAwait(false);
                else if (logEvent.Message == "TIMEOUT")
                    await Task.Delay(15000, token).ConfigureAwait(false);
                else
                    await Task.Delay(10, token).ContinueWith((t) => Logs.Enqueue(RenderLogEvent(Layout, logEvent)), token).ContinueWith(async (t) => await Task.Delay(10).ConfigureAwait(false)).ConfigureAwait(false);
            }
        }

        [Fact]
        public void AsyncTaskTarget_TestLogging()
        {
            ILogger logger = LogManager.GetCurrentClassLogger();

            var asyncTarget = new AsyncTaskTestTarget();
            asyncTarget.Layout = "${threadid}|${level}|${message}";

            SimpleConfigurator.ConfigureForTargetLogging(asyncTarget, LogLevel.Trace);
            Assert.True(asyncTarget.Logs.Count == 0);
            logger.Trace("TTT");
            logger.Debug("DDD");
            logger.Info("III");
            logger.Warn("WWW");
            logger.Error("EEE");
            logger.Fatal("FFF");
            System.Threading.Thread.Sleep(50);
            Assert.True(asyncTarget.Logs.Count != 0);
            LogManager.Flush();
            Assert.True(asyncTarget.Logs.Count == 6);
            while (asyncTarget.Logs.Count > 0)
            {
                string logEventMessage = asyncTarget.Logs.Dequeue();
                Assert.Equal(0, logEventMessage.IndexOf(System.Threading.Thread.CurrentThread.ManagedThreadId.ToString() + "|"));
            }

            LogManager.Configuration = null;
        }

        [Fact]
        public void AsyncTaskTarget_TestException()
        {
            ILogger logger = LogManager.GetCurrentClassLogger();

            var asyncTarget = new AsyncTaskTestTarget();
            asyncTarget.Layout = "${threadid}|${level}|${message}";

            SimpleConfigurator.ConfigureForTargetLogging(asyncTarget, LogLevel.Trace);
            Assert.True(asyncTarget.Logs.Count == 0);
            logger.Trace("TTT");
            logger.Debug("EXCEPTION");
            logger.Info("III");
            logger.Warn("WWW");
            logger.Error("EEE");
            logger.Fatal("FFF");
            System.Threading.Thread.Sleep(50);
            Assert.True(asyncTarget.Logs.Count != 0);
            LogManager.Flush();
            Assert.True(asyncTarget.Logs.Count == 5);
            while (asyncTarget.Logs.Count > 0)
            {
                string logEventMessage = asyncTarget.Logs.Dequeue();
                Assert.Equal(-1, logEventMessage.IndexOf("|Debug|"));
                Assert.Equal(0, logEventMessage.IndexOf(System.Threading.Thread.CurrentThread.ManagedThreadId.ToString() + "|"));
            }

            LogManager.Configuration = null;
        }

        [Fact]
        public void AsyncTaskTarget_TestTimeout()
        {
            ILogger logger = LogManager.GetCurrentClassLogger();

            var asyncTarget = new AsyncTaskTestTarget();
            asyncTarget.Layout = "${threadid}|${level}|${message}";
            asyncTarget.TaskTimeoutSeconds = 1;

            SimpleConfigurator.ConfigureForTargetLogging(asyncTarget, LogLevel.Trace);
            Assert.True(asyncTarget.Logs.Count == 0);
            logger.Trace("TTT");
            logger.Debug("TIMEOUT");
            logger.Info("III");
            logger.Warn("WWW");
            logger.Error("EEE");
            logger.Fatal("FFF");
            System.Threading.Thread.Sleep(50);
            Assert.True(asyncTarget.Logs.Count != 0);
            LogManager.Flush();
            Assert.True(asyncTarget.Logs.Count == 5);
            while (asyncTarget.Logs.Count > 0)
            {
                string logEventMessage = asyncTarget.Logs.Dequeue();
                Assert.Equal(-1, logEventMessage.IndexOf("|Debug|"));
                Assert.Equal(0, logEventMessage.IndexOf(System.Threading.Thread.CurrentThread.ManagedThreadId.ToString() + "|"));
            }

            LogManager.Configuration = null;
        }
    }
#endif
}