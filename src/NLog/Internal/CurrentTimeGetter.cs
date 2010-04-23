// 
// Copyright (c) 2004-2010 Jaroslaw Kowalski <jaak@jkowalski.net>
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
    using System;

    /// <summary>
    /// Optimized methods to get current time.
    /// </summary>
    internal class CurrentTimeGetter
    {
        private static GetDelegate getDelegate;
        private static int lastTicks = -1;
        private static DateTime lastDateTime = DateTime.MinValue;

        /// <summary>
        /// Initializes static members of the CurrentTimeGetter class.
        /// </summary>
        static CurrentTimeGetter()
        {
            // this is to keep Mono compiler quiet
            getDelegate = new GetDelegate(NonOptimizedGet);
            getDelegate = new GetDelegate(ThrottledGet);
        }

        private delegate DateTime GetDelegate();

        /// <summary>
        /// Gets the current time in an optimized fashion.
        /// </summary>
        /// <value>Current time.</value>
        public static DateTime Now
        {
            get { return getDelegate(); }
        }

        private static DateTime NonOptimizedGet()
        {
            return DateTime.Now;
        }

        private static DateTime ThrottledGet()
        {
            int t = Environment.TickCount;

            if (t != lastTicks)
            {
                DateTime dt = DateTime.Now;

                lastTicks = t;
                lastDateTime = dt;
                return dt;
            }
            else
            {
                return lastDateTime;
            }
        }
    }
}
