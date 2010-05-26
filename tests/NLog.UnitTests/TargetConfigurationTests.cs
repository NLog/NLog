// 
// Copyright (c) 2004-2010 Jaroslaw Kowalski <jaak@jkowalski.net>
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

using System.Xml;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NLog.Config;
using NLog.LayoutRenderers;
using NLog.Layouts;
using NLog.Targets;
using NLog.Targets.Compound;
using NLog.Targets.Wrappers;

namespace NLog.UnitTests
{
    [TestClass]
    public class TargetConfigurationTests : NLogTestBase
    {
        [TestMethod]
        public void SimpleTest()
        {
            LoggingConfiguration c = CreateConfigurationFromString(@"
            <nlog>
                <targets>
                    <target name='d' type='Debug' layout='${message}' />
                </targets>
            </nlog>");

            DebugTarget t = c.FindTargetByName("d") as DebugTarget;
            Assert.IsNotNull(t);
            Assert.AreEqual(t.Name, "d");
            SimpleLayout l = t.Layout as SimpleLayout;
            Assert.AreEqual("${message}", l.Text);
            Assert.IsNotNull(t.Layout);
            Assert.AreEqual(1, l.Renderers.Count);
            Assert.IsInstanceOfType(l.Renderers[0], typeof(MessageLayoutRenderer));
        }

        [TestMethod]
        public void SimpleTest2()
        {
            LoggingConfiguration c = CreateConfigurationFromString(@"
            <nlog>
                <targets>
                    <target name='d' type='Debug' layout='${message} ${level}' />
                </targets>
            </nlog>");

            DebugTarget t = c.FindTargetByName("d") as DebugTarget;
            Assert.IsNotNull(t);
            Assert.AreEqual(t.Name, "d");
            SimpleLayout l = t.Layout as SimpleLayout;
            Assert.AreEqual("${message} ${level}", l.Text);
            Assert.IsNotNull(l);
            Assert.AreEqual(3, l.Renderers.Count);
            Assert.IsInstanceOfType(l.Renderers[0], typeof(MessageLayoutRenderer));
            Assert.IsInstanceOfType(l.Renderers[1], typeof(LiteralLayoutRenderer));
            Assert.IsInstanceOfType(l.Renderers[2], typeof(LevelLayoutRenderer));
            Assert.AreEqual(" ", ((LiteralLayoutRenderer)l.Renderers[1]).Text);
        }

        [TestMethod]
        public void WrapperTest()
        {
            LoggingConfiguration c = CreateConfigurationFromString(@"
            <nlog>
                <targets>
                    <target name='b' type='BufferingWrapper' bufferSize='19'>
                        <target name='a' type='AsyncWrapper'>
                            <target name='c' type='Debug' layout='${message}' />
                        </target>
                    </target>
                </targets>
            </nlog>");

            Assert.IsNotNull(c.FindTargetByName("a"));
            Assert.IsNotNull(c.FindTargetByName("b"));
            Assert.IsNotNull(c.FindTargetByName("c"));

            Assert.IsInstanceOfType(c.FindTargetByName("b"), typeof(BufferingTargetWrapper));
            Assert.IsInstanceOfType(c.FindTargetByName("a"), typeof(AsyncTargetWrapper));
            Assert.IsInstanceOfType(c.FindTargetByName("c"), typeof(DebugTarget));

            BufferingTargetWrapper btw = c.FindTargetByName("b") as BufferingTargetWrapper;
            AsyncTargetWrapper atw = c.FindTargetByName("a") as AsyncTargetWrapper;
            DebugTarget dt = c.FindTargetByName("c") as DebugTarget;

            Assert.AreSame(atw, btw.WrappedTarget);
            Assert.AreSame(dt, atw.WrappedTarget);
            Assert.AreEqual(19, btw.BufferSize);
        }

        [TestMethod]
        public void CompoundTest()
        {
            LoggingConfiguration c = CreateConfigurationFromString(@"
            <nlog>
                <targets>
                    <target name='rr' type='RoundRobinGroup'>
                        <target name='d1' type='Debug' layout='${message}1' />
                        <target name='d2' type='Debug' layout='${message}2' />
                        <target name='d3' type='Debug' layout='${message}3' />
                        <target name='d4' type='Debug' layout='${message}4' />
                    </target>
                </targets>
            </nlog>");

            Assert.IsNotNull(c.FindTargetByName("rr"));
            Assert.IsNotNull(c.FindTargetByName("d1"));
            Assert.IsNotNull(c.FindTargetByName("d2"));
            Assert.IsNotNull(c.FindTargetByName("d3"));
            Assert.IsNotNull(c.FindTargetByName("d4"));

            Assert.IsInstanceOfType(c.FindTargetByName("rr"), typeof(RoundRobinGroupTarget));
            Assert.IsInstanceOfType(c.FindTargetByName("d1"), typeof(DebugTarget));
            Assert.IsInstanceOfType(c.FindTargetByName("d2"), typeof(DebugTarget));
            Assert.IsInstanceOfType(c.FindTargetByName("d3"), typeof(DebugTarget));
            Assert.IsInstanceOfType(c.FindTargetByName("d4"), typeof(DebugTarget));

            RoundRobinGroupTarget rr = c.FindTargetByName("rr") as RoundRobinGroupTarget;
            DebugTarget d1 = c.FindTargetByName("d1") as DebugTarget;
            DebugTarget d2 = c.FindTargetByName("d2") as DebugTarget;
            DebugTarget d3 = c.FindTargetByName("d3") as DebugTarget;
            DebugTarget d4 = c.FindTargetByName("d4") as DebugTarget;

            Assert.AreEqual(4, rr.Targets.Count);
            Assert.AreSame(d1, rr.Targets[0]);
            Assert.AreSame(d2, rr.Targets[1]);
            Assert.AreSame(d3, rr.Targets[2]);
            Assert.AreSame(d4, rr.Targets[3]);

            Assert.AreEqual(((SimpleLayout)d1.Layout).Text, "${message}1");
            Assert.AreEqual(((SimpleLayout)d2.Layout).Text, "${message}2");
            Assert.AreEqual(((SimpleLayout)d3.Layout).Text, "${message}3");
            Assert.AreEqual(((SimpleLayout)d4.Layout).Text, "${message}4");
        }
    }
}