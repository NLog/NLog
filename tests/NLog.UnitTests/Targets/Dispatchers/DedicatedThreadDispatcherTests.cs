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

    public class DedicatedThreadDispatcherTests : DispatcherTestsBase
    {
        private const string ThreadName = "NLog DedicatedThreadDispatcherTests";

        internal override IDispatcher CreateDispatcher()
        {
            return new DedicatedThreadDispatcher(ThreadName);
        }

        [Fact]
        public void Callback_RunsOnNamedBackgroundNonThreadPoolThread()
        {
            var dispatcher = CreateDispatcher();
            Thread callbackThread = null;
            using (var called = new ManualResetEventSlim(false))
            {
                dispatcher.Set(() => { callbackThread = Thread.CurrentThread; called.Set(); });
                try
                {
                    dispatcher.Change(0, Timeout.Infinite);
                    Assert.True(called.Wait(WaitTimeout));
                    Assert.False(callbackThread.IsThreadPoolThread);
                    Assert.True(callbackThread.IsBackground);
                    Assert.Equal(ThreadName, callbackThread.Name);
                }
                finally
                {
                    dispatcher.Stop(WaitTimeout);
                }
            }
        }

        [Fact]
        public void Callback_Throws_LoopContinues()
        {
            var dispatcher = CreateDispatcher();
            int callCount = 0;
            using (var calledAgain = new ManualResetEventSlim(false))
            using (var firstCallDone = new ManualResetEventSlim(false))
            {
                dispatcher.Set(() =>
                {
                    if (Interlocked.Increment(ref callCount) == 1)
                    {
                        firstCallDone.Set();
                        throw new ApplicationException("callback failure");
                    }
                    calledAgain.Set();
                });
                try
                {
                    dispatcher.Change(0, Timeout.Infinite);
                    Assert.True(firstCallDone.Wait(WaitTimeout));
                    Thread.Sleep(50);
                    Assert.True(dispatcher.Change(0, Timeout.Infinite));
                    Assert.True(calledAgain.Wait(WaitTimeout));
                }
                finally
                {
                    dispatcher.Stop(WaitTimeout);
                }
            }
        }

        [Fact]
        public void Stop_FromInsideCallback_DoesNotDeadlock()
        {
            var dispatcher = CreateDispatcher();
            bool stopResult = false;
            using (var stopped = new ManualResetEventSlim(false))
            {
                dispatcher.Set(() => { stopResult = dispatcher.Stop(WaitTimeout); stopped.Set(); });
                dispatcher.Change(0, Timeout.Infinite);
                Assert.True(stopped.Wait(WaitTimeout));
                Assert.True(stopResult);
                Assert.False(dispatcher.Change(0, Timeout.Infinite));
            }
        }

        [Fact]
        public void Stop_JoinsThread()
        {
            var dispatcher = CreateDispatcher();
            Thread callbackThread = null;
            using (var called = new ManualResetEventSlim(false))
            {
                dispatcher.Set(() => { callbackThread = Thread.CurrentThread; called.Set(); });
                dispatcher.Change(0, Timeout.Infinite);
                Assert.True(called.Wait(WaitTimeout));
                Assert.True(dispatcher.Stop(WaitTimeout));
                Assert.False(callbackThread.IsAlive);
            }
        }

        [Fact]
        public void Stop_WhileCallbackRunning_ReturnsFalseThenThreadExits()
        {
            var dispatcher = CreateDispatcher();
            Thread callbackThread = null;
            using (var entered = new ManualResetEventSlim(false))
            using (var release = new ManualResetEventSlim(false))
            {
                dispatcher.Set(() => { callbackThread = Thread.CurrentThread; entered.Set(); release.Wait(WaitTimeout); });
                dispatcher.Change(0, Timeout.Infinite);
                Assert.True(entered.Wait(WaitTimeout));

                Assert.False(dispatcher.Stop(TimeSpan.FromMilliseconds(50)));

                release.Set();
                Assert.True(callbackThread.Join(WaitTimeout));
            }
        }

        private static readonly AsyncLocal<string> AmbientValue = new AsyncLocal<string>();

        [Fact]
        public void ExecutionContext_DoesNotFlowIntoDispatcherThread()
        {
            var dispatcher = CreateDispatcher();
            string observed = "not-called";
            using (var called = new ManualResetEventSlim(false))
            {
                AmbientValue.Value = "caller-value";
                try
                {
                    dispatcher.Set(() => { observed = AmbientValue.Value; called.Set(); });
                }
                finally
                {
                    AmbientValue.Value = null;
                }

                try
                {
                    dispatcher.Change(0, Timeout.Infinite);
                    Assert.True(called.Wait(WaitTimeout));
                    Assert.Null(observed);
                }
                finally
                {
                    dispatcher.Stop(WaitTimeout);
                }
            }
        }
    }
}
