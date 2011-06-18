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
    using NLog.Conditions;
    using NLog.Config;

    /// <summary>
    /// Filtering rule for <see cref="PostFilteringTargetWrapper"/>.
    /// </summary>
    [NLogConfigurationItem]
    public class FilteringRule
    {
        /// <summary>
        /// Initializes a new instance of the FilteringRule class.
        /// </summary>
        public FilteringRule()
            : this(null, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the FilteringRule class.
        /// </summary>
        /// <param name="whenExistsExpression">Condition to be tested against all events.</param>
        /// <param name="filterToApply">Filter to apply to all log events when the first condition matches any of them.</param>
        public FilteringRule(ConditionExpression whenExistsExpression, ConditionExpression filterToApply)
        {
            this.Exists = whenExistsExpression;
            this.Filter = filterToApply;
        }

        /// <summary>
        /// Gets or sets the condition to be tested.
        /// </summary>
        /// <docgen category='Filtering Options' order='10' />
        [RequiredParameter]
        public ConditionExpression Exists { get; set; }

        /// <summary>
        /// Gets or sets the resulting filter to be applied when the condition matches.
        /// </summary>
        /// <docgen category='Filtering Options' order='10' />
        [RequiredParameter]
        public ConditionExpression Filter { get; set; }
    }
}
