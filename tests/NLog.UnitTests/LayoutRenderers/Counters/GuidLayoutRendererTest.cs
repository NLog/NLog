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

namespace NLog.UnitTests.LayoutRenderers
{
    using NLog.LayoutRenderers;
    using Xunit;

    public class GuidLayoutRendererTest : NLogTestBase
    {
        [Fact]
        public void GuidTest()
        {
            GuidLayoutRenderer layoutRenderer = new GuidLayoutRenderer();
            LogEventInfo logEvent = LogEventInfo.CreateNullEvent();
            string newGuid1 = layoutRenderer.Render(logEvent);
            string newGuid2 = layoutRenderer.Render(logEvent);
            Assert.True(!string.IsNullOrEmpty(newGuid1));
            Assert.True(!string.IsNullOrEmpty(newGuid2));
            Assert.NotEqual(newGuid1, newGuid2);
        }

        [Fact]
        public void LogEventGuidTest()
        {
            GuidLayoutRenderer layoutRenderer = new GuidLayoutRenderer() { GeneratedFromLogEvent = true };
            LogEventInfo logEvent1 = LogEventInfo.CreateNullEvent();
            string newGuid11 = layoutRenderer.Render(logEvent1);
            string newGuid12 = layoutRenderer.Render(logEvent1);
            Assert.True(!string.IsNullOrEmpty(newGuid11));
            Assert.True(!string.IsNullOrEmpty(newGuid12));
            Assert.Equal(newGuid11, newGuid12);
            LogEventInfo logEvent2 = LogEventInfo.CreateNullEvent();
            string newGuid21 = layoutRenderer.Render(logEvent2);
            string newGuid22 = layoutRenderer.Render(logEvent2);
            Assert.True(!string.IsNullOrEmpty(newGuid21));
            Assert.True(!string.IsNullOrEmpty(newGuid22));
            Assert.Equal(newGuid21, newGuid22);
            Assert.NotEqual(newGuid11, newGuid22);
        }
    }
}