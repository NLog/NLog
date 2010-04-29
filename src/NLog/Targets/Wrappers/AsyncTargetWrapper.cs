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
    /// The action to be taken when the queue overflows
    /// </summary>
    public enum AsyncTargetWrapperOverflowAction
    {
        /// <summary>
        /// Grow the queue.
        /// </summary>
        Grow,

        /// <summary>
        /// Discard the overflowing item.
        /// </summary>
        Discard,

#if !NETCF
        /// <summary>
        /// Block until there's more room in the queue.
        /// </summary>
        Block,
#endif
    }

    /// <summary>
    /// A target wrapper that provides asynchronous, buffered execution of target writes.
    /// </summary>
    /// <remarks>
    /// <p>
    /// Asynchronous target wrapper allows the logger code to execute more quickly, by queueing
    /// messages and processing them in a separate thread. You should wrap targets
    /// that spend a non-trivial amount of time in their Write() method with asynchronous
    /// target to speed up logging.
    /// </p>
    /// <p>
    /// Because asynchronous logging is quite a common scenario, NLog supports a
    /// shorthand notation for wrapping all targets with AsyncWrapper. Just add async="true" to
    /// the &lt;targets/&gt; element in the configuration file.
    /// </p>
    /// <code lang="XML">
    /// <![CDATA[
    /// <targets async="true">
    ///    ... your targets go here ...
    /// </targets>
    /// ]]></code>
    /// </remarks>
    /// <example>
    /// <p>
    /// To set up the target in the <a href="config.html">configuration file</a>, 
    /// use the following syntax:
    /// </p>
    /// <code lang="XML" src="examples/targets/Configuration File/AsyncWrapper/NLog.config" />
    /// <p>
    /// The above examples assume just one target and a single rule. See below for
    /// a programmatic configuration that's equivalent to the above config file:
    /// </p>
    /// <code lang="C#" src="examples/targets/Configuration API/AsyncWrapper/Wrapping File/Example.cs" />
    /// </example>
    [Target("AsyncWrapper",IsWrapper=true)]
    [NotSupportedRuntime(Framework=RuntimeFramework.DotNetCompactFramework)]
    public class AsyncTargetWrapper: WrapperTargetBase
    {
        private int _batchSize = 100;
        private int _timeToSleepBetweenBatches = 50;

        /// <summary>
        /// Creates a new instance of <see cref="AsyncTargetWrapper"/>.
        /// </summary>
        public AsyncTargetWrapper()
        {
        }

        /// <summary>
        /// Creates a new instance of <see cref="AsyncTargetWrapper"/>
        /// which wraps the specified target.
        /// </summary>
        /// <param name="wrappedTarget">The target to be wrapped.</param>
        public AsyncTargetWrapper(Target wrappedTarget)
        {
            WrappedTarget = wrappedTarget;
        }

        /// <summary>
        /// Creates a new instance of <see cref="AsyncTargetWrapper"/>
        /// which wraps the specified target.
        /// </summary>
        /// <param name="wrappedTarget">The target to be wrapped.</param>
        /// <param name="queueLimit">Maximum number of requests in the queue.</param>
        /// <param name="overflowAction">The action to be taken when the queue overflows.</param>
        public AsyncTargetWrapper(Target wrappedTarget, int queueLimit, AsyncTargetWrapperOverflowAction overflowAction)
        {
            WrappedTarget = wrappedTarget;
            QueueLimit = queueLimit;
            OverflowAction = overflowAction;
        }

        /// <summary>
        /// Initializes the target by starting the lazy writer thread.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();
            StartLazyWriterTimer();
        }

        /// <summary>
        /// Closes the target by stopping the lazy writer thread.
        /// </summary>
        protected internal override void Close()
        {
            StopLazyWriterThread();
            base.Close();
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

        private object _inLazyWriterMonitor = new object();
        private bool _flushAll = false;

        private void LazyWriterTimerCallback(object state)
        {
            lock (_inLazyWriterMonitor)
            {
                try
                {
                    do
                    {
                        // Console.WriteLine("q: {0}", RequestQueue.RequestCount);
                        ArrayList pendingRequests = RequestQueue.DequeueBatch(BatchSize);

                        try
                        {
                            if (pendingRequests.Count == 0)
                                break;

                            LogEventInfo[] events = (LogEventInfo[])pendingRequests.ToArray(typeof(LogEventInfo));
                            WrappedTarget.Write(events);
                        }
                        finally
                        {
                            RequestQueue.BatchProcessed(pendingRequests);
                        }
                    } while (_flushAll);
                }
                catch (Exception ex)
                {
                    InternalLogger.Error("Error in lazy writer timer procedure: {0}", ex);
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

        private Timer _lazyWriterTimer = null;
        private AsyncRequestQueue _lazyWriterRequestQueue = new AsyncRequestQueue(10000, AsyncTargetWrapperOverflowAction.Discard);

        /// <summary>
        /// The queue of lazy writer thread requests.
        /// </summary>
        protected AsyncRequestQueue RequestQueue
        {
            get { return _lazyWriterRequestQueue; }
        }

        /// <summary>
        /// The action to be taken when the lazy writer thread request queue count
        /// exceeds the set limit.
        /// </summary>
        [System.ComponentModel.DefaultValue("Discard")]
        public AsyncTargetWrapperOverflowAction OverflowAction
        {
            get { return _lazyWriterRequestQueue.OnOverflow; }
            set { _lazyWriterRequestQueue.OnOverflow = value; }
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
        protected virtual void StartLazyWriterTimer()
        {
            _lazyWriterRequestQueue.Clear();
            Internal.InternalLogger.Debug("Starting lazy writer timer...");
            _lazyWriterTimer = new Timer(new TimerCallback(LazyWriterTimerCallback), null, 0, this.TimeToSleepBetweenBatches);
        }

        /// <summary>
        /// Starts the lazy writer thread.
        /// </summary>
        protected virtual void StopLazyWriterThread()
        {
            if (_lazyWriterTimer == null)
                return;

            Flush();
            _lazyWriterTimer.Change(0, 0);
            _lazyWriterTimer.Dispose();
            _lazyWriterTimer = null;

            _lazyWriterRequestQueue.Clear();
        }

        /// <summary>
        /// Waits for the lazy writer thread to finish writing messages.
        /// </summary>
        public override void Flush(TimeSpan timeout)
        {
            _lazyWriterTimer.Change(Timeout.Infinite, Timeout.Infinite);
            lock (_inLazyWriterMonitor)
            {
                _flushAll = true;
                LazyWriterTimerCallback(null);
            }
            _lazyWriterTimer.Change(TimeToSleepBetweenBatches, TimeToSleepBetweenBatches);
        }

        /// <summary>
        /// Asynchronous request queue
        /// </summary>
        public class AsyncRequestQueue
        {
            private Queue _queue = new Queue();
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
            public void Enqueue(object o)
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

#if !NETCF
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
#if !NETCF
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
}
