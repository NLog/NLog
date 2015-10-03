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
        // Clean up period, of the initialied files, is defined in days.
        private const int CleanupPeriod = 2;
        private const int MaxInitialisedFilesAllowed = 100;

        private readonly InitializedFiles initializedFiles = new InitializedFiles();

        private LineEndingMode lineEndingMode = LineEndingMode.Default;
        private IFileAppenderFactory appenderFactory;
        private BaseFileAppenderCache recentAppenders; 
        private Timer autoClosingTimer;

        private readonly FileArchiver fileArchiver;

        // Queue used so the oldest used filename can be removed from when the list of filenames
        // that exist have got too long.
        private Queue<string> previousFileNames;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileTarget" /> class.
        /// </summary>
        /// <remarks>
        /// The default value of the layout is: <code>${longdate}|${level:uppercase=true}|${logger}|${message}</code>
        /// </remarks>
        public FileTarget()
        {
            fileArchiver = new FileArchiver(this);
            this.ArchiveNumbering = ArchiveNumberingMode.Sequence;
            this.ConcurrentWriteAttemptDelay = 1;
            this.ArchiveEvery = FileArchivePeriod.None;
            this.ArchiveAboveSize = FileArchiver.ArchiveAboveSizeDisabled;
            this.ArchiveDateFormat = string.Empty;
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
            this.ForceManaged = false;
            this.MaxArchiveFiles = 0;

            this.maxLogFilenames = 20;
            this.previousFileNames = new Queue<string>(this.maxLogFilenames);
            recentAppenders = BaseFileAppenderCache.Empty;
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
        public Layout FileName { get; set; }

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

        /// <summary>
        /// Gets or sets a value specifying the date format to use when archving files.
        /// </summary>
        /// <remarks>
        /// This option works only when the "ArchiveNumbering" parameter is set either to Date or DateAndSequence.
        /// </remarks>
        /// <docgen category='Output Options' order='10' />
        [DefaultValue("")]
        public string ArchiveDateFormat
        {
            get { return fileArchiver.ArchiveDateFormat; }

            set { fileArchiver.ArchiveDateFormat = value; }
        }

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
            get { return fileArchiver.ArchiveAboveSize; }
            set { fileArchiver.ArchiveAboveSize = value; } 
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
            get { return fileArchiver.ArchiveEvery; }
            set { fileArchiver.ArchiveEvery = value; }
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
        public Layout ArchiveFileName { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of archive files that should be kept.
        /// </summary>
        /// <docgen category='Archival Options' order='10' />
        [DefaultValue(0)]
        public int MaxArchiveFiles
        {
            get
            {
                return fileArchiver.Size;
            }
            set
            {
                fileArchiver.Size = value;
            }
        }

        /// <summary>
        /// Gets or set a value indicating whether a managed file stream is forced, instead of used the native implementation.
        /// </summary>
        [DefaultValue(false)]
        public bool ForceManaged { get; set; }

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
        public bool EnableArchiveFileCompression 
        {
            get { return fileArchiver.CompressionEnabled; }
            set { fileArchiver.CompressionEnabled = value; } 
        }
#else
        /// <summary>
        /// Gets or sets a value indicating whether to compress archive files into the zip archive format.
        /// </summary>
        private const bool EnableArchiveFileCompression = false;
#endif

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

        internal InitializedFiles Files
        {
            get { return initializedFiles; }
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
            this.CleanupInitializedFiles(DateTime.Now.AddDays(-CleanupPeriod));
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
            // Uninitialize the files.
            foreach (string fileName in initializedFiles.GetExpired(cleanupThreshold)) {
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

        private IFileAppenderFactory GetFileAppenderFactory()
        {
            if (!this.KeepFileOpen)
            {
                return RetryingMultiProcessFileAppender.TheFactory;
            }
            else
            {
                if (this.ArchiveAboveSize != FileArchiver.ArchiveAboveSizeDisabled || this.ArchiveEvery != FileArchivePeriod.None)
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
            recentAppenders = new BaseFileAppenderCache(OpenFileCacheSize, appenderFactory, this);

            if ((this.OpenFileCacheSize > 0 || this.EnableFileDelete) && this.OpenFileCacheTimeout > 0)
            {
                this.autoClosingTimer = new Timer(
                    this.AutoClosingTimerCallback,
                    null,
                    this.OpenFileCacheTimeout * 1000,
                    this.OpenFileCacheTimeout * 1000);
            }

            // Console.Error.WriteLine("Name: {0} Factory: {1}", this.Name, this.appenderFactory.GetType().FullName);
        }

        /// <summary>
        /// Closes the file(s) opened for writing.
        /// </summary>
        protected override void CloseTarget()
        {
            base.CloseTarget();

            foreach (string fileName in initializedFiles.GetItems())
            {
                this.WriteFooterAndUninitialize(fileName);
            }

            if (this.autoClosingTimer != null)
            {
                this.autoClosingTimer.Change(Timeout.Infinite, Timeout.Infinite);
                this.autoClosingTimer.Dispose();
                this.autoClosingTimer = null;
            }

            recentAppenders.CloseAppenders();
        }

        /// <summary>
        /// Writes the specified logging event to a file specified in the FileName 
        /// parameter.
        /// </summary>
        /// <param name="logEvent">The logging event.</param>
        protected override void Write(LogEventInfo logEvent)
        {
#if !SILVERLIGHT
            string fileName = CleanupInvalidFileNameChars(this.FileName.Render(logEvent));
#else
            string fileName = this.FileName.Render(logEvent);
#endif
            byte[] bytes = this.GetBytesToWrite(logEvent);

            // Clean up old archives if this is the first time a log record has been written to
            // this log file and the archiving system is date/time based.
            if (this.ArchiveNumbering == ArchiveNumberingMode.Date && this.ArchiveEvery != FileArchivePeriod.None)
            {
                if (!previousFileNames.Contains(fileName))
                {
                    if (this.previousFileNames.Count > this.maxLogFilenames)
                    {
                        this.previousFileNames.Dequeue();
                    }

                    string fileNamePattern = this.GetFileNamePattern(fileName, logEvent);
                    fileArchiver.DeleteOldDateArchive(fileNamePattern);
                    this.previousFileNames.Enqueue(fileName);
                }
            }

            if (this.ShouldAutoArchive(fileName, logEvent, bytes.Length))
            {
                recentAppenders.InvalidateAppender(fileName);
                this.DoAutoArchive(fileName, logEvent);
            }

            this.WriteToFile(fileName, logEvent, bytes, false);
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
#if !SILVERLIGHT
                    string fileName = CleanupInvalidFileNameChars(bucket.Key);
#else
                    string fileName = bucket.Key;
#endif

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
                        recentAppenders.InvalidateAppender(currentFileName);
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

        private static bool ContainFileNamePattern(string fileName)
        {
            int startingIndex = fileName.IndexOf("{#", StringComparison.Ordinal);
            int endingIndex = fileName.IndexOf("#}", StringComparison.Ordinal);

            return (startingIndex != -1 && endingIndex != -1 && startingIndex < endingIndex);
        }

        private void DoAutoArchive(string fileName, LogEventInfo eventInfo)
        {
            FileInfo fileInfo = new FileInfo(fileName);
            if (!fileInfo.Exists)
            {
                return;
            }

            // Console.WriteLine("DoAutoArchive({0})", fileName);
            string fileNamePattern = GetFileNamePattern(fileName, eventInfo);

            if (!ContainFileNamePattern(fileNamePattern))
            {
                if (fileArchiver.Archive(fileNamePattern, fileInfo.FullName, CreateDirs))
                {
                    initializedFiles.Remove(fileInfo.FullName);
                }
            }
            else
            {
                switch (this.ArchiveNumbering)
                {
                    case ArchiveNumberingMode.Rolling:
                        fileArchiver.RollingArchive(fileInfo.FullName, fileNamePattern, 0);
                        break;

                    case ArchiveNumberingMode.Sequence:
                        fileArchiver.SequentialArchive(fileInfo.FullName, fileNamePattern);
                        break;

#if !NET_CF
                    case ArchiveNumberingMode.Date:
                        fileArchiver.DateArchive(fileInfo.FullName, fileNamePattern);
                        break;

                    case ArchiveNumberingMode.DateAndSequence:
                        fileArchiver.DateAndSequentialArchive(fileInfo.FullName, fileNamePattern, eventInfo);
                        break;
#endif
                }
            }
        }

        /// <summary>
        /// Gets the pattern that archive files will match
        /// </summary>
        /// <param name="fileName">Filename of the log file</param>
        /// <param name="eventInfo">Log event info of the log that is currently been written</param>
        /// <returns>A string with a pattern that will match the archive filenames</returns>
        private string GetFileNamePattern(string fileName, LogEventInfo eventInfo)
        {
            string fileNamePattern;

            FileInfo fileInfo = new FileInfo(fileName);

            if (this.ArchiveFileName == null)
            {
                string ext = Path.GetExtension(fileName);
                fileNamePattern = Path.ChangeExtension(fileInfo.FullName, ".{#}" + ext);
            }
            else
            {
                //The archive file name is given. There are two possibilities
                //(1) User supplied the Filename with pattern
                //(2) User supplied the normal filename
                fileNamePattern = this.ArchiveFileName.Render(eventInfo);
            }
            return fileNamePattern;
        }

        private bool ShouldAutoArchive(string fileName, LogEventInfo ev, int upcomingWriteSize)
        {
            return fileArchiver.ShouldAutoArchiveBasedOnFileSize(fileName, upcomingWriteSize) ||
                   fileArchiver.ShouldAutoArchiveBasedOnTime(fileName, ev);
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
                    recentAppenders.CloseAppenders(expireTime);
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

        private byte[] GetHeaderBytes()
        {
            return GetLayoutBytes(Header);
        }

        private byte[] GetFooterBytes()
        {
            return GetLayoutBytes(Footer);
        }

        private byte[] GetLayoutBytes(Layout layout)
        {
            if (layout == null)
            {
                return null;
            }

            string renderedText = layout.Render(LogEventInfo.CreateNullEvent()) + this.NewLineChars;
            return this.TransformBytes(this.Encoding.GetBytes(renderedText));
        }

        private void WriteToFile(string fileName, LogEventInfo logEvent, byte[] bytes, bool justData)
        {
            if (this.ReplaceFileContentsOnEachWrite)
            {
                ReplaceFileContent(fileName, bytes);
                return;
            }

            bool writeHeader = InitializeFile(fileName, logEvent, justData);
            BaseFileAppender appender = recentAppenders.AllocateAppender(fileName);

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

        private bool InitializeFile(string fileName, LogEventInfo logEvent, bool justData)
        {
            bool writeHeader = false;

            if (!justData)
            {
                if (!initializedFiles.Contains(fileName)) 
                {
                    ProcessOnStartup(fileName, logEvent);

                    writeHeader = true;
                    initializedFiles.AddOrUpdate(fileName);

                    if (initializedFiles.Count >= MaxInitialisedFilesAllowed) 
                    {
                        CleanupInitializedFiles();
                    }
                }

                initializedFiles.AddOrUpdate(fileName);
            }

            return writeHeader;
        }

        private void WriteFooterAndUninitialize(string fileName)
        {
            WriteFooter(fileName);
            initializedFiles.Remove(fileName);
        }

        private void ProcessOnStartup(string fileName, LogEventInfo logEvent)
        {
            ArchiveOnStartup(fileName, logEvent);
            DeleteOnStartup(fileName);
        }

        private void ArchiveOnStartup(string fileName, LogEventInfo logEvent) 
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
        }

        private void DeleteOnStartup(string fileName)
        {
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

        // HACK: Exposing GetFileInfo() method as internal creates tight coupling between FileTarget and FileArchiver classes. 
        //      Review code when possible.  
        internal bool GetFileInfo(string fileName, out DateTime lastWriteTime, out long fileLength)
        {
            if (recentAppenders.GetFileInfo(fileName, out lastWriteTime, out fileLength))
            {
                return true;
            }

            FileInfo fileInfo = new FileInfo(fileName);
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

#if !SILVERLIGHT
        private static string CleanupInvalidFileNameChars(string fileName)
        {
            var lastDirSeparator =
                fileName.LastIndexOfAny(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar });

            var fileName1 = fileName.Substring(lastDirSeparator + 1);
            var dirName = lastDirSeparator > 0 ? fileName.Substring(0, lastDirSeparator) : string.Empty;
            fileName1 = Path.GetInvalidFileNameChars().Aggregate(fileName1, (current, c) => current.Replace(c, '_'));
            return Path.Combine(dirName, fileName1);
        }
#endif

        internal sealed class InitializedFiles
        {
            // Key = Filename, Value = Insterted Date/Time
            private readonly Dictionary<string, DateTime> initializedFiles = new Dictionary<string, DateTime>();

            public int Count
            {
                get
                {
                    return initializedFiles.Count;
                }
            }

            public void AddOrUpdate(String fileName)
            {
                initializedFiles[fileName] = DateTime.Now;
            }

            public bool Remove(String fileName)
            {
                return initializedFiles.Remove(fileName);
            }

            public bool Contains(String fileName)
            {
                return initializedFiles.ContainsKey(fileName);
            }

            public IEnumerable<String> GetExpired(DateTime cleanupThreshold)
            {
                // Select the files require to be uninitialized.
                foreach (var file in initializedFiles)
                {
                    if (file.Value < cleanupThreshold)
                    {
                        yield return file.Key;
                    }
                }
            }

            public IEnumerable<String> GetItems()
            {
                return new List<String>(initializedFiles.Keys);
            }
        }
    }
}
