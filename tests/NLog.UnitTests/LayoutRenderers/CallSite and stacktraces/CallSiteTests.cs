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
using NLog.Config;
using NLog.Internal;
using NLog.Layouts;
using NLog.Targets;
using System.Runtime.CompilerServices;

namespace NLog.UnitTests.LayoutRenderers
{
    using System;
    using System.IO;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Xunit;

    public class CallSiteTests : NLogTestBase
    {
#if !NETSTANDARD
        [Fact]
        public void HiddenAssemblyTest()
        {
            const string code = @"
                                namespace Foo
                                {
                                    public class HiddenAssemblyLogger
                                    {
                                        public void LogDebug(NLog.Logger logger)
                                        {
                                            logger.Debug(""msg"");
                                        }
                                    }
                                }
                              ";

            var provider = new Microsoft.CSharp.CSharpCodeProvider();
            var parameters = new System.CodeDom.Compiler.CompilerParameters();

            // reference the NLog dll
            parameters.ReferencedAssemblies.Add("NLog.dll");

            // the assembly should be generated in memory
            parameters.GenerateInMemory = true;

            // generate a dll instead of an executable
            parameters.GenerateExecutable = false;

            // compile code and generate assembly
            System.CodeDom.Compiler.CompilerResults results = provider.CompileAssemblyFromSource(parameters, code);

            Assert.False(results.Errors.HasErrors, "Compiler errors: " + string.Join(";", results.Errors));

            // create nlog configuration
            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${callsite} ${message}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>");

            // create logger
            Logger logger = LogManager.GetLogger("A");

            // load HiddenAssemblyLogger type
            Assembly compiledAssembly = results.CompiledAssembly;
            Type hiddenAssemblyLoggerType = compiledAssembly.GetType("Foo.HiddenAssemblyLogger");
            Assert.NotNull(hiddenAssemblyLoggerType);

            // load methodinfo
            MethodInfo logDebugMethod = hiddenAssemblyLoggerType.GetMethod("LogDebug");
            Assert.NotNull(logDebugMethod);

            // instantiate the HiddenAssemblyLogger from previously generated assembly
            object instance = Activator.CreateInstance(hiddenAssemblyLoggerType);

            // Add the previously generated assembly to the "blacklist"
            LogManager.AddHiddenAssembly(compiledAssembly);

            // call the log method
            logDebugMethod.Invoke(instance, new object[] { logger });

            MethodBase currentMethod = MethodBase.GetCurrentMethod();
            AssertDebugLastMessage("debug", currentMethod.DeclaringType.FullName + "." + currentMethod.Name + " msg");
        }
#endif

#if !DEBUG
        [Fact(Skip = "RELEASE not working, only DEBUG")]
#else
        [Fact]
#endif
        public void LineNumberTest()
        {
            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${callsite:filename=true} ${message}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>");

            ILogger logger = LogManager.GetLogger("A");
#if DEBUG
#line 100000
#endif
            logger.Debug("msg");
            var linenumber = GetPrevLineNumber();
            string lastMessage = GetDebugLastMessage("debug");
            // There's a difference in handling line numbers between .NET and Mono
            // We're just interested in checking if it's above 100000
            Assert.True(lastMessage.IndexOf("callsitetests.cs:" + linenumber, StringComparison.OrdinalIgnoreCase) >= 0, "Invalid line number. Expected prefix of 10000, got: " + lastMessage);
#if DEBUG
#line default
#endif
        }

        [Fact]
        public void MethodNameTest()
        {
            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${callsite} ${message}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>");

            ILogger logger = LogManager.GetLogger("A");
            logger.Debug("msg");
            MethodBase currentMethod = MethodBase.GetCurrentMethod();
            AssertDebugLastMessage("debug", currentMethod.DeclaringType.FullName + "." + currentMethod.Name + " msg");
        }

        [Fact]
        public void MethodNameInChainTest()
        {
            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
            <nlog>
                <targets>
                    <target name='debug' type='Debug' layout='${message}' />
                    <target name='debug2' type='Debug' layout='${callsite} ${message}' />
                </targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug,debug2' />
                </rules>
            </nlog>");

            ILogger logger = LogManager.GetLogger("A");
            logger.Debug("msg2");
            MethodBase currentMethod = MethodBase.GetCurrentMethod();
            AssertDebugLastMessage("debug2", currentMethod.DeclaringType.FullName + "." + currentMethod.Name + " msg2");
        }

        [Fact]
        public void MethodNameNoCaptureStackTraceTest()
        {
            // Arrange
            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${callsite:captureStackTrace=false} ${message}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>");

            // Act
            LogManager.GetLogger("A").Debug("msg");

            // Assert
            AssertDebugLastMessage("debug", " msg");
        }

        [Fact]
        public void MethodNameNoCaptureStackTraceWithStackTraceTest()
        {
            // Arrange
            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${callsite:captureStackTrace=false} ${message}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>");
            MethodBase currentMethod = MethodBase.GetCurrentMethod();

            // Act
            var logEvent = new LogEventInfo(LogLevel.Info, null, "msg");
            logEvent.SetStackTrace(new System.Diagnostics.StackTrace(true), 0);
            LogManager.GetLogger("A").Log(logEvent);

            // Assert
            AssertDebugLastMessage("debug", currentMethod.DeclaringType.FullName + "." + currentMethod.Name + " msg");
        }

        [Fact]
        public void ClassNameTest()
        {
            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${callsite:classname=true:methodname=false} ${message}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>");

            ILogger logger = LogManager.GetLogger("A");
            logger.Debug("msg");
            AssertDebugLastMessage("debug", "NLog.UnitTests.LayoutRenderers.CallSiteTests msg");
        }

        [Fact]
        public void ClassNameTestWithoutNamespace()
        {
            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${callsite:classname=true:methodname=false:includeNamespace=false} ${message}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>");

            ILogger logger = LogManager.GetLogger("A");
            logger.Debug("msg");
            AssertDebugLastMessage("debug", "CallSiteTests msg");
        }

        [Fact]
        public void ClassNameTestWithOverride()
        {
            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${callsite:classname=true:methodname=false:includeNamespace=false} ${message}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>");

            ILogger logger = LogManager.GetLogger("A");
            var logEvent = LogEventInfo.Create(LogLevel.Debug, logger.Name, "msg");
            logEvent.SetCallerInfo("NLog.UnitTests.LayoutRenderers.OverrideClassName", nameof(ClassNameTestWithOverride), null, 0);
            logger.Log(logEvent);
            AssertDebugLastMessage("debug", "OverrideClassName msg");
        }

        [Fact]
        public void ClassNameWithPaddingTestPadLeftAlignLeftTest()
        {
            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${callsite:classname=true:methodname=false:padding=3:fixedlength=true} ${message}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>");

            ILogger logger = LogManager.GetLogger("A");
            logger.Debug("msg");
            MethodBase currentMethod = MethodBase.GetCurrentMethod();
            AssertDebugLastMessage("debug", currentMethod.DeclaringType.FullName.Substring(0, 3) + " msg");
        }

        [Fact]
        public void ClassNameWithPaddingTestPadLeftAlignRightTest()
        {
            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${callsite:classname=true:methodname=false:padding=3:fixedlength=true:alignmentOnTruncation=right} ${message}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>");

            ILogger logger = LogManager.GetLogger("A");
            logger.Debug("msg");
            MethodBase currentMethod = MethodBase.GetCurrentMethod();
            var typeName = currentMethod.DeclaringType.FullName;
            AssertDebugLastMessage("debug", typeName.Substring(typeName.Length - 3) + " msg");
        }

        [Fact]
        public void ClassNameWithPaddingTestPadRightAlignLeftTest()
        {
            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${callsite:classname=true:methodname=false:padding=-3:fixedlength=true:alignmentOnTruncation=left} ${message}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>");

            ILogger logger = LogManager.GetLogger("A");
            logger.Debug("msg");
            MethodBase currentMethod = MethodBase.GetCurrentMethod();
            AssertDebugLastMessage("debug", currentMethod.DeclaringType.FullName.Substring(0, 3) + " msg");
        }

        [Fact]
        public void ClassNameWithPaddingTestPadRightAlignRightTest()
        {
            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${callsite:classname=true:methodname=false:padding=-3:fixedlength=true:alignmentOnTruncation=right} ${message}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>");

            ILogger logger = LogManager.GetLogger("A");
            logger.Debug("msg");
            MethodBase currentMethod = MethodBase.GetCurrentMethod();
            var typeName = currentMethod.DeclaringType.FullName;
            AssertDebugLastMessage("debug", typeName.Substring(typeName.Length - 3) + " msg");
        }

        [Fact]
        public void MethodNameWithPaddingTestPadLeftAlignLeftTest()
        {
            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${callsite:classname=false:methodname=true:padding=16:fixedlength=true} ${message}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>");

            ILogger logger = LogManager.GetLogger("A");
            logger.Debug("msg");
            AssertDebugLastMessage("debug", "MethodNameWithPa msg");
        }

        [Fact]
        public void MethodNameWithPaddingTestPadLeftAlignRightTest()
        {
            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${callsite:classname=false:methodname=true:padding=16:fixedlength=true:alignmentOnTruncation=right} ${message}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>");

            ILogger logger = LogManager.GetLogger("A");
            logger.Debug("msg");
            AssertDebugLastMessage("debug", "ftAlignRightTest msg");
        }

        [Fact]
        public void MethodNameWithPaddingTestPadRightAlignLeftTest()
        {
            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${callsite:classname=false:methodname=true:padding=-16:fixedlength=true:alignmentOnTruncation=left} ${message}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>");

            ILogger logger = LogManager.GetLogger("A");
            logger.Debug("msg");
            AssertDebugLastMessage("debug", "MethodNameWithPa msg");
        }

        [Fact]
        public void MethodNameWithPaddingTestPadRightAlignRightTest()
        {
            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${callsite:classname=false:methodname=true:padding=-16:fixedlength=true:alignmentOnTruncation=right} ${message}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>");

            ILogger logger = LogManager.GetLogger("A");
            logger.Debug("msg");
            AssertDebugLastMessage("debug", "htAlignRightTest msg");
        }

        [Fact]
        public void GivenSkipFrameNotDefined_WhenLogging_ThenLogFirstUserStackFrame()
        {
            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${callsite} ${message}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>");

            ILogger logger = LogManager.GetLogger("A");
            logger.Debug("msg");
            AssertDebugLastMessage("debug", "NLog.UnitTests.LayoutRenderers.CallSiteTests.GivenSkipFrameNotDefined_WhenLogging_ThenLogFirstUserStackFrame msg");
        }

#if !DEBUG
        [Fact(Skip = "RELEASE not working, only DEBUG")]
#else
        [Fact]
#endif
        public void GivenOneSkipFrameDefined_WhenLogging_ShouldSkipOneUserStackFrame()
        {
            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${callsite:skipframes=1} ${message}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>");

            ILogger logger = LogManager.GetLogger("A");
            Action action = () => logger.Debug("msg");
            action.Invoke();
            AssertDebugLastMessage("debug", "NLog.UnitTests.LayoutRenderers.CallSiteTests.GivenOneSkipFrameDefined_WhenLogging_ShouldSkipOneUserStackFrame msg");
        }

        [Fact]
        public void CleanMethodNamesOfAnonymousDelegatesTest()
        {
            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
                <nlog>
                    <targets><target name='debug' type='Debug' layout='${callsite:ClassName=false:CleanNamesOfAnonymousDelegates=true}' /></targets>
                    <rules>
                        <logger name='*' levels='Fatal' writeTo='debug' />
                    </rules>
                </nlog>");

            ILogger logger = LogManager.GetLogger("A");

            bool done = false;
            ThreadPool.QueueUserWorkItem(
                state =>
                {
                    logger.Fatal("message");
                    done = true;
                },
                null);

            while (done == false)
            {
                Thread.Sleep(10);
            }

            if (done == true)
            {
                AssertDebugLastMessage("debug", "CleanMethodNamesOfAnonymousDelegatesTest");
            }
        }

        [Fact]
        public void DontCleanMethodNamesOfAnonymousDelegatesTest()
        {
            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
                <nlog>
                    <targets><target name='debug' type='Debug' layout='${callsite:ClassName=false:CleanNamesOfAnonymousDelegates=false}' /></targets>
                    <rules>
                        <logger name='*' levels='Fatal' writeTo='debug' />
                    </rules>
                </nlog>");

            ILogger logger = LogManager.GetLogger("A");

            bool done = false;
            ThreadPool.QueueUserWorkItem(
                state =>
                {
                    logger.Fatal("message");
                    done = true;
                },
                null);

            while (done == false)
            {
                Thread.Sleep(10);
            }

            if (done == true)
            {
                string lastMessage = GetDebugLastMessage("debug");
                Assert.StartsWith("<DontCleanMethodNamesOfAnonymousDelegatesTest>", lastMessage);
            }
        }

        [Fact]
        public void CleanClassNamesOfAnonymousDelegatesTest()
        {
            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
                <nlog>
                    <targets><target name='debug' type='Debug' layout='${callsite:ClassName=true:MethodName=false:CleanNamesOfAnonymousDelegates=true}' /></targets>
                    <rules>
                        <logger name='*' levels='Fatal' writeTo='debug' />
                    </rules>
                </nlog>");

            ILogger logger = LogManager.GetLogger("A");

            bool done = false;
            ThreadPool.QueueUserWorkItem(
                state =>
                {
                    logger.Fatal("message");
                    done = true;
                },
                null);

            while (done == false)
            {
                Thread.Sleep(10);
            }

            if (done == true)
            {
                AssertDebugLastMessage("debug", "NLog.UnitTests.LayoutRenderers.CallSiteTests");
            }
        }

        [Fact]
        public void DontCleanClassNamesOfAnonymousDelegatesTest()
        {
            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
                <nlog>
                    <targets><target name='debug' type='Debug' layout='${callsite:ClassName=true:MethodName=false:CleanNamesOfAnonymousDelegates=false}' /></targets>
                    <rules>
                        <logger name='*' levels='Fatal' writeTo='debug' />
                    </rules>
                </nlog>");

            ILogger logger = LogManager.GetLogger("A");

            bool done = false;
            ThreadPool.QueueUserWorkItem(
                state =>
                {
                    logger.Fatal("message");
                    done = true;
                },
                null);

            while (done == false)
            {
                Thread.Sleep(10);
            }

            if (done == true)
            {
                string lastMessage = GetDebugLastMessage("debug");
                Assert.Contains("+<>", lastMessage);
            }
        }

        [Fact]
        public void When_NotIncludeNameSpace_Then_CleanAnonymousDelegateClassNameShouldReturnParentClassName()
        {
            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
                <nlog>
                    <targets><target name='debug' type='Debug' layout='${callsite:ClassName=true:MethodName=false:IncludeNamespace=false:CleanNamesOfAnonymousDelegates=true}' /></targets>
                    <rules>
                        <logger name='*' levels='Fatal' writeTo='debug' />
                    </rules>
                </nlog>");

            ILogger logger = LogManager.GetLogger("A");

            bool done = false;
            ThreadPool.QueueUserWorkItem(
                state =>
                {
                    logger.Fatal("message");
                    done = true;
                },
                null);

            while (done == false)
            {
                Thread.Sleep(10);
            }

            if (done == true)
            {
                AssertDebugLastMessage("debug", "CallSiteTests");
            }
        }


        [Fact]
        public void When_Wrapped_Ignore_Wrapper_Methods_In_Callstack()
        {
            //namespace en name of current method
            const string currentMethodFullName = "NLog.UnitTests.LayoutRenderers.CallSiteTests.When_Wrapped_Ignore_Wrapper_Methods_In_Callstack";

            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
               <nlog>
                   <targets><target name='debug' type='Debug' layout='${callsite}|${message}' /></targets>
                   <rules>
                       <logger name='*' levels='Warn' writeTo='debug' />
                   </rules>
               </nlog>");

            var logger = LogManager.GetLogger("A");
            logger.Warn("direct");
            AssertDebugLastMessage("debug", $"{currentMethodFullName}|direct");

            LoggerTests.BaseWrapper wrappedLogger = new LoggerTests.MyWrapper();
            wrappedLogger.Log("wrapped");
            AssertDebugLastMessage("debug", $"{currentMethodFullName}|wrapped");
        }

        [Fact]
        public void CheckStackTraceUsageForTwoRules()
        {
            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
            <nlog>
                <targets>
                    <target name='debug' type='Debug' layout='${message}' />
                    <target name='debug2' type='Debug' layout='${callsite} ${message}' />
                </targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                    <logger name='*' minlevel='Debug' writeTo='debug2' />
                </rules>
            </nlog>");

            ILogger logger = LogManager.GetLogger("A");
            logger.Debug("msg");
            AssertDebugLastMessage("debug2", "NLog.UnitTests.LayoutRenderers.CallSiteTests.CheckStackTraceUsageForTwoRules msg");
        }


        [Fact]
        public void CheckStackTraceUsageForTwoRules_chained()
        {
            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
            <nlog>
                <targets>
                    <target name='debug' type='Debug' layout='${message}' />
                    <target name='debug2' type='Debug' layout='${callsite} ${message}' />
                </targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                    <logger name='*' minlevel='Debug' writeTo='debug,debug2' />
                </rules>
            </nlog>");

            ILogger logger = LogManager.GetLogger("A");
            logger.Debug("msg");
            AssertDebugLastMessage("debug2", "NLog.UnitTests.LayoutRenderers.CallSiteTests.CheckStackTraceUsageForTwoRules_chained msg");
        }


        [Fact]
        public void CheckStackTraceUsageForMultipleRules()
        {
            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
            <nlog>
                <targets>
                    <target name='debug' type='Debug' layout='${message}' />
                    <target name='debug2' type='Debug' layout='${callsite} ${message}' />
                </targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                    <logger name='*' minlevel='Debug' writeTo='debug,debug2' />
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>");

            ILogger logger = LogManager.GetLogger("A");
            logger.Debug("msg");
            AssertDebugLastMessage("debug2", "NLog.UnitTests.LayoutRenderers.CallSiteTests.CheckStackTraceUsageForMultipleRules msg");
        }


        #region Compositio unit test

        [Fact]
        public void When_WrappedInCompsition_Ignore_Wrapper_Methods_In_Callstack()
        {
            //namespace en name of current method
            const string currentMethodFullName = "NLog.UnitTests.LayoutRenderers.CallSiteTests.When_WrappedInCompsition_Ignore_Wrapper_Methods_In_Callstack";

            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
           <nlog>
               <targets><target name='debug' type='Debug' layout='${callsite}|${message}' /></targets>
               <rules>
                   <logger name='*' levels='Warn' writeTo='debug' />
               </rules>
           </nlog>");

            var logger = LogManager.GetLogger("A");
            logger.Warn("direct");
            AssertDebugLastMessage("debug", $"{currentMethodFullName}|direct");

            CompositeWrapper wrappedLogger = new CompositeWrapper();
            wrappedLogger.Log("wrapped");
            AssertDebugLastMessage("debug", $"{currentMethodFullName}|wrapped");
        }

#if NET35
        [Fact(Skip = "NET35 not supporting async callstack")]
#else
        [Fact]
#endif
        public void Show_correct_method_with_async()
        {
            //namespace en name of current method
            const string currentMethodFullName = "NLog.UnitTests.LayoutRenderers.CallSiteTests.AsyncMethod";

            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
           <nlog>
               <targets><target name='debug' type='Debug' layout='${callsite}|${message}' /></targets>
               <rules>
                   <logger name='*' levels='Warn' writeTo='debug' />
               </rules>
           </nlog>");

            AsyncMethod().Wait();
            AssertDebugLastMessage("debug", $"{currentMethodFullName}|direct");

            new InnerClassAsyncMethod().AsyncMethod().Wait();
            AssertDebugLastMessage("debug", $"{typeof(InnerClassAsyncMethod).ToString()}.AsyncMethod|direct");
        }

        private async Task AsyncMethod()
        {
            var logger = LogManager.GetCurrentClassLogger();
            logger.Warn("direct");
            var reader = new StreamReader(new MemoryStream(new byte[0]));
            await reader.ReadLineAsync();
        }

        private class InnerClassAsyncMethod
        {
            public async Task AsyncMethod()
            {
                var logger = LogManager.GetCurrentClassLogger();
                logger.Warn("direct");
                var reader = new StreamReader(new MemoryStream(new byte[0]));
                await reader.ReadLineAsync();
            }
        }

#if NET35
        [Fact(Skip = "NET35 not supporting async callstack")]
#elif MONO
        [Fact(Skip = "Not working under MONO - not sure if unit test is wrong, or the code")]
#else
        [Fact]
#endif
        public void Show_correct_filename_with_async()
        {
            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
           <nlog>
               <targets><target name='debug' type='Debug' layout='${callsite:className=False:fileName=True:includeSourcePath=False:methodName=False}|${message}' /></targets>
               <rules>
                   <logger name='*' levels='Warn' writeTo='debug' />
               </rules>
           </nlog>");

            AsyncMethod().Wait();
            Assert.Contains("CallSiteTests.cs", GetDebugLastMessage("debug"));
            Assert.Contains("|direct", GetDebugLastMessage("debug"));
        }

#if NET35
        [Fact(Skip = "NET35 not supporting async callstack")]
#else
        [Fact]
#endif
        public void Show_correct_method_with_async2()
        {
            //namespace en name of current method
            const string currentMethodFullName = "NLog.UnitTests.LayoutRenderers.CallSiteTests.AsyncMethod2b";

            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
           <nlog>
               <targets><target name='debug' type='Debug' layout='${callsite}|${message}' /></targets>
               <rules>
                   <logger name='*' levels='Warn' writeTo='debug' />
               </rules>
           </nlog>");

            AsyncMethod2a().Wait();
            AssertDebugLastMessage("debug", $"{currentMethodFullName}|direct");

            new InnerClassAsyncMethod2().AsyncMethod2a().Wait();
            AssertDebugLastMessage("debug", $"{typeof(InnerClassAsyncMethod2).ToString()}.AsyncMethod2b|direct");
        }

        private async Task AsyncMethod2a()
        {
            await AsyncMethod2b();
        }

        private async Task AsyncMethod2b()
        {
            var logger = LogManager.GetCurrentClassLogger();
            logger.Warn("direct");
            var reader = new StreamReader(new MemoryStream(new byte[0]));
            await reader.ReadLineAsync();
        }

        private class InnerClassAsyncMethod2
        {
            public async Task AsyncMethod2a()
            {
                await AsyncMethod2b();
            }

            public async Task AsyncMethod2b()
            {
                var logger = LogManager.GetCurrentClassLogger();
                logger.Warn("direct");
                var reader = new StreamReader(new MemoryStream(new byte[0]));
                await reader.ReadLineAsync();
            }
        }

#if NET35 
        [Fact(Skip = "NET35 not supporting async callstack")]
#elif !DEBUG
        [Fact(Skip = "RELEASE not working, only DEBUG")]
#else
        [Fact]
#endif
        public void Show_correct_method_with_async3()
        {
            //namespace en name of current method
            const string currentMethodFullName = "NLog.UnitTests.LayoutRenderers.CallSiteTests.AsyncMethod3b";

            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
           <nlog>
               <targets><target name='debug' type='Debug' layout='${callsite}|${message}' /></targets>
               <rules>
                   <logger name='*' levels='Warn' writeTo='debug' />
               </rules>
           </nlog>");

            AsyncMethod3a().Wait();
            AssertDebugLastMessage("debug", $"{currentMethodFullName}|direct");

            new InnerClassAsyncMethod3().AsyncMethod3a().Wait();
            AssertDebugLastMessage("debug", $"{typeof(InnerClassAsyncMethod3).ToString()}.AsyncMethod3b|direct");
        }

        private async Task AsyncMethod3a()
        {
            var reader = new StreamReader(new MemoryStream(new byte[0]));
            await reader.ReadLineAsync();
            AsyncMethod3b();
        }

        private void AsyncMethod3b()
        {
            var logger = LogManager.GetCurrentClassLogger();
            logger.Warn("direct");
        }

        private class InnerClassAsyncMethod3
        {
            public async Task AsyncMethod3a()
            {
                var reader = new StreamReader(new MemoryStream(new byte[0]));
                await reader.ReadLineAsync();
                AsyncMethod3b();
            }

            public void AsyncMethod3b()
            {
                var logger = LogManager.GetCurrentClassLogger();
                logger.Warn("direct");
            }
        }

#if NET35
        [Fact(Skip = "NET35 not supporting async callstack")]
#elif MONO
        [Fact(Skip = "Not working under MONO - not sure if unit test is wrong, or the code")]
#else
        [Fact]
#endif
        public void Show_correct_method_with_async4()
        {
            //namespace en name of current method
            const string currentMethodFullName = "NLog.UnitTests.LayoutRenderers.CallSiteTests.AsyncMethod4";

            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
           <nlog>
               <targets><target name='debug' type='Debug' layout='${callsite}|${message}' /></targets>
               <rules>
                   <logger name='*' levels='Info' writeTo='debug' />
               </rules>
           </nlog>");

            AsyncMethod4().Wait();
            AssertDebugLastMessage("debug", $"{currentMethodFullName}|Direct, async method");

            new InnerClassAsyncMethod4().AsyncMethod4().Wait();
            AssertDebugLastMessage("debug", $"{typeof(InnerClassAsyncMethod4).ToString()}.AsyncMethod4|Direct, async method");
       }

        private async Task<IEnumerable<string>> AsyncMethod4()
        {
            Logger logger = LogManager.GetLogger("AnnonTest");
            logger.Info("Direct, async method");

            return await Task.FromResult(new string[] { "value1", "value2" });
        }

        private class InnerClassAsyncMethod4
        {
            public async Task<IEnumerable<string>> AsyncMethod4()
            {
                Logger logger = LogManager.GetLogger("AnnonTest");
                logger.Info("Direct, async method");

                return await Task.FromResult(new string[] { "value1", "value2" });
            }
        }

#if NET35
        [Fact(Skip = "NET35 not supporting async callstack")]
#elif MONO
        [Fact(Skip = "Not working under MONO - not sure if unit test is wrong, or the code")]
#else
        [Fact]
#endif
        public void CallSiteShouldWorkForAsyncMethodsWithReturnValue()
        {
            var callSite = GetAsyncCallSite().GetAwaiter().GetResult();
            Assert.Equal("NLog.UnitTests.LayoutRenderers.CallSiteTests.GetAsyncCallSite", callSite);
        }

        public async Task<string> GetAsyncCallSite()
        {
            var logEvent = new LogEventInfo(LogLevel.Error, "logger1", "message1");
#if !NETSTANDARD1_5
            Type loggerType = typeof(Logger);
            var stacktrace = new System.Diagnostics.StackTrace();
            logEvent.GetCallSiteInformationInternal().SetStackTrace(stacktrace, null, loggerType);
#endif
            await Task.Delay(0);
            Layout l = "${callsite}";
            var callSite = l.Render(logEvent);
            return callSite;
        }

#if !DEBUG
        [Fact(Skip = "RELEASE not working, only DEBUG")]
#else
        [Fact]
#endif
        public void Show_correct_method_for_moveNext()
        {
            //namespace en name of current method
            const string currentMethodFullName = "NLog.UnitTests.LayoutRenderers.CallSiteTests.MoveNext";

            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
           <nlog>
               <targets><target name='debug' type='Debug' layout='${callsite}|${message}' /></targets>
               <rules>
                   <logger name='*' levels='Warn' writeTo='debug' />
               </rules>
           </nlog>");

            MoveNext();
            AssertDebugLastMessage("debug", $"{currentMethodFullName}|direct");
        }

        private void MoveNext()
        {
            var logger = LogManager.GetCurrentClassLogger();
            logger.Warn("direct");
        }

        public class CompositeWrapper
        {
            private readonly MyWrapper wrappedLogger;

            public CompositeWrapper()
            {
                wrappedLogger = new MyWrapper();
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            public void Log(string what)
            {
                wrappedLogger.Log(typeof(CompositeWrapper), what);
            }
        }

        public abstract class BaseWrapper
        {
            public void Log(string what)
            {
                InternalLog(typeof(BaseWrapper), what);
            }

            public void Log(Type type, string what) //overloaded with type for composition
            {
                InternalLog(type, what);
            }

            protected abstract void InternalLog(Type type, string what);
        }

        public class MyWrapper : BaseWrapper
        {
            private readonly ILogger wrapperLogger;

            public MyWrapper()
            {
                wrapperLogger = LogManager.GetLogger("WrappedLogger");
            }

            protected override void InternalLog(Type type, string what) //added type for composition
            {
                LogEventInfo info = new LogEventInfo(LogLevel.Warn, wrapperLogger.Name, what);

                // Provide BaseWrapper as wrapper type.
                // Expected: UserStackFrame should point to the method that calls a 
                // method of BaseWrapper.
                wrapperLogger.Log(type, info);
            }
        }

        #endregion

        private class MyLogger : Logger
        {
        }

        [Fact]
        public void CallsiteBySubclass_interface()
        {
            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${callsite:classname=true:methodname=true} ${message}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>");

            ILogger logger = LogManager.GetLogger("mylogger", typeof(MyLogger));

            Assert.True(logger is MyLogger, "logger isn't MyLogger");
            logger.Debug("msg");
            AssertDebugLastMessage("debug", "NLog.UnitTests.LayoutRenderers.CallSiteTests.CallsiteBySubclass_interface msg");
        }

        [Fact]
        public void CallsiteBySubclass_mylogger()
        {
            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${callsite:classname=true:methodname=true} ${message}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>");

            MyLogger logger = LogManager.GetLogger("mylogger", typeof(MyLogger)) as MyLogger;

            Assert.NotNull(logger);
            logger.Debug("msg");
            AssertDebugLastMessage("debug", "NLog.UnitTests.LayoutRenderers.CallSiteTests.CallsiteBySubclass_mylogger msg");
        }

        [Fact]
        public void CallsiteBySubclass_logger()
        {
            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${callsite:classname=true:methodname=true} ${message}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>");

            Logger logger = LogManager.GetLogger("mylogger", typeof(MyLogger)) as Logger;

            Assert.NotNull(logger);
            logger.Debug("msg");
            AssertDebugLastMessage("debug", "NLog.UnitTests.LayoutRenderers.CallSiteTests.CallsiteBySubclass_logger msg");
        }

        [Fact]
        public void Should_preserve_correct_callsite_information()
        {
            // Step 1. Create configuration object 
            var config = new LoggingConfiguration();

            // Step 2. Create targets and add them to the configuration 
            var target = new MemoryTarget();
            var wrapper = new NLog.Targets.Wrappers.AsyncTargetWrapper(target) { TimeToSleepBetweenBatches = 0 };
            config.AddTarget("target", wrapper);

            // Step 3. Set target properties 
            target.Layout = "${date:format=HH\\:MM\\:ss} ${logger} ${message}";

            // Step 4. Define rules
            var rule = new LoggingRule("*", LogLevel.Debug, wrapper);
            config.LoggingRules.Add(rule);

            LogManager.Configuration = config;
            var factory = new NLogFactory(config);
            var logger = factory.Create("MyLoggerName");

            WriteLogMessage(logger);
            LogManager.Flush();
            var logMessage = target.Logs[0];
            Assert.Contains("MyLoggerName", logMessage);

            // See that LogManager.ReconfigExistingLoggers() is able to upgrade the Logger
            target.Layout = "${date:format=HH\\:MM\\:ss} ${logger} ${callsite} ${message}";
            LogManager.ReconfigExistingLoggers();
            WriteLogMessage(logger);
            LogManager.Flush();
            logMessage = target.Logs[1];
            Assert.Contains("CallSiteTests.WriteLogMessage", logMessage);

            // See that LogManager.ReconfigExistingLoggers() is able to upgrade the Logger
            target.Layout = "${date:format=HH\\:MM\\:ss} ${logger} ${callsite} ${message} ThreadId=${threadid}";
            LogManager.ReconfigExistingLoggers();
            WriteLogMessage(logger);
            LogManager.Flush();
            logMessage = target.Logs[2];
            Assert.Contains("ThreadId=" + Thread.CurrentThread.ManagedThreadId.ToString(), logMessage);

            // See that interface logging also works (Improve support for Microsoft.Extension.Logging.ILogger replacement)
            INLogLogger ilogger = logger;
            WriteLogMessage(ilogger);
            LogManager.Flush();
            logMessage = target.Logs[3];
            Assert.Contains("CallSiteTests.WriteLogMessage", logMessage);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void WriteLogMessage(NLogLogger logger)
        {
            logger.Debug("something");
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void WriteLogMessage(INLogLogger logger)
        {
            logger.Log(LogLevel.Debug, "something");
        }

        /// <summary>
        ///   
        /// </summary>
        public class NLogFactory
        {
            internal const string defaultConfigFileName = "nlog.config";

            /// <summary>
            ///   Initializes a new instance of the <see cref="NLogFactory" /> class.
            /// </summary>
            /// <param name="loggingConfiguration"> The NLog Configuration </param>
            public NLogFactory(LoggingConfiguration loggingConfiguration)
            {
                LogManager.Configuration = loggingConfiguration;
            }

            /// <summary>
            ///   Creates a logger with specified <paramref name="name" />.
            /// </summary>
            /// <param name="name"> The name. </param>
            /// <returns> </returns>
            public NLogLogger Create(string name)
            {
                var log = LogManager.GetLogger(name);
                return new NLogLogger(log);
            }
        }

#if !NETSTANDARD1_5
        /// <summary>
        /// If some calls got inlined, we can't find LoggerType anymore. We should fallback if loggerType can be found
        /// 
        /// Example of those stacktraces:
        ///    at NLog.LoggerImpl.Write(Type loggerType, TargetWithFilterChain targets, LogEventInfo logEvent, LogFactory factory) in c:\temp\NLog\src\NLog\LoggerImpl.cs:line 68
        ///    at NLog.UnitTests.LayoutRenderers.NLogLogger.ErrorWithoutLoggerTypeArg(String message) in c:\temp\NLog\tests\NLog.UnitTests\LayoutRenderers\CallSiteTests.cs:line 989
        ///    at NLog.UnitTests.LayoutRenderers.CallSiteTests.TestCallsiteWhileCallsGotInlined() in c:\temp\NLog\tests\NLog.UnitTests\LayoutRenderers\CallSiteTests.cs:line 893
        /// 
        /// </summary>
        [Fact]
        public void CallSiteShouldWorkEvenInlined()
        {
            var logEvent = new LogEventInfo(LogLevel.Error, "logger1", "message1");
            Type loggerType = typeof(Logger);
            var stacktrace = new System.Diagnostics.StackTrace();
            logEvent.GetCallSiteInformationInternal().SetStackTrace(stacktrace, null, loggerType);
            Layout l = "${callsite}";
            var callSite = l.Render(logEvent);
            Assert.Equal("NLog.UnitTests.LayoutRenderers.CallSiteTests.CallSiteShouldWorkEvenInlined", callSite);
        }
#endif

#if NET35
        [Fact(Skip = "NET35 not supporting async callstack")]
#elif MONO
        [Fact(Skip = "Not working under MONO - not sure if unit test is wrong, or the code")]
#else
        [Fact]
#endif
        public void LogAfterAwait_ShouldCleanMethodNameAsync5()
        {
            //namespace en name of current method
            const string currentMethodFullName = "NLog.UnitTests.LayoutRenderers.CallSiteTests.AsyncMethod5";

            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
           <nlog>
               <targets><target name='debug' type='Debug' layout='${callsite}|${message}' /></targets>
               <rules>
                   <logger name='*' levels='Debug' writeTo='debug' />
               </rules>
           </nlog>");

            AsyncMethod5().Wait();
            AssertDebugLastMessage("debug", $"{currentMethodFullName}|dude");

            new InnerClassAsyncMethod5().AsyncMethod5().Wait();
            AssertDebugLastMessage("debug", $"{typeof(InnerClassAsyncMethod5).ToString()}.AsyncMethod5|dude");
        }

        private async Task AsyncMethod5()
        {
            await AMinimalAsyncMethod();

            var logger = LogManager.GetCurrentClassLogger();
            logger.Debug("dude");
        }

        private class InnerClassAsyncMethod5
        {
            public async Task AsyncMethod5()
            {
                await AMinimalAsyncMethod();

                var logger = LogManager.GetCurrentClassLogger();
                logger.Debug("dude");
            }

            private async Task AMinimalAsyncMethod()
            {
                await Task.Delay(1);    // Ensure it always becomes async, and it is not inlined
            }
        }

        private async Task AMinimalAsyncMethod()
        {
            await Task.Delay(1);    // Ensure it always becomes async, and it is not inlined
        }

#if NET35
        [Fact(Skip = "NET35 not supporting async callstack")]
#elif MONO
        [Fact(Skip = "Not working under MONO - not sure if unit test is wrong, or the code")]
#else
        [Fact]
#endif
        public void LogAfterTaskRunAwait_CleanNamesOfAsyncContinuationsIsTrue_ShouldCleanMethodName()
        {
            // name of the logging method
            const string callsiteMethodName = nameof(LogAfterTaskRunAwait_CleanNamesOfAsyncContinuationsIsTrue_ShouldCleanMethodName);

            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
                <nlog>
                    <targets><target name='debug' type='Debug' layout='${callsite:classname=false:cleannamesofasynccontinuations=true}' /></targets>
                    <rules>
                        <logger name='*' levels='Debug' writeTo='debug' />
                    </rules>
                </nlog>");

            Task.Run(async () => {
                await AMinimalAsyncMethod();
                var logger = LogManager.GetCurrentClassLogger();
                logger.Debug("dude");
            }).Wait();

            AssertDebugLastMessage("debug", callsiteMethodName);
        }

#if NET35
        [Fact(Skip = "NET35 not supporting async callstack")]
#elif MONO
        [Fact(Skip = "Not working under MONO - not sure if unit test is wrong, or the code")]
#else
        [Fact]
#endif
        public void LogAfterTaskRunAwait_CleanNamesOfAsyncContinuationsIsTrue_ShouldCleanClassName()
        {
            // full name of the logging method
            string callsiteMethodFullName = $"{GetType()}.{nameof(LogAfterTaskRunAwait_CleanNamesOfAsyncContinuationsIsTrue_ShouldCleanClassName)}";

            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
                <nlog>
                    <targets><target name='debug' type='Debug' layout='${callsite:classname=true:includenamespace=true:cleannamesofasynccontinuations=true:cleanNamesOfAnonymousDelegates=true}' /></targets>
                    <rules>
                        <logger name='*' levels='Debug' writeTo='debug' />
                    </rules>
                </nlog>");

            Task.Run(async () =>
            {
                await AMinimalAsyncMethod();
                var logger = LogManager.GetCurrentClassLogger();
                logger.Debug("dude");
            }).Wait();

            AssertDebugLastMessage("debug", callsiteMethodFullName);

            new InnerClassAsyncMethod6().AsyncMethod6a().Wait();
            AssertDebugLastMessage("debug", $"{typeof(InnerClassAsyncMethod6).ToString()}.AsyncMethod6a");

            new InnerClassAsyncMethod6().AsyncMethod6b();
            AssertDebugLastMessage("debug", $"{typeof(InnerClassAsyncMethod6).ToString()}.AsyncMethod6b");
        }

#if NET35
        [Fact(Skip = "NET35 not supporting async callstack")]
#elif MONO
        [Fact(Skip = "Not working under MONO - not sure if unit test is wrong, or the code")]
#else
        [Fact]
#endif
        public void LogAfterTaskRunAwait_CleanNamesOfAsyncContinuationsIsFalse_ShouldNotCleanNames()
        {
            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
                <nlog>
                    <targets><target name='debug' type='Debug' layout='${callsite:includenamespace=true:cleannamesofasynccontinuations=false}' /></targets>
                    <rules>
                        <logger name='*' levels='Debug' writeTo='debug' />
                    </rules>
                </nlog>");

            Task.Run(async () => {
                await AMinimalAsyncMethod();
                var logger = LogManager.GetCurrentClassLogger();
                logger.Debug("dude");
            }).Wait();

            AssertDebugLastMessageContains("debug", "NLog.UnitTests.LayoutRenderers.CallSiteTests");
            AssertDebugLastMessageContains("debug", "MoveNext");
            AssertDebugLastMessageContains("debug", "b__");
        }

        private class InnerClassAsyncMethod6
        {
            public virtual async Task AsyncMethod6a()
            {
                await Task.Run(async () =>
                {
                    await AMinimalAsyncMethod();
                    var logger = LogManager.GetCurrentClassLogger();
                    logger.Debug("dude");
                });
            }

            public virtual void AsyncMethod6b()
            {
                Task.Run(async () =>
                {
                    await AMinimalAsyncMethod();
                    var logger = LogManager.GetCurrentClassLogger();
                    logger.Debug("dude");
                }).Wait();
            }

            private async Task AMinimalAsyncMethod()
            {
                await Task.Delay(1);    // Ensure it always becomes async, and it is not inlined
            }
        }

        public interface INLogLogger
        {
            void Log(LogLevel logLevel, string message);
        }

        public abstract class NLogLoggerBase : INLogLogger
        {
            protected abstract Logger Logger { get; }

            void INLogLogger.Log(LogLevel logLevel, string message)
            {
                Logger.Log(typeof(INLogLogger), new LogEventInfo(logLevel, Logger.Name, message));
            }
        }

        /// <summary>
        ///   Implementation of <see cref="ILogger" /> for NLog.
        /// </summary>
        public class NLogLogger : NLogLoggerBase
        {
            /// <summary>
            ///   Initializes a new instance of the <see cref="NLogLogger" /> class.
            /// </summary>
            /// <param name="logger"> The logger. </param>
            public NLogLogger(Logger logger)
            {
                Logger = logger;
            }

            /// <summary>
            ///   Gets or sets the logger.
            /// </summary>
            /// <value> The logger. </value>
            protected override Logger Logger { get; }

            /// <summary>
            ///   Returns a <see cref="string" /> that represents this instance.
            /// </summary>
            /// <returns> A <see cref="string" /> that represents this instance. </returns>
            public override string ToString()
            {
                return Logger.ToString();
            }

            /// <summary>
            ///   Logs a debug message.
            /// </summary>
            /// <param name="message"> The message to log </param>
            public void Debug(string message)
            {
                Log(LogLevel.Debug, message);
            }

            public void Log(LogLevel logLevel, string message)
            {
                Logger.Log(typeof(NLogLogger), new LogEventInfo(logLevel, Logger.Name, message));
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void CallsiteTargetUsesStackTraceTest(bool includeStackTraceUsage)
        {
            var target = new MyTarget() { StackTraceUsage = includeStackTraceUsage ? StackTraceUsage.WithStackTrace : StackTraceUsage.None };
            SimpleConfigurator.ConfigureForTargetLogging(target);
            var logger = LogManager.GetLogger(nameof(CallsiteTargetUsesStackTraceTest));
            string t = null;
            logger.Info("Testing null:{0}", t);
            Assert.NotNull(target.LastEvent);
            if (includeStackTraceUsage)
                Assert.NotNull(target.LastEvent.StackTrace);
            else
                Assert.Null(target.LastEvent.StackTrace);
        }

        [Theory]
        [InlineData(true, StackTraceUsage.WithCallSite | StackTraceUsage.WithCallSiteClassName)]
        [InlineData(false, StackTraceUsage.WithCallSite | StackTraceUsage.WithCallSiteClassName)]   // Will capture StackTrace automatically
        [InlineData(true, StackTraceUsage.WithCallSiteClassName)]
        [InlineData(false, StackTraceUsage.WithCallSiteClassName)]  // Will NOT capture StackTrace automatically
        public void CallsiteTargetSkipsStackTraceTest(bool includeLogEventCallSite, StackTraceUsage stackTraceUsage)
        {
            var target = new MyTarget() { StackTraceUsage = stackTraceUsage };
            SimpleConfigurator.ConfigureForTargetLogging(target);
            var logger = LogManager.GetLogger(nameof(CallsiteTargetUsesStackTraceTest));
            var logEvent = LogEventInfo.Create(LogLevel.Info, logger.Name, "Hello");
            if (includeLogEventCallSite)
                logEvent.SetCallerInfo(nameof(CallSiteTests), nameof(CallsiteTargetSkipsStackTraceTest), string.Empty, 0);
            logger.Log(logEvent);
            Assert.NotNull(target.LastEvent);

            if (includeLogEventCallSite || stackTraceUsage == StackTraceUsage.WithCallSiteClassName)
                Assert.Null(target.LastEvent.StackTrace);
            else
                Assert.NotNull(target.LastEvent.StackTrace);

            if (includeLogEventCallSite || stackTraceUsage != StackTraceUsage.WithCallSiteClassName)
                Assert.Equal(nameof(CallSiteTests), target.LastEvent.GetCallSiteInformationInternal().GetCallerClassName(null, false, true, true));
        }

        public class MyTarget : TargetWithLayout, IUsesStackTrace
        {
            public MyTarget()
            {
            }

            public MyTarget(string name) : this()
            {
                Name = name;
            }

            public LogEventInfo LastEvent { get; private set; }

            public new StackTraceUsage StackTraceUsage { get; set; }

            protected override void Write(LogEventInfo logEvent)
            {
                LastEvent = logEvent;
                base.Write(logEvent);
            }
        }
    }
}
