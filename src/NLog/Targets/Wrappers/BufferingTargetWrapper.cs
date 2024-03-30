// 
// Copyright (c) 2004-2021 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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
    using System.Threading;
    using NLog.Common;
    using NLog.Internal;
    using NLog.Layouts;

    /// <summary>
    /// A target that buffers log events and sends them in batches to the wrapped target.
    /// </summary>
    /// <remarks>
    /// <a href="https://github.com/nlog/nlog/wiki/BufferingWrapper-target">See NLog Wiki</a>
    /// </remarks>
    /// <seealso href="https://github.com/nlog/nlog/wiki/BufferingWrapper-target">Documentation on NLog Wiki</seealso>
    [Target("BufferingWrapper", IsWrapper = true)]
    public class BufferingTargetWrapper : WrapperTargetBase
    {
        private AsyncRequestQueue _buffer;
        private Timer _flushTimer;
        private readonly object _lockObject = new object();

        /// <summary>
        /// Initializes a new instance of the <see cref="BufferingTargetWrapper" /> class.
        /// </summary>
        public BufferingTargetWrapper()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BufferingTargetWrapper" /> class.
        /// </summary>
        /// <param name="name">Name of the target.</param>
        /// <param name="wrappedTarget">The wrapped target.</param>
        public BufferingTargetWrapper(string name, Target wrappedTarget)
            : this(wrappedTarget)
        {
            Name = name ?? Name;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BufferingTargetWrapper" /> class.
        /// </summary>
        /// <param name="wrappedTarget">The wrapped target.</param>
        public BufferingTargetWrapper(Target wrappedTarget)
            : this(wrappedTarget, 100)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BufferingTargetWrapper" /> class.
        /// </summary>
        /// <param name="wrappedTarget">The wrapped target.</param>
        /// <param name="bufferSize">Size of the buffer.</param>
        public BufferingTargetWrapper(Target wrappedTarget, int bufferSize)
            : this(wrappedTarget, bufferSize, -1)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BufferingTargetWrapper" /> class.
        /// </summary>
        /// <param name="wrappedTarget">The wrapped target.</param>
        /// <param name="bufferSize">Size of the buffer.</param>
        /// <param name="flushTimeout">The flush timeout.</param>
        public BufferingTargetWrapper(Target wrappedTarget, int bufferSize, int flushTimeout)
            : this(wrappedTarget, bufferSize, flushTimeout, BufferingTargetWrapperOverflowAction.Flush)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BufferingTargetWrapper" /> class.
        /// </summary>
        /// <param name="wrappedTarget">The wrapped target.</param>
        /// <param name="bufferSize">Size of the buffer.</param>
        /// <param name="flushTimeout">The flush timeout.</param>
        /// <param name="overflowAction">The action to take when the buffer overflows.</param>
        public BufferingTargetWrapper(Target wrappedTarget, int bufferSize, int flushTimeout, BufferingTargetWrapperOverflowAction overflowAction)
        {
            Name = string.IsNullOrEmpty(wrappedTarget?.Name) ? Name : (wrappedTarget.Name + "_wrapper");
            WrappedTarget = wrappedTarget;
            BufferSize = bufferSize;
            FlushTimeout = flushTimeout;
            OverflowAction = overflowAction;
        }

        /// <summary>
        /// Gets or sets the number of log events to be buffered.
        /// </summary>
        /// <docgen category='Buffering Options' order='10' />
        public Layout<int> BufferSize { get; set; }

        /// <summary>
        /// Gets or sets the timeout (in milliseconds) after which the contents of buffer will be flushed 
        /// if there's no write in the specified period of time. Use -1 to disable timed flushes.
        /// </summary>
        /// <docgen category='Buffering Options' order='100' />
        public Layout<int> FlushTimeout { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to use sliding timeout.
        /// </summary>
        /// <remarks>
        /// This value determines how the inactivity period is determined. If sliding timeout is enabled,
        /// the inactivity timer is reset after each write, if it is disabled - inactivity timer will 
        /// count from the first event written to the buffer. 
        /// </remarks>
        /// <docgen category='Buffering Options' order='100' />
        public bool SlidingTimeout { get; set; } = true;

        /// <summary>
        /// Gets or sets the action to take if the buffer overflows.
        /// </summary>
        /// <remarks>
        /// Setting to <see cref="BufferingTargetWrapperOverflowAction.Discard"/> will replace the
        /// oldest event with new events without sending events down to the wrapped target, and
        /// setting to <see cref="BufferingTargetWrapperOverflowAction.Flush"/> will flush the
        /// entire buffer to the wrapped target.
        /// </remarks>
        /// <docgen category='Buffering Options' order='50' />
        public BufferingTargetWrapperOverflowAction OverflowAction { get; set; }

        /// <summary>
        /// Flushes pending events in the buffer (if any), followed by flushing the WrappedTarget.
        /// </summary>
        /// <param name="asyncContinuation">The asynchronous continuation.</param>
        protected override void FlushAsync(AsyncContinuation asyncContinuation)
        {
            WriteEventsInBuffer("Flush Async");
            base.FlushAsync(asyncContinuation);
        }

        /// <inheritdoc/>
        protected override void InitializeTarget()
        {
            base.InitializeTarget();

            var bufferSize = RenderLogEvent(BufferSize, LogEventInfo.CreateNullEvent());

            _buffer = new AsyncRequestQueue(bufferSize, AsyncTargetWrapperOverflowAction.Discard);
            InternalLogger.Trace("{0}: Create Timer", this);

            var flushTimeout = RenderLogEvent(FlushTimeout, LogEventInfo.CreateNullEvent());
            _flushTimer = new Timer(FlushCallback, (int?)flushTimeout, Timeout.Infinite, Timeout.Infinite);
        }

        /// <summary>
        /// Closes the target by flushing pending events in the buffer (if any).
        /// </summary>
        protected override void CloseTarget()
        {
            var currentTimer = _flushTimer;
            if (currentTimer != null)
            {
                _flushTimer = null;
                if (currentTimer.WaitForDispose(TimeSpan.FromSeconds(1)))
                {
                    if (OverflowAction == BufferingTargetWrapperOverflowAction.Discard)
                    {
                        _buffer.Clear();
                    }
                    else
                    {
                        WriteEventsInBuffer("Closing Target");
                    }
                }
            }

            base.CloseTarget();
        }

        /// <summary>
        /// Adds the specified log event to the buffer and flushes
        /// the buffer in case the buffer gets full.
        /// </summary>
        /// <param name="logEvent">The log event.</param>
        protected override void Write(AsyncLogEventInfo logEvent)
        {
            PrecalculateVolatileLayouts(logEvent.LogEvent);

            var firstEventInQueue = _buffer.Enqueue(logEvent);
            if (_buffer.RequestCount >= _buffer.RequestLimit)
            {
                // If the OverflowAction action is set to "Discard", the buffer will automatically
                // roll over the oldest item.
                if (OverflowAction == BufferingTargetWrapperOverflowAction.Flush)
                {
                    WriteEventsInBuffer("Exceeding BufferSize");
                }
            }
            else
            {
                if (SlidingTimeout || firstEventInQueue)
                {
                    var flushTimeout = RenderLogEvent(FlushTimeout, logEvent.LogEvent);
                    if (flushTimeout > 0)
                    {
                        // reset the timer on first item added to the buffer or whenever SlidingTimeout is set to true
                        _flushTimer.Change(flushTimeout, -1);
                    }
                }
            }
        }

        private void FlushCallback(object state)
        {
            bool lockTaken = false;

            try
            {
                var flushTimeout = (state as int?) ?? 0;
                int timeoutMilliseconds = Math.Min(flushTimeout / 2, 100);
                lockTaken = Monitor.TryEnter(_lockObject, timeoutMilliseconds);
                if (lockTaken)
                {
                    if (_flushTimer is null)
                        return;

                    WriteEventsInBuffer(null);
                }
                else
                {
                    if (!_buffer.IsEmpty)
                        _flushTimer?.Change(timeoutMilliseconds, -1);   // Schedule new retry timer
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
                InternalLogger.Error(exception, "{0}: Error in flush procedure.", this);
            }
            finally
            {
                if (lockTaken)
                {
                    Monitor.Exit(_lockObject);
                }
            }
        }

        private void WriteEventsInBuffer(string reason)
        {
            if (WrappedTarget is null)
            {
                InternalLogger.Error("{0}: WrappedTarget is NULL", this);
                return;
            }

            lock (_lockObject)
            {
                AsyncLogEventInfo[] logEvents = _buffer.DequeueBatch(int.MaxValue);
                if (logEvents.Length > 0)
                {
                    if (reason != null)
                        InternalLogger.Trace("{0}: Writing {1} events ({2})", this, logEvents.Length, reason);
                    WrappedTarget.WriteAsyncLogEvents(logEvents);
                }
            }
        }
    }
}
