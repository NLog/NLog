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
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using JetBrains.Annotations;

    /// <content>
    /// Logging methods which only are executed when the DEBUG conditional compilation symbol is set.
    ///
    /// Remarks:
    /// The DEBUG conditional compilation symbol is default enabled (only) in a debug build.
    ///
    /// If the DEBUG conditional compilation symbol isn't set in the calling library, the compiler will remove all the invocations to these methods.
    /// This could lead to better performance.
    ///
    /// See: https://msdn.microsoft.com/en-us/library/4xssyw96%28v=vs.90%29.aspx
    /// </content>
    public partial class Logger
    {
        #region ConditionalDebug

        /// <overloads>
        /// Writes the diagnostic message at the <c>Debug</c> level using the specified format provider and format parameters.
        /// </overloads>
        /// <summary>
        /// Writes the diagnostic message at the <c>Debug</c> level.
        /// Only executed when the DEBUG conditional compilation symbol is set.</summary>
        /// <typeparam name="T">Type of the value.</typeparam>
        /// <param name="value">The value to be written.</param>
        [Conditional("DEBUG")]
        public void ConditionalDebug<T>(T? value)
        {
            Debug(value);
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Debug</c> level.
        /// Only executed when the DEBUG conditional compilation symbol is set.</summary>
        /// <typeparam name="T">Type of the value.</typeparam>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="value">The value to be written.</param>
        [Conditional("DEBUG")]
        public void ConditionalDebug<T>(IFormatProvider? formatProvider, T? value)
        {
            Debug(formatProvider, value);
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Debug</c> level.
        /// Only executed when the DEBUG conditional compilation symbol is set.</summary>
        /// <param name="messageFunc">A function returning message to be written. Function is not evaluated if logging is not enabled.</param>
        [Conditional("DEBUG")]
        public void ConditionalDebug(LogMessageGenerator messageFunc)
        {
            Debug(messageFunc);
        }

        /// <summary>
        /// Writes the diagnostic message and exception at the <c>Debug</c> level.
        /// Only executed when the DEBUG conditional compilation symbol is set.</summary>
        /// <param name="message">A <see langword="string" /> to be written.</param>
        /// <param name="exception">An exception to be logged.</param>
        /// <param name="args">Arguments to format.</param>
        [Conditional("DEBUG")]
        [MessageTemplateFormatMethod("message")]
        public void ConditionalDebug(Exception? exception, [Localizable(false)][StructuredMessageTemplate] string message, params object?[] args)
        {
            Debug(exception, message, args);
        }

#if NETSTANDARD2_1_OR_GREATER || NET9_0_OR_GREATER
        /// <summary>
        /// Writes the diagnostic message and exception at the <c>Debug</c> level.
        /// Only executed when the DEBUG conditional compilation symbol is set.</summary>
        /// <param name="message">A <see langword="string" /> to be written.</param>
        /// <param name="exception">An exception to be logged.</param>
        /// <param name="args">Arguments to format.</param>
        [Conditional("DEBUG")]
        [MessageTemplateFormatMethod("message")]
        public void ConditionalDebug(Exception? exception, [Localizable(false)][StructuredMessageTemplate] string message, params ReadOnlySpan<object?> args)
        {
            Debug(exception, message, args);
        }
#endif

        /// <summary>
        /// Writes the diagnostic message and exception at the <c>Debug</c> level.
        /// Only executed when the DEBUG conditional compilation symbol is set.</summary>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> to be written.</param>
        /// <param name="exception">An exception to be logged.</param>
        /// <param name="args">Arguments to format.</param>
        [Conditional("DEBUG")]
        [MessageTemplateFormatMethod("message")]
        public void ConditionalDebug(Exception? exception, IFormatProvider? formatProvider, [Localizable(false)][StructuredMessageTemplate] string message, params object?[] args)
        {
            Debug(exception, formatProvider, message, args);
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Debug</c> level using the specified parameters and formatting them with the supplied format provider.
        /// Only executed when the DEBUG conditional compilation symbol is set.</summary>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing format items.</param>
        /// <param name="args">Arguments to format.</param>
        [Conditional("DEBUG")]
        [MessageTemplateFormatMethod("message")]
        public void ConditionalDebug(IFormatProvider? formatProvider, [Localizable(false)][StructuredMessageTemplate] string message, params object?[] args)
        {
            Debug(formatProvider, message, args);
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Debug</c> level.
        /// Only executed when the DEBUG conditional compilation symbol is set.</summary>
        /// <param name="message">Log message.</param>
        [Conditional("DEBUG")]
        public void ConditionalDebug([Localizable(false)] string message)
        {
            Debug(message);
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Debug</c> level using the specified parameters.
        /// Only executed when the DEBUG conditional compilation symbol is set.</summary>
        /// <param name="message">A <see langword="string" /> containing format items.</param>
        /// <param name="args">Arguments to format.</param>
        [Conditional("DEBUG")]
        [MessageTemplateFormatMethod("message")]
        public void ConditionalDebug([Localizable(false)][StructuredMessageTemplate] string message, params object?[] args)
        {
            Debug(message, args);
        }

#if NETSTANDARD2_1_OR_GREATER || NET9_0_OR_GREATER
        /// <summary>
        /// Writes the diagnostic message at the <c>Debug</c> level using the specified parameters.
        /// Only executed when the DEBUG conditional compilation symbol is set.</summary>
        /// <param name="message">A <see langword="string" /> containing format items.</param>
        /// <param name="args">Arguments to format.</param>
        [Conditional("DEBUG")]
        [MessageTemplateFormatMethod("message")]
        public void ConditionalDebug([Localizable(false)][StructuredMessageTemplate] string message, params ReadOnlySpan<object?> args)
        {
            Debug(message, args);
        }
#endif

        /// <summary>
        /// Writes the diagnostic message at the <c>Debug</c> level using the specified parameter and formatting it with the supplied format provider.
        /// Only executed when the DEBUG conditional compilation symbol is set.</summary>
        /// <typeparam name="TArgument">The type of the argument.</typeparam>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The argument to format.</param>
        [Conditional("DEBUG")]
        [MessageTemplateFormatMethod("message")]
        public void ConditionalDebug<TArgument>(IFormatProvider? formatProvider, [Localizable(false)][StructuredMessageTemplate] string message, TArgument? argument)
        {
            Debug(formatProvider, message, argument);
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Debug</c> level using the specified parameter.
        /// Only executed when the DEBUG conditional compilation symbol is set.</summary>
        /// <typeparam name="TArgument">The type of the argument.</typeparam>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The argument to format.</param>
        [Conditional("DEBUG")]
        [MessageTemplateFormatMethod("message")]
        public void ConditionalDebug<TArgument>([Localizable(false)][StructuredMessageTemplate] string message, TArgument? argument)
        {
            Debug(message, argument);
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Debug</c> level using the specified arguments formatting it with the supplied format provider.
        /// Only executed when the DEBUG conditional compilation symbol is set.</summary>
        /// <typeparam name="TArgument1">The type of the first argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the second argument.</typeparam>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument1">The first argument to format.</param>
        /// <param name="argument2">The second argument to format.</param>
        [Conditional("DEBUG")]
        [MessageTemplateFormatMethod("message")]
        public void ConditionalDebug<TArgument1, TArgument2>(IFormatProvider? formatProvider, [Localizable(false)][StructuredMessageTemplate] string message, TArgument1? argument1, TArgument2? argument2)
        {
            Debug(formatProvider, message, argument1, argument2);
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Debug</c> level using the specified parameters.
        /// Only executed when the DEBUG conditional compilation symbol is set.</summary>
        /// <typeparam name="TArgument1">The type of the first argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the second argument.</typeparam>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument1">The first argument to format.</param>
        /// <param name="argument2">The second argument to format.</param>
        [Conditional("DEBUG")]
        [MessageTemplateFormatMethod("message")]
        public void ConditionalDebug<TArgument1, TArgument2>([Localizable(false)][StructuredMessageTemplate] string message, TArgument1? argument1, TArgument2? argument2)
        {
            Debug(message, argument1, argument2);
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Debug</c> level using the specified arguments formatting it with the supplied format provider.
        /// Only executed when the DEBUG conditional compilation symbol is set.</summary>
        /// <typeparam name="TArgument1">The type of the first argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the second argument.</typeparam>
        /// <typeparam name="TArgument3">The type of the third argument.</typeparam>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument1">The first argument to format.</param>
        /// <param name="argument2">The second argument to format.</param>
        /// <param name="argument3">The third argument to format.</param>
        [Conditional("DEBUG")]
        [MessageTemplateFormatMethod("message")]
        public void ConditionalDebug<TArgument1, TArgument2, TArgument3>(IFormatProvider? formatProvider, [Localizable(false)][StructuredMessageTemplate] string message, TArgument1? argument1, TArgument2? argument2, TArgument3? argument3)
        {
            Debug(formatProvider, message, argument1, argument2, argument3);
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Debug</c> level using the specified parameters.
        /// Only executed when the DEBUG conditional compilation symbol is set.</summary>
        /// <typeparam name="TArgument1">The type of the first argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the second argument.</typeparam>
        /// <typeparam name="TArgument3">The type of the third argument.</typeparam>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument1">The first argument to format.</param>
        /// <param name="argument2">The second argument to format.</param>
        /// <param name="argument3">The third argument to format.</param>
        [Conditional("DEBUG")]
        [MessageTemplateFormatMethod("message")]
        public void ConditionalDebug<TArgument1, TArgument2, TArgument3>([Localizable(false)][StructuredMessageTemplate] string message, TArgument1? argument1, TArgument2? argument2, TArgument3? argument3)
        {
            Debug(message, argument1, argument2, argument3);
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Debug</c> level.
        /// Only executed when the DEBUG conditional compilation symbol is set.</summary>
        /// <param name="value">A <see langword="object" /> to be written.</param>
        [Conditional("DEBUG")]
        [EditorBrowsable(EditorBrowsableState.Never)]
#if NETSTANDARD2_1_OR_GREATER || NET9_0_OR_GREATER
        [OverloadResolutionPriority(-1)]
#endif
        public void ConditionalDebug(object? value)
        {
            Debug<object>(value);
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Debug</c> level.
        /// Only executed when the DEBUG conditional compilation symbol is set.</summary>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="value">A <see langword="object" /> to be written.</param>
        [Conditional("DEBUG")]
        [EditorBrowsable(EditorBrowsableState.Never)]
#if NETSTANDARD2_1_OR_GREATER || NET9_0_OR_GREATER
        [OverloadResolutionPriority(-1)]
#endif
        public void ConditionalDebug(IFormatProvider? formatProvider, object? value)
        {
            Debug<object>(formatProvider, value);
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Debug</c> level using the specified parameters.
        /// Only executed when the DEBUG conditional compilation symbol is set.</summary>
        /// <param name="message">A <see langword="string" /> containing format items.</param>
        /// <param name="arg1">First argument to format.</param>
        /// <param name="arg2">Second argument to format.</param>
        [Conditional("DEBUG")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [MessageTemplateFormatMethod("message")]
#if NETSTANDARD2_1_OR_GREATER || NET9_0_OR_GREATER
        [OverloadResolutionPriority(-1)]
#endif
        public void ConditionalDebug([Localizable(false)][StructuredMessageTemplate] string message, object? arg1, object? arg2)
        {
            Debug<object, object>(message, arg1, arg2);
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Debug</c> level using the specified parameters.
        /// Only executed when the DEBUG conditional compilation symbol is set.</summary>
        /// <param name="message">A <see langword="string" /> containing format items.</param>
        /// <param name="arg1">First argument to format.</param>
        /// <param name="arg2">Second argument to format.</param>
        /// <param name="arg3">Third argument to format.</param>
        [Conditional("DEBUG")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [MessageTemplateFormatMethod("message")]
#if NETSTANDARD2_1_OR_GREATER || NET9_0_OR_GREATER
        [OverloadResolutionPriority(-1)]
#endif
        public void ConditionalDebug([Localizable(false)][StructuredMessageTemplate] string message, object? arg1, object? arg2, object? arg3)
        {
            Debug<object, object, object>(message, arg1, arg2, arg3);
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Debug</c> level using the specified value as a parameter and formatting it with the supplied format provider.
        /// Only executed when the DEBUG conditional compilation symbol is set.</summary>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The argument to format.</param>
        [Conditional("DEBUG")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [MessageTemplateFormatMethod("message")]
#if NETSTANDARD2_1_OR_GREATER || NET9_0_OR_GREATER
        [OverloadResolutionPriority(-1)]
#endif
        public void ConditionalDebug(IFormatProvider? formatProvider, [Localizable(false)][StructuredMessageTemplate] string message, bool argument)
        {
            Debug<bool>(formatProvider, message, argument);
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Debug</c> level using the specified value as a parameter.
        /// Only executed when the DEBUG conditional compilation symbol is set.</summary>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The argument to format.</param>
        [Conditional("DEBUG")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [MessageTemplateFormatMethod("message")]
#if NETSTANDARD2_1_OR_GREATER || NET9_0_OR_GREATER
        [OverloadResolutionPriority(-1)]
#endif
        public void ConditionalDebug([Localizable(false)][StructuredMessageTemplate] string message, bool argument)
        {
            Debug<bool>(message, argument);
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Debug</c> level using the specified value as a parameter and formatting it with the supplied format provider.
        /// Only executed when the DEBUG conditional compilation symbol is set.</summary>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The argument to format.</param>
        [Conditional("DEBUG")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [MessageTemplateFormatMethod("message")]
#if NETSTANDARD2_1_OR_GREATER || NET9_0_OR_GREATER
        [OverloadResolutionPriority(-1)]
#endif
        public void ConditionalDebug(IFormatProvider? formatProvider, [Localizable(false)][StructuredMessageTemplate] string message, char argument)
        {
            Debug<char>(formatProvider, message, argument);
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Debug</c> level using the specified value as a parameter.
        /// Only executed when the DEBUG conditional compilation symbol is set.</summary>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The argument to format.</param>
        [Conditional("DEBUG")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [MessageTemplateFormatMethod("message")]
#if NETSTANDARD2_1_OR_GREATER || NET9_0_OR_GREATER
        [OverloadResolutionPriority(-1)]
#endif
        public void ConditionalDebug([Localizable(false)][StructuredMessageTemplate] string message, char argument)
        {
            Debug<char>(message, argument);
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Debug</c> level using the specified value as a parameter and formatting it with the supplied format provider.
        /// Only executed when the DEBUG conditional compilation symbol is set.</summary>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The argument to format.</param>
        [Conditional("DEBUG")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [MessageTemplateFormatMethod("message")]
#if NETSTANDARD2_1_OR_GREATER || NET9_0_OR_GREATER
        [OverloadResolutionPriority(-1)]
#endif
        public void ConditionalDebug(IFormatProvider? formatProvider, [Localizable(false)][StructuredMessageTemplate] string message, byte argument)
        {
            Debug<byte>(formatProvider, message, argument);
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Debug</c> level using the specified value as a parameter.
        /// Only executed when the DEBUG conditional compilation symbol is set.</summary>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The argument to format.</param>
        [Conditional("DEBUG")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [MessageTemplateFormatMethod("message")]
#if NETSTANDARD2_1_OR_GREATER || NET9_0_OR_GREATER
        [OverloadResolutionPriority(-1)]
#endif
        public void ConditionalDebug([Localizable(false)][StructuredMessageTemplate] string message, byte argument)
        {
            Debug<byte>(message, argument);
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Debug</c> level using the specified value as a parameter and formatting it with the supplied format provider.
        /// Only executed when the DEBUG conditional compilation symbol is set.</summary>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The argument to format.</param>
        [Conditional("DEBUG")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [MessageTemplateFormatMethod("message")]
#if NETSTANDARD2_1_OR_GREATER || NET9_0_OR_GREATER
        [OverloadResolutionPriority(-1)]
#endif
        public void ConditionalDebug(IFormatProvider? formatProvider, [Localizable(false)][StructuredMessageTemplate] string message, string? argument)
        {
            Debug<string>(formatProvider, message, argument);
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Debug</c> level using the specified value as a parameter.
        /// Only executed when the DEBUG conditional compilation symbol is set.</summary>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The argument to format.</param>
        [Conditional("DEBUG")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [MessageTemplateFormatMethod("message")]
#if NETSTANDARD2_1_OR_GREATER || NET9_0_OR_GREATER
        [OverloadResolutionPriority(-1)]
#endif
        public void ConditionalDebug([Localizable(false)][StructuredMessageTemplate] string message, string? argument)
        {
            Debug<string>(message, argument);
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Debug</c> level using the specified value as a parameter and formatting it with the supplied format provider.
        /// Only executed when the DEBUG conditional compilation symbol is set.</summary>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The argument to format.</param>
        [Conditional("DEBUG")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [MessageTemplateFormatMethod("message")]
#if NETSTANDARD2_1_OR_GREATER || NET9_0_OR_GREATER
        [OverloadResolutionPriority(-1)]
#endif
        public void ConditionalDebug(IFormatProvider? formatProvider, [Localizable(false)][StructuredMessageTemplate] string message, int argument)
        {
            Debug<int>(formatProvider, message, argument);
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Debug</c> level using the specified value as a parameter.
        /// Only executed when the DEBUG conditional compilation symbol is set.</summary>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The argument to format.</param>
        [Conditional("DEBUG")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [MessageTemplateFormatMethod("message")]
#if NETSTANDARD2_1_OR_GREATER || NET9_0_OR_GREATER
        [OverloadResolutionPriority(-1)]
#endif
        public void ConditionalDebug([Localizable(false)][StructuredMessageTemplate] string message, int argument)
        {
            Debug<int>(message, argument);
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Debug</c> level using the specified value as a parameter and formatting it with the supplied format provider.
        /// Only executed when the DEBUG conditional compilation symbol is set.</summary>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The argument to format.</param>
        [Conditional("DEBUG")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [MessageTemplateFormatMethod("message")]
#if NETSTANDARD2_1_OR_GREATER || NET9_0_OR_GREATER
        [OverloadResolutionPriority(-1)]
#endif
        public void ConditionalDebug(IFormatProvider? formatProvider, [Localizable(false)][StructuredMessageTemplate] string message, long argument)
        {
            Debug<long>(formatProvider, message, argument);
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Debug</c> level using the specified value as a parameter.
        /// Only executed when the DEBUG conditional compilation symbol is set.</summary>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The argument to format.</param>
        [Conditional("DEBUG")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [MessageTemplateFormatMethod("message")]
#if NETSTANDARD2_1_OR_GREATER || NET9_0_OR_GREATER
        [OverloadResolutionPriority(-1)]
#endif
        public void ConditionalDebug([Localizable(false)][StructuredMessageTemplate] string message, long argument)
        {
            Debug<long>(message, argument);
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Debug</c> level using the specified value as a parameter and formatting it with the supplied format provider.
        /// Only executed when the DEBUG conditional compilation symbol is set.</summary>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The argument to format.</param>
        [Conditional("DEBUG")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [MessageTemplateFormatMethod("message")]
#if NETSTANDARD2_1_OR_GREATER || NET9_0_OR_GREATER
        [OverloadResolutionPriority(-1)]
#endif
        public void ConditionalDebug(IFormatProvider? formatProvider, [Localizable(false)][StructuredMessageTemplate] string message, float argument)
        {
            Debug<float>(formatProvider, message, argument);
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Debug</c> level using the specified value as a parameter.
        /// Only executed when the DEBUG conditional compilation symbol is set.</summary>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The argument to format.</param>
        [Conditional("DEBUG")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [MessageTemplateFormatMethod("message")]
#if NETSTANDARD2_1_OR_GREATER || NET9_0_OR_GREATER
        [OverloadResolutionPriority(-1)]
#endif
        public void ConditionalDebug([Localizable(false)][StructuredMessageTemplate] string message, float argument)
        {
            Debug<float>(message, argument);
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Debug</c> level using the specified value as a parameter and formatting it with the supplied format provider.
        /// Only executed when the DEBUG conditional compilation symbol is set.</summary>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The argument to format.</param>
        [Conditional("DEBUG")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [MessageTemplateFormatMethod("message")]
#if NETSTANDARD2_1_OR_GREATER || NET9_0_OR_GREATER
        [OverloadResolutionPriority(-1)]
#endif
        public void ConditionalDebug(IFormatProvider? formatProvider, [Localizable(false)][StructuredMessageTemplate] string message, double argument)
        {
            Debug<double>(formatProvider, message, argument);
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Debug</c> level using the specified value as a parameter.
        /// Only executed when the DEBUG conditional compilation symbol is set.</summary>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The argument to format.</param>
        [Conditional("DEBUG")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [MessageTemplateFormatMethod("message")]
#if NETSTANDARD2_1_OR_GREATER || NET9_0_OR_GREATER
        [OverloadResolutionPriority(-1)]
#endif
        public void ConditionalDebug([Localizable(false)][StructuredMessageTemplate] string message, double argument)
        {
            Debug<double>(message, argument);
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Debug</c> level using the specified value as a parameter and formatting it with the supplied format provider.
        /// Only executed when the DEBUG conditional compilation symbol is set.</summary>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The argument to format.</param>
        [Conditional("DEBUG")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [MessageTemplateFormatMethod("message")]
#if NETSTANDARD2_1_OR_GREATER || NET9_0_OR_GREATER
        [OverloadResolutionPriority(-1)]
#endif
        public void ConditionalDebug(IFormatProvider? formatProvider, [Localizable(false)][StructuredMessageTemplate] string message, decimal argument)
        {
            Debug<decimal>(formatProvider, message, argument);
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Debug</c> level using the specified value as a parameter.
        /// Only executed when the DEBUG conditional compilation symbol is set.</summary>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The argument to format.</param>
        [Conditional("DEBUG")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [MessageTemplateFormatMethod("message")]
#if NETSTANDARD2_1_OR_GREATER || NET9_0_OR_GREATER
        [OverloadResolutionPriority(-1)]
#endif
        public void ConditionalDebug([Localizable(false)][StructuredMessageTemplate] string message, decimal argument)
        {
            Debug<decimal>(message, argument);
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Debug</c> level using the specified value as a parameter and formatting it with the supplied format provider.
        /// Only executed when the DEBUG conditional compilation symbol is set.</summary>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The argument to format.</param>
        [Conditional("DEBUG")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [MessageTemplateFormatMethod("message")]
#if NETSTANDARD2_1_OR_GREATER || NET9_0_OR_GREATER
        [OverloadResolutionPriority(-1)]
#endif
        public void ConditionalDebug(IFormatProvider? formatProvider, [Localizable(false)][StructuredMessageTemplate] string message, object? argument)
        {
            Debug<object>(formatProvider, message, argument);
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Debug</c> level using the specified value as a parameter.
        /// Only executed when the DEBUG conditional compilation symbol is set.</summary>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The argument to format.</param>
        [Conditional("DEBUG")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [MessageTemplateFormatMethod("message")]
#if NETSTANDARD2_1_OR_GREATER || NET9_0_OR_GREATER
        [OverloadResolutionPriority(-1)]
#endif
        public void ConditionalDebug([Localizable(false)][StructuredMessageTemplate] string message, object? argument)
        {
            Debug<object>(message, argument);
        }

        #endregion

        #region ConditionalTrace

        /// <overloads>
        /// Writes the diagnostic message at the <c>Trace</c> level using the specified format provider and format parameters.
        /// </overloads>
        /// <summary>
        /// Writes the diagnostic message at the <c>Trace</c> level.
        /// Only executed when the DEBUG conditional compilation symbol is set.</summary>
        /// <typeparam name="T">Type of the value.</typeparam>
        /// <param name="value">The value to be written.</param>
        [Conditional("DEBUG")]
        public void ConditionalTrace<T>(T? value)
        {
            Trace(value);
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Trace</c> level.
        /// Only executed when the DEBUG conditional compilation symbol is set.</summary>
        /// <typeparam name="T">Type of the value.</typeparam>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="value">The value to be written.</param>
        [Conditional("DEBUG")]
        public void ConditionalTrace<T>(IFormatProvider? formatProvider, T? value)
        {
            Trace(formatProvider, value);
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Trace</c> level.
        /// Only executed when the DEBUG conditional compilation symbol is set.</summary>
        /// <param name="messageFunc">A function returning message to be written. Function is not evaluated if logging is not enabled.</param>
        [Conditional("DEBUG")]
        public void ConditionalTrace(LogMessageGenerator messageFunc)
        {
            Trace(messageFunc);
        }

        /// <summary>
        /// Writes the diagnostic message and exception at the <c>Trace</c> level.
        /// Only executed when the DEBUG conditional compilation symbol is set.</summary>
        /// <param name="message">A <see langword="string" /> to be written.</param>
        /// <param name="exception">An exception to be logged.</param>
        /// <param name="args">Arguments to format.</param>
        [Conditional("DEBUG")]
        [MessageTemplateFormatMethod("message")]
        public void ConditionalTrace(Exception? exception, [Localizable(false)][StructuredMessageTemplate] string message, params object?[] args)
        {
            Trace(exception, message, args);
        }

#if NETSTANDARD2_1_OR_GREATER || NET9_0_OR_GREATER
        /// <summary>
        /// Writes the diagnostic message and exception at the <c>Trace</c> level.
        /// Only executed when the DEBUG conditional compilation symbol is set.</summary>
        /// <param name="message">A <see langword="string" /> to be written.</param>
        /// <param name="exception">An exception to be logged.</param>
        /// <param name="args">Arguments to format.</param>
        [Conditional("DEBUG")]
        [MessageTemplateFormatMethod("message")]
        public void ConditionalTrace(Exception? exception, [Localizable(false)][StructuredMessageTemplate] string message, params ReadOnlySpan<object?> args)
        {
            Trace(exception, message, args);
        }
#endif

        /// <summary>
        /// Writes the diagnostic message and exception at the <c>Trace</c> level.
        /// Only executed when the DEBUG conditional compilation symbol is set.</summary>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> to be written.</param>
        /// <param name="exception">An exception to be logged.</param>
        /// <param name="args">Arguments to format.</param>
        [Conditional("DEBUG")]
        [MessageTemplateFormatMethod("message")]
        public void ConditionalTrace(Exception? exception, IFormatProvider? formatProvider, [Localizable(false)][StructuredMessageTemplate] string message, params object?[] args)
        {
            Trace(exception, formatProvider, message, args);
        }


        /// <summary>
        /// Writes the diagnostic message at the <c>Trace</c> level using the specified parameters and formatting them with the supplied format provider.
        /// Only executed when the DEBUG conditional compilation symbol is set.</summary>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing format items.</param>
        /// <param name="args">Arguments to format.</param>
        [Conditional("DEBUG")]
        [MessageTemplateFormatMethod("message")]
        public void ConditionalTrace(IFormatProvider? formatProvider, [Localizable(false)][StructuredMessageTemplate] string message, params object?[] args)
        {
            Trace(formatProvider, message, args);
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Trace</c> level.
        /// Only executed when the DEBUG conditional compilation symbol is set.</summary>
        /// <param name="message">Log message.</param>
        [Conditional("DEBUG")]
        public void ConditionalTrace([Localizable(false)] string message)
        {
            Trace(message);
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Trace</c> level using the specified parameters.
        /// Only executed when the DEBUG conditional compilation symbol is set.</summary>
        /// <param name="message">A <see langword="string" /> containing format items.</param>
        /// <param name="args">Arguments to format.</param>
        [Conditional("DEBUG")]
        [MessageTemplateFormatMethod("message")]
        public void ConditionalTrace([Localizable(false)][StructuredMessageTemplate] string message, params object?[] args)
        {
            Trace(message, args);
        }

#if NETSTANDARD2_1_OR_GREATER || NET9_0_OR_GREATER
        /// <summary>
        /// Writes the diagnostic message at the <c>Trace</c> level using the specified parameters.
        /// Only executed when the DEBUG conditional compilation symbol is set.</summary>
        /// <param name="message">A <see langword="string" /> containing format items.</param>
        /// <param name="args">Arguments to format.</param>
        [Conditional("DEBUG")]
        [MessageTemplateFormatMethod("message")]
        public void ConditionalTrace([Localizable(false)][StructuredMessageTemplate] string message, params ReadOnlySpan<object?> args)
        {
            Trace(message, args);
        }
#endif

        /// <summary>
        /// Writes the diagnostic message at the <c>Trace</c> level using the specified parameter and formatting it with the supplied format provider.
        /// Only executed when the DEBUG conditional compilation symbol is set.</summary>
        /// <typeparam name="TArgument">The type of the argument.</typeparam>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The argument to format.</param>
        [Conditional("DEBUG")]
        [MessageTemplateFormatMethod("message")]
        public void ConditionalTrace<TArgument>(IFormatProvider? formatProvider, [Localizable(false)][StructuredMessageTemplate] string message, TArgument? argument)
        {
            Trace(formatProvider, message, argument);
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Trace</c> level using the specified parameter.
        /// Only executed when the DEBUG conditional compilation symbol is set.</summary>
        /// <typeparam name="TArgument">The type of the argument.</typeparam>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The argument to format.</param>
        [Conditional("DEBUG")]
        [MessageTemplateFormatMethod("message")]
        public void ConditionalTrace<TArgument>([Localizable(false)][StructuredMessageTemplate] string message, TArgument? argument)
        {
            Trace(message, argument);
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Trace</c> level using the specified arguments formatting it with the supplied format provider.
        /// Only executed when the DEBUG conditional compilation symbol is set.</summary>
        /// <typeparam name="TArgument1">The type of the first argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the second argument.</typeparam>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument1">The first argument to format.</param>
        /// <param name="argument2">The second argument to format.</param>
        [Conditional("DEBUG")]
        [MessageTemplateFormatMethod("message")]
        public void ConditionalTrace<TArgument1, TArgument2>(IFormatProvider? formatProvider, [Localizable(false)][StructuredMessageTemplate] string message, TArgument1? argument1, TArgument2? argument2)
        {
            Trace(formatProvider, message, argument1, argument2);
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Trace</c> level using the specified parameters.
        /// Only executed when the DEBUG conditional compilation symbol is set.</summary>
        /// <typeparam name="TArgument1">The type of the first argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the second argument.</typeparam>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument1">The first argument to format.</param>
        /// <param name="argument2">The second argument to format.</param>
        [Conditional("DEBUG")]
        [MessageTemplateFormatMethod("message")]
        public void ConditionalTrace<TArgument1, TArgument2>([Localizable(false)][StructuredMessageTemplate] string message, TArgument1? argument1, TArgument2? argument2)
        {
            Trace(message, argument1, argument2);
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Trace</c> level using the specified arguments formatting it with the supplied format provider.
        /// Only executed when the DEBUG conditional compilation symbol is set.</summary>
        /// <typeparam name="TArgument1">The type of the first argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the second argument.</typeparam>
        /// <typeparam name="TArgument3">The type of the third argument.</typeparam>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument1">The first argument to format.</param>
        /// <param name="argument2">The second argument to format.</param>
        /// <param name="argument3">The third argument to format.</param>
        [Conditional("DEBUG")]
        [MessageTemplateFormatMethod("message")]
        public void ConditionalTrace<TArgument1, TArgument2, TArgument3>(IFormatProvider? formatProvider, [Localizable(false)][StructuredMessageTemplate] string message, TArgument1? argument1, TArgument2? argument2, TArgument3? argument3)
        {
            Trace(formatProvider, message, argument1, argument2, argument3);
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Trace</c> level using the specified parameters.
        /// Only executed when the DEBUG conditional compilation symbol is set.</summary>
        /// <typeparam name="TArgument1">The type of the first argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the second argument.</typeparam>
        /// <typeparam name="TArgument3">The type of the third argument.</typeparam>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument1">The first argument to format.</param>
        /// <param name="argument2">The second argument to format.</param>
        /// <param name="argument3">The third argument to format.</param>
        [Conditional("DEBUG")]
        [MessageTemplateFormatMethod("message")]
        public void ConditionalTrace<TArgument1, TArgument2, TArgument3>([Localizable(false)][StructuredMessageTemplate] string message, TArgument1? argument1, TArgument2? argument2, TArgument3? argument3)
        {
            Trace(message, argument1, argument2, argument3);
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Trace</c> level.
        /// Only executed when the DEBUG conditional compilation symbol is set.</summary>
        /// <param name="value">A <see langword="object" /> to be written.</param>
        [Conditional("DEBUG")]
        [EditorBrowsable(EditorBrowsableState.Never)]
#if NETSTANDARD2_1_OR_GREATER || NET9_0_OR_GREATER
        [OverloadResolutionPriority(-1)]
#endif
        public void ConditionalTrace(object value)
        {
            Trace<object>(value);
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Trace</c> level.
        /// Only executed when the DEBUG conditional compilation symbol is set.</summary>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="value">A <see langword="object" /> to be written.</param>
        [Conditional("DEBUG")]
        [EditorBrowsable(EditorBrowsableState.Never)]
#if NETSTANDARD2_1_OR_GREATER || NET9_0_OR_GREATER
        [OverloadResolutionPriority(-1)]
#endif
        public void ConditionalTrace(IFormatProvider? formatProvider, object? value)
        {
            Trace<object>(formatProvider, value);
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Trace</c> level using the specified parameters.
        /// Only executed when the DEBUG conditional compilation symbol is set.</summary>
        /// <param name="message">A <see langword="string" /> containing format items.</param>
        /// <param name="arg1">First argument to format.</param>
        /// <param name="arg2">Second argument to format.</param>
        [Conditional("DEBUG")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [MessageTemplateFormatMethod("message")]
#if NETSTANDARD2_1_OR_GREATER || NET9_0_OR_GREATER
        [OverloadResolutionPriority(-1)]
#endif
        public void ConditionalTrace([Localizable(false)][StructuredMessageTemplate] string message, object? arg1, object? arg2)
        {
            Trace<object, object>(message, arg1, arg2);
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Trace</c> level using the specified parameters.
        /// Only executed when the DEBUG conditional compilation symbol is set.</summary>
        /// <param name="message">A <see langword="string" /> containing format items.</param>
        /// <param name="arg1">First argument to format.</param>
        /// <param name="arg2">Second argument to format.</param>
        /// <param name="arg3">Third argument to format.</param>
        [Conditional("DEBUG")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [MessageTemplateFormatMethod("message")]
#if NETSTANDARD2_1_OR_GREATER || NET9_0_OR_GREATER
        [OverloadResolutionPriority(-1)]
#endif
        public void ConditionalTrace([Localizable(false)][StructuredMessageTemplate] string message, object? arg1, object? arg2, object? arg3)
        {
            Trace<object, object, object>(message, arg1, arg2, arg3);
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Trace</c> level using the specified value as a parameter and formatting it with the supplied format provider.
        /// Only executed when the DEBUG conditional compilation symbol is set.</summary>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The argument to format.</param>
        [Conditional("DEBUG")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [MessageTemplateFormatMethod("message")]
#if NETSTANDARD2_1_OR_GREATER || NET9_0_OR_GREATER
        [OverloadResolutionPriority(-1)]
#endif
        public void ConditionalTrace(IFormatProvider? formatProvider, [Localizable(false)][StructuredMessageTemplate] string message, bool argument)
        {
            Trace<bool>(formatProvider, message, argument);
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Trace</c> level using the specified value as a parameter.
        /// Only executed when the DEBUG conditional compilation symbol is set.</summary>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The argument to format.</param>
        [Conditional("DEBUG")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [MessageTemplateFormatMethod("message")]
#if NETSTANDARD2_1_OR_GREATER || NET9_0_OR_GREATER
        [OverloadResolutionPriority(-1)]
#endif
        public void ConditionalTrace([Localizable(false)][StructuredMessageTemplate] string message, bool argument)
        {
            Trace<bool>(message, argument);
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Trace</c> level using the specified value as a parameter and formatting it with the supplied format provider.
        /// Only executed when the DEBUG conditional compilation symbol is set.</summary>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The argument to format.</param>
        [Conditional("DEBUG")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [MessageTemplateFormatMethod("message")]
#if NETSTANDARD2_1_OR_GREATER || NET9_0_OR_GREATER
        [OverloadResolutionPriority(-1)]
#endif
        public void ConditionalTrace(IFormatProvider? formatProvider, [Localizable(false)][StructuredMessageTemplate] string message, char argument)
        {
            Trace<char>(formatProvider, message, argument);
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Trace</c> level using the specified value as a parameter.
        /// Only executed when the DEBUG conditional compilation symbol is set.</summary>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The argument to format.</param>
        [Conditional("DEBUG")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [MessageTemplateFormatMethod("message")]
#if NETSTANDARD2_1_OR_GREATER || NET9_0_OR_GREATER
        [OverloadResolutionPriority(-1)]
#endif
        public void ConditionalTrace([Localizable(false)][StructuredMessageTemplate] string message, char argument)
        {
            Trace<char>(message, argument);
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Trace</c> level using the specified value as a parameter and formatting it with the supplied format provider.
        /// Only executed when the DEBUG conditional compilation symbol is set.</summary>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The argument to format.</param>
        [Conditional("DEBUG")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [MessageTemplateFormatMethod("message")]
#if NETSTANDARD2_1_OR_GREATER || NET9_0_OR_GREATER
        [OverloadResolutionPriority(-1)]
#endif
        public void ConditionalTrace(IFormatProvider? formatProvider, [Localizable(false)][StructuredMessageTemplate] string message, byte argument)
        {
            Trace<byte>(formatProvider, message, argument);
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Trace</c> level using the specified value as a parameter.
        /// Only executed when the DEBUG conditional compilation symbol is set.</summary>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The argument to format.</param>
        [Conditional("DEBUG")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [MessageTemplateFormatMethod("message")]
#if NETSTANDARD2_1_OR_GREATER || NET9_0_OR_GREATER
        [OverloadResolutionPriority(-1)]
#endif
        public void ConditionalTrace([Localizable(false)][StructuredMessageTemplate] string message, byte argument)
        {
            Trace<byte>(message, argument);
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Trace</c> level using the specified value as a parameter and formatting it with the supplied format provider.
        /// Only executed when the DEBUG conditional compilation symbol is set.</summary>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The argument to format.</param>
        [Conditional("DEBUG")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [MessageTemplateFormatMethod("message")]
#if NETSTANDARD2_1_OR_GREATER || NET9_0_OR_GREATER
        [OverloadResolutionPriority(-1)]
#endif
        public void ConditionalTrace(IFormatProvider? formatProvider, [Localizable(false)][StructuredMessageTemplate] string message, string? argument)
        {
            Trace<string>(formatProvider, message, argument);
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Trace</c> level using the specified value as a parameter.
        /// Only executed when the DEBUG conditional compilation symbol is set.</summary>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The argument to format.</param>
        [Conditional("DEBUG")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [MessageTemplateFormatMethod("message")]
#if NETSTANDARD2_1_OR_GREATER || NET9_0_OR_GREATER
        [OverloadResolutionPriority(-1)]
#endif
        public void ConditionalTrace([Localizable(false)][StructuredMessageTemplate] string message, string? argument)
        {
            Trace<string>(message, argument);
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Trace</c> level using the specified value as a parameter and formatting it with the supplied format provider.
        /// Only executed when the DEBUG conditional compilation symbol is set.</summary>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The argument to format.</param>
        [Conditional("DEBUG")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [MessageTemplateFormatMethod("message")]
#if NETSTANDARD2_1_OR_GREATER || NET9_0_OR_GREATER
        [OverloadResolutionPriority(-1)]
#endif
        public void ConditionalTrace(IFormatProvider? formatProvider, [Localizable(false)][StructuredMessageTemplate] string message, int argument)
        {
            Trace<int>(formatProvider, message, argument);
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Trace</c> level using the specified value as a parameter.
        /// Only executed when the DEBUG conditional compilation symbol is set.</summary>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The argument to format.</param>
        [Conditional("DEBUG")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [MessageTemplateFormatMethod("message")]
#if NETSTANDARD2_1_OR_GREATER || NET9_0_OR_GREATER
        [OverloadResolutionPriority(-1)]
#endif
        public void ConditionalTrace([Localizable(false)][StructuredMessageTemplate] string message, int argument)
        {
            Trace<int>(message, argument);
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Trace</c> level using the specified value as a parameter and formatting it with the supplied format provider.
        /// Only executed when the DEBUG conditional compilation symbol is set.</summary>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The argument to format.</param>
        [Conditional("DEBUG")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [MessageTemplateFormatMethod("message")]
#if NETSTANDARD2_1_OR_GREATER || NET9_0_OR_GREATER
        [OverloadResolutionPriority(-1)]
#endif
        public void ConditionalTrace(IFormatProvider? formatProvider, [Localizable(false)][StructuredMessageTemplate] string message, long argument)
        {
            Trace<long>(formatProvider, message, argument);
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Trace</c> level using the specified value as a parameter.
        /// Only executed when the DEBUG conditional compilation symbol is set.</summary>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The argument to format.</param>
        [Conditional("DEBUG")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [MessageTemplateFormatMethod("message")]
#if NETSTANDARD2_1_OR_GREATER || NET9_0_OR_GREATER
        [OverloadResolutionPriority(-1)]
#endif
        public void ConditionalTrace([Localizable(false)][StructuredMessageTemplate] string message, long argument)
        {
            Trace<long>(message, argument);
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Trace</c> level using the specified value as a parameter and formatting it with the supplied format provider.
        /// Only executed when the DEBUG conditional compilation symbol is set.</summary>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The argument to format.</param>
        [Conditional("DEBUG")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [MessageTemplateFormatMethod("message")]
#if NETSTANDARD2_1_OR_GREATER || NET9_0_OR_GREATER
        [OverloadResolutionPriority(-1)]
#endif
        public void ConditionalTrace(IFormatProvider? formatProvider, [Localizable(false)][StructuredMessageTemplate] string message, float argument)
        {
            Trace<float>(formatProvider, message, argument);
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Trace</c> level using the specified value as a parameter.
        /// Only executed when the DEBUG conditional compilation symbol is set.</summary>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The argument to format.</param>
        [Conditional("DEBUG")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [MessageTemplateFormatMethod("message")]
#if NETSTANDARD2_1_OR_GREATER || NET9_0_OR_GREATER
        [OverloadResolutionPriority(-1)]
#endif
        public void ConditionalTrace([Localizable(false)][StructuredMessageTemplate] string message, float argument)
        {
            Trace<float>(message, argument);
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Trace</c> level using the specified value as a parameter and formatting it with the supplied format provider.
        /// Only executed when the DEBUG conditional compilation symbol is set.</summary>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The argument to format.</param>
        [Conditional("DEBUG")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [MessageTemplateFormatMethod("message")]
#if NETSTANDARD2_1_OR_GREATER || NET9_0_OR_GREATER
        [OverloadResolutionPriority(-1)]
#endif
        public void ConditionalTrace(IFormatProvider? formatProvider, [Localizable(false)][StructuredMessageTemplate] string message, double argument)
        {
            Trace<double>(formatProvider, message, argument);
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Trace</c> level using the specified value as a parameter.
        /// Only executed when the DEBUG conditional compilation symbol is set.</summary>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The argument to format.</param>
        [Conditional("DEBUG")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [MessageTemplateFormatMethod("message")]
#if NETSTANDARD2_1_OR_GREATER || NET9_0_OR_GREATER
        [OverloadResolutionPriority(-1)]
#endif
        public void ConditionalTrace([Localizable(false)][StructuredMessageTemplate] string message, double argument)
        {
            Trace<double>(message, argument);
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Trace</c> level using the specified value as a parameter and formatting it with the supplied format provider.
        /// Only executed when the DEBUG conditional compilation symbol is set.</summary>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The argument to format.</param>
        [Conditional("DEBUG")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [MessageTemplateFormatMethod("message")]
#if NETSTANDARD2_1_OR_GREATER || NET9_0_OR_GREATER
        [OverloadResolutionPriority(-1)]
#endif
        public void ConditionalTrace(IFormatProvider? formatProvider, [Localizable(false)][StructuredMessageTemplate] string message, decimal argument)
        {
            Trace<decimal>(formatProvider, message, argument);
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Trace</c> level using the specified value as a parameter.
        /// Only executed when the DEBUG conditional compilation symbol is set.</summary>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The argument to format.</param>
        [Conditional("DEBUG")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [MessageTemplateFormatMethod("message")]
#if NETSTANDARD2_1_OR_GREATER || NET9_0_OR_GREATER
        [OverloadResolutionPriority(-1)]
#endif
        public void ConditionalTrace([Localizable(false)][StructuredMessageTemplate] string message, decimal argument)
        {
            Trace<decimal>(message, argument);
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Trace</c> level using the specified value as a parameter and formatting it with the supplied format provider.
        /// Only executed when the DEBUG conditional compilation symbol is set.</summary>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The argument to format.</param>
        [Conditional("DEBUG")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [MessageTemplateFormatMethod("message")]
#if NETSTANDARD2_1_OR_GREATER || NET9_0_OR_GREATER
        [OverloadResolutionPriority(-1)]
#endif
        public void ConditionalTrace(IFormatProvider? formatProvider, [Localizable(false)][StructuredMessageTemplate] string message, object? argument)
        {
            Trace<object>(formatProvider, message, argument);
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Trace</c> level using the specified value as a parameter.
        /// Only executed when the DEBUG conditional compilation symbol is set.</summary>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The argument to format.</param>
        [Conditional("DEBUG")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [MessageTemplateFormatMethod("message")]
#if NETSTANDARD2_1_OR_GREATER || NET9_0_OR_GREATER
        [OverloadResolutionPriority(-1)]
#endif
        public void ConditionalTrace([Localizable(false)][StructuredMessageTemplate] string message, object? argument)
        {
            Trace<object>(message, argument);
        }


        #endregion
    }
}
