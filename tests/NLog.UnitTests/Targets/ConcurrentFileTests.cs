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

using NLog;
using NLog.Config;

using NUnit.Framework;
using NLog.Targets;
using System.IO;
using System.Text;
using NLog.Targets.Wrappers;
using NLog.LayoutRenderers;
using System.Diagnostics;
using System.Threading;

namespace NLog.UnitTests.Targets
{
    [TestFixture]
	public class ConcurrentFileTargetTests : NLogTestBase
	{
        private Logger logger = LogManager.GetCurrentClassLogger();

        private void ConfigureSharedFile()
        {
            FileTarget ft = new FileTarget();
            ft.FileName = "${basedir}/file.txt";
            ft.Layout = "${threadname} ${message}";
            ft.KeepFileOpen = true;
            ft.OpenFileCacheTimeout = 10;
            ft.OpenFileCacheSize = 1;
            ft.LineEnding = FileTarget.LineEndingMode.LF;
            SimpleConfigurator.ConfigureForTargetLogging(ft, LogLevel.Debug);
        }

        public void Process(string threadName)
        {
            System.Threading.Thread.CurrentThread.Name = threadName;
            ConfigureSharedFile();
            for (int i = 0; i < 10; ++i)
            {
                string line = Console.ReadLine();
                Assert.AreEqual("go!" + i, line);
                logger.Debug("log{0}", i);
                Console.WriteLine("done!" + i);
            }
        }

        [Test]
        public void ConcurrentTest1()
        {
            string logFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "file.txt");

            if (File.Exists(logFile))
                File.Delete(logFile);

            StringBuilder expectedOutput = new StringBuilder();

            using (Process p1 = SpawnMethod("Process", "p1"))
            {
                using (Process p2 = SpawnMethod("Process", "p2"))
                {
                    using (Process p3 = SpawnMethod("Process", "p3"))
                    {
                        for (int i = 0; i < 10; ++i)
                        {
                            p1.StandardInput.WriteLine("go!" + i);
                            Assert.AreEqual("done!" + i, p1.StandardOutput.ReadLine());
                            p2.StandardInput.WriteLine("go!" + i);
                            Assert.AreEqual("done!" + i, p2.StandardOutput.ReadLine());
                            p3.StandardInput.WriteLine("go!" + i);
                            Assert.AreEqual("done!" + i, p3.StandardOutput.ReadLine());

                            expectedOutput.AppendFormat("p1 log{0}\n", i);
                            expectedOutput.AppendFormat("p2 log{0}\n", i);
                            expectedOutput.AppendFormat("p3 log{0}\n", i); 
                        }

                        p3.WaitForExit();
                        p2.WaitForExit();
                        p1.WaitForExit();
                    }
                }
            }
            AssertFileContents(logFile, expectedOutput.ToString(), Encoding.ASCII);
        }
    }
}
