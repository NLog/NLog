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
    using NLog.Layouts;
    using Xunit;

    public class CachedTests : NLogTestBase
    {
        [Fact]
        public void CachedLayoutRendererWrapper()
        {
            SimpleLayout l = "${guid}";

            string s1 = l.Render(LogEventInfo.CreateNullEvent());
            string s2 = l.Render(LogEventInfo.CreateNullEvent());
            string s3;

            // normally GUIDs are never the same
            Assert.NotEqual(s1, s2);

            // but when you apply ${cached}, the guid will only be generated once
            l = "${cached:${guid}:cached=true}";
            s1 = l.Render(LogEventInfo.CreateNullEvent());
            s2 = l.Render(LogEventInfo.CreateNullEvent());
            Assert.Equal(s1, s2);

            // calling Close() on Layout Renderer will reset the cached value
            l.Renderers[0].Close();
            s3 = l.Render(LogEventInfo.CreateNullEvent());
            Assert.NotEqual(s2, s3);

            // unless we use clearcache=none
            l = "${cached:${guid}:cached=true:clearcache=none}";
            s1 = l.Render(LogEventInfo.CreateNullEvent());
            l.Renderers[0].Close();
            s2 = l.Render(LogEventInfo.CreateNullEvent());
            Assert.Equal(s1, s2);

            // another way to achieve the same thing is using cached=true
            l = "${guid:cached=true}";
            s1 = l.Render(LogEventInfo.CreateNullEvent());
            s2 = l.Render(LogEventInfo.CreateNullEvent());
            Assert.Equal(s1, s2);

            // another way to achieve the same thing is using cached=true
            l = "${guid:cached=false}";
            s1 = l.Render(LogEventInfo.CreateNullEvent());
            s2 = l.Render(LogEventInfo.CreateNullEvent());
            Assert.NotEqual(s1, s2);
        }

        /// <summary>
        /// test the cachekey
        /// </summary>
        [Fact]
        public void CacheKeyTest()
        {
            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
            <nlog>
                <targets><target name='debug' type='debug' layout='${cached:${guid}:cached=true:cachekey=${var:var1}}' /></targets>
                <rules>
                    <logger name='*' minlevel='debug' appendto='debug'>
                       
                    </logger>
                </rules>
            </nlog>");
            LogManager.Configuration.Variables["var1"] = "a";

            ILogger logger = LogManager.GetLogger("A");
            logger.Debug("msg");
            var s1 = GetDebugLastMessage("debug");
            logger.Debug("msg");
            var s2 = GetDebugLastMessage("debug");
            Assert.NotEmpty(s1);
            Assert.Equal(s1, s2);
            //change var will reset cache
            LogManager.Configuration.Variables["var1"] = "b";
            logger.Debug("msg");
            var s3 = GetDebugLastMessage("debug");
            Assert.NotEmpty(s3);
            Assert.NotEqual(s1, s3);
        }

        [Fact]
        public void CachedSecondsTimeoutTest()
        {
            SimpleLayout l = "${guid:cachedSeconds=60}";
            var s1 = l.Render(LogEventInfo.CreateNullEvent());
            var s2 = l.Render(new LogEventInfo());
            Assert.Equal(s1, s2);
            var s3 = l.Render(new LogEventInfo() { TimeStamp = NLog.Time.TimeSource.Current.Time.AddMinutes(2) });
            Assert.NotEqual(s2, s3);
        }
    }
}