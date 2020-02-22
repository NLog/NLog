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
    using Xunit;

    public class PaddingTests : NLogTestBase
    {
        [Fact]
        public void PositivePaddingWithLeftAlign()
        {
            SimpleLayout l;

            var le = LogEventInfo.Create(LogLevel.Info, "logger", "message");

            l = @"${message:padding=10:alignmentOnTruncation=left}";
            Assert.Equal("   message", l.Render(le));

            l = @"${message:padding=9:alignmentOnTruncation=left}";
            Assert.Equal("  message", l.Render(le));

            l = @"${message:padding=8:alignmentOnTruncation=left}";
            Assert.Equal(" message", l.Render(le));

            l = @"${message:padding=7:alignmentOnTruncation=left}";
            Assert.Equal("message", l.Render(le));

            l = @"${message:padding=6:alignmentOnTruncation=left}";
            Assert.Equal("message", l.Render(le));

            l = @"${message:padding=6:fixedLength=true:alignmentOnTruncation=left}";
            Assert.Equal("messag", l.Render(le));

            l = @"${message:padding=5:fixedLength=true:alignmentOnTruncation=left}";
            Assert.Equal("messa", l.Render(le));
        }

        [Fact]
        public void PositivePaddingWithRightAlign()
        {
            SimpleLayout l;

            var le = LogEventInfo.Create(LogLevel.Info, "logger", "message");

            l = @"${message:padding=10:alignmentOnTruncation=right}";
            Assert.Equal("   message", l.Render(le));

            l = @"${message:padding=9:alignmentOnTruncation=right}";
            Assert.Equal("  message", l.Render(le));

            l = @"${message:padding=8:alignmentOnTruncation=right}";
            Assert.Equal(" message", l.Render(le));

            l = @"${message:padding=7:alignmentOnTruncation=right}";
            Assert.Equal("message", l.Render(le));

            l = @"${message:padding=6:alignmentOnTruncation=right}";
            Assert.Equal("message", l.Render(le));

            l = @"${message:padding=6:fixedLength=true:alignmentOnTruncation=right}";
            Assert.Equal("essage", l.Render(le));

            l = @"${message:padding=5:fixedLength=true:alignmentOnTruncation=right}";
            Assert.Equal("ssage", l.Render(le));
        }

        [Fact]
        public void NegativePaddingWithLeftAlign()
        {
            SimpleLayout l;

            var le = LogEventInfo.Create(LogLevel.Info, "logger", "message");

            l = @"${message:padding=-10:alignmentOnTruncation=left}";
            Assert.Equal("message   ", l.Render(le));

            l = @"${message:padding=-9:alignmentOnTruncation=left}";
            Assert.Equal("message  ", l.Render(le));

            l = @"${message:padding=-8:alignmentOnTruncation=left}";
            Assert.Equal("message ", l.Render(le));

            l = @"${message:padding=-7:alignmentOnTruncation=left}";
            Assert.Equal("message", l.Render(le));

            l = @"${message:padding=-6:alignmentOnTruncation=left}";
            Assert.Equal("message", l.Render(le));

            l = @"${message:padding=-6:fixedLength=true:alignmentOnTruncation=left}";
            Assert.Equal("messag", l.Render(le));

            l = @"${message:padding=-5:fixedLength=true:alignmentOnTruncation=left}";
            Assert.Equal("messa", l.Render(le));
        }

        [Fact]
        public void NegativePaddingWithRightAlign()
        {
            SimpleLayout l;

            var le = LogEventInfo.Create(LogLevel.Info, "logger", "message");

            l = @"${message:padding=-10:alignmentOnTruncation=right}";
            Assert.Equal("message   ", l.Render(le));

            l = @"${message:padding=-9:alignmentOnTruncation=right}";
            Assert.Equal("message  ", l.Render(le));

            l = @"${message:padding=-8:alignmentOnTruncation=right}";
            Assert.Equal("message ", l.Render(le));

            l = @"${message:padding=-7:alignmentOnTruncation=right}";
            Assert.Equal("message", l.Render(le));

            l = @"${message:padding=-6:alignmentOnTruncation=right}";
            Assert.Equal("message", l.Render(le));

            l = @"${message:padding=-6:fixedLength=true:alignmentOnTruncation=right}";
            Assert.Equal("essage", l.Render(le));

            l = @"${message:padding=-5:fixedLength=true:alignmentOnTruncation=right}";
            Assert.Equal("ssage", l.Render(le));
        }

        [Fact]
        public void DefaultAlignmentIsLeft()
        {
            SimpleLayout defaultLayout, leftLayout, rightLayout;

            var le = LogEventInfo.Create(LogLevel.Info, "logger", "message");

            defaultLayout = @"${message:padding=5:fixedLength=true}";
            leftLayout = @"${message:padding=5:fixedLength=true:alignmentOnTruncation=left}";
            rightLayout = @"${message:padding=5:fixedLength=true:alignmentOnTruncation=right}";

            Assert.Equal(leftLayout.Render(le), defaultLayout.Render(le));
            Assert.NotEqual(rightLayout.Render(le), defaultLayout.Render(le));
        }
    }
}
