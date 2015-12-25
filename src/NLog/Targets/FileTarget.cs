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
    using System.ComponentModel;
    using System.Globalization;
    using System.IO;
#if !SILVERLIGHT
    using System.IO.Compression;
#endif
    using System.Linq;
    using System.Text;
    using System.Threading;
    using Common;
    using Config;
    using Internal;
    using Internal.FileAppenders;
    using Layouts;
    using Time;

    /// <summary>
    /// Writes log messages to one or more files.
    /// </summary>
    /// <seealso href="https://github.com/nlog/nlog/wiki/File-target">Documentation on NLog Wiki</seealso>
    [Target("File")]
    public class FileTarget : TargetWithLayoutHeaderAndFooter, ICreateFileParameters
    {
        /// <summary>
        /// Default clean up period of the initilized files. When a file exceeds the clean up period is removed from the list.
        /// </summary>
        /// <remarks>Clean up period is defined in days.</remarks>
        private const int InitializedFilesCleanupPeriod = 2;

        /// <summary>
        /// The maximum number of initialised files at any one time. Once this number is exceeded clean up procedures
        /// are initiated to reduce the number of initialised files.
        /// </summary>
        private const int InitializedFilesCounterMax = 100;

        /// <summary>
        /// This value disables file archiving based on the size. 
        /// </summary>
        private const int ArchiveAboveSizeDisabled = -1;

        /// <summary>
        /// Cached directory separator char array to avoid memory allocation on each method call.
        /// </summary>
        private readonly static char[] DirectorySeparatorChars = new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar };

#if !SILVERLIGHT

        /// <summary>
        /// Cached invalid filenames char array to avoid memory allocation everytime Path.GetInvalidFileNameChars() is called.
        /// </summary>
        private readonly static char[] InvalidFileNameChars = Path.GetInvalidFileNameChars();

#endif 
        /// <summary>
        /// Holds the initialised files each given time by the <see cref="FileTarget"/> instance. Against each file, the last write time is stored. 
        /// </summary>
        /// <remarks>Last write time is store in local time (no UTC).</remarks>
        private readonly Dictionary<string, DateTime> initializedFiles = new Dictionary<string, DateTime>();

        private LineEndingMode lineEndingMode = LineEndingMode.Default;

        /// <summary>
        /// Factory used to create the file appeanders in the <see cref="FileTarget"/> instance. 
        /// </summary>
        /// <remarks>File appenders are stored in an instance of <see cref="FileAppenderCache"/>.</remarks>
        private IFileAppenderFactory appenderFactory;

        /// <summary>
        /// List of the associated file appenders with the <see cref="FileTarget"/> instance.
        /// </summary>
        private FileAppenderCache recentAppenders;

        private Timer autoClosingTimer;

        /// <summary>
        /// The number of initialised files at any one time.
        /// </summary>
        private int initializedFilesCounter;

        /// <summary>
        /// The maximum number of archive files that should be kept.
        /// </summary>
        private int maxArchiveFiles;

        private readonly DynamicFileArchive fileArchive;

        /// <summary>
        /// It holds the file names of existing archives in order for the oldest archives to be removed when the list of
        /// filenames becomes too long.
        /// </summary>
        private Queue<string> previousFileNames;

        /// <summary>
        /// The filename as target
        /// </summary>
        private Layout fileName;

        /// <summary>
        /// The archive file name as target
        /// </summary>
        private Layout archiveFileName;

        /// <summary>
        /// The filename if <see cref="FileName"/> is a fixed string
        /// </summary>
        private string cachedCleanedFileNamed;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileTarget" /> class.
        /// </summary>
        /// <remarks>
        /// The default value of the layout is: <code>${longdate}|${level:uppercase=true}|${logger}|${message}</code>
        /// </remarks>
        public FileTarget()
        {
            this.ArchiveNumbering = ArchiveNumberingMode.Sequence;
            this.maxArchiveFiles = 0;
            this.ConcurrentWriteAttemptDelay = 1;
            this.ArchiveEvery = FileArchivePeriod.None;
            this.ArchiveAboveSize = FileTarget.ArchiveAboveSizeDisabled;
            this.ConcurrentWriteAttempts = 10;
            this.ConcurrentWrites = true;
#if SILVERLIGHT
            this.Encoding = Encoding.UTF8;
#else
            this.Encoding = Encoding.Default;
#endif
            this.BufferSize = 32768;
            this.AutoFlush = true;
#if !SILVERLIGHT
            this.FileAttributes = Win32FileAttributes.Normal;
#endif
            this.LineEnding = LineEndingMode.Default;
            this.EnableFileDelete = true;
            this.OpenFileCacheTimeout = -1;
            this.OpenFileCacheSize = 5;
            this.CreateDirs = true;
            this.fileArchive = new DynamicFileArchive(MaxArchiveFiles);
            this.ForceManaged = false;
            this.ArchiveDateFormat = string.Empty;

            this.maxLogFilenames = 20;
            this.previousFileNames = new Queue<string>(this.maxLogFilenames);
            this.recentAppenders = FileAppenderCache.Empty;
            this.CleanupFileName = true;
        }

        /// <summary>
        /// Gets or sets the name of the file to write to.
        /// </summary>
        /// <remarks>
        /// This FileName string is a layout which may include instances of layout renderers.
        /// This lets you use a single target to write to multiple files.
        /// </remarks>
        /// <example>
        /// The following value makes NLog write logging events to files based on the log level in the directory where
        /// the application runs.
        /// <code>${basedir}/${level}.log</code>
        /// All <c>Debug</c> messages will go to <c>Debug.log</c>, all <c>Info</c> messages will go to <c>Info.log</c> and so on.
        /// You can combine as many of the layout renderers as you want to produce an arbitrary log file name.
        /// </example>
        /// <docgen category='Output Options' order='1' />
        [RequiredParameter]
        public Layout FileName
        {
            get { return fileName; }
            set
            {
                var simpleLayout = value as SimpleLayout;
                if (simpleLayout != null && simpleLayout.IsFixedText)
                {
                    cachedCleanedFileNamed = CleanupInvalidFileNameChars(simpleLayout.FixedText);
                }
                else
                {
                    //clear cache
                    cachedCleanedFileNamed = null;
                }

                fileName = value;

                RefreshFileArchive();
            }
        }

        /// <summary>
        /// Cleanup invalid values in a filename, e.g. slashes in a filename. If set to <c>true</c>, this can impact the performance of massive writes. 
        /// If set to <c>false</c>, nothing gets written when the filename is wrong.
        /// </summary>
        [DefaultValue(true)]
        public bool CleanupFileName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to create directories if they do not exist.
        /// </summary>
        /// <remarks>
        /// Setting this to false may improve performance a bit, but you'll receive an error
        /// when attempting to write to a directory that's not present.
        /// </remarks>
        /// <docgen category='Output Options' order='10' />
        [DefaultValue(true)]
        [Advanced]
        public bool CreateDirs { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to delete old log file on startup.
        /// </summary>
        /// <remarks>
        /// This option works only when the "FileName" parameter denotes a single file.
        /// </remarks>
        /// <docgen category='Output Options' order='10' />
        [DefaultValue(false)]
        public bool DeleteOldFileOnStartup { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to replace file contents on each write instead of appending log message at the end.
        /// </summary>
        /// <docgen category='Output Options' order='10' />
        [DefaultValue(false)]
        [Advanced]
        public bool ReplaceFileContentsOnEachWrite { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to keep log file open instead of opening and closing it on each logging event.
        /// </summary>
        /// <remarks>
        /// Setting this property to <c>True</c> helps improve performance.
        /// </remarks>
        /// <docgen category='Performance Tuning Options' order='10' />
        [DefaultValue(false)]
        public bool KeepFileOpen { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of log filenames that should be stored as existing.
        /// </summary>
        /// <remarks>
        /// The bigger this number is the longer it will take to write each log record. The smaller the number is
        /// the higher the chance that the clean function will be run when no new files have been opened.
        /// </remarks>
        /// <docgen category='Performance Tuning Options' order='10' />
        [DefaultValue(20)] //NLog5: todo rename correct for text case
        public int maxLogFilenames { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to enable log file(s) to be deleted.
        /// </summary>
        /// <docgen category='Output Options' order='10' />
        [DefaultValue(true)]
        public bool EnableFileDelete { get; set; }

#if !SILVERLIGHT
        /// <summary>
        /// Gets or sets the file attributes (Windows only).
        /// </summary>
        /// <docgen category='Output Options' order='10' />
        [Advanced]
        public Win32FileAttributes FileAttributes { get; set; }
#endif

        /// <summary>
        /// Gets or sets the line ending mode.
        /// </summary>
        /// <docgen category='Layout Options' order='10' />
        [Advanced]
        public LineEndingMode LineEnding
        {
            get
            {
                return this.lineEndingMode;
            }

            set
            {
                this.lineEndingMode = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to automatically flush the file buffers after each log message.
        /// </summary>
        /// <docgen category='Performance Tuning Options' order='10' />
        [DefaultValue(true)]
        public bool AutoFlush { get; set; }

        /// <summary>
        /// Gets or sets the number of files to be kept open. Setting this to a higher value may improve performance
        /// in a situation where a single File target is writing to many files
        /// (such as splitting by level or by logger).
        /// </summary>
        /// <remarks>
        /// The files are managed on a LRU (least recently used) basis, which flushes
        /// the files that have not been used for the longest period of time should the
        /// cache become full. As a rule of thumb, you shouldn't set this parameter to 
        /// a very high value. A number like 10-15 shouldn't be exceeded, because you'd
        /// be keeping a large number of files open which consumes system resources.
        /// </remarks>
        /// <docgen category='Performance Tuning Options' order='10' />
        [DefaultValue(5)]
        [Advanced]
        public int OpenFileCacheSize { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of seconds that files are kept open. If this number is negative the files are 
        /// not automatically closed after a period of inactivity.
        /// </summary>
        /// <docgen category='Performance Tuning Options' order='10' />
        [DefaultValue(-1)]
        [Advanced]
        public int OpenFileCacheTimeout { get; set; }

        /// <summary>
        /// Gets or sets the log file buffer size in bytes.
        /// </summary>
        /// <docgen category='Performance Tuning Options' order='10' />
        [DefaultValue(32768)]
        public int BufferSize { get; set; }

        /// <summary>
        /// Gets or sets the file encoding.
        /// </summary>
        /// <docgen category='Layout Options' order='10' />
        public Encoding Encoding { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether concurrent writes to the log file by multiple processes on the same host.
        /// </summary>
        /// <remarks>
        /// This makes multi-process logging possible. NLog uses a special technique
        /// that lets it keep the files open for writing.
        /// </remarks>
        /// <docgen category='Performance Tuning Options' order='10' />
        [DefaultValue(true)]
        public bool ConcurrentWrites { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether concurrent writes to the log file by multiple processes on different network hosts.
        /// </summary>
        /// <remarks>
        /// This effectively prevents files from being kept open.
        /// </remarks>
        /// <docgen category='Performance Tuning Options' order='10' />
        [DefaultValue(false)]
        public bool NetworkWrites { get; set; }

        /// <summary>
        /// Gets or sets the number of times the write is appended on the file before NLog
        /// discards the log message.
        /// </summary>
        /// <docgen category='Performance Tuning Options' order='10' />
        [DefaultValue(10)]
        [Advanced]
        public int ConcurrentWriteAttempts { get; set; }

        /// <summary>
        /// Gets or sets the delay in milliseconds to wait before attempting to write to the file again.
        /// </summary>
        /// <remarks>
        /// The actual delay is a random value between 0 and the value specified
        /// in this parameter. On each failed attempt the delay base is doubled
        /// up to <see cref="ConcurrentWriteAttempts" /> times.
        /// </remarks>
        /// <example>
        /// Assuming that ConcurrentWriteAttemptDelay is 10 the time to wait will be:<p/>
        /// a random value between 0 and 10 milliseconds - 1st attempt<br/>
        /// a random value between 0 and 20 milliseconds - 2nd attempt<br/>
        /// a random value between 0 and 40 milliseconds - 3rd attempt<br/>
        /// a random value between 0 and 80 milliseconds - 4th attempt<br/>
        /// ...<p/>
        /// and so on.
        /// </example>
        /// <docgen category='Performance Tuning Options' order='10' />
        [DefaultValue(1)]
        [Advanced]
        public int ConcurrentWriteAttemptDelay { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to archive old log file on startup.
        /// </summary>
        /// <remarks>
        /// This option works only when the "FileName" parameter denotes a single file.
        /// After archiving the old file, the current log file will be empty.
        /// </remarks>
        /// <docgen category='Output Options' order='10' />
        [DefaultValue(false)]
        public bool ArchiveOldFileOnStartup { get; set; }

        /// <summary>
        /// Gets or sets a value specifying the date format to use when archving files.
        /// </summary>
        /// <remarks>
        /// This option works only when the "ArchiveNumbering" parameter is set either to Date or DateAndSequence.
        /// </remarks>
        /// <docgen category='Output Options' order='10' />
        [DefaultValue("")]
        public string ArchiveDateFormat { get; set; }

        /// <summary>
        /// Gets or sets the size in bytes above which log files will be automatically archived.
        /// 
        /// Warning: combining this with <see cref="ArchiveNumberingMode.Date"/> isn't supported. We cannot create multiple archive files, if they should have the same name.
        /// Choose:  <see cref="ArchiveNumberingMode.DateAndSequence"/> 
        /// </summary>
        /// <remarks>
        /// Caution: Enabling this option can considerably slow down your file 
        /// logging in multi-process scenarios. If only one process is going to
        /// be writing to the file, consider setting <c>ConcurrentWrites</c>
        /// to <c>false</c> for maximum performance.
        /// </remarks>
        /// <docgen category='Archival Options' order='10' />
        public long ArchiveAboveSize { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to automatically archive log files every time the specified time passes.
        /// </summary>
        /// <remarks>
        /// Files are moved to the archive as part of the write operation if the current period of time changes. For example
        /// if the current <c>hour</c> changes from 10 to 11, the first write that will occur
        /// on or after 11:00 will trigger the archiving.
        /// <p>
        /// Caution: Enabling this option can considerably slow down your file 
        /// logging in multi-process scenarios. If only one process is going to
        /// be writing to the file, consider setting <c>ConcurrentWrites</c>
        /// to <c>false</c> for maximum performance.
        /// </p>
        /// </remarks>
        /// <docgen category='Archival Options' order='10' />
        public FileArchivePeriod ArchiveEvery { get; set; }

        /// <summary>
        /// Gets or sets the name of the file to be used for an archive.
        /// </summary>
        /// <remarks>
        /// It may contain a special placeholder {#####}
        /// that will be replaced with a sequence of numbers depending on 
        /// the archiving strategy. The number of hash characters used determines
        /// the number of numerical digits to be used for numbering files.
        /// </remarks>
        /// <docgen category='Archival Options' order='10' />
        public Layout ArchiveFileName
        {
            get { return archiveFileName; }
            set
            {
                archiveFileName = value;
                RefreshFileArchive();
            }
        }

        /// <summary>
        /// Gets or sets the maximum number of archive files that should be kept.
        /// </summary>
        /// <docgen category='Archival Options' order='10' />
        [DefaultValue(0)]
        public int MaxArchiveFiles
        {
            get
            {
                return maxArchiveFiles;
            }
            set
            {
                maxArchiveFiles = value;
                fileArchive.MaxArchiveFileToKeep = value;
            }
        }

        /// <summary>
        /// Gets or sets the way file archives are numbered. 
        /// </summary>
        /// <docgen category='Archival Options' order='10' />
        public ArchiveNumberingMode ArchiveNumbering { get; set; }

#if NET4_5
        /// <summary>
        /// Gets or sets a value indicating whether to compress archive files into the zip archive format.
        /// </summary>
        /// <docgen category='Archival Options' order='10' />
        [DefaultValue(false)]
        public bool EnableArchiveFileCompression { get; set; }
#else
        /// <summary>
        /// Gets or sets a value indicating whether to compress archive files into the zip archive format.
        /// </summary>
        private const bool EnableArchiveFileCompression = false;
#endif

        /// <summary>
        /// Gets or set a value indicating whether a managed file stream is forced, instead of used the native implementation.
        /// </summary>
        [DefaultValue(false)]
        public bool ForceManaged { get; set; }

        /// <summary>
        /// Gets the characters that are appended after each line.
        /// </summary>
        protected internal string NewLineChars
        {
            get
            {
                return lineEndingMode.NewLineCharacters;
            }
        }

        private void RefreshFileArchive()
        {
            var nullEvent = LogEventInfo.CreateNullEvent();
            string fileNamePattern = GetArchiveFileNamePattern(GetCleanedFileName(nullEvent), nullEvent);
            if (!ContainsFileNamePattern(fileNamePattern))
            {
                try
                {
                    fileArchive.InitializeForArchiveFolderPath(Path.GetDirectoryName(fileNamePattern));
                }
                catch (Exception exc)
                {
                    InternalLogger.Warn("Error while initializing archive folder: {0}.", exc);
                }
            }
        }

        /// <summary>
        /// Removes records of initialized files that have not been 
        /// accessed in the last two days.
        /// </summary>
        /// <remarks>
        /// Files are marked 'initialized' for the purpose of writing footers when the logging finishes.
        /// </remarks>
        public void CleanupInitializedFiles()
        {
            this.CleanupInitializedFiles(DateTime.UtcNow.AddDays(-FileTarget.InitializedFilesCleanupPeriod));
        }

        /// <summary>
        /// Removes records of initialized files that have not been
        /// accessed after the specified date.
        /// </summary>
        /// <param name="cleanupThreshold">The cleanup threshold.</param>
        /// <remarks>
        /// Files are marked 'initialized' for the purpose of writing footers when the logging finishes.
        /// </remarks>
        public void CleanupInitializedFiles(DateTime cleanupThreshold)
        {
            var filesToUninitialize = new List<string>();

            // Select the files require to be uninitialized.
            foreach (var file in this.initializedFiles)
            {
                if (file.Value < cleanupThreshold)
                {
                    filesToUninitialize.Add(file.Key);
                }
            }

            // Uninitialize the files.
            foreach (string fileName in filesToUninitialize)
            {
                this.WriteFooterAndUninitialize(fileName);
            }
        }

        /// <summary>
        /// Flushes all pending file operations.
        /// </summary>
        /// <param name="asyncContinuation">The asynchronous continuation.</param>
        /// <remarks>
        /// The timeout parameter is ignored, because file APIs don't provide
        /// the needed functionality.
        /// </remarks>
        protected override void FlushAsync(AsyncContinuation asyncContinuation)
        {
            try
            {
                recentAppenders.FlushAppenders();
                asyncContinuation(null);
            }
            catch (Exception exception)
            {
                if (exception.MustBeRethrown())
                {
                    throw;
                }

                asyncContinuation(exception);
            }
        }

        /// <summary>
        /// Returns the suitable appender factory ( <see cref="IFileAppenderFactory"/>) to be used to generate the file
        /// appenders associated with the <see cref="FileTarget"/> instance.
        /// 
        /// The type of the file appender factory returned depends on the values of various <see cref="FileTarget"/> properties.
        /// </summary>
        /// <returns><see cref="IFileAppenderFactory"/> suitable for this instance.</returns>
        private IFileAppenderFactory GetFileAppenderFactory()
        {
            if (!this.KeepFileOpen)
            {
                return RetryingMultiProcessFileAppender.TheFactory;
            }
            else
            {
                if (this.ArchiveAboveSize != FileTarget.ArchiveAboveSizeDisabled || this.ArchiveEvery != FileArchivePeriod.None)
                {
                    if (this.NetworkWrites)
                    {
                        return RetryingMultiProcessFileAppender.TheFactory;
                    }
                    else if (this.ConcurrentWrites)
                    {
#if SILVERLIGHT
                        return RetryingMultiProcessFileAppender.TheFactory;
#elif MONO
                        //
                        // mono on Windows uses mutexes, on Unix - special appender
                        //
                        if (PlatformDetector.IsUnix)
                        {
                            return UnixMultiProcessFileAppender.TheFactory;
                        }
                        else
                        {
                            return MutexMultiProcessFileAppender.TheFactory;
                        }
#else
                        return MutexMultiProcessFileAppender.TheFactory;
#endif
                    }
                    else
                    {
                        return CountingSingleProcessFileAppender.TheFactory;
                    }
                }
                else
                {
                    if (this.NetworkWrites)
                    {
                        return RetryingMultiProcessFileAppender.TheFactory;
                    }
                    else if (this.ConcurrentWrites)
                    {
#if SILVERLIGHT
                        return RetryingMultiProcessFileAppender.TheFactory;
#elif MONO
                        //
                        // mono on Windows uses mutexes, on Unix - special appender
                        //
                        if (PlatformDetector.IsUnix)
                        {
                            return UnixMultiProcessFileAppender.TheFactory;
                        }
                        else
                        {
                            return MutexMultiProcessFileAppender.TheFactory;
                        }
#else
                        return MutexMultiProcessFileAppender.TheFactory;
#endif
                    }
                    else
                    {
                        return SingleProcessFileAppender.TheFactory;
                    }
                }
            }
        }

        /// <summary>
        /// Initializes file logging by creating data structures that
        /// enable efficient multi-file logging.
        /// </summary>
        protected override void InitializeTarget()
        {
            base.InitializeTarget();
            this.appenderFactory = GetFileAppenderFactory();

            this.recentAppenders = new FileAppenderCache(this.OpenFileCacheSize, this.appenderFactory, this);

            if ((this.OpenFileCacheSize > 0 || this.EnableFileDelete) && this.OpenFileCacheTimeout > 0)
            {
                this.autoClosingTimer = new Timer(
                    this.AutoClosingTimerCallback,
                    null,
                    this.OpenFileCacheTimeout * 1000,
                    this.OpenFileCacheTimeout * 1000);
            }
        }

        /// <summary>
        /// Closes the file(s) opened for writing.
        /// </summary>
        protected override void CloseTarget()
        {
            base.CloseTarget();

            foreach (string fileName in new List<string>(this.initializedFiles.Keys))
            {
                this.WriteFooterAndUninitialize(fileName);
            }

            if (this.autoClosingTimer != null)
            {
                this.autoClosingTimer.Change(Timeout.Infinite, Timeout.Infinite);
                this.autoClosingTimer.Dispose();
                this.autoClosingTimer = null;
            }

            this.recentAppenders.CloseAppenders();
        }

        /// <summary>
        /// Writes the specified logging event to a file specified in the FileName 
        /// parameter.
        /// </summary>
        /// <param name="logEvent">The logging event.</param>
        protected override void Write(LogEventInfo logEvent)
        {
            var fileName = GetCleanedFileName(logEvent);



            byte[] bytes = this.GetBytesToWrite(logEvent);

            if (this.ShouldAutoArchive(fileName, logEvent, bytes.Length))
            {
                this.recentAppenders.InvalidateAppender(fileName);
                this.DoAutoArchive(fileName, logEvent);
            }

            // Clean up old archives if this is the first time a log record is being written to
            // this log file and the archiving system is date/time based.
            if (this.ArchiveNumbering == ArchiveNumberingMode.Date && this.ArchiveEvery != FileArchivePeriod.None)
            {
                if (!previousFileNames.Contains(fileName))
                {
                    if (this.previousFileNames.Count > this.maxLogFilenames)
                    {
                        this.previousFileNames.Dequeue();
                    }

                    string fileNamePattern = this.GetArchiveFileNamePattern(fileName, logEvent);
                    this.DeleteOldDateArchives(fileNamePattern);
                    this.previousFileNames.Enqueue(fileName);
                }
            }

            this.WriteToFile(fileName, logEvent, bytes, false);
        }

        private string GetCleanedFileName(LogEventInfo logEvent)
        {
            return cachedCleanedFileNamed ?? CleanupInvalidFileNameChars(this.FileName.Render(logEvent));
        }

        /// <summary>
        /// Writes the specified array of logging events to a file specified in the FileName
        /// parameter.
        /// </summary>
        /// <param name="logEvents">An array of <see cref="AsyncLogEventInfo"/> objects.</param>
        /// <remarks>
        /// This function makes use of the fact that the events are batched by sorting
        /// the requests by filename. This optimizes the number of open/close calls
        /// and can help improve performance.
        /// </remarks>
        protected override void Write(AsyncLogEventInfo[] logEvents)
        {
            var buckets = logEvents.BucketSort(c => this.FileName.Render(c.LogEvent));
            using (var ms = new MemoryStream())
            {
                var pendingContinuations = new List<AsyncContinuation>();

                foreach (var bucket in buckets)
                {
                    string fileName = CleanupInvalidFileNameChars(bucket.Key);

                    ms.SetLength(0);
                    ms.Position = 0;

                    LogEventInfo firstLogEvent = null;

                    foreach (AsyncLogEventInfo ev in bucket.Value)
                    {
                        if (firstLogEvent == null)
                        {
                            firstLogEvent = ev.LogEvent;
                        }

                        byte[] bytes = this.GetBytesToWrite(ev.LogEvent);
                        ms.Write(bytes, 0, bytes.Length);
                        pendingContinuations.Add(ev.Continuation);
                    }

                    this.FlushCurrentFileWrites(fileName, firstLogEvent, ms, pendingContinuations);
                }
            }
        }

        /// <summary>
        /// Formats the log event for write.
        /// </summary>
        /// <param name="logEvent">The log event to be formatted.</param>
        /// <returns>A string representation of the log event.</returns>
        protected virtual string GetFormattedMessage(LogEventInfo logEvent)
        {
            return this.Layout.Render(logEvent);
        }

        /// <summary>
        /// Gets the bytes to be written to the file.
        /// </summary>
        /// <param name="logEvent">Log event.</param>
        /// <returns>Array of bytes that are ready to be written.</returns>
        protected virtual byte[] GetBytesToWrite(LogEventInfo logEvent)
        {
            string renderedText = this.GetFormattedMessage(logEvent) + this.NewLineChars;
            return this.TransformBytes(this.Encoding.GetBytes(renderedText));
        }

        /// <summary>
        /// Modifies the specified byte array before it gets sent to a file.
        /// </summary>
        /// <param name="value">The byte array.</param>
        /// <returns>The modified byte array. The function can do the modification in-place.</returns>
        protected virtual byte[] TransformBytes(byte[] value)
        {
            return value;
        }

        /// <summary>
        /// Replaces the numeric pattern i.e. {#} in a file name with the <paramref name="value"/> parameter value.
        /// </summary>
        /// <param name="pattern">File name which contains the numeric pattern.</param>
        /// <param name="value">Value which will replace the numeric pattern.</param>
        /// <returns>File name with the value of <paramref name="value"/> in the position of the numberic pattern.</returns>
        private static string ReplaceNumberPattern(string pattern, int value)
        {
            int firstPart = pattern.IndexOf("{#", StringComparison.Ordinal);
            int lastPart = pattern.IndexOf("#}", StringComparison.Ordinal) + 2;
            int numDigits = lastPart - firstPart - 2;

            return pattern.Substring(0, firstPart) + Convert.ToString(value, 10).PadLeft(numDigits, '0') + pattern.Substring(lastPart);
        }

        private void FlushCurrentFileWrites(string currentFileName, LogEventInfo firstLogEvent, MemoryStream ms, List<AsyncContinuation> pendingContinuations)
        {
            Exception lastException = null;

            try
            {
                if (currentFileName != null)
                {
                    if (this.ShouldAutoArchive(currentFileName, firstLogEvent, (int)ms.Length))
                    {
                        this.WriteFooterAndUninitialize(currentFileName);
                        this.recentAppenders.InvalidateAppender(currentFileName);
                        this.DoAutoArchive(currentFileName, firstLogEvent);
                    }

                    this.WriteToFile(currentFileName, firstLogEvent, ms.ToArray(), false);
                }
            }
            catch (Exception exception)
            {
                if (exception.MustBeRethrown())
                {
                    throw;
                }

                lastException = exception;
            }

            foreach (AsyncContinuation cont in pendingContinuations)
            {
                cont(lastException);
            }

            pendingContinuations.Clear();
        }

        /// <summary>
        /// Determines if the file name as <see cref="String"/> contains a numeric pattern i.e. {#} in it.  
        ///
        /// Example: 
        ///     trace{#}.log        Contains the numeric pattern.
        ///     trace{###}.log      Contains the numeric pattern.
        ///     trace{#X#}.log      Contains the numeric pattern (See remarks).
        ///     trace.log           Does not contain the pattern.
        /// </summary>
        /// <remarks>Occationally, this method can identify the existance of the {#} pattern incorrectly.</remarks>
        /// <param name="fileName">File name to be checked.</param>
        /// <returns><see langword="true"/> when the pattern is found; <see langword="false"/> otherwise.</returns>
        private static bool ContainsFileNamePattern(string fileName)
        {
            int startingIndex = fileName.IndexOf("{#", StringComparison.Ordinal);
            int endingIndex = fileName.IndexOf("#}", StringComparison.Ordinal);

            return (startingIndex != -1 && endingIndex != -1 && startingIndex < endingIndex);
        }

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
        private void RecursiveRollingRename(string fileName, string pattern, int archiveNumber)
        {
            if (this.MaxArchiveFiles > 0 && archiveNumber >= this.MaxArchiveFiles)
            {
                File.Delete(fileName);
                return;
            }

            if (!File.Exists(fileName))
            {
                return;
            }

            string newFileName = ReplaceNumberPattern(pattern, archiveNumber);
            RecursiveRollingRename(newFileName, pattern, archiveNumber + 1);

            var shouldCompress = archiveNumber == 0;
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

        /// <summary>
        /// Archives the <paramref name="fileName"/> using a sequence style numbering. The most recent archive has the
        /// highest number. When the number of archive files exceed <see cref="P:MaxArchiveFiles"/> the obsolete
        /// archives are deleted.
        /// </summary>
        /// <param name="fileName">File name to be archived.</param>
        /// <param name="pattern">File name template which contains the numeric pattern to be replaced.</param>
        private void ArchiveBySequence(string fileName, string pattern)
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
#if SILVERLIGHT && !WINDOWS_PHONE
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

            if (minNumber != -1 && this.MaxArchiveFiles != 0)
            {
                int minNumberToKeep = nextNumber - this.MaxArchiveFiles + 1;
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

            string newFileName = ReplaceNumberPattern(pattern, nextNumber);
            RollArchiveForward(fileName, newFileName, allowCompress: true);
        }

        /// <summary>
        /// Creates an archive copy of source file either by compressing it or moving to a new location in the file
        /// system. Which action will be used is determined by the value of <paramref name="enableCompression"/> parameter.
        /// </summary>
        /// <param name="fileName">File name to be archived.</param>
        /// <param name="archiveFileName">Name of the archive file.</param>
        /// <param name="enableCompression">Enables file compression</param>
        private static void ArchiveFile(string fileName, string archiveFileName, bool enableCompression)
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

        private void RollArchiveForward(string existingFileName, string archiveFileName, bool allowCompress)
        {
            ArchiveFile(existingFileName, archiveFileName, allowCompress && EnableArchiveFileCompression);

            string fileName = Path.GetFileName(existingFileName);
            if (fileName == null) { return; }

            // When the file has been moved, the original filename is 
            // no longer one of the initializedFiles. The initializedFilesCounter
            // should be left alone, the amount is still valid.
            if (this.initializedFiles.ContainsKey(fileName))
            {
                this.initializedFiles.Remove(fileName);
            }
            else if (this.initializedFiles.ContainsKey(existingFileName))
            {
                this.initializedFiles.Remove(existingFileName);
            }
        }

#if !NET_CF
        /// <summary>
        /// <para>
        /// Archives the <paramref name="fileName"/> using a date and sequence style numbering. Archives will be stamped
        /// with the prior period (Year, Month, Day) datetime. The most recent archive has the highest number (in
        /// combination with the date).
        /// </para>
        /// <para>
        /// When the number of archive files exceed <see cref="P:MaxArchiveFiles"/> the obsolete archives are deleted.
        /// </para>
        /// </summary>
        /// <param name="fileName">File name to be archived.</param>
        /// <param name="pattern">File name template which contains the numeric pattern to be replaced.</param>
        /// <param name="logEvent">Log event that the <see cref="FileTarget"/> instance is currently processing.</param>
        private void ArchiveByDateAndSequence(string fileName, string pattern, LogEventInfo logEvent)
        {
            string baseNamePattern = Path.GetFileName(pattern);

            if (string.IsNullOrEmpty(baseNamePattern))
            {
                return;
            }

            FileNameTemplate fileTemplate = new FileNameTemplate(baseNamePattern);
            string fileNameMask = fileTemplate.ReplacePattern("*");
            string dateFormat = GetDateFormatString(this.ArchiveDateFormat);

            string dirName = Path.GetDirectoryName(Path.GetFullPath(pattern));
            if (string.IsNullOrEmpty(dirName))
            {
                return;
            }

            int minSequenceLength = fileTemplate.EndAt - fileTemplate.BeginAt - 2;
            int nextSequenceNumber;
            DateTime archiveDate = GetArchiveDate(IsDaySwitch(fileName, logEvent));
            List<string> archiveFileNames;
            if (Directory.Exists(dirName))
            {
                List<DateAndSequenceArchive> archives = FindDateAndSequenceArchives(dirName, fileName, fileNameMask, minSequenceLength, dateFormat, fileTemplate)
                    .ToList();

                // Find out the next sequence number among existing archives having the same date part as the current date.
                int? lastSequenceNumber = archives
                    .Where(a => a.HasSameFormattedDate(archiveDate))
                    .Max(a => (int?)a.Sequence);
                nextSequenceNumber = (int)(lastSequenceNumber != null ? lastSequenceNumber + 1 : 0);

                archiveFileNames = archives
                    .OrderBy(a => a.Date)
                    .ThenBy(a => a.Sequence)
                    .Select(a => a.FileName)
                    .ToList();
            }
            else
            {
                Directory.CreateDirectory(dirName);
                nextSequenceNumber = 0;
                archiveFileNames = new List<string>();
            }

            string paddedSequence = nextSequenceNumber.ToString().PadLeft(minSequenceLength, '0');
            string archiveFileNameWithoutPath = fileNameMask.Replace("*",
                string.Format("{0}.{1}", archiveDate.ToString(dateFormat), paddedSequence));
            string archiveFileName = Path.Combine(dirName, archiveFileNameWithoutPath);

            RollArchiveForward(fileName, archiveFileName, allowCompress: true);
            archiveFileNames.Add(archiveFileName);
            EnsureArchiveCount(archiveFileNames);
        }

        /// <summary>
        /// Determines whether a file with a different name from <paramref name="fileName"/> is needed to receive the
        /// <paramref name="logEvent"/>. This is determined based on the last date and time which the file has been
        /// written compared to the time the log event was initiated.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> when log event time is "different" than the last write time; <see langword="false"/> otherwise.
        /// </returns>
        private bool IsDaySwitch(string fileName, LogEventInfo logEvent)
        {
            DateTime lastWriteTime;
            long fileLength;
            if (this.GetFileInfo(fileName, out lastWriteTime, out fileLength))
            {
                string formatString = GetDateFormatString(string.Empty);
                string ts = lastWriteTime.ToLocalTime().ToString(formatString, CultureInfo.InvariantCulture);
                string ts2 = logEvent.TimeStamp.ToLocalTime().ToString(formatString, CultureInfo.InvariantCulture);

                return ts != ts2;
            }

            return false;
        }

        /// <summary>
        /// Deletes files among a given list, and stops as soon as the remaining files are fewer than the <see
        /// cref="P:FileTarget.MaxArchiveFiles"/> setting.
        /// </summary>
        /// <param name="oldArchiveFileNames">List of the file archives.</param>
        /// <remarks>
        /// Items are deleted in the same order as in <paramref name="oldArchiveFileNames"/>. No file is deleted if <see
        /// cref="P:FileTarget.MaxArchiveFiles"/> property is zero.
        /// </remarks>
        private void EnsureArchiveCount(List<string> oldArchiveFileNames)
        {
            if (this.MaxArchiveFiles <= 0) return;

            int numberToDelete = oldArchiveFileNames.Count - this.MaxArchiveFiles;
            for (int fileIndex = 0; fileIndex < numberToDelete; fileIndex++)
            {
                InternalLogger.Info("Deleting old archive {0}.", oldArchiveFileNames[fileIndex]);
                File.Delete(oldArchiveFileNames[fileIndex]);
            }
        }

        /// <summary>
        /// Searches a given directory for archives that comply with the current archive pattern.
        /// </summary>
        /// <returns>An enumeration of archive infos, ordered by their file creation date.</returns>
        private IEnumerable<DateAndSequenceArchive> FindDateAndSequenceArchives(string dirName, string logFileName,
            string fileNameMask,
            int minSequenceLength, string dateFormat, FileNameTemplate fileTemplate)
        {
            var directoryInfo = new DirectoryInfo(dirName);

            int archiveFileNameMinLength = fileNameMask.Length + minSequenceLength;
            var archiveFileNames = GetFiles(directoryInfo, fileNameMask)
                .Where(n => n.Name.Length >= archiveFileNameMinLength)
                .OrderBy(n => n.CreationTime)
                .Select(n => n.FullName);

            foreach (string archiveFileName in archiveFileNames)
            {
                //Get the archive file name or empty string if it's null
                string archiveFileNameWithoutPath = Path.GetFileName(archiveFileName) ?? "";

                DateTime date;
                int sequence;
                if (
                    !TryParseDateAndSequence(archiveFileNameWithoutPath, dateFormat, fileTemplate, out date,
                        out sequence))
                {
                    continue;
                }

                //It's possible that the log file itself has a name that will match the archive file mask.
                if (string.IsNullOrEmpty(archiveFileNameWithoutPath) ||
                    archiveFileNameWithoutPath.Equals(Path.GetFileName(logFileName)))
                {
                    continue;
                }

                yield return new DateAndSequenceArchive(archiveFileName, date, dateFormat, sequence);
            }
        }

        private static bool TryParseDateAndSequence(string archiveFileNameWithoutPath, string dateFormat, FileNameTemplate fileTemplate, out DateTime date, out int sequence)
        {
            int trailerLength = fileTemplate.Template.Length - fileTemplate.EndAt;
            int dateAndSequenceIndex = fileTemplate.BeginAt;
            int dateAndSequenceLength = archiveFileNameWithoutPath.Length - trailerLength - dateAndSequenceIndex;

            string dateAndSequence = archiveFileNameWithoutPath.Substring(dateAndSequenceIndex, dateAndSequenceLength);
            int sequenceIndex = dateAndSequence.LastIndexOf('.') + 1;

            string sequencePart = dateAndSequence.Substring(sequenceIndex);
            if (!Int32.TryParse(sequencePart, NumberStyles.None, CultureInfo.CurrentCulture, out sequence))
            {
                date = default(DateTime);
                return false;
            }

            string datePart = dateAndSequence.Substring(0, dateAndSequence.Length - sequencePart.Length - 1);
            if (!DateTime.TryParseExact(datePart, dateFormat, CultureInfo.CurrentCulture, DateTimeStyles.None,
                out date))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Gets the collection of files in the specified directory which they match the <paramref name="fileNameMask"/>.
        /// </summary>
        /// <param name="directoryInfo">Directory to searched.</param>
        /// <param name="fileNameMask">Pattern whihc the files will be searched against.</param>
        /// <returns>Lisf of files matching the pattern.</returns>
        private static IEnumerable<FileInfo> GetFiles(DirectoryInfo directoryInfo, string fileNameMask)
        {
#if SILVERLIGHT && !WINDOWS_PHONE
            return directoryInfo.EnumerateFiles(fileNameMask);
#else
            return directoryInfo.GetFiles(fileNameMask);
#endif
        }

        /// <summary>
        /// Replaces the string-based pattern i.e. {#} in a file name with the value passed in <paramref
        /// name="replacementValue"/> parameter.
        /// </summary>
        /// <param name="pattern">File name which contains the string-based pattern.</param>
        /// <param name="replacementValue">Value which will replace the string-based pattern.</param>
        /// <returns>
        /// File name with the value of <paramref name="replacementValue"/> in the position of the string-based pattern.
        /// </returns>
        private static string ReplaceFileNamePattern(string pattern, string replacementValue)
        {
            //
            // TODO: ReplaceFileNamePattern() method is nearly identical to ReplaceNumberPattern(). Consider merging.
            //

            return new FileNameTemplate(Path.GetFileName(pattern)).ReplacePattern(replacementValue);
        }

        /// <summary>
        /// Archives the <paramref name="fileName"/> using a date style numbering. Archives will be stamped with the
        /// prior period (Year, Month, Day, Hour, Minute) datetime. When the number of archive files exceed <see
        /// cref="P:MaxArchiveFiles"/> the obsolete archives are deleted.
        /// </summary>
        /// <param name="fileName">File name to be archived.</param>
        /// <param name="pattern">File name template which contains the numeric pattern to be replaced.</param>
        private void ArchiveByDate(string fileName, string pattern)
        {
            string fileNameMask = ReplaceFileNamePattern(pattern, "*");
            string dirName = Path.GetDirectoryName(Path.GetFullPath(pattern));
            string dateFormat = GetDateFormatString(this.ArchiveDateFormat);

            DateTime archiveDate = GetArchiveDate(true);
            if (dirName != null)
            {
                string archiveFileName = Path.Combine(dirName, fileNameMask.Replace("*", archiveDate.ToString(dateFormat)));
                RollArchiveForward(fileName, archiveFileName, allowCompress: true);
            }

            DeleteOldDateArchives(pattern);
        }

        /// <summary>
        /// Deletes archive files in reverse chronological order until only the
        /// MaxArchiveFiles number of archive files remain.
        /// </summary>
        /// <param name="pattern">The pattern that archive filenames will match</param>
        private void DeleteOldDateArchives(string pattern)
        {

            string fileNameMask = ReplaceFileNamePattern(pattern, "*");
            string dirName = Path.GetDirectoryName(Path.GetFullPath(pattern));
            string dateFormat = GetDateFormatString(this.ArchiveDateFormat);

            if (dirName != null)
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(dirName);
                if (!directoryInfo.Exists)
                {
                    Directory.CreateDirectory(dirName);
                    return;
                }

#if SILVERLIGHT && !WINDOWS_PHONE
                var files = directoryInfo.EnumerateFiles(fileNameMask).OrderBy(n => n.CreationTime).Select(n => n.FullName);
#else
                var files = directoryInfo.GetFiles(fileNameMask).OrderBy(n => n.CreationTime).Select(n => n.FullName);
#endif
                List<string> filesByDate = new List<string>();

                foreach (string nextFile in files)
                {
                    string archiveFileName = Path.GetFileName(nextFile);
                    string datePart = archiveFileName.Substring(fileNameMask.LastIndexOf('*'), dateFormat.Length);
                    DateTime fileDate = DateTime.MinValue;
                    if (DateTime.TryParseExact(datePart, dateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out fileDate))
                    {
                        filesByDate.Add(nextFile);
                    }
                }

                EnsureArchiveCount(filesByDate);
            }
        }
#endif

        /// <summary>
        /// Gets the correct formating <see langword="String"/> to be used based on the value of <see
        /// cref="P:ArchiveEvery"/> for converting <see langword="DateTime"/> values which will be inserting into file
        /// names during archiving.
        /// 
        /// This value will be computed only when a empty value or <see langword="null"/> is passed into <paramref name="defaultFormat"/>
        /// </summary>
        /// <param name="defaultFormat">Date format to used irrespectively of <see cref="P:ArchiveEvery"/> value.</param>
        /// <returns>Formatting <see langword="String"/> for dates.</returns>
        private string GetDateFormatString(string defaultFormat)
        {
            // If archiveDateFormat is not set in the config file, use a default 
            // date format string based on the archive period.
            string formatString = defaultFormat;
            if (string.IsNullOrEmpty(formatString))
            {
                switch (this.ArchiveEvery)
                {
                    case FileArchivePeriod.Year:
                        formatString = "yyyy";
                        break;

                    case FileArchivePeriod.Month:
                        formatString = "yyyyMM";
                        break;

                    default:
                        formatString = "yyyyMMdd";
                        break;

                    case FileArchivePeriod.Hour:
                        formatString = "yyyyMMddHH";
                        break;

                    case FileArchivePeriod.Minute:
                        formatString = "yyyyMMddHHmm";
                        break;
                }
            }
            return formatString;
        }

        private DateTime GetArchiveDate(bool isNextCycle)
        {
            DateTime archiveDate = TimeSource.Current.Time;

            // Because AutoArchive/ArchiveByDate gets called after the FileArchivePeriod condition matches, decrement the archive period by 1
            // (i.e. If ArchiveEvery = Day, the file will be archived with yesterdays date)
            int addCount = isNextCycle ? -1 : 0;

            switch (this.ArchiveEvery)
            {
                case FileArchivePeriod.Day:
                    archiveDate = archiveDate.AddDays(addCount);
                    break;

                case FileArchivePeriod.Hour:
                    archiveDate = archiveDate.AddHours(addCount);
                    break;

                case FileArchivePeriod.Minute:
                    archiveDate = archiveDate.AddMinutes(addCount);
                    break;

                case FileArchivePeriod.Month:
                    archiveDate = archiveDate.AddMonths(addCount);
                    break;

                case FileArchivePeriod.Year:
                    archiveDate = archiveDate.AddYears(addCount);
                    break;
            }

            return archiveDate;
        }

        /// <summary>
        /// Invokes the archiving process after determining when and which type of archiving is required.
        /// </summary>
        /// <param name="fileName">File name to be checked and archived.</param>
        /// <param name="eventInfo">Log event that the <see cref="FileTarget"/> instance is currently processing.</param>
        private void DoAutoArchive(string fileName, LogEventInfo eventInfo)
        {
            FileInfo fileInfo = new FileInfo(fileName);
            if (!fileInfo.Exists)
            {
                return;
            }

            string fileNamePattern = GetArchiveFileNamePattern(fileName, eventInfo);

            if (!ContainsFileNamePattern(fileNamePattern))
            {
                if (fileArchive.Archive(fileNamePattern, fileInfo.FullName, CreateDirs, EnableArchiveFileCompression))
                {
                    if (this.initializedFiles.ContainsKey(fileInfo.FullName))
                    {
                        this.initializedFiles.Remove(fileInfo.FullName);
                    }
                }
            }
            else
            {
                switch (this.ArchiveNumbering)
                {
                    case ArchiveNumberingMode.Rolling:
                        this.RecursiveRollingRename(fileInfo.FullName, fileNamePattern, 0);
                        break;

                    case ArchiveNumberingMode.Sequence:
                        this.ArchiveBySequence(fileInfo.FullName, fileNamePattern);
                        break;

#if !NET_CF
                    case ArchiveNumberingMode.Date:
                        this.ArchiveByDate(fileInfo.FullName, fileNamePattern);
                        break;

                    case ArchiveNumberingMode.DateAndSequence:
                        this.ArchiveByDateAndSequence(fileInfo.FullName, fileNamePattern, eventInfo);
                        break;
#endif
                }
            }
        }

        /// <summary>
        /// Gets the pattern that archive files will match
        /// </summary>
        /// <param name="fileName">Filename of the log file</param>
        /// <param name="eventInfo">Log event that the <see cref="FileTarget"/> instance is currently processing.</param>
        /// <returns>A string with a pattern that will match the archive filenames</returns>
        private string GetArchiveFileNamePattern(string fileName, LogEventInfo eventInfo)
        {
            if (this.ArchiveFileName == null)
            {
                string ext = EnableArchiveFileCompression ? ".zip" : Path.GetExtension(fileName);
                return Path.ChangeExtension(fileName, ".{#}" + ext);
            }
            else
            {
                //The archive file name is given. There are two possibilities
                //(1) User supplied the Filename with pattern
                //(2) User supplied the normal filename
                string archiveFileName = this.ArchiveFileName.Render(eventInfo);
                return CleanupInvalidFileNameChars(archiveFileName);
            }
        }

        /// <summary>
        /// Indicates if the automatic archiving process should be executed.
        /// </summary>
        /// <param name="fileName">File name to be written.</param>
        /// <param name="ev">Log event that the <see cref="FileTarget"/> instance is currently processing.</param>
        /// <param name="upcomingWriteSize">The size in bytes of the next chunk of data to be written in the file.</param>
        /// <returns><see langword="true"/> when archiving should be executed; <see langword="false"/> otherwise.</returns>
        private bool ShouldAutoArchive(string fileName, LogEventInfo ev, int upcomingWriteSize)
        {
            return ShouldAutoArchiveBasedOnFileSize(fileName, upcomingWriteSize) ||
                   ShouldAutoArchiveBasedOnTime(fileName, ev);
        }

        /// <summary>
        /// Indicates if the automatic archiving process should be executed based on file size constrains.
        /// </summary>
        /// <param name="fileName">File name to be written.</param>
        /// <param name="upcomingWriteSize">The size in bytes of the next chunk of data to be written in the file.</param>
        /// <returns><see langword="true"/> when archiving should be executed; <see langword="false"/> otherwise.</returns>
        private bool ShouldAutoArchiveBasedOnFileSize(string fileName, int upcomingWriteSize)
        {
            if (this.ArchiveAboveSize == FileTarget.ArchiveAboveSizeDisabled)
            {
                return false;
            }

            DateTime lastWriteTime;
            long fileLength;

            if (!this.GetFileInfo(fileName, out lastWriteTime, out fileLength))
            {
                return false;
            }

            if (this.ArchiveAboveSize != FileTarget.ArchiveAboveSizeDisabled)
            {
                if (fileLength + upcomingWriteSize > this.ArchiveAboveSize)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Indicates if the automatic archiving process should be executed based on date/time constrains.
        /// </summary>
        /// <param name="fileName">File name to be written.</param>
        /// <param name="logEvent">Log event that the <see cref="FileTarget"/> instance is currently processing.</param>
        /// <returns><see langword="true"/> when archiving should be executed; <see langword="false"/> otherwise.</returns>
        private bool ShouldAutoArchiveBasedOnTime(string fileName, LogEventInfo logEvent)
        {
            if (this.ArchiveEvery == FileArchivePeriod.None)
            {
                return false;
            }

            DateTime lastWriteTime;
            long fileLength;

            if (!this.GetFileInfo(fileName, out lastWriteTime, out fileLength))
            {
                return false;
            }

            if (this.ArchiveEvery != FileArchivePeriod.None)
            {
                // file write time is in Utc and logEvent's timestamp is originated from TimeSource.Current,
                // so we should ask the TimeSource to convert file time to TimeSource time:
                lastWriteTime = TimeSource.Current.FromSystemTime(lastWriteTime);
                string formatString = GetDateFormatString(string.Empty);
                string fileLastChanged = lastWriteTime.ToString(formatString, CultureInfo.InvariantCulture);
                string logEventRecorded = logEvent.TimeStamp.ToString(formatString, CultureInfo.InvariantCulture);

                if (fileLastChanged != logEventRecorded)
                {
                    return true;
                }
            }

            return false;
        }

        private void AutoClosingTimerCallback(object state)
        {
            lock (this.SyncRoot)
            {
                if (!this.IsInitialized)
                {
                    return;
                }

                try
                {
                    DateTime expireTime = DateTime.UtcNow.AddSeconds(-this.OpenFileCacheTimeout);
                    this.recentAppenders.CloseAppenders(expireTime);
                }
                catch (Exception exception)
                {
                    if (exception.MustBeRethrown())
                    {
                        throw;
                    }

                    InternalLogger.Warn("Exception in AutoClosingTimerCallback: {0}", exception);
                }
            }
        }

        /// <summary>
        /// The sequence of <see langword="byte"/> to be written for the file header.
        /// </summary>
        /// <returns>Sequence of <see langword="byte"/> to be written.</returns>
        private byte[] GetHeaderBytes()
        {
            return this.GetLayoutBytes(this.Header);
        }

        /// <summary>
        /// The sequence of <see langword="byte"/> to be written for the file footer.
        /// </summary>
        /// <returns>Sequence of <see langword="byte"/> to be written.</returns>        
        private byte[] GetFooterBytes()
        {
            return this.GetLayoutBytes(this.Footer);
        }

        /// <summary>
        /// Evaluates which parts of a file should be written (header, content, footer) based on various properties of
        /// <see cref="FileTarget"/> instance and writes them.
        /// </summary>
        /// <param name="fileName">File name to be written.</param>
        /// <param name="logEvent">Log event that the <see cref="FileTarget"/> instance is currently processing.</param>
        /// <param name="bytes">Raw sequence of <see langword="byte"/> to be written into the content part of the file.</param>        
        /// <param name="justData">Indicates that only content section should be written in the file.</param>
        private void WriteToFile(string fileName, LogEventInfo logEvent, byte[] bytes, bool justData)
        {
            if (this.ReplaceFileContentsOnEachWrite)
            {
                ReplaceFileContent(fileName, bytes);
                return;
            }

            bool writeHeader = InitializeFile(fileName, logEvent, justData);
            BaseFileAppender appender = this.recentAppenders.AllocateAppender(fileName);

            if (writeHeader)
            {
                this.WriteHeader(appender);
            }

            appender.Write(bytes);

            if (this.AutoFlush)
            {
                appender.Flush();
            }
        }

        /// <summary>
        /// Initialise a file to be used by the <see cref="FileTarget"/> instance. Based on the number of initialised
        /// files and the values of various instance properties clean up and/or archiving processes can be invoked.
        /// </summary>
        /// <param name="fileName">File name to be written.</param>
        /// <param name="logEvent">Log event that the <see cref="FileTarget"/> instance is currently processing.</param>
        /// <param name="justData">Indicates that only content section should be written in the file.</param>
        /// <returns><see langword="true"/> when file header should be written; <see langword="false"/> otherwise.</returns>
        private bool InitializeFile(string fileName, LogEventInfo logEvent, bool justData)
        {
            bool writeHeader = false;

            if (!justData)
            {
                //UtcNow is much faster then .now. This was a bottleneck in writing a lot of files after CPU test.
                var now = DateTime.UtcNow;
                if (!this.initializedFiles.ContainsKey(fileName))
                {
                    ProcessOnStartup(fileName, logEvent);

                    this.initializedFiles[fileName] = now;
                    this.initializedFilesCounter++;
                    writeHeader = true;

                    if (this.initializedFilesCounter >= FileTarget.InitializedFilesCounterMax)
                    {
                        this.initializedFilesCounter = 0;
                        this.CleanupInitializedFiles();
                    }
                }

                this.initializedFiles[fileName] = now;
            }

            return writeHeader;
        }

        /// <summary>
        /// Writes the file footer and uninitialise the file in <see cref="FileTarget"/> instance internal structures.
        /// </summary>
        /// <param name="fileName">File name to be written.</param>
        private void WriteFooterAndUninitialize(string fileName)
        {
            byte[] footerBytes = this.GetFooterBytes();
            if (footerBytes != null)
            {
                if (File.Exists(fileName))
                {
                    this.WriteToFile(fileName, null, footerBytes, true);
                }
            }

            this.initializedFiles.Remove(fileName);
        }

        /// <summary>
        /// Invokes the archiving and clean up of older archive file based on the values of <see
        /// cref="P:NLog.Targets.FileTarget.ArchiveOldFileOnStartup"/> and <see
        /// cref="P:NLog.Targets.FileTarget.DeleteOldFileOnStartup"/> properties respectively.
        /// </summary>
        /// <param name="fileName">File name to be written.</param>
        /// <param name="logEvent">Log event that the <see cref="FileTarget"/> instance is currently processing.</param>
        private void ProcessOnStartup(string fileName, LogEventInfo logEvent)
        {
            if (this.ArchiveOldFileOnStartup)
            {
                try
                {
                    this.DoAutoArchive(fileName, logEvent);
                }
                catch (Exception exception)
                {
                    if (exception.MustBeRethrown())
                    {
                        throw;
                    }

                    InternalLogger.Warn("Unable to archive old log file '{0}': {1}", fileName, exception);
                }
            }

            if (this.DeleteOldFileOnStartup)
            {
                try
                {
                    File.Delete(fileName);
                }
                catch (Exception exception)
                {
                    if (exception.MustBeRethrown())
                    {
                        throw;
                    }

                    InternalLogger.Warn("Unable to delete old log file '{0}': {1}", fileName, exception);
                }
            }
        }

        /// <summary>
        /// Creates the file specified in <paramref name="fileName"/> and writes the file content in each entirety i.e.
        /// Header, Content and Footer.
        /// </summary>
        /// <param name="fileName">The name of the file to be written.</param>
        /// <param name="bytes">Sequence of <see langword="byte"/> to be written in the content section of the file.</param>
        /// <remarks>This method is used when the content of the log file is re-written on every write.</remarks>
        private void ReplaceFileContent(string fileName, byte[] bytes)
        {
            using (FileStream fs = File.Create(fileName))
            {
                byte[] headerBytes = this.GetHeaderBytes();
                if (headerBytes != null)
                {
                    fs.Write(headerBytes, 0, headerBytes.Length);
                }

                fs.Write(bytes, 0, bytes.Length);

                byte[] footerBytes = this.GetFooterBytes();
                if (footerBytes != null)
                {
                    fs.Write(footerBytes, 0, footerBytes.Length);
                }
            }
        }

        /// <summary>
        /// Writes the header information to a file.
        /// </summary>
        /// <param name="appender">File appender associated with the file.</param>
        private void WriteHeader(BaseFileAppender appender)
        {
            long fileLength;
            DateTime lastWriteTime;

            //  Write header only on empty files or if file info cannot be obtained.
            if (!appender.GetFileInfo(out lastWriteTime, out fileLength) || fileLength == 0)
            {
                byte[] headerBytes = this.GetHeaderBytes();
                if (headerBytes != null)
                {
                    appender.Write(headerBytes);
                }
            }
        }

        /// <summary>
        /// Returns the length of a specified file and the last time it has been written. File appender is queried before the file system.  
        /// </summary>
        /// <param name="filePath">File which the information are requested.</param>
        /// <param name="lastWriteTime">The last time the file has been written is returned.</param>
        /// <param name="fileLength">The length of the file is returned.</param>
        /// <returns><see langword="true"/> when file details returned; <see langword="false"/> otherwise.</returns>
        private bool GetFileInfo(string filePath, out DateTime lastWriteTime, out long fileLength)
        {
            if (this.recentAppenders.GetFileInfo(filePath, out lastWriteTime, out fileLength))
            {
                return true;
            }

            FileInfo fileInfo = new FileInfo(filePath);
            if (fileInfo.Exists)
            {
                fileLength = fileInfo.Length;
#if !SILVERLIGHT
                lastWriteTime = fileInfo.LastWriteTimeUtc;
#else
                lastWriteTime = fileInfo.LastWriteTime;
#endif
                return true;
            }

            fileLength = -1;
            lastWriteTime = DateTime.MinValue;
            return false;
        }

        /// <summary>
        /// The sequence of <see langword="byte"/> to be written in a file after applying any formating and any
        /// transformations required from the <see cref="Layout"/>.
        /// </summary>
        /// <param name="layout">The layout used to render output message.</param>
        /// <returns>Sequence of <see langword="byte"/> to be written.</returns>
        /// <remarks>Usually it is used to render the header and hooter of the files.</remarks>
        private byte[] GetLayoutBytes(Layout layout)
        {
            if (layout == null)
            {
                return null;
            }

            string renderedText = layout.Render(LogEventInfo.CreateNullEvent()) + this.NewLineChars;
            return this.TransformBytes(this.Encoding.GetBytes(renderedText));
        }

        /// <summary>
        /// Replaces any invalid characters found in the <paramref name="fileName"/> with underscore i.e _ character.
        /// Invalid characters are defined by .NET framework and they returned by <see
        /// cref="M:System.IO.Path.GetInvalidFileNameChars"/> method.
        /// <para>Note: not implemented in Silverlight</para>
        /// </summary>
        /// <param name="fileName">The original file name which might contain invalid characters.</param>
        /// <returns>The cleaned up file name without any invalid characters.</returns>
        private string CleanupInvalidFileNameChars(string fileName)
        {

            if (!this.CleanupFileName)
            {
                return fileName;
            }

#if !SILVERLIGHT

            var lastDirSeparator = fileName.LastIndexOfAny(DirectorySeparatorChars);

            var fileName1 = fileName.Substring(lastDirSeparator + 1);
            var dirName = lastDirSeparator > 0 ? fileName.Substring(0, lastDirSeparator) : string.Empty;

            char[] fileName1Chars = null;
            foreach (var invalidChar in InvalidFileNameChars)
            {
                for (int i = 0; i < fileName1.Length; i++)
                {
                    if (fileName1[i] == invalidChar)
                    {
                        //delay char[] creation until first invalid char
                        //is found to avoid memory allocation.
                        if (fileName1Chars == null)
                            fileName1Chars = fileName1.ToCharArray();
                        fileName1Chars[i] = '_';
                    }
                }
            }

            //only if an invalid char was replaced do we create a new string.
            if (fileName1Chars != null)
                fileName1 = new string(fileName1Chars);

            return Path.Combine(dirName, fileName1);
#else
            return fileName;
#endif
        }


        private class DynamicFileArchive
        {
            private readonly Queue<string> archiveFileQueue = new Queue<string>();
            
            /// <summary>
            /// Creates an instance of <see cref="DynamicFileArchive"/> class.
            /// </summary>
            /// <param name="maxArchivedFiles">Maximum number of archive files to be kept.</param>
            public DynamicFileArchive(int maxArchivedFiles)
            {
                this.MaxArchiveFileToKeep = maxArchivedFiles;
            }

            /// <summary>
            /// Creates an instance of <see cref="DynamicFileArchive"/> class.
            /// </summary>
            public DynamicFileArchive() : this(-1) { }

            /// <summary>
            /// Gets or sets the maximum number of archive files that should be kept.
            /// </summary>
            public int MaxArchiveFileToKeep { get; set; }

            /// <summary>
            /// Adds the files in the specified path to the archive file queue.
            /// </summary>
            /// <param name="archiveFolderPath">The folder where the archive files are stored.</param>
            public void InitializeForArchiveFolderPath(string archiveFolderPath)
            {
                archiveFileQueue.Clear();
                if (Directory.Exists(archiveFolderPath))
                {
#if SILVERLIGHT && !WINDOWS_PHONE
                    var files = Directory.EnumerateFiles(archiveFolderPath);
#else
                    var files = Directory.GetFiles(archiveFolderPath);
#endif
                    foreach (string nextFile in files.OrderBy(f => ExtractArchiveNumberFromFileName(f)))
                        archiveFileQueue.Enqueue(nextFile);
                }
            }

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
                if (MaxArchiveFileToKeep < 1)
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
                return true;
            }
            
            /// <summary>
            /// Archives the file, either by copying it to a new file system location or by compressing it, and add the file name into the list of archives.
            /// </summary>
            /// <param name="archiveFileName">Target file name.</param>
            /// <param name="fileName">Original file name.</param>
            /// <param name="createDirectory">Create a directory, if it does not exist.</param>
            /// <param name="enableCompression">Enables file compression.</param>
            private void AddToArchive(string archiveFileName, string fileName, bool createDirectory, bool enableCompression)
            {
                if (archiveFileQueue.Count != 0)
                    archiveFileName = GetNextArchiveFileName(archiveFileName);

                try
                {
                    ArchiveFile(fileName, archiveFileName, enableCompression);
                    archiveFileQueue.Enqueue(archiveFileName);
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
                if (MaxArchiveFileToKeep == 1 && archiveFileQueue.Any())
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

                while (archiveFileQueue.Count >= MaxArchiveFileToKeep)
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
            /// Gets the file name for the next archive file by appending a number to the provided
            /// "base"-filename.
            /// 
            /// Example: 
            ///     Original Filename   trace.log
            ///     Target Filename     trace.15.log
            /// </summary>          
            /// <param name="fileName">Original file name.</param>
            /// <returns>File name suitable for archiving</returns>
            private string GetNextArchiveFileName(string fileName)
            {
                int currentArchiveNumber = archiveFileQueue.Count == 0 ? 0 : ExtractArchiveNumberFromFileName(archiveFileQueue.Last());
                string archiveFileName = string.Format("{0}.{1}{2}", Path.GetFileNameWithoutExtension(fileName), currentArchiveNumber + 1, Path.GetExtension(fileName));
                return Path.Combine(Path.GetDirectoryName(fileName), archiveFileName);
            }

            private static int ExtractArchiveNumberFromFileName(string archiveFileName)
            {
                archiveFileName = Path.GetFileName(archiveFileName);
                int lastDotIdx = archiveFileName.LastIndexOf('.');
                if (lastDotIdx == -1)
                    return 0;

                int previousToLastDotIdx = archiveFileName.LastIndexOf('.', lastDotIdx - 1);
                string numberPart = previousToLastDotIdx == -1 ? archiveFileName.Substring(lastDotIdx + 1) : archiveFileName.Substring(previousToLastDotIdx + 1, lastDotIdx - previousToLastDotIdx - 1);

                int archiveNumber;
                return Int32.TryParse(numberPart, out archiveNumber) ? archiveNumber : 0;
            }
        }

        private sealed class FileNameTemplate
        {
            /// <summary>
            /// Characters determining the start of the <see cref="P:FileNameTemplate.Pattern"/>.
            /// </summary>
            public const string PatternStartCharacters = "{#";

            /// <summary>
            /// Characters determining the end of the <see cref="P:FileNameTemplate.Pattern"/>.
            /// </summary>
            public const string PatternEndCharacters = "#}";

            /// <summary>
            /// File name which is used as template for matching and replacements. 
            /// It is expected to contain a pattern to match.
            /// </summary>
            public string Template
            {
                get { return this.template; }
            }

            /// <summary>
            /// The begging position of the <see cref="P:FileNameTemplate.Pattern"/> 
            /// within the <see cref="P:FileNameTemplate.Template"/>. -1 is returned 
            /// when no pattern can be found.
            /// </summary>
            public int BeginAt
            {
                get
                {
                    return startIndex;
                }
            }

            /// <summary>
            /// The ending position of the <see cref="P:FileNameTemplate.Pattern"/> 
            /// within the <see cref="P:FileNameTemplate.Template"/>. -1 is returned 
            /// when no pattern can be found.
            /// </summary>
            public int EndAt
            {
                get
                {
                    return endIndex;
                }
            }

            private bool FoundPattern
            {
                get { return startIndex != -1 && endIndex != -1; }
            }

            private readonly string template;

            private readonly int startIndex;
            private readonly int endIndex;

            public FileNameTemplate(string template)
            {
                this.template = template;
                this.startIndex = template.IndexOf(PatternStartCharacters, StringComparison.Ordinal);
                if (this.startIndex != -1)
                    this.endIndex = template.IndexOf(PatternEndCharacters, StringComparison.Ordinal) + PatternEndCharacters.Length;
            }

            /// <summary>
            /// Replace the pattern with the specified String.
            /// </summary>
            /// <param name="replacementValue"></param>
            /// <returns></returns>
            public string ReplacePattern(string replacementValue)
            {
                return !FoundPattern || String.IsNullOrEmpty(replacementValue) ? this.Template : template.Substring(0, this.BeginAt) + replacementValue + template.Substring(this.EndAt);
            }
        }

    }
}