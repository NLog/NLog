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
    using System.Threading;
    using NLog.Common;
    using NLog.Targets.Wrappers;
    using Xunit;

    /// <summary>
    /// The timer channel wakes up every timeToSleepBetweenBatches and checks the queue; writes do not trigger it.
    /// </summary>
    public class ChannelTimerThreadTests
    {
        private static readonly TimeSpan WaitTimeout = TimeSpan.FromSeconds(5);

        private static readonly AsyncLocal<string> AmbientValue = new AsyncLocal<string>();

        private static AsyncLogEventChannel CreateChannel(Func<ChannelConsumeResult> consume, int timeToSleepBetweenBatches)
        {
            return new ChannelTimerThread(1000, AsyncTargetWrapperOverflowAction.Discard, consume, timeToSleepBetweenBatches, "test");
        }

        private static bool WaitUntil(Func<bool> condition)
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

        private static int Read(ref int value) => Interlocked.CompareExchange(ref value, 0, 0);

        private static AsyncLogEventInfo NewEvent() => LogEventInfo.CreateNullEvent().WithContinuation(ex => { });

        [Fact]
        public void RunsPeriodicallyWithoutWrites()
        {
            int runs = 0;
            var channel = CreateChannel(() => { Interlocked.Increment(ref runs); return ChannelConsumeResult.Empty; }, 50);
            try
            {
                channel.Start();
                Assert.True(WaitUntil(() => Read(ref runs) >= 3));
            }
            finally
            {
                channel.Stop(WaitTimeout);
            }
        }

        [Fact]
        public void FirstWrite_DoesNotTriggerRun()
        {
            int runs = 0;
            var channel = CreateChannel(() => { Interlocked.Increment(ref runs); return ChannelConsumeResult.Empty; }, 1000);
            try
            {
                channel.Start();
                channel.Write(NewEvent());
                Thread.Sleep(300);

                Assert.Equal(0, Read(ref runs));
            }
            finally
            {
                channel.Stop(WaitTimeout);
            }
        }

        [Fact]
        public void NoOverlappingRuns()
        {
            int runs = 0;
            int active = 0;
            int maxActive = 0;
            var channel = CreateChannel(() =>
            {
                Interlocked.Increment(ref runs);
                int nowActive = Interlocked.Increment(ref active);
                int seenMax;
                while (nowActive > (seenMax = Read(ref maxActive)) && Interlocked.CompareExchange(ref maxActive, nowActive, seenMax) != seenMax)
                {
                    // Retry until maxActive holds the highest value seen
                }
                Thread.Sleep(100);  // Slower than the 20 ms period
                Interlocked.Decrement(ref active);
                return ChannelConsumeResult.Empty;
            }, 20);
            try
            {
                channel.Start();
                Assert.True(WaitUntil(() => Read(ref runs) >= 3));
                Assert.Equal(1, Read(ref maxActive));
            }
            finally
            {
                channel.Stop(WaitTimeout);
            }
        }

        [Fact]
        public void ConsumerThrows_KeepsTicking()
        {
            int runs = 0;
            var channel = CreateChannel(() =>
            {
                if (Interlocked.Increment(ref runs) == 1)
                    throw new ApplicationException("consumer failure");
                return ChannelConsumeResult.Empty;
            }, 50);
            try
            {
                channel.Start();
                Assert.True(WaitUntil(() => Read(ref runs) >= 3));
            }
            finally
            {
                channel.Stop(WaitTimeout);
            }
        }

        [Fact]
        public void Stop_CalledTwice_ReturnsTrue()
        {
            var channel = CreateChannel(() => ChannelConsumeResult.Empty, 50);
            channel.Start();

            Assert.True(channel.Stop(WaitTimeout));
            Assert.True(channel.Stop(WaitTimeout));
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
                    return ChannelConsumeResult.Empty;
                }, 20);
                try
                {
                    channel.Start();
                    Assert.True(entered.WaitOne(WaitTimeout));

                    Assert.False(channel.Stop(TimeSpan.FromMilliseconds(50)));

                    release.Set();
                    Assert.True(WaitUntil(() => Read(ref completed) == 1));
                    Thread.Sleep(200);  // Ten periods: no new tick after stop
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
        public void ExecutionContext_FlowsFromInitializingThread()
        {
            string observed = "not-called";
            using (var called = new ManualResetEvent(false))
            {
                AsyncLogEventChannel channel;
                AmbientValue.Value = "caller-value";
                try
                {
                    channel = CreateChannel(() => { observed = AmbientValue.Value; called.Set(); return ChannelConsumeResult.Empty; }, 20);
                    channel.Start();
                }
                finally
                {
                    AmbientValue.Value = null;
                }

                try
                {
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
        public void Constructor_ShortSleep_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => CreateChannel(() => ChannelConsumeResult.Empty, 1));
        }
    }
}
