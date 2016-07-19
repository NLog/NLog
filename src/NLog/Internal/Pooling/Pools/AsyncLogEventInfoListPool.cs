﻿// 
// Copyright (c) 2004-2016 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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
using System.Collections.Generic;

using NLog.Common;
using NLog.Targets.Wrappers;

namespace NLog.Internal.Pooling.Pools
{
    /// <summary>
    /// Pool for AsyncLogEventInfo List's i.e. List{AsyncLogEventInfo}
    /// </summary>
    internal class AsyncLogEventInfoListPool : PoolBaseOfT<List<AsyncLogEventInfo>>
    {
        private int inialListSize;

        private int numberOfTargets;

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncLogEventInfoArrayPool"/>
        /// </summary>
        /// <param name="poolSize">Number of items in the pool</param>
        /// <param name="initialListSize">The initialize size of the list.</param>
        /// <param name="preFill">Whether or not to prefill the pool</param>
        public AsyncLogEventInfoListPool(int poolSize, int initialListSize, bool preFill=false)
            :base(poolSize,preFill)
        {
            this.inialListSize = initialListSize;
        }

        /// <summary>
        /// Factory method that creates an instance of the pooled type.
        /// </summary>
        /// <returns>An instance of the pooled type</returns>
        protected override List<AsyncLogEventInfo> Factory()
        {
            return new List<AsyncLogEventInfo>(this.inialListSize);
        }

        /// <summary>
        /// Clears the item if necessary before putting it back into the pool.
        /// </summary>
        /// <param name="item">The item to clear.</param>
        protected override void Clear(List<AsyncLogEventInfo> item)
        {
            for (int x = 0; x < item.Count; x++)
            {
                LogEventInfo info = item[x].LogEvent;
                if (info != null)
                {
                    info.PutBack();
                }
            }

            item.Clear();
        }

        /// <summary>
        /// Has to be implemented in implementation.
        /// PoolBase will call this to figure out how big a pool should be dependent on the given configuration.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <returns>The number of pooled objects this pool should support.</returns>
        protected override int GetPoolSize(PoolConfiguration configuration)
        {
            int max = 1000;
            return Math.Min(max, base.GetPoolSize(configuration));
        }

        /// <summary>
        /// Implementations of pools can inspect the pool configuration and tweak their configuration based on the new configuration.
        /// Do not copy the configuration to a local variable.
        /// </summary>
        /// <param name="configuration">The new configuration to use.</param>
        protected override void PoolLimitsChanged(PoolConfiguration configuration)
        {
            this.inialListSize = 100;

            if (configuration.LoggingConfiguration != null)
            {
                this.numberOfTargets = configuration.LoggingConfiguration.AllTargets.Count;
                InternalLogger.Info("Setting numberOfTargets to be:{0}", this.numberOfTargets);
                int maxQueue = 0;

                for (int x = 0; x < configuration.LoggingConfiguration.AllTargets.Count; x++)
                {
                    var target = configuration.LoggingConfiguration.AllTargets[x] as AsyncTargetWrapper;

                    if (target != null)
                    {
                        if (target.BatchSize > maxQueue)
                        {
                            maxQueue = target.BatchSize;
                        }
                    }
                }
                InternalLogger.Info("Setting individual array size for AsyncLogEventInfoListPool to be:{0}", maxQueue);
                // Set a good default array size to accomodate the highest batchsize of the async targets.
                this.inialListSize = maxQueue;
            }
        }

    }
}