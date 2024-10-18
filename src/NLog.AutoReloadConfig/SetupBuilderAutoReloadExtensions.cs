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
//

namespace NLog
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using NLog.Common;
    using NLog.Config;
    using NLog.Internal;

    /// <summary>
    /// Setup()-extension method that hooks into LogFactory.ConfiguractionChanged, and setup file-monitor on config-change
    /// </summary>
    public static class SetupBuilderAutoReloadExtensions
    {
        private static readonly Dictionary<LogFactory, AutoReloadConfigFileWatcher> _watchers = new Dictionary<LogFactory, AutoReloadConfigFileWatcher>();

        /// <summary>
        /// Enables AutoReload support for <see cref="XmlLoggingConfiguration"/>
        /// </summary>
        /// <remarks>
        /// Hooks into <see cref="LogFactory.OnConfigurationChanged"/> and setup file-monitoring for NLog.config file-changes.
        /// </remarks>
        public static ISetupBuilder SetupMonitorForAutoReload(this ISetupBuilder setupBuilder)
        {
            AutoReloadConfigFileWatcher fileWatcher = null;
            lock (_watchers)
            {
                _watchers.TryGetValue(setupBuilder.LogFactory, out fileWatcher);
            }

            if (fileWatcher is null || fileWatcher.IsDisposed)
            {
                InternalLogger.Info("AutoReload Config File Monitor starting");
                fileWatcher = new AutoReloadConfigFileWatcher(setupBuilder.LogFactory);
                lock (_watchers)
                {
                    _watchers[setupBuilder.LogFactory] = fileWatcher;
                }
            }

            setupBuilder.LoadConfiguration(cfg =>
            {
                var fileNamesToWatch = cfg.Configuration.FileNamesToWatch?.ToList();
                if (fileNamesToWatch?.Count > 0 || cfg.Configuration is XmlLoggingConfiguration)
                {
                    fileWatcher.RefreshFileWatcher(fileNamesToWatch ?? System.Linq.Enumerable.Empty<string>());
                }
                else if (cfg.Configuration.LoggingRules.Count == 0 && cfg.Configuration.AllTargets.Count == 0)
                {
                    cfg.Configuration = null;
                }
            });
            return setupBuilder;
        }

        private sealed class AutoReloadConfigFileWatcher : IDisposable
        {
            private readonly LogFactory _logFactory;
            private readonly MultiFileWatcher _fileWatcher = new MultiFileWatcher();
            private readonly object _lockObject = new object();
            private Timer _reloadTimer;
            private bool _isDisposing;

            internal bool IsDisposed => _isDisposing;

            public AutoReloadConfigFileWatcher(LogFactory logFactory)
            {
                _logFactory = logFactory;
                _logFactory.ConfigurationChanged += LogFactory_ConfigurationChanged;
                _fileWatcher.FileChanged += FileWatcher_FileChanged;
            }

            private void LogFactory_ConfigurationChanged(object sender, LoggingConfigurationChangedEventArgs e)
            {
                try
                {
                    if (_isDisposing)
                        return;

                    if (e.ActivatedConfiguration is null)
                    {
                        InternalLogger.Info("AutoReload Config File Monitor stopping, since no active configuration");
                        Dispose();
                    }
                    else
                    {
                        InternalLogger.Debug("AutoReload Config File Monitor refreshing after configuration changed");
                        var fileNamesToWatch = e.ActivatedConfiguration?.FileNamesToWatch ?? Enumerable.Empty<string>();
                        _fileWatcher.Watch(fileNamesToWatch);
                    }
                }
                catch (Exception ex)
                {
                    InternalLogger.Error(ex, "AutoReload Config File Monitor failed to refresh after configuration changed.");

                    if (LogManager.ThrowExceptions || _logFactory.ThrowExceptions)
                    {
                        throw;
                    }
                }
            }

            private void FileWatcher_FileChanged(object sender, System.IO.FileSystemEventArgs e)
            {
                lock (_lockObject)
                {
                    if (_isDisposing)
                        return;

                    var reloadTimer = _reloadTimer;
                    if (reloadTimer is null)
                    {
                        var currentConfig = _logFactory.Configuration;
                        if (currentConfig is null)
                            return;

                        _reloadTimer = new Timer((s) => ReloadTimer(s), currentConfig, 1000, Timeout.Infinite);
                    }
                    else
                    {
                        reloadTimer.Change(1000, Timeout.Infinite);
                    }
                }
            }

            private void ReloadTimer(object state)
            {
                if (_reloadTimer is null && _isDisposing)
                {
                    return; //timer was disposed already.
                }

                LoggingConfiguration oldConfig = (LoggingConfiguration)state;

                InternalLogger.Info("AutoReload Config File Monitor reloading configuration...");

                lock (_lockObject)
                {
                    if (_isDisposing)
                    {
                        return; //timer was disposed already.
                    }

                    var currentTimer = _reloadTimer;
                    if (currentTimer != null)
                    {
                        _reloadTimer = null;
                        currentTimer.Dispose();
                    }
                }

                LoggingConfiguration newConfig = null;

                try
                {
                    var currentConfig = _logFactory.Configuration;
                    if (!ReferenceEquals(currentConfig, oldConfig))
                    {
                        InternalLogger.Debug("AutoReload Config File Monitor skipping reload, since existing NLog config has changed.");
                        return;
                    }

                    newConfig = oldConfig.Reload();
                    if (newConfig is null || ReferenceEquals(newConfig, oldConfig))
                    {
                        InternalLogger.Debug("AutoReload Config File Monitor skipping reload, since new configuration has not changed.");
                        return;
                    }

                    currentConfig = _logFactory.Configuration;
                    if (!ReferenceEquals(currentConfig, oldConfig))
                    {
                        InternalLogger.Debug("AutoReload Config File Monitor skipping reload, since existing NLog config has changed.");
                        return;
                    }
                }
                catch (Exception exception)
                {
                    InternalLogger.Warn(exception, "AutoReload Config File Monitor failed to reload NLog LoggingConfiguration.");
                    return;
                }

                try
                {
                    TryUnwatchConfigFile();
                    _logFactory.Configuration = newConfig;  // Will trigger LogFactory_ConfigurationChanged
                }
                catch (Exception exception)
                {
                    InternalLogger.Warn(exception, "AutoReload Config File Monitor failed to activate new NLog LoggingConfiguration.");
                    _fileWatcher.Watch(oldConfig.FileNamesToWatch);
                }
            }

            public void RefreshFileWatcher(IEnumerable<string> fileNamesToWatch)
            {
                _fileWatcher.Watch(fileNamesToWatch);
            }

            public void Dispose()
            {
                _isDisposing = true;
                _logFactory.ConfigurationChanged -= LogFactory_ConfigurationChanged;
                _fileWatcher.FileChanged -= FileWatcher_FileChanged;
                lock (_lockObject)
                {
                    var reloadTimer = _reloadTimer;
                    _reloadTimer = null;
                    reloadTimer?.Dispose();
                }
                _fileWatcher.Dispose();
            }

            private void TryUnwatchConfigFile()
            {
                try
                {
                    _fileWatcher?.StopWatching();
                }
                catch (Exception exception)
                {
                    InternalLogger.Warn(exception, "AutoReload Config File Monitor failed to stop file watcher.");

                    if (LogManager.ThrowExceptions || _logFactory.ThrowExceptions)
                    {
                        throw;
                    }
                }
            }
        }
    }
}
