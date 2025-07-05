//
// Copyright (c) 2004-2024 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.IO;
    using JetBrains.Annotations;
    using NLog.Internal;
    using NLog.Internal.Fakeables;
    using NLog.Time;

    /// <summary>
    /// NLog internal logger.
    ///
    /// Writes to file, console or custom text writer (see <see cref="InternalLogger.LogWriter"/>)
    /// </summary>
    /// <seealso href="https://github.com/NLog/NLog/wiki/Internal-Logging">Documentation on NLog Wiki</seealso>
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
            LogLevel = LogLevel.Off;
            IncludeTimestamp = true;
            LogToConsole = false;
            LogToConsoleError = false;
            LogFile = null;
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
        /// Gets or sets the file path of the internal log file.
        /// </summary>
        /// <remarks>A value of <see langword="null" /> value disables internal logging to a file.</remarks>
        public static string? LogFile
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

                var logFile = (value != null && !string.IsNullOrEmpty(value)) ? ExpandFilePathVariables(value) : null;
                _logFile = logFile;
                if (logFile != null)
                {
                    CreateDirectoriesIfNeeded(logFile);
                }
            }
        }
        private static string? _logFile;

        /// <summary>
        /// Gets or sets the text writer that will receive internal logs.
        /// </summary>
        public static TextWriter? LogWriter { get; set; }

        /// <summary>
        /// Internal LogEvent written to the InternalLogger
        /// </summary>
        /// <remarks>
        /// EventHandler will only be triggered for events, where severity matches the configured <see cref="LogLevel"/>.
        ///
        /// Never use/call NLog Logger-objects when handling these internal events, as it will lead to deadlock / stackoverflow.
        /// </remarks>
        public static event InternalEventOccurredHandler? InternalEventOccurred;

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
        public static void Log(LogLevel level, [Localizable(false)] string message, params object?[] args)
        {
            Write(null, level, message, args);
        }

#if NETSTANDARD2_1_OR_GREATER || NET9_0_OR_GREATER
        /// <summary>
        /// Logs the specified message without an <see cref="Exception"/> at the specified level.
        /// </summary>
        /// <param name="level">Log level.</param>
        /// <param name="message">Message which may include positional parameters.</param>
        /// <param name="args">Arguments to the message.</param>
        [StringFormatMethod("message")]
        public static void Log(LogLevel level, [Localizable(false)] string message, params ReadOnlySpan<object?> args)
        {
            if (IsLogLevelEnabled(level))
                Write(null, level, message, args.IsEmpty ? null : args.ToArray());
        }

        /// <summary>
        /// Logs the specified message with an <see cref="Exception"/> at the specified level.
        /// </summary>
        /// <param name="ex">Exception to be logged.</param>
        /// <param name="level">Log level.</param>
        /// <param name="message">Message which may include positional parameters.</param>
        /// <param name="args">Arguments to the message.</param>
        [StringFormatMethod("message")]
        public static void Log(Exception? ex, LogLevel level, [Localizable(false)] string message, params ReadOnlySpan<object?> args)
        {
            if (IsLogLevelEnabled(level))
                Write(ex, level, message, args.IsEmpty ? null : args.ToArray());
        }
#endif

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
        public static void Log(Exception? ex, LogLevel level, [Localizable(false)] Func<string> messageFunc)
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
        public static void Log(Exception? ex, LogLevel level, [Localizable(false)] string message, params object?[] args)
        {
            Write(ex, level, message, args);
        }

        /// <summary>
        /// Logs the specified message with an <see cref="Exception"/> at the specified level.
        /// </summary>
        /// <param name="ex">Exception to be logged.</param>
        /// <param name="level">Log level.</param>
        /// <param name="message">Log message.</param>
        public static void Log(Exception? ex, LogLevel level, [Localizable(false)] string message)
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
        private static void Write(Exception? ex, LogLevel level, string message, object?[]? args)
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

        private static void WriteToLog(LogLevel level, Exception? ex, string fullMessage, IInternalLoggerContext? loggerContext)
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
                var loggerContextName = (loggerContext is null || string.IsNullOrEmpty(loggerContext.Name)) ? loggerContext?.ToString() : loggerContext.Name;
                InternalEventOccurred?.Invoke(null, new InternalLogEventArgs(fullMessage, level, ex, loggerContext?.GetType(), loggerContextName));
            }
        }

        /// <summary>
        /// Create log line with timestamp, exception message etc (if configured)
        /// </summary>
        private static string CreateLogLine(Exception? ex, LogLevel level, string fullMessage)
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
                    ex != null ? " Exception: " : string.Empty,
                    ex?.ToString() ?? "");
            }
            else
            {
                return string.Concat(
                    level.ToString(),
                    fieldSeparator,
                    fullMessage,
                    ex != null ? " Exception: " : string.Empty,
                    ex?.ToString() ?? string.Empty);
            }
        }

        /// <summary>
        /// Determine if logging should be avoided because of exception type.
        /// </summary>
        /// <param name="exception">The exception to check.</param>
        /// <returns><see langword="true"/> if logging should be avoided; otherwise, <see langword="false"/>.</returns>
        private static bool IsSeriousException(Exception? exception)
        {
            return exception != null && exception.MustBeRethrownImmediately();
        }

        /// <summary>
        /// Determine if logging is enabled for given LogLevel
        /// </summary>
        /// <param name="logLevel">The <see cref="LogLevel"/> for the log event.</param>
        /// <returns><see langword="true"/> if logging is enabled; otherwise, <see langword="false"/>.</returns>
        private static bool IsLogLevelEnabled(LogLevel logLevel)
        {
            return !ReferenceEquals(_logLevel, LogLevel.Off) && _logLevel.CompareTo(logLevel) <= 0;
        }

        /// <summary>
        /// Determine if logging is enabled.
        /// </summary>
        /// <returns><see langword="true"/> if logging is enabled; otherwise, <see langword="false"/>.</returns>
        internal static bool HasActiveLoggers()
        {
            if (InternalEventOccurred is null && LogWriter is null)
                return false;
            else
                return true;
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
                if (ContainsSubStringIgnoreCase(internalLogFile, "${currentdir}", out var currentDirToken))
                    internalLogFile = internalLogFile.Replace(currentDirToken, System.IO.Directory.GetCurrentDirectory() + System.IO.Path.DirectorySeparatorChar.ToString());
                if (ContainsSubStringIgnoreCase(internalLogFile, "${basedir}", out var baseDirToken))
                    internalLogFile = internalLogFile.Replace(baseDirToken, LogManager.LogFactory.CurrentAppEnvironment.AppDomainBaseDirectory + System.IO.Path.DirectorySeparatorChar.ToString());
                if (ContainsSubStringIgnoreCase(internalLogFile, "${tempdir}", out var tempDirToken))
                    internalLogFile = internalLogFile.Replace(tempDirToken, LogManager.LogFactory.CurrentAppEnvironment.UserTempFilePath + System.IO.Path.DirectorySeparatorChar.ToString());
                if (ContainsSubStringIgnoreCase(internalLogFile, "${processdir}", out var processDirToken))
                    internalLogFile = internalLogFile.Replace(processDirToken, System.IO.Path.GetDirectoryName(LogManager.LogFactory.CurrentAppEnvironment.CurrentProcessFilePath) + System.IO.Path.DirectorySeparatorChar.ToString());
                if (ContainsSubStringIgnoreCase(internalLogFile, "${commonApplicationDataDir}", out var commonAppDataDirToken))
                    internalLogFile = internalLogFile.Replace(commonAppDataDirToken, NLog.LayoutRenderers.SpecialFolderLayoutRenderer.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + System.IO.Path.DirectorySeparatorChar.ToString());
                if (ContainsSubStringIgnoreCase(internalLogFile, "${userApplicationDataDir}", out var appDataDirToken))
                    internalLogFile = internalLogFile.Replace(appDataDirToken, NLog.LayoutRenderers.SpecialFolderLayoutRenderer.GetFolderPath(Environment.SpecialFolder.ApplicationData) + System.IO.Path.DirectorySeparatorChar.ToString());
                if (ContainsSubStringIgnoreCase(internalLogFile, "${userLocalApplicationDataDir}", out var localapplicationdatadir))
                    internalLogFile = internalLogFile.Replace(localapplicationdatadir, NLog.LayoutRenderers.SpecialFolderLayoutRenderer.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + System.IO.Path.DirectorySeparatorChar.ToString());
                if (internalLogFile.IndexOf('%') >= 0)
                    internalLogFile = Environment.ExpandEnvironmentVariables(internalLogFile);

                if (!string.IsNullOrEmpty(internalLogFile) && internalLogFile.IndexOf('.') >= 0)
                    internalLogFile = AppEnvironmentWrapper.FixFilePathWithLongUNC(internalLogFile);

                return internalLogFile;
            }
            catch
            {
                return internalLogFile;
            }
        }

        private static bool ContainsSubStringIgnoreCase(string haystack, string needle, out string? result)
        {
            int needlePos = haystack.IndexOf(needle, StringComparison.OrdinalIgnoreCase);
            result = needlePos >= 0 ? haystack.Substring(needlePos, needle.Length) : null;
            return result != null;
        }

        private static void LogToConsoleSubscription(object? sender, InternalLogEventArgs eventArgs)
        {
            var logLine = CreateLogLine(eventArgs.Exception, eventArgs.Level, eventArgs.Message);
            NLog.Targets.ConsoleTargetHelper.WriteLineThreadSafe(Console.Out, logLine);
        }

        private static void LogToConsoleErrorSubscription(object? sender, InternalLogEventArgs eventArgs)
        {
            var logLine = CreateLogLine(eventArgs.Exception, eventArgs.Level, eventArgs.Message);
            NLog.Targets.ConsoleTargetHelper.WriteLineThreadSafe(Console.Error, logLine);
        }

        private static void LogToFileSubscription(object? sender, InternalLogEventArgs eventArgs)
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
    }
}
