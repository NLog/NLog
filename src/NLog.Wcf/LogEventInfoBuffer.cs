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

namespace NLog.Wcf
{
    using NLog.Common;
    using System;

    /// <summary>
    /// A cyclic buffer of <see cref="LogEventInfo"/> object.
    /// </summary>
    internal class LogEventInfoBuffer
    {
        private readonly object _lockObject = new object();
        private readonly bool _growAsNeeded;
        private readonly int _growLimit;

        private AsyncLogEventInfo[] _buffer;
        private int _getPointer;
        private int _putPointer;
        private int _count;

        /// <summary>
        /// Initializes a new instance of the <see cref="LogEventInfoBuffer" /> class.
        /// </summary>
        /// <param name="size">Buffer size.</param>
        /// <param name="growAsNeeded">Whether buffer should grow as it becomes full.</param>
        /// <param name="growLimit">The maximum number of items that the buffer can grow to.</param>
        public LogEventInfoBuffer(int size, bool growAsNeeded, int growLimit)
        {
            _growAsNeeded = growAsNeeded;
            _buffer = new AsyncLogEventInfo[size];
            _growLimit = growLimit;
            _getPointer = 0;
            _putPointer = 0;
        }

        /// <summary>
        /// Gets the capacity of the buffer
        /// </summary>
        public int Size => _buffer.Length;

        /// <summary>
        /// Gets the number of items in the buffer
        /// </summary>
        internal int Count { get { lock (_lockObject) return _count; } }

        /// <summary>
        /// Adds the specified log event to the buffer.
        /// </summary>
        /// <param name="eventInfo">Log event.</param>
        /// <returns>The number of items in the buffer.</returns>
        public int Append(AsyncLogEventInfo eventInfo)
        {
            lock (_lockObject)
            {
                // make room for additional item
                if (_count >= _buffer.Length)
                {
                    if (_growAsNeeded && _buffer.Length < _growLimit)
                    {
                        // create a new buffer, copy data from current
                        int newLength = _buffer.Length * 2;
                        if (newLength >= _growLimit)
                        {
                            newLength = _growLimit;
                        }

                        InternalLogger.Trace("Enlarging LogEventInfoBuffer from {0} to {1}", _buffer.Length, newLength);
                        var newBuffer = new AsyncLogEventInfo[newLength];
                        Array.Copy(_buffer, 0, newBuffer, 0, _buffer.Length);
                        _buffer = newBuffer;
                    }
                    else
                    {
                        // lose the oldest item
                        _getPointer = _getPointer + 1;
                    }
                }

                // put the item
                _putPointer = _putPointer % _buffer.Length;
                _buffer[_putPointer] = eventInfo;
                _putPointer = _putPointer + 1;
                _count++;
                if (_count >= _buffer.Length)
                {
                    _count = _buffer.Length;
                }

                return _count;
            }
        }

        /// <summary>
        /// Gets the array of events accumulated in the buffer and clears the buffer as one atomic operation.
        /// </summary>
        /// <returns>Events in the buffer.</returns>
        public AsyncLogEventInfo[] GetEventsAndClear()
        {
            lock (_lockObject)
            {
                int cnt = _count;
                if (cnt == 0)
                    return new AsyncLogEventInfo[0];

                var returnValue = new AsyncLogEventInfo[cnt];

                for (int i = 0; i < cnt; ++i)
                {
                    int p = (_getPointer + i) % _buffer.Length;
                    var e = _buffer[p];
                    _buffer[p] = default(AsyncLogEventInfo); // we don't want memory leaks
                    returnValue[i] = e;
                }

                _count = 0;
                _getPointer = 0;
                _putPointer = 0;

                return returnValue;
            }
        }
    }
}
