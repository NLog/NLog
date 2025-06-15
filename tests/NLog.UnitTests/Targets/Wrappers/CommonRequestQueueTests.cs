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
    using NLog.Common;
    using NLog.Targets.Wrappers;
    using Xunit;

    internal static class CommonRequestQueueTests
    {
        internal static void RaiseEventLogEventQueueGrow_OnLogItems(Func<int, AsyncTargetWrapperOverflowAction, AsyncRequestQueueBase> getQueue)
        {
            const int InitialSize = 1;
            const int ExpectedFinalSize = 8;

            var requestQueue = getQueue(InitialSize, AsyncTargetWrapperOverflowAction.Grow);

            int growingTimesCount = 0;
            long reportedRequestsCount = 0;
            long reportedNewQueueSize = 0;
            requestQueue.LogEventQueueGrow += (_, e) =>
            {
                growingTimesCount++;
                reportedRequestsCount = e.RequestsCount;
                reportedNewQueueSize = e.NewQueueSize;
            };

            requestQueue.Enqueue(new AsyncLogEventInfo());
            Assert.Equal(0, growingTimesCount);

            requestQueue.Enqueue(new AsyncLogEventInfo());
            Assert.Equal(1, growingTimesCount);
            Assert.Equal(2, reportedRequestsCount);
            Assert.Equal(2, reportedNewQueueSize);

            requestQueue.Enqueue(new AsyncLogEventInfo());
            Assert.Equal(2, growingTimesCount);
            Assert.Equal(3, reportedRequestsCount);
            Assert.Equal(4, reportedNewQueueSize);

            requestQueue.Enqueue(new AsyncLogEventInfo());
            Assert.Equal(2, growingTimesCount);

            requestQueue.Enqueue(new AsyncLogEventInfo());
            Assert.Equal(3, growingTimesCount);
            Assert.Equal(5, reportedRequestsCount);
            Assert.Equal(ExpectedFinalSize, reportedNewQueueSize);
            Assert.Equal(ExpectedFinalSize, requestQueue.RequestLimit);
        }

        internal static void RaiseEventLogEventDropped_OnLogItems(Func<int, AsyncTargetWrapperOverflowAction, AsyncRequestQueueBase> getQueue)
        {
            const int RequestsLimit = 2;
            const int EventsCount = 5;
            const int ExpectedDiscardedItemsCount = EventsCount - RequestsLimit;

            var requestQueue = getQueue(RequestsLimit, AsyncTargetWrapperOverflowAction.Discard);

            int discardedItemsCount = 0;
            requestQueue.LogEventDropped += (o, e) => { discardedItemsCount++; };

            for (int i = 0; i < EventsCount; i++)
            {
                requestQueue.Enqueue(new AsyncLogEventInfo());
            }

            Assert.Equal(ExpectedDiscardedItemsCount, discardedItemsCount);
        }
    }
}
