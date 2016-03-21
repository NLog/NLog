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

using NLog.Targets;

namespace NLog.UnitTests.Layouts
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using NLog.Layouts;
    using Xunit;

    public class JsonLayoutTests : NLogTestBase
    {
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
    }
}
