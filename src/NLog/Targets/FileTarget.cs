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
    using FileTargetArchival;

    /// <summary>
    /// Writes log messages to one or more files.
    /// </summary>
    /// <seealso href="https://github.com/nlog/nlog/wiki/File-target">Documentation on NLog Wiki</seealso>
    [Target("File")]
    public class FileTarget : TargetWithLayoutHeaderAndFooter, ICreateFileParameters, IArchivalOptions, IArchivalCallbacks
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
        /// Holds the initialised files each given time by the <see cref="FileTarget"/> instance. Against each file, the last write time is stored. 
        /// </summary>
        /// <remarks>Last write time is store in local time (no UTC).</remarks>
        private readonly Dictionary<string, DateTime> initializedFiles = new Dictionary<string, DateTime>();

        private LineEndingMode lineEndingMode = LineEndingMode.Default;

        /// <summary>
        /// Provides log file archival functionality.
        /// </summary>
        private Archival archival;

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
        /// Initializes a new instance of the <see cref="FileTarget" /> class.
        /// </summary>
        /// <remarks>
        /// The default value of the layout is: <code>${longdate}|${level:uppercase=true}|${logger}|${message}</code>
        /// </remarks>
        public FileTarget()
        {
            this.ArchiveNumbering = ArchiveNumberingMode.Sequence;
            this.ConcurrentWriteAttemptDelay = 1;
            this.ArchiveEvery = FileArchivePeriod.None;
            this.ArchiveAboveSize = Archival.ArchiveAboveSizeDisabled;
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
            this.ArchiveDateFormat = string.Empty;

            this.maxLogFilenames = 20;
            this.recentAppenders = FileAppenderCache.Empty;

            this.archival = new Archival(this, this);
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

        int IArchivalOptions.MaxLogFilenames { get { return maxLogFilenames; } } // NLog5: todo remove and use renamed property

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
        public Layout ArchiveFileName { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of archive files that should be kept.
        /// </summary>
        /// <docgen category='Archival Options' order='10' />
        [DefaultValue(0)]
        public int MaxArchiveFiles { get; set; }

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

        bool IArchivalOptions.EnableArchiveFileCompression { get { return EnableArchiveFileCompression; } }
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

        /// <summary>
        /// Removes records of initialized files that have not been 
        /// accessed in the last two days.
        /// </summary>
        /// <remarks>
        /// Files are marked 'initialized' for the purpose of writing footers when the logging finishes.
        /// </remarks>
        public void CleanupInitializedFiles()
        {
            this.CleanupInitializedFiles(DateTime.Now.AddDays(-FileTarget.InitializedFilesCleanupPeriod));
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
                    // TODO: document why this difference matters
                    if (archival.Enabled)
                    {
                        return CountingSingleProcessFileAppender.TheFactory;
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
            string fileName = CleanupInvalidFileNameChars(this.FileName.Render(logEvent));

            byte[] bytes = this.GetBytesToWrite(logEvent);

            if (archival.ShouldAutoArchive(fileName, logEvent, bytes.Length))
            {
                this.recentAppenders.InvalidateAppender(fileName);
                archival.DoAutoArchive(fileName, logEvent);
            }

            // Offer to clean up old archives if this is the first time a log record is being written
            archival.CheckToDeleteOldArchives(fileName, logEvent);

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

        private void FlushCurrentFileWrites(string currentFileName, LogEventInfo firstLogEvent, MemoryStream ms, List<AsyncContinuation> pendingContinuations)
        {
            Exception lastException = null;

            try
            {
                if (currentFileName != null)
                {
                    if (archival.ShouldAutoArchive(currentFileName, firstLogEvent, (int)ms.Length))
                    {
                        this.WriteFooterAndUninitialize(currentFileName);
                        this.recentAppenders.InvalidateAppender(currentFileName);
                        archival.DoAutoArchive(currentFileName, firstLogEvent);
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
                if (!this.initializedFiles.ContainsKey(fileName))
                {
                    ProcessOnStartup(fileName, logEvent);

                    this.initializedFiles[fileName] = DateTime.Now;
                    this.initializedFilesCounter++;
                    writeHeader = true;

                    if (this.initializedFilesCounter >= FileTarget.InitializedFilesCounterMax)
                    {
                        this.initializedFilesCounter = 0;
                        this.CleanupInitializedFiles();
                    }
                }

                this.initializedFiles[fileName] = DateTime.Now;
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
                    archival.DoAutoArchive(fileName, logEvent);
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
        /// Removes an item from <c>initializedFiles</c>.
        /// </summary>
        /// <param name="fileNameOrPath">The filename or path to remove.</param>
        void IArchivalCallbacks.RemoveInitializedFileNameOrPath(string fileNameOrPath)
        {
            // force non-null to safely use as dictionary key
            fileNameOrPath = fileNameOrPath ?? string.Empty;

            string fileName = Path.GetFileName(fileNameOrPath);
            if (fileName == null) { return; }

            // When the file has been moved, the original filename is 
            // no longer one of the initializedFiles. The initializedFilesCounter
            // should be left alone, the amount is still valid.
            if (this.initializedFiles.ContainsKey(fileName))
            {
                this.initializedFiles.Remove(fileName);
            }
            else if (this.initializedFiles.ContainsKey(fileNameOrPath))
            {
                this.initializedFiles.Remove(fileNameOrPath);
            }
        }

        /// <summary>
        /// Removes an item from <c>initializedFiles</c>.
        /// </summary>
        /// <param name="fileName">The filename to remove.</param>
        void IArchivalCallbacks.RemoveInitializedFileName(string fileName)
        {
            // force non-null to safely use as dictionary key
            fileName = fileName ?? string.Empty;

            if (this.initializedFiles.ContainsKey(fileName))
            {
                this.initializedFiles.Remove(fileName);
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
        /// Gets info about the given file, from cached data if possible.
        /// </summary>
        /// <param name="filePath">File which the information are requested.</param>
        /// <param name="lastWriteTime">The last time the file has been written is returned.</param>
        /// <param name="fileLength">The length of the file is returned.</param>
        /// <returns><see langword="true"/> when file details returned; <see langword="false"/> otherwise.</returns>
        public bool GetFileInfo(string filePath, out DateTime lastWriteTime, out long fileLength)
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
        public static string CleanupInvalidFileNameChars(string fileName)
        {
#if !SILVERLIGHT

            var lastDirSeparator =
                fileName.LastIndexOfAny(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar });

            var fileName1 = fileName.Substring(lastDirSeparator + 1);
            var dirName = lastDirSeparator > 0 ? fileName.Substring(0, lastDirSeparator) : string.Empty;
            fileName1 = Path.GetInvalidFileNameChars().Aggregate(fileName1, (current, c) => current.Replace(c, '_'));
            return Path.Combine(dirName, fileName1);
#else
            return fileName;
#endif
        }
    }
}