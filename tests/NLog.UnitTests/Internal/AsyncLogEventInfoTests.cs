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

using NLog.Common;
using Xunit;

namespace NLog.UnitTests.Internal
{
    public class AsyncLogEventInfoTests : NLogTestBase
    {
        [Fact]
        public void TestEquals()
        {
            var logEvent1 = new LogEventInfo(LogLevel.Debug, "logger1", "message1");
            AsyncContinuation cont1 = new AsyncContinuation(exception => { });
            var async1 = new AsyncLogEventInfo(logEvent1, cont1);
            var async2 = new AsyncLogEventInfo(logEvent1, cont1);
            Assert.True(async1.Equals(async2));
            Assert.True(async1 == async2);
            Assert.False(async1 != async2);
            Assert.Equal(async1.GetHashCode(), async2.GetHashCode());
        }

        [Fact]
        public void TestNotEquals()
        {
            var logEvent1 = new LogEventInfo(LogLevel.Debug, "logger1", "message1");
            AsyncContinuation cont1 = new AsyncContinuation(exception => { });
            AsyncContinuation cont2 = new AsyncContinuation(exception => { InternalLogger.Debug("test"); });
            var async1 = new AsyncLogEventInfo(logEvent1, cont1);
            var async2 = new AsyncLogEventInfo(logEvent1, cont2);
            Assert.False(async1.Equals(async2));
            Assert.False(async1 == async2);
            Assert.True(async1 != async2);

            //2 delegates will return the same hashcode, https://stackoverflow.com/questions/6624151/why-do-2-delegate-instances-return-the-same-hashcode
            //and that isn't really bad, so ignore this
            //   Assert.NotEqual(async1.GetHashCode(), async2.GetHashCode());
        }

        [Fact]
        public void TestNotEquals2()
        {
            var logEvent1 = new LogEventInfo(LogLevel.Debug, "logger1", "message1");
            var logEvent2 = new LogEventInfo(LogLevel.Debug, "logger1", "message1");
            AsyncContinuation cont = new AsyncContinuation(exception => { });
            var async1 = new AsyncLogEventInfo(logEvent1, cont);
            var async2 = new AsyncLogEventInfo(logEvent2, cont);
            Assert.False(async1.Equals(async2));
            Assert.False(async1 == async2);
            Assert.True(async1 != async2);

            Assert.NotEqual(async1.GetHashCode(), async2.GetHashCode());
        }

    }
}
