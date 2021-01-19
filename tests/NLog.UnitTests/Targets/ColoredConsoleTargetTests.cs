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
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using NLog.Targets;
    using Xunit;
    using Xunit.Extensions;

    public class ColoredConsoleTargetTests : NLogTestBase
    {
        [Fact]
        public void RowHighLightNewLineTest()
        {
            var target = new ColoredConsoleTarget { Layout = "${logger} ${message}" };
            AssertOutput(target, "Before\a\nAfter\a",
    new string[] { "Before", "After" });
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void WordHighlightingTextTest(bool compileRegex)
        {
            var target = new ColoredConsoleTarget { Layout = "${logger} ${message}" };
            target.WordHighlightingRules.Add(
                new ConsoleWordHighlightingRule
                {
                    ForegroundColor = ConsoleOutputColor.Red,
                    Text = "at",
                    CompileRegex = compileRegex

                });

            AssertOutput(target, "The Cat Sat At The Bar\t\n.\a",
                new string[] { "The C", "at", " S", "at", " At The Bar", "." });
        }

        [Fact]
        public void WordHighlightingTextCondition()
        {
            var highlightLogger = "SpecialLogger";

            var target = new ColoredConsoleTarget { Layout = "${logger}${message}" };
            target.WordHighlightingRules.Add(
                new ConsoleWordHighlightingRule
                {
                    ForegroundColor = ConsoleOutputColor.Red,
                    Text = "at",
                    Condition = string.Concat("logger=='", highlightLogger, "'"),
                });

            AssertOutput(target, "The Cat Sat At The Bar.",
                new string[] { "The C", "at", " S", "at", " At The Bar." }, loggerName: highlightLogger);

            AssertOutput(target, "The Cat Sat At The Bar",
                new string[] { "The Cat Sat At The Bar" }, loggerName: "DefaultLogger");
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void WordHighlightingTextIgnoreCase(bool compileRegex)
        {
            var target = new ColoredConsoleTarget { Layout = "${logger} ${message}" };
            target.WordHighlightingRules.Add(
                new ConsoleWordHighlightingRule
                {
                    ForegroundColor = ConsoleOutputColor.Red,
                    Text = "at",
                    IgnoreCase = true,
                    CompileRegex = compileRegex
                });

            AssertOutput(target, "The Cat Sat At The Bar.",
                new string[] { "The C", "at", " S", "at", " ", "At", " The Bar." });
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void WordHighlightingTextWholeWords(bool compileRegex)
        {
            var target = new ColoredConsoleTarget { Layout = "${logger} ${message}" };
            target.WordHighlightingRules.Add(
                new ConsoleWordHighlightingRule
                {
                    ForegroundColor = ConsoleOutputColor.Red,
                    Text = "at",
                    WholeWords = true,
                    CompileRegex = compileRegex
                });

            AssertOutput(target, "The cat sat at the bar.",
                new string[] { "The cat sat ", "at", " the bar." });
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void WordHighlightingRegex(bool compileRegex)
        {
            var target = new ColoredConsoleTarget { Layout = "${logger} ${message}" };
            target.WordHighlightingRules.Add(
                new ConsoleWordHighlightingRule
                {
                    ForegroundColor = ConsoleOutputColor.Red,
                    Regex = "\\wat",
                    CompileRegex = compileRegex
                });

            AssertOutput(target, "The cat sat at the bar.",
                new string[] { "The ", "cat", " ", "sat", " at the bar." });
        }

        [Fact]
        public void ColoredConsoleAnsi_OverlappingWordHighlight_VerificationTest()
        {
            var target = new ColoredConsoleTarget { Layout = "${logger} ${message}", EnableAnsiOutput = true };
            target.UseDefaultRowHighlightingRules = false;
            target.WordHighlightingRules.Add(new ConsoleWordHighlightingRule
            {
                Text = "big warning",
                ForegroundColor = ConsoleOutputColor.DarkRed,
                BackgroundColor = ConsoleOutputColor.NoChange
            });
            target.WordHighlightingRules.Add(new ConsoleWordHighlightingRule
            {
                Text = "warn",
                ForegroundColor = ConsoleOutputColor.DarkMagenta,
                BackgroundColor = ConsoleOutputColor.NoChange
            });
            target.WordHighlightingRules.Add(new ConsoleWordHighlightingRule
            {
                Text = "a",
                ForegroundColor = ConsoleOutputColor.DarkGreen,
                BackgroundColor = ConsoleOutputColor.NoChange
            });

            AssertOutput(target, "The big warning message",
                    new string[] { "The \x1B[31mbig \x1B[35mw\x1B[32ma\x1B[35mrn\x1B[31ming\x1B[0m mess\x1B[32ma\x1B[0mge\x1B[0m" });
        }

        [Fact]
        public void ColoredConsoleAnsi_RepeatedWordHighlight_VerificationTest()
        {
            var target = new ColoredConsoleTarget { Layout = "${logger} ${message}", EnableAnsiOutput = true };
            target.UseDefaultRowHighlightingRules = false;
            target.WordHighlightingRules.Add(new ConsoleWordHighlightingRule
            {
                Text = "big big",
                ForegroundColor = ConsoleOutputColor.DarkRed,
                BackgroundColor = ConsoleOutputColor.NoChange
            });

            AssertOutput(target, "The big big big big warning message",
                    new string[] { "The \x1B[31mbig big\x1B[0m \x1B[31mbig big\x1B[0m warning message\x1B[0m" });
        }

        [Theory]
        [InlineData("The big warning message", "\x1B[42mThe big warning message\x1B[0m")]
        [InlineData("The big\r\nwarning message", "\x1B[42mThe big\x1B[0m\r\n\x1B[42mwarning message\x1B[0m")]
        public void ColoredConsoleAnsi_RowColor_VerificationTest(string inputText, string expectedResult)
        {
            var target = new ColoredConsoleTarget { Layout = "${message}", EnableAnsiOutput = true };
            target.UseDefaultRowHighlightingRules = false;
            target.RowHighlightingRules.Add(new ConsoleRowHighlightingRule() { BackgroundColor = ConsoleOutputColor.DarkGreen });

            AssertOutput(target, inputText,
                    new string[] { expectedResult },
                    string.Empty);
        }

        [Fact]
        public void ColoredConsoleAnsi_RowColorWithWordHighlight_VerificationTest()
        {
            var target = new ColoredConsoleTarget { Layout = "${message}", EnableAnsiOutput = true };
            target.UseDefaultRowHighlightingRules = false;
            target.RowHighlightingRules.Add(new ConsoleRowHighlightingRule() { BackgroundColor = ConsoleOutputColor.Green });
            target.WordHighlightingRules.Add(new ConsoleWordHighlightingRule
            {
                Text = "big big",
                ForegroundColor = ConsoleOutputColor.DarkRed,
                BackgroundColor = ConsoleOutputColor.NoChange
            });

            AssertOutput(target, "The big big big big warning message",
                    new string[] { "\x1B[102mThe \x1B[31mbig big\x1B[0m\x1B[102m \x1B[31mbig big\x1B[0m\x1B[102m warning message\x1B[0m" },
                    string.Empty);
        }

        /// <summary>
        /// With or without CompileRegex, CompileRegex is never null, even if not used when CompileRegex=false. (needed for backwards-compatibility)
        /// </summary>
        /// <param name="compileRegex"></param>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void CompiledRegexPropertyNotNull(bool compileRegex)
        {
            var rule = new ConsoleWordHighlightingRule
            {
                ForegroundColor = ConsoleOutputColor.Red,
                Regex = "\\wat",
                CompileRegex = compileRegex
            };

            Assert.NotNull(rule.CompiledRegex);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void DonRemoveIfRegexIsEmpty(bool compileRegex)
        {
            var target = new ColoredConsoleTarget { Layout = "${logger} ${message}" };
            target.WordHighlightingRules.Add(
                new ConsoleWordHighlightingRule
                {
                    ForegroundColor = ConsoleOutputColor.Red,
                    Text = null,
                    IgnoreCase = true,
                    CompileRegex = compileRegex
                });

            AssertOutput(target, "The Cat Sat At The Bar.",
                new string[] { "The Cat Sat At The Bar." });
        }

        [Fact]
        public void ColortedConsoleAutoFlushOnWrite()
        {
            var target = new ColoredConsoleTarget { Layout = "${logger} ${message}", AutoFlush = true };
            AssertOutput(target, "The Cat Sat At The Bar.",
                new string[] { "The Cat Sat At The Bar." });

        }

#if !MONO
        [Fact]
        public void ColoredConsoleRaceCondtionIgnoreTest()
        {
            var configXml = @"
            <nlog throwExceptions='true'>
                <targets>
                  <target name='console' type='coloredConsole' layout='${message}' />
                  <target name='console2' type='coloredConsole' layout='${message}' />
                  <target name='console3' type='coloredConsole' layout='${message}' />
                </targets>
                <rules>
                  <logger name='*' minlevel='Trace' writeTo='console,console2,console3' />
                </rules>
            </nlog>";

            ConsoleTargetTests.ConsoleRaceCondtionIgnoreInnerTest(configXml);
        }
#endif

#if !NET35 && !NET40
        [Fact]
        public void ColoredConsoleDetectOutputRedirectedTest()
        {
            var target = new ColoredConsoleTarget { Layout = "${logger} ${message}", DetectOutputRedirected = true };
            AssertOutput(target, "The Cat Sat At The Bar.",
                new string[] { "The Cat Sat At The Bar." });
        }
#endif

        private static void AssertOutput(Target target, string message, string[] expectedParts, string loggerName = "Logger ")
        {
            var consoleOutWriter = new PartsWriter();
            TextWriter oldConsoleOutWriter = Console.Out;
            Console.SetOut(consoleOutWriter);

            try
            {
                var exceptions = new List<Exception>();
                target.Initialize(null);
                target.WriteAsyncLogEvent(new LogEventInfo(LogLevel.Info, loggerName.Trim(), message).WithContinuation(exceptions.Add));
                target.Close();

                Assert.Single(exceptions);
                Assert.True(exceptions.TrueForAll(e => e == null));
            }
            finally
            {
                Console.SetOut(oldConsoleOutWriter);
            }

            var expected = Enumerable.Repeat(loggerName + expectedParts[0], 1).Concat(expectedParts.Skip(1));
            Assert.Equal(expected, consoleOutWriter.Values);
            Assert.True(consoleOutWriter.SingleWriteLine);
            Assert.True(consoleOutWriter.SingleFlush);
        }


        private class PartsWriter : StringWriter
        {
            public PartsWriter()
            {
                Values = new List<string>();
            }

            public List<string> Values { get; private set; }
            public bool SingleWriteLine { get; private set; }
            public bool SingleFlush { get; private set; }

            public override void Write(string value)
            {
                Values.Add(value);
            }

            public override void Flush()
            {
                if (SingleFlush)
                {
                    throw new InvalidOperationException("Single Flush only");
                }
                SingleFlush = true;
                base.Flush();
            }

            public override void WriteLine(string value)
            {
                if (SingleWriteLine)
                {
                    Values.Clear();
                    throw new InvalidOperationException("Single WriteLine only");
                }
                SingleWriteLine = true;
                if (!string.IsNullOrEmpty(value))
                    Values.Add(value);
            }

            public override void WriteLine()
            {
                if (SingleWriteLine)
                {
                    Values.Clear();
                    throw new InvalidOperationException("Single WriteLine only");
                }
                SingleWriteLine = true;
            }
        }
    }
}