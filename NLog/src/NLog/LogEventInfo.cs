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
using System.Diagnostics;

namespace NLog
{
    public struct LogEventInfo
    {
        public static readonly DateTime ZeroDate = DateTime.Now;

        private DateTime _timeStamp;
        private LogLevel _level;
        private string _loggerName;
        private string _message;

        public static readonly LogEventInfo Empty = new LogEventInfo(DateTime.Now, LogLevel.Debug, String.Empty, String.Empty);

        public LogEventInfo(DateTime ts, LogLevel level, string loggerName, string message) {
            _timeStamp = ts;
            _level = level;
            _loggerName = loggerName;
            _message = message;
#if !NETCF
            _stackTrace = null;
            _userStackFrame = 0;
#endif
        }

        public DateTime TimeStamp
        {
            get { return _timeStamp; }
        }

        public LogLevel Level
        {
            get { return _level; }
        }

#if !NETCF
        private StackTrace _stackTrace;
        private int _userStackFrame;

        internal void SetStackTrace(StackTrace stackTrace, int userStackFrame) {
            _stackTrace = stackTrace;
            _userStackFrame = userStackFrame;
        }

        public StackFrame UserStackFrame
        {
            get { return (_stackTrace != null) ? _stackTrace.GetFrame(_userStackFrame) : null; }
        }

        public StackTrace StackTrace
        {
            get { return _stackTrace; }
        }
#endif
        public string LoggerName
        {
            get { return _loggerName; }
        }

        public string Message
        {
            get { return _message; }
        }

    }
}
