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

namespace NLog.Targets
{
#if NET4_5
using System.IO;
using System.IO.Compression;

    /// <summary>
    /// Builtin IFileCompressor implementation utilizing the .Net4.5 specific <see cref="ZipArchive"/> 
    /// and is used as the default value for <see cref="FileTarget.FileCompressor"/> on .Net4.5.
    /// So log files created via <see cref="FileTarget"/> can be zipped when archived
    /// w/o 3rd party zip library when run on .Net4.5 or higher.
    /// </summary>
    internal class ZipArchiveFileCompressor : IFileCompressor
    {
        /// <summary>
        /// Implements <see cref="IFileCompressor.CompressFile(string, string)"/> using the .Net4.5 specific <see cref="ZipArchive"/>
        /// </summary>
        public void CompressFile(string fileName, string archiveFileName)
        {
            using (var archiveStream = new FileStream(archiveFileName, FileMode.Create))
            using (var archive = new ZipArchive(archiveStream, ZipArchiveMode.Create))
            using (var originalFileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite ))
            {
                var zipArchiveEntry = archive.CreateEntry(Path.GetFileName(fileName));
                using (var destination = zipArchiveEntry.Open())
                {
                    originalFileStream.CopyTo(destination);
                }
            }
        }
    }
#endif
}
