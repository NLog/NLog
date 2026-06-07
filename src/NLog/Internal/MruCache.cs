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
    using System.Collections.Generic;
#if !NET35
    using System.Collections.Concurrent;
#endif

    /// <summary>
    /// Most-Recently-Used-Cache, that discards less frequently used items on overflow
    /// </summary>
    internal sealed class MruCache<TKey, TValue>
#if NETSTANDARD2_1_OR_GREATER || NET
        where TKey : notnull
#endif
    {
#if NET35
        private readonly Dictionary<TKey, MruCacheItem> _dictionary;
#else
        private readonly ConcurrentDictionary<TKey, MruCacheItem> _dictionary;
#endif
        private readonly int _maxCapacity;
        private long _currentVersion;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="maxCapacity">Maximum number of items the cache will hold before discarding (Avoid higher than 10000 to ensure fast cache prune).</param>
        public MruCache(int maxCapacity)
        {
            _maxCapacity = maxCapacity;
#if NET35
            _dictionary = new Dictionary<TKey, MruCacheItem>(_maxCapacity / 4);
#else
            _dictionary = new ConcurrentDictionary<TKey, MruCacheItem>(concurrencyLevel: 1, capacity: maxCapacity / 4);
#endif
            _currentVersion = 1;
        }

        /// <summary>
        /// Attempt to insert item into cache.
        /// </summary>
        /// <param name="key">Key of the item to be inserted in the cache.</param>
        /// <param name="value">Value of the item to be inserted in the cache.</param>
        /// <returns><see langword="true"/> when the key does not already exist in the cache, <see langword="false"/> otherwise.</returns>
        public bool TryAddValue(TKey key, TValue value)
        {
            lock (_dictionary)
            {
                MruCacheItem item;
                if (_dictionary.TryGetValue(key, out item))
                {
                    var version = _currentVersion;
                    if (item.Version != version || !EqualityComparer<TValue>.Default.Equals(value, item.Value))
                    {
                        _dictionary[key] = new MruCacheItem(value, version, false);
                    }
                    return false;   // Already exists
                }
                else
                {
                    var orgSize = _dictionary.Count;    // Count hurts performance when ConcurrentDictionary
                    if (orgSize >= _maxCapacity)
                    {
                        ++_currentVersion;
                        PruneCache(orgSize);
                    }

                    // Insert as new virgin, so old virgins are more likely to be pruned first
                    _dictionary[key] = new MruCacheItem(value, _currentVersion, true);
                    return true;
                }
            }
        }

        private void PruneCache(int orgSize)
        {
            // There 3 priorities:
            //  - High Priority - Non-Virgins with version very close to _currentVersion
            //  - Medium Priority - Version close to _currentVersion
            //  - Low Priority - Old-Virgins
            // Make 3 sweeps:
            //  - Kill all the old ones
            //  - Kill all the virgins
            //  - Slaughterhouse
            long latestGeneration = _currentVersion - 2;
            long oldestGeneration = 1;
#if NET35
            var pruneKeys = new List<TKey>((int)(orgSize / 2.5));
#endif
            for (int i = 0; i < 3; ++i)
            {
                long oldGeneration = _currentVersion - 5;
                switch (i)
                {
                    case 0: oldGeneration = _currentVersion - (int)(_maxCapacity / 1.5); break;
                    case 1: oldGeneration = _currentVersion - 10; break;
                }
                if (oldGeneration < oldestGeneration)
                    oldGeneration = oldestGeneration;

                oldestGeneration = long.MaxValue;

                long elementGeneration;
                foreach (var element in _dictionary)
                {
                    elementGeneration = element.Value.Version;
                    if (elementGeneration <= oldGeneration || (element.Value.Virgin && (i != 0 || elementGeneration < latestGeneration)))
                    {
                        --orgSize;
#if NET35
                        pruneKeys.Add(element.Key);
#else
                        _dictionary.TryRemove(element.Key, out _);
#endif
                        if (orgSize < _maxCapacity / 1.5)
                        {
                            i = 3;
                            break;  // Found enough candidates, avoid clearing the entire cache
                        }
                    }
                    else if (elementGeneration < oldestGeneration)
                    {
                        oldestGeneration = elementGeneration;
                    }
                }
            }

#if NET35
            foreach (var pruneKey in pruneKeys)
            {
                _dictionary.Remove(pruneKey);
            }
            orgSize = _dictionary.Count;    // Multiple sweeps may have found duplicate candidates
#endif

            if (orgSize >= _maxCapacity)
            {
                _dictionary.Clear(); // Failed to perform sweep, fallback to fail safe
            }
        }

        /// <summary>
        /// Lookup existing item in cache.
        /// </summary>
        /// <param name="key">Key of the item to be searched in the cache.</param>
        /// <param name="value">Output value of the item found in the cache.</param>
        /// <returns><see langword="true"/> when the key is found in the cache, <see langword="false"/> otherwise.</returns>
        public bool TryGetValue(TKey key, out TValue? value)
        {
#if !NET35
            if (!_dictionary.TryGetValue(key, out var item))
            {
                value = default(TValue);
                return false;
            }
#else
            MruCacheItem item;
            try
            {
                if (!_dictionary.TryGetValue(key, out item))
                {
                    value = default(TValue);
                    return false;
                }
            }
            catch
            {
                item = default(MruCacheItem);    // Too many people in the same room
            }
#endif

            if (item.Version != _currentVersion || item.Virgin)
            {
                if (!TryMarkRecentlyUsed(key, out item))
                {
                    value = default(TValue);
                    return false;
                }
            }

            value = item.Value;
            return true;
        }

        private bool TryMarkRecentlyUsed(TKey key, out MruCacheItem item)
        {
            // Update item version to mark as recently used
            lock (_dictionary)
            {
                var version = _currentVersion;
                if (_dictionary.TryGetValue(key, out item))
                {
                    if (item.Version != version || item.Virgin)
                    {
                        if (item.Virgin)
                        {
                            version = ++_currentVersion;
                        }
                        _dictionary[key] = new MruCacheItem(item.Value, version, false);
                    }
                    return true;
                }
                else
                {
                    item = default(MruCacheItem);
                    return false;
                }
            }
        }

        private
#if !NETFRAMEWORK
        readonly
#endif
        struct MruCacheItem
        {
            public readonly TValue Value;
            public readonly long Version;
            public readonly bool Virgin;

            public MruCacheItem(TValue value, long version, bool virgin)
            {
                Value = value;
                Version = version;
                Virgin = virgin;
            }
        }
    }
}
