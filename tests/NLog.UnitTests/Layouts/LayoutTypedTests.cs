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
using NLog.Layouts;
using Xunit;

namespace NLog.UnitTests.Layouts
{
    public class LayoutTypedTests : NLogTestBase
    {
        [Fact]
        public void LayoutFixedIntValueTest()
        {
            // Arrange
            Layout<int> layout = 5;

            // Act
            var result = layout.RenderValue(LogEventInfo.CreateNullEvent());

            // Assert
            Assert.Equal(5, result);
            Assert.Equal("5", layout.Render(LogEventInfo.CreateNullEvent()));
            Assert.Equal(5, layout.StaticValue);
            Assert.Equal("5", layout.ToString());
        }

        [Fact]
        public void LayoutFixedNullableIntValueTest()
        {
            // Arrange
            Layout<int?> layout = 5;

            // Act
            var result = layout.RenderValue(LogEventInfo.CreateNullEvent());

            // Assert
            Assert.Equal(5, result);
            Assert.Equal("5", layout.Render(LogEventInfo.CreateNullEvent()));
            Assert.Equal(5, layout.StaticValue);
            Assert.Equal("5", layout.ToString());
        }

        [Fact]
        public void LayoutFixedNullIntValueTest()
        {
            // Arrange
            Layout<int?> layout = new Layout<int?>((int?)null);

            // Act
            var result = layout.RenderValue(LogEventInfo.CreateNullEvent());
            var result5 = layout.RenderValue(LogEventInfo.CreateNullEvent(), 5);

            // Assert
            Assert.Null(result);
            Assert.Null(result5);
            Assert.Equal("", layout.Render(LogEventInfo.CreateNullEvent()));
            Assert.Null(layout.StaticValue);
            Assert.Equal("null", layout.ToString());
        }

        [Fact]
        public void LayoutFixedUrlValueTest()
        {
            // Arrange
            var uri = new Uri("http://nlog");
            Layout<Uri> layout = uri.ToString();

            // Act
            var result = layout.RenderValue(LogEventInfo.CreateNullEvent());

            // Assert
            Assert.Equal(uri, result);
            Assert.Same(result, layout.RenderValue(LogEventInfo.CreateNullEvent()));
            Assert.Equal(uri.ToString(), layout.Render(LogEventInfo.CreateNullEvent()));
            Assert.Equal(uri, layout.StaticValue);
            Assert.Same(layout.StaticValue, layout.StaticValue);
            Assert.Equal(uri.ToString(), layout.ToString());
        }

        [Fact]
        public void LayoutFixedNullUrlValueTest()
        {
            // Arrange
            Uri uri = null;
            Layout<Uri> layout = new Layout<Uri>(uri);

            // Act
            var result = layout.RenderValue(LogEventInfo.CreateNullEvent());

            // Assert
            Assert.Equal(uri, result);
            Assert.Same(result, layout.RenderValue(LogEventInfo.CreateNullEvent()));
            Assert.Equal("", layout.Render(LogEventInfo.CreateNullEvent()));
            Assert.Equal(uri, layout.StaticValue);
            Assert.Same(layout.StaticValue, layout.StaticValue);
            Assert.Equal("null", layout.ToString());
        }

        [Fact]
        public void LayoutDynamicIntValueTest()
        {
            // Arrange
            string simpleLayout = "${event-properties:intvalue}";
            Layout<int> layout = simpleLayout;

            // Act
            var logevent = LogEventInfo.Create(LogLevel.Info, null, null, "{intvalue}", new object[] { 5 });
            var result = layout.RenderValue(logevent);

            // Assert
            Assert.Equal(5, result);
            Assert.Equal("5", layout.Render(logevent));
            Assert.Equal(0, layout.StaticValue);
            Assert.Equal(simpleLayout, layout.ToString());
        }

        [Fact]
        public void LayoutDynamicNullableIntValueTest()
        {
            // Arrange
            Layout<int?> layout = "${event-properties:intvalue}";

            // Act
            var logevent = LogEventInfo.Create(LogLevel.Info, null, null, "{intvalue}", new object[] { 5 });
            var result = layout.RenderValue(logevent);

            // Assert
            Assert.Equal(5, result);
            Assert.Equal("5", layout.Render(logevent));
            Assert.Null(layout.StaticValue);
        }

        [Fact]
        public void LayoutDynamicNullIntValueTest()
        {
            // Arrange
            Layout<int?> layout = "${event-properties:intvalue}";

            // Act
            var logevent = LogEventInfo.Create(LogLevel.Info, null, null, "{intvalue}", new object[] { null });
            var result = layout.RenderValue(logevent);
            var result5 = layout.RenderValue(logevent, 5);

            // Assert
            Assert.Null(result);
            Assert.Equal(5, result5);
            Assert.Equal("", layout.Render(logevent));
            Assert.Null(layout.StaticValue);
        }

        [Fact]
        public void LayoutDynamicUrlValueTest()
        {
            // Arrange
            Layout<Uri> layout = "${event-properties:urlvalue}";
            var uri = new Uri("http://nlog");

            // Act
            var logevent = LogEventInfo.Create(LogLevel.Info, null, null, "{urlvalue}", new object[] { uri.ToString() });
            var result = layout.RenderValue(logevent);

            // Assert
            Assert.Equal(uri, result);
            Assert.Same(result, layout.RenderValue(logevent));
            Assert.Equal(uri.ToString(), layout.Render(logevent));
            Assert.Null(layout.StaticValue);
        }

        [Fact]
        public void LayoutDynamicNullUrlValueTest()
        {
            // Arrange
            Layout<Uri> layout = "${event-properties:urlvalue}";
            Uri uri = null;

            // Act
            var logevent = LogEventInfo.Create(LogLevel.Info, null, null, "{urlvalue}", new object[] { uri });
            var result = layout.RenderValue(logevent);

            // Assert
            Assert.Equal(uri, result);
            Assert.Same(result, layout.RenderValue(logevent));
            Assert.Equal("", layout.Render(logevent));
            Assert.Null(layout.StaticValue);
        }

        [Fact]
        public void LayoutDynamicIntValueAsyncTest()
        {
            // Arrange
            Layout<int> layout = "${scopeproperty:intvalue}";

            // Act
            var logevent = LogEventInfo.CreateNullEvent();
            using (ScopeContext.PushProperty("intvalue", 5))
            {
                layout.Precalculate(logevent);
            }

            // Assert
            Assert.Equal(5, layout.RenderValue(logevent));
            Assert.Equal("5", layout.Render(logevent));
            Assert.Equal(0, layout.StaticValue);
        }

        [Fact]
        public void LayoutDynamicNullableIntValueAsyncTest()
        {
            // Arrange
            Layout<int?> layout = "${scopeproperty:intvalue}";

            // Act
            var logevent = LogEventInfo.CreateNullEvent();
            using (ScopeContext.PushProperty("intvalue", 5))
            {
                layout.Precalculate(logevent);
            }

            // Assert
            Assert.Equal(5, layout.RenderValue(logevent));
            Assert.Equal("5", layout.Render(logevent));
            Assert.Null(layout.StaticValue);
        }

        [Fact]
        public void LayoutDynamicUrlValueAsyncTest()
        {
            // Arrange
            Layout<Uri> layout = "${scopeproperty:urlvalue}";
            var uri = new Uri("http://nlog");

            // Act
            var logevent = LogEventInfo.CreateNullEvent();
            using (ScopeContext.PushProperty("urlvalue", uri.ToString()))
            {
                layout.Precalculate(logevent);
            }

            // Assert
            Assert.Equal(uri, layout.RenderValue(logevent));
            Assert.Same(layout.RenderValue(logevent), layout.RenderValue(logevent));
            Assert.Equal(uri.ToString(), layout.Render(logevent));
            Assert.Null(layout.StaticValue);
        }

        [Fact]
        public void LayoutDynamicUrlValueRawAsyncTest()
        {
            // Arrange
            Layout<Uri> layout = "${event-properties:urlvalue}";
            var uri = new Uri("http://nlog");

            // Act
            var logevent = LogEventInfo.Create(LogLevel.Info, null, null, "{urlvalue}", new object[] { uri });
            layout.Precalculate(logevent);

            // Assert
            Assert.Same(uri, layout.RenderValue(logevent));
            Assert.Same(layout.RenderValue(logevent), layout.RenderValue(logevent));
            Assert.Same(uri, layout.RenderValue(logevent));
            Assert.Equal(uri.ToString(), layout.Render(logevent));
            Assert.Null(layout.StaticValue);
        }

        [Fact]
        public void LayoutDynamicRenderExceptionTypeTest()
        {
            // Arrange
            Layout<Type> layout = "${exception:format=type:norawvalue=true}";
            var exception = new System.ApplicationException("Test");
            var stringBuilder = new System.Text.StringBuilder();

            // Act
            var logevent = LogEventInfo.Create(LogLevel.Info, null, exception, null, "");
            var exceptionType = layout.RenderTypedValue(logevent, stringBuilder, null);
            stringBuilder.Length = 0;

            // Assert
            Assert.Equal(exception.GetType(), exceptionType);
            Assert.Same(exceptionType, layout.RenderTypedValue(logevent, stringBuilder, null));
        }

        [Fact]
        public void ComplexTypeTestWithStringConversion()
        {
            // Arrange
            var value = "utf8";
            var layout = CreateLayoutRenderedFromProperty<System.Text.Encoding>();
            var logEventInfo = CreateLogEventInfoWithValue(value);

            // Act
            var result = layout.RenderValue(logEventInfo);

            // Assert
            Assert.Equal(System.Text.Encoding.UTF8.EncodingName, result.EncodingName);
        }

        [Fact]
        public void LayoutRenderIntValueWhenNull()
        {
            // Arrange
            var integer = 42;
            Layout<int> layout = null;

            // Act
            var value = LayoutTypedExtensions.RenderValue(layout, null, integer);

            // Assert
            Assert.Equal(integer, value);
        }

        [Fact]
        public void LayoutRenderNullableIntValueWhenNull()
        {
            // Arrange
            var integer = 42;
            Layout<int?> layout = null;

            // Act
            var value = LayoutTypedExtensions.RenderValue(layout, null, integer);

            // Assert
            Assert.Equal(integer, value);
        }

        [Fact]
        public void LayoutRenderUrlValueWhenNull()
        {
            // Arrange
            var url = new Uri("http://nlog");
            Layout<Uri> layout = null;

            // Act
            var value = LayoutTypedExtensions.RenderValue(layout, null, url);

            // Assert
            Assert.Equal(url, value);
        }

        [Fact]
        public void LayoutEqualsIntValueFixedTest()
        {
            // Arrange
            Layout<int> layout1 = "42";
            Layout<int> layout2 = "42";

            // Act + Assert
            Assert.True(layout1 == 42);
            Assert.True(42 == layout1);
            Assert.True(layout1.Equals(42));
            Assert.True(layout1.Equals((object)42));
            Assert.Equal(layout1, layout2);
            Assert.Equal(layout1.GetHashCode(), layout2.GetHashCode());
        }

        [Fact]
        public void LayoutEqualsNullableIntValueFixedTest()
        {
            // Arrange
            Layout<int?> layout1 = "42";
            Layout<int?> layout2 = "42";

            // Act + Assert
            Assert.True(layout1 == 42);
            Assert.True(42 == layout1);
            Assert.True(layout1.Equals(42));
            Assert.True(layout1.Equals((object)42));
            Assert.Equal(layout1, layout2);
            Assert.Equal(layout1.GetHashCode(), layout2.GetHashCode());
        }

        [Fact]
        public void LayoutEqualsNullIntValueFixedTest()
        {
            // Arrange
            int? nullInt = null;
            Layout<int?> layout1 = nullInt;
            Layout<int?> layout2 = nullInt;

            // Act + Assert
            Assert.True(layout1 == nullInt);
            Assert.True(nullInt == layout1);
            Assert.True(layout1.Equals(nullInt));
            Assert.True(layout1.Equals((object)nullInt));
            Assert.Equal(layout1, layout2);
            Assert.Equal(layout1.GetHashCode(), layout2.GetHashCode());
        }

        [Fact]
        public void LayoutNotEqualsIntValueFixedTest()
        {
            // Arrange
            Layout<int> layout1 = "2";
            Layout<int> layout2 = "42";

            // Act + Assert
            Assert.False(layout1 == 42);
            Assert.False(42 == layout1);
            Assert.False(layout1.Equals(42));
            Assert.False(layout1.Equals((object)42));
            Assert.NotEqual(layout1, layout2);
            Assert.NotEqual(layout1.GetHashCode(), layout2.GetHashCode());
        }

        [Fact]
        public void LayoutNotEqualsNullableIntValueFixedTest()
        {
            // Arrange
            Layout<int?> layout1 = "2";
            Layout<int?> layout2 = "42";

            // Act + Assert
            Assert.False(layout1 == 42);
            Assert.False(42 == layout1);
            Assert.False(layout1.Equals(42));
            Assert.False(layout1.Equals((object)42));
            Assert.NotEqual(layout1, layout2);
            Assert.NotEqual(layout1.GetHashCode(), layout2.GetHashCode());
        }

        [Fact]
        public void LayoutNotEqualsNullIntValueFixedTest()
        {
            // Arrange
            int? nullInt = null;
            Layout<int?> layout1 = "2";
            Layout<int?> layout2 = nullInt;

            // Act + Assert
            Assert.False(layout1 == nullInt);
            Assert.False(nullInt == layout1);
            Assert.False(layout1.Equals(nullInt));
            Assert.False(layout1.Equals((object)nullInt));
            Assert.NotEqual(layout1, layout2);
            Assert.NotEqual(layout1.GetHashCode(), layout2.GetHashCode());
        }

        [Fact]
        public void LayoutEqualsUrlValueFixedTest()
        {
            // Arrange
            var url = new Uri("http://nlog");
            Layout<Uri> layout1 = url;
            Layout<Uri> layout2 = url;

            // Act + Assert
            Assert.True(layout1 == url);
            Assert.True(url == layout1);
            Assert.True(layout1.Equals(url));
            Assert.True(layout1.Equals((object)url));
            Assert.Equal(layout1, layout2);
            Assert.Equal(layout1.GetHashCode(), layout2.GetHashCode());
        }

        [Fact]
        public void LayoutEqualsNullUrlValueFixedTest()
        {
            // Arrange
            Uri url = null;
            Layout<Uri> layout1 = url;
            Layout<Uri> layout2 = url;

            // Act + Assert
            Assert.True(layout1 == url);
            Assert.True(url == layout1);
            Assert.True(layout1.Equals(url));
            Assert.True(layout1.Equals((object)url));
            Assert.Equal(layout1, layout2);
            Assert.Equal(layout1.GetHashCode(), layout2.GetHashCode());
        }

        [Fact]
        public void LayoutNotEqualsUrlValueFixedTest()
        {
            // Arrange
            var url = new Uri("http://nlog");
            var url2 = new Uri("http://nolog");
            Layout<Uri> layout1 = url2;
            Layout<Uri> layout2 = url;

            // Act + Assert
            Assert.False(layout1 == url);
            Assert.False(url == layout1);
            Assert.False(layout1.Equals(url));
            Assert.False(layout1.Equals((object)url));
            Assert.NotEqual(layout1, layout2);
            Assert.NotEqual(layout1.GetHashCode(), layout2.GetHashCode());
        }

        [Fact]
        public void LayoutNotEqualsNullUrlValueFixedTest()
        {
            // Arrange
            Uri url = null;
            var url2 = new Uri("http://nlog");
            Layout<Uri> layout1 = url2;
            Layout<Uri> layout2 = url;

            // Act + Assert
            Assert.False(layout1 == url);
            Assert.False(url == layout1);
            Assert.False(layout1.Equals(url));
            Assert.False(layout1.Equals((object)url));
            Assert.NotEqual(layout1, layout2);
            Assert.NotEqual(layout1.GetHashCode(), layout2.GetHashCode());
        }

        [Fact]
        public void LayoutEqualsIntValueDynamicTest()
        {
            // Arrange
            Layout<int> layout1 = "${event-properties:intvalue}";
            Layout<int> layout2 = "${event-properties:intvalue}";

            // Act + Assert (LogEventInfo.LayoutCache must work)
            Assert.NotEqual(layout1, layout2);
            Assert.NotEqual(layout1.GetHashCode(), layout2.GetHashCode());
        }

        [Fact]
        public void LayoutEqualsNullableIntValueDynamicTest()
        {
            // Arrange
            Layout<int?> layout1 = "${event-properties:intvalue}";
            Layout<int?> layout2 = "${event-properties:intvalue}";

            // Act + Assert (LogEventInfo.LayoutCache must work)
            Assert.NotEqual(layout1, layout2);
            Assert.NotEqual(layout1.GetHashCode(), layout2.GetHashCode());
        }

        [Fact]
        public void LayoutEqualsUrlValueDynamicTest()
        {
            // Arrange
            Layout<Uri> layout1 = "${event-properties:urlvalue}";
            Layout<Uri> layout2 = "${event-properties:urlvalue}";

            // Act + Assert (LogEventInfo.LayoutCache must work)
            Assert.NotEqual(layout1, layout2);
            Assert.NotEqual(layout1.GetHashCode(), layout2.GetHashCode());
        }

        [Theory]
        [InlineData(100)]
        [InlineData(100d)]
        [InlineData("100")]
        [InlineData(" 100 ")]
        public void TypedIntLayoutDynamicTest(object value)
        {
            // Arrange
            var layout = CreateLayoutRenderedFromProperty<int>();
            var logEventInfo = CreateLogEventInfoWithValue(value);

            // Act
            var result = layout.RenderValue(logEventInfo);

            // Assert
            Assert.Equal(100, result);
        }

        [Fact]
        public void TypedNullableIntToIntLayoutDynamicTest()
        {
            // Arrange
            var layout = CreateLayoutRenderedFromProperty<int>();
            int? value = 100;
            var logEventInfo = CreateLogEventInfoWithValue(value);

            // Act
            var result = layout.RenderValue(logEventInfo);

            // Assert
            Assert.Equal(100, result);
        }

        [Theory]
        [InlineData(100)]
        [InlineData(100d)]
        [InlineData("100")]
        [InlineData(" 100 ")]
        public void TypedNullableIntLayoutDynamicTest(object value)
        {
            // Arrange
            var layout = CreateLayoutRenderedFromProperty<int?>();
            var logEventInfo = CreateLogEventInfoWithValue(value);

            // Act
            var result = layout.RenderValue(logEventInfo);

            // Assert
            Assert.Equal(100, result);
        }

        [Theory]
        [InlineData(100.5)]
        [InlineData("100.5", "EN-us")]
        [InlineData("  100.5  ", "EN-us")]
        public void TypedDecimalLayoutDynamicTest(object value, string culture = null)
        {
            // Arrange
            var oldCulture = System.Threading.Thread.CurrentThread.CurrentCulture;

            try
            {
                if (!string.IsNullOrEmpty(culture))
                {
                    System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(culture);
                }

                var layout = CreateLayoutRenderedFromProperty<decimal>();
                var logEventInfo = CreateLogEventInfoWithValue(value);

                // Act
                var result = layout.RenderValue(logEventInfo);

                // Assert
                decimal expected = 100.5m;
                Assert.Equal(expected, result);
            }
            finally
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = oldCulture;
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData("true")]
        [InlineData(" true ")]
        public void TypedBoolLayoutDynamicTest(object value)
        {
            // Arrange
            var layout = CreateLayoutRenderedFromProperty<bool>();
            var logEventInfo = CreateLogEventInfoWithValue(value);

            // Act
            var result = layout.RenderValue(logEventInfo);

            // Assert
            Assert.True(result);
        }

        /// <remarks>Cache usage, see coverage result</remarks>
        [Fact]
        public void SameValueShouldUseCacheAndCorrectResult()
        {
            // Arrange
            var layout = CreateLayoutRenderedFromProperty<bool>();
            var logEventInfo1 = CreateLogEventInfoWithValue("true");
            var logEventInfo2 = CreateLogEventInfoWithValue("true");
            var logEventInfo3 = CreateLogEventInfoWithValue("true");

            // Act
            var result1 = layout.RenderValue(logEventInfo1);
            var result2 = layout.RenderValue(logEventInfo2);
            var result3 = layout.RenderValue(logEventInfo3);

            // Assert
            Assert.True(result1);
            Assert.True(result2);
            Assert.True(result3);
        }

        [Fact]
        public void ComplexTypeTest()
        {
            // Arrange + Act + Assert
            Assert.Throws<NLogConfigurationException>(() => CreateLayoutRenderedFromProperty<TestObject>());
        }

        [Fact]
        public void WrongValueErrorTest()
        {
            // Arrange
            var value = "12312aa3";
            var layout = CreateLayoutRenderedFromProperty<int>();
            var logEventInfo = CreateLogEventInfoWithValue(value);

            // Act
            var result = layout.RenderValue(logEventInfo);

            // Assert
            Assert.Equal(0, result);
        }

        [Theory]
        [InlineData("100", 100)]
        [InlineData("1  00", null)]
        public void TryGetRawValueTest(string input, int? expected)
        {
            // Arrange
            var layout = new Layout<int?>("${event-properties:prop1}");
            var logEventInfo = LogEventInfo.CreateNullEvent();
            logEventInfo.Properties["prop1"] = input;

            // Act
            var result = layout.RenderValue(logEventInfo);

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void TryConvertToShouldHandleNullLayout()
        {
            // Arrange
            var layout = new Layout<int>((Layout)null);
            var logEventInfo = LogEventInfo.CreateNullEvent();

            // Act
            var result = layout.RenderValue(logEventInfo);

            // Assert
            Assert.Equal(0, result);
        }

        [Fact]
        public void RenderShouldHandleInvalidConversionAndLogFactory()
        {
            // Arrange
            var factory = new LogFactory
            {
                ThrowExceptions = false,
            };
            var configuration = new NLog.Config.LoggingConfiguration(factory);

            var layout = new Layout<int>("${event-properties:prop1}");
            layout.Initialize(configuration);
            var logEventInfo = LogEventInfo.CreateNullEvent();
            logEventInfo.Properties["prop1"] = "Not a int";

            // Act
            int result;
            using (new NoThrowNLogExceptions())
            {
                result = layout.RenderValue(logEventInfo);
            }

            // Assert
            Assert.Equal(0, result);
        }

        [Fact]
        public void RenderShouldHandleInvalidConversionAndLogFactoryAndDefault()
        {
            // Arrange
            var factory = new LogFactory
            {
                ThrowExceptions = false,
            };
            var configuration = new NLog.Config.LoggingConfiguration(factory);

            var layout = new Layout<int>("${event-properties:prop1}");
            layout.Initialize(configuration);
            var logEventInfo = LogEventInfo.CreateNullEvent();
            logEventInfo.Properties["prop1"] = "Not a int";

            // Act
            int result;
            using (new NoThrowNLogExceptions())
            {
                result = layout.RenderValue(logEventInfo, 200);
            }

            // Assert
            Assert.Equal(200, result);
        }

        [Fact]
        public void RenderShouldHandleValidConversion()
        {
            // Arrange
            var layout = new Layout<int>("${event-properties:prop1}");
            var logEventInfo = LogEventInfo.CreateNullEvent();
            logEventInfo.Properties["prop1"] = "100";

            // Act
            var result = layout.RenderValue(logEventInfo);

            // Assert
            Assert.Equal(100, result);
        }

        private class TestObject
        {
            public string Value { get; set; }
        }

        private static Layout<T> CreateLayoutRenderedFromProperty<T>()
        {
            var layout = new Layout<T>("${event-properties:value1}");
            return layout;
        }

        private static LogEventInfo CreateLogEventInfoWithValue(object value)
        {
            var logEventInfo = LogEventInfo.Create(LogLevel.Info, "logger1", "message1");
            logEventInfo.Properties.Add("value1", value);
            return logEventInfo;
        }
    }
}
