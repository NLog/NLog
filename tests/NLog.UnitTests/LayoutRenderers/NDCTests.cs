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
    public class NDCTests : NLogTestBase
    {
        [Test]
        public void NDCTest()
        {
            LogManager.Configuration = CreateConfigurationFromString(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${ndc} ${message}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>");

            NestedDiagnosticsContext.Clear();
            LogManager.GetLogger("A").Debug("0");
            AssertDebugLastMessage("debug", " 0");
            using (NestedDiagnosticsContext.Push("ala"))
            {
                LogManager.GetLogger("A").Debug("a");
                AssertDebugLastMessage("debug", "ala a");
                using (NestedDiagnosticsContext.Push("ma"))
                {
                    LogManager.GetLogger("A").Debug("b");
                    AssertDebugLastMessage("debug", "ala ma b");
                    using (NestedDiagnosticsContext.Push("kota"))
                    {
                        LogManager.GetLogger("A").Debug("c");
                        AssertDebugLastMessage("debug", "ala ma kota c");
                        using (NestedDiagnosticsContext.Push("kopytko"))
                        {
                            LogManager.GetLogger("A").Debug("d");
                            AssertDebugLastMessage("debug", "ala ma kota kopytko d");
                        }
                        LogManager.GetLogger("A").Debug("c");
                        AssertDebugLastMessage("debug", "ala ma kota c");
                    }
                    LogManager.GetLogger("A").Debug("b");
                    AssertDebugLastMessage("debug", "ala ma b");
                }
                LogManager.GetLogger("A").Debug("a");
                AssertDebugLastMessage("debug", "ala a");
            }
            LogManager.GetLogger("A").Debug("0");
            AssertDebugLastMessage("debug", " 0");
        }

        [Test]
        public void NDCTopTestTest()
        {
            LogManager.Configuration = CreateConfigurationFromString(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${ndc:topframes=2} ${message}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>");

            NestedDiagnosticsContext.Clear();
            LogManager.GetLogger("A").Debug("0");
            AssertDebugLastMessage("debug", " 0");
            using (NestedDiagnosticsContext.Push("ala"))
            {
                LogManager.GetLogger("A").Debug("a");
                AssertDebugLastMessage("debug", "ala a");
                using (NestedDiagnosticsContext.Push("ma"))
                {
                    LogManager.GetLogger("A").Debug("b");
                    AssertDebugLastMessage("debug", "ala ma b");
                    using (NestedDiagnosticsContext.Push("kota"))
                    {
                        LogManager.GetLogger("A").Debug("c");
                        AssertDebugLastMessage("debug", "ma kota c");
                        using (NestedDiagnosticsContext.Push("kopytko"))
                        {
                            LogManager.GetLogger("A").Debug("d");
                            AssertDebugLastMessage("debug", "kota kopytko d");
                        }
                        LogManager.GetLogger("A").Debug("c");
                        AssertDebugLastMessage("debug", "ma kota c");
                    }
                    LogManager.GetLogger("A").Debug("b");
                    AssertDebugLastMessage("debug", "ala ma b");
                }
                LogManager.GetLogger("A").Debug("a");
                AssertDebugLastMessage("debug", "ala a");
            }
            LogManager.GetLogger("A").Debug("0");
            AssertDebugLastMessage("debug", " 0");
        }


        [Test]
        public void NDCTop1TestTest()
        {
            LogManager.Configuration = CreateConfigurationFromString(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${ndc:topframes=1} ${message}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>");

            NestedDiagnosticsContext.Clear();
            LogManager.GetLogger("A").Debug("0");
            AssertDebugLastMessage("debug", " 0");
            using (NestedDiagnosticsContext.Push("ala"))
            {
                LogManager.GetLogger("A").Debug("a");
                AssertDebugLastMessage("debug", "ala a");
                using (NestedDiagnosticsContext.Push("ma"))
                {
                    LogManager.GetLogger("A").Debug("b");
                    AssertDebugLastMessage("debug", "ma b");
                    using (NestedDiagnosticsContext.Push("kota"))
                    {
                        LogManager.GetLogger("A").Debug("c");
                        AssertDebugLastMessage("debug", "kota c");
                        NestedDiagnosticsContext.Push("kopytko");
                        LogManager.GetLogger("A").Debug("d");
                        AssertDebugLastMessage("debug", "kopytko d");
                        Assert.AreEqual("kopytko", NestedDiagnosticsContext.Pop()); // manual pop
                        LogManager.GetLogger("A").Debug("c");
                        AssertDebugLastMessage("debug", "kota c");
                    }
                    LogManager.GetLogger("A").Debug("b");
                    AssertDebugLastMessage("debug", "ma b");
                }
                LogManager.GetLogger("A").Debug("a");
                AssertDebugLastMessage("debug", "ala a");
            }
            LogManager.GetLogger("A").Debug("0");
            AssertDebugLastMessage("debug", " 0");
            Assert.AreEqual(String.Empty, NestedDiagnosticsContext.Pop());
            Assert.AreEqual(String.Empty, NestedDiagnosticsContext.TopMessage);
            NestedDiagnosticsContext.Push("zzz");
            Assert.AreEqual("zzz", NestedDiagnosticsContext.TopMessage);
            NestedDiagnosticsContext.Clear();
            Assert.AreEqual(String.Empty, NestedDiagnosticsContext.Pop());
            Assert.AreEqual(String.Empty, NestedDiagnosticsContext.TopMessage);
        }

        [Test]
        public void NDCBottomTest()
        {
            LogManager.Configuration = CreateConfigurationFromString(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${ndc:bottomframes=2} ${message}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>");

            NestedDiagnosticsContext.Clear();
            LogManager.GetLogger("A").Debug("0");
            AssertDebugLastMessage("debug", " 0");
            using (NestedDiagnosticsContext.Push("ala"))
            {
                LogManager.GetLogger("A").Debug("a");
                AssertDebugLastMessage("debug", "ala a");
                using (NestedDiagnosticsContext.Push("ma"))
                {
                    LogManager.GetLogger("A").Debug("b");
                    AssertDebugLastMessage("debug", "ala ma b");
                    using (NestedDiagnosticsContext.Push("kota"))
                    {
                        LogManager.GetLogger("A").Debug("c");
                        AssertDebugLastMessage("debug", "ala ma c");
                        using (NestedDiagnosticsContext.Push("kopytko"))
                        {
                            LogManager.GetLogger("A").Debug("d");
                            AssertDebugLastMessage("debug", "ala ma d");
                        }
                        LogManager.GetLogger("A").Debug("c");
                        AssertDebugLastMessage("debug", "ala ma c");
                    }
                    LogManager.GetLogger("A").Debug("b");
                    AssertDebugLastMessage("debug", "ala ma b");
                }
                LogManager.GetLogger("A").Debug("a");
                AssertDebugLastMessage("debug", "ala a");
            }
            LogManager.GetLogger("A").Debug("0");
            AssertDebugLastMessage("debug", " 0");
        }

        [Test]
        public void NDCSeparatorTest()
        {
            LogManager.Configuration = CreateConfigurationFromString(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${ndc:separator=\:} ${message}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>");

            NestedDiagnosticsContext.Clear();
            LogManager.GetLogger("A").Debug("0");
            AssertDebugLastMessage("debug", " 0");
            using (NestedDiagnosticsContext.Push("ala"))
            {
                LogManager.GetLogger("A").Debug("a");
                AssertDebugLastMessage("debug", "ala a");
                using (NestedDiagnosticsContext.Push("ma"))
                {
                    LogManager.GetLogger("A").Debug("b");
                    AssertDebugLastMessage("debug", "ala:ma b");
                    using (NestedDiagnosticsContext.Push("kota"))
                    {
                        LogManager.GetLogger("A").Debug("c");
                        AssertDebugLastMessage("debug", "ala:ma:kota c");
                        using (NestedDiagnosticsContext.Push("kopytko"))
                        {
                            LogManager.GetLogger("A").Debug("d");
                            AssertDebugLastMessage("debug", "ala:ma:kota:kopytko d");
                        }
                        LogManager.GetLogger("A").Debug("c");
                        AssertDebugLastMessage("debug", "ala:ma:kota c");
                    }
                    LogManager.GetLogger("A").Debug("b");
                    AssertDebugLastMessage("debug", "ala:ma b");
                }
                LogManager.GetLogger("A").Debug("a");
                AssertDebugLastMessage("debug", "ala a");
            }
            LogManager.GetLogger("A").Debug("0");
            AssertDebugLastMessage("debug", " 0");
        }

    }
}