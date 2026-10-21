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
    using System.Threading;
    using NLog.Common;

    /// <summary>
    /// <see cref="IDispatcher"/> that invokes the callback from a <see cref="Timer"/>.
    /// </summary>
    internal sealed class TimerDispatcher : IDispatcher
    {
        private readonly object _lockObject = new object();
        private Timer? _timer;
        private bool _isSet;
        private bool _stopped;

        /// <inheritdoc/>
        public bool Set(Action callback)
        {
            if (callback is null)
                throw new ArgumentNullException(nameof(callback));

            lock (_lockObject)
            {
                if (_isSet || _stopped)
                    throw new InvalidOperationException("The dispatcher has already been set.");

                _timer = new Timer(_ => callback(), null, Timeout.Infinite, Timeout.Infinite);
                _isSet = true;
                return true;
            }
        }

        /// <inheritdoc/>
        public bool Change(int dueTime, int period)
        {
            lock (_lockObject)
            {
                if (!_isSet)
                    throw new InvalidOperationException("The dispatcher has not been initialized. Call the Set method to initialize the dispatcher");
                if (_stopped || _timer is null)
                    return false;

                return _timer.Change(dueTime, period);
            }
        }

        /// <inheritdoc/>
        public bool Stop(TimeSpan timeout)
        {
            Timer? timer;
            lock (_lockObject)
            {
                _stopped = true;
                timer = _timer;
                _timer = null;
            }

            // Wait outside the lock, so a callback calling Change() cannot deadlock against Stop()
            return timer?.WaitForDispose(timeout) ?? true;
        }
    }
}
