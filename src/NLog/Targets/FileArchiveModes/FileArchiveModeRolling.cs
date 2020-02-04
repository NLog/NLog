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

using System;
using System.Collections.Generic;
using System.IO;

namespace NLog.Targets.FileArchiveModes
{
    /// <summary>
    /// Archives the log-files using a rolling style numbering (the most recent is always #0 then
    /// #1, ..., #N. 
    /// 
    /// When the number of archive files exceed <see cref="P:MaxArchiveFiles"/> the obsolete archives
    /// are deleted.
    /// </summary>
    internal sealed class FileArchiveModeRolling : IFileArchiveMode
    {
        public bool IsArchiveCleanupEnabled => true; // Always to roll

        public bool AttemptCleanupOnInitializeFile(string archiveFilePath, int maxArchiveFiles, int maxArchiveDays)
        {
            return false;   // For historic reasons, then cleanup of rolling archives are not done on startup
        }

        public string GenerateFileNameMask(string archiveFilePath)
        {
            return new FileArchiveModeBase.FileNameTemplate(Path.GetFileName(archiveFilePath)).ReplacePattern("*");
        }

        public List<DateAndSequenceArchive> GetExistingArchiveFiles(string archiveFilePath)
        {
            List<DateAndSequenceArchive> existingArchiveFiles = new List<DateAndSequenceArchive>();
            for (int archiveNumber = 0; archiveNumber < int.MaxValue; archiveNumber++)
            {
                string existingFileName = ReplaceNumberPattern(archiveFilePath, archiveNumber);
                FileInfo existingFileInfo = new FileInfo(existingFileName);
                if (!existingFileInfo.Exists)
                    break;

                existingArchiveFiles.Add(new DateAndSequenceArchive(existingFileInfo.FullName, DateTime.MinValue, string.Empty, archiveNumber));
            }

            return existingArchiveFiles;
        }

        /// <summary>
        /// Replaces the numeric pattern i.e. {#} in a file name with the <paramref name="value"/> parameter value.
        /// </summary>
        /// <param name="pattern">File name which contains the numeric pattern.</param>
        /// <param name="value">Value which will replace the numeric pattern.</param>
        /// <returns>File name with the value of <paramref name="value"/> in the position of the numeric pattern.</returns>
        private static string ReplaceNumberPattern(string pattern, int value)
        {
            int firstPart = pattern.IndexOf("{#", StringComparison.Ordinal);
            int lastPart = pattern.IndexOf("#}", StringComparison.Ordinal) + 2;
            int numDigits = lastPart - firstPart - 2;

            return pattern.Substring(0, firstPart) + Convert.ToString(value, 10).PadLeft(numDigits, '0') +
                   pattern.Substring(lastPart);
        }

        public DateAndSequenceArchive GenerateArchiveFileName(string archiveFilePath, DateTime archiveDate, List<DateAndSequenceArchive> existingArchiveFiles)
        {
            if (existingArchiveFiles.Count > 0)
            {
                // We are about to perform roll, so we add an artificial file to cache the next rollFileName
                int rollSequenceNo = existingArchiveFiles[existingArchiveFiles.Count - 1].Sequence + 1;
                string rollFileName = ReplaceNumberPattern(archiveFilePath, rollSequenceNo);
                existingArchiveFiles.Add(new DateAndSequenceArchive(rollFileName, DateTime.MinValue, string.Empty, int.MaxValue));
            }
            string newFileName = ReplaceNumberPattern(archiveFilePath, 0);
            newFileName = Path.GetFullPath(newFileName);    // Rebuild to fix non-standard path-format
            return new DateAndSequenceArchive(newFileName, DateTime.MinValue, string.Empty, int.MinValue);
        }

        public IEnumerable<DateAndSequenceArchive> CheckArchiveCleanup(string archiveFilePath, List<DateAndSequenceArchive> existingArchiveFiles, int maxArchiveFiles, int maxArchiveDays)
        {
            if (existingArchiveFiles.Count <= 1)
                yield break;

            existingArchiveFiles.Sort((x, y) => x.Sequence.CompareTo(y.Sequence));

            if (maxArchiveFiles > 0 && existingArchiveFiles.Count > maxArchiveFiles)
            {
                for (int i = 0; i < existingArchiveFiles.Count; i++)
                {
                    if (existingArchiveFiles[i].Sequence == int.MinValue || existingArchiveFiles[i].Sequence == int.MaxValue)
                        continue;

                    if (i >= maxArchiveFiles)
                    {
                        yield return existingArchiveFiles[i];
                    }
                }
            }

            // After deleting the last/oldest, then roll the others forward
            if (existingArchiveFiles.Count > 1 && existingArchiveFiles[0].Sequence == int.MinValue)
            {
                string newFileName = string.Empty;
                int maxFileCount = existingArchiveFiles.Count - 1;
                if (maxArchiveFiles > 0 && maxFileCount > maxArchiveFiles)
                    maxFileCount = maxArchiveFiles;
                for (int i = maxFileCount; i >= 1; --i)
                {
                    string fileName = existingArchiveFiles[i].FileName;
                    if (!string.IsNullOrEmpty(newFileName))
                    {
                        Common.InternalLogger.Info("Roll archive {0} to {1}", fileName, newFileName);
                        File.Move(fileName, newFileName);
                    }
                    newFileName = fileName;
                }
            }
        }
    }
}
