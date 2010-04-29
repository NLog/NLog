// 
// Copyright (c) 2004-2006 Jaroslaw Kowalski <jaak@jkowalski.net>
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
using System.IO;
using System.Text;
using System.Xml;
using System.Reflection;
using System.Collections;
using System.Diagnostics;

using NLog.Internal;
using NLog.Config;
using NLog.Conditions;

namespace NLog.Targets.Wrappers
{
    /// <summary>
    /// A target wrapper that filters buffered log entries based on a set of conditions
    /// that are evaluated on all events.
    /// </summary>
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
    /// <code lang="XML" src="examples/targets/Configuration File/PostFilteringWrapper/NLog.config" />
    /// <p>
    /// The above examples assume just one target and a single rule. See below for
    /// a programmatic configuration that's equivalent to the above config file:
    /// </p>
    /// <code lang="C#" src="examples/targets/Configuration API/PostFilteringWrapper/Simple/Example.cs" />
    /// </example>
    [Target("PostFilteringWrapper", IgnoresLayout = true, IsWrapper = true)]
    public class PostFilteringTargetWrapper: WrapperTargetBase
    {
        private ConditionExpression _defaultFilter;
        private FilteringRuleCollection _rules = new FilteringRuleCollection();

        /// <summary>
        /// Creates a new instance of <see cref="PostFilteringTargetWrapper"/>.
        /// </summary>
        public PostFilteringTargetWrapper()
        {
        }

        /// <summary>
        /// Default filter to be applied when no specific rule matches.
        /// </summary>
        public string DefaultFilter
        {
            get { return _defaultFilter.ToString(); }
            set { _defaultFilter = ConditionParser.ParseExpression(value); }
        }

        /// <summary>
        /// Collection of filtering rules. The rules are processed top-down
        /// and the first rule that matches determines the filtering condition to
        /// be applied to log events.
        /// </summary>
        [ArrayParameter(typeof(FilteringRule), "when")]
        public FilteringRuleCollection Rules
        {
            get { return _rules; }
        }


        /// <summary>
        /// Evaluates all filtering rules to find the first one that matches.
        /// The matching rule determines the filtering condition to be applied
        /// to all items in a buffer. If no condition matches, default filter
        /// is applied to the array of log events.
        /// </summary>
        /// <param name="logEvents">Array of log events to be post-filtered.</param>
        protected internal override void Write(LogEventInfo[] logEvents)
        {
            ConditionExpression resultFilter = null;

            if (InternalLogger.IsTraceEnabled)
            {
                InternalLogger.Trace("Input: {0} events", logEvents.Length);
            }

            // evaluate all the rules to get the filtering condition

            for (int i = 0; i < logEvents.Length; ++i)
            {
                for (int j = 0; j < _rules.Count; ++j)
                {
                    object v = _rules[j].ExistsCondition.Evaluate(logEvents[i]);

                    if (v is bool && (bool)v)
                    {
                        if (InternalLogger.IsTraceEnabled)
                            InternalLogger.Trace("Rule matched: {0}", _rules[j].ExistsCondition);
                        resultFilter = _rules[j].FilterCondition;
                        break;
                    }
                }
                if (resultFilter != null)
                    break;
            }

            if (resultFilter == null)
                resultFilter = _defaultFilter;

            if (InternalLogger.IsTraceEnabled)
                InternalLogger.Trace("Filter to apply: {0}", resultFilter);

            // apply the condition to the buffer

            ArrayList resultBuffer = new ArrayList();

            for (int i = 0; i < logEvents.Length; ++i)
            {
                object v = resultFilter.Evaluate(logEvents[i]);
                if (v is bool && (bool)v)
                    resultBuffer.Add(logEvents[i]);
            }

            if (InternalLogger.IsTraceEnabled)
                InternalLogger.Trace("After filtering: {0} events", resultBuffer.Count);

            if (resultBuffer.Count > 0)
            {
                WrappedTarget.Write((LogEventInfo[])resultBuffer.ToArray(typeof(LogEventInfo)));
            }
        }

        /// <summary>
        /// Processes a single log event. Not very useful for this post-filtering
        /// wrapper.
        /// </summary>
        /// <param name="logEvent">Log event.</param>
        protected internal override void Write(LogEventInfo logEvent)
        {
            Write(new LogEventInfo[] { logEvent });
        }

        /// <summary>
        /// Adds all layouts used by this target to the specified collection.
        /// </summary>
        /// <param name="layouts">The collection to add layouts to.</param>
        public override void PopulateLayouts(LayoutCollection layouts)
        {
            base.PopulateLayouts(layouts);
            foreach (FilteringRule fr in Rules)
            {
                fr.FilterCondition.PopulateLayouts(layouts);
                fr.ExistsCondition.PopulateLayouts(layouts);
            }
            _defaultFilter.PopulateLayouts(layouts);
        }
    }
}
