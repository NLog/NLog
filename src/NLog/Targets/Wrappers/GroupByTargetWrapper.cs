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
    using System.Collections.Generic;
    using NLog.Common;
    using NLog.Config;
    using NLog.Internal;
    using NLog.Layouts;

    /// <summary>
    /// A target that buffers log events and sends them in batches to the wrapped target.
    /// </summary>
    [Target("GroupByWrapper", IsWrapper = true)]
    class GroupByTargetWrapper : WrapperTargetBase
    {
        SortHelpers.KeySelector<AsyncLogEventInfo, string> _buildKeyStringDelegate;

        /// <summary>
        /// Identifier to perform group-by
        /// </summary>
        [RequiredParameter]
        public Layout Key { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GroupByTargetWrapper" /> class.
        /// </summary>
        public GroupByTargetWrapper() : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GroupByTargetWrapper" /> class.
        /// </summary>
        /// <param name="wrappedTarget">The wrapped target.</param>
        public GroupByTargetWrapper(Target wrappedTarget)
            : this(null, wrappedTarget)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GroupByTargetWrapper" /> class.
        /// </summary>
        /// <param name="name">The name of the target.</param>
        /// <param name="wrappedTarget">The wrapped target.</param>
        public GroupByTargetWrapper(string name, Target wrappedTarget)
            : this(name, wrappedTarget, string.Empty)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GroupByTargetWrapper" /> class.
        /// </summary>
        /// <param name="name">The name of the target.</param>
        /// <param name="wrappedTarget">The wrapped target.</param>
        /// <param name="key">Group by identifier.</param>
        public GroupByTargetWrapper(string name, Target wrappedTarget, Layout key)
        {
            Name = name;
            WrappedTarget = wrappedTarget;
            Key = key;
        }

        // <inheritdoc />
        protected override void Write(AsyncLogEventInfo logEvent)
        {
            WrappedTarget.WriteAsyncLogEvent(logEvent);
        }

        // <inheritdoc />
        protected override void Write(IList<AsyncLogEventInfo> logEvents)
        {
            if (_buildKeyStringDelegate == null)
                _buildKeyStringDelegate = logEvent => RenderLogEvent(Key, logEvent.LogEvent);

            var buckets = logEvents.BucketSort(_buildKeyStringDelegate);
            foreach (var bucket in buckets)
            {
                WrappedTarget.WriteAsyncLogEvents(bucket.Value);
            }
        }
    }
}
