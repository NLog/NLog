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
    using Common;

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
        internal delegate TKey KeySelector<in TValue, out TKey>(TValue value);

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
        public static Dictionary<TKey, List<TValue>> BucketSort<TValue, TKey>(this IEnumerable<TValue> inputs, KeySelector<TValue, TKey> keySelector)
        {
            var buckets = new Dictionary<TKey, List<TValue>>();

            foreach (var input in inputs)
            {
                var keyValue = keySelector(input);
                if (!buckets.TryGetValue(keyValue, out var eventsInBucket))
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
        public static ReadOnlySingleBucketDictionary<TKey, IList<TValue>> BucketSort<TValue, TKey>(this IList<TValue> inputs, KeySelector<TValue, TKey> keySelector)
        {
            return BucketSort(inputs, keySelector, EqualityComparer<TKey>.Default);
        }

        /// <summary>
        /// Performs bucket sort (group by) on an array of items and returns a dictionary for easy traversal of the result set.
        /// </summary>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <param name="inputs">The inputs.</param>
        /// <param name="keySelector">The key selector function.</param>
        /// <param name="keyComparer">The key comparer function.</param>
        /// <returns>
        /// Dictionary where keys are unique input keys, and values are lists of <see cref="AsyncLogEventInfo"/>.
        /// </returns>
        public static ReadOnlySingleBucketDictionary<TKey, IList<TValue>> BucketSort<TValue, TKey>(this IList<TValue> inputs, KeySelector<TValue, TKey> keySelector, IEqualityComparer<TKey> keyComparer)
        {
            Dictionary<TKey, IList<TValue>> buckets = null;
            bool singleBucketFirstKey = false;
            TKey singleBucketKey = default(TKey);
            for (int i = 0; i < inputs.Count; i++)
            {
                TKey keyValue = keySelector(inputs[i]);
                if (!singleBucketFirstKey)
                {
                    singleBucketFirstKey = true;
                    singleBucketKey = keyValue;
                }
                else if (buckets == null)
                {
                    if (!keyComparer.Equals(singleBucketKey, keyValue))
                    {
                        // Multiple buckets needed, allocate full dictionary
                        buckets = CreateBucketDictionaryWithValue(inputs, keyComparer, i, singleBucketKey, keyValue);
                    }
                }
                else
                {
                    if (!buckets.TryGetValue(keyValue, out var eventsInBucket))
                    {
                        eventsInBucket = new List<TValue>();
                        buckets.Add(keyValue, eventsInBucket);
                    }
                    eventsInBucket.Add(inputs[i]);
                }
            }

            if (buckets != null)
            {
                return new ReadOnlySingleBucketDictionary<TKey, IList<TValue>>(buckets, keyComparer);
            }
            else
            {
                return new ReadOnlySingleBucketDictionary<TKey, IList<TValue>>(new KeyValuePair<TKey, IList<TValue>>(singleBucketKey, inputs), keyComparer);
            }
        }

        private static Dictionary<TKey, IList<TValue>> CreateBucketDictionaryWithValue<TValue, TKey>(IList<TValue> inputs, IEqualityComparer<TKey> keyComparer, int currentIndex, TKey singleBucketKey, TKey keyValue)
        {
            var buckets = new Dictionary<TKey, IList<TValue>>(keyComparer);
            var bucket = new List<TValue>(currentIndex);
            for (int i = 0; i < currentIndex; i++)
            {
                bucket.Add(inputs[i]);
            }

            buckets[singleBucketKey] = bucket;
            bucket = new List<TValue> {inputs[currentIndex]};
            buckets[keyValue] = bucket;
            return buckets;
        }

        /// <summary>
        /// Single-Bucket optimized readonly dictionary. Uses normal internally Dictionary if multiple buckets are needed.
        ///
        /// Avoids allocating a new dictionary, when all items are using the same bucket
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        public struct ReadOnlySingleBucketDictionary<TKey, TValue> : IDictionary<TKey, TValue>
        {
            KeyValuePair<TKey, TValue>? _singleBucket;  // Not readonly to avoid struct-copy, and to avoid VerificationException when medium-trust AppDomain
            readonly Dictionary<TKey, TValue> _multiBucket;
            readonly IEqualityComparer<TKey> _comparer;
            public IEqualityComparer<TKey> Comparer => _comparer;

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

            /// <inheritDoc/>
            public int Count { get { if (_multiBucket != null) return _multiBucket.Count; else if (_singleBucket.HasValue) return 1; else return 0; } }

            /// <inheritDoc/>
            public ICollection<TKey> Keys
            {
                get
                {
                    if (_multiBucket != null)
                        return _multiBucket.Keys;
                    else if (_singleBucket.HasValue)
                        return new[] { _singleBucket.Value.Key };
                    else
                        return ArrayHelper.Empty<TKey>();
                }
            }

            /// <inheritDoc/>
            public ICollection<TValue> Values
            {
                get
                {
                    if (_multiBucket != null)
                        return _multiBucket.Values;
                    else if (_singleBucket.HasValue)
                        return new TValue[] { _singleBucket.Value.Value };
                    else
                        return ArrayHelper.Empty<TValue>();
                }
            }

            /// <inheritDoc/>
            public bool IsReadOnly => true;

            /// <summary>
            /// Allows direct lookup of existing keys. If trying to access non-existing key exception is thrown.
            /// Consider to use <see cref="TryGetValue(TKey, out TValue)"/> instead for better safety.
            /// </summary>
            /// <param name="key">Key value for lookup</param>
            /// <returns>Mapped value found</returns>
            public TValue this[TKey key]
            {
                get
                {
                    if (_multiBucket != null)
                        return _multiBucket[key];
                    else if (_singleBucket.HasValue && _comparer.Equals(_singleBucket.Value.Key, key))
                        return _singleBucket.Value.Value;
                    else
                        throw new KeyNotFoundException();
                }
                set => throw new NotSupportedException("Readonly");
            }

            /// <summary>
            /// Non-Allocating struct-enumerator
            /// </summary>
            public struct Enumerator : IEnumerator<KeyValuePair<TKey, TValue>>
            {
                bool _singleBucketFirstRead;
                KeyValuePair<TKey, TValue> _singleBucket;   // Not readonly to avoid struct-copy, and to avoid VerificationException when medium-trust AppDomain
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

                object IEnumerator.Current => Current;

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
                    else
                        _singleBucketFirstRead = false;
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

            /// <inheritDoc/>
            IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
            {
                return GetEnumerator();
            }

            /// <inheritDoc/>
            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            /// <inheritDoc/>
            public bool ContainsKey(TKey key)
            {
                if (_multiBucket != null)
                    return _multiBucket.ContainsKey(key);
                else if (_singleBucket.HasValue)
                    return _comparer.Equals(_singleBucket.Value.Key, key);
                else
                    return false;
            }

            /// <summary>Will always throw, as dictionary is readonly</summary>
            public void Add(TKey key, TValue value)
            {
                throw new NotSupportedException();  // Readonly
            }

            /// <summary>Will always throw, as dictionary is readonly</summary>
            public bool Remove(TKey key)
            {
                throw new NotSupportedException();  // Readonly
            }

            /// <inheritDoc/>
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

            /// <summary>Will always throw, as dictionary is readonly</summary>
            public void Add(KeyValuePair<TKey, TValue> item)
            {
                throw new NotSupportedException();  // Readonly
            }

            /// <summary>Will always throw, as dictionary is readonly</summary>
            public void Clear()
            {
                throw new NotSupportedException();  // Readonly
            }

            /// <inheritDoc/>
            public bool Contains(KeyValuePair<TKey, TValue> item)
            {
                if (_multiBucket != null)
                    return ((IDictionary<TKey, TValue>)_multiBucket).Contains(item);
                else if (_singleBucket.HasValue)
                    return _comparer.Equals(_singleBucket.Value.Key, item.Key) && EqualityComparer<TValue>.Default.Equals(_singleBucket.Value.Value, item.Value);
                else
                    return false;
            }

            /// <inheritDoc/>
            public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
            {
                if (_multiBucket != null)
                    ((IDictionary<TKey, TValue>)_multiBucket).CopyTo(array, arrayIndex);
                else if (_singleBucket.HasValue)
                    array[arrayIndex] = _singleBucket.Value;
            }

            /// <summary>Will always throw, as dictionary is readonly</summary>
            public bool Remove(KeyValuePair<TKey, TValue> item)
            {
                throw new NotSupportedException();  // Readonly
            }
        }
    }
}
