// 
// Copyright (c) 2004-2020 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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
    using System.ComponentModel;
    using Common;
    using Time;


    /// <summary>
    /// Limits the number of messages written per timespan to the wrapped target.
    /// </summary>
    [Target("LimitingWrapper", IsWrapper = true)]
    public class LimitingTargetWrapper : WrapperTargetBase
    {
        private DateTime _firstWriteInInterval;

        /// <summary>
        /// Initializes a new instance of the <see cref="LimitingTargetWrapper" /> class.
        /// </summary>
        public LimitingTargetWrapper():this(null)
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="LimitingTargetWrapper" /> class.
        /// </summary>
        /// <param name="name">The name of the target.</param>
        /// <param name="wrappedTarget">The wrapped target.</param>
        public LimitingTargetWrapper(string name, Target wrappedTarget) 
            : this(wrappedTarget, 1000, TimeSpan.FromHours(1))
        {
            Name = name;
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
            MessageLimit = messageLimit;
            Interval = interval;
            WrappedTarget = wrappedTarget;
        }

        /// <summary>
        /// Gets or sets the maximum allowed number of messages written per <see cref="Interval"/>.
        /// </summary>
        /// <remarks>
        /// Messages received after <see cref="MessageLimit"/> has been reached in the current <see cref="Interval"/> will be discarded.
        /// </remarks>
        /// <docgen category='General Options' order='10' />
        [DefaultValue(1000)]
        public int MessageLimit { get; set; }

        /// <summary>
        /// Gets or sets the interval in which messages will be written up to the <see cref="MessageLimit"/> number of messages.
        /// </summary>
        /// <remarks>
        /// Messages received after <see cref="MessageLimit"/> has been reached in the current <see cref="Interval"/> will be discarded.
        /// </remarks>
        /// <docgen category='General Options' order='10' />
        [DefaultValue(typeof(TimeSpan), "01:00")]
        public TimeSpan Interval { get; set; }

        /// <summary>
        /// Gets the <c>DateTime</c> when the current <see cref="Interval"/> will be reset.
        /// </summary>
        /// <docgen category='General Options' order='10' />
        public DateTime IntervalResetsAt => _firstWriteInInterval + Interval;

        /// <summary>
        /// Gets the number of <see cref="AsyncLogEventInfo"/> written in the current <see cref="Interval"/>.
        /// </summary>
        /// <docgen category='General Options' order='10' />
        public int MessagesWrittenCount { get; private set; }

        /// <summary>
        /// Initializes the target and resets the current Interval and <see cref="MessagesWrittenCount"/>.
        ///  </summary>
        protected override void InitializeTarget()
        {
            if(MessageLimit<=0)
                throw new NLogConfigurationException("The LimitingTargetWrapper\'s MessageLimit property must be > 0.");
            if(Interval<=TimeSpan.Zero)
                throw new NLogConfigurationException("The LimitingTargetWrapper\'s property Interval must be > 0.");

            base.InitializeTarget();
            ResetInterval();
            InternalLogger.Trace("{0}: Initialized with MessageLimit={1} and Interval={2}.", this, MessageLimit, Interval);
        }


        /// <summary>
        /// Writes log event to the wrapped target if the current <see cref="MessagesWrittenCount"/> is lower than <see cref="MessageLimit"/>.
        /// If the <see cref="MessageLimit"/> is already reached, no log event will be written to the wrapped target.
        /// <see cref="MessagesWrittenCount"/> resets when the current <see cref="Interval"/> is expired.
        /// </summary>
        /// <param name="logEvent">Log event to be written out.</param>
        protected override void Write(AsyncLogEventInfo logEvent)
        {
            if (IsIntervalExpired())
            {
                ResetInterval();
                InternalLogger.Debug("{0}: New interval of '{1}' started.", this, Interval);
            }

            if (MessagesWrittenCount < MessageLimit)
            {
                WrappedTarget.WriteAsyncLogEvent(logEvent);
                MessagesWrittenCount++;
            }
            else
            {
                logEvent.Continuation(null);
                InternalLogger.Trace("{0}: Discarded event, because MessageLimit of '{1}' was reached.", this, MessageLimit);
            }
        }

        private void ResetInterval()
        {
            _firstWriteInInterval = TimeSource.Current.Time;
            MessagesWrittenCount = 0;
        }

        private bool IsIntervalExpired()
        {
            return TimeSource.Current.Time - _firstWriteInInterval > Interval;
        }

    }
}