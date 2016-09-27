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

namespace NLog.Internal
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using NLog.Common;

    /// <summary>
    /// Provides helpers to sort log events and associated continuations.
    /// </summary>
    internal static class SortHelpers
    {
        /// <summary>
        /// Key selector delegate.
        /// </summary>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <param name="value">Value to extract key information from.</param>
        /// <returns>Key selected from log event.</returns>
        internal delegate TKey KeySelector<TValue, TKey>(TValue value);

        /// <summary>
        /// Performs bucket sort (group by) on an array of items and returns a dictionary for easy traversal of the result set.
        /// </summary>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <param name="inputs">The inputs.</param>
        /// <param name="keySelector">The key selector function.</param>
        /// <returns>
        /// Dictionary where keys are unique input keys, and values are lists of <see cref="AsyncLogEventInfo"/>.
        /// </returns>
        public static IDictionary<TKey, IList<TValue>> BucketSort<TValue, TKey>(this IEnumerable<TValue> inputs, KeySelector<TValue, TKey> keySelector)
        {
            var buckets = new Dictionary<TKey, IList<TValue>>();

            foreach (var input in inputs)
            {
                var keyValue = keySelector(input);
                IList<TValue> eventsInBucket;
                if (!buckets.TryGetValue(keyValue, out eventsInBucket))
                {
                    eventsInBucket = new List<TValue>();
                    buckets.Add(keyValue, eventsInBucket);
                }

                eventsInBucket.Add(input);
            }

            return buckets;
        }

        /// <summary>
        /// Performs bucket sort (group by) on an array of items and returns a dictionary for easy traversal of the result set.
        /// </summary>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <param name="inputs">The inputs.</param>
        /// <param name="keySelector">The key selector function.</param>
        /// <returns>
        /// Dictionary where keys are unique input keys, and values are lists of <see cref="AsyncLogEventInfo"/>.
        /// </returns>
        public static ReadOnlySingleBucketDictionary<TKey, IList<TValue>> BucketSort<TValue, TKey>(this ArraySegment<TValue> inputs, KeySelector<TValue, TKey> keySelector)
        {
            Dictionary<TKey, IList<TValue>> buckets = null;
            bool singleBucketFirstKey = false;
            TKey singleBucketKey = default(TKey);
            EqualityComparer<TKey> c = EqualityComparer<TKey>.Default;
            for (int i = inputs.Offset; i < (inputs.Offset + inputs.Count); ++i)
            {
                TKey keyValue = keySelector(inputs.Array[i]);
                if (!singleBucketFirstKey)
                {
                    singleBucketFirstKey = true;
                    singleBucketKey = keyValue;
                }
                else if (buckets == null)
                {
                    if (!c.Equals(singleBucketKey, keyValue))
                    {
                        // Multiple buckets needed, allocate full dictionary
                        buckets = new Dictionary<TKey, IList<TValue>>();
                        var bucket = new List<TValue>(i);
                        for (int j = inputs.Offset; j < i; ++j)
                        {
                            bucket.Add(inputs.Array[j]);
                        }
                        buckets[singleBucketKey] = bucket;
                        bucket = new List<TValue>();
                        bucket.Add(inputs.Array[i]);
                        buckets[keyValue] = bucket;
                    }
                }
                else
                {
                    IList<TValue> eventsInBucket;
                    if (!buckets.TryGetValue(keyValue, out eventsInBucket))
                    {
                        eventsInBucket = new List<TValue>();
                        buckets.Add(keyValue, eventsInBucket);
                    }
                    eventsInBucket.Add(inputs.Array[i]);
                }
            }
            if (buckets != null || inputs.Count == 0)
                return new ReadOnlySingleBucketDictionary<TKey, IList<TValue>>(buckets != null ? buckets : new Dictionary<TKey, IList<TValue>>());
            else
                return new ReadOnlySingleBucketDictionary<TKey, IList<TValue>>(new KeyValuePair<TKey, IList<TValue>>(singleBucketKey, new ReadOnlyArrayList<TValue>(inputs)));
        }

        /// <summary>
        /// Single-Bucket optimized readonly dictionary. Uses normal internally Dictionary if multiple buckets are needed.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam> 
        public struct ReadOnlySingleBucketDictionary<TKey, TValue> : IDictionary<TKey, TValue>
        {
            readonly KeyValuePair<TKey, TValue>? _singleBucket;
            readonly Dictionary<TKey, TValue> _multiBucket;
            readonly IEqualityComparer<TKey> _comparer;
            public IEqualityComparer<TKey> Comparer { get { return _comparer; } }

            public ReadOnlySingleBucketDictionary(KeyValuePair<TKey, TValue> singleBucket)
                : this(singleBucket, EqualityComparer<TKey>.Default)
            {
            }

            public ReadOnlySingleBucketDictionary(Dictionary<TKey, TValue> multiBucket)
                : this(multiBucket, EqualityComparer<TKey>.Default)
            {
            }

            public ReadOnlySingleBucketDictionary(KeyValuePair<TKey, TValue> singleBucket, IEqualityComparer<TKey> comparer)
            {
                _comparer = comparer;
                _multiBucket = null;
                _singleBucket = singleBucket;
            }

            public ReadOnlySingleBucketDictionary(Dictionary<TKey, TValue> multiBucket, IEqualityComparer<TKey> comparer)
            {
                _comparer = comparer;
                _multiBucket = multiBucket;
                _singleBucket = default(KeyValuePair<TKey, TValue>);
            }

            public int Count { get { if (_multiBucket != null) return _multiBucket.Count; else if (_singleBucket.HasValue) return 1; else return 0; } }

            public ICollection<TKey> Keys
            {
                get
                {
                    if (_multiBucket != null)
                        return _multiBucket.Keys;
                    else if (_singleBucket.HasValue)
                        return new[] { _singleBucket.Value.Key };
                    else
                        return new TKey[0];
                }
            }

            public ICollection<TValue> Values
            {
                get
                {
                    if (_multiBucket != null)
                        return _multiBucket.Values;
                    else if (_singleBucket.HasValue)
                        return new TValue[] { _singleBucket.Value.Value };
                    else
                        return new TValue[] { };
                }
            }

            public bool IsReadOnly { get { return true; } }

            public TValue this[TKey key]
            {
                get
                {
                    if (_multiBucket != null)
                        return _multiBucket[key];
                    else if (_singleBucket.HasValue && _comparer.Equals(_singleBucket.Value.Key, key))
                        return _singleBucket.Value.Value;
                    else
                        throw new System.Collections.Generic.KeyNotFoundException();
                }
                set
                {
                    throw new NotSupportedException("Readonly");
                }
            }

            public struct Enumerator : IEnumerator<KeyValuePair<TKey, TValue>>
            {
                bool _singleBucketFirstRead;
                readonly KeyValuePair<TKey, TValue> _singleBucket;
                readonly IEnumerator<KeyValuePair<TKey, TValue>> _multiBuckets;

                internal Enumerator(Dictionary<TKey, TValue> multiBucket)
                {
                    _singleBucketFirstRead = false;
                    _singleBucket = default(KeyValuePair<TKey, TValue>);
                    _multiBuckets = multiBucket.GetEnumerator();
                }

                internal Enumerator(KeyValuePair<TKey, TValue> singleBucket)
                {
                    _singleBucketFirstRead = false;
                    _singleBucket = singleBucket;
                    _multiBuckets = null;
                }

                public KeyValuePair<TKey, TValue> Current
                {
                    get
                    {
                        if (_multiBuckets != null)
                            return new KeyValuePair<TKey, TValue>(_multiBuckets.Current.Key, _multiBuckets.Current.Value);
                        else
                            return new KeyValuePair<TKey, TValue>(_singleBucket.Key, _singleBucket.Value);
                    }
                }

                object IEnumerator.Current { get { return Current; } }

                public void Dispose()
                {
                    if (_multiBuckets != null)
                        _multiBuckets.Dispose();
                }

                public bool MoveNext()
                {
                    if (_multiBuckets != null)
                        return _multiBuckets.MoveNext();
                    else if (_singleBucketFirstRead)
                        return false;
                    else
                        return _singleBucketFirstRead = true;

                }

                public void Reset()
                {
                    if (_multiBuckets != null)
                        _multiBuckets.Reset();
                }
            }

            public Enumerator GetEnumerator()
            {
                if (_multiBucket != null)
                    return new Enumerator(_multiBucket);
                else if (_singleBucket.HasValue)
                    return new Enumerator(_singleBucket.Value);
                else
                    return new Enumerator(new Dictionary<TKey, TValue>());
            }

            IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
            {
                return GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public bool ContainsKey(TKey key)
            {
                if (_multiBucket != null)
                    return _multiBucket.ContainsKey(key);
                else if (_singleBucket.HasValue)
                    return _comparer.Equals(_singleBucket.Value.Key, key);
                else
                    return false;
            }

            public void Add(TKey key, TValue value)
            {
                throw new NotSupportedException();  // Readonly
            }

            public bool Remove(TKey key)
            {
                throw new NotSupportedException();  // Readonly
            }

            public bool TryGetValue(TKey key, out TValue value)
            {
                if (_multiBucket != null)
                {
                    return _multiBucket.TryGetValue(key, out value);
                }
                else if (_singleBucket.HasValue && _comparer.Equals(_singleBucket.Value.Key, key))
                {
                    value = _singleBucket.Value.Value;
                    return true;
                }
                else
                {
                    value = default(TValue);
                    return false;
                }
            }

            public void Add(KeyValuePair<TKey, TValue> item)
            {
                throw new NotSupportedException();  // Readonly
            }

            public void Clear()
            {
                throw new NotSupportedException();  // Readonly
            }

            public bool Contains(KeyValuePair<TKey, TValue> item)
            {
                if (_multiBucket != null)
                    return ((IDictionary<TKey, TValue>)_multiBucket).Contains(item);
                else if (_singleBucket.HasValue)
                    return _comparer.Equals(_singleBucket.Value.Key, item.Key) && EqualityComparer<TValue>.Default.Equals(_singleBucket.Value.Value, item.Value);
                else
                    return false;
            }

            public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
            {
                if (_multiBucket != null)
                    ((IDictionary<TKey, TValue>)_multiBucket).CopyTo(array, arrayIndex);
                else if (_singleBucket.HasValue)
                    array[arrayIndex] = _singleBucket.Value;
            }

            public bool Remove(KeyValuePair<TKey, TValue> item)
            {
                throw new NotSupportedException();  // Readonly
            }
        }

        internal struct ReadOnlyArrayList<TValue> : IList<TValue>
        {
            readonly ArraySegment<TValue> _singleBucket;

            public ReadOnlyArrayList(ArraySegment<TValue> singleBucket)
            {
                _singleBucket = singleBucket;
            }

            public TValue this[int index]
            {
                get
                {
                    return _singleBucket.Array[_singleBucket.Offset + index];
                }
                set
                {
                    throw new NotSupportedException("Readonly");
                }
            }

            public int Count
            {
                get
                {
                    return _singleBucket.Count;
                }
            }

            bool ICollection<TValue>.IsReadOnly { get { return true; } }

            IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator()
            {
                return GetSingleBucketEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetSingleBucketEnumerator();
            }

            IEnumerator<TValue> GetSingleBucketEnumerator()
            {
                for (int i = _singleBucket.Offset; i < (_singleBucket.Offset + _singleBucket.Count); i++)
                    yield return _singleBucket.Array[i];
            }

            int IList<TValue>.IndexOf(TValue item)
            {
                EqualityComparer<TValue> c = EqualityComparer<TValue>.Default;
                for (int i = 0; i < Count; ++i)
                    if (c.Equals(this[i], item))
                        return i;
                return -1;
            }

            void IList<TValue>.Insert(int index, TValue item)
            {
                throw new NotSupportedException("Readonly");
            }

            void IList<TValue>.RemoveAt(int index)
            {
                throw new NotSupportedException("Readonly");
            }

            void ICollection<TValue>.Add(TValue item)
            {
                throw new NotSupportedException("Readonly");
            }

            void ICollection<TValue>.Clear()
            {
                throw new NotSupportedException("Readonly");
            }

            bool ICollection<TValue>.Contains(TValue item)
            {
                return ((IList<TValue>)this).IndexOf(item) != -1;
            }

            void ICollection<TValue>.CopyTo(TValue[] array, int arrayIndex)
            {
                for (int i = 0; i < Count; ++i)
                    array[arrayIndex + i] = this[i];
            }

            bool ICollection<TValue>.Remove(TValue item)
            {
                throw new NotSupportedException("Readonly");
            }
        }
    }
}
