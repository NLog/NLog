// 
// Copyright (c) 2004-2020 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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
        public void RaiseEventLogEventQueueGrow_OnLogItems()
        {
            const int RequestsLimit = 2;
            const int EventsCount = 5;
            const int ExpectedCountOfGrovingTimes = 2;
            const int ExpectedFinalSize = 8;
            int grovingItemsCount = 0;

            ConcurrentRequestQueue requestQueue = new ConcurrentRequestQueue(RequestsLimit, AsyncTargetWrapperOverflowAction.Grow);

            requestQueue.LogEventQueueGrow += (o, e) => { grovingItemsCount++; };

            for (int i = 0; i < EventsCount; i++)
            {
                requestQueue.Enqueue(new AsyncLogEventInfo());
            }

            Assert.Equal(ExpectedCountOfGrovingTimes, grovingItemsCount);
            Assert.Equal(ExpectedFinalSize, requestQueue.RequestLimit);
        }

        [Fact]
        public void RaiseEventLogEventDropped_OnLogItems()
        {
            const int RequestsLimit = 2;
            const int EventsCount = 5;
            int discardedItemsCount = 0;
	        
            int ExpectedDiscardedItemsCount = EventsCount - RequestsLimit;
            ConcurrentRequestQueue requestQueue = new ConcurrentRequestQueue(RequestsLimit, AsyncTargetWrapperOverflowAction.Discard);

            requestQueue.LogEventDropped+= (o, e) => { discardedItemsCount++; };

            for (int i = 0; i < EventsCount; i++)
            {
                requestQueue.Enqueue(new AsyncLogEventInfo());
            }

            Assert.Equal(ExpectedDiscardedItemsCount, discardedItemsCount);
        }
    }
}

#endif