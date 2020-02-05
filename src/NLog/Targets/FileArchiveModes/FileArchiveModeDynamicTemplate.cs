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
using System.IO;

namespace NLog.Targets.FileArchiveModes
{
    /// <summary>
    /// Dynamically converts a non-template archiveFilePath into a correct archiveFilePattern.
    /// Before called the original IFileArchiveMode, that has been wrapped by this
    /// </summary>
    internal sealed class FileArchiveModeDynamicTemplate : IFileArchiveMode
    {
        private readonly IFileArchiveMode _archiveHelper;

        public bool IsArchiveCleanupEnabled => _archiveHelper.IsArchiveCleanupEnabled;

        private static string CreateDynamicTemplate(string archiveFilePath)
        {
            string ext = Path.GetExtension(archiveFilePath);
            return Path.ChangeExtension(archiveFilePath, ".{#}" + ext);
        }

        public FileArchiveModeDynamicTemplate(IFileArchiveMode archiveHelper)
        {
            _archiveHelper = archiveHelper;
        }

        public bool AttemptCleanupOnInitializeFile(string archiveFilePath, int maxArchiveFiles, int maxArchiveDays)
        {
            return _archiveHelper.AttemptCleanupOnInitializeFile(archiveFilePath, maxArchiveFiles, maxArchiveDays);
        }

        public IEnumerable<DateAndSequenceArchive> CheckArchiveCleanup(string archiveFilePath, List<DateAndSequenceArchive> existingArchiveFiles, int maxArchiveFiles, int maxArchiveDays)
        {
            return _archiveHelper.CheckArchiveCleanup(CreateDynamicTemplate(archiveFilePath), existingArchiveFiles, maxArchiveFiles, maxArchiveDays);
        }

        public DateAndSequenceArchive GenerateArchiveFileName(string archiveFilePath, DateTime archiveDate, List<DateAndSequenceArchive> existingArchiveFiles)
        {
            return _archiveHelper.GenerateArchiveFileName(CreateDynamicTemplate(archiveFilePath), archiveDate, existingArchiveFiles);
        }

        public string GenerateFileNameMask(string archiveFilePath)
        {
            return _archiveHelper.GenerateFileNameMask(CreateDynamicTemplate(archiveFilePath));
        }

        public List<DateAndSequenceArchive> GetExistingArchiveFiles(string archiveFilePath)
        {
            return _archiveHelper.GetExistingArchiveFiles(CreateDynamicTemplate(archiveFilePath));
        }
    }
}
