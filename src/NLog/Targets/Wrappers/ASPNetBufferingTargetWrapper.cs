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
    /// <remarks>
    /// <p>
    /// Typically this target is used in cooperation with PostFilteringTargetWrapper
    /// to provide verbose logging for failing requests and normal or no logging for
    /// successful requests. We need to make the decision of the final filtering rule
    /// to apply after all logs for a page have been generated.
    /// </p>
    /// <p>
    /// To use this target, you need to add an entry in the httpModules section of
    /// web.config:
    /// </p>
    /// <code lang="XML">
    /// <![CDATA[<?xml version="1.0" ?>
    /// <configuration>
    ///   <system.web>
    ///     <httpModules>
    ///       <add name="NLog" type="NLog.Web.NLogHttpModule, NLog"/>
    ///     </httpModules>
    ///   </system.web>
    /// </configuration>
    /// ]]>
    /// </code>
    /// </remarks>
    /// <example>
    /// <p>To set up the ASP.NET Buffering target wrapper <a href="config.html">configuration file</a>, put
    /// the following in <c>web.nlog</c> file in your web application directory (this assumes
    /// that PostFilteringWrapper is used to provide the filtering and actual logs go to
    /// a file).
    /// </p>
    /// <code lang="XML" src="examples/targets/Configuration File/ASPNetBufferingWrapper/web.nlog" />
    /// <p>
    /// This assumes just one target and a single rule. More configuration
    /// options are described <a href="config.html">here</a>.
    /// </p>
    /// <p>
    /// To configure the target programmatically, put the following
    /// piece of code in your <c>Application_OnStart()</c> handler in Global.asax.cs 
    /// or some other place that gets executed at the very beginning of your code:
    /// </p>
    /// <code lang="C#" src="examples/targets/Configuration API/ASPNetBufferingWrapper/Global.asax.cs" />
    /// <p>
    /// Fully working C# project can be found in the <c>Examples/Targets/Configuration API/ASPNetBufferingWrapper</c>
    /// directory along with usage instructions.
    /// </p>
    /// </example>
    [Target("ASPNetBufferingWrapper", IgnoresLayout = true, IsWrapper = true)]
    [NotSupportedRuntime(Framework=RuntimeFramework.DotNetCompactFramework)]
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

        /// <summary>
        /// Initializes the target by hooking up the NLogHttpModule BeginRequest and EndRequest events.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();
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
