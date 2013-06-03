// 
// Copyright (c) 2004-2011 Jaroslaw Kowalski <jaak@jkowalski.net>
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

using NUnit.Framework;

#if !NUNIT
    using SetUp = Microsoft.VisualStudio.TestTools.UnitTesting.TestInitializeAttribute;
    using TestFixture = Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute;
    using Test = Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute;
    using TearDown = Microsoft.VisualStudio.TestTools.UnitTesting.TestCleanupAttribute;
    using ExpectedException = Microsoft.VisualStudio.TestTools.UnitTesting.ExpectedExceptionAttribute;
#endif

using NLog.Time;

namespace NLog.UnitTests
{
    [TestFixture]
    public class TimeSourceTests : NLogTestBase
    {
        [TearDown]
        public void TearDown()
        {
            TimeSource.Current = new CachedNowTimeSource();
        }

        [Test]
        public void NowTest()
        {
            TestTimeSource(new NowTimeSource(), DateTime.Now, DateTimeKind.Local);
        }

        [Test]
        public void UtcNowTest()
        {
            TestTimeSource(new UtcNowTimeSource(), DateTime.UtcNow, DateTimeKind.Utc);
        }

        [Test]
        public void CachedNowTest()
        {
            TestTimeSource(new CachedNowTimeSource(), DateTime.Now, DateTimeKind.Local);
        }

        [Test]
        public void CachedUtcNowTest()
        {
            TestTimeSource(new CachedUtcNowTimeSource(), DateTime.UtcNow, DateTimeKind.Utc);
        }

        [Test]
        public void CustomTimeSourceTest()
        {
            TestTimeSource(new CustomTimeSource(), DateTime.UtcNow.AddHours(1), DateTimeKind.Unspecified);
        }

        class CustomTimeSource : TimeSource
        {
            public override DateTime Time
            {
                get
                {
                    return new DateTime(DateTime.UtcNow.AddHours(1).Ticks, DateTimeKind.Unspecified);
                }
            }
        }

        void TestTimeSource(TimeSource source, DateTime expected, DateTimeKind kind)
        {
            Assert.IsInstanceOfType(typeof(CachedNowTimeSource), TimeSource.Current);
            TimeSource.Current = source;
            Assert.AreSame(source, TimeSource.Current);
            var evt = new LogEventInfo(LogLevel.Info, "logger", "msg");
            Assert.AreEqual(kind, evt.TimeStamp.Kind);
            Assert.IsTrue(expected.AddSeconds(-5) < evt.TimeStamp && evt.TimeStamp < expected.AddSeconds(5));
            LogEventInfo evt2;
            do
            {
                evt2 = new LogEventInfo(LogLevel.Info, "logger", "msg");
            } while (evt.TimeStamp == evt2.TimeStamp);
            Assert.AreEqual(kind, evt2.TimeStamp.Kind);
            Assert.IsTrue(evt2.TimeStamp > evt.TimeStamp);
            Assert.IsTrue(evt2.TimeStamp - evt.TimeStamp <= TimeSpan.FromSeconds(1));
        }
    }
}
