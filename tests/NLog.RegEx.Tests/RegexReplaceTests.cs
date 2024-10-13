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
    using NLog;
    using NLog.Config;
    using NLog.LayoutRenderers.Wrappers;
    using NLog.Layouts;
    using NLog.Targets;
    using Xunit;

    public class RegexReplaceTests
    {
        public RegexReplaceTests()
        {
            LogManager.ThrowExceptions = true;
            LogManager.Setup().SetupExtensions(ext => ext.RegisterAssembly(typeof(Conditions.RegexConditionMethods).Assembly));
        }

        [Fact]
        public void ReplaceTestWithoutRegEx()
        {
            // Arrange
            SimpleLayout layout = @"${regex-replace:inner=${message}:searchFor=foo:replaceWith=BAR}";

            // Act
            var result = layout.Render(new LogEventInfo(LogLevel.Info, "Test", " foo bar bar foo bar FOO"));

            // Assert
            Assert.Equal(" BAR bar bar BAR bar FOO", result);
        }

        [Fact]
        public void ReplaceTestIgnoreCaseWithoutRegEx()
        {
            // Arrange
            SimpleLayout layout = @"${regex-replace:inner=${message}:searchFor=foo:replaceWith=BAR:ignorecase=true}";

            // Act
            var result = layout.Render(new LogEventInfo(LogLevel.Info, "Test", " foo bar bar foo bar FOO"));

            // Assert
            Assert.Equal(" BAR bar bar BAR bar BAR", result);
        }

        [Fact]
        public void ReplaceTestWholeWordsWithoutRegEx()
        {
            // Arrange
            SimpleLayout layout = @"${regex-replace:inner=${message}:searchFor=foo:replaceWith=BAR:ignorecase=true:WholeWords=true}";

            // Act
            var result = layout.Render(new LogEventInfo(LogLevel.Info, "Test", " foo bar bar foobar bar FOO"));

            // Assert
            Assert.Equal(" BAR bar bar foobar bar BAR", result);
        }

        [Fact]
        public void ReplaceTestWithSimpleRegExFromConfig()
        {
            var configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
<nlog throwExceptions='true'>
    <targets>
        <target name='d1' type='Debug' layout='${regex-replace:inner=${message}:searchFor=\\r\\n|\\s:replaceWith= }' />
    </targets>
    <rules>
      <logger name=""*"" minlevel=""Trace"" writeTo=""d1"" />
    </rules>
</nlog>");

            var d1 = configuration.FindTargetByName("d1") as DebugTarget;
            Assert.NotNull(d1);
            var layout = d1.Layout as SimpleLayout;
            Assert.NotNull(layout);

            var result = layout.Render(new LogEventInfo(LogLevel.Info, "Test", "\r\nfoo\rbar\nbar\tbar bar \n bar"));
            Assert.Equal(" foo bar bar bar bar   bar", result);
        }

        [Fact]
        public void ReplaceTestWithSimpleRegExFromConfig2()
        {
            var configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
<nlog throwExceptions='true'>
    <variable name=""whitespace"" value=""\\r\\n|\\s"" />
    <variable name=""oneLineMessage"" value=""${regex-replace:inner=${message}:searchFor=${whitespace}:replaceWith= }"" />
    <targets>
      <target name=""d1"" type=""Debug"" layout=""${oneLineMessage}"" />
    </targets>
    <rules>
      <logger name=""*"" minlevel=""Trace"" writeTo=""d1"" />
    </rules>
</nlog>");

            var d1 = configuration.FindTargetByName("d1") as DebugTarget;
            Assert.NotNull(d1);
            var layout = d1.Layout as SimpleLayout;
            Assert.NotNull(layout);

            var result = layout.Render(new LogEventInfo(LogLevel.Info, "Test", "\r\nfoo\rbar\nbar\tbar bar \n bar"));
            Assert.Equal(" foo bar bar bar bar   bar", result);
        }

        [Fact]
        public void ReplaceTestWithComplexRegEx()
        {
            var logFactory = new LogFactory().Setup().LoadConfigurationFromXml(@"
<nlog throwExceptions='true'>
    <variable name=""searchExp""
              value=""(?&lt;!\\d[ -]*)(?\:(?&lt;digits&gt;\\d)[ -]*)\{8,16\}(?=(\\d[ -]*)\{3\}(\\d)(?![ -]\\d))""
              />

    <variable name=""message1"" value=""${regex-replace:inner=${message}:searchFor=${searchExp}:replaceWith=X:replaceGroupName=digits:ignorecase=true}"" />

    <targets>
      <target name=""d1"" type=""Debug"" layout=""${message1}"" />
    </targets>

    <rules>
      <logger name=""*"" minlevel=""Trace"" writeTo=""d1"" />
    </rules>
</nlog>").LogFactory;

            var d1 = logFactory.Configuration.FindTargetByName<DebugTarget>("d1");
            Assert.NotNull(d1);
            var layout = d1.Layout as SimpleLayout;
            Assert.NotNull(layout);

            var testCases = new List<Tuple<string, string>>
            {
                Tuple.Create("1234", "1234"),
                Tuple.Create("1234-5678-1234-5678", "XXXX-XXXX-XXXX-5678"),
                Tuple.Create("1234 5678 1234 5678", "XXXX XXXX XXXX 5678"),
                Tuple.Create("1234567812345678", "XXXXXXXXXXXX5678"),
                Tuple.Create("ABCD-1234-5678-1234-5678", "ABCD-XXXX-XXXX-XXXX-5678"),
                Tuple.Create("1234-5678-1234-5678-ABCD", "XXXX-XXXX-XXXX-5678-ABCD"),
                Tuple.Create("ABCD-1234-5678-1234-5678-ABCD", "ABCD-XXXX-XXXX-XXXX-5678-ABCD"),
            };

            foreach (var testCase in testCases)
            {
                var result = layout.Render(new LogEventInfo(LogLevel.Info, "Test", testCase.Item1));
                Assert.Equal(testCase.Item2, result);
            }
        }
    }
}
