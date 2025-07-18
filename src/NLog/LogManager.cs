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
    using System.Diagnostics.CodeAnalysis;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using JetBrains.Annotations;
    using NLog.Common;
    using NLog.Config;
    using NLog.Internal;

    /// <summary>
    /// Creates and manages instances of <see cref="NLog.Logger" /> objects.
    /// </summary>
    /// <remarks>
    /// LogManager wraps a singleton instance of <see cref="NLog.LogFactory" />.
    /// </remarks>
    public static class LogManager
    {
        /// <summary>
        /// Gets the <see cref="NLog.LogFactory" /> instance used in the <see cref="LogManager"/>.
        /// </summary>
        public static LogFactory LogFactory => _logFactory ?? CreateLogFactorySingleton();
        private static LogFactory? _logFactory;

        private static LogFactory CreateLogFactorySingleton()
        {
            var logFactory = new LogFactory();
            if (!(System.Threading.Interlocked.CompareExchange(ref _logFactory, logFactory, null) is null))
                logFactory.Dispose();   // Raced by other thread, so dispose instance not needed
            return _logFactory;
        }

        /// <summary>
        /// Occurs when logging <see cref="Configuration" /> changes. Both when assigned to new config or config unloaded.
        /// </summary>
        /// <remarks>
        /// Note <see cref="LoggingConfigurationChangedEventArgs.ActivatedConfiguration"/> can be <c>null</c> when unloading configuration at shutdown.
        /// </remarks>
        public static event EventHandler<LoggingConfigurationChangedEventArgs> ConfigurationChanged
        {
            add => LogFactory.ConfigurationChanged += value;
            remove => LogFactory.ConfigurationChanged -= value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether NLog should throw exceptions.
        /// By default exceptions are not thrown under any circumstances.
        /// </summary>
        public static bool ThrowExceptions
        {
            get => LogFactory.ThrowExceptions;
            set => LogFactory.ThrowExceptions = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether <see cref="NLogConfigurationException"/> should be thrown.
        /// </summary>
        /// <value>A value of <see langword="true"/> if exception should be thrown; otherwise, <see langword="false"/>.</value>
        /// <remarks>
        /// This option is for backwards-compatibility.
        /// By default exceptions are not thrown under any circumstances.
        ///
        /// </remarks>
        public static bool? ThrowConfigExceptions
        {
            get => LogFactory.ThrowConfigExceptions;
            set => LogFactory.ThrowConfigExceptions = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether Variables should be kept on configuration reload.
        /// </summary>
        public static bool KeepVariablesOnReload
        {
            get => LogFactory.KeepVariablesOnReload;
            set => LogFactory.KeepVariablesOnReload = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether to automatically call <see cref="LogManager.Shutdown"/>
        /// on AppDomain.Unload or AppDomain.ProcessExit
        /// </summary>
        public static bool AutoShutdown
        {
            get => LogFactory.AutoShutdown;
            set => LogFactory.AutoShutdown = value;
        }

        /// <summary>
        /// Gets or sets the current logging configuration.
        /// </summary>
        /// <remarks>
        /// Setter will re-configure all <see cref="Logger"/>-objects, so no need to also call <see cref="ReconfigExistingLoggers()" />
        /// </remarks>
        [CanBeNull]
        public static LoggingConfiguration? Configuration
        {
            get => LogFactory.Configuration;
            set => LogFactory.Configuration = value;
        }

        /// <summary>
        /// Gets or sets the global log threshold. Log events below this threshold are not logged.
        /// </summary>
        public static LogLevel GlobalThreshold
        {
            get => LogFactory.GlobalThreshold;
            set => LogFactory.GlobalThreshold = value;
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
        /// Obsolete and replaced by <see cref="LogManager.Setup()"/> and <see cref="SetupBuilderExtensions.LoadConfigurationFromFile(ISetupBuilder, string, bool)"/> with NLog v5.2.
        /// Loads logging configuration from file (Only XML configuration files supported)
        /// </summary>
        /// <param name="configFile">Configuration file to be read</param>
        /// <returns>LogFactory instance for fluent interface</returns>
        [Obsolete("Replaced by LogManager.Setup().LoadConfigurationFromFile(). Marked obsolete on NLog 5.2")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static LogFactory LoadConfiguration(string configFile)
        {
            return LogFactory.LoadConfiguration(configFile);
        }

        /// <summary>
        /// Adds the given assembly which will be skipped
        /// when NLog is trying to find the calling method on stack trace.
        /// </summary>
        /// <param name="assembly">The assembly to skip.</param>
        [Obsolete("Replaced by LogManager.Setup().SetupLogFactory(setup => setup.AddCallSiteHiddenAssembly(assembly)). Marked obsolete on NLog 5.3")]
        public static void AddHiddenAssembly(Assembly assembly)
        {
            CallSiteInformation.AddCallSiteHiddenAssembly(assembly);
        }

        /// <summary>
        /// Gets the logger with the full name of the current class, so namespace and class name.
        /// </summary>
        /// <returns>The logger.</returns>
        /// <remarks>This method introduces performance hit, because of StackTrace capture.
        /// Make sure you are not calling this method in a loop.</remarks>
        [CLSCompliant(false)]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static Logger GetCurrentClassLogger()
        {
            var className = StackTraceUsageUtils.GetClassFullName(new System.Diagnostics.StackFrame(1, false));
            return LogFactory.GetLogger(className);
        }

        /// <summary>
        /// Obsolete and replaced by <see cref="LogFactory.GetCurrentClassLogger{T}()"/> with NLog v5.2.
        /// Gets a custom logger with the full name of the current class, so namespace and class name.
        /// Use <paramref name="loggerType"/> to create instance of a custom <see cref="Logger"/>.
        /// If you haven't defined your own <see cref="Logger"/> class, then use the overload without the loggerType.
        /// </summary>
        /// <param name="loggerType">The type of the logger to create. The type must inherit from <see cref="Logger"/></param>
        /// <returns>The logger of type <paramref name="loggerType"/>.</returns>
        /// <remarks>This method introduces performance hit, because of StackTrace capture.
        /// Make sure you are not calling this method in a loop.</remarks>
        [CLSCompliant(false)]
        [MethodImpl(MethodImplOptions.NoInlining)]
        [Obsolete("Replaced by LogFactory.GetCurrentClassLogger<T>(). Marked obsolete on NLog 5.2")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static Logger GetCurrentClassLogger([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] Type loggerType)
        {
            var className = StackTraceUsageUtils.GetClassFullName(new System.Diagnostics.StackFrame(1, false));
            return LogFactory.GetLogger(className, loggerType);
        }

        /// <summary>
        /// Creates a logger that discards all log messages.
        /// </summary>
        /// <returns>Null logger which discards all log messages.</returns>
        public static Logger CreateNullLogger()
        {
            return LogFactory.CreateNullLogger();
        }

        /// <summary>
        /// Gets the specified named logger.
        /// </summary>
        /// <param name="name">Name of the logger.</param>
        /// <returns>The logger reference. Multiple calls to <c>GetLogger</c> with the same argument aren't guaranteed to return the same logger reference.</returns>
        public static Logger GetLogger(string name)
        {
            return LogFactory.GetLogger(name);
        }

        /// <summary>
        /// Obsolete and replaced by <see cref="LogFactory.GetLogger{T}(string)"/> with NLog v5.2.
        /// Gets the specified named custom <see cref="Logger"/> using the parameter <paramref name="loggerType"/> for creating instance.
        /// If you haven't defined your own <see cref="Logger"/> class, then use the overload without the loggerType.
        /// </summary>
        /// <param name="name">Name of the logger.</param>
        /// <param name="loggerType">The logger class. This class must inherit from <see cref="Logger" />.</param>
        /// <returns>The logger of type <paramref name="loggerType"/>. Multiple calls to <c>GetLogger</c> with the same argument aren't guaranteed to return the same logger reference.</returns>
        /// <remarks>The generic way for this method is <see cref="NLog.LogFactory{loggerType}.GetLogger(string)"/></remarks>
        [Obsolete("Replaced by LogFactory.GetLogger<T>(). Marked obsolete on NLog 5.2")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static Logger GetLogger(string name, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] Type loggerType)
        {
            return LogFactory.GetLogger(name, loggerType);
        }

        /// <summary>
        /// Loops through all loggers previously returned by GetLogger.
        /// and recalculates their target and filter list. Useful after modifying the configuration programmatically
        /// to ensure that all loggers have been properly configured.
        /// </summary>
        public static void ReconfigExistingLoggers()
        {
            LogFactory.ReconfigExistingLoggers();
        }

        /// <summary>
        /// Loops through all loggers previously returned by GetLogger.
        /// and recalculates their target and filter list. Useful after modifying the configuration programmatically
        /// to ensure that all loggers have been properly configured.
        /// </summary>
        /// <param name="purgeObsoleteLoggers">Purge garbage collected logger-items from the cache</param>
        public static void ReconfigExistingLoggers(bool purgeObsoleteLoggers)
        {
            LogFactory.ReconfigExistingLoggers(purgeObsoleteLoggers);
        }

        /// <summary>
        /// Flush any pending log messages (in case of asynchronous targets) with the default timeout of 15 seconds.
        /// </summary>
        public static void Flush()
        {
            LogFactory.Flush();
        }

        /// <summary>
        /// Flush any pending log messages (in case of asynchronous targets).
        /// </summary>
        /// <param name="timeout">Maximum time to allow for the flush. Any messages after that time will be discarded.</param>
        public static void Flush(TimeSpan timeout)
        {
            LogFactory.Flush(timeout);
        }

        /// <summary>
        /// Flush any pending log messages (in case of asynchronous targets).
        /// </summary>
        /// <param name="timeoutMilliseconds">Maximum time to allow for the flush. Any messages after that time will be discarded.</param>
        public static void Flush(int timeoutMilliseconds)
        {
            LogFactory.Flush(timeoutMilliseconds);
        }

        /// <summary>
        /// Flush any pending log messages (in case of asynchronous targets).
        /// </summary>
        /// <param name="asyncContinuation">The asynchronous continuation.</param>
        public static void Flush(AsyncContinuation asyncContinuation)
        {
            LogFactory.Flush(asyncContinuation);
        }

        /// <summary>
        /// Flush any pending log messages (in case of asynchronous targets).
        /// </summary>
        /// <param name="asyncContinuation">The asynchronous continuation.</param>
        /// <param name="timeout">Maximum time to allow for the flush. Any messages after that time will be discarded.</param>
        public static void Flush(AsyncContinuation asyncContinuation, TimeSpan timeout)
        {
            LogFactory.Flush(asyncContinuation, timeout);
        }

        /// <summary>
        /// Flush any pending log messages (in case of asynchronous targets).
        /// </summary>
        /// <param name="asyncContinuation">The asynchronous continuation.</param>
        /// <param name="timeoutMilliseconds">Maximum time to allow for the flush. Any messages after that time will be discarded.</param>
        public static void Flush(AsyncContinuation asyncContinuation, int timeoutMilliseconds)
        {
            LogFactory.Flush(asyncContinuation, timeoutMilliseconds);
        }

        /// <summary>
        /// Obsolete and replaced by by <see cref="SuspendLogging"/> with NLog v5.
        /// Suspends the logging, and returns object for using-scope so scope-exit calls <see cref="EnableLogging"/>
        /// </summary>
        /// <remarks>
        /// Logging is suspended when the number of <see cref="DisableLogging"/> calls are greater
        /// than the number of <see cref="EnableLogging"/> calls.
        /// </remarks>
        /// <returns>An object that implements IDisposable whose Dispose() method re-enables logging.
        /// To be used with C# <c>using ()</c> statement.</returns>
        [Obsolete("Use SuspendLogging() instead. Marked obsolete on NLog 5.0")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static IDisposable DisableLogging()
        {
            return LogFactory.SuspendLogging();
        }

        /// <summary>
        /// Obsolete and replaced by disposing the scope returned from <see cref="SuspendLogging"/> with NLog v5.
        /// Resumes logging if having called <see cref="DisableLogging"/>.
        /// </summary>
        /// <remarks>
        /// Logging is suspended when the number of <see cref="DisableLogging"/> calls are greater
        /// than the number of <see cref="EnableLogging"/> calls.
        /// </remarks>
        [Obsolete("Use ResumeLogging() instead. Marked obsolete on NLog 5.0")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void EnableLogging()
        {
            LogFactory.ResumeLogging();
        }

        /// <summary>
        /// Suspends the logging, and returns object for using-scope so scope-exit calls <see cref="ResumeLogging"/>
        /// </summary>
        /// <remarks>
        /// Logging is suspended when the number of <see cref="SuspendLogging"/> calls are greater
        /// than the number of <see cref="ResumeLogging"/> calls.
        /// </remarks>
        /// <returns>An object that implements IDisposable whose Dispose() method re-enables logging.
        /// To be used with C# <c>using ()</c> statement.</returns>
        public static IDisposable SuspendLogging()
        {
            return LogFactory.SuspendLogging();
        }

        /// <summary>
        /// Resumes logging if having called <see cref="SuspendLogging"/>.
        /// </summary>
        /// <remarks>
        /// Logging is suspended when the number of <see cref="SuspendLogging"/> calls are greater
        /// than the number of <see cref="ResumeLogging"/> calls.
        /// </remarks>
        public static void ResumeLogging()
        {
            LogFactory.ResumeLogging();
        }

        /// <summary>
        /// Returns <see langword="true" /> if logging is currently enabled.
        /// </summary>
        /// <remarks>
        /// Logging is suspended when the number of <see cref="SuspendLogging"/> calls are greater
        /// than the number of <see cref="ResumeLogging"/> calls.
        /// </remarks>
        /// <returns>A value of <see langword="true" /> if logging is currently enabled,
        /// <see langword="false"/> otherwise.</returns>
        public static bool IsLoggingEnabled()
        {
            return LogFactory.IsLoggingEnabled();
        }

        /// <summary>
        /// Dispose all targets, and shutdown logging.
        /// </summary>
        public static void Shutdown()
        {
            LogFactory.Shutdown();
        }
    }
}
