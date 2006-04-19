// 
// Copyright (c) 2004-2006 Jaroslaw Kowalski <jaak@jkowalski.net>
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

#if !NETCF

using System;
using System.Collections;

namespace NLog.Internal
{
    /// <summary>
    /// Asynchronous request queue
    /// </summary>
	public class AsyncRequestQueue
	{
        /// <summary>
        /// The action to be taken when the queue overflows
        /// </summary>
		public enum OverflowAction
		{
            /// <summary>
            /// Do no action - accept another item into the queue.
            /// </summary>
			None,

            /// <summary>
            /// Discard the overflowing item.
            /// </summary>
			Discard,
			
            /// <summary>
            /// Block until there's more room in the queue.
            /// </summary>
            Block,
		}

		private Queue _queue = new Queue();
        private int _batchedItems = 0;
		private OverflowAction _overflowAction = OverflowAction.Discard;
		private int _requestLimit = 10000;

        /// <summary>
        /// Creates a new instance of <see cref="AsyncRequestQueue"/> and
        /// sets the request limit and overflow action.
        /// </summary>
        /// <param name="requestLimit">Request limit.</param>
        /// <param name="overflowAction">The overflow action.</param>
		public AsyncRequestQueue(int requestLimit, OverflowAction overflowAction)
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
		public OverflowAction OnOverflow
		{
			get { return _overflowAction; }
			set { _overflowAction = value; }
		}

        /// <summary>
        /// Enqueues another item. If the queue is overflown the appropriate
        /// action is taken as specified by <see cref="OnOverflow"/>.
        /// </summary>
        /// <param name="o">The item to be queued.</param>
		public void Enqueue(object o)
		{
			lock (this)
			{
                if (_queue.Count >= RequestLimit)
                {
                    switch (OnOverflow)
                    {
                        case OverflowAction.Discard:
                            return;

                        case OverflowAction.None:
                            break;

                        case OverflowAction.Block:
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
		public ArrayList DequeueBatch(int count)
		{
            ArrayList target = new ArrayList();
			lock (this)
			{
				for (int i = 0; i < count; ++i)
				{
					if (_queue.Count <= 0)
						break;

					object o = _queue.Dequeue();

					target.Add(o);
				}
                System.Threading.Monitor.PulseAll(this);
			}
            _batchedItems = target.Count;
            return target;
		}

        /// <summary>
        /// Notifies the queue that the request batch has been processed.
        /// </summary>
        /// <param name="batch">The batch.</param>
        public void BatchProcessed(ArrayList batch)
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

#endif
