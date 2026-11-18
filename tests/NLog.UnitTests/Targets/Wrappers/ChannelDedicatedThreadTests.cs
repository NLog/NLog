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
    using System.Threading;
    using NLog.Targets.Wrappers;
    using Xunit;

    public class ChannelDedicatedThreadTests : SignalChannelTestsBase
    {
        private const string ThreadName = "NLog ChannelTests";

        internal override AsyncLogEventChannel CreateChannel(Func<ChannelConsumeResult> consume, int timeToSleepBetweenBatches)
        {
            return new ChannelDedicatedThread(1000, AsyncTargetWrapperOverflowAction.Discard, consume, timeToSleepBetweenBatches, ThreadName);
        }

        [Fact]
        public void Consumer_RunsOnNamedBackgroundNonThreadPoolThread()
        {
            Thread consumerThread = null;
            using (var called = new ManualResetEvent(false))
            {
                AsyncLogEventChannel channel = null;
                channel = CreateChannel(() => { consumerThread = Thread.CurrentThread; channel.ReadAll(); called.Set(); return ChannelConsumeResult.Empty; }, 1);
                try
                {
                    channel.Start();
                    channel.Write(NewEvent());

                    Assert.True(called.WaitOne(WaitTimeout));
                    Assert.Equal(ThreadName, consumerThread.Name);
                    Assert.True(consumerThread.IsBackground);
                    Assert.False(consumerThread.IsThreadPoolThread);
                }
                finally
                {
                    channel.Stop(WaitTimeout);
                }
            }
        }

        [Fact]
        public void Stop_JoinsThread()
        {
            Thread consumerThread = null;
            using (var called = new ManualResetEvent(false))
            {
                AsyncLogEventChannel channel = null;
                channel = CreateChannel(() => { consumerThread = Thread.CurrentThread; channel.ReadAll(); called.Set(); return ChannelConsumeResult.Empty; }, 1);
                channel.Start();
                channel.Write(NewEvent());
                Assert.True(called.WaitOne(WaitTimeout));

                Assert.True(channel.Stop(WaitTimeout));
                Assert.False(consumerThread.IsAlive);
            }
        }

        [Fact]
        public void Stop_WhenThreadStartFailed_DoesNotThrow()
        {
            var channel = CreateChannel(() => ChannelConsumeResult.Empty, 1);

            // Same state as when Thread.Start throws (out of memory): marked as started, thread never started
            typeof(ChannelDedicatedThread).GetField("_started", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(channel, true);

            Assert.True(channel.Stop(TimeSpan.Zero));
            Assert.True(channel.Stop(TimeSpan.Zero));
        }
    }
}
