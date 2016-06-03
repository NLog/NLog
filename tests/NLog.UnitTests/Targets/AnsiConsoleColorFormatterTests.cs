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

#if !SILVERLIGHT && !UWP10 || NETSTANDARD1_3

namespace NLog.UnitTests.Targets
{
    using NLog.Targets;
    using Xunit;
    
    public class AnsiConsoleColorFormatterTests
    {
        [Theory]
        [InlineData("This is a warning message", ConsoleOutputColor.NoChange, ConsoleOutputColor.NoChange, "This is a warning message")]
        [InlineData("This is a warning message", ConsoleOutputColor.DarkRed, ConsoleOutputColor.NoChange, "\x1B[31mThis is a warning message\x1B[39m")]
        [InlineData("This is a warning message", ConsoleOutputColor.Red, ConsoleOutputColor.NoChange, "\x1B[91mThis is a warning message\x1B[39m")]
        [InlineData("This is a warning message", ConsoleOutputColor.NoChange, ConsoleOutputColor.DarkRed, "\x1B[41mThis is a warning message\x1B[0m")]
        [InlineData("This is a warning message", ConsoleOutputColor.NoChange, ConsoleOutputColor.Red, "\x1B[101mThis is a warning message\x1B[0m")]
        [InlineData("This is a warning message", ConsoleOutputColor.Blue, ConsoleOutputColor.DarkBlue, "\x1B[44m\x1B[94mThis is a warning message\x1B[39m\x1B[0m")]
        [InlineData("This is a warning message", ConsoleOutputColor.DarkBlue, ConsoleOutputColor.Blue, "\x1B[104m\x1B[34mThis is a warning message\x1B[39m\x1B[0m")]
        public void RowHighlightingTextTest(string message, ConsoleOutputColor foregroundColor, ConsoleOutputColor backgroundColor, string expectedMessage)
        {
            var rule = new ConsoleRowHighlightingRule { ForegroundColor = foregroundColor, BackgroundColor = backgroundColor };
            
            var formattedMessage = AnsiConsoleColorFormatter.FormatRow(message, rule);
            
            Assert.Equal(expectedMessage, formattedMessage);
        }
    }
}

#endif