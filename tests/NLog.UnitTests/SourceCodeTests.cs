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

#if !SILVERLIGHT && !NET_CF

using System;
using System.Collections.Generic;
using System.IO;
#if !NET2_0
using System.Linq;
#endif
using System.Text.RegularExpressions;
#if !NET2_0
using System.Xml.Linq;
#endif
using NUnit.Framework;

#if !NUNIT
    using SetUp = Microsoft.VisualStudio.TestTools.UnitTesting.TestInitializeAttribute;
    using TestFixture = Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute;
    using Test = Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute;
    using TearDown =  Microsoft.VisualStudio.TestTools.UnitTesting.TestCleanupAttribute;
#endif

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

        private static IList<string> fileNamesToIgnore = new List<string>()
        {
            "AssemblyInfo.cs",
            "AssemblyBuildInfo.cs",
            "GlobalSuppressions.cs",
            "CompilerAttributes.cs",
        };

        private string sourceCodeDirectory;
        private string licenseFile;
        private string[] licenseLines;

        [SetUp]
        public void Initialize()
        {
            this.sourceCodeDirectory = Directory.GetCurrentDirectory();
            while (this.sourceCodeDirectory != null)
            {
                this.licenseFile = Path.Combine(sourceCodeDirectory, "LICENSE.txt");
                if (File.Exists(licenseFile))
                {
                    break;
                }

                this.sourceCodeDirectory = Path.GetDirectoryName(this.sourceCodeDirectory);
            }

            if (this.sourceCodeDirectory != null)
            {
                this.licenseLines = File.ReadAllLines(this.licenseFile);
            }
        }

        [Test]
        public void VerifyFileHeaders()
        {
            if (this.sourceCodeDirectory == null)
            {
                return;
            }

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

#if !NET2_0
        private static XNamespace MSBuildNamespace = "http://schemas.microsoft.com/developer/msbuild/2003";

        [Test]
        public void VerifyProjectsInSync()
        {
            if (this.sourceCodeDirectory == null)
            {
                return;
            }

            int failures = 0;
            var filesToCompile = new List<string>();

            GetAllFilesToCompileInDirectory(filesToCompile, Path.Combine(this.sourceCodeDirectory, "src/NLog/"), "*.cs", "");

            failures += CompareDirectoryWithProjects(filesToCompile, "src/NLog/NLog.netfx35.csproj");
            failures += CompareDirectoryWithProjects(filesToCompile, "src/NLog/NLog.netfx40.csproj");
            failures += CompareDirectoryWithProjects(filesToCompile, "src/NLog/NLog.netfx20.csproj");
            failures += CompareDirectoryWithProjects(filesToCompile, "src/NLog/NLog.netcf20.csproj");
            failures += CompareDirectoryWithProjects(filesToCompile, "src/NLog/NLog.netcf35.csproj");
            failures += CompareDirectoryWithProjects(filesToCompile, "src/NLog/NLog.sl2.csproj");
            failures += CompareDirectoryWithProjects(filesToCompile, "src/NLog/NLog.sl3.csproj");
            failures += CompareDirectoryWithProjects(filesToCompile, "src/NLog/NLog.sl4.csproj");
            failures += CompareDirectoryWithProjects(filesToCompile, "src/NLog/NLog.wp7.csproj");
            failures += CompareDirectoryWithProjects(filesToCompile, "src/NLog/NLog.monodevelop.csproj");

            filesToCompile.Clear();
            GetAllFilesToCompileInDirectory(filesToCompile, Path.Combine(this.sourceCodeDirectory, "src/NLog.Extended/"), "*.cs", "");

            failures += CompareDirectoryWithProjects(filesToCompile, "src/NLog.Extended/NLog.Extended.netfx35.csproj");
            failures += CompareDirectoryWithProjects(filesToCompile, "src/NLog.Extended/NLog.Extended.netfx40.csproj");
            failures += CompareDirectoryWithProjects(filesToCompile, "src/NLog.Extended/NLog.Extended.netfx20.csproj");
            failures += CompareDirectoryWithProjects(filesToCompile, "src/NLog.Extended/NLog.Extended.mono2.csproj");
            failures += CompareDirectoryWithProjects(filesToCompile, "src/NLog.Extended/NLog.Extended.monodevelop.csproj");

            filesToCompile.Clear();
            GetAllFilesToCompileInDirectory(filesToCompile, Path.Combine(this.sourceCodeDirectory, "tests/NLog.UnitTests/"), "*.cs", "");

            failures += CompareDirectoryWithProjects(filesToCompile, "tests/NLog.UnitTests/NLog.UnitTests.netfx35.csproj");
            failures += CompareDirectoryWithProjects(filesToCompile, "tests/NLog.UnitTests/NLog.UnitTests.netfx40.csproj");
            failures += CompareDirectoryWithProjects(filesToCompile, "tests/NLog.UnitTests/NLog.UnitTests.netfx20.csproj");
            failures += CompareDirectoryWithProjects(filesToCompile, "tests/NLog.UnitTests/NLog.UnitTests.netcf20.csproj");
            failures += CompareDirectoryWithProjects(filesToCompile, "tests/NLog.UnitTests/NLog.UnitTests.netcf35.csproj");
            failures += CompareDirectoryWithProjects(filesToCompile, "tests/NLog.UnitTests/NLog.UnitTests.sl2.csproj");
            failures += CompareDirectoryWithProjects(filesToCompile, "tests/NLog.UnitTests/NLog.UnitTests.sl3.csproj");
            failures += CompareDirectoryWithProjects(filesToCompile, "tests/NLog.UnitTests/NLog.UnitTests.sl4.csproj");
            //failures += CompareDirectoryWithProjects(filesToCompile, "tests/NLog.UnitTests/NLog.UnitTests.mono2.csproj");
            failures += CompareDirectoryWithProjects(filesToCompile, "tests/NLog.UnitTests/NLog.UnitTests.monodevelop.csproj");

            Assert.AreEqual(0, failures, "Failures found.");
        }

        private int CompareDirectoryWithProjects(List<string> filesToCompile, params string[] projectFiles)
        {
            var filesInProject = new List<string>();
            this.GetCompileItemsFromProjects(filesInProject, projectFiles);

            var missingFiles = filesToCompile.Except(filesInProject).ToList();
            if (missingFiles.Count > 0)
            {
                Console.WriteLine("The following files must be added to {0}", string.Join(";", projectFiles));
                foreach (var f in missingFiles)
                {
                    Console.WriteLine("  {0}", f);
                }
            }

            return missingFiles.Count;
        }

        private void GetCompileItemsFromProjects(List<string> filesInProject, params string[] projectFiles)
        {
            foreach (string proj in projectFiles)
            {
                string csproj = Path.Combine(this.sourceCodeDirectory, proj);
                GetCompileItemsFromProject(filesInProject, csproj);
            }
        }

        private static void GetCompileItemsFromProject(List<string> filesInProject, string csproj)
        {
            XElement contents = XElement.Load(csproj);
            filesInProject.AddRange(contents.Descendants(MSBuildNamespace + "Compile").Select(c => (string)c.Attribute("Include")));
        }

        private static void GetAllFilesToCompileInDirectory(List<string> output, string path, string pattern, string prefix)
        {
            foreach (string file in Directory.GetFiles(path, pattern))
            {
                if (file.EndsWith(".xaml.cs"))
                {
                    continue;
                }

                if (file.Contains(".g."))
                {
                    continue;
                }

                output.Add(prefix + Path.GetFileName(file));
            }

            foreach (string dir in Directory.GetDirectories(path))
            {
                GetAllFilesToCompileInDirectory(output, dir, pattern, prefix + Path.GetFileName(dir) + "\\");
            }
        }
#endif

        [Test]
        public void VerifyNamespacesAndClassNames()
        {
            if (this.sourceCodeDirectory == null)
            {
                return;
            }

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

            if (baseName.IndexOf(".xaml", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }

            if (baseName.IndexOf(".g.", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }

            if (baseName == "ExtensionAttribute.cs")
            {
                return true;
            }

            if (baseName == "NUnitAdapter.cs")
            {
                return true;
            }

            if (baseName == "LocalizableAttribute.cs")
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

#endif