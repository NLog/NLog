// 
// Copyright (c) 2004-2006 Jaroslaw Kowalski <jaak@jkowalski.net>
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
using System.IO;
using System.Text;
using System.Xml;
using System.Reflection;
using System.Diagnostics;

using NLog.Internal;
using System.Net;
using System.Net.Sockets;

using NLog.Config;

namespace NLog.Internal
{
    /// <summary>
    /// A cyclic buffer of <see cref="LogEventInfo"/> object.
    /// </summary>
    public class LogEventInfoBuffer
    {
        /// <summary>
        /// Creates a new instance of <see cref="LogEventInfoBuffer"/>, sets the buffer size and grow options.
        /// </summary>
        /// <param name="size">Buffer size.</param>
        /// <param name="growAsNeeded">Whether buffer should grow as it becomes full.</param>
        /// <param name="growLimit">The maximum number of items that the buffer can grow to.</param>
        public LogEventInfoBuffer(int size, bool growAsNeeded, int growLimit)
        {
            _growAsNeeded = growAsNeeded;
            _buffer = new LogEventInfo[size];
            _growLimit = growLimit;
            _getPointer = 0;
            _putPointer = 0;
        }

        private LogEventInfo[] _buffer;
        private int _getPointer = 0;
        private int _putPointer = 0;
        private int _count = 0;
        private bool _growAsNeeded;
        private int _growLimit = 0;

        /// <summary>
        /// Adds the specified log event to the buffer.
        /// </summary>
        /// <param name="e">Log event</param>
        /// <returns>The number of items in the buffer.</returns>
        public int Append(LogEventInfo e)
        {
            lock (this)
            {
                // make room for additional item

                if (_count >= _buffer.Length)
                {
                    if (_growAsNeeded && _buffer.Length < _growLimit)
                    {
                        // create a new buffer, copy data from current

                        int newLength = _buffer.Length * 2;
                        if (newLength >= _growLimit)
                            newLength = _growLimit;

                        LogEventInfo[] newBuffer = new LogEventInfo[newLength];
                        // InternalLogger.Trace("Enlarging LogEventInfoBuffer from {0} to {1}", _buffer.Length, _buffer.Length * 2);
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
                _buffer[_putPointer] = e;
                _putPointer = _putPointer + 1;
                _count++;
                if (_count >= _buffer.Length)
                    _count = _buffer.Length;
                return _count;

/*
                InternalLogger.Trace("GetPointer: {0} PutPointer: {1} Count: {2} BufferLength: {3}", _getPointer, _putPointer, _count, _buffer.Length);
                for (int i = 0; i < _count; ++i)
                {
                    InternalLogger.Trace("buffer[{0}] = {1}", i, _buffer[(_getPointer + i) % _buffer.Length].Message);
                }
                
*/

                //Console.ReadLine();
            }
        }

        /// <summary>
        /// Gets the array of events accumulated in the buffer and clears the buffer as one atomic operation.
        /// </summary>
        /// <returns>An array of <see cref="LogEventInfo"/> objects that have been accumulated in the buffer.</returns>
        /// <remarks>
        /// In case there are no items in the buffer, the function returns an empty array.
        /// </remarks>
        public LogEventInfo[] GetEventsAndClear()
        {
            lock (this)
            {
                int cnt = _count;
                LogEventInfo[] returnValue = new LogEventInfo[cnt];
                // InternalLogger.Trace("GetEventsAndClear({0},{1},{2})", _getPointer, _putPointer, _count);
                for (int i = 0; i < cnt; ++i)
                {
                    int p = (_getPointer + i) % _buffer.Length;
                    LogEventInfo e = _buffer[p];
                    _buffer[p] = null; // we don't want memory leaks
                    returnValue[i] = e;
                }
                _count = 0;
                _getPointer = 0;
                _putPointer = 0;
                return returnValue;
            }
        }

        /// <summary>
        /// Gets the number of items in the array.
        /// </summary>
        public int Size
        {
            get { return _buffer.Length; }
        }
    }
}
