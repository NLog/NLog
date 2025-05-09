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

namespace NLog.UnitTests.Layouts
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using NLog.Config;
    using NLog.Layouts;
    using NLog.Targets;
    using Xunit;

    public class JsonLayoutTests : NLogTestBase
    {
        private const string ExpectedIncludeAllPropertiesWithExcludes = "{\"StringProp\":\"ValueA\",\"IntProp\":123,\"DoubleProp\":123.123,\"DecimalProp\":123.123,\"BoolProp\":true,\"NullProp\":null,\"DateTimeProp\":\"2345-01-23T12:34:56Z\"}";
        private const string ExpectedExcludeEmptyPropertiesWithExcludes = "{\"StringProp\":\"ValueA\",\"IntProp\":123,\"DoubleProp\":123.123,\"DecimalProp\":123.123,\"BoolProp\":true,\"DateTimeProp\":\"2345-01-23T12:34:56Z\",\"NoEmptyProp4\":\"hello\\\"\"}";

        [Fact]
        public void JsonLayoutRendering()
        {
            var jsonLayout = new JsonLayout()
            {
                Attributes =
                    {
                        new JsonAttribute("date", "${longdate}"),
                        new JsonAttribute("level", "${level}"),
                        new JsonAttribute("message", "${message}"),
                    }
            };

            var logEventInfo = new LogEventInfo
            {
                TimeStamp = new DateTime(2010, 01, 01, 12, 34, 56),
                Level = LogLevel.Info,
                Message = "hello, world"
            };

            Assert.Equal("{\"date\":\"2010-01-01 12:34:56.0000\",\"level\":\"Info\",\"message\":\"hello, world\"}", jsonLayout.Render(logEventInfo));
        }

        [Fact]
        public void JsonLayoutRenderingIndentJson()
        {
            var jsonLayout = new JsonLayout()
            {
                Attributes =
                    {
                        new JsonAttribute("date", "${longdate}"),
                        new JsonAttribute("level", "${level}"),
                        new JsonAttribute("message", "${message}"),
                    },
                IndentJson = true
            };

            var logEventInfo = new LogEventInfo
            {
                TimeStamp = new DateTime(2010, 01, 01, 12, 34, 56),
                Level = LogLevel.Info,
                Message = "hello, world"
            };

            Assert.Equal($"{{{Environment.NewLine}  \"date\": \"2010-01-01 12:34:56.0000\",{Environment.NewLine}  \"level\": \"Info\",{Environment.NewLine}  \"message\": \"hello, world\"{Environment.NewLine}}}", jsonLayout.Render(logEventInfo));
        }

        [Fact]
        public void JsonLayoutRenderingNoSpaces()
        {
            var jsonLayout = new JsonLayout()
            {
                Attributes =
                    {
                        new JsonAttribute("date", "${longdate}"),
                        new JsonAttribute("level", "${level}"),
                        new JsonAttribute("message", "${message}"),
                    },
                SuppressSpaces = true
            };

            var logEventInfo = new LogEventInfo
            {
                TimeStamp = new DateTime(2010, 01, 01, 12, 34, 56),
                Level = LogLevel.Info,
                Message = "hello, world"
            };

            Assert.Equal("{\"date\":\"2010-01-01 12:34:56.0000\",\"level\":\"Info\",\"message\":\"hello, world\"}", jsonLayout.Render(logEventInfo));
        }

        [Fact]
        public void JsonLayoutRenderingEscapeUnicode()
        {
            var jsonLayout = new JsonLayout()
            {
                Attributes =
                    {
                        new JsonAttribute("logger", "${logger}") { EscapeUnicode = true },
                        new JsonAttribute("level", "${level}"),
                        new JsonAttribute("message", "${event-properties:msg}") { EscapeUnicode = false },
                    },
                SuppressSpaces = true,
                IncludeEventProperties = true,
            };

            var logEventInfo = LogEventInfo.Create(LogLevel.Info, "\u00a9", null, "{$a}", new object[] { "\\" });
            logEventInfo.Properties["msg"] = "\u00a9";
            Assert.Equal("{\"logger\":\"\\u00a9\",\"level\":\"Info\",\"message\":\"\u00a9\",\"a\":\"\\\\\",\"msg\":\"\u00a9\"}", jsonLayout.Render(logEventInfo));
        }

        [Fact]
        public void JsonLayoutRenderingAndEncodingSpecialCharacters()
        {
            var jsonLayout = new JsonLayout()
            {
                Attributes =
                    {
                        new JsonAttribute("date", "${longdate}"),
                        new JsonAttribute("level", "${level}"),
                        new JsonAttribute("message", "${message}"),
                    }
            };

            var logEventInfo = new LogEventInfo
            {
                TimeStamp = new DateTime(2010, 01, 01, 12, 34, 56),
                Level = LogLevel.Info,
                Message = "\"hello, world\""
            };

            Assert.Equal("{\"date\":\"2010-01-01 12:34:56.0000\",\"level\":\"Info\",\"message\":\"\\\"hello, world\\\"\"}", jsonLayout.Render(logEventInfo));
        }

        [Fact]
        public void JsonLayoutRenderingAndEncodingLineBreaks()
        {
            var jsonLayout = new JsonLayout()
            {
                Attributes =
                    {
                        new JsonAttribute("date", "${longdate}"),
                        new JsonAttribute("level", "${level}"),
                        new JsonAttribute("message", "${message}"),
                    }
            };

            var logEventInfo = new LogEventInfo
            {
                TimeStamp = new DateTime(2010, 01, 01, 12, 34, 56),
                Level = LogLevel.Info,
                Message = "hello,\n\r world"
            };

            Assert.Equal("{\"date\":\"2010-01-01 12:34:56.0000\",\"level\":\"Info\",\"message\":\"hello,\\n\\r world\"}", jsonLayout.Render(logEventInfo));
        }

        [Fact]
        public void JsonLayoutRenderingAndNotEncodingMessageAttribute()
        {
            var jsonLayout = new JsonLayout()
            {
                Attributes =
                    {
                        new JsonAttribute("date", "${longdate}"),
                        new JsonAttribute("level", "${level}"),
                        new JsonAttribute("message", "${message}", false),
                    }
            };

            var logEventInfo = new LogEventInfo
            {
                TimeStamp = new DateTime(2010, 01, 01, 12, 34, 56),
                Level = LogLevel.Info,
                Message = "{ \"hello\" : \"world\" }"
            };

            Assert.Equal("{\"date\":\"2010-01-01 12:34:56.0000\",\"level\":\"Info\",\"message\":{ \"hello\" : \"world\" }}", jsonLayout.Render(logEventInfo));
        }

        [Fact]
        public void JsonLayoutRenderingAndEncodingMessageAttribute()
        {
            var jsonLayout = new JsonLayout()
            {
                Attributes =
                    {
                        new JsonAttribute("date", "${longdate}"),
                        new JsonAttribute("level", "${level}"),
                        new JsonAttribute("message", "${message}"),
                    }
            };

            var logEventInfo = new LogEventInfo
            {
                TimeStamp = new DateTime(2010, 01, 01, 12, 34, 56),
                Level = LogLevel.Info,
                Message = "{ \"hello\" : \"world\" }"
            };

            Assert.Equal("{\"date\":\"2010-01-01 12:34:56.0000\",\"level\":\"Info\",\"message\":\"{ \\\"hello\\\" : \\\"world\\\" }\"}", jsonLayout.Render(logEventInfo));
        }

        [Fact]
        public void JsonLayoutValueTypeAttribute()
        {
            var jsonLayout = new JsonLayout()
            {
                Attributes =
                    {
                        new JsonAttribute("date", "${longdate}") { ValueType = typeof(DateTime) },
                        new JsonAttribute("level", "${level}"),
                        new JsonAttribute("message", "${message}"),
                    }
            };

            var logEventInfo = new LogEventInfo
            {
                TimeStamp = new DateTime(2010, 01, 01, 12, 34, 56),
                Level = LogLevel.Info,
                Message = "{ \"hello\" : \"world\" }"
            };

            Assert.Equal("{\"date\":\"2010-01-01T12:34:56Z\",\"level\":\"Info\",\"message\":\"{ \\\"hello\\\" : \\\"world\\\" }\"}", jsonLayout.Render(logEventInfo));
        }

        [Fact]
        public void JsonAttributeThreadAgnosticTest()
        {
            var jsonLayout = new JsonLayout()
            {
                Attributes =
                {
                    new JsonAttribute("type", "${exception:format=Type}"),
                    new JsonAttribute("message", "${exception:format=Message}"),
                    new JsonAttribute("threadid", "${threadid}"),
                }
            };

            var logFactory = new LogFactory().Setup().LoadConfiguration(builder =>
            {
                builder.ForLogger().WriteTo(new DebugTarget("debug") { Layout = jsonLayout }).WithAsync();
            }).LogFactory;

            var logger = logFactory.GetLogger("B");

            var logEventInfo = CreateLogEventWithExcluded();

            logger.Debug(logEventInfo);

            var target = logFactory.Configuration.AllTargets.OfType<DebugTarget>().First();
            logFactory.Shutdown();  // Flush

            var message = target.LastMessage;
            Assert.Contains(CurrentManagedThreadId.ToString(), message);

            // Verify that JsonLayout performed minimal thread context capture
            System.Threading.Tasks.Task.Run(() => message = jsonLayout.Attributes.Last().Layout.Render(logEventInfo)).Wait();
            Assert.Equal(CurrentManagedThreadId.ToString(), message);
        }

        [Fact]
        public void JsonAttributeStackTraceUsageTest()
        {
            var logFactory = new LogFactory().Setup().LoadConfigurationFromXml(@"
            <nlog throwExceptions='true'>
            <targets>
                <target name='debug' type='Debug'  >
                 <layout type='JsonLayout'>
                    <attribute name='type' layout='${exception:format=Type}'/>
                    <attribute name='message' layout='${exception:format=Message}'/>
                    <attribute name='className' layout='${callsite:className=true}'/>
                 </layout>
                </target>
            </targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>").LogFactory;

            var logger = logFactory.GetLogger("C");

            var logEventInfo = CreateLogEventWithExcluded();

            logger.Debug(logEventInfo);

            var message = GetDebugLastMessage("debug", logFactory);
            Assert.Contains(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.FullName, message);
        }

        [Fact]
        public void NestedJsonAttrTest()
        {
            var jsonLayout = new JsonLayout
            {
                Attributes =
                {
                    new JsonAttribute("type", "${exception:format=Type}"),
                    new JsonAttribute("message", "${exception:format=Message}"),
                    new JsonAttribute("innerException", new JsonLayout
                    {

                        Attributes =
                        {
                            new JsonAttribute("type", "${exception:format=:innerFormat=Type:MaxInnerExceptionLevel=1:InnerExceptionSeparator=}"),
                            new JsonAttribute("message", "${exception:format=:innerFormat=Message:MaxInnerExceptionLevel=1:InnerExceptionSeparator=}"),
                        }
                    },
                    //don't escape layout
                    false)
                }
            };

            var logEventInfo = new LogEventInfo
            {
                Exception = new NLogRuntimeException("test", new NullReferenceException("null is bad!"))
            };

            var json = jsonLayout.Render(logEventInfo);
            Assert.Equal("{\"type\":\"NLog.NLogRuntimeException\",\"message\":\"test\",\"innerException\":{\"type\":\"System.NullReferenceException\",\"message\":\"null is bad!\"}}", json);
        }

        [Fact]
        public void NestedJsonAttrDoesNotRenderEmptyLiteralIfRenderEmptyObjectIsFalseTest()
        {
            var jsonLayout = new JsonLayout
            {
                Attributes =
                {
                    new JsonAttribute("type", "${exception:format=Type}"),
                    new JsonAttribute("message", "${exception:format=Message}"),
                    new JsonAttribute("innerException", new JsonLayout
                    {

                        Attributes =
                        {
                            new JsonAttribute("type", "${exception:format=:innerFormat=Type:MaxInnerExceptionLevel=1:InnerExceptionSeparator=}"),
                            new JsonAttribute("message", "${exception:format=:innerFormat=Message:MaxInnerExceptionLevel=1:InnerExceptionSeparator=}"),
                        },
                        RenderEmptyObject = false
                    },
                    //don't escape layout
                    false)
                }
            };

            var logEventInfo = new LogEventInfo
            {
                Exception = new NLogRuntimeException("test", (Exception)null)
            };

            var json = jsonLayout.Render(logEventInfo);
            Assert.Equal("{\"type\":\"NLog.NLogRuntimeException\",\"message\":\"test\"}", json);

        }

        [Fact]
        public void NestedJsonAttrRendersEmptyLiteralIfRenderEmptyObjectIsTrueTest()
        {
            var jsonLayout = new JsonLayout
            {
                Attributes =
                {
                    new JsonAttribute("type", "${exception:format=Type}"),
                    new JsonAttribute("message", "${exception:format=Message}"),
                    new JsonAttribute("innerException", new JsonLayout
                    {

                        Attributes =
                        {
                            new JsonAttribute("type", "${exception:format=:innerFormat=Type:MaxInnerExceptionLevel=1:InnerExceptionSeparator=}"),
                            new JsonAttribute("message", "${exception:format=:innerFormat=Message:MaxInnerExceptionLevel=1:InnerExceptionSeparator=}"),
                        },
                        RenderEmptyObject = true
                    },
                    //don't escape layout
                    false)
                }
            };

            var logEventInfo = new LogEventInfo
            {
                Exception = new NLogRuntimeException("test", (Exception)null)
            };

            var json = jsonLayout.Render(logEventInfo);
            Assert.Equal("{\"type\":\"NLog.NLogRuntimeException\",\"message\":\"test\",\"innerException\":{}}", json);

        }

        [Fact]
        public void NestedJsonAttrTestFromXML()
        {
            var configXml = @"
<nlog>
  <targets>
    <target name='jsonFile' type='File' fileName='log.json'>
      <layout type='JsonLayout'>
        <attribute name='time' layout='${longdate}' />
        <attribute name='level' layout='${level:upperCase=true}'/>
        <attribute name='nested' encode='false'  >
          <layout type='JsonLayout'>
            <attribute name='message' layout='${message}' />
            <attribute name='exception' layout='${exception:message}' />
          </layout>
        </attribute>
      </layout>
    </target>
  </targets>
  <rules>
  </rules>
</nlog>
";

            var config = XmlLoggingConfiguration.CreateFromXmlString(configXml);

            Assert.NotNull(config);
            var target = config.FindTargetByName<FileTarget>("jsonFile");
            Assert.NotNull(target);
            var jsonLayout = target.Layout as JsonLayout;
            Assert.NotNull(jsonLayout);
            var attrs = jsonLayout.Attributes;
            Assert.NotNull(attrs);
            Assert.Equal(3, attrs.Count);
            Assert.Equal(typeof(SimpleLayout), attrs[0].Layout.GetType());
            Assert.Equal(typeof(SimpleLayout), attrs[1].Layout.GetType());
            Assert.Equal(typeof(JsonLayout), attrs[2].Layout.GetType());
            var nestedJsonLayout = (JsonLayout)attrs[2].Layout;
            Assert.Equal(2, nestedJsonLayout.Attributes.Count);
            Assert.Equal("${message}", nestedJsonLayout.Attributes[0].Layout.ToString());
            Assert.Equal("${exception:message}", nestedJsonLayout.Attributes[1].Layout.ToString());

            var logEventInfo = new LogEventInfo
            {
                TimeStamp = new DateTime(2016, 10, 30, 13, 30, 55),
                Message = "this is message",
                Level = LogLevel.Info,
                Exception = new NLogRuntimeException("test", new NullReferenceException("null is bad!"))
            };

            var json = jsonLayout.Render(logEventInfo);
            Assert.Equal("{\"time\":\"2016-10-30 13:30:55.0000\",\"level\":\"INFO\",\"nested\":{\"message\":\"this is message\",\"exception\":\"test\"}}", json);
        }

        [Fact]
        public void IncludeAllJsonProperties()
        {
            var jsonLayout = new JsonLayout()
            {
                IncludeEventProperties = true
            };

            jsonLayout.ExcludeProperties.Add("Excluded1");
            jsonLayout.ExcludeProperties.Add("Excluded2");

            var logEventInfo = CreateLogEventWithExcluded();

            Assert.Equal(ExpectedIncludeAllPropertiesWithExcludes, jsonLayout.Render(logEventInfo));
        }

        [Fact]
        public void PropertyKeyWithQuote()
        {
            var jsonLayout = new JsonLayout()
            {
                IncludeEventProperties = true,
            };

            var logEventInfo = new LogEventInfo();
            logEventInfo.Properties.Add(@"fo""o", "bar");
            Assert.Equal(@"{""fo\""o"":""bar""}", jsonLayout.Render(logEventInfo));
        }

        [Fact]
        public void AttributerKeyWithQuote()
        {
            var jsonLayout = new JsonLayout();
            jsonLayout.Attributes.Add(new JsonAttribute(@"fo""o", "bar"));

            Assert.Equal(@"{""fo\""o"":""bar""}", jsonLayout.Render(LogEventInfo.CreateNullEvent()));
        }

        [Fact]
        public void ExcludeEmptyJsonProperties()
        {
            var jsonLayout = new JsonLayout()
            {
                IncludeEventProperties = true,
                ExcludeEmptyProperties = true
            };

            jsonLayout.ExcludeProperties.Add("Excluded1");
            jsonLayout.ExcludeProperties.Add("Excluded2");

            var logEventInfo = CreateLogEventWithExcluded();
            logEventInfo.Properties.Add("EmptyProp", "");
            logEventInfo.Properties.Add("EmptyProp1", null);
            logEventInfo.Properties.Add("EmptyProp2", new DummyContextLogger() { Value = null });
            logEventInfo.Properties.Add("EmptyProp3", new DummyContextLogger() { Value = "" });
            logEventInfo.Properties.Add("NoEmptyProp4", new DummyContextLogger() { Value = "hello\"" });

            Assert.Equal(ExpectedExcludeEmptyPropertiesWithExcludes, jsonLayout.Render(logEventInfo));
        }

        [Fact]
        public void ExcludeEmptyJsonStructuredProperties()
        {
            var jsonLayout = new JsonLayout()
            {
                IncludeEventProperties = true,
                ExcludeEmptyProperties = true
            };

            jsonLayout.ExcludeProperties.Add("Excluded1");
            jsonLayout.ExcludeProperties.Add("Excluded2");

            var logEventInfo = CreateLogEventWithExcluded();
            logEventInfo.Properties.Add("EmptyProp2", new DummyContextLogger() { Value = null });
            logEventInfo.Properties.Add("EmptyProp3", new DummyContextLogger() { Value = "" });
            logEventInfo.Properties.Add("NoEmptyProp4", new DummyContextLogger() { Value = "hello\"" });

            var structuredLogEvent = new LogEventInfo(LogLevel.Info, "", null, "{@EmptyProp1} {$EmptyProp}", new[] { null, "" });
            foreach (var property in logEventInfo.Properties)
            {
                structuredLogEvent.Properties[property.Key] = property.Value;
            }

            Assert.Equal(ExpectedExcludeEmptyPropertiesWithExcludes, jsonLayout.Render(structuredLogEvent));
        }

        [Fact]
        public void IncludeAllJsonPropertiesMaxRecursionLimit()
        {
            var jsonLayout = new JsonLayout()
            {
                IncludeEventProperties = true,
                MaxRecursionLimit = 1,
            };

            LogEventInfo logEventInfo = new LogEventInfo()
            {
                TimeStamp = new DateTime(2010, 01, 01, 12, 34, 56),
                Level = LogLevel.Info,
            };
            logEventInfo.Properties["Message"] = new
            {
                data = new Dictionary<int, string>() { { 42, "Hello" } }
            };

            Assert.Equal(@"{""Message"":{""data"":{}}}", jsonLayout.Render(logEventInfo));
        }

        [Fact]
        [Obsolete("Replaced by ScopeContext.PushProperty or Logger.PushScopeProperty using ${scopeproperty}. Marked obsolete on NLog 5.0")]
        public void IncludeMdcJsonProperties()
        {
            var logFactory = new LogFactory().Setup().LoadConfigurationFromXml(@"
            <nlog throwExceptions='true'>
            <targets>
                <target name='asyncDebug' type='AsyncWrapper' timeToSleepBetweenBatches='0'>
                <target name='debug' type='Debug'  >
                 <layout type=""JsonLayout"" IncludeMdc='true' ExcludeProperties='Excluded1,Excluded2'>
                 </layout>
                </target>
                </target>
            </targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='asyncDebug' />
                </rules>
            </nlog>").LogFactory;

            var logger = logFactory.GetLogger("A");

            var logEventInfo = CreateLogEventWithExcluded();

            MappedDiagnosticsContext.Clear();
            foreach (var prop in logEventInfo.Properties)
                if (prop.Key.ToString() != "Excluded1" && prop.Key.ToString() != "Excluded2")
                    MappedDiagnosticsContext.Set(prop.Key.ToString(), prop.Value);
            logEventInfo.Properties.Clear();

            logger.Debug(logEventInfo);

            logFactory.Flush();

            logFactory.AssertDebugLastMessage(ExpectedIncludeAllPropertiesWithExcludes);
        }

        [Fact]
        [Obsolete("Replaced by ScopeContext.PushProperty or Logger.PushScopeProperty using ${scopeproperty}. Marked obsolete on NLog 5.0")]
        public void IncludeMdcNoEmptyJsonProperties()
        {
            var logFactory = new LogFactory().Setup().LoadConfigurationFromXml(@"
            <nlog throwExceptions='true'>
            <targets>
                <target name='asyncDebug' type='AsyncWrapper' timeToSleepBetweenBatches='0'>
                <target name='debug' type='Debug'  >
                 <layout type=""JsonLayout"" IncludeMdc='true' ExcludeProperties='Excluded1,Excluded2' ExcludeEmptyProperties='true'>
                 </layout>
                </target>
                </target>
            </targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='asyncDebug' />
                </rules>
            </nlog>").LogFactory;

            ILogger logger = logFactory.GetLogger("A");

            var logEventInfo = CreateLogEventWithExcluded();
            logEventInfo.Properties.Add("EmptyProp", "");
            logEventInfo.Properties.Add("EmptyProp1", null);
            logEventInfo.Properties.Add("EmptyProp2", new DummyContextLogger() { Value = null });
            logEventInfo.Properties.Add("EmptyProp3", new DummyContextLogger() { Value = "" });
            logEventInfo.Properties.Add("NoEmptyProp4", new DummyContextLogger() { Value = "hello\"" });

            MappedDiagnosticsContext.Clear();
            foreach (var prop in logEventInfo.Properties)
                if (prop.Key.ToString() != "Excluded1" && prop.Key.ToString() != "Excluded2")
                    MappedDiagnosticsContext.Set(prop.Key.ToString(), prop.Value);
            logEventInfo.Properties.Clear();

            logger.Debug(logEventInfo);

            logFactory.Flush();

            logFactory.AssertDebugLastMessage(ExpectedExcludeEmptyPropertiesWithExcludes);
        }

        [Fact]
        public void IncludeGdcJsonProperties()
        {
            var logFactory = new LogFactory().Setup().LoadConfigurationFromXml(@"
            <nlog throwExceptions='true'>
            <targets>
                <target name='asyncDebug' type='AsyncWrapper' timeToSleepBetweenBatches='0'>
                <target name='debug' type='Debug'  >
                 <layout type=""JsonLayout"" IncludeGdc='true' ExcludeProperties='Excluded1,Excluded2'>
                 </layout>
                </target>
                </target>
            </targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='asyncDebug' />
                </rules>
            </nlog>").LogFactory;

            var logger = logFactory.GetLogger("A");

            var logEventInfo = CreateLogEventWithExcluded();

            GlobalDiagnosticsContext.Clear();
            foreach (var prop in logEventInfo.Properties)
                if (prop.Key.ToString() != "Excluded1" && prop.Key.ToString() != "Excluded2")
                    GlobalDiagnosticsContext.Set(prop.Key.ToString(), prop.Value);
            logEventInfo.Properties.Clear();

            logger.Debug(logEventInfo);

            logFactory.Flush();

            logFactory.AssertDebugLastMessage(ExpectedIncludeAllPropertiesWithExcludes);
        }

        [Fact]
        public void IncludeGdcNoEmptyJsonProperties()
        {
            var logFactory = new LogFactory().Setup().LoadConfigurationFromXml(@"
            <nlog throwExceptions='true'>
            <targets>
                <target name='asyncDebug' type='AsyncWrapper' timeToSleepBetweenBatches='0'>
                <target name='debug' type='Debug'  >
                 <layout type=""JsonLayout"" IncludeGdc='true' ExcludeProperties='Excluded1,Excluded2' ExcludeEmptyProperties='true'>
                 </layout>
                </target>
                </target>
            </targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='asyncDebug' />
                </rules>
            </nlog>").LogFactory;

            ILogger logger = logFactory.GetLogger("A");

            var logEventInfo = CreateLogEventWithExcluded();
            logEventInfo.Properties.Add("EmptyProp", "");
            logEventInfo.Properties.Add("EmptyProp1", null);
            logEventInfo.Properties.Add("EmptyProp2", new DummyContextLogger() { Value = null });
            logEventInfo.Properties.Add("EmptyProp3", new DummyContextLogger() { Value = "" });
            logEventInfo.Properties.Add("NoEmptyProp4", new DummyContextLogger() { Value = "hello\"" });

            GlobalDiagnosticsContext.Clear();
            foreach (var prop in logEventInfo.Properties)
                if (prop.Key.ToString() != "Excluded1" && prop.Key.ToString() != "Excluded2")
                    GlobalDiagnosticsContext.Set(prop.Key.ToString(), prop.Value);
            logEventInfo.Properties.Clear();

            logger.Debug(logEventInfo);

            logFactory.Flush();

            logFactory.AssertDebugLastMessage(ExpectedExcludeEmptyPropertiesWithExcludes);
        }

        [Fact]
        [Obsolete("Replaced by ScopeContext.PushProperty or Logger.PushScopeProperty using ${scopeproperty}. Marked obsolete on NLog 5.0")]
        public void IncludeMdlcJsonProperties()
        {
            var logFactory = new LogFactory().Setup().LoadConfigurationFromXml(@"
            <nlog throwExceptions='true'>
            <targets>
                <target name='asyncDebug' type='AsyncWrapper' timeToSleepBetweenBatches='0'>
                <target name='debug' type='Debug'  >
                 <layout type=""JsonLayout"" IncludeMdlc='true' ExcludeProperties='Excluded1,Excluded2'>
                 </layout>
                </target>
                </target>
            </targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='asyncDebug' />
                </rules>
            </nlog>").LogFactory;

            var logger = logFactory.GetLogger("A");

            var logEventInfo = CreateLogEventWithExcluded();

            MappedDiagnosticsLogicalContext.Clear();
            foreach (var prop in logEventInfo.Properties)
                if (prop.Key.ToString() != "Excluded1" && prop.Key.ToString() != "Excluded2")
                    MappedDiagnosticsLogicalContext.Set(prop.Key.ToString(), prop.Value);
            logEventInfo.Properties.Clear();

            logger.Debug(logEventInfo);

            logFactory.Flush();

            logFactory.AssertDebugLastMessage(ExpectedIncludeAllPropertiesWithExcludes);
        }

        [Fact]
        [Obsolete("Replaced by ScopeContext.PushProperty or Logger.PushScopeProperty using ${scopeproperty}. Marked obsolete on NLog 5.0")]
        public void IncludeMdlcNoEmptyJsonProperties()
        {
            var logFactory = new LogFactory().Setup().LoadConfigurationFromXml(@"
            <nlog throwExceptions='true'>
            <targets>
                <target name='asyncDebug' type='AsyncWrapper' timeToSleepBetweenBatches='0'>
                <target name='debug' type='Debug'  >
                 <layout type=""JsonLayout"" IncludeMdlc='true' ExcludeProperties='Excluded1,Excluded2' ExcludeEmptyProperties='true'>
                 </layout>
                </target>
                </target>
            </targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='asyncDebug' />
                </rules>
            </nlog>").LogFactory;

            ILogger logger = logFactory.GetLogger("A");

            var logEventInfo = CreateLogEventWithExcluded();
            logEventInfo.Properties.Add("EmptyProp", "");
            logEventInfo.Properties.Add("EmptyProp1", null);
            logEventInfo.Properties.Add("EmptyProp2", new DummyContextLogger() { Value = null });
            logEventInfo.Properties.Add("EmptyProp3", new DummyContextLogger() { Value = "" });
            logEventInfo.Properties.Add("NoEmptyProp4", new DummyContextLogger() { Value = "hello\"" });

            MappedDiagnosticsLogicalContext.Clear();
            foreach (var prop in logEventInfo.Properties)
                if (prop.Key.ToString() != "Excluded1" && prop.Key.ToString() != "Excluded2")
                    MappedDiagnosticsLogicalContext.Set(prop.Key.ToString(), prop.Value);
            logEventInfo.Properties.Clear();

            logger.Debug(logEventInfo);

            logFactory.Flush();

            logFactory.AssertDebugLastMessage(ExpectedExcludeEmptyPropertiesWithExcludes);
        }

        [Fact]
        [Obsolete("Replaced by ScopeContext.PushProperty or Logger.PushScopeProperty using ${scopeproperty}. Marked obsolete on NLog 5.0")]
        public void IncludeMdlcJsonNestedProperties()
        {
            var logFactory = new LogFactory().Setup().LoadConfigurationFromXml(@"
            <nlog throwExceptions='true'>
            <targets>
                <target name='asyncDebug' type='AsyncWrapper' timeToSleepBetweenBatches='0'>
                    <target name='debug' type='Debug'>
                        <layout type='JsonLayout'>
                            <attribute name='scope' encode='false' >
                                <layout type='JsonLayout' includeMdlc='true' />
                            </attribute>
                        </layout>
                    </target>
                </target>
                </targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='asyncDebug' />
                </rules>
            </nlog>").LogFactory;

            var logger = logFactory.GetLogger("A");

            var logEventInfo = CreateLogEventWithExcluded();

            MappedDiagnosticsLogicalContext.Clear();
            foreach (var prop in logEventInfo.Properties)
                if (prop.Key.ToString() != "Excluded1" && prop.Key.ToString() != "Excluded2")
                    MappedDiagnosticsLogicalContext.Set(prop.Key.ToString(), prop.Value);
            logEventInfo.Properties.Clear();

            logger.Debug(logEventInfo);

            logFactory.Flush();

            logFactory.AssertDebugLastMessage(@"{""scope"":" + ExpectedIncludeAllPropertiesWithExcludes + "}");
        }

        /// <summary>
        /// Test from XML, needed for the list (ExcludeProperties)
        /// </summary>
        [Fact]
        public void IncludeAllJsonPropertiesXml()
        {
            var logFactory = new LogFactory().Setup().LoadConfigurationFromXml(@"
            <nlog throwExceptions='true'>
            <targets>
                <target name='debug' type='Debug'  >
                 <layout type=""JsonLayout"" IncludeAllProperties='true' ExcludeProperties='Excluded1,EXCLUDED2'>

                 </layout>
                </target>
            </targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>").LogFactory;

            var logger = logFactory.GetLogger("A");

            var logEventInfo = CreateLogEventWithExcluded();

            logger.Debug(logEventInfo);

            logFactory.AssertDebugLastMessage(ExpectedIncludeAllPropertiesWithExcludes);
        }

        [Fact]
        public void IncludeAllJsonPropertiesMutableXml()
        {
            // Arrange
            var logFactory = new LogFactory().Setup().LoadConfigurationFromXml(@"
            <nlog throwExceptions='true'>
                <targets>
                    <target name='asyncDebug' type='BufferingWrapper'>
                        <target name='debug' type='Debug'>
                            <layout type='JsonLayout' IncludeAllProperties='true' ExcludeProperties='Excluded1,Excluded2' maxRecursionLimit='0' />
                        </target>
                    </target>
                </targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='asyncDebug' />
                </rules>
            </nlog>").LogFactory;

            var logger = logFactory.GetLogger("A");

            // Act
            var logEventInfo = CreateLogEventWithExcluded();
            var stringPropBuilder = new System.Text.StringBuilder(logEventInfo.Properties["StringProp"].ToString());
            logEventInfo.Properties["StringProp"] = stringPropBuilder;
            logger.Debug(logEventInfo);
            stringPropBuilder.Clear();

            logFactory.Flush();

            // Assert
            logFactory.AssertDebugLastMessage(ExpectedIncludeAllPropertiesWithExcludes);
        }

        [Fact]
        public void IncludeAllJsonPropertiesMutableNestedXml()
        {
            // Arrange
            var logFactory = new LogFactory().Setup().LoadConfigurationFromXml(@"
            <nlog throwExceptions='true'>
                <targets>
                    <target name='asyncDebug' type='BufferingWrapper'>
                        <target name='debug' type='Debug'>
                            <layout type='JsonLayout' maxRecursionLimit='0'>
                                <attribute name='properties' encode='false' >
                                    <layout type='JsonLayout' IncludeAllProperties='true' ExcludeProperties='Excluded1,Excluded2' maxRecursionLimit='0'/>
                                </attribute>
                            </layout>
                        </target>
                    </target>
                </targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='asyncDebug' />
                </rules>
            </nlog>").LogFactory;

            var logger = logFactory.GetLogger("A");

            // Act
            var logEventInfo = CreateLogEventWithExcluded();
            var stringPropBuilder = new System.Text.StringBuilder(logEventInfo.Properties["StringProp"].ToString());
            logEventInfo.Properties["StringProp"] = stringPropBuilder;
            logger.Debug(logEventInfo);
            stringPropBuilder.Clear();

            logFactory.Flush();

            // Assert
            logFactory.AssertDebugLastMessage(@"{""properties"":" + ExpectedIncludeAllPropertiesWithExcludes + "}");
        }

        /// <summary>
        /// Serialize object deep
        /// </summary>
        [Fact]
        public void SerializeObjectRecursionSingle()
        {
            var logFactory = new LogFactory().Setup().LoadConfigurationFromXml(@"
            <nlog throwExceptions='true'>
            <targets>
                <target name='debug' type='Debug'  >
                 <layout type=""JsonLayout"" IncludeAllProperties='true' maxRecursionLimit='1' >
                 </layout>
                </target>
            </targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>").LogFactory;


            var logger = logFactory.GetLogger("A");

            var logEventInfo1 = new LogEventInfo();

            logEventInfo1.Properties.Add("nestedObject", new List<object> { new { val = 1, val2 = "value2" }, new { val3 = 3, val4 = "value4" } });

            logger.Debug(logEventInfo1);

            logFactory.AssertDebugLastMessage("{\"nestedObject\":[{\"val\":1,\"val2\":\"value2\"},{\"val3\":3,\"val4\":\"value4\"}]}");

            var logEventInfo2 = new LogEventInfo();

            logEventInfo2.Properties.Add("nestedObject", new { val = 1, val2 = "value2" });

            logger.Debug(logEventInfo2);

            logFactory.AssertDebugLastMessage("{\"nestedObject\":{\"val\":1,\"val2\":\"value2\"}}");

            var logEventInfo3 = new LogEventInfo();

            logEventInfo3.Properties.Add("nestedObject", new List<object> { new List<object> { new { val = 1, val2 = "value2" } } });

            logger.Debug(logEventInfo3);

            logFactory.AssertDebugLastMessage("{\"nestedObject\":[[\"{ val = 1, val2 = value2 }\"]]}");  // Allows nested collection, but then only ToString
        }

        [Fact]
        public void SerializeObjectRecursionZero()
        {
            var logFactory = new LogFactory().Setup().LoadConfigurationFromXml(@"
            <nlog throwExceptions='true'>
            <targets>
                <target name='debug' type='Debug'  >
                 <layout type=""JsonLayout"" IncludeAllProperties='true' maxRecursionLimit='0' >
                 </layout>
                </target>
            </targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>").LogFactory;


            var logger = logFactory.GetLogger("A");

            var logEventInfo1 = new LogEventInfo();

            logEventInfo1.Properties.Add("nestedObject", new List<object> { new { val = 1, val2 = "value2" }, new { val3 = 3, val4 = "value5" } });

            logger.Debug(logEventInfo1);

            logFactory.AssertDebugLastMessage("{\"nestedObject\":[\"{ val = 1, val2 = value2 }\",\"{ val3 = 3, val4 = value5 }\"]}");  // Allows single collection recursion

            var logEventInfo2 = new LogEventInfo();

            logEventInfo2.Properties.Add("nestedObject", new { val = 1, val2 = "value2" });

            logger.Debug(logEventInfo2);

            logFactory.AssertDebugLastMessage("{\"nestedObject\":\"{ val = 1, val2 = value2 }\"}");    // Never object recursion, only ToString

            var logEventInfo3 = new LogEventInfo();

            logEventInfo3.Properties.Add("nestedObject", new List<object> { new List<object> { new { val = 1, val2 = "value2" } } });

            logger.Debug(logEventInfo3);

            logFactory.AssertDebugLastMessage("{\"nestedObject\":[[]]}");  // No support for nested collections
        }

        [Fact]
        public void EncodesInvalidCharacters()
        {
            var logFactory = new LogFactory().Setup().LoadConfigurationFromXml(@"
            <nlog throwExceptions='true'>
            <targets>
                <target name='debug' type='Debug'  >
                 <layout type=""JsonLayout"" IncludeAllProperties='true'>
                 </layout>
                </target>
            </targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>").LogFactory;

            var logger = logFactory.GetLogger("A");

            var logEventInfo1 = new LogEventInfo();

            logEventInfo1.Properties.Add("InvalidCharacters", "|#{}%&\"~+\\/:*?<>".ToCharArray());

            logger.Debug(logEventInfo1);

            logFactory.AssertDebugLastMessage("{\"InvalidCharacters\":[\"|\",\"#\",\"{\",\"}\",\"%\",\"&\",\"\\\"\",\"~\",\"+\",\"\\\\\",\"/\",\":\",\"*\",\"?\",\"<\",\">\"]}");
        }

        [Fact]
        public void EncodesInvalidDoubles()
        {
            var logFactory = new LogFactory().Setup().LoadConfigurationFromXml(@"
            <nlog throwExceptions='true'>
            <targets>
                <target name='debug' type='Debug'  >
                 <layout type=""JsonLayout"" IncludeAllProperties='true' >
                 </layout>
                </target>
            </targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>").LogFactory;


            var logger = logFactory.GetLogger("A");

            var logEventInfo1 = new LogEventInfo();

            logEventInfo1.Properties.Add("DoubleNaN", double.NaN);
            logEventInfo1.Properties.Add("DoubleInfPositive", double.PositiveInfinity);
            logEventInfo1.Properties.Add("DoubleInfNegative", double.NegativeInfinity);
            logEventInfo1.Properties.Add("FloatNaN", float.NaN);
            logEventInfo1.Properties.Add("FloatInfPositive", float.PositiveInfinity);
            logEventInfo1.Properties.Add("FloatInfNegative", float.NegativeInfinity);

            logger.Debug(logEventInfo1);

            logFactory.AssertDebugLastMessage("{\"DoubleNaN\":\"NaN\",\"DoubleInfPositive\":\"Infinity\",\"DoubleInfNegative\":\"-Infinity\",\"FloatNaN\":\"NaN\",\"FloatInfPositive\":\"Infinity\",\"FloatInfNegative\":\"-Infinity\"}");
        }

        [Fact]
        public void EscapeForwardSlashDefaultTest()
        {
            var logFactory = new LogFactory().Setup().LoadConfigurationFromXml(@"
            <nlog throwExceptions='true'>
            <targets>
                <target name='debug' type='Debug'  >
                  <layout type='JsonLayout' escapeForwardSlash='false' includeAllProperties='true'>
                    <attribute name='myurl1' layout='${event-properties:myurl}' />
                  </layout>
                </target>
            </targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>").LogFactory;

            var logger = logFactory.GetLogger("A");

            var logEventInfo1 = new LogEventInfo();
            logEventInfo1.Properties.Add("myurl", "http://hello.world.com/");
            logger.Debug(logEventInfo1);

            logFactory.AssertDebugLastMessage("{\"myurl1\":\"http://hello.world.com/\",\"myurl\":\"http://hello.world.com/\"}");
        }

        [Fact]
        public void SkipInvalidJsonPropertyValues()
        {
            var jsonLayout = new JsonLayout() { IncludeEventProperties = true, MaxRecursionLimit = 10 };

            var logEventInfo = new LogEventInfo
            {
                TimeStamp = new DateTime(2010, 01, 01, 12, 34, 56),
                Level = LogLevel.Info,
                Message = string.Empty,
            };

            var expectedValue = Guid.NewGuid();
            logEventInfo.Properties["BadObject"] = new BadObject();
            logEventInfo.Properties["EvilObject"] = new EvilObject();
            logEventInfo.Properties["RequestId"] = expectedValue;

            var actualValue = jsonLayout.Render(logEventInfo);
            Assert.Equal($"{{\"BadObject\":{{\"Recursive\":[\"Hello\"],\"WeirdProperty\":\"System.Action\"}},\"RequestId\":\"{expectedValue}\"}}", actualValue);
        }

        class BadObject
        {
            public IEnumerable<object> Recursive => new List<object>(new[] { "Hello", (object)this });

            public IEnumerable<string> EvilProperty => throw new NotSupportedException();

            public System.Action WeirdProperty { get; } = new System.Action(() => throw new NotSupportedException());
        }

        class EvilObject : IFormattable
        {
            public string ToString(string format, IFormatProvider formatProvider)
            {
                throw new ApplicationException("BadObject");
            }

            public override string ToString()
            {
                return ToString(null, null);
            }
        }

        private static LogEventInfo CreateLogEventWithExcluded()
        {
            var logEventInfo = new LogEventInfo
            {
                TimeStamp = new DateTime(2010, 01, 01, 12, 34, 56),
                Level = LogLevel.Info,
                Message = "hello, world"
            };

            logEventInfo.Properties.Add("StringProp", "ValueA");
            logEventInfo.Properties.Add("IntProp", 123);
            logEventInfo.Properties.Add("DoubleProp", 123.123);
            logEventInfo.Properties.Add("DecimalProp", 123.123m);
            logEventInfo.Properties.Add("BoolProp", true);
            logEventInfo.Properties.Add("NullProp", null);
            logEventInfo.Properties.Add("DateTimeProp", new DateTime(2345, 1, 23, 12, 34, 56, DateTimeKind.Utc));
            logEventInfo.Properties.Add("Excluded1", "ExcludedValue");
            logEventInfo.Properties.Add("Excluded2", "Also excluded");
            return logEventInfo;
        }

        public class DummyContextLogger
        {
            internal string Value { get; set; }

            public override string ToString()
            {
                return Value;
            }
        }
    }
}
