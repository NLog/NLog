// 
// Copyright (c) 2004-2019 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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
    using System.Text;
    using NLog.Internal;
    using NLog.Time;

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
        /// Initializes static members of the InternalLogger class.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline", Justification = "Significant logic in .cctor()")]
        static InternalLogger()
        {
            Reset();
        }

        /// <summary>
        /// Set the config of the InternalLogger with defaults and config.
        /// </summary>
        public static void Reset()
        {
            // TODO: Extract class - InternalLoggerConfigurationReader

#if !SILVERLIGHT && !__IOS__ && !__ANDROID__
            LogToConsole = GetSetting("nlog.internalLogToConsole", "NLOG_INTERNAL_LOG_TO_CONSOLE", false);
            LogToConsoleError = GetSetting("nlog.internalLogToConsoleError", "NLOG_INTERNAL_LOG_TO_CONSOLE_ERROR", false);
            LogLevel = GetSetting("nlog.internalLogLevel", "NLOG_INTERNAL_LOG_LEVEL", LogLevel.Info);
            LogFile = GetSetting("nlog.internalLogFile", "NLOG_INTERNAL_LOG_FILE", string.Empty);
            LogToTrace = GetSetting("nlog.internalLogToTrace", "NLOG_INTERNAL_LOG_TO_TRACE", false);
            IncludeTimestamp = GetSetting("nlog.internalLogIncludeTimestamp", "NLOG_INTERNAL_INCLUDE_TIMESTAMP", true);
            Info("NLog internal logger initialized.");
#else
            LogLevel = LogLevel.Info;
            LogToConsole = false;
            LogToConsoleError = false;
            LogFile = string.Empty;
            IncludeTimestamp = true;
#endif
            ExceptionThrowWhenWriting = false;
            LogWriter = null;
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

#if !SILVERLIGHT && !__IOS__ && !__ANDROID__
        /// <summary>
        /// Gets or sets a value indicating whether internal messages should be written to the <see cref="System.Diagnostics"/>.Trace
        /// </summary>
        public static bool LogToTrace { get; set; }
#endif

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

#if !SILVERLIGHT
                if (!string.IsNullOrEmpty(_logFile))
                {
                    CreateDirectoriesIfNeeded(_logFile);
                }
#endif
            }
        }

        /// <summary>
        /// Gets or sets the text writer that will receive internal logs.
        /// </summary>
        public static TextWriter LogWriter { get; set; }

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
            if (!IsLogLevelDisabled(level))
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
            if (!IsLogLevelDisabled(level))
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
            if (IsLogLevelDisabled(level))
            {
                return;
            }

            if (IsSeriousException(ex))
            {
                //no logging!
                return;
            }

            if (!HasActiveLoggers())
            {
                return;
            }

            try
            {
                string msg = FormatMessage(ex, level, message, args);

                WriteToLogFile(msg);
                WriteToTextWriter(msg);

#if !NETSTANDARD1_3
                WriteToConsole(msg);
                WriteToErrorConsole(msg);
#endif

#if !SILVERLIGHT && !__IOS__ && !__ANDROID__ && !NETSTANDARD1_3
                WriteToTrace(msg);
#endif
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

        private static string FormatMessage([CanBeNull]Exception ex, LogLevel level, string message, [CanBeNull]object[] args)
        {
            const string timeStampFormat = "yyyy-MM-dd HH:mm:ss.ffff";
            const string fieldSeparator = " ";

            var formattedMessage =
                (args == null) ? message : string.Format(CultureInfo.InvariantCulture, message, args);

            var builder = new StringBuilder(formattedMessage.Length + timeStampFormat.Length + (ex?.ToString().Length ?? 0) + 25);
            if (IncludeTimestamp)
            {
                builder
                    .Append(TimeSource.Current.Time.ToString(timeStampFormat, CultureInfo.InvariantCulture))
                    .Append(fieldSeparator);
            }

            builder
                .Append(level)
                .Append(fieldSeparator)
                .Append(formattedMessage);

            if (ex != null)
            {
                ex.MarkAsLoggedToInternalLogger();
                builder
                    .Append(fieldSeparator)
                    .Append("Exception: ")
                    .Append(ex);
            }

            return builder.ToString();
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
        private static bool IsLogLevelDisabled(LogLevel logLevel)
        {
            return ReferenceEquals(_logLevel, LogLevel.Off) || logLevel < _logLevel;
        }

        /// <summary>
        /// Determine if logging is enabled.
        /// </summary>
        /// <returns><c>true</c> if logging is enabled; otherwise, <c>false</c>.</returns>
        internal static bool HasActiveLoggers()
        {
            return !string.IsNullOrEmpty(LogFile) ||
                   LogToConsole ||
                   LogToConsoleError ||
#if !SILVERLIGHT && !__IOS__ && !__ANDROID__
                   LogToTrace ||
#endif
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
#endif

#if !SILVERLIGHT && !__IOS__ && !__ANDROID__ && !NETSTANDARD1_3
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
#if SILVERLIGHT || __IOS__ || __ANDROID__ || NETSTANDARD1_0
                Info(assembly.FullName);
#else
                var fileVersionInfo = !string.IsNullOrEmpty(assembly.Location) ?
                    System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location) : null;
                Info("{0}. File version: {1}. Product version: {2}.",
                    assembly.FullName,
                    fileVersionInfo?.FileVersion,
                    fileVersionInfo?.ProductVersion);
#endif
            }
            catch (Exception ex)
            {
                Error(ex, "Error logging version of assembly {0}.", assembly.FullName);
            }
        }

        private static string GetAppSettings(string configName)
        {
#if !SILVERLIGHT && !__IOS__ && !__ANDROID__ && !NETSTANDARD
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

#if !SILVERLIGHT
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
#endif
    }
}
