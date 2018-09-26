// 
// Copyright (c) 2004-2018 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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

namespace NLog.Targets
{
#if !NET3_5 && !SILVERLIGHT4
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Threading;
    using System.Threading.Tasks;
    using NLog.Common;
    using NLog.Internal;
    using NLog.Targets.Wrappers;

    /// <summary>
    /// Abstract Target with async Task support
    /// </summary>
    public abstract class AsyncTaskTarget : TargetWithContext
    {
        private readonly Timer _taskTimeoutTimer;
        private CancellationTokenSource _cancelTokenSource;
        AsyncRequestQueueBase _requestQueue;
        private readonly Action _taskCancelledToken;
        private readonly Action<Task, object> _taskCompletion;
        private Task _previousTask;
        private Timer _lazyWriterTimer;
        private readonly ReusableAsyncLogEventList _reusableAsyncLogEventList = new ReusableAsyncLogEventList(200);
        private System.Tuple<List<LogEventInfo>, List<AsyncContinuation>> _reusableLogEvents;

        /// <summary>
        /// How many milliseconds to delay the actual write operation to optimize for batching
        /// </summary>
        [DefaultValue(1)]
        public int TaskDelayMilliseconds { get; set; }

        /// <summary>
        /// How many seconds a Task is allowed to run before it is cancelled.
        /// </summary>
        [DefaultValue(150)]
        public int TaskTimeoutSeconds { get; set; }

        /// <summary>
        /// How many attempts to retry the same Task, before it is aborted
        /// </summary>
        [DefaultValue(0)]
        public int RetryCount { get; set; }

        /// <summary>
        /// How many milliseconds to wait before next retry (will double with each retry)
        /// </summary>
        [DefaultValue(500)]
        public int RetryDelayMilliseconds { get; set; }

        /// <summary>
        /// Gets or sets whether to use the locking queue, instead of a lock-free concurrent queue
        /// The locking queue is less concurrent when many logger threads, but reduces memory allocation
        /// </summary>
        [DefaultValue(false)]
        public bool ForceLockingQueue { get => _forceLockingQueue ?? false; set => _forceLockingQueue = value; }
        private bool? _forceLockingQueue;

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
        /// Gets or sets the number of log events that should be processed in a batch
        /// by the lazy writer thread.
        /// </summary>
        /// <docgen category='Buffering Options' order='100' />
        [DefaultValue(1)]
        public int BatchSize { get; set; }

        /// <summary>
        /// Task Scheduler used for processing async Tasks
        /// </summary>
        protected virtual TaskScheduler TaskScheduler => TaskScheduler.Default;

        /// <summary>
        /// Constructor
        /// </summary>
        protected AsyncTaskTarget()
        {
            OptimizeBufferReuse = true;
            TaskTimeoutSeconds = 150;
            TaskDelayMilliseconds = 1;
            BatchSize = 1;
            RetryDelayMilliseconds = 500;

            _taskCompletion = TaskCompletion;
            _taskCancelledToken = TaskCancelledToken;
            _taskTimeoutTimer = new Timer(TaskTimeout, null, Timeout.Infinite, Timeout.Infinite);
            _cancelTokenSource = new CancellationTokenSource();
            _cancelTokenSource.Token.Register(_taskCancelledToken);

#if NETSTANDARD2_0
            // NetStandard20 includes many optimizations for ConcurrentQueue:
            //  - See: https://blogs.msdn.microsoft.com/dotnet/2017/06/07/performance-improvements-in-net-core/
            // Net40 ConcurrencyQueue can seem to leak, because it doesn't clear properly on dequeue
            //  - See: https://blogs.msdn.microsoft.com/pfxteam/2012/05/08/concurrentqueuet-holding-on-to-a-few-dequeued-elements/
            _requestQueue = new ConcurrentRequestQueue(10000, AsyncTargetWrapperOverflowAction.Discard);
#else
            _requestQueue = new AsyncRequestQueue(10000, AsyncTargetWrapperOverflowAction.Discard);
#endif

            _lazyWriterTimer = new Timer(TaskStartNext, null, Timeout.Infinite, Timeout.Infinite);
        }

        /// <summary>
        /// Initializes the internal queue for pending logevents
        /// </summary>
        protected override void InitializeTarget()
        {
            base.InitializeTarget();

            if (BatchSize <= 0)
            {
                BatchSize = 1;
            }

            if (!ForceLockingQueue && OverflowAction == AsyncTargetWrapperOverflowAction.Block && BatchSize * 1.5m > QueueLimit)
            {
                ForceLockingQueue = true;   // ConcurrentQueue does not perform well if constantly hitting QueueLimit
            }

#if NET4_5 || NET4_0
            if (_forceLockingQueue.HasValue && _forceLockingQueue.Value != (_requestQueue is AsyncRequestQueue))
            {
                _requestQueue = ForceLockingQueue ? (AsyncRequestQueueBase)new AsyncRequestQueue(QueueLimit, OverflowAction) : new ConcurrentRequestQueue(QueueLimit, OverflowAction);
            }
#endif

            if (BatchSize > QueueLimit)
            {
                BatchSize = QueueLimit;     // Avoid too much throttling 
            }
        }

        /// <summary>
        /// Override this to create the actual logging task
        /// <example>
        /// Example of how to override this method, and call custom async method
        /// <code>
        /// protected override Task WriteAsyncTask(LogEventInfo logEvent, CancellationToken token)
        /// {
        ///    return CustomWriteAsync(logEvent, token);
        /// }
        /// 
        /// private async Task CustomWriteAsync(LogEventInfo logEvent, CancellationToken token)
        /// {
        ///     await MyLogMethodAsync(logEvent, token).ConfigureAwait(false);
        /// }
        /// </code></example>
        /// </summary>
        /// <param name="logEvent">The log event.</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns></returns>
        protected abstract Task WriteAsyncTask(LogEventInfo logEvent, CancellationToken cancellationToken);

        /// <summary>
        /// Override this to create the actual logging task for handling batch of logevents
        /// </summary>
        /// <param name="logEvents">A batch of logevents.</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns></returns>
        protected virtual Task WriteAsyncTask(IList<LogEventInfo> logEvents, CancellationToken cancellationToken)
        {
            if (logEvents.Count == 1)
            {
                return WriteAsyncTask(logEvents[0], cancellationToken);
            }
            else
            {
                // Should never come here. Only here if someone by mistake configured BatchSize > 1 for target that only handles single LogEventInfo
                Task taskChain = null;
                for (int i = 0; i < logEvents.Count; ++i)
                {
                    LogEventInfo logEvent = logEvents[i];
                    if (taskChain == null)
                        taskChain = WriteAsyncTask(logEvent, cancellationToken);
                    else
                        taskChain = taskChain.ContinueWith(t => WriteAsyncTask(logEvent, cancellationToken), cancellationToken, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler).Unwrap();
                }
                return taskChain;
            }
        }

        /// <summary>
        /// Handle cleanup after failed write operation
        /// </summary>
        /// <param name="exception">Exception from previous failed Task</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <param name="retryCountRemaining">Number of retries remaining</param>
        /// <param name="retryDelay">Time to sleep before retrying</param>
        /// <returns>Should attempt retry</returns>
        protected virtual bool RetryFailedAsyncTask(Exception exception, CancellationToken cancellationToken, int retryCountRemaining, out TimeSpan retryDelay)
        {
            if (cancellationToken.IsCancellationRequested || retryCountRemaining < 0)
            {
                retryDelay = TimeSpan.Zero;
                return false;
            }

            retryDelay = TimeSpan.FromMilliseconds(RetryDelayMilliseconds * (RetryCount - retryCountRemaining) * 2 + RetryDelayMilliseconds);
            return true;
        }

        /// <summary>
        /// Schedules the LogEventInfo for async writing
        /// </summary>
        /// <param name="logEvent">The log event.</param>
        protected override void Write(AsyncLogEventInfo logEvent)
        {
            if (_cancelTokenSource.IsCancellationRequested)
            {
                logEvent.Continuation(null);
                return;
            }

            PrecalculateVolatileLayouts(logEvent.LogEvent);

            bool queueWasEmpty = _requestQueue.Enqueue(logEvent);
            if (queueWasEmpty)
            {
                lock (SyncRoot)
                {
                    if (_previousTask == null)
                    {
                        _lazyWriterTimer.Change(TaskDelayMilliseconds, Timeout.Infinite);
                    }
                }
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
                if (exception.MustBeRethrown())
                {
                    throw;
                }

                logEvent.Continuation(exception);
            }
        }

        /// <summary>
        /// Schedules notification of when all messages has been written
        /// </summary>
        /// <param name="asyncContinuation"></param>
        protected override void FlushAsync(AsyncContinuation asyncContinuation)
        {
            if (_previousTask?.IsCompleted == false || !_requestQueue.IsEmpty)
            {
                InternalLogger.Debug("{0} Flushing {1}", Name, _requestQueue.IsEmpty ? "empty queue" : "pending queue items");
                _requestQueue.Enqueue(new AsyncLogEventInfo(null, asyncContinuation));
                _lazyWriterTimer.Change(0, Timeout.Infinite);
            }
            else
            {
                InternalLogger.Debug("{0} Flushing Nothing", Name);
                asyncContinuation(null);
            }
        }

        /// <summary>
        /// Closes Target by updating CancellationToken 
        /// </summary>
        protected override void CloseTarget()
        {
            _taskTimeoutTimer.Change(Timeout.Infinite, Timeout.Infinite);
            _cancelTokenSource.Cancel();
            _requestQueue.Clear();
            _previousTask = null;
            base.CloseTarget();
        }

        /// <summary>
        /// Releases any managed resources
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                _cancelTokenSource.Dispose();
                _taskTimeoutTimer.WaitForDispose(TimeSpan.Zero);
                _lazyWriterTimer.WaitForDispose(TimeSpan.Zero);
            }
        }

        /// <summary>
        /// Checks the internal queue for the next <see cref="LogEventInfo"/> to create a new task for
        /// </summary>
        /// <param name="previousTask">Used for race-condition validation betweewn task-completion and timeout</param>
        private void TaskStartNext(object previousTask)
        {
            do
            {
                lock (SyncRoot)
                {
                    if (previousTask != null)
                    {
                        if (_previousTask != null && !ReferenceEquals(previousTask, _previousTask))
                            break;

                        _previousTask = null;
                        previousTask = null;
                    }
                    else
                    {
                        if (_previousTask?.IsCompleted == false)
                            break;
                    }

                    if (!IsInitialized)
                        break;

                    if (_requestQueue.IsEmpty)
                    {
                        _previousTask = null;
                        break;
                    }

                    using (var targetList = _reusableAsyncLogEventList.Allocate())
                    {
                        var logEvents = targetList.Result;
                        _requestQueue.DequeueBatch(BatchSize, logEvents);
                        if (logEvents.Count > 0)
                        {
                            if (TaskCreation(logEvents))
                                break;
                        }
                    }
                }
            } while (!_requestQueue.IsEmpty);
        }

        /// <summary>
        /// Generates recursive task-chain to perform retry of writing logevents with increasing retry-delay
        /// </summary>
        internal Task WriteAsyncTaskWithRetry(Task firstTask, IList<LogEventInfo> logEvents, CancellationToken cancellationToken, int retryCount)
        {
            var tcs = new TaskCompletionSource<object>();

            try
            {
                return firstTask.ContinueWith(t =>
                {
                    if (t.IsFaulted || t.IsCanceled)
                    {
                        if (t.Exception != null)
                            tcs.TrySetException(t.Exception);

                        Exception actualException = ExtractActualException(t.Exception);

                        if (RetryFailedAsyncTask(actualException, cancellationToken, retryCount - 1, out var retryDelay))
                        {
                            InternalLogger.Warn(actualException, "{0}: Write operation failed. {1} attempts left. Sleep {2} ms", Name, retryCount, retryDelay.TotalMilliseconds);
                            AsyncHelpers.WaitForDelay(retryDelay);
                            if (!cancellationToken.IsCancellationRequested)
                            {
                                Task retryTask;
                                lock (SyncRoot)
                                {
                                    retryTask = StartWriteAsyncTask(logEvents, cancellationToken);
                                }
                                if (retryTask != null)
                                {
                                    return WriteAsyncTaskWithRetry(retryTask, logEvents, cancellationToken, retryCount - 1);
                                }
                            }
                        }

                        InternalLogger.Warn(actualException, "{0}: Write operation failed after {1} retries", Name, RetryCount - retryCount);
                    }
                    else
                    {
                        tcs.SetResult(null);
                    }
                    return tcs.Task;
                }, cancellationToken, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler).Unwrap();
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }

            return tcs.Task;
        }

        /// <summary>
        /// Creates new task to handle the writing of the input <see cref="LogEventInfo"/>
        /// </summary>
        /// <param name="logEvents">LogEvents to write</param>
        /// <returns>New Task created [true / false]</returns>
        private bool TaskCreation(IList<AsyncLogEventInfo> logEvents)
        {
            System.Tuple<List<LogEventInfo>, List<AsyncContinuation>> reusableLogEvents = null;

            try
            {
                if (_cancelTokenSource.IsCancellationRequested)
                {
                    for (int i = 0; i < logEvents.Count; ++i)
                        logEvents[i].Continuation(null);
                    return false;
                }

                reusableLogEvents = Interlocked.CompareExchange(ref _reusableLogEvents, null, _reusableLogEvents) ?? System.Tuple.Create(new List<LogEventInfo>(), new List<AsyncContinuation>());

                for (int i = 0; i < logEvents.Count; ++i)
                {
                    if (logEvents[i].LogEvent == null)
                    {
                        // Flush Request
                        reusableLogEvents.Item2.Add(logEvents[i].Continuation);
                    }
                    else
                    {
                        reusableLogEvents.Item1.Add(logEvents[i].LogEvent);
                        reusableLogEvents.Item2.Add(logEvents[i].Continuation);
                    }
                }

                if (reusableLogEvents.Item1.Count == 0)
                {
                    // Everything was flush events, no need to schedule write
                    NotifyTaskCompletion(reusableLogEvents.Item2, null);
                    reusableLogEvents.Item2.Clear();
                    Interlocked.CompareExchange(ref _reusableLogEvents, reusableLogEvents, null);
                    InternalLogger.Debug("{0} Flush Completed", Name);
                    return false;
                }

                Task newTask = StartWriteAsyncTask(reusableLogEvents.Item1, _cancelTokenSource.Token);
                if (newTask == null)
                {
                    InternalLogger.Debug("{0} WriteAsyncTask returned null", Name);
                    NotifyTaskCompletion(reusableLogEvents.Item2, null);
                    return false;
                }
                if (RetryCount > 0)
                    newTask = WriteAsyncTaskWithRetry(newTask, reusableLogEvents.Item1, _cancelTokenSource.Token, RetryCount);

                _previousTask = newTask;

                if (TaskTimeoutSeconds > 0)
                    _taskTimeoutTimer.Change(TaskTimeoutSeconds * 1000, Timeout.Infinite);

                // NOTE - Not using _cancelTokenSource for ContinueWith, or else they will also be cancelled on timeout
#if (SILVERLIGHT && !WINDOWS_PHONE) || NET4_0
                newTask.ContinueWith(completedTask => TaskCompletion(completedTask, reusableLogEvents));
#else
                newTask.ContinueWith(_taskCompletion, reusableLogEvents);
#endif
                return true;
            }
            catch (Exception ex)
            {
                _previousTask = null;
                InternalLogger.Error(ex, "{0} WriteAsyncTask failed on creation", Name);
                NotifyTaskCompletion(reusableLogEvents?.Item2, ex);
            }
            return false;
        }

        private Task StartWriteAsyncTask(IList<LogEventInfo> logEvents, CancellationToken cancellationToken)
        {
            try
            {
                var newTask = WriteAsyncTask(logEvents, cancellationToken);
                if (newTask?.Status == TaskStatus.Created)
                    newTask.Start(TaskScheduler);
                return newTask;
            }
            catch (Exception ex)
            {
                if (ex.MustBeRethrownImmediately())
                    throw;

                InternalLogger.Error(ex, "{0} WriteAsyncTask failed on creation", Name);
                return Task.Factory.StartNew(e => throw (Exception)e, new AggregateException(ex), _cancelTokenSource.Token, TaskCreationOptions.None, TaskScheduler);
            }
        }

        private void NotifyTaskCompletion(IList<AsyncContinuation> reusableContinuations, Exception ex)
        {
            try
            {
                for (int i = 0; i < reusableContinuations?.Count; ++i)
                    reusableContinuations[i](ex);
            }
            catch
            {
                // Don't wanna die
            }
        }

        /// <summary>
        /// Handles that scheduled task has completed (succesfully or failed), and starts the next pending task
        /// </summary>
        /// <param name="completedTask">Task just completed</param>
        /// <param name="continuation">AsyncContinuation to notify of success or failure</param>
        private void TaskCompletion(Task completedTask, object continuation)
        {
            bool success = true;

            try
            {
                if (ReferenceEquals(completedTask, _previousTask))
                {
                    if (TaskTimeoutSeconds > 0)
                    {
                        // Prevent timeout-timer from triggering task cancellation token
                        _taskTimeoutTimer.Change(Timeout.Infinite, Timeout.Infinite);
                    }
                }
                else
                {
                    // Not the expected task to complete, most likely noise from retry/recovery
                    success = false;
                    if (!IsInitialized)
                        return;
                }

                var reusableLogEvents = continuation as System.Tuple<List<LogEventInfo>, List<AsyncContinuation>>;
                if (reusableLogEvents != null)
                    NotifyTaskCompletion(reusableLogEvents.Item2, null);
                else
                    success = false;

                if (completedTask.IsCanceled)
                {
                    success = false;
                    if (completedTask.Exception != null)
                        InternalLogger.Warn(completedTask.Exception, "{0} WriteAsyncTask was cancelled", Name);
                    else
                        InternalLogger.Info("{0} WriteAsyncTask was cancelled", Name);
                }
                else if (completedTask.Exception != null)
                {
                    Exception actualException = ExtractActualException(completedTask.Exception);

                    success = false;
                    if (RetryCount <= 0)
                    {
                        if (RetryFailedAsyncTask(actualException, CancellationToken.None, 0, out var retryDelay))
                        {
                            InternalLogger.Warn(actualException, "{0}: WriteAsyncTask failed on completion. Sleep {1} ms", Name, retryDelay.TotalMilliseconds);
                            AsyncHelpers.WaitForDelay(retryDelay);
                        }
                    }
                    else
                    {
                        InternalLogger.Warn(actualException, "{0} WriteAsyncTask failed on completion", Name);
                    }
                }

                if (success)
                {
                    if (OptimizeBufferReuse)
                    {
                        // The expected Task completed with success, allow buffer reuse
                        reusableLogEvents.Item1.Clear();
                        reusableLogEvents.Item2.Clear();
                        Interlocked.CompareExchange(ref _reusableLogEvents, reusableLogEvents, null);
                    }
                }
            }
            finally
            {
                TaskStartNext(completedTask);
            }
        }

        /// <summary>
        /// Timer method, that is fired when pending task fails to complete within timeout
        /// </summary>
        /// <param name="state"></param>
        private void TaskTimeout(object state)
        {
            try
            {
                if (!IsInitialized)
                    return;

                InternalLogger.Warn("{0} WriteAsyncTask had timeout. Task will be cancelled.", Name);

                var previousTask = _previousTask;
                try
                {
                    lock (SyncRoot)
                    {
                        // Check if active Task changed while waiting for SyncRoot-lock
                        if (previousTask != null && ReferenceEquals(previousTask, _previousTask))
                        {
                            _previousTask = null;
                            _cancelTokenSource.Cancel();    // Notice how TaskCancelledToken auto recreates token
                        }
                        else
                        {
                            // Not the expected task to timeout, most likely noise from retry/recovery
                            previousTask = null;
                        }
                    }

                    if (previousTask != null)
                    {
                        if (previousTask.Status != TaskStatus.Canceled &&
                            previousTask.Status != TaskStatus.Faulted &&
                            previousTask.Status != TaskStatus.RanToCompletion)
                        {
                            if (!previousTask.Wait(100))
                            {
                                InternalLogger.Debug("{0} WriteAsyncTask had timeout. Task did not cancel properly: {1}.", Name, previousTask.Status);
                            }
                        }

                        Exception actualException = ExtractActualException(previousTask.Exception);
                        RetryFailedAsyncTask(actualException ?? new TimeoutException("WriteAsyncTask had timeout"), CancellationToken.None, 0, out var retryDelay);
                    }
                }
                catch (Exception ex)
                {
                    InternalLogger.Debug(ex, "{0} WriteAsyncTask had timeout. Task failed to cancel properly.", Name);
                }

                TaskStartNext(null);
            }
            catch (Exception ex)
            {
                InternalLogger.Error(ex, "{0} WriteAsyncTask failed on timeout", Name);
            }
        }

        private static Exception ExtractActualException(AggregateException taskException)
        {
            var flattenExceptions = taskException?.Flatten()?.InnerExceptions;
            Exception actualException = flattenExceptions?.Count == 1 ? flattenExceptions[0] : taskException;
            return actualException;
        }

        private void TaskCancelledToken()
        {
            lock (SyncRoot)
            {
                if (!IsInitialized)
                    return;

                _cancelTokenSource = new CancellationTokenSource();
                _cancelTokenSource.Token.Register(_taskCancelledToken);
            }
        }
    }
#endif
}
