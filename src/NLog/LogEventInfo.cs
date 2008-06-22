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
using System.Globalization;
using System.Diagnostics;
using System.Threading;
using System.Reflection;

using System.Collections;
using System.Collections.Specialized;
using NLog.Layouts;
using NLog.Internal;
using System.Collections.Generic;

namespace NLog
{
    /// <summary>
    /// Represents the logging event.
    /// </summary>
    public abstract class LogEventInfo
    {
        /// <summary>
        /// The date of the first log event created.
        /// </summary>
        public static readonly DateTime ZeroDate = DateTime.Now;

        private static int _globalSequenceID = 0;

        private DateTime _timeStamp;
        private LogLevel _level;
        private string _loggerName;
        private IDictionary<Layout,string> _layoutCache;
        private IDictionary<string,object> _eventContext;
        private int _sequenceID;

        /// <summary>
        /// Creates the null event.
        /// </summary>
        /// <returns></returns>
        public static LogEventInfo CreateNullEvent()
        {
            return new UnformattedLogEventInfo(LogLevel.Off, String.Empty, String.Empty);
        }

        /// <summary>
        /// Creates a new instance of <see cref="LogEventInfo"/> and assigns its fields.
        /// </summary>
        /// <param name="level">Log level</param>
        /// <param name="loggerName">Logger name</param>
        /// <param name="message">Log message including parameter placeholders</param>
        protected LogEventInfo(LogLevel level, string loggerName)
        {
            _timeStamp = CurrentTimeGetter.Now;
            _level = level;
            _loggerName = loggerName;
            _layoutCache = null;
            _sequenceID = Interlocked.Increment(ref _globalSequenceID);

#if !NET_CF
            _stackTrace = null;
            _userStackFrame = 0;
#endif 
        } 

        /// <summary>
        /// Gets or sets the timestamp of the logging event.
        /// </summary>
        public DateTime TimeStamp
        {
            get { return _timeStamp; }
            set { _timeStamp = value; }
        }

        /// <summary>
        /// Gets or sets the level of the logging event.
        /// </summary>
        public LogLevel Level
        {
            get { return _level; }
            set { _level = value; }
        }

#if !NET_CF
        private StackTrace _stackTrace;
        private int _userStackFrame;

        /// <summary>
        /// Returns true if stack trace has been set for this event.
        /// </summary>
        public bool HasStackTrace
        {
            get { return _stackTrace != null; }
        }

        internal void SetStackTrace(StackTrace stackTrace, int userStackFrame)
        {
            _stackTrace = stackTrace;
            _userStackFrame = userStackFrame;
        }

        /// <summary>
        /// Gets the stack frame of the method that did the logging.
        /// </summary>
        public StackFrame UserStackFrame
        {
            get { return (_stackTrace != null) ? _stackTrace.GetFrame(_userStackFrame): null; }
        }

        /// <summary>
        /// Gets the number index of the stack frame that represents the user
        /// code (not the NLog code)
        /// </summary>
        public int UserStackFrameNumber
        {
            get { return _userStackFrame; }
        }

        /// <summary>
        /// Gets the entire stack trace.
        /// </summary>
        public StackTrace StackTrace
        {
            get { return _stackTrace; }
        }
#endif 
        /// <summary>
        /// Gets or sets the exception information.
        /// </summary>
        public virtual Exception Exception
        {
            get { return null; }
        }

        /// <summary>
        /// Gets or sets the logger name.
        /// </summary>
        public string LoggerName
        {
            get { return _loggerName; }
            set { _loggerName = value; }
        }

        /// <summary>
        /// Returns the formatted message.
        /// </summary>
        public abstract string FormattedMessage { get; }

        /// <summary>
        /// Gets the dictionary of per-event context properties.
        /// </summary>
        public IDictionary<string,object> Context
        {
            get
            {
                if (_eventContext == null)
                    _eventContext = new Dictionary<string,object>();
                return _eventContext;
            }
        }

        /// <summary>
        /// The unique identifier of log event which is automatically generated
        /// and monotonously increasing.
        /// </summary>
        public int SequenceID
        {
            get { return _sequenceID; }
        }

        internal bool TryGetCachedLayoutValue(Layout layout, out string result)
        {
            if (_layoutCache == null)
            {
                result = null;
                return false;
            }
            return _layoutCache.TryGetValue(layout, out result);
        }

        internal void AddCachedLayoutValue(Layout layout, string value)
        {
            if (_layoutCache == null)
                _layoutCache = new Dictionary<Layout, string>();
            _layoutCache[layout] = value;
        }
    }
}