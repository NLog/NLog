// 
// Copyright (c) 2004-2016 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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

namespace NLog.Common
{
    using System;
    using NLog.Common;

    /// <summary>
    /// A cyclic buffer of <see cref="LogEventInfo"/> object.
    /// </summary>
    public class LogEventInfoBuffer
    {
        private readonly bool growAsNeeded;
        private readonly int growLimit;

        private AsyncLogEventInfo[] buffer;
        private int getPointer;
        private int putPointer;
        private int count;

        /// <summary>
        /// Initializes a new instance of the <see cref="LogEventInfoBuffer" /> class.
        /// </summary>
        /// <param name="size">Buffer size.</param>
        /// <param name="growAsNeeded">Whether buffer should grow as it becomes full.</param>
        /// <param name="growLimit">The maximum number of items that the buffer can grow to.</param>
        public LogEventInfoBuffer(int size, bool growAsNeeded, int growLimit)
        {
            this.growAsNeeded = growAsNeeded;
            this.buffer = new AsyncLogEventInfo[size];
            this.growLimit = growLimit;
            this.getPointer = 0;
            this.putPointer = 0;
        }

        /// <summary>
        /// Gets the number of items in the array.
        /// </summary>
        public int Size
        {
            get { return this.buffer.Length; }
        }

        /// <summary>
        /// Adds the specified log event to the buffer.
        /// </summary>
        /// <param name="eventInfo">Log event.</param>
        /// <returns>The number of items in the buffer.</returns>
        public int Append(AsyncLogEventInfo eventInfo)
        {
            lock (this)
            {
                // make room for additional item
                if (this.count >= this.buffer.Length)
                {
                    if (this.growAsNeeded && this.buffer.Length < this.growLimit)
                    {
                        // create a new buffer, copy data from current
                        int newLength = this.buffer.Length * 2;
                        if (newLength >= this.growLimit)
                        {
                            newLength = this.growLimit;
                        }

                        // InternalLogger.Trace("Enlarging LogEventInfoBuffer from {0} to {1}", this.buffer.Length, this.buffer.Length * 2);
                        var newBuffer = new AsyncLogEventInfo[newLength];
                        Array.Copy(this.buffer, 0, newBuffer, 0, this.buffer.Length);
                        this.buffer = newBuffer;
                    }
                    else
                    {
                        // lose the oldest item
                        this.getPointer = this.getPointer + 1;
                    }
                }

                // put the item
                this.putPointer = this.putPointer % this.buffer.Length;
                this.buffer[this.putPointer] = eventInfo;
                this.putPointer = this.putPointer + 1;
                this.count++;
                if (this.count >= this.buffer.Length)
                {
                    this.count = this.buffer.Length;
                }

                return this.count;
            }
        }

        /// <summary>
        /// Gets the array of events accumulated in the buffer and clears the buffer as one atomic operation.
        /// </summary>
        /// <returns>Events in the buffer.</returns>
        public AsyncLogEventInfo[] GetEventsAndClear()
        {
            lock (this)
            {
                int cnt = this.count;
                var returnValue = new AsyncLogEventInfo[cnt];

                // InternalLogger.Trace("GetEventsAndClear({0},{1},{2})", this.getPointer, this.putPointer, this.count);
                for (int i = 0; i < cnt; ++i)
                {
                    int p = (this.getPointer + i) % this.buffer.Length;
                    var e = this.buffer[p];
                    this.buffer[p] = default(AsyncLogEventInfo); // we don't want memory leaks
                    returnValue[i] = e;
                }

                this.count = 0;
                this.getPointer = 0;
                this.putPointer = 0;

                return returnValue;
            }
        }
    }
}
