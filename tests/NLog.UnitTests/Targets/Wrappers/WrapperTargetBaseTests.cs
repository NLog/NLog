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

namespace NLog.UnitTests.Targets.Wrappers
{
    using System;
    using NLog.Common;
    using NLog.Targets;
    using NLog.Targets.Wrappers;
    using Xunit;

    public class WrapperTargetBaseTests : NLogTestBase
    {
        [Fact]
        public void WrapperTargetNestedToStringTest()
        {
            var wrapper = new MyWrapper
            {
                WrappedTarget = new DebugTarget() { Name = "foo" },
            };

            var wrapper2 = new MyWrapper()
            {
                WrappedTarget = wrapper,
            };

            Assert.Equal("MyWrapper_MyWrapper_DebugTarget(Name=foo)", wrapper2.ToString());
        }

        [Fact]
        public void WrapperTargetToStringTest()
        {
            var wrapper = new MyWrapper
            {
                Name = "foo",
                WrappedTarget = new DebugTarget() { Name = "foo_wrapped" },
            };

            Assert.Equal("MyWrapper_DebugTarget(Name=foo)", wrapper.ToString());
        }

        [Fact]
        public void WrapperTargetFlushTest()
        {
            var wrapped = new MyWrappedTarget();

            var wrapper = new MyWrapper
            {
                WrappedTarget = wrapped,
            };

            wrapper.Initialize(null);
            wrapped.Initialize(null);

            wrapper.Flush(ex => { });
            Assert.Equal(1, wrapped.FlushCount);
        }

        [Fact]
        public void WrapperTargetDefaultWriteTest()
        {
            using (new NoThrowNLogExceptions())
            {
                Exception lastException = null;
                var wrapper = new MyWrapper();
                wrapper.WrappedTarget = new MyWrappedTarget();
                wrapper.Initialize(null);
                wrapper.WriteAsyncLogEvent(LogEventInfo.CreateNullEvent().WithContinuation(ex => lastException = ex));
                Assert.NotNull(lastException);
                Assert.IsType<NotSupportedException>(lastException);
            }
        }

        public class MyWrapper : WrapperTargetBase
        {
            public MyWrapper() : base()
            {
            }

            public MyWrapper(string name) : this()
            {
                Name = name;
            }
        }

        public class MyWrappedTarget : Target
        {
            public MyWrappedTarget() : base()
            {
            }

            public MyWrappedTarget(string name) : this()
            {
                Name = name;
            }

            public int FlushCount { get; set; }

            protected override void FlushAsync(AsyncContinuation asyncContinuation)
            {
                FlushCount++;
                base.FlushAsync(asyncContinuation);
            }

            protected override void Write(LogEventInfo logEvent)
            {
                throw new NotImplementedException();
            }
        }
    }
}
