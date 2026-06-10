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
using NLog.Internal;

namespace NLog.Targets
{
    /// <summary>
    /// Two <see cref="FileLifecycleHooks"/> linked together to form an execution chain.
    /// </summary>
    public sealed class LinkedFileLifecycleHooks : FileLifecycleHooks
    {
        private readonly FileLifecycleHooks _first;
        private readonly FileLifecycleHooks _second;

        /// <summary>
        /// Creates a new <see cref="LinkedFileLifecycleHooks"/> linking <paramref name="first"/> and <paramref name="second"/> to one <see cref="FileLifecycleHooks"/>.
        /// </summary>
        /// <param name="first">First <see cref="FileLifecycleHooks"/> in chain.</param>
        /// <param name="second">Second <see cref="FileLifecycleHooks"/> in chain</param>
        public LinkedFileLifecycleHooks(FileLifecycleHooks first, FileLifecycleHooks second)
        {
            _first = Guard.ThrowIfNull(first);
            _second = Guard.ThrowIfNull(second);
        }

        #region Overrides of FileLifecycleHooks

        /// <summary>
        /// Called before a log file gets deleted.
        /// This can be used to copy old logs to an archive location or send to a backup server.
        /// </summary>
        /// <param name="filePath">The full path to the file being deleted.</param>
        public override void OnFileDeleting(String filePath)
        {
            _first.OnFileDeleting(filePath);
            _second.OnFileDeleting(filePath);
        }

        /// <summary>
        /// Called after an open file was closed.
        /// </summary>
        /// <param name="filePath">The full path to the file being closed.</param>
        public override void OnFileClosed(String filePath)
        {
            _first.OnFileClosed(filePath);
            _second.OnFileClosed(filePath);
        }

        /// <summary>
        /// Initialize or wrap the <paramref name="underlyingStream"/> opened on the log file. Wrap the stream in another that adds buffering, compression, encryption, etc. The underlying
        /// file may or may not be empty when this method is called.
        /// </summary>
        /// <param name="filePath">The full path to the log file.</param>
        /// <param name="underlyingStream">The underlying <see cref="Stream"/> opened on the log file.</param>
        /// <returns>The <see cref="Stream"/> NLog should use when writing events to the log file.</returns>
        public override Stream OnFileOpened(String filePath, Stream underlyingStream)
        {

            var wrappedStream = _first.OnFileOpened(filePath, underlyingStream);
            wrappedStream = _second.OnFileOpened(filePath, wrappedStream);

            return wrappedStream;
        }

        /// <summary>
        /// Called when the target gets initialized.
        /// </summary>
        /// <param name="target">Initialized target.</param>
        public override void OnTargetInitialize(FileTarget target)
        {
            _first.OnTargetInitialize(target);
            _second.OnTargetInitialize(target);
        }

        /// <summary>
        /// Called when the target is being closed.
        /// </summary>
        /// <param name="target">Closing target.</param>
        public override void OnTargetClose(FileTarget target)
        {
            _first.OnTargetClose(target);
            _second.OnTargetClose(target);
        }

        #endregion
    }
}
