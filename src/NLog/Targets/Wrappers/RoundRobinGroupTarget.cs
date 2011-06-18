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
    using System.Threading;
    using NLog.Common;
    using NLog.Internal;

    /// <summary>
    /// Distributes log events to targets in a round-robin fashion.
    /// </summary>
    /// <seealso href="http://nlog-project.org/wiki/RoundRobinGroup_target">Documentation on NLog Wiki</seealso>
    /// <example>
    /// <p>This example causes the messages to be written to either file1.txt or file2.txt.
    /// Each odd message is written to file2.txt, each even message goes to file1.txt.
    /// </p>
    /// <p>
    /// To set up the target in the <a href="config.html">configuration file</a>, 
    /// use the following syntax:
    /// </p>
    /// <code lang="XML" source="examples/targets/Configuration File/RoundRobinGroup/NLog.config" />
    /// <p>
    /// The above examples assume just one target and a single rule. See below for
    /// a programmatic configuration that's equivalent to the above config file:
    /// </p>
    /// <code lang="C#" source="examples/targets/Configuration API/RoundRobinGroup/Simple/Example.cs" />
    /// </example>
    [Target("RoundRobinGroup", IsCompound = true)]
    public class RoundRobinGroupTarget : CompoundTargetBase
    {
        private int currentTarget = 0;
        private object lockObject = new object();

        /// <summary>
        /// Initializes a new instance of the <see cref="RoundRobinGroupTarget" /> class.
        /// </summary>
        public RoundRobinGroupTarget()
            : this(new Target[0])
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RoundRobinGroupTarget" /> class.
        /// </summary>
        /// <param name="targets">The targets.</param>
        public RoundRobinGroupTarget(params Target[] targets)
            : base(targets)
        {
        }

        /// <summary>
        /// Forwards the write to one of the targets from
        /// the <see cref="Targets"/> collection.
        /// </summary>
        /// <param name="logEvent">The log event.</param>
        /// <remarks>
        /// The writes are routed in a round-robin fashion.
        /// The first log event goes to the first target, the second
        /// one goes to the second target and so on looping to the
        /// first target when there are no more targets available.
        /// In general request N goes to Targets[N % Targets.Count].
        /// </remarks>
        protected override void Write(AsyncLogEventInfo logEvent)
        {
            if (this.Targets.Count == 0)
            {
                logEvent.Continuation(null);
                return;
            }

            int selectedTarget;

            lock (this.lockObject)
            {
                selectedTarget = this.currentTarget;
                this.currentTarget = (this.currentTarget + 1) % this.Targets.Count;
            }

            this.Targets[selectedTarget].WriteAsyncLogEvent(logEvent);
        }
    }
}
