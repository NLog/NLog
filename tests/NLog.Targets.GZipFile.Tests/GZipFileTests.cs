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

using System;
using System.IO;
using System.IO.Compression;
using Xunit;

namespace NLog.Targets.GZipFile.Tests
{
    public class GZipFileTests
    {
        [Fact]
        public void SimpleFileGZipStream()
        {
            var tempDir = Path.Combine(Path.GetTempPath(), "nlog_" + Guid.NewGuid().ToString());
            var logFileName = Path.Combine(tempDir, "log.gzip");

            try
            {
                var logFactory = new LogFactory().Setup().LoadConfiguration(cfg =>
                {
                    cfg.ForLogger().WriteTo(new GZipFileTarget() { FileName = logFileName, Layout = "${message}", LineEnding = LineEndingMode.LF });
                }).LogFactory;

                logFactory.GetCurrentClassLogger().Info("Hello");
                logFactory.GetCurrentClassLogger().Info("World");
                logFactory.Shutdown();

                using (var logFile = new StreamReader(new GZipStream(new FileStream(logFileName, FileMode.Open), CompressionMode.Decompress)))
                {
                    Assert.Equal("Hello", logFile.ReadLine());
                    Assert.Equal("World", logFile.ReadLine());
                    Assert.Null(logFile.ReadLine());
                }
            }
            finally
            {
                if (Directory.Exists(tempDir))
                    Directory.Delete(tempDir, true);
            }
        }

        [Fact]
        public void SimpleFileGZipStream_AutoFlush_False()
        {
            var tempDir = Path.Combine(Path.GetTempPath(), "nlog_" + Guid.NewGuid().ToString());
            var logFileName = Path.Combine(tempDir, "log.gzip");

            try
            {
                var logFactory = new LogFactory().Setup().LoadConfiguration(cfg =>
                {
                    cfg.ForLogger().WriteTo(new GZipFileTarget() { FileName = logFileName, Layout = "${message}", LineEnding = LineEndingMode.LF, AutoFlush = false, CompressionLevel = CompressionLevel.Optimal });
                }).LogFactory;

                logFactory.GetCurrentClassLogger().Info("Hello");
                logFactory.GetCurrentClassLogger().Info("World");
                logFactory.Shutdown();

                using (var logFile = new StreamReader(new GZipStream(new FileStream(logFileName, FileMode.Open), CompressionMode.Decompress)))
                {
                    Assert.Equal("Hello", logFile.ReadLine());
                    Assert.Equal("World", logFile.ReadLine());
                    Assert.Null(logFile.ReadLine());
                }
            }
            finally
            {
                if (Directory.Exists(tempDir))
                    Directory.Delete(tempDir, true);
            }
        }

        [Fact]
        public void SimpleFileGZipStream_ArchiveOldFileOnStartup()
        {
            var tempDir = Path.Combine(Path.GetTempPath(), "nlog_" + Guid.NewGuid().ToString());
            var logFileName = Path.Combine(tempDir, "log.gzip");

            try
            {
                var logFactory = new LogFactory().Setup().LoadConfiguration(cfg =>
                {
                    cfg.ForLogger().WriteTo(new GZipFileTarget() { FileName = logFileName, Layout = "${message}", LineEnding = LineEndingMode.LF });
                }).LogFactory;

                logFactory.GetCurrentClassLogger().Info("Hello");
                logFactory.Shutdown();

                var logFactory2 = new LogFactory().Setup().LoadConfiguration(cfg =>
                {
                    cfg.ForLogger().WriteTo(new GZipFileTarget() { FileName = logFileName, Layout = "${message}", LineEnding = LineEndingMode.LF });
                }).LogFactory;

                logFactory2.GetCurrentClassLogger().Info("World");
                logFactory2.Shutdown();

                using (var logFile = new StreamReader(new GZipStream(new FileStream(logFileName, FileMode.Open), CompressionMode.Decompress)))
                {
                    Assert.Equal("Hello", logFile.ReadLine());
                    Assert.Null(logFile.ReadLine());
                }

                using (var logFile = new StreamReader(new GZipStream(new FileStream(Path.Combine(tempDir, "log_01.gzip"), FileMode.Open), CompressionMode.Decompress)))
                {
                    Assert.Equal("World", logFile.ReadLine());
                    Assert.Null(logFile.ReadLine());
                }
            }
            finally
            {
                if (Directory.Exists(tempDir))
                    Directory.Delete(tempDir, true);
            }
        }
    }
}
