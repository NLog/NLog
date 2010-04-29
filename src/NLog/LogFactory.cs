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
using System.Runtime.CompilerServices;

using NLog.Config;
using NLog.Internal;
using NLog.Targets;

namespace NLog
{
    /// <summary>
    /// Represents a method that's invoked each time a logging configuration changes.
    /// </summary>
    public delegate void LoggingConfigurationChanged(LoggingConfiguration oldConfig, LoggingConfiguration newConfig);

    /// <summary>
    /// Represents a method that's invoked each time a logging configuration gets reloaded
    /// to signal either success or failure.
    /// </summary>
    public delegate void LoggingConfigurationReloaded(bool succeeded, Exception ex);
    
    /// <summary>
    /// Creates and manages instances of <see cref="T:NLog.Logger" /> objects.
    /// </summary>
    public class LogFactory
    {
        private Hashtable _loggerCache = new Hashtable();
        private LoggingConfiguration _config;
        private LogLevel _globalThreshold = LogLevel.MinLevel;
        private bool _configLoaded = false;
        private bool _throwExceptions = false;
        private int _logsEnabled = 0;

        /// <summary>
        /// Occurs when logging <see cref="Configuration" /> changes.
        /// </summary>
        public event LoggingConfigurationChanged ConfigurationChanged;

#if !NETCF
        /// <summary>
        /// Occurs when logging <see cref="Configuration" /> gets reloaded.
        /// </summary>
        public event LoggingConfigurationReloaded ConfigurationReloaded;
#endif
        /// <summary>
        /// Specified whether NLog should throw exceptions. By default exceptions
        /// are not thrown under any circumstances.
        /// </summary>
        public bool ThrowExceptions
        {
            get { return _throwExceptions; }
            set { _throwExceptions = value; }
        }

        /// <summary>
        /// Creates a new instance of <see cref="LogFactory"/>
        /// </summary>
        public LogFactory()
        {
#if !NETCF
            _watcher = new MultiFileWatcher(new EventHandler(ConfigFileChanged));
#endif
        }

        /// <summary>
        /// Creates a new instance of <see cref="LogFactory"/> and sets the initial configuration.
        /// </summary>
        public LogFactory(LoggingConfiguration config) : this()
        {
            Configuration = config;
        }

        /// <summary>
        /// Creates a logger that discards all log messages.
        /// </summary>
        /// <returns></returns>
        public Logger CreateNullLogger()
        {
            TargetWithFilterChain[]targetsByLevel = new TargetWithFilterChain[LogLevel.MaxLevel.Ordinal + 1];
            Logger newLogger = new Logger();
            newLogger.Initialize("", new LoggerConfiguration(targetsByLevel), this);
            return newLogger;
        }

#if !NETCF
        /// <summary>
        /// Gets the logger named after the currently-being-initialized class.
        /// </summary>
        /// <returns>The logger.</returns>
        /// <remarks>This is a slow-running method. 
        /// Make sure you're not doing this in a loop.</remarks>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public Logger GetCurrentClassLogger()
        {
            StackFrame frame = new StackFrame(1, false);

            return GetLogger(frame.GetMethod().DeclaringType.FullName);
        }

        /// <summary>
        /// Gets the logger named after the currently-being-initialized class.
        /// </summary>
        /// <param name="loggerType">type of the logger to create. The type must inherit from NLog.Logger</param>
        /// <returns>The logger.</returns>
        /// <remarks>This is a slow-running method. 
        /// Make sure you're not doing this in a loop.</remarks>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public Logger GetCurrentClassLogger(Type loggerType)
        {
            StackFrame frame = new StackFrame(1, false);

            return GetLogger(frame.GetMethod().DeclaringType.FullName, loggerType);
        }
#endif

        class LoggerCacheKey
        {
            private Type _loggerConcreteType;
            private string _name;

            public LoggerCacheKey(Type loggerConcreteType, string name)
            {
                _loggerConcreteType = loggerConcreteType;
                _name = name;
            }

            public override int GetHashCode()
            {
                return _loggerConcreteType.GetHashCode() ^ _name.GetHashCode();
            }

            public override bool Equals(object o)
            {
                LoggerCacheKey lck2 = (LoggerCacheKey)o;
                if (lck2 == null)
                    return false;

                return (ConcreteType == lck2.ConcreteType) && (lck2.Name == Name);
            }

            public Type ConcreteType
            {
                get { return _loggerConcreteType; }
            }

            public string Name
            {
                get { return _name; }
            }
        }

        private Logger GetLogger(LoggerCacheKey cacheKey)
        {
            lock(this)
            {
                Logger l = (Logger)_loggerCache[cacheKey];
                if (l != null)
                    return l;

                //Activator.CreateInstance(cacheKey.ConcreteType);
                Logger newLogger;

                if (cacheKey.ConcreteType != null && cacheKey.ConcreteType != typeof(Logger))
                    newLogger = (Logger)FactoryHelper.CreateInstance(cacheKey.ConcreteType);
                else
                    newLogger = new Logger();

                if (cacheKey.ConcreteType != null)
                    
                newLogger.Initialize(cacheKey.Name, GetConfigurationForLogger(cacheKey.Name, Configuration), this);
                _loggerCache[cacheKey] = newLogger;
                return newLogger;
            }
        }

        /// <summary>
        /// Gets the specified named logger.
        /// </summary>
        /// <param name="name">name of the logger</param>
        /// <returns>The logger reference. Multiple calls to <c>GetLogger</c> with the same argument aren't guaranteed to return the same logger reference.</returns>
        public Logger GetLogger(string name)
        {
            return GetLogger(new LoggerCacheKey(typeof(Logger),name));
        }

        /// <summary>
        /// Gets the specified named logger.
        /// </summary>
        /// <param name="name">name of the logger</param>
        /// <param name="loggerType">type of the logger to create. The type must inherit from NLog.Logger</param>
        /// <returns>The logger reference. Multiple calls to <c>GetLogger</c> with the 
        /// same argument aren't guaranteed to return the same logger reference.</returns>
        public Logger GetLogger(string name, Type loggerType)
        {
            return GetLogger(new LoggerCacheKey(loggerType,name));
        }

        /// <summary>
        /// Gets or sets the current logging configuration.
        /// </summary>
        public LoggingConfiguration Configuration
        {
            get
            {
                lock(this)
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
                        string configFile = typeof(LogFactory).Assembly.GetName().CodeBase + ".nlog";
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

                lock(this)
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
                    if (ConfigurationChanged != null)
                        ConfigurationChanged(oldConfig, value);
                }
            }
        }

#if !NETCF
        private MultiFileWatcher _watcher;
        private Timer _reloadTimer = null;

        const int ReconfigAfterFileChangedTimeout = 1000;

        private void ConfigFileChanged(object sender, EventArgs args)
        {
            InternalLogger.Info("Configuration file change detected! Reloading in {0}ms...", ReconfigAfterFileChangedTimeout);

            // In the rare cases we may get multiple notifications here, 
            // but we need to reload config only once.
            //
            // The trick is to schedule the reload in one second after
            // the last change notification comes in.

            lock (this)
            {
                if (_reloadTimer == null)
                {
                    _reloadTimer = new Timer(new TimerCallback(ReloadConfigOnTimer), Configuration, ReconfigAfterFileChangedTimeout, Timeout.Infinite);
                }
                else
                {
                    _reloadTimer.Change(ReconfigAfterFileChangedTimeout, Timeout.Infinite);
                }
            }
        }
#endif 

        private void Dump(LoggingConfiguration config)
        {
            if (!InternalLogger.IsDebugEnabled)
                return;

            InternalLogger.Debug("--- NLog configuration dump. ---");
            InternalLogger.Debug("Targets:");
            foreach (Target target in config._targets.Values)
            {
                InternalLogger.Info("{0}", target);
            }
            InternalLogger.Debug("Rules:");
            foreach (LoggingRule rule in config.LoggingRules)
            {
                InternalLogger.Info("{0}", rule);
            }
            InternalLogger.Debug("--- End of NLog configuration dump ---");
        }

#if !NETCF
        internal void ReloadConfigOnTimer(object state)
        {
            LoggingConfiguration configurationToReload = (LoggingConfiguration)state;

            InternalLogger.Info("Reloading configuration...");
            lock(this)
            {
                if (_reloadTimer != null)
                {
                    _reloadTimer.Dispose();
                    _reloadTimer = null;
                }

                _watcher.StopWatching();
                try
                {
                    if (Configuration != configurationToReload)
                    {
                        throw new Exception("Config changed in between. Not reloading.");
                    }

                    LoggingConfiguration newConfig = configurationToReload.Reload();
                    if (newConfig != null)
                    {
                        Configuration = newConfig;
                        if (ConfigurationReloaded != null)
                            ConfigurationReloaded(true, null);
                    }
                    else
                    {
                        throw new Exception("Configuration.Reload() returned null. Not reloading.");
                    }
                }
                catch (Exception ex)
                {
                    _watcher.Watch(configurationToReload.FileNamesToWatch);
                    if (ConfigurationReloaded != null)
                        ConfigurationReloaded(false, ex);
                }
            }
        }
#endif

        /// <summary>
        /// Loops through all loggers previously returned by GetLogger
        /// and recalculates their target and filter list. Useful after modifying the configuration programmatically
        /// to ensure that all loggers have been properly configured.
        /// </summary>
        public void ReconfigExistingLoggers()
        {
            ReconfigExistingLoggers(Configuration);
        }

        internal void ReconfigExistingLoggers(LoggingConfiguration config)
        {
            foreach (Logger logger in _loggerCache.Values)
            {
                logger.SetConfiguration(GetConfigurationForLogger(logger.Name, config));
            }
        }

        internal void GetTargetsByLevelForLogger(string name, LoggingRuleCollection rules, TargetWithFilterChain[]targetsByLevel, TargetWithFilterChain[]lastTargetsByLevel)
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

        internal LoggerConfiguration GetConfigurationForLogger(string name, LoggingConfiguration config)
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
        public void Flush()
        {
            Configuration.FlushAllTargets(TimeSpan.MaxValue);
        }

        /// <summary>
        /// Flush any pending log messages (in case of asynchronous targets).
        /// </summary>
        /// <param name="timeout">Maximum time to allow for the flush. Any messages after that time will be discarded.</param>
        public void Flush(TimeSpan timeout)
        {
            Configuration.FlushAllTargets(timeout);
        }

        /// <summary>
        /// Flush any pending log messages (in case of asynchronous targets).
        /// </summary>
        /// <param name="timeoutMilliseconds">Maximum time to allow for the flush. Any messages after that time will be discarded.</param>
        public void Flush(int timeoutMilliseconds)
        {
            Configuration.FlushAllTargets(TimeSpan.FromMilliseconds(timeoutMilliseconds));
        }

        class LogEnabler: IDisposable
        {
            private LogFactory _factory;

            public LogEnabler(LogFactory factory)
            {
                _factory = factory;
            }

            void IDisposable.Dispose()
            {
                _factory.EnableLogging();
            }
        }

        /// <summary>Decreases the log enable counter and if it reaches -1 
        /// the logs are disabled.</summary>
        /// <remarks>Logging is enabled if the number of <see cref="EnableLogging"/> calls is greater 
        /// than or equal to <see cref="DisableLogging"/> calls.</remarks>
        /// <returns>An object that iplements IDisposable whose Dispose() method
        /// reenables logging. To be used with C# <c>using ()</c> statement.</returns>
        public IDisposable DisableLogging()
        {
            lock (this)
            {
                _logsEnabled--;
                if (_logsEnabled == -1)
                    ReconfigExistingLoggers();
            }
            return new LogEnabler(this);
        }

        /// <summary>Increases the log enable counter and if it reaches 0 the logs are disabled.</summary>
        /// <remarks>Logging is enabled if the number of <see cref="EnableLogging"/> calls is greater 
        /// than or equal to <see cref="DisableLogging"/> calls.</remarks>
        public void EnableLogging()
        {
            lock (this)
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
        public bool IsLoggingEnabled()
        {
            return _logsEnabled >= 0;
        }

        /// <summary>
        /// Global log threshold. Log events below this threshold are not logged.
        /// </summary>
        public LogLevel GlobalThreshold
        {
            get { return _globalThreshold; }
            set 
            { 
                lock(this)
                {
                    _globalThreshold = value;
                    ReconfigExistingLoggers();
                }
            }
        }
    }

#if NET_2_API
    /// <summary>
    /// Specialized LogFactory that can return instances of custom logger types.
    /// </summary>
    /// <typeparam name="LoggerType">The type of the logger to be returned. Must inherit from <see cref="Logger"/>.</typeparam>
    public class LogFactory<LoggerType> : LogFactory where LoggerType : Logger
    {
        /// <summary>
        /// Gets the logger.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>An instance of <typeparamref name="LoggerType"/>.</returns>
        public new LoggerType GetLogger(string name)
        {
            return (LoggerType)base.GetLogger(name, typeof(LoggerType));
        }

#if !NETCF
        /// <summary>
        /// Gets the logger named after the currently-being-initialized class.
        /// </summary>
        /// <returns>The logger.</returns>
        /// <remarks>This is a slow-running method. 
        /// Make sure you're not doing this in a loop.</remarks>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public new LoggerType GetCurrentClassLogger()
        {
            StackFrame frame = new StackFrame(1, false);

            return GetLogger(frame.GetMethod().DeclaringType.FullName);
        }
#endif
    
    }
#endif
}
