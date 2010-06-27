// 
// Copyright (c) 2004-2010 Jaroslaw Kowalski <jaak@jkowalski.net>
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

namespace RunUnitTests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Xml.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    public class TestHarness
    {
        private static XNamespace xmlns = "http://microsoft.com/schemas/VisualStudio/TeamTest/2006";

        public TestHarness()
        {
            this.TestClasses = new List<Type>();
            this.PassedTests = new List<MethodInfo>();
            this.FailedTests = new List<MethodInfo>();
        }

        public List<Type> TestClasses { get; private set; }
        public int PassCount { get; set; }
        public int FailCount { get; set; }
        public bool PrintPassedTests { get; set; }
        public List<MethodInfo> PassedTests { get; private set; }
        public List<MethodInfo> FailedTests { get; private set; }
        public Dictionary<MethodInfo, string> consoleOutput = new Dictionary<MethodInfo, string>();
        public Dictionary<MethodInfo, string> consoleError = new Dictionary<MethodInfo, string>();

        public void AddAssembly(Assembly assembly)
        {
            foreach (var type in assembly.GetExportedTypes())
            {
                if (type.IsDefined(typeof(TestClassAttribute), false))
                {
                    AddType(type);
                }
            }
        }

        public void AddType(Type type)
        {
            this.TestClasses.Add(type);
        }

        public void Run()
        {
            var testContext = new TestContext();

            foreach (var testClass in this.TestClasses)
            {
                foreach (var method in testClass.GetMethods(BindingFlags.Static | BindingFlags.Public))
                {
                    if (method.IsDefined(typeof(AssemblyInitializeAttribute), false))
                    {
                        Console.WriteLine("Initializing... ({0}.{1})", testClass.Name, method.Name);
                        method.Invoke(null, new object[] { testContext });
                        break;
                    }
                }
            }

            foreach (var testClass in this.TestClasses)
            {
                this.Run(testClass);
            }
        }

        public void SaveTrxFile(string fileName)
        {
            var testDefinitions = new XElement(xmlns + "TestDefinitions");
            var testEntries = new XElement(xmlns + "TestEntries");
            var results = new XElement(xmlns + "Results");

            string finalOutcome = (this.FailCount > 0) ? "Failed" : "Passed";
            string testListId = Guid.NewGuid().ToString().ToLowerInvariant();

            var testRun = new XElement(xmlns + "TestRun",
                new XAttribute("id", Guid.NewGuid().ToString().ToLowerInvariant()),
                new XAttribute("name", Environment.UserName + "@" + Environment.MachineName + " " + DateTime.Now),
                new XAttribute("runUser", Environment.UserDomainName + "\\" + Environment.MachineName),
                //new XElement(xmlns + "TestRunConfiguration",
                //    new XAttribute("id", Guid.NewGuid().ToString().ToLowerInvariant()),
                //    new XAttribute("name", "Default settings"),
                //    new XElement(xmlns + "Documentation", "Default configuration")),
                new XElement(xmlns + "ResultSummary",
                    new XAttribute("outcome", finalOutcome),
                    new XElement(xmlns + "Counters",
                        new XAttribute("total", this.PassCount + this.FailCount),
                        new XAttribute("passed", this.PassCount),
                        new XAttribute("failed", this.FailCount))),
                testDefinitions,
                new XElement(xmlns + "TestLists",
                    new XElement(xmlns + "TestList",
                        new XAttribute("name", "All results"),
                        new XAttribute("id", testListId))),
                testEntries,
                results);

            foreach (var testMethod in this.FailedTests)
            {
                AddResult(testListId, testDefinitions, testEntries, results, testMethod, "Failed");
            }

            foreach (var testMethod in this.PassedTests)
            {
                AddResult(testListId, testDefinitions, testEntries, results, testMethod, "Passed");
            }

            testRun.Save(fileName);
        }

        private void AddResult(string testListId, XElement testDefinitions, XElement testEntries, XElement results, MethodInfo testMethod, string outcome)
        {
            var unitTestId = Guid.NewGuid().ToString().ToLowerInvariant();
            var executionId = Guid.NewGuid().ToString().ToLowerInvariant();

            //// <UnitTest name="RetryingTargetWrapperTest1" storage="nlog.unittests.dll" id="8de73835-d8cc-98cb-0536-550b848c2b11">
            ////   <Css projectStructure="" iteration="" />
            ////   <Owners>
            ////     <Owner name="" />
            ////   </Owners>
            ////   <Execution id="cde26156-ae20-4e47-9428-3545d8257fd0" />
            ////   <TestMethod codeBase="D:/Work/NLog/build/bin/Debug/.NET Compact Framework 2.0/NLog.UnitTests.DLL" adapterTypeName="Microsoft.VisualStudio.TestTools.TestTypes.Unit.UnitTestAdapter, Microsoft.VisualStudio.QualityTools.Tips.UnitTest.Adapter" className="NLog.UnitTests.Targets.Wrappers.RetryingTargetWrapperTests, NLog.UnitTests, Version=1.0.0.0, Culture=neutral, PublicKeyToken=b793d3de60bec2b9" name="RetryingTargetWrapperTest1" />
            //// </UnitTest>

            var unitTest = new XElement(xmlns + "UnitTest",
                new XAttribute("name", testMethod.Name),
                new XAttribute("storage", testMethod.DeclaringType.Assembly.GetName().Name),
                new XAttribute("id", unitTestId),
                new XElement(xmlns + "Css",
                    new XAttribute("projectStructure", ""),
                    new XAttribute("iteration", "")),
                new XElement(xmlns + "Owners",
                    new XElement(xmlns + "Owner",
                        new XAttribute("name", ""))),
                new XElement(xmlns + "Execution",
                    new XAttribute("id", executionId)),
                new XElement(xmlns + "TestMethod",
                    new XAttribute("codeBase", testMethod.DeclaringType.Assembly.Location),
                    new XAttribute("adapterTypeName", "Microsoft.VisualStudio.TestTools.TestTypes.Unit.UnitTestAdapter, Microsoft.VisualStudio.QualityTools.Tips.UnitTest.Adapter"),
                    new XAttribute("className", testMethod.DeclaringType.AssemblyQualifiedName),
                    new XAttribute("name", testMethod.Name)));

            testDefinitions.Add(unitTest);

            //// <TestEntry testId="814f9fa6-8419-63d3-2fdd-d2b15046f0c3" executionId="78a5a6cb-b27f-4c7a-aaac-dec708deda6f" testListId="8c84fa94-04c1-424b-9868-57a2d4851a1d" />
            var testEntry = new XElement(xmlns + "TestEntry",
                new XAttribute("testId", unitTestId),
                new XAttribute("executionId", executionId),
                new XAttribute("testListId", testListId));
            testEntries.Add(testEntry);

            //// <UnitTestResult executionId="d17d594d-74e7-4f96-9799-02234a13cf0b" testId="e73f9e7d-5f5d-4189-1793-757ee8339443" testName="AsyncTargetWrapperCloseTest" computerName="Pocket PC 2003 SE Emulator" duration="00:00:01.0900623" startTime="2010-06-26T16:11:27.5989107-07:00" endTime="2010-06-26T16:11:28.6889730-07:00" testType="13cdc9d9-ddb5-4fa4-a97d-d965ccfc6d4b" outcome="Passed" testListId="8c84fa94-04c1-424b-9868-57a2d4851a1d">
            ////   <Output>
            ////   </Output>
            //// </UnitTestResult>
            XElement consoleOutputElement = null;
            XElement consoleErrorElement = null;

            string txt;
            if (this.consoleOutput.TryGetValue(testMethod, out txt))
            {
                consoleOutputElement = new XElement(xmlns + "StdOut", txt);
            }

            if (this.consoleError.TryGetValue(testMethod, out txt))
            {
                consoleErrorElement = new XElement(xmlns + "StdErr", txt);
            }

            var unitTestResult = new XElement(xmlns + "UnitTestResult",
                new XAttribute("executionId", executionId),
                new XAttribute("testId", unitTestId),
                new XAttribute("testType", "13cdc9d9-ddb5-4fa4-a97d-d965ccfc6d4b"),
                new XAttribute("testName", testMethod.Name),
                new XAttribute("outcome", outcome),
                new XAttribute("computerName", Environment.MachineName),
                new XAttribute("testListId", testListId),
                new XElement(xmlns + "Output",
                    consoleOutputElement,
                    consoleErrorElement));

            results.Add(unitTestResult);
        }

        private void Run(Type testClass)
        {
            var oldConsoleColor = Console.ForegroundColor;

            MethodInfo testInitializeMethod = null;
            MethodInfo testCleanupMethod = null;
            var methodsToRun = new List<MethodInfo>();

            foreach (var method in testClass.GetMethods())
            {
                if (method.IsDefined(typeof(TestInitializeAttribute), false))
                {
                    testInitializeMethod = method;
                }

                if (method.IsDefined(typeof(TestCleanupAttribute), false))
                {
                    testCleanupMethod = method;
                }

                if (method.IsDefined(typeof(TestMethodAttribute), false))
                {
                    methodsToRun.Add(method);
                }
            }

            foreach (var method in methodsToRun)
            {
                Exception gotException = null;

                var originalOut = Console.Out;
                var originalErr = Console.Error;

                var newOut = new StringWriter();
                var newError = new StringWriter();

                Console.SetOut(newOut);
                Console.SetError(newError);

                try
                {
                    var instance = Activator.CreateInstance(testClass);
                    if (testInitializeMethod != null)
                    {
                        testInitializeMethod.Invoke(instance, null);
                    }

                    method.Invoke(instance, null);

                    if (testCleanupMethod != null)
                    {
                        testCleanupMethod.Invoke(instance, null);
                    }
                }
                catch (TargetInvocationException ex)
                {
                    gotException = ex.InnerException;
                }
                finally
                {
                    Console.SetOut(originalOut);
                    Console.SetError(originalErr);
                }

                string stdout = newOut.ToString();
                string stderr = newError.ToString();

                var expectedExceptionAttribute = (ExpectedExceptionAttribute)Attribute.GetCustomAttribute(method, typeof(ExpectedExceptionAttribute), false);

                bool passed;
                
                if (gotException != null)
                {
                    if (expectedExceptionAttribute == null)
                    {
                        passed = false;
                    }
                    else
                    {
                        if (expectedExceptionAttribute.ExceptionType.IsAssignableFrom(gotException.GetType()))
                        {
                            passed = true;
                        }
                        else
                        {
                            passed = false;
                            gotException = new TestFailureException("Expected exception " + expectedExceptionAttribute.ExceptionType + ", but got " + gotException.GetType() + ".");
                        }
                    }
                }
                else
                {
                    if (expectedExceptionAttribute == null)
                    {
                        passed = true;
                    }
                    else
                    {
                        passed = false;
                        gotException = new TestFailureException("Expected exception " + expectedExceptionAttribute.ExceptionType + " which was not thrown.");
                    }
                }

                if (!passed)
                {
                    stderr += gotException.ToString();
                }

                if (!string.IsNullOrEmpty(stdout))
                {
                    this.consoleOutput[method] = stdout;
                }

                if (!string.IsNullOrEmpty(stderr))
                {
                    this.consoleError[method] = stderr;
                }

                if (passed)
                {
                    if (this.PrintPassedTests)
                    {
                        Console.Write("{0}.{1} ", testClass.Name, method.Name);
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.Write("PASSED");
                        Console.ForegroundColor = oldConsoleColor;
                        Console.WriteLine();
                    }

                    this.PassedTests.Add(method);
                    this.PassCount++;
                }
                else
                {
                    Console.Write("{0}.{1} ", testClass.Name, method.Name);
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write("FAILED {0}", gotException);
                    Console.ForegroundColor = oldConsoleColor;
                    Console.WriteLine();
                    this.FailCount++;
                    this.FailedTests.Add(method);
                }
            }
        }
    }
}
