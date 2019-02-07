// 
// Copyright (c) 2004-2019 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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
    using Common;
    using Internal;

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
            Name = name;
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
            AsyncHelpers.ForEachItemSequentially(Targets, logEvent.Continuation, (t, cont) => t.WriteAsyncLogEvent(logEvent.LogEvent.WithContinuation(cont)));
        }

        /// <summary>
        /// NOTE! Obsolete, instead override Write(IList{AsyncLogEventInfo} logEvents)
        ///
        /// Writes an array of logging events to the log target. By default it iterates on all
        /// events and passes them to "Write" method. Inheriting classes can use this method to
        /// optimize batch writes.
        /// </summary>
        /// <param name="logEvents">Logging events to be written out.</param>
        [Obsolete("Instead override Write(IList<AsyncLogEventInfo> logEvents. Marked obsolete on NLog 4.5")]
        protected override void Write(AsyncLogEventInfo[] logEvents)
        {
            Write((IList<AsyncLogEventInfo>)logEvents);
        }

        /// <summary>
        /// Writes an array of logging events to the log target. By default it iterates on all
        /// events and passes them to "Write" method. Inheriting classes can use this method to
        /// optimize batch writes.
        /// </summary>
        /// <param name="logEvents">Logging events to be written out.</param>
        protected override void Write(IList<AsyncLogEventInfo> logEvents)
        {
            InternalLogger.Trace("SplitGroup(Name={0}): Writing {1} events", Name, logEvents.Count);

            for (int i = 0; i < logEvents.Count; ++i)
            {
                AsyncLogEventInfo ev = logEvents[i];
                logEvents[i] = new AsyncLogEventInfo(ev.LogEvent, CountedWrap(ev.Continuation, Targets.Count));
            }

            for (int i = 0; i < Targets.Count; ++i)
            {
                InternalLogger.Trace("SplitGroup(Name={0}): Sending {1} events to {2}", Name, logEvents.Count, Targets[i]);

                var targetLogEvents = logEvents;
                if (i < Targets.Count - 1)
                {
                    // OptimizeBufferReuse = true, will change the input-array (so we make clones here)
                    AsyncLogEventInfo[] cloneLogEvents = new AsyncLogEventInfo[logEvents.Count];
                    logEvents.CopyTo(cloneLogEvents, 0);
                    targetLogEvents = cloneLogEvents;
                }

                Targets[i].WriteAsyncLogEvents(targetLogEvents);
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
