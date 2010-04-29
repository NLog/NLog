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

#if !NETCF

using System;
using System.Xml;
using System.IO;
using System.Threading;
using System.Text;
using System.Collections;
using System.Collections.Specialized;

using NLog;
using NLog.Config;

using NLog.Internal;

namespace NLog.Internal.FileAppenders
{
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
    internal class MutexMultiProcessFileAppender : BaseFileAppender
    {
        private Mutex _mutex;
        private FileStream _file;

        public static readonly IFileAppenderFactory TheFactory = new Factory();

        public class Factory : IFileAppenderFactory
        {
            public BaseFileAppender Open(string fileName, ICreateFileParameters parameters)
            {
                return new MutexMultiProcessFileAppender(fileName, parameters);
            }
        }

        public MutexMultiProcessFileAppender(string fileName, ICreateFileParameters parameters) : base(fileName, parameters)
        {
            try
            {
                _mutex = new Mutex(false, GetMutexName(fileName));
                _file = CreateFileStream(true);
            }
            catch
            {
                if (_mutex != null)
                {
                    _mutex.Close();
                    _mutex = null;
                }
                if (_file != null)
                {
                    _file.Close();
                    _file = null;
                }
                throw;
            }
        }

        public override void Write(byte[] bytes)
        {
            if (_mutex == null)
                return;
            _mutex.WaitOne();
            try
            {
                _file.Seek(0, SeekOrigin.End);
                _file.Write(bytes, 0, bytes.Length);
                _file.Flush();
                FileTouched();
            }
            finally
            {
                _mutex.ReleaseMutex();
            }
        }

        public override void Close()
        {
            InternalLogger.Trace("Closing '{0}'", FileName);
            if (_mutex != null)
            {
                _mutex.Close();
            }
            if (_file != null)
            {
                _file.Close();
            }
            _mutex = null;
            _file = null;
            FileTouched();
        }

        public override void Flush()
        {
            // do nothing, the stream is always flushed
        }

        private string GetMutexName(string fileName)
        {
            string canonicalName = Path.GetFullPath(fileName).ToLower();

            canonicalName = canonicalName.Replace('\\', '_');
            canonicalName = canonicalName.Replace('/', '_');
            canonicalName = canonicalName.Replace(':', '_');

            return "filelock-mutex-" + canonicalName;
        }

        public override bool GetFileInfo(out DateTime lastWriteTime, out long fileLength)
        {
#if NET_2_API
            return FileInfoHelper.Helper.GetFileInfo(FileName, _file.SafeFileHandle.DangerousGetHandle(), out lastWriteTime, out fileLength);
#else
            return FileInfoHelper.Helper.GetFileInfo(FileName, _file.Handle, out lastWriteTime, out fileLength);
#endif
        }
    }
}

#endif
