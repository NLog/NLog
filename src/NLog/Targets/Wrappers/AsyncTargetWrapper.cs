// 
// Copyright (c) 2004-2020 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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
    using NLog.Common;
    using NLog.Internal;

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
        private readonly object _writeLockObject = new object();
        private readonly object _timerLockObject = new object();
        private Timer _lazyWriterTimer;
        private readonly ReusableAsyncLogEventList _reusableAsyncLogEventList = new ReusableAsyncLogEventList(200);
        private event EventHandler<LogEventDroppedEventArgs> _logEventDroppedEvent;
        private event EventHandler<LogEventQueueGrowEventArgs> _eventQueueGrowEvent;
        private bool _missingServiceTypes;

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
            Name = name;
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
#if NETSTANDARD2_0
            // NetStandard20 includes many optimizations for ConcurrentQueue:
            //  - See: https://blogs.msdn.microsoft.com/dotnet/2017/06/07/performance-improvements-in-net-core/
            // Net40 ConcurrencyQueue can seem to leak, because it doesn't clear properly on dequeue
            //  - See: https://blogs.msdn.microsoft.com/pfxteam/2012/05/08/concurrentqueuet-holding-on-to-a-few-dequeued-elements/
            _requestQueue = new ConcurrentRequestQueue(10000, AsyncTargetWrapperOverflowAction.Discard);
#else
            _requestQueue = new AsyncRequestQueue(10000, AsyncTargetWrapperOverflowAction.Discard);
#endif
            TimeToSleepBetweenBatches = 1;
            BatchSize = 200;
            FullBatchSizeWriteLimit = 5;
            WrappedTarget = wrappedTarget;
            QueueLimit = queueLimit;
            OverflowAction = overflowAction;
        }

        /// <summary>
        /// Gets or sets the number of log events that should be processed in a batch
        /// by the lazy writer thread.
        /// </summary>
        /// <docgen category='Buffering Options' order='100' />
        [DefaultValue(200)]
        public int BatchSize { get; set; }

        /// <summary>
        /// Gets or sets the time in milliseconds to sleep between batches. (1 or less means trigger on new activity)
        /// </summary>
        /// <docgen category='Buffering Options' order='100' />
        [DefaultValue(1)]
        public int TimeToSleepBetweenBatches { get; set; }

        
        /// <summary>
        /// Raise event when Target cannot store LogEvent.
        /// Event arg contains lost LogEvents
        /// </summary>
        public event EventHandler<LogEventDroppedEventArgs> LogEventDropped
        {
            add
            {
                if (_logEventDroppedEvent == null && _requestQueue != null )
                {
                    _requestQueue.LogEventDropped += OnRequestQueueDropItem;
                }

                _logEventDroppedEvent += value;
            }
            remove
            {
                _logEventDroppedEvent -= value;

                if (_logEventDroppedEvent == null && _requestQueue != null)
                {
                    _requestQueue.LogEventDropped -= OnRequestQueueDropItem;
                }
            }
        }
        
        /// <summary>
        /// Raises when event queue grow. 
        /// Queue can grow when <see cref="OverflowAction"/> was set to <see cref="AsyncTargetWrapperOverflowAction.Grow"/>
        /// </summary>
        public event EventHandler<LogEventQueueGrowEventArgs> EventQueueGrow
        {
            add
            {
                if (_eventQueueGrowEvent == null && _requestQueue != null)
                {
                    _requestQueue.LogEventQueueGrow += OnRequestQueueGrow;
                }

                _eventQueueGrowEvent += value;
            }
            remove
            {
                _eventQueueGrowEvent -= value;

                if (_eventQueueGrowEvent == null && _requestQueue != null)
                {
                    _requestQueue.LogEventQueueGrow -= OnRequestQueueGrow;
                }
            }
        }
        
        /// <summary>
        /// Gets or sets the action to be taken when the lazy writer thread request queue count
        /// exceeds the set limit.
        /// </summary>
        /// <docgen category='Buffering Options' order='100' />
        [DefaultValue("Discard")]
        public AsyncTargetWrapperOverflowAction OverflowAction
        {
            get => _requestQueue.OnOverflow;
            set => _requestQueue.OnOverflow = value;
        }

        /// <summary>
        /// Gets or sets the limit on the number of requests in the lazy writer thread request queue.
        /// </summary>
        /// <docgen category='Buffering Options' order='100' />
        [DefaultValue(10000)]
        public int QueueLimit
        {
            get => _requestQueue.RequestLimit;
            set => _requestQueue.RequestLimit = value;
        }

        /// <summary>
        /// Gets or sets the limit of full <see cref="BatchSize"/>s to write before yielding into <see cref="TimeToSleepBetweenBatches"/> 
        /// Performance is better when writing many small batches, than writing a single large batch
        /// </summary>
        /// <docgen category='Buffering Options' order='100' />
        [DefaultValue(5)]
        public int FullBatchSizeWriteLimit { get; set; }

        /// <summary>
        /// Gets or sets whether to use the locking queue, instead of a lock-free concurrent queue
        /// The locking queue is less concurrent when many logger threads, but reduces memory allocation
        /// </summary>
        /// <docgen category='Buffering Options' order='100' />
        [DefaultValue(false)]
        public bool ForceLockingQueue { get => _forceLockingQueue ?? false; set => _forceLockingQueue = value; }
        private bool? _forceLockingQueue;

        /// <summary>
        /// Gets the queue of lazy writer thread requests.
        /// </summary>
        AsyncRequestQueueBase _requestQueue;

        /// <summary>
        /// Schedules a flush of pending events in the queue (if any), followed by flushing the WrappedTarget.
        /// </summary>
        /// <param name="asyncContinuation">The asynchronous continuation.</param>
        protected override void FlushAsync(AsyncContinuation asyncContinuation)
        {
            if (_flushEventsInQueueDelegate == null)
                _flushEventsInQueueDelegate = new AsyncHelpersTask(FlushEventsInQueue);
            AsyncHelpers.StartAsyncTask(_flushEventsInQueueDelegate.Value, asyncContinuation);
        }

        private AsyncHelpersTask? _flushEventsInQueueDelegate;

        /// <summary>
        /// Initializes the target by starting the lazy writer timer.
        /// </summary>
        protected override void InitializeTarget()
        {
            base.InitializeTarget();

            if (!ForceLockingQueue && OverflowAction == AsyncTargetWrapperOverflowAction.Block && BatchSize * 1.5m > QueueLimit)
            {
                ForceLockingQueue = true;   // ConcurrentQueue does not perform well if constantly hitting QueueLimit
            }

#if !NET35
            if (_forceLockingQueue.HasValue && _forceLockingQueue.Value != (_requestQueue is AsyncRequestQueue))
            {
                _requestQueue = ForceLockingQueue ? (AsyncRequestQueueBase)new AsyncRequestQueue(QueueLimit, OverflowAction) : new ConcurrentRequestQueue(QueueLimit, OverflowAction);
            }
#endif

            if (BatchSize > QueueLimit && TimeToSleepBetweenBatches <= 1)
            {
                BatchSize = QueueLimit;     // Avoid too much throttling 
            }

            if (WrappedTarget != null && WrappedTarget.InitializeException is Config.NLogDependencyResolveException && OverflowAction == AsyncTargetWrapperOverflowAction.Discard)
            {
                _missingServiceTypes = true;
            }

            _requestQueue.Clear();
            InternalLogger.Trace("{0}: Start Timer", this);
            _lazyWriterTimer = new Timer(ProcessPendingEvents, null, Timeout.Infinite, Timeout.Infinite);
            StartLazyWriterTimer();
        }

        /// <summary>
        /// Shuts down the lazy writer timer.
        /// </summary>
        protected override void CloseTarget()
        {
            StopLazyWriterThread();
            
            if (Monitor.TryEnter(_writeLockObject, 500))
            {
                try
                {
                    WriteEventsInQueue(int.MaxValue, "Closing Target");
                }
                finally
                {
                    Monitor.Exit(_writeLockObject);
                }
            }

            if (OverflowAction == AsyncTargetWrapperOverflowAction.Block)
            {
                _requestQueue.Clear();  // Try to eject any threads, that are blocked in the RequestQueue
            }

            base.CloseTarget();
        }

        /// <summary>
        /// Starts the lazy writer thread which periodically writes
        /// queued log messages.
        /// </summary>
        protected virtual void StartLazyWriterTimer()
        {
            lock (_timerLockObject)
            {
                if (_lazyWriterTimer != null)
                {
                    if (TimeToSleepBetweenBatches <= 1)
                    {
                        InternalLogger.Trace("{0}: Throttled timer scheduled", this);
                        _lazyWriterTimer.Change(1, Timeout.Infinite);
                    }
                    else
                    {
                        _lazyWriterTimer.Change(TimeToSleepBetweenBatches, Timeout.Infinite);
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
            return StartTimerUnlessWriterActive(true);
        }

        private bool StartTimerUnlessWriterActive(bool instant)
        {
            bool lockTaken = false;
            try
            {
                lockTaken = Monitor.TryEnter(_writeLockObject);
                if (lockTaken)
                {
                    // Lock taken means no other timer-worker-thread is trying to write, schedule timer now
                    if (instant)
                    {
                        lock (_timerLockObject)
                        {
                            if (_lazyWriterTimer != null)
                            {
                                // Not optimal to schedule timer-worker-thread while holding lock,
                                // as the newly scheduled timer-worker-thread will hammer into the writeLockObject
                                _lazyWriterTimer.Change(0, Timeout.Infinite);
                                return true;
                            }
                        }
                    }
                    else
                    {
                        StartLazyWriterTimer();
                        return true;
                    }
                }
            }
            finally
            {
                // If not able to take lock, then it means timer-worker-thread is already active,
                // and timer-worker-thread will check RequestQueue after leaving writeLockObject
                if (lockTaken)
                    Monitor.Exit(_writeLockObject);
            }

            return false;
        }

        /// <summary>
        /// Stops the lazy writer thread.
        /// </summary>
        protected virtual void StopLazyWriterThread()
        {
            lock (_timerLockObject)
            {
                var currentTimer = _lazyWriterTimer;
                if (currentTimer != null)
                {
                    _lazyWriterTimer = null;
                    currentTimer.WaitForDispose(TimeSpan.FromSeconds(1));
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
            PrecalculateVolatileLayouts(logEvent.LogEvent);
            bool queueWasEmpty = _requestQueue.Enqueue(logEvent);
            if (queueWasEmpty)
            {
                if (TimeToSleepBetweenBatches == 0)
                    StartInstantWriterTimer();
                else if (TimeToSleepBetweenBatches <= 1)
                    StartLazyWriterTimer();
            }
        }

        /// <summary>
        /// Write to queue without locking <see cref="Target.SyncRoot"/> 
        /// </summary>
        /// <param name="logEvent"></param>
        protected override void WriteAsyncThreadSafe(AsyncLogEventInfo logEvent)
        {
            try
            {
                Write(logEvent);
            }
            catch (Exception exception)
            {
                if (ExceptionMustBeRethrown(exception))
                {
                    throw;
                }

                logEvent.Continuation(exception);
            }
        }

        private void ProcessPendingEvents(object state)
        {
            if (_lazyWriterTimer == null)
                return;

            bool wroteFullBatchSize = false;

            try
            {
                lock (_writeLockObject)
                {
                    int count = WriteEventsInQueue(BatchSize, "Timer");
                    if (count == BatchSize)
                        wroteFullBatchSize = true;

                    if (wroteFullBatchSize && TimeToSleepBetweenBatches <= 1)
                        StartInstantWriterTimer(); // Found full batch, fast schedule to take next batch (within lock to avoid pile up)
                }
            }
            catch (Exception exception)
            {
                wroteFullBatchSize = false; // Something went wrong, lets throttle retry

                InternalLogger.Error(exception, "{0}: Error in lazy writer timer procedure.", this);

                if (exception.MustBeRethrownImmediately())
                {
                    throw;  // Throwing exceptions here will crash the entire application (.NET 2.0 behavior)
                }
            }
            finally
            {
                if (TimeToSleepBetweenBatches <= 1)
                {
                    if (!wroteFullBatchSize && !_requestQueue.IsEmpty)
                    {
                        // If queue was not empty, then more might have arrived while writing the first batch
                        // Do not use instant timer, so we can process in larger batches (faster)
                        StartTimerUnlessWriterActive(false);
                    }
                }
                else
                {
                    StartLazyWriterTimer();
                }
            }
        }

        private void FlushEventsInQueue(object state)
        {
            try
            {
                var asyncContinuation = state as AsyncContinuation;
                lock (_writeLockObject)
                {
                    WriteEventsInQueue(int.MaxValue, "Flush Async");
                    if (asyncContinuation != null)
                        base.FlushAsync(asyncContinuation);
                }

                if (TimeToSleepBetweenBatches <= 1 && !_requestQueue.IsEmpty)
                    StartTimerUnlessWriterActive(false);
            }
            catch (Exception exception)
            {
                InternalLogger.Error(exception, "{0}: Error in flush procedure.", this);

                if (exception.MustBeRethrownImmediately())
                {
                    throw;  // Throwing exceptions here will crash the entire application (.NET 2.0 behavior)
                }
            }
        }

        private int WriteEventsInQueue(int batchSize, string reason)
        {
            if (WrappedTarget == null)
            {
                InternalLogger.Error("{0}: WrappedTarget is NULL", this);
                return 0;
            }

            if (_missingServiceTypes)
            {
                if (WrappedTarget.InitializeException is Config.NLogDependencyResolveException)
                {
                    return 0;
                }

                _missingServiceTypes = false;
                InternalLogger.Debug("{0}: WrappedTarget has resolved missing dependency", this);
            }

            int count = 0;
            for (int i = 0; i < FullBatchSizeWriteLimit; ++i)
            {
                if (batchSize == int.MaxValue)
                {
                    var logEvents = _requestQueue.DequeueBatch(batchSize);
                    if (logEvents.Length > 0)
                    {
                        if (reason != null)
                            InternalLogger.Trace("{0}: Writing {1} events ({2})", this, logEvents.Length, reason);
                        WrappedTarget.WriteAsyncLogEvents(logEvents);
                    }
                    count = logEvents.Length;
                }
                else
                {
                    using (var targetList = _reusableAsyncLogEventList.Allocate())
                    {
                        var logEvents = targetList.Result;
                        _requestQueue.DequeueBatch(batchSize, logEvents);
                        if (logEvents.Count > 0)
                        {
                            if (reason != null)
                                InternalLogger.Trace("{0}: Writing {1} events ({2})", this, logEvents.Count, reason);
                            WrappedTarget.WriteAsyncLogEvents(logEvents);
                        }
                        count = logEvents.Count;
                    }
                }
                if (count < batchSize)
                    break;
            }
            return count;
        }

        private void OnRequestQueueDropItem(object sender, LogEventDroppedEventArgs logEventDroppedEventArgs) 
        {
            _logEventDroppedEvent?.Invoke(this, logEventDroppedEventArgs);
        }

        private void OnRequestQueueGrow(object sender, LogEventQueueGrowEventArgs logEventQueueGrowEventArgs) 
        {
            _eventQueueGrowEvent?.Invoke(this, logEventQueueGrowEventArgs);
        }
    }
}