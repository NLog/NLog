// 
// Copyright (c) 2004 Jaroslaw Kowalski <jaak@polbox.com>
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
// * Neither the name of the Jaroslaw Kowalski nor the names of its 
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

using NLog;

namespace NLog.Appenders
{
    [Appender("File")]
    public class FileAppender : NLog.Appender
    {
        private Random _random = new Random();
        private Layout _fileNameLayout;
        private bool _createDirs = false;
        private bool _keepFileOpen = true;
        private string _lastFileName = String.Empty;
        private StreamWriter _outputFile;
        private System.Text.Encoding _encoding = System.Text.Encoding.Default;
        private bool _autoFlush = true;
        private bool _concurrentWrites = true;
        private int _concurrentWriteAttempts = 10;
        private int _concurrentWriteAttemptDelay = 1;

        public string FileName
        {
            get { return _fileNameLayout.Text; }
            set { _fileNameLayout = new Layout(value); }
        }

        public bool CreateDirs
        {
            get { return _createDirs; }
            set { _createDirs = value; }
        }

        public bool KeepFileOpen
        {
            get { return _keepFileOpen; }
            set { _keepFileOpen = value; }
        }

        public bool AutoFlush
        {
            get { return _autoFlush; }
            set { _autoFlush = value; }
        }

        public string Encoding
        {
            get { return _encoding.WebName; }
            set { _encoding = System.Text.Encoding.GetEncoding(value); }
        }

        public bool ConcurrentWrites
        {
            get { return _concurrentWrites; }
            set { _concurrentWrites = value; }
        }

        public int ConcurrentWriteAttempts
        {
            get { return _concurrentWriteAttempts; }
            set { _concurrentWriteAttempts = value; }
        }

        public int ConcurrentWriteAttemptDelay
        {
            get { return _concurrentWriteAttemptDelay; }
            set { _concurrentWriteAttemptDelay = value; }
        }

        private StreamWriter OpenStreamWriter(string fileName) {
            StreamWriter retVal;

            FileInfo fi = new FileInfo(fileName);
            if (!fi.Exists)
            {
                if (!fi.Directory.Exists)
                {
                    Directory.CreateDirectory(fi.DirectoryName);
                }
            }

            if (!ConcurrentWrites) {
                retVal = new StreamWriter(fileName, true, _encoding);
            } else {
                int currentDelay = _concurrentWriteAttemptDelay;
                retVal = null;

                for (int i = 0; i < _concurrentWriteAttempts; ++i) {
                    try {
                        retVal = new StreamWriter(fileName, true, _encoding);
                        break;
                    }
                    catch (IOException ex)
                    {
                        Console.WriteLine("ex: {0}", ex.Message);
                        int actualDelay = _random.Next(currentDelay);
                        currentDelay *= 2;
                        System.Threading.Thread.Sleep(actualDelay);
                    }
                }
            }
            
            retVal.AutoFlush = _autoFlush;
            return retVal;
        }

        public override void Append(LogEventInfo ev) {
            string fileName = _fileNameLayout.GetFormattedMessage(ev);

            if (fileName != _lastFileName && _outputFile != null) {
                _outputFile.Close();
                _outputFile = null;
            }
            _lastFileName = fileName;
            if (_outputFile == null) {
                _outputFile = OpenStreamWriter(fileName);
                if (_outputFile == null)
                    return;
            }
            _outputFile.WriteLine(CompiledLayout.GetFormattedMessage(ev));
            _outputFile.Flush();
            if (!KeepFileOpen || ConcurrentWrites) {
                _outputFile.Close();
                _outputFile = null;
            }
        }
    }
}
