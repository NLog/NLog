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

using NLog.Config;

namespace NLog.UnitTests.LayoutRenderers
{
    using System;
    using System.Globalization;
    using NLog.LayoutRenderers;
    using Xunit;

    public class DateTests : NLogTestBase
    {
        [Fact]
        public void DefaultDateTest()
        {
            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${date}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>");

            LogManager.GetLogger("d").Debug("zzz");
            DateTime dt = DateTime.ParseExact(GetDebugLastMessage("debug"), "yyyy/MM/dd HH:mm:ss.fff", CultureInfo.InvariantCulture);
            DateTime now = DateTime.Now;

            Assert.True(Math.Abs((dt - now).TotalSeconds) < 5);
        }

        [Fact]
        public void TimeZoneTest()
        {
            var dateLayoutRenderer = new DateLayoutRenderer();
          
            dateLayoutRenderer.Format = "yyyy-MM-ddTHH:mmK";

            var logEvent = new LogEventInfo(LogLevel.Info, "logger", "msg");
            var result = dateLayoutRenderer.Render(logEvent);

            var offset = TimeZoneInfo.Local;
            var offset2 = offset.GetUtcOffset(DateTime.Now);

            if (offset2 >= new TimeSpan(0))
            {
                //+00:00, +01:00 etc
                Assert.Contains($"+{offset2.Hours:D2}:{offset2.Minutes:D2}", result);
            }
            else
            {
                //-01:00, etc
                Assert.Contains($"{offset2.Hours:D2}:{offset2.Minutes:D2}", result);
            }

        }

        [Fact]
        public void UniversalTimeTest()
        {
            var dt = new DateLayoutRenderer();
            dt.UniversalTime = true;
            dt.Format = "R";

            var ei = new LogEventInfo(LogLevel.Info, "logger", "msg");
            Assert.Equal(ei.TimeStamp.ToUniversalTime().ToString("R"), dt.Render(ei));
        }

        [Fact]
        public void LocalTimeTest()
        {
            var dt = new DateLayoutRenderer();
            dt.UniversalTime = false;
            dt.Format = "R";

            var ei = new LogEventInfo(LogLevel.Info, "logger", "msg");
            Assert.Equal(ei.TimeStamp.ToString("R"), dt.Render(ei));
        }

        [Fact]
        public void FormattedDateTest()
        {
            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${date:format=yyyy-MM-dd}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>");

            LogManager.GetLogger("d").Debug("zzz");
            AssertDebugLastMessage("debug", DateTime.Now.ToString("yyyy-MM-dd"));
        }
    }
}