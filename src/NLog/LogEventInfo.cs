// 
// Copyright (c) 2004-2016 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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

namespace NLog
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.Threading;
    using NLog.Common;
    using NLog.Internal;
    using NLog.Layouts;
    using NLog.Time;

    /// <summary>
    /// Represents the logging event.
    /// </summary>
    public class LogEventInfo : Internal.PoolFactory.IPoolObject
    {
        /// <summary>
        /// Gets the date of the first log event created.
        /// </summary>
        public static readonly DateTime ZeroDate = DateTime.UtcNow;

        private static int globalSequenceId;

        private readonly object layoutCacheLock = new object();

        private string formattedMessage;
        private string message;
        private object[] parameters;
        private IFormatProvider formatProvider;
        private IDictionary<Layout, string> layoutCache;
        private IDictionary<object, object> properties;
        private IDictionary eventContextAdapter;

        private readonly PoolHandler _poolHandler;

        /// <summary>
        /// Initializes a new instance of the <see cref="LogEventInfo" /> class.
        /// </summary>
        public LogEventInfo()
        {
            this.TimeStamp = TimeSource.Current.Time;
            this.SequenceID = Interlocked.Increment(ref globalSequenceId);
        }

        internal LogEventInfo(Internal.PoolFactory.LogEventPoolFactory owner)
        {
            _poolHandler = new PoolHandler(this, owner);
        }

        internal void Init(LogLevel level, string loggerName, IFormatProvider formatProvider, [Localizable(false)] string message, object[] parameters, Exception exception)
        {
            if (_poolHandler != null)
                _poolHandler.Init();
            this.TimeStamp = TimeSource.Current.Time;
            this.SequenceID = Interlocked.Increment(ref globalSequenceId);
            this.Level = level;
            this.LoggerName = loggerName;
            this.Message = message;
            this.Parameters = parameters;
            this.FormatProvider = formatProvider;
            this.Exception = exception;
            CalcFormattedMessageIfNeeded();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LogEventInfo" /> class.
        /// </summary>
        /// <param name="level">Log level.</param>
        /// <param name="loggerName">Logger name.</param>
        /// <param name="message">Log message including parameter placeholders.</param>
        public LogEventInfo(LogLevel level, string loggerName, [Localizable(false)] string message)
            : this(level, loggerName, null, message, null, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LogEventInfo" /> class.
        /// </summary>
        /// <param name="level">Log level.</param>
        /// <param name="loggerName">Logger name.</param>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">Log message including parameter placeholders.</param>
        /// <param name="parameters">Parameter array.</param>
        public LogEventInfo(LogLevel level, string loggerName, IFormatProvider formatProvider, [Localizable(false)] string message, object[] parameters) 
            : this(level, loggerName, formatProvider, message, parameters, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LogEventInfo" /> class.
        /// </summary>
        /// <param name="level">Log level.</param>
        /// <param name="loggerName">Logger name.</param>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">Log message including parameter placeholders.</param>
        /// <param name="parameters">Parameter array.</param>
        /// <param name="exception">Exception information.</param>
        public LogEventInfo(LogLevel level, string loggerName, IFormatProvider formatProvider, [Localizable(false)] string message, object[] parameters, Exception exception)
        {
            Init(level, loggerName, formatProvider, message, parameters, exception);
        }

        /// <summary>
        /// Gets the unique identifier of log event which is automatically generated
        /// and monotonously increasing.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "ID", Justification = "Backwards compatibility")]
        public int SequenceID { get; private set; }

        /// <summary>
        /// Gets or sets the timestamp of the logging event.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "TimeStamp", Justification = "Backwards compatibility.")]
        public DateTime TimeStamp { get; set; }

        /// <summary>
        /// Gets or sets the level of the logging event.
        /// </summary>
        public LogLevel Level { get; set; }

        /// <summary>
        /// Gets a value indicating whether stack trace has been set for this event.
        /// </summary>
        public bool HasStackTrace
        {
            get { return this.StackTrace != null; }
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
        public string Message
        {
            get { return message; }
            set
            {
                message = value; 
                ResetFormattedMessage();
            }
        }

        /// <summary>
        /// Gets or sets the parameter values or null if no parameters have been specified.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "For backwards compatibility.")]
        public object[] Parameters
        {
            get { return parameters; }
            set
            {
                parameters = value;
                ResetFormattedMessage();
            }
        }

        /// <summary>
        /// Gets or sets the format provider that was provided while logging or <see langword="null" />
        /// when no formatProvider was specified.
        /// </summary>
        public IFormatProvider FormatProvider
        {
            get { return formatProvider; }
            set
            {
                if (formatProvider != value)
                {
                    formatProvider = value;
                    ResetFormattedMessage();
                }
            }
        }

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
        public IDictionary<object, object> Properties
        {
            get
            {
                if (this.properties == null)
                {
                    this.InitEventContext();
                }

                return this.properties;
            }
        }

        /// <summary>
        /// Gets the dictionary of per-event context properties.
        /// </summary>
        [Obsolete("Use LogEventInfo.Properties instead.", true)]
        public IDictionary Context
        {
            get
            {
                if (this.eventContextAdapter == null)
                {
                    this.InitEventContext();
                }

                return this.eventContextAdapter;
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
        public static LogEventInfo Create(LogLevel logLevel, string loggerName, [Localizable(false)] string message)
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
        public static LogEventInfo Create(LogLevel logLevel, string loggerName, IFormatProvider formatProvider, [Localizable(false)] string message, object[] parameters)
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
        [Obsolete("use Create(LogLevel logLevel, string loggerName, Exception exception, IFormatProvider formatProvider, string message)")]
        public static LogEventInfo Create(LogLevel logLevel, string loggerName, [Localizable(false)] string message, Exception exception)
        {
            return new LogEventInfo(logLevel, loggerName, null, message, null, exception);
        }

        /// <summary>
        /// Creates the log event.
        /// </summary>
        /// <param name="logLevel">The log level.</param>
        /// <param name="loggerName">Name of the logger.</param>
        /// <param name="exception">The exception.</param>
        /// <param name="formatProvider">The format provider.</param>
        /// <param name="message">The message.</param>
        /// <returns>Instance of <see cref="LogEventInfo"/>.</returns>
        public static LogEventInfo Create(LogLevel logLevel, string loggerName, Exception exception, IFormatProvider formatProvider, [Localizable(false)] string message)
        {
            return Create(logLevel, loggerName, exception, formatProvider, message, null);
        }

        /// <summary>
        /// Creates the log event.
        /// </summary>
        /// <param name="logLevel">The log level.</param>
        /// <param name="loggerName">Name of the logger.</param>
        /// <param name="exception">The exception.</param>
        /// <param name="formatProvider">The format provider.</param>
        /// <param name="message">The message.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>Instance of <see cref="LogEventInfo"/>.</returns>
        public static LogEventInfo Create(LogLevel logLevel, string loggerName, Exception exception, IFormatProvider formatProvider, [Localizable(false)] string message, object[] parameters)
        {
            return new LogEventInfo(logLevel, loggerName,formatProvider, message, parameters, exception);
        }

        /// <summary>
        /// Creates the initial <see cref="AsyncLogEventInfo"/> from this <see cref="LogEventInfo"/> by attaching the specified asynchronous continuation.
        /// </summary>
        /// <param name="asyncContinuation"></param>
        /// <returns></returns>
        public AsyncLogEventInfo StartContinuation(AsyncContinuation asyncContinuation)
        {
            if (_poolHandler != null)
            {
                // Ensure that the LogEvent is only released once
                var singleCallContinuation = new SingleCallContinuation(_poolHandler.PoolReleaseContinuation.CreateContinuation(asyncContinuation, _poolHandler.PoolReleaseDelegate));
                singleCallContinuation.AllowExceptions = true;  // Allows synchronous targets to throw exceptions back to Logger. Async-Targets will automatically change it to false.
                asyncContinuation = singleCallContinuation.Function;
            }
            return new AsyncLogEventInfo(this, asyncContinuation);
        }

        /// <summary>
        /// Creates <see cref="AsyncLogEventInfo"/> from this <see cref="LogEventInfo"/> by attaching the specified asynchronous continuation.
        /// </summary>
        /// <param name="asyncContinuation">The asynchronous continuation.</param>
        /// <returns>Instance of <see cref="AsyncLogEventInfo"/> with attached continuation.</returns>
        public AsyncLogEventInfo WithContinuation(AsyncContinuation asyncContinuation)
        {
            return new AsyncLogEventInfo(this, asyncContinuation);
        }

        /// <summary>
        /// Returns a string representation of this log event.
        /// </summary>
        /// <returns>String representation of the log event.</returns>
        public override string ToString()
        {
            return "Log Event: Logger='" + this.LoggerName + "' Level=" + this.Level + " Message='" + this.FormattedMessage + "' SequenceID=" + this.SequenceID;
        }

        /// <summary>
        /// Sets the stack trace for the event info.
        /// </summary>
        /// <param name="stackTrace">The stack trace.</param>
        /// <param name="userStackFrame">Index of the first user stack frame within the stack trace.</param>
        public void SetStackTrace(StackTrace stackTrace, int userStackFrame)
        {
            this.StackTrace = stackTrace;
            this.UserStackFrameNumber = userStackFrame;
        }

        internal string AddCachedLayoutValue(Layout layout, string value)
        {
            lock (this.layoutCacheLock)
            {
                if (this.layoutCache == null)
                {
                    this.layoutCache = new Dictionary<Layout, string>();
                }

                this.layoutCache[layout] = value;
            }

            return value;
        }

        internal bool TryGetCachedLayoutValue(Layout layout, out string value)
        {
            lock (this.layoutCacheLock)
            {
                if (this.layoutCache == null)
                {
                    value = null;
                    return false;
                }

                return this.layoutCache.TryGetValue(layout, out value);
            }
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

        internal void CalcFormattedMessageIfNeeded()
        {
            if (NeedToPreformatMessage(this.Parameters))
            {
                this.CalcFormattedMessage();
            }
        }

        private void CalcFormattedMessage()
        {
            if (this.Parameters == null || this.Parameters.Length == 0)
            {
                this.formattedMessage = this.Message;
            }
            else
            {
                try
                {
                    this.formattedMessage = string.Format(this.FormatProvider ?? CultureInfo.CurrentCulture, this.Message, this.Parameters);
                }
                catch (Exception exception)
                {
                    this.formattedMessage = this.Message;
                    InternalLogger.Warn(exception, "Error when formatting a message.");

                    if (exception.MustBeRethrown())
                    {
                        throw;
                    }
                }
            }
        }

        private void ResetFormattedMessage()
        {
            this.formattedMessage = null;
        }

        private void InitEventContext()
        {
            this.properties = new Dictionary<object, object>();
            this.eventContextAdapter = new DictionaryAdapter<object, object>(this.properties);
        }

        class PoolHandler
        {
            internal Internal.PoolFactory.ILogEventObjectFactory Owner;
            internal readonly CompleteWhenAllContinuation PoolReleaseContinuation;
            internal readonly CompleteWhenAllContinuation.Counter PoolReleaseCounter;
            internal readonly AsyncContinuation PoolReleaseDelegate;

            public PoolHandler(LogEventInfo logEvent, Internal.PoolFactory.LogEventPoolFactory owner)
            {
                this.Owner = owner;
                this.PoolReleaseDelegate = (ex) => this.Owner.ReleaseLogEvent(logEvent);
                this.PoolReleaseCounter = new CompleteWhenAllContinuation.Counter();
                this.PoolReleaseContinuation = new CompleteWhenAllContinuation(this.PoolReleaseCounter);
            }

            public void Init()
            {
                this.PoolReleaseContinuation.Init(PoolReleaseCounter);
            }

            public void Clear()
            {
                this.PoolReleaseContinuation.Clear();
                this.PoolReleaseCounter.Clear();
            }
        }

        object Internal.PoolFactory.IPoolObject.Owner { get { return _poolHandler.Owner; } set { _poolHandler.Owner = (Internal.PoolFactory.ILogEventObjectFactory)value; } }
        internal Internal.PoolFactory.ILogEventObjectFactory ObjectFactory { get { return _poolHandler != null ? _poolHandler.Owner : Internal.PoolFactory.LogEventObjectFactory.Instance; } }
        internal CompleteWhenAllContinuation PoolReleaseContinuation { get { return _poolHandler != null ? _poolHandler.PoolReleaseContinuation : null; } }

        /// <summary>
        /// Clears the log event info for reuse purposes
        /// </summary>
        void Internal.PoolFactory.IPoolObject.Clear()
        {
            if (this.properties != null)
            {
                // just reset, so we dont have to allocate another dictionary
                this.properties.Clear();
            }
            else
            {
                this.eventContextAdapter = null;
            }
            this.parameters = null;
            this.formatProvider = null;
            if (this.layoutCache != null)
            {
                // just reset, so we dont have to allocate another dictionary
                this.layoutCache.Clear();
            }

            this.Exception = null;
            this.formattedMessage = null;
            this.Level = null;
            this.LoggerName = null;
            this.message = null;
            this.StackTrace = null;
            this.TimeStamp = default(DateTime);
            this.UserStackFrameNumber = 0;
            if (_poolHandler != null)
                _poolHandler.Clear();
            this.SequenceID = 0;
        }

#if DEBUG
#if !SILVERLIGHT
        /// <summary>
        /// Allows an object to try to free resources and perform other cleanup operations before it is reclaimed by garbage collection.
        /// </summary>
        ~LogEventInfo()
        {
            if (!AppDomain.CurrentDomain.IsFinalizingForUnload())
            {
                if (InternalLogger.IsTraceEnabled)
                {
                    if (this._poolHandler != null)
                    {
                        InternalLogger.Trace(string.Format("Pooled LogEventInfo with SequenceID:{0} was collected by garbage collector even if not shutting down", this.SequenceID));
                    }
                }
            }
        }
#endif
#endif
    }
}
