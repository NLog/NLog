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
                        _dictionary[key] = new MruItem(value, version);
                    }
                    return false;   // Already exists
                }
                else
                {
                    var version = ++_currentVersion;
                    if (_dictionary.Count >= _maxCapacity)
                    {
                        // Make 3 sweeps:
                        //  1) Versions below _currentVerison - _maxCapacity / 2
                        //  2) Versions below _currentVerison - _maxCapacity / 10
                        //  3) Versions below _currentVerison - 2
                        var pruneKeys = new List<TKey>(_dictionary.Count);
                        for (int i = 1; i <= 3; ++i)
                        {
                            long oldVersion = version - 2;
                            switch (i)
                            {
                                case 1: oldVersion = version - (int)(_maxCapacity / 1.5); break;
                                case 2: oldVersion = version - _maxCapacity / 10; break;
                            }
                            foreach (var element in _dictionary)
                            {
                                if (element.Value.Version <= oldVersion)
                                {
                                    pruneKeys.Add(element.Key);
                                    if (_dictionary.Count - pruneKeys.Count < _maxCapacity / 1.5)
                                    {
                                        i = 3;
                                        break;  // Do not clear the entire cache
                                    }
                                }
                            }
                        }

                        foreach (var pruneKey in pruneKeys)
                        {
                            _dictionary.Remove(pruneKey);
                        }

                        if (_dictionary.Count >= _maxCapacity)
                        {
                            _dictionary.Clear();
                        }
                    }

                    _dictionary.Add(key, new MruItem(value, version));
                    return true;
                }
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

            if (item.Version != _currentVersion)
            {
                lock (_dictionary)
                {
                    var version = _currentVersion;
                    if (_dictionary.TryGetValue(key, out item))
                    {
                        if (item.Version != version)
                        {
                            _dictionary[key] = new MruItem(item.Value, version);
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

            public MruItem(TValue value, long version)
            {
                Value = value;
                Version = version;
            }
        }
    }
}
