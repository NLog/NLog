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
using System.Text;
using System.Diagnostics;
using System.Reflection;
using System.Data;
using System.Collections;

using NLog.Internal;
using NLog.Config;
using NLog.Conditions;

namespace NLog.Targets.Wrappers
{
    /// <summary>
    /// Filtering rule for <see cref="PostFilteringTargetWrapper"/>.
    /// </summary>
    public class FilteringRule
    {
        private ConditionExpression _exists;
        private ConditionExpression _filter;

        /// <summary>
        /// Creates a new instance of <see cref="FilteringRule"/>.
        /// </summary>
        public FilteringRule() {}

        /// <summary>
        /// Condition to be tested.
        /// </summary>
        [RequiredParameter]
        [AcceptsCondition]
        public string Exists
        {
            get { return _exists.ToString(); }
            set { _exists = ConditionParser.ParseExpression(value); }
        }

        /// <summary>
        /// Resulting filter to be applied when the condition matches.
        /// </summary>
        [RequiredParameter]
        [AcceptsCondition]
        public string Filter
        {
            get { return _filter.ToString(); }
            set { _filter = ConditionParser.ParseExpression(value); }
        }

        /// <summary>
        /// Parsed Filter condition.
        /// </summary>
        public ConditionExpression FilterCondition 
        {
            get { return _filter; }
            set { _filter = value; }
        }

        /// <summary>
        /// Parsed Exists condition.
        /// </summary>
        public ConditionExpression ExistsCondition
        {
            get { return _exists; }
            set { _exists = value; }
        }
    }
}
