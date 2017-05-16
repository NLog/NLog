// 
// Copyright (c) 2004-2016 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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
#if NET4_0 || NET4_5
    using Xunit;

    public class NDLCTests : NLogTestBase
    {
        [Fact]
        public void NDLCTest()
        {
            LogManager.Configuration = CreateConfigurationFromString(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${ndlc} ${message}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>");

            NestedDiagnosticsLogicalContext.Clear();
            LogManager.GetLogger("A").Debug("0");
            AssertDebugLastMessage("debug", " 0");
            using (NestedDiagnosticsLogicalContext.Push("ala"))
            {
                LogManager.GetLogger("A").Debug("a");
                AssertDebugLastMessage("debug", "ala a");
                using (NestedDiagnosticsLogicalContext.Push("ma"))
                {
                    LogManager.GetLogger("A").Debug("b");
                    AssertDebugLastMessage("debug", "ala ma b");
                    using (NestedDiagnosticsLogicalContext.Push("kota"))
                    {
                        LogManager.GetLogger("A").Debug("c");
                        AssertDebugLastMessage("debug", "ala ma kota c");
                        using (NestedDiagnosticsLogicalContext.Push("kopytko"))
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

        [Fact]
        public void NDLCTopTestTest()
        {
            LogManager.Configuration = CreateConfigurationFromString(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${ndlc:topframes=2} ${message}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>");

            NestedDiagnosticsLogicalContext.Clear();
            LogManager.GetLogger("A").Debug("0");
            AssertDebugLastMessage("debug", " 0");
            using (NestedDiagnosticsLogicalContext.Push("ala"))
            {
                LogManager.GetLogger("A").Debug("a");
                AssertDebugLastMessage("debug", "ala a");
                using (NestedDiagnosticsLogicalContext.Push("ma"))
                {
                    LogManager.GetLogger("A").Debug("b");
                    AssertDebugLastMessage("debug", "ala ma b");
                    using (NestedDiagnosticsLogicalContext.Push("kota"))
                    {
                        LogManager.GetLogger("A").Debug("c");
                        AssertDebugLastMessage("debug", "ma kota c");
                        using (NestedDiagnosticsLogicalContext.Push("kopytko"))
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


        [Fact]
        public void NDLCTop1TestTest()
        {
            LogManager.Configuration = CreateConfigurationFromString(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${ndlc:topframes=1} ${message}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>");

            NestedDiagnosticsLogicalContext.Clear();
            LogManager.GetLogger("A").Debug("0");
            AssertDebugLastMessage("debug", " 0");
            using (NestedDiagnosticsLogicalContext.Push("ala"))
            {
                LogManager.GetLogger("A").Debug("a");
                AssertDebugLastMessage("debug", "ala a");
                using (NestedDiagnosticsLogicalContext.Push("ma"))
                {
                    LogManager.GetLogger("A").Debug("b");
                    AssertDebugLastMessage("debug", "ma b");
                    using (NestedDiagnosticsLogicalContext.Push("kota"))
                    {
                        LogManager.GetLogger("A").Debug("c");
                        AssertDebugLastMessage("debug", "kota c");
                        NestedDiagnosticsLogicalContext.Push("kopytko");
                        LogManager.GetLogger("A").Debug("d");
                        AssertDebugLastMessage("debug", "kopytko d");
                        Assert.Equal("kopytko", NestedDiagnosticsLogicalContext.Pop()); // manual pop
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
            Assert.Equal(string.Empty, NestedDiagnosticsLogicalContext.Pop());
            NestedDiagnosticsLogicalContext.Push("zzz");
            NestedDiagnosticsLogicalContext.Push("yyy");
            Assert.Equal("yyy", NestedDiagnosticsLogicalContext.Pop());
            NestedDiagnosticsLogicalContext.Clear();
            Assert.Equal(string.Empty, NestedDiagnosticsLogicalContext.Pop());
        }

        [Fact]
        public void NDLCBottomTest()
        {
            LogManager.Configuration = CreateConfigurationFromString(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${ndlc:bottomframes=2} ${message}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>");

            NestedDiagnosticsLogicalContext.Clear();
            LogManager.GetLogger("A").Debug("0");
            AssertDebugLastMessage("debug", " 0");
            using (NestedDiagnosticsLogicalContext.Push("ala"))
            {
                LogManager.GetLogger("A").Debug("a");
                AssertDebugLastMessage("debug", "ala a");
                using (NestedDiagnosticsLogicalContext.Push("ma"))
                {
                    LogManager.GetLogger("A").Debug("b");
                    AssertDebugLastMessage("debug", "ala ma b");
                    using (NestedDiagnosticsLogicalContext.Push("kota"))
                    {
                        LogManager.GetLogger("A").Debug("c");
                        AssertDebugLastMessage("debug", "ala ma c");
                        using (NestedDiagnosticsLogicalContext.Push("kopytko"))
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

        [Fact]
        public void NDLCSeparatorTest()
        {
            LogManager.Configuration = CreateConfigurationFromString(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${ndlc:separator=\:} ${message}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>");

            NestedDiagnosticsLogicalContext.Clear();
            LogManager.GetLogger("A").Debug("0");
            AssertDebugLastMessage("debug", " 0");
            using (NestedDiagnosticsLogicalContext.Push("ala"))
            {
                LogManager.GetLogger("A").Debug("a");
                AssertDebugLastMessage("debug", "ala a");
                using (NestedDiagnosticsLogicalContext.Push("ma"))
                {
                    LogManager.GetLogger("A").Debug("b");
                    AssertDebugLastMessage("debug", "ala:ma b");
                    using (NestedDiagnosticsLogicalContext.Push("kota"))
                    {
                        LogManager.GetLogger("A").Debug("c");
                        AssertDebugLastMessage("debug", "ala:ma:kota c");
                        using (NestedDiagnosticsLogicalContext.Push("kopytko"))
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

        [Fact]
        public void NDLCDeepTest()
        {
            LogManager.Configuration = CreateConfigurationFromString(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${ndlc:topframes=1} ${message}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>");

            NestedDiagnosticsLogicalContext.Clear();

            for (int i = 1; i <= 100; ++i)
                NestedDiagnosticsLogicalContext.Push(i);

            LogManager.GetLogger("A").Debug("0");
            AssertDebugLastMessage("debug", "100 0");

            NestedDiagnosticsLogicalContext.Pop();
            LogManager.GetLogger("A").Debug("1");
            AssertDebugLastMessage("debug", "99 1");

            NestedDiagnosticsLogicalContext.Clear();
            LogManager.GetLogger("A").Debug("2");
            AssertDebugLastMessage("debug", " 2");
        }
    }
#endif
}