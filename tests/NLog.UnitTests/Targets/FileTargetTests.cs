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

namespace NLog.UnitTests.Targets
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using Mocks;
    using NLog.Config;
    using NLog.Layouts;
    using NLog.Targets;
    using NLog.Targets.Wrappers;
    using NLog.Time;
    using NSubstitute;
    using NSubstitute.Core;
    using Xunit;

    public class FileTargetTests : NLogTestBase
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
                    from forceMutexConcurrentWrites in booleanValues
                    where UniqueBaseAppender(concurrentWrites, keepFileOpen, networkWrites, forceMutexConcurrentWrites)
                    from forceManaged in booleanValues
                    select new object[] { concurrentWrites, keepFileOpen, networkWrites, forceManaged, forceMutexConcurrentWrites };
            }
        }

        private static bool UniqueBaseAppender(bool concurrentWrites, bool keepFileOpen, bool networkWrites, bool forceMutexConcurrentWrites)
        {
            if (!concurrentWrites && !networkWrites && !forceMutexConcurrentWrites)
                return true;    // Allow keepFileOpen = true / false, Allow forceManaged = true / false
            if (concurrentWrites && !networkWrites && keepFileOpen)
                return true;    // Allow forceManaged = true / false, forceMutexConcurrentWrites = true / false
            if (networkWrites && keepFileOpen && !concurrentWrites && !forceMutexConcurrentWrites)
                return true;    // Allow forceManaged = true / false
            return false;
        }

        [Theory]
        [MemberData(nameof(SimpleFileTest_TestParameters))]
        public void SimpleFileTest(bool concurrentWrites, bool keepFileOpen, bool networkWrites, bool forceManaged, bool forceMutexConcurrentWrites)
        {
            var logFile = Path.GetTempFileName();
            try
            {
                var fileTarget = new FileTarget
                {
                    FileName = SimpleLayout.Escape(logFile),
                    LineEnding = LineEndingMode.LF,
                    Layout = "${level} ${message}",
                    OpenFileCacheTimeout = 0,
                    ConcurrentWrites = concurrentWrites,
                    KeepFileOpen = keepFileOpen,
                    NetworkWrites = networkWrites,
                    ForceManaged = forceManaged,
                    ForceMutexConcurrentWrites = forceMutexConcurrentWrites,
                };

                SimpleConfigurator.ConfigureForTargetLogging(fileTarget, LogLevel.Debug);

                logger.Debug("aaa");
                logger.Info("bbb");
                logger.Warn("ccc");

                LogManager.Configuration = null;    // Flush

                AssertFileContents(logFile, "Debug aaa\nInfo bbb\nWarn ccc\n", Encoding.UTF8);
            }
            finally
            {
                if (File.Exists(logFile))
                    File.Delete(logFile);
            }
        }

        [Theory]
        [MemberData(nameof(SimpleFileTest_TestParameters))]
        public void SimpleFileDeleteTest(bool concurrentWrites, bool keepFileOpen, bool networkWrites, bool forceManaged, bool forceMutexConcurrentWrites)
        {
            bool isSimpleKeepFileOpen = keepFileOpen && !networkWrites && !concurrentWrites && IsLinux();
#if MONO
            if (IsLinux() && concurrentWrites && keepFileOpen && !networkWrites)
            {
                Console.WriteLine("[SKIP] FileTargetTests.SimpleFileDeleteTest Not supported on MONO on Travis, because of FileSystemWatcher not working");
                return;
            }
#endif

            RetryingIntegrationTest(3, () =>
            {
                var logFile = Path.GetTempFileName();
                var logFile2 = Path.Combine(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()), Path.GetFileName(logFile));

                try
                {
                    var fileTarget = new FileTarget
                    {
                        FileName = SimpleLayout.Escape(logFile),
                        LineEnding = LineEndingMode.LF,
                        Layout = "${level} ${message}",
                        OpenFileCacheTimeout = 0,
                        EnableFileDelete = true,
                        ConcurrentWrites = concurrentWrites,
                        KeepFileOpen = keepFileOpen,
                        NetworkWrites = networkWrites,
                        ForceManaged = forceManaged,
                        ForceMutexConcurrentWrites = forceMutexConcurrentWrites,
                    };

                    SimpleConfigurator.ConfigureForTargetLogging(fileTarget, LogLevel.Debug);

                    logger.Debug("aaa");

                    LogManager.Flush();

                    Directory.CreateDirectory(Path.GetDirectoryName(logFile2));
                    File.Move(logFile, logFile2);

                    if (isSimpleKeepFileOpen)
                        Thread.Sleep(1500); // Ensure EnableFileDeleteSimpleMonitor will trigger
                    else if (keepFileOpen && !networkWrites)
                        Thread.Sleep(150); // Allow AutoClose-Timer-Thread to react (FileWatcher schedules timer after 50 msec)

                    logger.Info("bbb");

                    LogManager.Configuration = null;

                    AssertFileContents(logFile, "Info bbb\n", Encoding.UTF8);
                }
                finally
                {
                    if (File.Exists(logFile2))
                    {
                        File.Delete(logFile2);
                        Directory.Delete(Path.GetDirectoryName(logFile2));
                    }

                    if (File.Exists(logFile))
                        File.Delete(logFile);
                }
            });
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
                var dirPath = "C:\\";
                var directoryInfo = new DirectoryInfo(dirPath);

                if (directoryInfo.Exists)
                {
                    return;
                }

                var logFile = dirPath + "nlog-test.log";
                SimpleFileWriteLogTest(logFile);
            }
        }


        [Fact]
        public void SimpleFileWithSpecialCharsTest()
        {
            var logFile = Path.Combine(Path.GetTempPath(), "nlog_" + Guid.NewGuid() + "!@#$%^&()_-=+ .log");
            SimpleFileWriteLogTest(logFile);
        }

        private void SimpleFileWriteLogTest(string logFile)
        {
            try
            {
                var fileTarget = new FileTarget
                {
                    FileName = SimpleLayout.Escape(logFile),
                    LineEnding = LineEndingMode.LF,
                    Layout = "${level} ${message}",
                };

                SimpleConfigurator.ConfigureForTargetLogging(fileTarget, LogLevel.Debug);

                logger.Debug("aaa");
                logger.Info("bbb");
                logger.Warn("ccc");

                LogManager.Configuration = null; // Flush

                AssertFileContents(logFile, "Debug aaa\nInfo bbb\nWarn ccc\n", Encoding.UTF8);
            }
            finally
            {
                if (File.Exists(logFile))
                    File.Delete(logFile);
            }
        }

        [Fact]
        public void SimpleFileTestWriteBom()
        {
            var logFile = Path.GetTempFileName();
            try
            {
                var fileTarget = new FileTarget
                {
                    FileName = SimpleLayout.Escape(logFile),
                    LineEnding = LineEndingMode.LF,
                    Encoding = Encoding.UTF8,
                    WriteBom = true,
                    Layout = "${level} ${message}",
                };

                SimpleConfigurator.ConfigureForTargetLogging(fileTarget, LogLevel.Debug);

                logger.Debug("aaa");

                LogManager.Configuration = null;    // Flush

                AssertFileContents(logFile, "Debug aaa\n", Encoding.UTF8, true);
            }
            finally
            {
                if (File.Exists(logFile))
                    File.Delete(logFile);
            }
        }

#if !MONO
        /// <summary>
        /// If a drive doesn't existing, before repeatably creating a dir was tried. This test was taking +60 seconds 
        /// </summary>
        [Theory]
        [MemberData(nameof(SimpleFileTest_TestParameters))]
        public void NonExistingDriveShouldNotDelayMuch(bool concurrentWrites, bool keepFileOpen, bool networkWrites, bool forceManaged, bool forceMutexConcurrentWrites)
        {
            var nonExistingDrive = GetFirstNonExistingDriveWindows();

            var logFile = nonExistingDrive + "://dont-extist/no-timeout.log";

            DateTime start = DateTime.UtcNow;

            try
            {
                using (new NoThrowNLogExceptions())
                {
                    var fileTarget = new FileTarget
                    {
                        FileName = logFile,
                        Layout = "${level} ${message}",
                        ConcurrentWrites = concurrentWrites,
                        KeepFileOpen = keepFileOpen,
                        NetworkWrites = networkWrites,
                        ForceManaged = forceManaged,
                        ForceMutexConcurrentWrites = forceMutexConcurrentWrites,
                    };

                    SimpleConfigurator.ConfigureForTargetLogging(fileTarget, LogLevel.Debug);

                    for (int i = 0; i < 300; i++)
                    {
                        logger.Debug("aaa");
                    }

                    LogManager.Configuration = null;    // Flush

                    Assert.True(DateTime.UtcNow - start < TimeSpan.FromSeconds(5));
                }
            }
            finally
            {
                //should not be necessary
                if (File.Exists(logFile))
                    File.Delete(logFile);
            }
        }

        /// <summary>
        /// Get first drive letter of non-existing drive
        /// </summary>
        /// <returns></returns>
        private static char GetFirstNonExistingDriveWindows()
        {
            var existingDrives = new HashSet<string>(Environment.GetLogicalDrives().Select(d => d[0].ToString()),
                StringComparer.OrdinalIgnoreCase);
            var nonExistingDrive =
                "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToList().First(driveLetter => !existingDrives.Contains(driveLetter.ToString()));
            return nonExistingDrive;
        }

#endif

        [Fact]
        public void RollingArchiveEveryMonth()
        {
            var tempPath = Path.Combine(Path.GetTempPath(), "nlog_" + Guid.NewGuid().ToString());
            var defaultTimeSource = TimeSource.Current;

            try
            {
                var timeSource = new TimeSourceTests.ShiftedTimeSource(DateTimeKind.Local);
                if (timeSource.Time.Minute == 59)
                {
                    // Avoid double-archive due to overflow of the hour.
                    timeSource.AddToLocalTime(TimeSpan.FromMinutes(1));
                    timeSource.AddToSystemTime(TimeSpan.FromMinutes(1));
                }
                TimeSource.Current = timeSource;

                var fileTarget = new FileTarget
                {
                    FileName = Path.Combine(tempPath, "${date:format=dd}_AppName.log"),
                    LineEnding = LineEndingMode.LF,
                    Layout = "${message}",
                    ArchiveNumbering = ArchiveNumberingMode.Rolling,
                    ArchiveEvery = FileArchivePeriod.Month,
                    MaxArchiveFiles = 1,
                };

                SimpleConfigurator.ConfigureForTargetLogging(fileTarget, LogLevel.Debug);
                for (int i = 0; i < 12; ++i)
                {
                    for (int j = 0; j < 31; ++j)
                    {
                        logger.Debug("aaa");
                        timeSource.AddToLocalTime(TimeSpan.FromDays(1));
                        timeSource.AddToSystemTime(TimeSpan.FromDays(1));
                    }
                }

                var files = Directory.GetFiles(tempPath);
                // Cleanup doesn't work, as all file names has the same timestamp
                if (files.Length < 28 || files.Length > 31)
                    Assert.Equal(30, files.Length);

                foreach (var file in files)
                {
                    Assert.Equal(14, Path.GetFileName(file).Length);
                }
            }
            finally
            {
                TimeSource.Current = defaultTimeSource;

                if (Directory.Exists(tempPath))
                    Directory.Delete(tempPath, true);
            }
        }

#if !MONO
        [Theory]
#else
        [Theory(Skip="Not supported on MONO on Travis, because of File birthtime not working")]
#endif
        [InlineData(false, false, ArchiveNumberingMode.DateAndSequence)]
        [InlineData(false, true, ArchiveNumberingMode.DateAndSequence)]
        [InlineData(false, false, ArchiveNumberingMode.Sequence)]
        [InlineData(false, true, ArchiveNumberingMode.Sequence)]
        [InlineData(true, false, ArchiveNumberingMode.DateAndSequence)]
        [InlineData(true, true, ArchiveNumberingMode.DateAndSequence)]
        [InlineData(true, false, ArchiveNumberingMode.Sequence)]
        [InlineData(true, true, ArchiveNumberingMode.Sequence)]
        public void DatedArchiveEveryMonth(bool archiveSubFolder, bool maxArchiveDays, ArchiveNumberingMode archiveNumberingMode)
        {
            if (IsLinux())
            {
                Console.WriteLine("[SKIP] FileTargetTests.DatedArchiveEveryMonth because SetCreationTime is not working on Travis");
                return;
            }

            var tempPath = Path.Combine(Path.GetTempPath(), "nlog_" + Guid.NewGuid().ToString());
            var logFile = Path.Combine(tempPath, "AppName.log");
            var archivePath = archiveSubFolder ? Path.Combine(tempPath, "Archive") : tempPath;

            var defaultTimeSource = TimeSource.Current;

            try
            {
                var timeSource = new TimeSourceTests.ShiftedTimeSource(DateTimeKind.Local);
                if (timeSource.Time.Minute == 59)
                {
                    // Avoid double-archive due to overflow of the hour.
                    timeSource.AddToLocalTime(TimeSpan.FromMinutes(1));
                    timeSource.AddToSystemTime(TimeSpan.FromMinutes(1));
                }
                TimeSource.Current = timeSource;

                // Generate 4 files with the following contents
                //  000 - 6 Months Old (Deleted)
                //  111 - 4 Months Old
                //  222 - 2 Months Old
                //  333 - Current
                List<string> createdFiles = new List<string>();
                List<string> currentFiles = new List<string>();
                for (int i = 0; i < 4; ++i)
                {
                    if (i != 0)
                    {
                        // Make the files 2 months older, and lets try again
                        for (int x = 0; x < createdFiles.Count; ++x)
                        {
                            var existingFile = createdFiles[x];
                            var monthsOld = x == 0 ? 2 : ((i - x) * 2 + 2);
                            File.SetCreationTime(existingFile, DateTime.Now.AddDays(-(32 * monthsOld)));
                        }

                        timeSource.AddToLocalTime(TimeSpan.FromDays(32 * 3));
                        timeSource.AddToSystemTime(TimeSpan.FromDays(32 * 3));
                    }

                    var fileTarget = new FileTarget
                    {
                        FileName = logFile,
                        LineEnding = LineEndingMode.LF,
                        Encoding = Encoding.ASCII,
                        Layout = "${message}",
                        KeepFileOpen = i % 2 != 0,
                        ArchiveFileName = archiveSubFolder ? Path.Combine(archivePath, "AppName.{#}.log") : (Layout)null,
                        ArchiveNumbering = archiveNumberingMode,
                        ArchiveEvery = FileArchivePeriod.Month,
                        ArchiveDateFormat = "yyyyMMdd",
                        MaxArchiveFiles = maxArchiveDays ? 0 : 2,
                        MaxArchiveDays = maxArchiveDays ? 5 * 30 : 0
                    };

                    SimpleConfigurator.ConfigureForTargetLogging(fileTarget, LogLevel.Debug);
                    logger.Debug($"{i.ToString()}{i.ToString()}{i.ToString()}");
                    LogManager.Configuration = null;    // Flush

                    currentFiles = Directory.GetFiles(tempPath).ToList();
                    if (archiveSubFolder && Directory.Exists(archivePath))
                        currentFiles.AddRange(Directory.GetFiles(archivePath));

                    string newFile = string.Empty;
                    foreach (var fileName in currentFiles)
                    {
                        if (!createdFiles.Contains(fileName))
                        {
                            Assert.Empty(newFile);
                            newFile = fileName;

                            if (archiveNumberingMode == ArchiveNumberingMode.DateAndSequence && createdFiles.Count > 1)
                            {
                                // Verify it used the last-modified-time (And not file-creation-time)
                                string dateName = string.Empty;
                                dateName = Path.GetFileName(fileName);
                                dateName = dateName.Replace("AppName.", "");
                                dateName = dateName.Replace(".0.log", "");
                                dateName = dateName.Replace("log", "");
                                Assert.NotEmpty(dateName);
                                Assert.Equal(timeSource.Time.Month, DateTime.ParseExact(dateName, "yyyyMMdd", null).Month);
                            }
                        }
                    }

                    Assert.False(string.IsNullOrEmpty(newFile), $"Missing new file. OldFileCount={createdFiles.Count}, NewFileCount={currentFiles.Count}");
                    createdFiles.Add(newFile);
                }

                Assert.Equal(3, currentFiles.Count);
                AssertFileContents(logFile, "333\n", Encoding.ASCII);
            }
            finally
            {
                TimeSource.Current = defaultTimeSource;

                if (Directory.Exists(archivePath))
                    Directory.Delete(archivePath, true);

                if (Directory.Exists(tempPath))
                    Directory.Delete(tempPath, true);
            }
        }

        [Fact]
        public void CsvHeaderTest()
        {
            var tempPath = Path.Combine(Path.GetTempPath(), "nlog_" + Guid.NewGuid().ToString());
            var logFile = Path.Combine(tempPath, "log.log");
            if (Path.DirectorySeparatorChar == '\\')
                logFile = logFile.Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

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

                    var fileTarget = new FileTarget
                    {
                        FileName = SimpleLayout.Escape(logFile),
                        LineEnding = LineEndingMode.LF,
                        Layout = layout,
                        OpenFileCacheTimeout = 0,
                        ReplaceFileContentsOnEachWrite = false,
                        ArchiveAboveSize = 120, // Only 2 LogEvents per file
                        MaxArchiveFiles = 1,
                    };
                    SimpleConfigurator.ConfigureForTargetLogging(fileTarget, LogLevel.Debug);

                    if (i == 0)
                    {
                        for (int j = 0; j < 3; j++)
                            logger.Debug("aaa");

                        LogManager.Configuration = null;    // Flush

                        // See that the 3rd LogEvent was placed in its own file
                        AssertFileContents(logFile, "name;level;message\nNLog.UnitTests.Targets.FileTargetTests;Debug;aaa\n", Encoding.UTF8);
                    }
                    else
                    {
                        logger.Debug("aaa");
                    }
                }

                // See that opening closing 
                AssertFileContents(logFile, "name;level;message\nNLog.UnitTests.Targets.FileTargetTests;Debug;aaa\nNLog.UnitTests.Targets.FileTargetTests;Debug;aaa\n", Encoding.UTF8);

                Assert.NotEqual(3, Directory.GetFiles(tempPath).Count());   // See that archive cleanup worked
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
        public void DeleteFileOnStartTest()
        {
            var logFile = Path.GetTempFileName();
            try
            {
                var fileTarget = new FileTarget
                {
                    DeleteOldFileOnStartup = false,
                    FileName = SimpleLayout.Escape(logFile),
                    LineEnding = LineEndingMode.LF,
                    Layout = "${level} ${message}"
                };

                SimpleConfigurator.ConfigureForTargetLogging(fileTarget, LogLevel.Debug);

                logger.Debug("aaa");
                logger.Info("bbb");
                logger.Warn("ccc");

                LogManager.Configuration = null;

                AssertFileContents(logFile, "Debug aaa\nInfo bbb\nWarn ccc\n", Encoding.UTF8);

                // configure again, without
                // DeleteOldFileOnStartup

                fileTarget = new FileTarget
                {
                    DeleteOldFileOnStartup = false,
                    FileName = SimpleLayout.Escape(logFile),
                    LineEnding = LineEndingMode.LF,
                    Layout = "${level} ${message}"
                };

                SimpleConfigurator.ConfigureForTargetLogging(fileTarget, LogLevel.Debug);

                logger.Debug("aaa");
                logger.Info("bbb");
                logger.Warn("ccc");

                LogManager.Configuration = null;
                AssertFileContents(logFile, "Debug aaa\nInfo bbb\nWarn ccc\nDebug aaa\nInfo bbb\nWarn ccc\n", Encoding.UTF8);

                // configure again, this time with
                // DeleteOldFileOnStartup

                fileTarget = new FileTarget
                {
                    FileName = SimpleLayout.Escape(logFile),
                    LineEnding = LineEndingMode.LF,
                    Layout = "${level} ${message}",
                    DeleteOldFileOnStartup = true
                };

                SimpleConfigurator.ConfigureForTargetLogging(fileTarget, LogLevel.Debug);
                logger.Debug("aaa");
                logger.Info("bbb");
                logger.Warn("ccc");

                LogManager.Configuration = null;    // Flush

                AssertFileContents(logFile, "Debug aaa\nInfo bbb\nWarn ccc\n", Encoding.UTF8);
            }
            finally
            {
                if (File.Exists(logFile))
                    File.Delete(logFile);
            }
        }

        /// <summary>
        /// todo not needed to execute twice.
        /// </summary>
        [Fact]
        public void DeleteFileOnStartTest_noExceptionWhenMissing()
        {
            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"<nlog throwExceptions='true'>
    <targets>
      <target name='file1' encoding='UTF-8' type='File'  deleteOldFileOnStartup='true' fileName='c://temp2/logs/i-dont-exist.log' layout='${message} ' />
    </targets>
    <rules>
      <logger name='*' minlevel='Trace' writeTo='file1' />
    </rules>
</nlog>
");
            var logger = LogManager.GetCurrentClassLogger();
            logger.Trace("running test");
        }

#if !NETSTANDARD
        public static IEnumerable<object[]> ArchiveFileOnStartTests_TestParameters
        {
            get
            {
                var booleanValues = new[] { true, false };
                return
                    from enableCompression in booleanValues
                    from customFileCompressor in booleanValues
                    select new object[] { enableCompression, customFileCompressor };
            }
        }
#else
        public static IEnumerable<object[]> ArchiveFileOnStartTests_TestParameters
        {
            get
            {
                var booleanValues = new[] { true, false };
                return
                    from enableCompression in booleanValues
                    select new object[] { enableCompression, false };
            }
        }
#endif

        [Theory]
        [MemberData(nameof(ArchiveFileOnStartTests_TestParameters))]
        public void ArchiveFileOnStartTests(bool enableCompression, bool customFileCompressor)
        {
            var logFile = Path.GetTempFileName() + ".txt";
            var tempArchiveFolder = Path.Combine(Path.GetTempPath(), "Archive");
            var archiveExtension = enableCompression ? "zip" : "txt";
            IFileCompressor fileCompressor = null;
            try
            {
                if (customFileCompressor)
                {
                    fileCompressor = FileTarget.FileCompressor;
                    FileTarget.FileCompressor = new CustomFileCompressor();
                }

                // Configure first time with ArchiveOldFileOnStartup = false. 
                var fileTarget = new FileTarget
                {
                    ArchiveOldFileOnStartup = false,
                    FileName = SimpleLayout.Escape(logFile),
                    LineEnding = LineEndingMode.LF,
                    Layout = "${level} ${message}"
                };

                SimpleConfigurator.ConfigureForTargetLogging(fileTarget, LogLevel.Debug);

                logger.Debug("aaa");
                logger.Info("bbb");
                logger.Warn("ccc");

                LogManager.Configuration = null;

                AssertFileContents(logFile, "Debug aaa\nInfo bbb\nWarn ccc\n", Encoding.UTF8);

                // Configure second time with ArchiveOldFileOnStartup = false again. 
                // Expected behavior: Extra content to be appended to the file.
                fileTarget = new FileTarget
                {
                    ArchiveOldFileOnStartup = false,
                    FileName = SimpleLayout.Escape(logFile),
                    LineEnding = LineEndingMode.LF,
                    Layout = "${level} ${message}"
                };

                SimpleConfigurator.ConfigureForTargetLogging(fileTarget, LogLevel.Debug);

                logger.Debug("aaa");
                logger.Info("bbb");
                logger.Warn("ccc");

                LogManager.Configuration = null;    // Flush
                AssertFileContents(logFile, "Debug aaa\nInfo bbb\nWarn ccc\nDebug aaa\nInfo bbb\nWarn ccc\n", Encoding.UTF8);


                // Configure third time with ArchiveOldFileOnStartup = true again. 
                // Expected behavior: Extra content will be stored in a new file; the 
                //      old content should be moved into a new location.

                var archiveTempName = Path.Combine(tempArchiveFolder, "archive." + archiveExtension);

                FileTarget ft;
                fileTarget = ft = new FileTarget
                {
                    EnableArchiveFileCompression = enableCompression,
                    FileName = SimpleLayout.Escape(logFile),
                    LineEnding = LineEndingMode.LF,
                    Layout = "${level} ${message}",
                    ArchiveOldFileOnStartup = true,
                    ArchiveFileName = archiveTempName,
                    ArchiveNumbering = ArchiveNumberingMode.Sequence,
                    MaxArchiveFiles = 1
                };

                SimpleConfigurator.ConfigureForTargetLogging(fileTarget, LogLevel.Debug);
                logger.Debug("ddd");
                logger.Info("eee");
                logger.Warn("fff");

                LogManager.Configuration = null;    // Flush

                AssertFileContents(logFile, "Debug ddd\nInfo eee\nWarn fff\n", Encoding.UTF8);
                Assert.True(File.Exists(archiveTempName));

                var assertFileContents = ft.EnableArchiveFileCompression ?
                    new Action<string, string, string, Encoding>(AssertZipFileContents) :
                    AssertFileContents;

#if !NET35
                string expectedEntryName = Path.GetFileNameWithoutExtension(archiveTempName) + ".txt";
#else
                string expectedEntryName = Path.GetFileName(logFile);
#endif
                assertFileContents(archiveTempName, expectedEntryName, "Debug aaa\nInfo bbb\nWarn ccc\nDebug aaa\nInfo bbb\nWarn ccc\n",
                    Encoding.UTF8);
            }
            finally
            {
                if (customFileCompressor)
                    FileTarget.FileCompressor = fileCompressor;
                if (File.Exists(logFile))
                    File.Delete(logFile);
                if (Directory.Exists(tempArchiveFolder))
                    Directory.Delete(tempArchiveFolder, true);
            }
        }

        [Fact]
        public void ArchiveOldFileOnStartupAboveSize()
        {
            var logFile = Path.GetTempFileName();
            var tempArchiveFolder = Path.Combine(Path.GetTempPath(), "Archive");
            var archiveTempName = Path.Combine(tempArchiveFolder, "archive_size_threshold.txt");
            FileTarget CreateTestTarget(long threshold)
            {
                return new FileTarget
                {
                    FileName = SimpleLayout.Escape(logFile),
                    LineEnding = LineEndingMode.LF,
                    Layout = "${level} ${message}",
                    ArchiveOldFileOnStartupAboveSize = threshold,
                    ArchiveFileName = archiveTempName,
                    ArchiveNumbering = ArchiveNumberingMode.Sequence,
                    MaxArchiveFiles = 1
                };
            }
            try
            {
                // No archive on startup (ignoring threshold)
                SimpleConfigurator.ConfigureForTargetLogging(CreateTestTarget(1000));
                logger.Info("aaa");
                LogManager.Flush();
                AssertFileContents(logFile, "Info aaa\n", Encoding.UTF8);
                Assert.False(File.Exists(archiveTempName));

                // Archive on startup with small threshold -> Must be archived
                SimpleConfigurator.ConfigureForTargetLogging(CreateTestTarget(3));
                logger.Info("ccc");
                LogManager.Flush();
                AssertFileContents(logFile, "Info ccc\n", Encoding.UTF8);
                Assert.True(File.Exists(archiveTempName));
                AssertFileContents(archiveTempName, "Info aaa\n", Encoding.UTF8);
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
        [MemberData(nameof(ReplaceFileContentsOnEachWriteTest_TestParameters))]
        public void ReplaceFileContentsOnEachWriteTest(bool useHeader, bool useFooter)
        {
            const string header = "Headerline", footer = "Footerline";

            var logFile = Path.GetTempFileName();
            try
            {
                var fileTarget = new FileTarget
                {
                    DeleteOldFileOnStartup = false,
                    FileName = SimpleLayout.Escape(logFile),
                    ReplaceFileContentsOnEachWrite = true,
                    LineEnding = LineEndingMode.LF,
                    Layout = "${level} ${message}"
                };
                if (useHeader)
                    fileTarget.Header = header;
                if (useFooter)
                    fileTarget.Footer = footer;

                SimpleConfigurator.ConfigureForTargetLogging(fileTarget, LogLevel.Debug);

                string headerPart = useHeader ? header + LineEndingMode.LF.NewLineCharacters : string.Empty;
                string footerPart = useFooter ? footer + LineEndingMode.LF.NewLineCharacters : string.Empty;

                logger.Debug("aaa");
                LogManager.Flush();
                AssertFileContents(logFile, headerPart + "Debug aaa\n" + footerPart, Encoding.UTF8);

                logger.Info("bbb");
                LogManager.Flush();
                AssertFileContents(logFile, headerPart + "Info bbb\n" + footerPart, Encoding.UTF8);

                logger.Warn("ccc");
                LogManager.Flush();
                AssertFileContents(logFile, headerPart + "Warn ccc\n" + footerPart, Encoding.UTF8);
            }
            finally
            {
                if (File.Exists(logFile))
                    File.Delete(logFile);
            }
        }


        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ReplaceFileContentsOnEachWrite_CreateDirs(bool createDirs)
        {
            var tempPath = Path.Combine(Path.GetTempPath(), "dir_" + Guid.NewGuid().ToString());
            var logfile = Path.Combine(tempPath, "log.log");

            try
            {
                using (new NoThrowNLogExceptions())
                {
                    var target = new FileTarget
                    {
                        FileName = logfile,
                        ReplaceFileContentsOnEachWrite = true,
                        CreateDirs = createDirs
                    };
                    var config = new LoggingConfiguration();

                    config.AddTarget("logfile", target);

                    config.AddRuleForAllLevels(target);

                    LogManager.Configuration = config;

                    ILogger logger = LogManager.GetLogger("A");
                    logger.Info("a");

                    Assert.Equal(createDirs, Directory.Exists(tempPath));
                }
            }
            finally
            {
                if (File.Exists(logfile))
                    File.Delete(logfile);
                if (Directory.Exists(tempPath))
                    Directory.Delete(tempPath, true);
            }
        }

        [Fact]
        public void CreateDirsTest()
        {
            var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var logFile = Path.Combine(tempPath, "file.txt");
            try
            {
                var fileTarget = new FileTarget
                {
                    FileName = logFile,
                    LineEnding = LineEndingMode.LF,
                    Layout = "${level} ${message}"
                };

                SimpleConfigurator.ConfigureForTargetLogging(fileTarget, LogLevel.Debug);

                logger.Debug("aaa");
                logger.Info("bbb");
                logger.Warn("ccc");

                LogManager.Configuration = null;    // Flush
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

        [Theory]
        [InlineData(true, 0)]
        [InlineData(false, 0)]
        [InlineData(false, 1)]
        public void AutoFlushTest(bool autoFlush, int autoFlushTimeout)
        {
            var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var logFile = Path.Combine(tempPath, "file.txt");
            try
            {
                var fileTarget = new FileTarget
                {
                    FileName = logFile,
                    LineEnding = LineEndingMode.LF,
                    Layout = "${level} ${message}",
                    KeepFileOpen = true,
                    ConcurrentWrites = false,
                    AutoFlush = autoFlush,
                    OpenFileFlushTimeout = autoFlushTimeout,
                };

                SimpleConfigurator.ConfigureForTargetLogging(fileTarget, LogLevel.Debug);

                logger.Debug("aaa");
                logger.Info("bbb");
                logger.Warn("ccc");

                if (autoFlush)
                {
                    AssertFileContents(logFile, "Debug aaa\nInfo bbb\nWarn ccc\n", Encoding.UTF8);
                }
                else
                {
                    AssertFileContents(logFile, string.Empty, Encoding.UTF8);
                    if (autoFlushTimeout > 0)
                    {
                        Thread.Sleep(TimeSpan.FromSeconds(autoFlushTimeout * 1.5));
                        AssertFileContents(logFile, "Debug aaa\nInfo bbb\nWarn ccc\n", Encoding.UTF8);
                    }
                }

                LogManager.Configuration = null;    // Flush
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
                var fileTarget = new FileTarget
                {
                    FileName = logFile,
                    ArchiveFileName = Path.Combine(archiveFolder, "{####}.txt"),
                    ArchiveAboveSize = 100,
                    LineEnding = LineEndingMode.LF,
                    Layout = "${message}",
                    MaxArchiveFiles = 3,
                    ArchiveNumbering = ArchiveNumberingMode.Sequence
                };

                SimpleConfigurator.ConfigureForTargetLogging(fileTarget, LogLevel.Debug);

                // we emit 5 * 25 *(3 x aaa + \n) bytes
                // so that we should get a full file + 3 archives
                Generate100BytesLog('a');
                Generate100BytesLog('b');
                Generate100BytesLog('c');
                Generate100BytesLog('d');
                Generate100BytesLog('e');

                LogManager.Configuration = null;    // Flush

                var times = 25;
                AssertFileContents(logFile,
                    StringRepeat(times, "eee\n"),
                    Encoding.UTF8);

                AssertFileContents(
                    Path.Combine(archiveFolder, "0001.txt"),
                    StringRepeat(times, "bbb\n"),
                    Encoding.UTF8);

                AssertFileContents(
                    Path.Combine(archiveFolder, "0002.txt"),
                    StringRepeat(times, "ccc\n"),
                    Encoding.UTF8);

                AssertFileContents(
                    Path.Combine(archiveFolder, "0003.txt"),
                    StringRepeat(times, "ddd\n"),
                    Encoding.UTF8);
                //0000 should not exists because of MaxArchiveFiles=3
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
                var fileTarget = new FileTarget
                {
                    FileName = logFile,
                    ArchiveFileName = Path.Combine(archiveFolder, "{####}.txt"),
                    ArchiveAboveSize = 100,
                    LineEnding = LineEndingMode.LF,
                    ArchiveNumbering = ArchiveNumberingMode.Sequence,
                    Layout = "${message}",
                    MaxArchiveFiles = 0
                };

                SimpleConfigurator.ConfigureForTargetLogging(fileTarget, LogLevel.Debug);

                // we emit 5 * 25 *(3 x aaa + \n) bytes
                // so that we should get a full file + 4 archives
                Generate100BytesLog('a');
                Generate100BytesLog('b');
                Generate100BytesLog('c');
                Generate100BytesLog('d');
                Generate100BytesLog('e');

                LogManager.Configuration = null;    // Flush

                AssertFileContents(logFile,
                    StringRepeat(25, "eee\n"),
                    Encoding.UTF8);

                AssertFileContents(
                   Path.Combine(archiveFolder, "0000.txt"),
                   StringRepeat(25, "aaa\n"),
                   Encoding.UTF8);

                AssertFileContents(
                    Path.Combine(archiveFolder, "0001.txt"),
                    StringRepeat(25, "bbb\n"),
                    Encoding.UTF8);

                AssertFileContents(
                    Path.Combine(archiveFolder, "0002.txt"),
                    StringRepeat(25, "ccc\n"),
                    Encoding.UTF8);

                AssertFileContents(
                    Path.Combine(archiveFolder, "0003.txt"),
                    StringRepeat(25, "ddd\n"),
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

        [Fact]
        public void ArchiveAboveSizeWithArchiveNumberingModeDate_maxfiles_o()
        {
            var tempPath = Path.Combine(Path.GetTempPath(), "ArchiveEveryCombinedWithArchiveAboveSize_" + Guid.NewGuid().ToString());
            var logFile = Path.Combine(tempPath, "file.txt");
            try
            {
                string archiveFolder = Path.Combine(tempPath, "archive");
                var fileTarget = new FileTarget
                {
                    FileName = logFile,
                    ArchiveFileName = Path.Combine(archiveFolder, "{####}.txt"),
                    ArchiveAboveSize = 100,
                    LineEnding = LineEndingMode.LF,
                    Layout = "${message}",
                    ArchiveNumbering = ArchiveNumberingMode.Date
                };

                SimpleConfigurator.ConfigureForTargetLogging(fileTarget, LogLevel.Debug);

                //e.g. 20150804
                var archiveFileName = DateTime.Now.ToString("yyyyMMdd");

                // we emit 5 * 25 *(3 x aaa + \n) bytes
                // so that we should get a full file + 3 archives
                var times = 25;
                for (var i = 0; i < times; ++i)
                {
                    logger.Debug("aaa");
                }

                for (var i = 0; i < times; ++i)
                {
                    logger.Debug("bbb");
                }
                for (var i = 0; i < times; ++i)
                {
                    logger.Debug("ccc");
                }
                for (var i = 0; i < times; ++i)
                {
                    logger.Debug("ddd");
                }
                for (var i = 0; i < times; ++i)
                {
                    logger.Debug("eee");
                }

                LogManager.Configuration = null;    // Flush

                //we expect only eee and all other in the archive
                AssertFileContents(logFile,
                    StringRepeat(times, "eee\n"),
                    Encoding.UTF8);

                //DUNNO what to expected!
                //try (which fails)
                AssertFileContents(
                    Path.Combine(archiveFolder, $"{archiveFileName}.txt"),
                   StringRepeat(times, "aaa\n") + StringRepeat(times, "bbb\n") + StringRepeat(times, "ccc\n") + StringRepeat(times, "ddd\n"),
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
                var fileTarget = new FileTarget
                {
                    FileName = logFile,
                    ArchiveFileName = Path.Combine(archiveFolder, "{#}.txt"),
                    ArchiveAboveSize = 50,
                    LineEnding = LineEndingMode.LF,
                    ArchiveNumbering = ArchiveNumberingMode.Date,
                    ArchiveDateFormat = "yyyyMMddHHmmssfff", //make sure the milliseconds are set in the filename
                    Layout = "${message}",
                    MaxArchiveFiles = maxArchiveFiles
                };

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
                LogManager.Configuration = null;    // Flush

                var files = Directory.GetFiles(archiveFolder).OrderBy(s => s);
                //the amount of archived files may not exceed the set 'MaxArchiveFiles'
                Assert.Equal(maxArchiveFiles, files.Count());


                SimpleConfigurator.ConfigureForTargetLogging(fileTarget, LogLevel.Debug);
                //writing just one line of 11 bytes will trigger the cleanup of old archived files
                //as stated by the MaxArchiveFiles property, but will only delete the oldest file
                logger.Debug("1234567890");

                LogManager.Configuration = null;    // Flush

                var files2 = Directory.GetFiles(archiveFolder).OrderBy(s => s);
                Assert.Equal(maxArchiveFiles, files2.Count());

                //the oldest file should be deleted
                Assert.DoesNotContain(files.ElementAt(0), files2);
                //two files should still be there
                Assert.Equal(files.ElementAt(1), files2.ElementAt(0));
                Assert.Equal(files.ElementAt(2), files2.ElementAt(1));
                //one new archive file should be created
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
            LogManager.ThrowExceptions = true;
            var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            try
            {
                var logFile = Path.Combine(tempPath, "${date:format=yyyyMMddHHmmssfff}.txt");
                var fileTarget = new FileTarget
                {
                    FileName = logFile,
                    ArchiveFileName = Path.Combine(tempPath, "{#}.txt"),
                    ArchiveEvery = FileArchivePeriod.Year,
                    LineEnding = LineEndingMode.LF,
                    ArchiveNumbering = ArchiveNumberingMode.Date,
                    ArchiveDateFormat = "yyyyMMddHHmmssfff", //make sure the milliseconds are set in the filename
                    Layout = "${message}",
                    MaxArchiveFiles = maxArchiveFiles
                };

                SimpleConfigurator.ConfigureForTargetLogging(fileTarget, LogLevel.Debug);
                for (var i = 0; i < 4; ++i)
                {
                    logger.Debug("123456789");
                    //build in a  sleep to make sure the current time is reflected in the filename
                    Thread.Sleep(50);
                }
                //Setting the Configuration to [null] will result in a 'Dump' of the current log entries
                LogManager.Configuration = null;    // Flush

                var files = Directory.GetFiles(tempPath).OrderBy(s => s);
                //we expect 3 archive files, plus one current file
                Assert.Equal(maxArchiveFiles + 1, files.Count());


                SimpleConfigurator.ConfigureForTargetLogging(fileTarget, LogLevel.Debug);
                //writing 50ms later will trigger the cleanup of old archived files
                //as stated by the MaxArchiveFiles property, but will only delete the oldest file
                Thread.Sleep(50);
                logger.Debug("123456789");
                LogManager.Configuration = null;    // Flush

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
                if (Directory.Exists(tempPath))
                    Directory.Delete(tempPath, true);
            }
        }

        public static IEnumerable<object[]> DateArchive_UsesDateFromCurrentTimeSource_TestParameters
        {
            get
            {
                var maxArchiveDays = false;
                var booleanValues = new[] { true, false };
                var timeKindValues = new[] { DateTimeKind.Utc, DateTimeKind.Local };
                return
                    from timeKind in timeKindValues
                    from includeDateInLogFilePath in booleanValues
                    from concurrentWrites in booleanValues
                    from keepFileOpen in booleanValues
                    from networkWrites in booleanValues
                    from forceMutexConcurrentWrites in booleanValues
                    where UniqueBaseAppender(concurrentWrites, keepFileOpen, networkWrites, forceMutexConcurrentWrites)
                    from includeSequenceInArchive in booleanValues
                    from forceManaged in booleanValues
                    select new object[] { timeKind, includeDateInLogFilePath, concurrentWrites, keepFileOpen, networkWrites, includeSequenceInArchive, forceManaged, forceMutexConcurrentWrites, maxArchiveDays };
            }
        }

        [Theory]
        [MemberData(nameof(DateArchive_UsesDateFromCurrentTimeSource_TestParameters))]
        public void DateArchive_UsesDateFromCurrentTimeSource(DateTimeKind timeKind, bool includeDateInLogFilePath, bool concurrentWrites, bool keepFileOpen, bool networkWrites, bool includeSequenceInArchive, bool forceManaged, bool forceMutexConcurrentWrites, bool maxArhiveDays)
        {
#if NETSTANDARD || MONO
            if (IsLinux())
            {
                Console.WriteLine("[SKIP] FileTargetTests.DateArchive_UsesDateFromCurrentTimeSource because SetLastWriteTime is not working on Travis");
                return;
            }
#endif

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
                var fileTarget = new FileTarget
                {
                    FileName = logFile,
                    ArchiveFileName = archiveFileNameTemplate,
                    LineEnding = LineEndingMode.LF,
                    ArchiveNumbering = includeSequenceInArchive ? ArchiveNumberingMode.DateAndSequence : ArchiveNumberingMode.Date,
                    ArchiveEvery = FileArchivePeriod.Day,
                    ArchiveDateFormat = archiveDateFormat,
                    Layout = "${date:format=O}|${message}",
                    MaxArchiveFiles = maxArhiveDays ? 0 : maxArchiveFiles,
                    MaxArchiveDays = maxArhiveDays ? maxArchiveFiles : 0,
                    ConcurrentWrites = concurrentWrites,
                    KeepFileOpen = keepFileOpen,
                    NetworkWrites = networkWrites,
                    ForceManaged = forceManaged,
                    ForceMutexConcurrentWrites = forceMutexConcurrentWrites,
                    Header = "header",
                };

                SimpleConfigurator.ConfigureForTargetLogging(fileTarget, LogLevel.Debug);

                logger.Debug("123456789");
                DateTime previousWriteTime = timeSource.Time;

                const int daysToTestLogging = 3;
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
                    LogManager.Flush();

                    var dayIsChanged = eventInfo.TimeStamp.Date != previousWriteTime.Date;
                    // ensure new archive is created only when the day part of time is changed
                    var archiveFileName = archiveFileNameTemplate.Replace("{#}", previousWriteTime.ToString(archiveDateFormat) + (includeSequenceInArchive ? ".0" : string.Empty));
                    var archiveExists = File.Exists(archiveFileName);
                    if (dayIsChanged)
                        Assert.True(archiveExists,
                            $"new archive should be created when the day part of {timeKind} time is changed");
                    else
                        Assert.False(archiveExists,
                            $"new archive should not be create when day part of {timeKind} time is unchanged");

                    previousWriteTime = eventInfo.TimeStamp.Date;
                    if (dayIsChanged)
                        timeSource.AddToSystemTime(TimeSpan.FromDays(1));
                }
                //Setting the Configuration to [null] will result in a 'Dump' of the current log entries
                LogManager.Configuration = null;    // Flush

                var files = Directory.GetFiles(archiveFolder);
                //the amount of archived files may not exceed the set 'MaxArchiveFiles'
                Assert.Equal(maxArchiveFiles, files.Length);

                foreach (var file in files)
                    AssertFileContentsStartsWith(file, "header", Encoding.UTF8);

                SimpleConfigurator.ConfigureForTargetLogging(fileTarget, LogLevel.Debug);
                //writing one line on a new day will trigger the cleanup of old archived files
                //as stated by the MaxArchiveFiles property, but will only delete the oldest file
                timeSource.AddToLocalTime(TimeSpan.FromDays(1));
                logger.Debug("1234567890");

                LogManager.Configuration = null;    // Flush

                var files2 = Directory.GetFiles(archiveFolder);
                Assert.Equal(maxArchiveFiles, files2.Length);

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

        [Theory]
        [InlineData(DateTimeKind.Utc, false, false)]
        [InlineData(DateTimeKind.Local, false, false)]
        [InlineData(DateTimeKind.Utc, true, false)]
        [InlineData(DateTimeKind.Local, true, false)]
        [InlineData(DateTimeKind.Utc, false, true)]
        [InlineData(DateTimeKind.Local, false, true)]
        [InlineData(DateTimeKind.Utc, true, true)]
        [InlineData(DateTimeKind.Local, true, true)]
        public void DateArchive_UsesDateFromCurrentTimeSource_MaxArchiveDays(DateTimeKind timeKind, bool includeDateInLogFilePath, bool includeSequenceInArchive)
        {
            const bool MaxArchiveDays = true;
            DateArchive_UsesDateFromCurrentTimeSource(timeKind, includeDateInLogFilePath, false, false, false, includeSequenceInArchive, false, false, MaxArchiveDays);
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
                    from forceMutexConcurrentWrites in booleanValues
                    where UniqueBaseAppender(concurrentWrites, keepFileOpen, networkWrites, forceMutexConcurrentWrites)
                    from includeDateInLogFilePath in booleanValues
                    from includeSequenceInArchive in booleanValues
                    from forceManaged in booleanValues
                    select new object[] { concurrentWrites, keepFileOpen, networkWrites, includeDateInLogFilePath, includeSequenceInArchive, forceManaged, forceMutexConcurrentWrites };
            }
        }

        [Theory]
        [MemberData(nameof(DateArchive_ArchiveOnceOnly_TestParameters))]
        public void DateArchive_ArchiveOnceOnly(bool concurrentWrites, bool keepFileOpen, bool networkWrites, bool dateInLogFilePath, bool includeSequenceInArchive, bool forceManaged, bool forceMutexConcurrentWrites)
        {
            var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var logFile = Path.Combine(tempPath, dateInLogFilePath ? "file_${shortdate}.txt" : "file.txt");

            var defaultTimeSource = TimeSource.Current;

            try
            {
                var timeSource = new TimeSourceTests.ShiftedTimeSource(DateTimeKind.Local);
                if (timeSource.Time.Minute == 59)
                {
                    // Avoid double-archive due to overflow of the hour.
                    timeSource.AddToLocalTime(TimeSpan.FromMinutes(1));
                    timeSource.AddToSystemTime(TimeSpan.FromMinutes(1));
                }
                TimeSource.Current = timeSource;

                string archiveFolder = Path.Combine(tempPath, "archive");
                var fileTarget = new FileTarget
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
                    NetworkWrites = networkWrites,
                    ForceManaged = forceManaged,
                    ForceMutexConcurrentWrites = forceMutexConcurrentWrites,
                };

                SimpleConfigurator.ConfigureForTargetLogging(fileTarget, LogLevel.Debug);

                logger.Debug("123456789");
                LogManager.Flush();

                timeSource.AddToLocalTime(TimeSpan.FromDays(1));

                // This should archive the log before logging.
                logger.Debug("123456789");

                timeSource.AddToSystemTime(TimeSpan.FromDays(1));   // Archive only once

                // This must not archive.
                logger.Debug("123456789");

                LogManager.Configuration = null;    // Flush

                Assert.Single(Directory.GetFiles(archiveFolder));
                var prevLogFile = Directory.GetFiles(archiveFolder)[0];
                AssertFileContents(prevLogFile, StringRepeat(1, "123456789\n"), Encoding.UTF8);

                var currentLogFile = Directory.GetFiles(tempPath)[0];
                AssertFileContents(currentLogFile, StringRepeat(2, "123456789\n"), Encoding.UTF8);
            }
            finally
            {
                TimeSource.Current = defaultTimeSource; // restore default time source

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
        [MemberData(nameof(DateArchive_SkipPeriod_TestParameters))]
        public void DateArchive_SkipPeriod(DateTimeKind timeKind, FileArchivePeriod archivePeriod, bool includeDateInLogFilePath, bool includeSequenceInArchive)
        {
            var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var logFile = Path.Combine(tempPath, includeDateInLogFilePath ? "file_${date:format=yyyyMMddHHmm}.txt" : "file.txt");
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

                var fileTarget = new FileTarget
                {
                    FileName = logFile,
                    ArchiveFileName = Path.Combine(tempPath, "archive", "{#}.txt"),
                    LineEnding = LineEndingMode.LF,
                    ArchiveNumbering = includeSequenceInArchive ? ArchiveNumberingMode.DateAndSequence : ArchiveNumberingMode.Date,
                    ArchiveEvery = archivePeriod,
                    ArchiveDateFormat = "yyyyMMddHHmm",
                    Layout = "${date:format=O}|${message}",
                };
                string archiveDateFormat = fileTarget.ArchiveDateFormat;
                SimpleConfigurator.ConfigureForTargetLogging(fileTarget, LogLevel.Debug);

                logger.Debug("1234567890");
                timeSource.AddToLocalTime(TimeSpan.FromMinutes(1));
                logger.Debug("1234567890");
                // The archive file name must be based on the last time the file was written.
                string archiveFileName =
                    $"{timeSource.Time.ToString(archiveDateFormat) + (includeSequenceInArchive ? ".0" : string.Empty)}.txt";
                // Effectively update the file's last-write-time.
                timeSource.AddToSystemTime(TimeSpan.FromMinutes(1));

                timeSource.AddToLocalTime(TimeSpan.FromDays(2));
                logger.Debug("1234567890");

                LogManager.Configuration = null;    // Flush

                string archivePath = Path.Combine(tempPath, "archive");
                var archiveFiles = Directory.GetFiles(archivePath);
                Assert.Single(archiveFiles);
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
                    from forceMutexConcurrentWrites in booleanValues
                    where UniqueBaseAppender(concurrentWrites, keepFileOpen, networkWrites, forceMutexConcurrentWrites)
                    from includeDateInLogFilePath in booleanValues
                    from includeSequenceInArchive in booleanValues
                    from enableArchiveCompression in booleanValues
                    from forceManaged in booleanValues
                    select new object[] { concurrentWrites, keepFileOpen, networkWrites, includeDateInLogFilePath, includeSequenceInArchive, enableArchiveCompression, forceManaged, forceMutexConcurrentWrites };
            }
        }

        [Theory]
        [MemberData(nameof(DateArchive_AllLoggersTransferToCurrentLogFile_TestParameters))]
        public void DateArchive_AllLoggersTransferToCurrentLogFile(bool concurrentWrites, bool keepFileOpen, bool networkWrites, bool includeDateInLogFilePath, bool includeSequenceInArchive, bool enableArchiveCompression, bool forceManaged, bool forceMutexConcurrentWrites)
        {
            if (keepFileOpen && !networkWrites && !concurrentWrites)
                return; // This combination do not support two local FileTargets to the same file

#if NET35 || NET40
            if (enableArchiveCompression)
                return; // No need to test with compression
#endif

            var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var logfile = Path.Combine(tempPath, includeDateInLogFilePath ? "file_${shortdate}.txt" : "file.txt");
            var defaultTimeSource = TimeSource.Current;

#if NET35 || NET40
            IFileCompressor fileCompressor = null;
#endif

            try
            {
                var timeSource = new TimeSourceTests.ShiftedTimeSource(DateTimeKind.Local);
                if (timeSource.Time.Minute == 59)
                {
                    // Avoid double-archive due to overflow of the hour.
                    timeSource.AddToLocalTime(TimeSpan.FromMinutes(1));
                    timeSource.AddToSystemTime(TimeSpan.FromMinutes(1));
                }
                TimeSource.Current = timeSource;

                var config = new LoggingConfiguration();

#if NET35 || NET40
                if (enableArchiveCompression)
                {
                    fileCompressor = FileTarget.FileCompressor;
                    FileTarget.FileCompressor = new CustomFileCompressor();
                }
#endif

                string archiveFolder = Path.Combine(tempPath, "archive");
                var fileTarget1 = new FileTarget
                {
                    FileName = logfile,
                    ArchiveFileName = Path.Combine(archiveFolder, "{#}.txt"),
                    LineEnding = LineEndingMode.LF,
                    ArchiveNumbering = includeSequenceInArchive ? ArchiveNumberingMode.DateAndSequence : ArchiveNumberingMode.Date,
                    ArchiveEvery = FileArchivePeriod.Day,
                    ArchiveDateFormat = "yyyyMMdd",
                    EnableArchiveFileCompression = enableArchiveCompression,
                    Layout = "${message}",
                    ConcurrentWrites = concurrentWrites,
                    KeepFileOpen = keepFileOpen,
                    NetworkWrites = networkWrites,
                    ForceManaged = forceManaged,
                    ForceMutexConcurrentWrites = forceMutexConcurrentWrites,
                };
                var logger1Rule = new LoggingRule("logger1", LogLevel.Debug, fileTarget1);
                config.LoggingRules.Add(logger1Rule);

                var fileTarget2 = new FileTarget
                {
                    FileName = logfile,
                    ArchiveFileName = Path.Combine(archiveFolder, "{#}.txt"),
                    LineEnding = LineEndingMode.LF,
                    ArchiveNumbering = includeSequenceInArchive ? ArchiveNumberingMode.DateAndSequence : ArchiveNumberingMode.Date,
                    ArchiveEvery = FileArchivePeriod.Day,
                    ArchiveDateFormat = "yyyyMMdd",
                    EnableArchiveFileCompression = enableArchiveCompression,
                    Layout = "${message}",
                    ConcurrentWrites = concurrentWrites,
                    KeepFileOpen = keepFileOpen,
                    NetworkWrites = networkWrites,
                    ForceManaged = forceManaged,
                    ForceMutexConcurrentWrites = forceMutexConcurrentWrites,
                };
                var logger2Rule = new LoggingRule("logger2", LogLevel.Debug, fileTarget2);
                config.LoggingRules.Add(logger2Rule);

                LogManager.Configuration = config;

                var logger1 = LogManager.GetLogger("logger1");
                var logger2 = LogManager.GetLogger("logger2");

                logger1.Debug("123456789");
                logger2.Debug("123456789");
                LogManager.Flush();

                timeSource.AddToLocalTime(TimeSpan.FromDays(1));

                // This should archive the log before logging.
                logger1.Debug("123456789");

                timeSource.AddToSystemTime(TimeSpan.FromDays(1));   // Archive only once

                Thread.Sleep(10);
                logger2.Debug("123456789");

                LogManager.Configuration = null;    // Flush

                var files = Directory.GetFiles(archiveFolder);
                Assert.Single(files);
                if (!enableArchiveCompression)
                {
                    string prevLogFile = Directory.GetFiles(archiveFolder)[0];
                    AssertFileContents(prevLogFile, StringRepeat(2, "123456789\n"), Encoding.UTF8);
                }
                string currentLogFile = Directory.GetFiles(tempPath)[0];
                AssertFileContents(currentLogFile, StringRepeat(2, "123456789\n"), Encoding.UTF8);
            }
            finally
            {
                TimeSource.Current = defaultTimeSource; // restore default time source

#if NET35 || NET40
                if (enableArchiveCompression)
                {
                    FileTarget.FileCompressor = fileCompressor;
                }
#endif

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
                var fileTarget = new FileTarget
                {
                    FileName = logFile,
                    ArchiveFileName = Path.Combine(archiveFolder, "{#}.txt"),
                    ArchiveAboveSize = 50,
                    LineEnding = LineEndingMode.LF,
                    ArchiveNumbering = ArchiveNumberingMode.Date,
                    ArchiveDateFormat = "yyyyMMddHHmmssfff", //make sure the milliseconds are set in the filename
                    Layout = "${message}",
                    MaxArchiveFiles = 0
                };

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
                LogManager.Configuration = null;    // Flush

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
                var fileTarget = new FileTarget
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
                Assert.Equal(fileTarget.MaxArchiveFiles, files.Count());

                //alter the MaxArchivedFiles
                fileTarget.MaxArchiveFiles = 2;
                SimpleConfigurator.ConfigureForTargetLogging(fileTarget, LogLevel.Debug);
                //writing just one line of 11 bytes will trigger the cleanup of old archived files
                //as stated by the MaxArchiveFiles property, but will only delete the oldest files
                logger.Debug("1234567890");
                LogManager.Configuration = null;    // Flush

                var files2 = Directory.GetFiles(archiveFolder).OrderBy(s => s);
                Assert.Equal(fileTarget.MaxArchiveFiles, files2.Count());

                //the oldest files should be deleted
                Assert.DoesNotContain(files.ElementAt(0), files2);
                Assert.DoesNotContain(files.ElementAt(1), files2);
                Assert.DoesNotContain(files.ElementAt(2), files2);
                Assert.DoesNotContain(files.ElementAt(3), files2);
                //one files should still be there
                Assert.Equal(files.ElementAt(4), files2.ElementAt(0));
                //one new archive file should be created
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
                var fileTarget = new FileTarget
                {
                    FileName = logFile,
                    ArchiveFileName = Path.Combine(archiveFolder, "{####}.txt"),
                    ArchiveAboveSize = 51,
                    LineEnding = LineEndingMode.LF,
                    ArchiveNumbering = ArchiveNumberingMode.Sequence,
                    Layout = "${message}",
                    Header = header,
                    MaxArchiveFiles = 2,
                };

                SimpleConfigurator.ConfigureForTargetLogging(fileTarget, LogLevel.Debug);

                // Writing 16 times 10 bytes = 160 bytes = 3 files
                for (var i = 0; i < 16; ++i)
                {
                    logger.Debug("123456789");
                }

                LogManager.Configuration = null;    // Flush

                AssertFileContentsStartsWith(logFile, header, Encoding.UTF8);

                AssertFileContentsStartsWith(Path.Combine(archiveFolder, "0002.txt"), header, Encoding.UTF8);

                AssertFileContentsStartsWith(Path.Combine(archiveFolder, "0001.txt"), header, Encoding.UTF8);

                Assert.True(!File.Exists(Path.Combine(archiveFolder, "0000.txt"))); // MaxArchiveFiles = 2 (Removes the first file)
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
        [InlineData(false)]
        [InlineData(true)]
        public void RepeatingFooterTest(bool writeFooterOnArchivingOnly)
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
                    WriteFooterOnArchivingOnly = writeFooterOnArchivingOnly
                };

                SimpleConfigurator.ConfigureForTargetLogging(ft, LogLevel.Debug);

                // Writing 16 times 10 bytes = 160 bytes = 3 files
                for (var i = 0; i < 16; ++i)
                {
                    logger.Debug("123456789");
                }

                LogManager.Configuration = null;    // Flush

                string expectedEnding = footer + ft.LineEnding.NewLineCharacters;
                if (writeFooterOnArchivingOnly)
                    Assert.False(File.ReadAllText(logFile).EndsWith(expectedEnding), "Footer was unexpectedly written to log file.");
                else
                    AssertFileContentsEndsWith(logFile, expectedEnding, Encoding.UTF8);
                AssertFileContentsEndsWith(Path.Combine(archiveFolder, "0002.txt"), expectedEnding, Encoding.UTF8);
                AssertFileContentsEndsWith(Path.Combine(archiveFolder, "0001.txt"), expectedEnding, Encoding.UTF8);
                Assert.False(File.Exists(Path.Combine(archiveFolder, "0000.txt"))); // MaxArchiveFiles = 2 (Removes the first file)
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

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void RollingArchiveCompressionTest(bool specifyArchiveFileName)
        {
            RollingArchiveTests(enableCompression: true, specifyArchiveFileName: specifyArchiveFileName);
        }

        private void RollingArchiveTests(bool enableCompression, bool specifyArchiveFileName)
        {
            var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var logFile = Path.Combine(tempPath, "file.txt");
            var archiveExtension = enableCompression ? "zip" : "txt";

#if NET35 || NET40
            IFileCompressor fileCompressor = null;
#endif

            try
            {
                var fileTarget = new FileTarget
                {
                    EnableArchiveFileCompression = enableCompression,
                    FileName = logFile,
                    ArchiveAboveSize = 100,
                    LineEnding = LineEndingMode.LF,
                    ArchiveNumbering = ArchiveNumberingMode.Rolling,
                    Layout = "${message}",
                    MaxArchiveFiles = 3
                };

#if NET35 || NET40
                if (enableCompression)
                {
                    fileCompressor = FileTarget.FileCompressor;
                    FileTarget.FileCompressor = new CustomFileCompressor();
                }
#endif

                if (specifyArchiveFileName)
                    fileTarget.ArchiveFileName = Path.Combine(tempPath, "archive", "{####}." + archiveExtension);

                SimpleConfigurator.ConfigureForTargetLogging(fileTarget, LogLevel.Debug);

                // we emit 5 * 25 * (3 x aaa + \n) bytes
                // so that we should get a full file + 3 archives
                Generate100BytesLog('a');
                Generate100BytesLog('b');
                Generate100BytesLog('c');
                Generate100BytesLog('d');
                Generate100BytesLog('e');

                LogManager.Configuration = null;    // Flush

                var assertFileContents =
                    enableCompression ? new Action<string, string, string, Encoding>(AssertZipFileContents) : AssertFileContents;

                var times = 25;
                AssertFileContents(logFile,
                    StringRepeat(times, "eee\n"),
                    Encoding.UTF8);

                string archiveFileNameFormat = specifyArchiveFileName
                    ? Path.Combine("archive", "000{0}." + archiveExtension)
                    : "file.{0}." + archiveExtension;

                assertFileContents(
                    Path.Combine(tempPath, string.Format(archiveFileNameFormat, 0)),
                    "file.txt",
                    StringRepeat(times, "ddd\n"),
                    Encoding.UTF8);

                assertFileContents(
                    Path.Combine(tempPath, string.Format(archiveFileNameFormat, 1)),
                    "file.txt",
                    StringRepeat(times, "ccc\n"),
                    Encoding.UTF8);

                assertFileContents(
                    Path.Combine(tempPath, string.Format(archiveFileNameFormat, 2)),
                    "file.txt",
                    StringRepeat(times, "bbb\n"),
                    Encoding.UTF8);

                Assert.True(!File.Exists(Path.Combine(tempPath, string.Format(archiveFileNameFormat, 3))));
            }
            finally
            {
#if NET35 || NET40
                if (enableCompression)
                {
                    FileTarget.FileCompressor = fileCompressor;
                }
#endif
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
                var fileTarget = new FileTarget
                {
                    FileName = logFile,
                    ArchiveFileName = Path.Combine(tempPath, "archive" + slash + "{####}.txt"),
                    ArchiveAboveSize = 100,
                    LineEnding = LineEndingMode.LF,
                    ArchiveNumbering = ArchiveNumberingMode.Rolling,
                    Layout = "${message}",
                    MaxArchiveFiles = 0
                };

                SimpleConfigurator.ConfigureForTargetLogging(fileTarget, LogLevel.Debug);

                // we emit 5 * 25 * (3 x aaa + \n) bytes
                // so that we should get a full file + 4 archives
                Generate100BytesLog('a');
                Generate100BytesLog('b');
                Generate100BytesLog('c');
                Generate100BytesLog('d');
                Generate100BytesLog('e');

                LogManager.Configuration = null;    // Flush

                var times = 25;
                AssertFileContents(logFile,
                    StringRepeat(times, "eee\n"),
                    Encoding.UTF8);

                AssertFileContents(
                    Path.Combine(tempPath, "archive" + slash + "0000.txt"),
                    StringRepeat(times, "ddd\n"),
                    Encoding.UTF8);

                AssertFileContents(
                    Path.Combine(tempPath, "archive" + slash + "0001.txt"),
                    StringRepeat(times, "ccc\n"),
                    Encoding.UTF8);

                AssertFileContents(
                    Path.Combine(tempPath, "archive" + slash + "0002.txt"),
                    StringRepeat(times, "bbb\n"),
                    Encoding.UTF8);

                AssertFileContents(
                    Path.Combine(tempPath, "archive" + slash + "0003.txt"),
                    StringRepeat(times, "aaa\n"),
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
                var fileTarget = new FileTarget
                {
                    FileName = Path.Combine(tempPath, "${level}.txt"),
                    LineEnding = LineEndingMode.LF,
                    Layout = "${message}"
                };

                SimpleConfigurator.ConfigureForTargetLogging(fileTarget, LogLevel.Debug);

                var times = 25;
                for (var i = 0; i < times; ++i)
                {
                    logger.Trace("@@@");
                    logger.Debug("aaa");
                    logger.Info("bbb");
                    logger.Warn("ccc");
                    logger.Error("ddd");
                    logger.Fatal("eee");
                }

                LogManager.Configuration = null;    // Flush

                Assert.False(File.Exists(Path.Combine(tempPath, "Trace.txt")));

                AssertFileContents(Path.Combine(tempPath, "Debug.txt"),
                    StringRepeat(times, "aaa\n"), Encoding.UTF8);

                AssertFileContents(Path.Combine(tempPath, "Info.txt"),
                    StringRepeat(times, "bbb\n"), Encoding.UTF8);

                AssertFileContents(Path.Combine(tempPath, "Warn.txt"),
                    StringRepeat(times, "ccc\n"), Encoding.UTF8);

                AssertFileContents(Path.Combine(tempPath, "Error.txt"),
                    StringRepeat(times, "ddd\n"), Encoding.UTF8);

                AssertFileContents(Path.Combine(tempPath, "Fatal.txt"),
                    StringRepeat(times, "eee\n"), Encoding.UTF8);
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
                var fileTarget = new FileTarget
                {
                    FileName = Path.Combine(tempPath, "${level}.txt"),
                    LineEnding = LineEndingMode.LF,
                    Layout = "${message}"
                };

                SimpleConfigurator.ConfigureForTargetLogging(new BufferingTargetWrapper(fileTarget, 10), LogLevel.Debug);

                var times = 25;
                for (var i = 0; i < times; ++i)
                {
                    logger.Trace("@@@");
                    logger.Debug("aaa");
                    logger.Info("bbb");
                    logger.Warn("ccc");
                    logger.Error("ddd");
                    logger.Fatal("eee");
                }

                LogManager.Configuration = null; // Flush

                Assert.False(File.Exists(Path.Combine(tempPath, "Trace.txt")));

                AssertFileContents(Path.Combine(tempPath, "Debug.txt"),
                    StringRepeat(times, "aaa\n"), Encoding.UTF8);

                AssertFileContents(Path.Combine(tempPath, "Info.txt"),
                    StringRepeat(times, "bbb\n"), Encoding.UTF8);

                AssertFileContents(Path.Combine(tempPath, "Warn.txt"),
                    StringRepeat(times, "ccc\n"), Encoding.UTF8);

                AssertFileContents(Path.Combine(tempPath, "Error.txt"),
                    StringRepeat(times, "ddd\n"), Encoding.UTF8);

                AssertFileContents(Path.Combine(tempPath, "Fatal.txt"),
                    StringRepeat(times, "eee\n"), Encoding.UTF8);
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
                var fileTarget = new FileTarget
                {
                    FileName = Path.Combine(tempPath, "${level}.txt"),
                    LineEnding = LineEndingMode.LF,
                    Layout = "${message} ${threadid}"
                };

                // this also checks that thread-volatile layouts
                // such as ${threadid} are properly cached and not recalculated
                // in logging threads.
                var threadID = Thread.CurrentThread.ManagedThreadId.ToString();

                SimpleConfigurator.ConfigureForTargetLogging(new AsyncTargetWrapper(fileTarget, 10, AsyncTargetWrapperOverflowAction.Grow)
                {
                    Name = "AsyncMultiFileWrite_wrapper",
                    TimeToSleepBetweenBatches = 1,
                }, LogLevel.Debug);

                var times = 25;
                for (var i = 0; i < times; ++i)
                {
                    logger.Trace("@@@");
                    logger.Debug("aaa");
                    logger.Info("bbb");
                    logger.Warn("ccc");
                    logger.Error("ddd");
                    logger.Fatal("eee");
                }

                LogManager.Configuration = null;    // Flush

                Assert.False(File.Exists(Path.Combine(tempPath, "Trace.txt")));

                AssertFileContents(Path.Combine(tempPath, "Debug.txt"),
                    StringRepeat(times, "aaa " + threadID + "\n"), Encoding.UTF8);

                AssertFileContents(Path.Combine(tempPath, "Info.txt"),
                    StringRepeat(times, "bbb " + threadID + "\n"), Encoding.UTF8);

                AssertFileContents(Path.Combine(tempPath, "Warn.txt"),
                    StringRepeat(times, "ccc " + threadID + "\n"), Encoding.UTF8);

                AssertFileContents(Path.Combine(tempPath, "Error.txt"),
                    StringRepeat(times, "ddd " + threadID + "\n"), Encoding.UTF8);

                AssertFileContents(Path.Combine(tempPath, "Fatal.txt"),
                    StringRepeat(times, "eee " + threadID + "\n"), Encoding.UTF8);
            }
            finally
            {
                if (Directory.Exists(tempPath))
                    Directory.Delete(tempPath, true);
            }
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
            FileTarget_ArchiveNumbering_DateAndSequenceTests(enableCompression: false, fileTxt: "file.txt", archiveFileName: Path.Combine("archive", "{#}.txt"));
        }

        [Fact]
        public void FileTarget_ArchiveNumbering_DateAndSequence_archive_same_as_log_name()
        {
            FileTarget_ArchiveNumbering_DateAndSequenceTests(enableCompression: false, fileTxt: "file-${date:format=yyyy-MM-dd}.txt", archiveFileName: "file-{#}.txt");
        }

        [Fact]
        public void FileTarget_ArchiveNumbering_DateAndSequence_WithCompression()
        {
            FileTarget_ArchiveNumbering_DateAndSequenceTests(enableCompression: true, fileTxt: "file.txt", archiveFileName: Path.Combine("archive", "{#}.zip"));
        }

        private void FileTarget_ArchiveNumbering_DateAndSequenceTests(bool enableCompression, string fileTxt, string archiveFileName)
        {
            const string archiveDateFormat = "yyyy-MM-dd";
            const int archiveAboveSize = 100;

            var tempPath = ArchiveFileNameHelper.GenerateTempPath();
            Layout logFile = Path.Combine(tempPath, fileTxt);
            var logFileName = logFile.Render(LogEventInfo.CreateNullEvent());

#if NET35 || NET40
            IFileCompressor fileCompressor = null;
#endif

            try
            {
                var fileTarget = new FileTarget
                {
                    EnableArchiveFileCompression = enableCompression,
                    FileName = logFile,
                    ArchiveFileName = Path.Combine(tempPath, archiveFileName),
                    ArchiveDateFormat = archiveDateFormat,
                    ArchiveAboveSize = archiveAboveSize,
                    LineEnding = LineEndingMode.LF,
                    Layout = "${message}",
                    MaxArchiveFiles = 3,
                    ArchiveNumbering = ArchiveNumberingMode.DateAndSequence,
                    ArchiveEvery = FileArchivePeriod.Day
                };


#if NET35 || NET40
                if (enableCompression)
                {
                    fileCompressor = FileTarget.FileCompressor;
                    FileTarget.FileCompressor = new CustomFileCompressor();
                }
#endif

                SimpleConfigurator.ConfigureForTargetLogging(fileTarget, LogLevel.Debug);

                // we emit 5 * 25 *(3 x aaa + \n) bytes
                // so that we should get a full file + 3 archives
                Generate100BytesLog('a');
                Generate100BytesLog('b');
                Generate100BytesLog('c');
                Generate100BytesLog('d');
                Generate100BytesLog('e');

                string renderedArchiveFileName = archiveFileName.Replace("{#}", DateTime.Now.ToString(archiveDateFormat));

                LogManager.Configuration = null;

                var assertFileContents = enableCompression ? new Action<string, string, string, Encoding>(AssertZipFileContents) : AssertFileContents;

                var extension = Path.GetExtension(renderedArchiveFileName);
                var fileNameWithoutExt = renderedArchiveFileName.Substring(0, renderedArchiveFileName.Length - extension.Length);
                ArchiveFileNameHelper helper = new ArchiveFileNameHelper(tempPath, fileNameWithoutExt, extension);

                var times = 25;
                AssertFileContents(logFileName,
                    StringRepeat(times, "eee\n"),
                    Encoding.UTF8);

#if !NET35
                string expectedEntry1Name = Path.GetFileNameWithoutExtension(helper.GetFullPath(1)) + ".txt";
                string expectedEntry2Name = Path.GetFileNameWithoutExtension(helper.GetFullPath(2)) + ".txt";
                string expectedEntry3Name = Path.GetFileNameWithoutExtension(helper.GetFullPath(3)) + ".txt";
#else
                string expectedEntry1Name = fileTxt;
                string expectedEntry2Name = fileTxt;
                string expectedEntry3Name = fileTxt;
#endif
                assertFileContents(helper.GetFullPath(1), expectedEntry1Name, StringRepeat(times, "bbb\n"), Encoding.UTF8);
                assertFileContents(helper.GetFullPath(2), expectedEntry2Name, StringRepeat(times, "ccc\n"), Encoding.UTF8);
                assertFileContents(helper.GetFullPath(3), expectedEntry3Name, StringRepeat(times, "ddd\n"), Encoding.UTF8);

                Assert.False(helper.Exists(0), "First archive should have been deleted due to max archive count.");
                Assert.False(helper.Exists(4), "Fifth archive must not have been created yet.");
            }
            finally
            {
#if NET35 || NET40
                if (enableCompression)
                {
                    FileTarget.FileCompressor = fileCompressor;
                }
#endif

                if (File.Exists(logFileName))
                    File.Delete(logFileName);
                if (Directory.Exists(tempPath))
                    Directory.Delete(tempPath, true);
            }
        }

        [Theory]
        [InlineData("archive/test.log.{####}", "archive/test.log.0000", ArchiveNumberingMode.Sequence)]
        [InlineData("archive\\test.log.{####}", "archive\\test.log.0000", ArchiveNumberingMode.Sequence)]
        [InlineData("file-${date:format=yyyyMMdd}.txt", "file-${date:format=yyyyMMdd}.txt", ArchiveNumberingMode.Sequence)]
        [InlineData("file-{#}.txt", "file-${date:format=yyyyMMdd}.txt", ArchiveNumberingMode.Date)]
        public void FileTargetArchiveFileNameTest(string archiveFileName, string expectedArchiveFileName, ArchiveNumberingMode archiveNumbering)
        {
            var subPath = Guid.NewGuid().ToString();
            var tempPath = Path.Combine(Path.GetTempPath(), subPath);
            var logFile = Path.Combine(tempPath, "file-${date:format=yyyyMMdd}.txt");
            try
            {
                var fileTarget = new FileTarget
                {
                    FileName = logFile,
                    ArchiveFileName = Path.Combine(tempPath, "..", subPath, archiveFileName),
                    ArchiveNumbering = archiveNumbering,
                    ArchiveAboveSize = 1000,
                    MaxArchiveFiles = 1000,
                };

                SimpleConfigurator.ConfigureForTargetLogging(fileTarget, LogLevel.Debug);

                for (var i = 0; i < 25; ++i)
                {
                    logger.Debug("a");
                }

                LogManager.Configuration = null;

                logFile = new SimpleLayout(logFile).Render(LogEventInfo.CreateNullEvent());
                expectedArchiveFileName = new SimpleLayout(expectedArchiveFileName).Render(LogEventInfo.CreateNullEvent());

                Assert.True(File.Exists(logFile));
                Assert.True(File.Exists(Path.Combine(tempPath, expectedArchiveFileName)));
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
                var fileTarget = new FileTarget
                {
                    FileName = SimpleLayout.Escape(invalidLogFileName),
                    LineEnding = LineEndingMode.LF,
                    Layout = "${level} ${message}",
                    OpenFileCacheTimeout = 0
                };

                SimpleConfigurator.ConfigureForTargetLogging(fileTarget, LogLevel.Fatal);

                logger.Fatal("aaa");

                LogManager.Configuration = null;    // Flush

                AssertFileContents(expectedCorrectedTempFile, "Fatal aaa\n", Encoding.UTF8);
            }
            finally
            {
                if (File.Exists(expectedCorrectedTempFile))
                    File.Delete(expectedCorrectedTempFile);
                if (File.Exists(invalidLogFileName))
                    File.Delete(invalidLogFileName);
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

                var fileTarget = new FileTarget
                {
                    FileName = logFile,
                    ArchiveFileName = archiveFile,
                    ArchiveAboveSize = 1, //Force immediate archival
                    ArchiveNumbering = ArchiveNumberingMode.DateAndSequence,
                    MaxArchiveFiles = 5
                };

                SimpleConfigurator.ConfigureForTargetLogging(fileTarget, LogLevel.Debug);

                //Creates 5 archive files.
                for (int i = 0; i <= 5; i++)
                {
                    logger.Debug("a");
                }

                LogManager.Configuration = null;    // Flush

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

                var fileTarget = new FileTarget
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
                };

                SimpleConfigurator.ConfigureForTargetLogging(fileTarget, LogLevel.Debug);

                string existingFile = archiveFileLayout.Replace("{#}", "notadate");
                Directory.CreateDirectory(Path.GetDirectoryName(logFile));
                File.Create(existingFile).Close();

                logger.Debug("test");

                LogManager.Configuration = null;    // Flush

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
                var fileTarget = new FileTarget
                {
                    FileName = logFile,
                    ArchiveFileName = Path.Combine(tempPath, "archive", "file.txt2"),
                    ArchiveAboveSize = 100,
                    LineEnding = LineEndingMode.LF,
                    Layout = "${message}",
                    MaxArchiveFiles = 1,
                };

                SimpleConfigurator.ConfigureForTargetLogging(fileTarget, LogLevel.Debug);

                // we emit 2 * 25 *(aaa + \n) bytes
                // so that we should get a full file + 1 archives
                var times = 25;
                for (var i = 0; i < times; ++i)
                {
                    logger.Debug("aaa");
                }
                for (var i = 0; i < times; ++i)
                {
                    logger.Debug("bbb");
                }

                LogManager.Flush();

                AssertFileContents(logFile,
                    StringRepeat(times, "bbb\n"),
                    Encoding.UTF8);
                AssertFileContents(
                    Path.Combine(tempPath, "archive", "file.txt2"),
                    StringRepeat(times, "aaa\n"),
                    Encoding.UTF8);

                for (var i = 0; i < times; ++i)
                {
                    logger.Debug("ccc");
                }

                LogManager.Configuration = null;    // Flush

                AssertFileContents(logFile,
                    StringRepeat(times, "ccc\n"),
                    Encoding.UTF8);
                AssertFileContents(
                    Path.Combine(tempPath, "archive", "file.txt2"),
                    StringRepeat(times, "bbb\n"),
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
                var fileTarget = new FileTarget
                {
                    FileName = logFile,
                    ArchiveFileName = Path.Combine(tempPath, "archive", "file.txt2"),
                    ArchiveAboveSize = 100,
                    LineEnding = LineEndingMode.LF,
                    Layout = "${message}",
                    MaxArchiveFiles = 2,
                };

                SimpleConfigurator.ConfigureForTargetLogging(fileTarget, LogLevel.Debug);

                // we emit 3 * 25 *(aaa + \n) bytes
                // so that we should get a full file + 2 archives
                var times = 25;
                for (var i = 0; i < times; ++i)
                {
                    logger.Debug("aaa");
                }
                for (var i = 0; i < times; ++i)
                {
                    logger.Debug("bbb");
                }
                for (var i = 0; i < times; ++i)
                {
                    logger.Debug("ccc");
                }

                LogManager.Flush();

                AssertFileContents(logFile,
                    StringRepeat(times, "ccc\n"),
                    Encoding.UTF8);
                AssertFileContents(
                    Path.Combine(tempPath, "archive", "file.1.txt2"),
                    StringRepeat(times, "bbb\n"),
                    Encoding.UTF8);
                AssertFileContents(
                    Path.Combine(tempPath, "archive", "file.txt2"),
                    StringRepeat(times, "aaa\n"),
                    Encoding.UTF8);

                for (var i = 0; i < times; ++i)
                {
                    logger.Debug("ddd");
                }

                LogManager.Configuration = null;    // Flush

                AssertFileContents(logFile,
                    StringRepeat(times, "ddd\n"),
                    Encoding.UTF8);
                AssertFileContents(
                    Path.Combine(tempPath, "archive", "file.2.txt2"),
                    StringRepeat(times, "ccc\n"),
                    Encoding.UTF8);
                AssertFileContents(
                    Path.Combine(tempPath, "archive", "file.1.txt2"),
                    StringRepeat(times, "bbb\n"),
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

                var fileTarget = new FileTarget
                {
                    FileName = logFile,
                    ArchiveFileName = Path.Combine(tempPath, "archive", "file.txt2"),
                    ArchiveAboveSize = 100,
                    LineEnding = LineEndingMode.LF,
                    Layout = "${message}",
                    MaxArchiveFiles = 2,
                };

                SimpleConfigurator.ConfigureForTargetLogging(fileTarget, LogLevel.Debug);

                // we emit 2 * 25 *(aaa + \n) bytes
                // so that we should get a full file + 1 archive
                var times = 25;
                for (var i = 0; i < times; ++i)
                {
                    logger.Debug("aaa");
                }
                for (var i = 0; i < times; ++i)
                {
                    logger.Debug("bbb");
                }

                LogManager.Configuration = null;    // Flush

                AssertFileContents(logFile,
                    StringRepeat(times, "bbb\n"),
                    Encoding.UTF8);
                AssertFileContents(
                    Path.Combine(tempPath, "archive", "file.11.txt2"),
                    StringRepeat(times, "aaa\n"),
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
                    ArchiveAboveSize = 100,
                    LineEnding = LineEndingMode.LF,
                    Layout = "${message}",
                    MaxArchiveFiles = maxArchiveFiles,
                    ArchiveNumbering = ArchiveNumberingMode.DateAndSequence,
                };

                SimpleConfigurator.ConfigureForTargetLogging(fileTarget, LogLevel.Debug);

                ArchiveFileNameHelper helper = new ArchiveFileNameHelper(Path.Combine(tempPath, "archive"), DateTime.Now.ToString(fileTarget.ArchiveDateFormat), archiveExtension);

                Generate100BytesLog('a');

                for (int i = 0; i < maxArchiveFiles; i++)
                {
                    Generate100BytesLog('a');
                    Assert.True(helper.Exists(i), $"file {i} is missing");
                }

                for (int i = maxArchiveFiles; i < 21; i++)
                {
                    Generate100BytesLog('b');
                    var numberToBeRemoved = i - maxArchiveFiles; // number 11, we need to remove 1 etc
                    Assert.True(!helper.Exists(numberToBeRemoved),
                        $"archive file {numberToBeRemoved} has not been removed! We are created file {i}");
                }

                LogManager.Configuration = null;
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
        /// Allow multiple archives within the same directory
        /// </summary>
        [Fact]
        public void FileTarget_ArchiveNumbering_remove_correct_wildcard()
        {
            const int maxArchiveFiles = 5;

            var tempPath = ArchiveFileNameHelper.GenerateTempPath();
            var logFile = Path.Combine(tempPath, "{0}{1}.txt");

            var defaultTimeSource = TimeSource.Current;
            try
            {
                var timeSource = new TimeSourceTests.ShiftedTimeSource(DateTimeKind.Local);
                if (timeSource.Time.Minute == 59)
                {
                    // Avoid double-archive due to overflow of the hour.
                    timeSource.AddToLocalTime(TimeSpan.FromMinutes(1));
                    timeSource.AddToSystemTime(TimeSpan.FromMinutes(1));
                }
                TimeSource.Current = timeSource;

                var fileTarget = new FileTarget
                {
                    FileName = string.Format(logFile, "${logger}", "${shortdate}"),
                    ArchiveAboveSize = 100,
                    LineEnding = LineEndingMode.LF,
                    Layout = "${message}",
                    MaxArchiveFiles = maxArchiveFiles,
                };

                SimpleConfigurator.ConfigureForTargetLogging(fileTarget, LogLevel.Debug);
                var logger1 = LogManager.GetLogger("log");
                var logger2 = LogManager.GetLogger("log-other");

                timeSource.AddToLocalTime(TimeSpan.Zero - TimeSpan.FromDays(1));

                Generate100BytesLog((char)('0'), logger1);
                Generate100BytesLog((char)('0'), logger2);
                for (int i = 0; i <= maxArchiveFiles - 3; i++)
                {
                    Generate100BytesLog((char)('1' + i), logger1);
                    Generate100BytesLog((char)('1' + i), logger2);
                    var logFile1 = string.Format(logFile, logger1.Name, TimeSource.Current.Time.Date.ToString("yyyy-MM-dd"));
                    var logFile2 = string.Format(logFile, logger2.Name, TimeSource.Current.Time.Date.ToString("yyyy-MM-dd"));
                    Assert.True(File.Exists(logFile1),
                        $"{logFile1} is missing");
                    Assert.True(File.Exists(logFile2),
                        $"{logFile2} is missing");
                    logFile1 = string.Format(logFile, logger1.Name, TimeSource.Current.Time.Date.ToString("yyyy-MM-dd") + "." + i.ToString());
                    logFile2 = string.Format(logFile, logger2.Name, TimeSource.Current.Time.Date.ToString("yyyy-MM-dd") + "." + i.ToString());
                    Assert.True(File.Exists(logFile1),
                        $"{logFile1} is missing");
                    Assert.True(File.Exists(logFile2),
                        $"{logFile2} is missing");
                }

                TimeSource.Current = defaultTimeSource; // restore default time source
                Generate100BytesLog((char)('a'), logger1);
                Generate100BytesLog((char)('a'), logger2);
                for (int i = 0; i < maxArchiveFiles; i++)
                {
                    Generate100BytesLog((char)('b' + i), logger1);
                    Generate100BytesLog((char)('b' + i), logger2);
                    var logFile1 = string.Format(logFile, logger1.Name, TimeSource.Current.Time.Date.ToString("yyyy-MM-dd") + "." + i.ToString());
                    var logFile2 = string.Format(logFile, logger2.Name, TimeSource.Current.Time.Date.ToString("yyyy-MM-dd") + "." + i.ToString());
                    Assert.True(File.Exists(logFile1),
                        $"{logFile1} is missing");
                    Assert.True(File.Exists(logFile2),
                        $"{logFile2} is missing");
                }

                for (int i = maxArchiveFiles; i < 10; i++)
                {
                    Generate100BytesLog((char)('b' + i), logger1);
                    Generate100BytesLog((char)('b' + i), logger2);
                    var numberToBeRemoved = i - maxArchiveFiles;

                    var logFile1 = string.Format(logFile, logger1.Name, TimeSource.Current.Time.Date.ToString("yyyy-MM-dd") + "." + numberToBeRemoved.ToString());
                    var logFile2 = string.Format(logFile, logger2.Name, TimeSource.Current.Time.Date.ToString("yyyy-MM-dd") + "." + numberToBeRemoved.ToString());

                    Assert.False(File.Exists(logFile1),
                        $"archive FirstFile {numberToBeRemoved} has not been removed! We are created file {i}");
                    Assert.False(File.Exists(logFile2),
                        $"archive SecondFile {numberToBeRemoved} has not been removed! We are created file {i}");

                    logFile1 = string.Format(logFile, logger1.Name, TimeSource.Current.Time.Date.ToString("yyyy-MM-dd") + "." + i.ToString());
                    logFile2 = string.Format(logFile, logger2.Name, TimeSource.Current.Time.Date.ToString("yyyy-MM-dd") + "." + i.ToString());

                    Assert.True(File.Exists(logFile1),
                        $"{logFile1} is missing");
                    Assert.True(File.Exists(logFile2),
                        $"{logFile2} is missing");
                }

                LogManager.Configuration = null;
            }
            finally
            {
                TimeSource.Current = defaultTimeSource; // restore default time source

                if (Directory.Exists(tempPath))
                    Directory.Delete(tempPath, true);
            }
        }

        /// <summary>
        /// See that dynamic sequence archive supports same-folder archiving.
        /// </summary>
        [Fact]
        public void FileTarget_SameDirectory_MaxArchiveFiles_One()
        {
            const int maxArchiveFiles = 1;

            var tempPath = ArchiveFileNameHelper.GenerateTempPath();
            var logFile1 = Path.Combine(tempPath, "MyFile{0}.txt");
            try
            {
                var fileTarget1 = new FileTarget
                {
                    FileName = string.Format(logFile1, ""),
                    ArchiveAboveSize = 100,
                    LineEnding = LineEndingMode.LF,
                    Layout = "${message}",
                    MaxArchiveFiles = maxArchiveFiles,
                    Encoding = Encoding.ASCII,
                };

                SimpleConfigurator.ConfigureForTargetLogging(fileTarget1, LogLevel.Debug);

                Generate100BytesLog('a');
                Generate100BytesLog('b');
                Generate100BytesLog('c');

                var times = 25;
                AssertFileContents(string.Format(logFile1, ".0"),
                    StringRepeat(times, "bbb\n"),
                    Encoding.ASCII);

                AssertFileContents(string.Format(logFile1, ""),
                    StringRepeat(times, "ccc\n"),
                    Encoding.ASCII);

                LogManager.Configuration = null;
            }
            finally
            {
                if (Directory.Exists(tempPath))
                    Directory.Delete(tempPath, true);
            }
        }

        private void Generate100BytesLog(char c, ILogger logger = null)
        {
            logger = logger ?? this.logger;
            for (var i = 0; i < 25; ++i)
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
                Ext = ext.TrimStart('.');
                FileName = fileName;
                FolderName = folderName;
            }

            public bool Exists(int number)
            {
                return File.Exists(GetFullPath(number));
            }

            public string GetFullPath(int number)
            {
                return Path.Combine($"{FolderName}/{FileName}.{number}.{Ext}");
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
            string archiveFileName = Path.Combine(archivePath, $"{{{placeHolderSharps}}}.log");
            string expectedArchiveFullName =
                $"{archivePath}/{DateTime.Now.ToString(archiveDateFormat)}.{expectedSequenceInArchiveFileName}.log";

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
            string expectedArchiveFullName = $"{archivePath}/{expectedDateInArchiveFileName}.1.log";

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
            var fileTarget = new FileTarget
            {
                FileName = logFileName,
                ArchiveFileName = archiveFileName,
                ArchiveDateFormat = archiveDateFormat,
                ArchiveNumbering = archiveNumbering,
                ArchiveAboveSize = logFileMaxSize
            };
            SimpleConfigurator.ConfigureForTargetLogging(fileTarget, LogLevel.Debug);
            for (int currentSequenceNumber = 0; currentSequenceNumber < count; currentSequenceNumber++)
                logger.Debug("Test {0}", currentSequenceNumber);

            LogManager.Flush();
        }

        [Fact]
        public void Dont_throw_Exception_when_archiving_is_enabled()
        {
            try
            {
                LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"<?xml version='1.0' encoding='utf-8' ?>
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

                LogManager.GetLogger("Test").Info("very important message");
            }
            finally
            {
                LogManager.Configuration = null;
                NLog.Common.InternalLogger.Reset();
            }
        }

        [Fact]
        public void Dont_throw_Exception_when_archiving_is_enabled_with_async()
        {
            try
            {
                LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"<?xml version='1.0' encoding='utf-8' ?>
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

                LogManager.GetLogger("Test").Info("very important message");
            }
            finally
            {
                LogManager.Configuration = null;
            }
        }

        [Fact]
        public void DatedArchiveForFileTargetWithMultipleFiles()
        {
            var defaultTimeSource = TimeSource.Current;

            var tempPath = Path.Combine(Path.GetTempPath(), "nlog_" + Guid.NewGuid().ToString()) + Path.DirectorySeparatorChar;

            try
            {
                var timeSource = new TimeSourceTests.ShiftedTimeSource(DateTimeKind.Local);
                if (timeSource.Time.Minute == 59)
                {
                    // Avoid double-archive due to overflow of the hour.
                    timeSource.AddToLocalTime(TimeSpan.FromMinutes(1));
                    timeSource.AddToSystemTime(TimeSpan.FromMinutes(1));
                }
                TimeSource.Current = timeSource;

                GlobalDiagnosticsContext.Set("basedir", tempPath);

                LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"<?xml version='1.0' encoding='utf-8' ?>
<nlog>
  <variable name='basedir' value='' />
  <targets>
      <target name='file' type='File'
              fileName='${gdc:item=basedir}${event-properties:item=serialNo}.txt'
              layout='${message}'
              archiveFileName='${gdc:item=basedir}${event-properties:item=serialNo}.{#}.txt'
              archiveNumbering='Date'
              archiveDateFormat='yyyy-MM-dd'
              archiveEvery='Day' />
  </targets>
  <rules>
    <logger name='*' writeTo='file' />
  </rules>
</nlog>
");

                var fileLogger = LogManager.GetLogger(nameof(DatedArchiveForFileTargetWithMultipleFiles));
                LogEventInfo logEvent = LogEventInfo.Create(LogLevel.Info, fileLogger.Name, "Very Important Message");
                logEvent.Properties["serialNo"] = "M91803ED2172";
                logger.Log(logEvent);

                LogEventInfo logEvent2 = LogEventInfo.Create(LogLevel.Info, fileLogger.Name, "Very Important Message");
                logEvent2.Properties["serialNo"] = "M91803ED2137";
                logger.Log(logEvent2);

                var currentDate = timeSource.Time.Date;
                timeSource.AddToLocalTime(TimeSpan.FromDays(5));

                LogEventInfo logEvent3 = LogEventInfo.Create(LogLevel.Info, fileLogger.Name, "Very Important Message");
                logEvent3.Properties["serialNo"] = logEvent.Properties["serialNo"];
                logger.Log(logEvent3);

                LogEventInfo logEvent4 = LogEventInfo.Create(LogLevel.Info, fileLogger.Name, "Very Important Message");
                logEvent4.Properties["serialNo"] = logEvent2.Properties["serialNo"];
                logger.Log(logEvent4);

                var currentFiles = new DirectoryInfo(tempPath).GetFiles();
                Assert.Equal(4, currentFiles.Length);
                Assert.Contains(logEvent.Properties["serialNo"] + ".txt", currentFiles.Select(f => f.Name));
                Assert.Contains(logEvent.Properties["serialNo"] + "." + currentDate.ToString("yyyy-MM-dd") + ".txt", currentFiles.Select(f => f.Name));
                Assert.Contains(logEvent2.Properties["serialNo"] + ".txt", currentFiles.Select(f => f.Name));
                Assert.Contains(logEvent2.Properties["serialNo"] + "." + currentDate.ToString("yyyy-MM-dd") + ".txt", currentFiles.Select(f => f.Name));
            }
            finally
            {
                TimeSource.Current = defaultTimeSource;

                LogManager.Configuration = null;
                if (Directory.Exists(tempPath))
                    Directory.Delete(tempPath, true);
            }
        }

        [Fact]
        public void LoggingShouldNotTriggerTypeResolveEventTest()
        {
            var tempPath = Path.Combine(Path.GetTempPath(), "nlog_" + Guid.NewGuid().ToString());
            var logFile = Path.Combine(tempPath, "log.log");

            System.ResolveEventHandler noResolveTest = (s, args) => { Assert.True(false); return null; };

            try
            {
                LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"<nlog throwExceptions='true'>
                        <targets>
                            <target name='file1' encoding='UTF-8' type='File' fileName='" + logFile + @"'>
                                <layout type='JsonLayout'>
                                  <attribute name='message' layout='${message}' />
                                  <attribute name='xml'>
                                    <layout type='Log4JXmlEventLayout' />
                                  </attribute>
                                </layout>
                            </target>
                        </targets>
                        <rules>
                            <logger name='*' minlevel='Trace' writeTo='file1' />
                        </rules>
                    </nlog>");

                LogManager.GetLogger("Test").Info("very important message");

                AppDomain.CurrentDomain.TypeResolve += noResolveTest;
                AppDomain.CurrentDomain.AssemblyResolve += noResolveTest;

                LogManager.GetLogger("Test").Info("very important message");
            }
            finally
            {
                AppDomain.CurrentDomain.TypeResolve -= noResolveTest;
                AppDomain.CurrentDomain.AssemblyResolve -= noResolveTest;
                LogManager.Configuration = null;
                if (Directory.Exists(tempPath))
                    Directory.Delete(tempPath, true);
            }
        }

        [Theory]
        [InlineData("yyyyMMdd-HHmm")]
        [InlineData("yyyyMMdd")]
        [InlineData("yyyy-MM-dd")]
        public void MaxArchiveFilesWithDateFormatTest(string archiveDateFormat)
        {
            TestMaxArchiveFilesWithDate(2, 2, archiveDateFormat, true);
            TestMaxArchiveFilesWithDate(2, 2, archiveDateFormat, false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="maxArchiveFilesConfig">max count of archived files</param>
        /// <param name="expectedArchiveFiles">expected count of archived files</param>
        /// <param name="dateFormat">date format</param>
        /// <param name="changeCreationAndWriteTime">change file creation/last write date</param>
        private void TestMaxArchiveFilesWithDate(int maxArchiveFilesConfig, int expectedArchiveFiles, string dateFormat, bool changeCreationAndWriteTime)
        {
            string logdir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            string archivePath = Path.Combine(logdir, "archive");

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
                var configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
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
                      <logger name='*' writeTo='fileAll' />
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
                if (Directory.Exists(logdir))
                    Directory.Delete(logdir, true);
            }
        }

        /// unit test for issue #1681.
        ///- When clearing out archive files exceeding maxArchiveFiles, NLog should
        ///  not delete all files in the directory.Only files matching the target's
        ///  archiveFileName pattern.
        ///- Create test for 2 applications sharing the same archive directory.
        ///- Create test for 2 applications sharing same archive directory and 1
        ///  application containing multiple targets to the same archive directory.
        ///  *\* Expected outcome of this should be verified**
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void HandleArchiveFilesMultipleContextMultipleTargetTest(bool changeCreationAndWriteTime)
        {
            HandleArchiveFilesMultipleContextMultipleTargetsTest(2, 2, "yyyyMMdd-HHmm", changeCreationAndWriteTime);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void HandleArchiveFilesMultipleContextSingleTargetTest_ascii(bool changeCreationAndWriteTime)
        {
            HandleArchiveFilesMultipleContextSingleTargetsTest(2, 2, "yyyyMMdd-HHmm", changeCreationAndWriteTime);
        }

        /// <summary>
        /// Test the case when multiple applications are archiving to the same directory and using multiple targets.
        /// Only the archives for this application instance should be deleted per the target archive rules.
        /// </summary>
        /// <param name="maxArchiveFilesConfig"># to use for maxArchiveFiles in NLog configuration.</param>
        /// <param name="expectedArchiveFiles">Expected number of archive files after archiving has occured.</param>
        /// <param name="dateFormat">string to be used for formatting log file names</param>
        /// <param name="changeCreationAndWriteTime"></param>
        private void HandleArchiveFilesMultipleContextMultipleTargetsTest(
            int maxArchiveFilesConfig, int expectedArchiveFiles, string dateFormat, bool changeCreationAndWriteTime)
        {
            string logdir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            string archivePath = Path.Combine(logdir, "archive");
            var archiveDir = new DirectoryInfo(archivePath);
            try
            {
                archiveDir.Create();
                //set-up, create files.
                var numberFilesCreatedPerTargetArchive = 30;

                // use same config vars for mock files, as for nlog config
                var fileExt = ".log";
                var app1TraceNm = "App1_Trace";
                var app1DebugNm = "App1_Debug";
                var app2Nm = "App2";

                var now = DateTime.Now;
                var i = 0;
                // create mock app1_trace archives (matches app1 config for trace target)
                foreach (string filePath in ArchiveFileNamesGenerator(archivePath, dateFormat, app1TraceNm + fileExt).Take(numberFilesCreatedPerTargetArchive))
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
                i = 0;
                // create mock app1_debug archives (matches app1 config for debug target)
                foreach (string filePath in ArchiveFileNamesGenerator(archivePath, dateFormat, app1DebugNm + fileExt).Take(numberFilesCreatedPerTargetArchive))
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
                i = 0;
                // create mock app2 archives (matches app2 config for target)
                foreach (string filePath in ArchiveFileNamesGenerator(archivePath, dateFormat, app2Nm + fileExt).Take(numberFilesCreatedPerTargetArchive))
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

                // Create same app1 Debug file as config defines. Will force archiving to happen on startup
                File.WriteAllLines(logdir + "\\" + app1DebugNm + fileExt, new[] { "Write first app debug target. Startup will archive this file" }, Encoding.ASCII);

                var app1Config = XmlLoggingConfiguration.CreateFromXmlString(@"<nlog throwExceptions='true'>
                                    <targets>
                                      <target name='traceFile' type='File' 
                                        fileName='" + Path.Combine(logdir, app1TraceNm + fileExt) + @"'
                                        archiveFileName='" + Path.Combine(archivePath, @"${date:format=" + dateFormat + "}-" + app1TraceNm + fileExt) + @"' 
                                        archiveEvery='minute' 
                                        archiveOldFileOnStartup='true'
                                        maxArchiveFiles='" + maxArchiveFilesConfig + @"'
                                        layout='${longdate} [${level}] [${callsite}] ${message}' 
                                        concurrentWrites='true' keepFileOpen='false' />
                                    <target name='debugFile' type='File' 
                                        fileName='" + Path.Combine(logdir, app1DebugNm + fileExt) + @"'
                                        archiveFileName='" + Path.Combine(archivePath, @"${date:format=" + dateFormat + "}-" + app1DebugNm + fileExt) + @"' 
                                        archiveEvery='minute' 
                                        archiveOldFileOnStartup='true'
                                        maxArchiveFiles='" + maxArchiveFilesConfig + @"'
                                        layout='${longdate} [${level}] [${callsite}] ${message}' 
                                        concurrentWrites='true' keepFileOpen='false' />
                                    </targets>
                                    <rules>
                                      <logger name='*' minLevel='Trace' writeTo='traceFile' />
                                      <logger name='*' minLevel='Debug' writeTo='debugFile' />
                                    </rules>
                                  </nlog>");

                var app2Config = XmlLoggingConfiguration.CreateFromXmlString(@"<nlog throwExceptions='true'>
                                    <targets>
                                      <target name='logfile' type='File' 
                                        fileName='" + Path.Combine(logdir, app2Nm + fileExt) + @"'
                                        archiveFileName='" + Path.Combine(archivePath, @"${date:format=" + dateFormat + "}-" + app2Nm + fileExt) + @"' 
                                        archiveEvery='minute' 
                                        archiveOldFileOnStartup='true'
                                        maxArchiveFiles='" + maxArchiveFilesConfig + @"'
                                        layout='${longdate} [${level}] [${callsite}] ${message}' 
                                        concurrentWrites='true' keepFileOpen='false' />
                                    </targets>;
                                    <rules>
                                      <logger name='*' minLevel='Trace' writeTo='logfile' />
                                    </rules>
                                  </nlog>");

                LogManager.Configuration = app1Config;

                var logger = LogManager.GetCurrentClassLogger();
                // Trigger archive to happen on startup
                logger.Trace("Test 1 - Write to the log file that already exists; trigger archive to happen because archiveOldFileOnStartup='true'");

                // TODO: perhaps extra App1 Debug and Trace files should both be deleted?  (then app1TraceTargetFileCnt would be expected to = expectedArchiveFiles too)
                // I think it depends on how NLog works with logging to both of those files in the call to logger.Debug() above

                // verify file counts. EXPECTED OUTCOME:
                // app1 trace target: removed all extra
                // app1 debug target: has all extra files
                // app2: has all extra files
                var app1TraceTargetFileCnt = archiveDir.GetFiles("*" + app1TraceNm + "*").Length;
                var app1DebugTargetFileCnt = archiveDir.GetFiles("*" + app1DebugNm + "*").Length;
                var app2FileTargetCnt = archiveDir.GetFiles("*" + app2Nm + "*").Length;

                Assert.Equal(numberFilesCreatedPerTargetArchive, app1DebugTargetFileCnt);
                Assert.Equal(numberFilesCreatedPerTargetArchive, app2FileTargetCnt);
                Assert.Equal(expectedArchiveFiles, app1TraceTargetFileCnt);
            }
            finally
            {
                //cleanup
                LogManager.Configuration = null;
                archiveDir.Delete(true);
                if (Directory.Exists(logdir))
                    Directory.Delete(logdir, true);
            }
        }

        /// <summary>
        /// Test the case when multiple applications are archiving to the same directory.
        /// Only the archives for this application instance should be deleted per the target archive rules.
        /// </summary>
        /// <param name="maxArchiveFilesConfig"># to use for maxArchiveFiles in NLog configuration.</param>
        /// <param name="expectedArchiveFiles">Expected number of archive files after archiving has occured.</param>
        /// <param name="dateFormat">string to be used for formatting log file names</param>
        /// <param name="changeCreationAndWriteTime"></param>
        private void HandleArchiveFilesMultipleContextSingleTargetsTest(
            int maxArchiveFilesConfig, int expectedArchiveFiles, string dateFormat, bool changeCreationAndWriteTime)
        {
            string logdir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            string archivePath = Path.Combine(logdir, "archive");
            var archiveDir = new DirectoryInfo(archivePath);
            try
            {
                archiveDir.Create();
                var numberFilesCreatedPerTargetArchive = 30;

                // use same config vars for mock files, as for nlog config
                var fileExt = ".log";
                var app1Nm = "App1";
                var app2Nm = "App2";

                var now = DateTime.Now;
                var i = 0;
                // create mock app1 archives (matches app1 config for target)
                foreach (string filePath in ArchiveFileNamesGenerator(archivePath, dateFormat, app1Nm + fileExt).Take(numberFilesCreatedPerTargetArchive))
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
                i = 0;
                // create mock app2 archives (matches app2 config for target)
                foreach (string filePath in ArchiveFileNamesGenerator(archivePath, dateFormat, app2Nm + fileExt).Take(numberFilesCreatedPerTargetArchive))
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

                // Create same app1 file as config defines. Will force archiving to happen on startup
                File.WriteAllLines(Path.Combine(logdir, app1Nm + fileExt), new[] { "Write first app debug target. Startup will archive this file" }, Encoding.ASCII);

                var app1Config = XmlLoggingConfiguration.CreateFromXmlString(@"<nlog throwExceptions='true'>
                                    <targets>
                                      <target name='logfile' type='File' 
                                        fileName='" + Path.Combine(logdir, app1Nm + fileExt) + @"'
                                        archiveFileName='" + Path.Combine(archivePath, @"${date:format=" + dateFormat + "}-" + app1Nm + fileExt) + @"' 
                                        archiveEvery='minute' 
                                        archiveOldFileOnStartup='true'
                                        maxArchiveFiles='" + maxArchiveFilesConfig + @"'
                                        layout='${longdate} [${level}] [${callsite}] ${message}' 
                                        concurrentWrites='true' keepFileOpen='false' />
                                    </targets>;
                                    <rules>
                                      <logger name='*' minLevel='Trace' writeTo='logfile' />
                                    </rules>
                                  </nlog>");

                var app2Config = XmlLoggingConfiguration.CreateFromXmlString(@"<nlog throwExceptions='true'>
                                    <targets>
                                      <target name='logfile' type='File' 
                                        fileName='" + Path.Combine(logdir, app2Nm + fileExt) + @"'
                                        archiveFileName='" + Path.Combine(archivePath, @"${date:format=" + dateFormat + "}-" + app2Nm + fileExt) + @"' 
                                        archiveEvery='minute' 
                                        archiveOldFileOnStartup='true'
                                        maxArchiveFiles='" + maxArchiveFilesConfig + @"'
                                        layout='${longdate} [${level}] [${callsite}] ${message}' 
                                        concurrentWrites='true' keepFileOpen='false' />
                                    </targets>;
                                    <rules>
                                      <logger name='*' minLevel='Trace' writeTo='logfile' />
                                    </rules>
                                  </nlog>");

                LogManager.Configuration = app1Config;

                var logger = LogManager.GetCurrentClassLogger();
                // Trigger archive to happen on startup
                logger.Debug("Test 1 - Write to the log file that already exists; trigger archive to happen because archiveOldFileOnStartup='true'");

                // verify file counts. EXPECTED OUTCOME:
                // app1: Removed extra archives
                // app2: Has all extra archives
                var app1TargetFileCnt = archiveDir.GetFiles("*" + app1Nm + "*").Length;
                var app2FileTargetCnt = archiveDir.GetFiles("*" + app2Nm + "*").Length;

                Assert.Equal(numberFilesCreatedPerTargetArchive, app2FileTargetCnt);
                Assert.Equal(expectedArchiveFiles, app1TargetFileCnt);
            }
            finally
            {
                //cleanup
                LogManager.Configuration = null;
                archiveDir.Delete(true);
                if (Directory.Exists(logdir))
                    Directory.Delete(logdir, true);
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
                var fileTarget = new FileTarget
                {
                    FileName = fullFilePath,
                    LineEnding = LineEndingMode.LF,
                    Layout = "${level} ${message}",
                    OpenFileCacheTimeout = 0
                };

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
                var fileTarget = new FileTarget
                {
                    FileName = fullFilePath,
                    LineEnding = LineEndingMode.LF,
                    Layout = "${level} ${message}",
                    OpenFileCacheTimeout = 0
                };

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
            string tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var logfile = Path.Combine(tempPath, "file.txt");
            try
            {
                string archiveFolder = Path.Combine(tempPath, "archive");
                var fileTarget = new FileTarget
                {
                    FileName = logfile,
                    ArchiveFileName = Path.Combine(archiveFolder, "{####}.txt"),
                    ArchiveAboveSize = 100,
                    ArchiveOldFileOnStartup = true, // Verify ArchiveOldFileOnStartup works together with ArchiveAboveSize
                    LineEnding = LineEndingMode.LF,
                    ArchiveNumbering = ArchiveNumberingMode.Sequence,
                    Layout = "${message}",
                    MaxArchiveFiles = 0
                };

                SimpleConfigurator.ConfigureForTargetLogging(fileTarget, LogLevel.Debug);
                logfile = Path.GetFullPath(logfile);
                // we emit 5 * 25 *(3 x aaa + \n) bytes
                // so that we should get a full file + 4 archives
                Generate100BytesLog('a');
                Generate100BytesLog('b');
                Generate100BytesLog('c');
                Generate100BytesLog('d');
                Generate100BytesLog('e');

                LogManager.Configuration = null;

                var times = 25;
                AssertFileContents(logfile,
                    StringRepeat(times, "eee\n"),
                    Encoding.UTF8);

                AssertFileContents(
                   Path.Combine(archiveFolder, "0000.txt"),
                   StringRepeat(times, "aaa\n"),
                   Encoding.UTF8);

                AssertFileContents(
                    Path.Combine(archiveFolder, "0001.txt"),
                    StringRepeat(times, "bbb\n"),
                    Encoding.UTF8);

                AssertFileContents(
                    Path.Combine(archiveFolder, "0002.txt"),
                    StringRepeat(times, "ccc\n"),
                    Encoding.UTF8);

                AssertFileContents(
                    Path.Combine(archiveFolder, "0003.txt"),
                    StringRepeat(times, "ddd\n"),
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

        [Fact]
        public void TestFilenameCleanup()
        {
            var invalidChars = Path.GetInvalidFileNameChars();
            var invalidFileName = Path.DirectorySeparatorChar.ToString();
            var expectedFileName = "";
            for (int i = 0; i < invalidChars.Count(); i++)
            {
                var invalidChar = invalidChars[i];
                if (invalidChar == Path.DirectorySeparatorChar || invalidChar == Path.AltDirectorySeparatorChar)
                {
                    //ignore, won't used in cleanup (but for find filename in path)
                    continue;
                }

                invalidFileName += i + invalidChar.ToString();
                //underscore is used for clean
                expectedFileName += i + "_";
            }
            //under mono this the invalid chars is sometimes only 1 char (so min width 2)
            Assert.True(invalidFileName.Length >= 2);
            //CleanupFileName is default true;
            var fileTarget = new FileTarget();
            fileTarget.FileName = invalidFileName;

            var filePathLayout = new NLog.Internal.FilePathLayout(invalidFileName, true, FilePathKind.Absolute);


            var path = filePathLayout.Render(LogEventInfo.CreateNullEvent());
            Assert.Equal(expectedFileName, path);
        }

        [Theory]
        [InlineData(DayOfWeek.Sunday, "2017-03-02 15:27:34.651", "2017-03-05 15:27:34.651")]    // On a Thursday, finding next Sunday
        [InlineData(DayOfWeek.Thursday, "2017-03-02 15:27:34.651", "2017-03-09 15:27:34.651")]  // On a Thursday, finding next Thursday
        [InlineData(DayOfWeek.Monday, "2017-03-02 00:00:00.000", "2017-03-06 00:00:00.000")]    // On Thursday at Midnight, finding next Monday
        public void TestCalculateNextWeekday(DayOfWeek day, string todayString, string expectedString)
        {
            DateTime today = DateTime.Parse(todayString);
            DateTime expected = DateTime.Parse(expectedString);
            DateTime actual = FileTarget.CalculateNextWeekday(today, day);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("UTF-16", true)]
        [InlineData("UTF-16BE", true)]
        [InlineData("UTF-32", true)]
        [InlineData("UTF-32BE", true)]
        [InlineData("UTF-7", false)]
        [InlineData("UTF-8", false)]
        [InlineData("ASCII", false)]
        public void TestInitialBomValue(string encodingName, bool expected)
        {
            var fileTarget = new FileTarget();

            // Act
            fileTarget.Encoding = Encoding.GetEncoding(encodingName);

            // Assert
            Assert.Equal(expected, fileTarget.WriteBom);
        }

        [Fact]
        public void BatchErrorHandlingTest()
        {
            using (new NoThrowNLogExceptions())
            {
                var fileTarget = new FileTarget { FileName = "${logger}", Layout = "${message}", ArchiveAboveSize = 10, DiscardAll = true };
                fileTarget.Initialize(null);

                // make sure that when file names get sorted, the asynchronous continuations are sorted with them as well
                var exceptions = new List<Exception>();
                var events = new[]
                {
                    new LogEventInfo(LogLevel.Info, "file99.txt", "msg1").WithContinuation(exceptions.Add),
                    new LogEventInfo(LogLevel.Info, "", "msg2").WithContinuation(exceptions.Add),
                    new LogEventInfo(LogLevel.Info, "", "msg3").WithContinuation(exceptions.Add),
                    new LogEventInfo(LogLevel.Info, "", "msg4").WithContinuation(exceptions.Add),
                    new LogEventInfo(LogLevel.Info, "file99.txt", "msg5").WithContinuation(exceptions.Add),
                };

                fileTarget.WriteAsyncLogEvents(events);
                LogManager.Flush();

                Assert.Equal(5, exceptions.Count);
                Assert.Null(exceptions[0]);
                Assert.Null(exceptions[1]); // Will be written together
                Assert.NotNull(exceptions[2]);
                Assert.NotNull(exceptions[3]);
                Assert.NotNull(exceptions[4]);
            }
        }

        [Fact]
        public void BatchBufferOverflowTest()
        {
            string tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var logfile = Path.Combine(tempPath, "file.txt");
            try
            {
                // Arrange
                var fileTarget = new FileTarget
                {
                    FileName = logfile,
                    BufferSize = 5,
                    LineEnding = LineEndingMode.LF,
                    Layout = "${message}",
                    Encoding = Encoding.UTF8,
                };
                fileTarget.Initialize(null);

                var result = new List<int>();
                var events = new List<NLog.Common.AsyncLogEventInfo>();
                var times = 200;
                for (int i = 1; i <= times; ++i)
                {
                    int counter = i;
                    events.Add(new LogEventInfo(LogLevel.Info, "logger", counter.ToString()).WithContinuation(ex => result.Add(ex == null ? counter : -1)));
                }

                // Act
                fileTarget.WriteAsyncLogEvents(events);

                // Assert
                Assert.Equal(Enumerable.Range(1, times).ToList(), result);
                AssertFileContents(logfile, string.Join("\n", result.ToArray()) + "\n", Encoding.UTF8);
            }
            finally
            {
                if (File.Exists(logfile))
                    File.Delete(logfile);
                if (Directory.Exists(tempPath))
                    Directory.Delete(tempPath, true);
            }
        }

        [Fact]
        public void HandleArchiveFileAlreadyExistsTest_noBom()
        {
            //NO bom
            var utf8nobom = new UTF8Encoding(false);

            HandleArchiveFileAlreadyExistsTest(utf8nobom, false);
        }

        [Fact]
        public void HandleArchiveFileAlreadyExistsTest_withBom()
        {
            // bom
            var utf8nobom = new UTF8Encoding(true);

            HandleArchiveFileAlreadyExistsTest(utf8nobom, true);
        }

        [Fact]
        public void HandleArchiveFileAlreadyExistsTest_ascii()
        {
            //NO bom
            var encoding = Encoding.ASCII;

            HandleArchiveFileAlreadyExistsTest(encoding, false);
        }

        private void HandleArchiveFileAlreadyExistsTest(Encoding encoding, bool hasBom)
        {
            var tempDir = Path.Combine(Path.GetTempPath(), "HandleArchiveFileAlreadyExistsTest-" + Guid.NewGuid());
            string logFile = Path.Combine(tempDir, "log.txt");
            try
            {
                // set log file access times the same way as when this issue comes up.
                Directory.CreateDirectory(tempDir);

                File.WriteAllText(logFile, "some content" + Environment.NewLine, encoding);
                var oldTime = DateTime.Now.AddDays(-2);
                File.SetCreationTime(logFile, oldTime);
                File.SetLastWriteTime(logFile, oldTime);
                File.SetLastAccessTime(logFile, oldTime);

                //write to archive directly
                var archiveDateFormat = "yyyy-MM-dd";
                var archiveFileNamePattern = Path.Combine(tempDir, "log-{#}.txt");
                var archiveFileName = archiveFileNamePattern.Replace("{#}", oldTime.ToString(archiveDateFormat));
                File.WriteAllText(archiveFileName, "message already in archive" + Environment.NewLine, encoding);

                LogManager.ThrowExceptions = true;

                // configure nlog
                var fileTarget = new FileTarget("file")
                {
                    FileName = logFile,
                    ArchiveEvery = FileArchivePeriod.Day,
                    ArchiveFileName = archiveFileNamePattern,
                    ArchiveNumbering = ArchiveNumberingMode.Date,
                    ArchiveDateFormat = archiveDateFormat,
                    Encoding = encoding,
                    Layout = "${message}",
                    WriteBom = hasBom,
                };

                var config = new LoggingConfiguration();
                config.AddTarget(fileTarget);
                config.AddRuleForAllLevels(fileTarget);

                LogManager.Configuration = config;

                var logger = LogManager.GetLogger("HandleArchiveFileAlreadyExistsTest");
                // write, this should append.
                logger.Info("log to force archiving");
                logger.Info("log to same file");

                LogManager.Configuration = null;    // Flush

                AssertFileContents(archiveFileName, "message already in archive" + Environment.NewLine + "some content" + Environment.NewLine, encoding, hasBom);
                AssertFileContents(logFile, "log to force archiving" + Environment.NewLine + "log to same file" + Environment.NewLine, encoding, hasBom);
            }
            finally
            {
                try
                {
                    if (File.Exists(logFile))
                        File.Delete(logFile);
                    Directory.Delete(tempDir, true);
                }
                catch (Exception)
                {
                }
            }
        }

        [Fact]
        public void DontCrashWhenDateAndSequenceDoesntMatchFiles()
        {
            var tempDir = Path.Combine(Path.GetTempPath(), "DontCrashWhenDateAndSequenceDoesntMatchFiles-" + Guid.NewGuid());
            string logFile = Path.Combine(tempDir, "log.txt");
            try
            {
                // set log file access times the same way as when this issue comes up.
                Directory.CreateDirectory(tempDir);

                File.WriteAllText(logFile, "some content" + Environment.NewLine);
                var oldTime = DateTime.Now.AddDays(-2);
                File.SetCreationTime(logFile, oldTime);
                File.SetLastWriteTime(logFile, oldTime);
                File.SetLastAccessTime(logFile, oldTime);

                //write to archive directly
                var archiveDateFormat = "yyyyMMdd";
                var archiveFileNamePattern = Path.Combine(tempDir, "log-{#}.txt");
                var archiveFileName = archiveFileNamePattern.Replace("{#}", oldTime.ToString(archiveDateFormat));
                File.WriteAllText(archiveFileName, "some archive content");

                LogManager.ThrowExceptions = true;

                // configure nlog
                var fileTarget = new FileTarget("file")
                {
                    FileName = logFile,
                    ArchiveEvery = FileArchivePeriod.Day,
                    ArchiveFileName = "log-{#}.txt",
                    ArchiveNumbering = ArchiveNumberingMode.DateAndSequence,
                    ArchiveAboveSize = 50000,
                    MaxArchiveFiles = 7
                };

                var config = new LoggingConfiguration();
                config.AddRuleForAllLevels(fileTarget);
                LogManager.Configuration = config;

                // write
                var logger = LogManager.GetLogger("DontCrashWhenDateAndSequenceDoesntMatchFiles");
                logger.Info("Log message");

                LogManager.Configuration = null;
            }
            finally
            {
                try
                {
                    if (File.Exists(logFile))
                        File.Delete(logFile);
                    Directory.Delete(tempDir, true);
                }
                catch (Exception)
                {
                }
            }
        }

        [Theory]
        [InlineData(true, 100, true)] // archive, as size of file is 101
        [InlineData(true, 101, false)] //equals is not above
        [InlineData(true, 102, false)] // don;t archive, we didn't reach the aboveSize
        [InlineData(false, 100, false)]
        [InlineData(null, 0, false)]
        [InlineData(null, 99, true)]
        [InlineData(null, 100, true)]
        [InlineData(null, 101, false)]
        public void ShouldArchiveOldFileOnStartupTest(bool? archiveOldFileOnStartup, long archiveOldFileOnStartupAboveSize, bool expected)
        {
            // Arrange
            var fileAppenderCacheMock = Substitute.For<NLog.Internal.FileAppenders.IFileAppenderCache>();

            var filePath = "x:/somewhere/file.txt";
            fileAppenderCacheMock.GetFileLength(filePath).Returns(101);

            var target = new FileTarget(fileAppenderCacheMock)
            {
                ArchiveOldFileOnStartupAboveSize = archiveOldFileOnStartupAboveSize
            };
            if (archiveOldFileOnStartup.HasValue)
            {
                target.ArchiveOldFileOnStartup = archiveOldFileOnStartup.Value;
            }

            // Act
            var result = target.ShouldArchiveOldFileOnStartup(filePath);

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void ShouldNotArchiveWhenMeetingOldLogEventTimestamps()
        {
            var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var logFile = Path.Combine(tempPath, "file.txt");
            try
            {
                string archiveFolder = Path.Combine(tempPath, "archive");
                var fileTarget = new FileTarget
                {
                    FileName = logFile,
                    ArchiveFileName = Path.Combine(archiveFolder, "{####}.txt"),
                    ArchiveEvery = FileArchivePeriod.Day,
                    LineEnding = LineEndingMode.LF,
                    ArchiveNumbering = ArchiveNumberingMode.Sequence,
                    Layout = "${message}",
                };

                SimpleConfigurator.ConfigureForTargetLogging(fileTarget, LogLevel.Debug);

                var logger = LogManager.GetLogger(nameof(ShouldNotArchiveWhenMeetingOldLogEventTimestamps));
                logger.Info("123");
                logger.Info("456");
                logger.Log(new LogEventInfo(LogLevel.Info, null, "789") { TimeStamp = NLog.Time.TimeSource.Current.Time.AddDays(-2) });
                logger.Log(new LogEventInfo(LogLevel.Info, null, "123") { TimeStamp = NLog.Time.TimeSource.Current.Time.AddDays(-2) });
                logger.Info("456");
                logger.Log(new LogEventInfo(LogLevel.Info, null, "123") { TimeStamp = NLog.Time.TimeSource.Current.Time.AddDays(1) });
                LogManager.Configuration = null;    // Flush

                AssertFileContents(logFile,
                    "123\n",
                    Encoding.UTF8);

                AssertFileContents(
                   Path.Combine(archiveFolder, "0000.txt"),
                    "123\n456\n789\n123\n456\n",
                    Encoding.UTF8);

                Assert.True(!File.Exists(Path.Combine(archiveFolder, "0001.txt")));
            }
            finally
            {
                if (File.Exists(logFile))
                    File.Delete(logFile);
                if (Directory.Exists(tempPath))
                    Directory.Delete(tempPath, true);
            }
        }
    }
}
