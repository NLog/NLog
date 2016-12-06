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
    using Xunit.Extensions;

    public class ConcurrentFileTargetTests : NLogTestBase
    {
        private ILogger logger = LogManager.GetLogger("NLog.UnitTests.Targets.ConcurrentFileTargetTests");

        private void ConfigureSharedFile(string mode, string fileName)
        {
            var modes = mode.Split('|');

            FileTarget ft = new FileTarget();
            ft.FileName = "${basedir}/" + fileName;
            ft.Layout = "${message}";
            ft.KeepFileOpen = true;
            ft.OpenFileCacheTimeout = 10;
            ft.OpenFileCacheSize = 1;
            ft.LineEnding = LineEndingMode.LF;
            ft.ForceMutexConcurrentWrites = modes.Length == 2 && modes[1] == "mutex" ? true : false;

            var name = "ConfigureSharedFile_" + mode.Replace('|', '_') + "-wrapper";

            switch (modes[0])
            {
                case "async":
                    SimpleConfigurator.ConfigureForTargetLogging(new AsyncTargetWrapper(ft, 100, AsyncTargetWrapperOverflowAction.Grow) { Name = name }, LogLevel.Debug);
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

        public void Process(string processIndex, string numProcessesString, string numLogsString, string mode)
        {
            Thread.CurrentThread.Name = processIndex;

            int numProcesses = Convert.ToInt32(numProcessesString);
            int numLogs = Convert.ToInt32(numLogsString);
            string fileName = MakeFileName(numProcesses, numLogs, mode);

            ConfigureSharedFile(mode, fileName);

            // Having the internal logger enabled would just slow things down, reducing the 
            // likelyhood for uncovering racing conditions.
            //InternalLogger.LogLevel = LogLevel.Trace;
            //InternalLogger.LogToConsole = true;

            string format = processIndex + " {0}";

            for (int i = 0; i < numLogs; ++i)
            {
                logger.Debug(format, i);
            }

            LogManager.Configuration = null;
        }

        private string MakeFileName(int numProcesses, int numLogs, string mode)
        {
            // Having separate filenames for the various tests makes debugging easier.
            return string.Format("test_{0}_{1}_{2}.txt", numProcesses, numLogs, mode.Replace('|', '_'));
        }

        private void DoConcurrentTest(int numProcesses, int numLogs, string mode)
        {
            string logFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, MakeFileName(numProcesses, numLogs, mode));

            if (File.Exists(logFile))
                File.Delete(logFile);

            Process[] processes = new Process[numProcesses];

            for (int i = 0; i < numProcesses; ++i)
            {
                processes[i] = ProcessRunner.SpawnMethod(
                    this.GetType(),
                    "Process",
                    i.ToString(),
                    numProcesses.ToString(),
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

            int[] maxNumber = new int[numProcesses];

            Console.WriteLine("Verifying output file {0}", logFile);
            using (StreamReader sr = File.OpenText(logFile))
            {
                string line;

                while ((line = sr.ReadLine()) != null)
                {
                    string[] tokens = line.Split(' ');
                    Assert.Equal(2, tokens.Length);
                    try
                    {
                        int thread = Convert.ToInt32(tokens[0]);
                        int number = Convert.ToInt32(tokens[1]);
                        Assert.True(thread >= 0);
                        Assert.True(thread < numProcesses);
                        Assert.Equal(maxNumber[thread], number);
                        maxNumber[thread]++;
                    }
                    catch (Exception ex)
                    {
                        throw new InvalidOperationException("Error when parsing line '" + line + "'", ex);
                    }
                }
            }
        }

        [Theory]
        [InlineData(2, 10000, "none")]
        [InlineData(5, 4000, "none")]
        [InlineData(10, 2000, "none")]
        [InlineData(2, 10000, "none|mutex")]
        [InlineData(5, 4000, "none|mutex")]
        [InlineData(10, 2000, "none|mutex")]
        public void SimpleConcurrentTest(int numProcesses, int numLogs, string mode)
        {
            DoConcurrentTest(numProcesses, numLogs, mode);
        }

        [Theory]
        [InlineData("async")]
        [InlineData("async|mutex")]
        public void AsyncConcurrentTest(string mode)
        {
            // Before 2 processes are running into concurrent writes, 
            // the first process typically already has written couple thousend events.
            // Thus to have a meaningful test, at least 10K events are required.
            // Due to the buffering it makes no big difference in runtime, whether we
            // have 2 process writing 10K events each or couple more processes with even more events.
            // Runtime is mostly defined by Runner.exe compilation and JITing the first.
            DoConcurrentTest(5, 50000, mode);
        }

        [Theory]
        [InlineData("buffered")]
        [InlineData("buffered|mutex")]
        public void BufferedConcurrentTest(string mode)
        {
            DoConcurrentTest(5, 50000, mode);
        }

        [Theory]
        [InlineData("buffered_timed_flush")]
        [InlineData("buffered_timed_flush|mutex")]
        public void BufferedTimedFlushConcurrentTest(string mode)
        {
            DoConcurrentTest(5, 50000, mode);
        }
    }
}