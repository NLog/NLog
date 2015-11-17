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
#if !SILVERLIGHT
using System.IO.Compression;
#endif
using System.Collections.Generic;
using System.Globalization;
using NLog.Time;
using NLog.Common;

namespace NLog.Targets.FileTargetArchival
{
    /// <summary>
    /// Top-level class for handling log file archival.
    /// </summary>
    class Archival
    {
        /// <summary>
        /// This value disables file archiving based on the size. 
        /// </summary>
        public const int ArchiveAboveSizeDisabled = -1;

        /// <summary>
        /// The options that determine archival behavior.
        /// </summary>
        public IArchivalOptions Options { get; private set; }

        /// <summary>
        /// Provides notifications or gets context-sensitive info from
        /// the component that requests archival functionality.
        /// </summary>
        public IArchivalCallbacks Callbacks { get; private set; }

        /// <summary>
        /// Provides archive behavior for corner cases.
        /// TODO: it's defective. Remove and handle those corner cases in normal code paths.
        /// </summary>
        private DynamicFileArchive dynamicFileArchive;

        /// <summary>
        /// Provides archive behavior with rolling numeric filenames.
        /// </summary>
        private RollingArchival rollingArchival;

        /// <summary>
        /// Provides archive behavior with sequential numeric filenames.
        /// </summary>
        private SequenceArchival sequenceArchival;

        /// <summary>
        /// Provides archive behavior with date-time filenames.
        /// </summary>
        private DateArchival dateArchival;

        /// <summary>
        /// Provides archive behavior with date-time and numeric filenames.
        /// </summary>
        private DateAndSequenceArchival dateAndSequenceArchival;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="options">The archival options to use.</param>
        /// <param name="callbacks">The archival callbacks to use.</param>
        public Archival(IArchivalOptions options, IArchivalCallbacks callbacks)
        {
            this.Options = options;
            this.Callbacks = callbacks;
            this.dynamicFileArchive = new DynamicFileArchive() { Archival = this };
            this.rollingArchival = new RollingArchival() { Archival = this };
            this.sequenceArchival = new SequenceArchival() { Archival = this };
            this.dateArchival = new DateArchival(options.MaxArchiveFiles) { Archival = this };
            this.dateAndSequenceArchival = new DateAndSequenceArchival() { Archival = this };
        }

        /// <summary>
        /// Gets a value indicating whether log file archival is configured to occur.
        /// </summary>
        public bool Enabled
        {
            get
            {
                return this.Options.ArchiveAboveSize != ArchiveAboveSizeDisabled ||
                       this.Options.ArchiveEvery != FileArchivePeriod.None;
            }
        }

        /// <summary>
        /// Causes a file to be archived. Usually this means it's renamed.
        /// </summary>
        /// <param name="fileName">File name to be checked and archived.</param>
        /// <param name="eventInfo">Log event that the <see cref="FileTarget"/> instance is currently processing.</param>
        public void DoAutoArchive(string fileName, LogEventInfo eventInfo)
        {
            FileInfo fileInfo = new FileInfo(fileName);
            if (!fileInfo.Exists)
            {
                return;
            }

            string fileNamePattern = GetFileNamePattern(fileName, eventInfo);

            if (!FileNameTemplate.ContainsFileNamePattern(fileNamePattern))
            {
                if (this.dynamicFileArchive.Archive(fileNamePattern, fileInfo.FullName, Options.CreateDirs, Options.EnableArchiveFileCompression))
                {
                    Callbacks.RemoveInitializedFileName(fileInfo.FullName);
                }
            }
            else
            {
                switch (Options.ArchiveNumbering)
                {
                    case ArchiveNumberingMode.Rolling:
                        this.rollingArchival.RecursiveRollingRename(fileInfo.FullName, fileNamePattern, 0);
                        break;

                    case ArchiveNumberingMode.Sequence:
                        this.sequenceArchival.ArchiveBySequence(fileInfo.FullName, fileNamePattern);
                        break;

#if !NET_CF
                    case ArchiveNumberingMode.Date:
                        this.dateArchival.ArchiveByDate(fileInfo.FullName, fileNamePattern);
                        break;

                    case ArchiveNumberingMode.DateAndSequence:
                        this.dateAndSequenceArchival.ArchiveByDateAndSequence(fileInfo.FullName, fileNamePattern, eventInfo);
                        break;
#endif
                }
            }
        }

        /// <summary>
        /// Determines whether the contents of a file should be archived
        /// before log event info is written to the file.
        /// </summary>
        /// <param name="fileName">File name to be written.</param>
        /// <param name="ev">Log event that the <see cref="FileTarget"/> instance is currently processing.</param>
        /// <param name="upcomingWriteSize">The size in bytes of the next chunk of data to be written in the file.</param>
        /// <returns><see langword="true"/> when archiving should be executed; <see langword="false"/> otherwise.</returns>
        public bool ShouldAutoArchive(string fileName, LogEventInfo ev, int upcomingWriteSize)
        {
            return ShouldAutoArchiveBasedOnFileSize(fileName, upcomingWriteSize) ||
                   ShouldAutoArchiveBasedOnTime(fileName, ev);
        }

        /// <summary>
        /// Indicates if the automatic archiving process should be executed based on file size constraints.
        /// </summary>
        /// <param name="fileName">File name to be written.</param>
        /// <param name="upcomingWriteSize">The size in bytes of the next chunk of data to be written in the file.</param>
        /// <returns><see langword="true"/> when archiving should be executed; <see langword="false"/> otherwise.</returns>
        private bool ShouldAutoArchiveBasedOnFileSize(string fileName, int upcomingWriteSize)
        {
            if (this.Options.ArchiveAboveSize == ArchiveAboveSizeDisabled)
            {
                return false;
            }

            DateTime lastWriteTime;
            long fileLength;

            if (!Callbacks.GetFileInfo(fileName, out lastWriteTime, out fileLength))
            {
                return false;
            }

            if (Options.ArchiveAboveSize != ArchiveAboveSizeDisabled)
            {
                if (fileLength + upcomingWriteSize > Options.ArchiveAboveSize)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Indicates if the automatic archiving process should be executed based on date/time constraints.
        /// </summary>
        /// <param name="fileName">File name to be written.</param>
        /// <param name="logEvent">Log event that the <see cref="FileTarget"/> instance is currently processing.</param>
        /// <returns><see langword="true"/> when archiving should be executed; <see langword="false"/> otherwise.</returns>
        private bool ShouldAutoArchiveBasedOnTime(string fileName, LogEventInfo logEvent)
        {
            if (Options.ArchiveEvery == FileArchivePeriod.None)
            {
                return false;
            }

            DateTime lastWriteTime;
            long fileLength;

            if (!Callbacks.GetFileInfo(fileName, out lastWriteTime, out fileLength))
            {
                return false;
            }

            if (Options.ArchiveEvery != FileArchivePeriod.None)
            {
                // file write time is in Utc and logEvent's timestamp is originated from TimeSource.Current,
                // so we should ask the TimeSource to convert file time to TimeSource time:
                lastWriteTime = TimeSource.Current.FromSystemTime(lastWriteTime);
                string formatString = DateArchivalCommon.GetDateFormatString(string.Empty, Options.ArchiveEvery);
                string fileLastChanged = lastWriteTime.ToString(formatString, CultureInfo.InvariantCulture);
                string logEventRecorded = logEvent.TimeStamp.ToString(formatString, CultureInfo.InvariantCulture);

                if (fileLastChanged != logEventRecorded)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Deletes old archives if necessary before writing to the given log file.
        /// </summary>
        /// <param name="fileName">The log filename about to be written-to.</param>
        /// <param name="logEvent">The logging event.</param>
        public void CheckToDeleteOldArchives(string fileName, LogEventInfo logEvent)
        {
            // only date/time-based archives do this
            if (Options.ArchiveNumbering == ArchiveNumberingMode.Date)
            {
                this.dateArchival.CheckToDeleteOldDateArchives(fileName, logEvent);
            }
        }

        /// <summary>
        /// Creates an archive copy of source file either by compressing it or moving to a new location in the filesystem,
        /// and fires callback to maintain <see cref="P:FileTarget:initializedFiles"/>
        /// </summary>
        /// <param name="existingFileName">File name to be archived.</param>
        /// <param name="archiveFileName">Name of the archive file.</param>
        /// <param name="allowCompress">Determines whether compression is allowed.
        /// This value is only respected if <see cref="P:Options:EnableArchiveFileCompression"/> is <c>true</c>.</param>
        public void RollArchiveForward(string existingFileName, string archiveFileName, bool allowCompress)
        {
            ArchiveFile(existingFileName, archiveFileName, allowCompress && Options.EnableArchiveFileCompression);
            Callbacks.RemoveInitializedFileNameOrPath(existingFileName);
        }

        /// <summary>
        /// Creates an archive copy of source file either by compressing it or moving to a new location in the file
        /// system. Which action will be used is determined by the value of <paramref name="enableCompression"/> parameter.
        /// </summary>
        /// <param name="fileName">File name to be archived.</param>
        /// <param name="archiveFileName">Name of the archive file.</param>
        /// <param name="enableCompression">Enables file compression</param>
        public void ArchiveFile(string fileName, string archiveFileName, bool enableCompression)
        {
            string archiveFolderPath = Path.GetDirectoryName(archiveFileName);
            if (!Directory.Exists(archiveFolderPath))
                Directory.CreateDirectory(archiveFolderPath);

#if NET4_5
            if (enableCompression)
            {
                InternalLogger.Info("Archiving {0} to zip-archive {1}", fileName, archiveFileName);
                using (var archiveStream = new FileStream(archiveFileName, FileMode.Create))
                using (var archive = new ZipArchive(archiveStream, ZipArchiveMode.Create))
                using (var originalFileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    var zipArchiveEntry = archive.CreateEntry(Path.GetFileName(fileName));
                    using (var destination = zipArchiveEntry.Open())
                    {
                        originalFileStream.CopyTo(destination);
                    }
                }

                File.Delete(fileName);
            }
            else
#endif
            {
                InternalLogger.Info("Archiving {0} to {1}", fileName, archiveFileName);
                File.Move(fileName, archiveFileName);
            }
        }

        /// <summary>
        /// Gets the pattern that archive files will match
        /// </summary>
        /// <param name="fileName">Filename of the log file</param>
        /// <param name="eventInfo">Log event that the <see cref="FileTarget"/> instance is currently processing.</param>
        /// <returns>A string with a pattern that will match the archive filenames</returns>
        public string GetFileNamePattern(string fileName, LogEventInfo eventInfo)
        {
            string fileNamePattern;

            FileInfo fileInfo = new FileInfo(fileName);

            if (Options.ArchiveFileName == null)
            {
                string ext = Options.EnableArchiveFileCompression ? ".zip" : Path.GetExtension(fileName);
                fileNamePattern = Path.ChangeExtension(fileInfo.FullName, ".{#}" + ext);
            }
            else
            {
                //The archive file name is given. There are two possibilities
                //(1) User supplied the Filename with pattern
                //(2) User supplied the normal filename
                fileNamePattern = Options.ArchiveFileName.Render(eventInfo);
                fileNamePattern = FileTarget.CleanupInvalidFileNameChars(fileNamePattern);
            }
            return fileNamePattern;
        }
    }
}
