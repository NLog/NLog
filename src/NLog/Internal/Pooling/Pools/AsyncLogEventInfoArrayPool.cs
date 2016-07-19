// 
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

using NLog.Common;

namespace NLog.Internal.Pooling.Pools
{
    /// <summary>
    /// Pool for AsyncLogEventInfo arrays
    /// </summary>
    internal class AsyncLogEventInfoArrayPool : ArrayPoolBase<AsyncLogEventInfo>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncLogEventInfoArrayPool"/>
        /// </summary>
        /// <param name="poolSize">Number of items in the pool</param>
        /// <param name="individualArraySize">The size of the individual arrays.</param>
        /// <param name="preFill">Whether or not to prefill the pool</param>
        /// <param name="poolName">The name of the pool</param>
        /// <param name="enabled">Whether or not the pool is enabled.</param>
        public AsyncLogEventInfoArrayPool(int poolSize, int individualArraySize, bool preFill = false, string poolName = null, bool enabled = false)
            : base(poolSize, individualArraySize, preFill, poolName, enabled)
        {
        }

        /// <summary>
        /// Implementations of pools can inspect the pool configuration and tweak their configuration based on the new configuration.
        /// Do not copy the configuration to a local variable.
        /// </summary>
        /// <param name="configuration">The new configuration to use.</param>
        protected override void PoolLimitsChanged(PoolConfiguration configuration)
        {
            base.PoolLimitsChanged(configuration);

            // set array size to be at least 90k to get above 85k elements, so it gets put into LOH immediately
            int min = 90000;
            
            this.IndividualArraySize = Math.Max(min, this.MaxQueueSize);
            InternalLogger.Info("Setting IndividualArraySize for:{0} to be:{1}", this.GetType().FullName, this.IndividualArraySize.AsString());
        }

        /// <summary>
        /// Clears the item if necessary before putting it back into the pool.
        /// </summary>
        /// <param name="item">The item to clear.</param>
        protected override void Clear(AsyncLogEventInfo[] item)
        {
            for (int x = 0; x < item.Length; x++)
            {
                item[x] = default(AsyncLogEventInfo);
            }
            
        }

        /// <summary>
        /// Has to be implemented in implementation.
        /// PoolBase will call this to figure out how big a pool should be dependent on the given configuration.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <returns>The number of pooled objects this pool should support.</returns>
        protected override int GetPoolSize(PoolConfiguration configuration)
        {
            var size = this.NumberOfTargets * 100;
            InternalLogger.Info("Setting PoolSize for:{0} to be:{1}", this.GetType().FullName, size.AsString());
            return size;
        }
    }
}