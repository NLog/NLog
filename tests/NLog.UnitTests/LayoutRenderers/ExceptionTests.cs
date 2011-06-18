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

using System;
using System.Xml;
using System.Globalization;

using NLog;
using NLog.Config;
using NLog.LayoutRenderers;
using NLog.Layouts;
using NLog.Targets;
using NUnit.Framework;

#if !NUNIT
    using SetUp = Microsoft.VisualStudio.TestTools.UnitTesting.TestInitializeAttribute;
    using TestFixture = Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute;
    using Test = Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute;
    using TearDown =  Microsoft.VisualStudio.TestTools.UnitTesting.TestCleanupAttribute;
#endif

namespace NLog.UnitTests.LayoutRenderers
{
    using System.Collections.Generic;
    using NLog.Internal;

    [TestFixture]
    public class ExceptionTests : NLogTestBase
    {
        private Logger logger = LogManager.GetLogger("NLog.UnitTests.LayoutRenderer.ExceptionTests");

        [Test]
        public void ExceptionWithStackTraceTest()
        {
            LogManager.Configuration = CreateConfigurationFromString(@"
            <nlog>
                <targets>
                    <target name='debug1' type='Debug' layout='${exception}' />
                    <target name='debug2' type='Debug' layout='${exception:format=stacktrace}' />
                    <target name='debug3' type='Debug' layout='${exception:format=type}' />
                    <target name='debug4' type='Debug' layout='${exception:format=shorttype}' />
                    <target name='debug5' type='Debug' layout='${exception:format=tostring}' />
                    <target name='debug6' type='Debug' layout='${exception:format=message}' />
                    <target name='debug7' type='Debug' layout='${exception:format=method}' />
                    <target name='debug8' type='Debug' layout='${exception:format=message,shorttype:separator=*}' />
                </targets>
                <rules>
                    <logger minlevel='Info' writeTo='debug1,debug2,debug3,debug4,debug5,debug6,debug7,debug8' />
                </rules>
            </nlog>");

            string exceptionMessage = "Test exception";
            Exception ex = GetExceptionWithStackTrace(exceptionMessage);
            logger.ErrorException("msg", ex);
            AssertDebugLastMessage("debug1", exceptionMessage);
            AssertDebugLastMessage("debug2", ex.StackTrace);
            AssertDebugLastMessage("debug3", typeof(InvalidOperationException).FullName);
            AssertDebugLastMessage("debug4", typeof(InvalidOperationException).Name);
            AssertDebugLastMessage("debug5", ex.ToString());
            AssertDebugLastMessage("debug6", exceptionMessage);

            // each version of the framework produces slightly different information for MethodInfo, so we just 
            // make sure it's not empty
            var debug7Target = (NLog.Targets.DebugTarget)LogManager.Configuration.FindTargetByName("debug7");
            Assert.IsFalse(string.IsNullOrEmpty(debug7Target.LastMessage));

            AssertDebugLastMessage("debug8", "Test exception*" + typeof(InvalidOperationException).Name);
        }

        [Test]
        public void ExceptionWithoutStackTraceTest()
        {
            LogManager.Configuration = CreateConfigurationFromString(@"
            <nlog>
                <targets>
                    <target name='debug1' type='Debug' layout='${exception}' />
                    <target name='debug2' type='Debug' layout='${exception:format=stacktrace}' />
                    <target name='debug3' type='Debug' layout='${exception:format=type}' />
                    <target name='debug4' type='Debug' layout='${exception:format=shorttype}' />
                    <target name='debug5' type='Debug' layout='${exception:format=tostring}' />
                    <target name='debug6' type='Debug' layout='${exception:format=message}' />
                    <target name='debug7' type='Debug' layout='${exception:format=method}' />
                    <target name='debug8' type='Debug' layout='${exception:format=message,shorttype:separator=*}' />
                </targets>
                <rules>
                    <logger minlevel='Info' writeTo='debug1,debug2,debug3,debug4,debug5,debug6,debug7,debug8' />
                </rules>
            </nlog>");

            string exceptionMessage = "Test exception";
            Exception ex = GetExceptionWithoutStackTrace(exceptionMessage);
            logger.ErrorException("msg", ex);
            AssertDebugLastMessage("debug1", exceptionMessage);
            AssertDebugLastMessage("debug2", "");
            AssertDebugLastMessage("debug3", typeof(InvalidOperationException).FullName);
            AssertDebugLastMessage("debug4", typeof(InvalidOperationException).Name);
            AssertDebugLastMessage("debug5", ex.ToString());
            AssertDebugLastMessage("debug6", exceptionMessage);
            AssertDebugLastMessage("debug7", "");
            AssertDebugLastMessage("debug8", "Test exception*" + typeof(InvalidOperationException).Name);
        }

        [Test]
        public void ExceptionNewLineSeparatorTest()
        {
            LogManager.Configuration = CreateConfigurationFromString(@"
            <nlog>
                <targets>
                    <target name='debug1' type='Debug' layout='${exception:format=message,shorttype:separator=&#13;&#10;}' />
                </targets>
                <rules>
                    <logger minlevel='Info' writeTo='debug1' />
                </rules>
            </nlog>");

            string exceptionMessage = "Test exception";
            Exception ex = GetExceptionWithStackTrace(exceptionMessage);
            logger.ErrorException("msg", ex);
            AssertDebugLastMessage("debug1", "Test exception\r\n" + typeof(InvalidOperationException).Name);
        }

        [Test]
        public void InnerExceptionTest()
        {
            LogManager.Configuration = CreateConfigurationFromString(@"
            <nlog>
                <targets>
                    <target name='debug1' type='Debug' layout='${exception:format=shorttype,message:maxInnerExceptionLevel=3}' />
                </targets>
                <rules>
                    <logger minlevel='Info' writeTo='debug1' />
                </rules>
            </nlog>");

            string exceptionMessage = "Test exception";
            Exception ex = GetNestedExceptionWithStackTrace(exceptionMessage);
            logger.ErrorException("msg", ex);
            AssertDebugLastMessage("debug1", "InvalidOperationException Wrapper2" + EnvironmentHelper.NewLine + 
"InvalidOperationException Wrapper1" + EnvironmentHelper.NewLine +
"InvalidOperationException Test exception");
        }

        [Test]
        public void CustomInnerExceptionTest()
        {
            LogManager.Configuration = CreateConfigurationFromString(@"
            <nlog>
                <targets>
                    <target name='debug1' type='Debug' layout='${exception:format=shorttype,message:maxInnerExceptionLevel=1:innerExceptionSeparator=&#13;&#10;----INNER----&#13;&#10;:innerFormat=type,message}' />
                </targets>
                <rules>
                    <logger minlevel='Info' writeTo='debug1' />
                </rules>
            </nlog>");

            var t = (DebugTarget)LogManager.Configuration.AllTargets[0];
            var elr = ((SimpleLayout) t.Layout).Renderers[0] as ExceptionLayoutRenderer;
            Assert.AreEqual("\r\n----INNER----\r\n", elr.InnerExceptionSeparator);

            string exceptionMessage = "Test exception";
            Exception ex = GetNestedExceptionWithStackTrace(exceptionMessage);
            logger.ErrorException("msg", ex);
            AssertDebugLastMessage("debug1", "InvalidOperationException Wrapper2" + 
"\r\n----INNER----\r\n" +
"System.InvalidOperationException Wrapper1");
        }

        private Exception GetExceptionWithStackTrace(string exceptionMessage)
        {
            try
            {
                GenericClass<int, string, bool>.Method1("aaa", true, null, 42, DateTime.Now);
                return null;
            }
            catch (Exception exception)
            {
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
                        GenericClass<int, string, bool>.Method1("aaa", true, null, 42, DateTime.Now);
                    }
                    catch (Exception exception)
                    {
                        throw new InvalidOperationException("Wrapper1", exception);
                    }
                }
                catch (Exception exception)
                {
                    throw new InvalidOperationException("Wrapper2", exception);
                }

                return null;
            }
            catch (Exception ex)
            {
                return ex;
            }
        }

        private Exception GetExceptionWithoutStackTrace(string exceptionMessage)
        {
            return new InvalidOperationException(exceptionMessage);
        }

        private class GenericClass<TA, TB, TC>
        {
            internal static List<GenericClass<TA, TB, TC>> Method1(string aaa, bool b, object o, int i, DateTime now)
            {
                Method2(aaa, b, o, i, now, null, null);
                return null;
            }

            internal static int Method2<T1, T2, T3>(T1 aaa, T2 b, T3 o, int i, DateTime now, Nullable<int> gfff, List<int>[] something)
            {
                throw new InvalidOperationException("Test exception");
            }
        }
    }
}
