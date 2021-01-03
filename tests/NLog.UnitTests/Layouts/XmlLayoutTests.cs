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

using System.Collections.Generic;
using NLog.Config;

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
                        new XmlElement("date", "${longdate}"),
                        new XmlElement("level", "${level}"),
                        new XmlElement("message", "${message}"),
                    },
                IndentXml = true,
                IncludeEventProperties = true,
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
            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
                <nlog throwExceptions='true'>
                    <targets>
                        <target name='debug' type='debug'>
                            <layout type='xmllayout' elementName='log4j:event' propertiesElementName='log4j:data' propertiesElementKeyAttribute='name' propertiesElementValueAttribute='value' includeAllProperties='true' includeMdc='true' includeMdlc='true' >
                                <attribute name='logger' layout='${logger}' includeEmptyValue='true' />
                                <attribute name='level' layout='${uppercase:${level}}' includeEmptyValue='true' />
                                <element name='log4j:message' value='${message}' />
                                <element name='log4j:throwable' value='${exception:format=tostring}' />
                                <element name='log4j:locationInfo'>
                                    <attribute name='class' layout='${callsite:methodName=false}' includeEmptyValue='true' />
                                </element>
                            </layout>
                        </target>
                    </targets>
                    <rules>
                        <logger name='*' minlevel='debug' appendto='debug' />
                    </rules>
                </nlog>");

            ScopeContext.Clear();

            ScopeContext.PushProperty("foo1", "bar1");
            ScopeContext.PushProperty("foo2", "bar2");
            ScopeContext.PushProperty("foo3", "bar3");

            var logger = LogManager.GetLogger("hello");

            var logEventInfo = LogEventInfo.Create(LogLevel.Debug, "A", null, null, "some message");
            logEventInfo.Properties["nlogPropertyKey"] = "<nlog\r\nPropertyValue>";
            logger.Log(logEventInfo);

            var target = LogManager.Configuration.FindTargetByName<NLog.Targets.DebugTarget>("debug");
            Assert.Equal(@"<log4j:event logger=""A"" level=""DEBUG""><log4j:message>some message</log4j:message><log4j:locationInfo class=""NLog.UnitTests.Layouts.XmlLayoutTests""/><log4j:data name=""foo1"" value=""bar1""/><log4j:data name=""foo2"" value=""bar2""/><log4j:data name=""foo3"" value=""bar3""/><log4j:data name=""nlogPropertyKey"" value=""&lt;nlog&#13;&#10;PropertyValue&gt;""/></log4j:event>", target.LastMessage);
        }

        [Fact]
        public void XmlLayout_EncodeValue_RenderXmlMessage()
        {
            // Arrange
            var xmlLayout = new XmlLayout()
            {
                Elements =
                {
                    new XmlElement("message", "${message}"),
                },
            };

            var logEventInfo = new LogEventInfo {  Message = @"<hello planet=""earth""/>" };

            // Act
            var result = xmlLayout.Render(logEventInfo);

            // Assert
            const string expected = @"<logevent><message>&lt;hello planet=&quot;earth&quot;/&gt;</message></logevent>";
            Assert.Equal(expected, result);
        }

        [Fact]
        public void XmlLayout_SkipEncodeValue_RenderXmlMessage()
        {
            // Arrange
            var xmlLayout = new XmlLayout()
            {
                Elements =
                {
                    new XmlElement("message", "${message}") { Encode = false }
                },
            };

            var logEventInfo = new LogEventInfo { Message = @"<hello planet=""earth""/>" };

            // Act
            var result = xmlLayout.Render(logEventInfo);

            // Assert
            const string expected = @"<logevent><message><hello planet=""earth""/></message></logevent>";
            Assert.Equal(expected, result);
        }

        [Fact]
        public void XmlLayout_IncludeEmptyValue_RenderEmptyValue()
        {
            // Arrange
            var xmlLayout = new XmlLayout()
            {
                Elements =
                {
                    new XmlElement("message", "${message}") { IncludeEmptyValue = true },
                },
                IncludeEventProperties = true,
                IncludeEmptyValue = true,
            };

            var logEventInfo = new LogEventInfo
            {
                Message = ""
            };
            logEventInfo.Properties["nlogPropertyKey"] = null;

            // Act
            var result = xmlLayout.Render(logEventInfo);

            // Assert
            const string expected = @"<logevent><message></message><property key=""nlogPropertyKey"">null</property></logevent>";
            Assert.Equal(expected, result);
        }


        [Fact]
        public void XmlLayout_NoIndent_RendersOneLine()
        {
            // Arrange
            var xmlLayout = new XmlLayout()
            {
                Elements =
                {
                    new XmlElement("level", "${level}"),
                    new XmlElement("message", "${message}"),
                },
                IndentXml = false,
                IncludeEventProperties = true,
            };

            var logEventInfo = new LogEventInfo
            {
                Message = "message 1",
                Level = LogLevel.Debug
            };

            logEventInfo.Properties["prop1"] = "a";
            logEventInfo.Properties["prop2"] = "b";
            logEventInfo.Properties["prop3"] = "c";

            // Act
            var result = xmlLayout.Render(logEventInfo);

            // Assert
            const string expected =
                @"<logevent><level>Debug</level><message>message 1</message><property key=""prop1"">a</property><property key=""prop2"">b</property><property key=""prop3"">c</property></logevent>";

            Assert.Equal(expected, result);
        }

        [Fact]
        public void XmlLayout_ExcludeProperties_RenderNotProperty()
        {
            // Arrange
            var xmlLayout = new XmlLayout()
            {
                Elements =
                {
                    new XmlElement("message", "${message}"),
                },
                IncludeEventProperties = true,
                ExcludeProperties = new HashSet<string> { "prop2" }
            };

            var logEventInfo = new LogEventInfo
            {
                Message = "message 1"
            };
            logEventInfo.Properties["prop1"] = "a";
            logEventInfo.Properties["prop2"] = "b";
            logEventInfo.Properties["prop3"] = "c";

            // Act
            var result = xmlLayout.Render(logEventInfo);

            // Assert
            const string expected = @"<logevent><message>message 1</message><property key=""prop1"">a</property><property key=""prop3"">c</property></logevent>";
            Assert.Equal(expected, result);
        }

        [Fact]
        public void XmlLayout_OnlyLogEventProperties_RenderRootCorrect()
        {
            // Arrange
            var xmlLayout = new XmlLayout()
            {
                IncludeEventProperties = true,
            };

            var logEventInfo = new LogEventInfo
            {
                Message = "message 1"
            };
            logEventInfo.Properties["prop1"] = "a";
            logEventInfo.Properties["prop2"] = "b";
            logEventInfo.Properties["prop3"] = "c";

            // Act
            var result = xmlLayout.Render(logEventInfo);

            // Assert
            const string expected = @"<logevent><property key=""prop1"">a</property><property key=""prop2"">b</property><property key=""prop3"">c</property></logevent>";
            Assert.Equal(expected, result);
        }

        [Fact]
        public void XmlLayout_InvalidXmlPropertyName_RenderNameCorrect()
        {
            // Arrange
            var xmlLayout = new XmlLayout()
            {
                IncludeEventProperties = true,
                PropertiesElementName = "{0}",
                PropertiesElementKeyAttribute = "",
            };

            var logEventInfo = new LogEventInfo
            {
                Message = "message 1"
            };
            logEventInfo.Properties["1prop"] = "a";
            logEventInfo.Properties["_2prop"] = "b";
            logEventInfo.Properties[" 3prop"] = "c";
            logEventInfo.Properties["_4 prop"] = "d";

            // Act
            var result = xmlLayout.Render(logEventInfo);

            // Assert
            const string expected = @"<logevent><_1prop>a</_1prop><_2prop>b</_2prop><_3prop>c</_3prop><_4_prop>d</_4_prop></logevent>";
            Assert.Equal(expected, result);
        }

        [Fact]
        public void XmlLayout_PropertiesAttributeNames_RenderPropertyName()
        {
            // Arrange
            var xmlLayout = new XmlLayout()
            {
                IncludeEventProperties = true,
                PropertiesElementName = "p",
                PropertiesElementKeyAttribute = "k",
                PropertiesElementValueAttribute = "v",
            };

            var logEventInfo = new LogEventInfo
            {
                Message = "message 1"
            };
            logEventInfo.Properties["prop1"] = "a";
            logEventInfo.Properties["prop2"] = "b";

            // Act
            var result = xmlLayout.Render(logEventInfo);

            // Assert
            const string expected = @"<logevent><p k=""prop1"" v=""a""/><p k=""prop2"" v=""b""/></logevent>";
            Assert.Equal(expected, result);
        }

        [Fact]
        public void XmlLayout_PropertiesElementNameFormat_RenderPropertyName()
        {
            // Arrange
            var xmlLayout = new XmlLayout()
            {
                IncludeEventProperties = true,
                PropertiesElementName = "{0}",
                PropertiesElementKeyAttribute = "",
                PropertiesElementValueAttribute = "v",
            };

            var logEventInfo = new LogEventInfo
            {
                Message = "message 1"
            };
            logEventInfo.Properties["prop1"] = "a";
            logEventInfo.Properties["prop2"] = "b";

            // Act
            var result = xmlLayout.Render(logEventInfo);

            // Assert
            const string expected = @"<logevent><prop1 v=""a""/><prop2 v=""b""/></logevent>";
            Assert.Equal(expected, result);
        }

        [Fact]
        public void XmlLayout_DoubleNestedElements_RendersAllElements()
        {
            // Arrange
            var xmlLayout = new XmlLayout()
            {
                Elements =
                {
                    new XmlElement("message", "${message}")
                    {
                        Elements =
                        {
                            new XmlElement("level", "${level}")
                        },
                        IncludeEventProperties = true,
                    }

                },
            };

            var logEventInfo = new LogEventInfo
            {
                Level = LogLevel.Debug,
                Message = "message 1"
            };
            logEventInfo.Properties["prop1"] = "a";
            logEventInfo.Properties["prop2"] = "b";

            // Act
            var result = xmlLayout.Render(logEventInfo);

            // Assert
            string expected = @"<logevent><message>message 1<level>Debug</level><property key=""prop1"">a</property><property key=""prop2"">b</property></message></logevent>";
            Assert.Equal(expected, result);
        }

        [Fact]
        public void XmlLayout_PropertiesElementNameDefault_Properties_RenderPropertyDictionary()
        {
            // Arrange
            var xmlLayout = new XmlLayout()
            {
                Elements = { new XmlElement("message", "${message}") },
                IncludeEventProperties = true,
            };

            var logEventInfo = new LogEventInfo
            {
                Message = "Monster massage"
            };
            logEventInfo.Properties["nlogPropertyKey"] = new Dictionary<string, object> { { "Hello", "World" } };

            // Act
            var result = xmlLayout.Render(logEventInfo);

            // Assert
            const string expected = @"<logevent><message>Monster massage</message><property key=""nlogPropertyKey""><property key=""Hello"">World</property></property></logevent>";
            Assert.Equal(expected, result);
        }

        [Fact]
        public void XmlLayout_PropertiesElementNameFormat_RenderPropertyDictionary()
        {
            // Arrange
            var xmlLayout = new XmlLayout()
            {
                Elements = { new XmlElement("message", "${message}") },
                PropertiesElementName = "{0}",
                PropertiesElementKeyAttribute = "",
                IncludeEventProperties = true,
            };

            var logEventInfo = new LogEventInfo
            {
                Message = "Monster massage"
            };
            logEventInfo.Properties["nlogPropertyKey"] = new Dictionary<string, object> { { "Hello", "World" } };

            // Act
            var result = xmlLayout.Render(logEventInfo);

            // Assert
            const string expected = @"<logevent><message>Monster massage</message><nlogPropertyKey><Hello>World</Hello></nlogPropertyKey></logevent>";
            Assert.Equal(expected, result);
        }

        [Fact]
        public void XmlLayout_PropertiesElementNameDefault_Properties_RenderPropertyList()
        {
            // Arrange
            var xmlLayout = new XmlLayout()
            {
                Elements = { new XmlElement("message", "${message}") },
                IncludeEventProperties = true,
            };

            var logEventInfo = new LogEventInfo
            {
                Message = "Monster massage"
            };
            logEventInfo.Properties["nlogPropertyKey"] = new [] { "Hello", "World" };

            // Act
            var result = xmlLayout.Render(logEventInfo);

            // Assert
            const string expected = @"<logevent><message>Monster massage</message><property key=""nlogPropertyKey""><item>Hello</item><item>World</item></property></logevent>";
            Assert.Equal(expected, result);
        }

        [Fact]
        public void XmlLayout_PropertiesElementNameFormat_RenderPropertyList()
        {
            // Arrange
            var xmlLayout = new XmlLayout()
            {
                Elements = { new XmlElement("message", "${message}") },
                PropertiesElementName = "{0}",
                PropertiesElementKeyAttribute = "",
                IncludeEventProperties = true,
                PropertiesCollectionItemName = "node",
                PropertiesElementValueAttribute = "value",
            };

            var logEventInfo = new LogEventInfo
            {
                Message = "Monster massage"
            };
            logEventInfo.Properties["nlogPropertyKey"] = new[] { "Hello", "World" };

            // Act
            var result = xmlLayout.Render(logEventInfo);

            // Assert
            const string expected = @"<logevent><message>Monster massage</message><nlogPropertyKey><node value=""Hello""/><node value=""World""/></nlogPropertyKey></logevent>";
            Assert.Equal(expected, result);
        }

        [Fact]
        public void XmlLayout_PropertiesElementNameDefault_Properties_RenderPropertyObject()
        {
            // Arrange
            var xmlLayout = new XmlLayout()
            {
                Elements = { new XmlElement("message", "${message}") },
                IncludeEventProperties = true,
            };

            var guid = Guid.NewGuid();
            var logEventInfo = new LogEventInfo
            {
                Message = "Monster massage"
            };
            logEventInfo.Properties["nlogPropertyKey"] = new { Id = guid, Name = "Hello World", Elements = new[] { "Earth", "Wind", "Fire" } };

            // Act
            var result = xmlLayout.Render(logEventInfo);

            // Assert
            string expected = @"<logevent><message>Monster massage</message><property key=""nlogPropertyKey""><property key=""Id"">" + guid.ToString() + @"</property><property key=""Name"">Hello World</property><property key=""Elements""><item>Earth</item><item>Wind</item><item>Fire</item></property></property></logevent>";
            Assert.Equal(expected, result);
        }

        [Fact]
        public void XmlLayout_PropertiesElementNameFormat_RenderPropertyObject()
        {
            // Arrange
            var xmlLayout = new XmlLayout()
            {
                Elements = { new XmlElement("message", "${message}") },
                PropertiesElementName = "{0}",
                PropertiesElementKeyAttribute = "",
                IncludeEventProperties = true,
            };

            var guid = Guid.NewGuid();

            var logEventInfo = new LogEventInfo
            {
                Message = "Monster massage"
            };
            logEventInfo.Properties["nlogPropertyKey"] = new { Id = guid, Name = "Hello World", Elements = new[] { "Earth", "Wind", "Fire" } };

            // Act
            var result = xmlLayout.Render(logEventInfo);

            // Assert
            string expected = @"<logevent><message>Monster massage</message><nlogPropertyKey><Id>" + guid.ToString() + @"</Id><Name>Hello World</Name><Elements><item>Earth</item><item>Wind</item><item>Fire</item></Elements></nlogPropertyKey></logevent>";
            Assert.Equal(expected, result);
        }

#if !NET35 && !NET40

        [Fact]
        public void XmlLayout_PropertiesElementNameDefault_Properties_RenderPropertyExpando()
        {
            // Arrange
            var xmlLayout = new XmlLayout()
            {
                Elements = { new XmlElement("message", "${message}") },
                IncludeEventProperties = true,
            };

            dynamic object1 = new System.Dynamic.ExpandoObject();
            object1.Id = 123;
            object1.Name = "test name";

            var logEventInfo = new LogEventInfo
            {
                Message = "Monster massage"
            };
            logEventInfo.Properties["nlogPropertyKey"] = object1;

            // Act
            var result = xmlLayout.Render(logEventInfo);

            // Assert
            const string expected = @"<logevent><message>Monster massage</message><property key=""nlogPropertyKey""><property key=""Id"">123</property><property key=""Name"">test name</property></property></logevent>";
            Assert.Equal(expected, result);
        }

#endif

        [Fact]
        public void XmlLayout_PropertiesElementNameFormat_RenderInfiniteLoop()
        {
            // Arrange
            var xmlLayout = new XmlLayout()
            {
                Elements = { new XmlElement("message", "${message}") },
                PropertiesElementName = "{0}",
                PropertiesElementKeyAttribute = "",
                IncludeEventProperties = true,
                MaxRecursionLimit = 10,
            };

            var logEventInfo = new LogEventInfo
            {
                Message = "Monster massage"
            };
            logEventInfo.Properties["nlogPropertyKey"] = new TestList();

            // Act
            var result = xmlLayout.Render(logEventInfo);

            // Assert
            var cnt = System.Text.RegularExpressions.Regex.Matches(result, "<item>alpha</item><item>bravo</item>").Count;
            Assert.Equal(10, cnt);
        }

        [Fact]
        public void XmlLayout_PropertiesElementNameFormat_RenderTrickyDictionary()
        {
            // Arrange
            var xmlLayout = new XmlLayout()
            {
                Elements = { new XmlElement("message", "${message}") },
                PropertiesElementName = "{0}",
                PropertiesElementKeyAttribute = "",
                IncludeEventProperties = true,
                MaxRecursionLimit = 10,
            };

            var logEventInfo = new LogEventInfo
            {
                Message = "Monster massage"
            };
            IDictionary<object, object> testDictionary = new Internal.TrickyTestDictionary();
            testDictionary.Add("key1", 13);
            testDictionary.Add("key 2", 1.3m);
            logEventInfo.Properties["nlogPropertyKey"] = testDictionary;

            // Act
            var result = xmlLayout.Render(logEventInfo);

            // Assert
            const string expected = @"<logevent><message>Monster massage</message><nlogPropertyKey><key1>13</key1><key_2>1.3</key_2></nlogPropertyKey></logevent>";
            Assert.Equal(expected, result);
        }

        private class TestList : IEnumerable<System.Collections.IEnumerable>
        {
            static List<int> _list1 = new List<int> { 17, 3 };
            static List<string> _list2 = new List<string> { "alpha", "bravo" };

            public IEnumerator<System.Collections.IEnumerable> GetEnumerator()
            {
                yield return _list1;
                yield return _list2;
                yield return new TestList();
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public override bool Equals(object obj)
            {
                throw new Exception("object.Equals should never be called");
            }

            public override int GetHashCode()
            {
                throw new Exception("GetHashCode should never be called");
            }
        }
    }
}
