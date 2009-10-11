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

using NLog;
using NLog.Config;

using NUnit.Framework;
using System.IO;

namespace NLog.UnitTests
{
    [TestFixture]
	public class SourceCodeTests
	{
        private const string relativeNLogDirectory = "src/NLog";

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

            this.sourceCodeDirectory = Path.Combine(this.sourceCodeDirectory, relativeNLogDirectory);
            this.licenseLines = File.ReadAllLines(this.licenseFile);
        }

        [Test]
        public void VerifyFileHeaders()
        {
            int failedFiles = 0;

            foreach (string file in Directory.GetFiles(this.sourceCodeDirectory, "*.cs", SearchOption.AllDirectories))
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

            Assert.AreEqual(0, failedFiles, "One or more files don't have valid license headers.");
        }

        bool IgnoreFile(string file)
        {
            string baseName = Path.GetFileName(file);
            if (baseName == "AssemblyBuildInfo.cs")
            {
                return true;
            }

            if (baseName == "GlobalSuppressions.cs")
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
    }
}
