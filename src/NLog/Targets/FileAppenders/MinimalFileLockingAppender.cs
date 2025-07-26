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
    using NLog.Internal;

#if NETFRAMEWORK
    [System.Security.SecuritySafeCritical]
#endif
    internal sealed class MinimalFileLockingAppender : IFileAppender
    {
        private readonly FileTarget _fileTarget;
        private readonly string _filePath;
        private bool _initialFileOpen;

        public string FilePath => _filePath;

        public DateTime OpenStreamTime { get; }

        public DateTime FileLastModified
        {
            get
            {
                try
                {
                    var fileInfo = new FileInfo(_filePath);
                    if (fileInfo.Exists && fileInfo.Length != 0)
                    {
                        return Time.TimeSource.Current.FromSystemTime(fileInfo.LastWriteTimeUtc);
                    }
                    return OpenStreamTime;
                }
                catch (Exception ex)
                {
                    NLog.Common.InternalLogger.Error(ex, "{0}: Failed to lookup FileInfo.LastWriteTimeUtc for file: {1}", _fileTarget, _filePath);
                    if (ex.MustBeRethrown())
                        throw;

                    return OpenStreamTime;
                }
            }
        }

        public DateTime NextArchiveTime
        {
            get
            {
                if (_nextArchiveTime < NLog.Time.TimeSource.Current.Time.AddMinutes(1) || _lastFileBirthTimeUtc == DateTime.MinValue)
                {
                    var fileInfo = new FileInfo(_filePath);
                    var fileBirthTimeUtc = (fileInfo.Exists && fileInfo.Length != 0) ? (FileInfoHelper.LookupValidFileCreationTimeUtc(fileInfo) ?? DateTime.MinValue) : DateTime.MinValue;
                    if (fileBirthTimeUtc == DateTime.MinValue || _lastFileBirthTimeUtc < fileBirthTimeUtc)
                    {
                        var fileBirthTime = fileBirthTimeUtc != DateTime.MinValue ? NLog.Time.TimeSource.Current.FromSystemTime(fileBirthTimeUtc) : OpenStreamTime;
                        _nextArchiveTime = FileTarget.CalculateNextArchiveEventTime(_fileTarget.ArchiveEvery, fileBirthTime);
                        _lastFileBirthTimeUtc = fileBirthTimeUtc;
                    }
                }
                return _nextArchiveTime;
            }
        }
        private DateTime _nextArchiveTime;
        private DateTime _lastFileBirthTimeUtc;

        public long FileSize
        {
            get
            {
                try
                {
                    var fileInfo = new FileInfo(_filePath);
                    var fileSize = fileInfo.Exists ? fileInfo.Length : 0;
                    return fileSize;
                }
                catch (Exception ex)
                {
                    NLog.Common.InternalLogger.Error(ex, "{0}: Failed to lookup FileInfo.Length for file: {1}", _fileTarget, _filePath);
                    if (ex.MustBeRethrown())
                        throw;

                    return 0;
                }
            }
        }

        public MinimalFileLockingAppender(FileTarget fileTarget, string filePath)
        {
            _fileTarget = fileTarget;
            _filePath = filePath;
            _initialFileOpen = true;
            OpenStreamTime = Time.TimeSource.Current.Time;
        }

        public void Write(byte[] buffer, int offset, int count)
        {
            int overrideBufferSize = Math.Min((count / 4096 + 1) * 4096, _fileTarget.BufferSize);

            var initialFileOpen = _initialFileOpen;
            _initialFileOpen = false;

            using (var fileStream = _fileTarget.CreateFileStreamWithRetry(this, overrideBufferSize, initialFileOpen))
            {
                fileStream.Write(buffer, offset, count);

                if (_fileTarget.ReplaceFileContentsOnEachWrite)
                {
                    var footerBytes = _fileTarget.GetFooterLayoutBytes();
                    if (footerBytes?.Length > 0)
                    {
                        fileStream.Write(footerBytes, 0, footerBytes.Length);
                    }
                }
            }
        }

        public void Dispose()
        {
            // Nothing to dispose
        }

        public void Flush()
        {
            // Nothing to flush
        }

        public bool VerifyFileExists()
        {
            return FileSize != 0;
        }

        public override string ToString() => _filePath;
    }
}
