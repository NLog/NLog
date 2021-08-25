﻿// 
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

namespace NLog.UnitTests.LayoutRenderers
{
    using Xunit;

    public class ScopePropertyTests : NLogTestBase
    {
        [Fact]
        public void ScopePropertyTest()
        {
            // Arrange
            ScopeContext.Clear();
            var logFactory = new LogFactory();
            logFactory.Setup().LoadConfigurationFromXml(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${scopeproperty:item=myitem} ${message}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>");
            var logger = logFactory.GetCurrentClassLogger();
            var target = logFactory.Configuration.FindTargetByName<NLog.Targets.DebugTarget>("debug");

            // Act
            using (logger.PushScopeProperty("myitem", "myvalue"))
            {
                logger.Debug("a");
            }

            // Assert
            Assert.Equal("myvalue a", target.LastMessage);
        }

        [Fact]
        public void ScopePropertyFormatTests()
        {
            // Arrange
            ScopeContext.Clear();
            var logFactory = new LogFactory();
            logFactory.Setup().LoadConfigurationFromXml(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${scopeproperty:item=myitem:format=@} ${message}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>");
            var logger = logFactory.GetCurrentClassLogger();
            var target = logFactory.Configuration.FindTargetByName<NLog.Targets.DebugTarget>("debug");

            // Act
            using (logger.PushScopeProperty("myitem", new { RequestId = 123 }))
            {
                logger.Debug("a");
            }

            // Assert
            Assert.Equal("{\"RequestId\":123} a", target.LastMessage);
        }
    }
}