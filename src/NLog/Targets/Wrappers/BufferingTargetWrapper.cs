// 
// Copyright (c) 2004-2006 Jaroslaw Kowalski <jaak@jkowalski.net>
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
using System.IO;
using System.Text;
using System.Xml;
using System.Reflection;
using System.Diagnostics;

using NLog.Internal;
using System.Net;
using System.Net.Sockets;

using NLog.Config;
using System.Threading;

namespace NLog.Targets.Wrappers
{
    /// <summary>
    /// A target that buffers log events and sends them in batches to the wrapped target.
    /// </summary>
    /// <example>
    /// <p>
    /// To set up the target in the <a href="config.html">configuration file</a>, 
    /// use the following syntax:
    /// </p>
    /// <code lang="XML" src="examples/targets/Configuration File/BufferingWrapper/NLog.config" />
    /// <p>
    /// The above examples assume just one target and a single rule. See below for
    /// a programmatic configuration that's equivalent to the above config file:
    /// </p>
    /// <code lang="C#" src="examples/targets/Configuration API/BufferingWrapper/Simple/Example.cs" />
    /// </example>
    [Target("BufferingWrapper", IgnoresLayout = true, IsWrapper = true)]
    public class BufferingTargetWrapper: WrapperTargetBase
    {
        private LogEventInfoBuffer _buffer;
        private Timer _flushTimer;
        private int _flushTimeout = -1;

        /// <summary>
        /// Creates a new instance of the <see cref="BufferingTargetWrapper"/> and initializes <see cref="BufferSize"/> to 100.
        /// </summary>
        public BufferingTargetWrapper()
        {
            BufferSize = 100;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="BufferingTargetWrapper"/>, initializes <see cref="BufferSize"/> to 100 and
        /// sets the <see cref="WrapperTargetBase.WrappedTarget"/> to the specified value.
        /// </summary>
        public BufferingTargetWrapper(Target writeTo) : this(writeTo, 100)
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="BufferingTargetWrapper"/>, initializes <see cref="BufferSize"/> and
        /// the <see cref="WrapperTargetBase.WrappedTarget"/> to the specified values.
        /// </summary>
        public BufferingTargetWrapper(Target writeTo, int bufferSize)
        {
            WrappedTarget = writeTo;
            BufferSize = bufferSize;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="BufferingTargetWrapper"/>, 
        /// initializes <see cref="BufferSize"/>, <see cref="WrapperTargetBase.WrappedTarget"/> 
        /// and <see cref="FlushTimeout"/> to the specified values.
        /// </summary>
        public BufferingTargetWrapper(Target writeTo, int bufferSize, int flushTimeout)
        {
            WrappedTarget = writeTo;
            BufferSize = bufferSize;
            FlushTimeout = flushTimeout;
        }

        /// <summary>
        /// Number of log events to be buffered.
        /// </summary>
        [System.ComponentModel.DefaultValue(100)]
        public int BufferSize
        {
            get { return _buffer.Size; }
            set { _buffer = new LogEventInfoBuffer(value, false, 0); }
        }

        /// <summary>
        /// Flush the contents of buffer if there's no write in the specified period of time
        /// (milliseconds). Use -1 to disable timed flushes.
        /// </summary>
        [System.ComponentModel.DefaultValue(-1)]
        public int FlushTimeout
        {
            get { return _flushTimeout; }
            set { _flushTimeout = value; }
        }

        /// <summary>
        /// Initializes the target.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();
            _flushTimer = new Timer(new TimerCallback(this.FlushCallback), null, -1, -1);
        }

        /// <summary>
        /// Adds the specified log event to the buffer and flushes
        /// the buffer in case the buffer gets full.
        /// </summary>
        /// <param name="logEvent">The log event.</param>
        protected internal override void Write(LogEventInfo logEvent)
        {
            lock (this)
            {
                WrappedTarget.PrecalculateVolatileLayouts(logEvent);
                int count = _buffer.Append(logEvent);
                if (count >= BufferSize)
                {
                    LogEventInfo[] events = _buffer.GetEventsAndClear();
                    WrappedTarget.Write(events);
                }
                else
                {
                    if (FlushTimeout > 0 && _flushTimer != null)
                    {
                        _flushTimer.Change(FlushTimeout, -1);
                    }
                }
            }
        }

        /// <summary>
        /// Flushes pending events in the buffer (if any).
        /// </summary>
        public override void Flush(TimeSpan timeout)
        {
            base.Flush (timeout);

            lock (this)
            {
                LogEventInfo[] events = _buffer.GetEventsAndClear();
                if (events.Length > 0)
                {
                    WrappedTarget.Write(events);
                }
            }
        }

        /// <summary>
        /// Closes the target by flushing pending events in the buffer (if any).
        /// </summary>
        protected internal override void Close()
        {
            Flush(TimeSpan.FromSeconds(3));
            base.Close ();
            _flushTimer.Dispose();
            _flushTimer = null;
        }

        void FlushCallback(object state)
        {
            lock (this)
            {
                LogEventInfo[] events = _buffer.GetEventsAndClear();
                if (events.Length > 0)
                {
                    WrappedTarget.Write(events);
                }
            }
        }
   }
}
