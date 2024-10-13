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

namespace NLog.RegEx.Tests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using NLog.Targets;
    using Xunit;

    public class ColoredConsoleTargetTests
    {
        public ColoredConsoleTargetTests()
        {
            LogManager.ThrowExceptions = true;
            LogManager.Setup().SetupExtensions(ext => ext.RegisterAssembly(typeof(Conditions.RegexConditionMethods).Assembly));
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void WordHighlightingTextIgnoreCase(bool compileRegex)
        {
            var target = new ColoredConsoleTarget { Layout = "${logger} ${message}" };
            target.WordHighlightingRules.Add(
                new ConsoleWordHighlightingRuleRegex
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
                new ConsoleWordHighlightingRuleRegex
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
                new ConsoleWordHighlightingRuleRegex
                {
                    ForegroundColor = ConsoleOutputColor.Red,
                    Regex = "\\wat",
                    CompileRegex = compileRegex
                });

            AssertOutput(target, "The cat sat at the bar.",
                new string[] { "The ", "cat", " ", "sat", " at the bar." });
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void DonRemoveIfRegexIsEmpty(bool compileRegex)
        {
            var target = new ColoredConsoleTarget { Layout = "${logger} ${message}" };
            target.WordHighlightingRules.Add(
                new ConsoleWordHighlightingRuleRegex
                {
                    ForegroundColor = ConsoleOutputColor.Red,
                    Text = null,
                    IgnoreCase = true,
                    CompileRegex = compileRegex
                });

            AssertOutput(target, "The Cat Sat At The Bar.",
                new string[] { "The Cat Sat At The Bar." });
        }

        private static void AssertOutput(Target target, string message, string[] expectedParts, string loggerName = "Logger ")
        {
            var consoleOutWriter = new PartsWriter();
            TextWriter oldConsoleOutWriter = Console.Out;
            Console.SetOut(consoleOutWriter);

            try
            {
                var exceptions = new List<Exception>();
                var logFactory = new LogFactory().Setup().LoadConfiguration(cfg => cfg.ForLogger().WriteTo(target)).LogFactory;
                target.WriteAsyncLogEvent(new LogEventInfo(LogLevel.Info, loggerName.Trim(), message).WithContinuation(exceptions.Add));
                logFactory.Shutdown();

                Assert.Single(exceptions);
                Assert.True(exceptions.TrueForAll(e => e is null));
            }
            finally
            {
                Console.SetOut(oldConsoleOutWriter);
            }

            var expected = Enumerable.Repeat(loggerName + expectedParts[0], 1).Concat(expectedParts.Skip(1));
            Assert.Equal(expected, consoleOutWriter.Values);
            Assert.True(consoleOutWriter.SingleWriteLine);
        }

        private sealed class PartsWriter : StringWriter
        {
            public PartsWriter()
            {
                Values = new List<string>();
            }

            public List<string> Values { get; private set; }
            public bool SingleWriteLine { get; private set; }

            public override void Write(string value)
            {
                Values.Add(value);
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
