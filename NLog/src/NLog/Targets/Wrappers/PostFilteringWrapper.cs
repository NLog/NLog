// 
// Copyright (c) 2004,2005 Jaroslaw Kowalski <jkowalski@users.sourceforge.net>
// 
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
// * Neither the name of the Jaroslaw Kowalski nor the names of its 
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
    [Target("PostFilteringWrapper",IgnoresLayout=true,IsWrapper=true)]
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

        public string DefaultFilter
        {
            get { return _defaultFilter.ToString(); }
            set { _defaultFilter = ConditionParser.ParseExpression(value); }
        }

        [ArrayParameter(typeof(FilteringRule), "when")]
        public FilteringRuleCollection Rules
        {
            get { return _rules; }
        }

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

        protected internal override void Write(LogEventInfo logEvent)
        {
            Write(new LogEventInfo[] { logEvent });
        }

        public override void PopulateLayouts(LayoutCollection layouts)
        {
            base.PopulateLayouts(layouts);
            _defaultFilter.PopulateLayouts(layouts);
        }
    }
}
