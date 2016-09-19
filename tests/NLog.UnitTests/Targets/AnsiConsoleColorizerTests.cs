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
    using System.IO;
    using NLog.Targets;
    using Xunit;

    public class AnsiConsoleColorizerTests
    {
        [Fact]
        public void EmptyMessageShouldNotFailTest()
        {
            var sut = new AnsiConsoleColorizer(string.Empty);
            var colorizedMessage = GetColorizedMessage(sut);

            Assert.Equal(string.Empty, colorizedMessage);
        }

        [Fact]
        public void NullMessageShouldNotFailTest()
        {
            var sut = new AnsiConsoleColorizer(null);
            var colorizedMessage = GetColorizedMessage(sut);

            Assert.Equal(string.Empty, colorizedMessage);
        }

        [Theory]
        [InlineData("This is a message", ConsoleOutputColor.NoChange, ConsoleOutputColor.NoChange, "This is a message")]
        [InlineData("This is a message", ConsoleOutputColor.DarkRed, ConsoleOutputColor.NoChange, "\x1B[31mThis is a message\x1B[39m\x1B[22m")]
        [InlineData("This is a message", ConsoleOutputColor.Red, ConsoleOutputColor.NoChange, "\x1B[1m\x1B[31mThis is a message\x1B[39m\x1B[22m")]
        [InlineData("This is a message", ConsoleOutputColor.NoChange, ConsoleOutputColor.DarkRed, "\x1B[41mThis is a message\x1B[49m")]
        [InlineData("This is a message", ConsoleOutputColor.NoChange, ConsoleOutputColor.Red, "\x1B[101mThis is a message\x1B[49m")]
        [InlineData("This is a message", ConsoleOutputColor.Blue, ConsoleOutputColor.DarkBlue, "\x1B[44m\x1B[1m\x1B[34mThis is a message\x1B[0m")]
        [InlineData("This is a message", ConsoleOutputColor.DarkBlue, ConsoleOutputColor.Blue, "\x1B[104m\x1B[34mThis is a message\x1B[0m")]
        public void RowHighlightingTextTest(string message, ConsoleOutputColor foregroundColor, ConsoleOutputColor backgroundColor, string expectedMessage)
        {
            var rule = new ConsoleRowHighlightingRule { ForegroundColor = foregroundColor, BackgroundColor = backgroundColor };
            var sut = new AnsiConsoleColorizer(message);
            sut.RowHighlightingRule = rule;
            
            var colorizedMessage = GetColorizedMessage(sut);

            Assert.Equal(expectedMessage, colorizedMessage);
        }

        [Theory]
        [InlineData("This is a message", ConsoleOutputColor.Gray, ConsoleOutputColor.NoChange, "\x1B[37mThis is a message\x1B[39m\x1B[22m")]
        [InlineData("This is a message", ConsoleOutputColor.NoChange, ConsoleOutputColor.Gray, "\x1B[47mThis is a message\x1B[49m")]
        public void GrayHasDarkWhiteAnsiCodeTest(string message, ConsoleOutputColor foregroundColor, ConsoleOutputColor backgroundColor, string expectedMessage)
        {
            var rule = new ConsoleRowHighlightingRule { ForegroundColor = foregroundColor, BackgroundColor = backgroundColor };
            var sut = new AnsiConsoleColorizer(message);
            sut.RowHighlightingRule = rule;
            
            var colorizedMessage = GetColorizedMessage(sut);
            
            Assert.Equal(expectedMessage, colorizedMessage);
        }
        
        [Theory]
        [InlineData("This is a message", ConsoleOutputColor.DarkGray, ConsoleOutputColor.NoChange, "\x1B[1m\x1B[30mThis is a message\x1B[39m\x1B[22m")]
        [InlineData("This is a message", ConsoleOutputColor.NoChange, ConsoleOutputColor.DarkGray, "\x1B[100mThis is a message\x1B[49m")]
        public void DarkGrayHasBrightBlackAnsiCodeTest(string message, ConsoleOutputColor foregroundColor, ConsoleOutputColor backgroundColor, string expectedMessage)
        {
            var rule = new ConsoleRowHighlightingRule { ForegroundColor = foregroundColor, BackgroundColor = backgroundColor };
            var sut = new AnsiConsoleColorizer(message);
            sut.RowHighlightingRule = rule;
            
            var colorizedMessage = GetColorizedMessage(sut);
            
            Assert.Equal(expectedMessage, colorizedMessage);
        }
        
        [Theory]
        [InlineData("This is my word", ConsoleOutputColor.NoChange, ConsoleOutputColor.NoChange, ConsoleOutputColor.NoChange, ConsoleOutputColor.NoChange, "This is my word")]
        [InlineData("This is my word", ConsoleOutputColor.DarkRed, ConsoleOutputColor.NoChange, ConsoleOutputColor.NoChange, ConsoleOutputColor.NoChange, "\x1B[31mThis is my word\x1B[39m\x1B[22m")]
        [InlineData("This is my word", ConsoleOutputColor.NoChange, ConsoleOutputColor.DarkRed, ConsoleOutputColor.NoChange, ConsoleOutputColor.NoChange, "\x1B[41mThis is my word\x1B[49m")]
        [InlineData("This is my word", ConsoleOutputColor.DarkBlue, ConsoleOutputColor.DarkRed, ConsoleOutputColor.NoChange, ConsoleOutputColor.NoChange, "\x1B[41m\x1B[34mThis is my word\x1B[0m")]
        [InlineData("This is my word", ConsoleOutputColor.DarkRed, ConsoleOutputColor.NoChange, ConsoleOutputColor.DarkBlue, ConsoleOutputColor.NoChange, "\x1B[31mThis is my word\x1B[34m")]
        [InlineData("This is my word", ConsoleOutputColor.NoChange, ConsoleOutputColor.DarkRed, ConsoleOutputColor.NoChange, ConsoleOutputColor.DarkBlue, "\x1B[41mThis is my word\x1B[44m")]
        [InlineData("This is my word", ConsoleOutputColor.DarkBlue, ConsoleOutputColor.DarkRed, ConsoleOutputColor.DarkMagenta, ConsoleOutputColor.DarkGreen, "\x1B[41m\x1B[34mThis is my word\x1B[35m\x1B[42m")]
        public void FormatWordTest(string word, ConsoleOutputColor matchForegroundColor, ConsoleOutputColor matchBackgroundColor, 
                                   ConsoleOutputColor nextForegroundColor, ConsoleOutputColor nextBackgroundColor, string expectedMessage)
        {
            var colorizedMessage = AnsiConsoleColorizer.ColorizeWord(word, matchForegroundColor, matchBackgroundColor, nextForegroundColor, nextBackgroundColor);
            
            Assert.Equal(expectedMessage, colorizedMessage);
        }
        
        [Fact]
        public void InvalidWordRuleShouldResultInNoHighlightingTest()
        {
            var message = "The big warning message";
            var rowRule = new ConsoleRowHighlightingRule{ ForegroundColor = ConsoleOutputColor.NoChange, BackgroundColor = ConsoleOutputColor.NoChange };
            var wordRules = new []{ new ConsoleWordHighlightingRule 
                                    {
                                        ForegroundColor = ConsoleOutputColor.DarkRed,
                                        BackgroundColor = ConsoleOutputColor.NoChange
                                    }};
            var sut = new AnsiConsoleColorizer(message);
            sut.RowHighlightingRule = rowRule;
            sut.WordHighlightingRules = wordRules;
            
            var colorizedMessage = GetColorizedMessage(sut);
            
            var expectedMessage = "The big warning message";
            Assert.Equal(expectedMessage, colorizedMessage);
        }
        
        [Fact]
        public void OverlappingWordRulesShouldBeFormattedCorrectlyTest()
        {
            var message = "The big warning message";
            var rowRule = new ConsoleRowHighlightingRule{ ForegroundColor = ConsoleOutputColor.NoChange, BackgroundColor = ConsoleOutputColor.NoChange };
            var wordRules = new []{ new ConsoleWordHighlightingRule 
                                    {
                                        Text = "big warning",
                                        ForegroundColor = ConsoleOutputColor.DarkRed,
                                        BackgroundColor = ConsoleOutputColor.NoChange
                                    },
                                    new ConsoleWordHighlightingRule
                                    {
                                        Text = "warn",
                                        ForegroundColor = ConsoleOutputColor.DarkMagenta,
                                        BackgroundColor = ConsoleOutputColor.NoChange
                                    },
                                    new ConsoleWordHighlightingRule
                                    {
                                        Text = "a",
                                        ForegroundColor = ConsoleOutputColor.DarkGreen,
                                        BackgroundColor = ConsoleOutputColor.NoChange
                                    }};
            var sut = new AnsiConsoleColorizer(message);
            sut.RowHighlightingRule = rowRule;
            sut.WordHighlightingRules = wordRules;
            
            var colorizedMessage = GetColorizedMessage(sut);
            
            var expectedMessage = "The \x1B[31mbig \x1B[35mw\x1B[32ma\x1B[35mrn\x1B[31ming\x1B[39m\x1B[22m mess\x1B[32ma\x1B[39m\x1B[22mge";
            Assert.Equal(expectedMessage, colorizedMessage);
        }
        
        [Fact]
        public void WordRuleWithMultipleResutsShouldBeFormattedCorrectlyTest()
        {
            var message = "The big big big big warning message";
            var rowRule = new ConsoleRowHighlightingRule{ ForegroundColor = ConsoleOutputColor.NoChange, BackgroundColor = ConsoleOutputColor.NoChange };
            var wordRules = new []{ new ConsoleWordHighlightingRule 
                                    {
                                        Text = "big big",
                                        ForegroundColor = ConsoleOutputColor.DarkRed,
                                        BackgroundColor = ConsoleOutputColor.NoChange
                                    }};
            var sut = new AnsiConsoleColorizer(message);
            sut.RowHighlightingRule = rowRule;
            sut.WordHighlightingRules = wordRules;
            
            var colorizedMessage = GetColorizedMessage(sut);
            
            var expectedMessage = "The \x1B[31mbig big\x1B[39m\x1B[22m \x1B[31mbig big\x1B[39m\x1B[22m warning message";
            Assert.Equal(expectedMessage, colorizedMessage);
        }

        [Fact]
        public void RowAndWordHighlightingTextTest()
        {
            var message = "The big warning message";
            var rowRule = new ConsoleRowHighlightingRule { ForegroundColor = ConsoleOutputColor.DarkRed, BackgroundColor = ConsoleOutputColor.Gray };
            var wordRules = new []{ new ConsoleWordHighlightingRule 
                                    {
                                        Text = "warning",
                                        ForegroundColor = ConsoleOutputColor.DarkBlue,
                                        BackgroundColor = ConsoleOutputColor.DarkMagenta
                                    }};
            var sut = new AnsiConsoleColorizer(message);
            sut.RowHighlightingRule = rowRule;
            sut.WordHighlightingRules = wordRules;
            
            var colorizedMessage = GetColorizedMessage(sut);
            
            var expectedMessage = "\x1B[47m\x1B[31mThe big \x1B[45m\x1B[34mwarning\x1B[31m\x1B[47m message\x1B[0m";
            Assert.Equal(expectedMessage, colorizedMessage);
        }

        private static string GetColorizedMessage(AnsiConsoleColorizer colorizer)
        {
            var writer = new StringWriter();
            colorizer.ColorizeMessage(writer);
            return writer.ToString().Trim();
        }
    }
}

#endif
