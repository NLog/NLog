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

using System;
using System.ComponentModel;
using System.Diagnostics;
#if !NET40 && !NET35
using System.Threading.Tasks;
#endif
using JetBrains.Annotations;

namespace NLog
{
    /// <summary>
    /// Extensions for NLog <see cref="Logger"/>
    /// </summary>
    public static class LoggerExtensions
    {
        /// <summary>
        /// Starts building a log event with the specified <see cref="LogLevel"/>.
        /// </summary>
        /// <param name="logger">The logger to write the log event to.</param>
        /// <param name="logLevel">The log level. When not</param>
        /// <returns><see cref="LogEventBuilder"/> for chaining calls.</returns>
        public static LogEventBuilder ForLogEvent([NotNull] this ILog logger, LogLevel logLevel = null)
        {
            return logLevel != null ? new LogEventBuilder(logger, logLevel) : new LogEventBuilder(logger);
        }

        /// <summary>
        /// Starts building a log event at the <c>Trace</c> level.
        /// </summary>
        /// <param name="logger">The logger to write the log event to.</param>
        /// <returns><see cref="LogEventBuilder"/> for chaining calls.</returns>
        public static LogEventBuilder ForTraceEvent([NotNull] this ILog logger)
        {
            return new LogEventBuilder(logger, LogLevel.Trace);
        }

        /// <summary>
        /// Starts building a log event at the <c>Debug</c> level.
        /// </summary>
        /// <param name="logger">The logger to write the log event to.</param>
        /// <returns><see cref="LogEventBuilder"/> for chaining calls.</returns>
        public static LogEventBuilder ForDebugEvent([NotNull] this ILog logger)
        {
            return new LogEventBuilder(logger, LogLevel.Debug);
        }

        /// <summary>
        /// Starts building a log event at the <c>Info</c> level.
        /// </summary>
        /// <param name="logger">The logger to write the log event to.</param>
        /// <returns><see cref="LogEventBuilder"/> for chaining calls.</returns>
        public static LogEventBuilder ForInfoEvent([NotNull] this ILog logger)
        {
            return new LogEventBuilder(logger, LogLevel.Info);
        }

        /// <summary>
        /// Starts building a log event at the <c>Warn</c> level.
        /// </summary>
        /// <param name="logger">The logger to write the log event to.</param>
        /// <returns><see cref="LogEventBuilder"/> for chaining calls.</returns>
        public static LogEventBuilder ForWarnEvent([NotNull] this ILog logger)
        {
            return new LogEventBuilder(logger, LogLevel.Warn);
        }

        /// <summary>
        /// Starts building a log event at the <c>Error</c> level.
        /// </summary>
        /// <param name="logger">The logger to write the log event to.</param>
        /// <returns><see cref="LogEventBuilder"/> for chaining calls.</returns>
        public static LogEventBuilder ForErrorEvent([NotNull] this ILog logger)
        {
            return new LogEventBuilder(logger, LogLevel.Error);
        }

        /// <summary>
        /// Starts building a log event at the <c>Fatal</c> level.
        /// </summary>
        /// <param name="logger">The logger to write the log event to.</param>
        /// <returns><see cref="LogEventBuilder"/> for chaining calls.</returns>
        public static LogEventBuilder ForFatalEvent([NotNull] this ILog logger)
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
        public static LogEventBuilder ForExceptionEvent([NotNull] this ILog logger, Exception exception, LogLevel logLevel = null)
        {
            return ForLogEvent(logger, logLevel ?? LogLevel.Error).Exception(exception);
        }

        /// <summary>
        /// Writes the diagnostic message and exception at the specified level.
        /// </summary>
        /// <param name="logger">A logger implementation that will handle the message.</param>
        /// <param name="level">The log level.</param>
        /// <param name="exception">An exception to be logged.</param>
        /// <param name="messageFunc">A function returning message to be written. Function is not evaluated if logging is not enabled.</param>
        public static void Log([NotNull] this ILog logger, [NotNull] LogLevel level, Exception exception, LogMessageGenerator messageFunc)
        {
            if (logger.IsEnabled(level))
            {
                if (messageFunc == null)
                {
                    throw new ArgumentNullException(nameof(messageFunc));
                }

                logger.Log(level, exception, null, messageFunc(), null);
            }
        }

        /// <summary>
        /// Writes the diagnostic message and exception at the specified level.
        /// </summary>
        /// <param name="logger">A logger implementation that will handle the message.</param>
        /// <param name="level">The log level.</param>
        /// <param name="exception">An exception to be logged.</param>
        /// <param name="messageFunc">A function returning message to be written. Function is not evaluated if logging is not enabled.</param>
        public static void Log([NotNull] this Logger logger, [NotNull] LogLevel level, Exception exception, LogMessageGenerator messageFunc)
        {
            Log((ILog)logger, level, exception, messageFunc);
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Trace</c> level.
        /// </summary>
        /// <typeparam name="T">Type of the value.</typeparam>
        /// <param name="logger">A logger implementation that will handle the message.</param>
        /// <param name="value">The value to be written.</param>
        public static void Trace<T>([NotNull] this ILog logger, T value)
        {
            if (logger.IsTraceEnabled)
            {
                object objectValue = value;
                if (objectValue is Exception exception)
                    logger.Trace(exception, (string)null);
                else if (objectValue is LogEventInfo logEventInfo)
                    logger.Log(PrepareLogEvent(logger, LogLevel.Trace, logEventInfo));
                else
                    logger.Trace(null, "{0}", new object[] { objectValue });
            }
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Trace</c> level.
        /// </summary>
        /// <typeparam name="T">Type of the value.</typeparam>
        /// <param name="logger">A logger implementation that will handle the message.</param>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="value">The value to be written.</param>
        public static void Trace<T>([NotNull] this ILog logger, IFormatProvider formatProvider, T value)
        {
            if (logger.IsTraceEnabled)
            {
                object objectValue = value;
                logger.Log(LogLevel.Trace, objectValue as Exception, formatProvider, "{0}", new object[] { objectValue });
            }
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Trace</c> level.
        /// </summary>
        /// <param name="logger">A logger implementation that will handle the message.</param>
        /// <param name="message">A <see langword="string" /> to be written.</param>
        public static void Trace([NotNull] this ILog logger, [Localizable(false)] string message)
        {
            logger.Trace(null, message);
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Trace</c> level using the specified parameters.
        /// </summary>
        /// <param name="logger">A logger implementation that will handle the message.</param>
        /// <param name="message">A <see langword="string" /> containing format items.</param>
        /// <param name="args">Arguments to format.</param>
        [MessageTemplateFormatMethod("message")]
        public static void Trace([NotNull] this ILog logger, [Localizable(false)] string message, params object[] args)
        {
            logger.Trace(null, message, args);
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Trace</c> level using the specified parameter.
        /// </summary>
        /// <typeparam name="TArgument">The type of the argument.</typeparam>
        /// <param name="logger">A logger implementation that will handle the message.</param>
        /// <param name="message">A <see langword="string" /> containing format items.</param>
        /// <param name="argument">The argument to format.</param>
        [MessageTemplateFormatMethod("message")]
        public static void Trace<TArgument>([NotNull] this ILog logger, [Localizable(false)] string message, TArgument argument)
        {
            if (logger.IsTraceEnabled)
            {
                logger.Trace(null, message, new object[] { argument });
            }
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Trace</c> level using the specified parameters.
        /// </summary>
        /// <typeparam name="TArgument1">The type of the first argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the second argument.</typeparam>
        /// <param name="logger">A logger implementation that will handle the message.</param>
        /// <param name="message">A <see langword="string" /> containing format items.</param>
        /// <param name="argument1">The first argument to format.</param>
        /// <param name="argument2">The second argument to format.</param>
        [MessageTemplateFormatMethod("message")]
        public static void Trace<TArgument1, TArgument2>([NotNull] this ILog logger, [Localizable(false)] string message, TArgument1 argument1, TArgument2 argument2)
        {
            if (logger.IsTraceEnabled)
            {
                logger.Trace(null, message, new object[] { argument1, argument2 });
            }
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Trace</c> level using the specified parameters.
        /// </summary>
        /// <typeparam name="TArgument1">The type of the first argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the second argument.</typeparam>
        /// <typeparam name="TArgument3">The type of the third argument.</typeparam>
        /// <param name="logger">A logger implementation that will handle the message.</param>
        /// <param name="message">A <see langword="string" /> containing format items.</param>
        /// <param name="argument1">The first argument to format.</param>
        /// <param name="argument2">The second argument to format.</param>
        /// <param name="argument3">The third argument to format.</param>
        [MessageTemplateFormatMethod("message")]
        public static void Trace<TArgument1, TArgument2, TArgument3>([NotNull] this ILog logger, [Localizable(false)] string message, TArgument1 argument1, TArgument2 argument2, TArgument3 argument3)
        {
            if (logger.IsTraceEnabled)
            {
                logger.Trace(null, message, new object[] { argument1, argument2, argument3 });
            }
        }

        /// <summary>
        /// Writes the diagnostic message and exception at the <c>Trace</c> level.
        /// </summary>
        /// <param name="logger">A logger implementation that will handle the message.</param>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing format items.</param>
        /// <param name="args">Arguments to format.</param>
        [MessageTemplateFormatMethod("message")]
        public static void Trace([NotNull] this ILog logger, IFormatProvider formatProvider, [Localizable(false)] string message, params object[] args)
        {
            logger.Log(LogLevel.Trace, null, formatProvider, message, args);
        }

        /// <summary>
        /// Writes the diagnostic message and exception at the <c>Trace</c> level.
        /// </summary>
        /// <param name="logger">A logger implementation that will handle the message.</param>
        /// <param name="exception">An exception to be logged.</param>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing format items.</param>
        /// <param name="args">Arguments to format.</param>
        [MessageTemplateFormatMethod("message")]
        public static void Trace([NotNull] this ILog logger, Exception exception, IFormatProvider formatProvider, [Localizable(false)] string message, params object[] args)
        {
            logger.Log(LogLevel.Trace, exception, formatProvider, message, args);
        }

        /// <summary>
        /// Writes the diagnostic message and exception at the <c>Trace</c> level.
        /// </summary>
        /// <param name="logger">A logger implementation that will handle the message.</param>
        /// <param name="messageFunc">A function returning message to be written. Function is not evaluated if logging is not enabled.</param>
        public static void Trace([NotNull] this ILog logger, [NotNull] LogMessageGenerator messageFunc)
        {
            if (logger.IsTraceEnabled)
            {
                if (messageFunc == null)
                {
                    throw new ArgumentNullException(nameof(messageFunc));
                }
                logger.Trace(null, messageFunc());
            }
        }

        /// <summary>
        /// Writes the diagnostic message and exception at the <c>Trace</c> level.
        /// </summary>
        /// <param name="logger">A logger implementation that will handle the message.</param>
        /// <param name="exception">An exception to be logged.</param>
        /// <param name="messageFunc">A function returning message to be written. Function is not evaluated if logging is not enabled.</param>
        public static void Trace([NotNull] this ILog logger, Exception exception, [NotNull] LogMessageGenerator messageFunc)
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
        /// Writes the diagnostic message and exception at the <c>Trace</c> level.
        /// </summary>
        /// <param name="logger">A logger implementation that will handle the message.</param>
        /// <param name="exception">An exception to be logged.</param>
        /// <param name="messageFunc">A function returning message to be written. Function is not evaluated if logging is not enabled.</param>
        public static void Trace([NotNull] this Logger logger, Exception exception, [NotNull] LogMessageGenerator messageFunc)
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
        /// Writes the diagnostic message at the <c>Debug</c> level.
        /// </summary>
        /// <typeparam name="T">Type of the value.</typeparam>
        /// <param name="logger">A logger implementation that will handle the message.</param>
        /// <param name="value">The value to be written.</param>
        public static void Debug<T>([NotNull] this ILog logger, T value)
        {
            if (logger.IsDebugEnabled)
            {
                object objectValue = value;
                if (objectValue is Exception exception)
                    logger.Debug(exception, (string)null);
                else if (objectValue is LogEventInfo logEventInfo)
                    logger.Log(PrepareLogEvent(logger, LogLevel.Debug, logEventInfo));
                else
                    logger.Debug(null, "{0}", new object[] { objectValue });
            }
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Debug</c> level.
        /// </summary>
        /// <typeparam name="T">Type of the value.</typeparam>
        /// <param name="logger">A logger implementation that will handle the message.</param>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="value">The value to be written.</param>
        public static void Debug<T>([NotNull] this ILog logger, IFormatProvider formatProvider, T value)
        {
            if (logger.IsDebugEnabled)
            {
                object objectValue = value;
                logger.Log(LogLevel.Debug, value as Exception, formatProvider, "{0}", new object[] { objectValue });
            }
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Debug</c> level.
        /// </summary>
        /// <param name="logger">A logger implementation that will handle the message.</param>
        /// <param name="message">A <see langword="string" /> to be written.</param>
        public static void Debug([NotNull] this ILog logger, [Localizable(false)] string message)
        {
            logger.Debug(null, message);
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Debug</c> level using the specified parameters.
        /// </summary>
        /// <param name="logger">A logger implementation that will handle the message.</param>
        /// <param name="message">A <see langword="string" /> containing format items.</param>
        /// <param name="args">Arguments to format.</param>
        [MessageTemplateFormatMethod("message")]
        public static void Debug([NotNull] this ILog logger, [Localizable(false)] string message, params object[] args)
        {
            logger.Debug(null, message, args);
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Debug</c> level using the specified parameter.
        /// </summary>
        /// <typeparam name="TArgument">The type of the argument.</typeparam>
        /// <param name="logger">A logger implementation that will handle the message.</param>
        /// <param name="message">A <see langword="string" /> containing format items.</param>
        /// <param name="argument">The argument to format.</param>
        [MessageTemplateFormatMethod("message")]
        public static void Debug<TArgument>([NotNull] this ILog logger, [Localizable(false)] string message, TArgument argument)
        {
            if (logger.IsDebugEnabled)
            {
                logger.Debug(null, message, new object[] { argument });
            }
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Debug</c> level using the specified parameters.
        /// </summary>
        /// <typeparam name="TArgument1">The type of the first argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the second argument.</typeparam>
        /// <param name="logger">A logger implementation that will handle the message.</param>
        /// <param name="message">A <see langword="string" /> containing format items.</param>
        /// <param name="argument1">The first argument to format.</param>
        /// <param name="argument2">The second argument to format.</param>
        [MessageTemplateFormatMethod("message")]
        public static void Debug<TArgument1, TArgument2>([NotNull] this ILog logger, [Localizable(false)] string message, TArgument1 argument1, TArgument2 argument2)
        {
            if (logger.IsDebugEnabled)
            {
                logger.Debug(null, message, new object[] { argument1, argument2 });
            }
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Debug</c> level using the specified parameters.
        /// </summary>
        /// <typeparam name="TArgument1">The type of the first argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the second argument.</typeparam>
        /// <typeparam name="TArgument3">The type of the third argument.</typeparam>
        /// <param name="logger">A logger implementation that will handle the message.</param>
        /// <param name="message">A <see langword="string" /> containing format items.</param>
        /// <param name="argument1">The first argument to format.</param>
        /// <param name="argument2">The second argument to format.</param>
        /// <param name="argument3">The third argument to format.</param>
        [MessageTemplateFormatMethod("message")]
        public static void Debug<TArgument1, TArgument2, TArgument3>([NotNull] this ILog logger, [Localizable(false)] string message, TArgument1 argument1, TArgument2 argument2, TArgument3 argument3)
        {
            if (logger.IsDebugEnabled)
            {
                logger.Debug(null, message, new object[] { argument1, argument2, argument3 });
            }
        }

        /// <summary>
        /// Writes the diagnostic message and exception at the <c>Debug</c> level.
        /// </summary>
        /// <param name="logger">A logger implementation that will handle the message.</param>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing format items.</param>
        /// <param name="args">Arguments to format.</param>
        [MessageTemplateFormatMethod("message")]
        public static void Debug([NotNull] this ILog logger, IFormatProvider formatProvider, [Localizable(false)] string message, params object[] args)
        {
            logger.Log(LogLevel.Debug, null, formatProvider, message, args);
        }

        /// <summary>
        /// Writes the diagnostic message and exception at the <c>Debug</c> level.
        /// </summary>
        /// <param name="logger">A logger implementation that will handle the message.</param>
        /// <param name="exception">An exception to be logged.</param>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing format items.</param>
        /// <param name="args">Arguments to format.</param>
        [MessageTemplateFormatMethod("message")]
        public static void Debug([NotNull] this ILog logger, Exception exception, IFormatProvider formatProvider, [Localizable(false)] string message, params object[] args)
        {
            logger.Log(LogLevel.Debug, exception, formatProvider, message, args);
        }

        /// <summary>
        /// Writes the diagnostic message and exception at the <c>Debug</c> level.
        /// </summary>
        /// <param name="logger">A logger implementation that will handle the message.</param>
        /// <param name="messageFunc">A function returning message to be written. Function is not evaluated if logging is not enabled.</param>
        public static void Debug([NotNull] this ILog logger, [NotNull] LogMessageGenerator messageFunc)
        {
            if (logger.IsDebugEnabled)
            {
                if (messageFunc == null)
                {
                    throw new ArgumentNullException(nameof(messageFunc));
                }
                logger.Debug(null, messageFunc());
            }
        }

        /// <summary>
        /// Writes the diagnostic message and exception at the <c>Debug</c> level.
        /// </summary>
        /// <param name="logger">A logger implementation that will handle the message.</param>
        /// <param name="exception">An exception to be logged.</param>
        /// <param name="messageFunc">A function returning message to be written. Function is not evaluated if logging is not enabled.</param>
        public static void Debug([NotNull] this ILog logger, Exception exception, [NotNull] LogMessageGenerator messageFunc)
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
        /// Writes the diagnostic message and exception at the <c>Debug</c> level.
        /// </summary>
        /// <param name="logger">A logger implementation that will handle the message.</param>
        /// <param name="exception">An exception to be logged.</param>
        /// <param name="messageFunc">A function returning message to be written. Function is not evaluated if logging is not enabled.</param>
        public static void Debug([NotNull] this Logger logger, Exception exception, [NotNull] LogMessageGenerator messageFunc)
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
        /// Writes the diagnostic message at the <c>Info</c> level.
        /// </summary>
        /// <typeparam name="T">Type of the value.</typeparam>
        /// <param name="logger">A logger implementation that will handle the message.</param>
        /// <param name="value">The value to be written.</param>
        public static void Info<T>([NotNull] this ILog logger, T value)
        {
            if (logger.IsInfoEnabled)
            {
                object objectValue = value;
                if (objectValue is Exception exception)
                    logger.Info(exception, (string)null);
                else if (objectValue is LogEventInfo logEventInfo)
                    logger.Log(PrepareLogEvent(logger, LogLevel.Info, logEventInfo));
                else
                    logger.Info(null, "{0}", new object[] { objectValue });
            }
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Info</c> level.
        /// </summary>
        /// <typeparam name="T">Type of the value.</typeparam>
        /// <param name="logger">A logger implementation that will handle the message.</param>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="value">The value to be written.</param>
        public static void Info<T>([NotNull] this ILog logger, IFormatProvider formatProvider, T value)
        {
            if (logger.IsInfoEnabled)
            {
                object objectValue = value;
                logger.Log(LogLevel.Info, value as Exception, formatProvider, "{0}", new object[] { objectValue });
            }
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Info</c> level.
        /// </summary>
        /// <param name="logger">A logger implementation that will handle the message.</param>
        /// <param name="message">A <see langword="string" /> to be written.</param>
        public static void Info([NotNull] this ILog logger, [Localizable(false)] string message)
        {
            logger.Info(null, message);
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Info</c> level using the specified parameters.
        /// </summary>
        /// <param name="logger">A logger implementation that will handle the message.</param>
        /// <param name="message">A <see langword="string" /> containing format items.</param>
        /// <param name="args">Arguments to format.</param>
        [MessageTemplateFormatMethod("message")]
        public static void Info([NotNull] this ILog logger, [Localizable(false)] string message, params object[] args)
        {
            logger.Info(null, message, args);
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Info</c> level using the specified parameter.
        /// </summary>
        /// <typeparam name="TArgument">The type of the argument.</typeparam>
        /// <param name="logger">A logger implementation that will handle the message.</param>
        /// <param name="message">A <see langword="string" /> containing format items.</param>
        /// <param name="argument">The argument to format.</param>
        [MessageTemplateFormatMethod("message")]
        public static void Info<TArgument>([NotNull] this ILog logger, [Localizable(false)] string message, TArgument argument)
        {
            if (logger.IsInfoEnabled)
            {
                logger.Info(null, message, new object[] { argument });
            }
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Info</c> level using the specified parameters.
        /// </summary>
        /// <typeparam name="TArgument1">The type of the first argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the second argument.</typeparam>
        /// <param name="logger">A logger implementation that will handle the message.</param>
        /// <param name="message">A <see langword="string" /> containing format items.</param>
        /// <param name="argument1">The first argument to format.</param>
        /// <param name="argument2">The second argument to format.</param>
        [MessageTemplateFormatMethod("message")]
        public static void Info<TArgument1, TArgument2>([NotNull] this ILog logger, [Localizable(false)] string message, TArgument1 argument1, TArgument2 argument2)
        {
            if (logger.IsInfoEnabled)
            {
                logger.Info(null, message, new object[] { argument1, argument2 });
            }
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Info</c> level using the specified parameters.
        /// </summary>
        /// <typeparam name="TArgument1">The type of the first argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the second argument.</typeparam>
        /// <typeparam name="TArgument3">The type of the third argument.</typeparam>
        /// <param name="logger">A logger implementation that will handle the message.</param>
        /// <param name="message">A <see langword="string" /> containing format items.</param>
        /// <param name="argument1">The first argument to format.</param>
        /// <param name="argument2">The second argument to format.</param>
        /// <param name="argument3">The third argument to format.</param>
        [MessageTemplateFormatMethod("message")]
        public static void Info<TArgument1, TArgument2, TArgument3>([NotNull] this ILog logger, [Localizable(false)] string message, TArgument1 argument1, TArgument2 argument2, TArgument3 argument3)
        {
            if (logger.IsInfoEnabled)
            {
                logger.Info(null, message, new object[] { argument1, argument2, argument3 });
            }
        }

        /// <summary>
        /// Writes the diagnostic message and exception at the <c>Info</c> level.
        /// </summary>
        /// <param name="logger">A logger implementation that will handle the message.</param>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing format items.</param>
        /// <param name="args">Arguments to format.</param>
        [MessageTemplateFormatMethod("message")]
        public static void Info([NotNull] this ILog logger, IFormatProvider formatProvider, [Localizable(false)] string message, params object[] args)
        {
            logger.Log(LogLevel.Info, null, formatProvider, message, args);
        }

        /// <summary>
        /// Writes the diagnostic message and exception at the <c>Info</c> level.
        /// </summary>
        /// <param name="logger">A logger implementation that will handle the message.</param>
        /// <param name="exception">An exception to be logged.</param>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing format items.</param>
        /// <param name="args">Arguments to format.</param>
        [MessageTemplateFormatMethod("message")]
        public static void Info([NotNull] this ILog logger, Exception exception, IFormatProvider formatProvider, [Localizable(false)] string message, params object[] args)
        {
            logger.Log(LogLevel.Info, exception, formatProvider, message, args);
        }

        /// <summary>
        /// Writes the diagnostic message and exception at the <c>Info</c> level.
        /// </summary>
        /// <param name="logger">A logger implementation that will handle the message.</param>
        /// <param name="messageFunc">A function returning message to be written. Function is not evaluated if logging is not enabled.</param>
        public static void Info([NotNull] this ILog logger, [NotNull] LogMessageGenerator messageFunc)
        {
            if (logger.IsInfoEnabled)
            {
                if (messageFunc == null)
                {
                    throw new ArgumentNullException(nameof(messageFunc));
                }
                logger.Info(null, messageFunc());
            }
        }

        /// <summary>
        /// Writes the diagnostic message and exception at the <c>Info</c> level.
        /// </summary>
        /// <param name="logger">A logger implementation that will handle the message.</param>
        /// <param name="exception">An exception to be logged.</param>
        /// <param name="messageFunc">A function returning message to be written. Function is not evaluated if logging is not enabled.</param>
        public static void Info([NotNull] this ILog logger, Exception exception, [NotNull] LogMessageGenerator messageFunc)
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
        /// Writes the diagnostic message and exception at the <c>Info</c> level.
        /// </summary>
        /// <param name="logger">A logger implementation that will handle the message.</param>
        /// <param name="exception">An exception to be logged.</param>
        /// <param name="messageFunc">A function returning message to be written. Function is not evaluated if logging is not enabled.</param>
        public static void Info([NotNull] this Logger logger, Exception exception, [NotNull] LogMessageGenerator messageFunc)
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
        /// Writes the diagnostic message at the <c>Warn</c> level.
        /// </summary>
        /// <typeparam name="T">Type of the value.</typeparam>
        /// <param name="logger">A logger implementation that will handle the message.</param>
        /// <param name="value">The value to be written.</param>
        public static void Warn<T>([NotNull] this ILog logger, T value)
        {
            if (logger.IsWarnEnabled)
            {
                object objectValue = value;
                if (objectValue is Exception exception)
                    logger.Warn(exception, (string)null);
                else if (objectValue is LogEventInfo logEventInfo)
                    logger.Log(PrepareLogEvent(logger, LogLevel.Warn, logEventInfo));
                else
                    logger.Warn(null, "{0}", new object[] { objectValue });
            }
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Warn</c> level.
        /// </summary>
        /// <typeparam name="T">Type of the value.</typeparam>
        /// <param name="logger">A logger implementation that will handle the message.</param>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="value">The value to be written.</param>
        public static void Warn<T>([NotNull] this ILog logger, IFormatProvider formatProvider, T value)
        {
            if (logger.IsWarnEnabled)
            {
                object objectValue = value;
                logger.Log(LogLevel.Warn, value as Exception, formatProvider, "{0}", new object[] { objectValue });
            }
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Warn</c> level.
        /// </summary>
        /// <param name="logger">A logger implementation that will handle the message.</param>
        /// <param name="message">A <see langword="string" /> to be written.</param>
        public static void Warn([NotNull] this ILog logger, [Localizable(false)] string message)
        {
            logger.Warn(null, message);
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Warn</c> level using the specified parameters.
        /// </summary>
        /// <param name="logger">A logger implementation that will handle the message.</param>
        /// <param name="message">A <see langword="string" /> containing format items.</param>
        /// <param name="args">Arguments to format.</param>
        [MessageTemplateFormatMethod("message")]
        public static void Warn([NotNull] this ILog logger, [Localizable(false)] string message, params object[] args)
        {
            logger.Warn(null, message, args);
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Warn</c> level using the specified parameter.
        /// </summary>
        /// <typeparam name="TArgument">The type of the argument.</typeparam>
        /// <param name="logger">A logger implementation that will handle the message.</param>
        /// <param name="message">A <see langword="string" /> containing format items.</param>
        /// <param name="argument">The argument to format.</param>
        [MessageTemplateFormatMethod("message")]
        public static void Warn<TArgument>([NotNull] this ILog logger, [Localizable(false)] string message, TArgument argument)
        {
            if (logger.IsWarnEnabled)
            {
                logger.Warn(null, message, new object[] { argument });
            }
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Warn</c> level using the specified parameters.
        /// </summary>
        /// <typeparam name="TArgument1">The type of the first argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the second argument.</typeparam>
        /// <param name="logger">A logger implementation that will handle the message.</param>
        /// <param name="message">A <see langword="string" /> containing format items.</param>
        /// <param name="argument1">The first argument to format.</param>
        /// <param name="argument2">The second argument to format.</param>
        [MessageTemplateFormatMethod("message")]
        public static void Warn<TArgument1, TArgument2>([NotNull] this ILog logger, [Localizable(false)] string message, TArgument1 argument1, TArgument2 argument2)
        {
            if (logger.IsWarnEnabled)
            {
                logger.Warn(null, message, new object[] { argument1, argument2 });
            }
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Warn</c> level using the specified parameters.
        /// </summary>
        /// <typeparam name="TArgument1">The type of the first argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the second argument.</typeparam>
        /// <typeparam name="TArgument3">The type of the third argument.</typeparam>
        /// <param name="logger">A logger implementation that will handle the message.</param>
        /// <param name="message">A <see langword="string" /> containing format items.</param>
        /// <param name="argument1">The first argument to format.</param>
        /// <param name="argument2">The second argument to format.</param>
        /// <param name="argument3">The third argument to format.</param>
        [MessageTemplateFormatMethod("message")]
        public static void Warn<TArgument1, TArgument2, TArgument3>([NotNull] this ILog logger, [Localizable(false)] string message, TArgument1 argument1, TArgument2 argument2, TArgument3 argument3)
        {
            if (logger.IsWarnEnabled)
            {
                logger.Warn(null, message, new object[] { argument1, argument2, argument3 });
            }
        }

        /// <summary>
        /// Writes the diagnostic message and exception at the <c>Warn</c> level.
        /// </summary>
        /// <param name="logger">A logger implementation that will handle the message.</param>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing format items.</param>
        /// <param name="args">Arguments to format.</param>
        [MessageTemplateFormatMethod("message")]
        public static void Warn([NotNull] this ILog logger, IFormatProvider formatProvider, [Localizable(false)] string message, params object[] args)
        {
            logger.Log(LogLevel.Warn, null, formatProvider, message, args);
        }

        /// <summary>
        /// Writes the diagnostic message and exception at the <c>Warn</c> level.
        /// </summary>
        /// <param name="logger">A logger implementation that will handle the message.</param>
        /// <param name="exception">An exception to be logged.</param>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing format items.</param>
        /// <param name="args">Arguments to format.</param>
        [MessageTemplateFormatMethod("message")]
        public static void Warn([NotNull] this ILog logger, Exception exception, IFormatProvider formatProvider, [Localizable(false)] string message, params object[] args)
        {
            logger.Log(LogLevel.Warn, exception, formatProvider, message, args);
        }

        /// <summary>
        /// Writes the diagnostic message and exception at the <c>Warn</c> level.
        /// </summary>
        /// <param name="logger">A logger implementation that will handle the message.</param>
        /// <param name="messageFunc">A function returning message to be written. Function is not evaluated if logging is not enabled.</param>
        public static void Warn([NotNull] this ILog logger, [NotNull] LogMessageGenerator messageFunc)
        {
            if (logger.IsWarnEnabled)
            {
                if (messageFunc == null)
                {
                    throw new ArgumentNullException(nameof(messageFunc));
                }
                logger.Warn(null, messageFunc());
            }
        }

        /// <summary>
        /// Writes the diagnostic message and exception at the <c>Warn</c> level.
        /// </summary>
        /// <param name="logger">A logger implementation that will handle the message.</param>
        /// <param name="exception">An exception to be logged.</param>
        /// <param name="messageFunc">A function returning message to be written. Function is not evaluated if logging is not enabled.</param>
        public static void Warn([NotNull] this ILog logger, Exception exception, [NotNull] LogMessageGenerator messageFunc)
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
        /// Writes the diagnostic message and exception at the <c>Warn</c> level.
        /// </summary>
        /// <param name="logger">A logger implementation that will handle the message.</param>
        /// <param name="exception">An exception to be logged.</param>
        /// <param name="messageFunc">A function returning message to be written. Function is not evaluated if logging is not enabled.</param>
        public static void Warn([NotNull] this Logger logger, Exception exception, [NotNull] LogMessageGenerator messageFunc)
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
        /// Writes the diagnostic message at the <c>Error</c> level.
        /// </summary>
        /// <typeparam name="T">Type of the value.</typeparam>
        /// <param name="logger">A logger implementation that will handle the message.</param>
        /// <param name="value">The value to be written.</param>
        public static void Error<T>([NotNull] this ILog logger, T value)
        {
            if (logger.IsErrorEnabled)
            {
                object objectValue = value;
                if (objectValue is Exception exception)
                    logger.Error(exception, (string)null);
                else if (objectValue is LogEventInfo logEventInfo)
                    logger.Log(PrepareLogEvent(logger, LogLevel.Error, logEventInfo));
                else
                    logger.Error(null, "{0}", new object[] { objectValue });
            }
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Error</c> level.
        /// </summary>
        /// <typeparam name="T">Type of the value.</typeparam>
        /// <param name="logger">A logger implementation that will handle the message.</param>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="value">The value to be written.</param>
        public static void Error<T>([NotNull] this ILog logger, IFormatProvider formatProvider, T value)
        {
            if (logger.IsErrorEnabled)
            {
                object objectValue = value;
                logger.Log(LogLevel.Error, value as Exception, formatProvider, "{0}", new object[] { objectValue });
            }
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Error</c> level.
        /// </summary>
        /// <param name="logger">A logger implementation that will handle the message.</param>
        /// <param name="message">A <see langword="string" /> to be written.</param>
        public static void Error([NotNull] this ILog logger, [Localizable(false)] string message)
        {
            logger.Error(null, message);
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Error</c> level using the specified parameters.
        /// </summary>
        /// <param name="logger">A logger implementation that will handle the message.</param>
        /// <param name="message">A <see langword="string" /> containing format items.</param>
        /// <param name="args">Arguments to format.</param>
        [MessageTemplateFormatMethod("message")]
        public static void Error([NotNull] this ILog logger, [Localizable(false)] string message, params object[] args)
        {
            logger.Error(null, message, args);
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Error</c> level using the specified parameter.
        /// </summary>
        /// <typeparam name="TArgument">The type of the argument.</typeparam>
        /// <param name="logger">A logger implementation that will handle the message.</param>
        /// <param name="message">A <see langword="string" /> containing format items.</param>
        /// <param name="argument">The argument to format.</param>
        [MessageTemplateFormatMethod("message")]
        public static void Error<TArgument>([NotNull] this ILog logger, [Localizable(false)] string message, TArgument argument)
        {
            if (logger.IsErrorEnabled)
            {
                logger.Error(null, message, new object[] { argument });
            }
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Error</c> level using the specified parameters.
        /// </summary>
        /// <typeparam name="TArgument1">The type of the first argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the second argument.</typeparam>
        /// <param name="logger">A logger implementation that will handle the message.</param>
        /// <param name="message">A <see langword="string" /> containing format items.</param>
        /// <param name="argument1">The first argument to format.</param>
        /// <param name="argument2">The second argument to format.</param>
        [MessageTemplateFormatMethod("message")]
        public static void Error<TArgument1, TArgument2>([NotNull] this ILog logger, [Localizable(false)] string message, TArgument1 argument1, TArgument2 argument2)
        {
            if (logger.IsErrorEnabled)
            {
                logger.Error(null, message, new object[] { argument1, argument2 });
            }
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Error</c> level using the specified parameters.
        /// </summary>
        /// <typeparam name="TArgument1">The type of the first argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the second argument.</typeparam>
        /// <typeparam name="TArgument3">The type of the third argument.</typeparam>
        /// <param name="logger">A logger implementation that will handle the message.</param>
        /// <param name="message">A <see langword="string" /> containing format items.</param>
        /// <param name="argument1">The first argument to format.</param>
        /// <param name="argument2">The second argument to format.</param>
        /// <param name="argument3">The third argument to format.</param>
        [MessageTemplateFormatMethod("message")]
        public static void Error<TArgument1, TArgument2, TArgument3>([NotNull] this ILog logger, [Localizable(false)] string message, TArgument1 argument1, TArgument2 argument2, TArgument3 argument3)
        {
            if (logger.IsErrorEnabled)
            {
                logger.Error(null, message, new object[] { argument1, argument2, argument3 });
            }
        }

        /// <summary>
        /// Writes the diagnostic message and exception at the <c>Error</c> level.
        /// </summary>
        /// <param name="logger">A logger implementation that will handle the message.</param>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing format items.</param>
        /// <param name="args">Arguments to format.</param>
        [MessageTemplateFormatMethod("message")]
        public static void Error([NotNull] this ILog logger, IFormatProvider formatProvider, [Localizable(false)] string message, params object[] args)
        {
            logger.Log(LogLevel.Error, null, formatProvider, message, args);
        }

        /// <summary>
        /// Writes the diagnostic message and exception at the <c>Error</c> level.
        /// </summary>
        /// <param name="logger">A logger implementation that will handle the message.</param>
        /// <param name="exception">An exception to be logged.</param>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing format items.</param>
        /// <param name="args">Arguments to format.</param>
        [MessageTemplateFormatMethod("message")]
        public static void Error([NotNull] this ILog logger, Exception exception, IFormatProvider formatProvider, [Localizable(false)] string message, params object[] args)
        {
            logger.Log(LogLevel.Error, exception, formatProvider, message, args);
        }

        /// <summary>
        /// Writes the diagnostic message and exception at the <c>Error</c> level.
        /// </summary>
        /// <param name="logger">A logger implementation that will handle the message.</param>
        /// <param name="messageFunc">A function returning message to be written. Function is not evaluated if logging is not enabled.</param>
        public static void Error([NotNull] this ILog logger, [NotNull] LogMessageGenerator messageFunc)
        {
            if (logger.IsErrorEnabled)
            {
                if (messageFunc == null)
                {
                    throw new ArgumentNullException(nameof(messageFunc));
                }
                logger.Error(null, messageFunc());
            }
        }

        /// <summary>
        /// Writes the diagnostic message and exception at the <c>Error</c> level.
        /// </summary>
        /// <param name="logger">A logger implementation that will handle the message.</param>
        /// <param name="exception">An exception to be logged.</param>
        /// <param name="messageFunc">A function returning message to be written. Function is not evaluated if logging is not enabled.</param>
        public static void Error([NotNull] this ILog logger, Exception exception, [NotNull] LogMessageGenerator messageFunc)
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
        /// Writes the diagnostic message and exception at the <c>Error</c> level.
        /// </summary>
        /// <param name="logger">A logger implementation that will handle the message.</param>
        /// <param name="exception">An exception to be logged.</param>
        /// <param name="messageFunc">A function returning message to be written. Function is not evaluated if logging is not enabled.</param>
        public static void Error([NotNull] this Logger logger, Exception exception, [NotNull] LogMessageGenerator messageFunc)
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
        /// Writes the diagnostic message at the <c>Fatal</c> level.
        /// </summary>
        /// <typeparam name="T">Type of the value.</typeparam>
        /// <param name="logger">A logger implementation that will handle the message.</param>
        /// <param name="value">The value to be written.</param>
        public static void Fatal<T>([NotNull] this ILog logger, T value)
        {
            if (logger.IsFatalEnabled)
            {
                object objectValue = value;
                if (objectValue is Exception exception)
                    logger.Fatal(exception, (string)null);
                else if (objectValue is LogEventInfo logEventInfo)
                    logger.Log(PrepareLogEvent(logger, LogLevel.Fatal, logEventInfo));
                else
                    logger.Fatal(null, "{0}", new object[] { objectValue });
            }
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Fatal</c> level.
        /// </summary>
        /// <typeparam name="T">Type of the value.</typeparam>
        /// <param name="logger">A logger implementation that will handle the message.</param>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="value">The value to be written.</param>
        public static void Fatal<T>([NotNull] this ILog logger, IFormatProvider formatProvider, T value)
        {
            if (logger.IsFatalEnabled)
            {
                object objectValue = value;
                logger.Log(LogLevel.Fatal, value as Exception, formatProvider, "{0}", new object[] { objectValue });
            }
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Fatal</c> level.
        /// </summary>
        /// <param name="logger">A logger implementation that will handle the message.</param>
        /// <param name="message">A <see langword="string" /> to be written.</param>
        public static void Fatal([NotNull] this ILog logger, [Localizable(false)] string message)
        {
            logger.Fatal(null, message);
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Fatal</c> level using the specified parameters.
        /// </summary>
        /// <param name="logger">A logger implementation that will handle the message.</param>
        /// <param name="message">A <see langword="string" /> containing format items.</param>
        /// <param name="args">Arguments to format.</param>
        [MessageTemplateFormatMethod("message")]
        public static void Fatal([NotNull] this ILog logger, [Localizable(false)] string message, params object[] args)
        {
            logger.Fatal(null, message, args);
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Fatal</c> level using the specified parameter.
        /// </summary>
        /// <typeparam name="TArgument">The type of the argument.</typeparam>
        /// <param name="logger">A logger implementation that will handle the message.</param>
        /// <param name="message">A <see langword="string" /> containing format items.</param>
        /// <param name="argument">The argument to format.</param>
        [MessageTemplateFormatMethod("message")]
        public static void Fatal<TArgument>([NotNull] this ILog logger, [Localizable(false)] string message, TArgument argument)
        {
            if (logger.IsFatalEnabled)
            {
                logger.Fatal(null, message, new object[] { argument });
            }
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Fatal</c> level using the specified parameters.
        /// </summary>
        /// <typeparam name="TArgument1">The type of the first argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the second argument.</typeparam>
        /// <param name="logger">A logger implementation that will handle the message.</param>
        /// <param name="message">A <see langword="string" /> containing format items.</param>
        /// <param name="argument1">The first argument to format.</param>
        /// <param name="argument2">The second argument to format.</param>
        [MessageTemplateFormatMethod("message")]
        public static void Fatal<TArgument1, TArgument2>([NotNull] this ILog logger, [Localizable(false)] string message, TArgument1 argument1, TArgument2 argument2)
        {
            if (logger.IsFatalEnabled)
            {
                logger.Fatal(null, message, new object[] { argument1, argument2 });
            }
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Fatal</c> level using the specified parameters.
        /// </summary>
        /// <typeparam name="TArgument1">The type of the first argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the second argument.</typeparam>
        /// <typeparam name="TArgument3">The type of the third argument.</typeparam>
        /// <param name="logger">A logger implementation that will handle the message.</param>
        /// <param name="message">A <see langword="string" /> containing format items.</param>
        /// <param name="argument1">The first argument to format.</param>
        /// <param name="argument2">The second argument to format.</param>
        /// <param name="argument3">The third argument to format.</param>
        [MessageTemplateFormatMethod("message")]
        public static void Fatal<TArgument1, TArgument2, TArgument3>([NotNull] this ILog logger, [Localizable(false)] string message, TArgument1 argument1, TArgument2 argument2, TArgument3 argument3)
        {
            if (logger.IsFatalEnabled)
            {
                logger.Fatal(null, message, new object[] { argument1, argument2, argument3 });
            }
        }

        /// <summary>
        /// Writes the diagnostic message and exception at the <c>Fatal</c> level.
        /// </summary>
        /// <param name="logger">A logger implementation that will handle the message.</param>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing format items.</param>
        /// <param name="args">Arguments to format.</param>
        [MessageTemplateFormatMethod("message")]
        public static void Fatal([NotNull] this ILog logger, IFormatProvider formatProvider, [Localizable(false)] string message, params object[] args)
        {
            logger.Log(LogLevel.Fatal, null, formatProvider, message, args);
        }

        /// <summary>
        /// Writes the diagnostic message and exception at the <c>Fatal</c> level.
        /// </summary>
        /// <param name="logger">A logger implementation that will handle the message.</param>
        /// <param name="exception">An exception to be logged.</param>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing format items.</param>
        /// <param name="args">Arguments to format.</param>
        [MessageTemplateFormatMethod("message")]
        public static void Fatal([NotNull] this ILog logger, Exception exception, IFormatProvider formatProvider, [Localizable(false)] string message, params object[] args)
        {
            logger.Log(LogLevel.Fatal, exception, formatProvider, message, args);
        }

        /// <summary>
        /// Writes the diagnostic message and exception at the <c>Fatal</c> level.
        /// </summary>
        /// <param name="logger">A logger implementation that will handle the message.</param>
        /// <param name="messageFunc">A function returning message to be written. Function is not evaluated if logging is not enabled.</param>
        public static void Fatal([NotNull] this ILog logger, [NotNull] LogMessageGenerator messageFunc)
        {
            if (logger.IsFatalEnabled)
            {
                if (messageFunc == null)
                {
                    throw new ArgumentNullException(nameof(messageFunc));
                }
                logger.Fatal(null, messageFunc());
            }
        }

        /// <summary>
        /// Writes the diagnostic message and exception at the <c>Fatal</c> level.
        /// </summary>
        /// <param name="logger">A logger implementation that will handle the message.</param>
        /// <param name="exception">An exception to be logged.</param>
        /// <param name="messageFunc">A function returning message to be written. Function is not evaluated if logging is not enabled.</param>
        public static void Fatal([NotNull] this ILog logger, Exception exception, [NotNull] LogMessageGenerator messageFunc)
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

        /// <summary>
        /// Writes the diagnostic message and exception at the <c>Fatal</c> level.
        /// </summary>
        /// <param name="logger">A logger implementation that will handle the message.</param>
        /// <param name="exception">An exception to be logged.</param>
        /// <param name="messageFunc">A function returning message to be written. Function is not evaluated if logging is not enabled.</param>
        public static void Fatal([NotNull] this Logger logger, Exception exception, [NotNull] LogMessageGenerator messageFunc)
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

        private static LogEventInfo PrepareLogEvent(ILog logger, LogLevel logLevel, LogEventInfo logEventInfo)
        {
            logEventInfo.LoggerName = logger.Name;
            logEventInfo.Level = logLevel;
            return logEventInfo;
        }

        /// <summary>
        /// Runs the provided action. If the action throws, the exception is logged at <c>Error</c> level. The exception is not propagated outside of this method.
        /// </summary>
        /// <param name="logger">A logger implementation that will handle the message.</param>
        /// <param name="action">Action to execute.</param>
        public static void Swallow([NotNull] this ILog logger, Action action)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                logger.Error(e);
            }
        }

        /// <summary>
        /// Runs the provided function and returns its result. If an exception is thrown, it is logged at <c>Error</c> level.
        /// The exception is not propagated outside of this method; a fallback value is returned instead.
        /// </summary>
        /// <typeparam name="T">Return type of the provided function.</typeparam>
        /// <param name="logger">A logger implementation that will handle the message.</param>
        /// <param name="func">Function to run.</param>
        /// <param name="fallback">Fallback value to return in case of exception.</param>
        /// <returns>Result returned by the provided function or fallback value in case of exception.</returns>
        public static T Swallow<T>([NotNull] this ILog logger, Func<T> func, T fallback = default(T))
        {
            try
            {
                return func();
            }
            catch (Exception e)
            {
                logger.Error(e);
                return fallback;
            }
        }

#if !NET40 && !NET35
        /// <summary>
        /// Returns a task that completes when a specified task to completes. If the task does not run to completion, an exception is logged at <c>Error</c> level. The returned task always runs to completion.
        /// </summary>
        /// <param name="logger">A logger implementation that will handle the message.</param>
        /// <param name="task">The task for which to log an error if it does not run to completion.</param>
        /// <returns>A task that completes in the <see cref="TaskStatus.RanToCompletion"/> state when <paramref name="task"/> completes.</returns>
        public static async Task SwallowAsync([NotNull] this ILog logger, [NotNull] Task task)
        {
            try
            {
                await task.ConfigureAwait(false);
            }
            catch (Exception e)
            {
                logger.Error(e);
            }
        }

        /// <summary>
        /// Runs async action. If the action throws, the exception is logged at <c>Error</c> level. The exception is not propagated outside of this method.
        /// </summary>
        /// <param name="logger">A logger implementation that will handle the message.</param>
        /// <param name="asyncAction">Async action to execute.</param>
        public static async Task SwallowAsync([NotNull] this ILog logger, [NotNull] Func<Task> asyncAction)
        {
            try
            {
                await asyncAction().ConfigureAwait(false);
            }
            catch (Exception e)
            {
                logger.Error(e);
            }
        }

        /// <summary>
        /// Runs the provided async function and returns its result. If the task does not run to completion, an exception is logged at <c>Error</c> level.
        /// The exception is not propagated outside of this method; a fallback value is returned instead.
        /// </summary>
        /// <typeparam name="TResult">Return type of the provided function.</typeparam>
        /// <param name="logger">A logger implementation that will handle the message.</param>
        /// <param name="asyncFunc">Async function to run.</param>
        /// <param name="fallback">Fallback value to return if the task does not end in the <see cref="TaskStatus.RanToCompletion"/> state.</param>
        /// <returns>A task that represents the completion of the supplied task. If the supplied task ends in the <see cref="TaskStatus.RanToCompletion"/> state, the result of the new task will be the result of the supplied task; otherwise, the result of the new task will be the fallback value.</returns>
        public static async Task<TResult> SwallowAsync<TResult>([NotNull] this ILog logger, [NotNull] Func<Task<TResult>> asyncFunc, TResult fallback = default(TResult))
        {
            try
            {
                return await asyncFunc().ConfigureAwait(false);
            }
            catch (Exception e)
            {
                logger.Error(e);
                return fallback;
            }
        }
#endif

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
        public static void ConditionalDebug<T>([NotNull] this ILog logger, T value)
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
        public static void ConditionalDebug<T>([NotNull] this ILog logger, IFormatProvider formatProvider, T value)
        {
            logger.Debug(formatProvider, value);
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Debug</c> level.
        /// Only executed when the DEBUG conditional compilation symbol is set.</summary>
        /// <param name="logger">A logger implementation that will handle the message.</param>
        /// <param name="messageFunc">A function returning message to be written. Function is not evaluated if logging is not enabled.</param>
        [Conditional("DEBUG")]
        public static void ConditionalDebug([NotNull] this ILog logger, LogMessageGenerator messageFunc)
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
        public static void ConditionalDebug([NotNull] this ILog logger, Exception exception, string message, params object[] args)
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
        public static void ConditionalDebug([NotNull] this ILog logger, Exception exception, IFormatProvider formatProvider, string message, params object[] args)
        {
            logger.Log(LogLevel.Debug, exception, formatProvider, message, args);
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Debug</c> level.
        /// Only executed when the DEBUG conditional compilation symbol is set.</summary>
        /// <param name="logger">A logger implementation that will handle the message.</param>
        /// <param name="message">Log message.</param>
        [Conditional("DEBUG")]
        public static void ConditionalDebug([NotNull] this ILog logger, string message)
        {
            logger.Debug(null, message);
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Debug</c> level using the specified parameters.
        /// Only executed when the DEBUG conditional compilation symbol is set.</summary>
        /// <param name="logger">A logger implementation that will handle the message.</param>
        /// <param name="message">A <see langword="string" /> containing format items.</param>
        /// <param name="args">Arguments to format.</param>
        [Conditional("DEBUG")]
        [MessageTemplateFormatMethod("message")]
        public static void ConditionalDebug([NotNull] this ILog logger, string message, params object[] args)
        {
            logger.Debug(null, message, args);
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
        public static void ConditionalDebug([NotNull] this ILog logger, IFormatProvider formatProvider, string message, params object[] args)
        {
            logger.Log(LogLevel.Debug, null, formatProvider, message, args);
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
        public static void ConditionalDebug<TArgument>([NotNull] this ILog logger, string message, TArgument argument)
        {
            if (logger.IsDebugEnabled)
            {
                logger.Debug(null, message, new object[] { argument });
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
        public static void ConditionalDebug<TArgument1, TArgument2>([NotNull] this ILog logger, string message, TArgument1 argument1, TArgument2 argument2)
        {
            if (logger.IsDebugEnabled)
            {
                logger.Debug(null, message, new object[] { argument1, argument2 });
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
        public static void ConditionalDebug<TArgument1, TArgument2, TArgument3>([NotNull] this ILog logger, string message, TArgument1 argument1, TArgument2 argument2, TArgument3 argument3)
        {
            if (logger.IsDebugEnabled)
            {
                logger.Debug(null, message, new object[] { argument1, argument2, argument3 });
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
        public static void ConditionalTrace<T>([NotNull] this ILog logger, T value)
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
        public static void ConditionalTrace<T>([NotNull] this ILog logger, IFormatProvider formatProvider, T value)
        {
            logger.Trace(formatProvider, value);
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Debug</c> level.
        /// Only executed when the DEBUG conditional compilation symbol is set.</summary>
        /// <param name="logger">A logger implementation that will handle the message.</param>
        /// <param name="messageFunc">A function returning message to be written. Function is not evaluated if logging is not enabled.</param>
        [Conditional("DEBUG")]
        public static void ConditionalTrace([NotNull] this ILog logger, LogMessageGenerator messageFunc)
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
        public static void ConditionalTrace([NotNull] this ILog logger, Exception exception, string message, params object[] args)
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
        public static void ConditionalTrace([NotNull] this ILog logger, Exception exception, IFormatProvider formatProvider, string message, params object[] args)
        {
            logger.Log(LogLevel.Trace, exception, formatProvider, message, args);
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Debug</c> level.
        /// Only executed when the DEBUG conditional compilation symbol is set.</summary>
        /// <param name="logger">A logger implementation that will handle the message.</param>
        /// <param name="message">Log message.</param>
        [Conditional("DEBUG")]
        public static void ConditionalTrace([NotNull] this ILog logger, string message)
        {
            logger.Trace(null, message);
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Debug</c> level using the specified parameters.
        /// Only executed when the DEBUG conditional compilation symbol is set.</summary>
        /// <param name="logger">A logger implementation that will handle the message.</param>
        /// <param name="message">A <see langword="string" /> containing format items.</param>
        /// <param name="args">Arguments to format.</param>
        [Conditional("DEBUG")]
        [MessageTemplateFormatMethod("message")]
        public static void ConditionalTrace([NotNull] this ILog logger, string message, params object[] args)
        {
            logger.Trace(null, message, args);
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
        public static void ConditionalTrace([NotNull] this ILog logger, IFormatProvider formatProvider, string message, params object[] args)
        {
            logger.Log(LogLevel.Trace, null, formatProvider, message, args);
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
        public static void ConditionalTrace<TArgument>([NotNull] this ILog logger, string message, TArgument argument)
        {
            if (logger.IsTraceEnabled)
            {
                logger.Trace(null, message, new object[] { argument });
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
        public static void ConditionalTrace<TArgument1, TArgument2>([NotNull] this ILog logger, string message, TArgument1 argument1, TArgument2 argument2)
        {
            if (logger.IsTraceEnabled)
            {
                logger.Trace(null, message, new object[] { argument1, argument2 });
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
        public static void ConditionalTrace<TArgument1, TArgument2, TArgument3>([NotNull] this ILog logger, string message, TArgument1 argument1, TArgument2 argument2, TArgument3 argument3)
        {
            if (logger.IsTraceEnabled)
            {
                logger.Trace(null, message, new object[] { argument1, argument2, argument3 });
            }
        }

        #endregion
    }
}
