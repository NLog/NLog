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
using System.Linq;
using System.Runtime.InteropServices;
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
            var logEvent1 = new LogEventInfo
            {
                Exception = new ArgumentException("ArgumentException with param name", "MyParam")
            };
            var logEvent = logEvent1;
            Layout l = layout;

            // Act
            var result = l.Render(logEvent);

            // Assert
            Assert.Equal("MyParam", result);
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
    }
}
