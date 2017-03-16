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
        private readonly object writeLockObject = new object();
        private readonly object timerLockObject = new object();
        private Timer lazyWriterTimer;
        private readonly ReusableAsyncLogEventList reusableAsyncLogEventList = new ReusableAsyncLogEventList(200);

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
            this.BatchSize = 200;
            this.FullBatchSizeWriteLimit = 5;
            this.WrappedTarget = wrappedTarget;
            this.QueueLimit = queueLimit;
            this.OverflowAction = overflowAction;
        }

        /// <summary>
        /// Gets or sets the number of log events that should be processed in a batch
        /// by the lazy writer thread.
        /// </summary>
        /// <docgen category='Buffering Options' order='100' />
        [DefaultValue(200)]
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
        /// Gets or sets the limit of full <see cref="BatchSize"/>s to write before yielding into <see cref="TimeToSleepBetweenBatches"/> 
        /// Performance is better when writing many small batches, than writing a single large batch
        /// </summary>
        /// <docgen category='Buffering Options' order='100' />
        [DefaultValue(5)]
        public int FullBatchSizeWriteLimit { get; set; }

        /// <summary>
        /// Gets the queue of lazy writer thread requests.
        /// </summary>
        internal AsyncRequestQueue RequestQueue { get; private set; }

        /// <summary>
        /// Schedules a flush of pending events in the queue (if any), followed by flushing the WrappedTarget.
        /// </summary>
        /// <param name="asyncContinuation">The asynchronous continuation.</param>
        protected override void FlushAsync(AsyncContinuation asyncContinuation)
        {
            if (flushEventsInQueueDelegate == null)
                flushEventsInQueueDelegate = FlushEventsInQueue;
            ThreadPool.QueueUserWorkItem(flushEventsInQueueDelegate, asyncContinuation);
        }
        private WaitCallback flushEventsInQueueDelegate;

        /// <summary>
        /// Initializes the target by starting the lazy writer timer.
        /// </summary>
        protected override void InitializeTarget()
        {
            base.InitializeTarget();
            if (!OptimizeBufferReuse && WrappedTarget != null && WrappedTarget.OptimizeBufferReuse)
                OptimizeBufferReuse = GetType() == typeof(AsyncTargetWrapper); // TODO NLog 5 - Manual Opt-Out

            this.RequestQueue.Clear();
            InternalLogger.Trace("AsyncWrapper '{0}': start timer", this.Name);
            this.lazyWriterTimer = new Timer(this.ProcessPendingEvents, null, Timeout.Infinite, Timeout.Infinite);
            this.StartLazyWriterTimer();
        }

        /// <summary>
        /// Shuts down the lazy writer timer.
        /// </summary>
        protected override void CloseTarget()
        {
            this.StopLazyWriterThread();
            if (Monitor.TryEnter(this.writeLockObject, 500))
            {
                try
                {
                    WriteEventsInQueue(int.MaxValue, "Closing Target");
                }
                finally
                {
                    Monitor.Exit(this.writeLockObject);
                }
            }
            base.CloseTarget();
        }

        /// <summary>
        /// Starts the lazy writer thread which periodically writes
        /// queued log messages.
        /// </summary>
        protected virtual void StartLazyWriterTimer()
        {
            lock (this.timerLockObject)
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
                lockTaken = Monitor.TryEnter(this.writeLockObject);
                if (lockTaken)
                {
                    // Lock taken means no timer-worker-thread is active writing, schedule timer now
                    lock (this.timerLockObject)
                    {
                        if (this.lazyWriterTimer != null)
                        {
                            // Not optimal to shedule timer-worker-thread while holding lock,
                            // as the newly scheduled timer-worker-thread will hammer into the writeLockObject
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
                // and timer-worker-thread will check RequestQueue after leaving writeLockObject
                if (lockTaken)
                    Monitor.Exit(this.writeLockObject);
            }
        }

        /// <summary>
        /// Stops the lazy writer thread.
        /// </summary>
        protected virtual void StopLazyWriterThread()
        {
            lock (this.timerLockObject)
            {
                var currentTimer = this.lazyWriterTimer;
                if (currentTimer != null)
                {
                    this.lazyWriterTimer = null;
                    currentTimer.Change(Timeout.Infinite, Timeout.Infinite);
                    ManualResetEvent waitHandle = new ManualResetEvent(false);
                    if (currentTimer.Dispose(waitHandle))
                    {
                        if (waitHandle.WaitOne(1000))
                            waitHandle.Close();
                    }
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
            if (queueWasEmpty && this.TimeToSleepBetweenBatches <= 0)
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

        private void ProcessPendingEvents(object state)
        {
            if (this.lazyWriterTimer == null)
                return;

            bool wroteFullBatchSize = false;

            try
            {
                lock (this.writeLockObject)
                {
                    int count = WriteEventsInQueue(this.BatchSize, "Timer");
                    if (count == this.BatchSize)
                        wroteFullBatchSize = true;

                    if (wroteFullBatchSize && this.TimeToSleepBetweenBatches <= 0)
                        this.StartInstantWriterTimer(); // Found full batch, fast schedule to take next batch (within lock to avoid pile up)
                }
            }
            catch (Exception exception)
            {
                wroteFullBatchSize = false; // Something went wrong, lets throttle retry

                InternalLogger.Error(exception, "AsyncWrapper '{0}': Error in lazy writer timer procedure.", this.Name);

                if (exception.MustBeRethrownImmediately())
                {
                    throw;  // Throwing exceptions here will crash the entire application (.NET 2.0 behavior)
                }
            }
            finally
            {
                if (this.TimeToSleepBetweenBatches <= 0)
                {
                    // If queue was not empty, then more might have arrived while writing the first batch
                    // Uses throttled timer here, so we can process in batches (faster)
                    if (!wroteFullBatchSize && this.RequestQueue.RequestCount > 0)
                        this.StartLazyWriterTimer();    // Queue was checked as empty, but now we have more
                }
                else
                {
                    this.StartLazyWriterTimer();
                }
            }
        }

        private void FlushEventsInQueue(object state)
        {
            try
            {
                var asyncContinuation = state as AsyncContinuation;
                lock (this.writeLockObject)
                {
                    WriteEventsInQueue(int.MaxValue, "Flush Async");
                    if (asyncContinuation != null)
                        base.FlushAsync(asyncContinuation);
                }
                if (this.TimeToSleepBetweenBatches <= 0 && this.RequestQueue.RequestCount > 0)
                    this.StartLazyWriterTimer();    // Queue was checked as empty, but now we have more
            }
            catch (Exception exception)
            {
                InternalLogger.Error(exception, "AsyncWrapper '{0}': Error in flush procedure.", this.Name);

                if (exception.MustBeRethrownImmediately())
                {
                    throw;  // Throwing exceptions here will crash the entire application (.NET 2.0 behavior)
                }
            }
        }

        private int WriteEventsInQueue(int batchSize, string reason)
        {
            if (this.WrappedTarget == null)
            {
                InternalLogger.Error("AsyncWrapper '{0}': WrappedTarget is NULL", this.Name);
                return 0;
            }

            int count = 0;
            for (int i = 0; i < this.FullBatchSizeWriteLimit; ++i)
            {
                if (!this.OptimizeBufferReuse || batchSize == int.MaxValue)
                {
                    var logEvents = this.RequestQueue.DequeueBatch(batchSize);
                    if (logEvents.Length > 0)
                    {
                        if (reason != null)
                            InternalLogger.Trace("AsyncWrapper '{0}': writing {1} events ({2})", this.Name, logEvents.Length, reason);
                        this.WrappedTarget.WriteAsyncLogEvents(logEvents);
                    }
                    count = logEvents.Length;
                }
                else
                {
                    using (var targetList = this.reusableAsyncLogEventList.Allocate())
                    {
                        var logEvents = targetList.Result;
                        this.RequestQueue.DequeueBatch(batchSize, logEvents);
                        if (logEvents.Count > 0)
                        {
                            if (reason != null)
                                InternalLogger.Trace("AsyncWrapper '{0}': writing {1} events ({2})", this.Name, logEvents.Count, reason);
                            this.WrappedTarget.WriteAsyncLogEvents(logEvents);
                        }
                        count = logEvents.Count;
                    }
                }
                if (count < batchSize)
                    break;
            }
            return count;
        }
    }
}