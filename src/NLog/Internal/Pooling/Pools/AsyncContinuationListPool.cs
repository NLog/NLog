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

using System.Collections.Generic;

using NLog.Common;
using NLog.Targets.Wrappers;

namespace NLog.Internal.Pooling.Pools
{
    /// <summary>
    /// Pool for async continuation List's
    /// </summary>
    internal class AsyncContinuationListPool : PoolBaseOfT<List<AsyncContinuation>>
    {
        private const int DefaultIndividualArraySize = 100;
        private int individualArraySize;
        private int numberOfTargets;

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncContinuationListPool"/>.
        /// </summary>
        /// <param name="poolSize">The pool size</param>
        /// <param name="preFill">Whether or not to prefill the pool</param>
        /// <param name="poolName">Name of the pool.</param>
        /// <param name="enabled">Whether or not the pool is enabled</param>
        public AsyncContinuationListPool(int poolSize, bool preFill = false, string poolName = null, bool enabled = false)
            : base(poolSize, preFill, poolName, enabled)
        {
            this.individualArraySize = DefaultIndividualArraySize;
            this.numberOfTargets = 2;
        }

        /// <summary>
        /// Factory method that creates an instance of the pooled type.
        /// </summary>
        /// <returns>An instance of the pooled type</returns>
        protected override List<AsyncContinuation> Factory()
        {
            return new List<AsyncContinuation>(this.individualArraySize);
        }

        /// <summary>
        /// Has to be implemented in implementation.
        /// PoolBase will call this to figure out how big a pool should be dependent on the given configuration.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <returns>The number of pooled objects this pool should support.</returns>
        protected override int GetPoolSize(PoolConfiguration configuration)
        {
            return this.numberOfTargets*4;
        }

        /// <summary>
        /// Implementations of pools can inspect the pool configuration and tweak their configuration based on the new configuration.
        /// Do not copy the configuration to a local variable.
        /// </summary>
        /// <param name="configuration">The new configuration to use.</param>
        protected override void PoolLimitsChanged(PoolConfiguration configuration)
        {
            this.individualArraySize = DefaultIndividualArraySize;

            if (configuration.LoggingConfiguration != null)
            {
                this.numberOfTargets = configuration.LoggingConfiguration.AllTargets.Count;
                InternalLogger.Info("Setting numberOfTargets to be:{0}", this.numberOfTargets);
                int maxQueue = 0;
                
                for (int x = 0; x < configuration.LoggingConfiguration.AllTargets.Count; x++)
                {
                    var target = configuration.LoggingConfiguration.AllTargets[x] as AsyncTargetWrapper;
                    
                    if (target !=null)
                    {
                        if(target.BatchSize > maxQueue)
                        {
                            maxQueue = target.BatchSize;
                        }
                    }
                }
                InternalLogger.Info("Setting individual array size for AsyncContinuationListPool to be:{0}", maxQueue);
                // Set a good default array size to accomodate the highest batchsize of the async targets.
                this.individualArraySize = maxQueue;
                
            }
        }
    }
}