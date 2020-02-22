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

namespace NLog.Targets.FileArchiveModes
{
    /// <summary>
    /// Archives the log-files using a date style numbering. Archives will be stamped with the
    /// prior period (Year, Month, Day, Hour, Minute) datetime.
    /// 
    /// When the number of archive files exceed <see cref="P:MaxArchiveFiles"/> the obsolete archives are deleted.
    /// When the age of archive files exceed <see cref="P:MaxArchiveDays"/> the obsolete archives are deleted.
    /// </summary>
    internal sealed class FileArchiveModeDate : FileArchiveModeBase
    {
        private readonly string _archiveDateFormat;

        public FileArchiveModeDate(string archiveDateFormat, bool isArchiveCleanupEnabled)
            :base(isArchiveCleanupEnabled)
        {
            _archiveDateFormat = archiveDateFormat;
        }

        public override List<DateAndSequenceArchive> GetExistingArchiveFiles(string archiveFilePath)
        {
            if (IsArchiveCleanupEnabled)
                return base.GetExistingArchiveFiles(archiveFilePath);
            else
                return new List<DateAndSequenceArchive>();
        }

        protected override DateAndSequenceArchive GenerateArchiveFileInfo(FileInfo archiveFile, FileNameTemplate fileTemplate)
        {
            string archiveFileName = Path.GetFileName(archiveFile.FullName) ?? "";
            string fileNameMask = fileTemplate.ReplacePattern("*");
            int lastIndexOfStar = fileNameMask.LastIndexOf('*');

            if (lastIndexOfStar + _archiveDateFormat.Length <= archiveFileName.Length)
            {
                string datePart = archiveFileName.Substring(lastIndexOfStar, _archiveDateFormat.Length);
                DateTime fileDate;
                if (DateTime.TryParseExact(datePart, _archiveDateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out fileDate))
                {
                    fileDate = DateTime.SpecifyKind(fileDate, NLog.Time.TimeSource.Current.Time.Kind);
                    return new DateAndSequenceArchive(archiveFile.FullName, fileDate, _archiveDateFormat, -1);
                }
            }

            return null;
        }

        public override DateAndSequenceArchive GenerateArchiveFileName(string archiveFilePath, DateTime archiveDate, List<DateAndSequenceArchive> existingArchiveFiles)
        {
            FileNameTemplate archiveFileNameTemplate = GenerateFileNameTemplate(archiveFilePath);
            string dirName = Path.GetDirectoryName(archiveFilePath);
            archiveFilePath = Path.Combine(dirName, archiveFileNameTemplate.ReplacePattern("*").Replace("*", archiveDate.ToString(_archiveDateFormat)));
            archiveFilePath = Path.GetFullPath(archiveFilePath);    // Rebuild to fix non-standard path-format
            return new DateAndSequenceArchive(archiveFilePath, archiveDate, _archiveDateFormat, 0);
        }
    }
}
