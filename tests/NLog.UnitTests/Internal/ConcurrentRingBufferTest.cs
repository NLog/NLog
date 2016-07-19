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


#if!SILVERLIGHT

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using System.Text;
using System.Threading;
using System.Threading.Tasks;

using NLog.Internal.Pooling;

using Xunit;
using Xunit.Extensions;



namespace NLog.UnitTests.Internal
{
    public class ConcurrentRingBufferTest
    {
        [Fact]
        public void TestEnumeration()
        {
            int size = 4;
            ConcurrentRingBufferImpl<Data> list = new ConcurrentRingBufferImpl<Data>(size);
            list.Initialize(() => new Data(), true);

            int count = 0;
            foreach (var item in list)
            {
                count += 1;
                Console.WriteLine(item.Hash);
            }
            Assert.Equal(size, count);
        }

        [Theory]
        [InlineData(5)]
        [InlineData(57)]
        [InlineData(501)]
        [InlineData(5032)]
        public void ShouldNotBeAbleToAddMoreToAFullBuffer(int size)
        {
            var evt = new ManualResetEvent(false);
            Thread t = new Thread(() =>
                {
                    var buffer = new ConcurrentRingBufferImpl<Data>(size);
                    buffer.Initialize(() => new Data(), false);

                    for (int x = 0; x < size; x++)
                    {
                        buffer.Push(new Data());
                    }

                    buffer.Push(new Data());
                });

            t.Start();

            if (evt.WaitOne(100))
            {
                Assert.False(true, "Thread managed to push more than there is room for");
            }

        }

        [Theory]
        [InlineData(5)]
        [InlineData(57)]
        public void ShouldNotBeAbleToPopFromAnEmptyBuffer(int size)
        {
            var evt = new ManualResetEvent(false);
            Thread t = new Thread(() =>
            {
                var buffer = new ConcurrentRingBufferImpl<Data>(size);
                buffer.Initialize(() => new Data(), false);
                var data = buffer.Pop();
                Assert.NotNull(data);
            });

            t.Start();

            if (evt.WaitOne(100))
            {
                Assert.False(true, "Thread managed to pop from an empty buffer");
            }
        }

        [Theory]
        [InlineData(5)]
        [InlineData(57)]
        [InlineData(501)]
        [InlineData(5032)]
        public void ShouldBeAbleToEmptyBufferCompletely(int size)
        {
            var buffer = new ConcurrentRingBufferImpl<Data>(size);
            buffer.Initialize(() => new Data(), false);

            List<Data> list = new List<Data>();

            for (int x = 0; x < size; x++)
            {
                var data = new Data();

                list.Add(data);
                buffer.Push(data);
            }

            for (int x = 0; x < size; x++)
            {
                Assert.True(list.Contains(buffer.Pop()));
            }
        }

        public static IEnumerable<object[]> SingleThreadAccessTestData
        {
            get
            {
                var bools = new[] { true, false };
                var counts = new[] { 10, 100, 1000, 10000, 100000 };

                return from tf in bools
                       from count in counts
                       select new object[] { count, tf };
            }
        }



        [PropertyData("SingleThreadAccessTestData")]
        [Theory]
        public void SingleThreadAccessTest(int size, bool prefill)
        {
            ConcurrentRingBufferImpl<Data> pool = new ConcurrentRingBufferImpl<Data>(size);
            pool.Initialize(() => new Data(), prefill);

            var distintValues = pool.Data.Distinct(new DataComparer()).Count();
            Assert.Equal(pool.Data.Count, distintValues);
            List<Data> list = new List<Data>(size);
            for (int x = 0; x < size; x++)
            {
                var item = pool.Pop();
                item.Counter += 1;
                list.Add(item);
            }

            Assert.Equal(0, pool.Data.Count);

            for (int x = 0; x < size; x++)
            {
                pool.Push(list[x]);
            }
            foreach (var item in pool.Data)
            {
                item.tracked = false;
            }

            GC.Collect(2, GCCollectionMode.Forced);
            long totalUsage = 0;
            long totalCount = 0;
            foreach (var item in pool.Data)
            {
                if (item.Counter > 0)
                {
                    totalUsage += 1;
                }
                totalCount += item.Counter;
            }

            Console.WriteLine("Counter:" + totalCount);
            Console.WriteLine("Usage:" + totalUsage);
            Console.WriteLine("Thrown away:" + Data.destroyed);



            distintValues = pool.Data.Distinct(new DataComparer()).Count();
            Assert.True(pool.Data.Count <= distintValues);
            Assert.True(size <= pool.Data.Count);
        }

        [Fact(Skip ="For development purposes")]
        //[Fact]
        public void Test()
        {
            //MultiThreadedAccess(1280, false, 12, 1);
            SingleThreadAccessTest(10, true);

        }

        public static IEnumerable<object[]> MultiThreadAccessTestData
        {
            get
            {
                var bools = new[] { true, false };
                var counts = new[] { 16, 32, 128, 320, 448, 576 };
                var threads = new[] { 1, 2, 4, 8, 16 };
                var sleeps = new[] { 0, 1, 2 };

                return from tf in bools
                       from count in counts
                       from thread in threads
                       from sleep in sleeps
                       select new object[] { count, tf, thread, sleep };
            }
        }

        [Theory]
        [PropertyData("MultiThreadAccessTestData")]
        public void MultiThreadedAccess(int size, bool prefill, int threadCount, int sleep)
        {
            ConcurrentRingBufferImpl<Data> pool = new ConcurrentRingBufferImpl<Data>(size);
            pool.Initialize(() => new Data(), prefill);
            var distintValues = pool.Data.Distinct(new DataComparer()).Count();
            Assert.Equal(pool.Data.Count, distintValues);

            int localCount = Math.Max(1, size / threadCount);
            List<Thread> threads = new List<Thread>();
            ManualResetEvent startEvent = new ManualResetEvent(false);
            StringBuilder sb = new StringBuilder();

            Console.WriteLine("Testing with:{0} items per thread", localCount);
            for (int y = 0; y < threadCount; y++)
            {
                var thread = new Thread(() =>
                    {
                        var random = new Random(Thread.CurrentThread.ManagedThreadId);
                        try
                        {
                            startEvent.WaitOne();

                            Thread.Sleep(TimeSpan.FromTicks(random.Next(5)));
                            Stack<Data> list = new Stack<Data>(localCount);
                            for (int x = 0; x < localCount; x++)
                            {
                                var item = pool.Pop();
                                item.SetOwner();
                                Thread.Sleep(TimeSpan.FromMilliseconds(random.Next(2) * sleep));

                                item.Counter += 1;
                                list.Push(item);

                            }

                            // Assert that we got at least as many as we requested
                            Assert.Equal(localCount, list.Count);

                            for (int x = localCount - 1; x > -1; x--)
                            {
                                var item = list.Pop();
                                Thread.Sleep(TimeSpan.FromMilliseconds(random.Next(1)));
                                item.ReleaseOwner();
                                pool.Push(item);
                            }
                        }
                        catch (Exception e)
                        {
                            lock (sb)
                            {
                                sb.Append(e);
                            }
                        }
                    });

                threads.Add(thread);
            }
            threads.ForEach(t => t.Start());
            startEvent.Set();
            Parallel.ForEach(threads, t => t.Join(TimeSpan.FromSeconds(10)));

            if (sb.Length > 0)
            {
                Assert.True(false, sb.ToString());
            }
            foreach (var item in pool.Data)
            {
                item.tracked = false;
            }
            GC.Collect(2, GCCollectionMode.Forced);
            long totalUsage = 0;
            long totalCount = 0;
            foreach (var item in pool.Data)
            {
                if (item.Counter > 0)
                {
                    totalUsage += 1;
                }
                totalCount += item.Counter;
            }

            Console.WriteLine("Counter:" + totalCount);
            Console.WriteLine("Usage:" + totalUsage);
            Console.WriteLine("Thrown away:" + Data.destroyed);
            Console.WriteLine("Thrown away (pool):" + pool.ThrownAwayObjects);
            Console.WriteLine("Created (pool):" + pool.Created);



            distintValues = pool.Data.Distinct(new DataComparer()).Count();
            Assert.True(pool.Data.Count <= distintValues);

            if (prefill)
            {
                // We cant know how many objects are created when we dont prefill pools
                if (threadCount <= size)
                {
                    Assert.Equal(size, pool.ObjectsInPool);

                }
            }

        }

        private class DataComparer : IEqualityComparer<Data>
        {
            public bool Equals(Data x, Data y)
            {
                return ReferenceEquals(x, y);
            }

            public int GetHashCode(Data obj)
            {
                return obj.Hash;
            }
        }

        [Fact]
        public void TestSimplePopPush()
        {
            var pool1 = new ConcurrentRingBufferImpl<Data>(4);
            var pool2 = new ConcurrentRingBufferImpl<Data>(4);
            pool1.Initialize(() => new Data(), true);
            pool2.Initialize(() => new Data(), true);


            var quitEvent = new ManualResetEvent(false);


            var t1 = new Thread(() =>
                {
                    do
                    {
                        var item = pool1.Pop();
                        Thread.Sleep(TimeSpan.FromTicks(new Random(Thread.CurrentThread.ManagedThreadId).Next(5)));
                        pool2.Push(item);
                    }
                    while (!quitEvent.WaitOne(1, true));


                });

            var t2 = new Thread(() =>
            {
                do
                {
                    var item = pool2.Pop();
                    Thread.Sleep(TimeSpan.FromTicks(new Random(Thread.CurrentThread.ManagedThreadId).Next(5)));
                    pool1.Push(item);
                }
                while (!quitEvent.WaitOne(1, true));
            });
            t1.Start();
            t2.Start();
            Thread.Sleep(50);
            quitEvent.Set();
            t1.Join();
            t2.Join();
            GC.Collect(2, GCCollectionMode.Forced);
            Console.WriteLine("Created:{0}", Data.numberCreated);
            Console.WriteLine("Thrown away:{0}", Data.destroyed);


        }

        [Fact(Skip = "For development purposes only")]
        public void PerformanceComparison()
        {
            int count = 120000;
            int iterations = 10000;
            int threadCount = 2;

            var cPool = new ConcurrentRingBufferImpl<Data>(count);
            cPool.Initialize(() => new Data(), true);


            var stack = new Stack<Data>(count);

            for (int x = 0; x < count; x++)
            {
                stack.Push(new Data());
            }

            var wrapper = new StackWrapper<Data>(stack);

            Action action = () =>
                {
                    this.StackRunner(iterations, wrapper);
                    //this.StackRunner2(iterations, wrapper2);
                    //this.PoolRunner(iterations, cPool);

                };

            TimeSpan begin = TimeSpan.Zero;
            DateTime now = DateTime.Now;

            while (true)
            {
                begin += RunPerformanceTest(action, threadCount);
                GC.Collect(2, GCCollectionMode.Forced);

                if ((DateTime.Now - now) > TimeSpan.FromSeconds(10))
                {
                    break;
                }
            }


            Console.WriteLine((DateTime.Now - now));
            int totalCount = 0;
            int totalUsage = 0;
            foreach (var item in stack)
            {
                if (item.Counter > 0)
                {
                    totalUsage += 1;
                }
                totalCount += item.Counter;
            }

            foreach (var item in cPool.Data)
            {
                if (item.Counter > 0)
                {
                    totalUsage += 1;
                }
                totalCount += item.Counter;
            }
            Console.WriteLine("Counter:" + totalCount);
            Console.WriteLine("Usage:" + totalUsage);
            Console.WriteLine("Thrown away (pool):" + cPool.ThrownAwayObjects);
            Console.WriteLine("Created (pool):" + cPool.Created);
        }

        private TimeSpan RunPerformanceTest(Action action, int threadcount)
        {
            List<Thread> threads = new List<Thread>();
            ManualResetEvent evt = new ManualResetEvent(false);

            for (int x = 0; x < threadcount; x++)
            {
                Thread t = new Thread((o) =>
                {
                    evt.WaitOne();

                    try
                    {
                        action();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                });
                threads.Add(t);
            }
            threads.ForEach(t => t.Start());
            evt.Set();
            Stopwatch watch = Stopwatch.StartNew();
            threads.ForEach(t => t.Join());
            watch.Stop();
            return watch.Elapsed;
        }


        [DebuggerDisplay("Hash:{Hash}")]
        private class Data
        {
            public static long numberCreated;

            public static long destroyed;

            public bool tracked = true;

            public int owner;
            public int Hash
            {
                get
                {
                    return _hash;
                }
            }

            public void SetOwner()
            {
                if (Interlocked.CompareExchange(ref this.owner, Thread.CurrentThread.ManagedThreadId, 0) != 0)
                {
                    throw new InvalidOperationException("Cannot set owner on an object another thread owns");
                }
            }

            public void ReleaseOwner()
            {
                if (Interlocked.CompareExchange(ref this.owner, 0, Thread.CurrentThread.ManagedThreadId) != Thread.CurrentThread.ManagedThreadId)
                {
                    throw new InvalidOperationException("Cannot release owner on an object another thread owns");
                }
            }

            private int _hash;
            public Data()
            {
                unchecked
                {
                    _hash = (int)Interlocked.Increment(ref numberCreated);
                }
            }
            public int Counter;

            ~Data()
            {
                if (tracked)
                {
                    Interlocked.Increment(ref destroyed);
                }
            }
        }

        private void StackRunner(int iterations, StackWrapper<Data> stack)
        {
            for (int i = 0; i < iterations; i++)
            {
                Data data = stack.Pop();
                data.Counter += 1;
                stack.Push(data);
            }
        }

        private void StackRunner2(int iterations, StackWrapper2<Data> stack)
        {
            for (int i = 0; i < iterations; i++)
            {
                Data data = stack.Pop();
                data.Counter += 1;
                stack.Push(data);
            }
        }

        private void PoolRunner(int iterations, ConcurrentRingBufferImpl<Data> pool)
        {
            for (int i = 0; i < iterations; i++)
            {
                Data data = pool.Pop();
                Interlocked.Increment(ref data.Counter);
                pool.Push(data);
            }
        }

        public class StackWrapper<T>
        {
            private Stack<T> stack;
            public StackWrapper(Stack<T> stack)
            {
                this.stack = stack;
            }
            public T Pop()
            {
                lock (this.stack)
                {
                    return this.stack.Pop();
                }
            }

            public void Push(T item)
            {
                lock (this.stack)
                {
                    this.stack.Push(item);
                }
            }
        }

        public class StackWrapper2<T>
            where T : class, new()
        {
            private Stack<T> stack;

            private ReaderWriterLockSlim lockSlim = new ReaderWriterLockSlim();
            public StackWrapper2(Stack<T> stack)
            {
                this.stack = stack;
            }
            public T Pop()
            {
                lockSlim.EnterReadLock();
                try
                {
                    var val = this.stack.Pop();
                    if (val == null)
                    {
                        return new T();
                    }
                    return val;
                }
                finally
                {
                    lockSlim.ExitReadLock();
                }

            }

            public void Push(T item)
            {
                lockSlim.EnterWriteLock();
                try
                {
                    this.stack.Push(item);
                }
                finally
                {
                    lockSlim.ExitWriteLock();
                }

            }
        }


    }
}
#endif