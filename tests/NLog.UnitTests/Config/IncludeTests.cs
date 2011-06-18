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

namespace NLog.UnitTests.Config
{
    using System;
    using System.IO;
    using NUnit.Framework;

#if !NUNIT
    using SetUp = Microsoft.VisualStudio.TestTools.UnitTesting.TestInitializeAttribute;
    using TestFixture = Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute;
    using Test = Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute;
    using TearDown =  Microsoft.VisualStudio.TestTools.UnitTesting.TestCleanupAttribute;
    using ExpectedException = Microsoft.VisualStudio.TestTools.UnitTesting.ExpectedExceptionAttribute;
#endif
    using NLog.Config;

    [TestFixture]
    public class IncludeTests : NLogTestBase
    {
        [Test]
        public void IncludeTest()
        {
#if SILVERLIGHT
            // file is pre-packaged in the XAP
            string fileToLoad = "ConfigFiles/main.nlog";
#else
            string tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempPath);

            using (StreamWriter fs = File.CreateText(Path.Combine(tempPath, "included.nlog")))
            {
                fs.Write(@"<nlog>
                    <targets><target name='debug' type='Debug' layout='${message}' /></targets>
            </nlog>");
            }

            using (StreamWriter fs = File.CreateText(Path.Combine(tempPath, "main.nlog")))
            {
                fs.Write(@"<nlog>
                <include file='included.nlog' />
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>");
            }

            string fileToLoad = Path.Combine(tempPath, "main.nlog");
#endif
            try
            {
                // load main.nlog from the XAP
                LogManager.Configuration = new XmlLoggingConfiguration(fileToLoad);

                LogManager.GetLogger("A").Debug("aaa");
                AssertDebugLastMessage("debug", "aaa");
            }
            finally
            {
                LogManager.Configuration = null;
#if !SILVERLIGHT
                if (Directory.Exists(tempPath))
                    Directory.Delete(tempPath, true);
#endif
            }
        }

        [Test]
        [ExpectedException(typeof(NLogConfigurationException))]
        public void IncludeNotExistingTest()
        {
#if SILVERLIGHT
            string fileToLoad = "ConfigFiles/referencemissingfile.nlog";
#else
            string tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempPath);

            using (StreamWriter fs = File.CreateText(Path.Combine(tempPath, "main.nlog")))
            {
                fs.Write(@"<nlog>
                <include file='included.nlog' />
            </nlog>");
            }

            string fileToLoad = Path.Combine(tempPath, "main.nlog");

#endif
            try
            {
                new XmlLoggingConfiguration(fileToLoad);
            }
            finally
            {
#if !SILVERLIGHT
                if (Directory.Exists(tempPath))
                    Directory.Delete(tempPath, true);
#endif
            }
        }

        [Test]
        public void IncludeNotExistingIgnoredTest()
        {
#if SILVERLIGHT
            string fileToLoad = "ConfigFiles/referencemissingfileignored.nlog";
#else
            string tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempPath);

            using (StreamWriter fs = File.CreateText(Path.Combine(tempPath, "main.nlog")))
            {
                fs.Write(@"<nlog>
                <include file='included-notpresent.nlog' ignoreErrors='true' />
                <targets><target name='debug' type='Debug' layout='${message}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>");
            }

            string fileToLoad = Path.Combine(tempPath, "main.nlog");
#endif
            try
            {
                LogManager.Configuration = new XmlLoggingConfiguration(fileToLoad);
                LogManager.GetLogger("A").Debug("aaa");
                AssertDebugLastMessage("debug", "aaa");
            }
            finally
            {
                LogManager.Configuration = null;
#if !SILVERLIGHT
                if (Directory.Exists(tempPath))
                    Directory.Delete(tempPath, true);
#endif
            }
        }
    }
}