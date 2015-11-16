// 
// Copyright (c) 2004-2011 Jaroslaw Kowalski <jaak@jkowalski.net>
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
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;

namespace NLog.Targets.FileTargetArchival
{
    /// <summary>
    /// Provides archive behavior with date-time filenames.
    /// </summary>
    class DateArchival
    {
        /// <summary>
        /// It holds the file names of existing archives in order for the oldest archives to be removed when the list of
        /// filenames becomes too long.
        /// </summary>
        private Queue<string> previousFileNames;

        /// <summary>
        /// Provides archival options and context.
        /// </summary>
        public Archival Archival { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="previousFileNamesCapacity">Capacity for <c>previousFileNames</c>.</param>
        public DateArchival(int previousFileNamesCapacity)
        {
            this.previousFileNames = new Queue<string>(previousFileNamesCapacity);
        }

#if !NET_CF
        /// <summary>
        /// Archives the <paramref name="fileName"/> using a date style numbering. Archives will be stamped with the
        /// prior period (Year, Month, Day, Hour, Minute) datetime. When the number of archive files exceed <see
        /// cref="P:MaxArchiveFiles"/> the obsolete archives are deleted.
        /// </summary>
        /// <param name="fileName">File name to be archived.</param>
        /// <param name="pattern">File name template which contains the numeric pattern to be replaced.</param>
        public void ArchiveByDate(string fileName, string pattern)
        {
            string fileNameMask = FileNameTemplate.ReplaceFileNamePattern(pattern, "*");
            string dirName = Path.GetDirectoryName(Path.GetFullPath(pattern));
            string dateFormat = DateArchivalCommon.GetDateFormatString(Archival.Options.ArchiveDateFormat, Archival.Options.ArchiveEvery);

            DateTime archiveDate = DateArchivalCommon.GetArchiveDate(true, Archival.Options.ArchiveEvery);
            if (dirName != null)
            {
                string archiveFileName = Path.Combine(dirName, fileNameMask.Replace("*", archiveDate.ToString(dateFormat)));
                Archival.RollArchiveForward(fileName, archiveFileName, allowCompress: true);
            }

            DeleteOldDateArchives(pattern);
        }

        /// <summary>
        /// Deletes archive files in reverse chronological order until only the
        /// MaxArchiveFiles number of archive files remain.
        /// </summary>
        /// <param name="pattern">The pattern that archive filenames will match</param>
        public void DeleteOldDateArchives(string pattern)
        {
            string fileNameMask = FileNameTemplate.ReplaceFileNamePattern(pattern, "*");
            string dirName = Path.GetDirectoryName(Path.GetFullPath(pattern));
            string dateFormat = DateArchivalCommon.GetDateFormatString(Archival.Options.ArchiveDateFormat, Archival.Options.ArchiveEvery);

            if (dirName != null)
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(dirName);
                if (!directoryInfo.Exists)
                {
                    Directory.CreateDirectory(dirName);
                    return;
                }

#if SILVERLIGHT
                var files = directoryInfo.EnumerateFiles(fileNameMask).OrderBy(n => n.CreationTime).Select(n => n.FullName);
#else
                var files = directoryInfo.GetFiles(fileNameMask).OrderBy(n => n.CreationTime).Select(n => n.FullName);
#endif
                List<string> filesByDate = new List<string>();

                foreach (string nextFile in files)
                {
                    string archiveFileName = Path.GetFileName(nextFile);
                    string datePart = archiveFileName.Substring(fileNameMask.LastIndexOf('*'), dateFormat.Length);
                    DateTime fileDate = DateTime.MinValue;
                    if (DateTime.TryParseExact(datePart, dateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out fileDate))
                    {
                        filesByDate.Add(nextFile);
                    }
                }

                DateArchivalCommon.EnsureArchiveCount(filesByDate, Archival.Options.MaxArchiveFiles);
            }
        }
#endif // !NET_CF

        /// <summary>
        /// Deletes old date archives if necessary before writing to the given log file.
        /// </summary>
        /// <param name="fileName">The log filename about to be written-to.</param>
        /// <param name="logEvent">The logging event.</param>
        public void CheckToDeleteOldDateArchives(string fileName, LogEventInfo logEvent)
        {
            // Clean up old archives if this is the first time a log record is being written to
            // this log file and the archiving system is date/time based.
            if (Archival.Options.ArchiveNumbering == ArchiveNumberingMode.Date && Archival.Options.ArchiveEvery != FileArchivePeriod.None)
            {
                if (!previousFileNames.Contains(fileName))
                {
                    if (previousFileNames.Count > Archival.Options.MaxLogFilenames)
                    {
                        previousFileNames.Dequeue();
                    }

                    string fileNamePattern = Archival.GetFileNamePattern(fileName, logEvent);
                    DeleteOldDateArchives(fileNamePattern);
                    previousFileNames.Enqueue(fileName);
                }
            }
        }
    }
}
