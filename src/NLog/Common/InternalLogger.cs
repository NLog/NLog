// 
// Copyright (c) 2004-2009 Jaroslaw Kowalski <jaak@jkowalski.net>
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
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Text;
using NLog.Internal;

namespace NLog.Common
{
    /// <summary>
    /// NLog internal logger.
    /// </summary>
    public static class InternalLogger
    {
        /// <summary>
        /// Initializes static members of the InternalLogger class.
        /// </summary>
        static InternalLogger()
        {
#if !NET_CF && !SILVERLIGHT
            LogToConsole = GetSetting("nlog.internalLogToConsole", "NLOG_INTERNAL_LOG_TO_CONSOLE", false);
            LogToConsoleError = GetSetting("nlog.internalLogToConsoleError", "NLOG_INTERNAL_LOG_TO_CONSOLE_ERROR", false);
            LogLevel = GetSetting("nlog.internalLogLevel", "NLOG_INTERNAL_LOG_LEVEL", LogLevel.Info);
            LogFile = GetSetting("nlog.internalLogFile", "NLOG_INTERNAL_LOG_FILE", string.Empty);
            Info("NLog internal logger initialized.");
#endif
        }

        /// <summary>
        /// Gets or sets the internal log level.
        /// </summary>
        public static LogLevel LogLevel { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether internal messages should be written to the console output stream.
        /// </summary>
        public static bool LogToConsole { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether internal messages should be written to the console error stream.
        /// </summary>
        public static bool LogToConsoleError { get; set; }

        /// <summary>
        /// Gets or sets the name of the internal log file.
        /// </summary>
        /// <remarks>A value of <see langword="null" /> value disables internal logging to a file.</remarks>
        public static string LogFile { get; set; }

        /// <summary>
        /// Gets a value indicating whether internal log includes Trace messages.
        /// </summary>
        public static bool IsTraceEnabled
        {
            get { return LogLevel.Trace >= LogLevel; }
        }

        /// <summary>
        /// Gets a value indicating whether internal log includes Debug messages.
        /// </summary>
        public static bool IsDebugEnabled
        {
            get { return LogLevel.Debug >= LogLevel; }
        }

        /// <summary>
        /// Gets a value indicating whether internal log includes Info messages.
        /// </summary>
        public static bool IsInfoEnabled
        {
            get { return LogLevel.Info >= LogLevel; }
        }

        /// <summary>
        /// Gets a value indicating whether internal log includes Warn messages.
        /// </summary>
        public static bool IsWarnEnabled
        {
            get { return LogLevel.Warn >= LogLevel; }
        }

        /// <summary>
        /// Gets a value indicating whether internal log includes Error messages.
        /// </summary>
        public static bool IsErrorEnabled
        {
            get { return LogLevel.Error >= LogLevel; }
        }

        /// <summary>
        /// Gets a value indicating whether internal log includes Fatal messages.
        /// </summary>
        public static bool IsFatalEnabled
        {
            get { return LogLevel.Fatal >= LogLevel; }
        }

        /// <summary>
        /// Logs the specified message at the specified level.
        /// </summary>
        /// <param name="level">Log level.</param>
        /// <param name="message">Message which may include positional parameters.</param>
        /// <param name="args">Arguments to the message.</param>
        public static void Log(LogLevel level, string message, params object[] args)
        {
            Write(level, message, args);
        }

        /// <summary>
        /// Logs the specified message at the specified level.
        /// </summary>
        /// <param name="level">Log level.</param>
        /// <param name="message">Log message.</param>
        public static void Log(LogLevel level, string message)
        {
            Write(level, message, null);
        }

        /// <summary>
        /// Logs the specified message at the Trace level.
        /// </summary>
        /// <param name="message">Message which may include positional parameters.</param>
        /// <param name="args">Arguments to the message.</param>
        public static void Trace(string message, params object[] args)
        {
            Write(LogLevel.Trace, message, args);
        }

        /// <summary>
        /// Logs the specified message at the Trace level.
        /// </summary>
        /// <param name="message">Log message.</param>
        public static void Trace(string message)
        {
            Write(LogLevel.Trace, message, null);
        }

        /// <summary>
        /// Logs the specified message at the Debug level.
        /// </summary>
        /// <param name="message">Message which may include positional parameters.</param>
        /// <param name="args">Arguments to the message.</param>
        public static void Debug(string message, params object[] args)
        {
            Write(LogLevel.Debug, message, args);
        }

        /// <summary>
        /// Logs the specified message at the Debug level.
        /// </summary>
        /// <param name="message">Log message.</param>
        public static void Debug(string message)
        {
            Write(LogLevel.Debug, message, null);
        }

        /// <summary>
        /// Logs the specified message at the Info level.
        /// </summary>
        /// <param name="message">Message which may include positional parameters.</param>
        /// <param name="args">Arguments to the message.</param>
        public static void Info(string message, params object[] args)
        {
            Write(LogLevel.Info, message, args);
        }

        /// <summary>
        /// Logs the specified message at the Info level.
        /// </summary>
        /// <param name="message">Log message.</param>
        public static void Info(string message)
        {
            Write(LogLevel.Info, message, null);
        }

        /// <summary>
        /// Logs the specified message at the Warn level.
        /// </summary>
        /// <param name="message">Message which may include positional parameters.</param>
        /// <param name="args">Arguments to the message.</param>
        public static void Warn(string message, params object[] args)
        {
            Write(LogLevel.Warn, message, args);
        }

        /// <summary>
        /// Logs the specified message at the Warn level.
        /// </summary>
        /// <param name="message">Log message.</param>
        public static void Warn(string message)
        {
            Write(LogLevel.Warn, message, null);
        }

        /// <summary>
        /// Logs the specified message at the Error level.
        /// </summary>
        /// <param name="message">Message which may include positional parameters.</param>
        /// <param name="args">Arguments to the message.</param>
        public static void Error(string message, params object[] args)
        {
            Write(LogLevel.Error, message, args);
        }

        /// <summary>
        /// Logs the specified message at the Error level.
        /// </summary>
        /// <param name="message">Log message.</param>
        public static void Error(string message)
        {
            Write(LogLevel.Error, message, null);
        }

        /// <summary>
        /// Logs the specified message at the Fatal level.
        /// </summary>
        /// <param name="message">Message which may include positional parameters.</param>
        /// <param name="args">Arguments to the message.</param>
        public static void Fatal(string message, params object[] args)
        {
            Write(LogLevel.Fatal, message, args);
        }

        /// <summary>
        /// Logs the specified message at the Fatal level.
        /// </summary>
        /// <param name="message">Log message.</param>
        public static void Fatal(string message)
        {
            Write(LogLevel.Fatal, message, null);
        }

        private static void Write(LogLevel level, string message, object[] args)
        {
            if (level < LogLevel)
            {
                return;
            }

            if (String.IsNullOrEmpty(LogFile) && !LogToConsole && !LogToConsoleError)
            {
                return;
            }

            try
            {
                string formattedMessage = message;
                if (args != null)
                {
                    formattedMessage = string.Format(CultureInfo.InvariantCulture, message, args);
                }

                var builder = new StringBuilder(message.Length + 32);
                builder.Append(CurrentTimeGetter.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff", CultureInfo.InvariantCulture));
                builder.Append(" ");
                builder.Append(level.ToString());
                builder.Append(" ");
                builder.Append(formattedMessage);
                string msg = builder.ToString();

                if (!String.IsNullOrEmpty(LogFile))
                {
                    using (TextWriter textWriter = File.AppendText(LogFile))
                    {
                        textWriter.WriteLine(msg);
                    }
                }

                if (LogToConsole)
                {
                    Console.WriteLine(msg);
                }

                if (LogToConsoleError)
                {
                    Console.Error.WriteLine(msg);
                }
            }
            catch
            {
                // we have no place to log the message to so we ignore it
            }
        }

#if !NET_CF && !SILVERLIGHT
        private static string GetSettingString(string configName, string envName)
        {
            string settingValue = ConfigurationManager.AppSettings[configName];
            if (settingValue == null)
            {
                try
                {
                    settingValue = Environment.GetEnvironmentVariable(envName);
                }
                catch (Exception)
                {
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
            catch
            {
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
            catch
            {
                return defaultValue;
            }
        }
#endif
    }
}
