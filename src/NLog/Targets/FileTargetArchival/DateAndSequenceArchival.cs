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
    /// Provides archive behavior with date-time and numeric filenames.
    /// </summary>
    class DateAndSequenceArchival
    {
        /// <summary>
        /// Provides archival options and context.
        /// </summary>
        public Archival Archival { get; set; }

#if !NET_CF
        /// <summary>
        /// <para>
        /// Archives the <paramref name="fileName"/> using a date and sequence style numbering. Archives will be stamped
        /// with the prior period (Year, Month, Day) datetime. The most recent archive has the highest number (in
        /// combination with the date).
        /// </para>
        /// <para>
        /// When the number of archive files exceed <see cref="P:MaxArchiveFiles"/> the obsolete archives are deleted.
        /// </para>
        /// </summary>
        /// <param name="fileName">File name to be archived.</param>
        /// <param name="pattern">File name template which contains the numeric pattern to be replaced.</param>
        /// <param name="logEvent">Log event that the <see cref="FileTarget"/> instance is currently processing.</param>
        public void ArchiveByDateAndSequence(string fileName, string pattern, LogEventInfo logEvent)
        {
            string baseNamePattern = Path.GetFileName(pattern);

            if (string.IsNullOrEmpty(baseNamePattern))
            {
                return;
            }

            FileNameTemplate fileTemplate = new FileNameTemplate(baseNamePattern);
            string fileNameMask = fileTemplate.ReplacePattern("*");
            string dateFormat = DateArchivalCommon.GetDateFormatString(Archival.Options.ArchiveDateFormat, Archival.Options.ArchiveEvery);

            string dirName = Path.GetDirectoryName(Path.GetFullPath(pattern));
            if (string.IsNullOrEmpty(dirName))
            {
                return;
            }

            int minSequenceLength = fileTemplate.EndAt - fileTemplate.BeginAt - 2;
            int nextSequenceNumber;
            DateTime archiveDate = DateArchivalCommon.GetArchiveDate(IsDaySwitch(fileName, logEvent), Archival.Options.ArchiveEvery);
            List<string> archiveFileNames;
            if (Directory.Exists(dirName))
            {
                List<DateAndSequenceArchive> archives = FindDateAndSequenceArchives(dirName, fileName, fileNameMask, minSequenceLength, dateFormat, fileTemplate)
                    .ToList();

                // Find out the next sequence number among existing archives having the same date part as the current date.
                int? lastSequenceNumber = archives
                    .Where(a => a.HasSameFormattedDate(archiveDate))
                    .Max(a => (int?)a.Sequence);
                nextSequenceNumber = (int)(lastSequenceNumber != null ? lastSequenceNumber + 1 : 0);

                archiveFileNames = archives
                    .OrderBy(a => a.Date)
                    .ThenBy(a => a.Sequence)
                    .Select(a => a.FileName)
                    .ToList();
            }
            else
            {
                Directory.CreateDirectory(dirName);
                nextSequenceNumber = 0;
                archiveFileNames = new List<string>();
            }

            string paddedSequence = nextSequenceNumber.ToString().PadLeft(minSequenceLength, '0');
            string archiveFileNameWithoutPath = fileNameMask.Replace("*",
                string.Format("{0}.{1}", archiveDate.ToString(dateFormat), paddedSequence));
            string archiveFileName = Path.Combine(dirName, archiveFileNameWithoutPath);

            Archival.RollArchiveForward(fileName, archiveFileName, allowCompress: true);
            archiveFileNames.Add(archiveFileName);
            DateArchivalCommon.EnsureArchiveCount(archiveFileNames, Archival.Options.MaxArchiveFiles);
        }

        /// <summary>
        /// Determines whether a file with a different name from <paramref name="fileName"/> is needed to receive the
        /// <paramref name="logEvent"/>. This is determined based on the last date and time which the file has been
        /// written compared to the time the log event was initiated.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> when log event time is "different" than the last write time; <see langword="false"/> otherwise.
        /// </returns>
        private bool IsDaySwitch(string fileName, LogEventInfo logEvent)
        {
            DateTime lastWriteTime;
            long fileLength;
            if (Archival.Callbacks.GetFileInfo(fileName, out lastWriteTime, out fileLength))
            {
                string formatString = DateArchivalCommon.GetDateFormatString(string.Empty, Archival.Options.ArchiveEvery);
                string ts = lastWriteTime.ToLocalTime().ToString(formatString, CultureInfo.InvariantCulture);
                string ts2 = logEvent.TimeStamp.ToLocalTime().ToString(formatString, CultureInfo.InvariantCulture);

                return ts != ts2;
            }

            return false;
        }

        /// <summary>
        /// Searches a given directory for archives that comply with the current archive pattern.
        /// </summary>
        /// <returns>An enumeration of archive infos, ordered by their file creation date.</returns>
        private static IEnumerable<DateAndSequenceArchive> FindDateAndSequenceArchives(string dirName, string logFileName,
            string fileNameMask,
            int minSequenceLength, string dateFormat, FileNameTemplate fileTemplate)
        {
            var directoryInfo = new DirectoryInfo(dirName);

            int archiveFileNameMinLength = fileNameMask.Length + minSequenceLength;
            var archiveFileNames = GetFiles(directoryInfo, fileNameMask)
                .Where(n => n.Name.Length >= archiveFileNameMinLength)
                .OrderBy(n => n.CreationTime)
                .Select(n => n.FullName);

            foreach (string archiveFileName in archiveFileNames)
            {
                //Get the archive file name or empty string if it's null
                string archiveFileNameWithoutPath = Path.GetFileName(archiveFileName) ?? "";

                DateTime date;
                int sequence;
                if (
                    !TryParseDateAndSequence(archiveFileNameWithoutPath, dateFormat, fileTemplate, out date,
                        out sequence))
                {
                    continue;
                }

                //It's possible that the log file itself has a name that will match the archive file mask.
                if (string.IsNullOrEmpty(archiveFileNameWithoutPath) ||
                    archiveFileNameWithoutPath.Equals(Path.GetFileName(logFileName)))
                {
                    continue;
                }

                yield return new DateAndSequenceArchive(archiveFileName, date, dateFormat, sequence);
            }
        }

        /// <summary>
        /// Attempts to parse date and sequence info from a filename.
        /// </summary>
        /// <param name="archiveFileNameWithoutPath">Filename without path</param>
        /// <param name="dateFormat">The expected DateTime format string.</param>
        /// <param name="fileTemplate">Indicates where the date and sequence info is expected to be in the filename.</param>
        /// <param name="date">Populated with the found date-time.</param>
        /// <param name="sequence">Populated with the found sequence number.</param>
        /// <returns>Returns true if date-time and sequence info is successfully found.</returns>
        private static bool TryParseDateAndSequence(string archiveFileNameWithoutPath, string dateFormat, FileNameTemplate fileTemplate, out DateTime date, out int sequence)
        {
            int trailerLength = fileTemplate.Template.Length - fileTemplate.EndAt;
            int dateAndSequenceIndex = fileTemplate.BeginAt;
            int dateAndSequenceLength = archiveFileNameWithoutPath.Length - trailerLength - dateAndSequenceIndex;

            string dateAndSequence = archiveFileNameWithoutPath.Substring(dateAndSequenceIndex, dateAndSequenceLength);
            int sequenceIndex = dateAndSequence.LastIndexOf('.') + 1;

            string sequencePart = dateAndSequence.Substring(sequenceIndex);
            if (!Int32.TryParse(sequencePart, NumberStyles.None, CultureInfo.CurrentCulture, out sequence))
            {
                date = default(DateTime);
                return false;
            }

            string datePart = dateAndSequence.Substring(0, dateAndSequence.Length - sequencePart.Length - 1);
            if (!DateTime.TryParseExact(datePart, dateFormat, CultureInfo.CurrentCulture, DateTimeStyles.None,
                out date))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Gets the collection of files in the specified directory which they match the <paramref name="fileNameMask"/>.
        /// </summary>
        /// <param name="directoryInfo">Directory to searched.</param>
        /// <param name="fileNameMask">Pattern whihc the files will be searched against.</param>
        /// <returns>Lisf of files matching the pattern.</returns>
        private static IEnumerable<FileInfo> GetFiles(DirectoryInfo directoryInfo, string fileNameMask)
        {
#if SILVERLIGHT
            return directoryInfo.EnumerateFiles(fileNameMask);
#else
            return directoryInfo.GetFiles(fileNameMask);
#endif
        }
    }

#endif // !NET_CF
}
