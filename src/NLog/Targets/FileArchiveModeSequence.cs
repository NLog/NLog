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
    using System.Globalization;
    using System.IO;

    /// <summary>
    /// Archives the log-files using a sequence style numbering. The most recent archive has the
    /// highest number. When the number of archive files exceed <see cref="P:MaxArchiveFiles"/> the obsolete
    /// archives are deleted.
    /// </summary>
    sealed class FileArchiveModeSequence : FileArchiveModeBase
    {
        private readonly string _archiveDateFormat;

        public FileArchiveModeSequence(string archiveDateFormat)
        {
            _archiveDateFormat = archiveDateFormat;
        }

        public override bool AttemptCleanupOnInitializeFile(string archiveFilePath)
        {
            return false;   // For historic reasons, then cleanup of sequence archives are not done on startup
        }

        protected override DateAndSequenceArchive GenerateArchiveFileInfo(FileInfo archiveFile, FileNameTemplate fileTemplate)
        {
            string baseName = Path.GetFileName(archiveFile.FullName) ?? "";
            int trailerLength = fileTemplate.Template.Length - fileTemplate.EndAt;
            string number = baseName.Substring(fileTemplate.BeginAt, baseName.Length - trailerLength - fileTemplate.BeginAt);
            int num;

            try
            {
                num = Convert.ToInt32(number, CultureInfo.InvariantCulture);
            }
            catch (FormatException)
            {
                return null;
            }

            return new DateAndSequenceArchive(archiveFile.FullName, DateTime.MinValue, string.Empty, num);
        }

        public override DateAndSequenceArchive GenerateArchiveFileName(string archiveFilePath, DateTime archiveDate, List<DateAndSequenceArchive> existingArchiveFiles)
        {
            int nextSequenceNumber = 0;

            FileNameTemplate archiveFileNameTemplate = GenerateFileNameTemplate(archiveFilePath);
            foreach (var existingFile in existingArchiveFiles)
                nextSequenceNumber = Math.Max(nextSequenceNumber, existingFile.Sequence + 1);

            int minSequenceLength = archiveFileNameTemplate.EndAt - archiveFileNameTemplate.BeginAt - 2;
            string paddedSequence = nextSequenceNumber.ToString().PadLeft(minSequenceLength, '0');
            string dirName = Path.GetDirectoryName(archiveFilePath);
            archiveFilePath = Path.Combine(dirName, archiveFileNameTemplate.ReplacePattern("*").Replace("*", paddedSequence));
            return new DateAndSequenceArchive(archiveFilePath, archiveDate, _archiveDateFormat, nextSequenceNumber);
        }

        public override IEnumerable<DateAndSequenceArchive> CheckArchiveCleanup(string archiveFilePath, List<DateAndSequenceArchive> existingArchiveFiles, int maxArchiveFiles)
        {
            if (maxArchiveFiles <= 0 || existingArchiveFiles.Count == 0 || existingArchiveFiles.Count < maxArchiveFiles)
                yield break;

            int nextSequenceNumber = existingArchiveFiles[existingArchiveFiles.Count - 1].Sequence;
            int minNumberToKeep = nextSequenceNumber - maxArchiveFiles + 1;
            if (minNumberToKeep <= 0)
                yield break;
            foreach (var existingFile in existingArchiveFiles)
                if (existingFile.Sequence < minNumberToKeep)
                    yield return existingFile;
        }
    }
}
