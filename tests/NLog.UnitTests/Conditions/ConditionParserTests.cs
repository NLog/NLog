// 
// Copyright (c) 2004-2010 Jaroslaw Kowalski <jaak@jkowalski.net>
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
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using NLog.Conditions;
    using NLog.Config;
    using NLog.LayoutRenderers;
    using NLog.Layouts;

    [TestClass]
    public class ConditionParserTests : NLogTestBase
    {
        [TestMethod]
        public void ParseNullText()
        {
            Assert.IsNull(ConditionParser.ParseExpression(null));
        }

        [TestMethod]
        [ExpectedException(typeof(ConditionParseException))]
        public void ParseEmptyText()
        {
            ConditionParser.ParseExpression("");
        }

        [TestMethod]
        public void ImplicitOperatorTest()
        {
            ConditionExpression cond = "true and true";

            Assert.IsInstanceOfType(cond, typeof(ConditionAndExpression));
        }

        [TestMethod]
        public void NullLiteralTest()
        {
            Assert.AreEqual("null", ConditionParser.ParseExpression("null").ToString());
        }

        [TestMethod]
        public void BooleanLiteralTest()
        {
            Assert.AreEqual("True", ConditionParser.ParseExpression("true").ToString());
            Assert.AreEqual("True", ConditionParser.ParseExpression("tRuE").ToString());
            Assert.AreEqual("False", ConditionParser.ParseExpression("false").ToString());
            Assert.AreEqual("False", ConditionParser.ParseExpression("fAlSe").ToString());
        }

        [TestMethod]
        public void AndTest()
        {
            Assert.AreEqual("(True and True)", ConditionParser.ParseExpression("true and true").ToString());
            Assert.AreEqual("(True and True)", ConditionParser.ParseExpression("tRuE AND true").ToString());
            Assert.AreEqual("(True and True)", ConditionParser.ParseExpression("tRuE && true").ToString());
            Assert.AreEqual("((True and True) and True)", ConditionParser.ParseExpression("true and true && true").ToString());
            Assert.AreEqual("((True and True) and True)", ConditionParser.ParseExpression("tRuE AND true and true").ToString());
            Assert.AreEqual("((True and True) and True)", ConditionParser.ParseExpression("tRuE && true AND true").ToString());
        }

        [TestMethod]
        public void OrTest()
        {
            Assert.AreEqual("(True or True)", ConditionParser.ParseExpression("true or true").ToString());
            Assert.AreEqual("(True or True)", ConditionParser.ParseExpression("tRuE OR true").ToString());
            Assert.AreEqual("(True or True)", ConditionParser.ParseExpression("tRuE || true").ToString());
            Assert.AreEqual("((True or True) or True)", ConditionParser.ParseExpression("true or true || true").ToString());
            Assert.AreEqual("((True or True) or True)", ConditionParser.ParseExpression("tRuE OR true or true").ToString());
            Assert.AreEqual("((True or True) or True)", ConditionParser.ParseExpression("tRuE || true OR true").ToString());
        }

        [TestMethod]
        public void NotTest()
        {
            Assert.AreEqual("(not True)", ConditionParser.ParseExpression("not true").ToString());
            Assert.AreEqual("(not (not True))", ConditionParser.ParseExpression("not not true").ToString());
            Assert.AreEqual("(not (not (not True)))", ConditionParser.ParseExpression("not not not true").ToString());
        }

        [TestMethod]
        public void StringTest()
        {
            Assert.AreEqual("''", ConditionParser.ParseExpression("''").ToString());
            Assert.AreEqual("'Foo'", ConditionParser.ParseExpression("'Foo'").ToString());
            Assert.AreEqual("'Bar'", ConditionParser.ParseExpression("'Bar'").ToString());
            Assert.AreEqual("'d'Artagnan'", ConditionParser.ParseExpression("'d''Artagnan'").ToString());

            var cle = ConditionParser.ParseExpression("'${message} ${level}'") as ConditionLayoutExpression;
            Assert.IsNotNull(cle);
            SimpleLayout sl = cle.Layout as SimpleLayout;
            Assert.IsNotNull(sl);
            Assert.AreEqual(3, sl.Renderers.Count);
            Assert.IsInstanceOfType(sl.Renderers[0], typeof(MessageLayoutRenderer));
            Assert.IsInstanceOfType(sl.Renderers[1], typeof(LiteralLayoutRenderer));
            Assert.IsInstanceOfType(sl.Renderers[2], typeof(LevelLayoutRenderer));

        }

        [TestMethod]
        public void LogLevelTest()
        {
            var result = ConditionParser.ParseExpression("LogLevel.Info") as ConditionLiteralExpression;
            Assert.IsNotNull(result);
            Assert.AreSame(LogLevel.Info, result.LiteralValue);

            result = ConditionParser.ParseExpression("LogLevel.Trace") as ConditionLiteralExpression;
            Assert.IsNotNull(result);
            Assert.AreSame(LogLevel.Trace, result.LiteralValue);
        }

        [TestMethod]
        public void RelationalOperatorTest()
        {
            RelationalOperatorTest("=", "==");
            RelationalOperatorTest("==", "==");
            RelationalOperatorTest("!=", "!=");
            RelationalOperatorTest("<>", "!=");
            RelationalOperatorTest("<", "<");
            RelationalOperatorTest(">", ">");
            RelationalOperatorTest("<=", "<=");
            RelationalOperatorTest(">=", ">=");
        }

        [TestMethod]
        public void NumberTest()
        {
            Assert.AreEqual("3.141592", ConditionParser.ParseExpression("3.141592").ToString());
            Assert.AreEqual("42", ConditionParser.ParseExpression("42").ToString());
            Assert.AreEqual("-42", ConditionParser.ParseExpression("-42").ToString());
            Assert.AreEqual("-3.141592", ConditionParser.ParseExpression("-3.141592").ToString());
        }

        [TestMethod]
        public void ExtraParenthesisTest()
        {
            Assert.AreEqual("3.141592", ConditionParser.ParseExpression("(((3.141592)))").ToString());
        }

        [TestMethod]
        public void MessageTest()
        {
            var result = ConditionParser.ParseExpression("message");
            Assert.IsInstanceOfType(result, typeof(ConditionMessageExpression));
            Assert.AreEqual("message", result.ToString());
        }

        [TestMethod]
        public void LevelTest()
        {
            var result = ConditionParser.ParseExpression("level");
            Assert.IsInstanceOfType(result, typeof(ConditionLevelExpression));
            Assert.AreEqual("level", result.ToString());
        }

        [TestMethod]
        public void LoggerTest()
        {
            var result = ConditionParser.ParseExpression("logger");
            Assert.IsInstanceOfType(result, typeof(ConditionLoggerNameExpression));
            Assert.AreEqual("logger", result.ToString());
        }

        [TestMethod]
        public void ConditionFunctionTests()
        {
            var result = ConditionParser.ParseExpression("starts-with(logger, 'x${message}')") as ConditionMethodExpression;
            Assert.IsNotNull(result);
            Assert.AreEqual("starts-with(logger, 'x${message}')", result.ToString());
            Assert.AreEqual("StartsWith", result.MethodInfo.Name);
            Assert.AreEqual(typeof(ConditionMethods), result.MethodInfo.DeclaringType);
        }

        [TestMethod]
        public void CustomNLogFactoriesTest()
        {
            var configurationItemFactory = new ConfigurationItemFactory();
            configurationItemFactory.LayoutRenderers.RegisterDefinition("foo", typeof(FooLayoutRenderer));
            configurationItemFactory.ConditionMethods.RegisterDefinition("check", typeof(MyConditionMethods).GetMethod("CheckIt"));

            ConditionParser.ParseExpression("check('${foo}')", configurationItemFactory);
        }

        [TestMethod]
        public void MethodNameWithUnderscores()
        {
            var configurationItemFactory = new ConfigurationItemFactory();
            configurationItemFactory.LayoutRenderers.RegisterDefinition("foo", typeof(FooLayoutRenderer));
            configurationItemFactory.ConditionMethods.RegisterDefinition("__check__", typeof(MyConditionMethods).GetMethod("CheckIt"));

            ConditionParser.ParseExpression("__check__('${foo}')", configurationItemFactory);
        }

        [TestMethod]
        [ExpectedException(typeof(ConditionParseException))]
        public void UnbalancedParenthesis1Test()
        {
            ConditionParser.ParseExpression("check(");
        }

        [TestMethod]
        [ExpectedException(typeof(ConditionParseException))]
        public void UnbalancedParenthesis2Test()
        {
            ConditionParser.ParseExpression("((1)");
        }

        [TestMethod]
        [ExpectedException(typeof(ConditionParseException))]
        public void UnbalancedParenthesis3Test()
        {
            ConditionParser.ParseExpression("(1))");
        }

        [TestMethod]
        [ExpectedException(typeof(ConditionParseException))]
        public void LogLevelWithoutAName()
        {
            ConditionParser.ParseExpression("LogLevel.'somestring'");
        }

        [TestMethod]
        [ExpectedException(typeof(ConditionParseException))]
        public void InvalidNumberWithUnaryMinusTest()
        {
            ConditionParser.ParseExpression("-a31");
        }

        [TestMethod]
        [ExpectedException(typeof(ConditionParseException))]
        public void InvalidNumberTest()
        {
            ConditionParser.ParseExpression("-123.4a");
        }

        [TestMethod]
        [ExpectedException(typeof(ConditionParseException))]
        public void UnclosedString()
        {
            ConditionParser.ParseExpression("'Hello world");
        }

        [TestMethod]
        [ExpectedException(typeof(ConditionParseException))]
        public void UnrecognizedToken()
        {
            ConditionParser.ParseExpression("somecompletelyunrecognizedtoken");
        }

        [TestMethod]
        [ExpectedException(typeof(ConditionParseException))]
        public void UnrecognizedPunctuation()
        {
            ConditionParser.ParseExpression("#");
        }

        [TestMethod]
        [ExpectedException(typeof(ConditionParseException))]
        public void UnrecognizedUnicodeChar()
        {
            ConditionParser.ParseExpression("\u0090");
        }

        [TestMethod]
        [ExpectedException(typeof(ConditionParseException))]
        public void UnrecognizedUnicodeChar2()
        {
            ConditionParser.ParseExpression("\u0015");
        }

        [TestMethod]
        [ExpectedException(typeof(ConditionParseException))]
        public void UnrecognizedMethod()
        {
            ConditionParser.ParseExpression("unrecognized-method()");
        }

        [TestMethod]
        [ExpectedException(typeof(ConditionParseException))]
        public void TokenizerEOFTest()
        {
            var tokenizer = new ConditionTokenizer(string.Empty);
            tokenizer.GetNextToken();
        }

        private void RelationalOperatorTest(string op, string result)
        {
            string operand1 = "3";
            string operand2 = "7";

            string input = operand1 + " " + op + " " + operand2;
            string expectedOutput = "(" + operand1 + " " + result + " " + operand2 + ")";
            var condition = ConditionParser.ParseExpression(input);
            Assert.AreEqual(expectedOutput, condition.ToString());
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