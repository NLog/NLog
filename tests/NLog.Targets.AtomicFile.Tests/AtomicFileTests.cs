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

namespace NLog.Targets.AtomicFile.Tests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Xunit;

    public class AtomicFileTests
    {
        public AtomicFileTests()
        {
            LogManager.ThrowExceptions = true;
        }

        [Fact]
        public void SimpleAtomicFileStream_HeaderFooterBom()
        {
            var tempDir = Path.Combine(Path.GetTempPath(), "nlog_" + Guid.NewGuid().ToString());
            var logFileName = Path.Combine(tempDir, "log.txt");

            try
            {
                var callRecording = new List<string>();
                var logFactory = new LogFactory().Setup().LoadConfiguration(cfg =>
                {
                    cfg.ForLogger().WriteTo(new AtomicFileTarget(new MockFileLifecycleHooks("hooks", callRecording))
                    {
                        FileName = logFileName,
                        Layout = "${message}",
                        LineEnding = LineEndingMode.LF,
                        Header = "Hello World",
                        Footer = "Goodbye World",
                        WriteBom = true
                    });
                }).LogFactory;

                logFactory.GetCurrentClassLogger().Info("There was light");
                logFactory.Shutdown();

                using (var logFile = new StreamReader(new FileStream(logFileName, FileMode.Open)))
                {
                    Assert.Equal("Hello World", logFile.ReadLine());
                    Assert.Equal("There was light", logFile.ReadLine());
                    Assert.Equal("Goodbye World", logFile.ReadLine());
                    Assert.Null(logFile.ReadLine());
                }

                var expected = new List<string>
                {
                    "hooks_OnTargetInitialize_AtomFile",
                    $"hooks_OnFileOpened_{logFileName}_FileStream",
                    "hooks_OnTargetClose_AtomFile",
                    $"hooks_OnFileClosed_{logFileName}"
                };

                Assert.Equal(expected.Count, callRecording.Count);
                for (var i = 0; i < expected.Count; i++)
                {
                    var expectedString = expected[i];
                    var actualString = callRecording[i];
                    Assert.True(expectedString.Equals(actualString, StringComparison.OrdinalIgnoreCase), $"Call record '{actualString}' does not match expected value '{expectedString}'");
                }

            }
            finally
            {
                if (File.Exists(logFileName))
                    File.Delete(logFileName);
                if (Directory.Exists(tempDir))
                    Directory.Delete(tempDir, true);
            }
        }

        private sealed class MockFileLifecycleHooks : FileLifecycleHooks
        {
            private string _name;
            private List<string> _callRecording;

            public MockFileLifecycleHooks(string name, List<string> callRecording)
            {
                _name = name;
                _callRecording = callRecording;
            }

            #region Overrides of FileLifecycleHooks

            public override void OnTargetClose(FileTarget target) =>
                _callRecording.Add($"{_name}_{nameof(OnTargetClose)}_{target.Name}");

            public override void OnFileClosed(String filePath) =>
                _callRecording.Add($"{_name}_{nameof(OnFileClosed)}_{filePath}");

            public override void OnFileDeleting(String filePath) =>
                _callRecording.Add($"{_name}_{nameof(OnFileDeleting)}_{filePath}");

            public override Stream OnFileOpened(String filePath, Stream underlyingStream)
            {
                _callRecording.Add($"{_name}_{nameof(OnFileOpened)}_{filePath}_{underlyingStream.GetType().Name}");
                return underlyingStream;
            }

            public override void OnTargetInitialize(FileTarget target) =>
                _callRecording.Add($"{_name}_{nameof(OnTargetInitialize)}_{target.Name}");

            #endregion
        }
    }
}
