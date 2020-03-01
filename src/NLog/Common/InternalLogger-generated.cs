// 
// Copyright (c) 2004-2020 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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

namespace NLog.Common
{
    using JetBrains.Annotations;
    using System;
    using System.ComponentModel;
	
    public static partial class InternalLogger
    {
        /// <summary>
        /// Gets a value indicating whether internal log includes Trace messages.
        /// </summary>
        public static bool IsTraceEnabled => IsLogLevelEnabled(LogLevel.Trace);

        /// <summary>
        /// Gets a value indicating whether internal log includes Debug messages.
        /// </summary>
        public static bool IsDebugEnabled => IsLogLevelEnabled(LogLevel.Debug);

        /// <summary>
        /// Gets a value indicating whether internal log includes Info messages.
        /// </summary>
        public static bool IsInfoEnabled => IsLogLevelEnabled(LogLevel.Info);

        /// <summary>
        /// Gets a value indicating whether internal log includes Warn messages.
        /// </summary>
        public static bool IsWarnEnabled => IsLogLevelEnabled(LogLevel.Warn);

        /// <summary>
        /// Gets a value indicating whether internal log includes Error messages.
        /// </summary>
        public static bool IsErrorEnabled => IsLogLevelEnabled(LogLevel.Error);

        /// <summary>
        /// Gets a value indicating whether internal log includes Fatal messages.
        /// </summary>
        public static bool IsFatalEnabled => IsLogLevelEnabled(LogLevel.Fatal);


        /// <summary>
        /// Logs the specified message without an <see cref="Exception"/> at the Trace level.
        /// </summary>
        /// <param name="message">Message which may include positional parameters.</param>
        /// <param name="args">Arguments to the message.</param>
        [StringFormatMethod("message")]
        public static void Trace([Localizable(false)] string message, params object[] args)
        {
            Write(null, LogLevel.Trace, message, args);
        }

        /// <summary>
        /// Logs the specified message without an <see cref="Exception"/> at the Trace level.
        /// </summary>
        /// <param name="message">Log message.</param>
        public static void Trace([Localizable(false)] string message)
        {
            Write(null, LogLevel.Trace, message, null);
        }

        /// <summary>
        /// Logs the specified message without an <see cref="Exception"/> at the Trace level. 
        /// <paramref name="messageFunc"/> will be only called when logging is enabled for level  Trace.
        /// </summary>
        /// <param name="messageFunc">Function that returns the log message.</param>
        public static void Trace([Localizable(false)] Func<string> messageFunc)
        {
            if (IsTraceEnabled)
                Write(null, LogLevel.Trace, messageFunc(), null);
        }

        /// <summary>
        /// Logs the specified message with an <see cref="Exception"/> at the Trace level.
        /// </summary>
        /// <param name="ex">Exception to be logged.</param>
        /// <param name="message">Message which may include positional parameters.</param>
        /// <param name="args">Arguments to the message.</param>
        [StringFormatMethod("message")]
        public static void Trace(Exception ex, [Localizable(false)] string message, params object[] args)
        {
            Write(ex, LogLevel.Trace, message, args);
        }

        /// <summary>
        /// Logs the specified message without an <see cref="Exception"/> at the Trace level.
        /// </summary>
        /// <typeparam name="TArgument1">The type of the first argument.</typeparam>
        /// <param name="message">Message which may include positional parameters.</param>
        /// <param name="arg0">Argument {0} to the message.</param>
        [StringFormatMethod("message")]
        public static void Trace<TArgument1>([Localizable(false)] string message, TArgument1 arg0)
        {
            if (IsTraceEnabled)
                Log(null, LogLevel.Trace, message, arg0);
        }

        /// <summary>
        /// Logs the specified message without an <see cref="Exception"/> at the Trace level.
        /// </summary>
        /// <typeparam name="TArgument1">The type of the first argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the second argument.</typeparam>
        /// <param name="message">Message which may include positional parameters.</param>
        /// <param name="arg0">Argument {0} to the message.</param>
        /// <param name="arg1">Argument {1} to the message.</param>
        [StringFormatMethod("message")]
        public static void Trace<TArgument1, TArgument2>([Localizable(false)] string message, TArgument1 arg0, TArgument2 arg1)
        {
            if (IsTraceEnabled)
                Log(null, LogLevel.Trace, message, arg0, arg1);
        }

        /// <summary>
        /// Logs the specified message without an <see cref="Exception"/> at the Trace level.
        /// </summary>
        /// <typeparam name="TArgument1">The type of the first argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the second argument.</typeparam>
        /// <typeparam name="TArgument3">The type of the third argument.</typeparam>
        /// <param name="message">Message which may include positional parameters.</param>
        /// <param name="arg0">Argument {0} to the message.</param>
        /// <param name="arg1">Argument {1} to the message.</param>
        /// <param name="arg2">Argument {2} to the message.</param>
        [StringFormatMethod("message")]
        public static void Trace<TArgument1, TArgument2, TArgument3>([Localizable(false)] string message, TArgument1 arg0, TArgument2 arg1, TArgument3 arg2)
        {
            if (IsTraceEnabled)
                Log(null, LogLevel.Trace, message, arg0, arg1, arg2);
        }

        /// <summary>
        /// Logs the specified message with an <see cref="Exception"/> at the Trace level.
        /// </summary>
        /// <param name="ex">Exception to be logged.</param>
        /// <param name="message">Log message.</param>
        public static void Trace(Exception ex, [Localizable(false)] string message)
        {
            Write(ex, LogLevel.Trace, message, null);
        }
		
        /// <summary>
        /// Logs the specified message with an <see cref="Exception"/> at the Trace level.
        /// <paramref name="messageFunc"/> will be only called when logging is enabled for level  Trace.
        /// </summary>
        /// <param name="ex">Exception to be logged.</param>
        /// <param name="messageFunc">Function that returns the log message.</param>
        public static void Trace(Exception ex, [Localizable(false)] Func<string> messageFunc)
        {
            if (IsTraceEnabled)
                Write(ex, LogLevel.Trace, messageFunc(), null);
        }

        /// <summary>
        /// Logs the specified message without an <see cref="Exception"/> at the Debug level.
        /// </summary>
        /// <param name="message">Message which may include positional parameters.</param>
        /// <param name="args">Arguments to the message.</param>
        [StringFormatMethod("message")]
        public static void Debug([Localizable(false)] string message, params object[] args)
        {
            Write(null, LogLevel.Debug, message, args);
        }

        /// <summary>
        /// Logs the specified message without an <see cref="Exception"/> at the Debug level.
        /// </summary>
        /// <param name="message">Log message.</param>
        public static void Debug([Localizable(false)] string message)
        {
            Write(null, LogLevel.Debug, message, null);
        }

        /// <summary>
        /// Logs the specified message without an <see cref="Exception"/> at the Debug level. 
        /// <paramref name="messageFunc"/> will be only called when logging is enabled for level  Debug.
        /// </summary>
        /// <param name="messageFunc">Function that returns the log message.</param>
        public static void Debug([Localizable(false)] Func<string> messageFunc)
        {
            if (IsDebugEnabled)
                Write(null, LogLevel.Debug, messageFunc(), null);
        }

        /// <summary>
        /// Logs the specified message with an <see cref="Exception"/> at the Debug level.
        /// </summary>
        /// <param name="ex">Exception to be logged.</param>
        /// <param name="message">Message which may include positional parameters.</param>
        /// <param name="args">Arguments to the message.</param>
        [StringFormatMethod("message")]
        public static void Debug(Exception ex, [Localizable(false)] string message, params object[] args)
        {
            Write(ex, LogLevel.Debug, message, args);
        }

        /// <summary>
        /// Logs the specified message without an <see cref="Exception"/> at the Trace level.
        /// </summary>
        /// <typeparam name="TArgument1">The type of the first argument.</typeparam>
        /// <param name="message">Message which may include positional parameters.</param>
        /// <param name="arg0">Argument {0} to the message.</param>
        [StringFormatMethod("message")]
        public static void Debug<TArgument1>([Localizable(false)] string message, TArgument1 arg0)
        {
            if (IsDebugEnabled)
                Log(null, LogLevel.Debug, message, arg0);
        }

        /// <summary>
        /// Logs the specified message without an <see cref="Exception"/> at the Trace level.
        /// </summary>
        /// <typeparam name="TArgument1">The type of the first argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the second argument.</typeparam>
        /// <param name="message">Message which may include positional parameters.</param>
        /// <param name="arg0">Argument {0} to the message.</param>
        /// <param name="arg1">Argument {1} to the message.</param>
        [StringFormatMethod("message")]
        public static void Debug<TArgument1, TArgument2>([Localizable(false)] string message, TArgument1 arg0, TArgument2 arg1)
        {
            if (IsDebugEnabled)
                Log(null, LogLevel.Debug, message, arg0, arg1);
        }

        /// <summary>
        /// Logs the specified message without an <see cref="Exception"/> at the Trace level.
        /// </summary>
        /// <typeparam name="TArgument1">The type of the first argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the second argument.</typeparam>
        /// <typeparam name="TArgument3">The type of the third argument.</typeparam>
        /// <param name="message">Message which may include positional parameters.</param>
        /// <param name="arg0">Argument {0} to the message.</param>
        /// <param name="arg1">Argument {1} to the message.</param>
        /// <param name="arg2">Argument {2} to the message.</param>
        [StringFormatMethod("message")]
        public static void Debug<TArgument1, TArgument2, TArgument3>([Localizable(false)] string message, TArgument1 arg0, TArgument2 arg1, TArgument3 arg2)
        {
            if (IsDebugEnabled)
                Log(null, LogLevel.Debug, message, arg0, arg1, arg2);
        }

        /// <summary>
        /// Logs the specified message with an <see cref="Exception"/> at the Debug level.
        /// </summary>
        /// <param name="ex">Exception to be logged.</param>
        /// <param name="message">Log message.</param>
        public static void Debug(Exception ex, [Localizable(false)] string message)
        {
            Write(ex, LogLevel.Debug, message, null);
        }
		
        /// <summary>
        /// Logs the specified message with an <see cref="Exception"/> at the Debug level.
        /// <paramref name="messageFunc"/> will be only called when logging is enabled for level  Debug.
        /// </summary>
        /// <param name="ex">Exception to be logged.</param>
        /// <param name="messageFunc">Function that returns the log message.</param>
        public static void Debug(Exception ex, [Localizable(false)] Func<string> messageFunc)
        {
            if (IsDebugEnabled)
                Write(ex, LogLevel.Debug, messageFunc(), null);
        }

        /// <summary>
        /// Logs the specified message without an <see cref="Exception"/> at the Info level.
        /// </summary>
        /// <param name="message">Message which may include positional parameters.</param>
        /// <param name="args">Arguments to the message.</param>
        [StringFormatMethod("message")]
        public static void Info([Localizable(false)] string message, params object[] args)
        {
            Write(null, LogLevel.Info, message, args);
        }

        /// <summary>
        /// Logs the specified message without an <see cref="Exception"/> at the Info level.
        /// </summary>
        /// <param name="message">Log message.</param>
        public static void Info([Localizable(false)] string message)
        {
            Write(null, LogLevel.Info, message, null);
        }

        /// <summary>
        /// Logs the specified message without an <see cref="Exception"/> at the Info level. 
        /// <paramref name="messageFunc"/> will be only called when logging is enabled for level  Info.
        /// </summary>
        /// <param name="messageFunc">Function that returns the log message.</param>
        public static void Info([Localizable(false)] Func<string> messageFunc)
        {
            if (IsInfoEnabled)
                Write(null, LogLevel.Info, messageFunc(), null);
        }

        /// <summary>
        /// Logs the specified message with an <see cref="Exception"/> at the Info level.
        /// </summary>
        /// <param name="ex">Exception to be logged.</param>
        /// <param name="message">Message which may include positional parameters.</param>
        /// <param name="args">Arguments to the message.</param>
        [StringFormatMethod("message")]
        public static void Info(Exception ex, [Localizable(false)] string message, params object[] args)
        {
            Write(ex, LogLevel.Info, message, args);
        }

        /// <summary>
        /// Logs the specified message without an <see cref="Exception"/> at the Trace level.
        /// </summary>
        /// <typeparam name="TArgument1">The type of the first argument.</typeparam>
        /// <param name="message">Message which may include positional parameters.</param>
        /// <param name="arg0">Argument {0} to the message.</param>
        [StringFormatMethod("message")]
        public static void Info<TArgument1>([Localizable(false)] string message, TArgument1 arg0)
        {
            if (IsInfoEnabled)
                Log(null, LogLevel.Info, message, arg0);
        }

        /// <summary>
        /// Logs the specified message without an <see cref="Exception"/> at the Trace level.
        /// </summary>
        /// <typeparam name="TArgument1">The type of the first argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the second argument.</typeparam>
        /// <param name="message">Message which may include positional parameters.</param>
        /// <param name="arg0">Argument {0} to the message.</param>
        /// <param name="arg1">Argument {1} to the message.</param>
        [StringFormatMethod("message")]
        public static void Info<TArgument1, TArgument2>([Localizable(false)] string message, TArgument1 arg0, TArgument2 arg1)
        {
            if (IsInfoEnabled)
                Log(null, LogLevel.Info, message, arg0, arg1);
        }

        /// <summary>
        /// Logs the specified message without an <see cref="Exception"/> at the Trace level.
        /// </summary>
        /// <typeparam name="TArgument1">The type of the first argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the second argument.</typeparam>
        /// <typeparam name="TArgument3">The type of the third argument.</typeparam>
        /// <param name="message">Message which may include positional parameters.</param>
        /// <param name="arg0">Argument {0} to the message.</param>
        /// <param name="arg1">Argument {1} to the message.</param>
        /// <param name="arg2">Argument {2} to the message.</param>
        [StringFormatMethod("message")]
        public static void Info<TArgument1, TArgument2, TArgument3>([Localizable(false)] string message, TArgument1 arg0, TArgument2 arg1, TArgument3 arg2)
        {
            if (IsInfoEnabled)
                Log(null, LogLevel.Info, message, arg0, arg1, arg2);
        }

        /// <summary>
        /// Logs the specified message with an <see cref="Exception"/> at the Info level.
        /// </summary>
        /// <param name="ex">Exception to be logged.</param>
        /// <param name="message">Log message.</param>
        public static void Info(Exception ex, [Localizable(false)] string message)
        {
            Write(ex, LogLevel.Info, message, null);
        }
		
        /// <summary>
        /// Logs the specified message with an <see cref="Exception"/> at the Info level.
        /// <paramref name="messageFunc"/> will be only called when logging is enabled for level  Info.
        /// </summary>
        /// <param name="ex">Exception to be logged.</param>
        /// <param name="messageFunc">Function that returns the log message.</param>
        public static void Info(Exception ex, [Localizable(false)] Func<string> messageFunc)
        {
            if (IsInfoEnabled)
                Write(ex, LogLevel.Info, messageFunc(), null);
        }

        /// <summary>
        /// Logs the specified message without an <see cref="Exception"/> at the Warn level.
        /// </summary>
        /// <param name="message">Message which may include positional parameters.</param>
        /// <param name="args">Arguments to the message.</param>
        [StringFormatMethod("message")]
        public static void Warn([Localizable(false)] string message, params object[] args)
        {
            Write(null, LogLevel.Warn, message, args);
        }

        /// <summary>
        /// Logs the specified message without an <see cref="Exception"/> at the Warn level.
        /// </summary>
        /// <param name="message">Log message.</param>
        public static void Warn([Localizable(false)] string message)
        {
            Write(null, LogLevel.Warn, message, null);
        }

        /// <summary>
        /// Logs the specified message without an <see cref="Exception"/> at the Warn level. 
        /// <paramref name="messageFunc"/> will be only called when logging is enabled for level  Warn.
        /// </summary>
        /// <param name="messageFunc">Function that returns the log message.</param>
        public static void Warn([Localizable(false)] Func<string> messageFunc)
        {
            if (IsWarnEnabled)
                Write(null, LogLevel.Warn, messageFunc(), null);
        }

        /// <summary>
        /// Logs the specified message with an <see cref="Exception"/> at the Warn level.
        /// </summary>
        /// <param name="ex">Exception to be logged.</param>
        /// <param name="message">Message which may include positional parameters.</param>
        /// <param name="args">Arguments to the message.</param>
        [StringFormatMethod("message")]
        public static void Warn(Exception ex, [Localizable(false)] string message, params object[] args)
        {
            Write(ex, LogLevel.Warn, message, args);
        }

        /// <summary>
        /// Logs the specified message without an <see cref="Exception"/> at the Trace level.
        /// </summary>
        /// <typeparam name="TArgument1">The type of the first argument.</typeparam>
        /// <param name="message">Message which may include positional parameters.</param>
        /// <param name="arg0">Argument {0} to the message.</param>
        [StringFormatMethod("message")]
        public static void Warn<TArgument1>([Localizable(false)] string message, TArgument1 arg0)
        {
            if (IsWarnEnabled)
                Log(null, LogLevel.Warn, message, arg0);
        }

        /// <summary>
        /// Logs the specified message without an <see cref="Exception"/> at the Trace level.
        /// </summary>
        /// <typeparam name="TArgument1">The type of the first argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the second argument.</typeparam>
        /// <param name="message">Message which may include positional parameters.</param>
        /// <param name="arg0">Argument {0} to the message.</param>
        /// <param name="arg1">Argument {1} to the message.</param>
        [StringFormatMethod("message")]
        public static void Warn<TArgument1, TArgument2>([Localizable(false)] string message, TArgument1 arg0, TArgument2 arg1)
        {
            if (IsWarnEnabled)
                Log(null, LogLevel.Warn, message, arg0, arg1);
        }

        /// <summary>
        /// Logs the specified message without an <see cref="Exception"/> at the Trace level.
        /// </summary>
        /// <typeparam name="TArgument1">The type of the first argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the second argument.</typeparam>
        /// <typeparam name="TArgument3">The type of the third argument.</typeparam>
        /// <param name="message">Message which may include positional parameters.</param>
        /// <param name="arg0">Argument {0} to the message.</param>
        /// <param name="arg1">Argument {1} to the message.</param>
        /// <param name="arg2">Argument {2} to the message.</param>
        [StringFormatMethod("message")]
        public static void Warn<TArgument1, TArgument2, TArgument3>([Localizable(false)] string message, TArgument1 arg0, TArgument2 arg1, TArgument3 arg2)
        {
            if (IsWarnEnabled)
                Log(null, LogLevel.Warn, message, arg0, arg1, arg2);
        }

        /// <summary>
        /// Logs the specified message with an <see cref="Exception"/> at the Warn level.
        /// </summary>
        /// <param name="ex">Exception to be logged.</param>
        /// <param name="message">Log message.</param>
        public static void Warn(Exception ex, [Localizable(false)] string message)
        {
            Write(ex, LogLevel.Warn, message, null);
        }
		
        /// <summary>
        /// Logs the specified message with an <see cref="Exception"/> at the Warn level.
        /// <paramref name="messageFunc"/> will be only called when logging is enabled for level  Warn.
        /// </summary>
        /// <param name="ex">Exception to be logged.</param>
        /// <param name="messageFunc">Function that returns the log message.</param>
        public static void Warn(Exception ex, [Localizable(false)] Func<string> messageFunc)
        {
            if (IsWarnEnabled)
                Write(ex, LogLevel.Warn, messageFunc(), null);
        }

        /// <summary>
        /// Logs the specified message without an <see cref="Exception"/> at the Error level.
        /// </summary>
        /// <param name="message">Message which may include positional parameters.</param>
        /// <param name="args">Arguments to the message.</param>
        [StringFormatMethod("message")]
        public static void Error([Localizable(false)] string message, params object[] args)
        {
            Write(null, LogLevel.Error, message, args);
        }

        /// <summary>
        /// Logs the specified message without an <see cref="Exception"/> at the Error level.
        /// </summary>
        /// <param name="message">Log message.</param>
        public static void Error([Localizable(false)] string message)
        {
            Write(null, LogLevel.Error, message, null);
        }

        /// <summary>
        /// Logs the specified message without an <see cref="Exception"/> at the Error level. 
        /// <paramref name="messageFunc"/> will be only called when logging is enabled for level  Error.
        /// </summary>
        /// <param name="messageFunc">Function that returns the log message.</param>
        public static void Error([Localizable(false)] Func<string> messageFunc)
        {
            if (IsErrorEnabled)
                Write(null, LogLevel.Error, messageFunc(), null);
        }

        /// <summary>
        /// Logs the specified message with an <see cref="Exception"/> at the Error level.
        /// </summary>
        /// <param name="ex">Exception to be logged.</param>
        /// <param name="message">Message which may include positional parameters.</param>
        /// <param name="args">Arguments to the message.</param>
        [StringFormatMethod("message")]
        public static void Error(Exception ex, [Localizable(false)] string message, params object[] args)
        {
            Write(ex, LogLevel.Error, message, args);
        }

        /// <summary>
        /// Logs the specified message without an <see cref="Exception"/> at the Trace level.
        /// </summary>
        /// <typeparam name="TArgument1">The type of the first argument.</typeparam>
        /// <param name="message">Message which may include positional parameters.</param>
        /// <param name="arg0">Argument {0} to the message.</param>
        [StringFormatMethod("message")]
        public static void Error<TArgument1>([Localizable(false)] string message, TArgument1 arg0)
        {
            if (IsErrorEnabled)
                Log(null, LogLevel.Error, message, arg0);
        }

        /// <summary>
        /// Logs the specified message without an <see cref="Exception"/> at the Trace level.
        /// </summary>
        /// <typeparam name="TArgument1">The type of the first argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the second argument.</typeparam>
        /// <param name="message">Message which may include positional parameters.</param>
        /// <param name="arg0">Argument {0} to the message.</param>
        /// <param name="arg1">Argument {1} to the message.</param>
        [StringFormatMethod("message")]
        public static void Error<TArgument1, TArgument2>([Localizable(false)] string message, TArgument1 arg0, TArgument2 arg1)
        {
            if (IsErrorEnabled)
                Log(null, LogLevel.Error, message, arg0, arg1);
        }

        /// <summary>
        /// Logs the specified message without an <see cref="Exception"/> at the Trace level.
        /// </summary>
        /// <typeparam name="TArgument1">The type of the first argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the second argument.</typeparam>
        /// <typeparam name="TArgument3">The type of the third argument.</typeparam>
        /// <param name="message">Message which may include positional parameters.</param>
        /// <param name="arg0">Argument {0} to the message.</param>
        /// <param name="arg1">Argument {1} to the message.</param>
        /// <param name="arg2">Argument {2} to the message.</param>
        [StringFormatMethod("message")]
        public static void Error<TArgument1, TArgument2, TArgument3>([Localizable(false)] string message, TArgument1 arg0, TArgument2 arg1, TArgument3 arg2)
        {
            if (IsErrorEnabled)
                Log(null, LogLevel.Error, message, arg0, arg1, arg2);
        }

        /// <summary>
        /// Logs the specified message with an <see cref="Exception"/> at the Error level.
        /// </summary>
        /// <param name="ex">Exception to be logged.</param>
        /// <param name="message">Log message.</param>
        public static void Error(Exception ex, [Localizable(false)] string message)
        {
            Write(ex, LogLevel.Error, message, null);
        }
		
        /// <summary>
        /// Logs the specified message with an <see cref="Exception"/> at the Error level.
        /// <paramref name="messageFunc"/> will be only called when logging is enabled for level  Error.
        /// </summary>
        /// <param name="ex">Exception to be logged.</param>
        /// <param name="messageFunc">Function that returns the log message.</param>
        public static void Error(Exception ex, [Localizable(false)] Func<string> messageFunc)
        {
            if (IsErrorEnabled)
                Write(ex, LogLevel.Error, messageFunc(), null);
        }

        /// <summary>
        /// Logs the specified message without an <see cref="Exception"/> at the Fatal level.
        /// </summary>
        /// <param name="message">Message which may include positional parameters.</param>
        /// <param name="args">Arguments to the message.</param>
        [StringFormatMethod("message")]
        public static void Fatal([Localizable(false)] string message, params object[] args)
        {
            Write(null, LogLevel.Fatal, message, args);
        }

        /// <summary>
        /// Logs the specified message without an <see cref="Exception"/> at the Fatal level.
        /// </summary>
        /// <param name="message">Log message.</param>
        public static void Fatal([Localizable(false)] string message)
        {
            Write(null, LogLevel.Fatal, message, null);
        }

        /// <summary>
        /// Logs the specified message without an <see cref="Exception"/> at the Fatal level. 
        /// <paramref name="messageFunc"/> will be only called when logging is enabled for level  Fatal.
        /// </summary>
        /// <param name="messageFunc">Function that returns the log message.</param>
        public static void Fatal([Localizable(false)] Func<string> messageFunc)
        {
            if (IsFatalEnabled)
                Write(null, LogLevel.Fatal, messageFunc(), null);
        }

        /// <summary>
        /// Logs the specified message with an <see cref="Exception"/> at the Fatal level.
        /// </summary>
        /// <param name="ex">Exception to be logged.</param>
        /// <param name="message">Message which may include positional parameters.</param>
        /// <param name="args">Arguments to the message.</param>
        [StringFormatMethod("message")]
        public static void Fatal(Exception ex, [Localizable(false)] string message, params object[] args)
        {
            Write(ex, LogLevel.Fatal, message, args);
        }

        /// <summary>
        /// Logs the specified message without an <see cref="Exception"/> at the Trace level.
        /// </summary>
        /// <typeparam name="TArgument1">The type of the first argument.</typeparam>
        /// <param name="message">Message which may include positional parameters.</param>
        /// <param name="arg0">Argument {0} to the message.</param>
        [StringFormatMethod("message")]
        public static void Fatal<TArgument1>([Localizable(false)] string message, TArgument1 arg0)
        {
            if (IsFatalEnabled)
                Log(null, LogLevel.Fatal, message, arg0);
        }

        /// <summary>
        /// Logs the specified message without an <see cref="Exception"/> at the Trace level.
        /// </summary>
        /// <typeparam name="TArgument1">The type of the first argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the second argument.</typeparam>
        /// <param name="message">Message which may include positional parameters.</param>
        /// <param name="arg0">Argument {0} to the message.</param>
        /// <param name="arg1">Argument {1} to the message.</param>
        [StringFormatMethod("message")]
        public static void Fatal<TArgument1, TArgument2>([Localizable(false)] string message, TArgument1 arg0, TArgument2 arg1)
        {
            if (IsFatalEnabled)
                Log(null, LogLevel.Fatal, message, arg0, arg1);
        }

        /// <summary>
        /// Logs the specified message without an <see cref="Exception"/> at the Trace level.
        /// </summary>
        /// <typeparam name="TArgument1">The type of the first argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the second argument.</typeparam>
        /// <typeparam name="TArgument3">The type of the third argument.</typeparam>
        /// <param name="message">Message which may include positional parameters.</param>
        /// <param name="arg0">Argument {0} to the message.</param>
        /// <param name="arg1">Argument {1} to the message.</param>
        /// <param name="arg2">Argument {2} to the message.</param>
        [StringFormatMethod("message")]
        public static void Fatal<TArgument1, TArgument2, TArgument3>([Localizable(false)] string message, TArgument1 arg0, TArgument2 arg1, TArgument3 arg2)
        {
            if (IsFatalEnabled)
                Log(null, LogLevel.Fatal, message, arg0, arg1, arg2);
        }

        /// <summary>
        /// Logs the specified message with an <see cref="Exception"/> at the Fatal level.
        /// </summary>
        /// <param name="ex">Exception to be logged.</param>
        /// <param name="message">Log message.</param>
        public static void Fatal(Exception ex, [Localizable(false)] string message)
        {
            Write(ex, LogLevel.Fatal, message, null);
        }
		
        /// <summary>
        /// Logs the specified message with an <see cref="Exception"/> at the Fatal level.
        /// <paramref name="messageFunc"/> will be only called when logging is enabled for level  Fatal.
        /// </summary>
        /// <param name="ex">Exception to be logged.</param>
        /// <param name="messageFunc">Function that returns the log message.</param>
        public static void Fatal(Exception ex, [Localizable(false)] Func<string> messageFunc)
        {
            if (IsFatalEnabled)
                Write(ex, LogLevel.Fatal, messageFunc(), null);
        }
     
    }
}