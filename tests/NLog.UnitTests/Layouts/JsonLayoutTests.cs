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

using NLog.Targets;

namespace NLog.UnitTests.Layouts
{
    using System;
    using NLog.Layouts;
    using Xunit;

    public class JsonLayoutTests : NLogTestBase
    {
        private const string ExpectedIncludeAllPropertiesWithExcludes = "{ \"StringProp\": \"ValueA\", \"IntProp\": 123, \"DoubleProp\": 123.123, \"DecimalProp\": 123.123, \"BoolProp\": true, \"NullProp\": null, \"DateTimeProp\": \"2345-01-23T12:34:56Z\" }";

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

            Assert.Equal("{ \"date\": \"2010-01-01 12:34:56.0000\", \"level\": \"Info\", \"message\": \"hello, world\" }", jsonLayout.Render(logEventInfo));
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
                        new JsonAttribute("message", "${message}") { EscapeUnicode = false },
                    },
                SuppressSpaces = true
            };

            var logEventInfo = new LogEventInfo
            {
                LoggerName = "\u00a9",
                Level = LogLevel.Info,
                Message = "\u00a9",
            };

            Assert.Equal("{\"logger\":\"\\u00a9\",\"level\":\"Info\",\"message\":\"\u00a9\"}", jsonLayout.Render(logEventInfo));
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

            Assert.Equal("{ \"date\": \"2010-01-01 12:34:56.0000\", \"level\": \"Info\", \"message\": \"\\\"hello, world\\\"\" }", jsonLayout.Render(logEventInfo));
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

            Assert.Equal("{ \"date\": \"2010-01-01 12:34:56.0000\", \"level\": \"Info\", \"message\": \"hello,\\n\\r world\" }", jsonLayout.Render(logEventInfo));
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

            Assert.Equal("{ \"date\": \"2010-01-01 12:34:56.0000\", \"level\": \"Info\", \"message\": { \"hello\" : \"world\" } }", jsonLayout.Render(logEventInfo));
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

            Assert.Equal("{ \"date\": \"2010-01-01 12:34:56.0000\", \"level\": \"Info\", \"message\": \"{ \\\"hello\\\" : \\\"world\\\" }\" }", jsonLayout.Render(logEventInfo));
        }

        [Fact]
        public void JsonAttributeThreadAgnosticTest()
        {
            LogManager.Configuration = CreateConfigurationFromString(@"
            <nlog throwExceptions='true'>
            <targets>
                <target name='debug' type='Debug'  >
                 <layout type='JsonLayout'>
                    <attribute name='type' layout='${exception:format=Type}'/>
                    <attribute name='message' layout='${exception:format=Message}'/>
                    <attribute name='threadid' layout='${threadid}'/>
                 </layout>
                </target>
            </targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>");

            ILogger logger = LogManager.GetLogger("B");

            var logEventInfo = CreateLogEventWithExcluded();

            logger.Debug(logEventInfo);

            var message = GetDebugLastMessage("debug");
            Assert.Equal(true, message.Contains(System.Threading.Thread.CurrentThread.ManagedThreadId.ToString()));
        }

        [Fact]
        public void JsonAttributeStackTraceUsageTest()
        {
            LogManager.Configuration = CreateConfigurationFromString(@"
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
            </nlog>");

            ILogger logger = LogManager.GetLogger("C");

            var logEventInfo = CreateLogEventWithExcluded();

            logger.Debug(logEventInfo);

            var message = GetDebugLastMessage("debug");
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
            Assert.Equal("{ \"type\": \"NLog.NLogRuntimeException\", \"message\": \"test\", \"innerException\": { \"type\": \"System.NullReferenceException\", \"message\": \"null is bad!\" } }", json);

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
            Assert.Equal("{ \"type\": \"NLog.NLogRuntimeException\", \"message\": \"test\" }", json);

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
            Assert.Equal("{ \"type\": \"NLog.NLogRuntimeException\", \"message\": \"test\", \"innerException\": {  } }", json);

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
            <attribute name='exception' layout='${exception}' />
          </layout>
        </attribute>
      </layout>
    </target>
  </targets>
  <rules>
  </rules>
</nlog>
";

            var config = CreateConfigurationFromString(configXml);

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
            Assert.Equal("'${message}'", nestedJsonLayout.Attributes[0].Layout.ToString());
            Assert.Equal("'${exception}'", nestedJsonLayout.Attributes[1].Layout.ToString());

            var logEventInfo = new LogEventInfo
            {
                TimeStamp = new DateTime(2016, 10, 30, 13, 30, 55),
                Message = "this is message",
                Level = LogLevel.Info,
                Exception = new NLogRuntimeException("test", new NullReferenceException("null is bad!"))
            };

            var json = jsonLayout.Render(logEventInfo);
            Assert.Equal("{ \"time\": \"2016-10-30 13:30:55.0000\", \"level\": \"INFO\", \"nested\": { \"message\": \"this is message\", \"exception\": \"test\" } }", json);
        }

        [Fact]
        public void IncludeAllJsonProperties()
        {
            var jsonLayout = new JsonLayout()
            {
                IncludeAllProperties = true
            };

            jsonLayout.ExcludeProperties.Add("Excluded1");
            jsonLayout.ExcludeProperties.Add("Excluded2");

            var logEventInfo = CreateLogEventWithExcluded();

            Assert.Equal(ExpectedIncludeAllPropertiesWithExcludes, jsonLayout.Render(logEventInfo));
        }

        [Fact]
        public void IncludeMdcJsonProperties()
        {
            LogManager.Configuration = CreateConfigurationFromString(@"
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
            </nlog>");

            ILogger logger = LogManager.GetLogger("A");

            var logEventInfo = CreateLogEventWithExcluded();

            MappedDiagnosticsContext.Clear();
            foreach (var prop in logEventInfo.Properties)
                if (prop.Key.ToString() != "Excluded1" && prop.Key.ToString() != "Excluded2")
                    MappedDiagnosticsContext.Set(prop.Key.ToString(), prop.Value);
            logEventInfo.Properties.Clear();

            logger.Debug(logEventInfo);

            LogManager.Flush();

            AssertDebugLastMessage("debug", ExpectedIncludeAllPropertiesWithExcludes);
        }

#if NET4_0 || NET4_5
        [Fact]
        public void IncludeMdlcJsonProperties()
        {
            LogManager.Configuration = CreateConfigurationFromString(@"
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
            </nlog>");

            ILogger logger = LogManager.GetLogger("A");

            var logEventInfo = CreateLogEventWithExcluded();

            MappedDiagnosticsLogicalContext.Clear();
            foreach (var prop in logEventInfo.Properties)
                if (prop.Key.ToString() != "Excluded1" && prop.Key.ToString() != "Excluded2")
                    MappedDiagnosticsLogicalContext.Set(prop.Key.ToString(), prop.Value);
            logEventInfo.Properties.Clear();

            logger.Debug(logEventInfo);

            LogManager.Flush();

            AssertDebugLastMessage("debug", ExpectedIncludeAllPropertiesWithExcludes);
        }
#endif

        /// <summary>
        /// Test from XML, needed for the list (ExcludeProperties)
        /// </summary>
        [Fact]
        public void IncludeAllJsonPropertiesXml()
        {
            LogManager.Configuration = CreateConfigurationFromString(@"
            <nlog throwExceptions='true'>
            <targets>
                <target name='debug' type='Debug'  >
                 <layout type=""JsonLayout"" IncludeAllProperties='true' ExcludeProperties='Excluded1,Excluded2'>
            
                 </layout>
                </target>
            </targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>");


            ILogger logger = LogManager.GetLogger("A");

            var logEventInfo = CreateLogEventWithExcluded();

            logger.Debug(logEventInfo);

            AssertDebugLastMessage("debug", ExpectedIncludeAllPropertiesWithExcludes);
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
    }
}
