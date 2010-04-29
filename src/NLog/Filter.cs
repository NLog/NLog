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

using NLog.Config;

namespace NLog
{
    /// <summary>
    /// An abstract filter class. Provides a way to eliminate log messages
    /// based on properties other than logger name and log level.
    /// </summary>
    public abstract class Filter
    {
        /// <summary>
        /// 
        /// </summary>
        protected Filter(){}

        private FilterResult _filterResult = FilterResult.Neutral;

        /// <summary>
        /// The <see cref="FilterResult"/> value that should be returned 
        /// when this filter matches.
        /// </summary>
        protected FilterResult Result
        {
            get { return _filterResult; }
        }

        /// <summary>
        /// User-requested action to be taken when filter matches.
        /// </summary>
        /// <remarks>
        /// Allowed values are <c>log</c>, <c>ignore</c>, <c>neutral</c>.
        /// </remarks>
        [RequiredParameter]
        public FilterResult Action
        {
            get { return _filterResult; }
            set { _filterResult = value; }
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
        protected internal abstract FilterResult Check(LogEventInfo logEvent);

        /// <summary>
        /// Determines whether stack trace information should be gathered
        /// during log event processing. 
        /// </summary>
        /// <returns>0 - don't include stack trace<br/>1 - include stack trace without source file information<br/>2 - include full stack trace</returns>
        public virtual int NeedsStackTrace()
        {
            return 0;
        }
    }
}
