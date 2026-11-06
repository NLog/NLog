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
    /// <see cref="IChannelReader"/> that runs the consumer on its own dedicated background thread,
    /// so it keeps working when the .NET ThreadPool is starved.
    /// </summary>
    /// <remarks>
    /// The lock is only held while waiting for the next run and while updating state.
    /// The consumer always runs outside the lock, so <see cref="Signal"/> never waits for a slow write.
    /// </remarks>
    internal sealed class DedicatedThreadChannelReader : IChannelReader
    {
        private readonly object _lockObject = new object();
        private readonly Func<ConsumeResult> _consume;
        private readonly int _timeToSleepBetweenBatches;
        private readonly string _threadName;
        private readonly Thread _thread;
        private bool _started;
        private bool _stopped;

        /// <summary>
        /// Current <see cref="ChannelReaderState"/>. Scheduled means a run is due at <see cref="_dueAtTicks"/>.
        /// Only read or changed while holding <see cref="_lockObject"/>.
        /// </summary>
        private ChannelReaderState _state = ChannelReaderState.Idle;

        /// <summary>
        /// <see cref="Signal"/> was called while the consumer was running.
        /// </summary>
        private bool _signaled;

        private int _dueAtTicks;

        /// <summary>
        /// Initializes a new instance of the <see cref="DedicatedThreadChannelReader"/> class.
        /// </summary>
        /// <param name="consume">Writes queued log events, and reports what it found.</param>
        /// <param name="timeToSleepBetweenBatches">Milliseconds between batches. 1 or less means run on new activity.</param>
        /// <param name="threadName">Name of the dedicated thread.</param>
        public DedicatedThreadChannelReader(Func<ConsumeResult> consume, int timeToSleepBetweenBatches, string threadName)
        {
            _consume = consume ?? throw new ArgumentNullException(nameof(consume));
            _timeToSleepBetweenBatches = timeToSleepBetweenBatches;
            _threadName = string.IsNullOrEmpty(threadName) ? nameof(DedicatedThreadChannelReader) : threadName;
            _thread = new Thread(RunLoop) { IsBackground = true, Name = _threadName };
        }

        /// <inheritdoc/>
        public void Start()
        {
            lock (_lockObject)
            {
                if (_started || _stopped)
                    return;

                _started = true;
                Schedule(ChannelReaderSchedule.StartDelay(_timeToSleepBetweenBatches));
            }

            // Long-lived thread must not capture the caller's AsyncLocal / ScopeContext state
            if (ExecutionContext.IsFlowSuppressed())
            {
                _thread.Start();
            }
            else
            {
                using (ExecutionContext.SuppressFlow())
                {
                    _thread.Start();
                }
            }
        }

        /// <inheritdoc/>
        public void Signal()
        {
            if (ChannelReaderSchedule.IsPeriodic(_timeToSleepBetweenBatches))
                return;

            lock (_lockObject)
            {
                if (_stopped)
                    return;

                switch (_state)
                {
                    case ChannelReaderState.Running:
                        // Remember it: checked when the running consumer reports the queue empty
                        _signaled = true;
                        break;

                    case ChannelReaderState.Idle:
                        // Nothing pending: schedule a run, and wake the thread waiting in WaitForNextRun
                        Schedule(ChannelReaderSchedule.SignalDelay(_timeToSleepBetweenBatches));
                        Monitor.Pulse(_lockObject);
                        break;

                    // Scheduled: a run is already coming, nothing to do
                }
            }
        }

        /// <inheritdoc/>
        public bool Stop(TimeSpan timeout)
        {
            bool started;
            lock (_lockObject)
            {
                _stopped = true;
                started = _started;
                Monitor.Pulse(_lockObject);
            }

            if (!started || ReferenceEquals(_thread, Thread.CurrentThread))
                return true;    // Never started, or Stop called from inside the consumer (avoid self-join)

            return _thread.Join(timeout);
        }

        /// <summary>
        /// Moves to Scheduled with the next run due in <paramref name="delayMs"/>. Caller holds <see cref="_lockObject"/>.
        /// </summary>
        private void Schedule(int delayMs)
        {
            _state = ChannelReaderState.Scheduled;
            _dueAtTicks = unchecked(Environment.TickCount + delayMs);
        }

        private void RunLoop()
        {
            while (WaitForNextRun())
            {
                var result = Consume();
                if (!ScheduleNext(result))
                    return;
            }
        }

        /// <summary>
        /// Waits until a consumer run is due. Returns <see langword="false"/> when stopped.
        /// </summary>
        private bool WaitForNextRun()
        {
            lock (_lockObject)
            {
                while (!_stopped)
                {
                    if (_state != ChannelReaderState.Scheduled)
                    {
                        // Idle: sleep until Signal() or Stop() pulses the lock
                        Monitor.Wait(_lockObject);
                        continue;
                    }

                    int remainingMs = unchecked(_dueAtTicks - Environment.TickCount);
                    if (remainingMs > 0)
                    {
                        // Not due yet: sleep until due, or until pulsed (the loop re-checks everything)
                        Monitor.Wait(_lockObject, remainingMs);
                        continue;
                    }

                    _state = ChannelReaderState.Running;
                    _signaled = false;  // This run reads the queue, so earlier signals are covered
                    return true;
                }

                return false;
            }
        }

        private ConsumeResult Consume()
        {
            try
            {
                return _consume();
            }
            catch (Exception ex)
            {
                if (ex.MustBeRethrownImmediately())
                    throw;

                InternalLogger.Error(ex, "{0}: Error in channel reader.", _threadName);
                return ConsumeResult.MorePending;   // Throttled retry
            }
        }

        /// <summary>
        /// Applies the scheduling rules after a consumer run. Returns <see langword="false"/> when stopped.
        /// </summary>
        private bool ScheduleNext(ConsumeResult result)
        {
            lock (_lockObject)
            {
                // Run finished: Idle unless a rule below schedules the next one
                _state = ChannelReaderState.Idle;
                if (_stopped)
                    return false;

                int delay = ChannelReaderSchedule.NextDelay(result, _timeToSleepBetweenBatches);
                if (delay != Timeout.Infinite)
                {
                    // More work (or periodic mode)
                    Schedule(delay);
                }
                else if (_signaled)
                {
                    // A signal arrived while running, and may have come after the consumer found the queue empty
                    _signaled = false;
                    Schedule(ChannelReaderSchedule.SignalDelay(_timeToSleepBetweenBatches));
                }

                return true;
            }
        }
    }
}
