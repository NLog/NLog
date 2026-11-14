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

namespace NLog.Targets.Wrappers
{
    using System;
    using System.Threading;
    using NLog.Common;

    /// <summary>
    /// One-shot <see cref="Timer"/> on the .NET ThreadPool for the timer-based channels. No scheduling policy:
    /// the channel decides when to <see cref="Schedule"/> the next callback.
    /// </summary>
    internal sealed class ChannelTimer
    {
        [ThreadStatic]
        private static ChannelTimer? _currentCallbackTimer;

        private readonly object _lockObject = new object();
        private readonly TimerCallback _callback;
        private readonly Timer _timer;
        private bool _stopped;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChannelTimer"/> class.
        /// </summary>
        /// <param name="callback">Called on the ThreadPool when the scheduled time is reached.</param>
        public ChannelTimer(TimerCallback callback)
        {
            _callback = callback ?? throw new ArgumentNullException(nameof(callback));
            _timer = new Timer(OnTimer, this, Timeout.Infinite, Timeout.Infinite);
        }

        /// <summary>
        /// Schedules one callback after <paramref name="dueTime"/> milliseconds. Ignored after <see cref="Stop"/>.
        /// </summary>
        public void Schedule(int dueTime)
        {
            lock (_lockObject)
            {
                if (!_stopped)
                    _timer.Change(dueTime, Timeout.Infinite);
            }
        }

        /// <summary>
        /// Stops the timer, and waits for a callback in progress to complete.
        /// </summary>
        /// <returns><see langword="true"/> when fully stopped within <paramref name="timeout"/>. Calling it again returns <see langword="true"/>.</returns>
        public bool Stop(TimeSpan timeout)
        {
            lock (_lockObject)
            {
                if (_stopped)
                    return true;
                _stopped = true;
            }

            // Stop from inside the callback
            if (ReferenceEquals(_currentCallbackTimer, this))
            {
                _timer.Dispose();
                return true;
            }

            return _timer.WaitForDispose(timeout);
        }

        private static void OnTimer(object? state)
        {
            var channelTimer = (ChannelTimer)state!;
            lock (channelTimer._lockObject)
            {
                if (channelTimer._stopped)
                    return;
            }

            var previousTimer = _currentCallbackTimer;
            _currentCallbackTimer = channelTimer;
            try
            {
                channelTimer._callback(null);
            }
            finally
            {
                _currentCallbackTimer = previousTimer;
            }
        }
    }
}
