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
        }

        [Fact]
        public void LayoutDynamicIntValueTest()
        {
            // Arrange
            Layout<int> layout = "${event-properties:intvalue}";

            // Act
            var logevent = LogEventInfo.Create(LogLevel.Info, null, null, "{intvalue}", new object[] { 5 });
            var result = layout.RenderValue(logevent);

            // Assert
            Assert.Equal(5, result);
            Assert.Equal("5", layout.Render(logevent));
            Assert.Equal(0, layout.StaticValue);
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
    }
}
