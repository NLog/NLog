// 
// Copyright (c) 2004 Jaroslaw Kowalski <jaak@polbox.com>
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

namespace NLog
{
    public sealed class InternalLogger
    {
        private static LogLevel _logLevel = LogLevel.Debug;

        public static LogLevel LogLevel
        {
            get { return _logLevel; }
            set { _logLevel = value; }
        }

        private static string _logFile = null;

#if !NETCF
        static InternalLogger()
        {
            try
            {
                _logFile = Environment.GetEnvironmentVariable("NLOG_INTERNAL");
                Info("NLog internal logger initialized.");
            }
            catch (Exception)
            {
            }
        }
#endif
        
        private static void Write(LogLevel level, IFormatProvider formatProvider, string message, object[] args)
        {
            if (level < _logLevel)
                return;

            if (_logFile == null)
                return;

            try {
                using (TextWriter textWriter = File.AppendText(_logFile)) {
                    string formattedMessage = message;
                    if (args != null)
                        formattedMessage = String.Format(formatProvider, message, args);

                    StringBuilder builder = new StringBuilder(message.Length + 32);
                    builder.Append(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff", CultureInfo.InvariantCulture));
                    builder.Append(" ");
                    builder.Append(level.ToString());
                    builder.Append(" ");
                    builder.Append(formattedMessage);
                    textWriter.WriteLine(builder.ToString());
                }
            }
            catch (Exception) {
            }
        }

        public bool IsDebugEnabled { get { return _logLevel >= LogLevel.Debug; } }
        public bool IsInfoEnabled  { get { return _logLevel >= LogLevel.Info; } }
        public bool IsWarnEnabled  { get { return _logLevel >= LogLevel.Warn; } }
        public bool IsErrorEnabled { get { return _logLevel >= LogLevel.Error; } }
        public bool IsFatalEnabled { get { return _logLevel >= LogLevel.Fatal; } }

		public static void Log(LogLevel level, IFormatProvider formatProvider, string message, params object[] args) 
		{
			Write(level, formatProvider, message, args);
		}
        
		public static void Log(LogLevel level, string message, params object[] args) 
		{
            Write(level, null, message, args);
        }
        
        public static void Log(LogLevel level, string message) {
            Write(level, null, message, null);
        }
        
		public static void Debug(IFormatProvider formatProvider, string message, params object[] args) 
		{
			Write(LogLevel.Debug, formatProvider, message, args);
		}
        
		public static void Debug(string message, params object[] args) 
		{
            Write(LogLevel.Debug, null, message, args);
        }
        
        public static void Debug(string message) {
            Write(LogLevel.Debug, null, message, null);
        }
        
		public static void Info(IFormatProvider formatProvider, string message, params object[] args) 
		{
			Write(LogLevel.Info, formatProvider, message, args);
		}
        
		public static void Info(string message, params object[] args) 
		{
            Write(LogLevel.Info, null, message, args);
        }
        
        public static void Info(string message) {
            Write(LogLevel.Info, null, message, null);
        }
        
		public static void Warn(IFormatProvider formatProvider, string message, params object[] args) 
		{
			Write(LogLevel.Warn, formatProvider, message, args);
		}
        
		public static void Warn(string message, params object[] args) 
		{
            Write(LogLevel.Warn, null, message, args);
        }
        
        public static void Warn(string message) {
            Write(LogLevel.Warn, null, message, null);
        }
        
		public static void Error(IFormatProvider formatProvider, string message, params object[] args) 
		{
			Write(LogLevel.Error, formatProvider, message, args);
		}
        
		public static void Error(string message, params object[] args) 
		{
            Write(LogLevel.Error, null, message, args);
        }
        
        public static void Error(string message) {
            Write(LogLevel.Error, null, message, null);
        }
        
		public static void Fatal(IFormatProvider formatProvider, string message, params object[] args) 
		{
			Write(LogLevel.Fatal, formatProvider, message, args);
		}
        
		public static void Fatal(string message, params object[] args) 
		{
            Write(LogLevel.Fatal, null, message, args);
        }
        
        public static void Fatal(string message) {
            Write(LogLevel.Fatal, null, message, null);
        }
    }
}
