using System;
using System.Collections.Generic;

using Wintellect.PowerCollections;

namespace NLogViewer
{
    public class FastSortedList<T> where T : class
    {
        private IComparer<T> _comparer;
        private BigList<T> _list;

        public FastSortedList(IComparer<T> comparer)
        {
            _comparer = comparer;
            _list = new BigList<T>();
        }

        public void Add(T item)
        {
            int pos = _list.BinarySearch(item, _comparer);
            int insertBefore = pos;
            if (insertBefore < 0)
                insertBefore = ~insertBefore;
            _list.Insert(insertBefore, item);
        }

        public void AddRange(IList<T> items)
        {
            for (int i = 0; i < items.Count; ++i)
                Add(items[i]);
        }

        public void Remove(T item)
        {
            int pos = _list.BinarySearch(item, _comparer);
            if (pos < 0)
                return;
            _list.RemoveAt(pos);
        }

        public T this[int pos]
        {
            get { return _list[pos]; }
        }

        public void Clear()
        {
            _list.Clear();
        }

        public int Count
        {
            get { return _list.Count; }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _list.GetEnumerator();
        }
    }
}
