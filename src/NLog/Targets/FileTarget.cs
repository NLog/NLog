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
        /// Holds the initialised files each given time by the <see cref="FileTarget"/> instance. Against each file, the last write time is stored. 
        /// </summary>
        /// <remarks>Last write time is store in local time (no UTC).</remarks>
        private readonly Dictionary<string, DateTime> initializedFiles = new Dictionary<string, DateTime>();

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

        private Timer autoClosingTimer;

#if !SILVERLIGHT && !__IOS__ && !__ANDROID__
        private Thread appenderInvalidatorThread = null;
        private EventWaitHandle stopAppenderInvalidatorThreadWaitHandle = new ManualResetEvent(false);
#endif

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
        private FilePathLayout fullFileName;

        /// <summary>
        /// The archive file name as target
        /// </summary>
        private FilePathLayout fullarchiveFileName;

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
            this.fileArchive = new DynamicFileArchive(this, MaxArchiveFiles);
            this.ForceManaged = false;
            this.ArchiveDateFormat = string.Empty;

            this.maxLogFilenames = 20;
            this.previousFileNames = new Queue<string>(this.maxLogFilenames);
            this.fileAppenderCache = FileAppenderCache.Empty;
            this.CleanupFileName = true;
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

                if (IsInitialized)
                {
                    //don't call before initialized because this could lead to stackoverflows.
                    RefreshFileArchive();
                    RefreshArchiveFilePatternToWatch();
                }
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
            get { return _cleanupFileName; }
            set
            {
                _cleanupFileName = value;
                fullFileName = CreateFileNameLayout(FileName);
                fullarchiveFileName = CreateFileNameLayout(ArchiveFileName);
            }
        }

        /// <summary>
        /// Is the  <see cref="FileName"/> an absolute or relative path?
        /// </summary>
        [DefaultValue(FilePathKind.Unknown)]
        public FilePathKind FileNameKind


        {
            get { return _fileNameKind; }
            set
            {

                _fileNameKind = value;
                fullFileName = CreateFileNameLayout(FileName);
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
                keepFileOpen = value;
                if (IsInitialized)
                {
                    RefreshArchiveFilePatternToWatch();
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
        public bool ConcurrentWrites
        {
            get { return concurrentWrites; }
            set
            {
                concurrentWrites = value;
                if (IsInitialized)
                {
                    RefreshArchiveFilePatternToWatch();
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
        public long ArchiveAboveSize
        {
            get { return archiveAboveSize; }
            set
            {
                archiveAboveSize = value;
                if (IsInitialized)
                {
                    RefreshArchiveFilePatternToWatch();
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
                archiveEvery = value;
                if (IsInitialized)
                {
                    RefreshArchiveFilePatternToWatch();
                }
            }
        }

        /// <summary>
        /// Is the  <see cref="ArchiveFileName"/> an absolute or relative path?
        /// </summary>
        public FilePathKind ArchiveFileKind
        {
            get { return _archiveFileKind; }
            set
            {
                _archiveFileKind = value;
                fullarchiveFileName = CreateFileNameLayout(ArchiveFileName);
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
                if (fullarchiveFileName == null) return null;

                return fullarchiveFileName.GetLayout();
            }
            set
            {
                fullarchiveFileName = CreateFileNameLayout(value);
                if (IsInitialized)
                {
                    //don't call before initialized because this could lead to stackoverflows.
                    RefreshFileArchive();
                    RefreshArchiveFilePatternToWatch();
                }
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
                enableArchiveFileCompression = value;
                if (IsInitialized)
                {
                    RefreshArchiveFilePatternToWatch();
                }
            }
        }


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
            string fileNamePattern = GetArchiveFileNamePattern(GetFullFileName(nullEvent), nullEvent);
            if (fileNamePattern == null)
            {
                InternalLogger.Debug("no RefreshFileArchive because fileName is NULL");
                return;
            }

            if (!ContainsFileNamePattern(fileNamePattern))
            {
                try
                {
                    fileArchive.InitializeForArchiveFolderPath(Path.GetDirectoryName(fileNamePattern));
                }
                catch (Exception exception)
                {

                    if (exception.MustBeRethrownImmediately())
                    {
                        throw;
                    }

                    //TODO NLog 5, check MustBeRethrown()

                    InternalLogger.Warn(exception, "Error while initializing archive folder.");
                }
            }
        }

        /// <summary>
        /// Refresh the ArchiveFilePatternToWatch option of the <see cref="FileAppenderCache" />. 
        /// The log file must be watched for archiving when multiple processes are writing to the same 
        /// open file.
        /// </summary>
        private void RefreshArchiveFilePatternToWatch()
        {
#if !SILVERLIGHT && !__IOS__ && !__ANDROID__
            if (this.fileAppenderCache != null)
            {
                bool mustWatchArchiving = IsArchivingEnabled() && ConcurrentWrites && KeepFileOpen;
                if (mustWatchArchiving)
                {
                    var nullEvent = LogEventInfo.CreateNullEvent();
                    string fileNamePattern = GetArchiveFileNamePattern(GetFullFileName(nullEvent), nullEvent);
                    if (!string.IsNullOrEmpty(fileNamePattern))
                    {
                        fileNamePattern = Path.Combine(Path.GetDirectoryName(fileNamePattern), ReplaceFileNamePattern(fileNamePattern, "*"));
                        //fileNamePattern is absolute
                        this.fileAppenderCache.ArchiveFilePatternToWatch = fileNamePattern;

                        if ((EnableArchiveFileCompression) && (this.appenderInvalidatorThread == null))
                        {
                            // EnableArchiveFileCompression creates a new file for the archive, instead of just moving the log file.
                            // The log file is deleted instead of moved. This process may be holding a lock to that file which will
                            // avoid the file from being deleted. Therefore we must periodically close appenders for files that 
                            // were archived so that the file can be deleted.

                            this.appenderInvalidatorThread = new Thread(() =>
                            {
                                while (true)
                                {
                                    try
                                    {
                                        if (this.stopAppenderInvalidatorThreadWaitHandle.WaitOne(200))
                                            break;

                                        lock (SyncRoot)
                                        {
                                            this.fileAppenderCache.InvalidateAppendersForInvalidFiles();
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        InternalLogger.Debug(ex, "Exception in FileTarget appender-invalidator thread.");
                                    }
                                }
                            });
                            this.appenderInvalidatorThread.IsBackground = true;
                            this.appenderInvalidatorThread.Start();
                        }
                    }
                }
                else
                {
                    this.fileAppenderCache.ArchiveFilePatternToWatch = null;

                    this.StopAppenderInvalidatorThread();
                }
            }
#endif
        }

        private void StopAppenderInvalidatorThread()
        {
#if !SILVERLIGHT && !__IOS__ && !__ANDROID__
            if (this.appenderInvalidatorThread != null)
            {
                this.stopAppenderInvalidatorThreadWaitHandle.Set();
                this.appenderInvalidatorThread = null;
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
                this.UninitializeFile(fileName);
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
            if (!this.KeepFileOpen)
            {
                return RetryingMultiProcessFileAppender.TheFactory;
            }
            else if (this.NetworkWrites)
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
            else if (IsArchivingEnabled())
                return CountingSingleProcessFileAppender.TheFactory;
            else
                return SingleProcessFileAppender.TheFactory;
        }

        private bool IsArchivingEnabled()
        {
            return this.ArchiveAboveSize != FileTarget.ArchiveAboveSizeDisabled || this.ArchiveEvery != FileArchivePeriod.None;
        }

        /// <summary>
        /// Initializes file logging by creating data structures that
        /// enable efficient multi-file logging.
        /// </summary>
        protected override void InitializeTarget()
        {
            base.InitializeTarget();

            RefreshFileArchive();
            this.appenderFactory = GetFileAppenderFactory();

            this.fileAppenderCache = new FileAppenderCache(this.OpenFileCacheSize, this.appenderFactory, this);
            RefreshArchiveFilePatternToWatch();

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
                this.UninitializeFile(fileName);
            }

            if (this.autoClosingTimer != null)
            {
                this.autoClosingTimer.Change(Timeout.Infinite, Timeout.Infinite);
                this.autoClosingTimer.Dispose();
                this.autoClosingTimer = null;
            }

            this.StopAppenderInvalidatorThread();

            this.fileAppenderCache.CloseAppenders();
        }

        /// <summary>
        /// Writes the specified logging event to a file specified in the FileName 
        /// parameter.
        /// </summary>
        /// <param name="logEvent">The logging event.</param>
        protected override void Write(LogEventInfo logEvent)
        {
            var fullFileName = this.GetFullFileName(logEvent);
            byte[] bytes = this.GetBytesToWrite(logEvent);
            ProcessLogEvent(logEvent, fullFileName, bytes);
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
            return this.fullFileName.Render(logEvent);
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
            var buckets = logEvents.BucketSort(c => this.GetFullFileName(c.LogEvent));
            using (var ms = new MemoryStream())
            {
                var pendingContinuations = new List<AsyncContinuation>();

                foreach (var bucket in buckets)
                {
                    string fileName = bucket.Key;

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



        private void ProcessLogEvent(LogEventInfo logEvent, string fileName, byte[] bytesToWrite)
        {
#if !SILVERLIGHT && !__IOS__ && !__ANDROID__
            this.fileAppenderCache.InvalidateAppendersForInvalidFiles();
#endif
            var archiveFile = this.GetAutoArchiveFileName(fileName, logEvent, bytesToWrite.Length);
            if (!string.IsNullOrEmpty(archiveFile))
            {
                this.DoAutoArchive(archiveFile, logEvent);
            }

            // Clean up old archives if this is the first time a log record is being written to
            // this log file and the archiving system is date/time based.
            if (this.ArchiveNumbering == ArchiveNumberingMode.Date && this.ArchiveEvery != FileArchivePeriod.None &&
                ShouldDeleteOldArchives())
            {
                if (!previousFileNames.Contains(fileName))
                {
                    if (this.previousFileNames.Count > this.maxLogFilenames)
                    {
                        this.previousFileNames.Dequeue();
                    }

                    string fileNamePattern = this.GetArchiveFileNamePattern(fileName, logEvent);
                    if (fileNamePattern != null)
                    {
                        this.DeleteOldDateArchives(fileNamePattern);
                    }
                    this.previousFileNames.Enqueue(fileName);
                }
            }

            this.WriteToFile(fileName, logEvent, bytesToWrite, false);

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
        /// <returns>File name with the value of <paramref name="value"/> in the position of the numeric pattern.</returns>
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
                    ProcessLogEvent(firstLogEvent, currentFileName, ms.ToArray());
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
        /// <remarks>Occasionally, this method can identify the existence of the {#} pattern incorrectly.</remarks>
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
        private void RollArchivesForward(string fileName, string pattern, int archiveNumber)
        {
            if (ShouldDeleteOldArchives() && archiveNumber >= this.MaxArchiveFiles)
            {
                File.Delete(fileName);
                return;
            }

            if (!File.Exists(fileName))
            {
                return;
            }

            string newFileName = ReplaceNumberPattern(pattern, archiveNumber);
            RollArchivesForward(newFileName, pattern, archiveNumber + 1);

            if (archiveNumber == 0)
                ArchiveFile(fileName, newFileName);
            else
                RollArchiveForward(fileName, newFileName);
        }

        /// <summary>
        /// Moves the archive file to the specified file name.
        /// </summary>
        /// <param name="existingFileName">The archive file to move.</param>
        /// <param name="newFileName">The destination file name.</param>
        private void RollArchiveForward(string existingFileName, string newFileName)
        {
            InternalLogger.Info("Roll archive {0} to {1}", existingFileName, newFileName);
            File.Move(existingFileName, newFileName);
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

            if (minNumber != -1 && ShouldDeleteOldArchives())
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
            ArchiveFile(fileName, newFileName);
        }

        /// <summary>
        /// Archives fileName to archiveFileName.
        /// </summary>
        /// <param name="fileName">File name to be archived.</param>
        /// <param name="archiveFileName">Name of the archive file.</param>
        private void ArchiveFile(string fileName, string archiveFileName)
        {
            UninitializeFile(fileName);

            string archiveFolderPath = Path.GetDirectoryName(archiveFileName);
            if (!Directory.Exists(archiveFolderPath))
                Directory.CreateDirectory(archiveFolderPath);

            if (EnableArchiveFileCompression)
            {
                InternalLogger.Info("Archiving {0} to compressed {1}", fileName, archiveFileName);
                FileCompressor.CompressFile(fileName, archiveFileName);
                DeleteAndWaitForFileDelete(fileName);
            }
            else
            {
                InternalLogger.Info("Archiving {0} to {1}", fileName, archiveFileName);
                File.Move(fileName, archiveFileName);
            }
        }

        private static void DeleteAndWaitForFileDelete(string fileName)
        {
            var originalFileCreationTime = (new FileInfo(fileName)).CreationTime;
            File.Delete(fileName);

            if (File.Exists(fileName))
            {
                FileInfo currentFileInfo;
                do
                {
                    Thread.Sleep(100);
                    currentFileInfo = new FileInfo(fileName);
                }
                while ((currentFileInfo.Exists) && (currentFileInfo.CreationTime == originalFileCreationTime));
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
            string dateFormat = GetArchiveDateFormatString(this.ArchiveDateFormat);

            string dirName = Path.GetDirectoryName(Path.GetFullPath(pattern));
            if (string.IsNullOrEmpty(dirName))
            {
                return;
            }

            int minSequenceLength = fileTemplate.EndAt - fileTemplate.BeginAt - 2;
            int nextSequenceNumber;
            DateTime archiveDate = GetArchiveDate(fileName, logEvent);
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

            ArchiveFile(fileName, archiveFileName);
            archiveFileNames.Add(archiveFileName);
            EnsureArchiveCount(archiveFileNames);
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
            if (!ShouldDeleteOldArchives())
            {
                return;
            }

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
        /// <param name="fileNameMask">Pattern which the files will be searched against.</param>
        /// <returns>List of files matching the pattern.</returns>
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
        /// <param name="logEvent">Log event that the <see cref="FileTarget"/> instance is currently processing.</param>
        private void ArchiveByDate(string fileName, string pattern, LogEventInfo logEvent)
        {
            string fileNameMask = ReplaceFileNamePattern(pattern, "*");
            string dirName = Path.GetDirectoryName(Path.GetFullPath(pattern));
            string dateFormat = GetArchiveDateFormatString(this.ArchiveDateFormat);

            DateTime archiveDate = GetArchiveDate(fileName, logEvent);
            if (dirName != null)
            {
                string archiveFileName = Path.Combine(dirName, fileNameMask.Replace("*", archiveDate.ToString(dateFormat)));
                ArchiveFile(fileName, archiveFileName);
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
            if (!ShouldDeleteOldArchives())
            {
                return;
            }

            string fileNameMask = ReplaceFileNamePattern(pattern, "*");
            string dirName = Path.GetDirectoryName(Path.GetFullPath(pattern));
            string dateFormat = GetArchiveDateFormatString(this.ArchiveDateFormat);

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
                    int lastIndexOfStar = fileNameMask.LastIndexOf('*');

                    if (lastIndexOfStar + dateFormat.Length <= archiveFileName.Length)
                    {
                        string datePart = archiveFileName.Substring(lastIndexOfStar, dateFormat.Length);
                        DateTime fileDate = DateTime.MinValue;
                        if (DateTime.TryParseExact(datePart, dateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out fileDate))
                        {
                            filesByDate.Add(nextFile);
                        }
                    }
                }

                EnsureArchiveCount(filesByDate);
            }
        }
#endif

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
                    default: formatString = "yyyyMMdd"; break;
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
            if (!previousLogEventTimestamp.HasValue)
                return false;

            string formatString = GetArchiveDateFormatString(string.Empty);
            string lastWriteTimeString = lastWrite.ToString(formatString, CultureInfo.InvariantCulture);
            string logEventTimeString = logEvent.TimeStamp.ToString(formatString, CultureInfo.InvariantCulture);

            if (lastWriteTimeString != logEventTimeString)
                return false;

            DateTime periodAfterPreviousLogEventTime;
            switch (this.ArchiveEvery)
            {
                case FileArchivePeriod.Year: periodAfterPreviousLogEventTime = previousLogEventTimestamp.Value.AddYears(1); break;
                case FileArchivePeriod.Month: periodAfterPreviousLogEventTime = previousLogEventTimestamp.Value.AddMonths(1); break;
                case FileArchivePeriod.Day: periodAfterPreviousLogEventTime = previousLogEventTimestamp.Value.AddDays(1); break;
                case FileArchivePeriod.Hour: periodAfterPreviousLogEventTime = previousLogEventTimestamp.Value.AddHours(1); break;
                case FileArchivePeriod.Minute: periodAfterPreviousLogEventTime = previousLogEventTimestamp.Value.AddMinutes(1); break;
                default: return false;
            }

            string periodAfterPreviousLogEventTimeString = periodAfterPreviousLogEventTime.ToString(formatString, CultureInfo.InvariantCulture);
            return lastWriteTimeString == periodAfterPreviousLogEventTimeString;
        }

        /// <summary>
        /// Invokes the archiving process after determining when and which type of archiving is required.
        /// </summary>
        /// <param name="fileName">File name to be checked and archived.</param>
        /// <param name="eventInfo">Log event that the <see cref="FileTarget"/> instance is currently processing.</param>
        private void DoAutoArchive(string fileName, LogEventInfo eventInfo)
        {
            var fileInfo = new FileInfo(fileName);
            if (!fileInfo.Exists)
            {
                return;
            }

            string fileNamePattern = GetArchiveFileNamePattern(fileName, eventInfo);

            if (fileNamePattern == null)
            {
                InternalLogger.Warn("Skip auto archive because fileName is NULL");
                return;
            }

            if (!ContainsFileNamePattern(fileNamePattern))
            {
                if (fileArchive.Archive(fileNamePattern, fileInfo.FullName, CreateDirs))
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
                        this.RollArchivesForward(fileInfo.FullName, fileNamePattern, 0);
                        break;

                    case ArchiveNumberingMode.Sequence:
                        this.ArchiveBySequence(fileInfo.FullName, fileNamePattern);
                        break;

#if !NET_CF
                    case ArchiveNumberingMode.Date:
                        this.ArchiveByDate(fileInfo.FullName, fileNamePattern, eventInfo);
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
            if (this.fullarchiveFileName == null)
            {
                string ext = EnableArchiveFileCompression ? ".zip" : Path.GetExtension(fileName);
                return Path.ChangeExtension(fileName, ".{#}" + ext);
            }
            else
            {
                //The archive file name is given. There are two possibilities
                //(1) User supplied the Filename with pattern
                //(2) User supplied the normal filename
                string archiveFileName = this.fullarchiveFileName.Render(eventInfo);
                return archiveFileName;
            }
        }

        /// <summary>
        /// Determine if old archive files should be deleted.
        /// </summary>
        /// <returns><see langword="true"/> when old archives should be deleted; <see langword="false"/> otherwise.</returns>
        private bool ShouldDeleteOldArchives()
        {
            return MaxArchiveFiles > 0;
        }

        /// <summary>
        /// Indicates if the automatic archiving process should be executed.
        /// </summary>
        /// <param name="fileName">File name to be written.</param>
        /// <param name="ev">Log event that the <see cref="FileTarget"/> instance is currently processing.</param>
        /// <param name="upcomingWriteSize">The size in bytes of the next chunk of data to be written in the file.</param>
        /// <returns>Filename to archive. If <c>null</c>, then nothing to archive.</returns>
        private string GetAutoArchiveFileName(string fileName, LogEventInfo ev, int upcomingWriteSize)
        {
            var hasFileName = !(fileName == null && previousLogFileName == null);
            if (hasFileName)
            {
                return DirectorySeparatorCharForFileSize(fileName, upcomingWriteSize) ??
                       DirectorySeparatorCharForTime(fileName, ev);
            }

            return null;
        }

        /// <summary>
        /// Returns the correct filename to archive
        /// </summary>
        /// <returns></returns>
        private string GetPotentialFileForArchiving(string fileName)
        {
            if (fileName == previousLogFileName)
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
        /// Indicates if the automatic archiving process should be executed based on file size constrains.
        /// </summary>
        /// <param name="fileName">File name to be written.</param>
        /// <param name="upcomingWriteSize">The size in bytes of the next chunk of data to be written in the file.</param>
        /// <returns>Filename to archive. If <c>null</c>, then nothing to archive.</returns>
        private string DirectorySeparatorCharForFileSize(string fileName, int upcomingWriteSize)
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
        /// Indicates if the automatic archiving process should be executed based on date/time constrains.
        /// </summary>
        /// <param name="fileName">File name to be written.</param>
        /// <param name="logEvent">Log event that the <see cref="FileTarget"/> instance is currently processing.</param>
        /// <returns>Filename to archive. If <c>null</c>, then nothing to archive.</returns>
        private string DirectorySeparatorCharForTime(string fileName, LogEventInfo logEvent)
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

            var creationTimeUtc = this.fileAppenderCache.GetFileCreationTimeUtc(fileName, true);
            if (creationTimeUtc == null)
            {
                return null;
            }

            // file creation time is in Utc and logEvent's timestamp is originated from TimeSource.Current,
            // so we should ask the TimeSource to convert file time to TimeSource time:

            DateTime creationTime = TimeSource.Current.FromSystemTime(creationTimeUtc.Value);
            string formatString = GetArchiveDateFormatString(string.Empty);
            string fileCreated = creationTime.ToString(formatString, CultureInfo.InvariantCulture);
            string logEventRecorded = logEvent.TimeStamp.ToString(formatString, CultureInfo.InvariantCulture);

            var shouldArchive = fileCreated != logEventRecorded;
            if (shouldArchive)
            {
                return fileName;
            }
            return null;
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
                    this.fileAppenderCache.CloseAppenders(expireTime);
                }
                catch (Exception exception)
                {
                    InternalLogger.Warn(exception, "Exception in AutoClosingTimerCallback.");

                    if (exception.MustBeRethrown())
                    {
                        throw;
                    }
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
                ReplaceFileContent(fileName, bytes, true);
                return;
            }

            bool writeHeader = InitializeFile(fileName, logEvent, justData);
            BaseFileAppender appender = this.fileAppenderCache.AllocateAppender(fileName);

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
        /// Writes the file footer and uninitialises the file in <see cref="FileTarget"/> instance internal structures.
        /// </summary>
        /// <param name="fileName">File name to close.</param>
        private void UninitializeFile(string fileName)
        {
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
            byte[] footerBytes = this.GetFooterBytes();
            if (footerBytes != null)
            {
                if (File.Exists(fileName))
                {
                    this.WriteToFile(fileName, null, footerBytes, true);
                }
            }
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
                    InternalLogger.Warn(exception, "Unable to archive old log file '{0}'.", fileName);

                    if (exception.MustBeRethrown())
                    {
                        throw;
                    }
                }
            }

            if (this.DeleteOldFileOnStartup)
            {
                try
                {
                    File.Delete(fileName);
                }
                catch (DirectoryNotFoundException exception)
                {
                    //never rethrow this, as this isn't an exceptional case.
                    InternalLogger.Debug(exception, "Unable to delete old log file '{0}' as directory is missing.", fileName);
                }

                catch (Exception exception)
                {
                    InternalLogger.Warn(exception, "Unable to delete old log file '{0}'.", fileName);

                    if (exception.MustBeRethrown())
                    {
                        throw;
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
        private void ReplaceFileContent(string fileName, byte[] bytes, bool firstAttempt)
        {
            try
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
                byte[] headerBytes = this.GetHeaderBytes();
                if (headerBytes != null)
                {
                    appender.Write(headerBytes);
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
        private byte[] GetLayoutBytes(Layout layout)
        {
            if (layout == null)
            {
                return null;
            }
            //todo remove 
            string renderedText = layout.Render(LogEventInfo.CreateNullEvent()) + this.NewLineChars;
            return this.TransformBytes(this.Encoding.GetBytes(renderedText));
        }


        private class DynamicFileArchive
        {
            private readonly Queue<string> archiveFileQueue = new Queue<string>();
            private readonly FileTarget fileTarget;

            /// <summary>
            /// Creates an instance of <see cref="DynamicFileArchive"/> class.
            /// </summary>
            /// <param name="fileTarget">The file target instance whose files to archive.</param>
            /// <param name="maxArchivedFiles">Maximum number of archive files to be kept.</param>
            public DynamicFileArchive(FileTarget fileTarget, int maxArchivedFiles)
            {
                this.fileTarget = fileTarget;
                this.MaxArchiveFileToKeep = maxArchivedFiles;
            }

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
            /// <returns><see langword="true"/> if the file has been moved successfully; <see langword="false"/> otherwise.</returns>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
            public bool Archive(string archiveFileName, string fileName, bool createDirectory)
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
                AddToArchive(archiveFileName, fileName, createDirectory);
                return true;
            }

            /// <summary>
            /// Archives the file, either by copying it to a new file system location or by compressing it, and add the file name into the list of archives.
            /// </summary>
            /// <param name="archiveFileName">Target file name.</param>
            /// <param name="fileName">Original file name.</param>
            /// <param name="createDirectory">Create a directory, if it does not exist.</param>
            private void AddToArchive(string archiveFileName, string fileName, bool createDirectory)
            {
                if (archiveFileQueue.Count != 0)
                    archiveFileName = GetNextArchiveFileName(archiveFileName);

                try
                {
                    fileTarget.ArchiveFile(fileName, archiveFileName);
                    archiveFileQueue.Enqueue(archiveFileName);
                }
                catch (Exception ex)
                {
                    InternalLogger.Error(ex, "Cannot archive file '{0}'.", fileName);
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
                        InternalLogger.Warn(ex, "Cannot delete old archive file : '{0}'.", archiveFileName);
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
                        InternalLogger.Warn(ex, "Cannot delete old archive file : '{0}'.", oldestArchivedFileName);
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