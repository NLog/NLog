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

namespace NLog.UnitTests.LayoutRenderers
{
    using System;
    using System.Reflection;
    using System.Threading;
    using Xunit;

    public class CallSiteTests : NLogTestBase
    {
#if !SILVERLIGHT
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

            Assert.False(results.Errors.HasErrors);

            // create nlog configuration
            LogManager.Configuration = CreateConfigurationFromString(@"
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

#if !SILVERLIGHT
#if MONO
        [Fact(Skip="Not working under MONO - not sure if unit test is wrong, or the code")]
#else
        [Fact]
#endif
        public void LineNumberTest()
        {
            LogManager.Configuration = CreateConfigurationFromString(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${callsite:filename=true} ${message}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>");

            ILogger logger = LogManager.GetLogger("A");
#if !NET4_5 && !MONO
#line 100000
#endif
            logger.Debug("msg");
            var linenumber = GetPrevLineNumber();
            string lastMessage = GetDebugLastMessage("debug");
            // There's a difference in handling line numbers between .NET and Mono
            // We're just interested in checking if it's above 100000
            Assert.True(lastMessage.IndexOf("callsitetests.cs:" + linenumber, StringComparison.OrdinalIgnoreCase) >= 0, "Invalid line number. Expected prefix of 10000, got: " + lastMessage);
#if !NET4_5 && !MONO
#line default
#endif
        }
#endif

        [Fact]
        public void MethodNameTest()
        {
            LogManager.Configuration = CreateConfigurationFromString(@"
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
        public void ClassNameTest()
        {
            LogManager.Configuration = CreateConfigurationFromString(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${callsite:classname=true:methodname=false} ${message}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>");

            ILogger logger = LogManager.GetLogger("A");
            logger.Debug("msg");
            MethodBase currentMethod = MethodBase.GetCurrentMethod();
            AssertDebugLastMessage("debug", currentMethod.DeclaringType.FullName + " msg");
        }

        [Fact]
        public void ClassNameWithPaddingTestPadLeftAlignLeftTest()
        {
            LogManager.Configuration = CreateConfigurationFromString(@"
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
            LogManager.Configuration = CreateConfigurationFromString(@"
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
            LogManager.Configuration = CreateConfigurationFromString(@"
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
            LogManager.Configuration = CreateConfigurationFromString(@"
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
            LogManager.Configuration = CreateConfigurationFromString(@"
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
            LogManager.Configuration = CreateConfigurationFromString(@"
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
            LogManager.Configuration = CreateConfigurationFromString(@"
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
            LogManager.Configuration = CreateConfigurationFromString(@"
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
            LogManager.Configuration = CreateConfigurationFromString(@"
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

        [Fact]
        public void GivenOneSkipFrameDefined_WhenLogging_ShouldSkipOneUserStackFrame()
        {
            LogManager.Configuration = CreateConfigurationFromString(@"
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

#if MONO
        [Fact(Skip="Not working under MONO - not sure if unit test is wrong, or the code")]
#else
        [Fact]
#endif
        public void CleanMethodNamesOfAnonymousDelegatesTest()
        {
            LogManager.Configuration = CreateConfigurationFromString(@"
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

#if MONO
        [Fact(Skip="Not working under MONO - not sure if unit test is wrong, or the code")]
#else
        [Fact]
#endif
        public void DontCleanMethodNamesOfAnonymousDelegatesTest()
        {
            LogManager.Configuration = CreateConfigurationFromString(@"
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
                Assert.True(lastMessage.StartsWith("<DontCleanMethodNamesOfAnonymousDelegatesTest>"));
            }
        }

#if MONO
        [Fact(Skip="Not working under MONO - not sure if unit test is wrong, or the code")]
#else
        [Fact]
#endif
        public void CleanClassNamesOfAnonymousDelegatesTest()
        {
            LogManager.Configuration = CreateConfigurationFromString(@"
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

#if MONO
        [Fact(Skip="Not working under MONO - not sure if unit test is wrong, or the code")]
#else
        [Fact]
#endif
        public void DontCleanClassNamesOfAnonymousDelegatesTest()
        {
            LogManager.Configuration = CreateConfigurationFromString(@"
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
                Assert.True(lastMessage.Contains("+<>"));
            }
        }


        [Fact]
        public void When_Wrapped_Ignore_Wrapper_Methods_In_Callstack()
        {

            //namespace en name of current method
            const string currentMethodFullName = "NLog.UnitTests.LayoutRenderers.CallSiteTests.When_Wrapped_Ignore_Wrapper_Methods_In_Callstack";

            LogManager.Configuration = CreateConfigurationFromString(@"
               <nlog>
                   <targets><target name='debug' type='Debug' layout='${callsite}|${message}' /></targets>
                   <rules>
                       <logger name='*' levels='Warn' writeTo='debug' />
                   </rules>
               </nlog>");

            var logger = LogManager.GetLogger("A");
            logger.Warn("direct");
            AssertDebugLastMessage("debug", string.Format("{0}|direct", currentMethodFullName));

            LoggerTests.BaseWrapper wrappedLogger = new LoggerTests.MyWrapper();
            wrappedLogger.Log("wrapped");
            AssertDebugLastMessage("debug", string.Format("{0}|wrapped", currentMethodFullName));


        }



        #region Compositio unit test

        [Fact]
        public void When_WrappedInCompsition_Ignore_Wrapper_Methods_In_Callstack()
        {

            //namespace en name of current method
            const string currentMethodFullName = "NLog.UnitTests.LayoutRenderers.CallSiteTests.When_WrappedInCompsition_Ignore_Wrapper_Methods_In_Callstack";

            LogManager.Configuration = CreateConfigurationFromString(@"
           <nlog>
               <targets><target name='debug' type='Debug' layout='${callsite}|${message}' /></targets>
               <rules>
                   <logger name='*' levels='Warn' writeTo='debug' />
               </rules>
           </nlog>");

            var logger = LogManager.GetLogger("A");
            logger.Warn("direct");
            AssertDebugLastMessage("debug", string.Format("{0}|direct", currentMethodFullName));

            CompositeWrapper wrappedLogger = new CompositeWrapper();
            wrappedLogger.Log("wrapped");
            AssertDebugLastMessage("debug", string.Format("{0}|wrapped", currentMethodFullName));

        }



        public class CompositeWrapper
        {
            private readonly MyWrapper wrappedLogger;
            public CompositeWrapper()
            {
                wrappedLogger = new MyWrapper();
            }
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
            LogManager.Configuration = CreateConfigurationFromString(@"
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
            LogManager.Configuration = CreateConfigurationFromString(@"
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
            LogManager.Configuration = CreateConfigurationFromString(@"
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


    }

}
