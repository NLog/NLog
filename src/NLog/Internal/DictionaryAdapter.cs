// 
// Copyright (c) 2004-2009 Jaroslaw Kowalski <jaak@jkowalski.net>
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

using System;
using System.Collections;
using System.Collections.Generic;

namespace NLog.Internal
{
    internal class DictionaryAdapter<TKey,TValue> : IDictionary
    {
        private readonly IDictionary<TKey, TValue> implementation;

        public DictionaryAdapter(IDictionary<TKey, TValue> implementation)
        {
            this.implementation = implementation;
        }

        public void Add(object key, object value)
        {
            this.implementation.Add((TKey)key, (TValue)value);
        }

        public void Clear()
        {
            this.implementation.Clear();
        }

        public bool Contains(object key)
        {
            return this.implementation.ContainsKey((TKey)key);
        }

        public IDictionaryEnumerator GetEnumerator()
        {
            return new MyEnumerator(this.implementation.GetEnumerator());
        }

        public bool IsFixedSize
        {
            get { return false; }
        }

        public bool IsReadOnly
        {
            get { return this.implementation.IsReadOnly; }
        }

        public ICollection Keys
        {
            get { return new List<TKey>(this.implementation.Keys); }
        }

        public void Remove(object key)
        {
            this.implementation.Remove((TKey)key);
        }

        public ICollection Values
        {
            get { return new List<TValue>(this.implementation.Values); }
        }

        public object this[object key]
        {
            get { return this.implementation[(TKey)key]; }
            set { this.implementation[(TKey) key] = (TValue) value; } 
        }

        public void CopyTo(Array array, int index)
        {
            throw new NotImplementedException();
        }

        public int Count
        {
            get { return this.implementation.Count; }
        }

        public bool IsSynchronized
        {
            get { return false; }
        }

        public object SyncRoot
        {
            get { return this.implementation; }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        class MyEnumerator : IDictionaryEnumerator
        {
            private IEnumerator<KeyValuePair<TKey, TValue>> wrapped;

            public MyEnumerator(IEnumerator<KeyValuePair<TKey, TValue>> wrapped)
            {
                this.wrapped = wrapped;
            }

            public DictionaryEntry Entry
            {
                get { return new DictionaryEntry(this.wrapped.Current.Key, this.wrapped.Current.Value); }
            }

            public object Key
            {
                get { return this.wrapped.Current.Key; }
            }

            public object Value
            {
                get { return this.wrapped.Current.Value; }
            }

            public object Current
            {
                get { return this.Entry; }
            }

            public bool MoveNext()
            {
                return this.wrapped.MoveNext();
            }

            public void Reset()
            {
                this.wrapped.Reset();
            }
        }
    }
}
