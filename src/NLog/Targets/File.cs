// 
// Copyright (c) 2004-2006 Jaroslaw Kowalski <jaak@jkowalski.net>
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

using System;
using System.Xml;
using System.IO;
using System.Threading;
using System.Collections;
using System.Collections.Specialized;

using NLog;
using NLog.Config;

using NLog.Internal;
using NLog.Internal.FileAppenders;
#if !NETCF
using System.Runtime.InteropServices;
using NLog.Internal.Win32;
#endif

namespace NLog.Targets
{
    /// <summary>
    /// Writes logging messages to one or more files.
    /// </summary>
    /// <example>
    /// <p>
    /// To set up the target in the <a href="config.html">configuration file</a>, 
    /// use the following syntax:
    /// </p>
    /// <code lang="XML" src="examples/targets/Configuration File/File/Simple/NLog.config" />
    /// <p>
    /// You can use a single target to write to multiple files. The following
    /// example writes each log message to a file named after its log level, so
    /// it will create:
    /// <c>Trace.log</c>, <c>Debug.log</c>, <c>Info.log</c>, <c>Warn.log</c>, 
    /// <c>Error.log</c>, <c>Fatal.log</c>
    /// </p>
    /// <code lang="XML" src="examples/targets/Configuration File/File/Multiple/NLog.config" />
    /// <p>
    /// The file names can be quite complex for the most demanding scenarios. This
    /// example shows a way to create separate files for each day, user and log level.
    /// As you can see, the possibilities are endless.
    /// </p>
    /// <code lang="XML" src="examples/targets/Configuration File/File/Multiple2/NLog.config" />
    /// <p>
    /// Depending on your usage scenario it may be useful to add an <a href="target.AsyncWrapper.html">asynchronous target wrapper</a>
    /// around the file target. This way all your log messages
    /// will be written in a separate thread so your main thread can finish
    /// your work more quickly. Asynchronous logging is recommended
    /// for multi-threaded server applications which run for a long time and
    /// is not recommended for quickly-finishing command line applications.
    /// </p>
    /// <code lang="XML" src="examples/targets/Configuration File/File/Asynchronous/NLog.config" />
    /// <p>
    /// The above examples assume just one target and a single rule. See below for
    /// a programmatic configuration that's equivalent to the above config file:
    /// </p>
    /// <code lang="C#" src="examples/targets/Configuration API/File/Asynchronous/Example.cs" />
    /// <p>
    /// More configuration options are described <a href="config.html">here</a>.
    /// </p>
    /// <p>
    /// To set up the log target programmatically use code like this:
    /// </p>
    /// <code lang="C#" src="examples/targets/Configuration API/File/Simple/Example.cs" />
    /// </example>
    [Target("File")]
    public class FileTarget: Target, IFileOpener
    {
        /// <summary>
        /// Specifies the way archive numbering is performed.
        /// </summary>
        public enum ArchiveNumberingMode
        {
            /// <summary>
            /// Sequence style numbering. The most recent archive has the highest number.
            /// </summary>
            Sequence,

            /// <summary>
            /// Rolling style numbering (the most recent is always #0 then #1, ..., #N
            /// </summary>
            Rolling,
        }

        /// <summary>
        /// Modes of archiving files based on time.
        /// </summary>
        public enum ArchiveEveryMode
        {
            /// <summary>
            /// Don't archive based on time.
            /// </summary>
            None,

            /// <summary>
            /// Archive every year.
            /// </summary>
            Year,

            /// <summary>
            /// Archive every month.
            /// </summary>
            Month,

            /// <summary>
            /// Archive daily.
            /// </summary>
            Day,

            /// <summary>
            /// Archive every hour.
            /// </summary>
            Hour,

            /// <summary>
            /// Archive every minute.
            /// </summary>
            Minute
        }

        private Random _random = new Random();
        private Layout _fileNameLayout;
        private bool _createDirs = true;
        private bool _keepFileOpen = true;
        private System.Text.Encoding _encoding = System.Text.Encoding.Default;
#if NETCF
        private string _newLine = "\r\n";
#else
        private string _newLine = Environment.NewLine;
#endif
        private bool _autoFlush = true;
        private bool _concurrentWrites = true;
        private bool _networkWrites = false;
        private int _concurrentWriteAttempts = 10;
        private int _bufferSize = 32768;
        private int _concurrentWriteAttemptDelay = 1;
        private LogEventComparer _logEventComparer;
        private Layout _autoArchiveFileName = null;
        private int _maxArchiveFiles = 9;
        private long _archiveAboveSize = -1;
        private ArchiveEveryMode _archiveEvery = ArchiveEveryMode.None;
        private int _openFileCacheSize = 5;
        private IFileAppenderFactory _appenderFactory;
        private IFileAppender[] _recentAppenders;
        private DateTime[] _lastWriteTime;
        private DateTime[] _openTime;
        private ArchiveNumberingMode _archiveNumbering = ArchiveNumberingMode.Sequence;
        private Timer _autoClosingTimer = null;
        private int _openFileCacheTimeout = 1;
        private bool _first = true;
        private bool _deleteOldFileOnStartup = false;
        private bool _replaceFileContentsOnEachWrite = false;
        private bool _enableFileDelete = true;
#if !NETCF
        private Win32FileAttributes _fileAttributes = Win32FileAttributes.Normal;
#endif

        /// <summary>
        /// Creates a new instance of <see cref="FileTarget"/>.
        /// </summary>
        public FileTarget()
        {
            _logEventComparer = new LogEventComparer(this);
        }

        /// <summary>
        /// The name of the file to write to.
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
        [RequiredParameter]
        [AcceptsLayout]
        public string FileName
        {
            get { return _fileNameLayout.Text; }
            set { _fileNameLayout = new Layout(value); }
        }

        /// <summary>
        /// Create directories if they don't exist.
        /// </summary>
        /// <remarks>
        /// Setting this to false may improve performance a bit, but you'll receive an error
        /// when attempting to write to a directory that's not present.
        /// </remarks>
        [System.ComponentModel.DefaultValue(true)]
        public bool CreateDirs
        {
            get { return _createDirs; }
            set { _createDirs = value; }
        }

        /// <summary>
        /// The number of files to be kept open. Setting this to a higher value may improve performance
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
        [System.ComponentModel.DefaultValue(5)]
        public int OpenFileCacheSize
        {
            get { return _openFileCacheSize; }
            set { _openFileCacheSize = value; }
        }

        /// <summary>
        /// Maximum number of seconds that files are kept open.
        /// </summary>
        [System.ComponentModel.DefaultValue(1)]
        public int OpenFileCacheTimeout
        {
            get { return _openFileCacheTimeout; }
            set { _openFileCacheTimeout = value; }
        }

        /// <summary>
        /// Delete old log file on startup.
        /// </summary>
        /// <remarks>
        /// This option works only when the "fileName" parameter denotes a single file.
        /// </remarks>
        [System.ComponentModel.DefaultValue(false)]
        public bool DeleteOldFileOnStartup
        {
            get { return _deleteOldFileOnStartup; }
            set { _deleteOldFileOnStartup = value; }
        }

        /// <summary>
        /// Replace file contents on each write instead of appending log message at the end.
        /// </summary>
        [System.ComponentModel.DefaultValue(false)]
        public bool ReplaceFileContentsOnEachWrite
        {
            get { return _replaceFileContentsOnEachWrite; }
            set { _replaceFileContentsOnEachWrite = value; }
        }

        /// <summary>
        /// Keep log file open instead of opening and closing it on each logging event.
        /// </summary>
        /// <remarks>
        /// Setting this property to <c>True</c> helps improve performance.
        /// </remarks>
        [System.ComponentModel.DefaultValue(true)]
        public bool KeepFileOpen
        {
            get { return _keepFileOpen; }
            set { _keepFileOpen = value; }
        }

        /// <summary>
        /// Enable log file(s) to be deleted.
        /// </summary>
        [System.ComponentModel.DefaultValue(true)]
        public bool EnableFileDelete
        {
            get { return _enableFileDelete; }
            set { _enableFileDelete = value; }
        }

#if !NETCF
        /// <summary>
        /// File attributes (Windows only).
        /// </summary>
        public Win32FileAttributes FileAttributes
        {
            get { return _fileAttributes; }
            set { _fileAttributes = value; }
        }
#endif

        /// <summary>
        /// Automatically flush the file buffers after each log message.
        /// </summary>
        [System.ComponentModel.DefaultValue(true)]
        public bool AutoFlush
        {
            get { return _autoFlush; }
            set { _autoFlush = value; }
        }

        /// <summary>
        /// Log file buffer size in bytes.
        /// </summary>
        [System.ComponentModel.DefaultValue(32768)]
        public int BufferSize
        {
            get { return _bufferSize; }
            set { _bufferSize = value; }
        }

        /// <summary>
        /// File encoding.</summary>
        /// <remarks>
        /// Can be any encoding name supported by System.Text.Encoding.GetEncoding() e.g. <c>windows-1252</c>, <c>iso-8859-2</c>.
        /// </remarks>
        public string Encoding
        {
            get { return _encoding.WebName; }
            set { _encoding = System.Text.Encoding.GetEncoding(value); }
        }

        /// <summary>
        /// Enables concurrent writes to the log file by multiple processes on the same host.
        /// </summary>
        /// <remarks>
        /// This makes multi-process logging possible. NLog uses a special technique
        /// that lets it keep the files open for writing.
        /// </remarks>
        [System.ComponentModel.DefaultValue(true)]
        public bool ConcurrentWrites
        {
            get { return _concurrentWrites; }
            set { _concurrentWrites = value; }
        }

        /// <summary>
        /// Disables open-fi
        /// </summary>
        [System.ComponentModel.DefaultValue(false)]
        public bool NetworkWrites
        {
            get { return _networkWrites; }
            set { _networkWrites = value; }
        }

        /// <summary>
        /// The number of times the write is appended on the file before NLog
        /// discards the log message.
        /// </summary>
        [System.ComponentModel.DefaultValue(10)]
        public int ConcurrentWriteAttempts
        {
            get { return _concurrentWriteAttempts; }
            set { _concurrentWriteAttempts = value; }
        }

        /// <summary>
        /// Automatically <a href="filearchive.html">archive log files</a> that exceed the specified size in bytes.
        /// </summary>
        /// <remarks>
        /// Caution: Enabling this option can considerably slow down your file 
        /// logging in multi-process scenarios. If only one process is going to
        /// be writing to the file, consider setting <c>ConcurrentWrites</c>
        /// to <c>false</c> for maximum performance.
        /// </remarks>
        public long ArchiveAboveSize
        {
            get { return _archiveAboveSize; }
            set { _archiveAboveSize = value; }
        }

        /// <summary>
        /// Automatically <a href="filearchive.html">archive log files</a> every time the specified time passes.
        /// Possible options are: <c>year</c>, <c>month</c>, <c>day</c>, <c>hour</c>, <c>minute</c>. Files are 
        /// moved to the archive as part of the write operation if the current period of time changes. For example
        /// if the current <c>hour</c> changes from 10 to 11, the first write that will occur
        /// on or after 11:00 will trigger the <a href="filearchive.html">archiving</a>.
        /// </summary>
        /// <remarks>
        /// <p>
        /// Caution: Enabling this option can considerably slow down your file 
        /// logging in multi-process scenarios. If only one process is going to
        /// be writing to the file, consider setting <c>ConcurrentWrites</c>
        /// to <c>false</c> for maximum performance.
        /// </p>
        /// </remarks>
        public ArchiveEveryMode ArchiveEvery
        {
            get { return _archiveEvery; }
            set { _archiveEvery = value; }
        }

        /// <summary>
        /// The name of the file to be used for an archive. It may contain a special placeholder {#####}
        /// that will be replaced with a sequence of numbers depending on the archiving strategy.
        /// </summary>
        [AcceptsLayout]
        public string ArchiveFileName
        {
            get 
            { 
                if (_autoArchiveFileName == null)
                    return null;
                return _autoArchiveFileName.Text;
            }
            set { _autoArchiveFileName = new Layout(value); }
        }

        /// <summary>
        /// The delay in milliseconds to wait before attempting to write to the file again.
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
        [System.ComponentModel.DefaultValue(1)]
        public int ConcurrentWriteAttemptDelay
        {
            get { return _concurrentWriteAttemptDelay; }
            set { _concurrentWriteAttemptDelay = value; }
        }

        /// <summary>
        /// Maximum number of archive files that should be kept.
        /// </summary>
        [System.ComponentModel.DefaultValue(9)]
        public int MaxArchiveFiles
        {
            get { return _maxArchiveFiles; }
            set { _maxArchiveFiles = value; }
        }

        /// <summary>
        /// Determines the way <a href="filearchive.html">file archives</a> are numbered. 
        /// </summary>
        public ArchiveNumberingMode ArchiveNumbering
        {
            get { return _archiveNumbering; }
            set { _archiveNumbering = value; }
        }

        private void RecursiveRollingRename(string fileName, string pattern, int archiveNumber)
        {
            if (archiveNumber >= MaxArchiveFiles)
            {
                File.Delete(fileName);
                return;
            }

            if (!File.Exists(fileName))
                return;

            string newFileName = ReplaceNumber(pattern, archiveNumber);
            if (File.Exists(fileName))
                RecursiveRollingRename(newFileName, pattern, archiveNumber + 1);

            if (InternalLogger.IsTraceEnabled)
                InternalLogger.Trace("Renaming {0} to {1}", fileName, newFileName);
            try
            {
                File.Move(fileName, newFileName);
            }
            catch (DirectoryNotFoundException)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(newFileName));
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

            //Console.WriteLine("baseNamePatern: {0}", baseNamePattern);

            int firstPart = baseNamePattern.IndexOf("{#");
            int lastPart = baseNamePattern.IndexOf("#}") + 2;
            int trailerLength = baseNamePattern.Length - lastPart;

            string fileNameMask = baseNamePattern.Substring(0, firstPart) + "*" + baseNamePattern.Substring(lastPart);

            //Console.WriteLine("fileNameMask: {0}", fileNameMask);
            string dirName = Path.GetDirectoryName(Path.GetFullPath(pattern));
            int nextNumber = -1;
            int minNumber = -1;

            Hashtable number2name = new Hashtable();

            try
            {
                // Console.WriteLine("dirName: {0}", dirName);
                foreach (string s in Directory.GetFiles(dirName, fileNameMask))
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
                int minNumberToKeep = nextNumber - _maxArchiveFiles + 1;
                for (int i = minNumber; i < minNumberToKeep; ++i)
                {
                    string s = (string)number2name[i];
                    if (s != null)
                    {
                        File.Delete(s);
                    }
                }
            }

            string newFileName = ReplaceNumber(pattern, nextNumber);
            File.Move(fileName, newFileName);
        }

        private void DoAutoArchive(string fileName, LogEventInfo ev)
        {
            FileInfo fi = new FileInfo(fileName);
            if (!fi.Exists)
                return;

            // Console.WriteLine("DoAutoArchive({0})", fileName);
            
            string fileNamePattern;

            if (_autoArchiveFileName == null)
            {
                string ext = Path.GetExtension(fileName);
                fileNamePattern = Path.ChangeExtension(fi.FullName, ".{#}" + ext);

            }
            else
            {
                fileNamePattern = _autoArchiveFileName.GetFormattedMessage(ev);
            }

            switch (ArchiveNumbering)
            {
                case ArchiveNumberingMode.Rolling:
                    RecursiveRollingRename(fi.FullName, fileNamePattern, 0);
                    break;

                case ArchiveNumberingMode.Sequence:
                    SequentialArchive(fi.FullName, fileNamePattern);
                    break;
            }
        }

        private bool ShouldAutoArchive(string fileName, LogEventInfo ev, int upcomingWriteSize)
        {
            if (_archiveAboveSize == -1 && _archiveEvery == ArchiveEveryMode.None)
                return false;

            DateTime lastWriteTime;
            long fileLength;

            if (!GetFileInfo(fileName, out lastWriteTime, out fileLength))
                return false;

            if (_archiveAboveSize != -1)
            {
                if (fileLength + upcomingWriteSize > _archiveAboveSize)
                    return true;
            }

            if (_archiveEvery != ArchiveEveryMode.None)
            {
                string formatString;

                switch (_archiveEvery)
                {
                    case ArchiveEveryMode.Year:
                        formatString = "yyyy";
                        break;

                    case ArchiveEveryMode.Month:
                        formatString = "yyyyMM";
                        break;

                    default:
                    case ArchiveEveryMode.Day:
                        formatString = "yyyyMMdd";
                        break;

                    case ArchiveEveryMode.Hour:
                        formatString = "yyyyMMddHH";
                        break;

                    case ArchiveEveryMode.Minute:
                        formatString = "yyyyMMddHHmm";
                        break;
                }

                string ts = lastWriteTime.ToString(formatString);
                string ts2 = ev.TimeStamp.ToString(formatString);

                if (ts != ts2)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Adds all layouts used by this target to the specified collection.
        /// </summary>
        /// <param name="layouts">The collection to add layouts to.</param>
        public override void PopulateLayouts(LayoutCollection layouts)
        {
            base.PopulateLayouts (layouts);
            layouts.Add(_fileNameLayout);
        }

        /// <summary>
        /// Writes the specified logging event to a file specified in the FileName 
        /// parameter.
        /// </summary>
        /// <param name="logEvent">The logging event.</param>
        protected internal override void Write(LogEventInfo logEvent)
        {
            lock (this)
            {
                string fileName = _fileNameLayout.GetFormattedMessage(logEvent);
                string renderedText = CompiledLayout.GetFormattedMessage(logEvent) + _newLine;
                byte[] bytes = TransformBytes(_encoding.GetBytes(renderedText));

                if (ShouldAutoArchive(fileName, logEvent, bytes.Length))
                {
                    InvalidateCacheItem(fileName);
                    DoAutoArchive(fileName, logEvent);
                }

                WriteToFile(fileName, bytes);
            }
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
        protected internal override void Write(LogEventInfo[] logEvents)
        {
            Array.Sort(logEvents, 0, logEvents.Length, _logEventComparer);

            lock (this)
            {
                string currentFileName = null;
                MemoryStream ms = new MemoryStream();
                LogEventInfo firstLogEvent = null;

                for (int i = 0; i < logEvents.Length; ++i)
                {
                    LogEventInfo logEvent = logEvents[i];
                    string logEventFileName = _fileNameLayout.GetFormattedMessage(logEvent);
                    if (logEventFileName != currentFileName)
                    {
                        if (currentFileName != null)
                        {
                            if (ShouldAutoArchive(currentFileName, firstLogEvent, (int)ms.Length))
                            {
                                InvalidateCacheItem(currentFileName);
                                DoAutoArchive(currentFileName, firstLogEvent);
                            }

                            WriteToFile(currentFileName, ms.ToArray());
                        }
                        currentFileName = logEventFileName;
                        firstLogEvent = logEvent;
                        ms.SetLength(0);
                        ms.Position = 0;
                    }

                    string logEventText = CompiledLayout.GetFormattedMessage(logEvent) + _newLine;
                    byte[] bytes = TransformBytes(_encoding.GetBytes(logEventText));

                    ms.Write(bytes, 0, bytes.Length);
                }
                if (currentFileName != null)
                {
                    if (ShouldAutoArchive(currentFileName, firstLogEvent, (int)ms.Length))
                    {
                        InvalidateCacheItem(currentFileName);
                        DoAutoArchive(currentFileName, firstLogEvent);
                    }

                    WriteToFile(currentFileName, ms.ToArray());
                }
            }
        }

        /// <summary>
        /// Initializes file logging by creating data structures that
        /// enable efficient multi-file logging.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize ();

            if (!KeepFileOpen)
            {
                _appenderFactory = RetryingMultiProcessFileAppender.TheFactory;
            }
            else
            {
                if (_archiveAboveSize != -1 || _archiveEvery != ArchiveEveryMode.None)
                {
                    if (NetworkWrites)
                    {
                        _appenderFactory = RetryingMultiProcessFileAppender.TheFactory;
                    }
                    else if (ConcurrentWrites)
                    {
#if NETCF
                        _appenderFactory = RetryingMultiProcessFileAppender.TheFactory;
#elif MONO
                        //
                        // mono on Windows uses mutexes, on Unix - special appender
                        //
                        if (PlatformDetector.IsCurrentOSCompatibleWith(RuntimeOS.Unix))
                            _appenderFactory = UnixMultiProcessFileAppender.TheFactory;
                        else
                            _appenderFactory = MutexMultiProcessFileAppender.TheFactory;
#else
                        _appenderFactory = MutexMultiProcessFileAppender.TheFactory;
#endif
                    }
                    else
                    {
                        _appenderFactory = CountingSingleProcessFileAppender.TheFactory;
                    }
                }
                else
                {
                    if (NetworkWrites)
                    {
                        _appenderFactory = RetryingMultiProcessFileAppender.TheFactory;
                    }
                    else if (ConcurrentWrites)
                    {
#if NETCF
                        _appenderFactory = RetryingMultiProcessFileAppender.TheFactory;
#else
                        _appenderFactory = MutexMultiProcessFileAppender.TheFactory;
#endif
                    }
                    else
                    {
                        _appenderFactory = SingleProcessFileAppender.TheFactory;
                    }
                }
            }

            _recentAppenders = new IFileAppender[OpenFileCacheSize];
            _lastWriteTime = new DateTime[OpenFileCacheSize];
            _openTime = new DateTime[OpenFileCacheSize];

            if ((OpenFileCacheSize > 0 || EnableFileDelete) && OpenFileCacheTimeout > 0)
            {
                _autoClosingTimer = new Timer(new TimerCallback(this.AutoClosingTimerCallback), null, OpenFileCacheTimeout * 1000, OpenFileCacheTimeout * 1000);
            }

            // Console.Error.WriteLine("Name: {0} Factory: {1}", this.Name, _appenderFactory.GetType().FullName);
        }

        private void AutoClosingTimerCallback(object state)
        {
            lock (this)
            {
                try
                {
                    DateTime timeToKill = DateTime.Now.AddSeconds(-OpenFileCacheTimeout);
                    for (int i = 0; i < _recentAppenders.Length; ++i)
                    {
                        if (_recentAppenders[i] == null)
                            break;

                        if (_openTime[i] < timeToKill)
                        {
                            for (int j = i; j < _recentAppenders.Length; ++j)
                            {
                                if (_recentAppenders[j] == null)
                                    break;
                                _recentAppenders[j].Close();
                                _recentAppenders[j] = null;
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

        /// <summary>
        /// Modifies the specified byte array before it gets sent to a file.
        /// </summary>
        /// <param name="bytes">The byte array</param>
        /// <returns>The modified byte array. The function can do the modification in-place.</returns>
        protected virtual byte[] TransformBytes(byte[] bytes)
        {
            return bytes;
        }

        private void WriteToFile(string fileName, byte[] bytes)
        {
            if (_first && DeleteOldFileOnStartup)
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
            _first = false;

            if (ReplaceFileContentsOnEachWrite)
            {
                using (FileStream fs = File.Create(fileName))
                {
                    fs.Write(bytes, 0, bytes.Length);
                }
                return;
            }

            //
            // IFileAppender.Write is the most expensive operation here
            // so the in-memory data structure doesn't have to be 
            // very sophisticated. It's a table-based LRU, where we move 
            // the used element to become the first one.
            // The number of items is usually very limited so the 
            // performance should be equivalent to the one of the hashtable.
            //

            int freeSpot = _recentAppenders.Length - 1;

            for (int i = 0; i < _recentAppenders.Length; ++i)
            {
                if (_recentAppenders[i] == null)
                {
                    freeSpot = i;
                    break;
                }

                if (_recentAppenders[i].FileName == fileName)
                {
                    // found it, move it to the first place on the list
                    // (MRU)

                    IFileAppender app = _recentAppenders[i];
                    DateTime openTime = _openTime[i];

                    for (int j = i; j > 0; --j)
                    {
                        _recentAppenders[j] = _recentAppenders[j - 1];
                        _lastWriteTime[j] = _lastWriteTime[j - 1];
                        _openTime[j] = _openTime[j - 1];
                    }
                    _recentAppenders[0] = app;
                    _openTime[0] = openTime;
                    _lastWriteTime[0] = DateTime.Now;
                    app.Write(bytes);
                    return;
                }
            }

            if (_recentAppenders[freeSpot] != null)
            {
                _recentAppenders[freeSpot].Close();
                _recentAppenders[freeSpot] = null;
            }

            for (int j = freeSpot; j > 0; --j)
            {
                _recentAppenders[j] = _recentAppenders[j - 1];
                _lastWriteTime[j] = _lastWriteTime[j - 1];
                _openTime[j] = _openTime[j - 1];
            }

            _recentAppenders[0] = _appenderFactory.Open(fileName, this);
            _recentAppenders[0].Write(bytes);
            _lastWriteTime[0] = DateTime.Now;
            _openTime[0] = DateTime.Now;
        }

        /// <summary>
        /// Flushes all pending file operations.
        /// </summary>
        /// <param name="timeout">The timeout</param>
        /// <remarks>
        /// The timeout parameter is ignored, because file APIs don't provide
        /// the needed functionality.
        /// </remarks>
        public override void Flush(TimeSpan timeout)
        {
            for (int i = 0; i < _recentAppenders.Length; ++i)
            {
                if (_recentAppenders[i] == null)
                    break;
                _recentAppenders[i].Flush();
                _lastWriteTime[i] = DateTime.Now;
            }
        }

        /// <summary>
        /// Closes the file(s) opened for writing.
        /// </summary>
        protected internal override void Close()
        {
            lock (this)
            {
                if (_autoClosingTimer != null)
                {
                    _autoClosingTimer.Change(Timeout.Infinite, Timeout.Infinite);
                    _autoClosingTimer.Dispose();
                    _autoClosingTimer = null;
                }
            }

            for (int i = 0; i < _recentAppenders.Length; ++i)
            {
                if (_recentAppenders[i] == null)
                    break;
                _recentAppenders[i].Close();
                _recentAppenders[i] = null;
            }
        }

        private bool GetFileInfo(string fileName, out DateTime lastWriteTime, out long fileLength)
        {
            for (int i = 0; i < _recentAppenders.Length; ++i)
            {
                if (_recentAppenders[i] == null)
                    break;
                if (_recentAppenders[i].FileName == fileName)
                {
                    _recentAppenders[i].GetFileInfo(out lastWriteTime, out fileLength);
                    return true;
                }
            }

            FileInfo fi = new FileInfo(fileName);
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
            for (int i = 0; i < _recentAppenders.Length; ++i)
            {
                if (_recentAppenders[i] == null)
                    break;
                if (_recentAppenders[i].FileName == fileName)
                {
                    _recentAppenders[i].Close();
                    for (int j = i; j < _recentAppenders.Length - 1; ++j)
                    {
                        _recentAppenders[j] = _recentAppenders[j + 1];
                        _lastWriteTime[j] = _lastWriteTime[j + 1];
                    }
                    _recentAppenders[_recentAppenders.Length - 1] = null;
                    break;
                }
            }
        }

        class LogEventComparer : IComparer
        {
            private FileTarget _fileTarget;

            public LogEventComparer(FileTarget fileTarget)
            {
                _fileTarget = fileTarget;
            }

            public int Compare(object x, object y)
            {
                LogEventInfo le1 = (LogEventInfo)x;
                LogEventInfo le2 = (LogEventInfo)y;

                string filename1 = _fileTarget._fileNameLayout.GetFormattedMessage(le1);
                string filename2 = _fileTarget._fileNameLayout.GetFormattedMessage(le2);

                int val = String.CompareOrdinal(filename1, filename2);
                if (val != 0)
                    return val;

                if (le1.SequenceID < le2.SequenceID)
                    return -1;
                if (le1.SequenceID > le2.SequenceID)
                    return 1;
                return 0;
            }
        }

        FileStream IFileOpener.Create(string fileName, bool enableConcurrentWrite)
        {
            int currentDelay = ConcurrentWriteAttemptDelay;

            InternalLogger.Trace("Opening {0} with concurrentWrite={1}", fileName, enableConcurrentWrite);
            for (int i = 0; i < ConcurrentWriteAttempts; ++i)
            {
                try
                {
                    try
                    {
                        return TryCreate(fileName, enableConcurrentWrite);
                    }
                    catch (DirectoryNotFoundException)
                    {
                        if (!CreateDirs)
                            throw;

                        Directory.CreateDirectory(Path.GetDirectoryName(fileName));
                        return TryCreate(fileName, enableConcurrentWrite);
                    }
                }
                catch (IOException)
                {
                    if (!ConcurrentWrites || i + 1 == ConcurrentWriteAttempts)
                        throw; // rethrow

                    int actualDelay = _random.Next(currentDelay);
                    InternalLogger.Warn("Attempt #{0} to open {1} failed. Sleeping for {2}ms", i, fileName, actualDelay);
                    currentDelay *= 2;
                    System.Threading.Thread.Sleep(actualDelay);
                }
            }
            throw new Exception("Should not be reached.");
        }

#if !NETCF
        private FileStream WindowsCreateFile(string fileName, bool enableConcurrentWrite)
        {
            int fileShare = Win32FileHelper.FILE_SHARE_READ;

            if (enableConcurrentWrite)
                fileShare |= Win32FileHelper.FILE_SHARE_WRITE;

            if (EnableFileDelete)
                fileShare |= Win32FileHelper.FILE_SHARE_DELETE;

            IntPtr hFile = Win32FileHelper.CreateFile(
                fileName,
                Win32FileHelper.FileAccess.GenericWrite,
                fileShare,
                IntPtr.Zero,
                Win32FileHelper.CreationDisposition.OpenAlways,
                _fileAttributes, IntPtr.Zero);

            if (hFile.ToInt32() == -1)
                Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());

            FileStream returnValue;

#if DOTNET_2_0 || NETCF_2_0
            Microsoft.Win32.SafeHandles.SafeFileHandle safeHandle = new Microsoft.Win32.SafeHandles.SafeFileHandle(hFile, true);
            returnValue = new FileStream(safeHandle, FileAccess.Write, BufferSize);
#else
            returnValue = new FileStream(hFile, FileAccess.Write, true, BufferSize);
#endif
            returnValue.Seek(0, SeekOrigin.End);
            return returnValue;
        }
#endif

        private FileStream TryCreate(string fileName, bool enableConcurrentWrite)
        {
            FileShare fileShare = FileShare.Read;

            if (enableConcurrentWrite)
                fileShare = FileShare.ReadWrite;

#if DOTNET_2_0
            if (EnableFileDelete)
            {
                fileShare |= FileShare.Delete;
            }
#endif

#if !NETCF
            if (PlatformDetector.IsCurrentOSCompatibleWith(RuntimeOS.WindowsNT) ||
                    PlatformDetector.IsCurrentOSCompatibleWith(RuntimeOS.Windows))
            {
                return WindowsCreateFile(fileName, enableConcurrentWrite);
            }
#endif

            return new FileStream(fileName, FileMode.Append, FileAccess.Write, fileShare, BufferSize);
        }
    }
}
