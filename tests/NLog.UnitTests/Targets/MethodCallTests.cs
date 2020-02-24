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

using System.Collections.Generic;
using System.Linq;
using NLog.Config;
using NLog.Targets;
using Xunit;

namespace NLog.UnitTests.Targets
{
    public class MethodCallTests : NLogTestBase
    {
        private const string CorrectClassName = "NLog.UnitTests.Targets.MethodCallTests, NLog.UnitTests";

        #region ToBeCalled Methods

#pragma warning disable xUnit1013 //we need public methods here

        private static MethodCallRecord LastCallTest;
        public static void StaticAndPublic(string param1, int param2)
        {
            LastCallTest = new MethodCallRecord("StaticAndPublic", param1, param2);
        }

        public static void StaticAndPublicWrongParameters(string param1, string param2)
        {
            LastCallTest = new MethodCallRecord("StaticAndPublic", param1, param2);
        }

        public static void StaticAndPublicTooLessParameters(string param1)
        {
            LastCallTest = new MethodCallRecord("StaticAndPublicTooLessParameters", param1);
        }

        public static void StaticAndPublicTooManyParameters(string param1, int param2, string param3)
        {
            LastCallTest = new MethodCallRecord("StaticAndPublicTooManyParameters", param1, param2);
        }

        public static void StaticAndPublicOptional(string param1, int param2, string param3 = "fixedValue")
        {
            LastCallTest = new MethodCallRecord("StaticAndPublicOptional", param1, param2, param3);
        }

        public void NonStaticAndPublic()
        {
            LastCallTest = new MethodCallRecord("NonStaticAndPublic");
        }


        public static void StaticAndPrivate()
        {
            LastCallTest = new MethodCallRecord("StaticAndPrivate");
        }

#pragma warning restore xUnit1013


        #endregion

        [Fact]
        public void TestMethodCall1()
        {
            TestMethodCall(new MethodCallRecord("StaticAndPublic", "test1", 2), "StaticAndPublic", CorrectClassName);
        }

        [Fact]
        public void TestMethodCall2()
        {
            //Type AssemblyQualifiedName 
            //to find, use typeof(MethodCallTests).AssemblyQualifiedName
            TestMethodCall(new MethodCallRecord("StaticAndPublic", "test1", 2), "StaticAndPublic", "NLog.UnitTests.Targets.MethodCallTests, NLog.UnitTests, Version=1.0.0.0, Culture=neutral, PublicKeyToken=b793d3de60bec2b9");
        }

        [Fact]
        public void PrivateMethodDontThrow()
        {
            using (new NoThrowNLogExceptions())
            {
                TestMethodCall(null, "NonStaticAndPublic", CorrectClassName);
            }
        }

        [Fact]
        public void WrongClassDontThrow()
        {
            using (new NoThrowNLogExceptions())
            {
                TestMethodCall(null, "StaticAndPublic", "NLog.UnitTests222.Targets.CallTest, NLog.UnitTests");
            }
        }

        [Fact]
        public void WrongParametersDontThrow()
        {
            using (new NoThrowNLogExceptions())
            {
                TestMethodCall(null, "StaticAndPublicWrongParameters", CorrectClassName);
            }
        }

        [Fact]
        public void TooLessParametersDontThrow()
        {
            using (new NoThrowNLogExceptions())
            {
                TestMethodCall(null, "StaticAndPublicTooLessParameters", CorrectClassName);
            }
        }

        [Fact]
        public void TooManyParametersDontThrow()
        {
            using (new NoThrowNLogExceptions())
            {
                TestMethodCall(null, "StaticAndPublicTooManyParameters", CorrectClassName);
            }
        }

        [Fact]
        public void OptionalParameters()
        {
            TestMethodCall(new MethodCallRecord("StaticAndPublicOptional", "test1", 2, "fixedValue"), "StaticAndPublicOptional", CorrectClassName);
        }

        [Fact]
        public void FluentDelegateConfiguration()
        {
            var configuration = new LoggingConfiguration();

            string expectedMessage = "Hello World";
            string actualMessage = string.Empty;
            configuration.AddRuleForAllLevels(new MethodCallTarget("Hello", (logEvent, parameters) => { actualMessage = logEvent.Message; }));
            LogManager.Configuration = configuration;

            LogManager.GetCurrentClassLogger().Debug(expectedMessage);

            Assert.Equal(expectedMessage, actualMessage);
        }

        private static void TestMethodCall(MethodCallRecord expected, string methodName, string className)
        {
            var target = new MethodCallTarget
            {
                Name = "t1",
                ClassName = className,
                MethodName = methodName
            };
            target.Parameters.Add(new MethodCallParameter("param1", "test1"));
            target.Parameters.Add(new MethodCallParameter("param2", "2", typeof(int)));

            var configuration = new LoggingConfiguration();
            configuration.AddTarget(target);
            configuration.LoggingRules.Add(new LoggingRule("*", LogLevel.Debug, target));
            LogManager.Configuration = configuration;

            LastCallTest = null;

            LogManager.GetCurrentClassLogger().Debug("test method 1");

            Assert.Equal(expected, LastCallTest);
        }

        private class MethodCallRecord
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="T:System.Object"/> class.
            /// </summary>
            public MethodCallRecord(string method, params object[] parameterValues)
            {
                Method = method;
                if (parameterValues != null) ParameterValues = parameterValues.ToList();
            }

            public string Method { get; set; }
            public List<object> ParameterValues { get; set; }

            protected bool Equals(MethodCallRecord other)
            {
                return string.Equals(Method, other.Method) && ParameterValues.SequenceEqual(other.ParameterValues);
            }

            /// <summary>
            /// Determines whether the specified object is equal to the current object.
            /// </summary>
            /// <returns>
            /// true if the specified object  is equal to the current object; otherwise, false.
            /// </returns>
            /// <param name="obj">The object to compare with the current object. </param>
            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != GetType()) return false;
                return Equals((MethodCallRecord)obj);
            }

            /// <summary>
            /// Serves as the default hash function. 
            /// </summary>
            /// <returns>
            /// A hash code for the current object.
            /// </returns>
            public override int GetHashCode()
            {
                unchecked
                {
                    return ((Method != null ? Method.GetHashCode() : 0) * 397) ^ (ParameterValues != null ? ParameterValues.GetHashCode() : 0);
                }
            }
        }
    }
}
