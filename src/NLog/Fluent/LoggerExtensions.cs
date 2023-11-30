// 
// Copyright (c) 2004-2021 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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
using System.ComponentModel;

namespace NLog.Fluent
{
    /// <summary>
    /// Extension methods for NLog <see cref="Logger"/>.
    /// </summary>
    public static class LoggerExtensions
    {
        /// <summary>
        /// Obsolete and replaced by <see cref="ILoggerExtensions.ForLogEvent"/> with NLog v5.
        /// 
        /// Starts building a log event with the specified <see cref="LogLevel"/>.
        /// </summary>
        /// <param name="logger">The logger to write the log event to.</param>
        /// <param name="logLevel">The log level.</param>
        /// <returns>current <see cref="LogBuilder"/> for chaining calls.</returns>
        [CLSCompliant(false)]
        [Obsolete("Obsoleted since it allocates unnecessary. Instead use ILogger.ForLogEvent and LogEventBuilder. Obsoleted in NLog 5.0")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static LogBuilder Log(this ILogger logger, LogLevel logLevel)
        {
            var builder = new LogBuilder(logger, logLevel);
            return builder;
        }

        /// <summary>
        /// Obsolete and replaced by <see cref="ILoggerExtensions.ForTraceEvent"/> with NLog v5.
        /// 
        /// Starts building a log event at the <c>Trace</c> level.
        /// </summary>
        /// <param name="logger">The logger to write the log event to.</param>
        /// <returns>current <see cref="LogBuilder"/> for chaining calls.</returns>
        [CLSCompliant(false)]
        [Obsolete("Obsoleted since it allocates unnecessary. Instead use ILogger.ForTraceEvent() and LogEventBuilder. Obsoleted in NLog 5.0")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static LogBuilder Trace(this ILogger logger)
        {
            var builder = new LogBuilder(logger, LogLevel.Trace);
            return builder;
        }

        /// <summary>
        /// Obsolete and replaced by <see cref="ILoggerExtensions.ForDebugEvent"/> with NLog v5.
        /// 
        /// Starts building a log event at the <c>Debug</c> level.
        /// </summary>
        /// <param name="logger">The logger to write the log event to.</param>
        /// <returns>current <see cref="LogBuilder"/> for chaining calls.</returns>
        [CLSCompliant(false)]
        [Obsolete("Obsoleted since it allocates unnecessary. Instead use ILogger.ForDebugEvent() and LogEventBuilder. Obsoleted in NLog 5.0")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static LogBuilder Debug(this ILogger logger)
        {
            var builder = new LogBuilder(logger, LogLevel.Debug);
            return builder;
        }

        /// <summary>
        /// Obsolete and replaced by <see cref="ILoggerExtensions.ForInfoEvent"/> with NLog v5.
        /// 
        /// Starts building a log event at the <c>Info</c> level.
        /// </summary>
        /// <param name="logger">The logger to write the log event to.</param>
        /// <returns>current <see cref="LogBuilder"/> for chaining calls.</returns>
        [CLSCompliant(false)]
        [Obsolete("Obsoleted since it allocates unnecessary. Instead use ILogger.ForInfoEvent() and LogEventBuilder. Obsoleted in NLog 5.0")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static LogBuilder Info(this ILogger logger)
        {
            var builder = new LogBuilder(logger, LogLevel.Info);
            return builder;
        }

        /// <summary>
        /// Obsolete and replaced by <see cref="ILoggerExtensions.ForWarnEvent"/> with NLog v5.
        /// 
        /// Starts building a log event at the <c>Warn</c> level.
        /// </summary>
        /// <param name="logger">The logger to write the log event to.</param>
        /// <returns>current <see cref="LogBuilder"/> for chaining calls.</returns>
        [CLSCompliant(false)]
        [Obsolete("Obsoleted since it allocates unnecessary. Instead use ILogger.ForWarnEvent() and LogEventBuilder. Obsoleted in NLog 5.0")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static LogBuilder Warn(this ILogger logger)
        {
            var builder = new LogBuilder(logger, LogLevel.Warn);
            return builder;
        }

        /// <summary>
        /// Obsolete and replaced by <see cref="ILoggerExtensions.ForErrorEvent"/> with NLog v5.
        /// 
        /// Starts building a log event at the <c>Error</c> level.
        /// </summary>
        /// <param name="logger">The logger to write the log event to.</param>
        /// <returns>current <see cref="LogBuilder"/> for chaining calls.</returns>
        [CLSCompliant(false)]
        [Obsolete("Obsoleted since it allocates unnecessary. Instead use ILogger.ForErrorEvent() and LogEventBuilder. Obsoleted in NLog 5.0")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static LogBuilder Error(this ILogger logger)
        {
            var builder = new LogBuilder(logger, LogLevel.Error);
            return builder;
        }

        /// <summary>
        /// Obsolete and replaced by <see cref="ILoggerExtensions.ForFatalEvent"/> with NLog v5.
        /// 
        /// Starts building a log event at the <c>Fatal</c> level.
        /// </summary>
        /// <param name="logger">The logger to write the log event to.</param>
        /// <returns>current <see cref="LogBuilder"/> for chaining calls.</returns>
        [CLSCompliant(false)]
        [Obsolete("Obsoleted since it allocates unnecessary. Instead use ILogger.ForFatalEvent() and LogEventBuilder. Obsoleted in NLog 5.0")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static LogBuilder Fatal(this ILogger logger)
        {
            var builder = new LogBuilder(logger, LogLevel.Fatal);
            return builder;
        }
    }
}
