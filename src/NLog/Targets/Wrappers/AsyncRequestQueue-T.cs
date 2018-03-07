// 
// Copyright (c) 2004-2018 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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

namespace NLog.Targets.Wrappers
{
    using System;
    using System.Collections.Generic;
    using Common;

    /// <summary>
    /// Asynchronous request queue.
    /// </summary>
	internal class AsyncRequestQueue
    {
        private readonly Queue<AsyncLogEventInfo> _logEventInfoQueue = new Queue<AsyncLogEventInfo>();

        /// <summary>
        /// Initializes a new instance of the AsyncRequestQueue class.
        /// </summary>
        /// <param name="requestLimit">Request limit.</param>
        /// <param name="overflowAction">The overflow action.</param>
        public AsyncRequestQueue(int requestLimit, AsyncTargetWrapperOverflowAction overflowAction)
        {
            RequestLimit = requestLimit;
            OnOverflow = overflowAction;
        }

        /// <summary>
        /// Gets or sets the request limit.
        /// </summary>
        public int RequestLimit { get; set; }

        /// <summary>
        /// Gets or sets the action to be taken when there's no more room in
        /// the queue and another request is enqueued.
        /// </summary>
        public AsyncTargetWrapperOverflowAction OnOverflow { get; set; }

        /// <summary>
        /// Notifies about log event that was dropped when <see cref="OnOverflow"/> setted to <see cref="AsyncTargetWrapperOverflowAction.Discard"/>
        /// </summary>
        public event EventHandler<LogEventDroppedEventArgs> LogEventDropped;

        /// <summary>
        /// Notifies when queue size is growing over <see cref="RequestLimit"/>
        /// </summary>
        public event EventHandler<LogEventQueueGrowEventArgs> LogEventQueueGrow;

        /// <summary>
        /// Gets the number of requests currently in the queue.
        /// </summary>
        public int RequestCount
        {
            get
            {
                lock (this)
                {
                    return _logEventInfoQueue.Count;
                }
            }
        }

        /// <summary>
        /// Enqueues another item. If the queue is overflown the appropriate
        /// action is taken as specified by <see cref="OnOverflow"/>.
        /// </summary>
        /// <param name="logEventInfo">The log event info.</param>
        /// <returns>Queue was empty before enqueue</returns>
        public bool Enqueue(AsyncLogEventInfo logEventInfo)
        {
            lock (this)
            {
                if (_logEventInfoQueue.Count >= RequestLimit)
                {
                    InternalLogger.Debug("Async queue is full");
                    switch (OnOverflow)
                    {
                        case AsyncTargetWrapperOverflowAction.Discard:
                            InternalLogger.Debug("Discarding one element from queue");
                            var lostItem = _logEventInfoQueue.Dequeue();
                            OnLogEventDropped(lostItem.LogEvent);
                            break;

                        case AsyncTargetWrapperOverflowAction.Grow:
                            InternalLogger.Debug("The overflow action is Grow, adding element anyway");
                            RequestLimit *= 2;
                            OnLogEventQueueGrows(RequestCount + 1);
                            break;

                        case AsyncTargetWrapperOverflowAction.Block:
                            while (_logEventInfoQueue.Count >= RequestLimit)
                            {
                                InternalLogger.Debug("Blocking because the overflow action is Block...");
                                System.Threading.Monitor.Wait(this);
                                InternalLogger.Trace("Entered critical section.");
                            }

                            InternalLogger.Trace("Limit ok.");
                            break;
                    }
                }

                _logEventInfoQueue.Enqueue(logEventInfo);
                return _logEventInfoQueue.Count == 1;
            }
        }

        /// <summary>
        /// Dequeues a maximum of <c>count</c> items from the queue
        /// and adds returns the list containing them.
        /// </summary>
        /// <param name="count">Maximum number of items to be dequeued (-1 means everything).</param>
        /// <returns>The array of log events.</returns>
        public AsyncLogEventInfo[] DequeueBatch(int count)
        {
            AsyncLogEventInfo[] resultEvents;

            lock (this)
            {
                if (_logEventInfoQueue.Count < count)
                    count = _logEventInfoQueue.Count;

                if (count == 0)
                    return Internal.ArrayHelper.Empty<AsyncLogEventInfo>();

                resultEvents = new AsyncLogEventInfo[count];
                for (int i = 0; i < count; ++i)
                {
                    resultEvents[i] = _logEventInfoQueue.Dequeue();
                }

                if (OnOverflow == AsyncTargetWrapperOverflowAction.Block)
                {
                    System.Threading.Monitor.PulseAll(this);
                }
            }

            return resultEvents;
        }

        /// <summary>
        /// Dequeues into a preallocated array, instead of allocating a new one
        /// </summary>
        /// <param name="count">Maximum number of items to be dequeued</param>
        /// <param name="result">Preallocated list</param>
        public void DequeueBatch(int count, IList<AsyncLogEventInfo> result)
        {
            lock (this)
            {
                if (_logEventInfoQueue.Count < count)
                    count = _logEventInfoQueue.Count;
                for (int i = 0; i < count; ++i)
                    result.Add(_logEventInfoQueue.Dequeue());
                if (OnOverflow == AsyncTargetWrapperOverflowAction.Block)
                {
                    System.Threading.Monitor.PulseAll(this);
                }
            }
        }

        /// <summary>
        /// Clears the queue.
        /// </summary>
        public void Clear()
        {
            lock (this)
            {
                _logEventInfoQueue.Clear();
            }
        }

        /// <summary>
        /// Raise event when queued element was dropped because of queue overflow
        /// </summary>
        /// <param name="logEventInfo">Dropped queue item</param>
        private void OnLogEventDropped(LogEventInfo logEventInfo) => LogEventDropped?.Invoke(this, new LogEventDroppedEventArgs(logEventInfo));

        /// <summary>
        /// Raise event when <see cref="RequestCount"/> overflow <see cref="RequestLimit"/>
        /// </summary>
        /// <param name="requestsCount"> current requests count</param>
        private void OnLogEventQueueGrows(int requestsCount) => LogEventQueueGrow?.Invoke(this, new LogEventQueueGrowEventArgs(RequestLimit, requestsCount));
    }
}
