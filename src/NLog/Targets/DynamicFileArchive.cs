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
    using Common;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    internal sealed class DynamicFileArchive : BaseFileArchive
    {
        private readonly Queue<string> fileQueue = new Queue<string>();

        public DynamicFileArchive(FileTarget target) : base(target) { }

        /// <summary>
        /// Adds a file into archive.
        /// </summary>
        /// <param name="archiveFileName">File name of the archive</param>
        /// <param name="fileName">Original file name</param>
        /// <param name="createDirectory">Create a directory, if it does not exist</param>
        /// <param name="shouldCompress">Enables file compression</param>
        /// <returns><c>true</c> if the file has been moved successfully; <c>false</c> otherwise</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public bool Process(string archiveFileName, string fileName, bool createDirectory, bool shouldCompress)
        {
            if (Size < 1)
            {
                InternalLogger.Warn("Archive process is called. Even though the Size property is set to less than 1.");
                return false;
            }

            if (!File.Exists(fileName))
            {
                InternalLogger.Error("Source file '{0}' not found.", fileName);
                return false;
            }

            DeleteOldArchiveFiles();
            AddArchive(archiveFileName, fileName, createDirectory, shouldCompress);
            fileQueue.Enqueue(archiveFileName);
            return true;
        }

        /// <summary>
        /// Deletes the specified file and logs a message to internal logger if the action fails. 
        /// </summary>
        /// <param name="fileName">Filename to be deleted</param>
        private static void DeleteFile(string fileName)
        {
            try
            {
                File.Delete(fileName);
            }
            catch (Exception ex)
            {
                InternalLogger.Warn("Cannot delete old archive file: {0}, Exception : {1}", fileName, ex);
            }
        }

        private void AddArchive(string archiveFileName, string fileName, bool createDirectory, bool shouldCompress)
        {
            String alternativeFileName = archiveFileName;

            if (fileQueue.Contains(archiveFileName))
            {
                InternalLogger.Trace("AddToArchive file {0} already exist. Trying different file name.", archiveFileName);
                alternativeFileName = FindSuitableFilename(archiveFileName, numberToStartWith: 1);
            }

            try
            {
                ArchiveFile(fileName, alternativeFileName, shouldCompress);
            }
            catch (DirectoryNotFoundException)
            {
                if (createDirectory)
                {
                    InternalLogger.Trace("AddToArchive directory not found. Creating {0}", Path.GetDirectoryName(archiveFileName));

                    try
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(archiveFileName));
                        ArchiveFile(fileName, alternativeFileName, shouldCompress);
                    }
                    catch (Exception ex)
                    {
                        InternalLogger.Error("Cannot create archive directory, Exception : {0}", ex);
                        throw;
                    }
                }
                else
                {
                    throw;
                }
            }
            catch (Exception ex)
            {
                InternalLogger.Error("Cannot archive file {0}, Exception : {1}", fileName, ex);
                throw;
            }
        }

        /// <summary>
        /// Remove old archive files when the files on the queue are more than the 
        /// MaxArchiveFilesToKeep.  
        /// </summary>
        private void DeleteOldArchiveFiles()
        {
            // TODO: When the Size = 1 than ONLY a single file will be deleted. Is this the intended behavior? 
            if (Size == 1 && fileQueue.Any())
            {
                string archiveFileName = fileQueue.Dequeue();
                DeleteFile(archiveFileName);
            }

            while (fileQueue.Count >= Size)
            {
                string oldestArchivedFileName = fileQueue.Dequeue();
                DeleteFile(oldestArchivedFileName);
            }
        }

        /// <summary>
        /// Creates a new unique filename by appending a number to it. This method tests that 
        /// the filename created does not exist.
        /// 
        /// This process can be slow as it increments the number sequentially from a specified 
        /// starting point until it finds a number which produces a filename which does not 
        /// exist.
        /// 
        /// Example: 
        ///     Original Filename   trace.log
        ///     Target Filename     trace.15.log
        /// </summary>          
        /// <param name="fileName">Original filename</param>
        /// <param name="numberToStartWith">Number starting point</param>
        /// <returns>File name suitable for archiving</returns>
        private string FindSuitableFilename(string fileName, int numberToStartWith)
        {
            String targetFileName = Path.GetFileNameWithoutExtension(fileName) + ".{#}" + Path.GetExtension(fileName);

            while (File.Exists(ReplaceNumbericPattern(targetFileName, numberToStartWith)))
            {
                InternalLogger.Trace("Archive file '{0}' already exist. Trying a different file name.", fileName);
                numberToStartWith++;
            }
            return targetFileName;
        }
    }
}
