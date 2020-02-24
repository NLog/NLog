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
using NLog.Internal;
using Xunit;

namespace NLog.UnitTests.Internal
{
    public class StringHelpersTests
    {
        [Theory]
        [InlineData("", true)]
        [InlineData("  ", true)]
        [InlineData("  \n", true)]
        [InlineData("  \na", false)]
        [InlineData("a", false)]
        [InlineData(" a", false)]
        public void IsNullOrWhiteSpaceTest(string input, bool result)
        {
            Assert.Equal(result, StringHelpers.IsNullOrWhiteSpace(input));
        }

        [Theory]
        [InlineData("", new string[0])]
        [InlineData("  ", new string[0])]
        [InlineData(" , ", new string[0])]
        [InlineData("a", new string[] { "a" })]
        [InlineData("a ", new string[] { "a" })]
        [InlineData(" a", new string[] { "a" })]
        [InlineData(" a,", new string[] { "a" })]
        [InlineData(" a, ", new string[] { "a" })]
        [InlineData("a,b", new string[] { "a", "b" })]
        [InlineData("a ,b", new string[] { "a", "b" })]
        [InlineData(" a ,b", new string[] { "a", "b" })]
        [InlineData(" a , b ", new string[] { "a", "b" })]
        [InlineData(" a b ", new string[] { "a b" })]
        public void SplitAndTrimToken(string input, string[] expected)
        {
            var result = input.SplitAndTrimTokens(',');
            Assert.Equal(expected, result);
        }


        [Theory]
        [InlineData("", "", "", StringComparison.InvariantCulture, "")]
        [InlineData("", "", null, StringComparison.InvariantCulture, "")]
        [InlineData("a", "a", "b", StringComparison.InvariantCulture, "b")]
        [InlineData("aa", "a", "b", StringComparison.InvariantCulture, "bb")]
        [InlineData("aa", "a", "", StringComparison.InvariantCulture, "")]
        [InlineData(" Caac ", "a", "", StringComparison.InvariantCulture, " Cc ")]
        [InlineData(" Caac ", "a", " ", StringComparison.InvariantCulture, " C  c ")]
        [InlineData("aA", "a", "b", StringComparison.InvariantCulture, "bA")]
        [InlineData("aA", "a", "b", StringComparison.InvariantCultureIgnoreCase, "bb")]
        [InlineData("œ", "œ", "", StringComparison.InvariantCulture, "")]
        [InlineData("œ", "oe", "", StringComparison.OrdinalIgnoreCase, "œ")]
        [InlineData("var ${var}", "${var}", "2", StringComparison.InvariantCulture, "var 2")]
        [InlineData("var ${var}", "${VAR}", "2", StringComparison.InvariantCulture, "var ${var}")]
        [InlineData("var ${VAR}", "${var}", "2", StringComparison.InvariantCulture, "var ${VAR}")]
        [InlineData("var ${var}", "${VAR}", "2", StringComparison.InvariantCultureIgnoreCase, "var 2")]
        [InlineData("var ${VAR}", "${var}", "2", StringComparison.InvariantCultureIgnoreCase, "var 2")]
        public void ReplaceTest(string input, string search, string replace, StringComparison comparer, string result)
        {
            Assert.Equal(result, StringHelpers.Replace(input, search, replace, comparer));
        }

        [Theory]
        [InlineData(null, "", "", StringComparison.InvariantCulture)]
        [InlineData("", null, "", StringComparison.InvariantCulture)]
        public void ReplaceTestThrowsNullException(string input, string search, string replace, StringComparison comparer)
        {
            Assert.Throws<ArgumentNullException>(() => StringHelpers.Replace(input, search, replace, comparer));
        }
    }
}
