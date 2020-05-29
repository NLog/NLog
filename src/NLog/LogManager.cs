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
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;

    using NLog.Common;
    using NLog.Config;
    using NLog.Internal;

    /// <summary>
    /// Creates and manages instances of <see cref="T:NLog.Logger" /> objects.
    /// </summary>
    public static class LogManager
    {
        /// <remarks>
        /// Internal for unit tests
        /// </remarks>
        internal static readonly LogFactory factory = new LogFactory();
        private static ICollection<Assembly> _hiddenAssemblies;

        private static readonly object lockObject = new object();

        /// <summary>
        /// Gets the <see cref="NLog.LogFactory" /> instance used in the <see cref="LogManager"/>.
        /// </summary>
        /// <remarks>Could be used to pass the to other methods</remarks>
        public static LogFactory LogFactory => factory;

        /// <summary>
        /// Occurs when logging <see cref="Configuration" /> changes.
        /// </summary>
        public static event EventHandler<LoggingConfigurationChangedEventArgs> ConfigurationChanged
        {
            add => factory.ConfigurationChanged += value;
            remove => factory.ConfigurationChanged -= value;
        }

#if !NETSTANDARD1_3
        /// <summary>
        /// Occurs when logging <see cref="Configuration" /> gets reloaded.
        /// </summary>
        public static event EventHandler<LoggingConfigurationReloadedEventArgs> ConfigurationReloaded
        {
            add => factory.ConfigurationReloaded += value;
            remove => factory.ConfigurationReloaded -= value;
        }
#endif
        /// <summary>
        /// Gets or sets a value indicating whether NLog should throw exceptions. 
        /// By default exceptions are not thrown under any circumstances.
        /// </summary>
        public static bool ThrowExceptions
        {
            get => factory.ThrowExceptions;
            set => factory.ThrowExceptions = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether <see cref="NLogConfigurationException"/> should be thrown.
        /// </summary>
        /// <value>A value of <c>true</c> if exception should be thrown; otherwise, <c>false</c>.</value>
        /// <remarks>
        /// This option is for backwards-compatibility.
        /// By default exceptions are not thrown under any circumstances.
        /// 
        /// </remarks>
        public static bool? ThrowConfigExceptions
        {
            get => factory.ThrowConfigExceptions;
            set => factory.ThrowConfigExceptions = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether Variables should be kept on configuration reload.
        /// Default value - false.
        /// </summary>
        public static bool KeepVariablesOnReload
        {
            get => factory.KeepVariablesOnReload;
            set => factory.KeepVariablesOnReload = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether to automatically call <see cref="LogManager.Shutdown"/>
        /// on AppDomain.Unload or AppDomain.ProcessExit
        /// </summary>
        public static bool AutoShutdown
        {
            get => factory.AutoShutdown;
            set => factory.AutoShutdown = value;
        }

        /// <summary>
        /// Gets or sets the current logging configuration.
        /// <see cref="NLog.LogFactory.Configuration" />
        /// </summary>
        public static LoggingConfiguration Configuration
        {
            get => factory.Configuration;
            set => factory.Configuration = value;
        }

        /// <summary>
        /// Begins configuration of the LogFactory options using fluent interface
        /// </summary>
        public static ISetupBuilder Setup()
        {
            return LogFactory.Setup();
        }

        /// <summary>
        /// Begins configuration of the LogFactory options using fluent interface
        /// </summary>
        public static LogFactory Setup(Action<ISetupBuilder> setupBuilder)
        {
            return LogFactory.Setup(setupBuilder);
        }

        /// <summary>
        /// Loads logging configuration from file (Currently only XML configuration files supported)
        /// </summary>
        /// <param name="configFile">Configuration file to be read</param>
        /// <returns>LogFactory instance for fluent interface</returns>
        public static LogFactory LoadConfiguration(string configFile)
        {
            factory.LoadConfiguration(configFile);
            return factory;
        }

        /// <summary>
        /// Gets or sets the global log threshold. Log events below this threshold are not logged.
        /// </summary>
        public static LogLevel GlobalThreshold
        {
            get => factory.GlobalThreshold;
            set => factory.GlobalThreshold = value;
        }

        /// <summary>
        /// Gets the logger with the full name of the current class, so namespace and class name.
        /// </summary>
        /// <returns>The logger.</returns>
        /// <remarks>This is a slow-running method. 
        /// Make sure you're not doing this in a loop.</remarks>
        [CLSCompliant(false)]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static Logger GetCurrentClassLogger()
        {
            return factory.GetLogger(StackTraceUsageUtils.GetClassFullName());
        }

        internal static bool IsHiddenAssembly(Assembly assembly)
        {
            return _hiddenAssemblies != null && _hiddenAssemblies.Contains(assembly);
        }

        /// <summary>
        /// Adds the given assembly which will be skipped 
        /// when NLog is trying to find the calling method on stack trace.
        /// </summary>
        /// <param name="assembly">The assembly to skip.</param>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void AddHiddenAssembly(Assembly assembly)
        {
            lock (lockObject)
            {
                if (_hiddenAssemblies != null && _hiddenAssemblies.Contains(assembly))
                    return;

                _hiddenAssemblies = new HashSet<Assembly>(_hiddenAssemblies ?? Enumerable.Empty<Assembly>())
                {
                    assembly
                };
            }

            InternalLogger.Trace("Assembly '{0}' will be hidden in callsite stacktrace", assembly?.FullName);
        }

        /// <summary>
        /// Gets a custom logger with the full name of the current class, so namespace and class name.
        /// Use <paramref name="loggerType"/> to create instance of a custom <see cref="Logger"/>.
        /// If you haven't defined your own <see cref="Logger"/> class, then use the overload without the loggerType.
        /// </summary>
        /// <param name="loggerType">The logger class. This class must inherit from <see cref="Logger" />.</param>
        /// <returns>The logger of type <paramref name="loggerType"/>.</returns>
        /// <remarks>This is a slow-running method. 
        /// Make sure you're not doing this in a loop.</remarks>
        [CLSCompliant(false)]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static Logger GetCurrentClassLogger(Type loggerType)
        {
            return factory.GetLogger(StackTraceUsageUtils.GetClassFullName(), loggerType);
        }

        /// <summary>
        /// Creates a logger that discards all log messages.
        /// </summary>
        /// <returns>Null logger which discards all log messages.</returns>
        public static Logger CreateNullLogger()
        {
            return factory.CreateNullLogger();
        }

        /// <summary>
        /// Gets the specified named logger.
        /// </summary>
        /// <param name="name">Name of the logger.</param>
        /// <returns>The logger reference. Multiple calls to <c>GetLogger</c> with the same argument aren't guaranteed to return the same logger reference.</returns>
        public static Logger GetLogger(string name)
        {
            return factory.GetLogger(name);
        }

        /// <summary>
        /// Gets the specified named custom logger.
        /// Use <paramref name="loggerType"/> to create instance of a custom <see cref="Logger"/>.
        /// If you haven't defined your own <see cref="Logger"/> class, then use the overload without the loggerType.
        /// </summary>
        /// <param name="name">Name of the logger.</param>
        /// <param name="loggerType">The logger class. This class must inherit from <see cref="Logger" />.</param>
        /// <returns>The logger of type <paramref name="loggerType"/>. Multiple calls to <c>GetLogger</c> with the same argument aren't guaranteed to return the same logger reference.</returns>
        /// <remarks>The generic way for this method is <see cref="NLog.LogFactory{loggerType}.GetLogger(string)"/></remarks>
        public static Logger GetLogger(string name, Type loggerType)
        {
            return factory.GetLogger(name, loggerType);
        }

        /// <summary>
        /// Loops through all loggers previously returned by GetLogger.
        /// and recalculates their target and filter list. Useful after modifying the configuration programmatically
        /// to ensure that all loggers have been properly configured.
        /// </summary>
        public static void ReconfigExistingLoggers()
        {
            factory.ReconfigExistingLoggers();
        }

        /// <summary>
        /// Flush any pending log messages (in case of asynchronous targets) with the default timeout of 15 seconds.
        /// </summary>
        public static void Flush()
        {
            factory.Flush();
        }

        /// <summary>
        /// Flush any pending log messages (in case of asynchronous targets).
        /// </summary>
        /// <param name="timeout">Maximum time to allow for the flush. Any messages after that time will be discarded.</param>
        public static void Flush(TimeSpan timeout)
        {
            factory.Flush(timeout);
        }

        /// <summary>
        /// Flush any pending log messages (in case of asynchronous targets).
        /// </summary>
        /// <param name="timeoutMilliseconds">Maximum time to allow for the flush. Any messages after that time will be discarded.</param>
        public static void Flush(int timeoutMilliseconds)
        {
            factory.Flush(timeoutMilliseconds);
        }

        /// <summary>
        /// Flush any pending log messages (in case of asynchronous targets).
        /// </summary>
        /// <param name="asyncContinuation">The asynchronous continuation.</param>
        public static void Flush(AsyncContinuation asyncContinuation)
        {
            factory.Flush(asyncContinuation);
        }

        /// <summary>
        /// Flush any pending log messages (in case of asynchronous targets).
        /// </summary>
        /// <param name="asyncContinuation">The asynchronous continuation.</param>
        /// <param name="timeout">Maximum time to allow for the flush. Any messages after that time will be discarded.</param>
        public static void Flush(AsyncContinuation asyncContinuation, TimeSpan timeout)
        {
            factory.Flush(asyncContinuation, timeout);
        }

        /// <summary>
        /// Flush any pending log messages (in case of asynchronous targets).
        /// </summary>
        /// <param name="asyncContinuation">The asynchronous continuation.</param>
        /// <param name="timeoutMilliseconds">Maximum time to allow for the flush. Any messages after that time will be discarded.</param>
        public static void Flush(AsyncContinuation asyncContinuation, int timeoutMilliseconds)
        {
            factory.Flush(asyncContinuation, timeoutMilliseconds);
        }

        /// <summary>
        /// Decreases the log enable counter and if it reaches -1 the logs are disabled.
        /// </summary>
        /// <remarks>Logging is enabled if the number of <see cref="EnableLogging"/> calls is greater 
        ///     than or equal to <see cref="DisableLogging"/> calls.</remarks>
        /// <returns>An object that implements IDisposable whose Dispose() method re-enables logging. 
        ///     To be used with C# <c>using ()</c> statement.</returns>
        public static IDisposable DisableLogging()
        {
            return factory.SuspendLogging();
        }

        /// <summary>
        /// Increases the log enable counter and if it reaches 0 the logs are disabled.
        /// </summary>
        /// <remarks>Logging is enabled if the number of <see cref="EnableLogging"/> calls is greater 
        ///     than or equal to <see cref="DisableLogging"/> calls.</remarks>
        public static void EnableLogging()
        {
            factory.ResumeLogging();
        }

        /// <summary>
        /// Checks if logging is currently enabled.
        /// </summary>
        /// <returns><see langword="true" /> if logging is currently enabled, <see langword="false"/> 
        ///     otherwise.</returns>
        /// <remarks>Logging is enabled if the number of <see cref="EnableLogging"/> calls is greater 
        ///     than or equal to <see cref="DisableLogging"/> calls.</remarks>
        public static bool IsLoggingEnabled()
        {
            return factory.IsLoggingEnabled();
        }

        /// <summary>
        /// Dispose all targets, and shutdown logging.
        /// </summary>
        public static void Shutdown()
        {
            factory.Shutdown();
        }
    }
}
