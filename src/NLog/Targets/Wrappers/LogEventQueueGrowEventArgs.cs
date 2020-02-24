// 
// Copyright (c) 2004-2020 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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

    /// <summary> 
    /// Raises by  <see cref="AsyncRequestQueue"/> when 
    /// queue is full
    /// and <see cref="AsyncRequestQueueBase.OnOverflow"/> set to <see cref="AsyncTargetWrapperOverflowAction.Grow"/>
    /// By default queue doubles it size.
    /// </summary>
    public class LogEventQueueGrowEventArgs : EventArgs
    {
        /// <summary>
        /// Contains <see cref="AsyncRequestQueue"/> items count and new queue size.
        /// </summary>
        /// <param name="newQueueSize">Required queue size</param>
        /// <param name="requestsCount">Current queue size</param>
        public LogEventQueueGrowEventArgs(long newQueueSize, long requestsCount)
        {
            NewQueueSize = newQueueSize;
            RequestsCount = requestsCount;
        }

        /// <summary>
        /// New queue size
        /// </summary>
        public long NewQueueSize { get; }

        /// <summary>
        /// Current requests count
        /// </summary>
        public long RequestsCount { get; }
    }
}