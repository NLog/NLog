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

namespace NLog.UnitTests
{
    using System;
    using Time;
    using Xunit;

    public class TimeSourceTests : NLogTestBase, IDisposable
    {
        public void Dispose()
        {
            TimeSource.Current = new FastLocalTimeSource();
        }

        [Fact]
        public void AccurateLocalTest()
        {
            TestTimeSource(new AccurateLocalTimeSource(), DateTime.Now, DateTimeKind.Local);
        }

        [Fact]
        public void AccurateUtcTest()
        {
            TestTimeSource(new AccurateUtcTimeSource(), DateTime.UtcNow, DateTimeKind.Utc);
        }

        [Fact]
        public void FastLocalTest()
        {
            TestTimeSource(new FastLocalTimeSource(), DateTime.Now, DateTimeKind.Local);
        }

        [Fact]
        public void FastUtcTest()
        {
            TestTimeSource(new FastUtcTimeSource(), DateTime.UtcNow, DateTimeKind.Utc);
        }

        [Fact]
        public void CustomTimeSourceTest()
        {
            TestTimeSource(new CustomTimeSource(), DateTime.UtcNow.AddHours(1), DateTimeKind.Unspecified);
        }
        
        [Theory]
        [InlineData("FastUTC", typeof(FastUtcTimeSource))]
        [InlineData("FastLocal", typeof(FastLocalTimeSource))]
        [InlineData("AccurateUTC", typeof(AccurateUtcTimeSource))]
        [InlineData("AccurateLocal", typeof(AccurateLocalTimeSource))]
        public void ToStringDefaultImplementationsTest(string expectedName, Type timeSourceType)
        {
            var instance = Activator.CreateInstance(timeSourceType) as TimeSource;
            var actual = instance.ToString();

            Assert.Equal(expectedName + " (time source)", actual);
        }

        [Theory]
        [InlineData(typeof(CustomTimeSource))]
        public void ToStringNoImplementationTest(Type timeSourceType)
        {
            var instance = Activator.CreateInstance(timeSourceType) as TimeSource;

            var expected = timeSourceType.Name;
            var actual = instance.ToString();

            Assert.Equal(expected, actual);
        }

        class CustomTimeSource : TimeSource
        {
            public override DateTime Time => FromSystemTime(DateTime.UtcNow);

            public override DateTime FromSystemTime(DateTime systemTime)
            {
                return new DateTime(systemTime.ToUniversalTime().AddHours(1).Ticks, DateTimeKind.Unspecified);
            }
        }

        internal class ShiftedTimeSource : TimeSource
        {
            private readonly DateTimeKind kind;
            private DateTimeOffset sourceTime;
            private TimeSpan systemTimeDelta;

            public ShiftedTimeSource(DateTimeKind kind)
            {
                this.kind = kind;
                sourceTime = DateTimeOffset.UtcNow;
                systemTimeDelta = TimeSpan.Zero;
            }

            public override DateTime Time => ConvertToKind(sourceTime);

            public DateTime SystemTime => ConvertToKind(sourceTime - systemTimeDelta);

            public override DateTime FromSystemTime(DateTime systemTime)
            {
                return ConvertToKind(systemTime + systemTimeDelta);
            }

            private DateTime ConvertToKind(DateTimeOffset value)
            {
                return kind == DateTimeKind.Local ? value.LocalDateTime : value.UtcDateTime;
            }

            public void AddToSystemTime(TimeSpan delta)
            {
                systemTimeDelta += delta;
            }

            public void AddToLocalTime(TimeSpan delta)
            {
                sourceTime += delta;
            }
        }



        void TestTimeSource(TimeSource source, DateTime expected, DateTimeKind kind)
        {
            Assert.IsType<FastLocalTimeSource>(TimeSource.Current);
            TimeSource.Current = source;
            Assert.Same(source, TimeSource.Current);
            var evt = new LogEventInfo(LogLevel.Info, "logger", "msg");
            Assert.Equal(kind, evt.TimeStamp.Kind);
            Assert.True((expected - evt.TimeStamp).Duration() < TimeSpan.FromSeconds(5));

            Assert.True((source.Time - source.FromSystemTime(DateTime.UtcNow)).Duration() < TimeSpan.FromSeconds(5));

            LogEventInfo evt2;
            do
            {
                evt2 = new LogEventInfo(LogLevel.Info, "logger", "msg");
            } while (evt.TimeStamp == evt2.TimeStamp);
            Assert.Equal(kind, evt2.TimeStamp.Kind);
            Assert.True(evt2.TimeStamp > evt.TimeStamp);
            Assert.True(evt2.TimeStamp - evt.TimeStamp <= TimeSpan.FromSeconds(1));
        }
    }
}
