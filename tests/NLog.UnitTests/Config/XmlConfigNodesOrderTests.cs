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

namespace NLog.UnitTests.Config
{
    using NLog.Config;
    using Xunit;

    public class XmlConfigNodesOrderTests : NLogTestBase
    {
        [Fact]
        public void RulesBeforeTargetsTest()
        {
            LoggingConfiguration c = XmlLoggingConfiguration.CreateFromXmlString(@"
            <nlog>
                <rules>
                    <logger name='*' minLevel='Info' writeTo='d1' />
                </rules>

                <targets>
                    <target name='d1' type='Debug' />
                </targets>
            </nlog>");

            Assert.Equal(1, c.LoggingRules.Count);
            var rule = c.LoggingRules[0];
            Assert.Equal("*", rule.LoggerNamePattern);
            Assert.Equal(4, rule.Levels.Count);
            Assert.Contains(LogLevel.Info, rule.Levels);
            Assert.Contains(LogLevel.Warn, rule.Levels);
            Assert.Contains(LogLevel.Error, rule.Levels);
            Assert.Contains(LogLevel.Fatal, rule.Levels);
            Assert.Equal(1, rule.Targets.Count);
            Assert.Same(c.FindTargetByName("d1"), rule.Targets[0]);
            Assert.False(rule.Final);
            Assert.Equal(0, rule.Filters.Count);
        }
    }
}