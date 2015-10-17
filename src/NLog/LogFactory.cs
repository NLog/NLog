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
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Threading;

#if SILVERLIGHT
    using System.Windows;
#endif
    using JetBrains.Annotations;

    using NLog.Common;
    using NLog.Config;
    using NLog.Internal;
    using NLog.Internal.Fakeables;
    using NLog.Targets;

    /// <summary>
    /// Creates and manages instances of <see cref="T:NLog.Logger" /> objects.
    /// </summary>
    public class LogFactory : IDisposable
    {
#if !SILVERLIGHT
        /// <remarks>
        /// The file (re)configuration timeout is defined in milliseconds. 
        /// </remarks>
        private const int ReconfigAfterFileChangedTimeout = 1000;

        private static TimeSpan defaultFlushTimeout = TimeSpan.FromSeconds(15);
        private Timer reloadTimer;
        private readonly MultiFileWatcher watcher;

        /// <summary>
        /// Declares if the instance is currently disposing. 
        /// </summary>
        private bool isDisposing = false;
#endif

        private static IAppDomain currentAppDomain;
        private readonly object syncRoot = new object();

        private LoggingConfiguration config;
        private bool configLoaded = false;
        private LogLevel globalThreshold = LogLevel.MinLevel;
        // TODO: logsEnabled property might be possible to be encapsulated into LogFactory.LogsEnabler class. 
        private int logsEnabled;
        private readonly LoggerCache loggerCache = new LoggerCache();

        /// <summary>
        /// Occurs when logging <see cref="Configuration" /> changes.
        /// </summary>
        public event EventHandler<LoggingConfigurationChangedEventArgs> ConfigurationChanged;

#if !SILVERLIGHT
        /// <summary>
        /// Occurs when logging <see cref="Configuration" /> gets reloaded.
        /// </summary>
        public event EventHandler<LoggingConfigurationReloadedEventArgs> ConfigurationReloaded;
#endif

        /// <summary>
        /// Initializes a new instance of the <see cref="LogFactory" /> class.
        /// </summary>
        public LogFactory()
        {
#if !SILVERLIGHT
            this.watcher = new MultiFileWatcher();
            this.watcher.OnChange += this.ConfigFileChanged;
            CurrentAppDomain.DomainUnload += CurrentAppDomainUnload;
#endif
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LogFactory" /> class.
        /// </summary>
        /// <param name="config">The config.</param>
        public LogFactory(LoggingConfiguration config)
            : this()
        {
            this.Configuration = config;
        }

        /// <summary>
        /// Gets the current <see cref="IAppDomain"/>.
        /// </summary>
        public static IAppDomain CurrentAppDomain
        {
            get { return currentAppDomain ?? (currentAppDomain = AppDomainWrapper.CurrentDomain); }
            set { currentAppDomain = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether exceptions should be thrown.
        /// </summary>
        /// <value>A value of <c>true</c> if exception should be thrown; otherwise, <c>false</c>.</value>
        /// <remarks>By default exceptions are not thrown under any circumstances.</remarks>
        public bool ThrowExceptions { get; set; }

        /// <summary>
        /// Gets or sets the current logging configuration. After setting this property all
        /// existing loggers will be re-configured, so that there is no need to call <see cref="ReconfigExistingLoggers" />
        /// manually.
        /// </summary>
        public LoggingConfiguration Configuration
        {
            get
            {
                return GetConfiguration();
            }

            set
            {
                SetConfiguration(value);
            }
        }

        /// <summary>
        /// Gets or sets the global log threshold. Log events below this threshold are not logged.
        /// </summary>
        public LogLevel GlobalThreshold
        {
            get
            {
                return this.globalThreshold;
            }

            set
            {
                lock (this.syncRoot)
                {
                    this.globalThreshold = value;
                    this.ReconfigExistingLoggers();
                }
            }
        }

        /// <summary>
        /// Gets the default culture info to use as <see cref="LogEventInfo.FormatProvider"/>.
        /// </summary>
        /// <value>
        /// Specific culture info or null to use <see cref="CultureInfo.CurrentCulture"/>
        /// </value>
        [CanBeNull]
        public CultureInfo DefaultCultureInfo
        {
            get
            {
                var configuration = this.Configuration;
                return configuration != null ? configuration.DefaultCultureInfo : null;
            }
        }
        
        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting 
        /// unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Creates a logger that discards all log messages.
        /// </summary>
        /// <returns>Null logger instance.</returns>
        public Logger CreateNullLogger()
        {
            TargetWithFilterChain[] targetsByLevel = new TargetWithFilterChain[LogLevel.MaxLevel.Ordinal + 1];
            Logger newLogger = new Logger();
            newLogger.Initialize(string.Empty, new LoggerConfiguration(targetsByLevel,false), this);
            return newLogger;
        }

        /// <summary>
        /// Gets the logger named after the currently-being-initialized class.
        /// </summary>
        /// <returns>The logger.</returns>
        /// <remarks>This is a slow-running method. 
        /// Make sure you're not doing this in a loop.</remarks>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public Logger GetCurrentClassLogger()
        {
#if SILVERLIGHT
            var frame = new StackFrame(1);
#else
            var frame = new StackFrame(1, false);
#endif

            return this.GetLogger(frame.GetMethod().DeclaringType.FullName);
        }

        /// <summary>
        /// Gets the logger named after the currently-being-initialized class.
        /// </summary>
        /// <param name="loggerType">The type of the logger to create. The type must inherit from 
        /// NLog.Logger.</param>
        /// <returns>The logger.</returns>
        /// <remarks>This is a slow-running method. Make sure you are not calling this method in a 
        /// loop.</remarks>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public Logger GetCurrentClassLogger(Type loggerType)
        {
#if !SILVERLIGHT
            var frame = new StackFrame(1, false);
#else
            var frame = new StackFrame(1);
#endif

            return this.GetLogger(frame.GetMethod().DeclaringType.FullName, loggerType);
        }

        /// <summary>
        /// Gets the specified named logger.
        /// </summary>
        /// <param name="name">Name of the logger.</param>
        /// <returns>The logger reference. Multiple calls to <c>GetLogger</c> with the same argument 
        /// are not guaranteed to return the same logger reference.</returns>
        public Logger GetLogger(string name)
        {
            return this.GetLogger(new LoggerCacheKey(name, typeof(Logger)));
        }

        /// <summary>
        /// Gets the specified named logger.
        /// </summary>
        /// <param name="name">Name of the logger.</param>
        /// <param name="loggerType">The type of the logger to create. The type must inherit from NLog.Logger.</param>
        /// <returns>The logger reference. Multiple calls to <c>GetLogger</c> with the 
        /// same argument aren't guaranteed to return the same logger reference.</returns>
        public Logger GetLogger(string name, Type loggerType)
        {
            return this.GetLogger(new LoggerCacheKey(name, loggerType));
        }

        /// <summary>
        /// Loops through all loggers previously returned by GetLogger and recalculates their 
        /// target and filter list. Useful after modifying the configuration programmatically
        /// to ensure that all loggers have been properly configured.
        /// </summary>
        public void ReconfigExistingLoggers()
        {
            if (this.config != null)
            {
                this.config.InitializeAll();
            }

            //new list to avoid "Collection was modified; enumeration operation may not execute"
            var loggers = new List<Logger>(loggerCache.Loggers);
            foreach (var logger in loggers)
            {
                logger.SetConfiguration(this.GetConfigurationForLogger(logger.Name, this.config));
            }
        }

#if !SILVERLIGHT
        /// <summary>
        /// Flush any pending log messages (in case of asynchronous targets).
        /// </summary>
        public void Flush()
        {
            this.Flush(defaultFlushTimeout);
        }

        /// <summary>
        /// Flush any pending log messages (in case of asynchronous targets).
        /// </summary>
        /// <param name="timeout">Maximum time to allow for the flush. Any messages after that time 
        /// will be discarded.</param>
        public void Flush(TimeSpan timeout)
        {
            try
            {
                AsyncHelpers.RunSynchronously(cb => this.Flush(cb, timeout));
            }
            catch (Exception e)
            {
                if (ThrowExceptions)
                {
                    throw;
                }

                InternalLogger.Error(e.ToString());
            }
        }

        /// <summary>
        /// Flush any pending log messages (in case of asynchronous targets).
        /// </summary>
        /// <param name="timeoutMilliseconds">Maximum time to allow for the flush. Any messages 
        /// after that time will be discarded.</param>
        public void Flush(int timeoutMilliseconds)
        {
            this.Flush(TimeSpan.FromMilliseconds(timeoutMilliseconds));
        }
#endif

        /// <summary>
        /// Flush any pending log messages (in case of asynchronous targets).
        /// </summary>
        /// <param name="asyncContinuation">The asynchronous continuation.</param>
        public void Flush(AsyncContinuation asyncContinuation)
        {
            this.Flush(asyncContinuation, TimeSpan.MaxValue);
        }

        /// <summary>
        /// Flush any pending log messages (in case of asynchronous targets).
        /// </summary>
        /// <param name="asyncContinuation">The asynchronous continuation.</param>
        /// <param name="timeoutMilliseconds">Maximum time to allow for the flush. Any messages 
        /// after that time will be discarded.</param>
        public void Flush(AsyncContinuation asyncContinuation, int timeoutMilliseconds)
        {
            this.Flush(asyncContinuation, TimeSpan.FromMilliseconds(timeoutMilliseconds));
        }

        /// <summary>
        /// Flush any pending log messages (in case of asynchronous targets).
        /// </summary>
        /// <param name="asyncContinuation">The asynchronous continuation.</param>
        /// <param name="timeout">Maximum time to allow for the flush. Any messages after that time will be discarded.</param>
        public void Flush(AsyncContinuation asyncContinuation, TimeSpan timeout)
        {
            try
            {
                InternalLogger.Trace("LogFactory.Flush({0})", timeout);

                var loggingConfiguration = this.Configuration;
                if (loggingConfiguration != null)
                {
                    InternalLogger.Trace("Flushing all targets...");
                    loggingConfiguration.FlushAllTargets(AsyncHelpers.WithTimeout(asyncContinuation, timeout));
                }
                else
                {
                    asyncContinuation(null);
                }
            }
            catch (Exception e)
            {
                if (ThrowExceptions)
                {
                    throw;
                }

                InternalLogger.Error(e.ToString());
            }
        }

        /// <summary>
        /// Decreases the log enable counter and if it reaches -1 the logs are disabled.
        /// </summary>
        /// <remarks>
        /// Logging is enabled if the number of <see cref="ResumeLogging"/> calls is greater than 
        /// or equal to <see cref="SuspendLogging"/> calls.
        /// </remarks>
        /// <returns>An object that implements IDisposable whose Dispose() method re-enables logging. 
        /// To be used with C# <c>using ()</c> statement.</returns>
        [Obsolete("Use SuspendLogging() instead. This option will be removed in NLog 5")]
        public IDisposable DisableLogging()
        {
            return SuspendLogging();
        }

        /// <summary>
        /// Increases the log enable counter and if it reaches 0 the logs are disabled.
        /// </summary>
        /// <remarks>
        /// Logging is enabled if the number of <see cref="ResumeLogging"/> calls is greater than 
        /// or equal to <see cref="SuspendLogging"/> calls.</remarks>
        [Obsolete("Use ResumeLogging() instead. This option will be removed in NLog 5")]
        public void EnableLogging()
        {
            ResumeLogging();
        }

        /// <summary>
        /// Decreases the log enable counter and if it reaches -1 the logs are disabled.
        /// </summary>
        /// <remarks>
        /// Logging is enabled if the number of <see cref="ResumeLogging"/> calls is greater than 
        /// or equal to <see cref="SuspendLogging"/> calls.
        /// </remarks>
        /// <returns>An object that implements IDisposable whose Dispose() method re-enables logging. 
        /// To be used with C# <c>using ()</c> statement.</returns>
        public IDisposable SuspendLogging()
        {
            lock (this.syncRoot)
            {
                this.logsEnabled--;
                if (this.logsEnabled == -1)
                {
                    this.ReconfigExistingLoggers();
                }
            }

            return new LogEnabler(this);
        }

        /// <summary>
        /// Increases the log enable counter and if it reaches 0 the logs are disabled.
        /// </summary>
        /// <remarks>Logging is enabled if the number of <see cref="ResumeLogging"/> calls is greater 
        /// than or equal to <see cref="SuspendLogging"/> calls.</remarks>
        public void ResumeLogging()
        {
            lock (this.syncRoot)
            {
                this.logsEnabled++;
                if (this.logsEnabled == 0)
                {
                    this.ReconfigExistingLoggers();
                }
            }
        }

        /// <summary>
        /// Returns <see langword="true" /> if logging is currently enabled.
        /// </summary>
        /// <returns>A value of <see langword="true" /> if logging is currently enabled, 
        /// <see langword="false"/> otherwise.</returns>
        /// <remarks>Logging is enabled if the number of <see cref="ResumeLogging"/> calls is greater 
        /// than or equal to <see cref="SuspendLogging"/> calls.</remarks>
        public bool IsLoggingEnabled()
        {
            return this.logsEnabled >= 0;
        }

        /// <summary>
        /// Invoke the Changed event; called whenever list changes
        /// </summary>
        /// <param name="e">Event arguments.</param>
        protected virtual void OnConfigurationChanged(LoggingConfigurationChangedEventArgs e)
        {
            var changed = this.ConfigurationChanged;
            if (changed != null)
            {
                changed(this, e);
            }
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>True</c> to release both managed and unmanaged resources;
        /// <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
#if !SILVERLIGHT
            if (disposing)
            {
                this.watcher.Dispose();

                if (this.reloadTimer != null)
                {
                    this.reloadTimer.Dispose();
                    this.reloadTimer = null;
                }
            }
#endif
        }

#if !SILVERLIGHT
        /// <remarks>
        /// This method does NOT need to internal as it is only referenced from this class. The only reason it remains
        /// as internal is to be accesible to Unit Tests.
        /// </remarks>
        internal void ReloadConfigOnTimer(object state)
        {
            LoggingConfiguration configurationToReload = (LoggingConfiguration)state;

            InternalLogger.Info("Reloading configuration...");
            lock (this.syncRoot)
            {
                if (this.reloadTimer != null)
                {
                    this.reloadTimer.Dispose();
                    this.reloadTimer = null;
                }
                
                if(isDisposing)
                {
                    //timer was disposed already. 
                    this.watcher.Dispose();
                    return;
                }

                this.watcher.StopWatching();
                try
                {
                    if (this.Configuration != configurationToReload)
                    {
                        throw new NLogConfigurationException("Config changed in between. Not reloading.");
                    }

                    LoggingConfiguration newConfig = configurationToReload.Reload();

                    //problem: XmlLoggingConfiguration.Initialize eats exception with invalid XML. ALso XmlLoggingConfiguration.Reload never returns null.
                    //therefor we check the InitializeSucceeded property.

                    var xmlConfig = newConfig as XmlLoggingConfiguration;
                    if (xmlConfig != null)
                    {

                        if (!xmlConfig.InitializeSucceeded.HasValue || !xmlConfig.InitializeSucceeded.Value)
                        {
                            throw new NLogConfigurationException("Configuration.Reload() failed. Invalid XML?");
                        }
                    }

                    if (newConfig != null)
                    {
                        this.Configuration = newConfig;
                        if (this.ConfigurationReloaded != null)
                        {
                            this.ConfigurationReloaded(this, new LoggingConfigurationReloadedEventArgs(true, null));
                        }
                    }
                    else
                    {
                        throw new NLogConfigurationException("Configuration.Reload() returned null. Not reloading.");
                    }
                }
                catch (Exception exception)
                {
                    if (exception is NLogConfigurationException)
                    {
                        InternalLogger.Warn(exception.Message);
                    }
                    else if (exception.MustBeRethrown())
                    {
                        throw;
                    }

                    this.watcher.Watch(configurationToReload.FileNamesToWatch);

                    var configurationReloadedDelegate = this.ConfigurationReloaded;
                    if (configurationReloadedDelegate != null)
                    {
                        configurationReloadedDelegate(this, new LoggingConfigurationReloadedEventArgs(false, exception));
                    }
                }
            }
        }
#endif
        /// <remarks>
        /// This method does NOT need to internal as it is only referenced from this class. The only reason it remains
        /// as internal is to be accesible to Unit Tests.
        /// </remarks>
        internal LoggerConfiguration GetConfigurationForLogger(string name, LoggingConfiguration configuration)
        {
            TargetWithFilterChain[] targetsByLevel = new TargetWithFilterChain[LogLevel.MaxLevel.Ordinal + 1];
            TargetWithFilterChain[] lastTargetsByLevel = new TargetWithFilterChain[LogLevel.MaxLevel.Ordinal + 1];
            bool[] suppressedLevels = new bool[LogLevel.MaxLevel.Ordinal + 1];

            if (configuration != null && this.IsLoggingEnabled())
            {
                this.GetTargetsByLevelForLogger(name, configuration.LoggingRules, targetsByLevel, lastTargetsByLevel, suppressedLevels);
            }

            InternalLogger.Debug("Targets for {0} by level:", name);
            for (int i = 0; i <= LogLevel.MaxLevel.Ordinal; ++i)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat(CultureInfo.InvariantCulture, "{0} =>", LogLevel.FromOrdinal(i));
                for (TargetWithFilterChain afc = targetsByLevel[i]; afc != null; afc = afc.NextInChain)
                {
                    sb.AppendFormat(CultureInfo.InvariantCulture, " {0}", afc.Target.Name);
                    if (afc.FilterChain.Count > 0)
                    {
                        sb.AppendFormat(CultureInfo.InvariantCulture, " ({0} filters)", afc.FilterChain.Count);
                    }
                }

                InternalLogger.Debug(sb.ToString());
            }

#pragma warning disable 618
            return new LoggerConfiguration(targetsByLevel, configuration != null && configuration.ExceptionLoggingOldStyle);
#pragma warning restore 618
        }

        private void GetTargetsByLevelForLogger(string name, IEnumerable<LoggingRule> rules, TargetWithFilterChain[] targetsByLevel, TargetWithFilterChain[] lastTargetsByLevel, bool[] suppressedLevels)
        {
            foreach (LoggingRule rule in rules)
            {
                if (!rule.NameMatches(name))
                {
                    continue;
                }

                for (int i = 0; i <= LogLevel.MaxLevel.Ordinal; ++i)
                {
                    if (i < this.GlobalThreshold.Ordinal || suppressedLevels[i] || !rule.IsLoggingEnabledForLevel(LogLevel.FromOrdinal(i)))
                    {
                        continue;
                    }

                    if (rule.Final)
                        suppressedLevels[i] = true;

                    foreach (Target target in rule.Targets)
                    {
                        var awf = new TargetWithFilterChain(target, rule.Filters);
                        if (lastTargetsByLevel[i] != null)
                        {
                            lastTargetsByLevel[i].NextInChain = awf;
                        }
                        else
                        {
                            targetsByLevel[i] = awf;
                        }

                        lastTargetsByLevel[i] = awf;
                    }
                }

                // Recursively analyze the child rules.
                this.GetTargetsByLevelForLogger(name, rule.ChildRules, targetsByLevel, lastTargetsByLevel, suppressedLevels);

            }

            for (int i = 0; i <= LogLevel.MaxLevel.Ordinal; ++i)
            {
                TargetWithFilterChain tfc = targetsByLevel[i];
                if (tfc != null)
                {
                    tfc.PrecalculateStackTraceUsage();
                }
            }
        }

        private static IEnumerable<string> GetCandidateConfigFileNames()
        {
#if SILVERLIGHT
            yield return "NLog.config";
#else
            // NLog.config from application directory
            if (CurrentAppDomain.BaseDirectory != null)
            {
                yield return Path.Combine(CurrentAppDomain.BaseDirectory, "NLog.config");
            }

            // Current config file with .config renamed to .nlog
            string cf = CurrentAppDomain.ConfigurationFile;
            if (cf != null)
            {
                yield return Path.ChangeExtension(cf, ".nlog");

                // .nlog file based on the non-vshost version of the current config file
                const string vshostSubStr = ".vshost.";
                if (cf.Contains(vshostSubStr))
                {
                    yield return Path.ChangeExtension(cf.Replace(vshostSubStr, "."), ".nlog");
                }

                IEnumerable<string> privateBinPaths = CurrentAppDomain.PrivateBinPath;
                if (privateBinPaths != null)
                {
                    foreach (var path in privateBinPaths)
                    {
                        if (path != null)
                        {
                            yield return Path.Combine(path, "NLog.config");
                        }
                    }
                }
            }

            // Get path to NLog.dll.nlog only if the assembly is not in the GAC
            var nlogAssembly = typeof(LogFactory).Assembly;
            if (!nlogAssembly.GlobalAssemblyCache)
            {
                if (!string.IsNullOrEmpty(nlogAssembly.Location))
                {
                    yield return nlogAssembly.Location + ".nlog";
                }
            }
#endif
        }

        private Logger GetLogger(LoggerCacheKey cacheKey)
        {
            lock (this.syncRoot)
            {
                Logger existingLogger = loggerCache.Retrieve(cacheKey);
                if (existingLogger != null)
                {
                    // Logger is still in cache and referenced.
                    return existingLogger;
                }

                Logger newLogger;

                if (cacheKey.ConcreteType != null && cacheKey.ConcreteType != typeof(Logger))
                {
                    try
                    {
                        newLogger = (Logger)FactoryHelper.CreateInstance(cacheKey.ConcreteType);
                    }
                    catch (Exception ex)
                    {
                        if (ex.MustBeRethrown() || ThrowExceptions)
                        {
                            throw;
                        }

                        InternalLogger.Error("Cannot create instance of specified type. Proceeding with default type instance. Exception : {0}", ex);

                        // Creating default instance of logger if instance of specified type cannot be created.
                        cacheKey = new LoggerCacheKey(cacheKey.Name, typeof(Logger));

                        newLogger = new Logger();
                    }
                }
                else
                {
                    newLogger = new Logger();
                }

                if (cacheKey.ConcreteType != null)
                {
                    newLogger.Initialize(cacheKey.Name, this.GetConfigurationForLogger(cacheKey.Name, this.Configuration), this);
                }

                // TODO: Clarify what is the intention when cacheKey.ConcreteType = null.
                //      At the moment, a logger typeof(Logger) will be created but the ConcreteType 
                //      will remain null and inserted into the cache. 
                //      Should we set cacheKey.ConcreteType = typeof(Logger) for default loggers?

                loggerCache.InsertOrUpdate(cacheKey, newLogger);
                return newLogger;
            }
        }

#if !SILVERLIGHT
        private void ConfigFileChanged(object sender, EventArgs args)
        {
            InternalLogger.Info("Configuration file change detected! Reloading in {0}ms...", LogFactory.ReconfigAfterFileChangedTimeout);

            // In the rare cases we may get multiple notifications here, 
            // but we need to reload config only once.
            //
            // The trick is to schedule the reload in one second after
            // the last change notification comes in.
            lock (this.syncRoot)
            {
                if (this.reloadTimer == null)
                {
                    this.reloadTimer = new Timer(
                            this.ReloadConfigOnTimer,
                            this.Configuration,
                            LogFactory.ReconfigAfterFileChangedTimeout,
                            Timeout.Infinite);
                }
                else
                {
                    this.reloadTimer.Change(
                            LogFactory.ReconfigAfterFileChangedTimeout,
                            Timeout.Infinite);
                }
            }
        }
#endif

        private void ConfigurationInitialized()
        {
            InternalLogger.Info("Configuration initialized.");
            InternalLogger.LogAssemblyVersion(typeof(ILogger).Assembly);
        }

        private void LoadConfiguration(string configFile)
        {
            InternalLogger.Debug("Loading config from {0}", configFile);
            this.config = new XmlLoggingConfiguration(configFile);
        }

        private LoggingConfiguration GetConfiguration()
        {
            lock (syncRoot)
            {
                if (configLoaded)
                {
                    return config;
                }

                configLoaded = true;

#if !SILVERLIGHT
                if (config == null)
                {
                    // Try to load default configuration.
                    config = XmlLoggingConfiguration.AppConfig;
                }
#endif
                // Retest the condition as we might have loaded a config.
                if (config == null)
                {
                    foreach (string configFile in GetCandidateConfigFileNames())
                    {
#if SILVERLIGHT
                        Uri configFileUri = new Uri(configFile, UriKind.Relative);
                        if (Application.GetResourceStream(configFileUri) != null)
                        {
                            LoadConfiguration(configFile);
                            break;
                        }
#else
                        if (File.Exists(configFile))
                        {
                            LoadConfiguration(configFile);
                            break;
                        }
#endif
                    }
                }

                if (config != null)
                {
#if !SILVERLIGHT
                    config.Dump();
                    try
                    {
                        watcher.Watch(config.FileNamesToWatch);
                    }
                    catch (Exception exception)
                    {
                        InternalLogger.Warn("Cannot start file watching: {0}. File watching is disabled", exception);
                    }
#endif
                    config.InitializeAll();
                    ConfigurationInitialized();
                }

                return config;
            }
        }

        private void SetConfiguration(LoggingConfiguration value)
        {
#if !SILVERLIGHT
            try
            {
                watcher.StopWatching();
            }
            catch (Exception exception)
            {
                if (exception.MustBeRethrown())
                {
                    throw;
                }

                InternalLogger.Error("Cannot stop file watching: {0}", exception);
            }
#endif

            lock (syncRoot)
            {
                LoggingConfiguration oldConfig = config;
                if (oldConfig != null)
                {
                    InternalLogger.Info("Closing old configuration.");
#if !SILVERLIGHT
                    Flush();
#endif
                    oldConfig.Close();
                }

                config = value;
                configLoaded = true;

                if (config != null)
                {
                    config.Dump();

                    config.InitializeAll();
                    ReconfigExistingLoggers();
#if !SILVERLIGHT
                    try
                    {
                        watcher.Watch(config.FileNamesToWatch);
                    }
                    catch (Exception exception)
                    {
                        if (exception.MustBeRethrown())
                        {
                            throw;
                        }

                        InternalLogger.Warn("Cannot start file watching: {0}", exception);
                    }
#endif
                }

                OnConfigurationChanged(new LoggingConfigurationChangedEventArgs(value, oldConfig));
            }
        }

#if !SILVERLIGHT
        private void CurrentAppDomainUnload(object sender, EventArgs e)
        {
            //stop timer on domain unload, otherwise: 
            //Exception: System.AppDomainUnloadedException
            //Message: Attempted to access an unloaded AppDomain.
            lock (this.syncRoot)
            {
                isDisposing = true;
                if (this.reloadTimer != null)
                {
                    this.reloadTimer.Dispose();
                    this.reloadTimer = null;
                }
            }
        }
#endif

        /// <summary>
        /// Logger cache key.
        /// </summary>
        private sealed class LoggerCacheKey : IEquatable<LoggerCacheKey>
        {
            private readonly string name;
            private readonly Type concreteType;

            /// <summary>
            /// Name associated with a <see cref="Logger"/>.
            /// </summary>
            public string Name
            {
                get
                {
                    return name;
                }
            }

            /// <summary>
            /// The concrete type of associated with a <see cref="Logger"/>. 
            /// </summary>
            public Type ConcreteType
            {
                get
                {
                    return concreteType;
                }
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="LoggerCacheKey" /> class.
            /// </summary>
            /// <param name="name">Name associated with a <see cref="Logger"/>.</param>
            /// <param name="concreteType">The concrete type of associated with a <see cref="Logger"/>.</param>
            public LoggerCacheKey(string name, Type concreteType)
            {
                this.name = name;
                this.concreteType = concreteType;
            }

            /// <summary>
            /// Serves as a hash function for a particular type.
            /// </summary>
            /// <returns>A hash code for the current <see cref="T:System.Object"/>.</returns>
            public override int GetHashCode()
            {
                return ConcreteType.GetHashCode() ^ Name.GetHashCode();
            }

            /// <summary>
            /// Determines if two objects are equal in value.
            /// </summary>
            /// <param name="obj">Other <see cref="T:System.Object"/> to compare to.</param>
            /// <returns><see langword="true" /> if objects are equal, <see langword="false" /> otherwise.</returns>
            public override bool Equals(object obj)
            {
                LoggerCacheKey key = obj as LoggerCacheKey;
                if (ReferenceEquals(key, null))
                {
                    return false;
                }

                return (ConcreteType == key.ConcreteType) && (Name == key.Name);
            }

            /// <summary>
            /// Determines if two objects of the same type are equal in value.
            /// </summary>
            /// <param name="key">Other <see cref="LoggerCacheKey"/> to compare to.</param>
            /// <returns><see langword="true" /> if objects are equal, <see langword="false" /> otherwise.</returns>
            public bool Equals(LoggerCacheKey key)
            {
                if (ReferenceEquals(key, null))
                {
                    return false;
                }

                return (ConcreteType == key.ConcreteType) && (Name == key.Name);
            }
        }

        /// <summary>
        /// Logger cache.
        /// </summary>
        private sealed class LoggerCache
        {
            // The values of WeakReferences are of type Logger i.e. Directory<LoggerCacheKey, Logger>.
            private readonly Dictionary<LoggerCacheKey, WeakReference> loggerCache =
                    new Dictionary<LoggerCacheKey, WeakReference>();

            /// <summary>
            /// Inserts the <see cref="Logger"/> value if the <see cref="LoggerCacheKey"/> is not in the collection or
            /// otherwise updates the existing entry.
            /// </summary>
            /// <param name="cacheKey">The object to use as the key of the element to add.</param>
            /// <param name="logger">The object to use as the value of the element to add.</param>
            public void InsertOrUpdate(LoggerCacheKey cacheKey, Logger logger)
            {
                loggerCache[cacheKey] = new WeakReference(logger);
            }

            /// <summary>
            /// Retrieves and returns the <see cref="Logger"/> for the specified <see cref="LoggerCacheKey"/> key.
            /// </summary>
            /// <param name="cacheKey">The object to use as the key of the element to retrieve.</param>
            /// <returns>
            /// Returns the <see cref="Logger"/> when the <see cref="LoggerCacheKey"/> is found in the collection, <see
            /// langword="null"/> otherwise.
            /// </returns>
            public Logger Retrieve(LoggerCacheKey cacheKey)
            {
                WeakReference loggerReference;
                if (loggerCache.TryGetValue(cacheKey, out loggerReference))
                {
                    // logger in the cache and still referenced
                    return loggerReference.Target as Logger;
                }

                return null;
            }

            /// <summary>
            /// Get all the <see cref="Logger"/> availiable in the collection.
            /// </summary>
            public IEnumerable<Logger> Loggers
            {
                get { return GetLoggers(); }
            }

            private IEnumerable<Logger> GetLoggers()
            {
                // TODO: Test if loggerCache.Values.ToList<Logger>() can be used for the conversion instead.
                List<Logger> values = new List<Logger>(loggerCache.Count);

                foreach (WeakReference loggerReference in loggerCache.Values)
                {
                    Logger logger = loggerReference.Target as Logger;
                    if (logger != null)
                    {
                        values.Add(logger);
                    }
                }

                return values;
            }
        }

        /// <summary>
        /// Enables logging in <see cref="IDisposable.Dispose"/> implementation.
        /// </summary>
        private sealed class LogEnabler : IDisposable
        {
            private readonly LogFactory factory;

            /// <summary>
            /// Initializes a new instance of the <see cref="LogEnabler" /> class.
            /// </summary>
            /// <param name="factory">The factory.</param>
            public LogEnabler(LogFactory factory)
            {
                this.factory = factory;
            }

            /// <summary>
            /// Enables logging.
            /// </summary>
            void IDisposable.Dispose()
            {
                factory.ResumeLogging();
            }
        }
    }
}
