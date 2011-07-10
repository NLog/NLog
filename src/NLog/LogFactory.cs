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
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Threading;
    using System.Windows;
    using NLog.Common;
    using NLog.Config;
    using NLog.Internal;
    using NLog.Targets;

    /// <summary>
    /// Creates and manages instances of <see cref="T:NLog.Logger" /> objects.
    /// </summary>
    public class LogFactory : IDisposable
    {
#if !NET_CF && !SILVERLIGHT
        private readonly MultiFileWatcher watcher;
        private const int ReconfigAfterFileChangedTimeout = 1000;
#endif

        private readonly Dictionary<LoggerCacheKey, WeakReference> loggerCache = new Dictionary<LoggerCacheKey, WeakReference>();

        private static TimeSpan defaultFlushTimeout = TimeSpan.FromSeconds(15);

#if !NET_CF && !SILVERLIGHT
        private Timer reloadTimer;
#endif

        private LoggingConfiguration config;
        private LogLevel globalThreshold = LogLevel.MinLevel;
        private bool configLoaded;
        private int logsEnabled;

        /// <summary>
        /// Initializes a new instance of the <see cref="LogFactory" /> class.
        /// </summary>
        public LogFactory()
        {
#if !NET_CF && !SILVERLIGHT
            this.watcher = new MultiFileWatcher();
            this.watcher.OnChange += this.ConfigFileChanged;
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
        /// Occurs when logging <see cref="Configuration" /> changes.
        /// </summary>
        public event EventHandler<LoggingConfigurationChangedEventArgs> ConfigurationChanged;

#if !NET_CF && !SILVERLIGHT
        /// <summary>
        /// Occurs when logging <see cref="Configuration" /> gets reloaded.
        /// </summary>
        public event EventHandler<LoggingConfigurationReloadedEventArgs> ConfigurationReloaded;
#endif

        /// <summary>
        /// Gets or sets a value indicating whether exceptions should be thrown.
        /// </summary>
        /// <value>A value of <c>true</c> if exceptiosn should be thrown; otherwise, <c>false</c>.</value>
        /// <remarks>By default exceptions
        /// are not thrown under any circumstances.
        /// </remarks>
        public bool ThrowExceptions { get; set; }

        /// <summary>
        /// Gets or sets the current logging configuration.
        /// </summary>
        public LoggingConfiguration Configuration
        {
            get
            {
                lock (this)
                {
                    if (this.configLoaded)
                    {
                        return this.config;
                    }

                    this.configLoaded = true;

#if !NET_CF && !SILVERLIGHT
                    if (this.config == null)
                    {
                        // try to load default configuration
                        this.config = XmlLoggingConfiguration.AppConfig;
                    }
#endif

                    if (this.config == null)
                    {
                        foreach (string configFile in GetCandidateFileNames())
                        {
#if !SILVERLIGHT
                            if (File.Exists(configFile))
                            {
                                InternalLogger.Debug("Attempting to load config from {0}", configFile);
                                this.config = new XmlLoggingConfiguration(configFile);
                            }
#else
                            Uri configFileUri = new Uri(configFile, UriKind.Relative);
                            if (Application.GetResourceStream(configFileUri) != null)
                            {
                                InternalLogger.Debug("Attempting to load config from {0}", configFile);
                                this.config = new XmlLoggingConfiguration(configFile);
                            }
#endif
                        }
                    }

#if !NET_CF && !SILVERLIGHT
                    if (this.config != null)
                    {
                        Dump(this.config);
                        this.watcher.Watch(this.config.FileNamesToWatch);
                    }
#endif
                    if (this.config != null)
                    {
                        this.config.InitializeAll();
                    }

                    return this.config;
                }
            }

            set
            {
#if !NET_CF && !SILVERLIGHT
                try
                {
                    this.watcher.StopWatching();
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

                lock (this)
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
                    this.configLoaded = true;

                    if (this.config != null)
                    {
                        Dump(this.config);

                        this.config.InitializeAll();
                        this.ReconfigExistingLoggers(this.config);
#if !NET_CF && !SILVERLIGHT
                        try
                        {
                            this.watcher.Watch(this.config.FileNamesToWatch);
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

                    var configurationChangedDelegate = this.ConfigurationChanged;

                    if (configurationChangedDelegate != null)
                    {
                        configurationChangedDelegate(this, new LoggingConfigurationChangedEventArgs(oldConfig, value));
                    }
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
                lock (this)
                {
                    this.globalThreshold = value;
                    this.ReconfigExistingLoggers();
                }
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
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
            newLogger.Initialize(string.Empty, new LoggerConfiguration(targetsByLevel), this);
            return newLogger;
        }

#if !NET_CF
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
        /// <param name="loggerType">The type of the logger to create. The type must inherit from NLog.Logger.</param>
        /// <returns>The logger.</returns>
        /// <remarks>This is a slow-running method. 
        /// Make sure you're not doing this in a loop.</remarks>
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
#endif

        /// <summary>
        /// Gets the specified named logger.
        /// </summary>
        /// <param name="name">Name of the logger.</param>
        /// <returns>The logger reference. Multiple calls to <c>GetLogger</c> with the same argument aren't guaranteed to return the same logger reference.</returns>
        public Logger GetLogger(string name)
        {
            return this.GetLogger(new LoggerCacheKey(typeof(Logger), name));
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
            return this.GetLogger(new LoggerCacheKey(loggerType, name));
        }

        /// <summary>
        /// Loops through all loggers previously returned by GetLogger
        /// and recalculates their target and filter list. Useful after modifying the configuration programmatically
        /// to ensure that all loggers have been properly configured.
        /// </summary>
        public void ReconfigExistingLoggers()
        {
            this.ReconfigExistingLoggers(this.config);
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
        /// <param name="timeout">Maximum time to allow for the flush. Any messages after that time will be discarded.</param>
        public void Flush(TimeSpan timeout)
        {
            AsyncHelpers.RunSynchronously(cb => this.Flush(cb, timeout));
        }

        /// <summary>
        /// Flush any pending log messages (in case of asynchronous targets).
        /// </summary>
        /// <param name="timeoutMilliseconds">Maximum time to allow for the flush. Any messages after that time will be discarded.</param>
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
        /// <param name="timeoutMilliseconds">Maximum time to allow for the flush. Any messages after that time will be discarded.</param>
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
                this.logsEnabled--;
                if (this.logsEnabled == -1)
                {
                    this.ReconfigExistingLoggers();
                }
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
        /// <remarks>Logging is enabled if the number of <see cref="EnableLogging"/> calls is greater 
        /// than or equal to <see cref="DisableLogging"/> calls.</remarks>
        public bool IsLoggingEnabled()
        {
            return this.logsEnabled >= 0;
        }

#if !NET_CF && !SILVERLIGHT
        internal void ReloadConfigOnTimer(object state)
        {
            LoggingConfiguration configurationToReload = (LoggingConfiguration)state;

            InternalLogger.Info("Reloading configuration...");
            lock (this)
            {
                if (this.reloadTimer != null)
                {
                    this.reloadTimer.Dispose();
                    this.reloadTimer = null;
                }

                this.watcher.StopWatching();
                try
                {
                    if (this.Configuration != configurationToReload)
                    {
                        throw new NLogConfigurationException("Config changed in between. Not reloading.");
                    }

                    LoggingConfiguration newConfig = configurationToReload.Reload();
                    if (newConfig != null)
                    {
                        this.Configuration = newConfig;
                        if (this.ConfigurationReloaded != null)
                        {
                            this.ConfigurationReloaded(true, null);
                        }
                    }
                    else
                    {
                        throw new NLogConfigurationException("Configuration.Reload() returned null. Not reloading.");
                    }
                }
                catch (Exception exception)
                {
                    if (exception.MustBeRethrown())
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

        internal void ReconfigExistingLoggers(LoggingConfiguration configuration)
        {
            if (configuration != null)
            {
                configuration.EnsureInitialized();
            }

            foreach (var loggerWrapper in this.loggerCache.Values.ToList())
            {
                Logger logger = loggerWrapper.Target as Logger;
                if (logger != null)
                {
                    logger.SetConfiguration(this.GetConfigurationForLogger(logger.Name, configuration));
                }
            }
        }

        internal void GetTargetsByLevelForLogger(string name, IList<LoggingRule> rules, TargetWithFilterChain[] targetsByLevel, TargetWithFilterChain[] lastTargetsByLevel)
        {
            foreach (LoggingRule rule in rules)
            {
                if (!rule.NameMatches(name))
                {
                    continue;
                }

                for (int i = 0; i <= LogLevel.MaxLevel.Ordinal; ++i)
                {
                    if (i < this.GlobalThreshold.Ordinal || !rule.IsLoggingEnabledForLevel(LogLevel.FromOrdinal(i)))
                    {
                        continue;
                    }

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

                this.GetTargetsByLevelForLogger(name, rule.ChildRules, targetsByLevel, lastTargetsByLevel);

                if (rule.Final)
                {
                    break;
                }
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

            if (configuration != null && this.IsLoggingEnabled())
            {
                this.GetTargetsByLevelForLogger(name, configuration.LoggingRules, targetsByLevel, lastTargetsByLevel);
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

            return new LoggerConfiguration(targetsByLevel);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing">True to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
#if !NET_CF && !SILVERLIGHT
                this.watcher.Dispose();

                if (this.reloadTimer != null)
                {
                    this.reloadTimer.Dispose();
                    this.reloadTimer = null;
                }
#endif
            }
        }

        private static IEnumerable<string> GetCandidateFileNames()
        {
#if NET_CF
            yield return CompactFrameworkHelper.GetExeFileName() + ".nlog";
            yield return Path.Combine(Path.GetDirectoryName(CompactFrameworkHelper.GetExeFileName()), "NLog.config");
            yield return typeof(LogFactory).Assembly.GetName().CodeBase + ".nlog";
#elif SILVERLIGHT
            yield return "NLog.config";
#else
            // NLog.config from application directory
            yield return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "NLog.config");

            // current config file with .config renamed to .nlog
            string cf = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
            if (cf != null)
            {
                yield return Path.ChangeExtension(cf, ".nlog");
            }

            // get path to NLog.dll.nlog only if the assembly is not in the GAC
            var nlogAssembly = typeof(LogFactory).Assembly;
            if (!nlogAssembly.GlobalAssemblyCache)
            {
                var codeBase = nlogAssembly.GetName().CodeBase;
                if (!string.IsNullOrEmpty(codeBase))
                {
                    var uri = new Uri(codeBase, UriKind.RelativeOrAbsolute);
                    if (uri.Scheme == "file")
                    {
                        yield return uri.AbsolutePath + ".nlog";
                    }
                }
            }
#endif
        }

        private static void Dump(LoggingConfiguration config)
        {
            if (!InternalLogger.IsDebugEnabled)
            {
                return;
            }

            config.Dump();
        }

        private Logger GetLogger(LoggerCacheKey cacheKey)
        {
            lock (this)
            {
                WeakReference l;

                if (this.loggerCache.TryGetValue(cacheKey, out l))
                {
                    Logger existingLogger = l.Target as Logger;
                    if (existingLogger != null)
                    {
                        // logger in the cache and still referenced
                        return existingLogger;
                    }
                }

                Logger newLogger;

                if (cacheKey.ConcreteType != null && cacheKey.ConcreteType != typeof(Logger))
                {
                    newLogger = (Logger)FactoryHelper.CreateInstance(cacheKey.ConcreteType);
                }
                else
                {
                    newLogger = new Logger();
                }

                if (cacheKey.ConcreteType != null)
                {
                    newLogger.Initialize(cacheKey.Name, this.GetConfigurationForLogger(cacheKey.Name, this.Configuration), this);
                }

                this.loggerCache[cacheKey] = new WeakReference(newLogger);
                return newLogger;
            }
        }

#if !NET_CF && !SILVERLIGHT
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
                if (this.reloadTimer == null)
                {
                    this.reloadTimer = new Timer(
                        this.ReloadConfigOnTimer,
                        this.Configuration,
                        ReconfigAfterFileChangedTimeout,
                        Timeout.Infinite);
                }
                else
                {
                    this.reloadTimer.Change(ReconfigAfterFileChangedTimeout, Timeout.Infinite);
                }
            }
        }
#endif

        /// <summary>
        /// Logger cache key.
        /// </summary>
        internal class LoggerCacheKey
        {
            internal LoggerCacheKey(Type loggerConcreteType, string name)
            {
                this.ConcreteType = loggerConcreteType;
                this.Name = name;
            }

            internal Type ConcreteType { get; private set; }

            internal string Name { get; private set; }

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
            /// <param name="o">Other object to compare to.</param>
            /// <returns>True if objects are equal, false otherwise.</returns>
            public override bool Equals(object o)
            {
                var key = o as LoggerCacheKey;
                if (ReferenceEquals(key, null))
                {
                    return false;
                }

                return (this.ConcreteType == key.ConcreteType) && (key.Name == this.Name);
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
                this.factory.EnableLogging();
            }
        }
    }
}
