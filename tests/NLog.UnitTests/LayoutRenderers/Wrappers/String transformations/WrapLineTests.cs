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

namespace NLog.UnitTests.LayoutRenderers.Wrappers
{
    using NLog;
    using NLog.Common;
    using NLog.Layouts;
    using NLog.Targets;
    using Xunit;

    public class WrapLineTests : NLogTestBase
    {
        [Fact]
        public void WrapLineWithInnerLayoutDefaultTest()
        {
            ScopeContext.PushProperty("foo", "foobar");

            SimpleLayout le = "${wrapline:${scopeproperty:foo}:WrapLine=3}";

            Assert.Equal("foo" + System.Environment.NewLine + "bar", le.Render(LogEventInfo.CreateNullEvent()));
        }

        [Fact]
        public void WrapLineWithInnerLayoutTest()
        {
            ScopeContext.PushProperty("foo", "foobar");

            SimpleLayout le = "${wrapline:Inner=${scopeproperty:foo}:WrapLine=3}";

            Assert.Equal("foo" + System.Environment.NewLine + "bar", le.Render(LogEventInfo.CreateNullEvent()));
        }

        [Fact]
        public void WrapLineAtPositionOnceTest()
        {
            SimpleLayout l = "${message:wrapline=3}";

            var le = LogEventInfo.Create(LogLevel.Info, "logger", "foobar");

            Assert.Equal("foo" + System.Environment.NewLine + "bar", l.Render(le));
        }

        [Fact]
        public void WrapLineAtPositionOnceTextLengthNotMultipleTest()
        {
            SimpleLayout l = "${message:wrapline=3}";

            var le = LogEventInfo.Create(LogLevel.Info, "logger", "fooba");

            Assert.Equal("foo" + System.Environment.NewLine + "ba", l.Render(le));
        }

        [Fact]
        public void WrapLineMultipleTimesTest()
        {
            SimpleLayout l = "${message:wrapline=3}";

            var le = LogEventInfo.Create(LogLevel.Info, "logger", "foobarbaz");

            Assert.Equal("foo" + System.Environment.NewLine + "bar" + System.Environment.NewLine + "baz", l.Render(le));
        }

        [Fact]
        public void WrapLineMultipleTimesTextLengthNotMultipleTest()
        {
            SimpleLayout l = "${message:wrapline=3}";

            var le = LogEventInfo.Create(LogLevel.Info, "logger", "foobarba");

            Assert.Equal("foo" + System.Environment.NewLine + "bar" + System.Environment.NewLine + "ba", l.Render(le));
        }

        [Fact]
        public void WrapLineAtPositionAtExactTextLengthTest()
        {
            SimpleLayout l = "${message:wrapline=6}";

            var le = LogEventInfo.Create(LogLevel.Info, "logger", "foobar");

            Assert.Equal("foobar", l.Render(le));
        }

        [Fact]
        public void WrapLineAtPositionGreaterThanTextLengthTest()
        {
            SimpleLayout l = "${message:wrapline=10}";

            var le = LogEventInfo.Create(LogLevel.Info, "logger", "foobar");

            Assert.Equal("foobar", l.Render(le));
        }

        [Fact]
        public void WrapLineAtPositionZeroTest()
        {
            SimpleLayout l = "${message:wrapline=0}";

            var le = LogEventInfo.Create(LogLevel.Info, "logger", "foobar");

            Assert.Equal("foobar", l.Render(le));
        }

        [Fact]
        public void WrapLineAtNegativePositionTest()
        {
            SimpleLayout l = "${message:wrapline=0}";

            var le = LogEventInfo.Create(LogLevel.Info, "logger", "foobar");

            Assert.Equal("foobar", l.Render(le));
        }

        [Fact]
        public void WrapLineFromConfig()
        {
            var configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
<nlog throwExceptions='true'>
    <targets>
        <target name='d1' type='Debug' layout='${message:wrapline=3}' />
    </targets>
    <rules>
      <logger name=""*"" minlevel=""Trace"" writeTo=""d1"" />
    </rules>
</nlog>");

            var d1 = configuration.FindTargetByName("d1") as DebugTarget;
            Assert.NotNull(d1);
            var layout = d1.Layout as SimpleLayout;
            Assert.NotNull(layout);

            var result = layout.Render(new LogEventInfo(LogLevel.Info, "Test", "foobar"));
            Assert.Equal("foo" + System.Environment.NewLine + "bar", result);
        }
    }
}