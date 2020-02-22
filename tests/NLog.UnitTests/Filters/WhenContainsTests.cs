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
    using Xunit;

    public class WhenContainsTests : NLogTestBase
    {
        [Fact]
        public void WhenContainsTest()
        {
            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${message}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug'>
                   <filters defaultAction='log'>
                        <whenContains layout='${message}' substring='zzz' action='Ignore' />
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
            AssertDebugCounter("debug", 2);
        }

        [Fact]
        public void WhenContainsInsensitiveTest()
        {
            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${message}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug'>
                   <filters defaultAction='log'>
                        <whenContains layout='${message}' substring='zzz' action='Ignore' ignoreCase='true' />
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
            logger.Debug("aaa");
            AssertDebugCounter("debug", 2);
        }

        [Fact]
        public void WhenContainsQuoteTest()
        {
            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${message}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug'>
                   <filters defaultAction='log'>
                        <whenContains layout='${message}' substring='&apos;' action='Ignore' ignoreCase='true' />
                    </filters>
                    </logger>
                </rules>
            </nlog>");

            ILogger logger = LogManager.GetLogger("A");
            logger.Debug("'");
            AssertDebugCounter("debug", 0);
            logger.Debug("a'a");
            AssertDebugCounter("debug", 0);
            logger.Debug("a");
            AssertDebugCounter("debug", 1);
            logger.Debug("aaa");
            AssertDebugCounter("debug", 2);
        }

        [Fact]
        public void WhenContainsQuoteTestComplex()
        {
            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
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
            </nlog>");

            ILogger logger = LogManager.GetLogger("A");
            logger.Debug("Test");
            AssertDebugCounter("debug", 0);
            logger.Debug("Cannot insert the value NULL into column 'Col1");
            AssertDebugCounter("debug", 1);
            logger.Debug("Cannot insert the value NULL into column 'Col1'");
            AssertDebugCounter("debug", 2);
            logger.Debug("Cannot insert the value NULL into column Col1");
            AssertDebugCounter("debug", 2);
            logger.Debug("Cannot insert the value NULL into column 'COL1'");
            AssertDebugCounter("debug", 3);
        }
    }
}