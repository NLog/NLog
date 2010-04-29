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
        private IDictionary _eventContext;
        private int _sequenceID;

        /// <summary>
        /// Creates a new instance of <see cref="LogEventInfo"/>.
        /// </summary>
        public LogEventInfo()
        {
        }

        /// <summary>
        /// Creates the null event.
        /// </summary>
        /// <returns></returns>
        public static LogEventInfo CreateNullEvent()
        {
            return new LogEventInfo(LogLevel.Off, "", "");
        }

        /// <summary>
        /// Creates a new instance of <see cref="LogEventInfo"/> and assigns its fields.
        /// </summary>
        /// <param name="level">Log level</param>
        /// <param name="loggerName">Logger name</param>
        /// <param name="message">Log message including parameter placeholders</param>
        public LogEventInfo(LogLevel level, string loggerName, string message)
            : this(level, loggerName, null, message, null, null)
        {
        }

        /// <summary>
        /// Creates a new instance of <see cref="LogEventInfo"/> and assigns its fields.
        /// </summary>
        /// <param name="level">Log level</param>
        /// <param name="loggerName">Logger name</param>
        /// <param name="formatProvider"><see cref="IFormatProvider"/> object</param>
        /// <param name="message">Log message including parameter placeholders</param>
        /// <param name="parameters">Parameter array.</param>
        public LogEventInfo(LogLevel level, string loggerName, IFormatProvider formatProvider, string message, object[] parameters) 
            : this(level, loggerName, formatProvider, message, parameters, null)
        {
        }

        /// <summary>
        /// Creates a new instance of <see cref="LogEventInfo"/> and assigns its fields.
        /// </summary>
        /// <param name="level">Log level</param>
        /// <param name="loggerName">Logger name</param>
        /// <param name="formatProvider"><see cref="IFormatProvider"/> object</param>
        /// <param name="message">Log message including parameter placeholders</param>
        /// <param name="parameters">Parameter array.</param>
        /// <param name="exception">Exception information.</param>
        public LogEventInfo(LogLevel level, string loggerName, IFormatProvider formatProvider, string message, object[] parameters, Exception exception)
        {
            _timeStamp = CurrentTimeGetter.Now;
            _level = level;
            _loggerName = loggerName;
            _message = message;
            _parameters = parameters;
            _formatProvider = formatProvider;
            _exception = exception;
            _layoutCache = null;
            _sequenceID = Interlocked.Increment(ref _globalSequenceID);
            _formattedMessage = null;

            if (NeedToPreformatMessage(parameters))
                CalcFormattedMessage();

#if !NETCF
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

#if !NETCF
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
        public Exception Exception
        {
            get { return _exception; }
            set { _exception = value; }
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
            get { return _message; }
            set { _message = value; }
        }

        /// <summary>
        /// Gets the parameter values or <see langword="null" /> if no parameters have
        /// been specified.
        /// </summary>
        public object[] Parameters
        {
            get { return _parameters; }
            set { _parameters = value; }
        }

        /// <summary>
        /// Gets the format provider that was provided while logging or <see langword="null" />
        /// when no formatProvider was specified.
        /// </summary>
        public IFormatProvider FormatProvider
        {
            get { return _formatProvider; }
            set { _formatProvider = value; }
        }

        /// <summary>
        /// Returns the formatted message.
        /// </summary>
        public string FormattedMessage
        {
            get 
            {
                if (_formattedMessage == null)
                    CalcFormattedMessage();

                return _formattedMessage;
            }
        }

        /// <summary>
        /// Gets the dictionary of per-event context properties.
        /// </summary>
        public IDictionary Context
        {
            get
            {
                if (_eventContext == null)
                    _eventContext = new HybridDictionary();
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

        internal string GetCachedLayoutValue(ILayout layout)
        {
            if (_layoutCache == null)
                return null;
            string result = (string)_layoutCache[layout];
            return result;
        }

        internal void AddCachedLayoutValue(ILayout layout, string value)
        {
            if (_layoutCache == null)
                _layoutCache = new HybridDictionary();
            _layoutCache[layout] = value;
        }

        private void CalcFormattedMessage()
        {
            _formattedMessage = _message;

            if (_parameters == null || _parameters.Length == 0)
                return;

            if (_formatProvider != null)
                _formattedMessage = String.Format(_formatProvider, _message, _parameters);
            else
                _formattedMessage = String.Format(_message, _parameters);
        }

        private bool NeedToPreformatMessage(object[] parameters)
        {
            // we need to preformat message if it contains any parameters which could possibly
            // do logging in their ToString()
            if (parameters == null)
                return false;
            
            if (parameters.Length == 0)
                return false;
            
            if (parameters.Length > 3)
            {
                // too many parameters, too costly to check
                return true;
            }

            if (!IsSafeToDeferFormatting(parameters[0]))
                return true;
            if (parameters.Length >= 2)
            {
                if (!IsSafeToDeferFormatting(parameters[1]))
                    return true;
            }
            if (parameters.Length >= 3)
            {
                if (!IsSafeToDeferFormatting(parameters[2]))
                    return true;
            }
            return false;
        }

        private bool IsSafeToDeferFormatting(object value)
        {
            if (value == null)
                return true;

            return (value.GetType().IsPrimitive || value is string);
        }
    }
}
