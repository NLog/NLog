// 
// Copyright (c) 2004-2016 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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
    using System.ComponentModel;
    using System.Threading;
    using Common;
    using Internal;
    using System.Collections.Generic;

    /// <summary>
    /// Provides asynchronous, buffered execution of target writes.
    /// </summary>
    /// <seealso href="https://github.com/nlog/nlog/wiki/AsyncWrapper-target">Documentation on NLog Wiki</seealso>
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
        private readonly object lockObject = new object();
        private Timer lazyWriterTimer;
        private readonly Queue<AsyncContinuation> flushAllContinuations = new Queue<AsyncContinuation>();
        private readonly object continuationQueueLock = new object();

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncTargetWrapper" /> class.
        /// </summary>
        public AsyncTargetWrapper()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncTargetWrapper" /> class.
        /// </summary>
        /// <param name="name">Name of the target.</param>
        /// <param name="wrappedTarget">The wrapped target.</param>
        public AsyncTargetWrapper(string name, Target wrappedTarget)
            : this(wrappedTarget)
        {
            this.Name = name;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncTargetWrapper" /> class.
        /// </summary>
        /// <param name="wrappedTarget">The wrapped target.</param>
        public AsyncTargetWrapper(Target wrappedTarget)
            : this(wrappedTarget, 10000, AsyncTargetWrapperOverflowAction.Discard)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncTargetWrapper" /> class.
        /// </summary>
        /// <param name="wrappedTarget">The wrapped target.</param>
        /// <param name="queueLimit">Maximum number of requests in the queue.</param>
        /// <param name="overflowAction">The action to be taken when the queue overflows.</param>
        public AsyncTargetWrapper(Target wrappedTarget, int queueLimit, AsyncTargetWrapperOverflowAction overflowAction)
        {
            this.RequestQueue = new AsyncRequestQueue(10000, AsyncTargetWrapperOverflowAction.Discard);
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
        /// <docgen category='Buffering Options' order='100' />
        [DefaultValue(100)]
        public int BatchSize { get; set; }

        /// <summary>
        /// Gets or sets the time in milliseconds to sleep between batches.
        /// </summary>
        /// <docgen category='Buffering Options' order='100' />
        [DefaultValue(50)]
        public int TimeToSleepBetweenBatches { get; set; }

        /// <summary>
        /// Gets or sets the action to be taken when the lazy writer thread request queue count
        /// exceeds the set limit.
        /// </summary>
        /// <docgen category='Buffering Options' order='100' />
        [DefaultValue("Discard")]
        public AsyncTargetWrapperOverflowAction OverflowAction
        {
            get { return this.RequestQueue.OnOverflow; }
            set { this.RequestQueue.OnOverflow = value; }
        }

        /// <summary>
        /// Gets or sets the limit on the number of requests in the lazy writer thread request queue.
        /// </summary>
        /// <docgen category='Buffering Options' order='100' />
        [DefaultValue(10000)]
        public int QueueLimit
        {
            get { return this.RequestQueue.RequestLimit; }
            set { this.RequestQueue.RequestLimit = value; }
        }

        /// <summary>
        /// Gets the queue of lazy writer thread requests.
        /// </summary>
        internal AsyncRequestQueue RequestQueue { get; private set; }

        /// <summary>
        /// Waits for the lazy writer thread to finish writing messages.
        /// </summary>
        /// <param name="asyncContinuation">The asynchronous continuation.</param>
        protected override void FlushAsync(AsyncContinuation asyncContinuation)
        {
            bool queueWasEmpty = false;
            lock (this.continuationQueueLock)
            {
                this.flushAllContinuations.Enqueue(asyncContinuation);
                if (TimeToSleepBetweenBatches <= 0 && this.flushAllContinuations.Count == 1)
                    queueWasEmpty = RequestQueue.RequestCount == 0;
            }
            if (queueWasEmpty)
                StartLazyWriterTimer(); // Will schedule new timer-worker-thread, after waiting for the current to have completed its last batch
        }

        /// <summary>
        /// Initializes the target by starting the lazy writer timer.
        /// </summary>
        protected override void InitializeTarget()
        {
            base.InitializeTarget();
            this.RequestQueue.Clear();
            InternalLogger.Trace("AsyncWrapper '{0}': start timer", Name);
            this.lazyWriterTimer = new Timer(this.ProcessPendingEvents, null, Timeout.Infinite, Timeout.Infinite);
            this.StartLazyWriterTimer();
        }

        /// <summary>
        /// Shuts down the lazy writer timer.
        /// </summary>
        protected override void CloseTarget()
        {
            this.StopLazyWriterThread();
            if (this.RequestQueue.RequestCount > 0)
            {
                ProcessPendingEvents(null);
            }

            base.CloseTarget();
        }

        /// <summary>
        /// Starts the lazy writer thread which periodically writes
        /// queued log messages.
        /// </summary>
        protected virtual void StartLazyWriterTimer()
        {
            lock (this.lockObject)
            {
                if (this.lazyWriterTimer != null)
                {
                    if (this.TimeToSleepBetweenBatches <= 0)
                    {
                        InternalLogger.Trace("AsyncWrapper '{0}': Throttled timer scheduled", this.Name);
                        this.lazyWriterTimer.Change(1, Timeout.Infinite);
                    }
                    else
                    {
                        this.lazyWriterTimer.Change(this.TimeToSleepBetweenBatches, Timeout.Infinite);
                    }
                }
            }
        }

        /// <summary>
        /// Attempts to start an instant timer-worker-thread which can write
        /// queued log messages.
        /// </summary>
        /// <returns>Returns true when scheduled a timer-worker-thread</returns>
        protected virtual bool StartInstantWriterTimer()
        {
            bool lockTaken = false;
            try
            {
                lockTaken = Monitor.TryEnter(this.lockObject);
                if (lockTaken)
                {
                    // Lock taken means no timer-worker-thread is active, schedule now
                    if (this.lazyWriterTimer != null)
                    {
                        if (this.TimeToSleepBetweenBatches <= 0)
                        {
                            // Not optimal to shedule timer-worker-thread while holding lock,
                            // as the newly scheduled timer-worker-thread will hammer into the lockObject
                            this.lazyWriterTimer.Change(0, Timeout.Infinite);
                            return true;
                        }
                    }
                }

                return false;
            }
            finally
            {
                // If not able to take lock, then it means timer-worker-thread is already active,
                // and timer-worker-thread will check RequestQueue after leaving lockObject
                if (lockTaken)
                    Monitor.Exit(this.lockObject);
            }
        }

        /// <summary>
        /// Stops the lazy writer thread.
        /// </summary>
        protected virtual void StopLazyWriterThread()
        {
            lock (this.lockObject)
            {
                if (this.lazyWriterTimer != null)
                {
                    this.lazyWriterTimer.Change(Timeout.Infinite, Timeout.Infinite);
                    this.lazyWriterTimer = null;
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
        protected override void Write(AsyncLogEventInfo logEvent)
        {
            this.MergeEventProperties(logEvent.LogEvent);
            this.PrecalculateVolatileLayouts(logEvent.LogEvent);
            bool queueWasEmpty = this.RequestQueue.Enqueue(logEvent);
            if (queueWasEmpty && TimeToSleepBetweenBatches <= 0)
                StartInstantWriterTimer();
        }

        /// <summary>
        /// Write to queue without locking <see cref="Target.SyncRoot"/> 
        /// </summary>
        /// <param name="logEvent"></param>
        protected override void WriteAsyncThreadSafe(AsyncLogEventInfo logEvent)
        {
            try
            {
                this.Write(logEvent);
            }
            catch (Exception exception)
            {
                if (exception.MustBeRethrown())
                {
                    throw;
                }

                logEvent.Continuation(exception);
            }
        }

        private static readonly AsyncContinuation[] noFlushNullContinuationArray = new AsyncContinuation[] { null };

        private void ProcessPendingEvents(object state)
        {
            bool? wroteFullBatchSize = false;
            AsyncContinuation[] continuations;
            bool lockTaken = false;

            try
            {
                if (TimeToSleepBetweenBatches <= 0)
                {
                    Monitor.Enter(this.lockObject);
                    lockTaken = true;
                }

                lock (this.continuationQueueLock)
                {
                    continuations = this.flushAllContinuations.Count > 0
                        ? this.flushAllContinuations.ToArray()
                        : noFlushNullContinuationArray;
                    this.flushAllContinuations.Clear();
                }

                if (this.WrappedTarget == null)
                {
                    InternalLogger.Error("AsyncWrapper '{0}': WrappedTarget is NULL", this.Name);
                    return;
                }

                for (int x = 0; x < continuations.Length; x++)
                {
                    var continuation = continuations[x];

                    int count = this.BatchSize;
                    if (continuation != null)
                    {
                        count = -1; // Dequeue all
                    }

                    AsyncLogEventInfo[] logEventInfos = this.RequestQueue.DequeueBatch(count);
                    if (logEventInfos.Length == 0)
                        wroteFullBatchSize = null;    // Nothing to write
                    else if (logEventInfos.Length == BatchSize)
                        wroteFullBatchSize = true;

                    if (InternalLogger.IsTraceEnabled || continuation != null)
                        InternalLogger.Trace("AsyncWrapper '{0}': Flushing {1} events.", this.Name, logEventInfos.Length);

                    if (continuation != null)
                    {
                        // write all events, then flush, then call the continuation
                        this.WrappedTarget.WriteAsyncLogEvents(logEventInfos, ex => this.WrappedTarget.Flush(continuation));
                    }
                    else
                    {
                        // just write all events
                        this.WrappedTarget.WriteAsyncLogEvents(logEventInfos);
                    }
                }
            }
            catch (Exception exception)
            {
                wroteFullBatchSize = false; // Something went wrong, lets throttle retry

                InternalLogger.Error(exception, "AsyncWrapper '{0}': Error in lazy writer timer procedure.", this.Name);

                if (exception.MustBeRethrown())
                {
                    throw;
                }
            }
            finally
            {
                if (TimeToSleepBetweenBatches <= 0 && wroteFullBatchSize == true)
                    this.StartInstantWriterTimer(); // Found full batch, fast schedule to take next batch (within lock to avoid pile up)

                if (lockTaken)
                    Monitor.Exit(this.lockObject);

                if (TimeToSleepBetweenBatches <= 0)
                {
                    // If queue was not empty, then more might have arrived while writing the first batch
                    // Uses throttled timer here, so we can process in batches (faster)
                    if (wroteFullBatchSize.HasValue && !wroteFullBatchSize.Value)
                        this.StartLazyWriterTimer();    // Queue was not empty, more might have come (Skip expensive RequestQueue-check)
                    else if (!wroteFullBatchSize.HasValue)
                    {
                        if (this.RequestQueue.RequestCount > 0)
                            this.StartLazyWriterTimer();    // Queue was checked as empty, but now we have more
                        else
                        {
                            lock (this.continuationQueueLock)
                                if (this.flushAllContinuations.Count > 0)
                                    this.StartLazyWriterTimer();    // Flush queue was checked as empty, but now we have more
                        }
                    }
                }
                else
                {
                    this.StartLazyWriterTimer();
                }
            }
        }
    }
}