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
    /// Schedules invocations of a single callback, using <see cref="System.Threading.Timer"/>-like semantics.
    /// </summary>
    /// <remarks>
    /// Contract shared by all implementations:
    /// <see cref="Change"/> before <see cref="Set"/> throws <see cref="InvalidOperationException"/>;
    /// calling <see cref="Set"/> twice (or after <see cref="Stop"/>) throws <see cref="InvalidOperationException"/>;
    /// <see cref="Change"/> after <see cref="Stop"/> returns <c>false</c>;
    /// <see cref="Stop"/> is idempotent.
    /// </remarks>
    internal interface IDispatcher
    {
        /// <summary>
        /// Registers the callback. Does not schedule it.
        /// </summary>
        /// <param name="callback">The method that gets called by the dispatcher when it triggers.</param>
        /// <returns>true if the setup was successful.</returns>
        bool Set(Action callback);

        /// <summary>
        /// Changes when the callback is invoked next, and the interval between invocations.
        /// </summary>
        /// <param name="dueTime">
        /// Milliseconds to delay before invoking the callback. Zero (0) invokes it immediately.
        /// A negative number (<see cref="System.Threading.Timeout.Infinite"/>) disarms the dispatcher.
        /// </param>
        /// <param name="period">
        /// Milliseconds between invocations. Zero (0) or a negative number disables periodic signaling.
        /// </param>
        /// <returns>true if the change was applied; false if the dispatcher has been stopped.</returns>
        bool Change(int dueTime, int period);

        /// <summary>
        /// Stops the dispatcher and waits for an in-flight callback to complete.
        /// </summary>
        /// <param name="timeout">How long to wait for an in-flight callback.</param>
        /// <returns>true if the dispatcher fully stopped within <paramref name="timeout"/>.</returns>
        bool Stop(TimeSpan timeout);
    }
}
