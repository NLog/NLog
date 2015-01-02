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

#if !UNITY3D_WEB
using System.Linq;

namespace NLog.Targets
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using System.Threading;
    using Common;
    using Config;
    using Internal;
    using Internal.FileAppenders;
    using Layouts;

    /// <summary>
    /// Writes log messages to one or more files.
    /// </summary>
    /// <seealso href="http://nlog-project.org/wiki/File_target">Documentation on NLog Wiki</seealso>
    [Target("File")]
    public class FileTarget : TargetWithLayoutHeaderAndFooter, ICreateFileParameters
    {
        private readonly Dictionary<string, DateTime> initializedFiles = new Dictionary<string, DateTime>();

        private LineEndingMode lineEndingMode = LineEndingMode.Default;
        private IFileAppenderFactory appenderFactory;
        private BaseFileAppender[] recentAppenders;
        private Timer autoClosingTimer;
        private int initializedFilesCounter;

        private int _MaxArchiveFilesField;

        private readonly DynamicArchiveFileHandlerClass dynamicArchiveFileHandler;

        private class DynamicArchiveFileHandlerClass
        {
            private readonly Queue<string> archiveFileEntryQueue;

            public DynamicArchiveFileHandlerClass(int MaxArchivedFiles)
                : this()
            {
                this.MaxArchiveFileToKeep = MaxArchivedFiles;
            }

            public DynamicArchiveFileHandlerClass()
            {
                this.MaxArchiveFileToKeep = -1;

                archiveFileEntryQueue = new Queue<string>();
            }

            public int MaxArchiveFileToKeep { get; set; }

            /// <returns><c>true</c> if the file has been moved successfully</returns>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
            public bool AddToArchive(string archiveFileName, string fileName, bool createDirectoryIfNotExists)
            {

                if (MaxArchiveFileToKeep < 1)
                {
                    InternalLogger.Warn("AddToArchive is called. Even though the MaxArchiveFiles is set to less than 1");

                    return false;
                }

                if (!File.Exists(fileName))
                {
                    InternalLogger.Error("Error while trying to archive, Source File : {0} Not found.", fileName);

                    return false;
                }

                while (archiveFileEntryQueue.Count >= MaxArchiveFileToKeep)
                {
                    string oldestArchivedFileName = archiveFileEntryQueue.Dequeue();

                    try
                    {
                        File.Delete(oldestArchivedFileName);
                    }
                    catch (Exception exceptionThrown)
                    {
                        InternalLogger.Warn("Can't Delete Old Archive File : {0} , Exception : {1}", oldestArchivedFileName, exceptionThrown);
                    }
                }


                String archiveFileNamePattern = archiveFileName;

                if (archiveFileEntryQueue.Contains(archiveFileName))
                {
                    InternalLogger.Trace("Archive File {0} seems to be already exist. Trying with Different File Name..", archiveFileName);

                    int NumberToStartWith = 1;

                    archiveFileNamePattern = Path.GetFileNameWithoutExtension(archiveFileName) + ".{#}" + Path.GetExtension(archiveFileName);

                    while (File.Exists(ReplaceNumber(archiveFileNamePattern, NumberToStartWith)))
                    {
                        InternalLogger.Trace("Archive File {0} seems to be already exist, too. Trying with Different File Name..", archiveFileName);
                        NumberToStartWith++;
                    }

                }

                try
                {
                    File.Move(fileName, archiveFileNamePattern);
                }
                catch (DirectoryNotFoundException)
                {
                    if (createDirectoryIfNotExists)
                    {
                        InternalLogger.Trace("Directory For Archive File is not created. Creating it..");

                        try
                        {
                            Directory.CreateDirectory(Path.GetDirectoryName(archiveFileName));

                            File.Move(fileName, archiveFileNamePattern);
                        }
                        catch (Exception ExceptionThrown)
                        {
                            InternalLogger.Error("Can't create Archive File Directory , Exception : {0}", ExceptionThrown);
                            throw;
                        }
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (Exception ExceptionThrown)
                {
                    InternalLogger.Error("Can't Archive File : {0} , Exception : {1}", fileName, ExceptionThrown);

                    throw;
                }

                archiveFileEntryQueue.Enqueue(archiveFileName);
                return true;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileTarget" /> class.
        /// </summary>
        /// <remarks>
        /// The default value of the layout is: <code>${longdate}|${level:uppercase=true}|${logger}|${message}</code>
        /// </remarks>
        public FileTarget()
        {
            this.ArchiveNumbering = ArchiveNumberingMode.Sequence;
            this._MaxArchiveFilesField = 9;
            this.ConcurrentWriteAttemptDelay = 1;
            this.ArchiveEvery = FileArchivePeriod.None;
            this.ArchiveAboveSize = -1;
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
            this.NewLineChars = EnvironmentHelper.NewLine;
            this.EnableFileDelete = true;
            this.OpenFileCacheTimeout = -1;
            this.OpenFileCacheSize = 5;
            this.CreateDirs = true;
            this.dynamicArchiveFileHandler = new DynamicArchiveFileHandlerClass(MaxArchiveFiles);
            this.ForceManaged = false;
            this.ArchiveDateFormat = string.Empty;
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
        /// Gets or sets a value indicating whether to create directories if they don't exist.
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
        /// Gets or sets a value indicating whether to enable log file(s) to be deleted.
        /// </summary>
        /// <docgen category='Output Options' order='10' />
        [DefaultValue(true)]
        public bool EnableFileDelete { get; set; }

        /// <summary>
        /// Gets or sets a value specifying the date format to use when archving files.
        /// </summary>
        /// <remarks>
        /// This option works only when the "ArchiveNumbering" parameter is set to Date.
        /// </remarks>
        /// <docgen category='Output Options' order='10' />
        [DefaultValue("")]
        public string ArchiveDateFormat { get; set; }

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
                switch (value)
                {
                    case LineEndingMode.CR:
                        this.NewLineChars = "\r";
                        break;

                    case LineEndingMode.LF:
                        this.NewLineChars = "\n";
                        break;

                    case LineEndingMode.CRLF:
                        this.NewLineChars = "\r\n";
                        break;

                    case LineEndingMode.Default:
                        this.NewLineChars = EnvironmentHelper.NewLine;
                        break;

                    case LineEndingMode.None:
                        this.NewLineChars = string.Empty;
                        break;
                }
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
        [DefaultValue(9)]
        public int MaxArchiveFiles
        {
            get
            {
                return _MaxArchiveFilesField;
            }
            set
            {
                _MaxArchiveFilesField = value;

                dynamicArchiveFileHandler.MaxArchiveFileToKeep = value;
            }
        }

        /// <summary>
        /// Gets ors set a value indicating whether a managed file stream is forced, instead of used the native implementation.
        /// </summary>
        [DefaultValue(false)]
        public bool ForceManaged { get; set; }

        /// <summary>
        /// Gets or sets the way file archives are numbered. 
        /// </summary>
        /// <docgen category='Archival Options' order='10' />
        public ArchiveNumberingMode ArchiveNumbering { get; set; }

        /// <summary>
        /// Gets the characters that are appended after each line.
        /// </summary>
        protected internal string NewLineChars { get; private set; }

        /// <summary>
        /// Removes records of initialized files that have not been 
        /// accessed in the last two days.
        /// </summary>
        /// <remarks>
        /// Files are marked 'initialized' for the purpose of writing footers when the logging finishes.
        /// </remarks>
        public void CleanupInitializedFiles()
        {
            this.CleanupInitializedFiles(DateTime.Now.AddDays(-2));
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
            // clean up files that are two days old
            var filesToUninitialize = new List<string>();

            foreach (var de in this.initializedFiles)
            {
                string fileName = de.Key;
                DateTime lastWriteTime = de.Value;
                if (lastWriteTime < cleanupThreshold)
                {
                    filesToUninitialize.Add(fileName);
                }
            }

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
                foreach (BaseFileAppender t in this.recentAppenders)
                {
                    if (t == null)
                    {
                        break;
                    }

                    t.Flush();
                }

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
        /// Initializes file logging by creating data structures that
        /// enable efficient multi-file logging.
        /// </summary>
        protected override void InitializeTarget()
        {
            base.InitializeTarget();

            if (!this.KeepFileOpen)
            {
                this.appenderFactory = RetryingMultiProcessFileAppender.TheFactory;
            }
            else
            {
                if (this.ArchiveAboveSize != -1 || this.ArchiveEvery != FileArchivePeriod.None)
                {
                    if (this.NetworkWrites)
                    {
                        this.appenderFactory = RetryingMultiProcessFileAppender.TheFactory;
                    }
                    else if (this.ConcurrentWrites)
                    {
#if SILVERLIGHT
                        this.appenderFactory = RetryingMultiProcessFileAppender.TheFactory;
#elif MONO
                        //
                        // mono on Windows uses mutexes, on Unix - special appender
                        //
                        if (PlatformDetector.IsUnix)
                        {
                            this.appenderFactory = UnixMultiProcessFileAppender.TheFactory;
                        }
                        else
                        {
                            this.appenderFactory = MutexMultiProcessFileAppender.TheFactory;
                        }
#else
                        this.appenderFactory = MutexMultiProcessFileAppender.TheFactory;
#endif
                    }
                    else
                    {
                        this.appenderFactory = CountingSingleProcessFileAppender.TheFactory;
                    }
                }
                else
                {
                    if (this.NetworkWrites)
                    {
                        this.appenderFactory = RetryingMultiProcessFileAppender.TheFactory;
                    }
                    else if (this.ConcurrentWrites)
                    {
#if SILVERLIGHT
                        this.appenderFactory = RetryingMultiProcessFileAppender.TheFactory;
#elif MONO
                        //
                        // mono on Windows uses mutexes, on Unix - special appender
                        //
                        if (PlatformDetector.IsUnix)
                        {
                            this.appenderFactory = UnixMultiProcessFileAppender.TheFactory;
                        }
                        else
                        {
                            this.appenderFactory = MutexMultiProcessFileAppender.TheFactory;
                        }
#else
                        this.appenderFactory = MutexMultiProcessFileAppender.TheFactory;
#endif
                    }
                    else
                    {
                        this.appenderFactory = SingleProcessFileAppender.TheFactory;
                    }
                }
            }

            this.recentAppenders = new BaseFileAppender[this.OpenFileCacheSize];

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

            if (this.recentAppenders != null)
            {
                for (int i = 0; i < this.recentAppenders.Length; ++i)
                {
                    if (this.recentAppenders[i] == null)
                    {
                        break;
                    }

                    this.recentAppenders[i].Close();
                    this.recentAppenders[i] = null;
                }
            }
        }

        /// <summary>
        /// Writes the specified logging event to a file specified in the FileName 
        /// parameter.
        /// </summary>
        /// <param name="logEvent">The logging event.</param>
        protected override void Write(LogEventInfo logEvent)
        {
#if !SILVERLIGHT
            string fileName = CleanupFileName(this.FileName.Render(logEvent));
#else
            string fileName = this.FileName.Render(logEvent);
#endif
            byte[] bytes = this.GetBytesToWrite(logEvent);

            if (this.ShouldAutoArchive(fileName, logEvent, bytes.Length))
            {
                this.InvalidateCacheItem(fileName);
                this.DoAutoArchive(fileName, logEvent);
            }

            this.WriteToFile(fileName, bytes, false);
        }

        /// <summary>
        /// Writes the specified array of logging events to a file specified in the FileName
        /// parameter.
        /// </summary>
        /// <param name="logEvents">An array of <see cref="LogEventInfo "/> objects.</param>
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
                    string fileName = CleanupFileName(bucket.Key);
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

        private static Boolean IsContainValidNumberPatternForReplacement(string pattern)
        {
            int StartingIndex = pattern.IndexOf("{#", StringComparison.Ordinal);
            int EndingIndex = pattern.IndexOf("#}", StringComparison.Ordinal);

            return (StartingIndex != -1 && EndingIndex != -1 && StartingIndex < EndingIndex);
        }

        private static string ReplaceNumber(string pattern, int value)
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
                        this.InvalidateCacheItem(currentFileName);
                        this.DoAutoArchive(currentFileName, firstLogEvent);
                    }

                    this.WriteToFile(currentFileName, ms.ToArray(), false);
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

        private void RecursiveRollingRename(string fileName, string pattern, int archiveNumber)
        {
            if (archiveNumber >= this.MaxArchiveFiles)
            {
                File.Delete(fileName);
                return;
            }

            if (!File.Exists(fileName))
            {
                return;
            }

            string newFileName = ReplaceNumber(pattern, archiveNumber);
            if (File.Exists(fileName))
            {
                this.RecursiveRollingRename(newFileName, pattern, archiveNumber + 1);
            }

            InternalLogger.Trace("Renaming {0} to {1}", fileName, newFileName);

            try
            {
                MoveFileToArchive(fileName, newFileName);
            }
            catch (IOException)
            {
                string dir = Path.GetDirectoryName(newFileName);
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                MoveFileToArchive(fileName, newFileName);
            }
        }

        private void SequentialArchive(string fileName, string pattern)
        {
            string baseNamePattern = Path.GetFileName(pattern);

            int firstPart = baseNamePattern.IndexOf("{#", StringComparison.Ordinal);
            int lastPart = baseNamePattern.IndexOf("#}", StringComparison.Ordinal) + 2;
            int trailerLength = baseNamePattern.Length - lastPart;

            string fileNameMask = baseNamePattern.Substring(0, firstPart) + "*" + baseNamePattern.Substring(lastPart);

            string dirName = Path.GetDirectoryName(Path.GetFullPath(pattern));
            int nextNumber = -1;
            int minNumber = -1;

            var number2name = new Dictionary<int, string>();

            try
            {
#if SILVERLIGHT
                foreach (string s in Directory.EnumerateFiles(dirName, fileNameMask))
#else
                foreach (string s in Directory.GetFiles(dirName, fileNameMask))
#endif
                {
                    string baseName = Path.GetFileName(s);
                    string number = baseName.Substring(firstPart, baseName.Length - trailerLength - firstPart);
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

                    number2name[num] = s;
                }

                nextNumber++;
            }
            catch (DirectoryNotFoundException)
            {
                Directory.CreateDirectory(dirName);
                nextNumber = 0;
            }

            if (minNumber != -1)
            {
                int minNumberToKeep = nextNumber - this.MaxArchiveFiles + 1;
                for (int i = minNumber; i < minNumberToKeep; ++i)
                {
                    string s;

                    if (number2name.TryGetValue(i, out s))
                    {
                        File.Delete(s);
                    }
                }
            }

            string newFileName = ReplaceNumber(pattern, nextNumber);
            MoveFileToArchive(fileName, newFileName);
        }

        private void MoveFileToArchive(string existingFileName, string archiveFileName)
        {
            File.Move(existingFileName, archiveFileName);
            var fileName = Path.GetFileName(existingFileName);
            if (fileName == null) return;
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
        private void DateAndSequentialArchive(string fileName, string pattern, LogEventInfo logEvent)
        {
            string baseNamePattern = Path.GetFileName(pattern);

            if (string.IsNullOrEmpty(baseNamePattern))
            {
                return;
            }

            int placeholderFirstPart = baseNamePattern.IndexOf("{#", StringComparison.Ordinal);
            int placeholderLastPart = baseNamePattern.IndexOf("#}", StringComparison.Ordinal) + 2;
            int dateTrailerLength = baseNamePattern.Length - placeholderLastPart;

            string fileNameMask = baseNamePattern.Substring(0, placeholderFirstPart) + "*" + baseNamePattern.Substring(placeholderLastPart);
            string dateFormat = GetDateFormatString(this.ArchiveDateFormat);

            string dirName = Path.GetDirectoryName(Path.GetFullPath(pattern));

            if (string.IsNullOrEmpty(dirName))
            {
                return;
            }

            bool isDaySwitch = false;

            DateTime lastWriteTime;
            long fileLength;
            if (this.GetFileInfo(fileName, out lastWriteTime, out fileLength))
            {
                string formatString = GetDateFormatString(string.Empty);
                string ts = lastWriteTime.ToString(formatString, CultureInfo.InvariantCulture);
                string ts2 = logEvent.TimeStamp.ToLocalTime().ToString(formatString, CultureInfo.InvariantCulture);

                isDaySwitch = ts != ts2;
            }

            int nextSequenceNumber = -1;

            try
            {
                var directoryInfo = new DirectoryInfo(dirName);
#if SILVERLIGHT
                List<string> files = directoryInfo.EnumerateFiles(fileNameMask).OrderBy(n => n.CreationTime).Select(n => n.FullName).ToList();
#else
                List<string> files = directoryInfo.GetFiles(fileNameMask).OrderBy(n => n.CreationTime).Select(n => n.FullName).ToList();
#endif

                var filesByDate = new List<string>();
                for (int index = 0; index < files.Count; index++)
                {
                    string archiveFileName = Path.GetFileName(files[index]);

                    if (string.IsNullOrEmpty(archiveFileName))
                    {
                        continue;
                    }

                    string datePart = archiveFileName.Substring(fileNameMask.LastIndexOf('*'), dateFormat.Length);
                    string numberPart = archiveFileName.Substring(fileNameMask.LastIndexOf('*') + dateFormat.Length + 1,
                        archiveFileName.Length - dateTrailerLength - (fileNameMask.LastIndexOf('*') + dateFormat.Length + 1));

                    int num;

                    try
                    {
                        num = Convert.ToInt32(numberPart, CultureInfo.InvariantCulture);
                    }
                    catch (FormatException)
                    {
                        continue;
                    }

                    if (datePart == GetArchiveDate(isDaySwitch).ToString(dateFormat))
                    {
                        nextSequenceNumber = Math.Max(nextSequenceNumber, num);
                    }

                    DateTime fileDate;

                    if (DateTime.TryParseExact(datePart, dateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None,
                        out fileDate))
                    {
                        filesByDate.Add(files[index]);
                    }
                }

                nextSequenceNumber++;

                // Cleanup archive files
                for (int fileIndex = 0; fileIndex < filesByDate.Count; fileIndex++)
                {
                    if (fileIndex > files.Count - this.MaxArchiveFiles)
                        break;

                    File.Delete(filesByDate[fileIndex]);
                }
            }
            catch (DirectoryNotFoundException)
            {
                Directory.CreateDirectory(dirName);
                nextSequenceNumber = 0;
            }

            DateTime newFileDate = GetArchiveDate(isDaySwitch);
            string newFileName = Path.Combine(dirName,
                fileNameMask.Replace("*", string.Format("{0}.{1}", newFileDate.ToString(dateFormat), nextSequenceNumber)));

            MoveFileToArchive(fileName, newFileName);
        }

        private void DateArchive(string fileName, string pattern)
        {
            string baseNamePattern = Path.GetFileName(pattern);

            int firstPart = baseNamePattern.IndexOf("{#", StringComparison.Ordinal);
            int lastPart = baseNamePattern.IndexOf("#}", StringComparison.Ordinal) + 2;
            string fileNameMask = baseNamePattern.Substring(0, firstPart) + "*" + baseNamePattern.Substring(lastPart);
            string dirName = Path.GetDirectoryName(Path.GetFullPath(pattern));
            string dateFormat = GetDateFormatString(this.ArchiveDateFormat);

            try
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(dirName);
#if SILVERLIGHT
                List<string> files = directoryInfo.EnumerateFiles(fileNameMask).OrderBy(n => n.CreationTime).Select(n => n.FullName).ToList();
#else
                List<string> files = directoryInfo.GetFiles(fileNameMask).OrderBy(n => n.CreationTime).Select(n => n.FullName).ToList();
#endif
                List<string> filesByDate = new List<string>();

                for (int index = 0; index < files.Count; index++)
                {
                    string archiveFileName = Path.GetFileName(files[index]);
                    string datePart = archiveFileName.Substring(fileNameMask.LastIndexOf('*'), dateFormat.Length);
                    DateTime fileDate = DateTime.MinValue;
                    if (DateTime.TryParseExact(datePart, dateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out fileDate))
                    {
                        filesByDate.Add(files[index]);
                    }
                }

                for (int fileIndex = 0; fileIndex < filesByDate.Count; fileIndex++)
                {
                    if (fileIndex > files.Count - this.MaxArchiveFiles)
                        break;

                    File.Delete(filesByDate[fileIndex]);
                }
            }
            catch (DirectoryNotFoundException)
            {
                Directory.CreateDirectory(dirName);
            }

            DateTime newFileDate = GetArchiveDate(true);
            string newFileName = Path.Combine(dirName, fileNameMask.Replace("*", newFileDate.ToString(dateFormat)));
            MoveFileToArchive(fileName, newFileName);
        }
#endif

        private string GetDateFormatString(string defaultFormat)
        {
            // If archiveDateFormat is not set in the config file, use a default date format string based on the archive period
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
            DateTime archiveDate = DateTime.Now;

            // Because AutoArchive/DateArchive gets called after the FileArchivePeriod condition matches, decrement the archive period by 1
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

        private void DoAutoArchive(string fileName, LogEventInfo ev)
        {
            var fi = new FileInfo(fileName);
            if (!fi.Exists)
            {
                return;
            }

            // Console.WriteLine("DoAutoArchive({0})", fileName);
            string fileNamePattern;

            if (this.ArchiveFileName == null)
            {
                string ext = Path.GetExtension(fileName);
                fileNamePattern = Path.ChangeExtension(fi.FullName, ".{#}" + ext);
            }
            else
            {
                //The archive file name is given. There are two possibiliy 
                //(1) User supplied the Filename with pattern
                //(2) User supplied the normal filename
                fileNamePattern = this.ArchiveFileName.Render(ev);

            }

            if (!IsContainValidNumberPatternForReplacement(fileNamePattern))
            {
                if (dynamicArchiveFileHandler.AddToArchive(fileNamePattern, fi.FullName, CreateDirs))
                    if (this.initializedFiles.ContainsKey(fi.FullName))
                        this.initializedFiles.Remove(fi.FullName);
            }
            else
            {
                switch (this.ArchiveNumbering)
                {
                    case ArchiveNumberingMode.Rolling:
                        this.RecursiveRollingRename(fi.FullName, fileNamePattern, 0);
                        break;

                    case ArchiveNumberingMode.Sequence:
                        this.SequentialArchive(fi.FullName, fileNamePattern);
                        break;

#if !NET_CF
                    case ArchiveNumberingMode.Date:
                        this.DateArchive(fi.FullName, fileNamePattern);
                        break;

                    case ArchiveNumberingMode.DateAndSequence:
                        this.DateAndSequentialArchive(fi.FullName, fileNamePattern, ev);
                        break;
#endif
                }
            }
        }

        private bool ShouldAutoArchive(string fileName, LogEventInfo ev, int upcomingWriteSize)
        {
            if (this.ArchiveAboveSize == -1 && this.ArchiveEvery == FileArchivePeriod.None)
            {
                return false;
            }

            DateTime lastWriteTime;
            long fileLength;

            if (!this.GetFileInfo(fileName, out lastWriteTime, out fileLength))
            {
                return false;
            }

            if (this.ArchiveAboveSize != -1)
            {
                if (fileLength + upcomingWriteSize > this.ArchiveAboveSize)
                {
                    return true;
                }
            }

            if (this.ArchiveEvery != FileArchivePeriod.None)
            {
                string formatString = GetDateFormatString(string.Empty);
                string ts = lastWriteTime.ToString(formatString, CultureInfo.InvariantCulture);
                string ts2 = ev.TimeStamp.ToLocalTime().ToString(formatString, CultureInfo.InvariantCulture);

                if (ts != ts2)
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
                    DateTime timeToKill = DateTime.Now.AddSeconds(-this.OpenFileCacheTimeout);
                    for (int i = 0; i < this.recentAppenders.Length; ++i)
                    {
                        if (this.recentAppenders[i] == null)
                        {
                            break;
                        }

                        if (this.recentAppenders[i].OpenTime < timeToKill)
                        {
                            for (int j = i; j < this.recentAppenders.Length; ++j)
                            {
                                if (this.recentAppenders[j] == null)
                                {
                                    break;
                                }

                                this.recentAppenders[j].Close();
                                this.recentAppenders[j] = null;
                            }

                            break;
                        }
                    }
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

        private void WriteToFile(string fileName, byte[] bytes, bool justData)
        {
            if (this.ReplaceFileContentsOnEachWrite)
            {
                using (FileStream fs = File.Create(fileName))
                {
                    byte[] headerBytes = this.GetHeaderBytes();
                    byte[] footerBytes = this.GetFooterBytes();

                    if (headerBytes != null)
                    {
                        fs.Write(headerBytes, 0, headerBytes.Length);
                    }

                    fs.Write(bytes, 0, bytes.Length);
                    if (footerBytes != null)
                    {
                        fs.Write(footerBytes, 0, footerBytes.Length);
                    }
                }

                return;
            }

            bool writeHeader = false;

            if (!justData)
            {
                if (!this.initializedFiles.ContainsKey(fileName))
                {
                    if (this.ArchiveOldFileOnStartup)
                    {
                        try
                        {
                            this.DoAutoArchive(fileName, null);
                        }
                        catch (Exception exception)
                        {
                            if (exception.MustBeRethrown())
                                throw;
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

                    this.initializedFiles[fileName] = DateTime.Now;
                    this.initializedFilesCounter++;
                    writeHeader = true;

                    if (this.initializedFilesCounter >= 100)
                    {
                        this.initializedFilesCounter = 0;
                        this.CleanupInitializedFiles();
                    }
                }

                this.initializedFiles[fileName] = DateTime.Now;
            }

            //
            // BaseFileAppender.Write is the most expensive operation here
            // so the in-memory data structure doesn't have to be 
            // very sophisticated. It's a table-based LRU, where we move 
            // the used element to become the first one.
            // The number of items is usually very limited so the 
            // performance should be equivalent to the one of the hashtable.
            //
            BaseFileAppender appenderToWrite = null;
            int freeSpot = this.recentAppenders.Length - 1;

            for (int i = 0; i < this.recentAppenders.Length; ++i)
            {
                if (this.recentAppenders[i] == null)
                {
                    freeSpot = i;
                    break;
                }

                if (this.recentAppenders[i].FileName == fileName)
                {
                    // found it, move it to the first place on the list
                    // (MRU)

                    // file open has a chance of failure
                    // if it fails in the constructor, we won't modify any data structures
                    BaseFileAppender app = this.recentAppenders[i];
                    for (int j = i; j > 0; --j)
                    {
                        this.recentAppenders[j] = this.recentAppenders[j - 1];
                    }

                    this.recentAppenders[0] = app;
                    appenderToWrite = app;
                    break;
                }
            }

            if (appenderToWrite == null)
            {
                BaseFileAppender newAppender = this.appenderFactory.Open(fileName, this);

                if (this.recentAppenders[freeSpot] != null)
                {
                    this.recentAppenders[freeSpot].Close();
                    this.recentAppenders[freeSpot] = null;
                }

                for (int j = freeSpot; j > 0; --j)
                {
                    this.recentAppenders[j] = this.recentAppenders[j - 1];
                }

                this.recentAppenders[0] = newAppender;
                appenderToWrite = newAppender;
            }

            if (writeHeader)
            {
                long fileLength;
                DateTime lastWriteTime;

                // Only write header on empty files or if file info cannot be obtained
                if (!appenderToWrite.GetFileInfo(out lastWriteTime, out fileLength) || fileLength == 0)
                {
                    byte[] headerBytes = this.GetHeaderBytes();
                    if (headerBytes != null)
                    {
                        appenderToWrite.Write(headerBytes);
                    }
                }
            }

            appenderToWrite.Write(bytes);

            if (this.AutoFlush)
            {
                appenderToWrite.Flush();
            }
        }

        private byte[] GetHeaderBytes()
        {
            if (this.Header == null)
            {
                return null;
            }

            string renderedText = this.Header.Render(LogEventInfo.CreateNullEvent()) + this.NewLineChars;
            return this.TransformBytes(this.Encoding.GetBytes(renderedText));
        }

        private byte[] GetFooterBytes()
        {
            if (this.Footer == null)
            {
                return null;
            }

            string renderedText = this.Footer.Render(LogEventInfo.CreateNullEvent()) + this.NewLineChars;
            return this.TransformBytes(this.Encoding.GetBytes(renderedText));
        }

        private void WriteFooterAndUninitialize(string fileName)
        {
            byte[] footerBytes = this.GetFooterBytes();
            if (footerBytes != null)
            {
                if (File.Exists(fileName))
                {
                    this.WriteToFile(fileName, footerBytes, true);
                }
            }

            this.initializedFiles.Remove(fileName);
        }

        private bool GetFileInfo(string fileName, out DateTime lastWriteTime, out long fileLength)
        {
            foreach (BaseFileAppender t in this.recentAppenders)
            {
                if (t == null)
                {
                    break;
                }

                if (t.FileName == fileName)
                {
                    t.GetFileInfo(out lastWriteTime, out fileLength);
                    return true;
                }
            }

            var fi = new FileInfo(fileName);
            if (fi.Exists)
            {
                fileLength = fi.Length;
                lastWriteTime = fi.LastWriteTime;
                return true;
            }

            fileLength = -1;
            lastWriteTime = DateTime.MinValue;
            return false;
        }

        private void InvalidateCacheItem(string fileName)
        {
            for (int i = 0; i < this.recentAppenders.Length; ++i)
            {
                if (this.recentAppenders[i] == null)
                {
                    break;
                }

                if (this.recentAppenders[i].FileName == fileName)
                {
                    this.recentAppenders[i].Close();
                    for (int j = i; j < this.recentAppenders.Length - 1; ++j)
                    {
                        this.recentAppenders[j] = this.recentAppenders[j + 1];
                    }

                    this.recentAppenders[this.recentAppenders.Length - 1] = null;
                    break;
                }
            }
        }

#if !SILVERLIGHT
        private static string CleanupFileName(string fileName)
        {
            var lastDirSeparator =
                fileName.LastIndexOfAny(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar });

            var fileName1 = fileName.Substring(lastDirSeparator + 1);
            var dirName = lastDirSeparator > 0 ? fileName.Substring(0, lastDirSeparator) : string.Empty;
            fileName1 = Path.GetInvalidFileNameChars().Aggregate(fileName1, (current, c) => current.Replace(c, '_'));
            return Path.Combine(dirName, fileName1);
        }
#endif
    }
}
#endif