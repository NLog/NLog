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
        private static LoggerDictionary _loggerCache = new LoggerDictionary();
        private static LoggingConfiguration _config;
        private static LogLevel _globalThreshold = LogLevel.MinLevel;
        private static bool _configLoaded = false;
        private static bool _throwExceptions = false;

        /// <summary>
        /// Specified whether NLog should throw exceptions. By default exceptions
        /// are not thrown under any circumstances.
        /// </summary>
        public static bool ThrowExceptions
        {
            get { return _throwExceptions; }
            set { _throwExceptions = value; }
        }

        private LogManager(){}

#if !NETCF
        /// <summary>
        /// Gets the logger named after the currently-being-initialized class.
        /// </summary>
        /// <returns>The logger.</returns>
        /// <remarks>This is a slow-running method. 
        /// Make sure you're not doing this in a loop.</remarks>
        public static Logger GetCurrentClassLogger()
        {
            StackFrame frame = new StackFrame(1, false);

            return GetLogger(frame.GetMethod().DeclaringType.FullName);
        }
#endif

        /// <summary>
        /// Creates a logger that discards all log messages.
        /// </summary>
        /// <returns></returns>
        public static Logger CreateNullLogger()
        {
            TargetWithFilterChain[]targetsByLevel = new TargetWithFilterChain[LogLevel.MaxLevel.Ordinal + 1];
            return new Logger("", new LoggerConfiguration(targetsByLevel));

        }

        /// <summary>
        /// Gets the specified named logger.
        /// </summary>
        /// <param name="name">name of the logger</param>
        /// <returns>The logger reference. Multiple calls to <c>GetLogger</c> with the same argument aren't guaranteed to return the same logger reference.</returns>
        public static Logger GetLogger(string name)
        {
            lock(typeof(LogManager))
            {
                Logger l = _loggerCache[name];
                if (l != null)
                    return l;

                Logger newLogger = new Logger(name, GetConfigurationForLogger(name, Configuration));
                _loggerCache[name] = newLogger;
                return newLogger;
            }
        }

        /// <summary>
        /// Gets or sets the current logging configuration.
        /// </summary>
        public static LoggingConfiguration Configuration
        {
            get
            {
                lock(typeof(LogManager))
                {
                    if (_configLoaded)
                        return _config;

                    _configLoaded = true;
#if !NETCF
                    if (_config == null)
                    {
                        // try to load default configuration
                        _config = XmlLoggingConfiguration.AppConfig;
                    }
                    if (_config == null)
                    {
                        string configFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "NLog.config");
                        if (File.Exists(configFile))
                        {
                            InternalLogger.Debug("Attempting to load config from {0}", configFile);
                            _config = new XmlLoggingConfiguration(configFile);
                        }
                    }
                    if (_config == null)
                    {
                        string configFile = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
                        if (configFile != null) 
                        {
                            configFile = Path.ChangeExtension(configFile, ".nlog");
                            if (File.Exists(configFile))
                            {
                                InternalLogger.Debug("Attempting to load config from {0}", configFile);
                                _config = new XmlLoggingConfiguration(configFile);
                            }
                        }
                    }
                    if (_config == null)
                    {
                        Assembly nlogAssembly = typeof(LoggingConfiguration).Assembly;
                        if (!nlogAssembly.GlobalAssemblyCache)
                        {
                            string configFile = nlogAssembly.Location + ".nlog";
                            if (File.Exists(configFile))
                            {
                                InternalLogger.Debug("Attempting to load config from {0}", configFile);
                                _config = new XmlLoggingConfiguration(configFile);
                            }
                        }
                    }

                    if (_config == null)
                    {
                        if (EnvironmentHelper.GetSafeEnvironmentVariable("NLOG_GLOBAL_CONFIG_FILE") != null)
                        {
                            string configFile = Environment.GetEnvironmentVariable("NLOG_GLOBAL_CONFIG_FILE");
                            if (File.Exists(configFile))
                            {
                                InternalLogger.Debug("Attempting to load config from {0}", configFile);
                                _config = new XmlLoggingConfiguration(configFile);
                            }
                            else
                            {
                                InternalLogger.Warn("NLog global config file pointed by NLOG_GLOBAL_CONFIG '{0}' doesn't exist.", configFile);
                            }
                        }
                    }

                    if (_config != null)
                    {
                        Dump(_config);
                        _watcher.Watch(_config.FileNamesToWatch);
                    }
#else
                    if (_config == null)
                    {
                        string configFile = CompactFrameworkHelper.GetExeFileName() + ".nlog";
                        if (File.Exists(configFile))
                        {
                            InternalLogger.Debug("Attempting to load config from {0}", configFile);
                            _config = new XmlLoggingConfiguration(configFile);
                        }
                    }
                    if (_config == null)
                    {
                        string configFile = Path.Combine(Path.GetDirectoryName(CompactFrameworkHelper.GetExeFileName()), "NLog.config");
                        if (File.Exists(configFile))
                        {
                            InternalLogger.Debug("Attempting to load config from {0}", configFile);
                            _config = new XmlLoggingConfiguration(configFile);
                        }
                    }
                    if (_config == null)
                    {
                        string configFile = typeof(LogManager).Assembly.GetName().CodeBase + ".nlog";
                        if (File.Exists(configFile))
                        {
                            InternalLogger.Debug("Attempting to load config from {0}", configFile);
                            _config = new XmlLoggingConfiguration(configFile);
                        }
                    }
#endif 
                    if (_config != null)
                    {
                        _config.InitializeAll();
                    }
                    return _config;
                }
            }

            set
            {
#if !NETCF
                try
                {
                    _watcher.StopWatching();
                }
                catch (Exception ex)
                {
                    InternalLogger.Error("Cannot stop file watching: {0}", ex);
                }
#endif 

                lock(typeof(LogManager))
                {
                    LoggingConfiguration oldConfig = _config;
                    if (oldConfig != null)
                    {
                        InternalLogger.Info("Closing old configuration.");
                        oldConfig.Close();
                    }

                    _config = value;
                    _configLoaded = true;

                    if (_config != null)
                    {
                        Dump(_config);

                        _config.InitializeAll();
                        ReconfigExistingLoggers(_config);
#if !NETCF
                        try
                        {
                            _watcher.Watch(_config.FileNamesToWatch);
                        }
                        catch (Exception ex)
                        {
                            InternalLogger.Warn("Cannot start file watching: {0}", ex);
                        }
#endif 
                    }

                }
            }
        }

#if !NETCF
        private static MultiFileWatcher _watcher = new MultiFileWatcher(new EventHandler(ConfigFileChanged));
        private static Timer _reloadTimer = null;

        const int ReconfigAfterFileChangedTimeout = 1000;

        private static void ConfigFileChanged(object sender, EventArgs args)
        {
            InternalLogger.Info("Configuration file change detected! Reloading in {0}ms...", ReconfigAfterFileChangedTimeout);

            // In the rare cases we may get multiple notifications here, 
            // but we need to reload config onlyonce.
            //
            // The trick is to schedule the reload in one second after
            // the last change notification comes in.

            lock (typeof(LogManager))
            {
                if (_reloadTimer == null)
                {
                    _reloadTimer = new Timer(new TimerCallback(ReloadConfigOnTimer), null, ReconfigAfterFileChangedTimeout, Timeout.Infinite);
                }
                else
                {
                    _reloadTimer.Change(ReconfigAfterFileChangedTimeout, Timeout.Infinite);
                }
            }
        }
#endif 

        private static void Dump(LoggingConfiguration config)
        {
            if (!InternalLogger.IsDebugEnabled)
                return;

            InternalLogger.Info("--- NLog configuration dump. ---");
            InternalLogger.Info("Targets:");
            foreach (Target target in config._targets.Values)
            {
                InternalLogger.Info("{0}", target);
            }
            InternalLogger.Info("Rules:");
            foreach (LoggingRule rule in config.LoggingRules)
            {
                InternalLogger.Info("{0}", rule);
            }
            InternalLogger.Info("--- End of NLog configuration dump ---");
        }

#if !NETCF
        internal static void ReloadConfigOnTimer(object state)
        {
            InternalLogger.Info("Reloading configuration...");
            lock(typeof(LogManager))
            {
                try
                {
                    LoggingConfiguration newConfig = Configuration.Reload();
                    if (newConfig != null)
                        Configuration = newConfig;
                    else
                    {
                        InternalLogger.Info("Configuration.Reload() returned null. Not reloading.");
                    }
                }
                catch (Exception ex)
                {
                    InternalLogger.Error("Error while reloading config file: {0}", ex);
                }

                if (_reloadTimer != null)
                {
                    _reloadTimer.Dispose();
                    _reloadTimer = null;
                }
            }
        }
#endif

        /// <summary>
        /// Loops through all loggers previously returned by <see cref="GetLogger" />
        /// and recalculates their target and filter list. Useful after modifying the configuration programmatically
        /// to ensure that all loggers have been properly configured.
        /// </summary>
        public static void ReconfigExistingLoggers()
        {
            ReconfigExistingLoggers(Configuration);
        }

        internal static void ReconfigExistingLoggers(LoggingConfiguration config)
        {
            foreach (Logger logger in _loggerCache.Values)
            {
                logger.SetConfiguration(GetConfigurationForLogger(logger.Name, config));
            }
        }

        internal static void GetTargetsByLevelForLogger(string name, LoggingRuleCollection rules, TargetWithFilterChain[]targetsByLevel, TargetWithFilterChain[]lastTargetsByLevel)
        {
            foreach (LoggingRule rule in rules)
            {
                if (rule.NameMatches(name))
                {
                    for (int i = 0; i <= LogLevel.MaxLevel.Ordinal; ++i)
                    {
                        if (i >= GlobalThreshold.Ordinal && rule.IsLoggingEnabledForLevel(LogLevel.FromOrdinal(i)))
                        {
                            foreach (Target target in rule.Targets)
                            {
                                TargetWithFilterChain awf = new TargetWithFilterChain(target, rule.Filters);
                                if (lastTargetsByLevel[i] != null)
                                {
                                    lastTargetsByLevel[i].Next = awf;
                                }
                                else
                                {
                                    targetsByLevel[i] = awf;
                                }
                                lastTargetsByLevel[i] = awf;
                            }
                        }
                    }

                    GetTargetsByLevelForLogger(name, rule.ChildRules, targetsByLevel, lastTargetsByLevel);

                    if (rule.Final)
                        break;
                }
            }
            for (int i = 0; i <= LogLevel.MaxLevel.Ordinal; ++i)
            {
                TargetWithFilterChain tfc = targetsByLevel[i];
                if (tfc != null)
                    tfc.PrecalculateNeedsStackTrace();
            }
        }

        internal static LoggerConfiguration GetConfigurationForLogger(string name, LoggingConfiguration config)
        {
            TargetWithFilterChain[]targetsByLevel = new TargetWithFilterChain[LogLevel.MaxLevel.Ordinal + 1];
            TargetWithFilterChain[]lastTargetsByLevel = new TargetWithFilterChain[LogLevel.MaxLevel.Ordinal + 1];

            if (config != null && IsLoggingEnabled())
            {
                GetTargetsByLevelForLogger(name, config.LoggingRules, targetsByLevel, lastTargetsByLevel);
            }

            InternalLogger.Debug("Targets for {0} by level:", name);
            for (int i = 0; i <= LogLevel.MaxLevel.Ordinal; ++i)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat(CultureInfo.InvariantCulture, "{0} =>", LogLevel.FromOrdinal(i));
                for (TargetWithFilterChain afc = targetsByLevel[i]; afc != null; afc = afc.Next)
                {
                    sb.AppendFormat(CultureInfo.InvariantCulture, " {0}", afc.Target.Name);
                    if (afc.FilterChain.Count > 0)
                        sb.AppendFormat(CultureInfo.InvariantCulture, " ({0} filters)", afc.FilterChain.Count);
                }
                InternalLogger.Debug(sb.ToString());
            }

            return new LoggerConfiguration(targetsByLevel);
        }

        /// <summary>
        /// Flush any pending log messages (in case of asynchronous targets).
        /// </summary>
        public static void Flush()
        {
            Configuration.FlushAllTargets(TimeSpan.MaxValue);
        }

        /// <summary>
        /// Flush any pending log messages (in case of asynchronous targets).
        /// </summary>
        /// <param name="timeout">Maximum time to allow for the flush. Any messages after that time will be discarded.</param>
        public static void Flush(TimeSpan timeout)
        {
            Configuration.FlushAllTargets(timeout);
        }

        /// <summary>
        /// Flush any pending log messages (in case of asynchronous targets).
        /// </summary>
        /// <param name="timeoutMilliseconds">Maximum time to allow for the flush. Any messages after that time will be discarded.</param>
        public static void Flush(int timeoutMilliseconds)
        {
            Configuration.FlushAllTargets(TimeSpan.FromMilliseconds(timeoutMilliseconds));
        }

        private static int _logsEnabled = 0;

        class LogEnabler: IDisposable
        {
            public static IDisposable TheEnabler = new LogEnabler();

            private LogEnabler(){}

            void IDisposable.Dispose()
            {
                LogManager.EnableLogging();
            }
        }

        /// <summary>Decreases the log enable counter and if it reaches -1 
        /// the logs are disabled.</summary>
        /// <remarks>Logging is enabled if the number of <see cref="EnableLogging"/> calls is greater 
        /// than or equal to <see cref="DisableLogging"/> calls.</remarks>
        /// <returns>An object that iplements IDisposable whose Dispose() method
        /// reenables logging. To be used with C# <c>using ()</c> statement.</returns>
        public static IDisposable DisableLogging()
        {
            lock (typeof(LogManager))
            {
                _logsEnabled--;
                if (_logsEnabled == -1)
                    ReconfigExistingLoggers();
            }
            return LogEnabler.TheEnabler;
        }

        /// <summary>Increases the log enable counter and if it reaches 0 the logs are disabled.</summary>
        /// <remarks>Logging is enabled if the number of <see cref="EnableLogging"/> calls is greater 
        /// than or equal to <see cref="DisableLogging"/> calls.</remarks>
        public static void EnableLogging()
        {
            lock (typeof(LogManager))
            {
                _logsEnabled++;
                if (_logsEnabled == 0)
                    ReconfigExistingLoggers();
            }
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
            return _logsEnabled >= 0;
        }

        /// <summary>
        /// Global log threshold. Log events below this threshold are not logged.
        /// </summary>
        public static LogLevel GlobalThreshold
        {
            get { return _globalThreshold; }
            set 
            { 
                lock(typeof(LogManager))
                {
                    _globalThreshold = value;
                    ReconfigExistingLoggers();
                }
            }
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
