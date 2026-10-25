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
using System.Linq;
using NLog.Internal;
using Xunit;

namespace NLog.UnitTests.Internal
{
    public class StringSplitterTests
    {
        private const char SingleQuote = '\'';
        private const char Backslash = '\\';

        [Theory]
        [InlineData("abc", ';', "abc")]
        [InlineData("  abc", ';', "  abc")]
        [InlineData(null, ';', "")]
        [InlineData("", ';', "")]
        [InlineData(";", ';', ",")]
        [InlineData("a;", ';', "a,")]
        [InlineData("a; ", ';', "a, ")]
        [InlineData("   ", ';', "   ")]
        [InlineData(@"a;b;;c", ';', @"a,b,,c")]
        [InlineData(@"a;b;;;;c", ';', @"a,b,,,,c")]
        [InlineData(@"a;;b", ';', @"a,,b")]
        [InlineData(@"a'b'c''d", SingleQuote, @"a,b,c,,d")]
        [InlineData(@"'a", SingleQuote, @",a")]
        public void SplitStringWithSelfEscape(string input, char splitChar, string output)
        {
            var quoteChar = splitChar == SingleQuote ? '"' : SingleQuote;
            var strings = StringSplitter.SplitQuoted(input, splitChar, quoteChar, quoteChar).ToArray();
            var result = string.Join(",", strings);
            Assert.Equal(output, result);
        }

        [Theory]
        [InlineData("abc", "abc")]
        [InlineData("  abc", "  abc")]
        [InlineData(null, "")]
        [InlineData("", "")]
        [InlineData("   ", "   ")]
        [InlineData(@"a;b;c", "a,b,c")]
        [InlineData(@"a;b;c;", "a,b,c,")]
        [InlineData(@"a;", "a,")]
        [InlineData(@";", ",")]
        [InlineData(@";a;b;c", ",a,b,c")]
        [InlineData(@"a;;b;c;", "a,,b,c,")]
        [InlineData(@"a\b;c", @"a\b,c")]
        [InlineData(@"a;b;c\", @"a,b,c\")]
        [InlineData(@"a;b;c\;", @"a,b,c\,")]
        [InlineData(@"a;b;c\;;", @"a,b,c\,,")]
        [InlineData(@"a\;b;c", @"a\,b,c")]
        [InlineData(@"a\;b\;c", @"a\,b\,c")]
        [InlineData(@"a\;b\;c;d", @"a\,b\,c,d")]
        [InlineData(@"a\;b;c\;d", @"a\,b,c\,d")]
        [InlineData(@"abc\;", @"abc\,")]
        public void SplitStringWithEscape(string input, string output)
        {
            // Not possible to escape the splitter without using quotes
            SplitStringWithEscape2(input, ';', Backslash, output);
        }

        [Theory]
        [InlineData(@"abc", ';', ',', "abc")]
        void SplitStringWithEscape2(string input, char splitChar, char escapeChar, string output)
        {
            var strings = StringSplitter.SplitQuoted(input, splitChar, SingleQuote, escapeChar).ToArray();
            var result = string.Join(",", strings);
            Assert.Equal(output, result);
        }

        /// <summary>
        /// Tests with ; as separator, quoted and escaped with '
        /// </summary>
        [Theory]
        [InlineData(@";", @",")]
        [InlineData(@";;", @",,")]
        [InlineData(@"a;", @"a,")]
        [InlineData(@"a;''b;c", "a,'b,c")]
        [InlineData(@"a;''b;c'", "a,'b,c'")]
        [InlineData(@"abc", "abc")]
        [InlineData(@"abc'", "abc'")]
        [InlineData(@"''abc'", "'abc'")]
        [InlineData(@"'abc'", "abc")]
        [InlineData(@"'ab;c'", "ab;c")]
        [InlineData(@"'ab\c'", @"ab\c")]
        [InlineData(@"'", @"'")]
        [InlineData(@"'a", @"'a")]
        [InlineData(@"a'", @"a'")]
        [InlineData(@"\", @"\")]
        [InlineData(@"a\", @"a\")]
        [InlineData(@"\b", @"\b")]
        [InlineData(@"'';a", "',a")]
        [InlineData(@"'a;b';c", "a;b,c")]
        [InlineData(@"a''b;c", "a''b,c")]
        [InlineData(@"a;'';b", "a,',b")]
        [InlineData(@"''", "'")]
        [InlineData(@"''''", "'''")]
        [InlineData(@"a'b;c", "a'b,c")]
        [InlineData(@"a; 'b;c'", "a, 'b,c'")]
        [InlineData(@"a;;", "a,,")]
        [InlineData(@";a", ",a")]
        [InlineData(@";;a", ",,a")]
        [InlineData(@"'a';", "a,")]
        [InlineData(@";'a'", ",a")]
        [InlineData(@"'a';'b'", "a,b")]
        [InlineData(@"'a;b';'c;d'", "a;b,c;d")]
        [InlineData(@"'a''b'", "a'b")]          // Bug fix as before expected = a,b (Split into 2 parts)
        [InlineData(@"'a''''b'", "a''b")]       // Bug fix as before expected = a,''b' (Split into 2 parts)
        [InlineData(@"'a''b;c'", "a'b;c")]      // Bug fix as before expected = a,b;c (Split into 2 parts)
        [InlineData(@"a'''b;c", "a'''b,c")]
        [InlineData(@"'''a';b", "''a',b")]
        [InlineData(@"'a''", "a'")]            // Todo How to handle ending with double quotes? (Old expected 2 parts = a,')
        [InlineData(@"'a''';b", "a',b")]        // Bug fix as before expected = a,',b (Split into 3 parts)
        [InlineData(@"'a''b';", "a'b,")]        // Bug fix as before expected = a,b, (Split into 3 parts)
        [InlineData(@"'a';;", "a,,")]
        [InlineData(@"'';''", "','")]
        [InlineData(@"a'b''c'", "a'b''c'")] 
        [InlineData(@"'ab''cd';e", "ab'cd,e")]  // Bug fix as before expected = ab,cd,e (Split into 3 parts)
        public void SplitStringWithQuotes_selfQuoted(string input, string output)
        {
            AssertSplit(input, ';', SingleQuote, SingleQuote, output);
        }

        /// <summary>
        /// Tests with ; as separator, quoted with single quote and escaped with backslash
        /// </summary>
        [Theory]
        [InlineData(null, "")]
        [InlineData("", "")]
        [InlineData(@"\", @"\")]
        [InlineData(@"'", @"'")]
        [InlineData(@"' ", @"' ")]
        [InlineData(@"a'", "a'")]
        [InlineData(@" ' ", @" ' ")]
        [InlineData(@" ; ", @" , ")]
        [InlineData(@";", @",")]
        [InlineData(@";;", @",,")]
        [InlineData(@"abc", "abc")]
        [InlineData(@"abc;", "abc,")]
        [InlineData(@"abc'", "abc'")]
        [InlineData(@"abc\", @"abc\")]
        [InlineData(@"abc\\", @"abc\\")]
        [InlineData(@"abc\b", @"abc\b")]
        [InlineData(@"a;b;c", "a,b,c")]
        [InlineData(@"a;'b;c'", "a,b;c")]
        [InlineData(@"a;'b;c", "a,'b;c")]
        [InlineData(@"a;b'c;d", "a,b'c,d")]
        [InlineData(@"a;\'b;c", "a,'b,c")]
        [InlineData(@"'a;b';c", "a;b,c")]
        [InlineData("a; 'b;c'", "a, 'b,c'")]
        [InlineData("a'b;c", "a'b,c")]
        [InlineData(@"a\;b;c", @"a\,b,c")]
        [InlineData(@"\b", @"\b")]
        [InlineData(@"'\'", @"\")]     //todo check case (How to handle escape before last quote?)
        [InlineData(@"'\\'", @"\")]    //todo check case (How to handle escape before last quote?)
        [InlineData(@"'\\\'", @"\\")]  //todo check case (How to handle escape before last quote?)
        [InlineData(@"'\\\\'", @"\\")]
        [InlineData(@"'\''", "'")]
        [InlineData(@"'\\''", @"\,'")]
        [InlineData(@"'a\'b';c", "a'b,c")]
        [InlineData(@"a\'b;c", "a'b,c")]
        [InlineData(@"'a\\b';c", @"a\b,c")]
        [InlineData(@"'abc\", @"'abc\")]
        [InlineData(@"'abc\';", @"'abc';")]
        [InlineData(@"'abc", @"'abc")]
        [InlineData(@"a''b;c", "a''b,c")]
        [InlineData(@"a\\b;c", @"a\\b,c")]
        [InlineData(@"a\\;b;c", @"a\\,b,c")]
        [InlineData(@"a\;;b", @"a\,,b")]
        [InlineData(@"'a\'b'", "a'b")]
        [InlineData(@"a\'b", "a'b")]
        [InlineData(@"'a\\b'", @"a\b")]
        [InlineData("a;;c;", "a,,c,")]
        [InlineData(";a", ",a")]
        [InlineData("a;", "a,")]
        [InlineData("a;;b", "a,,b")]
        [InlineData(@"\;a", @"\,a")]
        [InlineData(@"a\;", @"a\,")]
        [InlineData(@"'abc'", "abc")]
        [InlineData(@"a;'b'", "a,b")]
        [InlineData(@"'a';'b'", "a,b")]
        [InlineData(@"a;;", "a,,")]
        [InlineData(@"a;;b;", "a,,b,")]
        [InlineData(@"a;b\", @"a,b\")]
        [InlineData(@"'abc''", "abc,'")]
        [InlineData("a;'';b", "a,,b")]
        [InlineData(@"'\a'", @"a")]
        [InlineData("'a' ", "a, ")]
        [InlineData("'a'a", "a,a")]
        [InlineData("' a ' ", " a , ")]
        [InlineData("'a' 'b'", "a, 'b'")]
        [InlineData("'a'a; b", "a,a, b")]
        [InlineData("'a' ; b", "a, , b")]
        [InlineData("' a ' ; b", " a , , b")]
        [InlineData(@"a\''b;c", @"a''b,c")]
        [InlineData(@"'a''b';c", @"a,b,c")]
        public void SplitStringWithQuotes_escaped(string input, string output)
        {
            AssertSplit(input, ';', SingleQuote, Backslash, output);
        }

        private static void AssertSplit(string input, char splitChar, char quoteChar, char escapeChar, string output)
        {
            var strings = StringSplitter.SplitQuoted(input, splitChar, quoteChar, escapeChar).ToArray();
            var result = string.Join(",", strings);
            Assert.Equal(output, result);
        }

        [Theory]
        [InlineData(';', ';', Backslash)]
        [InlineData(';', Backslash, ';')]
        public void SplitStringNotSupported(char splitChar, char quoteChar, char escapeChar)
        {
            Assert.Throws<NotSupportedException>(() => StringSplitter.SplitQuoted("dont care", splitChar, quoteChar, escapeChar));
        }
    }
}
