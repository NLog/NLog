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

using NLog.Common;

namespace NLog.Internal.Pooling.Pools
{
    /// <summary>
    /// Pool of <see cref="SingleCallContinuation"/>
    /// </summary>
    internal class SingleCallContinuationPool : PoolBaseOfT<SingleCallContinuation>
    {
        /// <summary>
        /// Initializes an instance of the <see cref="SingleCallContinuationPool"/>
        /// </summary>
        /// <param name="poolSize">Size of the pool.</param>
        /// <param name="preFill">Whether or not to prefill the pool</param>
        public SingleCallContinuationPool(int poolSize, bool preFill = false) : base(poolSize, preFill)
        {
        }

        /// <summary>
        /// Factory method that creates an instance of the pooled type.
        /// </summary>
        /// <returns>An instance of the pooled type</returns>
        protected override SingleCallContinuation Factory()
        {
            return new SingleCallContinuation(this);
        }

        /// <summary>
        /// Specialized Get method where the AsyncContinuation can be passed on to the single call continuation.
        /// </summary>
        /// <param name="asyncContinuation">The asynccontinuation.</param>
        /// <returns>A single call continuation.</returns>
        internal SingleCallContinuation Get(AsyncContinuation asyncContinuation)
        {
            var cont = this.Get();
            cont.Initialize(this, asyncContinuation);
            return cont;
        }

        /// <summary>
        /// Clears the item if necessary before putting it back into the pool.
        /// </summary>
        /// <param name="item">The item to clear.</param>
        protected override void Clear(SingleCallContinuation item)
        {
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
            return (int)(configuration.EstimatedLogEventsPerSecond * 1.5);
        }
    }
}