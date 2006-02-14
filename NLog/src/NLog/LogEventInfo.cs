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
using System.Globalization;
using System.Diagnostics;
using System.Threading;

using System.Collections;
using System.Collections.Specialized;

namespace NLog
{
    /// <summary>
    /// Represents the logging event.
    /// </summary>
    public class LogEventInfo
    {
        /// <summary>
        /// The date of the first log event created.
        /// </summary>
        public static readonly DateTime ZeroDate = DateTime.Now;

        private static int _globalSequenceID = 0;

        private DateTime _timeStamp;
        private LogLevel _level;
        private string _loggerName;
        private string _message;
        private string _formattedMessage;
        private Exception _exception;
        private object[] _parameters;
        private IFormatProvider _formatProvider;
        private IDictionary _layoutCache;
        private IDictionary _callContext;
        private int _sequenceID;

        /// <summary>
        /// An empty event - for rendering layouts where logging
        /// event is not otherwise available.
        /// </summary>
        public static readonly LogEventInfo Empty = new LogEventInfo(DateTime.Now, LogLevel.Debug, String.Empty, CultureInfo.InvariantCulture, String.Empty, null, null, null);

        /// <summary>
        /// Creates a new instance of <see cref="LogEventInfo"/> and assigns its fields.
        /// </summary>
        /// <param name="ts">Logging timestamp.</param>
        /// <param name="level">Log level</param>
        /// <param name="loggerName">Logger name</param>
        /// <param name="formatProvider"><see cref="IFormatProvider"/> object</param>
        /// <param name="message">Log message including parameter placeholders</param>
        /// <param name="parameters">Parameter array.</param>
        /// <param name="exception">Exception information.</param>
        /// <param name="callContext">Call context information dictionary (not interpreted by NLog, suitable for passing call-level context parameters)</param>
        public LogEventInfo(DateTime ts, LogLevel level, string loggerName, IFormatProvider formatProvider, string message, object[] parameters, Exception exception, IDictionary callContext)
        {
            _timeStamp = ts;
            _level = level;
            _loggerName = loggerName;
            _message = message;
            _parameters = parameters;
            _formatProvider = formatProvider;
            _exception = exception;
            _layoutCache = null;
            _sequenceID = Interlocked.Increment(ref _globalSequenceID);
            _callContext = callContext;
            if (_parameters == null || _parameters.Length == 0)
                _formattedMessage = _message;
            else if (_formatProvider != null)
                _formattedMessage = String.Format(_formatProvider, _message, _parameters);
            else
                _formattedMessage = String.Format(_message, _parameters);

#if !NETCF
            _stackTrace = null;
            _userStackFrame = 0;
#endif 
        } 

        /// <summary>
        /// Gets the timestamp of the logging event.
        /// </summary>
        public DateTime TimeStamp
        {
            get
            {
                return _timeStamp;
            }
        }

        /// <summary>
        /// Gets the level of the logging event.
        /// </summary>
        public LogLevel Level
        {
            get
            {
                return _level;
            }
        }

#if !NETCF
        private StackTrace _stackTrace;
        private int _userStackFrame;

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
            get
            {
                return (_stackTrace != null) ? _stackTrace.GetFrame(_userStackFrame): null;
            }
        }

        /// <summary>
        /// Gets the number index of the stack frame that represents the user
        /// code (not the NLog code)
        /// </summary>
        public int UserStackFrameNumber
        {
            get
            {
                return _userStackFrame;
            }
        }

        /// <summary>
        /// Gets the entire stack trace.
        /// </summary>
        public StackTrace StackTrace
        {
            get
            {
                return _stackTrace;
            }
        }
#endif 
        /// <summary>
        /// Gets the exception information.
        /// </summary>
        public Exception Exception
        {
            get
            {
                return _exception;
            }
        }

        /// <summary>
        /// Gets the logger name.
        /// </summary>
        public string LoggerName
        {
            get
            {
                return _loggerName;
            }
        }

        /// <summary>
        /// Gets the logger short name.
        /// </summary>
        public string LoggerShortName
        {
            get
            {
                int lastDot = _loggerName.LastIndexOf('.');
                if (lastDot >= 0)
                    return _loggerName.Substring(lastDot + 1);
                else
                    return _loggerName;
            }
        }

        /// <summary>
        /// Gets the raw log message including any parameter placeholders.
        /// </summary>
        public string Message
        {
            get
            {
                return _message;
            }
        }

        /// <summary>
        /// Gets the parameter values or <see langword="null" /> if no parameters have
        /// been specified.
        /// </summary>
        public object[] Parameters
        {
            get
            {
                return _parameters;
            }
        }

        /// <summary>
        /// Gets the format provider that was provided while logging or <see langword="null" />
        /// when no formatProvider was specified.
        /// </summary>
        public IFormatProvider FormatProvider
        {
            get
            {
                return _formatProvider;
            }
        }

        /// <summary>
        /// Renders the logging message by invoking <see cref="String.Format"/> on
        /// a <see cref="Message"/>, <see cref="Parameters"/> and <see cref="FormatProvider"/>.
        /// </summary>
        public string FormattedMessage
        {
            get 
            {
                return _formattedMessage;
            }
        }

        /// <summary>
        /// Gets the specified call context value.
        /// </summary>
        /// <param name="key">The key value</param>
        /// <returns>Call context value corresponding to the specified key.</returns>
        /// <remarks>
        /// NLog doesn't interpret the call context values in any way. They are
        /// intented to be used within layout renderers and targets.
        /// </remarks>
        public object CallContext(object key)
        {
            if (_callContext == null)
                return null;
            return _callContext[key];
        }

        /// <summary>
        /// The unique identifier of log event which is automatically generated
        /// and monotonously increasing.
        /// </summary>
        public int SequenceID
        {
            get { return _sequenceID; }
        }

        internal string GetCachedLayoutValue(Layout layout)
        {
            if (_layoutCache == null)
                return null;
            string result = (string)_layoutCache[layout];
            return result;
        }

        internal void AddCachedLayoutValue(Layout layout, string value)
        {
            if (_layoutCache == null)
                _layoutCache = new HybridDictionary();
            _layoutCache[layout] = value;
        }
    }
}
