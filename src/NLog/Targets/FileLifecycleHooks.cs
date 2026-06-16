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

using System.IO;

namespace NLog.Targets
{
    /// <summary>
    ///     Enables hooking into log file lifecycle events.
    ///     Hooks run synchronously and therefore may affect responsiveness of the application if long operations are
    ///     performed.
    /// </summary>
    public abstract class FileLifecycleHooks
    {
        /// <summary>
        ///        Called after an open file has been closed.
        /// </summary>
        /// <remarks>
        ///     Note that<see cref="OnFileClosed" /> is not guaranteed to be invoked before <see cref = "FileTarget" /> advances to the next file.
        ///     You may override <see cref="OnTargetClose" /> to determine whether a file-closed event was caused by <see cref="FileTarget" /> shutting down or by <see cref="FileTarget" /> switching to a different file.
        ///     <see cref="FileTarget"/> may close files in the following scenarios:
        ///     - <see cref="FileTarget"/> archives the current file because of an archive constraint and switches to the next file.
        ///     - An open file is deleted from storage and no longer exists.
        ///     - If <see cref="FileTarget.DeleteOldFileOnStartup"/> is enabled, <see cref="FileTarget"/> may close open files.
        ///     - <see cref="FileTarget"/> closes all open files when the target itself is shutting down.
        ///     - If <see cref="FileTarget.OpenFileMonitorTimerInterval"/> is greater than 0, <see cref="FileTarget"/> may close files that have not received log events for a period of time.
        /// </remarks>
        /// <param name="filePath">The full path to the file being closed.</param>
        public virtual void OnFileClosed(string filePath)
        {
        }

        /// <summary>
        ///     Invoked immediately before a log file is deleted.
        /// </summary>
        /// <remarks>
        ///     Use this callback to perform custom actions before a log file is removed, such as copying it to an archive location or uploading it to a backup server.
        ///     The timing of file deletion depends on the archive retention settings. Use <see cref="FileTarget.MaxArchiveDays" /> and <see cref="FileTarget.MaxArchiveFiles" /> to control when archived log files become eligible for deletion.
        /// </remarks>
        /// <param name="filePath">The full path of the log file that is about to be deleted.</param>
        public virtual void OnFileDeleting(string filePath)
        {
        }

        /// <summary>
        ///     Initialize or wrap the <paramref name="underlyingStream" /> opened on the log file. Wrap the stream in another that adds buffering, compression, encryption, etc.
        ///     The underlying file may or may not be empty when this method is called.
        /// </summary>
        /// <param name="filePath">The full path to the log file.</param>
        /// <param name="underlyingStream">The underlying <see cref="Stream" /> opened on the log file.</param>
        /// <returns>The <see cref="Stream" /> NLog should use when writing events to the log file.</returns>
        public virtual Stream OnFileOpened(string filePath, Stream underlyingStream)
            => underlyingStream;

        /// <summary>
        ///     Called when the target is being closed.
        /// </summary>
        /// <param name="target">Closing target.</param>
        public virtual void OnTargetClose(FileTarget target)
        {
        }

        /// <summary>
        ///     Called when the target gets initialized.
        /// </summary>
        /// <param name="target">Initialized target.</param>
        public virtual void OnTargetInitialize(FileTarget target)
        {
        }
    }
}
