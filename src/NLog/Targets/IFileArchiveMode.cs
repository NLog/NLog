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

namespace NLog.Targets
{
    using System;
    using System.Collections.Generic;

    interface IFileArchiveMode
    {
        bool IsArchiveCleanupEnabled { get; }

        /// <summary>
        /// Check if cleanup should be performed on initialize new file
        /// </summary>
        /// <param name="archiveFilePath">Base archive file pattern</param>
        /// <param name="maxArchiveFiles">Maximum number of archive files that should be kept</param>
        /// <param name="maxArchiveDays">Maximum days of archive files that should be kept</param>
        /// <returns>True, when archive cleanup is needed</returns>
        bool AttemptCleanupOnInitializeFile(string archiveFilePath, int maxArchiveFiles, int maxArchiveDays);

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
        /// <param name="maxArchiveDays">Maximum days of archive files that should be kept</param>
        IEnumerable<DateAndSequenceArchive> CheckArchiveCleanup(string archiveFilePath, List<DateAndSequenceArchive> existingArchiveFiles, int maxArchiveFiles, int maxArchiveDays);
    };
}
