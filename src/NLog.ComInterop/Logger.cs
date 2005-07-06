// 
// Copyright (c) 2004,2005 Jaroslaw Kowalski <jkowalski@users.sourceforge.net>
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
using System.Runtime.InteropServices;

using NLog;
using NLog.Internal;

namespace NLog.ComInterop
{
    /// <summary>
    /// NLog COM Interop logger implementation
    /// </summary>
    [ComVisible(true)]
    [ProgId("NLog.Logger")]
    [Guid("181f39a8-41a8-4e35-91b6-5f8d96f5e61c")]
    [ClassInterface(ClassInterfaceType.None)]
    public class Logger: ILogger
    {
        private static NLog.Logger _defaultLogger = NLog.LogManager.CreateNullLogger();

        private NLog.Logger _logger = _defaultLogger;
        private string _loggerName = String.Empty;

        void ILogger.Log(string level, string message)
        {
            _logger.Log(StringToLevel(level), message);
        }

        void ILogger.Trace(string message)
        {
            _logger.Trace(message);
        }

        void ILogger.Debug(string message)
        {
            _logger.Debug(message);
        }

        void ILogger.Info(string message)
        {
            _logger.Info(message);
        }

        void ILogger.Warn(string message)
        {
            _logger.Warn(message);
        }

        void ILogger.Error(string message)
        {
            _logger.Error(message);
        }

        void ILogger.Fatal(string message)
        {
            _logger.Fatal(message);
        }

        bool ILogger.IsEnabled(string level)
        {
            return _logger.IsEnabled(StringToLevel(level));
        }

        bool ILogger.IsTraceEnabled
        {
            get
            {
                return _logger.IsTraceEnabled;
            }
        }

        bool ILogger.IsDebugEnabled
        {
            get
            {
                return _logger.IsDebugEnabled;
            }
        }

        bool ILogger.IsInfoEnabled
        {
            get
            {
                return _logger.IsInfoEnabled;
            }
        }

        bool ILogger.IsWarnEnabled
        {
            get
            {
                return _logger.IsWarnEnabled;
            }
        }

        bool ILogger.IsErrorEnabled
        {
            get
            {
                return _logger.IsErrorEnabled;
            }
        }

        bool ILogger.IsFatalEnabled
        {
            get
            {
                return _logger.IsFatalEnabled;
            }
        }

        string ILogger.LoggerName
        {
            get
            {
                return _loggerName;
            }
            set
            {
                _loggerName = value;
                _logger = NLog.LogManager.GetLogger(value);
            }
        }

        private static LogLevel StringToLevel(string s)
        {
            switch (s[0])
            {
                case 'T':
                    return LogLevel.Trace;
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
