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


#if !SILVERLIGHT

namespace NLog.UnitTests.Targets
{
    using System;
    using System.IO;
    using System.Text;

    using NUnit.Framework;

#if !NUNIT
    using TestFixture = Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute;
    using Test = Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute;
#endif
    using NLog.Config;
    using NLog.Layouts;
    using NLog.Targets;
    using NLog.Targets.Wrappers;

    using System.Threading;
    using System.Collections.Generic;

    [TestFixture]
    public class FileTargetTests : NLogTestBase
    {
        private readonly Logger logger = LogManager.GetLogger("NLog.UnitTests.Targets.FileTargetTests");

        [Test]
        public void SimpleFileTest1()
        {
            string tempFile = Path.GetTempFileName();
            try
            {
                FileTarget ft = new FileTarget
                                    {
                                        FileName = SimpleLayout.Escape(tempFile),
                                        LineEnding = LineEndingMode.LF,
                                        Layout = "${level} ${message}",
                                        OpenFileCacheTimeout = 0
                                    };

                SimpleConfigurator.ConfigureForTargetLogging(ft, LogLevel.Debug);

                logger.Debug("aaa");
                logger.Info("bbb");
                logger.Warn("ccc");
                LogManager.Configuration = null;
                AssertFileContents(tempFile, "Debug aaa\nInfo bbb\nWarn ccc\n", Encoding.UTF8);
            }
            finally
            {
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
            }
        }

        [Test]
        public void CsvHeaderTest()
        {
            // test for the following changes 
            // https://github.com/NLog/NLog/commit/e1ed0d4857dddc95d5db09ee95e9a0c85afc7810
            // codeplex ticket 6370

            string tempFile = Path.GetTempFileName();
            try
            {

                for (int i = 0; i < 2; i++)
                {
                    var layout = new CsvLayout
                    {
                        Delimiter = CsvColumnDelimiterMode.Semicolon,
                        WithHeader = true,
                        Columns =
                        {
                            new CsvColumn("name", "${logger}"),
                            new CsvColumn("level", "${level}"),
                            new CsvColumn("message", "${message}"),
                        }
                    };

                    FileTarget ft = new FileTarget
                        {
                            FileName = SimpleLayout.Escape(tempFile),
                            LineEnding = LineEndingMode.LF,
                            Layout = layout,
                            OpenFileCacheTimeout = 0,
                            ReplaceFileContentsOnEachWrite = false
                        };
                    SimpleConfigurator.ConfigureForTargetLogging(ft, LogLevel.Debug);

                    logger.Debug("aaa");
                    LogManager.Configuration = null;
                }
                AssertFileContents(tempFile, "name;level;message\nNLog.UnitTests.Targets.FileTargetTests;Debug;aaa\nNLog.UnitTests.Targets.FileTargetTests;Debug;aaa\n", Encoding.UTF8);
            }
            finally
            {
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
            }
        }

        [Test]
        public void DeleteFileOnStartTest()
        {
            string tempFile = Path.GetTempFileName();
            try
            {
                FileTarget ft = new FileTarget
                                    {
                                        FileName = SimpleLayout.Escape(tempFile),
                                        LineEnding = LineEndingMode.LF,
                                        Layout = "${level} ${message}"
                                    };

                SimpleConfigurator.ConfigureForTargetLogging(ft, LogLevel.Debug);

                logger.Debug("aaa");
                logger.Info("bbb");
                logger.Warn("ccc");

                LogManager.Configuration = null;

                AssertFileContents(tempFile, "Debug aaa\nInfo bbb\nWarn ccc\n", Encoding.UTF8);

                // configure again, without
                // DeleteOldFileOnStartup

                ft = new FileTarget
                         {
                             FileName = SimpleLayout.Escape(tempFile),
                             LineEnding = LineEndingMode.LF,
                             Layout = "${level} ${message}"
                         };

                SimpleConfigurator.ConfigureForTargetLogging(ft, LogLevel.Debug);

                logger.Debug("aaa");
                logger.Info("bbb");
                logger.Warn("ccc");

                LogManager.Configuration = null;
                AssertFileContents(tempFile, "Debug aaa\nInfo bbb\nWarn ccc\nDebug aaa\nInfo bbb\nWarn ccc\n", Encoding.UTF8);

                // configure again, this time with
                // DeleteOldFileOnStartup

                ft = new FileTarget
                         {
                             FileName = SimpleLayout.Escape(tempFile),
                             LineEnding = LineEndingMode.LF,
                             Layout = "${level} ${message}",
                             DeleteOldFileOnStartup = true
                         };

                SimpleConfigurator.ConfigureForTargetLogging(ft, LogLevel.Debug);
                logger.Debug("aaa");
                logger.Info("bbb");
                logger.Warn("ccc");

                LogManager.Configuration = null;
                AssertFileContents(tempFile, "Debug aaa\nInfo bbb\nWarn ccc\n", Encoding.UTF8);
            }
            finally
            {
                LogManager.Configuration = null;
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
            }
        }

        [Test]
        public void CreateDirsTest()
        {
            // create the file in a not-existent
            // directory which forces creation
            string tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            string tempFile = Path.Combine(tempPath, "file.txt");
            try
            {
                FileTarget ft = new FileTarget
                                    {
                                        FileName = tempFile,
                                        LineEnding = LineEndingMode.LF,
                                        Layout = "${level} ${message}"
                                    };

                SimpleConfigurator.ConfigureForTargetLogging(ft, LogLevel.Debug);

                logger.Debug("aaa");
                logger.Info("bbb");
                logger.Warn("ccc");
                LogManager.Configuration = null;
                AssertFileContents(tempFile, "Debug aaa\nInfo bbb\nWarn ccc\n", Encoding.UTF8);
            }
            finally
            {
                LogManager.Configuration = null;
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
                if (Directory.Exists(tempPath))
                    Directory.Delete(tempPath, true);
            }
        }

        [Test]
        public void SequentialArchiveTest1()
        {
            // create the file in a not-existent
            // directory which forces creation
            string tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            string tempFile = Path.Combine(tempPath, "file.txt");
            try
            {
                FileTarget ft = new FileTarget
                                    {
                                        FileName = tempFile,
                                        ArchiveFileName = Path.Combine(tempPath, "archive/{####}.txt"),
                                        ArchiveAboveSize = 1000,
                                        LineEnding = LineEndingMode.LF,
                                        Layout = "${message}",
                                        MaxArchiveFiles = 3,
                                        ArchiveNumbering = ArchiveNumberingMode.Sequence
                                    };

                SimpleConfigurator.ConfigureForTargetLogging(ft, LogLevel.Debug);

                // we emit 5 * 250 *(3 x aaa + \n) bytes
                // so that we should get a full file + 3 archives
                for (int i = 0; i < 250; ++i)
                {
                    logger.Debug("aaa");
                }
                for (int i = 0; i < 250; ++i)
                {
                    logger.Debug("bbb");
                }
                for (int i = 0; i < 250; ++i)
                {
                    logger.Debug("ccc");
                }
                for (int i = 0; i < 250; ++i)
                {
                    logger.Debug("ddd");
                }
                for (int i = 0; i < 250; ++i)
                {
                    logger.Debug("eee");
                }

                LogManager.Configuration = null;

                AssertFileContents(tempFile,
                    StringRepeat(250, "eee\n"),
                    Encoding.UTF8);

                AssertFileContents(
                    Path.Combine(tempPath, "archive/0001.txt"),
                    StringRepeat(250, "bbb\n"),
                    Encoding.UTF8);

                AssertFileContents(
                    Path.Combine(tempPath, "archive/0002.txt"),
                    StringRepeat(250, "ccc\n"),
                    Encoding.UTF8);

                AssertFileContents(
                    Path.Combine(tempPath, "archive/0003.txt"),
                    StringRepeat(250, "ddd\n"),
                    Encoding.UTF8);

                Assert.IsTrue(!File.Exists(Path.Combine(tempPath, "archive/0000.txt")));
                Assert.IsTrue(!File.Exists(Path.Combine(tempPath, "archive/0004.txt")));
            }
            finally
            {
                LogManager.Configuration = null;
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
                if (Directory.Exists(tempPath))
                    Directory.Delete(tempPath, true);
            }
        }

        [Test]
        public void RollingArchiveTest1()
        {
            // create the file in a not-existent
            // directory which forces creation
            string tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            string tempFile = Path.Combine(tempPath, "file.txt");
            try
            {
                FileTarget ft = new FileTarget
                                    {
                                        FileName = tempFile,
                                        ArchiveFileName = Path.Combine(tempPath, "archive/{####}.txt"),
                                        ArchiveAboveSize = 1000,
                                        LineEnding = LineEndingMode.LF,
                                        ArchiveNumbering = ArchiveNumberingMode.Rolling,
                                        Layout = "${message}",
                                        MaxArchiveFiles = 3
                                    };

                SimpleConfigurator.ConfigureForTargetLogging(ft, LogLevel.Debug);

                // we emit 5 * 250 * (3 x aaa + \n) bytes
                // so that we should get a full file + 3 archives
                for (int i = 0; i < 250; ++i)
                {
                    logger.Debug("aaa");
                }
                for (int i = 0; i < 250; ++i)
                {
                    logger.Debug("bbb");
                }
                for (int i = 0; i < 250; ++i)
                {
                    logger.Debug("ccc");
                }
                for (int i = 0; i < 250; ++i)
                {
                    logger.Debug("ddd");
                }
                for (int i = 0; i < 250; ++i)
                {
                    logger.Debug("eee");
                }

                LogManager.Configuration = null;

                AssertFileContents(tempFile,
                    StringRepeat(250, "eee\n"),
                    Encoding.UTF8);

                AssertFileContents(
                    Path.Combine(tempPath, "archive/0000.txt"),
                    StringRepeat(250, "ddd\n"),
                    Encoding.UTF8);

                AssertFileContents(
                    Path.Combine(tempPath, "archive/0001.txt"),
                    StringRepeat(250, "ccc\n"),
                    Encoding.UTF8);

                AssertFileContents(
                    Path.Combine(tempPath, "archive/0002.txt"),
                    StringRepeat(250, "bbb\n"),
                    Encoding.UTF8);

                Assert.IsTrue(!File.Exists(Path.Combine(tempPath, "archive/0003.txt")));
            }
            finally
            {
                LogManager.Configuration = null;
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
                if (Directory.Exists(tempPath))
                    Directory.Delete(tempPath, true);
            }
        }

        [Test]
        public void MultiFileWrite()
        {
            // create the file in a not-existent
            // directory which forces creation
            string tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            try
            {
                FileTarget ft = new FileTarget
                                    {
                                        FileName = Path.Combine(tempPath, "${level}.txt"),
                                        LineEnding = LineEndingMode.LF,
                                        Layout = "${message}"
                                    };

                SimpleConfigurator.ConfigureForTargetLogging(ft, LogLevel.Debug);

                for (int i = 0; i < 250; ++i)
                {
                    logger.Trace("@@@");
                    logger.Debug("aaa");
                    logger.Info("bbb");
                    logger.Warn("ccc");
                    logger.Error("ddd");
                    logger.Fatal("eee");
                }

                LogManager.Configuration = null;

                Assert.IsFalse(File.Exists(Path.Combine(tempPath, "Trace.txt")));

                AssertFileContents(Path.Combine(tempPath, "Debug.txt"),
                    StringRepeat(250, "aaa\n"), Encoding.UTF8);

                AssertFileContents(Path.Combine(tempPath, "Info.txt"),
                    StringRepeat(250, "bbb\n"), Encoding.UTF8);

                AssertFileContents(Path.Combine(tempPath, "Warn.txt"),
                    StringRepeat(250, "ccc\n"), Encoding.UTF8);

                AssertFileContents(Path.Combine(tempPath, "Error.txt"),
                    StringRepeat(250, "ddd\n"), Encoding.UTF8);

                AssertFileContents(Path.Combine(tempPath, "Fatal.txt"),
                    StringRepeat(250, "eee\n"), Encoding.UTF8);
            }
            finally
            {
                //if (File.Exists(tempFile))
                //    File.Delete(tempFile);
                LogManager.Configuration = null;
                if (Directory.Exists(tempPath))
                    Directory.Delete(tempPath, true);
            }
        }

        [Test]
        public void BufferedMultiFileWrite()
        {
            // create the file in a not-existent
            // directory which forces creation
            string tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            try
            {
                FileTarget ft = new FileTarget
                                    {
                                        FileName = Path.Combine(tempPath, "${level}.txt"),
                                        LineEnding = LineEndingMode.LF,
                                        Layout = "${message}"
                                    };

                SimpleConfigurator.ConfigureForTargetLogging(new BufferingTargetWrapper(ft, 10), LogLevel.Debug);

                for (int i = 0; i < 250; ++i)
                {
                    logger.Trace("@@@");
                    logger.Debug("aaa");
                    logger.Info("bbb");
                    logger.Warn("ccc");
                    logger.Error("ddd");
                    logger.Fatal("eee");
                }

                LogManager.Configuration = null;

                Assert.IsFalse(File.Exists(Path.Combine(tempPath, "Trace.txt")));

                AssertFileContents(Path.Combine(tempPath, "Debug.txt"),
                    StringRepeat(250, "aaa\n"), Encoding.UTF8);

                AssertFileContents(Path.Combine(tempPath, "Info.txt"),
                    StringRepeat(250, "bbb\n"), Encoding.UTF8);

                AssertFileContents(Path.Combine(tempPath, "Warn.txt"),
                    StringRepeat(250, "ccc\n"), Encoding.UTF8);

                AssertFileContents(Path.Combine(tempPath, "Error.txt"),
                    StringRepeat(250, "ddd\n"), Encoding.UTF8);

                AssertFileContents(Path.Combine(tempPath, "Fatal.txt"),
                    StringRepeat(250, "eee\n"), Encoding.UTF8);
            }
            finally
            {
                //if (File.Exists(tempFile))
                //    File.Delete(tempFile);
                LogManager.Configuration = null;
                if (Directory.Exists(tempPath))
                    Directory.Delete(tempPath, true);
            }
        }

        [Test]
        public void AsyncMultiFileWrite()
        {
            //InternalLogger.LogToConsole = true;
            //InternalLogger.LogLevel = LogLevel.Trace;

            string tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            try
            {
                FileTarget ft = new FileTarget
                                    {
                                        FileName = Path.Combine(tempPath, "${level}.txt"),
                                        LineEnding = LineEndingMode.LF,
                                        Layout = "${message} ${threadid}"
                                    };

                // this also checks that thread-volatile layouts
                // such as ${threadid} are properly cached and not recalculated
                // in logging threads.

                string threadID = Thread.CurrentThread.ManagedThreadId.ToString();

                //InternalLogger.LogToConsole = true;
                //InternalLogger.LogLevel = LogLevel.Trace;
                SimpleConfigurator.ConfigureForTargetLogging(new AsyncTargetWrapper(ft, 1000, AsyncTargetWrapperOverflowAction.Grow), LogLevel.Debug);
                LogManager.ThrowExceptions = true;

                for (int i = 0; i < 250; ++i)
                {
                    logger.Trace("@@@");
                    logger.Debug("aaa");
                    logger.Info("bbb");
                    logger.Warn("ccc");
                    logger.Error("ddd");
                    logger.Fatal("eee");
                }
                LogManager.Flush();
                LogManager.Configuration = null;

                Assert.IsFalse(File.Exists(Path.Combine(tempPath, "Trace.txt")));

                AssertFileContents(Path.Combine(tempPath, "Debug.txt"),
                    StringRepeat(250, "aaa " + threadID + "\n"), Encoding.UTF8);

                AssertFileContents(Path.Combine(tempPath, "Info.txt"),
                    StringRepeat(250, "bbb " + threadID + "\n"), Encoding.UTF8);

                AssertFileContents(Path.Combine(tempPath, "Warn.txt"),
                    StringRepeat(250, "ccc " + threadID + "\n"), Encoding.UTF8);

                AssertFileContents(Path.Combine(tempPath, "Error.txt"),
                    StringRepeat(250, "ddd " + threadID + "\n"), Encoding.UTF8);

                AssertFileContents(Path.Combine(tempPath, "Fatal.txt"),
                    StringRepeat(250, "eee " + threadID + "\n"), Encoding.UTF8);
            }
            finally
            {
                //if (File.Exists(tempFile))
                //    File.Delete(tempFile);
                LogManager.Configuration = null;
                if (Directory.Exists(tempPath))
                    Directory.Delete(tempPath, true);

                // Clean up configuration change, breaks onetimeonlyexceptioninhandlertest
                LogManager.ThrowExceptions = true;
            }
        }

        [Test]
        public void BatchErrorHandlingTest()
        {
            var fileTarget = new FileTarget {FileName = "${logger}", Layout = "${message}"};
            fileTarget.Initialize(null);

            // make sure that when file names get sorted, the asynchronous continuations are sorted with them as well
            var exceptions = new List<Exception>();
            var events = new[]
            {
                new LogEventInfo(LogLevel.Info, "file99.txt", "msg1").WithContinuation(exceptions.Add),
                new LogEventInfo(LogLevel.Info, "a/", "msg1").WithContinuation(exceptions.Add),
                new LogEventInfo(LogLevel.Info, "a/", "msg2").WithContinuation(exceptions.Add),
                new LogEventInfo(LogLevel.Info, "a/", "msg3").WithContinuation(exceptions.Add)
            };

            fileTarget.WriteAsyncLogEvents(events);

            Assert.AreEqual(4, exceptions.Count);
            Assert.IsNull(exceptions[0]);
            Assert.IsNotNull(exceptions[1]);
            Assert.IsNotNull(exceptions[2]);
            Assert.IsNotNull(exceptions[3]);
        }

        [Test]
        public void DisposingFileTarget_WhenNotIntialized_ShouldNotThrow()
        {
            bool exceptionThrown = false;
            var fileTarget = new FileTarget();

            try
            {
                fileTarget.Dispose();
            }
            catch
            {
                exceptionThrown = true;
            }

            Assert.IsFalse(exceptionThrown);
        }

        [Test]
        public void FileTarget_WithArchiveFileNameEndingInNumberPlaceholder_ShouldArchiveFile()
        {
            // create the file in a not-existent
            // directory which forces creation
            string tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            string tempFile = Path.Combine(tempPath, "file.txt");
            try
            {
                FileTarget ft = new FileTarget
                {
                    FileName = tempFile,
                    ArchiveFileName = Path.Combine(tempPath, "archive/test.log.{####}"),
                    ArchiveAboveSize = 1000
                };

                SimpleConfigurator.ConfigureForTargetLogging(ft, LogLevel.Debug);

                for (int i = 0; i < 100; ++i)
                {
                    logger.Debug("a");
                }

                LogManager.Configuration = null;
                Assert.IsTrue(File.Exists(tempFile));
                Assert.IsTrue(File.Exists(Path.Combine(tempPath, "archive/test.log.0000")));
            }
            finally
            {
                LogManager.Configuration = null;
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
                if (Directory.Exists(tempPath))
                    Directory.Delete(tempPath, true);
            }
        }
    }
}

#endif