// 
// Copyright (c) 2004-2021 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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

#if !NETSTANDARD1_3
#define SupportsMutex
#endif

#if SupportsMutex

namespace NLog.Internal.FileAppenders
{
    using System;
    using System.IO;
    using System.Security;
    using System.Threading;
    using NLog.Common;

    /// <summary>
    /// Provides a multi process-safe atomic file appends while
    /// keeping the files open.
    /// </summary>
    /// <remarks>
    /// On Unix you can get all the appends to be atomic, even when multiple 
    /// processes are trying to write to the same file, because setting the file
    /// pointer to the end of the file and appending can be made one operation.
    /// On Win32 we need to maintain some synchronization between processes
    /// (global named mutex is used for this)
    /// </remarks>
    [SecuritySafeCritical]
    internal class MutexMultiProcessFileAppender : BaseMutexFileAppender
    {
        public static readonly IFileAppenderFactory TheFactory = new Factory();

        private FileStream _fileStream;
        private Mutex _mutex;

        /// <summary>
        /// Initializes a new instance of the <see cref="MutexMultiProcessFileAppender" /> class.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="parameters">The parameters.</param>
        public MutexMultiProcessFileAppender(string fileName, ICreateFileParameters parameters) : base(fileName, parameters)
        {
            try
            {
                _mutex = CreateSharableMutex("FileLock");
                _fileStream = CreateFileStream(true);
            }
            catch
            {
                _mutex?.Close();
                _mutex = null;

                _fileStream?.Close();
                _fileStream = null;

                throw;
            }
        }

        /// <inheritdoc/>
        public override void Write(byte[] bytes, int offset, int count)
        {
            if (_mutex is null || _fileStream is null)
            {
                return;
            }

            try
            {
                _mutex.WaitOne();
            }
            catch (AbandonedMutexException)
            {
                // ignore the exception, another process was killed without properly releasing the mutex
                // the mutex has been acquired, so proceed to writing
                // See: https://msdn.microsoft.com/en-us/library/system.threading.abandonedmutexexception.aspx
            }

            try
            {
                _fileStream.Seek(0, SeekOrigin.End);
                _fileStream.Write(bytes, offset, count);
                _fileStream.Flush();
            }
            finally
            {
                _mutex.ReleaseMutex();
            }
        }

        /// <inheritdoc/>
        public override void Close()
        {
            CloseFileSafe(ref _fileStream, FileName);

            try
            {
                _mutex?.Close();
            }
            catch (Exception ex)
            {
                // Swallow exception as the mutex now is in final state (abandoned instead of closed)
                InternalLogger.Warn(ex, "{0}: Failed to close mutex: '{1}'", CreateFileParameters, FileName);
            }
            finally
            {
                _mutex = null;
            }
        }

        /// <inheritdoc/>
        public override void Flush()
        {
            // do nothing, the stream is always flushed
        }

        /// <inheritdoc/>
        public override DateTime? GetFileCreationTimeUtc()
        {
            return CreationTimeUtc; // File is kept open, so creation time is static
        }

        /// <inheritdoc/>
        public override long? GetFileLength()
        {
            return _fileStream?.Length;
        }

        /// <summary>
        /// Factory class.
        /// </summary>
        private sealed class Factory : IFileAppenderFactory
        {
            /// <inheritdoc/>
            BaseFileAppender IFileAppenderFactory.Open(string fileName, ICreateFileParameters parameters)
            {
                return new MutexMultiProcessFileAppender(fileName, parameters);
            }
        }
    }
}

#endif
