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
    using NLog.Internal;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Text;

    interface IFileArchiveStyle
    {
        /// <summary>
        /// Check if cleanup should be performed on initialize new file
        /// </summary>
        /// <param name="archiveFilePath">Base archive file pattern</param>
        /// <returns>True, when archive cleanup is needed</returns>
        bool AttemptCleanupOnInitializeFile(string archiveFilePath);

        /// <summary>
        /// Create a wildcard file-mask that allows one to find all files belonging to the same archive.
        /// </summary>
        /// <param name="archiveFilePath">Base archive file pattern</param>
        /// <returns>Wildcard file-mask</returns>
        string GenerateFileNameMask(string archiveFilePath);
        
        /// <summary>
        /// Search directory for all existing files that are part of the same archive.
        /// </summary>
        /// <param name="archiveFilePath">Base archive file pattern</param>
        /// <returns></returns>
        List<DateAndSequenceArchive> GetExistingArchiveFiles(string archiveFilePath);
        
        /// <summary>
        /// Generate the next archive filename for the archive.
        /// </summary>
        /// <param name="archiveFilePath">Base archive file pattern</param>
        /// <param name="archiveDate">File date of archive</param>
        /// <param name="existingArchiveFiles">Existing files in the same archive</param>
        /// <returns></returns>
        DateAndSequenceArchive GenerateArchiveFileName(string archiveFilePath, DateTime archiveDate, List<DateAndSequenceArchive> existingArchiveFiles);

        /// <summary>
        /// Return all files that should be removed from the provided archive.
        /// </summary>
        /// <param name="archiveFilePath">Base archive file pattern</param>
        /// <param name="existingArchiveFiles">Existing files in the same archive</param>
        /// <param name="maxArchiveFiles">Maximum number of archive files that should be kept</param>
        /// <returns></returns>
        IEnumerable<DateAndSequenceArchive> CheckArchiveCleanup(string archiveFilePath, List<DateAndSequenceArchive> existingArchiveFiles, int maxArchiveFiles);
    };

    static class FileArchiveStyleFactory
    {
        public static IFileArchiveStyle CreateArchiveStyle(string archiveFilePath, ArchiveNumberingMode archiveNumbering, string dateFormat)
        {
            if (ContainsFileNamePattern(archiveFilePath))
            {
                switch (archiveNumbering)
                {
                    case ArchiveNumberingMode.Rolling: return new FileArchiveModeRolling();
                    case ArchiveNumberingMode.Sequence: return new FileArchiveModeSequence(dateFormat);
                    case ArchiveNumberingMode.Date: return new FileArchiveModeDate(dateFormat);
                    case ArchiveNumberingMode.DateAndSequence: return new FileArchiveModeDateAndSequence(dateFormat);
                }
            }

            return new FileArchiveModeDynamicSequence(archiveNumbering, dateFormat);
        }

        /// <summary>
        /// Determines if the file name as <see cref="String"/> contains a numeric pattern i.e. {#} in it.  
        ///
        /// Example: 
        ///     trace{#}.log        Contains the numeric pattern.
        ///     trace{###}.log      Contains the numeric pattern.
        ///     trace{#X#}.log      Contains the numeric pattern (See remarks).
        ///     trace.log           Does not contain the pattern.
        /// </summary>
        /// <remarks>Occasionally, this method can identify the existence of the {#} pattern incorrectly.</remarks>
        /// <param name="fileName">File name to be checked.</param>
        /// <returns><see langword="true"/> when the pattern is found; <see langword="false"/> otherwise.</returns>
        public static bool ContainsFileNamePattern(string fileName)
        {
            int startingIndex = fileName.IndexOf("{#", StringComparison.Ordinal);
            int endingIndex = fileName.IndexOf("#}", StringComparison.Ordinal);

            return (startingIndex != -1 && endingIndex != -1 && startingIndex < endingIndex);
        }
    }

    sealed class FileNameTemplate
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
        public string Template { get { return this.template; } }

        /// <summary>
        /// The begging position of the <see cref="P:FileNameTemplate.Pattern"/> 
        /// within the <see cref="P:FileNameTemplate.Template"/>. -1 is returned 
        /// when no pattern can be found.
        /// </summary>
        public int BeginAt { get { return startIndex; } }

        /// <summary>
        /// The ending position of the <see cref="P:FileNameTemplate.Pattern"/> 
        /// within the <see cref="P:FileNameTemplate.Template"/>. -1 is returned 
        /// when no pattern can be found.
        /// </summary>
        public int EndAt { get { return endIndex; } }

        private bool FoundPattern { get { return startIndex != -1 && endIndex != -1; } }

        private readonly string template;
        private readonly int startIndex;
        private readonly int endIndex;

        public FileNameTemplate(string template)
        {
            this.template = template;
            this.startIndex = template.IndexOf(PatternStartCharacters, StringComparison.Ordinal);
            if (this.startIndex != -1)
                this.endIndex = template.IndexOf(PatternEndCharacters, StringComparison.Ordinal) + PatternEndCharacters.Length;
        }

        /// <summary>
        /// Replace the pattern with the specified String.
        /// </summary>
        /// <param name="replacementValue"></param>
        /// <returns></returns>
        public string ReplacePattern(string replacementValue)
        {
            return !FoundPattern || String.IsNullOrEmpty(replacementValue) ? this.Template : template.Substring(0, this.BeginAt) + replacementValue + template.Substring(this.EndAt);
        }
    }

    abstract class FileArchiveModeBase : IFileArchiveStyle
    {
        private string _lastArchiveFilePath;

        /// <summary>
        /// Check if cleanup should be performed on initialize new file
        /// 
        /// Skip cleanup when initializing new file, just after having performed archive operation
        /// </summary>
        /// <param name="archiveFilePath">Base archive file pattern</param>
        /// <returns>True, when archive cleanup is needed</returns>
        public bool AttemptCleanupOnInitializeFile(string archiveFilePath)
        {
            if (_lastArchiveFilePath == archiveFilePath)
            {
                // Skip archive cleanup, when it has just been executed
                _lastArchiveFilePath = string.Empty;
                return false;
            }
            else
            {
                _lastArchiveFilePath = string.Empty;
                return true;
            }
        }

        public string GenerateFileNameMask(string archiveFilePath)
        {
            return GenerateFileNameMask(archiveFilePath, GenerateFileNameTemplate(archiveFilePath));
        }

        public List<DateAndSequenceArchive> GetExistingArchiveFiles(string archiveFilePath)
        {
            string archiveFolderPath = Path.GetDirectoryName(archiveFilePath);
            FileNameTemplate archiveFileNameTemplate = GenerateFileNameTemplate(archiveFilePath);
            string archiveFileMask = GenerateFileNameMask(archiveFilePath, archiveFileNameTemplate);

            var existingArchiveFiles = new List<DateAndSequenceArchive>();
            if (string.IsNullOrEmpty(archiveFileMask))
                return existingArchiveFiles;

            DirectoryInfo directoryInfo = new DirectoryInfo(archiveFolderPath);
            if (!directoryInfo.Exists)
                return existingArchiveFiles;

#if SILVERLIGHT && !WINDOWS_PHONE
            var existingFiles = directoryInfo.EnumerateFiles(archiveFileMask);
#else
            var existingFiles = directoryInfo.GetFiles(archiveFileMask);
#endif
            foreach (var fileInfo in existingFiles)
            {
                var archiveFileInfo = GenerateArchiveFileInfo(fileInfo, archiveFileNameTemplate);
                if (archiveFileInfo != null)
                    existingArchiveFiles.Add(archiveFileInfo);
            }

            if (existingArchiveFiles.Count > 1)
                existingArchiveFiles.Sort((x, y) => (x.Date != y.Date && !x.HasSameFormattedDate(y.Date)) ? x.Date.CompareTo(y.Date) : x.Sequence.CompareTo(y.Sequence) != 0 ? x.Sequence.CompareTo(y.Sequence) : string.CompareOrdinal(x.FileName, y.FileName));
            return existingArchiveFiles;
        }

        protected virtual FileNameTemplate GenerateFileNameTemplate(string archiveFilePath)
        {
            return new FileNameTemplate(Path.GetFileName(archiveFilePath));
        }
        protected virtual string GenerateFileNameMask(string archiveFilePath, FileNameTemplate fileTemplate)
        {
            if (fileTemplate != null)
                return fileTemplate.ReplacePattern("*");
            else
                return string.Empty;
        }
        protected abstract DateAndSequenceArchive GenerateArchiveFileInfo(FileInfo archiveFile, FileNameTemplate fileTemplate);
        public abstract DateAndSequenceArchive GenerateArchiveFileName(string archiveFilePath, DateTime archiveDate, List<DateAndSequenceArchive> existingArchiveFiles);

        public virtual IEnumerable<DateAndSequenceArchive> CheckArchiveCleanup(string archiveFilePath, List<DateAndSequenceArchive> existingArchiveFiles, int maxArchiveFiles)
        {
            if (maxArchiveFiles <= 0)
                yield break;

            _lastArchiveFilePath = archiveFilePath; // Cache that we have just performed cleanup for this archive-path

            if (existingArchiveFiles.Count == 0 || existingArchiveFiles.Count < maxArchiveFiles)
                yield break;

            for (int i = 0; i < existingArchiveFiles.Count - maxArchiveFiles; ++i)
                yield return existingArchiveFiles[i];
        }
    }

    /// <summary>
    /// Archives the log-files using a rolling style numbering (the most recent is always #0 then
    /// #1, ..., #N. When the number of archive files exceed <see cref="P:MaxArchiveFiles"/> the obsolete archives
    /// are deleted.
    /// </summary>
    sealed class FileArchiveModeRolling : IFileArchiveStyle
    {
        public bool AttemptCleanupOnInitializeFile(string archiveFilePath)
        {
            return false;   // For historic reasons, then cleanup of rolling archives are not done on startup
        }

        public string GenerateFileNameMask(string archiveFilePath)
        {
            return new FileNameTemplate(Path.GetFileName(archiveFilePath)).ReplacePattern("*");
        }

        public List<DateAndSequenceArchive> GetExistingArchiveFiles(string archiveFilePath)
        {
            List<DateAndSequenceArchive> existingArchiveFiles = new List<DateAndSequenceArchive>();
            for (int archiveNumber = 0; archiveNumber < int.MaxValue; ++archiveNumber)
            {
                string existingFileName = ReplaceNumberPattern(archiveFilePath, archiveNumber);
                if (!File.Exists(existingFileName))
                    break;

                existingArchiveFiles.Add(new DateAndSequenceArchive(existingFileName, DateTime.MinValue, string.Empty, archiveNumber));
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
            return new DateAndSequenceArchive(newFileName, DateTime.MinValue, string.Empty, int.MinValue);
        }

        public IEnumerable<DateAndSequenceArchive> CheckArchiveCleanup(string archiveFilePath, List<DateAndSequenceArchive> existingArchiveFiles, int maxArchiveFiles)
        {
            if (existingArchiveFiles.Count <= 1)
                yield break;

            existingArchiveFiles.Sort((x, y) => x.Sequence.CompareTo(y.Sequence));

            if (maxArchiveFiles > 0)
            {
                if (existingArchiveFiles.Count > maxArchiveFiles)
                {
                    for (int i = 0; i < existingArchiveFiles.Count; ++i)
                    {
                        if (existingArchiveFiles[i].Sequence == int.MinValue || existingArchiveFiles[i].Sequence == int.MaxValue)
                            continue;

                        if ((i + 1) > maxArchiveFiles)
                        {
                            yield return existingArchiveFiles[i];
                        }
                    }
                }
            }

            // After deleting the last, then roll the others forward
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

    /// <summary>
    /// Archives the log-files using a date style numbering. Archives will be stamped with the
    /// prior period (Year, Month, Day, Hour, Minute) datetime. When the number of archive files exceed <see
    /// cref="P:MaxArchiveFiles"/> the obsolete archives are deleted.
    /// </summary>
    sealed class FileArchiveModeDate : FileArchiveModeBase
    {
        private readonly string _archiveDateFormat;

        public FileArchiveModeDate(string archiveDateFormat)
        {
            _archiveDateFormat = archiveDateFormat;
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
            return new DateAndSequenceArchive(archiveFilePath, archiveDate, _archiveDateFormat, 0);
        }
    }

    /// <summary>
    /// <para>
    /// Archives the log-files using a date and sequence style numbering. Archives will be stamped
    /// with the prior period (Year, Month, Day) datetime. The most recent archive has the highest number (in
    /// combination with the date).
    /// </para>
    /// <para>
    /// When the number of archive files exceed <see cref="P:MaxArchiveFiles"/> the obsolete archives are deleted.
    /// </para>
    /// </summary>
    sealed class FileArchiveModeDateAndSequence : FileArchiveModeBase
    {
        private readonly string _archiveDateFormat;

        public FileArchiveModeDateAndSequence(string archiveDateFormat)
        {
            _archiveDateFormat = archiveDateFormat;
        }

        protected override DateAndSequenceArchive GenerateArchiveFileInfo(FileInfo archiveFile, FileNameTemplate fileTemplate)
        {
            //Get the archive file name or empty string if it's null
            string archiveFileNameWithoutPath = Path.GetFileName(archiveFile.FullName) ?? "";

            DateTime date;
            int sequence;
            if (
                !TryParseDateAndSequence(archiveFileNameWithoutPath, _archiveDateFormat, fileTemplate, out date,
                    out sequence))
            {
                return null;
            }

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
                string.Format("{0}.{1}", archiveDate.ToString(_archiveDateFormat), paddedSequence));
            string dirName = Path.GetDirectoryName(archiveFilePath);
            archiveFilePath = Path.Combine(dirName, archiveFileNameWithoutPath);
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
            if (!Int32.TryParse(sequencePart, NumberStyles.None, CultureInfo.CurrentCulture, out sequence))
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

            return true;
        }
    }

    /// <summary>
    /// Archives the log-files using the provided base-archive-filename. If the base-archive-filename causes
    /// duplicate archive filenames, then sequence-style is automatically enforced.
    /// 
    /// Example: 
    ///     Base Filename     trace.log
    ///     Next Filename     trace.0.log
    /// 
    /// The most recent archive has the highest number. When the number of archive files
    /// exceed <see cref="P:MaxArchiveFiles"/> the obsolete archives are deleted.
    /// </summary>
    sealed class FileArchiveModeDynamicSequence : FileArchiveModeBase
    {
        private readonly ArchiveNumberingMode _archiveNumbering;
        private readonly string _archiveDateFormat;

        public FileArchiveModeDynamicSequence(ArchiveNumberingMode archiveNumbering, string archiveDateFormat)
        {
            _archiveNumbering = archiveNumbering;
            _archiveDateFormat = archiveDateFormat;
        }

        protected override FileNameTemplate GenerateFileNameTemplate(string archiveFilePath)
        {
            return null;
        }

        private static bool RemoveNonLetters(string fileName, int startPosition, StringBuilder sb, out int digitsRemoved)
        {
            digitsRemoved = 0;
            sb.ClearBuilder();

            for (int i = 0; i < startPosition; ++i)
            {
                sb.Append(fileName[i]);
            }

            bool? wildCardActive = null;
            for (int i = startPosition; i < fileName.Length; ++i)
            {
                char nameChar = fileName[i];
                if (char.IsDigit(nameChar))
                {
                    if (!wildCardActive.HasValue)
                    {
                        wildCardActive = true;
                        digitsRemoved = 1;
                        sb.Append('*');
                    }
                    else if (wildCardActive.Value == false)
                    {
                        sb.Append(nameChar);
                    }
                    else
                    {
                        ++digitsRemoved;
                    }
                }
                else if (!char.IsLetter(nameChar))
                {
                    if (!wildCardActive.HasValue || wildCardActive.Value == false)
                        sb.Append(nameChar);
                }
                else
                {
                    if (wildCardActive.HasValue)
                        wildCardActive = false;
                    sb.Append(nameChar);
                }
            }

            return wildCardActive.HasValue;
        }

        protected override string GenerateFileNameMask(string archiveFilePath, FileNameTemplate fileTemplate)
        {
            string currentFileName = Path.GetFileNameWithoutExtension(archiveFilePath);
            int digitsRemoved;

            // Find the most optimal location to place the wildcard-mask
            StringBuilder sb = new StringBuilder();
            int optimalStartPosition = 0;
            int optimalLength = int.MaxValue;
            for (int i = 0; i < currentFileName.Length; ++i)
            {
                if (!RemoveNonLetters(currentFileName, i, sb, out digitsRemoved) && i == 0)
                    break;

                if (digitsRemoved <= 1)
                    continue;

                if (sb.Length < optimalLength)
                {
                    optimalStartPosition = i;
                    optimalLength = sb.Length;
                }
            }

            RemoveNonLetters(currentFileName, optimalStartPosition, sb, out digitsRemoved);
            if (digitsRemoved <= 1)
            {
                sb.ClearBuilder();
                sb.Append(currentFileName);
            }

            switch (_archiveNumbering)
            {
                case ArchiveNumberingMode.Sequence:
                case ArchiveNumberingMode.Rolling:
                case ArchiveNumberingMode.DateAndSequence:
                    {
                        // Force sequence-number into template (Just before extension)
                        if (sb.Length > 0 && sb[sb.Length - 1] != '*')
                            sb.Append('*');
                    }
                    break;
            }
            sb.Append(Path.GetExtension(archiveFilePath));
            return sb.ToString();
        }

        protected override DateAndSequenceArchive GenerateArchiveFileInfo(FileInfo archiveFile, FileNameTemplate fileTemplate)
        {
            int sequenceNumber = ExtractArchiveNumberFromFileName(archiveFile.FullName);
            var creationTimeUtc = FileCharacteristicsHelper.ValidateFileCreationTime(archiveFile, (f) => f.GetCreationTimeUtc(), (f) => f.GetLastWriteTimeUtc()).Value;
            return new DateAndSequenceArchive(archiveFile.FullName, creationTimeUtc, string.Empty, sequenceNumber > 0 ? sequenceNumber : 0);
        }

        private static int ExtractArchiveNumberFromFileName(string archiveFileName)
        {
            archiveFileName = Path.GetFileName(archiveFileName);
            int lastDotIdx = archiveFileName.LastIndexOf('.');
            if (lastDotIdx == -1)
                return 0;

            int previousToLastDotIdx = archiveFileName.LastIndexOf('.', lastDotIdx - 1);
            string numberPart = previousToLastDotIdx == -1 ? archiveFileName.Substring(lastDotIdx + 1) : archiveFileName.Substring(previousToLastDotIdx + 1, lastDotIdx - previousToLastDotIdx - 1);

            int archiveNumber;
            return Int32.TryParse(numberPart, out archiveNumber) ? archiveNumber : 0;
        }

        public override DateAndSequenceArchive GenerateArchiveFileName(string archiveFilePath, DateTime archiveDate, List<DateAndSequenceArchive> existingArchiveFiles)
        {
            int nextSequenceNumber = 0;
            string initialFileName = Path.GetFileName(archiveFilePath);

            foreach (var existingFile in existingArchiveFiles)
            {
                string existingFileName = Path.GetFileName(existingFile.FileName);
                if (string.Equals(existingFileName, initialFileName, StringComparison.OrdinalIgnoreCase))
                {
                    nextSequenceNumber = Math.Max(nextSequenceNumber, existingFile.Sequence + 1);
                }
                else if (existingFile.Sequence > 0)
                {
                    string existingExtension = Path.GetExtension(existingFileName);
                    existingFileName = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(existingFileName)) + existingExtension;
                    if (string.Equals(existingFileName, initialFileName, StringComparison.OrdinalIgnoreCase))
                    {
                        nextSequenceNumber = Math.Max(nextSequenceNumber, existingFile.Sequence + 1);
                    }
                }
            }

            if (nextSequenceNumber > 0)
            {
                archiveFilePath = Path.Combine(Path.GetDirectoryName(archiveFilePath), string.Concat(Path.GetFileNameWithoutExtension(archiveFilePath), ".", nextSequenceNumber.ToString(CultureInfo.InvariantCulture), Path.GetExtension(archiveFilePath)));
            }
            return new DateAndSequenceArchive(archiveFilePath, archiveDate, _archiveDateFormat, nextSequenceNumber);
        }
    }
}
