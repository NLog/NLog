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
    using System.Runtime.InteropServices;
    using System.Security;

    using NLog.Common;
    using NLog.Internal;

    /// <summary>
    /// Base class for optimized file appenders.
    /// </summary>
    [SecuritySafeCritical]
    internal abstract class BaseFileAppender : IDisposable
    {
#pragma warning disable S2245   // Make sure that using this pseudorandom number generator is safe here (Not security sensitive)
        private readonly Random _random = new Random(); //NOSONAR
#pragma warning restore S2245   // Make sure that using this pseudorandom number generator is safe here

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseFileAppender" /> class.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="createParameters">The create parameters.</param>
        protected BaseFileAppender(string fileName, ICreateFileParameters createParameters)
        {
            CreateFileParameters = createParameters;
            FileName = fileName;
            OpenTimeUtc = DateTime.UtcNow; // to be consistent with timeToKill in FileTarget.AutoClosingTimerCallback
        }

        /// <summary>
        /// Gets the path of the file, including file extension.
        /// </summary>
        /// <value>The name of the file.</value>
        public string FileName { get; }

        /// <summary>
        /// Gets or sets the creation time for a file associated with the appender. The time returned is in Coordinated
        /// Universal Time [UTC] standard.
        /// </summary>
        /// <returns>The creation time of the file.</returns>
        public DateTime CreationTimeUtc
        {
            get => _creationTimeUtc;
            internal set
            {
                _creationTimeUtc = value;
                CreationTimeSource = Time.TimeSource.Current.FromSystemTime(value); // Performance optimization to skip converting every time
            }
        }
        DateTime _creationTimeUtc;

        /// <summary>
        /// Gets or sets the creation time for a file associated with the appender. Synchronized by <see cref="CreationTimeUtc"/>
        /// The time format is based on <see cref="NLog.Time.TimeSource" />
        /// </summary>
        public DateTime CreationTimeSource { get; private set; }

        /// <summary>
        /// Gets the last time the file associated with the appender is opened. The time returned is in Coordinated
        /// Universal Time [UTC] standard.
        /// </summary>
        /// <returns>The time the file was last opened.</returns>
        public DateTime OpenTimeUtc { get; private set; }

        /// <summary>
        /// Gets the file creation parameters.
        /// </summary>
        /// <value>The file creation parameters.</value>
        public ICreateFileParameters CreateFileParameters { get; private set; }

        /// <summary>
        /// Writes the specified bytes.
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        public void Write(byte[] bytes)
        {
            Write(bytes, 0, bytes.Length);
        }

        /// <summary>
        /// Writes the specified bytes to a file.
        /// </summary>
        /// <param name="bytes">The bytes array.</param>
        /// <param name="offset">The bytes array offset.</param>
        /// <param name="count">The number of bytes.</param>
        public abstract void Write(byte[] bytes, int offset, int count);

        /// <summary>
        /// Flushes this file-appender instance.
        /// </summary>
        public abstract void Flush();

        /// <summary>
        /// Closes this file-appender instance.
        /// </summary>
        public abstract void Close();

        /// <summary>
        /// Gets the creation time for a file associated with the appender. The time returned is in Coordinated Universal
        /// Time [UTC] standard.
        /// </summary>
        /// <returns>The file creation time.</returns>
        public abstract DateTime? GetFileCreationTimeUtc();

        /// <summary>
        /// Gets the length in bytes of the file associated with the appender.
        /// </summary>
        /// <returns>A long value representing the length of the file in bytes.</returns>
        public abstract long? GetFileLength();

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged resources; <see langword="false"/> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Close();
            }
        }

        /// <summary>
        /// Creates the file stream.
        /// </summary>
        /// <param name="allowFileSharedWriting">If set to <c>true</c> sets the file stream to allow shared writing.</param>
        /// <param name="overrideBufferSize">If larger than 0 then it will be used instead of the default BufferSize for the FileStream.</param>
        /// <returns>A <see cref="FileStream"/> object which can be used to write to the file.</returns>
        protected FileStream CreateFileStream(bool allowFileSharedWriting, int overrideBufferSize = 0)
        {
            int currentDelay = CreateFileParameters.FileOpenRetryDelay;

            InternalLogger.Trace("{0}: Opening {1} with allowFileSharedWriting={2}", CreateFileParameters, FileName, allowFileSharedWriting);
            for (int i = 0; i <= CreateFileParameters.FileOpenRetryCount; ++i)
            {
                try
                {
                    return TryCreateDirectoryFileStream(allowFileSharedWriting, overrideBufferSize);
                }
                catch (IOException ex)
                {
                    if (i + 1 >= CreateFileParameters.FileOpenRetryCount)
                    {
                        throw; // rethrow
                    }

                    int actualDelay = _random.Next(currentDelay);
                    InternalLogger.Warn("{0}: Attempt #{1} to open {2} failed - {3} {4}. Sleeping for {5}ms", CreateFileParameters, i, FileName, ex.GetType(), ex.Message, actualDelay);
                    currentDelay *= 2;
                    System.Threading.Thread.Sleep(TimeSpan.FromMilliseconds(actualDelay));
                }
            }

            throw new InvalidOperationException("Should not be reached.");
        }

        private FileStream TryCreateDirectoryFileStream(bool allowFileSharedWriting, int overrideBufferSize)
        {
            try
            {
                RefreshFileCreationTime();

                return TryCreateFileStream(allowFileSharedWriting, overrideBufferSize);
            }
            catch (DirectoryNotFoundException)
            {
                //we don't check the directory on beforehand, as that will really slow down writing.
                if (!CreateFileParameters.CreateDirs)
                {
                    throw;
                }

                InternalLogger.Debug("{0}: DirectoryNotFoundException - Attempting to create directory for FileName: {1}", CreateFileParameters, FileName);

                var directoryName = Path.GetDirectoryName(FileName);

                try
                {
                    Directory.CreateDirectory(directoryName);
                }
                catch (DirectoryNotFoundException ex)
                {
                    //if creating a directory failed, don't retry for this message (e.g the FileOpenRetryCount below)
                    throw new NLogRuntimeException($"Could not create directory {directoryName}", ex);
                }

                return TryCreateFileStream(allowFileSharedWriting, overrideBufferSize);
            }
        }

#if NETFRAMEWORK && !MONO
        private FileStream WindowsCreateFile(bool allowFileSharedWriting, int bufferSize)
        {
            int fileShare = Win32FileNativeMethods.FILE_SHARE_READ;

            if (allowFileSharedWriting)
            {
                fileShare |= Win32FileNativeMethods.FILE_SHARE_WRITE;
            }

            if (CreateFileParameters.EnableFileDelete && PlatformDetector.CurrentOS != PlatformSystem.Windows9x)
            {
                fileShare |= Win32FileNativeMethods.FILE_SHARE_DELETE;
            }

            Microsoft.Win32.SafeHandles.SafeFileHandle handle = null;
            FileStream fileStream = null;

            try
            {
                handle = Win32FileNativeMethods.CreateFile(
                FileName,
                Win32FileNativeMethods.FileAccess.GenericWrite,
                fileShare,
                IntPtr.Zero,
                Win32FileNativeMethods.CreationDisposition.OpenAlways,
                CreateFileParameters.FileAttributes,
                IntPtr.Zero);

                if (handle.IsInvalid)
                {
                    Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
                }

                fileStream = new FileStream(handle, FileAccess.Write, bufferSize);
                fileStream.Seek(0, SeekOrigin.End);
                return fileStream;
            }
            catch
            {
                fileStream?.Dispose();

                if ((handle != null) && (!handle.IsClosed))
                    handle.Close();

                throw;
            }
        }
#endif

        private FileStream TryCreateFileStream(bool allowFileSharedWriting, int overrideBufferSize)
        {
            var bufferSize = overrideBufferSize > 0 ? overrideBufferSize : CreateFileParameters.BufferSize;

#if NETFRAMEWORK && !MONO
            try
            {
                if (!CreateFileParameters.ForceManaged && PlatformDetector.IsWin32 && !PlatformDetector.IsMono)
                {
                    return WindowsCreateFile(allowFileSharedWriting, bufferSize);
                }
            }
            catch (SecurityException)
            {
                InternalLogger.Debug("{0}: Could not use native Windows create file, falling back to managed filestream: {1}", CreateFileParameters, FileName);
            }
#endif

            FileShare fileShare = allowFileSharedWriting ? FileShare.ReadWrite : FileShare.Read;
            if (CreateFileParameters.EnableFileDelete)
            {
                fileShare |= FileShare.Delete;
            }

            return new FileStream(
                FileName,
                FileMode.Append,
                FileAccess.Write,
                fileShare,
                bufferSize);
        }

        private void RefreshFileCreationTime()
        {
            CreationTimeUtc = DateTime.UtcNow;

            try
            {
                FileInfo fileInfo = new FileInfo(FileName);
                if (fileInfo.Exists)
                {
                    CreationTimeUtc = fileInfo.LookupValidFileCreationTimeUtc();
                }
                else
                {
                    if (CreateFileParameters.IsArchivingEnabled || PlatformDetector.IsWin32)
                    {
                        File.Create(FileName).Dispose();

                        // Set the file's creation time to avoid being thwarted by Windows FileSystem Tunneling capabilities (https://support.microsoft.com/en-us/kb/172190).
                        File.SetCreationTimeUtc(FileName, CreationTimeUtc);
                    }
                }
            }
            catch (DirectoryNotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                InternalLogger.Debug(ex, "{0}: Failed to refresh CreationTimeUtc for FileName: {1}", CreateFileParameters, FileName);
            }
        }

        protected static void CloseFileSafe(ref FileStream fileStream, string fileName)
        {
            if (fileStream is null)
            {
                return;
            }

            InternalLogger.Trace("FileTarget: Closing '{0}'", fileName);
            try
            {
                fileStream.Close();
            }
            catch (Exception ex)
            {
                // Swallow exception as the file-stream now is in final state (broken instead of closed)
                InternalLogger.Warn(ex, "FileTarget: Failed to close file '{0}'", fileName);
                System.Threading.Thread.Sleep(TimeSpan.FromMilliseconds(1));    // Artificial delay to avoid hammering a bad file location
            }
            finally
            {
                fileStream = null;
            }
        }

        protected static bool MonitorForEnableFileDeleteEvent(string fileName, ref int lastSimpleMonitorCheckTickCount)
        {
            int ticksDelta = Environment.TickCount - lastSimpleMonitorCheckTickCount;
            if (ticksDelta > 1000 || ticksDelta < -1000)
            {
                lastSimpleMonitorCheckTickCount = Environment.TickCount;
                try
                {
                    if (!File.Exists(fileName))
                    {
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    InternalLogger.Error(ex, "FileTarget: Failed to check if File.Exists: '{0}'", fileName);
                    return true;
                }
            }
            return false;
        }
    }
}
