//
// Copyright (c) 2004-2024 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using NLog.Common;
    using NLog.Config;
    using NLog.Internal;
    using NLog.Internal.Fakeables;
    using NLog.Layouts;
    using NLog.Targets.FileAppenders;
    using NLog.Targets.FileArchiveHandlers;

    /// <summary>
    /// FileTarget for writing formatted messages to one or more log-files.
    /// </summary>
    /// <remarks>
    /// <a href="https://github.com/nlog/nlog/wiki/File-target">See NLog Wiki</a>
    /// </remarks>
    /// <seealso href="https://github.com/nlog/nlog/wiki/File-target">Documentation on NLog Wiki</seealso>
    [Target("File")]
    public class FileTarget : TargetWithLayoutHeaderAndFooter
    {
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
        public Layout FileName
        {
            get => _fileName;
            set
            {
                _fileName = value;
                _fixedFileName = (value is SimpleLayout simpleLayout && simpleLayout.IsFixedText) ? simpleLayout.FixedText : null;
            }
        }
        private Layout _fileName = Layout.Empty;
        private string? _fixedFileName;

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
        /// When current log-file exists, then it is deleted (and resetting sequence number)
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
        public bool KeepFileOpen { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether to enable log file(s) to be deleted.
        /// </summary>
        /// <docgen category='Output Options' order='50' />
        public bool EnableFileDelete { get; set; } = true;

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
        /// <docgen category='Output Options' order='100' />
        public bool DiscardAll { get; set; }

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
        /// Gets or sets a value indicating whether to archive old log file on startup.
        /// </summary>
        /// <remarks>
        /// When current log-file exists, then roll to the next sequence number
        /// </remarks>
        /// <docgen category='Archival Options' order='50' />
        public bool ArchiveOldFileOnStartup { get; set; }

        /// <summary>
        /// Gets or sets whether to write the Header on initial creation of file appender, even if the file is not empty.
        /// Default value is <see langword="false"/>, which means only write header when initial file is empty (Ex. ensures valid CSV files)
        /// </summary>
        /// <remarks>
        /// Alternative use <see cref="ArchiveOldFileOnStartup"/> to ensure each application session gets individual log-file.
        /// </remarks>
        /// <docgen category='Archival Options' order='50' />
        public bool WriteHeaderWhenInitialFileNotEmpty { get; set; }

        /// <summary>
        /// Gets or sets a value specifying the date format when using <see cref="ArchiveFileName"/>.
        /// Obsolete and only here for Legacy reasons, instead use <see cref="ArchiveSuffixFormat"/>.
        /// </summary>
        /// <docgen category='Archival Options' order='50' />
        [Obsolete("Instead use ArchiveSuffixFormat. Marked obsolete with NLog 6.0")]
        public string? ArchiveDateFormat
        {
            get => _archiveDateFormat;
            set
            {
                if (string.Equals(value, _archiveDateFormat))
                    return;

                if (!string.IsNullOrEmpty(value))
                    ArchiveSuffixFormat = @"_{1:" + value + @"}_{0:00}";
                _archiveDateFormat = value;
            }
        }
        private string? _archiveDateFormat;

        /// <summary>
        /// Gets or sets the size in bytes above which log files will be automatically archived.
        /// </summary>
        /// <docgen category='Archival Options' order='50' />
        public long ArchiveAboveSize
        {
            get => _archiveAboveSize;
            set
            {
                _archiveAboveSize = value > 0 ? value : 0;
                _fileArchiveHandler = null;
            }
        }
        private long _archiveAboveSize;

        /// <summary>
        /// Gets or sets a value indicating whether to trigger archive operation based on time-period, by moving active-file to file-path specified by <see cref="ArchiveFileName"/>
        /// </summary>
        /// <remarks>
        /// Archive move operation only works if <see cref="FileName"/> is static in nature, and not rolling automatically because of ${date} or ${shortdate}
        ///
        /// NLog FileTarget probes the file-birthtime to recognize when time-period has passed, but file-birthtime is not supported by all filesystems.
        /// </remarks>
        /// <docgen category='Archival Options' order='50' />
        public FileArchivePeriod ArchiveEvery
        {
            get => _archiveEvery;
            set
            {
                _archiveEvery = value;
                _fileArchiveHandler = null;
            }
        }
        FileArchivePeriod _archiveEvery;

        /// <summary>
        /// Legacy archive logic where file-archive-logic moves active file to path specified by <see cref="ArchiveFileName"/>, and then recreates the active file.
        ///
        /// Use <see cref="ArchiveSuffixFormat"/> to control suffix format, instead of now obsolete token {#}
        /// </summary>
        /// <remarks>
        /// Archive file-move operation only works if <see cref="FileName"/> is static in nature, and not rolling automatically because of ${date} or ${shortdate} .
        /// 
        /// Legacy archive file-move operation can fail because of file-locks, so file-archiving can stop working because of environment issues (Other applications locking files).
        ///
        /// Avoid using <see cref="ArchiveFileName"/> when possible, and instead rely on only using <see cref="FileName"/> and <see cref="ArchiveSuffixFormat"/>.
        /// </remarks>
        /// <docgen category='Archival Options' order='50' />
        public Layout? ArchiveFileName
        {
            get => _archiveFileName ?? (_archiveSuffixFormat?.IndexOf("{1") >= 0 ? FileName : null);
            set
            {
                var archiveSuffixFormat = _archiveSuffixFormat;
                if (value is SimpleLayout simpleLayout)
                {
                    if (simpleLayout.OriginalText.IndexOf("${date", StringComparison.OrdinalIgnoreCase) >= 0 || simpleLayout.OriginalText.IndexOf("${shortdate", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        if (_archiveSuffixFormat is null || ReferenceEquals(_legacySequenceArchiveSuffixFormat, _archiveSuffixFormat) || ReferenceEquals(_legacyDateArchiveSuffixFormat, _archiveSuffixFormat))
                        {
                            archiveSuffixFormat = "_{0}";
                        }
                    }

                    if (simpleLayout.OriginalText.Contains('#'))
                    {
                        var repairLegacyLayout = simpleLayout.OriginalText.Replace(".{#}", string.Empty).Replace("_{#}", "").Replace("-{#}", "").Replace("{#}", "").Replace(".{#", "").Replace("_{#", "").Replace("-{#", "").Replace("{#", "").Replace("#}", "").Replace("#", "");
                        archiveSuffixFormat = _archiveSuffixFormat ?? _legacySequenceArchiveSuffixFormat;
                        value = new SimpleLayout(repairLegacyLayout);
                    }
                }

                _archiveFileName = value;
                if (!ReferenceEquals(_archiveSuffixFormat, archiveSuffixFormat) && archiveSuffixFormat != null)
                {
                    ArchiveSuffixFormat = archiveSuffixFormat;
                }
                _fileArchiveHandler = null;
            }
        }
        private Layout? _archiveFileName;
        private static readonly string _legacyDateArchiveSuffixFormat = "_{1:yyyyMMdd}_{0:00}"; // Cater for ArchiveNumbering.DateAndSequence
        private static readonly string _legacySequenceArchiveSuffixFormat = "_{0:00}";          // Cater for ArchiveNumbering.Sequence

        /// <summary>
        /// Gets or sets the maximum number of archive files that should be kept.
        /// </summary>
        /// <docgen category='Archival Options' order='50' />
        public int MaxArchiveFiles
        {
            get => _maxArchiveFiles;
            set
            {
                _maxArchiveFiles = value;
                _fileArchiveHandler = null;
            }
        }
        private int _maxArchiveFiles = -1;

        /// <summary>
        /// Gets or sets the maximum days of archive files that should be kept.
        /// </summary>
        /// <docgen category='Archival Options' order='50' />
        public int MaxArchiveDays
        {
            get => _maxArchiveDays;
            set
            {
                _maxArchiveDays = value > 0 ? value : 0;
                _fileArchiveHandler = null;
            }
        }
        private int _maxArchiveDays;

        /// <summary>
        /// Gets or sets the way file archives are numbered.
        /// Obsolete and only here for Legacy reasons, instead use <see cref="ArchiveSuffixFormat"/>.
        /// </summary>
        /// <docgen category='Archival Options' order='50' />
        [Obsolete("Instead use ArchiveSuffixFormat. Marked obsolete with NLog 6.0")]
        public string ArchiveNumbering
        {
            get => _archiveNumbering ?? "Sequence";
            set
            {
                if (string.Equals(value, _archiveNumbering))
                    return;

                _archiveNumbering = string.IsNullOrEmpty(value) ? null : value.Trim();
                if (_archiveNumbering is null || string.IsNullOrEmpty(_archiveNumbering))
                    return;

                if (_archiveSuffixFormat is null || ReferenceEquals(_archiveSuffixFormat, _legacyDateArchiveSuffixFormat) || ReferenceEquals(_archiveSuffixFormat, _legacySequenceArchiveSuffixFormat))
                {
                    ArchiveSuffixFormat = _archiveNumbering.IndexOf("date", StringComparison.OrdinalIgnoreCase) >= 0 ? _legacyDateArchiveSuffixFormat : _legacySequenceArchiveSuffixFormat;
                }
            }
        }
        private string? _archiveNumbering;

        /// <summary>
        /// Gets or sets the format-string to convert archive sequence-number by using string.Format
        /// </summary>
        /// <remarks>
        /// Ex. to prefix with leading zero's then one can use _{0:000} .
        ///
        /// Legacy archive-logic with <see cref="ArchiveFileName"/> uses default suffix _{1:yyyyMMdd}_{0:00} .
        /// </remarks>
        /// <docgen category='Archival Options' order='50' />
        public string ArchiveSuffixFormat
        {
            get
            {
                if (ArchiveEvery != FileArchivePeriod.None && (_archiveSuffixFormat is null || ReferenceEquals(_legacyDateArchiveSuffixFormat, _archiveSuffixFormat) || ReferenceEquals(_legacySequenceArchiveSuffixFormat, _archiveSuffixFormat)) && ArchiveFileName != null)
                {
                    switch (ArchiveEvery)
                    {
                        case FileArchivePeriod.Year:
                            return "_{1:yyyy}_{0:00}";
                        case FileArchivePeriod.Month:
                            return "_{1:yyyyMM}_{0:00}";
                        case FileArchivePeriod.Hour:
                            return "_{1:yyyyMMddHH}_{0:00}";
                        case FileArchivePeriod.Minute:
                            return "_{1:yyyyMMddHHmm}_{0:00}";
                        default:
                            return _legacyDateArchiveSuffixFormat;   // Also for weekdays
                    }
                }

                return _archiveSuffixFormat ?? _legacySequenceArchiveSuffixFormat;
            }
            set
            {
                if (!string.IsNullOrEmpty(value) && ArchiveFileName is SimpleLayout simpleLayout)
                {
                    // When legacy ArchiveFileName only contains file-extension, then strip away leading underscore from suffix
                    var fileName = Path.GetFileNameWithoutExtension(simpleLayout.OriginalText);
                    if (StringHelpers.IsNullOrWhiteSpace(fileName) && value.IndexOf('_') == 0)
                    {
                        value = value.Substring(1);
                    }
                }

                _archiveSuffixFormat = value;
            }
        }
        private string? _archiveSuffixFormat;

        /// <summary>
        /// Gets or sets a value indicating whether the footer should be written only when the file is archived.
        /// </summary>
        /// <docgen category='Archival Options' order='50' />
        public bool WriteFooterOnArchivingOnly { get; set; }

        private int OpenFileMonitorTimerInterval
        {
            get
            {
                if (OpenFileFlushTimeout <= 0 || AutoFlush || !KeepFileOpen)
                    return OpenFileCacheTimeout;
                else if (OpenFileCacheTimeout <= 0)
                    return OpenFileFlushTimeout;
                else
                    return Math.Min(OpenFileFlushTimeout, OpenFileCacheTimeout);
            }
        }

        private IFileArchiveHandler FileAchiveHandler => _fileArchiveHandler ?? (_fileArchiveHandler = CreateFileArchiveHandler());
        private IFileArchiveHandler? _fileArchiveHandler;

        private
#if !NETFRAMEWORK
        readonly
#endif
        struct OpenFileAppender
        {
            public IFileAppender FileAppender { get; }
            public int SequenceNumber { get; }

            public OpenFileAppender(IFileAppender fileAppender, int sequenceNumber)
            {
                FileAppender = fileAppender;
                SequenceNumber = sequenceNumber;
            }
        }

        private readonly Dictionary<string, OpenFileAppender> _openFileCache = new Dictionary<string, OpenFileAppender>(StringComparer.OrdinalIgnoreCase);

        private readonly ReusableStreamCreator _reusableFileWriteStream = new ReusableStreamCreator();
        private readonly ReusableStreamCreator _reusableBatchFileWriteStream = new ReusableStreamCreator(true);
        private readonly ReusableBufferCreator _reusableEncodingBuffer = new ReusableBufferCreator(1024);

        private readonly SortHelpers.KeySelector<AsyncLogEventInfo, string> _getFileNameFromLayout;

        private DateTime _lastWriteTime;
        private Timer? _openFileMonitorTimer;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileTarget" /> class.
        /// </summary>
        /// <remarks>
        /// The default value of the layout is: <code>${longdate}|${level:uppercase=true}|${logger}|${message:withexception=true}</code>
        /// </remarks>
        public FileTarget()
        {
            _getFileNameFromLayout = l => GetFileNameFromLayout(l.LogEvent);
        }

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
        /// Flushes all pending file operations.
        /// </summary>
        /// <param name="asyncContinuation">The asynchronous continuation.</param>
        protected override void FlushAsync(AsyncContinuation asyncContinuation)
        {
            try
            {
                InternalLogger.Trace("{0}: FlushAsync", this);
                if (_openFileCache.Count > 0)
                {
                    foreach (var openFile in _openFileCache)
                    {
                        openFile.Value.FileAppender.Flush();
                    }
                }
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

        /// <inheritdoc />
        protected override void InitializeTarget()
        {
            if (FileName is null || ReferenceEquals(FileName, Layout.Empty))
                throw new NLogConfigurationException("FileTarget FileName-property must be assigned. FileName is needed for file writing.");

            if (OpenFileMonitorTimerInterval > 0)
            {
                // Prepare Timer for periodic checking of flushing/closing open files (inactive until first file is opened)
                _openFileMonitorTimer = new Timer(OpenFileMonitorTimer);
            }

            base.InitializeTarget();
        }

        /// <inheritdoc />
        protected override void CloseTarget()
        {
            var openFileMonitorTimer = _openFileMonitorTimer;
            _openFileMonitorTimer = null;
            openFileMonitorTimer?.Change(Timeout.Infinite, Timeout.Infinite);
            openFileMonitorTimer?.Dispose();

            if (_openFileCache.Count > 0)
            {
                foreach (var openFile in _openFileCache.ToList())
                {
                    CloseFileWithFooter(openFile.Key, openFile.Value, false);
                }
                _openFileCache.Clear();
            }

            base.CloseTarget();
        }

        /// <inheritdoc />
        protected override void Write(LogEventInfo logEvent)
        {
            var filename = GetFileNameFromLayout(logEvent);
            if (string.IsNullOrEmpty(filename))
            {
                throw new ArgumentException("The path is not of a legal form.");
            }

            try
            {
                using (var targetStream = _reusableBatchFileWriteStream.Allocate())
                {
                    using (var targetBuilder = ReusableLayoutBuilder.Allocate())
                    using (var targetBuffer = _reusableEncodingBuffer.Allocate())
                    {
                        RenderFormattedMessageToStream(logEvent, targetBuilder.Result, targetBuffer.Result, targetStream.Result);
                    }

                    WriteBytesToFile(filename, logEvent, targetStream.Result);
                }
            }
            catch (Exception ex)
            {
                InternalLogger.Error(ex, "{0}: Failed writing to FileName: '{1}'", this, filename);
                throw;
            }
        }

        /// <inheritdoc />
        protected override void Write(IList<AsyncLogEventInfo> logEvents)
        {
            var buckets = logEvents.BucketSort(_getFileNameFromLayout);
            foreach (var bucket in buckets)
            {
                if (string.IsNullOrEmpty(bucket.Key))
                {
                    InternalLogger.Warn("{0}: FileName Layout returned empty string. The path is not of a legal form.", this);
                    var emptyPathException = new ArgumentException("The path is not of a legal form.");
                    for (int i = 0; i < logEvents.Count; ++i)
                    {
                        logEvents[i].Continuation(emptyPathException);
                    }
                    continue;
                }

                try
                {
                    WriteLogEventsToFile(bucket.Key, bucket.Value);
                }
                catch (Exception ex)
                {
                    InternalLogger.Error(ex, "{0}: Failed writing to FileName: '{1}'", this, bucket.Key);
                    if (ExceptionMustBeRethrown(ex))
                        throw;

                    for (int i = 0; i < bucket.Value.Count; ++i)
                        bucket.Value[i].Continuation(ex);
                }
            }
        }

        private void WriteLogEventsToFile(string filename, IList<AsyncLogEventInfo> logEvents)
        {
            using (var reusableStream = _reusableBatchFileWriteStream.Allocate())
            {
                var ms = reusableStream.Result ?? new MemoryStream();

                int currentIndex = 0;
                while (currentIndex < logEvents.Count)
                {
                    ms.Position = 0;
                    ms.SetLength(0);

                    var logEventWriteCount = WriteToMemoryStream(logEvents, currentIndex, ms);
                    WriteBytesToFile(filename, logEvents[currentIndex].LogEvent, ms);

                    for (int i = 0; i < logEventWriteCount; ++i)
                        logEvents[currentIndex++].Continuation(null);
                }
            }
        }

        private string GetFileNameFromLayout(LogEventInfo logEvent)
        {
            if (_fixedFileName != null)
                return _fixedFileName;

            var lastFileNameFromLayout = _lastFileNameFromLayout;
            using (var targetBuilder = ReusableLayoutBuilder.Allocate())
            {
                FileName.Render(logEvent, targetBuilder.Result);
                if (targetBuilder.Result.EqualTo(lastFileNameFromLayout))
                    return _lastFileNameFromLayout;

                lastFileNameFromLayout = targetBuilder.Result.ToString();
            }
            _lastFileNameFromLayout = lastFileNameFromLayout;
            return lastFileNameFromLayout;
        }

        private string _lastFileNameFromLayout = string.Empty;

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

        private void RenderFormattedMessageToStream(LogEventInfo logEvent, StringBuilder formatBuilder, char[] transformBuffer, MemoryStream streamTarget)
        {
            RenderFormattedMessage(logEvent, formatBuilder);
            formatBuilder.Append(LineEnding.NewLineCharacters);
            formatBuilder.CopyToStream(streamTarget, Encoding, transformBuffer);
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

        private void WriteBytesToFile(string filename, LogEventInfo firstLogEvent, MemoryStream ms)
        {
            bool hasWritten = true;
            if (!_openFileCache.TryGetValue(filename, out var openFile))
            {
                hasWritten = false;
                openFile = OpenFile(filename, firstLogEvent, null);
            }

            try
            {
                if (ArchiveAboveSize != 0 || ArchiveEvery != FileArchivePeriod.None)
                {
                    openFile = RollArchiveFile(filename, openFile, firstLogEvent, hasWritten);
                }

                openFile.FileAppender.Write(ms.GetBuffer(), 0, (int)ms.Length);

                if (AutoFlush)
                {
                    openFile.FileAppender.Flush();
                }
            }
            catch
            {
                // Close file (any retry-logic must happen inside the FileStream-implementation)
                _openFileCache.Remove(filename);
                openFile.FileAppender.Dispose();
                throw;
            }
            finally
            {
                _lastWriteTime = firstLogEvent.TimeStamp;
            }
        }

        private OpenFileAppender RollArchiveFile(string filename, OpenFileAppender openFile, LogEventInfo firstLogEvent, bool hasWritten)
        {
            var lastSequenceNo = -1;

            bool skipFileLastModified = ArchiveFileName is null;

            while (lastSequenceNo != openFile.SequenceNumber && MustArchiveFile(openFile.FileAppender, firstLogEvent))
            {
                lastSequenceNo = openFile.SequenceNumber;

                DateTime? previousFileLastModified = skipFileLastModified ? default(DateTime?) : openFile.FileAppender.FileLastModified;
                if (_lastWriteTime > DateTime.MinValue && previousFileLastModified > _lastWriteTime && (previousFileLastModified == openFile.FileAppender.OpenStreamTime || firstLogEvent.TimeStamp.Date == previousFileLastModified?.Date))
                    previousFileLastModified = _lastWriteTime;

                // Close file and roll to next file
                if (hasWritten)
                    CloseFileWithFooter(filename, openFile, true);
                else
                    CloseFile(filename, openFile);

                hasWritten = false;
                openFile = OpenFile(filename, firstLogEvent, previousFileLastModified, openFile.SequenceNumber + 1);
            }

            return openFile;
        }

        private bool MustArchiveFile(IFileAppender fileAppender, LogEventInfo firstLogEvent)
        {
            if (ArchiveAboveSize != 0 && MustArchiveBySize(fileAppender))
                return true;

            if (ArchiveEvery != FileArchivePeriod.None && MustArchiveEveryTimePeriod(fileAppender, firstLogEvent))
                return true;

            return false;
        }

        private bool MustArchiveBySize(IFileAppender fileAppender)
        {
            var currentFileSize = fileAppender.FileSize;
            if (currentFileSize == 0 || currentFileSize + 1 < ArchiveAboveSize)
                return false;

            InternalLogger.Debug("{0}: Archive because of filesize={1} of file: {2}", this, currentFileSize, fileAppender.FilePath);
            return true;
        }

        private bool MustArchiveEveryTimePeriod(IFileAppender fileAppender, LogEventInfo firstLogEvent)
        {
            var nextArchiveTime = fileAppender.NextArchiveTime;
            if (nextArchiveTime >= firstLogEvent.TimeStamp)
                return false;

            InternalLogger.Debug("{0}: Archive because of filetime of file: {1}", this, fileAppender.FilePath);
            return true;
        }

        internal static DateTime CalculateNextArchiveEventTime(FileArchivePeriod archiveEvery, DateTime fileBirthTime)
        {
            switch (archiveEvery)
            {
                case FileArchivePeriod.Year: return new DateTime(fileBirthTime.Year, 1, 1, 0, 0, 0, fileBirthTime.Kind).AddYears(1);
                case FileArchivePeriod.Month: return new DateTime(fileBirthTime.Year, fileBirthTime.Month, 1, 0, 0, 0, fileBirthTime.Kind).AddMonths(1);
                case FileArchivePeriod.Day: return new DateTime(fileBirthTime.Year, fileBirthTime.Month, fileBirthTime.Day, 0, 0, 0, fileBirthTime.Kind).AddDays(1);
                case FileArchivePeriod.Hour: return new DateTime(fileBirthTime.Year, fileBirthTime.Month, fileBirthTime.Day, fileBirthTime.Hour, 0, 0, fileBirthTime.Kind).AddHours(1);
                case FileArchivePeriod.Minute: return new DateTime(fileBirthTime.Year, fileBirthTime.Month, fileBirthTime.Day, fileBirthTime.Hour, fileBirthTime.Minute, 0, fileBirthTime.Kind).AddMinutes(1);
                case FileArchivePeriod.Monday: return CalculateNextWeekday(fileBirthTime, DayOfWeek.Monday);
                case FileArchivePeriod.Tuesday: return CalculateNextWeekday(fileBirthTime, DayOfWeek.Tuesday);
                case FileArchivePeriod.Wednesday: return CalculateNextWeekday(fileBirthTime, DayOfWeek.Wednesday);
                case FileArchivePeriod.Thursday: return CalculateNextWeekday(fileBirthTime, DayOfWeek.Thursday);
                case FileArchivePeriod.Friday: return CalculateNextWeekday(fileBirthTime, DayOfWeek.Friday);
                case FileArchivePeriod.Saturday: return CalculateNextWeekday(fileBirthTime, DayOfWeek.Saturday);
                case FileArchivePeriod.Sunday: return CalculateNextWeekday(fileBirthTime, DayOfWeek.Sunday);
                default: return DateTime.MaxValue;
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
            return previousLogEventTimestamp.Date.AddDays(target - start);
        }

        private OpenFileAppender OpenFile(string filename, LogEventInfo firstLogEvent, DateTime? previousFileLastModified, int sequenceNumber = 0)
        {
            bool createDirs = sequenceNumber == 0 && CreateDirs && _openFileCache.Count == 0;

            PruneOpenFileCache();

            sequenceNumber = FileAchiveHandler.ArchiveBeforeOpenFile(filename, firstLogEvent, previousFileLastModified, sequenceNumber);
            var fullFilePath = BuildFullFilePath(filename, sequenceNumber);

            if (createDirs)
            {
                InternalLogger.Debug("{0}: Verify directory and creating writer to file: {1}", this, fullFilePath);

                var directory = Path.GetDirectoryName(fullFilePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
            }
            else
            {
                InternalLogger.Debug("{0}: Creating writer to file: {1}", this, fullFilePath);
            }

            var fileAppender = CreateFileAppender(fullFilePath);
            var openFile = new OpenFileAppender(fileAppender, sequenceNumber);
            _openFileCache[filename] = openFile;

            if (_openFileCache.Count == 1)
            {
                _openFileMonitorTimer?.Change(OpenFileMonitorTimerInterval * 1000, Timeout.Infinite);
            }

            return openFile;
        }

        private void PruneOpenFileCache()
        {
            while (_openFileCache.Count > 0)
            {
                // Close files if the filepath no longer exists (without writing footer)
                KeyValuePair<string, OpenFileAppender> openFileDeleted = default;
                foreach (var openFile in _openFileCache)
                {
                    if (!openFile.Value.FileAppender.VerifyFileExists())
                    {
                        openFileDeleted = openFile;
                        break;
                    }
                }

                if (string.IsNullOrEmpty(openFileDeleted.Key))
                    break;

                CloseFile(openFileDeleted.Key, openFileDeleted.Value);
            }

            while (_openFileCache.Count >= OpenFileCacheSize)
            {
                // Close the oldest filestream (not the least recently used)
                DateTime oldestFileTime = DateTime.MaxValue;
                KeyValuePair<string, OpenFileAppender> oldestOpenFile = default;
                foreach (var oldOpenFile in _openFileCache)
                {
                    if (oldOpenFile.Value.FileAppender.OpenStreamTime < oldestFileTime)
                    {
                        oldestOpenFile = oldOpenFile;
                    }
                }
                if (!string.IsNullOrEmpty(oldestOpenFile.Key))
                    break;

                CloseFileWithFooter(oldestOpenFile.Key, oldestOpenFile.Value, false);
            }
        }

        internal void CloseOpenFileBeforeArchiveCleanup(string filepath)
        {
            KeyValuePair<string, OpenFileAppender> foundOpenFile;

            string fileName = _openFileCache.Count > 0 ? Path.GetFileName(filepath) : string.Empty;

            do
            {
                foundOpenFile = default;

                foreach (var openFile in _openFileCache)
                {
                    var openFileName = Path.GetFileName(openFile.Value.FileAppender.FilePath);
                    if (string.Equals(fileName, openFileName, StringComparison.OrdinalIgnoreCase))
                    {
                        foundOpenFile = openFile;
                        break;
                    }
                }

                if (foundOpenFile.Key != null)
                {
                    InternalLogger.Debug("{0}: Archive cleanup closing file: {1}", this, filepath);
                    CloseFile(foundOpenFile.Key, foundOpenFile.Value);
                }
            } while (foundOpenFile.Key != null);
        }

        private void CloseFile(string filename, OpenFileAppender openFile)
        {
            try
            {
                _openFileCache.Remove(filename);
                if (_openFileCache.Count == 0)
                {
                    _openFileMonitorTimer?.Change(Timeout.Infinite, Timeout.Infinite);
                }
            }
            finally
            {
                openFile.FileAppender.Dispose();
            }
        }

        private void CloseFileWithFooter(string filename, OpenFileAppender openFile, bool archiveFile)
        {
            try
            {
                if (!ReplaceFileContentsOnEachWrite && (!WriteFooterOnArchivingOnly || archiveFile))
                {
                    var footerBytes = GetFooterLayoutBytes();
                    if (footerBytes?.Length > 0)
                    {
                        openFile.FileAppender.Write(footerBytes, 0, footerBytes.Length);
                    }
                }
            }
            catch (Exception ex)
            {
                InternalLogger.Error(ex, "{0}: Failed closing file: '{1}'", this, filename);
                if (ex.MustBeRethrownImmediately())
                    throw;
            }
            finally
            {
                CloseFile(filename, openFile);
            }
        }

        private void OpenFileMonitorTimer(object state)
        {
            bool startTimer = !(_openFileMonitorTimer is null);

            try
            {
                lock (SyncRoot)
                {
                    startTimer = _openFileCache.Count != 0 && !(_openFileMonitorTimer is null);

                    if (OpenFileCacheTimeout > 0)
                    {
                        PruneOpenFileCacheUsingTimeout();
                    }

                    if (OpenFileFlushTimeout > 0 && !AutoFlush)
                    {
                        DateTime flushTime = Time.TimeSource.Current.Time.AddSeconds(-(OpenFileFlushTimeout + 1) * 1.5);
                        if (_lastWriteTime > flushTime)
                        {
                            // Only Flush when something has been written
                            foreach (var openFile in _openFileCache)
                            {
                                openFile.Value.FileAppender.Flush();
                            }
                        }
                    }

                    startTimer = startTimer && _openFileCache.Count != 0;
                }
            }
            catch (Exception ex)
            {
                InternalLogger.Warn(ex, "{0}: Exception in OpenFileMonitorTimer", this);
            }
            finally
            {
                if (startTimer)
                    _openFileMonitorTimer?.Change(OpenFileMonitorTimerInterval * 1000, Timeout.Infinite);
            }
        }

        private void PruneOpenFileCacheUsingTimeout()
        {
            DateTime closeTime = Time.TimeSource.Current.Time.AddSeconds(-OpenFileCacheTimeout);
            bool oldFilesMustBeClosed = false;

            foreach (var openFile in _openFileCache)
            {
                if (openFile.Value.FileAppender.OpenStreamTime < closeTime)
                {
                    oldFilesMustBeClosed = true;
                    break;
                }
            }

            if (oldFilesMustBeClosed)
            {
                foreach (var openFile in _openFileCache.ToList())
                {
                    if (openFile.Value.FileAppender.OpenStreamTime < closeTime)
                    {
                        CloseFile(openFile.Key, openFile.Value);
                    }
                }
            }
        }

        internal string BuildFullFilePath(string newFileName, int sequenceNumber, DateTime fileLastModified = default)
        {
            if (sequenceNumber > 0 || fileLastModified != default)
            {
                var fileName = Path.GetFileName(newFileName) ?? string.Empty;
                var fileExt = Path.GetExtension(fileName) ?? string.Empty;
                newFileName = newFileName.Substring(0, newFileName.Length - fileName.Length);
                if (!string.IsNullOrEmpty(fileExt))
                    fileName = fileName.Substring(0, fileName.Length - fileExt.Length);

                object fileLastModifiedObj = fileLastModified == default ? string.Empty : (object)fileLastModified;
                try
                {
                    newFileName = newFileName + fileName + string.Format(ArchiveSuffixFormat, sequenceNumber, fileLastModifiedObj) + fileExt;
                }
                catch (Exception ex)
                {
                    InternalLogger.Error(ex, "{0}: Failed to apply ArchiveSuffixFormat={1} using SequenceNumber={2} for file: '{3}'", this, ArchiveSuffixFormat, sequenceNumber, newFileName);
                    if (ExceptionMustBeRethrown(ex))
                        throw;
                    newFileName = newFileName + fileName + string.Format(_legacySequenceArchiveSuffixFormat, sequenceNumber) + fileExt;
                }
            }

            var filepath = CleanFullFilePath(newFileName);
            return filepath;
        }

        internal static string CleanFullFilePath(string filename)
        {
            var lastDirSeparator = filename.LastIndexOfAny(DirectorySeparatorChars);

            char[]? fileNameChars = null;    // defer char[] memory-allocation until detecting invalid char

            for (int i = lastDirSeparator + 1; i < filename.Length; i++)
            {
                if (InvalidFileNameChars.Contains(filename[i]))
                {
                    if (fileNameChars is null)
                    {
                        fileNameChars = filename.Substring(lastDirSeparator + 1).ToCharArray();
                    }
                    fileNameChars[i - (lastDirSeparator + 1)] = '_';
                }
            }

            //only if an invalid char was replaced do we create a new string.
            if (fileNameChars != null)
            {
                //keep the / in the dirname, because dirname could be c:/ and combine of c: and file name won't work well.
                var dirName = lastDirSeparator > 0 ? filename.Substring(0, lastDirSeparator + 1) : string.Empty;
                filename = Path.Combine(dirName, new string(fileNameChars));
            }

            var filepath = FileInfoHelper.IsRelativeFilePath(filename) ? Path.Combine(AppEnvironmentWrapper.FixFilePathWithLongUNC(LogManager.LogFactory.CurrentAppEnvironment.AppDomainBaseDirectory), filename) : filename;
            filepath = Path.GetFullPath(filepath);
            return filepath;
        }
        private static readonly char[] DirectorySeparatorChars = new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar };
        private static readonly HashSet<char> InvalidFileNameChars = new HashSet<char>(Path.GetInvalidFileNameChars());

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
        /// The sequence of <see langword="byte"/> to be written in a file after applying any formatting and any
        /// transformations required from the <see cref="Layout"/>.
        /// </summary>
        /// <param name="layout">The layout used to render output message.</param>
        /// <returns>Sequence of <see langword="byte"/> to be written.</returns>
        /// <remarks>Usually it is used to render the header and hooter of the files.</remarks>
        private byte[] GetLayoutBytes(Layout layout)
        {
            if (layout is null)
            {
                return ArrayHelper.Empty<byte>();
            }

            using (var targetBuilder = ReusableLayoutBuilder.Allocate())
            using (var targetBuffer = _reusableEncodingBuffer.Allocate())
            {
                layout.Render(LogEventInfo.CreateNullEvent(), targetBuilder.Result);
                targetBuilder.Result.Append(LineEnding.NewLineCharacters);
                using (MemoryStream ms = new MemoryStream(targetBuilder.Result.Length))
                {
                    targetBuilder.Result.CopyToStream(ms, Encoding, targetBuffer.Result);
                    return ms.ToArray();
                }
            }
        }

        internal byte[] GetFooterLayoutBytes()
        {
            if (Footer != null)
            {
                InternalLogger.Trace("{0}: Write footer", this);
                return GetLayoutBytes(Footer);
            }
            return ArrayHelper.Empty<byte>();
        }

        internal Stream CreateFileStreamWithRetry(IFileAppender fileAppender, int bufferSize, bool initialFileOpen)
        {
            int currentDelay = 1;
            int retryCount = KeepFileOpen ? 0 : 5;
            var filePath = fileAppender.FilePath;

            for (int i = 0; i <= retryCount; ++i)
            {
                try
                {
                    return OpenNewFileStream(filePath, bufferSize, initialFileOpen);
                }
                catch (DirectoryNotFoundException)
                {
                    if (!CreateDirs)
                        throw;

                    InternalLogger.Debug("{0}: DirectoryNotFoundException - Attempting to create directory for FileName: {1}", this, filePath);

                    var directoryName = Path.GetDirectoryName(filePath);

                    try
                    {
                        Directory.CreateDirectory(directoryName);
                    }
                    catch (Exception ex)
                    {
                        // If creating a directory failed, don't retry for this message
                        throw new NLogRuntimeException($"Could not create directory {directoryName}", ex);
                    }
                }
                catch (IOException ex)
                {
                    if (i >= retryCount)
                    {
                        throw; // rethrow
                    }

                    var actualDelay = currentDelay > 4 ? new Random().Next(4, currentDelay) : currentDelay;
                    InternalLogger.Warn("{0}: Attempt #{1} to open {2} failed - {3} {4}. Sleeping for {5}ms", this, i, filePath, ex.GetType(), ex.Message, actualDelay);
                    currentDelay *= 4;
                    System.Threading.Thread.Sleep(actualDelay);
                }
            }

            throw new InvalidOperationException("Should not be reached.");
        }

        private Stream OpenNewFileStream(string filePath, int bufferSize, bool initialFileOpen)
        {
            var fileStream = CreateFileStream(filePath, bufferSize);

            try
            {
                bool? fileWasEmpty = null;

                if (WriteBom)
                {
                    fileWasEmpty = ReplaceFileContentsOnEachWrite || fileStream.Length == 0;
                    if (fileWasEmpty == true)
                    {
                        InternalLogger.Trace("{0}: Write byte order mark from encoding={1}", this, Encoding);
                        var preamble = Encoding.GetPreamble();
                        if (preamble.Length > 0)
                        {
                            fileStream.Write(preamble, 0, preamble.Length);
                        }
                    }
                }

                if (Header != null)
                {
                    bool writeHeader = (initialFileOpen && WriteHeaderWhenInitialFileNotEmpty) || ReplaceFileContentsOnEachWrite || (fileWasEmpty ?? fileStream.Length == 0);
                    if (writeHeader)
                    {
                        InternalLogger.Trace("{0}: Write header", this);
                        var headerBytes = GetLayoutBytes(Header);
                        fileStream.Write(headerBytes, 0, headerBytes.Length);
                    }
                }

                return fileStream;
            }
            catch
            {
                fileStream?.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Creates stream for appending to the specified <paramref name="filePath"/>
        /// </summary>
        /// <param name="filePath">Path of the file to be written</param>
        /// <param name="bufferSize">Wanted internal buffer size for the stream</param>
        /// <returns>Stream for appending to the file</returns>
        protected virtual Stream CreateFileStream(string filePath, int bufferSize)
        {
            var fileShare = FileShare.Read;
            if (EnableFileDelete)
            {
                fileShare |= FileShare.Delete;
            }

            var fileMode = FileMode.Append;
            if (ReplaceFileContentsOnEachWrite)
            {
                fileMode = FileMode.Create; // Create or truncate
            }

            return new FileStream(filePath, fileMode, FileAccess.Write, fileShare, bufferSize);
        }

        private IFileAppender CreateFileAppender(string filePath)
        {
            if (DiscardAll)
            {
                return new DiscardAllFileAppender(filePath);
            }

            if (ReplaceFileContentsOnEachWrite)
            {
                return new MinimalFileLockingAppender(this, filePath);
            }

            if (KeepFileOpen)
            {
                return new ExclusiveFileLockingAppender(this, filePath);
            }

            return new MinimalFileLockingAppender(this, filePath);
        }

        private IFileArchiveHandler CreateFileArchiveHandler()
        {
            if (MaxArchiveFiles < 0 && MaxArchiveDays == 0 && ArchiveAboveSize == 0 && ArchiveEvery == FileArchivePeriod.None)
            {
                if (!DeleteOldFileOnStartup && !ArchiveOldFileOnStartup)
                    return DisabledFileArchiveHandler.Default;   // Archive-logic disabled, always append to active file without rolling
                else if (!ArchiveOldFileOnStartup)
                    return new ZeroFileArchiveHandler(this);     // No archive but cleanup old files at startup
            }

            if (MaxArchiveFiles == 0)
                return new ZeroFileArchiveHandler(this);    // MaxArchiveFiles = 0 means truncate active file on archive-event

            if (ArchiveFileName is null)
                return new RollingArchiveFileHandler(this); // Updated dynamic sequence handling without file-move-logic

            return new LegacyArchiveFileNameHandler(this);  // Legacy / unstable because file-move can fail because of file-locks from other applications
        }
    }
}
