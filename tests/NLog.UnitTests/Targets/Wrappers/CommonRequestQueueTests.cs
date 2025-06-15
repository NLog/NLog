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
