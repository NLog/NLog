// 
// Copyright (c) 2004-2018 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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

namespace NLog
{
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using JetBrains.Annotations;

    /// <summary>
    /// Provides logging interface and utility functions.
    /// </summary>
    public partial class LoggerCallSite
    {
        /// <summary>
        /// Writes log-event at specified level using provided message
        /// </summary>
        /// <param name="level">The log level.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        public void Log(LogLevel level, [Localizable(false)] string message
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0) 
        { 
            if (IsEnabled(level))
            {
                var logEvent = new LogEventInfo(level, Name, null, message, null, null);
                if (IsCallSiteEnabled(level))
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at specified level using provided exception and message
        /// </summary>
        /// <param name="level">The log level.</param>
        /// <param name="exception">Exception to be logged.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        public void Log(LogLevel level, Exception exception, [Localizable(false)] string message
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0) 
        { 
            if (IsEnabled(level))
            {
                var logEvent = new LogEventInfo(level, Name, null, message, null, exception);
                if (IsCallSiteEnabled(level))
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at specified level using provided generic value
        /// </summary>
        /// <typeparam name="T">The type of the argument.</typeparam>
        /// <param name="level">The log level.</param>
        /// <param name="value">The value to be written.</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        public void Log<T>(LogLevel level, T value
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsEnabled(level))
            {
                var logEvent = LogEventInfo.Create(level, Name, null, value);
                if (IsCallSiteEnabled(level))
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at specified level using provided generic value
        /// </summary>
        /// <typeparam name="T">The type of the argument.</typeparam>
        /// <param name="level">The log level.</param>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="value">The value to be written.</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        public void Log<T>(LogLevel level, IFormatProvider formatProvider, T value
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsEnabled(level))
            {
                var logEvent = LogEventInfo.Create(level, Name, formatProvider, value);
                if (IsCallSiteEnabled(level))
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at specified level using provided delegate
        /// </summary>
        /// <param name="level">The log level.</param>
        /// <param name="messageFunc">A function returning message to be written. Function is not evaluated if logging is not enabled.</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        public void Log(LogLevel level, LogMessageGenerator messageFunc
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsEnabled(level))
            {
                if (messageFunc == null)
                {
                    throw new ArgumentNullException("messageFunc");
                }

                var logEvent = new LogEventInfo(level, Name, null, messageFunc(), null, null);
                if (IsCallSiteEnabled(level))
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at specified level using provided exception and message-delegate
        /// </summary>
        /// <param name="level">The log level.</param>
        /// <param name="exception">Exception to be logged.</param>
        /// <param name="messageFunc">A function returning message to be written. Function is not evaluated if logging is not enabled.</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        public void Log(LogLevel level, Exception exception, LogMessageGenerator messageFunc
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsEnabled(level))
            {
                if (messageFunc == null)
                {
                    throw new ArgumentNullException("messageFunc");
                }

                var logEvent = new LogEventInfo(level, Name, null, messageFunc(), null, exception);
                if (IsCallSiteEnabled(level))
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at specified level using provided message (without CallSite details)
        /// </summary>
        /// <param name="level">The log level.</param>
        /// <param name="message">Log event mesage</param>
        /// <param name="args">Arguments to format.</param>
        [MessageTemplateFormatMethod("message")]
        public void Log(LogLevel level, [Localizable(false)] string message, params object[] args)
        {
            if (IsEnabled(level))
            {
                var logEvent = new LogEventInfo(level, Name, null, message, args, null);
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at specified level using provided exception and message (without CallSite details)
        /// </summary>
        /// <param name="level">The log level.</param>
        /// <param name="exception">Exception to be logged.</param>
        /// <param name="message">Log event mesage</param>
        /// <param name="args">Arguments to format.</param>
        [MessageTemplateFormatMethod("message")]
        public void Log(LogLevel level, Exception exception, [Localizable(false)] string message, params object[] args)
        {
            if (IsEnabled(level))
            {
                var logEvent = new LogEventInfo(level, Name, null, message, args, exception);
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at specified level using provided exception and message (without CallSite details)
        /// </summary>
        /// <param name="level">The log level.</param>
        /// <param name="exception">Exception to be logged.</param>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">Log event mesage</param>
        /// <param name="args">Arguments to format.</param>
        [MessageTemplateFormatMethod("message")]
        public void Log(LogLevel level, Exception exception, IFormatProvider formatProvider, [Localizable(false)] string message, params object[] args)
        {
            if (IsEnabled(level))
            {
                var logEvent = new LogEventInfo(level, Name, formatProvider, message, args, exception);
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at specified level using provided message (without CallSite details)
        /// </summary>
        /// <param name="level">The log level.</param>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">Log event mesage</param>
        /// <param name="args">Arguments to format.</param>
        [MessageTemplateFormatMethod("message")]
        public void Log(LogLevel level, IFormatProvider formatProvider, [Localizable(false)] string message, params object[] args)
        {
            if (IsEnabled(level))
            {
                var logEvent = new LogEventInfo(level, Name, formatProvider, message, args, null);
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at specified level using provided message
        /// </summary>
        /// <typeparam name="TArgument1">The type of the argument.</typeparam>
        /// <param name="level">The log level.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="arg1">The argument to format.</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        [MessageTemplateFormatMethod("message")]
        public void Log<TArgument1>(LogLevel level, [Localizable(false)] string message, TArgument1 arg1
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0) 
        {
            if (IsEnabled(level))
            {
                var logEvent = new LogEventInfo(level, Name, null, message, new object[] { arg1 }, null);
                if (IsCallSiteEnabled(level))
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at specified level using provided exception and message
        /// </summary>
        /// <typeparam name="TArgument1">The type of the argument.</typeparam>
        /// <param name="level">The log level.</param>
        /// <param name="exception">Exception to be logged.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="arg1">The argument to format.</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        [MessageTemplateFormatMethod("message")]
        public void Log<TArgument1>(LogLevel level, Exception exception, [Localizable(false)] string message, TArgument1 arg1
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0) 
        { 
            if (IsEnabled(level))
            {
                var logEvent = new LogEventInfo(level, Name, null, message, new object[] { arg1 }, exception);
                if (IsCallSiteEnabled(level))
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at specified level using provided exception and message
        /// </summary>
        /// <typeparam name="TArgument1">The type of the argument.</typeparam>
        /// <param name="level">The log level.</param>
        /// <param name="exception">Exception to be logged.</param>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="arg1">The argument to format.</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        [MessageTemplateFormatMethod("message")]
        public void Log<TArgument1>(LogLevel level, Exception exception, IFormatProvider formatProvider, [Localizable(false)] string message, TArgument1 arg1
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0) 
        { 
            if (IsEnabled(level))
            {
                var logEvent = new LogEventInfo(level, Name, formatProvider, message, new object[] { arg1 }, exception);
                if (IsCallSiteEnabled(level))
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at specified level using provided message
        /// </summary>
        /// <typeparam name="TArgument1">The type of the argument.</typeparam>
        /// <param name="level">The log level.</param>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="arg1">The argument to format.</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        [MessageTemplateFormatMethod("message")]
        public void Log<TArgument1>(LogLevel level, IFormatProvider formatProvider, [Localizable(false)] string message, TArgument1 arg1
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0) 
        { 
            if (IsEnabled(level))
            {
                var logEvent = new LogEventInfo(level, Name, formatProvider, message, new object[] { arg1 }, null);
                if (IsCallSiteEnabled(level))
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at specified level using provided message
        /// </summary>
        /// <typeparam name="TArgument1">The type of the argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the argument.</typeparam>
        /// <param name="level">The log level.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="arg1">The argument to format.</param>
        /// <param name="arg2">The argument to format.</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        [MessageTemplateFormatMethod("message")]
        public void Log<TArgument1, TArgument2>(LogLevel level, [Localizable(false)] string message, TArgument1 arg1, TArgument2 arg2
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0) 
        { 
            if (IsEnabled(level))
            {
                var logEvent = new LogEventInfo(level, Name, null, message, new object[] { arg1, arg2 }, null);
                if (IsCallSiteEnabled(level))
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at specified level using provided exception and message
        /// </summary>
        /// <typeparam name="TArgument1">The type of the argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the argument.</typeparam>
        /// <param name="level">The log level.</param>
        /// <param name="exception">Exception to be logged.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="arg1">The argument to format.</param>
        /// <param name="arg2">The argument to format.</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        [MessageTemplateFormatMethod("message")]
        public void Log<TArgument1, TArgument2>(LogLevel level, Exception exception, [Localizable(false)] string message, TArgument1 arg1, TArgument2 arg2
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)  
        { 
            if (IsEnabled(level))
            {
                var logEvent = new LogEventInfo(level, Name, null, message, new object[] { arg1, arg2 }, exception);
                if (IsCallSiteEnabled(level))
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at specified level using provided exception and message
        /// </summary>
        /// <typeparam name="TArgument1">The type of the argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the argument.</typeparam>
        /// <param name="level">The log level.</param>
        /// <param name="exception">Exception to be logged.</param>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="arg1">The argument to format.</param>
        /// <param name="arg2">The argument to format.</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        [MessageTemplateFormatMethod("message")]
        public void Log<TArgument1, TArgument2>(LogLevel level, Exception exception, IFormatProvider formatProvider, [Localizable(false)] string message, TArgument1 arg1, TArgument2 arg2
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)  
        { 
            if (IsEnabled(level))
            {
                var logEvent = new LogEventInfo(level, Name, formatProvider, message, new object[] { arg1, arg2 }, exception);
                if (IsCallSiteEnabled(level))
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at specified level using provided message
        /// </summary>
        /// <typeparam name="TArgument1">The type of the argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the argument.</typeparam>
        /// <param name="level">The log level.</param>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="arg1">The argument to format.</param>
        /// <param name="arg2">The argument to format.</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        [MessageTemplateFormatMethod("message")]
        public void Log<TArgument1, TArgument2>(LogLevel level, IFormatProvider formatProvider, [Localizable(false)] string message, TArgument1 arg1, TArgument2 arg2
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)  
        { 
            if (IsEnabled(level))
            {
                var logEvent = new LogEventInfo(level, Name, formatProvider, message, new object[] { arg1, arg2 }, null);
                if (IsCallSiteEnabled(level))
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at specified level using provided message
        /// </summary>
        /// <typeparam name="TArgument1">The type of the argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the argument.</typeparam>
        /// <typeparam name="TArgument3">The type of the argument.</typeparam>
        /// <param name="level">The log level.</param>
        /// <param name="message">Log event mesage</param>
        /// <param name="arg1">The argument to format.</param>
        /// <param name="arg2">The argument to format.</param>
        /// <param name="arg3">The argument to format.</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        [MessageTemplateFormatMethod("message")]
        public void Log<TArgument1, TArgument2, TArgument3>(LogLevel level, [Localizable(false)] string message, TArgument1 arg1, TArgument2 arg2, TArgument3 arg3
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0) 
        { 
            if (IsEnabled(level))
            {
                var logEvent = new LogEventInfo(level, Name, null, message, new object[] { arg1, arg2, arg3 }, null);
                if (IsCallSiteEnabled(level))
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at specified level using provided exception and message
        /// </summary>
        /// <typeparam name="TArgument1">The type of the argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the argument.</typeparam>
        /// <typeparam name="TArgument3">The type of the argument.</typeparam>
        /// <param name="level">The log level.</param>
        /// <param name="exception">Exception to be logged.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="arg1">The argument to format.</param>
        /// <param name="arg2">The argument to format.</param>
        /// <param name="arg3">The argument to format.</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        [MessageTemplateFormatMethod("message")]
        public void Log<TArgument1, TArgument2, TArgument3>(LogLevel level, Exception exception, [Localizable(false)] string message, TArgument1 arg1, TArgument2 arg2, TArgument3 arg3
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)  
        { 
            if (IsEnabled(level))
            {
                var logEvent = new LogEventInfo(level, Name, null, message, new object[] { arg1, arg2, arg3 }, exception);
                if (IsCallSiteEnabled(level))
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at specified level using provided exception and message
        /// </summary>
        /// <typeparam name="TArgument1">The type of the argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the argument.</typeparam>
        /// <typeparam name="TArgument3">The type of the argument.</typeparam>
        /// <param name="level">The log level.</param>
        /// <param name="exception">Exception to be logged.</param>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="arg1">The argument to format.</param>
        /// <param name="arg2">The argument to format.</param>
        /// <param name="arg3">The argument to format.</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        [MessageTemplateFormatMethod("message")]
        public void Log<TArgument1, TArgument2, TArgument3>(LogLevel level, Exception exception, IFormatProvider formatProvider, [Localizable(false)] string message, TArgument1 arg1, TArgument2 arg2, TArgument3 arg3
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)  
        { 
            if (IsEnabled(level))
            {
                var logEvent = new LogEventInfo(level, Name, formatProvider, message, new object[] { arg1, arg2, arg3 }, exception);
                if (IsCallSiteEnabled(level))
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at specified level using provided message
        /// </summary>
        /// <typeparam name="TArgument1">The type of the argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the argument.</typeparam>
        /// <typeparam name="TArgument3">The type of the argument.</typeparam>
        /// <param name="level">The log level.</param>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="arg1">The argument to format.</param>
        /// <param name="arg2">The argument to format.</param>
        /// <param name="arg3">The argument to format.</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        [MessageTemplateFormatMethod("message")]
        public void Log<TArgument1, TArgument2, TArgument3>(LogLevel level, IFormatProvider formatProvider, [Localizable(false)] string message, TArgument1 arg1, TArgument2 arg2, TArgument3 arg3
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)  
        { 
            if (IsEnabled(level))
            {
                var logEvent = new LogEventInfo(level, Name, formatProvider, message, new object[] { arg1, arg2, arg3 }, null);
                if (IsCallSiteEnabled(level))
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at specified level using provided generic value
        /// </summary>
        /// <typeparam name="T">The type of the argument.</typeparam>
        /// <param name="value">The value to be written.</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        public void Trace<T>(T value
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsTraceEnabled)
            {
                var logEvent = LogEventInfo.Create(LogLevel.Trace, Name, null, value);
                if (_isTraceCallSiteEnabled)
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at specified level using provided generic value
        /// </summary>
        /// <typeparam name="T">The type of the argument.</typeparam>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="value">The value to be written.</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        public void Trace<T>(IFormatProvider formatProvider, T value
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsTraceEnabled)
            {
                var logEvent = LogEventInfo.Create(LogLevel.Trace, Name, formatProvider, value);
                if (_isTraceCallSiteEnabled)
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }


        /// <summary>
        /// Writes log-event at <c>Trace</c> level using provided message-delegate
        /// </summary>
        /// <param name="messageFunc">A function returning message to be written. Function is not evaluated if logging is not enabled.</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        public void Trace(LogMessageGenerator messageFunc
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsTraceEnabled)
            {
                if (messageFunc == null)
                {
                    throw new ArgumentNullException("messageFunc");
                }

                var logEvent = new LogEventInfo(LogLevel.Trace, Name, null, messageFunc(), null, null);
                if (_isTraceCallSiteEnabled)
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Trace</c> level using provided exception and message-delegate
        /// </summary>
        /// <param name="exception">Exception to be logged.</param>
        /// <param name="messageFunc">A function returning message to be written. Function is not evaluated if logging is not enabled.</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        public void Trace(Exception exception, LogMessageGenerator messageFunc
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsTraceEnabled)
            {
                if (messageFunc == null)
                {
                    throw new ArgumentNullException("messageFunc");
                }

                var logEvent = new LogEventInfo(LogLevel.Trace, Name, null, messageFunc(), null, exception);
                if (_isTraceCallSiteEnabled)
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Trace</c> level using provided message
        /// </summary>
        /// <param name="message">Log event mesage</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        [MessageTemplateFormatMethod("message")]
        public void Trace([Localizable(false)] string message
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsTraceEnabled)
            {
                var logEvent = new LogEventInfo(LogLevel.Trace, Name, message);
                if (_isTraceCallSiteEnabled)
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Trace</c> level using provided exception and message
        /// </summary>
        /// <param name="exception">Exception to be logged.</param>
        /// <param name="message">Log event mesage</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        [MessageTemplateFormatMethod("message")]
        public void Trace(Exception exception, [Localizable(false)] string message
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsTraceEnabled)
            {
                var logEvent = new LogEventInfo(LogLevel.Trace, Name, null, message, null, exception);
                if (_isTraceCallSiteEnabled)
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Trace</c> level using provided message (without CallSite details)
        /// </summary>
        /// <param name="message">Log event mesage</param>
        /// <param name="args">Arguments to format.</param>
        [MessageTemplateFormatMethod("message")]
        public void Trace([Localizable(false)] string message, params object[] args)
        { 
            if (IsTraceEnabled)
            {
                var logEvent = new LogEventInfo(LogLevel.Trace, Name, null, message, args, null);
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Trace</c> level using provided message (without CallSite details)
        /// </summary>
        /// <param name="exception">Exception to be logged.</param>
        /// <param name="message">Log event mesage</param>
        /// <param name="args">Arguments to format.</param>
        [MessageTemplateFormatMethod("message")]
        public void Trace(Exception exception, [Localizable(false)] string message, params object[] args)
        { 
            if (IsTraceEnabled)
            {
                var logEvent = new LogEventInfo(LogLevel.Trace, Name, null, message, args, exception);
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Trace</c> level using provided exeption and message (without CallSite details)
        /// </summary>
        /// <param name="exception">Exception to be logged.</param>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">Log event mesage</param>
        /// <param name="args">Arguments to format.</param>
        [MessageTemplateFormatMethod("message")]
        public void Trace(Exception exception, IFormatProvider formatProvider, [Localizable(false)] string message, params object[] args)
        { 
            if (IsTraceEnabled)
            {
                var logEvent = new LogEventInfo(LogLevel.Trace, Name, formatProvider, message, args, exception);
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Trace</c> level using provided message (without CallSite details)
        /// </summary>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">Log event mesage</param>
        /// <param name="args">Arguments to format.</param>
        [MessageTemplateFormatMethod("message")]
        public void Trace(IFormatProvider formatProvider, [Localizable(false)] string message, params object[] args)
        { 
            if (IsTraceEnabled)
            {
                var logEvent = new LogEventInfo(LogLevel.Trace, Name, formatProvider, message, args, null);
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Trace</c> level using provided message
        /// </summary>
        /// <typeparam name="TArgument1">The type of the argument.</typeparam>
        /// <param name="message">Log event mesage</param>
        /// <param name="arg1">Arguments to format</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        [MessageTemplateFormatMethod("message")]
        public void Trace<TArgument1>([Localizable(false)] string message, TArgument1 arg1
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsTraceEnabled)
            {
                var logEvent = new LogEventInfo(LogLevel.Trace, Name, null, message, new object[] { arg1 });
                if (_isTraceCallSiteEnabled)
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Trace</c> level using provided exception and message
        /// </summary>
        /// <typeparam name="TArgument1">The type of the argument.</typeparam>
        /// <param name="exception">Exception to be logged.</param>
        /// <param name="message">Log event mesage</param>
        /// <param name="arg1">Arguments to format</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        [MessageTemplateFormatMethod("message")]
        public void Trace<TArgument1>(Exception exception, [Localizable(false)] string message, TArgument1 arg1
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsTraceEnabled)
            {
                var logEvent = new LogEventInfo(LogLevel.Trace, Name, null, message, new object[] { arg1 }, exception);
                if (_isTraceCallSiteEnabled)
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Trace</c> level using provided exception and message
        /// </summary>
        /// <typeparam name="TArgument1">The type of the argument.</typeparam>
        /// <param name="exception">Exception to be logged.</param>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">Log event mesage</param>
        /// <param name="arg1">Arguments to format</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        [MessageTemplateFormatMethod("message")]
        public void Trace<TArgument1>(Exception exception, IFormatProvider formatProvider, [Localizable(false)] string message, TArgument1 arg1
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsTraceEnabled)
            {
                var logEvent = new LogEventInfo(LogLevel.Trace, Name, formatProvider, message, new object[] { arg1 }, exception);
                if (_isTraceCallSiteEnabled)
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Trace</c> level using provided message
        /// </summary>
        /// <typeparam name="TArgument1">The type of the argument.</typeparam>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">Log event mesage</param>
        /// <param name="arg1">Arguments to format</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        [MessageTemplateFormatMethod("message")]
        public void Trace<TArgument1>(IFormatProvider formatProvider, [Localizable(false)] string message, TArgument1 arg1
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsTraceEnabled)
            {
                var logEvent = new LogEventInfo(LogLevel.Trace, Name, formatProvider, message, new object[] { arg1 }, null);
                if (_isTraceCallSiteEnabled)
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Trace</c> level using provided message
        /// </summary>
        /// <typeparam name="TArgument1">The type of the argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the argument.</typeparam>
        /// <param name="message">Log event mesage</param>
        /// <param name="arg1">Arguments to format</param>
        /// <param name="arg2">Arguments to format</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        [MessageTemplateFormatMethod("message")]
        public void Trace<TArgument1, TArgument2>([Localizable(false)] string message, TArgument1 arg1, TArgument2 arg2
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsTraceEnabled)
            {
                var logEvent = new LogEventInfo(LogLevel.Trace, Name, null, message, new object[] { arg1, arg2 });
                if (_isTraceCallSiteEnabled)
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Trace</c> level using provided exception and message
        /// </summary>
        /// <typeparam name="TArgument1">The type of the argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the argument.</typeparam>
        /// <param name="exception">Exception to be logged.</param>
        /// <param name="message">Log event mesage</param>
        /// <param name="arg1">Arguments to format</param>
        /// <param name="arg2">Arguments to format</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        [MessageTemplateFormatMethod("message")]
        public void Trace<TArgument1, TArgument2>(Exception exception, [Localizable(false)] string message, TArgument1 arg1, TArgument2 arg2
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsTraceEnabled)
            {
                var logEvent = new LogEventInfo(LogLevel.Trace, Name, null, message, new object[] { arg1, arg2 }, exception);
                if (_isTraceCallSiteEnabled)
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Trace</c> level using provided exception and message
        /// </summary>
        /// <typeparam name="TArgument1">The type of the argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the argument.</typeparam>
        /// <param name="exception">Exception to be logged.</param>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">Log event mesage</param>
        /// <param name="arg1">Arguments to format</param>
        /// <param name="arg2">Arguments to format</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        [MessageTemplateFormatMethod("message")]
        public void Trace<TArgument1, TArgument2>(Exception exception, IFormatProvider formatProvider, [Localizable(false)] string message, TArgument1 arg1, TArgument2 arg2
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsTraceEnabled)
            {
                var logEvent = new LogEventInfo(LogLevel.Trace, Name, formatProvider, message, new object[] { arg1, arg2 }, exception);
                if (_isTraceCallSiteEnabled)
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Trace</c> level using provided exception and message
        /// </summary>
        /// <typeparam name="TArgument1">The type of the argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the argument.</typeparam>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">Log event mesage</param>
        /// <param name="arg1">Arguments to format</param>
        /// <param name="arg2">Arguments to format</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        [MessageTemplateFormatMethod("message")]
        public void Trace<TArgument1, TArgument2>(IFormatProvider formatProvider, [Localizable(false)] string message, TArgument1 arg1, TArgument2 arg2
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsTraceEnabled)
            {
                var logEvent = new LogEventInfo(LogLevel.Trace, Name, formatProvider, message, new object[] { arg1, arg2 }, null);
                if (_isTraceCallSiteEnabled)
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Trace</c> level using provided message
        /// </summary>
        /// <typeparam name="TArgument1">The type of the argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the argument.</typeparam>
        /// <typeparam name="TArgument3">The type of the argument.</typeparam>
        /// <param name="message">Log event mesage</param>
        /// <param name="arg1">Arguments to format</param>
        /// <param name="arg2">Arguments to format</param>
        /// <param name="arg3">Arguments to format</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        [MessageTemplateFormatMethod("message")]
        public void Trace<TArgument1, TArgument2, TArgument3>([Localizable(false)] string message, TArgument1 arg1, TArgument2 arg2, TArgument3 arg3
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsTraceEnabled)
            {
                var logEvent = new LogEventInfo(LogLevel.Trace, Name, null, message, new object[] { arg1, arg2, arg3 });
                if (_isTraceCallSiteEnabled)
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Trace</c> level using provided exception and message
        /// </summary>
        /// <typeparam name="TArgument1">The type of the argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the argument.</typeparam>
        /// <typeparam name="TArgument3">The type of the argument.</typeparam>
        /// <param name="exception">Exception to be logged.</param>
        /// <param name="message">Log event mesage</param>
        /// <param name="arg1">Arguments to format</param>
        /// <param name="arg2">Arguments to format</param>
        /// <param name="arg3">Arguments to format</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        [MessageTemplateFormatMethod("message")]
        public void Trace<TArgument1, TArgument2, TArgument3>(Exception exception, [Localizable(false)] string message, TArgument1 arg1, TArgument2 arg2, TArgument3 arg3
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsTraceEnabled)
            {
                var logEvent = new LogEventInfo(LogLevel.Trace, Name, null, message, new object[] { arg1, arg2, arg3 }, exception);
                if (_isTraceCallSiteEnabled)
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Trace</c> level using provided exception and message
        /// </summary>
        /// <typeparam name="TArgument1">The type of the argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the argument.</typeparam>
        /// <typeparam name="TArgument3">The type of the argument.</typeparam>
        /// <param name="exception">Exception to be logged.</param>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">Log event mesage</param>
        /// <param name="arg1">Arguments to format</param>
        /// <param name="arg2">Arguments to format</param>
        /// <param name="arg3">Arguments to format</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        [MessageTemplateFormatMethod("message")]
        public void Trace<TArgument1, TArgument2, TArgument3>(Exception exception, IFormatProvider formatProvider, [Localizable(false)] string message, TArgument1 arg1, TArgument2 arg2, TArgument3 arg3
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsTraceEnabled)
            {
                var logEvent = new LogEventInfo(LogLevel.Trace, Name, formatProvider, message, new object[] { arg1, arg2, arg3 }, exception);
                if (_isTraceCallSiteEnabled)
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Trace</c> level using provided message
        /// </summary>
        /// <typeparam name="TArgument1">The type of the argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the argument.</typeparam>
        /// <typeparam name="TArgument3">The type of the argument.</typeparam>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">Log event mesage</param>
        /// <param name="arg1">Arguments to format</param>
        /// <param name="arg2">Arguments to format</param>
        /// <param name="arg3">Arguments to format</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        [MessageTemplateFormatMethod("message")]
        public void Trace<TArgument1, TArgument2, TArgument3>(IFormatProvider formatProvider, [Localizable(false)] string message, TArgument1 arg1, TArgument2 arg2, TArgument3 arg3
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsTraceEnabled)
            {
                var logEvent = new LogEventInfo(LogLevel.Trace, Name, formatProvider, message, new object[] { arg1, arg2, arg3 }, null);
                if (_isTraceCallSiteEnabled)
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Trace</c> level using provided message
        /// </summary>
        /// <typeparam name="TArgument1">The type of the argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the argument.</typeparam>
        /// <typeparam name="TArgument3">The type of the argument.</typeparam>
        /// <typeparam name="TArgument4">The type of the argument.</typeparam>
        /// <param name="message">Log event mesage</param>
        /// <param name="arg1">Arguments to format</param>
        /// <param name="arg2">Arguments to format</param>
        /// <param name="arg3">Arguments to format</param>
        /// <param name="arg4">Arguments to format</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        [MessageTemplateFormatMethod("message")]
        public void Trace<TArgument1, TArgument2, TArgument3, TArgument4>([Localizable(false)] string message, TArgument1 arg1, TArgument2 arg2, TArgument3 arg3, TArgument4 arg4
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsTraceEnabled)
            {
                var logEvent = new LogEventInfo(LogLevel.Trace, Name, null, message, new object[] { arg1, arg2, arg3, arg4 });
                if (_isTraceCallSiteEnabled)
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Trace</c> level using provided exception and message
        /// </summary>
        /// <typeparam name="TArgument1">The type of the argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the argument.</typeparam>
        /// <typeparam name="TArgument3">The type of the argument.</typeparam>
        /// <typeparam name="TArgument4">The type of the argument.</typeparam>
        /// <param name="exception">Exception to be logged.</param>
        /// <param name="message">Log event mesage</param>
        /// <param name="arg1">Arguments to format</param>
        /// <param name="arg2">Arguments to format</param>
        /// <param name="arg3">Arguments to format</param>
        /// <param name="arg4">Arguments to format</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        [MessageTemplateFormatMethod("message")]
        public void Trace<TArgument1, TArgument2, TArgument3, TArgument4>(Exception exception, [Localizable(false)] string message, TArgument1 arg1, TArgument2 arg2, TArgument3 arg3, TArgument4 arg4
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsTraceEnabled)
            {
                var logEvent = new LogEventInfo(LogLevel.Trace, Name, null, message, new object[] { arg1, arg2, arg3, arg4 }, exception);
                if (_isTraceCallSiteEnabled)
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Trace</c> level using provided exception and message
        /// </summary>
        /// <typeparam name="TArgument1">The type of the argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the argument.</typeparam>
        /// <typeparam name="TArgument3">The type of the argument.</typeparam>
        /// <typeparam name="TArgument4">The type of the argument.</typeparam>
        /// <param name="exception">Exception to be logged.</param>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">Log event mesage</param>
        /// <param name="arg1">Arguments to format</param>
        /// <param name="arg2">Arguments to format</param>
        /// <param name="arg3">Arguments to format</param>
        /// <param name="arg4">Arguments to format</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        [MessageTemplateFormatMethod("message")]
        public void Trace<TArgument1, TArgument2, TArgument3, TArgument4>(Exception exception, IFormatProvider formatProvider, [Localizable(false)] string message, TArgument1 arg1, TArgument2 arg2, TArgument3 arg3, TArgument4 arg4
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsTraceEnabled)
            {
                var logEvent = new LogEventInfo(LogLevel.Trace, Name, formatProvider, message, new object[] { arg1, arg2, arg3, arg4 }, exception);
                if (_isTraceCallSiteEnabled)
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Trace</c> level using provided message
        /// </summary>
        /// <typeparam name="TArgument1">The type of the argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the argument.</typeparam>
        /// <typeparam name="TArgument3">The type of the argument.</typeparam>
        /// <typeparam name="TArgument4">The type of the argument.</typeparam>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">Log event mesage</param>
        /// <param name="arg1">Arguments to format</param>
        /// <param name="arg2">Arguments to format</param>
        /// <param name="arg3">Arguments to format</param>
        /// <param name="arg4">Arguments to format</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        [MessageTemplateFormatMethod("message")]
        public void Trace<TArgument1, TArgument2, TArgument3, TArgument4>(IFormatProvider formatProvider, [Localizable(false)] string message, TArgument1 arg1, TArgument2 arg2, TArgument3 arg3, TArgument4 arg4
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsTraceEnabled)
            {
                var logEvent = new LogEventInfo(LogLevel.Trace, Name, formatProvider, message, new object[] { arg1, arg2, arg3, arg4 }, null);
                if (_isTraceCallSiteEnabled)
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Trace</c> level using provided message
        /// </summary>
        /// <typeparam name="TArgument1">The type of the argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the argument.</typeparam>
        /// <typeparam name="TArgument3">The type of the argument.</typeparam>
        /// <typeparam name="TArgument4">The type of the argument.</typeparam>
        /// <typeparam name="TArgument5">The type of the argument.</typeparam>
        /// <param name="message">Log event mesage</param>
        /// <param name="arg1">Arguments to format</param>
        /// <param name="arg2">Arguments to format</param>
        /// <param name="arg3">Arguments to format</param>
        /// <param name="arg4">Arguments to format</param>
        /// <param name="arg5">Arguments to format</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        [MessageTemplateFormatMethod("message")]
        public void Trace<TArgument1, TArgument2, TArgument3, TArgument4, TArgument5>([Localizable(false)] string message, TArgument1 arg1, TArgument2 arg2, TArgument3 arg3, TArgument4 arg4, TArgument5 arg5
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsTraceEnabled)
            {
                var logEvent = new LogEventInfo(LogLevel.Trace, Name, null, message, new object[] { arg1, arg2, arg3, arg4, arg5 });
                if (_isTraceCallSiteEnabled)
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Trace</c> level using provided exception and message
        /// </summary>
        /// <typeparam name="TArgument1">The type of the argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the argument.</typeparam>
        /// <typeparam name="TArgument3">The type of the argument.</typeparam>
        /// <typeparam name="TArgument4">The type of the argument.</typeparam>
        /// <typeparam name="TArgument5">The type of the argument.</typeparam>
        /// <param name="exception">Exception to be logged.</param>
        /// <param name="message">Log event mesage</param>
        /// <param name="arg1">Arguments to format</param>
        /// <param name="arg2">Arguments to format</param>
        /// <param name="arg3">Arguments to format</param>
        /// <param name="arg4">Arguments to format</param>
        /// <param name="arg5">Arguments to format</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        [MessageTemplateFormatMethod("message")]
        public void Trace<TArgument1, TArgument2, TArgument3, TArgument4, TArgument5>(Exception exception, [Localizable(false)] string message, TArgument1 arg1, TArgument2 arg2, TArgument3 arg3, TArgument4 arg4, TArgument5 arg5
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsTraceEnabled)
            {
                var logEvent = new LogEventInfo(LogLevel.Trace, Name, null, message, new object[] { arg1, arg2, arg3, arg4, arg5 }, exception);
                if (_isTraceCallSiteEnabled)
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Trace</c> level using provided exception and message
        /// </summary>
        /// <typeparam name="TArgument1">The type of the argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the argument.</typeparam>
        /// <typeparam name="TArgument3">The type of the argument.</typeparam>
        /// <typeparam name="TArgument4">The type of the argument.</typeparam>
        /// <typeparam name="TArgument5">The type of the argument.</typeparam>
        /// <param name="exception">Exception to be logged.</param>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">Log event mesage</param>
        /// <param name="arg1">Arguments to format</param>
        /// <param name="arg2">Arguments to format</param>
        /// <param name="arg3">Arguments to format</param>
        /// <param name="arg4">Arguments to format</param>
        /// <param name="arg5">Arguments to format</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        [MessageTemplateFormatMethod("message")]
        public void Trace<TArgument1, TArgument2, TArgument3, TArgument4, TArgument5>(Exception exception, IFormatProvider formatProvider, [Localizable(false)] string message, TArgument1 arg1, TArgument2 arg2, TArgument3 arg3, TArgument4 arg4, TArgument5 arg5
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsTraceEnabled)
            {
                var logEvent = new LogEventInfo(LogLevel.Trace, Name, formatProvider, message, new object[] { arg1, arg2, arg3, arg4, arg5 }, exception);
                if (_isTraceCallSiteEnabled)
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Trace</c> level using provided message
        /// </summary>
        /// <typeparam name="TArgument1">The type of the argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the argument.</typeparam>
        /// <typeparam name="TArgument3">The type of the argument.</typeparam>
        /// <typeparam name="TArgument4">The type of the argument.</typeparam>
        /// <typeparam name="TArgument5">The type of the argument.</typeparam>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">Log event mesage</param>
        /// <param name="arg1">Arguments to format</param>
        /// <param name="arg2">Arguments to format</param>
        /// <param name="arg3">Arguments to format</param>
        /// <param name="arg4">Arguments to format</param>
        /// <param name="arg5">Arguments to format</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        [MessageTemplateFormatMethod("message")]
        public void Trace<TArgument1, TArgument2, TArgument3, TArgument4, TArgument5>(IFormatProvider formatProvider, [Localizable(false)] string message, TArgument1 arg1, TArgument2 arg2, TArgument3 arg3, TArgument4 arg4, TArgument5 arg5
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsTraceEnabled)
            {
                var logEvent = new LogEventInfo(LogLevel.Trace, Name, formatProvider, message, new object[] { arg1, arg2, arg3, arg4, arg5 }, null);
                if (_isTraceCallSiteEnabled)
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }


        /// <summary>
        /// Writes log-event at specified level using provided generic value
        /// </summary>
        /// <typeparam name="T">The type of the argument.</typeparam>
        /// <param name="value">The value to be written.</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        public void Debug<T>(T value
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsDebugEnabled)
            {
                var logEvent = LogEventInfo.Create(LogLevel.Debug, Name, null, value);
                if (_isDebugCallSiteEnabled)
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at specified level using provided generic value
        /// </summary>
        /// <typeparam name="T">The type of the argument.</typeparam>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="value">The value to be written.</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        public void Debug<T>(IFormatProvider formatProvider, T value
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsDebugEnabled)
            {
                var logEvent = LogEventInfo.Create(LogLevel.Debug, Name, formatProvider, value);
                if (_isDebugCallSiteEnabled)
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }


        /// <summary>
        /// Writes log-event at <c>Debug</c> level using provided message-delegate
        /// </summary>
        /// <param name="messageFunc">A function returning message to be written. Function is not evaluated if logging is not enabled.</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        public void Debug(LogMessageGenerator messageFunc
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsDebugEnabled)
            {
                if (messageFunc == null)
                {
                    throw new ArgumentNullException("messageFunc");
                }

                var logEvent = new LogEventInfo(LogLevel.Debug, Name, null, messageFunc(), null, null);
                if (_isDebugCallSiteEnabled)
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Debug</c> level using provided exception and message-delegate
        /// </summary>
        /// <param name="exception">Exception to be logged.</param>
        /// <param name="messageFunc">A function returning message to be written. Function is not evaluated if logging is not enabled.</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        public void Debug(Exception exception, LogMessageGenerator messageFunc
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsDebugEnabled)
            {
                if (messageFunc == null)
                {
                    throw new ArgumentNullException("messageFunc");
                }

                var logEvent = new LogEventInfo(LogLevel.Debug, Name, null, messageFunc(), null, exception);
                if (_isDebugCallSiteEnabled)
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Debug</c> level using provided message
        /// </summary>
        /// <param name="message">Log event mesage</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        [MessageTemplateFormatMethod("message")]
        public void Debug([Localizable(false)] string message
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsDebugEnabled)
            {
                var logEvent = new LogEventInfo(LogLevel.Debug, Name, message);
                if (_isDebugCallSiteEnabled)
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Debug</c> level using provided exception and message
        /// </summary>
        /// <param name="exception">Exception to be logged.</param>
        /// <param name="message">Log event mesage</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        [MessageTemplateFormatMethod("message")]
        public void Debug(Exception exception, [Localizable(false)] string message
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsDebugEnabled)
            {
                var logEvent = new LogEventInfo(LogLevel.Debug, Name, null, message, null, exception);
                if (_isDebugCallSiteEnabled)
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Debug</c> level using provided message (without CallSite details)
        /// </summary>
        /// <param name="message">Log event mesage</param>
        /// <param name="args">Arguments to format.</param>
        [MessageTemplateFormatMethod("message")]
        public void Debug([Localizable(false)] string message, params object[] args)
        { 
            if (IsDebugEnabled)
            {
                var logEvent = new LogEventInfo(LogLevel.Debug, Name, null, message, args, null);
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Debug</c> level using provided message (without CallSite details)
        /// </summary>
        /// <param name="exception">Exception to be logged.</param>
        /// <param name="message">Log event mesage</param>
        /// <param name="args">Arguments to format.</param>
        [MessageTemplateFormatMethod("message")]
        public void Debug(Exception exception, [Localizable(false)] string message, params object[] args)
        { 
            if (IsDebugEnabled)
            {
                var logEvent = new LogEventInfo(LogLevel.Debug, Name, null, message, args, exception);
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Debug</c> level using provided exeption and message (without CallSite details)
        /// </summary>
        /// <param name="exception">Exception to be logged.</param>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">Log event mesage</param>
        /// <param name="args">Arguments to format.</param>
        [MessageTemplateFormatMethod("message")]
        public void Debug(Exception exception, IFormatProvider formatProvider, [Localizable(false)] string message, params object[] args)
        { 
            if (IsDebugEnabled)
            {
                var logEvent = new LogEventInfo(LogLevel.Debug, Name, formatProvider, message, args, exception);
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Debug</c> level using provided message (without CallSite details)
        /// </summary>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">Log event mesage</param>
        /// <param name="args">Arguments to format.</param>
        [MessageTemplateFormatMethod("message")]
        public void Debug(IFormatProvider formatProvider, [Localizable(false)] string message, params object[] args)
        { 
            if (IsDebugEnabled)
            {
                var logEvent = new LogEventInfo(LogLevel.Debug, Name, formatProvider, message, args, null);
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Debug</c> level using provided message
        /// </summary>
        /// <typeparam name="TArgument1">The type of the argument.</typeparam>
        /// <param name="message">Log event mesage</param>
        /// <param name="arg1">Arguments to format</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        [MessageTemplateFormatMethod("message")]
        public void Debug<TArgument1>([Localizable(false)] string message, TArgument1 arg1
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsDebugEnabled)
            {
                var logEvent = new LogEventInfo(LogLevel.Debug, Name, null, message, new object[] { arg1 });
                if (_isDebugCallSiteEnabled)
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Debug</c> level using provided exception and message
        /// </summary>
        /// <typeparam name="TArgument1">The type of the argument.</typeparam>
        /// <param name="exception">Exception to be logged.</param>
        /// <param name="message">Log event mesage</param>
        /// <param name="arg1">Arguments to format</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        [MessageTemplateFormatMethod("message")]
        public void Debug<TArgument1>(Exception exception, [Localizable(false)] string message, TArgument1 arg1
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsDebugEnabled)
            {
                var logEvent = new LogEventInfo(LogLevel.Debug, Name, null, message, new object[] { arg1 }, exception);
                if (_isDebugCallSiteEnabled)
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Debug</c> level using provided exception and message
        /// </summary>
        /// <typeparam name="TArgument1">The type of the argument.</typeparam>
        /// <param name="exception">Exception to be logged.</param>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">Log event mesage</param>
        /// <param name="arg1">Arguments to format</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        [MessageTemplateFormatMethod("message")]
        public void Debug<TArgument1>(Exception exception, IFormatProvider formatProvider, [Localizable(false)] string message, TArgument1 arg1
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsDebugEnabled)
            {
                var logEvent = new LogEventInfo(LogLevel.Debug, Name, formatProvider, message, new object[] { arg1 }, exception);
                if (_isDebugCallSiteEnabled)
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Debug</c> level using provided message
        /// </summary>
        /// <typeparam name="TArgument1">The type of the argument.</typeparam>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">Log event mesage</param>
        /// <param name="arg1">Arguments to format</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        [MessageTemplateFormatMethod("message")]
        public void Debug<TArgument1>(IFormatProvider formatProvider, [Localizable(false)] string message, TArgument1 arg1
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsDebugEnabled)
            {
                var logEvent = new LogEventInfo(LogLevel.Debug, Name, formatProvider, message, new object[] { arg1 }, null);
                if (_isDebugCallSiteEnabled)
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Debug</c> level using provided message
        /// </summary>
        /// <typeparam name="TArgument1">The type of the argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the argument.</typeparam>
        /// <param name="message">Log event mesage</param>
        /// <param name="arg1">Arguments to format</param>
        /// <param name="arg2">Arguments to format</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        [MessageTemplateFormatMethod("message")]
        public void Debug<TArgument1, TArgument2>([Localizable(false)] string message, TArgument1 arg1, TArgument2 arg2
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsDebugEnabled)
            {
                var logEvent = new LogEventInfo(LogLevel.Debug, Name, null, message, new object[] { arg1, arg2 });
                if (_isDebugCallSiteEnabled)
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Debug</c> level using provided exception and message
        /// </summary>
        /// <typeparam name="TArgument1">The type of the argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the argument.</typeparam>
        /// <param name="exception">Exception to be logged.</param>
        /// <param name="message">Log event mesage</param>
        /// <param name="arg1">Arguments to format</param>
        /// <param name="arg2">Arguments to format</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        [MessageTemplateFormatMethod("message")]
        public void Debug<TArgument1, TArgument2>(Exception exception, [Localizable(false)] string message, TArgument1 arg1, TArgument2 arg2
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsDebugEnabled)
            {
                var logEvent = new LogEventInfo(LogLevel.Debug, Name, null, message, new object[] { arg1, arg2 }, exception);
                if (_isDebugCallSiteEnabled)
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Debug</c> level using provided exception and message
        /// </summary>
        /// <typeparam name="TArgument1">The type of the argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the argument.</typeparam>
        /// <param name="exception">Exception to be logged.</param>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">Log event mesage</param>
        /// <param name="arg1">Arguments to format</param>
        /// <param name="arg2">Arguments to format</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        [MessageTemplateFormatMethod("message")]
        public void Debug<TArgument1, TArgument2>(Exception exception, IFormatProvider formatProvider, [Localizable(false)] string message, TArgument1 arg1, TArgument2 arg2
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsDebugEnabled)
            {
                var logEvent = new LogEventInfo(LogLevel.Debug, Name, formatProvider, message, new object[] { arg1, arg2 }, exception);
                if (_isDebugCallSiteEnabled)
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Debug</c> level using provided exception and message
        /// </summary>
        /// <typeparam name="TArgument1">The type of the argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the argument.</typeparam>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">Log event mesage</param>
        /// <param name="arg1">Arguments to format</param>
        /// <param name="arg2">Arguments to format</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        [MessageTemplateFormatMethod("message")]
        public void Debug<TArgument1, TArgument2>(IFormatProvider formatProvider, [Localizable(false)] string message, TArgument1 arg1, TArgument2 arg2
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsDebugEnabled)
            {
                var logEvent = new LogEventInfo(LogLevel.Debug, Name, formatProvider, message, new object[] { arg1, arg2 }, null);
                if (_isDebugCallSiteEnabled)
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Debug</c> level using provided message
        /// </summary>
        /// <typeparam name="TArgument1">The type of the argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the argument.</typeparam>
        /// <typeparam name="TArgument3">The type of the argument.</typeparam>
        /// <param name="message">Log event mesage</param>
        /// <param name="arg1">Arguments to format</param>
        /// <param name="arg2">Arguments to format</param>
        /// <param name="arg3">Arguments to format</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        [MessageTemplateFormatMethod("message")]
        public void Debug<TArgument1, TArgument2, TArgument3>([Localizable(false)] string message, TArgument1 arg1, TArgument2 arg2, TArgument3 arg3
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsDebugEnabled)
            {
                var logEvent = new LogEventInfo(LogLevel.Debug, Name, null, message, new object[] { arg1, arg2, arg3 });
                if (_isDebugCallSiteEnabled)
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Debug</c> level using provided exception and message
        /// </summary>
        /// <typeparam name="TArgument1">The type of the argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the argument.</typeparam>
        /// <typeparam name="TArgument3">The type of the argument.</typeparam>
        /// <param name="exception">Exception to be logged.</param>
        /// <param name="message">Log event mesage</param>
        /// <param name="arg1">Arguments to format</param>
        /// <param name="arg2">Arguments to format</param>
        /// <param name="arg3">Arguments to format</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        [MessageTemplateFormatMethod("message")]
        public void Debug<TArgument1, TArgument2, TArgument3>(Exception exception, [Localizable(false)] string message, TArgument1 arg1, TArgument2 arg2, TArgument3 arg3
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsDebugEnabled)
            {
                var logEvent = new LogEventInfo(LogLevel.Debug, Name, null, message, new object[] { arg1, arg2, arg3 }, exception);
                if (_isDebugCallSiteEnabled)
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Debug</c> level using provided exception and message
        /// </summary>
        /// <typeparam name="TArgument1">The type of the argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the argument.</typeparam>
        /// <typeparam name="TArgument3">The type of the argument.</typeparam>
        /// <param name="exception">Exception to be logged.</param>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">Log event mesage</param>
        /// <param name="arg1">Arguments to format</param>
        /// <param name="arg2">Arguments to format</param>
        /// <param name="arg3">Arguments to format</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        [MessageTemplateFormatMethod("message")]
        public void Debug<TArgument1, TArgument2, TArgument3>(Exception exception, IFormatProvider formatProvider, [Localizable(false)] string message, TArgument1 arg1, TArgument2 arg2, TArgument3 arg3
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsDebugEnabled)
            {
                var logEvent = new LogEventInfo(LogLevel.Debug, Name, formatProvider, message, new object[] { arg1, arg2, arg3 }, exception);
                if (_isDebugCallSiteEnabled)
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Debug</c> level using provided message
        /// </summary>
        /// <typeparam name="TArgument1">The type of the argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the argument.</typeparam>
        /// <typeparam name="TArgument3">The type of the argument.</typeparam>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">Log event mesage</param>
        /// <param name="arg1">Arguments to format</param>
        /// <param name="arg2">Arguments to format</param>
        /// <param name="arg3">Arguments to format</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        [MessageTemplateFormatMethod("message")]
        public void Debug<TArgument1, TArgument2, TArgument3>(IFormatProvider formatProvider, [Localizable(false)] string message, TArgument1 arg1, TArgument2 arg2, TArgument3 arg3
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsDebugEnabled)
            {
                var logEvent = new LogEventInfo(LogLevel.Debug, Name, formatProvider, message, new object[] { arg1, arg2, arg3 }, null);
                if (_isDebugCallSiteEnabled)
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Debug</c> level using provided message
        /// </summary>
        /// <typeparam name="TArgument1">The type of the argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the argument.</typeparam>
        /// <typeparam name="TArgument3">The type of the argument.</typeparam>
        /// <typeparam name="TArgument4">The type of the argument.</typeparam>
        /// <param name="message">Log event mesage</param>
        /// <param name="arg1">Arguments to format</param>
        /// <param name="arg2">Arguments to format</param>
        /// <param name="arg3">Arguments to format</param>
        /// <param name="arg4">Arguments to format</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        [MessageTemplateFormatMethod("message")]
        public void Debug<TArgument1, TArgument2, TArgument3, TArgument4>([Localizable(false)] string message, TArgument1 arg1, TArgument2 arg2, TArgument3 arg3, TArgument4 arg4
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsDebugEnabled)
            {
                var logEvent = new LogEventInfo(LogLevel.Debug, Name, null, message, new object[] { arg1, arg2, arg3, arg4 });
                if (_isDebugCallSiteEnabled)
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Debug</c> level using provided exception and message
        /// </summary>
        /// <typeparam name="TArgument1">The type of the argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the argument.</typeparam>
        /// <typeparam name="TArgument3">The type of the argument.</typeparam>
        /// <typeparam name="TArgument4">The type of the argument.</typeparam>
        /// <param name="exception">Exception to be logged.</param>
        /// <param name="message">Log event mesage</param>
        /// <param name="arg1">Arguments to format</param>
        /// <param name="arg2">Arguments to format</param>
        /// <param name="arg3">Arguments to format</param>
        /// <param name="arg4">Arguments to format</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        [MessageTemplateFormatMethod("message")]
        public void Debug<TArgument1, TArgument2, TArgument3, TArgument4>(Exception exception, [Localizable(false)] string message, TArgument1 arg1, TArgument2 arg2, TArgument3 arg3, TArgument4 arg4
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsDebugEnabled)
            {
                var logEvent = new LogEventInfo(LogLevel.Debug, Name, null, message, new object[] { arg1, arg2, arg3, arg4 }, exception);
                if (_isDebugCallSiteEnabled)
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Debug</c> level using provided exception and message
        /// </summary>
        /// <typeparam name="TArgument1">The type of the argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the argument.</typeparam>
        /// <typeparam name="TArgument3">The type of the argument.</typeparam>
        /// <typeparam name="TArgument4">The type of the argument.</typeparam>
        /// <param name="exception">Exception to be logged.</param>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">Log event mesage</param>
        /// <param name="arg1">Arguments to format</param>
        /// <param name="arg2">Arguments to format</param>
        /// <param name="arg3">Arguments to format</param>
        /// <param name="arg4">Arguments to format</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        [MessageTemplateFormatMethod("message")]
        public void Debug<TArgument1, TArgument2, TArgument3, TArgument4>(Exception exception, IFormatProvider formatProvider, [Localizable(false)] string message, TArgument1 arg1, TArgument2 arg2, TArgument3 arg3, TArgument4 arg4
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsDebugEnabled)
            {
                var logEvent = new LogEventInfo(LogLevel.Debug, Name, formatProvider, message, new object[] { arg1, arg2, arg3, arg4 }, exception);
                if (_isDebugCallSiteEnabled)
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Debug</c> level using provided message
        /// </summary>
        /// <typeparam name="TArgument1">The type of the argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the argument.</typeparam>
        /// <typeparam name="TArgument3">The type of the argument.</typeparam>
        /// <typeparam name="TArgument4">The type of the argument.</typeparam>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">Log event mesage</param>
        /// <param name="arg1">Arguments to format</param>
        /// <param name="arg2">Arguments to format</param>
        /// <param name="arg3">Arguments to format</param>
        /// <param name="arg4">Arguments to format</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        [MessageTemplateFormatMethod("message")]
        public void Debug<TArgument1, TArgument2, TArgument3, TArgument4>(IFormatProvider formatProvider, [Localizable(false)] string message, TArgument1 arg1, TArgument2 arg2, TArgument3 arg3, TArgument4 arg4
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsDebugEnabled)
            {
                var logEvent = new LogEventInfo(LogLevel.Debug, Name, formatProvider, message, new object[] { arg1, arg2, arg3, arg4 }, null);
                if (_isDebugCallSiteEnabled)
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Debug</c> level using provided message
        /// </summary>
        /// <typeparam name="TArgument1">The type of the argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the argument.</typeparam>
        /// <typeparam name="TArgument3">The type of the argument.</typeparam>
        /// <typeparam name="TArgument4">The type of the argument.</typeparam>
        /// <typeparam name="TArgument5">The type of the argument.</typeparam>
        /// <param name="message">Log event mesage</param>
        /// <param name="arg1">Arguments to format</param>
        /// <param name="arg2">Arguments to format</param>
        /// <param name="arg3">Arguments to format</param>
        /// <param name="arg4">Arguments to format</param>
        /// <param name="arg5">Arguments to format</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        [MessageTemplateFormatMethod("message")]
        public void Debug<TArgument1, TArgument2, TArgument3, TArgument4, TArgument5>([Localizable(false)] string message, TArgument1 arg1, TArgument2 arg2, TArgument3 arg3, TArgument4 arg4, TArgument5 arg5
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsDebugEnabled)
            {
                var logEvent = new LogEventInfo(LogLevel.Debug, Name, null, message, new object[] { arg1, arg2, arg3, arg4, arg5 });
                if (_isDebugCallSiteEnabled)
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Debug</c> level using provided exception and message
        /// </summary>
        /// <typeparam name="TArgument1">The type of the argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the argument.</typeparam>
        /// <typeparam name="TArgument3">The type of the argument.</typeparam>
        /// <typeparam name="TArgument4">The type of the argument.</typeparam>
        /// <typeparam name="TArgument5">The type of the argument.</typeparam>
        /// <param name="exception">Exception to be logged.</param>
        /// <param name="message">Log event mesage</param>
        /// <param name="arg1">Arguments to format</param>
        /// <param name="arg2">Arguments to format</param>
        /// <param name="arg3">Arguments to format</param>
        /// <param name="arg4">Arguments to format</param>
        /// <param name="arg5">Arguments to format</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        [MessageTemplateFormatMethod("message")]
        public void Debug<TArgument1, TArgument2, TArgument3, TArgument4, TArgument5>(Exception exception, [Localizable(false)] string message, TArgument1 arg1, TArgument2 arg2, TArgument3 arg3, TArgument4 arg4, TArgument5 arg5
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsDebugEnabled)
            {
                var logEvent = new LogEventInfo(LogLevel.Debug, Name, null, message, new object[] { arg1, arg2, arg3, arg4, arg5 }, exception);
                if (_isDebugCallSiteEnabled)
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Debug</c> level using provided exception and message
        /// </summary>
        /// <typeparam name="TArgument1">The type of the argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the argument.</typeparam>
        /// <typeparam name="TArgument3">The type of the argument.</typeparam>
        /// <typeparam name="TArgument4">The type of the argument.</typeparam>
        /// <typeparam name="TArgument5">The type of the argument.</typeparam>
        /// <param name="exception">Exception to be logged.</param>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">Log event mesage</param>
        /// <param name="arg1">Arguments to format</param>
        /// <param name="arg2">Arguments to format</param>
        /// <param name="arg3">Arguments to format</param>
        /// <param name="arg4">Arguments to format</param>
        /// <param name="arg5">Arguments to format</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        [MessageTemplateFormatMethod("message")]
        public void Debug<TArgument1, TArgument2, TArgument3, TArgument4, TArgument5>(Exception exception, IFormatProvider formatProvider, [Localizable(false)] string message, TArgument1 arg1, TArgument2 arg2, TArgument3 arg3, TArgument4 arg4, TArgument5 arg5
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsDebugEnabled)
            {
                var logEvent = new LogEventInfo(LogLevel.Debug, Name, formatProvider, message, new object[] { arg1, arg2, arg3, arg4, arg5 }, exception);
                if (_isDebugCallSiteEnabled)
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Debug</c> level using provided message
        /// </summary>
        /// <typeparam name="TArgument1">The type of the argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the argument.</typeparam>
        /// <typeparam name="TArgument3">The type of the argument.</typeparam>
        /// <typeparam name="TArgument4">The type of the argument.</typeparam>
        /// <typeparam name="TArgument5">The type of the argument.</typeparam>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">Log event mesage</param>
        /// <param name="arg1">Arguments to format</param>
        /// <param name="arg2">Arguments to format</param>
        /// <param name="arg3">Arguments to format</param>
        /// <param name="arg4">Arguments to format</param>
        /// <param name="arg5">Arguments to format</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        [MessageTemplateFormatMethod("message")]
        public void Debug<TArgument1, TArgument2, TArgument3, TArgument4, TArgument5>(IFormatProvider formatProvider, [Localizable(false)] string message, TArgument1 arg1, TArgument2 arg2, TArgument3 arg3, TArgument4 arg4, TArgument5 arg5
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsDebugEnabled)
            {
                var logEvent = new LogEventInfo(LogLevel.Debug, Name, formatProvider, message, new object[] { arg1, arg2, arg3, arg4, arg5 }, null);
                if (_isDebugCallSiteEnabled)
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }


        /// <summary>
        /// Writes log-event at specified level using provided generic value
        /// </summary>
        /// <typeparam name="T">The type of the argument.</typeparam>
        /// <param name="value">The value to be written.</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        public void Info<T>(T value
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsInfoEnabled)
            {
                var logEvent = LogEventInfo.Create(LogLevel.Info, Name, null, value);
                if (_isInfoCallSiteEnabled)
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at specified level using provided generic value
        /// </summary>
        /// <typeparam name="T">The type of the argument.</typeparam>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="value">The value to be written.</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        public void Info<T>(IFormatProvider formatProvider, T value
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsInfoEnabled)
            {
                var logEvent = LogEventInfo.Create(LogLevel.Info, Name, formatProvider, value);
                if (_isInfoCallSiteEnabled)
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }


        /// <summary>
        /// Writes log-event at <c>Info</c> level using provided message-delegate
        /// </summary>
        /// <param name="messageFunc">A function returning message to be written. Function is not evaluated if logging is not enabled.</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        public void Info(LogMessageGenerator messageFunc
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsInfoEnabled)
            {
                if (messageFunc == null)
                {
                    throw new ArgumentNullException("messageFunc");
                }

                var logEvent = new LogEventInfo(LogLevel.Info, Name, null, messageFunc(), null, null);
                if (_isInfoCallSiteEnabled)
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Info</c> level using provided exception and message-delegate
        /// </summary>
        /// <param name="exception">Exception to be logged.</param>
        /// <param name="messageFunc">A function returning message to be written. Function is not evaluated if logging is not enabled.</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        public void Info(Exception exception, LogMessageGenerator messageFunc
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsInfoEnabled)
            {
                if (messageFunc == null)
                {
                    throw new ArgumentNullException("messageFunc");
                }

                var logEvent = new LogEventInfo(LogLevel.Info, Name, null, messageFunc(), null, exception);
                if (_isInfoCallSiteEnabled)
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Info</c> level using provided message
        /// </summary>
        /// <param name="message">Log event mesage</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        [MessageTemplateFormatMethod("message")]
        public void Info([Localizable(false)] string message
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsInfoEnabled)
            {
                var logEvent = new LogEventInfo(LogLevel.Info, Name, message);
                if (_isInfoCallSiteEnabled)
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Info</c> level using provided exception and message
        /// </summary>
        /// <param name="exception">Exception to be logged.</param>
        /// <param name="message">Log event mesage</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        [MessageTemplateFormatMethod("message")]
        public void Info(Exception exception, [Localizable(false)] string message
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsInfoEnabled)
            {
                var logEvent = new LogEventInfo(LogLevel.Info, Name, null, message, null, exception);
                if (_isInfoCallSiteEnabled)
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Info</c> level using provided message (without CallSite details)
        /// </summary>
        /// <param name="message">Log event mesage</param>
        /// <param name="args">Arguments to format.</param>
        [MessageTemplateFormatMethod("message")]
        public void Info([Localizable(false)] string message, params object[] args)
        { 
            if (IsInfoEnabled)
            {
                var logEvent = new LogEventInfo(LogLevel.Info, Name, null, message, args, null);
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Info</c> level using provided message (without CallSite details)
        /// </summary>
        /// <param name="exception">Exception to be logged.</param>
        /// <param name="message">Log event mesage</param>
        /// <param name="args">Arguments to format.</param>
        [MessageTemplateFormatMethod("message")]
        public void Info(Exception exception, [Localizable(false)] string message, params object[] args)
        { 
            if (IsInfoEnabled)
            {
                var logEvent = new LogEventInfo(LogLevel.Info, Name, null, message, args, exception);
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Info</c> level using provided exeption and message (without CallSite details)
        /// </summary>
        /// <param name="exception">Exception to be logged.</param>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">Log event mesage</param>
        /// <param name="args">Arguments to format.</param>
        [MessageTemplateFormatMethod("message")]
        public void Info(Exception exception, IFormatProvider formatProvider, [Localizable(false)] string message, params object[] args)
        { 
            if (IsInfoEnabled)
            {
                var logEvent = new LogEventInfo(LogLevel.Info, Name, formatProvider, message, args, exception);
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Info</c> level using provided message (without CallSite details)
        /// </summary>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">Log event mesage</param>
        /// <param name="args">Arguments to format.</param>
        [MessageTemplateFormatMethod("message")]
        public void Info(IFormatProvider formatProvider, [Localizable(false)] string message, params object[] args)
        { 
            if (IsInfoEnabled)
            {
                var logEvent = new LogEventInfo(LogLevel.Info, Name, formatProvider, message, args, null);
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Info</c> level using provided message
        /// </summary>
        /// <typeparam name="TArgument1">The type of the argument.</typeparam>
        /// <param name="message">Log event mesage</param>
        /// <param name="arg1">Arguments to format</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        [MessageTemplateFormatMethod("message")]
        public void Info<TArgument1>([Localizable(false)] string message, TArgument1 arg1
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsInfoEnabled)
            {
                var logEvent = new LogEventInfo(LogLevel.Info, Name, null, message, new object[] { arg1 });
                if (_isInfoCallSiteEnabled)
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Info</c> level using provided exception and message
        /// </summary>
        /// <typeparam name="TArgument1">The type of the argument.</typeparam>
        /// <param name="exception">Exception to be logged.</param>
        /// <param name="message">Log event mesage</param>
        /// <param name="arg1">Arguments to format</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        [MessageTemplateFormatMethod("message")]
        public void Info<TArgument1>(Exception exception, [Localizable(false)] string message, TArgument1 arg1
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsInfoEnabled)
            {
                var logEvent = new LogEventInfo(LogLevel.Info, Name, null, message, new object[] { arg1 }, exception);
                if (_isInfoCallSiteEnabled)
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Info</c> level using provided exception and message
        /// </summary>
        /// <typeparam name="TArgument1">The type of the argument.</typeparam>
        /// <param name="exception">Exception to be logged.</param>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">Log event mesage</param>
        /// <param name="arg1">Arguments to format</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        [MessageTemplateFormatMethod("message")]
        public void Info<TArgument1>(Exception exception, IFormatProvider formatProvider, [Localizable(false)] string message, TArgument1 arg1
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsInfoEnabled)
            {
                var logEvent = new LogEventInfo(LogLevel.Info, Name, formatProvider, message, new object[] { arg1 }, exception);
                if (_isInfoCallSiteEnabled)
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Info</c> level using provided message
        /// </summary>
        /// <typeparam name="TArgument1">The type of the argument.</typeparam>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">Log event mesage</param>
        /// <param name="arg1">Arguments to format</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        [MessageTemplateFormatMethod("message")]
        public void Info<TArgument1>(IFormatProvider formatProvider, [Localizable(false)] string message, TArgument1 arg1
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsInfoEnabled)
            {
                var logEvent = new LogEventInfo(LogLevel.Info, Name, formatProvider, message, new object[] { arg1 }, null);
                if (_isInfoCallSiteEnabled)
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Info</c> level using provided message
        /// </summary>
        /// <typeparam name="TArgument1">The type of the argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the argument.</typeparam>
        /// <param name="message">Log event mesage</param>
        /// <param name="arg1">Arguments to format</param>
        /// <param name="arg2">Arguments to format</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        [MessageTemplateFormatMethod("message")]
        public void Info<TArgument1, TArgument2>([Localizable(false)] string message, TArgument1 arg1, TArgument2 arg2
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsInfoEnabled)
            {
                var logEvent = new LogEventInfo(LogLevel.Info, Name, null, message, new object[] { arg1, arg2 });
                if (_isInfoCallSiteEnabled)
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Info</c> level using provided exception and message
        /// </summary>
        /// <typeparam name="TArgument1">The type of the argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the argument.</typeparam>
        /// <param name="exception">Exception to be logged.</param>
        /// <param name="message">Log event mesage</param>
        /// <param name="arg1">Arguments to format</param>
        /// <param name="arg2">Arguments to format</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        [MessageTemplateFormatMethod("message")]
        public void Info<TArgument1, TArgument2>(Exception exception, [Localizable(false)] string message, TArgument1 arg1, TArgument2 arg2
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsInfoEnabled)
            {
                var logEvent = new LogEventInfo(LogLevel.Info, Name, null, message, new object[] { arg1, arg2 }, exception);
                if (_isInfoCallSiteEnabled)
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Info</c> level using provided exception and message
        /// </summary>
        /// <typeparam name="TArgument1">The type of the argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the argument.</typeparam>
        /// <param name="exception">Exception to be logged.</param>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">Log event mesage</param>
        /// <param name="arg1">Arguments to format</param>
        /// <param name="arg2">Arguments to format</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        [MessageTemplateFormatMethod("message")]
        public void Info<TArgument1, TArgument2>(Exception exception, IFormatProvider formatProvider, [Localizable(false)] string message, TArgument1 arg1, TArgument2 arg2
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsInfoEnabled)
            {
                var logEvent = new LogEventInfo(LogLevel.Info, Name, formatProvider, message, new object[] { arg1, arg2 }, exception);
                if (_isInfoCallSiteEnabled)
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Info</c> level using provided exception and message
        /// </summary>
        /// <typeparam name="TArgument1">The type of the argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the argument.</typeparam>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">Log event mesage</param>
        /// <param name="arg1">Arguments to format</param>
        /// <param name="arg2">Arguments to format</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        [MessageTemplateFormatMethod("message")]
        public void Info<TArgument1, TArgument2>(IFormatProvider formatProvider, [Localizable(false)] string message, TArgument1 arg1, TArgument2 arg2
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsInfoEnabled)
            {
                var logEvent = new LogEventInfo(LogLevel.Info, Name, formatProvider, message, new object[] { arg1, arg2 }, null);
                if (_isInfoCallSiteEnabled)
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Info</c> level using provided message
        /// </summary>
        /// <typeparam name="TArgument1">The type of the argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the argument.</typeparam>
        /// <typeparam name="TArgument3">The type of the argument.</typeparam>
        /// <param name="message">Log event mesage</param>
        /// <param name="arg1">Arguments to format</param>
        /// <param name="arg2">Arguments to format</param>
        /// <param name="arg3">Arguments to format</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        [MessageTemplateFormatMethod("message")]
        public void Info<TArgument1, TArgument2, TArgument3>([Localizable(false)] string message, TArgument1 arg1, TArgument2 arg2, TArgument3 arg3
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsInfoEnabled)
            {
                var logEvent = new LogEventInfo(LogLevel.Info, Name, null, message, new object[] { arg1, arg2, arg3 });
                if (_isInfoCallSiteEnabled)
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Info</c> level using provided exception and message
        /// </summary>
        /// <typeparam name="TArgument1">The type of the argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the argument.</typeparam>
        /// <typeparam name="TArgument3">The type of the argument.</typeparam>
        /// <param name="exception">Exception to be logged.</param>
        /// <param name="message">Log event mesage</param>
        /// <param name="arg1">Arguments to format</param>
        /// <param name="arg2">Arguments to format</param>
        /// <param name="arg3">Arguments to format</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        [MessageTemplateFormatMethod("message")]
        public void Info<TArgument1, TArgument2, TArgument3>(Exception exception, [Localizable(false)] string message, TArgument1 arg1, TArgument2 arg2, TArgument3 arg3
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsInfoEnabled)
            {
                var logEvent = new LogEventInfo(LogLevel.Info, Name, null, message, new object[] { arg1, arg2, arg3 }, exception);
                if (_isInfoCallSiteEnabled)
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Info</c> level using provided exception and message
        /// </summary>
        /// <typeparam name="TArgument1">The type of the argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the argument.</typeparam>
        /// <typeparam name="TArgument3">The type of the argument.</typeparam>
        /// <param name="exception">Exception to be logged.</param>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">Log event mesage</param>
        /// <param name="arg1">Arguments to format</param>
        /// <param name="arg2">Arguments to format</param>
        /// <param name="arg3">Arguments to format</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        [MessageTemplateFormatMethod("message")]
        public void Info<TArgument1, TArgument2, TArgument3>(Exception exception, IFormatProvider formatProvider, [Localizable(false)] string message, TArgument1 arg1, TArgument2 arg2, TArgument3 arg3
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsInfoEnabled)
            {
                var logEvent = new LogEventInfo(LogLevel.Info, Name, formatProvider, message, new object[] { arg1, arg2, arg3 }, exception);
                if (_isInfoCallSiteEnabled)
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Info</c> level using provided message
        /// </summary>
        /// <typeparam name="TArgument1">The type of the argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the argument.</typeparam>
        /// <typeparam name="TArgument3">The type of the argument.</typeparam>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">Log event mesage</param>
        /// <param name="arg1">Arguments to format</param>
        /// <param name="arg2">Arguments to format</param>
        /// <param name="arg3">Arguments to format</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        [MessageTemplateFormatMethod("message")]
        public void Info<TArgument1, TArgument2, TArgument3>(IFormatProvider formatProvider, [Localizable(false)] string message, TArgument1 arg1, TArgument2 arg2, TArgument3 arg3
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsInfoEnabled)
            {
                var logEvent = new LogEventInfo(LogLevel.Info, Name, formatProvider, message, new object[] { arg1, arg2, arg3 }, null);
                if (_isInfoCallSiteEnabled)
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Info</c> level using provided message
        /// </summary>
        /// <typeparam name="TArgument1">The type of the argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the argument.</typeparam>
        /// <typeparam name="TArgument3">The type of the argument.</typeparam>
        /// <typeparam name="TArgument4">The type of the argument.</typeparam>
        /// <param name="message">Log event mesage</param>
        /// <param name="arg1">Arguments to format</param>
        /// <param name="arg2">Arguments to format</param>
        /// <param name="arg3">Arguments to format</param>
        /// <param name="arg4">Arguments to format</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        [MessageTemplateFormatMethod("message")]
        public void Info<TArgument1, TArgument2, TArgument3, TArgument4>([Localizable(false)] string message, TArgument1 arg1, TArgument2 arg2, TArgument3 arg3, TArgument4 arg4
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsInfoEnabled)
            {
                var logEvent = new LogEventInfo(LogLevel.Info, Name, null, message, new object[] { arg1, arg2, arg3, arg4 });
                if (_isInfoCallSiteEnabled)
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Info</c> level using provided exception and message
        /// </summary>
        /// <typeparam name="TArgument1">The type of the argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the argument.</typeparam>
        /// <typeparam name="TArgument3">The type of the argument.</typeparam>
        /// <typeparam name="TArgument4">The type of the argument.</typeparam>
        /// <param name="exception">Exception to be logged.</param>
        /// <param name="message">Log event mesage</param>
        /// <param name="arg1">Arguments to format</param>
        /// <param name="arg2">Arguments to format</param>
        /// <param name="arg3">Arguments to format</param>
        /// <param name="arg4">Arguments to format</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        [MessageTemplateFormatMethod("message")]
        public void Info<TArgument1, TArgument2, TArgument3, TArgument4>(Exception exception, [Localizable(false)] string message, TArgument1 arg1, TArgument2 arg2, TArgument3 arg3, TArgument4 arg4
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsInfoEnabled)
            {
                var logEvent = new LogEventInfo(LogLevel.Info, Name, null, message, new object[] { arg1, arg2, arg3, arg4 }, exception);
                if (_isInfoCallSiteEnabled)
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Info</c> level using provided exception and message
        /// </summary>
        /// <typeparam name="TArgument1">The type of the argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the argument.</typeparam>
        /// <typeparam name="TArgument3">The type of the argument.</typeparam>
        /// <typeparam name="TArgument4">The type of the argument.</typeparam>
        /// <param name="exception">Exception to be logged.</param>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">Log event mesage</param>
        /// <param name="arg1">Arguments to format</param>
        /// <param name="arg2">Arguments to format</param>
        /// <param name="arg3">Arguments to format</param>
        /// <param name="arg4">Arguments to format</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        [MessageTemplateFormatMethod("message")]
        public void Info<TArgument1, TArgument2, TArgument3, TArgument4>(Exception exception, IFormatProvider formatProvider, [Localizable(false)] string message, TArgument1 arg1, TArgument2 arg2, TArgument3 arg3, TArgument4 arg4
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsInfoEnabled)
            {
                var logEvent = new LogEventInfo(LogLevel.Info, Name, formatProvider, message, new object[] { arg1, arg2, arg3, arg4 }, exception);
                if (_isInfoCallSiteEnabled)
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Info</c> level using provided message
        /// </summary>
        /// <typeparam name="TArgument1">The type of the argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the argument.</typeparam>
        /// <typeparam name="TArgument3">The type of the argument.</typeparam>
        /// <typeparam name="TArgument4">The type of the argument.</typeparam>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">Log event mesage</param>
        /// <param name="arg1">Arguments to format</param>
        /// <param name="arg2">Arguments to format</param>
        /// <param name="arg3">Arguments to format</param>
        /// <param name="arg4">Arguments to format</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        [MessageTemplateFormatMethod("message")]
        public void Info<TArgument1, TArgument2, TArgument3, TArgument4>(IFormatProvider formatProvider, [Localizable(false)] string message, TArgument1 arg1, TArgument2 arg2, TArgument3 arg3, TArgument4 arg4
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsInfoEnabled)
            {
                var logEvent = new LogEventInfo(LogLevel.Info, Name, formatProvider, message, new object[] { arg1, arg2, arg3, arg4 }, null);
                if (_isInfoCallSiteEnabled)
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Info</c> level using provided message
        /// </summary>
        /// <typeparam name="TArgument1">The type of the argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the argument.</typeparam>
        /// <typeparam name="TArgument3">The type of the argument.</typeparam>
        /// <typeparam name="TArgument4">The type of the argument.</typeparam>
        /// <typeparam name="TArgument5">The type of the argument.</typeparam>
        /// <param name="message">Log event mesage</param>
        /// <param name="arg1">Arguments to format</param>
        /// <param name="arg2">Arguments to format</param>
        /// <param name="arg3">Arguments to format</param>
        /// <param name="arg4">Arguments to format</param>
        /// <param name="arg5">Arguments to format</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        [MessageTemplateFormatMethod("message")]
        public void Info<TArgument1, TArgument2, TArgument3, TArgument4, TArgument5>([Localizable(false)] string message, TArgument1 arg1, TArgument2 arg2, TArgument3 arg3, TArgument4 arg4, TArgument5 arg5
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsInfoEnabled)
            {
                var logEvent = new LogEventInfo(LogLevel.Info, Name, null, message, new object[] { arg1, arg2, arg3, arg4, arg5 });
                if (_isInfoCallSiteEnabled)
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Info</c> level using provided exception and message
        /// </summary>
        /// <typeparam name="TArgument1">The type of the argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the argument.</typeparam>
        /// <typeparam name="TArgument3">The type of the argument.</typeparam>
        /// <typeparam name="TArgument4">The type of the argument.</typeparam>
        /// <typeparam name="TArgument5">The type of the argument.</typeparam>
        /// <param name="exception">Exception to be logged.</param>
        /// <param name="message">Log event mesage</param>
        /// <param name="arg1">Arguments to format</param>
        /// <param name="arg2">Arguments to format</param>
        /// <param name="arg3">Arguments to format</param>
        /// <param name="arg4">Arguments to format</param>
        /// <param name="arg5">Arguments to format</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        [MessageTemplateFormatMethod("message")]
        public void Info<TArgument1, TArgument2, TArgument3, TArgument4, TArgument5>(Exception exception, [Localizable(false)] string message, TArgument1 arg1, TArgument2 arg2, TArgument3 arg3, TArgument4 arg4, TArgument5 arg5
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsInfoEnabled)
            {
                var logEvent = new LogEventInfo(LogLevel.Info, Name, null, message, new object[] { arg1, arg2, arg3, arg4, arg5 }, exception);
                if (_isInfoCallSiteEnabled)
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Info</c> level using provided exception and message
        /// </summary>
        /// <typeparam name="TArgument1">The type of the argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the argument.</typeparam>
        /// <typeparam name="TArgument3">The type of the argument.</typeparam>
        /// <typeparam name="TArgument4">The type of the argument.</typeparam>
        /// <typeparam name="TArgument5">The type of the argument.</typeparam>
        /// <param name="exception">Exception to be logged.</param>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">Log event mesage</param>
        /// <param name="arg1">Arguments to format</param>
        /// <param name="arg2">Arguments to format</param>
        /// <param name="arg3">Arguments to format</param>
        /// <param name="arg4">Arguments to format</param>
        /// <param name="arg5">Arguments to format</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        [MessageTemplateFormatMethod("message")]
        public void Info<TArgument1, TArgument2, TArgument3, TArgument4, TArgument5>(Exception exception, IFormatProvider formatProvider, [Localizable(false)] string message, TArgument1 arg1, TArgument2 arg2, TArgument3 arg3, TArgument4 arg4, TArgument5 arg5
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsInfoEnabled)
            {
                var logEvent = new LogEventInfo(LogLevel.Info, Name, formatProvider, message, new object[] { arg1, arg2, arg3, arg4, arg5 }, exception);
                if (_isInfoCallSiteEnabled)
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Info</c> level using provided message
        /// </summary>
        /// <typeparam name="TArgument1">The type of the argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the argument.</typeparam>
        /// <typeparam name="TArgument3">The type of the argument.</typeparam>
        /// <typeparam name="TArgument4">The type of the argument.</typeparam>
        /// <typeparam name="TArgument5">The type of the argument.</typeparam>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">Log event mesage</param>
        /// <param name="arg1">Arguments to format</param>
        /// <param name="arg2">Arguments to format</param>
        /// <param name="arg3">Arguments to format</param>
        /// <param name="arg4">Arguments to format</param>
        /// <param name="arg5">Arguments to format</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        [MessageTemplateFormatMethod("message")]
        public void Info<TArgument1, TArgument2, TArgument3, TArgument4, TArgument5>(IFormatProvider formatProvider, [Localizable(false)] string message, TArgument1 arg1, TArgument2 arg2, TArgument3 arg3, TArgument4 arg4, TArgument5 arg5
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsInfoEnabled)
            {
                var logEvent = new LogEventInfo(LogLevel.Info, Name, formatProvider, message, new object[] { arg1, arg2, arg3, arg4, arg5 }, null);
                if (_isInfoCallSiteEnabled)
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }


        /// <summary>
        /// Writes log-event at specified level using provided generic value
        /// </summary>
        /// <typeparam name="T">The type of the argument.</typeparam>
        /// <param name="value">The value to be written.</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        public void Warn<T>(T value
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsWarnEnabled)
            {
                var logEvent = LogEventInfo.Create(LogLevel.Warn, Name, null, value);
                if (_isWarnCallSiteEnabled)
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at specified level using provided generic value
        /// </summary>
        /// <typeparam name="T">The type of the argument.</typeparam>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="value">The value to be written.</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        public void Warn<T>(IFormatProvider formatProvider, T value
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsWarnEnabled)
            {
                var logEvent = LogEventInfo.Create(LogLevel.Warn, Name, formatProvider, value);
                if (_isWarnCallSiteEnabled)
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }


        /// <summary>
        /// Writes log-event at <c>Warn</c> level using provided message-delegate
        /// </summary>
        /// <param name="messageFunc">A function returning message to be written. Function is not evaluated if logging is not enabled.</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        public void Warn(LogMessageGenerator messageFunc
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsWarnEnabled)
            {
                if (messageFunc == null)
                {
                    throw new ArgumentNullException("messageFunc");
                }

                var logEvent = new LogEventInfo(LogLevel.Warn, Name, null, messageFunc(), null, null);
                if (_isWarnCallSiteEnabled)
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Warn</c> level using provided exception and message-delegate
        /// </summary>
        /// <param name="exception">Exception to be logged.</param>
        /// <param name="messageFunc">A function returning message to be written. Function is not evaluated if logging is not enabled.</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        public void Warn(Exception exception, LogMessageGenerator messageFunc
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsWarnEnabled)
            {
                if (messageFunc == null)
                {
                    throw new ArgumentNullException("messageFunc");
                }

                var logEvent = new LogEventInfo(LogLevel.Warn, Name, null, messageFunc(), null, exception);
                if (_isWarnCallSiteEnabled)
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Warn</c> level using provided message
        /// </summary>
        /// <param name="message">Log event mesage</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        [MessageTemplateFormatMethod("message")]
        public void Warn([Localizable(false)] string message
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsWarnEnabled)
            {
                var logEvent = new LogEventInfo(LogLevel.Warn, Name, message);
                if (_isWarnCallSiteEnabled)
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Warn</c> level using provided exception and message
        /// </summary>
        /// <param name="exception">Exception to be logged.</param>
        /// <param name="message">Log event mesage</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        [MessageTemplateFormatMethod("message")]
        public void Warn(Exception exception, [Localizable(false)] string message
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsWarnEnabled)
            {
                var logEvent = new LogEventInfo(LogLevel.Warn, Name, null, message, null, exception);
                if (_isWarnCallSiteEnabled)
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Warn</c> level using provided message (without CallSite details)
        /// </summary>
        /// <param name="message">Log event mesage</param>
        /// <param name="args">Arguments to format.</param>
        [MessageTemplateFormatMethod("message")]
        public void Warn([Localizable(false)] string message, params object[] args)
        { 
            if (IsWarnEnabled)
            {
                var logEvent = new LogEventInfo(LogLevel.Warn, Name, null, message, args, null);
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Warn</c> level using provided message (without CallSite details)
        /// </summary>
        /// <param name="exception">Exception to be logged.</param>
        /// <param name="message">Log event mesage</param>
        /// <param name="args">Arguments to format.</param>
        [MessageTemplateFormatMethod("message")]
        public void Warn(Exception exception, [Localizable(false)] string message, params object[] args)
        { 
            if (IsWarnEnabled)
            {
                var logEvent = new LogEventInfo(LogLevel.Warn, Name, null, message, args, exception);
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Warn</c> level using provided exeption and message (without CallSite details)
        /// </summary>
        /// <param name="exception">Exception to be logged.</param>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">Log event mesage</param>
        /// <param name="args">Arguments to format.</param>
        [MessageTemplateFormatMethod("message")]
        public void Warn(Exception exception, IFormatProvider formatProvider, [Localizable(false)] string message, params object[] args)
        { 
            if (IsWarnEnabled)
            {
                var logEvent = new LogEventInfo(LogLevel.Warn, Name, formatProvider, message, args, exception);
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Warn</c> level using provided message (without CallSite details)
        /// </summary>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">Log event mesage</param>
        /// <param name="args">Arguments to format.</param>
        [MessageTemplateFormatMethod("message")]
        public void Warn(IFormatProvider formatProvider, [Localizable(false)] string message, params object[] args)
        { 
            if (IsWarnEnabled)
            {
                var logEvent = new LogEventInfo(LogLevel.Warn, Name, formatProvider, message, args, null);
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Warn</c> level using provided message
        /// </summary>
        /// <typeparam name="TArgument1">The type of the argument.</typeparam>
        /// <param name="message">Log event mesage</param>
        /// <param name="arg1">Arguments to format</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        [MessageTemplateFormatMethod("message")]
        public void Warn<TArgument1>([Localizable(false)] string message, TArgument1 arg1
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsWarnEnabled)
            {
                var logEvent = new LogEventInfo(LogLevel.Warn, Name, null, message, new object[] { arg1 });
                if (_isWarnCallSiteEnabled)
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Warn</c> level using provided exception and message
        /// </summary>
        /// <typeparam name="TArgument1">The type of the argument.</typeparam>
        /// <param name="exception">Exception to be logged.</param>
        /// <param name="message">Log event mesage</param>
        /// <param name="arg1">Arguments to format</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        [MessageTemplateFormatMethod("message")]
        public void Warn<TArgument1>(Exception exception, [Localizable(false)] string message, TArgument1 arg1
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsWarnEnabled)
            {
                var logEvent = new LogEventInfo(LogLevel.Warn, Name, null, message, new object[] { arg1 }, exception);
                if (_isWarnCallSiteEnabled)
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Warn</c> level using provided exception and message
        /// </summary>
        /// <typeparam name="TArgument1">The type of the argument.</typeparam>
        /// <param name="exception">Exception to be logged.</param>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">Log event mesage</param>
        /// <param name="arg1">Arguments to format</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        [MessageTemplateFormatMethod("message")]
        public void Warn<TArgument1>(Exception exception, IFormatProvider formatProvider, [Localizable(false)] string message, TArgument1 arg1
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsWarnEnabled)
            {
                var logEvent = new LogEventInfo(LogLevel.Warn, Name, formatProvider, message, new object[] { arg1 }, exception);
                if (_isWarnCallSiteEnabled)
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Warn</c> level using provided message
        /// </summary>
        /// <typeparam name="TArgument1">The type of the argument.</typeparam>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">Log event mesage</param>
        /// <param name="arg1">Arguments to format</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        [MessageTemplateFormatMethod("message")]
        public void Warn<TArgument1>(IFormatProvider formatProvider, [Localizable(false)] string message, TArgument1 arg1
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsWarnEnabled)
            {
                var logEvent = new LogEventInfo(LogLevel.Warn, Name, formatProvider, message, new object[] { arg1 }, null);
                if (_isWarnCallSiteEnabled)
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Warn</c> level using provided message
        /// </summary>
        /// <typeparam name="TArgument1">The type of the argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the argument.</typeparam>
        /// <param name="message">Log event mesage</param>
        /// <param name="arg1">Arguments to format</param>
        /// <param name="arg2">Arguments to format</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        [MessageTemplateFormatMethod("message")]
        public void Warn<TArgument1, TArgument2>([Localizable(false)] string message, TArgument1 arg1, TArgument2 arg2
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsWarnEnabled)
            {
                var logEvent = new LogEventInfo(LogLevel.Warn, Name, null, message, new object[] { arg1, arg2 });
                if (_isWarnCallSiteEnabled)
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Warn</c> level using provided exception and message
        /// </summary>
        /// <typeparam name="TArgument1">The type of the argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the argument.</typeparam>
        /// <param name="exception">Exception to be logged.</param>
        /// <param name="message">Log event mesage</param>
        /// <param name="arg1">Arguments to format</param>
        /// <param name="arg2">Arguments to format</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        [MessageTemplateFormatMethod("message")]
        public void Warn<TArgument1, TArgument2>(Exception exception, [Localizable(false)] string message, TArgument1 arg1, TArgument2 arg2
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsWarnEnabled)
            {
                var logEvent = new LogEventInfo(LogLevel.Warn, Name, null, message, new object[] { arg1, arg2 }, exception);
                if (_isWarnCallSiteEnabled)
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Warn</c> level using provided exception and message
        /// </summary>
        /// <typeparam name="TArgument1">The type of the argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the argument.</typeparam>
        /// <param name="exception">Exception to be logged.</param>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">Log event mesage</param>
        /// <param name="arg1">Arguments to format</param>
        /// <param name="arg2">Arguments to format</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        [MessageTemplateFormatMethod("message")]
        public void Warn<TArgument1, TArgument2>(Exception exception, IFormatProvider formatProvider, [Localizable(false)] string message, TArgument1 arg1, TArgument2 arg2
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsWarnEnabled)
            {
                var logEvent = new LogEventInfo(LogLevel.Warn, Name, formatProvider, message, new object[] { arg1, arg2 }, exception);
                if (_isWarnCallSiteEnabled)
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Warn</c> level using provided exception and message
        /// </summary>
        /// <typeparam name="TArgument1">The type of the argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the argument.</typeparam>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">Log event mesage</param>
        /// <param name="arg1">Arguments to format</param>
        /// <param name="arg2">Arguments to format</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        [MessageTemplateFormatMethod("message")]
        public void Warn<TArgument1, TArgument2>(IFormatProvider formatProvider, [Localizable(false)] string message, TArgument1 arg1, TArgument2 arg2
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsWarnEnabled)
            {
                var logEvent = new LogEventInfo(LogLevel.Warn, Name, formatProvider, message, new object[] { arg1, arg2 }, null);
                if (_isWarnCallSiteEnabled)
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Warn</c> level using provided message
        /// </summary>
        /// <typeparam name="TArgument1">The type of the argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the argument.</typeparam>
        /// <typeparam name="TArgument3">The type of the argument.</typeparam>
        /// <param name="message">Log event mesage</param>
        /// <param name="arg1">Arguments to format</param>
        /// <param name="arg2">Arguments to format</param>
        /// <param name="arg3">Arguments to format</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        [MessageTemplateFormatMethod("message")]
        public void Warn<TArgument1, TArgument2, TArgument3>([Localizable(false)] string message, TArgument1 arg1, TArgument2 arg2, TArgument3 arg3
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsWarnEnabled)
            {
                var logEvent = new LogEventInfo(LogLevel.Warn, Name, null, message, new object[] { arg1, arg2, arg3 });
                if (_isWarnCallSiteEnabled)
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Warn</c> level using provided exception and message
        /// </summary>
        /// <typeparam name="TArgument1">The type of the argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the argument.</typeparam>
        /// <typeparam name="TArgument3">The type of the argument.</typeparam>
        /// <param name="exception">Exception to be logged.</param>
        /// <param name="message">Log event mesage</param>
        /// <param name="arg1">Arguments to format</param>
        /// <param name="arg2">Arguments to format</param>
        /// <param name="arg3">Arguments to format</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        [MessageTemplateFormatMethod("message")]
        public void Warn<TArgument1, TArgument2, TArgument3>(Exception exception, [Localizable(false)] string message, TArgument1 arg1, TArgument2 arg2, TArgument3 arg3
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsWarnEnabled)
            {
                var logEvent = new LogEventInfo(LogLevel.Warn, Name, null, message, new object[] { arg1, arg2, arg3 }, exception);
                if (_isWarnCallSiteEnabled)
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Warn</c> level using provided exception and message
        /// </summary>
        /// <typeparam name="TArgument1">The type of the argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the argument.</typeparam>
        /// <typeparam name="TArgument3">The type of the argument.</typeparam>
        /// <param name="exception">Exception to be logged.</param>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">Log event mesage</param>
        /// <param name="arg1">Arguments to format</param>
        /// <param name="arg2">Arguments to format</param>
        /// <param name="arg3">Arguments to format</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        [MessageTemplateFormatMethod("message")]
        public void Warn<TArgument1, TArgument2, TArgument3>(Exception exception, IFormatProvider formatProvider, [Localizable(false)] string message, TArgument1 arg1, TArgument2 arg2, TArgument3 arg3
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsWarnEnabled)
            {
                var logEvent = new LogEventInfo(LogLevel.Warn, Name, formatProvider, message, new object[] { arg1, arg2, arg3 }, exception);
                if (_isWarnCallSiteEnabled)
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Warn</c> level using provided message
        /// </summary>
        /// <typeparam name="TArgument1">The type of the argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the argument.</typeparam>
        /// <typeparam name="TArgument3">The type of the argument.</typeparam>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">Log event mesage</param>
        /// <param name="arg1">Arguments to format</param>
        /// <param name="arg2">Arguments to format</param>
        /// <param name="arg3">Arguments to format</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        [MessageTemplateFormatMethod("message")]
        public void Warn<TArgument1, TArgument2, TArgument3>(IFormatProvider formatProvider, [Localizable(false)] string message, TArgument1 arg1, TArgument2 arg2, TArgument3 arg3
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsWarnEnabled)
            {
                var logEvent = new LogEventInfo(LogLevel.Warn, Name, formatProvider, message, new object[] { arg1, arg2, arg3 }, null);
                if (_isWarnCallSiteEnabled)
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Warn</c> level using provided message
        /// </summary>
        /// <typeparam name="TArgument1">The type of the argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the argument.</typeparam>
        /// <typeparam name="TArgument3">The type of the argument.</typeparam>
        /// <typeparam name="TArgument4">The type of the argument.</typeparam>
        /// <param name="message">Log event mesage</param>
        /// <param name="arg1">Arguments to format</param>
        /// <param name="arg2">Arguments to format</param>
        /// <param name="arg3">Arguments to format</param>
        /// <param name="arg4">Arguments to format</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        [MessageTemplateFormatMethod("message")]
        public void Warn<TArgument1, TArgument2, TArgument3, TArgument4>([Localizable(false)] string message, TArgument1 arg1, TArgument2 arg2, TArgument3 arg3, TArgument4 arg4
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsWarnEnabled)
            {
                var logEvent = new LogEventInfo(LogLevel.Warn, Name, null, message, new object[] { arg1, arg2, arg3, arg4 });
                if (_isWarnCallSiteEnabled)
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Warn</c> level using provided exception and message
        /// </summary>
        /// <typeparam name="TArgument1">The type of the argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the argument.</typeparam>
        /// <typeparam name="TArgument3">The type of the argument.</typeparam>
        /// <typeparam name="TArgument4">The type of the argument.</typeparam>
        /// <param name="exception">Exception to be logged.</param>
        /// <param name="message">Log event mesage</param>
        /// <param name="arg1">Arguments to format</param>
        /// <param name="arg2">Arguments to format</param>
        /// <param name="arg3">Arguments to format</param>
        /// <param name="arg4">Arguments to format</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        [MessageTemplateFormatMethod("message")]
        public void Warn<TArgument1, TArgument2, TArgument3, TArgument4>(Exception exception, [Localizable(false)] string message, TArgument1 arg1, TArgument2 arg2, TArgument3 arg3, TArgument4 arg4
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsWarnEnabled)
            {
                var logEvent = new LogEventInfo(LogLevel.Warn, Name, null, message, new object[] { arg1, arg2, arg3, arg4 }, exception);
                if (_isWarnCallSiteEnabled)
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Warn</c> level using provided exception and message
        /// </summary>
        /// <typeparam name="TArgument1">The type of the argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the argument.</typeparam>
        /// <typeparam name="TArgument3">The type of the argument.</typeparam>
        /// <typeparam name="TArgument4">The type of the argument.</typeparam>
        /// <param name="exception">Exception to be logged.</param>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">Log event mesage</param>
        /// <param name="arg1">Arguments to format</param>
        /// <param name="arg2">Arguments to format</param>
        /// <param name="arg3">Arguments to format</param>
        /// <param name="arg4">Arguments to format</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        [MessageTemplateFormatMethod("message")]
        public void Warn<TArgument1, TArgument2, TArgument3, TArgument4>(Exception exception, IFormatProvider formatProvider, [Localizable(false)] string message, TArgument1 arg1, TArgument2 arg2, TArgument3 arg3, TArgument4 arg4
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsWarnEnabled)
            {
                var logEvent = new LogEventInfo(LogLevel.Warn, Name, formatProvider, message, new object[] { arg1, arg2, arg3, arg4 }, exception);
                if (_isWarnCallSiteEnabled)
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Warn</c> level using provided message
        /// </summary>
        /// <typeparam name="TArgument1">The type of the argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the argument.</typeparam>
        /// <typeparam name="TArgument3">The type of the argument.</typeparam>
        /// <typeparam name="TArgument4">The type of the argument.</typeparam>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">Log event mesage</param>
        /// <param name="arg1">Arguments to format</param>
        /// <param name="arg2">Arguments to format</param>
        /// <param name="arg3">Arguments to format</param>
        /// <param name="arg4">Arguments to format</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        [MessageTemplateFormatMethod("message")]
        public void Warn<TArgument1, TArgument2, TArgument3, TArgument4>(IFormatProvider formatProvider, [Localizable(false)] string message, TArgument1 arg1, TArgument2 arg2, TArgument3 arg3, TArgument4 arg4
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsWarnEnabled)
            {
                var logEvent = new LogEventInfo(LogLevel.Warn, Name, formatProvider, message, new object[] { arg1, arg2, arg3, arg4 }, null);
                if (_isWarnCallSiteEnabled)
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Warn</c> level using provided message
        /// </summary>
        /// <typeparam name="TArgument1">The type of the argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the argument.</typeparam>
        /// <typeparam name="TArgument3">The type of the argument.</typeparam>
        /// <typeparam name="TArgument4">The type of the argument.</typeparam>
        /// <typeparam name="TArgument5">The type of the argument.</typeparam>
        /// <param name="message">Log event mesage</param>
        /// <param name="arg1">Arguments to format</param>
        /// <param name="arg2">Arguments to format</param>
        /// <param name="arg3">Arguments to format</param>
        /// <param name="arg4">Arguments to format</param>
        /// <param name="arg5">Arguments to format</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        [MessageTemplateFormatMethod("message")]
        public void Warn<TArgument1, TArgument2, TArgument3, TArgument4, TArgument5>([Localizable(false)] string message, TArgument1 arg1, TArgument2 arg2, TArgument3 arg3, TArgument4 arg4, TArgument5 arg5
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsWarnEnabled)
            {
                var logEvent = new LogEventInfo(LogLevel.Warn, Name, null, message, new object[] { arg1, arg2, arg3, arg4, arg5 });
                if (_isWarnCallSiteEnabled)
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Warn</c> level using provided exception and message
        /// </summary>
        /// <typeparam name="TArgument1">The type of the argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the argument.</typeparam>
        /// <typeparam name="TArgument3">The type of the argument.</typeparam>
        /// <typeparam name="TArgument4">The type of the argument.</typeparam>
        /// <typeparam name="TArgument5">The type of the argument.</typeparam>
        /// <param name="exception">Exception to be logged.</param>
        /// <param name="message">Log event mesage</param>
        /// <param name="arg1">Arguments to format</param>
        /// <param name="arg2">Arguments to format</param>
        /// <param name="arg3">Arguments to format</param>
        /// <param name="arg4">Arguments to format</param>
        /// <param name="arg5">Arguments to format</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        [MessageTemplateFormatMethod("message")]
        public void Warn<TArgument1, TArgument2, TArgument3, TArgument4, TArgument5>(Exception exception, [Localizable(false)] string message, TArgument1 arg1, TArgument2 arg2, TArgument3 arg3, TArgument4 arg4, TArgument5 arg5
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsWarnEnabled)
            {
                var logEvent = new LogEventInfo(LogLevel.Warn, Name, null, message, new object[] { arg1, arg2, arg3, arg4, arg5 }, exception);
                if (_isWarnCallSiteEnabled)
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Warn</c> level using provided exception and message
        /// </summary>
        /// <typeparam name="TArgument1">The type of the argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the argument.</typeparam>
        /// <typeparam name="TArgument3">The type of the argument.</typeparam>
        /// <typeparam name="TArgument4">The type of the argument.</typeparam>
        /// <typeparam name="TArgument5">The type of the argument.</typeparam>
        /// <param name="exception">Exception to be logged.</param>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">Log event mesage</param>
        /// <param name="arg1">Arguments to format</param>
        /// <param name="arg2">Arguments to format</param>
        /// <param name="arg3">Arguments to format</param>
        /// <param name="arg4">Arguments to format</param>
        /// <param name="arg5">Arguments to format</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        [MessageTemplateFormatMethod("message")]
        public void Warn<TArgument1, TArgument2, TArgument3, TArgument4, TArgument5>(Exception exception, IFormatProvider formatProvider, [Localizable(false)] string message, TArgument1 arg1, TArgument2 arg2, TArgument3 arg3, TArgument4 arg4, TArgument5 arg5
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsWarnEnabled)
            {
                var logEvent = new LogEventInfo(LogLevel.Warn, Name, formatProvider, message, new object[] { arg1, arg2, arg3, arg4, arg5 }, exception);
                if (_isWarnCallSiteEnabled)
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Warn</c> level using provided message
        /// </summary>
        /// <typeparam name="TArgument1">The type of the argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the argument.</typeparam>
        /// <typeparam name="TArgument3">The type of the argument.</typeparam>
        /// <typeparam name="TArgument4">The type of the argument.</typeparam>
        /// <typeparam name="TArgument5">The type of the argument.</typeparam>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">Log event mesage</param>
        /// <param name="arg1">Arguments to format</param>
        /// <param name="arg2">Arguments to format</param>
        /// <param name="arg3">Arguments to format</param>
        /// <param name="arg4">Arguments to format</param>
        /// <param name="arg5">Arguments to format</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        [MessageTemplateFormatMethod("message")]
        public void Warn<TArgument1, TArgument2, TArgument3, TArgument4, TArgument5>(IFormatProvider formatProvider, [Localizable(false)] string message, TArgument1 arg1, TArgument2 arg2, TArgument3 arg3, TArgument4 arg4, TArgument5 arg5
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsWarnEnabled)
            {
                var logEvent = new LogEventInfo(LogLevel.Warn, Name, formatProvider, message, new object[] { arg1, arg2, arg3, arg4, arg5 }, null);
                if (_isWarnCallSiteEnabled)
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }


        /// <summary>
        /// Writes log-event at specified level using provided generic value
        /// </summary>
        /// <typeparam name="T">The type of the argument.</typeparam>
        /// <param name="value">The value to be written.</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        public void Error<T>(T value
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsErrorEnabled)
            {
                var logEvent = LogEventInfo.Create(LogLevel.Error, Name, null, value);
                if (_isErrorCallSiteEnabled)
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at specified level using provided generic value
        /// </summary>
        /// <typeparam name="T">The type of the argument.</typeparam>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="value">The value to be written.</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        public void Error<T>(IFormatProvider formatProvider, T value
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsErrorEnabled)
            {
                var logEvent = LogEventInfo.Create(LogLevel.Error, Name, formatProvider, value);
                if (_isErrorCallSiteEnabled)
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }


        /// <summary>
        /// Writes log-event at <c>Error</c> level using provided message-delegate
        /// </summary>
        /// <param name="messageFunc">A function returning message to be written. Function is not evaluated if logging is not enabled.</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        public void Error(LogMessageGenerator messageFunc
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsErrorEnabled)
            {
                if (messageFunc == null)
                {
                    throw new ArgumentNullException("messageFunc");
                }

                var logEvent = new LogEventInfo(LogLevel.Error, Name, null, messageFunc(), null, null);
                if (_isErrorCallSiteEnabled)
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Error</c> level using provided exception and message-delegate
        /// </summary>
        /// <param name="exception">Exception to be logged.</param>
        /// <param name="messageFunc">A function returning message to be written. Function is not evaluated if logging is not enabled.</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        public void Error(Exception exception, LogMessageGenerator messageFunc
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsErrorEnabled)
            {
                if (messageFunc == null)
                {
                    throw new ArgumentNullException("messageFunc");
                }

                var logEvent = new LogEventInfo(LogLevel.Error, Name, null, messageFunc(), null, exception);
                if (_isErrorCallSiteEnabled)
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Error</c> level using provided message
        /// </summary>
        /// <param name="message">Log event mesage</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        [MessageTemplateFormatMethod("message")]
        public void Error([Localizable(false)] string message
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsErrorEnabled)
            {
                var logEvent = new LogEventInfo(LogLevel.Error, Name, message);
                if (_isErrorCallSiteEnabled)
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Error</c> level using provided exception and message
        /// </summary>
        /// <param name="exception">Exception to be logged.</param>
        /// <param name="message">Log event mesage</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        [MessageTemplateFormatMethod("message")]
        public void Error(Exception exception, [Localizable(false)] string message
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsErrorEnabled)
            {
                var logEvent = new LogEventInfo(LogLevel.Error, Name, null, message, null, exception);
                if (_isErrorCallSiteEnabled)
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Error</c> level using provided message (without CallSite details)
        /// </summary>
        /// <param name="message">Log event mesage</param>
        /// <param name="args">Arguments to format.</param>
        [MessageTemplateFormatMethod("message")]
        public void Error([Localizable(false)] string message, params object[] args)
        { 
            if (IsErrorEnabled)
            {
                var logEvent = new LogEventInfo(LogLevel.Error, Name, null, message, args, null);
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Error</c> level using provided message (without CallSite details)
        /// </summary>
        /// <param name="exception">Exception to be logged.</param>
        /// <param name="message">Log event mesage</param>
        /// <param name="args">Arguments to format.</param>
        [MessageTemplateFormatMethod("message")]
        public void Error(Exception exception, [Localizable(false)] string message, params object[] args)
        { 
            if (IsErrorEnabled)
            {
                var logEvent = new LogEventInfo(LogLevel.Error, Name, null, message, args, exception);
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Error</c> level using provided exeption and message (without CallSite details)
        /// </summary>
        /// <param name="exception">Exception to be logged.</param>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">Log event mesage</param>
        /// <param name="args">Arguments to format.</param>
        [MessageTemplateFormatMethod("message")]
        public void Error(Exception exception, IFormatProvider formatProvider, [Localizable(false)] string message, params object[] args)
        { 
            if (IsErrorEnabled)
            {
                var logEvent = new LogEventInfo(LogLevel.Error, Name, formatProvider, message, args, exception);
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Error</c> level using provided message (without CallSite details)
        /// </summary>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">Log event mesage</param>
        /// <param name="args">Arguments to format.</param>
        [MessageTemplateFormatMethod("message")]
        public void Error(IFormatProvider formatProvider, [Localizable(false)] string message, params object[] args)
        { 
            if (IsErrorEnabled)
            {
                var logEvent = new LogEventInfo(LogLevel.Error, Name, formatProvider, message, args, null);
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Error</c> level using provided message
        /// </summary>
        /// <typeparam name="TArgument1">The type of the argument.</typeparam>
        /// <param name="message">Log event mesage</param>
        /// <param name="arg1">Arguments to format</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        [MessageTemplateFormatMethod("message")]
        public void Error<TArgument1>([Localizable(false)] string message, TArgument1 arg1
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsErrorEnabled)
            {
                var logEvent = new LogEventInfo(LogLevel.Error, Name, null, message, new object[] { arg1 });
                if (_isErrorCallSiteEnabled)
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Error</c> level using provided exception and message
        /// </summary>
        /// <typeparam name="TArgument1">The type of the argument.</typeparam>
        /// <param name="exception">Exception to be logged.</param>
        /// <param name="message">Log event mesage</param>
        /// <param name="arg1">Arguments to format</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        [MessageTemplateFormatMethod("message")]
        public void Error<TArgument1>(Exception exception, [Localizable(false)] string message, TArgument1 arg1
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsErrorEnabled)
            {
                var logEvent = new LogEventInfo(LogLevel.Error, Name, null, message, new object[] { arg1 }, exception);
                if (_isErrorCallSiteEnabled)
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Error</c> level using provided exception and message
        /// </summary>
        /// <typeparam name="TArgument1">The type of the argument.</typeparam>
        /// <param name="exception">Exception to be logged.</param>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">Log event mesage</param>
        /// <param name="arg1">Arguments to format</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        [MessageTemplateFormatMethod("message")]
        public void Error<TArgument1>(Exception exception, IFormatProvider formatProvider, [Localizable(false)] string message, TArgument1 arg1
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsErrorEnabled)
            {
                var logEvent = new LogEventInfo(LogLevel.Error, Name, formatProvider, message, new object[] { arg1 }, exception);
                if (_isErrorCallSiteEnabled)
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Error</c> level using provided message
        /// </summary>
        /// <typeparam name="TArgument1">The type of the argument.</typeparam>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">Log event mesage</param>
        /// <param name="arg1">Arguments to format</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        [MessageTemplateFormatMethod("message")]
        public void Error<TArgument1>(IFormatProvider formatProvider, [Localizable(false)] string message, TArgument1 arg1
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsErrorEnabled)
            {
                var logEvent = new LogEventInfo(LogLevel.Error, Name, formatProvider, message, new object[] { arg1 }, null);
                if (_isErrorCallSiteEnabled)
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Error</c> level using provided message
        /// </summary>
        /// <typeparam name="TArgument1">The type of the argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the argument.</typeparam>
        /// <param name="message">Log event mesage</param>
        /// <param name="arg1">Arguments to format</param>
        /// <param name="arg2">Arguments to format</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        [MessageTemplateFormatMethod("message")]
        public void Error<TArgument1, TArgument2>([Localizable(false)] string message, TArgument1 arg1, TArgument2 arg2
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsErrorEnabled)
            {
                var logEvent = new LogEventInfo(LogLevel.Error, Name, null, message, new object[] { arg1, arg2 });
                if (_isErrorCallSiteEnabled)
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Error</c> level using provided exception and message
        /// </summary>
        /// <typeparam name="TArgument1">The type of the argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the argument.</typeparam>
        /// <param name="exception">Exception to be logged.</param>
        /// <param name="message">Log event mesage</param>
        /// <param name="arg1">Arguments to format</param>
        /// <param name="arg2">Arguments to format</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        [MessageTemplateFormatMethod("message")]
        public void Error<TArgument1, TArgument2>(Exception exception, [Localizable(false)] string message, TArgument1 arg1, TArgument2 arg2
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsErrorEnabled)
            {
                var logEvent = new LogEventInfo(LogLevel.Error, Name, null, message, new object[] { arg1, arg2 }, exception);
                if (_isErrorCallSiteEnabled)
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Error</c> level using provided exception and message
        /// </summary>
        /// <typeparam name="TArgument1">The type of the argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the argument.</typeparam>
        /// <param name="exception">Exception to be logged.</param>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">Log event mesage</param>
        /// <param name="arg1">Arguments to format</param>
        /// <param name="arg2">Arguments to format</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        [MessageTemplateFormatMethod("message")]
        public void Error<TArgument1, TArgument2>(Exception exception, IFormatProvider formatProvider, [Localizable(false)] string message, TArgument1 arg1, TArgument2 arg2
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsErrorEnabled)
            {
                var logEvent = new LogEventInfo(LogLevel.Error, Name, formatProvider, message, new object[] { arg1, arg2 }, exception);
                if (_isErrorCallSiteEnabled)
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Error</c> level using provided exception and message
        /// </summary>
        /// <typeparam name="TArgument1">The type of the argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the argument.</typeparam>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">Log event mesage</param>
        /// <param name="arg1">Arguments to format</param>
        /// <param name="arg2">Arguments to format</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        [MessageTemplateFormatMethod("message")]
        public void Error<TArgument1, TArgument2>(IFormatProvider formatProvider, [Localizable(false)] string message, TArgument1 arg1, TArgument2 arg2
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsErrorEnabled)
            {
                var logEvent = new LogEventInfo(LogLevel.Error, Name, formatProvider, message, new object[] { arg1, arg2 }, null);
                if (_isErrorCallSiteEnabled)
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Error</c> level using provided message
        /// </summary>
        /// <typeparam name="TArgument1">The type of the argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the argument.</typeparam>
        /// <typeparam name="TArgument3">The type of the argument.</typeparam>
        /// <param name="message">Log event mesage</param>
        /// <param name="arg1">Arguments to format</param>
        /// <param name="arg2">Arguments to format</param>
        /// <param name="arg3">Arguments to format</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        [MessageTemplateFormatMethod("message")]
        public void Error<TArgument1, TArgument2, TArgument3>([Localizable(false)] string message, TArgument1 arg1, TArgument2 arg2, TArgument3 arg3
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsErrorEnabled)
            {
                var logEvent = new LogEventInfo(LogLevel.Error, Name, null, message, new object[] { arg1, arg2, arg3 });
                if (_isErrorCallSiteEnabled)
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Error</c> level using provided exception and message
        /// </summary>
        /// <typeparam name="TArgument1">The type of the argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the argument.</typeparam>
        /// <typeparam name="TArgument3">The type of the argument.</typeparam>
        /// <param name="exception">Exception to be logged.</param>
        /// <param name="message">Log event mesage</param>
        /// <param name="arg1">Arguments to format</param>
        /// <param name="arg2">Arguments to format</param>
        /// <param name="arg3">Arguments to format</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        [MessageTemplateFormatMethod("message")]
        public void Error<TArgument1, TArgument2, TArgument3>(Exception exception, [Localizable(false)] string message, TArgument1 arg1, TArgument2 arg2, TArgument3 arg3
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsErrorEnabled)
            {
                var logEvent = new LogEventInfo(LogLevel.Error, Name, null, message, new object[] { arg1, arg2, arg3 }, exception);
                if (_isErrorCallSiteEnabled)
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Error</c> level using provided exception and message
        /// </summary>
        /// <typeparam name="TArgument1">The type of the argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the argument.</typeparam>
        /// <typeparam name="TArgument3">The type of the argument.</typeparam>
        /// <param name="exception">Exception to be logged.</param>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">Log event mesage</param>
        /// <param name="arg1">Arguments to format</param>
        /// <param name="arg2">Arguments to format</param>
        /// <param name="arg3">Arguments to format</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        [MessageTemplateFormatMethod("message")]
        public void Error<TArgument1, TArgument2, TArgument3>(Exception exception, IFormatProvider formatProvider, [Localizable(false)] string message, TArgument1 arg1, TArgument2 arg2, TArgument3 arg3
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsErrorEnabled)
            {
                var logEvent = new LogEventInfo(LogLevel.Error, Name, formatProvider, message, new object[] { arg1, arg2, arg3 }, exception);
                if (_isErrorCallSiteEnabled)
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Error</c> level using provided message
        /// </summary>
        /// <typeparam name="TArgument1">The type of the argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the argument.</typeparam>
        /// <typeparam name="TArgument3">The type of the argument.</typeparam>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">Log event mesage</param>
        /// <param name="arg1">Arguments to format</param>
        /// <param name="arg2">Arguments to format</param>
        /// <param name="arg3">Arguments to format</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        [MessageTemplateFormatMethod("message")]
        public void Error<TArgument1, TArgument2, TArgument3>(IFormatProvider formatProvider, [Localizable(false)] string message, TArgument1 arg1, TArgument2 arg2, TArgument3 arg3
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsErrorEnabled)
            {
                var logEvent = new LogEventInfo(LogLevel.Error, Name, formatProvider, message, new object[] { arg1, arg2, arg3 }, null);
                if (_isErrorCallSiteEnabled)
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Error</c> level using provided message
        /// </summary>
        /// <typeparam name="TArgument1">The type of the argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the argument.</typeparam>
        /// <typeparam name="TArgument3">The type of the argument.</typeparam>
        /// <typeparam name="TArgument4">The type of the argument.</typeparam>
        /// <param name="message">Log event mesage</param>
        /// <param name="arg1">Arguments to format</param>
        /// <param name="arg2">Arguments to format</param>
        /// <param name="arg3">Arguments to format</param>
        /// <param name="arg4">Arguments to format</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        [MessageTemplateFormatMethod("message")]
        public void Error<TArgument1, TArgument2, TArgument3, TArgument4>([Localizable(false)] string message, TArgument1 arg1, TArgument2 arg2, TArgument3 arg3, TArgument4 arg4
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsErrorEnabled)
            {
                var logEvent = new LogEventInfo(LogLevel.Error, Name, null, message, new object[] { arg1, arg2, arg3, arg4 });
                if (_isErrorCallSiteEnabled)
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Error</c> level using provided exception and message
        /// </summary>
        /// <typeparam name="TArgument1">The type of the argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the argument.</typeparam>
        /// <typeparam name="TArgument3">The type of the argument.</typeparam>
        /// <typeparam name="TArgument4">The type of the argument.</typeparam>
        /// <param name="exception">Exception to be logged.</param>
        /// <param name="message">Log event mesage</param>
        /// <param name="arg1">Arguments to format</param>
        /// <param name="arg2">Arguments to format</param>
        /// <param name="arg3">Arguments to format</param>
        /// <param name="arg4">Arguments to format</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        [MessageTemplateFormatMethod("message")]
        public void Error<TArgument1, TArgument2, TArgument3, TArgument4>(Exception exception, [Localizable(false)] string message, TArgument1 arg1, TArgument2 arg2, TArgument3 arg3, TArgument4 arg4
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsErrorEnabled)
            {
                var logEvent = new LogEventInfo(LogLevel.Error, Name, null, message, new object[] { arg1, arg2, arg3, arg4 }, exception);
                if (_isErrorCallSiteEnabled)
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Error</c> level using provided exception and message
        /// </summary>
        /// <typeparam name="TArgument1">The type of the argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the argument.</typeparam>
        /// <typeparam name="TArgument3">The type of the argument.</typeparam>
        /// <typeparam name="TArgument4">The type of the argument.</typeparam>
        /// <param name="exception">Exception to be logged.</param>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">Log event mesage</param>
        /// <param name="arg1">Arguments to format</param>
        /// <param name="arg2">Arguments to format</param>
        /// <param name="arg3">Arguments to format</param>
        /// <param name="arg4">Arguments to format</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        [MessageTemplateFormatMethod("message")]
        public void Error<TArgument1, TArgument2, TArgument3, TArgument4>(Exception exception, IFormatProvider formatProvider, [Localizable(false)] string message, TArgument1 arg1, TArgument2 arg2, TArgument3 arg3, TArgument4 arg4
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsErrorEnabled)
            {
                var logEvent = new LogEventInfo(LogLevel.Error, Name, formatProvider, message, new object[] { arg1, arg2, arg3, arg4 }, exception);
                if (_isErrorCallSiteEnabled)
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Error</c> level using provided message
        /// </summary>
        /// <typeparam name="TArgument1">The type of the argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the argument.</typeparam>
        /// <typeparam name="TArgument3">The type of the argument.</typeparam>
        /// <typeparam name="TArgument4">The type of the argument.</typeparam>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">Log event mesage</param>
        /// <param name="arg1">Arguments to format</param>
        /// <param name="arg2">Arguments to format</param>
        /// <param name="arg3">Arguments to format</param>
        /// <param name="arg4">Arguments to format</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        [MessageTemplateFormatMethod("message")]
        public void Error<TArgument1, TArgument2, TArgument3, TArgument4>(IFormatProvider formatProvider, [Localizable(false)] string message, TArgument1 arg1, TArgument2 arg2, TArgument3 arg3, TArgument4 arg4
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsErrorEnabled)
            {
                var logEvent = new LogEventInfo(LogLevel.Error, Name, formatProvider, message, new object[] { arg1, arg2, arg3, arg4 }, null);
                if (_isErrorCallSiteEnabled)
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Error</c> level using provided message
        /// </summary>
        /// <typeparam name="TArgument1">The type of the argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the argument.</typeparam>
        /// <typeparam name="TArgument3">The type of the argument.</typeparam>
        /// <typeparam name="TArgument4">The type of the argument.</typeparam>
        /// <typeparam name="TArgument5">The type of the argument.</typeparam>
        /// <param name="message">Log event mesage</param>
        /// <param name="arg1">Arguments to format</param>
        /// <param name="arg2">Arguments to format</param>
        /// <param name="arg3">Arguments to format</param>
        /// <param name="arg4">Arguments to format</param>
        /// <param name="arg5">Arguments to format</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        [MessageTemplateFormatMethod("message")]
        public void Error<TArgument1, TArgument2, TArgument3, TArgument4, TArgument5>([Localizable(false)] string message, TArgument1 arg1, TArgument2 arg2, TArgument3 arg3, TArgument4 arg4, TArgument5 arg5
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsErrorEnabled)
            {
                var logEvent = new LogEventInfo(LogLevel.Error, Name, null, message, new object[] { arg1, arg2, arg3, arg4, arg5 });
                if (_isErrorCallSiteEnabled)
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Error</c> level using provided exception and message
        /// </summary>
        /// <typeparam name="TArgument1">The type of the argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the argument.</typeparam>
        /// <typeparam name="TArgument3">The type of the argument.</typeparam>
        /// <typeparam name="TArgument4">The type of the argument.</typeparam>
        /// <typeparam name="TArgument5">The type of the argument.</typeparam>
        /// <param name="exception">Exception to be logged.</param>
        /// <param name="message">Log event mesage</param>
        /// <param name="arg1">Arguments to format</param>
        /// <param name="arg2">Arguments to format</param>
        /// <param name="arg3">Arguments to format</param>
        /// <param name="arg4">Arguments to format</param>
        /// <param name="arg5">Arguments to format</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        [MessageTemplateFormatMethod("message")]
        public void Error<TArgument1, TArgument2, TArgument3, TArgument4, TArgument5>(Exception exception, [Localizable(false)] string message, TArgument1 arg1, TArgument2 arg2, TArgument3 arg3, TArgument4 arg4, TArgument5 arg5
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsErrorEnabled)
            {
                var logEvent = new LogEventInfo(LogLevel.Error, Name, null, message, new object[] { arg1, arg2, arg3, arg4, arg5 }, exception);
                if (_isErrorCallSiteEnabled)
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Error</c> level using provided exception and message
        /// </summary>
        /// <typeparam name="TArgument1">The type of the argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the argument.</typeparam>
        /// <typeparam name="TArgument3">The type of the argument.</typeparam>
        /// <typeparam name="TArgument4">The type of the argument.</typeparam>
        /// <typeparam name="TArgument5">The type of the argument.</typeparam>
        /// <param name="exception">Exception to be logged.</param>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">Log event mesage</param>
        /// <param name="arg1">Arguments to format</param>
        /// <param name="arg2">Arguments to format</param>
        /// <param name="arg3">Arguments to format</param>
        /// <param name="arg4">Arguments to format</param>
        /// <param name="arg5">Arguments to format</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        [MessageTemplateFormatMethod("message")]
        public void Error<TArgument1, TArgument2, TArgument3, TArgument4, TArgument5>(Exception exception, IFormatProvider formatProvider, [Localizable(false)] string message, TArgument1 arg1, TArgument2 arg2, TArgument3 arg3, TArgument4 arg4, TArgument5 arg5
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsErrorEnabled)
            {
                var logEvent = new LogEventInfo(LogLevel.Error, Name, formatProvider, message, new object[] { arg1, arg2, arg3, arg4, arg5 }, exception);
                if (_isErrorCallSiteEnabled)
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Error</c> level using provided message
        /// </summary>
        /// <typeparam name="TArgument1">The type of the argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the argument.</typeparam>
        /// <typeparam name="TArgument3">The type of the argument.</typeparam>
        /// <typeparam name="TArgument4">The type of the argument.</typeparam>
        /// <typeparam name="TArgument5">The type of the argument.</typeparam>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">Log event mesage</param>
        /// <param name="arg1">Arguments to format</param>
        /// <param name="arg2">Arguments to format</param>
        /// <param name="arg3">Arguments to format</param>
        /// <param name="arg4">Arguments to format</param>
        /// <param name="arg5">Arguments to format</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        [MessageTemplateFormatMethod("message")]
        public void Error<TArgument1, TArgument2, TArgument3, TArgument4, TArgument5>(IFormatProvider formatProvider, [Localizable(false)] string message, TArgument1 arg1, TArgument2 arg2, TArgument3 arg3, TArgument4 arg4, TArgument5 arg5
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsErrorEnabled)
            {
                var logEvent = new LogEventInfo(LogLevel.Error, Name, formatProvider, message, new object[] { arg1, arg2, arg3, arg4, arg5 }, null);
                if (_isErrorCallSiteEnabled)
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }


        /// <summary>
        /// Writes log-event at specified level using provided generic value
        /// </summary>
        /// <typeparam name="T">The type of the argument.</typeparam>
        /// <param name="value">The value to be written.</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        public void Fatal<T>(T value
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsFatalEnabled)
            {
                var logEvent = LogEventInfo.Create(LogLevel.Fatal, Name, null, value);
                if (_isFatalCallSiteEnabled)
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at specified level using provided generic value
        /// </summary>
        /// <typeparam name="T">The type of the argument.</typeparam>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="value">The value to be written.</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        public void Fatal<T>(IFormatProvider formatProvider, T value
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsFatalEnabled)
            {
                var logEvent = LogEventInfo.Create(LogLevel.Fatal, Name, formatProvider, value);
                if (_isFatalCallSiteEnabled)
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }


        /// <summary>
        /// Writes log-event at <c>Fatal</c> level using provided message-delegate
        /// </summary>
        /// <param name="messageFunc">A function returning message to be written. Function is not evaluated if logging is not enabled.</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        public void Fatal(LogMessageGenerator messageFunc
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsFatalEnabled)
            {
                if (messageFunc == null)
                {
                    throw new ArgumentNullException("messageFunc");
                }

                var logEvent = new LogEventInfo(LogLevel.Fatal, Name, null, messageFunc(), null, null);
                if (_isFatalCallSiteEnabled)
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Fatal</c> level using provided exception and message-delegate
        /// </summary>
        /// <param name="exception">Exception to be logged.</param>
        /// <param name="messageFunc">A function returning message to be written. Function is not evaluated if logging is not enabled.</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        public void Fatal(Exception exception, LogMessageGenerator messageFunc
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsFatalEnabled)
            {
                if (messageFunc == null)
                {
                    throw new ArgumentNullException("messageFunc");
                }

                var logEvent = new LogEventInfo(LogLevel.Fatal, Name, null, messageFunc(), null, exception);
                if (_isFatalCallSiteEnabled)
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Fatal</c> level using provided message
        /// </summary>
        /// <param name="message">Log event mesage</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        [MessageTemplateFormatMethod("message")]
        public void Fatal([Localizable(false)] string message
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsFatalEnabled)
            {
                var logEvent = new LogEventInfo(LogLevel.Fatal, Name, message);
                if (_isFatalCallSiteEnabled)
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Fatal</c> level using provided exception and message
        /// </summary>
        /// <param name="exception">Exception to be logged.</param>
        /// <param name="message">Log event mesage</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        [MessageTemplateFormatMethod("message")]
        public void Fatal(Exception exception, [Localizable(false)] string message
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsFatalEnabled)
            {
                var logEvent = new LogEventInfo(LogLevel.Fatal, Name, null, message, null, exception);
                if (_isFatalCallSiteEnabled)
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Fatal</c> level using provided message (without CallSite details)
        /// </summary>
        /// <param name="message">Log event mesage</param>
        /// <param name="args">Arguments to format.</param>
        [MessageTemplateFormatMethod("message")]
        public void Fatal([Localizable(false)] string message, params object[] args)
        { 
            if (IsFatalEnabled)
            {
                var logEvent = new LogEventInfo(LogLevel.Fatal, Name, null, message, args, null);
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Fatal</c> level using provided message (without CallSite details)
        /// </summary>
        /// <param name="exception">Exception to be logged.</param>
        /// <param name="message">Log event mesage</param>
        /// <param name="args">Arguments to format.</param>
        [MessageTemplateFormatMethod("message")]
        public void Fatal(Exception exception, [Localizable(false)] string message, params object[] args)
        { 
            if (IsFatalEnabled)
            {
                var logEvent = new LogEventInfo(LogLevel.Fatal, Name, null, message, args, exception);
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Fatal</c> level using provided exeption and message (without CallSite details)
        /// </summary>
        /// <param name="exception">Exception to be logged.</param>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">Log event mesage</param>
        /// <param name="args">Arguments to format.</param>
        [MessageTemplateFormatMethod("message")]
        public void Fatal(Exception exception, IFormatProvider formatProvider, [Localizable(false)] string message, params object[] args)
        { 
            if (IsFatalEnabled)
            {
                var logEvent = new LogEventInfo(LogLevel.Fatal, Name, formatProvider, message, args, exception);
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Fatal</c> level using provided message (without CallSite details)
        /// </summary>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">Log event mesage</param>
        /// <param name="args">Arguments to format.</param>
        [MessageTemplateFormatMethod("message")]
        public void Fatal(IFormatProvider formatProvider, [Localizable(false)] string message, params object[] args)
        { 
            if (IsFatalEnabled)
            {
                var logEvent = new LogEventInfo(LogLevel.Fatal, Name, formatProvider, message, args, null);
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Fatal</c> level using provided message
        /// </summary>
        /// <typeparam name="TArgument1">The type of the argument.</typeparam>
        /// <param name="message">Log event mesage</param>
        /// <param name="arg1">Arguments to format</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        [MessageTemplateFormatMethod("message")]
        public void Fatal<TArgument1>([Localizable(false)] string message, TArgument1 arg1
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsFatalEnabled)
            {
                var logEvent = new LogEventInfo(LogLevel.Fatal, Name, null, message, new object[] { arg1 });
                if (_isFatalCallSiteEnabled)
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Fatal</c> level using provided exception and message
        /// </summary>
        /// <typeparam name="TArgument1">The type of the argument.</typeparam>
        /// <param name="exception">Exception to be logged.</param>
        /// <param name="message">Log event mesage</param>
        /// <param name="arg1">Arguments to format</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        [MessageTemplateFormatMethod("message")]
        public void Fatal<TArgument1>(Exception exception, [Localizable(false)] string message, TArgument1 arg1
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsFatalEnabled)
            {
                var logEvent = new LogEventInfo(LogLevel.Fatal, Name, null, message, new object[] { arg1 }, exception);
                if (_isFatalCallSiteEnabled)
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Fatal</c> level using provided exception and message
        /// </summary>
        /// <typeparam name="TArgument1">The type of the argument.</typeparam>
        /// <param name="exception">Exception to be logged.</param>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">Log event mesage</param>
        /// <param name="arg1">Arguments to format</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        [MessageTemplateFormatMethod("message")]
        public void Fatal<TArgument1>(Exception exception, IFormatProvider formatProvider, [Localizable(false)] string message, TArgument1 arg1
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsFatalEnabled)
            {
                var logEvent = new LogEventInfo(LogLevel.Fatal, Name, formatProvider, message, new object[] { arg1 }, exception);
                if (_isFatalCallSiteEnabled)
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Fatal</c> level using provided message
        /// </summary>
        /// <typeparam name="TArgument1">The type of the argument.</typeparam>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">Log event mesage</param>
        /// <param name="arg1">Arguments to format</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        [MessageTemplateFormatMethod("message")]
        public void Fatal<TArgument1>(IFormatProvider formatProvider, [Localizable(false)] string message, TArgument1 arg1
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsFatalEnabled)
            {
                var logEvent = new LogEventInfo(LogLevel.Fatal, Name, formatProvider, message, new object[] { arg1 }, null);
                if (_isFatalCallSiteEnabled)
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Fatal</c> level using provided message
        /// </summary>
        /// <typeparam name="TArgument1">The type of the argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the argument.</typeparam>
        /// <param name="message">Log event mesage</param>
        /// <param name="arg1">Arguments to format</param>
        /// <param name="arg2">Arguments to format</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        [MessageTemplateFormatMethod("message")]
        public void Fatal<TArgument1, TArgument2>([Localizable(false)] string message, TArgument1 arg1, TArgument2 arg2
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsFatalEnabled)
            {
                var logEvent = new LogEventInfo(LogLevel.Fatal, Name, null, message, new object[] { arg1, arg2 });
                if (_isFatalCallSiteEnabled)
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Fatal</c> level using provided exception and message
        /// </summary>
        /// <typeparam name="TArgument1">The type of the argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the argument.</typeparam>
        /// <param name="exception">Exception to be logged.</param>
        /// <param name="message">Log event mesage</param>
        /// <param name="arg1">Arguments to format</param>
        /// <param name="arg2">Arguments to format</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        [MessageTemplateFormatMethod("message")]
        public void Fatal<TArgument1, TArgument2>(Exception exception, [Localizable(false)] string message, TArgument1 arg1, TArgument2 arg2
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsFatalEnabled)
            {
                var logEvent = new LogEventInfo(LogLevel.Fatal, Name, null, message, new object[] { arg1, arg2 }, exception);
                if (_isFatalCallSiteEnabled)
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Fatal</c> level using provided exception and message
        /// </summary>
        /// <typeparam name="TArgument1">The type of the argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the argument.</typeparam>
        /// <param name="exception">Exception to be logged.</param>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">Log event mesage</param>
        /// <param name="arg1">Arguments to format</param>
        /// <param name="arg2">Arguments to format</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        [MessageTemplateFormatMethod("message")]
        public void Fatal<TArgument1, TArgument2>(Exception exception, IFormatProvider formatProvider, [Localizable(false)] string message, TArgument1 arg1, TArgument2 arg2
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsFatalEnabled)
            {
                var logEvent = new LogEventInfo(LogLevel.Fatal, Name, formatProvider, message, new object[] { arg1, arg2 }, exception);
                if (_isFatalCallSiteEnabled)
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Fatal</c> level using provided exception and message
        /// </summary>
        /// <typeparam name="TArgument1">The type of the argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the argument.</typeparam>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">Log event mesage</param>
        /// <param name="arg1">Arguments to format</param>
        /// <param name="arg2">Arguments to format</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        [MessageTemplateFormatMethod("message")]
        public void Fatal<TArgument1, TArgument2>(IFormatProvider formatProvider, [Localizable(false)] string message, TArgument1 arg1, TArgument2 arg2
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsFatalEnabled)
            {
                var logEvent = new LogEventInfo(LogLevel.Fatal, Name, formatProvider, message, new object[] { arg1, arg2 }, null);
                if (_isFatalCallSiteEnabled)
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Fatal</c> level using provided message
        /// </summary>
        /// <typeparam name="TArgument1">The type of the argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the argument.</typeparam>
        /// <typeparam name="TArgument3">The type of the argument.</typeparam>
        /// <param name="message">Log event mesage</param>
        /// <param name="arg1">Arguments to format</param>
        /// <param name="arg2">Arguments to format</param>
        /// <param name="arg3">Arguments to format</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        [MessageTemplateFormatMethod("message")]
        public void Fatal<TArgument1, TArgument2, TArgument3>([Localizable(false)] string message, TArgument1 arg1, TArgument2 arg2, TArgument3 arg3
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsFatalEnabled)
            {
                var logEvent = new LogEventInfo(LogLevel.Fatal, Name, null, message, new object[] { arg1, arg2, arg3 });
                if (_isFatalCallSiteEnabled)
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Fatal</c> level using provided exception and message
        /// </summary>
        /// <typeparam name="TArgument1">The type of the argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the argument.</typeparam>
        /// <typeparam name="TArgument3">The type of the argument.</typeparam>
        /// <param name="exception">Exception to be logged.</param>
        /// <param name="message">Log event mesage</param>
        /// <param name="arg1">Arguments to format</param>
        /// <param name="arg2">Arguments to format</param>
        /// <param name="arg3">Arguments to format</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        [MessageTemplateFormatMethod("message")]
        public void Fatal<TArgument1, TArgument2, TArgument3>(Exception exception, [Localizable(false)] string message, TArgument1 arg1, TArgument2 arg2, TArgument3 arg3
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsFatalEnabled)
            {
                var logEvent = new LogEventInfo(LogLevel.Fatal, Name, null, message, new object[] { arg1, arg2, arg3 }, exception);
                if (_isFatalCallSiteEnabled)
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Fatal</c> level using provided exception and message
        /// </summary>
        /// <typeparam name="TArgument1">The type of the argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the argument.</typeparam>
        /// <typeparam name="TArgument3">The type of the argument.</typeparam>
        /// <param name="exception">Exception to be logged.</param>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">Log event mesage</param>
        /// <param name="arg1">Arguments to format</param>
        /// <param name="arg2">Arguments to format</param>
        /// <param name="arg3">Arguments to format</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        [MessageTemplateFormatMethod("message")]
        public void Fatal<TArgument1, TArgument2, TArgument3>(Exception exception, IFormatProvider formatProvider, [Localizable(false)] string message, TArgument1 arg1, TArgument2 arg2, TArgument3 arg3
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsFatalEnabled)
            {
                var logEvent = new LogEventInfo(LogLevel.Fatal, Name, formatProvider, message, new object[] { arg1, arg2, arg3 }, exception);
                if (_isFatalCallSiteEnabled)
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Fatal</c> level using provided message
        /// </summary>
        /// <typeparam name="TArgument1">The type of the argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the argument.</typeparam>
        /// <typeparam name="TArgument3">The type of the argument.</typeparam>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">Log event mesage</param>
        /// <param name="arg1">Arguments to format</param>
        /// <param name="arg2">Arguments to format</param>
        /// <param name="arg3">Arguments to format</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        [MessageTemplateFormatMethod("message")]
        public void Fatal<TArgument1, TArgument2, TArgument3>(IFormatProvider formatProvider, [Localizable(false)] string message, TArgument1 arg1, TArgument2 arg2, TArgument3 arg3
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsFatalEnabled)
            {
                var logEvent = new LogEventInfo(LogLevel.Fatal, Name, formatProvider, message, new object[] { arg1, arg2, arg3 }, null);
                if (_isFatalCallSiteEnabled)
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Fatal</c> level using provided message
        /// </summary>
        /// <typeparam name="TArgument1">The type of the argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the argument.</typeparam>
        /// <typeparam name="TArgument3">The type of the argument.</typeparam>
        /// <typeparam name="TArgument4">The type of the argument.</typeparam>
        /// <param name="message">Log event mesage</param>
        /// <param name="arg1">Arguments to format</param>
        /// <param name="arg2">Arguments to format</param>
        /// <param name="arg3">Arguments to format</param>
        /// <param name="arg4">Arguments to format</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        [MessageTemplateFormatMethod("message")]
        public void Fatal<TArgument1, TArgument2, TArgument3, TArgument4>([Localizable(false)] string message, TArgument1 arg1, TArgument2 arg2, TArgument3 arg3, TArgument4 arg4
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsFatalEnabled)
            {
                var logEvent = new LogEventInfo(LogLevel.Fatal, Name, null, message, new object[] { arg1, arg2, arg3, arg4 });
                if (_isFatalCallSiteEnabled)
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Fatal</c> level using provided exception and message
        /// </summary>
        /// <typeparam name="TArgument1">The type of the argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the argument.</typeparam>
        /// <typeparam name="TArgument3">The type of the argument.</typeparam>
        /// <typeparam name="TArgument4">The type of the argument.</typeparam>
        /// <param name="exception">Exception to be logged.</param>
        /// <param name="message">Log event mesage</param>
        /// <param name="arg1">Arguments to format</param>
        /// <param name="arg2">Arguments to format</param>
        /// <param name="arg3">Arguments to format</param>
        /// <param name="arg4">Arguments to format</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        [MessageTemplateFormatMethod("message")]
        public void Fatal<TArgument1, TArgument2, TArgument3, TArgument4>(Exception exception, [Localizable(false)] string message, TArgument1 arg1, TArgument2 arg2, TArgument3 arg3, TArgument4 arg4
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsFatalEnabled)
            {
                var logEvent = new LogEventInfo(LogLevel.Fatal, Name, null, message, new object[] { arg1, arg2, arg3, arg4 }, exception);
                if (_isFatalCallSiteEnabled)
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Fatal</c> level using provided exception and message
        /// </summary>
        /// <typeparam name="TArgument1">The type of the argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the argument.</typeparam>
        /// <typeparam name="TArgument3">The type of the argument.</typeparam>
        /// <typeparam name="TArgument4">The type of the argument.</typeparam>
        /// <param name="exception">Exception to be logged.</param>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">Log event mesage</param>
        /// <param name="arg1">Arguments to format</param>
        /// <param name="arg2">Arguments to format</param>
        /// <param name="arg3">Arguments to format</param>
        /// <param name="arg4">Arguments to format</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        [MessageTemplateFormatMethod("message")]
        public void Fatal<TArgument1, TArgument2, TArgument3, TArgument4>(Exception exception, IFormatProvider formatProvider, [Localizable(false)] string message, TArgument1 arg1, TArgument2 arg2, TArgument3 arg3, TArgument4 arg4
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsFatalEnabled)
            {
                var logEvent = new LogEventInfo(LogLevel.Fatal, Name, formatProvider, message, new object[] { arg1, arg2, arg3, arg4 }, exception);
                if (_isFatalCallSiteEnabled)
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Fatal</c> level using provided message
        /// </summary>
        /// <typeparam name="TArgument1">The type of the argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the argument.</typeparam>
        /// <typeparam name="TArgument3">The type of the argument.</typeparam>
        /// <typeparam name="TArgument4">The type of the argument.</typeparam>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">Log event mesage</param>
        /// <param name="arg1">Arguments to format</param>
        /// <param name="arg2">Arguments to format</param>
        /// <param name="arg3">Arguments to format</param>
        /// <param name="arg4">Arguments to format</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        [MessageTemplateFormatMethod("message")]
        public void Fatal<TArgument1, TArgument2, TArgument3, TArgument4>(IFormatProvider formatProvider, [Localizable(false)] string message, TArgument1 arg1, TArgument2 arg2, TArgument3 arg3, TArgument4 arg4
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsFatalEnabled)
            {
                var logEvent = new LogEventInfo(LogLevel.Fatal, Name, formatProvider, message, new object[] { arg1, arg2, arg3, arg4 }, null);
                if (_isFatalCallSiteEnabled)
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Fatal</c> level using provided message
        /// </summary>
        /// <typeparam name="TArgument1">The type of the argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the argument.</typeparam>
        /// <typeparam name="TArgument3">The type of the argument.</typeparam>
        /// <typeparam name="TArgument4">The type of the argument.</typeparam>
        /// <typeparam name="TArgument5">The type of the argument.</typeparam>
        /// <param name="message">Log event mesage</param>
        /// <param name="arg1">Arguments to format</param>
        /// <param name="arg2">Arguments to format</param>
        /// <param name="arg3">Arguments to format</param>
        /// <param name="arg4">Arguments to format</param>
        /// <param name="arg5">Arguments to format</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        [MessageTemplateFormatMethod("message")]
        public void Fatal<TArgument1, TArgument2, TArgument3, TArgument4, TArgument5>([Localizable(false)] string message, TArgument1 arg1, TArgument2 arg2, TArgument3 arg3, TArgument4 arg4, TArgument5 arg5
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsFatalEnabled)
            {
                var logEvent = new LogEventInfo(LogLevel.Fatal, Name, null, message, new object[] { arg1, arg2, arg3, arg4, arg5 });
                if (_isFatalCallSiteEnabled)
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Fatal</c> level using provided exception and message
        /// </summary>
        /// <typeparam name="TArgument1">The type of the argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the argument.</typeparam>
        /// <typeparam name="TArgument3">The type of the argument.</typeparam>
        /// <typeparam name="TArgument4">The type of the argument.</typeparam>
        /// <typeparam name="TArgument5">The type of the argument.</typeparam>
        /// <param name="exception">Exception to be logged.</param>
        /// <param name="message">Log event mesage</param>
        /// <param name="arg1">Arguments to format</param>
        /// <param name="arg2">Arguments to format</param>
        /// <param name="arg3">Arguments to format</param>
        /// <param name="arg4">Arguments to format</param>
        /// <param name="arg5">Arguments to format</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        [MessageTemplateFormatMethod("message")]
        public void Fatal<TArgument1, TArgument2, TArgument3, TArgument4, TArgument5>(Exception exception, [Localizable(false)] string message, TArgument1 arg1, TArgument2 arg2, TArgument3 arg3, TArgument4 arg4, TArgument5 arg5
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsFatalEnabled)
            {
                var logEvent = new LogEventInfo(LogLevel.Fatal, Name, null, message, new object[] { arg1, arg2, arg3, arg4, arg5 }, exception);
                if (_isFatalCallSiteEnabled)
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Fatal</c> level using provided exception and message
        /// </summary>
        /// <typeparam name="TArgument1">The type of the argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the argument.</typeparam>
        /// <typeparam name="TArgument3">The type of the argument.</typeparam>
        /// <typeparam name="TArgument4">The type of the argument.</typeparam>
        /// <typeparam name="TArgument5">The type of the argument.</typeparam>
        /// <param name="exception">Exception to be logged.</param>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">Log event mesage</param>
        /// <param name="arg1">Arguments to format</param>
        /// <param name="arg2">Arguments to format</param>
        /// <param name="arg3">Arguments to format</param>
        /// <param name="arg4">Arguments to format</param>
        /// <param name="arg5">Arguments to format</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        [MessageTemplateFormatMethod("message")]
        public void Fatal<TArgument1, TArgument2, TArgument3, TArgument4, TArgument5>(Exception exception, IFormatProvider formatProvider, [Localizable(false)] string message, TArgument1 arg1, TArgument2 arg2, TArgument3 arg3, TArgument4 arg4, TArgument5 arg5
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsFatalEnabled)
            {
                var logEvent = new LogEventInfo(LogLevel.Fatal, Name, formatProvider, message, new object[] { arg1, arg2, arg3, arg4, arg5 }, exception);
                if (_isFatalCallSiteEnabled)
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }

        /// <summary>
        /// Writes log-event at <c>Fatal</c> level using provided message
        /// </summary>
        /// <typeparam name="TArgument1">The type of the argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the argument.</typeparam>
        /// <typeparam name="TArgument3">The type of the argument.</typeparam>
        /// <typeparam name="TArgument4">The type of the argument.</typeparam>
        /// <typeparam name="TArgument5">The type of the argument.</typeparam>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">Log event mesage</param>
        /// <param name="arg1">Arguments to format</param>
        /// <param name="arg2">Arguments to format</param>
        /// <param name="arg3">Arguments to format</param>
        /// <param name="arg4">Arguments to format</param>
        /// <param name="arg5">Arguments to format</param>
        /// <param name="_">Parameter Validation Helper</param>
        /// <param name="callerMemberName">Capture Caller Method</param>
        /// <param name="callerFilePath">Capture Caller FilePath</param>
        /// <param name="callerLineNumber">Capture Caller LineNumber</param>
        [MessageTemplateFormatMethod("message")]
        public void Fatal<TArgument1, TArgument2, TArgument3, TArgument4, TArgument5>(IFormatProvider formatProvider, [Localizable(false)] string message, TArgument1 arg1, TArgument2 arg2, TArgument3 arg3, TArgument4 arg4, TArgument5 arg5
            , LogWithOptionalParameterList _ = default(LogWithOptionalParameterList)
            , [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (IsFatalEnabled)
            {
                var logEvent = new LogEventInfo(LogLevel.Fatal, Name, formatProvider, message, new object[] { arg1, arg2, arg3, arg4, arg5 }, null);
                if (_isFatalCallSiteEnabled)
                {
                    logEvent.SetCallerInfo(null, callerMemberName, callerFilePath, callerLineNumber);
                }
                WriteToTargets(null, logEvent);
            }
        }



#if !NET45
        /// <summary>
        /// Support ILoggerBasicExtensions on NET35/NET40
        /// </summary>
        [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
        public class CallerMemberNameAttribute : Attribute
        {
        }

        /// <summary>
        /// Support ILoggerBasicExtensions on NET35/NET40
        /// </summary>
        [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
        public class CallerFilePathAttribute : Attribute
        {
        }

        /// <summary>
        /// Support ILoggerBasicExtensions on NET35/NET40
        /// </summary>
        [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
        public class CallerLineNumberAttribute : Attribute
        {
        }
#endif

        /// <summary>
        /// The purpose of this type is to act as a guard between 
        /// the actual parameter list and optional parameter list.
        /// If you need to pass this type as an argument you are using
        /// the wrong overload.
        /// </summary>
        public struct LogWithOptionalParameterList
        {
            // This type has no other purpose.
        }

        private static LogEventInfo PrepareLogEventInfo(LogEventInfo logEvent)
        {
            if (logEvent.FormatProvider == null)
            {
                logEvent.FormatProvider = LogManager.LogFactory.DefaultCultureInfo;
            }
            return logEvent;
        }
    }
}