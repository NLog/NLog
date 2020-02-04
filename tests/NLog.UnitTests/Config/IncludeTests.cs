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

using System.Diagnostics;
using System.Linq;
using System.Threading;
using NLog.Targets;

namespace NLog.UnitTests.Config
{
    using System;
    using System.IO;
    using NLog.Config;
    using Xunit;

    public class IncludeTests : NLogTestBase
    {
        [Fact]
        public void IncludeTest()
        {
            LogManager.ThrowExceptions = true;
            var includeAttrValue = @"included.nlog";
            IncludeTest_inner(includeAttrValue, GetTempDir());
        }

        [Fact]
        public void IncludeWildcardTest_relative()
        {
            var includeAttrValue = @"*.nlog";
            IncludeTest_inner(includeAttrValue, GetTempDir());
        }

        [Fact]
        public void IncludeWildcardTest_absolute()
        {
            var includeAttrValue = @"*.nlog";
            var tempPath = GetTempDir();
            includeAttrValue = Path.Combine(tempPath, includeAttrValue);
            IncludeTest_inner(includeAttrValue, tempPath);
        }

        private void IncludeTest_inner(string includeAttrValue, string tempDir)
        {
            Directory.CreateDirectory(tempDir);

            CreateConfigFile(tempDir, "included.nlog", @"<nlog>
                    <targets><target name='debug' type='Debug' layout='${message}' /></targets>
            </nlog>");


            CreateConfigFile(tempDir, "main.nlog", $@"<nlog>
                <include file='{includeAttrValue}' />
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>");


            string fileToLoad = Path.Combine(tempDir, "main.nlog");
            try
            {
                // load main.nlog from the XAP
                LogManager.Configuration = new XmlLoggingConfiguration(fileToLoad);

                LogManager.GetLogger("A").Debug("aaa");
                AssertDebugLastMessage("debug", "aaa");
            }
            finally
            {
                if (Directory.Exists(tempDir))
                    Directory.Delete(tempDir, true);
            }
        }



        [Fact]
        public void IncludeNotExistingTest()
        {
            LogManager.ThrowConfigExceptions = true;
            string tempPath = GetTempDir();
            Directory.CreateDirectory(tempPath);

            using (StreamWriter fs = File.CreateText(Path.Combine(tempPath, "main.nlog")))
            {
                fs.Write(@"<nlog>
                <include file='included.nlog' />
            </nlog>");
            }

            string fileToLoad = Path.Combine(tempPath, "main.nlog");

            try
            {
                Assert.Throws<NLogConfigurationException>(() => new XmlLoggingConfiguration(fileToLoad));
            }
            finally
            {
                if (Directory.Exists(tempPath))
                    Directory.Delete(tempPath, true);
            }
        }

        [Fact]
        public void IncludeNotExistingIgnoredTest()
        {
            var tempPath = GetTempDir();
            Directory.CreateDirectory(tempPath);

            var config = @"<nlog>
                <include file='included-notpresent.nlog' ignoreErrors='true' />
                <targets><target name='debug' type='Debug' layout='${message}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>";

            CreateConfigFile(tempPath, "main.nlog", config);

            string fileToLoad = Path.Combine(tempPath, "main.nlog");
            try
            {
                LogManager.Configuration = new XmlLoggingConfiguration(fileToLoad);
                LogManager.GetLogger("A").Debug("aaa");
                AssertDebugLastMessage("debug", "aaa");
            }
            finally
            {
                if (Directory.Exists(tempPath))
                    Directory.Delete(tempPath, true);
            }
        }

        [Fact]
        public void IncludeNotExistingIgnoredTest_DoesNotThrow()
        {
            LogManager.ThrowExceptions = true;
            var tempPath = GetTempDir();
            Directory.CreateDirectory(tempPath);

            var config = @"<nlog>
                <include file='included-notpresent.nlog' ignoreErrors='true' />
                <targets><target name='debug' type='Debug' layout='${message}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>";

            CreateConfigFile(tempPath, "main.nlog", config);
            string fileToLoad = Path.Combine(tempPath, "main.nlog");

            var ex = Record.Exception(() => new XmlLoggingConfiguration(fileToLoad));
            Assert.Null(ex);
        }

        /// <summary>
        /// Create config file in dir
        /// </summary>
        /// <param name="tempPath"></param>
        /// <param name="filename"></param>
        /// <param name="config"></param>
        private static void CreateConfigFile(string tempPath, string filename, string config)
        {
            using (var fs = File.CreateText(Path.Combine(tempPath, filename)))
            {
                fs.Write(config);
            }
        }
        private static string GetTempDir()
        {
            return Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        }
    }
}