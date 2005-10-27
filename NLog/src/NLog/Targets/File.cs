// 
// Copyright (c) 2004,2005 Jaroslaw Kowalski <jkowalski@users.sourceforge.net>
// 
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
// * Neither the name of the Jaroslaw Kowalski nor the names of its 
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
    /// <xml src="examples/targets/File/FileTarget.nlog" />
    /// <p>
    /// You can use a single target to write to multiple files. The following
    /// example writes each log message to a file named after its log level, so
    /// it will create:
    /// <c>Trace.log</c>, <c>Debug.log</c>, <c>Info.log</c>, <c>Warn.log</c>, 
    /// <c>Error.log</c>, <c>Fatal.log</c>
    /// </p>
    /// <xml src="examples/targets/File/FileTargetMultiple.nlog" />
    /// <p>
    /// The file names can be quite complex for the most demanding scenarios. This
    /// example shows a way to create separate files for each day, user and log level.
    /// As you can see, the possibilities are endless.
    /// </p>
    /// <xml src="examples/targets/File/FileTargetMultiple2.nlog" />
    /// <p>
    /// Depending on your usage scenario it may be useful to add an <a href="target.AsyncWrapper.html">asynchronous target wrapper</a>
    /// around the file target. This way all your log messages
    /// will be written in a separate thread so your main thread can finish
    /// your work more quickly. Asynchronous logging is recommended
    /// for multi-threaded server applications which run for a long time and
    /// is not recommended for quickly-finishing command line applications.
    /// </p>
    /// <xml src="examples/targets/File/FileTargetAsync.nlog" />
    /// <p>
    /// The above examples assume just one target and a single rule. See below for
    /// a programmatic configuration that's equivalent to the above config file:
    /// </p>
    /// <cs src="examples/targets/File/FileTargetAsync.cs" />
    /// <p>
    /// More configuration options are described <a href="config.html">here</a>.
    /// </p>
    /// <p>
    /// To set up the log target programmatically use code like this:
    /// </p>
    /// <cs src="examples/targets/File/FileTarget.cs" />
    /// </example>
    [Target("File")]
    public class FileTarget: Target
    {
        private Random _random = new Random();
        private Layout _fileNameLayout;
        private bool _createDirs = false;
        private bool _keepFileOpen = false;
        private string _lastFileName = String.Empty;
        private StreamWriter _outputFile;
        private System.Text.Encoding _encoding = System.Text.Encoding.Default;
        private bool _autoFlush = true;
        private bool _concurrentWrites = true;
        private int _concurrentWriteAttempts = 10;
        private int _bufferSize = 32768;
        private int _concurrentWriteAttemptDelay = 1;
        private LogEventComparer _logEventComparer;

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
            get
            {
                return _fileNameLayout.Text;
            }
            set
            {
                _fileNameLayout = new Layout(value);
            }
        }

        /// <summary>
        /// Create directories if they don't exist.
        /// </summary>
        [System.ComponentModel.DefaultValue(false)]
        public bool CreateDirs
        {
            get
            {
                return _createDirs;
            }
            set
            {
                _createDirs = value;
            }
        }

        /// <summary>
        /// Keep log file open instead of opening and closing it on each logging event.
        /// </summary>
        /// <remarks>
        /// Setting this property to <c>True</c> helps improve performance but is not recommended in multithreaded or multiprocess
        /// scenarios because the file is kept locked and other processes cannot write to it which
        /// effectively prevents logging.
        /// </remarks>
        [System.ComponentModel.DefaultValue(false)]
        public bool KeepFileOpen
        {
            get
            {
                return _keepFileOpen;
            }
            set
            {
                _keepFileOpen = value;
            }
        }

        /// <summary>
        /// Automatically flush the file buffers after each log message.
        /// </summary>
        [System.ComponentModel.DefaultValue(true)]
        public bool AutoFlush
        {
            get
            {
                return _autoFlush;
            }
            set
            {
                _autoFlush = value;
            }
        }

        /// <summary>
        /// Log file buffer size in bytes.
        /// </summary>
        [System.ComponentModel.DefaultValue(32768)]
        public int BufferSize
        {
            get
            {
                return _bufferSize;
            }
            set
            {
                _bufferSize = value;
            }
        }

        /// <summary>
        /// File encoding.</summary>
        /// <remarks>
        /// Can be any encoding name supported by System.Text.Encoding.GetEncoding() e.g. <c>windows-1252</c>, <c>iso-8859-2</c>.
        /// </remarks>
        public string Encoding
        {
            get
            {
                return _encoding.WebName;
            }
            set
            {
                _encoding = System.Text.Encoding.GetEncoding(value);
            }
        }

        /// <summary>
        /// Enables concurrent writes to the log file by multiple processes.
        /// </summary>
        /// <remarks>
        /// This prevents the log files from being kept open and makes NLog
        /// retry file writes until a write succeeds. This allows for logging in
        /// multiprocess environment.
        /// </remarks>
        [System.ComponentModel.DefaultValue(true)]
        public bool ConcurrentWrites
        {
            get
            {
                return _concurrentWrites;
            }
            set
            {
                _concurrentWrites = value;
            }
        }

        /// <summary>
        /// The number of times the write is appended on the file before NLog
        /// discards the log message.
        /// </summary>
        [System.ComponentModel.DefaultValue(10)]
        public int ConcurrentWriteAttempts
        {
            get
            {
                return _concurrentWriteAttempts;
            }
            set
            {
                _concurrentWriteAttempts = value;
            }
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
            get
            {
                return _concurrentWriteAttemptDelay;
            }
            set
            {
                _concurrentWriteAttemptDelay = value;
            }
        }

        private StreamWriter OpenStreamWriter(string fileName, bool throwOnError)
        {
            try
            {
                StreamWriter retVal;

                FileInfo fi = new FileInfo(fileName);
                if (!fi.Exists)
                {
                    if (!fi.Directory.Exists)
                    {
                        Directory.CreateDirectory(fi.DirectoryName);
                    }
                }

                if (!ConcurrentWrites)
                {
                    retVal = new StreamWriter(fileName, true, _encoding, _bufferSize);
                }
                else
                {
                    int currentDelay = _concurrentWriteAttemptDelay;
                    retVal = null;

                    for (int i = 0; i < _concurrentWriteAttempts; ++i)
                    {
                        try
                        {
                            retVal = new StreamWriter(fileName, true, _encoding, _bufferSize);
                            break;
                        }
                        catch (IOException)
                        {
                            // Console.WriteLine("ex: {0}", ex.Message);
                            int actualDelay = _random.Next(currentDelay);
                            currentDelay *= 2;
                            System.Threading.Thread.Sleep(actualDelay);
                        }
                    }
                }

                retVal.AutoFlush = _autoFlush;
                return retVal;
            }
            catch (Exception ex)
            {
                InternalLogger.Error("Unable to create file: '{0}' {1}", fileName, ex);
                throw;
            }
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

                if (fileName != _lastFileName && _outputFile != null)
                {
                    _outputFile.Close();
                    _outputFile = null;
                }
                
                _lastFileName = fileName;
                if (_outputFile == null)
                {
                    _outputFile = OpenStreamWriter(fileName, true);
                    if (_outputFile == null)
                        return ;
                }
                WriteToFile(_outputFile, CompiledLayout.GetFormattedMessage(logEvent));
                if (AutoFlush)
                {
                    _outputFile.Flush();
                }
                if (!KeepFileOpen || ConcurrentWrites)
                {
                    _outputFile.Close();
                    _outputFile = null;
                }
            }
        }

        /// <summary>
        /// Closes the file.
        /// </summary>
        protected internal override void Close()
        {
            lock (this)
            {
                if (_outputFile != null)
                {
                    _outputFile.Close();
                    _outputFile = null;
                }
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
            string currentFileName = "";
            int requests = 0;
            int reopens = 0;
            StreamWriter currentStreamWriter = null;

            try
            {
                Array.Sort(logEvents, 0, logEvents.Length, _logEventComparer);

                for (int i = 0; i < logEvents.Length; ++i)
                {
                    LogEventInfo logEvent = logEvents[i];
                    string logEventFileName = _fileNameLayout.GetFormattedMessage(logEvent);
                    string logEventText = CompiledLayout.GetFormattedMessage(logEvent);

                    if (logEventFileName != currentFileName)
                    {
                        if (currentStreamWriter != null)
                        {
                            currentStreamWriter.Close();
                        }
                        currentFileName = logEventFileName;
                        currentStreamWriter = OpenStreamWriter(logEventFileName, false);
                        reopens++;
                    }
                    requests++;
                    if (currentStreamWriter != null)
                    {
                        WriteToFile(currentStreamWriter, logEventText);
                    }
                }
            }
            finally
            {
                if (currentStreamWriter != null)
                {
                    currentStreamWriter.Close();
                    currentStreamWriter = null;
                }
            }
        }

        /// <summary>
        /// Writes the specified text to the specified file.
        /// </summary>
        /// <param name="file">file to write to</param>
        /// <param name="text">text to be written</param>
        /// <remarks>
        /// You can override this method to additional things before
        /// the text is actually written to the file. For example this
        /// is a way to add encryption support.
        /// </remarks>
        protected virtual void WriteToFile(StreamWriter file, string text)
        {
            file.WriteLine(text);
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
    }
}
