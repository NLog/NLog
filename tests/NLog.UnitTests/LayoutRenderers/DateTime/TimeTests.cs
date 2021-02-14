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

using System.Globalization;
using NLog.Config;

namespace NLog.UnitTests.LayoutRenderers
{
    using NLog.LayoutRenderers;
    using Xunit;

    public class TimeTests : NLogTestBase
    {
        [Fact]
        public void UniversalTimeTest()
        {
            var dt = new TimeLayoutRenderer();
            dt.UniversalTime = true;

            var ei = new LogEventInfo(LogLevel.Info, "logger", "msg");
            Assert.Equal(ei.TimeStamp.ToUniversalTime().ToString("HH:mm:ss.ffff", CultureInfo.InvariantCulture), dt.Render(ei));
        }

        [Fact]
        public void LocalTimeTest()
        {
            var dt = new TimeLayoutRenderer();
            dt.UniversalTime = false;

            var ei = new LogEventInfo(LogLevel.Info, "logger", "msg");
            Assert.Equal(ei.TimeStamp.ToString("HH:mm:ss.ffff", CultureInfo.InvariantCulture), dt.Render(ei));
        }
        
        [Fact]
        public void TimeTest()
        {
            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${time}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>");

            LogManager.GetLogger("d").Debug("zzz");
            string date = GetDebugLastMessage("debug");
            Assert.Equal(13, date.Length);
            Assert.Equal(':', date[2]);
            Assert.Equal(':', date[5]);
            Assert.Equal('.', date[8]);
        }

        [Fact]
        public void LongDateWithPaddingPadLeftAlignLeft()
        {
            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${longdate:padding=5:fixedlength=true}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>");

            LogManager.GetLogger("d").Debug("zzz");
            string date = GetDebugLastMessage("debug");
            Assert.Equal(5, date.Length);
            Assert.Equal('-', date[4]);
        }

        [Fact]
        public void LongDateWithPaddingPadLeftAlignRight()
        {
            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${longdate:padding=5:fixedlength=true:alignmentOnTruncation=right}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>");

            LogManager.GetLogger("d").Debug("zzz");
            string date = GetDebugLastMessage("debug");
            Assert.Equal(5, date.Length);
            Assert.Equal('.', date[0]);
        }

        [Fact]
        public void LongDateWithPaddingPadRightAlignLeft()
        {
            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${longdate:padding=-5:fixedlength=true:alignmentOnTruncation=left}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>");

            LogManager.GetLogger("d").Debug("zzz");
            string date = GetDebugLastMessage("debug");
            Assert.Equal(5, date.Length);
            Assert.Equal('-', date[4]);
        }

        [Fact]
        public void LongDateWithPaddingPadRightAlignRight()
        {
            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${longdate:padding=-5:fixedlength=true:alignmentOnTruncation=right}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>");

            LogManager.GetLogger("d").Debug("zzz");
            string date = GetDebugLastMessage("debug");
            Assert.Equal(5, date.Length);
            Assert.Equal('.', date[0]);
        }
    }
}