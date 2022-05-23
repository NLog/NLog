// 
// Copyright (c) 2004-2021 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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

namespace NLog.UnitTests.Filters
{
    using NLog.Filters;
    using NLog.Layouts;
    using Xunit;

    public class WhenContainsTests : NLogTestBase
    {
        [Fact]
        public void WhenContainsTest()
        {
            var logFactory = new LogFactory().Setup().LoadConfigurationFromXml(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${message}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug'>
                        <filters defaultAction='log'>
                            <whenContains layout='${message}' substring='zzz' action='Ignore' />
                        </filters>
                    </logger>
                </rules>
            </nlog>").LogFactory;

            var logger = logFactory.GetLogger("A");
            logger.Debug("a");
            logFactory.AssertDebugLastMessage("a");
            logger.Debug("zzz");
            logFactory.AssertDebugLastMessage("a");
            logger.Debug("ZzzZ");
            logFactory.AssertDebugLastMessage("ZzzZ");

            Assert.True(logFactory.Configuration.LoggingRules[0].Filters[0] is WhenContainsFilter);
            var wcf = (WhenContainsFilter)logFactory.Configuration.LoggingRules[0].Filters[0];
            Assert.IsType<SimpleLayout>(wcf.Layout);
            Assert.Equal("${message}", ((SimpleLayout)wcf.Layout).Text);
            Assert.Equal("zzz", wcf.Substring);
            Assert.Equal(FilterResult.Ignore, wcf.Action);
        }

        [Fact]
        public void WhenContainsInsensitiveTest()
        {
            var logFactory = new LogFactory().Setup().LoadConfigurationFromXml(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${message}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug'>
                        <filters defaultAction='log'>
                            <whenContains layout='${message}' substring='zzz' action='Ignore' ignoreCase='true' />
                        </filters>
                    </logger>
                </rules>
            </nlog>").LogFactory;

            var logger = logFactory.GetLogger("A");
            logger.Debug("a");
            logFactory.AssertDebugLastMessage("a");
            logger.Debug("zzz");
            logFactory.AssertDebugLastMessage("a");
            logger.Debug("ZzzZ");
            logFactory.AssertDebugLastMessage("a");
            logger.Debug("aaa");
            logFactory.AssertDebugLastMessage("aaa");
        }

        [Fact]
        public void WhenContainsQuoteTest()
        {
            var logFactory = new LogFactory().Setup().LoadConfigurationFromXml(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${message}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug'>
                        <filters defaultAction='log'>
                            <whenContains layout='${message}' substring='&apos;' action='Ignore' ignoreCase='true' />
                        </filters>
                    </logger>
                </rules>
            </nlog>").LogFactory;

            var logger = logFactory.GetLogger("A");
            logger.Debug("a");
            logFactory.AssertDebugLastMessage("a");
            logger.Debug("'");
            logFactory.AssertDebugLastMessage("a");
            logger.Debug("a'a");
            logFactory.AssertDebugLastMessage("a");
            logger.Debug("aaa");
            logFactory.AssertDebugLastMessage("aaa");
        }

        [Fact]
        public void WhenContainsQuoteTestComplex()
        {
            var logFactory = new LogFactory().Setup().LoadConfigurationFromXml(@"
            <nlog throwExceptions='true'>
                <targets><target name='debug' type='Debug' layout='${message}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug'>
                        <filters defaultAction='log'>
                            <when condition=""contains('${message}', 'Cannot insert the value NULL into column ''Col1')"" action=""Log""></when>
                            <when condition='true' action='Ignore' />
                        </filters>
                    </logger>
                </rules>
            </nlog>").LogFactory;

            var logger = logFactory.GetLogger("A");

            var expectedMessage = "Cannot insert the value NULL into column 'Col1";
            logger.Debug(expectedMessage);
            logFactory.AssertDebugLastMessage(expectedMessage);

            expectedMessage = "Cannot insert the value NULL into column 'Col1'";
            logger.Debug(expectedMessage);
            logFactory.AssertDebugLastMessage(expectedMessage);

            expectedMessage = "Cannot insert the value NULL into column 'COL1'";
            logger.Debug(expectedMessage);
            logFactory.AssertDebugLastMessage(expectedMessage);

            logger.Debug("Cannot insert the value NULL into column Col1");
            logFactory.AssertDebugLastMessage(expectedMessage);

            logger.Debug("Test");
            logFactory.AssertDebugLastMessage(expectedMessage);
        }

        [Fact]
        public void WhenContainsFilterActionMustOverrideDefault()
        {
            var ex = Assert.Throws<NLogConfigurationException>(() =>
            {
                var logFactory = new LogFactory().Setup().LoadConfigurationFromXml(@"
                <nlog>
                    <targets><target name='debug' type='Debug' layout='${message}' /></targets>
                    <rules>
                        <logger name='*' minlevel='Debug' writeTo='debug'>
                            <filters defaultAction='Ignore'>
                                <whenContains layout='${message}' substring='zzz' action='Ignore' />
                            </filters>
                        </logger>
                    </rules>
                </nlog>").LogFactory;
            });
            Assert.Contains("FilterDefaultAction=Ignore", ex.InnerException?.Message ?? ex.Message);
        }
    }
}