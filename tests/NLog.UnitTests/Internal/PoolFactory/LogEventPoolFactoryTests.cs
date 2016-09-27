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
using System.Collections.Generic;
using NLog.Internal.PoolFactory;
using Xunit;

namespace NLog.UnitTests.Internal.PoolFactory
{
    public class LogEventPoolFactoryTests : NLogTestBase
    {
        private void TestPool<T>(ILogEventObjectFactory pool, Func<ILogEventObjectFactory, T> createObject, Action<ILogEventObjectFactory, T> releaseObject, int objectCount) where T : class
        {
            if (objectCount > 1)
                TestPool(pool, createObject, releaseObject, objectCount / 2);

            List<T> createdItems = new List<T>(objectCount);
            for (int i = 0; i < objectCount; ++i)
            {
                T item = createObject(pool);
                for (int j = 0; j < i; ++j)
                    Assert.False(ReferenceEquals(createdItems[j], item));
                createdItems.Add(item);
            }

            for (int i = 0; i < objectCount; ++i)
            {
                releaseObject(pool, createdItems[i]);
            }
        }

        [Fact]
        public void TestLogEventInfoPool()
        {
            LogEventPoolFactory pool = new LogEventPoolFactory("Test", NLog.Common.PoolSetup.Active, false, 0);
            TestPool(pool, (p) => p.CreateLogEvent(LogLevel.Off, "Test", null, "Test", null, null), (p, i) => pool.ReleaseLogEvent(i), 234);
            var poolStats = pool.GetPoolsStats();
            Assert.Equal(1, poolStats.Count);
            Assert.Equal(234, poolStats[0].Count);

            LogEventPoolFactory poolLarge = new LogEventPoolFactory("Test", NLog.Common.PoolSetup.Large, false, 0);
            TestPool(poolLarge, (p) => p.CreateLogEvent(LogLevel.Off, "Test", null, "Test", null, null), (p, i) => poolLarge.ReleaseLogEvent(i), 234);
            var poolLargeStats = poolLarge.GetPoolsStats();
            Assert.Equal(1, poolLargeStats.Count);
            Assert.Equal(234, poolLargeStats[0].Count);
            Assert.True(poolStats[0].MaxCapacity < poolLargeStats[0].MaxCapacity);
        }

        [Fact]
        public void TestSingleCallPool()
        {
            LogEventPoolFactory pool = new LogEventPoolFactory("Test", NLog.Common.PoolSetup.Active, false, 0);
            TestPool(pool, (p) => p.CreateSingleCallContinuation(null), (p, i) => p.ReleaseSingleCallContinuation(i), 234);
            var poolStats = pool.GetPoolsStats();
            Assert.Equal(1, poolStats.Count);
            Assert.Equal(234, poolStats[0].Count);

            LogEventPoolFactory poolLarge = new LogEventPoolFactory("Test", NLog.Common.PoolSetup.Large, false, 0);
            TestPool(poolLarge, (p) => p.CreateSingleCallContinuation(null), (p, i) => p.ReleaseSingleCallContinuation(i), 234);
            var poolLargeStats = poolLarge.GetPoolsStats();
            Assert.Equal(1, poolLargeStats.Count);
            Assert.Equal(234, poolLargeStats[0].Count);
            Assert.True(poolStats[0].MaxCapacity < poolLargeStats[0].MaxCapacity);
        }

        [Fact]
        public void TestExceptionHandlerPool()
        {
            LogEventPoolFactory pool = new LogEventPoolFactory("Test", NLog.Common.PoolSetup.Active, false, 0);
            TestPool(pool, (p) => p.CreateExceptionHandlerContinuation(0, false), (p, i) => p.ReleaseExceptionHandlerContinuation(i), 234);
            var poolStats = pool.GetPoolsStats();
            Assert.Equal(1, poolStats.Count);
            Assert.Equal(234, poolStats[0].Count);

            LogEventPoolFactory poolLarge = new LogEventPoolFactory("Test", NLog.Common.PoolSetup.Large, false, 0);
            TestPool(poolLarge, (p) => p.CreateExceptionHandlerContinuation(0, false), (p, i) => p.ReleaseExceptionHandlerContinuation(i), 234);
            var poolLargeStats = poolLarge.GetPoolsStats();
            Assert.Equal(1, poolLargeStats.Count);
            Assert.Equal(234, poolLargeStats[0].Count);
            Assert.True(poolStats[0].MaxCapacity < poolLargeStats[0].MaxCapacity);
        }

        [Fact]
        public void TestCompleteWhenAllPool()
        {
            LogEventPoolFactory pool = new LogEventPoolFactory("Test", NLog.Common.PoolSetup.Active, false, 0);
            TestPool(pool, (p) => p.CreateCompleteWhenAllContinuation(), (p, i) => p.ReleaseCompleteWhenAllContinuation(i), 234);
            var poolStats = pool.GetPoolsStats();
            Assert.Equal(1, poolStats.Count);
            Assert.Equal(234, poolStats[0].Count);

            LogEventPoolFactory poolLarge = new LogEventPoolFactory("Test", NLog.Common.PoolSetup.Large, false, 0);
            TestPool(poolLarge, (p) => p.CreateCompleteWhenAllContinuation(), (p, i) => p.ReleaseCompleteWhenAllContinuation(i), 234);
            var poolLargeStats = poolLarge.GetPoolsStats();
            Assert.Equal(1, poolLargeStats.Count);
            Assert.Equal(234, poolLargeStats[0].Count);
            Assert.True(poolStats[0].MaxCapacity < poolLargeStats[0].MaxCapacity);
        }

        [Fact]
        public void TestStringBuilderPool()
        {
            LogEventPoolFactory pool = new LogEventPoolFactory("Test", NLog.Common.PoolSetup.Active, false, 0);
            TestPool(pool, (p) => p.CreateStringBuilder(), (p, i) => p.ReleaseStringBuilder(i), 234);
            var poolStats = pool.GetPoolsStats();
            Assert.Equal(2, poolStats.Count);
            Assert.Equal(234, poolStats[0].Count);
            Assert.Equal(0, poolStats[1].Count);    // Attempts to reuse big-streams

            TestPool(pool, (p) => p.CreateStringBuilder(), (p, i) => p.ReleaseStringBuilder(i), 234);
            TestPool(pool, (p) => p.CreateStringBuilder(100000), (p, i) => p.ReleaseStringBuilder(i), 234);
            poolStats = pool.GetPoolsStats();
            Assert.Equal(2, poolStats.Count);
            Assert.Equal(234, poolStats[0].Count);
            Assert.Equal(234, poolStats[1].Count);

            LogEventPoolFactory poolLarge = new LogEventPoolFactory("Test", NLog.Common.PoolSetup.Large, false, 0);
            TestPool(poolLarge, (p) => p.CreateStringBuilder(), (p, i) => p.ReleaseStringBuilder(i), 234);
            var poolLargeStats = poolLarge.GetPoolsStats();
            Assert.Equal(2, poolLargeStats.Count);
            Assert.Equal(234, poolLargeStats[0].Count);
            Assert.Equal(0, poolLargeStats[1].Count);    // Attempts to reuse big-streams
            Assert.True(poolStats[0].MaxCapacity < poolLargeStats[0].MaxCapacity);

            TestPool(poolLarge, (p) => p.CreateStringBuilder(), (p, i) => p.ReleaseStringBuilder(i), 234);
            TestPool(poolLarge, (p) => p.CreateStringBuilder(100000), (p, i) => p.ReleaseStringBuilder(i), 234);
            poolLargeStats = poolLarge.GetPoolsStats();
            Assert.Equal(2, poolLargeStats.Count);
            Assert.Equal(234, poolLargeStats[0].Count);
            Assert.Equal(234, poolLargeStats[1].Count);
            Assert.True(poolStats[1].MaxCapacity < poolLargeStats[1].MaxCapacity);
            Assert.True(poolStats[1].MaxCapacity < poolLargeStats[1].MaxCapacity);
        }

        [Fact]
        public void TestMemoryStreamPool()
        {
            LogEventPoolFactory pool = new LogEventPoolFactory("Test", NLog.Common.PoolSetup.Active, false, 0);
            TestPool(pool, (p) => p.CreateMemoryStream(), (p, i) => p.ReleaseMemoryStream(i), 234);
            var poolStats = pool.GetPoolsStats();
            Assert.Equal(2, poolStats.Count);
            Assert.Equal(234, poolStats[0].Count);
            Assert.Equal(0, poolStats[1].Count);    // Attempts to reuse big-streams

            TestPool(pool, (p) => p.CreateMemoryStream(), (p, i) => p.ReleaseMemoryStream(i), 234);
            TestPool(pool, (p) => p.CreateMemoryStream(100000), (p, i) => p.ReleaseMemoryStream(i), 234);
            poolStats = pool.GetPoolsStats();
            Assert.Equal(2, poolStats.Count);
            Assert.Equal(234, poolStats[0].Count);
            Assert.Equal(234, poolStats[1].Count);

            LogEventPoolFactory poolLarge = new LogEventPoolFactory("Test", NLog.Common.PoolSetup.Large, false, 0);
            TestPool(poolLarge, (p) => p.CreateMemoryStream(), (p, i) => p.ReleaseMemoryStream(i), 234);
            var poolLargeStats = poolLarge.GetPoolsStats();
            Assert.Equal(2, poolLargeStats.Count);
            Assert.Equal(234, poolLargeStats[0].Count);
            Assert.Equal(0, poolLargeStats[1].Count);    // Attempts to reuse big-streams

            TestPool(poolLarge, (p) => p.CreateMemoryStream(), (p, i) => p.ReleaseMemoryStream(i), 234);
            TestPool(poolLarge, (p) => p.CreateMemoryStream(100000), (p, i) => p.ReleaseMemoryStream(i), 234);
            poolLargeStats = poolLarge.GetPoolsStats();
            Assert.Equal(2, poolLargeStats.Count);
            Assert.Equal(234, poolLargeStats[0].Count);
            Assert.Equal(234, poolLargeStats[1].Count);
        }

        [Fact]
        public void TestLogEventArrayPool()
        {
            LogEventPoolFactory pool = new LogEventPoolFactory("Test", NLog.Common.PoolSetup.Active, false, 0);
            TestPool(pool, (p) => p.CreateAsyncLogEventArray(), (p, i) => p.ReleaseAsyncLogEventArray(i), 234);
            var poolStats = pool.GetPoolsStats();
            Assert.Equal(1, poolStats.Count);
            Assert.Equal(234, poolStats[0].Count);

            TestPool(pool, (p) => p.CreateAsyncLogEventArray(), (p, i) => p.ReleaseAsyncLogEventArray(i), 234);
            TestPool(pool, (p) => p.CreateAsyncLogEventArray(100000), (p, i) => p.ReleaseAsyncLogEventArray(i), 234);
            poolStats = pool.GetPoolsStats();
            Assert.Equal(2, poolStats.Count);
            Assert.Equal(234, poolStats[0].Count);
            Assert.Equal(234, poolStats[1].Count);

            LogEventPoolFactory poolLarge = new LogEventPoolFactory("Test", NLog.Common.PoolSetup.Large, false, 0);
            TestPool(poolLarge, (p) => p.CreateAsyncLogEventArray(), (p, i) => p.ReleaseAsyncLogEventArray(i), 234);
            var poolLargeStats = poolLarge.GetPoolsStats();
            Assert.Equal(1, poolLargeStats.Count);
            Assert.Equal(234, poolLargeStats[0].Count);

            TestPool(poolLarge, (p) => p.CreateAsyncLogEventArray(), (p, i) => p.ReleaseAsyncLogEventArray(i), 234);
            TestPool(poolLarge, (p) => p.CreateAsyncLogEventArray(100000), (p, i) => p.ReleaseAsyncLogEventArray(i), 234);
            poolLargeStats = poolLarge.GetPoolsStats();
            Assert.Equal(2, poolLargeStats.Count);
            Assert.Equal(234, poolLargeStats[0].Count);
            Assert.Equal(234, poolLargeStats[1].Count);
        }
    }
}
