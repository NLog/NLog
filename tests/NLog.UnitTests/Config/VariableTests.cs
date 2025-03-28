//
// Copyright (c) 2004-2024 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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
    using System.Linq;
    using NLog.Config;
    using NLog.LayoutRenderers;
    using NLog.Layouts;
    using NLog.Targets;
    using Xunit;

    public class VariableTests : NLogTestBase
    {
        [Theory]
        [InlineData("${prefix}${message}${suffix}", "prefix")]
        [InlineData("${prefix}${message}${suffix}", "Prefix")]
        [InlineData("${PreFix}${MessAGE}${SUFFIX}", "Prefix")]
        public void VariablesTest1(string targetLayout, string variableName1)
        {
            var configuration = XmlLoggingConfiguration.CreateFromXmlString($@"
<nlog throwExceptions='true'>
    <variable name='{variableName1}' value='[[' />
    <variable name='suffix' value=']]' />
    <targets>
        <target name='d1' type='Debug' layout='{targetLayout}' />
    </targets>
</nlog>");

            var d1 = configuration.FindTargetByName("d1") as DebugTarget;
            Assert.NotNull(d1);
            var layout = d1.Layout as SimpleLayout;
            Assert.NotNull(layout);
            Assert.Equal(3, layout.Renderers.Count);
            var lr1 = layout.Renderers[0] as LiteralLayoutRenderer;
            var lr2 = layout.Renderers[1] as MessageLayoutRenderer;
            var lr3 = layout.Renderers[2] as LiteralLayoutRenderer;
            Assert.NotNull(lr1);
            Assert.NotNull(lr2);
            Assert.NotNull(lr3);
            Assert.Equal("[[", lr1.Text);
            Assert.Equal("]]", lr3.Text);
        }

        /// <summary>
        /// Expand of property which are not layoutable <see cref="Layout"/>, but still get expanded.
        /// </summary>
        [Fact]
        public void VariablesTest_string_expanding()
        {
            var configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
<nlog throwExceptions='true'>
  <variable name='test' value='hello'/>
  <targets>
    <target type='File' name='test' ArchiveSuffixFormat='${test}'/>
  </targets>
</nlog>");

            var target = configuration.FindTargetByName("test") as FileTarget;
            Assert.NotNull(target);
            //dont change the ${test} as it isn't a Layout
            Assert.Equal(typeof(string), target.ArchiveSuffixFormat.GetType());
            Assert.Equal("hello", target.ArchiveSuffixFormat);
        }

        [Fact]
        public void VariablesTest_WriteTo_expanding()
        {
            var configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
<nlog throwExceptions='true'>
  <variable name='test' value='test_target'/>
  <targets>
    <target type='Debug' name='test_target' />
  </targets>
    <rules>
      <logger writeTo='${test}' />
    </rules>
</nlog>");

            var target = configuration.FindTargetByName("test_target");
            Assert.NotNull(target);

            Assert.Single(configuration.LoggingRules);
            Assert.Single(configuration.LoggingRules[0].Targets);
            Assert.Same(target, configuration.LoggingRules[0].Targets.First());
        }

        [Fact]
        public void VariablesTest_minLevel_maxLevel_expanding()
        {
            var configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
<nlog throwExceptions='true'>
    <variable name='test1' value='info'/>
    <variable name='test2' value='warn'/>
    <rules>
      <logger minLevel='${test1}' maxLevel='${test2}' final='true' />
    </rules>
</nlog>");

            var rule = configuration.LoggingRules[0];
            Assert.NotNull(rule);
            Assert.False(rule.IsLoggingEnabledForLevel(LogLevel.Trace));
            Assert.False(rule.IsLoggingEnabledForLevel(LogLevel.Debug));
            Assert.True(rule.IsLoggingEnabledForLevel(LogLevel.Info));
            Assert.True(rule.IsLoggingEnabledForLevel(LogLevel.Warn));
            Assert.False(rule.IsLoggingEnabledForLevel(LogLevel.Error));
            Assert.False(rule.IsLoggingEnabledForLevel(LogLevel.Fatal));
        }

        [Fact]
        public void VariablesTest_Level_expanding()
        {
            var configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
<nlog throwExceptions='true'>
   <variable name='test' value='debug'/>
    <rules>
      <logger level='${test}' final='true' />
    </rules>
</nlog>");

            var rule = configuration.LoggingRules[0];
            Assert.NotNull(rule);
            Assert.False(rule.IsLoggingEnabledForLevel(LogLevel.Trace));
            Assert.True(rule.IsLoggingEnabledForLevel(LogLevel.Debug));
            Assert.False(rule.IsLoggingEnabledForLevel(LogLevel.Info));
            Assert.False(rule.IsLoggingEnabledForLevel(LogLevel.Warn));
            Assert.False(rule.IsLoggingEnabledForLevel(LogLevel.Error));
            Assert.False(rule.IsLoggingEnabledForLevel(LogLevel.Fatal));
        }

        [Fact]
        public void VariablesTest_Levels_expanding()
        {
            var configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
<nlog throwExceptions='true'>
   <variable name='test' value='debug,fatal'/>
    <rules>
      <logger levels='${test}' final='true' />
    </rules>
</nlog>");

            var rule = configuration.LoggingRules[0];
            Assert.NotNull(rule);
            Assert.False(rule.IsLoggingEnabledForLevel(LogLevel.Trace));
            Assert.True(rule.IsLoggingEnabledForLevel(LogLevel.Debug));
            Assert.False(rule.IsLoggingEnabledForLevel(LogLevel.Info));
            Assert.False(rule.IsLoggingEnabledForLevel(LogLevel.Warn));
            Assert.False(rule.IsLoggingEnabledForLevel(LogLevel.Error));
            Assert.True(rule.IsLoggingEnabledForLevel(LogLevel.Fatal));
        }

        [Fact]
        public void Xml_configuration_returns_defined_variables()
        {
            var configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
<nlog throwExceptions='true'>
    <variables>
        <variable name='prefix' value='[[' />
        <variable name='suffix' value=']]' />
    </variables>
</nlog>");

            var nullEvent = LogEventInfo.CreateNullEvent();

            // Act & Assert
            Assert.Equal("[[", configuration.Variables["prefix"].Render(nullEvent));
            Assert.Equal("]]", configuration.Variables["suffix"].Render(nullEvent));
        }

        [Fact]
        public void Xml_configuration_with_inner_returns_defined_variables()
        {
            var configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
<nlog throwExceptions='true'>
    <variable name='prefix'><layout><![CDATA[
newline
]]></layout></variable>
    <variable name='suffix'><layout>]]</layout></variable>
</nlog>");

            var nullEvent = LogEventInfo.CreateNullEvent();

            // Act & Assert
            Assert.Equal("\nnewline\n", configuration.Variables["prefix"].Render(nullEvent).Replace("\r", ""));
            Assert.Equal("]]", configuration.Variables["suffix"].Render(nullEvent));
        }

        [Fact]
        public void Xml_configuration_with_innerLayouts_returns_defined_variables()
        {
            var configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
<nlog throwExceptions='true'>
    <variable name='myJson'  >
        <layout type='JsonLayout'>
            <attribute name='short date' layout='${shortdate}' />
            <attribute name='message' layout='${message}' />
        </layout>
    </variable>
</nlog>");

            // Act & Assert
            var jsonLayout = Assert.IsType<JsonLayout>(configuration.Variables["myJson"]);
            Assert.Equal(2, jsonLayout.Attributes.Count);
            Assert.Equal("short date", jsonLayout.Attributes[0].Name);
            Assert.NotNull(jsonLayout.Attributes[0].Layout);
        }

        [Fact]
        public void Xml_configuration_with_inner_returns_defined_variables_withValueElement()
        {
            var configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
<nlog throwExceptions='true'>
    <variable name='prefix'>

<value><![CDATA[
newline
]]></value>

</variable>
    <variable name='suffix'><value>]]</value></variable>
</nlog>");

            var nullEvent = LogEventInfo.CreateNullEvent();

            // Act & Assert
            Assert.Equal("\nnewline\n", configuration.Variables["prefix"].Render(nullEvent).Replace("\r", ""));
            Assert.Equal("]]", configuration.Variables["suffix"].Render(nullEvent));
        }

        [Fact]
        public void Xml_configuration_variableWithInnerAndAttribute_attributeHasPrecedence()
        {
            using (new NoThrowNLogExceptions())
            {
                var configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
<nlog>
    <variable name='var1' value='1'><value>2</value></variable>
</nlog>");
                var nullEvent = LogEventInfo.CreateNullEvent();

                // Act & Assert
                Assert.Equal("1", configuration.Variables["var1"].Render(nullEvent));
            }
        }

        [Fact]
        public void NLogConfigurationExceptionShouldThrown_WhenVariableNodeIsWrittenToWrongPlace()
        {
            LogManager.ThrowConfigExceptions = true;
            const string configurationString_VariableNodeIsInnerTargets =
                    @"<nlog>
	                        <targets>
			                    <variable name='variableOne' value='${longdate:universalTime=True}Z | ${message}'/>
                    			<target name='d1' type='Debug' layout='${variableOne}' />
	                        </targets>
                            <rules>
			                    <logger name='*' minlevel='Debug' writeTo='d1'/>
                            </rules>
                    </nlog>";


            const string configurationString_VariableNodeIsAfterTargets =
                    @"<nlog>
	                        <targets>
			                    <target name='d1' type='Debug' layout='${variableOne}' />
	                        </targets>
                            <variable name='variableOne' value='${longdate:universalTime=True}Z | ${message}'/>
                            <rules>
			                    <logger name='*' minlevel='Debug' writeTo='d1'/>
                            </rules>
                    </nlog>";

            NLogConfigurationException nlogConfEx_ForInnerTargets = Assert.Throws<NLogConfigurationException>(
                () => XmlLoggingConfiguration.CreateFromXmlString(configurationString_VariableNodeIsInnerTargets)
                );

            NLogConfigurationException nlogConfExForAfterTargets = Assert.Throws<NLogConfigurationException>(
                () => XmlLoggingConfiguration.CreateFromXmlString(configurationString_VariableNodeIsAfterTargets)
                );
        }
    }
}
