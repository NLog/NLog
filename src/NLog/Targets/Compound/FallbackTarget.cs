// 
// Copyright (c) 2004-2009 Jaroslaw Kowalski <jaak@jkowalski.net>
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

using System;

using NLog.Internal;

namespace NLog.Targets.Compound
{
    /// <summary>
    /// A compound target that provides fallback-on-error functionality.
    /// </summary>
    /// <example>
    /// <p>This example causes the messages to be written to server1, 
    /// and if it fails, messages go to server2.</p>
    /// <p>
    /// To set up the target in the <a href="config.html">configuration file</a>, 
    /// use the following syntax:
    /// </p>
    /// <code lang="XML" src="examples/targets/Configuration File/FallbackGroup/NLog.config" />
    /// <p>
    /// The above examples assume just one target and a single rule. See below for
    /// a programmatic configuration that's equivalent to the above config file:
    /// </p>
    /// <code lang="C#" src="examples/targets/Configuration API/FallbackGroup/Simple/Example.cs" />
    /// </example>
    [Target("FallbackGroup", IsCompound = true)]
    public class FallbackTarget : CompoundTargetBase
    {
        private int currentTarget = 0;
        private bool returnToFirstOnSuccess = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="FallbackTarget"/> class.
        /// </summary>
        public FallbackTarget()
        {
        }

        /// <summary>
        /// Initializes a new instance of the FallbackTarget class.
        /// </summary>
        /// <param name="targets">The targets.</param>
        public FallbackTarget(params Target[] targets)
            : base(targets)
        {
        }

        /// <summary>
        /// Gets or sets a value indicating whether to return to the first target after any successful write.
        /// </summary>
        public bool ReturnToFirstOnSuccess
        {
            get { return this.returnToFirstOnSuccess; }
            set { this.returnToFirstOnSuccess = value; }
        }

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
        protected internal override void Write(LogEventInfo logEvent)
        {
            lock (this)
            {
                for (int i = 0; i < this.Targets.Count; ++i)
                {
                    try
                    {
                        this.Targets[this.currentTarget].Write(logEvent);
                        if (this.currentTarget != 0)
                        {
                            if (this.ReturnToFirstOnSuccess)
                            {
                                InternalLogger.Debug("Fallback: target '{0}' succeeded. Returning to the first one.", this.Targets[this.currentTarget]);
                                this.currentTarget = 0;
                            }
                        }

                        return;
                    }
                    catch (Exception ex)
                    {
                        InternalLogger.Warn("Fallback: target '{0}' failed. Proceeding to the next one. Error was: {1}", this.Targets[this.currentTarget], ex);

                        // error while writing, try another one
                        this.currentTarget = (this.currentTarget + 1) % this.Targets.Count;
                    }
                }
            }
        }
    }
}
