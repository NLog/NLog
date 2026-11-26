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

    /// <summary>
    /// Channel that writes from a <see cref="Timer"/> callback on the .NET ThreadPool on a fixed schedule:
    /// it wakes up every timeToSleepBetweenBatches milliseconds and writes what is queued. Writes do not trigger it.
    /// </summary>
    /// <remarks>
    /// The next tick is scheduled after each consumer run completes, so runs never overlap
    /// (a slow write delays the next tick instead of starting a second one).
    /// </remarks>
    internal sealed class ChannelTimerThread : AsyncLogEventChannel
    {
        private readonly int _timeToSleepBetweenBatches;
        private readonly string _name;
        private readonly ChannelTimer _timer;
        private int _started;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChannelTimerThread"/> class.
        /// </summary>
        /// <param name="queueLimit">Maximum number of log events in the queue.</param>
        /// <param name="overflowAction">The action to be taken when the queue overflows.</param>
        /// <param name="consume">Writes queued log events, and reports what it found.</param>
        /// <param name="timeToSleepBetweenBatches">Milliseconds between ticks. Must be above 1, to avoid a polling loop.</param>
        /// <param name="name">Name used in internal logging.</param>
        public ChannelTimerThread(int queueLimit, AsyncTargetWrapperOverflowAction overflowAction, Func<ChannelConsumeResult> consume, int timeToSleepBetweenBatches, string name)
            : base(queueLimit, overflowAction, consume)
        {
            if (timeToSleepBetweenBatches <= 1)
                throw new ArgumentOutOfRangeException(nameof(timeToSleepBetweenBatches), timeToSleepBetweenBatches, "The timer channel needs more than 1 ms between ticks.");

            _timeToSleepBetweenBatches = timeToSleepBetweenBatches;
            _name = name;
            _timer = new ChannelTimer(OnTimer);
        }

        /// <inheritdoc/>
        public override void Start()
        {
            if (Interlocked.Exchange(ref _started, 1) == 0)
                _timer.Schedule(_timeToSleepBetweenBatches);
        }

        /// <inheritdoc/>
        public override bool Stop(TimeSpan timeout) => _timer.Stop(timeout);

        /// <inheritdoc/>
        public override string ToString() => _name;

        /// <inheritdoc/>
        protected override void OnFirstWrite()
        {
            // Writes do not trigger the timer channel: the next tick checks the queue
        }

        private void OnTimer(object? state)
        {
            RunConsumer();
            _timer.Schedule(_timeToSleepBetweenBatches);    // Re-armed after the run, so ticks never overlap
        }
    }
}
