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
    using System;
    using System.Globalization;
    using System.Text;
    using NUnit.Framework;

#if !NUNIT
    using SetUp = Microsoft.VisualStudio.TestTools.UnitTesting.TestInitializeAttribute;
    using TestFixture = Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute;
    using Test = Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute;
    using TearDown =  Microsoft.VisualStudio.TestTools.UnitTesting.TestCleanupAttribute;
#endif
    using NLog.Conditions;
    using NLog.Config;
    using NLog.LayoutRenderers;
    using NLog.Layouts;
    using NLog.Targets;
    using NLog.Targets.Wrappers;

    [TestFixture]
    public class TargetConfigurationTests : NLogTestBase
    {
        [Test]
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
            Assert.IsInstanceOfType(typeof(MessageLayoutRenderer), l.Renderers[0]);
        }

        [Test]
        public void SimpleElementSyntaxTest()
        {
            LoggingConfiguration c = CreateConfigurationFromString(@"
            <nlog>
                <targets>
                    <target type='Debug'>
                        <name>d</name>
                        <layout>${message}</layout>
                    </target>
                </targets>
            </nlog>");

            DebugTarget t = c.FindTargetByName("d") as DebugTarget;
            Assert.IsNotNull(t);
            Assert.AreEqual(t.Name, "d");
            SimpleLayout l = t.Layout as SimpleLayout;
            Assert.AreEqual("${message}", l.Text);
            Assert.IsNotNull(t.Layout);
            Assert.AreEqual(1, l.Renderers.Count);
            Assert.IsInstanceOfType(typeof(MessageLayoutRenderer), l.Renderers[0]);
        }

        [Test]
        public void ArrayParameterTest()
        {
            LoggingConfiguration c = CreateConfigurationFromString(@"
            <nlog>
                <targets>
                    <target type='MethodCall' name='mct'>
                        <parameter name='p1' layout='${message}' />
                        <parameter name='p2' layout='${level}' />
                        <parameter name='p3' layout='${logger}' />
                    </target>
                </targets>
            </nlog>");

            var t = c.FindTargetByName("mct") as MethodCallTarget;
            Assert.IsNotNull(t);
            Assert.AreEqual(3, t.Parameters.Count);
            Assert.AreEqual("p1", t.Parameters[0].Name);
            Assert.AreEqual("'${message}'", t.Parameters[0].Layout.ToString());

            Assert.AreEqual("p2", t.Parameters[1].Name);
            Assert.AreEqual("'${level}'", t.Parameters[1].Layout.ToString());
            
            Assert.AreEqual("p3", t.Parameters[2].Name);
            Assert.AreEqual("'${logger}'", t.Parameters[2].Layout.ToString());
        }

        [Test]
        public void ArrayElementParameterTest()
        {
            LoggingConfiguration c = CreateConfigurationFromString(@"
            <nlog>
                <targets>
                    <target type='MethodCall' name='mct'>
                        <parameter>
                            <name>p1</name>
                            <layout>${message}</layout>
                        </parameter>
                        <parameter>
                            <name>p2</name>
                            <layout type='CsvLayout'>
                                <column name='x' layout='${message}' />
                                <column name='y' layout='${level}' />
                            </layout>
                        </parameter>
                        <parameter>
                            <name>p3</name>
                            <layout>${logger}</layout>
                        </parameter>
                    </target>
                </targets>
            </nlog>");

            var t = c.FindTargetByName("mct") as MethodCallTarget;
            Assert.IsNotNull(t);
            Assert.AreEqual(3, t.Parameters.Count);
            Assert.AreEqual("p1", t.Parameters[0].Name);
            Assert.AreEqual("'${message}'", t.Parameters[0].Layout.ToString());

            Assert.AreEqual("p2", t.Parameters[1].Name);
            CsvLayout csvLayout = t.Parameters[1].Layout as CsvLayout;
            Assert.IsNotNull(csvLayout);
            Assert.AreEqual(2, csvLayout.Columns.Count);
            Assert.AreEqual("x", csvLayout.Columns[0].Name);
            Assert.AreEqual("y", csvLayout.Columns[1].Name);

            Assert.AreEqual("p3", t.Parameters[2].Name);
            Assert.AreEqual("'${logger}'", t.Parameters[2].Layout.ToString());
        }

        [Test]
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
            Assert.IsInstanceOfType(typeof(MessageLayoutRenderer), l.Renderers[0]);
            Assert.IsInstanceOfType(typeof(LiteralLayoutRenderer), l.Renderers[1]);
            Assert.IsInstanceOfType(typeof(LevelLayoutRenderer), l.Renderers[2]);
            Assert.AreEqual(" ", ((LiteralLayoutRenderer)l.Renderers[1]).Text);
        }

        [Test]
        public void WrapperTest()
        {
            LoggingConfiguration c = CreateConfigurationFromString(@"
            <nlog>
                <targets>
                    <wrapper-target name='b' type='BufferingWrapper' bufferSize='19'>
                        <wrapper name='a' type='AsyncWrapper'>
                            <target name='c' type='Debug' layout='${message}' />
                        </wrapper>
                    </wrapper-target>
                </targets>
            </nlog>");

            Assert.IsNotNull(c.FindTargetByName("a"));
            Assert.IsNotNull(c.FindTargetByName("b"));
            Assert.IsNotNull(c.FindTargetByName("c"));

            Assert.IsInstanceOfType(typeof(BufferingTargetWrapper), c.FindTargetByName("b"));
            Assert.IsInstanceOfType(typeof(AsyncTargetWrapper), c.FindTargetByName("a"));
            Assert.IsInstanceOfType(typeof(DebugTarget), c.FindTargetByName("c"));

            BufferingTargetWrapper btw = c.FindTargetByName("b") as BufferingTargetWrapper;
            AsyncTargetWrapper atw = c.FindTargetByName("a") as AsyncTargetWrapper;
            DebugTarget dt = c.FindTargetByName("c") as DebugTarget;

            Assert.AreSame(atw, btw.WrappedTarget);
            Assert.AreSame(dt, atw.WrappedTarget);
            Assert.AreEqual(19, btw.BufferSize);
        }

        [Test]
        public void WrapperRefTest()
        {
            LoggingConfiguration c = CreateConfigurationFromString(@"
            <nlog>
                <targets>
                    <target name='c' type='Debug' layout='${message}' />

                    <wrapper name='a' type='AsyncWrapper'>
                        <target-ref name='c' />
                    </wrapper>

                    <wrapper-target name='b' type='BufferingWrapper' bufferSize='19'>
                        <wrapper-target-ref name='a' />
                    </wrapper-target>
                </targets>
            </nlog>");

            Assert.IsNotNull(c.FindTargetByName("a"));
            Assert.IsNotNull(c.FindTargetByName("b"));
            Assert.IsNotNull(c.FindTargetByName("c"));

            Assert.IsInstanceOfType(typeof(BufferingTargetWrapper), c.FindTargetByName("b"));
            Assert.IsInstanceOfType(typeof(AsyncTargetWrapper), c.FindTargetByName("a"));
            Assert.IsInstanceOfType(typeof(DebugTarget), c.FindTargetByName("c"));

            BufferingTargetWrapper btw = c.FindTargetByName("b") as BufferingTargetWrapper;
            AsyncTargetWrapper atw = c.FindTargetByName("a") as AsyncTargetWrapper;
            DebugTarget dt = c.FindTargetByName("c") as DebugTarget;

            Assert.AreSame(atw, btw.WrappedTarget);
            Assert.AreSame(dt, atw.WrappedTarget);
            Assert.AreEqual(19, btw.BufferSize);
        }

        [Test]
        public void CompoundTest()
        {
            LoggingConfiguration c = CreateConfigurationFromString(@"
            <nlog>
                <targets>
                    <compound-target name='rr' type='RoundRobinGroup'>
                        <target name='d1' type='Debug' layout='${message}1' />
                        <target name='d2' type='Debug' layout='${message}2' />
                        <target name='d3' type='Debug' layout='${message}3' />
                        <target name='d4' type='Debug' layout='${message}4' />
                    </compound-target>
                </targets>
            </nlog>");

            Assert.IsNotNull(c.FindTargetByName("rr"));
            Assert.IsNotNull(c.FindTargetByName("d1"));
            Assert.IsNotNull(c.FindTargetByName("d2"));
            Assert.IsNotNull(c.FindTargetByName("d3"));
            Assert.IsNotNull(c.FindTargetByName("d4"));

            Assert.IsInstanceOfType(typeof(RoundRobinGroupTarget), c.FindTargetByName("rr"));
            Assert.IsInstanceOfType(typeof(DebugTarget), c.FindTargetByName("d1"));
            Assert.IsInstanceOfType(typeof(DebugTarget), c.FindTargetByName("d2"));
            Assert.IsInstanceOfType(typeof(DebugTarget), c.FindTargetByName("d3"));
            Assert.IsInstanceOfType(typeof(DebugTarget), c.FindTargetByName("d4"));

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

        [Test]
        public void CompoundRefTest()
        {
            LoggingConfiguration c = CreateConfigurationFromString(@"
            <nlog>
                <targets>
                    <target name='d1' type='Debug' layout='${message}1' />
                    <target name='d2' type='Debug' layout='${message}2' />
                    <target name='d3' type='Debug' layout='${message}3' />
                    <target name='d4' type='Debug' layout='${message}4' />

                    <compound-target name='rr' type='RoundRobinGroup'>
                        <target-ref name='d1' />
                        <target-ref name='d2' />
                        <target-ref name='d3' />
                        <target-ref name='d4' />
                    </compound-target>
                </targets>
            </nlog>");

            Assert.IsNotNull(c.FindTargetByName("rr"));
            Assert.IsNotNull(c.FindTargetByName("d1"));
            Assert.IsNotNull(c.FindTargetByName("d2"));
            Assert.IsNotNull(c.FindTargetByName("d3"));
            Assert.IsNotNull(c.FindTargetByName("d4"));

            Assert.IsInstanceOfType(typeof(RoundRobinGroupTarget), c.FindTargetByName("rr"));
            Assert.IsInstanceOfType(typeof(DebugTarget), c.FindTargetByName("d1"));
            Assert.IsInstanceOfType(typeof(DebugTarget), c.FindTargetByName("d2"));
            Assert.IsInstanceOfType(typeof(DebugTarget), c.FindTargetByName("d3"));
            Assert.IsInstanceOfType(typeof(DebugTarget), c.FindTargetByName("d4"));

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

        [Test]
        public void AsyncWrappersTest()
        {
            LoggingConfiguration c = CreateConfigurationFromString(@"
            <nlog>
                <targets async='true'>
                    <target type='Debug' name='d' />
                    <target type='Debug' name='d2' />
                </targets>
            </nlog>");

            var t = c.FindTargetByName("d") as AsyncTargetWrapper;
            Assert.IsNotNull(t);
            Assert.AreEqual(t.Name, "d");

            var wrappedTarget = t.WrappedTarget as DebugTarget;
            Assert.IsNotNull(wrappedTarget);
            Assert.AreEqual("d_wrapped", wrappedTarget.Name);

            t = c.FindTargetByName("d2") as AsyncTargetWrapper;
            Assert.IsNotNull(t);
            Assert.AreEqual(t.Name, "d2");

            wrappedTarget = t.WrappedTarget as DebugTarget;
            Assert.IsNotNull(wrappedTarget);
            Assert.AreEqual("d2_wrapped", wrappedTarget.Name);
        }

        [Test]
        public void DefaultTargetParametersTest()
        {
            LoggingConfiguration c = CreateConfigurationFromString(@"
            <nlog>
                <targets>
                    <default-target-parameters type='Debug' layout='x${message}x' />
                    <target type='Debug' name='d' />
                    <target type='Debug' name='d2' />
                </targets>
            </nlog>");

            var t = c.FindTargetByName("d") as DebugTarget;
            Assert.IsNotNull(t);
            Assert.AreEqual("'x${message}x'", t.Layout.ToString());

            t = c.FindTargetByName("d2") as DebugTarget;
            Assert.IsNotNull(t);
            Assert.AreEqual("'x${message}x'", t.Layout.ToString());
        }

        [Test]
        public void DefaultWrapperTest()
        {
            LoggingConfiguration c = CreateConfigurationFromString(@"
            <nlog>
                <targets>
                    <default-wrapper type='BufferingWrapper'>
                        <wrapper type='RetryingWrapper' />
                    </default-wrapper>
                    <target type='Debug' name='d' layout='${level}' />
                    <target type='Debug' name='d2' layout='${level}' />
                </targets>
            </nlog>");

            var bufferingTargetWrapper = c.FindTargetByName("d") as BufferingTargetWrapper;
            Assert.IsNotNull(bufferingTargetWrapper);

            var retryingTargetWrapper = bufferingTargetWrapper.WrappedTarget as RetryingTargetWrapper;
            Assert.IsNotNull(retryingTargetWrapper);
            Assert.IsNull(retryingTargetWrapper.Name);

            var debugTarget = retryingTargetWrapper.WrappedTarget as DebugTarget;
            Assert.IsNotNull(debugTarget);
            Assert.AreEqual("d_wrapped", debugTarget.Name);
            Assert.AreEqual("'${level}'", debugTarget.Layout.ToString());
        }

        [Test]
        public void DataTypesTest()
        {
            LoggingConfiguration c = CreateConfigurationFromString(@"
            <nlog>
                <extensions>
                    <add type='" + typeof(MyTarget).AssemblyQualifiedName + @"' />
                </extensions>

                <targets>
                    <target type='MyTarget' name='myTarget'
                        byteProperty='42' 
                        int16Property='42' 
                        int32Property='42' 
                        int64Property='42000000000' 
                        stringProperty='foobar'
                        boolProperty='true'
                        doubleProperty='3.14159'
                        floatProperty='3.14159'
                        enumProperty='Value3'
                        flagsEnumProperty='Value1,Value3'
                        encodingProperty='utf-8'
                        cultureProperty='en-US'
                        typeProperty='System.Int32'
                        layoutProperty='${level}'
                        conditionProperty=""starts-with(message, 'x')""
                        />
                </targets>
            </nlog>");

            var myTarget = c.FindTargetByName("myTarget") as MyTarget;
            Assert.IsNotNull(myTarget);
            Assert.AreEqual((byte)42, myTarget.ByteProperty);
            Assert.AreEqual((short)42, myTarget.Int16Property);
            Assert.AreEqual(42, myTarget.Int32Property);
            Assert.AreEqual(42000000000L, myTarget.Int64Property);
            Assert.AreEqual("foobar", myTarget.StringProperty);
            Assert.AreEqual(true, myTarget.BoolProperty);
            Assert.AreEqual(3.14159, myTarget.DoubleProperty);
            Assert.AreEqual(3.14159f, myTarget.FloatProperty);
            Assert.AreEqual(MyEnum.Value3, myTarget.EnumProperty);
            Assert.AreEqual(MyFlagsEnum.Value1 | MyFlagsEnum.Value3, myTarget.FlagsEnumProperty);
            Assert.AreEqual(Encoding.UTF8, myTarget.EncodingProperty);
            Assert.AreEqual("en-US", myTarget.CultureProperty.Name);
            Assert.AreEqual(typeof(int), myTarget.TypeProperty);
            Assert.AreEqual("'${level}'", myTarget.LayoutProperty.ToString());
            Assert.AreEqual("starts-with(message, 'x')", myTarget.ConditionProperty.ToString());
        }

        [Test]
        public void NullableDataTypesTest()
        {
            LoggingConfiguration c = CreateConfigurationFromString(@"
            <nlog>
                <extensions>
                    <add type='" + typeof(MyNullableTarget).AssemblyQualifiedName + @"' />
                </extensions>

                <targets>
                    <target type='MyNullableTarget' name='myTarget'
                        byteProperty='42' 
                        int16Property='42' 
                        int32Property='42' 
                        int64Property='42000000000' 
                        stringProperty='foobar'
                        boolProperty='true'
                        doubleProperty='3.14159'
                        floatProperty='3.14159'
                        enumProperty='Value3'
                        flagsEnumProperty='Value1,Value3'
                        encodingProperty='utf-8'
                        cultureProperty='en-US'
                        typeProperty='System.Int32'
                        layoutProperty='${level}'
                        conditionProperty=""starts-with(message, 'x')""
                        />
                </targets>
            </nlog>");

            var myTarget = c.FindTargetByName("myTarget") as MyNullableTarget;
            Assert.IsNotNull(myTarget);
            Assert.AreEqual((byte)42, myTarget.ByteProperty);
            Assert.AreEqual((short)42, myTarget.Int16Property);
            Assert.AreEqual(42, myTarget.Int32Property);
            Assert.AreEqual(42000000000L, myTarget.Int64Property);
            Assert.AreEqual("foobar", myTarget.StringProperty);
            Assert.AreEqual(true, myTarget.BoolProperty);
            Assert.AreEqual(3.14159, myTarget.DoubleProperty);
            Assert.AreEqual(3.14159f, myTarget.FloatProperty);
            Assert.AreEqual(MyEnum.Value3, myTarget.EnumProperty);
            Assert.AreEqual(MyFlagsEnum.Value1 | MyFlagsEnum.Value3, myTarget.FlagsEnumProperty);
            Assert.AreEqual(Encoding.UTF8, myTarget.EncodingProperty);
            Assert.AreEqual("en-US", myTarget.CultureProperty.Name);
            Assert.AreEqual(typeof(int), myTarget.TypeProperty);
            Assert.AreEqual("'${level}'", myTarget.LayoutProperty.ToString());
            Assert.AreEqual("starts-with(message, 'x')", myTarget.ConditionProperty.ToString());
        }

        [Target("MyTarget")]
        public class MyTarget : Target
        {
            public byte ByteProperty { get; set; }

            public short Int16Property { get; set; }

            public int Int32Property { get; set; }

            public long Int64Property { get; set; }

            public string StringProperty { get; set; }

            public bool BoolProperty { get; set; }

            public double DoubleProperty { get; set; }

            public float FloatProperty { get; set; }

            public MyEnum EnumProperty { get; set; }

            public MyFlagsEnum FlagsEnumProperty { get; set; }

            public Encoding EncodingProperty { get; set; }

            public CultureInfo CultureProperty { get; set; }

            public Type TypeProperty { get; set; }

            public Layout LayoutProperty { get; set; }

            public ConditionExpression ConditionProperty { get; set; }
        }


        [Target("MyNullableTarget")]
        public class MyNullableTarget : Target
        {
            public byte? ByteProperty { get; set; }

            public short? Int16Property { get; set; }

            public int? Int32Property { get; set; }

            public long? Int64Property { get; set; }

            public string StringProperty { get; set; }

            public bool? BoolProperty { get; set; }

            public double? DoubleProperty { get; set; }

            public float? FloatProperty { get; set; }

            public MyEnum? EnumProperty { get; set; }

            public MyFlagsEnum? FlagsEnumProperty { get; set; }

            public Encoding EncodingProperty { get; set; }

            public CultureInfo CultureProperty { get; set; }

            public Type TypeProperty { get; set; }

            public Layout LayoutProperty { get; set; }

            public ConditionExpression ConditionProperty { get; set; }
        }

        public enum MyEnum
        {
            Value1,

            Value2,

            Value3,
        }

        [Flags]
        public enum MyFlagsEnum
        {
            Value1 = 1,

            Value2 = 2,

            Value3 = 4,
        }
    }
}