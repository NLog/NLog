// 
// Copyright (c) 2004-2021 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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

#if !NETSTANDARD1_3
#define SupportsMutex
#endif

namespace NLog.Targets
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Globalization;
    using System.IO;
    using System.IO.Compression;
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
    /// <remarks>
    /// <a href="https://github.com/nlog/nlog/wiki/File-target">See NLog Wiki</a>
    /// </remarks>
    /// <seealso href="https://github.com/nlog/nlog/wiki/File-target">Documentation on NLog Wiki</seealso>
    [Target("File")]
    public class FileTarget : TargetWithLayoutHeaderAndFooter, ICreateFileParameters
    {
        /// <summary>
        /// Default clean up period of the initialized files. When a file exceeds the clean up period is removed from the list.
        /// </summary>
        /// <remarks>Clean up period is defined in days.</remarks>
        private const int InitializedFilesCleanupPeriod = 2;

        /// <summary>
        /// This value disables file archiving based on the size. 
        /// </summary>
        private const long ArchiveAboveSizeDisabled = -1L;

        /// <summary>
        /// Holds the initialized files each given time by the <see cref="FileTarget"/> instance. Against each file, the last write time is stored. 
        /// </summary>
        /// <remarks>Last write time is store in local time (no UTC).</remarks>
        private readonly Dictionary<string, DateTime> _initializedFiles = new Dictionary<string, DateTime>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// List of the associated file appenders with the <see cref="FileTarget"/> instance.
        /// </summary>
        private IFileAppenderCache _fileAppenderCache;

        IFileArchiveMode GetFileArchiveHelper(string archiveFilePattern)
        {
            return _fileArchiveHelper ?? (_fileArchiveHelper = FileArchiveModeFactory.CreateArchiveStyle(archiveFilePattern, ArchiveNumbering, GetArchiveDateFormatString(ArchiveDateFormat), ArchiveFileName != null, MaxArchiveFiles > 0 || MaxArchiveDays > 0));
        }
        private IFileArchiveMode _fileArchiveHelper;

        private Timer _autoClosingTimer;

        /// <summary>
        /// The number of initialized files at any one time.
        /// </summary>
        private int _initializedFilesCounter;

        /// <summary>
        /// The maximum number of archive files that should be kept.
        /// </summary>
        private int _maxArchiveFiles;

        /// <summary>
        /// The maximum days of archive files that should be kept.
        /// </summary>
        private int _maxArchiveDays;

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

        private bool _concurrentWrites;
        private bool _cleanupFileName;
        private FilePathKind _fileNameKind = FilePathKind.Unknown;
        private FilePathKind _archiveFileKind;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileTarget" /> class.
        /// </summary>
        /// <remarks>
        /// The default value of the layout is: <code>${longdate}|${level:uppercase=true}|${logger}|${message:withexception=true}</code>
        /// </remarks>
        public FileTarget() : this(FileAppenderCache.Empty)
        {
        }

        internal FileTarget(IFileAppenderCache fileAppenderCache)
        {
            ArchiveNumbering = ArchiveNumberingMode.Sequence;
            _maxArchiveFiles = 0;
            _maxArchiveDays = 0;

            ArchiveEvery = FileArchivePeriod.None;
            ArchiveAboveSize = ArchiveAboveSizeDisabled;
            _cleanupFileName = true;

            _fileAppenderCache = fileAppenderCache;
        }

#if !NET35 && !NET40
        static FileTarget()
        {
            FileCompressor = new ZipArchiveFileCompressor();
        }
#endif

        /// <summary>
        /// Initializes a new instance of the <see cref="FileTarget" /> class.
        /// </summary>
        /// <remarks>
        /// The default value of the layout is: <code>${longdate}|${level:uppercase=true}|${logger}|${message:withexception=true}</code>
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
        /// <docgen category='General Options' order='2' />
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
            if (value is null)
                return null;

            return new FilePathLayout(value, CleanupFileName, FileNameKind);
        }

        /// <summary>
        /// Cleanup invalid values in a filename, e.g. slashes in a filename. If set to <c>true</c>, this can impact the performance of massive writes. 
        /// If set to <c>false</c>, nothing gets written when the filename is wrong.
        /// </summary>
        /// <docgen category='Output Options' order='100' />
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
        /// <docgen category='Output Options' order='100' />
        public FilePathKind FileNameKind
        {
            get => _fileNameKind;
            set
            {
                if (_fileNameKind != value)
                {
                    _fileNameKind = value;
                    _fullFileName = CreateFileNameLayout(FileName);
                    _fullArchiveFileName = CreateFileNameLayout(ArchiveFileName);
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
        /// <docgen category='Output Options' order='50' />
        public bool CreateDirs { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether to delete old log file on startup.
        /// </summary>
        /// <remarks>
        /// This option works only when the "FileName" parameter denotes a single file.
        /// </remarks>
        /// <docgen category='Output Options' order='50' />
        public bool DeleteOldFileOnStartup { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to replace file contents on each write instead of appending log message at the end.
        /// </summary>
        /// <docgen category='Output Options' order='100' />
        public bool ReplaceFileContentsOnEachWrite { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to keep log file open instead of opening and closing it on each logging event.
        /// </summary>
        /// <remarks>
        /// KeepFileOpen = true gives the best performance, and ensure the file-lock is not lost to other applications.<br/>
        /// KeepFileOpen = false gives the best compability, but slow performance and lead to file-locking issues with other applications.
        /// </remarks>
        /// <docgen category='Performance Tuning Options' order='10' />
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
        private bool _keepFileOpen = true;

        /// <summary>
        /// Gets or sets a value indicating whether to enable log file(s) to be deleted.
        /// </summary>
        /// <docgen category='Output Options' order='50' />
        public bool EnableFileDelete { get; set; } = true;

        /// <summary>
        /// Gets or sets the file attributes (Windows only).
        /// </summary>
        /// <docgen category='Output Options' order='100' />
        public Win32FileAttributes FileAttributes
        {
            get => _fileAttributes;
            set
            {
                if (value != Win32FileAttributes.Normal && PlatformDetector.IsWin32)
                {
                    ForceManaged = false;
                }
                _fileAttributes = value;
            }
        }
        Win32FileAttributes _fileAttributes = Win32FileAttributes.Normal;

        bool ICreateFileParameters.IsArchivingEnabled => IsArchivingEnabled;

        int ICreateFileParameters.FileOpenRetryCount => ConcurrentWrites ? ConcurrentWriteAttempts : (KeepFileOpen ? 0 : (_concurrentWriteAttempts ?? 2));

        int ICreateFileParameters.FileOpenRetryDelay => ConcurrentWriteAttemptDelay;

        /// <summary>
        /// Gets or sets the line ending mode.
        /// </summary>
        /// <docgen category='Output Options' order='100' />
        public LineEndingMode LineEnding { get; set; } = LineEndingMode.Default;

        /// <summary>
        /// Gets or sets a value indicating whether to automatically flush the file buffers after each log message.
        /// </summary>
        /// <docgen category='Performance Tuning Options' order='50' />
        public bool AutoFlush { get; set; } = true;

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
        public int OpenFileCacheSize { get; set; } = 5;

        /// <summary>
        /// Gets or sets the maximum number of seconds that files are kept open. Zero or negative means disabled.
        /// </summary>
        /// <docgen category='Performance Tuning Options' order='50' />
        public int OpenFileCacheTimeout { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of seconds before open files are flushed. Zero or negative means disabled.
        /// </summary>
        /// <docgen category='Performance Tuning Options' order='50' />
        public int OpenFileFlushTimeout { get; set; }

        /// <summary>
        /// Gets or sets the log file buffer size in bytes.
        /// </summary>
        /// <docgen category='Performance Tuning Options' order='50' />
        public int BufferSize { get; set; } = 32768;

        /// <summary>
        /// Gets or sets the file encoding.
        /// </summary>
        /// <docgen category='Output Options' order='10' />
        public Encoding Encoding
        {
            get => _encoding;
            set
            {
                _encoding = value;
                if (!_writeBom.HasValue && InitialValueBom(value))
                    _writeBom = true;
            }
        }
        private Encoding _encoding = Encoding.UTF8;

        /// <summary>
        /// Gets or sets whether or not this target should just discard all data that its asked to write.
        /// Mostly used for when testing NLog Stack except final write
        /// </summary>
        /// <docgen category='Performance Tuning Options' order='100' />
        public bool DiscardAll { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether concurrent writes to the log file by multiple processes on the same host.
        /// </summary>
        /// <remarks>
        /// This makes multi-process logging possible. NLog uses a special technique
        /// that lets it keep the files open for writing.
        /// </remarks>
        /// <docgen category='Performance Tuning Options' order='10' />
        public bool ConcurrentWrites
        {
            get => _concurrentWrites;
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
        /// Obsolete and replaced by <see cref="KeepFileOpen"/> = <c>false</c> with NLog v5.3.
        /// Gets or sets a value indicating whether concurrent writes to the log file by multiple processes on different network hosts.
        /// </summary>
        /// <remarks>
        /// This effectively prevents files from being kept open.
        /// </remarks>
        /// <docgen category='Performance Tuning Options' order='50' />
        [Obsolete("Instead use KeepFileOpen = false. Marked obsolete with NLog v5.3")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool NetworkWrites { get => !KeepFileOpen; set => KeepFileOpen = !value; }

        /// <summary>
        /// Gets or sets a value indicating whether to write BOM (byte order mark) in created files.
        ///
        /// Defaults to true for UTF-16 and UTF-32
        /// </summary>
        /// <docgen category='Output Options' order='50' />
        public bool WriteBom
        {
            get => _writeBom ?? false;
            set => _writeBom = value;
        }
        private bool? _writeBom;

        /// <summary>
        /// Gets or sets the number of times the write is appended on the file before NLog
        /// discards the log message.
        /// </summary>
        /// <docgen category='Performance Tuning Options' order='100' />
        public int ConcurrentWriteAttempts { get => _concurrentWriteAttempts ?? 10; set => _concurrentWriteAttempts = value; }
        private int? _concurrentWriteAttempts;

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
        /// <docgen category='Performance Tuning Options' order='100' />
        public int ConcurrentWriteAttemptDelay { get; set; } = 1;

        /// <summary>
        /// Gets or sets a value indicating whether to archive old log file on startup.
        /// </summary>
        /// <remarks>
        /// This option works only when the "FileName" parameter denotes a single file.
        /// After archiving the old file, the current log file will be empty.
        /// </remarks>
        /// <docgen category='Archival Options' order='50' />
        public bool ArchiveOldFileOnStartup
        {
            get => _archiveOldFileOnStartup ?? false;
            set => _archiveOldFileOnStartup = value;
        }
        private bool? _archiveOldFileOnStartup;

        /// <summary>
        /// Gets or sets whether to write the Header on initial creation of file appender, even if the file is not empty.
        /// Default value is <see langword="false"/>, which means only write header when initial file is empty (Ex. ensures valid CSV files)
        /// </summary>
        /// <remarks>
        /// Alternative use <see cref="ArchiveOldFileOnStartup"/> to ensure each application session gets individual log-file.
        /// </remarks>
        public bool WriteHeaderWhenInitialFileNotEmpty { get; set; }

        /// <summary>
        /// Gets or sets a value of the file size threshold to archive old log file on startup.
        /// </summary>
        /// <remarks>
        /// This option won't work if <see cref="ArchiveOldFileOnStartup"/> is set to <c>false</c>
        /// Default value is 0 which means that the file is archived as soon as archival on
        /// startup is enabled.
        /// </remarks>
        /// <docgen category='Archival Options' order='50' />
        public long ArchiveOldFileOnStartupAboveSize { get; set; }

        /// <summary>
        /// Gets or sets a value specifying the date format to use when archiving files.
        /// </summary>
        /// <remarks>
        /// This option works only when the "ArchiveNumbering" parameter is set either to Date or DateAndSequence.
        /// </remarks>
        /// <docgen category='Archival Options' order='50' />
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
        private string _archiveDateFormat = string.Empty;

        /// <summary>
        /// Gets or sets the size in bytes above which log files will be automatically archived.
        /// </summary>
        /// <remarks>
        /// Notice when combined with <see cref="ArchiveNumberingMode.Date"/> then it will attempt to append to any existing
        /// archive file if grown above size multiple times. New archive file will be created when using <see cref="ArchiveNumberingMode.DateAndSequence"/>
        /// </remarks>
        /// <docgen category='Archival Options' order='50' />
        public long ArchiveAboveSize
        {
            get => _archiveAboveSize;
            set
            {
                var newValue = value > 0 ? value : ArchiveAboveSizeDisabled;
                if ((_archiveAboveSize > 0) != (newValue > 0))
                {
                    _archiveAboveSize = newValue;
                    ResetFileAppenders("ArchiveAboveSize Changed"); // Reset archive file-monitoring
                }
                else
                {
                    _archiveAboveSize = newValue;
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
        /// </remarks>
        /// <docgen category='Archival Options' order='50' />
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
        /// <docgen category='Archival Options' order='50' />
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
        /// <docgen category='Archival Options' order='50' />
        public Layout ArchiveFileName
        {
            get
            {
                return _fullArchiveFileName?.GetLayout();
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
        /// <docgen category='Archival Options' order='50' />
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
        /// Gets or sets the maximum days of archive files that should be kept.
        /// </summary>
        /// <docgen category='Archival Options' order='50' />
        public int MaxArchiveDays
        {
            get => _maxArchiveDays;
            set
            {
                if (_maxArchiveDays != value)
                {
                    _maxArchiveDays = value;
                    ResetFileAppenders("MaxArchiveDays Changed");  // Enforce archive cleanup
                }
            }
        }

        /// <summary>
        /// Gets or sets the way file archives are numbered. 
        /// </summary>
        /// <docgen category='Archival Options' order='50' />
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
        /// <docgen category='Archival Options' order='50' />
        public static IFileCompressor FileCompressor { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to compress archive files into the zip archive format.
        /// </summary>
        /// <docgen category='Archival Options' order='50' />
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
        /// <docgen category='Output Options' order='100' />
        public bool ForceManaged { get; set; } = true;

#if SupportsMutex
        /// <summary>
        /// Gets or sets a value indicating whether file creation calls should be synchronized by a system global mutex.
        /// </summary>
        /// <docgen category='Output Options' order='100' />
        public bool ForceMutexConcurrentWrites { get; set; }
#endif

        /// <summary>
        /// Gets or sets a value indicating whether the footer should be written only when the file is archived.
        /// </summary>
        /// <docgen category='Archival Options' order='50' />
        public bool WriteFooterOnArchivingOnly { get; set; }

        /// <summary>
        /// Gets the characters that are appended after each line.
        /// </summary>
        protected internal string NewLineChars => LineEnding.NewLineCharacters;

        /// <summary>
        /// Refresh the ArchiveFilePatternToWatch option of the <see cref="FileAppenderCache" />. 
        /// The log file must be watched for archiving when multiple processes are writing to the same 
        /// open file.
        /// </summary>
        private void RefreshArchiveFilePatternToWatch(string fileName, LogEventInfo logEvent)
        {
            _fileAppenderCache.CheckCloseAppenders -= AutoCloseAppendersAfterArchive;

            bool mustWatchArchiving = IsArchivingEnabled && KeepFileOpen && ConcurrentWrites;
            bool mustWatchActiveFile = EnableFileDelete && ((KeepFileOpen && ConcurrentWrites) || (IsSimpleKeepFileOpen && !EnableFileDeleteSimpleMonitor));
            if (mustWatchArchiving || mustWatchActiveFile)
            {
                _fileAppenderCache.CheckCloseAppenders += AutoCloseAppendersAfterArchive;   // Activates FileSystemWatcher
            }

#if !NETSTANDARD1_3
            if (mustWatchArchiving)
            {
                string archiveFilePattern = GetArchiveFileNamePattern(fileName, logEvent);
                var fileArchiveStyle = !string.IsNullOrEmpty(archiveFilePattern) ? GetFileArchiveHelper(archiveFilePattern) : null;
                string fileNameMask = fileArchiveStyle != null ? _fileArchiveHelper.GenerateFileNameMask(archiveFilePattern) : string.Empty;
                string directoryMask = !string.IsNullOrEmpty(fileNameMask) ? Path.Combine(Path.GetDirectoryName(archiveFilePattern), fileNameMask) : string.Empty;
                _fileAppenderCache.ArchiveFilePatternToWatch = directoryMask;
            }
            else
            {
                _fileAppenderCache.ArchiveFilePatternToWatch = null;
            }
#endif
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
            try
            {
                CleanupInitializedFiles(TimeSource.Current.Time.AddDays(-InitializedFilesCleanupPeriod));
            }
            catch (Exception exception)
            {
                if (exception.MustBeRethrownImmediately())
                    throw;

                InternalLogger.Error(exception, "{0}: Exception in CleanupInitializedFiles", this);
            }
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
            InternalLogger.Trace("{0}: CleanupInitializedFiles with cleanupThreshold {1}", this, cleanupThreshold);

            List<string> filesToFinalize = null;

            // Select the files require to be finalized.
            foreach (var file in _initializedFiles)
            {
                if (file.Value < cleanupThreshold)
                {
                    if (filesToFinalize is null)
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

            InternalLogger.Trace("{0}: CleanupInitializedFiles Completed and finalized {0} files", this, filesToFinalize?.Count ?? 0);
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
                InternalLogger.Trace("{0}: FlushAsync", this);
                _fileAppenderCache.FlushAppenders();
                asyncContinuation(null);
                InternalLogger.Trace("{0}: FlushAsync Done", this);
            }
            catch (Exception exception)
            {
                if (ExceptionMustBeRethrown(exception))
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
                    if (PlatformDetector.IsWin32 && !PlatformDetector.IsMono)
                    {
                        return WindowsMultiProcessFileAppender.TheFactory;
                    }
#endif
                }

                if (MutexDetector.SupportsSharableMutex)
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

        private bool IsSimpleKeepFileOpen => KeepFileOpen && !ConcurrentWrites && !ReplaceFileContentsOnEachWrite;

        private bool EnableFileDeleteSimpleMonitor => EnableFileDelete && IsSimpleKeepFileOpen
#if !NETSTANDARD
            && !PlatformDetector.IsWin32
#endif
            ;

        bool ICreateFileParameters.EnableFileDeleteSimpleMonitor => EnableFileDeleteSimpleMonitor;

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
                InternalLogger.Trace("{0}: Using appenderFactory: {1}", this, appenderFactory.GetType());
            }

            _fileAppenderCache = new FileAppenderCache(OpenFileCacheSize, appenderFactory, this);

            if ((OpenFileCacheSize > 0 || EnableFileDelete) && (OpenFileCacheTimeout > 0 || OpenFileFlushTimeout > 0))
            {
                int openFileAutoTimeoutSecs = (OpenFileCacheTimeout > 0 && OpenFileFlushTimeout > 0) ? Math.Min(OpenFileCacheTimeout, OpenFileFlushTimeout) : Math.Max(OpenFileCacheTimeout, OpenFileFlushTimeout);
                InternalLogger.Trace("{0}: Start autoClosingTimer", this);
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
                InternalLogger.Trace("{0}: Stop autoClosingTimer", this);
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

        private readonly ReusableStreamCreator _reusableFileWriteStream = new ReusableStreamCreator(4096);
        private readonly ReusableStreamCreator _reusableAsyncFileWriteStream = new ReusableStreamCreator(4096);
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

        /// <summary>
        /// Get full filename (=absolute) and cleaned if needed.
        /// </summary>
        /// <param name="logEvent"></param>
        /// <returns></returns>
        internal string GetFullFileName(LogEventInfo logEvent)
        {
            if (_fullFileName is null)
                return null;

            if (_fullFileName.IsFixedFilePath)
                return _fullFileName.Render(logEvent);

            using (var targetBuilder = ReusableLayoutBuilder.Allocate())
            {
                return _fullFileName.RenderWithBuilder(logEvent, targetBuilder.Result);
            }
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
            if (_getFullFileNameDelegate is null)
                _getFullFileNameDelegate = c => GetFullFileName(c.LogEvent);

            var buckets = logEvents.BucketSort(_getFullFileNameDelegate);

            using (var reusableStream = _reusableAsyncFileWriteStream.Allocate())
            {
                var ms = reusableStream.Result ?? new MemoryStream();

                foreach (var bucket in buckets)
                {
                    int bucketCount = bucket.Value.Count;
                    if (bucketCount <= 0)
                        continue;

                    string fileName = bucket.Key;
                    if (string.IsNullOrEmpty(fileName))
                    {
                        InternalLogger.Warn("{0}: FileName Layout returned empty string. The path is not of a legal form.", this);
                        var emptyPathException = new ArgumentException("The path is not of a legal form.");
                        for (int i = 0; i < bucketCount; ++i)
                        {
                            bucket.Value[i].Continuation(emptyPathException);
                        }
                        continue;
                    }

                    int currentIndex = 0;
                    while (currentIndex < bucketCount)
                    {
                        ms.Position = 0;
                        ms.SetLength(0);

                        var written = WriteToMemoryStream(bucket.Value, currentIndex, ms);
                        AppendMemoryStreamToFile(fileName, bucket.Value[currentIndex].LogEvent, ms, out var lastException);
                        for (int i = 0; i < written; ++i)
                        {
                            bucket.Value[currentIndex++].Continuation(lastException);
                        }
                    }
                }
            }
        }

        private int WriteToMemoryStream(IList<AsyncLogEventInfo> logEvents, int startIndex, MemoryStream ms)
        {
            long maxBufferSize = BufferSize * 100;   // Max Buffer Default = 30 KiloByte * 100 = 3 MegaByte

            using (var targetStream = _reusableFileWriteStream.Allocate())
            using (var targetBuilder = ReusableLayoutBuilder.Allocate())
            using (var targetBuffer = _reusableEncodingBuffer.Allocate())
            {
                var formatBuilder = targetBuilder.Result;
                var transformBuffer = targetBuffer.Result;
                var encodingStream = targetStream.Result;

                for (int i = startIndex; i < logEvents.Count; ++i)
                {
                    // For some CPU's then it is faster to write to a small MemoryStream, and then copy to the larger one
                    encodingStream.Position = 0;
                    encodingStream.SetLength(0);
                    formatBuilder.ClearBuilder();

                    AsyncLogEventInfo ev = logEvents[i];
                    RenderFormattedMessageToStream(ev.LogEvent, formatBuilder, transformBuffer, encodingStream);
                    ms.Write(encodingStream.GetBuffer(), 0, (int)encodingStream.Length);
                    if (ms.Length > maxBufferSize && !ReplaceFileContentsOnEachWrite)
                        return i - startIndex + 1;  // Max Chunk Size Limit to avoid out-of-memory issues
                }
            }

            return logEvents.Count - startIndex;
        }

        private void ProcessLogEvent(LogEventInfo logEvent, string fileName, ArraySegment<byte> bytesToWrite)
        {
            DateTime previousLogEventTimestamp = InitializeFile(fileName, logEvent);
            bool initializedNewFile = previousLogEventTimestamp == DateTime.MinValue;
            if (initializedNewFile && fileName == _previousLogFileName && _previousLogEventTimestamp.HasValue)
                previousLogEventTimestamp = _previousLogEventTimestamp.Value;

            bool archiveOccurred = TryArchiveFile(fileName, logEvent, bytesToWrite.Count, previousLogEventTimestamp, initializedNewFile);
            if (archiveOccurred)
                initializedNewFile = InitializeFile(fileName, logEvent) == DateTime.MinValue || initializedNewFile;

            if (ReplaceFileContentsOnEachWrite)
            {
                ReplaceFileContent(fileName, bytesToWrite, true);
            }
            else
            {
                WriteToFile(fileName, bytesToWrite, initializedNewFile);
            }

            _previousLogFileName = fileName;
            _previousLogEventTimestamp = logEvent.TimeStamp;
        }

        /// <summary>
        /// Obsolete and replaced by <see cref="RenderFormattedMessage"/> with NLog v5.
        /// Formats the log event for write.
        /// </summary>
        /// <param name="logEvent">The log event to be formatted.</param>
        /// <returns>A string representation of the log event.</returns>
        [Obsolete("No longer used and replaced by RenderFormattedMessage. Marked obsolete on NLog 5.0")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        protected virtual string GetFormattedMessage(LogEventInfo logEvent)
        {
            return Layout.Render(logEvent);
        }

        /// <summary>
        /// Obsolete and replaced by <see cref="RenderFormattedMessage"/> with NLog v5.
        /// Gets the bytes to be written to the file.
        /// </summary>
        /// <param name="logEvent">Log event.</param>
        /// <returns>Array of bytes that are ready to be written.</returns>
        [Obsolete("No longer used and replaced by RenderFormattedMessage. Marked obsolete on NLog 5.0")]
        [EditorBrowsable(EditorBrowsableState.Never)]
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
        /// Obsolete and replaced by <see cref="TransformStream"/> with NLog v5.
        /// Modifies the specified byte array before it gets sent to a file.
        /// </summary>
        /// <param name="value">The byte array.</param>
        /// <returns>The modified byte array. The function can do the modification in-place.</returns>
        [Obsolete("No longer used and replaced by TransformStream. Marked obsolete on NLog 5.0")]
        [EditorBrowsable(EditorBrowsableState.Never)]
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
            Layout.Render(logEvent, target);
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

        private void AppendMemoryStreamToFile(string currentFileName, LogEventInfo firstLogEvent, MemoryStream ms, out Exception lastException)
        {
            try
            {
                ArraySegment<byte> bytes = new ArraySegment<byte>(ms.GetBuffer(), 0, (int)ms.Length);
                ProcessLogEvent(firstLogEvent, currentFileName, bytes);
                lastException = null;
            }
            catch (Exception exception)
            {
                if (ExceptionMustBeRethrown(exception))
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
            if (archiveFolderPath != null && !Directory.Exists(archiveFolderPath))
                Directory.CreateDirectory(archiveFolderPath);

            if (string.Equals(fileName, archiveFileName, StringComparison.OrdinalIgnoreCase))
            {
                InternalLogger.Info("{0}: Archiving {1} skipped as ArchiveFileName equals FileName", this, fileName);
            }
            else if (EnableArchiveFileCompression)
            {
                InternalLogger.Info("{0}: Archiving {1} to compressed {2}", this, fileName, archiveFileName);
                if (File.Exists(archiveFileName))
                {
                    InternalLogger.Warn("{0}: Failed archiving because compressed file already exists: {1}", this, archiveFileName);
                }
                else
                {
                    ArchiveFileCompress(fileName, archiveFileName);
                }
            }
            else
            {
                InternalLogger.Info("{0}: Archiving {1} to {2}", this, fileName, archiveFileName);
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

        private void ArchiveFileCompress(string fileName, string archiveFileName)
        {
            int fileCompressRetryCount = ConcurrentWrites ? ConcurrentWriteAttempts : 2;
            for (int i = 1; i <= fileCompressRetryCount; ++i)
            {
                try
                {
                    if (FileCompressor is IArchiveFileCompressor archiveFileCompressor)
                    {
                        string entryName = (ArchiveNumbering != ArchiveNumberingMode.Rolling) ? (Path.GetFileNameWithoutExtension(archiveFileName) + Path.GetExtension(fileName)) : Path.GetFileName(fileName);
                        archiveFileCompressor.CompressFile(fileName, archiveFileName, entryName);
                    }
                    else
                    {
                        FileCompressor.CompressFile(fileName, archiveFileName);
                    }

                    break;  // Success
                }
                catch (DirectoryNotFoundException)
                {
                    throw;  // Skip retry when directory does not exist
                }
                catch (FileNotFoundException)
                {
                    throw;  // Skip retry when file does not exist
                }
                catch (IOException ex)
                {
                    if (i == fileCompressRetryCount)
                        throw;

                    if (File.Exists(archiveFileName))
                        throw;

                    int sleepTimeMs = i * 50;
                    InternalLogger.Warn("{0}: Archiving Attempt #{1} to compress {2} to {3} failed - {4} {5}. Sleeping for {6}ms", this, i, fileName, archiveFileName, ex.GetType(), ex.Message, sleepTimeMs);
                    AsyncHelpers.WaitForDelay(TimeSpan.FromMilliseconds(sleepTimeMs));
                }
            }

            DeleteAndWaitForFileDelete(fileName);
        }

        private void ArchiveFileAppendExisting(string fileName, string archiveFileName)
        {
            //todo handle double footer
            InternalLogger.Info("{0}: Already exists, append to {1}", this, archiveFileName);

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

                if (EnableFileDelete && !DeleteOldArchiveFile(fileName))
                {
                    // Attempt to delete file to reset File-Creation-Time (Delete under file-lock)
                    fileShare &= ~FileShare.Delete;  // Retry after having released file-lock
                }

                fileStream.Close(); // This flushes the content, too.
#if !NET35
                archiveFileStream.Flush(true);
#else
                archiveFileStream.Flush();                
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
                InternalLogger.Debug("{0}: Move file from '{1}' to '{2}'", this, fileName, archiveFileName);
                File.Move(fileName, archiveFileName);
            }
            catch (IOException ex)
            {
                if (IsSimpleKeepFileOpen)
                    throw;  // No need to retry, when only single process access

                if (!EnableFileDelete && KeepFileOpen)
                    throw;  // No need to retry when file delete has been disabled

                if (ConcurrentWrites && !MutexDetector.SupportsSharableMutex)
                    throw;  // No need to retry when not having a real archive mutex to protect us

                // It is possible to move a file while other processes has open file-handles.
                // Unless the other process is actively writing, then the file move might fail.
                // We are already holding the archive-mutex, so lets retry if things are stable
                InternalLogger.Warn(ex, "{0}: Archiving failed. Checking for retry move of {1} to {2}.", this, fileName, archiveFileName);
                if (!File.Exists(fileName) || File.Exists(archiveFileName))
                    throw;

                AsyncHelpers.WaitForDelay(TimeSpan.FromMilliseconds(50));

                if (!File.Exists(fileName) || File.Exists(archiveFileName))
                    throw;

                InternalLogger.Debug("{0}: Archiving retrying move of {1} to {2}.", this, fileName, archiveFileName);
                File.Move(fileName, archiveFileName);
            }
        }

        private bool DeleteOldArchiveFile(string fileName)
        {
            try
            {
                InternalLogger.Info("{0}: Deleting old archive file: '{1}'.", this, fileName);
                CloseInvalidFileHandle(fileName);
                File.Delete(fileName);
                return true;
            }
            catch (DirectoryNotFoundException exception)
            {
                //never rethrow this, as this isn't an exceptional case.
                InternalLogger.Debug(exception, "{0}: Failed to delete old log file '{1}' as directory is missing.", this, fileName);
                return false;
            }
            catch (Exception exception)
            {
                InternalLogger.Warn(exception, "{0}: Failed to delete old archive file: '{1}'.", this, fileName);
                if (ExceptionMustBeRethrown(exception))
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
                InternalLogger.Trace("{0}: Waiting for file delete of '{1}' for 12 sec", this, fileName);
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

                    InternalLogger.Warn("{0}: Timeout while deleting old archive file: '{1}'.", this, fileName);
                }
            }
            catch (Exception exception)
            {
                InternalLogger.Warn(exception, "{0}: Failed to delete old archive file: '{1}'.", this, fileName);
                if (ExceptionMustBeRethrown(exception))
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// Gets the correct formatting <see langword="String"/> to be used based on the value of <see
        /// cref="ArchiveEvery"/> for converting <see langword="DateTime"/> values which will be inserting into file
        /// names during archiving.
        /// 
        /// This value will be computed only when a empty value or <see langword="null"/> is passed into <paramref name="defaultFormat"/>
        /// </summary>
        /// <param name="defaultFormat">Date format to used irrespectively of <see cref="ArchiveEvery"/> value.</param>
        /// <returns>Formatting <see langword="String"/> for dates.</returns>
        private string GetArchiveDateFormatString(string defaultFormat)
        {
            // If archiveDateFormat is not set in the config file, use a default 
            // date format string based on the archive period.
            if (!string.IsNullOrEmpty(defaultFormat))
                return defaultFormat;

            switch (ArchiveEvery)
            {
                case FileArchivePeriod.Year:
                    return "yyyy";
                case FileArchivePeriod.Month:
                    return "yyyyMM";
                case FileArchivePeriod.Hour:
                    return "yyyyMMddHH";
                case FileArchivePeriod.Minute:
                    return "yyyyMMddHHmm";
                default:
                    return "yyyyMMdd";  // Also for Weekdays
            }
        }

        private DateTime? GetArchiveDate(string fileName, LogEventInfo logEvent, DateTime previousLogEventTimestamp)
        {
            // Using File LastModified to handle FileArchivePeriod.Month (where file creation time is one month ago)
            var fileLastModifiedUtc = _fileAppenderCache.GetFileLastWriteTimeUtc(fileName);

            InternalLogger.Trace("{0}: Calculating archive date. File-LastModifiedUtc: {1}; Previous LogEvent-TimeStamp: {2}", this, fileLastModifiedUtc, previousLogEventTimestamp);
            if (!fileLastModifiedUtc.HasValue)
            {
                if (previousLogEventTimestamp == DateTime.MinValue)
                {
                    InternalLogger.Info("{0}: Unable to acquire useful timestamp to archive file: {1}", this, fileName);
                    return null;
                }
                return previousLogEventTimestamp;
            }

            var lastWriteTimeSource = Time.TimeSource.Current.FromSystemTime(fileLastModifiedUtc.Value);
            if (previousLogEventTimestamp != DateTime.MinValue)
            {
                if (previousLogEventTimestamp > lastWriteTimeSource)
                {
                    InternalLogger.Trace("{0}: Using previous LogEvent-TimeStamp {1}, because more recent than File-LastModified {2}", this, previousLogEventTimestamp, lastWriteTimeSource);
                    return previousLogEventTimestamp;
                }

                if (PreviousLogOverlappedPeriod(logEvent, previousLogEventTimestamp, lastWriteTimeSource))
                {
                    InternalLogger.Trace("{0}: Using previous LogEvent-TimeStamp {1}, because archive period is overlapping with File-LastModified {2}", this, previousLogEventTimestamp, lastWriteTimeSource);
                    return previousLogEventTimestamp;
                }

                if (!AutoFlush && IsSimpleKeepFileOpen && previousLogEventTimestamp < lastWriteTimeSource)
                {
                    InternalLogger.Trace("{0}: Using previous LogEvent-TimeStamp {1}, because AutoFlush=false affects File-LastModified {2}", this, previousLogEventTimestamp, lastWriteTimeSource);
                    return previousLogEventTimestamp;
                }
            }

            InternalLogger.Trace("{0}: Using last write time: {1}", this, lastWriteTimeSource);
            return lastWriteTimeSource;
        }

        private bool PreviousLogOverlappedPeriod(LogEventInfo logEvent, DateTime previousLogEventTimestamp, DateTime lastFileWrite)
        {
            string formatString = GetArchiveDateFormatString(string.Empty);
            string lastWriteTimeString = lastFileWrite.ToString(formatString, CultureInfo.InvariantCulture);
            string logEventTimeString = logEvent.TimeStamp.ToString(formatString, CultureInfo.InvariantCulture);

            if (lastWriteTimeString != logEventTimeString)
                return false;

            DateTime? periodAfterPreviousLogEventTime = CalculateNextArchiveEventTime(previousLogEventTimestamp);
            if (!periodAfterPreviousLogEventTime.HasValue)
                return false;

            string periodAfterPreviousLogEventTimeString = periodAfterPreviousLogEventTime.Value.ToString(formatString, CultureInfo.InvariantCulture);
            return lastWriteTimeString == periodAfterPreviousLogEventTimeString;
        }

        DateTime? CalculateNextArchiveEventTime(DateTime timestamp)
        {
            switch (ArchiveEvery)
            {
                case FileArchivePeriod.Year:
                    return timestamp.AddYears(1);
                case FileArchivePeriod.Month:
                    return timestamp.AddMonths(1);
                case FileArchivePeriod.Day:
                    return timestamp.AddDays(1);
                case FileArchivePeriod.Hour:
                    return timestamp.AddHours(1);
                case FileArchivePeriod.Minute:
                    return timestamp.AddMinutes(1);
                case FileArchivePeriod.Sunday:
                    return CalculateNextWeekday(timestamp, DayOfWeek.Sunday);
                case FileArchivePeriod.Monday:
                    return CalculateNextWeekday(timestamp, DayOfWeek.Monday);
                case FileArchivePeriod.Tuesday:
                    return CalculateNextWeekday(timestamp, DayOfWeek.Tuesday);
                case FileArchivePeriod.Wednesday:
                    return CalculateNextWeekday(timestamp, DayOfWeek.Wednesday);
                case FileArchivePeriod.Thursday:
                    return CalculateNextWeekday(timestamp, DayOfWeek.Thursday);
                case FileArchivePeriod.Friday:
                    return CalculateNextWeekday(timestamp, DayOfWeek.Friday);
                case FileArchivePeriod.Saturday:
                    return CalculateNextWeekday(timestamp, DayOfWeek.Saturday);
                default:
                    return null;
            }
        }

        /// <summary>
        /// Calculate the DateTime of the requested day of the week.
        /// </summary>
        /// <param name="previousLogEventTimestamp">The DateTime of the previous log event.</param>
        /// <param name="dayOfWeek">The next occurring day of the week to return a DateTime for.</param>
        /// <returns>The DateTime of the next occurring dayOfWeek.</returns>
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
            InternalLogger.Debug("{0}: Do archive file: '{1}'", this, fileName);
            var fileInfo = new FileInfo(fileName);
            if (!fileInfo.Exists)
            {
                CloseInvalidFileHandle(fileName);   // Close possible stale file handles
                return;
            }

            string archiveFilePattern = GetArchiveFileNamePattern(fileName, eventInfo);
            if (string.IsNullOrEmpty(archiveFilePattern))
            {
                InternalLogger.Warn("{0}: Skip auto archive because archiveFilePattern is blank", this);
                return;
            }

            DateTime? archiveDate = GetArchiveDate(fileName, eventInfo, previousLogEventTimestamp);

            var archiveFileName = GenerateArchiveFileNameAfterCleanup(fileName, fileInfo, archiveFilePattern, archiveDate, initializedNewFile);
            if (!string.IsNullOrEmpty(archiveFileName))
            {
                ArchiveFile(fileInfo.FullName, archiveFileName);
            }
        }

        private string GenerateArchiveFileNameAfterCleanup(string fileName, FileInfo fileInfo, string archiveFilePattern, DateTime? archiveDate, bool initializedNewFile)
        {
            InternalLogger.Trace("{0}: Archive pattern '{1}'", this, archiveFilePattern);

            var fileArchiveStyle = GetFileArchiveHelper(archiveFilePattern);
            var existingArchiveFiles = fileArchiveStyle.GetExistingArchiveFiles(archiveFilePattern);

            if (MaxArchiveFiles == 1)
            {
                InternalLogger.Trace("{0}: MaxArchiveFiles = 1", this);
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

                if (initializedNewFile && string.Equals(Path.GetDirectoryName(archiveFilePattern), fileInfo.DirectoryName, StringComparison.OrdinalIgnoreCase))
                {
                    DeleteOldArchiveFile(fileName);
                    return null;
                }
            }

            var archiveFileName = archiveDate.HasValue ? fileArchiveStyle.GenerateArchiveFileName(archiveFilePattern, archiveDate.Value, existingArchiveFiles) : null;
            if (archiveFileName is null)
                return null;

            if (!initializedNewFile)
            {
                FinalizeFile(fileName, isArchiving: true);
            }

            if (existingArchiveFiles.Count > 0)
            {
                CleanupOldArchiveFiles(fileInfo, archiveFilePattern, existingArchiveFiles, archiveFileName);
            }

            return archiveFileName.FileName;
        }

        private void CleanupOldArchiveFiles(FileInfo currentFile, string archiveFilePattern, List<DateAndSequenceArchive> existingArchiveFiles, DateAndSequenceArchive newArchiveFile = null)
        {
            var fileArchiveStyle = GetFileArchiveHelper(archiveFilePattern);

            if (fileArchiveStyle.IsArchiveCleanupEnabled)
            {
                if (currentFile != null)
                    ExcludeActiveFileFromOldArchiveFiles(currentFile, existingArchiveFiles);

                if (newArchiveFile != null)
                    existingArchiveFiles.Add(newArchiveFile);

                var cleanupArchiveFiles = fileArchiveStyle.CheckArchiveCleanup(archiveFilePattern, existingArchiveFiles, MaxArchiveFiles, MaxArchiveDays);
                foreach (var oldArchiveFile in cleanupArchiveFiles)
                {
                    DeleteOldArchiveFile(oldArchiveFile.FileName);
                }
            }
        }

        private static void ExcludeActiveFileFromOldArchiveFiles(FileInfo currentFile, List<DateAndSequenceArchive> existingArchiveFiles)
        {
            if (existingArchiveFiles.Count > 0)
            {
                var archiveDirectory = Path.GetDirectoryName(existingArchiveFiles[0].FileName);
                if (string.Equals(archiveDirectory, currentFile.DirectoryName, StringComparison.OrdinalIgnoreCase))
                {
                    // Extra handling when archive-directory is the same as logging-directory
                    for (int i = 0; i < existingArchiveFiles.Count; ++i)
                    {
                        if (string.Equals(existingArchiveFiles[i].FileName, currentFile.FullName, StringComparison.OrdinalIgnoreCase))
                        {
                            existingArchiveFiles.RemoveAt(i);
                            break;
                        }
                    }
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
            if (_fullArchiveFileName is null)
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
                archiveFile = GetArchiveFileName(fileName, ev, upcomingWriteSize, previousLogEventTimestamp, initializedNewFile);
                if (!string.IsNullOrEmpty(archiveFile))
                {
                    archivedAppender = TryCloseFileAppenderBeforeArchive(fileName, archiveFile);
                }

#if !NETSTANDARD1_3
                // Closes all file handles if any archive operation has been detected by file-watcher
                _fileAppenderCache.InvalidateAppendersForArchivedFiles();
#endif
            }
            catch (Exception exception)
            {
                InternalLogger.Warn(exception, "{0}: Failed to check archive for file '{1}'.", this, fileName);
                if (ExceptionMustBeRethrown(exception))
                {
                    throw;
                }
            }

            if (string.IsNullOrEmpty(archiveFile))
                return false;

            try
            {
#if SupportsMutex
                try
                {
                    if (archivedAppender is BaseMutexFileAppender mutexFileAppender && mutexFileAppender.ArchiveMutex != null)
                    {
                        mutexFileAppender.ArchiveMutex.WaitOne();
                    }
                    else if (!IsSimpleKeepFileOpen)
                    {
                        InternalLogger.Debug("{0}: Archive mutex not available for file '{1}'", this, archiveFile);
                    }
                }
                catch (AbandonedMutexException)
                {
                    // ignore the exception, another process was killed without properly releasing the mutex
                    // the mutex has been acquired, so proceed to writing
                    // See: https://msdn.microsoft.com/en-us/library/system.threading.abandonedmutexexception.aspx
                }
#endif

                ArchiveFileAfterCloseFileAppender(archivedAppender, archiveFile, ev, upcomingWriteSize, previousLogEventTimestamp);
                return true;
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

        /// <summary>
        /// Closes any active file-appenders that matches the input filenames.
        /// File-appender is requested to invalidate/close its filehandle, but keeping its archive-mutex alive
        /// </summary>
        private BaseFileAppender TryCloseFileAppenderBeforeArchive(string fileName, string archiveFile)
        {
            InternalLogger.Trace("{0}: Archive attempt for file '{1}'", this, archiveFile);
            BaseFileAppender archivedAppender = _fileAppenderCache.InvalidateAppender(fileName);
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

            return archivedAppender;
        }

        private void ArchiveFileAfterCloseFileAppender(BaseFileAppender archivedAppender, string archiveFile, LogEventInfo ev, int upcomingWriteSize, DateTime previousLogEventTimestamp)
        {
            try
            {
                DateTime fallbackFileCreationTimeSource = previousLogEventTimestamp;

                if (archivedAppender != null && IsSimpleKeepFileOpen)
                {
                    var fileCreationTimeUtc = archivedAppender.GetFileCreationTimeUtc();
                    if (fileCreationTimeUtc > DateTime.MinValue)
                    {
                        var fileCreationTimeSource = Time.TimeSource.Current.FromSystemTime(fileCreationTimeUtc.Value);
                        if (fileCreationTimeSource < fallbackFileCreationTimeSource || fallbackFileCreationTimeSource == DateTime.MinValue)
                        {
                            fallbackFileCreationTimeSource = fileCreationTimeSource;
                        }
                    }
                }

                // Check again if archive is needed. We could have been raced by another process
                var validatedArchiveFile = GetArchiveFileName(archiveFile, ev, upcomingWriteSize, fallbackFileCreationTimeSource, false);
                if (string.IsNullOrEmpty(validatedArchiveFile))
                {
                    InternalLogger.Debug("{0}: Skip archiving '{1}' because no longer necessary", this, archiveFile);
                    _initializedFiles.Remove(archiveFile);
                }
                else
                {
                    if (archiveFile != validatedArchiveFile)
                    {
                        _initializedFiles.Remove(archiveFile);
                        archiveFile = validatedArchiveFile;
                    }
                    _initializedFiles.Remove(archiveFile);

                    DoAutoArchive(archiveFile, ev, previousLogEventTimestamp, false);
                }

                if (_previousLogFileName == archiveFile)
                {
                    _previousLogFileName = null;
                    _previousLogEventTimestamp = null;
                }
            }
            catch (Exception exception)
            {
                InternalLogger.Warn(exception, "{0}: Failed to archive file '{1}'.", this, archiveFile);
                if (ExceptionMustBeRethrown(exception))
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// Indicates if the automatic archiving process should be executed.
        /// </summary>
        /// <param name="fileName">File name to be written.</param>
        /// <param name="ev">Log event that the <see cref="FileTarget"/> instance is currently processing.</param>
        /// <param name="upcomingWriteSize">The size in bytes of the next chunk of data to be written in the file.</param>
        /// <param name="previousLogEventTimestamp">The DateTime of the previous log event for this file.</param>
        /// <param name="initializedNewFile">File has just been opened.</param>
        /// <returns>Filename to archive. If <c>null</c>, then nothing to archive.</returns>
        private string GetArchiveFileName(string fileName, LogEventInfo ev, int upcomingWriteSize, DateTime previousLogEventTimestamp, bool initializedNewFile)
        {
            fileName = fileName ?? _previousLogFileName;
            if (!string.IsNullOrEmpty(fileName))
            {
                return GetArchiveFileNameBasedOnFileSize(fileName, upcomingWriteSize, initializedNewFile) ??
                       GetArchiveFileNameBasedOnTime(fileName, ev, previousLogEventTimestamp, initializedNewFile);
            }

            return null;
        }

        /// <summary>
        /// Returns the correct filename to archive
        /// </summary>
        private string GetPotentialFileForArchiving(string fileName)
        {
            if (!string.IsNullOrEmpty(fileName))
            {
                return fileName;
            }

            if (!string.IsNullOrEmpty(_previousLogFileName))
            {
                return _previousLogFileName;
            }

            return fileName;
        }

        /// <summary>
        /// Gets the file name for archiving, or null if archiving should not occur based on file size.
        /// </summary>
        /// <param name="fileName">File name to be written.</param>
        /// <param name="upcomingWriteSize">The size in bytes of the next chunk of data to be written in the file.</param>
        /// <param name="initializedNewFile">File has just been opened.</param>
        /// <returns>Filename to archive. If <c>null</c>, then nothing to archive.</returns>
        private string GetArchiveFileNameBasedOnFileSize(string fileName, int upcomingWriteSize, bool initializedNewFile)
        {
            if (ArchiveAboveSize == ArchiveAboveSizeDisabled)
            {
                return null;
            }

            var archiveFileName = GetPotentialFileForArchiving(fileName);
            if (string.IsNullOrEmpty(archiveFileName))
            {
                return null;
            }

            //this is an expensive call
            var fileLength = _fileAppenderCache.GetFileLength(archiveFileName);
            if (!fileLength.HasValue)
            {
                archiveFileName = TryFallbackToPreviousLogFileName(archiveFileName, initializedNewFile);
                if (!string.IsNullOrEmpty(archiveFileName))
                {
                    upcomingWriteSize = 0;
                    return GetArchiveFileNameBasedOnFileSize(archiveFileName, upcomingWriteSize, false);
                }
                else
                {
                    return null;
                }
            }

            if (archiveFileName != fileName)
            {
                upcomingWriteSize = 0;  // Not going to write to this file
            }

            var shouldArchive = (fileLength.Value + upcomingWriteSize) > ArchiveAboveSize;
            if (shouldArchive)
            {
                InternalLogger.Debug("{0}: Start archiving '{1}' because FileSize={2} + {3} is larger than ArchiveAboveSize={4}", this, archiveFileName, fileLength.Value, upcomingWriteSize, ArchiveAboveSize);
                return archiveFileName;    // Will re-check if archive is still necessary after flush/close file
            }

            return null;
        }

        /// <summary>
        /// Check if archive operation should check previous filename, because FileAppenderCache tells us current filename no longer exists
        /// </summary>
        private string TryFallbackToPreviousLogFileName(string archiveFileName, bool initializedNewFile)
        {
            if (!initializedNewFile && _initializedFiles.Remove(archiveFileName))
            {
                // Register current filename needs re-initialization
                InternalLogger.Debug("{0}: Invalidate appender as archive file no longer exists: '{1}'", this, archiveFileName);
                _fileAppenderCache.InvalidateAppender(archiveFileName)?.Dispose();
            }

            if (!string.IsNullOrEmpty(_previousLogFileName) && !string.Equals(archiveFileName, _previousLogFileName, StringComparison.OrdinalIgnoreCase))
            {
                return _previousLogFileName;
            }

            return string.Empty;
        }

        /// <summary>
        /// Returns the file name for archiving, or null if archiving should not occur based on date/time.
        /// </summary>
        /// <param name="fileName">File name to be written.</param>
        /// <param name="logEvent">Log event that the <see cref="FileTarget"/> instance is currently processing.</param>
        /// <param name="previousLogEventTimestamp">The DateTime of the previous log event for this file.</param>
        /// <param name="initializedNewFile">File has just been opened.</param>
        /// <returns>Filename to archive. If <c>null</c>, then nothing to archive.</returns>
        private string GetArchiveFileNameBasedOnTime(string fileName, LogEventInfo logEvent, DateTime previousLogEventTimestamp, bool initializedNewFile)
        {
            if (ArchiveEvery == FileArchivePeriod.None)
            {
                return null;
            }

            var archiveFileName = GetPotentialFileForArchiving(fileName);
            if (string.IsNullOrEmpty(archiveFileName))
            {
                return null;
            }

            DateTime? creationTimeSource = TryGetArchiveFileCreationTimeSource(archiveFileName, previousLogEventTimestamp);
            if (!creationTimeSource.HasValue)
            {
                archiveFileName = TryFallbackToPreviousLogFileName(archiveFileName, initializedNewFile);
                if (!string.IsNullOrEmpty(archiveFileName))
                {
                    return GetArchiveFileNameBasedOnTime(archiveFileName, logEvent, previousLogEventTimestamp, false);
                }
                else
                {
                    return null;
                }
            }

            DateTime fileCreateTime = TruncateArchiveTime(creationTimeSource.Value, ArchiveEvery);
            DateTime logEventTime = TruncateArchiveTime(logEvent.TimeStamp, ArchiveEvery);
            if (fileCreateTime != logEventTime)
            {
                string formatString = GetArchiveDateFormatString(string.Empty);
                var validLogEventTime = EnsureValidLogEventTimeStamp(logEvent.TimeStamp, creationTimeSource.Value);
                string fileCreated = creationTimeSource.Value.ToString(formatString, CultureInfo.InvariantCulture);
                string logEventRecorded = validLogEventTime.ToString(formatString, CultureInfo.InvariantCulture);
                var shouldArchive = fileCreated != logEventRecorded;
                if (shouldArchive)
                {
                    InternalLogger.Debug("{0}: Start archiving '{1}' because FileCreatedTime='{2}' is older than now '{3}' using ArchiveEvery='{4}'", this, archiveFileName, fileCreated, logEventRecorded, formatString);
                    return archiveFileName;    // Will re-check if archive is still necessary after flush/close file
                }
            }

            return null;
        }

        private DateTime? TryGetArchiveFileCreationTimeSource(string fileName, DateTime previousLogEventTimestamp)
        {
            // Linux FileSystems doesn't always have file-birth-time, so NLog tries to provide a little help
            DateTime? fallbackTimeSourceLinux = (previousLogEventTimestamp != DateTime.MinValue && IsSimpleKeepFileOpen) ? previousLogEventTimestamp : (DateTime?)null;
            var creationTimeSource = _fileAppenderCache.GetFileCreationTimeSource(fileName, fallbackTimeSourceLinux);
            if (!creationTimeSource.HasValue)
                return null;

            if (previousLogEventTimestamp > DateTime.MinValue && previousLogEventTimestamp < creationTimeSource)
            {
                if (TruncateArchiveTime(previousLogEventTimestamp, FileArchivePeriod.Minute) < TruncateArchiveTime(creationTimeSource.Value, FileArchivePeriod.Minute) && PlatformDetector.IsUnix)
                {
                    if (IsSimpleKeepFileOpen)
                    {
                        InternalLogger.Debug("{0}: Adjusted file creation time from {1} to {2}. Linux FileSystem probably don't support file birthtime.", this, creationTimeSource, previousLogEventTimestamp);
                        creationTimeSource = previousLogEventTimestamp;
                    }
                    else
                    {
                        InternalLogger.Debug("{0}: File creation time {1} newer than previous file write time {2}. Linux FileSystem probably don't support file birthtime, unless multiple applications are writing to the same file. Configure FileTarget.KeepFileOpen=true AND FileTarget.ConcurrentWrites=false, so NLog can fix this.", this, creationTimeSource, previousLogEventTimestamp);
                    }
                }
            }

            return creationTimeSource;
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

                InternalLogger.Trace("{0}: Auto Close FileAppenders after archive", this);
                _fileAppenderCache.CloseExpiredAppenders(DateTime.MinValue);
            }
            catch (Exception exception)
            {
#if DEBUG
                if (exception.MustBeRethrownImmediately())
                {
                    throw;  // Throwing exceptions here will crash the entire application (.NET 2.0 behavior)
                }
#endif

                InternalLogger.Warn(exception, "{0}: Exception in AutoCloseAppendersAfterArchive", this);
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
                    DateTime expireTimeUtc = DateTime.UtcNow.AddSeconds(-OpenFileCacheTimeout);
                    InternalLogger.Trace("{0}: Auto Close FileAppenders", this);
                    _fileAppenderCache.CloseExpiredAppenders(expireTimeUtc);
                }

                if (OpenFileFlushTimeout > 0 && !AutoFlush)
                {
                    ConditionalFlushOpenFileAppenders();
                }
            }
            catch (Exception exception)
            {
#if DEBUG
                if (exception.MustBeRethrownImmediately())
                {
                    throw;  // Throwing exceptions here will crash the entire application (.NET 2.0 behavior)
                }
#endif

                InternalLogger.Warn(exception, "{0}: Exception in AutoClosingTimerCallback", this);
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
                InternalLogger.Trace("{0}: Auto Flush FileAppenders", this);
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
                InternalLogger.Error(ex, "{0}: Failed write to file '{1}'.", this, fileName);
                _fileAppenderCache.InvalidateAppender(fileName)?.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Initialize a file to be used by the <see cref="FileTarget"/> instance. Based on the number of initialized
        /// files and the values of various instance properties clean up and/or archiving processes can be invoked.
        /// </summary>
        /// <param name="fileName">File name to be written.</param>
        /// <param name="logEvent">Log event that the <see cref="FileTarget"/> instance is currently processing.</param>
        /// <returns>The DateTime of the previous log event for this file (DateTime.MinValue if just initialized).</returns>
        private DateTime InitializeFile(string fileName, LogEventInfo logEvent)
        {
            if (_initializedFiles.Count != 0 && logEvent.TimeStamp == _previousLogEventTimestamp && _previousLogFileName == fileName)
            {
                return logEvent.TimeStamp;
            }

            var now = logEvent.TimeStamp;
            if (!_initializedFiles.TryGetValue(fileName, out var lastTime))
            {
                PrepareForNewFile(fileName, logEvent);

                _initializedFilesCounter++;
                if (_initializedFilesCounter > OpenFileCacheSize)
                {
                    // Attempt to write footer, before closing file appender
                    _initializedFilesCounter = 0;
                    CleanupInitializedFiles();
                    _initializedFilesCounter = Math.Min(_initializedFiles.Count, OpenFileCacheSize / 2);
                }

                _initializedFiles[fileName] = now;
                return DateTime.MinValue;
            }
            else if (lastTime != now)
            {
                now = EnsureValidLogEventTimeStamp(now, lastTime);
                _initializedFiles[fileName] = now;
            }

            return lastTime;
        }

        private static DateTime EnsureValidLogEventTimeStamp(DateTime logEventTimeStamp, DateTime previousTimeStamp)
        {
            // Truncating using DateTime.Date is "expensive", so first check if it look like it is from the past
            if (logEventTimeStamp < previousTimeStamp && logEventTimeStamp.Date < previousTimeStamp.Date)
            {
                // Received LogEvent from the past when comparing to the previous timestamp
                var currentTime = TimeSource.Current.Time;
                if (logEventTimeStamp.Date < currentTime.AddMinutes(-1).Date)
                {
                    // It is not because the machine-time has changed. Probably a LogEvent from the past
                    if (currentTime.Date < previousTimeStamp.Date)
                        return currentTime; // Previous timestamp is from the future. We choose machine-time
                    else
                        return previousTimeStamp;
                }
            }

            return logEventTimeStamp;
        }

        private void CloseInvalidFileHandle(string fileName)
        {
            try
            {
                _fileAppenderCache.InvalidateAppender(fileName)?.Dispose();
            }
            finally
            {
                _initializedFiles.Remove(fileName); // Skip finalize non-existing file
            }
        }

        /// <summary>
        /// Writes the file footer and finalizes the file in <see cref="FileTarget"/> instance internal structures.
        /// </summary>
        /// <param name="fileName">File name to close.</param>
        /// <param name="isArchiving">Indicates if the file is being finalized for archiving.</param>
        private void FinalizeFile(string fileName, bool isArchiving = false)
        {
            try
            {
                InternalLogger.Trace("{0}: FinalizeFile '{1}, isArchiving: {2}'", this, fileName, isArchiving);
                if ((isArchiving) || (!WriteFooterOnArchivingOnly))
                    WriteFooter(fileName);
            }
            finally
            {
                CloseInvalidFileHandle(fileName);
            }
        }

        /// <summary>
        /// Writes the footer information to a file.
        /// </summary>
        /// <param name="fileName">The file path to write to.</param>
        private void WriteFooter(string fileName)
        {
            ArraySegment<byte> footerBytes = GetLayoutBytes(Footer);
            if (footerBytes.Count > 0 && File.Exists(fileName))
            {
                WriteToFile(fileName, footerBytes, false);
            }
        }

        /// <summary>
        /// Decision logic whether to archive logfile on startup.
        /// <see cref="ArchiveOldFileOnStartup"/> and <see cref="ArchiveOldFileOnStartupAboveSize"/> properties.
        /// </summary>
        /// <param name="fileName">File name to be written.</param>
        /// <returns>Decision whether to archive or not.</returns>
        internal bool ShouldArchiveOldFileOnStartup(string fileName)
        {
            if (_archiveOldFileOnStartup == false)
            {
                // explicitly disabled and not the default
                return false;
            }

            var aboveSizeSet = ArchiveOldFileOnStartupAboveSize > 0;
            if (aboveSizeSet)
            {
                // Check whether size threshold exceeded
                var length = _fileAppenderCache.GetFileLength(fileName);
                return length.HasValue && length.Value > ArchiveOldFileOnStartupAboveSize;
            }

            // No size threshold specified, use archiveOldFileOnStartup flag
            return _archiveOldFileOnStartup == true;
        }

        /// <summary>
        /// Invokes the archiving and clean up of older archive file based on the values of
        /// <see cref="NLog.Targets.FileTarget.ArchiveOldFileOnStartup"/> and
        /// <see cref="NLog.Targets.FileTarget.DeleteOldFileOnStartup"/> properties respectively.
        /// </summary>
        /// <param name="fileName">File name to be written.</param>
        /// <param name="logEvent">Log event that the <see cref="FileTarget"/> instance is currently processing.</param>
        private void PrepareForNewFile(string fileName, LogEventInfo logEvent)
        {
            InternalLogger.Debug("{0}: Preparing for new file: '{1}'", this, fileName);
            RefreshArchiveFilePatternToWatch(fileName, logEvent);

            try
            {
                if (ShouldArchiveOldFileOnStartup(fileName))
                {
                    DoAutoArchive(fileName, logEvent, DateTime.MinValue, true);
                }
            }
            catch (Exception exception)
            {
                InternalLogger.Warn(exception, "{0}: Unable to archive old log file '{1}'.", this, fileName);
                if (ExceptionMustBeRethrown(exception))
                {
                    throw;
                }
            }

            if (DeleteOldFileOnStartup)
            {
                DeleteOldArchiveFile(fileName);
            }

            try
            {
                string archiveFilePattern = GetArchiveFileNamePattern(fileName, logEvent);
                if (!string.IsNullOrEmpty(archiveFilePattern))
                {
                    var fileArchiveStyle = GetFileArchiveHelper(archiveFilePattern);
                    if (fileArchiveStyle.AttemptCleanupOnInitializeFile(archiveFilePattern, MaxArchiveFiles, MaxArchiveDays))
                    {
                        var existingArchiveFiles = fileArchiveStyle.GetExistingArchiveFiles(archiveFilePattern);
                        if (existingArchiveFiles.Count > 0)
                        {
                            CleanupOldArchiveFiles(new FileInfo(fileName), archiveFilePattern, existingArchiveFiles);
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                InternalLogger.Warn(exception, "FileTarget(Name={0}): Failed to cleanup old archive files when starting on new file: '{1}'", Name, fileName);

                if (ExceptionMustBeRethrown(exception))
                {
                    throw;
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

        private static bool InitialValueBom(Encoding encoding)
        {
            // Initial of true for UTF 16 and UTF 32
            const int utf16 = 1200;
            const int utf16Be = 1201;
            const int utf32 = 12000;
            const int urf32Be = 12001;
            var codePage = encoding?.CodePage ?? 0;
            return codePage == utf16
                   || codePage == utf16Be
                   || codePage == utf32
                   || codePage == urf32Be;
        }

        /// <summary>
        /// Writes the header information and byte order mark to a file.
        /// </summary>
        /// <param name="appender">File appender associated with the file.</param>
        private void WriteHeaderAndBom(BaseFileAppender appender)
        {
            //performance: cheap check before checking file info 
            if (Header is null && !WriteBom) return;

            var length = appender.GetFileLength();
            // File is empty or file info cannot be obtained
            var isNewOrEmptyFile = length is null || length == 0;
            
            if (isNewOrEmptyFile && WriteBom)
            {
                InternalLogger.Trace("{0}: Write byte order mark from encoding={1}", this, Encoding);
                var preamble = Encoding.GetPreamble();
                if (preamble.Length > 0)
                    appender.Write(preamble, 0, preamble.Length);
            }
            
            if (Header != null && (isNewOrEmptyFile || WriteHeaderWhenInitialFileNotEmpty))
            {
                InternalLogger.Trace("{0}: Write header", this);
                ArraySegment<byte> headerBytes = GetLayoutBytes(Header);
                if (headerBytes.Count > 0)
                {
                    appender.Write(headerBytes.Array, headerBytes.Offset, headerBytes.Count);
                }
            }
        }

        /// <summary>
        /// The sequence of <see langword="byte"/> to be written in a file after applying any formatting and any
        /// transformations required from the <see cref="Layout"/>.
        /// </summary>
        /// <param name="layout">The layout used to render output message.</param>
        /// <returns>Sequence of <see langword="byte"/> to be written.</returns>
        /// <remarks>Usually it is used to render the header and hooter of the files.</remarks>
        private ArraySegment<byte> GetLayoutBytes(Layout layout)
        {
            if (layout is null)
            {
                return default(ArraySegment<byte>);
            }

            using (var targetBuilder = ReusableLayoutBuilder.Allocate())
            using (var targetBuffer = _reusableEncodingBuffer.Allocate())
            {
                var nullEvent = LogEventInfo.CreateNullEvent();
                layout.Render(nullEvent, targetBuilder.Result);
                targetBuilder.Result.Append(NewLineChars);
                using (MemoryStream ms = new MemoryStream(targetBuilder.Result.Length))
                {
                    TransformBuilderToStream(nullEvent, targetBuilder.Result, targetBuffer.Result, ms);
                    return new ArraySegment<byte>(ms.ToArray());
                }
            }
        }
    }
}
