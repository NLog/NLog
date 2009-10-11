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
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace NLog.UnitTests
{
    /// <summary>
    /// Source code tests.
    /// </summary>
    [TestFixture]
    public class SourceCodeTests
    {
        private static Regex classNameRegex = new Regex(@"^    (public |abstract |sealed |static |partial |internal )*(class|interface|struct|enum) (?<className>\w+)\b", RegexOptions.Compiled);
        private static Regex delegateTypeRegex = new Regex(@"^    (public |internal )delegate .*\b(?<delegateType>\w+)\(", RegexOptions.Compiled);
        private static string[] directoriesToVerify = new[]
            {
                "src/NLog",
                "tests/NLog.UnitTests"
            };

        private static ICollection<string> fileNamesToIgnore = new List<string>()
        {
            "AssemblyInfo.cs",
            "AssemblyBuildInfo.cs",
            "GlobalSuppressions.cs",
        };

        private string sourceCodeDirectory;
        private string licenseFile;
        private string[] licenseLines;

        [SetUp]
        public void Initialize()
        {
            this.sourceCodeDirectory = Directory.GetCurrentDirectory();
            while (true)
            {
                this.licenseFile = Path.Combine(sourceCodeDirectory, "LICENSE.txt");
                if (File.Exists(licenseFile))
                {
                    break;
                }

                this.sourceCodeDirectory = Path.GetDirectoryName(this.sourceCodeDirectory);
            }

            this.licenseLines = File.ReadAllLines(this.licenseFile);
        }

        [Test]
        public void VerifyFileHeaders()
        {
            int failedFiles = 0;

            foreach (string dir in directoriesToVerify)
            {
                foreach (string file in Directory.GetFiles(Path.Combine(this.sourceCodeDirectory, dir), "*.cs", SearchOption.AllDirectories))
                {
                    if (IgnoreFile(file))
                    {
                        continue;
                    }

                    if (!VerifySingleFile(file))
                    {
                        failedFiles++;
                        Console.WriteLine("Missing header: {0}", file);
                    }
                }
            }

            Assert.AreEqual(0, failedFiles, "One or more files don't have valid license headers.");
        }


        [Test]
        public void VerifyNamespacesAndClassNames()
        {
            int failedFiles = 0;

            foreach (string dir in directoriesToVerify)
            {
                failedFiles += VerifyClassNames(Path.Combine(this.sourceCodeDirectory, dir), Path.GetFileName(dir));
            }

            Assert.AreEqual(0, failedFiles, "One or more files don't have valid class names and/or namespaces.");
        }

        bool IgnoreFile(string file)
        {
            string baseName = Path.GetFileName(file);
            if (fileNamesToIgnore.Contains(baseName))
            {
                return true;
            }

            return false;
        }

        private bool VerifySingleFile(string file)
        {
            using (StreamReader reader = File.OpenText(file))
            {
                for (int i = 0; i < this.licenseLines.Length; ++i)
                {
                    string line = reader.ReadLine();
                    string expected = "// " + this.licenseLines[i];
                    if (line != expected)
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        private int VerifyClassNames(string path, string expectedNamespace)
        {
            int failureCount = 0;

            foreach (string file in Directory.GetFiles(path, "*.cs"))
            {
                if (IgnoreFile(file))
                {
                    continue;
                }

                string expectedClassName = Path.GetFileNameWithoutExtension(file);
                int p = expectedClassName.IndexOf('-');
                if (p >= 0)
                {
                    expectedClassName = expectedClassName.Substring(0, p);
                }

                if (!this.VerifySingleFile(file, expectedNamespace, expectedClassName))
                {
                    failureCount++;
                }
            }

            foreach (string dir in Directory.GetDirectories(path))
            {
                failureCount += VerifyClassNames(dir, expectedNamespace + "." + Path.GetFileName(dir));
            }

            return failureCount;
        }

        private bool VerifySingleFile(string file, string expectedNamespace, string expectedClassName)
        {
            bool success = true;
            List<string> classNames = new List<string>();
            using (StreamReader sr = File.OpenText(file))
            {
                string line;

                while ((line = sr.ReadLine()) != null)
                {
                    if (line.StartsWith("namespace ", StringComparison.Ordinal))
                    {
                        string ns = line.Substring(10);
                        if (expectedNamespace != ns)
                        {
                            Console.WriteLine("Invalid namespace: '{0}' Expected: '{1}'", ns, expectedNamespace);
                            success = false;
                        }
                    }

                    Match match = classNameRegex.Match(line);
                    if (match.Success)
                    {
                        classNames.Add(match.Groups["className"].Value);
                    }

                    match = delegateTypeRegex.Match(line);
                    if (match.Success)
                    {
                        classNames.Add(match.Groups["delegateType"].Value);
                    }
                }
            }

            if (classNames.Count == 0)
            {
                Console.WriteLine("No classes found in {0}", file);
                success = false;
            }

            if (classNames.Count > 1)
            {
                Console.WriteLine("More than 1 class name found in {0}", file);
                success = false;
            }

            if (classNames.Count == 1 && classNames[0] != expectedClassName)
            {
                Console.WriteLine("Invalid class name. Expected '{0}', actual: '{1}'", expectedClassName, classNames[0]);
                success = false;
            }

            return success;
        }
    }
}
