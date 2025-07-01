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

namespace NLog.Targets
{
    using System.IO;
    using System.IO.Compression;

    /// <summary>
    /// Extended standard FileTarget with GZip compression as part of file-logging
    /// </summary>
    /// <remarks>
    /// <a href="https://github.com/nlog/nlog/wiki/GZip-File-target">See NLog Wiki</a>
    /// </remarks>
    /// <seealso href="https://github.com/nlog/nlog/wiki/GZip-File-target">Documentation on NLog Wiki</seealso>
    [Target("GZipFile")]
    public class GZipFileTarget : FileTarget
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GZipFileTarget" /> class.
        /// </summary>
        public GZipFileTarget()
        {
            ArchiveOldFileOnStartup = true; // Not possible to append to existing file using GZip, must read the entire file into memory and rewrite everything again
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GZipFileTarget" /> class.
        /// </summary>
        public GZipFileTarget(string name) : this()
        {
            Name = name;
        }

        /// <summary>
        /// Gets or sets whether to enable file-compression using <see cref="GZipStream" />
        /// </summary>
        public bool EnableArchiveFileCompression { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to emphasize Fastest-speed or Optimal-compression
        /// </summary>
        public CompressionLevel CompressionLevel { get; set; } = CompressionLevel.Fastest;

        /// <inheritdoc />
        protected override void InitializeTarget()
        {
            base.InitializeTarget();

            if (!KeepFileOpen)
                throw new NLogConfigurationException("GZipFileTarget requires KeepFileOpen = true");

            if (!ArchiveOldFileOnStartup)
                throw new NLogConfigurationException("GZipFileTarget requires ArchiveOldFileOnStartup = true");
        }

        /// <inheritdoc />
        protected override Stream CreateFileStream(string filePath, int bufferSize)
        {
            if (!EnableArchiveFileCompression || CompressionLevel == CompressionLevel.NoCompression || !ArchiveOldFileOnStartup || !KeepFileOpen)
            {
                NLog.Common.InternalLogger.Debug("{0}: File Compression has been disabled, fallback to normal FileStream", this);
                return base.CreateFileStream(filePath, bufferSize);
            }

            var underlyingStream = base.CreateFileStream(filePath, bufferSize);
            var compressStream = new GZipStream(underlyingStream, CompressionLevel);

            if (!AutoFlush && BufferSize > 0)
                return new BufferedStream(compressStream, BufferSize);
            else
                return compressStream;
        }
    }
}
