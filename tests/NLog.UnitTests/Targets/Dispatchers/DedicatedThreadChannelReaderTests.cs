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
    using System.Threading;
    using NLog.Targets.Dispatchers;
    using Xunit;

    public class DedicatedThreadChannelReaderTests : ChannelReaderTestsBase
    {
        private const string ThreadName = "NLog ChannelReaderTests";

        private static readonly AsyncLocal<string> AmbientValue = new AsyncLocal<string>();

        internal override IChannelReader CreateReader(Func<ConsumeResult> consume, int timeToSleepBetweenBatches)
        {
            return new DedicatedThreadChannelReader(consume, timeToSleepBetweenBatches, ThreadName);
        }

        [Fact]
        public void Consumer_RunsOnNamedBackgroundNonThreadPoolThread()
        {
            Thread consumerThread = null;
            using (var called = new ManualResetEvent(false))
            {
                var reader = CreateReader(() => { consumerThread = Thread.CurrentThread; called.Set(); return ConsumeResult.Drained; }, 1);
                try
                {
                    reader.Start();

                    Assert.True(called.WaitOne(WaitTimeout));
                    Assert.Equal(ThreadName, consumerThread.Name);
                    Assert.True(consumerThread.IsBackground);
                    Assert.False(consumerThread.IsThreadPoolThread);
                }
                finally
                {
                    reader.Stop(WaitTimeout);
                }
            }
        }

        [Fact]
        public void ExecutionContext_DoesNotFlowIntoReaderThread()
        {
            string observed = "not-called";
            using (var called = new ManualResetEvent(false))
            {
                IChannelReader reader;
                AmbientValue.Value = "caller-value";
                try
                {
                    reader = CreateReader(() => { observed = AmbientValue.Value; called.Set(); return ConsumeResult.Drained; }, 1);
                    reader.Start();
                }
                finally
                {
                    AmbientValue.Value = null;
                }

                try
                {
                    Assert.True(called.WaitOne(WaitTimeout));
                    Assert.Null(observed);
                }
                finally
                {
                    reader.Stop(WaitTimeout);
                }
            }
        }

        [Fact]
        public void Stop_JoinsThread()
        {
            Thread consumerThread = null;
            using (var called = new ManualResetEvent(false))
            {
                var reader = CreateReader(() => { consumerThread = Thread.CurrentThread; called.Set(); return ConsumeResult.Drained; }, 1);
                reader.Start();
                Assert.True(called.WaitOne(WaitTimeout));

                Assert.True(reader.Stop(WaitTimeout));
                Assert.False(consumerThread.IsAlive);
            }
        }
    }
}
