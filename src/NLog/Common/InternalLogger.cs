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

namespace NLog.Common
{
    using JetBrains.Annotations;
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Text;
    using Internal;
    using Time;
#if !SILVERLIGHT && !__IOS__ && !__ANDROID__
    using ConfigurationManager = System.Configuration.ConfigurationManager;
    using System.Diagnostics;
#endif

    /// <summary>
    /// NLog internal logger.
    /// 
    /// Writes to file, console or custom textwriter (see <see cref="InternalLogger.LogWriter"/>)
    /// </summary>
    /// <remarks>
    /// Don't use <see cref="ExceptionHelper.MustBeRethrown"/> as that can lead to recursive calls - stackoverflows
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
        public static LogLevel LogLevel { get; set; }

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
        /// Gets or sets a value indicating whether internal messages should be written to the <see cref="System.Diagnostics.Trace"/>.
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

#if !SILVERLIGHT && !__IOS__ && !__ANDROID__
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

        internal static bool ExceptionThrowWhenWriting = false;

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
            if (IsSeriousException(ex))
            {
                //no logging!
                return;
            }

            if (!LoggingEnabled(level))
            {
                return;
            }

            try
            {
                var formattedMessage = message;
                if (args != null)
                {
                    formattedMessage = string.Format(CultureInfo.InvariantCulture, message, args);
                }

                var builder = new StringBuilder(message.Length + 32);
                if (IncludeTimestamp)
                {
                    builder.Append(TimeSource.Current.Time.ToString("yyyy-MM-dd HH:mm:ss.ffff", CultureInfo.InvariantCulture));
                    builder.Append(" ");
                }

                builder.Append(level);
                builder.Append(" ");
                builder.Append(formattedMessage);
                if (ex != null)
                {
                    ex.MarkAsLoggedToInternalLogger();
                    builder.Append(" Exception: ");
                    builder.Append(ex);
                }
                var msg = builder.ToString();

                // log to file
                var logFile = LogFile;
                if (!string.IsNullOrEmpty(logFile))
                {
                    using (var textWriter = File.AppendText(logFile))
                    {
                        textWriter.WriteLine(msg);
                    }
                }

                // log to LogWriter
                var writer = LogWriter;
                if (writer != null)
                {
                    lock (LockObject)
                    {
                        writer.WriteLine(msg);
                    }
                }

                // log to console
                if (LogToConsole)
                {
                    Console.WriteLine(msg);
                }

                // log to console error
                if (LogToConsoleError)
                {
                    Console.Error.WriteLine(msg);
                }
#if !SILVERLIGHT && !__IOS__ && !__ANDROID__
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
        /// Determine if logging is enabled.
        /// </summary>
        /// <param name="logLevel">The <see cref="LogLevel"/> for the log event.</param>
        /// <returns><c>true</c> if logging is enabled; otherwise, <c>false</c>.</returns>
        private static bool LoggingEnabled(LogLevel logLevel)
        {
            if (logLevel == LogLevel.Off || logLevel < LogLevel)
            {
                return false;
            }

            return !string.IsNullOrEmpty(LogFile) ||
                   LogToConsole ||
                   LogToConsoleError ||
#if !SILVERLIGHT && !__IOS__ && !__ANDROID__
                   LogToTrace ||
#endif
                   LogWriter != null;
        }

#if !SILVERLIGHT && !__IOS__ && !__ANDROID__
        /// <summary>
        /// Write internal messages to the <see cref="System.Diagnostics.Trace"/>.
        /// </summary>
        /// <param name="message">A message to write.</param>
        /// <remarks>
        /// Works when property <see cref="LogToTrace"/> set to true.
        /// The <see cref="System.Diagnostics.Trace"/> is used in Debug and Relese configuration. 
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
#if SILVERLIGHT || __IOS__ || __ANDROID__
                Info(assembly.FullName);
#else
                var fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
                Info("{0}. File version: {1}. Product version: {2}.",
                    assembly.FullName,
                    fileVersionInfo.FileVersion,
                    fileVersionInfo.ProductVersion);
#endif
            }
            catch (Exception ex)
            {
                Error(ex, "Error logging version of assembly {0}.", assembly.FullName);
            }
        }

#if !SILVERLIGHT && !__IOS__ && !__ANDROID__
        private static string GetSettingString(string configName, string envName)
        {
            string settingValue = ConfigurationManager.AppSettings[configName];
            if (settingValue == null)
            {
                try
                {
                    settingValue = Environment.GetEnvironmentVariable(envName);
                }
                catch (Exception exception)
                {
                    if (exception.MustBeRethrownImmediately())
                    {
                        throw;
                    }
                }
            }

            return settingValue;
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
                if (InternalLogger.LogLevel == NLog.LogLevel.Off)
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