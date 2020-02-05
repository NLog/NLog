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
using NLog.MessageTemplates;
using Xunit;

namespace NLog.UnitTests.MessageTemplates
{
    public class MessageTemplateParameterTests
    {
        [Theory]
        [InlineData("0", 0)] //all single numbers for all code branches
        [InlineData("1", 1)]
        [InlineData("2", 2)]
        [InlineData("3", 3)]
        [InlineData("4", 4)]
        [InlineData("5", 5)]
        [InlineData("6", 6)]
        [InlineData("7", 7)]
        [InlineData("8", 8)]
        [InlineData("9", 9)] 
        [InlineData(" 1 ", null)] //no trim
        [InlineData("100", 100)] //parse test
        [InlineData(" 100 ", null)] //no trim in parse test
        [InlineData("a", null)] //no parse test
        [InlineData("a1", null)] //no partial parse test
        [InlineData("0a", null)] //condition branch 1
        [InlineData("1a", null)] //condition branch 1b
        [InlineData("9a", null)] //condition branch 2a
        [InlineData("8a", null)] //condition branch 2b
        public void PositionalIndexTest(string name, int? expected)
        {
            // Arrange
            var parameter = new MessageTemplateParameter(name, null, null);

            // Act
            var positionalIndex = parameter.PositionalIndex;

            // Assert
            Assert.Equal(expected, positionalIndex);
        }
    }
}
