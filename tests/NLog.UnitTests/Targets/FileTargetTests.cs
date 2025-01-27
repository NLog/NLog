//
// Copyright (c) 2004-2024 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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
    using System.Threading.Tasks;
    using NLog.Config;
    using NLog.Layouts;
    using NLog.Targets;
    using NLog.Targets.Wrappers;
    using NLog.Time;
    using Xunit;

    public class FileTargetTests : NLogTestBase
    {
        private readonly Logger logger = LogManager.GetLogger("NLog.UnitTests.Targets.FileTargetTests");

        [Fact]
        public void SetupBuilder_WriteToFile()
        {
            var tempDir = Path.Combine(Path.GetTempPath(), "nlog_" + Guid.NewGuid().ToString());
            LogFactory logFactory = null;

            try
            {
                logFactory = new LogFactory().Setup().LoadConfiguration(c =>
                {
                    c.ForLogger().WriteToFile(Path.Combine(tempDir, "${logger}.txt"), "${message}", Encoding.UTF8, LineEndingMode.LF);
                }).LogFactory;

                logFactory.GetLogger("SetupBuilder").Info("Hello");

                AssertFileContents(Path.Combine(tempDir, "SetupBuilder.txt"), "Hello\n", Encoding.UTF8);
            }
            finally
            {
                logFactory?.Shutdown();
                if (Directory.Exists(tempDir))
                    Directory.Delete(tempDir, true);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void SimpleFileTest(bool keepFileOpen)
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
                    KeepFileOpen = keepFileOpen,
                };

                LogManager.Setup().LoadConfiguration(c => c.ForLogger().WriteTo(fileTarget));

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
        [InlineData(true)]
        [InlineData(false)]
        public void SimpleFileDeleteTest(bool keepFileOpen)
        {
            foreach (var archiveSameFolder in new[] { true, false })
            {
                RetryingIntegrationTest(3, () =>
                {
                    var logPath = Path.Combine(Path.GetTempPath(), "nlog_" + Guid.NewGuid().ToString(), "Archive");
                    var logFile = Path.GetFullPath(Path.Combine(logPath, "..", "nlogA.txt"));
                    //var arhiveFile = archiveSameFolder ? Path.GetFullPath(Path.Combine(logPath, "..", "nlogB.txt")) : Path.GetFullPath(Path.Combine(logPath, "nlogB.txt"));
                    var arhiveFile = Path.GetFullPath(Path.Combine(logPath, archiveSameFolder ? ".." : ".", "nlogB.txt"));

                    try
                    {
                        var fileTarget = new FileTarget
                        {
                            FileName = SimpleLayout.Escape(logFile),
                            ArchiveFileName = SimpleLayout.Escape(arhiveFile),
                            ArchiveEvery = FileArchivePeriod.Year,
                            LineEnding = LineEndingMode.LF,
                            Layout = "${level} ${message}",
                            OpenFileCacheTimeout = 0,
                            EnableFileDelete = true,
                            KeepFileOpen = keepFileOpen,
                            ArchiveAboveSize = archiveSameFolder ? 1000000 : 0,
                        };

                        LogManager.Setup().LoadConfiguration(c => c.ForLogger().WriteTo(fileTarget));

                        logger.Debug("aaa");

                        LogManager.Flush();

                        Directory.CreateDirectory(Path.GetDirectoryName(arhiveFile));
                        File.Move(logFile, arhiveFile);

                        if (keepFileOpen)
                            Thread.Sleep(1500); // Ensure EnableFileDeleteSimpleMonitor will trigger

                        logger.Info("bbb");

                        LogManager.Configuration = null;

                        AssertFileContents(logFile, "Info bbb\n", Encoding.UTF8);
                    }
                    finally
                    {
                        if (File.Exists(arhiveFile))
                        {
                            File.Delete(arhiveFile);
                        }

                        if (File.Exists(logFile))
                        {
                            File.Delete(logFile);
                        }

                        if (Directory.Exists(Path.GetDirectoryName(arhiveFile)))
                        {
                            Directory.Delete(Path.GetDirectoryName(arhiveFile));
                        }

                        if (Directory.Exists(Path.GetDirectoryName(logFile)))
                        {
                            Directory.Delete(Path.GetDirectoryName(logFile));
                        }
                    }
                });
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
            var logFile = Path.Combine(Path.GetTempPath(), "nlog_" + Guid.NewGuid().ToString() + "!@#$%^&()_-=+ .log");
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

                LogManager.Setup().LoadConfiguration(c => c.ForLogger().WriteTo(fileTarget));

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

                LogManager.Setup().LoadConfiguration(c => c.ForLogger().WriteTo(fileTarget));

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
        [InlineData(true)]
        [InlineData(false)]
        public void NonExistingDriveShouldNotDelayMuch(bool keepFileOpen)
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
                        KeepFileOpen = keepFileOpen,
                    };

                    LogManager.Setup().LoadConfiguration(c => c.ForLogger().WriteTo(fileTarget));

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

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void RollingArchiveEveryMonday(bool keepFileOpen)
        {
            var tempDir = Path.Combine(Path.GetTempPath(), "nlog_" + Guid.NewGuid().ToString());
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
                    FileName = Path.Combine(tempDir, "${message}_AppName.log"),
                    LineEnding = LineEndingMode.LF,
                    Layout = "${message}",
                    ArchiveEvery = FileArchivePeriod.Monday,
                    MaxArchiveFiles = 1,
                    KeepFileOpen = keepFileOpen,
                };

                LogManager.Setup().LoadConfiguration(c => c.ForLogger().WriteTo(fileTarget));
                for (int i = 0; i < 3; ++i)
                {
                    for (char j = 'A'; j < 'Z'; ++j)
                    {
                        logger.Debug(new string(j, 2));
                        timeSource.AddToLocalTime(TimeSpan.FromDays(1));
                        //timeSource.AddToSystemTime(TimeSpan.FromDays(1));
                    }
                }

                LogManager.Configuration = null;    // Flush and close

                var files = new DirectoryInfo(tempDir).GetFiles();
                Assert.Equal(25, files.Length);

                foreach (var file in files)
                {
                    Assert.Equal(14, file.Name.Length);
                    Assert.Equal(3, file.Length);
                }
            }
            finally
            {
                TimeSource.Current = defaultTimeSource;

                if (Directory.Exists(tempDir))
                    Directory.Delete(tempDir, true);
            }
        }

#if !MONO
        [Theory]
#else
        [Theory(Skip="Not supported on MONO on Travis, because of File birthtime not working")]
#endif
        [InlineData(false, false, true)]
        [InlineData(false, true, true)]
        [InlineData(false, false, false)]
        [InlineData(false, true, false)]
        [InlineData(true, false, true)]
        [InlineData(true, true, true)]
        [InlineData(true, false, false)]
        [InlineData(true, true, false)]
        public void DatedArchiveEveryMonth(bool archiveSubFolder, bool maxArchiveDays, bool archiveWithDateAndSequence)
        {
            if (IsLinux())
            {
                Console.WriteLine("[SKIP] FileTargetTests.DatedArchiveEveryMonth because SetCreationTime is not working on Travis");
                return;
            }

            var tempDir = Path.Combine(Path.GetTempPath(), "nlog_" + Guid.NewGuid().ToString());
            var logFile = Path.Combine(tempDir, "AppName.log");
            var archiveDir = archiveSubFolder ? Path.Combine(tempDir, "Archive") : tempDir;

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
                        ArchiveFileName = archiveSubFolder ? Path.Combine(archiveDir, "AppName.{#}.log") : Path.Combine(tempDir, "AppName.{#}.log"),
                        ArchiveSuffixFormat = archiveWithDateAndSequence ? "_{1:yyyyMMdd}_{0:0}" : "_{0:0}",
                        ArchiveEvery = FileArchivePeriod.Month
                    };
                    if (maxArchiveDays)
                        fileTarget.MaxArchiveDays = 5 * 30;
                    else
                        fileTarget.MaxArchiveFiles = 2;

                    LogManager.Setup().LoadConfiguration(c => c.ForLogger().WriteTo(fileTarget));
                    logger.Debug($"{i.ToString()}{i.ToString()}{i.ToString()}");
                    LogManager.Configuration = null;    // Flush

                    currentFiles = Directory.GetFiles(tempDir).ToList();
                    if (archiveSubFolder && Directory.Exists(archiveDir))
                        currentFiles.AddRange(Directory.GetFiles(archiveDir));

                    string newFile = string.Empty;
                    foreach (var fileName in currentFiles)
                    {
                        if (!createdFiles.Contains(fileName))
                        {
                            Assert.Empty(newFile);
                            newFile = fileName;

                            if (archiveWithDateAndSequence && createdFiles.Count > 1)
                            {
                                // Verify it used the last-modified-time (And not file-creation-time)
                                string dateName = string.Empty;
                                dateName = Path.GetFileName(fileName);
                                dateName = dateName.Replace("AppName_", "");
                                dateName = dateName.Replace("_0.log", "");
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

                if (Directory.Exists(archiveDir))
                    Directory.Delete(archiveDir, true);

                if (Directory.Exists(tempDir))
                    Directory.Delete(tempDir, true);
            }
        }

        [Fact]
        public void CsvHeaderTest()
        {
            var tempDir = Path.Combine(Path.GetTempPath(), "nlog_" + Guid.NewGuid().ToString());
            var logFile = Path.Combine(tempDir, "log.log");
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
                        ArchiveAboveSize = 100, // Only 2 LogEvents per file
                        MaxArchiveFiles = 1,
                    };
                    LogManager.Setup().LoadConfiguration(c => c.ForLogger().WriteTo(fileTarget));

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

                Assert.NotEqual(3, Directory.GetFiles(tempDir).Length);   // See that archive cleanup worked

                LogManager.Configuration = null;    // Close
            }
            finally
            {
                if (File.Exists(logFile))
                    File.Delete(logFile);
                if (Directory.Exists(tempDir))
                    Directory.Delete(tempDir, true);
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

                LogManager.Setup().LoadConfiguration(c => c.ForLogger().WriteTo(fileTarget));

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

                LogManager.Setup().LoadConfiguration(c => c.ForLogger().WriteTo(fileTarget));

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

                LogManager.Setup().LoadConfiguration(c => c.ForLogger().WriteTo(fileTarget));
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
            var tempDir = Path.Combine(Path.GetTempPath(), "nlog_" + Guid.NewGuid().ToString());
            var logFile = Path.Combine(tempDir, "log.log");

            try
            {
                LogManager.Setup().LoadConfigurationFromXml($@"
<nlog throwExceptions='true'>
    <targets>
      <target name='file1' encoding='UTF-8' type='File'  deleteOldFileOnStartup='true' fileName='{logFile}' />
    </targets>
    <rules>
      <logger name='*' minlevel='Trace' writeTo='file1' />
    </rules>
</nlog>");

                Assert.False(File.Exists(logFile));

                var logger = LogManager.GetCurrentClassLogger();
                logger.Trace("running test");

                Assert.NotNull(LogManager.Configuration);

                LogManager.Configuration = null;    // Flush
            }
            finally
            {
                if (File.Exists(logFile))
                    File.Delete(logFile);
                if (Directory.Exists(tempDir))
                    Directory.Delete(tempDir, true);
            }
        }

        [Fact]
        public void ArchiveFileOnStartTests()
        {
            var logFile = Path.GetTempFileName() + ".txt";
            var tempArchiveFolder = Path.Combine(Path.GetTempPath(), "nlog_" + Guid.NewGuid().ToString(), "Archive");
            var archiveExtension = "txt";

            try
            {
                // Configure first time with ArchiveOldFileOnStartup = false.
                var fileTarget = new FileTarget
                {
                    ArchiveOldFileOnStartup = false,
                    ArchiveSuffixFormat = "",
                    FileName = SimpleLayout.Escape(logFile),
                    LineEnding = LineEndingMode.LF,
                    Layout = "${level} ${message}"
                };

                LogManager.Setup().LoadConfiguration(c => c.ForLogger().WriteTo(fileTarget));

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

                LogManager.Setup().LoadConfiguration(c => c.ForLogger().WriteTo(fileTarget));

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
                    FileName = SimpleLayout.Escape(logFile),
                    LineEnding = LineEndingMode.LF,
                    Layout = "${level} ${message}",
                    ArchiveOldFileOnStartup = true,
                    ArchiveFileName = archiveTempName,
                    ArchiveSuffixFormat = "",
                    MaxArchiveFiles = 1
                };

                LogManager.Setup().LoadConfiguration(c => c.ForLogger().WriteTo(fileTarget));
                logger.Debug("ddd");
                logger.Info("eee");
                logger.Warn("fff");

                LogManager.Configuration = null;    // Flush

                AssertFileContents(logFile, "Debug ddd\nInfo eee\nWarn fff\n", Encoding.UTF8);
                Assert.True(File.Exists(archiveTempName));

                Action<string, string, string, Encoding> assertFileContents = (f1, f2, content, encoding) => AssertFileContents(f1, content, encoding, false);

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
            var tempArchiveFolder = Path.Combine(Path.GetTempPath(), "nlog_" + Guid.NewGuid().ToString());
            var archiveTempName = Path.Combine(tempArchiveFolder, "archive_size_threshold.txt");
            FileTarget CreateTestTarget(long threshold)
            {
                return new FileTarget
                {
                    FileName = SimpleLayout.Escape(logFile),
                    LineEnding = LineEndingMode.LF,
                    Layout = "${level} ${message}",
                    ArchiveAboveSize = threshold,
                    ArchiveFileName = archiveTempName,
                    ArchiveSuffixFormat = "",
                    MaxArchiveFiles = 1
                };
            }
            try
            {
                // No archive on startup (ignoring threshold)
                LogManager.Setup().LoadConfiguration(c => c.ForLogger().WriteTo(CreateTestTarget(1000)));
                logger.Info("aaa");
                LogManager.Shutdown();
                AssertFileContents(logFile, "Info aaa\n", Encoding.UTF8);
                Assert.False(File.Exists(archiveTempName));

                // Archive on startup with small threshold -> Must be archived
                LogManager.Setup().LoadConfiguration(c => c.ForLogger().WriteTo(CreateTestTarget(3)));
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

        [Fact]
        public void ArchiveOldFileOnStartupAboveSizeWhenFileLocked()
        {
            var logFile = Path.GetTempFileName();
            var tempArchiveFolder = Path.Combine(Path.GetTempPath(), "nlog_" + Guid.NewGuid().ToString(), "Archive");
            var archiveTempName = Path.Combine(tempArchiveFolder, "archive_size_threshold.zip");

            FileTarget CreateTestTarget(long threshold)
            {
                return new FileTarget
                {
                    FileName = SimpleLayout.Escape(logFile),
                    LineEnding = LineEndingMode.LF,
                    Layout = "${level} ${message}",
                    ArchiveAboveSize = threshold,
                    ArchiveFileName = archiveTempName,
                    MaxArchiveFiles = 1
                };
            }

            try
            {
                // No archive on startup (ignoring threshold)
                LogManager.Setup().LoadConfiguration(c => c.ForLogger().WriteTo(CreateTestTarget(1000)));
                logger.Info("aaa");
                LogManager.Shutdown();
                AssertFileContents(logFile, "Info aaa\n", Encoding.UTF8);
                Assert.False(File.Exists(archiveTempName));

                NLog.LogManager.ThrowExceptions = false;
                LogManager.Setup().LoadConfiguration(c => c.ForLogger().WriteTo(CreateTestTarget(3)));

                using (var fileStream = new FileStream(logFile, FileMode.Open, FileAccess.Write, FileShare.None))
                {
                    // Archive on startup with small threshold -> Must be archived
                    logger.Info("ccc");
                    LogManager.Flush();
                    fileStream.Close();
                    AssertFileContents(logFile, "Info aaa\n", Encoding.UTF8);
                    Assert.False(File.Exists(archiveTempName));
                }
            }
            finally
            {
                NLog.LogManager.ThrowExceptions = true;
                if (File.Exists(logFile))
                    File.Delete(logFile);
                if (Directory.Exists(tempArchiveFolder))
                    Directory.Delete(tempArchiveFolder, true);
            }
        }

        [Fact]
        public void RetryFileOpenWhenFileLocked()
        {
            var logFile = Path.GetTempFileName();

            var fileTarget = new FileTarget("file")
            {
                FileName = SimpleLayout.Escape(logFile),
                LineEnding = LineEndingMode.LF,
                Layout = "${level} ${message}",
                KeepFileOpen = false,
            };

            LogManager.Setup().LoadConfiguration(c => c.ForLogger().WriteTo(fileTarget));

            try
            {
                var fileStream = new FileStream(logFile, FileMode.Open, FileAccess.Write, FileShare.None);
                var task = Task.Run(() => logger.Info("aaa"));
                Assert.False(task.Wait(TimeSpan.FromMilliseconds(50)));
                fileStream.Dispose();
                Assert.True(task.Wait(TimeSpan.FromSeconds(60)));

                AssertFileContents(logFile, "Info aaa\n", Encoding.UTF8);
            }
            finally
            {
                if (File.Exists(logFile))
                    File.Delete(logFile);
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

                LogManager.Setup().LoadConfiguration(c => c.ForLogger().WriteTo(fileTarget));

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
            var tempDir = Path.Combine(Path.GetTempPath(), "nlog_" + Guid.NewGuid().ToString());
            var logfile = Path.Combine(tempDir, "log.log");

            try
            {
                LogManager.ThrowExceptions = false;

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

                var logger = LogManager.GetLogger("A");
                logger.Info("a");

                Assert.Equal(createDirs, Directory.Exists(tempDir));
            }
            finally
            {
                LogManager.ThrowExceptions = true;

                if (File.Exists(logfile))
                    File.Delete(logfile);
                if (Directory.Exists(tempDir))
                    Directory.Delete(tempDir, true);
            }
        }

        [Fact]
        public void CreateDirsTest()
        {
            var tempDir = Path.Combine(Path.GetTempPath(), "nlog_" + Guid.NewGuid().ToString());
            var logFile = Path.Combine(tempDir, "file.txt");
            try
            {
                var fileTarget = new FileTarget
                {
                    FileName = logFile,
                    LineEnding = LineEndingMode.LF,
                    Layout = "${level} ${message}"
                };

                LogManager.Setup().LoadConfiguration(c => c.ForLogger().WriteTo(fileTarget));

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
                if (Directory.Exists(tempDir))
                    Directory.Delete(tempDir, true);
            }
        }

        [Theory]
        [InlineData(true, true, 0)]
        [InlineData(false, true, 0)]
        [InlineData(false, true, 1)]
        [InlineData(true, false, 0)]
        [InlineData(false, false, 0)]
        [InlineData(false, false, 1)]
        public void AutoFlushTest(bool autoFlush, bool keepFileOpen, int autoFlushTimeout)
        {
            var tempDir = Path.Combine(Path.GetTempPath(), "nlog_" + Guid.NewGuid().ToString());
            var logFile = Path.Combine(tempDir, "file.txt");
            try
            {
                var fileTarget = new FileTarget
                {
                    FileName = logFile,
                    LineEnding = LineEndingMode.LF,
                    Layout = "${level} ${message}",
                    KeepFileOpen = keepFileOpen,
                    OpenFileFlushTimeout = autoFlushTimeout,
                };
                if (!autoFlush)
                    fileTarget.AutoFlush = autoFlush;   // Also ensures default-value is valid

                LogManager.Setup().LoadConfiguration(c => c.ForLogger().WriteTo(fileTarget));

                logger.Debug("aaa");
                logger.Info("bbb");
                logger.Warn("ccc");

                if (autoFlush || !keepFileOpen) 
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
                if (Directory.Exists(tempDir))
                    Directory.Delete(tempDir, true);
            }
        }

        [Fact]
        public void SequentialArchiveTest()
        {
            var tempDir = Path.Combine(Path.GetTempPath(), "nlog_" + Guid.NewGuid().ToString());
            var logFile = Path.Combine(tempDir, "file.txt");
            try
            {
                string archiveFolder = Path.Combine(tempDir, "archive");
                var fileTarget = new FileTarget
                {
                    FileName = logFile,
                    ArchiveFileName = Path.Combine(archiveFolder, "{##}.txt"),
                    ArchiveAboveSize = 100,
                    LineEnding = LineEndingMode.LF,
                    Layout = "${message}",
                    MaxArchiveFiles = 3,
                };

                LogManager.Setup().LoadConfiguration(c => c.ForLogger().WriteTo(fileTarget));

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
                    Path.Combine(archiveFolder, "01.txt"),
                    StringRepeat(times, "bbb\n"),
                    Encoding.UTF8);

                AssertFileContents(
                    Path.Combine(archiveFolder, "02.txt"),
                    StringRepeat(times, "ccc\n"),
                    Encoding.UTF8);

                AssertFileContents(
                    Path.Combine(archiveFolder, "03.txt"),
                    StringRepeat(times, "ddd\n"),
                    Encoding.UTF8);

                //0000 should not exists because of MaxArchiveFiles=3
                Assert.False(File.Exists(Path.Combine(archiveFolder, "00.txt")));
                Assert.False(File.Exists(Path.Combine(archiveFolder, "04.txt")));
            }
            finally
            {
                if (File.Exists(logFile))
                    File.Delete(logFile);
                if (Directory.Exists(tempDir))
                    Directory.Delete(tempDir, true);
            }
        }

        [Fact]
        public void SequentialArchiveTest_MaxArchiveFiles_NoLimit()
        {
            var tempDir = Path.Combine(Path.GetTempPath(), "nlog_" + Guid.NewGuid().ToString());
            var logFile = Path.Combine(tempDir, "file.txt");
            try
            {
                string archiveFolder = Path.Combine(tempDir, "archive");
                var fileTarget = new FileTarget
                {
                    FileName = logFile,
                    ArchiveFileName = Path.Combine(archiveFolder, "{##}.txt"),
                    ArchiveAboveSize = 100,
                    LineEnding = LineEndingMode.LF,
                    Layout = "${message}",
                };

                LogManager.Setup().LoadConfiguration(c => c.ForLogger().WriteTo(fileTarget));

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
                   Path.Combine(archiveFolder, "00.txt"),
                   StringRepeat(25, "aaa\n"),
                   Encoding.UTF8);

                AssertFileContents(
                    Path.Combine(archiveFolder, "01.txt"),
                    StringRepeat(25, "bbb\n"),
                    Encoding.UTF8);

                AssertFileContents(
                    Path.Combine(archiveFolder, "02.txt"),
                    StringRepeat(25, "ccc\n"),
                    Encoding.UTF8);

                AssertFileContents(
                    Path.Combine(archiveFolder, "03.txt"),
                    StringRepeat(25, "ddd\n"),
                    Encoding.UTF8);

                Assert.False(File.Exists(Path.Combine(archiveFolder, "04.txt")));
            }
            finally
            {
                if (File.Exists(logFile))
                    File.Delete(logFile);
                if (Directory.Exists(tempDir))
                    Directory.Delete(tempDir, true);
            }
        }

        [Fact]
        public void ArchiveAboveSize_AppendSameArchiveDate()
        {
            var tempDir = Path.Combine(Path.GetTempPath(), "nlog_" + Guid.NewGuid().ToString());
            var logFile = Path.Combine(tempDir, "file.txt");
            try
            {
                var archiveDateFormat = "yyyyMMdd";

                string archiveFolder = Path.Combine(tempDir, "archive");
                var fileTarget = new FileTarget
                {
                    FileName = logFile,
                    ArchiveFileName = Path.Combine(archiveFolder, ".txt"),
                    ArchiveSuffixFormat = "{1:" + archiveDateFormat + "}",
                    ArchiveAboveSize = 100,
                    LineEnding = LineEndingMode.LF,
                    Layout = "${message}",
                };

                LogManager.Setup().LoadConfiguration(c => c.ForLogger().WriteTo(fileTarget));

                //e.g. 20150804
                var archiveFileName = DateTime.Now.ToString(archiveDateFormat);

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
                if (Directory.Exists(tempDir))
                    Directory.Delete(tempDir, true);
            }
        }

        [Fact]
        public void DeleteArchiveFilesByDate()
        {
            const int maxArchiveFiles = 3;

            var tempDir = Path.Combine(Path.GetTempPath(), "nlog_" + Guid.NewGuid().ToString());
            var logFile = Path.Combine(tempDir, "file.txt");
            try
            {
                string archiveFolder = Path.Combine(tempDir, "archive");
                var fileTarget = new FileTarget
                {
                    FileName = logFile,
                    ArchiveFileName = Path.Combine(archiveFolder, "{#}.txt"),
                    ArchiveAboveSize = 50,
                    LineEnding = LineEndingMode.LF,
#pragma warning disable CS0618 // Type or member is obsolete
                    ArchiveDateFormat = "yyyyMMddHHmmssfff", //make sure the milliseconds are set in the filename
#pragma warning restore CS0618 // Type or member is obsolete
                    Layout = "${message}",
                    MaxArchiveFiles = maxArchiveFiles
                };

                LogManager.Setup().LoadConfiguration(c => c.ForLogger().WriteTo(fileTarget));

                //writing 20 times 10 bytes (9 char + linefeed) will result in 3 archive files and 1 current file
                for (var i = 0; i < 20; ++i)
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


                LogManager.Setup().LoadConfiguration(c => c.ForLogger().WriteTo(fileTarget));
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
                if (Directory.Exists(tempDir))
                    Directory.Delete(tempDir, true);
            }
        }

        [Fact]
        public void DeleteArchiveFilesByDateWithDateName()
        {
            const int maxArchiveFiles = 3;
            LogManager.ThrowExceptions = true;
            var tempDir = Path.Combine(Path.GetTempPath(), "nlog_" + Guid.NewGuid().ToString());
            try
            {
                var logFile = Path.Combine(tempDir, "${date:format=yyyyMMddHHmmssfff}.txt");
                var fileTarget = new FileTarget
                {
                    FileName = logFile,
                    ArchiveFileName = Path.Combine(tempDir, "{#}.txt"),
                    ArchiveEvery = FileArchivePeriod.Year,
                    LineEnding = LineEndingMode.LF,
#pragma warning disable CS0618 // Type or member is obsolete
                    ArchiveDateFormat = "yyyyMMddHHmmssfff", //make sure the milliseconds are set in the filename
#pragma warning restore CS0618 // Type or member is obsolete
                    Layout = "${message}",
                    MaxArchiveFiles = maxArchiveFiles,
                };

                LogManager.Setup().LoadConfiguration(c => c.ForLogger().WriteTo(fileTarget));
                for (var i = 0; i < 4; ++i)
                {
                    logger.Debug("123456789");
                    //build in a  sleep to make sure the current time is reflected in the filename
                    Thread.Sleep(50);
                }
                //Setting the Configuration to [null] will result in a 'Dump' of the current log entries
                LogManager.Configuration = null;    // Flush

                var files = Directory.GetFiles(tempDir).OrderBy(s => s);
                //we expect 3 archive files, plus one current file
                Assert.Equal(maxArchiveFiles + 1, files.Count());


                LogManager.Setup().LoadConfiguration(c => c.ForLogger().WriteTo(fileTarget));
                //writing 50ms later will trigger the cleanup of old archived files
                //as stated by the MaxArchiveFiles property, but will only delete the oldest file
                Thread.Sleep(50);
                logger.Debug("123456789");
                LogManager.Configuration = null;    // Flush

                var files2 = Directory.GetFiles(tempDir).OrderBy(s => s);
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
                if (Directory.Exists(tempDir))
                    Directory.Delete(tempDir, true);
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
                    from keepFileOpen in booleanValues
                    from maxArchiveDays in booleanValues
                    select new object[] { timeKind, keepFileOpen, maxArchiveDays };
            }
        }

        [Theory]
        [MemberData(nameof(DateArchive_UsesDateFromCurrentTimeSource_TestParameters))]
        public void DateArchive_UsesDateFromCurrentTimeSource(DateTimeKind timeKind, bool keepFileOpen, bool maxArchiveDays)
        {
#if !NETFRAMEWORK || MONO
            if (IsLinux() && !maxArchiveDays)
            {
                Console.WriteLine("[SKIP] FileTargetTests.DateArchive_UsesDateFromCurrentTimeSource because SetLastWriteTime is not working on Travis");
                return;
            }
#endif

            const string archiveDateFormat = "yyyyMMdd";
            const int maxArchiveFiles = 3;

            var tempDir = Path.Combine(Path.GetTempPath(), "nlog_" + Guid.NewGuid().ToString());
            var logFile = Path.Combine(tempDir, "file.txt");
            var defaultTimeSource = TimeSource.Current;
            try
            {
                var timeSource = new TimeSourceTests.ShiftedTimeSource(timeKind);

                TimeSource.Current = timeSource;

                string archiveFolder = Path.Combine(tempDir, "archive");
                string archiveFileNameTemplate = Path.Combine(archiveFolder, "{#}.txt");
                var fileTarget = new FileTarget
                {
                    FileName = logFile,
                    ArchiveFileName = archiveFileNameTemplate,
                    LineEnding = LineEndingMode.LF,
                    ArchiveEvery = FileArchivePeriod.Day,
#pragma warning disable CS0618 // Type or member is obsolete
                    ArchiveDateFormat = archiveDateFormat,
#pragma warning restore CS0618 // Type or member is obsolete
                    Layout = "${date:format=O}|${message}",
                    KeepFileOpen = keepFileOpen,
                    Header = "header",
                };
                if (maxArchiveDays)
                    fileTarget.MaxArchiveDays = maxArchiveFiles;
                else
                    fileTarget.MaxArchiveFiles = maxArchiveFiles;

                LogManager.Setup().LoadConfiguration(c => c.ForLogger().WriteTo(fileTarget));

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
                        // Simulate that previous file write began in previous day and ended on current day.
                        try
                        {
                            File.SetLastWriteTime(logFile, timeSource.SystemTime);
                        }
                        catch { }
                    }

                    var eventInfo = new LogEventInfo(LogLevel.Debug, logger.Name, "123456789");
                    logger.Log(eventInfo);

                    var dayIsChanged = eventInfo.TimeStamp.Date != previousWriteTime.Date;
                    // ensure new archive is created only when the day part of time is changed
                    var archiveFileName = archiveFileNameTemplate.Replace("{#}", previousWriteTime.ToString(archiveDateFormat) + "_00");
                    var archiveExists = File.Exists(archiveFileName);
                    if (dayIsChanged)
                        Assert.True(archiveExists,
                            $"new archive should be created when the day part of {timeKind} time is changed");
                    else
                        Assert.False(archiveExists,
                            $"new archive should not be create when day part of {timeKind} time is unchanged");

                    previousWriteTime = eventInfo.TimeStamp;
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

                LogManager.Setup().LoadConfiguration(c => c.ForLogger().WriteTo(fileTarget));
                //writing one line on a new day will trigger the cleanup of old archived files
                //as stated by the MaxArchiveFiles property, but will only delete the oldest file
                timeSource.AddToLocalTime(TimeSpan.FromDays(3));
                logger.Debug("1234567890");

                LogManager.Configuration = null;    // Flush

                if (!maxArchiveDays)
                {
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
            }
            finally
            {
                TimeSource.Current = defaultTimeSource; // restore default time source
                if (Directory.Exists(tempDir))
                    Directory.Delete(tempDir, true);
            }
        }

        public static IEnumerable<object[]> DateArchive_ArchiveOnceOnly_TestParameters
        {
            get
            {
                var booleanValues = new[] { true, false };
                return
                    from keepFileOpen in booleanValues
                    from includeSequenceInArchive in booleanValues
                    select new object[] { keepFileOpen, includeSequenceInArchive };
            }
        }

        [Theory]
        [MemberData(nameof(DateArchive_ArchiveOnceOnly_TestParameters))]
        public void DateArchive_ArchiveOnceOnly(bool keepFileOpen, bool includeSequenceInArchive)
        {
            var tempDir = Path.Combine(Path.GetTempPath(), "nlog_" + Guid.NewGuid().ToString());
            var logFile = Path.Combine(tempDir, "file.txt");

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

                string archiveFolder = Path.Combine(tempDir, "archive");
                var fileTarget = new FileTarget
                {
                    FileName = logFile,
                    ArchiveFileName = Path.Combine(archiveFolder, "{#}.txt"),
                    LineEnding = LineEndingMode.LF,
                    ArchiveEvery = FileArchivePeriod.Day,
#pragma warning disable CS0618 // Type or member is obsolete
                    ArchiveDateFormat = "yyyyMMdd",
#pragma warning restore CS0618 // Type or member is obsolete
                    Layout = "${message}",
                    KeepFileOpen = keepFileOpen,
                };
                if (!includeSequenceInArchive)
                    fileTarget.ArchiveSuffixFormat = @"_{1:yyyyMMdd}";

                LogManager.Setup().LoadConfiguration(c => c.ForLogger().WriteTo(fileTarget));

                logger.Debug("123456789");
                LogManager.Configuration = null;
                LogManager.Setup().LoadConfiguration(c => c.ForLogger().WriteTo(fileTarget));

                timeSource.AddToLocalTime(TimeSpan.FromDays(1));
                File.SetCreationTimeUtc(logFile, timeSource.Time.ToUniversalTime());
                timeSource.AddToLocalTime(TimeSpan.FromDays(1));

                // This should archive the log before logging.
                logger.Debug("123456789");

                LogManager.Configuration = null;
                LogManager.Setup().LoadConfiguration(c => c.ForLogger().WriteTo(fileTarget));
                timeSource.AddToSystemTime(TimeSpan.FromDays(2));   // Archive only once

                // This must not archive.
                logger.Debug("123456789");

                LogManager.Configuration = null;    // Flush

                Assert.Single(Directory.GetFiles(archiveFolder));
                var prevLogFile = Directory.GetFiles(archiveFolder)[0];
                AssertFileContents(prevLogFile, StringRepeat(1, "123456789\n"), Encoding.UTF8);

                var currentLogFile = Directory.GetFiles(tempDir)[0];
                AssertFileContents(currentLogFile, StringRepeat(2, "123456789\n"), Encoding.UTF8);
            }
            finally
            {
                TimeSource.Current = defaultTimeSource; // restore default time source

                if (Directory.Exists(tempDir))
                    Directory.Delete(tempDir, true);
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
                    from includeSequenceInArchive in booleanValues
                    select new object[] { timeKind, archivePeriod, includeSequenceInArchive };
            }
        }

        [Theory]
        [MemberData(nameof(DateArchive_SkipPeriod_TestParameters))]
        public void DateArchive_SkipPeriod(DateTimeKind timeKind, FileArchivePeriod archivePeriod, bool includeSequenceInArchive)
        {
            var tempDir = Path.Combine(Path.GetTempPath(), "nlog_" + Guid.NewGuid().ToString());
            var logFile = Path.Combine(tempDir, "file.txt");
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
                    ArchiveFileName = Path.Combine(tempDir, "archive", "{#}.txt"),
                    LineEnding = LineEndingMode.LF,
                    ArchiveEvery = archivePeriod,
#pragma warning disable CS0618 // Type or member is obsolete
                    ArchiveDateFormat = "yyyyMMddHHmm",
#pragma warning restore CS0618 // Type or member is obsolete
                    Layout = "${date:format=O}|${message}",
                };
                if (!includeSequenceInArchive)
                    fileTarget.ArchiveSuffixFormat = @"_{1:yyyyMMddHHmm}";

#pragma warning disable CS0618 // Type or member is obsolete
                string archiveDateFormat = fileTarget.ArchiveDateFormat;
#pragma warning restore CS0618 // Type or member is obsolete
                LogManager.Setup().LoadConfiguration(c => c.ForLogger().WriteTo(fileTarget));

                logger.Debug("1234567890");
                timeSource.AddToLocalTime(TimeSpan.FromMinutes(1));
                logger.Debug("1234567890");
                // The archive file name must be based on the last time the file was written.
                string archiveFileName =
                    $"{timeSource.Time.ToString(archiveDateFormat) + (includeSequenceInArchive ? "_00" : string.Empty)}.txt";
                // Effectively update the file's last-write-time.
                timeSource.AddToSystemTime(TimeSpan.FromMinutes(1));

                timeSource.AddToLocalTime(TimeSpan.FromDays(2));
                logger.Debug("1234567890");

                LogManager.Configuration = null;    // Flush

                string archivePath = Path.Combine(tempDir, "archive");
                var archiveFiles = Directory.GetFiles(archivePath);
                Assert.Single(archiveFiles);
                Assert.Equal(archiveFileName, Path.GetFileName(archiveFiles[0]));
            }
            finally
            {
                TimeSource.Current = defaultTimeSource; // restore default time source
                if (Directory.Exists(tempDir))
                    Directory.Delete(tempDir, true);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void DateArchive_AllLoggersTransferToCurrentLogFile(bool includeSequenceInArchive)
        {
            var tempDir = Path.Combine(Path.GetTempPath(), "nlog_" + Guid.NewGuid().ToString());
            var logfile = Path.Combine(tempDir, "file.txt");
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

                var config = new LoggingConfiguration();

                string archiveFolder = Path.Combine(tempDir, "archive");
                var fileTarget1 = new FileTarget
                {
                    FileName = logfile,
                    ArchiveFileName = Path.Combine(archiveFolder, "{#}.txt"),
                    LineEnding = LineEndingMode.LF,
                    ArchiveEvery = FileArchivePeriod.Day,
#pragma warning disable CS0618 // Type or member is obsolete
                    ArchiveDateFormat = "yyyyMMdd",
#pragma warning restore CS0618 // Type or member is obsolete
                    Layout = "${message}",
                    KeepFileOpen = false,   // KeepFileOpen = false is required when 2 file-targets to the same file
                };
                if (!includeSequenceInArchive)
                    fileTarget1.ArchiveSuffixFormat = @"_{1:yyyyMMdd}";

                var logger1Rule = new LoggingRule("logger1", LogLevel.Debug, fileTarget1);
                config.LoggingRules.Add(logger1Rule);

                var fileTarget2 = new FileTarget
                {
                    FileName = logfile,
                    ArchiveFileName = Path.Combine(archiveFolder, "{#}.txt"),
                    LineEnding = LineEndingMode.LF,
                    ArchiveEvery = FileArchivePeriod.Day,
#pragma warning disable CS0618 // Type or member is obsolete
                    ArchiveDateFormat = "yyyyMMdd",
#pragma warning restore CS0618 // Type or member is obsolete
                    Layout = "${message}",
                    KeepFileOpen = false,   // KeepFileOpen = false is required when 2 file-targets to the same file
                };
                if (!includeSequenceInArchive)
                    fileTarget2.ArchiveSuffixFormat = @"_{1:yyyyMMdd}";

                var logger2Rule = new LoggingRule("logger2", LogLevel.Debug, fileTarget2);
                config.LoggingRules.Add(logger2Rule);

                LogManager.Configuration = config;

                var logger1 = LogManager.GetLogger("logger1");
                var logger2 = LogManager.GetLogger("logger2");

                logger1.Debug("123456789");
                logger2.Debug("123456789");

                timeSource.AddToLocalTime(TimeSpan.FromDays(1));
                Thread.Sleep(50);

                // This should archive the log before logging.
                logger1.Debug("123456789");

                timeSource.AddToSystemTime(TimeSpan.FromDays(1));   // Archive only once

                logger2.Debug("123456789");

                LogManager.Configuration = null;    // Flush

                var files = Directory.GetFiles(archiveFolder);
                Assert.Single(files);

                string prevLogFile = Directory.GetFiles(archiveFolder)[0];
                AssertFileContents(prevLogFile, StringRepeat(2, "123456789\n"), Encoding.UTF8);

                string currentLogFile = Directory.GetFiles(tempDir)[0];
                AssertFileContents(currentLogFile, StringRepeat(2, "123456789\n"), Encoding.UTF8);
            }
            finally
            {
                TimeSource.Current = defaultTimeSource; // restore default time source

                if (Directory.Exists(tempDir))
                    Directory.Delete(tempDir, true);
            }
        }

        [Fact]
        public void DeleteArchiveFilesByDate_MaxArchiveFiles_NoLimit()
        {
            var tempDir = Path.Combine(Path.GetTempPath(), "nlog_" + Guid.NewGuid().ToString());
            var logFile = Path.Combine(tempDir, "file.txt");
            try
            {
                string archiveFolder = Path.Combine(tempDir, "archive");
                var fileTarget = new FileTarget
                {
                    FileName = logFile,
                    ArchiveFileName = Path.Combine(archiveFolder, "{#}.txt"),
                    ArchiveAboveSize = 50,
                    LineEnding = LineEndingMode.LF,
#pragma warning disable CS0618 // Type or member is obsolete
                    ArchiveDateFormat = "yyyyMMddHHmmssfff", //make sure the milliseconds are set in the filename
#pragma warning restore CS0618 // Type or member is obsolete
                    Layout = "${message}",
                };

                LogManager.Setup().LoadConfiguration(c => c.ForLogger().WriteTo(fileTarget));
                //writing 19 times 10 bytes (9 char + linefeed) will result in 3 archive files and 1 current file
                for (var i = 0; i < 20; ++i)
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

                LogManager.Setup().LoadConfiguration(c => c.ForLogger().WriteTo(fileTarget));
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

                if (Directory.Exists(tempDir))
                {
                    Directory.Delete(tempDir, true);
                }
            }
        }

        [Fact]
        public void DeleteArchiveFilesByDate_AlteredMaxArchive()
        {
            var tempDir = Path.Combine(Path.GetTempPath(), "nlog_" + Guid.NewGuid().ToString());
            var logFile = Path.Combine(tempDir, "file.txt");
            try
            {
                string archiveFolder = Path.Combine(tempDir, "archive");
                var fileTarget = new FileTarget
                {
                    FileName = logFile,
                    ArchiveFileName = Path.Combine(archiveFolder, "{#}.txt"),
                    ArchiveAboveSize = 50,
                    LineEnding = LineEndingMode.LF,
#pragma warning disable CS0618 // Type or member is obsolete
                    ArchiveDateFormat = "yyyyMMddHHmmssfff", //make sure the milliseconds are set in the filename
#pragma warning restore CS0618 // Type or member is obsolete
                    Layout = "${message}",
                    MaxArchiveFiles = 5
                };

                LogManager.Setup().LoadConfiguration(c => c.ForLogger().WriteTo(fileTarget));
                //writing 29 times 10 bytes (9 char + linefeed) will result in 3 archive files and 1 current file
                for (var i = 0; i < 30; ++i)
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
                LogManager.Setup().LoadConfiguration(c => c.ForLogger().WriteTo(fileTarget));
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
                if (Directory.Exists(tempDir))
                    Directory.Delete(tempDir, true);
            }
        }

        [Fact]
        public void RepeatingHeaderTest()
        {
            var tempDir = Path.Combine(Path.GetTempPath(), "nlog_" + Guid.NewGuid().ToString());
            var logFile = Path.Combine(tempDir, "file.txt");
            try
            {
                const string header = "Headerline";

                string archiveFolder = Path.Combine(tempDir, "archive");
                var fileTarget = new FileTarget
                {
                    FileName = logFile,
                    ArchiveFileName = Path.Combine(archiveFolder, "{##}.txt"),
                    ArchiveAboveSize = 51,
                    LineEnding = LineEndingMode.LF,
                    Layout = "${message}",
                    Header = header,
                    MaxArchiveFiles = 2,
                };

                LogManager.Setup().LoadConfiguration(c => c.ForLogger().WriteTo(fileTarget));

                // Writing 16 times 10 bytes = 160 bytes = 3 files
                for (var i = 0; i < 16; ++i)
                {
                    logger.Debug("123456789");
                }

                LogManager.Configuration = null;    // Flush

                AssertFileContentsStartsWith(logFile, header, Encoding.UTF8);

                AssertFileContentsStartsWith(Path.Combine(archiveFolder, "02.txt"), header, Encoding.UTF8);

                AssertFileContentsStartsWith(Path.Combine(archiveFolder, "01.txt"), header, Encoding.UTF8);

                Assert.False(File.Exists(Path.Combine(archiveFolder, "00.txt"))); // MaxArchiveFiles = 2 (Removes the first file)
            }
            finally
            {
                if (File.Exists(logFile))
                    File.Delete(logFile);
                if (Directory.Exists(tempDir))
                    Directory.Delete(tempDir, true);
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void RepeatingFooterTest(bool writeFooterOnArchivingOnly)
        {
            var tempDir = Path.Combine(Path.GetTempPath(), "nlog_" + Guid.NewGuid().ToString());
            var logFile = Path.Combine(tempDir, "file.txt");
            try
            {
                const string footer = "Footerline";

                string archiveFolder = Path.Combine(tempDir, "archive");
                var fileTarget = new FileTarget
                {
                    FileName = logFile,
                    ArchiveFileName = Path.Combine(archiveFolder, "{##}.txt"),
                    ArchiveAboveSize = 51,
                    LineEnding = LineEndingMode.LF,
                    Layout = "${message}",
                    Footer = footer,
                    MaxArchiveFiles = 2,
                    WriteFooterOnArchivingOnly = writeFooterOnArchivingOnly
                };

                LogManager.Setup().LoadConfiguration(c => c.ForLogger().WriteTo(fileTarget));

                // Writing 16 times 10 bytes = 160 bytes = 3 files
                for (var i = 0; i < 16; ++i)
                {
                    logger.Debug("123456789");
                }

                LogManager.Configuration = null;    // Flush

                string expectedEnding = footer + fileTarget.LineEnding.NewLineCharacters;
                if (writeFooterOnArchivingOnly)
                    Assert.False(File.ReadAllText(logFile).EndsWith(expectedEnding), "Footer was unexpectedly written to log file.");
                else
                    AssertFileContentsEndsWith(logFile, expectedEnding, Encoding.UTF8);
                AssertFileContentsEndsWith(Path.Combine(archiveFolder, "02.txt"), expectedEnding, Encoding.UTF8);
                AssertFileContentsEndsWith(Path.Combine(archiveFolder, "01.txt"), expectedEnding, Encoding.UTF8);
                Assert.False(File.Exists(Path.Combine(archiveFolder, "00.txt"))); // MaxArchiveFiles = 2 (Removes the first file)
            }
            finally
            {
                LogManager.Configuration = null;
                if (File.Exists(logFile))
                    File.Delete(logFile);
                if (Directory.Exists(tempDir))
                    Directory.Delete(tempDir, true);
            }
        }

        [Theory]
        [InlineData(false, false, true)]
        [InlineData(true, false, true)]
        [InlineData(false, false, false)]
        [InlineData(true, false, false)]
        [InlineData(false, true, true)]
        [InlineData(true, true, true)]
        [InlineData(false, true, false)]
        [InlineData(true, true, false)]
        public void WriteHeaderOnStartupTest(bool writeHeaderWhenInitialFileNotEmpty, bool writeBom, bool keepFileOpen)
        {
            var logFile = Path.GetTempFileName() + ".txt";
            try
            {
                const string header = "Headerline";

                // Configure first time
                var fileTarget = new FileTarget
                {
                    FileName = SimpleLayout.Escape(logFile),
                    LineEnding = LineEndingMode.LF,
                    Layout = "${message}",
                    Header = header,
                    KeepFileOpen = keepFileOpen,
                    WriteBom = writeBom,
                    WriteHeaderWhenInitialFileNotEmpty = true
                };

                LogManager.Setup().LoadConfiguration(c => c.ForLogger().WriteTo(fileTarget));

                logger.Debug("aaa");
                logger.Info("bbb");
                logger.Warn("ccc");

                LogManager.Configuration = null;

                string headerPart = header + LineEndingMode.LF.NewLineCharacters;
                string logPart = "aaa\nbbb\nccc\n";
                AssertFileContents(logFile, headerPart + logPart, Encoding.UTF8, addBom: writeBom);

                // Configure second time
                fileTarget = new FileTarget
                {
                    FileName = SimpleLayout.Escape(logFile),
                    LineEnding = LineEndingMode.LF,
                    Layout = "${message}",
                    Header = header,
                    KeepFileOpen = keepFileOpen,
                    WriteBom = writeBom,
                    WriteHeaderWhenInitialFileNotEmpty = writeHeaderWhenInitialFileNotEmpty
                };

                LogManager.Setup().LoadConfiguration(c => c.ForLogger().WriteTo(fileTarget));

                logger.Debug("aaa");
                logger.Info("bbb");
                logger.Warn("ccc");

                LogManager.Configuration = null;    // Flush

                if (writeHeaderWhenInitialFileNotEmpty)
                    AssertFileContents(logFile, headerPart + logPart + headerPart + logPart, Encoding.UTF8, addBom: writeBom);
                else
                    AssertFileContents(logFile, headerPart + logPart + logPart, Encoding.UTF8, addBom: writeBom);
            }
            finally
            {
                LogManager.Configuration = null;
                if (File.Exists(logFile))
                    File.Delete(logFile);
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void RollingArchiveTest_MaxArchiveFiles(bool specifyArchiveFileName)
        {
            var tempDir = Path.Combine(Path.GetTempPath(), "nlog_" + Guid.NewGuid().ToString());
            var logFile = Path.Combine(tempDir, "file.txt");
            var archiveExtension = "txt";

            try
            {
                var fileTarget = new FileTarget
                {
                    FileName = logFile,
                    ArchiveAboveSize = 100,
                    LineEnding = LineEndingMode.LF,
                    Layout = "${message}",
                    MaxArchiveFiles = 3
                };

                if (specifyArchiveFileName)
                {
                    fileTarget.ArchiveFileName = Path.Combine(tempDir, "archive", "." + archiveExtension);
                    fileTarget.ArchiveSuffixFormat = "{0:00}";
                }

                LogManager.Setup().LoadConfiguration(c => c.ForLogger().WriteTo(fileTarget));

                // we emit 5 * 25 * (3 x aaa + \n) bytes
                // so that we should get a full file + 3 archives
                Generate100BytesLog('a');
                Generate100BytesLog('b');
                Generate100BytesLog('c');
                Generate100BytesLog('d');
                Generate100BytesLog('e');

                LogManager.Configuration = null;    // Flush

                Action<string, string, Encoding> assertFileContents = (f1, content, encoding) => AssertFileContents(f1, content, encoding, false);

                string archiveFileNameFormat = specifyArchiveFileName
                    ? Path.Combine("archive", "0{0}." + archiveExtension)
                    : "file_0{0}." + archiveExtension;

                var currentFile = specifyArchiveFileName ? logFile : Path.Combine(tempDir, string.Format(archiveFileNameFormat, 4));

                var times = 25;
                AssertFileContents(currentFile,
                    StringRepeat(times, "eee\n"),
                    Encoding.UTF8);

                if (!specifyArchiveFileName)
                {
                    Assert.False(File.Exists(logFile));
                    Assert.False(File.Exists(Path.Combine(tempDir, string.Format(archiveFileNameFormat, 1))));
                }
                else
                {
                    Assert.False(File.Exists(Path.Combine(tempDir, string.Format(archiveFileNameFormat, 0))));
                    Assert.False(File.Exists(Path.Combine(tempDir, string.Format(archiveFileNameFormat, 4))));

                    assertFileContents(
                        Path.Combine(tempDir, string.Format(archiveFileNameFormat, 1)),
                        StringRepeat(times, "bbb\n"),
                        Encoding.UTF8);
                }

                assertFileContents(
                    Path.Combine(tempDir, string.Format(archiveFileNameFormat, 2)),
                    StringRepeat(times, "ccc\n"),
                    Encoding.UTF8);

                assertFileContents(
                    Path.Combine(tempDir, string.Format(archiveFileNameFormat, 3)),
                    StringRepeat(times, "ddd\n"),
                    Encoding.UTF8);
            }
            finally
            {
                if (File.Exists(logFile))
                    File.Delete(logFile);
                if (Directory.Exists(tempDir))
                    Directory.Delete(tempDir, true);
            }
        }

        [InlineData("/")]
        [InlineData("\\")]
        [Theory]
        public void RollingArchiveTest_MaxArchiveFiles_NoLimit(string slash)
        {
            var tempDir = Path.Combine(Path.GetTempPath(), "nlog_" + Guid.NewGuid().ToString());
            var logFile = Path.Combine(tempDir, "file.txt");
            try
            {
                var fileTarget = new FileTarget
                {
                    FileName = logFile,
                    ArchiveFileName = Path.Combine(tempDir, "archive" + slash + "log_{##}.txt"),
                    ArchiveAboveSize = 100,
                    LineEnding = LineEndingMode.LF,
                    Layout = "${message}",
                };

                LogManager.Setup().LoadConfiguration(c => c.ForLogger().WriteTo(fileTarget));

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
                    Path.Combine(tempDir, "archive" + slash + "log_00.txt"),
                    StringRepeat(times, "aaa\n"),
                    Encoding.UTF8);

                AssertFileContents(
                    Path.Combine(tempDir, "archive" + slash + "log_01.txt"),
                    StringRepeat(times, "bbb\n"),
                    Encoding.UTF8);

                AssertFileContents(
                    Path.Combine(tempDir, "archive" + slash + "log_02.txt"),
                    StringRepeat(times, "ccc\n"),
                    Encoding.UTF8);

                AssertFileContents(
                    Path.Combine(tempDir, "archive" + slash + "log_03.txt"),
                    StringRepeat(times, "ddd\n"),
                    Encoding.UTF8);
            }
            finally
            {
                if (File.Exists(logFile))
                {
                    File.Delete(logFile);
                }

                if (Directory.Exists(tempDir))
                {
                    Directory.Delete(tempDir, true);
                }
            }
        }

        [Fact]
        public void MultiFileWrite()
        {
            var tempDir = Path.Combine(Path.GetTempPath(), "nlog_" + Guid.NewGuid().ToString());
            try
            {
                var fileTarget = new FileTarget
                {
                    FileName = Path.Combine(tempDir, "${level}.txt"),
                    LineEnding = LineEndingMode.LF,
                    Layout = "${message}"
                };

                LogManager.Setup().LoadConfiguration(c => c.ForLogger(LogLevel.Debug).WriteTo(fileTarget));

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

                Assert.False(File.Exists(Path.Combine(tempDir, "Trace.txt")));

                AssertFileContents(Path.Combine(tempDir, "Debug.txt"),
                    StringRepeat(times, "aaa\n"), Encoding.UTF8);

                AssertFileContents(Path.Combine(tempDir, "Info.txt"),
                    StringRepeat(times, "bbb\n"), Encoding.UTF8);

                AssertFileContents(Path.Combine(tempDir, "Warn.txt"),
                    StringRepeat(times, "ccc\n"), Encoding.UTF8);

                AssertFileContents(Path.Combine(tempDir, "Error.txt"),
                    StringRepeat(times, "ddd\n"), Encoding.UTF8);

                AssertFileContents(Path.Combine(tempDir, "Fatal.txt"),
                    StringRepeat(times, "eee\n"), Encoding.UTF8);
            }
            finally
            {
                if (Directory.Exists(tempDir))
                    Directory.Delete(tempDir, true);
            }
        }

        [Fact]
        public void BufferedMultiFileWrite()
        {
            var tempDir = Path.Combine(Path.GetTempPath(), "nlog_" + Guid.NewGuid().ToString());
            try
            {
                var fileTarget = new FileTarget
                {
                    FileName = Path.Combine(tempDir, "${level}.txt"),
                    LineEnding = LineEndingMode.LF,
                    Layout = "${message}"
                };

                LogManager.Setup().LoadConfiguration(c => c.ForLogger(LogLevel.Debug).WriteTo(new BufferingTargetWrapper(fileTarget, 10)));

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

                Assert.False(File.Exists(Path.Combine(tempDir, "Trace.txt")));

                AssertFileContents(Path.Combine(tempDir, "Debug.txt"),
                    StringRepeat(times, "aaa\n"), Encoding.UTF8);

                AssertFileContents(Path.Combine(tempDir, "Info.txt"),
                    StringRepeat(times, "bbb\n"), Encoding.UTF8);

                AssertFileContents(Path.Combine(tempDir, "Warn.txt"),
                    StringRepeat(times, "ccc\n"), Encoding.UTF8);

                AssertFileContents(Path.Combine(tempDir, "Error.txt"),
                    StringRepeat(times, "ddd\n"), Encoding.UTF8);

                AssertFileContents(Path.Combine(tempDir, "Fatal.txt"),
                    StringRepeat(times, "eee\n"), Encoding.UTF8);
            }
            finally
            {
                if (Directory.Exists(tempDir))
                    Directory.Delete(tempDir, true);
            }
        }

        [Fact]
        public void AsyncMultiFileWrite()
        {
            var tempDir = Path.Combine(Path.GetTempPath(), "nlog_" + Guid.NewGuid().ToString());
            try
            {
                var fileTarget = new FileTarget
                {
                    FileName = Path.Combine(tempDir, "${level}.txt"),
                    LineEnding = LineEndingMode.LF,
                    Layout = "${message} ${threadid}"
                };

                // this also checks that thread-volatile layouts
                // such as ${threadid} are properly cached and not recalculated
                // in logging threads.
                var threadID = Thread.CurrentThread.ManagedThreadId.ToString();

                LogManager.Setup().LoadConfiguration(c => c.ForLogger(LogLevel.Debug).WriteTo(fileTarget).WithAsync());

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

                Assert.False(File.Exists(Path.Combine(tempDir, "Trace.txt")));

                AssertFileContents(Path.Combine(tempDir, "Debug.txt"),
                    StringRepeat(times, "aaa " + threadID + "\n"), Encoding.UTF8);

                AssertFileContents(Path.Combine(tempDir, "Info.txt"),
                    StringRepeat(times, "bbb " + threadID + "\n"), Encoding.UTF8);

                AssertFileContents(Path.Combine(tempDir, "Warn.txt"),
                    StringRepeat(times, "ccc " + threadID + "\n"), Encoding.UTF8);

                AssertFileContents(Path.Combine(tempDir, "Error.txt"),
                    StringRepeat(times, "ddd " + threadID + "\n"), Encoding.UTF8);

                AssertFileContents(Path.Combine(tempDir, "Fatal.txt"),
                    StringRepeat(times, "eee " + threadID + "\n"), Encoding.UTF8);
            }
            finally
            {
                if (Directory.Exists(tempDir))
                    Directory.Delete(tempDir, true);
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
            FileTarget_ArchiveNumbering_DateAndSequenceTests(fileTxt: "file.txt", archiveFileName: Path.Combine("archive", "{#}.txt"));
        }

        [Fact]
        public void FileTarget_ArchiveNumbering_DateAndSequence_archive_same_as_log_name()
        {
            FileTarget_ArchiveNumbering_DateAndSequenceTests(fileTxt: "file${date:format=yyyy-MM-dd}.txt", archiveFileName: "file_{#}.txt");
        }

        private void FileTarget_ArchiveNumbering_DateAndSequenceTests(string fileTxt, string archiveFileName)
        {
            const string archiveDateFormat = "yyyy-MM-dd";
            const int archiveAboveSize = 100;

            var tempDir = ArchiveFileNameHelper.GenerateTempPath();
            Layout logFile = Path.Combine(tempDir, fileTxt);
            var logFileName = logFile.Render(LogEventInfo.CreateNullEvent());

            try
            {
                var fileTarget = new FileTarget
                {
                    FileName = logFile,
                    ArchiveFileName = Path.Combine(tempDir, archiveFileName),
#pragma warning disable CS0618 // Type or member is obsolete
                    ArchiveDateFormat = archiveDateFormat,
#pragma warning restore CS0618 // Type or member is obsolete
                    ArchiveAboveSize = archiveAboveSize,
                    LineEnding = LineEndingMode.LF,
                    Layout = "${message}",
                    MaxArchiveFiles = 3,
                    ArchiveEvery = FileArchivePeriod.Day
                };

                LogManager.Setup().LoadConfiguration(c => c.ForLogger().WriteTo(fileTarget));

                // we emit 5 * 25 *(3 x aaa + \n) bytes
                // so that we should get a full file + 3 archives
                Generate100BytesLog('a');
                Generate100BytesLog('b');
                Generate100BytesLog('c');
                Generate100BytesLog('d');
                Generate100BytesLog('e');

                string renderedArchiveFileName = archiveFileName.Replace("{#}", DateTime.Now.ToString(archiveDateFormat));

                LogManager.Configuration = null;

                Action<string, string, Encoding> assertFileContents = (f1, content, encoding) => AssertFileContents(f1, content, encoding, false);

                var extension = Path.GetExtension(renderedArchiveFileName);
                var fileNameWithoutExt = renderedArchiveFileName.Substring(0, renderedArchiveFileName.Length - extension.Length);
                ArchiveFileNameHelper helper = new ArchiveFileNameHelper(tempDir, fileNameWithoutExt, extension);

                var times = 25;
                AssertFileContents(logFileName,
                    StringRepeat(times, "eee\n"),
                    Encoding.UTF8);

                assertFileContents(helper.GetFullPath(1), StringRepeat(times, "bbb\n"), Encoding.UTF8);
                assertFileContents(helper.GetFullPath(2), StringRepeat(times, "ccc\n"), Encoding.UTF8);
                assertFileContents(helper.GetFullPath(3), StringRepeat(times, "ddd\n"), Encoding.UTF8);

                Assert.False(helper.Exists(0), "First archive should have been deleted due to max archive count.");
                Assert.False(helper.Exists(4), "Fifth archive must not have been created yet.");
            }
            finally
            {
                if (File.Exists(logFileName))
                    File.Delete(logFileName);
                if (Directory.Exists(tempDir))
                    Directory.Delete(tempDir, true);
            }
        }

        [Theory]
        [InlineData("archive/test.log_{##}.txt", "archive/test.log_00.txt", null)]
        [InlineData("archive\\test.log_{##}.txt", "archive\\test.log_00.txt", null)]
        [InlineData("file_${date:format=yyyyMMdd}.txt", "file_${date:format=yyyyMMdd}_0.txt", null)]
        [InlineData("file_{#}.txt", "file_${date:format=yyyyMMdd}_00.txt", "yyyyMMdd")]
        public void FileTargetArchiveFileNameTest(string archiveFileName, string expectedArchiveFileName, string archiveDateFormat)
        {
            var subPath = "nlog_" + Guid.NewGuid().ToString();
            var tempDir = Path.Combine(Path.GetTempPath(), subPath);
            var logFile = Path.Combine(tempDir, "file-${date:format=yyyyMMdd}.txt");
            try
            {
                var fileTarget = new FileTarget
                {
                    FileName = logFile,
                    ArchiveFileName = Path.Combine(tempDir, "..", subPath, archiveFileName),
                    ArchiveAboveSize = 1000,
                    MaxArchiveFiles = 1000,
                };
#pragma warning disable CS0618 // Type or member is obsolete
                if (!string.IsNullOrEmpty(archiveDateFormat))
                    fileTarget.ArchiveDateFormat = archiveDateFormat;
#pragma warning restore CS0618 // Type or member is obsolete

                LogManager.Setup().LoadConfiguration(c => c.ForLogger().WriteTo(fileTarget));

                for (var i = 0; i < 25; ++i)
                {
                    logger.Debug("a");
                }

                LogManager.Configuration = null;

                logFile = new SimpleLayout(logFile).Render(LogEventInfo.CreateNullEvent());
                expectedArchiveFileName = new SimpleLayout(expectedArchiveFileName).Render(LogEventInfo.CreateNullEvent());

                Assert.True(File.Exists(logFile));
                Assert.True(File.Exists(Path.Combine(tempDir, expectedArchiveFileName)));
            }
            finally
            {
                if (File.Exists(logFile))
                    File.Delete(logFile);
                if (Directory.Exists(tempDir))
                    Directory.Delete(tempDir, true);
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

                LogManager.Setup().LoadConfiguration(c => c.ForLogger().WriteTo(fileTarget));

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
            var tempDir = Path.Combine(Path.GetTempPath(), "nlog_" + Guid.NewGuid().ToString());
            var logFile = Path.Combine(tempDir, "Application.log");
            var tempDirectory = new DirectoryInfo(tempDir);
            try
            {

                var archiveFile = Path.Combine(tempDir, "Application{#}.log");
                var archiveFileMask = "Application*.log";

                var fileTarget = new FileTarget
                {
                    FileName = logFile,
                    ArchiveFileName = archiveFile,
                    ArchiveAboveSize = 1, //Force immediate archival
                    MaxArchiveFiles = 5
                };

                LogManager.Setup().LoadConfiguration(c => c.ForLogger().WriteTo(fileTarget));

                //Creates 5 archive files.
                for (int i = 0; i <= 5; i++)
                {
                    logger.Debug("a");
                }

                LogManager.Configuration = null;    // Flush

                Assert.True(File.Exists(logFile));

                //Five archive files, plus the log file itself.
                Assert.True(tempDirectory.GetFiles(archiveFileMask).Length == 5 + 1);
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
        public void FileTarget_ArchiveAboveSize_RollWhenFull()
        {
            var tempDir = Path.Combine(Path.GetTempPath(), "nlog_" + Guid.NewGuid().ToString());
            var logFile = Path.Combine(tempDir, "Application");
            var tempDirectory = new DirectoryInfo(tempDir);
            var maxArchiveFiles = 2;

            try
            {
                var fileTarget = new FileTarget
                {
                    FileName = logFile + ".log",
                    Layout = "${message}",
                    LineEnding = LineEndingMode.LF,
                    ArchiveAboveSize = 7,
                    MaxArchiveFiles = maxArchiveFiles,
                };

                LogManager.Setup().LoadConfiguration(c => c.ForLogger().WriteTo(fileTarget));

                logger.Debug("aaaa");
                logger.Debug("bbbb");    // Not roll (new style so all agree when rolling)
                logger.Debug("cccc");    // Roll
                logger.Debug("dddd");    // Not roll (new style so all agree when rolling)
                logger.Debug("eeee");    // Roll
                logger.Debug("ffff");    // Not roll (new style so all agree when rolling)
                logger.Debug("gggg");    // Roll

                LogManager.Configuration = null;    // Flush

                Assert.False(File.Exists(logFile + ".log"));
                Assert.False(File.Exists(logFile + "_01.log"));
                AssertFileContents(logFile + "_02.log", "eeee\nffff\n", Encoding.UTF8);
                AssertFileContents(logFile + "_03.log", "gggg\n", Encoding.UTF8);
                Assert.Equal(maxArchiveFiles, tempDirectory.GetFiles().Length);
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
            var tempDir = Path.Combine(Path.GetTempPath(), "nlog_" + Guid.NewGuid().ToString());
            var logFile = Path.Combine(tempDir, "Application.log");
            var tempDirectory = new DirectoryInfo(tempDir);

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
#pragma warning disable CS0618 // Type or member is obsolete
                    ArchiveDateFormat = "___________yyyyMMddHHmm",
#pragma warning restore CS0618 // Type or member is obsolete
                    MaxArchiveFiles = 10   // Get past the optimization to avoid deleting old files.
                };

                LogManager.Setup().LoadConfiguration(c => c.ForLogger().WriteTo(fileTarget));

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
            var tempDir = Path.Combine(Path.GetTempPath(), "nlog_" + Guid.NewGuid().ToString());
            var logFile = Path.Combine(tempDir, "file.txt");
            try
            {
                var fileTarget = new FileTarget
                {
                    FileName = logFile,
                    ArchiveFileName = Path.Combine(tempDir, "archive", "file.txt2"),
                    ArchiveSuffixFormat = "",
                    ArchiveAboveSize = 100,
                    LineEnding = LineEndingMode.LF,
                    Layout = "${message}",
                    MaxArchiveFiles = 1,
                };

                LogManager.Setup().LoadConfiguration(c => c.ForLogger().WriteTo(fileTarget));

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
                    Path.Combine(tempDir, "archive", "file.txt2"),
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
                    Path.Combine(tempDir, "archive", "file.txt2"),
                    StringRepeat(times, "bbb\n"),
                    Encoding.UTF8);
            }
            finally
            {
                if (File.Exists(logFile))
                    File.Delete(logFile);
                if (Directory.Exists(tempDir))
                    Directory.Delete(tempDir, true);
            }
        }

        [Fact]
        public void ArchiveFileRollsCorrectly()
        {
            var tempDir = Path.Combine(Path.GetTempPath(), "nlog_" + Guid.NewGuid().ToString());
            var logFile = Path.Combine(tempDir, "file.txt");
            try
            {
                var fileTarget = new FileTarget
                {
                    FileName = logFile,
                    ArchiveFileName = Path.Combine(tempDir, "archive", "file.txt2"),
                    ArchiveSuffixFormat = ".{0}",
                    ArchiveAboveSize = 100,
                    LineEnding = LineEndingMode.LF,
                    Layout = "${message}",
                    MaxArchiveFiles = 2,
                };

                LogManager.Setup().LoadConfiguration(c => c.ForLogger().WriteTo(fileTarget));

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
                    Path.Combine(tempDir, "archive", "file.1.txt2"),
                    StringRepeat(times, "bbb\n"),
                    Encoding.UTF8);
                AssertFileContents(
                    Path.Combine(tempDir, "archive", "file.0.txt2"),
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
                    Path.Combine(tempDir, "archive", "file.2.txt2"),
                    StringRepeat(times, "ccc\n"),
                    Encoding.UTF8);
                AssertFileContents(
                    Path.Combine(tempDir, "archive", "file.1.txt2"),
                    StringRepeat(times, "bbb\n"),
                    Encoding.UTF8);
                Assert.False(File.Exists(Path.Combine(tempDir, "archive", "file.txt2")));
            }
            finally
            {
                if (File.Exists(logFile))
                    File.Delete(logFile);
                if (Directory.Exists(tempDir))
                    Directory.Delete(tempDir, true);
            }
        }

        [Fact]
        public void ArchiveFileRollsCorrectly_ExistingArchives()
        {
            var tempDir = Path.Combine(Path.GetTempPath(), "nlog_" + Guid.NewGuid().ToString());
            var logFile = Path.Combine(tempDir, "file.txt");
            try
            {
                Directory.CreateDirectory(Path.Combine(tempDir, "archive"));
                File.Create(Path.Combine(tempDir, "archive", "file.10.txt2")).Dispose();
                File.Create(Path.Combine(tempDir, "archive", "file.9.txt2")).Dispose();

                var fileTarget = new FileTarget
                {
                    FileName = logFile,
                    ArchiveFileName = Path.Combine(tempDir, "archive", "file.txt2"),
                    ArchiveSuffixFormat = ".{0}",
                    ArchiveAboveSize = 100,
                    LineEnding = LineEndingMode.LF,
                    Layout = "${message}",
                    MaxArchiveFiles = 2,
                };

                LogManager.Setup().LoadConfiguration(c => c.ForLogger().WriteTo(fileTarget));

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
                    Path.Combine(tempDir, "archive", "file.11.txt2"),
                    StringRepeat(times, "aaa\n"),
                    Encoding.UTF8);
                Assert.True(File.Exists(Path.Combine(tempDir, "archive", "file.10.txt2")));
                Assert.False(File.Exists(Path.Combine(tempDir, "archive", "file.9.txt2")));
            }
            finally
            {
                if (File.Exists(logFile))
                    File.Delete(logFile);
                if (Directory.Exists(tempDir))
                    Directory.Delete(tempDir, true);
            }
        }

        /// <summary>
        /// Remove archived files in correct order
        /// </summary>
        [Fact]
        public void FileTarget_ArchiveNumbering_remove_correct_order()
        {
            const int maxArchiveFiles = 10;

            var tempDir = ArchiveFileNameHelper.GenerateTempPath();
            var logFile = Path.Combine(tempDir, "file.txt");
            var archiveExtension = "txt";
            var archiveDateFormat = "yyyy-MM-dd";

            try
            {
                var fileTarget = new FileTarget
                {
                    FileName = logFile,
                    ArchiveFileName = Path.Combine(tempDir, "archive", "{#}." + archiveExtension),
#pragma warning disable CS0618 // Type or member is obsolete
                    ArchiveDateFormat = archiveDateFormat,
#pragma warning restore CS0618 // Type or member is obsolete
                    ArchiveAboveSize = 100,
                    LineEnding = LineEndingMode.LF,
                    Layout = "${message}",
                    MaxArchiveFiles = maxArchiveFiles,
                };

                LogManager.Setup().LoadConfiguration(c => c.ForLogger().WriteTo(fileTarget));

                ArchiveFileNameHelper helper = new ArchiveFileNameHelper(Path.Combine(tempDir, "archive"), DateTime.Now.ToString(archiveDateFormat), archiveExtension);

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
                    Assert.False(helper.Exists(numberToBeRemoved),
                        $"archive file {numberToBeRemoved} has not been removed! We are created file {i}");
                }

                LogManager.Configuration = null;
            }
            finally
            {
                if (File.Exists(logFile))
                    File.Delete(logFile);
                if (Directory.Exists(tempDir))
                    Directory.Delete(tempDir, true);
            }
        }

        /// <summary>
        /// Allow multiple archives within the same directory
        /// </summary>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void FileTarget_ArchiveNumbering_remove_correct_wildcard(bool keepFileOpen)
        {
            const int maxArchiveFiles = 5;

            var tempDir = ArchiveFileNameHelper.GenerateTempPath();
            var logFile = Path.Combine(tempDir, "{0}_{1:00}.txt");

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
                    KeepFileOpen = keepFileOpen,
                };

                LogManager.Setup().LoadConfiguration(c => c.ForLogger().WriteTo(fileTarget));
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
                    logFile1 = string.Format(logFile, logger1.Name, TimeSource.Current.Time.Date.ToString("yyyy-MM-dd") + $"_{(i + 1):00}");
                    logFile2 = string.Format(logFile, logger2.Name, TimeSource.Current.Time.Date.ToString("yyyy-MM-dd") + $"_{(i + 1):00}");
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
                    var logFile1 = string.Format(logFile, logger1.Name, TimeSource.Current.Time.Date.ToString("yyyy-MM-dd") + $"_{(i + 1):00}");
                    var logFile2 = string.Format(logFile, logger2.Name, TimeSource.Current.Time.Date.ToString("yyyy-MM-dd") + $"_{(i + 1):00}");
                    Assert.True(File.Exists(logFile1),
                        $"{logFile1} is missing");
                    Assert.True(File.Exists(logFile2),
                        $"{logFile2} is missing");
                }

                for (int i = maxArchiveFiles; i < 10; i++)
                {
                    Generate100BytesLog((char)('b' + i), logger1);
                    Generate100BytesLog((char)('b' + i), logger2);
                    var numberToBeRemoved = i - maxArchiveFiles + 1;

                    var logFile1 = string.Format(logFile, logger1.Name, TimeSource.Current.Time.Date.ToString("yyyy-MM-dd") + $"_{numberToBeRemoved:00}");
                    var logFile2 = string.Format(logFile, logger2.Name, TimeSource.Current.Time.Date.ToString("yyyy-MM-dd") + $"_{numberToBeRemoved:00}");

                    Assert.False(File.Exists(logFile1),
                        $"archive FirstFile {numberToBeRemoved} has not been removed! We are created file {i}");
                    Assert.False(File.Exists(logFile2),
                        $"archive SecondFile {numberToBeRemoved} has not been removed! We are created file {i}");

                    logFile1 = string.Format(logFile, logger1.Name, TimeSource.Current.Time.Date.ToString("yyyy-MM-dd") + $"_{(i + 1):00}");
                    logFile2 = string.Format(logFile, logger2.Name, TimeSource.Current.Time.Date.ToString("yyyy-MM-dd") + $"_{(i + 1):00}");

                    Assert.True(File.Exists(logFile1),
                        $"{logFile1} is missing");
                    Assert.True(File.Exists(logFile2),
                        $"{logFile2} is missing");
                }

                LogManager.Configuration = null;
                Assert.Equal(10, Directory.GetFiles(tempDir).Length);

                // Verify that archieve-cleanup after startup handles same folder archive correctly
                fileTarget.ArchiveAboveSize = 200;
                LogManager.Setup().LoadConfiguration(c => c.ForLogger().WriteTo(fileTarget));
                logger1.Info("Bye");
                logger2.Info("Bye");
                Assert.Equal(10, Directory.GetFiles(tempDir).Length);

                LogManager.Configuration = null;
            }
            finally
            {
                TimeSource.Current = defaultTimeSource; // restore default time source

                LogManager.Configuration = null;

                if (Directory.Exists(tempDir))
                    Directory.Delete(tempDir, true);
            }
        }

        /// <summary>
        /// See that dynamic sequence archive supports same-folder archiving.
        /// </summary>
        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        public void FileTarget_SameDirectory_MaxArchiveFiles(int maxArchiveFiles)
        {
            var tempDir = ArchiveFileNameHelper.GenerateTempPath();
            var logFile1 = Path.Combine(tempDir, "Log{0}.txt");
            try
            {
                var fileTarget = new FileTarget
                {
                    FileName = string.Format(logFile1, ""),
                    ArchiveAboveSize = 100,
                    LineEnding = LineEndingMode.LF,
                    Layout = "${message}",
                    MaxArchiveFiles = maxArchiveFiles,
                    Encoding = Encoding.ASCII,
                };

                LogManager.Setup().LoadConfiguration(c => c.ForLogger().WriteTo(fileTarget));

                Generate100BytesLog('a');
                Generate100BytesLog('b');
                Generate100BytesLog('c');

                var times = 25;
                if (maxArchiveFiles > 1)
                {
                    Assert.False(File.Exists(string.Format(logFile1, "")));

                    AssertFileContents(string.Format(logFile1, "_01"),
                        StringRepeat(times, "bbb\n"),
                        Encoding.ASCII);

                    AssertFileContents(string.Format(logFile1, "_02"),
                        StringRepeat(times, "ccc\n"),
                        Encoding.ASCII);
                }
                else
                {
                    AssertFileContents(string.Format(logFile1, ""),
                        StringRepeat(times, "ccc\n"),
                        Encoding.ASCII);
                }

                LogManager.Configuration = null;
            }
            finally
            {
                if (Directory.Exists(tempDir))
                    Directory.Delete(tempDir, true);
            }
        }

        private void Generate100BytesLog(char c, Logger logger = null)
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
        private sealed class ArchiveFileNameHelper
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
                return Path.Combine(FolderName, $"{FileName}_{number:00}.{Ext}");
            }

            public static string GenerateTempPath()
            {
                return Path.Combine(Path.GetTempPath(), "nlog_" + Guid.NewGuid().ToString());
            }
        }

        [Theory]
        [InlineData("##", 0, "00")]
        [InlineData("##", 1, "01")]
        [InlineData("#", 20, "20")]
        public void FileTarget_WithDateAndSequenceArchiveNumbering_ShouldPadSequenceNumberInArchiveFileName(
            string placeHolderSharps, int sequenceNumber, string expectedSequenceInArchiveFileName)
        {
            string tempDir = Path.Combine(Path.GetTempPath(), "nlog_" + Guid.NewGuid().ToString());
            const string archiveDateFormat = "yyyy-MM-dd";
            string archiveFileName = Path.Combine(tempDir, $"{{{placeHolderSharps}}}.log");
            string expectedArchiveFullName =
                $"{tempDir}/{DateTime.Now.ToString(archiveDateFormat)}_{expectedSequenceInArchiveFileName}.log";

            GenerateArchives(count: sequenceNumber + 2, archiveDateFormat: archiveDateFormat,
                archiveFileName: archiveFileName);
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
            string tempDir = Path.Combine(Path.GetTempPath(), "nlog_" + Guid.NewGuid().ToString());
            string archiveFileName = Path.Combine(tempDir, "{##}.log");
            string expectedDateInArchiveFileName = DateTime.Now.ToString(archiveDateFormat);
            string expectedArchiveFullName = $"{tempDir}/{expectedDateInArchiveFileName}_00.log";

            // We generate 2 archives so that the algorithm that seeks old archives is also tested.
            GenerateArchives(count: 2, archiveDateFormat: archiveDateFormat, archiveFileName: archiveFileName);
            bool resultArchiveWithExpectedNameExists = File.Exists(expectedArchiveFullName);

            Assert.True(resultArchiveWithExpectedNameExists);
        }

        private void GenerateArchives(int count, string archiveDateFormat, string archiveFileName)
        {
            string logFileName = Path.GetTempFileName();
            const int logFileMaxSize = 1;
            var fileTarget = new FileTarget
            {
                FileName = logFileName,
                ArchiveFileName = archiveFileName,
#pragma warning disable CS0618 // Type or member is obsolete
                ArchiveDateFormat = archiveDateFormat,
#pragma warning restore CS0618 // Type or member is obsolete
                ArchiveAboveSize = logFileMaxSize
            };
            LogManager.Setup().LoadConfiguration(c => c.ForLogger().WriteTo(fileTarget));
            for (int currentSequenceNumber = 0; currentSequenceNumber < count; currentSequenceNumber++)
                logger.Debug("Test {0}", currentSequenceNumber);

            LogManager.Configuration = null;
        }

        [Fact]
        public void Dont_throw_Exception_when_archiving_is_enabled()
        {
            try
            {
                LogManager.Setup().LoadConfigurationFromXml(@"<?xml version='1.0' encoding='utf-8' ?>
<nlog xmlns='http://www.nlog-project.org/schemas/NLog.xsd'
      xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'
      throwExceptions='true' >

  <targets>
    <target name='logfile' xsi:type='File' fileName='${basedir}/log.txt' archiveFileName='${basedir}/log.${date}' archiveEvery='Day' />
  </targets>

  <rules>
    <logger name='*' writeTo='logfile' />
  </rules>
</nlog>
");

                LogManager.GetLogger("Test").Info("very important message");

                Assert.NotNull(LogManager.Configuration);
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
                LogManager.Setup().LoadConfigurationFromXml(@"<?xml version='1.0' encoding='utf-8' ?>
<nlog xmlns='http://www.nlog-project.org/schemas/NLog.xsd'
      xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'
      throwExceptions='true' >

  <targets async=""true"" >
    <target  name='logfile' xsi:type='File' fileName='${basedir}/log.txt' archiveFileName='${basedir}/log.${date}' archiveEvery='Day' />
  </targets>

  <rules>
    <logger name='*' writeTo='logfile' />
  </rules>
</nlog>
");

                LogManager.GetLogger("Test").Info("very important message");

                Assert.NotNull(LogManager.Configuration);
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

            var tempDir = Path.Combine(Path.GetTempPath(), "nlog_" + Guid.NewGuid().ToString()) + Path.DirectorySeparatorChar;

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

                GlobalDiagnosticsContext.Set("basedir", tempDir);

                LogManager.Setup().LoadConfigurationFromXml(@"<?xml version='1.0' encoding='utf-8' ?>
<nlog>
  <variable name='basedir' value='' />
  <targets>
      <target name='file' type='File'
              fileName='${gdc:item=basedir}${event-properties:item=serialNo}.txt'
              layout='${message}'
              archiveFileName='${gdc:item=basedir}${event-properties:item=serialNo}.txt'
              archiveSuffixFormat='.{1:yyyy-MM-dd}'
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

                var currentFiles = new DirectoryInfo(tempDir).GetFiles();
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
                if (Directory.Exists(tempDir))
                    Directory.Delete(tempDir, true);
            }
        }

        [Fact]
        public void LoggingShouldNotTriggerTypeResolveEventTest()
        {
            var tempDir = Path.Combine(Path.GetTempPath(), "nlog_" + Guid.NewGuid().ToString());
            var logFile = Path.Combine(tempDir, "log.log");

            System.ResolveEventHandler noResolveTest = (s, args) => { Assert.True(false); return null; };

            try
            {
                LogManager.Setup().LoadConfigurationFromXml(@"<nlog throwExceptions='true'>
                        <targets>
                            <target name='file1' encoding='UTF-8' type='File' fileName='" + logFile + @"'>
                                <layout type='JsonLayout'>
                                  <attribute name='message' layout='${message}' />
                                  <attribute name='xml'>
                                    <layout type='XmlLayout' includeEventProperties='true' />
                                  </attribute>
                                </layout>
                            </target>
                        </targets>
                        <rules>
                            <logger name='*' minlevel='Trace' writeTo='file1' />
                        </rules>
                    </nlog>");

                LogManager.GetLogger("Test").Info("very important message");

                Assert.NotNull(LogManager.Configuration);

                AppDomain.CurrentDomain.TypeResolve += noResolveTest;
                AppDomain.CurrentDomain.AssemblyResolve += noResolveTest;

                LogManager.GetLogger("Test").Info("very important message");
            }
            finally
            {
                AppDomain.CurrentDomain.TypeResolve -= noResolveTest;
                AppDomain.CurrentDomain.AssemblyResolve -= noResolveTest;
                LogManager.Configuration = null;
                if (Directory.Exists(tempDir))
                    Directory.Delete(tempDir, true);
            }
        }

        [Theory]
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
        private static void TestMaxArchiveFilesWithDate(int maxArchiveFilesConfig, int expectedArchiveFiles, string dateFormat, bool changeCreationAndWriteTime)
        {
            string tempDir = Path.Combine(Path.GetTempPath(), "nlog_" + Guid.NewGuid().ToString());
            string archivePath = Path.Combine(tempDir, "archive");

            var archiveDir = new DirectoryInfo(archivePath);
            try
            {
                archiveDir.Create();
                //set-up, create files.

                //same dateformat as in config
                string fileExt = ".log";
                DateTime now = DateTime.Now;
                int i = 0;
                foreach (string filePath in ArchiveFileNamesGenerator(archivePath, "{0:" + dateFormat + "}" + fileExt).Take(30))
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
                            fileName='" + tempDir + @"/${date:format=yyyyMMdd-HHmm}" + fileExt + @"'
                            layout='${message}'
                            archiveEvery='minute'
                            maxArchiveFiles='" + maxArchiveFilesConfig + @"'
                            archiveFileName='" + archivePath + @"/{#}.log'
                            archiveDateFormat='" + dateFormat + @"' />
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

                LogManager.Configuration = null;    // Flush
            }
            finally
            {
                //cleanup
                archiveDir.Delete(true);
                if (Directory.Exists(tempDir))
                    Directory.Delete(tempDir, true);
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
        private static void HandleArchiveFilesMultipleContextMultipleTargetsTest(
            int maxArchiveFilesConfig, int expectedArchiveFiles, string dateFormat, bool changeCreationAndWriteTime)
        {
            string tempDir = Path.Combine(Path.GetTempPath(), "nlog_" + Guid.NewGuid().ToString());
            string archivePath = Path.Combine(tempDir, "archive");
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
                foreach (string filePath in ArchiveFileNamesGenerator(archivePath, app1TraceNm + "_{0:" + dateFormat + "}" + fileExt).Take(numberFilesCreatedPerTargetArchive))
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
                foreach (string filePath in ArchiveFileNamesGenerator(archivePath, app1DebugNm + "_{0:" + dateFormat + "}" + fileExt).Take(numberFilesCreatedPerTargetArchive))
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
                foreach (string filePath in ArchiveFileNamesGenerator(archivePath, app2Nm + "_{0:" + dateFormat + "}" + fileExt).Take(numberFilesCreatedPerTargetArchive))
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
                File.WriteAllLines(Path.Combine(tempDir, app1TraceNm + fileExt), new[] { "Write first app debug target. Startup will archive this file" }, Encoding.ASCII);

                var app1Config = XmlLoggingConfiguration.CreateFromXmlString(@"<nlog throwExceptions='true'>
                                    <targets>
                                      <target name='traceFile' type='File'
                                        fileName='" + Path.Combine(tempDir, app1TraceNm + fileExt) + @"'
                                        archiveFileName='" + Path.Combine(archivePath, app1TraceNm + @"_${date:format=" + dateFormat + "}" + fileExt) + @"'
                                        archiveEvery='minute'
                                        archiveOldFileOnStartup='true'
                                        maxArchiveFiles='" + maxArchiveFilesConfig + @"'
                                        layout='${longdate} [${level}] [${callsite}] ${message}'
                                        keepFileOpen='false' />
                                    <target name='debugFile' type='File'
                                        fileName='" + Path.Combine(tempDir, app1DebugNm + fileExt) + @"'
                                        archiveFileName='" + Path.Combine(archivePath, app1DebugNm + @"_${date:format=" + dateFormat + "}" + fileExt) + @"'
                                        archiveEvery='minute'
                                        archiveOldFileOnStartup='true'
                                        maxArchiveFiles='" + maxArchiveFilesConfig + @"'
                                        layout='${longdate} [${level}] [${callsite}] ${message}'
                                        keepFileOpen='false' />
                                    </targets>
                                    <rules>
                                      <logger name='*' minLevel='Trace' writeTo='traceFile' />
                                      <logger name='*' minLevel='Debug' writeTo='debugFile' />
                                    </rules>
                                  </nlog>");

                var app2Config = XmlLoggingConfiguration.CreateFromXmlString(@"<nlog throwExceptions='true'>
                                    <targets>
                                      <target name='logfile' type='File'
                                        fileName='" + Path.Combine(tempDir, app2Nm + fileExt) + @"'
                                        archiveFileName='" + Path.Combine(archivePath, app2Nm + @"_${date:format=" + dateFormat + "}" + fileExt) + @"'
                                        archiveEvery='minute'
                                        archiveOldFileOnStartup='true'
                                        maxArchiveFiles='" + maxArchiveFilesConfig + @"'
                                        layout='${longdate} [${level}] [${callsite}] ${message}'
                                        keepFileOpen='false' />
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
                if (Directory.Exists(tempDir))
                    Directory.Delete(tempDir, true);
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
        private static void HandleArchiveFilesMultipleContextSingleTargetsTest(
            int maxArchiveFilesConfig, int expectedArchiveFiles, string dateFormat, bool changeCreationAndWriteTime)
        {
            string tempDir = Path.Combine(Path.GetTempPath(), "nlog_" + Guid.NewGuid().ToString());
            string archivePath = Path.Combine(tempDir, "archive");
            var archiveDir = new DirectoryInfo(archivePath);
            try
            {
                archiveDir.Create();
                var numberFilesCreatedPerTargetArchive = 30;

                // use same config vars for mock files, as for nlog config
                var fileExt = ".log";
                var app1Nm = "AppA";
                var app2Nm = "AppB";

                var now = DateTime.Now;
                var i = 0;
                // create mock app1 archives (matches app1 config for target)
                foreach (string filePath in ArchiveFileNamesGenerator(archivePath, app1Nm + "_{0:" + dateFormat + "}" + fileExt).Take(numberFilesCreatedPerTargetArchive))
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
                foreach (string filePath in ArchiveFileNamesGenerator(archivePath, app2Nm + "_{0:" + dateFormat + "}" + fileExt).Take(numberFilesCreatedPerTargetArchive))
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
                File.WriteAllLines(Path.Combine(tempDir, app1Nm + fileExt), new[] { "Write first app debug target. Startup will archive this file" }, Encoding.ASCII);

                var app1Config = XmlLoggingConfiguration.CreateFromXmlString(@"<nlog throwExceptions='true'>
                                    <targets>
                                      <target name='logfile' type='File'
                                        fileName='" + Path.Combine(tempDir, app1Nm + fileExt) + @"'
                                        archiveFileName='" + Path.Combine(archivePath, app1Nm + @"_${date:format=" + dateFormat + "}" + fileExt) + @"'
                                        archiveEvery='minute'
                                        archiveOldFileOnStartup='true'
                                        maxArchiveFiles='" + maxArchiveFilesConfig + @"'
                                        layout='${longdate} [${level}] [${callsite}] ${message}'
                                        keepFileOpen='false' />
                                    </targets>;
                                    <rules>
                                      <logger name='*' minLevel='Trace' writeTo='logfile' />
                                    </rules>
                                  </nlog>");

                var app2Config = XmlLoggingConfiguration.CreateFromXmlString(@"<nlog throwExceptions='true'>
                                    <targets>
                                      <target name='logfile' type='File'
                                        fileName='" + Path.Combine(tempDir, app2Nm + fileExt) + @"'
                                        archiveFileName='" + Path.Combine(archivePath, app2Nm + @"_${date:format=" + dateFormat + "}" + fileExt) + @"'
                                        archiveEvery='minute'
                                        archiveOldFileOnStartup='true'
                                        maxArchiveFiles='" + maxArchiveFilesConfig + @"'
                                        layout='${longdate} [${level}] [${callsite}] ${message}'
                                        keepFileOpen='false' />
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
                if (Directory.Exists(tempDir))
                    Directory.Delete(tempDir, true);
            }
        }

        /// <summary>
        /// Generate unlimited archivefiles names. Don't use toList on this ;)
        /// </summary>
        /// <param name="path"></param>
        /// <param name="dateFormat"></param>
        /// <param name="fileExt">fileext with .</param>
        /// <returns></returns>
        private static IEnumerable<string> ArchiveFileNamesGenerator(string path, string fileFormat)
        {
            //yyyyMMdd-HHmm
            int dateOffset = 1;
            var now = DateTime.Now;
            while (true)
            {
                dateOffset--;
                yield return Path.Combine(path, string.Format(fileFormat, now.AddDays(dateOffset)));
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

                LogManager.Setup().LoadConfiguration(c => c.ForLogger().WriteTo(fileTarget));

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

                LogManager.Setup().LoadConfiguration(c => c.ForLogger().WriteTo(fileTarget));

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
        public void RelativeSequentialArchiveTest_MaxArchiveFiles_NoLimit()
        {
            string tempDir = Path.Combine(Path.GetTempPath(), "nlog_" + Guid.NewGuid().ToString());
            var logfile = Path.Combine(tempDir, "file.txt");
            try
            {
                string archiveFolder = Path.Combine(tempDir, "archive");
                var fileTarget = new FileTarget
                {
                    FileName = logfile,
                    ArchiveFileName = Path.Combine(archiveFolder, "{##}.txt"),
                    ArchiveAboveSize = 100,
                    ArchiveOldFileOnStartup = true, // Verify ArchiveOldFileOnStartup works together with ArchiveAboveSize
                    LineEnding = LineEndingMode.LF,
                    Layout = "${message}",
                };

                LogManager.Setup().LoadConfiguration(c => c.ForLogger().WriteTo(fileTarget));
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
                   Path.Combine(archiveFolder, "00.txt"),
                   StringRepeat(times, "aaa\n"),
                   Encoding.UTF8);

                AssertFileContents(
                    Path.Combine(archiveFolder, "01.txt"),
                    StringRepeat(times, "bbb\n"),
                    Encoding.UTF8);

                AssertFileContents(
                    Path.Combine(archiveFolder, "02.txt"),
                    StringRepeat(times, "ccc\n"),
                    Encoding.UTF8);

                AssertFileContents(
                    Path.Combine(archiveFolder, "03.txt"),
                    StringRepeat(times, "ddd\n"),
                    Encoding.UTF8);

                Assert.False(File.Exists(Path.Combine(archiveFolder, "04.txt")));
            }
            finally
            {
                if (File.Exists(logfile))
                    File.Delete(logfile);
                if (Directory.Exists(tempDir))
                    Directory.Delete(tempDir, true);
            }
        }

        [Fact]
        public void TestFilenameCleanup()
        {
            var invalidChars = Path.GetInvalidFileNameChars();
            var invalidFileName = Path.DirectorySeparatorChar.ToString();
            var expectedFileName = "";
            for (int i = 0; i < invalidChars.Length; i++)
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
            expectedFileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, expectedFileName);

            //under mono this the invalid chars is sometimes only 1 char (so min width 2)
            Assert.True(invalidFileName.Length >= 2);
            //CleanupFileName is default true;
            var fileTarget = new FileTarget();
            fileTarget.FileName = invalidFileName;

            var path = FileTarget.CleanFullFilePath(fileTarget.FileName.Render(LogEventInfo.CreateNullEvent()));
            Assert.Equal(expectedFileName, path);
        }

        [Theory]
        [InlineData(DayOfWeek.Sunday, "2017-03-02 15:27:34.651", "2017-03-05")]    // On a Thursday, finding next Sunday
        [InlineData(DayOfWeek.Thursday, "2017-03-02 15:27:34.651", "2017-03-09")]  // On a Thursday, finding next Thursday
        [InlineData(DayOfWeek.Monday, "2017-03-02 00:00:00.000", "2017-03-06")]    // On Thursday at Midnight, finding next Monday
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
#if !NET6_0_OR_GREATER
        [InlineData("UTF-7", false)]
#endif
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
            try
            {
                LogManager.ThrowExceptions = false;

                var fileTarget = new FileTarget { FileName = "${logger}", Layout = "${message}", ArchiveAboveSize = 10, DiscardAll = true };
                var logFactory = new LogFactory().Setup().LoadConfiguration(cfg => cfg.ForLogger().WriteTo(fileTarget)).LogFactory;

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
                logFactory.Flush();

                Assert.Equal(5, exceptions.Count);
                Assert.Null(exceptions[0]);
                Assert.Null(exceptions[1]); // Will be written together
                Assert.NotNull(exceptions[2]);
                Assert.NotNull(exceptions[3]);
                Assert.NotNull(exceptions[4]);
            }
            finally
            {
                LogManager.ThrowExceptions = true;
            }
        }

        [Fact]
        public void BatchBufferOverflowTest()
        {
            string tempDir = Path.Combine(Path.GetTempPath(), "nlog_" + Guid.NewGuid().ToString());
            var logfile = Path.Combine(tempDir, "file.txt");
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

                var logFactory = new LogFactory().Setup().LoadConfiguration(cfg => cfg.ForLogger().WriteTo(fileTarget)).LogFactory;

                var result = new List<int>();
                var events = new List<NLog.Common.AsyncLogEventInfo>();
                var times = 200;
                for (int i = 1; i <= times; ++i)
                {
                    int counter = i;
                    events.Add(new LogEventInfo(LogLevel.Info, "logger", counter.ToString()).WithContinuation(ex => result.Add(ex is null ? counter : -1)));
                }

                // Act
                fileTarget.WriteAsyncLogEvents(events);
                logFactory.Flush();
                logFactory.Dispose();

                // Assert
                Assert.Equal(Enumerable.Range(1, times).ToList(), result);
                AssertFileContents(logfile, string.Join("\n", result.ToArray()) + "\n", Encoding.UTF8);
            }
            finally
            {
                if (File.Exists(logfile))
                    File.Delete(logfile);
                if (Directory.Exists(tempDir))
                    Directory.Delete(tempDir, true);
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

        private static void HandleArchiveFileAlreadyExistsTest(Encoding encoding, bool hasBom)
        {
            var tempDir = Path.Combine(Path.GetTempPath(), "nlog_" + Guid.NewGuid().ToString());
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
                var archiveFileNamePattern = Path.Combine(tempDir, "log_{#}.txt");
                var archiveFileName = archiveFileNamePattern.Replace("{#}", oldTime.ToString(archiveDateFormat));
                File.WriteAllText(archiveFileName, "message already in archive" + Environment.NewLine, encoding);

                LogManager.ThrowExceptions = true;

                // configure nlog
                var fileTarget = new FileTarget("file")
                {
                    FileName = logFile,
                    ArchiveEvery = FileArchivePeriod.Day,
                    ArchiveFileName = archiveFileNamePattern,
                    ArchiveSuffixFormat = "_{1:" + archiveDateFormat + "}",
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
                if (File.Exists(logFile))
                    File.Delete(logFile);
                if (Directory.Exists(tempDir))
                    Directory.Delete(tempDir, true);
            }
        }

        [Fact]
        public void DontCrashWhenDateAndSequenceDoesntMatchFiles()
        {
            var tempDir = Path.Combine(Path.GetTempPath(), "nlog_" + Guid.NewGuid().ToString());
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

                Assert.True(File.Exists(logFile));
            }
            finally
            {
                if (File.Exists(logFile))
                    File.Delete(logFile);
                if (Directory.Exists(tempDir))
                    Directory.Delete(tempDir, true);
            }
        }

        [Fact]
        public void ShouldNotArchiveWhenMeetingOldLogEventTimestamps()
        {
            var tempDir = Path.Combine(Path.GetTempPath(), "nlog_" + Guid.NewGuid().ToString());
            var logFile = Path.Combine(tempDir, "file.txt");
            try
            {
                string archiveFolder = Path.Combine(tempDir, "archive");
                var fileTarget = new FileTarget
                {
                    FileName = logFile,
                    ArchiveFileName = Path.Combine(archiveFolder, ".txt"),
                    ArchiveSuffixFormat = "{0:0000}",
                    ArchiveEvery = FileArchivePeriod.Day,
                    LineEnding = LineEndingMode.LF,
                    Layout = "${message}",
                };

                LogManager.Setup().LoadConfiguration(c => c.ForLogger().WriteTo(fileTarget));

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

                Assert.False(File.Exists(Path.Combine(archiveFolder, "0001.txt")));
            }
            finally
            {
                if (File.Exists(logFile))
                    File.Delete(logFile);
                if (Directory.Exists(tempDir))
                    Directory.Delete(tempDir, true);
            }
        }

        private static void AssertFileContentsStartsWith(string fileName, string contents, Encoding encoding)
        {
            FileInfo fi = new FileInfo(fileName);
            if (!fi.Exists)
                Assert.Fail("File '" + fileName + "' doesn't exist.");

            byte[] encodedBuf = encoding.GetBytes(contents);

            byte[] buf = File.ReadAllBytes(fileName);
            Assert.True(encodedBuf.Length <= buf.Length,
                $"File:{fileName} encodedBytes:{encodedBuf.Length} does not match file.content:{buf.Length}, file.length = {fi.Length}");

            for (int i = 0; i < encodedBuf.Length; ++i)
            {
                if (encodedBuf[i] != buf[i])
                    Assert.True(encodedBuf[i] == buf[i],
                        $"File:{fileName} content mismatch {(int)encodedBuf[i]} <> {(int)buf[i]} at index {i}");
            }
        }

        private static void AssertFileContentsEndsWith(string fileName, string contents, Encoding encoding)
        {
            if (!File.Exists(fileName))
                Assert.Fail("File '" + fileName + "' doesn't exist.");

            string fileText = File.ReadAllText(fileName, encoding);
            Assert.True(fileText.Length >= contents.Length);
            Assert.Equal(contents, fileText.Substring(fileText.Length - contents.Length));
        }
    }
}
