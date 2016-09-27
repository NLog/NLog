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

using System;
using NLog.Common;
using NLog.Internal.PoolFactory;
using Xunit;

namespace NLog.UnitTests.Internal.PoolFactory
{
    public class AsyncLogEventArrayTests : NLogTestBase
    {
        [Fact]
        public void TestArrayEmpty()
        {
            ReusableAsyncLogEventInfoArray target = new ReusableAsyncLogEventInfoArray(0);
            Assert.Equal(0, target.Buffer.Length);
            Assert.Equal(0, new ArraySegment<AsyncLogEventInfo>(target.Buffer, 0, 0).Count);
            target.Clear();
        }

        [Fact]
        public void TestArrayOneItem()
        {
            ReusableAsyncLogEventInfoArray target = new ReusableAsyncLogEventInfoArray(1);
            Assert.Equal(1, target.Buffer.Length);
            target.Buffer[0] = new AsyncLogEventInfo(new LogEventInfo(), null);
            var arraySegment = new ArraySegment<AsyncLogEventInfo>(target.Buffer, 0, 1);
            Assert.Equal(1, arraySegment.Count);
            for (int i = arraySegment.Offset; i < arraySegment.Offset + arraySegment.Count; ++i)
                Assert.NotNull(arraySegment.Array[i].LogEvent);
            target.Clear();
            for (int i = arraySegment.Offset; i < arraySegment.Offset + arraySegment.Count; ++i)
                Assert.Null(arraySegment.Array[i].LogEvent);
        }

        [Fact]
        public void TestArrayOneItemCount()
        {
            ReusableAsyncLogEventInfoArray target = new ReusableAsyncLogEventInfoArray(2);
            Assert.Equal(2, target.Buffer.Length);
            target.Buffer[0] = new AsyncLogEventInfo(new LogEventInfo(), null);
            var arraySegment = new ArraySegment<AsyncLogEventInfo>(target.Buffer, 0, 1);
            Assert.Equal(1, arraySegment.Count);
            for (int i = arraySegment.Offset; i < arraySegment.Offset + arraySegment.Count; ++i)
                Assert.NotNull(arraySegment.Array[i].LogEvent);
            target.Clear();
            for (int i = arraySegment.Offset; i < arraySegment.Offset + arraySegment.Count; ++i)
                Assert.Null(arraySegment.Array[i].LogEvent);
        }

        [Fact]
        public void TestArrayTwoItems()
        {
            ReusableAsyncLogEventInfoArray target = new ReusableAsyncLogEventInfoArray(2);
            Assert.Equal(2, target.Buffer.Length);
            target.Buffer[0] = new AsyncLogEventInfo(new LogEventInfo(), null);
            target.Buffer[1] = new AsyncLogEventInfo(new LogEventInfo(), null);
            var arraySegment = new ArraySegment<AsyncLogEventInfo>(target.Buffer, 0, 2);
            Assert.Equal(2, arraySegment.Count);
            for (int i = arraySegment.Offset; i < arraySegment.Offset + arraySegment.Count; ++i)
                Assert.NotNull(arraySegment.Array[i].LogEvent);
            target.Clear();
            for (int i = arraySegment.Offset; i < arraySegment.Offset + arraySegment.Count; ++i)
                Assert.Null(arraySegment.Array[i].LogEvent);
        }
    }
}
