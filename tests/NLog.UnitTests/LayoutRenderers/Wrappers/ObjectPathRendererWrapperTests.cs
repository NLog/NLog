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

using System;
using System.Runtime.InteropServices;
using System.Text;
using NLog.Layouts;
using Xunit;

namespace NLog.UnitTests.LayoutRenderers.Wrappers
{
    public class ObjectPathRendererWrapperTests
    {
        [Theory]
        [InlineData("${object-path:${exception}:path=ParamName}")]
        [InlineData("${exception:objectpath=ParamName}")]
        public void RenderPropertyOfException(string layout)
        {
            // Arrange
            var logEvent = new LogEventInfo
            {
                Exception = new ArgumentException("ArgumentException with param name", "MyParam")
            };
            Layout l = layout;

            // Act
            var result = l.Render(logEvent);

            // Assert
            Assert.Equal("MyParam", result);
        }

        [Theory]
        [InlineData("${object-path:${exception}:path=ParamName:Format=@}")]
        [InlineData("${exception:objectpath=ParamName}")]
        public void RenderPropertyWithoutException(string layout)
        {
            // Arrange
            var logEvent = LogEventInfo.CreateNullEvent();
            Layout l = layout;

            // Act
            var result = l.Render(logEvent);

            // Assert
            Assert.Equal("", result);
        }

        [Theory]
        [InlineData("${object-path:${exception}:path=ParamName:Format=@}")]
        public void RenderPropertyWithExceptionNullValue(string layout)
        {
            // Arrange
            var logEvent = new LogEventInfo
            {
                Exception = new ArgumentException("ArgumentException with null param name", (string)null)
            };
            Layout l = layout;

            // Act
            var result = l.Render(logEvent);

            // Assert
            Assert.Equal("null", result);
        }

        [Theory]
        [InlineData(null, "5000")]
        [InlineData("N2", "5.000,00")]
        public void RenderPropertyOfExceptionWithFormat(string format, string expected)
        {
            // Arrange
            var logEvent = new LogEventInfo
            {
                Exception = new ExternalException("Exception with errorCode", 5000),
            };
            Layout l = "${object-path:${exception}:path=HResult:Culture=NL-nl:format=" + format + "}";

            // Act
            var result = l.Render(logEvent);

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("HResult", 5000, true)]
        [InlineData("hResult", null, false)] // path is case sensitive
        public void RenderPropertyOfExceptionRawValue(string path, int? expectedInt, bool expectedResult)
        {
            // Arrange
            var logEvent = new LogEventInfo
            {
                Exception = new ExternalException("Exception with errorCode", 5000),
            };
            Layout l = "${object-path:${exception}:path=" + path + "}";

            // Act
            var result = l.TryGetRawValue(logEvent, out var rawValue);

            // Assert
            Assert.Equal(expectedInt, rawValue);
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public void ObjectPathPrecalculateMutableEventPropertyTest()
        {
            Layout stringLayout = "${object-path:${event-properties:prop1}:path=Name}";
            Layout intLayout = "${object-path:${event-properties:prop1}:path=Id}";
            Layout formattedLayout = "${object-path:${event-properties:prop1}:path=Id:format=D4}";
            Layout nullJsonLayout = "${object-path:${event-properties:prop1}:path=Optional:format=@}";
            Layout missingJsonLayout = "${object-path:${event-properties:prop1}:path=Child.Name:format=@}";
            Layout mutableSubValueLayout = "${object-path:${event-properties:prop1}:path=Builder}";

            var mutableHolder = new MutablePropertyHolder
            {
                Id = 42,
                Name = "initial",
                Optional = null,
                Child = null,
                Builder = new StringBuilder("initial")
            };
            var logEvent = LogEventInfo.Create(LogLevel.Info, "logger1", "message1");
            logEvent.Properties["prop1"] = mutableHolder;

            stringLayout.Precalculate(logEvent);
            intLayout.Precalculate(logEvent);
            formattedLayout.Precalculate(logEvent);
            nullJsonLayout.Precalculate(logEvent);
            missingJsonLayout.Precalculate(logEvent);
            mutableSubValueLayout.Precalculate(logEvent);

            mutableHolder.Id = 99;
            mutableHolder.Name = "mutated";
            mutableHolder.Optional = "now-set";
            mutableHolder.Child = new MutableChild { Name = "mutated-child", Code = 99 };
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

            Assert.Equal("null", nullJsonLayout.Render(logEvent));
            Assert.True(nullJsonLayout.TryGetRawValue(logEvent, out var rawNull));
            Assert.Null(rawNull);

            Assert.Equal(string.Empty, missingJsonLayout.Render(logEvent));
            Assert.False(missingJsonLayout.TryGetRawValue(logEvent, out var rawMissing));
            Assert.Null(rawMissing);

            Assert.Equal("initial", mutableSubValueLayout.Render(logEvent));
            Assert.False(mutableSubValueLayout.TryGetRawValue(logEvent, out _));
        }

        [Fact]
        public void ObjectPathNestedWrapperPrecalculateTest()
        {
            Layout nestedNameLayout = "${object-path:${object-path:${event-properties:prop1}:path=Child}:path=Name}";
            Layout nestedCodeLayout = "${object-path:${object-path:${event-properties:prop1}:path=Child}:path=Code}";

            // Without Precalculate
            var holder1 = new MutablePropertyHolder { Child = new MutableChild { Name = "initial-child", Code = 7 } };
            var logEvent1 = LogEventInfo.Create(LogLevel.Info, "logger1", "message1");
            logEvent1.Properties["prop1"] = holder1;

            Assert.Equal("initial-child", nestedNameLayout.Render(logEvent1));
            Assert.True(nestedCodeLayout.TryGetRawValue(logEvent1, out var rawCode1));
            Assert.Equal(7, rawCode1);

            // With Precalculate + mutation
            var holder2 = new MutablePropertyHolder { Child = new MutableChild { Name = "initial-child", Code = 7 } };
            var logEvent2 = LogEventInfo.Create(LogLevel.Info, "logger1", "message1");
            logEvent2.Properties["prop1"] = holder2;

            nestedNameLayout.Precalculate(logEvent2);
            nestedCodeLayout.Precalculate(logEvent2);

            holder2.Child.Name = "mutated-child";
            holder2.Child.Code = 99;
            holder2.Child = null;

            Assert.Equal("initial-child", nestedNameLayout.Render(logEvent2));
            Assert.True(nestedNameLayout.TryGetRawValue(logEvent2, out var rawName2));
            Assert.Equal("initial-child", rawName2);

            Assert.Equal("7", nestedCodeLayout.Render(logEvent2));
            Assert.True(nestedCodeLayout.TryGetRawValue(logEvent2, out var rawCode2));
            Assert.Equal(7, rawCode2);
        }

        [Fact]
        public void ObjectPathNestedWrapperNullChildPrecalculateTest()
        {
            Layout nestedNameLayout = "${object-path:${object-path:${event-properties:prop1}:path=Child}:path=Name}";

            var holder = new MutablePropertyHolder { Child = null };
            var logEvent = LogEventInfo.Create(LogLevel.Info, "logger1", "message1");
            logEvent.Properties["prop1"] = holder;

            nestedNameLayout.Precalculate(logEvent);

            holder.Child = new MutableChild { Name = "mutated-child", Code = 99 };

            Assert.Equal(string.Empty, nestedNameLayout.Render(logEvent));
            Assert.False(nestedNameLayout.TryGetRawValue(logEvent, out var rawValue));
            Assert.Null(rawValue);
        }

        private sealed class MutablePropertyHolder
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Optional { get; set; }
            public MutableChild Child { get; set; }
            public StringBuilder Builder { get; set; }
        }

        private sealed class MutableChild
        {
            public string Name { get; set; }
            public int Code { get; set; }
        }
    }
}
