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

namespace NLog.Internal.Pooling.Pools
{
    /// <summary>
    /// Generic pool for items that need no special handling, i.e. objects that takes no parameters in the constructors
    /// or where the callers will fill the object with data.
    /// Requires that all poolsed items will be implementations of <see cref="IPooledItem{TPooled}"/>.
    /// </summary>
    /// <typeparam name="TPooled">The type of pooled item.</typeparam>
    internal class GenericPool<TPooled> : PoolBaseOfT<TPooled>
        where TPooled : class, IPooledItem<TPooled>, new()
    {
        /// <summary>
        /// Initializes an instance of the <see cref="GenericPool{TPooled}"/>
        /// </summary>
        /// <param name="poolSize">Size of the pool.</param>
        /// <param name="preFill">Whether or not to prefill the pool</param>
        /// <param name="poolName">The name of the pool</param>
        /// <param name="enabled">Whether or not the pool is enabled.</param>
        public GenericPool(int poolSize, bool preFill = false, string poolName = null, bool enabled = false) : base(poolSize, preFill, poolName, enabled)
        {
        }

        /// <summary>
        /// Factory method that creates an instance of the pooled type.
        /// </summary>
        /// <returns>An instance of the pooled type</returns>
        protected override TPooled Factory()
        {
            return new TPooled();
        }

        /// <summary>
        /// Gets an item from the pool.
        /// If the pool is empty, the factory method is called to create an item,
        /// </summary>
        /// <returns>An instance of <typeparamref name="TPooled"/>the pooled item</returns>
        public override TPooled Get()
        {
            var pooled = base.Get();
            pooled.Init(this);
            return pooled;
        }

        /// <summary>
        /// Clears the item if necessary before putting it back into the pool.
        /// </summary>
        /// <param name="item">The item to clear.</param>
        protected override void Clear(TPooled item)
        {
            item.Clear();
        }
    }
}
