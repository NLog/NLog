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

namespace NLog.Internal.Pooling
{
    /// <summary>
    /// Interface for an object pool
    /// This
    /// </summary>
    /// <typeparam name="TPooled"></typeparam>
    internal interface IPool<TPooled>
        where TPooled:class
    {
        /// <summary>
        /// Push an item into the pool
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>Whether or not the item was added to the pool.</returns>
        bool Push(TPooled item);

        /// <summary>
        /// Pops an item from the pool
        /// </summary>
        /// <returns>The pooled item</returns>
        TPooled Pop();

        /// <summary>
        /// Initialize the pool
        /// </summary>
        /// <param name="factory">The object factory.</param>
        /// <param name="preFill">Whether or not to prefill the pool</param>
        void Initialize(Func<TPooled> factory, bool preFill);

        /// <summary>
        /// ReInitialize the pool
        /// </summary>
        /// <param name="factory">The object factory.</param>
        /// <param name="capacity">The new capacity.</param>
        /// <param name="preFill">Whether or not to prefill the pool</param>
        void ReInitialize(Func<TPooled> factory, int capacity, bool preFill);

        /// <summary>
        /// Clears the pool of all items
        /// </summary>
        void Clear();

        /// <summary>
        /// Gets the number of items in the pool.
        /// </summary>
        long ObjectsInPool { get; }

        /// <summary>
        /// Gets the number of objects thrown away.
        /// </summary>
        long ThrownAwayObjects { get; }

        /// <summary>
        /// Resets the stats of the pool
        /// </summary>
        void ResetStats();

        /// <summary>
        /// Free space in the pool.
        /// </summary>
        int FreeSpace { get; }

        /// <summary>
        /// Gets whether or not the pool supports auto increase, i.e. can soak up any number of items.
        /// </summary>
        bool SupportsAutoIncrease { get; }
    }
}
