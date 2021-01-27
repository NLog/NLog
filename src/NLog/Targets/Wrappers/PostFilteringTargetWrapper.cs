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
    using NLog.Common;
    using NLog.Conditions;
    using NLog.Config;
    using NLog.Internal;

    /// <summary>
    /// Filters buffered log entries based on a set of conditions that are evaluated on a group of events.
    /// </summary>
    /// <seealso href="https://github.com/nlog/nlog/wiki/PostFilteringWrapper-target">Documentation on NLog Wiki</seealso>
    /// <remarks>
    /// PostFilteringWrapper must be used with some type of buffering target or wrapper, such as
    /// AsyncTargetWrapper, BufferingWrapper or ASPNetBufferingWrapper.
    /// </remarks>
    /// <example>
    /// <p>
    /// This example works like this. If there are no Warn,Error or Fatal messages in the buffer
    /// only Info messages are written to the file, but if there are any warnings or errors, 
    /// the output includes detailed trace (levels &gt;= Debug). You can plug in a different type
    /// of buffering wrapper (such as ASPNetBufferingWrapper) to achieve different
    /// functionality.
    /// </p>
    /// <p>
    /// To set up the target in the <a href="config.html">configuration file</a>, 
    /// use the following syntax:
    /// </p>
    /// <code lang="XML" source="examples/targets/Configuration File/PostFilteringWrapper/NLog.config" />
    /// <p>
    /// The above examples assume just one target and a single rule. See below for
    /// a programmatic configuration that's equivalent to the above config file:
    /// </p>
    /// <code lang="C#" source="examples/targets/Configuration API/PostFilteringWrapper/Simple/Example.cs" />
    /// </example>
    [Target("PostFilteringWrapper", IsWrapper = true)]
    public class PostFilteringTargetWrapper : WrapperTargetBase
    {
        private static object boxedTrue = true;

        /// <summary>
        /// Initializes a new instance of the <see cref="PostFilteringTargetWrapper" /> class.
        /// </summary>
        public PostFilteringTargetWrapper()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PostFilteringTargetWrapper" /> class.
        /// </summary>
        public PostFilteringTargetWrapper(Target wrappedTarget)
            : this(null, wrappedTarget)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PostFilteringTargetWrapper" /> class.
        /// </summary>
        /// <param name="name">Name of the target.</param>
        /// <param name="wrappedTarget">The wrapped target.</param>
        public PostFilteringTargetWrapper(string name, Target wrappedTarget)
        {
            Name = name;
            WrappedTarget = wrappedTarget;
            Rules = new List<FilteringRule>();
        }

        /// <summary>
        /// Gets or sets the default filter to be applied when no specific rule matches.
        /// </summary>
        /// <docgen category='Filtering Options' order='10' />
        public ConditionExpression DefaultFilter { get; set; }

        /// <summary>
        /// Gets the collection of filtering rules. The rules are processed top-down
        /// and the first rule that matches determines the filtering condition to
        /// be applied to log events.
        /// </summary>
        /// <docgen category='Filtering Rules' order='10' />
        [ArrayParameter(typeof(FilteringRule), "when")]
        public IList<FilteringRule> Rules { get; private set; }

        /// <inheritdoc/>
        protected override void Write(AsyncLogEventInfo logEvent)
        {
            Write((IList<AsyncLogEventInfo>)new[] { logEvent });  // Single LogEvent should also work
        }

        /// <summary>
        /// Evaluates all filtering rules to find the first one that matches.
        /// The matching rule determines the filtering condition to be applied
        /// to all items in a buffer. If no condition matches, default filter
        /// is applied to the array of log events.
        /// </summary>
        /// <param name="logEvents">Array of log events to be post-filtered.</param>
        protected override void Write(IList<AsyncLogEventInfo> logEvents)
        {
            InternalLogger.Trace("{0}: Running on {1} events", this, logEvents.Count);

            var resultFilter = EvaluateAllRules(logEvents) ?? DefaultFilter;
            if (resultFilter == null)
            {
                WrappedTarget.WriteAsyncLogEvents(logEvents);
            }
            else
            {
                InternalLogger.Trace("{0}: Filter to apply: {1}", this, resultFilter);
                var resultBuffer = logEvents.Filter(resultFilter, (logEvent, filter) => ApplyFilter(logEvent, filter));
                InternalLogger.Trace("{0}: After filtering: {1} events.", this, resultBuffer.Count);
                if (resultBuffer.Count > 0)
                {
                    InternalLogger.Trace("{0}: Sending to {1}", this, WrappedTarget);
                    WrappedTarget.WriteAsyncLogEvents(resultBuffer);
                }
            }
        }

        private static bool ApplyFilter(AsyncLogEventInfo logEvent, ConditionExpression resultFilter)
        {
            object v = resultFilter.Evaluate(logEvent.LogEvent);
            if (boxedTrue.Equals(v))
            {
                return true;
            }
            else
            {
                logEvent.Continuation(null);
                return false;
            }
        }

        /// <summary>
        /// Evaluate all the rules to get the filtering condition
        /// </summary>
        /// <param name="logEvents"></param>
        /// <returns></returns>
        private ConditionExpression EvaluateAllRules(IList<AsyncLogEventInfo> logEvents)
        {
            if (Rules.Count == 0)
                return null;

            for (int i = 0; i < logEvents.Count; ++i)
            {
                for (int j = 0; j < Rules.Count; ++j)
                {
                    var rule = Rules[j];
                    object v = rule.Exists.Evaluate(logEvents[i].LogEvent);
                    if (boxedTrue.Equals(v))
                    {
                        InternalLogger.Trace("{0}: Rule matched: {1}", this, rule.Exists);
                        return rule.Filter;
                    }
                }
            }

            return null;
        }
    }
}
