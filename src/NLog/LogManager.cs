// 
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
    using System.Diagnostics;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using Internal.Fakeables;
    using NLog.Common;
    using NLog.Config;
    using NLog.Internal;

    /// <summary>
    /// Creates and manages instances of <see cref="T:NLog.Logger" /> objects.
    /// </summary>
    public sealed class LogManager
    {
        private static readonly LogFactory factory = new LogFactory();
        private static IAppDomain currentAppDomain;
        private static GetCultureInfo defaultCultureInfo = () => CultureInfo.CurrentCulture;

        /// <summary>
        /// Delegate used to set/get the culture in use.
        /// </summary>
        public delegate CultureInfo GetCultureInfo();

#if !SILVERLIGHT && !MONO
        /// <summary>
        /// Initializes static members of the LogManager class.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline", Justification = "Significant logic in .cctor()")]
        static LogManager()
        {
            SetupTerminationEvents();            
        }
#endif

        /// <summary>
        /// Prevents a default instance of the LogManager class from being created.
        /// </summary>
        private LogManager()
        {
        }

        /// <summary>
        /// Occurs when logging <see cref="Configuration" /> changes.
        /// </summary>
        public static event EventHandler<LoggingConfigurationChangedEventArgs> ConfigurationChanged
        {
            add { factory.ConfigurationChanged += value; }
            remove { factory.ConfigurationChanged -= value; }
        }

#if !SILVERLIGHT
        /// <summary>
        /// Occurs when logging <see cref="Configuration" /> gets reloaded.
        /// </summary>
        public static event EventHandler<LoggingConfigurationReloadedEventArgs> ConfigurationReloaded
        {
            add { factory.ConfigurationReloaded += value; }
            remove { factory.ConfigurationReloaded -= value; }
        }
#endif
        /// <summary>
        /// Gets or sets a value indicating whether NLog should throw exceptions. 
        /// By default exceptions are not thrown under any circumstances.
        /// </summary>
        public static bool ThrowExceptions
        {
            get { return factory.ThrowExceptions; }
            set { factory.ThrowExceptions = value; }
        }

        internal static IAppDomain CurrentAppDomain
        {
            get { return currentAppDomain ?? (currentAppDomain = AppDomainWrapper.CurrentDomain); }
            set { currentAppDomain = value; }
        }

        /// <summary>
        /// Gets or sets the current logging configuration.
        /// </summary>
        public static LoggingConfiguration Configuration
        {
            get { return factory.Configuration; }
            set { factory.Configuration = value; }
        }

        /// <summary>
        /// Gets or sets the global log threshold. Log events below this threshold are not logged.
        /// </summary>
        public static LogLevel GlobalThreshold
        {
            get { return factory.GlobalThreshold; }
            set { factory.GlobalThreshold = value; }
        }

        /// <summary>
        /// Gets or sets the default culture to use.
        /// </summary>
        public static GetCultureInfo DefaultCultureInfo
        {
            get { return defaultCultureInfo; }
            set { defaultCultureInfo = value; }
        }

        /// <summary>
        /// Gets the logger named after the currently-being-initialized class.
        /// </summary>
        /// <returns>The logger.</returns>
        /// <remarks>This is a slow-running method. 
        /// Make sure you're not doing this in a loop.</remarks>
        [CLSCompliant(false)]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static Logger GetCurrentClassLogger()
        {
            return factory.GetLogger(GetClassFullName());
        }

        /// <summary>
        /// Gets the logger named after the currently-being-initialized class.
        /// </summary>
        /// <param name="loggerType">The logger class. The class must inherit from <see cref="Logger" />.</param>
        /// <returns>The logger.</returns>
        /// <remarks>This is a slow-running method. 
        /// Make sure you're not doing this in a loop.</remarks>
        [CLSCompliant(false)]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static Logger GetCurrentClassLogger(Type loggerType)
        {
            return factory.GetLogger(GetClassFullName(), loggerType);            
        }

        /// <summary>
        /// Creates a logger that discards all log messages.
        /// </summary>
        /// <returns>Null logger which discards all log messages.</returns>
        [CLSCompliant(false)]
        public static Logger CreateNullLogger()
        {
            return factory.CreateNullLogger();
        }

        /// <summary>
        /// Gets the specified named logger.
        /// </summary>
        /// <param name="name">Name of the logger.</param>
        /// <returns>The logger reference. Multiple calls to <c>GetLogger</c> with the same argument aren't guaranteed to return the same logger reference.</returns>
        [CLSCompliant(false)]
        public static Logger GetLogger(string name)
        {
            return factory.GetLogger(name);
        }

        /// <summary>
        /// Gets the specified named logger.
        /// </summary>
        /// <param name="name">Name of the logger.</param>
        /// <param name="loggerType">The logger class. The class must inherit from <see cref="Logger" />.</param>
        /// <returns>The logger reference. Multiple calls to <c>GetLogger</c> with the same argument aren't guaranteed to return the same logger reference.</returns>
        [CLSCompliant(false)]
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

#if !SILVERLIGHT
        /// <summary>
        /// Flush any pending log messages (in case of asynchronous targets).
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
#endif

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
        /// <returns>An object that iplements IDisposable whose Dispose() method reenables logging. 
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
            foreach (var target in Configuration.AllTargets)
            {
                target.Dispose();
            }
        }

        /// <summary>
        /// Gets the fully qualified name of the class invoking the LogManager, including the 
        /// namespace but not the assembly.    
        /// </summary>
        private static string GetClassFullName()
        {
            string className;
            Type declaringType;
            int framesToSkip = 2;

            do
            {
#if SILVERLIGHT
                StackFrame frame = new StackTrace().GetFrame(framesToSkip);
#else
                StackFrame frame = new StackFrame(framesToSkip, false);
#endif
                MethodBase method = frame.GetMethod();
                declaringType = method.DeclaringType;
                if (declaringType == null)
                {
                    className = method.Name;
                    break;
                }

                framesToSkip++;
                className = declaringType.FullName;
            } while (declaringType.Module.Name.Equals("mscorlib.dll", StringComparison.OrdinalIgnoreCase));

            return className;
        }

#if !SILVERLIGHT && !MONO
        private static void SetupTerminationEvents()
        {
            try
            {
                CurrentAppDomain.ProcessExit += TurnOffLogging;
                CurrentAppDomain.DomainUnload += TurnOffLogging;
            }
            catch (Exception exception)
            {
                if (exception.MustBeRethrown())
                {
                    throw;
                }

                InternalLogger.Warn("Error setting up termination events: {0}", exception);
            }            
        }

        private static void TurnOffLogging(object sender, EventArgs args)
        {
            // Reset logging configuration to null; this causes old configuration (if any) to be 
            // closed.
            InternalLogger.Info("Shutting down logging...");
            Configuration = null;
            InternalLogger.Info("Logger has been shut down.");
        }
#endif
    }
}
