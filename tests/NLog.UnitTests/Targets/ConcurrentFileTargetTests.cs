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

#if !SILVERLIGHT && !NET_CF

using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using NUnit.Framework;

#if !NUNIT
    using SetUp = Microsoft.VisualStudio.TestTools.UnitTesting.TestInitializeAttribute;
    using TestFixture = Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute;
    using Test = Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute;
    using TearDown =  Microsoft.VisualStudio.TestTools.UnitTesting.TestCleanupAttribute;
#endif
using NLog.Config;
using NLog.Targets;
using NLog.Targets.Wrappers;
using NLog.Common;

namespace NLog.UnitTests.Targets
{
    [TestFixture]
    public class ConcurrentFileTargetTests : NLogTestBase
	{
        private Logger logger = LogManager.GetLogger("NLog.UnitTests.Targets.ConcurrentFileTargetTests");

        private void ConfigureSharedFile(string mode)
        {
            FileTarget ft = new FileTarget();
            ft.FileName = "${basedir}/file.txt";
            ft.Layout = "${threadname} ${message}";
            ft.KeepFileOpen = true;
            ft.OpenFileCacheTimeout = 10;
            ft.OpenFileCacheSize = 1;
            ft.LineEnding = LineEndingMode.LF;

            switch (mode)
            {
                case "async":
                    SimpleConfigurator.ConfigureForTargetLogging(new AsyncTargetWrapper(ft, 100, AsyncTargetWrapperOverflowAction.Grow), LogLevel.Debug);
                    break;

                case "buffered":
                    SimpleConfigurator.ConfigureForTargetLogging(new BufferingTargetWrapper(ft, 100), LogLevel.Debug);
                    break;

                case "buffered_timed_flush":
                    SimpleConfigurator.ConfigureForTargetLogging(new BufferingTargetWrapper(ft, 100, 10), LogLevel.Debug);
                    break;

                default:
                    SimpleConfigurator.ConfigureForTargetLogging(ft, LogLevel.Debug);
                    break;
            }
        }

        public void Process(string threadName, string numLogsString, string mode)
        {
            if (threadName != null)
            {
                Thread.CurrentThread.Name = threadName;
            }

            ConfigureSharedFile(mode);
            InternalLogger.LogLevel = LogLevel.Trace;
            InternalLogger.LogToConsole = true;
            int numLogs = Convert.ToInt32(numLogsString);
            for (int i = 0; i < numLogs; ++i)
            {
                logger.Debug("{0}", i);
            }
            
            LogManager.Configuration = null;
        }

        private void DoConcurrentTest(int numProcesses, int numLogs, string mode)
        {
            string logFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "file.txt");

            if (File.Exists(logFile))
                File.Delete(logFile);

            Process[] processes = new Process[numProcesses];

            for (int i = 0; i < numProcesses; ++i)
            {
                processes[i] = ProcessRunner.SpawnMethod(this.GetType(), "Process", i.ToString(), numLogs.ToString(), mode);
            }
            
            for (int i = 0; i < numProcesses; ++i)
            {
                processes[i].WaitForExit();
                string output = processes[i].StandardOutput.ReadToEnd();
                Assert.AreEqual(0, processes[i].ExitCode, "Runner returned with an error. Standard output: " + output);
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
                    Assert.AreEqual(2, tokens.Length, "invalid output line: '" + line + "' expected two numbers.");
                    try
                    {
                        int thread = Convert.ToInt32(tokens[0]);
                        int number = Convert.ToInt32(tokens[1]);

                        Assert.AreEqual(maxNumber[thread], number);
                        maxNumber[thread]++;
                    }
                    catch (Exception ex)
                    {
                        throw new InvalidOperationException("Error when parsing line '" + line + "'", ex);
                    }
                }
            }
        }

        private void DoConcurrentTest(string mode)
        {
            DoConcurrentTest(2, 10000, mode);
            DoConcurrentTest(5, 4000, mode);
            DoConcurrentTest(10, 2000, mode);
        }

        [Test]
        public void SimpleConcurrentTest()
        {
            DoConcurrentTest("none");
        }

        [Test]
        public void AsyncConcurrentTest()
        {
            DoConcurrentTest(2, 100, "async");
        }

        [Test]
        public void BufferedConcurrentTest()
        {
            DoConcurrentTest(2, 100, "buffered");
        }

        [Test]
        public void BufferedTimedFlushConcurrentTest()
        {
            DoConcurrentTest(2, 100, "buffered_timed_flush");
        }
    }
}

#endif
