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
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.Threading;

using NLog.Internal;
using NLog.Layouts;

namespace NLog
{
    /// <summary>
    /// Represents the logging event.
    /// </summary>
    public class LogEventInfo
    {
        /// <summary>
        /// Gets the date of the first log event created.
        /// </summary>
        public static readonly DateTime ZeroDate = DateTime.Now;

        private static int globalSequenceId;

        private string formattedMessage;
        private IDictionary<Layout, string> layoutCache;
        private IDictionary eventContext;

        /// <summary>
        /// Initializes a new instance of the LogEventInfo class.
        /// </summary>
        public LogEventInfo()
        {
        }

        /// <summary>
        /// Initializes a new instance of the LogEventInfo class.
        /// </summary>
        /// <param name="level">Log level.</param>
        /// <param name="loggerName">Logger name.</param>
        /// <param name="message">Log message including parameter placeholders.</param>
        public LogEventInfo(LogLevel level, string loggerName, string message)
            : this(level, loggerName, null, message, null, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the LogEventInfo class.
        /// </summary>
        /// <param name="level">Log level.</param>
        /// <param name="loggerName">Logger name.</param>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">Log message including parameter placeholders.</param>
        /// <param name="parameters">Parameter array.</param>
        public LogEventInfo(LogLevel level, string loggerName, IFormatProvider formatProvider, string message, object[] parameters) 
            : this(level, loggerName, formatProvider, message, parameters, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the LogEventInfo class.
        /// </summary>
        /// <param name="level">Log level.</param>
        /// <param name="loggerName">Logger name.</param>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">Log message including parameter placeholders.</param>
        /// <param name="parameters">Parameter array.</param>
        /// <param name="exception">Exception information.</param>
        public LogEventInfo(LogLevel level, string loggerName, IFormatProvider formatProvider, string message, object[] parameters, Exception exception)
        {
            this.TimeStamp = CurrentTimeGetter.Now;
            this.Level = level;
            this.LoggerName = loggerName;
            this.Message = message;
            this.Parameters = parameters;
            this.FormatProvider = formatProvider;
            this.Exception = exception;
            this.SequenceID = Interlocked.Increment(ref globalSequenceId);

            if (NeedToPreformatMessage(parameters))
            {
                this.CalcFormattedMessage();
            }
        }

        /// <summary>
        /// Gets the unique identifier of log event which is automatically generated
        /// and monotonously increasing.
        /// </summary>
        public int SequenceID { get; private set; }

        /// <summary>
        /// Gets or sets the timestamp of the logging event.
        /// </summary>
        public DateTime TimeStamp { get; set; }

        /// <summary>
        /// Gets or sets the level of the logging event.
        /// </summary>
        public LogLevel Level { get; set; }

#if !NETCF
        /// <summary>
        /// Gets a value indicating whether stack trace has been set for this event.
        /// </summary>
        public bool HasStackTrace
        {
            get { return StackTrace != null; }
        }

        /// <summary>
        /// Gets the stack frame of the method that did the logging.
        /// </summary>
        public StackFrame UserStackFrame
        {
            get { return (this.StackTrace != null) ? this.StackTrace.GetFrame(this.UserStackFrameNumber) : null; }
        }

        /// <summary>
        /// Gets the number index of the stack frame that represents the user
        /// code (not the NLog code).
        /// </summary>
        public int UserStackFrameNumber { get; private set; }

        /// <summary>
        /// Gets the entire stack trace.
        /// </summary>
        public StackTrace StackTrace { get; private set; }
#endif

        /// <summary>
        /// Gets or sets the exception information.
        /// </summary>
        public Exception Exception { get; set; }

        /// <summary>
        /// Gets or sets the logger name.
        /// </summary>
        public string LoggerName { get; set; }

        /// <summary>
        /// Gets the logger short name.
        /// </summary>
        [Obsolete("This property should not be used.")]
        public string LoggerShortName
        {
            get
            {
                int lastDot = this.LoggerName.LastIndexOf('.');
                if (lastDot >= 0)
                {
                    return this.LoggerName.Substring(lastDot + 1);
                }

                return this.LoggerName;
            }
        }

        /// <summary>
        /// Gets or sets the log message including any parameter placeholders.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the parameter values or null if no parameters have been specified.
        /// </summary>
        public object[] Parameters { get; set; }

        /// <summary>
        /// Gets or sets the format provider that was provided while logging or <see langword="null" />
        /// when no formatProvider was specified.
        /// </summary>
        public IFormatProvider FormatProvider { get; set; }

        /// <summary>
        /// Gets the formatted message.
        /// </summary>
        public string FormattedMessage
        {
            get 
            {
                if (this.formattedMessage == null)
                {
                    this.CalcFormattedMessage();
                }

                return this.formattedMessage;
            }
        }

        /// <summary>
        /// Gets the dictionary of per-event context properties.
        /// </summary>
        public IDictionary Context
        {
            get
            {
                if (this.eventContext == null)
                {
                    this.eventContext = new HybridDictionary();
                }

                return this.eventContext;
            }
        }

        /// <summary>
        /// Creates the null event.
        /// </summary>
        /// <returns>Null log event.</returns>
        public static LogEventInfo CreateNullEvent()
        {
            return new LogEventInfo(LogLevel.Off, string.Empty, string.Empty);
        }

        /// <summary>
        /// Creates the log event.
        /// </summary>
        /// <param name="logLevel">The log level.</param>
        /// <param name="loggerName">Name of the logger.</param>
        /// <param name="message">The message.</param>
        /// <returns>Instance of <see cref="LogEventInfo"/>.</returns>
        public static LogEventInfo Create(LogLevel logLevel, string loggerName, string message)
        {
            return new LogEventInfo(logLevel, loggerName, null, message, null);
        }

        /// <summary>
        /// Creates the log event.
        /// </summary>
        /// <param name="logLevel">The log level.</param>
        /// <param name="loggerName">Name of the logger.</param>
        /// <param name="formatProvider">The format provider.</param>
        /// <param name="message">The message.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>Instance of <see cref="LogEventInfo"/>.</returns>
        public static LogEventInfo Create(LogLevel logLevel, string loggerName, IFormatProvider formatProvider, string message, object[] parameters)
        {
            return new LogEventInfo(logLevel, loggerName, formatProvider, message, parameters);
        }

        /// <summary>
        /// Creates the log event.
        /// </summary>
        /// <param name="logLevel">The log level.</param>
        /// <param name="loggerName">Name of the logger.</param>
        /// <param name="formatProvider">The format provider.</param>
        /// <param name="message">The message.</param>
        /// <returns>Instance of <see cref="LogEventInfo"/>.</returns>
        public static LogEventInfo Create(LogLevel logLevel, string loggerName, IFormatProvider formatProvider, object message)
        {
            return new LogEventInfo(logLevel, loggerName, formatProvider, "{0}", new[] { message });
        }

        /// <summary>
        /// Creates the log event.
        /// </summary>
        /// <param name="logLevel">The log level.</param>
        /// <param name="loggerName">Name of the logger.</param>
        /// <param name="message">The message.</param>
        /// <param name="exception">The exception.</param>
        /// <returns>Instance of <see cref="LogEventInfo"/>.</returns>
        public static LogEventInfo Create(LogLevel logLevel, string loggerName, string message, Exception exception)
        {
            return new LogEventInfo(logLevel, loggerName, null, message, null, exception);
        }

#if !NETCF
        internal void SetStackTrace(StackTrace stackTrace, int userStackFrame)
        {
            this.StackTrace = stackTrace;
            this.UserStackFrameNumber = userStackFrame;
        }
#endif

        internal void AddCachedLayoutValue(Layout layout, string value)
        {
            if (this.layoutCache == null)
            {
                this.layoutCache = new Dictionary<Layout, string>();
            }

            this.layoutCache[layout] = value;
        }

        internal bool TryGetCachedLayoutValue(Layout layout, out string value)
        {
            if (this.layoutCache == null)
            {
                value = null;
                return false;
            }

            return this.layoutCache.TryGetValue(layout, out value);
        }

        private static bool NeedToPreformatMessage(object[] parameters)
        {
            // we need to preformat message if it contains any parameters which could possibly
            // do logging in their ToString()
            if (parameters == null || parameters.Length == 0)
            {
                return false;
            }

            if (parameters.Length > 3)
            {
                // too many parameters, too costly to check
                return true;
            }

            if (!IsSafeToDeferFormatting(parameters[0]))
            {
                return true;
            }

            if (parameters.Length >= 2)
            {
                if (!IsSafeToDeferFormatting(parameters[1]))
                {
                    return true;
                }
            }

            if (parameters.Length >= 3)
            {
                if (!IsSafeToDeferFormatting(parameters[2]))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsSafeToDeferFormatting(object value)
        {
            if (value == null)
            {
                return true;
            }

            return value.GetType().IsPrimitive || (value is string);
        }

        private void CalcFormattedMessage()
        {
            if (this.Parameters == null || this.Parameters.Length == 0)
            {
                this.formattedMessage = this.Message;
            }
            else
            {
                this.formattedMessage = string.Format(this.FormatProvider ?? CultureInfo.CurrentCulture, this.Message, this.Parameters);
            }
        }
    }
}
