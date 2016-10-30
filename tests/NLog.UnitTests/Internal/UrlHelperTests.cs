﻿// 
// Copyright (c) 2004-2016 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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


#if !SILVERLIGHT
//no silverlight for xunit InlineData

using NLog.Internal;
using NLog.Layouts;
using Xunit;
using Xunit.Extensions;

namespace NLog.UnitTests.Internal
{
    public class UrlHelperTests
    {
        [Theory]
        [InlineData("", true, "")]
        [InlineData("", false, "")]
        [InlineData(null, false, "")]
        [InlineData(null, true, "")]
        [InlineData("ab cd", true,"ab+cd")]
        [InlineData("ab cd", false, "ab%20cd")]
        [InlineData("one&two =three", true, "one%26two+%3dthree")]
        [InlineData("one&two =three", false, "one%26two%20%3dthree")]
        [InlineData(" €;✈ Ĕ  ßß ßß ", true, "+%u20ac%3b%u2708+%u0114++%df%df+%df%df+")] //current implementation, not sure if correct
        [InlineData(" €;✈ Ĕ  ßß ßß ", false, "%20%u20ac%3b%u2708%20%u0114%20%20%df%df%20%df%df%20")] //current implementation, not sure if correct
        [InlineData(".()*-_!ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz", true, ".()*-_!ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz")]
        [InlineData(".()*-_!ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz", false, ".()*-_!ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz")]
        [InlineData("《∠∠⊙⌒∈∽》`````", true, "%u300a%u2220%u2220%u2299%u2312%u2208%u223d%u300b%60%60%60%60%60")] //current implementation, not sure if correct
        public void UrlEncodeTest(string input, bool spaceAsPlus, string result)
        {
            SimpleLayout l = spaceAsPlus ? "${url-encode:escapeDataNLogLegacy=true:${message}}" : "${url-encode:escapeDataNLogLegacy=true:spaceAsPlus=false:${message}}";
            string urlencoded = l.Render(LogEventInfo.Create(LogLevel.Debug, "logger", input));
            Assert.Equal(result, urlencoded);
        }

        [Theory]
        [InlineData("", true, "")]
        [InlineData("", false, "")]
        [InlineData("ab cd", true, "ab+cd")]
        [InlineData("ab cd", false, "ab%20cd")]
        [InlineData("one&two =three", true, "one%26two+%3dthree")]
        [InlineData("one&two =three", false, "one%26two%20%3dthree")]
        [InlineData(".()*-_!ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz", true, ".()*-_!ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz")]
        [InlineData(".()*-_!ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz", false, ".()*-_!ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz")]
        public void EscapeDataEncodeTestRfc2396(string input, bool spaceAsPlus, string result)
        {
            System.Text.StringBuilder builder = new System.Text.StringBuilder(input.Length + 20);
            UrlHelper.EscapeEncodingFlag encodingFlags = UrlHelper.EscapeEncodingFlag.LowerCaseHex | UrlHelper.EscapeEncodingFlag.LegacyRfc2396 | UrlHelper.EscapeEncodingFlag.UriString;
            if (spaceAsPlus)
                encodingFlags |= UrlHelper.EscapeEncodingFlag.SpaceAsPlus;
            UrlHelper.EscapeDataEncode(input, builder, encodingFlags);
            Assert.Equal(result, builder.ToString());
        }

        [Theory]
        [InlineData("", true, "")]
        [InlineData("", false, "")]
        [InlineData("ab cd", true, "ab+cd")]
        [InlineData("ab cd", false, "ab%20cd")]
        [InlineData(@"http://foo:bar@host:123/path?query#fragment", true, @"http://foo:bar@host:123/path?query#fragment")]
        [InlineData(@"http://foo:bar@host:123/path?query#fragment", false, @"http://foo:bar@host:123/path?query#fragment")] 
        public void EscapeDataEncodeTestRfc3986(string input, bool spaceAsPlus, string result)
        {
            System.Text.StringBuilder builder = new System.Text.StringBuilder(input.Length + 20);
            UrlHelper.EscapeEncodingFlag encodingFlags = UrlHelper.EscapeEncodingFlag.None;
            if (spaceAsPlus)
                encodingFlags |= UrlHelper.EscapeEncodingFlag.SpaceAsPlus;
            UrlHelper.EscapeDataEncode(input, builder, encodingFlags);
            Assert.Equal(result, builder.ToString());
        }
    }
}
#endif