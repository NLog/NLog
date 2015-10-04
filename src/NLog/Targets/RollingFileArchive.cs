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

namespace NLog.Targets
{
    using System.IO;
    using NLog.Common;

    internal class RollingFileArchive : BaseFileArchive
    {
        public RollingFileArchive(FileTarget target) : base(target) { }

        public void RollingArchive(string fileName, string pattern, int archiveNumber)
        {
            if (Size > 0 && archiveNumber >= Size)
            {
                File.Delete(fileName);
                return;
            }

            if (!File.Exists(fileName))
            {
                return;
            }

            string newFileName = ReplaceNumberPattern(pattern, archiveNumber);
            if (File.Exists(fileName))
            {
                RollingArchive(newFileName, pattern, archiveNumber + 1);
            }

            InternalLogger.Trace("Renaming {0} to {1}", fileName, newFileName);

            var shouldCompress = (archiveNumber == 0);
            try
            {
                RollArchiveForward(fileName, newFileName, shouldCompress);
            }
            catch (IOException)
            {
                // TODO: Check the value of CreateDirs property before creating directories.
                string dir = Path.GetDirectoryName(newFileName);
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                RollArchiveForward(fileName, newFileName, shouldCompress);
            }
        }
    }

}
