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

#if MONO

using System;
using System.Xml;
using System.IO;
using System.Threading;
using System.Text;
using System.Collections;
using System.Collections.Specialized;

using NLog;
using NLog.Config;
using NLog.Common;

using NLog.Internal;

using Mono.Unix;
using Mono.Unix.Native;

namespace NLog.Internal.FileAppenders
{
    /// <summary>
    /// Provides a multiprocess-safe atomic file appends while
    /// keeping the files open.
    /// </summary>
    /// <remarks>
    /// On Unix you can get all the appends to be atomic, even when multiple 
    /// processes are trying to write to the same file, because setting the file
    /// pointer to the end of the file and appending can be made one operation.
    /// </remarks>
    internal class UnixMultiProcessFileAppender : BaseFileAppender
    {
        private UnixStream file;

        public static readonly IFileAppenderFactory TheFactory = new Factory();

        public class Factory : IFileAppenderFactory
        {
            public BaseFileAppender Open(string fileName, ICreateFileParameters parameters)
            {
                return new UnixMultiProcessFileAppender(fileName, parameters);
            }
        }

        public UnixMultiProcessFileAppender(string fileName, ICreateFileParameters parameters) : base(fileName, parameters)
        {
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
                this.file = new UnixStream(fd, true);
            }
            catch
            {
                Syscall.close(fd);
                throw;
            }
        }

        public override void Write(byte[] bytes)
        {
            if (this.file == null)
                return;
            this.file.Write(bytes, 0, bytes.Length);
        }

        public override void Close()
        {
            if (this.file == null)
                return;
            InternalLogger.Trace("Closing '{0}'", FileName);
            this.file.Close();
            this.file = null;
        }

        public override DateTime? GetFileCreationTimeUtc()
        {
            FileInfo fileInfo = new FileInfo(FileName);
            if (!fileInfo.Exists)
                return null;
            return fileInfo.CreationTime;
        }

        public override DateTime? GetFileLastWriteTimeUtc()
        {
            FileInfo fileInfo = new FileInfo(FileName);
            if (!fileInfo.Exists)
                return null;
            return fileInfo.LastWriteTime;
        }

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
