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
    [Target("File")]
    public class FileTarget: AsyncTarget
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
        /// Determines whether stack trace information should be gathered
        /// during log event processing. It calls <see cref="NLog.Layout.NeedsStackTrace" /> on
        /// Layout and FileName parameters.
        /// </summary>
        /// <returns>0 - don't include stack trace<br/>1 - include stack trace without source file information<br/>2 - include full stack trace</returns>
        protected internal override int NeedsStackTrace()
        {
            return Math.Max(base.NeedsStackTrace(), _fileNameLayout.NeedsStackTrace());
        }

        /// <summary>
        /// Writes the specified logging event to a file specified in the FileName 
        /// parameter.
        /// </summary>
        /// <param name="ev">The logging event.</param>
        protected internal override void Append(LogEventInfo ev)
        {
#if !NETCF
            if (Async)
            {
				RequestQueue.Enqueue(
                        new FileWriteRequest(
                        _fileNameLayout.GetFormattedMessage(ev),
                        CompiledLayout.GetFormattedMessage(ev)));
                return;
            }
#endif

            lock (this)
            {
                string fileName = _fileNameLayout.GetFormattedMessage(ev);

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
                _outputFile.WriteLine(CompiledLayout.GetFormattedMessage(ev));
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
#if !NETCF
            StopLoggingThread();
#endif
        }

#if !NETCF
        protected override void LoggingThreadProc()
        {
            ArrayList pendingFileRequests = new ArrayList();
            while (!LoggingThreadStopRequested)
            {
                pendingFileRequests.Clear();
				RequestQueue.DequeueBatch(pendingFileRequests, 100);

                if (pendingFileRequests.Count == 0)
                {
                    Thread.Sleep(100);
                    continue;
                }

                // sort the file requests by the file name and 
                // the sequence to maximize file handle reuse

                pendingFileRequests.Sort(FileWriteRequest.GetComparer());

                /*
                InternalLogger.Debug("---");
                foreach (FileWriteRequest fwr in pendingFileRequests)
                {
                    InternalLogger.Debug("request: {0} {1}", fwr.FileName, fwr.Sequence);
                }
                */

                string currentFileName = "";
                StreamWriter currentStreamWriter = null;
                int requests = 0;
                int reopens = 0;

                for (int i = 0; i < pendingFileRequests.Count; ++i)
                {
                    FileWriteRequest fwr = (FileWriteRequest)pendingFileRequests[i];

                    if (fwr.FileName != currentFileName)
                    {
                        if (currentStreamWriter != null)
                        {
                            currentStreamWriter.Close();
                        }
                        currentFileName = fwr.FileName;
                        currentStreamWriter = OpenStreamWriter(fwr.FileName, false);
                        reopens++;
                    }
                    requests++;
                    if (currentStreamWriter != null)
                        currentStreamWriter.WriteLine(fwr.Text);
                }
                if (currentStreamWriter != null)
                {
                    currentStreamWriter.Close();
                    currentStreamWriter = null;
                }
                /*
                
                if (requests > 0)
                {
                    InternalLogger.Debug("Processed {0} requests/ {1} reopens", requests, reopens);
                }
                
                */
            }
        }

        /// <summary>
        /// Represents a single request to write to a file.
        /// </summary>
        class FileWriteRequest
        {
            private string _fileName;
            private string _text;
            private long _sequence;

            private static long _globalSequence;

            public FileWriteRequest(string fileName, string text)
            {
                _fileName = fileName;
                _text = text;
                _sequence = Interlocked.Increment(ref _globalSequence);
            }

            public string FileName
            {
                get { return _fileName; }
            }

            public string Text
            {
                get { return _text; }
            }

            public long Sequence
            {
                get { return _sequence; }
            }

            private static IComparer _comparer = new Comparer();

            public static IComparer GetComparer()
            {
                return _comparer;
            }

            class Comparer : IComparer
            {
                public int Compare(object x, object y)
                {
                    FileWriteRequest fwr1 = (FileWriteRequest)x;
                    FileWriteRequest fwr2 = (FileWriteRequest)y;

                    int val = String.CompareOrdinal(fwr1.FileName, fwr2.FileName);
                    if (val != 0)
                        return val;

                    if (fwr1.Sequence < fwr2.Sequence)
                        return -1;
                    if (fwr1.Sequence > fwr2.Sequence)
                        return 1;
                    return 0;
                }
            }
        }
#endif
    }
}
