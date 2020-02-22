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

#if MONO

namespace NLog.Internal.FileAppenders
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.IO;
    using System.Security;
    using System.Text;
    using System.Threading;
    using System.Xml;

    using NLog;
    using NLog.Common;
    using NLog.Config;
    using NLog.Internal;

    using Mono.Unix;
    using Mono.Unix.Native;

    /// <summary>
    /// Provides a multiprocess-safe atomic file appends while
    /// keeping the files open.
    /// </summary>
    /// <remarks>
    /// On Unix you can get all the appends to be atomic, even when multiple 
    /// processes are trying to write to the same file, because setting the file
    /// pointer to the end of the file and appending can be made one operation.
    /// </remarks>
    [SecuritySafeCritical]
    internal class UnixMultiProcessFileAppender : BaseMutexFileAppender
    {
        private UnixStream _file;

        public static readonly IFileAppenderFactory TheFactory = new Factory();

        private class Factory : IFileAppenderFactory
        {
            BaseFileAppender IFileAppenderFactory.Open(string fileName, ICreateFileParameters parameters)
            {
                return new UnixMultiProcessFileAppender(fileName, parameters);
            }
        }

        public UnixMultiProcessFileAppender(string fileName, ICreateFileParameters parameters) : base(fileName, parameters)
        {
            UnixFileInfo fileInfo = null;
            bool fileExists = false;
            try
            {
                fileInfo = new UnixFileInfo(fileName);
                fileExists = fileInfo.Exists;
            }
            catch
            {
            }

            int fd = Syscall.open(fileName, OpenFlags.O_CREAT | OpenFlags.O_WRONLY | OpenFlags.O_APPEND, (FilePermissions)(6 | (6 << 3) | (6 << 6)));
            if (fd == -1)
            {
                if (Stdlib.GetLastError() == Errno.ENOENT && parameters.CreateDirs)
                {
                    string dirName = Path.GetDirectoryName(fileName);
                    if (!Directory.Exists(dirName) && parameters.CreateDirs)
                        Directory.CreateDirectory(dirName);
                    
                    fd = Syscall.open(fileName, OpenFlags.O_CREAT | OpenFlags.O_WRONLY | OpenFlags.O_APPEND, (FilePermissions)(6 | (6 << 3) | (6 << 6)));
                }
            }
            if (fd == -1)
                UnixMarshal.ThrowExceptionForLastError();

            try
            {
                _file = new UnixStream(fd, true);

                long filePosition = _file.Position;
                if (fileExists || filePosition > 0)
                {
                    if (fileInfo != null)
                    {
                        CreationTimeUtc = FileInfoHelper.LookupValidFileCreationTimeUtc(fileInfo, (f) => File.GetCreationTimeUtc(f.FullName), (f) => { f.Refresh(); return f.LastStatusChangeTimeUtc; }, (f) => DateTime.UtcNow).Value;
                    }
                    else
                    {
                        CreationTimeUtc = FileInfoHelper.LookupValidFileCreationTimeUtc(fileName, (f) => File.GetCreationTimeUtc(f), (f) => DateTime.UtcNow).Value;
                    }
                }
                else
                {
                    // We actually created the file and eventually concurrent processes 
                    CreationTimeUtc = DateTime.UtcNow;
                    File.SetCreationTimeUtc(FileName, CreationTimeUtc);
                }
            }
            catch
            {
                Syscall.close(fd);
                throw;
            }
        }

        /// <summary>
        /// Writes the specified bytes.
        /// </summary>
        /// <param name="bytes">The bytes array.</param>
        /// <param name="offset">The bytes array offset.</param>
        /// <param name="count">The number of bytes.</param>
        public override void Write(byte[] bytes, int offset, int count)
        {
            if (_file == null)
                return;
            _file.Write(bytes, offset, count);
        }

        /// <summary>
        /// Closes this instance.
        /// </summary>
        public override void Close()
        {
            if (_file == null)
                return;

            InternalLogger.Trace("Closing '{0}'", FileName);
            try
            {
                _file?.Close();
            }
            catch (Exception ex)
            {
                // Swallow exception as the file-stream now is in final state (broken instead of closed)
                InternalLogger.Warn(ex, "Failed to close file '{0}'", FileName);
                System.Threading.Thread.Sleep(1);   // Artificial delay to avoid hammering a bad file location
            }
            finally
            {
                _file = null;
            }
        }
        
        /// <summary>
        /// Gets the creation time for a file associated with the appender. The time returned is in Coordinated Universal 
        /// Time [UTC] standard.
        /// </summary>
        /// <returns>The file creation time.</returns>
        public override DateTime? GetFileCreationTimeUtc()
        {
            return CreationTimeUtc;    // File is kept open, so creation time is static
        }
        
        /// <summary>
        /// Gets the length in bytes of the file associated with the appender.
        /// </summary>
        /// <returns>A long value representing the length of the file in bytes.</returns>
        public override long? GetFileLength()
        {
            FileInfo fileInfo = new FileInfo(FileName);
            if (!fileInfo.Exists)
                return null;
            return fileInfo.Length;
        }

        public override void Flush()
        {
            // do nothing, the stream is always flushed
        }
    }
}

#endif
