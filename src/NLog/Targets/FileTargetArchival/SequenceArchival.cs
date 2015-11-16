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
using System.Collections.Generic;
using System.Globalization;
using NLog.Common;

namespace NLog.Targets.FileTargetArchival
{
    /// <summary>
    /// Provides archive behavior with sequential numeric filenames.
    /// </summary>
    class SequenceArchival
    {
        /// <summary>
        /// Provides archival options and context.
        /// </summary>
        public Archival Archival { get; set; }

        /// <summary>
        /// Archives the <paramref name="fileName"/> using a sequence style numbering. The most recent archive has the
        /// highest number. When the number of archive files exceed <see cref="P:MaxArchiveFiles"/> the obsolete
        /// archives are deleted.
        /// </summary>
        /// <param name="fileName">File name to be archived.</param>
        /// <param name="pattern">File name template which contains the numeric pattern to be replaced.</param>
        public void ArchiveBySequence(string fileName, string pattern)
        {
            FileNameTemplate fileTemplate = new FileNameTemplate(Path.GetFileName(pattern));
            int trailerLength = fileTemplate.Template.Length - fileTemplate.EndAt;
            string fileNameMask = fileTemplate.ReplacePattern("*");

            string dirName = Path.GetDirectoryName(Path.GetFullPath(pattern));
            int nextNumber = -1;
            int minNumber = -1;

            var number2Name = new Dictionary<int, string>();

            try
            {
#if SILVERLIGHT
                foreach (string s in Directory.EnumerateFiles(dirName, fileNameMask))
#else
                foreach (string s in Directory.GetFiles(dirName, fileNameMask))
#endif
                {
                    string baseName = Path.GetFileName(s);
                    string number = baseName.Substring(fileTemplate.BeginAt, baseName.Length - trailerLength - fileTemplate.BeginAt);
                    int num;

                    try
                    {
                        num = Convert.ToInt32(number, CultureInfo.InvariantCulture);
                    }
                    catch (FormatException)
                    {
                        continue;
                    }

                    nextNumber = Math.Max(nextNumber, num);
                    minNumber = minNumber != -1 ? Math.Min(minNumber, num) : num;

                    number2Name[num] = s;
                }

                nextNumber++;
            }
            catch (DirectoryNotFoundException)
            {
                Directory.CreateDirectory(dirName);
                nextNumber = 0;
            }

            if (minNumber != -1 && Archival.Options.MaxArchiveFiles != 0)
            {
                int minNumberToKeep = nextNumber - Archival.Options.MaxArchiveFiles + 1;
                for (int i = minNumber; i < minNumberToKeep; ++i)
                {
                    string s;

                    if (number2Name.TryGetValue(i, out s))
                    {
                        InternalLogger.Info("Deleting old archive {0}", s);
                        File.Delete(s);
                    }
                }
            }

            string newFileName = FileNameTemplate.ReplaceNumberPattern(pattern, nextNumber);
            Archival.RollArchiveForward(fileName, newFileName, allowCompress: true);
        }
    }
}
