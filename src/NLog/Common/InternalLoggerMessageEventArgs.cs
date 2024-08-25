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

namespace NLog.Common
{
    using System;
    using System.ComponentModel;
    using JetBrains.Annotations;

    /// <summary>
    /// A message has been written to the internal logger
    /// </summary>
    [Obsolete("Instead use InternalEventOccurred and InternalLogEventArgs. Marked obsolete with NLog v5.3")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public sealed class InternalLoggerMessageEventArgs : EventArgs
    {
        /// <summary>
        /// The rendered message
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// The log level
        /// </summary>
        public LogLevel Level { get; }

        /// <summary>
        /// The exception. Could be null.
        /// </summary>
        [CanBeNull]
        public Exception Exception { get; }

        /// <summary>
        /// The type that triggered this internal log event, for example the FileTarget.
        /// This property is not always populated.
        /// </summary>
        [CanBeNull]
        public Type SenderType { get; }

        /// <summary>
        /// The context name that triggered this internal log event, for example the name of the Target.
        /// This property is not always populated.
        /// </summary>
        [CanBeNull]
        public string SenderName { get; }

        internal InternalLoggerMessageEventArgs(string message, LogLevel level, [CanBeNull] Exception exception, [CanBeNull] Type senderType, [CanBeNull] string senderName)
        {
            Message = message;
            Level = level;
            Exception = exception;
            SenderType = senderType;
            SenderName = senderName;
        }
    }
}
