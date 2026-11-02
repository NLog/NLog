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
    using System.Collections.Generic;
    using System.Threading;
    using NLog.Common;
    using NLog.Layouts;

    /// <summary>
    /// Limits the number of messages written per timespan to the wrapped target.
    /// </summary>
    /// <remarks>
    /// <a href="https://github.com/NLog/NLog/wiki/LimitingWrapper-target">See NLog Wiki</a>
    /// </remarks>
    /// <seealso href="https://github.com/NLog/NLog/wiki/LimitingWrapper-target">Documentation on NLog Wiki</seealso>
    [Target("LimitingWrapper", IsWrapper = true)]
    public class LimitingTargetWrapper : WrapperTargetBase
    {
        private readonly object _rolloverLock = new object();

        private volatile IntervalRollState _intervalRollState = new IntervalRollState(DateTime.MinValue);

        /// <summary>
        /// Initializes a new instance of the <see cref="LimitingTargetWrapper" /> class.
        /// </summary>
        public LimitingTargetWrapper()
        {
            MessageLimit = 1000;
            Interval = TimeSpan.FromHours(1);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LimitingTargetWrapper" /> class.
        /// </summary>
        /// <param name="name">The name of the target.</param>
        /// <param name="wrappedTarget">The wrapped target.</param>
        public LimitingTargetWrapper(string name, Target wrappedTarget)
            : this(wrappedTarget, 1000, TimeSpan.FromHours(1))
        {
            Name = name ?? Name;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LimitingTargetWrapper" /> class.
        /// </summary>
        /// <param name="wrappedTarget">The wrapped target.</param>
        public LimitingTargetWrapper(Target wrappedTarget)
            : this(wrappedTarget, 1000, TimeSpan.FromHours(1))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LimitingTargetWrapper" /> class.
        /// </summary>
        /// <param name="wrappedTarget">The wrapped target.</param>
        /// <param name="messageLimit">Maximum number of messages written per interval.</param>
        /// <param name="interval">Interval in which the maximum number of messages can be written.</param>
        public LimitingTargetWrapper(Target wrappedTarget, int messageLimit, TimeSpan interval)
        {
            Name = (wrappedTarget is null || string.IsNullOrEmpty(wrappedTarget.Name)) ? Name : (wrappedTarget.Name + "_wrapper");
            WrappedTarget = wrappedTarget;
            MessageLimit = messageLimit;
            Interval = interval;
        }

        /// <summary>
        /// Gets or sets the maximum allowed number of messages written per <see cref="Interval"/>.
        /// </summary>
        /// <remarks>Default: <see langword="1000"/> . Messages received after <see cref="MessageLimit"/> has been reached in the current <see cref="Interval"/> will be discarded.</remarks>
        /// <docgen category='General Options' order='10' />
        public Layout<int> MessageLimit { get; set; }

        /// <summary>
        /// Gets or sets the interval in which messages will be written up to the <see cref="MessageLimit"/> number of messages.
        /// </summary>
        /// <remarks>Default: <see langword="1"/> hour. Messages received after <see cref="MessageLimit"/> has been reached in the current <see cref="Interval"/> will be discarded.</remarks>
        /// <docgen category='General Options' order='10' />
        public Layout<TimeSpan> Interval { get; set; }

        /// <summary>
        /// Gets the number of logevents processed in the current <see cref="Interval"/>.
        /// </summary>
        public int MessagesWrittenCount
        {
            get
            {
                var count = Interlocked.Read(ref _intervalRollState.MessagesWrittenCount);
                return count <= 0 ? 0 : (int)Math.Min(count, int.MaxValue);
            }
        }

        /// <inheritdoc />
        protected override void InitializeTarget()
        {
            if (MessageLimit.IsFixed && MessageLimit.FixedValue <= 0)
                throw new NLogConfigurationException("The LimitingTargetWrapper MessageLimit property must be > 0.");

            if (Interval.IsFixed && Interval.FixedValue <= TimeSpan.Zero)
                throw new NLogConfigurationException("The LimitingTargetWrapper property Interval must be > 0.");

            base.InitializeTarget();

            lock (_rolloverLock)
            {
                _intervalRollState = new IntervalRollState(DateTime.MinValue);
            }

            InternalLogger.Trace("{0}: Initialized with MessageLimit={1} and Interval={2}.", this, MessageLimit, Interval);
        }

        /// <inheritdoc />
        protected sealed override void WriteAsyncThreadSafe(IList<AsyncLogEventInfo> logEvents)
        {
            for (int i = 0; i < logEvents.Count; ++i)
            {
                WriteAsyncThreadSafe(logEvents[i]);
            }
        }

        /// <inheritdoc />
        protected sealed override void WriteAsyncThreadSafe(AsyncLogEventInfo logEvent)
        {
            Write(logEvent);
        }

        /// <summary>
        /// Writes log event to the wrapped target if the current <see cref="MessagesWrittenCount"/> is lower than <see cref="MessageLimit"/>.
        /// If the <see cref="MessageLimit"/> is already reached, no log event will be written to the wrapped target.
        /// <see cref="MessagesWrittenCount"/> resets when the current <see cref="Interval"/> is expired.
        /// </summary>
        /// <param name="logEvent">Log event to be written out.</param>
        protected override void Write(AsyncLogEventInfo logEvent)
        {
            var logEventInfo = logEvent.LogEvent;
            var timestamp = logEventInfo.TimeStamp;
            var messageLimit = MessageLimit.RenderValue(logEventInfo);
            var intervalDuration = Interval.RenderValue(logEventInfo);

            while (true)
            {
                var state = _intervalRollState;
                // Do not roll the interval backward for out-of-order events
                if (timestamp > state.IntervalStart && timestamp - state.IntervalStart > intervalDuration)
                {
                    ResetInterval(state, timestamp, intervalDuration);
                    continue;
                }

                var count = Interlocked.Increment(ref state.MessagesWrittenCount);
                if (count < 0)
                    continue;   // Negative means state has rolled, retry with latest state.

                // Reject if the limit has been reached.
                if (messageLimit > 0 && count > messageLimit)
                {
                    logEvent.Continuation(null);
                    InternalLogger.Trace("{0}: Discarded event, because MessageLimit of '{1}' was reached.", this, messageLimit);
                    return;
                }

                WrappedTarget?.WriteAsyncLogEvent(logEvent);
                return;
            }
        }

        private void ResetInterval(IntervalRollState observedState, DateTime intervalStart, TimeSpan intervalDuration)
        {
            long messagesWrittenCount = 0;

            lock (_rolloverLock)
            {
                var current = _intervalRollState;
                if (!ReferenceEquals(current, observedState))
                    return; // We have been raced by other thread in the reset.

                // Close the old state before publishing the replacement.
                messagesWrittenCount = Interlocked.Exchange(ref current.MessagesWrittenCount, IntervalRollState.MarkedAsClosed);
                _intervalRollState = new IntervalRollState(intervalStart);
            }

            InternalLogger.Debug("{0}: New interval started with {1} secs duration. Previous Interval LogEvent Count={2}", this, intervalDuration.TotalSeconds, messagesWrittenCount < 0 ? -1 : messagesWrittenCount);
        }

        private sealed class IntervalRollState
        {
            public const long MarkedAsClosed = long.MinValue;
            public readonly DateTime IntervalStart;
            public long MessagesWrittenCount;

            public IntervalRollState(DateTime intervalStart)
            {
                IntervalStart = intervalStart;
            }
        }
    }
}
