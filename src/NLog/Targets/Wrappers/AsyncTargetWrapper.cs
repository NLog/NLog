// 
// Copyright (c) 2004-2009 Jaroslaw Kowalski <jaak@jkowalski.net>
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
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using NLog.Common;

namespace NLog.Targets.Wrappers
{
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
    /// <code lang="XML" source="examples/targets/Configuration File/AsyncWrapper/NLog.config" />
    /// <p>
    /// The above examples assume just one target and a single rule. See below for
    /// a programmatic configuration that's equivalent to the above config file:
    /// </p>
    /// <code lang="C#" source="examples/targets/Configuration API/AsyncWrapper/Wrapping File/Example.cs" />
    /// </example>
    [Target("AsyncWrapper", IsWrapper = true)]
    public class AsyncTargetWrapper : WrapperTargetBase
    {
        private readonly object inLazyWriterMonitor = new object();
        private bool flushAll;
        private Timer lazyWriterTimer;

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncTargetWrapper" /> class.
        /// </summary>
        public AsyncTargetWrapper()
        {
            this.RequestQueue = new AsyncRequestQueue<LogEventInfo>(10000, AsyncTargetWrapperOverflowAction.Discard);
            this.TimeToSleepBetweenBatches = 50;
            this.BatchSize = 100;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncTargetWrapper" /> class.
        /// </summary>
        /// <param name="wrappedTarget">The wrapped target.</param>
        public AsyncTargetWrapper(Target wrappedTarget)
        {
            this.RequestQueue = new AsyncRequestQueue<LogEventInfo>(10000, AsyncTargetWrapperOverflowAction.Discard);
            this.TimeToSleepBetweenBatches = 50;
            this.BatchSize = 100;
            this.WrappedTarget = wrappedTarget;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncTargetWrapper" /> class.
        /// </summary>
        /// <param name="wrappedTarget">The wrapped target.</param>
        /// <param name="queueLimit">Maximum number of requests in the queue.</param>
        /// <param name="overflowAction">The action to be taken when the queue overflows.</param>
        public AsyncTargetWrapper(Target wrappedTarget, int queueLimit, AsyncTargetWrapperOverflowAction overflowAction)
        {
            this.RequestQueue = new AsyncRequestQueue<LogEventInfo>(10000, AsyncTargetWrapperOverflowAction.Discard);
            this.TimeToSleepBetweenBatches = 50;
            this.BatchSize = 100;
            this.WrappedTarget = wrappedTarget;
            this.QueueLimit = queueLimit;
            this.OverflowAction = overflowAction;
        }

        /// <summary>
        /// Gets or sets the number of log events that should be processed in a batch
        /// by the lazy writer thread.
        /// </summary>
        [DefaultValue(100)]
        public int BatchSize { get; set; }

        /// <summary>
        /// Gets or sets the time in milliseconds to sleep between batches.
        /// </summary>
        [DefaultValue(50)]
        public int TimeToSleepBetweenBatches { get; set; }

        /// <summary>
        /// Gets or sets the action to be taken when the lazy writer thread request queue count
        /// exceeds the set limit.
        /// </summary>
        [DefaultValue("Discard")]
        public AsyncTargetWrapperOverflowAction OverflowAction
        {
            get { return this.RequestQueue.OnOverflow; }
            set { this.RequestQueue.OnOverflow = value; }
        }

        /// <summary>
        /// Gets or sets the limit on the number of requests in the lazy writer thread request queue.
        /// </summary>
        [DefaultValue(10000)]
        public int QueueLimit
        {
            get { return this.RequestQueue.RequestLimit; }
            set { this.RequestQueue.RequestLimit = value; }
        }

        /// <summary>
        /// Gets the queue of lazy writer thread requests.
        /// </summary>
        protected AsyncRequestQueue<LogEventInfo> RequestQueue { get; private set; }

        /// <summary>
        /// Initializes the target by starting the lazy writer thread.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();
            this.StartLazyWriterTimer();
        }

        /// <summary>
        /// Waits for the lazy writer thread to finish writing messages.
        /// </summary>
        /// <param name="timeout">Maximum time to allow for the flush. Any messages after that time will be discarded.</param>
        public override void Flush(TimeSpan timeout)
        {
            this.lazyWriterTimer.Change(Timeout.Infinite, Timeout.Infinite);
            lock (this.inLazyWriterMonitor)
            {
                this.flushAll = true;
                this.LazyWriterTimerCallback(null);
            }

            this.lazyWriterTimer.Change(this.TimeToSleepBetweenBatches, this.TimeToSleepBetweenBatches);
        }

        /// <summary>
        /// Closes the target by stopping the lazy writer thread.
        /// </summary>
        public override void Close()
        {
            this.StopLazyWriterThread();
            base.Close();
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
            this.WrappedTarget.PrecalculateVolatileLayouts(logEvent);
            this.RequestQueue.Enqueue(logEvent);
        }

        /// <summary>
        /// Starts the lazy writer thread which periodically writes
        /// queued log messages.
        /// </summary>
        protected virtual void StartLazyWriterTimer()
        {
            this.RequestQueue.Clear();
            InternalLogger.Debug("Starting lazy writer timer...");
            this.lazyWriterTimer = new Timer(this.LazyWriterTimerCallback, null, 0, this.TimeToSleepBetweenBatches);
        }

        /// <summary>
        /// Starts the lazy writer thread.
        /// </summary>
        protected virtual void StopLazyWriterThread()
        {
            if (this.lazyWriterTimer == null)
            {
                return;
            }

            this.Flush();
            this.lazyWriterTimer.Change(0, 0);
            this.lazyWriterTimer.Dispose();
            this.lazyWriterTimer = null;

            this.RequestQueue.Clear();
        }

        private void LazyWriterTimerCallback(object state)
        {
            lock (this.inLazyWriterMonitor)
            {
                try
                {
                    do
                    {
                        // Console.WriteLine("q: {0}", RequestQueue.RequestCount);
                        List<LogEventInfo> pendingRequests = this.RequestQueue.DequeueBatch(this.BatchSize);

                        try
                        {
                            if (pendingRequests.Count == 0)
                            {
                                break;
                            }

                            WrappedTarget.Write(pendingRequests.ToArray());
                        }
                        finally
                        {
                            this.RequestQueue.BatchProcessed(pendingRequests);
                        }
                    }
                    while (this.flushAll);
                }
                catch (Exception ex)
                {
                    InternalLogger.Error("Error in lazy writer timer procedure: {0}", ex);
                }
            }
        }
    }
}
