using System;
using System.Collections.Generic;
using System.IO;

namespace NLog.Internal
{
    internal class BufferPool
    {
        private readonly Stack<byte[]> _bufferPool;
        internal readonly int MaxPoolCapacity;
        internal readonly int BufferCapacity;

        public BufferPool(int maxPoolCapacity, int bufferCapacity)
        {
            _bufferPool = new Stack<byte[]>(maxPoolCapacity);
            MaxPoolCapacity = maxPoolCapacity;
            BufferCapacity = bufferCapacity;
        }

        public BufferPoolStream CreateStream()
            => new BufferPoolStream(this);

        internal byte[] Rent()
        {
            if (_bufferPool.Count < 1)
                return new byte[BufferCapacity];

            return _bufferPool.Pop();
        }

        internal void Return(byte[] buffer)
        {
            if (buffer.Length == BufferCapacity && _bufferPool.Count < MaxPoolCapacity)
                _bufferPool.Push(buffer);
        }

        public void Dispose()
        {
            while (_bufferPool.Count > 0)
                _bufferPool.Pop();
        }
    }

    internal class BufferPoolStream : MemoryStream
    {
        private readonly BufferPool _bufferPool;
        private readonly List<byte[]> _buffers;
        private long _length;
        private long _position;

        public BufferPoolStream(BufferPool bufferPool)
        {
            _bufferPool = bufferPool;
            _buffers = new List<byte[]>(_bufferPool.BufferCapacity);
        }
        
        public override bool CanRead => true;
        public override bool CanSeek => true;
        public override bool CanWrite => true;
        public override long Length => _length;
        public override long Position
        {
            get => _position;
            set
            {
                if (value < 0 || value > _length) 
                    throw new ArgumentOutOfRangeException(nameof(value));
                
                _position = value;
            }
        }
        
        public override void Flush()
        {
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (buffer == null) throw new ArgumentNullException(nameof(buffer));
            if (offset < 0 || count < 0 || buffer.Length - offset < count) throw new ArgumentOutOfRangeException();

            if (_position >= _length) return 0;

            int bytesRead = 0;
            while (count > 0 && _position < _length)
            {
                int segmentIndex = (int)(_position / _bufferPool.BufferCapacity);
                int segmentOffset = (int)(_position % _bufferPool.BufferCapacity);

                int bytesToRead = Math.Min(count, _bufferPool.BufferCapacity - segmentOffset);
                bytesToRead = Math.Min(bytesToRead, (int)(_length - _position));

                Array.Copy(_buffers[segmentIndex], segmentOffset, buffer, offset, bytesToRead);

                offset += bytesToRead;
                count -= bytesToRead;
                _position += bytesToRead;
                bytesRead += bytesToRead;
            }

            return bytesRead;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            long newPosition;
            switch (origin)
            {
                case SeekOrigin.Begin:
                    newPosition = offset;
                    break;
                case SeekOrigin.Current:
                    newPosition = _position + offset;
                    break;
                case SeekOrigin.End:
                    newPosition = _length + offset;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(origin), origin, null);
            }

            if (newPosition < 0) throw new ArgumentOutOfRangeException(nameof(offset));
            
            _position = newPosition;
            return _position;
        }

        public override void SetLength(long value)
        {
            if (value < 0) throw new ArgumentOutOfRangeException(nameof(value));
            
            var requiredBuffersCount = (value + _bufferPool.BufferCapacity - 1) / _bufferPool.BufferCapacity;
                
            if (value > _length)
            {
                while (_buffers.Count < requiredBuffersCount)
                    _buffers.Add(_bufferPool.Rent());
            }
            else if (value < _length)
            {
                while (_buffers.Count > requiredBuffersCount)
                {
                    _bufferPool.Return(_buffers[_buffers.Count - 1]);
                    _buffers.RemoveAt(_buffers.Count - 1);
                }
            }

            _length = value;
            
            if (_position > _length)
                _position = _length;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (buffer == null) throw new ArgumentNullException(nameof(buffer));
            if (offset < 0 || count < 0 || buffer.Length - offset < count) throw new ArgumentOutOfRangeException();

            while (count > 0)
            {
                int segmentIndex = (int)(_position / _bufferPool.BufferCapacity);
                int segmentOffset = (int)(_position % _bufferPool.BufferCapacity);

                if (segmentIndex >= _buffers.Count)
                    _buffers.Add(_bufferPool.Rent());

                int bytesToWrite = Math.Min(count, _bufferPool.BufferCapacity - segmentOffset);
                Array.Copy(buffer, offset, _buffers[segmentIndex], segmentOffset, bytesToWrite);

                offset += bytesToWrite;
                count -= bytesToWrite;
                _position += bytesToWrite;

                if (_position > _length)
                    _length = _position;
            }
        }
    }
}