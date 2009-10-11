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
