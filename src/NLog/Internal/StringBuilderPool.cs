//
// Copyright (c) 2004-2024 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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

    internal sealed class StringBuilderPool
    {
        private StringBuilder? _fastPool;
        private readonly SlowSlot[] _slowPool;

        private struct SlowSlot
        {
            public StringBuilder? Item;

            // Total 64-byte stride to reduce false sharing.
            // Struct with only object reference is not blittable, so [StructLayout(LayoutKind.Sequential, Size = 64)] alone will not affect managed layout.
#pragma warning disable CS0169 // The field is never used
            long _padding1; // NOSONAR
            long _padding2; // NOSONAR
            long _padding3; // NOSONAR
            long _padding4; // NOSONAR
            long _padding5; // NOSONAR
            long _padding6; // NOSONAR
            long _padding7; // NOSONAR
#pragma warning restore CS0169 // The field is never used
        }

        private readonly int _maxBuilderCapacity;

        public int MaxPoolCapacity => _slowPool.Length + 1;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="initialBuilderCapacity">Initial StringBuilder Size</param>
        /// <param name="maxBuilderCapacity">Max StringBuilder Size</param>
        public StringBuilderPool(
            int initialBuilderCapacity = 1024,
            int maxBuilderCapacity = 40 * 1024)
        {
            if (initialBuilderCapacity < 0)
                throw new ArgumentOutOfRangeException(nameof(initialBuilderCapacity));

            if (maxBuilderCapacity < initialBuilderCapacity)
                throw new ArgumentOutOfRangeException(nameof(maxBuilderCapacity));

            _fastPool = new StringBuilder(10 * initialBuilderCapacity);

            _maxBuilderCapacity = maxBuilderCapacity;
            var primePoolCapacity = Environment.ProcessorCount > 8 ? 17 : 11;   // Must be prime to iterate with stride of 2
            _slowPool = new SlowSlot[primePoolCapacity];
            for (int i = 0; i < _slowPool.Length; ++i)
            {
                _slowPool[i].Item = new StringBuilder(initialBuilderCapacity);
            }
            _maxBuilderCapacity = maxBuilderCapacity;
        }

        private int GetPreferredIndex()
        {
#if !NET35
            return (int)((uint)Environment.CurrentManagedThreadId % (uint)_slowPool.Length);
#else
            return 0;
#endif
        }

        /// <summary>
        /// Takes StringBuilder from pool
        /// </summary>
        /// <returns>Allow return to pool</returns>
        public ItemHolder Acquire()
        {
            StringBuilder? item = _fastPool;
            if (item is null || item != Interlocked.CompareExchange(ref _fastPool, null, item))
            {
                int length = _slowPool.Length;
                int index = GetPreferredIndex();

                for (int i = 0; i < length; i++)
                {
                    item = _slowPool[index].Item;
                    if (item != null)
                    {
                        item = Interlocked.Exchange(ref _slowPool[index].Item, null);
                        if (item != null)
                            return new ItemHolder(item, this, index);
                    }

                    index += 2; // Will wrap around to visit all slots. 2 is coprime with prime pool capacity.

                    if (index >= length)
                        index -= length;
                }

                // Pool exhausted.
                return new ItemHolder(new StringBuilder(), null, 0);
            }

            return new ItemHolder(item, this, -1);
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
                        stringBuilder.Capacity = _maxBuilderCapacity;
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
                _slowPool[poolIndex].Item = stringBuilder;
            }
        }

        /// <summary>
        /// Keeps track of acquired pool item
        /// </summary>
        public
#if !NETFRAMEWORK
            readonly
#endif
            struct ItemHolder : IDisposable
        {
            public readonly StringBuilder Item;
            readonly StringBuilderPool? _owner;
            readonly int _poolIndex;

            public ItemHolder(StringBuilder stringBuilder, StringBuilderPool? owner, int poolIndex)
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
                _owner?.Release(Item, _poolIndex);
            }
        }
    }
}
