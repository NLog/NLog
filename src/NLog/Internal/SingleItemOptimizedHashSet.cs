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
    using System.Collections;
    using System.Collections.Generic;

    /// <summary>
    /// HashSet optimized for single item
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal struct SingleItemOptimizedHashSet<T> : ICollection<T>
    {
        private readonly T _singleItem;
        private HashSet<T> _hashset;
        private readonly IEqualityComparer<T> _comparer;

        private IEqualityComparer<T> Comparer => _comparer ?? EqualityComparer<T>.Default;

        public struct SingleItemScopedInsert : IDisposable
        {
            private readonly T _singleItem;
            private readonly HashSet<T> _hashset;

            /// <summary>
            /// Insert single item on scope start, and remove on scope exit
            /// </summary>
            /// <param name="singleItem">Item to insert in scope</param>
            /// <param name="existing">Existing hashset to update</param>
            /// <param name="forceHashSet">Force allocation of real hashset-container</param>
            /// <param name="comparer">HashSet EqualityComparer</param>
            public SingleItemScopedInsert(T singleItem, ref SingleItemOptimizedHashSet<T> existing, bool forceHashSet, IEqualityComparer<T> comparer)
            {
                _singleItem = singleItem;
                if (existing._hashset != null)
                {
                    existing._hashset.Add(singleItem);
                    _hashset = existing._hashset;
                }
                else if (forceHashSet)
                {
                    existing = new SingleItemOptimizedHashSet<T>(singleItem, existing, comparer);
                    existing.Add(singleItem);
                    _hashset = existing._hashset;
                }
                else
                {
                    existing = new SingleItemOptimizedHashSet<T>(singleItem, existing, comparer);
                    _hashset = null;
                }
            }

            public void Dispose()
            {
                if (_hashset != null)
                {
                    _hashset.Remove(_singleItem);
                }
            }
        }

        public int Count => _hashset?.Count ?? (EqualityComparer<T>.Default.Equals(_singleItem, default(T)) ? 0 : 1);   // Object Equals to default value

        public bool IsReadOnly => false;

        public SingleItemOptimizedHashSet(T singleItem, SingleItemOptimizedHashSet<T> existing, IEqualityComparer<T> comparer = null)
        {
            _comparer = existing._comparer ?? comparer ?? EqualityComparer<T>.Default;
            if (existing._hashset != null)
            {
                _hashset = new HashSet<T>(existing._hashset, _comparer);
                _hashset.Add(singleItem);
                _singleItem = default(T);
            }
            else if (existing.Count == 1)
            {
                _hashset = new HashSet<T>(_comparer);
                _hashset.Add(existing._singleItem);
                _hashset.Add(singleItem);
                _singleItem = default(T);
            }
            else
            {
                _hashset = null;
                _singleItem = singleItem;
            }
        }

        /// <summary>
        /// Add item to collection, if it not already exists
        /// </summary>
        /// <param name="item">Item to insert</param>
        public void Add(T item)
        {
            if (_hashset != null)
            {
                _hashset.Add(item);
            }
            else
            {
                var hashset = new HashSet<T>(Comparer);
                if (Count != 0)
                {
                    hashset.Add(_singleItem);
                }
                hashset.Add(item);
                _hashset = hashset;
            }
        }

        /// <summary>
        /// Clear hashset
        /// </summary>
        public void Clear()
        {
            if (_hashset != null)
            {
                _hashset.Clear();
            }
            else
            {
                _hashset = new HashSet<T>(Comparer);
            }
        }

        /// <summary>
        /// Check if hashset contains item
        /// </summary>
        /// <param name="item"></param>
        /// <returns>Item exists in hashset (true/false)</returns>
        public bool Contains(T item)
        {
            if (_hashset != null)
            {
                return _hashset.Contains(item);
            }
            else
            {
                return Count == 1 && Comparer.Equals(_singleItem, item);
            }
        }

        /// <summary>
        /// Remove item from hashset
        /// </summary>
        /// <param name="item"></param>
        /// <returns>Item removed from hashset (true/false)</returns>
        public bool Remove(T item)
        {
            if (_hashset != null)
            {
                return _hashset.Remove(item);
            }
            else
            {
                _hashset = new HashSet<T>(Comparer);
                return Comparer.Equals(_singleItem, item);
            }
        }

        /// <summary>
        /// Copy items in hashset to array
        /// </summary>
        /// <param name="array">Destination array</param>
        /// <param name="arrayIndex">Array offset</param>
        public void CopyTo(T[] array, int arrayIndex)
        {
            if (_hashset != null)
            {
                _hashset.CopyTo(array, arrayIndex);
            }
            else if (Count == 1)
            {
                array[arrayIndex] = _singleItem;
            }
        }

        /// <summary>
        /// Create hashset enumerator
        /// </summary>
        /// <returns>Enumerator</returns>
        public IEnumerator<T> GetEnumerator()
        {
            if (_hashset != null)
            {
                return _hashset.GetEnumerator();
            }
            else
            {
                return SingleItemEnumerator();
            }
        }

        private IEnumerator<T> SingleItemEnumerator()
        {
            if (Count != 0)
                yield return _singleItem;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public sealed class ReferenceEqualityComparer : IEqualityComparer<T>
        {
            public static readonly ReferenceEqualityComparer Default = new ReferenceEqualityComparer();

            bool IEqualityComparer<T>.Equals(T x, T y)
            {
                return ReferenceEquals(x, y);
            }

            int IEqualityComparer<T>.GetHashCode(T obj)
            {
                return System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj);
            }
        }
    }
}
