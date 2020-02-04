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
using System.Collections.Generic;
using System.Linq;
using NLog.MessageTemplates;
using Xunit;

namespace NLog.UnitTests.MessageTemplates
{
    public class MessageTemplateParametersTests
    {
        [Theory]
        [InlineData("", 0)]
        [InlineData("Hello {0}", 1)]
        [InlineData("I like my {car}", 1)]
        [InlineData("I have {0} {1} {2} parameters", 3)]
        [InlineData("I have {a} {1} {2} parameters", 3)]
        [InlineData("I have {{text}} and {{0}}", 0)]
        [InlineData(" {3} {4} {9} {8} {5} {6} {7}", 7)]
        public void ParseParameters(string input, int expected)
        {
            // Arrange
            var parameters = CreateParameters(expected);

            // Act
            var messageTemplateParameters = new MessageTemplateParameters(input, parameters);

            // Assert
            Assert.Equal(expected, messageTemplateParameters.Count);
        }

        [Theory]
        [InlineData("", true)] // no really important when empty
        [InlineData("{0}", true)]
        [InlineData("{ 0}", false)] //no trimming
        [InlineData("{0} {1} {2}", true)]
        [InlineData("{a}", false)]
        [InlineData("{a} {0}", false)]
        [InlineData("{0} {a}", false)]
        [InlineData("{0} {a} {1}", false)]
        public void IsPositionalTest(string input, bool expected)
        {
            // Arrange
            var parameters = CreateParameters(10);

            // Act
            var messageTemplateParameters = new MessageTemplateParameters(input, parameters);

            // Assert
            Assert.Equal(expected, messageTemplateParameters.IsPositional);
        }

        [Theory]
        [InlineData("{0}", 0, "0", "0", 0, CaptureType.Normal)]
        [InlineData("{a}", 0, "0", "a", null, CaptureType.Normal)]
        public void IndexerTest(string input, int index, object expectedValue, string expectedName, int? expectedPositionalIndex, CaptureType expectedCaptureType)
        {
            // Arrange
            var parameters = CreateParameters(1);

            // Act
            var messageTemplateParameters = new MessageTemplateParameters(input, parameters);

            // Assert
            Assert.Equal(expectedValue, messageTemplateParameters[index].Value);
            Assert.Equal(expectedName, messageTemplateParameters[index].Name);
            Assert.Equal(expectedPositionalIndex, messageTemplateParameters[index].PositionalIndex);
            Assert.Equal(expectedCaptureType, messageTemplateParameters[index].CaptureType);
        }

        [Theory]
        [InlineData("{a}", "a")]
        [InlineData("{a} {b}", "a;b")]
        [InlineData("{b} {0} {a} ", "b;0;a")]
        public void EnumeratorTest(string input, string namesRaw)
        {
            // Arrange
            var parameters = CreateParameters(1);
            var names = namesRaw.Split(';');

            // Act
            var messageTemplateParameters = new MessageTemplateParameters(input, parameters);

            // Assert
            var resultNames = messageTemplateParameters.Select(t => t.Name).ToArray();
            Assert.Equal(names, resultNames);
        }

        [Theory]
        [InlineData("", 0, true)] //empty OK
        [InlineData("  ", 0, true)] //empty OK
        [InlineData("", 1, false)]
        [InlineData("{0}", 1, true)]
        [InlineData("{A}", 1, true)]
        //[InlineData("{A}", 0, false)]
        [InlineData("{A}", 2, false)]
        [InlineData("{ 0}", 1, true)]
        //[InlineData("{0} {1}", 0, false)]
        [InlineData("{0} {1}", 1, false)]
        [InlineData("{0} {1}", 2, true)]
        //[InlineData("{0} {A}", 0, false)]
        [InlineData("{0} {A}", 1, false)]
        [InlineData("{0} {A}", 2, true)]
        //[InlineData("{A} {1}", 0, false)]
        [InlineData("{A} {1}", 1, false)]
        [InlineData("{A} {1}", 2, true)]
        //[InlineData("{A} {B}", 0, false)]
        [InlineData("{A} {B}", 1, false)]
        [InlineData("{A} {B}", 2, true)]
        //[InlineData("{0} {0}", 0, false)]
        [InlineData("{0} {0}", 1, true)]
        [InlineData("{0} {0}", 2, false)]
        //[InlineData("{A} {A}", 0, false)]
        [InlineData("{A} {A}", 1, false)]
        [InlineData("{A} {A}", 2, true)] //overwrite
        public void IsValidTemplateTest(string input, int parameterCount, bool expected)
        {
            // Arrange
            var parameters = CreateParameters(parameterCount);

            // Act
            var messageTemplateParameters = new MessageTemplateParameters(input, parameters);

            // Assert
            Assert.Equal(expected, messageTemplateParameters.IsValidTemplate);
        }


        private static object[] CreateParameters(int count)
        {
            var parameters = new List<object>(count);
            for (int i = 0; i < count; i++)
            {
                parameters.Add(i.ToString());
            }

            return parameters.ToArray();
        }
    }
}
