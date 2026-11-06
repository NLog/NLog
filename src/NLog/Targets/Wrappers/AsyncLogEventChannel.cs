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

namespace NLog.Targets.Wrappers
{
    using System;
    using System.Collections.Generic;
    using NLog.Common;

    /// <summary>
    /// Bounded channel between the logger threads that write log events (producers)
    /// and the single consumer that reads them in batches for <see cref="AsyncTargetWrapper"/>.
    /// </summary>
    /// <remarks>
    /// Owns the request queue, which applies <see cref="QueueLimit"/> and <see cref="OverflowAction"/>.
    /// The channel is passive: it never calls the consumer, the caller of <see cref="Write"/> decides what to do
    /// when the queue was empty.
    /// </remarks>
    internal sealed class AsyncLogEventChannel
    {
        private AsyncRequestQueueBase _queue;
        private EventHandler<LogEventDroppedEventArgs>? _logEventDropped;
        private EventHandler<LogEventQueueGrowEventArgs>? _logEventQueueGrow;

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncLogEventChannel"/> class.
        /// </summary>
        /// <param name="queueLimit">Maximum number of log events in the queue.</param>
        /// <param name="overflowAction">The action to be taken when the queue overflows.</param>
        public AsyncLogEventChannel(int queueLimit, AsyncTargetWrapperOverflowAction overflowAction)
        {
#if NETFRAMEWORK
            _queue = new AsyncRequestQueue(queueLimit, overflowAction);
#else
            // NetStandard20 includes many optimizations for ConcurrentQueue:
            //  - See: https://blogs.msdn.microsoft.com/dotnet/2017/06/07/performance-improvements-in-net-core/
            // Net40 ConcurrencyQueue can seem to leak, because it doesn't clear properly on dequeue
            //  - See: https://blogs.msdn.microsoft.com/pfxteam/2012/05/08/concurrentqueuet-holding-on-to-a-few-dequeued-elements/
            _queue = new ConcurrentRequestQueue(queueLimit, overflowAction);
#endif
        }

        /// <summary>
        /// Gets or sets the maximum number of log events in the queue.
        /// </summary>
        public int QueueLimit
        {
            get => _queue.QueueLimit;
            set => _queue.QueueLimit = value;
        }

        /// <summary>
        /// Gets or sets the action to be taken when the queue is full and another log event is written.
        /// </summary>
        public AsyncTargetWrapperOverflowAction OverflowAction
        {
            get => _queue.OnOverflow;
            set => _queue.OnOverflow = value;
        }

        /// <summary>
        /// Gets whether the queue is empty.
        /// </summary>
        public bool IsEmpty => _queue.IsEmpty;

        /// <summary>
        /// Gets whether the queue uses Monitor.Lock, instead of a lock-free concurrent queue.
        /// </summary>
        public bool IsLockingQueue => _queue is AsyncRequestQueue;

        /// <summary>
        /// Occurs when a log event has been dropped, because the queue is full and <see cref="OverflowAction"/> is <see cref="AsyncTargetWrapperOverflowAction.Discard"/>.
        /// </summary>
        public event EventHandler<LogEventDroppedEventArgs> LogEventDropped
        {
            add
            {
                if (_logEventDropped is null)
                    _queue.LogEventDropped += OnQueueLogEventDropped;
                _logEventDropped += value;
            }
            remove
            {
                _logEventDropped -= value;
                if (_logEventDropped is null)
                    _queue.LogEventDropped -= OnQueueLogEventDropped;
            }
        }

        /// <summary>
        /// Occurs when the queue grows, because it is full and <see cref="OverflowAction"/> is <see cref="AsyncTargetWrapperOverflowAction.Grow"/>.
        /// </summary>
        public event EventHandler<LogEventQueueGrowEventArgs> LogEventQueueGrow
        {
            add
            {
                if (_logEventQueueGrow is null)
                    _queue.LogEventQueueGrow += OnQueueLogEventQueueGrow;
                _logEventQueueGrow += value;
            }
            remove
            {
                _logEventQueueGrow -= value;
                if (_logEventQueueGrow is null)
                    _queue.LogEventQueueGrow -= OnQueueLogEventQueueGrow;
            }
        }

        /// <summary>
        /// Switches between the locking queue and the lock-free concurrent queue, keeping configuration and event subscriptions.
        /// </summary>
        /// <remarks>
        /// Only call while no log events are queued (during initialization). Queued log events are not moved.
        /// </remarks>
        /// <param name="locking">Use the locking queue.</param>
        public void UseLockingQueue(bool locking)
        {
#if !NET35
            if (locking == IsLockingQueue)
                return;

            var previousQueue = _queue;
            AsyncRequestQueueBase newQueue = locking
                ? new AsyncRequestQueue(previousQueue.QueueLimit, previousQueue.OnOverflow)
                : new ConcurrentRequestQueue(previousQueue.QueueLimit, previousQueue.OnOverflow);

            if (_logEventDropped != null)
            {
                previousQueue.LogEventDropped -= OnQueueLogEventDropped;
                newQueue.LogEventDropped += OnQueueLogEventDropped;
            }

            if (_logEventQueueGrow != null)
            {
                previousQueue.LogEventQueueGrow -= OnQueueLogEventQueueGrow;
                newQueue.LogEventQueueGrow += OnQueueLogEventQueueGrow;
            }

            _queue = newQueue;
#endif
        }

        /// <summary>
        /// Writes a log event to the queue, applying <see cref="OverflowAction"/> when the queue is full.
        /// </summary>
        /// <param name="logEvent">The log event.</param>
        /// <returns><see langword="true"/> when the queue was empty before the write.</returns>
        public bool Write(AsyncLogEventInfo logEvent) => _queue.Enqueue(logEvent);

        /// <summary>
        /// Reads at most <paramref name="count"/> log events into a preallocated list.
        /// </summary>
        /// <param name="count">Maximum number of log events to read.</param>
        /// <param name="result">List that receives the log events.</param>
        public void ReadBatch(int count, IList<AsyncLogEventInfo> result) => _queue.DequeueBatch(count, result);

        /// <summary>
        /// Reads all queued log events.
        /// </summary>
        /// <returns>The log events.</returns>
        public AsyncLogEventInfo[] ReadAll() => _queue.DequeueBatch(int.MaxValue);

        /// <summary>
        /// Removes all queued log events, and releases writers blocked by <see cref="AsyncTargetWrapperOverflowAction.Block"/>.
        /// </summary>
        public void Clear() => _queue.Clear();

        private void OnQueueLogEventDropped(object? sender, LogEventDroppedEventArgs eventArgs) => _logEventDropped?.Invoke(this, eventArgs);

        private void OnQueueLogEventQueueGrow(object? sender, LogEventQueueGrowEventArgs eventArgs) => _logEventQueueGrow?.Invoke(this, eventArgs);
    }
}
