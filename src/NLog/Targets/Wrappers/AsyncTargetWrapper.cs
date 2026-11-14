//
// Copyright (c) 2004-2024 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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
    using System.Collections.Generic;
    using System.Threading;
    using NLog.Common;
    using NLog.Internal;
    using NLog.Targets.Dispatchers;

    /// <summary>
    /// Provides asynchronous, buffered execution of target writes.
    /// </summary>
    /// <remarks>
    /// <a href="https://github.com/NLog/NLog/wiki/AsyncWrapper-target">See NLog Wiki</a>
    /// </remarks>
    /// <seealso href="https://github.com/NLog/NLog/wiki/AsyncWrapper-target">Documentation on NLog Wiki</seealso>
    /// <remarks>
    /// <p>
    /// Asynchronous target wrapper allows the logger code to execute more quickly, by queuing
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
    /// <p>
    /// By default the queue is written from a timer callback on the .NET ThreadPool. Set dispatcher="DedicatedThread"
    /// to write from a dedicated background thread instead, so logging keeps working when the ThreadPool is starved.
    /// This costs one thread per target. To apply it to all targets, use &lt;default-wrapper&gt;:
    /// </p>
    /// <code lang="XML">
    /// <![CDATA[
    /// <targets>
    ///    <default-wrapper xsi:type="AsyncWrapper" overflowAction="Block" dispatcher="DedicatedThread" />
    ///    ... your targets go here ...
    /// </targets>
    /// ]]></code>
    /// <p>
    /// Don't combine it with async="true". That puts a second AsyncWrapper with default settings between
    /// the &lt;default-wrapper&gt; and each target, and that inner wrapper still writes from the ThreadPool.
    /// </p>
    /// </remarks>
    /// <example>
    /// <p>
    /// To set up the target in the <a href="https://github.com/NLog/NLog/wiki/Configuration-file">configuration file</a>,
    /// use the following syntax:
    /// </p>
    /// <code lang="XML" source="examples/targets/Configuration File/AsyncWrapper/NLog.config" />
    /// <p>
    /// To set up the log target programmatically use code like this:
    /// </p>
    /// <code lang="C#" source="examples/targets/Configuration API/AsyncWrapper/Wrapping File/Example.cs" />
    /// </example>
    [Target("AsyncWrapper", IsWrapper = true)]
    public class AsyncTargetWrapper : WrapperTargetBase
    {
        private readonly object _writeLockObject = new object();
        private readonly AsyncLogEventChannel _channel;
        private IChannelReader? _reader;
        private readonly ReusableAsyncLogEventList _reusableAsyncLogEventList = new ReusableAsyncLogEventList(200);
        private readonly WaitCallback _flushEventsInQueueDelegate;
        private event EventHandler<LogEventDroppedEventArgs>? _logEventDroppedEvent;
        private event EventHandler<LogEventQueueGrowEventArgs>? _eventQueueGrowEvent;
        private bool _missingServiceTypes;

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncTargetWrapper" /> class.
        /// </summary>
        public AsyncTargetWrapper()
        {
            _channel = new AsyncLogEventChannel(10000, AsyncTargetWrapperOverflowAction.Discard);
            _flushEventsInQueueDelegate = FlushEventsInQueue;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncTargetWrapper" /> class.
        /// </summary>
        /// <param name="name">Name of the target.</param>
        /// <param name="wrappedTarget">The wrapped target.</param>
        public AsyncTargetWrapper(string name, Target wrappedTarget)
            : this(wrappedTarget)
        {
            Name = name ?? Name;
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
            : this()
        {
            Name = (wrappedTarget is null || string.IsNullOrEmpty(wrappedTarget.Name)) ? Name : (wrappedTarget.Name + "_wrapper");
            WrappedTarget = wrappedTarget;
            QueueLimit = queueLimit;
            OverflowAction = overflowAction;
        }

        /// <summary>
        /// Gets or sets the number of log events that should be processed in a batch
        /// by the lazy writer thread.
        /// </summary>
        /// <remarks>Default: <see langword="200"/></remarks>
        /// <docgen category='Buffering Options' order='100' />
        public int BatchSize { get; set; } = 200;

        /// <summary>
        /// Gets or sets the time in milliseconds to sleep between batches. (1 or less means trigger on new activity)
        /// </summary>
        /// <remarks>Default: <see langword="1"/>ms</remarks>
        /// <docgen category='Buffering Options' order='100' />
        public int TimeToSleepBetweenBatches { get; set; } = 1;

        /// <summary>
        /// Occurs when LogEvent has been dropped, because internal queue is full and <see cref="OverflowAction"/> set to <see cref="AsyncTargetWrapperOverflowAction.Discard"/>
        /// </summary>
        public event EventHandler<LogEventDroppedEventArgs> LogEventDropped
        {
            add
            {
                if (_logEventDroppedEvent is null)
                {
                    _channel.LogEventDropped += OnRequestQueueDropItem;
                }

                _logEventDroppedEvent += value;
            }
            remove
            {
                _logEventDroppedEvent -= value;

                if (_logEventDroppedEvent is null)
                {
                    _channel.LogEventDropped -= OnRequestQueueDropItem;
                }
            }
        }

        /// <summary>
        /// Occurs when internal queue size is growing, because internal queue is full and <see cref="OverflowAction"/> set to <see cref="AsyncTargetWrapperOverflowAction.Grow"/>
        /// </summary>
        public event EventHandler<LogEventQueueGrowEventArgs> EventQueueGrow
        {
            add
            {
                if (_eventQueueGrowEvent is null)
                {
                    _channel.LogEventQueueGrow += OnRequestQueueGrow;
                }

                _eventQueueGrowEvent += value;
            }
            remove
            {
                _eventQueueGrowEvent -= value;

                if (_eventQueueGrowEvent is null)
                {
                    _channel.LogEventQueueGrow -= OnRequestQueueGrow;
                }
            }
        }

        /// <summary>
        /// Gets or sets the action to be taken when the lazy writer thread request queue count
        /// exceeds the set limit.
        /// </summary>
        /// <remarks>Default: <see cref="AsyncTargetWrapperOverflowAction.Discard"/></remarks>
        /// <docgen category='Buffering Options' order='10' />
        public AsyncTargetWrapperOverflowAction OverflowAction
        {
            get => _channel.OverflowAction;
            set => _channel.OverflowAction = value;
        }

        /// <summary>
        /// Gets or sets the limit on the number of requests in the lazy writer thread request queue.
        /// </summary>
        /// <remarks>Default: <see langword="10000"/></remarks>
        /// <docgen category='Buffering Options' order='10' />
        public int QueueLimit
        {
            get => _channel.QueueLimit;
            set => _channel.QueueLimit = value;
        }

        /// <summary>
        /// Gets or sets the number of batches of <see cref="BatchSize"/> to write before yielding into <see cref="TimeToSleepBetweenBatches"/>
        /// </summary>
        /// <remarks>Default: <see langword="10"/> . Better performance when writing small batches, than single large batch.</remarks>
        /// <docgen category='Buffering Options' order='100' />
        public int FullBatchSizeWriteLimit { get; set; } = 10;

        /// <summary>
        /// Gets or sets whether to use the locking queue, instead of a lock-free concurrent queue
        /// </summary>
        /// <remarks>Default: <see langword="false"/> . Queue with Monitor.Lock is less concurrent when many logger threads, but reduces memory allocation</remarks>
        /// <docgen category='Buffering Options' order='100' />
        public bool ForceLockingQueue { get => _forceLockingQueue ?? false; set => _forceLockingQueue = value; }
        private bool? _forceLockingQueue;

        /// <summary>
        /// Gets or sets where the background writer runs: on the .NET ThreadPool, or on a dedicated thread owned by this target.
        /// </summary>
        /// <remarks>
        /// Default: <see cref="AsyncTargetWrapperDispatcher.ThreadPool"/> .
        /// Use <see cref="AsyncTargetWrapperDispatcher.DedicatedThread"/> to keep writing when the .NET ThreadPool is starved,
        /// which matters most with <see cref="AsyncTargetWrapperOverflowAction.Block"/>. Costs one thread per target.
        /// Applied when the target is initialized.
        /// </remarks>
        /// <docgen category='Buffering Options' order='100' />
        public AsyncTargetWrapperDispatcher Dispatcher { get; set; }

        /// <summary>
        /// Overrides <see cref="Dispatcher"/> with a custom <see cref="IChannelReader"/>, created from the consumer
        /// and <see cref="TimeToSleepBetweenBatches"/>. A new reader is created on every initialize.
        /// </summary>
        internal Func<Func<ConsumeResult>, int, IChannelReader>? ReaderFactory { get; set; }

        /// <summary>
        /// Schedules a flush of pending events in the queue (if any), followed by flushing the WrappedTarget.
        /// </summary>
        /// <param name="asyncContinuation">The asynchronous continuation.</param>
        protected override void FlushAsync(AsyncContinuation asyncContinuation)
        {
            AsyncHelpers.StartAsyncTask(_flushEventsInQueueDelegate, asyncContinuation);
        }

        /// <summary>
        /// Initializes the target by starting the channel reader, which writes queued log events in the background.
        /// </summary>
        protected override void InitializeTarget()
        {
            base.InitializeTarget();

            if (!ForceLockingQueue && OverflowAction == AsyncTargetWrapperOverflowAction.Block && BatchSize * 1.5m > QueueLimit)
            {
                ForceLockingQueue = true;   // ConcurrentQueue does not perform well if constantly hitting QueueLimit
            }

            if (_forceLockingQueue.HasValue)
            {
                _channel.UseLockingQueue(_forceLockingQueue.Value);    // Only when set, otherwise keep the default queue for the platform
            }

            if (BatchSize > QueueLimit && TimeToSleepBetweenBatches <= 1)
            {
                BatchSize = QueueLimit;     // Avoid too much throttling
            }

            _layoutWithLock = _layoutWithLock ?? WrappedTarget?._layoutWithLock;

            if (WrappedTarget != null && OverflowAction == AsyncTargetWrapperOverflowAction.Discard && WrappedTarget.InitializeMissingDependencyException != null)
            {
                _missingServiceTypes = true;
                InternalLogger.Debug("{0} WrappedTarget has unresolved missing dependencies.", this);
            }

            _channel.Clear();
            InternalLogger.Trace("{0}: Start channel reader", this);
            var reader = CreateReader();

            // Still running when a subclass skipped base.CloseTarget on close. A dedicated thread would otherwise leak.
            Interlocked.Exchange(ref _reader, reader)?.Stop(TimeSpan.Zero);

            try
            {
                reader.Start();
            }
            catch
            {
                // Target.Close skips CloseTarget after failed initialize, so stop the reader here
                Interlocked.CompareExchange(ref _reader, null, reader);
                reader.Stop(TimeSpan.Zero);
                throw;
            }
        }

        private IChannelReader CreateReader()
        {
            if (ReaderFactory != null)
                return ReaderFactory(ConsumePendingEvents, TimeToSleepBetweenBatches);

            if (Dispatcher == AsyncTargetWrapperDispatcher.DedicatedThread)
                return new DedicatedThreadChannelReader(ConsumePendingEvents, TimeToSleepBetweenBatches, "NLog AsyncWrapper " + Name);

            return new ThreadPoolChannelReader(ConsumePendingEvents, TimeToSleepBetweenBatches, ToString());
        }

        /// <summary>
        /// Stops the channel reader, and writes the remaining queued log events.
        /// </summary>
        protected override void CloseTarget()
        {
            Interlocked.Exchange(ref _reader, null)?.Stop(TimeSpan.FromSeconds(1));

            var lockTimeoutMs = OverflowAction == AsyncTargetWrapperOverflowAction.Discard ? 500 : 1500;
            if (Monitor.TryEnter(_writeLockObject, lockTimeoutMs))
            {
                try
                {
                    WriteLogEventsInQueue(int.MaxValue, "Closing Target");
                }
                finally
                {
                    Monitor.Exit(_writeLockObject);
                }
            }
            else
            {
                InternalLogger.Debug("{0}: Failed to flush after lock timeout", this);
            }

            if (OverflowAction == AsyncTargetWrapperOverflowAction.Block)
            {
                _channel.Clear();  // Try to eject any threads, that are blocked in the RequestQueue
            }

            base.CloseTarget();
        }

        /// <summary>
        /// Adds the log event to asynchronous queue to be processed by
        /// the channel reader in the background.
        /// </summary>
        /// <param name="logEvent">The log event.</param>
        /// <remarks>
        /// The <see cref="Target.PrecalculateVolatileLayouts"/> is called
        /// to ensure that the log event can be processed in another thread.
        /// </remarks>
        protected override void Write(AsyncLogEventInfo logEvent)
        {
            PrecalculateVolatileLayouts(logEvent.LogEvent);
            if (_channel.Write(logEvent))
            {
                // Queue was empty, so tell the reader there is work. Otherwise a consumer run is already on its way.
                _reader?.Signal();
            }
        }

        /// <summary>
        /// Write to queue without locking <see cref="Target.SyncRoot"/>
        /// </summary>
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

        /// <summary>
        /// Write to queue without locking <see cref="Target.SyncRoot"/>
        /// </summary>
        protected override sealed void WriteAsyncThreadSafe(IList<AsyncLogEventInfo> logEvents)
        {
            for (int i = 0; i < logEvents.Count; ++i)
            {
                WriteAsyncThreadSafe(logEvents[i]);
            }
        }

        /// <summary>
        /// Consumer run, called by the channel reader: writes queued log events and reports what it found.
        /// Makes no scheduling decision, the reader decides when to run it next.
        /// </summary>
        private ConsumeResult ConsumePendingEvents()
        {
            if (_reader is null)
                return ConsumeResult.Drained;   // Closed

            try
            {
                lock (_writeLockObject)
                {
                    int lastBatchSize = WriteLogEventsInQueue(BatchSize, "Timer");
                    if (lastBatchSize == BatchSize)
                        return ConsumeResult.FullBatch;     // Found full batch, so more is probably queued: run again right away
                }
            }
            catch (Exception exception)
            {
#if DEBUG
                if (exception.MustBeRethrownImmediately())
                {
                    throw;  // Throwing exceptions here will crash the entire application (.NET 2.0 behavior)
                }
#endif
                InternalLogger.Error(exception, "{0}: Error in lazy writer procedure.", this);  // Something went wrong, let the reader throttle the retry
            }

            // More might have arrived while writing the last batch. Let the reader throttle, so it is written as a larger batch.
            return _channel.IsEmpty ? ConsumeResult.Drained : ConsumeResult.MorePending;
        }

        private void FlushEventsInQueue(object? state)
        {
            var asyncContinuation = state as AsyncContinuation;
            if (Monitor.TryEnter(_writeLockObject, 1500))
            {
                try
                {
                    WriteLogEventsInQueue(int.MaxValue, "Flush Async");
                    if (asyncContinuation != null)
                        base.FlushAsync(asyncContinuation);
                }
                catch (Exception exception)
                {
#if DEBUG
                    if (exception.MustBeRethrownImmediately())
                    {
                        throw;  // Throwing exceptions here will crash the entire application (.NET 2.0 behavior)
                    }
#endif
                    InternalLogger.Error(exception, "{0}: Error in flush procedure.", this);
                }
                finally
                {
                    Monitor.Exit(_writeLockObject);
                    if (TimeToSleepBetweenBatches <= 1 && !_channel.IsEmpty)
                    {
                        _reader?.Signal();  // More arrived while flushing
                    }
                }
            }
            else
            {
                asyncContinuation?.Invoke(new NLogRuntimeException($"Target {this} failed to flush after lock timeout."));
            }
        }

        private int WriteLogEventsInQueue(int batchSize, string reason)
        {
            if (WrappedTarget is null)
            {
                InternalLogger.Debug("{0}: No output because WrappedTarget is NULL", this);
                return 0;
            }

            if (_missingServiceTypes)
            {
                if (WrappedTarget.InitializeMissingDependencyException != null)
                {
                    return 0;
                }

                _missingServiceTypes = false;
                InternalLogger.Debug("{0}: WrappedTarget has resolved missing dependency", this);
            }

            if (batchSize == int.MaxValue)
            {
                var logEvents = _channel.ReadAll();
                return WriteLogEventsToTarget(logEvents, reason);
            }
            else
            {
                int lastBatchSize = 0;

                for (int i = 0; i < FullBatchSizeWriteLimit; ++i)
                {
                    using (var targetList = _reusableAsyncLogEventList.Allocate())
                    {
                        var logEvents = targetList.Result;
                        _channel.ReadBatch(batchSize, logEvents);
                        lastBatchSize = WriteLogEventsToTarget(logEvents, reason);
                    }

                    if (lastBatchSize < batchSize)
                        break;
                }

                return lastBatchSize;
            }
        }

        private int WriteLogEventsToTarget(IList<AsyncLogEventInfo> logEvents, string reason)
        {
            int batchSize = logEvents.Count;
            if (batchSize > 0)
            {
                if (reason != null)
                    InternalLogger.Trace("{0}: Writing {1} events ({2})", this, batchSize, reason);
                WrappedTarget?.WriteAsyncLogEvents(logEvents);
            }

            return batchSize;
        }

        private void OnRequestQueueDropItem(object? sender, LogEventDroppedEventArgs logEventDroppedEventArgs)
        {
            _logEventDroppedEvent?.Invoke(this, logEventDroppedEventArgs);
        }

        private void OnRequestQueueGrow(object? sender, LogEventQueueGrowEventArgs logEventQueueGrowEventArgs)
        {
            _eventQueueGrowEvent?.Invoke(this, logEventQueueGrowEventArgs);
        }
    }
}
