// 
// Copyright (c) 2004-2011 Jaroslaw Kowalski <jaak@jkowalski.net>
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
using System.IO;

namespace NLog.Targets.FileTargetArchival
{
    /// <summary>
    /// Provides archive behavior with rolling numeric filenames.
    /// </summary>
    class RollingArchival
    {
        /// <summary>
        /// Provides archival options and context.
        /// </summary>
        public Archival Archival { get; set; }

        /// <summary>
        /// Archives the <paramref name="fileName"/> using a rolling style numbering (the most recent is always #0 then
        /// #1, ..., #N. When the number of archive files exceed <see cref="P:MaxArchiveFiles"/> the obsolete archives
        /// are deleted.
        /// </summary>
        /// <remarks>
        /// This method is called recursively. This is the reason the <paramref name="archiveNumber"/> is required.
        /// </remarks>
        /// <param name="fileName">File name to be archived.</param>
        /// <param name="pattern">File name template which contains the numeric pattern to be replaced.</param>
        /// <param name="archiveNumber">Value which will replace the numeric pattern.</param>
        public void RecursiveRollingRename(string fileName, string pattern, int archiveNumber)
        {
            if (Archival.Options.MaxArchiveFiles > 0 && archiveNumber >= Archival.Options.MaxArchiveFiles)
            {
                File.Delete(fileName);
                return;
            }

            if (!File.Exists(fileName))
            {
                return;
            }

            string newFileName = FileNameTemplate.ReplaceNumberPattern(pattern, archiveNumber);
            RecursiveRollingRename(newFileName, pattern, archiveNumber + 1);

            var shouldCompress = archiveNumber == 0;
            try
            {
                Archival.RollArchiveForward(fileName, newFileName, shouldCompress);
            }
            catch (IOException)
            {
                // TODO: Check the value of CreateDirs property before creating directories.
                string dir = Path.GetDirectoryName(newFileName);
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                Archival.RollArchiveForward(fileName, newFileName, shouldCompress);
            }
        }
    }
}
