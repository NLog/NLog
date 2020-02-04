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
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Threading;
    using NLog.Common;
    using NLog.Internal;

    /// <summary>
    /// Retries in case of write error.
    /// </summary>
    /// <seealso href="https://github.com/nlog/nlog/wiki/RetryingWrapper-target">Documentation on NLog Wiki</seealso>
    /// <example>
    /// <p>This example causes each write attempt to be repeated 3 times, 
    /// sleeping 1 second between attempts if first one fails.</p>
    /// <p>
    /// To set up the target in the <a href="config.html">configuration file</a>, 
    /// use the following syntax:
    /// </p>
    /// <code lang="XML" source="examples/targets/Configuration File/RetryingWrapper/NLog.config" />
    /// <p>
    /// The above examples assume just one target and a single rule. See below for
    /// a programmatic configuration that's equivalent to the above config file:
    /// </p>
    /// <code lang="C#" source="examples/targets/Configuration API/RetryingWrapper/Simple/Example.cs" />
    /// </example>
    [Target("RetryingWrapper", IsWrapper = true)]
    public class RetryingTargetWrapper : WrapperTargetBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RetryingTargetWrapper" /> class.
        /// </summary>
        public RetryingTargetWrapper()
            : this(null, 3, 100)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RetryingTargetWrapper" /> class.
        /// </summary>
        /// <param name="name">Name of the target.</param>
        /// <param name="wrappedTarget">The wrapped target.</param>
        /// <param name="retryCount">The retry count.</param>
        /// <param name="retryDelayMilliseconds">The retry delay milliseconds.</param>
        public RetryingTargetWrapper(string name, Target wrappedTarget, int retryCount, int retryDelayMilliseconds)
            : this(wrappedTarget, retryCount, retryDelayMilliseconds)
        {
            Name = name;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RetryingTargetWrapper" /> class.
        /// </summary>
        /// <param name="wrappedTarget">The wrapped target.</param>
        /// <param name="retryCount">The retry count.</param>
        /// <param name="retryDelayMilliseconds">The retry delay milliseconds.</param>
        public RetryingTargetWrapper(Target wrappedTarget, int retryCount, int retryDelayMilliseconds)
        {
            WrappedTarget = wrappedTarget;
            RetryCount = retryCount;
            RetryDelayMilliseconds = retryDelayMilliseconds;
            OptimizeBufferReuse = GetType() == typeof(RetryingTargetWrapper);   // Class not sealed, reduce breaking changes
        }

        /// <summary>
        /// Gets or sets the number of retries that should be attempted on the wrapped target in case of a failure.
        /// </summary>
        /// <docgen category='Retrying Options' order='10' />
        [DefaultValue(3)]
        public int RetryCount { get; set; }

        /// <summary>
        /// Gets or sets the time to wait between retries in milliseconds.
        /// </summary>
        /// <docgen category='Retrying Options' order='10' />
        [DefaultValue(100)]
        public int RetryDelayMilliseconds { get; set; }

        /// <summary>
        /// Special SyncObject to allow closing down Target while busy retrying
        /// </summary>
        private readonly object _retrySyncObject = new object();

        /// <summary>
        /// Writes the specified log event to the wrapped target, retrying and pausing in case of an error.
        /// </summary>
        /// <param name="logEvents">The log event.</param>
        protected override void WriteAsyncThreadSafe(IList<AsyncLogEventInfo> logEvents)
        {
            lock (_retrySyncObject)
            {
                for (int i = 0; i < logEvents.Count; ++i)
                {
                    if (!IsInitialized)
                        logEvents[i].Continuation(null);
                    else 
                        WriteAsyncThreadSafe(logEvents[i]);
                }
            }
        }

        /// <summary>
        /// Writes the specified log event to the wrapped target in a thread-safe manner.
        /// </summary>
        /// <param name="logEvent">The log event.</param>
        protected override void WriteAsyncThreadSafe(AsyncLogEventInfo logEvent)
        {
            lock (_retrySyncObject)
            {
                // Uses RetrySyncObject instead of Target.SyncRoot to allow closing target while doing sleep and retry.
                Write(logEvent);
            }
        }

        /// <summary>
        /// Writes the specified log event to the wrapped target, retrying and pausing in case of an error.
        /// </summary>
        /// <param name="logEvent">The log event.</param>
        protected override void Write(AsyncLogEventInfo logEvent)
        {
            AsyncContinuation continuation = null;
            int counter = 0;

            continuation = ex =>
            {
                if (ex == null)
                {
                    logEvent.Continuation(null);
                    return;
                }

                int retryNumber = Interlocked.Increment(ref counter);
                InternalLogger.Warn(ex, "RetryingWrapper(Name={0}): Error while writing to '{1}'. Try {2}/{3}", Name, WrappedTarget, retryNumber, RetryCount);

                // exceeded retry count
                if (retryNumber >= RetryCount)
                {
                    InternalLogger.Warn("Too many retries. Aborting.");
                    logEvent.Continuation(ex);
                    return;
                }

                // sleep and try again (Check every 100 ms if target have been closed)
                for (int i = 0; i < RetryDelayMilliseconds;)
                {
                    int retryDelay = Math.Min(100, RetryDelayMilliseconds - i);
                    AsyncHelpers.WaitForDelay(TimeSpan.FromMilliseconds(retryDelay));
                    i += retryDelay;
                    if (!IsInitialized)
                    {
                        InternalLogger.Warn("RetryingWrapper(Name={0}): Target closed. Aborting.", Name);
                        logEvent.Continuation(ex);
                        return;
                    }
                }

                WrappedTarget.WriteAsyncLogEvent(logEvent.LogEvent.WithContinuation(continuation));
            };

            WrappedTarget.WriteAsyncLogEvent(logEvent.LogEvent.WithContinuation(continuation));
        }
    }
}
