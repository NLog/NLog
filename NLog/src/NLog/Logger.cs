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

using NLog.Internal;

namespace NLog
{
    /// <summary>
    /// Provides logging interface and utility functions.
    /// </summary>
    [CLSCompliant(true)]
    public class Logger
    {
        private LoggerConfiguration _configuration;
        private string _loggerName;
        private Type _loggerType = typeof(Logger);
        private bool _isTraceEnabled;
        private bool _isDebugEnabled;
        private bool _isInfoEnabled;
        private bool _isWarnEnabled;
        private bool _isErrorEnabled;
        private bool _isFatalEnabled;

        private TargetWithFilterChain GetTargetsForLevel(LogLevel level)
        {
            return _configuration.GetTargetsForLevel(level);
        }

        internal Logger(string name, LoggerConfiguration configuration)
        {
            _loggerName = name;
            SetConfiguration(configuration);
        }

        /// <summary>
        /// Gets the name of the logger.
        /// </summary>
        public string Name
        {
            get
            {
                return _loggerName;
            }
        }

        internal void WriteToTargets(LogLevel level, IFormatProvider formatProvider, string message, object[]args, Exception exception)
        {
            LoggerImpl.Write(_loggerType, this.Name, level, GetTargetsForLevel(level), formatProvider, message, args, exception);
        }

        internal void WriteToTargets(LogLevel level, string message)
        {
            LoggerImpl.Write(_loggerType, this.Name, level, GetTargetsForLevel(level), null, message, null, null);
        }

        internal void WriteToTargets(LogLevel level, string message, object[]args)
        {
            LoggerImpl.Write(_loggerType, this.Name, level, GetTargetsForLevel(level), null, message, args, null);
        }

        internal void SetConfiguration(LoggerConfiguration configuration)
        {
            _configuration = configuration;

            // pre-calculate 'enabled' flags
            _isTraceEnabled = configuration.IsEnabled(LogLevel.Trace);
            _isDebugEnabled = configuration.IsEnabled(LogLevel.Debug);
            _isInfoEnabled = configuration.IsEnabled(LogLevel.Info);
            _isWarnEnabled = configuration.IsEnabled(LogLevel.Warn);
            _isErrorEnabled = configuration.IsEnabled(LogLevel.Error);
            _isFatalEnabled = configuration.IsEnabled(LogLevel.Fatal);
        }

        /// <summary>
        /// Determines if logging is enabled for the specified level.
        /// </summary>
        /// <param name="level">level to be checked</param>
        /// <returns><see langword="true" /> if logging is enabled for the specified level, otherwise it returns <see langword="false" />.</returns>
        public bool IsEnabled(LogLevel level)
        {
            LogManager.CheckForConfigChanges();
            return GetTargetsForLevel(level) != null; 
        }

        /// <summary>
        /// Determines if logging is enabled for the <c>Trace</c> level.
        /// </summary>
        /// <returns><see langword="true" /> if logging is enabled for the <c>Trace</c> level, otherwise it returns <see langword="false" />.</returns>
        public bool IsTraceEnabled
        {
            get
            {
                LogManager.CheckForConfigChanges();
                return _isTraceEnabled; 
            }
        }

        /// <summary>
        /// Determines if logging is enabled for the <c>Debug</c> level.
        /// </summary>
        /// <returns><see langword="true" /> if logging is enabled for the <c>Debug</c> level, otherwise it returns <see langword="false" />.</returns>
        public bool IsDebugEnabled
        {
            get
            {
                LogManager.CheckForConfigChanges();
                return _isDebugEnabled; 
            }
        }

        /// <summary>
        /// Determines if logging is enabled for the <c>Info</c> level.
        /// </summary>
        /// <returns><see langword="true" /> if logging is enabled for the <c>Info</c> level, otherwise it returns <see langword="false" />.</returns>
        public bool IsInfoEnabled
        {
            get
            {
                LogManager.CheckForConfigChanges();
                return _isInfoEnabled; 
            }
        }

        /// <summary>
        /// Determines if logging is enabled for the <c>Warn</c> level.
        /// </summary>
        /// <returns><see langword="true" /> if logging is enabled for the <c>Warn</c> level, otherwise it returns <see langword="false" />.</returns>
        public bool IsWarnEnabled
        {
            get
            {
                LogManager.CheckForConfigChanges();
                return _isWarnEnabled;
            }
        }

        /// <summary>
        /// Determines if logging is enabled for the <c>Error</c> level.
        /// </summary>
        /// <returns><see langword="true" /> if logging is enabled for the <c>Error</c> level, otherwise it returns <see langword="false" />.</returns>
        public bool IsErrorEnabled
        {
            get
            {
                LogManager.CheckForConfigChanges();
                return _isErrorEnabled; 
            }
        }

        /// <summary>
        /// Determines if logging is enabled for the <c>Fatal</c> level.
        /// </summary>
        /// <returns><see langword="true" /> if logging is enabled for the <c>Fatal</c> level, otherwise it returns <see langword="false" />.</returns>
        public bool IsFatalEnabled
        {
            get
            {
                LogManager.CheckForConfigChanges();
                return _isFatalEnabled; 
            }
        }

        // the following code has been automatically generated by a PERL script

        #region Log() overloads 

        /// <overloads>
        /// Writes the diagnostic message at the specified level using the specified format provider and format parameters.
        /// </overloads>
        /// <summary>
        /// Writes the diagnostic message at the specified level.
        /// </summary>
        /// <param name="level">the log level.</param>
        /// <param name="message">A <see langword="string" /> to be written.</param>
        public void Log(LogLevel level, string message) {
            if (IsEnabled(level))
                WriteToTargets(level, message);
        }

        /// <summary>
        /// Writes the diagnostic message and exception at the specified level.
        /// </summary>
        /// <param name="level">the log level.</param>
        /// <param name="message">A <see langword="string" /> to be written.</param>
        /// <param name="exception">An exception to be logged.</param>
        public void LogException(LogLevel level, string message, Exception exception) {
            if (IsEnabled(level))
                WriteToTargets(level, null, message, null, exception);
        }

        /// <summary>
        /// Writes the diagnostic message at the specified level using the specified parameters and formatting them with the supplied format provider.
        /// </summary>
        /// <param name="level">the log level.</param>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing format items.</param>
        /// <param name="args">Arguments to format.</param>
        public void Log(LogLevel level, IFormatProvider formatProvider, string message, params object[] args) { 
            if (IsEnabled(level))
                WriteToTargets(level, formatProvider, message, args, null); 
        }
        /// <summary>
        /// Writes the diagnostic message at the specified level using the specified parameters.
        /// </summary>
        /// <param name="level">the log level.</param>
        /// <param name="message">A <see langword="string" /> containing format items.</param>
        /// <param name="args">Arguments to format.</param>
        public void Log(LogLevel level, string message, params object[] args) { 
            if (IsEnabled(level))
                WriteToTargets(level, message, args);
        }

        /// <summary>
        /// Writes the diagnostic message at the specified level using the specified parameters.
        /// </summary>
        /// <param name="level">the log level.</param>
        /// <param name="message">A <see langword="string" /> containing format items.</param>
        /// <param name="arg1">First argument to format.</param>
        /// <param name="arg2">Second argument to format.</param>
        public void Log(LogLevel level, string message, System.Object arg1, System.Object arg2) { 
            if (IsEnabled(level))
                WriteToTargets(level, message, new object[] { arg1, arg2 });
        }

        /// <summary>
        /// Writes the diagnostic message at the specified level using the specified parameters.
        /// </summary>
        /// <param name="level">the log level.</param>
        /// <param name="message">A <see langword="string" /> containing format items.</param>
        /// <param name="arg1">First argument to format.</param>
        /// <param name="arg2">Second argument to format.</param>
        /// <param name="arg3">Third argument to format.</param>
        public void Log(LogLevel level, string message, System.Object arg1, System.Object arg2, System.Object arg3) { 
            if (IsEnabled(level))
                WriteToTargets(level, message, new object[] { arg1, arg2, arg3 });
        }
        /// <summary>
        /// Writes the diagnostic message at the specified level using the specified <see cref="T:System.Boolean" /> as a parameter and formatting it with the supplied format provider.
        /// </summary>
        /// <param name="level">the log level.</param>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.Boolean" /> argument to format.</param>
        public void Log(LogLevel level, IFormatProvider formatProvider, string message, System.Boolean argument) { 
            if (IsEnabled(level))
                WriteToTargets(level, formatProvider, message, new object[] { argument }, null); 
        }
        /// <summary>
        /// Writes the diagnostic message at the specified level using the specified <see cref="T:System.Boolean" /> as a parameter.
        /// </summary>
        /// <param name="level">the log level.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.Boolean" /> argument to format.</param>
        public void Log(LogLevel level, string message, System.Boolean argument) { 
            if (IsEnabled(level))
                WriteToTargets(level, message, new object[] { argument });
        }
        /// <summary>
        /// Writes the diagnostic message at the specified level using the specified <see cref="T:System.Char" /> as a parameter and formatting it with the supplied format provider.
        /// </summary>
        /// <param name="level">the log level.</param>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.Char" /> argument to format.</param>
        public void Log(LogLevel level, IFormatProvider formatProvider, string message, System.Char argument) { 
            if (IsEnabled(level))
                WriteToTargets(level, formatProvider, message, new object[] { argument }, null); 
        }
        /// <summary>
        /// Writes the diagnostic message at the specified level using the specified <see cref="T:System.Char" /> as a parameter.
        /// </summary>
        /// <param name="level">the log level.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.Char" /> argument to format.</param>
        public void Log(LogLevel level, string message, System.Char argument) { 
            if (IsEnabled(level))
                WriteToTargets(level, message, new object[] { argument });
        }
        /// <summary>
        /// Writes the diagnostic message at the specified level using the specified <see cref="T:System.Byte" /> as a parameter and formatting it with the supplied format provider.
        /// </summary>
        /// <param name="level">the log level.</param>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.Byte" /> argument to format.</param>
        public void Log(LogLevel level, IFormatProvider formatProvider, string message, System.Byte argument) { 
            if (IsEnabled(level))
                WriteToTargets(level, formatProvider, message, new object[] { argument }, null); 
        }
        /// <summary>
        /// Writes the diagnostic message at the specified level using the specified <see cref="T:System.Byte" /> as a parameter.
        /// </summary>
        /// <param name="level">the log level.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.Byte" /> argument to format.</param>
        public void Log(LogLevel level, string message, System.Byte argument) { 
            if (IsEnabled(level))
                WriteToTargets(level, message, new object[] { argument });
        }
        /// <summary>
        /// Writes the diagnostic message at the specified level using the specified <see cref="T:System.String" /> as a parameter and formatting it with the supplied format provider.
        /// </summary>
        /// <param name="level">the log level.</param>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.String" /> argument to format.</param>
        public void Log(LogLevel level, IFormatProvider formatProvider, string message, System.String argument) { 
            if (IsEnabled(level))
                WriteToTargets(level, formatProvider, message, new object[] { argument }, null); 
        }
        /// <summary>
        /// Writes the diagnostic message at the specified level using the specified <see cref="T:System.String" /> as a parameter.
        /// </summary>
        /// <param name="level">the log level.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.String" /> argument to format.</param>
        public void Log(LogLevel level, string message, System.String argument) { 
            if (IsEnabled(level))
                WriteToTargets(level, message, new object[] { argument });
        }
        /// <summary>
        /// Writes the diagnostic message at the specified level using the specified <see cref="T:System.Int32" /> as a parameter and formatting it with the supplied format provider.
        /// </summary>
        /// <param name="level">the log level.</param>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.Int32" /> argument to format.</param>
        public void Log(LogLevel level, IFormatProvider formatProvider, string message, System.Int32 argument) { 
            if (IsEnabled(level))
                WriteToTargets(level, formatProvider, message, new object[] { argument }, null); 
        }
        /// <summary>
        /// Writes the diagnostic message at the specified level using the specified <see cref="T:System.Int32" /> as a parameter.
        /// </summary>
        /// <param name="level">the log level.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.Int32" /> argument to format.</param>
        public void Log(LogLevel level, string message, System.Int32 argument) { 
            if (IsEnabled(level))
                WriteToTargets(level, message, new object[] { argument });
        }
        /// <summary>
        /// Writes the diagnostic message at the specified level using the specified <see cref="T:System.Int64" /> as a parameter and formatting it with the supplied format provider.
        /// </summary>
        /// <param name="level">the log level.</param>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.Int64" /> argument to format.</param>
        public void Log(LogLevel level, IFormatProvider formatProvider, string message, System.Int64 argument) { 
            if (IsEnabled(level))
                WriteToTargets(level, formatProvider, message, new object[] { argument }, null); 
        }
        /// <summary>
        /// Writes the diagnostic message at the specified level using the specified <see cref="T:System.Int64" /> as a parameter.
        /// </summary>
        /// <param name="level">the log level.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.Int64" /> argument to format.</param>
        public void Log(LogLevel level, string message, System.Int64 argument) { 
            if (IsEnabled(level))
                WriteToTargets(level, message, new object[] { argument });
        }
        /// <summary>
        /// Writes the diagnostic message at the specified level using the specified <see cref="T:System.Single" /> as a parameter and formatting it with the supplied format provider.
        /// </summary>
        /// <param name="level">the log level.</param>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.Single" /> argument to format.</param>
        public void Log(LogLevel level, IFormatProvider formatProvider, string message, System.Single argument) { 
            if (IsEnabled(level))
                WriteToTargets(level, formatProvider, message, new object[] { argument }, null); 
        }
        /// <summary>
        /// Writes the diagnostic message at the specified level using the specified <see cref="T:System.Single" /> as a parameter.
        /// </summary>
        /// <param name="level">the log level.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.Single" /> argument to format.</param>
        public void Log(LogLevel level, string message, System.Single argument) { 
            if (IsEnabled(level))
                WriteToTargets(level, message, new object[] { argument });
        }
        /// <summary>
        /// Writes the diagnostic message at the specified level using the specified <see cref="T:System.Double" /> as a parameter and formatting it with the supplied format provider.
        /// </summary>
        /// <param name="level">the log level.</param>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.Double" /> argument to format.</param>
        public void Log(LogLevel level, IFormatProvider formatProvider, string message, System.Double argument) { 
            if (IsEnabled(level))
                WriteToTargets(level, formatProvider, message, new object[] { argument }, null); 
        }
        /// <summary>
        /// Writes the diagnostic message at the specified level using the specified <see cref="T:System.Double" /> as a parameter.
        /// </summary>
        /// <param name="level">the log level.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.Double" /> argument to format.</param>
        public void Log(LogLevel level, string message, System.Double argument) { 
            if (IsEnabled(level))
                WriteToTargets(level, message, new object[] { argument });
        }
        /// <summary>
        /// Writes the diagnostic message at the specified level using the specified <see cref="T:System.Decimal" /> as a parameter and formatting it with the supplied format provider.
        /// </summary>
        /// <param name="level">the log level.</param>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.Decimal" /> argument to format.</param>
        public void Log(LogLevel level, IFormatProvider formatProvider, string message, System.Decimal argument) { 
            if (IsEnabled(level))
                WriteToTargets(level, formatProvider, message, new object[] { argument }, null); 
        }
        /// <summary>
        /// Writes the diagnostic message at the specified level using the specified <see cref="T:System.Decimal" /> as a parameter.
        /// </summary>
        /// <param name="level">the log level.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.Decimal" /> argument to format.</param>
        public void Log(LogLevel level, string message, System.Decimal argument) { 
            if (IsEnabled(level))
                WriteToTargets(level, message, new object[] { argument });
        }
        /// <summary>
        /// Writes the diagnostic message at the specified level using the specified <see cref="T:System.Object" /> as a parameter and formatting it with the supplied format provider.
        /// </summary>
        /// <param name="level">the log level.</param>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.Object" /> argument to format.</param>
        public void Log(LogLevel level, IFormatProvider formatProvider, string message, System.Object argument) { 
            if (IsEnabled(level))
                WriteToTargets(level, formatProvider, message, new object[] { argument }, null); 
        }
        /// <summary>
        /// Writes the diagnostic message at the specified level using the specified <see cref="T:System.Object" /> as a parameter.
        /// </summary>
        /// <param name="level">the log level.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.Object" /> argument to format.</param>
        public void Log(LogLevel level, string message, System.Object argument) { 
            if (IsEnabled(level))
                WriteToTargets(level, message, new object[] { argument });
        }
        /// <summary>
        /// Writes the diagnostic message at the specified level using the specified <see cref="T:System.SByte" /> as a parameter and formatting it with the supplied format provider.
        /// </summary>
        /// <param name="level">the log level.</param>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.SByte" /> argument to format.</param>
        [CLSCompliant(false)]
        public void Log(LogLevel level, IFormatProvider formatProvider, string message, System.SByte argument) { 
            if (IsEnabled(level))
                WriteToTargets(level, formatProvider, message, new object[] { argument }, null); 
        }
        /// <summary>
        /// Writes the diagnostic message at the specified level using the specified <see cref="T:System.SByte" /> as a parameter.
        /// </summary>
        /// <param name="level">the log level.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.SByte" /> argument to format.</param>
        [CLSCompliant(false)]
        public void Log(LogLevel level, string message, System.SByte argument) { 
            if (IsEnabled(level))
                WriteToTargets(level, message, new object[] { argument });
        }
        /// <summary>
        /// Writes the diagnostic message at the specified level using the specified <see cref="T:System.UInt32" /> as a parameter and formatting it with the supplied format provider.
        /// </summary>
        /// <param name="level">the log level.</param>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.UInt32" /> argument to format.</param>
        [CLSCompliant(false)]
        public void Log(LogLevel level, IFormatProvider formatProvider, string message, System.UInt32 argument) { 
            if (IsEnabled(level))
                WriteToTargets(level, formatProvider, message, new object[] { argument }, null); 
        }
        /// <summary>
        /// Writes the diagnostic message at the specified level using the specified <see cref="T:System.UInt32" /> as a parameter.
        /// </summary>
        /// <param name="level">the log level.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.UInt32" /> argument to format.</param>
        [CLSCompliant(false)]
        public void Log(LogLevel level, string message, System.UInt32 argument) { 
            if (IsEnabled(level))
                WriteToTargets(level, message, new object[] { argument });
        }
        /// <summary>
        /// Writes the diagnostic message at the specified level using the specified <see cref="T:System.UInt64" /> as a parameter and formatting it with the supplied format provider.
        /// </summary>
        /// <param name="level">the log level.</param>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.UInt64" /> argument to format.</param>
        [CLSCompliant(false)]
        public void Log(LogLevel level, IFormatProvider formatProvider, string message, System.UInt64 argument) { 
            if (IsEnabled(level))
                WriteToTargets(level, formatProvider, message, new object[] { argument }, null); 
        }
        /// <summary>
        /// Writes the diagnostic message at the specified level using the specified <see cref="T:System.UInt64" /> as a parameter.
        /// </summary>
        /// <param name="level">the log level.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.UInt64" /> argument to format.</param>
        [CLSCompliant(false)]
        public void Log(LogLevel level, string message, System.UInt64 argument) { 
            if (IsEnabled(level))
                WriteToTargets(level, message, new object[] { argument });
        }

        #endregion


        #region Trace() overloads 

        /// <overloads>
        /// Writes the diagnostic message at the <c>Trace</c> level using the specified format provider and format parameters.
        /// </overloads>
        /// <summary>
        /// Writes the diagnostic message at the <c>Trace</c> level.
        /// </summary>
        /// <param name="message">A <see langword="string" /> to be written.</param>
        public void Trace(string message) {
            if (IsTraceEnabled)
                WriteToTargets(LogLevel.Trace, message);
        }

        /// <summary>
        /// Writes the diagnostic message and exception at the <c>Trace</c> level.
        /// </summary>
        /// <param name="message">A <see langword="string" /> to be written.</param>
        /// <param name="exception">An exception to be logged.</param>
        public void TraceException(string message, Exception exception) {
            if (IsTraceEnabled)
                WriteToTargets(LogLevel.Trace, null, message, null, exception);
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Trace</c> level using the specified parameters and formatting them with the supplied format provider.
        /// </summary>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing format items.</param>
        /// <param name="args">Arguments to format.</param>
        public void Trace(IFormatProvider formatProvider, string message, params object[] args) { 
            if (IsTraceEnabled)
                WriteToTargets(LogLevel.Trace, formatProvider, message, args, null); 
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Trace</c> level using the specified parameters.
        /// </summary>
        /// <param name="message">A <see langword="string" /> containing format items.</param>
        /// <param name="args">Arguments to format.</param>
        public void Trace(string message, params object[] args) { 
            if (IsTraceEnabled)
                WriteToTargets(LogLevel.Trace, message, args);
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Trace</c> level using the specified parameters.
        /// </summary>
        /// <param name="message">A <see langword="string" /> containing format items.</param>
        /// <param name="arg1">First argument to format.</param>
        /// <param name="arg2">Second argument to format.</param>
        public void Trace(string message, System.Object arg1, System.Object arg2) { 
            if (IsTraceEnabled)
                WriteToTargets(LogLevel.Trace, message, new object[] { arg1, arg2 });
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Trace</c> level using the specified parameters.
        /// </summary>
        /// <param name="message">A <see langword="string" /> containing format items.</param>
        /// <param name="arg1">First argument to format.</param>
        /// <param name="arg2">Second argument to format.</param>
        /// <param name="arg3">Third argument to format.</param>
        public void Trace(string message, System.Object arg1, System.Object arg2, System.Object arg3) { 
            if (IsTraceEnabled)
                WriteToTargets(LogLevel.Trace, message, new object[] { arg1, arg2, arg3 });
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Trace</c> level using the specified <see cref="T:System.Boolean" /> as a parameter and formatting it with the supplied format provider.
        /// </summary>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.Boolean" /> argument to format.</param>
        public void Trace(IFormatProvider formatProvider, string message, System.Boolean argument) { 
            if (IsTraceEnabled)
                WriteToTargets(LogLevel.Trace, formatProvider, message, new object[] { argument }, null); 
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Trace</c> level using the specified <see cref="T:System.Boolean" /> as a parameter.
        /// </summary>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.Boolean" /> argument to format.</param>
        public void Trace(string message, System.Boolean argument) { 
            if (IsTraceEnabled)
                WriteToTargets(LogLevel.Trace, message, new object[] { argument });
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Trace</c> level using the specified <see cref="T:System.Char" /> as a parameter and formatting it with the supplied format provider.
        /// </summary>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.Char" /> argument to format.</param>
        public void Trace(IFormatProvider formatProvider, string message, System.Char argument) { 
            if (IsTraceEnabled)
                WriteToTargets(LogLevel.Trace, formatProvider, message, new object[] { argument }, null); 
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Trace</c> level using the specified <see cref="T:System.Char" /> as a parameter.
        /// </summary>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.Char" /> argument to format.</param>
        public void Trace(string message, System.Char argument) { 
            if (IsTraceEnabled)
                WriteToTargets(LogLevel.Trace, message, new object[] { argument });
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Trace</c> level using the specified <see cref="T:System.Byte" /> as a parameter and formatting it with the supplied format provider.
        /// </summary>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.Byte" /> argument to format.</param>
        public void Trace(IFormatProvider formatProvider, string message, System.Byte argument) { 
            if (IsTraceEnabled)
                WriteToTargets(LogLevel.Trace, formatProvider, message, new object[] { argument }, null); 
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Trace</c> level using the specified <see cref="T:System.Byte" /> as a parameter.
        /// </summary>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.Byte" /> argument to format.</param>
        public void Trace(string message, System.Byte argument) { 
            if (IsTraceEnabled)
                WriteToTargets(LogLevel.Trace, message, new object[] { argument });
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Trace</c> level using the specified <see cref="T:System.String" /> as a parameter and formatting it with the supplied format provider.
        /// </summary>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.String" /> argument to format.</param>
        public void Trace(IFormatProvider formatProvider, string message, System.String argument) { 
            if (IsTraceEnabled)
                WriteToTargets(LogLevel.Trace, formatProvider, message, new object[] { argument }, null); 
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Trace</c> level using the specified <see cref="T:System.String" /> as a parameter.
        /// </summary>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.String" /> argument to format.</param>
        public void Trace(string message, System.String argument) { 
            if (IsTraceEnabled)
                WriteToTargets(LogLevel.Trace, message, new object[] { argument });
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Trace</c> level using the specified <see cref="T:System.Int32" /> as a parameter and formatting it with the supplied format provider.
        /// </summary>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.Int32" /> argument to format.</param>
        public void Trace(IFormatProvider formatProvider, string message, System.Int32 argument) { 
            if (IsTraceEnabled)
                WriteToTargets(LogLevel.Trace, formatProvider, message, new object[] { argument }, null); 
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Trace</c> level using the specified <see cref="T:System.Int32" /> as a parameter.
        /// </summary>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.Int32" /> argument to format.</param>
        public void Trace(string message, System.Int32 argument) { 
            if (IsTraceEnabled)
                WriteToTargets(LogLevel.Trace, message, new object[] { argument });
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Trace</c> level using the specified <see cref="T:System.Int64" /> as a parameter and formatting it with the supplied format provider.
        /// </summary>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.Int64" /> argument to format.</param>
        public void Trace(IFormatProvider formatProvider, string message, System.Int64 argument) { 
            if (IsTraceEnabled)
                WriteToTargets(LogLevel.Trace, formatProvider, message, new object[] { argument }, null); 
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Trace</c> level using the specified <see cref="T:System.Int64" /> as a parameter.
        /// </summary>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.Int64" /> argument to format.</param>
        public void Trace(string message, System.Int64 argument) { 
            if (IsTraceEnabled)
                WriteToTargets(LogLevel.Trace, message, new object[] { argument });
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Trace</c> level using the specified <see cref="T:System.Single" /> as a parameter and formatting it with the supplied format provider.
        /// </summary>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.Single" /> argument to format.</param>
        public void Trace(IFormatProvider formatProvider, string message, System.Single argument) { 
            if (IsTraceEnabled)
                WriteToTargets(LogLevel.Trace, formatProvider, message, new object[] { argument }, null); 
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Trace</c> level using the specified <see cref="T:System.Single" /> as a parameter.
        /// </summary>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.Single" /> argument to format.</param>
        public void Trace(string message, System.Single argument) { 
            if (IsTraceEnabled)
                WriteToTargets(LogLevel.Trace, message, new object[] { argument });
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Trace</c> level using the specified <see cref="T:System.Double" /> as a parameter and formatting it with the supplied format provider.
        /// </summary>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.Double" /> argument to format.</param>
        public void Trace(IFormatProvider formatProvider, string message, System.Double argument) { 
            if (IsTraceEnabled)
                WriteToTargets(LogLevel.Trace, formatProvider, message, new object[] { argument }, null); 
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Trace</c> level using the specified <see cref="T:System.Double" /> as a parameter.
        /// </summary>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.Double" /> argument to format.</param>
        public void Trace(string message, System.Double argument) { 
            if (IsTraceEnabled)
                WriteToTargets(LogLevel.Trace, message, new object[] { argument });
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Trace</c> level using the specified <see cref="T:System.Decimal" /> as a parameter and formatting it with the supplied format provider.
        /// </summary>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.Decimal" /> argument to format.</param>
        public void Trace(IFormatProvider formatProvider, string message, System.Decimal argument) { 
            if (IsTraceEnabled)
                WriteToTargets(LogLevel.Trace, formatProvider, message, new object[] { argument }, null); 
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Trace</c> level using the specified <see cref="T:System.Decimal" /> as a parameter.
        /// </summary>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.Decimal" /> argument to format.</param>
        public void Trace(string message, System.Decimal argument) { 
            if (IsTraceEnabled)
                WriteToTargets(LogLevel.Trace, message, new object[] { argument });
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Trace</c> level using the specified <see cref="T:System.Object" /> as a parameter and formatting it with the supplied format provider.
        /// </summary>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.Object" /> argument to format.</param>
        public void Trace(IFormatProvider formatProvider, string message, System.Object argument) { 
            if (IsTraceEnabled)
                WriteToTargets(LogLevel.Trace, formatProvider, message, new object[] { argument }, null); 
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Trace</c> level using the specified <see cref="T:System.Object" /> as a parameter.
        /// </summary>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.Object" /> argument to format.</param>
        public void Trace(string message, System.Object argument) { 
            if (IsTraceEnabled)
                WriteToTargets(LogLevel.Trace, message, new object[] { argument });
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Trace</c> level using the specified <see cref="T:System.SByte" /> as a parameter and formatting it with the supplied format provider.
        /// </summary>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.SByte" /> argument to format.</param>
        [CLSCompliant(false)]
        public void Trace(IFormatProvider formatProvider, string message, System.SByte argument) { 
            if (IsTraceEnabled)
                WriteToTargets(LogLevel.Trace, formatProvider, message, new object[] { argument }, null); 
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Trace</c> level using the specified <see cref="T:System.SByte" /> as a parameter.
        /// </summary>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.SByte" /> argument to format.</param>
        [CLSCompliant(false)]
        public void Trace(string message, System.SByte argument) { 
            if (IsTraceEnabled)
                WriteToTargets(LogLevel.Trace, message, new object[] { argument });
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Trace</c> level using the specified <see cref="T:System.UInt32" /> as a parameter and formatting it with the supplied format provider.
        /// </summary>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.UInt32" /> argument to format.</param>
        [CLSCompliant(false)]
        public void Trace(IFormatProvider formatProvider, string message, System.UInt32 argument) { 
            if (IsTraceEnabled)
                WriteToTargets(LogLevel.Trace, formatProvider, message, new object[] { argument }, null); 
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Trace</c> level using the specified <see cref="T:System.UInt32" /> as a parameter.
        /// </summary>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.UInt32" /> argument to format.</param>
        [CLSCompliant(false)]
        public void Trace(string message, System.UInt32 argument) { 
            if (IsTraceEnabled)
                WriteToTargets(LogLevel.Trace, message, new object[] { argument });
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Trace</c> level using the specified <see cref="T:System.UInt64" /> as a parameter and formatting it with the supplied format provider.
        /// </summary>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.UInt64" /> argument to format.</param>
        [CLSCompliant(false)]
        public void Trace(IFormatProvider formatProvider, string message, System.UInt64 argument) { 
            if (IsTraceEnabled)
                WriteToTargets(LogLevel.Trace, formatProvider, message, new object[] { argument }, null); 
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Trace</c> level using the specified <see cref="T:System.UInt64" /> as a parameter.
        /// </summary>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.UInt64" /> argument to format.</param>
        [CLSCompliant(false)]
        public void Trace(string message, System.UInt64 argument) { 
            if (IsTraceEnabled)
                WriteToTargets(LogLevel.Trace, message, new object[] { argument });
        }

        #endregion


        #region Debug() overloads 

        /// <overloads>
        /// Writes the diagnostic message at the <c>Debug</c> level using the specified format provider and format parameters.
        /// </overloads>
        /// <summary>
        /// Writes the diagnostic message at the <c>Debug</c> level.
        /// </summary>
        /// <param name="message">A <see langword="string" /> to be written.</param>
        public void Debug(string message) {
            if (IsDebugEnabled)
                WriteToTargets(LogLevel.Debug, message);
        }

        /// <summary>
        /// Writes the diagnostic message and exception at the <c>Debug</c> level.
        /// </summary>
        /// <param name="message">A <see langword="string" /> to be written.</param>
        /// <param name="exception">An exception to be logged.</param>
        public void DebugException(string message, Exception exception) {
            if (IsDebugEnabled)
                WriteToTargets(LogLevel.Debug, null, message, null, exception);
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Debug</c> level using the specified parameters and formatting them with the supplied format provider.
        /// </summary>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing format items.</param>
        /// <param name="args">Arguments to format.</param>
        public void Debug(IFormatProvider formatProvider, string message, params object[] args) { 
            if (IsDebugEnabled)
                WriteToTargets(LogLevel.Debug, formatProvider, message, args, null); 
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Debug</c> level using the specified parameters.
        /// </summary>
        /// <param name="message">A <see langword="string" /> containing format items.</param>
        /// <param name="args">Arguments to format.</param>
        public void Debug(string message, params object[] args) { 
            if (IsDebugEnabled)
                WriteToTargets(LogLevel.Debug, message, args);
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Debug</c> level using the specified parameters.
        /// </summary>
        /// <param name="message">A <see langword="string" /> containing format items.</param>
        /// <param name="arg1">First argument to format.</param>
        /// <param name="arg2">Second argument to format.</param>
        public void Debug(string message, System.Object arg1, System.Object arg2) { 
            if (IsDebugEnabled)
                WriteToTargets(LogLevel.Debug, message, new object[] { arg1, arg2 });
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Debug</c> level using the specified parameters.
        /// </summary>
        /// <param name="message">A <see langword="string" /> containing format items.</param>
        /// <param name="arg1">First argument to format.</param>
        /// <param name="arg2">Second argument to format.</param>
        /// <param name="arg3">Third argument to format.</param>
        public void Debug(string message, System.Object arg1, System.Object arg2, System.Object arg3) { 
            if (IsDebugEnabled)
                WriteToTargets(LogLevel.Debug, message, new object[] { arg1, arg2, arg3 });
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Debug</c> level using the specified <see cref="T:System.Boolean" /> as a parameter and formatting it with the supplied format provider.
        /// </summary>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.Boolean" /> argument to format.</param>
        public void Debug(IFormatProvider formatProvider, string message, System.Boolean argument) { 
            if (IsDebugEnabled)
                WriteToTargets(LogLevel.Debug, formatProvider, message, new object[] { argument }, null); 
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Debug</c> level using the specified <see cref="T:System.Boolean" /> as a parameter.
        /// </summary>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.Boolean" /> argument to format.</param>
        public void Debug(string message, System.Boolean argument) { 
            if (IsDebugEnabled)
                WriteToTargets(LogLevel.Debug, message, new object[] { argument });
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Debug</c> level using the specified <see cref="T:System.Char" /> as a parameter and formatting it with the supplied format provider.
        /// </summary>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.Char" /> argument to format.</param>
        public void Debug(IFormatProvider formatProvider, string message, System.Char argument) { 
            if (IsDebugEnabled)
                WriteToTargets(LogLevel.Debug, formatProvider, message, new object[] { argument }, null); 
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Debug</c> level using the specified <see cref="T:System.Char" /> as a parameter.
        /// </summary>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.Char" /> argument to format.</param>
        public void Debug(string message, System.Char argument) { 
            if (IsDebugEnabled)
                WriteToTargets(LogLevel.Debug, message, new object[] { argument });
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Debug</c> level using the specified <see cref="T:System.Byte" /> as a parameter and formatting it with the supplied format provider.
        /// </summary>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.Byte" /> argument to format.</param>
        public void Debug(IFormatProvider formatProvider, string message, System.Byte argument) { 
            if (IsDebugEnabled)
                WriteToTargets(LogLevel.Debug, formatProvider, message, new object[] { argument }, null); 
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Debug</c> level using the specified <see cref="T:System.Byte" /> as a parameter.
        /// </summary>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.Byte" /> argument to format.</param>
        public void Debug(string message, System.Byte argument) { 
            if (IsDebugEnabled)
                WriteToTargets(LogLevel.Debug, message, new object[] { argument });
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Debug</c> level using the specified <see cref="T:System.String" /> as a parameter and formatting it with the supplied format provider.
        /// </summary>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.String" /> argument to format.</param>
        public void Debug(IFormatProvider formatProvider, string message, System.String argument) { 
            if (IsDebugEnabled)
                WriteToTargets(LogLevel.Debug, formatProvider, message, new object[] { argument }, null); 
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Debug</c> level using the specified <see cref="T:System.String" /> as a parameter.
        /// </summary>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.String" /> argument to format.</param>
        public void Debug(string message, System.String argument) { 
            if (IsDebugEnabled)
                WriteToTargets(LogLevel.Debug, message, new object[] { argument });
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Debug</c> level using the specified <see cref="T:System.Int32" /> as a parameter and formatting it with the supplied format provider.
        /// </summary>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.Int32" /> argument to format.</param>
        public void Debug(IFormatProvider formatProvider, string message, System.Int32 argument) { 
            if (IsDebugEnabled)
                WriteToTargets(LogLevel.Debug, formatProvider, message, new object[] { argument }, null); 
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Debug</c> level using the specified <see cref="T:System.Int32" /> as a parameter.
        /// </summary>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.Int32" /> argument to format.</param>
        public void Debug(string message, System.Int32 argument) { 
            if (IsDebugEnabled)
                WriteToTargets(LogLevel.Debug, message, new object[] { argument });
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Debug</c> level using the specified <see cref="T:System.Int64" /> as a parameter and formatting it with the supplied format provider.
        /// </summary>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.Int64" /> argument to format.</param>
        public void Debug(IFormatProvider formatProvider, string message, System.Int64 argument) { 
            if (IsDebugEnabled)
                WriteToTargets(LogLevel.Debug, formatProvider, message, new object[] { argument }, null); 
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Debug</c> level using the specified <see cref="T:System.Int64" /> as a parameter.
        /// </summary>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.Int64" /> argument to format.</param>
        public void Debug(string message, System.Int64 argument) { 
            if (IsDebugEnabled)
                WriteToTargets(LogLevel.Debug, message, new object[] { argument });
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Debug</c> level using the specified <see cref="T:System.Single" /> as a parameter and formatting it with the supplied format provider.
        /// </summary>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.Single" /> argument to format.</param>
        public void Debug(IFormatProvider formatProvider, string message, System.Single argument) { 
            if (IsDebugEnabled)
                WriteToTargets(LogLevel.Debug, formatProvider, message, new object[] { argument }, null); 
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Debug</c> level using the specified <see cref="T:System.Single" /> as a parameter.
        /// </summary>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.Single" /> argument to format.</param>
        public void Debug(string message, System.Single argument) { 
            if (IsDebugEnabled)
                WriteToTargets(LogLevel.Debug, message, new object[] { argument });
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Debug</c> level using the specified <see cref="T:System.Double" /> as a parameter and formatting it with the supplied format provider.
        /// </summary>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.Double" /> argument to format.</param>
        public void Debug(IFormatProvider formatProvider, string message, System.Double argument) { 
            if (IsDebugEnabled)
                WriteToTargets(LogLevel.Debug, formatProvider, message, new object[] { argument }, null); 
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Debug</c> level using the specified <see cref="T:System.Double" /> as a parameter.
        /// </summary>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.Double" /> argument to format.</param>
        public void Debug(string message, System.Double argument) { 
            if (IsDebugEnabled)
                WriteToTargets(LogLevel.Debug, message, new object[] { argument });
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Debug</c> level using the specified <see cref="T:System.Decimal" /> as a parameter and formatting it with the supplied format provider.
        /// </summary>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.Decimal" /> argument to format.</param>
        public void Debug(IFormatProvider formatProvider, string message, System.Decimal argument) { 
            if (IsDebugEnabled)
                WriteToTargets(LogLevel.Debug, formatProvider, message, new object[] { argument }, null); 
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Debug</c> level using the specified <see cref="T:System.Decimal" /> as a parameter.
        /// </summary>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.Decimal" /> argument to format.</param>
        public void Debug(string message, System.Decimal argument) { 
            if (IsDebugEnabled)
                WriteToTargets(LogLevel.Debug, message, new object[] { argument });
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Debug</c> level using the specified <see cref="T:System.Object" /> as a parameter and formatting it with the supplied format provider.
        /// </summary>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.Object" /> argument to format.</param>
        public void Debug(IFormatProvider formatProvider, string message, System.Object argument) { 
            if (IsDebugEnabled)
                WriteToTargets(LogLevel.Debug, formatProvider, message, new object[] { argument }, null); 
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Debug</c> level using the specified <see cref="T:System.Object" /> as a parameter.
        /// </summary>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.Object" /> argument to format.</param>
        public void Debug(string message, System.Object argument) { 
            if (IsDebugEnabled)
                WriteToTargets(LogLevel.Debug, message, new object[] { argument });
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Debug</c> level using the specified <see cref="T:System.SByte" /> as a parameter and formatting it with the supplied format provider.
        /// </summary>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.SByte" /> argument to format.</param>
        [CLSCompliant(false)]
        public void Debug(IFormatProvider formatProvider, string message, System.SByte argument) { 
            if (IsDebugEnabled)
                WriteToTargets(LogLevel.Debug, formatProvider, message, new object[] { argument }, null); 
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Debug</c> level using the specified <see cref="T:System.SByte" /> as a parameter.
        /// </summary>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.SByte" /> argument to format.</param>
        [CLSCompliant(false)]
        public void Debug(string message, System.SByte argument) { 
            if (IsDebugEnabled)
                WriteToTargets(LogLevel.Debug, message, new object[] { argument });
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Debug</c> level using the specified <see cref="T:System.UInt32" /> as a parameter and formatting it with the supplied format provider.
        /// </summary>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.UInt32" /> argument to format.</param>
        [CLSCompliant(false)]
        public void Debug(IFormatProvider formatProvider, string message, System.UInt32 argument) { 
            if (IsDebugEnabled)
                WriteToTargets(LogLevel.Debug, formatProvider, message, new object[] { argument }, null); 
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Debug</c> level using the specified <see cref="T:System.UInt32" /> as a parameter.
        /// </summary>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.UInt32" /> argument to format.</param>
        [CLSCompliant(false)]
        public void Debug(string message, System.UInt32 argument) { 
            if (IsDebugEnabled)
                WriteToTargets(LogLevel.Debug, message, new object[] { argument });
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Debug</c> level using the specified <see cref="T:System.UInt64" /> as a parameter and formatting it with the supplied format provider.
        /// </summary>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.UInt64" /> argument to format.</param>
        [CLSCompliant(false)]
        public void Debug(IFormatProvider formatProvider, string message, System.UInt64 argument) { 
            if (IsDebugEnabled)
                WriteToTargets(LogLevel.Debug, formatProvider, message, new object[] { argument }, null); 
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Debug</c> level using the specified <see cref="T:System.UInt64" /> as a parameter.
        /// </summary>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.UInt64" /> argument to format.</param>
        [CLSCompliant(false)]
        public void Debug(string message, System.UInt64 argument) { 
            if (IsDebugEnabled)
                WriteToTargets(LogLevel.Debug, message, new object[] { argument });
        }

        #endregion


        #region Info() overloads 

        /// <overloads>
        /// Writes the diagnostic message at the <c>Info</c> level using the specified format provider and format parameters.
        /// </overloads>
        /// <summary>
        /// Writes the diagnostic message at the <c>Info</c> level.
        /// </summary>
        /// <param name="message">A <see langword="string" /> to be written.</param>
        public void Info(string message) {
            if (IsInfoEnabled)
                WriteToTargets(LogLevel.Info, message);
        }

        /// <summary>
        /// Writes the diagnostic message and exception at the <c>Info</c> level.
        /// </summary>
        /// <param name="message">A <see langword="string" /> to be written.</param>
        /// <param name="exception">An exception to be logged.</param>
        public void InfoException(string message, Exception exception) {
            if (IsInfoEnabled)
                WriteToTargets(LogLevel.Info, null, message, null, exception);
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Info</c> level using the specified parameters and formatting them with the supplied format provider.
        /// </summary>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing format items.</param>
        /// <param name="args">Arguments to format.</param>
        public void Info(IFormatProvider formatProvider, string message, params object[] args) { 
            if (IsInfoEnabled)
                WriteToTargets(LogLevel.Info, formatProvider, message, args, null); 
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Info</c> level using the specified parameters.
        /// </summary>
        /// <param name="message">A <see langword="string" /> containing format items.</param>
        /// <param name="args">Arguments to format.</param>
        public void Info(string message, params object[] args) { 
            if (IsInfoEnabled)
                WriteToTargets(LogLevel.Info, message, args);
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Info</c> level using the specified parameters.
        /// </summary>
        /// <param name="message">A <see langword="string" /> containing format items.</param>
        /// <param name="arg1">First argument to format.</param>
        /// <param name="arg2">Second argument to format.</param>
        public void Info(string message, System.Object arg1, System.Object arg2) { 
            if (IsInfoEnabled)
                WriteToTargets(LogLevel.Info, message, new object[] { arg1, arg2 });
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Info</c> level using the specified parameters.
        /// </summary>
        /// <param name="message">A <see langword="string" /> containing format items.</param>
        /// <param name="arg1">First argument to format.</param>
        /// <param name="arg2">Second argument to format.</param>
        /// <param name="arg3">Third argument to format.</param>
        public void Info(string message, System.Object arg1, System.Object arg2, System.Object arg3) { 
            if (IsInfoEnabled)
                WriteToTargets(LogLevel.Info, message, new object[] { arg1, arg2, arg3 });
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Info</c> level using the specified <see cref="T:System.Boolean" /> as a parameter and formatting it with the supplied format provider.
        /// </summary>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.Boolean" /> argument to format.</param>
        public void Info(IFormatProvider formatProvider, string message, System.Boolean argument) { 
            if (IsInfoEnabled)
                WriteToTargets(LogLevel.Info, formatProvider, message, new object[] { argument }, null); 
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Info</c> level using the specified <see cref="T:System.Boolean" /> as a parameter.
        /// </summary>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.Boolean" /> argument to format.</param>
        public void Info(string message, System.Boolean argument) { 
            if (IsInfoEnabled)
                WriteToTargets(LogLevel.Info, message, new object[] { argument });
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Info</c> level using the specified <see cref="T:System.Char" /> as a parameter and formatting it with the supplied format provider.
        /// </summary>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.Char" /> argument to format.</param>
        public void Info(IFormatProvider formatProvider, string message, System.Char argument) { 
            if (IsInfoEnabled)
                WriteToTargets(LogLevel.Info, formatProvider, message, new object[] { argument }, null); 
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Info</c> level using the specified <see cref="T:System.Char" /> as a parameter.
        /// </summary>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.Char" /> argument to format.</param>
        public void Info(string message, System.Char argument) { 
            if (IsInfoEnabled)
                WriteToTargets(LogLevel.Info, message, new object[] { argument });
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Info</c> level using the specified <see cref="T:System.Byte" /> as a parameter and formatting it with the supplied format provider.
        /// </summary>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.Byte" /> argument to format.</param>
        public void Info(IFormatProvider formatProvider, string message, System.Byte argument) { 
            if (IsInfoEnabled)
                WriteToTargets(LogLevel.Info, formatProvider, message, new object[] { argument }, null); 
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Info</c> level using the specified <see cref="T:System.Byte" /> as a parameter.
        /// </summary>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.Byte" /> argument to format.</param>
        public void Info(string message, System.Byte argument) { 
            if (IsInfoEnabled)
                WriteToTargets(LogLevel.Info, message, new object[] { argument });
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Info</c> level using the specified <see cref="T:System.String" /> as a parameter and formatting it with the supplied format provider.
        /// </summary>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.String" /> argument to format.</param>
        public void Info(IFormatProvider formatProvider, string message, System.String argument) { 
            if (IsInfoEnabled)
                WriteToTargets(LogLevel.Info, formatProvider, message, new object[] { argument }, null); 
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Info</c> level using the specified <see cref="T:System.String" /> as a parameter.
        /// </summary>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.String" /> argument to format.</param>
        public void Info(string message, System.String argument) { 
            if (IsInfoEnabled)
                WriteToTargets(LogLevel.Info, message, new object[] { argument });
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Info</c> level using the specified <see cref="T:System.Int32" /> as a parameter and formatting it with the supplied format provider.
        /// </summary>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.Int32" /> argument to format.</param>
        public void Info(IFormatProvider formatProvider, string message, System.Int32 argument) { 
            if (IsInfoEnabled)
                WriteToTargets(LogLevel.Info, formatProvider, message, new object[] { argument }, null); 
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Info</c> level using the specified <see cref="T:System.Int32" /> as a parameter.
        /// </summary>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.Int32" /> argument to format.</param>
        public void Info(string message, System.Int32 argument) { 
            if (IsInfoEnabled)
                WriteToTargets(LogLevel.Info, message, new object[] { argument });
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Info</c> level using the specified <see cref="T:System.Int64" /> as a parameter and formatting it with the supplied format provider.
        /// </summary>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.Int64" /> argument to format.</param>
        public void Info(IFormatProvider formatProvider, string message, System.Int64 argument) { 
            if (IsInfoEnabled)
                WriteToTargets(LogLevel.Info, formatProvider, message, new object[] { argument }, null); 
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Info</c> level using the specified <see cref="T:System.Int64" /> as a parameter.
        /// </summary>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.Int64" /> argument to format.</param>
        public void Info(string message, System.Int64 argument) { 
            if (IsInfoEnabled)
                WriteToTargets(LogLevel.Info, message, new object[] { argument });
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Info</c> level using the specified <see cref="T:System.Single" /> as a parameter and formatting it with the supplied format provider.
        /// </summary>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.Single" /> argument to format.</param>
        public void Info(IFormatProvider formatProvider, string message, System.Single argument) { 
            if (IsInfoEnabled)
                WriteToTargets(LogLevel.Info, formatProvider, message, new object[] { argument }, null); 
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Info</c> level using the specified <see cref="T:System.Single" /> as a parameter.
        /// </summary>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.Single" /> argument to format.</param>
        public void Info(string message, System.Single argument) { 
            if (IsInfoEnabled)
                WriteToTargets(LogLevel.Info, message, new object[] { argument });
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Info</c> level using the specified <see cref="T:System.Double" /> as a parameter and formatting it with the supplied format provider.
        /// </summary>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.Double" /> argument to format.</param>
        public void Info(IFormatProvider formatProvider, string message, System.Double argument) { 
            if (IsInfoEnabled)
                WriteToTargets(LogLevel.Info, formatProvider, message, new object[] { argument }, null); 
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Info</c> level using the specified <see cref="T:System.Double" /> as a parameter.
        /// </summary>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.Double" /> argument to format.</param>
        public void Info(string message, System.Double argument) { 
            if (IsInfoEnabled)
                WriteToTargets(LogLevel.Info, message, new object[] { argument });
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Info</c> level using the specified <see cref="T:System.Decimal" /> as a parameter and formatting it with the supplied format provider.
        /// </summary>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.Decimal" /> argument to format.</param>
        public void Info(IFormatProvider formatProvider, string message, System.Decimal argument) { 
            if (IsInfoEnabled)
                WriteToTargets(LogLevel.Info, formatProvider, message, new object[] { argument }, null); 
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Info</c> level using the specified <see cref="T:System.Decimal" /> as a parameter.
        /// </summary>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.Decimal" /> argument to format.</param>
        public void Info(string message, System.Decimal argument) { 
            if (IsInfoEnabled)
                WriteToTargets(LogLevel.Info, message, new object[] { argument });
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Info</c> level using the specified <see cref="T:System.Object" /> as a parameter and formatting it with the supplied format provider.
        /// </summary>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.Object" /> argument to format.</param>
        public void Info(IFormatProvider formatProvider, string message, System.Object argument) { 
            if (IsInfoEnabled)
                WriteToTargets(LogLevel.Info, formatProvider, message, new object[] { argument }, null); 
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Info</c> level using the specified <see cref="T:System.Object" /> as a parameter.
        /// </summary>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.Object" /> argument to format.</param>
        public void Info(string message, System.Object argument) { 
            if (IsInfoEnabled)
                WriteToTargets(LogLevel.Info, message, new object[] { argument });
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Info</c> level using the specified <see cref="T:System.SByte" /> as a parameter and formatting it with the supplied format provider.
        /// </summary>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.SByte" /> argument to format.</param>
        [CLSCompliant(false)]
        public void Info(IFormatProvider formatProvider, string message, System.SByte argument) { 
            if (IsInfoEnabled)
                WriteToTargets(LogLevel.Info, formatProvider, message, new object[] { argument }, null); 
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Info</c> level using the specified <see cref="T:System.SByte" /> as a parameter.
        /// </summary>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.SByte" /> argument to format.</param>
        [CLSCompliant(false)]
        public void Info(string message, System.SByte argument) { 
            if (IsInfoEnabled)
                WriteToTargets(LogLevel.Info, message, new object[] { argument });
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Info</c> level using the specified <see cref="T:System.UInt32" /> as a parameter and formatting it with the supplied format provider.
        /// </summary>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.UInt32" /> argument to format.</param>
        [CLSCompliant(false)]
        public void Info(IFormatProvider formatProvider, string message, System.UInt32 argument) { 
            if (IsInfoEnabled)
                WriteToTargets(LogLevel.Info, formatProvider, message, new object[] { argument }, null); 
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Info</c> level using the specified <see cref="T:System.UInt32" /> as a parameter.
        /// </summary>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.UInt32" /> argument to format.</param>
        [CLSCompliant(false)]
        public void Info(string message, System.UInt32 argument) { 
            if (IsInfoEnabled)
                WriteToTargets(LogLevel.Info, message, new object[] { argument });
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Info</c> level using the specified <see cref="T:System.UInt64" /> as a parameter and formatting it with the supplied format provider.
        /// </summary>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.UInt64" /> argument to format.</param>
        [CLSCompliant(false)]
        public void Info(IFormatProvider formatProvider, string message, System.UInt64 argument) { 
            if (IsInfoEnabled)
                WriteToTargets(LogLevel.Info, formatProvider, message, new object[] { argument }, null); 
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Info</c> level using the specified <see cref="T:System.UInt64" /> as a parameter.
        /// </summary>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.UInt64" /> argument to format.</param>
        [CLSCompliant(false)]
        public void Info(string message, System.UInt64 argument) { 
            if (IsInfoEnabled)
                WriteToTargets(LogLevel.Info, message, new object[] { argument });
        }

        #endregion


        #region Warn() overloads 

        /// <overloads>
        /// Writes the diagnostic message at the <c>Warn</c> level using the specified format provider and format parameters.
        /// </overloads>
        /// <summary>
        /// Writes the diagnostic message at the <c>Warn</c> level.
        /// </summary>
        /// <param name="message">A <see langword="string" /> to be written.</param>
        public void Warn(string message) {
            if (IsWarnEnabled)
                WriteToTargets(LogLevel.Warn, message);
        }

        /// <summary>
        /// Writes the diagnostic message and exception at the <c>Warn</c> level.
        /// </summary>
        /// <param name="message">A <see langword="string" /> to be written.</param>
        /// <param name="exception">An exception to be logged.</param>
        public void WarnException(string message, Exception exception) {
            if (IsWarnEnabled)
                WriteToTargets(LogLevel.Warn, null, message, null, exception);
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Warn</c> level using the specified parameters and formatting them with the supplied format provider.
        /// </summary>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing format items.</param>
        /// <param name="args">Arguments to format.</param>
        public void Warn(IFormatProvider formatProvider, string message, params object[] args) { 
            if (IsWarnEnabled)
                WriteToTargets(LogLevel.Warn, formatProvider, message, args, null); 
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Warn</c> level using the specified parameters.
        /// </summary>
        /// <param name="message">A <see langword="string" /> containing format items.</param>
        /// <param name="args">Arguments to format.</param>
        public void Warn(string message, params object[] args) { 
            if (IsWarnEnabled)
                WriteToTargets(LogLevel.Warn, message, args);
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Warn</c> level using the specified parameters.
        /// </summary>
        /// <param name="message">A <see langword="string" /> containing format items.</param>
        /// <param name="arg1">First argument to format.</param>
        /// <param name="arg2">Second argument to format.</param>
        public void Warn(string message, System.Object arg1, System.Object arg2) { 
            if (IsWarnEnabled)
                WriteToTargets(LogLevel.Warn, message, new object[] { arg1, arg2 });
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Warn</c> level using the specified parameters.
        /// </summary>
        /// <param name="message">A <see langword="string" /> containing format items.</param>
        /// <param name="arg1">First argument to format.</param>
        /// <param name="arg2">Second argument to format.</param>
        /// <param name="arg3">Third argument to format.</param>
        public void Warn(string message, System.Object arg1, System.Object arg2, System.Object arg3) { 
            if (IsWarnEnabled)
                WriteToTargets(LogLevel.Warn, message, new object[] { arg1, arg2, arg3 });
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Warn</c> level using the specified <see cref="T:System.Boolean" /> as a parameter and formatting it with the supplied format provider.
        /// </summary>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.Boolean" /> argument to format.</param>
        public void Warn(IFormatProvider formatProvider, string message, System.Boolean argument) { 
            if (IsWarnEnabled)
                WriteToTargets(LogLevel.Warn, formatProvider, message, new object[] { argument }, null); 
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Warn</c> level using the specified <see cref="T:System.Boolean" /> as a parameter.
        /// </summary>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.Boolean" /> argument to format.</param>
        public void Warn(string message, System.Boolean argument) { 
            if (IsWarnEnabled)
                WriteToTargets(LogLevel.Warn, message, new object[] { argument });
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Warn</c> level using the specified <see cref="T:System.Char" /> as a parameter and formatting it with the supplied format provider.
        /// </summary>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.Char" /> argument to format.</param>
        public void Warn(IFormatProvider formatProvider, string message, System.Char argument) { 
            if (IsWarnEnabled)
                WriteToTargets(LogLevel.Warn, formatProvider, message, new object[] { argument }, null); 
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Warn</c> level using the specified <see cref="T:System.Char" /> as a parameter.
        /// </summary>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.Char" /> argument to format.</param>
        public void Warn(string message, System.Char argument) { 
            if (IsWarnEnabled)
                WriteToTargets(LogLevel.Warn, message, new object[] { argument });
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Warn</c> level using the specified <see cref="T:System.Byte" /> as a parameter and formatting it with the supplied format provider.
        /// </summary>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.Byte" /> argument to format.</param>
        public void Warn(IFormatProvider formatProvider, string message, System.Byte argument) { 
            if (IsWarnEnabled)
                WriteToTargets(LogLevel.Warn, formatProvider, message, new object[] { argument }, null); 
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Warn</c> level using the specified <see cref="T:System.Byte" /> as a parameter.
        /// </summary>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.Byte" /> argument to format.</param>
        public void Warn(string message, System.Byte argument) { 
            if (IsWarnEnabled)
                WriteToTargets(LogLevel.Warn, message, new object[] { argument });
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Warn</c> level using the specified <see cref="T:System.String" /> as a parameter and formatting it with the supplied format provider.
        /// </summary>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.String" /> argument to format.</param>
        public void Warn(IFormatProvider formatProvider, string message, System.String argument) { 
            if (IsWarnEnabled)
                WriteToTargets(LogLevel.Warn, formatProvider, message, new object[] { argument }, null); 
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Warn</c> level using the specified <see cref="T:System.String" /> as a parameter.
        /// </summary>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.String" /> argument to format.</param>
        public void Warn(string message, System.String argument) { 
            if (IsWarnEnabled)
                WriteToTargets(LogLevel.Warn, message, new object[] { argument });
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Warn</c> level using the specified <see cref="T:System.Int32" /> as a parameter and formatting it with the supplied format provider.
        /// </summary>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.Int32" /> argument to format.</param>
        public void Warn(IFormatProvider formatProvider, string message, System.Int32 argument) { 
            if (IsWarnEnabled)
                WriteToTargets(LogLevel.Warn, formatProvider, message, new object[] { argument }, null); 
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Warn</c> level using the specified <see cref="T:System.Int32" /> as a parameter.
        /// </summary>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.Int32" /> argument to format.</param>
        public void Warn(string message, System.Int32 argument) { 
            if (IsWarnEnabled)
                WriteToTargets(LogLevel.Warn, message, new object[] { argument });
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Warn</c> level using the specified <see cref="T:System.Int64" /> as a parameter and formatting it with the supplied format provider.
        /// </summary>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.Int64" /> argument to format.</param>
        public void Warn(IFormatProvider formatProvider, string message, System.Int64 argument) { 
            if (IsWarnEnabled)
                WriteToTargets(LogLevel.Warn, formatProvider, message, new object[] { argument }, null); 
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Warn</c> level using the specified <see cref="T:System.Int64" /> as a parameter.
        /// </summary>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.Int64" /> argument to format.</param>
        public void Warn(string message, System.Int64 argument) { 
            if (IsWarnEnabled)
                WriteToTargets(LogLevel.Warn, message, new object[] { argument });
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Warn</c> level using the specified <see cref="T:System.Single" /> as a parameter and formatting it with the supplied format provider.
        /// </summary>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.Single" /> argument to format.</param>
        public void Warn(IFormatProvider formatProvider, string message, System.Single argument) { 
            if (IsWarnEnabled)
                WriteToTargets(LogLevel.Warn, formatProvider, message, new object[] { argument }, null); 
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Warn</c> level using the specified <see cref="T:System.Single" /> as a parameter.
        /// </summary>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.Single" /> argument to format.</param>
        public void Warn(string message, System.Single argument) { 
            if (IsWarnEnabled)
                WriteToTargets(LogLevel.Warn, message, new object[] { argument });
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Warn</c> level using the specified <see cref="T:System.Double" /> as a parameter and formatting it with the supplied format provider.
        /// </summary>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.Double" /> argument to format.</param>
        public void Warn(IFormatProvider formatProvider, string message, System.Double argument) { 
            if (IsWarnEnabled)
                WriteToTargets(LogLevel.Warn, formatProvider, message, new object[] { argument }, null); 
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Warn</c> level using the specified <see cref="T:System.Double" /> as a parameter.
        /// </summary>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.Double" /> argument to format.</param>
        public void Warn(string message, System.Double argument) { 
            if (IsWarnEnabled)
                WriteToTargets(LogLevel.Warn, message, new object[] { argument });
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Warn</c> level using the specified <see cref="T:System.Decimal" /> as a parameter and formatting it with the supplied format provider.
        /// </summary>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.Decimal" /> argument to format.</param>
        public void Warn(IFormatProvider formatProvider, string message, System.Decimal argument) { 
            if (IsWarnEnabled)
                WriteToTargets(LogLevel.Warn, formatProvider, message, new object[] { argument }, null); 
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Warn</c> level using the specified <see cref="T:System.Decimal" /> as a parameter.
        /// </summary>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.Decimal" /> argument to format.</param>
        public void Warn(string message, System.Decimal argument) { 
            if (IsWarnEnabled)
                WriteToTargets(LogLevel.Warn, message, new object[] { argument });
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Warn</c> level using the specified <see cref="T:System.Object" /> as a parameter and formatting it with the supplied format provider.
        /// </summary>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.Object" /> argument to format.</param>
        public void Warn(IFormatProvider formatProvider, string message, System.Object argument) { 
            if (IsWarnEnabled)
                WriteToTargets(LogLevel.Warn, formatProvider, message, new object[] { argument }, null); 
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Warn</c> level using the specified <see cref="T:System.Object" /> as a parameter.
        /// </summary>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.Object" /> argument to format.</param>
        public void Warn(string message, System.Object argument) { 
            if (IsWarnEnabled)
                WriteToTargets(LogLevel.Warn, message, new object[] { argument });
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Warn</c> level using the specified <see cref="T:System.SByte" /> as a parameter and formatting it with the supplied format provider.
        /// </summary>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.SByte" /> argument to format.</param>
        [CLSCompliant(false)]
        public void Warn(IFormatProvider formatProvider, string message, System.SByte argument) { 
            if (IsWarnEnabled)
                WriteToTargets(LogLevel.Warn, formatProvider, message, new object[] { argument }, null); 
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Warn</c> level using the specified <see cref="T:System.SByte" /> as a parameter.
        /// </summary>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.SByte" /> argument to format.</param>
        [CLSCompliant(false)]
        public void Warn(string message, System.SByte argument) { 
            if (IsWarnEnabled)
                WriteToTargets(LogLevel.Warn, message, new object[] { argument });
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Warn</c> level using the specified <see cref="T:System.UInt32" /> as a parameter and formatting it with the supplied format provider.
        /// </summary>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.UInt32" /> argument to format.</param>
        [CLSCompliant(false)]
        public void Warn(IFormatProvider formatProvider, string message, System.UInt32 argument) { 
            if (IsWarnEnabled)
                WriteToTargets(LogLevel.Warn, formatProvider, message, new object[] { argument }, null); 
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Warn</c> level using the specified <see cref="T:System.UInt32" /> as a parameter.
        /// </summary>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.UInt32" /> argument to format.</param>
        [CLSCompliant(false)]
        public void Warn(string message, System.UInt32 argument) { 
            if (IsWarnEnabled)
                WriteToTargets(LogLevel.Warn, message, new object[] { argument });
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Warn</c> level using the specified <see cref="T:System.UInt64" /> as a parameter and formatting it with the supplied format provider.
        /// </summary>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.UInt64" /> argument to format.</param>
        [CLSCompliant(false)]
        public void Warn(IFormatProvider formatProvider, string message, System.UInt64 argument) { 
            if (IsWarnEnabled)
                WriteToTargets(LogLevel.Warn, formatProvider, message, new object[] { argument }, null); 
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Warn</c> level using the specified <see cref="T:System.UInt64" /> as a parameter.
        /// </summary>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.UInt64" /> argument to format.</param>
        [CLSCompliant(false)]
        public void Warn(string message, System.UInt64 argument) { 
            if (IsWarnEnabled)
                WriteToTargets(LogLevel.Warn, message, new object[] { argument });
        }

        #endregion


        #region Error() overloads 

        /// <overloads>
        /// Writes the diagnostic message at the <c>Error</c> level using the specified format provider and format parameters.
        /// </overloads>
        /// <summary>
        /// Writes the diagnostic message at the <c>Error</c> level.
        /// </summary>
        /// <param name="message">A <see langword="string" /> to be written.</param>
        public void Error(string message) {
            if (IsErrorEnabled)
                WriteToTargets(LogLevel.Error, message);
        }

        /// <summary>
        /// Writes the diagnostic message and exception at the <c>Error</c> level.
        /// </summary>
        /// <param name="message">A <see langword="string" /> to be written.</param>
        /// <param name="exception">An exception to be logged.</param>
        public void ErrorException(string message, Exception exception) {
            if (IsErrorEnabled)
                WriteToTargets(LogLevel.Error, null, message, null, exception);
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Error</c> level using the specified parameters and formatting them with the supplied format provider.
        /// </summary>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing format items.</param>
        /// <param name="args">Arguments to format.</param>
        public void Error(IFormatProvider formatProvider, string message, params object[] args) { 
            if (IsErrorEnabled)
                WriteToTargets(LogLevel.Error, formatProvider, message, args, null); 
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Error</c> level using the specified parameters.
        /// </summary>
        /// <param name="message">A <see langword="string" /> containing format items.</param>
        /// <param name="args">Arguments to format.</param>
        public void Error(string message, params object[] args) { 
            if (IsErrorEnabled)
                WriteToTargets(LogLevel.Error, message, args);
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Error</c> level using the specified parameters.
        /// </summary>
        /// <param name="message">A <see langword="string" /> containing format items.</param>
        /// <param name="arg1">First argument to format.</param>
        /// <param name="arg2">Second argument to format.</param>
        public void Error(string message, System.Object arg1, System.Object arg2) { 
            if (IsErrorEnabled)
                WriteToTargets(LogLevel.Error, message, new object[] { arg1, arg2 });
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Error</c> level using the specified parameters.
        /// </summary>
        /// <param name="message">A <see langword="string" /> containing format items.</param>
        /// <param name="arg1">First argument to format.</param>
        /// <param name="arg2">Second argument to format.</param>
        /// <param name="arg3">Third argument to format.</param>
        public void Error(string message, System.Object arg1, System.Object arg2, System.Object arg3) { 
            if (IsErrorEnabled)
                WriteToTargets(LogLevel.Error, message, new object[] { arg1, arg2, arg3 });
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Error</c> level using the specified <see cref="T:System.Boolean" /> as a parameter and formatting it with the supplied format provider.
        /// </summary>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.Boolean" /> argument to format.</param>
        public void Error(IFormatProvider formatProvider, string message, System.Boolean argument) { 
            if (IsErrorEnabled)
                WriteToTargets(LogLevel.Error, formatProvider, message, new object[] { argument }, null); 
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Error</c> level using the specified <see cref="T:System.Boolean" /> as a parameter.
        /// </summary>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.Boolean" /> argument to format.</param>
        public void Error(string message, System.Boolean argument) { 
            if (IsErrorEnabled)
                WriteToTargets(LogLevel.Error, message, new object[] { argument });
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Error</c> level using the specified <see cref="T:System.Char" /> as a parameter and formatting it with the supplied format provider.
        /// </summary>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.Char" /> argument to format.</param>
        public void Error(IFormatProvider formatProvider, string message, System.Char argument) { 
            if (IsErrorEnabled)
                WriteToTargets(LogLevel.Error, formatProvider, message, new object[] { argument }, null); 
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Error</c> level using the specified <see cref="T:System.Char" /> as a parameter.
        /// </summary>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.Char" /> argument to format.</param>
        public void Error(string message, System.Char argument) { 
            if (IsErrorEnabled)
                WriteToTargets(LogLevel.Error, message, new object[] { argument });
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Error</c> level using the specified <see cref="T:System.Byte" /> as a parameter and formatting it with the supplied format provider.
        /// </summary>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.Byte" /> argument to format.</param>
        public void Error(IFormatProvider formatProvider, string message, System.Byte argument) { 
            if (IsErrorEnabled)
                WriteToTargets(LogLevel.Error, formatProvider, message, new object[] { argument }, null); 
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Error</c> level using the specified <see cref="T:System.Byte" /> as a parameter.
        /// </summary>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.Byte" /> argument to format.</param>
        public void Error(string message, System.Byte argument) { 
            if (IsErrorEnabled)
                WriteToTargets(LogLevel.Error, message, new object[] { argument });
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Error</c> level using the specified <see cref="T:System.String" /> as a parameter and formatting it with the supplied format provider.
        /// </summary>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.String" /> argument to format.</param>
        public void Error(IFormatProvider formatProvider, string message, System.String argument) { 
            if (IsErrorEnabled)
                WriteToTargets(LogLevel.Error, formatProvider, message, new object[] { argument }, null); 
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Error</c> level using the specified <see cref="T:System.String" /> as a parameter.
        /// </summary>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.String" /> argument to format.</param>
        public void Error(string message, System.String argument) { 
            if (IsErrorEnabled)
                WriteToTargets(LogLevel.Error, message, new object[] { argument });
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Error</c> level using the specified <see cref="T:System.Int32" /> as a parameter and formatting it with the supplied format provider.
        /// </summary>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.Int32" /> argument to format.</param>
        public void Error(IFormatProvider formatProvider, string message, System.Int32 argument) { 
            if (IsErrorEnabled)
                WriteToTargets(LogLevel.Error, formatProvider, message, new object[] { argument }, null); 
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Error</c> level using the specified <see cref="T:System.Int32" /> as a parameter.
        /// </summary>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.Int32" /> argument to format.</param>
        public void Error(string message, System.Int32 argument) { 
            if (IsErrorEnabled)
                WriteToTargets(LogLevel.Error, message, new object[] { argument });
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Error</c> level using the specified <see cref="T:System.Int64" /> as a parameter and formatting it with the supplied format provider.
        /// </summary>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.Int64" /> argument to format.</param>
        public void Error(IFormatProvider formatProvider, string message, System.Int64 argument) { 
            if (IsErrorEnabled)
                WriteToTargets(LogLevel.Error, formatProvider, message, new object[] { argument }, null); 
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Error</c> level using the specified <see cref="T:System.Int64" /> as a parameter.
        /// </summary>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.Int64" /> argument to format.</param>
        public void Error(string message, System.Int64 argument) { 
            if (IsErrorEnabled)
                WriteToTargets(LogLevel.Error, message, new object[] { argument });
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Error</c> level using the specified <see cref="T:System.Single" /> as a parameter and formatting it with the supplied format provider.
        /// </summary>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.Single" /> argument to format.</param>
        public void Error(IFormatProvider formatProvider, string message, System.Single argument) { 
            if (IsErrorEnabled)
                WriteToTargets(LogLevel.Error, formatProvider, message, new object[] { argument }, null); 
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Error</c> level using the specified <see cref="T:System.Single" /> as a parameter.
        /// </summary>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.Single" /> argument to format.</param>
        public void Error(string message, System.Single argument) { 
            if (IsErrorEnabled)
                WriteToTargets(LogLevel.Error, message, new object[] { argument });
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Error</c> level using the specified <see cref="T:System.Double" /> as a parameter and formatting it with the supplied format provider.
        /// </summary>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.Double" /> argument to format.</param>
        public void Error(IFormatProvider formatProvider, string message, System.Double argument) { 
            if (IsErrorEnabled)
                WriteToTargets(LogLevel.Error, formatProvider, message, new object[] { argument }, null); 
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Error</c> level using the specified <see cref="T:System.Double" /> as a parameter.
        /// </summary>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.Double" /> argument to format.</param>
        public void Error(string message, System.Double argument) { 
            if (IsErrorEnabled)
                WriteToTargets(LogLevel.Error, message, new object[] { argument });
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Error</c> level using the specified <see cref="T:System.Decimal" /> as a parameter and formatting it with the supplied format provider.
        /// </summary>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.Decimal" /> argument to format.</param>
        public void Error(IFormatProvider formatProvider, string message, System.Decimal argument) { 
            if (IsErrorEnabled)
                WriteToTargets(LogLevel.Error, formatProvider, message, new object[] { argument }, null); 
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Error</c> level using the specified <see cref="T:System.Decimal" /> as a parameter.
        /// </summary>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.Decimal" /> argument to format.</param>
        public void Error(string message, System.Decimal argument) { 
            if (IsErrorEnabled)
                WriteToTargets(LogLevel.Error, message, new object[] { argument });
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Error</c> level using the specified <see cref="T:System.Object" /> as a parameter and formatting it with the supplied format provider.
        /// </summary>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.Object" /> argument to format.</param>
        public void Error(IFormatProvider formatProvider, string message, System.Object argument) { 
            if (IsErrorEnabled)
                WriteToTargets(LogLevel.Error, formatProvider, message, new object[] { argument }, null); 
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Error</c> level using the specified <see cref="T:System.Object" /> as a parameter.
        /// </summary>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.Object" /> argument to format.</param>
        public void Error(string message, System.Object argument) { 
            if (IsErrorEnabled)
                WriteToTargets(LogLevel.Error, message, new object[] { argument });
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Error</c> level using the specified <see cref="T:System.SByte" /> as a parameter and formatting it with the supplied format provider.
        /// </summary>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.SByte" /> argument to format.</param>
        [CLSCompliant(false)]
        public void Error(IFormatProvider formatProvider, string message, System.SByte argument) { 
            if (IsErrorEnabled)
                WriteToTargets(LogLevel.Error, formatProvider, message, new object[] { argument }, null); 
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Error</c> level using the specified <see cref="T:System.SByte" /> as a parameter.
        /// </summary>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.SByte" /> argument to format.</param>
        [CLSCompliant(false)]
        public void Error(string message, System.SByte argument) { 
            if (IsErrorEnabled)
                WriteToTargets(LogLevel.Error, message, new object[] { argument });
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Error</c> level using the specified <see cref="T:System.UInt32" /> as a parameter and formatting it with the supplied format provider.
        /// </summary>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.UInt32" /> argument to format.</param>
        [CLSCompliant(false)]
        public void Error(IFormatProvider formatProvider, string message, System.UInt32 argument) { 
            if (IsErrorEnabled)
                WriteToTargets(LogLevel.Error, formatProvider, message, new object[] { argument }, null); 
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Error</c> level using the specified <see cref="T:System.UInt32" /> as a parameter.
        /// </summary>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.UInt32" /> argument to format.</param>
        [CLSCompliant(false)]
        public void Error(string message, System.UInt32 argument) { 
            if (IsErrorEnabled)
                WriteToTargets(LogLevel.Error, message, new object[] { argument });
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Error</c> level using the specified <see cref="T:System.UInt64" /> as a parameter and formatting it with the supplied format provider.
        /// </summary>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.UInt64" /> argument to format.</param>
        [CLSCompliant(false)]
        public void Error(IFormatProvider formatProvider, string message, System.UInt64 argument) { 
            if (IsErrorEnabled)
                WriteToTargets(LogLevel.Error, formatProvider, message, new object[] { argument }, null); 
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Error</c> level using the specified <see cref="T:System.UInt64" /> as a parameter.
        /// </summary>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.UInt64" /> argument to format.</param>
        [CLSCompliant(false)]
        public void Error(string message, System.UInt64 argument) { 
            if (IsErrorEnabled)
                WriteToTargets(LogLevel.Error, message, new object[] { argument });
        }

        #endregion


        #region Fatal() overloads 

        /// <overloads>
        /// Writes the diagnostic message at the <c>Fatal</c> level using the specified format provider and format parameters.
        /// </overloads>
        /// <summary>
        /// Writes the diagnostic message at the <c>Fatal</c> level.
        /// </summary>
        /// <param name="message">A <see langword="string" /> to be written.</param>
        public void Fatal(string message) {
            if (IsFatalEnabled)
                WriteToTargets(LogLevel.Fatal, message);
        }

        /// <summary>
        /// Writes the diagnostic message and exception at the <c>Fatal</c> level.
        /// </summary>
        /// <param name="message">A <see langword="string" /> to be written.</param>
        /// <param name="exception">An exception to be logged.</param>
        public void FatalException(string message, Exception exception) {
            if (IsFatalEnabled)
                WriteToTargets(LogLevel.Fatal, null, message, null, exception);
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Fatal</c> level using the specified parameters and formatting them with the supplied format provider.
        /// </summary>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing format items.</param>
        /// <param name="args">Arguments to format.</param>
        public void Fatal(IFormatProvider formatProvider, string message, params object[] args) { 
            if (IsFatalEnabled)
                WriteToTargets(LogLevel.Fatal, formatProvider, message, args, null); 
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Fatal</c> level using the specified parameters.
        /// </summary>
        /// <param name="message">A <see langword="string" /> containing format items.</param>
        /// <param name="args">Arguments to format.</param>
        public void Fatal(string message, params object[] args) { 
            if (IsFatalEnabled)
                WriteToTargets(LogLevel.Fatal, message, args);
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Fatal</c> level using the specified parameters.
        /// </summary>
        /// <param name="message">A <see langword="string" /> containing format items.</param>
        /// <param name="arg1">First argument to format.</param>
        /// <param name="arg2">Second argument to format.</param>
        public void Fatal(string message, System.Object arg1, System.Object arg2) { 
            if (IsFatalEnabled)
                WriteToTargets(LogLevel.Fatal, message, new object[] { arg1, arg2 });
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Fatal</c> level using the specified parameters.
        /// </summary>
        /// <param name="message">A <see langword="string" /> containing format items.</param>
        /// <param name="arg1">First argument to format.</param>
        /// <param name="arg2">Second argument to format.</param>
        /// <param name="arg3">Third argument to format.</param>
        public void Fatal(string message, System.Object arg1, System.Object arg2, System.Object arg3) { 
            if (IsFatalEnabled)
                WriteToTargets(LogLevel.Fatal, message, new object[] { arg1, arg2, arg3 });
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Fatal</c> level using the specified <see cref="T:System.Boolean" /> as a parameter and formatting it with the supplied format provider.
        /// </summary>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.Boolean" /> argument to format.</param>
        public void Fatal(IFormatProvider formatProvider, string message, System.Boolean argument) { 
            if (IsFatalEnabled)
                WriteToTargets(LogLevel.Fatal, formatProvider, message, new object[] { argument }, null); 
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Fatal</c> level using the specified <see cref="T:System.Boolean" /> as a parameter.
        /// </summary>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.Boolean" /> argument to format.</param>
        public void Fatal(string message, System.Boolean argument) { 
            if (IsFatalEnabled)
                WriteToTargets(LogLevel.Fatal, message, new object[] { argument });
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Fatal</c> level using the specified <see cref="T:System.Char" /> as a parameter and formatting it with the supplied format provider.
        /// </summary>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.Char" /> argument to format.</param>
        public void Fatal(IFormatProvider formatProvider, string message, System.Char argument) { 
            if (IsFatalEnabled)
                WriteToTargets(LogLevel.Fatal, formatProvider, message, new object[] { argument }, null); 
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Fatal</c> level using the specified <see cref="T:System.Char" /> as a parameter.
        /// </summary>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.Char" /> argument to format.</param>
        public void Fatal(string message, System.Char argument) { 
            if (IsFatalEnabled)
                WriteToTargets(LogLevel.Fatal, message, new object[] { argument });
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Fatal</c> level using the specified <see cref="T:System.Byte" /> as a parameter and formatting it with the supplied format provider.
        /// </summary>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.Byte" /> argument to format.</param>
        public void Fatal(IFormatProvider formatProvider, string message, System.Byte argument) { 
            if (IsFatalEnabled)
                WriteToTargets(LogLevel.Fatal, formatProvider, message, new object[] { argument }, null); 
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Fatal</c> level using the specified <see cref="T:System.Byte" /> as a parameter.
        /// </summary>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.Byte" /> argument to format.</param>
        public void Fatal(string message, System.Byte argument) { 
            if (IsFatalEnabled)
                WriteToTargets(LogLevel.Fatal, message, new object[] { argument });
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Fatal</c> level using the specified <see cref="T:System.String" /> as a parameter and formatting it with the supplied format provider.
        /// </summary>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.String" /> argument to format.</param>
        public void Fatal(IFormatProvider formatProvider, string message, System.String argument) { 
            if (IsFatalEnabled)
                WriteToTargets(LogLevel.Fatal, formatProvider, message, new object[] { argument }, null); 
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Fatal</c> level using the specified <see cref="T:System.String" /> as a parameter.
        /// </summary>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.String" /> argument to format.</param>
        public void Fatal(string message, System.String argument) { 
            if (IsFatalEnabled)
                WriteToTargets(LogLevel.Fatal, message, new object[] { argument });
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Fatal</c> level using the specified <see cref="T:System.Int32" /> as a parameter and formatting it with the supplied format provider.
        /// </summary>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.Int32" /> argument to format.</param>
        public void Fatal(IFormatProvider formatProvider, string message, System.Int32 argument) { 
            if (IsFatalEnabled)
                WriteToTargets(LogLevel.Fatal, formatProvider, message, new object[] { argument }, null); 
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Fatal</c> level using the specified <see cref="T:System.Int32" /> as a parameter.
        /// </summary>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.Int32" /> argument to format.</param>
        public void Fatal(string message, System.Int32 argument) { 
            if (IsFatalEnabled)
                WriteToTargets(LogLevel.Fatal, message, new object[] { argument });
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Fatal</c> level using the specified <see cref="T:System.Int64" /> as a parameter and formatting it with the supplied format provider.
        /// </summary>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.Int64" /> argument to format.</param>
        public void Fatal(IFormatProvider formatProvider, string message, System.Int64 argument) { 
            if (IsFatalEnabled)
                WriteToTargets(LogLevel.Fatal, formatProvider, message, new object[] { argument }, null); 
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Fatal</c> level using the specified <see cref="T:System.Int64" /> as a parameter.
        /// </summary>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.Int64" /> argument to format.</param>
        public void Fatal(string message, System.Int64 argument) { 
            if (IsFatalEnabled)
                WriteToTargets(LogLevel.Fatal, message, new object[] { argument });
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Fatal</c> level using the specified <see cref="T:System.Single" /> as a parameter and formatting it with the supplied format provider.
        /// </summary>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.Single" /> argument to format.</param>
        public void Fatal(IFormatProvider formatProvider, string message, System.Single argument) { 
            if (IsFatalEnabled)
                WriteToTargets(LogLevel.Fatal, formatProvider, message, new object[] { argument }, null); 
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Fatal</c> level using the specified <see cref="T:System.Single" /> as a parameter.
        /// </summary>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.Single" /> argument to format.</param>
        public void Fatal(string message, System.Single argument) { 
            if (IsFatalEnabled)
                WriteToTargets(LogLevel.Fatal, message, new object[] { argument });
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Fatal</c> level using the specified <see cref="T:System.Double" /> as a parameter and formatting it with the supplied format provider.
        /// </summary>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.Double" /> argument to format.</param>
        public void Fatal(IFormatProvider formatProvider, string message, System.Double argument) { 
            if (IsFatalEnabled)
                WriteToTargets(LogLevel.Fatal, formatProvider, message, new object[] { argument }, null); 
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Fatal</c> level using the specified <see cref="T:System.Double" /> as a parameter.
        /// </summary>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.Double" /> argument to format.</param>
        public void Fatal(string message, System.Double argument) { 
            if (IsFatalEnabled)
                WriteToTargets(LogLevel.Fatal, message, new object[] { argument });
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Fatal</c> level using the specified <see cref="T:System.Decimal" /> as a parameter and formatting it with the supplied format provider.
        /// </summary>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.Decimal" /> argument to format.</param>
        public void Fatal(IFormatProvider formatProvider, string message, System.Decimal argument) { 
            if (IsFatalEnabled)
                WriteToTargets(LogLevel.Fatal, formatProvider, message, new object[] { argument }, null); 
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Fatal</c> level using the specified <see cref="T:System.Decimal" /> as a parameter.
        /// </summary>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.Decimal" /> argument to format.</param>
        public void Fatal(string message, System.Decimal argument) { 
            if (IsFatalEnabled)
                WriteToTargets(LogLevel.Fatal, message, new object[] { argument });
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Fatal</c> level using the specified <see cref="T:System.Object" /> as a parameter and formatting it with the supplied format provider.
        /// </summary>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.Object" /> argument to format.</param>
        public void Fatal(IFormatProvider formatProvider, string message, System.Object argument) { 
            if (IsFatalEnabled)
                WriteToTargets(LogLevel.Fatal, formatProvider, message, new object[] { argument }, null); 
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Fatal</c> level using the specified <see cref="T:System.Object" /> as a parameter.
        /// </summary>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.Object" /> argument to format.</param>
        public void Fatal(string message, System.Object argument) { 
            if (IsFatalEnabled)
                WriteToTargets(LogLevel.Fatal, message, new object[] { argument });
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Fatal</c> level using the specified <see cref="T:System.SByte" /> as a parameter and formatting it with the supplied format provider.
        /// </summary>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.SByte" /> argument to format.</param>
        [CLSCompliant(false)]
        public void Fatal(IFormatProvider formatProvider, string message, System.SByte argument) { 
            if (IsFatalEnabled)
                WriteToTargets(LogLevel.Fatal, formatProvider, message, new object[] { argument }, null); 
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Fatal</c> level using the specified <see cref="T:System.SByte" /> as a parameter.
        /// </summary>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.SByte" /> argument to format.</param>
        [CLSCompliant(false)]
        public void Fatal(string message, System.SByte argument) { 
            if (IsFatalEnabled)
                WriteToTargets(LogLevel.Fatal, message, new object[] { argument });
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Fatal</c> level using the specified <see cref="T:System.UInt32" /> as a parameter and formatting it with the supplied format provider.
        /// </summary>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.UInt32" /> argument to format.</param>
        [CLSCompliant(false)]
        public void Fatal(IFormatProvider formatProvider, string message, System.UInt32 argument) { 
            if (IsFatalEnabled)
                WriteToTargets(LogLevel.Fatal, formatProvider, message, new object[] { argument }, null); 
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Fatal</c> level using the specified <see cref="T:System.UInt32" /> as a parameter.
        /// </summary>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.UInt32" /> argument to format.</param>
        [CLSCompliant(false)]
        public void Fatal(string message, System.UInt32 argument) { 
            if (IsFatalEnabled)
                WriteToTargets(LogLevel.Fatal, message, new object[] { argument });
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Fatal</c> level using the specified <see cref="T:System.UInt64" /> as a parameter and formatting it with the supplied format provider.
        /// </summary>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.UInt64" /> argument to format.</param>
        [CLSCompliant(false)]
        public void Fatal(IFormatProvider formatProvider, string message, System.UInt64 argument) { 
            if (IsFatalEnabled)
                WriteToTargets(LogLevel.Fatal, formatProvider, message, new object[] { argument }, null); 
        }
        /// <summary>
        /// Writes the diagnostic message at the <c>Fatal</c> level using the specified <see cref="T:System.UInt64" /> as a parameter.
        /// </summary>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The <see cref="T:System.UInt64" /> argument to format.</param>
        [CLSCompliant(false)]
        public void Fatal(string message, System.UInt64 argument) { 
            if (IsFatalEnabled)
                WriteToTargets(LogLevel.Fatal, message, new object[] { argument });
        }

        #endregion

     // end of generated code
    }
}
