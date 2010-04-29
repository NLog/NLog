// 
// Copyright (c) 2004-2006 Jaroslaw Kowalski <jaak@jkowalski.net>
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
using System.Collections;
using System.Xml;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Diagnostics;
using System.Security;
using System.Text;
using System.Runtime.CompilerServices;
using System.Globalization;

using NLog.Config;
using NLog.Internal;
using NLog.Targets;

namespace NLog
{
    /// <summary>
    /// Creates and manages instances of <see cref="T:NLog.Logger" /> objects.
    /// </summary>
    public sealed class LogManager
    {
        private static LogFactory _globalFactory = new LogFactory();

        /// <summary>
        /// Occurs when logging <see cref="Configuration" /> changes.
        /// </summary>
        public static event LoggingConfigurationChanged ConfigurationChanged
        {
            add { _globalFactory.ConfigurationChanged += value; }
            remove { _globalFactory.ConfigurationChanged -= value; }
        }

#if !NETCF
        /// <summary>
        /// Occurs when logging <see cref="Configuration" /> gets reloaded.
        /// </summary>
        public static event LoggingConfigurationReloaded ConfigurationReloaded
        {
            add { _globalFactory.ConfigurationReloaded += value; }
            remove { _globalFactory.ConfigurationReloaded -= value; }
        }
#endif
        /// <summary>
        /// Specified whether NLog should throw exceptions. By default exceptions
        /// are not thrown under any circumstances.
        /// </summary>
        public static bool ThrowExceptions
        {
            get { return _globalFactory.ThrowExceptions; }
            set { _globalFactory.ThrowExceptions = value; }
        }

        private LogManager(){}

#if !NETCF
        /// <summary>
        /// Gets the logger named after the currently-being-initialized class.
        /// </summary>
        /// <returns>The logger.</returns>
        /// <remarks>This is a slow-running method. 
        /// Make sure you're not doing this in a loop.</remarks>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static Logger GetCurrentClassLogger()
        {
            StackFrame frame = new StackFrame(1, false);

            return _globalFactory.GetLogger(frame.GetMethod().DeclaringType.FullName);
        }

        /// <summary>
        /// Gets the logger named after the currently-being-initialized class.
        /// </summary>
        /// <param name="loggerType">the logger class. The class must inherit from <see cref="Logger" /></param>
        /// <returns>The logger.</returns>
        /// <remarks>This is a slow-running method. 
        /// Make sure you're not doing this in a loop.</remarks>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static Logger GetCurrentClassLogger(Type loggerType)
        {
            StackFrame frame = new StackFrame(1, false);

            return _globalFactory.GetLogger(frame.GetMethod().DeclaringType.FullName, loggerType);
        }
#endif

        /// <summary>
        /// Creates a logger that discards all log messages.
        /// </summary>
        /// <returns></returns>
        public static Logger CreateNullLogger()
        {
            return _globalFactory.CreateNullLogger();

        }

        /// <summary>
        /// Gets the specified named logger.
        /// </summary>
        /// <param name="name">name of the logger</param>
        /// <returns>The logger reference. Multiple calls to <c>GetLogger</c> with the same argument aren't guaranteed to return the same logger reference.</returns>
        public static Logger GetLogger(string name)
        {
            return _globalFactory.GetLogger(name);
        }

        /// <summary>
        /// Gets the specified named logger.
        /// </summary>
        /// <param name="name">name of the logger</param>
        /// <param name="loggerType">the logger class. The class must inherit from <see cref="Logger" /></param>
        /// <returns>The logger reference. Multiple calls to <c>GetLogger</c> with the same argument aren't guaranteed to return the same logger reference.</returns>
        public static Logger GetLogger(string name, Type loggerType)
        {
            return _globalFactory.GetLogger(name, loggerType);
        }

        /// <summary>
        /// Gets or sets the current logging configuration.
        /// </summary>
        public static LoggingConfiguration Configuration
        {
            get { return _globalFactory.Configuration; }
            set { _globalFactory.Configuration = value; }
        }


        /// <summary>
        /// Loops through all loggers previously returned by GetLogger.
        /// and recalculates their target and filter list. Useful after modifying the configuration programmatically
        /// to ensure that all loggers have been properly configured.
        /// </summary>
        public static void ReconfigExistingLoggers()
        {
            _globalFactory.ReconfigExistingLoggers();
        }

        /// <summary>
        /// Flush any pending log messages (in case of asynchronous targets).
        /// </summary>
        public static void Flush()
        {
            _globalFactory.Flush();
        }

        /// <summary>
        /// Flush any pending log messages (in case of asynchronous targets).
        /// </summary>
        /// <param name="timeout">Maximum time to allow for the flush. Any messages after that time will be discarded.</param>
        public static void Flush(TimeSpan timeout)
        {
            _globalFactory.Flush(timeout);
        }

        /// <summary>
        /// Flush any pending log messages (in case of asynchronous targets).
        /// </summary>
        /// <param name="timeoutMilliseconds">Maximum time to allow for the flush. Any messages after that time will be discarded.</param>
        public static void Flush(int timeoutMilliseconds)
        {
            _globalFactory.Flush(timeoutMilliseconds);
        }

        /// <summary>Decreases the log enable counter and if it reaches -1 
        /// the logs are disabled.</summary>
        /// <remarks>Logging is enabled if the number of <see cref="EnableLogging"/> calls is greater 
        /// than or equal to <see cref="DisableLogging"/> calls.</remarks>
        /// <returns>An object that iplements IDisposable whose Dispose() method
        /// reenables logging. To be used with C# <c>using ()</c> statement.</returns>
        public static IDisposable DisableLogging()
        {
            return _globalFactory.DisableLogging();
        }

        /// <summary>Increases the log enable counter and if it reaches 0 the logs are disabled.</summary>
        /// <remarks>Logging is enabled if the number of <see cref="EnableLogging"/> calls is greater 
        /// than or equal to <see cref="DisableLogging"/> calls.</remarks>
        public static void EnableLogging()
        {
            _globalFactory.EnableLogging();
        }

        /// <summary>
        /// Returns <see langword="true" /> if logging is currently enabled.
        /// </summary>
        /// <returns><see langword="true" /> if logging is currently enabled, 
        /// <see langword="false"/> otherwise.</returns>
        /// <remarks>Logging is enabled if the number of <see cref="EnableLogging"/> calls is greater 
        /// than or equal to <see cref="DisableLogging"/> calls.</remarks>
        public static bool IsLoggingEnabled()
        {
            return _globalFactory.IsLoggingEnabled();
        }

        /// <summary>
        /// Global log threshold. Log events below this threshold are not logged.
        /// </summary>
        public static LogLevel GlobalThreshold
        {
            get { return _globalFactory.GlobalThreshold; }
            set { _globalFactory.GlobalThreshold = value; }
        }

#if !NETCF
        private static void SetupTerminationEvents()
        {
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(TurnOffLogging);
            AppDomain.CurrentDomain.DomainUnload += new EventHandler(TurnOffLogging);
        }

        private static void TurnOffLogging(object sender, EventArgs args)
        {
            // reset logging configuration to null
            // this causes old configuration (if any) to be closed.

            InternalLogger.Info("Shutting down logging...");
            Configuration = null;
            InternalLogger.Info("Logger has been shut down.");
        }
#endif
        static LogManager()
        {
#if !NETCF
            try
            {
                SetupTerminationEvents();
            }
            catch (Exception ex)
            {
                InternalLogger.Warn("Error setting up termiation events: {0}", ex);
            }
#endif
        }
    }
}
