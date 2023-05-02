// 
// Copyright (c) 2004-2021 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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
using System.ComponentModel;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using NLog.Internal;

namespace NLog
{
    /// <summary>
    /// A fluent builder for logging events to NLog.
    /// </summary>
    [CLSCompliant(false)]
    public struct LogEventBuilder
    {
        private readonly ILogger _logger;
        private readonly LogEventInfo _logEvent;

        /// <summary>
        /// Initializes a new instance of the <see cref="LogEventBuilder"/> class.
        /// </summary>
        /// <param name="logger">The <see cref="NLog.Logger"/> to send the log event.</param>
        public LogEventBuilder([NotNull] ILogger logger)
        {
            _logger = Guard.ThrowIfNull(logger);
            _logEvent = new LogEventInfo() { LoggerName = _logger.Name };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LogEventBuilder"/> class.
        /// </summary>
        /// <param name="logger">The <see cref="NLog.Logger"/> to send the log event.</param>
        /// <param name="logLevel">The log level. LogEvent is only created when <see cref="LogLevel"/> is enabled for <paramref name="logger"/></param>
        public LogEventBuilder([NotNull] ILogger logger, [NotNull] LogLevel logLevel)
        {
            _logger = Guard.ThrowIfNull(logger);

            Guard.ThrowIfNull(logLevel);

            if (logger.IsEnabled(logLevel))
            {
                _logEvent = new LogEventInfo() { LoggerName = _logger.Name, Level = logLevel };
            }
            else
            {
                _logEvent = null;
            }
        }

        /// <summary>
        /// The logger to write the log event to
        /// </summary>
        [NotNull]
        public ILogger Logger => _logger;

        /// <summary>
        /// Logging event that will be written
        /// </summary>
        [CanBeNull]
        public LogEventInfo LogEvent => _logEvent is null ? null : ResolveLogEvent(_logEvent);

        /// <summary>
        /// Sets a per-event context property on the logging event.
        /// </summary>
        /// <param name="propertyName">The name of the context property.</param>
        /// <param name="propertyValue">The value of the context property.</param>
        public LogEventBuilder Property<T>([NotNull] string propertyName, T propertyValue)
        {
            Guard.ThrowIfNull(propertyName);

            if (_logEvent is null)
                return this;

            _logEvent.Properties[propertyName] = propertyValue;
            return this;
        }

        /// <summary>
        /// Sets multiple per-event context properties on the logging event.
        /// </summary>
        /// <param name="properties">The properties to set.</param>
        public LogEventBuilder Properties([NotNull] IEnumerable<KeyValuePair<string, object>> properties)
        {
            Guard.ThrowIfNull(properties);

            if (_logEvent is null)
                return this;

            foreach (var property in properties)
                _logEvent.Properties[property.Key] = property.Value;
            return this;
        }

        /// <summary>
        /// Sets the <paramref name="exception"/> information of the logging event.
        /// </summary>
        /// <param name="exception">The exception information of the logging event.</param>
        public LogEventBuilder Exception(Exception exception)
        {
            if (_logEvent != null)
            {
                _logEvent.Exception = exception;
            }
            return this;
        }

        /// <summary>
        /// Sets the timestamp of the logging event.
        /// </summary>
        /// <param name="timeStamp">The timestamp of the logging event.</param>
        public LogEventBuilder TimeStamp(DateTime timeStamp)
        {
            if (_logEvent != null)
            {
                _logEvent.TimeStamp = timeStamp;
            }
            return this;
        }

        /// <summary>
        /// Sets the log message on the logging event.
        /// </summary>
        /// <param name="message">A <see langword="string" /> to be written.</param>
        public LogEventBuilder Message([Localizable(false)] string message)
        {
            if (_logEvent != null)
            {
                _logEvent.Parameters = null;
                _logEvent.Message = message;
            }
            return this;
        }

        /// <summary>
        /// Sets the log message and parameters for formatting for the logging event.
        /// </summary>
        /// <typeparam name="TArgument">The type of the argument.</typeparam>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The argument to format.</param>
        [MessageTemplateFormatMethod("message")]
        public LogEventBuilder Message<TArgument>([Localizable(false)][StructuredMessageTemplate] string message, TArgument argument)
        {
            if (_logEvent != null)
            {
                _logEvent.Message = message;
                _logEvent.Parameters = new object[] { argument };
            }
            return this;
        }

        /// <summary>
        /// Sets the log message and parameters for formatting on the logging event.
        /// </summary>
        /// <typeparam name="TArgument1">The type of the first argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the second argument.</typeparam>
        /// <param name="message">A <see langword="string" /> containing format items.</param>
        /// <param name="argument1">The first argument to format.</param>
        /// <param name="argument2">The second argument to format.</param>
        [MessageTemplateFormatMethod("message")]
        public LogEventBuilder Message<TArgument1, TArgument2>([Localizable(false)][StructuredMessageTemplate] string message, TArgument1 argument1, TArgument2 argument2)
        {
            if (_logEvent != null)
            {
                _logEvent.Message = message;
                _logEvent.Parameters = new object[] { argument1, argument2 };
            }
            return this;
        }

        /// <summary>
        /// Sets the log message and parameters for formatting on the logging event.
        /// </summary>
        /// <typeparam name="TArgument1">The type of the first argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the second argument.</typeparam>
        /// <typeparam name="TArgument3">The type of the third argument.</typeparam>
        /// <param name="message">A <see langword="string" /> containing format items.</param>
        /// <param name="argument1">The first argument to format.</param>
        /// <param name="argument2">The second argument to format.</param>
        /// <param name="argument3">The third argument to format.</param>
        [MessageTemplateFormatMethod("message")]
        public LogEventBuilder Message<TArgument1, TArgument2, TArgument3>([Localizable(false)][StructuredMessageTemplate] string message, TArgument1 argument1, TArgument2 argument2, TArgument3 argument3)
        {
            if (_logEvent != null)
            {
                _logEvent.Message = message;
                _logEvent.Parameters = new object[] { argument1, argument2, argument3 };
            }
            return this;
        }

        /// <summary>
        /// Sets the log message and parameters for formatting on the logging event.
        /// </summary>
        /// <param name="message">A <see langword="string" /> containing format items.</param>
        /// <param name="args">Arguments to format.</param>
        [MessageTemplateFormatMethod("message")]
        public LogEventBuilder Message([Localizable(false)][StructuredMessageTemplate] string message, params object[] args)
        {
            if (_logEvent != null)
            {
                _logEvent.Message = message;
                _logEvent.Parameters = args;
            }
            return this;
        }

        /// <summary>
        /// Sets the log message and parameters for formatting on the logging event.
        /// </summary>
        /// <param name="formatProvider">An object that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing format items.</param>
        /// <param name="args">Arguments to format.</param>
        [MessageTemplateFormatMethod("message")]
        public LogEventBuilder Message(IFormatProvider formatProvider, [Localizable(false)][StructuredMessageTemplate] string message, params object[] args)
        {
            if (_logEvent != null)
            {
                _logEvent.FormatProvider = formatProvider;
                _logEvent.Message = message;
                _logEvent.Parameters = args;
            }
            return this;
        }

        /// <summary>
        /// Writes the log event to the underlying logger.
        /// </summary>
        /// <param name="callerClassName">The class of the caller to the method. This is captured by the NLog engine when necessary</param>
        /// <param name="callerMemberName">The method or property name of the caller to the method. This is set at by the compiler.</param>
        /// <param name="callerFilePath">The full path of the source file that contains the caller. This is set at by the compiler.</param>
        /// <param name="callerLineNumber">The line number in the source file at which the method is called. This is set at by the compiler.</param>
#if !NET35
        public LogEventBuilder Callsite(string callerClassName = null,
                                       [CallerMemberName]string callerMemberName = null,
                                       [CallerFilePath]string callerFilePath = null,
                                       [CallerLineNumber]int callerLineNumber = 0)
#else
        public LogEventBuilder Callsite(string callerClassName = null,
                                        string callerMemberName = null,
                                        string callerFilePath = null,
                                        int callerLineNumber = 0)
#endif
        {
            if (_logEvent != null)
            {
                _logEvent.SetCallerInfo(callerClassName, callerMemberName, callerFilePath, callerLineNumber);
            }
            return this;
        }

        /// <summary>
        /// Writes the log event to the underlying logger.
        /// </summary>
        /// <param name="logLevel">The log level. Optional but when assigned to <see cref="LogLevel.Off"/> then it will discard the LogEvent.</param>
        /// <param name="callerMemberName">The method or property name of the caller to the method. This is set at by the compiler.</param>
        /// <param name="callerFilePath">The full path of the source file that contains the caller. This is set at by the compiler.</param>
        /// <param name="callerLineNumber">The line number in the source file at which the method is called. This is set at by the compiler.</param>
#if !NET35
        public void Log(LogLevel logLevel = null,
                        [CallerMemberName]string callerMemberName = null,
                        [CallerFilePath]string callerFilePath = null,
                        [CallerLineNumber]int callerLineNumber = 0)
#else
        public void Log(LogLevel logLevel = null,
                        string callerMemberName = null,
                        string callerFilePath = null,
                        int callerLineNumber = 0)
#endif
        {
            if (_logEvent != null)
            {
                var logEvent = ResolveLogEvent(_logEvent, logLevel);
                if (logEvent.CallSiteInformation is null && _logger.IsEnabled(logEvent.Level))
                {
                    _logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                _logger.Log(logEvent);
            }
        }

        /// <summary>
        /// Writes the log event to the underlying logger.
        /// </summary>
        /// <param name="wrapperType">Type of custom Logger wrapper.</param>
#if !NET35
        public void Log(Type wrapperType)
#else
        public void Log(Type wrapperType)
#endif
        {
            if (_logEvent != null)
            {
                var logEvent = ResolveLogEvent(_logEvent);
                _logger.Log(wrapperType, logEvent);
            }
        }

        private LogEventInfo ResolveLogEvent(LogEventInfo logEvent, LogLevel logLevel = null)
        {
            if (logLevel is null)
            {
                if (logEvent.Level is null)
                    logEvent.Level = logEvent.Exception != null ? LogLevel.Error : LogLevel.Info;
            }
            else
            {
                logEvent.Level = logLevel;
            }
            
            if (logEvent.Message is null && logEvent.Exception != null && _logger.IsEnabled(logEvent.Level))
            {
                logEvent.FormatProvider = NLog.Internal.ExceptionMessageFormatProvider.Instance;
                logEvent.Message = "{0}";
                logEvent.Parameters = new object[] { logEvent.Exception };
            }

            return logEvent;
        }
    }
}