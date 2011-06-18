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
using System.Diagnostics;
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
using NLog.LayoutRenderers;
using NLog.Targets;
using NLog.Layouts;
using NLog.LayoutRenderers.Wrappers;

namespace NLog.UnitTests.LayoutRenderers
{
    [TestFixture]
    public class Rot13Tests : NLogTestBase
    {
        [Test]
        public void Test1()
        {
            Assert.AreEqual("NOPQRSTUVWXYZABCDEFGHIJKLM",
                    Rot13LayoutRendererWrapper.DecodeRot13("ABCDEFGHIJKLMNOPQRSTUVWXYZ"));
            Assert.AreEqual("nopqrstuvwxyzabcdefghijklm0123456789",
                    Rot13LayoutRendererWrapper.DecodeRot13("abcdefghijklmnopqrstuvwxyz0123456789"));
            Assert.AreEqual("How can you tell an extrovert from an introvert at NSA? Va gur ryringbef, gur rkgebiregf ybbx ng gur BGURE thl'f fubrf.",
            Rot13LayoutRendererWrapper.DecodeRot13(
                            "Ubj pna lbh gryy na rkgebireg sebz na vagebireg ng AFN? In the elevators, the extroverts look at the OTHER guy's shoes."));
        }

        [Test]
        public void Test2()
        {
            Layout l = "${rot13:HELLO}";
            LogEventInfo lei = LogEventInfo.Create(LogLevel.Info, "aaa", "bbb");
            Assert.AreEqual("URYYB", l.Render(lei));
        }

        [Test]
        public void Test3()
        {
            Layout l = "${rot13:text=HELLO}";
            LogEventInfo lei = LogEventInfo.Create(LogLevel.Info, "aaa", "bbb");
            Assert.AreEqual("URYYB", l.Render(lei));
        }

        [Test]
        public void Test4()
        {
            Layout l = "${rot13:${event-context:aaa}}";
            LogEventInfo lei = LogEventInfo.Create(LogLevel.Info, "aaa", "bbb");
            lei.Properties["aaa"] = "HELLO";
            Assert.AreEqual("URYYB", l.Render(lei));
        }

        [Test]
        public void Test5()
        {
            LogManager.Configuration = CreateConfigurationFromString(@"
                <nlog>
                    <targets>
                        <target name='debug' type='Debug' layout='${rot13:${mdc:A}}' />
                        <target name='debug2' type='Debug' layout='${rot13:${rot13:${mdc:A}}}' />
                     </targets>
                    <rules>
                        <logger name='*' levels='Trace' writeTo='debug,debug2' />
                    </rules>
                </nlog>");

            MappedDiagnosticsContext.Set("A", "Foo.Bar!");
            Logger l = LogManager.GetLogger("NLog.UnitTests.LayoutRenderers.Rot13Tests");
            l.Trace("aaa");

            AssertDebugLastMessage("debug", "Sbb.One!");

            // double rot-13 should be identity
            AssertDebugLastMessage("debug2", "Foo.Bar!");
        }
    }
}