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

namespace NLog.Internal.FileAppenders
{
    internal class CountingSingleProcessFileAppender : IFileAppender
    {
        private FileStream _file;
        private string _fileName;
        private long _fileLength;
        private DateTime _lastWriteTime;

        public static readonly IFileAppenderFactory TheFactory = new Factory();

        class Factory : IFileAppenderFactory
        {
            public IFileAppender Open(string fileName, IFileOpener opener)
            {
                return new CountingSingleProcessFileAppender(fileName, opener);
            }
        }

        public string FileName
        {
            get { return _fileName; }
        }

        public CountingSingleProcessFileAppender(string fileName, IFileOpener opener)
        {
            _fileName = fileName;
            FileInfo fi = new FileInfo(fileName);
            if (fi.Exists)
            {
                _lastWriteTime = fi.LastWriteTime;
                _fileLength = fi.Length;
            }
            else
            {
                _lastWriteTime = DateTime.Now;
                _fileLength = 0;
            }

            _file = opener.Create(fileName, false);
        }

        public void Write(byte[] bytes)
        {
            _fileLength += bytes.Length;
            _lastWriteTime = DateTime.Now;
            _file.Write(bytes, 0, bytes.Length);
        }

        public void Flush()
        {
            _file.Flush();
        }

        public void Close()
        {
            //InternalLogger.Trace("Closing '{0}'", _fileName);
            _file.Close();
        }

        public bool GetFileInfo(out DateTime lastWriteTime, out long fileLength)
        {
            lastWriteTime = _lastWriteTime;
            fileLength = _fileLength;
            return true;
        }
    }
}
