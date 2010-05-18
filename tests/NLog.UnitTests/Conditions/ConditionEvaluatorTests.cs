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

using System.Xml;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NLog.Config;
using NLog.Conditions;
using NLog.Layouts;
using NLog.LayoutRenderers;

namespace NLog.UnitTests.Conditions
{
    [TestClass]
    public class ConditionEvaluatorTests : NLogTestBase
    {
        [TestMethod]
        public void LiteralTest()
        {
            AssertEvaluationResult(null, "null");
            AssertEvaluationResult(0, "0");
            AssertEvaluationResult(3, "3");
            AssertEvaluationResult(3.1415, "3.1415");
            AssertEvaluationResult(-1, "-1");
            AssertEvaluationResult(-3.1415, "-3.1415");
            AssertEvaluationResult(true, "true");
            AssertEvaluationResult(false, "false");
            AssertEvaluationResult(string.Empty, "''");
            AssertEvaluationResult("x", "'x'");
            AssertEvaluationResult("d'Artagnan", "'d''Artagnan'");
        }

        [TestMethod]
        public void LogEventInfoPropertiesTest()
        {
            AssertEvaluationResult(LogLevel.Warn, "level");
            AssertEvaluationResult("some message", "message");
            AssertEvaluationResult("MyCompany.Product.Class", "logger");
        }

        [TestMethod]
        public void BooleanOperatorTest()
        {
            AssertEvaluationResult(false, "false or false");
            AssertEvaluationResult(true, "false or true");
            AssertEvaluationResult(true, "true or false");
            AssertEvaluationResult(true, "true or true");
            AssertEvaluationResult(false, "false and false");
            AssertEvaluationResult(false, "false and true");
            AssertEvaluationResult(false, "true and false");
            AssertEvaluationResult(true, "true and true");
            AssertEvaluationResult(false, "not true");
            AssertEvaluationResult(true, "not false");
            AssertEvaluationResult(false, "not not false");
            AssertEvaluationResult(true, "not not true");
        }

        [TestMethod]
        public void ConditionMethodsTest()
        {
            AssertEvaluationResult(true, "starts-with('foobar','foo')");
            AssertEvaluationResult(false, "starts-with('foobar','bar')");
            AssertEvaluationResult(true, "ends-with('foobar','bar')");
            AssertEvaluationResult(false, "ends-with('foobar','foo')");
            AssertEvaluationResult(0, "length('')");
            AssertEvaluationResult(4, "length('${level}')");
            AssertEvaluationResult(false, "equals(1, 2)");
            AssertEvaluationResult(true, "equals(3.14, 3.14)");
            AssertEvaluationResult(true, "contains('foobar','ooba')");
            AssertEvaluationResult(false, "contains('foobar','oobe')");
            AssertEvaluationResult(false, "contains('','foo')");
            AssertEvaluationResult(true, "contains('foo','')");
        }

        [TestMethod]
        public void RelationalOperatorTest()
        {
            AssertEvaluationResult(true, "1 < 2");
            AssertEvaluationResult(false, "1 < 1");

            AssertEvaluationResult(true, "2 > 1");
            AssertEvaluationResult(false, "1 > 1");

            AssertEvaluationResult(true, "1 <= 2");
            AssertEvaluationResult(false, "1 <= 0");

            AssertEvaluationResult(true, "2 >= 1");
            AssertEvaluationResult(false, "0 >= 1");

            AssertEvaluationResult(true, "2 == 2");
            AssertEvaluationResult(false, "2 == 3");

            AssertEvaluationResult(true, "2 != 3");
            AssertEvaluationResult(false, "2 != 2");

            AssertEvaluationResult(false, "1 < null");
            AssertEvaluationResult(true, "1 > null");

            AssertEvaluationResult(true, "null < 1");
            AssertEvaluationResult(false, "null > 1");

            AssertEvaluationResult(true, "null == null");
            AssertEvaluationResult(false, "null != null");

            AssertEvaluationResult(false, "null == 1");
            AssertEvaluationResult(false, "1 == null");

            AssertEvaluationResult(true, "null != 1");
            AssertEvaluationResult(true, "1 != null");
        }

        private void AssertEvaluationResult(object expectedResult, string conditionText)
        {
            var condition = ConditionParser.ParseExpression(conditionText);
            var context = CreateWellKnownContext();
            object actualResult = condition.Evaluate(context);
            Assert.AreEqual(expectedResult, actualResult);
        }

        private LogEventInfo CreateWellKnownContext()
        {
            LogEventInfo context = new LogEventInfo();
            context.Level = LogLevel.Warn;
            context.Message = "some message";
            context.LoggerName = "MyCompany.Product.Class";

            return context;
        }
    }
}