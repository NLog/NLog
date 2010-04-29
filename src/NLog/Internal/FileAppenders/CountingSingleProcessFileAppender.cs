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
    internal class CountingSingleProcessFileAppender : BaseFileAppender
    {
        private FileStream _file;
        private long _fileLength;

        public static readonly IFileAppenderFactory TheFactory = new Factory();

        class Factory : IFileAppenderFactory
        {
            public BaseFileAppender Open(string fileName, ICreateFileParameters parameters)
            {
                return new CountingSingleProcessFileAppender(fileName, parameters);
            }
        }

        public CountingSingleProcessFileAppender(string fileName, ICreateFileParameters parameters) : base(fileName, parameters)
        {
            FileInfo fi = new FileInfo(fileName);
            if (fi.Exists)
            {
                FileTouched(fi.LastWriteTime);
                _fileLength = fi.Length;
            }
            else
            {
                FileTouched();
                _fileLength = 0;
            }

            _file = CreateFileStream(false);
        }

        public override void Write(byte[] bytes)
        {
            if (_file == null)
                return;
            _fileLength += bytes.Length;
            _file.Write(bytes, 0, bytes.Length);
            FileTouched();
        }

        public override void Flush()
        {
            if (_file == null)
                return;
            _file.Flush();
            FileTouched();
        }

        public override void Close()
        {
            if (_file == null)
                return;
            //InternalLogger.Trace("Closing '{0}'", _fileName);
            _file.Close();
            _file = null;
        }

        public override bool GetFileInfo(out DateTime lastWriteTime, out long fileLength)
        {
            lastWriteTime = LastWriteTime;
            fileLength = _fileLength;
            return true;
        }
    }
}
