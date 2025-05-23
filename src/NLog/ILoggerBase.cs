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

    /// <summary>
    /// Obsolete and replaced by <see cref="ILogger"/> with NLog v5.3.
    ///
    /// Logger with only generic methods (passing 'LogLevel' to methods) and core properties.
    /// </summary>
    [Obsolete("ILoggerBase should be replaced with ILogger. Marked obsolete with NLog v5.3")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public partial interface ILoggerBase
    {
        /// <summary>
        /// Obsolete on the ILogger-interface, instead use <see cref="Logger.LoggerReconfigured"/> with NLog v5.3.
        /// Occurs when logger configuration changes.
        /// </summary>
        [Obsolete("LoggerReconfigured-EventHandler is very exotic for ILogger-interface. Instead use Logger.LoggerReconfigured. Marked obsolete with NLog v5.3")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        event EventHandler<EventArgs> LoggerReconfigured;

        /// <summary>
        /// Gets the name of the logger.
        /// </summary>
        string Name
        {
            get;
        }

        /// <summary>
        /// Obsolete on the ILogger-interface, instead use <see cref="Logger.Factory"/> with NLog v5.3.
        /// Gets the factory that created this logger.
        /// </summary>
        [Obsolete("Factory-property is hard to mock for ILogger-interface. Instead use Logger.Factory. Marked obsolete with NLog v5.3")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        LogFactory Factory
        {
            get;
        }

        /// <summary>
        /// Gets a value indicating whether logging is enabled for the specified level.
        /// </summary>
        /// <param name="level">Log level to be checked.</param>
        /// <returns>A value of <see langword="true" /> if logging is enabled for the specified level, otherwise it returns <see langword="false" />.</returns>
        bool IsEnabled(LogLevel level);

        #region Log() overloads

        /// <summary>
        /// Writes the specified diagnostic message.
        /// </summary>
        /// <param name="logEvent">Log event.</param>
        void Log(LogEventInfo logEvent);

        /// <summary>
        /// Writes the specified diagnostic message.
        /// </summary>
        /// <param name="wrapperType">Type of custom Logger wrapper.</param>
        /// <param name="logEvent">Log event.</param>
        void Log(Type wrapperType, LogEventInfo logEvent);

        /// <overloads>
        /// Writes the diagnostic message at the specified level using the specified format provider and format parameters.
        /// </overloads>
        /// <summary>
        /// Writes the diagnostic message at the specified level.
        /// </summary>
        /// <typeparam name="T">Type of the value.</typeparam>
        /// <param name="level">The log level.</param>
        /// <param name="value">The value to be written.</param>
        void Log<T>(LogLevel level, T? value);

        /// <summary>
        /// Writes the diagnostic message at the specified level.
        /// </summary>
        /// <typeparam name="T">Type of the value.</typeparam>
        /// <param name="level">The log level.</param>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="value">The value to be written.</param>
        void Log<T>(LogLevel level, IFormatProvider? formatProvider, T? value);

        /// <summary>
        /// Writes the diagnostic message at the specified level.
        /// </summary>
        /// <param name="level">The log level.</param>
        /// <param name="messageFunc">A function returning message to be written. Function is not evaluated if logging is not enabled.</param>
        void Log(LogLevel level, LogMessageGenerator messageFunc);

        /// <summary>
        /// Writes the diagnostic message and exception at the specified level.
        /// </summary>
        /// <param name="level">The log level.</param>
        /// <param name="message">A <see langword="string" /> to be written.</param>
        /// <param name="args">Arguments to format.</param>
        /// <param name="exception">An exception to be logged.</param>
        [MessageTemplateFormatMethod("message")]
        void Log(LogLevel level, Exception? exception, [Localizable(false)][StructuredMessageTemplate] string message, params object?[] args);

        /// <summary>
        /// Writes the diagnostic message and exception at the specified level.
        /// </summary>
        /// <param name="level">The log level.</param>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> to be written.</param>
        /// <param name="args">Arguments to format.</param>
        /// <param name="exception">An exception to be logged.</param>
        [MessageTemplateFormatMethod("message")]
        void Log(LogLevel level, Exception? exception, IFormatProvider? formatProvider, [Localizable(false)][StructuredMessageTemplate] string message, params object?[] args);

        /// <summary>
        /// Writes the diagnostic message at the specified level using the specified parameters and formatting them with the supplied format provider.
        /// </summary>
        /// <param name="level">The log level.</param>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing format items.</param>
        /// <param name="args">Arguments to format.</param>
        [MessageTemplateFormatMethod("message")]
        void Log(LogLevel level, IFormatProvider? formatProvider, [Localizable(false)][StructuredMessageTemplate] string message, params object?[] args);

        /// <summary>
        /// Writes the diagnostic message at the specified level.
        /// </summary>
        /// <param name="level">The log level.</param>
        /// <param name="message">Log message.</param>
        void Log(LogLevel level, [Localizable(false)] string message);

        /// <summary>
        /// Writes the diagnostic message at the specified level using the specified parameters.
        /// </summary>
        /// <param name="level">The log level.</param>
        /// <param name="message">A <see langword="string" /> containing format items.</param>
        /// <param name="args">Arguments to format.</param>
        [MessageTemplateFormatMethod("message")]
        void Log(LogLevel level, [Localizable(false)][StructuredMessageTemplate] string message, params object?[] args);

        /// <summary>
        /// Writes the diagnostic message at the specified level using the specified parameter and formatting it with the supplied format provider.
        /// </summary>
        /// <typeparam name="TArgument">The type of the argument.</typeparam>
        /// <param name="level">The log level.</param>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The argument to format.</param>
        [MessageTemplateFormatMethod("message")]
        void Log<TArgument>(LogLevel level, IFormatProvider? formatProvider, [Localizable(false)][StructuredMessageTemplate] string message, TArgument? argument);

        /// <summary>
        /// Writes the diagnostic message at the specified level using the specified parameter.
        /// </summary>
        /// <typeparam name="TArgument">The type of the argument.</typeparam>
        /// <param name="level">The log level.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The argument to format.</param>
        [MessageTemplateFormatMethod("message")]
        void Log<TArgument>(LogLevel level, [Localizable(false)][StructuredMessageTemplate] string message, TArgument? argument);

        /// <summary>
        /// Writes the diagnostic message at the specified level using the specified arguments formatting it with the supplied format provider.
        /// </summary>
        /// <typeparam name="TArgument1">The type of the first argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the second argument.</typeparam>
        /// <param name="level">The log level.</param>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument1">The first argument to format.</param>
        /// <param name="argument2">The second argument to format.</param>
        [MessageTemplateFormatMethod("message")]
        void Log<TArgument1, TArgument2>(LogLevel level, IFormatProvider? formatProvider, [Localizable(false)][StructuredMessageTemplate] string message, TArgument1? argument1, TArgument2? argument2);

        /// <summary>
        /// Writes the diagnostic message at the specified level using the specified parameters.
        /// </summary>
        /// <typeparam name="TArgument1">The type of the first argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the second argument.</typeparam>
        /// <param name="level">The log level.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument1">The first argument to format.</param>
        /// <param name="argument2">The second argument to format.</param>
        [MessageTemplateFormatMethod("message")]
        void Log<TArgument1, TArgument2>(LogLevel level, [Localizable(false)][StructuredMessageTemplate] string message, TArgument1? argument1, TArgument2? argument2);

        /// <summary>
        /// Writes the diagnostic message at the specified level using the specified arguments formatting it with the supplied format provider.
        /// </summary>
        /// <typeparam name="TArgument1">The type of the first argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the second argument.</typeparam>
        /// <typeparam name="TArgument3">The type of the third argument.</typeparam>
        /// <param name="level">The log level.</param>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument1">The first argument to format.</param>
        /// <param name="argument2">The second argument to format.</param>
        /// <param name="argument3">The third argument to format.</param>
        [MessageTemplateFormatMethod("message")]
        void Log<TArgument1, TArgument2, TArgument3>(LogLevel level, IFormatProvider? formatProvider, [Localizable(false)][StructuredMessageTemplate] string message, TArgument1? argument1, TArgument2? argument2, TArgument3? argument3);

        /// <summary>
        /// Writes the diagnostic message at the specified level using the specified parameters.
        /// </summary>
        /// <typeparam name="TArgument1">The type of the first argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the second argument.</typeparam>
        /// <typeparam name="TArgument3">The type of the third argument.</typeparam>
        /// <param name="level">The log level.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument1">The first argument to format.</param>
        /// <param name="argument2">The second argument to format.</param>
        /// <param name="argument3">The third argument to format.</param>
        [MessageTemplateFormatMethod("message")]
        void Log<TArgument1, TArgument2, TArgument3>(LogLevel level, [Localizable(false)][StructuredMessageTemplate] string message, TArgument1? argument1, TArgument2? argument2, TArgument3? argument3);

        /// <summary>
        /// Obsolete and replaced by <see cref="Log(LogLevel, Exception, string, object[])"/> - Writes the diagnostic message and exception at the specified level.
        /// </summary>
        /// <param name="level">The log level.</param>
        /// <param name="message">A <see langword="string" /> to be written.</param>
        /// <param name="exception">An exception to be logged.</param>
        /// <remarks>This method was marked as obsolete before NLog 4.3.11 and it may be removed in a future release.</remarks>
        [Obsolete("Use Log(LogLevel level, Exception exception, [Localizable(false)] string message, params object[] args) instead. Marked obsolete with v4.3.11")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        void Log(LogLevel level, [Localizable(false)] string message, Exception? exception);

        /// <summary>
        /// Obsolete and replaced by <see cref="Log(LogLevel, Exception, string, object[])"/> - Writes the diagnostic message and exception at the specified level.
        /// </summary>
        /// <param name="level">The log level.</param>
        /// <param name="message">A <see langword="string" /> to be written.</param>
        /// <param name="exception">An exception to be logged.</param>
        /// <remarks>This method was marked as obsolete before NLog 4.3.11 and it may be removed in a future release.</remarks>
        [Obsolete("Use Log(LogLevel level, Exception exception, [Localizable(false)] string message, params object[] args) instead. Marked obsolete with v4.3.11")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        void LogException(LogLevel level, [Localizable(false)] string message, Exception? exception);
        #endregion
    }
}
