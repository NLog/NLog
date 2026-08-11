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

namespace NLog.UnitTests.Conditions
{
    using System;
    using System.Linq;
    using NLog.Conditions;
    using NLog.Config;
    using NLog.Internal;
    using NLog.LayoutRenderers;
    using NLog.Layouts;
    using Xunit;

    public class ConditionParserTests : NLogTestBase
    {
        [Fact]
        public void ParseNullText()
        {
            Assert.Null((ConditionExpression)(string)null);
        }

        [Fact]
        public void ParseEmptyText()
        {
            Assert.Throws<ConditionParseException>(() =>((ConditionExpression)""));
        }

        [Fact]
        public void ImplicitOperatorTest()
        {
            ConditionExpression cond = "true and true";

            Assert.IsType<ConditionAndExpression>(cond);
        }

        [Fact]
        public void NullLiteralTest()
        {
            Assert.Equal("null", ((ConditionExpression)"null").ToString());
        }

        [Fact]
        public void BooleanLiteralTest()
        {
            Assert.Equal("True", ((ConditionExpression)"true").ToString());
            Assert.Equal("True", ((ConditionExpression)"tRuE").ToString());
            Assert.Equal("False", ((ConditionExpression)"false").ToString());
            Assert.Equal("False", ((ConditionExpression)"fAlSe").ToString());
        }

        [Fact]
        public void AndTest()
        {
            Assert.Equal("(True and True)", ((ConditionExpression)"true and true").ToString());
            Assert.Equal("(True and True)", ((ConditionExpression)"tRuE AND true").ToString());
            Assert.Equal("(True and True)", ((ConditionExpression)"tRuE && true").ToString());
            Assert.Equal("((True and True) and True)", ((ConditionExpression)"true and true && true").ToString());
            Assert.Equal("((True and True) and True)", ((ConditionExpression)"tRuE AND true and true").ToString());
            Assert.Equal("((True and True) and True)", ((ConditionExpression)"tRuE && true AND true").ToString());
        }

        [Fact]
        public void OrTest()
        {
            Assert.Equal("(True or True)", ((ConditionExpression)"true or true").ToString());
            Assert.Equal("(True or True)", ((ConditionExpression)"tRuE OR true").ToString());
            Assert.Equal("(True or True)", ((ConditionExpression)"tRuE || true").ToString());
            Assert.Equal("((True or True) or True)", ((ConditionExpression)"true or true || true").ToString());
            Assert.Equal("((True or True) or True)", ((ConditionExpression)"tRuE OR true or true").ToString());
            Assert.Equal("((True or True) or True)", ((ConditionExpression)"tRuE || true OR true").ToString());
        }

        [Fact]
        public void NotTest()
        {
            Assert.Equal("(not True)", ((ConditionExpression)"not true").ToString());
            Assert.Equal("(not (not True))", ((ConditionExpression)"not not true").ToString());
            Assert.Equal("(not (not (not True)))", ((ConditionExpression)"not not not true").ToString());
        }

        [Fact]
        public void StringTest()
        {
            Assert.Equal("''", ((ConditionExpression)"''").ToString());
            Assert.Equal("'Foo'", ((ConditionExpression)"'Foo'").ToString());
            Assert.Equal("'Bar'", ((ConditionExpression)"'Bar'").ToString());
            Assert.Equal("'d'Artagnan'", ((ConditionExpression)"'d''Artagnan'").ToString());

            var cle = ((ConditionExpression)"'${message} ${level}'") as ConditionLayoutExpression;
            Assert.NotNull(cle);
            SimpleLayout sl = cle.Layout as SimpleLayout;
            Assert.NotNull(sl);
            Assert.Equal(3, sl.LayoutRenderers.Count());
            Assert.IsType<MessageLayoutRenderer>(sl.LayoutRenderers.ElementAt(0));
            Assert.IsType<LiteralLayoutRenderer>(sl.LayoutRenderers.ElementAt(1));
            Assert.IsType<LevelLayoutRenderer>(sl.LayoutRenderers.ElementAt(2));
        }

        [Fact]
        public void LogLevelTest()
        {
            var result = ((ConditionExpression)"LogLevel.Info") as ConditionLiteralExpression;
            Assert.NotNull(result);
            Assert.Same(LogLevel.Info, result.LiteralValue);

            result = ((ConditionExpression)"LogLevel.Trace") as ConditionLiteralExpression;
            Assert.NotNull(result);
            Assert.Same(LogLevel.Trace, result.LiteralValue);
        }

        [Fact]
        public void RelationalOperatorTest()
        {
            RelationalOperatorTestInner("=", "==");
            RelationalOperatorTestInner("==", "==");
            RelationalOperatorTestInner("!=", "!=");
            RelationalOperatorTestInner("<>", "!=");
            RelationalOperatorTestInner("<", "<");
            RelationalOperatorTestInner(">", ">");
            RelationalOperatorTestInner("<=", "<=");
            RelationalOperatorTestInner(">=", ">=");
        }

        [Fact]
        public void NumberTest()
        {
            ConditionExpression conditionExpression = "3.141592";
            Assert.Equal("3.141592", conditionExpression.ToString());
            Assert.Equal("42", ((ConditionExpression)"42").ToString());
            Assert.Equal("-42", ((ConditionExpression)"-42").ToString());
            Assert.Equal("-3.141592", ((ConditionExpression)"-3.141592").ToString());
        }

        [Fact]
        public void ExtraParenthesisTest()
        {
            Assert.Equal("3.141592", ((ConditionExpression)"(((3.141592)))").ToString());
        }

        [Fact]
        public void MessageTest()
        {
            ConditionExpression result = "message";
            Assert.IsType<ConditionMessageExpression>(result);
            Assert.Equal("message", result.ToString());
        }

        [Fact]
        public void LevelTest()
        {
            ConditionExpression result = "level";
            Assert.IsType<ConditionLevelExpression>(result);
            Assert.Equal("level", result.ToString());
        }

        [Fact]
        public void LoggerTest()
        {
            ConditionExpression result = "logger";
            Assert.IsType<ConditionLoggerNameExpression>(result);
            Assert.Equal("logger", result.ToString());
        }

        [Fact]
        public void ConditionFunctionTests()
        {
            var result = ((ConditionExpression)"starts-with(logger, 'x${message}')") as ConditionMethodExpression;
            Assert.NotNull(result);
            Assert.Equal("starts-with", result.MethodName);
            Assert.Equal("starts-with(logger, 'x${message}')", result.ToString());
            Assert.Equal(2, result.MethodParameters.Count);
        }

        [Fact]
        [Obsolete("ConditionParser will become internal with NLog v7")]
        public void CustomNLogFactoriesTest()
        {
            var configurationItemFactory = new ConfigurationItemFactory();
            configurationItemFactory.GetLayoutRendererFactory().RegisterType<FooLayoutRenderer>("foo");
            configurationItemFactory.ConditionMethodFactory.RegisterDefinition("check", typeof(MyConditionMethods).GetMethod("CheckIt"));

            var result = ConditionParser.ParseExpression("check('${foo}')", configurationItemFactory);
            Assert.NotNull(result);
        }

        [Fact]
        [Obsolete("ConditionParser will become internal with NLog v7")]
        public void MethodNameWithUnderscores()
        {
            var configurationItemFactory = new ConfigurationItemFactory();
            configurationItemFactory.GetLayoutRendererFactory().RegisterType<FooLayoutRenderer>("foo");
            configurationItemFactory.ConditionMethodFactory.RegisterDefinition("__check__", typeof(MyConditionMethods).GetMethod("CheckIt"));

            var result = ConditionParser.ParseExpression("__check__('${foo}')", configurationItemFactory);
            Assert.NotNull(result);
        }

        [Fact]
        public void UnbalancedParenthesis1Test()
        {
            Assert.Throws<ConditionParseException>(() => ((ConditionExpression)"check("));
        }

        [Fact]
        public void UnbalancedParenthesis2Test()
        {
            Assert.Throws<ConditionParseException>(() => ((ConditionExpression)"((1)"));
        }

        [Fact]
        public void UnbalancedParenthesis3Test()
        {
            Assert.Throws<ConditionParseException>(() => ((ConditionExpression)"(1))"));
        }

        [Fact]
        public void LogLevelWithoutAName()
        {
            Assert.Throws<ConditionParseException>(() => ((ConditionExpression)"LogLevel.'somestring'"));
        }

        [Fact]
        public void InvalidNumberWithUnaryMinusTest()
        {
            Assert.Throws<ConditionParseException>(() => ((ConditionExpression)"-a31"));
        }

        [Fact]
        public void InvalidNumberTest()
        {
            Assert.Throws<ConditionParseException>(() => ((ConditionExpression)"-123.4a"));
        }

        [Fact]
        public void UnclosedString()
        {
            Assert.Throws<ConditionParseException>(() => ((ConditionExpression)"'Hello world"));
        }

        [Fact]
        public void UnrecognizedToken()
        {
            Assert.Throws<ConditionParseException>(() => ((ConditionExpression)"somecompletelyunrecognizedtoken"));
        }

        [Fact]
        public void UnrecognizedPunctuation()
        {
            Assert.Throws<ConditionParseException>(() => ((ConditionExpression)"#"));
        }

        [Fact]
        public void UnrecognizedUnicodeChar()
        {
            Assert.Throws<ConditionParseException>(() => ((ConditionExpression)"\u0090"));
        }

        [Fact]
        public void UnrecognizedUnicodeChar2()
        {
            Assert.Throws<ConditionParseException>(() => ((ConditionExpression)"\u0015"));
        }

        [Fact]
        public void UnrecognizedMethod()
        {
            Assert.Throws<ConditionParseException>(() => ((ConditionExpression)"unrecognized-method()"));
        }

        [Fact]
        public void TokenizerEOFTest()
        {
            var tokenizer = new ConditionTokenizer(new SimpleStringReader(string.Empty));
            Assert.Throws<ConditionParseException>(() => tokenizer.GetNextToken());
        }

        private static void RelationalOperatorTestInner(string op, string result)
        {
            string operand1 = "3";
            string operand2 = "7";

            string input = operand1 + " " + op + " " + operand2;
            string expectedOutput = "(" + operand1 + " " + result + " " + operand2 + ")";
            ConditionExpression condition = input;
            Assert.Equal(expectedOutput, condition.ToString());
        }

        public class FooLayoutRenderer : LayoutRenderer
        {
            protected override void Append(System.Text.StringBuilder builder, LogEventInfo logEvent)
            {
                throw new System.NotImplementedException();
            }
        }

        public class MyConditionMethods
        {
            public static bool CheckIt(string s)
            {
                return s == "X";
            }
        }
    }
}
