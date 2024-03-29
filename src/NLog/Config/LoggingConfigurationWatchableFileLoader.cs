// 
// Copyright (c) 2004-2021 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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

#if !NETSTANDARD1_3

namespace NLog.Config
{
    using System;
    using System.Linq;
    using System.Threading;
    using NLog.Common;
    using NLog.Internal;
    using NLog.Internal.Fakeables;

    /// <summary>
    /// Enables FileWatcher for the currently loaded NLog Configuration File,
    /// and supports automatic reload on file modification.
    /// </summary>
    internal sealed class LoggingConfigurationWatchableFileLoader : LoggingConfigurationFileLoader
    {
        private const int ReconfigAfterFileChangedTimeout = 1000;
        private readonly object _lockObject = new object();
        private Timer _reloadTimer;
        private MultiFileWatcher _watcher;
        private bool _isDisposing;
        private LogFactory _logFactory;

        public LoggingConfigurationWatchableFileLoader(IAppEnvironment appEnvironment)
            :base(appEnvironment)
        {
        }

        public override LoggingConfiguration Load(LogFactory logFactory, string filename = null)
        {
#if !NETSTANDARD
            if (string.IsNullOrEmpty(filename))
            {
                var config = TryLoadFromAppConfig();
                if (config != null)
                    return config;
            }
#endif

            return base.Load(logFactory, filename);
        }

        public override void Activated(LogFactory logFactory, LoggingConfiguration config)
        {
            _logFactory = logFactory;

            TryUnwatchConfigFile();

            if (config != null)
            {
                TryWachtingConfigFile(config);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _isDisposing = true;

                if (_watcher != null)
                {
                    // Disable startup of new reload-timers
                    _watcher.FileChanged -= ConfigFileChanged;
                    _watcher.StopWatching();
                }

                var currentTimer = _reloadTimer;
                if (currentTimer != null)
                {
                    _reloadTimer = null;
                    currentTimer.WaitForDispose(TimeSpan.Zero);
                }

                // Dispose file-watcher after having dispose timer to avoid race
                _watcher?.Dispose();
            }

            base.Dispose(disposing);
        }

#if !NETSTANDARD
        private LoggingConfiguration TryLoadFromAppConfig()
        {
            try
            {
                // Try to load default configuration.
                return XmlLoggingConfiguration.AppConfig;
            }
            catch (Exception ex)
            {
                //loading could fail due to an invalid XML file (app.config) etc.
                if (ex.MustBeRethrown())
                {
                    throw;
                }
                return null;
            }
        }
#endif

        internal void ReloadConfigOnTimer(object state)
        {
            if (_reloadTimer is null && _isDisposing)
            {
                return; //timer was disposed already. 
            }

            LoggingConfiguration oldConfig = (LoggingConfiguration)state;

            InternalLogger.Info("Reloading configuration...");
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
                    currentTimer.WaitForDispose(TimeSpan.Zero);
                }
            }

            lock (_logFactory._syncRoot)
            {
                LoggingConfiguration newConfig;
                try
                {
                    if (!ReferenceEquals(_logFactory._config, oldConfig))
                    {
                        InternalLogger.Warn("NLog Config changed in between. Not reloading.");
                        return;
                    }

                    newConfig = oldConfig.Reload();
                    if (ReferenceEquals(newConfig, oldConfig))
                        return;

                    if (newConfig is IInitializeSucceeded config2 && config2.InitializeSucceeded != true)
                    {
                        InternalLogger.Warn("NLog Config Reload() failed. Invalid XML?");
                        return;
                    }
                }
                catch (Exception exception)
                {
#if DEBUG
                    if (exception.MustBeRethrownImmediately())
                    {
                        throw;  // Throwing exceptions here will crash the entire application (.NET 2.0 behavior)
                    }
#endif
                    InternalLogger.Warn(exception, "NLog configuration failed to reload");
#pragma warning disable CS0618 // Type or member is obsolete
                    _logFactory?.NotifyConfigurationReloaded(new LoggingConfigurationReloadedEventArgs(false, exception));
#pragma warning restore CS0618 // Type or member is obsolete
                    return;
                }

                try
                {
                    TryUnwatchConfigFile();

                    _logFactory.Configuration = newConfig;  // Triggers LogFactory to call Activated(...) that adds file-watch again

#pragma warning disable CS0618 // Type or member is obsolete
                    _logFactory?.NotifyConfigurationReloaded(new LoggingConfigurationReloadedEventArgs(true));
#pragma warning restore CS0618 // Type or member is obsolete
                }
                catch (Exception exception)
                {
#if DEBUG
                    if (exception.MustBeRethrownImmediately())
                    {
                        throw;  // Throwing exceptions here will crash the entire application (.NET 2.0 behavior)
                    }
#endif
                    InternalLogger.Warn(exception, "NLog configuration reloaded, failed to be assigned");
                    _watcher.Watch(oldConfig.FileNamesToWatch);
#pragma warning disable CS0618 // Type or member is obsolete
                    _logFactory?.NotifyConfigurationReloaded(new LoggingConfigurationReloadedEventArgs(false, exception));
#pragma warning restore CS0618 // Type or member is obsolete
                }
            }
        }

        private void ConfigFileChanged(object sender, EventArgs args)
        {
            InternalLogger.Info("Configuration file change detected! Reloading in {0}ms...", ReconfigAfterFileChangedTimeout);

            // In the rare cases we may get multiple notifications here, 
            // but we need to reload config only once.
            //
            // The trick is to schedule the reload in one second after
            // the last change notification comes in.
            lock (_lockObject)
            {
                if (_isDisposing)
                {
                    return;
                }

                if (_reloadTimer is null)
                {
                    var configuration = _logFactory._config;
                    if (configuration != null)
                    {
                        _reloadTimer = new Timer(
                                ReloadConfigOnTimer,
                                configuration,
                                ReconfigAfterFileChangedTimeout,
                                Timeout.Infinite);
                    }
                }
                else
                {
                    _reloadTimer.Change(
                            ReconfigAfterFileChangedTimeout,
                            Timeout.Infinite);
                }
            }
        }

        private void TryWachtingConfigFile(LoggingConfiguration config)
        {
            try
            {
                var fileNamesToWatch = config.FileNamesToWatch?.ToList();
                if (fileNamesToWatch?.Count > 0)
                {
                    if (_watcher is null)
                    {
                        _watcher = new MultiFileWatcher();
                        _watcher.FileChanged += ConfigFileChanged;
                    }

                    _watcher.Watch(fileNamesToWatch);
                }
            }
            catch (Exception exception)
            {
                if (exception.MustBeRethrown())
                {
                    throw;
                }

                //ToArray needed for .Net 3.5
                InternalLogger.Warn(exception, "Cannot start file watching: {0}", string.Join(",", _logFactory._config.FileNamesToWatch.ToArray()));
            }
        }

        private void TryUnwatchConfigFile()
        {
            try
            {
                _watcher?.StopWatching();
            }
            catch (Exception exception)
            {
                InternalLogger.Error(exception, "Cannot stop file watching.");

                if (exception.MustBeRethrown())
                {
                    throw;
                }
            }
        }
    }
}

#endif
