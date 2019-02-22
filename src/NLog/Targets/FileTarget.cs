// 
// Copyright (c) 2004-2019 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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

#if !SILVERLIGHT && !__ANDROID__ && !__IOS__ && !NETSTANDARD1_3
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
    using NLog.Common;
    using NLog.Config;
    using NLog.Internal;
    using NLog.Internal.FileAppenders;
    using NLog.Layouts;
    using NLog.Targets.FileArchiveModes;
    using NLog.Time;

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
        private readonly Dictionary<string, DateTime> _initializedFiles = new Dictionary<string, DateTime>(StringComparer.OrdinalIgnoreCase);

        private LineEndingMode _lineEndingMode = LineEndingMode.Default;


        /// <summary>
        /// List of the associated file appenders with the <see cref="FileTarget"/> instance.
        /// </summary>
        private FileAppenderCache _fileAppenderCache;

        IFileArchiveMode GetFileArchiveHelper(string archiveFilePattern)
        {
            return _fileArchiveHelper ?? (_fileArchiveHelper = FileArchiveModeFactory.CreateArchiveStyle(archiveFilePattern, ArchiveNumbering, GetArchiveDateFormatString(ArchiveDateFormat), ArchiveFileName != null, MaxArchiveFiles));
        }
        private IFileArchiveMode _fileArchiveHelper;

        private Timer _autoClosingTimer;

        /// <summary>
        /// The number of initialised files at any one time.
        /// </summary>
        private int _initializedFilesCounter;

        /// <summary>
        /// The maximum number of archive files that should be kept.
        /// </summary>
        private int _maxArchiveFiles;

        /// <summary>
        /// The filename as target
        /// </summary>
        private FilePathLayout _fullFileName;

        /// <summary>
        /// The archive file name as target
        /// </summary>
        private FilePathLayout _fullArchiveFileName;

        private FileArchivePeriod _archiveEvery;
        private long _archiveAboveSize;

        private bool _enableArchiveFileCompression;

        /// <summary>
        /// The date of the previous log event.
        /// </summary>
        private DateTime? _previousLogEventTimestamp;

        /// <summary>
        /// The file name of the previous log event.
        /// </summary>
        private string _previousLogFileName;

        private bool? _concurrentWrites;
        private bool _keepFileOpen;
        private bool _cleanupFileName;
        private FilePathKind _fileNameKind;
        private FilePathKind _archiveFileKind;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileTarget" /> class.
        /// </summary>
        /// <remarks>
        /// The default value of the layout is: <code>${longdate}|${level:uppercase=true}|${logger}|${message}</code>
        /// </remarks>
        public FileTarget()
        {
            ArchiveNumbering = ArchiveNumberingMode.Sequence;
            _maxArchiveFiles = 0;
            ConcurrentWriteAttemptDelay = 1;
            ArchiveEvery = FileArchivePeriod.None;
            ArchiveAboveSize = ArchiveAboveSizeDisabled;
            ConcurrentWriteAttempts = 10;
            ConcurrentWrites = true;
#if SILVERLIGHT || NETSTANDARD1_0
            Encoding = Encoding.UTF8;
#else
            Encoding = Encoding.Default;
#endif
            BufferSize = 32768;
            AutoFlush = true;
#if !SILVERLIGHT && !__IOS__ && !__ANDROID__
            FileAttributes = Win32FileAttributes.Normal;
#endif
            LineEnding = LineEndingMode.Default;
            EnableFileDelete = true;
            OpenFileCacheTimeout = -1;
            OpenFileCacheSize = 5;
            CreateDirs = true;
            ForceManaged = false;
            ArchiveDateFormat = string.Empty;

            _fileAppenderCache = FileAppenderCache.Empty;
            CleanupFileName = true;

            WriteFooterOnArchivingOnly = false;

            OptimizeBufferReuse = GetType() == typeof(FileTarget);    // Class not sealed, reduce breaking changes
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
            Name = name;
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
                return _fullFileName?.GetLayout();
            }
            set
            {
                _fullFileName = CreateFileNameLayout(value);
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
        /// <docgen category='Output Options' order='10' />
        [DefaultValue(true)]
        public bool CleanupFileName
        {
            get => _cleanupFileName;
            set
            {
                if (_cleanupFileName != value)
                {
                    _cleanupFileName = value;
                    _fullFileName = CreateFileNameLayout(FileName);
                    _fullArchiveFileName = CreateFileNameLayout(ArchiveFileName);
                    ResetFileAppenders("CleanupFileName Changed");
                }
            }
        }

        /// <summary>
        /// Is the  <see cref="FileName"/> an absolute or relative path?
        /// </summary>
        /// <docgen category='Output Options' order='10' />
        [DefaultValue(FilePathKind.Unknown)]
        public FilePathKind FileNameKind
        {
            get => _fileNameKind;
            set
            {
                if (_fileNameKind != value)
                {
                    _fileNameKind = value;
                    _fullFileName = CreateFileNameLayout(FileName);
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
            get => _keepFileOpen;
            set
            {
                if (_keepFileOpen != value)
                {
                    _keepFileOpen = value;
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

        bool ICreateFileParameters.IsArchivingEnabled => IsArchivingEnabled;

        /// <summary>
        /// Gets or sets the line ending mode.
        /// </summary>
        /// <docgen category='Layout Options' order='10' />
        [Advanced]
        public LineEndingMode LineEnding
        {
            get => _lineEndingMode;
            set => _lineEndingMode = value;
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
        /// Gets or sets the maximum number of seconds before open files are flushed. If this number is negative or zero
        /// the files are not flushed by timer.
        /// </summary>
        /// <docgen category='Performance Tuning Options' order='10' />
        public int OpenFileFlushTimeout { get; set; }

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
        /// <docgen category='Performance Tuning Options' order='10' />
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
            get
            {
#if SupportsMutex

                return _concurrentWrites ?? PlatformDetector.SupportsSharableMutex;
#else
                return _concurrentWrites ?? false;  // Better user experience for mobile platforms
#endif
            }
            set
            {
                if (_concurrentWrites != value)
                {
                    _concurrentWrites = value;
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
        /// Gets or sets a value indicating whether to write BOM (byte order mark) in created files
        /// </summary>
        /// <docgen category='Output Options' order='10' />
        [DefaultValue(false)]
        public bool WriteBom { get; set; }

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
            get => _archiveDateFormat;
            set
            {
                if (_archiveDateFormat != value)
                {
                    _archiveDateFormat = value;
                    ResetFileAppenders("ArchiveDateFormat Changed"); // Reset archive file-monitoring
                }
            }
        }
        private string _archiveDateFormat;

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
            get => _archiveAboveSize;
            set
            {
                if ((_archiveAboveSize == ArchiveAboveSizeDisabled) != (value == ArchiveAboveSizeDisabled))
                {
                    _archiveAboveSize = value;
                    ResetFileAppenders("ArchiveAboveSize Changed"); // Reset archive file-monitoring
                }
                else
                {
                    _archiveAboveSize = value;
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
            get => _archiveEvery;
            set
            {
                if (_archiveEvery != value)
                {
                    _archiveEvery = value;
                    ResetFileAppenders("ArchiveEvery Changed"); // Reset archive file-monitoring
                }
            }
        }

        /// <summary>
        /// Is the  <see cref="ArchiveFileName"/> an absolute or relative path?
        /// </summary>
        /// <docgen category='Archival Options' order='10' />
        public FilePathKind ArchiveFileKind
        {
            get => _archiveFileKind;
            set
            {
                if (_archiveFileKind != value)
                {
                    _archiveFileKind = value;
                    _fullArchiveFileName = CreateFileNameLayout(ArchiveFileName);
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
                if (_fullArchiveFileName == null) return null;

                return _fullArchiveFileName.GetLayout();
            }
            set
            {
                _fullArchiveFileName = CreateFileNameLayout(value);
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
            get => _maxArchiveFiles;
            set
            {
                if (_maxArchiveFiles != value)
                {
                    _maxArchiveFiles = value;
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
            get => _archiveNumbering;
            set
            {
                if (_archiveNumbering != value)
                {
                    _archiveNumbering = value;
                    ResetFileAppenders("ArchiveNumbering Changed"); // Reset archive file-monitoring
                }
            }
        }
        private ArchiveNumberingMode _archiveNumbering;

        /// <summary>
        /// Used to compress log files during archiving.
        /// This may be used to provide your own implementation of a zip file compressor,
        /// on platforms other than .Net4.5.
        /// Defaults to ZipArchiveFileCompressor on .Net4.5 and to null otherwise.
        /// </summary>
        /// <docgen category='Output Options' order='10' />
        public static IFileCompressor FileCompressor { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to compress archive files into the zip archive format.
        /// </summary>
        /// <docgen category='Archival Options' order='10' />
        [DefaultValue(false)]
        public bool EnableArchiveFileCompression
        {
            get => _enableArchiveFileCompression && FileCompressor != null;
            set
            {
                if (_enableArchiveFileCompression != value)
                {
                    _enableArchiveFileCompression = value;
                    ResetFileAppenders("EnableArchiveFileCompression Changed"); // Reset archive file-monitoring
                }
            }
        }

        /// <summary>
        /// Gets or set a value indicating whether a managed file stream is forced, instead of using the native implementation.
        /// </summary>
        /// <docgen category='Output Options' order='10' />
        [DefaultValue(false)]
        public bool ForceManaged { get; set; }

#if SupportsMutex
        /// <summary>
        /// Gets or sets a value indicationg whether file creation calls should be synchronized by a system global mutex.
        /// </summary>
        /// <docgen category='Output Options' order='10' />
        [DefaultValue(false)]
        public bool ForceMutexConcurrentWrites { get; set; }
#endif

        /// <summary>
        /// Gets or sets a value indicating whether the footer should be written only when the file is archived.
        /// </summary>
        /// <docgen category='Archival Options' order='10' />
        [DefaultValue(false)]
        public bool WriteFooterOnArchivingOnly { get; set; }

        /// <summary>
        /// Gets the characters that are appended after each line.
        /// </summary>
        protected internal string NewLineChars => _lineEndingMode.NewLineCharacters;

        /// <summary>
        /// Refresh the ArchiveFilePatternToWatch option of the <see cref="FileAppenderCache" />. 
        /// The log file must be watched for archiving when multiple processes are writing to the same 
        /// open file.
        /// </summary>
        private void RefreshArchiveFilePatternToWatch(string fileName, LogEventInfo logEvent)
        {
            if (_fileAppenderCache != null)
            {
                _fileAppenderCache.CheckCloseAppenders -= AutoCloseAppendersAfterArchive;

                if (KeepFileOpen)
                    _fileAppenderCache.CheckCloseAppenders += AutoCloseAppendersAfterArchive;

#if !SILVERLIGHT && !__IOS__ && !__ANDROID__ && !NETSTANDARD1_3
                bool mustWatchArchiving = IsArchivingEnabled && ConcurrentWrites && KeepFileOpen;
                if (mustWatchArchiving)
                {
                    string fileNamePattern = GetArchiveFileNamePattern(fileName, logEvent);
                    var fileArchiveStyle = !string.IsNullOrEmpty(fileNamePattern) ? GetFileArchiveHelper(fileNamePattern) : null;
                    string fileNameMask = fileArchiveStyle != null ? _fileArchiveHelper.GenerateFileNameMask(fileNamePattern) : string.Empty;
                    string directoryMask = !string.IsNullOrEmpty(fileNameMask) ? Path.Combine(Path.GetDirectoryName(fileNamePattern), fileNameMask) : string.Empty;
                    _fileAppenderCache.ArchiveFilePatternToWatch = directoryMask;
                }
                else
                {
                    _fileAppenderCache.ArchiveFilePatternToWatch = null;
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
            CleanupInitializedFiles(TimeSource.Current.Time.AddDays(-InitializedFilesCleanupPeriod));
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
            if (InternalLogger.IsTraceEnabled)
            {
                InternalLogger.Trace("FileTarget(Name={0}): Cleanup Initialized Files with cleanupThreshold {1}", Name, cleanupThreshold);
            }

            List<string> filesToFinalize = null;

            // Select the files require to be finalized.
            foreach (var file in _initializedFiles)
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
                    FinalizeFile(fileName);
                }
            }

            InternalLogger.Trace("FileTarget(Name={0}): CleanupInitializedFiles Done", Name);
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
                InternalLogger.Trace("FileTarget(Name={0}): FlushAsync", Name);
                _fileAppenderCache.FlushAppenders();
                asyncContinuation(null);
                InternalLogger.Trace("FileTarget(Name={0}): FlushAsync Done", Name);
            }
            catch (Exception exception)
            {
                InternalLogger.Warn(exception, "FileTarget(Name={0}): Exception in FlushAsync", Name);
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
            if (DiscardAll)
            {
                return NullAppender.TheFactory;
            }
            else if (!KeepFileOpen)
            {
                return RetryingMultiProcessFileAppender.TheFactory;
            }
            else if (NetworkWrites)
            {
                return RetryingMultiProcessFileAppender.TheFactory;
            }
            else if (ConcurrentWrites)
            {
#if SupportsMutex
                if (!ForceMutexConcurrentWrites)
                {
#if MONO
                    if (PlatformDetector.IsUnix)
                    {
                        return UnixMultiProcessFileAppender.TheFactory;
                    }
#elif !NETSTANDARD
                    if (PlatformDetector.IsDesktopWin32 && !PlatformDetector.IsMono)
                    {
                        return WindowsMultiProcessFileAppender.TheFactory;
                    }
#endif
                }

                if (PlatformDetector.SupportsSharableMutex)
                {
                    return MutexMultiProcessFileAppender.TheFactory;
                }
                else
#endif  // SupportsMutex
                {
                    return RetryingMultiProcessFileAppender.TheFactory;
                }
            }
            else if (IsArchivingEnabled)
                return CountingSingleProcessFileAppender.TheFactory;
            else
                return SingleProcessFileAppender.TheFactory;
        }

        private bool IsArchivingEnabled => ArchiveAboveSize != ArchiveAboveSizeDisabled || ArchiveEvery != FileArchivePeriod.None;

        /// <summary>
        /// Initializes file logging by creating data structures that
        /// enable efficient multi-file logging.
        /// </summary>
        protected override void InitializeTarget()
        {
            base.InitializeTarget();

            var appenderFactory = GetFileAppenderFactory();
            if (InternalLogger.IsTraceEnabled)
            {
                InternalLogger.Trace("FileTarget(Name={0}): Using appenderFactory: {1}", Name, appenderFactory.GetType());
            }

            _fileAppenderCache = new FileAppenderCache(OpenFileCacheSize, appenderFactory, this);

            if ((OpenFileCacheSize > 0 || EnableFileDelete) && (OpenFileCacheTimeout > 0 || OpenFileFlushTimeout > 0))
            {
                int openFileAutoTimeoutSecs = (OpenFileCacheTimeout > 0 && OpenFileFlushTimeout > 0) ? Math.Min(OpenFileCacheTimeout, OpenFileFlushTimeout) : Math.Max(OpenFileCacheTimeout, OpenFileFlushTimeout);
                InternalLogger.Trace("FileTarget(Name={0}): Start autoClosingTimer", Name);
                _autoClosingTimer = new Timer(
                    (state) => AutoClosingTimerCallback(this, EventArgs.Empty),
                    null,
                    openFileAutoTimeoutSecs * 1000,
                    openFileAutoTimeoutSecs * 1000);
            }
        }

        /// <summary>
        /// Closes the file(s) opened for writing.
        /// </summary>
        protected override void CloseTarget()
        {
            base.CloseTarget();

            foreach (string fileName in new List<string>(_initializedFiles.Keys))
            {
                FinalizeFile(fileName);
            }

            _fileArchiveHelper = null;

            var currentTimer = _autoClosingTimer;
            if (currentTimer != null)
            {
                InternalLogger.Trace("FileTarget(Name={0}): Stop autoClosingTimer", Name);
                _autoClosingTimer = null;
                currentTimer.WaitForDispose(TimeSpan.Zero);
            }

            _fileAppenderCache.CloseAppenders("Dispose");
            _fileAppenderCache.Dispose();
        }

        private void ResetFileAppenders(string reason)
        {
            _fileArchiveHelper = null;
            if (IsInitialized)
            {
                _fileAppenderCache.CloseAppenders(reason);
                _initializedFiles.Clear();
            }
        }

        /// <summary>
        /// Can be used if <see cref="Target.OptimizeBufferReuse"/> has been enabled.
        /// </summary>
        private readonly ReusableStreamCreator _reusableFileWriteStream = new ReusableStreamCreator(4096);
        /// <summary>
        /// Can be used if <see cref="Target.OptimizeBufferReuse"/> has been enabled.
        /// </summary>
        private readonly ReusableStreamCreator _reusableAsyncFileWriteStream = new ReusableStreamCreator(4096);
        /// <summary>
        /// Can be used if <see cref="Target.OptimizeBufferReuse"/> has been enabled.
        /// </summary>
        private readonly ReusableBufferCreator _reusableEncodingBuffer = new ReusableBufferCreator(1024);

        /// <summary>
        /// Writes the specified logging event to a file specified in the FileName 
        /// parameter.
        /// </summary>
        /// <param name="logEvent">The logging event.</param>
        protected override void Write(LogEventInfo logEvent)
        {
            var logFileName = GetFullFileName(logEvent);
            if (string.IsNullOrEmpty(logFileName))
            {
                throw new ArgumentException("The path is not of a legal form.");
            }

            if (OptimizeBufferReuse)
            {
                using (var targetStream = _reusableFileWriteStream.Allocate())
                {
                    using (var targetBuilder = ReusableLayoutBuilder.Allocate())
                    using (var targetBuffer = _reusableEncodingBuffer.Allocate())
                    {
                        RenderFormattedMessageToStream(logEvent, targetBuilder.Result, targetBuffer.Result, targetStream.Result);
                    }

                    ProcessLogEvent(logEvent, logFileName, new ArraySegment<byte>(targetStream.Result.GetBuffer(), 0, (int)targetStream.Result.Length));
                }
            }
            else
            {
                byte[] bytes = GetBytesToWrite(logEvent);
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
            if (_fullFileName == null)
            {
                return null;
            }

            if (OptimizeBufferReuse)
            {
                using (var targetBuilder = ReusableLayoutBuilder.Allocate())
                {
                    return _fullFileName.RenderWithBuilder(logEvent, targetBuilder.Result);
                }
            }
            else
            {
                return _fullFileName.Render(logEvent);
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

        SortHelpers.KeySelector<AsyncLogEventInfo, string> _getFullFileNameDelegate;

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
            if (_getFullFileNameDelegate == null)
                _getFullFileNameDelegate = c => GetFullFileName(c.LogEvent);

            var buckets = logEvents.BucketSort(_getFullFileNameDelegate);

            using (var reusableStream = (OptimizeBufferReuse && logEvents.Count <= 1000) ? _reusableAsyncFileWriteStream.Allocate() : _reusableAsyncFileWriteStream.None)
            using (var allocatedStream = reusableStream.Result != null ? null : new MemoryStream())
            {
                var ms = allocatedStream ?? reusableStream.Result;

                foreach (var bucket in buckets)
                {
                    int bucketCount = bucket.Value.Count;

                    string fileName = bucket.Key;
                    if (string.IsNullOrEmpty(fileName))
                    {
                        var emptyPathException = new ArgumentException("The path is not of a legal form.");
                        for (int i = 0; i < bucketCount; ++i)
                        {
                            bucket.Value[i].Continuation(emptyPathException);
                        }
                        continue;
                    }

                    ms.SetLength(0);
                    ms.Position = 0;

                    LogEventInfo firstLogEvent = null;

                    using (var targetBuilder = OptimizeBufferReuse ? ReusableLayoutBuilder.Allocate() : ReusableLayoutBuilder.None)
                    using (var targetBuffer = OptimizeBufferReuse ? _reusableEncodingBuffer.Allocate() : _reusableEncodingBuffer.None)
                    using (var targetStream = OptimizeBufferReuse ? _reusableFileWriteStream.Allocate() : _reusableFileWriteStream.None)
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
                                byte[] bytes = GetBytesToWrite(ev.LogEvent);
                                if (ms.Capacity == 0)
                                {
                                    ms.Capacity = GetMemoryStreamInitialSize(bucket.Value.Count, bytes.Length);
                                }
                                ms.Write(bytes, 0, bytes.Length);
                            }
                        }
                    }

                    Exception lastException;
                    FlushCurrentFileWrites(fileName, firstLogEvent, ms, out lastException);

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
            DateTime previousLogEventTimestamp = InitializeFile(fileName, logEvent);
            bool initializedNewFile = previousLogEventTimestamp == DateTime.MinValue;
            if (initializedNewFile && fileName == _previousLogFileName && _previousLogEventTimestamp.HasValue)
                previousLogEventTimestamp = _previousLogEventTimestamp.Value;

            bool archiveOccurred = TryArchiveFile(fileName, logEvent, bytesToWrite.Count, previousLogEventTimestamp, initializedNewFile);
            if (archiveOccurred)
                initializedNewFile = InitializeFile(fileName, logEvent) == DateTime.MinValue;

            WriteToFile(fileName, bytesToWrite, initializedNewFile);

            _previousLogFileName = fileName;
            _previousLogEventTimestamp = logEvent.TimeStamp;
        }

        /// <summary>
        /// Formats the log event for write.
        /// </summary>
        /// <param name="logEvent">The log event to be formatted.</param>
        /// <returns>A string representation of the log event.</returns>
        protected virtual string GetFormattedMessage(LogEventInfo logEvent)
        {
            return Layout.Render(logEvent);
        }

        /// <summary>
        /// Gets the bytes to be written to the file.
        /// </summary>
        /// <param name="logEvent">Log event.</param>
        /// <returns>Array of bytes that are ready to be written.</returns>
        protected virtual byte[] GetBytesToWrite(LogEventInfo logEvent)
        {
            string text = GetFormattedMessage(logEvent);
            int textBytesCount = Encoding.GetByteCount(text);
            int newLineBytesCount = Encoding.GetByteCount(NewLineChars);
            byte[] bytes = new byte[textBytesCount + newLineBytesCount];
            Encoding.GetBytes(text, 0, text.Length, bytes, 0);
            Encoding.GetBytes(NewLineChars, 0, NewLineChars.Length, bytes, textBytesCount);
            return TransformBytes(bytes);
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
        /// <param name="target"><see cref="StringBuilder"/> for the result.</param>
        protected virtual void RenderFormattedMessage(LogEventInfo logEvent, StringBuilder target)
        {
            Layout.RenderAppendBuilder(logEvent, target);
        }

        private void TransformBuilderToStream(LogEventInfo logEvent, StringBuilder builder, char[] transformBuffer, MemoryStream workStream)
        {
            builder.CopyToStream(workStream, Encoding, transformBuffer);
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
                InternalLogger.Info("FileTarget(Name={0}): Archiving {1} skipped as ArchiveFileName equals FileName", Name, fileName);
            }
            else if (EnableArchiveFileCompression)
            {
                InternalLogger.Info("FileTarget(Name={0}): Archiving {1} to compressed {2}", Name, fileName, archiveFileName);
                FileCompressor.CompressFile(fileName, archiveFileName);
                DeleteAndWaitForFileDelete(fileName);
            }
            else
            {
                InternalLogger.Info("FileTarget(Name={0}): Archiving {1} to {2}", Name, fileName, archiveFileName);
                if (File.Exists(archiveFileName))
                {
                    ArchiveFileAppendExisting(fileName, archiveFileName);
                }
                else
                {
                    ArchiveFileMove(fileName, archiveFileName);
                }
            }
        }

        private void ArchiveFileAppendExisting(string fileName, string archiveFileName)
        {
            //todo handle double footer
            InternalLogger.Info("FileTarget(Name={0}): Already exists, append to {1}", Name, archiveFileName);

             //copy to archive file.
            var fileShare = FileShare.ReadWrite;
            if (EnableFileDelete)
            {
                fileShare |= FileShare.Delete;
            }

            using (FileStream fileStream = File.Open(fileName, FileMode.Open, FileAccess.ReadWrite, fileShare))
            using (FileStream archiveFileStream = File.Open(archiveFileName, FileMode.Append))
            {
                fileStream.CopyAndSkipBom(archiveFileStream, Encoding);
                //clear old content
                fileStream.SetLength(0);

                if (EnableFileDelete)
                {
                    // Attempt to delete file to reset File-Creation-Time (Delete under file-lock)
                    if (!DeleteOldArchiveFile(fileName))
                    {
                        fileShare &= ~FileShare.Delete;  // Retry after having released file-lock
                    }
                }

                fileStream.Close(); // This flushes the content, too.
#if NET3_5
                archiveFileStream.Flush();
#else
                archiveFileStream.Flush(true);
#endif
            }

            if ((fileShare & FileShare.Delete) == FileShare.None)
            {
                DeleteOldArchiveFile(fileName); // Attempt to delete file to reset File-Creation-Time
            }
        }

        private void ArchiveFileMove(string fileName, string archiveFileName)
        {
            try
            {
                InternalLogger.Debug("FileTarget(Name={0}): Move file from '{1}' to '{2}'", Name, fileName, archiveFileName);
                File.Move(fileName, archiveFileName);
            }
            catch (IOException ex)
            {
                if (KeepFileOpen && !ConcurrentWrites)
                    throw;  // No need to retry, when only single process access

                if (!EnableFileDelete && KeepFileOpen)
                    throw;  // No need to retry when file delete has been disabled

                if (!PlatformDetector.SupportsSharableMutex)
                    throw;  // No need to retry when not having a real archive mutex to protect us

                // It is possible to move a file while other processes has open file-handles.
                // Unless the other process is actively writing, then the file move might fail.
                // We are already holding the archive-mutex, so lets retry if things are stable
                InternalLogger.Warn(ex, "FileTarget(Name={0}): Archiving failed. Checking for retry move of {1} to {2}.", Name, fileName, archiveFileName);
                if (!File.Exists(fileName) || File.Exists(archiveFileName))
                    throw;

                AsyncHelpers.WaitForDelay(TimeSpan.FromMilliseconds(50));

                if (!File.Exists(fileName) || File.Exists(archiveFileName))
                    throw;

                InternalLogger.Debug("FileTarget(Name={0}): Archiving retrying move of {1} to {2}.", Name, fileName, archiveFileName);
                File.Move(fileName, archiveFileName);
            }
        }

        private bool DeleteOldArchiveFile(string fileName)
        {
            try
            {
                InternalLogger.Info("FileTarget(Name={0}): Deleting old archive file: '{1}'.", Name, fileName);
                File.Delete(fileName);
                return true;
            }
            catch (DirectoryNotFoundException exception)
            {
                //never rethrow this, as this isn't an exceptional case.
                InternalLogger.Debug(exception, "FileTarget(Name={0}): Failed to delete old log file '{1}' as directory is missing.", Name, fileName);
                return false;
            }
            catch (Exception exception)
            {
                InternalLogger.Warn(exception, "FileTarget(Name={0}): Failed to delete old archive file: '{1}'.", Name, fileName);
                if (exception.MustBeRethrown())
                {
                    throw;
                }

                return false;
            }
        }

        private void DeleteAndWaitForFileDelete(string fileName)
        {
            try
            {
                InternalLogger.Trace("FileTarget(Name={0}): Waiting for file delete of '{1}' for 12 sec", Name, fileName);
                var originalFileCreationTime = (new FileInfo(fileName)).CreationTime;
                if (DeleteOldArchiveFile(fileName) && File.Exists(fileName))
                {
                    FileInfo currentFileInfo;
                    for (int i = 0; i < 120; ++i)
                    {
                        AsyncHelpers.WaitForDelay(TimeSpan.FromMilliseconds(100));
                        currentFileInfo = new FileInfo(fileName);
                        if (!currentFileInfo.Exists || currentFileInfo.CreationTime != originalFileCreationTime)
                            return;
                    }

                    InternalLogger.Warn("FileTarget(Name={0}): Timeout while deleting old archive file: '{1}'.", Name, fileName);
                }
            }
            catch (Exception exception)
            {
                InternalLogger.Warn(exception, "FileTarget(Name={0}): Failed to delete old archive file: '{1}'.", Name, fileName);
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
                switch (ArchiveEvery)
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

        private DateTime? GetArchiveDate(string fileName, LogEventInfo logEvent, DateTime previousLogEventTimestamp)
        {
            // Using File LastModifed to handle FileArchivePeriod.Month (where file creation time is one month ago)
            var fileLastModifiedUtc = _fileAppenderCache.GetFileLastWriteTimeUtc(fileName);

            InternalLogger.Trace("FileTarget(Name={0}): Calculating archive date. File-LastModifiedUtc: {1}; Previous LogEvent-TimeStamp: {2}", Name, fileLastModifiedUtc, previousLogEventTimestamp);
            if (!fileLastModifiedUtc.HasValue)
            {
                if (previousLogEventTimestamp == DateTime.MinValue)
                {
                    InternalLogger.Info("FileTarget(Name={0}): Unable to acquire useful timestamp to archive file: {1}", Name, fileName);
                    return null;
                }
                return previousLogEventTimestamp;
            }

            var lastWriteTimeSource = Time.TimeSource.Current.FromSystemTime(fileLastModifiedUtc.Value);
            if (previousLogEventTimestamp != DateTime.MinValue)
            {
                if (previousLogEventTimestamp > lastWriteTimeSource)
                {
                    InternalLogger.Trace("FileTarget(Name={0}): Using previous LogEvent-TimeStamp {1}, because more recent than File-LastModified {2}", Name, previousLogEventTimestamp, lastWriteTimeSource);
                    return previousLogEventTimestamp;
                }

                if (PreviousLogOverlappedPeriod(logEvent, previousLogEventTimestamp, lastWriteTimeSource))
                {
                    InternalLogger.Trace("FileTarget(Name={0}): Using previous LogEvent-TimeStamp {1}, because archive period is overlapping with File-LastModified {2}", Name, previousLogEventTimestamp, lastWriteTimeSource);
                    return previousLogEventTimestamp;
                }

                if (!AutoFlush && KeepFileOpen && !ConcurrentWrites && !NetworkWrites && previousLogEventTimestamp < lastWriteTimeSource)
                {
                    InternalLogger.Trace("FileTarget(Name={0}): Using previous LogEvent-TimeStamp {1}, because AutoFlush=false affects File-LastModified {2}", Name, previousLogEventTimestamp, lastWriteTimeSource);
                    return previousLogEventTimestamp;
                }
            }

            InternalLogger.Trace("FileTarget(Name={0}): Using last write time: {1}", Name, lastWriteTimeSource);
            return lastWriteTimeSource;
        }

        private bool PreviousLogOverlappedPeriod(LogEventInfo logEvent, DateTime previousLogEventTimestamp, DateTime lastFileWrite)
        {
            DateTime timestamp = previousLogEventTimestamp;

            string formatString = GetArchiveDateFormatString(string.Empty);
            string lastWriteTimeString = lastFileWrite.ToString(formatString, CultureInfo.InvariantCulture);
            string logEventTimeString = logEvent.TimeStamp.ToString(formatString, CultureInfo.InvariantCulture);

            if (lastWriteTimeString != logEventTimeString)
                return false;

            DateTime periodAfterPreviousLogEventTime;
            switch (ArchiveEvery)
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
            // Shamelessly taken from https://stackoverflow.com/a/7611480/1354930
            int start = (int)previousLogEventTimestamp.DayOfWeek;
            int target = (int)dayOfWeek;
            if (target <= start)
                target += 7;
            return previousLogEventTimestamp.AddDays(target - start);
        }

        /// <summary>
        /// Invokes the archiving process after determining when and which type of archiving is required.
        /// </summary>
        /// <param name="fileName">File name to be checked and archived.</param>
        /// <param name="eventInfo">Log event that the <see cref="FileTarget"/> instance is currently processing.</param>
        /// <param name="previousLogEventTimestamp">The DateTime of the previous log event for this file.</param>
        /// <param name="initializedNewFile">File has just been opened.</param>
        private void DoAutoArchive(string fileName, LogEventInfo eventInfo, DateTime previousLogEventTimestamp, bool initializedNewFile)
        {
            InternalLogger.Debug("FileTarget(Name={0}): Do archive file: '{1}'", Name, fileName);
            var fileInfo = new FileInfo(fileName);
            if (!fileInfo.Exists)
            {
                // Close possible stale file handles
                _fileAppenderCache.InvalidateAppender(fileName)?.Dispose();
                return;
            }

            string archiveFilePattern = GetArchiveFileNamePattern(fileName, eventInfo);

            if (string.IsNullOrEmpty(archiveFilePattern))
            {
                InternalLogger.Warn("FileTarget(Name={0}): Skip auto archive because archiveFilePattern is NULL", Name);
                return;
            }
            InternalLogger.Trace("FileTarget(Name={0}): Archive pattern '{1}'", Name, archiveFilePattern);

            var fileArchiveStyle = GetFileArchiveHelper(archiveFilePattern);
            var existingArchiveFiles = fileArchiveStyle.GetExistingArchiveFiles(archiveFilePattern);

            if (MaxArchiveFiles == 1)
            {
                InternalLogger.Trace("FileTarget(Name={0}): MaxArchiveFiles = 1", Name);
                // Perform archive cleanup before generating the next filename,
                // as next archive-filename can be affected by existing files.
                for (int i = existingArchiveFiles.Count - 1; i >= 0; i--)
                {
                    var oldArchiveFile = existingArchiveFiles[i];
                    if (!string.Equals(oldArchiveFile.FileName, fileInfo.FullName, StringComparison.OrdinalIgnoreCase))
                    {
                        DeleteOldArchiveFile(oldArchiveFile.FileName);
                        existingArchiveFiles.RemoveAt(i);
                    }
                }

                if (initializedNewFile)
                {
                    if (string.Equals(Path.GetDirectoryName(archiveFilePattern), fileInfo.DirectoryName, StringComparison.OrdinalIgnoreCase))
                    {
                        DeleteOldArchiveFile(fileName);
                        return;
                    }
                }
            }

            DateTime? archiveDate = GetArchiveDate(fileName, eventInfo, previousLogEventTimestamp);
            var archiveFileName = archiveDate.HasValue ? fileArchiveStyle.GenerateArchiveFileName(archiveFilePattern, archiveDate.Value, existingArchiveFiles) : null;
            if (archiveFileName != null)
            {
                if (!initializedNewFile)
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

                var cleanupArchiveFiles = fileArchiveStyle.CheckArchiveCleanup(archiveFilePattern, existingArchiveFiles, MaxArchiveFiles);
                foreach (var oldArchiveFile in cleanupArchiveFiles)
                {
                    DeleteOldArchiveFile(oldArchiveFile.FileName);
                }

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
            if (_fullArchiveFileName == null)
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
                string archiveFileName = _fullArchiveFileName.Render(eventInfo);
                return archiveFileName;
            }
        }

        /// <summary>
        /// Archives the file if it should be archived.
        /// </summary>
        /// <param name="fileName">The file name to check for.</param>
        /// <param name="ev">Log event that the <see cref="FileTarget"/> instance is currently processing.</param>
        /// <param name="upcomingWriteSize">The size in bytes of the next chunk of data to be written in the file.</param>
        /// <param name="previousLogEventTimestamp">The DateTime of the previous log event for this file.</param>
        /// <param name="initializedNewFile">File has just been opened.</param>
        /// <returns>True when archive operation of the file was completed (by this target or a concurrent target)</returns>
        private bool TryArchiveFile(string fileName, LogEventInfo ev, int upcomingWriteSize, DateTime previousLogEventTimestamp, bool initializedNewFile)
        {
            if (!IsArchivingEnabled)
                return false;

            string archiveFile = string.Empty;

            BaseFileAppender archivedAppender = null;

            try
            {
                archiveFile = GetArchiveFileName(fileName, ev, upcomingWriteSize, previousLogEventTimestamp);
                if (!string.IsNullOrEmpty(archiveFile))
                {
                    InternalLogger.Trace("FileTarget(Name={0}): Archive attempt for file '{1}'", Name, archiveFile);
                    archivedAppender = _fileAppenderCache.InvalidateAppender(fileName);
                    if (fileName != archiveFile)
                    {
                        var fileAppender = _fileAppenderCache.InvalidateAppender(archiveFile);
                        archivedAppender = archivedAppender ?? fileAppender;
                    }

                    if (!string.IsNullOrEmpty(_previousLogFileName) && _previousLogFileName != archiveFile && _previousLogFileName != fileName)
                    {
                        var fileAppender = _fileAppenderCache.InvalidateAppender(_previousLogFileName);
                        archivedAppender = archivedAppender ?? fileAppender;
                    }

#if !SILVERLIGHT && !__IOS__ && !__ANDROID__ && !NETSTANDARD1_3
                    // Closes all file handles if any archive operation has been detected by file-watcher
                    _fileAppenderCache.InvalidateAppendersForArchivedFiles();
#endif
                }
                else
                {
#if !SILVERLIGHT && !__IOS__ && !__ANDROID__ && !NETSTANDARD1_3
                    _fileAppenderCache.InvalidateAppendersForArchivedFiles();
#endif
                }
            }
            catch (Exception exception)
            {
                InternalLogger.Warn(exception, "FileTarget(Name={0}): Failed to check archive for file '{1}'.", Name, fileName);
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
                        if (archivedAppender is BaseMutexFileAppender mutexFileAppender && mutexFileAppender.ArchiveMutex != null)
                        {
                            mutexFileAppender.ArchiveMutex.WaitOne();
                        }
                        else
                        {
                            InternalLogger.Info("FileTarget(Name={0}): Archive mutex not available: {1}", Name, archiveFile);
                        }

                    }
                    catch (AbandonedMutexException)
                    {
                        // ignore the exception, another process was killed without properly releasing the mutex
                        // the mutex has been acquired, so proceed to writing
                        // See: https://msdn.microsoft.com/en-us/library/system.threading.abandonedmutexexception.aspx
                    }
#endif

                    // Check again if archive is needed. We could have been raced by another process
                    var validatedArchiveFile = GetArchiveFileName(fileName, ev, upcomingWriteSize, previousLogEventTimestamp);
                    if (string.IsNullOrEmpty(validatedArchiveFile))
                    {
                        InternalLogger.Trace("FileTarget(Name={0}): Archive already performed for file '{1}'", Name, archiveFile);
                        if (archiveFile != fileName)
                            _initializedFiles.Remove(fileName);
                        _initializedFiles.Remove(archiveFile);
                    }
                    else
                    {
                        archiveFile = validatedArchiveFile;
                        DoAutoArchive(archiveFile, ev, previousLogEventTimestamp, initializedNewFile);
                        _initializedFiles.Remove(archiveFile);
                    }

                    if (_previousLogFileName == archiveFile)
                    {
                        _previousLogFileName = null;
                        _previousLogEventTimestamp = null;
                    }
                    return true;
                }
                catch (Exception exception)
                {
                    InternalLogger.Warn(exception, "FileTarget(Name={0}): Failed to archive file '{1}'.", Name, archiveFile);
                    if (exception.MustBeRethrown())
                    {
                        throw;
                    }
                }
                finally
                {
#if SupportsMutex
                    if (archivedAppender is BaseMutexFileAppender mutexFileAppender)
                        mutexFileAppender.ArchiveMutex?.ReleaseMutex();
#endif
                    archivedAppender?.Dispose();    // Dispose of Archive Mutex
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
        /// <param name="previousLogEventTimestamp">The DateTime of the previous log event for this file.</param>
        /// <returns>Filename to archive. If <c>null</c>, then nothing to archive.</returns>
        private string GetArchiveFileName(string fileName, LogEventInfo ev, int upcomingWriteSize, DateTime previousLogEventTimestamp)
        {
            var hasFileName = !(fileName == null && _previousLogFileName == null);
            if (hasFileName)
            {
                return GetArchiveFileNameBasedOnFileSize(fileName, upcomingWriteSize) ??
                       GetArchiveFileNameBasedOnTime(fileName, ev, previousLogEventTimestamp);
            }

            return null;
        }

        /// <summary>
        /// Returns the correct filename to archive
        /// </summary>
        /// <returns></returns>
        private string GetPotentialFileForArchiving(string fileName)
        {
            if (string.Equals(fileName, _previousLogFileName, StringComparison.OrdinalIgnoreCase))
            {
                //both the same, so don't care
                return fileName;
            }

            if (string.IsNullOrEmpty(_previousLogFileName))
            {
                return fileName;
            }

            if (string.IsNullOrEmpty(fileName))
            {
                return _previousLogFileName;
            }

            //this is an expensive call
            var fileLength = _fileAppenderCache.GetFileLength(fileName);
            string fileToArchive = fileLength != null ? fileName : _previousLogFileName;
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
            if (ArchiveAboveSize == ArchiveAboveSizeDisabled)
            {
                return null;
            }

            var previousFileName = GetPotentialFileForArchiving(fileName);
            if (previousFileName == null)
            {
                return null;
            }

            var length = _fileAppenderCache.GetFileLength(previousFileName);
            if (length == null)
            {
                return null;
            }

            if (previousFileName != fileName)
            {
                upcomingWriteSize = 0;  // Not going to write to this file
            }

            var shouldArchive = length.Value + upcomingWriteSize > ArchiveAboveSize;
            if (shouldArchive)
            {
                return previousFileName;
            }

            return null;
        }

        /// <summary>
        /// Returns the file name for archiving, or null if archiving should not occur based on date/time.
        /// </summary>
        /// <param name="fileName">File name to be written.</param>
        /// <param name="logEvent">Log event that the <see cref="FileTarget"/> instance is currently processing.</param>
        /// <param name="previousLogEventTimestamp">The DateTime of the previous log event for this file.</param>
        /// <returns>Filename to archive. If <c>null</c>, then nothing to archive.</returns>
        private string GetArchiveFileNameBasedOnTime(string fileName, LogEventInfo logEvent, DateTime previousLogEventTimestamp)
        {
            if (ArchiveEvery == FileArchivePeriod.None)
            {
                return null;
            }

            fileName = GetPotentialFileForArchiving(fileName);
            if (fileName == null)
            {
                return null;
            }

            // Linux FileSystems doesn't always have file-birth-time, so NLog tries to provide a little help
            DateTime? fallbackTimeSourceLinux = (previousLogEventTimestamp != DateTime.MinValue && KeepFileOpen && !ConcurrentWrites && !NetworkWrites) ? previousLogEventTimestamp : (DateTime?)null;
            var creationTimeSource = _fileAppenderCache.GetFileCreationTimeSource(fileName, fallbackTimeSourceLinux);
            if (creationTimeSource == null)
            {
                return null;
            }

            if (previousLogEventTimestamp != DateTime.MinValue && previousLogEventTimestamp < creationTimeSource)
            {
                if (TruncateArchiveTime(previousLogEventTimestamp, FileArchivePeriod.Minute) < TruncateArchiveTime(creationTimeSource.Value, FileArchivePeriod.Minute) && PlatformDetector.IsUnix)
                {
                    if (KeepFileOpen && !ConcurrentWrites && !NetworkWrites)
                    {
                        InternalLogger.Debug("FileTarget(Name={0}): Adjusted file creation time from {1} to {2}. Linux FileSystem probably don't support file birthtime.", Name, creationTimeSource, previousLogEventTimestamp);
                        creationTimeSource = previousLogEventTimestamp;
                    }
                    else
                    {
                        InternalLogger.Debug("FileTarget(Name={0}): File creation time {1} newer than previous file write time {2}. Linux FileSystem probably don't support file birthtime, unless multiple applications are writing to the same file. Configure FileTarget.KeepFileOpen=true AND FileTarget.ConcurrentWrites=false, so NLog can fix this.", Name, creationTimeSource, previousLogEventTimestamp);
                    }
                }
            }

            DateTime fileCreateTime = TruncateArchiveTime(creationTimeSource.Value, ArchiveEvery);
            DateTime logEventTime = TruncateArchiveTime(logEvent.TimeStamp, ArchiveEvery);
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

        private void AutoCloseAppendersAfterArchive(object sender, EventArgs state)
        {
            bool lockTaken = Monitor.TryEnter(SyncRoot, TimeSpan.FromSeconds(2));
            if (!lockTaken)
                return; // Archive events triggered by FileWatcher are important, but not life critical

            try
            {
                if (!IsInitialized)
                {
                    return;
                }

                InternalLogger.Trace("FileTarget(Name={0}): Auto Close FileAppenders after archive", Name);
                _fileAppenderCache.CloseAppenders(DateTime.MinValue);
            }
            catch (Exception exception)
            {
                InternalLogger.Warn(exception, "FileTarget(Name={0}): Exception in AutoCloseAppendersAfterArchive", Name);

                if (exception.MustBeRethrownImmediately())
                {
                    throw;  // Throwing exceptions here will crash the entire application (.NET 2.0 behavior)
                }
            }
            finally
            {
                Monitor.Exit(SyncRoot);
            }
        }

        private void AutoClosingTimerCallback(object sender, EventArgs state)
        {
            bool lockTaken = Monitor.TryEnter(SyncRoot, TimeSpan.FromSeconds(0.5));
            if (!lockTaken)
                return; // Timer will trigger again, no need for timers to queue up

            try
            {
                if (!IsInitialized)
                {
                    return;
                }

                if (OpenFileCacheTimeout > 0)
                {
                    DateTime expireTime = DateTime.UtcNow.AddSeconds(-OpenFileCacheTimeout);
                    InternalLogger.Trace("FileTarget(Name={0}): Auto Close FileAppenders", Name);
                    _fileAppenderCache.CloseAppenders(expireTime);
                }

                if (OpenFileFlushTimeout > 0 && !AutoFlush)
                {
                    ConditionalFlushOpenFileAppenders();
                }
            }
            catch (Exception exception)
            {
                InternalLogger.Warn(exception, "FileTarget(Name={0}): Exception in AutoClosingTimerCallback", Name);

                if (exception.MustBeRethrownImmediately())
                {
                    throw;  // Throwing exceptions here will crash the entire application (.NET 2.0 behavior)
                }
            }
            finally
            {
                Monitor.Exit(SyncRoot);
            }
        }

        private void ConditionalFlushOpenFileAppenders()
        {
            DateTime flushTime = Time.TimeSource.Current.Time.AddSeconds(-Math.Max(OpenFileFlushTimeout, 5) * 2);

            bool flushAppenders = false;
            foreach (var file in _initializedFiles)
            {
                if (file.Value > flushTime)
                {
                    flushAppenders = true;
                    break;
                }
            }

            if (flushAppenders)
            {
                // Only request flush of file-handles, when something has been written
                InternalLogger.Trace("FileTarget(Name={0}): Auto Flush FileAppenders", Name);
                _fileAppenderCache.FlushAppenders();
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
            if (ReplaceFileContentsOnEachWrite)
            {
                ReplaceFileContent(fileName, bytes, true);
                return;
            }

            BaseFileAppender appender = _fileAppenderCache.AllocateAppender(fileName);
            try
            {
                if (initializedNewFile)
                {
                    WriteHeaderAndBom(appender);
                }

                appender.Write(bytes.Array, bytes.Offset, bytes.Count);

                if (AutoFlush)
                {
                    appender.Flush();
                }
            }
            catch (Exception ex)
            {
                InternalLogger.Error(ex, "FileTarget(Name={0}): Failed write to file '{1}'.", Name, fileName);
                _fileAppenderCache.InvalidateAppender(fileName)?.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Initialise a file to be used by the <see cref="FileTarget"/> instance. Based on the number of initialised
        /// files and the values of various instance properties clean up and/or archiving processes can be invoked.
        /// </summary>
        /// <param name="fileName">File name to be written.</param>
        /// <param name="logEvent">Log event that the <see cref="FileTarget"/> instance is currently processing.</param>
        /// <returns>The DateTime of the previous log event for this file (DateTime.MinValue if just initialized).</returns>
        private DateTime InitializeFile(string fileName, LogEventInfo logEvent)
        {
            if (_initializedFiles.Count != 0 && _previousLogEventTimestamp.HasValue && _previousLogFileName == fileName)
            {
                if (logEvent.TimeStamp == _previousLogEventTimestamp.Value)
                {
                    return _previousLogEventTimestamp.Value;
                }
            }

            var now = logEvent.TimeStamp;
            DateTime lastTime;
            if (!_initializedFiles.TryGetValue(fileName, out lastTime))
            {
                ProcessOnStartup(fileName, logEvent);

                _initializedFilesCounter++;
                if (_initializedFilesCounter >= InitializedFilesCounterMax)
                {
                    _initializedFilesCounter = 0;
                    CleanupInitializedFiles();
                }

                _initializedFiles[fileName] = now;
                return DateTime.MinValue;
            }
            else if (lastTime != now)
            {
                _initializedFiles[fileName] = now;
            }

            return lastTime;
        }

        /// <summary>
        /// Writes the file footer and finalizes the file in <see cref="FileTarget"/> instance internal structures.
        /// </summary>
        /// <param name="fileName">File name to close.</param>
        /// <param name="isArchiving">Indicates if the file is being finalized for archiving.</param>
        private void FinalizeFile(string fileName, bool isArchiving = false)
        {
            InternalLogger.Trace("FileTarget(Name={0}): FinalizeFile '{1}, isArchiving: {2}'", Name, fileName, isArchiving);
            if ((isArchiving) || (!WriteFooterOnArchivingOnly))
                WriteFooter(fileName);

            _fileAppenderCache.InvalidateAppender(fileName)?.Dispose();
            _initializedFiles.Remove(fileName);
        }

        /// <summary>
        /// Writes the footer information to a file.
        /// </summary>
        /// <param name="fileName">The file path to write to.</param>
        private void WriteFooter(string fileName)
        {
            ArraySegment<byte> footerBytes = GetLayoutBytes(Footer);
            if (footerBytes.Count > 0)
            {
                if (File.Exists(fileName))
                {
                    WriteToFile(fileName, footerBytes, false);
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
            InternalLogger.Debug("FileTarget(Name={0}): Process file '{1}' on startup", Name, fileName);
            RefreshArchiveFilePatternToWatch(fileName, logEvent);

            if (ArchiveOldFileOnStartup)
            {
                try
                {
                    DoAutoArchive(fileName, logEvent, DateTime.MinValue, true);
                }
                catch (Exception exception)
                {
                    InternalLogger.Warn(exception, "FileTarget(Name={0}): Unable to archive old log file '{1}'.", Name, fileName);

                    if (exception.MustBeRethrown())
                    {
                        throw;
                    }
                }
            }

            if (DeleteOldFileOnStartup)
            {
                DeleteOldArchiveFile(fileName);
            }

            string archiveFilePattern = GetArchiveFileNamePattern(fileName, logEvent);
            if (!string.IsNullOrEmpty(archiveFilePattern))
            {
                if (FileArchiveModeFactory.ShouldDeleteOldArchives(MaxArchiveFiles))
                {
                    var fileArchiveStyle = GetFileArchiveHelper(archiveFilePattern);
                    if (fileArchiveStyle.AttemptCleanupOnInitializeFile(archiveFilePattern, MaxArchiveFiles))
                    {
                        var existingArchiveFiles = fileArchiveStyle.GetExistingArchiveFiles(archiveFilePattern);
                        var cleanupArchiveFiles = fileArchiveStyle.CheckArchiveCleanup(archiveFilePattern, existingArchiveFiles, MaxArchiveFiles);
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
                    ArraySegment<byte> headerBytes = GetLayoutBytes(Header);
                    if (headerBytes.Count > 0)
                    {
                        fs.Write(headerBytes.Array, headerBytes.Offset, headerBytes.Count);
                    }

                    fs.Write(bytes.Array, bytes.Offset, bytes.Count);

                    ArraySegment<byte> footerBytes = GetLayoutBytes(Footer);
                    if (footerBytes.Count > 0)
                    {
                        fs.Write(footerBytes.Array, footerBytes.Offset, footerBytes.Count);
                    }
                }
            }
            catch (DirectoryNotFoundException)
            {
                if (!CreateDirs || !firstAttempt)
                {
                    throw;
                }
                Directory.CreateDirectory(Path.GetDirectoryName(fileName));
                //retry.
                ReplaceFileContent(fileName, bytes, false);
            }
        }

        /// <summary>
        /// Writes the header information and byte order mark to a file.
        /// </summary>
        /// <param name="appender">File appender associated with the file.</param>
        private void WriteHeaderAndBom(BaseFileAppender appender)
        {
            //performance: cheap check before checking file info 
            if (Header == null && !WriteBom) return;

            var length = appender.GetFileLength();
            //  Write header and BOM only on empty files or if file info cannot be obtained.
            if (length == null || length == 0)
            {
                if (WriteBom)
                {
                    InternalLogger.Trace("FileTarget(Name={0}): Write byte order mark from encoding={1}", Name, Encoding);
                    var preamble = Encoding.GetPreamble();
                    if (preamble.Length > 0)
                        appender.Write(preamble, 0, preamble.Length);
                }

                if (Header != null)
                {
                    InternalLogger.Trace("FileTarget(Name={0}): Write header", Name);
                    ArraySegment<byte> headerBytes = GetLayoutBytes(Header);
                    if (headerBytes.Count > 0)
                    {
                        appender.Write(headerBytes.Array, headerBytes.Offset, headerBytes.Count);
                    }
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
                using (var targetBuilder = ReusableLayoutBuilder.Allocate())
                using (var targetBuffer = _reusableEncodingBuffer.Allocate())
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
                string renderedText = layout.Render(LogEventInfo.CreateNullEvent()) + NewLineChars;
                return new ArraySegment<byte>(TransformBytes(Encoding.GetBytes(renderedText)));
            }
        }
    }
}