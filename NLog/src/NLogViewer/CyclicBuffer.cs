using System;

namespace NLogViewer
{
    public class CyclicBuffer
    {
        private int _getPointer;
        private int _setPointer;
        private int _maxSize;
        private object[] _items;
        private int _count;

        public CyclicBuffer(int maxSize)
        {
            _maxSize = maxSize;
            _items = new object[maxSize];
            _getPointer = 0;
            _setPointer = 0;
        }

        public object AddAndRemoveLast(object item)
        {
            int pos = _setPointer;
            _setPointer = (_setPointer + 1) % _maxSize;
            if (_count < _maxSize)
            {
                _count++;
            }
            else
            {
                _getPointer = (_getPointer + 1) % _maxSize;
            }
            object oldItem = _items[pos];
            _items[pos] = item;
            return oldItem;
        }

        public object this[int pos]
        {
            get { return _items[(pos + _getPointer) % _items.Length]; }
        }

        public int Count
        {
            get { return _count; }
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
    }
}
