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

using System;
using System.Security;

namespace NLog.Internal.FileAppenders
{
    using System.IO;

    /// <summary>
    /// Implementation of <see cref="BaseFileAppender"/> which caches 
    /// file information.
    /// </summary>
    [SecuritySafeCritical]
    internal class CountingSingleProcessFileAppender : BaseFileAppender
    {
        public static readonly IFileAppenderFactory TheFactory = new Factory();

        private FileStream file;

        private long currentFileLength;

        /// <summary>
        /// Initializes a new instance of the <see cref="CountingSingleProcessFileAppender" /> class.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="parameters">The parameters.</param>
        public CountingSingleProcessFileAppender(string fileName, ICreateFileParameters parameters)
            : base(fileName, parameters)
        {
            var fileInfo = new FileInfo(fileName);
            if (fileInfo.Exists)
            {
                if (CaptureLastWriteTime)
                {
                    FileTouched(fileInfo.GetLastWriteTimeUtc());
                }
                this.currentFileLength = fileInfo.Length;
            }
            else
            {
                FileTouched();
                this.currentFileLength = 0;
            }

            this.file = this.CreateFileStream(false);
        }

        /// <summary>
        /// Closes this instance of the appender.
        /// </summary>
        public override void Close()
        {
            if (this.file != null)
            {
                this.file.Close();
                this.file = null;
            }
        }

        /// <summary>
        /// Flushes this current appender.
        /// </summary>
        public override void Flush()
        {
            if (this.file == null)
            {
                return;
            }

            this.file.Flush();
            FileTouched();
        }

        public override DateTime? GetFileCreationTimeUtc()
        {
            return this.CreationTime;
        }

        public override DateTime? GetFileLastWriteTimeUtc()
        {
            return this.LastWriteTime;
        }

        public override long? GetFileLength()
        {
            return this.currentFileLength;
        }

        /// <summary>
        /// Writes the specified bytes to a file.
        /// </summary>
        /// <param name="bytes">The bytes to be written.</param>
        public override void Write(byte[] bytes)
        {
            if (this.file == null)
            {
                return;
            }

            this.currentFileLength += bytes.Length;
            this.file.Write(bytes, 0, bytes.Length);
            if (CaptureLastWriteTime)
            {
                FileTouched();
            }
        }

        /// <summary>
        /// Factory class which creates <see cref="CountingSingleProcessFileAppender"/> objects.
        /// </summary>
        private class Factory : IFileAppenderFactory
        {
            /// <summary>
            /// Opens the appender for given file name and parameters.
            /// </summary>
            /// <param name="fileName">Name of the file.</param>
            /// <param name="parameters">Creation parameters.</param>
            /// <returns>
            /// Instance of <see cref="BaseFileAppender"/> which can be used to write to the file.
            /// </returns>
            BaseFileAppender IFileAppenderFactory.Open(string fileName, ICreateFileParameters parameters)
            {
                return new CountingSingleProcessFileAppender(fileName, parameters);
            }
        }
    }
}