// 
// Copyright (c) 2004 Jaroslaw Kowalski <jaak@polbox.com>
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

#if !NETCF

using System;
using System.Collections;
using System.IO;

namespace NLog.Internal
{
    internal class MultiFileWatcher : IDisposable
    {
        private ArrayList _watchers = new ArrayList();
        private bool _triggerred = false;

        public MultiFileWatcher()
        {
        }
        
        public MultiFileWatcher(EventHandler onChange)
        {
            OnChange += onChange;
        }

        public MultiFileWatcher(ICollection fileNames)
        {
            Watch(fileNames);
        }

        public void Dispose()
        {
            StopWatching();
        }

        public void StopWatching()
        {
            lock (this) 
            {
                foreach (FileSystemWatcher watcher in _watchers) 
                {
                    Console.WriteLine("releasing watch path: {0} filter: {1}", watcher.Path, watcher.Filter);
                    watcher.EnableRaisingEvents = false;
                    watcher.Dispose();
                    Console.WriteLine("aa");
                }
                _watchers.Clear();
                _triggerred = false;
            }
        }

        public void Watch(ICollection fileNames)
        {
            if (fileNames == null)
                return;
            foreach (string s in fileNames) 
            {
                Watch(s);
            }
        }

        public void Watch(string fileName) 
        {
            FileSystemWatcher watcher = new FileSystemWatcher();
            watcher.Path = Path.GetDirectoryName(fileName);
            watcher.Filter = Path.GetFileName(fileName);
            // Console.WriteLine("watching path: {0} filter: {1}", watcher.Path, watcher.Filter);
            watcher.NotifyFilter = NotifyFilters.LastWrite;
            watcher.Changed += new FileSystemEventHandler(this.OnWatcherChanged);
            watcher.EnableRaisingEvents = true;

            lock (this) 
            {
                _watchers.Add(watcher);
            }
        }

        private void OnWatcherChanged(object source, FileSystemEventArgs e)
        {
            lock (this) 
            {
                if (!_triggerred) 
                {
                    _triggerred = true;
                    // Console.WriteLine("OnWatcherChanged()");
                    OnChange(source, e);
                }
            }
        }

        public event EventHandler OnChange;
    }
}

#endif
