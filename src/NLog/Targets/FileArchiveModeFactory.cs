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
    using System;

    static class FileArchiveModeFactory
    {
        public static IFileArchiveMode CreateArchiveStyle(string archiveFilePath, ArchiveNumberingMode archiveNumbering, string dateFormat, bool customArchiveFileName, int maxArchiveFiles)
        {
            if (ContainsFileNamePattern(archiveFilePath))
            {
                IFileArchiveMode archiveHelper = CreateStrictFileArchiveMode(archiveNumbering, dateFormat, maxArchiveFiles);
                if (archiveHelper != null)
                    return archiveHelper;
            }

            if (archiveNumbering != ArchiveNumberingMode.Sequence && !customArchiveFileName)
            {
                IFileArchiveMode archiveHelper = CreateStrictFileArchiveMode(archiveNumbering, dateFormat, maxArchiveFiles);
                if (archiveHelper != null)
                    return new FileArchiveModeDynamicTemplate(archiveHelper);
            }

            return new FileArchiveModeDynamicSequence(archiveNumbering, dateFormat, customArchiveFileName);
        }

        private static IFileArchiveMode CreateStrictFileArchiveMode(ArchiveNumberingMode archiveNumbering, string dateFormat, int maxArchiveFiles)
        {
            switch (archiveNumbering)
            {
                case ArchiveNumberingMode.Rolling: return new FileArchiveModeRolling();
                case ArchiveNumberingMode.Sequence: return new FileArchiveModeSequence(dateFormat);
                case ArchiveNumberingMode.Date: return new FileArchiveModeDate(dateFormat, ShouldDeleteOldArchives(maxArchiveFiles));
                case ArchiveNumberingMode.DateAndSequence: return new FileArchiveModeDateAndSequence(dateFormat);
            }

            return null;
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

        /// <summary>
        /// Determine if old archive files should be deleted.
        /// </summary>
        /// <param name="maxArchiveFiles">Maximum number of archive files that should be kept</param>
        /// <returns><see langword="true"/> when old archives should be deleted; <see langword="false"/> otherwise.</returns>
        public static bool ShouldDeleteOldArchives(int maxArchiveFiles)
        {
            return maxArchiveFiles > 0;
        }
    }
}
