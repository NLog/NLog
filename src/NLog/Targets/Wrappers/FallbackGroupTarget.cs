// 
// Copyright (c) 2004-2021 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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
    using System.Collections.Generic;
    using System.Threading;
    using NLog.Common;

    /// <summary>
    /// Provides fallback-on-error.
    /// </summary>
    /// <remarks>
    /// <a href="https://github.com/nlog/nlog/wiki/FallbackGroup-target">See NLog Wiki</a>
    /// </remarks>
    /// <seealso href="https://github.com/nlog/nlog/wiki/FallbackGroup-target">Documentation on NLog Wiki</seealso>
    /// <example>
    /// <p>This example causes the messages to be written to server1, 
    /// and if it fails, messages go to server2.</p>
    /// <p>
    /// To set up the target in the <a href="https://github.com/NLog/NLog/wiki/Configuration-file">configuration file</a>, 
    /// use the following syntax:
    /// </p>
    /// <code lang="XML" source="examples/targets/Configuration File/FallbackGroup/NLog.config" />
    /// <p>
    /// To set up the log target programmatically use code like this:
    /// </p>
    /// <code lang="C#" source="examples/targets/Configuration API/FallbackGroup/Simple/Example.cs" />
    /// </example>
    [Target("FallbackGroup", IsCompound = true)]
    public class FallbackGroupTarget : CompoundTargetBase
    {
        private long _currentTarget;

        /// <summary>
        /// Initializes a new instance of the <see cref="FallbackGroupTarget"/> class.
        /// </summary>
        public FallbackGroupTarget()
            : this(NLog.Internal.ArrayHelper.Empty<Target>())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FallbackGroupTarget"/> class.
        /// </summary>
        /// <param name="name">Name of the target.</param>
        /// <param name="targets">The targets.</param>
        public FallbackGroupTarget(string name, params Target[] targets)
            : this(targets)
        {
            Name = name;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FallbackGroupTarget" /> class.
        /// </summary>
        /// <param name="targets">The targets.</param>
        public FallbackGroupTarget(params Target[] targets)
            : base(targets)
        {
        }

        /// <summary>
        /// Gets or sets a value indicating whether to return to the first target after any successful write.
        /// </summary>
        /// <docgen category='Fallback Options' order='10' />
        public bool ReturnToFirstOnSuccess { get; set; }

        /// <summary>
        /// Gets or sets whether to enable batching, but fallback will be handled individually
        /// </summary>
        /// <docgen category='Fallback Options' order='50' />
        public bool EnableBatchWrite { get; set; } = true;

        /// <summary>
        /// Forwards the log event to the sub-targets until one of them succeeds.
        /// </summary>
        /// <param name="logEvents">The log event.</param>
        protected override void WriteAsyncThreadSafe(IList<AsyncLogEventInfo> logEvents)
        {
            if (logEvents.Count == 1)
            {
                WriteAsyncThreadSafe(logEvents[0]);
            }
            else if (EnableBatchWrite)
            {
                var targetToInvoke = (int)Interlocked.Read(ref _currentTarget);

                for (int i = 0; i < logEvents.Count; ++i)
                {
                    logEvents[i] = WrapWithFallback(logEvents[i], targetToInvoke);
                }

                Targets[targetToInvoke].WriteAsyncLogEvents(logEvents);
            }
            else
            {
                for (int i = 0; i < logEvents.Count; ++i)
                {
                    WriteAsyncThreadSafe(logEvents[i]);
                }
            }
        }

        /// <inheritdoc/>
        protected override void WriteAsyncThreadSafe(AsyncLogEventInfo logEvent)
        {
            Write(logEvent);
        }

        /// <summary>
        /// Forwards the log event to the sub-targets until one of them succeeds.
        /// </summary>
        protected override void Write(AsyncLogEventInfo logEvent)
        {
            var targetToInvoke = (int)Interlocked.Read(ref _currentTarget);
            var result = WrapWithFallback(logEvent, targetToInvoke);
            Targets[targetToInvoke].WriteAsyncLogEvent(result);
        }

        private AsyncLogEventInfo WrapWithFallback(AsyncLogEventInfo logEvent, int targetToInvoke)
        {
            for (int i = 0; i < Targets.Count; ++i)
            {
                if (i != targetToInvoke)
                {
                    Targets[i].PrecalculateVolatileLayouts(logEvent.LogEvent);
                }
            }

            AsyncContinuation continuation = null;
            int tryCounter = 0;
            continuation = ex =>
            {
                if (ex is null)
                {
                    // success
                    if (ReturnToFirstOnSuccess && Interlocked.Read(ref _currentTarget) != 0)
                    {
                        InternalLogger.Debug("{0}: Target '{1}' succeeded. Returning to the first one.", this, Targets[targetToInvoke]);
                        Interlocked.Exchange(ref _currentTarget, 0);
                    }

                    logEvent.Continuation(null);
                    return;
                }

                // error while writing, fallback to next one
                tryCounter++;
                int nextTarget = (targetToInvoke + 1) % Targets.Count;
                Interlocked.CompareExchange(ref _currentTarget, nextTarget, targetToInvoke);
                if (tryCounter < Targets.Count)
                {
                    InternalLogger.Warn(ex, "{0}: Target '{1}' failed. Fallback to next: `{2}`", this, Targets[targetToInvoke], Targets[nextTarget]);
                    targetToInvoke = nextTarget;
                    Targets[targetToInvoke].WriteAsyncLogEvent(logEvent.LogEvent.WithContinuation(continuation));
                }
                else
                {
                    InternalLogger.Warn(ex, "{0}: Target '{1}' failed. Fallback not possible", this, Targets[targetToInvoke]);
                    logEvent.Continuation(ex);
                }
            };

            return logEvent.LogEvent.WithContinuation(continuation);
        }
    }
}
