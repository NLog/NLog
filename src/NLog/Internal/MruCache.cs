// 
// Copyright (c) 2004-2017 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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

using System.Collections.Generic;

namespace NLog.Internal
{
    /// <summary>
    /// Most-Recently-Used-Cache, that discards less frequently used items on overflow
    /// </summary>
    internal class MruCache<TKey, TValue>
    {
        private readonly Dictionary<TKey, MruItem> _dictionary;
        private readonly int _maxCapacity;
        private long _currentVersion;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="maxCapacity">Max number of items to hold before discarding</param>
        public MruCache(int maxCapacity)
        {
            _maxCapacity = maxCapacity;
            _dictionary = new Dictionary<TKey, MruItem>(_maxCapacity);
            _currentVersion = 1;
        }

        /// <summary>
        /// Insert item into caceh
        /// </summary>
        /// <param name="key">Dictionary Key</param>
        /// <param name="value">Dictionary Value</param>
        /// <returns>Key did not exist already</returns>
        public bool TryAddValue(TKey key, TValue value)
        {
            lock (_dictionary)
            {
                MruItem item;
                if (_dictionary.TryGetValue(key, out item))
                {
                    var version = _currentVersion;
                    if (item.Version != version || !EqualityComparer<TValue>.Default.Equals(value, item.Value))
                    {
                        _dictionary[key] = new MruItem(value, version, false);
                    }
                    return false;   // Already exists
                }
                else
                {
                    if (_dictionary.Count >= _maxCapacity)
                    {
                        ++_currentVersion;
                        PruneCache();
                    }

                    _dictionary.Add(key, new MruItem(value, _currentVersion, true));
                    return true;
                }
            }
        }

        private void PruneCache()
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
            var pruneKeys = new List<TKey>((int)(_dictionary.Count / 2.5));
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

                long elementGeneration = 0;
                foreach (var element in _dictionary)
                {
                    elementGeneration = element.Value.Version;
                    if (elementGeneration <= oldGeneration || (element.Value.Virgin && (i != 0 || elementGeneration < latestGeneration)))
                    {
                        pruneKeys.Add(element.Key);
                        if (_dictionary.Count - pruneKeys.Count < _maxCapacity / 1.5)
                        {
                            i = 3;
                            break;  // Do not clear the entire cache
                        }
                    }
                    else if (elementGeneration < oldestGeneration)
                    {
                        oldestGeneration = elementGeneration;
                    }
                }
            }

            foreach (var pruneKey in pruneKeys)
            {
                _dictionary.Remove(pruneKey);
            }

            if (_dictionary.Count >= _maxCapacity)
            {
                _dictionary.Clear(); // Failed to perform sweep, fallback to fail safe
            }
        }

        /// <summary>
        /// Lookup existing item in cache
        /// </summary>
        /// <param name="key">Dictionary Key</param>
        /// <param name="value">Dictionary Value [out]</param>
        /// <returns>Key found</returns>
        public bool TryGetValue(TKey key, out TValue value)
        {
            MruItem item;
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
                item = default(MruItem);    // Too many people in the same room
            }

            if (item.Version != _currentVersion || item.Virgin)
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
                            _dictionary[key] = new MruItem(item.Value, version, false);
                        }
                    }
                    else
                    {
                        value = default(TValue);
                        return false;
                    }
                }
            }

            value = item.Value;
            return true;
        }

        struct MruItem
        {
            public readonly TValue Value;
            public readonly long Version;
            public readonly bool Virgin;

            public MruItem(TValue value, long version, bool virgin)
            {
                Value = value;
                Version = version;
                Virgin = virgin;
            }
        }
    }
}
