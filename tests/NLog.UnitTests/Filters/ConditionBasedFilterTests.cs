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

namespace NLog.UnitTests.Filters
{
    using NLog;
    using Xunit;

    public class ConditionBasedFilterTests : NLogTestBase
    {
        [Fact]
        public void WhenTest()
        {
            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${message}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug'>
                   <filters defaultAction='log'>
                        <when condition=""contains(message,'zzz')"" action='Ignore' />
                    </filters>
                    </logger>
                </rules>
            </nlog>");

            ILogger logger = LogManager.GetLogger("A");
            logger.Debug("a");
            AssertDebugCounter("debug", 1);
            logger.Debug("zzz");
            AssertDebugCounter("debug", 1);
            logger.Debug("ZzzZ");
            AssertDebugCounter("debug", 1);
            logger.Debug("Zz");
            AssertDebugCounter("debug", 2);
        }

        [Fact]
        public void WhenLogLevelTest()
        {
            var logFactory = new LogFactory().Setup().LoadConfigurationFromXml(@"
                <nlog>
                    <targets><target name='debug' type='debug' layout='${message}' /></targets>
                    <rules>
                        <logger name='*' minLevel='Debug' writeTo='debug'>
                            <filters defaultAction='ignore'>
                                <when condition=""level >= '${scopeproperty:filterlevel:whenEmpty=Off}'"" action='Log' />
                            </filters>
                        </logger>
                    </rules>
                </nlog>
            ").LogFactory;
            var logger = logFactory.GetCurrentClassLogger();

            logger.Fatal("Hello Emptiness");
            logFactory.AssertDebugLastMessage("");

            using (logger.PushScopeProperty("filterLevel", LogLevel.Warn))
            {
                logger.Error("Hello can you hear me");
                logFactory.AssertDebugLastMessage("Hello can you hear me");
            }
        }

        [Fact]
        public void WhenExceptionTest()
        {
            var logFactory = new LogFactory().Setup().LoadConfigurationFromXml(@"
                <nlog>
                    <targets><target name='debug' type='debug' layout='${message}' /></targets>
                    <rules>
                        <logger name='*' minLevel='Debug' writeTo='debug'>
                            <filters defaultAction='ignore'>
                                <when condition='exception != null' action='Log' />
                            </filters>
                        </logger>
                    </rules>
                </nlog>
            ").LogFactory;
            var logger = logFactory.GetCurrentClassLogger();

            logger.Fatal("Hello missing Exception");
            logFactory.AssertDebugLastMessage("");

            logger.Error(new System.Exception("Oh no"), "Hello with Exception");
            logFactory.AssertDebugLastMessage("Hello with Exception");
        }
    }
}