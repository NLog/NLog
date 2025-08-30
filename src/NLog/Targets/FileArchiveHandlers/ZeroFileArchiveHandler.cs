//
// Copyright (c) 2004-2024 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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

namespace NLog.Targets.FileArchiveHandlers
{
    using System;
    using System.IO;
    using System.Linq;
    using NLog.Common;
    using NLog.Internal;

    /// <summary>
    /// Deletes/truncates the active logging-file when archive-roll-event is triggered
    /// </summary>
    internal sealed class ZeroFileArchiveHandler : BaseFileArchiveHandler, IFileArchiveHandler
    {
        public ZeroFileArchiveHandler(FileTarget fileTarget)
            :base(fileTarget)
        {
        }

        public int ArchiveBeforeOpenFile(string newFileName, LogEventInfo firstLogEvent, DateTime? previousFileLastModified, int newSequenceNumber)
        {
            var newFilePath = FileTarget.CleanFullFilePath(newFileName);

            bool initialFileOpen = newSequenceNumber == 0;
            if (DeleteOldArchiveFiles(newFilePath, initialFileOpen))
            {
                FixWindowsFileSystemTunneling(newFilePath);
            }
            return 0;   // No rolling of active file
        }

        private bool DeleteOldArchiveFiles(string newFilePath, bool initialFileOpen)
        {
            try
            {
                if (initialFileOpen && _fileTarget.DeleteOldFileOnStartup)
                {
                    var filePathWithoutExtension = Path.GetFileNameWithoutExtension(newFilePath);
                    if (filePathWithoutExtension.Any(chr => char.IsDigit(chr)))
                    {
                        // Delete with wildcard (also files from yesterday)
                        return DeleteOldFilesBeforeArchive(newFilePath, initialFileOpen);
                    }
                }

                // Wildcard not detected as required, so just delete/truncate the active file
                if (File.Exists(newFilePath))
                {
                    var archiveCleanupReason = (_fileTarget.MaxArchiveFiles < 0 && _fileTarget.ArchiveOldFileOnStartup) ? "ArchiveOldFileOnStartup=true" : $"MaxArchiveFiles={_fileTarget.MaxArchiveFiles}";
                    return DeleteOldArchiveFile(newFilePath, archiveCleanupReason);
                }
            }
            catch (Exception ex)
            {
                InternalLogger.Debug(ex, "{0}: Failed to archive file: '{1}'", _fileTarget, newFilePath);
                if (ex.MustBeRethrown(_fileTarget))
                    throw;
            }

            return false;
        }
    }
}
