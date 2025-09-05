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

namespace NLog.Targets.FileArchiveHandlers
{
    using System;
    using System.IO;
    using System.Linq;
    using NLog.Common;
    using NLog.Internal;

    /// <summary>
    /// Rolls the active-file to the next sequence-number
    /// </summary>
    internal class RollingArchiveFileHandler : BaseFileArchiveHandler, IFileArchiveHandler
    {
        public RollingArchiveFileHandler(FileTarget fileTarget)
            :base(fileTarget)
        {
        }

        public virtual int ArchiveBeforeOpenFile(string newFileName, LogEventInfo firstLogEvent, DateTime? previousFileLastModified, int newSequenceNumber)
        {
            bool initialFileOpen = newSequenceNumber == 0;

            if (_fileTarget.MaxArchiveFiles >= 0 || _fileTarget.MaxArchiveDays > 0 || (initialFileOpen && _fileTarget.DeleteOldFileOnStartup))
            {
                var newFilePath = FileTarget.CleanFullFilePath(newFileName);
                var archiveSuffixWithSeqNo = !Path.GetFileNameWithoutExtension(newFilePath).Any(c => char.IsDigit(c));
                bool deletedOldFiles = DeleteOldFilesBeforeArchive(newFilePath, initialFileOpen, archiveSuffixWithSeqNo: archiveSuffixWithSeqNo);

                if (_fileTarget.MaxArchiveFiles == 0 || _fileTarget.MaxArchiveFiles == 1 || (initialFileOpen && _fileTarget.DeleteOldFileOnStartup))
                {
                    if (deletedOldFiles)
                    {
                        FixWindowsFileSystemTunneling(newFilePath);
                    }
                    return 0;
                }
            }

            if (initialFileOpen)
            {
                if (_fileTarget.ArchiveOldFileOnStartup || _fileTarget.ArchiveAboveSize > 0 || _fileTarget.ArchiveEvery != FileArchivePeriod.None)
                {
                    var newFilePath = FileTarget.CleanFullFilePath(newFileName);
                    return RollToInitialSequenceNumber(newFilePath);
                }
            }

            return newSequenceNumber;
        }

        private int RollToInitialSequenceNumber(string newFilePath)
        {
            int newSequenceNumber = 0;

            try
            {
                if (AllowOptimizedRollingForArchiveAboveSize())
                {
                    // Fast FileSize check of a single file, and skip enumerating all archive-files
                    var newFileInfo = new FileInfo(newFilePath);
                    var newFileLength = newFileInfo.Exists ? newFileInfo.Length : 0;
                    if (newFileLength > 0 && newFileLength < _fileTarget.ArchiveAboveSize)
                    {
                        InternalLogger.Debug("{0}: Archive rolling skipped because file-size={1} < ArchiveAboveSize for file: {2}", _fileTarget, newFileLength, newFilePath);
                        return newSequenceNumber;
                    }
                }

                var filedir = Path.GetDirectoryName(newFilePath);
                if (string.IsNullOrEmpty(filedir))
                    return 0;

                var directoryInfo = new DirectoryInfo(filedir);
                if (!directoryInfo.Exists)
                {
                    InternalLogger.Debug("{0}: Archive Sequence Rolling found no files in directory: {1}", _fileTarget, filedir);
                    return 0;
                }

                var filename = Path.GetFileNameWithoutExtension(newFilePath);
                var fileext = Path.GetExtension(newFilePath);
                var fileWildCard = filename + "*" + fileext;
                var fileInfos = directoryInfo.GetFiles(fileWildCard);
                InternalLogger.Debug("{0}: Archive Sequence Rolling found {1} files matching wildcard {2} in directory: {3}", _fileTarget, fileInfos.Length, fileWildCard, filedir);
                if (fileInfos.Length == 0)
                    return 0;

                if (_fileTarget.DeleteOldFileOnStartup)
                {
                    // Delete all files in the directory with matching filename-wildcard
                    foreach (var fileInfo in fileInfos)
                    {
                        DeleteOldArchiveFile(fileInfo.FullName, "DeleteOldFileOnStartup=true");
                    }

                    return 0;
                }

                var archivePathWildCard = _fileTarget.BuildFullFilePath(newFilePath, int.MaxValue, DateTime.MinValue).Replace(int.MaxValue.ToString(), "*");
                var archiveFileName = Path.GetFileName(archivePathWildCard);
                var fileWildcardStartIndex = archiveFileName.IndexOf('*');
                var fileWildcardEndIndex = fileWildcardStartIndex >= 0 ? archiveFileName.Length - fileWildcardStartIndex : -1;

                // Search for matching files to find the highest sequence-number
                newSequenceNumber = GetMaxArchiveSequenceNo(fileInfos, fileWildcardStartIndex, fileWildcardEndIndex) ?? 0;

                if (_fileTarget.ArchiveOldFileOnStartup)
                {
                    // Search for matching files to find the highest sequence-number, and roll to next sequence-no
                    newSequenceNumber += 1;
                }

                return newSequenceNumber;
            }
            catch (Exception ex)
            {
                InternalLogger.Warn(ex, "{0}: Failed to resolve initial archive sequence number for file: {1}", _fileTarget, newFilePath);
                if (ex.MustBeRethrown(_fileTarget))
                    throw;

                return newSequenceNumber;
            }
        }

        private bool AllowOptimizedRollingForArchiveAboveSize()
        {
            return _fileTarget.ArchiveAboveSize > 0 && _fileTarget.ArchiveEvery == FileArchivePeriod.None && !_fileTarget.ArchiveOldFileOnStartup && !_fileTarget.DeleteOldFileOnStartup && _fileTarget.GetType().Equals(typeof(FileTarget));
        }
    }
}
