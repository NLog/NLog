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

namespace NLog.Targets.Wrappers
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Threading;
    using NLog.Common;

    /// <summary>
    /// Concurrent Asynchronous request queue based on <see cref="ConcurrentQueue{T}"/>
    /// </summary>
	internal sealed class ConcurrentRequestQueue : AsyncRequestQueueBase
    {
        private readonly ConcurrentQueue<AsyncLogEventInfo> _logEventInfoQueue = new ConcurrentQueue<AsyncLogEventInfo>();

        /// <summary>
        /// Initializes a new instance of the AsyncRequestQueue class.
        /// </summary>
        /// <param name="queueLimit">Queue max size.</param>
        /// <param name="overflowAction">The overflow action.</param>
        public ConcurrentRequestQueue(int queueLimit, AsyncTargetWrapperOverflowAction overflowAction)
        {
            QueueLimit = queueLimit;
            OnOverflow = overflowAction;
        }

        public override bool IsEmpty => _logEventInfoQueue.IsEmpty && Interlocked.Read(ref _count) == 0;

        /// <summary>
        /// Gets the number of requests currently in the queue.
        /// </summary>
        /// <remarks>
        /// Only for debugging purposes
        /// </remarks>
        public int Count => (int)_count;
        private long _count;

        /// <summary>
        /// Enqueues another item. If the queue is overflown the appropriate
        /// action is taken as specified by <see cref="AsyncRequestQueueBase.OnOverflow"/>.
        /// </summary>
        /// <param name="logEventInfo">The log event info.</param>
        /// <returns>Queue was empty before enqueue</returns>
        public override bool Enqueue(AsyncLogEventInfo logEventInfo)
        {
            long currentCount = Interlocked.Increment(ref _count);
            bool queueWasEmpty = currentCount == 1;  // Inserted first item in empty queue
            if (currentCount > QueueLimit)
            {
                switch (OnOverflow)
                {
                    case AsyncTargetWrapperOverflowAction.Discard:
                        {
                            queueWasEmpty = DequeueUntilBelowRequestLimit();
                        }
                        break;
                    case AsyncTargetWrapperOverflowAction.Block:
                        {
                            queueWasEmpty = WaitForBelowRequestLimit();
                        }
                        break;
                    case AsyncTargetWrapperOverflowAction.Grow:
                        {
                            InternalLogger.Debug("AsyncQueue - Growing the size of queue, because queue is full");
                            var currentQueueLimit = QueueLimit;
                            if (currentCount > currentQueueLimit)
                            {
                                QueueLimit = currentQueueLimit * 2;
                            }
                            OnLogEventQueueGrows(currentCount);
                        }
                        break;
                }
            }
            _logEventInfoQueue.Enqueue(logEventInfo);
            return queueWasEmpty;
        }

        private bool DequeueUntilBelowRequestLimit()
        {
            long currentCount;
            bool queueWasEmpty = false;

            do
            {
                if (_logEventInfoQueue.TryDequeue(out var lostItem))
                {
                    InternalLogger.Debug("AsyncQueue - Discarding single item, because queue is full");
                    queueWasEmpty = Interlocked.Decrement(ref _count) == 1 || queueWasEmpty;
                    OnLogEventDropped(lostItem.LogEvent);
                    break;
                }
                currentCount = Interlocked.Read(ref _count);
                queueWasEmpty = true;
            } while (currentCount > QueueLimit);

            return queueWasEmpty;
        }

        private bool WaitForBelowRequestLimit()
        {
            // Attempt to yield using SpinWait
            long currentCount = TrySpinWaitForLowerCount();

            // If yield did not help, then wait on a lock
            if (currentCount > QueueLimit)
            {
                InternalLogger.Debug("AsyncQueue - Blocking until ready, because queue is full");
                lock (_logEventInfoQueue)
                {
                    InternalLogger.Trace("AsyncQueue - Entered critical section.");
                    currentCount = Interlocked.Read(ref _count);
                    while (currentCount > QueueLimit)
                    {
                        Interlocked.Decrement(ref _count);
                        Monitor.Wait(_logEventInfoQueue);
                        InternalLogger.Trace("AsyncQueue - Entered critical section.");
                        currentCount = Interlocked.Increment(ref _count);
                    }
                }
            }

            InternalLogger.Trace("AsyncQueue - Limit ok.");
            return true;
        }

        private long TrySpinWaitForLowerCount()
        {
            long currentCount = 0;
            bool firstYield = true;
            SpinWait spinWait = new SpinWait();
            for (int i = 0; i < 15; ++i)
            {
                if (spinWait.NextSpinWillYield)
                {
                    if (firstYield)
                        InternalLogger.Debug("AsyncQueue - Blocking with yield, because queue is full");
                    firstYield = false;
                }

                spinWait.SpinOnce();
                currentCount = Interlocked.Read(ref _count);
                if (currentCount <= QueueLimit)
                    break;
            }

            return currentCount;
        }

        /// <summary>
        /// Dequeues a maximum of <c>count</c> items from the queue
        /// and adds returns the list containing them.
        /// </summary>
        /// <param name="count">Maximum number of items to be dequeued</param>
        /// <returns>The array of log events.</returns>
        public override AsyncLogEventInfo[] DequeueBatch(int count)
        {
            if (_logEventInfoQueue.IsEmpty)
                return Internal.ArrayHelper.Empty<AsyncLogEventInfo>();

            if (_count < count)
                count = Math.Min(count, Count);

            var resultEvents = new List<AsyncLogEventInfo>(count);

            DequeueBatch(count, resultEvents);

            if (resultEvents.Count == 0)
                return Internal.ArrayHelper.Empty<AsyncLogEventInfo>();
            else
                return resultEvents.ToArray();
        }

        /// <summary>
        /// Dequeues into a preallocated array, instead of allocating a new one
        /// </summary>
        /// <param name="count">Maximum number of items to be dequeued</param>
        /// <param name="result">Preallocated list</param>
        public override void DequeueBatch(int count, IList<AsyncLogEventInfo> result)
        {
            bool dequeueBatch = OnOverflow == AsyncTargetWrapperOverflowAction.Block;

            for (int i = 0; i < count; ++i)
            {
                if (_logEventInfoQueue.TryDequeue(out var item))
                {
                    if (!dequeueBatch)
                        Interlocked.Decrement(ref _count);
                    result.Add(item);
                }
                else
                {
                    count = i;
                    break;
                }
            }

            if (dequeueBatch)
            {
                lock (_logEventInfoQueue)
                {
                    Interlocked.Add(ref _count, -count);
                    Monitor.PulseAll(_logEventInfoQueue);
                }
            }
        }

        /// <summary>
        /// Clears the queue.
        /// </summary>
        public override void Clear()
        {
            while (!_logEventInfoQueue.IsEmpty)
                _logEventInfoQueue.TryDequeue(out var _);
            Interlocked.Exchange(ref _count, 0);

            if (OnOverflow == AsyncTargetWrapperOverflowAction.Block)
            {
                // Try to eject any threads, that are blocked in the RequestQueue
                lock (_logEventInfoQueue)
                {
                    Monitor.PulseAll(_logEventInfoQueue);
                }
            }
        }
    }
}
#endif
