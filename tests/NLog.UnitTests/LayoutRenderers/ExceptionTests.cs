// 
// Copyright (c) 2004-2021 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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
    using NLog.Config;
    using NLog.Internal;
    using NLog.LayoutRenderers;
    using NLog.Layouts;
    using NLog.Targets;
    using Xunit;

    public class ExceptionTests : NLogTestBase
    {
        const int E_FAIL = 80004005;

        private const string ExceptionDataFormat = "{0}: {1}";

        [Fact]
        public void ExceptionWithStackTraceTest()
        {
            var logFactory = new LogFactory().Setup().LoadConfigurationFromXml(@"
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
                    <target name='debug9' type='Debug' layout='${exception:format=data}' />
                    <target name='debug10' type='Debug' layout='${exception:format=source}' />
                    <target name='debug11' type='Debug' layout='${exception:format=hresult}' />
                </targets>
                <rules>
                    <logger minlevel='Info' writeTo='debug1,debug2,debug3,debug4,debug5,debug6,debug7,debug8,debug9,debug10,debug11' />
                </rules>
            </nlog>").LogFactory;

            const string exceptionMessage = "Test exception";
            const string exceptionDataKey = "testkey";
            const string exceptionDataValue = "testvalue";
            Exception ex = GetExceptionWithStackTrace(exceptionMessage);
            ex.Data.Add(exceptionDataKey, exceptionDataValue);
            logFactory.GetCurrentClassLogger().Error(ex, "msg");

            var dataText = string.Format(ExceptionDataFormat, exceptionDataKey, exceptionDataValue);
            logFactory.AssertDebugLastMessage("debug1", ex.ToString() + " " + dataText);
            logFactory.AssertDebugLastMessage("debug2", ex.StackTrace);
            logFactory.AssertDebugLastMessage("debug3", typeof(CustomArgumentException).FullName);
            logFactory.AssertDebugLastMessage("debug4", typeof(CustomArgumentException).Name);
            logFactory.AssertDebugLastMessage("debug5", ex.ToString());
            logFactory.AssertDebugLastMessage("debug6", exceptionMessage);
            logFactory.AssertDebugLastMessage("debug8", exceptionMessage + "*" + typeof(CustomArgumentException).Name);
            logFactory.AssertDebugLastMessage("debug9", dataText);
            logFactory.AssertDebugLastMessage("debug10", GetType().ToString());
#if !NET35 && !NET40
            logFactory.AssertDebugLastMessage("debug11", $"0x{E_FAIL:X8}");
#endif

            // each version of the framework produces slightly different information for MethodInfo, so we just 
            // make sure it's not empty
            var debug7Target = logFactory.Configuration.FindTargetByName<DebugTarget>("debug7");
            Assert.False(string.IsNullOrEmpty(debug7Target.LastMessage));
        }

        /// <summary>
        /// Just wrrite exception, no message argument.
        /// </summary>
        [Fact]
        public void ExceptionWithoutMessageParam()
        {
            var logFactory = new LogFactory().Setup().LoadConfigurationFromXml(@"
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
                    <target name='debug9' type='Debug' layout='${exception:format=data}' />
                    <target name='debug10' type='Debug' layout='${exception:format=source}' />
                    <target name='debug11' type='Debug' layout='${exception:format=hresult}' />
                </targets>
                <rules>
                    <logger minlevel='Info' writeTo='debug1,debug2,debug3,debug4,debug5,debug6,debug7,debug8,debug9,debug10,debug11' />
                </rules>
            </nlog>").LogFactory;

            const string exceptionMessage = "I don't like nullref exception!";
            const string exceptionDataKey = "testkey";
            const string exceptionDataValue = "testvalue";
            Exception ex = GetExceptionWithStackTrace(exceptionMessage);
            ex.Data.Add(exceptionDataKey, exceptionDataValue);
            logFactory.GetCurrentClassLogger().Error(ex);

            var dataText = string.Format(ExceptionDataFormat, exceptionDataKey, exceptionDataValue);
            logFactory.AssertDebugLastMessage("debug1", ex.ToString() + " " + dataText);
            logFactory.AssertDebugLastMessage("debug2", ex.StackTrace);
            logFactory.AssertDebugLastMessage("debug3", typeof(CustomArgumentException).FullName);
            logFactory.AssertDebugLastMessage("debug4", typeof(CustomArgumentException).Name);
            logFactory.AssertDebugLastMessage("debug5", ex.ToString());
            logFactory.AssertDebugLastMessage("debug6", exceptionMessage);
            logFactory.AssertDebugLastMessage("debug8", exceptionMessage + "*" + typeof(CustomArgumentException).Name);
            logFactory.AssertDebugLastMessage("debug9", dataText);
            logFactory.AssertDebugLastMessage("debug10", GetType().ToString());
#if !NET35 && !NET40
            logFactory.AssertDebugLastMessage("debug11", $"0x{E_FAIL:X8}");
#endif

            // each version of the framework produces slightly different information for MethodInfo, so we just 
            // make sure it's not empty
            var debug7Target = logFactory.Configuration.FindTargetByName<DebugTarget>("debug7");
            Assert.False(string.IsNullOrEmpty(debug7Target.LastMessage));
        }

        [Fact]
        public void ExceptionWithoutStackTraceTest()
        {
            var logFactory = new LogFactory().Setup().LoadConfigurationFromXml(@"
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
                    <target name='debug9' type='Debug' layout='${exception:format=data}' />
                    <target name='debug10' type='Debug' layout='${exception:format=source}' />
                    <target name='debug11' type='Debug' layout='${exception:format=hresult}' />
                </targets>
                <rules>
                    <logger minlevel='Info' writeTo='debug1,debug2,debug3,debug4,debug5,debug6,debug7,debug8,debug9,debug10,debug11' />
                </rules>
            </nlog>").LogFactory;

            const string exceptionMessage = "Test exception";
            const string exceptionDataKey = "testkey";
            const string exceptionDataValue = "testvalue";
            Exception ex = GetExceptionWithoutStackTrace(exceptionMessage);
            ex.Data.Add(exceptionDataKey, exceptionDataValue);
            logFactory.GetCurrentClassLogger().Error(ex, "msg");

            var dataText = string.Format(ExceptionDataFormat, exceptionDataKey, exceptionDataValue);
            logFactory.AssertDebugLastMessage("debug1", ex.ToString() + " " + dataText);
            logFactory.AssertDebugLastMessage("debug2", "");
            logFactory.AssertDebugLastMessage("debug3", typeof(CustomArgumentException).FullName);
            logFactory.AssertDebugLastMessage("debug4", typeof(CustomArgumentException).Name);
            logFactory.AssertDebugLastMessage("debug5", ex.ToString());
            logFactory.AssertDebugLastMessage("debug6", exceptionMessage);
            logFactory.AssertDebugLastMessage("debug7", "");
            logFactory.AssertDebugLastMessage("debug8", "Test exception*" + typeof(CustomArgumentException).Name);
            logFactory.AssertDebugLastMessage("debug9", dataText);
            logFactory.AssertDebugLastMessage("debug10", "");
#if !NET35 && !NET40
            logFactory.AssertDebugLastMessage("debug11", $"0x{E_FAIL:X8}");
#endif
        }

        [Fact]
        public void ExceptionNewLineSeparatorTest()
        {
            var logFactory = new LogFactory().Setup().LoadConfigurationFromXml(@"
            <nlog>
                <targets>
                    <target name='debug' type='Debug' layout='${exception:format=message,shorttype:separator=&#13;&#10;}' />
                </targets>
                <rules>
                    <logger minlevel='Info' writeTo='debug' />
                </rules>
            </nlog>").LogFactory;

            string exceptionMessage = "Test exception";
            Exception ex = GetExceptionWithStackTrace(exceptionMessage);

            logFactory.GetCurrentClassLogger().Error(ex, "msg");
            logFactory.AssertDebugLastMessage("Test exception\r\n" + typeof(CustomArgumentException).Name);
        }

        [Fact]
        public void ExceptionNewLineSeparatorLayoutTest()
        {
            var logFactory = new LogFactory().Setup().LoadConfigurationFromXml(@"
            <nlog>
                <targets>
                    <target name='debug' type='Debug' layout='${exception:separator= ${NewLine} :format=message,shorttype}' />
                </targets>
                <rules>
                    <logger minlevel='Info' writeTo='debug' />
                </rules>
            </nlog>").LogFactory;

            string exceptionMessage = "Test exception";
            Exception ex = GetExceptionWithStackTrace(exceptionMessage);

            logFactory.GetCurrentClassLogger().Error(ex, "msg");
            logFactory.AssertDebugLastMessage($"Test exception {System.Environment.NewLine} {typeof(CustomArgumentException).Name}");
        }

        [Fact]
        public void ExceptionUsingLogMethodTest()
        {
            var logFactory = BuildConfigurationForExceptionTests();

            string exceptionMessage = "Test exception";
            Exception ex = GetExceptionWithStackTrace(exceptionMessage);
            logFactory.GetCurrentClassLogger().Log(LogLevel.Error, ex, "msg");
            logFactory.AssertDebugLastMessage("ERROR*msg*Test exception*" + typeof(CustomArgumentException).Name);

            logFactory.GetCurrentClassLogger().Log(LogLevel.Error, ex, () => "msg func");
            logFactory.AssertDebugLastMessage("ERROR*msg func*Test exception*" + typeof(CustomArgumentException).Name);
        }

        [Fact]
        public void ExceptionUsingTraceMethodTest()
        {
            var logFactory = BuildConfigurationForExceptionTests();
            string exceptionMessage = "Test exception";
            Exception ex = GetExceptionWithStackTrace(exceptionMessage);
            logFactory.GetCurrentClassLogger().Trace(ex, "msg");
            logFactory.AssertDebugLastMessage("TRACE*msg*Test exception*" + typeof(CustomArgumentException).Name);

            logFactory.GetCurrentClassLogger().Trace(ex, () => "msg func");
            logFactory.AssertDebugLastMessage("TRACE*msg func*Test exception*" + typeof(CustomArgumentException).Name);
        }

        [Fact]
        public void ExceptionUsingDebugMethodTest()
        {
            var logFactory = BuildConfigurationForExceptionTests();
            string exceptionMessage = "Test exception";
            Exception ex = GetExceptionWithStackTrace(exceptionMessage);
            logFactory.GetCurrentClassLogger().Debug(ex, "msg");
            logFactory.AssertDebugLastMessage("DEBUG*msg*Test exception*" + typeof(CustomArgumentException).Name);

            logFactory.GetCurrentClassLogger().Debug(ex, () => "msg func");
            logFactory.AssertDebugLastMessage("DEBUG*msg func*Test exception*" + typeof(CustomArgumentException).Name);
        }

        [Fact]
        public void ExceptionUsingInfoMethodTest()
        {
            var logFactory = BuildConfigurationForExceptionTests();
            string exceptionMessage = "Test exception";
            Exception ex = GetExceptionWithStackTrace(exceptionMessage);
            logFactory.GetCurrentClassLogger().Info(ex, "msg");
            logFactory.AssertDebugLastMessage("INFO*msg*Test exception*" + typeof(CustomArgumentException).Name);

            logFactory.GetCurrentClassLogger().Info(ex, () => "msg func");
            logFactory.AssertDebugLastMessage("INFO*msg func*Test exception*" + typeof(CustomArgumentException).Name);
        }

        [Fact]
        public void ExceptionUsingWarnMethodTest()
        {
            var logFactory = BuildConfigurationForExceptionTests();
            string exceptionMessage = "Test exception";
            Exception ex = GetExceptionWithStackTrace(exceptionMessage);
            logFactory.GetCurrentClassLogger().Warn(ex, "msg");
            logFactory.AssertDebugLastMessage("WARN*msg*Test exception*" + typeof(CustomArgumentException).Name);

            logFactory.GetCurrentClassLogger().Warn(ex, () => "msg func");
            logFactory.AssertDebugLastMessage("WARN*msg func*Test exception*" + typeof(CustomArgumentException).Name);
        }

        [Fact]
        public void ExceptionUsingErrorMethodTest()
        {
            var logFactory = BuildConfigurationForExceptionTests();
            string exceptionMessage = "Test exception";
            Exception ex = GetExceptionWithStackTrace(exceptionMessage);
            logFactory.GetCurrentClassLogger().Error(ex, "msg");
            logFactory.AssertDebugLastMessage("ERROR*msg*Test exception*" + typeof(CustomArgumentException).Name);

            logFactory.GetCurrentClassLogger().Error(ex, () => "msg func");
            logFactory.AssertDebugLastMessage("ERROR*msg func*Test exception*" + typeof(CustomArgumentException).Name);
        }

        [Fact]
        public void ExceptionUsingFatalMethodTest()
        {
            var logFactory = BuildConfigurationForExceptionTests();
            string exceptionMessage = "Test exception";
            Exception ex = GetExceptionWithStackTrace(exceptionMessage);
            logFactory.GetCurrentClassLogger().Fatal(ex, "msg");
            logFactory.AssertDebugLastMessage("FATAL*msg*Test exception*" + typeof(CustomArgumentException).Name);

            logFactory.GetCurrentClassLogger().Fatal(ex, () => "msg func");
            logFactory.AssertDebugLastMessage("FATAL*msg func*Test exception*" + typeof(CustomArgumentException).Name);
        }

        [Fact]
        public void InnerExceptionTest()
        {
            var logFactory = new LogFactory().Setup().LoadConfigurationFromXml(@"
            <nlog>
                <targets>
                    <target name='debug' type='Debug' layout='${exception:format=shorttype,message:maxInnerExceptionLevel=3}' />
                </targets>
                <rules>
                    <logger minlevel='Info' writeTo='debug' />
                </rules>
            </nlog>").LogFactory;

            string exceptionMessage = "Test exception";
            Exception ex = GetNestedExceptionWithStackTrace(exceptionMessage);

            logFactory.GetCurrentClassLogger().Error(ex, "msg");
            logFactory.AssertDebugLastMessage("ApplicationException Wrapper2" + EnvironmentHelper.NewLine +
                                             "ArgumentException Wrapper1" + EnvironmentHelper.NewLine +
                                             "CustomArgumentException Test exception");
        }

        [Fact]
        public void InnerExceptionTest_Serialize()
        {
            var logFactory = new LogFactory().Setup().LoadConfigurationFromXml(@"
            <nlog>
                <targets>
                    <target name='debug' type='Debug' layout='${exception:format=@}' />
                </targets>
                <rules>
                    <logger minlevel='Info' writeTo='debug' />
                </rules>
            </nlog>").LogFactory;

            string exceptionMessage = "Test exception";
            Exception ex = GetNestedExceptionWithStackTrace(exceptionMessage);

            logFactory.GetCurrentClassLogger().Error(ex, "msg");

            var lastMessage = logFactory.AssertDebugLastMessageNotEmpty();
            Assert.StartsWith("{\"Type\":\"System.ApplicationException\", ", lastMessage);
            Assert.Contains("\"InnerException\":{\"Type\":\"System.ArgumentException\", ", lastMessage);
            Assert.Contains("\"ParamName\":\"exceptionMessage\"", lastMessage);
            Assert.Contains("1Really_Bad_Boy_", lastMessage);
        }

        [Fact]
        public void CustomInnerExceptionTest()
        {
            var logFactory = new LogFactory().Setup().LoadConfigurationFromXml(@"
            <nlog>
                <targets>
                    <target name='debug1' type='Debug' layout='${exception:format=shorttype,message:maxInnerExceptionLevel=1:innerExceptionSeparator=&#13;&#10;----INNER----&#13;&#10;:innerFormat=type,message}' />
                    <target name='debug2' type='Debug' layout='${exception:format=shorttype,message:maxInnerExceptionLevel=1:innerExceptionSeparator=&#13;&#10;----INNER----&#13;&#10;:innerFormat=type,message,data}' />
                </targets>
                <rules>
                    <logger minlevel='Info' writeTo='debug1' />
                    <logger minlevel='Info' writeTo='debug2' />
                </rules>
            </nlog>").LogFactory;

            var t = (DebugTarget)logFactory.Configuration.AllTargets[0];
            var elr = ((SimpleLayout)t.Layout).Renderers[0] as ExceptionLayoutRenderer;
            Assert.Equal("\r\n----INNER----\r\n", elr.InnerExceptionSeparator);

            string exceptionMessage = "Test exception";
            const string exceptionDataKey = "testkey";
            const string exceptionDataValue = "testvalue";
            Exception ex = GetNestedExceptionWithStackTrace(exceptionMessage);
            ex.InnerException.Data.Add(exceptionDataKey, exceptionDataValue);
            logFactory.GetCurrentClassLogger().Error(ex, "msg");

            logFactory.AssertDebugLastMessage("debug1", "ApplicationException Wrapper2" +
                                             "\r\n----INNER----\r\n" +
                                             "System.ArgumentException Wrapper1");
            logFactory.AssertDebugLastMessage("debug2", string.Format("ApplicationException Wrapper2" +
                                                           "\r\n----INNER----\r\n" +
                                                           "System.ArgumentException Wrapper1 " + ExceptionDataFormat, exceptionDataKey, exceptionDataValue));
        }

        [Fact]
        public void ErrorException_should_not_throw_exception_when_exception_message_property_throw_exception()
        {
            var logFactory = new LogFactory().Setup().LoadConfigurationFromXml(@"
            <nlog>
                <targets>
                    <target name='debug' type='Debug' layout='${exception}' />
                </targets>
                <rules>
                    <logger minlevel='Info' writeTo='debug' />
                </rules>
            </nlog>").LogFactory;
            var logger = logFactory.GetCurrentClassLogger();

            var ex = new ExceptionWithBrokenMessagePropertyException();
            var exRecorded = Record.Exception(() => logger.Error(ex, "msg"));
            Assert.Null(exRecorded);
        }

        [Fact]
        public void ErrorException_should_not_throw_exception_when_exception_message_property_throw_exception_serialize()
        {
            var logFactory = new LogFactory().Setup().LoadConfigurationFromXml(@"
            <nlog>
                <targets>
                    <target name='debug' type='Debug' layout='${exception:format=@}' />
                </targets>
                <rules>
                    <logger minlevel='Info' writeTo='debug' />
                </rules>
            </nlog>").LogFactory;
            var logger = logFactory.GetCurrentClassLogger();

            var ex = new ExceptionWithBrokenMessagePropertyException();
            var exRecorded = Record.Exception(() => logger.Error(ex, "msg"));
            Assert.Null(exRecorded);
        }

#if NET35
        [Fact(Skip = "NET35 not supporting AggregateException")]
#else
        [Fact]
#endif
        public void AggregateExceptionMultiTest()
        {
            var logFactory = new LogFactory().Setup().LoadConfigurationFromXml(@"
            <nlog>
                <targets>
                    <target name='debug' type='Debug' layout='${exception:format=shorttype,message:maxInnerExceptionLevel=5}' />
                </targets>
                <rules>
                    <logger minlevel='Info' writeTo='debug' />
                </rules>
            </nlog>").LogFactory;

            var task1 = System.Threading.Tasks.Task.Factory.StartNew(() => { throw new Exception("Test exception 1", new Exception("Test Inner 1")); },
                System.Threading.CancellationToken.None, System.Threading.Tasks.TaskCreationOptions.None, System.Threading.Tasks.TaskScheduler.Default);
            var task2 = System.Threading.Tasks.Task.Factory.StartNew(() => { throw new Exception("Test exception 2", new Exception("Test Inner 2")); },
                System.Threading.CancellationToken.None, System.Threading.Tasks.TaskCreationOptions.None, System.Threading.Tasks.TaskScheduler.Default);

            var aggregateExceptionMessage = "nothing thrown!";
            try
            {
                System.Threading.Tasks.Task.WaitAll(new[] { task1, task2 });
            }
            catch (AggregateException ex)
            {
                aggregateExceptionMessage = ex.ToString();
                logFactory.GetCurrentClassLogger().Error(ex, "msg");
            }

            Assert.Contains("Test exception 1", aggregateExceptionMessage);
            Assert.Contains("Test exception 2", aggregateExceptionMessage);
            Assert.Contains("Test Inner 1", aggregateExceptionMessage);
            Assert.Contains("Test Inner 2", aggregateExceptionMessage);

            logFactory.AssertDebugLastMessageContains("AggregateException");
            logFactory.AssertDebugLastMessageContains("One or more errors occurred");
            logFactory.AssertDebugLastMessageContains("Test exception 1");
            logFactory.AssertDebugLastMessageContains("Test exception 2");
            logFactory.AssertDebugLastMessageContains("Test Inner 1");
            logFactory.AssertDebugLastMessageContains("Test Inner 2");
        }

#if NET35
        [Fact(Skip = "NET35 not supporting AggregateException")]
#else
        [Fact]
#endif
        public void AggregateExceptionWithExceptionDataMultiTest()
        {
            var logFactory = new LogFactory().Setup().LoadConfigurationFromXml(@"
            <nlog>
                <targets>
                    <target name='debug' type='Debug' layout='${exception:format=shorttype,data,message:maxInnerExceptionLevel=5}' />
                </targets>
                <rules>
                    <logger minlevel='Info' writeTo='debug' />
                </rules>
            </nlog>").LogFactory;

            string exceptionData1Key = "ex1Key";
            string exceptionData1Value = "ex1Value";
            var task1 = System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                var ex1 = new Exception("Test exception 1", new Exception("Test Inner 1"));
                ex1.Data.Add(exceptionData1Key, exceptionData1Value);
                throw ex1;
            },
                System.Threading.CancellationToken.None, System.Threading.Tasks.TaskCreationOptions.None, System.Threading.Tasks.TaskScheduler.Default);

            string exceptionData2Key = "ex2Key";
            string exceptionData2Value = "ex2Value";
            var task2 = System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                var ex2 = new Exception("Test exception 2", new Exception("Test Inner 2"));
                ex2.Data.Add(exceptionData2Key, exceptionData2Value);
                throw ex2;
            },
                System.Threading.CancellationToken.None, System.Threading.Tasks.TaskCreationOptions.None, System.Threading.Tasks.TaskScheduler.Default);

            string aggregateExceptionDataKey = "aggreKey";
            string aggregateExceptionDataValue = "aggreValue";
            var aggregateExceptionMessage = "nothing thrown!";
            try
            {
                System.Threading.Tasks.Task.WaitAll(new[] { task1, task2 });
            }
            catch (AggregateException ex)
            {
                ex.Data.Add(aggregateExceptionDataKey, aggregateExceptionDataValue);
                aggregateExceptionMessage = ex.ToString();
                logFactory.GetCurrentClassLogger().Error(ex, "msg");
            }

            Assert.Contains("Test exception 1", aggregateExceptionMessage);
            Assert.Contains("Test exception 2", aggregateExceptionMessage);
            Assert.Contains("Test Inner 1", aggregateExceptionMessage);
            Assert.Contains("Test Inner 2", aggregateExceptionMessage);

            logFactory.AssertDebugLastMessageContains("AggregateException");
            logFactory.AssertDebugLastMessageContains("One or more errors occurred");
            logFactory.AssertDebugLastMessageContains("Test exception 1");
            logFactory.AssertDebugLastMessageContains("Test exception 2");
            logFactory.AssertDebugLastMessageContains("Test Inner 1");
            logFactory.AssertDebugLastMessageContains("Test Inner 2");
            logFactory.AssertDebugLastMessageContains(string.Format(ExceptionDataFormat, exceptionData1Key, exceptionData1Value));
            logFactory.AssertDebugLastMessageContains(string.Format(ExceptionDataFormat, exceptionData2Key, exceptionData2Value));
            logFactory.AssertDebugLastMessageContains(string.Format(ExceptionDataFormat, aggregateExceptionDataKey, aggregateExceptionDataValue));
        }

#if NET35
        [Fact(Skip = "NET35 not supporting AggregateException")]
#else
        [Fact]
#endif
        public void AggregateExceptionSingleTest()
        {
            var logFactory = new LogFactory().Setup().LoadConfigurationFromXml(@"
            <nlog>
                <targets>
                    <target name='debug' type='Debug' layout='${exception:format=message,shorttype:maxInnerExceptionLevel=5}' />
                </targets>
                <rules>
                    <logger minlevel='Info' writeTo='debug' />
                </rules>
            </nlog>").LogFactory;

            var task1 = System.Threading.Tasks.Task.Factory.StartNew(() => { throw new Exception("Test exception 1", new Exception("Test Inner 1")); },
                System.Threading.CancellationToken.None, System.Threading.Tasks.TaskCreationOptions.None, System.Threading.Tasks.TaskScheduler.Default);

            var aggregateExceptionMessage = "nothing thrown!";
            try
            {
                System.Threading.Tasks.Task.WaitAll(new[] { task1 });
            }
            catch (AggregateException ex)
            {
                aggregateExceptionMessage = ex.ToString();
                logFactory.GetCurrentClassLogger().Error(ex, "msg");
            }

            Assert.Contains(typeof(AggregateException).Name, aggregateExceptionMessage);
            Assert.Contains("Test exception 1", aggregateExceptionMessage);
            Assert.Contains("Test Inner 1", aggregateExceptionMessage);

            var lastMessage = logFactory.AssertDebugLastMessageNotEmpty();
            Assert.StartsWith("Test exception 1", lastMessage);
            Assert.Contains("Test Inner 1", lastMessage);
        }

#if NET35
        [Fact(Skip = "NET35 not supporting AggregateException")]
#else
        [Fact]
#endif
        public void AggregateExceptionWithExceptionDataSingleTest()
        {
            var logFactory = new LogFactory().Setup().LoadConfigurationFromXml(@"
            <nlog>
                <targets>
                    <target name='debug' type='Debug' layout='${exception:format=message,data,shorttype:maxInnerExceptionLevel=5}' />
                </targets>
                <rules>
                    <logger minlevel='Info' writeTo='debug' />
                </rules>
            </nlog>").LogFactory;

            string exceptionDataKey = "ex1Key";
            string exceptionDataValue = "ex1Value";
            var task1 = System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                var ex = new Exception("Test exception 1", new Exception("Test Inner 1"));
                ex.Data.Add(exceptionDataKey, exceptionDataValue);
                throw ex;
            },
                System.Threading.CancellationToken.None, System.Threading.Tasks.TaskCreationOptions.None, System.Threading.Tasks.TaskScheduler.Default);

            string aggregateExceptionDataKey = "aggreKey";
            string aggregateExceptionDataValue = "aggreValue";
            var aggregateExceptionMessage = "nothing thrown!";
            try
            {
                System.Threading.Tasks.Task.WaitAll(new[] { task1 });
            }
            catch (AggregateException ex)
            {
                ex.Data.Add(aggregateExceptionDataKey, aggregateExceptionDataValue);
                aggregateExceptionMessage = ex.ToString();
                logFactory.GetCurrentClassLogger().Error(ex, "msg");
            }

            Assert.Contains(typeof(AggregateException).Name, aggregateExceptionMessage);
            Assert.Contains("Test exception 1", aggregateExceptionMessage);
            Assert.Contains("Test Inner 1", aggregateExceptionMessage);

            var lastMessage = logFactory.AssertDebugLastMessageNotEmpty();
            Assert.StartsWith("Test exception 1", lastMessage);
            Assert.Contains("Test Inner 1", lastMessage);
            Assert.Contains(string.Format(ExceptionDataFormat, exceptionDataKey, exceptionDataValue), lastMessage);
            Assert.Contains(string.Format(ExceptionDataFormat, aggregateExceptionDataKey, aggregateExceptionDataValue), lastMessage);
        }

        [Fact]
        public void CustomExceptionProperties_Layout_Test()
        {
            var logFactory = new LogFactory().Setup().LoadConfigurationFromXml(@"
            <nlog>
                <targets>
                    <target name='debug' type='Debug' layout='${exception:format=Properties}' />
                </targets>
                <rules>
                    <logger minlevel='Info' writeTo='debug' />
                </rules>
            </nlog>").LogFactory;

            var ex = new CustomArgumentException("Goodbye World", "Nuke");
            logFactory.GetCurrentClassLogger().Fatal(ex, "msg");
            logFactory.AssertDebugLastMessage($"{nameof(CustomArgumentException.ParamName)}: Nuke");
        }

        [Fact]
        public void BaseExceptionTest()
        {
            var logFactory = new LogFactory().Setup().LoadConfigurationFromXml(@"
            <nlog>
                <targets>
                    <target name='debug' type='Debug' layout='${exception:format=Message:BaseException=true}' />
                </targets>
                <rules>
                    <logger minlevel='Info' writeTo='debug' />
                </rules>
            </nlog>").LogFactory;

            var ex = GetNestedExceptionWithStackTrace("Goodbye World");
            logFactory.GetCurrentClassLogger().Fatal(ex, "msg");
            logFactory.AssertDebugLastMessage("Goodbye World");
        }

#if NET35
        [Fact(Skip = "NET35 not supporting AggregateException")]
#else
        [Fact]
#endif
        public void RecursiveAsyncExceptionWithoutFlattenException()
        {
            var recursionCount = 3;
            Func<int> innerAction = () => throw new ApplicationException("Life is hard");
            var t1 = System.Threading.Tasks.Task<int>.Factory.StartNew(() =>
            {
                return NestedFunc(recursionCount, innerAction);
            });

            try
            {
                t1.Wait();
            }
            catch (AggregateException ex)
            {
                var layoutRenderer = new ExceptionLayoutRenderer() { Format = "ToString", FlattenException = false };
                var logEvent = LogEventInfo.Create(LogLevel.Error, null, null, (object)ex);
                var result = layoutRenderer.Render(logEvent);
                int needleCount = 0;
                int foundIndex = result.IndexOf(nameof(NestedFunc), 0);
                while (foundIndex >= 0)
                {
                    ++needleCount;
                    foundIndex = result.IndexOf(nameof(NestedFunc), foundIndex + nameof(NestedFunc).Length);
                }
                Assert.True(needleCount >= recursionCount, $"{needleCount} too small");
            }
        }

        private class ExceptionWithBrokenMessagePropertyException : NLogConfigurationException
        {
            public override string Message => throw new Exception("Exception from Message property");
        }

        private static LogFactory BuildConfigurationForExceptionTests()
        {
            return new LogFactory().Setup().LoadConfigurationFromXml(@"
            <nlog>
                <targets>
                    <target name='debug' type='Debug' layout='${level:uppercase=true}*${message}*${exception:format=message,shorttype:separator=*}' />
                </targets>
                <rules>
                    <logger minlevel='Trace' writeTo='debug' />
                </rules>
            </nlog>").LogFactory;
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

        private static Exception GetNestedExceptionWithStackTrace(string exceptionMessage)
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

        private static Exception GetExceptionWithoutStackTrace(string exceptionMessage)
        {
            return new CustomArgumentException(exceptionMessage, "exceptionMessage");
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

        [Fact]
        public void ExcpetionTestAPI()
        {
            var logFactory = new LogFactory().Setup().LoadConfiguration(builder =>
            {
                var debugTarget = new DebugTarget("debug");
                debugTarget.Layout = @"${exception:format=shorttype,message:maxInnerExceptionLevel=3}";
                builder.ForLogger().WriteTo(debugTarget);
            }).LogFactory;

            string exceptionMessage = "Test exception";
            Exception ex = GetNestedExceptionWithStackTrace(exceptionMessage);

            logFactory.GetCurrentClassLogger().Error(ex, "msg");
            logFactory.AssertDebugLastMessage("ApplicationException Wrapper2" + EnvironmentHelper.NewLine +
                                             "ArgumentException Wrapper1" + EnvironmentHelper.NewLine +
                                             "CustomArgumentException Test exception");

            var t = (DebugTarget)logFactory.Configuration.AllTargets[0];
            var elr = ((SimpleLayout)t.Layout).Renderers[0] as ExceptionLayoutRenderer;

            Assert.Equal(ExceptionRenderingFormat.ShortType, elr.Formats[0]);
            Assert.Equal(ExceptionRenderingFormat.Message, elr.Formats[1]);
        }

        [Fact]
        public void InnerExceptionTestAPI()
        {
            var logFactory = new LogFactory().Setup().LoadConfigurationFromXml(@"
            <nlog>
                <targets>
                    <target name='debug' type='Debug' layout='${exception:format=shorttype,message:maxInnerExceptionLevel=3:innerFormat=message}' />
                </targets>
                <rules>
                    <logger minlevel='Info' writeTo='debug' />
                </rules>
            </nlog>").LogFactory;

            string exceptionMessage = "Test exception";
            Exception ex = GetNestedExceptionWithStackTrace(exceptionMessage);

            logFactory.GetCurrentClassLogger().Error(ex, "msg");
            logFactory.AssertDebugLastMessage("ApplicationException Wrapper2" + EnvironmentHelper.NewLine +
                                             "Wrapper1" + EnvironmentHelper.NewLine +
                                             "Test exception");

            var t = (DebugTarget)logFactory.Configuration.AllTargets[0];
            var elr = ((SimpleLayout)t.Layout).Renderers[0] as ExceptionLayoutRenderer;

            Assert.Equal(ExceptionRenderingFormat.ShortType, elr.Formats[0]);
            Assert.Equal(ExceptionRenderingFormat.Message, elr.Formats[1]);

            Assert.Equal(ExceptionRenderingFormat.Message, elr.InnerFormats[0]);
        }

        [Fact]
        public void CustomExceptionLayoutRendrerInnerExceptionTest()
        {
            var logFactory = new LogFactory().Setup()
                .SetupExtensions(ext => ext.RegisterLayoutRenderer<CustomExceptionLayoutRendrer>("exception-custom"))
                .LoadConfigurationFromXml(@"
            <nlog>
                <targets>
                    <target name='debug1' type='Debug' layout='${exception-custom:format=shorttype,message:maxInnerExceptionLevel=1:innerExceptionSeparator=&#13;&#10;----INNER----&#13;&#10;:innerFormat=type,message}' />
                    <target name='debug2' type='Debug' layout='${exception-custom:format=shorttype,message:maxInnerExceptionLevel=1:innerExceptionSeparator=&#13;&#10;----INNER----&#13;&#10;:innerFormat=type,message,data}' />
                </targets>
                <rules>
                    <logger minlevel='Info' writeTo='debug1' />
                    <logger minlevel='Info' writeTo='debug2' />
                </rules>
            </nlog>").LogFactory;

            var t = (DebugTarget)logFactory.Configuration.AllTargets[0];
            var elr = ((SimpleLayout)t.Layout).Renderers[0] as CustomExceptionLayoutRendrer;
            Assert.Equal("\r\n----INNER----\r\n", elr.InnerExceptionSeparator);

            string exceptionMessage = "Test exception";
            const string exceptionDataKey = "testkey";
            const string exceptionDataValue = "testvalue";
            Exception ex = GetNestedExceptionWithStackTrace(exceptionMessage);
            ex.InnerException.Data.Add(exceptionDataKey, exceptionDataValue);
            logFactory.GetCurrentClassLogger().Error(ex, "msg");
            logFactory.AssertDebugLastMessage("debug1", "ApplicationException Wrapper2" + "\r\ncustom-exception-renderer" +
                                             "\r\n----INNER----\r\n" +
                                             "System.ArgumentException Wrapper1" + "\r\ncustom-exception-renderer");
            logFactory.AssertDebugLastMessage("debug2", string.Format("ApplicationException Wrapper2" + "\r\ncustom-exception-renderer" +
                                                           "\r\n----INNER----\r\n" +
                                                           "System.ArgumentException Wrapper1" + "\r\ncustom-exception-renderer " + ExceptionDataFormat, exceptionDataKey, exceptionDataValue + "\r\ncustom-exception-renderer-data"));
        }

        [Fact]
        public void ExceptionDataWithDifferentSeparators()
        {
            var logFactory = new LogFactory().Setup().LoadConfigurationFromXml(@"
            <nlog>
                <targets>                    
                    <target name='debug1' type='Debug' layout='${exception:format=data}' />
                    <target name='debug2' type='Debug' layout='${exception:format=data:ExceptionDataSeparator=*}' />
                    <target name='debug3' type='Debug' layout='${exception:format=data:ExceptionDataSeparator=## **}' />
                </targets>
                <rules>
                    <logger minlevel='Info' writeTo='debug1, debug2, debug3' />
                </rules>
            </nlog>").LogFactory;

            const string defaultExceptionDataSeparator = ";";
            const string exceptionMessage = "message for exception";
            const string exceptionDataKey1 = "testkey1";
            const string exceptionDataValue1 = "testvalue1";
            const string exceptionDataKey2 = "testkey2";
            const string exceptionDataValue2 = "testvalue2";

            var target = (DebugTarget)logFactory.Configuration.AllTargets[0];
            var exceptionLayoutRenderer = ((SimpleLayout)target.Layout).Renderers[0] as ExceptionLayoutRenderer;
            Assert.NotNull(exceptionLayoutRenderer);
            Assert.Equal(defaultExceptionDataSeparator, exceptionLayoutRenderer.ExceptionDataSeparator);

            Exception ex = GetExceptionWithStackTrace(exceptionMessage);
            ex.Data.Add(exceptionDataKey1, exceptionDataValue1);
            ex.Data.Add(exceptionDataKey2, exceptionDataValue2);

            logFactory.GetCurrentClassLogger().Error(ex);

            logFactory.AssertDebugLastMessage("debug1", string.Format(ExceptionDataFormat, exceptionDataKey1, exceptionDataValue1) + defaultExceptionDataSeparator + string.Format(ExceptionDataFormat, exceptionDataKey2, exceptionDataValue2));
            logFactory.AssertDebugLastMessage("debug2", string.Format(ExceptionDataFormat, exceptionDataKey1, exceptionDataValue1) + "*" + string.Format(ExceptionDataFormat, exceptionDataKey2, exceptionDataValue2));
            logFactory.AssertDebugLastMessage("debug3", string.Format(ExceptionDataFormat, exceptionDataKey1, exceptionDataValue1) + "## **" + string.Format(ExceptionDataFormat, exceptionDataKey2, exceptionDataValue2));
        }

        [Fact]
        public void ExceptionDataWithNewLineSeparator()
        {
            var logFactory = new LogFactory().Setup().LoadConfigurationFromXml(@"
            <nlog>
                <targets>                                        
                    <target name='debug1' type='Debug' layout='${exception:format=data:ExceptionDataSeparator=\r\n}' />
                    <target name='debug2' type='Debug' layout='${exception:format=data:ExceptionDataSeparator=\r\n----DATA----\r\n}' />
                    <target name='debug3' type='Debug' layout='${exception:format=data:ExceptionDataSeparator=&#13;&#10;----DATA----&#13;&#10;}' />
                </targets>
                <rules>
                    <logger minlevel='Info' writeTo='debug1, debug2, debug3' />
                </rules>
            </nlog>").LogFactory;

            const string exceptionMessage = "message for exception";
            const string exceptionDataKey1 = "testkey1";
            const string exceptionDataValue1 = "testvalue1";
            const string exceptionDataKey2 = "testkey2";
            const string exceptionDataValue2 = "testvalue2";

            Exception ex = GetExceptionWithStackTrace(exceptionMessage);
            ex.Data.Add(exceptionDataKey1, exceptionDataValue1);
            ex.Data.Add(exceptionDataKey2, exceptionDataValue2);

            logFactory.GetCurrentClassLogger().Error(ex);

            logFactory.AssertDebugLastMessage("debug1", string.Format(ExceptionDataFormat, exceptionDataKey1, exceptionDataValue1) + "\r\n" + string.Format(ExceptionDataFormat, exceptionDataKey2, exceptionDataValue2));
            logFactory.AssertDebugLastMessage("debug2", string.Format(ExceptionDataFormat, exceptionDataKey1, exceptionDataValue1) + "\r\n----DATA----\r\n" + string.Format(ExceptionDataFormat, exceptionDataKey2, exceptionDataValue2));
            logFactory.AssertDebugLastMessage("debug3", string.Format(ExceptionDataFormat, exceptionDataKey1, exceptionDataValue1) + "\r\n----DATA----\r\n" + string.Format(ExceptionDataFormat, exceptionDataKey2, exceptionDataValue2));
        }


        [Fact]
        public void ExceptionWithSeparatorForExistingRender()
        {
            var logFactory = new LogFactory().Setup().LoadConfigurationFromXml(@"
            <nlog>
                <targets>                                        
                    <target name='debug' type='Debug' layout='${exception:format=tostring,data:separator=\r\nXXX}' />
                </targets>
                <rules>
                    <logger minlevel='Info' writeTo='debug' />
                </rules>
            </nlog>").LogFactory;

            const string exceptionMessage = "message for exception";
            const string exceptionDataKey1 = "testkey1";
            const string exceptionDataValue1 = "testvalue1";

            Exception ex = GetExceptionWithoutStackTrace(exceptionMessage);
            ex.Data.Add(exceptionDataKey1, exceptionDataValue1);

            logFactory.GetCurrentClassLogger().Error(ex);

            logFactory.AssertDebugLastMessage(string.Format(ExceptionDataFormat, ex.GetType().FullName, exceptionMessage) + "\r\nXXX" + string.Format(ExceptionDataFormat, exceptionDataKey1, exceptionDataValue1));
        }

        [Fact]
        public void ExceptionWithSeparatorForExistingBetweenRender()
        {
            var logFactory = new LogFactory().Setup().LoadConfigurationFromXml(@"
            <nlog>
                <targets>                                        
                    <target name='debug' type='Debug' layout='${exception:format=tostring,data,type:separator=\r\nXXX}' />
                </targets>
                <rules>
                    <logger minlevel='Info' writeTo='debug' />
                </rules>
            </nlog>").LogFactory;

            const string exceptionMessage = "message for exception";
            const string exceptionDataKey1 = "testkey1";
            const string exceptionDataValue1 = "testvalue1";

            Exception ex = GetExceptionWithoutStackTrace(exceptionMessage);
            ex.Data.Add(exceptionDataKey1, exceptionDataValue1);

            logFactory.GetCurrentClassLogger().Error(ex);

            logFactory.AssertDebugLastMessage(
                string.Format(ExceptionDataFormat, ex.GetType().FullName, exceptionMessage) +
                "\r\nXXX" +
                string.Format(ExceptionDataFormat, exceptionDataKey1, exceptionDataValue1) +
                "\r\nXXX" +
                ex.GetType().FullName);
        }

        [Fact]
        public void ExceptionWithoutSeparatorForNoRender()
        {
            var logFactory = new LogFactory().Setup().LoadConfigurationFromXml(@"
            <nlog>
                <targets>                                        
                    <target name='debug' type='Debug' layout='${exception:format=tostring,data:separator=\r\nXXX}' />
                </targets>
                <rules>
                    <logger minlevel='Info' writeTo='debug' />
                </rules>
            </nlog>").LogFactory;

            const string exceptionMessage = "message for exception";

            Exception ex = GetExceptionWithoutStackTrace(exceptionMessage);

            logFactory.GetCurrentClassLogger().Error(ex);

            logFactory.AssertDebugLastMessage(string.Format(ExceptionDataFormat, ex.GetType().FullName, exceptionMessage));
        }


        [Fact]
        public void ExceptionWithoutSeparatorForNoBetweenRender()
        {
            var logFactory = new LogFactory().Setup().LoadConfigurationFromXml(@"
            <nlog>
                <targets>
                    <target name='debug' type='Debug' layout='${exception:format=tostring,data,type:separator=\r\nXXX}' />
                </targets>
                <rules>
                    <logger minlevel='Info' writeTo='debug' />
                </rules>
            </nlog>").LogFactory;

            const string exceptionMessage = "message for exception";

            Exception ex = GetExceptionWithoutStackTrace(exceptionMessage);

            logFactory.GetCurrentClassLogger().Error(ex);

            logFactory.AssertDebugLastMessage(
                string.Format(ExceptionDataFormat, ex.GetType().FullName, exceptionMessage) +
                "\r\nXXX" +
                ex.GetType().FullName);
    }
    }

    [LayoutRenderer("exception-custom")]
    [ThreadAgnostic]
    public class CustomExceptionLayoutRendrer : ExceptionLayoutRenderer
    {
        protected override void AppendMessage(System.Text.StringBuilder sb, Exception ex)
        {
            base.AppendMessage(sb, ex);
            sb.Append("\r\ncustom-exception-renderer");
        }

        protected override void AppendData(System.Text.StringBuilder sb, Exception ex)
        {
            base.AppendData(sb, ex);
            sb.Append("\r\ncustom-exception-renderer-data");
        }
    }
}