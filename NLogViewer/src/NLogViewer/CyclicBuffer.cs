using System;

namespace NLogViewer
{
    public class CyclicBuffer<T>
    {
        private int _getPointer;
        private int _setPointer;
        private int _capacity;
        private T[] _items;
        private int _count;

        public CyclicBuffer(int maxSize)
        {
            _capacity = maxSize;
            _items = new T[maxSize];
            _getPointer = 0;
            _setPointer = 0;
        }

        public T AddAndRemoveLast(T item)
        {
            int pos = _setPointer;
            _setPointer = (_setPointer + 1) % _capacity;
            if (_count < _capacity)
            {
                _count++;
            }
            else
            {
                _getPointer = (_getPointer + 1) % _capacity;
            }
            T oldItem = _items[pos];
            _items[pos] = item;
            return oldItem;
        }

        public void RemoveLastIfEqual(T item)
        {
            if (Object.ReferenceEquals(_items[_getPointer % _items.Length], item))
                _getPointer++;
        }

        public T this[int pos]
        {
            get { return _items[(pos + _getPointer) % _items.Length]; }
        }

        public int Count
        {
            get { return _count; }
        }

        public int Capacity
        {
            get { return _capacity; }
        }

        public int GetPointer
        {
            get { return _getPointer; }
        }

        public int SetPointer
        {
            get { return _setPointer; }
        }

        public void Clear()
        {
            _getPointer = 0;
            _setPointer = 0;
            _count = 0;
        }

        public void CopyTo(CyclicBuffer<T> destination)
        {
            int toCopy = Math.Min(Count, destination.Capacity);
            int startPos = Count - 1 - toCopy;
            for (int i = startPos; i < Count; ++i)
            {
                destination.AddAndRemoveLast(this[i]);
            }
        }
    }
}
