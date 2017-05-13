// 
// Copyright (c) 2004-2016 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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

namespace NLog.UnitTests.LayoutRenderers
{
    using System.Threading;
    using System.Diagnostics;
    using System;
    using System.Xml;
    using System.Reflection;
    using System.IO;
    using Xunit;
    using NLog.Internal;

    public class Log4JXmlTests : NLogTestBase
    {
        [Fact]
        public void Log4JXmlTest()
        {
            LogManager.Configuration = CreateConfigurationFromString(@"
            <nlog throwExceptions='true'>
                <targets>
        <target name='debug' type='Debug' layout='${log4jxmlevent:includeCallSite=true:includeSourceInfo=true:includeMdc=true:includeMdlc=true:includeMdlc=true:IncludeAllProperties=true:ndcItemSeparator=\:\::includenlogdata=true}' />
       </targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>");

            MappedDiagnosticsContext.Clear();
            NestedDiagnosticsContext.Clear();

            MappedDiagnosticsContext.Set("foo1", "bar1");
            MappedDiagnosticsContext.Set("foo2", "bar2");

#if NET4_0 || NET4_5
            MappedDiagnosticsLogicalContext.Clear();
            MappedDiagnosticsLogicalContext.Set("foo3", "bar3");
#endif
            NestedDiagnosticsContext.Push("baz1");
            NestedDiagnosticsContext.Push("baz2");
            NestedDiagnosticsContext.Push("baz3");

            ILogger logger = LogManager.GetLogger("A");
            var logEventInfo = LogEventInfo.Create(LogLevel.Debug, "A", new Exception("Hello Exception", new Exception("Goodbye Exception")), null, "some message");
            logEventInfo.Properties["nlogPropertyKey"] = "nlogPropertyValue";
            logger.Log(logEventInfo);
            string result = GetDebugLastMessage("debug");
            string wrappedResult = "<log4j:dummyRoot xmlns:log4j='http://log4j' xmlns:nlog='http://nlog'>" + result + "</log4j:dummyRoot>";

            Assert.NotEqual("", result);
            // make sure the XML can be read back and verify some fields
            StringReader stringReader = new StringReader(wrappedResult);
            using (XmlReader reader = XmlReader.Create(stringReader))
            {
                while (reader.Read())
                {
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

                                Assert.Equal(Thread.CurrentThread.ManagedThreadId.ToString(), reader.GetAttribute("thread"));
                                break;

                            case "message":
                                reader.Read();
                                Assert.Equal("some message", reader.Value);
                                break;

                            case "NDC":
                                reader.Read();
                                Assert.Equal("baz3::baz2::baz1", reader.Value);
                                break;

                            case "locationInfo":
                                Assert.Equal(MethodBase.GetCurrentMethod().DeclaringType.FullName, reader.GetAttribute("class"));
                                Assert.Equal(MethodBase.GetCurrentMethod().ToString(), reader.GetAttribute("method"));
                                break;

                            case "properties":
                                break;

                            case "throwable":
                                reader.Read();
                                Assert.True(reader.Value.Contains("Hello Exception"));
                                Assert.True(reader.Value.Contains("Goodbye Exception"));
                                break;
                            case "data":
                                string name = reader.GetAttribute("name");
                                string value = reader.GetAttribute("value");

                                switch (name)
                                {
                                    case "log4japp":
                                        Assert.Equal(AppDomain.CurrentDomain.FriendlyName + "(" + Process.GetCurrentProcess().Id + ")", value);
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

#if NET4_0 || NET4_5
                                    case "foo3":
                                        Assert.Equal("bar3", value);
                                        break;
#endif

                                    case "nlogPropertyKey":
                                        Assert.Equal("nlogPropertyValue", value);
                                        break;

                                    default:
                                        Assert.True(false, "Unknown <log4j:data>: " + name);
                                        break;
                                }
                                break;

                            default:
                                throw new NotSupportedException("Unknown element: " + reader.LocalName);
                        }
                        continue;
                    }

                    if (reader.NodeType == XmlNodeType.Element && reader.Prefix == "nlog")
                    {
                        switch (reader.LocalName)
                        {
                            case "eventSequenceNumber":
                                break;

                            case "locationInfo":
                                Assert.Equal(this.GetType().Assembly.FullName, reader.GetAttribute("assembly"));
                                break;

                            case "properties":
                                break;

                            case "data":
                                var name = reader.GetAttribute("name");
                                var value = reader.GetAttribute("value");
                                Assert.Equal("nlogPropertyKey", name);
                                Assert.Equal("nlogPropertyValue", value);
                                break;

                            default:
                                throw new NotSupportedException("Unknown element: " + reader.LocalName);
                        }
                    }
                }
            }
        }

        [Fact]
        void BadXmlValueTest()
        {
            var sb = new System.Text.StringBuilder();

            var forbidden = new System.Collections.Generic.HashSet<int>();
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

            sb.Clear();
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
            Assert.True(badString.Contains("abc"));
            Assert.True(goodString.Contains("abc"));
        }
    }
}