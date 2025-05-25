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
    using System;
    using System.IO;
    using NLog.Common;

    /// <summary>
    /// Extended standard FileTarget with atomic file append for multi-process logging to the same file
    /// </summary>
    /// <remarks>
    /// <a href="https://github.com/nlog/nlog/wiki/AtomicFile-target">See NLog Wiki</a>
    /// </remarks>
    /// <seealso href="https://github.com/nlog/nlog/wiki/AtomicFile-target">Documentation on NLog Wiki</seealso>
    [Target("AtomFile")]
    [Target("AtomicFile")]
    public class AtomicFileTarget : FileTarget
    {
        /// <summary>
        /// Gets or sets a value indicating whether concurrent writes to the log file by multiple processes on the same host.
        /// </summary>
        public bool ConcurrentWrites { get; set; } = true;

        /// <inheritdoc />
        protected override Stream CreateFileStream(string filePath, int bufferSize)
        {
            if (!ConcurrentWrites || !KeepFileOpen || ReplaceFileContentsOnEachWrite)
                return base.CreateFileStream(filePath, bufferSize);

            const int maxRetryCount = 5;
            for (int i = 1; i <= maxRetryCount; ++i)
            {
                try
                {
                    return CreateAtomicFileStream(filePath);
                }
                catch (DirectoryNotFoundException)
                {
                    throw;
                }
                catch (IOException ex)
                {
                    InternalLogger.Debug(ex, "{0}: Failed opening file: {1}", this, filePath);
                    if (i == maxRetryCount)
                        throw;
                }
                catch (Exception ex)
                {
                    InternalLogger.Debug(ex, "{0}: Failed opening file: {1}", this, filePath);
                    if (i > 1)
                        throw;
                }
            }

            throw new InvalidOperationException("Should not be reached.");
        }

        private Stream CreateAtomicFileStream(string filePath)
        {
            var fileShare = FileShare.ReadWrite;
            if (EnableFileDelete)
                fileShare |= FileShare.Delete;

#if NETFRAMEWORK
            // https://blogs.msdn.microsoft.com/oldnewthing/20151127-00/?p=92211/
            // https://msdn.microsoft.com/en-us/library/ff548289.aspx
            // If only the FILE_APPEND_DATA and SYNCHRONIZE flags are set, the caller can write only to the end of the file,
            // and any offset information about writes to the file is ignored.
            // However, the file will automatically be extended as necessary for this type of write operation.
            return new FileStream(
                filePath,
                FileMode.Append,
                System.Security.AccessControl.FileSystemRights.AppendData | System.Security.AccessControl.FileSystemRights.Synchronize, // <- Atomic append
                fileShare,
                bufferSize: 1,  // No internal buffer, write directly from user-buffer
                FileOptions.None);
#else
            if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
            {
                var systemRights = System.Security.AccessControl.FileSystemRights.AppendData | System.Security.AccessControl.FileSystemRights.Synchronize;
                return FileSystemAclExtensions.Create(
                    new FileInfo(filePath),
                    FileMode.Append,
                    systemRights,
                    fileShare,
                    bufferSize: 1,    // No internal buffer, write directly from user-buffer
                    FileOptions.None,
                    fileSecurity: null);
            }
            else
            {
                return CreateUnixStream(filePath);
            }
#endif
        }

#if !NETFRAMEWORK
        private Stream CreateUnixStream(string filePath)
        {
            // Use 0666 (read/write for all)
            var permissions = (Mono.Unix.Native.FilePermissions)(6 | (6 << 3) | (6 << 6));
            var openFlags = Mono.Unix.Native.OpenFlags.O_CREAT | Mono.Unix.Native.OpenFlags.O_WRONLY | Mono.Unix.Native.OpenFlags.O_APPEND;

            int fd = Mono.Unix.Native.Syscall.open(filePath, openFlags, permissions);
            if (fd == -1 && Mono.Unix.Native.Stdlib.GetLastError() == Mono.Unix.Native.Errno.ENOENT && CreateDirs)
            {
                var dirName = Path.GetDirectoryName(filePath);
                if (dirName != null && !Directory.Exists(dirName))
                    Directory.CreateDirectory(dirName);

                fd = Mono.Unix.Native.Syscall.open(filePath, openFlags, permissions);
            }
            if (fd == -1)
                Mono.Unix.UnixMarshal.ThrowExceptionForLastError();

            try
            {
                return new Mono.Unix.UnixStream(fd, true);
            }
            catch
            {
                Mono.Unix.Native.Syscall.close(fd);
                throw;
            }
        }
#endif
    }
}
