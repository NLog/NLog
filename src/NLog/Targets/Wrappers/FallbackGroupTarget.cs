// 
// Copyright (c) 2004-2016 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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
    using NLog.Common;
    using NLog.Internal;

    /// <summary>
    /// Provides fallback-on-error.
    /// </summary>
    /// <seealso href="https://github.com/nlog/nlog/wiki/FallbackGroup-target">Documentation on NLog Wiki</seealso>
    /// <example>
    /// <p>This example causes the messages to be written to server1, 
    /// and if it fails, messages go to server2.</p>
    /// <p>
    /// To set up the target in the <a href="config.html">configuration file</a>, 
    /// use the following syntax:
    /// </p>
    /// <code lang="XML" source="examples/targets/Configuration File/FallbackGroup/NLog.config" />
    /// <p>
    /// The above examples assume just one target and a single rule. See below for
    /// a programmatic configuration that's equivalent to the above config file:
    /// </p>
    /// <code lang="C#" source="examples/targets/Configuration API/FallbackGroup/Simple/Example.cs" />
    /// </example>
    [Target("FallbackGroup", IsCompound = true)]
    public class FallbackGroupTarget : CompoundTargetBase
    {
        private int currentTarget;
        private object lockObject = new object();

        /// <summary>
        /// Initializes a new instance of the <see cref="FallbackGroupTarget"/> class.
        /// </summary>
        public FallbackGroupTarget()
            : this(new Target[0])
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
            this.Name = name;
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
        /// Forwards the log event to the sub-targets until one of them succeeds.
        /// </summary>
        /// <param name="logEvent">The log event.</param>
        /// <remarks>
        /// The method remembers the last-known-successful target
        /// and starts the iteration from it.
        /// If <see cref="ReturnToFirstOnSuccess"/> is set, the method
        /// resets the target to the first target
        /// stored in <see cref="Targets"/>.
        /// </remarks>
        protected override void Write(AsyncLogEventInfo logEvent)
        {
            AsyncContinuation continuation = null;
            int tryCounter = 0;
            int targetToInvoke;

            continuation = ex =>
                {
                    if (ex == null)
                    {
                        // success
                        lock (this.lockObject)
                        {
                            if (this.currentTarget != 0)
                            {
                                if (this.ReturnToFirstOnSuccess)
                                {
                                    InternalLogger.Debug("Fallback: target '{0}' succeeded. Returning to the first one.", this.Targets[this.currentTarget]);
                                    this.currentTarget = 0;
                                }
                            }
                        }

                        logEvent.Continuation(null);
                        return;
                    }

                    // failure
                    lock (this.lockObject)
                    {
                        InternalLogger.Warn(ex, "Fallback: target '{0}' failed. Proceeding to the next one.", this.Targets[this.currentTarget]);

                        // error while writing, go to the next one
                        this.currentTarget = (this.currentTarget + 1) % this.Targets.Count;

                        tryCounter++;
                        targetToInvoke = this.currentTarget;
                        if (tryCounter >= this.Targets.Count)
                        {
                            targetToInvoke = -1;
                        }
                    }

                    if (targetToInvoke >= 0)
                    {
                        this.Targets[targetToInvoke].WriteAsyncLogEvent(logEvent.LogEvent.WithContinuation(continuation));
                    }
                    else
                    {
                        logEvent.Continuation(ex);
                    }
                };

            lock (this.lockObject)
            {
                targetToInvoke = this.currentTarget;
            }

            this.Targets[targetToInvoke].WriteAsyncLogEvent(logEvent.LogEvent.WithContinuation(continuation));
        }
    }
}
