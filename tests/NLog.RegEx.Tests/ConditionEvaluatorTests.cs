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
    using NLog.Conditions;
    using Xunit;

    public class ConditionEvaluatorTests 
    {
        public ConditionEvaluatorTests()
        {
            LogManager.ThrowExceptions = true;
            LogManager.Setup().SetupExtensions(ext => ext.RegisterAssembly(typeof(Conditions.RegexConditionMethods).Assembly));
        }

        [Fact]
        public void ConditionMethodsTest()
        {
            AssertEvaluationResult(true, "regex-matches('foo', '^foo$')");
            AssertEvaluationResult(false, "regex-matches('foo', '^bar$')");

            //Check that calling with empty string is equivalent with not passing the parameter
            AssertEvaluationResult(true, "regex-matches('foo', '^foo$', '')");
            AssertEvaluationResult(false, "regex-matches('foo', '^bar$', '')");

            //Check that options are parsed correctly
            AssertEvaluationResult(true, "regex-matches('Foo', '^foo$', 'ignorecase')");
            AssertEvaluationResult(false, "regex-matches('Foo', '^foo$')");
            AssertEvaluationResult(true, "regex-matches('foo\nbar', '^Foo$', 'ignorecase,multiline')");
            AssertEvaluationResult(false, "regex-matches('foo\nbar', '^Foo$')");
            Assert.Throws<ConditionEvaluationException>(() => AssertEvaluationResult(true, "regex-matches('foo\nbar', '^Foo$', 'ignorecase,nonexistent')"));
        }

        private static void AssertEvaluationResult(object expectedResult, string conditionText)
        {
            ConditionExpression condition = ConditionParser.ParseExpression(conditionText);
            LogEventInfo context = CreateWellKnownContext();
            object actualResult = condition.Evaluate(context);
            Assert.Equal(expectedResult, actualResult);
        }

        private static LogEventInfo CreateWellKnownContext()
        {
            var context = new LogEventInfo
            {
                Level = LogLevel.Warn,
                Message = "some message",
                LoggerName = "MyCompany.Product.Class"
            };

            return context;
        }
    }
}
