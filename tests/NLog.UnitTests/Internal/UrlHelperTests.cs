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
        [InlineData(" €;✈ Ĕ  ßß ßß ", true, "+%e2%82%ac%3b%e2%9c%88+%c4%94++%c3%9f%c3%9f+%c3%9f%c3%9f+")]
        [InlineData(" €;✈ Ĕ  ßß ßß ", false, "%20%e2%82%ac%3b%e2%9c%88%20%c4%94%20%20%c3%9f%c3%9f%20%c3%9f%c3%9f%20")]
        [InlineData(".()*-_!ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz", true, ".()*-_!ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz")]
        [InlineData(".()*-_!ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz", false, ".()*-_!ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz")]
        [InlineData(@"http://foo:bar@host:123/path?query=key_value+map_value#fragment", true, @"http%3a%2f%2ffoo%3abar%40host%3a123%2fpath%3fquery%3dkey_value%2bmap_value%23fragment")]
        [InlineData(@"http://foo:bar@host:123/path?query=key_value+map_value#fragment", false, @"http%3a%2f%2ffoo%3abar%40host%3a123%2fpath%3fquery%3dkey_value%2bmap_value%23fragment")]
        public void EscapeDataEncodeTestRfc2396(string input, bool spaceAsPlus, string result)
        {
            System.Text.StringBuilder builder = new System.Text.StringBuilder(input.Length + 20);
            UrlHelper.EscapeEncodingOptions encodingOptions = UrlHelper.EscapeEncodingOptions.LowerCaseHex | UrlHelper.EscapeEncodingOptions.LegacyRfc2396 | UrlHelper.EscapeEncodingOptions.UriString;
            if (spaceAsPlus)
                encodingOptions |= UrlHelper.EscapeEncodingOptions.SpaceAsPlus;
            UrlHelper.EscapeDataEncode(input, builder, encodingOptions);
            Assert.Equal(result, builder.ToString());
            Assert.Equal(input.Replace('+', ' '), DecodeUrlString(builder.ToString()));
        }

        [Theory]
        [InlineData("", true, "")]
        [InlineData("", false, "")]
        [InlineData("ab cd", true, "ab+cd")]
        [InlineData("ab cd", false, "ab%20cd")]
        [InlineData(" €;✈ Ĕ  ßß ßß ", true, "+%E2%82%AC;%E2%9C%88+%C4%94++%C3%9F%C3%9F+%C3%9F%C3%9F+")]
        [InlineData(" €;✈ Ĕ  ßß ßß ", false, "%20%E2%82%AC;%E2%9C%88%20%C4%94%20%20%C3%9F%C3%9F%20%C3%9F%C3%9F%20")]
        [InlineData(@"http://foo:bar@host:123/path?query=key_value+map_value#fragment", true, @"http://foo:bar@host:123/path?query=key%5Fvalue+map%5Fvalue#fragment")]
        [InlineData(@"http://foo:bar@host:123/path?query=key_value+map_value#fragment", false, @"http://foo:bar@host:123/path?query=key%5Fvalue+map%5Fvalue#fragment")] 
        public void EscapeDataEncodeTestRfc3986(string input, bool spaceAsPlus, string result)
        {
            System.Text.StringBuilder builder = new System.Text.StringBuilder(input.Length + 20);
            UrlHelper.EscapeEncodingOptions encodingOptions = UrlHelper.EscapeEncodingOptions.None;
            if (spaceAsPlus)
                encodingOptions |= UrlHelper.EscapeEncodingOptions.SpaceAsPlus;
            UrlHelper.EscapeDataEncode(input, builder, encodingOptions);
            Assert.Equal(result, builder.ToString());
            Assert.Equal(input.Replace('+', ' '), DecodeUrlString(builder.ToString()));
        }

        private static string DecodeUrlString(string url)
        {
            string newUrl;
            while ((newUrl = System.Uri.UnescapeDataString(url)) != url)
                url = newUrl;
            return newUrl.Replace('+', ' ');
        }
    }
}
