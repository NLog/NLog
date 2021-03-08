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
using System.Globalization;
using System.Linq;
using System.Text;
using NLog.Config;
using NLog.Filters;

namespace NLog.UnitTests.Layouts
{
    using NLog.LayoutRenderers;
    using NLog.LayoutRenderers.Wrappers;
    using NLog.Layouts;
    using NLog.Targets;
    using System;
    using Xunit;
    using static Config.TargetConfigurationTests;

    public class SimpleLayoutParserTests : NLogTestBase
    {
        [Fact]
        public void SimpleTest()
        {
            SimpleLayout l = "${message}";
            Assert.Single(l.Renderers);
            Assert.IsType<MessageLayoutRenderer>(l.Renderers[0]);
        }

        [Fact]
        public void UnclosedTest()
        {
            new SimpleLayout("${message");
        }

        [Fact]
        public void SingleParamTest()
        {
            SimpleLayout l = "${event-property:item=AAA}";
            Assert.Single(l.Renderers);
            var eventPropertyLayout = l.Renderers[0] as EventPropertiesLayoutRenderer;
            Assert.NotNull(eventPropertyLayout);
            Assert.Equal("AAA", eventPropertyLayout.Item);
        }

        [Fact]
        public void ValueWithColonTest()
        {
            SimpleLayout l = "${event-property:item=AAA\\:}";
            Assert.Single(l.Renderers);
            var eventPropertyLayout = l.Renderers[0] as EventPropertiesLayoutRenderer;
            Assert.NotNull(eventPropertyLayout);
            Assert.Equal("AAA:", eventPropertyLayout.Item);
        }

        [Fact]
        public void ValueWithBracketTest()
        {
            SimpleLayout l = "${event-property:item=AAA\\}\\:}";
            Assert.Equal("${event-property:item=AAA\\}\\:}", l.Text);
            Assert.Single(l.Renderers);
            var eventPropertyLayout = l.Renderers[0] as EventPropertiesLayoutRenderer;
            Assert.NotNull(eventPropertyLayout);
            Assert.Equal("AAA}:", eventPropertyLayout.Item);
        }

        [Fact]
        public void DefaultValueTest()
        {
            SimpleLayout l = "${event-property:BBB}";
            Assert.Single(l.Renderers);
            var eventPropertyLayout = l.Renderers[0] as EventPropertiesLayoutRenderer;
            Assert.NotNull(eventPropertyLayout);
            Assert.Equal("BBB", eventPropertyLayout.Item);
        }

        [Fact]
        public void DefaultValueWithBracketTest()
        {
            SimpleLayout l = "${event-property:AAA\\}\\:}";
            Assert.Equal("${event-property:AAA\\}\\:}", l.Text);
            Assert.Single(l.Renderers);
            var eventPropertyLayout = l.Renderers[0] as EventPropertiesLayoutRenderer;
            Assert.NotNull(eventPropertyLayout);
            Assert.Equal("AAA}:", eventPropertyLayout.Item);
        }

        [Fact]
        public void DefaultValueWithOtherParametersTest()
        {
            SimpleLayout l = "${exception:message,type:separator=x}";
            Assert.Single(l.Renderers);
            ExceptionLayoutRenderer elr = l.Renderers[0] as ExceptionLayoutRenderer;
            Assert.NotNull(elr);
            Assert.Equal("message,type", elr.Format);
            Assert.Equal("x", elr.Separator);
        }

        [Fact]
        public void EmptyValueTest()
        {
            SimpleLayout l = "${event-property:item=}";
            Assert.Single(l.Renderers);
            var eventPropertyLayout = l.Renderers[0] as EventPropertiesLayoutRenderer;
            Assert.NotNull(eventPropertyLayout);
            Assert.Equal("", eventPropertyLayout.Item);
        }

        [Fact]
        public void NestedLayoutTest()
        {
            SimpleLayout l = "${rot13:inner=${scopenested:topFrames=3:separator=x}}";
            Assert.Single(l.Renderers);
            var lr = l.Renderers[0] as Rot13LayoutRendererWrapper;
            Assert.NotNull(lr);
            var nestedLayout = lr.Inner as SimpleLayout;
            Assert.NotNull(nestedLayout);
            Assert.Equal("${scopenested:topFrames=3:separator=x}", nestedLayout.Text);
            Assert.Single(nestedLayout.Renderers);
            var nestedLayoutRenderer = nestedLayout.Renderers[0] as ScopeContextNestedStatesLayoutRenderer;
            Assert.NotNull(nestedLayoutRenderer);
            Assert.Equal(3, nestedLayoutRenderer.TopFrames);
            Assert.Equal("x", nestedLayoutRenderer.Separator.ToString());
        }

        [Fact]
        public void DoubleNestedLayoutTest()
        {
            SimpleLayout l = "${rot13:inner=${rot13:inner=${scopenested:topFrames=3:separator=x}}}";
            Assert.Single(l.Renderers);
            var lr = l.Renderers[0] as Rot13LayoutRendererWrapper;
            Assert.NotNull(lr);
            var nestedLayout0 = lr.Inner as SimpleLayout;
            Assert.NotNull(nestedLayout0);
            Assert.Equal("${rot13:inner=${scopenested:topFrames=3:separator=x}}", nestedLayout0.Text);
            var innerRot13 = nestedLayout0.Renderers[0] as Rot13LayoutRendererWrapper;
            var nestedLayout = innerRot13.Inner as SimpleLayout;
            Assert.NotNull(nestedLayout);
            Assert.Equal("${scopenested:topFrames=3:separator=x}", nestedLayout.Text);
            Assert.Single(nestedLayout.Renderers);
            var nestedLayoutRenderer = nestedLayout.Renderers[0] as ScopeContextNestedStatesLayoutRenderer;
            Assert.NotNull(nestedLayoutRenderer);
            Assert.Equal(3, nestedLayoutRenderer.TopFrames);
            Assert.Equal("x", nestedLayoutRenderer.Separator.ToString());
        }

        [Fact]
        public void DoubleNestedLayoutWithDefaultLayoutParametersTest()
        {
            SimpleLayout l = "${rot13:${rot13:${scopenested:topFrames=3:separator=x}}}";
            Assert.Single(l.Renderers);
            var lr = l.Renderers[0] as Rot13LayoutRendererWrapper;
            Assert.NotNull(lr);
            var nestedLayout0 = lr.Inner as SimpleLayout;
            Assert.NotNull(nestedLayout0);
            Assert.Equal("${rot13:${scopenested:topFrames=3:separator=x}}", nestedLayout0.Text);
            var innerRot13 = nestedLayout0.Renderers[0] as Rot13LayoutRendererWrapper;
            var nestedLayout = innerRot13.Inner as SimpleLayout;
            Assert.NotNull(nestedLayout);
            Assert.Equal("${scopenested:topFrames=3:separator=x}", nestedLayout.Text);
            Assert.Single(nestedLayout.Renderers);
            var nestedLayoutRenderer = nestedLayout.Renderers[0] as ScopeContextNestedStatesLayoutRenderer;
            Assert.NotNull(nestedLayoutRenderer);
            Assert.Equal(3, nestedLayoutRenderer.TopFrames);
            Assert.Equal("x", nestedLayoutRenderer.Separator.ToString());
        }

        [Fact]
        public void AmbientPropertyTest()
        {
            SimpleLayout l = "${message:padding=10}";
            Assert.Single(l.Renderers);
            var pad = l.Renderers[0] as PaddingLayoutRendererWrapper;
            Assert.NotNull(pad);
            var message = ((SimpleLayout)pad.Inner).Renderers[0] as MessageLayoutRenderer;
            Assert.NotNull(message);
        }

        [Fact]
        public void MissingLayoutRendererTest()
        {
            LogManager.ThrowConfigExceptions = true;
            Assert.Throws<NLogConfigurationException>(() =>
            {
                SimpleLayout l = "${rot13:${foobar}}";
            });
        }

        [Fact]
        public void DoubleAmbientPropertyTest()
        {
            SimpleLayout l = "${message:uppercase=true:padding=10}";
            Assert.Single(l.Renderers);
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
            Assert.Single(l.Renderers);
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
            ScopeContext.Clear();

            var configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
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
            Assert.True(l1.Regex);
            Assert.True(l1.IgnoreCase);
            Assert.Equal(@"::", l1.ReplaceWith);
            Assert.Equal(@"(?<!\d[ -]*)(?:(?<digits>\d)[ -]*){8,16}(?=(\d[ -]*){3}(\d)(?![ -]\d))", l1.SearchFor);
        }

        [Fact]
        public void LayoutParserEscapeCodesForRegExTestV2()
        {
            ScopeContext.Clear();

            var configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
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
            Assert.True(l1.Regex);
            Assert.True(l1.IgnoreCase);
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

        [Fact]
        public void InvalidLayoutWillParsePartly()
        {
            using (new NoThrowNLogExceptions())
            {
                SimpleLayout l = @"aaa ${iDontExist} bbb";

                var le = LogEventInfo.Create(LogLevel.Info, "logger", "message");
                Assert.Equal("aaa  bbb", l.Render(le));
            }
        }

        [Fact]
        public void InvalidLayoutWillThrowIfExceptionThrowingIsOn()
        {
            LogManager.ThrowConfigExceptions = true;
            Assert.Throws<NLogConfigurationException>(() =>
            {
                SimpleLayout l = @"aaa ${iDontExist} bbb";
            });
        }

        [Fact]
        public void InvalidLayoutWithExistingRenderer_WillThrowIfExceptionThrowingIsOn()
        {
            ConfigurationItemFactory.Default.LayoutRenderers.RegisterDefinition("layoutrenderer-with-list", typeof(LayoutRendererWithListParam));
            LogManager.ThrowConfigExceptions = true;
            Assert.Throws<NLogConfigurationException>(() =>
            {
                SimpleLayout l = @"${layoutrenderer-with-list:}";
            });

        }

        [Fact]
        public void UnknownPropertyInLayout_WillThrowIfExceptionThrowingIsOn()
        {
            ConfigurationItemFactory.Default.LayoutRenderers.RegisterDefinition("layoutrenderer-with-list", typeof(LayoutRendererWithListParam));
            LogManager.ThrowConfigExceptions = true;

            Assert.Throws<NLogConfigurationException>(() =>
            {
                SimpleLayout l = @"${layoutrenderer-with-list:iDontExist=1}";
            });
        }

        /// <summary>
        /// 
        /// Test layout with Generic List type. - is the separator
        /// 
        /// 
        /// </summary>
        /// <remarks>
        /// comma escape is backtick (cannot use backslash due to layout parse)
        /// </remarks>
        /// <param name="input"></param>
        /// <param name="propname"></param>
        /// <param name="expected"></param>
        [Theory]
        [InlineData("2,3,4", "numbers", "2-3-4")]
        [InlineData("a,b,c", "Strings", "a-b-c")]
        [InlineData("a,b,c", "Objects", "a-b-c")]
        [InlineData("a,,b,c", "Strings", "a--b-c")]
        [InlineData("a`b,c", "Strings", "a`b-c")]
        [InlineData("a\'b,c", "Strings", "a'b-c")]
        [InlineData("'a,b',c", "Strings", "a,b-c")]
        [InlineData("2.0,3.0,4.0", "doubles", "2-3-4")]
        [InlineData("2.1,3.2,4.3", "doubles", "2.1-3.2-4.3")]
        [InlineData("Ignore,Neutral,Ignore", "enums", "Ignore-Neutral-Ignore")]
        [InlineData("ASCII,ISO-8859-1, UTF-8", "encodings", "us-ascii-iso-8859-1-utf-8")]
        [InlineData("ASCII,ISO-8859-1,UTF-8", "encodings", "us-ascii-iso-8859-1-utf-8")]
        [InlineData("Value1,Value3,Value2", "FlagEnums", "Value1-Value3-Value2")]
        [InlineData("2,3,4", "IEnumerableNumber", "2-3-4")]
        [InlineData("2,3,4", "IListNumber", "2-3-4")]
        [InlineData("2,3,4", "HashsetNumber", "2-3-4")]
#if !NET35
        [InlineData("2,3,4", "ISetNumber", "2-3-4")]
#endif
        [InlineData("a,b,c", "IEnumerableString", "a-b-c")]
        [InlineData("a,b,c", "IListString", "a-b-c")]
        [InlineData("a,b,c", "HashSetString", "a-b-c")]
#if !NET35
        [InlineData("a,b,c", "ISetString", "a-b-c")]
#endif
        public void LayoutWithListParamTest(string input, string propname, string expected)
        {
            ConfigurationItemFactory.Default.LayoutRenderers.RegisterDefinition("layoutrenderer-with-list", typeof(LayoutRendererWithListParam));
            SimpleLayout l = $@"${{layoutrenderer-with-list:{propname}={input}}}";

            var le = LogEventInfo.Create(LogLevel.Info, "logger", "message");
            var actual = l.Render(le);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("2,,3,4", "numbers")]
        [InlineData("a,bc", "numbers")]
        [InlineData("value1,value10", "FlagEnums")]
        public void LayoutWithListParamTest_incorrect(string input, string propname)
        {
            //note flags enum already supported

            //can;t convert empty to int
            ConfigurationItemFactory.Default.LayoutRenderers.RegisterDefinition("layoutrenderer-with-list", typeof(LayoutRendererWithListParam));
            Assert.Throws<NLogConfigurationException>(() =>
            {
                SimpleLayout l = $@"${{layoutrenderer-with-list:{propname}={input}}}";

            });
        }

        [Theory]
        [InlineData(@"                                    ${literal:text={0\} {1\}}")]
        [InlineData(@"                           ${cached:${literal:text={0\} {1\}}}")]
        [InlineData(@"                  ${cached:${cached:${literal:text={0\} {1\}}}}")]
        [InlineData(@"         ${cached:${cached:${cached:${literal:text={0\} {1\}}}}}")]
        [InlineData(@"${cached:${cached:${cached:${cached:${literal:text={0\} {1\}}}}}}")]
        public void Render_EscapedBrackets_ShouldRenderAllBrackets(string input)
        {
            SimpleLayout simple = input.Trim();
            var result = simple.Render(LogEventInfo.CreateNullEvent());
            Assert.Equal("{0} {1}", result);
        }

        [Fact]
        void FuncLayoutRendererRegisterTest1()
        {
            LayoutRenderer.Register("the-answer", (info) => "42");
            Layout l = "${the-answer}";
            var result = l.Render(LogEventInfo.CreateNullEvent());
            Assert.Equal("42", result);
        }

        [Fact]
        void FuncLayoutRendererFluentMethod_ThreadSafe_Test()
        {
            // Arrange
            var layout = Layout.FromMethod(l => "42", LayoutRenderOptions.ThreadSafe);
            // Act
            var result = layout.Render(LogEventInfo.CreateNullEvent());
            // Assert
            Assert.Equal("42", result);
            Assert.True(layout.ThreadSafe);
            Assert.False(layout.ThreadAgnostic);
        }

        [Fact]
        void FuncLayoutRendererFluentMethod_ThreadAgnostic_Test()
        {
            // Arrange
            var layout = Layout.FromMethod(l => "42", LayoutRenderOptions.ThreadAgnostic);
            // Act
            var result = layout.Render(LogEventInfo.CreateNullEvent());
            // Assert
            Assert.Equal("42", result);
            Assert.True(layout.ThreadSafe);
            Assert.True(layout.ThreadAgnostic);
        }

        [Fact]
        void FuncLayoutRendererFluentMethod_ThreadUnsafe_Test()
        {
            // Arrange
            var layout = Layout.FromMethod(l => "42", LayoutRenderOptions.None);
            // Act
            var result = layout.Render(LogEventInfo.CreateNullEvent());
            // Assert
            Assert.Equal("42", result);
            Assert.False(layout.ThreadSafe);
            Assert.False(layout.ThreadAgnostic);
        }

        [Fact]
        void FuncLayoutRendererFluentMethod_NullThrows_Test()
        {
            // Arrange
            Assert.Throws<ArgumentNullException>(() => Layout.FromMethod(null));
        }

        [Fact]
        void FuncLayoutRendererRegisterTest1WithXML()
        {
            LayoutRenderer.Register("the-answer", (info) => 42);

            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
<nlog throwExceptions='true'>
            
                <targets>
                    <target name='debug' type='Debug' layout= 'TheAnswer=${the-answer:Format=D3}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>");

            var logger = LogManager.GetCurrentClassLogger();
            logger.Debug("test1");
            AssertDebugLastMessage("debug", "TheAnswer=042");
        }

        [Fact]
        void FuncLayoutRendererRegisterTest2()
        {
            LayoutRenderer.Register("message-length", (info) => info.Message.Length);
            Layout l = "${message-length}";
            var result = l.Render(LogEventInfo.Create(LogLevel.Error, "logger-adhoc", "1234567890"));
            Assert.Equal("10", result);
        }

        [Fact]
        void SimpleLayout_FromString_ThrowConfigExceptions()
        {
            Assert.Throws<NLogConfigurationException>(() => Layout.FromString("${evil}", true));
        }

        [Fact]
        void SimpleLayout_FromString_NoThrowConfigExceptions()
        {
            Assert.NotNull(Layout.FromString("${evil}", false));
        }

        [Theory]
        [InlineData("", true)]
        [InlineData(null, true)]
        [InlineData("'a'", true)]
        [InlineData("${gdc:a}", false)]
        public void FromString_isFixedText(string input, bool expected)
        {
            // Act
            var layout = (SimpleLayout)Layout.FromString(input);
            layout.Initialize(null);

            // Assert
            Assert.Equal(expected, layout.IsFixedText);
        }

        [Theory]
        [InlineData("", true)]
        [InlineData(null, true)]
        [InlineData("'a'", true)]
        [InlineData("${gdc:a}", true)]
        public void FromString_isThreadSafe(string input, bool expected)
        {
            // Act
            var layout = (SimpleLayout)Layout.FromString(input);
            layout.Initialize(null);

            // Assert
            Assert.Equal(expected, layout.ThreadSafe);
        }

        [Theory]
        [InlineData("", "")]
        [InlineData(null, "")]
        [InlineData("'a'", "'a'")]
        [InlineData("${gdc:a}", "")]
        public void Render(string input, string expected)
        {
            var layout = (SimpleLayout)Layout.FromString(input);

            // Act
            var result = layout.Render(LogEventInfo.CreateNullEvent());

            // Assert
            Assert.Equal(expected, result);
        }


        [Fact]
        public void Parse_AppDomainFixedOutput_ConvertToLiteral()
        {
            // Arrange
            var input = "${newline}";

            // Act
            var layout = (SimpleLayout)Layout.FromString(input);

            // Assert
            var single = Assert.Single(layout.Renderers);
            Assert.IsType<LiteralLayoutRenderer>(single);
        }

        [Fact]
        public void Parse_MultipleAppDomainFixedOutput_ConvertSingleToLiteral()
        {
            // Arrange
            var input = "${newline}${machinename}";

            // Act
            var layout = (SimpleLayout)Layout.FromString(input);

            // Assert
            var single = Assert.Single(layout.Renderers);
            Assert.IsType<LiteralLayoutRenderer>(single);
        }

        [Fact]
        public void Parse_AppDomainFixedOutputWithRawValue_ConvertSingleToLiteralAndKeepRawValue()
        {
            // Arrange
            var input = "${processid}";

            // Act
            var layout = (SimpleLayout)Layout.FromString(input);

            // Assert
            var single = Assert.Single(layout.Renderers);
            var singleRaw = Assert.IsType<LiteralWithRawValueLayoutRenderer>(single);
            var succeeded = singleRaw.TryGetRawValue(LogEventInfo.CreateNullEvent(), out var rawValue);
            Assert.True(succeeded);
            var rawValueInt = Assert.IsType<int>(rawValue);
            Assert.True(rawValueInt > 0);


        }

        /// <summary>
        /// Combined literals should not support rawValue
        /// </summary>
        [Theory]
        [InlineData("${newline}${processid}")]
        [InlineData("${processid}${processid}")]
        [InlineData("${processid}${processname}")]
        [InlineData("${processname}${processid}")]
        [InlineData("${processname}-${processid}")]
        public void Parse_Multiple_ConvertSingleToLiteralWithoutRaw(string input)
        {
            // Act
            var layout = (SimpleLayout)Layout.FromString(input);

            // Assert
            var single = Assert.Single(layout.Renderers);
            Assert.IsType<LiteralLayoutRenderer>(single);
        }

        private class LayoutRendererWithListParam : LayoutRenderer
        {
            public List<double> Doubles { get; set; }

            public List<FilterResult> Enums { get; set; }

            public List<MyFlagsEnum> FlagEnums { get; set; }

            public List<int> Numbers { get; set; }

            public List<string> Strings { get; set; }

            public List<object> Objects { get; set; }

            public List<Encoding> Encodings { get; set; }

            public IEnumerable<string> IEnumerableString { get; set; }

            public IEnumerable<int> IEnumerableNumber { get; set; }

            public IList<string> IListString { get; set; }

            public IList<int> IListNumber { get; set; }

#if !NET35
            public ISet<string> ISetString { get; set; }

            public ISet<int> ISetNumber { get; set; }

#endif

            public HashSet<int> HashSetNumber { get; set; }

            public HashSet<string> HashSetString { get; set; }

            /// <summary>
            /// Renders the specified environmental information and appends it to the specified <see cref="StringBuilder" />.
            /// </summary>
            /// <param name="builder">The <see cref="StringBuilder"/> to append the rendered data to.</param>
            /// <param name="logEvent">Logging event.</param>
            protected override void Append(StringBuilder builder, LogEventInfo logEvent)
            {
                Append(builder, Strings);
                AppendFormattable(builder, Numbers);
                AppendFormattable(builder, Enums);
                AppendFormattable(builder, FlagEnums);
                AppendFormattable(builder, Doubles);
                Append(builder, Encodings?.Select(e => e.BodyName).ToList());
                Append(builder, Objects);
                Append(builder, IEnumerableString);
                AppendFormattable(builder, IEnumerableNumber);
                Append(builder, IListString);
                AppendFormattable(builder, IListNumber);
#if !NET35
                Append(builder, ISetString);
                AppendFormattable(builder, ISetNumber);
#endif
                Append(builder, HashSetString);
                AppendFormattable(builder, HashSetNumber);
            }

            private void Append<T>(StringBuilder builder, IEnumerable<T> items)
            {
                if (items != null) builder.Append(string.Join("-", items.ToArray()));
            }

            private void AppendFormattable<T>(StringBuilder builder, IEnumerable<T> items)
                where T : IFormattable
            {
                if (items != null) builder.Append(string.Join("-", items.Select(it => it.ToString(null, CultureInfo.InvariantCulture)).ToArray()));
            }
        }
    }
}
