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
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using Xunit;
    using Xunit.Extensions;

    using Mocks;
    using NLog.Config;
    using NLog.Layouts;
    using NLog.Targets;
    using NLog.Targets.Wrappers;
    using NLog.Time;
    using System.Globalization;

    public abstract class FileTargetTests : NLogTestBase
    {
        private readonly ILogger logger = LogManager.GetLogger("NLog.UnitTests.Targets.FileTargetTests");

        public static IEnumerable<object[]> SimpleFileTest_TestParameters
        {
            get
            {
                var booleanValues = new[] { true, false };
                return
                    from concurrentWrites in booleanValues
                    from keepFileOpen in booleanValues
                    from networkWrites in booleanValues
                    select new object[] { concurrentWrites, keepFileOpen, networkWrites };
            }
        }

        [Theory]
        [PropertyData("SimpleFileTest_TestParameters")]
        public void SimpleFileTest(bool concurrentWrites, bool keepFileOpen, bool networkWrites)
        {
            var logFile = Path.GetTempFileName();
            try
            {
                var fileTarget = WrapFileTarget(new FileTarget
                {
                    FileName = SimpleLayout.Escape(logFile),
                    LineEnding = LineEndingMode.LF,
                    Layout = "${level} ${message}",
                    OpenFileCacheTimeout = 0,
                    ConcurrentWrites = concurrentWrites,
                    KeepFileOpen = keepFileOpen,
                    NetworkWrites = networkWrites
                });

                SimpleConfigurator.ConfigureForTargetLogging(fileTarget, LogLevel.Debug);

                logger.Debug("aaa");
                logger.Info("bbb");
                logger.Warn("ccc");
                LogManager.Configuration = null;
                AssertFileContents(logFile, "Debug aaa\nInfo bbb\nWarn ccc\n", Encoding.UTF8);
            }
            finally
            {
                if (File.Exists(logFile))
                    File.Delete(logFile);
            }
        }

        /// <summary>
        /// There was a bug when creating the file in the root.
        /// 
        /// Please note that this test can fail because the unit test doesn't have write access in the root.
        /// </summary>
        [Fact]
        public void SimpleFileTestInRoot()
        {

            if (NLog.Internal.PlatformDetector.IsWin32)
            {
                var logFile = "c:\\nlog-test.log";
                try
                {
                    var fileTarget = WrapFileTarget(new FileTarget
                    {
                        FileName = SimpleLayout.Escape(logFile),
                        LineEnding = LineEndingMode.LF,
                        Layout = "${level} ${message}",
                    });

                    SimpleConfigurator.ConfigureForTargetLogging(fileTarget, LogLevel.Debug);

                    logger.Debug("aaa");
                    logger.Info("bbb");
                    logger.Warn("ccc");
                    LogManager.Configuration = null;
                    AssertFileContents(logFile, "Debug aaa\nInfo bbb\nWarn ccc\n", Encoding.UTF8);
                }
                finally
                {
                    if (File.Exists(logFile))
                        File.Delete(logFile);
                }
            }
        }

        [Fact]
        public void CsvHeaderTest()
        {
            var logFile = Path.GetTempFileName();
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

                    var fileTarget = WrapFileTarget(new FileTarget
                    {
                        FileName = SimpleLayout.Escape(logFile),
                        LineEnding = LineEndingMode.LF,
                        Layout = layout,
                        OpenFileCacheTimeout = 0,
                        ReplaceFileContentsOnEachWrite = false
                    });
                    SimpleConfigurator.ConfigureForTargetLogging(fileTarget, LogLevel.Debug);

                    logger.Debug("aaa");
                    LogManager.Configuration = null;
                }
                AssertFileContents(logFile, "name;level;message\nNLog.UnitTests.Targets.FileTargetTests;Debug;aaa\nNLog.UnitTests.Targets.FileTargetTests;Debug;aaa\n", Encoding.UTF8);
            }
            finally
            {
                if (File.Exists(logFile))
                    File.Delete(logFile);
            }
        }

        [Fact]
        public void DeleteFileOnStartTest()
        {
            var logFile = Path.GetTempFileName();
            try
            {
                var fileTarget = WrapFileTarget(new FileTarget
                {
                    DeleteOldFileOnStartup = false,
                    FileName = SimpleLayout.Escape(logFile),
                    LineEnding = LineEndingMode.LF,
                    Layout = "${level} ${message}"
                });

                SimpleConfigurator.ConfigureForTargetLogging(fileTarget, LogLevel.Debug);

                logger.Debug("aaa");
                logger.Info("bbb");
                logger.Warn("ccc");

                LogManager.Configuration = null;

                AssertFileContents(logFile, "Debug aaa\nInfo bbb\nWarn ccc\n", Encoding.UTF8);

                // configure again, without
                // DeleteOldFileOnStartup

                fileTarget = WrapFileTarget(new FileTarget
                {
                    DeleteOldFileOnStartup = false,
                    FileName = SimpleLayout.Escape(logFile),
                    LineEnding = LineEndingMode.LF,
                    Layout = "${level} ${message}"
                });

                SimpleConfigurator.ConfigureForTargetLogging(fileTarget, LogLevel.Debug);

                logger.Debug("aaa");
                logger.Info("bbb");
                logger.Warn("ccc");

                LogManager.Configuration = null;
                AssertFileContents(logFile, "Debug aaa\nInfo bbb\nWarn ccc\nDebug aaa\nInfo bbb\nWarn ccc\n", Encoding.UTF8);

                // configure again, this time with
                // DeleteOldFileOnStartup

                fileTarget = WrapFileTarget(new FileTarget
                {
                    FileName = SimpleLayout.Escape(logFile),
                    LineEnding = LineEndingMode.LF,
                    Layout = "${level} ${message}",
                    DeleteOldFileOnStartup = true
                });

                SimpleConfigurator.ConfigureForTargetLogging(fileTarget, LogLevel.Debug);
                logger.Debug("aaa");
                logger.Info("bbb");
                logger.Warn("ccc");

                LogManager.Configuration = null;
                AssertFileContents(logFile, "Debug aaa\nInfo bbb\nWarn ccc\n", Encoding.UTF8);
            }
            finally
            {
                if (File.Exists(logFile))
                    File.Delete(logFile);
            }
        }

        [Fact]
        public void ArchiveFileOnStartTest()
        {
            ArchiveFileOnStartTests(enableCompression: false);
        }

#if NET4_5
        [Fact]
        public void ArchiveFileOnStartTest_WithCompression()
        {
            ArchiveFileOnStartTests(enableCompression: true);
        }
#endif

        private void ArchiveFileOnStartTests(bool enableCompression)
        {
            var logFile = Path.GetTempFileName();
            var tempArchiveFolder = Path.Combine(Path.GetTempPath(), "Archive");
            var archiveExtension = enableCompression ? "zip" : "txt";
            try
            {
                // Configure first time with ArchiveOldFileOnStartup = false. 
                var fileTarget = WrapFileTarget(new FileTarget
                {
                    ArchiveOldFileOnStartup = false,
                    FileName = SimpleLayout.Escape(logFile),
                    LineEnding = LineEndingMode.LF,
                    Layout = "${level} ${message}"
                });

                SimpleConfigurator.ConfigureForTargetLogging(fileTarget, LogLevel.Debug);

                logger.Debug("aaa");
                logger.Info("bbb");
                logger.Warn("ccc");

                LogManager.Configuration = null;

                AssertFileContents(logFile, "Debug aaa\nInfo bbb\nWarn ccc\n", Encoding.UTF8);

                // Configure second time with ArchiveOldFileOnStartup = false again. 
                // Expected behavior: Extra content to be appended to the file.
                fileTarget = WrapFileTarget(new FileTarget
                {
                    ArchiveOldFileOnStartup = false,
                    FileName = SimpleLayout.Escape(logFile),
                    LineEnding = LineEndingMode.LF,
                    Layout = "${level} ${message}"
                });

                SimpleConfigurator.ConfigureForTargetLogging(fileTarget, LogLevel.Debug);

                logger.Debug("aaa");
                logger.Info("bbb");
                logger.Warn("ccc");

                LogManager.Configuration = null;
                AssertFileContents(logFile, "Debug aaa\nInfo bbb\nWarn ccc\nDebug aaa\nInfo bbb\nWarn ccc\n", Encoding.UTF8);


                // Configure third time with ArchiveOldFileOnStartup = true again. 
                // Expected behavior: Extra content will be stored in a new file; the 
                //      old content should be moved into a new location.

                var archiveTempName = Path.Combine(tempArchiveFolder, "archive." + archiveExtension);

                fileTarget = WrapFileTarget(new FileTarget
                {
#if NET4_5
                    EnableArchiveFileCompression = enableCompression,
#endif
                    FileName = SimpleLayout.Escape(logFile),
                    LineEnding = LineEndingMode.LF,
                    Layout = "${level} ${message}",
                    ArchiveOldFileOnStartup = true,
                    ArchiveFileName = archiveTempName,
                    ArchiveNumbering = ArchiveNumberingMode.Sequence,
                    MaxArchiveFiles = 1
                });

                SimpleConfigurator.ConfigureForTargetLogging(fileTarget, LogLevel.Debug);
                logger.Debug("ddd");
                logger.Info("eee");
                logger.Warn("fff");

                LogManager.Configuration = null;
                AssertFileContents(logFile, "Debug ddd\nInfo eee\nWarn fff\n", Encoding.UTF8);
                Assert.True(File.Exists(archiveTempName));

                var assertFileContents =
#if NET4_5
 enableCompression ? new Action<string, string, Encoding>(AssertZipFileContents) : AssertFileContents;
#else
 new Action<string, string, Encoding>(AssertFileContents);
#endif
                assertFileContents(archiveTempName, "Debug aaa\nInfo bbb\nWarn ccc\nDebug aaa\nInfo bbb\nWarn ccc\n",
                    Encoding.UTF8);
            }
            finally
            {
                if (File.Exists(logFile))
                    File.Delete(logFile);
                if (Directory.Exists(tempArchiveFolder))
                    Directory.Delete(tempArchiveFolder, true);
            }
        }

        public static IEnumerable<object[]> ReplaceFileContentsOnEachWriteTest_TestParameters
        {
            get
            {
                bool[] boolValues = new[] { false, true };
                return
                    from useHeader in boolValues
                    from useFooter in boolValues
                    select new object[] { useHeader, useFooter };
            }
        }

        [Theory]
        [PropertyData("ReplaceFileContentsOnEachWriteTest_TestParameters")]
        public void ReplaceFileContentsOnEachWriteTest(bool useHeader, bool useFooter)
        {
            const string header = "Headerline", footer = "Footerline";

            var logFile = Path.GetTempFileName();
            try
            {
                var innerFileTarget = new FileTarget
                {
                    DeleteOldFileOnStartup = false,
                    FileName = SimpleLayout.Escape(logFile),
                    ReplaceFileContentsOnEachWrite = true,
                    LineEnding = LineEndingMode.LF,
                    Layout = "${level} ${message}"
                };
                if (useHeader)
                    innerFileTarget.Header = header;
                if (useFooter)
                    innerFileTarget.Footer = footer;
                var fileTarget = WrapFileTarget(innerFileTarget);

                SimpleConfigurator.ConfigureForTargetLogging(fileTarget, LogLevel.Debug);

                string headerPart = useHeader ? header + LineEndingMode.LF.NewLineCharacters : string.Empty;
                string footerPart = useFooter ? footer + LineEndingMode.LF.NewLineCharacters : string.Empty;

                logger.Debug("aaa");
                AssertFileContents(logFile, headerPart + "Debug aaa\n" + footerPart, Encoding.UTF8);
                logger.Info("bbb");
                AssertFileContents(logFile, headerPart + "Info bbb\n" + footerPart, Encoding.UTF8);
                logger.Warn("ccc");
                AssertFileContents(logFile, headerPart + "Warn ccc\n" + footerPart, Encoding.UTF8);

            }
            finally
            {
                if (File.Exists(logFile))
                    File.Delete(logFile);
            }
        }

        [Fact]
        public void CreateDirsTest()
        {
            var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var logFile = Path.Combine(tempPath, "file.txt");
            try
            {
                var fileTarget = WrapFileTarget(new FileTarget
                {
                    FileName = logFile,
                    LineEnding = LineEndingMode.LF,
                    Layout = "${level} ${message}"
                });

                SimpleConfigurator.ConfigureForTargetLogging(fileTarget, LogLevel.Debug);

                logger.Debug("aaa");
                logger.Info("bbb");
                logger.Warn("ccc");
                LogManager.Configuration = null;
                AssertFileContents(logFile, "Debug aaa\nInfo bbb\nWarn ccc\n", Encoding.UTF8);
            }
            finally
            {
                LogManager.Configuration = null;
                if (File.Exists(logFile))
                    File.Delete(logFile);
                if (Directory.Exists(tempPath))
                    Directory.Delete(tempPath, true);
            }
        }

        [Fact]
        public void SequentialArchiveTest()
        {
            var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var logFile = Path.Combine(tempPath, "file.txt");
            try
            {
                string archiveFolder = Path.Combine(tempPath, "archive");
                var fileTarget = WrapFileTarget(new FileTarget
                {
                    FileName = logFile,
                    ArchiveFileName = Path.Combine(archiveFolder, "{####}.txt"),
                    ArchiveAboveSize = 1000,
                    LineEnding = LineEndingMode.LF,
                    Layout = "${message}",
                    MaxArchiveFiles = 3,
                    ArchiveNumbering = ArchiveNumberingMode.Sequence
                });

                SimpleConfigurator.ConfigureForTargetLogging(fileTarget, LogLevel.Debug);

                // we emit 5 * 250 *(3 x aaa + \n) bytes
                // so that we should get a full file + 3 archives
                Generate1000BytesLog('a');
                Generate1000BytesLog('b');
                Generate1000BytesLog('c');
                Generate1000BytesLog('d');
                Generate1000BytesLog('e');

                LogManager.Configuration = null;

                AssertFileContents(logFile,
                    StringRepeat(250, "eee\n"),
                    Encoding.UTF8);

                AssertFileContents(
                    Path.Combine(archiveFolder, "0001.txt"),
                    StringRepeat(250, "bbb\n"),
                    Encoding.UTF8);

                AssertFileContents(
                    Path.Combine(archiveFolder, "0002.txt"),
                    StringRepeat(250, "ccc\n"),
                    Encoding.UTF8);

                AssertFileContents(
                    Path.Combine(archiveFolder, "0003.txt"),
                    StringRepeat(250, "ddd\n"),
                    Encoding.UTF8);
                //0000 should not extists because of MaxArchiveFiles=3
                Assert.True(!File.Exists(Path.Combine(archiveFolder, "0000.txt")));
                Assert.True(!File.Exists(Path.Combine(archiveFolder, "0004.txt")));
            }
            finally
            {
                if (File.Exists(logFile))
                    File.Delete(logFile);
                if (Directory.Exists(tempPath))
                    Directory.Delete(tempPath, true);
            }
        }

        [Fact]
        public void SequentialArchiveTest_MaxArchiveFiles_0()
        {
            var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var logFile = Path.Combine(tempPath, "file.txt");
            try
            {
                string archiveFolder = Path.Combine(tempPath, "archive");
                var fileTarget = WrapFileTarget(new FileTarget
                {
                    FileName = logFile,
                    ArchiveFileName = Path.Combine(archiveFolder, "{####}.txt"),
                    ArchiveAboveSize = 1000,
                    LineEnding = LineEndingMode.LF,
                    ArchiveNumbering = ArchiveNumberingMode.Sequence,
                    Layout = "${message}",
                    MaxArchiveFiles = 0
                });

                SimpleConfigurator.ConfigureForTargetLogging(fileTarget, LogLevel.Debug);

                // we emit 5 * 250 *(3 x aaa + \n) bytes
                // so that we should get a full file + 4 archives
                Generate1000BytesLog('a');
                Generate1000BytesLog('b');
                Generate1000BytesLog('c');
                Generate1000BytesLog('d');
                Generate1000BytesLog('e');

                LogManager.Configuration = null;

                AssertFileContents(logFile,
                    StringRepeat(250, "eee\n"),
                    Encoding.UTF8);

                AssertFileContents(
                   Path.Combine(archiveFolder, "0000.txt"),
                   StringRepeat(250, "aaa\n"),
                   Encoding.UTF8);

                AssertFileContents(
                    Path.Combine(archiveFolder, "0001.txt"),
                    StringRepeat(250, "bbb\n"),
                    Encoding.UTF8);

                AssertFileContents(
                    Path.Combine(archiveFolder, "0002.txt"),
                    StringRepeat(250, "ccc\n"),
                    Encoding.UTF8);

                AssertFileContents(
                    Path.Combine(archiveFolder, "0003.txt"),
                    StringRepeat(250, "ddd\n"),
                    Encoding.UTF8);

                Assert.True(!File.Exists(Path.Combine(archiveFolder, "0004.txt")));
            }
            finally
            {
                if (File.Exists(logFile))
                    File.Delete(logFile);
                if (Directory.Exists(tempPath))
                    Directory.Delete(tempPath, true);
            }
        }

        [Fact(Skip = "this is not supported, because we cannot create multiple archive files with  ArchiveNumberingMode.Date (for one day)")]
        public void ArchiveAboveSizeWithArchiveNumberingModeDate_maxfiles_o()
        {
            var tempPath = Path.Combine(Path.GetTempPath(), "ArchiveEveryCombinedWithArchiveAboveSize_" + Guid.NewGuid().ToString());
            var logFile = Path.Combine(tempPath, "file.txt");
            try
            {
                string archiveFolder = Path.Combine(tempPath, "archive");
                var fileTarget = WrapFileTarget(new FileTarget
                {
                    FileName = logFile,
                    ArchiveFileName = Path.Combine(archiveFolder, "{####}.txt"),
                    ArchiveAboveSize = 1000,
                    LineEnding = LineEndingMode.LF,
                    Layout = "${message}",
                    ArchiveNumbering = ArchiveNumberingMode.Date
                });

                SimpleConfigurator.ConfigureForTargetLogging(fileTarget, LogLevel.Debug);

                //e.g. 20150804
                var archiveFileName = DateTime.Now.ToString("yyyyMMdd");

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


                //we expect only eee and all other in the archive
                AssertFileContents(logFile,
                    StringRepeat(250, "eee\n"),
                    Encoding.UTF8);

                //DUNNO what to expected!
                //try (which fails)
                AssertFileContents(
                    Path.Combine(archiveFolder, string.Format("{0}.txt", archiveFileName)),
                   StringRepeat(250, "aaa\n") + StringRepeat(250, "bbb\n") + StringRepeat(250, "ccc\n") + StringRepeat(250, "ddd\n"),
                    Encoding.UTF8);

            }
            finally
            {
                if (File.Exists(logFile))
                    File.Delete(logFile);
                if (Directory.Exists(tempPath))
                    Directory.Delete(tempPath, true);
            }
        }


        [Fact]
        public void DeleteArchiveFilesByDate()
        {
            const int maxArchiveFiles = 3;

            var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var logFile = Path.Combine(tempPath, "file.txt");
            try
            {
                string archiveFolder = Path.Combine(tempPath, "archive");
                var fileTarget = WrapFileTarget(new FileTarget
                {
                    FileName = logFile,
                    ArchiveFileName = Path.Combine(archiveFolder, "{#}.txt"),
                    ArchiveAboveSize = 50,
                    LineEnding = LineEndingMode.LF,
                    ArchiveNumbering = ArchiveNumberingMode.Date,
                    ArchiveDateFormat = "yyyyMMddHHmmssfff", //make sure the milliseconds are set in the filename
                    Layout = "${message}",
                    MaxArchiveFiles = maxArchiveFiles
                });

                SimpleConfigurator.ConfigureForTargetLogging(fileTarget, LogLevel.Debug);
                //writing 19 times 10 bytes (9 char + linefeed) will result in 3 archive files and 1 current file
                for (var i = 0; i < 19; ++i)
                {
                    logger.Debug("123456789");
                    //build in a small sleep to make sure the current time is reflected in the filename
                    //do this every 5 entries
                    if (i % 5 == 0)
                        Thread.Sleep(50);
                }
                //Setting the Configuration to [null] will result in a 'Dump' of the current log entries
                LogManager.Configuration = null;

                var files = Directory.GetFiles(archiveFolder).OrderBy(s => s);
                //the amount of archived files may not exceed the set 'MaxArchiveFiles'
                Assert.Equal(maxArchiveFiles, files.Count());


                SimpleConfigurator.ConfigureForTargetLogging(fileTarget, LogLevel.Debug);
                //writing just one line of 11 bytes will trigger the cleanup of old archived files
                //as stated by the MaxArchiveFiles property, but will only delete the oldest file
                logger.Debug("1234567890");
                LogManager.Configuration = null;

                var files2 = Directory.GetFiles(archiveFolder).OrderBy(s => s);
                Assert.Equal(maxArchiveFiles, files2.Count());

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
                if (File.Exists(logFile))
                    File.Delete(logFile);
                if (Directory.Exists(tempPath))
                    Directory.Delete(tempPath, true);
            }
        }

        [Fact]
        public void DeleteArchiveFilesByDateWithDateName()
        {
            const int maxArchiveFiles = 3;

            var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var logFile = Path.Combine(tempPath, "${date:format=yyyyMMddHHmmssfff}.txt");
            try
            {
                var fileTarget = WrapFileTarget(new FileTarget
                {
                    FileName = logFile,
                    ArchiveFileName = Path.Combine(tempPath, "{#}.txt"),
                    ArchiveEvery = FileArchivePeriod.Minute,
                    LineEnding = LineEndingMode.LF,
                    ArchiveNumbering = ArchiveNumberingMode.Date,
                    ArchiveDateFormat = "yyyyMMddHHmmssfff", //make sure the milliseconds are set in the filename
                    Layout = "${message}",
                    MaxArchiveFiles = maxArchiveFiles
                });

                SimpleConfigurator.ConfigureForTargetLogging(fileTarget, LogLevel.Debug);
                for (var i = 0; i < 4; ++i)
                {
                    logger.Debug("123456789");
                    //build in a  sleep to make sure the current time is reflected in the filename
                    Thread.Sleep(50);
                }
                //Setting the Configuration to [null] will result in a 'Dump' of the current log entries
                LogManager.Configuration = null;

                var files = Directory.GetFiles(tempPath).OrderBy(s => s);
                //we expect 3 archive files, plus one current file
                Assert.Equal(maxArchiveFiles + 1, files.Count());


                SimpleConfigurator.ConfigureForTargetLogging(fileTarget, LogLevel.Debug);
                //writing 50ms later will trigger the cleanup of old archived files
                //as stated by the MaxArchiveFiles property, but will only delete the oldest file
                Thread.Sleep(50);
                logger.Debug("123456789");
                LogManager.Configuration = null;

                var files2 = Directory.GetFiles(tempPath).OrderBy(s => s);
                Assert.Equal(maxArchiveFiles + 1, files2.Count());

                //the oldest file should be deleted
                Assert.DoesNotContain(files.ElementAt(0), files2);
                //two files should still be there
                Assert.Equal(files.ElementAt(1), files2.ElementAt(0));
                Assert.Equal(files.ElementAt(2), files2.ElementAt(1));
                Assert.Equal(files.ElementAt(3), files2.ElementAt(2));
                //one new file should be created
                Assert.DoesNotContain(files2.ElementAt(3), files);
            }
            finally
            {
                if (File.Exists(logFile))
                    File.Delete(logFile);
                if (Directory.Exists(tempPath))
                    Directory.Delete(tempPath, true);
            }
        }

        public static IEnumerable<object[]> DateArchive_UsesDateFromCurrentTimeSource_TestParameters
        {
            get
            {
                var booleanValues = new[] { true, false };
                var timeKindValues = new[] { DateTimeKind.Utc, DateTimeKind.Local };
                return
                    from timeKind in timeKindValues
                    from includeDateInLogFilePath in booleanValues
                    from concurrentWrites in booleanValues
                    from keepFileOpen in booleanValues
                    from networkWrites in booleanValues
                    from includeSequenceInArchive in booleanValues
                    select new object[] { timeKind, includeDateInLogFilePath, concurrentWrites, keepFileOpen, networkWrites, includeSequenceInArchive };
            }
        }

        [Theory]
        [PropertyData("DateArchive_UsesDateFromCurrentTimeSource_TestParameters")]
        public void DateArchive_UsesDateFromCurrentTimeSource(DateTimeKind timeKind, bool includeDateInLogFilePath, bool concurrentWrites, bool keepFileOpen, bool networkWrites, bool includeSequenceInArchive)
        {
            const string archiveDateFormat = "yyyyMMdd";
            const int maxArchiveFiles = 3;

            var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var logFile = Path.Combine(tempPath, includeDateInLogFilePath ? "file_${shortdate}.txt" : "file.txt");
            var defaultTimeSource = TimeSource.Current;
            try
            {
                var timeSource = new TimeSourceTests.ShiftedTimeSource(timeKind);

                TimeSource.Current = timeSource;

                string archiveFolder = Path.Combine(tempPath, "archive");
                string archiveFileNameTemplate = Path.Combine(archiveFolder, "{#}.txt");
                var fileTarget = WrapFileTarget(new FileTarget
                {
                    FileName = logFile,
                    ArchiveFileName = archiveFileNameTemplate,
                    LineEnding = LineEndingMode.LF,
                    ArchiveNumbering = includeSequenceInArchive ? ArchiveNumberingMode.DateAndSequence : ArchiveNumberingMode.Date,
                    ArchiveEvery = FileArchivePeriod.Day,
                    ArchiveDateFormat = archiveDateFormat,
                    Layout = "${date:format=O}|${message}",
                    MaxArchiveFiles = maxArchiveFiles,
                    ConcurrentWrites = concurrentWrites,
                    KeepFileOpen = keepFileOpen,
                    NetworkWrites = networkWrites,
                });

                SimpleConfigurator.ConfigureForTargetLogging(fileTarget, LogLevel.Debug);

                logger.Debug("123456789");
                DateTime previousWriteTime = timeSource.Time;

                const int daysToTestLogging = 5;
                const int intervalsPerDay = 24;
                var loggingInterval = TimeSpan.FromHours(1);
                for (var i = 0; i < daysToTestLogging * intervalsPerDay; ++i)
                {
                    timeSource.AddToLocalTime(loggingInterval);

                    if (timeSource.Time.Date != previousWriteTime.Date)
                    {
                        string currentLogFile = includeDateInLogFilePath
                            ? logFile.Replace("${shortdate}", timeSource.Time.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture))
                            : logFile;
                        // Simulate that previous file write began in previous day and ended on current day.
                        try
                        {
                            File.SetLastWriteTime(currentLogFile, timeSource.SystemTime);
                        }
                        catch { }
                    }

                    var eventInfo = new LogEventInfo(LogLevel.Debug, logger.Name, "123456789");
                    logger.Log(eventInfo);

                    var dayIsChanged = eventInfo.TimeStamp.Date != previousWriteTime.Date;
                    // ensure new archive is created only when the day part of time is changed
                    var archiveFileName = archiveFileNameTemplate.Replace("{#}", previousWriteTime.ToString(archiveDateFormat) + (includeSequenceInArchive ? ".0" : string.Empty));
                    var archiveExists = File.Exists(archiveFileName);
                    if (dayIsChanged)
                        Assert.True(archiveExists, string.Format("new archive should be created when the day part of {0} time is changed", timeKind));
                    else
                        Assert.False(archiveExists, string.Format("new archive should not be create when day part of {0} time is unchanged", timeKind));

                    previousWriteTime = eventInfo.TimeStamp.Date;
                    if (dayIsChanged)
                        timeSource.AddToSystemTime(TimeSpan.FromDays(1));
                }
                //Setting the Configuration to [null] will result in a 'Dump' of the current log entries
                LogManager.Configuration = null;

                var files = Directory.GetFiles(archiveFolder).OrderBy(s => s).ToList();
                //the amount of archived files may not exceed the set 'MaxArchiveFiles'
                Assert.Equal(maxArchiveFiles, files.Count);


                SimpleConfigurator.ConfigureForTargetLogging(fileTarget, LogLevel.Debug);
                //writing one line on a new day will trigger the cleanup of old archived files
                //as stated by the MaxArchiveFiles property, but will only delete the oldest file
                timeSource.AddToLocalTime(TimeSpan.FromDays(1));
                logger.Debug("1234567890");
                LogManager.Configuration = null;

                var files2 = Directory.GetFiles(archiveFolder).OrderBy(s => s).ToList();
                Assert.Equal(maxArchiveFiles, files2.Count);

                //the oldest file should be deleted
                Assert.DoesNotContain(files[0], files2);
                //two files should still be there
                Assert.Equal(files[1], files2[0]);
                Assert.Equal(files[2], files2[1]);
                //one new archive file should be created
                Assert.DoesNotContain(files2[2], files);
            }
            finally
            {
                TimeSource.Current = defaultTimeSource; // restore default time source
                if (Directory.Exists(tempPath))
                    Directory.Delete(tempPath, true);
            }
        }

        public static IEnumerable<object[]> DateArchive_ArchiveOnceOnly_TestParameters
        {
            get
            {
                var booleanValues = new[] { true, false };
                return
                    from concurrentWrites in booleanValues
                    from keepFileOpen in booleanValues
                    from networkWrites in booleanValues
                    where AllowsExternalFileModification(concurrentWrites, keepFileOpen, networkWrites)
                    from includeDateInLogFilePath in booleanValues
                    from includeSequenceInArchive in booleanValues
                    select new object[] { concurrentWrites, keepFileOpen, networkWrites, includeDateInLogFilePath, includeSequenceInArchive };
            }
        }

        private static bool AllowsExternalFileModification(bool concurrentWrites, bool keepFileOpen, bool networkWrites)
        {
            return (concurrentWrites) || (!keepFileOpen) || (networkWrites);
        }

        [Theory]
        [PropertyData("DateArchive_ArchiveOnceOnly_TestParameters")]
        public void DateArchive_ArchiveOnceOnly(bool concurrentWrites, bool keepFileOpen, bool networkWrites, bool dateInLogFilePath, bool includeSequenceInArchive)
        {
            var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var logFile = Path.Combine(tempPath, dateInLogFilePath ? "file_${shortdate}.txt" : "file.txt");
            try
            {
                string archiveFolder = Path.Combine(tempPath, "archive");
                var fileTarget = WrapFileTarget(new FileTarget
                {
                    FileName = logFile,
                    ArchiveFileName = Path.Combine(archiveFolder, "{#}.txt"),
                    LineEnding = LineEndingMode.LF,
                    ArchiveNumbering = includeSequenceInArchive ? ArchiveNumberingMode.DateAndSequence : ArchiveNumberingMode.Date,
                    ArchiveEvery = FileArchivePeriod.Day,
                    ArchiveDateFormat = "yyyyMMdd",
                    Layout = "${message}",
                    ConcurrentWrites = concurrentWrites,
                    KeepFileOpen = keepFileOpen,
                    NetworkWrites = networkWrites
                });

                SimpleConfigurator.ConfigureForTargetLogging(fileTarget, LogLevel.Debug);

                logger.Debug("123456789");
                string currentLogFile = Directory.GetFiles(tempPath)[0];
                File.SetCreationTime(currentLogFile, File.GetCreationTime(currentLogFile).AddDays(-1));
                File.SetLastWriteTime(currentLogFile, File.GetLastWriteTime(currentLogFile).AddDays(-1));
                // This should archive the log before logging.
                logger.Debug("123456789");
                // This must not archive.
                logger.Debug("123456789");

                LogManager.Configuration = null;
                File.Equals(1, Directory.GetFiles(archiveFolder).Length);
                AssertFileContents(currentLogFile, StringRepeat(2, "123456789\n"), Encoding.UTF8);
            }
            finally
            {
                if (Directory.Exists(tempPath))
                    Directory.Delete(tempPath, true);
            }
        }

        public static IEnumerable<object[]> DateArchive_SkipPeriod_TestParameters
        {
            get
            {
                var timeKindValues = new[] { DateTimeKind.Utc, DateTimeKind.Local };
                var archivePeriodValues = new[] { FileArchivePeriod.Day, FileArchivePeriod.Hour };
                var booleanValues = new[] { true, false };
                return
                    from timeKind in timeKindValues
                    from archivePeriod in archivePeriodValues
                    from includeDateInLogFilePath in booleanValues
                    from includeSequenceInArchive in booleanValues
                    select new object[] { timeKind, archivePeriod, includeDateInLogFilePath, includeSequenceInArchive };
            }
        }

        [Theory]
        [PropertyData("DateArchive_SkipPeriod_TestParameters")]
        public void DateArchive_SkipPeriod(DateTimeKind timeKind, FileArchivePeriod archivePeriod, bool includeDateInLogFilePath, bool includeSequenceInArchive)
        {
            var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var logFile = Path.Combine(tempPath, includeDateInLogFilePath ? "file_${shortdate}.txt" : "file.txt");
            var defaultTimeSource = TimeSource.Current;
            try
            {
                // Avoid inconsistency in file's last-write-time due to overflow of the minute during test run.
                while (DateTime.Now.Second > 55)
                    Thread.Sleep(1000);

                var timeSource = new TimeSourceTests.ShiftedTimeSource(timeKind);
                if (timeSource.Time.Minute == 59)
                {
                    // Avoid double-archive due to overflow of the hour.
                    timeSource.AddToLocalTime(TimeSpan.FromMinutes(1));
                    timeSource.AddToSystemTime(TimeSpan.FromMinutes(1));
                }
                TimeSource.Current = timeSource;

                var innerFileTarget = new FileTarget
                {
                    FileName = logFile,
                    ArchiveFileName = Path.Combine(tempPath, "archive", "{#}.txt"),
                    LineEnding = LineEndingMode.LF,
                    ArchiveNumbering = includeSequenceInArchive ? ArchiveNumberingMode.DateAndSequence : ArchiveNumberingMode.Date,
                    ArchiveEvery = archivePeriod,
                    ArchiveDateFormat = "yyyyMMddHHmm",
                    Layout = "${date:format=O}|${message}",
                };
                string archiveDateFormat = innerFileTarget.ArchiveDateFormat;
                var fileTarget = WrapFileTarget(innerFileTarget);
                SimpleConfigurator.ConfigureForTargetLogging(fileTarget, LogLevel.Debug);

                logger.Debug("1234567890");
                timeSource.AddToLocalTime(TimeSpan.FromMinutes(1));
                logger.Debug("1234567890");
                // The archive file name must be based on the last time the file was written.
                string archiveFileName = string.Format("{0}.txt", timeSource.Time.ToString(archiveDateFormat) + (includeSequenceInArchive ? ".0" : string.Empty));
                // Effectively update the file's last-write-time.
                timeSource.AddToSystemTime(TimeSpan.FromMinutes(1));

                timeSource.AddToLocalTime(TimeSpan.FromDays(2));
                logger.Debug("1234567890");
                LogManager.Configuration = null;

                string archivePath = Path.Combine(tempPath, "archive");
                var archiveFiles = Directory.GetFiles(archivePath);
                Assert.Equal(1, archiveFiles.Length);
                Assert.Equal(archiveFileName, Path.GetFileName(archiveFiles[0]));
            }
            finally
            {
                TimeSource.Current = defaultTimeSource; // restore default time source
                if (Directory.Exists(tempPath))
                    Directory.Delete(tempPath, true);
            }
        }

        public static IEnumerable<object[]> DateArchive_AllLoggersTransferToCurrentLogFile_TestParameters
        {
            get
            {
                var booleanValues = new[] { true, false };
                return
                    from concurrentWrites in booleanValues
                    from keepFileOpen in booleanValues
                    from networkWrites in booleanValues
                    where AllowsExternalFileModification(concurrentWrites, keepFileOpen, networkWrites)
                    from includeDateInLogFilePath in booleanValues
                    from includeSequenceInArchive in booleanValues
                    from enableArchiveCompression in booleanValues
                    select new object[] { concurrentWrites, keepFileOpen, networkWrites, includeDateInLogFilePath, includeSequenceInArchive, enableArchiveCompression };
            }
        }

        [Theory]
        [PropertyData("DateArchive_AllLoggersTransferToCurrentLogFile_TestParameters")]
        public void DateArchive_AllLoggersTransferToCurrentLogFile(bool concurrentWrites, bool keepFileOpen, bool networkWrites, bool includeDateInLogFilePath, bool includeSequenceInArchive, bool enableArchiveCompression)
        {
            var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var logfile = Path.Combine(tempPath, includeDateInLogFilePath ? "file_${shortdate}.txt" : "file.txt");
            try
            {
                var config = new LoggingConfiguration();

                string archiveFolder = Path.Combine(tempPath, "archive");
                var fileTarget1 = WrapFileTarget(new FileTarget
                {
                    FileName = logfile,
                    ArchiveFileName = Path.Combine(archiveFolder, "{#}.txt"),
                    LineEnding = LineEndingMode.LF,
                    ArchiveNumbering = includeSequenceInArchive ? ArchiveNumberingMode.DateAndSequence : ArchiveNumberingMode.Date,
                    ArchiveEvery = FileArchivePeriod.Day,
                    ArchiveDateFormat = "yyyyMMdd",
#if NET4_5
                    EnableArchiveFileCompression = enableArchiveCompression,
#endif
                    Layout = "${message}",
                    ConcurrentWrites = concurrentWrites,
                    KeepFileOpen = keepFileOpen,
                    NetworkWrites = networkWrites
                });
                var logger1Rule = new LoggingRule("logger1", LogLevel.Debug, fileTarget1);
                config.LoggingRules.Add(logger1Rule);

                var fileTarget2 = WrapFileTarget(new FileTarget
                {
                    FileName = logfile,
                    ArchiveFileName = Path.Combine(archiveFolder, "{#}.txt"),
                    LineEnding = LineEndingMode.LF,
                    ArchiveNumbering = includeSequenceInArchive ? ArchiveNumberingMode.DateAndSequence : ArchiveNumberingMode.Date,
                    ArchiveEvery = FileArchivePeriod.Day,
                    ArchiveDateFormat = "yyyyMMdd",
#if NET4_5
                    EnableArchiveFileCompression = enableArchiveCompression,
#endif
                    Layout = "${message}",
                    ConcurrentWrites = concurrentWrites,
                    KeepFileOpen = keepFileOpen,
                    NetworkWrites = networkWrites
                });
                var logger2Rule = new LoggingRule("logger2", LogLevel.Debug, fileTarget2);
                config.LoggingRules.Add(logger2Rule);

                LogManager.Configuration = config;

                var logger1 = LogManager.GetLogger("logger1");
                var logger2 = LogManager.GetLogger("logger2");

                logger1.Debug("123456789");
                logger2.Debug("123456789");
                string currentLogFile = Directory.GetFiles(tempPath)[0];
                File.SetCreationTime(currentLogFile, File.GetCreationTime(currentLogFile).AddDays(-1));
                File.SetLastWriteTime(currentLogFile, File.GetLastWriteTime(currentLogFile).AddDays(-1));
                logger1.Debug("123456789");
                Thread.Sleep(10);
                logger2.Debug("123456789");

                LogManager.Configuration = null;
                var files = Directory.GetFiles(archiveFolder);
                Assert.Equal(1, Directory.GetFiles(archiveFolder).Length);
                AssertFileContents(currentLogFile, StringRepeat(2, "123456789\n"), Encoding.UTF8);
            }
            finally
            {
                if (Directory.Exists(tempPath))
                    Directory.Delete(tempPath, true);
            }
        }

        [Fact]
        public void DeleteArchiveFilesByDate_MaxArchiveFiles_0()
        {
            var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var logFile = Path.Combine(tempPath, "file.txt");
            try
            {
                string archiveFolder = Path.Combine(tempPath, "archive");
                var fileTarget = WrapFileTarget(new FileTarget
                {
                    FileName = logFile,
                    ArchiveFileName = Path.Combine(archiveFolder, "{#}.txt"),
                    ArchiveAboveSize = 50,
                    LineEnding = LineEndingMode.LF,
                    ArchiveNumbering = ArchiveNumberingMode.Date,
                    ArchiveDateFormat = "yyyyMMddHHmmssfff", //make sure the milliseconds are set in the filename
                    Layout = "${message}",
                    MaxArchiveFiles = 0
                });

                SimpleConfigurator.ConfigureForTargetLogging(fileTarget, LogLevel.Debug);
                //writing 19 times 10 bytes (9 char + linefeed) will result in 3 archive files and 1 current file
                for (var i = 0; i < 19; ++i)
                {
                    logger.Debug("123456789");
                    //build in a small sleep to make sure the current time is reflected in the filename
                    //do this every 5 entries
                    if (i % 5 == 0)
                    {
                        Thread.Sleep(50);
                    }
                }

                //Setting the Configuration to [null] will result in a 'Dump' of the current log entries
                LogManager.Configuration = null;

                var fileCount = Directory.EnumerateFiles(archiveFolder).Count();

                Assert.Equal(3, fileCount);

                SimpleConfigurator.ConfigureForTargetLogging(fileTarget, LogLevel.Debug);
                //create 1 new file for archive
                logger.Debug("1234567890");
                LogManager.Configuration = null;

                var fileCount2 = Directory.EnumerateFiles(archiveFolder).Count();
                //there should be 1 more file
                Assert.Equal(4, fileCount2);
            }
            finally
            {

                if (File.Exists(logFile))
                {
                    File.Delete(logFile);
                }

                if (Directory.Exists(tempPath))
                {
                    Directory.Delete(tempPath, true);
                }
            }
        }

        [Fact]
        public void DeleteArchiveFilesByDate_AlteredMaxArchive()
        {
            var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var logFile = Path.Combine(tempPath, "file.txt");
            try
            {
                string archiveFolder = Path.Combine(tempPath, "archive");
                var innerFileTarget = new FileTarget
                {
                    FileName = logFile,
                    ArchiveFileName = Path.Combine(archiveFolder, "{#}.txt"),
                    ArchiveAboveSize = 50,
                    LineEnding = LineEndingMode.LF,
                    ArchiveNumbering = ArchiveNumberingMode.Date,
                    ArchiveDateFormat = "yyyyMMddHHmmssfff", //make sure the milliseconds are set in the filename
                    Layout = "${message}",
                    MaxArchiveFiles = 5
                };
                var fileTarget = WrapFileTarget(innerFileTarget);

                SimpleConfigurator.ConfigureForTargetLogging(fileTarget, LogLevel.Debug);
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

                var files = Directory.GetFiles(archiveFolder).OrderBy(s => s);
                //the amount of archived files may not exceed the set 'MaxArchiveFiles'
                Assert.Equal(innerFileTarget.MaxArchiveFiles, files.Count());


                //alter the MaxArchivedFiles
                innerFileTarget.MaxArchiveFiles = 2;
                SimpleConfigurator.ConfigureForTargetLogging(fileTarget, LogLevel.Debug);
                //writing just one line of 11 bytes will trigger the cleanup of old archived files
                //as stated by the MaxArchiveFiles property, but will only delete the oldest files
                logger.Debug("1234567890");
                LogManager.Configuration = null;

                var files2 = Directory.GetFiles(archiveFolder).OrderBy(s => s);
                Assert.Equal(innerFileTarget.MaxArchiveFiles, files2.Count());

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
                if (File.Exists(logFile))
                    File.Delete(logFile);
                if (Directory.Exists(tempPath))
                    Directory.Delete(tempPath, true);
            }
        }

        [Fact]
        public void RepeatingHeaderTest()
        {
            var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var logFile = Path.Combine(tempPath, "file.txt");
            try
            {
                const string header = "Headerline";

                string archiveFolder = Path.Combine(tempPath, "archive");
                var fileTarget = WrapFileTarget(new FileTarget
                {
                    FileName = logFile,
                    ArchiveFileName = Path.Combine(archiveFolder, "{####}.txt"),
                    ArchiveAboveSize = 51,
                    LineEnding = LineEndingMode.LF,
                    ArchiveNumbering = ArchiveNumberingMode.Sequence,
                    Layout = "${message}",
                    Header = header,
                    MaxArchiveFiles = 2,
                });

                SimpleConfigurator.ConfigureForTargetLogging(fileTarget, LogLevel.Debug);

                for (var i = 0; i < 16; ++i)
                {
                    logger.Debug("123456789");
                }

                LogManager.Configuration = null;

                AssertFileContentsStartsWith(logFile, header, Encoding.UTF8);

                AssertFileContentsStartsWith(Path.Combine(archiveFolder, "0002.txt"), header, Encoding.UTF8);

                AssertFileContentsStartsWith(Path.Combine(archiveFolder, "0001.txt"), header, Encoding.UTF8);

                Assert.True(!File.Exists(Path.Combine(archiveFolder, "0000.txt")));
            }
            finally
            {
                if (File.Exists(logFile))
                    File.Delete(logFile);
                if (Directory.Exists(tempPath))
                    Directory.Delete(tempPath, true);
            }
        }

        [Fact]
        public void RepeatingFooterTest()
        {
            var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var logFile = Path.Combine(tempPath, "file.txt");
            try
            {
                const string footer = "Footerline";

                string archiveFolder = Path.Combine(tempPath, "archive");
                var ft = new FileTarget
                {
                    FileName = logFile,
                    ArchiveFileName = Path.Combine(archiveFolder, "{####}.txt"),
                    ArchiveAboveSize = 51,
                    LineEnding = LineEndingMode.LF,
                    ArchiveNumbering = ArchiveNumberingMode.Sequence,
                    Layout = "${message}",
                    Footer = footer,
                    MaxArchiveFiles = 2,
                };

                SimpleConfigurator.ConfigureForTargetLogging(ft, LogLevel.Debug);

                for (var i = 0; i < 16; ++i)
                {
                    logger.Debug("123456789");
                }

                LogManager.Configuration = null;

                string expectedEnding = footer + ft.LineEnding.NewLineCharacters;
                AssertFileContentsEndsWith(logFile, expectedEnding, Encoding.UTF8);
                AssertFileContentsEndsWith(Path.Combine(archiveFolder, "0002.txt"), expectedEnding, Encoding.UTF8);
                AssertFileContentsEndsWith(Path.Combine(archiveFolder, "0001.txt"), expectedEnding, Encoding.UTF8);
                Assert.True(!File.Exists(Path.Combine(archiveFolder, "0000.txt")));
            }
            finally
            {
                LogManager.Configuration = null;
                if (File.Exists(logFile))
                    File.Delete(logFile);
                if (Directory.Exists(tempPath))
                    Directory.Delete(tempPath, true);
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void RollingArchiveTest(bool specifyArchiveFileName)
        {
            RollingArchiveTests(enableCompression: false, specifyArchiveFileName: specifyArchiveFileName);
        }

#if NET4_5
        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void RollingArchiveCompressionTest(bool specifyArchiveFileName)
        {
            RollingArchiveTests(enableCompression: true, specifyArchiveFileName: specifyArchiveFileName);
        }
#endif

        private void RollingArchiveTests(bool enableCompression, bool specifyArchiveFileName)
        {
            var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var logFile = Path.Combine(tempPath, "file.txt");
            var archiveExtension = enableCompression ? "zip" : "txt";
            try
            {
                var innerFileTarget = new FileTarget
                {
#if NET4_5
                    EnableArchiveFileCompression = enableCompression,
#endif
                    FileName = logFile,
                    ArchiveAboveSize = 1000,
                    LineEnding = LineEndingMode.LF,
                    ArchiveNumbering = ArchiveNumberingMode.Rolling,
                    Layout = "${message}",
                    MaxArchiveFiles = 3
                };
                if (specifyArchiveFileName)
                    innerFileTarget.ArchiveFileName = Path.Combine(tempPath, "archive", "{####}." + archiveExtension);
                var fileTarget = WrapFileTarget(innerFileTarget);

                SimpleConfigurator.ConfigureForTargetLogging(fileTarget, LogLevel.Debug);

                // we emit 5 * 250 * (3 x aaa + \n) bytes
                // so that we should get a full file + 3 archives
                Generate1000BytesLog('a');
                Generate1000BytesLog('b');
                Generate1000BytesLog('c');
                Generate1000BytesLog('d');
                Generate1000BytesLog('e');

                LogManager.Configuration = null;

                var assertFileContents =
#if NET4_5
 enableCompression ? new Action<string, string, Encoding>(AssertZipFileContents) : AssertFileContents;
#else
 new Action<string, string, Encoding>(AssertFileContents);
#endif

                AssertFileContents(logFile,
                    StringRepeat(250, "eee\n"),
                    Encoding.UTF8);

                string archiveFileNameFormat = specifyArchiveFileName
                    ? Path.Combine("archive", "000{0}." + archiveExtension)
                    : "file.{0}." + archiveExtension;

                assertFileContents(
                    Path.Combine(tempPath, string.Format(archiveFileNameFormat, 0)),
                    StringRepeat(250, "ddd\n"),
                    Encoding.UTF8);

                assertFileContents(
                    Path.Combine(tempPath, string.Format(archiveFileNameFormat, 1)),
                    StringRepeat(250, "ccc\n"),
                    Encoding.UTF8);

                assertFileContents(
                    Path.Combine(tempPath, string.Format(archiveFileNameFormat, 2)),
                    StringRepeat(250, "bbb\n"),
                    Encoding.UTF8);

                Assert.True(!File.Exists(Path.Combine(tempPath, string.Format(archiveFileNameFormat, 3))));
            }
            finally
            {
                if (File.Exists(logFile))
                    File.Delete(logFile);
                if (Directory.Exists(tempPath))
                    Directory.Delete(tempPath, true);
            }
        }

        [InlineData("/")]
        [InlineData("\\")]
        [Theory]
        public void RollingArchiveTest_MaxArchiveFiles_0(string slash)
        {
            var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var logFile = Path.Combine(tempPath, "file.txt");
            try
            {
                var fileTarget = WrapFileTarget(new FileTarget
                {
                    FileName = logFile,
                    ArchiveFileName = Path.Combine(tempPath, "archive" + slash + "{####}.txt"),
                    ArchiveAboveSize = 1000,
                    LineEnding = LineEndingMode.LF,
                    ArchiveNumbering = ArchiveNumberingMode.Rolling,
                    Layout = "${message}",
                    MaxArchiveFiles = 0
                });

                SimpleConfigurator.ConfigureForTargetLogging(fileTarget, LogLevel.Debug);

                // we emit 5 * 250 * (3 x aaa + \n) bytes
                // so that we should get a full file + 4 archives
                Generate1000BytesLog('a');
                Generate1000BytesLog('b');
                Generate1000BytesLog('c');
                Generate1000BytesLog('d');
                Generate1000BytesLog('e');

                LogManager.Configuration = null;

                AssertFileContents(logFile,
                    StringRepeat(250, "eee\n"),
                    Encoding.UTF8);

                AssertFileContents(
                    Path.Combine(tempPath, "archive" + slash + "0000.txt"),
                    StringRepeat(250, "ddd\n"),
                    Encoding.UTF8);

                AssertFileContents(
                    Path.Combine(tempPath, "archive" + slash + "0001.txt"),
                    StringRepeat(250, "ccc\n"),
                    Encoding.UTF8);

                AssertFileContents(
                    Path.Combine(tempPath, "archive" + slash + "0002.txt"),
                    StringRepeat(250, "bbb\n"),
                    Encoding.UTF8);

                AssertFileContents(
                    Path.Combine(tempPath, "archive" + slash + "0003.txt"),
                    StringRepeat(250, "aaa\n"),
                    Encoding.UTF8);
            }
            finally
            {

                if (File.Exists(logFile))
                {
                    File.Delete(logFile);
                }

                if (Directory.Exists(tempPath))
                {
                    Directory.Delete(tempPath, true);
                }
            }
        }

        [Fact]
        public void MultiFileWrite()
        {
            var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            try
            {
                var fileTarget = WrapFileTarget(new FileTarget
                {
                    FileName = Path.Combine(tempPath, "${level}.txt"),
                    LineEnding = LineEndingMode.LF,
                    Layout = "${message}"
                });

                SimpleConfigurator.ConfigureForTargetLogging(fileTarget, LogLevel.Debug);

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
                var fileTarget = WrapFileTarget(new FileTarget
                {
                    FileName = Path.Combine(tempPath, "${level}.txt"),
                    LineEnding = LineEndingMode.LF,
                    Layout = "${message}"
                });

                SimpleConfigurator.ConfigureForTargetLogging(new BufferingTargetWrapper(fileTarget, 10), LogLevel.Debug);

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
                var fileTarget = WrapFileTarget(new FileTarget
                {
                    FileName = Path.Combine(tempPath, "${level}.txt"),
                    LineEnding = LineEndingMode.LF,
                    Layout = "${message} ${threadid}"
                });

                // this also checks that thread-volatile layouts
                // such as ${threadid} are properly cached and not recalculated
                // in logging threads.

                var threadID = Thread.CurrentThread.ManagedThreadId.ToString();

                SimpleConfigurator.ConfigureForTargetLogging(new AsyncTargetWrapper(fileTarget, 1000, AsyncTargetWrapperOverflowAction.Grow)
                {
                    Name = "AsyncMultiFileWrite_wrapper"
                }, LogLevel.Debug);
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
                if (Directory.Exists(tempPath))
                    Directory.Delete(tempPath, true);

                // Clean up configuration change, breaks onetimeonlyexceptioninhandlertest
                LogManager.ThrowExceptions = true;
            }
        }

        [Fact]
        public void DisposingFileTarget_WhenNotIntialized_ShouldNotThrow()
        {
            var exceptionThrown = false;
            var fileTarget = WrapFileTarget(new FileTarget());

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
        public void FileTarget_ArchiveNumbering_DateAndSequence()
        {
            FileTarget_ArchiveNumbering_DateAndSequenceTests(enableCompression: false);
        }

#if NET4_5
        [Fact]
        public void FileTarget_ArchiveNumbering_DateAndSequence_WithCompression()
        {
            FileTarget_ArchiveNumbering_DateAndSequenceTests(enableCompression: true);
        }
#endif

        private void FileTarget_ArchiveNumbering_DateAndSequenceTests(bool enableCompression)
        {
            const string archiveDateFormat = "yyyy-MM-dd";
            const int archiveAboveSize = 1000;

            var tempPath = ArchiveFileNameHelper.GenerateTempPath();
            var logFile = Path.Combine(tempPath, "file.txt");
            var archiveExtension = enableCompression ? "zip" : "txt";
            try
            {
                var fileTarget = WrapFileTarget(new FileTarget
                {
#if NET4_5
                    EnableArchiveFileCompression = enableCompression,
#endif
                    FileName = logFile,
                    ArchiveFileName = Path.Combine(tempPath, "archive", "{#}." + archiveExtension),
                    ArchiveDateFormat = archiveDateFormat,
                    ArchiveAboveSize = archiveAboveSize,
                    LineEnding = LineEndingMode.LF,
                    Layout = "${message}",
                    MaxArchiveFiles = 3,
                    ArchiveNumbering = ArchiveNumberingMode.DateAndSequence,
                    ArchiveEvery = FileArchivePeriod.Day
                });

                SimpleConfigurator.ConfigureForTargetLogging(fileTarget, LogLevel.Debug);

                // we emit 5 * 250 *(3 x aaa + \n) bytes
                // so that we should get a full file + 3 archives
                Generate1000BytesLog('a');
                Generate1000BytesLog('b');
                Generate1000BytesLog('c');
                Generate1000BytesLog('d');
                Generate1000BytesLog('e');

                string archiveFilename = DateTime.Now.ToString(archiveDateFormat);

                LogManager.Configuration = null;


#if NET4_5
                var assertFileContents = enableCompression ? new Action<string, string, Encoding>(AssertZipFileContents) : AssertFileContents;
#else
                var assertFileContents = new Action<string, string, Encoding>(AssertFileContents);
#endif
                ArchiveFileNameHelper helper = new ArchiveFileNameHelper(Path.Combine(tempPath, "archive"), archiveFilename, archiveExtension);

                AssertFileContents(logFile,
                    StringRepeat(250, "eee\n"),
                    Encoding.UTF8);

                assertFileContents(helper.GetFullPath(1), StringRepeat(250, "bbb\n"), Encoding.UTF8);
                assertFileContents(helper.GetFullPath(2), StringRepeat(250, "ccc\n"), Encoding.UTF8);
                assertFileContents(helper.GetFullPath(3), StringRepeat(250, "ddd\n"), Encoding.UTF8);

                Assert.False(helper.Exists(0), "First archive should have been deleted due to max archive count.");
                Assert.False(helper.Exists(4), "Fifth archive must not have been created yet.");
            }
            finally
            {
                if (File.Exists(logFile))
                    File.Delete(logFile);
                if (Directory.Exists(tempPath))
                    Directory.Delete(tempPath, true);
            }
        }

        [Theory]
        [InlineData("/")]
        [InlineData("\\")]
        public void FileTarget_WithArchiveFileNameEndingInNumberPlaceholder_ShouldArchiveFile(string slash)
        {
            var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var logFile = Path.Combine(tempPath, "file.txt");
            try
            {
                var fileTarget = WrapFileTarget(new FileTarget
                {
                    FileName = logFile,
                    ArchiveFileName = Path.Combine(tempPath, "archive" + slash + "test.log.{####}"),
                    ArchiveAboveSize = 1000
                });

                SimpleConfigurator.ConfigureForTargetLogging(fileTarget, LogLevel.Debug);

                for (var i = 0; i < 100; ++i)
                {
                    logger.Debug("a");
                }

                LogManager.Configuration = null;
                Assert.True(File.Exists(logFile));
                Assert.True(File.Exists(Path.Combine(tempPath, "archive" + slash + "test.log.0000")));
            }
            finally
            {
                if (File.Exists(logFile))
                    File.Delete(logFile);
                if (Directory.Exists(tempPath))
                    Directory.Delete(tempPath, true);
            }
        }

        [Fact]
        public void FileTarget_InvalidFileNameCorrection()
        {
            var tempFile = Path.GetTempFileName();
            var invalidLogFileName = tempFile + Path.GetInvalidFileNameChars()[0];
            var expectedCorrectedTempFile = tempFile + "_";

            try
            {
                var fileTarget = WrapFileTarget(new FileTarget
                {
                    FileName = SimpleLayout.Escape(invalidLogFileName),
                    LineEnding = LineEndingMode.LF,
                    Layout = "${level} ${message}",
                    OpenFileCacheTimeout = 0
                });

                SimpleConfigurator.ConfigureForTargetLogging(fileTarget, LogLevel.Fatal);

                logger.Fatal("aaa");
                LogManager.Configuration = null;
                AssertFileContents(expectedCorrectedTempFile, "Fatal aaa\n", Encoding.UTF8);
            }
            finally
            {
                if (File.Exists(invalidLogFileName))
                    File.Delete(invalidLogFileName);
                if (File.Exists(expectedCorrectedTempFile))
                    File.Delete(expectedCorrectedTempFile);
            }
        }

        [Fact]
        public void FileTarget_LogAndArchiveFilesWithSameName_ShouldArchive()
        {
            var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var logFile = Path.Combine(tempPath, "Application.log");
            var tempDirectory = new DirectoryInfo(tempPath);
            try
            {

                var archiveFile = Path.Combine(tempPath, "Application{#}.log");
                var archiveFileMask = "Application*.log";

                var fileTarget = WrapFileTarget(new FileTarget
                {
                    FileName = logFile,
                    ArchiveFileName = archiveFile,
                    ArchiveAboveSize = 1, //Force immediate archival
                    ArchiveNumbering = ArchiveNumberingMode.DateAndSequence,
                    MaxArchiveFiles = 5
                });

                SimpleConfigurator.ConfigureForTargetLogging(fileTarget, LogLevel.Debug);

                //Creates 5 archive files.
                for (int i = 0; i <= 5; i++)
                {
                    logger.Debug("a");
                }

                Assert.True(File.Exists(logFile));

                //Five archive files, plus the log file itself.
                Assert.True(tempDirectory.GetFiles(archiveFileMask).Count() == 5 + 1);
            }
            finally
            {
                if (tempDirectory.Exists)
                {
                    tempDirectory.Delete(true);
                }
            }

        }

        [Fact]
        public void FileTarget_Handle_Other_Files_That_Match_Archive_Format()
        {
            var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var logFile = Path.Combine(tempPath, "Application.log");
            var tempDirectory = new DirectoryInfo(tempPath);

            try
            {
                string archiveFileLayout = Path.Combine(Path.GetDirectoryName(logFile), Path.GetFileNameWithoutExtension(logFile) + "{#}" + Path.GetExtension(logFile));

                var fileTarget = WrapFileTarget(new FileTarget
                {
                    FileName = logFile,
                    Layout = "${message}",
                    EnableFileDelete = false,
                    Encoding = Encoding.UTF8,
                    ArchiveFileName = archiveFileLayout,
                    ArchiveEvery = FileArchivePeriod.Day,
                    ArchiveNumbering = ArchiveNumberingMode.Date,
                    ArchiveDateFormat = "___________yyyyMMddHHmm",
                    MaxArchiveFiles = 10   // Get past the optimization to avoid deleting old files.
                });

                SimpleConfigurator.ConfigureForTargetLogging(fileTarget, LogLevel.Debug);


                string existingFile = archiveFileLayout.Replace("{#}", "notadate");
                Directory.CreateDirectory(Path.GetDirectoryName(logFile));
                File.Create(existingFile).Close();

                logger.Debug("test");

                AssertFileContents(logFile, "test" + LineEndingMode.Default.NewLineCharacters, Encoding.UTF8);
                Assert.True(File.Exists(existingFile));
            }
            finally
            {
                if (tempDirectory.Exists)
                {
                    tempDirectory.Delete(true);
                }
            }

        }

        [Fact]
        public void SingleArchiveFileRollsCorrectly()
        {
            var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var logFile = Path.Combine(tempPath, "file.txt");
            try
            {
                var fileTarget = WrapFileTarget(new FileTarget
                {
                    FileName = logFile,
                    ArchiveFileName = Path.Combine(tempPath, "archive", "file.txt2"),
                    ArchiveAboveSize = 1000,
                    LineEnding = LineEndingMode.LF,
                    Layout = "${message}",
                    MaxArchiveFiles = 1,
                });

                SimpleConfigurator.ConfigureForTargetLogging(fileTarget, LogLevel.Debug);

                // we emit 2 * 250 *(aaa + \n) bytes
                // so that we should get a full file + 1 archives
                for (var i = 0; i < 250; ++i)
                {
                    logger.Debug("aaa");
                }
                for (var i = 0; i < 250; ++i)
                {
                    logger.Debug("bbb");
                }

                AssertFileContents(logFile,
                    StringRepeat(250, "bbb\n"),
                    Encoding.UTF8);
                AssertFileContents(
                    Path.Combine(tempPath, "archive", "file.txt2"),
                    StringRepeat(250, "aaa\n"),
                    Encoding.UTF8);

                for (var i = 0; i < 250; ++i)
                {
                    logger.Debug("ccc");
                }

                LogManager.Configuration = null;

                AssertFileContents(logFile,
                    StringRepeat(250, "ccc\n"),
                    Encoding.UTF8);
                AssertFileContents(
                    Path.Combine(tempPath, "archive", "file.txt2"),
                    StringRepeat(250, "bbb\n"),
                    Encoding.UTF8);
            }
            finally
            {
                if (File.Exists(logFile))
                    File.Delete(logFile);
                if (Directory.Exists(tempPath))
                    Directory.Delete(tempPath, true);
            }
        }

        [Fact]
        public void ArchiveFileRollsCorrectly()
        {
            var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var logFile = Path.Combine(tempPath, "file.txt");
            try
            {
                var fileTarget = WrapFileTarget(new FileTarget
                {
                    FileName = logFile,
                    ArchiveFileName = Path.Combine(tempPath, "archive", "file.txt2"),
                    ArchiveAboveSize = 1000,
                    LineEnding = LineEndingMode.LF,
                    Layout = "${message}",
                    MaxArchiveFiles = 2,
                });

                SimpleConfigurator.ConfigureForTargetLogging(fileTarget, LogLevel.Debug);

                // we emit 3 * 250 *(aaa + \n) bytes
                // so that we should get a full file + 2 archives
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

                AssertFileContents(logFile,
                    StringRepeat(250, "ccc\n"),
                    Encoding.UTF8);
                AssertFileContents(
                    Path.Combine(tempPath, "archive", "file.1.txt2"),
                    StringRepeat(250, "bbb\n"),
                    Encoding.UTF8);
                AssertFileContents(
                    Path.Combine(tempPath, "archive", "file.txt2"),
                    StringRepeat(250, "aaa\n"),
                    Encoding.UTF8);

                for (var i = 0; i < 250; ++i)
                {
                    logger.Debug("ddd");
                }

                LogManager.Configuration = null;

                AssertFileContents(logFile,
                    StringRepeat(250, "ddd\n"),
                    Encoding.UTF8);
                AssertFileContents(
                    Path.Combine(tempPath, "archive", "file.2.txt2"),
                    StringRepeat(250, "ccc\n"),
                    Encoding.UTF8);
                AssertFileContents(
                    Path.Combine(tempPath, "archive", "file.1.txt2"),
                    StringRepeat(250, "bbb\n"),
                    Encoding.UTF8);
                Assert.False(File.Exists(Path.Combine(tempPath, "archive", "file.txt2")));
            }
            finally
            {
                if (File.Exists(logFile))
                    File.Delete(logFile);
                if (Directory.Exists(tempPath))
                    Directory.Delete(tempPath, true);
            }
        }

        [Fact]
        public void ArchiveFileRollsCorrectly_ExistingArchives()
        {
            var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var logFile = Path.Combine(tempPath, "file.txt");
            try
            {
                Directory.CreateDirectory(Path.Combine(tempPath, "archive"));
                File.Create(Path.Combine(tempPath, "archive", "file.10.txt2")).Dispose();
                File.Create(Path.Combine(tempPath, "archive", "file.9.txt2")).Dispose();

                var fileTarget = WrapFileTarget(new FileTarget
                {
                    FileName = logFile,
                    ArchiveFileName = Path.Combine(tempPath, "archive", "file.txt2"),
                    ArchiveAboveSize = 1000,
                    LineEnding = LineEndingMode.LF,
                    Layout = "${message}",
                    MaxArchiveFiles = 2,
                });

                SimpleConfigurator.ConfigureForTargetLogging(fileTarget, LogLevel.Debug);

                // we emit 2 * 250 *(aaa + \n) bytes
                // so that we should get a full file + 1 archive
                for (var i = 0; i < 250; ++i)
                {
                    logger.Debug("aaa");
                }
                for (var i = 0; i < 250; ++i)
                {
                    logger.Debug("bbb");
                }

                AssertFileContents(logFile,
                    StringRepeat(250, "bbb\n"),
                    Encoding.UTF8);
                AssertFileContents(
                    Path.Combine(tempPath, "archive", "file.11.txt2"),
                    StringRepeat(250, "aaa\n"),
                    Encoding.UTF8);
                Assert.True(File.Exists(Path.Combine(tempPath, "archive", "file.10.txt2")));
                Assert.False(File.Exists(Path.Combine(tempPath, "archive", "file.9.txt2")));
            }
            finally
            {
                if (File.Exists(logFile))
                    File.Delete(logFile);
                if (Directory.Exists(tempPath))
                    Directory.Delete(tempPath, true);
            }
        }

        /// <summary>
        /// Remove archived files in correct order
        /// </summary>
        [Fact]
        public void FileTarget_ArchiveNumbering_remove_correct_order()
        {
            const int maxArchiveFiles = 10;

            var tempPath = ArchiveFileNameHelper.GenerateTempPath();
            var logFile = Path.Combine(tempPath, "file.txt");
            var archiveExtension = "txt";
            try
            {
                var fileTarget = new FileTarget
                {
                    FileName = logFile,
                    ArchiveFileName = Path.Combine(tempPath, "archive", "{#}." + archiveExtension),
                    ArchiveDateFormat = "yyyy-MM-dd",
                    ArchiveAboveSize = 1000,
                    LineEnding = LineEndingMode.LF,
                    Layout = "${message}",
                    MaxArchiveFiles = maxArchiveFiles,
                    ArchiveNumbering = ArchiveNumberingMode.DateAndSequence,
                };

                SimpleConfigurator.ConfigureForTargetLogging(fileTarget, LogLevel.Debug);

                ArchiveFileNameHelper helper = new ArchiveFileNameHelper(Path.Combine(tempPath, "archive"), DateTime.Now.ToString(fileTarget.ArchiveDateFormat), archiveExtension);

                Generate1000BytesLog('a');

                for (int i = 0; i < maxArchiveFiles; i++)
                {
                    Generate1000BytesLog('a');
                    Assert.True(helper.Exists(i), string.Format("file {0} is missing", i));
                }

                for (int i = maxArchiveFiles; i < 100; i++)
                {
                    Generate1000BytesLog('b');
                    var numberToBeRemoved = i - maxArchiveFiles; // number 11, we need to remove 1 etc
                    Assert.True(!helper.Exists(numberToBeRemoved), string.Format("archive file {0} has not been removed! We are created file {1}", numberToBeRemoved, i));
                }

            }
            finally
            {
                if (File.Exists(logFile))
                    File.Delete(logFile);
                if (Directory.Exists(tempPath))
                    Directory.Delete(tempPath, true);
            }
        }

        private void Generate1000BytesLog(char c)
        {
            for (var i = 0; i < 250; ++i)
            {
                //3 chars with newlines = 4 bytes
                logger.Debug(new string(c, 3));
            }
        }

        /// <summary>
        /// Archive file helepr
        /// </summary>
        /// <remarks>TODO rewrite older test</remarks>
        private class ArchiveFileNameHelper
        {
            public string FolderName { get; private set; }

            public string FileName { get; private set; }
            /// <summary>
            /// Ext without dot
            /// </summary>
            public string Ext { get; set; }

            /// <summary>
            /// Initializes a new instance of the <see cref="T:System.Object"/> class.
            /// </summary>
            public ArchiveFileNameHelper(string folderName, string fileName, string ext)
            {
                Ext = ext;
                FileName = fileName;
                FolderName = folderName;
            }

            public bool Exists(int number)
            {
                return File.Exists(GetFullPath(number));
            }

            public string GetFullPath(int number)
            {
                return Path.Combine(String.Format("{0}/{1}.{2}.{3}", FolderName, FileName, number, Ext));
            }

            public static string GenerateTempPath()
            {
                return Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            }
        }

        [Theory]
        [InlineData("##", 0, "00")]
        [InlineData("###", 1, "001")]
        [InlineData("#", 20, "20")]
        public void FileTarget_WithDateAndSequenceArchiveNumbering_ShouldPadSequenceNumberInArchiveFileName(
            string placeHolderSharps, int sequenceNumber, string expectedSequenceInArchiveFileName)
        {
            string archivePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            const string archiveDateFormat = "yyyy-MM-dd";
            string archiveFileName = Path.Combine(archivePath, String.Format("{{{0}}}.log", placeHolderSharps));
            string expectedArchiveFullName = String.Format("{0}/{1}.{2}.log",
                archivePath,
                DateTime.Now.ToString(archiveDateFormat),
                expectedSequenceInArchiveFileName);

            GenerateArchives(count: sequenceNumber + 1, archiveDateFormat: archiveDateFormat,
                archiveFileName: archiveFileName, archiveNumbering: ArchiveNumberingMode.DateAndSequence);
            bool resultArchiveWithExpectedNameExists = File.Exists(expectedArchiveFullName);

            Assert.True(resultArchiveWithExpectedNameExists);
        }

        [Theory]
        [InlineData("yyyy-MM-dd HHmm")]
        [InlineData("y")]
        [InlineData("D")]
        public void FileTarget_WithDateAndSequenceArchiveNumbering_ShouldRespectArchiveDateFormat(
            string archiveDateFormat)
        {
            string archivePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            string archiveFileName = Path.Combine(archivePath, "{#}.log");
            string expectedDateInArchiveFileName = DateTime.Now.ToString(archiveDateFormat);
            string expectedArchiveFullName = String.Format("{0}/{1}.1.log",
                archivePath,
                expectedDateInArchiveFileName);

            // We generate 2 archives so that the algorithm that seeks old archives is also tested.
            GenerateArchives(count: 2, archiveDateFormat: archiveDateFormat, archiveFileName: archiveFileName,
                archiveNumbering: ArchiveNumberingMode.DateAndSequence);
            bool resultArchiveWithExpectedNameExists = File.Exists(expectedArchiveFullName);

            Assert.True(resultArchiveWithExpectedNameExists);
        }

        private void GenerateArchives(int count, string archiveDateFormat, string archiveFileName,
            ArchiveNumberingMode archiveNumbering)
        {
            string logFileName = Path.GetTempFileName();
            const int logFileMaxSize = 1;
            var fileTarget = WrapFileTarget(new FileTarget
            {
                FileName = logFileName,
                ArchiveFileName = archiveFileName,
                ArchiveDateFormat = archiveDateFormat,
                ArchiveNumbering = archiveNumbering,
                ArchiveAboveSize = logFileMaxSize
            });
            SimpleConfigurator.ConfigureForTargetLogging(fileTarget, LogLevel.Debug);
            for (int currentSequenceNumber = 0; currentSequenceNumber < count; currentSequenceNumber++)
                logger.Debug("Test {0}", currentSequenceNumber);
        }

        [Fact]
        public void Dont_throw_Exception_when_archiving_is_enabled()
        {
            try
            {
                LogManager.Configuration = this.CreateConfigurationFromString(@"<?xml version='1.0' encoding='utf-8' ?>
<nlog xmlns='http://www.nlog-project.org/schemas/NLog.xsd'
      xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'
 
      internalLogLevel='Debug'
      throwExceptions='true' >

  <targets>
    <target name='logfile' xsi:type='File' fileName='${basedir}/log.txt' archiveFileName='${basedir}/log.${date}' archiveEvery='Day' archiveNumbering='Date' />
  </targets>

  <rules>
    <logger name='*' writeTo='logfile' />
  </rules>
</nlog>
");

                NLog.LogManager.GetLogger("Test").Info("very important message");
            }
            finally
            {
                LogManager.Configuration = null;
            }
        }


        [Fact]
        public void Dont_throw_Exception_when_archiving_is_enabled_with_async()
        {
            try
            {
                LogManager.Configuration = this.CreateConfigurationFromString(@"<?xml version='1.0' encoding='utf-8' ?>
<nlog xmlns='http://www.nlog-project.org/schemas/NLog.xsd'
      xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'
 
      internalLogLevel='Debug'
      throwExceptions='true' >

  <targets async=""true"" >
    <target  name='logfile' xsi:type='File' fileName='${basedir}/log.txt' archiveFileName='${basedir}/log.${date}' archiveEvery='Day' archiveNumbering='Date' />
  </targets>

  <rules>
    <logger name='*' writeTo='logfile' />
  </rules>
</nlog>
");

                NLog.LogManager.GetLogger("Test").Info("very important message");
            }
            finally
            {
                LogManager.Configuration = null;
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void MaxArchiveFilesWithDate(bool changeCreationAndWriteTime)
        {
            string logdir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            string archivePath = Path.Combine(logdir, "archive");
            TestMaxArchiveFilesWithDate(archivePath, logdir, 2, 2, "yyyyMMdd-HHmm", changeCreationAndWriteTime);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void MaxArchiveFilesWithDate_only_date(bool changeCreationAndWriteTime)
        {
            string logdir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            string archivePath = Path.Combine(logdir, "archive");
            TestMaxArchiveFilesWithDate(archivePath, logdir, 2, 2, "yyyyMMdd", changeCreationAndWriteTime);
        }


        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void MaxArchiveFilesWithDate_only_date2(bool changeCreationAndWriteTime)
        {
            string logdir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            string archivePath = Path.Combine(logdir, "archive");
            TestMaxArchiveFilesWithDate(archivePath, logdir, 2, 2, "yyyy-MM-dd", changeCreationAndWriteTime);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void MaxArchiveFilesWithDate_in_sameDir(bool changeCreationAndWriteTime)
        {
            string logdir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            string archivePath = Path.Combine(logdir, "archive");
            TestMaxArchiveFilesWithDate(archivePath, logdir, 2, 2, "yyyyMMdd-HHmm", changeCreationAndWriteTime);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="archivePath">path to dir of archived files</param>
        /// <param name="logdir">path to dir of logged files</param>
        /// <param name="maxArchiveFilesConfig">max count of archived files</param>
        /// <param name="expectedArchiveFiles">expected count of archived files</param>
        /// <param name="dateFormat">date format</param>
        /// <param name="changeCreationAndWriteTime">change file creation/last write date</param>
        private void TestMaxArchiveFilesWithDate(string archivePath, string logdir,
            int maxArchiveFilesConfig, int expectedArchiveFiles, string dateFormat, bool changeCreationAndWriteTime)
        {
            var archiveDir = new DirectoryInfo(archivePath);
            try
            {
                archiveDir.Create();
                //set-up, create files.

                //same dateformat as in config
                string fileExt = ".log";
                DateTime now = DateTime.Now;
                int i = 0;
                foreach (string filePath in ArchiveFileNamesGenerator(archivePath, dateFormat, fileExt).Take(30))
                {
                    File.WriteAllLines(filePath, new[] { "test archive ", "=====", filePath });
                    var time = now.AddDays(i);
                    if (changeCreationAndWriteTime)
                    {
                        File.SetCreationTime(filePath, time);
                        File.SetLastWriteTime(filePath, time);
                    }
                    i--;
                }

                //create config with archiving
                var configuration = CreateConfigurationFromString(@"
                <nlog throwExceptions='true' >
                    <targets>
                       <target name='fileAll' type='File' 
                            fileName='" + logdir + @"/${date:format=yyyyMMdd-HHmm}" + fileExt + @"'
                            layout='${message}' 
                            archiveEvery='minute' 
                            maxArchiveFiles='" + maxArchiveFilesConfig + @"' 
                            archiveFileName='" + archivePath + @"/{#}.log' 
                            archiveDateFormat='" + dateFormat + @"' 
                            archiveNumbering='Date'/>
     
                    </targets>
                    <rules>
                      <logger name='*' writeTo='fileAll'>
                       
                      </logger>
                    </rules>
                </nlog>");

                LogManager.Configuration = configuration;
                var logger = LogManager.GetCurrentClassLogger();
                logger.Info("test");

                var currentFilesCount = archiveDir.GetFiles().Length;
                Assert.Equal(expectedArchiveFiles, currentFilesCount);
            }
            finally
            {
                //cleanup
                archiveDir.Delete(true);
            }
        }

        /// <summary>
        /// Generate unlimited archivefiles names. Don't use toList on this ;)
        /// </summary>
        /// <param name="path"></param>
        /// <param name="dateFormat"></param>
        /// <param name="fileExt">fileext with .</param>
        /// <returns></returns>
        private static IEnumerable<string> ArchiveFileNamesGenerator(string path, string dateFormat, string fileExt)
        {
            //yyyyMMdd-HHmm
            int dateOffset = 1;
            var now = DateTime.Now;
            while (true)
            {
                dateOffset--;
                yield return Path.Combine(path, now.AddDays(dateOffset).ToString(dateFormat) + fileExt);
            }
        }

        [Fact]
        public void RelativeFileNaming_ShouldSuccess()
        {
            var relativeFileName = @"Logs\myapp.log";
            var fullFilePath = Path.GetFullPath(relativeFileName);
            try
            {
                var fileTarget = WrapFileTarget(new FileTarget
                {
                    FileName = fullFilePath,
                    LineEnding = LineEndingMode.LF,
                    Layout = "${level} ${message}",
                    OpenFileCacheTimeout = 0
                });

                SimpleConfigurator.ConfigureForTargetLogging(fileTarget, LogLevel.Debug);

                logger.Debug("aaa");
                logger.Info("bbb");
                logger.Warn("ccc");
                LogManager.Configuration = null;
                AssertFileContents(fullFilePath, "Debug aaa\nInfo bbb\nWarn ccc\n", Encoding.UTF8);
            }
            finally
            {
                if (File.Exists(fullFilePath))
                    File.Delete(fullFilePath);
            }
        }

        [Fact]
        public void RelativeFileNaming_DirectoryNavigation_ShouldSuccess()
        {
            var relativeFileName = @"..\..\Logs\myapp.log";
            var fullFilePath = Path.GetFullPath(relativeFileName);
            try
            {
                var fileTarget = WrapFileTarget(new FileTarget
                {
                    FileName = fullFilePath,
                    LineEnding = LineEndingMode.LF,
                    Layout = "${level} ${message}",
                    OpenFileCacheTimeout = 0
                });

                SimpleConfigurator.ConfigureForTargetLogging(fileTarget, LogLevel.Debug);

                logger.Debug("aaa");
                logger.Info("bbb");
                logger.Warn("ccc");
                LogManager.Configuration = null;
                AssertFileContents(fullFilePath, "Debug aaa\nInfo bbb\nWarn ccc\n", Encoding.UTF8);
            }
            finally
            {
                if (File.Exists(fullFilePath))
                    File.Delete(fullFilePath);
            }
        }

        [Fact]
        public void RelativeSequentialArchiveTest_MaxArchiveFiles_0()
        {
            var tempPath = Guid.NewGuid().ToString();
            var logfile = Path.Combine(tempPath, "file.txt");
            try
            {
                string archiveFolder = Path.Combine(tempPath, "archive");
                var fileTarget = WrapFileTarget(new FileTarget
                {
                    FileName = logfile,
                    ArchiveFileName = Path.Combine(archiveFolder, "{####}.txt"),
                    ArchiveAboveSize = 1000,
                    LineEnding = LineEndingMode.LF,
                    ArchiveNumbering = ArchiveNumberingMode.Sequence,
                    Layout = "${message}",
                    MaxArchiveFiles = 0
                });

                SimpleConfigurator.ConfigureForTargetLogging(fileTarget, LogLevel.Debug);
                logfile = Path.GetFullPath(logfile);
                // we emit 5 * 250 *(3 x aaa + \n) bytes
                // so that we should get a full file + 4 archives
                Generate1000BytesLog('a');
                Generate1000BytesLog('b');
                Generate1000BytesLog('c');
                Generate1000BytesLog('d');
                Generate1000BytesLog('e');

                LogManager.Configuration = null;

                AssertFileContents(logfile,
                    StringRepeat(250, "eee\n"),
                    Encoding.UTF8);

                AssertFileContents(
                   Path.Combine(archiveFolder, "0000.txt"),
                   StringRepeat(250, "aaa\n"),
                   Encoding.UTF8);

                AssertFileContents(
                    Path.Combine(archiveFolder, "0001.txt"),
                    StringRepeat(250, "bbb\n"),
                    Encoding.UTF8);

                AssertFileContents(
                    Path.Combine(archiveFolder, "0002.txt"),
                    StringRepeat(250, "ccc\n"),
                    Encoding.UTF8);

                AssertFileContents(
                    Path.Combine(archiveFolder, "0003.txt"),
                    StringRepeat(250, "ddd\n"),
                    Encoding.UTF8);

                Assert.True(!File.Exists(Path.Combine(archiveFolder, "0004.txt")));
            }
            finally
            {
                if (File.Exists(logfile))
                    File.Delete(logfile);
                if (Directory.Exists(tempPath))
                    Directory.Delete(tempPath, true);
            }
        }


        protected abstract Target WrapFileTarget(FileTarget target);
    }


    public class PlainFileTargetTests : FileTargetTests
    {
        protected override Target WrapFileTarget(FileTarget target)
        {
            return target;
        }

        [Fact]
        public void BatchErrorHandlingTest()
        {
            var fileTarget = WrapFileTarget(new FileTarget { FileName = "${logger}", Layout = "${message}" });
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
    }


    public class WrappedFileTargetTests : FileTargetTests
    {
        protected override Target WrapFileTarget(FileTarget target)
        {
            return new MockTargetWrapper { WrappedTarget = target };
        }
    }
}

#endif
