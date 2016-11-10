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

#if !SILVERLIGHT && !__ANDROID__ && !__IOS__
// Unfortunately, Xamarin Android and Xamarin iOS don't support mutexes (see https://github.com/mono/mono/blob/3a9e18e5405b5772be88bfc45739d6a350560111/mcs/class/corlib/System.Threading/Mutex.cs#L167) so the BaseFileAppender class now throws an exception in the constructor.
#define SupportsMutex
#endif

using System.Security;

namespace NLog.Internal.FileAppenders
{
    using System;
    using System.IO;
    using System.Threading;

    /// <summary>
    /// Multi-process and multi-host file appender which attempts
    /// to get exclusive write access and retries if it's not available.
    /// </summary>
    [SecuritySafeCritical]
    internal class RetryingMultiProcessFileAppender : BaseFileAppender
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

        /// <summary>
        /// Writes the specified bytes.
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        public override void Write(byte[] bytes)
        {
            using (FileStream fileStream = CreateFileStream(false))
            {
                fileStream.Write(bytes, 0, bytes.Length);
            }

            if (CaptureLastWriteTime)
            {
                FileTouched();
            }
        }

        /// <summary>
        /// Flushes this instance.
        /// </summary>
        public override void Flush()
        {
            // nothing to do
        }

        /// <summary>
        /// Closes this instance.
        /// </summary>
        public override void Close()
        {
            // nothing to do
        }


        public override DateTime? GetFileCreationTimeUtc()
        {
            FileInfo fileInfo = new FileInfo(FileName);
            if (fileInfo.Exists)
            {
                return fileInfo.GetCreationTimeUtc();
            }
            return null;
        }

        public override DateTime? GetFileLastWriteTimeUtc()
        {
            FileInfo fileInfo = new FileInfo(FileName);
            if (fileInfo.Exists)
            {
                return fileInfo.GetLastWriteTimeUtc();
            }
            return null;
        }

        public override long? GetFileLength()
        {
            FileInfo fileInfo = new FileInfo(FileName);
            if (fileInfo.Exists)
            {
                return fileInfo.Length;
            }
            return null;
        }

#if SupportsMutex
        /// <summary>
        /// Creates a mutually-exclusive lock for archiving files.
        /// </summary>
        /// <returns>A <see cref="Mutex"/> object which can be used for controlling the archiving of files.</returns>
        protected override Mutex CreateArchiveMutex()
        {
            return CreateSharableArchiveMutex();
        }
#endif

        /// <summary>
        /// Factory class.
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
                return new RetryingMultiProcessFileAppender(fileName, parameters);
            }
        }
    }
}
