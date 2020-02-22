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

namespace NLog.UnitTests.Internal
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    /// <summary>
    /// Tricky implementation of IDictionary where GetEnumerator does not return expected DictionaryEntry by default
    /// </summary>
    internal class TrickyTestDictionary : IDictionary<object, object>, IDictionary
    {
        readonly Dictionary<object, object> _inner = new Dictionary<object, object>();

        public object this[object key] { get => _inner[key]; set => _inner[key] = value; }

        public ICollection<object> Keys => _inner.Keys;

        public ICollection<object> Values => _inner.Values;

        public int Count => _inner.Count;

        public bool IsReadOnly => ((IDictionary)_inner).IsReadOnly;

        public bool IsFixedSize => ((IDictionary)_inner).IsFixedSize;

        public object SyncRoot => ((IDictionary)_inner).SyncRoot;

        public bool IsSynchronized => ((IDictionary)_inner).IsSynchronized;

        ICollection IDictionary.Keys => _inner.Keys;

        ICollection IDictionary.Values => _inner.Values;

        public void Add(object key, object value)
        {
            _inner.Add(key, value);
        }

        public void Add(KeyValuePair<object, object> item)
        {
            _inner.Add(item.Key, item.Value);
        }

        public void Clear()
        {
            _inner.Clear();
        }

        public bool Contains(KeyValuePair<object, object> item)
        {
            return ((IDictionary)_inner).Contains(item);
        }

        public bool Contains(object key)
        {
            return ((IDictionary)_inner).Contains(key);
        }

        public bool ContainsKey(object key)
        {
            return _inner.ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<object, object>[] array, int arrayIndex)
        {
            ((IDictionary)_inner).CopyTo(array, arrayIndex);
        }

        public void CopyTo(Array array, int index)
        {
            ((IDictionary)_inner).CopyTo(array, index);
        }

        public IEnumerator<KeyValuePair<object, object>> GetEnumerator()
        {
            return _inner.GetEnumerator();
        }

        public bool Remove(object key)
        {
            return _inner.Remove(key);
        }

        public bool Remove(KeyValuePair<object, object> item)
        {
            return _inner.Remove(item.Key);
        }

        public bool TryGetValue(object key, out object value)
        {
            return _inner.TryGetValue(key, out value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _inner.GetEnumerator();
        }

        IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            return _inner.GetEnumerator();  // Non-standard enumerator returned. Should be: ((IDictionary)_inner).GetEnumerator()
        }

        void IDictionary.Remove(object key)
        {
            _inner.Remove(key);
        }
    }
}
