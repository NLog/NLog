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

namespace NLog.UnitTests.LayoutRenderers.Wrappers
{
    using NLog;
    using NLog.Layouts;
    using Xunit;

    public class ReplaceTests : NLogTestBase
    {
        [Fact]
        public void ReplaceTestWithoutRegEx()
        {
            // Arrange
            SimpleLayout layout = @"${replace:inner=${message}:searchFor=foo:replaceWith=BAR}";

            // Act
            var result = layout.Render(new LogEventInfo(LogLevel.Info, "Test", " foo bar bar foo bar FOO"));

            // Assert
            Assert.Equal(" BAR bar bar BAR bar FOO", result);
        }

        [Fact]
        public void ReplaceTestIgnoreCaseWithoutRegEx()
        {
            // Arrange
            SimpleLayout layout = @"${replace:inner=${message}:searchFor=foo:replaceWith=BAR:ignorecase=true}";

            // Act
            var result = layout.Render(new LogEventInfo(LogLevel.Info, "Test", " foo bar bar foo bar FOO"));

            // Assert
            Assert.Equal(" BAR bar bar BAR bar BAR", result);
        }

        [Fact]
        public void ReplaceTestWholeWordsWithoutRegEx()
        {
            // Arrange
            SimpleLayout layout = @"${replace:inner=${message}:searchFor=foo:replaceWith=BAR:ignorecase=true:WholeWords=true}";

            // Act
            var result = layout.Render(new LogEventInfo(LogLevel.Info, "Test", "foo bar bar foobar barfoo bar FOO"));

            // Assert
            Assert.Equal("BAR bar bar foobar barfoo bar BAR", result);
        }
    }
}
