//
// Copyright (c) 2004-2024 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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
    using JetBrains.Annotations;
    using NLog.Internal;

    /// <summary>
    /// Provides logging interface and utility functions.
    /// </summary>
    public partial class Logger
    {
        /// <summary>
        /// Gets a value indicating whether logging is enabled for the <c>Trace</c> level.
        /// </summary>
        /// <returns>A value of <see langword="true" /> if logging is enabled for the <c>Trace</c> level, otherwise it returns <see langword="false" />.</returns>
        public bool IsTraceEnabled
        {
            get { return _contextLogger._isTraceEnabled; }
        }

        /// <summary>
        /// Gets a value indicating whether logging is enabled for the <c>Debug</c> level.
        /// </summary>
        /// <returns>A value of <see langword="true" /> if logging is enabled for the <c>Debug</c> level, otherwise it returns <see langword="false" />.</returns>
        public bool IsDebugEnabled
        {
            get { return _contextLogger._isDebugEnabled; }
        }

        /// <summary>
        /// Gets a value indicating whether logging is enabled for the <c>Info</c> level.
        /// </summary>
        /// <returns>A value of <see langword="true" /> if logging is enabled for the <c>Info</c> level, otherwise it returns <see langword="false" />.</returns>
        public bool IsInfoEnabled
        {
            get { return _contextLogger._isInfoEnabled; }
        }

        /// <summary>
        /// Gets a value indicating whether logging is enabled for the <c>Warn</c> level.
        /// </summary>
        /// <returns>A value of <see langword="true" /> if logging is enabled for the <c>Warn</c> level, otherwise it returns <see langword="false" />.</returns>
        public bool IsWarnEnabled
        {
            get { return _contextLogger._isWarnEnabled; }
        }

        /// <summary>
        /// Gets a value indicating whether logging is enabled for the <c>Error</c> level.
        /// </summary>
        /// <returns>A value of <see langword="true" /> if logging is enabled for the <c>Error</c> level, otherwise it returns <see langword="false" />.</returns>
        public bool IsErrorEnabled
        {
            get { return _contextLogger._isErrorEnabled; }
        }

        /// <summary>
        /// Gets a value indicating whether logging is enabled for the <c>Fatal</c> level.
        /// </summary>
        /// <returns>A value of <see langword="true" /> if logging is enabled for the <c>Fatal</c> level, otherwise it returns <see langword="false" />.</returns>
        public bool IsFatalEnabled
        {
            get { return _contextLogger._isFatalEnabled; }
        }


        #region Trace() overloads

        /// <overloads>
        /// Writes the diagnostic message at the <c>Trace</c> level using the specified format provider and format parameters.
        /// </overloads>
        /// <summary>
        /// Writes the diagnostic message at the <c>Trace</c> level.
        /// </summary>
        /// <typeparam name="T">Type of the value.</typeparam>
        /// <param name="value">The value to be written.</param>
        public void Trace<T>(T? value)
        {
            if (IsTraceEnabled)
            {
                WriteToTargets(LogLevel.Trace, Factory.DefaultCultureInfo, value);
            }
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Trace</c> level.
        /// </summary>
        /// <typeparam name="T">Type of the value.</typeparam>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="value">The value to be written.</param>
        public void Trace<T>(IFormatProvider? formatProvider, T? value)
        {
            if (IsTraceEnabled)
            {
                WriteToTargets(LogLevel.Trace, formatProvider, value);
            }
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Trace</c> level.
        /// </summary>
        /// <param name="messageFunc">A function returning message to be written. Function is not evaluated if logging is not enabled.</param>
        public void Trace(LogMessageGenerator messageFunc)
        {
            if (IsTraceEnabled)
            {
                Guard.ThrowIfNull(messageFunc);
                WriteToTargets(LogLevel.Trace, messageFunc());
            }
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Trace</c> level using the specified parameters and formatting them with the supplied format provider.
        /// </summary>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing format items.</param>
        /// <param name="args">Arguments to format.</param>
        [MessageTemplateFormatMethod("message")]
        public void Trace(IFormatProvider? formatProvider, [Localizable(false)][StructuredMessageTemplate] string message, params object?[] args)
        {
            if (IsTraceEnabled)
            {
                WriteToTargets(LogLevel.Trace, formatProvider, message, args);
            }
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Trace</c> level.
        /// </summary>
        /// <param name="message">Log message.</param>
        public void Trace([Localizable(false)] string message)
        {
            if (IsTraceEnabled)
            {
                WriteToTargets(LogLevel.Trace, message);
            }
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Trace</c> level using the specified parameters.
        /// </summary>
        /// <param name="message">A <see langword="string" /> containing format items.</param>
        /// <param name="args">Arguments to format.</param>
        [MessageTemplateFormatMethod("message")]
        public void Trace([Localizable(false)][StructuredMessageTemplate] string message, params object?[] args)
        {
            if (IsTraceEnabled)
            {
                WriteToTargets(LogLevel.Trace, Factory.DefaultCultureInfo, message, args);
            }
        }

#if NETSTANDARD2_1_OR_GREATER || NET9_0_OR_GREATER
        /// <summary>
        /// Writes the diagnostic message at the <c>Trace</c> level using the specified parameters.
        /// </summary>
        /// <param name="message">A <see langword="string" /> containing format items.</param>
        /// <param name="args">Arguments to format.</param>
        [MessageTemplateFormatMethod("message")]
        public void Trace([Localizable(false)][StructuredMessageTemplate] string message, params ReadOnlySpan<object?> args)
        {
            if (IsTraceEnabled)
            {
                WriteToTargetsWithSpan(LogLevel.Trace, null, Factory.DefaultCultureInfo, message, args);
            }
        }

        /// <summary>
        /// Writes the diagnostic message and exception at the <c>Trace</c> level.
        /// </summary>
        /// <param name="message">A <see langword="string" /> to be written.</param>
        /// <param name="exception">An exception to be logged.</param>
        /// <param name="args">Arguments to format.</param>
        [MessageTemplateFormatMethod("message")]
        public void Trace(Exception? exception, [Localizable(false)][StructuredMessageTemplate] string message, params ReadOnlySpan<object?> args)
        {
            if (IsTraceEnabled)
            {
                WriteToTargetsWithSpan(LogLevel.Trace, exception, Factory.DefaultCultureInfo, message, args);
            }
        }
#endif

        /// <summary>
        /// Writes the diagnostic message and exception at the <c>Trace</c> level.
        /// </summary>
        /// <param name="message">A <see langword="string" /> to be written.</param>
        /// <param name="exception">An exception to be logged.</param>
        public void Trace(Exception? exception, [Localizable(false)] string message)
        {
            if (IsTraceEnabled)
            {
                WriteToTargets(LogLevel.Trace, exception, Factory.DefaultCultureInfo, message, null);
            }
        }

        /// <summary>
        /// Writes the diagnostic message and exception at the <c>Trace</c> level.
        /// </summary>
        /// <param name="message">A <see langword="string" /> to be written.</param>
        /// <param name="exception">An exception to be logged.</param>
        /// <param name="args">Arguments to format.</param>
        [MessageTemplateFormatMethod("message")]
        public void Trace(Exception? exception, [Localizable(false)][StructuredMessageTemplate] string message, params object?[] args)
        {
            if (IsTraceEnabled)
            {
                WriteToTargets(LogLevel.Trace, exception, Factory.DefaultCultureInfo, message, args);
            }
        }

        /// <summary>
        /// Writes the diagnostic message and exception at the <c>Trace</c> level.
        /// </summary>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> to be written.</param>
        /// <param name="exception">An exception to be logged.</param>
        /// <param name="args">Arguments to format.</param>
        [MessageTemplateFormatMethod("message")]
        public void Trace(Exception? exception, IFormatProvider? formatProvider, [Localizable(false)][StructuredMessageTemplate] string message, params object?[] args)
        {
            if (IsTraceEnabled)
            {
                WriteToTargets(LogLevel.Trace, exception, formatProvider, message, args);
            }
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Trace</c> level using the specified parameter and formatting it with the supplied format provider.
        /// </summary>
        /// <typeparam name="TArgument">The type of the argument.</typeparam>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The argument to format.</param>
        [MessageTemplateFormatMethod("message")]
        public void Trace<TArgument>(IFormatProvider? formatProvider, [Localizable(false)][StructuredMessageTemplate] string message, TArgument? argument)
        {
            if (IsTraceEnabled)
            {
#if NETSTANDARD2_1_OR_GREATER || NET9_0_OR_GREATER
                WriteToTargetsWithSpan(LogLevel.Trace, null, formatProvider, message, argument);
#else
                WriteToTargets(LogLevel.Trace, formatProvider, message, new object?[] { argument });
#endif
            }
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Trace</c> level using the specified parameter.
        /// </summary>
        /// <typeparam name="TArgument">The type of the argument.</typeparam>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The argument to format.</param>
        [MessageTemplateFormatMethod("message")]
        public void Trace<TArgument>([Localizable(false)][StructuredMessageTemplate] string message, TArgument? argument)
        {
            if (IsTraceEnabled)
            {
#if NETSTANDARD2_1_OR_GREATER || NET9_0_OR_GREATER
                WriteToTargetsWithSpan(LogLevel.Trace, null, Factory.DefaultCultureInfo, message, argument);
#else
                WriteToTargets(LogLevel.Trace, Factory.DefaultCultureInfo, message, new object?[] { argument });
#endif
            }
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Trace</c> level using the specified arguments formatting it with the supplied format provider.
        /// </summary>
        /// <typeparam name="TArgument1">The type of the first argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the second argument.</typeparam>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument1">The first argument to format.</param>
        /// <param name="argument2">The second argument to format.</param>
        [MessageTemplateFormatMethod("message")]
        public void Trace<TArgument1, TArgument2>(IFormatProvider? formatProvider, [Localizable(false)][StructuredMessageTemplate] string message, TArgument1? argument1, TArgument2? argument2)
        {
            if (IsTraceEnabled)
            {
#if NETSTANDARD2_1_OR_GREATER || NET9_0_OR_GREATER
                WriteToTargetsWithSpan(LogLevel.Trace, null, formatProvider, message, argument1, argument2);
#else
                WriteToTargets(LogLevel.Trace, formatProvider, message, new object?[] { argument1, argument2 });
#endif
            }
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Trace</c> level using the specified parameters.
        /// </summary>
        /// <typeparam name="TArgument1">The type of the first argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the second argument.</typeparam>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument1">The first argument to format.</param>
        /// <param name="argument2">The second argument to format.</param>
        [MessageTemplateFormatMethod("message")]
        public void Trace<TArgument1, TArgument2>([Localizable(false)][StructuredMessageTemplate] string message, TArgument1? argument1, TArgument2? argument2)
        {
            if (IsTraceEnabled)
            {
#if NETSTANDARD2_1_OR_GREATER || NET9_0_OR_GREATER
                WriteToTargetsWithSpan(LogLevel.Trace, null, Factory.DefaultCultureInfo, message, argument1, argument2);
#else
                WriteToTargets(LogLevel.Trace, Factory.DefaultCultureInfo, message, new object?[] { argument1, argument2 });
#endif
            }
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Trace</c> level using the specified arguments formatting it with the supplied format provider.
        /// </summary>
        /// <typeparam name="TArgument1">The type of the first argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the second argument.</typeparam>
        /// <typeparam name="TArgument3">The type of the third argument.</typeparam>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument1">The first argument to format.</param>
        /// <param name="argument2">The second argument to format.</param>
        /// <param name="argument3">The third argument to format.</param>
        [MessageTemplateFormatMethod("message")]
        public void Trace<TArgument1, TArgument2, TArgument3>(IFormatProvider? formatProvider, [Localizable(false)][StructuredMessageTemplate] string message, TArgument1? argument1, TArgument2? argument2, TArgument3? argument3)
        {
            if (IsTraceEnabled)
            {
#if NETSTANDARD2_1_OR_GREATER || NET9_0_OR_GREATER
                WriteToTargetsWithSpan(LogLevel.Trace, null, formatProvider, message, argument1, argument2, argument3);
#else
                WriteToTargets(LogLevel.Trace, formatProvider, message, new object?[] { argument1, argument2, argument3 });
#endif
            }
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Trace</c> level using the specified parameters.
        /// </summary>
        /// <typeparam name="TArgument1">The type of the first argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the second argument.</typeparam>
        /// <typeparam name="TArgument3">The type of the third argument.</typeparam>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument1">The first argument to format.</param>
        /// <param name="argument2">The second argument to format.</param>
        /// <param name="argument3">The third argument to format.</param>
        [MessageTemplateFormatMethod("message")]
        public void Trace<TArgument1, TArgument2, TArgument3>([Localizable(false)][StructuredMessageTemplate] string message, TArgument1? argument1, TArgument2? argument2, TArgument3? argument3)
        {
            if (IsTraceEnabled)
            {
#if NETSTANDARD2_1_OR_GREATER || NET9_0_OR_GREATER
                WriteToTargetsWithSpan(LogLevel.Trace, null, Factory.DefaultCultureInfo, message, argument1, argument2, argument3);
#else
                WriteToTargets(LogLevel.Trace, Factory.DefaultCultureInfo, message, new object?[] { argument1, argument2, argument3 });
#endif
            }
        }

        #endregion

        #region Debug() overloads

        /// <overloads>
        /// Writes the diagnostic message at the <c>Debug</c> level using the specified format provider and format parameters.
        /// </overloads>
        /// <summary>
        /// Writes the diagnostic message at the <c>Debug</c> level.
        /// </summary>
        /// <typeparam name="T">Type of the value.</typeparam>
        /// <param name="value">The value to be written.</param>
        public void Debug<T>(T? value)
        {
            if (IsDebugEnabled)
            {
                WriteToTargets(LogLevel.Debug, Factory.DefaultCultureInfo, value);
            }
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Debug</c> level.
        /// </summary>
        /// <typeparam name="T">Type of the value.</typeparam>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="value">The value to be written.</param>
        public void Debug<T>(IFormatProvider? formatProvider, T? value)
        {
            if (IsDebugEnabled)
            {
                WriteToTargets(LogLevel.Debug, formatProvider, value);
            }
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Debug</c> level.
        /// </summary>
        /// <param name="messageFunc">A function returning message to be written. Function is not evaluated if logging is not enabled.</param>
        public void Debug(LogMessageGenerator messageFunc)
        {
            if (IsDebugEnabled)
            {
                Guard.ThrowIfNull(messageFunc);
                WriteToTargets(LogLevel.Debug, messageFunc());
            }
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Debug</c> level using the specified parameters and formatting them with the supplied format provider.
        /// </summary>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing format items.</param>
        /// <param name="args">Arguments to format.</param>
        [MessageTemplateFormatMethod("message")]
        public void Debug(IFormatProvider? formatProvider, [Localizable(false)][StructuredMessageTemplate] string message, params object?[] args)
        {
            if (IsDebugEnabled)
            {
                WriteToTargets(LogLevel.Debug, formatProvider, message, args);
            }
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Debug</c> level.
        /// </summary>
        /// <param name="message">Log message.</param>
        public void Debug([Localizable(false)] string message)
        {
            if (IsDebugEnabled)
            {
                WriteToTargets(LogLevel.Debug, message);
            }
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Debug</c> level using the specified parameters.
        /// </summary>
        /// <param name="message">A <see langword="string" /> containing format items.</param>
        /// <param name="args">Arguments to format.</param>
        [MessageTemplateFormatMethod("message")]
        public void Debug([Localizable(false)][StructuredMessageTemplate] string message, params object?[] args)
        {
            if (IsDebugEnabled)
            {
                WriteToTargets(LogLevel.Debug, Factory.DefaultCultureInfo, message, args);
            }
        }

#if NETSTANDARD2_1_OR_GREATER || NET9_0_OR_GREATER
        /// <summary>
        /// Writes the diagnostic message at the <c>Debug</c> level using the specified parameters.
        /// </summary>
        /// <param name="message">A <see langword="string" /> containing format items.</param>
        /// <param name="args">Arguments to format.</param>
        [MessageTemplateFormatMethod("message")]
        public void Debug([Localizable(false)][StructuredMessageTemplate] string message, params ReadOnlySpan<object?> args)
        {
            if (IsDebugEnabled)
            {
                WriteToTargetsWithSpan(LogLevel.Debug, null, Factory.DefaultCultureInfo, message, args);
            }
        }

        /// <summary>
        /// Writes the diagnostic message and exception at the <c>Debug</c> level.
        /// </summary>
        /// <param name="message">A <see langword="string" /> to be written.</param>
        /// <param name="exception">An exception to be logged.</param>
        /// <param name="args">Arguments to format.</param>
        [MessageTemplateFormatMethod("message")]
        public void Debug(Exception? exception, [Localizable(false)][StructuredMessageTemplate] string message, params ReadOnlySpan<object?> args)
        {
            if (IsDebugEnabled)
            {
                WriteToTargetsWithSpan(LogLevel.Debug, exception, Factory.DefaultCultureInfo, message, args);
            }
        }
#endif

        /// <summary>
        /// Writes the diagnostic message and exception at the <c>Debug</c> level.
        /// </summary>
        /// <param name="message">A <see langword="string" /> to be written.</param>
        /// <param name="exception">An exception to be logged.</param>
        public void Debug(Exception? exception, [Localizable(false)] string message)
        {
            if (IsDebugEnabled)
            {
                WriteToTargets(LogLevel.Debug, exception, Factory.DefaultCultureInfo, message, null);
            }
        }

        /// <summary>
        /// Writes the diagnostic message and exception at the <c>Debug</c> level.
        /// </summary>
        /// <param name="message">A <see langword="string" /> to be written.</param>
        /// <param name="exception">An exception to be logged.</param>
        /// <param name="args">Arguments to format.</param>
        [MessageTemplateFormatMethod("message")]
        public void Debug(Exception? exception, [Localizable(false)][StructuredMessageTemplate] string message, params object?[] args)
        {
            if (IsDebugEnabled)
            {
                WriteToTargets(LogLevel.Debug, exception, Factory.DefaultCultureInfo, message, args);
            }
        }

        /// <summary>
        /// Writes the diagnostic message and exception at the <c>Debug</c> level.
        /// </summary>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> to be written.</param>
        /// <param name="exception">An exception to be logged.</param>
        /// <param name="args">Arguments to format.</param>
        [MessageTemplateFormatMethod("message")]
        public void Debug(Exception? exception, IFormatProvider? formatProvider, [Localizable(false)][StructuredMessageTemplate] string message, params object?[] args)
        {
            if (IsDebugEnabled)
            {
                WriteToTargets(LogLevel.Debug, exception, formatProvider, message, args);
            }
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Debug</c> level using the specified parameter and formatting it with the supplied format provider.
        /// </summary>
        /// <typeparam name="TArgument">The type of the argument.</typeparam>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The argument to format.</param>
        [MessageTemplateFormatMethod("message")]
        public void Debug<TArgument>(IFormatProvider? formatProvider, [Localizable(false)][StructuredMessageTemplate] string message, TArgument? argument)
        {
            if (IsDebugEnabled)
            {
#if NETSTANDARD2_1_OR_GREATER || NET9_0_OR_GREATER
                WriteToTargetsWithSpan(LogLevel.Debug, null, formatProvider, message, argument);
#else
                WriteToTargets(LogLevel.Debug, formatProvider, message, new object?[] { argument });
#endif
            }
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Debug</c> level using the specified parameter.
        /// </summary>
        /// <typeparam name="TArgument">The type of the argument.</typeparam>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The argument to format.</param>
        [MessageTemplateFormatMethod("message")]
        public void Debug<TArgument>([Localizable(false)][StructuredMessageTemplate] string message, TArgument? argument)
        {
            if (IsDebugEnabled)
            {
#if NETSTANDARD2_1_OR_GREATER || NET9_0_OR_GREATER
                WriteToTargetsWithSpan(LogLevel.Debug, null, Factory.DefaultCultureInfo, message, argument);
#else
                WriteToTargets(LogLevel.Debug, Factory.DefaultCultureInfo, message, new object?[] { argument });
#endif
            }
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Debug</c> level using the specified arguments formatting it with the supplied format provider.
        /// </summary>
        /// <typeparam name="TArgument1">The type of the first argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the second argument.</typeparam>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument1">The first argument to format.</param>
        /// <param name="argument2">The second argument to format.</param>
        [MessageTemplateFormatMethod("message")]
        public void Debug<TArgument1, TArgument2>(IFormatProvider? formatProvider, [Localizable(false)][StructuredMessageTemplate] string message, TArgument1? argument1, TArgument2? argument2)
        {
            if (IsDebugEnabled)
            {
#if NETSTANDARD2_1_OR_GREATER || NET9_0_OR_GREATER
                WriteToTargetsWithSpan(LogLevel.Debug, null, formatProvider, message, argument1, argument2);
#else
                WriteToTargets(LogLevel.Debug, formatProvider, message, new object?[] { argument1, argument2 });
#endif
            }
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Debug</c> level using the specified parameters.
        /// </summary>
        /// <typeparam name="TArgument1">The type of the first argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the second argument.</typeparam>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument1">The first argument to format.</param>
        /// <param name="argument2">The second argument to format.</param>
        [MessageTemplateFormatMethod("message")]
        public void Debug<TArgument1, TArgument2>([Localizable(false)][StructuredMessageTemplate] string message, TArgument1? argument1, TArgument2? argument2)
        {
            if (IsDebugEnabled)
            {
#if NETSTANDARD2_1_OR_GREATER || NET9_0_OR_GREATER
                WriteToTargetsWithSpan(LogLevel.Debug, null, Factory.DefaultCultureInfo, message, argument1, argument2);
#else
                WriteToTargets(LogLevel.Debug, Factory.DefaultCultureInfo, message, new object?[] { argument1, argument2 });
#endif
            }
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Debug</c> level using the specified arguments formatting it with the supplied format provider.
        /// </summary>
        /// <typeparam name="TArgument1">The type of the first argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the second argument.</typeparam>
        /// <typeparam name="TArgument3">The type of the third argument.</typeparam>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument1">The first argument to format.</param>
        /// <param name="argument2">The second argument to format.</param>
        /// <param name="argument3">The third argument to format.</param>
        [MessageTemplateFormatMethod("message")]
        public void Debug<TArgument1, TArgument2, TArgument3>(IFormatProvider? formatProvider, [Localizable(false)][StructuredMessageTemplate] string message, TArgument1? argument1, TArgument2? argument2, TArgument3? argument3)
        {
            if (IsDebugEnabled)
            {
#if NETSTANDARD2_1_OR_GREATER || NET9_0_OR_GREATER
                WriteToTargetsWithSpan(LogLevel.Debug, null, formatProvider, message, argument1, argument2, argument3);
#else
                WriteToTargets(LogLevel.Debug, formatProvider, message, new object?[] { argument1, argument2, argument3 });
#endif
            }
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Debug</c> level using the specified parameters.
        /// </summary>
        /// <typeparam name="TArgument1">The type of the first argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the second argument.</typeparam>
        /// <typeparam name="TArgument3">The type of the third argument.</typeparam>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument1">The first argument to format.</param>
        /// <param name="argument2">The second argument to format.</param>
        /// <param name="argument3">The third argument to format.</param>
        [MessageTemplateFormatMethod("message")]
        public void Debug<TArgument1, TArgument2, TArgument3>([Localizable(false)][StructuredMessageTemplate] string message, TArgument1? argument1, TArgument2? argument2, TArgument3? argument3)
        {
            if (IsDebugEnabled)
            {
#if NETSTANDARD2_1_OR_GREATER || NET9_0_OR_GREATER
                WriteToTargetsWithSpan(LogLevel.Debug, null, Factory.DefaultCultureInfo, message, argument1, argument2, argument3);
#else
                WriteToTargets(LogLevel.Debug, Factory.DefaultCultureInfo, message, new object?[] { argument1, argument2, argument3 });
#endif
            }
        }

        #endregion

        #region Info() overloads

        /// <overloads>
        /// Writes the diagnostic message at the <c>Info</c> level using the specified format provider and format parameters.
        /// </overloads>
        /// <summary>
        /// Writes the diagnostic message at the <c>Info</c> level.
        /// </summary>
        /// <typeparam name="T">Type of the value.</typeparam>
        /// <param name="value">The value to be written.</param>
        public void Info<T>(T? value)
        {
            if (IsInfoEnabled)
            {
                WriteToTargets(LogLevel.Info, Factory.DefaultCultureInfo, value);
            }
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Info</c> level.
        /// </summary>
        /// <typeparam name="T">Type of the value.</typeparam>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="value">The value to be written.</param>
        public void Info<T>(IFormatProvider? formatProvider, T? value)
        {
            if (IsInfoEnabled)
            {
                WriteToTargets(LogLevel.Info, formatProvider, value);
            }
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Info</c> level.
        /// </summary>
        /// <param name="messageFunc">A function returning message to be written. Function is not evaluated if logging is not enabled.</param>
        public void Info(LogMessageGenerator messageFunc)
        {
            if (IsInfoEnabled)
            {
                Guard.ThrowIfNull(messageFunc);
                WriteToTargets(LogLevel.Info, messageFunc());
            }
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Info</c> level using the specified parameters and formatting them with the supplied format provider.
        /// </summary>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing format items.</param>
        /// <param name="args">Arguments to format.</param>
        [MessageTemplateFormatMethod("message")]
        public void Info(IFormatProvider? formatProvider, [Localizable(false)][StructuredMessageTemplate] string message, params object?[] args)
        {
            if (IsInfoEnabled)
            {
                WriteToTargets(LogLevel.Info, formatProvider, message, args);
            }
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Info</c> level.
        /// </summary>
        /// <param name="message">Log message.</param>
        public void Info([Localizable(false)] string message)
        {
            if (IsInfoEnabled)
            {
                WriteToTargets(LogLevel.Info, message);
            }
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Info</c> level using the specified parameters.
        /// </summary>
        /// <param name="message">A <see langword="string" /> containing format items.</param>
        /// <param name="args">Arguments to format.</param>
        [MessageTemplateFormatMethod("message")]
        public void Info([Localizable(false)][StructuredMessageTemplate] string message, params object?[] args)
        {
            if (IsInfoEnabled)
            {
                WriteToTargets(LogLevel.Info, Factory.DefaultCultureInfo, message, args);
            }
        }

#if NETSTANDARD2_1_OR_GREATER || NET9_0_OR_GREATER
        /// <summary>
        /// Writes the diagnostic message at the <c>Info</c> level using the specified parameters.
        /// </summary>
        /// <param name="message">A <see langword="string" /> containing format items.</param>
        /// <param name="args">Arguments to format.</param>
        [MessageTemplateFormatMethod("message")]
        public void Info([Localizable(false)][StructuredMessageTemplate] string message, params ReadOnlySpan<object?> args)
        {
            if (IsInfoEnabled)
            {
                WriteToTargetsWithSpan(LogLevel.Info, null, Factory.DefaultCultureInfo, message, args);
            }
        }

        /// <summary>
        /// Writes the diagnostic message and exception at the <c>Info</c> level.
        /// </summary>
        /// <param name="message">A <see langword="string" /> to be written.</param>
        /// <param name="exception">An exception to be logged.</param>
        /// <param name="args">Arguments to format.</param>
        [MessageTemplateFormatMethod("message")]
        public void Info(Exception? exception, [Localizable(false)][StructuredMessageTemplate] string message, params ReadOnlySpan<object?> args)
        {
            if (IsInfoEnabled)
            {
                WriteToTargetsWithSpan(LogLevel.Info, exception, Factory.DefaultCultureInfo, message, args);
            }
        }
#endif

        /// <summary>
        /// Writes the diagnostic message and exception at the <c>Info</c> level.
        /// </summary>
        /// <param name="message">A <see langword="string" /> to be written.</param>
        /// <param name="exception">An exception to be logged.</param>
        public void Info(Exception? exception, [Localizable(false)] string message)
        {
            if (IsInfoEnabled)
            {
                WriteToTargets(LogLevel.Info, exception, Factory.DefaultCultureInfo, message, null);
            }
        }

        /// <summary>
        /// Writes the diagnostic message and exception at the <c>Info</c> level.
        /// </summary>
        /// <param name="message">A <see langword="string" /> to be written.</param>
        /// <param name="exception">An exception to be logged.</param>
        /// <param name="args">Arguments to format.</param>
        [MessageTemplateFormatMethod("message")]
        public void Info(Exception? exception, [Localizable(false)][StructuredMessageTemplate] string message, params object?[] args)
        {
            if (IsInfoEnabled)
            {
                WriteToTargets(LogLevel.Info, exception, Factory.DefaultCultureInfo, message, args);
            }
        }

        /// <summary>
        /// Writes the diagnostic message and exception at the <c>Info</c> level.
        /// </summary>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> to be written.</param>
        /// <param name="exception">An exception to be logged.</param>
        /// <param name="args">Arguments to format.</param>
        [MessageTemplateFormatMethod("message")]
        public void Info(Exception? exception, IFormatProvider? formatProvider, [Localizable(false)][StructuredMessageTemplate] string message, params object?[] args)
        {
            if (IsInfoEnabled)
            {
                WriteToTargets(LogLevel.Info, exception, formatProvider, message, args);
            }
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Info</c> level using the specified parameter and formatting it with the supplied format provider.
        /// </summary>
        /// <typeparam name="TArgument">The type of the argument.</typeparam>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The argument to format.</param>
        [MessageTemplateFormatMethod("message")]
        public void Info<TArgument>(IFormatProvider? formatProvider, [Localizable(false)][StructuredMessageTemplate] string message, TArgument? argument)
        {
            if (IsInfoEnabled)
            {
#if NETSTANDARD2_1_OR_GREATER || NET9_0_OR_GREATER
                WriteToTargetsWithSpan(LogLevel.Info, null, formatProvider, message, argument);
#else
                WriteToTargets(LogLevel.Info, formatProvider, message, new object?[] { argument });
#endif
            }
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Info</c> level using the specified parameter.
        /// </summary>
        /// <typeparam name="TArgument">The type of the argument.</typeparam>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The argument to format.</param>
        [MessageTemplateFormatMethod("message")]
        public void Info<TArgument>([Localizable(false)][StructuredMessageTemplate] string message, TArgument? argument)
        {
            if (IsInfoEnabled)
            {
#if NETSTANDARD2_1_OR_GREATER || NET9_0_OR_GREATER
                WriteToTargetsWithSpan(LogLevel.Info, null, Factory.DefaultCultureInfo, message, argument);
#else
                WriteToTargets(LogLevel.Info, Factory.DefaultCultureInfo, message, new object?[] { argument });
#endif
            }
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Info</c> level using the specified arguments formatting it with the supplied format provider.
        /// </summary>
        /// <typeparam name="TArgument1">The type of the first argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the second argument.</typeparam>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument1">The first argument to format.</param>
        /// <param name="argument2">The second argument to format.</param>
        [MessageTemplateFormatMethod("message")]
        public void Info<TArgument1, TArgument2>(IFormatProvider? formatProvider, [Localizable(false)][StructuredMessageTemplate] string message, TArgument1? argument1, TArgument2? argument2)
        {
            if (IsInfoEnabled)
            {
#if NETSTANDARD2_1_OR_GREATER || NET9_0_OR_GREATER
                WriteToTargetsWithSpan(LogLevel.Info, null, formatProvider, message, argument1, argument2);
#else
                WriteToTargets(LogLevel.Info, formatProvider, message, new object?[] { argument1, argument2 });
#endif
            }
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Info</c> level using the specified parameters.
        /// </summary>
        /// <typeparam name="TArgument1">The type of the first argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the second argument.</typeparam>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument1">The first argument to format.</param>
        /// <param name="argument2">The second argument to format.</param>
        [MessageTemplateFormatMethod("message")]
        public void Info<TArgument1, TArgument2>([Localizable(false)][StructuredMessageTemplate] string message, TArgument1? argument1, TArgument2? argument2)
        {
            if (IsInfoEnabled)
            {
#if NETSTANDARD2_1_OR_GREATER || NET9_0_OR_GREATER
                WriteToTargetsWithSpan(LogLevel.Info, null, Factory.DefaultCultureInfo, message, argument1, argument2);
#else
                WriteToTargets(LogLevel.Info, Factory.DefaultCultureInfo, message, new object?[] { argument1, argument2 });
#endif
            }
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Info</c> level using the specified arguments formatting it with the supplied format provider.
        /// </summary>
        /// <typeparam name="TArgument1">The type of the first argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the second argument.</typeparam>
        /// <typeparam name="TArgument3">The type of the third argument.</typeparam>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument1">The first argument to format.</param>
        /// <param name="argument2">The second argument to format.</param>
        /// <param name="argument3">The third argument to format.</param>
        [MessageTemplateFormatMethod("message")]
        public void Info<TArgument1, TArgument2, TArgument3>(IFormatProvider? formatProvider, [Localizable(false)][StructuredMessageTemplate] string message, TArgument1? argument1, TArgument2? argument2, TArgument3? argument3)
        {
            if (IsInfoEnabled)
            {
#if NETSTANDARD2_1_OR_GREATER || NET9_0_OR_GREATER
                WriteToTargetsWithSpan(LogLevel.Info, null, formatProvider, message, argument1, argument2, argument3);
#else
                WriteToTargets(LogLevel.Info, formatProvider, message, new object?[] { argument1, argument2, argument3 });
#endif
            }
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Info</c> level using the specified parameters.
        /// </summary>
        /// <typeparam name="TArgument1">The type of the first argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the second argument.</typeparam>
        /// <typeparam name="TArgument3">The type of the third argument.</typeparam>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument1">The first argument to format.</param>
        /// <param name="argument2">The second argument to format.</param>
        /// <param name="argument3">The third argument to format.</param>
        [MessageTemplateFormatMethod("message")]
        public void Info<TArgument1, TArgument2, TArgument3>([Localizable(false)][StructuredMessageTemplate] string message, TArgument1? argument1, TArgument2? argument2, TArgument3? argument3)
        {
            if (IsInfoEnabled)
            {
#if NETSTANDARD2_1_OR_GREATER || NET9_0_OR_GREATER
                WriteToTargetsWithSpan(LogLevel.Info, null, Factory.DefaultCultureInfo, message, argument1, argument2, argument3);
#else
                WriteToTargets(LogLevel.Info, Factory.DefaultCultureInfo, message, new object?[] { argument1, argument2, argument3 });
#endif
            }
        }

        #endregion

        #region Warn() overloads

        /// <overloads>
        /// Writes the diagnostic message at the <c>Warn</c> level using the specified format provider and format parameters.
        /// </overloads>
        /// <summary>
        /// Writes the diagnostic message at the <c>Warn</c> level.
        /// </summary>
        /// <typeparam name="T">Type of the value.</typeparam>
        /// <param name="value">The value to be written.</param>
        public void Warn<T>(T? value)
        {
            if (IsWarnEnabled)
            {
                WriteToTargets(LogLevel.Warn, Factory.DefaultCultureInfo, value);
            }
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Warn</c> level.
        /// </summary>
        /// <typeparam name="T">Type of the value.</typeparam>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="value">The value to be written.</param>
        public void Warn<T>(IFormatProvider? formatProvider, T? value)
        {
            if (IsWarnEnabled)
            {
                WriteToTargets(LogLevel.Warn, formatProvider, value);
            }
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Warn</c> level.
        /// </summary>
        /// <param name="messageFunc">A function returning message to be written. Function is not evaluated if logging is not enabled.</param>
        public void Warn(LogMessageGenerator messageFunc)
        {
            if (IsWarnEnabled)
            {
                Guard.ThrowIfNull(messageFunc);
                WriteToTargets(LogLevel.Warn, messageFunc());
            }
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Warn</c> level using the specified parameters and formatting them with the supplied format provider.
        /// </summary>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing format items.</param>
        /// <param name="args">Arguments to format.</param>
        [MessageTemplateFormatMethod("message")]
        public void Warn(IFormatProvider? formatProvider, [Localizable(false)][StructuredMessageTemplate] string message, params object?[] args)
        {
            if (IsWarnEnabled)
            {
                WriteToTargets(LogLevel.Warn, formatProvider, message, args);
            }
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Warn</c> level.
        /// </summary>
        /// <param name="message">Log message.</param>
        public void Warn([Localizable(false)] string message)
        {
            if (IsWarnEnabled)
            {
                WriteToTargets(LogLevel.Warn, message);
            }
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Warn</c> level using the specified parameters.
        /// </summary>
        /// <param name="message">A <see langword="string" /> containing format items.</param>
        /// <param name="args">Arguments to format.</param>
        [MessageTemplateFormatMethod("message")]
        public void Warn([Localizable(false)][StructuredMessageTemplate] string message, params object?[] args)
        {
            if (IsWarnEnabled)
            {
                WriteToTargets(LogLevel.Warn, Factory.DefaultCultureInfo, message, args);
            }
        }

#if NETSTANDARD2_1_OR_GREATER || NET9_0_OR_GREATER
        /// <summary>
        /// Writes the diagnostic message at the <c>Warn</c> level using the specified parameters.
        /// </summary>
        /// <param name="message">A <see langword="string" /> containing format items.</param>
        /// <param name="args">Arguments to format.</param>
        [MessageTemplateFormatMethod("message")]
        public void Warn([Localizable(false)][StructuredMessageTemplate] string message, params ReadOnlySpan<object?> args)
        {
            if (IsWarnEnabled)
            {
                WriteToTargetsWithSpan(LogLevel.Warn, null, Factory.DefaultCultureInfo, message, args);
            }
        }

        /// <summary>
        /// Writes the diagnostic message and exception at the <c>Warn</c> level.
        /// </summary>
        /// <param name="message">A <see langword="string" /> to be written.</param>
        /// <param name="exception">An exception to be logged.</param>
        /// <param name="args">Arguments to format.</param>
        [MessageTemplateFormatMethod("message")]
        public void Warn(Exception? exception, [Localizable(false)][StructuredMessageTemplate] string message, params ReadOnlySpan<object?> args)
        {
            if (IsWarnEnabled)
            {
                WriteToTargetsWithSpan(LogLevel.Warn, exception, Factory.DefaultCultureInfo, message, args);
            }
        }
#endif

        /// <summary>
        /// Writes the diagnostic message and exception at the <c>Warn</c> level.
        /// </summary>
        /// <param name="message">A <see langword="string" /> to be written.</param>
        /// <param name="exception">An exception to be logged.</param>
        public void Warn(Exception? exception, [Localizable(false)] string message)
        {
            if (IsWarnEnabled)
            {
                WriteToTargets(LogLevel.Warn, exception, Factory.DefaultCultureInfo, message, null);
            }
        }

        /// <summary>
        /// Writes the diagnostic message and exception at the <c>Warn</c> level.
        /// </summary>
        /// <param name="message">A <see langword="string" /> to be written.</param>
        /// <param name="exception">An exception to be logged.</param>
        /// <param name="args">Arguments to format.</param>
        [MessageTemplateFormatMethod("message")]
        public void Warn(Exception? exception, [Localizable(false)][StructuredMessageTemplate] string message, params object?[] args)
        {
            if (IsWarnEnabled)
            {
                WriteToTargets(LogLevel.Warn, exception, Factory.DefaultCultureInfo, message, args);
            }
        }

        /// <summary>
        /// Writes the diagnostic message and exception at the <c>Warn</c> level.
        /// </summary>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> to be written.</param>
        /// <param name="exception">An exception to be logged.</param>
        /// <param name="args">Arguments to format.</param>
        [MessageTemplateFormatMethod("message")]
        public void Warn(Exception? exception, IFormatProvider? formatProvider, [Localizable(false)][StructuredMessageTemplate] string message, params object?[] args)
        {
            if (IsWarnEnabled)
            {
                WriteToTargets(LogLevel.Warn, exception, formatProvider, message, args);
            }
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Warn</c> level using the specified parameter and formatting it with the supplied format provider.
        /// </summary>
        /// <typeparam name="TArgument">The type of the argument.</typeparam>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The argument to format.</param>
        [MessageTemplateFormatMethod("message")]
        public void Warn<TArgument>(IFormatProvider? formatProvider, [Localizable(false)][StructuredMessageTemplate] string message, TArgument? argument)
        {
            if (IsWarnEnabled)
            {
#if NETSTANDARD2_1_OR_GREATER || NET9_0_OR_GREATER
                WriteToTargetsWithSpan(LogLevel.Warn, null, formatProvider, message, argument);
#else
                WriteToTargets(LogLevel.Warn, formatProvider, message, new object?[] { argument });
#endif
            }
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Warn</c> level using the specified parameter.
        /// </summary>
        /// <typeparam name="TArgument">The type of the argument.</typeparam>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The argument to format.</param>
        [MessageTemplateFormatMethod("message")]
        public void Warn<TArgument>([Localizable(false)][StructuredMessageTemplate] string message, TArgument? argument)
        {
            if (IsWarnEnabled)
            {
#if NETSTANDARD2_1_OR_GREATER || NET9_0_OR_GREATER
                WriteToTargetsWithSpan(LogLevel.Warn, null, Factory.DefaultCultureInfo, message, argument);
#else
                WriteToTargets(LogLevel.Warn, Factory.DefaultCultureInfo, message, new object?[] { argument });
#endif
            }
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Warn</c> level using the specified arguments formatting it with the supplied format provider.
        /// </summary>
        /// <typeparam name="TArgument1">The type of the first argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the second argument.</typeparam>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument1">The first argument to format.</param>
        /// <param name="argument2">The second argument to format.</param>
        [MessageTemplateFormatMethod("message")]
        public void Warn<TArgument1, TArgument2>(IFormatProvider? formatProvider, [Localizable(false)][StructuredMessageTemplate] string message, TArgument1? argument1, TArgument2? argument2)
        {
            if (IsWarnEnabled)
            {
#if NETSTANDARD2_1_OR_GREATER || NET9_0_OR_GREATER
                WriteToTargetsWithSpan(LogLevel.Warn, null, formatProvider, message, argument1, argument2);
#else
                WriteToTargets(LogLevel.Warn, formatProvider, message, new object?[] { argument1, argument2 });
#endif
            }
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Warn</c> level using the specified parameters.
        /// </summary>
        /// <typeparam name="TArgument1">The type of the first argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the second argument.</typeparam>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument1">The first argument to format.</param>
        /// <param name="argument2">The second argument to format.</param>
        [MessageTemplateFormatMethod("message")]
        public void Warn<TArgument1, TArgument2>([Localizable(false)][StructuredMessageTemplate] string message, TArgument1? argument1, TArgument2? argument2)
        {
            if (IsWarnEnabled)
            {
#if NETSTANDARD2_1_OR_GREATER || NET9_0_OR_GREATER
                WriteToTargetsWithSpan(LogLevel.Warn, null, Factory.DefaultCultureInfo, message, argument1, argument2);
#else
                WriteToTargets(LogLevel.Warn, Factory.DefaultCultureInfo, message, new object?[] { argument1, argument2 });
#endif
            }
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Warn</c> level using the specified arguments formatting it with the supplied format provider.
        /// </summary>
        /// <typeparam name="TArgument1">The type of the first argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the second argument.</typeparam>
        /// <typeparam name="TArgument3">The type of the third argument.</typeparam>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument1">The first argument to format.</param>
        /// <param name="argument2">The second argument to format.</param>
        /// <param name="argument3">The third argument to format.</param>
        [MessageTemplateFormatMethod("message")]
        public void Warn<TArgument1, TArgument2, TArgument3>(IFormatProvider? formatProvider, [Localizable(false)][StructuredMessageTemplate] string message, TArgument1? argument1, TArgument2? argument2, TArgument3? argument3)
        {
            if (IsWarnEnabled)
            {
#if NETSTANDARD2_1_OR_GREATER || NET9_0_OR_GREATER
                WriteToTargetsWithSpan(LogLevel.Warn, null, formatProvider, message, argument1, argument2, argument3);
#else
                WriteToTargets(LogLevel.Warn, formatProvider, message, new object?[] { argument1, argument2, argument3 });
#endif
            }
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Warn</c> level using the specified parameters.
        /// </summary>
        /// <typeparam name="TArgument1">The type of the first argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the second argument.</typeparam>
        /// <typeparam name="TArgument3">The type of the third argument.</typeparam>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument1">The first argument to format.</param>
        /// <param name="argument2">The second argument to format.</param>
        /// <param name="argument3">The third argument to format.</param>
        [MessageTemplateFormatMethod("message")]
        public void Warn<TArgument1, TArgument2, TArgument3>([Localizable(false)][StructuredMessageTemplate] string message, TArgument1? argument1, TArgument2? argument2, TArgument3? argument3)
        {
            if (IsWarnEnabled)
            {
#if NETSTANDARD2_1_OR_GREATER || NET9_0_OR_GREATER
                WriteToTargetsWithSpan(LogLevel.Warn, null, Factory.DefaultCultureInfo, message, argument1, argument2, argument3);
#else
                WriteToTargets(LogLevel.Warn, Factory.DefaultCultureInfo, message, new object?[] { argument1, argument2, argument3 });
#endif
            }
        }

        #endregion

        #region Error() overloads

        /// <overloads>
        /// Writes the diagnostic message at the <c>Error</c> level using the specified format provider and format parameters.
        /// </overloads>
        /// <summary>
        /// Writes the diagnostic message at the <c>Error</c> level.
        /// </summary>
        /// <typeparam name="T">Type of the value.</typeparam>
        /// <param name="value">The value to be written.</param>
        public void Error<T>(T? value)
        {
            if (IsErrorEnabled)
            {
                WriteToTargets(LogLevel.Error, Factory.DefaultCultureInfo, value);
            }
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Error</c> level.
        /// </summary>
        /// <typeparam name="T">Type of the value.</typeparam>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="value">The value to be written.</param>
        public void Error<T>(IFormatProvider? formatProvider, T? value)
        {
            if (IsErrorEnabled)
            {
                WriteToTargets(LogLevel.Error, formatProvider, value);
            }
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Error</c> level.
        /// </summary>
        /// <param name="messageFunc">A function returning message to be written. Function is not evaluated if logging is not enabled.</param>
        public void Error(LogMessageGenerator messageFunc)
        {
            if (IsErrorEnabled)
            {
                Guard.ThrowIfNull(messageFunc);
                WriteToTargets(LogLevel.Error, messageFunc());
            }
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Error</c> level using the specified parameters and formatting them with the supplied format provider.
        /// </summary>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing format items.</param>
        /// <param name="args">Arguments to format.</param>
        [MessageTemplateFormatMethod("message")]
        public void Error(IFormatProvider? formatProvider, [Localizable(false)][StructuredMessageTemplate] string message, params object?[] args)
        {
            if (IsErrorEnabled)
            {
                WriteToTargets(LogLevel.Error, formatProvider, message, args);
            }
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Error</c> level.
        /// </summary>
        /// <param name="message">Log message.</param>
        public void Error([Localizable(false)] string message)
        {
            if (IsErrorEnabled)
            {
                WriteToTargets(LogLevel.Error, message);
            }
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Error</c> level using the specified parameters.
        /// </summary>
        /// <param name="message">A <see langword="string" /> containing format items.</param>
        /// <param name="args">Arguments to format.</param>
        [MessageTemplateFormatMethod("message")]
        public void Error([Localizable(false)][StructuredMessageTemplate] string message, params object?[] args)
        {
            if (IsErrorEnabled)
            {
                WriteToTargets(LogLevel.Error, Factory.DefaultCultureInfo, message, args);
            }
        }

#if NETSTANDARD2_1_OR_GREATER || NET9_0_OR_GREATER
        /// <summary>
        /// Writes the diagnostic message at the <c>Error</c> level using the specified parameters.
        /// </summary>
        /// <param name="message">A <see langword="string" /> containing format items.</param>
        /// <param name="args">Arguments to format.</param>
        [MessageTemplateFormatMethod("message")]
        public void Error([Localizable(false)][StructuredMessageTemplate] string message, params ReadOnlySpan<object?> args)
        {
            if (IsErrorEnabled)
            {
                WriteToTargetsWithSpan(LogLevel.Error, null, Factory.DefaultCultureInfo, message, args);
            }
        }

        /// <summary>
        /// Writes the diagnostic message and exception at the <c>Error</c> level.
        /// </summary>
        /// <param name="message">A <see langword="string" /> to be written.</param>
        /// <param name="exception">An exception to be logged.</param>
        /// <param name="args">Arguments to format.</param>
        [MessageTemplateFormatMethod("message")]
        public void Error(Exception? exception, [Localizable(false)][StructuredMessageTemplate] string message, params ReadOnlySpan<object?> args)
        {
            if (IsErrorEnabled)
            {
                WriteToTargetsWithSpan(LogLevel.Error, exception, Factory.DefaultCultureInfo, message, args);
            }
        }
#endif

        /// <summary>
        /// Writes the diagnostic message and exception at the <c>Error</c> level.
        /// </summary>
        /// <param name="message">A <see langword="string" /> to be written.</param>
        /// <param name="exception">An exception to be logged.</param>
        public void Error(Exception? exception, [Localizable(false)] string message)
        {
            if (IsErrorEnabled)
            {
                WriteToTargets(LogLevel.Error, exception, Factory.DefaultCultureInfo, message, null);
            }
        }

        /// <summary>
        /// Writes the diagnostic message and exception at the <c>Error</c> level.
        /// </summary>
        /// <param name="message">A <see langword="string" /> to be written.</param>
        /// <param name="exception">An exception to be logged.</param>
        /// <param name="args">Arguments to format.</param>
        [MessageTemplateFormatMethod("message")]
        public void Error(Exception? exception, [Localizable(false)][StructuredMessageTemplate] string message, params object?[] args)
        {
            if (IsErrorEnabled)
            {
                WriteToTargets(LogLevel.Error, exception, Factory.DefaultCultureInfo, message, args);
            }
        }

        /// <summary>
        /// Writes the diagnostic message and exception at the <c>Error</c> level.
        /// </summary>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> to be written.</param>
        /// <param name="exception">An exception to be logged.</param>
        /// <param name="args">Arguments to format.</param>
        [MessageTemplateFormatMethod("message")]
        public void Error(Exception? exception, IFormatProvider? formatProvider, [Localizable(false)][StructuredMessageTemplate] string message, params object?[] args)
        {
            if (IsErrorEnabled)
            {
                WriteToTargets(LogLevel.Error, exception, formatProvider, message, args);
            }
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Error</c> level using the specified parameter and formatting it with the supplied format provider.
        /// </summary>
        /// <typeparam name="TArgument">The type of the argument.</typeparam>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The argument to format.</param>
        [MessageTemplateFormatMethod("message")]
        public void Error<TArgument>(IFormatProvider? formatProvider, [Localizable(false)][StructuredMessageTemplate] string message, TArgument? argument)
        {
            if (IsErrorEnabled)
            {
#if NETSTANDARD2_1_OR_GREATER || NET9_0_OR_GREATER
                WriteToTargetsWithSpan(LogLevel.Error, null, formatProvider, message, argument);
#else
                WriteToTargets(LogLevel.Error, formatProvider, message, new object?[] { argument });
#endif
            }
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Error</c> level using the specified parameter.
        /// </summary>
        /// <typeparam name="TArgument">The type of the argument.</typeparam>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The argument to format.</param>
        [MessageTemplateFormatMethod("message")]
        public void Error<TArgument>([Localizable(false)][StructuredMessageTemplate] string message, TArgument? argument)
        {
            if (IsErrorEnabled)
            {
#if NETSTANDARD2_1_OR_GREATER || NET9_0_OR_GREATER
                WriteToTargetsWithSpan(LogLevel.Error, null, Factory.DefaultCultureInfo, message, argument);
#else
                WriteToTargets(LogLevel.Error, Factory.DefaultCultureInfo, message, new object?[] { argument });
#endif
            }
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Error</c> level using the specified arguments formatting it with the supplied format provider.
        /// </summary>
        /// <typeparam name="TArgument1">The type of the first argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the second argument.</typeparam>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument1">The first argument to format.</param>
        /// <param name="argument2">The second argument to format.</param>
        [MessageTemplateFormatMethod("message")]
        public void Error<TArgument1, TArgument2>(IFormatProvider? formatProvider, [Localizable(false)][StructuredMessageTemplate] string message, TArgument1? argument1, TArgument2? argument2)
        {
            if (IsErrorEnabled)
            {
#if NETSTANDARD2_1_OR_GREATER || NET9_0_OR_GREATER
                WriteToTargetsWithSpan(LogLevel.Error, null, formatProvider, message, argument1, argument2);
#else
                WriteToTargets(LogLevel.Error, formatProvider, message, new object?[] { argument1, argument2 });
#endif
            }
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Error</c> level using the specified parameters.
        /// </summary>
        /// <typeparam name="TArgument1">The type of the first argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the second argument.</typeparam>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument1">The first argument to format.</param>
        /// <param name="argument2">The second argument to format.</param>
        [MessageTemplateFormatMethod("message")]
        public void Error<TArgument1, TArgument2>([Localizable(false)][StructuredMessageTemplate] string message, TArgument1? argument1, TArgument2? argument2)
        {
            if (IsErrorEnabled)
            {
#if NETSTANDARD2_1_OR_GREATER || NET9_0_OR_GREATER
                WriteToTargetsWithSpan(LogLevel.Error, null, Factory.DefaultCultureInfo, message, argument1, argument2);
#else
                WriteToTargets(LogLevel.Error, Factory.DefaultCultureInfo, message, new object?[] { argument1, argument2 });
#endif
            }
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Error</c> level using the specified arguments formatting it with the supplied format provider.
        /// </summary>
        /// <typeparam name="TArgument1">The type of the first argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the second argument.</typeparam>
        /// <typeparam name="TArgument3">The type of the third argument.</typeparam>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument1">The first argument to format.</param>
        /// <param name="argument2">The second argument to format.</param>
        /// <param name="argument3">The third argument to format.</param>
        [MessageTemplateFormatMethod("message")]
        public void Error<TArgument1, TArgument2, TArgument3>(IFormatProvider? formatProvider, [Localizable(false)][StructuredMessageTemplate] string message, TArgument1? argument1, TArgument2? argument2, TArgument3? argument3)
        {
            if (IsErrorEnabled)
            {
#if NETSTANDARD2_1_OR_GREATER || NET9_0_OR_GREATER
                WriteToTargetsWithSpan(LogLevel.Error, null, formatProvider, message, argument1, argument2, argument3);
#else
                WriteToTargets(LogLevel.Error, formatProvider, message, new object?[] { argument1, argument2, argument3 });
#endif
            }
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Error</c> level using the specified parameters.
        /// </summary>
        /// <typeparam name="TArgument1">The type of the first argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the second argument.</typeparam>
        /// <typeparam name="TArgument3">The type of the third argument.</typeparam>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument1">The first argument to format.</param>
        /// <param name="argument2">The second argument to format.</param>
        /// <param name="argument3">The third argument to format.</param>
        [MessageTemplateFormatMethod("message")]
        public void Error<TArgument1, TArgument2, TArgument3>([Localizable(false)][StructuredMessageTemplate] string message, TArgument1? argument1, TArgument2? argument2, TArgument3? argument3)
        {
            if (IsErrorEnabled)
            {
#if NETSTANDARD2_1_OR_GREATER || NET9_0_OR_GREATER
                WriteToTargetsWithSpan(LogLevel.Error, null, Factory.DefaultCultureInfo, message, argument1, argument2, argument3);
#else
                WriteToTargets(LogLevel.Error, Factory.DefaultCultureInfo, message, new object?[] { argument1, argument2, argument3 });
#endif
            }
        }

        #endregion

        #region Fatal() overloads

        /// <overloads>
        /// Writes the diagnostic message at the <c>Fatal</c> level using the specified format provider and format parameters.
        /// </overloads>
        /// <summary>
        /// Writes the diagnostic message at the <c>Fatal</c> level.
        /// </summary>
        /// <typeparam name="T">Type of the value.</typeparam>
        /// <param name="value">The value to be written.</param>
        public void Fatal<T>(T? value)
        {
            if (IsFatalEnabled)
            {
                WriteToTargets(LogLevel.Fatal, Factory.DefaultCultureInfo, value);
            }
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Fatal</c> level.
        /// </summary>
        /// <typeparam name="T">Type of the value.</typeparam>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="value">The value to be written.</param>
        public void Fatal<T>(IFormatProvider? formatProvider, T? value)
        {
            if (IsFatalEnabled)
            {
                WriteToTargets(LogLevel.Fatal, formatProvider, value);
            }
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Fatal</c> level.
        /// </summary>
        /// <param name="messageFunc">A function returning message to be written. Function is not evaluated if logging is not enabled.</param>
        public void Fatal(LogMessageGenerator messageFunc)
        {
            if (IsFatalEnabled)
            {
                Guard.ThrowIfNull(messageFunc);
                WriteToTargets(LogLevel.Fatal, messageFunc());
            }
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Fatal</c> level using the specified parameters and formatting them with the supplied format provider.
        /// </summary>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing format items.</param>
        /// <param name="args">Arguments to format.</param>
        [MessageTemplateFormatMethod("message")]
        public void Fatal(IFormatProvider? formatProvider, [Localizable(false)][StructuredMessageTemplate] string message, params object?[] args)
        {
            if (IsFatalEnabled)
            {
                WriteToTargets(LogLevel.Fatal, formatProvider, message, args);
            }
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Fatal</c> level.
        /// </summary>
        /// <param name="message">Log message.</param>
        public void Fatal([Localizable(false)] string message)
        {
            if (IsFatalEnabled)
            {
                WriteToTargets(LogLevel.Fatal, message);
            }
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Fatal</c> level using the specified parameters.
        /// </summary>
        /// <param name="message">A <see langword="string" /> containing format items.</param>
        /// <param name="args">Arguments to format.</param>
        [MessageTemplateFormatMethod("message")]
        public void Fatal([Localizable(false)][StructuredMessageTemplate] string message, params object?[] args)
        {
            if (IsFatalEnabled)
            {
                WriteToTargets(LogLevel.Fatal, Factory.DefaultCultureInfo, message, args);
            }
        }

#if NETSTANDARD2_1_OR_GREATER || NET9_0_OR_GREATER
        /// <summary>
        /// Writes the diagnostic message at the <c>Fatal</c> level using the specified parameters.
        /// </summary>
        /// <param name="message">A <see langword="string" /> containing format items.</param>
        /// <param name="args">Arguments to format.</param>
        [MessageTemplateFormatMethod("message")]
        public void Fatal([Localizable(false)][StructuredMessageTemplate] string message, params ReadOnlySpan<object?> args)
        {
            if (IsFatalEnabled)
            {
                WriteToTargetsWithSpan(LogLevel.Fatal, null, Factory.DefaultCultureInfo, message, args);
            }
        }

        /// <summary>
        /// Writes the diagnostic message and exception at the <c>Fatal</c> level.
        /// </summary>
        /// <param name="message">A <see langword="string" /> to be written.</param>
        /// <param name="exception">An exception to be logged.</param>
        /// <param name="args">Arguments to format.</param>
        [MessageTemplateFormatMethod("message")]
        public void Fatal(Exception? exception, [Localizable(false)][StructuredMessageTemplate] string message, params ReadOnlySpan<object?> args)
        {
            if (IsFatalEnabled)
            {
                WriteToTargetsWithSpan(LogLevel.Fatal, exception, Factory.DefaultCultureInfo, message, args);
            }
        }
#endif

        /// <summary>
        /// Writes the diagnostic message and exception at the <c>Fatal</c> level.
        /// </summary>
        /// <param name="message">A <see langword="string" /> to be written.</param>
        /// <param name="exception">An exception to be logged.</param>
        public void Fatal(Exception? exception, [Localizable(false)] string message)
        {
            if (IsFatalEnabled)
            {
                WriteToTargets(LogLevel.Fatal, exception, Factory.DefaultCultureInfo, message, null);
            }
        }

        /// <summary>
        /// Writes the diagnostic message and exception at the <c>Fatal</c> level.
        /// </summary>
        /// <param name="message">A <see langword="string" /> to be written.</param>
        /// <param name="exception">An exception to be logged.</param>
        /// <param name="args">Arguments to format.</param>
        [MessageTemplateFormatMethod("message")]
        public void Fatal(Exception? exception, [Localizable(false)][StructuredMessageTemplate] string message, params object?[] args)
        {
            if (IsFatalEnabled)
            {
                WriteToTargets(LogLevel.Fatal, exception, Factory.DefaultCultureInfo, message, args);
            }
        }

        /// <summary>
        /// Writes the diagnostic message and exception at the <c>Fatal</c> level.
        /// </summary>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> to be written.</param>
        /// <param name="exception">An exception to be logged.</param>
        /// <param name="args">Arguments to format.</param>
        [MessageTemplateFormatMethod("message")]
        public void Fatal(Exception? exception, IFormatProvider? formatProvider, [Localizable(false)][StructuredMessageTemplate] string message, params object?[] args)
        {
            if (IsFatalEnabled)
            {
                WriteToTargets(LogLevel.Fatal, exception, formatProvider, message, args);
            }
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Fatal</c> level using the specified parameter and formatting it with the supplied format provider.
        /// </summary>
        /// <typeparam name="TArgument">The type of the argument.</typeparam>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The argument to format.</param>
        [MessageTemplateFormatMethod("message")]
        public void Fatal<TArgument>(IFormatProvider? formatProvider, [Localizable(false)][StructuredMessageTemplate] string message, TArgument? argument)
        {
            if (IsFatalEnabled)
            {
#if NETSTANDARD2_1_OR_GREATER || NET9_0_OR_GREATER
                WriteToTargetsWithSpan(LogLevel.Fatal, null, formatProvider, message, argument);
#else
                WriteToTargets(LogLevel.Fatal, formatProvider, message, new object?[] { argument });
#endif
            }
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Fatal</c> level using the specified parameter.
        /// </summary>
        /// <typeparam name="TArgument">The type of the argument.</typeparam>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The argument to format.</param>
        [MessageTemplateFormatMethod("message")]
        public void Fatal<TArgument>([Localizable(false)][StructuredMessageTemplate] string message, TArgument? argument)
        {
            if (IsFatalEnabled)
            {
#if NETSTANDARD2_1_OR_GREATER || NET9_0_OR_GREATER
                WriteToTargetsWithSpan(LogLevel.Fatal, null, Factory.DefaultCultureInfo, message, argument);
#else
                WriteToTargets(LogLevel.Fatal, Factory.DefaultCultureInfo, message, new object?[] { argument });
#endif
            }
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Fatal</c> level using the specified arguments formatting it with the supplied format provider.
        /// </summary>
        /// <typeparam name="TArgument1">The type of the first argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the second argument.</typeparam>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument1">The first argument to format.</param>
        /// <param name="argument2">The second argument to format.</param>
        [MessageTemplateFormatMethod("message")]
        public void Fatal<TArgument1, TArgument2>(IFormatProvider? formatProvider, [Localizable(false)][StructuredMessageTemplate] string message, TArgument1? argument1, TArgument2? argument2)
        {
            if (IsFatalEnabled)
            {
#if NETSTANDARD2_1_OR_GREATER || NET9_0_OR_GREATER
                WriteToTargetsWithSpan(LogLevel.Fatal, null, formatProvider, message, argument1, argument2);
#else
                WriteToTargets(LogLevel.Fatal, formatProvider, message, new object?[] { argument1, argument2 });
#endif
            }
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Fatal</c> level using the specified parameters.
        /// </summary>
        /// <typeparam name="TArgument1">The type of the first argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the second argument.</typeparam>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument1">The first argument to format.</param>
        /// <param name="argument2">The second argument to format.</param>
        [MessageTemplateFormatMethod("message")]
        public void Fatal<TArgument1, TArgument2>([Localizable(false)][StructuredMessageTemplate] string message, TArgument1? argument1, TArgument2? argument2)
        {
            if (IsFatalEnabled)
            {
#if NETSTANDARD2_1_OR_GREATER || NET9_0_OR_GREATER
                WriteToTargetsWithSpan(LogLevel.Fatal, null, Factory.DefaultCultureInfo, message, argument1, argument2);
#else
                WriteToTargets(LogLevel.Fatal, Factory.DefaultCultureInfo, message, new object?[] { argument1, argument2 });
#endif
            }
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Fatal</c> level using the specified arguments formatting it with the supplied format provider.
        /// </summary>
        /// <typeparam name="TArgument1">The type of the first argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the second argument.</typeparam>
        /// <typeparam name="TArgument3">The type of the third argument.</typeparam>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument1">The first argument to format.</param>
        /// <param name="argument2">The second argument to format.</param>
        /// <param name="argument3">The third argument to format.</param>
        [MessageTemplateFormatMethod("message")]
        public void Fatal<TArgument1, TArgument2, TArgument3>(IFormatProvider? formatProvider, [Localizable(false)][StructuredMessageTemplate] string message, TArgument1? argument1, TArgument2? argument2, TArgument3? argument3)
        {
            if (IsFatalEnabled)
            {
#if NETSTANDARD2_1_OR_GREATER || NET9_0_OR_GREATER
                WriteToTargetsWithSpan(LogLevel.Fatal, null, formatProvider, message, argument1, argument2, argument3);
#else
                WriteToTargets(LogLevel.Fatal, formatProvider, message, new object?[] { argument1, argument2, argument3 });
#endif
            }
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Fatal</c> level using the specified parameters.
        /// </summary>
        /// <typeparam name="TArgument1">The type of the first argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the second argument.</typeparam>
        /// <typeparam name="TArgument3">The type of the third argument.</typeparam>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument1">The first argument to format.</param>
        /// <param name="argument2">The second argument to format.</param>
        /// <param name="argument3">The third argument to format.</param>
        [MessageTemplateFormatMethod("message")]
        public void Fatal<TArgument1, TArgument2, TArgument3>([Localizable(false)][StructuredMessageTemplate] string message, TArgument1? argument1, TArgument2? argument2, TArgument3? argument3)
        {
            if (IsFatalEnabled)
            {
#if NETSTANDARD2_1_OR_GREATER || NET9_0_OR_GREATER
                WriteToTargetsWithSpan(LogLevel.Fatal, null, Factory.DefaultCultureInfo, message, argument1, argument2, argument3);
#else
                WriteToTargets(LogLevel.Fatal, Factory.DefaultCultureInfo, message, new object?[] { argument1, argument2, argument3 });
#endif
            }
        }

        #endregion
    }
}