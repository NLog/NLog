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

#if SupportsMutex

namespace NLog.Internal.FileAppenders
{
    using NLog.Common;
    using System;
    using System.IO;
    using System.Security;
    using System.Threading;

    /// <summary>
    /// Provides a multiprocess-safe atomic file appends while
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
    internal class MutexMultiProcessFileAppender : BaseFileAppender
    {
        public static readonly IFileAppenderFactory TheFactory = new Factory();

        private FileStream fileStream;
        private Mutex mutex;

        /// <summary>
        /// Initializes a new instance of the <see cref="MutexMultiProcessFileAppender" /> class.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="parameters">The parameters.</param>
        public MutexMultiProcessFileAppender(string fileName, ICreateFileParameters parameters) : base(fileName, parameters)
        {
            try
            {
                this.mutex = CreateSharableMutex("FileLock");
                this.fileStream = CreateFileStream(true);
            }
            catch
            {
                if (this.mutex != null)
                {
                    this.mutex.Close();
                    this.mutex = null;
                }

                if (this.fileStream != null)
                {
                    this.fileStream.Close();
                    this.fileStream = null;
                }

                throw;
            }
        }

        /// <summary>
        /// Writes the specified bytes.
        /// </summary>
        /// <param name="bytes">The bytes to be written.</param>
        public override void Write(byte[] bytes)
        {
            if (this.mutex == null)
            {
                return;
            }

            try
            {
                this.mutex.WaitOne();
            }
            catch (AbandonedMutexException)
            {
                // ignore the exception, another process was killed without properly releasing the mutex
                // the mutex has been acquired, so proceed to writing
                // See: http://msdn.microsoft.com/en-us/library/system.threading.abandonedmutexexception.aspx
            }

            try
            {
                this.fileStream.Seek(0, SeekOrigin.End);
                this.fileStream.Write(bytes, 0, bytes.Length);
                this.fileStream.Flush();
                if (CaptureLastWriteTime)
                {
                    FileTouched();
                }
            }
            finally
            {
                this.mutex.ReleaseMutex();
            }
        }

        /// <summary>
        /// Closes this instance.
        /// </summary>
        public override void Close()
        {
            InternalLogger.Trace("Closing '{0}'", FileName);
            if (this.mutex != null)
            {
                this.mutex.Close();
            }

            if (this.fileStream != null)
            {
                this.fileStream.Close();
            }

            this.mutex = null;
            this.fileStream = null;
            FileTouched();
        }

        /// <summary>
        /// Flushes this instance.
        /// </summary>
        public override void Flush()
        {
            // do nothing, the stream is always flushed
        }


        public override DateTime? GetFileCreationTimeUtc()
        {
           
            var fileChars = GetFileCharacteristics();
            return fileChars.CreationTimeUtc;
        }

        public override DateTime? GetFileLastWriteTimeUtc()
        {
            var fileChars = GetFileCharacteristics();
            return fileChars.LastWriteTimeUtc;
        }

        public override long? GetFileLength()
        {
            var fileChars = GetFileCharacteristics();
            return fileChars.FileLength;
        }

        private FileCharacteristics GetFileCharacteristics()
        {
            //todo not efficient to read all the whole FileCharacteristics and then using one property
            return FileCharacteristicsHelper.Helper.GetFileCharacteristics(FileName, this.fileStream.SafeFileHandle.DangerousGetHandle());
        }

        /// <summary>
        /// Creates a mutually-exclusive lock for archiving files.
        /// </summary>
        /// <returns>A <see cref="Mutex"/> object which can be used for controlling the archiving of files.</returns>
        protected override Mutex CreateArchiveMutex()
        {
            return CreateSharableArchiveMutex();
        }

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
                return new MutexMultiProcessFileAppender(fileName, parameters);
            }
        }
    }
}

#endif
