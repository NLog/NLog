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
using System.IO;

namespace NLog.Internal.Pooling.Pools
{
    /// <summary>
    /// Pool for <see cref="MemoryStream"/>
    /// </summary>
    internal class MemoryStreamPool : PoolBaseOfT<MemoryStream>
    {
        private int initialStreamSize = 4096;


        /// <summary>
        /// Initializes an instance of the <see cref="MemoryStreamPool"/>
        /// </summary>
        /// <param name="poolSize">Size of the pool.</param>
        /// <param name="preFill">Whether or not to prefill the pool</param>
        /// <param name="poolName">The name of the pool</param>
        /// <param name="enabled">Whether or not the pool is enabled.</param>
        public MemoryStreamPool(int poolSize, bool preFill = false, string poolName = null, bool enabled = false)
            : base(poolSize, preFill, poolName, enabled)
        {
        }

        /// <summary>
        /// Factory method that creates an instance of the pooled type.
        /// </summary>
        /// <returns>An instance of the pooled type</returns>
        protected override MemoryStream Factory()
        {
            return new MemoryStream(this.initialStreamSize);
        }

        /// <summary>
        /// Implementations of pools can inspect the pool configuration and tweak their configuration based on the new configuration.
        /// Do not copy the configuration to a local variable.
        /// </summary>
        /// <param name="configuration">The new configuration to use.</param>
        protected override void PoolLimitsChanged(PoolConfiguration configuration)
        {
            this.SetStreamSize(configuration);
        }

        /// <summary>
        /// Has to be implemented in implementation.
        /// PoolBase will call this to figure out how big a pool should be dependent on the given configuration.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <returns>The number of pooled objects this pool should support.</returns>
        protected override int GetPoolSize(PoolConfiguration configuration)
        {
            int max = 100;

            return Math.Min(max, configuration.EstimatedLogEventsPerSecond);
        }

        /// <summary>
        /// Sets the initial size of the memory stream, by guessing a good initial size based on the estimated max message size and the number of log events per second.
        /// </summary>
        /// <param name="configuration">The pool configuration.</param>
        private void SetStreamSize(PoolConfiguration configuration)
        {
            this.initialStreamSize = configuration.EstimatedMaxMessageSize * configuration.EstimatedLogEventsPerSecond * 4;
            if (this.initialStreamSize < 0)
            {
                this.initialStreamSize = 16384;
            }
        }

        /// <summary>
        /// Initializes the pool prefilling it with objects if its configured.
        /// Will use different type of pool depending on the platform and whether or not the configuration
        /// has been set to optimize for many concurrent writing threads.
        /// </summary>
        protected override void InitializePool(PoolConfiguration configuration)
        {
            base.InitializePool(configuration);

            this.SetStreamSize(configuration);
        }
    }
}
