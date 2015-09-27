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
    using System.ComponentModel;
    using System.Globalization;
    using System.IO;
#if !SILVERLIGHT
    using System.IO.Compression;
#endif
    using System.Linq;
    using System.Text;
    using System.Threading;
    using Common;
    using Config;
    using Internal;
    using Internal.FileAppenders;
    using Layouts;
    using Time;

    internal sealed class FileArchiver 
    {
        public const int ArchiveAboveSizeDisabled = -1;

        /// <summary>
        /// Gets or sets the size in bytes above which log files will be automatically archived.
        /// 
        /// Warning: combining this with <see cref="ArchiveNumberingMode.Date"/> isn't supported. We cannot create multiple archive files, if they should have the same name.
        /// Choose:  <see cref="ArchiveNumberingMode.DateAndSequence"/> 
        /// </summary>
        /// <remarks>
        /// Caution: Enabling this option can considerably slow down your file 
        /// logging in multi-process scenarios. If only one process is going to
        /// be writing to the file, consider setting <c>ConcurrentWrites</c>
        /// to <c>false</c> for maximum performance.
        /// </remarks>
        public long ArchiveAboveSize { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to automatically archive log files every time the specified time passes.
        /// </summary>
        /// <remarks>
        /// Files are moved to the archive as part of the write operation if the current period of time changes. For example
        /// if the current <c>hour</c> changes from 10 to 11, the first write that will occur
        /// on or after 11:00 will trigger the archiving.
        /// <p>
        /// Caution: Enabling this option can considerably slow down your file 
        /// logging in multi-process scenarios. If only one process is going to
        /// be writing to the file, consider setting <c>ConcurrentWrites</c>
        /// to <c>false</c> for maximum performance.
        /// </p>
        /// </remarks>
        public FileArchivePeriod ArchiveEvery { get; set; }

        /// <summary>
        /// Gets or sets a value specifying the date format to use when archving files.
        /// </summary>
        public string ArchiveDateFormat { get; set; }

#if NET4_5
        /// <summary>
        /// Gets or sets a value indicating whether to compress archive files into the zip archive format.
        /// </summary>
        public bool EnableArchiveFileCompression { get; set; }
#else
        /// <summary>
        /// Gets or sets a value indicating whether to compress archive files into the zip archive format.
        /// </summary>
        private const bool EnableArchiveFileCompression = false;
#endif

        /// <summary>
        /// Max
        /// </summary>
        public int Size
        {
            get
            {
                return fileArchive.Size;
            }

            set
            {
                fileArchive.Size = value;
            }
        }

        public FileTarget Target { get; private set; } 

        public FileArchiver(FileTarget target)
        {
            Target = target;
        }

        public bool Archive(string archiveFileName, string fileName, bool createDirectory, bool enableCompression)
        {
            return fileArchive.Archive(archiveFileName, fileName, createDirectory, enableCompression);
        }

        public void RecursiveRollingRename(string fileName, string pattern, int archiveNumber)
        {
            if (Size > 0 && archiveNumber >= Size)
            {
                File.Delete(fileName);
                return;
            }

            if (!File.Exists(fileName))
            {
                return;
            }

            string newFileName = ReplaceNumberPattern(pattern, archiveNumber);
            if (File.Exists(fileName))
            {
                RecursiveRollingRename(newFileName, pattern, archiveNumber + 1);
            }

            InternalLogger.Trace("Renaming {0} to {1}", fileName, newFileName);

            var shouldCompress = archiveNumber == 0;
            try
            {
                RollArchiveForward(fileName, newFileName, shouldCompress);
            }
            catch (IOException)
            {
                // TODO: Check the value of CreateDirs property before creating directories.
                string dir = Path.GetDirectoryName(newFileName);
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                RollArchiveForward(fileName, newFileName, shouldCompress);
            }
        }

        public void SequentialArchive(string fileName, string pattern)
        {
            FileNameTemplate fileTemplate = new FileNameTemplate(Path.GetFileName(pattern));
            int trailerLength = fileTemplate.Template.Length - fileTemplate.EndAt;
            string fileNameMask = fileTemplate.ReplacePattern("*");

            string dirName = Path.GetDirectoryName(Path.GetFullPath(pattern));
            int nextNumber = -1;
            int minNumber = -1;

            var number2Name = new Dictionary<int, string>();

            try
            {
#if SILVERLIGHT
                foreach (string s in Directory.EnumerateFiles(dirName, fileNameMask))
#else
                foreach (string s in Directory.GetFiles(dirName, fileNameMask))
#endif
                {
                    string baseName = Path.GetFileName(s);
                    string number = baseName.Substring(fileTemplate.BeginAt, baseName.Length - trailerLength - fileTemplate.BeginAt);
                    int num;

                    try
                    {
                        num = Convert.ToInt32(number, CultureInfo.InvariantCulture);
                    }
                    catch (FormatException)
                    {
                        continue;
                    }

                    nextNumber = Math.Max(nextNumber, num);
                    minNumber = minNumber != -1 ? Math.Min(minNumber, num) : num;

                    number2Name[num] = s;
                }

                nextNumber++;
            }
            catch (DirectoryNotFoundException)
            {
                Directory.CreateDirectory(dirName);
                nextNumber = 0;
            }

            if (minNumber != -1 && Size != 0)
            {
                int minNumberToKeep = nextNumber - Size + 1;
                for (int i = minNumber; i < minNumberToKeep; ++i)
                {
                    string s;

                    if (number2Name.TryGetValue(i, out s))
                    {
                        File.Delete(s);
                    }
                }
            }

            string newFileName = ReplaceNumberPattern(pattern, nextNumber);
            RollArchiveForward(fileName, newFileName, shouldCompress: true);
        }

#if !NET_CF
        public void DateArchive(string fileName, string pattern)
        {
            string fileNameMask = ReplaceFileNamePattern(pattern, "*");
            string dirName = Path.GetDirectoryName(Path.GetFullPath(pattern));
            string dateFormat = GetDateFormatString(ArchiveDateFormat);

            DeleteOldDateArchive(pattern);

            DateTime newFileDate = GetArchiveDate(true);
            if (dirName != null)
            {
                string newFileName = Path.Combine(dirName, fileNameMask.Replace("*", newFileDate.ToString(dateFormat)));
                RollArchiveForward(fileName, newFileName, shouldCompress: true);
            }
        }

        public void DateAndSequentialArchive(string fileName, string pattern, LogEventInfo logEvent)
        {
            string baseNamePattern = Path.GetFileName(pattern);

            if (string.IsNullOrEmpty(baseNamePattern))
            {
                return;
            }

            FileNameTemplate fileTemplate = new FileNameTemplate(baseNamePattern);
            string fileNameMask = fileTemplate.ReplacePattern("*");
            string dateFormat = GetDateFormatString(this.ArchiveDateFormat);

            string dirName = Path.GetDirectoryName(Path.GetFullPath(pattern));
            if (string.IsNullOrEmpty(dirName))
            {
                return;
            }

            int minSequenceLength = fileTemplate.EndAt - fileTemplate.BeginAt - 2;
            int nextSequenceNumber;
            DateTime archiveDate = GetArchiveDate(IsDaySwitch(fileName, logEvent));
            try
            {
                List<DateAndSequenceArchive> archives = FindDateAndSequenceArchives(dirName, fileName, fileNameMask, minSequenceLength, dateFormat, fileTemplate)
                    .ToList();

                // Find out the next sequence number among existing archives having the same date part as the current date.
                int? lastSequenceNumber = archives
                    .Where(a => a.HasSameFormattedDate(archiveDate))
                    .Max(a => (int?) a.Sequence);
                nextSequenceNumber = (int) (lastSequenceNumber != null ? lastSequenceNumber + 1 : 0);

                var oldArchiveFileNames = archives
                    .OrderBy(a => a.Date)
                    .ThenBy(a => a.Sequence)
                    .Select(a => a.FileName)
                    .ToList();
                EnsureArchiveCount(oldArchiveFileNames);
            }
            catch (DirectoryNotFoundException)
            {
                Directory.CreateDirectory(dirName);
                nextSequenceNumber = 0;
            }

            string paddedSequence = nextSequenceNumber.ToString().PadLeft(minSequenceLength, '0');
            string newFileNameWithoutPath = fileNameMask.Replace("*",
                string.Format("{0}.{1}", archiveDate.ToString(dateFormat), paddedSequence));
            string newFileName = Path.Combine(dirName, newFileNameWithoutPath);

            RollArchiveForward(fileName, newFileName, shouldCompress: true);
        }

        private static void ArchiveFile(string fileName, string archiveFileName, bool enableCompression)
        {
#if NET4_5
            if (enableCompression)
            {
                using (var archiveStream = new FileStream(archiveFileName, FileMode.Create))
                using (var archive = new ZipArchive(archiveStream, ZipArchiveMode.Create))
                using (var originalFileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    var zipArchiveEntry = archive.CreateEntry(Path.GetFileName(fileName));
                    using (var destination = zipArchiveEntry.Open())
                    {
                        originalFileStream.CopyTo(destination);
                    }
                }

                File.Delete(fileName);
            }
            else
#endif
            {
                File.Move(fileName, archiveFileName);
            }
        }

        private static string ReplaceFileNamePattern(string pattern, string replacementValue)
        {
            return new FileNameTemplate(Path.GetFileName(pattern)).ReplacePattern(replacementValue);
        }

        /// <summary>
        /// Deletes files among a given list, and stops as soon as the remaining files are fewer than the Size property.
        /// </summary>
        /// <remarks>
        /// Items are deleted in the same order as in <paramref name="oldArchiveFileNames" />.
        /// No file is deleted if MaxArchiveFile is equal to zero.
        /// </remarks>
        private void EnsureArchiveCount(List<string> oldArchiveFileNames)
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

        /// <summary>
        /// Searches a given directory for archives that comply with the current archive pattern.
        /// </summary>
        /// <returns>An enumeration of archive infos, ordered by their file creation date.</returns>
        private IEnumerable<DateAndSequenceArchive> FindDateAndSequenceArchives(string dirName, string logFileName,
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

        private static IEnumerable<FileInfo> GetFiles(DirectoryInfo directoryInfo, string fileNameMask)
        {
#if SILVERLIGHT
            return directoryInfo.EnumerateFiles(fileNameMask);
#else
            return directoryInfo.GetFiles(fileNameMask);
#endif
        }

        /// <summary>
        /// Determines whether a file with a different name from <paramref name="fileName"/> is needed to receive <paramref name="logEvent"/>.
        /// </summary>
        private bool IsDaySwitch(string fileName, LogEventInfo logEvent)
        {
            DateTime lastWriteTime;
            long fileLength;
            if (Target.GetFileInfo(fileName, out lastWriteTime, out fileLength))
            {
                string formatString = GetDateFormatString(string.Empty);
                string ts = lastWriteTime.ToString(formatString, CultureInfo.InvariantCulture);
                string ts2 = logEvent.TimeStamp.ToLocalTime().ToString(formatString, CultureInfo.InvariantCulture);

                return ts != ts2;
            }

            return false;
        }

        /// <summary>
        /// Deletes archive files in reverse chronological order until only the
        /// MaxArchiveFiles number of archive files remain.
        /// </summary>
        /// <param name="pattern">The pattern that archive filenames will match</param>
        public void DeleteOldDateArchive(string pattern)
        {

            string fileNameMask = ReplaceFileNamePattern(pattern, "*");
            string dirName = Path.GetDirectoryName(Path.GetFullPath(pattern));
            string dateFormat = GetDateFormatString(this.ArchiveDateFormat);

            try
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(dirName);
#if SILVERLIGHT
                List<string> files = directoryInfo.EnumerateFiles(fileNameMask).OrderBy(n => n.CreationTime).Select(n => n.FullName).ToList();
#else
                List<string> files = directoryInfo.GetFiles(fileNameMask).OrderBy(n => n.CreationTime).Select(n => n.FullName).ToList();
#endif
                List<string> filesByDate = new List<string>();

                for (int index = 0; index < files.Count; index++)
                {
                    string archiveFileName = Path.GetFileName(files[index]);
                    string datePart = archiveFileName.Substring(fileNameMask.LastIndexOf('*'), dateFormat.Length);
                    DateTime fileDate = DateTime.MinValue;
                    if (DateTime.TryParseExact(datePart, dateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out fileDate))
                    {
                        filesByDate.Add(files[index]);
                    }
                }

                EnsureArchiveCount(filesByDate);
            }
            catch (DirectoryNotFoundException)
            {
                Directory.CreateDirectory(dirName);
            }
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

        private DateTime GetArchiveDate(bool isNextCycle)
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

        private void RollArchiveForward(string existingFileName, string archiveFileName, bool shouldCompress)
        {
            ArchiveFile(existingFileName, archiveFileName, shouldCompress && EnableArchiveFileCompression);

            string fileName = Path.GetFileName(existingFileName);
            if (fileName == null) { return; }

            // When the file has been moved, the original filename is 
            // no longer one of the initializedFiles. The initializedFilesCounter
            // should be left alone, the amount is still valid.
            if (Target.Files.Contains(fileName))
            {
                Target.Files.Remove(fileName);
            }
            else if (Target.Files.Contains(existingFileName))
            {
                Target.Files.Remove(existingFileName);
            }
        }

        private static string ReplaceNumberPattern(string pattern, int value)
        {
            int firstPart = pattern.IndexOf("{#", StringComparison.Ordinal);
            int lastPart = pattern.IndexOf("#}", StringComparison.Ordinal) + 2;
            int numDigits = lastPart - firstPart - 2;

            return pattern.Substring(0, firstPart) + Convert.ToString(value, 10).PadLeft(numDigits, '0') + pattern.Substring(lastPart);
        }

        private sealed class DynamicFileArchive
        {
            /// <summary>
            /// Max
            /// </summary>
            public int Size { get; set; }

            public DynamicFileArchive()
            {
                fileQueue = new Queue<string>();
            }

            public DynamicFileArchive(int size)
            {
                Size = size;
                fileQueue = new Queue<string>(size);
            }

            /// <summary>
            /// Adds a file into archive.
            /// </summary>
            /// <param name="archiveFileName">File name of the archive</param>
            /// <param name="fileName">Original file name</param>
            /// <param name="createDirectory">Create a directory, if it does not exist</param>
            /// <param name="enableCompression">Enables file compression</param>
            /// <returns><c>true</c> if the file has been moved successfully; <c>false</c> otherwise</returns>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
            public bool Archive(string archiveFileName, string fileName, bool createDirectory, bool enableCompression)
            {
                if (Size < 1)
                {
                    InternalLogger.Warn("Archive is called. Even though the MaxArchiveFiles is set to less than 1");
                    return false;
                }

                if (!File.Exists(fileName))
                {
                    InternalLogger.Error("Error while archiving, Source File : {0} Not found.", fileName);
                    return false;
                }

                DeleteOldArchiveFiles();
                AddToArchive(archiveFileName, fileName, createDirectory, enableCompression);
                fileQueue.Enqueue(archiveFileName);
                return true;
            }

            private void AddToArchive(string archiveFileName, string fileName, bool createDirectory, bool enableCompression)
            {
                String alternativeFileName = archiveFileName;

                if (fileQueue.Contains(archiveFileName))
                {
                    InternalLogger.Trace("AddToArchive file {0} already exist. Trying different file name.", archiveFileName);
                    alternativeFileName = FindSuitableFilename(archiveFileName, 1);
                }

                try
                {
                    ArchiveFile(fileName, alternativeFileName, enableCompression);
                }
                catch (DirectoryNotFoundException)
                {
                    if (createDirectory)
                    {
                        InternalLogger.Trace("AddToArchive directory not found. Creating {0}", Path.GetDirectoryName(archiveFileName));

                        try
                        {
                            Directory.CreateDirectory(Path.GetDirectoryName(archiveFileName));
                            ArchiveFile(fileName, alternativeFileName, enableCompression);
                        }
                        catch (Exception ex)
                        {
                            InternalLogger.Error("Cannot create archive directory, Exception : {0}", ex);
                            throw;
                        }
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    InternalLogger.Error("Cannot archive file {0}, Exception : {1}", fileName, ex);
                    throw;
                }
            }

            /// <summary>
            /// Remove old archive files when the files on the queue are more than the 
            /// MaxArchiveFilesToKeep.  
            /// </summary>
            private void DeleteOldArchiveFiles()
            {
                // TODO: When the Size = 1 than ONLY a single file will be deleted. Is this the intended behavior? 
                if (Size == 1 && fileQueue.Any())
                {
                    string archiveFileName = fileQueue.Dequeue();
                    DeleteFile(archiveFileName);
                }

                while (fileQueue.Count >= Size)
                {
                    string oldestArchivedFileName = fileQueue.Dequeue();
                    DeleteFile(oldestArchivedFileName);
                }
            }

            private static void DeleteFile(string fileName)
            {
                try
                {
                    File.Delete(fileName);
                }
                catch (Exception ex)
                {
                    InternalLogger.Warn("Cannot delete old archive file: {0}, Exception : {1}", fileName, ex);
                }
            }

            /// <summary>
            /// Creates a new unique filename by appending a number to it. This method tests that 
            /// the filename created does not exist.
            /// 
            /// This process can be slow as it increments the number sequentially from a specified 
            /// starting point until it finds a number which produces a filename which does not 
            /// exist.
            /// 
            /// Example: 
            ///     Original Filename   trace.log
            ///     Target Filename     trace.15.log
            /// </summary>          
            /// <param name="fileName">Original filename</param>
            /// <param name="numberToStartWith">Number starting point</param>
            /// <returns>File name suitable for archiving</returns>
            private string FindSuitableFilename(string fileName, int numberToStartWith)
            {
                String targetFileName = Path.GetFileNameWithoutExtension(fileName) + ".{#}" + Path.GetExtension(fileName);

                while (File.Exists(ReplaceNumberPattern(targetFileName, numberToStartWith)))
                {
                    InternalLogger.Trace("AddToArchive file {0} already exist. Trying with different file name.", fileName);
                    numberToStartWith++;
                }
                return targetFileName;
            }

            private static void ArchiveFile(string fileName, string archiveFileName, bool enableCompression)
            {
#if NET4_5
                if (enableCompression)
                {
                    using (var archiveStream = new FileStream(archiveFileName, FileMode.Create))
                    using (var archive = new ZipArchive(archiveStream, ZipArchiveMode.Create))
                    using (var originalFileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        var zipArchiveEntry = archive.CreateEntry(Path.GetFileName(fileName));
                        using (var destination = zipArchiveEntry.Open())
                        {
                            originalFileStream.CopyTo(destination);
                        }
                    }

                    File.Delete(fileName);
                }
                else
#endif
                {
                    File.Move(fileName, archiveFileName);
                }
            }

            private readonly Queue<string> fileQueue;
        }

        private sealed class FileNameTemplate
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
            public string Template
            {
                get { return this.template; }
            }

            /// <summary>
            /// The begging position of the <see cref="P:FileNameTemplate.Pattern"/> 
            /// within the <see cref="P:FileNameTemplate.Template"/>. -1 is returned 
            /// when no pattern can be found.
            /// </summary>
            public int BeginAt
            {
                get
                {
                    return startIndex;
                }
            }

            /// <summary>
            /// The ending position of the <see cref="P:FileNameTemplate.Pattern"/> 
            /// within the <see cref="P:FileNameTemplate.Template"/>. -1 is returned 
            /// when no pattern can be found.
            /// </summary>
            public int EndAt
            {
                get
                {
                    return endIndex;
                }
            }

            private readonly string template;

            private readonly int startIndex;
            private readonly int endIndex;

            public FileNameTemplate(string template)
            {
                this.template = template;
                this.startIndex = template.IndexOf(PatternStartCharacters, StringComparison.Ordinal);
                this.endIndex = template.IndexOf(PatternEndCharacters, StringComparison.Ordinal) + PatternEndCharacters.Length;
            }

            /// <summary>
            /// Replace the pattern with the specified String.
            /// </summary>
            /// <param name="replacementValue"></param>
            /// <returns></returns>
            public string ReplacePattern(string replacementValue)
            {
                return String.IsNullOrEmpty(replacementValue) ? this.Template : template.Substring(0, this.BeginAt) + replacementValue + template.Substring(this.EndAt);
            }
        }

        private readonly DynamicFileArchive fileArchive = new DynamicFileArchive();
    }
}
