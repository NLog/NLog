// 
// Copyright (c) 2004-2011 Jaroslaw Kowalski <jaak@jkowalski.net>
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
    using System.ComponentModel;
    using System.Threading;
    using NLog.Common;
    using NLog.Internal;

    /// <summary>
    /// A target that buffers log events and sends them in batches to the wrapped target.
    /// </summary>
    /// <seealso href="http://nlog-project.org/wiki/BufferingWrapper_target">Documentation on NLog Wiki</seealso>
    [Target("BufferingWrapper", IsWrapper = true)]
    public class BufferingTargetWrapper : WrapperTargetBase
    {
        private LogEventInfoBuffer buffer;
        private Timer flushTimer;

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
        {
            this.WrappedTarget = wrappedTarget;
            this.BufferSize = bufferSize;
            this.FlushTimeout = flushTimeout;
            this.SlidingTimeout = true;
        }

        /// <summary>
        /// Gets or sets the number of log events to be buffered.
        /// </summary>
        /// <docgen category='Buffering Options' order='100' />
        [DefaultValue(100)]
        public int BufferSize { get; set; }

        /// <summary>
        /// Gets or sets the timeout (in milliseconds) after which the contents of buffer will be flushed 
        /// if there's no write in the specified period of time. Use -1 to disable timed flushes.
        /// </summary>
        /// <docgen category='Buffering Options' order='100' />
        [DefaultValue(-1)]
        public int FlushTimeout { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to use sliding timeout.
        /// </summary>
        /// <remarks>
        /// This value determines how the inactivity period is determined. If sliding timeout is enabled,
        /// the inactivity timer is reset after each write, if it is disabled - inactivity timer will 
        /// count from the first event written to the buffer. 
        /// </remarks>
        /// <docgen category='Buffering Options' order='100' />
        [DefaultValue(true)]
        public bool SlidingTimeout { get; set; }

        /// <summary>
        /// Flushes pending events in the buffer (if any).
        /// </summary>
        /// <param name="asyncContinuation">The asynchronous continuation.</param>
        protected override void FlushAsync(AsyncContinuation asyncContinuation)
        {
            AsyncLogEventInfo[] events = this.buffer.GetEventsAndClear();

            if (events.Length == 0)
            {
                this.WrappedTarget.Flush(asyncContinuation);
            }
            else
            {
                this.WrappedTarget.WriteAsyncLogEvents(events, ex => this.WrappedTarget.Flush(asyncContinuation));
            }
        }

        /// <summary>
        /// Initializes the target.
        /// </summary>
        protected override void InitializeTarget()
        {
            base.InitializeTarget();
            this.buffer = new LogEventInfoBuffer(this.BufferSize, false, 0);
            this.flushTimer = new Timer(this.FlushCallback, null, -1, -1);
        }

        /// <summary>
        /// Closes the target by flushing pending events in the buffer (if any).
        /// </summary>
        protected override void CloseTarget()
        {
            base.CloseTarget();
            if (this.flushTimer != null)
            {
                this.flushTimer.Dispose();
                this.flushTimer = null;
            }
        }

        /// <summary>
        /// Adds the specified log event to the buffer and flushes
        /// the buffer in case the buffer gets full.
        /// </summary>
        /// <param name="logEvent">The log event.</param>
        protected override void Write(AsyncLogEventInfo logEvent)
        {
            this.WrappedTarget.PrecalculateVolatileLayouts(logEvent.LogEvent);

            int count = this.buffer.Append(logEvent);
            if (count >= this.BufferSize)
            {
                AsyncLogEventInfo[] events = this.buffer.GetEventsAndClear();
                this.WrappedTarget.WriteAsyncLogEvents(events);
            }
            else
            {
                if (this.FlushTimeout > 0)
                {
                    // reset the timer on first item added to the buffer or whenever SlidingTimeout is set to true
                    if (this.SlidingTimeout || count == 1)
                    {
                        this.flushTimer.Change(this.FlushTimeout, -1);
                    }
                }
            }
        }

        private void FlushCallback(object state)
        {
            lock (this.SyncRoot)
            {
                if (this.IsInitialized)
                {
                    AsyncLogEventInfo[] events = this.buffer.GetEventsAndClear();
                    if (events.Length > 0)
                    {
                        this.WrappedTarget.WriteAsyncLogEvents(events);
                    }
                }
            }
        }
    }
}
