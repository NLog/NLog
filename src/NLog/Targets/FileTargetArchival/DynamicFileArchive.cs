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
using System.Linq;
using NLog.Common;

namespace NLog.Targets.FileTargetArchival
{
    /// <summary>
    /// Provides archive behavior for corner cases.
    /// TODO: it's defective. Remove and handle those corner cases in normal code paths.
    /// </summary>
    class DynamicFileArchive
    {
        /// <summary>
        /// Provides archival options and context.
        /// </summary>
        public Archival Archival { get; set; }

        /// <summary>
        /// Adds a file into archive.
        /// </summary>
        /// <param name="archiveFileName">File name of the archive</param>
        /// <param name="fileName">Original file name</param>
        /// <param name="createDirectory">Create a directory, if it does not exist</param>
        /// <param name="enableCompression">Enables file compression</param>
        /// <returns><see langword="true"/> if the file has been moved successfully; <see langword="false"/> otherwise.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public bool Archive(string archiveFileName, string fileName, bool createDirectory, bool enableCompression)
        {
            if (Archival.Options.MaxArchiveFiles < 1)
            {
                InternalLogger.Warn("Archive is called. Even though the MaxArchiveFiles is set to less than 1");
                return false;
            }

            if (!File.Exists(fileName))
            {
                InternalLogger.Error("Error while archiving, Source File : {0} Not found.", fileName);
                return false;
            }

            DeleteOldArchiveFiles();
            AddToArchive(archiveFileName, fileName, createDirectory, enableCompression);
            archiveFileQueue.Enqueue(archiveFileName);
            return true;
        }

        private readonly Queue<string> archiveFileQueue = new Queue<string>();

        /// <summary>
        /// Archives the file, either by copying it to a new file system location or by compressing it, and add the file name into the list of archives.
        /// </summary>
        /// <param name="archiveFileName">Target file name.</param>
        /// <param name="fileName">Original file name.</param>
        /// <param name="createDirectory">Create a directory, if it does not exist.</param>
        /// <param name="enableCompression">Enables file compression.</param>
        private void AddToArchive(string archiveFileName, string fileName, bool createDirectory, bool enableCompression)
        {
            String alternativeFileName = archiveFileName;

            if (archiveFileQueue.Contains(archiveFileName))
            {
                InternalLogger.Trace("AddToArchive file {0} already exist. Trying different file name.", archiveFileName);
                alternativeFileName = FindSuitableFilename(archiveFileName, 1);
            }

            try
            {
                Archival.ArchiveFile(fileName, alternativeFileName, enableCompression);
            }
            catch (DirectoryNotFoundException)
            {
                if (createDirectory)
                {
                    InternalLogger.Trace("AddToArchive directory not found. Creating {0}", Path.GetDirectoryName(archiveFileName));

                    try
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(archiveFileName));
                        Archival.ArchiveFile(fileName, alternativeFileName, enableCompression);
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
        /// Remove old archive files when the files on the queue are more than the <see cref="P:MaxArchiveFilesToKeep"/>.
        /// </summary>
        private void DeleteOldArchiveFiles()
        {
            if (Archival.Options.MaxArchiveFiles == 1 && archiveFileQueue.Any())
            {
                var archiveFileName = archiveFileQueue.Dequeue();

                try
                {
                    File.Delete(archiveFileName);
                }
                catch (Exception ex)
                {
                    InternalLogger.Warn("Cannot delete old archive file : {0} , Exception : {1}", archiveFileName, ex);
                }
            }

            while (archiveFileQueue.Count >= Archival.Options.MaxArchiveFiles)
            {
                string oldestArchivedFileName = archiveFileQueue.Dequeue();

                try
                {
                    File.Delete(oldestArchivedFileName);
                }
                catch (Exception ex)
                {
                    InternalLogger.Warn("Cannot delete old archive file : {0} , Exception : {1}", oldestArchivedFileName, ex);
                }
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
        /// <param name="fileName">Original file name.</param>
        /// <param name="numberToStartWith">Number starting point</param>
        /// <returns>File name suitable for archiving</returns>
        private string FindSuitableFilename(string fileName, int numberToStartWith)
        {
            String targetFileName = Path.GetFileNameWithoutExtension(fileName) + ".{#}" + Path.GetExtension(fileName);

            while (File.Exists(FileNameTemplate.ReplaceNumberPattern(targetFileName, numberToStartWith)))
            {
                InternalLogger.Trace("AddToArchive file {0} already exist. Trying with different file name.", fileName);
                numberToStartWith++;
            }
            return targetFileName;
        }
    }
}
