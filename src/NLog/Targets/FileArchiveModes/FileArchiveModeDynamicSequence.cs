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
using System.Text;
using NLog.Common;
using NLog.Internal;

namespace NLog.Targets.FileArchiveModes
{
    /// <summary>
    /// Archives the log-files using the provided base-archive-filename. If the base-archive-filename causes
    /// duplicate archive filenames, then sequence-style is automatically enforced.
    /// 
    /// Example: 
    ///     Base Filename     trace.log
    ///     Next Filename     trace.0.log
    /// 
    /// The most recent archive has the highest number. 
    /// 
    /// When the number of archive files exceed <see cref="P:MaxArchiveFiles"/> the obsolete archives are deleted.
    /// When the age of archive files exceed <see cref="P:MaxArchiveDays"/> the obsolete archives are deleted.
    /// </summary>
    internal sealed class FileArchiveModeDynamicSequence : FileArchiveModeBase
    {
        private readonly ArchiveNumberingMode _archiveNumbering;
        private readonly string _archiveDateFormat;
        private readonly bool _customArchiveFileName;

        public FileArchiveModeDynamicSequence(ArchiveNumberingMode archiveNumbering, string archiveDateFormat, bool customArchiveFileName, bool archiveCleanupEnabled)
            :base(archiveCleanupEnabled)
        {
            _archiveNumbering = archiveNumbering;
            _archiveDateFormat = archiveDateFormat;
            _customArchiveFileName = customArchiveFileName;
        }

        private static bool RemoveNonLetters(string fileName, int startPosition, StringBuilder sb, out int digitsRemoved)
        {
            digitsRemoved = 0;
            sb.ClearBuilder();

            for (int i = 0; i < startPosition; i++)
            {
                sb.Append(fileName[i]);
            }

            bool? wildCardActive = null;
            for (int i = startPosition; i < fileName.Length; i++)
            {
                char nameChar = fileName[i];
                if (char.IsDigit(nameChar))
                {
                    if (!wildCardActive.HasValue)
                    {
                        wildCardActive = true;
                        digitsRemoved = 1;
                        sb.Append('{');
                        sb.Append('#');
                        sb.Append('}');
                    }
                    else if (!wildCardActive.Value)
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
                    if (!wildCardActive.HasValue || !wildCardActive.Value)
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

        protected override FileNameTemplate GenerateFileNameTemplate(string archiveFilePath)
        {
            string currentFileName = Path.GetFileNameWithoutExtension(archiveFilePath);
            int digitsRemoved;

            // Find the most optimal location to place the wildcard-mask
            StringBuilder sb = new StringBuilder();
            int optimalStartPosition = 0;
            int optimalLength = int.MaxValue;
            for (int i = 0; i < currentFileName.Length; i++)
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
                        if (sb.Length > 3 && sb[sb.Length - 3] != '{' && sb[sb.Length - 2] != '#' && sb[sb.Length - 1] != '}')
                        {
                            if (digitsRemoved <= 1)
                            {
                                sb.Append("{");
                                sb.Append("#");
                                sb.Append("}");
                            }
                            else
                            {
                                sb.Append('*');
                            }
                        }
                    }
                    break;
            }
            sb.Append(Path.GetExtension(archiveFilePath));
            return base.GenerateFileNameTemplate(sb.ToString());
        }

        protected override DateAndSequenceArchive GenerateArchiveFileInfo(FileInfo archiveFile, FileNameTemplate fileTemplate)
        {
            if (fileTemplate?.EndAt > 0)
            {
                string filename = archiveFile.Name;
                int templatePos = 0;
                for (int i = 0; i < filename.Length; ++i)
                {
                    char fileNameChar = filename[i];

                    if (templatePos >= fileTemplate.Template.Length)
                    {
                        if (char.IsLetter(fileNameChar))
                            return null;    // reached end of template, but still letters

                        break;
                    }

                    char templateChar;
                    if (templatePos < fileTemplate.EndAt && i >= fileTemplate.BeginAt)
                    {
                        // Inside wildcard, skip validation of non-letters
                        if (char.IsLetter(fileNameChar))
                        {
                            templatePos = fileTemplate.EndAt;
                            do
                            {
                                if (templatePos >= fileTemplate.Template.Length)
                                    return null;    // reached end of template, but still letters
                                templateChar = fileTemplate.Template[templatePos];
                                ++templatePos;
                            } while (!char.IsLetter(templateChar));
                        }
                        else
                        {
                            continue;
                        }
                    }
                    else
                    {
                        templateChar = fileTemplate.Template[templatePos];
                        ++templatePos;
                    }

                    if (fileNameChar == templateChar || char.ToLowerInvariant(fileNameChar) == char.ToLowerInvariant(templateChar))
                    {
                        continue;
                    }

                    if (templateChar == '*' && !char.IsLetter(fileNameChar))
                    {
                        break;  // Reached archive-seq-no, lets call it a day
                    }

                    return null; // filename is not matching file-template
                }
            }

            int sequenceNumber = ExtractArchiveNumberFromFileName(archiveFile.FullName);
            var creationTimeUtc = archiveFile.LookupValidFileCreationTimeUtc();
            var creationTime = creationTimeUtc > DateTime.MinValue ? NLog.Time.TimeSource.Current.FromSystemTime(creationTimeUtc) : DateTime.MinValue;
            return new DateAndSequenceArchive(archiveFile.FullName, creationTime, _archiveDateFormat, sequenceNumber > 0 ? sequenceNumber : 0);
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
            return int.TryParse(numberPart, out archiveNumber) ? archiveNumber : 0;
        }

        public override DateAndSequenceArchive GenerateArchiveFileName(string archiveFilePath, DateTime archiveDate, List<DateAndSequenceArchive> existingArchiveFiles)
        {
            int nextSequenceNumber = GetStartSequenceNo() - 1;
            string initialFileName = Path.GetFileName(archiveFilePath);

            foreach (var existingFile in existingArchiveFiles)
            {
                string existingFileName = Path.GetFileName(existingFile.FileName);
                if (string.Equals(existingFileName, initialFileName, StringComparison.OrdinalIgnoreCase))
                {
                    nextSequenceNumber = Math.Max(nextSequenceNumber, existingFile.Sequence + GetStartSequenceNo());
                }
                else
                {
                    string existingExtension = Path.GetExtension(existingFileName);
                    existingFileName = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(existingFileName)) + existingExtension;
                    if (string.Equals(existingFileName, initialFileName, StringComparison.OrdinalIgnoreCase))
                    {
                        nextSequenceNumber = Math.Max(nextSequenceNumber, existingFile.Sequence + 1);
                    }
                }
            }

            archiveFilePath = Path.GetFullPath(archiveFilePath);    // Rebuild to fix non-standard path-format
            if (nextSequenceNumber >= GetStartSequenceNo())
            {
                archiveFilePath = Path.Combine(Path.GetDirectoryName(archiveFilePath), string.Concat(Path.GetFileNameWithoutExtension(initialFileName), ".", nextSequenceNumber.ToString(CultureInfo.InvariantCulture), Path.GetExtension(initialFileName)));
            }

            return new DateAndSequenceArchive(archiveFilePath, archiveDate, _archiveDateFormat, nextSequenceNumber);
        }

        private int GetStartSequenceNo()
        {
            // For historic reasons, then the sequence-number starts at 1, when having specified FileTarget.ArchiveFileName
            return _customArchiveFileName ? 1 : 0;
        }
    }
}
