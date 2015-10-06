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
    using System.Globalization;
    using System.IO;
#if !SILVERLIGHT
    using System.IO.Compression;
#endif
    using NLog.Time;

    internal sealed class FileArchiver 
    {
        public const int ArchiveAboveSizeDisabled = -1;

        private readonly DynamicFileArchive dynamicArchive;
        private readonly SequentialFileArchive sequentialArchive;
        private readonly RollingFileArchive rollingArchive;
#if !NET_CF
        private readonly DateFileArchive dateArchive;
        private readonly DateAndSequentialFileArchive dateAndSequentialArchive;
#endif
        private FileArchivePeriod archiveEvery = FileArchivePeriod.None;
        private string archiveDateFormat = String.Empty;
        private bool compressionEnabled;
        private int size = 0;

        public FileArchiver(FileTarget target)
        {
            Target = target;
            dynamicArchive = new DynamicFileArchive(target);
            sequentialArchive = new SequentialFileArchive(target);
            rollingArchive = new RollingFileArchive(target);
#if !NET_CF
            dateArchive = new DateFileArchive(target);
            dateAndSequentialArchive = new DateAndSequentialFileArchive(target);
#endif
        }

        /// <summary>
        /// Gets or sets the size in bytes above which log files will be automatically archived.
        /// 
        /// Warning: combining this with <see cref="ArchiveNumberingMode.Date"/> isn't supported. We cannot create
        ///          multiple archive files, if they should have the same name.
        /// Choose: <see cref="ArchiveNumberingMode.DateAndSequence"/>
        /// </summary>
        /// <remarks>
        /// Caution: Enabling this option can considerably slow down your file logging in multi-process scenarios. If
        /// only one process is going to be writing to the file, consider setting <c>ConcurrentWrites</c> to
        /// <c>false</c> for maximum performance.
        /// </remarks>
        public long ArchiveAboveSize { get; set; }

        /// <summary>
        /// Gets or sets a value specifying the date format to use when archving files.
        /// </summary>
        public string ArchiveDateFormat
        {
            get { return archiveDateFormat; }

            set
            {
                archiveDateFormat = value;
#if !NET_CF
                dateArchive.DateFormat = archiveDateFormat;
                dateAndSequentialArchive.DateFormat = archiveDateFormat;
#endif
            }
        }

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
        public FileArchivePeriod ArchiveEvery
        {

            get { return archiveEvery; }

            set
            {
                archiveEvery = value;
#if !NET_CF
                dateArchive.Period = archiveEvery;
                dateAndSequentialArchive.Period = archiveEvery;
#endif
            }
        }

#if NET4_5
        /// <summary>
        /// Gets or sets a value indicating whether to compress archive files into the zip archive format.
        /// </summary>
        public bool CompressionEnabled {
            get { return compressionEnabled; }

            set
            {
                compressionEnabled = value;
                dynamicArchive.CompressionEnabled = compressionEnabled;
                sequentialArchive.CompressionEnabled = compressionEnabled;
                rollingArchive.CompressionEnabled = compressionEnabled;
                dateArchive.CompressionEnabled = compressionEnabled;
                dateAndSequentialArchive.CompressionEnabled = compressionEnabled;
            }
        }
#else
        /// <summary>
        /// Gets or sets a value indicating whether to compress archive files into the zip archive format.
        /// </summary>
        private const bool CompressionEnabled = false;
#endif

        /// <summary>
        /// Max
        /// </summary>
        public int Size
        {
            get { return size; }

            set {
                size = value;
                dynamicArchive.Size = size;
                sequentialArchive.Size = size;
                rollingArchive.Size = size;
#if !NET_CF
                dateArchive.Size = size;
                dateAndSequentialArchive.Size = size;
#endif
            }
        }

        public FileTarget Target { get; private set; } 

        public bool DynamicArchive(string archiveFileName, string fileName, bool createDirectory)
        {
            return dynamicArchive.Process(archiveFileName, fileName, createDirectory, CompressionEnabled);
        }

        public void RollingArchive(string fileName, string pattern)
        {
            rollingArchive.Process(fileName, pattern);
        }

        public void SequentialArchive(string fileName, string pattern)
        {
            sequentialArchive.Process(fileName, pattern);
        }

#if !NET_CF
        public void DateArchive(string fileName, string pattern)
        {
            dateArchive.Process(fileName, pattern);
        }

        public void DateAndSequentialArchive(string fileName, string pattern, LogEventInfo logEvent)
        {
            dateAndSequentialArchive.Process(fileName, pattern, logEvent);
        }

        /// <summary>
        /// Deletes archive files in reverse chronological order until only the
        /// MaxArchiveFiles number of archive files remain.
        /// </summary>
        /// <param name="pattern">The pattern that archive filenames will match</param>
        public void DeleteOldDateArchive(string pattern)
        {
            dateArchive.DeleteOutdatedFiles(pattern);
        }
#endif

        public bool ShouldAutoArchiveBasedOnFileSize(string fileName, int upcomingWriteSize)
        {
            if (this.ArchiveAboveSize == FileArchiver.ArchiveAboveSizeDisabled)
            {
                return false;
            }

            DateTime lastWriteTime;
            long fileLength;

            if (!Target.GetFileInfo(fileName, out lastWriteTime, out fileLength))
            {
                return false;
            }

            if (this.ArchiveAboveSize != FileArchiver.ArchiveAboveSizeDisabled)
            {
                if (fileLength + upcomingWriteSize > this.ArchiveAboveSize)
                {
                    return true;
                }
            }

            return false;
        }

        public bool ShouldAutoArchiveBasedOnTime(string fileName, LogEventInfo logEvent)
        {
            if (this.ArchiveEvery == FileArchivePeriod.None)
            {
                return false;
            }

            DateTime lastWriteTime;
            long fileLength;

            if (!Target.GetFileInfo(fileName, out lastWriteTime, out fileLength))
            {
                return false;
            }

            if (this.ArchiveEvery != FileArchivePeriod.None)
            {
                // file write time is in Utc and logEvent's timestamp is originated from TimeSource.Current,
                // so we should ask the TimeSource to convert file time to TimeSource time:
                lastWriteTime = TimeSource.Current.FromSystemTime(lastWriteTime);
                string formatString = GetDateFormatString(string.Empty);
                string fileLastChanged = lastWriteTime.ToString(formatString, CultureInfo.InvariantCulture);
                string logEventRecorded = logEvent.TimeStamp.ToString(formatString, CultureInfo.InvariantCulture);

                if (fileLastChanged != logEventRecorded)
                {
                    return true;
                }
            }

            return false;
        }

        // TODO: Method duplicated in DateBasedFileArchive class.
        private string GetDateFormatString(string defaultFormat)
        {
            // If archiveDateFormat is not set in the config file, use a default 
            // date format string based on the archive period.
            string formatString = defaultFormat;
            if (string.IsNullOrEmpty(formatString))
            {
                switch (this.ArchiveEvery)
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
    }
}
