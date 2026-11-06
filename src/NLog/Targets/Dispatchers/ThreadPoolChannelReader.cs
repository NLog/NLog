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
    using NLog.Internal;

    /// <summary>
    /// <see cref="IChannelReader"/> that runs the consumer from a <see cref="Timer"/> callback on the .NET ThreadPool.
    /// </summary>
    internal sealed class ThreadPoolChannelReader : IChannelReader
    {
        private readonly Func<ConsumeResult> _consume;
        private readonly int _timeToSleepBetweenBatches;
        private readonly string _name;
        private readonly object _timerLock = new object();
        private readonly Timer _timer;

        /// <summary>
        /// Current <see cref="ChannelReaderState"/>, stored as int because Interlocked does not support enum fields.
        /// Scheduled means the timer is armed. Only change it through <see cref="SetState"/> and <see cref="TrySetState"/>.
        /// </summary>
        private int _state = (int)ChannelReaderState.Idle;

        /// <summary>
        /// 1 when <see cref="Signal"/> was called since the current run started, otherwise 0.
        /// </summary>
        private int _signaled;

        private bool _stopped;
        private Thread? _consumerThread;

        /// <summary>
        /// Initializes a new instance of the <see cref="ThreadPoolChannelReader"/> class.
        /// </summary>
        /// <param name="consume">Writes queued log events, and reports what it found.</param>
        /// <param name="timeToSleepBetweenBatches">Milliseconds between batches. 1 or less means run on new activity.</param>
        /// <param name="name">Name used in internal logging.</param>
        public ThreadPoolChannelReader(Func<ConsumeResult> consume, int timeToSleepBetweenBatches, string name)
        {
            _consume = consume ?? throw new ArgumentNullException(nameof(consume));
            _timeToSleepBetweenBatches = timeToSleepBetweenBatches;
            _name = name;
            
            _timer = new Timer(OnTimer, null, Timeout.Infinite, Timeout.Infinite);
        }

        /// <inheritdoc/>
        public void Start()
        {
            // Only schedule the first run if a Signal() did not already schedule one
            if (TrySetState(ChannelReaderState.Idle, ChannelReaderState.Scheduled))
                ScheduleRun(ChannelReaderSchedule.StartDelay(_timeToSleepBetweenBatches));
        }

        /// <inheritdoc/>
        public void Signal()
        {
            if (ChannelReaderSchedule.IsPeriodic(_timeToSleepBetweenBatches))
                return;     // Periodic mode runs every timeToSleepBetweenBatches anyway

            // Remember the signal first. If a run is in progress, it checks this flag when it finds the queue empty.
            Interlocked.Exchange(ref _signaled, 1);

            // Only the caller that moves the reader out of Idle arms the timer. When the reader is already
            // Scheduled or Running, another run is coming (or the flag above will cause one), so do nothing.
            if (TrySetState(ChannelReaderState.Idle, ChannelReaderState.Scheduled))
                ScheduleRun(ChannelReaderSchedule.SignalDelay(_timeToSleepBetweenBatches));
        }

        /// <inheritdoc/>
        public bool Stop(TimeSpan timeout)
        {
            lock (_timerLock)
            {
                if (_stopped)
                    return true;
                _stopped = true;
            }

            if (ReferenceEquals(_consumerThread, Thread.CurrentThread))
            {
                // Stop from inside the consumer: waiting for the timer callback would wait for ourselves
                _timer.Dispose();
                return true;
            }

            return _timer.WaitForDispose(timeout);
        }

        private void ScheduleRun(int dueTime)
        {
            lock (_timerLock)
            {
                if (!_stopped)
                    _timer.Change(dueTime, Timeout.Infinite);
            }
        }

        private void OnTimer(object? state)
        {
            lock (_timerLock)
            {
                if (_stopped)
                    return;     // Timer fired just as Stop() was called: skip the run, Close drains the queue itself
            }

            SetState(ChannelReaderState.Running);

            // This run reads the queue, so signals sent before this point are covered by it.
            Interlocked.Exchange(ref _signaled, 0);

            ConsumeResult result;
            _consumerThread = Thread.CurrentThread;
            try
            {
                result = _consume();
            }
            catch (Exception ex)
            {
                if (ex.MustBeRethrownImmediately())
                    throw;

                InternalLogger.Error(ex, "{0}: Error in channel reader.", _name);
                result = ConsumeResult.MorePending;     // Throttled retry
            }
            finally
            {
                _consumerThread = null;
            }

            ScheduleNext(result);
        }

        private void ScheduleNext(ConsumeResult result)
        {
            int delay = ChannelReaderSchedule.NextDelay(result, _timeToSleepBetweenBatches);
            if (delay != Timeout.Infinite)
            {
                SetState(ChannelReaderState.Scheduled);
                ScheduleRun(delay);
                return;
            }

            SetState(ChannelReaderState.Idle);

            // A signal that arrived while running may have come after the consumer found the queue empty.
            // Take the flag (resetting it to 0), and schedule a run for it.
            bool signaledWhileRunning = Interlocked.Exchange(ref _signaled, 0) == 1;
            if (signaledWhileRunning && TrySetState(ChannelReaderState.Idle, ChannelReaderState.Scheduled))
                ScheduleRun(ChannelReaderSchedule.SignalDelay(_timeToSleepBetweenBatches));
        }

        private void SetState(ChannelReaderState state)
        {
            Interlocked.Exchange(ref _state, (int)state);
        }

        /// <summary>
        /// Atomically moves from <paramref name="from"/> to <paramref name="to"/>.
        /// Returns <see langword="false"/> (and changes nothing) when the reader was not in <paramref name="from"/>.
        /// </summary>
        private bool TrySetState(ChannelReaderState from, ChannelReaderState to)
        {
            return Interlocked.CompareExchange(ref _state, (int)to, (int)from) == (int)from;
        }
    }
}
