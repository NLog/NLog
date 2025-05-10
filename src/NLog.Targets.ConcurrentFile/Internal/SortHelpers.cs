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
        public static ReadOnlySingleBucketGroupBy<TKey, IList<TValue>> BucketSort<TValue, TKey>(this IList<TValue> inputs, KeySelector<TValue, TKey> keySelector)
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
        public static ReadOnlySingleBucketGroupBy<TKey, IList<TValue>> BucketSort<TValue, TKey>(this IList<TValue> inputs, KeySelector<TValue, TKey> keySelector, IEqualityComparer<TKey> keyComparer)
        {
            if (inputs.Count == 0)
                return new ReadOnlySingleBucketGroupBy<TKey, IList<TValue>>(singleBucket: null, keyComparer);

            Dictionary<TKey, IList<TValue>> buckets = null;
            TKey singleBucketKey = keySelector(inputs[0]);
            for (int i = 1; i < inputs.Count; i++)
            {
                TKey keyValue = keySelector(inputs[i]);
                if (buckets is null)
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

            if (buckets is null)
                return new ReadOnlySingleBucketGroupBy<TKey, IList<TValue>>(new KeyValuePair<TKey, IList<TValue>>(singleBucketKey, inputs), keyComparer);
            else
                return new ReadOnlySingleBucketGroupBy<TKey, IList<TValue>>(buckets, keyComparer);
        }

        private static Dictionary<TKey, IList<TValue>> CreateBucketDictionaryWithValue<TValue, TKey>(IList<TValue> inputs, IEqualityComparer<TKey> keyComparer, int currentIndex, TKey firstBucketKey, TKey nextBucketKey)
        {
            var buckets = new Dictionary<TKey, IList<TValue>>(keyComparer);
            var firstBucket = new List<TValue>(currentIndex);
            for (int i = 0; i < currentIndex; i++)
            {
                firstBucket.Add(inputs[i]);
            }
            buckets[firstBucketKey] = firstBucket;

            var nextBucket = new List<TValue> { inputs[currentIndex] };
            buckets[nextBucketKey] = nextBucket;
            return buckets;
        }

        /// <summary>
        /// Single-Bucket optimized readonly dictionary. Uses normal internally Dictionary if multiple buckets are needed.
        ///
        /// Avoids allocating a new dictionary, when all items are using the same bucket
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        public
#if !NETFRAMEWORK
            readonly
#endif
            struct ReadOnlySingleBucketGroupBy<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
        {
            private
#if !NETFRAMEWORK
                readonly
#endif
                KeyValuePair<TKey, TValue>? _singleBucket;  // Not readonly to avoid struct-copy, and to avoid VerificationException when medium-trust AppDomain
            private readonly Dictionary<TKey, TValue> _multiBucket;
            private readonly IEqualityComparer<TKey> _comparer;
            public IEqualityComparer<TKey> Comparer => _comparer;

            public ReadOnlySingleBucketGroupBy(KeyValuePair<TKey, TValue> singleBucket)
                : this(singleBucket, EqualityComparer<TKey>.Default)
            {
            }

            public ReadOnlySingleBucketGroupBy(Dictionary<TKey, TValue> multiBucket)
                : this(multiBucket, EqualityComparer<TKey>.Default)
            {
            }

            public ReadOnlySingleBucketGroupBy(KeyValuePair<TKey, TValue>? singleBucket, IEqualityComparer<TKey> comparer)
            {
                _comparer = comparer;
                _multiBucket = null;
                _singleBucket = singleBucket;
            }

            public ReadOnlySingleBucketGroupBy(Dictionary<TKey, TValue> multiBucket, IEqualityComparer<TKey> comparer)
            {
                _comparer = comparer;
                _multiBucket = multiBucket;
                _singleBucket = default(KeyValuePair<TKey, TValue>);
            }

            /// <inheritDoc/>
            public int Count => _multiBucket?.Count ?? (_singleBucket.HasValue ? 1 : 0);

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
                            return _multiBuckets.Current;
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
        }
    }
}
