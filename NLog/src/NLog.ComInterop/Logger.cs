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

using NLog;

namespace NLog.ComInterop
{
	public class Logger : ILogger
    {
        private static NLog.Logger _defaultLogger = new NullLogger();

        private NLog.Logger _logger = _defaultLogger;
        private string _loggerName = String.Empty;

        public void Log(string level, string message) {
            _logger.Log(StringToLevel(level), message);
        }
        
        public void Debug(string message) {
            _logger.Debug(message);
        }
        
        public void Info(string message) {
            _logger.Info(message);
        }
        
        public void Warn(string message) {
            _logger.Warn(message);
        }
        
        public void Error(string message) {
            _logger.Error(message);
        }
        
        public void Fatal(string message) {
            _logger.Fatal(message);
        }

        public bool IsEnabled(string level)
        {
            return _logger.IsEnabled(StringToLevel(level));
        }
        
        public bool IsDebugEnabled 
        {
            get { return _logger.IsDebugEnabled; }
        }
        
        public bool IsInfoEnabled
        { 
            get { return _logger.IsInfoEnabled; } 
        }
        
        public bool IsWarnEnabled
        { 
            get { return _logger.IsWarnEnabled; } 
        }
        
        public bool IsErrorEnabled
        { 
            get { return _logger.IsErrorEnabled; } 
        }
        
        public bool IsFatalEnabled
        { 
            get { return _logger.IsFatalEnabled; } 
        }

        public string LoggerName
        {
            get { return _loggerName; }
            set {
                _loggerName = value;
                _logger = NLog.LogManager.GetLogger(value); 
            }
        }

        private static LogLevel StringToLevel(string s) {
            switch (s[0]) {
                case 'D':
                    return LogLevel.Debug;
                case 'I':
                    return LogLevel.Info;
                case 'W':
                    return LogLevel.Warn;
                case 'E':
                    return LogLevel.Error;
                case 'F':
                    return LogLevel.Fatal;

                default:
                    throw new NotSupportedException("LogLevel not supported: " + s);
            }
        }
    }
}
