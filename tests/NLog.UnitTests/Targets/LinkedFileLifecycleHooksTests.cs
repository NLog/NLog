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
using System.Collections.Generic;
using System.IO;
using NLog.Targets;
using Xunit;

namespace NLog.UnitTests.Targets
{
    public class LinkedFileLifecycleHooksTests : NLogTestBase
    {
        [Fact]
        public void Ctor_NullCheck()
        {
            var callRecording = new List<string>();

            var first = new MockFileLifecycleHooks("hook_1", callRecording);
            var second = new MockFileLifecycleHooks("hook_2", callRecording);
            Assert.Throws<ArgumentNullException>(() => _ = new LinkedFileLifecycleHooks(null, second));
            Assert.Throws<ArgumentNullException>(() => _ = new LinkedFileLifecycleHooks(first, null));
        }

        [Fact]
        public void VerifyChainedCalls()
        {
            var callRecording = new List<string>();

            var first = new MockFileLifecycleHooks("hook_1", callRecording);
            var second = new MockFileLifecycleHooks("hook_2", callRecording);
            var target = new LinkedFileLifecycleHooks(first, second);

            target.OnFileDeleting("del.log");
            target.OnFileClosed("close.log");
            using (var stream = new MemoryStream())
            {
                var actualStream = target.OnFileOpened("open.log", stream);
                Assert.IsType<MockStream>(actualStream);
                Assert.Equal("hook_2", ((MockStream)actualStream).Name);
            }

            var expectedTarget = new FileTarget("my-file_1");
            target.OnTargetInitialize(expectedTarget);

            expectedTarget = new FileTarget("my-file_2");
            expectedTarget.Dispose();
            target.OnTargetClose(expectedTarget);

            var expected = new List<string>
            {
                "hook_1_OnFileDeleting_del.log",
                "hook_2_OnFileDeleting_del.log",
                "hook_1_OnFileClosed_close.log",
                "hook_2_OnFileClosed_close.log",
                "hook_1_OnFileOpened_open.log_stream",
                "hook_2_OnFileOpened_open.log_hook_1",
                "hook_1_OnTargetInitialize_my-file_1",
                "hook_2_OnTargetInitialize_my-file_1",
                "hook_1_OnTargetClose_my-file_2",
                "hook_2_OnTargetClose_my-file_2"

            };

            Assert.Equal(expected, callRecording);
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
                var streamName = "stream";
                if (underlyingStream is MockStream mockStream)
                    streamName = mockStream.Name;
                _callRecording.Add($"{_name}_{nameof(OnFileOpened)}_{filePath}_{streamName}");

                return new MockStream(underlyingStream, _name);
            }

            public override void OnTargetInitialize(FileTarget target) =>
                _callRecording.Add($"{_name}_{nameof(OnTargetInitialize)}_{target.Name}");

            #endregion
        }

        private sealed class MockStream : Stream
        {
            private Stream _inner;
            public string Name { get; private set; }
            public MockStream(Stream inner, string name)
            {
                _inner = inner;
                Name = name;
            }

            #region Overrides of Stream

            public override void Flush() =>
                _inner.Flush();
            public override Int64 Seek(Int64 offset, SeekOrigin origin) =>
                _inner.Seek(offset, origin);
            public override void SetLength(Int64 value) =>
                _inner.SetLength(value);
            public override Int32 Read(Byte[] buffer, Int32 offset, Int32 count) =>
                _inner.Read(buffer, offset, count);
            public override void Write(Byte[] buffer, Int32 offset, Int32 count) =>
                _inner.Write(buffer, offset, count);
            public override Boolean CanRead => _inner.CanRead;
            public override Boolean CanSeek => _inner.CanSeek;
            public override Boolean CanWrite => _inner.CanWrite;
            public override Int64 Length => _inner.Length;
            public override Int64 Position
            {
                get => _inner.Position;
                set => _inner.Position = value;
            }

            #endregion
        }
    }
}
