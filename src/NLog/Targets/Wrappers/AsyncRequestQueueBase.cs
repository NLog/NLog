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

using System;

namespace NLog.Targets.Wrappers
{
    using System.Collections.Generic;
    using NLog.Common;

    internal abstract class AsyncRequestQueueBase
    {
        public abstract bool IsEmpty { get; }

        /// <summary>
        /// Gets or sets the request limit.
        /// </summary>
        public int RequestLimit { get; set; }

        /// <summary>
        /// Gets or sets the action to be taken when there's no more room in
        /// the queue and another request is enqueued.
        /// </summary>
        public AsyncTargetWrapperOverflowAction OnOverflow { get; set; }

        /// <summary>
        /// Notifies about log event that was dropped when <see cref="OnOverflow"/> set to <see cref="AsyncTargetWrapperOverflowAction.Discard"/>
        /// </summary>
        public event EventHandler<LogEventDroppedEventArgs> LogEventDropped;

        /// <summary>
        /// Notifies when queue size is growing over <see cref="RequestLimit"/>
        /// </summary>
        public event EventHandler<LogEventQueueGrowEventArgs> LogEventQueueGrow;

        public abstract bool Enqueue(AsyncLogEventInfo logEventInfo);

        public abstract AsyncLogEventInfo[] DequeueBatch(int count);

        public abstract void DequeueBatch(int count, IList<AsyncLogEventInfo> result);

        public abstract void Clear();

        /// <summary>
        /// Raise event when queued element was dropped because of queue overflow
        /// </summary>
        /// <param name="logEventInfo">Dropped queue item</param>
        protected void OnLogEventDropped(LogEventInfo logEventInfo) => LogEventDropped?.Invoke(this, new LogEventDroppedEventArgs(logEventInfo));

        /// <summary>
        /// Raise event when RequestCount overflow <see cref="RequestLimit"/>
        /// </summary>
        /// <param name="requestsCount"> current requests count</param>
        protected void OnLogEventQueueGrows(long requestsCount) => LogEventQueueGrow?.Invoke(this, new LogEventQueueGrowEventArgs(RequestLimit, requestsCount));
    }
}
