// 
// Copyright (c) 2004-2011 Jaroslaw Kowalski <jaak@jkowalski.net>
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
    using System.Threading;
    using NLog.Common;
    using NLog.Internal;

    /// <summary>
    /// Retries in case of write error.
    /// </summary>
    /// <seealso href="http://nlog-project.org/wiki/RetryingWrapper_target">Documentation on NLog Wiki</seealso>
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
        /// <param name="wrappedTarget">The wrapped target.</param>
        /// <param name="retryCount">The retry count.</param>
        /// <param name="retryDelayMilliseconds">The retry delay milliseconds.</param>
        public RetryingTargetWrapper(Target wrappedTarget, int retryCount, int retryDelayMilliseconds)
        {
            this.WrappedTarget = wrappedTarget;
            this.RetryCount = retryCount;
            this.RetryDelayMilliseconds = retryDelayMilliseconds;
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
                    InternalLogger.Warn("Error while writing to '{0}': {1}. Try {2}/{3}", this.WrappedTarget, ex, retryNumber, this.RetryCount);

                    // exceeded retry count
                    if (retryNumber >= this.RetryCount)
                    {
                        InternalLogger.Warn("Too many retries. Aborting.");
                        logEvent.Continuation(ex);
                        return;
                    }

                    // sleep and try again
                    Thread.Sleep(this.RetryDelayMilliseconds);
                    this.WrappedTarget.WriteAsyncLogEvent(logEvent.LogEvent.WithContinuation(continuation));
                };

            this.WrappedTarget.WriteAsyncLogEvent(logEvent.LogEvent.WithContinuation(continuation));
        }
    }
}
