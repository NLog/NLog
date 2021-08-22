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

    using NLog.Common;

    /// <summary>
    /// Optimized single-process file appender which keeps the file open for exclusive write.
    /// </summary>
    [SecuritySafeCritical]
    internal class SingleProcessFileAppender : BaseFileAppender
    {
        public static readonly IFileAppenderFactory TheFactory = new Factory();

        private FileStream _file;
        private readonly bool _enableFileDeleteSimpleMonitor;
        private DateTime _lastSimpleMonitorCheckTimeUtc;

        /// <summary>
        /// Initializes a new instance of the <see cref="SingleProcessFileAppender" /> class.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="parameters">The parameters.</param>
        public SingleProcessFileAppender(string fileName, ICreateFileParameters parameters) : base(fileName, parameters)
        {
            _file = CreateFileStream(false);
            _enableFileDeleteSimpleMonitor = parameters.EnableFileDeleteSimpleMonitor;
            _lastSimpleMonitorCheckTimeUtc = OpenTimeUtc;
        }

        /// <summary>
        /// Writes the specified bytes.
        /// </summary>
        /// <param name="bytes">The bytes array.</param>
        /// <param name="offset">The bytes array offset.</param>
        /// <param name="count">The number of bytes.</param>
        public override void Write(byte[] bytes, int offset, int count)
        {
            if (_file is null)
            {
                return;
            }

            if (_enableFileDeleteSimpleMonitor && MonitorForEnableFileDeleteEvent(FileName, ref _lastSimpleMonitorCheckTimeUtc))
            {
                CloseFileSafe(ref _file, FileName);
                _file = CreateFileStream(false);
            }

            _file.Write(bytes, offset, count);
        }

        /// <summary>
        /// Flushes this instance.
        /// </summary>
        public override void Flush()
        {
            if (_file is null)
            {
                return;
            }

            _file.Flush();
        }

        /// <summary>
        /// Closes this instance.
        /// </summary>
        public override void Close()
        {
            CloseFileSafe(ref _file, FileName);
        }


        /// <summary>
        /// Gets the creation time for a file associated with the appender. The time returned is in Coordinated Universal 
        /// Time [UTC] standard.
        /// </summary>
        /// <returns>The file creation time.</returns>
        public override DateTime? GetFileCreationTimeUtc()
        {
            return CreationTimeUtc;
        }

        /// <summary>
        /// Gets the length in bytes of the file associated with the appender.
        /// </summary>
        /// <returns>A long value representing the length of the file in bytes.</returns>
        public override long? GetFileLength()
        {
            return _file?.Length;
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
                return new SingleProcessFileAppender(fileName, parameters);
            }
        }
    }
}
