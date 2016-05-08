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
    using System.Collections.Generic;
    using System.Threading;
    using NLog.Common;
    using NLog.Internal;

    /// <summary>
    /// Writes log events to all targets.
    /// </summary>
    /// <seealso href="https://github.com/nlog/nlog/wiki/SplitGroup-target">Documentation on NLog Wiki</seealso>
    /// <example>
    /// <p>This example causes the messages to be written to both file1.txt or file2.txt 
    /// </p>
    /// <p>
    /// To set up the target in the <a href="config.html">configuration file</a>, 
    /// use the following syntax:
    /// </p>
    /// <code lang="XML" source="examples/targets/Configuration File/SplitGroup/NLog.config" />
    /// <p>
    /// The above examples assume just one target and a single rule. See below for
    /// a programmatic configuration that's equivalent to the above config file:
    /// </p>
    /// <code lang="C#" source="examples/targets/Configuration API/SplitGroup/Simple/Example.cs" />
    /// </example>
    [Target("SplitGroup", IsCompound = true)]
    public class SplitGroupTarget : CompoundTargetBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SplitGroupTarget" /> class.
        /// </summary>
        public SplitGroupTarget()
            : this(new Target[0])
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SplitGroupTarget" /> class.
        /// </summary>
        /// <param name="name">Name of the target.</param>
        /// <param name="targets">The targets.</param>
        public SplitGroupTarget(string name, params Target[] targets)
             : this(targets)
        {
            this.Name = name;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SplitGroupTarget" /> class.
        /// </summary>
        /// <param name="targets">The targets.</param>
        public SplitGroupTarget(params Target[] targets)
            : base(targets)
        {
        }

        /// <summary>
        /// Forwards the specified log event to all sub-targets.
        /// </summary>
        /// <param name="logEvent">The log event.</param>
        protected override void Write(AsyncLogEventInfo logEvent)
        {
            AsyncHelpers.ForEachItemSequentially(this.Targets, logEvent.Continuation, (t, cont) => t.WriteAsyncLogEvent(logEvent.LogEvent.WithContinuation(cont)));
        }

        /// <summary>
        /// Writes an array of logging events to the log target. By default it iterates on all
        /// events and passes them to "Write" method. Inheriting classes can use this method to
        /// optimize batch writes.
        /// </summary>
        /// <param name="logEvents">Logging events to be written out.</param>
        protected override void Write(AsyncLogEventInfo[] logEvents)
        {
            InternalLogger.Trace("Writing {0} events", logEvents.Length);

            for (int i = 0; i < logEvents.Length; ++i)
            {
                logEvents[i].Continuation = CountedWrap(logEvents[i].Continuation, this.Targets.Count);
            }

            foreach (var t in this.Targets)
            {
                InternalLogger.Trace("Sending {0} events to {1}", logEvents.Length, t);
                t.WriteAsyncLogEvents(logEvents);
            }
        }

        private static AsyncContinuation CountedWrap(AsyncContinuation originalContinuation, int counter)
        {
            if (counter == 1)
            {
                return originalContinuation;
            }

            var exceptions = new List<Exception>();

            AsyncContinuation wrapper =
                ex =>
                    {
                        if (ex != null)
                        {
                            lock (exceptions)
                            {
                                exceptions.Add(ex);
                            }
                        }

                        int c = Interlocked.Decrement(ref counter);

                        if (c == 0)
                        {
                            var combinedException = AsyncHelpers.GetCombinedException(exceptions);
                            InternalLogger.Trace("Combined exception: {0}", combinedException);
                            originalContinuation(combinedException);
                        }
                        else
                        {
                            InternalLogger.Trace("{0} remaining.", c);
                        }
                    };

            return wrapper;
        }
    }
}
