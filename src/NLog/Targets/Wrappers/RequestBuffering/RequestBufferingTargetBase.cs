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

namespace NLog.Targets.Wrappers.RequestBuffering
{
    /// <summary>
    /// A base class for creating smart per-request buffering target wrappers.
    /// The wrappers gather all log events making up the each request and 
    /// forwarding the events depending on the request status. 
    /// Request is usually a HTTP request but can be any logical transaction.
    /// </summary>
    public abstract class RequestBufferingTargetBase: WrapperTargetBase
    {
        private int _bufferSize;
        private int _growLimit = 0;
        private bool _growBufferAsNeeded = false;
        private EndRequestObject _endRequestObject;

        /// <summary>
        /// Creates a new instance of <see cref="RequestBufferingTargetBase"/>.
        /// </summary>
        public RequestBufferingTargetBase()
        {
            BufferSize = 4000;
            _endRequestObject = new EndRequestObject(this);
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

        protected internal override void Write(LogEventInfo logEvent)
        {
            LogEventInfoBuffer buffer = GetOrCreateBuffer();
            if (buffer != null)
            {
                buffer.Append(logEvent);
            }
        }

        protected abstract LogEventInfoBuffer GetOrCreateBuffer();

        protected LogEventInfoBuffer CreateBuffer()
        {
            return new LogEventInfoBuffer(this.BufferSize, this.GrowBufferAsNeeded, this.BufferGrowLimit);
        }

        public IDisposable BeginRequest()
        {
            // end previous request (if any)
            EndRequest();
            return _endRequestObject;
        }

        public void EndRequest()
        {
            LogEventInfoBuffer buffer = GetOrCreateBuffer();
            if (buffer != null)
            {
                // get buffered events

                LogEventInfo[] events = buffer.GetEventsAndClear();
                for (int i = 0; i < events.Length; ++i)
                {
                    LogEventInfo e = events[i];
                    if (ShouldWrite(e))
                        WrappedTarget.Write(e);
                }
            }
        }

        protected virtual bool ShouldWrite(LogEventInfo e)
        {
            return e.Level >= LogLevel.Info;
        }

        /// <summary>
        /// Adds all layouts used by this target to the specified collection.
        /// </summary>
        /// <param name="layouts">The collection to add layouts to.</param>
        public override void PopulateLayouts(LayoutCollection layouts)
        {
            base.PopulateLayouts (layouts);
            WrappedTarget.PopulateLayouts(layouts);
        }

        class EndRequestObject : IDisposable
        {
            private RequestBufferingTargetBase _target;

            public EndRequestObject(RequestBufferingTargetBase target)
            {
                _target = target;
            }

            public void Dispose()
            {
                _target.EndRequest();
            }
        }
    }
}