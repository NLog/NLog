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

using NLog.Common;
using NLog.Time;
using System;
using System.Collections.Generic;
using System.IO;

namespace NLog.Targets.FileTargetArchival
{
    /// <summary>
    /// Holds archival routines common to the date-time and date-time-sequence archival behaviors.
    /// </summary>
    static class DateArchivalCommon
    {
#if !NET_CF
        /// <summary>
        /// Gets the correct formating <see langword="String"/> to be used based on the value of <c>archiveEvery</c>
        /// for converting <see langword="DateTime"/> values which will be inserting into file
        /// names during archiving.
        /// 
        /// This value will be computed only when a empty value or <see langword="null"/> is passed into <paramref name="defaultFormat"/>
        /// </summary>
        /// <param name="defaultFormat">Date format to used irrespectively of <c>archiveEvery</c> value.</param>
        /// <param name="archiveEvery">Indicates the timespan to elapse between archived files.
        /// This determines how much date info is needed in a formatted filename.</param>
        /// <returns>Formatting <see langword="String"/> for dates.</returns>
        public static string GetDateFormatString(string defaultFormat, FileArchivePeriod archiveEvery)
        {
            // If archiveDateFormat is not set in the config file, use a default 
            // date format string based on the archive period.
            string formatString = defaultFormat;
            if (string.IsNullOrEmpty(formatString))
            {
                switch (archiveEvery)
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

        /// <summary>
        /// Gets the archive date that should be used in a file name.
        /// </summary>
        /// <param name="isNextCycle">If true, the current date is returned.
        /// If false, the current date minus one archival period is returned.</param>
        /// <param name="archiveEvery">The archival period.</param>
        /// <returns>Returns the requested archival date.</returns>
        public static DateTime GetArchiveDate(bool isNextCycle, FileArchivePeriod archiveEvery)
        {
            DateTime archiveDate = TimeSource.Current.Time;

            // Because AutoArchive/ArchiveByDate gets called after the FileArchivePeriod condition matches, decrement the archive period by 1
            // (i.e. If ArchiveEvery = Day, the file will be archived with yesterdays date)
            int addCount = isNextCycle ? -1 : 0;

            switch (archiveEvery)
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
        /// Deletes files among a given list, and stops as soon as the remaining files are fewer than the
        /// <c>marArchiveFiles</c> value.
        /// </summary>
        /// <param name="oldArchiveFileNames">List of the file archives.</param>
        /// <param name="maxArchiveFiles">The maximum number of archive files to keep.</param>
        /// <remarks>
        /// Items are deleted in the same order as in <paramref name="oldArchiveFileNames"/>.
        /// No file is deleted if <c>marArchiveFiles</c> is zero.
        /// </remarks>
        public static void EnsureArchiveCount(List<string> oldArchiveFileNames, int maxArchiveFiles)
        {
            if (maxArchiveFiles <= 0) return;

            int numberToDelete = oldArchiveFileNames.Count - maxArchiveFiles;
            for (int fileIndex = 0; fileIndex < numberToDelete; fileIndex++)
            {
                InternalLogger.Info("Deleting old archive {0}.", oldArchiveFileNames[fileIndex]);
                File.Delete(oldArchiveFileNames[fileIndex]);
            }
        }
#endif // !NET_CF
    }
}
