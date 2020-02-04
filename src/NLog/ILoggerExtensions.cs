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

namespace NLog
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Extensions for NLog <see cref="ILogger"/>.
    /// </summary>
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public static class ILoggerExtensions
    {
        /// <summary>
        /// Writes the diagnostic message and exception at the specified level.
        /// </summary>
        /// <param name="logger">A logger implementation that will handle the message.</param>
        /// <param name="level">The log level.</param>
        /// <param name="exception">An exception to be logged.</param>
        /// <param name="messageFunc">A function returning message to be written. Function is not evaluated if logging is not enabled.</param>
        [CLSCompliant(false)]
        public static void Log(this ILogger logger, LogLevel level, Exception exception, LogMessageGenerator messageFunc)
        {
            if (logger.IsEnabled(level))
            {
                logger.Log(level, exception, messageFunc(), null);
            }
        }

        /// <summary>
        /// Writes the diagnostic message and exception at the <c>Trace</c> level.
        /// </summary>
        /// <param name="logger">A logger implementation that will handle the message.</param>
        /// <param name="exception">An exception to be logged.</param>
        /// <param name="messageFunc">A function returning message to be written. Function is not evaluated if logging is not enabled.</param>
        [CLSCompliant(false)]
        public static void Trace(this ILogger logger, Exception exception, LogMessageGenerator messageFunc)
        {
            if (logger.IsTraceEnabled)
            {
                if (messageFunc == null)
                {
                    throw new ArgumentNullException(nameof(messageFunc));
                }

                logger.Trace(exception, messageFunc());
            }
        }

        /// <summary>
        /// Writes the diagnostic message and exception at the <c>Debug</c> level.
        /// </summary>
        /// <param name="logger">A logger implementation that will handle the message.</param>
        /// <param name="exception">An exception to be logged.</param>
        /// <param name="messageFunc">A function returning message to be written. Function is not evaluated if logging is not enabled.</param>
        [CLSCompliant(false)]
        public static void Debug(this ILogger logger, Exception exception, LogMessageGenerator messageFunc)
        {
            if (logger.IsDebugEnabled)
            {
                if (messageFunc == null)
                {
                    throw new ArgumentNullException(nameof(messageFunc));
                }

                logger.Debug(exception, messageFunc());
            }
        }

        /// <summary>
        /// Writes the diagnostic message and exception at the <c>Info</c> level.
        /// </summary>
        /// <param name="logger">A logger implementation that will handle the message.</param>
        /// <param name="exception">An exception to be logged.</param>
        /// <param name="messageFunc">A function returning message to be written. Function is not evaluated if logging is not enabled.</param>
        [CLSCompliant(false)]
        public static void Info(this ILogger logger, Exception exception, LogMessageGenerator messageFunc)
        {
            if (logger.IsInfoEnabled)
            {
                if (messageFunc == null)
                {
                    throw new ArgumentNullException(nameof(messageFunc));
                }

                logger.Info(exception, messageFunc());
            }
        }

        /// <summary>
        /// Writes the diagnostic message and exception at the <c>Warn</c> level.
        /// </summary>
        /// <param name="logger">A logger implementation that will handle the message.</param>
        /// <param name="exception">An exception to be logged.</param>
        /// <param name="messageFunc">A function returning message to be written. Function is not evaluated if logging is not enabled.</param>
        [CLSCompliant(false)]
        public static void Warn(this ILogger logger, Exception exception, LogMessageGenerator messageFunc)
        {
            if (logger.IsWarnEnabled)
            {
                if (messageFunc == null)
                {
                    throw new ArgumentNullException(nameof(messageFunc));
                }

                logger.Warn(exception, messageFunc());
            }
        }

        /// <summary>
        /// Writes the diagnostic message and exception at the <c>Error</c> level.
        /// </summary>
        /// <param name="logger">A logger implementation that will handle the message.</param>
        /// <param name="exception">An exception to be logged.</param>
        /// <param name="messageFunc">A function returning message to be written. Function is not evaluated if logging is not enabled.</param>
        [CLSCompliant(false)]
        public static void Error(this ILogger logger, Exception exception, LogMessageGenerator messageFunc)
        {
            if (logger.IsErrorEnabled)
            {
                if (messageFunc == null)
                {
                    throw new ArgumentNullException(nameof(messageFunc));
                }

                logger.Error(exception, messageFunc());
            }
        }

        /// <summary>
        /// Writes the diagnostic message and exception at the <c>Fatal</c> level.
        /// </summary>
        /// <param name="logger">A logger implementation that will handle the message.</param>
        /// <param name="exception">An exception to be logged.</param>
        /// <param name="messageFunc">A function returning message to be written. Function is not evaluated if logging is not enabled.</param>
        [CLSCompliant(false)]
        public static void Fatal(this ILogger logger, Exception exception, LogMessageGenerator messageFunc)
        {
            if (logger.IsFatalEnabled)
            {
                if (messageFunc == null)
                {
                    throw new ArgumentNullException(nameof(messageFunc));
                }

                logger.Fatal(exception, messageFunc());
            }
        }
    }
}
