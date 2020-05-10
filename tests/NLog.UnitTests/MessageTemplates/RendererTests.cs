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

namespace NLog.UnitTests.MessageTemplates
{
    using System;
    using System.Globalization;
    using NLog.MessageTemplates;
    using Xunit;
    using Xunit.Extensions;

    public class RendererTests
    {
        [Theory]
        [InlineData("{0}", new object[] { "a" }, "a")]
        [InlineData(" {0}", new object[] { "a" }, " a")]
        [InlineData(" {0} ", new object[] { "a" }, " a ")]
        [InlineData(" {0} {1} ", new object[] { "a", "b" }, " a b ")]
        [InlineData(" {1} {0} ", new object[] { "a", "b" }, " b a ")]
        [InlineData(" {1} {0} {0}", new object[] { "a", "b" }, " b a a")]
        [InlineData(" message {1} {0} {0}", new object[] { "a", "b" }, " message b a a")]
        [InlineData(" message {1} {0} {0}", new object[] { 'a', 'b' }, " message b a a")]
        [InlineData("char {one}", new object[] { 'X' }, "char \"X\"")]
        [InlineData("char {one:l}", new object[] { 'X' }, "char X")]
        [InlineData(" message {{{1}}} {0} {0}", new object[] { "a", "b" }, " message {b} a a")]
        [InlineData(" message {{{one}}} {two} {three}", new object[] { "a", "b", "c" }, " message {\"a\"} \"b\" \"c\"")]
        [InlineData(" message {{{1} {0} {0}}}", new object[] { "a", "b" }, " message {b a a}")]
        [InlineData(" completed in {time} sec", new object[] { 10 }, " completed in 10 sec")]
        [InlineData(" completed task {name} in {time} sec", new object[] { "test", 10 }, " completed task \"test\" in 10 sec")]
        [InlineData(" completed task {name:l} in {time} sec", new object[] { "test", 10 }, " completed task test in 10 sec")]
        [InlineData(" completed task {0} in {1} sec", new object[] { "test", 10 }, " completed task test in 10 sec")]
        [InlineData(" completed task {0} in {1:000} sec", new object[] { "test", 10 }, " completed task test in 010 sec")]
        [InlineData(" completed task {name} in {time:000} sec", new object[] { "test", 10 }, " completed task \"test\" in 010 sec")]
        [InlineData(" completed tasks {tasks} in {time:000} sec", new object[] { new [] { "parsing", "testing", "fixing"}, 10 }, " completed tasks \"parsing\", \"testing\", \"fixing\" in 010 sec")]
        [InlineData(" completed tasks {tasks:l} in {time:000} sec", new object[] { new [] { "parsing", "testing", "fixing"}, 10 }, " completed tasks parsing, testing, fixing in 010 sec")]
#if !MONO
        [InlineData(" completed tasks {$tasks} in {time:000} sec", new object[] { new [] { "parsing", "testing", "fixing"}, 10 }, " completed tasks \"System.String[]\" in 010 sec")]
        [InlineData(" completed tasks {0} in {1:000} sec", new object[] { new [] { "parsing", "testing", "fixing"}, 10 }, " completed tasks System.String[] in 010 sec")]
#endif
        [InlineData("{{{0:d}}}", new object[] { 3 }, "{d}")] //format is here "d}" ... because escape from left-to-right 
        [InlineData("{{{0:d} }}", new object[] { 3 }, "{3 }")]
        [InlineData("{{{0:dd}}}", new object[] { 3 }, "{dd}")]
        [InlineData("{{{0:0{{}", new object[] { 3 }, "{3{")] //format is here "0{"
        [InlineData("hello {0}", new object[] { null }, "hello NULL")]
        [InlineData("if its {yes}, it should not be {no}", new object[] { true, false }, "if its true, it should not be false")]
        [InlineData("Always use the correct {enum}", new object[] { NLog.Config.ExceptionRenderingFormat.Method }, "Always use the correct Method")]
        [InlineData("Always use the correct {enum:D}", new object[] { NLog.Config.ExceptionRenderingFormat.Method }, "Always use the correct 4")]
        [InlineData("hello {0,-10}", new object[] { null }, "hello NULL      ")]
        [InlineData("hello {0,10}", new object[] { null }, "hello       NULL")]
        [InlineData("Status [0x{status:X8}]", new object[] { 16 }, "Status [0x00000010]")]
        public void RenderTest(string input, object[] args, string expected)
        {
            var culture = CultureInfo.InvariantCulture;

            RenderAndTest(input, culture, args, expected);
        }

        [Theory]
        [InlineData("test {0}", "nl", new object[] { 12.3 }, "test 12,3")]
        [InlineData("test {0}", "en-gb", new object[] { 12.3 }, "test 12.3")]
        public void RenderCulture(string input, string language, object[] args, string expected)
        {
            var culture = new CultureInfo(language);

            RenderAndTest(input, culture, args, expected);
        }

        [Theory]
        [InlineData("test {0:u}", "1970-01-01", "test 1970-01-01 00:00:00Z")]
        [InlineData("test {0:MM/dd/yy}", "1970-01-01", "test 01/01/70")]
        public void RenderDateTime(string input, string arg, string expected)
        {
            var culture = CultureInfo.InvariantCulture;

            DateTime dt = DateTime.Parse(arg, culture, DateTimeStyles.AdjustToUniversal);
            RenderAndTest(input, culture, new object[] { dt }, expected);
        }

        [Theory]
        [InlineData("test {0:c}", "1:2:3:4.5", "test 1.02:03:04.5000000")]
        [InlineData("test {0:hh\\:mm\\:ss\\.ff}", "1:2:3:4.5", "test 02:03:04.50")]
        public void RenderTimeSpan(string input, string arg, string expected)
        {
            var culture = CultureInfo.InvariantCulture;

            TimeSpan ts = TimeSpan.Parse(arg, culture);
            RenderAndTest(input, culture, new object[] { ts }, expected);
        }

        [Theory]
        [InlineData("test {0:u}", "1 Jan 1970 01:02:03Z", "test 1970-01-01 01:02:03Z")]
        [InlineData("test {0:s}", "1 Jan 1970 01:02:03Z", "test 1970-01-01T01:02:03")]
        public void RenderDateTimeOffset(string input, string arg, string expected)
        {
            var culture = CultureInfo.InvariantCulture;

            DateTimeOffset dto = DateTimeOffset.Parse(arg, culture, DateTimeStyles.AdjustToUniversal);
            RenderAndTest(input, culture, new object[] { dto }, expected);
        }

        private static void RenderAndTest(string input, CultureInfo culture, object[] args, string expected)
        {
            var logEventInfoAlways = new LogEventInfo(LogLevel.Info, "Logger", culture, input, args);
            logEventInfoAlways.SetMessageFormatter(new NLog.Internal.LogMessageTemplateFormatter(LogManager.LogFactory.ServiceRepository, true, false).MessageFormatter, null);
            var templateAlways = logEventInfoAlways.MessageTemplateParameters;
            Assert.Equal(expected, logEventInfoAlways.FormattedMessage);
        }
    }
}