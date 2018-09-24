// 
// Copyright (c) 2004-2018 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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
    using NLog.Layouts;
    using Xunit;

    public class XmlLayoutTests : NLogTestBase
    {
        [Fact]
        public void XmlLayoutRendering()
        {
            var xmlLayout = new XmlLayout()
            {
                Elements =
                    {
                        new XmlLayout("date", "${longdate}"),
                        new XmlLayout("level", "${level}"),
                        new XmlLayout("message", "${message}"),
                    },
                IndentXml = true,
                IncludeAllProperties = true,
            };

            var logEventInfo = new LogEventInfo
            {
                TimeStamp = new DateTime(2010, 01, 01, 12, 34, 56),
                Level = LogLevel.Info,
                Message = "hello, world"
            };
            logEventInfo.Properties["nlogPropertyKey"] = "nlogPropertyValue";

            Assert.Equal(string.Format(System.Globalization.CultureInfo.InvariantCulture, @"<logevent>{0}{1}<date>2010-01-01 12:34:56.0000</date>{0}{1}<level>Info</level>{0}{1}<message>hello, world</message>{0}{1}<property key=""nlogPropertyKey"">nlogPropertyValue</property>{0}</logevent>", Environment.NewLine, "  "), xmlLayout.Render(logEventInfo));
        }

        [Fact]
        public void XmlLayoutLog4j()
        {
            LogManager.Configuration = CreateConfigurationFromString(@"
                <nlog throwExceptions='true'>
                    <targets>
                        <target name='debug' type='debug'>
                            <layout type='xmllayout' elementName='log4j:event' propertiesElementName='log4j:data' propertiesElementKeyAttribute='name' propertiesElementValueAttribute='value' includeAllProperties='true' includeMdc='true' includeMdlc='true' >
                                <attribute name='logger' layout='${logger}' includeEmptyValue='true' />
                                <attribute name='level' layout='${uppercase:${level}}' includeEmptyValue='true' />
                                <element elementName='log4j:message' elementValue='${message}' />
                                <element elementName='log4j:throwable' elementValue='${exception:format=tostring}' />
                                <element elementName='log4j:locationInfo'>
                                    <attribute name='class' layout='${callsite:methodName=false}' includeEmptyValue='true' />
                                </element>
                            </layout>
                        </target>
                    </targets>
                    <rules>
                        <logger name='*' minlevel='debug' appendto='debug' />
                    </rules>
                </nlog>");

            MappedDiagnosticsContext.Clear();
            MappedDiagnosticsContext.Set("foo1", "bar1");
            MappedDiagnosticsContext.Set("foo2", "bar2");

            MappedDiagnosticsLogicalContext.Clear();
            MappedDiagnosticsLogicalContext.Set("foo3", "bar3");

            var logger = LogManager.GetLogger("hello");

            var logEventInfo = LogEventInfo.Create(LogLevel.Debug, "A", null, null, "some message");
            logEventInfo.Properties["nlogPropertyKey"] = "<nlog\r\nPropertyValue>";
            logger.Log(logEventInfo);

            var target = LogManager.Configuration.FindTargetByName<NLog.Targets.DebugTarget>("debug");
            Assert.Equal(@"<log4j:event logger=""A"" level=""DEBUG""><log4j:message>some message</log4j:message><log4j:locationInfo class=""NLog.UnitTests.Layouts.XmlLayoutTests""/><log4j:data name=""foo1"" value=""bar1""/><log4j:data name=""foo2"" value=""bar2""/><log4j:data name=""foo3"" value=""bar3""/><log4j:data name=""nlogPropertyKey"" value=""&lt;nlog&#13;&#10;PropertyValue&gt;""/></log4j:event>", target.LastMessage);
        }

        [Fact]
        public void XmlLayout_IncludeEmptyValue_RenderEmptyValue()
        {
            // Arrange
            var xmlLayout = new XmlLayout()
            {
                Elements =
                {
                    new XmlLayout("message", "${message}"),
                },
                IndentXml = true,
                IncludeAllProperties = true,
            };

            var logEventInfo = new LogEventInfo
            {
                Message = ""
            };
            logEventInfo.Properties["nlogPropertyKey"] = null;

            // Act
            var result = xmlLayout.Render(logEventInfo);

            // Assert
            const string expected =
 @"<logevent>
  <message></message>
  <property key=""nlogPropertyKey"">null</property>
</logevent>";

            Assert.Equal(expected, result);
        }

        [Fact]
        public void XmlLayout_ExcludeProperties_RenderNotProperty()
        {
        }

        [Fact]
        public void XmlLayout_OnlyLogEventProperties_RenderRootCorrect()
        {
        }

        [Fact]
        public void XmlLayout_PropertiesElementNameFormat_RenderPropertyName()
        {
        }

        [Fact]
        public void XmlLayout_DoubleNestElements_RendersAllElements()
        {
        }
    }
}
