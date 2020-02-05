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

namespace NLog.UnitTests.Targets.Wrappers
{
    using System.Threading;
    using NLog.Common;
    using NLog.Targets.Wrappers;
    using Xunit;

    public class AsyncRequestQueueTests : NLogTestBase
	{
        [Fact]
        public void AsyncRequestQueueWithDiscardBehaviorTest()
        {
            var ev1 = LogEventInfo.CreateNullEvent().WithContinuation(ex => { });
            var ev2 = LogEventInfo.CreateNullEvent().WithContinuation(ex => { });
            var ev3 = LogEventInfo.CreateNullEvent().WithContinuation(ex => { });
            var ev4 = LogEventInfo.CreateNullEvent().WithContinuation(ex => { });

            var queue = new AsyncRequestQueue(3, AsyncTargetWrapperOverflowAction.Discard);
            Assert.Equal(3, queue.RequestLimit);
            Assert.Equal(AsyncTargetWrapperOverflowAction.Discard, queue.OnOverflow);
            Assert.Equal(0, queue.RequestCount);
            queue.Enqueue(ev1);
            Assert.Equal(1, queue.RequestCount);
            queue.Enqueue(ev2);
            Assert.Equal(2, queue.RequestCount);
            queue.Enqueue(ev3);
            Assert.Equal(3, queue.RequestCount);
            queue.Enqueue(ev4);
            Assert.Equal(3, queue.RequestCount);

            AsyncLogEventInfo[] logEventInfos = queue.DequeueBatch(10);
            Assert.Equal(0, queue.RequestCount);

            // ev1 is lost
            Assert.Same(logEventInfos[0].LogEvent, ev2.LogEvent);
            Assert.Same(logEventInfos[1].LogEvent, ev3.LogEvent);
            Assert.Same(logEventInfos[2].LogEvent, ev4.LogEvent);
            Assert.Same(logEventInfos[0].Continuation, ev2.Continuation);
            Assert.Same(logEventInfos[1].Continuation, ev3.Continuation);
            Assert.Same(logEventInfos[2].Continuation, ev4.Continuation);
        }

        [Fact]
        public void AsyncRequestQueueWithGrowBehaviorTest()
        {
            var ev1 = LogEventInfo.CreateNullEvent().WithContinuation(ex => { });
            var ev2 = LogEventInfo.CreateNullEvent().WithContinuation(ex => { });
            var ev3 = LogEventInfo.CreateNullEvent().WithContinuation(ex => { });
            var ev4 = LogEventInfo.CreateNullEvent().WithContinuation(ex => { });
            
            var queue = new AsyncRequestQueue(3, AsyncTargetWrapperOverflowAction.Grow);
            Assert.Equal(3, queue.RequestLimit);
            Assert.Equal(AsyncTargetWrapperOverflowAction.Grow, queue.OnOverflow);
            Assert.Equal(0, queue.RequestCount);
            queue.Enqueue(ev1);
            Assert.Equal(1, queue.RequestCount);
            queue.Enqueue(ev2);
            Assert.Equal(2, queue.RequestCount);
            queue.Enqueue(ev3);
            Assert.Equal(3, queue.RequestCount);
            queue.Enqueue(ev4);
            Assert.Equal(4, queue.RequestCount);

            AsyncLogEventInfo[] logEventInfos = queue.DequeueBatch(10);
            int result = logEventInfos.Length;

            Assert.Equal(4, result);
            Assert.Equal(0, queue.RequestCount);

            // ev1 is lost
            Assert.Same(logEventInfos[0].LogEvent, ev1.LogEvent);
            Assert.Same(logEventInfos[1].LogEvent, ev2.LogEvent);
            Assert.Same(logEventInfos[2].LogEvent, ev3.LogEvent);
            Assert.Same(logEventInfos[3].LogEvent, ev4.LogEvent);
            Assert.Same(logEventInfos[0].Continuation, ev1.Continuation);
            Assert.Same(logEventInfos[1].Continuation, ev2.Continuation);
            Assert.Same(logEventInfos[2].Continuation, ev3.Continuation);
            Assert.Same(logEventInfos[3].Continuation, ev4.Continuation);
        }

        [Fact]
        public void AsyncRequestQueueWithBlockBehavior()
        {
            var queue = new AsyncRequestQueue(10, AsyncTargetWrapperOverflowAction.Block);

            ManualResetEvent producerFinished = new ManualResetEvent(false);

            int pushingEvent = 0;

            ThreadPool.QueueUserWorkItem(
                s =>
                {
                    // producer thread
                    for (int i = 0; i < 1000; ++i)
                    {
                        AsyncLogEventInfo logEvent = LogEventInfo.CreateNullEvent().WithContinuation(ex => { });
                        logEvent.LogEvent.Message = "msg" + i;
                        
                        // Console.WriteLine("Pushing event {0}", i);
                        pushingEvent = i;
                        queue.Enqueue(logEvent);
                    }

                    producerFinished.Set();
                });

            // consumer thread
            AsyncLogEventInfo[] logEventInfos;
            int total = 0;

            while (total < 500)
            {
                int left = 500 - total;

                logEventInfos = queue.DequeueBatch(left);
                int got = logEventInfos.Length;
                Assert.True(got <= queue.RequestLimit);
                total += got;
            }

            Thread.Sleep(500);

            // producer is blocked on trying to push event #510
            Assert.Equal(510, pushingEvent);
            queue.DequeueBatch(1);
            total++;
            Thread.Sleep(500);

            // producer is now blocked on trying to push event #511

            Assert.Equal(511, pushingEvent);
            while (total < 1000)
            {
                int left = 1000 - total;

                logEventInfos = queue.DequeueBatch(left);
                int got = logEventInfos.Length;
                Assert.True(got <= queue.RequestLimit);
                total += got;
            }

            // producer should now finish
            producerFinished.WaitOne();
        }

        [Fact]
        public void AsyncRequestQueueClearTest()
        {
            var ev1 = LogEventInfo.CreateNullEvent().WithContinuation(ex => { });
            var ev2 = LogEventInfo.CreateNullEvent().WithContinuation(ex => { });
            var ev3 = LogEventInfo.CreateNullEvent().WithContinuation(ex => { });
            var ev4 = LogEventInfo.CreateNullEvent().WithContinuation(ex => { });

            var queue = new AsyncRequestQueue(3, AsyncTargetWrapperOverflowAction.Grow);
            Assert.Equal(3, queue.RequestLimit);
            Assert.Equal(AsyncTargetWrapperOverflowAction.Grow, queue.OnOverflow);
            Assert.Equal(0, queue.RequestCount);
            queue.Enqueue(ev1);
            Assert.Equal(1, queue.RequestCount);
            queue.Enqueue(ev2);
            Assert.Equal(2, queue.RequestCount);
            queue.Enqueue(ev3);
            Assert.Equal(3, queue.RequestCount);
            queue.Enqueue(ev4);
            Assert.Equal(4, queue.RequestCount);
            queue.Clear();
            Assert.Equal(0, queue.RequestCount);

            AsyncLogEventInfo[] logEventInfos;

            logEventInfos = queue.DequeueBatch(10);
            int result = logEventInfos.Length;
            Assert.Equal(0, result);
            Assert.Equal(0, queue.RequestCount);
        }

	    [Fact]
	    public void RaiseEventLogEventQueueGrow_OnLogItems()
	    {
	        const int RequestsLimit = 2;
	        const int EventsCount = 5;
	        const int ExpectedCountOfGrovingTimes = 2;
	        const int ExpectedFinalSize = 8;
	        int grovingItemsCount = 0;

	        AsyncRequestQueue requestQueue = new AsyncRequestQueue(RequestsLimit, AsyncTargetWrapperOverflowAction.Grow);

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
	        AsyncRequestQueue requestQueue = new AsyncRequestQueue(RequestsLimit, AsyncTargetWrapperOverflowAction.Discard);

	        requestQueue.LogEventDropped+= (o, e) => { discardedItemsCount++; };

	        for (int i = 0; i < EventsCount; i++)
	        {
	            requestQueue.Enqueue(new AsyncLogEventInfo());
	        }

	        Assert.Equal(ExpectedDiscardedItemsCount, discardedItemsCount);
	    }
    }
}
