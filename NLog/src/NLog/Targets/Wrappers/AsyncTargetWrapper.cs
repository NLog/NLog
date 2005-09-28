// 
// Copyright (c) 2004,2005 Jaroslaw Kowalski <jkowalski@users.sourceforge.net>
// 
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
// * Neither the name of the Jaroslaw Kowalski nor the names of its 
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
using System.Xml;
using System.IO;
using System.Threading;
using System.Collections;
using System.Collections.Specialized;

using NLog;
using NLog.Config;

using NLog.Internal;

namespace NLog.Targets.Wrappers
{
    /// <summary>
    /// A target wrapper that provides asynchronous, buffered execution of target writes.
    /// </summary>
    [Target("AsyncWrapper")]
    public class AsyncTargetWrapper: WrapperTargetBase
    {
        private int _batchSize = 100;
        private int _timeToSleepBetweenBatches = 50;

        /// <summary>
        /// Initializes the target by starting the lazy writer thread.
        /// </summary>
        public override void Initialize()
        {
            StartLazyWriterThread();
        }

        /// <summary>
        /// Closes the target by stopping the lazy writer thread.
        /// </summary>
        protected internal override void Close()
        {
            StopLazyWriterThread();
        }

        /// <summary>
        /// The number of log events that should be processed in a batch
        /// by the lazy writer thread.
        /// </summary>
        [System.ComponentModel.DefaultValue(100)]
        public int BatchSize
        {
            get { return _batchSize; }
            set { _batchSize = value; }
        }

        /// <summary>
        /// The time in milliseconds to sleep between batches.
        /// </summary>
        [System.ComponentModel.DefaultValue(50)]
        public int TimeToSleepBetweenBatches
        {
            get { return _timeToSleepBetweenBatches; }
            set { _timeToSleepBetweenBatches = value; }
        }

        private void LazyWriterThreadProc()
        {
            while (!LazyWriterThreadStopRequested)
            {
                ArrayList pendingRequests = RequestQueue.DequeueBatch(BatchSize);

                if (pendingRequests.Count == 0)
                {
                    Thread.Sleep(TimeToSleepBetweenBatches);
                    continue;
                }

                try
                {
                    LogEventInfo[] events = (LogEventInfo[])pendingRequests.ToArray(typeof(LogEventInfo));
                    WrappedTarget.Write(events);
                }
                catch (Exception ex)
                {
                    InternalLogger.Error("Error in lazy writer thread: {0}", ex);
                }
                finally
                {
                    RequestQueue.BatchProcessed(pendingRequests);
                }
            }
        }

        /// <summary>
        /// Adds the log event to asynchronous queue to be processed by 
        /// the lazy writer thread.
        /// </summary>
        /// <param name="logEvent">The log event.</param>
        /// <remarks>
        /// The <see cref="Target.PrecalculateVolatileLayouts"/> is called
        /// to ensure that the log event can be processed in another thread.
        /// </remarks>
        protected internal override void Write(LogEventInfo logEvent)
        {
            WrappedTarget.PrecalculateVolatileLayouts(logEvent);
            RequestQueue.Enqueue(logEvent);
        }

        private bool _stopLazyWriterThread = false;
        private Thread _lazyWriterThread = null;
        private AsyncRequestQueue _lazyWriterRequestQueue = new AsyncRequestQueue(10000, AsyncRequestQueue.OverflowAction.Discard);

        /// <summary>
        /// Checks whether lazy writer thread is requested to stop.
        /// </summary>
        protected bool LazyWriterThreadStopRequested
        {
            get { return _stopLazyWriterThread; }
        }

        /// <summary>
        /// The queue of lazy writer thread requests.
        /// </summary>
        protected AsyncRequestQueue RequestQueue
        {
            get { return _lazyWriterRequestQueue; }
        }

        /// <summary>
        /// The thread that is used to write log messages.
        /// </summary>
        protected Thread LazyWriterThread
        {
            get { return _lazyWriterThread; }
        }

		/// <summary>
		/// The action to be taken when the lazy writer thread request queue count
		/// exceeds the set limit.
		/// </summary>
		/// <remarks>
		/// Can be <c>none</c> - do nothing, add another request to the queue
		/// <c>discard</c> - discard the request, <c>block</c> - block the logging
		/// thread to give the processing thread more time.
		/// </remarks>
		[System.ComponentModel.DefaultValue("discard")]
		public string OverflowAction
		{
			get { return _lazyWriterRequestQueue.OverflowActionString; }
			set { _lazyWriterRequestQueue.OverflowActionString = value; }
		}

		/// <summary>
		/// The limit on the number of requests in the lazy writer thread request queue.
		/// </summary>
		[System.ComponentModel.DefaultValue(10000)]
		public int QueueLimit
		{
			get { return _lazyWriterRequestQueue.RequestLimit; }
			set { _lazyWriterRequestQueue.RequestLimit = value; }
		}

        /// <summary>
        /// Starts the lazy writer thread which periodically writes
        /// queued log messages.
        /// </summary>
        protected virtual void StartLazyWriterThread()
        {
            if (_lazyWriterThread != null)
            {
                // already started.
                return;
            }

            _stopLazyWriterThread = false;
			_lazyWriterRequestQueue.Clear();
            Internal.InternalLogger.Debug("Starting logging thread.");
            _lazyWriterThread = new Thread(new ThreadStart(LazyWriterThreadProc));
            _lazyWriterThread.IsBackground = true;
            _lazyWriterThread.Start();
        }

        /// <summary>
        /// Starts the lazy writer thread.
        /// </summary>
        protected virtual void StopLazyWriterThread()
        {
            if (_lazyWriterThread == null)
                return;

            Flush();
            _stopLazyWriterThread = true;
            if (!_lazyWriterThread.Join(3000))
            {
                InternalLogger.Warn("Logging thread failed to stop. Aborting.");
                _lazyWriterThread.Abort();
            }
            else
            {
                InternalLogger.Debug("Logging thread stopped.");
            }
            _lazyWriterRequestQueue.Clear();
        }
        
        /// <summary>
        /// Waits for the lazy writer thread to finish writing messages.
        /// </summary>
        public override void Flush(TimeSpan timeout)
        {
            InternalLogger.Debug("Flushing lazy writer thread. Requests in queue: {0}", _lazyWriterRequestQueue.UnprocessedRequestCount);

            DateTime deadLine = DateTime.MaxValue;
            if (timeout != TimeSpan.MaxValue)
                deadLine = DateTime.Now.Add(timeout);

            InternalLogger.Debug("Waiting until {0}", deadLine);

            while (_lazyWriterRequestQueue.UnprocessedRequestCount > 0 && DateTime.Now < deadLine)
            {
                int before = _lazyWriterRequestQueue.UnprocessedRequestCount;
                int after = before;

                // we wait max. 5 seconds, and if there's no queue activity
                // we give up
                for (int i = 0; i < 100 && DateTime.Now < deadLine; ++i)
                {
                    Thread.Sleep(50);
                    after = _lazyWriterRequestQueue.UnprocessedRequestCount;
                    if (after != before)
                        break;
                }
                if (after == before)
                {
                    InternalLogger.Debug("Aborting flush because of a possible lazy writer thread lockup. Requests in queue: {0}", _lazyWriterRequestQueue.RequestCount);
                    // some lockup - quit the thread
                    break;
                }
            }
            InternalLogger.Debug("After flush. Requests in queue: {0}", _lazyWriterRequestQueue.RequestCount);
        }
    }
}

#endif
