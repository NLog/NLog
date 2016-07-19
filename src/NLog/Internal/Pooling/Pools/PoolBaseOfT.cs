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
    /// simple abstract class for a generic pool with a given max pool size.
    /// </summary>
    /// <typeparam name="TPooled"></typeparam>
    internal abstract class PoolBaseOfT<TPooled> : PoolBase
        where TPooled : class
    {
        private IPool<TPooled> pool;

        /// <summary>
        /// Initializes a new instance of the pool with the given pool size, possibly prefilling the pool with items so its primed with items to use.
        /// </summary>
        /// <param name="poolSize">The number of items to support in the pool</param>
        /// <param name="preFill">Whether or not to prefill the pool.</param>
        /// <param name="poolName">The name of the pool to be written in statistics - default is type name</param>
        /// <param name="enabled">Whether or not to enable pool.</param>
        protected PoolBaseOfT(int poolSize, bool preFill = false, string poolName = null, bool enabled = false)
            : base(poolSize, preFill, poolName, enabled)
        {

        }

        /// <summary>
        /// Gets the free space in the pool. Pool implementation specific
        /// </summary>
        public int FreeSpace
        {
            get
            {
                if (this.pool == null)
                {
                    return 0;
                }
                return this.pool.FreeSpace;
            }
        }

        /// <summary>
        /// Gets the number of objects currently in the pool.
        /// This does not take into account the number of objects in use.
        /// So real number of pooled objects can be higher.
        /// </summary>
        public override long ObjectsInPool
        {
            get
            {
                if (this.pool == null)
                {
                    return 0;
                }
                return this.pool.ObjectsInPool;
            }
        }

        /// <summary>
        /// Gets the number of thrown away objects - a high number could indicate that your pool size is too small.
        /// </summary>
        public override long ThrownAwayObjects
        {
            get
            {
                if (this.pool == null)
                {
                    return 0;
                }
                return this.pool.ThrownAwayObjects;
            }
        }

        /// <summary>
        /// Restarts the already running pool with potential new limits.
        /// </summary>
        /// <param name="configuration">The configuration to use</param>
        protected override void RestartPool(PoolConfiguration configuration)
        {
            int newSize = this.GetPoolSize(configuration);
            if (this.pool == null)
            {
                this.InitializePool(configuration);
            }
            else
            {
                lock (this.pool)
                {
                    this.pool.Clear();
                    this.pool.ReInitialize(this.Factory, newSize, configuration.PrefillPools);
                }
            }
        }

        /// <summary>
        /// Empties the pool of all items. Pool specific implementation.
        /// </summary>
        protected override void EmptyPool()
        {
            if (this.pool == null)
            {
                return;
            }
            this.pool.Clear();
        }

        /// <summary>
        /// Resets all stats
        /// </summary>
        internal override void ResetStats()
        {
            if (this.pool != null)
            {
                this.pool.ResetStats();
                base.ResetStats();
            }
        }

        /// <summary>
        /// Gets a value indicating whether or not the concrete pool implementation supports auto increase.
        /// </summary>
        public bool SupportsAutoIncrease
        {
            get
            {
                if (this.pool == null)
                {
                    return false;
                }
                return this.pool.SupportsAutoIncrease;
            }
        }

        /// <summary>
        /// Initializes the pool prefilling it with objects if its configured.
        /// Will use different type of pool depending on the platform and whether or not the configuration
        /// has been set to optimize for many concurrent writing threads.
        /// </summary>
        protected override void InitializePool(PoolConfiguration configuration)
        {
#if SILVERLIGHT
             this.pool = new StackPoolImpl<TPooled>(this.PoolSize, this.Factory, this.AutoIncrease);
#else  
            if (configuration.OptimiseForManyThreads)
            {
               this.pool = new ConcurrentRingBufferImpl<TPooled>(this.PoolSize);
            }
            else
            {
                this.pool = new StackPoolImpl<TPooled>(this.PoolSize, this.Factory, this.AutoIncrease);
            }
#endif
            this.pool.Initialize(this.Factory, this.PreFill);
        }

        /// <summary>
        /// Gets an item from the pool.
        /// If the pool is empty, the factory method is called to create an item,
        /// </summary>
        /// <returns>An instance of <typeparamref name="TPooled"/>the pooled item</returns>
        public virtual TPooled Get()
        {
            if (!this.Enabled)
            {
                return this.Factory();
            }
            this.GivenOut();
            if (this.pool == null)
            {
                return this.Factory();
            }
            return this.pool.Pop();

        }
        /// <summary>
        /// Puts back the item into the pool.
        /// </summary>
        public void PutBack(TPooled item)
        {
            if (!this.Enabled)
            {
                return;
            }
            if (!this.VerifyPooledItem(item))
            {
                InternalLogger.Trace("Dropped {0} because VerifyPooledItem returned false", typeof(TPooled));
                // just silently drop it
                return;
            }
            this.GottenBack();
            this.Clear(item);

            if (this.pool != null)
            {
                this.pool.Push(item);
            }
        }


        /// <summary>
        /// Factory method that creates an instance of the pooled type.
        /// </summary>
        /// <returns>An instance of the pooled type</returns>
        protected abstract TPooled Factory();

        /// <summary>
        /// Method that verifies that an item is okay to put back into the pool
        /// </summary>
        /// <param name="item">The item to verify.</param>
        /// <returns>true if the item is okay to put back; false otherwise.</returns>
        protected virtual bool VerifyPooledItem(TPooled item)
        {
            return true;
        }

        /// <summary>
        /// Clears the item if necessary before putting it back into the pool.
        /// </summary>
        /// <param name="item">The item to clear.</param>
        protected virtual void Clear(TPooled item) { }
    }
}