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

namespace DumpTestResultSummary
{
    using System;
    using System.Collections.Generic;
    using System.Xml.Linq;

    public class MSTestResultsFileAnalyzer
    {
        private readonly XNamespace xmlNamespace;
        private Dictionary<string, string> guid2unitTestName = new Dictionary<string, string>();
        public AnalysisMode Mode { get; set; }

        public MSTestResultsFileAnalyzer(XNamespace xmlNamespace)
        {
            this.xmlNamespace = xmlNamespace;
        }

        public bool IncludeOutput { get; set; }

        public string Label { get; set; }

        public void DumpSummary(XElement element)
        {
            ConsoleColor oldColor = Console.ForegroundColor;

            XElement testDefinitionsElement = element.Element(xmlNamespace + "TestDefinitions");
            XElement resultsElement = element.Element(xmlNamespace + "Results");

            foreach (XElement unitTestElement in testDefinitionsElement.Elements(xmlNamespace + "UnitTest"))
            {
                XElement testMethod = unitTestElement.Element(xmlNamespace + "TestMethod");
                var guid = (string) unitTestElement.Attribute("id");
                var className = (string) testMethod.Attribute("className");
                int p = className.IndexOf(',');
                if (p >= 0)
                {
                    className = className.Substring(0, p);
                }

                var methodName = (string) testMethod.Attribute("name");
                guid2unitTestName[guid] = className + "." + methodName;
            }

            int passedCount = 0;
            int totalCount = 0;

            foreach (XElement unitTestResult in resultsElement.Elements(xmlNamespace + "UnitTestResult"))
            {
                var outcome = (string) unitTestResult.Attribute("outcome");
                totalCount++;
                if (outcome == "Passed")
                {
                    passedCount++;
                    continue;
                }
            }

            Console.Write(this.Label.PadRight(35));
            if (passedCount == totalCount)
            {
                Console.ForegroundColor = ConsoleColor.Green;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
            }

            Console.WriteLine("Passed {0}/{1} ({2}%)", passedCount, totalCount,
                              Math.Round(100.0*passedCount/totalCount, 2));
            if (passedCount != totalCount)
            {
                Console.WriteLine();
            }

            foreach (XElement unitTestResult in resultsElement.Elements(xmlNamespace + "UnitTestResult"))
            {
                var testId = (string) unitTestResult.Attribute("testId");
                string testName = guid2unitTestName[testId];
                var outcome = (string) unitTestResult.Attribute("outcome");

                if (outcome == "Passed")
                {
                    continue;
                }

                switch (outcome)
                {
                    case "Failed":
                        Console.ForegroundColor = ConsoleColor.Red;
                        break;

                    default:
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        break;
                }

                Console.Write("    ");
                Console.Write(outcome);
                Console.Write(": ");
                Console.ForegroundColor = oldColor;
                Console.WriteLine(testName);

                XElement outputElement = unitTestResult.Element(xmlNamespace + "Output");
                if (outputElement != null && this.Mode == AnalysisMode.Detailed)
                {
                    var stdOut = (string) outputElement.Element(xmlNamespace + "StdOut");
                    var stdErr = (string) outputElement.Element(xmlNamespace + "StdErr");
                    XElement errorInfo = outputElement.Element(xmlNamespace + "ErrorInfo");
                    if (!string.IsNullOrEmpty(stdOut))
                    {
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.WriteLine("Standard output:");
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                        Console.WriteLine(stdOut);
                    }

                    if (!string.IsNullOrEmpty(stdErr))
                    {
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.WriteLine("Standard error:");
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                        Console.WriteLine(stdErr);
                    }

                    if (errorInfo != null)
                    {
                        var message = (string) errorInfo.Element(xmlNamespace + "Message");
                        if (!string.IsNullOrEmpty(message))
                        {
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.WriteLine("Error message:");
                            Console.ForegroundColor = ConsoleColor.DarkGray;
                            Console.WriteLine(message);
                        }

                        var stackTrace = (string) errorInfo.Element(xmlNamespace + "StackTrace");
                        if (!string.IsNullOrEmpty(message))
                        {
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.WriteLine("Stack Trace:");
                            Console.ForegroundColor = ConsoleColor.DarkGray;
                            Console.WriteLine(stackTrace);
                        }
                    }
                }
            }

            if (passedCount != totalCount)
            {
                Console.WriteLine();
            }

            Console.ForegroundColor = oldColor;
        }
    }
}