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
    using System.Collections.Generic;
    using System.ComponentModel;
#if !NET35 && !NET40
    using System.Threading.Tasks;
#endif
    using JetBrains.Annotations;
    using NLog.Internal;

    /// <summary>
    /// Provides logging interface and utility functions.
    /// </summary>
    [CLSCompliant(true)]
    public partial class Logger : ILogger
    {
        internal static readonly Type DefaultLoggerType = typeof(Logger);
        private ITargetWithFilterChain[] _targetsByLevel = TargetWithFilterChain.NoTargetsByLevel;
        private Logger _contextLogger;
        private ThreadSafeDictionary<string, object> _contextProperties;
        private volatile bool _isTraceEnabled;
        private volatile bool _isDebugEnabled;
        private volatile bool _isInfoEnabled;
        private volatile bool _isWarnEnabled;
        private volatile bool _isErrorEnabled;
        private volatile bool _isFatalEnabled;

        /// <summary>
        /// Initializes a new instance of the <see cref="Logger"/> class.
        /// </summary>
        protected internal Logger()
        {
            _contextLogger = this;
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
        /// Collection of context properties for the Logger. The logger will append it for all log events
        /// </summary>
        /// <remarks>
        /// It is recommended to use <see cref="WithProperty(string, object)"/> for modifying context properties
        /// when same named logger is used at multiple locations or shared by different thread contexts.
        /// </remarks>
        public IDictionary<string, object> Properties => _contextProperties ?? System.Threading.Interlocked.CompareExchange(ref _contextProperties, CreateContextPropertiesDictionary(null), null) ?? _contextProperties;

        /// <summary>
        /// Gets a value indicating whether logging is enabled for the specified level.
        /// </summary>
        /// <param name="level">Log level to be checked.</param>
        /// <returns>A value of <see langword="true" /> if logging is enabled for the specified level, otherwise it returns <see langword="false" />.</returns>
        public bool IsEnabled(LogLevel level)
        {
            return GetTargetsForLevelSafe(level) != null;
        }

        /// <summary>
        /// Creates new logger that automatically appends the specified property to all log events (without changing current logger)
        /// 
        /// With <see cref="Properties"/> property, all properties can be enumerated. 
        /// </summary>
        /// <param name="propertyKey">Property Name</param>
        /// <param name="propertyValue">Property Value</param>
        /// <returns>New Logger object that automatically appends specified property</returns>
        public Logger WithProperty(string propertyKey, object propertyValue)
        {
            if (string.IsNullOrEmpty(propertyKey))
                throw new ArgumentException(nameof(propertyKey));

            Logger newLogger = CreateChildLogger();
            newLogger._contextProperties[propertyKey] = propertyValue;
            return newLogger;
        }

        /// <summary>
        /// Creates new logger that automatically appends the specified properties to all log events (without changing current logger)
        /// 
        /// With <see cref="Properties"/> property, all properties can be enumerated. 
        /// </summary>
        /// <param name="properties">Collection of key-value pair properties</param>
        /// <returns>New Logger object that automatically appends specified properties</returns>
        public Logger WithProperties(IEnumerable<KeyValuePair<string, object>> properties)
        {
            Guard.ThrowIfNull(properties);

            Logger newLogger = CreateChildLogger();
            foreach (KeyValuePair<string, object> property in properties)
            {
                newLogger._contextProperties[property.Key] = property.Value;
            }
            return newLogger;
        }

        /// <summary>
        /// Obsolete and replaced by <see cref="WithProperty"/> that prevents unexpected side-effects in Logger-state.
        /// 
        /// Updates the specified context property for the current logger. The logger will append it for all log events.
        ///
        /// With <see cref="Properties"/> property, all properties can be enumerated (or updated). 
        /// </summary>
        /// <remarks>
        /// It is highly recommended to ONLY use <see cref="WithProperty(string, object)"/> for modifying context properties.
        /// This method will affect all locations/contexts that makes use of the same named logger object. And can cause
        /// unexpected surprises at multiple locations and other thread contexts.
        /// </remarks>
        /// <param name="propertyKey">Property Name</param>
        /// <param name="propertyValue">Property Value</param>
        [Obsolete("Instead use WithProperty which is safe. If really necessary then one can use Properties-property. Marked obsolete on NLog 5.0")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void SetProperty(string propertyKey, object propertyValue)
        {
            if (string.IsNullOrEmpty(propertyKey))
                throw new ArgumentException(nameof(propertyKey));

            Properties[propertyKey] = propertyValue;
        }

        private static ThreadSafeDictionary<string, object> CreateContextPropertiesDictionary(ThreadSafeDictionary<string, object> contextProperties)
        {
            contextProperties = contextProperties != null
                ? new ThreadSafeDictionary<string, object>(contextProperties)
                : new ThreadSafeDictionary<string, object>();
            return contextProperties;
        }

        /// <summary>
        /// Updates the <see cref="ScopeContext"/> with provided property
        /// </summary>
        /// <param name="propertyName">Name of property</param>
        /// <param name="propertyValue">Value of property</param>
        /// <returns>A disposable object that removes the properties from logical context scope on dispose.</returns>
        /// <remarks><see cref="ScopeContext"/> property-dictionary-keys are case-insensitive</remarks>
        public IDisposable PushScopeProperty(string propertyName, object propertyValue)
        {
            return ScopeContext.PushProperty(propertyName, propertyValue);
        }

        /// <summary>
        /// Updates the <see cref="ScopeContext"/> with provided property
        /// </summary>
        /// <param name="propertyName">Name of property</param>
        /// <param name="propertyValue">Value of property</param>
        /// <returns>A disposable object that removes the properties from logical context scope on dispose.</returns>
        /// <remarks><see cref="ScopeContext"/> property-dictionary-keys are case-insensitive</remarks>
        public IDisposable PushScopeProperty<TValue>(string propertyName, TValue propertyValue)
        {
            return ScopeContext.PushProperty(propertyName, propertyValue);
        }

#if !NET35 && !NET40
        /// <summary>
        /// Updates the <see cref="ScopeContext"/> with provided properties
        /// </summary>
        /// <param name="scopeProperties">Properties being added to the scope dictionary</param>
        /// <returns>A disposable object that removes the properties from logical context scope on dispose.</returns>
        /// <remarks><see cref="ScopeContext"/> property-dictionary-keys are case-insensitive</remarks>
        public IDisposable PushScopeProperties(IReadOnlyCollection<KeyValuePair<string, object>> scopeProperties)
        {
            return ScopeContext.PushProperties(scopeProperties);
        }

        /// <summary>
        /// Updates the <see cref="ScopeContext"/> with provided properties
        /// </summary>
        /// <param name="scopeProperties">Properties being added to the scope dictionary</param>
        /// <returns>A disposable object that removes the properties from logical context scope on dispose.</returns>
        /// <remarks><see cref="ScopeContext"/> property-dictionary-keys are case-insensitive</remarks>
        public IDisposable PushScopeProperties<TValue>(IReadOnlyCollection<KeyValuePair<string, TValue>> scopeProperties)
        {
            return ScopeContext.PushProperties(scopeProperties);
        }
#endif

        /// <summary>
        /// Pushes new state on the logical context scope stack
        /// </summary>
        /// <param name="nestedState">Value to added to the scope stack</param>
        /// <returns>A disposable object that pops the nested scope state on dispose.</returns>
        public IDisposable PushScopeNested<T>(T nestedState)
        {
            return ScopeContext.PushNestedState(nestedState);
        }

        /// <summary>
        /// Pushes new state on the logical context scope stack
        /// </summary>
        /// <param name="nestedState">Value to added to the scope stack</param>
        /// <returns>A disposable object that pops the nested scope state on dispose.</returns>
        public IDisposable PushScopeNested(object nestedState)
        {
            return ScopeContext.PushNestedState(nestedState);
        }

        /// <summary>
        /// Writes the specified diagnostic message.
        /// </summary>
        /// <param name="logEvent">Log event.</param>
        public void Log(LogEventInfo logEvent)
        {
            var targetsForLevel = GetTargetsForLevelSafe(logEvent.Level);
            if (targetsForLevel != null)
            {
                if (logEvent.LoggerName is null)
                    logEvent.LoggerName = Name;
                if (logEvent.FormatProvider is null)
                    logEvent.FormatProvider = Factory.DefaultCultureInfo;
                WriteToTargets(logEvent, targetsForLevel);
            }
        }

        /// <summary>
        /// Writes the specified diagnostic message.
        /// </summary>
        /// <param name="wrapperType">Type of custom Logger wrapper.</param>
        /// <param name="logEvent">Log event.</param>
        public void Log(Type wrapperType, LogEventInfo logEvent)
        {
            var targetsForLevel = GetTargetsForLevelSafe(logEvent.Level);
            if (targetsForLevel != null)
            {
                if (logEvent.LoggerName is null)
                    logEvent.LoggerName = Name;
                if (logEvent.FormatProvider is null)
                    logEvent.FormatProvider = Factory.DefaultCultureInfo;
                WriteToTargets(wrapperType, logEvent, targetsForLevel);
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
            if (IsEnabled(level))
            {
                WriteToTargets(level, Factory.DefaultCultureInfo, value);
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
            if (IsEnabled(level))
            {
                WriteToTargets(level, formatProvider, value);
            }
        }

        /// <summary>
        /// Writes the diagnostic message at the specified level.
        /// </summary>
        /// <param name="level">The log level.</param>
        /// <param name="messageFunc">A function returning message to be written. Function is not evaluated if logging is not enabled.</param>
        public void Log(LogLevel level, LogMessageGenerator messageFunc)
        {
            if (IsEnabled(level))
            {
                Guard.ThrowIfNull(messageFunc);

                WriteToTargets(level, messageFunc());
            }
        }

        /// <summary>
        /// Writes the diagnostic message at the specified level using the specified parameters and formatting them with the supplied format provider.
        /// </summary>
        /// <param name="level">The log level.</param>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing format items.</param>
        /// <param name="args">Arguments to format.</param>
        [MessageTemplateFormatMethod("message")]
        public void Log(LogLevel level, IFormatProvider formatProvider, [Localizable(false)][StructuredMessageTemplate] string message, params object[] args)
        {
            if (IsEnabled(level))
            {
                WriteToTargets(level, formatProvider, message, args);
            }
        }

        /// <summary>
        /// Writes the diagnostic message at the specified level.
        /// </summary>
        /// <param name="level">The log level.</param>
        /// <param name="message">Log message.</param>
        public void Log(LogLevel level, [Localizable(false)] string message)
        {
            if (IsEnabled(level))
            {
                WriteToTargets(level, message);
            }
        }

        /// <summary>
        /// Writes the diagnostic message at the specified level using the specified parameters.
        /// </summary>
        /// <param name="level">The log level.</param>
        /// <param name="message">A <see langword="string" /> containing format items.</param>
        /// <param name="args">Arguments to format.</param>
        [MessageTemplateFormatMethod("message")]
        public void Log(LogLevel level, [Localizable(false)][StructuredMessageTemplate] string message, params object[] args)
        {
            if (IsEnabled(level))
            {
                WriteToTargets(level, message, args);
            }
        }

        /// <summary>
        /// Writes the diagnostic message and exception at the specified level.
        /// </summary>
        /// <param name="level">The log level.</param>
        /// <param name="exception">An exception to be logged.</param>
        /// <param name="message">A <see langword="string" /> to be written.</param>
        /// <param name="args">Arguments to format.</param>
        [MessageTemplateFormatMethod("message")]
        public void Log(LogLevel level, Exception exception, [Localizable(false)][StructuredMessageTemplate] string message, params object[] args)
        {
            if (IsEnabled(level))
            {
                WriteToTargets(level, exception, message, args);
            }
        }

        /// <summary>
        /// Writes the diagnostic message and exception at the specified level.
        /// </summary>
        /// <param name="level">The log level.</param>
        /// <param name="exception">An exception to be logged.</param>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> to be written.</param>
        /// <param name="args">Arguments to format.</param>
        [MessageTemplateFormatMethod("message")]
        public void Log(LogLevel level, Exception exception, IFormatProvider formatProvider, [Localizable(false)][StructuredMessageTemplate] string message, params object[] args)
        {
            if (IsEnabled(level))
            {
                WriteToTargets(level, exception, formatProvider, message, args);
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
        [MessageTemplateFormatMethod("message")]
        public void Log<TArgument>(LogLevel level, IFormatProvider formatProvider, [Localizable(false)][StructuredMessageTemplate] string message, TArgument argument)
        {
            if (IsEnabled(level))
            {
                WriteToTargets(level, formatProvider, message, new object[] { argument });
            }
        }

        /// <summary>
        /// Writes the diagnostic message at the specified level using the specified parameter.
        /// </summary>
        /// <typeparam name="TArgument">The type of the argument.</typeparam>
        /// <param name="level">The log level.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The argument to format.</param>
        [MessageTemplateFormatMethod("message")]
        public void Log<TArgument>(LogLevel level, [Localizable(false)][StructuredMessageTemplate] string message, TArgument argument)
        {
            if (IsEnabled(level))
            {
                WriteToTargets(level, message, new object[] { argument });
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
        [MessageTemplateFormatMethod("message")]
        public void Log<TArgument1, TArgument2>(LogLevel level, IFormatProvider formatProvider, [Localizable(false)][StructuredMessageTemplate] string message, TArgument1 argument1, TArgument2 argument2)
        {
            if (IsEnabled(level))
            {
                WriteToTargets(level, formatProvider, message, new object[] { argument1, argument2 });
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
        [MessageTemplateFormatMethod("message")]
        public void Log<TArgument1, TArgument2>(LogLevel level, [Localizable(false)][StructuredMessageTemplate] string message, TArgument1 argument1, TArgument2 argument2)
        {
            if (IsEnabled(level))
            {
                WriteToTargets(level, message, new object[] { argument1, argument2 });
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
        [MessageTemplateFormatMethod("message")]
        public void Log<TArgument1, TArgument2, TArgument3>(LogLevel level, IFormatProvider formatProvider, [Localizable(false)][StructuredMessageTemplate] string message, TArgument1 argument1, TArgument2 argument2, TArgument3 argument3)
        {
            if (IsEnabled(level))
            {
                WriteToTargets(level, formatProvider, message, new object[] { argument1, argument2, argument3 });
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
        [MessageTemplateFormatMethod("message")]
        public void Log<TArgument1, TArgument2, TArgument3>(LogLevel level, [Localizable(false)][StructuredMessageTemplate] string message, TArgument1 argument1, TArgument2 argument2, TArgument3 argument3)
        {
            if (IsEnabled(level))
            {
                WriteToTargets(level, message, new object[] { argument1, argument2, argument3 });
            }
        }

        private LogEventInfo PrepareLogEventInfo(LogEventInfo logEvent)
        {
            if (_contextProperties != null)
            {
                foreach (var property in _contextProperties)
                {
                    if (!logEvent.Properties.ContainsKey(property.Key))
                    {
                        logEvent.Properties[property.Key] = property.Value;
                    }
                }
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

#if !NET35 && !NET40
        /// <summary>
        /// Logs an exception is logged at <c>Error</c> level if the provided task does not run to completion.
        /// </summary>
        /// <param name="task">The task for which to log an error if it does not run to completion.</param>
        /// <remarks>This method is useful in fire-and-forget situations, where application logic does not depend on completion of task. This method is avoids C# warning CS4014 in such situations.</remarks>
        public async void Swallow(Task task)
        {
            try
            {
                await task.ConfigureAwait(false);
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
                await task.ConfigureAwait(false);
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
                await asyncAction().ConfigureAwait(false);
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
            return await SwallowAsync(asyncFunc, default(TResult)).ConfigureAwait(false);
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
                return await asyncFunc().ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Error(e);
                return fallback;
            }
        }
#endif

        internal void Initialize(string name, ITargetWithFilterChain[] targetsByLevel, LogFactory factory)
        {
            Name = name;
            Factory = factory;
            SetConfiguration(targetsByLevel);
        }

        private void WriteToTargets(LogLevel level, string message, object[] args)
        {
            WriteToTargets(level, Factory.DefaultCultureInfo, message, args);
        }

        private void WriteToTargets(LogLevel level, IFormatProvider formatProvider, string message, object[] args)
        {
            var targetsForLevel = GetTargetsForLevel(level);
            if (targetsForLevel != null)
            {
                var logEvent = LogEventInfo.Create(level, Name, formatProvider, message, args);
                WriteToTargets(logEvent, targetsForLevel);
            }
        }

        private void WriteToTargets(LogLevel level, string message)
        {
            var targetsForLevel = GetTargetsForLevel(level);
            if (targetsForLevel != null)
            {
                // please note that this overload calls the overload of LogEventInfo.Create with object[] parameter on purpose -
                // to avoid unnecessary string.Format (in case of calling Create(LogLevel, string, IFormatProvider, object))
                var logEvent = LogEventInfo.Create(level, Name, Factory.DefaultCultureInfo, message, (object[])null);
                WriteToTargets(logEvent, targetsForLevel);
            }
        }

        private void WriteToTargets<T>(LogLevel level, IFormatProvider formatProvider, T value)
        {
            var targetsForLevel = GetTargetsForLevel(level);
            if (targetsForLevel != null)
            {
                var logEvent = LogEventInfo.Create(level, Name, formatProvider, value);
                WriteToTargets(logEvent, targetsForLevel);
            }
        }

        private void WriteToTargets(LogLevel level, Exception ex, string message, object[] args)
        {
            var targetsForLevel = GetTargetsForLevel(level);
            if (targetsForLevel != null)
            {
                // Translate Exception with missing LogEvent message as log single value
                var logEvent = message is null && ex != null && !(args?.Length > 0) ? 
                    LogEventInfo.Create(level, Name, ExceptionMessageFormatProvider.Instance, ex) :
                    LogEventInfo.Create(level, Name, ex, Factory.DefaultCultureInfo, message, args);
                WriteToTargets(logEvent, targetsForLevel);
            }
        }

        private void WriteToTargets(LogLevel level, Exception ex, IFormatProvider formatProvider, string message, object[] args)
        {
            var targetsForLevel = GetTargetsForLevel(level);
            if (targetsForLevel != null)
            {
                var logEvent = LogEventInfo.Create(level, Name, ex, formatProvider, message, args);
                WriteToTargets(logEvent, targetsForLevel);
            }
        }

        private void WriteToTargets([NotNull] LogEventInfo logEvent, [NotNull] ITargetWithFilterChain targetsForLevel)
        {
            try
            {
                targetsForLevel.WriteToLoggerTargets(DefaultLoggerType, PrepareLogEventInfo(logEvent), Factory);
            }
            catch (Exception ex)
            {
#if DEBUG
                if (ex.MustBeRethrownImmediately())
                    throw;  // Throwing exceptions here might crash the entire application (.NET 2.0 behavior)

#endif
                if (Factory.ThrowExceptions || LogManager.ThrowExceptions)
                    throw;

                Common.InternalLogger.Error(ex, "Failed to write LogEvent");
            }
        }

        private void WriteToTargets(Type wrapperType, [NotNull] LogEventInfo logEvent, [NotNull] ITargetWithFilterChain targetsForLevel)
        {
            try
            {
                targetsForLevel.WriteToLoggerTargets(wrapperType ?? DefaultLoggerType, PrepareLogEventInfo(logEvent), Factory);
            }
            catch (Exception ex)
            {
#if DEBUG
                if (ex.MustBeRethrownImmediately())
                    throw;  // Throwing exceptions here might crash the entire application (.NET 2.0 behavior)

#endif
                if (Factory.ThrowExceptions || LogManager.ThrowExceptions)
                    throw;

                Common.InternalLogger.Error(ex, "Failed to write LogEvent");
            }
        }

        internal void SetConfiguration(ITargetWithFilterChain[] targetsByLevel)
        {
            _targetsByLevel = targetsByLevel;

            // pre-calculate 'enabled' flags
            _isTraceEnabled = IsEnabled(LogLevel.Trace);
            _isDebugEnabled = IsEnabled(LogLevel.Debug);
            _isInfoEnabled = IsEnabled(LogLevel.Info);
            _isWarnEnabled = IsEnabled(LogLevel.Warn);
            _isErrorEnabled = IsEnabled(LogLevel.Error);
            _isFatalEnabled = IsEnabled(LogLevel.Fatal);

            OnLoggerReconfigured(EventArgs.Empty);
        }

        private ITargetWithFilterChain GetTargetsForLevelSafe(LogLevel level)
        {
            if (level is null)
            {
                throw new InvalidOperationException("Log level must be defined");
            }

            return GetTargetsForLevel(level);
        }

        private ITargetWithFilterChain GetTargetsForLevel(LogLevel level)
        {
            if (ReferenceEquals(_contextLogger, this))
                return _targetsByLevel[level.Ordinal];
            else
                return _contextLogger.GetTargetsForLevel(level);    // Use the GetTargetsForLevel() of the parent Logger
        }

        /// <summary>
        /// Raises the event when the logger is reconfigured. 
        /// </summary>
        /// <param name="e">Event arguments</param>
        protected virtual void OnLoggerReconfigured(EventArgs e)
        {
            LoggerReconfigured?.Invoke(this, e);
        }

        private Logger CreateChildLogger()
        {
            Logger newLogger = (Logger)MemberwiseClone();
            newLogger.Initialize(Name, _targetsByLevel, Factory);
            newLogger._contextProperties = CreateContextPropertiesDictionary(_contextProperties);
            newLogger._contextLogger = _contextLogger;  // Use the GetTargetsForLevel() of the parent Logger
            return newLogger;
        }
    }
}
