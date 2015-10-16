﻿// 
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

namespace NLog
{
    using System;
    using System.ComponentModel;
    using JetBrains.Annotations;

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
            get { return this.isTraceEnabled; }
        }

        /// <summary>
        /// Gets a value indicating whether logging is enabled for the <c>Debug</c> level.
        /// </summary>
        /// <returns>A value of <see langword="true" /> if logging is enabled for the <c>Debug</c> level, otherwise it returns <see langword="false" />.</returns>
        public bool IsDebugEnabled
        {
            get { return this.isDebugEnabled; }
        }

        /// <summary>
        /// Gets a value indicating whether logging is enabled for the <c>Info</c> level.
        /// </summary>
        /// <returns>A value of <see langword="true" /> if logging is enabled for the <c>Info</c> level, otherwise it returns <see langword="false" />.</returns>
        public bool IsInfoEnabled
        {
            get { return this.isInfoEnabled; }
        }

        /// <summary>
        /// Gets a value indicating whether logging is enabled for the <c>Warn</c> level.
        /// </summary>
        /// <returns>A value of <see langword="true" /> if logging is enabled for the <c>Warn</c> level, otherwise it returns <see langword="false" />.</returns>
        public bool IsWarnEnabled
        {
            get { return this.isWarnEnabled; }
        }

        /// <summary>
        /// Gets a value indicating whether logging is enabled for the <c>Error</c> level.
        /// </summary>
        /// <returns>A value of <see langword="true" /> if logging is enabled for the <c>Error</c> level, otherwise it returns <see langword="false" />.</returns>
        public bool IsErrorEnabled
        {
            get { return this.isErrorEnabled; }
        }

        /// <summary>
        /// Gets a value indicating whether logging is enabled for the <c>Fatal</c> level.
        /// </summary>
        /// <returns>A value of <see langword="true" /> if logging is enabled for the <c>Fatal</c> level, otherwise it returns <see langword="false" />.</returns>
        public bool IsFatalEnabled
        {
            get { return this.isFatalEnabled; }
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
        public void Trace<T>(T value)
        {
            if (this.IsTraceEnabled)
            {
                this.WriteToTargets(LogLevel.Trace, null, value);
            }
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Trace</c> level.
        /// </summary>
        /// <typeparam name="T">Type of the value.</typeparam>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="value">The value to be written.</param>
        public void Trace<T>(IFormatProvider formatProvider, T value)
        {
            if (this.IsTraceEnabled)
            {
                this.WriteToTargets(LogLevel.Trace, formatProvider, value);
            }
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Trace</c> level.
        /// </summary>
        /// <param name="messageFunc">A function returning message to be written. Function is not evaluated if logging is not enabled.</param>
        public void Trace(LogMessageGenerator messageFunc)
        {
            if (this.IsTraceEnabled)
            {
                if (messageFunc == null)
                {
                    throw new ArgumentNullException("messageFunc");
                }

                this.WriteToTargets(LogLevel.Trace, null, messageFunc());
            }
        }

        /// <summary>
        /// Writes the diagnostic message and exception at the <c>Trace</c> level.
        /// </summary>
        /// <param name="message">A <see langword="string" /> to be written.</param>
        /// <param name="exception">An exception to be logged.</param>
        [Obsolete("Use Trace(Exception exception, string message, params object[] args) method instead.")]
        public void TraceException([Localizable(false)] string message, Exception exception)
        {
            this.Trace(message, exception); 
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Trace</c> level using the specified parameters and formatting them with the supplied format provider.
        /// </summary>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing format items.</param>
        /// <param name="args">Arguments to format.</param>
        [StringFormatMethod("message")]
        public void Trace(IFormatProvider formatProvider, [Localizable(false)] string message, params object[] args)
        { 
            if (this.IsTraceEnabled)
            {
                this.WriteToTargets(LogLevel.Trace, formatProvider, message, args); 
            }
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Trace</c> level.
        /// </summary>
        /// <param name="message">Log message.</param>
        public void Trace([Localizable(false)] string message) 
        { 
            if (this.IsTraceEnabled)
            {
                this.WriteToTargets(LogLevel.Trace, null, message);
            }
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Trace</c> level using the specified parameters.
        /// </summary>
        /// <param name="message">A <see langword="string" /> containing format items.</param>
        /// <param name="args">Arguments to format.</param>
        [StringFormatMethod("message")]
        public void Trace([Localizable(false)] string message, params object[] args) 
        { 
            if (this.IsTraceEnabled)
            {
                this.WriteToTargets(LogLevel.Trace, message, args);
            }
        }

        /// <summary>
        /// Writes the diagnostic message and exception at the <c>Trace</c> level.
        /// </summary>
        /// <param name="message">A <see langword="string" /> to be written.</param>
        /// <param name="exception">An exception to be logged.</param>
        [Obsolete("Use Trace(Exception exception, string message, params object[] args) method instead.")]
        public void Trace([Localizable(false)] string message, Exception exception)
        {
            if (this.IsTraceEnabled)
            {
                this.WriteToTargets(LogLevel.Trace, message, exception);
            }
        }

        /// <summary>
        /// Writes the diagnostic message and exception at the <c>Trace</c> level.
        /// </summary>
        /// <param name="message">A <see langword="string" /> to be written.</param>
        /// <param name="exception">An exception to be logged.</param>
        /// <param name="args">Arguments to format.</param>
        public void Trace(Exception exception, [Localizable(false)] string message, params object[] args)
        {
            if (this.IsTraceEnabled)
            {
                this.WriteToTargets(LogLevel.Trace, exception, message, args);
            }
        }

        /// <summary>
        /// Writes the diagnostic message and exception at the <c>Trace</c> level.
        /// </summary>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> to be written.</param>
        /// <param name="exception">An exception to be logged.</param>
        /// <param name="args">Arguments to format.</param>
        [StringFormatMethod("message")]
        public void Trace(Exception exception, IFormatProvider formatProvider, [Localizable(false)] string message, params object[] args)
        {
            if (this.IsTraceEnabled)
            {
                this.WriteToTargets(LogLevel.Trace, exception, formatProvider, message, args);
            }
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Trace</c> level using the specified parameter and formatting it with the supplied format provider.
        /// </summary>
        /// <typeparam name="TArgument">The type of the argument.</typeparam>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The argument to format.</param>
        [StringFormatMethod("message")]
        public void Trace<TArgument>(IFormatProvider formatProvider, [Localizable(false)] string message, TArgument argument)
        { 
            if (this.IsTraceEnabled)
            {
                this.WriteToTargets(LogLevel.Trace, formatProvider, message, new object[] { argument }); 
            }
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Trace</c> level using the specified parameter.
        /// </summary>
        /// <typeparam name="TArgument">The type of the argument.</typeparam>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The argument to format.</param>
        [StringFormatMethod("message")]
        public void Trace<TArgument>([Localizable(false)] string message, TArgument argument)
        { 
            if (this.IsTraceEnabled)
            {
#pragma warning disable 618
           
            //todo log also these calls as warning?
                if (this.configuration.ExceptionLoggingOldStyle)
#pragma warning restore 618
                {   
                    var exceptionCandidate = argument as Exception;		
                    if (exceptionCandidate != null)		
                    {

                        // ReSharper disable CSharpWarnings::CS0618
                        #pragma warning disable 618
                        this.Trace(message, exceptionCandidate);	
                        #pragma warning restore 618
                        // ReSharper restore CSharpWarnings::CS0618	
                        return;		
                    }
                }

                this.WriteToTargets(LogLevel.Trace, message, new object[] { argument });
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
        [StringFormatMethod("message")]
        public void Trace<TArgument1, TArgument2>(IFormatProvider formatProvider, [Localizable(false)] string message, TArgument1 argument1, TArgument2 argument2) 
        { 
            if (this.IsTraceEnabled)
            {
                this.WriteToTargets(LogLevel.Trace, formatProvider, message, new object[] { argument1, argument2 }); 
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
        [StringFormatMethod("message")]
        public void Trace<TArgument1, TArgument2>([Localizable(false)] string message, TArgument1 argument1, TArgument2 argument2)
        { 
            if (this.IsTraceEnabled)
            {
                this.WriteToTargets(LogLevel.Trace, message, new object[] { argument1, argument2 });
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
        [StringFormatMethod("message")]
        public void Trace<TArgument1, TArgument2, TArgument3>(IFormatProvider formatProvider, [Localizable(false)] string message, TArgument1 argument1, TArgument2 argument2, TArgument3 argument3) 
        { 
            if (this.IsTraceEnabled)
            {
                this.WriteToTargets(LogLevel.Trace, formatProvider, message, new object[] { argument1, argument2, argument3 }); 
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
        [StringFormatMethod("message")]
        public void Trace<TArgument1, TArgument2, TArgument3>([Localizable(false)] string message, TArgument1 argument1, TArgument2 argument2, TArgument3 argument3)
        { 
            if (this.IsTraceEnabled)
            {
                this.WriteToTargets(LogLevel.Trace, message, new object[] { argument1, argument2, argument3 });
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
        public void Debug<T>(T value)
        {
            if (this.IsDebugEnabled)
            {
                this.WriteToTargets(LogLevel.Debug, null, value);
            }
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Debug</c> level.
        /// </summary>
        /// <typeparam name="T">Type of the value.</typeparam>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="value">The value to be written.</param>
        public void Debug<T>(IFormatProvider formatProvider, T value)
        {
            if (this.IsDebugEnabled)
            {
                this.WriteToTargets(LogLevel.Debug, formatProvider, value);
            }
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Debug</c> level.
        /// </summary>
        /// <param name="messageFunc">A function returning message to be written. Function is not evaluated if logging is not enabled.</param>
        public void Debug(LogMessageGenerator messageFunc)
        {
            if (this.IsDebugEnabled)
            {
                if (messageFunc == null)
                {
                    throw new ArgumentNullException("messageFunc");
                }

                this.WriteToTargets(LogLevel.Debug, null, messageFunc());
            }
        }

        /// <summary>
        /// Writes the diagnostic message and exception at the <c>Debug</c> level.
        /// </summary>
        /// <param name="message">A <see langword="string" /> to be written.</param>
        /// <param name="exception">An exception to be logged.</param>
        [Obsolete("Use Debug(Exception exception, string message, params object[] args) method instead.")]
        public void DebugException([Localizable(false)] string message, Exception exception)
        {
            this.Debug(message, exception); 
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Debug</c> level using the specified parameters and formatting them with the supplied format provider.
        /// </summary>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing format items.</param>
        /// <param name="args">Arguments to format.</param>
        [StringFormatMethod("message")]
        public void Debug(IFormatProvider formatProvider, [Localizable(false)] string message, params object[] args)
        { 
            if (this.IsDebugEnabled)
            {
                this.WriteToTargets(LogLevel.Debug, formatProvider, message, args); 
            }
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Debug</c> level.
        /// </summary>
        /// <param name="message">Log message.</param>
        public void Debug([Localizable(false)] string message) 
        { 
            if (this.IsDebugEnabled)
            {
                this.WriteToTargets(LogLevel.Debug, null, message);
            }
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Debug</c> level using the specified parameters.
        /// </summary>
        /// <param name="message">A <see langword="string" /> containing format items.</param>
        /// <param name="args">Arguments to format.</param>
        [StringFormatMethod("message")]
        public void Debug([Localizable(false)] string message, params object[] args) 
        { 
            if (this.IsDebugEnabled)
            {
                this.WriteToTargets(LogLevel.Debug, message, args);
            }
        }

        /// <summary>
        /// Writes the diagnostic message and exception at the <c>Debug</c> level.
        /// </summary>
        /// <param name="message">A <see langword="string" /> to be written.</param>
        /// <param name="exception">An exception to be logged.</param>
        [Obsolete("Use Debug(Exception exception, string message, params object[] args) method instead.")]
        public void Debug([Localizable(false)] string message, Exception exception)
        {
            if (this.IsDebugEnabled)
            {
                this.WriteToTargets(LogLevel.Debug, message, exception);
            }
        }

        /// <summary>
        /// Writes the diagnostic message and exception at the <c>Debug</c> level.
        /// </summary>
        /// <param name="message">A <see langword="string" /> to be written.</param>
        /// <param name="exception">An exception to be logged.</param>
        /// <param name="args">Arguments to format.</param>
        public void Debug(Exception exception, [Localizable(false)] string message, params object[] args)
        {
            if (this.IsDebugEnabled)
            {
                this.WriteToTargets(LogLevel.Debug, exception, message, args);
            }
        }

        /// <summary>
        /// Writes the diagnostic message and exception at the <c>Debug</c> level.
        /// </summary>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> to be written.</param>
        /// <param name="exception">An exception to be logged.</param>
        /// <param name="args">Arguments to format.</param>
        [StringFormatMethod("message")]
        public void Debug(Exception exception, IFormatProvider formatProvider, [Localizable(false)] string message, params object[] args)
        {
            if (this.IsDebugEnabled)
            {
                this.WriteToTargets(LogLevel.Debug, exception, formatProvider, message, args);
            }
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Debug</c> level using the specified parameter and formatting it with the supplied format provider.
        /// </summary>
        /// <typeparam name="TArgument">The type of the argument.</typeparam>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The argument to format.</param>
        [StringFormatMethod("message")]
        public void Debug<TArgument>(IFormatProvider formatProvider, [Localizable(false)] string message, TArgument argument)
        { 
            if (this.IsDebugEnabled)
            {
                this.WriteToTargets(LogLevel.Debug, formatProvider, message, new object[] { argument }); 
            }
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Debug</c> level using the specified parameter.
        /// </summary>
        /// <typeparam name="TArgument">The type of the argument.</typeparam>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The argument to format.</param>
        [StringFormatMethod("message")]
        public void Debug<TArgument>([Localizable(false)] string message, TArgument argument)
        { 
            if (this.IsDebugEnabled)
            {
#pragma warning disable 618
           
            //todo log also these calls as warning?
                if (this.configuration.ExceptionLoggingOldStyle)
#pragma warning restore 618
                {   
                    var exceptionCandidate = argument as Exception;		
                    if (exceptionCandidate != null)		
                    {

                        // ReSharper disable CSharpWarnings::CS0618
                        #pragma warning disable 618
                        this.Debug(message, exceptionCandidate);	
                        #pragma warning restore 618
                        // ReSharper restore CSharpWarnings::CS0618	
                        return;		
                    }
                }

                this.WriteToTargets(LogLevel.Debug, message, new object[] { argument });
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
        [StringFormatMethod("message")]
        public void Debug<TArgument1, TArgument2>(IFormatProvider formatProvider, [Localizable(false)] string message, TArgument1 argument1, TArgument2 argument2) 
        { 
            if (this.IsDebugEnabled)
            {
                this.WriteToTargets(LogLevel.Debug, formatProvider, message, new object[] { argument1, argument2 }); 
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
        [StringFormatMethod("message")]
        public void Debug<TArgument1, TArgument2>([Localizable(false)] string message, TArgument1 argument1, TArgument2 argument2)
        { 
            if (this.IsDebugEnabled)
            {
                this.WriteToTargets(LogLevel.Debug, message, new object[] { argument1, argument2 });
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
        [StringFormatMethod("message")]
        public void Debug<TArgument1, TArgument2, TArgument3>(IFormatProvider formatProvider, [Localizable(false)] string message, TArgument1 argument1, TArgument2 argument2, TArgument3 argument3) 
        { 
            if (this.IsDebugEnabled)
            {
                this.WriteToTargets(LogLevel.Debug, formatProvider, message, new object[] { argument1, argument2, argument3 }); 
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
        [StringFormatMethod("message")]
        public void Debug<TArgument1, TArgument2, TArgument3>([Localizable(false)] string message, TArgument1 argument1, TArgument2 argument2, TArgument3 argument3)
        { 
            if (this.IsDebugEnabled)
            {
                this.WriteToTargets(LogLevel.Debug, message, new object[] { argument1, argument2, argument3 });
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
        public void Info<T>(T value)
        {
            if (this.IsInfoEnabled)
            {
                this.WriteToTargets(LogLevel.Info, null, value);
            }
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Info</c> level.
        /// </summary>
        /// <typeparam name="T">Type of the value.</typeparam>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="value">The value to be written.</param>
        public void Info<T>(IFormatProvider formatProvider, T value)
        {
            if (this.IsInfoEnabled)
            {
                this.WriteToTargets(LogLevel.Info, formatProvider, value);
            }
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Info</c> level.
        /// </summary>
        /// <param name="messageFunc">A function returning message to be written. Function is not evaluated if logging is not enabled.</param>
        public void Info(LogMessageGenerator messageFunc)
        {
            if (this.IsInfoEnabled)
            {
                if (messageFunc == null)
                {
                    throw new ArgumentNullException("messageFunc");
                }

                this.WriteToTargets(LogLevel.Info, null, messageFunc());
            }
        }

        /// <summary>
        /// Writes the diagnostic message and exception at the <c>Info</c> level.
        /// </summary>
        /// <param name="message">A <see langword="string" /> to be written.</param>
        /// <param name="exception">An exception to be logged.</param>
        [Obsolete("Use Info(Exception exception, string message, params object[] args) method instead.")]
        public void InfoException([Localizable(false)] string message, Exception exception)
        {
            this.Info(message, exception); 
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Info</c> level using the specified parameters and formatting them with the supplied format provider.
        /// </summary>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing format items.</param>
        /// <param name="args">Arguments to format.</param>
        [StringFormatMethod("message")]
        public void Info(IFormatProvider formatProvider, [Localizable(false)] string message, params object[] args)
        { 
            if (this.IsInfoEnabled)
            {
                this.WriteToTargets(LogLevel.Info, formatProvider, message, args); 
            }
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Info</c> level.
        /// </summary>
        /// <param name="message">Log message.</param>
        public void Info([Localizable(false)] string message) 
        { 
            if (this.IsInfoEnabled)
            {
                this.WriteToTargets(LogLevel.Info, null, message);
            }
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Info</c> level using the specified parameters.
        /// </summary>
        /// <param name="message">A <see langword="string" /> containing format items.</param>
        /// <param name="args">Arguments to format.</param>
        [StringFormatMethod("message")]
        public void Info([Localizable(false)] string message, params object[] args) 
        { 
            if (this.IsInfoEnabled)
            {
                this.WriteToTargets(LogLevel.Info, message, args);
            }
        }

        /// <summary>
        /// Writes the diagnostic message and exception at the <c>Info</c> level.
        /// </summary>
        /// <param name="message">A <see langword="string" /> to be written.</param>
        /// <param name="exception">An exception to be logged.</param>
        [Obsolete("Use Info(Exception exception, string message, params object[] args) method instead.")]
        public void Info([Localizable(false)] string message, Exception exception)
        {
            if (this.IsInfoEnabled)
            {
                this.WriteToTargets(LogLevel.Info, message, exception);
            }
        }

        /// <summary>
        /// Writes the diagnostic message and exception at the <c>Info</c> level.
        /// </summary>
        /// <param name="message">A <see langword="string" /> to be written.</param>
        /// <param name="exception">An exception to be logged.</param>
        /// <param name="args">Arguments to format.</param>
        public void Info(Exception exception, [Localizable(false)] string message, params object[] args)
        {
            if (this.IsInfoEnabled)
            {
                this.WriteToTargets(LogLevel.Info, exception, message, args);
            }
        }

        /// <summary>
        /// Writes the diagnostic message and exception at the <c>Info</c> level.
        /// </summary>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> to be written.</param>
        /// <param name="exception">An exception to be logged.</param>
        /// <param name="args">Arguments to format.</param>
        [StringFormatMethod("message")]
        public void Info(Exception exception, IFormatProvider formatProvider, [Localizable(false)] string message, params object[] args)
        {
            if (this.IsInfoEnabled)
            {
                this.WriteToTargets(LogLevel.Info, exception, formatProvider, message, args);
            }
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Info</c> level using the specified parameter and formatting it with the supplied format provider.
        /// </summary>
        /// <typeparam name="TArgument">The type of the argument.</typeparam>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The argument to format.</param>
        [StringFormatMethod("message")]
        public void Info<TArgument>(IFormatProvider formatProvider, [Localizable(false)] string message, TArgument argument)
        { 
            if (this.IsInfoEnabled)
            {
                this.WriteToTargets(LogLevel.Info, formatProvider, message, new object[] { argument }); 
            }
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Info</c> level using the specified parameter.
        /// </summary>
        /// <typeparam name="TArgument">The type of the argument.</typeparam>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The argument to format.</param>
        [StringFormatMethod("message")]
        public void Info<TArgument>([Localizable(false)] string message, TArgument argument)
        { 
            if (this.IsInfoEnabled)
            {
#pragma warning disable 618
           
            //todo log also these calls as warning?
                if (this.configuration.ExceptionLoggingOldStyle)
#pragma warning restore 618
                {   
                    var exceptionCandidate = argument as Exception;		
                    if (exceptionCandidate != null)		
                    {

                        // ReSharper disable CSharpWarnings::CS0618
                        #pragma warning disable 618
                        this.Info(message, exceptionCandidate);	
                        #pragma warning restore 618
                        // ReSharper restore CSharpWarnings::CS0618	
                        return;		
                    }
                }

                this.WriteToTargets(LogLevel.Info, message, new object[] { argument });
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
        [StringFormatMethod("message")]
        public void Info<TArgument1, TArgument2>(IFormatProvider formatProvider, [Localizable(false)] string message, TArgument1 argument1, TArgument2 argument2) 
        { 
            if (this.IsInfoEnabled)
            {
                this.WriteToTargets(LogLevel.Info, formatProvider, message, new object[] { argument1, argument2 }); 
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
        [StringFormatMethod("message")]
        public void Info<TArgument1, TArgument2>([Localizable(false)] string message, TArgument1 argument1, TArgument2 argument2)
        { 
            if (this.IsInfoEnabled)
            {
                this.WriteToTargets(LogLevel.Info, message, new object[] { argument1, argument2 });
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
        [StringFormatMethod("message")]
        public void Info<TArgument1, TArgument2, TArgument3>(IFormatProvider formatProvider, [Localizable(false)] string message, TArgument1 argument1, TArgument2 argument2, TArgument3 argument3) 
        { 
            if (this.IsInfoEnabled)
            {
                this.WriteToTargets(LogLevel.Info, formatProvider, message, new object[] { argument1, argument2, argument3 }); 
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
        [StringFormatMethod("message")]
        public void Info<TArgument1, TArgument2, TArgument3>([Localizable(false)] string message, TArgument1 argument1, TArgument2 argument2, TArgument3 argument3)
        { 
            if (this.IsInfoEnabled)
            {
                this.WriteToTargets(LogLevel.Info, message, new object[] { argument1, argument2, argument3 });
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
        public void Warn<T>(T value)
        {
            if (this.IsWarnEnabled)
            {
                this.WriteToTargets(LogLevel.Warn, null, value);
            }
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Warn</c> level.
        /// </summary>
        /// <typeparam name="T">Type of the value.</typeparam>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="value">The value to be written.</param>
        public void Warn<T>(IFormatProvider formatProvider, T value)
        {
            if (this.IsWarnEnabled)
            {
                this.WriteToTargets(LogLevel.Warn, formatProvider, value);
            }
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Warn</c> level.
        /// </summary>
        /// <param name="messageFunc">A function returning message to be written. Function is not evaluated if logging is not enabled.</param>
        public void Warn(LogMessageGenerator messageFunc)
        {
            if (this.IsWarnEnabled)
            {
                if (messageFunc == null)
                {
                    throw new ArgumentNullException("messageFunc");
                }

                this.WriteToTargets(LogLevel.Warn, null, messageFunc());
            }
        }

        /// <summary>
        /// Writes the diagnostic message and exception at the <c>Warn</c> level.
        /// </summary>
        /// <param name="message">A <see langword="string" /> to be written.</param>
        /// <param name="exception">An exception to be logged.</param>
        [Obsolete("Use Warn(Exception exception, string message, params object[] args) method instead.")]
        public void WarnException([Localizable(false)] string message, Exception exception)
        {
            this.Warn(message, exception); 
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Warn</c> level using the specified parameters and formatting them with the supplied format provider.
        /// </summary>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing format items.</param>
        /// <param name="args">Arguments to format.</param>
        [StringFormatMethod("message")]
        public void Warn(IFormatProvider formatProvider, [Localizable(false)] string message, params object[] args)
        { 
            if (this.IsWarnEnabled)
            {
                this.WriteToTargets(LogLevel.Warn, formatProvider, message, args); 
            }
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Warn</c> level.
        /// </summary>
        /// <param name="message">Log message.</param>
        public void Warn([Localizable(false)] string message) 
        { 
            if (this.IsWarnEnabled)
            {
                this.WriteToTargets(LogLevel.Warn, null, message);
            }
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Warn</c> level using the specified parameters.
        /// </summary>
        /// <param name="message">A <see langword="string" /> containing format items.</param>
        /// <param name="args">Arguments to format.</param>
        [StringFormatMethod("message")]
        public void Warn([Localizable(false)] string message, params object[] args) 
        { 
            if (this.IsWarnEnabled)
            {
                this.WriteToTargets(LogLevel.Warn, message, args);
            }
        }

        /// <summary>
        /// Writes the diagnostic message and exception at the <c>Warn</c> level.
        /// </summary>
        /// <param name="message">A <see langword="string" /> to be written.</param>
        /// <param name="exception">An exception to be logged.</param>
        [Obsolete("Use Warn(Exception exception, string message, params object[] args) method instead.")]
        public void Warn([Localizable(false)] string message, Exception exception)
        {
            if (this.IsWarnEnabled)
            {
                this.WriteToTargets(LogLevel.Warn, message, exception);
            }
        }

        /// <summary>
        /// Writes the diagnostic message and exception at the <c>Warn</c> level.
        /// </summary>
        /// <param name="message">A <see langword="string" /> to be written.</param>
        /// <param name="exception">An exception to be logged.</param>
        /// <param name="args">Arguments to format.</param>
        public void Warn(Exception exception, [Localizable(false)] string message, params object[] args)
        {
            if (this.IsWarnEnabled)
            {
                this.WriteToTargets(LogLevel.Warn, exception, message, args);
            }
        }

        /// <summary>
        /// Writes the diagnostic message and exception at the <c>Warn</c> level.
        /// </summary>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> to be written.</param>
        /// <param name="exception">An exception to be logged.</param>
        /// <param name="args">Arguments to format.</param>
        [StringFormatMethod("message")]
        public void Warn(Exception exception, IFormatProvider formatProvider, [Localizable(false)] string message, params object[] args)
        {
            if (this.IsWarnEnabled)
            {
                this.WriteToTargets(LogLevel.Warn, exception, formatProvider, message, args);
            }
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Warn</c> level using the specified parameter and formatting it with the supplied format provider.
        /// </summary>
        /// <typeparam name="TArgument">The type of the argument.</typeparam>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The argument to format.</param>
        [StringFormatMethod("message")]
        public void Warn<TArgument>(IFormatProvider formatProvider, [Localizable(false)] string message, TArgument argument)
        { 
            if (this.IsWarnEnabled)
            {
                this.WriteToTargets(LogLevel.Warn, formatProvider, message, new object[] { argument }); 
            }
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Warn</c> level using the specified parameter.
        /// </summary>
        /// <typeparam name="TArgument">The type of the argument.</typeparam>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The argument to format.</param>
        [StringFormatMethod("message")]
        public void Warn<TArgument>([Localizable(false)] string message, TArgument argument)
        { 
            if (this.IsWarnEnabled)
            {
#pragma warning disable 618
           
            //todo log also these calls as warning?
                if (this.configuration.ExceptionLoggingOldStyle)
#pragma warning restore 618
                {   
                    var exceptionCandidate = argument as Exception;		
                    if (exceptionCandidate != null)		
                    {

                        // ReSharper disable CSharpWarnings::CS0618
                        #pragma warning disable 618
                        this.Warn(message, exceptionCandidate);	
                        #pragma warning restore 618
                        // ReSharper restore CSharpWarnings::CS0618	
                        return;		
                    }
                }

                this.WriteToTargets(LogLevel.Warn, message, new object[] { argument });
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
        [StringFormatMethod("message")]
        public void Warn<TArgument1, TArgument2>(IFormatProvider formatProvider, [Localizable(false)] string message, TArgument1 argument1, TArgument2 argument2) 
        { 
            if (this.IsWarnEnabled)
            {
                this.WriteToTargets(LogLevel.Warn, formatProvider, message, new object[] { argument1, argument2 }); 
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
        [StringFormatMethod("message")]
        public void Warn<TArgument1, TArgument2>([Localizable(false)] string message, TArgument1 argument1, TArgument2 argument2)
        { 
            if (this.IsWarnEnabled)
            {
                this.WriteToTargets(LogLevel.Warn, message, new object[] { argument1, argument2 });
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
        [StringFormatMethod("message")]
        public void Warn<TArgument1, TArgument2, TArgument3>(IFormatProvider formatProvider, [Localizable(false)] string message, TArgument1 argument1, TArgument2 argument2, TArgument3 argument3) 
        { 
            if (this.IsWarnEnabled)
            {
                this.WriteToTargets(LogLevel.Warn, formatProvider, message, new object[] { argument1, argument2, argument3 }); 
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
        [StringFormatMethod("message")]
        public void Warn<TArgument1, TArgument2, TArgument3>([Localizable(false)] string message, TArgument1 argument1, TArgument2 argument2, TArgument3 argument3)
        { 
            if (this.IsWarnEnabled)
            {
                this.WriteToTargets(LogLevel.Warn, message, new object[] { argument1, argument2, argument3 });
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
        public void Error<T>(T value)
        {
            if (this.IsErrorEnabled)
            {
                this.WriteToTargets(LogLevel.Error, null, value);
            }
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Error</c> level.
        /// </summary>
        /// <typeparam name="T">Type of the value.</typeparam>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="value">The value to be written.</param>
        public void Error<T>(IFormatProvider formatProvider, T value)
        {
            if (this.IsErrorEnabled)
            {
                this.WriteToTargets(LogLevel.Error, formatProvider, value);
            }
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Error</c> level.
        /// </summary>
        /// <param name="messageFunc">A function returning message to be written. Function is not evaluated if logging is not enabled.</param>
        public void Error(LogMessageGenerator messageFunc)
        {
            if (this.IsErrorEnabled)
            {
                if (messageFunc == null)
                {
                    throw new ArgumentNullException("messageFunc");
                }

                this.WriteToTargets(LogLevel.Error, null, messageFunc());
            }
        }

        /// <summary>
        /// Writes the diagnostic message and exception at the <c>Error</c> level.
        /// </summary>
        /// <param name="message">A <see langword="string" /> to be written.</param>
        /// <param name="exception">An exception to be logged.</param>
        [Obsolete("Use Error(Exception exception, string message, params object[] args) method instead.")]
        public void ErrorException([Localizable(false)] string message, Exception exception)
        {
            this.Error(message, exception); 
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Error</c> level using the specified parameters and formatting them with the supplied format provider.
        /// </summary>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing format items.</param>
        /// <param name="args">Arguments to format.</param>
        [StringFormatMethod("message")]
        public void Error(IFormatProvider formatProvider, [Localizable(false)] string message, params object[] args)
        { 
            if (this.IsErrorEnabled)
            {
                this.WriteToTargets(LogLevel.Error, formatProvider, message, args); 
            }
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Error</c> level.
        /// </summary>
        /// <param name="message">Log message.</param>
        public void Error([Localizable(false)] string message) 
        { 
            if (this.IsErrorEnabled)
            {
                this.WriteToTargets(LogLevel.Error, null, message);
            }
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Error</c> level using the specified parameters.
        /// </summary>
        /// <param name="message">A <see langword="string" /> containing format items.</param>
        /// <param name="args">Arguments to format.</param>
        [StringFormatMethod("message")]
        public void Error([Localizable(false)] string message, params object[] args) 
        { 
            if (this.IsErrorEnabled)
            {
                this.WriteToTargets(LogLevel.Error, message, args);
            }
        }

        /// <summary>
        /// Writes the diagnostic message and exception at the <c>Error</c> level.
        /// </summary>
        /// <param name="message">A <see langword="string" /> to be written.</param>
        /// <param name="exception">An exception to be logged.</param>
        [Obsolete("Use Error(Exception exception, string message, params object[] args) method instead.")]
        public void Error([Localizable(false)] string message, Exception exception)
        {
            if (this.IsErrorEnabled)
            {
                this.WriteToTargets(LogLevel.Error, message, exception);
            }
        }

        /// <summary>
        /// Writes the diagnostic message and exception at the <c>Error</c> level.
        /// </summary>
        /// <param name="message">A <see langword="string" /> to be written.</param>
        /// <param name="exception">An exception to be logged.</param>
        /// <param name="args">Arguments to format.</param>
        public void Error(Exception exception, [Localizable(false)] string message, params object[] args)
        {
            if (this.IsErrorEnabled)
            {
                this.WriteToTargets(LogLevel.Error, exception, message, args);
            }
        }

        /// <summary>
        /// Writes the diagnostic message and exception at the <c>Error</c> level.
        /// </summary>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> to be written.</param>
        /// <param name="exception">An exception to be logged.</param>
        /// <param name="args">Arguments to format.</param>
        [StringFormatMethod("message")]
        public void Error(Exception exception, IFormatProvider formatProvider, [Localizable(false)] string message, params object[] args)
        {
            if (this.IsErrorEnabled)
            {
                this.WriteToTargets(LogLevel.Error, exception, formatProvider, message, args);
            }
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Error</c> level using the specified parameter and formatting it with the supplied format provider.
        /// </summary>
        /// <typeparam name="TArgument">The type of the argument.</typeparam>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The argument to format.</param>
        [StringFormatMethod("message")]
        public void Error<TArgument>(IFormatProvider formatProvider, [Localizable(false)] string message, TArgument argument)
        { 
            if (this.IsErrorEnabled)
            {
                this.WriteToTargets(LogLevel.Error, formatProvider, message, new object[] { argument }); 
            }
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Error</c> level using the specified parameter.
        /// </summary>
        /// <typeparam name="TArgument">The type of the argument.</typeparam>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The argument to format.</param>
        [StringFormatMethod("message")]
        public void Error<TArgument>([Localizable(false)] string message, TArgument argument)
        { 
            if (this.IsErrorEnabled)
            {
#pragma warning disable 618
           
            //todo log also these calls as warning?
                if (this.configuration.ExceptionLoggingOldStyle)
#pragma warning restore 618
                {   
                    var exceptionCandidate = argument as Exception;		
                    if (exceptionCandidate != null)		
                    {

                        // ReSharper disable CSharpWarnings::CS0618
                        #pragma warning disable 618
                        this.Error(message, exceptionCandidate);	
                        #pragma warning restore 618
                        // ReSharper restore CSharpWarnings::CS0618	
                        return;		
                    }
                }

                this.WriteToTargets(LogLevel.Error, message, new object[] { argument });
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
        [StringFormatMethod("message")]
        public void Error<TArgument1, TArgument2>(IFormatProvider formatProvider, [Localizable(false)] string message, TArgument1 argument1, TArgument2 argument2) 
        { 
            if (this.IsErrorEnabled)
            {
                this.WriteToTargets(LogLevel.Error, formatProvider, message, new object[] { argument1, argument2 }); 
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
        [StringFormatMethod("message")]
        public void Error<TArgument1, TArgument2>([Localizable(false)] string message, TArgument1 argument1, TArgument2 argument2)
        { 
            if (this.IsErrorEnabled)
            {
                this.WriteToTargets(LogLevel.Error, message, new object[] { argument1, argument2 });
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
        [StringFormatMethod("message")]
        public void Error<TArgument1, TArgument2, TArgument3>(IFormatProvider formatProvider, [Localizable(false)] string message, TArgument1 argument1, TArgument2 argument2, TArgument3 argument3) 
        { 
            if (this.IsErrorEnabled)
            {
                this.WriteToTargets(LogLevel.Error, formatProvider, message, new object[] { argument1, argument2, argument3 }); 
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
        [StringFormatMethod("message")]
        public void Error<TArgument1, TArgument2, TArgument3>([Localizable(false)] string message, TArgument1 argument1, TArgument2 argument2, TArgument3 argument3)
        { 
            if (this.IsErrorEnabled)
            {
                this.WriteToTargets(LogLevel.Error, message, new object[] { argument1, argument2, argument3 });
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
        public void Fatal<T>(T value)
        {
            if (this.IsFatalEnabled)
            {
                this.WriteToTargets(LogLevel.Fatal, null, value);
            }
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Fatal</c> level.
        /// </summary>
        /// <typeparam name="T">Type of the value.</typeparam>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="value">The value to be written.</param>
        public void Fatal<T>(IFormatProvider formatProvider, T value)
        {
            if (this.IsFatalEnabled)
            {
                this.WriteToTargets(LogLevel.Fatal, formatProvider, value);
            }
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Fatal</c> level.
        /// </summary>
        /// <param name="messageFunc">A function returning message to be written. Function is not evaluated if logging is not enabled.</param>
        public void Fatal(LogMessageGenerator messageFunc)
        {
            if (this.IsFatalEnabled)
            {
                if (messageFunc == null)
                {
                    throw new ArgumentNullException("messageFunc");
                }

                this.WriteToTargets(LogLevel.Fatal, null, messageFunc());
            }
        }

        /// <summary>
        /// Writes the diagnostic message and exception at the <c>Fatal</c> level.
        /// </summary>
        /// <param name="message">A <see langword="string" /> to be written.</param>
        /// <param name="exception">An exception to be logged.</param>
        [Obsolete("Use Fatal(Exception exception, string message, params object[] args) method instead.")]
        public void FatalException([Localizable(false)] string message, Exception exception)
        {
            this.Fatal(message, exception); 
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Fatal</c> level using the specified parameters and formatting them with the supplied format provider.
        /// </summary>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing format items.</param>
        /// <param name="args">Arguments to format.</param>
        [StringFormatMethod("message")]
        public void Fatal(IFormatProvider formatProvider, [Localizable(false)] string message, params object[] args)
        { 
            if (this.IsFatalEnabled)
            {
                this.WriteToTargets(LogLevel.Fatal, formatProvider, message, args); 
            }
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Fatal</c> level.
        /// </summary>
        /// <param name="message">Log message.</param>
        public void Fatal([Localizable(false)] string message) 
        { 
            if (this.IsFatalEnabled)
            {
                this.WriteToTargets(LogLevel.Fatal, null, message);
            }
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Fatal</c> level using the specified parameters.
        /// </summary>
        /// <param name="message">A <see langword="string" /> containing format items.</param>
        /// <param name="args">Arguments to format.</param>
        [StringFormatMethod("message")]
        public void Fatal([Localizable(false)] string message, params object[] args) 
        { 
            if (this.IsFatalEnabled)
            {
                this.WriteToTargets(LogLevel.Fatal, message, args);
            }
        }

        /// <summary>
        /// Writes the diagnostic message and exception at the <c>Fatal</c> level.
        /// </summary>
        /// <param name="message">A <see langword="string" /> to be written.</param>
        /// <param name="exception">An exception to be logged.</param>
        [Obsolete("Use Fatal(Exception exception, string message, params object[] args) method instead.")]
        public void Fatal([Localizable(false)] string message, Exception exception)
        {
            if (this.IsFatalEnabled)
            {
                this.WriteToTargets(LogLevel.Fatal, message, exception);
            }
        }

        /// <summary>
        /// Writes the diagnostic message and exception at the <c>Fatal</c> level.
        /// </summary>
        /// <param name="message">A <see langword="string" /> to be written.</param>
        /// <param name="exception">An exception to be logged.</param>
        /// <param name="args">Arguments to format.</param>
        public void Fatal(Exception exception, [Localizable(false)] string message, params object[] args)
        {
            if (this.IsFatalEnabled)
            {
                this.WriteToTargets(LogLevel.Fatal, exception, message, args);
            }
        }

        /// <summary>
        /// Writes the diagnostic message and exception at the <c>Fatal</c> level.
        /// </summary>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> to be written.</param>
        /// <param name="exception">An exception to be logged.</param>
        /// <param name="args">Arguments to format.</param>
        [StringFormatMethod("message")]
        public void Fatal(Exception exception, IFormatProvider formatProvider, [Localizable(false)] string message, params object[] args)
        {
            if (this.IsFatalEnabled)
            {
                this.WriteToTargets(LogLevel.Fatal, exception, formatProvider, message, args);
            }
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Fatal</c> level using the specified parameter and formatting it with the supplied format provider.
        /// </summary>
        /// <typeparam name="TArgument">The type of the argument.</typeparam>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The argument to format.</param>
        [StringFormatMethod("message")]
        public void Fatal<TArgument>(IFormatProvider formatProvider, [Localizable(false)] string message, TArgument argument)
        { 
            if (this.IsFatalEnabled)
            {
                this.WriteToTargets(LogLevel.Fatal, formatProvider, message, new object[] { argument }); 
            }
        }

        /// <summary>
        /// Writes the diagnostic message at the <c>Fatal</c> level using the specified parameter.
        /// </summary>
        /// <typeparam name="TArgument">The type of the argument.</typeparam>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The argument to format.</param>
        [StringFormatMethod("message")]
        public void Fatal<TArgument>([Localizable(false)] string message, TArgument argument)
        { 
            if (this.IsFatalEnabled)
            {
#pragma warning disable 618
           
            //todo log also these calls as warning?
                if (this.configuration.ExceptionLoggingOldStyle)
#pragma warning restore 618
                {   
                    var exceptionCandidate = argument as Exception;		
                    if (exceptionCandidate != null)		
                    {

                        // ReSharper disable CSharpWarnings::CS0618
                        #pragma warning disable 618
                        this.Fatal(message, exceptionCandidate);	
                        #pragma warning restore 618
                        // ReSharper restore CSharpWarnings::CS0618	
                        return;		
                    }
                }

                this.WriteToTargets(LogLevel.Fatal, message, new object[] { argument });
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
        [StringFormatMethod("message")]
        public void Fatal<TArgument1, TArgument2>(IFormatProvider formatProvider, [Localizable(false)] string message, TArgument1 argument1, TArgument2 argument2) 
        { 
            if (this.IsFatalEnabled)
            {
                this.WriteToTargets(LogLevel.Fatal, formatProvider, message, new object[] { argument1, argument2 }); 
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
        [StringFormatMethod("message")]
        public void Fatal<TArgument1, TArgument2>([Localizable(false)] string message, TArgument1 argument1, TArgument2 argument2)
        { 
            if (this.IsFatalEnabled)
            {
                this.WriteToTargets(LogLevel.Fatal, message, new object[] { argument1, argument2 });
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
        [StringFormatMethod("message")]
        public void Fatal<TArgument1, TArgument2, TArgument3>(IFormatProvider formatProvider, [Localizable(false)] string message, TArgument1 argument1, TArgument2 argument2, TArgument3 argument3) 
        { 
            if (this.IsFatalEnabled)
            {
                this.WriteToTargets(LogLevel.Fatal, formatProvider, message, new object[] { argument1, argument2, argument3 }); 
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
        [StringFormatMethod("message")]
        public void Fatal<TArgument1, TArgument2, TArgument3>([Localizable(false)] string message, TArgument1 argument1, TArgument2 argument2, TArgument3 argument3)
        { 
            if (this.IsFatalEnabled)
            {
                this.WriteToTargets(LogLevel.Fatal, message, new object[] { argument1, argument2, argument3 });
            }
        }

        #endregion
    }
}