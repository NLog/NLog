// 
// Copyright (c) 2004,2005 Jaroslaw Kowalski <jkowalski@users.sourceforge.net>
// 
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
// * Neither the name of the Jaroslaw Kowalski nor the names of its 
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

#if !NETCF

using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Reflection;
using System.Diagnostics;

using NLog.Internal;
using System.Web;

using NLog.Config;

namespace NLog.Targets.Wrappers
{
    /// <summary>
    /// A target that buffers log events for the duration of 
    /// the ASP.NET Request and sends them down to the wrapped target
    /// as soon as the request ends.
    /// </summary>
    [Target("ASPNetBufferingWrapper",IgnoresLayout=true,IsWrapper=true)]
    public class ASPNetBufferingTargetWrapper: WrapperTargetBase
    {
        private object _dataSlot = new object();
        private int _bufferSize = 100;
        private int _growLimit = 0;
        private bool _growBufferAsNeeded = true;

        /// <summary>
        /// Creates a new instance of the <see cref="ASPNetBufferingTargetWrapper"/> and initializes <see cref="BufferSize"/> to 100.
        /// </summary>
        public ASPNetBufferingTargetWrapper()
        {
            BufferSize = 100;
            GrowBufferAsNeeded = true;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="ASPNetBufferingTargetWrapper"/>, initializes <see cref="BufferSize"/> to 100 and
        /// sets the <see cref="WrapperTargetBase.WrappedTarget"/> to the specified value.
        /// </summary>
        public ASPNetBufferingTargetWrapper(Target writeTo) : this(writeTo, 100)
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="ASPNetBufferingTargetWrapper"/>, initializes <see cref="BufferSize"/> and
        /// the <see cref="WrapperTargetBase.WrappedTarget"/> to the specified values.
        /// </summary>
        public ASPNetBufferingTargetWrapper(Target writeTo, int bufferSize)
        {
            WrappedTarget = writeTo;
            BufferSize = bufferSize;
        }

        /// <summary>
        /// The number of log events to be buffered.
        /// </summary>
        [System.ComponentModel.DefaultValue(4000)]
        public int BufferSize
        {
            get { return _bufferSize; }
            set { _bufferSize = value; }
        }

        /// <summary>
        /// Grow the buffer when it gets full.
        /// </summary>
        /// <remarks>
        /// true causes the buffer to expand until <see cref="BufferGrowLimit" /> is hit,
        /// false causes the buffer to never expand and lose the earliest entries in case of overflow.
        /// </remarks>
        [System.ComponentModel.DefaultValue(false)]
        public bool GrowBufferAsNeeded
        {
            get { return _growBufferAsNeeded; }
            set { _growBufferAsNeeded = value; }
        }

        /// <summary>
        /// The maximum number of log events that the buffer can keep.
        /// </summary>
        public int BufferGrowLimit
        {
            get { return _growLimit; }
            set 
            {
                _growLimit = value; 
                GrowBufferAsNeeded = (value >= _bufferSize) ? true : false;
            }
        }

        public override void Initialize()
        {
            NLog.Web.NLogHttpModule.BeginRequest += new EventHandler(this.OnBeginRequest);
            NLog.Web.NLogHttpModule.EndRequest += new EventHandler(this.OnEndRequest);
        }

        /// <summary>
        /// Adds the specified log event to the buffer.
        /// </summary>
        /// <param name="logEvent">The log event.</param>
        protected internal override void Write(LogEventInfo logEvent)
        {
            WrappedTarget.PrecalculateVolatileLayouts(logEvent);
            LogEventInfoBuffer buffer = GetRequestBuffer();
            if (buffer != null)
            {
                buffer.Append(logEvent);
            }
        }

        /// <summary>
        /// Closes the target by flushing pending events in the buffer (if any).
        /// </summary>
        protected internal override void Close()
        {
            Flush(TimeSpan.FromSeconds(3));
            base.Close ();
        }

        private LogEventInfoBuffer GetRequestBuffer()
        {
            HttpContext context = HttpContext.Current;
            if (context == null)
                return null;
            return context.Items[_dataSlot] as LogEventInfoBuffer;
        }

        private void OnBeginRequest(object sender, EventArgs args)
        {
            HttpContext context = HttpContext.Current;
            context.Items[_dataSlot] = new LogEventInfoBuffer(this.BufferSize, this.GrowBufferAsNeeded, this.BufferGrowLimit);
        }

        private void OnEndRequest(object sender, EventArgs args)
        {
            LogEventInfoBuffer buffer = GetRequestBuffer();
            if (buffer != null)
            {
                LogEventInfo[] events = buffer.GetEventsAndClear();
                if (events.Length > 0)
                {
                    WrappedTarget.Write(events);
                }
            }
        }
    }
}

#endif
