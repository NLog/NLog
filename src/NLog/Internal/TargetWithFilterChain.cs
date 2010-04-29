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
using System.Collections;
using System.Text;

using NLog.Targets;
using NLog.Filters;

namespace NLog.Internal
{
    internal class TargetWithFilterChain
    {
        private Target _target;
        private FilterCollection _filterChain;
        private TargetWithFilterChain _next;
        private int _needsStackTrace = 0;

        public TargetWithFilterChain(Target a, FilterCollection filterChain)
        {
            _target = a;
            _filterChain = filterChain;
            _needsStackTrace = 0;
        }

        public Target Target
        {
            get { return _target; }
        }

        public int NeedsStackTrace
        {
            get { return _needsStackTrace; }
            set { _needsStackTrace = value; }
        }

        public FilterCollection FilterChain
        {
            get { return _filterChain; }
        }

        public TargetWithFilterChain Next
        {
            get { return _next; }
            set { _next = value; }
        }

        public void PrecalculateNeedsStackTrace()
        {
            _needsStackTrace = 0;

            for (TargetWithFilterChain awf = this; awf != null; awf = awf.Next)
            {
                if (_needsStackTrace >= 2)
                    break;
                Target app = awf.Target;

                int nst = app.NeedsStackTrace();
                _needsStackTrace = Math.Max(_needsStackTrace, nst);

                FilterCollection filterChain = awf.FilterChain;

                for (int i = 0; i < filterChain.Count; ++i)
                {
                    Filter filter = filterChain[i];

                    nst = filter.NeedsStackTrace();
                    _needsStackTrace = Math.Max(_needsStackTrace, nst);
                }
            }

        }
    }
}
