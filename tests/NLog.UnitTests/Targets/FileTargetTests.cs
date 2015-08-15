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

    using NLog.Config;
    using NLog.Layouts;
    using NLog.Targets;
    using NLog.Targets.Wrappers;
    using NLog.Time;
    using NLog.Internal;
    using NLog.LayoutRenderers;

    public class FileTargetTests : NLogTestBase
    {
        private readonly ILogger logger = LogManager.GetLogger("NLog.UnitTests.Targets.FileTargetTests");

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
                                        DeleteOldFileOnStartup = false,
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
                             DeleteOldFileOnStartup = false,
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
            var tempFile = Path.GetTempFileName();
            var tempArchiveFolder = Path.Combine(Path.GetTempPath(), "Archive");
            var archiveExtension = enableCompression ? "zip" : "txt";
            try
            {
                // Configure first time with ArchiveOldFileOnStartup = false. 
                var ft = new FileTarget
                {
                    ArchiveOldFileOnStartup = false,
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

                // Configure second time with ArchiveOldFileOnStartup = false again. 
                // Expected behavior: Extra content to be appended to the file.
                ft = new FileTarget
                {
                    ArchiveOldFileOnStartup = false,
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


                // Configure third time with ArchiveOldFileOnStartup = true again. 
                // Expected behavior: Extra content will be stored in a new file; the 
                //      old content should be moved into a new location.

                var archiveTempName = Path.Combine(tempArchiveFolder, "archive." + archiveExtension);

                ft = new FileTarget
                {
#if NET4_5
                    EnableArchiveFileCompression = enableCompression,
#endif
                    FileName = SimpleLayout.Escape(tempFile),
                    LineEnding = LineEndingMode.LF,
                    Layout = "${level} ${message}",
                    ArchiveOldFileOnStartup = true,
                    ArchiveFileName = archiveTempName,
                    ArchiveNumbering = ArchiveNumberingMode.Sequence,
                    MaxArchiveFiles = 1
                };

                SimpleConfigurator.ConfigureForTargetLogging(ft, LogLevel.Debug);
                logger.Debug("ddd");
                logger.Info("eee");
                logger.Warn("fff");

                LogManager.Configuration = null;
                AssertFileContents(tempFile, "Debug ddd\nInfo eee\nWarn fff\n", Encoding.UTF8);
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
                Generate1000BytesLog('a');
                Generate1000BytesLog('b');
                Generate1000BytesLog('c');
                Generate1000BytesLog('d');
                Generate1000BytesLog('e');

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
                //0000 should not extists because of MaxArchiveFiles=3
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
        public void SequentialArchiveTest1_MaxArchiveFiles_0()
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
                    ArchiveNumbering = ArchiveNumberingMode.Sequence,
                    Layout = "${message}",
                    MaxArchiveFiles = 0
                };

                SimpleConfigurator.ConfigureForTargetLogging(ft, LogLevel.Debug);

                // we emit 5 * 250 *(3 x aaa + \n) bytes
                // so that we should get a full file + 4 archives
                Generate1000BytesLog('a');
                Generate1000BytesLog('b');
                Generate1000BytesLog('c');
                Generate1000BytesLog('d');
                Generate1000BytesLog('e');

                LogManager.Configuration = null;

                AssertFileContents(tempFile,
                    StringRepeat(250, "eee\n"),
                    Encoding.UTF8);

                AssertFileContents(
                   Path.Combine(tempPath, "archive/0000.txt"),
                   StringRepeat(250, "aaa\n"),
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

        [Fact(Skip = "this is not supported, because we cannot create multiple archive files with  ArchiveNumberingMode.Date (for one day)")]
        
        public void ArchiveAboveSizeWithArchiveNumberingModeDate_maxfiles_o()
        {
            var tempPath = Path.Combine(Path.GetTempPath(), "ArchiveEveryCombinedWithArchiveAboveSize_" + Guid.NewGuid().ToString());
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
                    ArchiveNumbering = ArchiveNumberingMode.Date
                };

                SimpleConfigurator.ConfigureForTargetLogging(ft, LogLevel.Debug);

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
                AssertFileContents(tempFile,
                    StringRepeat(250, "eee\n"),
                    Encoding.UTF8);

                //DUNNO what to expected!
                //try (which fails)
                AssertFileContents(
                    Path.Combine(tempPath, string.Format("archive/{0}.txt", archiveFileName)),
                   StringRepeat(250, "aaa\n") +  StringRepeat(250, "bbb\n") + StringRepeat(250, "ccc\n") + StringRepeat(250, "ddd\n"),
                    Encoding.UTF8);

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
                    if (i % 5 == 0)
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

                var files2 = Directory.GetFiles(archivePath).OrderBy(s => s);
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
        public void DeleteArchiveFilesByDateWithDateName()
        {
            var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var tempFile = Path.Combine(tempPath, "${date:format=yyyyMMddHHmmssfff}.txt");
            try
            {
                var ft = new FileTarget
                {
                    FileName = tempFile,
                    ArchiveFileName = Path.Combine(tempPath, "{#}.txt"),
                    ArchiveEvery = FileArchivePeriod.Minute,
                    LineEnding = LineEndingMode.LF,
                    ArchiveNumbering = ArchiveNumberingMode.Date,
                    ArchiveDateFormat = "yyyyMMddHHmmssfff", //make sure the milliseconds are set in the filename
                    Layout = "${message}",
                    MaxArchiveFiles = 3
                };

                SimpleConfigurator.ConfigureForTargetLogging(ft, LogLevel.Debug);
                //writing 4 times 10 bytes (9 char + linefeed) will result in 2 archive files and 1 current file
                for (var i = 0; i < 4; ++i)
                {
                    logger.Debug("123456789");
                    //build in a  sleep to make sure the current time is reflected in the filename
                    Thread.Sleep(50);
                }
                //Setting the Configuration to [null] will result in a 'Dump' of the current log entries
                LogManager.Configuration = null;

                var files = Directory.GetFiles(tempPath).OrderBy(s => s);
                //the amount of archived files may not exceed the set 'MaxArchiveFiles'
                Assert.Equal(ft.MaxArchiveFiles, files.Count());


                SimpleConfigurator.ConfigureForTargetLogging(ft, LogLevel.Debug);
                //writing 50ms later will trigger the cleanup of old archived files
                //as stated by the MaxArchiveFiles property, but will only delete the oldest file
                Thread.Sleep(50);
                logger.Debug("123456789");
                LogManager.Configuration = null;

                var files2 = Directory.GetFiles(tempPath).OrderBy(s => s);
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

        public static IEnumerable<object[]> DateArchive_UsesDateFromCurrentTimeSource_TestParameters
        {
            get
            {
                var booleanValues = new[] { true, false };
                var timeKindValues = new[] { DateTimeKind.Utc, DateTimeKind.Local };
                return
                    from concurrentWrites in booleanValues
                    from keepFileOpen in booleanValues
                    from networkWrites in booleanValues
                    from timeKind in timeKindValues
                    select new object[] { timeKind, concurrentWrites, keepFileOpen, networkWrites };
            }
        }


        [Theory]
        [PropertyData("DateArchive_UsesDateFromCurrentTimeSource_TestParameters")]
        public void DateArchive_UsesDateFromCurrentTimeSource(DateTimeKind timeKind, bool concurrentWrites, bool keepFileOpen, bool networkWrites)
        {
            var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var tempFile = Path.Combine(tempPath, "file.txt");
            var defaultTimeSource = TimeSource.Current;
            try
            {
                var timeSource = new TimeSourceTests.ShiftedTimeSource(timeKind);

                TimeSource.Current = timeSource;

                var archiveFileNameTemplate = Path.Combine(tempPath, "archive/{#}.txt");
                var ft = new FileTarget
                {
                    FileName = tempFile,
                    ArchiveFileName = archiveFileNameTemplate,
                    LineEnding = LineEndingMode.LF,
                    ArchiveNumbering = ArchiveNumberingMode.Date,
                    ArchiveEvery = FileArchivePeriod.Day,
                    ArchiveDateFormat = "yyyyMMdd",
                    Layout = "${date:format=O}|${message}",
                    MaxArchiveFiles = 3,
                    ConcurrentWrites = concurrentWrites,
                    KeepFileOpen = keepFileOpen,
                    NetworkWrites = networkWrites,
                };

                SimpleConfigurator.ConfigureForTargetLogging(ft, LogLevel.Debug);

                logger.Debug("123456789");
                DateTime previousWriteTime = timeSource.Time;

                const int daysToTestLogging = 5;
                const int intervalsPerDay = 24;
                var loggingInterval = TimeSpan.FromHours(1);
                for (var i = 0; i < daysToTestLogging * intervalsPerDay; ++i)
                {
                    timeSource.AddToLocalTime(loggingInterval);

                    var eventInfo = new LogEventInfo(LogLevel.Debug, logger.Name, "123456789");
                    logger.Log(eventInfo);

                    var dayIsChanged = eventInfo.TimeStamp.Date != previousWriteTime.Date;
                    // ensure new archive is created only when the day part of time is changed
                    var archiveFileName = archiveFileNameTemplate.Replace("{#}", previousWriteTime.ToString(ft.ArchiveDateFormat));
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

                var archivePath = Path.Combine(tempPath, "archive");
                var files = Directory.GetFiles(archivePath).OrderBy(s => s).ToList();
                //the amount of archived files may not exceed the set 'MaxArchiveFiles'
                Assert.Equal(ft.MaxArchiveFiles, files.Count);


                SimpleConfigurator.ConfigureForTargetLogging(ft, LogLevel.Debug);
                //writing one line on a new day will trigger the cleanup of old archived files
                //as stated by the MaxArchiveFiles property, but will only delete the oldest file
                timeSource.AddToLocalTime(TimeSpan.FromDays(1));
                logger.Debug("1234567890");
                LogManager.Configuration = null;

                var files2 = Directory.GetFiles(archivePath).OrderBy(s => s).ToList();
                Assert.Equal(ft.MaxArchiveFiles, files2.Count);

                //the oldest file should be deleted
                Assert.DoesNotContain(files[0], files2);
                //two files should still be there
                Assert.Equal(files[1], files2[0]);
                Assert.Equal(files[2], files2[1]);
                //one new archive file shoud be created
                Assert.DoesNotContain(files2[2], files);
            }
            finally
            {
                TimeSource.Current = defaultTimeSource; // restore default time source
                LogManager.Configuration = null;
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
                if (Directory.Exists(tempPath))
                    Directory.Delete(tempPath, true);
            }
        }

        [Fact]
        public void DeleteArchiveFilesByDate_MaxArchiveFiles_0()
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
                    MaxArchiveFiles = 0
                };

                SimpleConfigurator.ConfigureForTargetLogging(ft, LogLevel.Debug);
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

                var archivePath = Path.Combine(tempPath, "archive");
                var fileCount = Directory.EnumerateFiles(archivePath).Count();

                Assert.Equal(3, fileCount);

                SimpleConfigurator.ConfigureForTargetLogging(ft, LogLevel.Debug);
                //create 1 new file for archive
                logger.Debug("1234567890");
                LogManager.Configuration = null;

                var fileCount2 = Directory.EnumerateFiles(archivePath).Count();
                //there should be 1 more file
                Assert.Equal(4, fileCount2);
            }
            finally
            {
                LogManager.Configuration = null;

                if (File.Exists(tempFile))
                {
                    File.Delete(tempFile);
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
            RollingArchiveTests(enableCompression: false);
        }

#if NET4_5
        [Fact]
        public void RollingArchiveCompressionTest1()
        {
            RollingArchiveTests(enableCompression: true);
        }
#endif

        private void RollingArchiveTests(bool enableCompression)
        {
            var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var tempFile = Path.Combine(tempPath, "file.txt");
            var archiveExtension = enableCompression ? "zip" : "txt";
            try
            {
                var ft = new FileTarget
                {
#if NET4_5
                    EnableArchiveFileCompression = enableCompression,
#endif
                    FileName = tempFile,
                    ArchiveFileName = Path.Combine(tempPath, "archive/{####}." + archiveExtension),
                    ArchiveAboveSize = 1000,
                    LineEnding = LineEndingMode.LF,
                    ArchiveNumbering = ArchiveNumberingMode.Rolling,
                    Layout = "${message}",
                    MaxArchiveFiles = 3
                };

                SimpleConfigurator.ConfigureForTargetLogging(ft, LogLevel.Debug);

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

                AssertFileContents(tempFile,
                    StringRepeat(250, "eee\n"),
                    Encoding.UTF8);

                assertFileContents(
                    Path.Combine(tempPath, "archive/0000." + archiveExtension),
                    StringRepeat(250, "ddd\n"),
                    Encoding.UTF8);

                assertFileContents(
                    Path.Combine(tempPath, "archive/0001." + archiveExtension),
                    StringRepeat(250, "ccc\n"),
                    Encoding.UTF8);

                assertFileContents(
                    Path.Combine(tempPath, "archive/0002." + archiveExtension),
                    StringRepeat(250, "bbb\n"),
                    Encoding.UTF8);

                Assert.True(!File.Exists(Path.Combine(tempPath, "archive/0003." + archiveExtension)));
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
        public void RollingArchiveTest_MaxArchiveFiles_0()
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
                    MaxArchiveFiles = 0
                };

                SimpleConfigurator.ConfigureForTargetLogging(ft, LogLevel.Debug);

                // we emit 5 * 250 * (3 x aaa + \n) bytes
                // so that we should get a full file + 4 archives
                Generate1000BytesLog('a');
                Generate1000BytesLog('b');
                Generate1000BytesLog('c');
                Generate1000BytesLog('d');
                Generate1000BytesLog('e');

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

                AssertFileContents(
                    Path.Combine(tempPath, "archive/0003.txt"),
                    StringRepeat(250, "aaa\n"),
                    Encoding.UTF8);
            }
            finally
            {
                LogManager.Configuration = null;

                if (File.Exists(tempFile))
                {
                    File.Delete(tempFile);
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
            var tempPath = ArchiveFilenameHelper.GenerateTempPath();
            var tempFile = Path.Combine(tempPath, "file.txt");
            var archiveExtension = enableCompression ? "zip" : "txt";
            try
            {
                var ft = new FileTarget
                {
#if NET4_5
                    EnableArchiveFileCompression = enableCompression,
#endif
                    FileName = tempFile,
                    ArchiveFileName = Path.Combine(tempPath, "archive/{#}." + archiveExtension),
                    ArchiveDateFormat = "yyyy-MM-dd",
                    ArchiveAboveSize = 1000,
                    LineEnding = LineEndingMode.LF,
                    Layout = "${message}",
                    MaxArchiveFiles = 3,
                    ArchiveNumbering = ArchiveNumberingMode.DateAndSequence,
                    ArchiveEvery = FileArchivePeriod.Day
                };

                SimpleConfigurator.ConfigureForTargetLogging(ft, LogLevel.Debug);

                // we emit 5 * 250 *(3 x aaa + \n) bytes
                // so that we should get a full file + 3 archives
                Generate1000BytesLog('a');
                Generate1000BytesLog('b');
                Generate1000BytesLog('c');
                Generate1000BytesLog('d');
                Generate1000BytesLog('e');

                string archiveFilename = DateTime.Now.ToString(ft.ArchiveDateFormat);

                LogManager.Configuration = null;


#if NET4_5
                var assertFileContents = enableCompression ? new Action<string, string, Encoding>(AssertZipFileContents) : AssertFileContents;
#else
                var assertFileContents = new Action<string, string, Encoding>(AssertFileContents);
#endif
                ArchiveFilenameHelper helper = new ArchiveFilenameHelper(Path.Combine(tempPath, "archive"), archiveFilename, archiveExtension);

                AssertFileContents(tempFile,
                    StringRepeat(250, "eee\n"),
                    Encoding.UTF8);

                assertFileContents(helper.GetFullPath(1), StringRepeat(250, "bbb\n"), Encoding.UTF8);
                AssertFileSize(helper.GetFullPath(1), ft.ArchiveAboveSize);

                assertFileContents(helper.GetFullPath(2), StringRepeat(250, "ccc\n"), Encoding.UTF8);
                AssertFileSize(helper.GetFullPath(2), ft.ArchiveAboveSize);

                assertFileContents(helper.GetFullPath(3), StringRepeat(250, "ddd\n"), Encoding.UTF8);
                AssertFileSize(helper.GetFullPath(3), ft.ArchiveAboveSize);

                Assert.True(!helper.Exists(0), "old one removed - max files");
                Assert.True(!helper.Exists(4), "stop at 3");
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

                var ft = new FileTarget
                {
                    FileName = logFile,
                    ArchiveFileName = archiveFile,
                    ArchiveAboveSize = 1, //Force immediate archival
                    ArchiveNumbering = ArchiveNumberingMode.DateAndSequence,
                    MaxArchiveFiles = 5
                };

                SimpleConfigurator.ConfigureForTargetLogging(ft, LogLevel.Debug);

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
                LogManager.Configuration = null;

                if (tempDirectory.Exists)
                {
                    tempDirectory.Delete(true);
                }
            }

        }

        [Fact]
        public void Single_Archive_File_Rolls_Correctly()
        {
            var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var tempFile = Path.Combine(tempPath, "file.txt");
            try
            {
                var ft = new FileTarget
                {
                    FileName = tempFile,
                    ArchiveFileName = Path.Combine(tempPath, "archive/file.txt2"),
                    ArchiveAboveSize = 1000,
                    LineEnding = LineEndingMode.LF,
                    Layout = "${message}",
                    MaxArchiveFiles = 1,
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

                LogManager.Configuration = null;

                AssertFileContents(tempFile,
                    StringRepeat(250, "bbb\n"),
                    Encoding.UTF8);

                AssertFileContents(
                    Path.Combine(tempPath, "archive/file.txt2"),
                    StringRepeat(250, "aaa\n"),
                    Encoding.UTF8);
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

        /// <summary>
        /// Remove archived files in correct order
        /// </summary>
        [Fact]
        public void FileTarget_ArchiveNumbering_remove_correct_order()
        {
            var tempPath = ArchiveFilenameHelper.GenerateTempPath();
            var tempFile = Path.Combine(tempPath, "file.txt");
            var archiveExtension = "txt";
            try
            {
                var maxArchiveFiles = 10;
                var ft = new FileTarget
                {
                    FileName = tempFile,
                    ArchiveFileName = Path.Combine(tempPath, "archive/{#}." + archiveExtension),
                    ArchiveDateFormat = "yyyy-MM-dd",
                    ArchiveAboveSize = 1000,
                    LineEnding = LineEndingMode.LF,
                    Layout = "${message}",
                    MaxArchiveFiles = maxArchiveFiles,
                    ArchiveNumbering = ArchiveNumberingMode.DateAndSequence,
                };

                SimpleConfigurator.ConfigureForTargetLogging(ft, LogLevel.Debug);


                ArchiveFilenameHelper helper = new ArchiveFilenameHelper(Path.Combine(tempPath, "archive"), DateTime.Now.ToString(ft.ArchiveDateFormat), archiveExtension);

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
                LogManager.Configuration = null;
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
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
        private class ArchiveFilenameHelper
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
            public ArchiveFilenameHelper(string folderName, string fileName, string ext)
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
    }
}

#endif
