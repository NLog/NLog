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

namespace NLog.Common
{
    using JetBrains.Annotations;
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using NLog.Internal;
    using NLog.Time;
    using NLog.Targets;

    /// <summary>
    /// NLog internal logger.
    /// 
    /// Writes to file, console or custom text writer (see <see cref="InternalLogger.LogWriter"/>)
    /// </summary>
    /// <remarks>
    /// Don't use <see cref="ExceptionHelper.MustBeRethrown"/> as that can lead to recursive calls - stackoverflow
    /// </remarks>
    public static partial class InternalLogger
    {
        private static readonly object LockObject = new object();
        private static string _logFile;

       /// <summary>
        /// Set the config of the InternalLogger with defaults and config.
        /// </summary>
        public static void Reset()
        {
            // TODO: Extract class - InternalLoggerConfigurationReader

            LogToConsole = GetSetting("nlog.internalLogToConsole", "NLOG_INTERNAL_LOG_TO_CONSOLE", false);
            LogToConsoleError = GetSetting("nlog.internalLogToConsoleError", "NLOG_INTERNAL_LOG_TO_CONSOLE_ERROR", false);
            LogLevel = GetSetting("nlog.internalLogLevel", "NLOG_INTERNAL_LOG_LEVEL", LogLevel.Info);
            LogFile = GetSetting("nlog.internalLogFile", "NLOG_INTERNAL_LOG_FILE", string.Empty);
            LogToTrace = GetSetting("nlog.internalLogToTrace", "NLOG_INTERNAL_LOG_TO_TRACE", false);
            IncludeTimestamp = GetSetting("nlog.internalLogIncludeTimestamp", "NLOG_INTERNAL_INCLUDE_TIMESTAMP", true);
            Info("NLog internal logger initialized.");
     
            ExceptionThrowWhenWriting = false;
            LogWriter = null;
            LogMessageReceived = null;
        }

        /// <summary>
        /// Gets or sets the minimal internal log level. 
        /// </summary>
        /// <example>If set to <see cref="NLog.LogLevel.Info"/>, then messages of the levels <see cref="NLog.LogLevel.Info"/>, <see cref="NLog.LogLevel.Error"/> and <see cref="NLog.LogLevel.Fatal"/> will be written.</example>
        public static LogLevel LogLevel { get => _logLevel; set => _logLevel = value ?? LogLevel.Info; }
        private static LogLevel _logLevel;

        /// <summary>
        /// Gets or sets a value indicating whether internal messages should be written to the console output stream.
        /// </summary>
        /// <remarks>Your application must be a console application.</remarks>
        public static bool LogToConsole { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether internal messages should be written to the console error stream.
        /// </summary>
        /// <remarks>Your application must be a console application.</remarks>
        public static bool LogToConsoleError { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether internal messages should be written to the <see cref="System.Diagnostics"/>.Trace
        /// </summary>
        public static bool LogToTrace { get; set; }

        /// <summary>
        /// Gets or sets the file path of the internal log file.
        /// </summary>
        /// <remarks>A value of <see langword="null" /> value disables internal logging to a file.</remarks>
        public static string LogFile
        {
            get
            {
                return _logFile;
            }

            set
            {
                _logFile = value;
                if (!string.IsNullOrEmpty(_logFile))
                {
                    CreateDirectoriesIfNeeded(_logFile);
                }
            }
        }

        /// <summary>
        /// Gets or sets the text writer that will receive internal logs.
        /// </summary>
        public static TextWriter LogWriter { get; set; }

        /// <summary>
        /// Event written to the internal log.
        /// Please note that the event is not triggered when then event hasn't the minimal log level set by <see cref="LogLevel"/> 
        /// </summary>
        public static event EventHandler<InternalLoggerMessageEventArgs> LogMessageReceived;

        /// <summary>
        /// Gets or sets a value indicating whether timestamp should be included in internal log output.
        /// </summary>
        public static bool IncludeTimestamp { get; set; }

        /// <summary>
        /// Is there an <see cref="Exception"/> thrown when writing the message?
        /// </summary>
        internal static bool ExceptionThrowWhenWriting { get; private set; }

        /// <summary>
        /// Logs the specified message without an <see cref="Exception"/> at the specified level.
        /// </summary>
        /// <param name="level">Log level.</param>
        /// <param name="message">Message which may include positional parameters.</param>
        /// <param name="args">Arguments to the message.</param>
        [StringFormatMethod("message")]
        public static void Log(LogLevel level, [Localizable(false)] string message, params object[] args)
        {
            Write(null, level, message, args);
        }

        /// <summary>
        /// Logs the specified message without an <see cref="Exception"/> at the specified level.
        /// </summary>
        /// <param name="level">Log level.</param>
        /// <param name="message">Log message.</param>
        public static void Log(LogLevel level, [Localizable(false)] string message)
        {
            Write(null, level, message, null);
        }

        /// <summary>
        /// Logs the specified message without an <see cref="Exception"/> at the specified level. 
        /// <paramref name="messageFunc"/> will be only called when logging is enabled for level <paramref name="level"/>.
        /// </summary>
        /// <param name="level">Log level.</param>
        /// <param name="messageFunc">Function that returns the log message.</param>
        public static void Log(LogLevel level, [Localizable(false)] Func<string> messageFunc)
        {
            if (IsLogLevelEnabled(level))
            {
                Write(null, level, messageFunc(), null);
            }
        }

        /// <summary>
        /// Logs the specified message with an <see cref="Exception"/> at the specified level. 
        /// <paramref name="messageFunc"/> will be only called when logging is enabled for level <paramref name="level"/>.
        /// </summary>
        /// <param name="ex">Exception to be logged.</param>
        /// <param name="level">Log level.</param>
        /// <param name="messageFunc">Function that returns the log message.</param>
        public static void Log(Exception ex, LogLevel level, [Localizable(false)] Func<string> messageFunc)
        {
            if (IsLogLevelEnabled(level))
            {
                Write(ex, level, messageFunc(), null);
            }
        }

        /// <summary>
        /// Logs the specified message with an <see cref="Exception"/> at the specified level.
        /// </summary>
        /// <param name="ex">Exception to be logged.</param>
        /// <param name="level">Log level.</param>
        /// <param name="message">Message which may include positional parameters.</param>
        /// <param name="args">Arguments to the message.</param>
        [StringFormatMethod("message")]
        public static void Log(Exception ex, LogLevel level, [Localizable(false)] string message, params object[] args)
        {
            Write(ex, level, message, args);
        }

        /// <summary>
        /// Logs the specified message with an <see cref="Exception"/> at the specified level.
        /// </summary>
        /// <param name="ex">Exception to be logged.</param>
        /// <param name="level">Log level.</param>
        /// <param name="message">Log message.</param>
        public static void Log(Exception ex, LogLevel level, [Localizable(false)] string message)
        {
            Write(ex, level, message, null);
        }

        /// <summary>
        /// Write to internallogger.
        /// </summary>
        /// <param name="ex">optional exception to be logged.</param>
        /// <param name="level">level</param>
        /// <param name="message">message</param>
        /// <param name="args">optional args for <paramref name="message"/></param>
        private static void Write([CanBeNull]Exception ex, LogLevel level, string message, [CanBeNull]object[] args)
        {
            if (!IsLogLevelEnabled(level))
            {
                return;
            }

            if (IsSeriousException(ex))
            {
                //no logging!
                return;
            }

            var hasActiveLoggersWithLine = HasActiveLoggersWithLine();
            var hasEventListeners = HasEventListeners();
            if (!hasActiveLoggersWithLine && !hasEventListeners)
            {
                return;
            }

            try
            {
                var fullMessage = CreateFullMessage(message, args);

                if (hasActiveLoggersWithLine)
                {
                    WriteLogLine(ex, level, fullMessage);
                }

                if (hasEventListeners)
                {
                    var loggerContext = args?.Length > 0 ? args[0] as IInternalLoggerContext : null;
                    var loggerContextName = string.IsNullOrEmpty(loggerContext?.Name) ? loggerContext?.ToString() : loggerContext.Name;
                    LogMessageReceived?.Invoke(null, new InternalLoggerMessageEventArgs(fullMessage, level, ex, loggerContext?.GetType(), loggerContextName));

                    ex?.MarkAsLoggedToInternalLogger();
                }
            }
            catch (Exception exception)
            {
                ExceptionThrowWhenWriting = true;

                // no log looping.
                // we have no place to log the message to so we ignore it
                if (exception.MustBeRethrownImmediately())
                {
                    throw;
                }
            }
        }

        private static void WriteLogLine(Exception ex, LogLevel level, string message)
        {
            try
            {
                string line = CreateLogLine(ex, level, message);

                WriteToLogFile(line);
                WriteToTextWriter(line);

#if !NETSTANDARD1_3
                WriteToConsole(line);
                WriteToErrorConsole(line);
                WriteToTrace(line);
#endif

                ex?.MarkAsLoggedToInternalLogger();
            }
            catch (Exception exception)
            {
                ExceptionThrowWhenWriting = true;

                // no log looping.
                // we have no place to log the message to so we ignore it
                if (exception.MustBeRethrownImmediately())
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// Create log line with timestamp, exception message etc (if configured)
        /// </summary>
        private static string CreateLogLine([CanBeNull]Exception ex, LogLevel level, string fullMessage)
        {
            const string timeStampFormat = "yyyy-MM-dd HH:mm:ss.ffff";
            const string fieldSeparator = " ";

            if (IncludeTimestamp)
            {
                return string.Concat(
                    TimeSource.Current.Time.ToString(timeStampFormat, CultureInfo.InvariantCulture),
                    fieldSeparator,
                    level.ToString(),
                    fieldSeparator,
                    fullMessage,
                    ex != null ? " Exception: " : "",
                    ex?.ToString() ?? "");
            }
            else
            {
                return string.Concat(
                    level.ToString(),
                    fieldSeparator,
                    fullMessage,
                    ex != null ? " Exception: " : "",
                    ex?.ToString() ?? "");
            }
        }

        private static string CreateFullMessage(string message, object[] args)
        {
            var formattedMessage =
                (args == null) ? message : string.Format(CultureInfo.InvariantCulture, message, args);
            return formattedMessage;
        }

        /// <summary>
        /// Determine if logging should be avoided because of exception type. 
        /// </summary>
        /// <param name="exception">The exception to check.</param>
        /// <returns><c>true</c> if logging should be avoided; otherwise, <c>false</c>.</returns>
        private static bool IsSeriousException(Exception exception)
        {
            return exception != null && exception.MustBeRethrownImmediately();
        }

        /// <summary>
        /// Determine if logging is enabled for given LogLevel
        /// </summary>
        /// <param name="logLevel">The <see cref="LogLevel"/> for the log event.</param>
        /// <returns><c>true</c> if logging is enabled; otherwise, <c>false</c>.</returns>
        private static bool IsLogLevelEnabled(LogLevel logLevel)
        {
            return !ReferenceEquals(_logLevel, LogLevel.Off) && logLevel >= _logLevel;
        }

        /// <summary>
        /// Determine if logging is enabled.
        /// </summary>
        /// <returns><c>true</c> if logging is enabled; otherwise, <c>false</c>.</returns>
        internal static bool HasActiveLoggers()
        {
            return HasActiveLoggersWithLine() || HasEventListeners();
        }

        private static bool HasEventListeners()
        {
            return LogMessageReceived != null;
        }

        internal static bool HasActiveLoggersWithLine()
        {
            return !string.IsNullOrEmpty(LogFile) ||
                   LogToConsole ||
                   LogToConsoleError ||
                   LogToTrace ||
                   LogWriter != null;
        }

        /// <summary>
        /// Write internal messages to the log file defined in <see cref="LogFile"/>.
        /// </summary>
        /// <param name="message">Message to write.</param>
        /// <remarks>
        /// Message will be logged only when the property <see cref="LogFile"/> is not <c>null</c>, otherwise the
        /// method has no effect.
        /// </remarks>
        private static void WriteToLogFile(string message)
        {
            var logFile = LogFile;
            if (string.IsNullOrEmpty(logFile))
            {
                return;
            }

            lock (LockObject)
            {
                using (var textWriter = File.AppendText(logFile))
                {
                    textWriter.WriteLine(message);
                }
            }
        }

        /// <summary>
        /// Write internal messages to the <see cref="System.IO.TextWriter"/> defined in <see cref="LogWriter"/>.
        /// </summary>
        /// <param name="message">Message to write.</param>
        /// <remarks>
        /// Message will be logged only when the property <see cref="LogWriter"/> is not <c>null</c>, otherwise the
        /// method has no effect.
        /// </remarks>
        private static void WriteToTextWriter(string message)
        {
            var writer = LogWriter;
            if (writer == null)
            {
                return;
            }

            lock (LockObject)
            {
                writer.WriteLine(message);
            }
        }

#if !NETSTANDARD1_3
        /// <summary>
        /// Write internal messages to the <see cref="System.Console"/>.
        /// </summary>
        /// <param name="message">Message to write.</param>
        /// <remarks>
        /// Message will be logged only when the property <see cref="LogToConsole"/> is <c>true</c>, otherwise the 
        /// method has no effect.
        /// </remarks>
        private static void WriteToConsole(string message)
        {
            if (!LogToConsole)
            {
                return;
            }

            NLog.Targets.ConsoleTargetHelper.WriteLineThreadSafe(Console.Out, message);
        }
#endif

#if !NETSTANDARD1_3
        /// <summary>
        /// Write internal messages to the <see cref="System.Console.Error"/>.
        /// </summary>
        /// <param name="message">Message to write.</param>
        /// <remarks>
        /// Message will be logged when the property <see cref="LogToConsoleError"/> is <c>true</c>, otherwise the 
        /// method has no effect.
        /// </remarks>
        private static void WriteToErrorConsole(string message)
        {
            if (!LogToConsoleError)
            {
                return;
            }

            NLog.Targets.ConsoleTargetHelper.WriteLineThreadSafe(Console.Error, message);
        }

        /// <summary>
        /// Write internal messages to the <see cref="System.Diagnostics.Trace"/>.
        /// </summary>
        /// <param name="message">A message to write.</param>
        /// <remarks>
        /// Works when property <see cref="LogToTrace"/> set to true.
        /// The <see cref="System.Diagnostics.Trace"/> is used in Debug and Release configuration. 
        /// The <see cref="System.Diagnostics.Debug"/> works only in Debug configuration and this is reason why is replaced by <see cref="System.Diagnostics.Trace"/>.
        /// in DEBUG 
        /// </remarks>
        private static void WriteToTrace(string message)
        {
            if (!LogToTrace)
            {
                return;
            }

            System.Diagnostics.Trace.WriteLine(message, "NLog");
        }
#endif

        /// <summary>
        /// Logs the assembly version and file version of the given Assembly.
        /// </summary>
        /// <param name="assembly">The assembly to log.</param>
        public static void LogAssemblyVersion(Assembly assembly)
        {
            try
            {
#if !NETSTANDARD1_3 && !NETSTANDARD1_5
                var fileVersionInfo = !string.IsNullOrEmpty(assembly.Location) ?
                    System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location) : null;
                Info("{0}. File version: {1}. Product version: {2}. GlobalAssemblyCache: {3}",
                    assembly.FullName,
                    fileVersionInfo?.FileVersion,
                    fileVersionInfo?.ProductVersion,
                    assembly.GlobalAssemblyCache);
#else
                Info(assembly.FullName);
#endif
            }
            catch (Exception ex)
            {
                Error(ex, "Error logging version of assembly {0}.", assembly.FullName);
            }
        }

        private static string GetAppSettings(string configName)
        {
#if !NETSTANDARD
            try
            {
                return System.Configuration.ConfigurationManager.AppSettings[configName];
            }
            catch (Exception ex)
            {
                if (ex.MustBeRethrownImmediately())
                {
                    throw;
                }
            }
#endif
            return null;
        }

        private static string GetSettingString(string configName, string envName)
        {
            try
            {
                string settingValue = GetAppSettings(configName);
                if (settingValue != null)
                    return settingValue;
            }
            catch (Exception ex)
            {
                if (ex.MustBeRethrownImmediately())
                {
                    throw;
                }
            }

            try
            {
                string settingValue = EnvironmentHelper.GetSafeEnvironmentVariable(envName);
                if (!string.IsNullOrEmpty(settingValue))
                    return settingValue;
            }
            catch (Exception ex)
            {
                if (ex.MustBeRethrownImmediately())
                {
                    throw;
                }
            }

            return null;
        }

        private static LogLevel GetSetting(string configName, string envName, LogLevel defaultValue)
        {
            string value = GetSettingString(configName, envName);
            if (value == null)
            {
                return defaultValue;
            }

            try
            {
                return LogLevel.FromString(value);
            }
            catch (Exception exception)
            {
                if (exception.MustBeRethrownImmediately())
                {
                    throw;
                }

                return defaultValue;
            }
        }

        private static T GetSetting<T>(string configName, string envName, T defaultValue)
        {
            string value = GetSettingString(configName, envName);
            if (value == null)
            {
                return defaultValue;
            }

            try
            {
                return (T)Convert.ChangeType(value, typeof(T), CultureInfo.InvariantCulture);
            }
            catch (Exception exception)
            {
                if (exception.MustBeRethrownImmediately())
                {
                    throw;
                }

                return defaultValue;
            }
        }

        private static void CreateDirectoriesIfNeeded(string filename)
        {
            try
            {
                if (LogLevel == LogLevel.Off)
                {
                    return;
                }

                string parentDirectory = Path.GetDirectoryName(filename);
                if (!string.IsNullOrEmpty(parentDirectory))
                {
                    Directory.CreateDirectory(parentDirectory);
                }
            }
            catch (Exception exception)
            {
                Error(exception, "Cannot create needed directories to '{0}'.", filename);

                if (exception.MustBeRethrownImmediately())
                {
                    throw;
                }
            }
        }
    }
}
