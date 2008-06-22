using System.Collections.Generic;
using NLog.Internal;
namespace NLog.Targets.Wrappers
{
    /// <summary>
    /// Asynchronous request queue
    /// </summary>
    public class AsyncRequestQueue<T>
    {
        private Queue<T> _queue = new Queue<T>();
        private int _batchedItems = 0;
        private AsyncTargetWrapperOverflowAction _overflowAction = AsyncTargetWrapperOverflowAction.Discard;
        private int _requestLimit = 10000;

        /// <summary>
        /// Creates a new instance of <see cref="AsyncRequestQueue"/> and
        /// sets the request limit and overflow action.
        /// </summary>
        /// <param name="requestLimit">Request limit.</param>
        /// <param name="overflowAction">The overflow action.</param>
        public AsyncRequestQueue(int requestLimit, AsyncTargetWrapperOverflowAction overflowAction)
        {
            _requestLimit = requestLimit;
            _overflowAction = overflowAction;
        }

        /// <summary>
        /// The request limit.
        /// </summary>
        public int RequestLimit
        {
            get { return _requestLimit; }
            set { _requestLimit = value; }
        }

        /// <summary>
        /// Action to be taken when there's no more room in
        /// the queue and another request is enqueued.
        /// </summary>
        public AsyncTargetWrapperOverflowAction OnOverflow
        {
            get { return _overflowAction; }
            set { _overflowAction = value; }
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
                if (_queue.Count >= RequestLimit)
                {
                    switch (OnOverflow)
                    {
                        case AsyncTargetWrapperOverflowAction.Discard:
                            return;

                        case AsyncTargetWrapperOverflowAction.Grow:
                            break;

#if !NET_CF
                        case AsyncTargetWrapperOverflowAction.Block:
                            while (_queue.Count >= RequestLimit)
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
                _queue.Enqueue(o);
            }
        }

        /// <summary>
        /// Dequeues a maximum of <c>count</c> items from the queue
        /// and adds returns the <see cref="ArrayList"/> containing them.
        /// </summary>
        /// <param name="count">Maximum number of items to be dequeued.</param>
        public List<T> DequeueBatch(int count)
        {
            List<T> target = new List<T>(count);
            lock (this)
            {
                for (int i = 0; i < count; ++i)
                {
                    if (_queue.Count <= 0)
                        break;

                    T o = _queue.Dequeue();

                    target.Add(o);
                }
#if !NET_CF
                if (OnOverflow == AsyncTargetWrapperOverflowAction.Block)
                    System.Threading.Monitor.PulseAll(this);
#endif
            }
            _batchedItems = target.Count;
            return target;
        }

        /// <summary>
        /// Notifies the queue that the request batch has been processed.
        /// </summary>
        /// <param name="batch">The batch.</param>
        public void BatchProcessed(ICollection<T> batch)
        {
            _batchedItems = 0;
        }

        /// <summary>
        /// Clears the queue.
        /// </summary>
        public void Clear()
        {
            lock (this)
            {
                _queue.Clear();
            }
        }

        /// <summary>
        /// Number of requests currently in the queue.
        /// </summary>
        public int RequestCount
        {
            get { return _queue.Count; }
        }

        /// <summary>
        /// Number of requests currently being processed (in the queue + batched)
        /// </summary>
        public int UnprocessedRequestCount
        {
            get { return _queue.Count + _batchedItems; }
        }
    }

}
