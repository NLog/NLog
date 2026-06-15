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

    public class AtomicFileTests
    {
        public AtomicFileTests()
        {
            LogManager.ThrowExceptions = true;
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
