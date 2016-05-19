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

#if !SILVERLIGHT && !__IOS__ && !__ANDROID__

namespace NLog.Internal.FileAppenders
{
    using System;
    using System.Security;
    using NLog.Common;
    using System.Runtime.InteropServices;
    using System.IO;
    using System.Threading;

    /// <summary>
    /// Provides a multiprocess-safe atomic file append while
    /// keeping the files open.
    /// </summary>
    [SecuritySafeCritical]
    internal class WindowsMultiProcessFileAppender : BaseFileAppender
    {
        public static readonly IFileAppenderFactory TheFactory = new Factory();

        private Microsoft.Win32.SafeHandles.SafeFileHandle file;

        /// <summary>
        /// Initializes a new instance of the <see cref="WindowsMultiProcessFileAppender" /> class.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="parameters">The parameters.</param>
        public WindowsMultiProcessFileAppender(string fileName, ICreateFileParameters parameters) : base(fileName, parameters)
        {
            try
            {
                CreateAppendOnlyFile(fileName);
            }
            catch
            {
                if (file != null)
                {
                    if (!file.IsClosed)
                        file.Close();
                    file = null;
                }

                throw;
            }
        }

        /// <summary>
        /// Creates or opens a file in a special mode, so that writes are automatically
        /// as atomic writes at the file end.
        /// See also "UnixMultiProcessFileAppender" which does a similar job on *nix platforms.
        /// </summary>
        /// <param name="fileName">File to create or open</param>
        private void CreateAppendOnlyFile(string fileName)
        {
            string dir = Path.GetDirectoryName(fileName);
            if (!Directory.Exists(dir))
            {
                if (!CreateFileParameters.CreateDirs)
                {
                    throw new DirectoryNotFoundException(dir);
                }
                Directory.CreateDirectory(dir);
            }

            int fileShare = Win32FileNativeMethods.FILE_SHARE_READ | Win32FileNativeMethods.FILE_SHARE_WRITE;

            if (this.CreateFileParameters.EnableFileDelete)
            {
                fileShare |= Win32FileNativeMethods.FILE_SHARE_DELETE;
            }
            
            try
            {
                // https://blogs.msdn.microsoft.com/oldnewthing/20151127-00/?p=92211/
                // https://msdn.microsoft.com/en-us/library/ff548289.aspx
                // If only the FILE_APPEND_DATA and SYNCHRONIZE flags are set, the caller can write only to the end of the file, 
                // and any offset information about writes to the file is ignored.
                // However, the file will automatically be extended as necessary for this type of write operation.

                file = Win32FileNativeMethods.CreateFile(
                    fileName,
                    Win32FileNativeMethods.FileAccess.FileAppendData | Win32FileNativeMethods.FileAccess.Synchronize,
                    fileShare,
                    IntPtr.Zero,
                    Win32FileNativeMethods.CreationDisposition.OpenAlways, // open or create
                    this.CreateFileParameters.FileAttributes,
                    IntPtr.Zero);

                if (file.IsInvalid)
                {
                    int hr = Marshal.GetHRForLastWin32Error();
                    InternalLogger.Error("Unable to open/create '{0}' HR:{1}", FileName, hr);
                    Marshal.ThrowExceptionForHR(hr);
                }
                int lastError = Marshal.GetLastWin32Error();
                if (lastError == Win32FileNativeMethods.NOERROR)
                {
                    // We actually created the file and eventually concurrent processes 
                    // may have opened the same file in between.
                    // Only the one process creating the file should adjust the file creation time 
                    // to avoid being thwarted by Windows' Tunneling capabilities (https://support.microsoft.com/en-us/kb/172190).
                    // Unfortunately we can't use the native SetFileTime() to prevent opening the file 2nd time.
                    // This would require another desiredAccess flag which would disable the atomic append feature.
                    // See also UpdateCreationTime()

                    this.CreationTime = DateTime.UtcNow;
                    File.SetCreationTimeUtc(this.FileName, this.CreationTime);
                }
                else if (lastError == Win32FileNativeMethods.ERROR_ALREADY_EXISTS)
                {
                    // Somebody else has already created the file and we just 
                    // need to record the files creation time.
                    // There's a small chance for a racing condition here:
                    // Suppose another process has created the file and before executing the above 
                    // code "File.SetCreationTimeUtc(...)" context is switched to us reading the
                    // currently set creation time, which in case of above mentioned tunneling issue
                    // may be "wrong". Consequences aren't fatal since this.CreationTime isn't used by NLog itself.
                    // Anyhow, to increase chances of getting the "right" creation time we may wait for some time
                    // and read creation time again.
                    this.CreationTime = File.GetCreationTimeUtc(this.FileName);
                    if (this.CreationTime < DateTime.UtcNow - TimeSpan.FromSeconds(2))
                    {
                        // File wasn't created "almost now". 
                        // This could mean creation time has tunneled through from another file (see comment above).
                        Thread.Sleep(10);
                        // Having waited for a short amount of time usually means the file creation process has continued
                        // code execution just enough to the above point where it has fixed up the creation time.
                        this.CreationTime = File.GetCreationTimeUtc(this.FileName);
                    }
                }
            }
            catch
            {
                if ((file != null) && (!file.IsClosed))
                    file.Close();

                file = null;

                throw;
            }
        }

        /// <summary>
        /// Writes the specified bytes.
        /// </summary>
        /// <param name="bytes">The bytes to be written.</param>
        public override void Write(byte[] bytes)
        {
            if (file == null)
                return;

            uint written;
            bool success = Win32FileNativeMethods.WriteFile(file.DangerousGetHandle(), bytes, (uint)bytes.Length, out written, IntPtr.Zero);
            if (!success || (uint)bytes.Length != written)
            {
                InternalLogger.Error("Written only {0} out of {1} bytes to '{2}'", written, bytes.Length, FileName);
                Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
            }
            FileTouched();
        }

        /// <summary>
        /// Closes this instance.
        /// </summary>
        public override void Close()
        {
            InternalLogger.Trace("Closing '{0}'", FileName);
            if (file != null && !file.IsClosed)
            {
                file.Close();
            }

            file = null;
            FileTouched();
        }

        /// <summary>
        /// Flushes this instance.
        /// </summary>
        public override void Flush()
        {
            // do nothing, the file is written directly
        }

        /// <summary>
        /// Gets the file info.
        /// </summary>
        /// <returns>The file characteristics, if the file information was retrieved successfully, otherwise null.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2001:AvoidCallingProblematicMethods", MessageId = "System.Runtime.InteropServices.SafeHandle.DangerousGetHandle", Justification = "Optimization")]
        public override FileCharacteristics GetFileCharacteristics()
        {
            return FileCharacteristicsHelper.Helper.GetFileCharacteristics(FileName, file.DangerousGetHandle());
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
                return new WindowsMultiProcessFileAppender(fileName, parameters);
            }
        }
    }
}

#endif
