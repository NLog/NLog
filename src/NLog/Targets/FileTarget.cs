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

#if !SILVERLIGHT && !__ANDROID__ && !__IOS__
// Unfortunately, Xamarin Android and Xamarin iOS don't support mutexes (see https://github.com/mono/mono/blob/3a9e18e5405b5772be88bfc45739d6a350560111/mcs/class/corlib/System.Threading/Mutex.cs#L167) so the BaseFileAppender class now throws an exception in the constructor.
#define SupportsMutex
#endif

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
        /// The maximum number of initialised files before clean up procedures are initiated,
        /// to keep the number of initialised files to a minimum. Chose 25 to cater for monthly rolling of log-files.
        /// </summary>
        private const int InitializedFilesCounterMax = 25;

        /// <summary>
        /// This value disables file archiving based on the size. 
        /// </summary>
        private const int ArchiveAboveSizeDisabled = -1;


        /// <summary>
        /// Holds the initialised files each given time by the <see cref="FileTarget"/> instance. Against each file, the last write time is stored. 
        /// </summary>
        /// <remarks>Last write time is store in local time (no UTC).</remarks>
        private readonly Dictionary<string, DateTime> initializedFiles = new Dictionary<string, DateTime>(StringComparer.OrdinalIgnoreCase);

        private LineEndingMode lineEndingMode = LineEndingMode.Default;

        /// <summary>
        /// Factory used to create the file appenders in the <see cref="FileTarget"/> instance. 
        /// </summary>
        /// <remarks>File appenders are stored in an instance of <see cref="FileAppenderCache"/>.</remarks>
        private IFileAppenderFactory appenderFactory;

        /// <summary>
        /// List of the associated file appenders with the <see cref="FileTarget"/> instance.
        /// </summary>
        private FileAppenderCache fileAppenderCache;

        IFileArchiveMode GetFileArchiveHelper(string archiveFilePattern)
        {
            return fileArchiveHelper ?? (fileArchiveHelper = FileArchiveModeFactory.CreateArchiveStyle(archiveFilePattern, this.ArchiveNumbering, this.GetArchiveDateFormatString(this.ArchiveDateFormat), this.ArchiveFileName != null, this.MaxArchiveFiles));
        }
        private IFileArchiveMode fileArchiveHelper;

        private Timer autoClosingTimer;

        /// <summary>
        /// The number of initialised files at any one time.
        /// </summary>
        private int initializedFilesCounter;

        /// <summary>
        /// The maximum number of archive files that should be kept.
        /// </summary>
        private int maxArchiveFiles;

        /// <summary>
        /// The filename as target
        /// </summary>
        private FilePathLayout fullFileName;

        /// <summary>
        /// The archive file name as target
        /// </summary>
        private FilePathLayout fullArchiveFileName;

        private FileArchivePeriod archiveEvery;
        private long archiveAboveSize;

        private bool enableArchiveFileCompression;

        /// <summary>
        /// The date of the previous log event.
        /// </summary>
        private DateTime? previousLogEventTimestamp;

        /// <summary>
        /// The file name of the previous log event.
        /// </summary>
        private string previousLogFileName;

        private bool concurrentWrites;
        private bool keepFileOpen;
        private bool cleanupFileName;
        private FilePathKind fileNameKind;
        private FilePathKind archiveFileKind;

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
#if !SILVERLIGHT && !__IOS__ && !__ANDROID__
            this.FileAttributes = Win32FileAttributes.Normal;
#endif
            this.LineEnding = LineEndingMode.Default;
            this.EnableFileDelete = true;
            this.OpenFileCacheTimeout = -1;
            this.OpenFileCacheSize = 5;
            this.CreateDirs = true;
            this.ForceManaged = false;
            this.ArchiveDateFormat = string.Empty;

            this.fileAppenderCache = FileAppenderCache.Empty;
            this.CleanupFileName = true;

            this.WriteFooterOnArchivingOnly = false;

            this.OptimizeBufferReuse = GetType() == typeof(FileTarget);    // Pure FileTarget has support
        }

#if NET4_5
        static FileTarget()
        {
            FileCompressor = new ZipArchiveFileCompressor();
        }
#endif

        /// <summary>
        /// Initializes a new instance of the <see cref="FileTarget" /> class.
        /// </summary>
        /// <remarks>
        /// The default value of the layout is: <code>${longdate}|${level:uppercase=true}|${logger}|${message}</code>
        /// </remarks>
        /// <param name="name">Name of the target.</param>
        public FileTarget(string name) : this()
        {
            this.Name = name;
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
            get
            {
                if (fullFileName == null) return null;

                return fullFileName.GetLayout();
            }
            set
            {
                fullFileName = CreateFileNameLayout(value);
                ResetFileAppenders("FileName Changed");
            }
        }

        private FilePathLayout CreateFileNameLayout(Layout value)
        {
            if (value == null)
                return null;

            return new FilePathLayout(value, CleanupFileName, FileNameKind);
        }


        /// <summary>
        /// Cleanup invalid values in a filename, e.g. slashes in a filename. If set to <c>true</c>, this can impact the performance of massive writes. 
        /// If set to <c>false</c>, nothing gets written when the filename is wrong.
        /// </summary>
        [DefaultValue(true)]
        public bool CleanupFileName
        {
            get { return cleanupFileName; }
            set
            {
                if (cleanupFileName != value)
                {
                    cleanupFileName = value;
                    fullFileName = CreateFileNameLayout(FileName);
                    fullArchiveFileName = CreateFileNameLayout(ArchiveFileName);
                    ResetFileAppenders("CleanupFileName Changed");
                }
            }
        }

        /// <summary>
        /// Is the  <see cref="FileName"/> an absolute or relative path?
        /// </summary>
        [DefaultValue(FilePathKind.Unknown)]
        public FilePathKind FileNameKind
        {
            get { return fileNameKind; }
            set
            {
                if (fileNameKind != value)
                {
                    fileNameKind = value;
                    fullFileName = CreateFileNameLayout(FileName);
                    ResetFileAppenders("FileNameKind Changed");
                }
            }
        }

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
        public bool KeepFileOpen
        {
            get { return keepFileOpen; }
            set
            {
                if (keepFileOpen != value)
                {
                    keepFileOpen = value;
                    ResetFileAppenders("KeepFileOpen Changed");
                }
            }
        }

        /// <summary>
        /// Gets or sets the maximum number of log filenames that should be stored as existing.
        /// </summary>
        /// <remarks>
        /// The bigger this number is the longer it will take to write each log record. The smaller the number is
        /// the higher the chance that the clean function will be run when no new files have been opened.
        /// </remarks>
        [Obsolete("This option will be removed in NLog 5. Marked obsolete on NLog 4.5")]
        [DefaultValue(0)]
        public int maxLogFilenames { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to enable log file(s) to be deleted.
        /// </summary>
        /// <docgen category='Output Options' order='10' />
        [DefaultValue(true)]
        public bool EnableFileDelete { get; set; }

#if !SILVERLIGHT && !__IOS__ && !__ANDROID__
        /// <summary>
        /// Gets or sets the file attributes (Windows only).
        /// </summary>
        /// <docgen category='Output Options' order='10' />
        [Advanced]
        public Win32FileAttributes FileAttributes { get; set; }
#endif

        /// <summary>
        /// Should we capture the last write time of a file?
        /// </summary>
        bool ICreateFileParameters.CaptureLastWriteTime
        {
            get
            {
                return ArchiveNumbering == ArchiveNumberingMode.Date ||
                       ArchiveNumbering == ArchiveNumberingMode.DateAndSequence;
            }
        }

        /// <summary>
        /// Gets or sets the line ending mode.
        /// </summary>
        /// <docgen category='Layout Options' order='10' />
        [Advanced]
        public LineEndingMode LineEnding
        {
            get { return this.lineEndingMode; }

            set { this.lineEndingMode = value; }
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
        /// Gets or sets whether or not this target should just discard all data that its asked to write.
        /// Mostly used for when testing NLog Stack except final write
        /// </summary>
        [DefaultValue(false)]
        [Advanced]
        public bool DiscardAll { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether concurrent writes to the log file by multiple processes on the same host.
        /// </summary>
        /// <remarks>
        /// This makes multi-process logging possible. NLog uses a special technique
        /// that lets it keep the files open for writing.
        /// </remarks>
        /// <docgen category='Performance Tuning Options' order='10' />
        [DefaultValue(true)]
        public bool ConcurrentWrites
        {
            get { return concurrentWrites; }
            set
            {
                if (concurrentWrites != value)
                {
                    concurrentWrites = value;
                    ResetFileAppenders("ConcurrentWrites Changed");
                }
            }
        }

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
        /// Gets or sets a value specifying the date format to use when archiving files.
        /// </summary>
        /// <remarks>
        /// This option works only when the "ArchiveNumbering" parameter is set either to Date or DateAndSequence.
        /// </remarks>
        /// <docgen category='Output Options' order='10' />
        [DefaultValue("")]
        public string ArchiveDateFormat
        {
            get { return archiveDateFormat; }
            set
            {
                if (archiveDateFormat != value)
                {
                    archiveDateFormat = value;
                    ResetFileAppenders("ArchiveDateFormat Changed"); // Reset archive file-monitoring
                }
            }
        }
        private string archiveDateFormat;

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
        public long ArchiveAboveSize
        {
            get { return archiveAboveSize; }
            set
            {
                if ((archiveAboveSize == FileTarget.ArchiveAboveSizeDisabled) != (value == FileTarget.ArchiveAboveSizeDisabled))
                {
                    archiveAboveSize = value;
                    ResetFileAppenders("ArchiveAboveSize Changed"); // Reset archive file-monitoring
                }
                else
                {
                    archiveAboveSize = value;
                }
            }
        }

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
        public FileArchivePeriod ArchiveEvery
        {
            get { return archiveEvery; }
            set
            {
                if (archiveEvery != value)
                {
                    archiveEvery = value;
                    ResetFileAppenders("ArchiveEvery Changed"); // Reset archive file-monitoring
                }
            }
        }

        /// <summary>
        /// Is the  <see cref="ArchiveFileName"/> an absolute or relative path?
        /// </summary>
        public FilePathKind ArchiveFileKind
        {
            get { return archiveFileKind; }
            set
            {
                if (archiveFileKind != value)
                {
                    archiveFileKind = value;
                    fullArchiveFileName = CreateFileNameLayout(ArchiveFileName);
                    ResetFileAppenders("ArchiveFileKind Changed");  // Reset archive file-monitoring
                }
            }
        }

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
            get
            {
                if (fullArchiveFileName == null) return null;

                return fullArchiveFileName.GetLayout();
            }
            set
            {
                fullArchiveFileName = CreateFileNameLayout(value);
                ResetFileAppenders("ArchiveFileName Changed");  // Reset archive file-monitoring
            }
        }

        /// <summary>
        /// Gets or sets the maximum number of archive files that should be kept.
        /// </summary>
        /// <docgen category='Archival Options' order='10' />
        [DefaultValue(0)]
        public int MaxArchiveFiles
        {
            get { return maxArchiveFiles; }
            set
            {
                if (maxArchiveFiles != value)
                {
                    maxArchiveFiles = value;
                    ResetFileAppenders("MaxArchiveFiles Changed");  // Enforce archive cleanup
                }
            }
        }

        /// <summary>
        /// Gets or sets the way file archives are numbered. 
        /// </summary>
        /// <docgen category='Archival Options' order='10' />
        public ArchiveNumberingMode ArchiveNumbering
        {
            get { return archiveNumbering; }
            set
            {
                if (archiveNumbering != value)
                {
                    archiveNumbering = value;
                    ResetFileAppenders("ArchiveNumbering Changed"); // Reset archive file-monitoring
                }
            }
        }
        private ArchiveNumberingMode archiveNumbering;

        /// <summary>
        /// Used to compress log files during archiving.
        /// This may be used to provide your own implementation of a zip file compressor,
        /// on platforms other than .Net4.5.
        /// Defaults to ZipArchiveFileCompressor on .Net4.5 and to null otherwise.
        /// </summary>
        public static IFileCompressor FileCompressor { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to compress archive files into the zip archive format.
        /// </summary>
        /// <docgen category='Archival Options' order='10' />
        [DefaultValue(false)]
        public bool EnableArchiveFileCompression
        {
            get { return enableArchiveFileCompression && FileCompressor != null; }
            set
            {
                if (enableArchiveFileCompression != value)
                {
                    enableArchiveFileCompression = value;
                    ResetFileAppenders("EnableArchiveFileCompression Changed"); // Reset archive file-monitoring
                }
            }
        }

        /// <summary>
        /// Gets or set a value indicating whether a managed file stream is forced, instead of using the native implementation.
        /// </summary>
        [DefaultValue(false)]
        public bool ForceManaged { get; set; }

#if SupportsMutex
        /// <summary>
        /// Gets or sets a value indicationg whether file creation calls should be synchronized by a system global mutex.
        /// </summary>
        [DefaultValue(false)]
        public bool ForceMutexConcurrentWrites { get; set; }
#endif

        /// <summary>
        /// Gets or sets a value indicating whether the footer should be written only when the file is archived.
        /// </summary>
        [DefaultValue(false)]
        public bool WriteFooterOnArchivingOnly { get; set; }

        /// <summary>
        /// Gets the characters that are appended after each line.
        /// </summary>
        protected internal string NewLineChars
        {
            get { return lineEndingMode.NewLineCharacters; }
        }

        /// <summary>
        /// Refresh the ArchiveFilePatternToWatch option of the <see cref="FileAppenderCache" />. 
        /// The log file must be watched for archiving when multiple processes are writing to the same 
        /// open file.
        /// </summary>
        private void RefreshArchiveFilePatternToWatch(string fileName, LogEventInfo logEvent)
        {
            if (this.fileAppenderCache != null)
            {
                this.fileAppenderCache.CheckCloseAppenders -= AutoClosingTimerCallback;

#if !SILVERLIGHT && !__IOS__ && !__ANDROID__
                if (KeepFileOpen)
                    this.fileAppenderCache.CheckCloseAppenders += AutoClosingTimerCallback;

                bool mustWatchArchiving = IsArchivingEnabled() && ConcurrentWrites && KeepFileOpen;
                if (mustWatchArchiving)
                {
                    string fileNamePattern = GetArchiveFileNamePattern(fileName, logEvent);
                    var fileArchiveStyle = !string.IsNullOrEmpty(fileNamePattern) ? GetFileArchiveHelper(fileNamePattern) : null;
                    string fileNameMask = fileArchiveStyle != null ? fileArchiveHelper.GenerateFileNameMask(fileNamePattern) : string.Empty;
                    string directoryMask = !string.IsNullOrEmpty(fileNameMask) ? Path.Combine(Path.GetDirectoryName(fileNamePattern), fileNameMask) : string.Empty;
                    this.fileAppenderCache.ArchiveFilePatternToWatch = directoryMask;
                }
                else
                {
                    this.fileAppenderCache.ArchiveFilePatternToWatch = null;
                }
#endif
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
            this.CleanupInitializedFiles(TimeSource.Current.Time.AddDays(-FileTarget.InitializedFilesCleanupPeriod));
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
            List<string> filesToFinalize = null;

            // Select the files require to be finalized.
            foreach (var file in this.initializedFiles)
            {
                if (file.Value < cleanupThreshold)
                {
                    if (filesToFinalize == null)
                    {
                        filesToFinalize = new List<string>();
                    }
                    filesToFinalize.Add(file.Key);
                }
            }

            // Finalize the files.
            if (filesToFinalize != null)
            {
                foreach (string fileName in filesToFinalize)
                {
                    this.FinalizeFile(fileName);
                }
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
                fileAppenderCache.FlushAppenders();
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
            if (this.DiscardAll)
            {
                return NullAppender.TheFactory;
            }
            else if (!this.KeepFileOpen)
            {
                return RetryingMultiProcessFileAppender.TheFactory;
            }
            else if (this.NetworkWrites)
            {
                return RetryingMultiProcessFileAppender.TheFactory;
            }
            else if (this.ConcurrentWrites)
            {
#if !SupportsMutex
                return RetryingMultiProcessFileAppender.TheFactory;
#elif MONO
//
// mono on Windows uses mutexes, on Unix - special appender
//
                if (PlatformDetector.IsUnix)
                {
                    return UnixMultiProcessFileAppender.TheFactory;
                }
                else if (PlatformDetector.SupportsSharableMutex)
                {
                    return MutexMultiProcessFileAppender.TheFactory;
                }
                else
                {
                    return RetryingMultiProcessFileAppender.TheFactory;
                }
#else
                if (!PlatformDetector.SupportsSharableMutex)
                    return RetryingMultiProcessFileAppender.TheFactory;
                else if (!this.ForceMutexConcurrentWrites && PlatformDetector.IsDesktopWin32 && !PlatformDetector.IsMono)
                    return WindowsMultiProcessFileAppender.TheFactory;
                else
                    return MutexMultiProcessFileAppender.TheFactory;
#endif
            }
            else if (IsArchivingEnabled())
                return CountingSingleProcessFileAppender.TheFactory;
            else
                return SingleProcessFileAppender.TheFactory;
        }

        private bool IsArchivingEnabled()
        {
            return this.ArchiveAboveSize != FileTarget.ArchiveAboveSizeDisabled ||
                   this.ArchiveEvery != FileArchivePeriod.None;
        }

        /// <summary>
        /// Initializes file logging by creating data structures that
        /// enable efficient multi-file logging.
        /// </summary>
        protected override void InitializeTarget()
        {
            base.InitializeTarget();

            this.appenderFactory = GetFileAppenderFactory();

            this.fileAppenderCache = new FileAppenderCache(this.OpenFileCacheSize, this.appenderFactory, this);

            if ((this.OpenFileCacheSize > 0 || this.EnableFileDelete) && this.OpenFileCacheTimeout > 0)
            {
                this.autoClosingTimer = new Timer(
                    (state) => this.AutoClosingTimerCallback(this, EventArgs.Empty),
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
                this.FinalizeFile(fileName);
            }

            this.fileArchiveHelper = null;

            if (this.autoClosingTimer != null)
            {
                this.autoClosingTimer.Change(Timeout.Infinite, Timeout.Infinite);
                this.autoClosingTimer.Dispose();
                this.autoClosingTimer = null;
            }

            this.fileAppenderCache.CloseAppenders("Dispose");
            this.fileAppenderCache.Dispose();
        }

        private void ResetFileAppenders(string reason)
        {
            this.fileArchiveHelper = null;
            if (IsInitialized)
            {
                this.fileAppenderCache.CloseAppenders(reason);
                this.initializedFiles.Clear();
            }
        }

        /// <summary>
        /// Can be used if <see cref="Target.OptimizeBufferReuse"/> has been enabled.
        /// </summary>
        private readonly ReusableStreamCreator reusableFileWriteStream = new ReusableStreamCreator(1024);
        /// <summary>
        /// Can be used if <see cref="Target.OptimizeBufferReuse"/> has been enabled.
        /// </summary>
        private readonly ReusableStreamCreator reusableAsyncFileWriteStream = new ReusableStreamCreator(1024);
        /// <summary>
        /// Can be used if <see cref="Target.OptimizeBufferReuse"/> has been enabled.
        /// </summary>
        private readonly ReusableBufferCreator reusableEncodingBuffer = new ReusableBufferCreator(1024);

        /// <summary>
        /// Writes the specified logging event to a file specified in the FileName 
        /// parameter.
        /// </summary>
        /// <param name="logEvent">The logging event.</param>
        protected override void Write(LogEventInfo logEvent)
        {
            var logFileName = this.GetFullFileName(logEvent);
            if (OptimizeBufferReuse)
            {
                using (var targetStream = this.reusableFileWriteStream.Allocate())
                {
                    using (var targetBuilder = this.ReusableLayoutBuilder.Allocate())
                    using (var targetBuffer = this.reusableEncodingBuffer.Allocate())
                    {
                        this.RenderFormattedMessageToStream(logEvent, targetBuilder.Result, targetBuffer.Result, targetStream.Result);
                    }

                    ProcessLogEvent(logEvent, logFileName, new ArraySegment<byte>(targetStream.Result.GetBuffer(), 0, (int)targetStream.Result.Length));
                }
            }
            else
            {
                byte[] bytes = this.GetBytesToWrite(logEvent);
                ProcessLogEvent(logEvent, logFileName, new ArraySegment<byte>(bytes));
            }
        }

        /// <summary>
        /// Get full filename (=absolute) and cleaned if needed.
        /// </summary>
        /// <param name="logEvent"></param>
        /// <returns></returns>
        internal string GetFullFileName(LogEventInfo logEvent)
        {
            if (this.fullFileName == null)
            {
                return null;
            }

            if (OptimizeBufferReuse)
            {
                using (var targetBuilder = this.ReusableLayoutBuilder.Allocate())
                {
                    return this.fullFileName.RenderWithBuilder(logEvent, targetBuilder.Result);
                }
            }
            else
            {
                return this.fullFileName.Render(logEvent);
            }
        }

        /// <summary>
        /// NOTE! Obsolete, instead override Write(IList{AsyncLogEventInfo} logEvents)
        /// 
        /// Writes an array of logging events to the log target. By default it iterates on all
        /// events and passes them to "Write" method. Inheriting classes can use this method to
        /// optimize batch writes.
        /// </summary>
        /// <param name="logEvents">Logging events to be written out.</param>
        [Obsolete("Instead override Write(IList<AsyncLogEventInfo> logEvents. Marked obsolete on NLog 4.5")]
        protected override void Write(AsyncLogEventInfo[] logEvents)
        {
            Write((IList<AsyncLogEventInfo>)logEvents);
        }

        SortHelpers.KeySelector<AsyncLogEventInfo, string> getFullFileNameDelegate;

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
        protected override void Write(IList<AsyncLogEventInfo> logEvents)
        {
            if (getFullFileNameDelegate == null)
                getFullFileNameDelegate = c => this.GetFullFileName(c.LogEvent);

            var buckets = logEvents.BucketSort(getFullFileNameDelegate);

            using (var reusableStream = (OptimizeBufferReuse && logEvents.Count <= 1000) ? reusableAsyncFileWriteStream.Allocate() : reusableAsyncFileWriteStream.None)
            using (var allocatedStream = reusableStream.Result != null ? null : new MemoryStream())
            {
                var ms = allocatedStream != null ? allocatedStream : reusableStream.Result;

                foreach (var bucket in buckets)
                {
                    string fileName = bucket.Key;

                    ms.SetLength(0);
                    ms.Position = 0;

                    LogEventInfo firstLogEvent = null;

                    int bucketCount = bucket.Value.Count;

                    using (var targetBuilder = OptimizeBufferReuse ? ReusableLayoutBuilder.Allocate() : ReusableLayoutBuilder.None)
                    using (var targetBuffer = OptimizeBufferReuse ? reusableEncodingBuffer.Allocate() : reusableEncodingBuffer.None)
                    using (var targetStream = OptimizeBufferReuse ? reusableFileWriteStream.Allocate() : reusableFileWriteStream.None)
                    {
                        for (int i = 0; i < bucketCount; i++)
                        {
                            AsyncLogEventInfo ev = bucket.Value[i];
                            if (firstLogEvent == null)
                            {
                                firstLogEvent = ev.LogEvent;
                            }

                            if (targetBuilder.Result != null && targetStream.Result != null)
                            {
                                // For some CPU's then it is faster to write to a small MemoryStream, and then copy to the larger one
                                targetStream.Result.Position = 0;
                                targetStream.Result.SetLength(0);
                                targetBuilder.Result.ClearBuilder();
                                RenderFormattedMessageToStream(ev.LogEvent, targetBuilder.Result, targetBuffer.Result, targetStream.Result);
                                ms.Write(targetStream.Result.GetBuffer(), 0, (int)targetStream.Result.Length);
                            }
                            else
                            {
                                byte[] bytes = this.GetBytesToWrite(ev.LogEvent);
                                if (ms.Capacity == 0)
                                {
                                    ms.Capacity = GetMemoryStreamInitialSize(bucket.Value.Count, bytes.Length);
                                }
                                ms.Write(bytes, 0, bytes.Length);
                            }
                        }
                    }

                    Exception lastException;
                    this.FlushCurrentFileWrites(fileName, firstLogEvent, ms, out lastException);

                    for (int i = 0; i < bucketCount; ++i)
                    {
                        bucket.Value[i].Continuation(lastException);
                    }
                }
            }
        }

        /// <summary>
        /// Returns estimated size for memory stream, based on events count and first event size in bytes.
        /// </summary>
        /// <param name="eventsCount">Count of events</param>
        /// <param name="firstEventSize">Bytes count of first event</param>
        private int GetMemoryStreamInitialSize(int eventsCount, int firstEventSize)
        {
            if (eventsCount > 10)
                return ((eventsCount + 1) * firstEventSize / 1024 + 1) * 1024;

            if (eventsCount > 1)
                return (1 + eventsCount) * firstEventSize;

            return firstEventSize;
        }

        private void ProcessLogEvent(LogEventInfo logEvent, string fileName, ArraySegment<byte> bytesToWrite)
        {
            bool initializedNewFile = InitializeFile(fileName, logEvent, false);

            bool archiveOccurred = TryArchiveFile(fileName, logEvent, bytesToWrite.Count, initializedNewFile);
            if (archiveOccurred)
                initializedNewFile = InitializeFile(fileName, logEvent, false);

            this.WriteToFile(fileName, bytesToWrite, initializedNewFile);

            previousLogFileName = fileName;
            previousLogEventTimestamp = logEvent.TimeStamp;
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
            string text = this.GetFormattedMessage(logEvent);
            int textBytesCount = this.Encoding.GetByteCount(text);
            int newLineBytesCount = this.Encoding.GetByteCount(this.NewLineChars);
            byte[] bytes = new byte[textBytesCount + newLineBytesCount];
            this.Encoding.GetBytes(text, 0, text.Length, bytes, 0);
            this.Encoding.GetBytes(this.NewLineChars, 0, this.NewLineChars.Length, bytes, textBytesCount);
            return this.TransformBytes(bytes);
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
        /// Gets the bytes to be written to the file.
        /// </summary>
        /// <param name="logEvent">The log event to be formatted.</param>
        /// <param name="formatBuilder"><see cref="StringBuilder"/> to help format log event.</param>
        /// <param name="transformBuffer">Optional temporary char-array to help format log event.</param>
        /// <param name="streamTarget">Destination <see cref="MemoryStream"/> for the encoded result.</param>
        protected virtual void RenderFormattedMessageToStream(LogEventInfo logEvent, StringBuilder formatBuilder, char[] transformBuffer, MemoryStream streamTarget)
        {
            RenderFormattedMessage(logEvent, formatBuilder);
            formatBuilder.Append(NewLineChars);
            TransformBuilderToStream(logEvent, formatBuilder, transformBuffer, streamTarget);
        }

        /// <summary>
        /// Formats the log event for write.
        /// </summary>
        /// <param name="logEvent">The log event to be formatted.</param>
        /// <param name="target">Initially empty <see cref="StringBuilder"/> for the result.</param>
        protected virtual void RenderFormattedMessage(LogEventInfo logEvent, StringBuilder target)
        {
            this.Layout.RenderAppendBuilder(logEvent, target);
        }

        private void TransformBuilderToStream(LogEventInfo logEvent, StringBuilder builder, char[] transformBuffer, MemoryStream workStream)
        {
            builder.CopyToStream(workStream, this.Encoding, transformBuffer);
            TransformStream(logEvent, workStream);
        }

        /// <summary>
        /// Modifies the specified byte array before it gets sent to a file.
        /// </summary>
        /// <param name="logEvent">The LogEvent being written</param>
        /// <param name="stream">The byte array.</param>
        protected virtual void TransformStream(LogEventInfo logEvent, MemoryStream stream)
        {
        }

        private void FlushCurrentFileWrites(string currentFileName, LogEventInfo firstLogEvent, MemoryStream ms, out Exception lastException)
        {
            lastException = null;

            try
            {
                if (currentFileName != null)
                {
                    ArraySegment<byte> bytes = new ArraySegment<byte>(ms.GetBuffer(), 0, (int)ms.Length);
                    ProcessLogEvent(firstLogEvent, currentFileName, bytes);
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
        }

        /// <summary>
        /// Archives fileName to archiveFileName.
        /// </summary>
        /// <param name="fileName">File name to be archived.</param>
        /// <param name="archiveFileName">Name of the archive file.</param>
        private void ArchiveFile(string fileName, string archiveFileName)
        {
            string archiveFolderPath = Path.GetDirectoryName(archiveFileName);
            if (!Directory.Exists(archiveFolderPath))
                Directory.CreateDirectory(archiveFolderPath);

            if (string.Equals(fileName, archiveFileName, StringComparison.OrdinalIgnoreCase))
            {
                InternalLogger.Info("Archiving {0} skipped as ArchiveFileName equals FileName", fileName);
            }
            else if (EnableArchiveFileCompression)
            {
                InternalLogger.Info("Archiving {0} to compressed {1}", fileName, archiveFileName);
                FileCompressor.CompressFile(fileName, archiveFileName);
                DeleteAndWaitForFileDelete(fileName);
            }
            else
            {
                InternalLogger.Info("Archiving {0} to {1}", fileName, archiveFileName);
                if (File.Exists(archiveFileName))
                {
                    //todo handle double footer
                    InternalLogger.Info("Already exists, append to {0}", archiveFileName);

                    //todo maybe needs a better filelock behaviour

                    //copy to archive file.
                    using (FileStream fileStream = File.Open(fileName, FileMode.Open))
                    using (FileStream archiveFileStream = File.Open(archiveFileName, FileMode.Append))
                    {
                        fileStream.CopyAndSkipBom(archiveFileStream, Encoding);
                        //clear old content
                        fileStream.SetLength(0);
                        fileStream.Close(); // This flushes the content, too.
#if NET3_5
                        archiveFileStream.Flush();
#else
                        archiveFileStream.Flush(true);
#endif
                    }
                }
                else
                {
                    File.Move(fileName, archiveFileName);
                }
            }
        }

        private static bool DeleteOldArchiveFile(string fileName)
        {
            try
            {
                InternalLogger.Info("Deleting old archive file: '{0}'.", fileName);
                File.Delete(fileName);
                return true;
            }
            catch (DirectoryNotFoundException exception)
            {
                //never rethrow this, as this isn't an exceptional case.
                InternalLogger.Debug(exception, "Failed to delete old log file '{0}' as directory is missing.", fileName);
                return false;
            }
            catch (Exception exception)
            {
                InternalLogger.Warn(exception, "Failed to delete old archive file: '{0}'.", fileName);
                if (exception.MustBeRethrown())
                {
                    throw;
                }

                return false;
            }
        }

        private static void DeleteAndWaitForFileDelete(string fileName)
        {
            try
            {
                var originalFileCreationTime = (new FileInfo(fileName)).CreationTime;
                if (DeleteOldArchiveFile(fileName) && File.Exists(fileName))
                {
                    FileInfo currentFileInfo;
                    for (int i = 0; i < 120; ++i)
                    {
                        Thread.Sleep(100);
                        currentFileInfo = new FileInfo(fileName);
                        if (!currentFileInfo.Exists || currentFileInfo.CreationTime != originalFileCreationTime)
                            return;
                    }

                    InternalLogger.Warn("Timeout while deleting old archive file: '{0}'.", fileName);
                }
            }
            catch (Exception exception)
            {
                InternalLogger.Warn(exception, "Failed to delete old archive file: '{0}'.", fileName);
                if (exception.MustBeRethrown())
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// Gets the correct formatting <see langword="String"/> to be used based on the value of <see
        /// cref="P:ArchiveEvery"/> for converting <see langword="DateTime"/> values which will be inserting into file
        /// names during archiving.
        /// 
        /// This value will be computed only when a empty value or <see langword="null"/> is passed into <paramref name="defaultFormat"/>
        /// </summary>
        /// <param name="defaultFormat">Date format to used irrespectively of <see cref="P:ArchiveEvery"/> value.</param>
        /// <returns>Formatting <see langword="String"/> for dates.</returns>
        private string GetArchiveDateFormatString(string defaultFormat)
        {
            // If archiveDateFormat is not set in the config file, use a default 
            // date format string based on the archive period.
            string formatString = defaultFormat;
            if (string.IsNullOrEmpty(formatString))
            {
                switch (this.ArchiveEvery)
                {
                    case FileArchivePeriod.Year: formatString = "yyyy"; break;
                    case FileArchivePeriod.Month: formatString = "yyyyMM"; break;
                    default: formatString = "yyyyMMdd"; break;      // Also for Weekdays
                    case FileArchivePeriod.Hour: formatString = "yyyyMMddHH"; break;
                    case FileArchivePeriod.Minute: formatString = "yyyyMMddHHmm"; break;
                }
            }
            return formatString;
        }

        private DateTime GetArchiveDate(string fileName, LogEventInfo logEvent)
        {
            var lastWriteTimeUtc = this.fileAppenderCache.GetFileLastWriteTimeUtc(fileName, true);

            //todo null check
            var lastWriteTime = TimeSource.Current.FromSystemTime(lastWriteTimeUtc.Value);

            InternalLogger.Trace("Calculating archive date. Last write time: {0}; Previous log event time: {1}", lastWriteTime, previousLogEventTimestamp);

            bool previousLogIsMoreRecent = previousLogEventTimestamp.HasValue && (previousLogEventTimestamp.Value > lastWriteTime);
            if (previousLogIsMoreRecent)
            {
                InternalLogger.Trace("Using previous log event time (is more recent)");
                return previousLogEventTimestamp.Value;
            }

            if (previousLogEventTimestamp.HasValue && PreviousLogOverlappedPeriod(logEvent, lastWriteTime))
            {
                InternalLogger.Trace("Using previous log event time (previous log overlapped period)");
                return previousLogEventTimestamp.Value;
            }

            InternalLogger.Trace("Using last write time");
            return lastWriteTime;
        }

        private bool PreviousLogOverlappedPeriod(LogEventInfo logEvent, DateTime lastWrite)
        {
            DateTime timestamp;
            if(!previousLogEventTimestamp.HasValue)
                return false;
            else
                timestamp = previousLogEventTimestamp.Value;

            string formatString = GetArchiveDateFormatString(string.Empty);
            string lastWriteTimeString = lastWrite.ToString(formatString, CultureInfo.InvariantCulture);
            string logEventTimeString = logEvent.TimeStamp.ToString(formatString, CultureInfo.InvariantCulture);

            if (lastWriteTimeString != logEventTimeString)
                return false;

            DateTime periodAfterPreviousLogEventTime;
            switch (this.ArchiveEvery)
            {
                case FileArchivePeriod.Year: periodAfterPreviousLogEventTime = timestamp.AddYears(1); break;
                case FileArchivePeriod.Month: periodAfterPreviousLogEventTime = timestamp.AddMonths(1); break;
                case FileArchivePeriod.Day: periodAfterPreviousLogEventTime = timestamp.AddDays(1); break;
                case FileArchivePeriod.Hour: periodAfterPreviousLogEventTime = timestamp.AddHours(1); break;
                case FileArchivePeriod.Minute: periodAfterPreviousLogEventTime = timestamp.AddMinutes(1); break;
                case FileArchivePeriod.Sunday: periodAfterPreviousLogEventTime = CalculateNextWeekday(timestamp, DayOfWeek.Sunday); break;
                case FileArchivePeriod.Monday: periodAfterPreviousLogEventTime = CalculateNextWeekday(timestamp, DayOfWeek.Monday); break;
                case FileArchivePeriod.Tuesday: periodAfterPreviousLogEventTime = CalculateNextWeekday(timestamp, DayOfWeek.Tuesday); break;
                case FileArchivePeriod.Wednesday: periodAfterPreviousLogEventTime = CalculateNextWeekday(timestamp, DayOfWeek.Wednesday); break;
                case FileArchivePeriod.Thursday: periodAfterPreviousLogEventTime = CalculateNextWeekday(timestamp, DayOfWeek.Thursday); break;
                case FileArchivePeriod.Friday: periodAfterPreviousLogEventTime = CalculateNextWeekday(timestamp, DayOfWeek.Friday); break;
                case FileArchivePeriod.Saturday: periodAfterPreviousLogEventTime = CalculateNextWeekday(timestamp, DayOfWeek.Saturday); break;
                default: return false;
            }

            string periodAfterPreviousLogEventTimeString = periodAfterPreviousLogEventTime.ToString(formatString, CultureInfo.InvariantCulture);
            return lastWriteTimeString == periodAfterPreviousLogEventTimeString;
        }

        /// <summary>
        /// Calculate the DateTime of the requested day of the week.
        /// </summary>
        /// <param name="previousLogEventTimestamp">The DateTime of the previous log event.</param>
        /// <param name="dayOfWeek">The next occuring day of the week to return a DateTime for.</param>
        /// <returns>The DateTime of the next occuring dayOfWeek.</returns>
        /// <remarks>For example: if previousLogEventTimestamp is Thursday 2017-03-02 and dayOfWeek is Sunday, this will return
        ///  Sunday 2017-03-05. If dayOfWeek is Thursday, this will return *next* Thursday 2017-03-09.</remarks>
        public static DateTime CalculateNextWeekday(DateTime previousLogEventTimestamp, DayOfWeek dayOfWeek)
        {
            // Shamelessly taken from http://stackoverflow.com/a/7611480/1354930
            int start = (int)previousLogEventTimestamp.DayOfWeek;
            int target = (int)dayOfWeek;
            if(target <= start)
                target += 7;
            return previousLogEventTimestamp.AddDays(target - start);
        }

        /// <summary>
        /// Invokes the archiving process after determining when and which type of archiving is required.
        /// </summary>
        /// <param name="fileName">File name to be checked and archived.</param>
        /// <param name="eventInfo">Log event that the <see cref="FileTarget"/> instance is currently processing.</param>
        /// <param name="initializedNewFile">File has just been opened.</param>
        private void DoAutoArchive(string fileName, LogEventInfo eventInfo, bool initializedNewFile)
        {
            var fileInfo = new FileInfo(fileName);
            if (!fileInfo.Exists)
            {
                // Close possible stale file handles
                this.fileAppenderCache.InvalidateAppender(fileName);
                this.initializedFiles.Remove(fileName);
                return;
            }

            string archiveFilePattern = GetArchiveFileNamePattern(fileName, eventInfo);

            if (string.IsNullOrEmpty(archiveFilePattern))
            {
                InternalLogger.Warn("Skip auto archive because archiveFilePattern is NULL");
                return;
            }

            var fileArchiveStyle = GetFileArchiveHelper(archiveFilePattern);
            var existingArchiveFiles = fileArchiveStyle.GetExistingArchiveFiles(archiveFilePattern);

            if (this.MaxArchiveFiles == 1)
            {
                // Perform archive cleanup before generating the next filename,
                // as next archive-filename can be affected by existing files.
                for (int i = existingArchiveFiles.Count - 1; i >= 0; i--)
                {
                    var oldArchiveFile = existingArchiveFiles[i];
                    if (!string.Equals(oldArchiveFile.FileName, fileInfo.FullName, StringComparison.InvariantCultureIgnoreCase))
                    {
                        DeleteOldArchiveFile(oldArchiveFile.FileName);
                        existingArchiveFiles.RemoveAt(i);
                    }
                }

                if (initializedNewFile)
                {
                    if (string.Equals(Path.GetDirectoryName(archiveFilePattern), fileInfo.DirectoryName, StringComparison.OrdinalIgnoreCase))
                    {
                        this.initializedFiles.Remove(fileName);
                        DeleteOldArchiveFile(fileName);
                        return;
                    }
                }
            }

            DateTime archiveDate = GetArchiveDate(fileName, eventInfo);
            var archiveFileName = fileArchiveStyle.GenerateArchiveFileName(archiveFilePattern, archiveDate, existingArchiveFiles);
            if (archiveFileName != null)
            {
                if (initializedNewFile)
                {
                    this.initializedFiles.Remove(fileName);
                }
                else
                {
                    FinalizeFile(fileName, isArchiving: true);
                }

                if (string.Equals(Path.GetDirectoryName(archiveFileName.FileName), fileInfo.DirectoryName, StringComparison.OrdinalIgnoreCase))
                {
                    // Extra handling when archive-directory is the same as logging-directory
                    for (int i = 0; i < existingArchiveFiles.Count; ++i)
                    {
                        if (string.Equals(existingArchiveFiles[i].FileName, fileInfo.FullName, StringComparison.OrdinalIgnoreCase))
                        {
                            existingArchiveFiles.RemoveAt(i);
                            break;
                        }
                    }
                }

                existingArchiveFiles.Add(archiveFileName);

                var cleanupArchiveFiles = fileArchiveStyle.CheckArchiveCleanup(archiveFilePattern, existingArchiveFiles, this.MaxArchiveFiles);
                foreach (var oldArchiveFile in cleanupArchiveFiles)
                    DeleteOldArchiveFile(oldArchiveFile.FileName);

                ArchiveFile(fileInfo.FullName, archiveFileName.FileName);
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
            if (this.fullArchiveFileName == null)
            {
                if (EnableArchiveFileCompression)
                    return Path.ChangeExtension(fileName, ".zip");
                else
                    return fileName;
            }
            else
            {
                //The archive file name is given. There are two possibilities
                //(1) User supplied the Filename with pattern
                //(2) User supplied the normal filename
                string archiveFileName = this.fullArchiveFileName.Render(eventInfo);
                return archiveFileName;
            }
        }

        /// <summary>
        /// Archives the file if it should be archived.
        /// </summary>
        /// <param name="fileName">The file name to check for.</param>
        /// <param name="ev">Log event that the <see cref="FileTarget"/> instance is currently processing.</param>
        /// <param name="upcomingWriteSize">The size in bytes of the next chunk of data to be written in the file.</param>
        /// <param name="initializedNewFile">File has just been opened.</param>
        /// <returns>True when archive operation of the file was completed (by this target or a concurrent target)</returns>
        private bool TryArchiveFile(string fileName, LogEventInfo ev, int upcomingWriteSize, bool initializedNewFile)
        {
            string archiveFile = string.Empty;

#if SupportsMutex
            Mutex archiveMutex = null;
#endif

            try
            {
                archiveFile = this.GetArchiveFileName(fileName, ev, upcomingWriteSize);
                if (!string.IsNullOrEmpty(archiveFile))
                {
#if SupportsMutex
                    // Acquire the mutex from the file-appender, before closing the file-apppender (remember not to close the Mutex)
                    archiveMutex = this.fileAppenderCache.GetArchiveMutex(fileName);
                    if (archiveMutex == null && fileName != archiveFile)
                        archiveMutex = this.fileAppenderCache.GetArchiveMutex(archiveFile);
#endif

#if !SILVERLIGHT && !__IOS__ && !__ANDROID__
                    this.fileAppenderCache.InvalidateAppendersForInvalidFiles();
#endif

                    // Close possible stale file handles, before doing extra check
                    if (archiveFile != fileName)
                        this.fileAppenderCache.InvalidateAppender(fileName);
                    this.fileAppenderCache.InvalidateAppender(archiveFile);
                }
                else
                {
#if !SILVERLIGHT && !__IOS__ && !__ANDROID__
                    this.fileAppenderCache.InvalidateAppendersForInvalidFiles();
#endif
                }
            }
            catch (Exception exception)
            {
                InternalLogger.Warn(exception, "Failed to check archive for file '{0}'.", fileName);
                if (exception.MustBeRethrown())
                {
                    throw;
                }
            }

            if (!string.IsNullOrEmpty(archiveFile))
            {
                try
                {
#if SupportsMutex
                    try
                    {
                        if (archiveMutex != null)
                            archiveMutex.WaitOne();
                    }
                    catch (AbandonedMutexException)
                    {
                        // ignore the exception, another process was killed without properly releasing the mutex
                        // the mutex has been acquired, so proceed to writing
                        // See: http://msdn.microsoft.com/en-us/library/system.threading.abandonedmutexexception.aspx
                    }
#endif

                    // Check again if archive is needed. We could have been raced by another process
                    var validatedArchiveFile = this.GetArchiveFileName(fileName, ev, upcomingWriteSize);
                    if (string.IsNullOrEmpty(validatedArchiveFile))
                    {
                        if (archiveFile != fileName)
                            this.initializedFiles.Remove(fileName);
                        this.initializedFiles.Remove(archiveFile);
                        return true;
                    }

                    archiveFile = validatedArchiveFile;
                    this.DoAutoArchive(archiveFile, ev, initializedNewFile);
                    return true;
                }
                catch (Exception exception)
                {
                    InternalLogger.Warn(exception, "Failed to archive file '{0}'.", archiveFile);
                    if (exception.MustBeRethrown())
                    {
                        throw;
                    }
                }
                finally
                {
#if SupportsMutex
                    if (archiveMutex != null)
                        archiveMutex.ReleaseMutex();
#endif
                }
            }

            return false;
        }

        /// <summary>
        /// Indicates if the automatic archiving process should be executed.
        /// </summary>
        /// <param name="fileName">File name to be written.</param>
        /// <param name="ev">Log event that the <see cref="FileTarget"/> instance is currently processing.</param>
        /// <param name="upcomingWriteSize">The size in bytes of the next chunk of data to be written in the file.</param>
        /// <returns>Filename to archive. If <c>null</c>, then nothing to archive.</returns>
        private string GetArchiveFileName(string fileName, LogEventInfo ev, int upcomingWriteSize)
        {
            var hasFileName = !(fileName == null && previousLogFileName == null);
            if (hasFileName)
            {
                return GetArchiveFileNameBasedOnFileSize(fileName, upcomingWriteSize) ??
                       GetArchiveFileNameBasedOnTime(fileName, ev);
            }

            return null;
        }

        /// <summary>
        /// Returns the correct filename to archive
        /// </summary>
        /// <returns></returns>
        private string GetPotentialFileForArchiving(string fileName)
        {
            if (string.Equals(fileName, previousLogFileName, StringComparison.OrdinalIgnoreCase))
            {
                //both the same, so don't care
                return fileName;
            }

            if (string.IsNullOrEmpty(previousLogFileName))
            {
                return fileName;
            }

            if (string.IsNullOrEmpty(fileName))
            {
                return previousLogFileName;
            }

            //this is an expensive call
            var fileLength = this.fileAppenderCache.GetFileLength(fileName, true);
            string fileToArchive = fileLength != null ? fileName : previousLogFileName;
            return fileToArchive;
        }

        /// <summary>
        /// Gets the file name for archiving, or null if archiving should not occur based on file size.
        /// </summary>
        /// <param name="fileName">File name to be written.</param>
        /// <param name="upcomingWriteSize">The size in bytes of the next chunk of data to be written in the file.</param>
        /// <returns>Filename to archive. If <c>null</c>, then nothing to archive.</returns>
        private string GetArchiveFileNameBasedOnFileSize(string fileName, int upcomingWriteSize)
        {
            if (this.ArchiveAboveSize == ArchiveAboveSizeDisabled)
            {
                return null;
            }

            fileName = GetPotentialFileForArchiving(fileName);

            if (fileName == null)
            {
                return null;
            }

            var length = this.fileAppenderCache.GetFileLength(fileName, true);
            if (length == null)
            {
                return null;
            }

            var shouldArchive = length.Value + upcomingWriteSize > this.ArchiveAboveSize;
            if (shouldArchive)
            {
                return fileName;
            }
            return null;

        }

        /// <summary>
        /// Returns the file name for archiving, or null if archiving should not occur based on date/time.
        /// </summary>
        /// <param name="fileName">File name to be written.</param>
        /// <param name="logEvent">Log event that the <see cref="FileTarget"/> instance is currently processing.</param>
        /// <returns>Filename to archive. If <c>null</c>, then nothing to archive.</returns>
        private string GetArchiveFileNameBasedOnTime(string fileName, LogEventInfo logEvent)
        {
            if (this.ArchiveEvery == FileArchivePeriod.None)
            {
                return null;
            }

            fileName = GetPotentialFileForArchiving(fileName);

            if (fileName == null)
            {
                return null;
            }

            var creationTimeSource = this.fileAppenderCache.GetFileCreationTimeSource(fileName, true);
            if (creationTimeSource == null)
            {
                return null;
            }

            DateTime fileCreateTime = TruncateArchiveTime(creationTimeSource.Value, this.ArchiveEvery);
            DateTime logEventTime = TruncateArchiveTime(logEvent.TimeStamp, this.ArchiveEvery);
            if (fileCreateTime != logEventTime)
            {
                string formatString = GetArchiveDateFormatString(string.Empty);
                string fileCreated = creationTimeSource.Value.ToString(formatString, CultureInfo.InvariantCulture);
                string logEventRecorded = logEvent.TimeStamp.ToString(formatString, CultureInfo.InvariantCulture);

                var shouldArchive = fileCreated != logEventRecorded;
                if (shouldArchive)
                {
                    return fileName;
                }
            }
            return null;
        }

        /// <summary>
        /// Truncates the input-time, so comparison of low resolution times (like dates) are not affected by ticks
        /// </summary>
        /// <param name="input">High resolution Time</param>
        /// <param name="resolution">Time Resolution Level</param>
        /// <returns>Truncated Low Resolution Time</returns>
        private static DateTime TruncateArchiveTime(DateTime input, FileArchivePeriod resolution)
        {
            switch (resolution)
            {
                case FileArchivePeriod.Year:
                    return new DateTime(input.Year, 1, 1, 0, 0, 0, 0, input.Kind);
                case FileArchivePeriod.Month:
                    return new DateTime(input.Year, input.Month, 1, 0, 0, 0, input.Kind);
                case FileArchivePeriod.Day:
                    return input.Date;
                case FileArchivePeriod.Hour:
                    return input.AddTicks(-(input.Ticks % TimeSpan.TicksPerHour));
                case FileArchivePeriod.Minute:
                    return input.AddTicks(-(input.Ticks % TimeSpan.TicksPerMinute));
                case FileArchivePeriod.Sunday:
                    return CalculateNextWeekday(input.Date, DayOfWeek.Sunday);
                case FileArchivePeriod.Monday:
                    return CalculateNextWeekday(input.Date, DayOfWeek.Monday);
                case FileArchivePeriod.Tuesday:
                    return CalculateNextWeekday(input.Date, DayOfWeek.Tuesday);
                case FileArchivePeriod.Wednesday:
                    return CalculateNextWeekday(input.Date, DayOfWeek.Wednesday);
                case FileArchivePeriod.Thursday:
                    return CalculateNextWeekday(input.Date, DayOfWeek.Thursday);
                case FileArchivePeriod.Friday:
                    return CalculateNextWeekday(input.Date, DayOfWeek.Friday);
                case FileArchivePeriod.Saturday:
                    return CalculateNextWeekday(input.Date, DayOfWeek.Saturday);
                default:
                    return input;   // Unknown time-resolution-truncate, leave unchanged
            }
        }

        private void AutoClosingTimerCallback(object sender, EventArgs state)
        {
            try
            {
                lock (this.SyncRoot)
                {
                    if (!this.IsInitialized)
                    {
                        return;
                    }

                    DateTime expireTime = this.OpenFileCacheTimeout > 0 ? DateTime.UtcNow.AddSeconds(-this.OpenFileCacheTimeout) : DateTime.MinValue;
                    this.fileAppenderCache.CloseAppenders(expireTime);
                }
            }
            catch (Exception exception)
            {
                InternalLogger.Warn(exception, "Exception in AutoClosingTimerCallback.");

                if (exception.MustBeRethrownImmediately())
                {
                    throw;  // Throwing exceptions here will crash the entire application (.NET 2.0 behavior)
                }
            }
        }

        /// <summary>
        /// Evaluates which parts of a file should be written (header, content, footer) based on various properties of
        /// <see cref="FileTarget"/> instance and writes them.
        /// </summary>
        /// <param name="fileName">File name to be written.</param>
        /// <param name="bytes">Raw sequence of <see langword="byte"/> to be written into the content part of the file.</param>        
        /// <param name="initializedNewFile">File has just been opened.</param>
        private void WriteToFile(string fileName, ArraySegment<byte> bytes, bool initializedNewFile)
        {
            if (this.ReplaceFileContentsOnEachWrite)
            {
                ReplaceFileContent(fileName, bytes, true);
                return;
            }

            BaseFileAppender appender = this.fileAppenderCache.AllocateAppender(fileName);
            try
            {
                if (initializedNewFile)
                {
                    this.WriteHeader(appender);
                }

                appender.Write(bytes.Array, bytes.Offset, bytes.Count);

                if (this.AutoFlush)
                {
                    appender.Flush();
                }
            }
            catch (Exception ex)
            {
                InternalLogger.Error(ex, "Failed write to file '{0}'.", fileName);
                this.fileAppenderCache.InvalidateAppender(fileName);
                throw;
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
            bool initializedNewFile = false;

            if (!justData)
            {
                var now = logEvent.TimeStamp;
                DateTime lastTime;
                if (!this.initializedFiles.TryGetValue(fileName, out lastTime))
                {
                    ProcessOnStartup(fileName, logEvent);

                    this.initializedFiles[fileName] = now;
                    this.initializedFilesCounter++;
                    initializedNewFile = true;

                    if (this.initializedFilesCounter >= FileTarget.InitializedFilesCounterMax)
                    {
                        this.initializedFilesCounter = 0;
                        this.CleanupInitializedFiles();
                    }
                }
                if (lastTime != now)
                    this.initializedFiles[fileName] = now;
            }

            return initializedNewFile;
        }

        /// <summary>
        /// Writes the file footer and finalizes the file in <see cref="FileTarget"/> instance internal structures.
        /// </summary>
        /// <param name="fileName">File name to close.</param>
        /// <param name="isArchiving">Indicates if the file is being finalized for archiving.</param>
        private void FinalizeFile(string fileName, bool isArchiving = false)
        {
            if ((isArchiving) || (!this.WriteFooterOnArchivingOnly))
                WriteFooter(fileName);

            this.fileAppenderCache.InvalidateAppender(fileName);
            this.initializedFiles.Remove(fileName);
        }

        /// <summary>
        /// Writes the footer information to a file.
        /// </summary>
        /// <param name="fileName">The file path to write to.</param>
        private void WriteFooter(string fileName)
        {
            ArraySegment<byte> footerBytes = this.GetLayoutBytes(Footer);
            if (footerBytes.Count > 0)
            {
                if (File.Exists(fileName))
                {
                    this.WriteToFile(fileName, footerBytes, false);
                }
            }
        }

        /// <summary>
        /// Invokes the archiving and clean up of older archive file based on the values of <see
        /// cref="NLog.Targets.FileTarget.ArchiveOldFileOnStartup"/> and <see
        /// cref="NLog.Targets.FileTarget.DeleteOldFileOnStartup"/> properties respectively.
        /// </summary>
        /// <param name="fileName">File name to be written.</param>
        /// <param name="logEvent">Log event that the <see cref="FileTarget"/> instance is currently processing.</param>
        private void ProcessOnStartup(string fileName, LogEventInfo logEvent)
        {
            RefreshArchiveFilePatternToWatch(fileName, logEvent);

            if (this.ArchiveOldFileOnStartup)
            {
                try
                {
                    this.DoAutoArchive(fileName, logEvent, true);
                }
                catch (Exception exception)
                {
                    InternalLogger.Warn(exception, "Unable to archive old log file '{0}'.", fileName);

                    if (exception.MustBeRethrown())
                    {
                        throw;
                    }
                }
            }

            if (this.DeleteOldFileOnStartup)
            {
                DeleteOldArchiveFile(fileName);
            }

            string archiveFilePattern = GetArchiveFileNamePattern(fileName, logEvent);
            if (!string.IsNullOrEmpty(archiveFilePattern))
            {
                if (FileArchiveModeFactory.ShouldDeleteOldArchives(this.MaxArchiveFiles))
                {
                    var fileArchiveStyle = GetFileArchiveHelper(archiveFilePattern);
                    if (fileArchiveStyle.AttemptCleanupOnInitializeFile(archiveFilePattern, this.MaxArchiveFiles))
                    {
                        var existingArchiveFiles = fileArchiveStyle.GetExistingArchiveFiles(archiveFilePattern);
                        var cleanupArchiveFiles = fileArchiveStyle.CheckArchiveCleanup(archiveFilePattern, existingArchiveFiles, this.MaxArchiveFiles);
                        foreach (var oldFile in cleanupArchiveFiles)
                        {
                            DeleteOldArchiveFile(oldFile.FileName);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Creates the file specified in <paramref name="fileName"/> and writes the file content in each entirety i.e.
        /// Header, Content and Footer.
        /// </summary>
        /// <param name="fileName">The name of the file to be written.</param>
        /// <param name="bytes">Sequence of <see langword="byte"/> to be written in the content section of the file.</param>
        /// <param name="firstAttempt">First attempt to write?</param>
        /// <remarks>This method is used when the content of the log file is re-written on every write.</remarks>
        private void ReplaceFileContent(string fileName, ArraySegment<byte> bytes, bool firstAttempt)
        {
            try
            {
                using (FileStream fs = File.Create(fileName))
                {
                    ArraySegment<byte> headerBytes = this.GetLayoutBytes(Header);
                    if (headerBytes.Count > 0)
                    {
                        fs.Write(headerBytes.Array, headerBytes.Offset, headerBytes.Count);
                    }

                    fs.Write(bytes.Array, bytes.Offset, bytes.Count);

                    ArraySegment<byte> footerBytes = this.GetLayoutBytes(Footer);
                    if (footerBytes.Count > 0)
                    {
                        fs.Write(footerBytes.Array, footerBytes.Offset, footerBytes.Count);
                    }
                }
            }
            catch (DirectoryNotFoundException)
            {
                if (!this.CreateDirs || !firstAttempt)
                {
                    throw;
                }
                Directory.CreateDirectory(Path.GetDirectoryName(fileName));
                //retry.
                ReplaceFileContent(fileName, bytes, false);
            }
        }

        /// <summary>
        /// Writes the header information to a file.
        /// </summary>
        /// <param name="appender">File appender associated with the file.</param>
        private void WriteHeader(BaseFileAppender appender)
        {
            //performance: cheap check before checking file info 
            if (Header == null) return;

            //todo replace with hasWritten?
            var length = appender.GetFileLength();
            //  Write header only on empty files or if file info cannot be obtained.
            if (length == null || length == 0)
            {
                ArraySegment<byte> headerBytes = this.GetLayoutBytes(Header);
                if (headerBytes.Count > 0)
                {
                    appender.Write(headerBytes.Array, headerBytes.Offset, headerBytes.Count);
                }
            }
        }

        /// <summary>
        /// The sequence of <see langword="byte"/> to be written in a file after applying any formating and any
        /// transformations required from the <see cref="Layout"/>.
        /// </summary>
        /// <param name="layout">The layout used to render output message.</param>
        /// <returns>Sequence of <see langword="byte"/> to be written.</returns>
        /// <remarks>Usually it is used to render the header and hooter of the files.</remarks>
        private ArraySegment<byte> GetLayoutBytes(Layout layout)
        {
            if (layout == null)
            {
                return default(ArraySegment<byte>);
            }

            if (OptimizeBufferReuse)
            {
                using (var targetBuilder = this.ReusableLayoutBuilder.Allocate())
                using (var targetBuffer = this.reusableEncodingBuffer.Allocate())
                {
                    var nullEvent = LogEventInfo.CreateNullEvent();
                    layout.RenderAppendBuilder(nullEvent, targetBuilder.Result);
                    targetBuilder.Result.Append(NewLineChars);
                    using (MemoryStream ms = new MemoryStream(targetBuilder.Result.Length))
                    {
                        TransformBuilderToStream(nullEvent, targetBuilder.Result, targetBuffer.Result, ms);
                        return new ArraySegment<byte>(ms.ToArray());
                    }
                }
            }
            else
            {
                string renderedText = layout.Render(LogEventInfo.CreateNullEvent()) + this.NewLineChars;
                return new ArraySegment<byte>(this.TransformBytes(this.Encoding.GetBytes(renderedText)));
            }
        }
    }
}