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
    /// Channel that writes from a <see cref="Timer"/> callback on the .NET ThreadPool, driven by writes:
    /// the first write into an empty queue runs the consumer after timeToSleepBetweenBatches,
    /// then it keeps writing until the queue is empty, and waits for the next first write.
    /// </summary>
    /// <remarks>
    /// Each consumer run writes at most FullBatchSizeWriteLimit batches, then continues in a new timer callback,
    /// so a constant stream of log events does not occupy one ThreadPool thread forever.
    /// </remarks>
    internal sealed class ChannelWorkerThread : AsyncLogEventChannel
    {
        private readonly int _timeToSleepBetweenBatches;
        private readonly string _name;
        private readonly ChannelTimer _timer;

        /// <summary>
        /// Current <see cref="ChannelRunState"/>, stored as int because Interlocked does not support enum fields.
        /// Only change it through <see cref="SetState"/> and <see cref="TrySetState"/>.
        /// </summary>
        private int _state = (int)ChannelRunState.Idle;

        /// <summary>
        /// 1 when a first write happened since the current run started, otherwise 0.
        /// </summary>
        private int _firstWriteDuringRun;

        private int _started;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChannelWorkerThread"/> class.
        /// </summary>
        /// <param name="queueLimit">Maximum number of log events in the queue.</param>
        /// <param name="overflowAction">The action to be taken when the queue overflows.</param>
        /// <param name="consume">Writes queued log events, and reports what it found.</param>
        /// <param name="timeToSleepBetweenBatches">Milliseconds from the first write until writing starts.</param>
        /// <param name="name">Name used in internal logging.</param>
        public ChannelWorkerThread(int queueLimit, AsyncTargetWrapperOverflowAction overflowAction, Func<ChannelConsumeResult> consume, int timeToSleepBetweenBatches, string name)
            : base(queueLimit, overflowAction, consume)
        {
            _timeToSleepBetweenBatches = timeToSleepBetweenBatches;
            _name = name;
            _timer = new ChannelTimer(OnTimer);
        }

        private int FirstWriteDelay => Math.Max(_timeToSleepBetweenBatches, 0);

        /// <inheritdoc/>
        public override void Start()
        {
            Interlocked.Exchange(ref _started, 1);
            if (!IsEmpty)
                OnFirstWrite();     // Events written before start
        }

        /// <inheritdoc/>
        public override bool Stop(TimeSpan timeout) => _timer.Stop(timeout);

        /// <inheritdoc/>
        public override string ToString() => _name;

        /// <inheritdoc/>
        protected override void OnFirstWrite()
        {
            // Remember the first write. If a run is in progress, it checks this flag when it finds the queue empty.
            Interlocked.Exchange(ref _firstWriteDuringRun, 1);

            if (Interlocked.CompareExchange(ref _started, 0, 0) == 0)
                return;     // Start() checks for queued events

            // Only the caller that moves the channel out of Idle arms the timer. When it is already
            // Scheduled or Running, another run is coming (or the flag above will cause one).
            if (TrySetState(ChannelRunState.Idle, ChannelRunState.Scheduled))
                _timer.Schedule(FirstWriteDelay);
        }

        private void OnTimer(object? state)
        {
            SetState(ChannelRunState.Running);

            // This run reads the queue, so first writes before this point are covered by it
            Interlocked.Exchange(ref _firstWriteDuringRun, 0);

            switch (RunConsumer())
            {
                case ChannelConsumeResult.MoreQueued:
                    // Keep writing without delay, but in a new callback, so the ThreadPool thread is handed back
                    SetState(ChannelRunState.Scheduled);
                    _timer.Schedule(0);
                    break;

                case ChannelConsumeResult.NoProgress:
                    // Nothing could be written (missing target or dependency): retry later instead of a busy loop
                    SetState(ChannelRunState.Scheduled);
                    _timer.Schedule(Math.Max(_timeToSleepBetweenBatches, 1));
                    break;

                default:
                    // Queue was empty: from now on, a first write schedules the next run
                    SetState(ChannelRunState.Idle);

                    // A first write during the run may have come after the consumer found the queue empty.
                    // Take the flag, and schedule a run for it. TrySetState makes sure that a first write
                    // racing with us after the Idle above does not arm the timer a second time.
                    bool firstWriteDuringRun = Interlocked.Exchange(ref _firstWriteDuringRun, 0) == 1;
                    if (firstWriteDuringRun && TrySetState(ChannelRunState.Idle, ChannelRunState.Scheduled))
                        _timer.Schedule(FirstWriteDelay);
                    break;
            }
        }

        private void SetState(ChannelRunState state)
        {
            Interlocked.Exchange(ref _state, (int)state);
        }

        /// <summary>
        /// Atomically moves from <paramref name="from"/> to <paramref name="to"/>.
        /// Returns <see langword="false"/> (and changes nothing) when the channel was not in <paramref name="from"/>.
        /// </summary>
        private bool TrySetState(ChannelRunState from, ChannelRunState to)
        {
            return Interlocked.CompareExchange(ref _state, (int)to, (int)from) == (int)from;
        }
    }
}
