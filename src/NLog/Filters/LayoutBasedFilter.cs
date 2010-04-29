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
    /// A base class for filters that are based on comparing a value to a layout.
    /// </summary>
    public abstract class LayoutBasedFilter: Filter
    {
        /// <summary>
        /// Initializes a new instance of <see cref="LayoutBasedFilter"/>.
        /// </summary>
        protected LayoutBasedFilter(){}

        private Layout _compiledlayout;

        /// <summary>
        /// The layout text;
        /// </summary>
        [RequiredParameter]
        public string Layout
        {
            get { return _compiledlayout.Text; }
            set { _compiledlayout = new Layout(value); }
        }

        /// <summary>
        /// Compiled layout.
        /// </summary>
        protected Layout CompiledLayout
        {
            get { return _compiledlayout; }
        }

        /// <summary>
        /// Determines whether stack trace information should be gathered
        /// during log event processing. By default it calls <see cref="NLog.Layout.NeedsStackTrace" /> on
        /// <see cref="TargetWithLayout.CompiledLayout" />.
        /// </summary>
        /// <returns>0 - don't include stack trace<br/>1 - include stack trace without source file information<br/>2 - include full stack trace</returns>
        public override int NeedsStackTrace()
        {
            return CompiledLayout.NeedsStackTrace();
        }
    }
}
