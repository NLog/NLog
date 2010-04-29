// 
// Copyright (c) 2004-2006 Jaroslaw Kowalski <jaak@jkowalski.net>
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
using System.Xml;
using System.IO;
using System.Threading;
using System.Collections;
using System.Collections.Specialized;

using NLog;
using NLog.Config;

using NLog.Internal;
using System.Runtime.InteropServices;
#if !NETCF
using NLog.Internal.Win32;
#endif

namespace NLog.Internal.FileAppenders
{
    internal abstract class BaseFileAppender
    {
        private Random _random = new Random();
        private string _fileName;
        private ICreateFileParameters _createParameters;
        private DateTime _openTime;
        private DateTime _lastWriteTime;

        public string FileName
        {
            get { return _fileName; }
        }

        public DateTime LastWriteTime
        {
            get { return _lastWriteTime; }
        }

        public DateTime OpenTime
        {
            get { return _openTime; }
        }

        public ICreateFileParameters CreateFileParameters
        {
            get { return _createParameters; }
        }

        protected void FileTouched()
        {
            _lastWriteTime = CurrentTimeGetter.Now;
        }

        protected void FileTouched(DateTime dt)
        {
            _lastWriteTime = dt;
        }

        public BaseFileAppender(string fileName, ICreateFileParameters createParameters)
        {
            _fileName = fileName;
            _createParameters = createParameters;
            _openTime = CurrentTimeGetter.Now;
            _lastWriteTime = DateTime.MinValue;
        }

        public abstract void Write(byte[] bytes);

        public abstract void Flush();
        public abstract void Close();

        public abstract bool GetFileInfo(out DateTime lastWriteTime, out long fileLength);

        protected FileStream CreateFileStream(bool allowConcurrentWrite)
        {
            int currentDelay = _createParameters.ConcurrentWriteAttemptDelay;

            InternalLogger.Trace("Opening {0} with concurrentWrite={1}", FileName, allowConcurrentWrite);
            for (int i = 0; i < _createParameters.ConcurrentWriteAttempts; ++i)
            {
                try
                {
                    try
                    {
                        return TryCreateFileStream(allowConcurrentWrite);
                    }
                    catch (DirectoryNotFoundException)
                    {
                        if (!_createParameters.CreateDirs)
                            throw;

                        Directory.CreateDirectory(Path.GetDirectoryName(FileName));
                        return TryCreateFileStream(allowConcurrentWrite);
                    }
                }
                catch (IOException)
                {
                    if (!_createParameters.ConcurrentWrites || !allowConcurrentWrite || i + 1 == _createParameters.ConcurrentWriteAttempts)
                        throw; // rethrow

                    int actualDelay = _random.Next(currentDelay);
                    InternalLogger.Warn("Attempt #{0} to open {1} failed. Sleeping for {2}ms", i, FileName, actualDelay);
                    currentDelay *= 2;
                    System.Threading.Thread.Sleep(actualDelay);
                }
            }
            throw new Exception("Should not be reached.");
        }


#if !NETCF
        private FileStream WindowsCreateFile(string fileName, bool allowConcurrentWrite)
        {
            int fileShare = Win32FileHelper.FILE_SHARE_READ;

            if (allowConcurrentWrite)
                fileShare |= Win32FileHelper.FILE_SHARE_WRITE;

            if (_createParameters.EnableFileDelete && PlatformDetector.GetCurrentRuntimeOS() != RuntimeOS.Windows)
                fileShare |= Win32FileHelper.FILE_SHARE_DELETE;

            IntPtr hFile = Win32FileHelper.CreateFile(
                fileName,
                Win32FileHelper.FileAccess.GenericWrite,
                fileShare,
                IntPtr.Zero,
                Win32FileHelper.CreationDisposition.OpenAlways,
                _createParameters.FileAttributes, IntPtr.Zero);

            if (hFile.ToInt32() == -1)
                Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());

            FileStream returnValue;

#if DOTNET_2_0 || NETCF_2_0
            Microsoft.Win32.SafeHandles.SafeFileHandle safeHandle = new Microsoft.Win32.SafeHandles.SafeFileHandle(hFile, true);
            returnValue = new FileStream(safeHandle, FileAccess.Write, _createParameters.BufferSize);
#else
            returnValue = new FileStream(hFile, FileAccess.Write, true, _createParameters.BufferSize);
#endif
            returnValue.Seek(0, SeekOrigin.End);
            return returnValue;
        }
#endif

        private FileStream TryCreateFileStream(bool allowConcurrentWrite)
        {
            FileShare fileShare = FileShare.Read;

            if (allowConcurrentWrite)
                fileShare = FileShare.ReadWrite;

#if DOTNET_2_0
            if (_createParameters.EnableFileDelete && PlatformDetector.GetCurrentRuntimeOS() != RuntimeOS.Windows)
            {
                fileShare |= FileShare.Delete;
            }
#endif

#if !NETCF
            if (PlatformDetector.IsCurrentOSCompatibleWith(RuntimeOS.WindowsNT) ||
                    PlatformDetector.IsCurrentOSCompatibleWith(RuntimeOS.Windows))
            {
                return WindowsCreateFile(FileName, allowConcurrentWrite);
            }
#endif

            return new FileStream(FileName, 
                FileMode.Append, 
                FileAccess.Write, 
                fileShare, 
                _createParameters.BufferSize);
        }
    }
}
