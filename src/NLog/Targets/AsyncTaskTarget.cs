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

namespace NLog.Targets
{
    using System;
    using System.Collections.Generic;
    using System.Threading;

#if !NET3_5 && !SILVERLIGHT4
    using System.Threading.Tasks;
    using NLog.Common;
    using System.ComponentModel;

    /// <summary>
    /// Abstract Target with async Task support
    /// </summary>
    public abstract class AsyncTaskTarget : Target
    {
        private readonly Timer _taskTimeoutTimer;
        private CancellationTokenSource _cancelTokenSource;
        private readonly Queue<AsyncLogEventInfo> _requestQueue;
        private readonly Action _taskStartNext;
        private readonly Action _taskCancelledToken;
        private readonly Action<Task, object> _taskCompletion;
        private Task _previousTask;

        /// <summary>
        /// How many seconds a Task is allowed to run before it is cancelled.
        /// </summary>
        [DefaultValue(150)]
        public int TaskTimeoutSeconds { get; set; }

        /// <summary>
        /// Task Scheduler used for processing async Tasks
        /// </summary>
        protected virtual TaskScheduler TaskScheduler { get { return TaskScheduler.Default; } }

        /// <summary>
        /// Constructor
        /// </summary>
        protected AsyncTaskTarget()
        {
            TaskTimeoutSeconds = 150;

            _taskStartNext = () => TaskStartNext(null);
            _taskCompletion = TaskCompletion;
            _taskCancelledToken = TaskCancelledToken;
            _taskTimeoutTimer = new Timer(TaskTimeout);
            _cancelTokenSource = new CancellationTokenSource();
            _cancelTokenSource.Token.Register(_taskCancelledToken);
            _requestQueue = new Queue<AsyncLogEventInfo>(10000);
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

            this.MergeEventProperties(logEvent.LogEvent);
            this.PrecalculateVolatileLayouts(logEvent.LogEvent);

            _requestQueue.Enqueue(logEvent);
            if (_previousTask == null)
            {
                _previousTask = Task.Factory.StartNew(_taskStartNext, _cancelTokenSource.Token, TaskCreationOptions.None, TaskScheduler);
            }
        }

        /// <summary>
        /// Schedules notification of when all messages has been written
        /// </summary>
        /// <param name="asyncContinuation"></param>
        protected override void FlushAsync(AsyncContinuation asyncContinuation)
        {
            if (_previousTask == null)
            {
                InternalLogger.Debug("{0} Flushing Nothing", this.Name);
                asyncContinuation(null);
            }
            else
            {
                InternalLogger.Debug("{0} Flushing {1} items", this.Name, _requestQueue.Count + 1);
                _requestQueue.Enqueue(new AsyncLogEventInfo(null, asyncContinuation));
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
                _taskTimeoutTimer.Dispose();
            }
        }

        /// <summary>
        /// Checks the internal queue for the next <see cref="LogEventInfo"/> to create a new task for
        /// </summary>
        /// <param name="previousTask">Used for race-condition validation betweewn task-completion and timeout</param>
        private void TaskStartNext(Task previousTask)
        {
            AsyncLogEventInfo logEvent;
            do
            {
                lock (this.SyncRoot)
                {
                    if (!IsInitialized)
                        break;

                    if (previousTask != null && !ReferenceEquals(previousTask, _previousTask))
                        break;

                    if (_requestQueue.Count == 0)
                    {
                        _previousTask = null;
                        break;
                    }

                    logEvent = _requestQueue.Dequeue();
                }
            } while (!TaskCreation(logEvent));
        }

        /// <summary>
        /// Creates new task to handle the writing of the input <see cref="LogEventInfo"/>
        /// </summary>
        /// <param name="logEvent">LogEvent to write</param>
        /// <returns>New Task created [true / false]</returns>
        private bool TaskCreation(AsyncLogEventInfo logEvent)
        {
            try
            {
                if (_cancelTokenSource.IsCancellationRequested)
                {
                    logEvent.Continuation(null);
                    return false;
                }

                if (logEvent.LogEvent == null)
                {
                    InternalLogger.Debug("{0} Flush Completed", this.Name);
                    logEvent.Continuation(null);
                    return false;
                }

                var newTask = WriteAsyncTask(logEvent.LogEvent, _cancelTokenSource.Token);
                if (newTask == null)
                {
                    InternalLogger.Debug("{0} WriteAsyncTask returned null", this.Name);
                }
                else
                {
                    lock (this.SyncRoot)
                    {
                        _previousTask = newTask;

                        if (TaskTimeoutSeconds > 0)
                            _taskTimeoutTimer.Change(TaskTimeoutSeconds * 1000, Timeout.Infinite);

                        // NOTE - Not using _cancelTokenSource for ContinueWith, or else they will also be cancelled on timeout
#if (SILVERLIGHT && !WINDOWS_PHONE) || NET4_0
                        var continuation = logEvent.Continuation;
                        _previousTask.ContinueWith(completedTask => TaskCompletion(completedTask, continuation));
#else
                        _previousTask.ContinueWith(_taskCompletion, logEvent.Continuation);
#endif
                        if (_previousTask.Status == TaskStatus.Created)
                            _previousTask.Start(TaskScheduler);
                    }
                    return true;
                }
            }
            catch (Exception ex)
            {
                try
                {
                    InternalLogger.Error(ex, "{0} WriteAsyncTask failed on creation", this.Name);
                    logEvent.Continuation(ex);
                }
                catch
                {
                    // Don't wanna die
                }
            }
            return false;
        }

        /// <summary>
        /// Handles that scheduled task has completed (succesfully or failed), and starts the next pending task
        /// </summary>
        /// <param name="completedTask">Task just completed</param>
        /// <param name="continuation">AsyncContinuation to notify of success or failure</param>
        private void TaskCompletion(Task completedTask, object continuation)
        {
            try
            {
                if (ReferenceEquals(completedTask, _previousTask))
                { 
                    if (TaskTimeoutSeconds > 0)
                    {
                        _taskTimeoutTimer.Change(Timeout.Infinite, Timeout.Infinite);
                    }
                }
                else
                {
                    if (!IsInitialized)
                        return;
                }

                if (completedTask.IsCanceled)
                {
                    if (completedTask.Exception != null)
                        InternalLogger.Warn(completedTask.Exception, "{0} WriteAsyncTask was cancelled", this.Name);
                    else
                        InternalLogger.Info("{0} WriteAsyncTask was cancelled", this.Name);
                }
                else if (completedTask.Exception != null)
                {
                    InternalLogger.Warn(completedTask.Exception, "{0} WriteAsyncTask failed on completion", this.Name);
                }

                var asyncContinuation = (AsyncContinuation)continuation;
                asyncContinuation(completedTask.Exception);
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

                InternalLogger.Warn("{0} WriteAsyncTask had timeout. Task will be cancelled.", this.Name);

                var previousTask = _previousTask;
                try
                {
                    lock (this.SyncRoot)
                    {
                        if (previousTask != null && ReferenceEquals(previousTask, _previousTask))
                        {
                            _previousTask = null;
                            _cancelTokenSource.Cancel();    // Notice how TaskCancelledToken auto recreates token
                        }
                        else
                        {
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
                                InternalLogger.Debug("{0} WriteAsyncTask had timeout. Task did not cancel properly: {1}.", this.Name, previousTask.Status);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    InternalLogger.Debug(ex, "{0} WriteAsyncTask had timeout. Task failed to cancel properly.", this.Name);
                }

                if (previousTask != null)
                {
                    TaskStartNext(null);
                }
            }
            catch (Exception ex)
            {
                InternalLogger.Error(ex, "{0} WriteAsyncTask failed on timeout", this.Name);
            }
        }

        private void TaskCancelledToken()
        {
            lock (this.SyncRoot)
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
