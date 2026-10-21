//
// Copyright (c) 2004-2024 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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

namespace NLog.UnitTests.Targets.Dispatchers
{
    using System;
    using System.Threading;
    using NLog.Targets.Dispatchers;
    using Xunit;

    /// <summary>
    /// Contract tests that every <see cref="IDispatcher"/> implementation must pass.
    /// Assertions use generous timeouts and "at least" counts.
    /// </summary>
    public abstract class DispatcherTestsBase
    {
        protected static readonly TimeSpan WaitTimeout = TimeSpan.FromSeconds(5);

        internal abstract IDispatcher CreateDispatcher();

        [Fact]
        public void Change_ZeroDueTime_InvokesCallbackOnce()
        {
            var dispatcher = CreateDispatcher();
            using (var called = new ManualResetEventSlim(false))
            {
                int callCount = 0;
                dispatcher.Set(() => { Interlocked.Increment(ref callCount); called.Set(); });
                try
                {
                    Assert.True(dispatcher.Change(0, Timeout.Infinite));
                    Assert.True(called.Wait(WaitTimeout));
                    Thread.Sleep(200);
                    Assert.Equal(1, Interlocked.CompareExchange(ref callCount, 0, 0));
                }
                finally
                {
                    dispatcher.Stop(WaitTimeout);
                }
            }
        }

        [Fact]
        public void Change_WithDueTime_DelaysInvocation()
        {
            var dispatcher = CreateDispatcher();
            using (var called = new ManualResetEventSlim(false))
            {
                dispatcher.Set(() => called.Set());
                try
                {
                    Assert.True(dispatcher.Change(500, Timeout.Infinite));
                    Assert.False(called.Wait(50));
                    Assert.True(called.Wait(WaitTimeout));
                }
                finally
                {
                    dispatcher.Stop(WaitTimeout);
                }
            }
        }

        [Fact]
        public void Change_WithPeriod_InvokesRepeatedly()
        {
            var dispatcher = CreateDispatcher();
            using (var calledThreeTimes = new ManualResetEventSlim(false))
            {
                int callCount = 0;
                dispatcher.Set(() =>
                {
                    if (Interlocked.Increment(ref callCount) >= 3)
                        calledThreeTimes.Set();
                });
                try
                {
                    Assert.True(dispatcher.Change(0, 20));
                    Assert.True(calledThreeTimes.Wait(WaitTimeout));
                }
                finally
                {
                    dispatcher.Stop(WaitTimeout);
                }
            }
        }

        [Fact]
        public void Change_NegativeDueTime_Disarms()
        {
            var dispatcher = CreateDispatcher();
            using (var called = new ManualResetEventSlim(false))
            {
                dispatcher.Set(() => called.Set());
                try
                {
                    Assert.True(dispatcher.Change(200, Timeout.Infinite));
                    Assert.True(dispatcher.Change(Timeout.Infinite, Timeout.Infinite));
                    Assert.False(called.Wait(500));
                }
                finally
                {
                    dispatcher.Stop(WaitTimeout);
                }
            }
        }

        [Fact]
        public void Change_FromInsideCallback_ReschedulesCallback()
        {
            var dispatcher = CreateDispatcher();
            using (var calledTwice = new ManualResetEventSlim(false))
            {
                int callCount = 0;
                dispatcher.Set(() =>
                {
                    int count = Interlocked.Increment(ref callCount);
                    if (count == 1)
                        dispatcher.Change(0, Timeout.Infinite);
                    else
                        calledTwice.Set();
                });
                try
                {
                    Assert.True(dispatcher.Change(0, Timeout.Infinite));
                    Assert.True(calledTwice.Wait(WaitTimeout));
                }
                finally
                {
                    dispatcher.Stop(WaitTimeout);
                }
            }
        }

        [Fact]
        public void Change_BeforeSet_Throws()
        {
            var dispatcher = CreateDispatcher();
            Assert.Throws<InvalidOperationException>(() => dispatcher.Change(0, Timeout.Infinite));
        }

        [Fact]
        public void Set_Twice_Throws()
        {
            var dispatcher = CreateDispatcher();
            dispatcher.Set(() => { });
            try
            {
                Assert.Throws<InvalidOperationException>(() => dispatcher.Set(() => { }));
            }
            finally
            {
                dispatcher.Stop(WaitTimeout);
            }
        }

        [Fact]
        public void Change_AfterStop_ReturnsFalse()
        {
            var dispatcher = CreateDispatcher();
            dispatcher.Set(() => { });
            Assert.True(dispatcher.Stop(WaitTimeout));
            Assert.False(dispatcher.Change(0, Timeout.Infinite));
        }

        [Fact]
        public void Stop_CalledTwice_ReturnsTrue()
        {
            var dispatcher = CreateDispatcher();
            dispatcher.Set(() => { });
            Assert.True(dispatcher.Stop(WaitTimeout));
            Assert.True(dispatcher.Stop(WaitTimeout));
        }

        [Fact]
        public void Stop_AfterStop_CallbackNotInvoked()
        {
            var dispatcher = CreateDispatcher();
            using (var called = new ManualResetEventSlim(false))
            {
                dispatcher.Set(() => called.Set());
                Assert.True(dispatcher.Change(300, Timeout.Infinite));
                Assert.True(dispatcher.Stop(WaitTimeout));
                Assert.False(called.Wait(600));
            }
        }

        [Fact]
        public void Stop_ConcurrentWithChange_DoesNotThrow()
        {
            var dispatcher = CreateDispatcher();
            dispatcher.Set(() => { });
            Exception failure = null;
            var producers = new Thread[4];
            using (var go = new ManualResetEvent(false))
            {
                for (int i = 0; i < producers.Length; ++i)
                {
                    producers[i] = new Thread(() =>
                    {
                        go.WaitOne();
                        try
                        {
                            for (int j = 0; j < 2000; ++j)
                                dispatcher.Change(0, Timeout.Infinite);
                        }
                        catch (Exception ex)
                        {
                            failure = ex;
                        }
                    });
                    producers[i].Start();
                }

                go.Set();
                Thread.Sleep(5);
                Assert.True(dispatcher.Stop(WaitTimeout));
                foreach (var producer in producers)
                    Assert.True(producer.Join(WaitTimeout));
            }

            Assert.Null(failure);
            Assert.False(dispatcher.Change(0, Timeout.Infinite));
        }
    }
}
