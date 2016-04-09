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

namespace NLog.UnitTests.Layouts
{
    using NLog.LayoutRenderers;
    using NLog.LayoutRenderers.Wrappers;
    using NLog.Layouts;
    using NLog.Targets;
    using Xunit;

    public class SimpleLayoutParserTests : NLogTestBase
    {
        [Fact]
        public void SimpleTest()
        {
            SimpleLayout l = "${message}";
            Assert.Equal(1, l.Renderers.Count);
            Assert.IsType(typeof(MessageLayoutRenderer), l.Renderers[0]);
        }

        [Fact]
        public void UnclosedTest()
        {
            new SimpleLayout("${message");
        }

        [Fact]
        public void SingleParamTest()
        {
            SimpleLayout l = "${mdc:item=AAA}";
            Assert.Equal(1, l.Renderers.Count);
            MdcLayoutRenderer mdc = l.Renderers[0] as MdcLayoutRenderer;
            Assert.NotNull(mdc);
            Assert.Equal("AAA", mdc.Item);
        }

        [Fact]
        public void ValueWithColonTest()
        {
            SimpleLayout l = "${mdc:item=AAA\\:}";
            Assert.Equal(1, l.Renderers.Count);
            MdcLayoutRenderer mdc = l.Renderers[0] as MdcLayoutRenderer;
            Assert.NotNull(mdc);
            Assert.Equal("AAA:", mdc.Item);
        }

        [Fact]
        public void ValueWithBracketTest()
        {
            SimpleLayout l = "${mdc:item=AAA\\}\\:}";
            Assert.Equal("${mdc:item=AAA\\}\\:}", l.Text);
            //Assert.Equal(1, l.Renderers.Count);
            MdcLayoutRenderer mdc = l.Renderers[0] as MdcLayoutRenderer;
            Assert.NotNull(mdc);
            Assert.Equal("AAA}:", mdc.Item);
        }

        [Fact]
        public void DefaultValueTest()
        {
            SimpleLayout l = "${mdc:BBB}";
            //Assert.Equal(1, l.Renderers.Count);
            MdcLayoutRenderer mdc = l.Renderers[0] as MdcLayoutRenderer;
            Assert.NotNull(mdc);
            Assert.Equal("BBB", mdc.Item);
        }

        [Fact]
        public void DefaultValueWithBracketTest()
        {
            SimpleLayout l = "${mdc:AAA\\}\\:}";
            Assert.Equal(l.Text, "${mdc:AAA\\}\\:}");
            Assert.Equal(1, l.Renderers.Count);
            MdcLayoutRenderer mdc = l.Renderers[0] as MdcLayoutRenderer;
            Assert.NotNull(mdc);
            Assert.Equal("AAA}:", mdc.Item);
        }

        [Fact]
        public void DefaultValueWithOtherParametersTest()
        {
            SimpleLayout l = "${exception:message,type:separator=x}";
            Assert.Equal(1, l.Renderers.Count);
            ExceptionLayoutRenderer elr = l.Renderers[0] as ExceptionLayoutRenderer;
            Assert.NotNull(elr);
            Assert.Equal("message,type", elr.Format);
            Assert.Equal("x", elr.Separator);
        }

        [Fact]
        public void EmptyValueTest()
        {
            SimpleLayout l = "${mdc:item=}";
            Assert.Equal(1, l.Renderers.Count);
            MdcLayoutRenderer mdc = l.Renderers[0] as MdcLayoutRenderer;
            Assert.NotNull(mdc);
            Assert.Equal("", mdc.Item);
        }

        [Fact]
        public void NestedLayoutTest()
        {
            SimpleLayout l = "${rot13:inner=${ndc:topFrames=3:separator=x}}";
            Assert.Equal(1, l.Renderers.Count);
            var lr = l.Renderers[0] as Rot13LayoutRendererWrapper;
            Assert.NotNull(lr);
            var nestedLayout = lr.Inner as SimpleLayout;
            Assert.NotNull(nestedLayout);
            Assert.Equal("${ndc:topFrames=3:separator=x}", nestedLayout.Text);
            Assert.Equal(1, nestedLayout.Renderers.Count);
            var ndcLayoutRenderer = nestedLayout.Renderers[0] as NdcLayoutRenderer;
            Assert.NotNull(ndcLayoutRenderer);
            Assert.Equal(3, ndcLayoutRenderer.TopFrames);
            Assert.Equal("x", ndcLayoutRenderer.Separator);
        }

        [Fact]
        public void DoubleNestedLayoutTest()
        {
            SimpleLayout l = "${rot13:inner=${rot13:inner=${ndc:topFrames=3:separator=x}}}";
            Assert.Equal(1, l.Renderers.Count);
            var lr = l.Renderers[0] as Rot13LayoutRendererWrapper;
            Assert.NotNull(lr);
            var nestedLayout0 = lr.Inner as SimpleLayout;
            Assert.NotNull(nestedLayout0);
            Assert.Equal("${rot13:inner=${ndc:topFrames=3:separator=x}}", nestedLayout0.Text);
            var innerRot13 = nestedLayout0.Renderers[0] as Rot13LayoutRendererWrapper;
            var nestedLayout = innerRot13.Inner as SimpleLayout;
            Assert.NotNull(nestedLayout);
            Assert.Equal("${ndc:topFrames=3:separator=x}", nestedLayout.Text);
            Assert.Equal(1, nestedLayout.Renderers.Count);
            var ndcLayoutRenderer = nestedLayout.Renderers[0] as NdcLayoutRenderer;
            Assert.NotNull(ndcLayoutRenderer);
            Assert.Equal(3, ndcLayoutRenderer.TopFrames);
            Assert.Equal("x", ndcLayoutRenderer.Separator);
        }

        [Fact]
        public void DoubleNestedLayoutWithDefaultLayoutParametersTest()
        {
            SimpleLayout l = "${rot13:${rot13:${ndc:topFrames=3:separator=x}}}";
            Assert.Equal(1, l.Renderers.Count);
            var lr = l.Renderers[0] as Rot13LayoutRendererWrapper;
            Assert.NotNull(lr);
            var nestedLayout0 = lr.Inner as SimpleLayout;
            Assert.NotNull(nestedLayout0);
            Assert.Equal("${rot13:${ndc:topFrames=3:separator=x}}", nestedLayout0.Text);
            var innerRot13 = nestedLayout0.Renderers[0] as Rot13LayoutRendererWrapper;
            var nestedLayout = innerRot13.Inner as SimpleLayout;
            Assert.NotNull(nestedLayout);
            Assert.Equal("${ndc:topFrames=3:separator=x}", nestedLayout.Text);
            Assert.Equal(1, nestedLayout.Renderers.Count);
            var ndcLayoutRenderer = nestedLayout.Renderers[0] as NdcLayoutRenderer;
            Assert.NotNull(ndcLayoutRenderer);
            Assert.Equal(3, ndcLayoutRenderer.TopFrames);
            Assert.Equal("x", ndcLayoutRenderer.Separator);
        }

        [Fact]
        public void AmbientPropertyTest()
        {
            SimpleLayout l = "${message:padding=10}";
            Assert.Equal(1, l.Renderers.Count);
            var pad = l.Renderers[0] as PaddingLayoutRendererWrapper;
            Assert.NotNull(pad);
            var message = ((SimpleLayout)pad.Inner).Renderers[0] as MessageLayoutRenderer;
            Assert.NotNull(message);
        }

        [Fact]
        public void MissingLayoutRendererTest()
        {
            Assert.Throws<NLogConfigurationException>(() =>
            {
                SimpleLayout l = "${rot13:${foobar}}";
            });
        }

        [Fact]
        public void DoubleAmbientPropertyTest()
        {
            SimpleLayout l = "${message:uppercase=true:padding=10}";
            Assert.Equal(1, l.Renderers.Count);
            var upperCase = l.Renderers[0] as UppercaseLayoutRendererWrapper;
            Assert.NotNull(upperCase);
            var pad = ((SimpleLayout)upperCase.Inner).Renderers[0] as PaddingLayoutRendererWrapper;
            Assert.NotNull(pad);
            var message = ((SimpleLayout)pad.Inner).Renderers[0] as MessageLayoutRenderer;
            Assert.NotNull(message);
        }

        [Fact]
        public void ReverseDoubleAmbientPropertyTest()
        {
            SimpleLayout l = "${message:padding=10:uppercase=true}";
            Assert.Equal(1, l.Renderers.Count);
            var pad = ((SimpleLayout)l).Renderers[0] as PaddingLayoutRendererWrapper;
            Assert.NotNull(pad);
            var upperCase = ((SimpleLayout)pad.Inner).Renderers[0] as UppercaseLayoutRendererWrapper;
            Assert.NotNull(upperCase);
            var message = ((SimpleLayout)upperCase.Inner).Renderers[0] as MessageLayoutRenderer;
            Assert.NotNull(message);
        }

        [Fact]
        public void EscapeTest()
        {
            AssertEscapeRoundTrips(string.Empty);
            AssertEscapeRoundTrips("hello ${${}} world!");
            AssertEscapeRoundTrips("hello $");
            AssertEscapeRoundTrips("hello ${");
            AssertEscapeRoundTrips("hello $${{");
            AssertEscapeRoundTrips("hello ${message}");
            AssertEscapeRoundTrips("hello ${${level}}");
            AssertEscapeRoundTrips("hello ${${level}${message}}");
        }

        [Fact]
        public void EvaluateTest()
        {
            var logEventInfo = LogEventInfo.CreateNullEvent();
            logEventInfo.Level = LogLevel.Warn;
            Assert.Equal("Warn", SimpleLayout.Evaluate("${level}", logEventInfo));
        }

        [Fact]
        public void EvaluateTest2()
        {
            Assert.Equal("Off", SimpleLayout.Evaluate("${level}"));
            Assert.Equal(string.Empty, SimpleLayout.Evaluate("${message}"));
            Assert.Equal(string.Empty, SimpleLayout.Evaluate("${logger}"));
        }

        private static void AssertEscapeRoundTrips(string originalString)
        {
            string escapedString = SimpleLayout.Escape(originalString);
            SimpleLayout l = escapedString;
            string renderedString = l.Render(LogEventInfo.CreateNullEvent());
            Assert.Equal(originalString, renderedString);
        }

        [Fact]
        public void LayoutParserEscapeCodesForRegExTestV1()
        {
            MappedDiagnosticsContext.Clear();

            var configuration = CreateConfigurationFromString(@"
<nlog throwExceptions='true'>
    <variable name=""searchExp""
              value=""(?&lt;!\\d[ -]*)(?\u003a(?&lt;digits&gt;\\d)[ -]*)\u007b8,16\u007d(?=(\\d[ -]*)\u007b3\u007d(\\d)(?![ -]\\d))""
              />
    
    <variable name=""message1"" value=""${replace:inner=${message}:searchFor=${searchExp}:replaceWith=\u003a\u003a:regex=true:ignorecase=true}"" />
      
    <targets>
      <target name=""d1"" type=""Debug"" layout=""${message1}"" />
    </targets>

    <rules>
      <logger name=""*"" minlevel=""Trace"" writeTo=""d1"" />
    </rules>
</nlog>");

            var d1 = configuration.FindTargetByName("d1") as DebugTarget;
            Assert.NotNull(d1);
            var layout = d1.Layout as SimpleLayout;
            Assert.NotNull(layout);

            var c = layout.Renderers.Count;
            Assert.Equal(1, c);

            var l1 = layout.Renderers[0] as ReplaceLayoutRendererWrapper;

            Assert.NotNull(l1);
            Assert.Equal(true, l1.Regex);
            Assert.Equal(true, l1.IgnoreCase);
            Assert.Equal(@"::", l1.ReplaceWith);
            Assert.Equal(@"(?<!\d[ -]*)(?:(?<digits>\d)[ -]*){8,16}(?=(\d[ -]*){3}(\d)(?![ -]\d))", l1.SearchFor);
        }

        [Fact]
        public void LayoutParserEscapeCodesForRegExTestV2()
        {
            MappedDiagnosticsContext.Clear();

            var configuration = CreateConfigurationFromString(@"
<nlog throwExceptions='true'>
    <variable name=""searchExp""
              value=""(?&lt;!\\d[ -]*)(?\:(?&lt;digits&gt;\\d)[ -]*)\{8,16\}(?=(\\d[ -]*)\{3\}(\\d)(?![ -]\\d))""
              />
    
    <variable name=""message1"" value=""${replace:inner=${message}:searchFor=${searchExp}:replaceWith=\u003a\u003a:regex=true:ignorecase=true}"" />
      
    <targets>
      <target name=""d1"" type=""Debug"" layout=""${message1}"" />
    </targets>

    <rules>
      <logger name=""*"" minlevel=""Trace"" writeTo=""d1"" />
    </rules>
</nlog>");

            var d1 = configuration.FindTargetByName("d1") as DebugTarget;
            Assert.NotNull(d1);
            var layout = d1.Layout as SimpleLayout;
            Assert.NotNull(layout);

            var c = layout.Renderers.Count;
            Assert.Equal(1, c);

            var l1 = layout.Renderers[0] as ReplaceLayoutRendererWrapper;

            Assert.NotNull(l1);
            Assert.Equal(true, l1.Regex);
            Assert.Equal(true, l1.IgnoreCase);
            Assert.Equal(@"::", l1.ReplaceWith);
            Assert.Equal(@"(?<!\d[ -]*)(?:(?<digits>\d)[ -]*){8,16}(?=(\d[ -]*){3}(\d)(?![ -]\d))", l1.SearchFor);
        }




        [Fact]
        public void InnerLayoutWithColonTest_with_workaround()
        {
            SimpleLayout l = @"${when:when=1 == 1:Inner=Test${literal:text=\:} Hello}";

            var le = LogEventInfo.Create(LogLevel.Info, "logger", "message");
            Assert.Equal("Test: Hello", l.Render(le));
        }

        [Fact]
        public void InnerLayoutWithColonTest()
        {
            SimpleLayout l = @"${when:when=1 == 1:Inner=Test\: Hello}";

            var le = LogEventInfo.Create(LogLevel.Info, "logger", "message");
            Assert.Equal("Test: Hello", l.Render(le));
        }

        [Fact]
        public void InnerLayoutWithSlashSingleTest()
        {
            SimpleLayout l = @"${when:when=1 == 1:Inner=Test\Hello}";

            var le = LogEventInfo.Create(LogLevel.Info, "logger", "message");
            Assert.Equal("Test\\Hello", l.Render(le));
        }

        [Fact]
        public void InnerLayoutWithSlashTest()
        {
            SimpleLayout l = @"${when:when=1 == 1:Inner=Test\Hello}";

            var le = LogEventInfo.Create(LogLevel.Info, "logger", "message");
            Assert.Equal("Test\\Hello", l.Render(le));
        }

        [Fact]
        public void InnerLayoutWithBracketsTest()
        {
            SimpleLayout l = @"${when:when=1 == 1:Inner=Test{Hello\}}";

            var le = LogEventInfo.Create(LogLevel.Info, "logger", "message");
            Assert.Equal("Test{Hello}", l.Render(le));
        }

        [Fact]
        public void InnerLayoutWithBracketsTest2()
        {
            SimpleLayout l = @"${when:when=1 == 1:Inner=Test{Hello\\}}";

            var le = LogEventInfo.Create(LogLevel.Info, "logger", "message");
            Assert.Equal(@"Test{Hello\}", l.Render(le));
        }

        [Fact]
        public void InnerLayoutWithBracketsTest_reverse()
        {
            SimpleLayout l = @"${when:Inner=Test{Hello\}:when=1 == 1}";

            var le = LogEventInfo.Create(LogLevel.Info, "logger", "message");
            Assert.Equal("Test{Hello}", l.Render(le));
        }



        [Fact]
        public void InnerLayoutWithBracketsTest_no_escape()
        {
            SimpleLayout l = @"${when:when=1 == 1:Inner=Test{Hello}}";

            var le = LogEventInfo.Create(LogLevel.Info, "logger", "message");
            Assert.Equal("Test{Hello}", l.Render(le));
        }

        [Fact]
        public void InnerLayoutWithHashTest()
        {
            SimpleLayout l = @"${when:when=1 == 1:inner=Log_{#\}.log}";

            var le = LogEventInfo.Create(LogLevel.Info, "logger", "message");
            Assert.Equal("Log_{#}.log", l.Render(le));
        }



        [Fact]
        public void InnerLayoutWithHashTest_need_escape()
        {
            SimpleLayout l = @"${when:when=1 == 1:inner=L\}.log}";

            var le = LogEventInfo.Create(LogLevel.Info, "logger", "message");
            Assert.Equal("L}.log", l.Render(le));
        }

        [Fact]
        public void InnerLayoutWithBracketsTest_needEscape()
        {
            SimpleLayout l = @"${when:when=1 == 1:inner=\}{.log}";

            var le = LogEventInfo.Create(LogLevel.Info, "logger", "message");
            Assert.Equal("}{.log", l.Render(le));
        }


        [Fact]
        public void InnerLayoutWithBracketsTest_needEscape2()
        {
            SimpleLayout l = @"${when:when=1 == 1:inner={\}\}{.log}";

            var le = LogEventInfo.Create(LogLevel.Info, "logger", "message");
            Assert.Equal("{}}{.log", l.Render(le));
        }

        [Fact]
        public void InnerLayoutWithBracketsTest_needEscape3()
        {
            SimpleLayout l = @"${when:when=1 == 1:inner={\}\}\}.log}";

            var le = LogEventInfo.Create(LogLevel.Info, "logger", "message");
            Assert.Equal("{}}}.log", l.Render(le));
        }

        [Fact]
        public void InnerLayoutWithBracketsTest_needEscape4()
        {
            SimpleLayout l = @"${when:when=1 == 1:inner={\}\}\}.log}";

            var le = LogEventInfo.Create(LogLevel.Info, "logger", "message");
            Assert.Equal("{}}}.log", l.Render(le));
        }

        [Fact]
        public void InnerLayoutWithBracketsTest_needEscape5()
        {
            SimpleLayout l = @"${when:when=1 == 1:inner=\}{a\}.log}";

            var le = LogEventInfo.Create(LogLevel.Info, "logger", "message");
            Assert.Equal("}{a}.log", l.Render(le));
        }

        [Fact]
        public void InnerLayoutWithHashTest_and_layoutrender()
        {
            SimpleLayout l = @"${when:when=1 == 1:inner=${counter}/Log_{#\}.log}";

            var le = LogEventInfo.Create(LogLevel.Info, "logger", "message");
            Assert.Equal("1/Log_{#}.log", l.Render(le));
        }

    }
}
