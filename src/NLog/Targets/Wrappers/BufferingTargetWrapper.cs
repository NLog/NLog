// 
// Copyright (c) 2004-2010 Jaroslaw Kowalski <jaak@jkowalski.net>
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
    using NLog.Internal;

    /// <summary>
    /// A target that buffers log events and sends them in batches to the wrapped target.
    /// </summary>
    /// <example>
    /// <p>
    /// To set up the target in the <a href="config.html">configuration file</a>, 
    /// use the following syntax:
    /// </p>
    /// <code lang="XML" source="examples/targets/Configuration File/BufferingWrapper/NLog.config" />
    /// <p>
    /// The above examples assume just one target and a single rule. See below for
    /// a programmatic configuration that's equivalent to the above config file:
    /// </p>
    /// <code lang="C#" source="examples/targets/Configuration API/BufferingWrapper/Simple/Example.cs" />
    /// </example>
    [Target("BufferingWrapper", IsWrapper = true)]
    public class BufferingTargetWrapper : WrapperTargetBase
    {
        private LogEventInfoBuffer buffer;
        private Timer flushTimer;

        /// <summary>
        /// Initializes a new instance of the <see cref="BufferingTargetWrapper" /> class.
        /// </summary>
        public BufferingTargetWrapper()
        {
            this.FlushTimeout = -1;
            this.BufferSize = 100;
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
        {
            this.FlushTimeout = -1;
            this.WrappedTarget = wrappedTarget;
            this.BufferSize = bufferSize;
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
        /// Flushes pending events in the buffer (if any).
        /// </summary>
        /// <param name="timeout">Maximum time to allow for the flush. Any messages after that time will be discarded.</param>
        public override void Flush(TimeSpan timeout)
        {
            base.Flush(timeout);

            lock (this)
            {
                var events = this.buffer.GetEventsAndClear();
                if (events.Length > 0)
                {
                    WrappedTarget.WriteLogEvents(events);
                }
            }
        }

        /// <summary>
        /// Initializes the target.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            this.buffer = new LogEventInfoBuffer(this.BufferSize, false, 0);
            this.flushTimer = new Timer(this.FlushCallback, null, -1, -1);
        }

        /// <summary>
        /// Closes the target by flushing pending events in the buffer (if any).
        /// </summary>
        protected override void Close()
        {
            this.Flush(TimeSpan.FromSeconds(3));
            base.Close();
            this.flushTimer.Dispose();
            this.flushTimer = null;
        }

        /// <summary>
        /// Adds the specified log event to the buffer and flushes
        /// the buffer in case the buffer gets full.
        /// </summary>
        /// <param name="logEvent">The log event.</param>
        protected override void Write(LogEventInfo logEvent)
        {
            lock (this)
            {
                this.WrappedTarget.PrecalculateVolatileLayouts(logEvent);
                int count = this.buffer.Append(logEvent);
                if (count >= this.BufferSize)
                {
                    var events = this.buffer.GetEventsAndClear();
                    WrappedTarget.WriteLogEvents(events);
                }
                else
                {
                    if (this.FlushTimeout > 0 && this.flushTimer != null)
                    {
                        this.flushTimer.Change(this.FlushTimeout, -1);
                    }
                }
            }
        }

        private void FlushCallback(object state)
        {
            lock (this)
            {
                LogEventInfo[] events = this.buffer.GetEventsAndClear();
                if (events.Length > 0)
                {
                    WrappedTarget.WriteLogEvents(events);
                }
            }
        }
    }
}
