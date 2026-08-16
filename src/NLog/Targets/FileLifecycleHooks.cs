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

namespace NLog.Targets
{
    using System.IO;

    /// <summary>
    ///     Enables hooking into file and <see cref="FileTarget"/> lifecycle events.
    ///     Hooks are invoked synchronously and therefore may affect application responsiveness
    ///     if they perform long-running operations.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         A <see cref="FileTarget"/> can have multiple log files open simultaneously.
    ///         File lifecycle callbacks therefore apply to the individual file identified by
    ///         <c>filePath</c> and must not assume that there is only one active file.
    ///     </para>
    ///     <para>
    ///         The same file may be opened and closed multiple times during the lifetime of a <see cref="FileTarget"/>.
    ///     </para>
    /// </remarks>
    public abstract class FileLifecycleHooks
    {
        /// <summary>
        ///     Initializes or wraps the stream opened for a log file.
        ///     This can be used to write file headers, or wrap the stream in another stream
        ///     that adds buffering, compression, encryption, etc.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         The underlying file may or may not be empty when this method is called.
        ///     </para>
        ///     <para>
        ///         NLog takes ownership of the returned stream and disposes it when the file
        ///         is closed. If the returned stream wraps <paramref name="underlyingStream"/>,
        ///         the returned stream is responsible for disposing the underlying stream.
        ///     </para>
        /// </remarks>
        /// <param name="filePath">The full path to the log file.</param>
        /// <param name="underlyingStream">The underlying <see cref="Stream"/> opened on the log file.</param>
        /// <returns>The <see cref="Stream"/> NLog should use when writing events to the log file.</returns>
        public virtual Stream OnFileOpened(string filePath, Stream underlyingStream)
            => underlyingStream;

        /// <summary>
        ///     Called after a log file has been closed by the <see cref="FileTarget"/>.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         This is a per-file event. A <see cref="FileTarget"/> can have multiple
        ///         files open simultaneously, and this method is invoked independently for
        ///         each file when that file is closed.
        ///     </para>
        ///     <para>
        ///         The same file may be opened and closed multiple times during the lifetime of the target.
        ///     </para>
        ///     <para>
        ///         A file can be closed because of archiving, file cleanup, file cache
        ///         management, or other <see cref="FileTarget"/> operations.
        ///     </para>
        /// </remarks>
        /// <param name="filePath">The full path to the file that was closed.</param>
        public virtual void OnFileClosed(string filePath)
        {
        }

        /// <summary>
        ///     Called immediately before a log file is deleted by the <see cref="FileTarget"/>.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         This is a per-file event and can be used to perform custom actions before
        ///         NLog deletes the file as part of its file cleanup logic, for example when
        ///         <see cref="FileTarget.MaxArchiveFiles"/> or <see cref="FileTarget.MaxArchiveDays"/>
        ///         triggers archive cleanup.
        ///     </para>
        ///     <para>
        ///         It is possible to move, rename, or delete the file in this method, as
        ///         <see cref="FileTarget"/> silently ignores that the file is already gone.
        ///     </para>
        /// </remarks>
        /// <param name="filePath">The full path to the file that is about to be deleted.</param>
        public virtual void OnFileDeleting(string filePath)
        {
        }

        /// <summary>
        ///     Called when the <see cref="FileTarget"/> is initialized.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         This callback is invoked once for each initialization of the target.
        ///     </para>
        ///     <para>
        ///         The <paramref name="target"/> can be used to inspect the target configuration
        ///         when implementing custom initialization logic.
        ///     </para>
        ///     <para>
        ///         If this method throws an exception, initialization of the
        ///         <see cref="FileTarget"/> may fail and the target may be disabled for output.
        ///     </para>
        /// </remarks>
        /// <param name="target">The <see cref="FileTarget"/> being initialized.</param>
        public virtual void OnTargetInitialize(FileTarget target)
        {
        }

        /// <summary>
        ///     Called when the <see cref="FileTarget"/> is being closed.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         This callback represents the lifecycle of the target itself rather than
        ///         the lifecycle of an individual file.
        ///     </para>
        /// </remarks>
        /// <param name="target">The <see cref="FileTarget"/> that is being closed.</param>
        public virtual void OnTargetClose(FileTarget target)
        {
        }
    }
}
