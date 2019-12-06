// 
// Copyright (c) 2004-2019 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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

using System;
using System.Collections.Generic;
using System.IO;

namespace NLog.Targets.FileArchiveModes
{
    internal abstract class FileArchiveModeBase : IFileArchiveMode
    {
        static readonly DateTime MaxAgeArchiveFileDate = new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        private int _lastArchiveFileCount = short.MaxValue * 2;
        private DateTime _oldestArchiveFileDate = MaxAgeArchiveFileDate;

        public bool IsArchiveCleanupEnabled { get; }

        protected FileArchiveModeBase(bool isArchiveCleanupEnabled)
        {
            IsArchiveCleanupEnabled = isArchiveCleanupEnabled;
        }

        /// <summary>
        /// Check if cleanup should be performed on initialize new file
        /// 
        /// Skip cleanup when initializing new file, just after having performed archive operation
        /// </summary>
        /// <param name="archiveFilePath">Base archive file pattern</param>
        /// <param name="maxArchiveFiles">Maximum number of archive files that should be kept</param>
        /// <param name="maxArchiveDays">Maximum days of archive files that should be kept</param>
        /// <returns>True, when archive cleanup is needed</returns>
        public virtual bool AttemptCleanupOnInitializeFile(string archiveFilePath, int maxArchiveFiles, int maxArchiveDays)
        {
            if (maxArchiveFiles > 0 && _lastArchiveFileCount++ > maxArchiveFiles)
                return true;

            if (maxArchiveDays > 0 && (NLog.Time.TimeSource.Current.Time.Date.ToUniversalTime() - _oldestArchiveFileDate.Date).TotalDays > maxArchiveDays)
                return true;

            return false;
        }

        public string GenerateFileNameMask(string archiveFilePath)
        {
            int lastArchiveFileCount = _lastArchiveFileCount;
            DateTime oldestArchiveFileDate = _oldestArchiveFileDate;
            var fileMask = GenerateFileNameMask(archiveFilePath, GenerateFileNameTemplate(archiveFilePath));
            _lastArchiveFileCount = lastArchiveFileCount;   // Restore if modified by "mistake"
            _oldestArchiveFileDate = oldestArchiveFileDate;   // Restore if modified by "mistake"
            return fileMask;
        }

        public virtual List<DateAndSequenceArchive> GetExistingArchiveFiles(string archiveFilePath)
        {
            _lastArchiveFileCount = short.MaxValue * 2;
            _oldestArchiveFileDate = MaxAgeArchiveFileDate;

            string archiveFolderPath = Path.GetDirectoryName(archiveFilePath);
            FileNameTemplate archiveFileNameTemplate = GenerateFileNameTemplate(archiveFilePath);
            string archiveFileMask = GenerateFileNameMask(archiveFilePath, archiveFileNameTemplate);

            var existingArchiveFiles = new List<DateAndSequenceArchive>();
            if (string.IsNullOrEmpty(archiveFileMask))
                return existingArchiveFiles;

            DirectoryInfo directoryInfo = new DirectoryInfo(archiveFolderPath);
            if (!directoryInfo.Exists)
                return existingArchiveFiles;

#if SILVERLIGHT && !WINDOWS_PHONE
            var existingFiles = directoryInfo.EnumerateFiles(archiveFileMask);
#else
            var existingFiles = directoryInfo.GetFiles(archiveFileMask);
#endif
            foreach (var fileInfo in existingFiles)
            {
                var archiveFileInfo = GenerateArchiveFileInfo(fileInfo, archiveFileNameTemplate);
                if (archiveFileInfo != null)
                    existingArchiveFiles.Add(archiveFileInfo);
            }

            if (existingArchiveFiles.Count > 1)
                existingArchiveFiles.Sort(FileSortOrderComparison);

            UpdateMaxArchiveState(existingArchiveFiles);
            return existingArchiveFiles;
        }

        protected void UpdateMaxArchiveState(List<DateAndSequenceArchive> existingArchiveFiles)
        {
            _lastArchiveFileCount = existingArchiveFiles.Count;
            _oldestArchiveFileDate = existingArchiveFiles.Count == 0 ? DateTime.UtcNow : existingArchiveFiles[0].Date.Date.ToUniversalTime();
        }

        private static int FileSortOrderComparison(DateAndSequenceArchive x, DateAndSequenceArchive y)
        {
            if (x.Date != y.Date && !x.HasSameFormattedDate(y.Date))
                return x.Date.CompareTo(y.Date);

            if (x.Sequence.CompareTo(y.Sequence) != 0)
                return x.Sequence.CompareTo(y.Sequence);

            return string.CompareOrdinal(x.FileName, y.FileName);
        }

        protected virtual FileNameTemplate GenerateFileNameTemplate(string archiveFilePath)
        {
            ++_lastArchiveFileCount;
            return new FileNameTemplate(Path.GetFileName(archiveFilePath));
        }

        protected virtual string GenerateFileNameMask(string archiveFilePath, FileNameTemplate fileTemplate)
        {
            if (fileTemplate != null)
                return fileTemplate.ReplacePattern("*");
            else
                return string.Empty;
        }

        protected abstract DateAndSequenceArchive GenerateArchiveFileInfo(FileInfo archiveFile, FileNameTemplate fileTemplate);
        public abstract DateAndSequenceArchive GenerateArchiveFileName(string archiveFilePath, DateTime archiveDate, List<DateAndSequenceArchive> existingArchiveFiles);

        public virtual IEnumerable<DateAndSequenceArchive> CheckArchiveCleanup(string archiveFilePath, List<DateAndSequenceArchive> existingArchiveFiles, int maxArchiveFiles, int maxArchiveDays)
        {
            if (maxArchiveFiles <= 0 && maxArchiveDays <= 0)
                yield break;

            UpdateMaxArchiveState(existingArchiveFiles);

            if (existingArchiveFiles.Count == 0)
                yield break;

            if (maxArchiveFiles > 0 && existingArchiveFiles.Count <= maxArchiveFiles && maxArchiveDays <= 0)
                yield break;

            for (int i = 0; i < existingArchiveFiles.Count; i++)
            {
                if (ShouldDeleteFile(existingArchiveFiles[i], existingArchiveFiles.Count - i, maxArchiveFiles, maxArchiveDays))
                {
                    if (_lastArchiveFileCount > 0)
                        --_lastArchiveFileCount;
                    yield return existingArchiveFiles[i];
                }
                else
                {
                    _oldestArchiveFileDate = existingArchiveFiles[i].Date.Date.ToUniversalTime();
                    break;
                }                
            }
        }

        private bool ShouldDeleteFile(DateAndSequenceArchive existingArchiveFile, int remainingFileCount, int maxArchiveFiles, int maxArchiveDays)
        {
            if (maxArchiveFiles > 0 && remainingFileCount > maxArchiveFiles)
                return true;

            if (maxArchiveDays > 0)
            {
                var fileDateUtc = existingArchiveFile.Date.Date.ToUniversalTime();
                if (fileDateUtc > MaxAgeArchiveFileDate && (NLog.Time.TimeSource.Current.Time.Date.ToUniversalTime() - fileDateUtc).TotalDays > maxArchiveDays)
                    return true;
            }
                
            return false;
        }

        internal sealed class FileNameTemplate
        {
            /// <summary>
            /// Characters determining the start of the <see cref="P:FileNameTemplate.Pattern"/>.
            /// </summary>
            public const string PatternStartCharacters = "{#";

            /// <summary>
            /// Characters determining the end of the <see cref="P:FileNameTemplate.Pattern"/>.
            /// </summary>
            public const string PatternEndCharacters = "#}";

            /// <summary>
            /// File name which is used as template for matching and replacements. 
            /// It is expected to contain a pattern to match.
            /// </summary>
            public string Template { get; private set; }

            /// <summary>
            /// The begging position of the <see cref="P:FileNameTemplate.Pattern"/> 
            /// within the <see cref="P:FileNameTemplate.Template"/>. -1 is returned 
            /// when no pattern can be found.
            /// </summary>
            public int BeginAt { get; private set; }

            /// <summary>
            /// The ending position of the <see cref="P:FileNameTemplate.Pattern"/> 
            /// within the <see cref="P:FileNameTemplate.Template"/>. -1 is returned 
            /// when no pattern can be found.
            /// </summary>
            public int EndAt { get; private set; }

            private bool FoundPattern => BeginAt != -1 && EndAt != -1;

            public FileNameTemplate(string template)
            {
                Template = template;
                BeginAt = template.IndexOf(PatternStartCharacters, StringComparison.Ordinal);
                if (BeginAt != -1)
                    EndAt = template.IndexOf(PatternEndCharacters, StringComparison.Ordinal) + PatternEndCharacters.Length;
            }

            /// <summary>
            /// Replace the pattern with the specified String.
            /// </summary>
            /// <param name="replacementValue"></param>
            /// <returns></returns>
            public string ReplacePattern(string replacementValue)
            {
                return !FoundPattern || string.IsNullOrEmpty(replacementValue) ? Template : Template.Substring(0, BeginAt) + replacementValue + Template.Substring(EndAt);
            }
        }
    }
}
