// 
// Copyright (c) 2004-2011 Jaroslaw Kowalski <jaak@jkowalski.net>
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
        public static bool IsTraceEnabled
        {
            get { return LogLevel.Trace >= LogLevel; }
        }

        /// <summary>
        /// Gets a value indicating whether internal log includes Debug messages.
        /// </summary>
        public static bool IsDebugEnabled
        {
            get { return LogLevel.Debug >= LogLevel; }
        }

        /// <summary>
        /// Gets a value indicating whether internal log includes Info messages.
        /// </summary>
        public static bool IsInfoEnabled
        {
            get { return LogLevel.Info >= LogLevel; }
        }

        /// <summary>
        /// Gets a value indicating whether internal log includes Warn messages.
        /// </summary>
        public static bool IsWarnEnabled
        {
            get { return LogLevel.Warn >= LogLevel; }
        }

        /// <summary>
        /// Gets a value indicating whether internal log includes Error messages.
        /// </summary>
        public static bool IsErrorEnabled
        {
            get { return LogLevel.Error >= LogLevel; }
        }

        /// <summary>
        /// Gets a value indicating whether internal log includes Fatal messages.
        /// </summary>
        public static bool IsFatalEnabled
        {
            get { return LogLevel.Fatal >= LogLevel; }
        }


        /// <summary>
        /// Logs the specified message at the Trace level.
        /// </summary>
        /// <param name="message">Message which may include positional parameters.</param>
        /// <param name="args">Arguments to the message.</param>
        [StringFormatMethod("message")]
        public static void Trace([Localizable(false)] string message, params object[] args)
        {
            Write(LogLevel.Trace, message, args);
        }

        /// <summary>
        /// Logs the specified message at the Trace level.
        /// </summary>
        /// <param name="message">Log message.</param>
        public static void Trace([Localizable(false)] string message)
        {
            Write(LogLevel.Trace, message, null);
        }

        /// <summary>
        /// Logs the specified message at the Debug level.
        /// </summary>
        /// <param name="message">Message which may include positional parameters.</param>
        /// <param name="args">Arguments to the message.</param>
        [StringFormatMethod("message")]
        public static void Debug([Localizable(false)] string message, params object[] args)
        {
            Write(LogLevel.Debug, message, args);
        }

        /// <summary>
        /// Logs the specified message at the Debug level.
        /// </summary>
        /// <param name="message">Log message.</param>
        public static void Debug([Localizable(false)] string message)
        {
            Write(LogLevel.Debug, message, null);
        }

        /// <summary>
        /// Logs the specified message at the Info level.
        /// </summary>
        /// <param name="message">Message which may include positional parameters.</param>
        /// <param name="args">Arguments to the message.</param>
        [StringFormatMethod("message")]
        public static void Info([Localizable(false)] string message, params object[] args)
        {
            Write(LogLevel.Info, message, args);
        }

        /// <summary>
        /// Logs the specified message at the Info level.
        /// </summary>
        /// <param name="message">Log message.</param>
        public static void Info([Localizable(false)] string message)
        {
            Write(LogLevel.Info, message, null);
        }

        /// <summary>
        /// Logs the specified message at the Warn level.
        /// </summary>
        /// <param name="message">Message which may include positional parameters.</param>
        /// <param name="args">Arguments to the message.</param>
        [StringFormatMethod("message")]
        public static void Warn([Localizable(false)] string message, params object[] args)
        {
            Write(LogLevel.Warn, message, args);
        }

        /// <summary>
        /// Logs the specified message at the Warn level.
        /// </summary>
        /// <param name="message">Log message.</param>
        public static void Warn([Localizable(false)] string message)
        {
            Write(LogLevel.Warn, message, null);
        }

        /// <summary>
        /// Logs the specified message at the Error level.
        /// </summary>
        /// <param name="message">Message which may include positional parameters.</param>
        /// <param name="args">Arguments to the message.</param>
        [StringFormatMethod("message")]
        public static void Error([Localizable(false)] string message, params object[] args)
        {
            Write(LogLevel.Error, message, args);
        }

        /// <summary>
        /// Logs the specified message at the Error level.
        /// </summary>
        /// <param name="message">Log message.</param>
        public static void Error([Localizable(false)] string message)
        {
            Write(LogLevel.Error, message, null);
        }

        /// <summary>
        /// Logs the specified message at the Fatal level.
        /// </summary>
        /// <param name="message">Message which may include positional parameters.</param>
        /// <param name="args">Arguments to the message.</param>
        [StringFormatMethod("message")]
        public static void Fatal([Localizable(false)] string message, params object[] args)
        {
            Write(LogLevel.Fatal, message, args);
        }

        /// <summary>
        /// Logs the specified message at the Fatal level.
        /// </summary>
        /// <param name="message">Log message.</param>
        public static void Fatal([Localizable(false)] string message)
        {
            Write(LogLevel.Fatal, message, null);
        }
     
    }
}