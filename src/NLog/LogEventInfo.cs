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
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using NLog.Internal;
using NLog.Layouts;

namespace NLog
{
    /// <summary>
    /// Represents the logging event.
    /// </summary>
    public abstract class LogEventInfo
    {
        /// <summary>
        /// Gets or sets the date of the first log event created.
        /// </summary>
        public static readonly DateTime ZeroDate = DateTime.Now;
        private readonly int sequenceID;

        private static int globalSequenceID;
        private IDictionary<Layout, string> layoutCache;
        private IDictionary<string, object> eventContext;
#if !NET_CF
        private StackTrace stackTrace;
        private int userStackFrame;
#endif

        /// <summary>
        /// Initializes a new instance of the LogEventInfo class.
        /// </summary>
        /// <param name="level">Log level.</param>
        /// <param name="loggerName">Logger name.</param>
        protected LogEventInfo(LogLevel level, string loggerName)
        {
            this.TimeStamp = CurrentTimeGetter.Now;
            this.Level = level;
            this.LoggerName = loggerName;
            this.sequenceID = Interlocked.Increment(ref globalSequenceID);
        }

        /// <summary>
        /// Gets or sets the timestamp of the logging event.
        /// </summary>
        public DateTime TimeStamp { get; set; }

        /// <summary>
        /// Gets or sets the level of the logging event.
        /// </summary>
        public LogLevel Level { get; set; }

#if !NET_CF
        /// <summary>
        /// Gets a value indicating whether stack trace has been set for this event.
        /// </summary>
        public bool HasStackTrace
        {
            get { return this.stackTrace != null; }
        }

        /// <summary>
        /// Gets the stack frame of the method that did the logging.
        /// </summary>
        public StackFrame UserStackFrame
        {
            get { return (this.stackTrace != null) ? this.stackTrace.GetFrame(this.userStackFrame) : null; }
        }

        /// <summary>
        /// Gets the number index of the stack frame that represents the user
        /// code (not the NLog code).
        /// </summary>
        public int UserStackFrameNumber
        {
            get { return this.userStackFrame; }
        }

        /// <summary>
        /// Gets the entire stack trace.
        /// </summary>
        public StackTrace StackTrace
        {
            get { return this.stackTrace; }
        }
#endif
        /// <summary>
        /// Gets the exception information.
        /// </summary>
        public virtual Exception Exception
        {
            get { return null; }
        }

        /// <summary>
        /// Gets or sets the logger name.
        /// </summary>
        public string LoggerName { get; set; }

        /// <summary>
        /// Gets the formatted message.
        /// </summary>
        public abstract string FormattedMessage { get; }

        /// <summary>
        /// Gets the dictionary of per-event context properties.
        /// </summary>
        public IDictionary<string, object> Context
        {
            get
            {
                if (this.eventContext == null)
                {
                    this.eventContext = new Dictionary<string, object>();
                }

                return this.eventContext;
            }
        }

        /// <summary>
        /// Gets the unique identifier of log event which is automatically generated
        /// and monotonously increasing.
        /// </summary>
        public int SequenceId
        {
            get { return this.sequenceID; }
        }

        /// <summary>
        /// Creates the null event.
        /// </summary>
        /// <returns>Null event (which can be used whenever <see cref="LogEventInfo"/> is required).</returns>
        public static LogEventInfo CreateNullEvent()
        {
            return LogEventInfo.Create(LogLevel.Off, String.Empty, null, String.Empty);
        }

        /// <summary>
        /// Creates <see cref="LogEventInfo"/> for a pre-formatted message.
        /// </summary>
        /// <param name="logLevel">The log level.</param>
        /// <param name="loggerName">Logger name.</param>
        /// <param name="message">Pre-formatted message.</param>
        /// <returns>Instance of <see cref="LogEventInfo"/>.</returns>
        public static LogEventInfo Create(LogLevel logLevel, string loggerName, string message)
        {
            return new FormattedLogEventInfo(logLevel, loggerName, null, "{0}", new object[] { message });
        }

        /// <summary>
        /// Creates <see cref="LogEventInfo"/> for a given value.
        /// </summary>
        /// <typeparam name="T">Type of the value.</typeparam>
        /// <param name="logLevel">The log level.</param>
        /// <param name="loggerName">Name of the logger.</param>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="value">The value to be written.</param>
        /// <returns>Instance of <see cref="LogEventInfo"/>.</returns>
        public static LogEventInfo Create<T>(LogLevel logLevel, string loggerName, IFormatProvider formatProvider, T value)
        {
            return new FormattedLogEventInfo(logLevel, loggerName, formatProvider, "{0}", new object[] { value });
        }

        /// <summary>
        /// Creates <see cref="LogEventInfo"/> for a message which requires formatting.
        /// </summary>
        /// <param name="logLevel">The log level.</param>
        /// <param name="loggerName">Name of the logger.</param>
        /// <param name="formatProvider">The format provider.</param>
        /// <param name="message">The message.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>Instance of <see cref="LogEventInfo"/>.</returns>
        public static LogEventInfo Create(LogLevel logLevel, string loggerName, IFormatProvider formatProvider, string message, object[] parameters)
        {
            return new FormattedLogEventInfo(logLevel, loggerName, formatProvider, message, parameters);
        }

        /// <summary>
        /// Creates <see cref="LogEventInfo"/> associated with an exception.
        /// </summary>
        /// <param name="logLevel">The log level.</param>
        /// <param name="loggerName">Name of the logger.</param>
        /// <param name="message">The message.</param>
        /// <param name="exception">The exception.</param>
        /// <returns>Instance of <see cref="LogEventInfo"/>.</returns>
        public static LogEventInfo Create(LogLevel logLevel, string loggerName, string message, Exception exception)
        {
            return new UnformattedLogEventInfoWithException(logLevel, loggerName, message, exception);
        }

#if !NET_CF
        internal void SetStackTrace(StackTrace stackTrace, int userStackFrame)
        {
            this.stackTrace = stackTrace;
            this.userStackFrame = userStackFrame;
        }
#endif

        internal bool TryGetCachedLayoutValue(Layout layout, out string result)
        {
            if (this.layoutCache == null)
            {
                result = null;
                return false;
            }

            return this.layoutCache.TryGetValue(layout, out result);
        }

        internal void AddCachedLayoutValue(Layout layout, string value)
        {
            if (this.layoutCache == null)
            {
                this.layoutCache = new Dictionary<Layout, string>();
            }

            this.layoutCache[layout] = value;
        }
    }
}