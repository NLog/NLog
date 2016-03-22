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
    using NLog.Conditions;
    using NLog.Config;
    using NLog.LayoutRenderers;
    using NLog.Layouts;
    using NLog.Targets;
    using NLog.Targets.Wrappers;
    using Xunit;

    public class TargetConfigurationTests : NLogTestBase
    {
        [Fact]
        public void SimpleTest()
        {
            LoggingConfiguration c = CreateConfigurationFromString(@"
            <nlog>
                <targets>
                    <target name='d' type='Debug' layout='${message}' />
                </targets>
            </nlog>");

            DebugTarget t = c.FindTargetByName("d") as DebugTarget;
            Assert.NotNull(t);
            Assert.Equal(t.Name, "d");
            SimpleLayout l = t.Layout as SimpleLayout;
            Assert.Equal("${message}", l.Text);
            Assert.NotNull(t.Layout);
            Assert.Equal(1, l.Renderers.Count);
            Assert.IsType(typeof(MessageLayoutRenderer), l.Renderers[0]);
        }

        [Fact]
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
            Assert.NotNull(t);
            Assert.Equal(t.Name, "d");
            SimpleLayout l = t.Layout as SimpleLayout;
            Assert.Equal("${message}", l.Text);
            Assert.NotNull(t.Layout);
            Assert.Equal(1, l.Renderers.Count);
            Assert.IsType(typeof(MessageLayoutRenderer), l.Renderers[0]);
        }

        [Fact]
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
            Assert.NotNull(t);
            Assert.Equal(3, t.Parameters.Count);
            Assert.Equal("p1", t.Parameters[0].Name);
            Assert.Equal("'${message}'", t.Parameters[0].Layout.ToString());

            Assert.Equal("p2", t.Parameters[1].Name);
            Assert.Equal("'${level}'", t.Parameters[1].Layout.ToString());
            
            Assert.Equal("p3", t.Parameters[2].Name);
            Assert.Equal("'${logger}'", t.Parameters[2].Layout.ToString());
        }

        [Fact]
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
            Assert.NotNull(t);
            Assert.Equal(3, t.Parameters.Count);
            Assert.Equal("p1", t.Parameters[0].Name);
            Assert.Equal("'${message}'", t.Parameters[0].Layout.ToString());

            Assert.Equal("p2", t.Parameters[1].Name);
            CsvLayout csvLayout = t.Parameters[1].Layout as CsvLayout;
            Assert.NotNull(csvLayout);
            Assert.Equal(2, csvLayout.Columns.Count);
            Assert.Equal("x", csvLayout.Columns[0].Name);
            Assert.Equal("y", csvLayout.Columns[1].Name);

            Assert.Equal("p3", t.Parameters[2].Name);
            Assert.Equal("'${logger}'", t.Parameters[2].Layout.ToString());
        }

        [Fact]
        public void SimpleTest2()
        {
            LoggingConfiguration c = CreateConfigurationFromString(@"
            <nlog>
                <targets>
                    <target name='d' type='Debug' layout='${message} ${level}' />
                </targets>
            </nlog>");

            DebugTarget t = c.FindTargetByName("d") as DebugTarget;
            Assert.NotNull(t);
            Assert.Equal(t.Name, "d");
            SimpleLayout l = t.Layout as SimpleLayout;
            Assert.Equal("${message} ${level}", l.Text);
            Assert.NotNull(l);
            Assert.Equal(3, l.Renderers.Count);
            Assert.IsType(typeof(MessageLayoutRenderer), l.Renderers[0]);
            Assert.IsType(typeof(LiteralLayoutRenderer), l.Renderers[1]);
            Assert.IsType(typeof(LevelLayoutRenderer), l.Renderers[2]);
            Assert.Equal(" ", ((LiteralLayoutRenderer)l.Renderers[1]).Text);
        }

        [Fact]
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

            Assert.NotNull(c.FindTargetByName("a"));
            Assert.NotNull(c.FindTargetByName("b"));
            Assert.NotNull(c.FindTargetByName("c"));

            Assert.IsType(typeof(BufferingTargetWrapper), c.FindTargetByName("b"));
            Assert.IsType(typeof(AsyncTargetWrapper), c.FindTargetByName("a"));
            Assert.IsType(typeof(DebugTarget), c.FindTargetByName("c"));

            BufferingTargetWrapper btw = c.FindTargetByName("b") as BufferingTargetWrapper;
            AsyncTargetWrapper atw = c.FindTargetByName("a") as AsyncTargetWrapper;
            DebugTarget dt = c.FindTargetByName("c") as DebugTarget;

            Assert.Same(atw, btw.WrappedTarget);
            Assert.Same(dt, atw.WrappedTarget);
            Assert.Equal(19, btw.BufferSize);
        }

        [Fact]
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

            Assert.NotNull(c.FindTargetByName("a"));
            Assert.NotNull(c.FindTargetByName("b"));
            Assert.NotNull(c.FindTargetByName("c"));

            Assert.IsType(typeof(BufferingTargetWrapper), c.FindTargetByName("b"));
            Assert.IsType(typeof(AsyncTargetWrapper), c.FindTargetByName("a"));
            Assert.IsType(typeof(DebugTarget), c.FindTargetByName("c"));

            BufferingTargetWrapper btw = c.FindTargetByName("b") as BufferingTargetWrapper;
            AsyncTargetWrapper atw = c.FindTargetByName("a") as AsyncTargetWrapper;
            DebugTarget dt = c.FindTargetByName("c") as DebugTarget;

            Assert.Same(atw, btw.WrappedTarget);
            Assert.Same(dt, atw.WrappedTarget);
            Assert.Equal(19, btw.BufferSize);
        }

        [Fact]
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

            Assert.NotNull(c.FindTargetByName("rr"));
            Assert.NotNull(c.FindTargetByName("d1"));
            Assert.NotNull(c.FindTargetByName("d2"));
            Assert.NotNull(c.FindTargetByName("d3"));
            Assert.NotNull(c.FindTargetByName("d4"));

            Assert.IsType(typeof(RoundRobinGroupTarget), c.FindTargetByName("rr"));
            Assert.IsType(typeof(DebugTarget), c.FindTargetByName("d1"));
            Assert.IsType(typeof(DebugTarget), c.FindTargetByName("d2"));
            Assert.IsType(typeof(DebugTarget), c.FindTargetByName("d3"));
            Assert.IsType(typeof(DebugTarget), c.FindTargetByName("d4"));

            RoundRobinGroupTarget rr = c.FindTargetByName("rr") as RoundRobinGroupTarget;
            DebugTarget d1 = c.FindTargetByName("d1") as DebugTarget;
            DebugTarget d2 = c.FindTargetByName("d2") as DebugTarget;
            DebugTarget d3 = c.FindTargetByName("d3") as DebugTarget;
            DebugTarget d4 = c.FindTargetByName("d4") as DebugTarget;

            Assert.Equal(4, rr.Targets.Count);
            Assert.Same(d1, rr.Targets[0]);
            Assert.Same(d2, rr.Targets[1]);
            Assert.Same(d3, rr.Targets[2]);
            Assert.Same(d4, rr.Targets[3]);

            Assert.Equal(((SimpleLayout)d1.Layout).Text, "${message}1");
            Assert.Equal(((SimpleLayout)d2.Layout).Text, "${message}2");
            Assert.Equal(((SimpleLayout)d3.Layout).Text, "${message}3");
            Assert.Equal(((SimpleLayout)d4.Layout).Text, "${message}4");
        }

        [Fact]
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

            Assert.NotNull(c.FindTargetByName("rr"));
            Assert.NotNull(c.FindTargetByName("d1"));
            Assert.NotNull(c.FindTargetByName("d2"));
            Assert.NotNull(c.FindTargetByName("d3"));
            Assert.NotNull(c.FindTargetByName("d4"));

            Assert.IsType(typeof(RoundRobinGroupTarget), c.FindTargetByName("rr"));
            Assert.IsType(typeof(DebugTarget), c.FindTargetByName("d1"));
            Assert.IsType(typeof(DebugTarget), c.FindTargetByName("d2"));
            Assert.IsType(typeof(DebugTarget), c.FindTargetByName("d3"));
            Assert.IsType(typeof(DebugTarget), c.FindTargetByName("d4"));

            RoundRobinGroupTarget rr = c.FindTargetByName("rr") as RoundRobinGroupTarget;
            DebugTarget d1 = c.FindTargetByName("d1") as DebugTarget;
            DebugTarget d2 = c.FindTargetByName("d2") as DebugTarget;
            DebugTarget d3 = c.FindTargetByName("d3") as DebugTarget;
            DebugTarget d4 = c.FindTargetByName("d4") as DebugTarget;

            Assert.Equal(4, rr.Targets.Count);
            Assert.Same(d1, rr.Targets[0]);
            Assert.Same(d2, rr.Targets[1]);
            Assert.Same(d3, rr.Targets[2]);
            Assert.Same(d4, rr.Targets[3]);

            Assert.Equal(((SimpleLayout)d1.Layout).Text, "${message}1");
            Assert.Equal(((SimpleLayout)d2.Layout).Text, "${message}2");
            Assert.Equal(((SimpleLayout)d3.Layout).Text, "${message}3");
            Assert.Equal(((SimpleLayout)d4.Layout).Text, "${message}4");
        }

        [Fact]
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
            Assert.NotNull(t);
            Assert.Equal(t.Name, "d");

            var wrappedTarget = t.WrappedTarget as DebugTarget;
            Assert.NotNull(wrappedTarget);
            Assert.Equal("d_wrapped", wrappedTarget.Name);

            t = c.FindTargetByName("d2") as AsyncTargetWrapper;
            Assert.NotNull(t);
            Assert.Equal(t.Name, "d2");

            wrappedTarget = t.WrappedTarget as DebugTarget;
            Assert.NotNull(wrappedTarget);
            Assert.Equal("d2_wrapped", wrappedTarget.Name);
        }

        [Fact]
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
            Assert.NotNull(t);
            Assert.Equal("'x${message}x'", t.Layout.ToString());

            t = c.FindTargetByName("d2") as DebugTarget;
            Assert.NotNull(t);
            Assert.Equal("'x${message}x'", t.Layout.ToString());
        }

        [Fact]
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
            Assert.NotNull(bufferingTargetWrapper);

            var retryingTargetWrapper = bufferingTargetWrapper.WrappedTarget as RetryingTargetWrapper;
            Assert.NotNull(retryingTargetWrapper);
            Assert.Equal("RetryingWrapper", retryingTargetWrapper.Name);

            var debugTarget = retryingTargetWrapper.WrappedTarget as DebugTarget;
            Assert.NotNull(debugTarget);
            Assert.Equal("d_wrapped", debugTarget.Name);
            Assert.Equal("'${level}'", debugTarget.Layout.ToString());
        }

        [Fact]
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
                        uriProperty='http://nlog-project.org'
                        lineEndingModeProperty='default'
                        />
                </targets>
            </nlog>");

            var myTarget = c.FindTargetByName("myTarget") as MyTarget;
            Assert.NotNull(myTarget);
            Assert.Equal((byte)42, myTarget.ByteProperty);
            Assert.Equal((short)42, myTarget.Int16Property);
            Assert.Equal(42, myTarget.Int32Property);
            Assert.Equal(42000000000L, myTarget.Int64Property);
            Assert.Equal("foobar", myTarget.StringProperty);
            Assert.Equal(true, myTarget.BoolProperty);
            Assert.Equal(3.14159, myTarget.DoubleProperty);
            Assert.Equal(3.14159f, myTarget.FloatProperty);
            Assert.Equal(MyEnum.Value3, myTarget.EnumProperty);
            Assert.Equal(MyFlagsEnum.Value1 | MyFlagsEnum.Value3, myTarget.FlagsEnumProperty);
            Assert.Equal(Encoding.UTF8, myTarget.EncodingProperty);
            Assert.Equal("en-US", myTarget.CultureProperty.Name);
            Assert.Equal(typeof(int), myTarget.TypeProperty);
            Assert.Equal("'${level}'", myTarget.LayoutProperty.ToString());
            Assert.Equal("starts-with(message, 'x')", myTarget.ConditionProperty.ToString());
            Assert.Equal(new Uri("http://nlog-project.org"), myTarget.UriProperty);
            Assert.Equal(LineEndingMode.Default, myTarget.LineEndingModeProperty);
        }

        [Fact]
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
            Assert.NotNull(myTarget);
            Assert.Equal((byte)42, myTarget.ByteProperty);
            Assert.Equal((short)42, myTarget.Int16Property);
            Assert.Equal(42, myTarget.Int32Property);
            Assert.Equal(42000000000L, myTarget.Int64Property);
            Assert.Equal("foobar", myTarget.StringProperty);
            Assert.Equal(true, myTarget.BoolProperty);
            Assert.Equal(3.14159, myTarget.DoubleProperty);
            Assert.Equal(3.14159f, myTarget.FloatProperty);
            Assert.Equal(MyEnum.Value3, myTarget.EnumProperty);
            Assert.Equal(MyFlagsEnum.Value1 | MyFlagsEnum.Value3, myTarget.FlagsEnumProperty);
            Assert.Equal(Encoding.UTF8, myTarget.EncodingProperty);
            Assert.Equal("en-US", myTarget.CultureProperty.Name);
            Assert.Equal(typeof(int), myTarget.TypeProperty);
            Assert.Equal("'${level}'", myTarget.LayoutProperty.ToString());
            Assert.Equal("starts-with(message, 'x')", myTarget.ConditionProperty.ToString());
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

            public Uri UriProperty { get; set; }

            public LineEndingMode LineEndingModeProperty { get; set; }
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