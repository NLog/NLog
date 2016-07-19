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
#if !SILVERLIGHT
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;

namespace NLog.Internal.Pooling
{
    /// <summary>
    /// Buffer implementation that is mostly useful when there are many threads pushing and popping at the same time.
    /// </summary>
    /// <typeparam name="TPooled">The item pooled.</typeparam>
    internal class ConcurrentRingBufferImpl<TPooled> : IEnumerable<TPooled>, IPool<TPooled>
        where TPooled : class
    {
        private readonly bool blocking;

        private long readersWaiting;

        private long writerWaiting;

        private readonly AutoResetEvent writerEvent;

        private readonly AutoResetEvent readerEvent;

        private long realCapacity;

        private TPooled[] buffer;

        private Func<TPooled> factory;

        private long producerIndex;

        private long consumerIndex;

        private long count;
        private long thrownAway;
        private long created;

        private static readonly int freeSpace = 4;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentRingBufferImpl{TPooled}"/>
        /// </summary>
        /// <param name="capacity">the capacity of the ring buffer.</param>
        /// <param name="blocking">Whether or not the pool methods should block when either the pool gets filled or become empty.</param>
        public ConcurrentRingBufferImpl(long capacity, bool blocking = false)
        {
            this.blocking = blocking;
            this.realCapacity = capacity + freeSpace;
            if (capacity < 1)
            {
                throw new ArgumentOutOfRangeException("capacity", "must be positive");
            }

            this.buffer = new TPooled[this.realCapacity];
            if (blocking)
            {
                this.writerEvent = new AutoResetEvent(false);
                this.readerEvent = new AutoResetEvent(false);
            }
        }

        /// <summary>
        /// Initializes the pool with the given object factory and prefil setting.
        /// </summary>
        /// <param name="objectFactory">A Func that can return a new instance of the pooled item.</param>
        /// <param name="preFill">Whether or not to prefill the pool.</param>
        public void Initialize(Func<TPooled> objectFactory, bool preFill)
        {
            this.factory = objectFactory;
            this.producerIndex = 0;
            this.consumerIndex = 0;
            this.count = 0;
            this.thrownAway = 0;
            this.created = 0;
            if (preFill)
            {
                for (int x = 0; x < this.realCapacity - freeSpace; x++)
                {
                    this.Push(objectFactory());
                }
            }
        }

        /// <summary>
        /// Free space in the pool, i.e. how many items can be put into the pool.
        /// </summary>
        public int FreeSpace
        {
            get
            {
                return freeSpace;
            }
        }

        /// <summary>
        /// Gets whether or not the pool supports auto increase, i.e. can soak up any number of items.
        /// </summary>
        public bool SupportsAutoIncrease
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// The number of items thrown away. 
        /// Will be 0 for a blocking pool, unless there is a bug.
        /// </summary>
        public long ThrownAwayObjects
        {
            get
            {
                return Interlocked.Read(ref this.thrownAway);
            }
        }

        /// <summary>
        /// Number of items created by this pool.
        /// </summary>
        public long Created
        {
            get
            {
                return this.created;
            }
        }

        /// <summary>
        /// Resets the stats for the pool
        /// </summary>
        public void ResetStats()
        {
            Interlocked.Exchange(ref this.thrownAway, 0);
        }

        /// <summary>
        /// Blocks the current thread until the pool is no longer full
        /// </summary>
        private void WaitUntilNonFull()
        {
            Interlocked.Increment(ref this.writerWaiting);

            try
            {
                while (this.IsFull)
                {
                    this.writerEvent.WaitOne();
                }
            }
            finally
            {
                Interlocked.Decrement(ref this.writerWaiting);
            }
        }

        /// <summary>
        /// Blocks the current thread until the pool is no longer empty.
        /// </summary>
        private void WaitUntilNonEmpty()
        {
            Interlocked.Increment(ref this.readersWaiting);

            try
            {
                while (this.IsEmpty)
                {
                    this.readerEvent.WaitOne();
                }
            }
            finally
            {
                Interlocked.Decrement(ref this.readersWaiting);
            }
        }

        /// <summary>
        /// Spins a few clock cycles.
        /// </summary>
        /// <param name="spintCount">The number of spins.</param>
        private static void Spin(int spintCount)
        {
#if NET4_5
            Thread.SpinWait(20 * spintCount);
#else
            Thread.Sleep(0);
#endif
        }

        /// <summary>
        /// Reads the value from the given location.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <returns>The value from the location.</returns>
        private long ReadVolatile(ref long location)
        {
#if NET3_5 || NET4_0
            return Interlocked.Read(ref location);
#else
            return Volatile.Read(ref location);
#endif
        }

        /// <summary>
        /// Pushes an item into the pool.
        /// If the pool is full and the pool is blocking, this method will block until the pool has room for the item.
        /// If the pool is non blocking and the pool is full, the item is thrown away.
        /// </summary>
        /// <param name="item">The item to put into the pool.</param>
        /// <returns>Whether or not the item was put into the pool or not.</returns>
        public bool Push(TPooled item)
        {
            var currentIndex = this.ReadVolatile(ref this.producerIndex);
            if (this.IsFull)
            {
                // Full
                if (this.blocking)
                {
                    this.WaitUntilNonFull();
                }
                else
                {
                    Interlocked.Increment(ref this.thrownAway);
                    return false;
                }
            }
            int spins = 0;
            do
            {
                var newCurrent = (currentIndex + 1) % this.realCapacity;

                if (Interlocked.CompareExchange(ref this.producerIndex, newCurrent, currentIndex) == currentIndex)
                {
                    // just overwrite blindly, even if by a freak accident that another thread goes the whole way around the ring
                    // and ends with same index, before the below line is executed, it will only cause an item in the pool to be lost.
                    this.buffer[newCurrent] = item;
                    Interlocked.Increment(ref this.count);
                    if (this.blocking && Interlocked.Read(ref this.readersWaiting) > 0)
                    {
                        this.readerEvent.Set();
                    }
                    return true;
                }
                Spin(spins++);
                currentIndex = this.ReadVolatile(ref this.producerIndex);
            }
            while (!this.IsFull);

            // Full
            if (this.blocking)
            {
                this.WaitUntilNonFull();
                return this.Push(item);
            }

            Interlocked.Increment(ref this.thrownAway);
            return false;
        }

        /// <summary>
        /// Gets whether or not the pool is empty.
        /// </summary>
        private bool IsEmpty
        {
            get
            {
                return (this.consumerIndex == this.producerIndex && this.count == 0);
            }
        }

        /// <summary>
        /// Gets whether or the pool is full.
        /// </summary>
        private bool IsFull
        {
            get
            {
                return (((this.producerIndex + freeSpace) % this.realCapacity) == this.consumerIndex) && this.count != 0;
            }
        }

        /// <summary>
        /// Pops an item from the pool.
        /// If the pool is blocking and the pool is empty, the current thread will block and wait for an item to become available.
        /// If the pool is empty and the pool is not blocking, then a new item is created and returned.
        /// </summary>
        /// <returns>The pooled item.</returns>
        public TPooled Pop()
        {
            var currentIndex = this.ReadVolatile(ref this.consumerIndex);
            if (this.IsEmpty)
            {
                // caught up or empty
                if (this.blocking)
                {
                    this.WaitUntilNonEmpty();
                }
                else
                {
                    Interlocked.Increment(ref this.created);
                    return this.factory();
                }
            }
            int spins = 0;
            do
            {
                var newCurrent = (currentIndex + 1) % this.realCapacity;
                var data = this.buffer[currentIndex];

                if (Interlocked.CompareExchange(ref this.consumerIndex, newCurrent, currentIndex) == currentIndex)
                {
                    if (data != null)
                    {
                        if (Interlocked.CompareExchange(ref this.buffer[currentIndex], null, data) == data)
                        {
                            Interlocked.Decrement(ref this.count);
                            if (this.blocking && Interlocked.Read(ref this.writerWaiting) > 0)
                            {
                                this.writerEvent.Set();
                            }

                            return data;
                        }
                    }
                }

                Spin(spins++);
                currentIndex = this.ReadVolatile(ref this.consumerIndex);
            }
            while (!this.IsEmpty);

            // caught up or empty
            if (this.blocking)
            {
                this.WaitUntilNonEmpty();
                return this.Pop();
            }

            Interlocked.Increment(ref this.created);
            return this.factory();
        }

        /// <summary>
        /// Clears the pool
        /// </summary>
        public void Clear()
        {
            this.producerIndex = 0;
            this.consumerIndex = 0;
            this.count = 0;
            this.thrownAway = 0;
            this.created = 0;
            this.buffer = new TPooled[this.realCapacity];
        }

        /// <summary>
        /// Gets the number of items in the pool.
        /// </summary>
        public long ObjectsInPool
        {
            get
            {
                return Interlocked.Read(ref this.count);
            }
        }

        /// <summary>
        /// Warning allocates, will return a readonly view of the pool.
        /// </summary>
        public ReadOnlyCollection<TPooled> Data
        {
            get
            {
                List<TPooled> list = new List<TPooled>();
                list.AddRange(this);
                return new ReadOnlyCollection<TPooled>(list);
            }
        }

        /// <summary>
        /// Warning allocates, will return an enumerator that will copy the items from the pool into another
        ///  array and return an IEnumerable with the items.
        /// </summary>
        public IEnumerator<TPooled> GetEnumerator()
        {
            if (this.ObjectsInPool == 0)
            {
                yield break;
            }
            var copy = new TPooled[this.buffer.Length];
            Array.Copy(this.buffer, copy, this.buffer.Length);

            for (int x = 0; x < copy.Length; x++)
            {
                if (copy[x] != null)
                {
                    yield return copy[x];
                }
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        /// <summary>
        /// Reinitializes the pool with the given new settings.
        /// </summary>
        /// <param name="objectFactory">A Func that can create an instance of the pooled type. </param>
        /// <param name="capacity">The capacity of the pool.</param>
        /// <param name="prefillPools">Whether or not to prefill the pool.</param>
        public void ReInitialize(Func<TPooled> objectFactory, int capacity, bool prefillPools)
        {
            this.realCapacity = capacity + freeSpace;
            if (capacity < 1)
            {
                throw new ArgumentOutOfRangeException("capacity", "must be positive");
            }

            this.buffer = new TPooled[this.realCapacity];
            this.Initialize(objectFactory, prefillPools);
        }
    }
}
#endif