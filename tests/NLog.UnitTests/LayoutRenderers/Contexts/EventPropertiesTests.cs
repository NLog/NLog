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

using System;
using NLog.Filters;

namespace NLog.UnitTests.LayoutRenderers
{
    using NLog.Layouts;
    using Xunit;

    public class EventPropertiesTests : NLogTestBase
    {
        [Fact]
        public void Test1()
        {
            Layout layout = "${event-properties:prop1}";
            LogEventInfo logEvent = LogEventInfo.Create(LogLevel.Info, "prop1", "bbb");
            // empty
            Assert.Equal("", layout.Render(logEvent));
        } 
        
        /// <summary>
        /// Test with alias
        /// </summary>
        [Fact]
        public void TestAlias1()
        {
            Layout layout = "${event-property:prop1}";
            LogEventInfo logEvent = LogEventInfo.Create(LogLevel.Info, "prop1", "bbb");
            // empty
            Assert.Equal("", layout.Render(logEvent));
        }

        [Fact]
        public void Test2()
        {
            Layout layout = "${event-properties:prop1}";
            LogEventInfo logEvent = LogEventInfo.Create(LogLevel.Info, "logger1", "message1");
            logEvent.Properties["prop1"] = "bbb";

            // empty
            Assert.Equal("bbb", layout.Render(logEvent));
        }

        [Fact]
        public void NoSet()
        {
            Layout layout = "${event-properties:prop1}";
            LogEventInfo logEvent = LogEventInfo.Create(LogLevel.Info, "logger1", "message1");

            // empty
            Assert.Equal("", layout.Render(logEvent));
        }


        [Fact]
        public void Null()
        {
            Layout layout = "${event-properties:prop1}";
            LogEventInfo logEvent = LogEventInfo.Create(LogLevel.Info, "logger1", "message1");
            logEvent.Properties["prop1"] = null;

            // empty
            Assert.Equal("", layout.Render(logEvent));
        }

        [Fact]
        public void DateTime()
        {
            Layout layout = "${event-properties:prop1}";
            LogEventInfo logEvent = LogEventInfo.Create(LogLevel.Info, "logger1", "message1");
            logEvent.Properties["prop1"] = new DateTime(2020, 2, 21, 23, 1, 0);

            Assert.Equal("02/21/2020 23:01:00", layout.Render(logEvent));
        }

        [Fact]
        public void DateTimeFormat()
        {
            Layout layout = "${event-properties:prop1:format=yyyy-M-dd}";
            LogEventInfo logEvent = LogEventInfo.Create(LogLevel.Info, "logger1", "message1");
            logEvent.Properties["prop1"] = new DateTime(2020, 2, 21, 23, 1, 0);

            Assert.Equal("2020-2-21", layout.Render(logEvent));
        }

        [Fact]
        public void DateTimeCulture()
        {
            if (IsLinux())
            {
                Console.WriteLine("[SKIP] EventPropertiesTests.DateTimeCulture because we are running in Travis");
                return;
            }

            Layout layout = "${event-properties:prop1:culture=nl-NL}";
            LogEventInfo logEvent = LogEventInfo.Create(LogLevel.Info, "logger1", "message1");
            logEvent.Properties["prop1"] = new DateTime(2020, 2, 21, 23, 1, 0);

            Assert.Equal("21-2-2020 23:01:00", layout.Render(logEvent));
        }

        [Fact]
        public void JsonFormat()
        {
            Layout layout = "${event-properties:prop1:format=@}";
            LogEventInfo logEvent = LogEventInfo.Create(LogLevel.Info, "logger1", "message1");
            logEvent.Properties["prop1"] = new string[] { "Hello", "World" };
            Assert.Equal("[\"Hello\",\"World\"]", layout.Render(logEvent));
        }

        [Theory]
        [InlineData("prop1", "", "{ Id = 1, id = 2, Name = test, Nested = { Id = 3 } }")]
        [InlineData("prop1", "Id", "1")]
        [InlineData("prop1", "id", "2")] //correct casing
        [InlineData("prop1", "Nested.Id", "3")]
        [InlineData("prop1", "Id.Wrong", "")] //don't crash on nesting
        [InlineData("prop1", "Name", "test")]
        [InlineData("prop1", "Name ", "test")] // trimend
        [InlineData("prop1", "NameWrong", "")]
        public void ObjectPathNestedProperty(string item, string objectPath, string expectedValue)
        {
            Layout layout = "${event-properties:" + item + ":objectpath=" + objectPath + "}";
            layout.Initialize(null);

            // Slow Uncached Lookup
            LogEventInfo logEvent = LogEventInfo.Create(LogLevel.Info, "logger1", "message1");
            logEvent.Properties["prop1"] = new { Id = 1, id = 2, Name = "test", Nested = new { Id = 3 } };
            Assert.Equal(expectedValue, layout.Render(logEvent));

            // Fast Cached Lookup
            LogEventInfo logEvent2 = LogEventInfo.Create(LogLevel.Info, "logger1", "message1");
            logEvent2.Properties["prop1"] = new { Id = 1, id = 2, Name = "test", Nested = new { Id = 3 } };
            Assert.Equal(expectedValue, layout.Render(logEvent2));
        }
    }
}