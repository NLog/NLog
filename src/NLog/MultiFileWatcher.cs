using System;
using System.Collections;
using System.IO;

namespace NLog
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
            lock (this) {
                foreach (FileSystemWatcher watcher in _watchers) {
                    // Console.WriteLine("releasing watch path: {0} filter: {1}", watcher.Path, watcher.Filter);
                    watcher.EnableRaisingEvents = false;
                    watcher.Dispose();
                }
                _watchers.Clear();
                _triggerred = false;
            }
        }

        public void Watch(ICollection fileNames)
        {
            if (fileNames == null)
                return;
            foreach (string s in fileNames) {
                Watch(s);
            }
        }

        public void Watch(string fileName) {
            FileSystemWatcher watcher = new FileSystemWatcher();
            watcher.Path = Path.GetDirectoryName(fileName);
            watcher.Filter = Path.GetFileName(fileName);
            // Console.WriteLine("watching path: {0} filter: {1}", watcher.Path, watcher.Filter);
            watcher.NotifyFilter = NotifyFilters.LastWrite;
            watcher.Changed += new FileSystemEventHandler(this.OnWatcherChanged);
            watcher.EnableRaisingEvents = true;

            lock (this) {
                _watchers.Add(watcher);
            }
        }

        private void OnWatcherChanged(object source, FileSystemEventArgs e)
        {
            lock (this) {
                if (!_triggerred) {
                    _triggerred = true;
                    // Console.WriteLine("OnWatcherChanged()");
                    OnChange(source, e);
                }
            }
        }

        public event EventHandler OnChange;
    }
}
