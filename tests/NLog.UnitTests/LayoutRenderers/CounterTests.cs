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

using System;
using System.Xml;
using System.Reflection;

using NLog;
using NLog.Config;

using NUnit.Framework;

#if !NUNIT
    using SetUp = Microsoft.VisualStudio.TestTools.UnitTesting.TestInitializeAttribute;
    using TestFixture = Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute;
    using Test = Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute;
    using TearDown =  Microsoft.VisualStudio.TestTools.UnitTesting.TestCleanupAttribute;
#endif

namespace NLog.UnitTests.LayoutRenderers
{
    [TestFixture]
    public class CounterTests : NLogTestBase
    {
        [Test]
        public void DefaultCounterTest()
        {
            LogManager.Configuration = CreateConfigurationFromString(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${message} ${counter} ${counter}' /></targets>
                <rules>
                    <logger name='*' minlevel='Info' writeTo='debug' />
                </rules>
            </nlog>");

            Logger logger = LogManager.GetLogger("A");
            logger.Debug("a");
            logger.Info("a");
            AssertDebugLastMessage("debug", "a 1 1");
            logger.Warn("a");
            AssertDebugLastMessage("debug", "a 2 2");
            logger.Error("a");
            AssertDebugLastMessage("debug", "a 3 3");
            logger.Fatal("a");
            AssertDebugLastMessage("debug", "a 4 4");
        }

        [Test]
        public void PresetCounterTest()
        {
            LogManager.Configuration = CreateConfigurationFromString(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${message} ${counter:value=1:increment=3} ${counter}' /></targets>
                <rules>
                    <logger name='*' minlevel='Info' writeTo='debug' />
                </rules>
            </nlog>");

            Logger logger = LogManager.GetLogger("A");
            logger.Debug("a");
            logger.Info("a");
            AssertDebugLastMessage("debug", "a 1 1");
            logger.Warn("a");
            AssertDebugLastMessage("debug", "a 4 2");
            logger.Error("a");
            AssertDebugLastMessage("debug", "a 7 3");
            logger.Fatal("a");
            AssertDebugLastMessage("debug", "a 10 4");
        }

        [Test]
        public void NamedCounterTest()
        {
            LogManager.Configuration = CreateConfigurationFromString(@"
            <nlog>
                <targets>
                    <target name='debug1' type='Debug' layout='${message} ${counter:sequence=aaa}' />
                    <target name='debug2' type='Debug' layout='${message} ${counter:sequence=bbb}' />
                    <target name='debug3' type='Debug' layout='${message} ${counter:sequence=aaa}' />
                </targets>
                <rules>
                    <logger name='debug1' minlevel='Debug' writeTo='debug1' />
                    <logger name='debug2' minlevel='Debug' writeTo='debug2' />
                    <logger name='debug3' minlevel='Debug' writeTo='debug3' />
                </rules>
            </nlog>");

            LogManager.GetLogger("debug1").Debug("a");
            AssertDebugLastMessage("debug1", "a 1");
            LogManager.GetLogger("debug2").Debug("a");
            AssertDebugLastMessage("debug2", "a 1");
            LogManager.GetLogger("debug3").Debug("a");
            AssertDebugLastMessage("debug3", "a 2");
        }
    }
}
