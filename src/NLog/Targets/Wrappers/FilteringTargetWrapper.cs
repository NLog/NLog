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
    using NLog.Conditions;
    using NLog.Config;
    using NLog.Internal;

    /// <summary>
    /// Filters log entries based on a condition.
    /// </summary>
    /// <seealso href="https://github.com/nlog/nlog/wiki/FilteringWrapper-target">Documentation on NLog Wiki</seealso>
    /// <example>
    /// <p>This example causes the messages not contains the string '1' to be ignored.</p>
    /// <p>
    /// To set up the target in the <a href="config.html">configuration file</a>, 
    /// use the following syntax:
    /// </p>
    /// <code lang="XML" source="examples/targets/Configuration File/FilteringWrapper/NLog.config" />
    /// <p>
    /// The above examples assume just one target and a single rule. See below for
    /// a programmatic configuration that's equivalent to the above config file:
    /// </p>
    /// <code lang="C#" source="examples/targets/Configuration API/FilteringWrapper/Simple/Example.cs" />
    /// </example>
    [Target("FilteringWrapper", IsWrapper = true)]
    public class FilteringTargetWrapper : WrapperTargetBase
    {
        private static readonly object boxedBooleanTrue = true;

        /// <summary>
        /// Initializes a new instance of the <see cref="FilteringTargetWrapper" /> class.
        /// </summary>
        public FilteringTargetWrapper()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FilteringTargetWrapper" /> class.
        /// </summary>
        /// <param name="name">Name of the target.</param>
        /// <param name="wrappedTarget">The wrapped target.</param>
        /// <param name="condition">The condition.</param>
        public FilteringTargetWrapper(string name, Target wrappedTarget, ConditionExpression condition)
            : this(wrappedTarget, condition)
        {
            this.Name = name;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FilteringTargetWrapper" /> class.
        /// </summary>
        /// <param name="wrappedTarget">The wrapped target.</param>
        /// <param name="condition">The condition.</param>
        public FilteringTargetWrapper(Target wrappedTarget, ConditionExpression condition)
        {
            this.WrappedTarget = wrappedTarget;
            this.Condition = condition;
        }

        /// <summary>
        /// Gets or sets the condition expression. Log events who meet this condition will be forwarded 
        /// to the wrapped target.
        /// </summary>
        /// <docgen category='Filtering Options' order='10' />
        [RequiredParameter]
        public ConditionExpression Condition { get; set; }

        /// <summary>
        /// Checks the condition against the passed log event.
        /// If the condition is met, the log event is forwarded to
        /// the wrapped target.
        /// </summary>
        /// <param name="logEvent">Log event.</param>
        protected override void Write(AsyncLogEventInfo logEvent)
        {
            object v = this.Condition.Evaluate(logEvent.LogEvent);
            if (boxedBooleanTrue.Equals(v))
            {
                this.WrappedTarget.WriteAsyncLogEvent(logEvent);
            }
            else
            {
                logEvent.Continuation(null);
            }
        }
    }
}
