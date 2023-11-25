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
    using System.IO;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Threading.Tasks;
    using NLog.Config;
    using NLog.Internal;
    using NLog.Layouts;
    using NLog.Targets;
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
                                        public HiddenAssemblyLogger(NLog.Logger logger)
                                        {
                                            LogDebug(logger);
                                        }

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

            // Add the previously generated assembly to the "blacklist"
            LogManager.AddHiddenAssembly(compiledAssembly);

            // instantiate the HiddenAssemblyLogger from previously generated assembly
            object instance = Activator.CreateInstance(hiddenAssemblyLoggerType, new object[] { logger });

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
            var logFactory = new LogFactory().Setup().LoadConfigurationFromXml(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${callsite:filename=true} ${message}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>").LogFactory;

            var logger = logFactory.GetLogger("A");
#if DEBUG
#line 100000
#endif
            logger.Debug("msg");
            var linenumber = GetPrevLineNumber();
            string lastMessage = GetDebugLastMessage("debug", logFactory);
            // There's a difference in handling line numbers between .NET and Mono
            // We're just interested in checking if it's above 100000
            Assert.Contains("callsitetests.cs:" + linenumber, lastMessage); // Expected prefix of 10000
#if DEBUG
#line default
#endif
        }

        [Fact]
        public void MethodNameTest()
        {
            var logFactory = new LogFactory().Setup().LoadConfigurationFromXml(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${callsite} ${message}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>").LogFactory;

            var logger = logFactory.GetLogger("A");
            logger.Debug("msg");
            MethodBase currentMethod = MethodBase.GetCurrentMethod();
            logFactory.AssertDebugLastMessage("debug", currentMethod.DeclaringType.FullName + "." + currentMethod.Name + " msg");
        }

        [Fact]
        public void MethodNameInChainTest()
        {
            var logFactory = new LogFactory().Setup().LoadConfigurationFromXml(@"
            <nlog>
                <targets>
                    <target name='debug' type='Debug' layout='${message}' />
                    <target name='debug2' type='Debug' layout='${callsite} ${message}' />
                </targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug,debug2' />
                </rules>
            </nlog>").LogFactory;

            var logger = logFactory.GetLogger("A");
            logger.Debug("msg2");
            MethodBase currentMethod = MethodBase.GetCurrentMethod();
            logFactory.AssertDebugLastMessage("debug2", currentMethod.DeclaringType.FullName + "." + currentMethod.Name + " msg2");
        }

        [Fact]
        public void MethodNameNoCaptureStackTraceTest()
        {
            // Arrange
            var logFactory = new LogFactory().Setup().LoadConfigurationFromXml(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${callsite:captureStackTrace=false} ${message}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>").LogFactory;

            // Act
            logFactory.GetLogger("A").Debug("msg");

            // Assert
            logFactory.AssertDebugLastMessage(" msg");
        }

        [Fact]
        public void MethodNameNoCaptureStackTraceWithStackTraceTest()
        {
            // Arrange
            var logFactory = new LogFactory().Setup().LoadConfigurationFromXml(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${callsite:captureStackTrace=false} ${message}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>").LogFactory;
            MethodBase currentMethod = MethodBase.GetCurrentMethod();

            // Act
            var logEvent = new LogEventInfo(LogLevel.Info, null, "msg");
            logEvent.SetStackTrace(new System.Diagnostics.StackTrace(true), 0);
            logFactory.GetLogger("A").Log(logEvent);

            // Assert
            logFactory.AssertDebugLastMessage(currentMethod.DeclaringType.FullName + "." + currentMethod.Name + " msg");
        }

        [Fact]
        public void ClassNameTest()
        {
            var logFactory = new LogFactory().Setup().LoadConfigurationFromXml(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${callsite:classname=true:methodname=false} ${message}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>").LogFactory;

            var logger = logFactory.GetLogger("A");
            logger.Debug("msg");
            logFactory.AssertDebugLastMessage("NLog.UnitTests.LayoutRenderers.CallSiteTests msg");
        }

        [Fact]
        public void ClassNameTestWithoutNamespace()
        {
            var logFactory = new LogFactory().Setup().LoadConfigurationFromXml(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${callsite:classname=true:methodname=false:includeNamespace=false} ${message}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>").LogFactory;

            var logger = logFactory.GetLogger("A");
            logger.Debug("msg");
            logFactory.AssertDebugLastMessage("CallSiteTests msg");
        }

        [Fact]
        public void ClassNameTestWithOverride()
        {
            var logFactory = new LogFactory().Setup().LoadConfigurationFromXml(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${callsite:classname=true:methodname=false:includeNamespace=false} ${message}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>").LogFactory;

            var logger = logFactory.GetLogger("A");
            var logEvent = LogEventInfo.Create(LogLevel.Debug, logger.Name, "msg");
            logEvent.SetCallerInfo("NLog.UnitTests.LayoutRenderers.OverrideClassName", nameof(ClassNameTestWithOverride), null, 0);
            logger.Log(logEvent);
            logFactory.AssertDebugLastMessage("OverrideClassName msg");
        }

        [Fact]
        public void ClassNameWithPaddingTestPadLeftAlignLeftTest()
        {
            var logFactory = new LogFactory().Setup().LoadConfigurationFromXml(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${callsite:classname=true:methodname=false:padding=3:fixedlength=true} ${message}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>").LogFactory;

            var logger = logFactory.GetLogger("A");
            logger.Debug("msg");
            MethodBase currentMethod = MethodBase.GetCurrentMethod();
            logFactory.AssertDebugLastMessage(currentMethod.DeclaringType.FullName.Substring(0, 3) + " msg");
        }

        [Fact]
        public void ClassNameWithPaddingTestPadLeftAlignRightTest()
        {
            var logFactory = new LogFactory().Setup().LoadConfigurationFromXml(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${callsite:classname=true:methodname=false:padding=3:fixedlength=true:alignmentOnTruncation=right} ${message}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>").LogFactory;

            var logger = logFactory.GetLogger("A");
            logger.Debug("msg");
            MethodBase currentMethod = MethodBase.GetCurrentMethod();
            var typeName = currentMethod.DeclaringType.FullName;
            logFactory.AssertDebugLastMessage(typeName.Substring(typeName.Length - 3) + " msg");
        }

        [Fact]
        public void ClassNameWithPaddingTestPadRightAlignLeftTest()
        {
            var logFactory = new LogFactory().Setup().LoadConfigurationFromXml(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${callsite:classname=true:methodname=false:padding=-3:fixedlength=true:alignmentOnTruncation=left} ${message}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>").LogFactory;

            var logger = logFactory.GetLogger("A");
            logger.Debug("msg");
            MethodBase currentMethod = MethodBase.GetCurrentMethod();
            logFactory.AssertDebugLastMessage(currentMethod.DeclaringType.FullName.Substring(0, 3) + " msg");
        }

        [Fact]
        public void ClassNameWithPaddingTestPadRightAlignRightTest()
        {
            var logFactory = new LogFactory().Setup().LoadConfigurationFromXml(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${callsite:classname=true:methodname=false:padding=-3:fixedlength=true:alignmentOnTruncation=right} ${message}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>").LogFactory;

            var logger = logFactory.GetLogger("A");
            logger.Debug("msg");
            MethodBase currentMethod = MethodBase.GetCurrentMethod();
            var typeName = currentMethod.DeclaringType.FullName;
            logFactory.AssertDebugLastMessage(typeName.Substring(typeName.Length - 3) + " msg");
        }

        [Fact]
        public void MethodNameWithPaddingTestPadLeftAlignLeftTest()
        {
            var logFactory = new LogFactory().Setup().LoadConfigurationFromXml(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${callsite:classname=false:methodname=true:padding=16:fixedlength=true} ${message}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>").LogFactory;

            var logger = logFactory.GetLogger("A");
            logger.Debug("msg");
            logFactory.AssertDebugLastMessage("MethodNameWithPa msg");
        }

        [Fact]
        public void MethodNameWithPaddingTestPadLeftAlignRightTest()
        {
            var logFactory = new LogFactory().Setup().LoadConfigurationFromXml(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${callsite:classname=false:methodname=true:padding=16:fixedlength=true:alignmentOnTruncation=right} ${message}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>").LogFactory;

            var logger = logFactory.GetLogger("A");
            logger.Debug("msg");
            logFactory.AssertDebugLastMessage("ftAlignRightTest msg");
        }

        [Fact]
        public void MethodNameWithPaddingTestPadRightAlignLeftTest()
        {
            var logFactory = new LogFactory().Setup().LoadConfigurationFromXml(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${callsite:classname=false:methodname=true:padding=-16:fixedlength=true:alignmentOnTruncation=left} ${message}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>").LogFactory;

            var logger = logFactory.GetLogger("A");
            logger.Debug("msg");
            logFactory.AssertDebugLastMessage("MethodNameWithPa msg");
        }

        [Fact]
        public void MethodNameWithPaddingTestPadRightAlignRightTest()
        {
            var logFactory = new LogFactory().Setup().LoadConfigurationFromXml(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${callsite:classname=false:methodname=true:padding=-16:fixedlength=true:alignmentOnTruncation=right} ${message}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>").LogFactory;

            var logger = logFactory.GetLogger("A");
            logger.Debug("msg");
            logFactory.AssertDebugLastMessage("htAlignRightTest msg");
        }

        [Fact]
        public void GivenSkipFrameNotDefined_WhenLogging_ThenLogFirstUserStackFrame()
        {
            var logFactory = new LogFactory().Setup().LoadConfigurationFromXml(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${callsite} ${message}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>").LogFactory;

            var logger = logFactory.GetLogger("A");
            logger.Debug("msg");
            logFactory.AssertDebugLastMessage("NLog.UnitTests.LayoutRenderers.CallSiteTests.GivenSkipFrameNotDefined_WhenLogging_ThenLogFirstUserStackFrame msg");
        }

#if !DEBUG
        [Fact(Skip = "RELEASE not working, only DEBUG")]
#else
        [Fact]
#endif
        public void GivenOneSkipFrameDefined_WhenLogging_ShouldSkipOneUserStackFrame()
        {
            var logFactory = new LogFactory().Setup().LoadConfigurationFromXml(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${callsite:skipframes=1} ${message}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>").LogFactory;

            var logger = logFactory.GetLogger("A");
            Action action = () => logger.Debug("msg");
            action.Invoke();
            logFactory.AssertDebugLastMessage("NLog.UnitTests.LayoutRenderers.CallSiteTests.GivenOneSkipFrameDefined_WhenLogging_ShouldSkipOneUserStackFrame msg");
        }

        [Fact]
        public void CleanMethodNamesOfAnonymousDelegatesTest()
        {
            var logFactory = new LogFactory().Setup().LoadConfigurationFromXml(@"
                <nlog>
                    <targets><target name='debug' type='Debug' layout='${callsite:ClassName=false:CleanNamesOfAnonymousDelegates=true}' /></targets>
                    <rules>
                        <logger name='*' levels='Fatal' writeTo='debug' />
                    </rules>
                </nlog>").LogFactory;

            var logger = logFactory.GetLogger("A");

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
                logFactory.AssertDebugLastMessage("CleanMethodNamesOfAnonymousDelegatesTest");
            }
        }

        [Fact]
        public void DontCleanMethodNamesOfAnonymousDelegatesTest()
        {
            var logFactory = new LogFactory().Setup().LoadConfigurationFromXml(@"
                <nlog>
                    <targets><target name='debug' type='Debug' layout='${callsite:ClassName=false:CleanNamesOfAnonymousDelegates=false}' /></targets>
                    <rules>
                        <logger name='*' levels='Fatal' writeTo='debug' />
                    </rules>
                </nlog>").LogFactory;

            var logger = logFactory.GetLogger("A");

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
                string lastMessage = GetDebugLastMessage("debug", logFactory);
                Assert.StartsWith("<DontCleanMethodNamesOfAnonymousDelegatesTest>", lastMessage);
            }
        }

        [Fact]
        public void CleanClassNamesOfAnonymousDelegatesTest()
        {
            var logFactory = new LogFactory().Setup().LoadConfigurationFromXml(@"
                <nlog>
                    <targets><target name='debug' type='Debug' layout='${callsite:ClassName=true:MethodName=false:CleanNamesOfAnonymousDelegates=true}' /></targets>
                    <rules>
                        <logger name='*' levels='Fatal' writeTo='debug' />
                    </rules>
                </nlog>").LogFactory;

            var logger = logFactory.GetLogger("A");

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
                logFactory.AssertDebugLastMessage("NLog.UnitTests.LayoutRenderers.CallSiteTests");
            }
        }

        [Fact]
        public void DontCleanClassNamesOfAnonymousDelegatesTest()
        {
            var logFactory = new LogFactory().Setup().LoadConfigurationFromXml(@"
                <nlog>
                    <targets><target name='debug' type='Debug' layout='${callsite:ClassName=true:MethodName=false:CleanNamesOfAnonymousDelegates=false}' /></targets>
                    <rules>
                        <logger name='*' levels='Fatal' writeTo='debug' />
                    </rules>
                </nlog>").LogFactory;

            var logger = logFactory.GetLogger("A");

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
                logFactory.AssertDebugLastMessageContains("+<>");
            }
        }

        [Fact]
        public void When_NotIncludeNameSpace_Then_CleanAnonymousDelegateClassNameShouldReturnParentClassName()
        {
            var logFactory = new LogFactory().Setup().LoadConfigurationFromXml(@"
                <nlog>
                    <targets><target name='debug' type='Debug' layout='${callsite:ClassName=true:MethodName=false:IncludeNamespace=false:CleanNamesOfAnonymousDelegates=true}' /></targets>
                    <rules>
                        <logger name='*' levels='Fatal' writeTo='debug' />
                    </rules>
                </nlog>").LogFactory;

            var logger = logFactory.GetLogger("A");

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
                logFactory.AssertDebugLastMessage("CallSiteTests");
            }
        }


        [Fact]
        public void When_Wrapped_Ignore_Wrapper_Methods_In_Callstack()
        {
            //namespace en name of current method
            const string currentMethodFullName = "NLog.UnitTests.LayoutRenderers.CallSiteTests.When_Wrapped_Ignore_Wrapper_Methods_In_Callstack";

            var logFactory = new LogFactory().Setup().LoadConfigurationFromXml(@"
               <nlog>
                   <targets><target name='debug' type='Debug' layout='${callsite}|${message}' /></targets>
                   <rules>
                       <logger name='*' levels='Warn' writeTo='debug' />
                   </rules>
               </nlog>").LogFactory;

            var logger = logFactory.GetLogger("A");
            logger.Warn("direct");
            logFactory.AssertDebugLastMessage($"{currentMethodFullName}|direct");

            var wrappedLogger = new MyWrapper(logFactory);
            wrappedLogger.Log("wrapped");
            logFactory.AssertDebugLastMessage($"{currentMethodFullName}|wrapped");

            var fluentLogger = new MyFluentWrapper(logFactory);
            fluentLogger.Log("wrapped");
            logFactory.AssertDebugLastMessage($"{currentMethodFullName}|wrapped");
        }

        [Fact]
        public void CheckStackTraceUsageForTwoRules()
        {
            var logFactory = new LogFactory().Setup().LoadConfigurationFromXml(@"
            <nlog>
                <targets>
                    <target name='debug' type='Debug' layout='${message}' />
                    <target name='debug2' type='Debug' layout='${callsite} ${message}' />
                </targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                    <logger name='*' minlevel='Debug' writeTo='debug2' />
                </rules>
            </nlog>").LogFactory;

            var logger = logFactory.GetLogger("A");
            logger.Debug("msg");
            logFactory.AssertDebugLastMessage("debug2", "NLog.UnitTests.LayoutRenderers.CallSiteTests.CheckStackTraceUsageForTwoRules msg");
        }


        [Fact]
        public void CheckStackTraceUsageForTwoRules_chained()
        {
            var logFactory = new LogFactory().Setup().LoadConfigurationFromXml(@"
            <nlog>
                <targets>
                    <target name='debug' type='Debug' layout='${message}' />
                    <target name='debug2' type='Debug' layout='${callsite} ${message}' />
                </targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                    <logger name='*' minlevel='Debug' writeTo='debug,debug2' />
                </rules>
            </nlog>").LogFactory;

            var logger = logFactory.GetLogger("A");
            logger.Debug("msg");
            logFactory.AssertDebugLastMessage("debug2", "NLog.UnitTests.LayoutRenderers.CallSiteTests.CheckStackTraceUsageForTwoRules_chained msg");
        }


        [Fact]
        public void CheckStackTraceUsageForMultipleRules()
        {
            var logFactory = new LogFactory().Setup().LoadConfigurationFromXml(@"
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
            </nlog>").LogFactory;

            var logger = logFactory.GetLogger("A");
            logger.Debug("msg");
            logFactory.AssertDebugLastMessage("debug2", "NLog.UnitTests.LayoutRenderers.CallSiteTests.CheckStackTraceUsageForMultipleRules msg");
        }

        [Fact]
        public void When_WrappedInCompsition_Ignore_Wrapper_Methods_In_Callstack()
        {
            //namespace en name of current method
            const string currentMethodFullName = "NLog.UnitTests.LayoutRenderers.CallSiteTests.When_WrappedInCompsition_Ignore_Wrapper_Methods_In_Callstack";

            var logFactory = new LogFactory().Setup().LoadConfigurationFromXml(@"
           <nlog>
               <targets><target name='debug' type='Debug' layout='${callsite}|${message}' /></targets>
               <rules>
                   <logger name='*' levels='Warn' writeTo='debug' />
               </rules>
           </nlog>").LogFactory;

            var logger = logFactory.GetLogger("A");
            logger.Warn("direct");
            logFactory.AssertDebugLastMessage($"{currentMethodFullName}|direct");

            CompositeWrapper wrappedLogger = new CompositeWrapper(logFactory);
            wrappedLogger.Log("wrapped");
            logFactory.AssertDebugLastMessage($"{currentMethodFullName}|wrapped");
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

            var logFactory = new LogFactory().Setup().LoadConfigurationFromXml(@"
           <nlog>
               <targets><target name='debug' type='Debug' layout='${callsite}|${message}' /></targets>
               <rules>
                   <logger name='*' levels='Warn' writeTo='debug' />
               </rules>
           </nlog>").LogFactory;

            AsyncMethod(logFactory).Wait();
            logFactory.AssertDebugLastMessage($"{currentMethodFullName}|direct");

            new InnerClassAsyncMethod().AsyncMethod(logFactory).Wait();
            logFactory.AssertDebugLastMessage($"{typeof(InnerClassAsyncMethod).ToString()}.AsyncMethod|direct");
        }

        private async Task AsyncMethod(LogFactory logFactory)
        {
            var logger = logFactory.GetCurrentClassLogger();
            logger.Warn("direct");
            var reader = new StreamReader(new MemoryStream(ArrayHelper.Empty<byte>()));
            await reader.ReadLineAsync();
        }

        private class InnerClassAsyncMethod
        {
            public async Task AsyncMethod(LogFactory logFactory)
            {
                var logger = logFactory.GetCurrentClassLogger();
                logger.Warn("direct");
                var reader = new StreamReader(new MemoryStream(ArrayHelper.Empty<byte>()));
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
            var logFactory = new LogFactory().Setup().LoadConfigurationFromXml(@"
           <nlog>
               <targets><target name='debug' type='Debug' layout='${callsite:className=False:fileName=True:includeSourcePath=False:methodName=False}|${message}' /></targets>
               <rules>
                   <logger name='*' levels='Warn' writeTo='debug' />
               </rules>
           </nlog>").LogFactory;

            AsyncMethod(logFactory).Wait();
            logFactory.AssertDebugLastMessageContains("CallSiteTests.cs");
            logFactory.AssertDebugLastMessageContains("|direct");
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

            var logFactory = new LogFactory().Setup().LoadConfigurationFromXml(@"
           <nlog>
               <targets><target name='debug' type='Debug' layout='${callsite}|${message}' /></targets>
               <rules>
                   <logger name='*' levels='Warn' writeTo='debug' />
               </rules>
           </nlog>").LogFactory;

            AsyncMethod2a(logFactory).Wait();
            logFactory.AssertDebugLastMessage($"{currentMethodFullName}|direct");

            new InnerClassAsyncMethod2().AsyncMethod2a(logFactory).Wait();
            logFactory.AssertDebugLastMessage($"{typeof(InnerClassAsyncMethod2).ToString()}.AsyncMethod2b|direct");
        }

        private async Task AsyncMethod2a(LogFactory logFactory)
        {
            await AsyncMethod2b(logFactory);
        }

        private async Task AsyncMethod2b(LogFactory logFactory)
        {
            var logger = logFactory.GetCurrentClassLogger();
            logger.Warn("direct");
            var reader = new StreamReader(new MemoryStream(ArrayHelper.Empty<byte>()));
            await reader.ReadLineAsync();
        }

        private class InnerClassAsyncMethod2
        {
            public async Task AsyncMethod2a(LogFactory logFactory)
            {
                await AsyncMethod2b(logFactory);
            }

            public async Task AsyncMethod2b(LogFactory logFactory)
            {
                var logger = logFactory.GetCurrentClassLogger();
                logger.Warn("direct");
                var reader = new StreamReader(new MemoryStream(ArrayHelper.Empty<byte>()));
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

            var logFactory = new LogFactory().Setup().LoadConfigurationFromXml(@"
           <nlog>
               <targets><target name='debug' type='Debug' layout='${callsite}|${message}' /></targets>
               <rules>
                   <logger name='*' levels='Warn' writeTo='debug' />
               </rules>
           </nlog>").LogFactory;

            AsyncMethod3a(logFactory).Wait();
            logFactory.AssertDebugLastMessage($"{currentMethodFullName}|direct");

            new InnerClassAsyncMethod3().AsyncMethod3a(logFactory).Wait();
            logFactory.AssertDebugLastMessage($"{typeof(InnerClassAsyncMethod3).ToString()}.AsyncMethod3b|direct");
        }

        private async Task AsyncMethod3a(LogFactory logFactory)
        {
            var reader = new StreamReader(new MemoryStream(ArrayHelper.Empty<byte>()));
            await reader.ReadLineAsync();
            AsyncMethod3b(logFactory);
        }

        private void AsyncMethod3b(LogFactory logFactory)
        {
            var logger = logFactory.GetCurrentClassLogger();
            logger.Warn("direct");
        }

        private class InnerClassAsyncMethod3
        {
            public async Task AsyncMethod3a(LogFactory logFactory)
            {
                var reader = new StreamReader(new MemoryStream(ArrayHelper.Empty<byte>()));
                await reader.ReadLineAsync();
                AsyncMethod3b(logFactory);
            }

            public void AsyncMethod3b(LogFactory logFactory)
            {
                var logger = logFactory.GetCurrentClassLogger();
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

            var logFactory = new LogFactory().Setup().LoadConfigurationFromXml(@"
           <nlog>
               <targets><target name='debug' type='Debug' layout='${callsite}|${message}' /></targets>
               <rules>
                   <logger name='*' levels='Info' writeTo='debug' />
               </rules>
           </nlog>").LogFactory;

            AsyncMethod4(logFactory).Wait();
            logFactory.AssertDebugLastMessage($"{currentMethodFullName}|Direct, async method");

            new InnerClassAsyncMethod4().AsyncMethod4(logFactory).Wait();
            logFactory.AssertDebugLastMessage($"{typeof(InnerClassAsyncMethod4).ToString()}.AsyncMethod4|Direct, async method");
       }

        private async Task<IEnumerable<string>> AsyncMethod4(LogFactory logFactory)
        {
            Logger logger = logFactory.GetLogger("AnnonTest");
            logger.Info("Direct, async method");

            return await Task.FromResult(new string[] { "value1", "value2" });
        }

        private class InnerClassAsyncMethod4
        {
            public async Task<IEnumerable<string>> AsyncMethod4(LogFactory logFactory)
            {
                Logger logger = logFactory.GetLogger("AnnonTest");
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

            var logFactory = new LogFactory().Setup().LoadConfigurationFromXml(@"
           <nlog>
               <targets><target name='debug' type='Debug' layout='${callsite}|${message}' /></targets>
               <rules>
                   <logger name='*' levels='Warn' writeTo='debug' />
               </rules>
           </nlog>").LogFactory;

            MoveNext(logFactory);
            logFactory.AssertDebugLastMessage($"{currentMethodFullName}|direct");
        }

        private void MoveNext(LogFactory logFactory)
        {
            var logger = logFactory.GetCurrentClassLogger();
            logger.Warn("direct");
        }

        public class CompositeWrapper
        {
            private readonly MyWrapper wrappedLogger;

            public CompositeWrapper(LogFactory logFactory)
            {
                wrappedLogger = new MyWrapper(logFactory);
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

            public void Log(Type wrapperType, string what) //overloaded with type for composition
            {
                InternalLog(wrapperType, what);
            }

            protected abstract void InternalLog(Type wrapperType, string what);
        }

        public class MyWrapper : BaseWrapper
        {
            private readonly Logger _wrapperLogger;

            public MyWrapper(LogFactory logFactory)
            {
                _wrapperLogger = logFactory.GetLogger("WrappedLogger");
            }

            protected override void InternalLog(Type wrapperType, string what) //added type for composition
            {
                LogEventInfo info = new LogEventInfo(LogLevel.Warn, _wrapperLogger.Name, what);

                // Provide BaseWrapper as wrapper type.
                // Expected: UserStackFrame should point to the method that calls a 
                // method of BaseWrapper.
                _wrapperLogger.Log(wrapperType, info);
            }
        }

        public class MyFluentWrapper : BaseWrapper
        {
            private readonly Logger _wrapperLogger;

            public MyFluentWrapper(LogFactory logFactory)
            {
                _wrapperLogger = logFactory.GetLogger("WrappedLogger");
            }

            protected override void InternalLog(Type wrapperType, string what) //added type for composition
            {
                // Provide BaseWrapper as wrapper type.
                // Expected: UserStackFrame should point to the method that calls a 
                // method of BaseWrapper.
                _wrapperLogger.ForWarnEvent().Message(what).Log(wrapperType);
            }
        }

        private class MyLogger : Logger
        {
        }

        [Fact]
        public void CallsiteBySubclass_interface()
        {
            var logFactory = new LogFactory().Setup().LoadConfigurationFromXml(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${callsite:classname=true:methodname=true} ${message}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>").LogFactory;

            var logger = logFactory.GetLogger<MyLogger>("mylogger");

            Assert.True(logger is MyLogger, "logger isn't MyLogger");
            logger.Debug("msg");
            logFactory.AssertDebugLastMessage("NLog.UnitTests.LayoutRenderers.CallSiteTests.CallsiteBySubclass_interface msg");
        }

        [Fact]
        public void CallsiteBySubclass_mylogger()
        {
            var logFactory = new LogFactory().Setup().LoadConfigurationFromXml(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${callsite:classname=true:methodname=true} ${message}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>").LogFactory;

            MyLogger logger = logFactory.GetLogger<MyLogger>("mylogger");

            Assert.NotNull(logger);
            logger.Debug("msg");
            logFactory.AssertDebugLastMessage("NLog.UnitTests.LayoutRenderers.CallSiteTests.CallsiteBySubclass_mylogger msg");
        }

        [Fact]
        public void CallsiteBySubclass_logger()
        {
            var logFactory = new LogFactory().Setup().LoadConfigurationFromXml(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${callsite:classname=true:methodname=true} ${message}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>").LogFactory;

            Logger logger = logFactory.GetLogger<MyLogger>("mylogger");

            Assert.NotNull(logger);
            logger.Debug("msg");
            logFactory.AssertDebugLastMessage("NLog.UnitTests.LayoutRenderers.CallSiteTests.CallsiteBySubclass_logger msg");
        }

        [Fact]
        public void Should_preserve_correct_callsite_information()
        {
            var target = new MemoryTarget();
            target.Layout = "${date:format=HH\\:MM\\:ss} ${logger} ${message}";
            var logFactory = new LogFactory().Setup().LoadConfiguration(builder =>
            {
                var wrapper = new NLog.Targets.Wrappers.AsyncTargetWrapper(target) { TimeToSleepBetweenBatches = 0 };
                builder.ForLogger().WriteTo(wrapper);
            }).LogFactory;

            var factory = new NLogFactory(logFactory);
            var logger = factory.Create("MyLoggerName");

            WriteLogMessage(logger);
            logFactory.Flush();
            var logMessage = target.Logs[0];
            Assert.Contains("MyLoggerName", logMessage);

            // See that LogManager.ReconfigExistingLoggers() is able to upgrade the Logger
            target.Layout = "${date:format=HH\\:MM\\:ss} ${logger} ${callsite} ${message}";
            logFactory.ReconfigExistingLoggers();
            WriteLogMessage(logger);
            logFactory.Flush();
            logMessage = target.Logs[1];
            Assert.Contains("CallSiteTests.WriteLogMessage", logMessage);

            // See that LogManager.ReconfigExistingLoggers() is able to upgrade the Logger
            target.Layout = "${date:format=HH\\:MM\\:ss} ${logger} ${callsite} ${message} ThreadId=${threadid}";
            logFactory.ReconfigExistingLoggers();
            WriteLogMessage(logger);
            logFactory.Flush();
            logMessage = target.Logs[2];
            Assert.Contains("ThreadId=" + CurrentManagedThreadId.ToString(), logMessage);

            // See that interface logging also works (Improve support for Microsoft.Extension.Logging.ILogger replacement)
            INLogLogger ilogger = logger;
            WriteLogMessage(ilogger);
            logFactory.Flush();
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
            private readonly LogFactory LogFactory;

            /// <summary>
            ///   Initializes a new instance of the <see cref="NLogFactory" /> class.
            /// </summary>
            /// <param name="loggingConfiguration"> The NLog Configuration </param>
            public NLogFactory(LogFactory logFactory)
            {
                LogFactory = logFactory;
            }

            /// <summary>
            ///   Creates a logger with specified <paramref name="name" />.
            /// </summary>
            /// <param name="name"> The name. </param>
            /// <returns> </returns>
            public NLogLogger Create(string name)
            {
                var log = LogFactory.GetLogger(name);
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

            var logFactory = new LogFactory().Setup().LoadConfigurationFromXml(@"
           <nlog>
               <targets><target name='debug' type='Debug' layout='${callsite}|${message}' /></targets>
               <rules>
                   <logger name='*' levels='Debug' writeTo='debug' />
               </rules>
           </nlog>").LogFactory;

            AsyncMethod5(logFactory).Wait();
            logFactory.AssertDebugLastMessage($"{currentMethodFullName}|dude");

            new InnerClassAsyncMethod5().AsyncMethod5(logFactory).Wait();
            logFactory.AssertDebugLastMessage($"{typeof(InnerClassAsyncMethod5).ToString()}.AsyncMethod5|dude");
        }

        private async Task AsyncMethod5(LogFactory logFactory)
        {
            await AMinimalAsyncMethod();

            var logger = logFactory.GetCurrentClassLogger();
            logger.Debug("dude");
        }

        private class InnerClassAsyncMethod5
        {
            public async Task AsyncMethod5(LogFactory logFactory)
            {
                await AMinimalAsyncMethod();

                var logger = logFactory.GetCurrentClassLogger();
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

            var logFactory = new LogFactory().Setup().LoadConfigurationFromXml(@"
                <nlog>
                    <targets><target name='debug' type='Debug' layout='${callsite:classname=false:cleannamesofasynccontinuations=true}' /></targets>
                    <rules>
                        <logger name='*' levels='Debug' writeTo='debug' />
                    </rules>
                </nlog>").LogFactory;

            Task.Run(async () => {
                await AMinimalAsyncMethod();
                var logger = logFactory.GetCurrentClassLogger();
                logger.Debug("dude");
            }).Wait();

            logFactory.AssertDebugLastMessage(callsiteMethodName);
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

            var logFactory = new LogFactory().Setup().LoadConfigurationFromXml(@"
                <nlog>
                    <targets><target name='debug' type='Debug' layout='${callsite:classname=true:includenamespace=true:cleannamesofasynccontinuations=true:cleanNamesOfAnonymousDelegates=true}' /></targets>
                    <rules>
                        <logger name='*' levels='Debug' writeTo='debug' />
                    </rules>
                </nlog>").LogFactory;

            Task.Run(async () =>
            {
                await AMinimalAsyncMethod();
                var logger = logFactory.GetCurrentClassLogger();
                logger.Debug("dude");
            }).Wait();

            logFactory.AssertDebugLastMessage(callsiteMethodFullName);

            new InnerClassAsyncMethod6().AsyncMethod6a(logFactory).Wait();
            logFactory.AssertDebugLastMessage($"{typeof(InnerClassAsyncMethod6).ToString()}.AsyncMethod6a");

            new InnerClassAsyncMethod6().AsyncMethod6b(logFactory);
            logFactory.AssertDebugLastMessage($"{typeof(InnerClassAsyncMethod6).ToString()}.AsyncMethod6b");
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
            var logFactory = new LogFactory().Setup().LoadConfigurationFromXml(@"
                <nlog>
                    <targets><target name='debug' type='Debug' layout='${callsite:includenamespace=true:cleannamesofasynccontinuations=false}' /></targets>
                    <rules>
                        <logger name='*' levels='Debug' writeTo='debug' />
                    </rules>
                </nlog>").LogFactory;

            Task.Run(async () =>
            {
                await AMinimalAsyncMethod();
                var logger = logFactory.GetCurrentClassLogger();
                logger.Debug("dude");
            }).Wait();

            logFactory.AssertDebugLastMessageContains("NLog.UnitTests.LayoutRenderers.CallSiteTests");
            logFactory.AssertDebugLastMessageContains("MoveNext");
        }

        private class InnerClassAsyncMethod6
        {
            public virtual async Task AsyncMethod6a(LogFactory logFactory)
            {
                await Task.Run(async () =>
                {
                    await AMinimalAsyncMethod();
                    var logger = logFactory.GetCurrentClassLogger();
                    logger.Debug("dude");
                });
            }

            public virtual void AsyncMethod6b(LogFactory logFactory)
            {
                Task.Run(async () =>
                {
                    await AMinimalAsyncMethod();
                    var logger = logFactory.GetCurrentClassLogger();
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
            var logger = new LogFactory().Setup().LoadConfiguration(builder =>
            {
                builder.ForLogger().WriteTo(target);
            }).GetLogger(nameof(CallsiteTargetUsesStackTraceTest));

            string t = null;
            logger.Info("Testing null:{0}", t);
            Assert.NotNull(target.LastEvent);
            if (includeStackTraceUsage)
                Assert.NotNull(target.LastEvent.StackTrace);
            else
                Assert.Null(target.LastEvent.StackTrace);
        }

        [Theory]
        [InlineData(false, StackTraceUsage.WithCallSite)]
        [InlineData(true, StackTraceUsage.WithCallSite | StackTraceUsage.WithCallSiteClassName)]
        [InlineData(false, StackTraceUsage.WithCallSite | StackTraceUsage.WithCallSiteClassName)]   // Will capture StackTrace automatically
        [InlineData(false, StackTraceUsage.WithStackTrace)]
        [InlineData(true, StackTraceUsage.WithCallSiteClassName)]
        [InlineData(false, StackTraceUsage.WithCallSiteClassName)]  // Will NOT capture StackTrace automatically
        public void CallsiteTargetSkipsStackTraceTest(bool includeLogEventCallSite, StackTraceUsage stackTraceUsage)
        {
            var target = new MyTarget() { StackTraceUsage = stackTraceUsage };
            var target2 = new MyTarget() { StackTraceUsage = StackTraceUsage.WithCallSite };
            var logger = new LogFactory().Setup().LoadConfiguration(builder =>
            {
                builder.ForLogger().WriteTo(target);
            }).GetLogger(nameof(CallsiteTargetSkipsStackTraceTest));

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
