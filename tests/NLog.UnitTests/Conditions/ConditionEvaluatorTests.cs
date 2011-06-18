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
    using System;
    using System.Globalization;
    using System.IO;
#if !NET_CF && !SILVERLIGHT
    using System.Runtime.Serialization.Formatters.Binary;
#endif
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

    [TestFixture]
    public class ConditionEvaluatorTests : NLogTestBase
    {
        [Test]
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

        [Test]
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

        [Test]
        public void ConditionMethodNegativeTest1()
        {
            try
            {
                AssertEvaluationResult(true, "starts-with('foobar')");
                Assert.Fail("Expected exception");
            }
            catch (ConditionParseException ex)
            {
                Assert.AreEqual("Cannot resolve function 'starts-with'", ex.Message);
                Assert.IsNotNull(ex.InnerException);
                Assert.AreEqual("Condition method 'starts-with' expects 2 parameters, but passed 1.", ex.InnerException.Message);
            }
        }

        [Test]
        public void ConditionMethodNegativeTest2()
        {
            try
            {
                AssertEvaluationResult(true, "starts-with('foobar','baz','qux','zzz')");
                Assert.Fail("Expected exception");
            }
            catch (ConditionParseException ex)
            {
                Assert.AreEqual("Cannot resolve function 'starts-with'", ex.Message);
                Assert.IsNotNull(ex.InnerException);
                Assert.AreEqual("Condition method 'starts-with' expects 2 parameters, but passed 4.", ex.InnerException.Message);
            }
        }

        [Test]
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

        [Test]
        public void LogEventInfoPropertiesTest()
        {
            AssertEvaluationResult(LogLevel.Warn, "level");
            AssertEvaluationResult("some message", "message");
            AssertEvaluationResult("MyCompany.Product.Class", "logger");
        }

        [Test]
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

        [Test]
        [ExpectedException(typeof(ConditionEvaluationException))]
        public void UnsupportedRelationalOperatorTest()
        {
            var cond = new ConditionRelationalExpression("true", "true", (ConditionRelationalOperator)(-1));
            cond.Evaluate(LogEventInfo.CreateNullEvent());
        }

        [Test]
        [ExpectedException(typeof(NotSupportedException))]
        public void UnsupportedRelationalOperatorTest2()
        {
            var cond = new ConditionRelationalExpression("true", "true", (ConditionRelationalOperator)(-1));
            cond.ToString();
        }

        [Test]
        public void MethodWithLogEventInfoTest()
        {
            var factories = SetupConditionMethods();
            Assert.AreEqual(true, ConditionParser.ParseExpression("IsValid()", factories).Evaluate(CreateWellKnownContext()));
        }

        [Test]
        public void TypePromotionTest()
        {
            var factories = SetupConditionMethods();

            Assert.AreEqual(true, ConditionParser.ParseExpression("ToDateTime('2010/01/01') == '2010/01/01'", factories).Evaluate(CreateWellKnownContext()));
            Assert.AreEqual(true, ConditionParser.ParseExpression("ToInt64(1) == ToInt32(1)", factories).Evaluate(CreateWellKnownContext()));
            Assert.AreEqual(true, ConditionParser.ParseExpression("'42' == 42", factories).Evaluate(CreateWellKnownContext()));
            Assert.AreEqual(true, ConditionParser.ParseExpression("42 == '42'", factories).Evaluate(CreateWellKnownContext()));
            Assert.AreEqual(true, ConditionParser.ParseExpression("ToDouble(3) == 3", factories).Evaluate(CreateWellKnownContext()));
            Assert.AreEqual(true, ConditionParser.ParseExpression("3 == ToDouble(3)", factories).Evaluate(CreateWellKnownContext()));
            Assert.AreEqual(true, ConditionParser.ParseExpression("ToSingle(3) == 3", factories).Evaluate(CreateWellKnownContext()));
            Assert.AreEqual(true, ConditionParser.ParseExpression("3 == ToSingle(3)", factories).Evaluate(CreateWellKnownContext()));
            Assert.AreEqual(true, ConditionParser.ParseExpression("ToDecimal(3) == 3", factories).Evaluate(CreateWellKnownContext()));
            Assert.AreEqual(true, ConditionParser.ParseExpression("3 == ToDecimal(3)", factories).Evaluate(CreateWellKnownContext()));
            Assert.AreEqual(true, ConditionParser.ParseExpression("ToInt32(3) == ToInt16(3)", factories).Evaluate(CreateWellKnownContext()));
            Assert.AreEqual(true, ConditionParser.ParseExpression("ToInt16(3) == ToInt32(3)", factories).Evaluate(CreateWellKnownContext()));
            Assert.AreEqual(true, ConditionParser.ParseExpression("true == ToInt16(1)", factories).Evaluate(CreateWellKnownContext()));
            Assert.AreEqual(true, ConditionParser.ParseExpression("ToInt16(1) == true", factories).Evaluate(CreateWellKnownContext()));

            Assert.AreEqual(false, ConditionParser.ParseExpression("ToDateTime('2010/01/01') == '2010/01/02'", factories).Evaluate(CreateWellKnownContext()));
            Assert.AreEqual(false, ConditionParser.ParseExpression("ToInt64(1) == ToInt32(2)", factories).Evaluate(CreateWellKnownContext()));
            Assert.AreEqual(false, ConditionParser.ParseExpression("'42' == 43", factories).Evaluate(CreateWellKnownContext()));
            Assert.AreEqual(false, ConditionParser.ParseExpression("42 == '43'", factories).Evaluate(CreateWellKnownContext()));
            Assert.AreEqual(false, ConditionParser.ParseExpression("ToDouble(3) == 4", factories).Evaluate(CreateWellKnownContext()));
            Assert.AreEqual(false, ConditionParser.ParseExpression("3 == ToDouble(4)", factories).Evaluate(CreateWellKnownContext()));
            Assert.AreEqual(false, ConditionParser.ParseExpression("ToSingle(3) == 4", factories).Evaluate(CreateWellKnownContext()));
            Assert.AreEqual(false, ConditionParser.ParseExpression("3 == ToSingle(4)", factories).Evaluate(CreateWellKnownContext()));
            Assert.AreEqual(false, ConditionParser.ParseExpression("ToDecimal(3) == 4", factories).Evaluate(CreateWellKnownContext()));
            Assert.AreEqual(false, ConditionParser.ParseExpression("3 == ToDecimal(4)", factories).Evaluate(CreateWellKnownContext()));
            Assert.AreEqual(false, ConditionParser.ParseExpression("ToInt32(3) == ToInt16(4)", factories).Evaluate(CreateWellKnownContext()));
            Assert.AreEqual(false, ConditionParser.ParseExpression("ToInt16(3) == ToInt32(4)", factories).Evaluate(CreateWellKnownContext()));
            Assert.AreEqual(false, ConditionParser.ParseExpression("false == ToInt16(4)", factories).Evaluate(CreateWellKnownContext()));
            Assert.AreEqual(false, ConditionParser.ParseExpression("ToInt16(1) == false", factories).Evaluate(CreateWellKnownContext()));
        }

        [Test]
        [ExpectedException(typeof(ConditionEvaluationException))]
        public void TypePromotionNegativeTest1()
        {
            var factories = SetupConditionMethods();

            Assert.AreEqual(true, ConditionParser.ParseExpression("ToDateTime('2010/01/01') == '20xx/01/01'", factories).Evaluate(CreateWellKnownContext()));
        }

        [Test]
        [ExpectedException(typeof(ConditionEvaluationException))]
        public void TypePromotionNegativeTest2()
        {
            var factories = SetupConditionMethods();

            Assert.AreEqual(true, ConditionParser.ParseExpression("GetGuid() == ToInt16(1)", factories).Evaluate(CreateWellKnownContext()));
        }

        [Test]
        public void ExceptionTest1()
        {
            var ex1 = new ConditionEvaluationException();
            Assert.IsNotNull(ex1.Message);
        }

        [Test]
        public void ExceptionTest2()
        {
            var ex1 = new ConditionEvaluationException("msg");
            Assert.AreEqual("msg", ex1.Message);
        }

        [Test]
        public void ExceptionTest3()
        {
            var inner = new InvalidOperationException("f");
            var ex1 = new ConditionEvaluationException("msg", inner);
            Assert.AreEqual("msg", ex1.Message);
            Assert.AreSame(inner, ex1.InnerException);
        }

#if !SILVERLIGHT && !NET_CF
        [Test]
        public void ExceptionTest4()
        {
            var inner = new InvalidOperationException("f");
            var ex1 = new ConditionEvaluationException("msg", inner);
            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            bf.Serialize(ms, ex1);
            ms.Position = 0;
            Exception ex2 = (Exception)bf.Deserialize(ms);

            Assert.AreEqual("msg", ex2.Message);
            Assert.AreEqual("f", ex2.InnerException.Message);
        }
#endif

        [Test]
        public void ExceptionTest11()
        {
            var ex1 = new ConditionParseException();
            Assert.IsNotNull(ex1.Message);
        }

        [Test]
        public void ExceptionTest12()
        {
            var ex1 = new ConditionParseException("msg");
            Assert.AreEqual("msg", ex1.Message);
        }

        [Test]
        public void ExceptionTest13()
        {
            var inner = new InvalidOperationException("f");
            var ex1 = new ConditionParseException("msg", inner);
            Assert.AreEqual("msg", ex1.Message);
            Assert.AreSame(inner, ex1.InnerException);
        }

#if !SILVERLIGHT && !NET_CF
        [Test]
        public void ExceptionTest14()
        {
            var inner = new InvalidOperationException("f");
            var ex1 = new ConditionParseException("msg", inner);
            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            bf.Serialize(ms, ex1);
            ms.Position = 0;
            Exception ex2 = (Exception)bf.Deserialize(ms);

            Assert.AreEqual("msg", ex2.Message);
            Assert.AreEqual("f", ex2.InnerException.Message);
        }
#endif

        private static ConfigurationItemFactory SetupConditionMethods()
        {
            var factories = new ConfigurationItemFactory();
            factories.ConditionMethods.RegisterDefinition("GetGuid", typeof(MyConditionMethods).GetMethod("GetGuid"));
            factories.ConditionMethods.RegisterDefinition("ToInt16", typeof(MyConditionMethods).GetMethod("ToInt16"));
            factories.ConditionMethods.RegisterDefinition("ToInt32", typeof(MyConditionMethods).GetMethod("ToInt32"));
            factories.ConditionMethods.RegisterDefinition("ToInt64", typeof(MyConditionMethods).GetMethod("ToInt64"));
            factories.ConditionMethods.RegisterDefinition("ToDouble", typeof(MyConditionMethods).GetMethod("ToDouble"));
            factories.ConditionMethods.RegisterDefinition("ToSingle", typeof(MyConditionMethods).GetMethod("ToSingle"));
            factories.ConditionMethods.RegisterDefinition("ToDateTime", typeof(MyConditionMethods).GetMethod("ToDateTime"));
            factories.ConditionMethods.RegisterDefinition("ToDecimal", typeof(MyConditionMethods).GetMethod("ToDecimal"));
            factories.ConditionMethods.RegisterDefinition("IsValid", typeof(MyConditionMethods).GetMethod("IsValid"));
            return factories;
        }

        private static void AssertEvaluationResult(object expectedResult, string conditionText)
        {
            ConditionExpression condition = ConditionParser.ParseExpression(conditionText);
            LogEventInfo context = CreateWellKnownContext();
            object actualResult = condition.Evaluate(context);
            Assert.AreEqual(expectedResult, actualResult);
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

        /// <summary>
        /// Conversion methods helpful in covering type promotion logic
        /// </summary>
        public class MyConditionMethods
        {
            public static Guid GetGuid()
            {
                return new Guid("{40190B01-C9C0-4F78-AA5A-615E413742E1}");
            }

            public static short ToInt16(object v)
            {
                return Convert.ToInt16(v, CultureInfo.InvariantCulture);
            }

            public static int ToInt32(object v)
            {
                return Convert.ToInt32(v, CultureInfo.InvariantCulture);
            }

            public static long ToInt64(object v)
            {
                return Convert.ToInt64(v, CultureInfo.InvariantCulture);
            }

            public static float ToSingle(object v)
            {
                return Convert.ToSingle(v, CultureInfo.InvariantCulture);
            }

            public static decimal ToDecimal(object v)
            {
                return Convert.ToDecimal(v, CultureInfo.InvariantCulture);
            }

            public static double ToDouble(object v)
            {
                return Convert.ToDouble(v, CultureInfo.InvariantCulture);
            }

            public static DateTime ToDateTime(object v)
            {
                return Convert.ToDateTime(v, CultureInfo.InvariantCulture);
            }

            public static bool IsValid(LogEventInfo context)
            {
                return true;
            }
        }
    }
}