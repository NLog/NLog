// 
// Copyright (c) 2004-2020 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace NLog.Fluent
{
    /// <summary>
    /// A fluent class to build log events for NLog.
    /// </summary>
    public class LogBuilder
    {
        private readonly LogEventInfo _logEvent;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="LogBuilder"/> class.
        /// </summary>
        /// <param name="logger">The <see cref="Logger"/> to send the log event.</param>
        [CLSCompliant(false)]
        public LogBuilder(ILogger logger)
            : this(logger, LogLevel.Debug)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LogBuilder"/> class.
        /// </summary>
        /// <param name="logger">The <see cref="Logger"/> to send the log event.</param>
        /// <param name="logLevel">The <see cref="LogLevel"/> for the log event.</param>
        [CLSCompliant(false)]
        public LogBuilder(ILogger logger, LogLevel logLevel)
        {
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));
            if (logLevel == null)
                throw new ArgumentNullException(nameof(logLevel));

            _logger = logger;
            _logEvent = new LogEventInfo() { LoggerName = logger.Name, Level = logLevel };
        }

        /// <summary>
        /// Gets the <see cref="LogEventInfo"/> created by the builder.
        /// </summary>
        public LogEventInfo LogEventInfo => _logEvent;

        /// <summary>
        /// Sets the <paramref name="exception"/> information of the logging event.
        /// </summary>
        /// <param name="exception">The exception information of the logging event.</param>
        /// <returns>current <see cref="LogBuilder"/> for chaining calls.</returns>
        public LogBuilder Exception(Exception exception)
        {
            _logEvent.Exception = exception;
            return this;
        }

        /// <summary>
        /// Sets the level of the logging event.
        /// </summary>
        /// <param name="logLevel">The level of the logging event.</param>
        /// <returns>current <see cref="LogBuilder"/> for chaining calls.</returns>
        public LogBuilder Level(LogLevel logLevel)
        {
            if (logLevel == null)
                throw new ArgumentNullException(nameof(logLevel));

            _logEvent.Level = logLevel;
            return this;
        }

        /// <summary>
        /// Sets the logger name of the logging event.
        /// </summary>
        /// <param name="loggerName">The logger name of the logging event.</param>
            /// <returns>current <see cref="LogBuilder"/> for chaining calls.</returns>
        public LogBuilder LoggerName(string loggerName)
        {
            _logEvent.LoggerName = loggerName;
            return this;
        }

        /// <summary>
        /// Sets the log message on the logging event.
        /// </summary>
        /// <param name="message">The log message for the logging event.</param>
            /// <returns>current <see cref="LogBuilder"/> for chaining calls.</returns>
        public LogBuilder Message(string message)
        {
            _logEvent.Message = message;

            return this;
        }

        /// <summary>
        /// Sets the log message and parameters for formatting on the logging event.
        /// </summary>
        /// <param name="format">A composite format string.</param>
        /// <param name="arg0">The object to format.</param>
        /// <returns>current <see cref="LogBuilder"/> for chaining calls.</returns>
        [MessageTemplateFormatMethod("format")]
        public LogBuilder Message(string format, object arg0)
        {
            _logEvent.Message = format;
            _logEvent.Parameters = new[] { arg0 };

            return this;
        }

        /// <summary>
        /// Sets the log message and parameters for formatting on the logging event.
        /// </summary>
        /// <param name="format">A composite format string.</param>
        /// <param name="arg0">The first object to format.</param>
        /// <param name="arg1">The second object to format.</param>
        /// <returns>current <see cref="LogBuilder"/> for chaining calls.</returns>
        [MessageTemplateFormatMethod("format")]
        public LogBuilder Message(string format, object arg0, object arg1)
        {
            _logEvent.Message = format;
            _logEvent.Parameters = new[] { arg0, arg1 };

            return this;
        }

        /// <summary>
        /// Sets the log message and parameters for formatting on the logging event.
        /// </summary>
        /// <param name="format">A composite format string.</param>
        /// <param name="arg0">The first object to format.</param>
        /// <param name="arg1">The second object to format.</param>
        /// <param name="arg2">The third object to format.</param>
        /// <returns>current <see cref="LogBuilder"/> for chaining calls.</returns>
        [MessageTemplateFormatMethod("format")]
        public LogBuilder Message(string format, object arg0, object arg1, object arg2)
        {
            _logEvent.Message = format;
            _logEvent.Parameters = new[] { arg0, arg1, arg2 };

            return this;
        }

        /// <summary>
        /// Sets the log message and parameters for formatting on the logging event.
        /// </summary>
        /// <param name="format">A composite format string.</param>
        /// <param name="arg0">The first object to format.</param>
        /// <param name="arg1">The second object to format.</param>
        /// <param name="arg2">The third object to format.</param>
        /// <param name="arg3">The fourth object to format.</param>
        /// <returns>current <see cref="LogBuilder"/> for chaining calls.</returns>
        [MessageTemplateFormatMethod("format")]
        public LogBuilder Message(string format, object arg0, object arg1, object arg2, object arg3)
        {
            _logEvent.Message = format;
            _logEvent.Parameters = new[] { arg0, arg1, arg2, arg3 };

            return this;
        }

        /// <summary>
        /// Sets the log message and parameters for formatting on the logging event.
        /// </summary>
        /// <param name="format">A composite format string.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <returns>current <see cref="LogBuilder"/> for chaining calls.</returns>
        [MessageTemplateFormatMethod("format")]
        public LogBuilder Message(string format, params object[] args)
        {
            _logEvent.Message = format;
            _logEvent.Parameters = args;

            return this;
        }

        /// <summary>
        /// Sets the log message and parameters for formatting on the logging event.
        /// </summary>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="format">A composite format string.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <returns>current <see cref="LogBuilder"/> for chaining calls.</returns>
        [MessageTemplateFormatMethod("format")]
        public LogBuilder Message(IFormatProvider provider, string format, params object[] args)
        {
            _logEvent.FormatProvider = provider;
            _logEvent.Message = format;
            _logEvent.Parameters = args;

            return this;
        }

        /// <summary>
        /// Sets a per-event context property on the logging event.
        /// </summary>
        /// <param name="name">The name of the context property.</param>
        /// <param name="value">The value of the context property.</param>
        /// <returns>current <see cref="LogBuilder"/> for chaining calls.</returns>
        public LogBuilder Property(object name, object value)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            _logEvent.Properties[name] = value;
            return this;
        }

        /// <summary>
        /// Sets multiple per-event context properties on the logging event.
        /// </summary>
        /// <param name="properties">The properties to set.</param>
        /// <returns>current <see cref="LogBuilder"/> for chaining calls.</returns>
        public LogBuilder Properties(IDictionary properties)
        {
            if (properties == null)
                throw new ArgumentNullException(nameof(properties));

            foreach (var key in properties.Keys)
            {
                _logEvent.Properties[key] = properties[key];
            }
            return this;
        }

        /// <summary>
        /// Sets the timestamp of the logging event.
        /// </summary>
        /// <param name="timeStamp">The timestamp of the logging event.</param>
        /// <returns>current <see cref="LogBuilder"/> for chaining calls.</returns>
        public LogBuilder TimeStamp(DateTime timeStamp)
        {
            _logEvent.TimeStamp = timeStamp;
            return this;
        }

        /// <summary>
        /// Sets the stack trace for the event info.
        /// </summary>
        /// <param name="stackTrace">The stack trace.</param>
        /// <param name="userStackFrame">Index of the first user stack frame within the stack trace.</param>
        /// <returns>current <see cref="LogBuilder"/> for chaining calls.</returns>
        public LogBuilder StackTrace(StackTrace stackTrace, int userStackFrame)
        {
            _logEvent.SetStackTrace(stackTrace, userStackFrame);
            return this;
        }

        /// <summary>
        /// Writes the log event to the underlying logger.
        /// </summary>
        /// <param name="callerMemberName">The method or property name of the caller to the method. This is set at by the compiler.</param>
        /// <param name="callerFilePath">The full path of the source file that contains the caller. This is set at by the compiler.</param>
        /// <param name="callerLineNumber">The line number in the source file at which the method is called. This is set at by the compiler.</param>
#if !NET35 && !NET40
        public void Write(
            [CallerMemberName]string callerMemberName = null,
            [CallerFilePath]string callerFilePath = null,
            [CallerLineNumber]int callerLineNumber = 0)
        {
            if (!_logger.IsEnabled(_logEvent.Level))
                return;

            SetCallerInfo(callerMemberName, callerFilePath, callerLineNumber);

            _logger.Log(_logEvent);
        }
#else
        public void Write(
            string callerMemberName = null,
            string callerFilePath = null,
            int callerLineNumber = 0)
        {
            _logger.Log(_logEvent);
        }
#endif

        /// <summary>
        /// Writes the log event to the underlying logger if the condition delegate is true.
        /// </summary>
        /// <param name="condition">If condition is true, write log event; otherwise ignore event.</param>
        /// <param name="callerMemberName">The method or property name of the caller to the method. This is set at by the compiler.</param>
        /// <param name="callerFilePath">The full path of the source file that contains the caller. This is set at by the compiler.</param>
        /// <param name="callerLineNumber">The line number in the source file at which the method is called. This is set at by the compiler.</param>
#if !NET35 && !NET40
        public void WriteIf(
            Func<bool> condition,
            [CallerMemberName]string callerMemberName = null,
            [CallerFilePath]string callerFilePath = null,
            [CallerLineNumber]int callerLineNumber = 0)
        {
            if (condition == null || !condition() || !_logger.IsEnabled(_logEvent.Level))
                return;

            SetCallerInfo(callerMemberName, callerFilePath, callerLineNumber);

            _logger.Log(_logEvent);
        }
#else
        public void WriteIf(
            Func<bool> condition,
            string callerMemberName = null,
            string callerFilePath = null,
            int callerLineNumber = 0)
        {
            if (condition == null || !condition() || !_logger.IsEnabled(_logEvent.Level))
                return;

            _logger.Log(_logEvent);
        }
#endif

        /// <summary>
        /// Writes the log event to the underlying logger if the condition is true.
        /// </summary>
        /// <param name="condition">If condition is true, write log event; otherwise ignore event.</param>
        /// <param name="callerMemberName">The method or property name of the caller to the method. This is set at by the compiler.</param>
        /// <param name="callerFilePath">The full path of the source file that contains the caller. This is set at by the compiler.</param>
        /// <param name="callerLineNumber">The line number in the source file at which the method is called. This is set at by the compiler.</param>
#if !NET35 && !NET40
        public void WriteIf(
            bool condition,
            [CallerMemberName]string callerMemberName = null,
            [CallerFilePath]string callerFilePath = null,
            [CallerLineNumber]int callerLineNumber = 0)
        {
            if (!condition || !_logger.IsEnabled(_logEvent.Level))
                return;

            SetCallerInfo(callerMemberName, callerFilePath, callerLineNumber);

            _logger.Log(_logEvent);
        }
#else
        public void WriteIf(
            bool condition,
            string callerMemberName = null,
            string callerFilePath = null,
            int callerLineNumber = 0)
        {
            if (!condition || !_logger.IsEnabled(_logEvent.Level))
                return;

            _logger.Log(_logEvent);
        }
#endif

        private void SetCallerInfo(string callerMethodName, string callerFilePath, int callerLineNumber)
        {
            if (callerMethodName != null || callerFilePath != null || callerLineNumber != 0)
                _logEvent.SetCallerInfo(null, callerMethodName, callerFilePath, callerLineNumber);
        }
    }
}