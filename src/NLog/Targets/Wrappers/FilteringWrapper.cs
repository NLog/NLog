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
using System.Diagnostics;

using NLog.Internal;
using System.Net;
using System.Net.Sockets;

using NLog.Config;
using NLog.Conditions;

namespace NLog.Targets.Wrappers
{
    /// <summary>
    /// A target wrapper that filters log entries based on a condition.
    /// </summary>
    /// <example>
    /// <p>This example causes the messages not contains the string '1' to be ignored.</p>
    /// <p>
    /// To set up the target in the <a href="config.html">configuration file</a>, 
    /// use the following syntax:
    /// </p>
    /// <code lang="XML" src="examples/targets/Configuration File/FilteringWrapper/NLog.config" />
    /// <p>
    /// The above examples assume just one target and a single rule. See below for
    /// a programmatic configuration that's equivalent to the above config file:
    /// </p>
    /// <code lang="C#" src="examples/targets/Configuration API/FilteringWrapper/Simple/Example.cs" />
    /// </example>
    [Target("FilteringWrapper", IgnoresLayout = true, IsWrapper = true)]
    public class FilteringTargetWrapper: WrapperTargetBase
    {
        private ConditionExpression _condition;

        /// <summary>
        /// Creates a new instance of <see cref="FilteringTargetWrapper"/>.
        /// </summary>
        public FilteringTargetWrapper()
        {
        }

        /// <summary>
        /// Creates a new instance of <see cref="FilteringTargetWrapper"/> and 
        /// initializes the <see cref="WrapperTargetBase.WrappedTarget"/> and
        /// <see cref="Condition"/> properties.
        /// </summary>
        public FilteringTargetWrapper(Target writeTo, string condition)
        {
            WrappedTarget = writeTo;
            Condition = condition;
        }

        /// <summary>
        /// Condition expression. Log events who meet this condition will be forwarded 
        /// to the wrapped target.
        /// </summary>
        [RequiredParameter]
        [AcceptsCondition]
        public string Condition
        {
            get { return _condition.ToString(); }
            set { _condition = ConditionParser.ParseExpression(value); }
        }

        /// <summary>
        /// Checks the condition against the passed log event.
        /// If the condition is met, the log event is forwarded to
        /// the wrapped target.
        /// </summary>
        /// <param name="logEvent">Log event.</param>
        protected internal override void Write(LogEventInfo logEvent)
        {
            object v = _condition.Evaluate(logEvent);
            if (v != null && v is bool && (bool)v)
                WrappedTarget.Write(logEvent);
        }

        /// <summary>
        /// Adds all layouts used by this target to the specified collection.
        /// </summary>
        /// <param name="layouts">The collection to add layouts to.</param>
        public override void PopulateLayouts(LayoutCollection layouts)
        {
            base.PopulateLayouts(layouts);
            _condition.PopulateLayouts(layouts);
        }

    }
}
