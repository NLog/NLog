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

namespace NLog.Targets
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using NLog.Time;

    internal abstract class DateBasedFileArchive : BaseFileArchive
    {
        public DateBasedFileArchive(FileTarget target) : base(target) { }

        /// <summary>
        /// Gets or sets a value indicating whether to automatically archive log files every time the specified time passes.
        /// </summary>
        /// <remarks>
        /// Files are moved to the archive as part of the write operation if the current period of time changes. For
        /// example if the current <c>hour</c> changes from 10 to 11, the first write that will occur on or after 11:00
        /// will trigger the archiving.
        /// <p>
        /// Caution: Enabling this option can considerably slow down your file logging in multi-process scenarios. If
        /// only one process is going to be writing to the file, consider setting <c>ConcurrentWrites</c> to
        /// <c>false</c> for maximum performance.
        /// </p>
        /// </remarks>
        public FileArchivePeriod ArchiveEvery { get; set; }

        /// <summary>
        /// Gets or sets a value specifying the date format to use when archving files.
        /// </summary>
        public string ArchiveDateFormat { get; set; }

        protected static string ReplaceFileNamePattern(string pattern, string replacementValue)
        {
            return new FileNameTemplate(Path.GetFileName(pattern)).ReplacePattern(replacementValue);
        }

        // TODO: Method duplicated in FileArchiver class.
        protected string GetDateFormatString(string defaultFormat)
        {
            // If archiveDateFormat is not set in the config file, use a default 
            // date format string based on the archive period.
            string formatString = defaultFormat;
            if (string.IsNullOrEmpty(formatString))
            {
                switch (ArchiveEvery)
                {
                    case FileArchivePeriod.Year:
                        formatString = "yyyy";
                        break;

                    case FileArchivePeriod.Month:
                        formatString = "yyyyMM";
                        break;

                    default:
                        formatString = "yyyyMMdd";
                        break;

                    case FileArchivePeriod.Hour:
                        formatString = "yyyyMMddHH";
                        break;

                    case FileArchivePeriod.Minute:
                        formatString = "yyyyMMddHHmm";
                        break;
                }
            }
            return formatString;
        }

        protected DateTime GetArchiveDate(bool isNextCycle)
        {
            DateTime archiveDate = TimeSource.Current.Time;

            // Because AutoArchive/DateArchive gets called after the FileArchivePeriod condition matches, decrement the archive period by 1
            // (i.e. If ArchiveEvery = Day, the file will be archived with yesterdays date)
            int addCount = isNextCycle ? -1 : 0;

            switch (ArchiveEvery)
            {
                case FileArchivePeriod.Day:
                    archiveDate = archiveDate.AddDays(addCount);
                    break;

                case FileArchivePeriod.Hour:
                    archiveDate = archiveDate.AddHours(addCount);
                    break;

                case FileArchivePeriod.Minute:
                    archiveDate = archiveDate.AddMinutes(addCount);
                    break;

                case FileArchivePeriod.Month:
                    archiveDate = archiveDate.AddMonths(addCount);
                    break;

                case FileArchivePeriod.Year:
                    archiveDate = archiveDate.AddYears(addCount);
                    break;
            }

            return archiveDate;
        }

        /// <summary>
        /// Deletes files among a given list, and stops as soon as the remaining files are fewer than the Size property.
        /// </summary>
        /// <remarks>
        /// Items are deleted in the same order as in <paramref name="oldArchiveFileNames" />.
        /// No file is deleted if MaxArchiveFile is equal to zero.
        /// </remarks>
        protected void EnsureArchiveCount(List<string> oldArchiveFileNames)
        {
            if (Size <= 0)
            {
                return;
            }

            int numberToDelete = oldArchiveFileNames.Count - Size;
            for (int fileIndex = 0; fileIndex <= numberToDelete; fileIndex++)
            {
                File.Delete(oldArchiveFileNames[fileIndex]);
            }
        }
    }
}
