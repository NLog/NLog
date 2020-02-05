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

namespace NLog.UnitTests.Internal
{
    using NLog.Internal;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Xunit;

#if DEBUG
    public class SimpleStringReaderTests : NLogTestBase
    {
        [Theory]
        [InlineData("", 0, "", char.MaxValue, "" )]
        [InlineData("abcdef", 0, "", 'a', "bcdef")]
        [InlineData("abcdef", 2, "ab", 'c', "def")]
        [InlineData("abcdef", 6, "abcdef", char.MaxValue, "")]
        [InlineData("abcdef", 7, "INVALID_CURRENT_STATE", char.MaxValue, "INVALID_CURRENT_STATE")]
        /// <summary>
        /// https://github.com/NLog/NLog/issues/3194
        /// </summary>
        public void DebugView_CurrentState(string input, int position, string expectedDone, char expectedCurrent, string expectedTodo)
        {
            var reader = new SimpleStringReader(input);
            reader.Position = position;
            Assert.Equal(
                SimpleStringReader.BuildCurrentState(expectedDone, expectedCurrent, expectedTodo), 
                reader.CurrentState);
        }

        [Fact]
        public void DebugView_CurrentState_NegativePosition()
        {
            Assert.Throws<IndexOutOfRangeException>(() => new SimpleStringReader("abcdef")
            {
                Position = -1,
            }.CurrentState);
        }
    }
#endif
}
