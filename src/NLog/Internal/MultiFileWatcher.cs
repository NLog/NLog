// 
// Copyright (c) 2004-2016 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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

#if !SILVERLIGHT && !__IOS__ && !__ANDROID__

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
    internal class MultiFileWatcher : IDisposable
    {
        private Dictionary<string, FileSystemWatcher> watcherMap = new Dictionary<string, FileSystemWatcher>();

        /// <summary>
        /// The types of changes to watch for.
        /// </summary>
        public NotifyFilters NotifyFilters { get; set; }

        /// <summary>
        /// Occurs when a change is detected in one of the monitored files.
        /// </summary>
        public event FileSystemEventHandler OnChange;

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
            this.StopWatching();
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Stops watching all files.
        /// </summary>
        public void StopWatching()
        {
            lock (this)
            {
                foreach (FileSystemWatcher watcher in this.watcherMap.Values)
                    StopWatching(watcher);

                this.watcherMap.Clear();
            }
        }

        /// <summary>
        /// Stops watching the specified file.
        /// </summary>
        /// <param name="fileName"></param>
        public void StopWatching(string fileName)
        {
            lock (this)
            {
                FileSystemWatcher watcher;
                if (this.watcherMap.TryGetValue(fileName, out watcher))
                {
                    StopWatching(watcher);
                    this.watcherMap.Remove(fileName);
                }
            }
        }

        private void StopWatching(FileSystemWatcher watcher)
        {
            InternalLogger.Info("Stopping file watching for path '{0}' filter '{1}'", watcher.Path, watcher.Filter);
            watcher.EnableRaisingEvents = false;
            watcher.Dispose();
        }

        /// <summary>
        /// Watches the specified files for changes.
        /// </summary>
        /// <param name="fileNames">The file names.</param>
        public void Watch(IEnumerable<string> fileNames)
        {
            if (fileNames == null)
            {
                return;
            }

            foreach (string s in fileNames)
            {
                this.Watch(s);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Watcher is released in Dispose()")]
        internal void Watch(string fileName)
        {
            var directory = Path.GetDirectoryName(fileName);
            if (!Directory.Exists(directory))
            {
                InternalLogger.Warn("Cannot watch {0} for changes as it doesn't exist", directory);
                return;
            }

            lock (this)
            {
                if (this.watcherMap.ContainsKey(fileName))
                    return;

                var watcher = new FileSystemWatcher
                {
                    Path = directory,
                    Filter = Path.GetFileName(fileName),
                    NotifyFilter = NotifyFilters
                };

                watcher.Created += this.OnWatcherChanged;
                watcher.Changed += this.OnWatcherChanged;
                watcher.Deleted += this.OnWatcherChanged;
                watcher.EnableRaisingEvents = true;
                InternalLogger.Info("Watching path '{0}' filter '{1}' for changes.", watcher.Path, watcher.Filter);
                
                this.watcherMap.Add(fileName, watcher);
            }
        }

        private void OnWatcherChanged(object source, FileSystemEventArgs e)
        {
            if (this.OnChange != null)
            {
                this.OnChange(source, e);
            }
        }
    }
}

#endif
