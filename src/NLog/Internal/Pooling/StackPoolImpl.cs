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
using System.Collections.Generic;
using System.Threading;

namespace NLog.Internal.Pooling
{
    /// <summary>
    /// Pool implementation that uses a Stack as a backing store.
    /// Useful when there are not a lot of threads logging items.
    /// Supports auto increase.
    /// </summary>
    /// <typeparam name="TPooled">The type of item pooled.</typeparam>
    internal class StackPoolImpl<TPooled> : IPool<TPooled>
        where TPooled : class
    {
        private Stack<TPooled> pool;
        private long thrownAway;

        private int poolSize;

        private Func<TPooled> factory;

        private readonly bool autoIncrease;

        public StackPoolImpl(int poolSize, Func<TPooled> factory, bool autoIncrease)
        {
            this.poolSize = poolSize;
            this.factory = factory;
            this.autoIncrease = autoIncrease;
            this.pool = new Stack<TPooled>(poolSize);
        }

        /// <summary>
        /// Pops an item from the pool
        /// </summary>
        /// <returns>The pooled item</returns>
        public TPooled Pop()
        {
            lock (this.pool)
            {
                if (this.pool.Count == 0)
                {
                    return this.factory();
                }

                return this.pool.Pop();
            }
        }

        /// <summary>
        /// Initialize the pool
        /// </summary>
        /// <param name="factory">The object factory.</param>
        /// <param name="preFill">Whether or not to prefill the pool</param>
        public void Initialize(Func<TPooled> factory, bool preFill)
        {
            if (preFill)
            {
                for (int x = 0; x < this.poolSize; x++)
                {
                    this.Push(this.factory());
                }
            }
        }

        /// <summary>
        /// ReInitialize the pool
        /// </summary>
        /// <param name="factory">The object factory.</param>
        /// <param name="capacity">The new capacity.</param>
        /// <param name="preFill">Whether or not to prefill the pool</param>
        public void ReInitialize(Func<TPooled> factory, int capacity, bool preFill)
        {
            lock (this.pool)
            {
                this.factory = factory;
                this.poolSize = capacity;
                this.pool = new Stack<TPooled>(capacity);
                if (preFill)
                {
                    for (int x = 0; x < capacity; x++)
                    {
                        this.Push(this.factory());
                    }
                }
            }
        }

        /// <summary>
        /// Clears the pool of all items
        /// </summary>
        public void Clear()
        {
            lock (this.pool)
            {
                this.thrownAway = 0;
                this.pool.Clear();
            }
        }

        /// <summary>
        /// Gets the number of items in the pool.
        /// </summary>
        public long ObjectsInPool
        {
            get
            {
                lock (this.pool)
                {
                    return this.pool.Count;
                }
            }
        }

        /// <summary>
        /// Gets the number of objects thrown away.
        /// </summary>
        public long ThrownAwayObjects
        {
            get
            {
#if !SILVERLIGHT
                return Interlocked.Read(ref this.thrownAway);
#else
                lock(this.pool)
                {
                    return this.thrownAway;
                }
#endif

            }
        }

        /// <summary>
        /// Resets the stats of the pool
        /// </summary>
        public void ResetStats()
        {
            this.thrownAway = 0;
        }

        /// <summary>
        /// Free space in the pool.
        /// </summary>
        public int FreeSpace
        {
            get
            {
                return 0;
            }
        }

        /// <summary>
        /// Gets whether or not the pool supports auto increase, i.e. can soak up any number of items.
        /// </summary>
        public bool SupportsAutoIncrease
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Push an item into the pool
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>Whether or not the item was added to the pool.</returns>
        public bool Push(TPooled item)
        {
            lock (this.pool)
            {
                if (this.pool.Count < this.poolSize || this.autoIncrease)
                {
                    this.pool.Push(item);
                    return true;
                }
            }

            this.ThrownAway();
            return false;
        }

        /// <summary>
        /// Increments the thrown away counter.
        /// </summary>
        private void ThrownAway()
        {
            Interlocked.Increment(ref this.thrownAway);
        }
    }
}