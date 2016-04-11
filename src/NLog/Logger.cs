// 
// Copyright (c) 2004-2016 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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
    using NLog.Internal;
    using JetBrains.Annotations;
#if ASYNC_SUPPORTED
    using System.Threading.Tasks;
#endif

    /// <summary>
    /// Provides logging interface and utility functions.
    /// </summary>
    [CLSCompliant(true)]
    public partial class Logger : ILogger
    {
        private readonly Type loggerType = typeof(Logger);

        private volatile LoggerConfiguration configuration;
        private volatile bool isTraceEnabled;
        private volatile bool isDebugEnabled;
        private volatile bool isInfoEnabled;
        private volatile bool isWarnEnabled;
        private volatile bool isErrorEnabled;
        private volatile bool isFatalEnabled;

        /// <summary>
        /// Initializes a new instance of the <see cref="Logger"/> class.
        /// </summary>
        protected internal Logger()
        {
        }

        /// <summary>
        /// Occurs when logger configuration changes.
        /// </summary>
        public event EventHandler<EventArgs> LoggerReconfigured;

        /// <summary>
        /// Gets the name of the logger.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the factory that created this logger.
        /// </summary>
        public LogFactory Factory { get; private set; }

        /// <summary>
        /// Gets a value indicating whether logging is enabled for the specified level.
        /// </summary>
        /// <param name="level">Log level to be checked.</param>
        /// <returns>A value of <see langword="true" /> if logging is enabled for the specified level, otherwise it returns <see langword="false" />.</returns>
        public bool IsEnabled(LogLevel level)
        {
            if (level == null)
            {
                throw new InvalidOperationException("Log level must be defined");
            }

            return this.GetTargetsForLevel(level) != null;
        }

        /// <summary>
        /// Writes the specified diagnostic message.
        /// </summary>
        /// <param name="logEvent">Log event.</param>
        public void Log(LogEventInfo logEvent)
        {
            if (this.IsEnabled(logEvent.Level))
            {
                this.WriteToTargets(logEvent);
            }
        }

        /// <summary>
        /// Writes the specified diagnostic message.
        /// </summary>
        /// <param name="wrapperType">The name of the type that wraps Logger.</param>
        /// <param name="logEvent">Log event.</param>
        public void Log(Type wrapperType, LogEventInfo logEvent)
        {
            if (this.IsEnabled(logEvent.Level))
            {
                this.WriteToTargets(wrapperType, logEvent);
            }
        }

        #region Log() overloads

        /// <overloads>
        /// Writes the diagnostic message at the specified level using the specified format provider and format parameters.
        /// </overloads>
        /// <summary>
        /// Writes the diagnostic message at the specified level.
        /// </summary>
        /// <typeparam name="T">Type of the value.</typeparam>
        /// <param name="level">The log level.</param>
        /// <param name="value">The value to be written.</param>
        public void Log<T>(LogLevel level, T value)
        {
            if (this.IsEnabled(level))
            {
                this.WriteToTargets(level, null, value);
            }
        }

        /// <summary>
        /// Writes the diagnostic message at the specified level.
        /// </summary>
        /// <typeparam name="T">Type of the value.</typeparam>
        /// <param name="level">The log level.</param>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="value">The value to be written.</param>
        public void Log<T>(LogLevel level, IFormatProvider formatProvider, T value)
        {
            if (this.IsEnabled(level))
            {
                this.WriteToTargets(level, formatProvider, value);
            }
        }

        /// <summary>
        /// Writes the diagnostic message at the specified level.
        /// </summary>
        /// <param name="level">The log level.</param>
        /// <param name="messageFunc">A function returning message to be written. Function is not evaluated if logging is not enabled.</param>
        public void Log(LogLevel level, LogMessageGenerator messageFunc)
        {
            if (this.IsEnabled(level))
            {
                if (messageFunc == null)
                {
                    throw new ArgumentNullException("messageFunc");
                }

                this.WriteToTargets(level, null, messageFunc());
            }
        }

        /// <summary>
        /// Writes the diagnostic message and exception at the specified level.
        /// </summary>
        /// <param name="level">The log level.</param>
        /// <param name="message">A <see langword="string" /> to be written.</param>
        /// <param name="exception">An exception to be logged.</param>
        [Obsolete("Use Log(LogLevel, String, Exception) method instead.")]
        public void LogException(LogLevel level, [Localizable(false)] string message, Exception exception)
        {
            this.Log(level, message, exception);
        }

        /// <summary>
        /// Writes the diagnostic message at the specified level using the specified parameters and formatting them with the supplied format provider.
        /// </summary>
        /// <param name="level">The log level.</param>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing format items.</param>
        /// <param name="args">Arguments to format.</param>
        [StringFormatMethod("message")]
        public void Log(LogLevel level, IFormatProvider formatProvider, [Localizable(false)] string message, params object[] args)
        { 
            if (this.IsEnabled(level))
            {
                this.WriteToTargets(level, formatProvider, message, args); 
            }
        }

        /// <summary>
        /// Writes the diagnostic message at the specified level.
        /// </summary>
        /// <param name="level">The log level.</param>
        /// <param name="message">Log message.</param>
        public void Log(LogLevel level, [Localizable(false)] string message) 
        { 
            if (this.IsEnabled(level))
            {
                this.WriteToTargets(level, null, message);
            }
        }

        /// <summary>
        /// Writes the diagnostic message at the specified level using the specified parameters.
        /// </summary>
        /// <param name="level">The log level.</param>
        /// <param name="message">A <see langword="string" /> containing format items.</param>
        /// <param name="args">Arguments to format.</param>
        public void Log(LogLevel level, [Localizable(false)] string message, params object[] args) 
        { 
            if (this.IsEnabled(level))
            {
                this.WriteToTargets(level, message, args);
            }
        }

        /// <summary>
        /// Writes the diagnostic message and exception at the specified level.
        /// </summary>
        /// <param name="level">The log level.</param>
        /// <param name="message">A <see langword="string" /> to be written.</param>
        /// <param name="exception">An exception to be logged.</param>
        [Obsolete("Use Log(LogLevel level, Exception exception, [Localizable(false)] string message, params object[] args)")]
        public void Log(LogLevel level, [Localizable(false)] string message, Exception exception)
        {
            if (this.IsEnabled(level))
            {
                this.WriteToTargets(level, message, exception);
            }
        }

        /// <summary>
        /// Writes the diagnostic message and exception at the specified level.
        /// </summary>
        /// <param name="level">The log level.</param>
        /// <param name="message">A <see langword="string" /> to be written.</param>
        /// <param name="args">Arguments to format.</param>
        /// <param name="exception">An exception to be logged.</param>
        public void Log(LogLevel level, Exception exception, [Localizable(false)] string message, params object[] args)
        {
            if (this.IsEnabled(level))
            {
                this.WriteToTargets(level, exception, message, args);
            }
        }

        /// <summary>
        /// Writes the diagnostic message and exception at the specified level.
        /// </summary>
        /// <param name="level">The log level.</param>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> to be written.</param>
        /// <param name="args">Arguments to format.</param>
        /// <param name="exception">An exception to be logged.</param>
        public void Log(LogLevel level, Exception exception, IFormatProvider formatProvider, [Localizable(false)] string message, params object[] args)
        {
            if (this.IsEnabled(level))
            {
                this.WriteToTargets(level, exception, formatProvider, message, args);
            }
        }

        /// <summary>
        /// Writes the diagnostic message at the specified level using the specified parameter and formatting it with the supplied format provider.
        /// </summary>
        /// <typeparam name="TArgument">The type of the argument.</typeparam>
        /// <param name="level">The log level.</param>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The argument to format.</param>
        [StringFormatMethod("message")]
        public void Log<TArgument>(LogLevel level, IFormatProvider formatProvider, [Localizable(false)] string message, TArgument argument)
        { 
            if (this.IsEnabled(level))
            {
                this.WriteToTargets(level, formatProvider, message, new object[] { argument }); 
            }
        }

        /// <summary>
        /// Writes the diagnostic message at the specified level using the specified parameter.
        /// </summary>
        /// <typeparam name="TArgument">The type of the argument.</typeparam>
        /// <param name="level">The log level.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The argument to format.</param>
        [StringFormatMethod("message")]
        public void Log<TArgument>(LogLevel level, [Localizable(false)] string message, TArgument argument)
        {
            if (this.IsEnabled(level))
            {
                this.WriteToTargets(level, message, new object[] { argument });
            }
        }

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
        public void Log<TArgument1, TArgument2>(LogLevel level, IFormatProvider formatProvider, [Localizable(false)] string message, TArgument1 argument1, TArgument2 argument2) 
        { 
            if (this.IsEnabled(level))
            {
                this.WriteToTargets(level, formatProvider, message, new object[] { argument1, argument2 }); 
            }
        }

        /// <summary>
        /// Writes the diagnostic message at the specified level using the specified parameters.
        /// </summary>
        /// <typeparam name="TArgument1">The type of the first argument.</typeparam>
        /// <typeparam name="TArgument2">The type of the second argument.</typeparam>
        /// <param name="level">The log level.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument1">The first argument to format.</param>
        /// <param name="argument2">The second argument to format.</param>
        [StringFormatMethod("message")]
        public void Log<TArgument1, TArgument2>(LogLevel level, [Localizable(false)] string message, TArgument1 argument1, TArgument2 argument2)
        { 
            if (this.IsEnabled(level))
            {
                this.WriteToTargets(level, message, new object[] { argument1, argument2 });
            }
        }

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
        public void Log<TArgument1, TArgument2, TArgument3>(LogLevel level, IFormatProvider formatProvider, [Localizable(false)] string message, TArgument1 argument1, TArgument2 argument2, TArgument3 argument3) 
        { 
            if (this.IsEnabled(level))
            {
                this.WriteToTargets(level, formatProvider, message, new object[] { argument1, argument2, argument3 }); 
            }
        }

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
        [StringFormatMethod("message")]
        public void Log<TArgument1, TArgument2, TArgument3>(LogLevel level, [Localizable(false)] string message, TArgument1 argument1, TArgument2 argument2, TArgument3 argument3)
        { 
            if (this.IsEnabled(level))
            {
                this.WriteToTargets(level, message, new object[] { argument1, argument2, argument3 });
            }
        }

        internal void WriteToTargets(LogLevel level, Exception ex, [Localizable(false)] string message, object[] args)
        {
            LoggerImpl.Write(this.loggerType, this.GetTargetsForLevel(level), PrepareLogEventInfo(LogEventInfo.Create(level, this.Name, ex, this.Factory.DefaultCultureInfo, message, args)), this.Factory);
        }

        internal void WriteToTargets(LogLevel level, Exception ex, IFormatProvider formatProvider, [Localizable(false)] string message, object[] args)
        {
            LoggerImpl.Write(this.loggerType, this.GetTargetsForLevel(level), PrepareLogEventInfo(LogEventInfo.Create(level, this.Name, ex, formatProvider, message, args)), this.Factory);
        }


        private LogEventInfo PrepareLogEventInfo(LogEventInfo logEvent)
        {
            if (logEvent.FormatProvider == null)
            {
                logEvent.FormatProvider = this.Factory.DefaultCultureInfo;
            }
            return logEvent;

        }

        #endregion


        /// <summary>
        /// Runs the provided action. If the action throws, the exception is logged at <c>Error</c> level. The exception is not propagated outside of this method.
        /// </summary>
        /// <param name="action">Action to execute.</param>
        public void Swallow(Action action)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                Error(e);
            }
        }

        /// <summary>
        /// Runs the provided function and returns its result. If an exception is thrown, it is logged at <c>Error</c> level.
        /// The exception is not propagated outside of this method; a default value is returned instead.
        /// </summary>
        /// <typeparam name="T">Return type of the provided function.</typeparam>
        /// <param name="func">Function to run.</param>
        /// <returns>Result returned by the provided function or the default value of type <typeparamref name="T"/> in case of exception.</returns>
        public T Swallow<T>(Func<T> func)
        {
            return Swallow(func, default(T));
        }

        /// <summary>
        /// Runs the provided function and returns its result. If an exception is thrown, it is logged at <c>Error</c> level.
        /// The exception is not propagated outside of this method; a fallback value is returned instead.
        /// </summary>
        /// <typeparam name="T">Return type of the provided function.</typeparam>
        /// <param name="func">Function to run.</param>
        /// <param name="fallback">Fallback value to return in case of exception.</param>
        /// <returns>Result returned by the provided function or fallback value in case of exception.</returns>
        public T Swallow<T>(Func<T> func, T fallback)
        {
            try
            {
                return func();
            }
            catch (Exception e)
            {
                Error(e);
                return fallback;
            }
        }

#if ASYNC_SUPPORTED
        /// <summary>
        /// Logs an exception is logged at <c>Error</c> level if the provided task does not run to completion.
        /// </summary>
        /// <param name="task">The task for which to log an error if it does not run to completion.</param>
        /// <remarks>This method is useful in fire-and-forget situations, where application logic does not depend on completion of task. This method is avoids C# warning CS4014 in such situations.</remarks>
        public async void Swallow(Task task)
        {
            try
            {
                await task;
            }
            catch (Exception e)
            {
                Error(e);
            }
        }

        /// <summary>
        /// Returns a task that completes when a specified task to completes. If the task does not run to completion, an exception is logged at <c>Error</c> level. The returned task always runs to completion.
        /// </summary>
        /// <param name="task">The task for which to log an error if it does not run to completion.</param>
        /// <returns>A task that completes in the <see cref="TaskStatus.RanToCompletion"/> state when <paramref name="task"/> completes.</returns>
        public async Task SwallowAsync(Task task)
        {
            try
            {
                await task;
            }
            catch (Exception e)
            {
                Error(e);
            }
        }

        /// <summary>
        /// Runs async action. If the action throws, the exception is logged at <c>Error</c> level. The exception is not propagated outside of this method.
        /// </summary>
        /// <param name="asyncAction">Async action to execute.</param>
        public async Task SwallowAsync(Func<Task> asyncAction)
        {
            try
            {
                await asyncAction();
            }
            catch (Exception e)
            {
                Error(e);
            }
        }

        /// <summary>
        /// Runs the provided async function and returns its result. If the task does not run to completion, an exception is logged at <c>Error</c> level.
        /// The exception is not propagated outside of this method; a default value is returned instead.
        /// </summary>
        /// <typeparam name="TResult">Return type of the provided function.</typeparam>
        /// <param name="asyncFunc">Async function to run.</param>
        /// <returns>A task that represents the completion of the supplied task. If the supplied task ends in the <see cref="TaskStatus.RanToCompletion"/> state, the result of the new task will be the result of the supplied task; otherwise, the result of the new task will be the default value of type <typeparamref name="TResult"/>.</returns>
        public async Task<TResult> SwallowAsync<TResult>(Func<Task<TResult>> asyncFunc)
        {
            return await SwallowAsync(asyncFunc, default(TResult));
        }

        /// <summary>
        /// Runs the provided async function and returns its result. If the task does not run to completion, an exception is logged at <c>Error</c> level.
        /// The exception is not propagated outside of this method; a fallback value is returned instead.
        /// </summary>
        /// <typeparam name="TResult">Return type of the provided function.</typeparam>
        /// <param name="asyncFunc">Async function to run.</param>
        /// <param name="fallback">Fallback value to return if the task does not end in the <see cref="TaskStatus.RanToCompletion"/> state.</param>
        /// <returns>A task that represents the completion of the supplied task. If the supplied task ends in the <see cref="TaskStatus.RanToCompletion"/> state, the result of the new task will be the result of the supplied task; otherwise, the result of the new task will be the fallback value.</returns>
        public async Task<TResult> SwallowAsync<TResult>(Func<Task<TResult>> asyncFunc, TResult fallback)
        {
            try
            {
                return await asyncFunc();
            }
            catch (Exception e)
            {
                Error(e);
                return fallback;
            }
        }
#endif

        internal void Initialize(string name, LoggerConfiguration loggerConfiguration, LogFactory factory)
        {
            this.Name = name;
            this.Factory = factory;
            this.SetConfiguration(loggerConfiguration);
        }

        internal void WriteToTargets(LogLevel level, IFormatProvider formatProvider, [Localizable(false)] string message, object[] args)
        {
            LoggerImpl.Write(this.loggerType, this.GetTargetsForLevel(level), PrepareLogEventInfo(LogEventInfo.Create(level, this.Name, formatProvider, message, args)), this.Factory);
        }

        internal void WriteToTargets(LogLevel level, IFormatProvider formatProvider, [Localizable(false)] string message)
        {
            // please note that this overload calls the overload of LogEventInfo.Create with object[] parameter on purpose -
            // to avoid unnecessary string.Format (in case of calling Create(LogLevel, string, IFormatProvider, object))
            var logEvent = LogEventInfo.Create(level, this.Name, formatProvider, message, (object[])null);
            LoggerImpl.Write(this.loggerType, this.GetTargetsForLevel(level), PrepareLogEventInfo(logEvent), this.Factory);
        }

        internal void WriteToTargets<T>(LogLevel level, IFormatProvider formatProvider, T value)
        {
            var logEvent = PrepareLogEventInfo(LogEventInfo.Create(level, this.Name, formatProvider, value));
            var ex = value as Exception;
            if (ex != null)
            {
                //also record exception
                logEvent.Exception = ex;
             
            }
            LoggerImpl.Write(this.loggerType, this.GetTargetsForLevel(level), logEvent, this.Factory);
        }

        [Obsolete("Use WriteToTargets(Exception ex, LogLevel level, IFormatProvider formatProvider, string message, object[] args) method instead.")]
        internal void WriteToTargets(LogLevel level, [Localizable(false)] string message, Exception ex)
        {
            LoggerImpl.Write(this.loggerType, this.GetTargetsForLevel(level), PrepareLogEventInfo(LogEventInfo.Create(level, this.Name, message, ex)), this.Factory);
        }


        internal void WriteToTargets(LogLevel level, [Localizable(false)] string message, object[] args)
        {
            this.WriteToTargets(level, this.Factory.DefaultCultureInfo, message, args);
        }

        internal void WriteToTargets(LogEventInfo logEvent)
        {
            LoggerImpl.Write(this.loggerType, this.GetTargetsForLevel(logEvent.Level), PrepareLogEventInfo(logEvent), this.Factory);
        }

        internal void WriteToTargets(Type wrapperType, LogEventInfo logEvent)
        {
            LoggerImpl.Write(wrapperType ?? this.loggerType, this.GetTargetsForLevel(logEvent.Level), PrepareLogEventInfo(logEvent), this.Factory);
        }

        internal void SetConfiguration(LoggerConfiguration newConfiguration)
        {
            this.configuration = newConfiguration;

            // pre-calculate 'enabled' flags
            this.isTraceEnabled = newConfiguration.IsEnabled(LogLevel.Trace);
            this.isDebugEnabled = newConfiguration.IsEnabled(LogLevel.Debug);
            this.isInfoEnabled = newConfiguration.IsEnabled(LogLevel.Info);
            this.isWarnEnabled = newConfiguration.IsEnabled(LogLevel.Warn);
            this.isErrorEnabled = newConfiguration.IsEnabled(LogLevel.Error);
            this.isFatalEnabled = newConfiguration.IsEnabled(LogLevel.Fatal);

            var loggerReconfiguredDelegate = this.LoggerReconfigured;

            if (loggerReconfiguredDelegate != null)
            {
                loggerReconfiguredDelegate(this, new EventArgs());
            }
        }

        private TargetWithFilterChain GetTargetsForLevel(LogLevel level)
        {
            return this.configuration.GetTargetsForLevel(level);
        }
    }
}
