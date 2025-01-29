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
    using NLog.Common;
    using NLog.Internal;
    using NLog.Layouts;

    /// <summary>
    /// Legacy archive logic with file-move of active-file to file-path specified by <see cref="FileTarget.ArchiveFileName" />
    /// </summary>
    /// <remarks>
    /// Kept mostly for legacy reasons, because archive operation can fail because of file-locks by other applications (or by multi-process logging).
    ///
    /// Avoid using <see cref="FileTarget.ArchiveFileName"/> when possible, and instead rely on only using <see cref="FileTarget.FileName"/> and <see cref="FileTarget.ArchiveSuffixFormat"/>.
    /// </remarks>
    internal sealed class LegacyArchiveFileNameHandler : RollingArchiveFileHandler, IFileArchiveHandler
    {
        public LegacyArchiveFileNameHandler(FileTarget fileTarget)
            : base(fileTarget)
        {
        }

        public override int ArchiveBeforeOpenFile(string newFileName, LogEventInfo firstLogEvent, DateTime? previousFileLastModified, int newSequenceNumber)
        {
            var archiveFileName = _fileTarget.ArchiveFileName?.Render(firstLogEvent);
            if (StringHelpers.IsNullOrWhiteSpace(archiveFileName))
            {
                return base.ArchiveBeforeOpenFile(newFileName, firstLogEvent, previousFileLastModified, newSequenceNumber);
            }

            var newFilePath = FileTarget.CleanFullFilePath(newFileName);

            bool initialFileOpen = newSequenceNumber == 0;
            if (ArchiveBeforeOpenFile(archiveFileName, newFilePath, firstLogEvent, previousFileLastModified, initialFileOpen))
            {
                FixWindowsFileSystemTunneling(newFilePath);
            }

            return 0;
        }

        private bool ArchiveBeforeOpenFile(string archiveFileName, string newFilePath, LogEventInfo firstLogEvent, DateTime? previousFileLastModified, bool initialFileOpen)
        {
            bool oldFilesDeleted = false;
            if (_fileTarget.MaxArchiveFiles >= 0 || _fileTarget.MaxArchiveDays > 0 || (initialFileOpen && _fileTarget.DeleteOldFileOnStartup))
            {
                bool wildCardStrictSeqNo = _fileTarget.ArchiveSuffixFormat.IndexOf("{0", StringComparison.Ordinal) >= 0 && _fileTarget.ArchiveSuffixFormat.IndexOf("{1", StringComparison.Ordinal) < 0 &&
                    !(_fileTarget.ArchiveFileName is SimpleLayout simpleLayout && (simpleLayout.OriginalText.IndexOf("${date", StringComparison.OrdinalIgnoreCase) >= 0 || simpleLayout.OriginalText.IndexOf("${shortdate", StringComparison.OrdinalIgnoreCase) >= 0));

                var excludeFileName = Path.GetFileName(newFilePath);
                if (wildCardStrictSeqNo)
                {
                    string archiveFilePath = BuildArchiveFilePath(archiveFileName, int.MaxValue, DateTime.MinValue);
                    string archiveFileWildcard = archiveFilePath.Replace(int.MaxValue.ToString(), "*");
                    string archiveDirectory = Path.GetDirectoryName(archiveFilePath);
                    oldFilesDeleted = DeleteOldFilesBeforeArchive(archiveDirectory, Path.GetFileName(archiveFileWildcard), initialFileOpen, excludeFileName, true);
                }
                else
                {
                    var archiveFilePath = FileTarget.CleanFullFilePath(archiveFileName);
                    oldFilesDeleted = DeleteOldFilesBeforeArchive(archiveFilePath, initialFileOpen, excludeFileName);
                }
            }

            if (initialFileOpen && !_fileTarget.ArchiveOldFileOnStartup)
                return oldFilesDeleted;

            if (!ArchiveOldFileWithRetry(archiveFileName, newFilePath, firstLogEvent, previousFileLastModified))
                return oldFilesDeleted;

            return true;
        }

        private bool ArchiveOldFileWithRetry(string archiveFileName, string newFilePath, LogEventInfo firstLogEvent, DateTime? previousFileLastModified)
        {
            DateTime? lastWriteTimeUtc = default(DateTime?);
            long? lastFileLength = default(long?);

            bool oldFilesDeleted = false;

            for (int i = 1; i <= 3; ++i)
            {
                try
                {
                    var newFileInfo = new FileInfo(newFilePath);
                    if (!newFileInfo.Exists)
                        return oldFilesDeleted;

                    oldFilesDeleted = true;
                    if (HasFileInfoChanged(newFileInfo, lastWriteTimeUtc, lastFileLength))
                        return false;   // File archive probably completed by someone else, and new file already created

                    lastWriteTimeUtc = lastWriteTimeUtc ?? newFileInfo.LastWriteTimeUtc;
                    lastFileLength = lastFileLength ?? newFileInfo.Length;
                    if (ArchiveOldFile(archiveFileName, newFileInfo, firstLogEvent, previousFileLastModified))
                        return true;
                }
                catch (IOException ex)
                {
                    InternalLogger.Debug(ex, "{0}: Failed to archive file, maybe file is locked: '{1}'", _fileTarget, newFilePath);
                    if (!File.Exists(newFilePath))
                        return oldFilesDeleted;
                    if (i >= 3 && LogManager.ThrowExceptions)
                        throw;
                }
                catch (Exception ex)
                {
                    InternalLogger.Debug(ex, "{0}: Failed to archive file: '{1}'", _fileTarget, newFilePath);
                    if (LogManager.ThrowExceptions)
                        throw;
                    System.Threading.Thread.Sleep(i * 10);
                }
            }

            return oldFilesDeleted;
        }

        private static bool HasFileInfoChanged(FileInfo newFileInfo, DateTime? lastWriteTimeUtc, long? lastFileLength)
        {
            if (lastWriteTimeUtc.HasValue && lastWriteTimeUtc.Value != newFileInfo.LastWriteTimeUtc)
                return true;   

            if (lastFileLength.HasValue && lastFileLength.Value != newFileInfo.Length)
                return true;

            return false;
        }

        private bool ArchiveOldFile(string archiveFileName, FileInfo newFileInfo, LogEventInfo firstLogEvent, DateTime? previousFileLastModified)
        {
            DateTime fileLastWriteTime = Time.TimeSource.Current.FromSystemTime(newFileInfo.LastWriteTimeUtc);
            if (previousFileLastModified.HasValue && (previousFileLastModified > fileLastWriteTime || fileLastWriteTime >= firstLogEvent.TimeStamp))
                fileLastWriteTime = previousFileLastModified.Value;

            var archiveNextSequenceNo = ResolveNextArchiveSequenceNo(archiveFileName, fileLastWriteTime);
            string archiveFullPath = BuildArchiveFilePath(archiveFileName, archiveNextSequenceNo, fileLastWriteTime);

            if (!File.Exists(archiveFullPath))
            {
                // Move active file to archive
                InternalLogger.Info("{0}: Move file from '{1}' to '{2}'", _fileTarget, newFileInfo.FullName, archiveFullPath);
                File.Move(newFileInfo.FullName, archiveFullPath);
                return true;
            }

            if (!newFileInfo.Exists)
            {
                return true;    // No active file to archive, we are done
            }

            if (archiveNextSequenceNo == 0)
            {
                // Append to existing file, and delete old file
                ArchiveFileAppendExisting(newFileInfo.FullName, archiveFullPath);
                return true;
            }

            // Retry (guess new sequence number)
            return false;
        }

        private string BuildArchiveFilePath(string archiveFileName, int archiveNextSequenceNo, DateTime fileLastWriteTime)
        {
            return _fileTarget.BuildFullFilePath(archiveFileName, archiveNextSequenceNo, fileLastWriteTime);
        }

        private void ArchiveFileAppendExisting(string newFilePath, string archiveFilePath)
        {
            // TODO Handle double footer
            InternalLogger.Info("{0}: Already exists, append to {1}", _fileTarget, archiveFilePath);

            var fileShare = FileShare.Read | FileShare.Delete;
            using (FileStream newFileStream = File.Open(newFilePath, FileMode.Open, FileAccess.ReadWrite, fileShare))
            using (FileStream archiveFileStream = File.Open(archiveFilePath, FileMode.Append))
            {
                if (_fileTarget.WriteBom)
                {
                    var preamble = _fileTarget.Encoding.GetPreamble();
                    if (preamble.Length > 0)
                        newFileStream.Seek(preamble.Length, SeekOrigin.Begin);
                }

                byte[] buffer = new byte[4096];
                int read;
                while ((read = newFileStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    archiveFileStream.Write(buffer, 0, read);
                }

                // Reset/Truncate the file
                newFileStream.SetLength(0);

                // Attempt to delete file to reset File-Creation-Time (Delete under file-lock)
                if (!DeleteOldArchiveFile(newFilePath))
                {
                    fileShare &= ~FileShare.Delete;  // Retry after having released file-lock
                }

                newFileStream.Close(); // This flushes the content
            }

            if ((fileShare & FileShare.Delete) == 0)
            {
                DeleteOldArchiveFile(newFilePath); // Attempt to delete file to reset File-Creation-Time
            }
        }

        private int ResolveNextArchiveSequenceNo(string archiveFileName, DateTime fileLastWriteTime)
        {
            // Archive operation triggered, how to resolve the next archive-sequence-number ?
            //  - Old version was able to "parse" the file-names of the archive-folder and "guess" the next sequence number
            var archiveFilePath = BuildArchiveFilePath(archiveFileName, int.MaxValue, fileLastWriteTime);
            var archiveDirectory = Path.GetDirectoryName(archiveFilePath);
            if (string.IsNullOrEmpty(archiveDirectory))
                return 0;

            var directoryInfo = new DirectoryInfo(archiveDirectory);
            if (!directoryInfo.Exists)
            {
                directoryInfo.Create();
            }

            var archiveWildCardFileName = Path.GetFileName(archiveFilePath).Replace(int.MaxValue.ToString(), "*");
            int fileWildcardStartIndex = archiveWildCardFileName.IndexOf('*');
            if (fileWildcardStartIndex < 0)
                return 0;

            int fileWildcardEndIndex = archiveWildCardFileName.Length - fileWildcardStartIndex;
            if (fileWildcardStartIndex > 0 && !char.IsLetter(archiveWildCardFileName[fileWildcardStartIndex - 1]) && !char.IsDigit(archiveWildCardFileName[fileWildcardStartIndex - 1]))
            {
                archiveWildCardFileName = archiveWildCardFileName.Substring(0, fileWildcardStartIndex - 1) + archiveWildCardFileName.Substring(fileWildcardStartIndex);
            }

            var archiveFiles = directoryInfo.GetFiles(archiveWildCardFileName);
            InternalLogger.Debug("{0}: Archive Sequence Rolling found {1} files matching wildcard {2} in directory: {3}", _fileTarget, archiveFiles.Length, archiveWildCardFileName, archiveDirectory);
            if (archiveFiles.Length == 0)
                return 0;

            var sequenceNo = GetMaxArchiveSequenceNo(archiveFiles, fileWildcardStartIndex, fileWildcardEndIndex);
            if (sequenceNo.HasValue)
                return sequenceNo.Value + 1;

            return 0;
        }
    }
}
