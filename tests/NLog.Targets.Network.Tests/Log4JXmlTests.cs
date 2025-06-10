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
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Text;
    using System.Xml;
    using NLog.Layouts;
    using NLog.Targets;
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
                  <targets async='true'>
                    <target name='debug' type='Debug'>
                      <layout type='Log4JXmlEventLayout'
                              includeCallSite='true'
                              includeSourceInfo='true'
                              includeScopeProperties='true'
                              includeEventProperties='true'
                              includeNdc='true'
                              ndcItemSeparator='::' />
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
            logFactory.Flush();

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
                                var eventTime = logEventInfo.TimeStamp.Date.AddHours(logEventInfo.TimeStamp.Hour).AddMinutes(logEventInfo.TimeStamp.Minute).AddSeconds(logEventInfo.TimeStamp.Second).AddMilliseconds(logEventInfo.TimeStamp.Millisecond).ToUniversalTime();
                                Assert.Equal(eventTime, epochStart.AddMilliseconds(timestamp));
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
                                        var expectedAppInfo = string.Format(CultureInfo.InvariantCulture, "{0}({1})", AppDomain.CurrentDomain.FriendlyName, System.Diagnostics.Process.GetCurrentProcess().Id);
                                        Assert.Equal(expectedAppInfo, value);
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
                IncludeEventProperties = false,
                Parameters =
                {
                    new Log4JXmlEventParameter
                    {
                        Name = "mt",
                        Layout = "${event-properties:planet}",
                    },
                },
            };
            log4jLayout.AppInfo = "MyApp";
            var logEventInfo = new LogEventInfo
            {
                LoggerName = "MyLOgger",
                TimeStamp = new DateTime(2010, 01, 01, 12, 34, 56, DateTimeKind.Utc),
                Level = LogLevel.Info,
                Message = "hello, {planet}",
                Parameters = new[] { "<earth>" }
            };

            var threadid = Environment.CurrentManagedThreadId;
            var machinename = Environment.MachineName;
            var result = log4jLayout.Render(logEventInfo);
            Assert.Equal($"<log4j:event logger=\"MyLOgger\" level=\"INFO\" timestamp=\"1262349296000\" thread=\"{threadid}\"><log4j:message>hello, &lt;earth&gt;</log4j:message><log4j:properties><log4j:data name=\"log4japp\" value=\"MyApp\"/><log4j:data name=\"log4jmachinename\" value=\"{machinename}\"/><log4j:data name=\"mt\" value=\"&lt;earth&gt;\"/></log4j:properties></log4j:event>", result);

            var logEventInfo2 = new LogEventInfo
            {
                LoggerName = "MyLOgger",
                TimeStamp = new DateTime(2010, 01, 01, 12, 34, 56, DateTimeKind.Utc),
                Level = LogLevel.Info,
                Message = "hello, <earth>",
            };

            var result2 = log4jLayout.Render(logEventInfo2);
            Assert.Equal($"<log4j:event logger=\"MyLOgger\" level=\"INFO\" timestamp=\"1262349296000\" thread=\"{threadid}\"><log4j:message>hello, &lt;earth&gt;</log4j:message><log4j:properties><log4j:data name=\"log4japp\" value=\"MyApp\"/><log4j:data name=\"log4jmachinename\" value=\"{machinename}\"/></log4j:properties></log4j:event>", result2);
        }

        [Fact]
        public void Log4JXmlEventLayout_ThrowableWrappedInCData_Test()
        {
            var log4jLayout = new Log4JXmlEventLayout
            {
                WriteThrowableCData = true,
                AppInfo = "TestApp",
            };

            var logEventInfo = new LogEventInfo
            {
                LoggerName = "TestLogger",
                TimeStamp = new DateTime(2010, 01, 01, 12, 34, 56, DateTimeKind.Utc),
                Level = LogLevel.Error,
                Message = "Error occurred",
                Exception = new Exception("Something went wrong <>&")
            };

            var threadid = Environment.CurrentManagedThreadId;
            var machinename = Environment.MachineName;
            var result = log4jLayout.Render(logEventInfo);
            Assert.Equal($"<log4j:event logger=\"TestLogger\" level=\"ERROR\" timestamp=\"1262349296000\" thread=\"{threadid}\"><log4j:message>Error occurred</log4j:message><log4j:throwable><![CDATA[System.Exception: Something went wrong <>&]]></log4j:throwable><log4j:properties><log4j:data name=\"log4japp\" value=\"TestApp\"/><log4j:data name=\"log4jmachinename\" value=\"{machinename}\"/></log4j:properties></log4j:event>", result);
        }


        public readonly struct PathString
        {
            public PathString(string value)
            {
                Value = value;
            }

            public string Value { get; }
            public bool HasValue
            {
                get { return !string.IsNullOrEmpty(Value); }
            }

            public override string ToString()
            {
                return Value ?? string.Empty;
            }
        }

        [Fact]
        public void Log4JXmlEventLayout_IncludeEventProperties_Test()
        {
            var log4jLayout = new Log4JXmlEventLayout
            {
                IncludeEventProperties = true,
                AppInfo = "MyApp",
            };

            var pathString = new PathString("/test");
            var logEvent = LogEventInfo.Create(LogLevel.Info, "TestLogger", null, "Test Message {Path}", new object[] { pathString });
            logEvent.TimeStamp = new DateTime(2010, 01, 01, 12, 34, 56, DateTimeKind.Utc);
            var result = log4jLayout.Render(logEvent);

            var threadid = Environment.CurrentManagedThreadId;
            var machinename = Environment.MachineName;

            Assert.Equal($"<log4j:event logger=\"TestLogger\" level=\"INFO\" timestamp=\"1262349296000\" thread=\"{threadid}\"><log4j:message>Test Message /test</log4j:message><log4j:properties><log4j:data name=\"log4japp\" value=\"MyApp\"/><log4j:data name=\"log4jmachinename\" value=\"{machinename}\"/><log4j:data name=\"Path\" value=\"/test\"/></log4j:properties></log4j:event>", result);
        }

        [Fact]
        public void Log4JXmlEventLayout_IncludeScopeNested_Test()
        {
            var log4jLayout = new Log4JXmlEventLayout
            {
                IncludeScopeNested = true,
                ScopeNestedSeparator = "::",
                AppInfo = "MyApp",
            };

            ScopeContext.Clear();
            ScopeContext.PushNestedState("One");
            ScopeContext.PushNestedState("Two");
            ScopeContext.PushNestedState("Three");

            var logEventInfo = new LogEventInfo
            {
                LoggerName = "TestLogger",
                TimeStamp = new DateTime(2010, 01, 01, 12, 34, 56, DateTimeKind.Utc),
                Level = LogLevel.Info,
                Message = "Nested scope log test"
            };

            var threadid = Environment.CurrentManagedThreadId;
            var machinename = Environment.MachineName;
            var result = log4jLayout.Render(logEventInfo);
            Assert.Equal($"<log4j:event logger=\"TestLogger\" level=\"INFO\" timestamp=\"1262349296000\" thread=\"{threadid}\"><log4j:message>Nested scope log test</log4j:message><log4j:NDC>One::Two::Three</log4j:NDC><log4j:properties><log4j:data name=\"log4japp\" value=\"MyApp\"/><log4j:data name=\"log4jmachinename\" value=\"{machinename}\"/></log4j:properties></log4j:event>", result);
        }

        [Fact]
        public void Log4JXmlEventLayout_ThrowableWithoutCData_EncodesCorrectly()
        {
            var log4jLayout = new Log4JXmlEventLayout
            {
                WriteThrowableCData = false,
                AppInfo = "TestApp",
            };

            var logEvent = new LogEventInfo(LogLevel.Error, "TestLogger", "Error occurred")
            {
                LoggerName = "TestLogger",
                TimeStamp = new DateTime(2010, 01, 01, 12, 34, 56, DateTimeKind.Utc),
                Level = LogLevel.Error,
                Exception = new Exception("Boom < & >")
            };

            var threadid = Environment.CurrentManagedThreadId;
            var machinename = Environment.MachineName;
            var result = log4jLayout.Render(logEvent);
            Assert.Equal($"<log4j:event logger=\"TestLogger\" level=\"ERROR\" timestamp=\"1262349296000\" thread=\"{threadid}\"><log4j:message>Error occurred</log4j:message><log4j:throwable>System.Exception: Boom &lt; &amp; &gt;</log4j:throwable><log4j:properties><log4j:data name=\"log4japp\" value=\"TestApp\"/><log4j:data name=\"log4jmachinename\" value=\"{machinename}\"/></log4j:properties></log4j:event>", result);
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
