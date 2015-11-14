﻿// 
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

namespace NLog.UnitTests.Layouts
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using NLog.Layouts;
#if (__IOS__)
    using NUnit.Framework;
    using Assert = NUnit.Framework.NLog.Assert;
#else
    using Xunit;
#endif

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

            var ev = new LogEventInfo();
            ev.TimeStamp = new DateTime(2010, 01, 01, 12, 34, 56);
            ev.Level = LogLevel.Info;
            ev.Message = "hello, world";

            Assert.Equal("{ \"date\": \"2010-01-01 12:34:56.0000\", \"level\": \"Info\", \"message\": \"hello, world\" }", jsonLayout.Render(ev));
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

            var ev = new LogEventInfo();
            ev.TimeStamp = new DateTime(2010, 01, 01, 12, 34, 56);
            ev.Level = LogLevel.Info;
            ev.Message = "hello, world";

            Assert.Equal("{\"date\":\"2010-01-01 12:34:56.0000\",\"level\":\"Info\",\"message\":\"hello, world\"}", jsonLayout.Render(ev));
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

            var ev = new LogEventInfo();
            ev.TimeStamp = new DateTime(2010, 01, 01, 12, 34, 56);
            ev.Level = LogLevel.Info;
            ev.Message = "\"hello, world\"";

            Assert.Equal("{ \"date\": \"2010-01-01 12:34:56.0000\", \"level\": \"Info\", \"message\": \"\\\"hello, world\\\"\" }", jsonLayout.Render(ev));
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

            var ev = new LogEventInfo();
            ev.TimeStamp = new DateTime(2010, 01, 01, 12, 34, 56);
            ev.Level = LogLevel.Info;
            ev.Message = "hello,\n\r world";

            Assert.Equal("{ \"date\": \"2010-01-01 12:34:56.0000\", \"level\": \"Info\", \"message\": \"hello,\\n\\r world\" }", jsonLayout.Render(ev));
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

            var ev = new LogEventInfo();
            ev.TimeStamp = new DateTime(2010, 01, 01, 12, 34, 56);
            ev.Level = LogLevel.Info;
            ev.Message = "{ \"hello\" : \"world\" }";

            Assert.Equal("{ \"date\": \"2010-01-01 12:34:56.0000\", \"level\": \"Info\", \"message\": { \"hello\" : \"world\" } }", jsonLayout.Render(ev));
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

            var ev = new LogEventInfo();
            ev.TimeStamp = new DateTime(2010, 01, 01, 12, 34, 56);
            ev.Level = LogLevel.Info;
            ev.Message = "{ \"hello\" : \"world\" }";

            Assert.Equal("{ \"date\": \"2010-01-01 12:34:56.0000\", \"level\": \"Info\", \"message\": \"{ \\\"hello\\\" : \\\"world\\\" }\" }", jsonLayout.Render(ev));
        }
    }
}
