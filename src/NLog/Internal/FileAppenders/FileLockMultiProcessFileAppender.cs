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

#if !SILVERLIGHT

namespace NLog.Internal.FileAppenders
{
    using NLog.Common;
    using System;
    using System.IO;

    /// <summary>
    /// Provides a multiprocess-safe atomic file appends while
    /// keeping the files open.
    /// </summary>
    /// <remarks>
    /// Useful for non-Linux systems where named Mutex is not available.
    /// Uses file-locking for controlling the atomic write logic.
    /// </remarks>
    internal class FileLockMultiProcessFileAppender : BaseMutexFileAppender
    {
        public static readonly IFileAppenderFactory TheFactory = new Factory();

        private FileStream fileStream;
        private readonly FileCharacteristicsHelper fileCharacteristicsHelper;
        private readonly Random random = new Random();

        /// <summary>
        /// Initializes a new instance of the <see cref="FileLockMultiProcessFileAppender" /> class.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="parameters">The parameters.</param>
        public FileLockMultiProcessFileAppender(string fileName, ICreateFileParameters parameters) : base(fileName, parameters)
        {
            try
            {
                this.fileStream = CreateFileStream(true);
                if (this.fileStream.Length == 0)
                {
                    // Need a file-length to lock, lets wait for the first write
                    this.fileStream.Close();
                    this.fileStream = null;
                }
                this.fileCharacteristicsHelper = FileCharacteristicsHelper.CreateHelper(parameters.ForceManaged);
            }
            catch
            {
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
            FileStream mutualExclusive = null;
            FileStream activeStream = this.fileStream;
            if (activeStream == null)
            {
                int currentDelay = this.CreateFileParameters.ConcurrentWriteAttemptDelay;
                for (int i = 1; i <= this.CreateFileParameters.ConcurrentWriteAttempts; ++i)
                {
                    try
                    {
                        mutualExclusive = new FileStream(this.FileName, FileMode.Append, FileAccess.Write, FileShare.Read, this.CreateFileParameters.BufferSize);
                        activeStream = mutualExclusive;
                        break;  // We have mutual exclusive access
                    }
                    catch (IOException ex)
                    {
                        if (i == this.CreateFileParameters.ConcurrentWriteAttempts)
                            throw;

                        InternalLogger.Warn(ex, "Initial attempt failed to open file {0}", this.FileName);
                    }

                    activeStream = CreateFileStream(true);
                    if (activeStream != null)
                    {
                        if (activeStream.Length > 0)
                            break;  // We have a file-length that we can lock

                        activeStream.Close();
                        activeStream = null;
                    }
                }
            }
            if (activeStream != null && mutualExclusive == null)
            {
                int currentDelay = this.CreateFileParameters.ConcurrentWriteAttemptDelay;
                for (int i = 1; i <= this.CreateFileParameters.ConcurrentWriteAttempts * 10; ++i)
                {
                    try
                    {
                        activeStream.Lock(0, 1);
                        break;
                    }
                    catch (FileNotFoundException)
                    {
                        throw;
                    }
                    catch (IOException)
                    {
                        if (i == this.CreateFileParameters.ConcurrentWriteAttempts * 10)
                            throw;

                        int actualDelay = this.random.Next(currentDelay);
                        if (currentDelay < 16)
                            currentDelay *= 2;
                        System.Threading.Thread.Sleep(actualDelay);
                        continue;
                    }
                }
            }

            try
            {
                activeStream.Seek(0, SeekOrigin.End);
                activeStream.Write(bytes, 0, bytes.Length);
                activeStream.Flush();
                if (CaptureLastWriteTime)
                {
                    FileTouched();
                }
            }
            finally
            {
                if (mutualExclusive != null)
                {
                    bool fileLockAvailable = mutualExclusive.Length > 0;
                    mutualExclusive.Close();
                    if (fileLockAvailable)
                    {
                        this.fileStream = CreateFileStream(true);
                        if (this.fileStream.Length == 0)
                        {
                            this.fileStream.Close();
                            this.fileStream = null;
                        }
                    }
                }
                else if (activeStream != null)
                {
                    activeStream.Unlock(0, 1);
                }
            }
        }

        /// <summary>
        /// Closes this instance.
        /// </summary>
        public override void Close()
        {
            InternalLogger.Trace("Closing '{0}'", FileName);
            if (this.fileStream != null)
            {
                try
                {
                    this.fileStream.Close();
                }
                catch (Exception ex)
                {
                    // Swallow exception as the file-stream now is in final state (broken instead of closed)
                    InternalLogger.Warn(ex, "Failed to close file: '{0}'", FileName);
                }
                finally
                {
                    this.fileStream = null;
                }
            }

            FileTouched();
        }

        /// <summary>
        /// Flushes this instance.
        /// </summary>
        public override void Flush()
        {
            // do nothing, the stream is always flushed
        }

        /// <summary>
        /// Gets the creation time for a file associated with the appender. The time returned is in Coordinated Universal 
        /// Time [UTC] standard.
        /// </summary>
        /// <returns>The file creation time.</returns>
        public override DateTime? GetFileCreationTimeUtc()
        {
            var fileChars = GetFileCharacteristics();
            return fileChars != null ? fileChars.CreationTimeUtc : (DateTime?)null;
        }

        /// <summary>
        /// Gets the last time the file associated with the appeander is written. The time returned is in Coordinated 
        /// Universal Time [UTC] standard.
        /// </summary>
        /// <returns>The time the file was last written to.</returns>
        public override DateTime? GetFileLastWriteTimeUtc()
        {
            var fileChars = GetFileCharacteristics();
            return fileChars != null ? fileChars.LastWriteTimeUtc : (DateTime?)null;
        }

        /// <summary>
        /// Gets the length in bytes of the file associated with the appeander.
        /// </summary>
        /// <returns>A long value representing the length of the file in bytes.</returns>
        public override long? GetFileLength()
        {
            var fileChars = GetFileCharacteristics();
            return fileChars != null ? fileChars.FileLength : (long?)null;
        }

        private FileCharacteristics GetFileCharacteristics()
        {
            // TODO: It is not efficient to read all the whole FileCharacteristics and then using one property.
            return fileCharacteristicsHelper.GetFileCharacteristics(FileName, this.fileStream);
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
                return new FileLockMultiProcessFileAppender(fileName, parameters);
            }
        }
    }
}

#endif