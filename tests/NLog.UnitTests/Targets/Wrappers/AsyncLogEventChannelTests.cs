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
    using System.Collections.Generic;
    using System.Threading;
    using NLog.Common;
    using NLog.Targets.Wrappers;
    using Xunit;

    public class AsyncLogEventChannelTests : NLogTestBase
    {
        private const int WaitTimeoutMs = 5000;

        private static AsyncLogEventChannel CreateChannel(int queueLimit, AsyncTargetWrapperOverflowAction overflowAction, bool lockingQueue)
        {
            var channel = new AsyncLogEventChannel(queueLimit, overflowAction);
            channel.UseLockingQueue(lockingQueue);
            return channel;
        }

        private static AsyncLogEventInfo NewEvent(string message)
        {
            return new LogEventInfo(LogLevel.Info, "test", message).WithContinuation(ex => { });
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Write_IntoEmptyQueue_ReturnsTrueThenFalse(bool lockingQueue)
        {
            var channel = CreateChannel(10, AsyncTargetWrapperOverflowAction.Discard, lockingQueue);

            Assert.True(channel.Write(NewEvent("1")));
            Assert.False(channel.Write(NewEvent("2")));
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void ReadBatch_ReturnsEventsInOrderUpToCount(bool lockingQueue)
        {
            var channel = CreateChannel(10, AsyncTargetWrapperOverflowAction.Discard, lockingQueue);
            for (int i = 1; i <= 5; ++i)
                channel.Write(NewEvent(i.ToString()));

            var batch = new List<AsyncLogEventInfo>();
            channel.ReadBatch(3, batch);

            Assert.Equal(new[] { "1", "2", "3" }, batch.ConvertAll(e => e.LogEvent.Message));
            Assert.False(channel.IsEmpty);

            var rest = channel.ReadAll();

            Assert.Equal(2, rest.Length);
            Assert.Equal("4", rest[0].LogEvent.Message);
            Assert.Equal("5", rest[1].LogEvent.Message);
            Assert.True(channel.IsEmpty);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Discard_WhenFull_RaisesLogEventDropped(bool lockingQueue)
        {
            var channel = CreateChannel(2, AsyncTargetWrapperOverflowAction.Discard, lockingQueue);
            var dropped = new List<LogEventInfo>();
            channel.LogEventDropped += (sender, args) => dropped.Add(args.DroppedLogEventInfo);

            var first = NewEvent("1");
            channel.Write(first);
            channel.Write(NewEvent("2"));
            channel.Write(NewEvent("3"));

            Assert.Single(dropped);
            Assert.Same(first.LogEvent, dropped[0]);
            Assert.Equal(2, channel.ReadAll().Length);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Grow_WhenFull_RaisesLogEventQueueGrow(bool lockingQueue)
        {
            var channel = CreateChannel(2, AsyncTargetWrapperOverflowAction.Grow, lockingQueue);
            int growCount = 0;
            channel.LogEventQueueGrow += (sender, args) => ++growCount;

            channel.Write(NewEvent("1"));
            channel.Write(NewEvent("2"));
            channel.Write(NewEvent("3"));

            Assert.Equal(1, growCount);
            Assert.Equal(4, channel.QueueLimit);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Block_WhenFull_WaitsUntilRead(bool lockingQueue)
        {
            var channel = CreateChannel(2, AsyncTargetWrapperOverflowAction.Block, lockingQueue);
            channel.Write(NewEvent("1"));
            channel.Write(NewEvent("2"));

            using (var written = new ManualResetEvent(false))
            {
                var writer = new Thread(() => { channel.Write(NewEvent("3")); written.Set(); }) { IsBackground = true };
                writer.Start();

                Assert.False(written.WaitOne(200));

                channel.ReadBatch(1, new List<AsyncLogEventInfo>());

                Assert.True(written.WaitOne(WaitTimeoutMs));
                Assert.True(writer.Join(WaitTimeoutMs));
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Block_WhenFull_ReleasedByClear(bool lockingQueue)
        {
            var channel = CreateChannel(2, AsyncTargetWrapperOverflowAction.Block, lockingQueue);
            channel.Write(NewEvent("1"));
            channel.Write(NewEvent("2"));

            using (var written = new ManualResetEvent(false))
            {
                var writer = new Thread(() => { channel.Write(NewEvent("3")); written.Set(); }) { IsBackground = true };
                writer.Start();

                Assert.False(written.WaitOne(200));

                channel.Clear();

                Assert.True(written.WaitOne(WaitTimeoutMs));
                Assert.True(writer.Join(WaitTimeoutMs));
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void UseLockingQueue_KeepsEventSubscriptions(bool lockingQueue)
        {
            var channel = CreateChannel(2, AsyncTargetWrapperOverflowAction.Discard, lockingQueue);
            int droppedCount = 0;
            channel.LogEventDropped += (sender, args) => ++droppedCount;

            channel.UseLockingQueue(!lockingQueue);
            channel.Write(NewEvent("1"));
            channel.Write(NewEvent("2"));
            channel.Write(NewEvent("3"));

            Assert.Equal(!lockingQueue, channel.IsLockingQueue);
            Assert.Equal(1, droppedCount);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void UseLockingQueue_KeepsQueueLimitAndOverflowAction(bool lockingQueue)
        {
            var channel = CreateChannel(10, AsyncTargetWrapperOverflowAction.Discard, lockingQueue);
            channel.QueueLimit = 7;
            channel.OverflowAction = AsyncTargetWrapperOverflowAction.Block;

            channel.UseLockingQueue(!lockingQueue);

            Assert.Equal(!lockingQueue, channel.IsLockingQueue);
            Assert.Equal(7, channel.QueueLimit);
            Assert.Equal(AsyncTargetWrapperOverflowAction.Block, channel.OverflowAction);
        }
    }
}
