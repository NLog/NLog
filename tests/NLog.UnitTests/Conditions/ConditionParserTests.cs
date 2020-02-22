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

namespace NLog.UnitTests.Conditions
{
    using NLog.Internal;
    using NLog.Conditions;
    using NLog.Config;
    using NLog.LayoutRenderers;
    using NLog.Layouts;
    using Xunit;

    public class ConditionParserTests : NLogTestBase
    {
        [Fact]
        public void ParseNullText()
        {
            Assert.Null(ConditionParser.ParseExpression(null));
        }

        [Fact]
        public void ParseEmptyText()
        {
            Assert.Throws<ConditionParseException>(() => ConditionParser.ParseExpression(""));
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
            Assert.Equal("null", ConditionParser.ParseExpression("null").ToString());
        }

        [Fact]
        public void BooleanLiteralTest()
        {
            Assert.Equal("True", ConditionParser.ParseExpression("true").ToString());
            Assert.Equal("True", ConditionParser.ParseExpression("tRuE").ToString());
            Assert.Equal("False", ConditionParser.ParseExpression("false").ToString());
            Assert.Equal("False", ConditionParser.ParseExpression("fAlSe").ToString());
        }

        [Fact]
        public void AndTest()
        {
            Assert.Equal("(True and True)", ConditionParser.ParseExpression("true and true").ToString());
            Assert.Equal("(True and True)", ConditionParser.ParseExpression("tRuE AND true").ToString());
            Assert.Equal("(True and True)", ConditionParser.ParseExpression("tRuE && true").ToString());
            Assert.Equal("((True and True) and True)", ConditionParser.ParseExpression("true and true && true").ToString());
            Assert.Equal("((True and True) and True)", ConditionParser.ParseExpression("tRuE AND true and true").ToString());
            Assert.Equal("((True and True) and True)", ConditionParser.ParseExpression("tRuE && true AND true").ToString());
        }

        [Fact]
        public void OrTest()
        {
            Assert.Equal("(True or True)", ConditionParser.ParseExpression("true or true").ToString());
            Assert.Equal("(True or True)", ConditionParser.ParseExpression("tRuE OR true").ToString());
            Assert.Equal("(True or True)", ConditionParser.ParseExpression("tRuE || true").ToString());
            Assert.Equal("((True or True) or True)", ConditionParser.ParseExpression("true or true || true").ToString());
            Assert.Equal("((True or True) or True)", ConditionParser.ParseExpression("tRuE OR true or true").ToString());
            Assert.Equal("((True or True) or True)", ConditionParser.ParseExpression("tRuE || true OR true").ToString());
        }

        [Fact]
        public void NotTest()
        {
            Assert.Equal("(not True)", ConditionParser.ParseExpression("not true").ToString());
            Assert.Equal("(not (not True))", ConditionParser.ParseExpression("not not true").ToString());
            Assert.Equal("(not (not (not True)))", ConditionParser.ParseExpression("not not not true").ToString());
        }

        [Fact]
        public void StringTest()
        {
            Assert.Equal("''", ConditionParser.ParseExpression("''").ToString());
            Assert.Equal("'Foo'", ConditionParser.ParseExpression("'Foo'").ToString());
            Assert.Equal("'Bar'", ConditionParser.ParseExpression("'Bar'").ToString());
            Assert.Equal("'d'Artagnan'", ConditionParser.ParseExpression("'d''Artagnan'").ToString());

            var cle = ConditionParser.ParseExpression("'${message} ${level}'") as ConditionLayoutExpression;
            Assert.NotNull(cle);
            SimpleLayout sl = cle.Layout as SimpleLayout;
            Assert.NotNull(sl);
            Assert.Equal(3, sl.Renderers.Count);
            Assert.IsType<MessageLayoutRenderer>(sl.Renderers[0]);
            Assert.IsType<LiteralLayoutRenderer>(sl.Renderers[1]);
            Assert.IsType<LevelLayoutRenderer>(sl.Renderers[2]);

        }

        [Fact]
        public void LogLevelTest()
        {
            var result = ConditionParser.ParseExpression("LogLevel.Info") as ConditionLiteralExpression;
            Assert.NotNull(result);
            Assert.Same(LogLevel.Info, result.LiteralValue);

            result = ConditionParser.ParseExpression("LogLevel.Trace") as ConditionLiteralExpression;
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
            var conditionExpression = ConditionParser.ParseExpression("3.141592");
            Assert.Equal("3.141592", conditionExpression.ToString());
            Assert.Equal("42", ConditionParser.ParseExpression("42").ToString());
            Assert.Equal("-42", ConditionParser.ParseExpression("-42").ToString());
            Assert.Equal("-3.141592", ConditionParser.ParseExpression("-3.141592").ToString());
        }

        [Fact]
        public void ExtraParenthesisTest()
        {
            Assert.Equal("3.141592", ConditionParser.ParseExpression("(((3.141592)))").ToString());
        }

        [Fact]
        public void MessageTest()
        {
            var result = ConditionParser.ParseExpression("message");
            Assert.IsType<ConditionMessageExpression>(result);
            Assert.Equal("message", result.ToString());
        }

        [Fact]
        public void LevelTest()
        {
            var result = ConditionParser.ParseExpression("level");
            Assert.IsType<ConditionLevelExpression>(result);
            Assert.Equal("level", result.ToString());
        }

        [Fact]
        public void LoggerTest()
        {
            var result = ConditionParser.ParseExpression("logger");
            Assert.IsType<ConditionLoggerNameExpression>(result);
            Assert.Equal("logger", result.ToString());
        }

        [Fact]
        public void ConditionFunctionTests()
        {
            var result = ConditionParser.ParseExpression("starts-with(logger, 'x${message}')") as ConditionMethodExpression;
            Assert.NotNull(result);
            Assert.Equal("starts-with(logger, 'x${message}')", result.ToString());
            Assert.Equal("StartsWith", result.MethodInfo.Name);
            Assert.Equal(typeof(ConditionMethods), result.MethodInfo.DeclaringType);
        }

        [Fact]
        public void CustomNLogFactoriesTest()
        {
            var configurationItemFactory = new ConfigurationItemFactory();
            configurationItemFactory.LayoutRenderers.RegisterDefinition("foo", typeof(FooLayoutRenderer));
            configurationItemFactory.ConditionMethods.RegisterDefinition("check", typeof(MyConditionMethods).GetMethod("CheckIt"));

            ConditionParser.ParseExpression("check('${foo}')", configurationItemFactory);
        }

        [Fact]
        public void MethodNameWithUnderscores()
        {
            var configurationItemFactory = new ConfigurationItemFactory();
            configurationItemFactory.LayoutRenderers.RegisterDefinition("foo", typeof(FooLayoutRenderer));
            configurationItemFactory.ConditionMethods.RegisterDefinition("__check__", typeof(MyConditionMethods).GetMethod("CheckIt"));

            ConditionParser.ParseExpression("__check__('${foo}')", configurationItemFactory);
        }

        [Fact]
        public void UnbalancedParenthesis1Test()
        {
            Assert.Throws<ConditionParseException>(() => ConditionParser.ParseExpression("check("));
        }

        [Fact]
        public void UnbalancedParenthesis2Test()
        {
            Assert.Throws<ConditionParseException>(() => ConditionParser.ParseExpression("((1)"));
        }

        [Fact]
        public void UnbalancedParenthesis3Test()
        {
            Assert.Throws<ConditionParseException>(() => ConditionParser.ParseExpression("(1))"));
        }

        [Fact]
        public void LogLevelWithoutAName()
        {
            Assert.Throws<ConditionParseException>(() => ConditionParser.ParseExpression("LogLevel.'somestring'"));
        }

        [Fact]
        public void InvalidNumberWithUnaryMinusTest()
        {
            Assert.Throws<ConditionParseException>(() => ConditionParser.ParseExpression("-a31"));
        }

        [Fact]
        public void InvalidNumberTest()
        {
            Assert.Throws<ConditionParseException>(() => ConditionParser.ParseExpression("-123.4a"));
        }

        [Fact]
        public void UnclosedString()
        {
            Assert.Throws<ConditionParseException>(() => ConditionParser.ParseExpression("'Hello world"));
        }

        [Fact]
        public void UnrecognizedToken()
        {
            Assert.Throws<ConditionParseException>(() => ConditionParser.ParseExpression("somecompletelyunrecognizedtoken"));
        }

        [Fact]
        public void UnrecognizedPunctuation()
        {
            Assert.Throws<ConditionParseException>(() => ConditionParser.ParseExpression("#"));
        }

        [Fact]
        public void UnrecognizedUnicodeChar()
        {
            Assert.Throws<ConditionParseException>(() => ConditionParser.ParseExpression("\u0090"));
        }

        [Fact]
        public void UnrecognizedUnicodeChar2()
        {
            Assert.Throws<ConditionParseException>(() => ConditionParser.ParseExpression("\u0015"));
        }

        [Fact]
        public void UnrecognizedMethod()
        {
            Assert.Throws<ConditionParseException>(() => ConditionParser.ParseExpression("unrecognized-method()"));
        }

        [Fact]
        public void TokenizerEOFTest()
        {
            var tokenizer = new ConditionTokenizer(new SimpleStringReader(string.Empty));
            Assert.Throws<ConditionParseException>(() => tokenizer.GetNextToken());
        }

        private void RelationalOperatorTestInner(string op, string result)
        {
            string operand1 = "3";
            string operand2 = "7";

            string input = operand1 + " " + op + " " + operand2;
            string expectedOutput = "(" + operand1 + " " + result + " " + operand2 + ")";
            var condition = ConditionParser.ParseExpression(input);
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