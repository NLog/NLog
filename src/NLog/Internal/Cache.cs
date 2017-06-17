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

#if NET3_5 || SILVERLIGHT || __IOS__ || __ANDROID__ || WINDOWS_PHONE
#define NO_MEMORY_CACHE
using System.Collections.Generic;
#else
using System.Runtime.Caching;
#endif

using System;
using NLog.Common;

namespace NLog.Internal
{
    /// <summary>
    /// Cache values, create the cache and store it static!
    /// 
    /// Note: <c>null</c> is not allowed as cache value.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class Cache<T> where T : class
    {
        /// <summary>
        /// Unique name, for logging and separating cache
        /// </summary>
        private readonly string _cacheName;

        /// <summary>
        /// Lock writing
        /// </summary>
        private readonly object _cacheLock = new object();

#if NO_MEMORY_CACHE
        private Dictionary<string, T> _cache;
        private int MaxItems = 1000;
#else

        private MemoryCache _cache;
        private CacheItemPolicy _cacheItemPolicy;
#endif

        /// <summary>
        /// Create a cache with cacheName, store this in a <c>static</c> variable.
        /// </summary>
        /// <param name="cacheName">Unique name</param>
        public Cache(string cacheName)
        {
            _cacheName = cacheName;
#if NO_MEMORY_CACHE
            InternalLogger.Trace("Cache {0}: Creating cache without MemoryCache", _cacheName);
            _cache = new Dictionary<string, T>();
#else
            InternalLogger.Trace("Cache {0}: Creating cache with MemoryCache", _cacheName);
            _cacheItemPolicy = new CacheItemPolicy();
            _cache = new MemoryCache(cacheName);
#endif
        }

        /// <summary>
        /// Get the value, or create and add to the cache
        /// </summary>
        /// <param name="key">Key to get the item</param>
        /// <param name="create">Function to create the value</param>
        /// <returns></returns>
        public T GetOrCreate(string key, Func<T> create)
        {
            var value = GetValue(key);

            if (value != null)
            {
                return value;
            }

            lock (_cacheLock)
            {
                //Check again if someone already wrote to the case.
                value = GetValue(key);

                if (value != null)
                {
                    return value;
                }

                value = create();
                if (value == null)
                {
                    InternalLogger.Warn("Cache {0}: creation of cache value yields null and won't be added to the cache", _cacheName);
                }
                else
                {
                    Add(key, value);
                }
                return value;
            }
        }


        /// <summary>
        /// Add item to the cache
        /// </summary>
        /// <param name="key">Key of the cache item</param>
        /// <param name="value"></param>
        private void Add(string key, T value)
        {
            InternalLogger.Trace("Cache {0}: Add items with key '{1}' to the cache", _cacheName, key);

#if NO_MEMORY_CACHE

            if (_cache.Count > MaxItems)
            {
                InternalLogger.Trace("Cache {0}: Clear cache because of max ({1}) entries. ", _cacheName, MaxItems);
                _cache.Clear();
            }

            _cache[key] = value;
#else
            _cache.Add(key, value, _cacheItemPolicy);
#endif

        }

        /// <summary>
        /// Get value, returns null if the items does not exist,
        /// </summary>
        /// <param name="key">Key of the cache item</param>
        /// <returns>null if and only if not exists.</returns>
        private T GetValue(string key)
        {
            // ReSharper disable InconsistentlySynchronizedField
#if NO_MEMORY_CACHE
            return _cache[key] as T;
#else
            return _cache.Get(key) as T;
            // ReSharper restore InconsistentlySynchronizedField
#endif
        }
    }
}
