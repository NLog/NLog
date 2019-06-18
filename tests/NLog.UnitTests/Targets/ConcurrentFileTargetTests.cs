// 
// Copyright (c) 2004-2020 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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

#if !NETSTANDARD

#define DISABLE_FILE_INTERNAL_LOGGING

namespace NLog.UnitTests.Targets
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Threading;
    using NLog.Config;
    using NLog.Targets;
    using NLog.Targets.Wrappers;
    using Xunit;
    using System.Collections.Generic;
    using System.Linq;

    using Xunit.Extensions;

    public class ConcurrentFileTargetTests : NLogTestBase
    {
        private ILogger logger = LogManager.GetLogger("NLog.UnitTests.Targets.ConcurrentFileTargetTests");

        private void ConfigureSharedFile(string mode, string fileName)
        {
            var modes = mode.Split('|');

            FileTarget ft = new FileTarget();
            ft.FileName = fileName;
            ft.Layout = "${message}";
            ft.ConcurrentWrites = true;
            ft.OpenFileCacheTimeout = 10;
            ft.OpenFileCacheSize = 1;
            ft.LineEnding = LineEndingMode.LF;
            ft.KeepFileOpen = Array.IndexOf(modes, "retry") >= 0 ? false : true;
            ft.ForceMutexConcurrentWrites = Array.IndexOf(modes, "mutex") >= 0 ? true : false;
            ft.ArchiveAboveSize = Array.IndexOf(modes, "archive") >= 0 ? 50 : -1;
            if (ft.ArchiveAboveSize > 0)
            {
                string archivePath = Path.Combine(Path.GetDirectoryName(fileName), "Archive");
                ft.ArchiveFileName = Path.Combine(archivePath, "{####}_" + Path.GetFileName(fileName));
                ft.MaxArchiveFiles = 10000;
            }

            var name = "ConfigureSharedFile_" + mode.Replace('|', '_') + "-wrapper";

            switch (modes[0])
            {
                case "async":
                    SimpleConfigurator.ConfigureForTargetLogging(new AsyncTargetWrapper(ft, 100, AsyncTargetWrapperOverflowAction.Grow) { Name = name, TimeToSleepBetweenBatches = 10 }, LogLevel.Debug);
                    break;

                case "buffered":
                    SimpleConfigurator.ConfigureForTargetLogging(new BufferingTargetWrapper(ft, 100) { Name = name }, LogLevel.Debug);
                    break;

                case "buffered_timed_flush":
                    SimpleConfigurator.ConfigureForTargetLogging(new BufferingTargetWrapper(ft, 100, 10) { Name = name }, LogLevel.Debug);
                    break;

                default:
                    SimpleConfigurator.ConfigureForTargetLogging(ft, LogLevel.Debug);
                    break;
            }
        }

#pragma warning disable xUnit1013 // Needed for test
        public void Process(string processIndex, string fileName, string numLogsString, string mode)
#pragma warning restore xUnit1013
        {
            Thread.CurrentThread.Name = processIndex;

            int numLogs = Convert.ToInt32(numLogsString);
            int idxProcess = Convert.ToInt32(processIndex);

            ConfigureSharedFile(mode, fileName);

            string format = processIndex + " {0}";

            // Having the internal logger enabled would just slow things down, reducing the 
            // likelyhood for uncovering racing conditions. Uncomment #define DISABLE_FILE_INTERNAL_LOGGING

#if !DISABLE_FILE_INTERNAL_LOGGING
            var logWriter = new StringWriter { NewLine = Environment.NewLine };
            NLog.Common.InternalLogger.LogLevel = LogLevel.Trace;
            NLog.Common.InternalLogger.LogFile = Path.Combine(Path.GetDirectoryName(fileName), string.Format("Internal_{0}.txt", processIndex));
            NLog.Common.InternalLogger.LogWriter = logWriter;
            NLog.Common.InternalLogger.LogToConsole = true;

            try
#endif
            {
                using (new NoThrowNLogExceptions())
                {
                    Thread.Sleep(Math.Max((10 - idxProcess), 1) * 5);  // Delay to wait for the other processes

                    for (int i = 0; i < numLogs; ++i)
                    {
                        logger.Debug(format, i);
                    }

                    LogManager.Configuration = null;     // Flush + Close
                }
            }
#if !DISABLE_FILE_INTERNAL_LOGGING
            catch (Exception ex)
            {
                using (var textWriter = File.AppendText(Path.Combine(Path.GetDirectoryName(fileName), string.Format("Internal_{0}.txt", processIndex))))
                {
                    textWriter.WriteLine(ex.ToString());
                    textWriter.WriteLine(logWriter.GetStringBuilder().ToString());
                }
                throw;
            }

            using (var textWriter = File.AppendText(Path.Combine(Path.GetDirectoryName(fileName), string.Format("Internal_{0}.txt", processIndex))))
            {
                textWriter.WriteLine(logWriter.GetStringBuilder().ToString());
            }
#endif
        }

        private string MakeFileName(int numProcesses, int numLogs, string mode)
        {
            // Having separate file names for the various tests makes debugging easier.
            return $"test_{numProcesses}_{numLogs}_{mode.Replace('|', '_')}.txt";
        }

        private void DoConcurrentTest(int numProcesses, int numLogs, string mode)
        {
            string tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            string archivePath = Path.Combine(tempPath, "Archive");

            try
            {
                Directory.CreateDirectory(tempPath);
                Directory.CreateDirectory(archivePath);

                string logFile = Path.Combine(tempPath, MakeFileName(numProcesses, numLogs, mode));
                if (File.Exists(logFile))
                {
                    throw new Exception($"file '{logFile}' already exists");
                }

                Process[] processes = new Process[numProcesses];

                for (int i = 0; i < numProcesses; ++i)
                {
                    processes[i] = ProcessRunner.SpawnMethod(
                        GetType(),
                        "Process",
                        i.ToString(),
                        logFile,
                        numLogs.ToString(),
                        mode);
                }

                // In case we'd like to capture stdout, we would need to drain it continuously.
                // StandardOutput.ReadToEnd() wont work, since the other processes console only has limited buffer.
                for (int i = 0; i < numProcesses; ++i)
                {
                    processes[i].WaitForExit();
                    Assert.Equal(0, processes[i].ExitCode);
                    processes[i].Dispose();
                    processes[i] = null;
                }

                var files = new System.Collections.Generic.List<string>(Directory.GetFiles(archivePath));
                files.Add(logFile);

                bool verifyFileSize = files.Count > 1;

                var receivedNumbersSet = new List<int>[numProcesses];
                for (int i = 0; i < numProcesses; i++)
                {
                    var receivedNumbers = new List<int>(numLogs);
                    receivedNumbersSet[i] = receivedNumbers;
                }

                //Console.WriteLine("Verifying output file {0}", logFile);
                foreach (var file in files)
                {

                    using (StreamReader sr = File.OpenText(file))
                    {

                        string line;
                        while ((line = sr.ReadLine()) != null)
                        {
                            string[] tokens = line.Split(' ');
                            Assert.Equal(2, tokens.Length);

                            int thread = Convert.ToInt32(tokens[0]);
                            int number = Convert.ToInt32(tokens[1]);
                            Assert.True(thread >= 0);
                            Assert.True(thread < numProcesses);
                            receivedNumbersSet[thread].Add(number);
                        }


                        if (verifyFileSize)
                        {
                            if (sr.BaseStream.Length > 100)
                                throw new InvalidOperationException(
                                    $"Error when reading file {file}, size {sr.BaseStream.Length} is too large");
                            else if (sr.BaseStream.Length < 35 && files[files.Count - 1] != file)
                                throw new InvalidOperationException(
                                    $"Error when reading file {file}, size {sr.BaseStream.Length} is too small");
                        }
                    }
                }

                var expected = Enumerable.Range(0, numLogs).ToList();

                int currentProcess = 0;
                bool? equalsWhenReorderd = null;
                try
                {
                    for (; currentProcess < numProcesses; currentProcess++)
                    {
                        var receivedNumbers = receivedNumbersSet[currentProcess];

                        var equalLength = expected.Count == receivedNumbers.Count;

                        var fastCheck = equalLength && expected.SequenceEqual(receivedNumbers);

                        if (!fastCheck)
                        //assert equals on two long lists in xUnit is lame. Not showing the difference.
                        {
                            if (equalLength)
                            {
                                var reodered = receivedNumbers.OrderBy(i => i);

                                equalsWhenReorderd = expected.SequenceEqual(reodered);
                            }

                            Assert.Equal(string.Join(",", expected), string.Join(",", receivedNumbers));
                        }
                    }
                }
                catch (Exception ex)
                {
                    var reoderProblem = equalsWhenReorderd == null ? "Dunno" : (equalsWhenReorderd == true ? "Yes" : "No");
                    throw new InvalidOperationException($"Error when comparing path {tempPath} for process {currentProcess}. Is this a recording problem? {reoderProblem}", ex);
                }

            }
            finally
            {
                try
                {
                    if (Directory.Exists(archivePath))
                        Directory.Delete(archivePath, true);
                    if (Directory.Exists(tempPath))
                        Directory.Delete(tempPath, true);
                }
                catch
                {
                }
            }
        }

        [Theory]
        [InlineData(2, 10000, "none")]
        [InlineData(5, 4000, "none")]
        [InlineData(10, 2000, "none")]
#if !MONO
        // MONO Doesn't work well with global mutex, and it is needed for successful concurrent archive operations
        [InlineData(2, 500, "none|archive")]
        [InlineData(2, 500, "none|mutex|archive")]
        [InlineData(2, 10000, "none|mutex")]
        [InlineData(5, 4000, "none|mutex")]
        [InlineData(10, 2000, "none|mutex")]
#endif
        public void SimpleConcurrentTest(int numProcesses, int numLogs, string mode)
        {
            RetryingIntegrationTest(3, () => DoConcurrentTest(numProcesses, numLogs, mode));
        }

        [Theory]
        [InlineData("async")]
#if !MONO
        [InlineData("async|mutex")]
#endif
        public void AsyncConcurrentTest(string mode)
        {
            // Before 2 processes are running into concurrent writes, 
            // the first process typically already has written couple thousand events.
            // Thus to have a meaningful test, at least 10K events are required.
            // Due to the buffering it makes no big difference in runtime, whether we
            // have 2 process writing 10K events each or couple more processes with even more events.
            // Runtime is mostly defined by Runner.exe compilation and JITing the first.
            DoConcurrentTest(5, 1000, mode);
        }

        [Theory]
        [InlineData("buffered")]
#if !MONO
        [InlineData("buffered|mutex")]
#endif
        public void BufferedConcurrentTest(string mode)
        {
            DoConcurrentTest(5, 1000, mode);
        }

        [Theory]
        [InlineData("buffered_timed_flush")]
#if !MONO
        [InlineData("buffered_timed_flush|mutex")]
#endif
        public void BufferedTimedFlushConcurrentTest(string mode)
        {
            DoConcurrentTest(5, 1000, mode);
        }
    }
}

#endif