// 
// Copyright (c) 2004-2020 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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
#define SupportsMutex
#endif

namespace NLog.Internal.FileAppenders
{
    using System;
    using System.IO;
    using System.Threading;
    using NLog.Common;

    /// <summary>
    /// Maintains a collection of file appenders usually associated with file targets.
    /// </summary>
    internal sealed class FileAppenderCache : IFileAppenderCache
    {
        private readonly BaseFileAppender[] _appenders;
        private Timer _autoClosingTimer;

#if !NETSTANDARD1_3
        private string _archiveFilePatternToWatch;
        private readonly MultiFileWatcher _externalFileArchivingWatcher = new MultiFileWatcher(NotifyFilters.DirectoryName | NotifyFilters.FileName);
        private bool _logFileWasArchived;
#endif

        /// <summary>
        /// An "empty" instance of the <see cref="FileAppenderCache"/> class with zero size and empty list of appenders.
        /// </summary>
        public static readonly FileAppenderCache Empty = new FileAppenderCache();

        /// <summary>
        /// Initializes a new "empty" instance of the <see cref="FileAppenderCache"/> class with zero size and empty
        /// list of appenders.
        /// </summary>
        private FileAppenderCache() : this(0, null, null) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileAppenderCache"/> class.
        /// </summary>
        /// <remarks>
        /// The size of the list should be positive. No validations are performed during initialization as it is an
        /// internal class.
        /// </remarks>
        /// <param name="size">Total number of appenders allowed in list.</param>
        /// <param name="appenderFactory">Factory used to create each appender.</param>
        /// <param name="createFileParams">Parameters used for creating a file.</param>
        public FileAppenderCache(int size, IFileAppenderFactory appenderFactory, ICreateFileParameters createFileParams)
        {
            Size = size;
            Factory = appenderFactory;
            CreateFileParameters = createFileParams;

            _appenders = new BaseFileAppender[Size];

            _autoClosingTimer = new Timer(AutoClosingTimerCallback, null, Timeout.Infinite, Timeout.Infinite);

#if !NETSTANDARD1_3
            _externalFileArchivingWatcher.FileChanged += ExternalFileArchivingWatcher_OnFileChanged;
#endif
        }

#if !NETSTANDARD1_3
        private void ExternalFileArchivingWatcher_OnFileChanged(object sender, FileSystemEventArgs e)
        {
            if (_logFileWasArchived || CheckCloseAppenders == null || _autoClosingTimer == null)
            {
                return;
            }

            if (FileAppenderFolderChanged(e.FullPath))
            {
                if ((e.ChangeType & (WatcherChangeTypes.Deleted | WatcherChangeTypes.Renamed)) != 0)
                    _logFileWasArchived = true;  // File Appender file deleted/renamed
            }
            else
            {
                if ((e.ChangeType & WatcherChangeTypes.Created) == WatcherChangeTypes.Created)
                    _logFileWasArchived = true;  // Something was created in the archive folder
            }

            if (_logFileWasArchived && _autoClosingTimer != null)
            {
                _autoClosingTimer.Change(50, Timeout.Infinite);
            }
        }

        private bool FileAppenderFolderChanged(string fullPath)
        {
            if (!string.IsNullOrEmpty(fullPath))
            {
                if (string.IsNullOrEmpty(_archiveFilePatternToWatch))
                {
                    return true;
                }
                else
                {
                    string archiveFolderPath = Path.GetDirectoryName(_archiveFilePatternToWatch);
                    if (!string.IsNullOrEmpty(archiveFolderPath))
                    {
                        string currentFolderPath = Path.GetDirectoryName(fullPath);
                        return !string.Equals(archiveFolderPath, currentFolderPath, StringComparison.OrdinalIgnoreCase);
                    }
                    else
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// The archive file path pattern that is used to detect when archiving occurs.
        /// </summary>
        public string ArchiveFilePatternToWatch
        {
            get => _archiveFilePatternToWatch;
            set
            {
                if (_archiveFilePatternToWatch != value)
                {
                    if (!string.IsNullOrEmpty(_archiveFilePatternToWatch))
                    {
                        string directoryPath = Path.GetDirectoryName(_archiveFilePatternToWatch);
                        if (string.IsNullOrEmpty(directoryPath))
                            _externalFileArchivingWatcher.StopWatching(directoryPath);
                    }

                    _archiveFilePatternToWatch = value;

                    _logFileWasArchived = false;
                }
            }
        }

        /// <summary>
        /// Invalidates appenders for all files that were archived.
        /// </summary>
        public void InvalidateAppendersForArchivedFiles()
        {
            if (_logFileWasArchived)
            {
                _logFileWasArchived = false;
                InternalLogger.Trace("{0}: Invalidate archived files", CreateFileParameters);
                CloseAppenders("Cleanup Archive");
            }
        }
#endif

        private void AutoClosingTimerCallback(object state)
        {
            var checkCloseAppenders = CheckCloseAppenders;
            if (checkCloseAppenders != null)
            {
                checkCloseAppenders(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Gets the parameters which will be used for creating a file.
        /// </summary>
        public ICreateFileParameters CreateFileParameters { get; private set; }

        /// <summary>
        /// Gets the file appender factory used by all the appenders in this list.
        /// </summary>
        public IFileAppenderFactory Factory { get; private set; }

        /// <summary>
        /// Gets the number of appenders which the list can hold.
        /// </summary>
        public int Size { get; private set; }

        /// <summary>
        /// Subscribe to background monitoring of active file appenders
        /// </summary>
        public event EventHandler CheckCloseAppenders;

        /// <summary>
        /// It allocates the first slot in the list when the file name does not already in the list and clean up any
        /// unused slots.
        /// </summary>
        /// <param name="fileName">File name associated with a single appender.</param>
        /// <returns>The allocated appender.</returns>
        /// <exception cref="NullReferenceException">
        /// Thrown when <see cref="M:AllocateAppender"/> is called on an <c>Empty</c><see cref="FileAppenderCache"/> instance.
        /// </exception>
        public BaseFileAppender AllocateAppender(string fileName)
        {
            //
            // BaseFileAppender.Write is the most expensive operation here
            // so the in-memory data structure doesn't have to be 
            // very sophisticated. It's a table-based LRU, where we move 
            // the used element to become the first one.
            // The number of items is usually very limited so the 
            // performance should be equivalent to the one of the hashtable.
            //

            BaseFileAppender appenderToWrite = null;
            int freeSpot = _appenders.Length - 1;

            for (int i = 0; i < _appenders.Length; ++i)
            {
                // Use empty slot in recent appender list, if there is one.
                if (_appenders[i] == null)
                {
                    freeSpot = i;
                    break;
                }

                if (string.Equals(_appenders[i].FileName, fileName, StringComparison.OrdinalIgnoreCase))
                {
                    // found it, move it to the first place on the list
                    // (MRU)
                    BaseFileAppender app = _appenders[i];
                    if (i > 0)
                    {
                        // file open has a chance of failure
                        // if it fails in the constructor, we won't modify any data structures
                        for (int j = i; j > 0; --j)
                        {
                            _appenders[j] = _appenders[j - 1];
                        }

                        _appenders[0] = app;
                    }
                    appenderToWrite = app;
                    break;
                }
            }

            if (appenderToWrite == null)
            {
                appenderToWrite = CreateAppender(fileName, freeSpot);
            }

            return appenderToWrite;
        }

        private BaseFileAppender CreateAppender(string fileName, int freeSpot)
        {
            BaseFileAppender appenderToWrite;
            try
            {
                InternalLogger.Debug("{0}: Creating file appender: {1}", CreateFileParameters, fileName);
                BaseFileAppender newAppender = Factory.Open(fileName, CreateFileParameters);

                if (_appenders[freeSpot] != null)
                {
                    CloseAppender(_appenders[freeSpot], "Stale", false);
                    _appenders[freeSpot] = null;
                }

                for (int j = freeSpot; j > 0; --j)
                {
                    _appenders[j] = _appenders[j - 1];
                }

                _appenders[0] = newAppender;
                appenderToWrite = newAppender;

                if (CheckCloseAppenders != null)
                {
#if !NETSTANDARD1_3
                    if (freeSpot == 0)
                        _logFileWasArchived = false;
                    if (!string.IsNullOrEmpty(_archiveFilePatternToWatch))
                    {
                        string directoryPath = Path.GetDirectoryName(_archiveFilePatternToWatch);
                        if (!Directory.Exists(directoryPath))
                            Directory.CreateDirectory(directoryPath);

                        _externalFileArchivingWatcher.Watch(_archiveFilePatternToWatch); // Always monitor the archive-folder
                    }
                    _externalFileArchivingWatcher.Watch(appenderToWrite.FileName); // Monitor the active file-appender
#endif
                }
            }
            catch (Exception ex)
            {
                InternalLogger.Warn(ex, "{0}: Failed to create file appender: {1}", CreateFileParameters, fileName);
                throw;
            }
            return appenderToWrite;
        }

        /// <summary>
        /// Close all the allocated appenders. 
        /// </summary>
        public void CloseAppenders(string reason)
        {
            for (int i = 0; i < _appenders.Length; ++i)
            {
                var oldAppender = _appenders[i];
                if (oldAppender == null)
                {
                    break;
                }

                CloseAppender(oldAppender, reason, true);
                _appenders[i] = null;
                oldAppender.Dispose();  // Dispose of Archive Mutex
            }
        }

        /// <summary>
        /// Close the allocated appenders initialized before the supplied time.
        /// </summary>
        /// <param name="expireTime">The time which prior the appenders considered expired</param>
        public void CloseAppenders(DateTime expireTime)
        {
#if !NETSTANDARD1_3
            if (_logFileWasArchived)
            {
                _logFileWasArchived = false;
                CloseAppenders("Cleanup Timer");
            }
            else
#endif
            {
                if (expireTime != DateTime.MinValue)
                {
                    for (int i = 0; i < _appenders.Length; ++i)
                    {
                        if (_appenders[i] == null)
                        {
                            break;
                        }

                        if (_appenders[i].OpenTimeUtc < expireTime)
                        {
                            for (int j = i; j < _appenders.Length; ++j)
                            {
                                var oldAppender = _appenders[j];
                                if (oldAppender == null)
                                {
                                    break;
                                }

                                CloseAppender(oldAppender, "Expired", i == 0);
                                _appenders[j] = null;
                                oldAppender.Dispose();  // Dispose of Archive Mutex
                            }

                            break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Flush all the allocated appenders. 
        /// </summary>
        public void FlushAppenders()
        {
            foreach (BaseFileAppender appender in _appenders)
            {
                if (appender == null)
                {
                    break;
                }

                try
                {
                    appender.Flush();
                }
                catch (Exception ex)
                {
                    InternalLogger.Error(ex, "{0}: Failed to flush file '{1}'.", CreateFileParameters, appender.FileName);
                    InvalidateAppender(appender.FileName)?.Dispose();
                    throw;
                }
            }
        }

        private BaseFileAppender GetAppender(string fileName)
        {
            for (int i = 0; i < _appenders.Length; ++i)
            {
                BaseFileAppender appender = _appenders[i];
                if (appender == null)
                    break;

                if (string.Equals(appender.FileName, fileName, StringComparison.OrdinalIgnoreCase))
                    return appender;
            }

            return null;
        }

        public DateTime? GetFileCreationTimeSource(string filePath, DateTime? fallbackTimeSource = null)
        {
            var appender = GetAppender(filePath);
            DateTime? result = null;
            if (appender != null)
            {
                try
                {
                    result = appender.GetFileCreationTimeUtc();
                    if (result.HasValue)
                    {
                        // Check if cached value is still valid, and update if not (Will automatically update CreationTimeSource)
                        DateTime cachedTimeUtc = appender.CreationTimeUtc;
                        if (result.Value != cachedTimeUtc)
                        {
                            appender.CreationTimeUtc = result.Value;
                        }
                        return appender.CreationTimeSource;
                    }
                }
                catch (Exception ex)
                {
                    InternalLogger.Error(ex, "{0}: Failed to get file creation time for file '{1}'.", CreateFileParameters, appender.FileName);
                    InvalidateAppender(appender.FileName)?.Dispose();
                    throw;
                }
            }

            var fileInfo = new FileInfo(filePath);
            if (fileInfo.Exists)
            {
                result = fileInfo.LookupValidFileCreationTimeUtc(fallbackTimeSource);
                return Time.TimeSource.Current.FromSystemTime(result.Value);
            }

            return result;
        }

        /// <summary>
        /// File Archive Logic uses the File-Creation-TimeStamp to detect if time to archive, and the File-LastWrite-Timestamp to name the archive-file.
        /// </summary>
        /// <remarks>
        /// NLog always closes all relevant appenders during archive operation, so no need to lookup file-appender
        /// </remarks>
        public DateTime? GetFileLastWriteTimeUtc(string filePath)
        {
            var fileInfo = new FileInfo(filePath);
            if (fileInfo.Exists)
            {
                return fileInfo.GetLastWriteTimeUtc();
            }

            return null;
        }

        public long? GetFileLength(string filePath)
        {
            var appender = GetAppender(filePath);
            if (appender != null)
            {
                try
                {
                    var result = appender.GetFileLength();
                    if (result.HasValue)
                    {
                        return result;
                    }
                }
                catch (IOException ex)
                {
                    InternalLogger.Error(ex, "{0}: Failed to get length for file '{1}'.", CreateFileParameters, appender.FileName);
                    InvalidateAppender(appender.FileName)?.Dispose();
                    // Do not rethrow as failed archive-file-size check should not drop logevents
                }
                catch (Exception ex)
                {
                    InternalLogger.Error(ex, "{0}: Failed to get length for file '{1}'.", CreateFileParameters, appender.FileName);
                    InvalidateAppender(appender.FileName)?.Dispose();
                    throw;
                }
            }

            var fileInfo = new FileInfo(filePath);
            if (fileInfo.Exists)
            {
                return fileInfo.Length;
            }

            return null;
        }

        /// <summary>
        /// Closes the specified appender and removes it from the list. 
        /// </summary>
        /// <param name="filePath">File name of the appender to be closed.</param>
        /// <returns>File Appender that matched the filePath (null if none found)</returns>
        public BaseFileAppender InvalidateAppender(string filePath)
        {
            for (int i = 0; i < _appenders.Length; ++i)
            {
                var oldAppender = _appenders[i];
                if (oldAppender == null)
                {
                    break;
                }

                if (string.Equals(oldAppender.FileName, filePath, StringComparison.OrdinalIgnoreCase))
                {
                    for (int j = i; j < _appenders.Length - 1; ++j)
                    {
                        _appenders[j] = _appenders[j + 1];
                    }
                    _appenders[_appenders.Length - 1] = null;
                    CloseAppender(oldAppender, "Invalidate", _appenders[0] == null);
                    return oldAppender; // Return without Dispose of Archive Mutex
                }
            }
            return null;
        }

        private void CloseAppender(BaseFileAppender appender, string reason, bool lastAppender)
        {
            InternalLogger.Debug("{0}: FileAppender Closing {1} - {2}", CreateFileParameters, reason, appender.FileName);

            if (lastAppender)
            {
                // No active appenders, deactivate background tasks
                _autoClosingTimer.Change(Timeout.Infinite, Timeout.Infinite);

#if !NETSTANDARD1_3
                _externalFileArchivingWatcher.StopWatching();
                _logFileWasArchived = false;
            }
            else
            {
                _externalFileArchivingWatcher.StopWatching(appender.FileName);
#endif
            }

            appender.Close();
        }

        public void Dispose()
        {
            CheckCloseAppenders = null;

#if !NETSTANDARD1_3
            _externalFileArchivingWatcher.Dispose();
            _logFileWasArchived = false;
#endif

            var currentTimer = _autoClosingTimer;
            if (currentTimer != null)
            {
                _autoClosingTimer = null;
                currentTimer.WaitForDispose(TimeSpan.Zero);
            }
        }
    }
}
