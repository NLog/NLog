// 
// Copyright (c) 2004-2011 Jaroslaw Kowalski <jaak@jkowalski.net>
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
    using NUnit.Framework;

#if !NUNIT
    using SetUp = Microsoft.VisualStudio.TestTools.UnitTesting.TestInitializeAttribute;
    using TestFixture = Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute;
    using Test = Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute;
    using TearDown =  Microsoft.VisualStudio.TestTools.UnitTesting.TestCleanupAttribute;
    using ExpectedException = Microsoft.VisualStudio.TestTools.UnitTesting.ExpectedExceptionAttribute;
#endif
    using NLog.Conditions;
    using NLog.Config;
    using NLog.LayoutRenderers;
    using NLog.Layouts;

    [TestFixture]
    public class ConditionParserTests : NLogTestBase
    {
        [Test]
        public void ParseNullText()
        {
            Assert.IsNull(ConditionParser.ParseExpression(null));
        }

        [Test]
        [ExpectedException(typeof(ConditionParseException))]
        public void ParseEmptyText()
        {
            ConditionParser.ParseExpression("");
        }

        [Test]
        public void ImplicitOperatorTest()
        {
            ConditionExpression cond = "true and true";

            Assert.IsInstanceOfType(typeof(ConditionAndExpression), cond);
        }

        [Test]
        public void NullLiteralTest()
        {
            Assert.AreEqual("null", ConditionParser.ParseExpression("null").ToString());
        }

        [Test]
        public void BooleanLiteralTest()
        {
            Assert.AreEqual("True", ConditionParser.ParseExpression("true").ToString());
            Assert.AreEqual("True", ConditionParser.ParseExpression("tRuE").ToString());
            Assert.AreEqual("False", ConditionParser.ParseExpression("false").ToString());
            Assert.AreEqual("False", ConditionParser.ParseExpression("fAlSe").ToString());
        }

        [Test]
        public void AndTest()
        {
            Assert.AreEqual("(True and True)", ConditionParser.ParseExpression("true and true").ToString());
            Assert.AreEqual("(True and True)", ConditionParser.ParseExpression("tRuE AND true").ToString());
            Assert.AreEqual("(True and True)", ConditionParser.ParseExpression("tRuE && true").ToString());
            Assert.AreEqual("((True and True) and True)", ConditionParser.ParseExpression("true and true && true").ToString());
            Assert.AreEqual("((True and True) and True)", ConditionParser.ParseExpression("tRuE AND true and true").ToString());
            Assert.AreEqual("((True and True) and True)", ConditionParser.ParseExpression("tRuE && true AND true").ToString());
        }

        [Test]
        public void OrTest()
        {
            Assert.AreEqual("(True or True)", ConditionParser.ParseExpression("true or true").ToString());
            Assert.AreEqual("(True or True)", ConditionParser.ParseExpression("tRuE OR true").ToString());
            Assert.AreEqual("(True or True)", ConditionParser.ParseExpression("tRuE || true").ToString());
            Assert.AreEqual("((True or True) or True)", ConditionParser.ParseExpression("true or true || true").ToString());
            Assert.AreEqual("((True or True) or True)", ConditionParser.ParseExpression("tRuE OR true or true").ToString());
            Assert.AreEqual("((True or True) or True)", ConditionParser.ParseExpression("tRuE || true OR true").ToString());
        }

        [Test]
        public void NotTest()
        {
            Assert.AreEqual("(not True)", ConditionParser.ParseExpression("not true").ToString());
            Assert.AreEqual("(not (not True))", ConditionParser.ParseExpression("not not true").ToString());
            Assert.AreEqual("(not (not (not True)))", ConditionParser.ParseExpression("not not not true").ToString());
        }

        [Test]
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
            Assert.IsInstanceOfType(typeof(MessageLayoutRenderer), sl.Renderers[0]);
            Assert.IsInstanceOfType(typeof(LiteralLayoutRenderer), sl.Renderers[1]);
            Assert.IsInstanceOfType(typeof(LevelLayoutRenderer), sl.Renderers[2]);

        }

        [Test]
        public void LogLevelTest()
        {
            var result = ConditionParser.ParseExpression("LogLevel.Info") as ConditionLiteralExpression;
            Assert.IsNotNull(result);
            Assert.AreSame(LogLevel.Info, result.LiteralValue);

            result = ConditionParser.ParseExpression("LogLevel.Trace") as ConditionLiteralExpression;
            Assert.IsNotNull(result);
            Assert.AreSame(LogLevel.Trace, result.LiteralValue);
        }

        [Test]
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

        [Test]
        public void NumberTest()
        {
            Assert.AreEqual("3.141592", ConditionParser.ParseExpression("3.141592").ToString());
            Assert.AreEqual("42", ConditionParser.ParseExpression("42").ToString());
            Assert.AreEqual("-42", ConditionParser.ParseExpression("-42").ToString());
            Assert.AreEqual("-3.141592", ConditionParser.ParseExpression("-3.141592").ToString());
        }

        [Test]
        public void ExtraParenthesisTest()
        {
            Assert.AreEqual("3.141592", ConditionParser.ParseExpression("(((3.141592)))").ToString());
        }

        [Test]
        public void MessageTest()
        {
            var result = ConditionParser.ParseExpression("message");
            Assert.IsInstanceOfType(typeof(ConditionMessageExpression), result);
            Assert.AreEqual("message", result.ToString());
        }

        [Test]
        public void LevelTest()
        {
            var result = ConditionParser.ParseExpression("level");
            Assert.IsInstanceOfType(typeof(ConditionLevelExpression), result);
            Assert.AreEqual("level", result.ToString());
        }

        [Test]
        public void LoggerTest()
        {
            var result = ConditionParser.ParseExpression("logger");
            Assert.IsInstanceOfType(typeof(ConditionLoggerNameExpression), result);
            Assert.AreEqual("logger", result.ToString());
        }

        [Test]
        public void ConditionFunctionTests()
        {
            var result = ConditionParser.ParseExpression("starts-with(logger, 'x${message}')") as ConditionMethodExpression;
            Assert.IsNotNull(result);
            Assert.AreEqual("starts-with(logger, 'x${message}')", result.ToString());
            Assert.AreEqual("StartsWith", result.MethodInfo.Name);
            Assert.AreEqual(typeof(ConditionMethods), result.MethodInfo.DeclaringType);
        }

        [Test]
        public void CustomNLogFactoriesTest()
        {
            var configurationItemFactory = new ConfigurationItemFactory();
            configurationItemFactory.LayoutRenderers.RegisterDefinition("foo", typeof(FooLayoutRenderer));
            configurationItemFactory.ConditionMethods.RegisterDefinition("check", typeof(MyConditionMethods).GetMethod("CheckIt"));

            ConditionParser.ParseExpression("check('${foo}')", configurationItemFactory);
        }

        [Test]
        public void MethodNameWithUnderscores()
        {
            var configurationItemFactory = new ConfigurationItemFactory();
            configurationItemFactory.LayoutRenderers.RegisterDefinition("foo", typeof(FooLayoutRenderer));
            configurationItemFactory.ConditionMethods.RegisterDefinition("__check__", typeof(MyConditionMethods).GetMethod("CheckIt"));

            ConditionParser.ParseExpression("__check__('${foo}')", configurationItemFactory);
        }

        [Test]
        [ExpectedException(typeof(ConditionParseException))]
        public void UnbalancedParenthesis1Test()
        {
            ConditionParser.ParseExpression("check(");
        }

        [Test]
        [ExpectedException(typeof(ConditionParseException))]
        public void UnbalancedParenthesis2Test()
        {
            ConditionParser.ParseExpression("((1)");
        }

        [Test]
        [ExpectedException(typeof(ConditionParseException))]
        public void UnbalancedParenthesis3Test()
        {
            ConditionParser.ParseExpression("(1))");
        }

        [Test]
        [ExpectedException(typeof(ConditionParseException))]
        public void LogLevelWithoutAName()
        {
            ConditionParser.ParseExpression("LogLevel.'somestring'");
        }

        [Test]
        [ExpectedException(typeof(ConditionParseException))]
        public void InvalidNumberWithUnaryMinusTest()
        {
            ConditionParser.ParseExpression("-a31");
        }

        [Test]
        [ExpectedException(typeof(ConditionParseException))]
        public void InvalidNumberTest()
        {
            ConditionParser.ParseExpression("-123.4a");
        }

        [Test]
        [ExpectedException(typeof(ConditionParseException))]
        public void UnclosedString()
        {
            ConditionParser.ParseExpression("'Hello world");
        }

        [Test]
        [ExpectedException(typeof(ConditionParseException))]
        public void UnrecognizedToken()
        {
            ConditionParser.ParseExpression("somecompletelyunrecognizedtoken");
        }

        [Test]
        [ExpectedException(typeof(ConditionParseException))]
        public void UnrecognizedPunctuation()
        {
            ConditionParser.ParseExpression("#");
        }

        [Test]
        [ExpectedException(typeof(ConditionParseException))]
        public void UnrecognizedUnicodeChar()
        {
            ConditionParser.ParseExpression("\u0090");
        }

        [Test]
        [ExpectedException(typeof(ConditionParseException))]
        public void UnrecognizedUnicodeChar2()
        {
            ConditionParser.ParseExpression("\u0015");
        }

        [Test]
        [ExpectedException(typeof(ConditionParseException))]
        public void UnrecognizedMethod()
        {
            ConditionParser.ParseExpression("unrecognized-method()");
        }

        [Test]
        [ExpectedException(typeof(ConditionParseException))]
        public void TokenizerEOFTest()
        {
            var tokenizer = new ConditionTokenizer(new SimpleStringReader(string.Empty));
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