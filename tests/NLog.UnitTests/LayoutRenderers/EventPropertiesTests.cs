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

namespace NLog.UnitTests.LayoutRenderers
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using NLog.Layouts;
    using Xunit;

    public class EventPropertiesTests : NLogTestBase
    {
        [Fact]
        public void TestNoProperty()
        {
            Layout layout = "${event-properties:prop1}";
            LogEventInfo logEvent = LogEventInfo.Create(LogLevel.Info, "prop1", "bbb");
            // empty
            Assert.Equal("", layout.Render(logEvent));
        }

        [Fact]
        public void TestProperty()
        {
            Layout layout = "${event-properties:prop1}";
            LogEventInfo logEvent = LogEventInfo.Create(LogLevel.Info, "logger1", "message1");
            logEvent.Properties["prop1"] = "bbb";

            Assert.Equal("bbb", layout.Render(logEvent));
        }

        /// <summary>
        /// Test with alias
        /// </summary>
        [Fact]
        public void TestPropertyAlias()
        {
            Layout layout = "${event-property:prop1}";
            LogEventInfo logEvent = LogEventInfo.Create(LogLevel.Info, "logger1", "message1");
            logEvent.Properties["prop1"] = "bbb";

            Assert.Equal("bbb", layout.Render(logEvent));
        }

        [Fact]
        public void TestNullProperty()
        {
            Layout layout = "${event-properties:prop1}";
            LogEventInfo logEvent = LogEventInfo.Create(LogLevel.Info, "logger1", "message1");
            logEvent.Properties["prop1"] = null;

            // empty
            Assert.Equal("", layout.Render(logEvent));
        }

        [Fact]
        public void TestDictionaryProperty()
        {
            Layout layout = "${event-properties:prop1}";
            LogEventInfo logEvent = LogEventInfo.Create(LogLevel.Info, "logger1", "message1");
            logEvent.Properties["prop1"] = new Dictionary<string, string>()
            {
                { "key1", "value1" }
            };

            // Dictionary should render as key-value pairs
            Assert.Equal("key1=value1", layout.Render(logEvent));
        }

        [Fact]
        public void TestPropertyIgnoreCase()
        {
            Layout layout1 = "${event-property:prop1}";
            Layout layout2 = "${event-property:Prop1}";
            Layout layout3 = "${event-property:PROP1}";
            LogEventInfo logEvent = LogEventInfo.Create(LogLevel.Info, "logger1", "message1");
            logEvent.Properties["PROP1"] = "bbb";

            Assert.Equal("bbb", layout1.Render(logEvent));
            Assert.Equal("bbb", layout2.Render(logEvent));
            Assert.Equal("bbb", layout3.Render(logEvent));
        }

        [Fact]
        public void TestPropertyCaseSensitive()
        {
            Layout layout1 = "${event-property:prop1:ignoreCase=false}";
            Layout layout2 = "${event-property:Prop1:ignoreCase=false}";
            Layout layout3 = "${event-property:PROP1:ignoreCase=false}";
            LogEventInfo logEvent = LogEventInfo.Create(LogLevel.Info, "logger1", "message1");
            logEvent.Properties["Prop1"] = "aaa";
            logEvent.Properties["PROP1"] = "bbb";

            Assert.Equal(2, logEvent.Properties.Count);
            Assert.Equal("aaa", logEvent.Properties["Prop1"]);
            Assert.Equal("bbb", logEvent.Properties["PROP1"]);
            Assert.Equal("", layout1.Render(logEvent));
            Assert.Equal("aaa", layout2.Render(logEvent));
            Assert.Equal("bbb", layout3.Render(logEvent));
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
            logEvent.Properties["prop1"] = new DateTime(2020, 11, 21, 23, 1, 0);

            Assert.Equal("21-11-2020 23:01:00", layout.Render(logEvent));
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

        [Fact]
        public void ObjectPathPrecalculateMutableObjectTest()
        {
            Layout stringLayout = "${event-properties:prop1:objectpath=Name}";
            Layout intLayout = "${event-properties:prop1:objectpath=Id}";
            Layout formattedLayout = "${event-properties:prop1:objectpath=Id:format=D4}";
            Layout nullLayout = "${event-properties:prop1:objectpath=Optional}";
            Layout nullJsonLayout = "${event-properties:prop1:objectpath=Optional:format=@}";
            Layout mutableSubValueLayout = "${event-properties:prop1:objectpath=Builder}";

            var mutableHolder = new MutablePropertyHolder
            {
                Id = 42,
                Name = "initial",
                Optional = null,
                Builder = new StringBuilder("initial")
            };
            var logEvent = LogEventInfo.Create(LogLevel.Info, "logger1", "message1");
            logEvent.Properties["prop1"] = mutableHolder;

            Assert.True(intLayout.TryGetRawValue(logEvent, out var rawBeforePrecalculate));
            Assert.Equal(42, rawBeforePrecalculate);

            stringLayout.Precalculate(logEvent);
            intLayout.Precalculate(logEvent);
            formattedLayout.Precalculate(logEvent);
            nullLayout.Precalculate(logEvent);
            nullJsonLayout.Precalculate(logEvent);
            mutableSubValueLayout.Precalculate(logEvent);

            mutableHolder.Id = 99;
            mutableHolder.Name = "mutated";
            mutableHolder.Optional = "now-set";
            mutableHolder.Builder.Append(" mutated");

            // Calling Precalculate again after mutation should keep the previously cached raw value
            intLayout.Precalculate(logEvent);

            Assert.Equal("initial", stringLayout.Render(logEvent));
            Assert.True(stringLayout.TryGetRawValue(logEvent, out var rawName));
            Assert.Equal("initial", rawName);

            Assert.Equal("42", intLayout.Render(logEvent));
            Assert.True(intLayout.TryGetRawValue(logEvent, out var rawId));
            Assert.Equal(42, rawId);

            Assert.Equal("0042", formattedLayout.Render(logEvent));
            Assert.True(formattedLayout.TryGetRawValue(logEvent, out var rawFormattedId));
            Assert.Equal(42, rawFormattedId);

            Assert.Equal(string.Empty, nullLayout.Render(logEvent));
            Assert.True(nullLayout.TryGetRawValue(logEvent, out var rawNull));
            Assert.Null(rawNull);

            Assert.Equal("null", nullJsonLayout.Render(logEvent));

            Assert.Equal("initial", mutableSubValueLayout.Render(logEvent));
            Assert.False(mutableSubValueLayout.TryGetRawValue(logEvent, out _));
        }

        [Fact]
        public void ObjectPathPrecalculateJsonArrayAndJsonLayoutTest()
        {
            var jsonArray = new JsonArrayLayout
            {
                SuppressSpaces = true,
                Items =
                {
                    "${level}",
                    "${logger}",
                    "${event-properties:prop1:objectpath=Id}",
                    "${event-properties:prop1:objectpath=Name}",
                }
            };
            jsonArray.Initialize(null);

            var jsonLayout = new JsonLayout
            {
                SuppressSpaces = true,
                Attributes =
                {
                    new JsonAttribute("level", "${level}"),
                    new JsonAttribute("logger", "${logger}"),
                    new JsonAttribute("id", "${event-properties:prop1:objectpath=Id}") { ValueType = typeof(int) },
                    new JsonAttribute("name", "${event-properties:prop1:objectpath=Name}"),
                }
            };
            jsonLayout.Initialize(null);

            var mutableHolder = new MutablePropertyHolder { Id = 42, Name = "initial" };
            var logEvent = LogEventInfo.Create(LogLevel.Info, "logger1", "message1");
            logEvent.Properties["prop1"] = mutableHolder;

            // Without Precalculate: numeric raw value is preserved
            Assert.Equal("[\"Info\",\"logger1\",42,\"initial\"]", jsonArray.Render(logEvent));
            Assert.Equal("{\"level\":\"Info\",\"logger\":\"logger1\",\"id\":42,\"name\":\"initial\"}", jsonLayout.Render(logEvent));

            // With PrecalculateBuilder (per-item precalculation) + mutation: precalculated raw value is preserved as numeric JSON
            var mutableHolder2 = new MutablePropertyHolder { Id = 42, Name = "initial" };
            var logEvent2 = LogEventInfo.Create(LogLevel.Info, "logger1", "message1");
            logEvent2.Properties["prop1"] = mutableHolder2;

            jsonArray.PrecalculateBuilder(logEvent2, new StringBuilder());
            jsonLayout.PrecalculateBuilder(logEvent2, new StringBuilder());
            Assert.False(logEvent2.TryGetCachedLayoutValue(jsonArray, out _));
            Assert.False(logEvent2.TryGetCachedLayoutValue(jsonLayout, out _));

            mutableHolder2.Id = 99;
            mutableHolder2.Name = "mutated";

            Assert.Equal("[\"Info\",\"logger1\",42,\"initial\"]", jsonArray.Render(logEvent2));
            Assert.Equal("{\"level\":\"Info\",\"logger\":\"logger1\",\"id\":42,\"name\":\"initial\"}", jsonLayout.Render(logEvent2));
        }

        [Fact]
        public void ObjectPathPrecalculateWhenWrappersTest()
        {
            Layout whenEmptyValueLayout = "${event-properties:prop1:objectpath=Id:whenEmpty=0}";
            Layout whenEmptyFallbackLayout = "${event-properties:prop1:objectpath=Optional:whenEmpty=fallback}";
            Layout whenStaticConditionLayout = "${when:when=level>=LogLevel.Info:inner=${event-properties:prop1:objectpath=Id}}";
            Layout whenMutableConditionLayout = "${when:when='${event-properties:prop1:objectpath=Name}'=='initial':inner=${event-properties:prop1:objectpath=Id}:else=0}";

            var mutableHolder = new MutablePropertyHolder { Id = 42, Name = "initial", Optional = null };
            var logEvent = LogEventInfo.Create(LogLevel.Info, "logger1", "message1");
            logEvent.Properties["prop1"] = mutableHolder;

            whenEmptyValueLayout.Precalculate(logEvent);
            whenEmptyFallbackLayout.Precalculate(logEvent);
            whenStaticConditionLayout.Precalculate(logEvent);
            whenMutableConditionLayout.Precalculate(logEvent);

            mutableHolder.Id = 99;
            mutableHolder.Name = "mutated";
            mutableHolder.Optional = "now-set";

            Assert.Equal("42", whenEmptyValueLayout.Render(logEvent));
            Assert.True(whenEmptyValueLayout.TryGetRawValue(logEvent, out var whenEmptyRaw));
            Assert.Equal(42, whenEmptyRaw);

            Assert.Equal("fallback", whenEmptyFallbackLayout.Render(logEvent));

            Assert.Equal("42", whenStaticConditionLayout.Render(logEvent));
            Assert.True(whenStaticConditionLayout.TryGetRawValue(logEvent, out var whenStaticRaw));
            Assert.Equal(42, whenStaticRaw);

            Assert.Equal("42", whenMutableConditionLayout.Render(logEvent));
        }

        [Fact]
        public void ObjectPathPrecalculateMissingPropertyTest()
        {
            Layout layout = "${event-properties:prop1:objectpath=Id}";
            var logEvent = LogEventInfo.Create(LogLevel.Info, "logger1", "message1");

            layout.Precalculate(logEvent);

            Assert.Equal(string.Empty, layout.Render(logEvent));
            Assert.True(layout.TryGetRawValue(logEvent, out var rawValue));
            Assert.Null(rawValue);
        }

        [Fact]
        public void ObjectPathTryGetRawValueStructTest()
        {
            Layout layout = "${event-properties:prop1:objectpath=Key}";
            var logEvent = LogEventInfo.Create(LogLevel.Info, "logger1", "message1");
            logEvent.Properties["prop1"] = new KeyValuePair<string, int>("myKey", 42);

            layout.Precalculate(logEvent);

            Assert.True(layout.TryGetRawValue(logEvent, out var rawValue));
            Assert.Equal("myKey", rawValue);
        }

        private sealed class MutablePropertyHolder
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Optional { get; set; }
            public StringBuilder Builder { get; set; }
        }
    }
}
