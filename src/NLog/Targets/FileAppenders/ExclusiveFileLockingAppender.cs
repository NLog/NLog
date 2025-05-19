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

namespace NLog.Targets.FileAppenders
{
    using System;
    using System.IO;
    using NLog.Common;
    using NLog.Internal;

#if NETFRAMEWORK
    [System.Security.SecuritySafeCritical]
#endif
    internal sealed class ExclusiveFileLockingAppender : IFileAppender
    {
        private readonly FileTarget _fileTarget;
        private readonly string _filePath;
        private Stream _fileStream;
        private int _lastFileDeletedCheck;
        private long? _countedFileSize;

        public string FilePath => _filePath;

        public DateTime OpenStreamTime { get; }

        public DateTime FileLastModified { get; private set; }

        private DateTime FileBirthTime
        {
            get => _fileBirthTime ?? OpenStreamTime;
            set => _fileBirthTime = value;
        }
        private DateTime? _fileBirthTime;

        public DateTime NextArchiveTime
        {
            get
            {
                var fileBirthTime = FileBirthTime;
                if (_lastFileBirthTime != fileBirthTime)
                {
                    _nextArchiveTime = FileTarget.CalculateNextArchiveEventTime(_fileTarget.ArchiveEvery, fileBirthTime);
                    _lastFileBirthTime = fileBirthTime;
                }
                return _nextArchiveTime;
            }
        }
        private DateTime _nextArchiveTime;
        private DateTime _lastFileBirthTime;

        public long FileSize => _countedFileSize ?? _fileStream.Length;

        public ExclusiveFileLockingAppender(FileTarget fileTarget, string filePath)
        {
            _fileTarget = fileTarget;
            _filePath = filePath;
            OpenStreamTime = Time.TimeSource.Current.Time;
            _lastFileDeletedCheck = Environment.TickCount;

            RefreshFileBirthTimeUtc(true);

            _fileStream = _fileTarget.CreateFileStreamWithRetry(this, fileTarget.BufferSize, initialFileOpen: true);
            _countedFileSize = RefreshCountedFileSize();
        }

        private bool SkipRefreshFileBirthTime()
        {
            return (_fileTarget.ArchiveFileName is null && _fileTarget.ArchiveEvery == FileArchivePeriod.None);
        }

        private void RefreshFileBirthTimeUtc(bool forceRefresh)
        {
            FileLastModified = NLog.Time.TimeSource.Current.Time;

            if (SkipRefreshFileBirthTime())
                return;

            try
            {
                FileInfo fileInfo = new FileInfo(_filePath);
                if (fileInfo.Exists && fileInfo.Length != 0)
                {
                    var fileBirthTimeUtc = FileInfoHelper.LookupValidFileCreationTimeUtc(fileInfo) ?? DateTime.MinValue;
                    var fileBirthTime = fileBirthTimeUtc != DateTime.MinValue ? NLog.Time.TimeSource.Current.FromSystemTime(fileBirthTimeUtc) : OpenStreamTime;
                    if (!forceRefresh && fileBirthTime.Date < FileBirthTime.Date)
                        fileBirthTime = FileBirthTime;
                    FileBirthTime = fileBirthTime;
                    FileLastModified = NLog.Time.TimeSource.Current.FromSystemTime(fileInfo.LastWriteTimeUtc);
                }
            }
            catch (Exception ex)
            {
                InternalLogger.Debug(ex, "{0}: Failed to refresh BirthTime for file: '{1}'", _fileTarget, _filePath);
            }
        }

        public void Write(byte[] buffer, int offset, int count)
        {
            var lastFileSizeCheck = Environment.TickCount - _lastFileDeletedCheck;
            if (lastFileSizeCheck > 1000 || lastFileSizeCheck < -1000)
            {
                MonitorFileHasBeenDeleted();
                _lastFileDeletedCheck = Environment.TickCount;
                if (!SkipRefreshFileBirthTime())
                    FileLastModified = NLog.Time.TimeSource.Current.Time;
            }

            _fileStream.Write(buffer, offset, count);
            if (_countedFileSize.HasValue)
                _countedFileSize += count;
        }

        public void Flush()
        {
            _fileStream.Flush();
        }

        public void Dispose()
        {
            SafeCloseFile(_filePath, _fileStream);
        }

        public bool VerifyFileExists()
        {
            return SafeFileExists(_filePath);
        }

        private void MonitorFileHasBeenDeleted()
        {
            if (!SafeFileExists(_filePath))
            {
                InternalLogger.Debug("{0}: Recreating FileStream because no longer File.Exists: '{1}'", _fileTarget, _filePath);
                SafeCloseFile(_filePath, _fileStream);
                _fileStream = _fileTarget.CreateFileStreamWithRetry(this, _fileTarget.BufferSize, initialFileOpen: false);
                _countedFileSize = RefreshCountedFileSize();
                RefreshFileBirthTimeUtc(false);
            }
        }

        private long? RefreshCountedFileSize()
        {
            return (_fileTarget.ArchiveAboveSize > 0 && _fileTarget.GetType().Equals(typeof(FileTarget))) ? _fileStream.Length : default(long?);
        }

        private void SafeCloseFile(string filepath, Stream? fileStream)
        {
            try
            {
                var stream = fileStream;
                stream?.Dispose();
            }
            catch (Exception ex)
            {
                InternalLogger.Error(ex, "{0}: Failed to close file: '{1}'", _fileTarget, filepath);
            }
        }

        private bool SafeFileExists(string filepath)
        {
            try
            {
                return File.Exists(filepath);
            }
            catch (Exception ex)
            {
                InternalLogger.Error(ex, "{0}: Failed to check if File.Exists: '{1}'", _fileTarget, filepath);
                return false;
            }
        }

        public override string ToString() => _filePath;
    }
}
