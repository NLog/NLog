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

namespace NLog.UnitTests.LayoutRenderers
{
    using System;
    using System.Collections.Generic;
    using NLog.LayoutRenderers;
    using NLog.Layouts;
    using NLog.Targets;
    using NLog.Internal;
    using Xunit;
    using NLog.Config;

    public class ExceptionDataLayoutRendererTests : NLogTestBase
    {
        const int E_FAIL = 80004005;

        private ILogger logger = LogManager.GetLogger("NLog.UnitTests.LayoutRenderer.ExceptionDataLayoutRendererTests");

        [Fact]
        public void ExceptionWithDataIsLogged()
        {
            const string exceptionMessage = "Test exception";
            const string exceptionDataKey = "testkey";
            const string exceptionDataValue = "testvalue";

            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
            <nlog>
                <targets>
                    <target name='debug1' type='Debug' layout='${exceptiondata:"+exceptionDataKey+@"}' />
                </targets>
                <rules>
                    <logger minlevel='Info' writeTo='debug1' />
                </rules>
            </nlog>");


            Exception ex = GetExceptionWithStackTrace(exceptionMessage);
            ex.Data.Add(exceptionDataKey, exceptionDataValue);
            logger.Error(ex, "msg");
            
            AssertDebugLastMessage("debug1", exceptionDataValue);
        }

        [Fact]
        public void ExceptionWithOutDataIsNotLogged()
        {
            const string exceptionMessage = "Test exception";
            const string exceptionDataKey = "testkey";
            const string exceptionDataValue = "testvalue";

            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
            <nlog>
                <targets>
                    <target name='debug1' type='Debug' layout='${exceptiondata:" + exceptionDataKey + @"}' />
                </targets>
                <rules>
                    <logger minlevel='Info' writeTo='debug1' />
                </rules>
            </nlog>");


            Exception ex = GetExceptionWithStackTrace(exceptionMessage);
            logger.Error(ex, "msg");

            Assert.NotEqual(GetDebugLastMessage("debug1"), exceptionDataValue);
        }

        [Fact]
        public void ExceptionUsingSpecifiedParamLogsProperly()
        {
            const string exceptionMessage = "I don't like nullref exception!";
            const string exceptionDataKey = "testkey";
            const string exceptionDataValue = "testvalue";

            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
            <nlog>
                <targets>
                    <target name='debug1' type='Debug' layout='${exceptiondata:DataKey="+exceptionDataKey+@"}' />
                </targets>
                <rules>
                    <logger minlevel='Info' writeTo='debug1' />
                </rules>
            </nlog>");


            Exception ex = GetExceptionWithStackTrace(exceptionMessage);
            ex.Data.Add(exceptionDataKey, exceptionDataValue);
            logger.Error(ex);
            AssertDebugLastMessage("debug1", exceptionDataValue);
        }



        [Fact]
        public void ErrorException_should_not_throw_exception_when_exception_message_property_throw_exception()
        {
            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
            <nlog>
                <targets>
                    <target name='debug1' type='Debug' layout='${exceptiondata}' />
                </targets>
                <rules>
                    <logger minlevel='Info' writeTo='debug1' />
                </rules>
            </nlog>");

            var ex = new ExceptionWithBrokenMessagePropertyException();
            var exRecorded = Record.Exception(() => logger.Error(ex, "msg"));
            Assert.Null(exRecorded);
        }

        [Fact]
        public void ErrorException_should_not_throw_exception_when_exception_message_property_throw_exception_serialize()
        {
            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
            <nlog>
                <targets>
                    <target name='debug1' type='Debug' layout='${exceptiondata:datakey=@}' />
                </targets>
                <rules>
                    <logger minlevel='Info' writeTo='debug1' />
                </rules>
            </nlog>");

            var ex = new ExceptionWithBrokenMessagePropertyException();
            var exRecorded = Record.Exception(() => logger.Error(ex, "msg"));
            Assert.Null(exRecorded);
        }

        [Fact]
        public void NoDatkeyTest()
        {
            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
            <nlog>
                <targets>
                    <target name='debug1' type='Debug' layout='${exceptiondata:}' />
                </targets>
                <rules>
                    <logger minlevel='Info' writeTo='debug1' />
                </rules>
            </nlog>");

            var ex = GetNestedExceptionWithStackTrace("Goodbye World");
            logger.Fatal(ex, "msg");
            AssertDebugLastMessage("debug1", "");
        }

        [Fact]
        public void NoDatkeyUingParamTest()
        {
            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
            <nlog>
                <targets>
                    <target name='debug1' type='Debug' layout='${exceptiondata:DataKey=}' />
                </targets>
                <rules>
                    <logger minlevel='Info' writeTo='debug1' />
                </rules>
            </nlog>");

            var ex = GetNestedExceptionWithStackTrace("Goodbye World");
            logger.Fatal(ex, "msg");
            AssertDebugLastMessage("debug1", "");
        }

        [Fact]
        public void BaseExceptionTest()
        {
            const string exceptionDataKey = "testkey";
            const string exceptionDataValue = "testvalue";
            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
            <nlog>
                <targets>
                    <target name='debug1' type='Debug' layout='${exceptiondata:DataKey=" + exceptionDataKey + @", BaseException=false}' />
                </targets>
                <rules>
                    <logger minlevel='Info' writeTo='debug1' />
                </rules>
            </nlog>");

            var ex = GetNestedExceptionWithStackTrace("Goodbye World");
            ex.Data.Add(exceptionDataKey, exceptionDataValue);
            logger.Fatal(ex, "msg");
            AssertDebugLastMessage("debug1", exceptionDataValue);
        }

        private class ExceptionWithBrokenMessagePropertyException : NLogConfigurationException
        {
            public override string Message => throw new Exception("Exception from Message property");
        }

        /// <summary>
        /// Get an exception with stacktrace by generating a exception
        /// </summary>
        /// <param name="exceptionMessage"></param>
        /// <returns></returns>
        private Exception GetExceptionWithStackTrace(string exceptionMessage)
        {
            try
            {
                GenericClass<int, string, bool>.Method1("aaa", true, null, 42, DateTime.Now, exceptionMessage);
                return null;
            }
            catch (Exception exception)
            {
                exception.Source = GetType().ToString();
                return exception;
            }
        }

        private Exception GetNestedExceptionWithStackTrace(string exceptionMessage)
        {
            try
            {
                try
                {
                    try
                    {
                        GenericClass<int, string, bool>.Method1("aaa", true, null, 42, DateTime.Now, exceptionMessage);
                    }
                    catch (Exception exception)
                    {
                        throw new System.ArgumentException("Wrapper1", exception);
                    }
                }
                catch (Exception exception)
                {
                    throw new ApplicationException("Wrapper2", exception);
                }

                return null;
            }
            catch (Exception ex)
            {
                ex.Data["1Really.Bad-Boy!"] = "Hello World";
                return ex;
            }
        }

        private int NestedFunc(int recursion, Func<int> innerAction)
        {
            try
            {
                if (recursion-- == 0)
                    return System.Threading.Tasks.Task<int>.Factory.StartNew(() => innerAction.Invoke())
                        .Result;
                return NestedFunc(recursion, innerAction);
            }
            catch
            {
                throw;  // Just to make the method complex, and avoid inline
            }
        }

        private class GenericClass<TA, TB, TC>
        {
            internal static List<GenericClass<TA, TB, TC>> Method1(string aaa, bool b, object o, int i, DateTime now, string exceptionMessage)
            {
                Method2(aaa, b, o, i, now, null, null, exceptionMessage);
                return null;
            }

            internal static int Method2<T1, T2, T3>(T1 aaa, T2 b, T3 o, int i, DateTime now, Nullable<int> gfff, List<int>[] something, string exceptionMessage)
            {
                throw new CustomArgumentException(exceptionMessage, "exceptionMessage");
            }
        }

        public class CustomArgumentException : ApplicationException
        {
            public CustomArgumentException(string message, string paramName)
                : base(message)
            {
                ParamName = paramName;
                StrangeProperty = "Strange World";
                HResult = E_FAIL;
            }

            public string ParamName { get; }

            public string StrangeProperty { private get; set; }
        }
    }
}