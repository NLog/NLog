// 
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


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog.Internal;
using Xunit;
using Xunit.Extensions;

namespace NLog.UnitTests.Internal
{
    public class StringSplitterTests
    {

        [Theory]
        [InlineData("abc", ';', '\\', "abc")]
        [InlineData("abc", ';', ';', "abc")]
        [InlineData("  abc", ';', '\\', "  abc")]
        [InlineData("  abc", ';', ';', "  abc")]
        [InlineData(null, ';', '\\', "")]
        [InlineData(null, ';', ';', "")]
        [InlineData("", ';', '\\', "")]
        [InlineData("", ';', ';', "")]
        [InlineData("   ", ';', '\\', "   ")]
        [InlineData("   ", ';', ';', "   ")]
        [InlineData(@"abc", ';', ',', "abc")]
        [InlineData(@"a;b;c", ';', '\\', "a,b,c")]
        [InlineData(@"a;b;c;", ';', '\\', "a,b,c")]
        [InlineData(@";a;b;c", ';', '\\', ",a,b,c")]
        [InlineData(@"a;;b;c;", ';', '\\', "a,,b,c")]
        [InlineData(@"a\b;c", ';', '\\', @"a\b,c")]
        [InlineData(@"a\;b;c", ';', '\\', @"a;b,c")]
        [InlineData(@"a\;b\;c", ';', '\\', @"a;b;c")]
        [InlineData(@"a\;b\;c;d", ';', '\\', @"a;b;c,d")]
        [InlineData(@"a\;b\;c;d", ';', '\\', @"a;b;c,d")]
        [InlineData(@"a\;b;c\;d", ';', '\\', @"a;b,c;d")]
        [InlineData(@"a;b;;c", ';', ';', @"a,b;c")]
        [InlineData(@"a;b;;;;c"   , ';', ';', @"a,b;;c")]
        [InlineData(@"a;;b"   , ';', ';', @"a;b")]
        [InlineData(@"abc"   , ';', ';', @"abc")]
        [InlineData(@"abc\;"   , ';', '\\', @"abc;")]
        [InlineData(@"abc;;"   , ';', ';', @"abc;")]

        void SplitStringWithEscape(string input, char splitChar, char escapeChar, string output)
        {
            var strings = input.SplitWithEscape(splitChar, escapeChar).ToArray();
            var result = string.Join(",", strings);
            Assert.Equal(output, result);
        }
    }
}
