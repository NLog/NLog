using System.Collections.Generic;
using NLog.Internal;

namespace NLog.Targets.Wrappers
{
    using System;

    /// <summary>
    /// Asynchronous request queue.
    /// </summary>
    /// <typeparam name="T">
    /// Item type.
    /// </typeparam>
    public class AsyncRequestQueue<T>
    {
        private Queue<T> queue = new Queue<T>();
        private int batchedItems = 0;

        /// <summary>
        /// Initializes a new instance of the AsyncRequestQueue class.
        /// </summary>
        /// <param name="requestLimit">Request limit.</param>
        /// <param name="overflowAction">The overflow action.</param>
        public AsyncRequestQueue(int requestLimit, AsyncTargetWrapperOverflowAction overflowAction)
        {
            this.RequestLimit = requestLimit;
            this.OnOverflow = overflowAction;
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
        /// Gets the number of requests currently in the queue.
        /// </summary>
        public int RequestCount
        {
            get { return this.queue.Count; }
        }

        /// <summary>
        /// Gets the number of requests currently being processed (in the queue + batched).
        /// </summary>
        public int UnprocessedRequestCount
        {
            get { return this.queue.Count + this.batchedItems; }
        }

        /// <summary>
        /// Enqueues another item. If the queue is overflown the appropriate
        /// action is taken as specified by <see cref="OnOverflow"/>.
        /// </summary>
        /// <param name="o">The item to be queued.</param>
        public void Enqueue(T o)
        {
            lock (this)
            {
                if (this.queue.Count >= this.RequestLimit)
                {
                    switch (this.OnOverflow)
                    {
                        case AsyncTargetWrapperOverflowAction.Discard:
                            return;

                        case AsyncTargetWrapperOverflowAction.Grow:
                            break;

#if !NET_CF
                        case AsyncTargetWrapperOverflowAction.Block:
                            while (this.queue.Count >= this.RequestLimit)
                            {
                                InternalLogger.Debug("Blocking...");
                                if (System.Threading.Monitor.Wait(this))
                                {
                                    InternalLogger.Debug("Entered critical section.");
                                }
                                else
                                {
                                    InternalLogger.Debug("Failed to enter critical section.");
                                }
                            }

                            InternalLogger.Debug("Limit ok.");
                            break;
#endif
                    }
                }

                this.queue.Enqueue(o);
            }
        }

        /// <summary>
        /// Dequeues a maximum of <c>count</c> items from the queue
        /// and adds returns the list containing them.
        /// </summary>
        /// <param name="count">Maximum number of items to be dequeued.</param>
        /// <returns>List of dequeued items.</returns>
        public List<T> DequeueBatch(int count)
        {
            List<T> target = new List<T>(count);
            lock (this)
            {
                for (int i = 0; i < count; ++i)
                {
                    if (this.queue.Count <= 0)
                    {
                        break;
                    }

                    T o = this.queue.Dequeue();

                    target.Add(o);
                }
#if !NET_CF
                if (this.OnOverflow == AsyncTargetWrapperOverflowAction.Block)
                {
                    System.Threading.Monitor.PulseAll(this);
                }
#endif
            }

            this.batchedItems = target.Count;
            return target;
        }

        /// <summary>
        /// Notifies the queue that the request batch has been processed.
        /// </summary>
        /// <param name="batch">The batch.</param>
        public void BatchProcessed(ICollection<T> batch)
        {
            this.batchedItems = 0;
        }

        /// <summary>
        /// Clears the queue.
        /// </summary>
        public void Clear()
        {
            lock (this)
            {
                this.queue.Clear();
            }
        }
    }
}
