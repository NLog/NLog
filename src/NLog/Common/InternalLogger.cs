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

       /// <summary>
        /// Set the config of the InternalLogger with defaults and config.
        /// </summary>
        public static void Reset()
        {
            ExceptionThrowWhenWriting = false;
            LogWriter = null;
            InternalEventOccurred = null;

#pragma warning disable CS0618 // Type or member is obsolete
            _logMessageReceived = null;
            LogLevel = GetSetting("nlog.internalLogLevel", "NLOG_INTERNAL_LOG_LEVEL", LogLevel.Off);
            IncludeTimestamp = GetSetting("nlog.internalLogIncludeTimestamp", "NLOG_INTERNAL_INCLUDE_TIMESTAMP", true);
            LogToConsole = GetSetting("nlog.internalLogToConsole", "NLOG_INTERNAL_LOG_TO_CONSOLE", false);
            LogToConsoleError = GetSetting("nlog.internalLogToConsoleError", "NLOG_INTERNAL_LOG_TO_CONSOLE_ERROR", false);
            LogFile = GetSetting("nlog.internalLogFile", "NLOG_INTERNAL_LOG_FILE", string.Empty);
            LogToTrace = GetSetting("nlog.internalLogToTrace", "NLOG_INTERNAL_LOG_TO_TRACE", false);
#pragma warning restore CS0618 // Type or member is obsolete
            Info("NLog internal logger initialized.");
        }

        /// <summary>
        /// Gets or sets the minimal internal log level. 
        /// </summary>
        /// <example>If set to <see cref="NLog.LogLevel.Info"/>, then messages of the levels <see cref="NLog.LogLevel.Info"/>, <see cref="NLog.LogLevel.Error"/> and <see cref="NLog.LogLevel.Fatal"/> will be written.</example>
        public static LogLevel LogLevel { get => _logLevel; set => _logLevel = value ?? LogLevel.Off; }
        private static LogLevel _logLevel = LogLevel.Off;

        /// <summary>
        /// Gets or sets a value indicating whether internal messages should be written to the console output stream.
        /// </summary>
        /// <remarks>Your application must be a console application.</remarks>
        public static bool LogToConsole
        {
            get => _logToConsole;
            set
            {
                if (_logToConsole != value)
                {
                    InternalEventOccurred -= LogToConsoleSubscription;
                    if (value)
                        InternalEventOccurred += LogToConsoleSubscription;
                    _logToConsole = value;
                }
            }
        }
        private static bool _logToConsole;

        /// <summary>
        /// Gets or sets a value indicating whether internal messages should be written to the console error stream.
        /// </summary>
        /// <remarks>Your application must be a console application.</remarks>
        public static bool LogToConsoleError
        {
            get => _logToConsoleError;
            set
            {
                if (_logToConsoleError != value)
                {
                    InternalEventOccurred -= LogToConsoleErrorSubscription;
                    if (value)
                        InternalEventOccurred += LogToConsoleErrorSubscription;
                    _logToConsoleError = value;
                }
            }
        }
        private static bool _logToConsoleError;

        /// <summary>
        /// Obsolete and replaced by <see cref="InternalLogger.LogWriter"/> with NLog v5.3.
        /// Gets or sets a value indicating whether internal messages should be written to the <see cref="System.Diagnostics"/>.Trace
        /// </summary>
        [Obsolete("Instead use InternalLogger.LogWriter. Marked obsolete with NLog v5.3")]
        public static bool LogToTrace
        {
            get => _logToTrace;
            set
            {
                if (_logToTrace != value)
                {
                    InternalEventOccurred -= LogToTraceSubscription;
                    if (value)
                        InternalEventOccurred += LogToTraceSubscription;
                    _logToTrace = value;
                }
            }
        }
        private static bool _logToTrace;

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
                if (!string.Equals(_logFile, value))
                {
                    InternalEventOccurred -= LogToFileSubscription;
                    if (!string.IsNullOrEmpty(value))
                        InternalEventOccurred += LogToFileSubscription;
                    _logFile = value;
                }

                if (!string.IsNullOrEmpty(value))
                {
                    _logFile = ExpandFilePathVariables(value);
                    CreateDirectoriesIfNeeded(_logFile);
                }
            }
        }
        private static string _logFile;

        /// <summary>
        /// Gets or sets the text writer that will receive internal logs.
        /// </summary>
        public static TextWriter LogWriter { get; set; }

        /// <summary>
        /// Obsolete and replaced by <see cref="InternalEventOccurred"/> with NLog 5.3.
        /// Event written to the internal log.
        /// </summary>
        /// <remarks>
        /// EventHandler will only be triggered for events, where severity matches the configured <see cref="LogLevel"/>.
        /// 
        /// Avoid using/calling NLog Logger-objects when handling these internal events, as it will lead to deadlock / stackoverflow.
        /// </remarks>
        [Obsolete("Instead use InternalEventOccurred. Marked obsolete with NLog v5.3")]
        public static event EventHandler<InternalLoggerMessageEventArgs> LogMessageReceived
        {
            add
            {
                if (_logMessageReceived == null)
                    InternalEventOccurred += LogToMessageReceived;
                _logMessageReceived += value;
            }
            remove
            {
                _logMessageReceived -= value;
                if (_logMessageReceived == null)
                    InternalEventOccurred -= LogToMessageReceived;
            }
        }
        [Obsolete("Instead use InternalEventOccurred. Marked obsolete with NLog v5.3")]
        private static event EventHandler<InternalLoggerMessageEventArgs> _logMessageReceived;

        /// <summary>
        /// Internal LogEvent written to the InternalLogger
        /// </summary>
        /// <remarks>
        /// EventHandler will only be triggered for events, where severity matches the configured <see cref="LogLevel"/>.
        /// 
        /// Never use/call NLog Logger-objects when handling these internal events, as it will lead to deadlock / stackoverflow.
        /// </remarks>
        public static event InternalEventOccurredHandler InternalEventOccurred;

        /// <summary>
        /// Gets or sets a value indicating whether timestamp should be included in internal log output.
        /// </summary>
        public static bool IncludeTimestamp { get; set; } = true;

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

            if (InternalEventOccurred is null && LogWriter is null)
            {
                return;
            }

            string fullMessage = message;

            try
            {
                fullMessage = args?.Length > 0 ? string.Format(CultureInfo.InvariantCulture, message, args) : message;
            }
            catch (Exception exception)
            {
                if (ex is null)
                    ex = exception;
                if (LogLevel.Error > level)
                    level = LogLevel.Error;
            }

            try
            {
                var loggerContext = args?.Length > 0 ? args[0] as IInternalLoggerContext : null;
                WriteToLog(level, ex, fullMessage, loggerContext);

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

        private static void WriteToLog(LogLevel level, Exception ex, string fullMessage, IInternalLoggerContext loggerContext)
        {
            if (LogWriter != null)
            {
                var logLine = CreateLogLine(ex, level, fullMessage);
                lock (LockObject)
                {
                    LogWriter?.WriteLine(logLine);
                }
            }

            if (InternalEventOccurred != null)
            {
                var loggerContextName = string.IsNullOrEmpty(loggerContext?.Name) ? loggerContext?.ToString() : loggerContext.Name;
                InternalEventOccurred?.Invoke(null, new InternalLogEventArgs(fullMessage, level, ex, loggerContext?.GetType(), loggerContextName));
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
            return !ReferenceEquals(_logLevel, LogLevel.Off) && _logLevel.CompareTo(logLevel) <= 0;
        }

        /// <summary>
        /// Determine if logging is enabled.
        /// </summary>
        /// <returns><c>true</c> if logging is enabled; otherwise, <c>false</c>.</returns>
        internal static bool HasActiveLoggers()
        {
            if (InternalEventOccurred is null && LogWriter is null)
                return false;
            else
                return true;
        }

        /// <summary>
        /// Logs the assembly version and file version of the given Assembly.
        /// </summary>
        /// <param name="assembly">The assembly to log.</param>
        [Obsolete("InternalLogger should be minimal. Marked obsolete with NLog v5.3")]
        public static void LogAssemblyVersion(Assembly assembly)
        {
            try
            {
#if !NETSTANDARD1_3 && !NETSTANDARD1_5
                var fileVersionInfo = !string.IsNullOrEmpty(assembly.Location) ?
                    System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location) : null;
                var globalAssemblyCache = false;
#if !NETSTANDARD
                if (assembly.GlobalAssemblyCache)
                    globalAssemblyCache = true;
#endif
                Info("{0}. File version: {1}. Product version: {2}. GlobalAssemblyCache: {3}",
                    assembly.FullName,
                    fileVersionInfo?.FileVersion,
                    fileVersionInfo?.ProductVersion,
                    globalAssemblyCache);
#else
                Info(assembly.FullName);
#endif
            }
            catch (Exception ex)
            {
                Error(ex, "Error logging version of assembly {0}.", assembly?.FullName);
            }
        }

        [Obsolete("InternalLogger should be minimal. Marked obsolete with NLog v5.3")]
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

        [Obsolete("InternalLogger should be minimal. Marked obsolete with NLog v5.3")]
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

        [Obsolete("InternalLogger should be minimal. Marked obsolete with NLog v5.3")]
        private static LogLevel GetSetting(string configName, string envName, LogLevel defaultValue)
        {
            string value = GetSettingString(configName, envName);
            if (value is null)
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

        [Obsolete("InternalLogger should be minimal. Marked obsolete with NLog v5.3")]
        private static T GetSetting<T>(string configName, string envName, T defaultValue)
        {
            string value = GetSettingString(configName, envName);
            if (value is null)
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

        private static string ExpandFilePathVariables(string internalLogFile)
        {
            try
            {
                if (ContainsSubStringIgnoreCase(internalLogFile, "${currentdir}", out string currentDirToken))
                    internalLogFile = internalLogFile.Replace(currentDirToken, System.IO.Directory.GetCurrentDirectory() + System.IO.Path.DirectorySeparatorChar.ToString());
                if (ContainsSubStringIgnoreCase(internalLogFile, "${basedir}", out string baseDirToken))
                    internalLogFile = internalLogFile.Replace(baseDirToken, LogManager.LogFactory.CurrentAppEnvironment.AppDomainBaseDirectory + System.IO.Path.DirectorySeparatorChar.ToString());
                if (ContainsSubStringIgnoreCase(internalLogFile, "${tempdir}", out string tempDirToken))
                    internalLogFile = internalLogFile.Replace(tempDirToken, LogManager.LogFactory.CurrentAppEnvironment.UserTempFilePath + System.IO.Path.DirectorySeparatorChar.ToString());
#if !NETSTANDARD1_3
                if (ContainsSubStringIgnoreCase(internalLogFile, "${processdir}", out string processDirToken))
                    internalLogFile = internalLogFile.Replace(processDirToken, System.IO.Path.GetDirectoryName(LogManager.LogFactory.CurrentAppEnvironment.CurrentProcessFilePath) + System.IO.Path.DirectorySeparatorChar.ToString());
#endif
#if !NETSTANDARD1_3 && !NETSTANDARD1_5
                if (ContainsSubStringIgnoreCase(internalLogFile, "${commonApplicationDataDir}", out string commonAppDataDirToken))
                    internalLogFile = internalLogFile.Replace(commonAppDataDirToken, NLog.LayoutRenderers.SpecialFolderLayoutRenderer.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + System.IO.Path.DirectorySeparatorChar.ToString());
                if (ContainsSubStringIgnoreCase(internalLogFile, "${userApplicationDataDir}", out string appDataDirToken))
                    internalLogFile = internalLogFile.Replace(appDataDirToken, NLog.LayoutRenderers.SpecialFolderLayoutRenderer.GetFolderPath(Environment.SpecialFolder.ApplicationData) + System.IO.Path.DirectorySeparatorChar.ToString());
                if (ContainsSubStringIgnoreCase(internalLogFile, "${userLocalApplicationDataDir}", out string localapplicationdatadir))
                    internalLogFile = internalLogFile.Replace(localapplicationdatadir, NLog.LayoutRenderers.SpecialFolderLayoutRenderer.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + System.IO.Path.DirectorySeparatorChar.ToString());
#endif
                if (internalLogFile.IndexOf('%') >= 0)
                    internalLogFile = Environment.ExpandEnvironmentVariables(internalLogFile);
                return internalLogFile;
            }
            catch
            {
                return internalLogFile;
            }
        }

        private static bool ContainsSubStringIgnoreCase(string haystack, string needle, out string result)
        {
            int needlePos = haystack.IndexOf(needle, StringComparison.OrdinalIgnoreCase);
            result = needlePos >= 0 ? haystack.Substring(needlePos, needle.Length) : null;
            return result != null;
        }

        private static void LogToConsoleSubscription(object sender, InternalLogEventArgs eventArgs)
        {
#if !NETSTANDARD1_3
            var logLine = CreateLogLine(eventArgs.Exception, eventArgs.Level, eventArgs.Message);
            NLog.Targets.ConsoleTargetHelper.WriteLineThreadSafe(Console.Out, logLine);
#endif
        }

        private static void LogToConsoleErrorSubscription(object sender, InternalLogEventArgs eventArgs)
        {
#if !NETSTANDARD1_3
            var logLine = CreateLogLine(eventArgs.Exception, eventArgs.Level, eventArgs.Message);
            NLog.Targets.ConsoleTargetHelper.WriteLineThreadSafe(Console.Error, logLine);
#endif
        }

        [Obsolete("Instead use InternalLogger.LogWriter. Marked obsolete with NLog v5.3")]
        private static void LogToTraceSubscription(object sender, InternalLogEventArgs eventArgs)
        {
#if !NETSTANDARD1_3
            var logLine = CreateLogLine(eventArgs.Exception, eventArgs.Level, eventArgs.Message);
            System.Diagnostics.Trace.WriteLine(logLine, "NLog");
#endif
        }

        private static void LogToFileSubscription(object sender, InternalLogEventArgs eventArgs)
        {
            var logLine = CreateLogLine(eventArgs.Exception, eventArgs.Level, eventArgs.Message);
            lock (LockObject)
            {
                try
                {
                    using (var textWriter = File.AppendText(_logFile))
                    {
                        textWriter.WriteLine(logLine);
                    }
                }
                catch (System.IO.IOException)
                {
                    // No where to report
                }
            }
        }

        [Obsolete("Instead use InternalEventOccurred and InternalLogEventArgs. Marked obsolete with NLog v5.3")]
        private static void LogToMessageReceived(object sender, InternalLogEventArgs eventArgs)
        {
            _logMessageReceived?.Invoke(null, new InternalLoggerMessageEventArgs(eventArgs.Message, eventArgs.Level, eventArgs.Exception, eventArgs.SenderType, eventArgs.SenderName));
        }
    }
}
