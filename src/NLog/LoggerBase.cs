// 
// Copyright (c) 2004-2018 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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
    using NLog.Config;
    using NLog.Internal;

    /// <summary>
    /// Logger interface with minimal interface (still some bonus properties for performance)
    /// </summary>
    public abstract class LoggerBase
    {
        private readonly Type _loggerType;
        private LoggerConfiguration _configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="LoggerBase"/> class.
        /// </summary>
        protected LoggerBase()
        {
            _loggerType = GetType();
        }

        /// <summary>
        /// Occurs when logger configuration changes.
        /// </summary>
        public event EventHandler<EventArgs> LoggerReconfigured;

        /// <summary>
        /// Gets the name of the logger.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the factory that created this logger.
        /// </summary>
        public LogFactory Factory { get; private set; }

        /// <summary>
        /// Gets the active LoggerConfiguration for the logger
        /// </summary>
        internal LoggerConfiguration LoggerConfiguration { get => _configuration; }

        /// <summary>
        /// Gets a value indicating whether logging is enabled for the <c>Trace</c> level.
        /// </summary>
        /// <returns>A value of <see langword="true" /> if logging is enabled for the <c>Trace</c> level, otherwise it returns <see langword="false" />.</returns>
        public bool IsTraceEnabled { get; private set; }

        /// <summary>
        /// Gets a value indicating whether logging is enabled for the <c>Debug</c> level.
        /// </summary>
        /// <returns>A value of <see langword="true" /> if logging is enabled for the <c>Debug</c> level, otherwise it returns <see langword="false" />.</returns>
        public bool IsDebugEnabled { get; private set; }

        /// <summary>
        /// Gets a value indicating whether logging is enabled for the <c>Info</c> level.
        /// </summary>
        /// <returns>A value of <see langword="true" /> if logging is enabled for the <c>Info</c> level, otherwise it returns <see langword="false" />.</returns>
        public bool IsInfoEnabled { get; private set; }

        /// <summary>
        /// Gets a value indicating whether logging is enabled for the <c>Warn</c> level.
        /// </summary>
        /// <returns>A value of <see langword="true" /> if logging is enabled for the <c>Warn</c> level, otherwise it returns <see langword="false" />.</returns>
        public bool IsWarnEnabled { get; private set; }

        /// <summary>
        /// Gets a value indicating whether logging is enabled for the <c>Error</c> level.
        /// </summary>
        /// <returns>A value of <see langword="true" /> if logging is enabled for the <c>Error</c> level, otherwise it returns <see langword="false" />.</returns>
        public bool IsErrorEnabled { get; private set; }

        /// <summary>
        /// Gets a value indicating whether logging is enabled for the <c>Fatal</c> level.
        /// </summary>
        /// <returns>A value of <see langword="true" /> if logging is enabled for the <c>Fatal</c> level, otherwise it returns <see langword="false" />.</returns>
        public bool IsFatalEnabled { get; private set; }

        /// <summary>
        /// Gets a value indicating whether logging is enabled for the specified level.
        /// </summary>
        /// <param name="level">Log level to be checked.</param>
        /// <returns>A value of <see langword="true" /> if logging is enabled for the specified level, otherwise it returns <see langword="false" />.</returns>
        public bool IsEnabled(LogLevel level)
        {
            if (level == null)
            {
                throw new InvalidOperationException("Log level must be defined");
            }

            return GetTargetsForLevel(level) != null;
        }

        /// <summary>
        /// Writes the specified log event.
        /// </summary>
        /// <param name="logEvent">Log event.</param>
        public void Log(LogEventInfo logEvent)
        {
            if (IsEnabled(logEvent.Level))
            {
                WriteToTargets(_loggerType, logEvent);
            }
        }

        /// <summary>
        /// Writes the specified log event.
        /// </summary>
        /// <param name="wrapperType">The name of the type that wraps Logger.</param>
        /// <param name="logEvent">Log event.</param>
        public void Log(Type wrapperType, LogEventInfo logEvent)
        {
            if (IsEnabled(logEvent.Level))
            {
                WriteToTargets(wrapperType, logEvent);
            }
        }

        /// <summary>
        /// Checks the targets whether StackTrace is required
        /// </summary>
        /// <param name="level">Log level to be checked.</param>
        /// <returns>Value indicating how stack trace should be captured when processing the log event</returns>
        internal StackTraceUsage GetStackTraceUsage(LogLevel level)
        {
            return GetTargetsForLevel(level).GetStackTraceUsage();
        }

        /// <summary>
        /// Writes the specified log event.
        /// </summary>
        /// <param name="wrapperType">The name of the type that wraps Logger.</param>
        /// <param name="logEvent">Log event.</param>
        protected void WriteToTargets(Type wrapperType, LogEventInfo logEvent)
        {
            LoggerImpl.Write(wrapperType ?? _loggerType, GetTargetsForLevel(logEvent.Level), PrepareLogEventInfo(logEvent), Factory);
        }

        private LogEventInfo PrepareLogEventInfo(LogEventInfo logEvent)
        {
            if (logEvent.FormatProvider == null)
            {
                logEvent.FormatProvider = Factory.DefaultCultureInfo;
            }
            return logEvent;
        }

        internal void Initialize(string name, LoggerConfiguration loggerConfiguration, LogFactory factory)
        {
            Name = name;
            Factory = factory;
            SetConfiguration(loggerConfiguration);
        }

        internal virtual void SetConfiguration(LoggerConfiguration newConfiguration)
        {
            _configuration = newConfiguration;

            // pre-calculate 'enabled' flags
            IsTraceEnabled = newConfiguration.IsEnabled(LogLevel.Trace);
            IsDebugEnabled = newConfiguration.IsEnabled(LogLevel.Debug);
            IsInfoEnabled = newConfiguration.IsEnabled(LogLevel.Info);
            IsWarnEnabled = newConfiguration.IsEnabled(LogLevel.Warn);
            IsErrorEnabled = newConfiguration.IsEnabled(LogLevel.Error);
            IsFatalEnabled = newConfiguration.IsEnabled(LogLevel.Fatal);

            OnLoggerReconfigured(EventArgs.Empty);
        }

        private TargetWithFilterChain GetTargetsForLevel(LogLevel level)
        {
            return _configuration.GetTargetsForLevel(level);
        }

        /// <summary>
        /// Raises the event when the logger is reconfigured. 
        /// </summary>
        /// <param name="e">Event arguments</param>
        protected virtual void OnLoggerReconfigured(EventArgs e)
        {
            LoggerReconfigured?.Invoke(this, e);
        }
    }
}
