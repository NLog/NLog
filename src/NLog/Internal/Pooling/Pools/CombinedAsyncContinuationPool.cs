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
    /// Pool for <see cref="CombinedAsyncContinuation"/>
    /// </summary>
    internal class CombinedAsyncContinuationPool : PoolBaseOfT<CombinedAsyncContinuation>
    {

        /// <summary>
        /// Initializes an instance of <see cref="CombinedAsyncContinuationPool"/> with the given max pool size.
        /// </summary>
        /// <param name="poolSize">The maximum number of elements this pool will support.</param>
        /// <param name="preFill">Whether or not to prefill the pool with  arrays.</param>
        /// <param name="poolName">the na,e of the pool, default null means name of derived type</param>
        /// <param name="enabled">Whether or not to enable pool</param>
        public CombinedAsyncContinuationPool(int poolSize, bool preFill = false, string poolName = null, bool enabled = false)
            : base(poolSize, preFill, poolName, enabled)
        {
        }

        /// <summary>
        /// Factory method that creates an instance of the pooled type.
        /// </summary>
        /// <returns>An instance of the pooled type</returns>
        protected override CombinedAsyncContinuation Factory()
        {
            return new CombinedAsyncContinuation(this);
        }

        /// <summary>
        /// Specialization of the Get method, so callers can pass in required parameters and not first call Get().
        /// </summary>
        /// <param name="originalContinuation">The original continuation.</param>
        /// <param name="continueWith">The continuation to continue with.</param>
        /// <returns>A combined async continuation that will first run the <paramref name="originalContinuation"/> and then the <paramref name="continueWith"/> continuation.</returns>
        internal CombinedAsyncContinuation Get(AsyncContinuation originalContinuation, AsyncContinuation continueWith)
        {
            var cont = this.Get();
            cont.Init(this, originalContinuation, continueWith);
            return cont;
        }

        /// <summary>
        /// Clears the item if necessary before putting it back into the pool.
        /// </summary>
        /// <param name="item">The item to clear.</param>
        protected override void Clear(CombinedAsyncContinuation item)
        {
            item.Clear();
        }
    }
}