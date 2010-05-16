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

namespace NLog.Targets.Wrappers
{
    using System.Collections.Generic;
    using NLog.Common;
    using NLog.Internal;

    /// <summary>
    /// Asynchronous request queue.
    /// </summary>
    public class AsyncRequestQueue
    {
        private Queue<LogEventInfo> logEventInfoQueue = new Queue<LogEventInfo>();
        private Queue<AsyncContinuation> asyncContinuationsQueue = new Queue<AsyncContinuation>();

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
            get { return this.logEventInfoQueue.Count; }
        }

        /// <summary>
        /// Enqueues another item. If the queue is overflown the appropriate
        /// action is taken as specified by <see cref="OnOverflow"/>.
        /// </summary>
        /// <param name="logEventInfo">The log event info.</param>
        /// <param name="asyncContinuation">The asynchronou continuation.</param>
        public void Enqueue(LogEventInfo logEventInfo, AsyncContinuation asyncContinuation)
        {
            lock (this)
            {
                if (this.logEventInfoQueue.Count >= this.RequestLimit)
                {
                    switch (this.OnOverflow)
                    {
                        case AsyncTargetWrapperOverflowAction.Discard:
                            return;

                        case AsyncTargetWrapperOverflowAction.Grow:
                            break;

#if !NET_CF
                        case AsyncTargetWrapperOverflowAction.Block:
                            while (this.logEventInfoQueue.Count >= this.RequestLimit)
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

                this.logEventInfoQueue.Enqueue(logEventInfo);
                this.asyncContinuationsQueue.Enqueue(asyncContinuation);
            }
        }

        /// <summary>
        /// Dequeues a maximum of <c>count</c> items from the queue
        /// and adds returns the list containing them.
        /// </summary>
        /// <param name="count">Maximum number of items to be dequeued.</param>
        /// <returns>List of dequeued items.</returns>
        public int DequeueBatch(int count, out LogEventInfo[] logEventInfos, out AsyncContinuation[] asyncContinuations)
        {
            var resultEvents = new List<LogEventInfo>();
            var resultContinutions = new List<AsyncContinuation>();

            lock (this)
            {
                for (int i = 0; i < count; ++i)
                {
                    if (this.logEventInfoQueue.Count <= 0)
                    {
                        break;
                    }

                    resultEvents.Add(this.logEventInfoQueue.Dequeue());
                    resultContinutions.Add(this.asyncContinuationsQueue.Dequeue());
                }
#if !NET_CF
                if (this.OnOverflow == AsyncTargetWrapperOverflowAction.Block)
                {
                    System.Threading.Monitor.PulseAll(this);
                }
#endif
            }

            logEventInfos = resultEvents.ToArray();
            asyncContinuations = resultContinutions.ToArray();

            return logEventInfos.Length;
        }

        /// <summary>
        /// Clears the queue.
        /// </summary>
        public void Clear()
        {
            lock (this)
            {
                this.logEventInfoQueue.Clear();
                this.asyncContinuationsQueue.Clear();
            }
        }
    }
}
