// 
// Copyright (c) 2004 Jaroslaw Kowalski <jkowalski@users.sourceforge.net>
// 
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
// * Neither the name of the Jaroslaw Kowalski nor the names of its 
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

using NLog.Config;
using NLog.Internal;
using NLog.Appenders;

namespace NLog
{
    /// <summary>
    /// Creates and manages instances of <see cref="T:NLog.Logger" /> objects.
    /// </summary>
    public sealed class LogManager
    {
        private static LoggerDictionary _loggerCache = new LoggerDictionary();
        private static LoggingConfiguration _config;
        private static bool _configLoaded = false;
        private static bool _throwExceptions = false;
        private static bool _reloadConfigOnNextLog = false;

        internal static bool ReloadConfigOnNextLog
        {
            get
            {
                return _reloadConfigOnNextLog;
            }
            set
            {
                _reloadConfigOnNextLog = value;
            }
        }

        public static bool ThrowExceptions
        {
            get
            {
                return _throwExceptions;
            }
            set
            {
                _throwExceptions = value;
            }
        }

        private LogManager(){}

        /// <summary>
        /// Gets the specified named logger.
        /// </summary>
        /// <param name="name">name of the logger</param>
        /// <returns>The logger reference. Multiple calls to <c>GetLogger</c> with the same argument aren't guaranteed to return the same logger reference.</returns>
        public static Logger GetLogger(string name)
        {
            if (ReloadConfigOnNextLog)
                ReloadConfig();

            lock(typeof(LogManager))
            {
                Logger l = _loggerCache[name];
                if (l != null)
                    return l;

                AppenderWithFilterChain[]appendersByLevel = GetAppendersByLevelForLogger(name, Configuration);

                Logger newLogger = new LoggerImpl(name, appendersByLevel);
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
                        string configFile = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
                        configFile = configFile.Replace(".config", ".nlog");
                        if (File.Exists(configFile))
                        {
                            InternalLogger.Debug("Attempting to load config from {0}", configFile);
                            _config = new XmlLoggingConfiguration(configFile);
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
                        if (Environment.GetEnvironmentVariable("NLOG_GLOBAL_CONFIG_FILE") != null)
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
                        string configFile = typeof(LogManager).Assembly.GetName().CodeBase + ".nlog";
                        if (File.Exists(configFile))
                        {
                            InternalLogger.Debug("Attempting to load config from {0}", configFile);
                            _config = new XmlLoggingConfiguration(configFile);
                        }
                    }
#endif 
                    return _config;
                }
            }

            set
            {
#if !NETCF
                _watcher.StopWatching();
#endif 

                lock(typeof(LogManager))
                {
                    _config = value;
                    _configLoaded = true;

                    if (_config != null)
                    {
                        ReconfigExistingLoggers(_config);
#if !NETCF
                        _watcher.Watch(_config.FileNamesToWatch);
#endif 
                    }
                }
            }
        }

#if !NETCF
        private static MultiFileWatcher _watcher = new MultiFileWatcher(new EventHandler(ConfigFileChanged));

        private static void ConfigFileChanged(object sender, EventArgs args)
        {
            InternalLogger.Debug("ConfigFileChanged!!!");
            ReloadConfigOnNextLog = true;
        }
#endif 

        internal static void ReloadConfig()
        {
            lock(typeof(LogManager))
            {
                if (!ReloadConfigOnNextLog)
                    return ;

                InternalLogger.Debug("Reloading Config...");
                LoggingConfiguration newConfig = Configuration.Reload();
                if (newConfig != null)
                    Configuration = newConfig;
                ReloadConfigOnNextLog = false;
            }
        }

        internal static void ReconfigExistingLoggers(LoggingConfiguration config)
        {
            foreach (LoggerImpl logger in _loggerCache.Values)
            {
                logger.Reconfig(GetAppendersByLevelForLogger(logger.Name, config));
            }
        }

        internal static AppenderWithFilterChain[]GetAppendersByLevelForLogger(string name, LoggingConfiguration config)
        {
            AppenderWithFilterChain[]appendersByLevel = new AppenderWithFilterChain[(int)LogLevel.MaxLevel + 1];

            if (config != null)
            {
                foreach (AppenderRule rule in config.AppenderRules)
                {
                    if (rule.Appenders.Count == 0)
                        continue;

                    if (rule.Matches(name))
                    {
                        for (int i = 0; i <= (int)LogLevel.MaxLevel; ++i)
                        {
                            if (rule.IsLoggingEnabledForLevel((LogLevel)i))
                            {
                                foreach (Appender appender in rule.Appenders)
                                {
                                    AppenderWithFilterChain awf = new AppenderWithFilterChain(appender, rule.Filters);
                                    if (appendersByLevel[i] != null)
                                        appendersByLevel[i].Next = awf;
                                    appendersByLevel[i] = awf;
                                }
                            }
                        }
                        if (rule.Final)
                            break;
                    }
                }
            }
            return appendersByLevel;
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

        public static IDisposable DisableLogging()
        {
            Interlocked.Decrement(ref _logsEnabled);
            return LogEnabler.TheEnabler;
        }

        public static void EnableLogging()
        {
            Interlocked.Increment(ref _logsEnabled);
        }

        public static bool IsLoggingEnabled()
        {
            return _logsEnabled >= 0;
        }
    }
}
