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
    using NLog.Layouts;
    using Xunit;

    public class JsonArrayLayoutTests : NLogTestBase
    {
        [Fact]
        public void JsonArrayLayoutRendering()
        {
            var jsonLayout = new JsonArrayLayout()
            {
                Items =
                    {
                        new SimpleLayout("${longdate}"),
                        new SimpleLayout("${level}"),
                        new SimpleLayout("${message}"),
                    }
            };

            var logEventInfo = new LogEventInfo
            {
                TimeStamp = new DateTime(2010, 01, 01, 12, 34, 56),
                Level = LogLevel.Info,
                Message = "hello\n world",
            };

            Assert.Equal("[ \"2010-01-01T12:34:56Z\", \"Info\", \"hello\\n world\" ]", jsonLayout.Render(logEventInfo));
        }

        [Fact]
        public void JsonArrayLayoutRenderingFromXml()
        {
            var logFactory = new LogFactory().Setup().LoadConfigurationFromXml(@"
                <nlog>
                    <targets>
                        <target type='Debug' name='Debug'>
                            <layout type='JsonArrayLayout'>
                                <item type='SimpleLayout' text='${longdate}' />
                                <item type='SimpleLayout' text='${level}' />
                                <item type='SimpleLayout' text='${message}' />
                            </layout>
                        </target>
                    </targets>
                    <rules>
                        <logger writeTo='Debug' />
                    </rules>
                </nlog>
            ").LogFactory;

            var logEventInfo = new LogEventInfo
            {
                TimeStamp = new DateTime(2010, 01, 01, 12, 34, 56),
                Level = LogLevel.Info,
                Message = "hello\n world",
            };
            logFactory.GetCurrentClassLogger().Log(logEventInfo);

            logFactory.AssertDebugLastMessage("[ \"2010-01-01T12:34:56Z\", \"Info\", \"hello\\n world\" ]");
        }

        [Fact]
        public void JsonArrayLayoutRenderingNoSpaces()
        {
            var jsonLayout = new JsonArrayLayout()
            {
                Items =
                    {
                        new SimpleLayout("${longdate}"),
                        new SimpleLayout("${level}"),
                        new SimpleLayout("${message}"),
                    },
                SuppressSpaces = true,
            };

            var logEventInfo = new LogEventInfo
            {
                TimeStamp = new DateTime(2010, 01, 01, 12, 34, 56),
                Level = LogLevel.Info,
                Message = "hello\n world",
            };

            Assert.Equal("[\"2010-01-01T12:34:56Z\",\"Info\",\"hello\\n world\"]", jsonLayout.Render(logEventInfo));
        }

        [Fact]
        public void JsonArrayLayoutRenderingNotEmpty()
        {
            var jsonLayout = new JsonArrayLayout()
            {
                Items =
                    {
                        new JsonLayout() { IncludeEventProperties = true, RenderEmptyObject = false },
                    }
            };

            var logEventInfo = new LogEventInfo
            {
                TimeStamp = new DateTime(2010, 01, 01, 12, 34, 56),
                Level = LogLevel.Info,
                Message = "hello\n world",
            };

            Assert.Equal("[ ]", jsonLayout.Render(logEventInfo));
        }

        [Fact]
        public void JsonArrayLayoutRenderingEmpty()
        {
            var jsonLayout = new JsonArrayLayout()
            {
                Items =
                    {
                        new JsonLayout() { IncludeEventProperties = true, RenderEmptyObject = false },
                    },
                RenderEmptyObject = false,
            };

            var logEventInfo = new LogEventInfo
            {
                TimeStamp = new DateTime(2010, 01, 01, 12, 34, 56),
                Level = LogLevel.Info,
                Message = "hello\n world",
            };

            Assert.Equal("", jsonLayout.Render(logEventInfo));
        }

        [Fact]
        public void JsonArrayLayoutObjectRendering()
        {
            var jsonLayout = new JsonArrayLayout()
            {
                Items =
                    {
                        new JsonLayout() { Attributes = { new JsonAttribute("date", "${longdate}") } },
                        new JsonLayout() { Attributes = { new JsonAttribute("level", "${level}") } },
                        new JsonLayout() { Attributes = { new JsonAttribute("message", "${message}") } },
                    }
            };

            var logEventInfo = new LogEventInfo
            {
                TimeStamp = new DateTime(2010, 01, 01, 12, 34, 56),
                Level = LogLevel.Info,
                Message = "hello\n world",
            };

            Assert.Equal("[ { \"date\": \"2010-01-01 12:34:56.0000\" }, { \"level\": \"Info\" }, { \"message\": \"hello\\n world\" } ]", jsonLayout.Render(logEventInfo));
        }
    }
}
