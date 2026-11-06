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

namespace NLog.Targets.Dispatchers
{
    using System;

    /// <summary>
    /// Reader side of the AsyncTargetWrapper channel: decides when the consumer runs.
    /// </summary>
    /// <remarks>
    /// The consumer and the delay between batches are given in the constructor of the implementation.
    /// At most one consumer run is in progress at any time.
    /// </remarks>
    internal interface IChannelReader
    {
        /// <summary>
        /// Starts scheduling consumer runs.
        /// </summary>
        void Start();

        /// <summary>
        /// Notifies that log events are available. Cheap and safe to call from any thread.
        /// A signal that arrives while the consumer is running causes another run, if the running one found the queue empty.
        /// </summary>
        void Signal();

        /// <summary>
        /// Stops scheduling consumer runs, and waits for a consumer run in progress to complete.
        /// </summary>
        /// <param name="timeout">How long to wait for a consumer run in progress.</param>
        /// <returns><see langword="true"/> when fully stopped within <paramref name="timeout"/>. Calling it again returns <see langword="true"/>.</returns>
        bool Stop(TimeSpan timeout);
    }
}
