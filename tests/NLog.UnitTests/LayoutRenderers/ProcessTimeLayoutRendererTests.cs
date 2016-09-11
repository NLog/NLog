// 
// Copyright (c) 2004-2016 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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

//no silverlight because of xUnit needed
#if !SILVERLIGHT

using System;
using System.Text;
using NLog.LayoutRenderers;
using Xunit;
using Xunit.Extensions;

namespace NLog.UnitTests.LayoutRenderers
{
    public class ProcessTimeLayoutRendererTests : NLogTestBase
    {
        [Theory]
        [InlineData(0, 1, 2, 30, 0, "01:02:30.000")]
        [InlineData(0, 1, 2, 30, 1, "01:02:30.001")]
        [InlineData(0, 1, 2, 30, 20, "01:02:30.020")]
        [InlineData(0, 11, 2, 30, 20, "11:02:30.020")]
        [InlineData(0, 50, 2, 30, 20, "02:02:30.020")]
        [InlineData(0, 1, 2, 30, 506, "01:02:30.506")]
        [InlineData(0, 1, 2, 30, -506, "01:02:29.494")]
        [InlineData(0, 0, 0, 0, -506, "00:00:00.000")]
        [InlineData(0, 0, 0, 0, 0, "00:00:00.000")]
        [InlineData(1, 0, 0, 0, 0, "00:00:00.000")]
        [InlineData(1, 0, 0, 0, 0, "00:00:00.000")]
        public void RenderTimeSpanTest(int day, int hour, int min, int sec, int milisec, string expected)
        {

            var time = new TimeSpan(day, hour, min, sec, milisec);

            var sb = new StringBuilder();
            ProcessTimeLayoutRenderer.WritetTimestamp(sb, time, null);
            var result = sb.ToString();
            Assert.Equal(expected, result);
        }

#if !NET3_5

        [Fact]
        public void RenderProcessTimeLayoutRenderer()
        {
            var layout = "${processtime}";
            var logEvent = new LogEventInfo(LogLevel.Debug, "logger1", "message1");
            var time = logEvent.TimeStamp.ToUniversalTime() - LogEventInfo.ZeroDate;

            var expected = time.ToString("hh\\:mm\\:ss\\.fff");
            AssertLayoutRendererOutput(layout, logEvent, expected);
        }
#endif
    }
}
#endif