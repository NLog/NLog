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

namespace NLog.Internal
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using NLog.Common;

    /// <summary>
    /// Watches multiple files at the same time and raises an event whenever 
    /// a single change is detected in any of those files.
    /// </summary>
    internal sealed class MultiFileWatcher : IDisposable
    {
        private readonly Dictionary<string, FileSystemWatcher> _watcherMap = new Dictionary<string, FileSystemWatcher>();

        /// <summary>
        /// The types of changes to watch for.
        /// </summary>
        public NotifyFilters NotifyFilters { get; set; }

        /// <summary>
        /// Occurs when a change is detected in one of the monitored files.
        /// </summary>
        public event FileSystemEventHandler FileChanged;

        public MultiFileWatcher() : 
            this(NotifyFilters.LastWrite | NotifyFilters.CreationTime | NotifyFilters.Size | NotifyFilters.Security | NotifyFilters.Attributes) { }

        public MultiFileWatcher(NotifyFilters notifyFilters)
        {
            NotifyFilters = notifyFilters;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            FileChanged = null;   // Release event listeners
            StopWatching();
        }

        /// <summary>
        /// Stops watching all files.
        /// </summary>
        public void StopWatching()
        {
            lock (_watcherMap)
            {
                foreach (var watcher in _watcherMap)
                {
                    StopWatching(watcher.Value);
                }
                _watcherMap.Clear();
            }
        }

        /// <summary>
        /// Stops watching the specified file.
        /// </summary>
        /// <param name="fileName"></param>
        public void StopWatching(string fileName)
        {
            lock (_watcherMap)
            {
                if (_watcherMap.TryGetValue(fileName, out var watcher))
                {
                    StopWatching(watcher);
                    _watcherMap.Remove(fileName);
                }
            }
        }

        /// <summary>
        /// Watches the specified files for changes.
        /// </summary>
        /// <param name="fileNames">The file names.</param>
        public void Watch(IEnumerable<string> fileNames)
        {
            if (fileNames is null)
            {
                return;
            }

            foreach (string s in fileNames)
            {
                Watch(s);
            }
        }

        public void Watch(string fileName)
        {
            try
            {
                var directory = Path.GetDirectoryName(fileName);
                var fileFilter = Path.GetFileName(fileName);
                directory = Path.GetFullPath(directory);
                if (!Directory.Exists(directory))
                {
                    InternalLogger.Warn("Cannot watch file-filter '{0}' when non-existing directory: {1}", fileFilter, directory);
                    return;
                }

                if (TryAddWatch(fileName, directory, fileFilter))
                {
                    InternalLogger.Debug("Start watching file-filter '{0}' in directory: {1}", fileFilter, directory);
                }
            }
            catch (System.Security.SecurityException ex)
            {
                InternalLogger.Debug(ex, "Failed to start FileSystemWatcher with file-path: {0}", fileName);
            }
        }

        private bool TryAddWatch(string fileName, string directory, string fileFilter)
        {
            lock (_watcherMap)
            {
                if (_watcherMap.ContainsKey(fileName))
                    return false;

                FileSystemWatcher watcher = null;

                try
                {
                    watcher = new FileSystemWatcher
                    {
                        Path = directory,
                        Filter = fileFilter,
                        NotifyFilter = NotifyFilters
                    };

                    watcher.Created += OnFileChanged;
                    watcher.Changed += OnFileChanged;
                    watcher.Deleted += OnFileChanged;
                    watcher.Renamed += OnFileChanged;
                    watcher.Error += OnWatcherError;
                    watcher.EnableRaisingEvents = true;

                    _watcherMap.Add(fileName, watcher);
                    return true;
                }
                catch (Exception ex)
                {
                    InternalLogger.Error(ex, "Failed to start FileSystemWatcher with file-filter '{0}' in directory: {1}", fileFilter, directory);
                    if (ex.MustBeRethrown())
                        throw;

                    if (watcher != null)
                    {
                        StopWatching(watcher);
                    }

                    return false;
                }
            }
        }

        private void StopWatching(FileSystemWatcher watcher)
        {
            string fileFilter = string.Empty;
            string fileDirectory = string.Empty;

            try
            {
                fileFilter = watcher.Filter;
                fileDirectory = watcher.Path;

                InternalLogger.Debug("Stop watching file-filter '{0}' in directory: {1}", fileFilter, fileDirectory);
                watcher.EnableRaisingEvents = false;
                watcher.Created -= OnFileChanged;
                watcher.Changed -= OnFileChanged;
                watcher.Deleted -= OnFileChanged;
                watcher.Renamed -= OnFileChanged;
                watcher.Error -= OnWatcherError;
                watcher.Dispose();
            }
            catch (Exception ex)
            {
                InternalLogger.Error(ex, "Failed to stop FileSystemWatcher with file-filter '{0}' in directory: {1}", fileFilter, fileDirectory);
                if (ex.MustBeRethrown())
                    throw;
            }
        }

        private static void OnWatcherError(object source, ErrorEventArgs e)
        {
            var watcher = source as FileSystemWatcher;
            var fileFilter = watcher?.Filter ?? string.Empty;
            var fileDirectory = watcher?.Path ?? string.Empty;
            InternalLogger.Warn(e.GetException(), "Error from FileSystemWatcher with file-filter '{0}' in directory: {1}", fileFilter, fileDirectory);
        }

        private void OnFileChanged(object source, FileSystemEventArgs e)
        {
            var changed = FileChanged;
            if (changed != null)
            {
                try
                {
                    changed(source, e);
                }
                catch (Exception ex)
                {
#if DEBUG
                    if (ex.MustBeRethrownImmediately())
                        throw;  // Throwing exceptions here might crash the entire application (.NET 2.0 behavior)
#endif
                    InternalLogger.Error(ex, "Error handling event from FileSystemWatcher with file-filter: '{0}' in directory: {1}", e.Name, e.FullPath);
                }
            }
        }
    }
}

#endif
