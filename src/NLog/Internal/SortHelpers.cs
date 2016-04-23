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

        internal delegate Dictionary<TKey, TValue> DictionaryFactory<TKey, TValue>();

        internal delegate List<TValue> ListFactory<TValue>();



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
            return BucketSort(inputs, val => true, keySelector, () => new Dictionary<TKey, List<TValue>>(), () => new List<TValue>());
        }

        public static Dictionary<TKey, List<TValue>> BucketSort<TKey, TValue>(
            this IEnumerable<TValue> inputs,
            Func<TValue, bool> validator,
            KeySelector<TValue, TKey> keySelector,
            DictionaryFactory<TKey, List<TValue>> dictionaryFactory,
            ListFactory<TValue> listFactory)
        {
            var buckets = dictionaryFactory();
            foreach (var input in inputs)
            {
                if (!validator(input))
                {
                    continue;
                }
                var keyValue = keySelector(input);
                List<TValue> eventsInBucket;
                if (!buckets.TryGetValue(keyValue, out eventsInBucket))
                {
                    eventsInBucket = listFactory();
                    buckets.Add(keyValue, eventsInBucket);
                }

                eventsInBucket.Add(input);
            }

            return buckets;
        }
        public static Dictionary<TKey, List<TValue>> BucketSort<TKey, TValue>(
          this ArraySegment<TValue> inputs,
          Func<TValue, bool> validator,
          KeySelector<TValue, TKey> keySelector)
        {
            return BucketSort(inputs, validator, keySelector, () => new Dictionary<TKey, List<TValue>>(), () => new List<TValue>());
        }

        public static Dictionary<TKey, List<TValue>> BucketSort<TKey, TValue>(
           this ArraySegment<TValue> inputs,
           Func<TValue, bool> validator,
           KeySelector<TValue, TKey> keySelector,
           DictionaryFactory<TKey, List<TValue>> dictionaryFactory,
           ListFactory<TValue> listFactory)
        {
            var buckets = dictionaryFactory();
            for (int x = 0; x < inputs.Count; x++)
            {
                var input = inputs.Array[x];
                if (!validator(input))
                {
                    continue;
                }

                var keyValue = keySelector(input);
                List<TValue> eventsInBucket;
                if (!buckets.TryGetValue(keyValue, out eventsInBucket))
                {
                    eventsInBucket = listFactory();
                    buckets.Add(keyValue, eventsInBucket);
                }

                eventsInBucket.Add(input);
            }

            return buckets;
        }
    }
}
