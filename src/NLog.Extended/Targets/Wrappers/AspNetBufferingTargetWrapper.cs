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

#if !NET_CF && !SILVERLIGHT

namespace NLog.Targets.Wrappers
{
    using System;
    using System.ComponentModel;
    using System.Web;
    using NLog.Common;
    using NLog.Internal;
    using NLog.Web;

    /// <summary>
    /// Buffers log events for the duration of ASP.NET request and sends them down 
    /// to the wrapped target at the end of a request.
    /// </summary>
    /// <seealso href="http://nlog-project.org/wiki/AspNetBufferingWrapper_target">Documentation on NLog Wiki</seealso>
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
    /// <code lang="XML" source="examples/targets/Configuration File/ASPNetBufferingWrapper/web.nlog" />
    /// <p>
    /// This assumes just one target and a single rule. More configuration
    /// options are described <a href="config.html">here</a>.
    /// </p>
    /// <p>
    /// To configure the target programmatically, put the following
    /// piece of code in your <c>Application_OnStart()</c> handler in Global.asax.cs 
    /// or some other place that gets executed at the very beginning of your code:
    /// </p>
    /// <code lang="C#" source="examples/targets/Configuration API/ASPNetBufferingWrapper/Global.asax.cs" />
    /// <p>
    /// Fully working C# project can be found in the <c>Examples/Targets/Configuration API/ASPNetBufferingWrapper</c>
    /// directory along with usage instructions.
    /// </p>
    /// </example>
    [Target("AspNetBufferingWrapper", IsWrapper = true)]
    public class AspNetBufferingTargetWrapper : WrapperTargetBase
    {
        private readonly object dataSlot = new object();
        private int growLimit;

        /// <summary>
        /// Initializes a new instance of the <see cref="AspNetBufferingTargetWrapper" /> class.
        /// </summary>
        public AspNetBufferingTargetWrapper()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AspNetBufferingTargetWrapper" /> class.
        /// </summary>
        /// <param name="wrappedTarget">The wrapped target.</param>
        public AspNetBufferingTargetWrapper(Target wrappedTarget)
            : this(wrappedTarget, 100)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AspNetBufferingTargetWrapper" /> class.
        /// </summary>
        /// <param name="wrappedTarget">The wrapped target.</param>
        /// <param name="bufferSize">Size of the buffer.</param>
        public AspNetBufferingTargetWrapper(Target wrappedTarget, int bufferSize)
        {
            this.WrappedTarget = wrappedTarget;
            this.BufferSize = bufferSize;
            this.GrowBufferAsNeeded = true;
        }

        /// <summary>
        /// Gets or sets the number of log events to be buffered.
        /// </summary>
        /// <docgen category='Buffering Options' order='100' />
        [DefaultValue(100)]
        public int BufferSize { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether buffer should grow as needed.
        /// </summary>
        /// <value>A value of <c>true</c> if buffer should grow as needed; otherwise, <c>false</c>.</value>
        /// <remarks>
        /// Value of <c>true</c> causes the buffer to expand until <see cref="BufferGrowLimit"/> is hit,
        /// <c>false</c> causes the buffer to never expand and lose the earliest entries in case of overflow.
        /// </remarks>
        /// <docgen category='Buffering Options' order='100' />
        [DefaultValue(false)]
        public bool GrowBufferAsNeeded { get; set; }
        
        /// <summary>
        /// Gets or sets the maximum number of log events that the buffer can keep.
        /// </summary>
        /// <docgen category='Buffering Options' order='100' />
        public int BufferGrowLimit
        {
            get
            {
                return this.growLimit;
            }

            set
            {
                this.growLimit = value;
                this.GrowBufferAsNeeded = (value >= this.BufferSize) ? true : false;
            }
        }

        /// <summary>
        /// Initializes the target by hooking up the NLogHttpModule BeginRequest and EndRequest events.
        /// </summary>
        protected override void InitializeTarget()
        {
            base.InitializeTarget();

            NLogHttpModule.BeginRequest += this.OnBeginRequest;
            NLogHttpModule.EndRequest += this.OnEndRequest;

            if (HttpContext.Current != null)
            {
                // we are in the context already, it's too late for OnBeginRequest to be called, so let's 
                // just call it ourselves
                this.OnBeginRequest(null, null);
            }
        }

        /// <summary>
        /// Closes the target by flushing pending events in the buffer (if any).
        /// </summary>
        protected override void CloseTarget()
        {
            NLogHttpModule.BeginRequest -= this.OnBeginRequest;
            NLogHttpModule.EndRequest -= this.OnEndRequest;
            base.CloseTarget();
        }

        /// <summary>
        /// Adds the specified log event to the buffer.
        /// </summary>
        /// <param name="logEvent">The log event.</param>
        protected override void Write(AsyncLogEventInfo logEvent)
        {
            LogEventInfoBuffer buffer = this.GetRequestBuffer();
            if (buffer != null)
            {
                this.WrappedTarget.PrecalculateVolatileLayouts(logEvent.LogEvent);

                buffer.Append(logEvent);
                InternalLogger.Trace("Appending log event {0} to ASP.NET request buffer.", logEvent.LogEvent.SequenceID);
            }
            else
            {
                InternalLogger.Trace("ASP.NET request buffer does not exist. Passing to wrapped target.");
                this.WrappedTarget.WriteAsyncLogEvent(logEvent);
            }
        }

        private LogEventInfoBuffer GetRequestBuffer()
        {
            HttpContext context = HttpContext.Current;
            if (context == null)
            {
                return null;
            }

            return context.Items[this.dataSlot] as LogEventInfoBuffer;
        }

        private void OnBeginRequest(object sender, EventArgs args)
        {
            InternalLogger.Trace("Setting up ASP.NET request buffer.");
            HttpContext context = HttpContext.Current;
            context.Items[this.dataSlot] = new LogEventInfoBuffer(this.BufferSize, this.GrowBufferAsNeeded, this.BufferGrowLimit);
        }

        private void OnEndRequest(object sender, EventArgs args)
        {
            LogEventInfoBuffer buffer = this.GetRequestBuffer();
            if (buffer != null)
            {
                InternalLogger.Trace("Sending buffered events to wrapped target: {0}.", this.WrappedTarget);
                AsyncLogEventInfo[] events= buffer.GetEventsAndClear();
                this.WrappedTarget.WriteAsyncLogEvents(events);
            }
        }
    }
}

#endif
