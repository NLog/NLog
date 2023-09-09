// 
// Copyright (c) 2004-2021 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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

namespace NLog.UnitTests.Layouts
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using NLog.LayoutRenderers;
    using NLog.Layouts;
    using NLog.Targets;
    using Xunit;

    public class AllEventPropertiesTests : NLogTestBase
    {
        [Fact]
        public void AllParametersAreSetToDefault()
        {
            var renderer = new AllEventPropertiesLayoutRenderer();
            var ev = BuildLogEventWithProperties();

            var result = renderer.Render(ev);

            Assert.Equal("a=1, hello=world, 17=100", result);
        }

        [Fact]
        public void CustomSeparator()
        {
            var renderer = new AllEventPropertiesLayoutRenderer();
            renderer.Separator = " | ";
            var ev = BuildLogEventWithProperties();

            var result = renderer.Render(ev);

            Assert.Equal("a=1 | hello=world | 17=100", result);
        }

        [Fact]
        public void CustomFormat()
        {
            var renderer = new AllEventPropertiesLayoutRenderer();
            renderer.Format = "[key] is [value]";
            var ev = BuildLogEventWithProperties();

            var result = renderer.Render(ev);

            Assert.Equal("a is 1, hello is world, 17 is 100", result);
        }

        [Fact]
        public void NoProperties()
        {
            var renderer = new AllEventPropertiesLayoutRenderer();
            var ev = new LogEventInfo();

            var result = renderer.Render(ev);

            Assert.Equal("", result);
        }

        [Fact]
        public void EventPropertyFormat()
        {
            var renderer = new AllEventPropertiesLayoutRenderer();
            var ev = new LogEventInfo(LogLevel.Info, null, null, "{pi:0}", new object[] { 3.14159265359 });
            var result = renderer.Render(ev);
            Assert.Equal("pi=3", result);
        }

        [Fact]
        public void StructuredLoggingProperties()
        {
            var renderer = new AllEventPropertiesLayoutRenderer();
            var planetProperties = new System.Collections.Generic.Dictionary<string, object>()
            {
                { "Name", "Earth" },
                { "PlanetType", "Water-world" },
            };

            var ev = new LogEventInfo(LogLevel.Info, null, null, "Hello Planet {@planet}", new object[] { planetProperties });
            var result = renderer.Render(ev);
            Assert.Equal(@"planet=""Name""=""Earth"", ""PlanetType""=""Water-world""", result);
        }

        [Fact]
        public void IncludeScopeProperties()
        {
            var testLayout = new SimpleLayout("${all-event-properties:IncludeScopeProperties=true}");

            var logFactory = new LogFactory().Setup().LoadConfiguration(builder =>
            {
                builder.ForLogger().WriteTo(new DebugTarget("debug") { Layout = testLayout }).WithAsync();
            }).LogFactory;

            var ev = new LogEventInfo(LogLevel.Info, null, null, "{pi:0}", new object[] { 3.14159265359 });
            using (ScopeContext.PushProperty("Figure", "Circle"))
            {
                var logger = logFactory.GetLogger("B");

                logger.Debug(ev);
            }

            var target = logFactory.Configuration.AllTargets.OfType<DebugTarget>().First();
            logFactory.Shutdown();  // Flush

            var result = target.LastMessage;
            Assert.Equal("pi=3, Figure=Circle", result);
        }

        [Fact]
        public void TestInvalidCustomFormatWithoutKeyPlaceholder()
        {
            var renderer = new AllEventPropertiesLayoutRenderer();
            var ex = Assert.Throws<ArgumentException>(() => renderer.Format = "[key is [value]");
            Assert.Equal("Invalid format: [key] placeholder is missing.", ex.Message);
        }

        [Fact]
        public void TestInvalidCustomFormatWithoutValuePlaceholder()
        {
            var renderer = new AllEventPropertiesLayoutRenderer();
            var ex = Assert.Throws<ArgumentException>(() => renderer.Format = "[key] is [vlue]");
            Assert.Equal("Invalid format: [value] placeholder is missing.", ex.Message);
        }

        [Fact]
        public void AllEventWithFluent_without_callerInformation()
        {
            //Arrange
            LogFactory logFactory = new LogFactory()
                .Setup()
                .LoadConfiguration(builder => 
                { 
                    builder.ForLogger().WriteTo(new DebugTarget("debug") { Layout = "${all-event-properties}" });
                })
                .LogFactory;

            //Act
            logFactory.GetCurrentClassLogger()
                .WithProperty("Test", "InfoWrite")
                .WithProperty("coolness", "200%")
                .WithProperty("a", "not b")
                .Debug("This is a test message '{0}'.", DateTime.Now.Ticks);

            //Assert
            logFactory.AssertDebugLastMessage("Test=InfoWrite, coolness=200%, a=not b");
        }

        [Fact]
        public void WithPropertiesTest()
        {
            //Arrange
            LogFactory logFactory = new LogFactory()
                .Setup()
                .LoadConfiguration(builder =>
                {
                    builder.ForLogger().WriteTo(new DebugTarget("debug") { Layout = "${all-event-properties}" });
                })
                .LogFactory;

            Dictionary<string, object> properties = new Dictionary<string, object>()
            {
                {"TestString", "myString" },
                {"TestInt", 999 }
            };

            //Act
            logFactory.GetCurrentClassLogger()              
                .WithProperties(properties)
                .Debug("This is a test message '{0}'.", DateTime.Now.Ticks);

            //Assert
            logFactory.AssertDebugLastMessage("TestString=myString, TestInt=999");
        }

#if NET35 || NET40
        [Fact(Skip = "NET35 not supporting Caller-Attributes")]
#else
        [Fact]
#endif
        public void AllEventWithFluent_with_callerInformation()
        {
            //Arrange
            LogFactory logFactory = new LogFactory()
                .Setup()
                .LoadConfiguration(builder =>
                {
                    builder.ForLogger().WriteTo(new DebugTarget("debug") { Layout = "${all-event-properties}${callsite}" });
                })
                .LogFactory;

            //Act
            logFactory.GetCurrentClassLogger()
                .WithProperty("Test", "InfoWrite")
                .WithProperty("coolness", "200%")
                .WithProperty("a", "not b")
                .Debug("This is a test message '{0}'.", DateTime.Now.Ticks);

            //Assert
            logFactory.AssertDebugLastMessageContains(nameof(AllEventWithFluent_with_callerInformation));
            logFactory.AssertDebugLastMessageContains(nameof(AllEventPropertiesTests));
        }

        [Theory]
        [InlineData(null, "a=1, hello=world, 17=100, notempty=0")]
        [InlineData(false, "a=1, hello=world, 17=100, notempty=0")]
        [InlineData(true, "a=1, hello=world, 17=100, empty1=, empty2=, notempty=0")]
        public void IncludeEmptyValuesTest(bool? includeEmptyValues, string expected)
        {
            // Arrange
            var renderer = new AllEventPropertiesLayoutRenderer();
            if (includeEmptyValues != null)
            {
                renderer.IncludeEmptyValues = includeEmptyValues.Value;
            }

            var ev = BuildLogEventWithProperties();
            ev.Properties["empty1"] = null;
            ev.Properties["empty2"] = "";
            ev.Properties["notempty"] = 0;

            // Act
            var result = renderer.Render(ev);

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("", "a=1, hello=world, 17=100")]
        [InlineData("Wrong", "a=1, hello=world, 17=100")]
        [InlineData("hello", "a=1, 17=100")]
        [InlineData("Hello", "a=1, 17=100")]
        [InlineData("Hello, 17", "a=1")]
        public void ExcludeSingleProperty(string exclude, string result)
        {
            // Arrange
            var logFactory = new LogFactory().Setup().LoadConfigurationFromXml(@"
                <nlog throwExceptions='true'>
                    <targets>
                        <target type='Debug'
                                name='debug'
                                layout='${all-event-properties:Exclude=" + exclude + @"}'
                                />
                    </targets>
                    <rules>
                      <logger name='*' writeTo='debug' />
                    </rules>
                </nlog>").LogFactory;
            var logger = logFactory.GetCurrentClassLogger();

            // Act
            var ev = BuildLogEventWithProperties();
            logger.Log(ev);

            // Assert
            logFactory.AssertDebugLastMessageContains(result);
        }

        private static LogEventInfo BuildLogEventWithProperties()
        {
            var ev = new LogEventInfo() { Level = LogLevel.Info };
            ev.Properties["a"] = 1;
            ev.Properties["hello"] = "world";
            ev.Properties[17] = 100;
            return ev;
        }
    }
}