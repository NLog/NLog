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
    using System.Linq;
    using System.Text;
    using NLog.Config;
    using NLog.Layouts;
    using NLog.Targets;
    using NLog.Targets.Wrappers;

    using System.Threading;
    using System.Collections.Generic;
    using Xunit;

    public class FileTargetTests : NLogTestBase
    {
        private readonly Logger logger = LogManager.GetLogger("NLog.UnitTests.Targets.FileTargetTests");

        [Fact]
        public void SimpleFileTest1()
        {
            var tempFile = Path.GetTempFileName();
            try
            {
                var ft = new FileTarget
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

        [Fact]
        public void CsvHeaderTest()
        {
            var tempFile = Path.GetTempFileName();
            try
            {

                for (var i = 0; i < 2; i++)
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

                    var ft = new FileTarget
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

        [Fact]
        public void DeleteFileOnStartTest()
        {
            var tempFile = Path.GetTempFileName();
            try
            {
                var ft = new FileTarget
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


        [Fact]
        public void ArchiveFileOnStartTest()
        {
            var tempFile = Path.GetTempFileName();
            var tempArchiveFolder = Path.Combine(Path.GetTempPath(), "Archive");
            try
            {
                var ft = new FileTarget
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
                // ArchiveOldFileOnStartup

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
                // ArchiveldFileOnStartup                
                var archiveTempName = Path.Combine(tempArchiveFolder, "archive.txt");

                ft = new FileTarget
                {
                    FileName = SimpleLayout.Escape(tempFile),
                    LineEnding = LineEndingMode.LF,
                    Layout = "${level} ${message}",
                    ArchiveOldFileOnStartup = true,
                    ArchiveNumbering = ArchiveNumberingMode.Sequence,
                    ArchiveFileName = archiveTempName
                };

                SimpleConfigurator.ConfigureForTargetLogging(ft, LogLevel.Debug);
                logger.Debug("ddd");
                logger.Info("eee");
                logger.Warn("fff");

                LogManager.Configuration = null;
                AssertFileContents(tempFile, "Debug ddd\nInfo eee\nWarn fff\n", Encoding.UTF8);
                AssertFileContents(archiveTempName, "Debug aaa\nInfo bbb\nWarn ccc\nDebug aaa\nInfo bbb\nWarn ccc\n", Encoding.UTF8);
            }
            finally
            {
                LogManager.Configuration = null;
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
                if (Directory.Exists(tempArchiveFolder))
                    Directory.Delete(tempArchiveFolder, true);
            }
        }


        [Fact]
        public void CreateDirsTest()
        {
            var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var tempFile = Path.Combine(tempPath, "file.txt");
            try
            {
                var ft = new FileTarget
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

        [Fact]
        public void SequentialArchiveTest1()
        {
            var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var tempFile = Path.Combine(tempPath, "file.txt");
            try
            {
                var ft = new FileTarget
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
                for (var i = 0; i < 250; ++i)
                {
                    logger.Debug("aaa");
                }
                for (var i = 0; i < 250; ++i)
                {
                    logger.Debug("bbb");
                }
                for (var i = 0; i < 250; ++i)
                {
                    logger.Debug("ccc");
                }
                for (var i = 0; i < 250; ++i)
                {
                    logger.Debug("ddd");
                }
                for (var i = 0; i < 250; ++i)
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

                Assert.True(!File.Exists(Path.Combine(tempPath, "archive/0000.txt")));
                Assert.True(!File.Exists(Path.Combine(tempPath, "archive/0004.txt")));
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

        [Fact]
        public void DeleteArchiveFilesByDate()
        {
            var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var tempFile = Path.Combine(tempPath, "file.txt");
            try
            {
                var ft = new FileTarget
                {
                    FileName = tempFile,
                    ArchiveFileName = Path.Combine(tempPath, "archive/{#}.txt"),
                    ArchiveAboveSize = 50,
                    LineEnding = LineEndingMode.LF,
                    ArchiveNumbering = ArchiveNumberingMode.Date,
                    ArchiveDateFormat = "yyyyMMddHHmmssfff", //make sure the milliseconds are set in the filename
                    Layout = "${message}",
                    MaxArchiveFiles = 3
                };

                SimpleConfigurator.ConfigureForTargetLogging(ft, LogLevel.Debug);
                //writing 19 times 10 bytes (9 char + linefeed) will result in 3 archive files and 1 current file
                for (var i = 0; i < 19; ++i)
                {
                    logger.Debug("123456789");
                    //build in a small sleep to make sure the current time is reflected in the filename
                    //do this every 5 entries
                    if (i%5==0)
                        Thread.Sleep(50);
                }
                //Setting the Configuration to [null] will result in a 'Dump' of the current log entries
                LogManager.Configuration = null;

                var archivePath = Path.Combine(tempPath, "archive");
                var files = Directory.GetFiles(archivePath).OrderBy(s => s);
                //the amount of archived files may not exceed the set 'MaxArchiveFiles'
                Assert.Equal(ft.MaxArchiveFiles, files.Count());
                

                SimpleConfigurator.ConfigureForTargetLogging(ft, LogLevel.Debug);
                //writing just one line of 11 bytes will trigger the cleanup of old archived files
                //as stated by the MaxArchiveFiles property, but will only delete the oldest file
                logger.Debug("1234567890");
                LogManager.Configuration = null;

                var files2 = Directory.GetFiles(archivePath).OrderBy(s=>s);
                Assert.Equal(ft.MaxArchiveFiles, files2.Count());
                
                //the oldest file should be deleted
                Assert.DoesNotContain(files.ElementAt(0), files2);
                //two files should still be there
                Assert.Equal(files.ElementAt(1), files2.ElementAt(0));
                Assert.Equal(files.ElementAt(2), files2.ElementAt(1));
                //one new archive file shoud be created
                Assert.DoesNotContain(files2.ElementAt(2), files);
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

        [Fact]
        public void DeleteArchiveFilesByDate_AlteredMaxArchive()
        {
            var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var tempFile = Path.Combine(tempPath, "file.txt");
            try
            {
                var ft = new FileTarget
                {
                    FileName = tempFile,
                    ArchiveFileName = Path.Combine(tempPath, "archive/{#}.txt"),
                    ArchiveAboveSize = 50,
                    LineEnding = LineEndingMode.LF,
                    ArchiveNumbering = ArchiveNumberingMode.Date,
                    ArchiveDateFormat = "yyyyMMddHHmmssfff", //make sure the milliseconds are set in the filename
                    Layout = "${message}",
                    MaxArchiveFiles = 5
                };

                SimpleConfigurator.ConfigureForTargetLogging(ft, LogLevel.Debug);
                //writing 29 times 10 bytes (9 char + linefeed) will result in 3 archive files and 1 current file
                for (var i = 0; i < 29; ++i)
                {
                    logger.Debug("123456789");
                    //build in a small sleep to make sure the current time is reflected in the filename
                    //do this every 5 entries
                    if (i % 5 == 0)
                        Thread.Sleep(50);
                }
                //Setting the Configuration to [null] will result in a 'Dump' of the current log entries
                LogManager.Configuration = null;

                var archivePath = Path.Combine(tempPath, "archive");
                var files = Directory.GetFiles(archivePath).OrderBy(s => s);
                //the amount of archived files may not exceed the set 'MaxArchiveFiles'
                Assert.Equal(ft.MaxArchiveFiles, files.Count());


                //alter the MaxArchivedFiles
                ft.MaxArchiveFiles = 2;
                SimpleConfigurator.ConfigureForTargetLogging(ft, LogLevel.Debug);
                //writing just one line of 11 bytes will trigger the cleanup of old archived files
                //as stated by the MaxArchiveFiles property, but will only delete the oldest files
                logger.Debug("1234567890");
                LogManager.Configuration = null;

                var files2 = Directory.GetFiles(archivePath).OrderBy(s => s);
                Assert.Equal(ft.MaxArchiveFiles, files2.Count());

                //the oldest files should be deleted
                Assert.DoesNotContain(files.ElementAt(0), files2);                
                Assert.DoesNotContain(files.ElementAt(1), files2);
                Assert.DoesNotContain(files.ElementAt(2), files2);
                Assert.DoesNotContain(files.ElementAt(3), files2);
                //one files should still be there
                Assert.Equal(files.ElementAt(4), files2.ElementAt(0));
                //one new archive file shoud be created
                Assert.DoesNotContain(files2.ElementAt(1), files);
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

        [Fact]
        public void RepeatingHeaderTest()
        {
            var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var tempFile = Path.Combine(tempPath, "file.txt");
            try
            {
                const string header = "Headerline";

                var ft = new FileTarget
                {
                    FileName = tempFile,
                    ArchiveFileName = Path.Combine(tempPath, "archive/{####}.txt"),
                    ArchiveAboveSize = 51,
                    LineEnding = LineEndingMode.LF,
                    ArchiveNumbering = ArchiveNumberingMode.Sequence,
                    Layout = "${message}",
                    Header = header,
                    MaxArchiveFiles = 2,
                };

                SimpleConfigurator.ConfigureForTargetLogging(ft, LogLevel.Debug);

                for (var i = 0; i < 16; ++i)
                {
                    logger.Debug("123456789");
                }

                LogManager.Configuration = null;

                AssertFileContentsStartsWith(tempFile, header, Encoding.UTF8);

                AssertFileContentsStartsWith(Path.Combine(tempPath, "archive/0002.txt"), header, Encoding.UTF8);

                AssertFileContentsStartsWith(Path.Combine(tempPath, "archive/0001.txt"), header, Encoding.UTF8);

                Assert.True(!File.Exists(Path.Combine(tempPath, "archive/0000.txt")));
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

        [Fact]
        public void RollingArchiveTest1()
        {
            var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var tempFile = Path.Combine(tempPath, "file.txt");
            try
            {
                var ft = new FileTarget
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
                for (var i = 0; i < 250; ++i)
                {
                    logger.Debug("aaa");
                }
                for (var i = 0; i < 250; ++i)
                {
                    logger.Debug("bbb");
                }
                for (var i = 0; i < 250; ++i)
                {
                    logger.Debug("ccc");
                }
                for (var i = 0; i < 250; ++i)
                {
                    logger.Debug("ddd");
                }
                for (var i = 0; i < 250; ++i)
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

                Assert.True(!File.Exists(Path.Combine(tempPath, "archive/0003.txt")));
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

        [Fact]
        public void MultiFileWrite()
        {
            var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            try
            {
                var ft = new FileTarget
                                    {
                                        FileName = Path.Combine(tempPath, "${level}.txt"),
                                        LineEnding = LineEndingMode.LF,
                                        Layout = "${message}"
                                    };

                SimpleConfigurator.ConfigureForTargetLogging(ft, LogLevel.Debug);

                for (var i = 0; i < 250; ++i)
                {
                    logger.Trace("@@@");
                    logger.Debug("aaa");
                    logger.Info("bbb");
                    logger.Warn("ccc");
                    logger.Error("ddd");
                    logger.Fatal("eee");
                }

                LogManager.Configuration = null;

                Assert.False(File.Exists(Path.Combine(tempPath, "Trace.txt")));

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

        [Fact]
        public void BufferedMultiFileWrite()
        {
            var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            try
            {
                var ft = new FileTarget
                                    {
                                        FileName = Path.Combine(tempPath, "${level}.txt"),
                                        LineEnding = LineEndingMode.LF,
                                        Layout = "${message}"
                                    };

                SimpleConfigurator.ConfigureForTargetLogging(new BufferingTargetWrapper(ft, 10), LogLevel.Debug);

                for (var i = 0; i < 250; ++i)
                {
                    logger.Trace("@@@");
                    logger.Debug("aaa");
                    logger.Info("bbb");
                    logger.Warn("ccc");
                    logger.Error("ddd");
                    logger.Fatal("eee");
                }

                LogManager.Configuration = null;

                Assert.False(File.Exists(Path.Combine(tempPath, "Trace.txt")));

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

        [Fact]
        public void AsyncMultiFileWrite()
        {
            var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            try
            {
                var ft = new FileTarget
                                    {
                                        FileName = Path.Combine(tempPath, "${level}.txt"),
                                        LineEnding = LineEndingMode.LF,
                                        Layout = "${message} ${threadid}"
                                    };

                // this also checks that thread-volatile layouts
                // such as ${threadid} are properly cached and not recalculated
                // in logging threads.

                var threadID = Thread.CurrentThread.ManagedThreadId.ToString();

                SimpleConfigurator.ConfigureForTargetLogging(new AsyncTargetWrapper(ft, 1000, AsyncTargetWrapperOverflowAction.Grow), LogLevel.Debug);
                LogManager.ThrowExceptions = true;

                for (var i = 0; i < 250; ++i)
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

                Assert.False(File.Exists(Path.Combine(tempPath, "Trace.txt")));

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

        [Fact]
        public void BatchErrorHandlingTest()
        {
            var fileTarget = new FileTarget { FileName = "${logger}", Layout = "${message}" };
            fileTarget.Initialize(null);

            // make sure that when file names get sorted, the asynchronous continuations are sorted with them as well
            var exceptions = new List<Exception>();
            var events = new[]
            {
                new LogEventInfo(LogLevel.Info, "file99.txt", "msg1").WithContinuation(exceptions.Add),
                new LogEventInfo(LogLevel.Info, "", "msg1").WithContinuation(exceptions.Add),
                new LogEventInfo(LogLevel.Info, "", "msg2").WithContinuation(exceptions.Add),
                new LogEventInfo(LogLevel.Info, "", "msg3").WithContinuation(exceptions.Add)
            };

            fileTarget.WriteAsyncLogEvents(events);

            Assert.Equal(4, exceptions.Count);
            Assert.Null(exceptions[0]);
            Assert.NotNull(exceptions[1]);
            Assert.NotNull(exceptions[2]);
            Assert.NotNull(exceptions[3]);
        }

        [Fact]
        public void DisposingFileTarget_WhenNotIntialized_ShouldNotThrow()
        {
            var exceptionThrown = false;
            var fileTarget = new FileTarget();

            try
            {
                fileTarget.Dispose();
            }
            catch
            {
                exceptionThrown = true;
            }

            Assert.False(exceptionThrown);
        }

        [Fact]
        public void FileTarget_WithArchiveFileNameEndingInNumberPlaceholder_ShouldArchiveFile()
        {
            var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var tempFile = Path.Combine(tempPath, "file.txt");
            try
            {
                var ft = new FileTarget
                {
                    FileName = tempFile,
                    ArchiveFileName = Path.Combine(tempPath, "archive/test.log.{####}"),
                    ArchiveAboveSize = 1000
                };

                SimpleConfigurator.ConfigureForTargetLogging(ft, LogLevel.Debug);

                for (var i = 0; i < 100; ++i)
                {
                    logger.Debug("a");
                }

                LogManager.Configuration = null;
                Assert.True(File.Exists(tempFile));
                Assert.True(File.Exists(Path.Combine(tempPath, "archive/test.log.0000")));
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

        [Fact]
        public void FileTarget_InvalidFileNameCorrection()
        {            
            var tempFile = Path.GetTempFileName();
            var invalidTempFile = tempFile + Path.GetInvalidFileNameChars()[0];
            var expectedCorrectedTempFile = tempFile + "_";

            try
            {
                var ft = new FileTarget
                {
                    FileName = SimpleLayout.Escape(invalidTempFile),
                    LineEnding = LineEndingMode.LF,
                    Layout = "${level} ${message}",
                    OpenFileCacheTimeout = 0
                };

                SimpleConfigurator.ConfigureForTargetLogging(ft, LogLevel.Fatal);

                logger.Fatal("aaa");
                LogManager.Configuration = null;
                AssertFileContents(expectedCorrectedTempFile, "Fatal aaa\n", Encoding.UTF8);
            }
            finally
            {
                if (File.Exists(invalidTempFile))
                    File.Delete(invalidTempFile); 
                if (File.Exists(expectedCorrectedTempFile))
                    File.Delete(expectedCorrectedTempFile);             
            }
        }
    }
}

#endif