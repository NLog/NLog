//
// Copyright (c) 2004-2024 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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
    using NLog.Layouts;
    using Xunit;

    public class LoggerNameTests : NLogTestBase
    {
        [Fact]
        public void LoggerNameTest()
        {
            var logFactory = new LogFactory().Setup().LoadConfigurationFromXml(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${logger} ${message}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>").LogFactory;

            var logger = logFactory.GetLogger("A");
            logger.Debug("a");
            logFactory.AssertDebugLastMessage("A a");
        }


        [Fact]
        public void LoggerShortNameTest()
        {
            var logFactory = new LogFactory().Setup().LoadConfigurationFromXml(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${logger:ShortName=true} ${message}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>").LogFactory;

            var logger = logFactory.GetLogger("A.B.C");
            logger.Debug("a");
            logFactory.AssertDebugLastMessage("C a");

            var renderer = new NLog.LayoutRenderers.LoggerNameLayoutRenderer() { ShortName = true };
            var result = renderer.Render(new LogEventInfo() { LoggerName = logger.Name });
            Assert.Equal("C", result);
        }

        [Fact]
        public void LoggerShortNameTest2()
        {
            var logFactory = new LogFactory().Setup().LoadConfigurationFromXml(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${logger:ShortName=true} ${message}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>").LogFactory;

            var logger = logFactory.GetLogger("C");
            logger.Debug("a");
            logFactory.AssertDebugLastMessage("C a");

            var renderer = new NLog.LayoutRenderers.LoggerNameLayoutRenderer() { ShortName = true };
            var result = renderer.Render(new LogEventInfo() { LoggerName = logger.Name });
            Assert.Equal("C", result);
        }

        [Fact]
        public void LoggerShortNameTest_false()
        {
            var logFactory = new LogFactory().Setup().LoadConfigurationFromXml(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${logger:ShortName=false} ${message}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>").LogFactory;

            var logger = logFactory.GetLogger("A.B.C");
            logger.Debug("a");
            logFactory.AssertDebugLastMessage("A.B.C a");
        }

        [Fact]
        public void LoggerPrefixNameTest()
        {
            var logFactory = new LogFactory().Setup().LoadConfigurationFromXml(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${logger:PrefixName=true} ${message}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>").LogFactory;

            var logger = logFactory.GetLogger("A.B.C");
            logger.Debug("a");
            logFactory.AssertDebugLastMessage("A.B a");

            var layout = new SimpleLayout("${logger:PrefixName=true}");
            var result = layout.Render(new LogEventInfo() { LoggerName = logger.Name });
            Assert.Equal("A.B", result);
        }

        [Fact]
        public void LoggerPrefixNameTest2()
        {
            var logFactory = new LogFactory().Setup().LoadConfigurationFromXml(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${logger:PrefixName=true} ${message}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>").LogFactory;

            var logger = logFactory.GetLogger("C");
            logger.Debug("a");
            logFactory.AssertDebugLastMessage("C a");

            var layout = new SimpleLayout("${logger:PrefixName=true}");
            var result = layout.Render(new LogEventInfo() { LoggerName = logger.Name });
            Assert.Equal("C", result);
        }

        [Theory]
        [InlineData("logger")]
        [InlineData("logger-name")]
        [InlineData("loggername")]
        public void LoggerNameAliasTest(string loggerLayout)
        {
            var logFactory = new LogFactory().Setup().LoadConfigurationFromXml($@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${{{loggerLayout}}} ${{message}}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>").LogFactory;

            var logger = logFactory.GetLogger("A.B.C");
            logger.Debug("a");
            logFactory.AssertDebugLastMessage("A.B.C a");
        }
    }
}
