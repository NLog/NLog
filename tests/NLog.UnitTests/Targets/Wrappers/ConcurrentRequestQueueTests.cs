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

#if !NET35

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NLog.Common;
using NLog.Targets.Wrappers;
using Xunit;

namespace NLog.UnitTests.Targets.Wrappers
{
    public class ConcurrentRequestQueueTests : NLogTestBase
    {
        [Theory]
        [InlineData(AsyncTargetWrapperOverflowAction.Block)]
        [InlineData(AsyncTargetWrapperOverflowAction.Discard)]
        [InlineData(AsyncTargetWrapperOverflowAction.Grow)]
        public void DequeueBatch_WithNonEmptyList_ReturnsValidCount(AsyncTargetWrapperOverflowAction overflowAction)
        {
            // Stage
            ConcurrentRequestQueue requestQueue = new ConcurrentRequestQueue(2, overflowAction);
            requestQueue.Enqueue(new AsyncLogEventInfo());
            requestQueue.Enqueue(new AsyncLogEventInfo());
            Assert.Equal(2, requestQueue.Count);

            // Act
            var batch = new List<AsyncLogEventInfo>();
            requestQueue.DequeueBatch(1, batch);    // Dequeue into empty-list
            Assert.Equal(1, requestQueue.Count);
            requestQueue.DequeueBatch(1, batch);    // Dequeue into non-empty-list

            // Assert
            Assert.Equal(0, requestQueue.Count);
            Assert.Equal(2, batch.Count);
        }

        [Fact]
        public void Enqueue_WhenGrowBehaviourAndHighlyConcurrent_GrowOnce()
        {
            // Arrange
            var requestQueue = new ConcurrentRequestQueue(2, AsyncTargetWrapperOverflowAction.Grow);

            for (int i = 0; i < 4; i++)
            {
                requestQueue.Enqueue(new AsyncLogEventInfo());
            }

            const int initialQueueCount = 4;
            const int initialQueueLimit = 4;
            Assert.Equal(initialQueueCount, requestQueue.Count);
            Assert.Equal(initialQueueLimit, requestQueue.QueueLimit);

            const int threadCount = 4;
            var readyToEnqueue = new Barrier(threadCount);
            var enqueued = new CountdownEvent(threadCount);

            // Act
            for (int i = 0; i < 100; i++)
            {
                for (int j = 0; j < threadCount; j++)
                {
                    Task.Run(EnqueueWhenAllThreadsReady);
                }

                Assert.True(enqueued.Wait(10000));
                enqueued.Reset();

                // Assert
                const int expectedQueueCount = initialQueueCount + threadCount;
                const int expectedQueueLimit = initialQueueLimit * 2;
                Assert.Equal(expectedQueueCount, requestQueue.Count);
                Assert.Equal(expectedQueueLimit, requestQueue.QueueLimit);

                // rollback requests count and limit
                requestQueue.DequeueBatch(threadCount);
                requestQueue.QueueLimit = initialQueueLimit;
            }

            void EnqueueWhenAllThreadsReady()
            {
                var logEvent = new AsyncLogEventInfo();
                readyToEnqueue.SignalAndWait();

                requestQueue.Enqueue(logEvent);

                enqueued.Signal();
            }
        }

        [Fact]
        public void RaiseEventLogEventQueueGrow_OnLogItems()
        {
            CommonRequestQueueTests.RaiseEventLogEventQueueGrow_OnLogItems(GetConcurrentRequestQueue);
        }

        [Fact]
        public void RaiseEventLogEventDropped_OnLogItems()
        {
            CommonRequestQueueTests.RaiseEventLogEventDropped_OnLogItems(GetConcurrentRequestQueue);
        }

        private static AsyncRequestQueueBase GetConcurrentRequestQueue(int size, AsyncTargetWrapperOverflowAction action) =>
            new ConcurrentRequestQueue(size, action);
    }
}

#endif
