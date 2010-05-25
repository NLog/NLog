// 
// Copyright (c) 2004-2010 Jaroslaw Kowalski <jaak@jkowalski.net>
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
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using NLog.Internal;
    using NLog.Targets.Wrappers;

    [TestClass]
    public class AsyncRequestQueueTests : NLogTestBase
	{
        [TestMethod]
        public void AsyncRequestQueueWithDiscardBehaviorTest()
        {
            AsyncContinuation cont1 = ex => { };
            AsyncContinuation cont2 = ex => { };
            AsyncContinuation cont3 = ex => { };
            AsyncContinuation cont4 = ex => { };
            AsyncContinuation cont5 = ex => { };

            var ev1 = LogEventInfo.CreateNullEvent();
            var ev2 = LogEventInfo.CreateNullEvent();
            var ev3 = LogEventInfo.CreateNullEvent();
            var ev4 = LogEventInfo.CreateNullEvent();
            var ev5 = LogEventInfo.CreateNullEvent();

            var queue = new AsyncRequestQueue(3, AsyncTargetWrapperOverflowAction.Discard);
            Assert.AreEqual(3, queue.RequestLimit);
            Assert.AreEqual(AsyncTargetWrapperOverflowAction.Discard, queue.OnOverflow);
            Assert.AreEqual(0, queue.RequestCount);
            queue.Enqueue(ev1, cont1);
            Assert.AreEqual(1, queue.RequestCount);
            queue.Enqueue(ev2, cont2);
            Assert.AreEqual(2, queue.RequestCount);
            queue.Enqueue(ev3, cont3);
            Assert.AreEqual(3, queue.RequestCount);
            queue.Enqueue(ev4, cont4);
            Assert.AreEqual(3, queue.RequestCount);

            LogEventInfo[] logEventInfos;
            AsyncContinuation[] asyncContinuations;

            int result = queue.DequeueBatch(10, out logEventInfos, out asyncContinuations);
            Assert.AreEqual(result, logEventInfos.Length);
            Assert.AreEqual(result, asyncContinuations.Length);

            Assert.AreEqual(3, result);
            Assert.AreEqual(0, queue.RequestCount);

            // ev1 is lost
            Assert.AreSame(logEventInfos[0], ev2);
            Assert.AreSame(logEventInfos[1], ev3);
            Assert.AreSame(logEventInfos[2], ev4);

            // cont1 is lost
            Assert.AreSame(asyncContinuations[0], cont2);
            Assert.AreSame(asyncContinuations[1], cont3);
            Assert.AreSame(asyncContinuations[2], cont4);
        }

        [TestMethod]
        public void AsyncRequestQueueWithGrowBehaviorTest()
        {
            AsyncContinuation cont1 = ex => { };
            AsyncContinuation cont2 = ex => { };
            AsyncContinuation cont3 = ex => { };
            AsyncContinuation cont4 = ex => { };
            AsyncContinuation cont5 = ex => { };

            var ev1 = LogEventInfo.CreateNullEvent();
            var ev2 = LogEventInfo.CreateNullEvent();
            var ev3 = LogEventInfo.CreateNullEvent();
            var ev4 = LogEventInfo.CreateNullEvent();
            var ev5 = LogEventInfo.CreateNullEvent();

            var queue = new AsyncRequestQueue(3, AsyncTargetWrapperOverflowAction.Grow);
            Assert.AreEqual(3, queue.RequestLimit);
            Assert.AreEqual(AsyncTargetWrapperOverflowAction.Grow, queue.OnOverflow);
            Assert.AreEqual(0, queue.RequestCount);
            queue.Enqueue(ev1, cont1);
            Assert.AreEqual(1, queue.RequestCount);
            queue.Enqueue(ev2, cont2);
            Assert.AreEqual(2, queue.RequestCount);
            queue.Enqueue(ev3, cont3);
            Assert.AreEqual(3, queue.RequestCount);
            queue.Enqueue(ev4, cont4);
            Assert.AreEqual(4, queue.RequestCount);

            LogEventInfo[] logEventInfos;
            AsyncContinuation[] asyncContinuations;

            int result = queue.DequeueBatch(10, out logEventInfos, out asyncContinuations);
            Assert.AreEqual(result, logEventInfos.Length);
            Assert.AreEqual(result, asyncContinuations.Length);

            Assert.AreEqual(4, result);
            Assert.AreEqual(0, queue.RequestCount);

            // ev1 is lost
            Assert.AreSame(logEventInfos[0], ev1);
            Assert.AreSame(logEventInfos[1], ev2);
            Assert.AreSame(logEventInfos[2], ev3);
            Assert.AreSame(logEventInfos[3], ev4);

            // cont1 is lost
            Assert.AreSame(asyncContinuations[0], cont1);
            Assert.AreSame(asyncContinuations[1], cont2);
            Assert.AreSame(asyncContinuations[2], cont3);
            Assert.AreSame(asyncContinuations[3], cont4);
        }

#if !NET_CF
        [TestMethod]
        public void AsyncRequestQueueWithBlockBehavior()
        {
            var queue = new AsyncRequestQueue(10, AsyncTargetWrapperOverflowAction.Block);

            ManualResetEvent consumerFinished = new ManualResetEvent(false);
            ManualResetEvent producerFinished = new ManualResetEvent(false);

            int pushingEvent = 0;

            ThreadPool.QueueUserWorkItem(
                s =>
                {
                    // producer thread
                    for (int i = 0; i < 1000; ++i)
                    {
                        LogEventInfo logEvent = LogEventInfo.CreateNullEvent();
                        logEvent.Message = "msg" + i;
                        AsyncContinuation cont = ex => { };

                        Console.WriteLine("Pushing event {0}", i);
                        pushingEvent = i;
                        queue.Enqueue(logEvent, cont);
                    }

                    producerFinished.Set();
                });

            // consumer thread
            LogEventInfo[] logEventInfos;
            AsyncContinuation[] asyncContinuations;
            int total = 0;

            while (total < 500)
            {
                int left = 500 - total;

                int got = queue.DequeueBatch(left, out logEventInfos, out asyncContinuations);
                Assert.IsTrue(got <= queue.RequestLimit);
                total += got;
            }

            Thread.Sleep(500);

            // producer is blocked on trying to push event #510
            Assert.AreEqual(510, pushingEvent);
            queue.DequeueBatch(1, out logEventInfos, out asyncContinuations);
            total++;
            Thread.Sleep(500);

            // producer is now blocked on trying to push event #511

            Assert.AreEqual(511, pushingEvent);
            while (total < 1000)
            {
                int left = 1000 - total;

                int got = queue.DequeueBatch(left, out logEventInfos, out asyncContinuations);
                Assert.IsTrue(got <= queue.RequestLimit);
                total += got;
            }

            // producer should now finish
            producerFinished.WaitOne();
        }
#endif

        [TestMethod]
        public void AsyncRequestQueueClearTest()
        {
            AsyncContinuation cont1 = ex => { };
            AsyncContinuation cont2 = ex => { };
            AsyncContinuation cont3 = ex => { };
            AsyncContinuation cont4 = ex => { };
            AsyncContinuation cont5 = ex => { };

            var ev1 = LogEventInfo.CreateNullEvent();
            var ev2 = LogEventInfo.CreateNullEvent();
            var ev3 = LogEventInfo.CreateNullEvent();
            var ev4 = LogEventInfo.CreateNullEvent();
            var ev5 = LogEventInfo.CreateNullEvent();

            var queue = new AsyncRequestQueue(3, AsyncTargetWrapperOverflowAction.Grow);
            Assert.AreEqual(3, queue.RequestLimit);
            Assert.AreEqual(AsyncTargetWrapperOverflowAction.Grow, queue.OnOverflow);
            Assert.AreEqual(0, queue.RequestCount);
            queue.Enqueue(ev1, cont1);
            Assert.AreEqual(1, queue.RequestCount);
            queue.Enqueue(ev2, cont2);
            Assert.AreEqual(2, queue.RequestCount);
            queue.Enqueue(ev3, cont3);
            Assert.AreEqual(3, queue.RequestCount);
            queue.Enqueue(ev4, cont4);
            Assert.AreEqual(4, queue.RequestCount);
            queue.Clear();
            Assert.AreEqual(0, queue.RequestCount);

            LogEventInfo[] logEventInfos;
            AsyncContinuation[] asyncContinuations;

            int result = queue.DequeueBatch(10, out logEventInfos, out asyncContinuations);
            Assert.AreEqual(result, logEventInfos.Length);
            Assert.AreEqual(result, asyncContinuations.Length);

            Assert.AreEqual(0, result);
            Assert.AreEqual(0, queue.RequestCount);
        }
    }
}
