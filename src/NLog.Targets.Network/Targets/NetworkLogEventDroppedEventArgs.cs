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

namespace NLog.Targets
{
    using System;

    /// <summary>
    /// Arguments for <see cref="NetworkTarget.LogEventDropped"/> events.
    /// </summary>
    public class NetworkLogEventDroppedEventArgs : EventArgs
    {
        /// <inheritdoc cref="NetworkLogEventDroppedReason.MaxMessageSizeOverflow"/>
        internal static readonly NetworkLogEventDroppedEventArgs MaxMessageSizeOverflow = new NetworkLogEventDroppedEventArgs(NetworkLogEventDroppedReason.MaxMessageSizeOverflow);

        /// <inheritdoc cref="NetworkLogEventDroppedReason.MaxConnectionsOverflow"/>
        internal static readonly NetworkLogEventDroppedEventArgs MaxConnectionsOverflow = new NetworkLogEventDroppedEventArgs(NetworkLogEventDroppedReason.MaxConnectionsOverflow);

        /// <inheritdoc cref="NetworkLogEventDroppedReason.MaxQueueOverflow"/>
        internal static readonly NetworkLogEventDroppedEventArgs MaxQueueOverflow = new NetworkLogEventDroppedEventArgs(NetworkLogEventDroppedReason.MaxQueueOverflow);

        /// <inheritdoc cref="NetworkLogEventDroppedReason.NetworkError"/>
        internal static readonly NetworkLogEventDroppedEventArgs NetworkErrorDetected = new NetworkLogEventDroppedEventArgs(NetworkLogEventDroppedReason.NetworkError);

        /// <summary>
        /// Creates new instance of NetworkTargetLogEventDroppedEventArgs
        /// </summary>
        public NetworkLogEventDroppedEventArgs(NetworkLogEventDroppedReason reason)
        {
            Reason = reason;
        }

        /// <summary>
        /// The reason why log was dropped
        /// </summary>
        public NetworkLogEventDroppedReason Reason { get; }
    }
}
