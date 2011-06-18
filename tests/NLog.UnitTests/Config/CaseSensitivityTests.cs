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

namespace NLog.UnitTests.Config
{
    using NUnit.Framework;

#if !NUNIT
    using SetUp = Microsoft.VisualStudio.TestTools.UnitTesting.TestInitializeAttribute;
    using TestFixture = Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute;
    using Test = Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute;
    using TearDown =  Microsoft.VisualStudio.TestTools.UnitTesting.TestCleanupAttribute;
#endif

    [TestFixture]
    public class CaseSensitivityTests : NLogTestBase
    {
        [Test]
        public void LowerCaseTest()
        {
            LogManager.Configuration = CreateConfigurationFromString(@"
            <nlog>
                <targets><target name='debug' type='debug' layout='${message}' /></targets>
                <rules>
                    <logger name='*' minlevel='info' appendto='debug'>
                        <filters>
                            <whencontains layout='${message}' substring='msg' action='ignore' />
                        </filters>
                    </logger>
                </rules>
            </nlog>");

            Logger logger = LogManager.GetLogger("A");
            logger.Debug("msg");
            logger.Info("msg");
            logger.Warn("msg");
            logger.Error("msg");
            logger.Fatal("msg");
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
        public void UpperCaseTest()
        {
            LogManager.Configuration = CreateConfigurationFromString(@"
            <nlog throwExceptions='true'>
                <TARGETS><TARGET NAME='DEBUG' TYPE='DEBUG' LAYOUT='${MESSAGE}' /></TARGETS>
                <RULES>
                    <LOGGER NAME='*' MINLEVEL='INFO' APPENDTO='DEBUG'>
                        <FILTERS>
                            <WHENCONTAINS LAYOUT='${MESSAGE}' SUBSTRING='msg' ACTION='IGNORE' />
                        </FILTERS>
                    </LOGGER>
                </RULES>
            </nlog>");

            Logger logger = LogManager.GetLogger("A");
            logger.Debug("msg");
            logger.Info("msg");
            logger.Warn("msg");
            logger.Error("msg");
            logger.Fatal("msg");
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
    }
}