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

using NLog;
using NLog.Config;

namespace NLog.Filters
{
    /// <summary>
    /// Matches when the calculated layout is NOT equal to the specified substring.
    /// This filter is deprecated in favour of <c>&lt;when /&gt;</c> which is based on <a href="conditions.html">contitions</a>
    /// </summary>
    [Filter("whenNotEqual")]
    public class WhenNotEqualFilter: LayoutBasedFilter
    {
        /// <summary>
        /// Initializes a new instance of the filter object.
        /// </summary>
        public WhenNotEqualFilter(){}

        private string _compareTo;

        /// <summary>
        /// String to compare the layout to.
        /// </summary>
        [RequiredParameter]
        public string CompareTo
        {
            get { return _compareTo; }
            set { _compareTo = value; }
        }

        private bool _ignoreCase = false;

        /// <summary>
        /// Ignore case when comparing strings.
        /// </summary>
        [System.ComponentModel.DefaultValue(false)]
        public bool IgnoreCase
        {
            get { return _ignoreCase; }
            set { _ignoreCase = value; }
        }

        /// <summary>
        /// Checks whether log event should be logged or not.
        /// </summary>
        /// <param name="logEvent">Log event.</param>
        /// <returns>
        /// <see cref="FilterResult.Ignore"/> - if the log event should be ignored<br/>
        /// <see cref="FilterResult.Neutral"/> - if the filter doesn't want to decide<br/>
        /// <see cref="FilterResult.Log"/> - if the log event should be logged<br/>
        /// </returns>
        protected internal override FilterResult Check(LogEventInfo logEvent)
        {
            if (0 != String.Compare(CompiledLayout.GetFormattedMessage(logEvent), CompareTo, IgnoreCase))
                return Result;
            else
                return FilterResult.Neutral;
        }
    }
}
