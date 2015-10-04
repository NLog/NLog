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
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;

#if !NET_CF
    internal class DateFileArchive : DateBasedFileArchive
    {
        public DateFileArchive(FileTarget target) : base(target) { }

        /// <summary>
        /// Gets the way file archives are numbered from this particular class. 
        /// </summary>
        public ArchiveNumberingMode ArchiveNumbering
        {
            get { return ArchiveNumberingMode.Date; }
        }

        public void Process(string fileName, string pattern)
        {
            string fileNameMask = ReplaceFileNamePattern(pattern, "*");
            string dirName = Path.GetDirectoryName(Path.GetFullPath(pattern));
            string dateFormat = GetDateFormatString(DateFormat);

            DeleteArchive(pattern);

            DateTime newFileDate = GetArchiveDate(true);
            if (dirName != null)
            {
                string newFileName = Path.Combine(dirName, fileNameMask.Replace("*", newFileDate.ToString(dateFormat)));
                RollArchiveForward(fileName, newFileName, shouldCompress: true);
            }
        }

        /// <summary>
        /// Deletes archive files in reverse chronological order until only a number equal to Size property of archive
        /// files remain.
        /// </summary>
        /// <param name="pattern">The pattern that archive filenames will match</param>
        public void DeleteArchive(string pattern)
        {
            string fileNameMask = ReplaceFileNamePattern(pattern, "*");
            string dirName = Path.GetDirectoryName(Path.GetFullPath(pattern));
            string dateFormat = GetDateFormatString(this.DateFormat);

            try
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(dirName);
#if SILVERLIGHT
                List<string> files = directoryInfo.EnumerateFiles(fileNameMask).OrderBy(n => n.CreationTime).Select(n => n.FullName).ToList();
#else
                List<string> files = directoryInfo.GetFiles(fileNameMask).OrderBy(n => n.CreationTime).Select(n => n.FullName).ToList();
#endif
                List<string> filesByDate = new List<string>();

                for (int index = 0; index < files.Count; index++)
                {
                    string archiveFileName = Path.GetFileName(files[index]);
                    string datePart = archiveFileName.Substring(fileNameMask.LastIndexOf('*'), dateFormat.Length);
                    DateTime fileDate = DateTime.MinValue;
                    if (DateTime.TryParseExact(datePart, dateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out fileDate))
                    {
                        filesByDate.Add(files[index]);
                    }
                }

                EnsureArchiveCount(filesByDate);
            }
            catch (DirectoryNotFoundException)
            {
                Directory.CreateDirectory(dirName);
            }
        }
    }
#endif
}
