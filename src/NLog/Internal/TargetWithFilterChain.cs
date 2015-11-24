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

namespace NLog.Internal
{
    using System.Collections.Generic;
    using NLog.Config;
    using NLog.Filters;
    using NLog.Targets;

    /// <summary>
    /// Represents target with a chain of filters which determine
    /// whether logging should happen.
    /// </summary>
    [NLogConfigurationItem]
	internal class TargetWithFilterChain
	{
        private StackTraceUsage stackTraceUsage = StackTraceUsage.None;

        /// <summary>
        /// Initializes a new instance of the <see cref="TargetWithFilterChain" /> class.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="filterChain">The filter chain.</param>
        public TargetWithFilterChain(Target target, IList<Filter> filterChain)
        {
            this.Target = target;
            this.FilterChain = filterChain;
            this.stackTraceUsage = StackTraceUsage.None;
        }

        /// <summary>
        /// Gets the target.
        /// </summary>
        /// <value>The target.</value>
        public Target Target { get; private set; }

        /// <summary>
        /// Gets the filter chain.
        /// </summary>
        /// <value>The filter chain.</value>
        public IList<Filter> FilterChain { get; private set; }

        /// <summary>
        /// Gets or sets the next <see cref="TargetWithFilterChain"/> item in the chain.
        /// </summary>
        /// <value>The next item in the chain.</value>
        public TargetWithFilterChain NextInChain { get; set; }

        /// <summary>
        /// Gets the stack trace usage.
        /// </summary>
        /// <returns>A <see cref="StackTraceUsage" /> value that determines stack trace handling.</returns>
        public StackTraceUsage GetStackTraceUsage()
        {
            return this.stackTraceUsage;
        }

        internal void PrecalculateStackTraceUsage()
        {
            this.stackTraceUsage = StackTraceUsage.None;

            // find all objects which may need stack trace
            // and determine maximum
            foreach (var item in ObjectGraphScanner.FindReachableObjects<IUsesStackTrace>(this))
            {
                var stu = item.StackTraceUsage;

                if (stu > this.stackTraceUsage)
                {
                    this.stackTraceUsage = stu;

                    if (this.stackTraceUsage >= StackTraceUsage.Max)
                    {
                        break;
                    }
                }
            }
        }
    }
}
