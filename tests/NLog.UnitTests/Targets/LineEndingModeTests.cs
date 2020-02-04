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

namespace NLog.UnitTests.Targets
{
    using NLog.Targets;
    using System;
    using Xunit;


    public class LineEndingModeTests : NLogTestBase
    {
        [Fact]
        public void LineEndingModeEqualityTest()
        {
            LineEndingMode modeDefault = LineEndingMode.Default;
            LineEndingMode modeNone = LineEndingMode.None;
            LineEndingMode modeLF = LineEndingMode.LF;
            LineEndingMode modeCRLF = LineEndingMode.CRLF;
            LineEndingMode modeNull = LineEndingMode.Null;

            Assert.True(LineEndingMode.Default == modeDefault);
            Assert.True(LineEndingMode.None == modeNone);
            Assert.True(LineEndingMode.LF == modeLF);
            Assert.True(LineEndingMode.Null == modeNull);
            Assert.False(LineEndingMode.Default == modeNone);
            Assert.False(LineEndingMode.None == modeLF);
            Assert.False(LineEndingMode.None == modeCRLF);
            Assert.False(LineEndingMode.None == modeNull);
            Assert.False(LineEndingMode.None == (object)new { });
            Assert.False(LineEndingMode.None == null);

            Assert.True(LineEndingMode.Default.Equals(modeDefault));
            Assert.True(LineEndingMode.None.Equals(modeNone));
            Assert.True(LineEndingMode.LF.Equals(modeLF));
            Assert.True(LineEndingMode.Null.Equals(modeNull));
            Assert.False(LineEndingMode.Default.Equals(modeNone));
            Assert.False(LineEndingMode.None.Equals(modeLF));
            Assert.False(LineEndingMode.None.Equals(modeCRLF));
            Assert.False(LineEndingMode.None.Equals(modeNull));
            Assert.False(LineEndingMode.None.Equals(new { }));
            Assert.False(LineEndingMode.None.Equals(null));

            // Handle running tests on different operating systems
            if (modeCRLF.NewLineCharacters == Environment.NewLine)
            {
                Assert.False(LineEndingMode.LF == modeDefault);
                Assert.True(LineEndingMode.CRLF == modeDefault);
            }
            else
            {
                Assert.True(LineEndingMode.LF == modeDefault);
                Assert.False(LineEndingMode.CRLF == modeDefault);
            }
        }

        [Fact]
        public void LineEndingModeInequalityTest()
        {
            LineEndingMode modeDefault = LineEndingMode.Default;
            LineEndingMode modeNone = LineEndingMode.None;
            LineEndingMode modeLF = LineEndingMode.LF;
            LineEndingMode modeCRLF = LineEndingMode.CRLF;
            LineEndingMode modeNull = LineEndingMode.Null;

            Assert.True(LineEndingMode.Default != modeNone);
            Assert.True(LineEndingMode.None != modeLF);
            Assert.True(LineEndingMode.None != modeCRLF);
            Assert.True(LineEndingMode.None != modeNull);
            Assert.False(LineEndingMode.Default != modeDefault);
            Assert.False(LineEndingMode.None != modeNone);
            Assert.False(LineEndingMode.LF != modeLF);
            Assert.False(LineEndingMode.CRLF != modeCRLF);
            Assert.False(LineEndingMode.Null != modeNull);

            Assert.True(null != LineEndingMode.LF);
            Assert.True(null != modeLF);
            Assert.True(LineEndingMode.LF != null);
            Assert.True(modeLF != null);
            Assert.True(null != LineEndingMode.CRLF);
            Assert.True(null != modeCRLF);
            Assert.True(LineEndingMode.CRLF != null);
            Assert.True(modeCRLF != null);
            Assert.True(null != LineEndingMode.Null);
            Assert.True(null != modeNull);
            Assert.True(LineEndingMode.Null != null);
            Assert.True(modeNull != null);

            // Handle running tests on different operating systems
            if (modeCRLF.NewLineCharacters == Environment.NewLine)
            {
                Assert.True(LineEndingMode.LF != modeDefault);
            }
            else
            {
                Assert.True(LineEndingMode.CRLF != modeDefault);
            }
        }

        [Fact]
        public void LineEndingModeNullComparison()
        {
            LineEndingMode mode1 = LineEndingMode.LF;
            Assert.False(mode1 == null);
            Assert.True(mode1 != null);
            Assert.False(null == mode1);
            Assert.True(null != mode1);

            LineEndingMode mode2 = null;
            Assert.True(mode2 == null);
            Assert.False(mode2 != null);
            Assert.True(null == mode2);
            Assert.False(null != mode2);
        }

        [Fact]
        public void LineEndingModeToStringTest()
        {
            Assert.Equal("None", LineEndingMode.None.ToString());
            Assert.Equal("Default", LineEndingMode.Default.ToString());
            Assert.Equal("CRLF", LineEndingMode.CRLF.ToString());
            Assert.Equal("CR", LineEndingMode.CR.ToString());
            Assert.Equal("LF", LineEndingMode.LF.ToString());
            Assert.Equal("Null", LineEndingMode.Null.ToString());
        }
    }
}
