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

namespace NLog.UnitTests.LayoutRenderers.Wrappers
{
    using NLog;
    using NLog.Layouts;
    using System;
    using Xunit;

    public class ReplaceNewLinesTests : NLogTestBase
    {
        [Fact]
        public void ReplaceNewLineWithDefaultTest()
        {
            ScopeContext.PushProperty("foo", "bar" + Environment.NewLine + "123");
            SimpleLayout l = "${replace-newlines:${scopeproperty:foo}}";

            Assert.Equal("bar 123", l.Render(LogEventInfo.CreateNullEvent()));
        }

        [Fact]
        public void ReplaceNewLineWithDefaultTestUnix()
        {
            ScopeContext.PushProperty("foo", "bar\n123");
            SimpleLayout l = "${replace-newlines:${scopeproperty:foo}}";

            Assert.Equal("bar 123", l.Render(LogEventInfo.CreateNullEvent()));
        }

        [Fact]
        public void ReplaceNewLineWithDefaultTestWindows()
        {
            ScopeContext.PushProperty("foo", "bar\r\n123");
            SimpleLayout l = "${replace-newlines:${scopeproperty:foo}}";

            Assert.Equal("bar 123", l.Render(LogEventInfo.CreateNullEvent()));
        }

        [Fact]
        public void ReplaceNewLineWithDefaultTestMixed()
        {
            ScopeContext.PushProperty("foo", "bar\r\n123\nabc");
            SimpleLayout l = "${replace-newlines:${scopeproperty:foo}}";

            Assert.Equal("bar 123 abc", l.Render(LogEventInfo.CreateNullEvent()));
        }

        [Fact]
        public void ReplaceNewLineWithSpecifiedSeparationStringTest()
        {
            ScopeContext.PushProperty("foo", "bar" + System.Environment.NewLine + "123");
            SimpleLayout l = "${replace-newlines:replacement=|:${scopeproperty:foo}}";

            Assert.Equal("bar|123", l.Render(LogEventInfo.CreateNullEvent()));
        }

        [Fact]
        public void ReplaceNewLineOneLineTest()
        {
            ScopeContext.PushProperty("foo", "bar123");
            SimpleLayout l = "${replace-newlines:${scopeproperty:foo}}";

            Assert.Equal("bar123", l.Render(LogEventInfo.CreateNullEvent()));
        }

        [Fact]
        public void ReplaceNewLineWithNoEmptySeparationStringTest()
        {
            ScopeContext.PushProperty("foo", "bar" + System.Environment.NewLine + "123");
            SimpleLayout l = "${replace-newlines:replacement=:${scopeproperty:foo}}";

            Assert.Equal("bar123", l.Render(LogEventInfo.CreateNullEvent()));
        }
    }
}