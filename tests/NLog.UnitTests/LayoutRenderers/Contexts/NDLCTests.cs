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

namespace NLog.UnitTests.LayoutRenderers
{
    using System;
    using NLog.Config;
    using Xunit;

    [Obsolete("Replaced by ScopeContext.PushNestedState or Logger.PushScopeState using ${scopenested}. Marked obsolete on NLog 5.0")]
    public class NDLCTests : NLogTestBase
    {
        [Fact]
        public void NdlcGetAllMessages()
        {
            object value = 5;

            NestedDiagnosticsLogicalContext.Clear();
            var popper = NestedDiagnosticsLogicalContext.Push(value);

            string expected = "5";
            string[] actual = NestedDiagnosticsLogicalContext.GetAllMessages();
            Assert.Single(actual);
            Assert.Equal(expected, actual[0]);

            popper.Dispose();
            actual = NestedDiagnosticsLogicalContext.GetAllMessages();
            Assert.Empty(actual);
        }

        [Fact]
        public void NDLCTest()
        {
            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
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
            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
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
            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
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
                        Assert.Equal("kopytko", NestedDiagnosticsLogicalContext.PopObject()); // manual pop
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
            Assert.Null(NestedDiagnosticsLogicalContext.Pop()); //inconsistent with NDC - should be string.empty, but for backwardsscomp. Fix in NLog 5
            NestedDiagnosticsLogicalContext.Push("zzz");
            NestedDiagnosticsLogicalContext.Push("yyy");
            Assert.Equal("yyy", NestedDiagnosticsLogicalContext.Pop());
            NestedDiagnosticsLogicalContext.Clear();
            Assert.Null(NestedDiagnosticsLogicalContext.Pop()); //inconsistent with NDC - should be string.empty, but for backwardsscomp. Fix in NLog 5
        }

        [Fact]
        public void NDLCBottomTest()
        {
            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
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
            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
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
            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
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

            NestedDiagnosticsLogicalContext.PopObject();
            LogManager.GetLogger("A").Debug("1");
            AssertDebugLastMessage("debug", "99 1");

            NestedDiagnosticsLogicalContext.Clear();
            LogManager.GetLogger("A").Debug("2");
            AssertDebugLastMessage("debug", " 2");
        }

#if !NET35 && !NET40 && !NET45
        [Fact]
        public void NDLCTimingTest()
        {
            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${ndlc}|${ndlctiming:CurrentScope=false:ScopeBeginTime=true:Format=yyyy-MM-dd HH\:mm\:ss}|${ndlctiming:CurrentScope=false:ScopeBeginTime=false:Format=fff}|${ndlctiming:CurrentScope=true:ScopeBeginTime=true:Format=HH\:mm\:ss.fff}|${ndlctiming:CurrentScope=true:ScopeBeginTime=false:Format=fffffff}|${message}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>");

            NestedDiagnosticsLogicalContext.Clear();
            LogManager.GetLogger("A").Debug("0");
            AssertDebugLastMessage("debug", "|||||0");
            using (NestedDiagnosticsLogicalContext.Push("ala"))
            {
                LogManager.GetLogger("A").Debug("a");
                var measurements = GetDebugLastMessage("debug").Split(new[] { '|' }, System.StringSplitOptions.RemoveEmptyEntries);
                Assert.Equal(6, measurements.Length);
                Assert.Equal("ala", measurements[0]);
                Assert.InRange(int.Parse(measurements[2]), 0, 999);
                Assert.InRange(int.Parse(measurements[4]), 0, 9999999);
                Assert.Equal("a", measurements[measurements.Length-1]);

                System.Threading.Thread.Sleep(10);

                LogManager.GetLogger("A").Debug("b");
                measurements = GetDebugLastMessage("debug").Split(new[] { '|' }, System.StringSplitOptions.RemoveEmptyEntries);
                Assert.Equal("ala", measurements[0]);
                Assert.InRange(int.Parse(measurements[2]), 10, 999);
                Assert.InRange(int.Parse(measurements[4]), 100000, 9999999);
                Assert.Equal("b", measurements[measurements.Length - 1]);

                using (NestedDiagnosticsLogicalContext.Push("ma"))
                {
                    LogManager.GetLogger("A").Debug("a");
                    measurements = GetDebugLastMessage("debug").Split(new[] { '|' }, System.StringSplitOptions.RemoveEmptyEntries);
                    Assert.Equal(6, measurements.Length);
                    Assert.Equal("ala ma", measurements[0]);
                    Assert.InRange(int.Parse(measurements[2]), 10, 999);
                    Assert.InRange(int.Parse(measurements[4]), 0, 9999999);
                    Assert.Equal("a", measurements[measurements.Length - 1]);

                    System.Threading.Thread.Sleep(10);

                    LogManager.GetLogger("A").Debug("b");
                    measurements = GetDebugLastMessage("debug").Split(new[] { '|' }, System.StringSplitOptions.RemoveEmptyEntries);
                    Assert.Equal(6, measurements.Length);
                    Assert.Equal("ala ma", measurements[0]);
                    Assert.InRange(int.Parse(measurements[2]), 20, 999);
                    Assert.InRange(int.Parse(measurements[4]), 100000, 9999999);
                    Assert.Equal("b", measurements[measurements.Length - 1]);
                }

                LogManager.GetLogger("A").Debug("c");
                measurements = GetDebugLastMessage("debug").Split(new[] { '|' }, System.StringSplitOptions.RemoveEmptyEntries);
                Assert.Equal("ala", measurements[0]);
                Assert.InRange(int.Parse(measurements[2]), 20, 999);
                Assert.InRange(int.Parse(measurements[4]), 200000, 9999999);
                Assert.Equal("c", measurements[measurements.Length - 1]);
            }

            LogManager.GetLogger("A").Debug("0");
            AssertDebugLastMessage("debug", "|||||0");
        }
#endif

        [Fact]
        public void NDLCAsyncLogging()
        {
            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${ndlc:separator=\:} ${message}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>");

            System.Threading.Tasks.Task task;
            using (NestedDiagnosticsLogicalContext.Push("ala"))
            {
                LogManager.GetLogger("A").Debug("a");
                AssertDebugLastMessage("debug", "ala a");
                task = System.Threading.Tasks.Task.Run(async () => { await System.Threading.Tasks.Task.Delay(50); LogManager.GetLogger("B").Debug("b"); });
            }
            task.Wait();
            AssertDebugLastMessage("debug", "ala b");
        }
    }
}