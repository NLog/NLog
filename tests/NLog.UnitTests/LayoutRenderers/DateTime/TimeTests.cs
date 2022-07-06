// 
// Copyright (c) 2004-2021 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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

namespace NLog.UnitTests.LayoutRenderers
{
    using System.Globalization;
    using NLog.LayoutRenderers;
    using Xunit;

    public class TimeTests : NLogTestBase
    {
        [Fact]
        public void UniversalTimeTest()
        {
            var orgTimeSource = NLog.Time.TimeSource.Current;

            try
            {
                NLog.Time.TimeSource.Current = new NLog.Time.AccurateLocalTimeSource();

                var dt = new TimeLayoutRenderer();
                dt.UniversalTime = true;

                var ei = new LogEventInfo(LogLevel.Info, "logger", "msg");
                Assert.Equal(ei.TimeStamp.ToUniversalTime().ToString("HH:mm:ss.ffff", CultureInfo.InvariantCulture), dt.Render(ei));
            }
            finally
            {
                NLog.Time.TimeSource.Current = orgTimeSource;
            }
        }

        [Fact]
        public void LocalTimeTest()
        {
            var orgTimeSource = NLog.Time.TimeSource.Current;

            try
            {
                NLog.Time.TimeSource.Current = new NLog.Time.AccurateUtcTimeSource();

                var dt = new TimeLayoutRenderer();
                dt.UniversalTime = false;

                var ei = new LogEventInfo(LogLevel.Info, "logger", "msg");
                Assert.Equal(ei.TimeStamp.ToLocalTime().ToString("HH:mm:ss.ffff", CultureInfo.InvariantCulture), dt.Render(ei));
            }
            finally
            {
                NLog.Time.TimeSource.Current = orgTimeSource;
            }
        }

        [Fact]
        public void LocalTimeGermanTest()
        {
            var dt = new TimeLayoutRenderer() { Culture = new CultureInfo("de-DE") };
            dt.UniversalTime = false;

            var ei = new LogEventInfo(LogLevel.Info, "logger", "msg");
            Assert.Equal(ei.TimeStamp.ToString("HH:mm:ss,ffff", CultureInfo.InvariantCulture), dt.Render(ei));
        }

        [Fact]
        public void TimeTest()
        {
            var orgTimeSource = NLog.Time.TimeSource.Current;

            try
            {
                NLog.Time.TimeSource.Current = new NLog.Time.AccurateUtcTimeSource();

                var logFactory = new LogFactory().Setup().LoadConfigurationFromXml(@"
                <nlog>
                    <targets><target name='debug' type='Debug' layout='${time}' /></targets>
                    <rules>
                        <logger name='*' minlevel='Debug' writeTo='debug' />
                    </rules>
                </nlog>").LogFactory;

                logFactory.GetLogger("d").Debug("zzz");
                string date = GetDebugLastMessage("debug", logFactory);
                Assert.Equal(13, date.Length);
                Assert.Equal(':', date[2]);
                Assert.Equal(':', date[5]);
                Assert.Equal('.', date[8]);
            }
            finally
            {
                NLog.Time.TimeSource.Current = orgTimeSource;
            }
        }
    }
}