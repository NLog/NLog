// 
// Copyright (c) 2004-2010 Jaroslaw Kowalski <jaak@jkowalski.net>
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

#if !SILVERLIGHT2 && !SILVERLIGHT3

namespace NLog.Targets
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using System.Threading;
    using NLog.Common;
    using NLog.Config;
    using NLog.Internal;
    using NLog.Internal.FileAppenders;
    using NLog.Layouts;

    /// <summary>
    /// Writes logging messages to one or more files.
    /// </summary>
    /// <example>
    /// <p>
    /// To set up the target in the <a href="config.html">configuration file</a>, 
    /// use the following syntax:
    /// </p>
    /// <code lang="XML" source="examples/targets/Configuration File/File/Simple/NLog.config" />
    /// <p>
    /// You can use a single target to write to multiple files. The following
    /// example writes each log message to a file named after its log level, so
    /// it will create:
    /// <c>Trace.log</c>, <c>Debug.log</c>, <c>Info.log</c>, <c>Warn.log</c>, 
    /// <c>Error.log</c>, <c>Fatal.log</c>
    /// </p>
    /// <code lang="XML" source="examples/targets/Configuration File/File/Multiple/NLog.config" />
    /// <p>
    /// The file names can be quite complex for the most demanding scenarios. This
    /// example shows a way to create separate files for each day, user and log level.
    /// As you can see, the possibilities are endless.
    /// </p>
    /// <code lang="XML" source="examples/targets/Configuration File/File/Multiple2/NLog.config" />
    /// <p>
    /// Depending on your usage scenario it may be useful to add an <a href="target.AsyncWrapper.html">asynchronous target wrapper</a>
    /// around the file target. This way all your log messages
    /// will be written in a separate thread so your main thread can finish
    /// your work more quickly. Asynchronous logging is recommended
    /// for multi-threaded server applications which run for a long time and
    /// is not recommended for quickly-finishing command line applications.
    /// </p>
    /// <code lang="XML" source="examples/targets/Configuration File/File/Asynchronous/NLog.config" />
    /// <p>
    /// The above examples assume just one target and a single rule. See below for
    /// a programmatic configuration that's equivalent to the above config file:
    /// </p>
    /// <code lang="C#" source="examples/targets/Configuration API/File/Asynchronous/Example.cs" />
    /// <p>
    /// More configuration options are described <a href="config.html">here</a>.
    /// </p>
    /// <p>
    /// To set up the log target programmatically use code like this:
    /// </p>
    /// <code lang="C#" source="examples/targets/Configuration API/File/Simple/Example.cs" />
    /// <p>
    /// File target can also do file archiving, meaning that the log file is automatically
    /// moved to another place based on its size and time. This example demonstrates 
    /// file archiving based on size. Files after 10000 bytes are moved to a separate folder
    /// and renamed log.00000.txt, log.00001.txt and so on.
    /// </p>
    /// <code lang="C#" source="examples/targets/Configuration API/File/Archive1/Example.cs" />
    /// <p>
    /// File archiving can also be done on date/time changes. For example, to create a new 
    /// archive file every minute use this code:
    /// </p>
    /// <code lang="C#" source="examples/targets/Configuration API/File/Archive2/Example.cs" />
    /// <p>
    /// You can combine both methods as demonstrated here:
    /// </p>
    /// <code lang="C#" source="examples/targets/Configuration API/File/Archive3/Example.cs" />
    /// <p>
    /// Note that file archiving works even when you use a single target instance
    /// to write to multiple files, such as putting each log level in a separate place:
    /// </p>
    /// <code lang="C#" source="examples/targets/Configuration API/File/Archive4/Example.cs" />
    /// <p>
    /// You can write texts using alternative layouts, such as CSV (comma-separated values).
    /// This example writes files which are properly CSV-quoted (can handle messages with line breaks
    /// and quotes)
    /// </p>
    /// <code lang="C#" source="examples/targets/Configuration API/File/CSV/Example.cs" />
    /// <para>
    /// This is the configuration file version:
    /// </para>
    /// <code lang="XML" source="examples/targets/Configuration File/File/CSV/NLog.config" />
    /// </example>
    [Target("File")]
    public class FileTarget : TargetWithLayoutHeaderAndFooter, ICreateFileParameters
    {
        private readonly LogEventComparer logEventComparer;
        private readonly Dictionary<string, DateTime> initializedFiles = new Dictionary<string, DateTime>();

        private LineEndingMode lineEndingMode = LineEndingMode.Default;
        private IFileAppenderFactory appenderFactory;
        private BaseFileAppender[] recentAppenders;
        private Timer autoClosingTimer;
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
            this.MaxArchiveFiles = 9;
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
#if !SILVERLIGHT && !NET_CF
            this.FileAttributes = Win32FileAttributes.Normal;
            this.NewLineChars = Environment.NewLine;
#else
            this.NewLineChars = "\r\n";
#endif
            this.EnableFileDelete = true;
            this.OpenFileCacheTimeout = -1;
            this.OpenFileCacheSize = 5;
            this.CreateDirs = true;
            this.logEventComparer = new LogEventComparer(this);
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

#if !NET_CF && !SILVERLIGHT
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
#if NET_CF
                        this.NewLineChars = "\r\n";
#else
                        this.NewLineChars = Environment.NewLine;
#endif
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
        public int MaxArchiveFiles { get; set; }

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
                for (int i = 0; i < this.recentAppenders.Length; ++i)
                {
                    if (this.recentAppenders[i] == null)
                    {
                        break;
                    }

                    this.recentAppenders[i].Flush();
                }

                asyncContinuation(null);
            }
            catch (Exception ex)
            {
                asyncContinuation(ex);
            }
        }

        /// <summary>
        /// Initializes file logging by creating data structures that
        /// enable efficient multi-file logging.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();

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
#if NET_CF || SILVERLIGHT
                        this.appenderFactory = RetryingMultiProcessFileAppender.TheFactory;
#elif MONO
    //
    // mono on Windows uses mutexes, on Unix - special appender
    //
                        if (PlatformDetector.IsCurrentOSCompatibleWith(RuntimeOS.Unix))
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
#if NET_CF || SILVERLIGHT
                        this.appenderFactory = RetryingMultiProcessFileAppender.TheFactory;
#elif MONO
    //
    // mono on Windows uses mutexes, on Unix - special appender
    //
                        if (PlatformDetector.IsCurrentOSCompatibleWith(RuntimeOS.Unix))
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
        protected override void Close()
        {
            base.Close();

            lock (this)
            {
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
            }

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

        /// <summary>
        /// Writes the specified logging event to a file specified in the FileName 
        /// parameter.
        /// </summary>
        /// <param name="logEvent">The logging event.</param>
        protected override void Write(LogEventInfo logEvent)
        {
            lock (this)
            {
                string fileName = this.FileName.Render(logEvent);
                byte[] bytes = this.GetBytesToWrite(logEvent);

                if (this.ShouldAutoArchive(fileName, logEvent, bytes.Length))
                {
                    this.InvalidateCacheItem(fileName);
                    this.DoAutoArchive(fileName, logEvent);
                }

                this.WriteToFile(fileName, bytes, false);
            }
        }

        /// <summary>
        /// Writes the specified array of logging events to a file specified in the FileName
        /// parameter.
        /// </summary>
        /// <param name="logEvents">An array of <see cref="LogEventInfo "/> objects.</param>
        /// <param name="asyncContinuations">The asynchronous continuations.</param>
        /// <remarks>
        /// This function makes use of the fact that the events are batched by sorting
        /// the requests by filename. This optimizes the number of open/close calls
        /// and can help improve performance.
        /// </remarks>
        protected override void Write(LogEventInfo[] logEvents, AsyncContinuation[] asyncContinuations)
        {
            Array.Sort(logEvents, 0, logEvents.Length, this.logEventComparer);

            lock (this)
            {
                string currentFileName = null;
                var ms = new MemoryStream();
                LogEventInfo firstLogEvent = null;
                var pendingContinuations = new List<AsyncContinuation>();

                for (int i = 0; i < logEvents.Length; ++i)
                {
                    LogEventInfo logEvent = logEvents[i];
                    string logEventFileName = this.FileName.Render(logEvent);
                    if (logEventFileName != currentFileName)
                    {
                        this.FlushCurrentFileWrites(currentFileName, firstLogEvent, ms, pendingContinuations);

                        currentFileName = logEventFileName;
                        firstLogEvent = logEvent;
                        ms.SetLength(0);
                        ms.Position = 0;
                    }

                    byte[] bytes = this.GetBytesToWrite(logEvent);
                    ms.Write(bytes, 0, bytes.Length);
                    pendingContinuations.Add(asyncContinuations[i]);
                }

                this.FlushCurrentFileWrites(currentFileName, firstLogEvent, ms, pendingContinuations);
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
        /// <param name="bytes">The byte array.</param>
        /// <returns>The modified byte array. The function can do the modification in-place.</returns>
        protected virtual byte[] TransformBytes(byte[] bytes)
        {
            return bytes;
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
            catch (Exception ex)
            {
                lastException = ex;
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

            string newFileName = this.ReplaceNumber(pattern, archiveNumber);
            if (File.Exists(fileName))
            {
                this.RecursiveRollingRename(newFileName, pattern, archiveNumber + 1);
            }

            InternalLogger.Trace("Renaming {0} to {1}", fileName, newFileName);

            try
            {
                File.Move(fileName, newFileName);
            }
            catch (IOException)
            {
                string dir = Path.GetDirectoryName(newFileName);
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                File.Move(fileName, newFileName);
            }
        }

        private string ReplaceNumber(string pattern, int value)
        {
            int firstPart = pattern.IndexOf("{#");
            int lastPart = pattern.IndexOf("#}") + 2;
            int numDigits = lastPart - firstPart - 2;

            return pattern.Substring(0, firstPart) + Convert.ToString(value, 10).PadLeft(numDigits, '0') + pattern.Substring(lastPart);
        }

        private void SequentialArchive(string fileName, string pattern)
        {
            string baseNamePattern = Path.GetFileName(pattern);

            int firstPart = baseNamePattern.IndexOf("{#");
            int lastPart = baseNamePattern.IndexOf("#}") + 2;
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
                        num = Convert.ToInt32(number);
                    }
                    catch (FormatException)
                    {
                        continue;
                    }

                    nextNumber = Math.Max(nextNumber, num);
                    if (minNumber != -1)
                    {
                        minNumber = Math.Min(minNumber, num);
                    }
                    else
                    {
                        minNumber = num;
                    }

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

            string newFileName = this.ReplaceNumber(pattern, nextNumber);
            File.Move(fileName, newFileName);
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
                fileNamePattern = this.ArchiveFileName.Render(ev);
            }

            switch (this.ArchiveNumbering)
            {
                case ArchiveNumberingMode.Rolling:
                    this.RecursiveRollingRename(fi.FullName, fileNamePattern, 0);
                    break;

                case ArchiveNumberingMode.Sequence:
                    this.SequentialArchive(fi.FullName, fileNamePattern);
                    break;
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
                string formatString;

                switch (this.ArchiveEvery)
                {
                    case FileArchivePeriod.Year:
                        formatString = "yyyy";
                        break;

                    case FileArchivePeriod.Month:
                        formatString = "yyyyMM";
                        break;

                    default:
                    case FileArchivePeriod.Day:
                        formatString = "yyyyMMdd";
                        break;

                    case FileArchivePeriod.Hour:
                        formatString = "yyyyMMddHH";
                        break;

                    case FileArchivePeriod.Minute:
                        formatString = "yyyyMMddHHmm";
                        break;
                }

                string ts = lastWriteTime.ToString(formatString, CultureInfo.InvariantCulture);
                string ts2 = ev.TimeStamp.ToString(formatString, CultureInfo.InvariantCulture);

                if (ts != ts2)
                {
                    return true;
                }
            }

            return false;
        }

        private void AutoClosingTimerCallback(object state)
        {
            lock (this)
            {
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
                catch (Exception ex)
                {
                    InternalLogger.Warn("Exception in AutoClosingTimerCallback: {0}", ex);
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
                    if (this.DeleteOldFileOnStartup)
                    {
                        try
                        {
                            File.Delete(fileName);
                        }
                        catch (Exception ex)
                        {
                            InternalLogger.Warn("Unable to delete old log file '{0}': {1}", fileName, ex);
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

            if (writeHeader && !justData)
            {
                byte[] headerBytes = this.GetHeaderBytes();
                if (headerBytes != null)
                {
                    appenderToWrite.Write(headerBytes);
                }
            }

            appenderToWrite.Write(bytes);
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
            for (int i = 0; i < this.recentAppenders.Length; ++i)
            {
                if (this.recentAppenders[i] == null)
                {
                    break;
                }

                if (this.recentAppenders[i].FileName == fileName)
                {
                    this.recentAppenders[i].GetFileInfo(out lastWriteTime, out fileLength);
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
            else
            {
                fileLength = -1;
                lastWriteTime = DateTime.MinValue;
                return false;
            }
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

        /// <summary>
        /// Compares two log events do determine their ordering 
        /// by filename first, then by sequence ID.
        /// </summary>
        private class LogEventComparer : IComparer
        {
            private readonly FileTarget fileTarget;

            /// <summary>
            /// Initializes a new instance of the <see cref="LogEventComparer" /> class.
            /// </summary>
            /// <param name="fileTarget">The file target.</param>
            public LogEventComparer(FileTarget fileTarget)
            {
                this.fileTarget = fileTarget;
            }

            /// <summary>
            /// Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.
            /// </summary>
            /// <param name="x">The first object to compare.</param>
            /// <param name="y">The second object to compare.</param>
            /// <returns>
            /// Value
            /// Condition
            /// Less than zero
            /// <paramref name="x"/> is less than <paramref name="y"/>.
            /// Zero
            /// <paramref name="x"/> equals <paramref name="y"/>.
            /// Greater than zero
            /// <paramref name="x"/> is greater than <paramref name="y"/>.
            /// </returns>
            /// <exception cref="T:System.ArgumentException">
            /// Neither <paramref name="x"/> nor <paramref name="y"/> implements the <see cref="T:System.IComparable"/> interface.
            /// -or-
            /// <paramref name="x"/> and <paramref name="y"/> are of different types and neither one can handle comparisons with the other.
            /// </exception>
            public int Compare(object x, object y)
            {
                var le1 = (LogEventInfo)x;
                var le2 = (LogEventInfo)y;

                string filename1 = this.fileTarget.FileName.Render(le1);
                string filename2 = this.fileTarget.FileName.Render(le2);

                int val = String.CompareOrdinal(filename1, filename2);
                if (val != 0)
                {
                    return val;
                }

                if (le1.SequenceID < le2.SequenceID)
                {
                    return -1;
                }

                if (le1.SequenceID > le2.SequenceID)
                {
                    return 1;
                }

                return 0;
            }
        }
    }
}

#endif