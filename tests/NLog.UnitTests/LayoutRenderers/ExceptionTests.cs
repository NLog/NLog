// 
// Copyright (c) 2004-2009 Jaroslaw Kowalski <jaak@jkowalski.net>
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

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NLog.UnitTests.LayoutRenderers
{
    using System.Collections.Generic;

    [TestClass]
    public class ExceptionTests : NLogTestBase
    {
        private Logger logger = LogManager.GetLogger("NLog.UnitTests.LayoutRenderer.ExceptionTests");

        [TestMethod]
        public void Test1()
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

            // each version of the framework produces slightly different information
#if SILVERLIGHT
            AssertDebugLastMessage("debug7", "NLog.UnitTests.LayoutRenderers.ExceptionTests.GenericClass`3.Method2[T1,T2,T3](T1 aaa, T2 b, T3 o, Int32 i, DateTime now, Nullable`1 gfff, List`1[] something)");
#elif NETCF2_0
            // NET Compact Framework 2.0 is not very chatty
            AssertDebugLastMessage("debug7", "GenericClass`3.Method2()");
#elif NET_CF
            // 3.5 is better...
            AssertDebugLastMessage("debug7", "NLog.UnitTests.LayoutRenderers.ExceptionTests.GenericClass`3.Method2[T1,T2,T3](String aaa, Boolean b, Object o, Int32 i, DateTime now, Nullable`1 gfff, List`1[] something)");
#else
            AssertDebugLastMessage("debug7", "Int32 Method2[T1,T2,T3](T1, T2, T3, Int32, System.DateTime, System.Nullable`1[System.Int32], System.Collections.Generic.List`1[System.Int32][])");
#endif

            AssertDebugLastMessage("debug8", "Test exception*" + typeof(InvalidOperationException).Name);
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