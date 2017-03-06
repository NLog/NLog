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

namespace NLog.Targets
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    abstract class FileArchiveModeBase : IFileArchiveMode
    {
        private string _lastArchiveFilePath;

        /// <summary>
        /// Check if cleanup should be performed on initialize new file
        /// 
        /// Skip cleanup when initializing new file, just after having performed archive operation
        /// </summary>
        /// <param name="archiveFilePath">Base archive file pattern</param>
        /// <returns>True, when archive cleanup is needed</returns>
        public virtual bool AttemptCleanupOnInitializeFile(string archiveFilePath)
        {
            string lastArchiveFilePath = _lastArchiveFilePath;
            _lastArchiveFilePath = string.Empty;
            return lastArchiveFilePath != archiveFilePath;  // Skip archive cleanup, when it has just been executed
        }

        public string GenerateFileNameMask(string archiveFilePath)
        {
            return GenerateFileNameMask(archiveFilePath, GenerateFileNameTemplate(archiveFilePath));
        }

        public virtual List<DateAndSequenceArchive> GetExistingArchiveFiles(string archiveFilePath)
        {
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
            return existingArchiveFiles;
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

        public virtual IEnumerable<DateAndSequenceArchive> CheckArchiveCleanup(string archiveFilePath, List<DateAndSequenceArchive> existingArchiveFiles, int maxArchiveFiles)
        {
            if (maxArchiveFiles <= 0)
                yield break;

            _lastArchiveFilePath = archiveFilePath; // Cache that we have just performed cleanup for this archive-path

            if (existingArchiveFiles.Count == 0 || existingArchiveFiles.Count < maxArchiveFiles)
                yield break;

            for (int i = 0; i < existingArchiveFiles.Count - maxArchiveFiles; i++)
                yield return existingArchiveFiles[i];
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

            private bool FoundPattern { get { return BeginAt != -1 && EndAt != -1; } }

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
                return !FoundPattern || String.IsNullOrEmpty(replacementValue) ? this.Template : Template.Substring(0, this.BeginAt) + replacementValue + Template.Substring(this.EndAt);
            }
        }
    }
}
