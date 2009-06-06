// 
// Copyright (c) 2004-2009 Jaroslaw Kowalski <jaak@jkowalski.net>
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
using System.IO;
using System.Runtime.InteropServices;

using NLog.Config;

#if !NET_CF && !SILVERLIGHT
using NLog.Internal.Win32;
#endif

namespace NLog.Internal.FileAppenders
{
    /// <summary>
    /// Base class for optimized file appenders.
    /// </summary>
    internal abstract class BaseFileAppender
    {
        private readonly Random random = new Random();
        private readonly string fileName;
        private readonly ICreateFileParameters createParameters;
        private readonly DateTime openTime;
        private DateTime lastWriteTime;

        /// <summary>
        /// Initializes a new instance of the BaseFileAppender class.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="createParameters">The create parameters.</param>
        public BaseFileAppender(string fileName, ICreateFileParameters createParameters)
        {
            this.fileName = fileName;
            this.createParameters = createParameters;
            this.openTime = CurrentTimeGetter.Now;
            this.lastWriteTime = DateTime.MinValue;
        }

        /// <summary>
        /// Gets the name of the file.
        /// </summary>
        /// <value>The name of the file.</value>
        public string FileName
        {
            get { return this.fileName; }
        }

        /// <summary>
        /// Gets the last write time.
        /// </summary>
        /// <value>The last write time.</value>
        public DateTime LastWriteTime
        {
            get { return this.lastWriteTime; }
        }

        /// <summary>
        /// Gets the open time of the file.
        /// </summary>
        /// <value>The open time.</value>
        public DateTime OpenTime
        {
            get { return this.openTime; }
        }

        /// <summary>
        /// Gets the file creation parameters.
        /// </summary>
        /// <value>The file creation parameters.</value>
        public ICreateFileParameters CreateFileParameters
        {
            get { return this.createParameters; }
        }

        /// <summary>
        /// Writes the specified bytes.
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        public abstract void Write(byte[] bytes);

        /// <summary>
        /// Flushes this instance.
        /// </summary>
        public abstract void Flush();

        /// <summary>
        /// Closes this instance.
        /// </summary>
        public abstract void Close();

        /// <summary>
        /// Gets the file info.
        /// </summary>
        /// <param name="lastWriteTime">The last write time.</param>
        /// <param name="fileLength">Length of the file.</param>
        /// <returns>True if the operation succeeded, false otherwise.</returns>
        public abstract bool GetFileInfo(out DateTime lastWriteTime, out long fileLength);

        /// <summary>
        /// Records the last write time for a file.
        /// </summary>
        protected void FileTouched()
        {
            this.lastWriteTime = CurrentTimeGetter.Now;
        }

        /// <summary>
        /// Records the last write time for a file to be specific date.
        /// </summary>
        /// <param name="dateTime">Date and time when the last write occured.</param>
        protected void FileTouched(DateTime dateTime)
        {
            this.lastWriteTime = dateTime;
        }

        /// <summary>
        /// Creates the file stream.
        /// </summary>
        /// <param name="allowConcurrentWrite">If set to <c>true</c> allow concurrent writes.</param>
        /// <returns>A <see cref="FileStream"/> object which can be used to write to the file.</returns>
        protected FileStream CreateFileStream(bool allowConcurrentWrite)
        {
            int currentDelay = this.createParameters.ConcurrentWriteAttemptDelay;

            InternalLogger.Trace("Opening {0} with concurrentWrite={1}", this.FileName, allowConcurrentWrite);
            for (int i = 0; i < this.createParameters.ConcurrentWriteAttempts; ++i)
            {
                try
                {
                    try
                    {
                        return this.TryCreateFileStream(allowConcurrentWrite);
                    }
                    catch (DirectoryNotFoundException)
                    {
                        if (!this.createParameters.CreateDirs)
                        {
                            throw;
                        }

                        Directory.CreateDirectory(Path.GetDirectoryName(this.FileName));
                        return this.TryCreateFileStream(allowConcurrentWrite);
                    }
                }
                catch (IOException)
                {
                    if (!this.createParameters.ConcurrentWrites || !allowConcurrentWrite || i + 1 == this.createParameters.ConcurrentWriteAttempts)
                    {
                        throw; // rethrow
                    }

                    int actualDelay = this.random.Next(currentDelay);
                    InternalLogger.Warn("Attempt #{0} to open {1} failed. Sleeping for {2}ms", i, this.FileName, actualDelay);
                    currentDelay *= 2;
                    System.Threading.Thread.Sleep(actualDelay);
                }
            }

            throw new Exception("Should not be reached.");
        }

#if !NET_CF && !SILVERLIGHT
        private FileStream WindowsCreateFile(string fileName, bool allowConcurrentWrite)
        {
            int fileShare = Win32FileHelper.FILE_SHAREC_READ;

            if (allowConcurrentWrite)
            {
                fileShare |= Win32FileHelper.FILE_SHARE_WRITE;
            }

            if (this.createParameters.EnableFileDelete && PlatformDetector.GetCurrentRuntimeOS() != RuntimeOS.Windows)
            {
                fileShare |= Win32FileHelper.FILE_SHARE_DELETE;
            }

            IntPtr handle = Win32FileHelper.CreateFile(
                fileName,
                Win32FileHelper.FileAccess.GenericWrite,
                fileShare,
                IntPtr.Zero,
                Win32FileHelper.CreationDisposition.OpenAlways,
                this.createParameters.FileAttributes, 
                IntPtr.Zero);

            if (handle.ToInt32() == -1)
            {
                Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
            }

            var safeHandle = new Microsoft.Win32.SafeHandles.SafeFileHandle(handle, true);
            FileStream returnValue = new FileStream(safeHandle, FileAccess.Write, this.createParameters.BufferSize);
            returnValue.Seek(0, SeekOrigin.End);
            return returnValue;
        }
#endif

        private FileStream TryCreateFileStream(bool allowConcurrentWrite)
        {
            FileShare fileShare = FileShare.Read;

            if (allowConcurrentWrite)
            {
                fileShare = FileShare.ReadWrite;
            }

#if !NET_CF
            if (this.createParameters.EnableFileDelete && PlatformDetector.GetCurrentRuntimeOS() != RuntimeOS.Windows)
            {
                fileShare |= FileShare.Delete;
            }
#endif

#if !NET_CF && !SILVERLIGHT
            if (PlatformDetector.IsCurrentOSCompatibleWith(RuntimeOS.WindowsNT) ||
                PlatformDetector.IsCurrentOSCompatibleWith(RuntimeOS.Windows))
            {
                return this.WindowsCreateFile(this.FileName, allowConcurrentWrite);
            }
#endif

            return new FileStream(
                this.FileName, 
                FileMode.Append, 
                FileAccess.Write, 
                fileShare, 
                this.createParameters.BufferSize);
        }
    }
}
