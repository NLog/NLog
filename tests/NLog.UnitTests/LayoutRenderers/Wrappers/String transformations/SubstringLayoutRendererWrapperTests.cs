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

using NLog.Layouts;
using Xunit;
using Xunit.Extensions;

namespace NLog.UnitTests.LayoutRenderers.Wrappers
{
    public class SubstringLayoutRendererWrapperTests
    {

        [Theory]
        [InlineData(":length=2", "12")]
        [InlineData(":length=0", "")]
        [InlineData(":start=10", "")]
        [InlineData(":start=9", "0")]
        [InlineData(":length=3", "before123", "before")]
        [InlineData(":length=3", "before123end", "before", "end")]
        [InlineData(":length=2:start=1", "23")]
        [InlineData(":length=2:start=100", "")]
        [InlineData(":length=100", "1234567890")]
        [InlineData("", "1234567890")]
        [InlineData(":start=0", "1234567890")]
        [InlineData(":start=-2:length=2", "90")]
        [InlineData(":start=-2", "90")]
        [InlineData(":start=-1:length=2", "0")] //won't take chars from start after starting at end.
        [InlineData(":length=-1", "")]
        public void SubstringWrapperTest(string options, string expected, string prefixText = null, string afterText = null)
        {
            SimpleLayout l = $"{prefixText}${{substring:${{message}}{options}}}{afterText}";
            var result = l.Render(LogEventInfo.Create(LogLevel.Debug, "substringTest", "1234567890"));
            Assert.Equal(expected, result);
        }
    }
}