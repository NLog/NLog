// 
// Copyright (c) 2004-2011 Jaroslaw Kowalski <jaak@jkowalski.net>
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

using NUnit.Framework;

#if !NUNIT
    using SetUp = Microsoft.VisualStudio.TestTools.UnitTesting.TestInitializeAttribute;
    using TestFixture = Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute;
    using Test = Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute;
    using TearDown =  Microsoft.VisualStudio.TestTools.UnitTesting.TestCleanupAttribute;
#endif

namespace NLog.UnitTests
{
    [TestFixture]
    public class RoutingTests : NLogTestBase
    {
        [Test]
        public void LogThresholdTest()
        {
            LogManager.Configuration = CreateConfigurationFromString(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${message}' /></targets>
                <rules>
                    <logger name='*' minlevel='Info' writeTo='debug' />
                </rules>
            </nlog>");

            Logger logger = LogManager.GetLogger("A");
            logger.Debug("message");
            AssertDebugCounter("debug", 0);

            logger.Info("message");
            AssertDebugCounter("debug", 1);

            logger.Warn("message");
            AssertDebugCounter("debug", 2);

            logger.Error("message");
            AssertDebugCounter("debug", 3);

            logger.Fatal("message");
            AssertDebugCounter("debug", 4);
        }

        [Test]
        public void LogThresholdTest2()
        {
            LogManager.Configuration = CreateConfigurationFromString(@"
            <nlog>
                <targets>
                    <target name='debug1' type='Debug' layout='${message}' />
                    <target name='debug2' type='Debug' layout='${message}' />
                    <target name='debug3' type='Debug' layout='${message}' />
                    <target name='debug4' type='Debug' layout='${message}' />
                    <target name='debug5' type='Debug' layout='${message}' />
                </targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug1' />
                    <logger name='*' minlevel='Info' writeTo='debug2' />
                    <logger name='*' minlevel='Warn' writeTo='debug3' />
                    <logger name='*' minlevel='Error' writeTo='debug4' />
                    <logger name='*' minlevel='Fatal' writeTo='debug5' />
                </rules>
            </nlog>");

            Logger logger = LogManager.GetLogger("A");

            logger.Fatal("messageE");
            logger.Error("messageD");
            logger.Warn("messageC");
            logger.Info("messageB");
            logger.Debug("messageA");

            AssertDebugCounter("debug1", 5);
            AssertDebugCounter("debug2", 4);
            AssertDebugCounter("debug3", 3);
            AssertDebugCounter("debug4", 2);
            AssertDebugCounter("debug5", 1);

            AssertDebugLastMessage("debug1", "messageA");
            AssertDebugLastMessage("debug2", "messageB");
            AssertDebugLastMessage("debug3", "messageC");
            AssertDebugLastMessage("debug4", "messageD");
            AssertDebugLastMessage("debug5", "messageE");
        }

        [Test]
        public void LoggerNameMatchTest()
        {
            LogManager.Configuration = CreateConfigurationFromString(@"
            <nlog>
                <targets>
                    <target name='debug1' type='Debug' layout='${message}' />
                    <target name='debug2' type='Debug' layout='${message}' />
                    <target name='debug3' type='Debug' layout='${message}' />
                    <target name='debug4' type='Debug' layout='${message}' />
                </targets>
                <rules>
                    <logger name='A' minlevel='Info' writeTo='debug1' />
                    <logger name='A*' minlevel='Info' writeTo='debug2' />
                    <logger name='*A*' minlevel='Info' writeTo='debug3' />
                    <logger name='*A' minlevel='Info' writeTo='debug4' />
                </rules>
            </nlog>");

            LogManager.GetLogger("A").Info("message"); // matches 1st, 2nd, 3rd and 4th rule
            LogManager.GetLogger("A2").Info("message"); // matches 2nd rule and 3rd rule
            LogManager.GetLogger("BAD").Info("message"); // matches 3rd rule
            LogManager.GetLogger("BA").Info("message"); // matches 3rd and 4th rule

            AssertDebugCounter("debug1", 1);
            AssertDebugCounter("debug2", 2);
            AssertDebugCounter("debug3", 4);
            AssertDebugCounter("debug4", 2);
        }

        [Test]
        public void MultiAppenderTest()
        {
            LogManager.Configuration = CreateConfigurationFromString(@"
            <nlog>
                <targets>
                    <target name='debug1' type='Debug' layout='${message}' />
                    <target name='debug2' type='Debug' layout='${message}' />
                    <target name='debug3' type='Debug' layout='${message}' />
                    <target name='debug4' type='Debug' layout='${message}' />
                </targets>
                <rules>
                    <logger name='A' minlevel='Info' writeTo='debug1' />
                    <logger name='A' minlevel='Info' writeTo='debug2' />
                    <logger name='B' minlevel='Info' writeTo='debug1,debug2' />
                    <logger name='C' minlevel='Info' writeTo='debug1,debug2,debug3' />
                    <logger name='D' minlevel='Info' writeTo='debug1,debug2' />
                    <logger name='D' minlevel='Info' writeTo='debug3,debug4' />
                </rules>
            </nlog>");

            LogManager.GetLogger("A").Info("message");
            LogManager.GetLogger("B").Info("message");
            LogManager.GetLogger("C").Info("message");
            LogManager.GetLogger("D").Info("message");

            AssertDebugCounter("debug1", 4);
            AssertDebugCounter("debug2", 4);
            AssertDebugCounter("debug3", 2);
            AssertDebugCounter("debug4", 1);
        }
    }
}
