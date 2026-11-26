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
    /// Channel that writes on its own dedicated background thread, so it keeps working when the .NET ThreadPool is starved:
    /// the first write into an empty queue runs the consumer after timeToSleepBetweenBatches,
    /// then it keeps writing until the queue is empty, and waits for the next first write.
    /// </summary>
    /// <remarks>
    /// The lock is only held while waiting for the next run and while updating state.
    /// The consumer always runs outside the lock, so writers never wait for a slow write.
    /// </remarks>
    internal sealed class ChannelDedicatedThread : AsyncLogEventChannel
    {
        private readonly object _lockObject = new object();
        private readonly int _timeToSleepBetweenBatches;
        private readonly string _threadName;
        private readonly Thread _thread;
        private bool _started;
        private bool _stopped;

        /// <summary>
        /// Current <see cref="ChannelRunState"/>. Scheduled means a run is due at <see cref="_dueAtTicks"/>.
        /// Only read or changed while holding <see cref="_lockObject"/>.
        /// </summary>
        private ChannelRunState _state = ChannelRunState.Idle;

        /// <summary>
        /// A first write happened while the consumer was running.
        /// </summary>
        private bool _firstWriteDuringRun;

        private int _dueAtTicks;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChannelDedicatedThread"/> class.
        /// </summary>
        /// <param name="queueLimit">Maximum number of log events in the queue.</param>
        /// <param name="overflowAction">The action to be taken when the queue overflows.</param>
        /// <param name="consume">Writes queued log events, and reports what it found.</param>
        /// <param name="timeToSleepBetweenBatches">Milliseconds from the first write until writing starts.</param>
        /// <param name="threadName">Name of the dedicated thread.</param>
        public ChannelDedicatedThread(int queueLimit, AsyncTargetWrapperOverflowAction overflowAction, Func<ChannelConsumeResult> consume, int timeToSleepBetweenBatches, string threadName)
            : base(queueLimit, overflowAction, consume)
        {
            _timeToSleepBetweenBatches = timeToSleepBetweenBatches;
            _threadName = string.IsNullOrEmpty(threadName) ? nameof(ChannelDedicatedThread) : threadName;
            _thread = new Thread(RunLoop) { IsBackground = true, Name = _threadName };
        }

        private int FirstWriteDelay => Math.Max(_timeToSleepBetweenBatches, 0);

        /// <inheritdoc/>
        public override void Start()
        {
            lock (_lockObject)
            {
                if (_started || _stopped)
                    return;

                _started = true;
                if (!IsEmpty)
                    Schedule(FirstWriteDelay);     // Events written before start
            }

            _thread.Start();
        }

        /// <inheritdoc/>
        public override bool Stop(TimeSpan timeout)
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

            if ((_thread.ThreadState & ThreadState.Unstarted) != 0)
                return true;    // Thread.Start failed (or has not run yet); once started it sees _stopped and exits

            return _thread.Join(timeout);
        }

        /// <inheritdoc/>
        public override string ToString() => _threadName;

        /// <inheritdoc/>
        protected override void OnFirstWrite()
        {
            lock (_lockObject)
            {
                if (_stopped || !_started)
                    return;

                switch (_state)
                {
                    case ChannelRunState.Running:
                        _firstWriteDuringRun = true;
                        break;

                    case ChannelRunState.Idle:
                        Schedule(FirstWriteDelay);
                        Monitor.Pulse(_lockObject);
                        break;

                    // Scheduled: a run is already coming, nothing to do
                }
            }
        }

        /// <summary>
        /// Moves to Scheduled with the next run due in <paramref name="delayMs"/>. Caller holds <see cref="_lockObject"/>.
        /// </summary>
        private void Schedule(int delayMs)
        {
            _state = ChannelRunState.Scheduled;
            _dueAtTicks = unchecked(Environment.TickCount + delayMs);
        }

        private void RunLoop()
        {
            while (WaitForNextRun())
            {
                var result = RunConsumer();
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
                    if (_state != ChannelRunState.Scheduled)
                    {
                        Monitor.Wait(_lockObject);
                        continue;
                    }

                    int remainingMs = unchecked(_dueAtTicks - Environment.TickCount);
                    if (remainingMs > 0)
                    {
                        Monitor.Wait(_lockObject, remainingMs);
                        continue;
                    }

                    _state = ChannelRunState.Running;
                    _firstWriteDuringRun = false;   // This run reads the queue, so earlier first writes are covered
                    return true;
                }

                return false;
            }
        }

        /// <summary>
        /// Decides the next run after a consumer run. Returns <see langword="false"/> when stopped.
        /// </summary>
        private bool ScheduleNext(ChannelConsumeResult result)
        {
            lock (_lockObject)
            {
                _state = ChannelRunState.Idle;
                if (_stopped)
                    return false;

                switch (result)
                {
                    case ChannelConsumeResult.MoreQueued:
                        Schedule(0);    // Keep writing until empty
                        break;

                    case ChannelConsumeResult.NoProgress:
                        // Nothing could be written: retry later instead of a busy loop
                        Schedule(Math.Max(_timeToSleepBetweenBatches, 1));
                        break;

                    default:
                        if (_firstWriteDuringRun)
                        {
                            // A first write during the run may have come after the consumer found the queue empty
                            _firstWriteDuringRun = false;
                            Schedule(FirstWriteDelay);
                        }
                        break;
                }

                return true;
            }
        }
    }
}
