// 
// Copyright (c) 2004 Jaroslaw Kowalski <jkowalski@users.sourceforge.net>
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
using System.Collections;
using System.Xml;
using System.IO;
using System.Reflection;
using System.Globalization;
using System.Text;

using NLog.Config;

namespace NLog.Internal
{
    public sealed class InternalLogger
    {
        private static LogLevel _logLevel = LogLevel.Info;
        private static bool _logToConsole = false;

        public static LogLevel LogLevel
        {
            get
            {
                return _logLevel;
            }
            set
            {
                _logLevel = value;
            }
        }

        public static bool LogToConsole
        {
            get
            {
                return _logToConsole;
            }
            set
            {
                _logToConsole = value;
            }
        }

        public static string LogFile
        {
            get
            {
                return _logFile;
            }
            set
            {
                _logFile = value;
            }
        }

        private static string _logFile = null;

#if !NETCF
        static InternalLogger()
        {
            try
            {
                if (EnvironmentHelper.GetSafeEnvironmentVariable("NLOG_INTERNAL_LOG_TO_CONSOLE") != null)
                {
                    LogToConsole = true;
                }
                if (EnvironmentHelper.GetSafeEnvironmentVariable("NLOG_INTERNAL_LOG_LEVEL") != null)
                {
                    LogLevel = Logger.LogLevelFromString(Environment.GetEnvironmentVariable("NLOG_INTERNAL_LOG_LEVEL"));
                }
                _logFile = EnvironmentHelper.GetSafeEnvironmentVariable("NLOG_INTERNAL_LOG_FILE");
                Info("NLog internal logger initialized.");
            }
            catch {}
        }
#endif 

        private InternalLogger(){}

        private static void Write(LogLevel level, IFormatProvider formatProvider, string message, object[]args)
        {
            if (level < _logLevel)
                return ;

            if (_logFile == null && !_logToConsole)
                return ;

            try
            {
                string formattedMessage = message;
                if (args != null)
                    formattedMessage = String.Format(formatProvider, message, args);

                StringBuilder builder = new StringBuilder(message.Length + 32);
                builder.Append(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff", CultureInfo.InvariantCulture));
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
            }
            catch 
            {
                // we have no place to log the message to so we ignore it
            }
        }

        internal static bool IsDebugEnabled
        {
            get
            {
                return LogLevel.Debug >= _logLevel;
            }
        }
        internal static bool IsInfoEnabled
        {
            get
            {
                return LogLevel.Info >= _logLevel;
            }
        }
        internal static bool IsWarnEnabled
        {
            get
            {
                return LogLevel.Warn >= _logLevel;
            }
        }
        internal static bool IsErrorEnabled
        {
            get
            {
                return LogLevel.Error >= _logLevel;
            }
        }
        internal static bool IsFatalEnabled
        {
            get
            {
                return LogLevel.Fatal >= _logLevel;
            }
        }

        internal static void Log(LogLevel level, IFormatProvider formatProvider, string message, params object[]args)
        {
            Write(level, formatProvider, message, args);
        }

        internal static void Log(LogLevel level, string message, params object[]args)
        {
            Write(level, null, message, args);
        }

        internal static void Log(LogLevel level, string message)
        {
            Write(level, null, message, null);
        }

        internal static void Debug(IFormatProvider formatProvider, string message, params object[]args)
        {
            Write(LogLevel.Debug, formatProvider, message, args);
        }
        internal static void Debug(string message, params object[]args)
        {
            Write(LogLevel.Debug, null, message, args);
        }
        internal static void Debug(string message)
        {
            Write(LogLevel.Debug, null, message, null);
        }

        internal static void Info(IFormatProvider formatProvider, string message, params object[]args)
        {
            Write(LogLevel.Info, formatProvider, message, args);
        }

        internal static void Info(string message, params object[]args)
        {
            Write(LogLevel.Info, null, message, args);
        }

        internal static void Info(string message)
        {
            Write(LogLevel.Info, null, message, null);
        }

        internal static void Warn(IFormatProvider formatProvider, string message, params object[]args)
        {
            Write(LogLevel.Warn, formatProvider, message, args);
        }

        internal static void Warn(string message, params object[]args)
        {
            Write(LogLevel.Warn, null, message, args);
        }

        internal static void Warn(string message)
        {
            Write(LogLevel.Warn, null, message, null);
        }

        internal static void Error(IFormatProvider formatProvider, string message, params object[]args)
        {
            Write(LogLevel.Error, formatProvider, message, args);
        }

        internal static void Error(string message, params object[]args)
        {
            Write(LogLevel.Error, null, message, args);
        }

        internal static void Error(string message)
        {
            Write(LogLevel.Error, null, message, null);
        }

        internal static void Fatal(IFormatProvider formatProvider, string message, params object[]args)
        {
            Write(LogLevel.Fatal, formatProvider, message, args);
        }

        internal static void Fatal(string message, params object[]args)
        {
            Write(LogLevel.Fatal, null, message, args);
        }

        internal static void Fatal(string message)
        {
            Write(LogLevel.Fatal, null, message, null);
        }
    }
}
