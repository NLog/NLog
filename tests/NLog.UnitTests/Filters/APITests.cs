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

namespace NLog.UnitTests.Filters
{
    using System.Linq;
    using NLog.Config;
    using NLog.Layouts;
    using NLog.Filters;
    using Xunit;

    public class APITests : NLogTestBase
    {
        [Fact]
        public void APITest()
        {
            // this is mostly to make Clover happy

            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${message}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug'>
                    <filters>
                        <whenContains layout='${message}' substring='zzz' action='Ignore' />
                    </filters>
                    </logger>
                </rules>
            </nlog>");

            Assert.True(LogManager.Configuration.LoggingRules[0].Filters[0] is WhenContainsFilter);
            var wcf = (WhenContainsFilter)LogManager.Configuration.LoggingRules[0].Filters[0];
            Assert.IsType<SimpleLayout>(wcf.Layout);
            Assert.Equal("${message}", ((SimpleLayout)wcf.Layout).Text);
            Assert.Equal("zzz", wcf.Substring);
            Assert.Equal(FilterResult.Ignore, wcf.Action);
        }

        [Fact]
        public void WhenMethodFilterApiTest()
        {
            // Stage
            var logFactory = new LogFactory();
            var logger1 = logFactory.GetLogger("Hello");
            var logger2 = logFactory.GetLogger("Goodbye");
            var config = new LoggingConfiguration(logFactory);
            var target = new NLog.Targets.DebugTarget() { Layout = "${message}" };
            config.AddRuleForAllLevels(target);
            config.LoggingRules.Last().Filters.Add(new WhenMethodFilter((l) => l.LoggerName == logger1.Name ? FilterResult.Ignore : FilterResult.Log));
            logFactory.Configuration = config;

            // Act 1
            logger1.Info("Hello World");
            Assert.Empty(target.LastMessage);

            // Act 2
            logger2.Info("Goodbye World");
            Assert.Equal("Goodbye World", target.LastMessage);
        }
    }
}