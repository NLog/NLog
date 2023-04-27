// 
// Copyright (c) 2004-2021 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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
    using System.Security;

    /// <summary>
    /// Implementation of <see cref="BaseFileAppender"/> which caches 
    /// file information.
    /// </summary>
    [SecuritySafeCritical]
    internal class CountingSingleProcessFileAppender : BaseFileAppender
    {
        public static readonly IFileAppenderFactory TheFactory = new Factory();

        private FileStream _file;
        private long _currentFileLength;
        private readonly bool _enableFileDeleteSimpleMonitor;
        private int _lastSimpleMonitorCheckTickCount;

        /// <summary>
        /// Initializes a new instance of the <see cref="CountingSingleProcessFileAppender" /> class.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="parameters">The parameters.</param>
        public CountingSingleProcessFileAppender(string fileName, ICreateFileParameters parameters)
            : base(fileName, parameters)
        {
            var fileInfo = new FileInfo(fileName);
            _currentFileLength = fileInfo.Exists ? fileInfo.Length : 0;
            _file = CreateFileStream(false);
            _enableFileDeleteSimpleMonitor = parameters.EnableFileDeleteSimpleMonitor;
            _lastSimpleMonitorCheckTickCount = Environment.TickCount;
        }

        /// <inheritdoc/>
        public override void Close()
        {
            CloseFileSafe(ref _file, FileName);
        }

        /// <inheritdoc/>
        public override void Flush()
        {
            _file?.Flush();
        }

        /// <inheritdoc/>
        public override DateTime? GetFileCreationTimeUtc()
        {
            return CreationTimeUtc;
        }

        /// <inheritdoc/>
        public override long? GetFileLength()
        {
            return _currentFileLength;
        }

        /// <inheritdoc/>
        public override void Write(byte[] bytes, int offset, int count)
        {
            if (_file is null)
            {
                return;
            }

            if (_enableFileDeleteSimpleMonitor && MonitorForEnableFileDeleteEvent(FileName, ref _lastSimpleMonitorCheckTickCount))
            {
                NLog.Common.InternalLogger.Debug("{0}: Recreating FileStream because no longer File.Exists: '{1}'", CreateFileParameters, FileName);
                CloseFileSafe(ref _file, FileName);
                _file = CreateFileStream(false);
                _currentFileLength = _file.Length;
            }

            _currentFileLength += count;
            _file.Write(bytes, offset, count);
        }

        /// <summary>
        /// Factory class which creates <see cref="CountingSingleProcessFileAppender"/> objects.
        /// </summary>
        private sealed class Factory : IFileAppenderFactory
        {
            /// <inheritdoc/>
            BaseFileAppender IFileAppenderFactory.Open(string fileName, ICreateFileParameters parameters)
            {
                return new CountingSingleProcessFileAppender(fileName, parameters);
            }
        }
    }
}