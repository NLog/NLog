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
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Threading;

    using JetBrains.Annotations;

    using NLog.Common;
    using NLog.Config;
    using NLog.Internal;
    using NLog.Targets;
    using NLog.Internal.Fakeables;
    using System.Reflection;

#if SILVERLIGHT && !__IOS__ && !__ANDROID__
    using System.Windows;
#endif

    /// <summary>
    /// Creates and manages instances of <see cref="T:NLog.Logger" /> objects.
    /// </summary>
    public class LogFactory : IDisposable
    {
#if !SILVERLIGHT && !__IOS__ && !__ANDROID__
        private const int ReconfigAfterFileChangedTimeout = 1000;
        private Timer reloadTimer;
        private readonly MultiFileWatcher watcher;
#endif

        private static TimeSpan defaultFlushTimeout = TimeSpan.FromSeconds(15);


        private static IAppDomain currentAppDomain;
        private readonly object syncRoot = new object();

        private LoggingConfiguration config;
        private LogLevel globalThreshold = LogLevel.MinLevel;
        private bool configLoaded;
        // TODO: logsEnabled property might be possible to be encapsulated into LogFactory.LogsEnabler class. 
        private int logsEnabled;
        private readonly LoggerCache loggerCache = new LoggerCache();

        /// <summary>
        /// Occurs when logging <see cref="Configuration" /> changes.
        /// </summary>
        public event EventHandler<LoggingConfigurationChangedEventArgs> ConfigurationChanged;

#if !SILVERLIGHT && !__IOS__ && !__ANDROID__
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
#if !SILVERLIGHT && !__IOS__ && !__ANDROID__
            this.watcher = new MultiFileWatcher();
            this.watcher.OnChange += this.ConfigFileChanged;
            CurrentAppDomain.DomainUnload += currentAppDomain_DomainUnload;
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
        /// Gets or sets a value indicating whether exceptions should be thrown. See also <see cref="ThrowConfigExceptions"/>.
        /// </summary>
        /// <value>A value of <c>true</c> if exception should be thrown; otherwise, <c>false</c>.</value>
        /// <remarks>By default exceptions are not thrown under any circumstances.</remarks>
        public bool ThrowExceptions { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether <see cref="NLogConfigurationException"/> should be thrown.
        /// 
        /// If <c>null</c> then <see cref="ThrowExceptions"/> is used.
        /// </summary>
        /// <value>A value of <c>true</c> if exception should be thrown; otherwise, <c>false</c>.</value>
        /// <remarks>
        /// This option is for backwards-compatiblity.
        /// By default exceptions are not thrown under any circumstances.
        /// 
        /// </remarks>
        public bool? ThrowConfigExceptions { get; set; }


        /// <summary>
        /// Gets or sets the current logging configuration. After setting this property all
        /// existing loggers will be re-configured, so that there is no need to call <see cref="ReconfigExistingLoggers" />
        /// manually.
        /// </summary>
        public LoggingConfiguration Configuration
        {
            get
            {
                if (this.configLoaded)
                    return this.config;

                lock (this.syncRoot)
                {
                    if (this.configLoaded)
                        return this.config;
                    
#if !SILVERLIGHT  && !__IOS__ && !__ANDROID__
                    if (this.config == null)
                    {
                        // Try to load default configuration.
                        this.config = XmlLoggingConfiguration.AppConfig;
                    }
#endif
                    // Retest the condition as we might have loaded a config.
                    if (this.config == null)
                    {
                        foreach (string configFile in GetCandidateConfigFileNames())
                        {
#if SILVERLIGHT
                            Uri configFileUri = new Uri(configFile, UriKind.Relative);
                            if (Application.GetResourceStream(configFileUri) != null)
                            {
                                LoadLoggingConfiguration(configFile);
                                break;
                            }
#else
                            if (File.Exists(configFile))
                            {
                                LoadLoggingConfiguration(configFile);
                                break;
                            }
#endif
                        }
                    }

#if __ANDROID__
                    if (this.config == null)
                    {
                        //try nlog.config in assets folder
                        const string nlogConfigFilename = "NLog.config";
                        try
                        {
                            using (var stream = Android.App.Application.Context.Assets.Open(nlogConfigFilename))
                            {
                                if (stream != null)
                                {
                                    LoadLoggingConfiguration(XmlLoggingConfiguration.AssetsPrefix + nlogConfigFilename);
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            InternalLogger.Trace(e, "no {0} in assets folder", nlogConfigFilename);
                        }

                    }
#endif

                    if (this.config != null)
                    {
                        try
                        {
#if !SILVERLIGHT && !__IOS__ && !__ANDROID__
                            config.Dump();
                            try
                            {
                                this.watcher.Watch(this.config.FileNamesToWatch);
                            }
                            catch (Exception exception)
                            {
                                if (exception.MustBeRethrownImmediately())
                                {
                                    throw;
                                }

                                InternalLogger.Warn(exception, "Cannot start file watching. File watching is disabled");
                                //TODO NLog 5: check "MustBeRethrown" 
                            }
#endif
                            this.config.InitializeAll();
                            LogConfigurationInitialized();
                        }
                        finally
                        {
                            this.configLoaded = true;
                        }
                    }

                    return this.config;
                }
            }

            set
            {
#if !SILVERLIGHT && !__IOS__ && !__ANDROID__
                try
                {
                    this.watcher.StopWatching();
                }
                catch (Exception exception)
                {
                    InternalLogger.Error(exception, "Cannot stop file watching.");

                    if (exception.MustBeRethrown())
                    {
                        throw;
                    }
                }
#endif

                lock (this.syncRoot)
                {
                    LoggingConfiguration oldConfig = this.config;
                    if (oldConfig != null)
                    {
                        InternalLogger.Info("Closing old configuration.");
#if !SILVERLIGHT
                        this.Flush();
#endif
                        oldConfig.Close();
                    }

                    this.config = value;

                    if (this.config != null)
                    {
                        try
                        {
                            config.Dump();

                            this.config.InitializeAll();
                            this.ReconfigExistingLoggers();
#if !SILVERLIGHT && !__IOS__ && !__ANDROID__
                            try
                            {
                                this.watcher.Watch(this.config.FileNamesToWatch);
                            }
                            catch (Exception exception)
                            {
                                //ToArray needed for .Net 3.5
                                InternalLogger.Warn(exception, "Cannot start file watching: {0}", string.Join(",", this.config.FileNamesToWatch.ToArray()));

                                if (exception.MustBeRethrown())
                                {
                                    throw;
                                }
                            }
#endif
                        }
                        finally
                        {
                            this.configLoaded = true;
                        }
                    }

                    this.OnConfigurationChanged(new LoggingConfigurationChangedEventArgs(value, oldConfig));
                }
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

        private void LogConfigurationInitialized()
        {
            InternalLogger.Info("Configuration initialized.");
            InternalLogger.LogAssemblyVersion(typeof(ILogger).Assembly);
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
            newLogger.Initialize(string.Empty, new LoggerConfiguration(targetsByLevel, false), this);
            return newLogger;
        }

        /// <summary>
        /// Gets the logger with the name of the current class. 
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
        /// Gets the logger with the name of the current class. 
        /// </summary>
        /// <returns>The logger with type <typeparamref name="T"/>.</returns>
        /// <typeparam name="T">Type of the logger</typeparam>
        /// <remarks>This is a slow-running method. 
        /// Make sure you're not doing this in a loop.</remarks>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public T GetCurrentClassLogger<T>() where T : Logger
        {
#if SILVERLIGHT
            var frame = new StackFrame(1);
#else
            var frame = new StackFrame(1, false);
#endif

            return (T)this.GetLogger(frame.GetMethod().DeclaringType.FullName, typeof(T));
        }

        /// <summary>
        /// Gets a custom logger with the name of the current class. Use <paramref name="loggerType"/> to pass the type of the needed Logger.
        /// </summary>
        /// <param name="loggerType">The type of the logger to create. The type must inherit from <see cref="Logger"/></param>
        /// <returns>The logger of type <paramref name="loggerType"/>.</returns>
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
        /// <typeparam name="T">Type of the logger</typeparam>
        /// <returns>The logger reference with type <typeparamref name="T"/>. Multiple calls to <c>GetLogger</c> with the same argument 
        /// are not guaranteed to return the same logger reference.</returns>
        public T GetLogger<T>(string name) where T : Logger
        {
            return (T)this.GetLogger(new LoggerCacheKey(name, typeof(T)));
        }

        /// <summary>
        /// Gets the specified named logger.  Use <paramref name="loggerType"/> to pass the type of the needed Logger.
        /// </summary>
        /// <param name="name">Name of the logger.</param>
        /// <param name="loggerType">The type of the logger to create. The type must inherit from <see cref="Logger" />.</param>
        /// <returns>The logger of type <paramref name="loggerType"/>. Multiple calls to <c>GetLogger</c> with the 
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
            catch (Exception ex)
            {
                InternalLogger.Error(ex, "Error with flush.");
                if (ex.MustBeRethrown())
                {
                    throw;
                }

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
            catch (Exception ex)
            {
                if (ThrowExceptions)
                {
                    throw;
                }

                InternalLogger.Error(ex, "Error with flush.");
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
        [Obsolete("Use SuspendLogging() instead.")]
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
        [Obsolete("Use ResumeLogging() instead.")]
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

#if !SILVERLIGHT && !__IOS__ && !__ANDROID__
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

                if (IsDisposing)
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
                    //special case, don't rethrow NLogConfigurationException
                    if (exception is NLogConfigurationException)
                    {
                        InternalLogger.Warn(exception, "NLog configuration while reloading");
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
        private void GetTargetsByLevelForLogger(string name, IEnumerable<LoggingRule> rules, TargetWithFilterChain[] targetsByLevel, TargetWithFilterChain[] lastTargetsByLevel, bool[] suppressedLevels)
        {
            //no "System.InvalidOperationException: Collection was modified"
            var loggingRules = new List<LoggingRule>(rules);
            foreach (LoggingRule rule in loggingRules)
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

                    foreach (Target target in rule.Targets.ToList())
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

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>True</c> to release both managed and unmanaged resources;
        /// <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
#if !SILVERLIGHT && !__IOS__ && !__ANDROID__
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

        private static IEnumerable<string> GetCandidateConfigFileNames()
        {
#if SILVERLIGHT || __ANDROID__ || __IOS__
            //try.nlog.config is ios/android/silverlight
            yield return "NLog.config";
#elif !SILVERLIGHT
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
                    var fullName = cacheKey.ConcreteType.FullName;
                    try
                    {

                        //creating instance of static class isn't possible, and also not wanted (it cannot inherited from Logger)
                        if (cacheKey.ConcreteType.IsStaticClass())
                        {
                            var errorMessage = string.Format("GetLogger / GetCurrentClassLogger is '{0}' as loggerType can be a static class and should inherit from Logger",
                                fullName);
                            InternalLogger.Error(errorMessage);
                            if (ThrowExceptions)
                            {
                                throw new NLogRuntimeException(errorMessage);
                            }
                            newLogger = CreateDefaultLogger(ref cacheKey);
                        }
                        else
                        {

                            var instance = FactoryHelper.CreateInstance(cacheKey.ConcreteType);
                            newLogger = instance as Logger;
                            if (newLogger == null)
                            {
                                //well, it's not a Logger, and we should return a Logger.

                                var errorMessage = string.Format("GetLogger / GetCurrentClassLogger got '{0}' as loggerType which doesn't inherit from Logger", fullName);
                                InternalLogger.Error(errorMessage);
                                if (ThrowExceptions)
                                {
                                    throw new NLogRuntimeException(errorMessage);
                                }

                                // Creating default instance of logger if instance of specified type cannot be created.
                                newLogger = CreateDefaultLogger(ref cacheKey);

                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        InternalLogger.Error(ex, "GetLogger / GetCurrentClassLogger. Cannot create instance of type '{0}'. It should have an default contructor. ", fullName);

                        if (ex.MustBeRethrown())
                        {
                            throw;
                        }

                        // Creating default instance of logger if instance of specified type cannot be created.
                        newLogger = CreateDefaultLogger(ref cacheKey);
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

        private static Logger CreateDefaultLogger(ref LoggerCacheKey cacheKey)
        {
            cacheKey = new LoggerCacheKey(cacheKey.Name, typeof(Logger));

            var newLogger = new Logger();
            return newLogger;
        }

#if !SILVERLIGHT && !__IOS__ && !__ANDROID__
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

        private void LoadLoggingConfiguration(string configFile)
        {
            InternalLogger.Debug("Loading config from {0}", configFile);
            this.config = new XmlLoggingConfiguration(configFile, this);
        }


#if !SILVERLIGHT && !__IOS__ && !__ANDROID__
        /// <summary>
        /// Currenty this logfactory is disposing?
        /// </summary>
        private bool IsDisposing;

        private void currentAppDomain_DomainUnload(object sender, EventArgs e)
        {
            //stop timer on domain unload, otherwise: 
            //Exception: System.AppDomainUnloadedException
            //Message: Attempted to access an unloaded AppDomain.
            lock (this.syncRoot)
            {
                IsDisposing = true;
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
        internal class LoggerCacheKey : IEquatable<LoggerCacheKey>
        {
            public string Name { get; private set; }

            public Type ConcreteType { get; private set; }

            public LoggerCacheKey(string name, Type concreteType)
            {
                this.Name = name;
                this.ConcreteType = concreteType;
            }

            /// <summary>
            /// Serves as a hash function for a particular type.
            /// </summary>
            /// <returns>
            /// A hash code for the current <see cref="T:System.Object"/>.
            /// </returns>
            public override int GetHashCode()
            {
                return this.ConcreteType.GetHashCode() ^ this.Name.GetHashCode();
            }

            /// <summary>
            /// Determines if two objects are equal in value.
            /// </summary>
            /// <param name="obj">Other object to compare to.</param>
            /// <returns>True if objects are equal, false otherwise.</returns>
            public override bool Equals(object obj)
            {
                LoggerCacheKey key = obj as LoggerCacheKey;
                if (ReferenceEquals(key, null))
                {
                    return false;
                }

                return (this.ConcreteType == key.ConcreteType) && (key.Name == this.Name);
            }

            /// <summary>
            /// Determines if two objects of the same type are equal in value.
            /// </summary>
            /// <param name="key">Other object to compare to.</param>
            /// <returns>True if objects are equal, false otherwise.</returns>
            public bool Equals(LoggerCacheKey key)
            {
                if (ReferenceEquals(key, null))
                {
                    return false;
                }

                return (this.ConcreteType == key.ConcreteType) && (key.Name == this.Name);
            }
        }

        /// <summary>
        /// Logger cache.
        /// </summary>
        private class LoggerCache
        {
            // The values of WeakReferences are of type Logger i.e. Directory<LoggerCacheKey, Logger>.
            private readonly Dictionary<LoggerCacheKey, WeakReference> loggerCache =
                    new Dictionary<LoggerCacheKey, WeakReference>();

            /// <summary>
            /// Inserts or updates. 
            /// </summary>
            /// <param name="cacheKey"></param>
            /// <param name="logger"></param>
            public void InsertOrUpdate(LoggerCacheKey cacheKey, Logger logger)
            {
                loggerCache[cacheKey] = new WeakReference(logger);
            }

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
        private class LogEnabler : IDisposable
        {
            private LogFactory factory;

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
                this.factory.ResumeLogging();
            }
        }
    }
}