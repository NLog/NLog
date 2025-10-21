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
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;
    using NLog.Common;
    using NLog.Internal;

    internal class BaseFileArchiveHandler
    {
        protected readonly FileTarget _fileTarget;

        public BaseFileArchiveHandler(FileTarget fileTarget)
        {
            _fileTarget = fileTarget;
        }

        protected bool DeleteOldFilesBeforeArchive(string filePath, bool initialFileOpen, bool parseArchiveSequenceNo, string? excludeFileName = null)
        {
            // Get all files matching the filename, order by timestamp, and when same timestamp then order by filename
            //  - First start with removing the oldest files
            string fileDirectory = Path.GetDirectoryName(filePath);
            // Replace all non-letter with '*' replace all '**' with single '*'
            string fileWildcard = GetDeleteOldFileNameWildcard(filePath);
            return DeleteOldFilesBeforeArchive(fileDirectory, fileWildcard, initialFileOpen, parseArchiveSequenceNo, excludeFileName);
        }

        protected bool DeleteOldFilesBeforeArchive(string fileDirectory, string fileWildcard, bool initialFileOpen, bool parseArchiveSequenceNo, string? excludeFileName = null)
        {
            try
            {
                if (string.IsNullOrEmpty(fileWildcard))
                    return false;

                if (string.IsNullOrEmpty(fileDirectory))
                    return false;

                var directoryInfo = new DirectoryInfo(fileDirectory);
                if (!directoryInfo.Exists)
                {
                    InternalLogger.Debug("{0}: Archive cleanup found no files matching wildcard '{1}' in directory: {2}", _fileTarget, fileWildcard, fileDirectory);
                    return false;
                }

                var fileInfos = directoryInfo.GetFiles(fileWildcard);
                InternalLogger.Debug("{0}: Archive cleanup found {1} files matching wildcard '{2}' in directory: {3}", _fileTarget, fileInfos.Length, fileWildcard, fileDirectory);
                if (fileInfos.Length == 0)
                    return false;

                int maxArchiveFiles = ResolveMaxArchiveFiles(initialFileOpen);
                if (maxArchiveFiles >= fileInfos.Length && _fileTarget.MaxArchiveDays <= 0)
                    return false;

                var archiveCleanupReason = (_fileTarget.MaxArchiveFiles < 0 && _fileTarget.MaxArchiveDays > 0) ? $"MaxArchiveDays={_fileTarget.MaxArchiveDays}" : $"MaxArchiveFiles={_fileTarget.MaxArchiveFiles}";
                if (initialFileOpen && _fileTarget.DeleteOldFileOnStartup)
                    archiveCleanupReason = "DeleteOldFileOnStartup=true";

                int fileWildcardStartIndex = fileWildcard.IndexOf('*');
                int fileWildcardEndIndex = (fileWildcardStartIndex >= 0 && fileWildcardStartIndex == fileWildcard.LastIndexOf('*')) ? fileWildcard.Length - fileWildcardStartIndex : -1;

                bool oldFilesDeleted = false;
                foreach (var cleanupFileInfo in FileInfoDateTime.CleanupFiles(fileInfos, maxArchiveFiles, _fileTarget.MaxArchiveDays, fileWildcardStartIndex, fileWildcardEndIndex, parseArchiveSequenceNo, excludeFileName))
                {
                    oldFilesDeleted = true;
                    DeleteOldArchiveFile(cleanupFileInfo.FullName, archiveCleanupReason);
                }

                return oldFilesDeleted;
            }
            catch (Exception exception)
            {
                InternalLogger.Warn(exception, "{0}: Failed to cleanup archive folder: {1}  {2}", _fileTarget, fileDirectory, fileWildcard ?? "");
                if (exception.MustBeRethrown(_fileTarget))
                    throw;
            }

            return false;
        }

        private int ResolveMaxArchiveFiles(bool initialFileOpen)
        {
            var maxArchiveFiles = _fileTarget.MaxArchiveFiles;
            if (initialFileOpen && (!_fileTarget.ArchiveOldFileOnStartup || _fileTarget.DeleteOldFileOnStartup))
                maxArchiveFiles = _fileTarget.DeleteOldFileOnStartup ? 0 : maxArchiveFiles;
            else if (maxArchiveFiles > 0)
                maxArchiveFiles -= 1;
            return maxArchiveFiles;
        }

        protected static int? GetMaxArchiveSequenceNo(FileInfo[] fileInfos, int fileWildcardStartIndex, int fileWildcardEndIndex)
        {
            return FileInfoDateTime.ScanFileNamesForMaxSequenceNo(fileInfos, fileWildcardStartIndex, fileWildcardEndIndex);
        }

        private
#if !NETFRAMEWORK
        readonly
#endif
        struct FileInfoDateTime : IComparer<FileInfoDateTime>
        {
            public FileInfo FileInfo { get; }
            public DateTime FileCreatedTimeUtc { get; }
            public int? ArchiveSequenceNumber { get; }

            public FileInfoDateTime(FileInfo fileInfo, DateTime fileCreatedTimeUtc, int? archiveSequenceNumber = null)
            {
                FileInfo = fileInfo;
                ArchiveSequenceNumber = archiveSequenceNumber;
                FileCreatedTimeUtc = fileCreatedTimeUtc;
            }

            public int Compare(FileInfoDateTime x, FileInfoDateTime y)
            {
                if (x.ArchiveSequenceNumber.HasValue && y.ArchiveSequenceNumber.HasValue)
                {
                    return x.ArchiveSequenceNumber.Value.CompareTo(y.ArchiveSequenceNumber.Value);
                }
                else if (x.FileCreatedTimeUtc.Date == y.FileCreatedTimeUtc.Date)
                {
                    return StringComparer.OrdinalIgnoreCase.Compare(x.FileInfo.Name, y.FileInfo.Name);
                }
                else
                {
                    return x.FileCreatedTimeUtc.CompareTo(y.FileCreatedTimeUtc);
                }
            }

            public override string ToString()
            {
                return FileInfo.Name;
            }

            public static int? ScanFileNamesForMaxSequenceNo(FileInfo[] fileInfos, int fileWildcardStartIndex, int fileWildcardEndIndex)
            {
                int? maxArchiveSequenceNo = null;
                if (fileWildcardStartIndex < 0)
                    return default(int?);

                foreach (var fileInfo in fileInfos)
                {
                    var archiveFileName = fileInfo.Name;

                    if (ExcludeFileName(archiveFileName, fileWildcardStartIndex, fileWildcardEndIndex, true, null))
                        continue;

                    if (TryParseStartSequenceNumber(archiveFileName, fileWildcardStartIndex, fileWildcardEndIndex, out var archiveSequenceNo))
                    {
                        if (!maxArchiveSequenceNo.HasValue || archiveSequenceNo > maxArchiveSequenceNo.Value)
                            maxArchiveSequenceNo = archiveSequenceNo;
                    }
                }

                return maxArchiveSequenceNo;
            }

            /// <summary>
            /// - Only strict scan for sequence-number (GetTodaysArchiveFiles) when having input "fileLastWriteTime"
            ///     - Expect optional DateTime-part to be "sortable" (when missing birthtime)
            ///         - Trim away sequencer-number, so not part of sorting
            ///     - Use DateTime part from FileSystem for ordering by Date-only, and sort by FileName
            /// </summary>
            public static IEnumerable<FileInfo> CleanupFiles(FileInfo[] fileInfos, int maxArchiveFiles, int maxArchiveDays, int fileWildcardStartIndex, int fileWildcardEndIndex, bool parseArchiveSequenceNo, string? excludeFileName = null)
            {
                if (fileInfos.Length == 1)
                {
                    var archiveFileName = fileInfos[0].Name;
                    if (ExcludeFileName(archiveFileName, fileWildcardStartIndex, fileWildcardEndIndex, parseArchiveSequenceNo, excludeFileName))
                    {
                        yield break;
                    }
                    else if (maxArchiveFiles == 0)
                    {
                        yield return fileInfos[0];
                        yield break;
                    }
                }

                var fileInfoDates = new List<FileInfoDateTime>(fileInfos.Length);
                foreach (var fileInfo in fileInfos)
                {
                    var archiveFileName = fileInfo.Name;
                    if (ExcludeFileName(archiveFileName, fileWildcardStartIndex, fileWildcardEndIndex, parseArchiveSequenceNo, excludeFileName))
                        continue;

                    var fileCreatedTimeUtc = FileInfoHelper.LookupValidFileCreationTimeUtc(fileInfo);
                    if (parseArchiveSequenceNo && TryParseStartSequenceNumber(archiveFileName, fileWildcardStartIndex, fileWildcardEndIndex, out var archiveSequenceNo))
                        fileInfoDates.Add(new FileInfoDateTime(fileInfo, fileCreatedTimeUtc, archiveSequenceNo));
                    else
                        fileInfoDates.Add(new FileInfoDateTime(fileInfo, fileCreatedTimeUtc));
                }
                fileInfoDates.Sort((x, y) => x.Compare(x, y));

                for (int i = 0; i < fileInfoDates.Count; i++)
                {
                    if (ShouldDeleteFile(fileInfoDates[i], fileInfoDates.Count - i, maxArchiveFiles, maxArchiveDays))
                    {
                        yield return fileInfoDates[i].FileInfo;
                    }
                    else
                    {
                        yield break;
                    }
                }
            }

            private static bool ExcludeFileName(string archiveFileName, int fileWildcardStartIndex, int fileWildcardEndIndex, bool parseArchiveSequenceNo, string? excludeFileName)
            {
                if (fileWildcardStartIndex >= 0 && fileWildcardEndIndex > 0 && parseArchiveSequenceNo)
                {
                    for (int i = fileWildcardStartIndex; i <= archiveFileName.Length - fileWildcardEndIndex; ++i)
                    {
                        if (char.IsLetter(archiveFileName[i]))
                            return true;
                    }
                }

                if (excludeFileName is null)
                    return false;
                else
                    return string.Equals(archiveFileName, excludeFileName, StringComparison.OrdinalIgnoreCase);
            }

            private static bool ShouldDeleteFile(FileInfoDateTime existingArchiveFile, int remainingFileCount, int maxArchiveFiles, int maxArchiveDays)
            {
                if (maxArchiveFiles >= 0 && remainingFileCount > maxArchiveFiles)
                    return true;

                if (maxArchiveDays > 0)
                {
                    var currentDate = NLog.Time.TimeSource.Current.Time.Date;
                    var fileCreatedDate = NLog.Time.TimeSource.Current.FromSystemTime(existingArchiveFile.FileCreatedTimeUtc);
                    var fileAgeDays = (currentDate - fileCreatedDate.Date).TotalDays;
                    if (fileAgeDays > maxArchiveDays)
                    {
                        InternalLogger.Debug("FileTarget: Archive cleanup found old file: {0} , Age={1} days, FileCreated={2:u}, CurrentDate={3:u}", existingArchiveFile.FileInfo.FullName, Math.Round(fileAgeDays, 1), fileCreatedDate, currentDate);
                        return true;
                    }
                }

                return false;
            }

            private static bool TryParseStartSequenceNumber(string archiveFileName, int fileWildcardStartIndex, int fileWildcardEndIndex, out int archiveSequenceNo)
            {
                int? parsedSequenceNo = null;
                bool foundSequenceNo = false;
                if (fileWildcardStartIndex < 0 || fileWildcardEndIndex <= 0)
                {
                    archiveSequenceNo = 0;
                    return false;
                }

                for (int i = fileWildcardStartIndex; i <= archiveFileName.Length - fileWildcardEndIndex; ++i)
                {
                    char chr = archiveFileName[i];
                    if (!char.IsDigit(chr))
                    {
                        foundSequenceNo = parsedSequenceNo.HasValue;
                        continue;
                    }
                    else if (foundSequenceNo)
                    {
                        archiveSequenceNo = 0;
                        return false;   // Abort bonus sequence-number found, so cannot trust anything
                    }

                    parsedSequenceNo = parsedSequenceNo > 0 ? parsedSequenceNo * 10 : 0;
                    parsedSequenceNo += (chr - '0');
                }
                archiveSequenceNo = parsedSequenceNo ?? 0;
                return parsedSequenceNo.HasValue;
            }
        }

        private static string GetDeleteOldFileNameWildcard(string filepath)
        {
            var filename = Path.GetFileNameWithoutExtension(filepath) ?? string.Empty;
            var fileext = Path.GetExtension(filepath) ?? string.Empty;
            if (string.IsNullOrEmpty(filename) && string.IsNullOrEmpty(fileext))
                return string.Empty;

            int lastStart = 0;
            int lastLength = 0;
            int currentLength = 0;
            int currentStart = 0;
            bool hasDigit = false;
            for (int i = 0; i < filename.Length; ++i)
            {
                if (!char.IsLetter(filename[i]))
                {
                    hasDigit = hasDigit || char.IsDigit(filename[i]);
                    if (hasDigit)
                    {
                        if (currentLength == 0)
                            currentStart = i;
                        ++currentLength;
                    }
                }
                else
                {
                    if (currentLength != 0)
                    {
                        if (lastLength <= currentLength)
                        {
                            lastStart = currentStart;
                            lastLength = currentLength;
                        }
                        currentLength = 0;
                    }
                    hasDigit = false;
                }
            }

            if (lastLength < currentLength)
            {
                lastStart = currentStart;
                lastLength = currentLength;
            }

            if (lastLength > 0)
            {
                var prefix = filename.Substring(0, lastStart);
                var suffix = filename.Substring(lastStart + lastLength);
                return string.IsNullOrEmpty(suffix) ? string.Concat(prefix, "*", fileext) : string.Concat(prefix, "*", suffix, "*", fileext);
            }

            return string.Concat(filename, "*", fileext);
        }

        protected bool DeleteOldArchiveFile(string filepath, string archiveCleanupReason)
        {
            for (int i = 1; i <= 3; ++i)
            {
                try
                {
                    InternalLogger.Info("{0}: Cleanup file archive {1}. Delete file: '{2}'", _fileTarget, archiveCleanupReason, filepath);
                    _fileTarget.CloseOpenFileBeforeArchiveCleanup(filepath);
                    File.Delete(filepath);
                    return true;
                }
                catch (DirectoryNotFoundException ex)
                {
                    InternalLogger.Debug(ex, "{0}: Failed to delete old file as directory not found: '{1}'", _fileTarget, filepath);
                    return true;
                }
                catch (FileNotFoundException ex)
                {
                    InternalLogger.Debug(ex, "{0}: Failed to delete old file as file not found: '{1}'", _fileTarget, filepath);
                    return true;
                }
                catch (IOException ex)
                {
                    InternalLogger.Debug(ex, "{0}: Failed to delete old file, maybe file is locked: '{1}'", _fileTarget, filepath);
                    if (!File.Exists(filepath))
                        return true;
                    if (i >= 3 && ex.MustBeRethrown(_fileTarget))
                        throw;
                }
                catch (Exception ex)
                {
                    InternalLogger.Warn(ex, "{0}: Failed to delete old archive file: '{1}'", _fileTarget, filepath);
                    if (ex.MustBeRethrown(_fileTarget))
                        throw;

                    return false;
                }

                Thread.Sleep(i * 10);
            }
            return false;
        }

        protected void FixWindowsFileSystemTunneling(string newFilePath)
        {
            try
            {
                if (PlatformDetector.IsWin32 && !File.Exists(newFilePath))
                {
                    // Set the file's creation time to avoid being thwarted by Windows FileSystem Tunneling capabilities (https://support.microsoft.com/en-us/kb/172190).
                    File.Create(newFilePath).Dispose();
                    File.SetCreationTimeUtc(newFilePath, DateTime.UtcNow);
                }
            }
            catch (Exception ex)
            {
                InternalLogger.Debug(ex, "{0}: Failed to refresh CreationTimeUtc for file: {1}", _fileTarget, newFilePath);
            }
        }
    }
}
