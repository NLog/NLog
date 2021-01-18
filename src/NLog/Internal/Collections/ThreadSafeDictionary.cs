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
    using System.Diagnostics;
    using System.Linq;

    [DebuggerDisplay("Count = {Count}")]
    internal class ThreadSafeDictionary<TKey, TValue> : IDictionary<TKey, TValue>
    {
        private readonly object _lockObject = new object();
        private Dictionary<TKey, TValue> _dict;
        private Dictionary<TKey, TValue> _dictReadOnly;  // Reset cache on change

        public ThreadSafeDictionary()
            :this(EqualityComparer<TKey>.Default)
        {
        }

        public ThreadSafeDictionary(IEqualityComparer<TKey> comparer)
        {
            _dict = new Dictionary<TKey, TValue>(comparer);
        }

        public ThreadSafeDictionary(ThreadSafeDictionary<TKey, TValue> source)
        {
            var sourceDictionary = source.GetReadOnlyDict();
            _dict = new Dictionary<TKey, TValue>(sourceDictionary.Count, sourceDictionary.Comparer);
            foreach (var item in sourceDictionary)
                _dict.Add(item.Key, item.Value);
        }

        public TValue this[TKey key]
        {
            get => GetReadOnlyDict()[key];
            set
            {
                lock (_lockObject)
                {
                    GetWritableDict()[key] = value;
                }
            }
        }

        public IEqualityComparer<TKey> Comparer => _dict.Comparer;

        public ICollection<TKey> Keys => GetReadOnlyDict().Keys;

        public ICollection<TValue> Values => GetReadOnlyDict().Values;

        public int Count => GetReadOnlyDict().Count;

        public bool IsReadOnly => false;

        public void Add(TKey key, TValue value)
        {
            lock (_lockObject)
            {
                GetWritableDict().Add(key, value);
            }
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            lock (_lockObject)
            {
                GetWritableDict().Add(item.Key, item.Value);
            }
        }

        public void Clear()
        {
            lock (_lockObject)
            {
                GetWritableDict(true);
            }
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return GetReadOnlyDict().Contains(item);
        }

        public bool ContainsKey(TKey key)
        {
            return GetReadOnlyDict().ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
           ((IDictionary<TKey,TValue>)GetReadOnlyDict()).CopyTo(array, arrayIndex);
        }

        public void CopyFrom(IDictionary<TKey, TValue> source)
        {
            if (!ReferenceEquals(this, source) && source?.Count > 0)
            {
                lock (_lockObject)
                {
                    var destDict = GetWritableDict();
                    foreach (var item in source)
                        destDict[item.Key] = item.Value;
                }
            }
        }

        public bool Remove(TKey key)
        {
            lock (_lockObject)
            {
                return GetWritableDict().Remove(key);
            }
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            lock (_lockObject)
            {
                return GetWritableDict().Remove(item);
            }
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return GetReadOnlyDict().TryGetValue(key, out value);
        }

        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
        {
            return GetReadOnlyDict().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetReadOnlyDict().GetEnumerator();
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(GetReadOnlyDict().GetEnumerator());
        }

        public struct Enumerator : IEnumerator<KeyValuePair<TKey, TValue>>
        {
            Dictionary<TKey, TValue>.Enumerator _enumerator;

            public Enumerator(Dictionary<TKey, TValue>.Enumerator enumerator)
            {
                _enumerator = enumerator;
            }

            public KeyValuePair<TKey, TValue> Current => _enumerator.Current;

            object IEnumerator.Current => _enumerator.Current;

            public void Dispose()
            {
                _enumerator.Dispose();
            }

            public bool MoveNext()
            {
                return _enumerator.MoveNext();
            }

            void IEnumerator.Reset()
            {
                ((IEnumerator)_enumerator).Reset();
            }
        }

        private Dictionary<TKey, TValue> GetReadOnlyDict()
        {
            var readOnly = _dictReadOnly;
            if (readOnly == null)
            {
                lock (_lockObject)
                {
                    readOnly = _dictReadOnly = _dict;
                }
            }
            return readOnly;
        }

        private IDictionary<TKey, TValue> GetWritableDict(bool clearDictionary = false)
        {
            if (_dictReadOnly == null)
            {
                // Never exposed the dictionary using enumerator, so immutable is not required
                if (clearDictionary)
                    _dict.Clear();
                return _dict;
            }

            var newDict = new Dictionary<TKey, TValue>(clearDictionary ? 0 : _dict.Count + 1, _dict.Comparer);
            if (!clearDictionary)
            {
                // Less allocation with enumerator than Dictionary-constructor
                foreach (var item in _dict)
                    newDict[item.Key] = item.Value;
            }
            _dict = newDict;
            _dictReadOnly = null;
            return newDict;
        }
    }
}
