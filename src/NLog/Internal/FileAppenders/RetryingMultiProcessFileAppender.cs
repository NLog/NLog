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

namespace NLog.Internal.FileAppenders
{
    using System;
    using System.IO;
    using System.Security;

    /// <summary>
    /// Multi-process and multi-host file appender which attempts
    /// to get exclusive write access and retries if it's not available.
    /// </summary>
    [SecuritySafeCritical]
    internal sealed class RetryingMultiProcessFileAppender : BaseMutexFileAppender
    {
        public static readonly IFileAppenderFactory TheFactory = new Factory();

        /// <summary>
        /// Initializes a new instance of the <see cref="RetryingMultiProcessFileAppender" /> class.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="parameters">The parameters.</param>
        public RetryingMultiProcessFileAppender(string fileName, ICreateFileParameters parameters) : base(fileName, parameters)
        {
        }

        /// <inheritdoc/>
        public override void Write(byte[] bytes, int offset, int count)
        {
            int overrideBufferSize = Math.Min((count / 4096 + 1) * 4096, CreateFileParameters.BufferSize);
            using (FileStream fileStream = CreateFileStream(false, overrideBufferSize))
            {
                fileStream.Write(bytes, offset, count);
            }
        }

        /// <inheritdoc/>
        public override void Flush()
        {
            // nothing to do
        }

        /// <inheritdoc/>
        public override void Close()
        {
            // nothing to do
        }

        /// <inheritdoc/>
        public override DateTime? GetFileCreationTimeUtc()
        {
            FileInfo fileInfo = new FileInfo(FileName);
            if (fileInfo.Exists)
            {
                return fileInfo.LookupValidFileCreationTimeUtc();
            }
            return null;
        }

        /// <inheritdoc/>
        public override long? GetFileLength()
        {
            FileInfo fileInfo = new FileInfo(FileName);
            if (fileInfo.Exists)
            {
                return fileInfo.Length;
            }
            return null;
        }

        /// <summary>
        /// Factory class.
        /// </summary>
        private sealed class Factory : IFileAppenderFactory
        {
            /// <inheritdoc/>
            BaseFileAppender IFileAppenderFactory.Open(string fileName, ICreateFileParameters parameters)
            {
                return new RetryingMultiProcessFileAppender(fileName, parameters);
            }
        }
    }
}
