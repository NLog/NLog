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

namespace NLog.Targets.Network
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Text;
    using System.Xml;
    using NLog.LayoutRenderers;
    using NLog.Layouts;
    using NLog.Targets;
    using NLog.Targets.Internal;
    using Xunit;

    public class Log4JXmlTests
    {
        public Log4JXmlTests()
        {
            LogManager.ThrowExceptions = true;
            LogManager.Setup().SetupExtensions(ext =>
            {
                ext.RegisterLayout<Log4JXmlEventLayout>();
            });
        }

        [Fact]
        public void Log4JXmlTest()
        {
            var logFactory = new LogFactory().Setup()
                .LoadConfigurationFromXml(@"
            <nlog throwExceptions='true'>
  <targets>
    <target name='debug' type='Debug'>
<layout type='Log4JXmlEventLayout'
        includeCallSite='true'
        includeSourceInfo='true'
        includeScopeNested='true'
        includeScopeProperties='true'
        includeEventProperties='true'
        ndcItemSeparator='::'
        appInfo='${appdomain}(${processid})' />
    </target>
  </targets>
  <rules>
    <logger name='*' minlevel='Debug' writeTo='debug' />
  </rules>
</nlog>
").LogFactory;

            ScopeContext.Clear();

            ScopeContext.PushProperty("foo1", "bar1");
            ScopeContext.PushProperty("foo2", "bar2");
            ScopeContext.PushProperty("foo3", "bar3");

            ScopeContext.PushNestedState("baz1");
            ScopeContext.PushNestedState("baz2");
            ScopeContext.PushNestedState("baz3");

            var logger = logFactory.GetLogger("A");
            var logEventInfo = LogEventInfo.Create(LogLevel.Debug, "A", new Exception("Hello Exception", new Exception("Goodbye Exception")), null, "some message \u0014");
            logEventInfo.Properties["nlogPropertyKey"] = "nlogPropertyValue";
            logger.Log(logEventInfo);
            string result = logFactory.Configuration.FindTargetByName<DebugTarget>("debug").LastMessage;
            Assert.DoesNotContain("dummy", result);

            string wrappedResult = "<log4j:dummyRoot xmlns:log4j='http://log4j' xmlns:nlog='http://nlog'>" + result + "</log4j:dummyRoot>";

            Assert.NotEqual("", result);
            // make sure the XML can be read back and verify some fields
            StringReader stringReader = new StringReader(wrappedResult);

            var foundsChilds = new Dictionary<string, int>();

            var requiredChilds = new List<string>
            {
                "log4j.event",
                "log4j.message",
                "log4j.NDC",
                "log4j.locationInfo",
                "log4j.properties",
                "log4j.throwable",
                "log4j.data",
            };

            using (XmlReader reader = XmlReader.Create(stringReader))
            {

                while (reader.Read())
                {
                    var key = reader.LocalName;
                    var fullKey = reader.Prefix + "." + key;
                    if (!foundsChilds.ContainsKey(fullKey))
                    {
                        foundsChilds[fullKey] = 0;
                    }
                    foundsChilds[fullKey]++;

                    if (reader.NodeType == XmlNodeType.Element && reader.Prefix == "log4j")
                    {
                        switch (reader.LocalName)
                        {
                            case "dummyRoot":
                                break;

                            case "event":
                                Assert.Equal("DEBUG", reader.GetAttribute("level"));
                                Assert.Equal("A", reader.GetAttribute("logger"));

                                var epochStart = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                                long timestamp = Convert.ToInt64(reader.GetAttribute("timestamp"));
                                var time = epochStart.AddMilliseconds(timestamp);
                                var now = DateTime.UtcNow;
                                Assert.True(now.Ticks - time.Ticks < TimeSpan.FromSeconds(3).Ticks);

                                Assert.Equal(Environment.CurrentManagedThreadId.ToString(), reader.GetAttribute("thread"));
                                break;

                            case "message":
                                reader.Read();
                                Assert.Equal("some message ", reader.Value);
                                break;

                            case "NDC":
                                reader.Read();
                                Assert.Equal("baz1::baz2::baz3", reader.Value);
                                break;

                            case "locationInfo":
                                Assert.Equal(MethodBase.GetCurrentMethod().DeclaringType.FullName, reader.GetAttribute("class"));
                                Assert.Equal(MethodBase.GetCurrentMethod().Name, reader.GetAttribute("method"));
                                break;

                            case "properties":
                                break;

                            case "throwable":
                                reader.Read();
                                Assert.Contains("Hello Exception", reader.Value);
                                Assert.Contains("Goodbye Exception", reader.Value);
                                break;
                            case "data":
                                string name = reader.GetAttribute("name");
                                string value = reader.GetAttribute("value");

                                switch (name)
                                {
                                    case "log4japp":
                                        Assert.Equal(AppDomain.CurrentDomain.FriendlyName + "(" + System.Diagnostics.Process.GetCurrentProcess().Id + ")", value);
                                        break;

                                    case "log4jmachinename":
                                        Assert.Equal(Environment.MachineName, value);
                                        break;

                                    case "foo1":
                                        Assert.Equal("bar1", value);
                                        break;

                                    case "foo2":
                                        Assert.Equal("bar2", value);
                                        break;

                                    case "foo3":
                                        Assert.Equal("bar3", value);
                                        break;

                                    case "nlogPropertyKey":
                                        Assert.Equal("nlogPropertyValue", value);
                                        break;

                                    default:
                                        Assert.Fail("Unknown <log4j:data>: " + name);
                                        break;
                                }
                                break;

                            default:
                                throw new NotSupportedException("Unknown element: " + key);
                        }
                    }
                }
            }

            foreach (var required in requiredChilds)
            {
                Assert.True(foundsChilds.ContainsKey(required), $"{required} not found!");
            }
        }

        [Fact]
        public void Log4JXmlEventLayoutParameterTest()
        {
            var log4jLayout = new Log4JXmlEventLayout()
            {
                Parameters =
                {
                    new Log4JXmlEventParameter
                    {
                        Name = "mt",
                        Layout = "${message:raw=true}",
                    }
                },
            };
            log4jLayout.AppInfo = "MyApp";
            var logEventInfo = new LogEventInfo
            {
                LoggerName = "MyLOgger",
                TimeStamp = new DateTime(2010, 01, 01, 12, 34, 56, DateTimeKind.Utc),
                Level = LogLevel.Info,
                Message = "hello, <{0}>",
                Parameters = new[] { "world" }
            };

            var threadid = Environment.CurrentManagedThreadId;
            var machinename = Environment.MachineName;
            var test = log4jLayout.Render(logEventInfo);
            Assert.Equal($"<log4j:event logger=\"MyLOgger\" level=\"INFO\" timestamp=\"1262349296000\" thread=\"{threadid}\"><log4j:message>hello, &lt;world&gt;</log4j:message><log4j:properties><log4j:data name=\"mt\" value=\"hello, &lt;{{0}}&gt;\"/><log4j:data name=\"log4japp\" value=\"MyApp\"/><log4j:data name=\"log4jmachinename\" value=\"{machinename}\"/></log4j:properties></log4j:event>", log4jLayout.Render(logEventInfo));
        }

        [Fact]
        public void Log4JXmlEventLayout_ThrowableWrappedInCData_Test()
        {
            var log4jLayout = new Log4JXmlEventLayout
            {
                WriteThrowableCData = true,
                AppInfo = "MyApp",
            };

            var logEventInfo = new LogEventInfo
            {
                LoggerName = "TestLogger",
                TimeStamp = new DateTime(2020, 01, 01, 12, 00, 00, DateTimeKind.Utc),
                Level = LogLevel.Error,
                Message = "Test message",
                Exception = new Exception("Something went wrong <>&")
            };

            var result = log4jLayout.Render(logEventInfo);

            Assert.Contains("<![CDATA[", result);
            Assert.Contains("Something went wrong", result);
            Assert.Contains("<log4j:throwable>", result);
            Assert.Contains("</log4j:throwable>", result);
        }


        [Fact]
        public void Log4JXmlEventLayout_ThrowableWithoutCData_EncodesCorrectly()
        {
            var layout = new Log4JXmlEventLayout
            {
                WriteThrowableCData = false,
                AppInfo = "TestApp",
            };

            var logEvent = new LogEventInfo(LogLevel.Error, "TestLogger", "Error occurred")
            {
                Exception = new Exception("Boom < & >")
            };

            string result = layout.Render(logEvent);

            Assert.Contains("<log4j:throwable>", result);
            Assert.DoesNotContain("<![CDATA[", result);
            Assert.Contains("Boom &lt; &amp; &gt;", result); // XML-escaped
            Assert.Contains("</log4j:throwable>", result);
        }




        [Fact(Skip = "Deprecated with new XmlLayout logic. Use Log4JXmlEventLayout_CompliantXml_Test instead.")]
        [Obsolete("This test uses old renderer layout, no longer relevant.")]
        public void BadXmlValueTest()
        {
            var sb = new System.Text.StringBuilder();

            var forbidden = new HashSet<int>();
            int start = 64976; int end = 65007;

            for (int i = start; i <= end; i++)
            {
                forbidden.Add(i);
            }

            forbidden.Add(0xFFFE);
            forbidden.Add(0xFFFF);

            for (int i = char.MinValue; i <= char.MaxValue; i++)
            {
                char c = Convert.ToChar(i);
                if (char.IsSurrogate(c))
                {
                    continue; // skip surrogates
                }

                if (forbidden.Contains(c))
                {
                    continue;
                }

                sb.Append(c);
            }

            var badString = sb.ToString();

            var settings = new XmlWriterSettings
            {
                Indent = true,
                ConformanceLevel = ConformanceLevel.Fragment,
                IndentChars = "  ",
            };

            sb.Length = 0;
            using (XmlWriter xtw = XmlWriter.Create(sb, settings))
            {
                xtw.WriteStartElement("log4j", "event", "http:://hello/");
                xtw.WriteElementSafeString("log4j", "message", "http:://hello/", badString);
                xtw.WriteEndElement();
                xtw.Flush();
            }

            string goodString = null;
            using (XmlReader reader = XmlReader.Create(new StringReader(sb.ToString())))
            {
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Text)
                    {
                        if (reader.Value.Contains("abc"))
                            goodString = reader.Value;
                    }
                }
            }

            Assert.NotNull(goodString);
            Assert.NotEqual(badString.Length, goodString.Length);
            Assert.Contains("abc", badString);
            Assert.Contains("abc", goodString);
        }

        [Fact]
        public void Log4JXmlEventLayout_Should_Remove_InvalidXmlCharacters()
        {
            // Arrange
            var sb = new System.Text.StringBuilder();

            // Build a string with all legal XML characters, plus some "known safe"
            var validContent = new StringBuilder("abc"); // something we can track
            int start = 0; int end = 0xFFFD;
            for (int i = start; i <= end; i++)
            {
                char c = (char)i;
                if (!char.IsSurrogate(c) && XmlConvert.IsXmlChar(c))
                {
                    validContent.Append(c);
                }
            }

            var badString = validContent.ToString();

            var log4jLayout = new Log4JXmlEventLayout()
            {
                FormattedMessage = badString, // assigning directly
            };

            var logEventInfo = new LogEventInfo(LogLevel.Info, "loggerName", badString);

            // Act
            string xmlOutput = log4jLayout.Render(logEventInfo);

            // Assert: Try reading the XML back and verify content
            string wrappedXml = $"<log4j:dummyRoot xmlns:log4j='http://log4j'>{xmlOutput}</log4j:dummyRoot>";
            string goodString = null;

            using (var reader = XmlReader.Create(new StringReader(wrappedXml)))
            {
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Text)
                    {
                        if (reader.Value.Contains("abc"))
                            goodString = reader.Value;
                    }
                }
            }

            Assert.NotNull(goodString);
            Assert.Contains("abc", goodString);
        }

    }
}
