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

namespace NLog.UnitTests.Targets.Dispatchers
{
    using System;
    using System.Diagnostics;
    using System.Threading;
    using NLog.Targets.Dispatchers;
    using Xunit;

    /// <summary>
    /// Contract tests that every <see cref="IChannelReader"/> implementation must pass.
    /// Assertions use generous timeouts and "at least" counts, never exact wall-clock counts.
    /// </summary>
    public abstract class ChannelReaderTestsBase
    {
        protected static readonly TimeSpan WaitTimeout = TimeSpan.FromSeconds(5);

        internal abstract IChannelReader CreateReader(Func<ConsumeResult> consume, int timeToSleepBetweenBatches);

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

        [Fact]
        public void Start_SchedulesFirstRun()
        {
            int runs = 0;
            var reader = CreateReader(() => { Interlocked.Increment(ref runs); return ConsumeResult.Drained; }, 1);
            try
            {
                reader.Start();
                Assert.True(WaitUntil(() => Read(ref runs) >= 1));
                Thread.Sleep(200);
                Assert.Equal(1, Read(ref runs));
            }
            finally
            {
                reader.Stop(WaitTimeout);
            }
        }

        [Fact]
        public void Signal_WhenIdle_RunsConsumer()
        {
            int runs = 0;
            var reader = CreateReader(() => { Interlocked.Increment(ref runs); return ConsumeResult.Drained; }, 1);
            try
            {
                reader.Start();
                Assert.True(WaitUntil(() => Read(ref runs) >= 1));
                Thread.Sleep(100);
                reader.Signal();
                Assert.True(WaitUntil(() => Read(ref runs) >= 2));
            }
            finally
            {
                reader.Stop(WaitTimeout);
            }
        }

        [Fact]
        public void Signal_ZeroSleep_RunsConsumer()
        {
            int runs = 0;
            var reader = CreateReader(() => { Interlocked.Increment(ref runs); return ConsumeResult.Drained; }, 0);
            try
            {
                reader.Start();
                Assert.True(WaitUntil(() => Read(ref runs) >= 1));
                Thread.Sleep(100);
                reader.Signal();
                Assert.True(WaitUntil(() => Read(ref runs) >= 2));
            }
            finally
            {
                reader.Stop(WaitTimeout);
            }
        }

        [Fact]
        public void Signal_DuringRun_CausesAnotherRun()
        {
            int runs = 0;
            using (var entered = new ManualResetEvent(false))
            using (var release = new ManualResetEvent(false))
            {
                var reader = CreateReader(() =>
                {
                    if (Interlocked.Increment(ref runs) == 2)
                    {
                        entered.Set();
                        release.WaitOne(WaitTimeout);
                    }
                    return ConsumeResult.Drained;
                }, 1);
                try
                {
                    reader.Start();
                    Assert.True(WaitUntil(() => Read(ref runs) >= 1));
                    Thread.Sleep(100);
                    reader.Signal();
                    Assert.True(entered.WaitOne(WaitTimeout));

                    reader.Signal();    // Arrives while the second run is in progress
                    release.Set();

                    Assert.True(WaitUntil(() => Read(ref runs) >= 3));
                }
                finally
                {
                    release.Set();
                    reader.Stop(WaitTimeout);
                }
            }
        }

        [Fact]
        public void FullBatch_RunsAgainWithoutSignal()
        {
            int runs = 0;
            var reader = CreateReader(() => Interlocked.Increment(ref runs) == 1 ? ConsumeResult.FullBatch : ConsumeResult.Drained, 1);
            try
            {
                reader.Start();
                Assert.True(WaitUntil(() => Read(ref runs) >= 2));
            }
            finally
            {
                reader.Stop(WaitTimeout);
            }
        }

        [Fact]
        public void MorePending_RunsAgainWithoutSignal()
        {
            int runs = 0;
            var reader = CreateReader(() => Interlocked.Increment(ref runs) == 1 ? ConsumeResult.MorePending : ConsumeResult.Drained, 1);
            try
            {
                reader.Start();
                Assert.True(WaitUntil(() => Read(ref runs) >= 2));
            }
            finally
            {
                reader.Stop(WaitTimeout);
            }
        }

        [Fact]
        public void Drained_WaitsForSignal()
        {
            int runs = 0;
            var reader = CreateReader(() => { Interlocked.Increment(ref runs); return ConsumeResult.Drained; }, 1);
            try
            {
                reader.Start();
                Assert.True(WaitUntil(() => Read(ref runs) >= 1));
                Thread.Sleep(100);
                reader.Signal();
                Assert.True(WaitUntil(() => Read(ref runs) >= 2));

                Thread.Sleep(300);

                Assert.Equal(2, Read(ref runs));
            }
            finally
            {
                reader.Stop(WaitTimeout);
            }
        }

        [Fact]
        public void PeriodicSleep_RunsRepeatedlyWithoutSignal()
        {
            int runs = 0;
            var reader = CreateReader(() => { Interlocked.Increment(ref runs); return ConsumeResult.Drained; }, 50);
            try
            {
                reader.Start();
                Assert.True(WaitUntil(() => Read(ref runs) >= 3));
            }
            finally
            {
                reader.Stop(WaitTimeout);
            }
        }

        [Fact]
        public void ConsumerThrows_RunsAgain()
        {
            int runs = 0;
            var reader = CreateReader(() =>
            {
                if (Interlocked.Increment(ref runs) == 1)
                    throw new ApplicationException("consumer failure");
                return ConsumeResult.Drained;
            }, 1);
            try
            {
                reader.Start();
                Assert.True(WaitUntil(() => Read(ref runs) >= 2));
            }
            finally
            {
                reader.Stop(WaitTimeout);
            }
        }

        [Fact]
        public void NoConcurrentRuns()
        {
            int runs = 0;
            int active = 0;
            int maxActive = 0;
            var reader = CreateReader(() =>
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
                return ConsumeResult.Drained;
            }, 0);
            try
            {
                reader.Start();
                var producers = new Thread[4];
                for (int i = 0; i < producers.Length; ++i)
                {
                    producers[i] = new Thread(() =>
                    {
                        // Keep signaling until signals have overlapped with many runs
                        var stopwatch = Stopwatch.StartNew();
                        while (Read(ref runs) < 20 && stopwatch.Elapsed < WaitTimeout)
                            reader.Signal();
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
                reader.Stop(WaitTimeout);
            }
        }

        [Fact]
        public void Stop_CalledTwice_ReturnsTrue()
        {
            var reader = CreateReader(() => ConsumeResult.Drained, 1);
            reader.Start();

            Assert.True(reader.Stop(WaitTimeout));
            Assert.True(reader.Stop(WaitTimeout));
        }

        [Fact]
        public void Signal_AfterStop_DoesNotRun()
        {
            int runs = 0;
            var reader = CreateReader(() => { Interlocked.Increment(ref runs); return ConsumeResult.Drained; }, 1);
            reader.Start();
            Assert.True(WaitUntil(() => Read(ref runs) >= 1));
            Assert.True(reader.Stop(WaitTimeout));

            reader.Signal();
            Thread.Sleep(300);

            Assert.Equal(1, Read(ref runs));
        }

        [Fact]
        public void Stop_WhileConsumerRunning_ReturnsFalseThenCompletes()
        {
            int runs = 0;
            int completed = 0;
            using (var entered = new ManualResetEvent(false))
            using (var release = new ManualResetEvent(false))
            {
                var reader = CreateReader(() =>
                {
                    Interlocked.Increment(ref runs);
                    entered.Set();
                    release.WaitOne(WaitTimeout);
                    Interlocked.Exchange(ref completed, 1);
                    return ConsumeResult.MorePending;   // Would schedule another run, unless stopped
                }, 1);
                try
                {
                    reader.Start();
                    Assert.True(entered.WaitOne(WaitTimeout));

                    Assert.False(reader.Stop(TimeSpan.FromMilliseconds(50)));

                    release.Set();
                    Assert.True(WaitUntil(() => Read(ref completed) == 1));
                    Thread.Sleep(200);
                    Assert.Equal(1, Read(ref runs));
                }
                finally
                {
                    release.Set();
                    reader.Stop(WaitTimeout);
                }
            }
        }

        [Fact]
        public void Stop_FromInsideConsumer_DoesNotDeadlock()
        {
            IChannelReader reader = null;
            bool stopResult = false;
            using (var stopped = new ManualResetEvent(false))
            {
                reader = CreateReader(() =>
                {
                    stopResult = reader.Stop(WaitTimeout);
                    stopped.Set();
                    return ConsumeResult.Drained;
                }, 1);
                try
                {
                    reader.Start();

                    Assert.True(stopped.WaitOne(WaitTimeout));
                    Assert.True(stopResult);
                }
                finally
                {
                    reader.Stop(WaitTimeout);
                }
            }
        }
    }
}
