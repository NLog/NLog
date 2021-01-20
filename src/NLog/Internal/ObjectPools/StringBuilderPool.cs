// 
// Copyright (c) 2004-2020 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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

namespace NLog.Internal
{
    using System;
    using System.Text;
    using System.Threading;

    internal class StringBuilderPool
    {
        private StringBuilder _fastPool;
        private readonly StringBuilder[] _slowPool;
        private readonly int _maxBuilderCapacity;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="poolCapacity">Max number of items</param>
        /// <param name="initialBuilderCapacity">Initial StringBuilder Size</param>
        /// <param name="maxBuilderCapacity">Max StringBuilder Size</param>
        public StringBuilderPool(int poolCapacity, int initialBuilderCapacity = 1024, int maxBuilderCapacity = 512 * 1024)
        {
            _fastPool = new StringBuilder(10 * initialBuilderCapacity);
            _slowPool = new StringBuilder[poolCapacity];
            for (int i = 0; i < _slowPool.Length; ++i)
            {
                _slowPool[i] = new StringBuilder(initialBuilderCapacity);
            }
            _maxBuilderCapacity = maxBuilderCapacity;
        }

        /// <summary>
        /// Takes StringBuilder from pool
        /// </summary>
        /// <returns>Allow return to pool</returns>
        public ItemHolder Acquire()
        {
            StringBuilder item = _fastPool;
            if (item == null || item != Interlocked.CompareExchange(ref _fastPool, null, item))
            {
                for (int i = 0; i < _slowPool.Length; i++)
                {
                    item = _slowPool[i];
                    if (item != null && item == Interlocked.CompareExchange(ref _slowPool[i], null, item))
                    {
                        return new ItemHolder(item, this, i);
                    }
                }

                return new ItemHolder(new StringBuilder(), null, 0);
            }
            else
            {
                return new ItemHolder(item, this, -1);
            }
        }

        /// <summary>
        /// Releases StringBuilder back to pool at its right place
        /// </summary>
        private void Release(StringBuilder stringBuilder, int poolIndex)
        {
            if (stringBuilder.Length > _maxBuilderCapacity)
            {
                // Avoid high memory usage by not keeping huge StringBuilders alive (Except one StringBuilder)
                int maxBuilderCapacity = poolIndex == -1 ? _maxBuilderCapacity * 10 : _maxBuilderCapacity;
                if (stringBuilder.Length > maxBuilderCapacity)
                {
                    stringBuilder.Remove(0, stringBuilder.Length - 1);  // Attempt soft clear that skips re-allocation
                    if (stringBuilder.Capacity > maxBuilderCapacity)
                    {
                        stringBuilder = new StringBuilder(maxBuilderCapacity / 2);
                    }
                }
            }

            stringBuilder.ClearBuilder();

            if (poolIndex == -1)
            {
                _fastPool = stringBuilder;
            }
            else
            {
                _slowPool[poolIndex] = stringBuilder;
            }
        }

        /// <summary>
        /// Keeps track of acquired pool item
        /// </summary>
        public struct ItemHolder : IDisposable
        {
            public readonly StringBuilder Item;
            readonly StringBuilderPool _owner;
            readonly int _poolIndex;

            public ItemHolder(StringBuilder stringBuilder, StringBuilderPool owner, int poolIndex)
            {
                Item = stringBuilder;
                _owner = owner;
                _poolIndex = poolIndex;
            }

            /// <summary>
            /// Releases pool item back into pool
            /// </summary>
            public void Dispose()
            {
                if (_owner != null)
                {
                    _owner.Release(Item, _poolIndex);
                }
            }
        }
    }
}
