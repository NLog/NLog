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
    using NLog.LayoutRenderers;
    using Xunit;

    public class ShortDateTests : NLogTestBase
    {
        [Fact]
        public void UniversalTimeTest()
        {
            var layoutRenderer = new ShortDateLayoutRenderer();
            layoutRenderer.UniversalTime = true;

            var logEvent = new LogEventInfo(LogLevel.Info, "logger", "msg");
            Assert.Equal(logEvent.TimeStamp.ToUniversalTime().ToString("yyyy-MM-dd"), layoutRenderer.Render(logEvent));
        }

        [Fact]
        public void LocalTimeTest()
        {
            var layoutRenderer = new ShortDateLayoutRenderer();
            layoutRenderer.UniversalTime = false;

            var logEvent = new LogEventInfo(LogLevel.Info, "logger", "msg");
            Assert.Equal(logEvent.TimeStamp.ToString("yyyy-MM-dd"), layoutRenderer.Render(logEvent));
        }

        [Fact]
        public void ShortDateTest()
        {
            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${shortdate}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>");

            LogManager.GetLogger("d").Debug("zzz");
            AssertDebugLastMessage("debug", DateTime.Now.ToString("yyyy-MM-dd"));
        }

        [Fact]
        public void OneDigitMonthTest()
        {
            var layoutRenderer = new ShortDateLayoutRenderer();
            layoutRenderer.UniversalTime = false;

            var logEvent = new LogEventInfo(LogLevel.Info, "logger", "msg");
            logEvent.TimeStamp = new DateTime(2015, 1, 1);
            Assert.Equal(logEvent.TimeStamp.ToString("yyyy-MM-dd"), layoutRenderer.Render(logEvent));
        }

        [Fact]
        public void TwoDigitMonthTest()
        {
            var layoutRenderer = new ShortDateLayoutRenderer();
            layoutRenderer.UniversalTime = false;

            var logEvent = new LogEventInfo(LogLevel.Info, "logger", "msg");
            logEvent.TimeStamp = new DateTime(2015, 12, 1);
            Assert.Equal(logEvent.TimeStamp.ToString("yyyy-MM-dd"), layoutRenderer.Render(logEvent));
        }

        [Fact]
        public void OneDigitDayTest()
        {
            var layoutRenderer = new ShortDateLayoutRenderer();
            layoutRenderer.UniversalTime = false;

            var logEvent = new LogEventInfo(LogLevel.Info, "logger", "msg");
            logEvent.TimeStamp = new DateTime(2015, 1, 1);
            Assert.Equal(logEvent.TimeStamp.ToString("yyyy-MM-dd"), layoutRenderer.Render(logEvent));
        }

        [Fact]
        public void TwoDigitDayTest()
        {
            var layoutRenderer = new ShortDateLayoutRenderer();
            layoutRenderer.UniversalTime = false;

            var logEvent = new LogEventInfo(LogLevel.Info, "logger", "msg");
            logEvent.TimeStamp = new DateTime(2015, 12, 12);
            Assert.Equal(logEvent.TimeStamp.ToString("yyyy-MM-dd"), layoutRenderer.Render(logEvent));
        }
    }
}