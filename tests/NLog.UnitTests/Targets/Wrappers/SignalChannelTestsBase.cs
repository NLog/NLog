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

namespace NLog.UnitTests.Targets.Wrappers
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.Threading;
    using NLog.Common;
    using NLog.Targets.Wrappers;
    using Xunit;

    /// <summary>
    /// Contract tests for the signal-driven channels (ThreadPool worker and dedicated thread):
    /// the first write into an empty queue runs the consumer after timeToSleepBetweenBatches, then it drains until empty.
    /// Assertions use generous timeouts and "at least" counts, never exact wall-clock counts.
    /// </summary>
    public abstract class SignalChannelTestsBase
    {
        protected static readonly TimeSpan WaitTimeout = TimeSpan.FromSeconds(5);

        private static readonly AsyncLocal<string> AmbientValue = new AsyncLocal<string>();

        internal abstract AsyncLogEventChannel CreateChannel(Func<ChannelConsumeResult> consume, int timeToSleepBetweenBatches);

        protected static bool WaitUntil(Func<bool> condition)
        {
            var stopwatch = Stopwatch.StartNew();
            while (!condition())
            {
                if (stopwatch.Elapsed > WaitTimeout)
                    return false;
                Thread.Sleep(10);
            }
            return true;
        }

        protected static int Read(ref int value) => Interlocked.CompareExchange(ref value, 0, 0);

        protected static AsyncLogEventInfo NewEvent() => LogEventInfo.CreateNullEvent().WithContinuation(ex => { });

        /// <summary>
        /// Creates a channel whose consumer counts runs, reads everything and reports Empty.
        /// </summary>
        private AsyncLogEventChannel CreateCountingChannel(int timeToSleepBetweenBatches, Action onRun = null)
        {
            AsyncLogEventChannel channel = null;
            channel = CreateChannel(() =>
            {
                onRun?.Invoke();
                channel.ReadAll();
                return ChannelConsumeResult.Empty;
            }, timeToSleepBetweenBatches);
            return channel;
        }

        [Fact]
        public void FirstWrite_RunsConsumerAfterDelay()
        {
            int runs = 0;
            var channel = CreateCountingChannel(300, () => Interlocked.Increment(ref runs));
            try
            {
                channel.Start();
                channel.Write(NewEvent());
                Thread.Sleep(50);
                Assert.Equal(0, Read(ref runs));
                Assert.True(WaitUntil(() => Read(ref runs) >= 1));
            }
            finally
            {
                channel.Stop(WaitTimeout);
            }
        }

        [Fact]
        public void FirstWrite_ZeroSleep_RunsConsumer()
        {
            int runs = 0;
            var channel = CreateCountingChannel(0, () => Interlocked.Increment(ref runs));
            try
            {
                channel.Start();
                channel.Write(NewEvent());
                Assert.True(WaitUntil(() => Read(ref runs) >= 1));
            }
            finally
            {
                channel.Stop(WaitTimeout);
            }
        }

        [Fact]
        public void Start_WithQueuedEvents_RunsConsumer()
        {
            int runs = 0;
            var channel = CreateCountingChannel(1, () => Interlocked.Increment(ref runs));
            try
            {
                channel.Write(NewEvent());
                Thread.Sleep(100);
                Assert.Equal(0, Read(ref runs));

                channel.Start();
                Assert.True(WaitUntil(() => Read(ref runs) >= 1));
            }
            finally
            {
                channel.Stop(WaitTimeout);
            }
        }

        [Fact]
        public void MoreQueued_RunsAgainWithoutWrite()
        {
            int runs = 0;
            AsyncLogEventChannel channel = null;
            channel = CreateChannel(() =>
            {
                if (Interlocked.Increment(ref runs) == 1)
                    return ChannelConsumeResult.MoreQueued;    // Leaves the event queued
                channel.ReadAll();
                return ChannelConsumeResult.Empty;
            }, 1);
            try
            {
                channel.Start();
                channel.Write(NewEvent());
                Assert.True(WaitUntil(() => Read(ref runs) >= 2));
            }
            finally
            {
                channel.Stop(WaitTimeout);
            }
        }

        [Fact]
        public void Empty_WaitsForNextFirstWrite()
        {
            int runs = 0;
            var channel = CreateCountingChannel(1, () => Interlocked.Increment(ref runs));
            try
            {
                channel.Start();
                channel.Write(NewEvent());
                Assert.True(WaitUntil(() => Read(ref runs) >= 1));

                Thread.Sleep(300);
                Assert.Equal(1, Read(ref runs));

                channel.Write(NewEvent());
                Assert.True(WaitUntil(() => Read(ref runs) >= 2));
            }
            finally
            {
                channel.Stop(WaitTimeout);
            }
        }

        [Fact]
        public void FirstWriteDuringRun_CausesAnotherRun()
        {
            int runs = 0;
            using (var entered = new ManualResetEvent(false))
            using (var release = new ManualResetEvent(false))
            {
                AsyncLogEventChannel channel = null;
                channel = CreateChannel(() =>
                {
                    int run = Interlocked.Increment(ref runs);
                    channel.ReadAll();
                    if (run == 1)
                    {
                        entered.Set();
                        release.WaitOne(WaitTimeout);
                    }
                    return ChannelConsumeResult.Empty;
                }, 1);
                try
                {
                    channel.Start();
                    channel.Write(NewEvent());
                    Assert.True(entered.WaitOne(WaitTimeout));

                    channel.Write(NewEvent());  // First write into the (now empty) queue while the consumer runs
                    release.Set();

                    Assert.True(WaitUntil(() => Read(ref runs) >= 2));
                }
                finally
                {
                    release.Set();
                    channel.Stop(WaitTimeout);
                }
            }
        }

        [Fact]
        public void NoProgress_RetriesWithoutBusyLoop()
        {
            int runs = 0;
            var channel = CreateChannel(() => { Interlocked.Increment(ref runs); return ChannelConsumeResult.NoProgress; }, 1);
            try
            {
                channel.Start();
                channel.Write(NewEvent());
                Thread.Sleep(500);

                int count = Read(ref runs);
                Assert.True(count >= 2, $"Runs: {count}");
                // Retry after 1 ms: at most ~500 runs in 500 ms where timers tick every 1 ms (Linux with 1000 Hz kernel), ~32 on Windows (15.6 ms).
                // A busy loop makes tens of thousands.
                Assert.True(count <= 1500, $"Runs: {count}");
            }
            finally
            {
                channel.Stop(WaitTimeout);
            }
        }

        [Fact]
        public void Stop_WhileNoProgressRetryPending_NoFurtherRuns()
        {
            int runs = 0;
            var channel = CreateChannel(() => { Interlocked.Increment(ref runs); return ChannelConsumeResult.NoProgress; }, 200);
            channel.Start();
            channel.Write(NewEvent());
            Assert.True(WaitUntil(() => Read(ref runs) >= 1));

            Assert.True(channel.Stop(WaitTimeout));
            int stoppedAt = Read(ref runs);
            Thread.Sleep(400);

            Assert.Equal(stoppedAt, Read(ref runs));
        }

        [Fact]
        public void ConsumerThrows_RunsAgain()
        {
            int runs = 0;
            AsyncLogEventChannel channel = null;
            channel = CreateChannel(() =>
            {
                if (Interlocked.Increment(ref runs) == 1)
                    throw new ApplicationException("consumer failure");   // Leaves the event queued
                channel.ReadAll();
                return ChannelConsumeResult.Empty;
            }, 1);
            try
            {
                channel.Start();
                channel.Write(NewEvent());
                Assert.True(WaitUntil(() => Read(ref runs) >= 2));
            }
            finally
            {
                channel.Stop(WaitTimeout);
            }
        }

        [Fact]
        public void NoConcurrentRuns()
        {
            int runs = 0;
            int active = 0;
            int maxActive = 0;
            var channel = CreateCountingChannel(0, () =>
            {
                Interlocked.Increment(ref runs);
                int nowActive = Interlocked.Increment(ref active);
                int seenMax;
                while (nowActive > (seenMax = Read(ref maxActive)) && Interlocked.CompareExchange(ref maxActive, nowActive, seenMax) != seenMax)
                {
                    // Retry until maxActive holds the highest value seen
                }
                Thread.Sleep(1);
                Interlocked.Decrement(ref active);
            });
            try
            {
                channel.Start();
                var producers = new Thread[4];
                for (int i = 0; i < producers.Length; ++i)
                {
                    producers[i] = new Thread(() =>
                    {
                        var stopwatch = Stopwatch.StartNew();
                        while (Read(ref runs) < 20 && stopwatch.Elapsed < WaitTimeout)
                            channel.NotifyPendingEvents();
                    });
                    producers[i].Start();
                }

                foreach (var producer in producers)
                    Assert.True(producer.Join(WaitTimeout + WaitTimeout));

                Assert.True(Read(ref runs) >= 20);
                Assert.Equal(1, Read(ref maxActive));
            }
            finally
            {
                channel.Stop(WaitTimeout);
            }
        }

        [Fact]
        public void Stop_CalledTwice_ReturnsTrue()
        {
            var channel = CreateCountingChannel(1);
            channel.Start();

            Assert.True(channel.Stop(WaitTimeout));
            Assert.True(channel.Stop(WaitTimeout));
        }

        [Fact]
        public void FirstWrite_AfterStop_DoesNotRun()
        {
            int runs = 0;
            var channel = CreateCountingChannel(1, () => Interlocked.Increment(ref runs));
            channel.Start();
            Assert.True(channel.Stop(WaitTimeout));

            channel.Write(NewEvent());
            Thread.Sleep(300);

            Assert.Equal(0, Read(ref runs));
        }

        [Fact]
        public void Stop_WhileConsumerRunning_ReturnsFalseThenCompletes()
        {
            int runs = 0;
            int completed = 0;
            using (var entered = new ManualResetEvent(false))
            using (var release = new ManualResetEvent(false))
            {
                var channel = CreateChannel(() =>
                {
                    Interlocked.Increment(ref runs);
                    entered.Set();
                    release.WaitOne(WaitTimeout);
                    Interlocked.Exchange(ref completed, 1);
                    return ChannelConsumeResult.MoreQueued;     // Would run again, unless stopped
                }, 1);
                try
                {
                    channel.Start();
                    channel.Write(NewEvent());
                    Assert.True(entered.WaitOne(WaitTimeout));

                    Assert.False(channel.Stop(TimeSpan.FromMilliseconds(50)));

                    release.Set();
                    Assert.True(WaitUntil(() => Read(ref completed) == 1));
                    Thread.Sleep(200);
                    Assert.Equal(1, Read(ref runs));
                }
                finally
                {
                    release.Set();
                    channel.Stop(WaitTimeout);
                }
            }
        }

        [Fact]
        public void Stop_FromInsideConsumer_DoesNotDeadlock()
        {
            AsyncLogEventChannel channel = null;
            bool stopResult = false;
            using (var stopped = new ManualResetEvent(false))
            {
                channel = CreateChannel(() =>
                {
                    stopResult = channel.Stop(WaitTimeout);
                    stopped.Set();
                    return ChannelConsumeResult.Empty;
                }, 1);
                try
                {
                    channel.Start();
                    channel.Write(NewEvent());

                    Assert.True(stopped.WaitOne(WaitTimeout));
                    Assert.True(stopResult);
                }
                finally
                {
                    channel.Stop(WaitTimeout);
                }
            }
        }

        [Fact]
        public void ExecutionContext_FlowsFromInitializingThread()
        {
            // Like the Timer AsyncTargetWrapper always used: the consumer runs with the ExecutionContext
            // (AsyncLocal values, culture) of the thread that created and started the channel during InitializeTarget
            string observed = "not-called";
            using (var called = new ManualResetEvent(false))
            {
                AsyncLogEventChannel channel = null;
                AmbientValue.Value = "caller-value";
                try
                {
                    channel = CreateChannel(() => { observed = AmbientValue.Value; channel.ReadAll(); called.Set(); return ChannelConsumeResult.Empty; }, 1);
                    channel.Start();
                }
                finally
                {
                    AmbientValue.Value = null;
                }

                try
                {
                    channel.Write(NewEvent());
                    Assert.True(called.WaitOne(WaitTimeout));
                    Assert.Equal("caller-value", observed);
                }
                finally
                {
                    channel.Stop(WaitTimeout);
                }
            }
        }

        [Fact]
        public void CurrentCulture_FlowsFromInitializingThread()
        {
            // Log formatting on the background thread must not depend on which dispatcher is selected
            var originalCulture = CultureInfo.CurrentCulture;
            var testCulture = originalCulture.Name == "de-DE" ? "fr-FR" : "de-DE";
            string observed = "not-called";
            using (var called = new ManualResetEvent(false))
            {
                AsyncLogEventChannel channel = null;
                CultureInfo.CurrentCulture = new CultureInfo(testCulture);
                try
                {
                    channel = CreateChannel(() => { observed = CultureInfo.CurrentCulture.Name; channel.ReadAll(); called.Set(); return ChannelConsumeResult.Empty; }, 1);
                    channel.Start();
                }
                finally
                {
                    CultureInfo.CurrentCulture = originalCulture;
                }

                try
                {
                    channel.Write(NewEvent());
                    Assert.True(called.WaitOne(WaitTimeout));
                    Assert.Equal(testCulture, observed);
                }
                finally
                {
                    channel.Stop(WaitTimeout);
                }
            }
        }
    }
}
