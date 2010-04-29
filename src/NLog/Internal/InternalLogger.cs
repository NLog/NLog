// 
// Copyright (c) 2004-2006 Jaroslaw Kowalski <jaak@jkowalski.net>
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
using System.Collections;
using System.Xml;
using System.IO;
using System.Reflection;
using System.Globalization;
using System.Text;

using NLog.Config;

namespace NLog.Internal
{
    /// <summary>
    /// NLog internal logger
    /// </summary>
    public sealed class InternalLogger
    {
        static LogLevel _logLevel = LogLevel.Info;
        static bool _logToConsole = false;
#if !NETCF_1_0
        static bool _logToConsoleError = false;
#endif
        static string _logFile = null;

        /// <summary>
        /// Internal log level.
        /// </summary>
        public static LogLevel LogLevel
        {
            get { return _logLevel; }
            set { _logLevel = value; }
        }

        /// <summary>
        /// Log internal messages to the console.
        /// </summary>
        public static bool LogToConsole
        {
            get { return _logToConsole; }
            set { _logToConsole = value; }
        }

#if !NETCF_1_0
        /// <summary>
        /// Log internal messages to the console error stream.
        /// </summary>
        public static bool LogToConsoleError
        {
            get { return _logToConsoleError; }
            set { _logToConsoleError = value; }
        }
#endif
        /// <summary>
        /// The name of the internal log file.
        /// </summary>
        /// <remarks><see langword="null" /> value disables internal logging to a file.</remarks>
        public static string LogFile
        {
            get { return _logFile; }
            set
            {
#if !NETCF
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
#else
                string baseDir = CompactFrameworkHelper.GetExeBaseDir();
#endif
                _logFile = Path.Combine(baseDir, value);
            }
        }

#if !NETCF
		
        static string GetSetting(string configName, string envName)
        {
#if DOTNET_2_0
            string setting = ConfigurationManager.AppSettings[configName];
#else
            string setting = ConfigurationSettings.AppSettings[configName];
#endif
            if (setting == null)
            {
                try
                {
                    setting = Environment.GetEnvironmentVariable(envName);
                }
                catch
                {
                    // ignore
                }
            }
            return setting;
        }

        static InternalLogger()
        {
            string setting = GetSetting("nlog.internalLogToConsole", "NLOG_INTERNAL_LOG_TO_CONSOLE");
            if (setting != null)
            {
                try
                {
                    LogToConsole = Convert.ToBoolean(setting);
                }
                catch
                {
                    // ignore
                }
            }

#if !NETCF_1_0
            setting = GetSetting("nlog.internalLogToConsoleError", "NLOG_INTERNAL_LOG_TO_CONSOLE_ERROR");
            if (setting != null)
            {
                try
                {
                    LogToConsoleError = Convert.ToBoolean(setting);
                }
                catch
                {
                    // ignore
                }
            }
#endif

            setting = GetSetting("nlog.internalLogLevel", "NLOG_INTERNAL_LOG_LEVEL");
            if (setting != null)
            {
                try
                {
                    LogLevel = LogLevel.FromString(setting);
                }
                catch
                {
                    // ignore
                }
            }

            setting = GetSetting("nlog.internalLogFile", "NLOG_INTERNAL_LOG_FILE");
            if (setting != null)
            {
                try
                {
                    LogFile = setting;
                }
                catch
                {
                    // ignore
                }
            }

            Info("NLog internal logger initialized.");
        }
#endif 

        private InternalLogger(){}

        private static void Write(LogLevel level, IFormatProvider formatProvider, string message, object[]args)
        {
            if (level < _logLevel)
                return ;

            if (_logFile == null && !_logToConsole
#if !NETCF_1_0
                && !_logToConsoleError
#endif
                )
                return ;

            try
            {
                string formattedMessage = message;
                if (args != null)
                    formattedMessage = String.Format(formatProvider, message, args);

                StringBuilder builder = new StringBuilder(message.Length + 32);
                builder.Append(CurrentTimeGetter.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff", CultureInfo.InvariantCulture));
                builder.Append(" ");
                builder.Append(level.ToString());
                builder.Append(" ");
                builder.Append(formattedMessage);
                string msg = builder.ToString();

                if (_logFile != null)
                {
                    using(TextWriter textWriter = File.AppendText(_logFile))
                    {
                        textWriter.WriteLine(msg);
                    }
                }

                if (_logToConsole)
                {
                    Console.WriteLine(msg);
                }

#if !NETCF_1_0
                if (_logToConsoleError)
                {
                    Console.Error.WriteLine(msg);
                }
#endif
            }
            catch 
            {
                // we have no place to log the message to so we ignore it
            }
        }

        /// <summary>
        /// Returns true when internal log level includes Trace messages
        /// </summary>
        public static bool IsTraceEnabled
        {
            get { return LogLevel.Trace >= _logLevel; }
        }

        /// <summary>
        /// Returns true when internal log level includes Debug messages
        /// </summary>
        public static bool IsDebugEnabled
        {
            get { return LogLevel.Debug >= _logLevel; }
        }
        /// <summary>
        /// Returns true when internal log level includes Info messages
        /// </summary>
        public static bool IsInfoEnabled
        {
            get { return LogLevel.Info >= _logLevel; }
        }
        /// <summary>
        /// Returns true when internal log level includes Warn messages
        /// </summary>
        public static bool IsWarnEnabled
        {
            get { return LogLevel.Warn >= _logLevel; }
        }
        /// <summary>
        /// Returns true when internal log level includes Error messages
        /// </summary>
        public static bool IsErrorEnabled
        {
            get { return LogLevel.Error >= _logLevel; }
        }
        /// <summary>
        /// Returns true when internal log level includes Fatal messages
        /// </summary>
        public static bool IsFatalEnabled
        {
            get { return LogLevel.Fatal >= _logLevel; }
        }

        /// <summary>
        /// Logs the specified message at the specified level.
        /// </summary>
        /// <param name="level">Log level.</param>
        /// <param name="formatProvider">Format provider to be used for formatting.</param>
        /// <param name="message">Message which may include positional parameters.</param>
        /// <param name="args">Arguments.</param>
        public static void Log(LogLevel level, IFormatProvider formatProvider, string message, params object[]args)
        {
            Write(level, formatProvider, message, args);
        }

        /// <summary>
        /// Logs the specified message at the specified level.
        /// </summary>
        /// <param name="level">Log level.</param>
        /// <param name="message">Message which may include positional parameters.</param>
        /// <param name="args">Arguments.</param>
        public static void Log(LogLevel level, string message, params object[]args)
        {
            Write(level, null, message, args);
        }

        /// <summary>
        /// Logs the specified message at the specified level.
        /// </summary>
        /// <param name="level">Log level.</param>
        /// <param name="message">Log message.</param>
        public static void Log(LogLevel level, string message)
        {
            Write(level, null, message, null);
        }

        /// <summary>
        /// Logs the specified message at the Trace level.
        /// </summary>
        /// <param name="formatProvider">Format provider to be used for formatting.</param>
        /// <param name="message">Message which may include positional parameters.</param>
        /// <param name="args">Arguments.</param>
        public static void Trace(IFormatProvider formatProvider, string message, params object[]args)
        {
            Write(LogLevel.Trace, formatProvider, message, args);
        }

        /// <summary>
        /// Logs the specified message at the Trace level.
        /// </summary>
        /// <param name="message">Message which may include positional parameters.</param>
        /// <param name="args">Arguments.</param>
        public static void Trace(string message, params object[]args)
        {
            Write(LogLevel.Trace, null, message, args);
        }

        /// <summary>
        /// Logs the specified message at the Trace level.
        /// </summary>
        /// <param name="message">Log message.</param>
        public static void Trace(string message)
        {
            Write(LogLevel.Trace, null, message, null);
        }

        /// <summary>
        /// Logs the specified message at the Debug level.
        /// </summary>
        /// <param name="formatProvider">Format provider to be used for formatting.</param>
        /// <param name="message">Message which may include positional parameters.</param>
        /// <param name="args">Arguments.</param>
        public static void Debug(IFormatProvider formatProvider, string message, params object[]args)
        {
            Write(LogLevel.Debug, formatProvider, message, args);
        }

        /// <summary>
        /// Logs the specified message at the Debug level.
        /// </summary>
        /// <param name="message">Message which may include positional parameters.</param>
        /// <param name="args">Arguments.</param>
        public static void Debug(string message, params object[]args)
        {
            Write(LogLevel.Debug, null, message, args);
        }

        /// <summary>
        /// Logs the specified message at the Debug level.
        /// </summary>
        /// <param name="message">Log message.</param>
        public static void Debug(string message)
        {
            Write(LogLevel.Debug, null, message, null);
        }

        /// <summary>
        /// Logs the specified message at the Info level.
        /// </summary>
        /// <param name="formatProvider">Format provider to be used for formatting.</param>
        /// <param name="message">Message which may include positional parameters.</param>
        /// <param name="args">Arguments.</param>
        public static void Info(IFormatProvider formatProvider, string message, params object[]args)
        {
            Write(LogLevel.Info, formatProvider, message, args);
        }

        /// <summary>
        /// Logs the specified message at the Info level.
        /// </summary>
        /// <param name="message">Message which may include positional parameters.</param>
        /// <param name="args">Arguments.</param>
        public static void Info(string message, params object[]args)
        {
            Write(LogLevel.Info, null, message, args);
        }

        /// <summary>
        /// Logs the specified message at the Info level.
        /// </summary>
        /// <param name="message">Log message.</param>
        public static void Info(string message)
        {
            Write(LogLevel.Info, null, message, null);
        }

        /// <summary>
        /// Logs the specified message at the Warn level.
        /// </summary>
        /// <param name="formatProvider">Format provider to be used for formatting.</param>
        /// <param name="message">Message which may include positional parameters.</param>
        /// <param name="args">Arguments.</param>
        public static void Warn(IFormatProvider formatProvider, string message, params object[]args)
        {
            Write(LogLevel.Warn, formatProvider, message, args);
        }

        /// <summary>
        /// Logs the specified message at the Warn level.
        /// </summary>
        /// <param name="message">Message which may include positional parameters.</param>
        /// <param name="args">Arguments.</param>
        public static void Warn(string message, params object[]args)
        {
            Write(LogLevel.Warn, null, message, args);
        }

        /// <summary>
        /// Logs the specified message at the Warn level.
        /// </summary>
        /// <param name="message">Log message.</param>
        public static void Warn(string message)
        {
            Write(LogLevel.Warn, null, message, null);
        }

        /// <summary>
        /// Logs the specified message at the Error level.
        /// </summary>
        /// <param name="formatProvider">Format provider to be used for formatting.</param>
        /// <param name="message">Message which may include positional parameters.</param>
        /// <param name="args">Arguments.</param>
        public static void Error(IFormatProvider formatProvider, string message, params object[]args)
        {
            Write(LogLevel.Error, formatProvider, message, args);
        }

        /// <summary>
        /// Logs the specified message at the Error level.
        /// </summary>
        /// <param name="message">Message which may include positional parameters.</param>
        /// <param name="args">Arguments.</param>
        public static void Error(string message, params object[]args)
        {
            Write(LogLevel.Error, null, message, args);
        }

        /// <summary>
        /// Logs the specified message at the Error level.
        /// </summary>
        /// <param name="message">Log message.</param>
        public static void Error(string message)
        {
            Write(LogLevel.Error, null, message, null);
        }

        /// <summary>
        /// Logs the specified message at the Fatal level.
        /// </summary>
        /// <param name="formatProvider">Format provider to be used for formatting.</param>
        /// <param name="message">Message which may include positional parameters.</param>
        /// <param name="args">Arguments.</param>
        public static void Fatal(IFormatProvider formatProvider, string message, params object[]args)
        {
            Write(LogLevel.Fatal, formatProvider, message, args);
        }

        /// <summary>
        /// Logs the specified message at the Fatal level.
        /// </summary>
        /// <param name="message">Message which may include positional parameters.</param>
        /// <param name="args">Arguments.</param>
        public static void Fatal(string message, params object[]args)
        {
            Write(LogLevel.Fatal, null, message, args);
        }

        /// <summary>
        /// Logs the specified message at the Fatal level.
        /// </summary>
        /// <param name="message">Log message.</param>
        public static void Fatal(string message)
        {
            Write(LogLevel.Fatal, null, message, null);
        }
    }
}
