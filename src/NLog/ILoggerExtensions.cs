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

namespace NLog
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
#if !NET40 && !NET35
    using System.Threading.Tasks;
#endif
    using JetBrains.Annotations;
    using NLog.Internal;

    /// <summary>
    /// Extensions for NLog <see cref="ILogger"/>.
    /// </summary>
    [CLSCompliant(false)]
    public static class ILoggerExtensions
    {
        /// <summary>
        /// Starts building a log event with the specified <see cref="LogLevel"/>.
        /// </summary>
        /// <param name="logger">The logger to write the log event to.</param>
        /// <param name="logLevel">The log level. When not</param>
        /// <returns><see cref="LogEventBuilder"/> for chaining calls.</returns>
        public static LogEventBuilder ForLogEvent([NotNull] this ILogger logger, LogLevel logLevel = null)
        {
            return logLevel is null ? new LogEventBuilder(logger) : new LogEventBuilder(logger, logLevel);
        }

        /// <summary>
        /// Starts building a log event at the <c>Trace</c> level.
        /// </summary>
        /// <param name="logger">The logger to write the log event to.</param>
        /// <returns><see cref="LogEventBuilder"/> for chaining calls.</returns>
        public static LogEventBuilder ForTraceEvent([NotNull] this ILogger logger)
        {
            return new LogEventBuilder(logger, LogLevel.Trace);
        }

        /// <summary>
        /// Starts building a log event at the <c>Debug</c> level.
        /// </summary>
        /// <param name="logger">The logger to write the log event to.</param>
        /// <returns><see cref="LogEventBuilder"/> for chaining calls.</returns>
        public static LogEventBuilder ForDebugEvent([NotNull] this ILogger logger)
        {
            return new LogEventBuilder(logger, LogLevel.Debug);
        }

        /// <summary>
        /// Starts building a log event at the <c>Info</c> level.
        /// </summary>
        /// <param name="logger">The logger to write the log event to.</param>
        /// <returns><see cref="LogEventBuilder"/> for chaining calls.</returns>
        public static LogEventBuilder ForInfoEvent([NotNull] this ILogger logger)
        {
            return new LogEventBuilder(logger, LogLevel.Info);
        }

        /// <summary>
        /// Starts building a log event at the <c>Warn</c> level.
        /// </summary>
        /// <param name="logger">The logger to write the log event to.</param>
        /// <returns><see cref="LogEventBuilder"/> for chaining calls.</returns>
        public static LogEventBuilder ForWarnEvent([NotNull] this ILogger logger)
        {
            return new LogEventBuilder(logger, LogLevel.Warn);
        }

        /// <summary>
        /// Starts building a log event at the <c>Error</c> level.
        /// </summary>
        /// <param name="logger">The logger to write the log event to.</param>
        /// <returns><see cref="LogEventBuilder"/> for chaining calls.</returns>
        public static LogEventBuilder ForErrorEvent([NotNull] this ILogger logger)
        {
            return new LogEventBuilder(logger, LogLevel.Error);
        }

        /// <summary>
        /// Starts building a log event at the <c>Fatal</c> level.
        /// </summary>
        /// <param name="logger">The logger to write the log event to.</param>
        /// <returns><see cref="LogEventBuilder"/> for chaining calls.</returns>
        public static LogEventBuilder ForFatalEvent([NotNull] this ILogger logger)
        {
            return new LogEventBuilder(logger, LogLevel.Fatal);
        }

        /// <summary>
        /// Starts building a log event at the <c>Exception</c> level.
        /// </summary>
        /// <param name="logger">The logger to write the log event to.</param>
        /// <param name="exception">The exception information of the logging event.</param>
        /// <param name="logLevel">The <see cref="LogLevel"/> for the log event. Defaults to <see cref="LogLevel.Error"/> when not specified.</param>
        /// <returns><see cref="LogEventBuilder"/> for chaining calls.</returns>
        public static LogEventBuilder ForExceptionEvent([NotNull] this ILogger logger, Exception exception, LogLevel logLevel = null)
        {
            return ForLogEvent(logger, logLevel ?? LogLevel.Error).Exception(exception);
        }

        #region ConditionalDebug

        /// <overloads>
        /// Writes the diagnostic message at the <c>Debug</c> level using the specified format provider and format parameters.
        /// </overloads>
        /// <summary>
        /// Writes the diagnostic message at the <c>Debug</c> level.
        /// Only executed when the DEBUG conditional compilation symbol is set.</summary>
        /// <typeparam name="T">Type of the value.</typeparam>
        /// <param name="logger">A logger implementation that will handle the message.</param>
        /// <param name="value">The value to be written.</param>
        [Conditional("DEBUG")]
        public static void ConditionalDebug<T>([NotNull] this ILogger logger, T value)
        {
            logger.Debug(value);
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Debug</c> level.
        /// Only executed when the DEBUG conditional compilation symbol is set.</summary>
        /// <typeparam name="T">Type of the value.</typeparam>
        /// <param name="logger">A logger implementation that will handle the message.</param>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="value">The value to be written.</param>
        [Conditional("DEBUG")]
        public static void ConditionalDebug<T>([NotNull] this ILogger logger, IFormatProvider formatProvider, T value)
        {
            logger.Debug(formatProvider, value);
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Debug</c> level.
        /// Only executed when the DEBUG conditional compilation symbol is set.</summary>
        /// <param name="logger">A logger implementation that will handle the message.</param>
        /// <param name="messageFunc">A function returning message to be written. Function is not evaluated if logging is not enabled.</param>
        [Conditional("DEBUG")]
        public static void ConditionalDebug([NotNull] this ILogger logger, LogMessageGenerator messageFunc)
        {
            logger.Debug(messageFunc);
        }

        /// <summary>
        /// Writes the diagnostic message and exception at the <c>Debug</c> level.
        /// Only executed when the DEBUG conditional compilation symbol is set.</summary>
        /// <param name="logger">A logger implementation that will handle the message.</param>
        /// <param name="exception">An exception to be logged.</param>
        /// <param name="message">A <see langword="string" /> to be written.</param>
        /// <param name="args">Arguments to format.</param>
        [Conditional("DEBUG")]
        [MessageTemplateFormatMethod("message")]
        public static void ConditionalDebug([NotNull] this ILogger logger, Exception exception, [Localizable(false)][StructuredMessageTemplate] string message, params object[] args)
        {
            logger.Debug(exception, message, args);
        }

        /// <summary>
        /// Writes the diagnostic message and exception at the <c>Debug</c> level.
        /// Only executed when the DEBUG conditional compilation symbol is set.</summary>
        /// <param name="logger">A logger implementation that will handle the message.</param>
        /// <param name="exception">An exception to be logged.</param>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> to be written.</param>
        /// <param name="args">Arguments to format.</param>
        [Conditional("DEBUG")]
        [MessageTemplateFormatMethod("message")]
        public static void ConditionalDebug([NotNull] this ILogger logger, Exception exception, IFormatProvider formatProvider, [Localizable(false)][StructuredMessageTemplate] string message, params object[] args)
        {
            logger.Debug(exception, formatProvider, message, args);
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Debug</c> level.
        /// Only executed when the DEBUG conditional compilation symbol is set.</summary>
        /// <param name="logger">A logger implementation that will handle the message.</param>
        /// <param name="message">Log message.</param>
        [Conditional("DEBUG")]
        public static void ConditionalDebug([NotNull] this ILogger logger, [Localizable(false)] string message)
        {
            logger.Debug(message, default(object[]));
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Debug</c> level using the specified parameters.
        /// Only executed when the DEBUG conditional compilation symbol is set.</summary>
        /// <param name="logger">A logger implementation that will handle the message.</param>
        /// <param name="message">A <see langword="string" /> containing format items.</param>
        /// <param name="args">Arguments to format.</param>
        [Conditional("DEBUG")]
        [MessageTemplateFormatMethod("message")]
        public static void ConditionalDebug([NotNull] this ILogger logger, [Localizable(false)][StructuredMessageTemplate] string message, params object[] args)
        {
            logger.Debug(message, args);
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Debug</c> level using the specified parameters.
        /// Only executed when the DEBUG conditional compilation symbol is set.</summary>
        /// <param name="logger">A logger implementation that will handle the message.</param>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing format items.</param>
        /// <param name="args">Arguments to format.</param>
        [Conditional("DEBUG")]
        [MessageTemplateFormatMethod("message")]
        public static void ConditionalDebug([NotNull] this ILogger logger, IFormatProvider formatProvider, [Localizable(false)][StructuredMessageTemplate] string message, params object[] args)
        {
            logger.Debug(formatProvider, message, args);
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Debug</c> level using the specified parameter.
        /// Only executed when the DEBUG conditional compilation symbol is set.</summary>
        /// <typeparam name="TArgument">The type of the argument.</typeparam>
        /// <param name="logger">A logger implementation that will handle the message.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The argument to format.</param>
        [Conditional("DEBUG")]
        [MessageTemplateFormatMethod("message")]
        public static void ConditionalDebug<TArgument>([NotNull] this ILogger logger, [Localizable(false)][StructuredMessageTemplate] string message, TArgument argument)
        {
            if (logger.IsDebugEnabled)
            {
                logger.Debug(message, new object[] { argument });
            }
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Debug</c> level using the specified parameters.
        /// Only executed when the DEBUG conditional compilation symbol is set.</summary>
        /// <typeparam name="TArgument1">The type of the first argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the second argument.</typeparam>
        /// <param name="logger">A logger implementation that will handle the message.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument1">The first argument to format.</param>
        /// <param name="argument2">The second argument to format.</param>
        [Conditional("DEBUG")]
        [MessageTemplateFormatMethod("message")]
        public static void ConditionalDebug<TArgument1, TArgument2>([NotNull] this ILogger logger, [Localizable(false)][StructuredMessageTemplate] string message, TArgument1 argument1, TArgument2 argument2)
        {
            if (logger.IsDebugEnabled)
            {
                logger.Debug(message, new object[] { argument1, argument2 });
            }
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Debug</c> level using the specified parameters.
        /// Only executed when the DEBUG conditional compilation symbol is set.</summary>
        /// <typeparam name="TArgument1">The type of the first argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the second argument.</typeparam>
        /// <typeparam name="TArgument3">The type of the third argument.</typeparam>
        /// <param name="logger">A logger implementation that will handle the message.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument1">The first argument to format.</param>
        /// <param name="argument2">The second argument to format.</param>
        /// <param name="argument3">The third argument to format.</param>
        [Conditional("DEBUG")]
        [MessageTemplateFormatMethod("message")]
        public static void ConditionalDebug<TArgument1, TArgument2, TArgument3>([NotNull] this ILogger logger, [Localizable(false)][StructuredMessageTemplate] string message, TArgument1 argument1, TArgument2 argument2, TArgument3 argument3)
        {
            if (logger.IsDebugEnabled)
            {
                logger.Debug(message, new object[] { argument1, argument2, argument3 });
            }
        }

        #endregion

        #region ConditionalTrace

        /// <overloads>
        /// Writes the diagnostic message at the <c>Debug</c> level using the specified format provider and format parameters.
        /// </overloads>
        /// <summary>
        /// Writes the diagnostic message at the <c>Debug</c> level.
        /// Only executed when the DEBUG conditional compilation symbol is set.</summary>
        /// <typeparam name="T">Type of the value.</typeparam>
        /// <param name="logger">A logger implementation that will handle the message.</param>
        /// <param name="value">The value to be written.</param>
        [Conditional("DEBUG")]
        public static void ConditionalTrace<T>([NotNull] this ILogger logger, T value)
        {
            logger.Trace(value);
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Debug</c> level.
        /// Only executed when the DEBUG conditional compilation symbol is set.</summary>
        /// <typeparam name="T">Type of the value.</typeparam>
        /// <param name="logger">A logger implementation that will handle the message.</param>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="value">The value to be written.</param>
        [Conditional("DEBUG")]
        public static void ConditionalTrace<T>([NotNull] this ILogger logger, IFormatProvider formatProvider, T value)
        {
            logger.Trace(formatProvider, value);
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Debug</c> level.
        /// Only executed when the DEBUG conditional compilation symbol is set.</summary>
        /// <param name="logger">A logger implementation that will handle the message.</param>
        /// <param name="messageFunc">A function returning message to be written. Function is not evaluated if logging is not enabled.</param>
        [Conditional("DEBUG")]
        public static void ConditionalTrace([NotNull] this ILogger logger, LogMessageGenerator messageFunc)
        {
            logger.Trace(messageFunc);
        }

        /// <summary>
        /// Writes the diagnostic message and exception at the <c>Debug</c> level.
        /// Only executed when the DEBUG conditional compilation symbol is set.</summary>
        /// <param name="logger">A logger implementation that will handle the message.</param>
        /// <param name="exception">An exception to be logged.</param>
        /// <param name="message">A <see langword="string" /> to be written.</param>
        /// <param name="args">Arguments to format.</param>
        [Conditional("DEBUG")]
        [MessageTemplateFormatMethod("message")]
        public static void ConditionalTrace([NotNull] this ILogger logger, Exception exception, [Localizable(false)][StructuredMessageTemplate] string message, params object[] args)
        {
            logger.Trace(exception, message, args);
        }

        /// <summary>
        /// Writes the diagnostic message and exception at the <c>Debug</c> level.
        /// Only executed when the DEBUG conditional compilation symbol is set.</summary>
        /// <param name="logger">A logger implementation that will handle the message.</param>
        /// <param name="exception">An exception to be logged.</param>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> to be written.</param>
        /// <param name="args">Arguments to format.</param>
        [Conditional("DEBUG")]
        [MessageTemplateFormatMethod("message")]
        public static void ConditionalTrace([NotNull] this ILogger logger, Exception exception, IFormatProvider formatProvider, [Localizable(false)][StructuredMessageTemplate] string message, params object[] args)
        {
            logger.Trace(exception, formatProvider, message, args);
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Debug</c> level.
        /// Only executed when the DEBUG conditional compilation symbol is set.</summary>
        /// <param name="logger">A logger implementation that will handle the message.</param>
        /// <param name="message">Log message.</param>
        [Conditional("DEBUG")]
        public static void ConditionalTrace([NotNull] this ILogger logger, [Localizable(false)] string message)
        {
            logger.Trace(message, default(object[]));
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Debug</c> level using the specified parameters.
        /// Only executed when the DEBUG conditional compilation symbol is set.</summary>
        /// <param name="logger">A logger implementation that will handle the message.</param>
        /// <param name="message">A <see langword="string" /> containing format items.</param>
        /// <param name="args">Arguments to format.</param>
        [Conditional("DEBUG")]
        [MessageTemplateFormatMethod("message")]
        public static void ConditionalTrace([NotNull] this ILogger logger, [Localizable(false)][StructuredMessageTemplate] string message, params object[] args)
        {
            logger.Trace(message, args);
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Debug</c> level using the specified parameters.
        /// Only executed when the DEBUG conditional compilation symbol is set.</summary>
        /// <param name="logger">A logger implementation that will handle the message.</param>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing format items.</param>
        /// <param name="args">Arguments to format.</param>
        [Conditional("DEBUG")]
        [MessageTemplateFormatMethod("message")]
        public static void ConditionalTrace([NotNull] this ILogger logger, IFormatProvider formatProvider, [Localizable(false)][StructuredMessageTemplate] string message, params object[] args)
        {
            logger.Trace(formatProvider, message, args);
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Debug</c> level using the specified parameter.
        /// Only executed when the DEBUG conditional compilation symbol is set.</summary>
        /// <typeparam name="TArgument">The type of the argument.</typeparam>
        /// <param name="logger">A logger implementation that will handle the message.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The argument to format.</param>
        [Conditional("DEBUG")]
        [MessageTemplateFormatMethod("message")]
        public static void ConditionalTrace<TArgument>([NotNull] this ILogger logger, [Localizable(false)][StructuredMessageTemplate] string message, TArgument argument)
        {
            if (logger.IsTraceEnabled)
            {
                logger.Trace(message, new object[] { argument });
            }
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Debug</c> level using the specified parameters.
        /// Only executed when the DEBUG conditional compilation symbol is set.</summary>
        /// <typeparam name="TArgument1">The type of the first argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the second argument.</typeparam>
        /// <param name="logger">A logger implementation that will handle the message.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument1">The first argument to format.</param>
        /// <param name="argument2">The second argument to format.</param>
        [Conditional("DEBUG")]
        [MessageTemplateFormatMethod("message")]
        public static void ConditionalTrace<TArgument1, TArgument2>([NotNull] this ILogger logger, [Localizable(false)][StructuredMessageTemplate] string message, TArgument1 argument1, TArgument2 argument2)
        {
            if (logger.IsTraceEnabled)
            {
                logger.Trace(message, new object[] { argument1, argument2 });
            }
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Debug</c> level using the specified parameters.
        /// Only executed when the DEBUG conditional compilation symbol is set.</summary>
        /// <typeparam name="TArgument1">The type of the first argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the second argument.</typeparam>
        /// <typeparam name="TArgument3">The type of the third argument.</typeparam>
        /// <param name="logger">A logger implementation that will handle the message.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument1">The first argument to format.</param>
        /// <param name="argument2">The second argument to format.</param>
        /// <param name="argument3">The third argument to format.</param>
        [Conditional("DEBUG")]
        [MessageTemplateFormatMethod("message")]
        public static void ConditionalTrace<TArgument1, TArgument2, TArgument3>([NotNull] this ILogger logger, [Localizable(false)][StructuredMessageTemplate] string message, TArgument1 argument1, TArgument2 argument2, TArgument3 argument3)
        {
            if (logger.IsTraceEnabled)
            {
                logger.Trace(message, new object[] { argument1, argument2, argument3 });
            }
        }

        #endregion

        /// <summary>
        /// Writes the diagnostic message and exception at the specified level.
        /// </summary>
        /// <param name="logger">A logger implementation that will handle the message.</param>
        /// <param name="level">The log level.</param>
        /// <param name="exception">An exception to be logged.</param>
        /// <param name="messageFunc">A function returning message to be written. Function is not evaluated if logging is not enabled.</param>
        public static void Log([NotNull] this ILogger logger, LogLevel level, Exception exception, LogMessageGenerator messageFunc)
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
        public static void Trace([NotNull] this ILogger logger, Exception exception, LogMessageGenerator messageFunc)
        {
            if (logger.IsTraceEnabled)
            {
                Guard.ThrowIfNull(messageFunc);

                logger.Trace(exception, messageFunc());
            }
        }

        /// <summary>
        /// Writes the diagnostic message and exception at the <c>Debug</c> level.
        /// </summary>
        /// <param name="logger">A logger implementation that will handle the message.</param>
        /// <param name="exception">An exception to be logged.</param>
        /// <param name="messageFunc">A function returning message to be written. Function is not evaluated if logging is not enabled.</param>
        public static void Debug([NotNull] this ILogger logger, Exception exception, LogMessageGenerator messageFunc)
        {
            if (logger.IsDebugEnabled)
            {
                Guard.ThrowIfNull(messageFunc);

                logger.Debug(exception, messageFunc());
            }
        }

        /// <summary>
        /// Writes the diagnostic message and exception at the <c>Info</c> level.
        /// </summary>
        /// <param name="logger">A logger implementation that will handle the message.</param>
        /// <param name="exception">An exception to be logged.</param>
        /// <param name="messageFunc">A function returning message to be written. Function is not evaluated if logging is not enabled.</param>
        public static void Info([NotNull] this ILogger logger, Exception exception, LogMessageGenerator messageFunc)
        {
            if (logger.IsInfoEnabled)
            {
                Guard.ThrowIfNull(messageFunc);

                logger.Info(exception, messageFunc());
            }
        }

        /// <summary>
        /// Writes the diagnostic message and exception at the <c>Warn</c> level.
        /// </summary>
        /// <param name="logger">A logger implementation that will handle the message.</param>
        /// <param name="exception">An exception to be logged.</param>
        /// <param name="messageFunc">A function returning message to be written. Function is not evaluated if logging is not enabled.</param>
        public static void Warn([NotNull] this ILogger logger, Exception exception, LogMessageGenerator messageFunc)
        {
            if (logger.IsWarnEnabled)
            {
                Guard.ThrowIfNull(messageFunc);

                logger.Warn(exception, messageFunc());
            }
        }

        /// <summary>
        /// Writes the diagnostic message and exception at the <c>Error</c> level.
        /// </summary>
        /// <param name="logger">A logger implementation that will handle the message.</param>
        /// <param name="exception">An exception to be logged.</param>
        /// <param name="messageFunc">A function returning message to be written. Function is not evaluated if logging is not enabled.</param>
        public static void Error([NotNull] this ILogger logger, Exception exception, LogMessageGenerator messageFunc)
        {
            if (logger.IsErrorEnabled)
            {
                Guard.ThrowIfNull(messageFunc);

                logger.Error(exception, messageFunc());
            }
        }

        /// <summary>
        /// Writes the diagnostic message and exception at the <c>Fatal</c> level.
        /// </summary>
        /// <param name="logger">A logger implementation that will handle the message.</param>
        /// <param name="exception">An exception to be logged.</param>
        /// <param name="messageFunc">A function returning message to be written. Function is not evaluated if logging is not enabled.</param>
        public static void Fatal([NotNull] this ILogger logger, Exception exception, LogMessageGenerator messageFunc)
        {
            if (logger.IsFatalEnabled)
            {
                Guard.ThrowIfNull(messageFunc);

                logger.Fatal(exception, messageFunc());
            }
        }
    }
}
