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

    public class ExceptionTests : NLogTestBase
    {
        const int E_FAIL = 80004005;

        private ILogger logger = LogManager.GetLogger("NLog.UnitTests.LayoutRenderer.ExceptionTests");
        private const string ExceptionDataFormat = "{0}: {1}";

        [Fact]
        public void ExceptionWithStackTraceTest()
        {
            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
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
            </nlog>");

            const string exceptionMessage = "Test exception";
            const string exceptionDataKey = "testkey";
            const string exceptionDataValue = "testvalue";
            Exception ex = GetExceptionWithStackTrace(exceptionMessage);
            ex.Data.Add(exceptionDataKey, exceptionDataValue);
            logger.Error(ex, "msg");
            var dataText = string.Format(ExceptionDataFormat, exceptionDataKey, exceptionDataValue);
            AssertDebugLastMessage("debug1", ex.ToString() + " " + dataText);
            AssertDebugLastMessage("debug2", ex.StackTrace);
            AssertDebugLastMessage("debug3", typeof(CustomArgumentException).FullName);
            AssertDebugLastMessage("debug4", typeof(CustomArgumentException).Name);
            AssertDebugLastMessage("debug5", ex.ToString());
            AssertDebugLastMessage("debug6", exceptionMessage);
            AssertDebugLastMessage("debug10", GetType().ToString());
#if !NET35 && !NET40
            AssertDebugLastMessage("debug11", $"0x{E_FAIL:X8}");
#endif

            AssertDebugLastMessage("debug9", dataText);

            // each version of the framework produces slightly different information for MethodInfo, so we just 
            // make sure it's not empty
            var debug7Target = (DebugTarget)LogManager.Configuration.FindTargetByName("debug7");
            Assert.False(string.IsNullOrEmpty(debug7Target.LastMessage));

            AssertDebugLastMessage("debug8", exceptionMessage + "*" + typeof(CustomArgumentException).Name);
        }

        /// <summary>
        /// Just wrrite exception, no message argument.
        /// </summary>
        [Fact]
        public void ExceptionWithoutMessageParam()
        {
            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
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
            </nlog>");

            const string exceptionMessage = "I don't like nullref exception!";
            const string exceptionDataKey = "testkey";
            const string exceptionDataValue = "testvalue";
            Exception ex = GetExceptionWithStackTrace(exceptionMessage);
            ex.Data.Add(exceptionDataKey, exceptionDataValue);
            logger.Error(ex);
            var dataText = string.Format(ExceptionDataFormat, exceptionDataKey, exceptionDataValue);
            AssertDebugLastMessage("debug1", ex.ToString() + " " + dataText);
            AssertDebugLastMessage("debug2", ex.StackTrace);
            AssertDebugLastMessage("debug3", typeof(CustomArgumentException).FullName);
            AssertDebugLastMessage("debug4", typeof(CustomArgumentException).Name);
            AssertDebugLastMessage("debug5", ex.ToString());
            AssertDebugLastMessage("debug6", exceptionMessage);
            AssertDebugLastMessage("debug9", dataText);
            AssertDebugLastMessage("debug10", GetType().ToString());
#if !NET35 && !NET40
            AssertDebugLastMessage("debug11", $"0x{E_FAIL:X8}");
#endif

            // each version of the framework produces slightly different information for MethodInfo, so we just 
            // make sure it's not empty
            var debug7Target = (DebugTarget)LogManager.Configuration.FindTargetByName("debug7");
            Assert.False(string.IsNullOrEmpty(debug7Target.LastMessage));

            AssertDebugLastMessage("debug8", exceptionMessage + "*" + typeof(CustomArgumentException).Name);
        }

        [Fact]
        public void ExceptionWithoutStackTraceTest()
        {
            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
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
            </nlog>");

            const string exceptionMessage = "Test exception";
            const string exceptionDataKey = "testkey";
            const string exceptionDataValue = "testvalue";
            Exception ex = GetExceptionWithoutStackTrace(exceptionMessage);
            ex.Data.Add(exceptionDataKey, exceptionDataValue);
            logger.Error(ex, "msg");
            var dataText = string.Format(ExceptionDataFormat, exceptionDataKey, exceptionDataValue);
            AssertDebugLastMessage("debug1", ex.ToString() + " " + dataText);
            AssertDebugLastMessage("debug2", "");
            AssertDebugLastMessage("debug3", typeof(CustomArgumentException).FullName);
            AssertDebugLastMessage("debug4", typeof(CustomArgumentException).Name);
            AssertDebugLastMessage("debug5", ex.ToString());
            AssertDebugLastMessage("debug6", exceptionMessage);
            AssertDebugLastMessage("debug7", "");
            AssertDebugLastMessage("debug8", "Test exception*" + typeof(CustomArgumentException).Name);
            AssertDebugLastMessage("debug9", dataText);
            AssertDebugLastMessage("debug10", "");
#if !NET35 && !NET40
            AssertDebugLastMessage("debug11", $"0x{E_FAIL:X8}");
#endif
        }

        [Fact]
        public void ExceptionNewLineSeparatorTest()
        {
            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
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

            logger.Error(ex, "msg");
            AssertDebugLastMessage("debug1", "Test exception\r\n" + typeof(CustomArgumentException).Name);
        }

        [Fact]
        public void ExceptionUsingLogMethodTest()
        {
            SetConfigurationForExceptionUsingRootMethodTests();
            string exceptionMessage = "Test exception";
            Exception ex = GetExceptionWithStackTrace(exceptionMessage);
            logger.Log(LogLevel.Error, ex, "msg");
            AssertDebugLastMessage("debug1", "ERROR*msg*Test exception*" + typeof(CustomArgumentException).Name);

            logger.Log(LogLevel.Error, ex, () => "msg func");
            AssertDebugLastMessage("debug1", "ERROR*msg func*Test exception*" + typeof(CustomArgumentException).Name);
        }

        [Fact]
        public void ExceptionUsingTraceMethodTest()
        {
            SetConfigurationForExceptionUsingRootMethodTests();
            string exceptionMessage = "Test exception";
            Exception ex = GetExceptionWithStackTrace(exceptionMessage);
            logger.Trace(ex, "msg");
            AssertDebugLastMessage("debug1", "TRACE*msg*Test exception*" + typeof(CustomArgumentException).Name);

            logger.Trace(ex, () => "msg func");
            AssertDebugLastMessage("debug1", "TRACE*msg func*Test exception*" + typeof(CustomArgumentException).Name);
        }

        [Fact]
        public void ExceptionUsingDebugMethodTest()
        {
            SetConfigurationForExceptionUsingRootMethodTests();
            string exceptionMessage = "Test exception";
            Exception ex = GetExceptionWithStackTrace(exceptionMessage);
            logger.Debug(ex, "msg");
            AssertDebugLastMessage("debug1", "DEBUG*msg*Test exception*" + typeof(CustomArgumentException).Name);

            logger.Debug(ex, () => "msg func");
            AssertDebugLastMessage("debug1", "DEBUG*msg func*Test exception*" + typeof(CustomArgumentException).Name);
        }

        [Fact]
        public void ExceptionUsingInfoMethodTest()
        {
            SetConfigurationForExceptionUsingRootMethodTests();
            string exceptionMessage = "Test exception";
            Exception ex = GetExceptionWithStackTrace(exceptionMessage);
            logger.Info(ex, "msg");
            AssertDebugLastMessage("debug1", "INFO*msg*Test exception*" + typeof(CustomArgumentException).Name);

            logger.Info(ex, () => "msg func");
            AssertDebugLastMessage("debug1", "INFO*msg func*Test exception*" + typeof(CustomArgumentException).Name);
        }

        [Fact]
        public void ExceptionUsingWarnMethodTest()
        {
            SetConfigurationForExceptionUsingRootMethodTests();
            string exceptionMessage = "Test exception";
            Exception ex = GetExceptionWithStackTrace(exceptionMessage);
            logger.Warn(ex, "msg");
            AssertDebugLastMessage("debug1", "WARN*msg*Test exception*" + typeof(CustomArgumentException).Name);

            logger.Warn(ex, () => "msg func");
            AssertDebugLastMessage("debug1", "WARN*msg func*Test exception*" + typeof(CustomArgumentException).Name);
        }

        [Fact]
        public void ExceptionUsingErrorMethodTest()
        {
            SetConfigurationForExceptionUsingRootMethodTests();
            string exceptionMessage = "Test exception";
            Exception ex = GetExceptionWithStackTrace(exceptionMessage);
            logger.Error(ex, "msg");
            AssertDebugLastMessage("debug1", "ERROR*msg*Test exception*" + typeof(CustomArgumentException).Name);

            logger.Error(ex, () => "msg func");
            AssertDebugLastMessage("debug1", "ERROR*msg func*Test exception*" + typeof(CustomArgumentException).Name);
        }

        [Fact]
        public void ExceptionUsingFatalMethodTest()
        {
            SetConfigurationForExceptionUsingRootMethodTests();
            string exceptionMessage = "Test exception";
            Exception ex = GetExceptionWithStackTrace(exceptionMessage);
            logger.Fatal(ex, "msg");
            AssertDebugLastMessage("debug1", "FATAL*msg*Test exception*" + typeof(CustomArgumentException).Name);

            logger.Fatal(ex, () => "msg func");
            AssertDebugLastMessage("debug1", "FATAL*msg func*Test exception*" + typeof(CustomArgumentException).Name);
        }

        [Fact]
        public void InnerExceptionTest()
        {
            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
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

            logger.Error(ex, "msg");
            AssertDebugLastMessage("debug1", "ApplicationException Wrapper2" + EnvironmentHelper.NewLine +
                                             "ArgumentException Wrapper1" + EnvironmentHelper.NewLine +
                                             "CustomArgumentException Test exception");
        }

        [Fact]
        public void InnerExceptionTest_Serialize()
        {
            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
            <nlog>
                <targets>
                    <target name='debug1' type='Debug' layout='${exception:format=@}' />
                </targets>
                <rules>
                    <logger minlevel='Info' writeTo='debug1' />
                </rules>
            </nlog>");

            string exceptionMessage = "Test exception";
            Exception ex = GetNestedExceptionWithStackTrace(exceptionMessage);

            logger.Error(ex, "msg");
            var lastMessage = GetDebugLastMessage("debug1");
            Assert.StartsWith("{\"Type\":\"System.ApplicationException\", \"Message\":\"Wrapper2\"", lastMessage);
            Assert.Contains("\"InnerException\":{\"Type\":\"System.ArgumentException\", \"Message\":\"Wrapper1\"", lastMessage);
            Assert.Contains("\"ParamName\":\"exceptionMessage\"", lastMessage);
            Assert.Contains("1Really_Bad_Boy_", lastMessage);
        }

        [Fact]
        public void CustomInnerExceptionTest()
        {
            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
            <nlog>
                <targets>
                    <target name='debug1' type='Debug' layout='${exception:format=shorttype,message:maxInnerExceptionLevel=1:innerExceptionSeparator=&#13;&#10;----INNER----&#13;&#10;:innerFormat=type,message}' />
                    <target name='debug2' type='Debug' layout='${exception:format=shorttype,message:maxInnerExceptionLevel=1:innerExceptionSeparator=&#13;&#10;----INNER----&#13;&#10;:innerFormat=type,message,data}' />
                </targets>
                <rules>
                    <logger minlevel='Info' writeTo='debug1' />
                    <logger minlevel='Info' writeTo='debug2' />
                </rules>
            </nlog>");

            var t = (DebugTarget)LogManager.Configuration.AllTargets[0];
            var elr = ((SimpleLayout)t.Layout).Renderers[0] as ExceptionLayoutRenderer;
            Assert.Equal("\r\n----INNER----\r\n", elr.InnerExceptionSeparator);

            string exceptionMessage = "Test exception";
            const string exceptionDataKey = "testkey";
            const string exceptionDataValue = "testvalue";
            Exception ex = GetNestedExceptionWithStackTrace(exceptionMessage);
            ex.InnerException.Data.Add(exceptionDataKey, exceptionDataValue);
            logger.Error(ex, "msg");
            AssertDebugLastMessage("debug1", "ApplicationException Wrapper2" +
                                             "\r\n----INNER----\r\n" +
                                             "System.ArgumentException Wrapper1");
            AssertDebugLastMessage("debug2", string.Format("ApplicationException Wrapper2" +
                                                           "\r\n----INNER----\r\n" +
                                                           "System.ArgumentException Wrapper1 " + ExceptionDataFormat, exceptionDataKey, exceptionDataValue));
        }

        [Fact]
        public void ErrorException_should_not_throw_exception_when_exception_message_property_throw_exception()
        {
            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
            <nlog>
                <targets>
                    <target name='debug1' type='Debug' layout='${exception}' />
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
                    <target name='debug1' type='Debug' layout='${exception:format=@}' />
                </targets>
                <rules>
                    <logger minlevel='Info' writeTo='debug1' />
                </rules>
            </nlog>");

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
            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
            <nlog>
                <targets>
                    <target name='debug1' type='Debug' layout='${exception:format=shorttype,message:maxInnerExceptionLevel=5}' />
                </targets>
                <rules>
                    <logger minlevel='Info' writeTo='debug1' />
                </rules>
            </nlog>");

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
                logger.Error(ex, "msg");
            }

            Assert.Contains("Test exception 1", aggregateExceptionMessage);
            Assert.Contains("Test exception 2", aggregateExceptionMessage);
            Assert.Contains("Test Inner 1", aggregateExceptionMessage);
            Assert.Contains("Test Inner 2", aggregateExceptionMessage);

            AssertDebugLastMessageContains("debug1", "AggregateException");
            AssertDebugLastMessageContains("debug1", "One or more errors occurred");
            AssertDebugLastMessageContains("debug1", "Test exception 1");
            AssertDebugLastMessageContains("debug1", "Test exception 2");
            AssertDebugLastMessageContains("debug1", "Test Inner 1");
            AssertDebugLastMessageContains("debug1", "Test Inner 2");
        }

#if NET35
        [Fact(Skip = "NET35 not supporting AggregateException")]
#else
        [Fact]
#endif
        public void AggregateExceptionWithExceptionDataMultiTest()
        {
            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
            <nlog>
                <targets>
                    <target name='debug1' type='Debug' layout='${exception:format=shorttype,data,message:maxInnerExceptionLevel=5}' />
                </targets>
                <rules>
                    <logger minlevel='Info' writeTo='debug1' />
                </rules>
            </nlog>");

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
                logger.Error(ex, "msg");
            }

            Assert.Contains("Test exception 1", aggregateExceptionMessage);
            Assert.Contains("Test exception 2", aggregateExceptionMessage);
            Assert.Contains("Test Inner 1", aggregateExceptionMessage);
            Assert.Contains("Test Inner 2", aggregateExceptionMessage);

            AssertDebugLastMessageContains("debug1", "AggregateException");
            AssertDebugLastMessageContains("debug1", "One or more errors occurred");
            AssertDebugLastMessageContains("debug1", "Test exception 1");
            AssertDebugLastMessageContains("debug1", "Test exception 2");
            AssertDebugLastMessageContains("debug1", "Test Inner 1");
            AssertDebugLastMessageContains("debug1", "Test Inner 2");
            AssertDebugLastMessageContains("debug1", string.Format(ExceptionDataFormat, exceptionData1Key, exceptionData1Value));
            AssertDebugLastMessageContains("debug1", string.Format(ExceptionDataFormat, exceptionData2Key, exceptionData2Value));
            AssertDebugLastMessageContains("debug1", string.Format(ExceptionDataFormat, aggregateExceptionDataKey, aggregateExceptionDataValue));
        }

#if NET35
        [Fact(Skip = "NET35 not supporting AggregateException")]
#else
        [Fact]
#endif
        public void AggregateExceptionSingleTest()
        {
            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
            <nlog>
                <targets>
                    <target name='debug1' type='Debug' layout='${exception:format=message,shorttype:maxInnerExceptionLevel=5}' />
                </targets>
                <rules>
                    <logger minlevel='Info' writeTo='debug1' />
                </rules>
            </nlog>");

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
                logger.Error(ex, "msg");
            }

            Assert.Contains(typeof(AggregateException).Name, aggregateExceptionMessage);
            Assert.Contains("Test exception 1", aggregateExceptionMessage);
            Assert.Contains("Test Inner 1", aggregateExceptionMessage);

            var lastMessage = GetDebugLastMessage("debug1");
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
            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
            <nlog>
                <targets>
                    <target name='debug1' type='Debug' layout='${exception:format=message,data,shorttype:maxInnerExceptionLevel=5}' />
                </targets>
                <rules>
                    <logger minlevel='Info' writeTo='debug1' />
                </rules>
            </nlog>");

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
                logger.Error(ex, "msg");
            }

            Assert.Contains(typeof(AggregateException).Name, aggregateExceptionMessage);
            Assert.Contains("Test exception 1", aggregateExceptionMessage);
            Assert.Contains("Test Inner 1", aggregateExceptionMessage);

            var lastMessage = GetDebugLastMessage("debug1");
            Assert.StartsWith("Test exception 1", lastMessage);
            Assert.Contains("Test Inner 1", lastMessage);
            Assert.Contains(string.Format(ExceptionDataFormat, exceptionDataKey, exceptionDataValue), lastMessage);
            Assert.Contains(string.Format(ExceptionDataFormat, aggregateExceptionDataKey, aggregateExceptionDataValue), lastMessage);
        }

        [Fact]
        public void CustomExceptionProperties_Layout_Test()
        {
            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
            <nlog>
                <targets>
                    <target name='debug1' type='Debug' layout='${exception:format=Properties}' />
                </targets>
                <rules>
                    <logger minlevel='Info' writeTo='debug1' />
                </rules>
            </nlog>");

            var ex = new CustomArgumentException("Goodbye World", "Nuke");
            logger.Fatal(ex, "msg");
            AssertDebugLastMessage("debug1", $"{nameof(CustomArgumentException.ParamName)}: Nuke");
        }

        [Fact]
        public void BaseExceptionTest()
        {
            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
            <nlog>
                <targets>
                    <target name='debug1' type='Debug' layout='${exception:format=Message:BaseException=true}' />
                </targets>
                <rules>
                    <logger minlevel='Info' writeTo='debug1' />
                </rules>
            </nlog>");

            var ex = GetNestedExceptionWithStackTrace("Goodbye World");
            logger.Fatal(ex, "msg");
            AssertDebugLastMessage("debug1", "Goodbye World");
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

        private void SetConfigurationForExceptionUsingRootMethodTests()
        {
            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
            <nlog>
                <targets>
                    <target name='debug1' type='Debug' layout='${level:uppercase=true}*${message}*${exception:format=message,shorttype:separator=*}' />
                </targets>
                <rules>
                    <logger minlevel='Trace' writeTo='debug1' />
                </rules>
            </nlog>");
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

        private Exception GetExceptionWithoutStackTrace(string exceptionMessage)
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
            var config = new LoggingConfiguration();

            var debugTarget = new DebugTarget();
            config.AddTarget("debug1", debugTarget);
            debugTarget.Layout = @"${exception:format=shorttype,message:maxInnerExceptionLevel=3}";

            var rule = new LoggingRule("*", LogLevel.Info, debugTarget);
            config.LoggingRules.Add(rule);

            LogManager.Configuration = config;

            string exceptionMessage = "Test exception";
            Exception ex = GetNestedExceptionWithStackTrace(exceptionMessage);

            logger.Error(ex, "msg");
            AssertDebugLastMessage("debug1", "ApplicationException Wrapper2" + EnvironmentHelper.NewLine +
                                             "ArgumentException Wrapper1" + EnvironmentHelper.NewLine +
                                             "CustomArgumentException Test exception");

            var t = (DebugTarget)LogManager.Configuration.AllTargets[0];
            var elr = ((SimpleLayout)t.Layout).Renderers[0] as ExceptionLayoutRenderer;


            Assert.Equal(ExceptionRenderingFormat.ShortType, elr.Formats[0]);
            Assert.Equal(ExceptionRenderingFormat.Message, elr.Formats[1]);
        }

        [Fact]
        public void InnerExceptionTestAPI()
        {
            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
            <nlog>
                <targets>
                    <target name='debug1' type='Debug' layout='${exception:format=shorttype,message:maxInnerExceptionLevel=3:innerFormat=message}' />
                </targets>
                <rules>
                    <logger minlevel='Info' writeTo='debug1' />
                </rules>
            </nlog>");

            string exceptionMessage = "Test exception";
            Exception ex = GetNestedExceptionWithStackTrace(exceptionMessage);

            logger.Error(ex, "msg");
            AssertDebugLastMessage("debug1", "ApplicationException Wrapper2" + EnvironmentHelper.NewLine +
                                             "Wrapper1" + EnvironmentHelper.NewLine +
                                             "Test exception");

            var t = (DebugTarget)LogManager.Configuration.AllTargets[0];
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
            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
            <nlog>
                <targets>                    
                    <target name='debug1' type='Debug' layout='${exception:format=data}' />
                    <target name='debug2' type='Debug' layout='${exception:format=data:ExceptionDataSeparator=*}' />
                    <target name='debug3' type='Debug' layout='${exception:format=data:ExceptionDataSeparator=## **}' />
                </targets>
                <rules>
                    <logger minlevel='Info' writeTo='debug1, debug2, debug3' />
                </rules>
            </nlog>");

            const string defaultExceptionDataSeparator = ";";
            const string exceptionMessage = "message for exception";
            const string exceptionDataKey1 = "testkey1";
            const string exceptionDataValue1 = "testvalue1";
            const string exceptionDataKey2 = "testkey2";
            const string exceptionDataValue2 = "testvalue2";

            var target = (DebugTarget)LogManager.Configuration.AllTargets[0];
            var exceptionLayoutRenderer = ((SimpleLayout)target.Layout).Renderers[0] as ExceptionLayoutRenderer;
            Assert.NotNull(exceptionLayoutRenderer);
            Assert.Equal(defaultExceptionDataSeparator, exceptionLayoutRenderer.ExceptionDataSeparator);

            Exception ex = GetExceptionWithStackTrace(exceptionMessage);
            ex.Data.Add(exceptionDataKey1, exceptionDataValue1);
            ex.Data.Add(exceptionDataKey2, exceptionDataValue2);

            logger.Error(ex);

            AssertDebugLastMessage("debug1", string.Format(ExceptionDataFormat, exceptionDataKey1, exceptionDataValue1) + defaultExceptionDataSeparator + string.Format(ExceptionDataFormat, exceptionDataKey2, exceptionDataValue2));
            AssertDebugLastMessage("debug2", string.Format(ExceptionDataFormat, exceptionDataKey1, exceptionDataValue1) + "*" + string.Format(ExceptionDataFormat, exceptionDataKey2, exceptionDataValue2));
            AssertDebugLastMessage("debug3", string.Format(ExceptionDataFormat, exceptionDataKey1, exceptionDataValue1) + "## **" + string.Format(ExceptionDataFormat, exceptionDataKey2, exceptionDataValue2));
        }

        [Fact]
        public void ExceptionDataWithNewLineSeparator()
        {
            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
            <nlog>
                <targets>                                        
                    <target name='debug1' type='Debug' layout='${exception:format=data:ExceptionDataSeparator=\r\n}' />
                    <target name='debug2' type='Debug' layout='${exception:format=data:ExceptionDataSeparator=\r\n----DATA----\r\n}' />
                    <target name='debug3' type='Debug' layout='${exception:format=data:ExceptionDataSeparator=&#13;&#10;----DATA----&#13;&#10;}' />
                </targets>
                <rules>
                    <logger minlevel='Info' writeTo='debug1, debug2, debug3' />
                </rules>
            </nlog>");

            const string exceptionMessage = "message for exception";
            const string exceptionDataKey1 = "testkey1";
            const string exceptionDataValue1 = "testvalue1";
            const string exceptionDataKey2 = "testkey2";
            const string exceptionDataValue2 = "testvalue2";

            Exception ex = GetExceptionWithStackTrace(exceptionMessage);
            ex.Data.Add(exceptionDataKey1, exceptionDataValue1);
            ex.Data.Add(exceptionDataKey2, exceptionDataValue2);

            logger.Error(ex);

            AssertDebugLastMessage("debug1", string.Format(ExceptionDataFormat, exceptionDataKey1, exceptionDataValue1) + "\r\n" + string.Format(ExceptionDataFormat, exceptionDataKey2, exceptionDataValue2));
            AssertDebugLastMessage("debug2", string.Format(ExceptionDataFormat, exceptionDataKey1, exceptionDataValue1) + "\r\n----DATA----\r\n" + string.Format(ExceptionDataFormat, exceptionDataKey2, exceptionDataValue2));
            AssertDebugLastMessage("debug3", string.Format(ExceptionDataFormat, exceptionDataKey1, exceptionDataValue1) + "\r\n----DATA----\r\n" + string.Format(ExceptionDataFormat, exceptionDataKey2, exceptionDataValue2));
        }


        [Fact]
        public void ExceptionWithSeparatorForExistingRender()
        {
            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
            <nlog>
                <targets>                                        
                    <target name='debug1' type='Debug' layout='${exception:format=tostring,data:separator=\r\nXXX}' />
                </targets>
                <rules>
                    <logger minlevel='Info' writeTo='debug1' />
                </rules>
            </nlog>");

            const string exceptionMessage = "message for exception";
            const string exceptionDataKey1 = "testkey1";
            const string exceptionDataValue1 = "testvalue1";

            Exception ex = GetExceptionWithoutStackTrace(exceptionMessage);
            ex.Data.Add(exceptionDataKey1, exceptionDataValue1);

            logger.Error(ex);

            AssertDebugLastMessage("debug1", string.Format(ExceptionDataFormat, ex.GetType().FullName, exceptionMessage) + "\r\nXXX" + string.Format(ExceptionDataFormat, exceptionDataKey1, exceptionDataValue1));
        }

        [Fact]
        public void ExceptionWithSeparatorForExistingBetweenRender()
        {
            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
            <nlog>
                <targets>                                        
                    <target name='debug1' type='Debug' layout='${exception:format=tostring,data,type:separator=\r\nXXX}' />
                </targets>
                <rules>
                    <logger minlevel='Info' writeTo='debug1' />
                </rules>
            </nlog>");

            const string exceptionMessage = "message for exception";
            const string exceptionDataKey1 = "testkey1";
            const string exceptionDataValue1 = "testvalue1";

            Exception ex = GetExceptionWithoutStackTrace(exceptionMessage);
            ex.Data.Add(exceptionDataKey1, exceptionDataValue1);

            logger.Error(ex);

            AssertDebugLastMessage(
                "debug1",
                string.Format(ExceptionDataFormat, ex.GetType().FullName, exceptionMessage) +
                "\r\nXXX" +
                string.Format(ExceptionDataFormat, exceptionDataKey1, exceptionDataValue1) +
                "\r\nXXX" +
                ex.GetType().FullName);
        }

        [Fact]
        public void ExceptionWithoutSeparatorForNoRender()
        {
            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
            <nlog>
                <targets>                                        
                    <target name='debug1' type='Debug' layout='${exception:format=tostring,data:separator=\r\nXXX}' />
                </targets>
                <rules>
                    <logger minlevel='Info' writeTo='debug1' />
                </rules>
            </nlog>");

            const string exceptionMessage = "message for exception";

            Exception ex = GetExceptionWithoutStackTrace(exceptionMessage);

            logger.Error(ex);

            AssertDebugLastMessage("debug1", string.Format(ExceptionDataFormat, ex.GetType().FullName, exceptionMessage));
        }


        [Fact]
        public void ExceptionWithoutSeparatorForNoBetweenRender()
        {
            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
            <nlog>
                <targets>
                    <target name='debug1' type='Debug' layout='${exception:format=tostring,data,type:separator=\r\nXXX}' />
                </targets>
                <rules>
                    <logger minlevel='Info' writeTo='debug1' />
                </rules>
            </nlog>");

            const string exceptionMessage = "message for exception";

            Exception ex = GetExceptionWithoutStackTrace(exceptionMessage);

            logger.Error(ex);

            AssertDebugLastMessage(
                "debug1",
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