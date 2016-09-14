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

#if !SILVERLIGHT

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

            AssertOutput(target, "The Cat Sat At The Bar.",
                new string[] { "The C", "at", " S", "at", " At The Bar." });
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

        /// <summary>
        /// With or wihout CompileRegex, CompileRegex is never null, even if not used when CompileRegex=false. (needed for backwardscomp)
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

#if !NET3_5 && !MONO

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


        private static void AssertOutput(Target target, string message, string[] expectedParts)
        {
            var consoleOutWriter = new PartsWriter();
            TextWriter oldConsoleOutWriter = Console.Out;
            Console.SetOut(consoleOutWriter);

            try
            {
                var exceptions = new List<Exception>();
                target.Initialize(null);
                target.WriteAsyncLogEvent(new LogEventInfo(LogLevel.Info, "Logger", message).WithContinuation(exceptions.Add));
                target.Close();

                Assert.Equal(1, exceptions.Count);
                Assert.True(exceptions.TrueForAll(e => e == null));
            }
            finally
            {
                Console.SetOut(oldConsoleOutWriter);
            }

            var expected = Enumerable.Repeat("Logger " + expectedParts[0], 1).Concat(expectedParts.Skip(1));
            Assert.Equal(expected, consoleOutWriter.Values);
        }


        private class PartsWriter : StringWriter
        {
            public PartsWriter()
            {
                Values = new List<string>();
            }

            public List<string> Values { get; private set; }

            public override void Write(string value)
            {
                Values.Add(value);
            }
        }
    }
}

#endif
