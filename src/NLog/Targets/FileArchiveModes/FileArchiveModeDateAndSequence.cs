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
using System.Globalization;
using System.IO;
using NLog.Common;

namespace NLog.Targets.FileArchiveModes
{
    /// <summary>
    /// Archives the log-files using a date and sequence style numbering. Archives will be stamped
    /// with the prior period (Year, Month, Day) datetime. The most recent archive has the highest number (in
    /// combination with the date).
    /// 
    /// When the number of archive files exceed <see cref="P:MaxArchiveFiles"/> the obsolete archives are deleted.
    /// When the age of archive files exceed <see cref="P:MaxArchiveDays"/> the obsolete archives are deleted.
    /// </summary>
    internal sealed class FileArchiveModeDateAndSequence : FileArchiveModeBase
    {
        private readonly string _archiveDateFormat;

        public FileArchiveModeDateAndSequence(string archiveDateFormat, bool archiveCleanupEnabled)
            :base(archiveCleanupEnabled)
        {
            _archiveDateFormat = archiveDateFormat;
        }

        public override bool AttemptCleanupOnInitializeFile(string archiveFilePath, int maxArchiveFiles, int maxArchiveDays)
        {
            return false;   // For historic reasons, then cleanup of sequence archives are not done on startup
        }

        protected override DateAndSequenceArchive GenerateArchiveFileInfo(FileInfo archiveFile, FileNameTemplate fileTemplate)
        {
            //Get the archive file name or empty string if it's null
            string archiveFileNameWithoutPath = Path.GetFileName(archiveFile.FullName) ?? string.Empty;

            if (!TryParseDateAndSequence(archiveFileNameWithoutPath, _archiveDateFormat, fileTemplate, out var date, out var sequence))
            {
                return null;
            }

            date = DateTime.SpecifyKind(date, NLog.Time.TimeSource.Current.Time.Kind);
            return new DateAndSequenceArchive(archiveFile.FullName, date, _archiveDateFormat, sequence);
        }

        public override DateAndSequenceArchive GenerateArchiveFileName(string archiveFilePath, DateTime archiveDate, List<DateAndSequenceArchive> existingArchiveFiles)
        {
            int nextSequenceNumber = 0;
            FileNameTemplate archiveFileNameTemplate = GenerateFileNameTemplate(archiveFilePath);

            foreach (var existingFile in existingArchiveFiles)
                if (existingFile.HasSameFormattedDate(archiveDate))
                    nextSequenceNumber = Math.Max(nextSequenceNumber, existingFile.Sequence + 1);

            int minSequenceLength = archiveFileNameTemplate.EndAt - archiveFileNameTemplate.BeginAt - 2;
            string paddedSequence = nextSequenceNumber.ToString().PadLeft(minSequenceLength, '0');
            string archiveFileNameWithoutPath = archiveFileNameTemplate.ReplacePattern("*").Replace("*",
                $"{archiveDate.ToString(_archiveDateFormat)}.{paddedSequence}");
            string dirName = Path.GetDirectoryName(archiveFilePath);
            archiveFilePath = Path.Combine(dirName, archiveFileNameWithoutPath);
            archiveFilePath = Path.GetFullPath(archiveFilePath);    // Rebuild to fix non-standard path-format
            return new DateAndSequenceArchive(archiveFilePath, archiveDate, _archiveDateFormat, nextSequenceNumber);
        }

        /// <summary>
        /// Parse filename with date and sequence pattern
        /// </summary>
        /// <param name="archiveFileNameWithoutPath"></param>
        /// <param name="dateFormat">dateformat for archive</param>
        /// <param name="fileTemplate"></param>
        /// <param name="date">the found pattern. When failed, then default</param>
        /// <param name="sequence">the found pattern. When failed, then default</param>
        /// <returns></returns>
        private static bool TryParseDateAndSequence(string archiveFileNameWithoutPath, string dateFormat, FileNameTemplate fileTemplate, out DateTime date, out int sequence)
        {
            int trailerLength = fileTemplate.Template.Length - fileTemplate.EndAt;
            int dateAndSequenceIndex = fileTemplate.BeginAt;
            int dateAndSequenceLength = archiveFileNameWithoutPath.Length - trailerLength - dateAndSequenceIndex;

            if (dateAndSequenceLength < 0)
            {
                date = default(DateTime);
                sequence = 0;
                return false;
            }
            string dateAndSequence = archiveFileNameWithoutPath.Substring(dateAndSequenceIndex, dateAndSequenceLength);
            int sequenceIndex = dateAndSequence.LastIndexOf('.') + 1;

            string sequencePart = dateAndSequence.Substring(sequenceIndex);
            if (!int.TryParse(sequencePart, NumberStyles.None, CultureInfo.CurrentCulture, out sequence))
            {
                date = default(DateTime);
                return false;
            }

            var dateAndSequenceLength2 = dateAndSequence.Length - sequencePart.Length - 1;
            if (dateAndSequenceLength2 < 0)
            {
                date = default(DateTime);
                return false;
            }

            string datePart = dateAndSequence.Substring(0, dateAndSequenceLength2);
            if (!DateTime.TryParseExact(datePart, dateFormat, CultureInfo.CurrentCulture, DateTimeStyles.None,
                out date))
            {
                return false;
            }
            InternalLogger.Trace("FileTarget: parsed date '{0}' from file-template '{1}'", datePart, fileTemplate?.Template);
            return true;
        }
    }
}
