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
    public class FormattedLogEventInfo : LogEventInfo
    {
        private IFormatProvider _formatProvider;
        private string _message;
        private object[] _parameters;
        private string _formattedMessage;

        public FormattedLogEventInfo(LogLevel level, string loggerName, IFormatProvider formatProvider, string message, object[] parameters)
            : base(level, loggerName)
        {
            _formatProvider = formatProvider;
            _message = message;
            _parameters = parameters;

            if (NeedToPreformatMessage(parameters))
                CalcFormattedMessage();
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

        public override string FormattedMessage
        {
            get
            { 
                if (_formattedMessage == null)
                    CalcFormattedMessage();
                return _formattedMessage;
            }
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

        internal static bool NeedToPreformatMessage(object[] parameters)
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

        private static bool IsSafeToDeferFormatting(object value)
        {
            if (value == null)
                return true;

            return (value.GetType().IsPrimitive || value is string);
        }
    }
}