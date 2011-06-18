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
    using NLog.Common;
    using NLog.Internal;

    /// <summary>
    /// Sends log messages to a randomly selected target.
    /// </summary>
    /// <seealso href="http://nlog-project.org/wiki/RandomizeGroup_target">Documentation on NLog Wiki</seealso>
    /// <example>
    /// <p>This example causes the messages to be written to either file1.txt or file2.txt 
    /// chosen randomly on a per-message basis.
    /// </p>
    /// <p>
    /// To set up the target in the <a href="config.html">configuration file</a>, 
    /// use the following syntax:
    /// </p>
    /// <code lang="XML" source="examples/targets/Configuration File/RandomizeGroup/NLog.config" />
    /// <p>
    /// The above examples assume just one target and a single rule. See below for
    /// a programmatic configuration that's equivalent to the above config file:
    /// </p>
    /// <code lang="C#" source="examples/targets/Configuration API/RandomizeGroup/Simple/Example.cs" />
    /// </example>
    [Target("RandomizeGroup", IsCompound = true)]
    public class RandomizeGroupTarget : CompoundTargetBase
    {
        private readonly Random random = new Random();

        /// <summary>
        /// Initializes a new instance of the <see cref="RandomizeGroupTarget" /> class.
        /// </summary>
        public RandomizeGroupTarget()
            : this(new Target[0])
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RandomizeGroupTarget" /> class.
        /// </summary>
        /// <param name="targets">The targets.</param>
        public RandomizeGroupTarget(params Target[] targets)
            : base(targets)
        {
        }

        /// <summary>
        /// Forwards the log event to one of the sub-targets.
        /// The sub-target is randomly chosen.
        /// </summary>
        /// <param name="logEvent">The log event.</param>
        protected override void Write(AsyncLogEventInfo logEvent)
        {
            if (this.Targets.Count == 0)
            {
                logEvent.Continuation(null);
                return;
            }

            int selectedTarget;

            lock (this.random)
            {
                selectedTarget = this.random.Next(this.Targets.Count);
            }

            this.Targets[selectedTarget].WriteAsyncLogEvent(logEvent);
        }
    }
}