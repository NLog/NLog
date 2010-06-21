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

namespace NLog.UnitTests.Config
{
    using System;
    using System.IO;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using NLog.Config;

    [TestClass]
    public class IncludeTests : NLogTestBase
    {
#if !SILVERLIGHT
        [TestMethod]
        public void IncludeTest()
        {
            string tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempPath);
            try
            {
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

                LogManager.Configuration = new XmlLoggingConfiguration(Path.Combine(tempPath, "main.nlog"));
                LogManager.GetLogger("A").Debug("aaa");
                AssertDebugLastMessage("debug", "aaa");
            }
            finally
            {
                LogManager.Configuration = null;
                if (Directory.Exists(tempPath))
                    Directory.Delete(tempPath, true);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(NLogConfigurationException))]
        public void IncludeNotExistingTest()
        {
            string tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempPath);

            try
            {
                using (StreamWriter fs = File.CreateText(Path.Combine(tempPath, "main.nlog")))
                {
                    fs.Write(@"<nlog>
                    <include file='included.nlog' />
                </nlog>");
                }

                LogManager.Configuration = new XmlLoggingConfiguration(Path.Combine(tempPath, "main.nlog"));
            }
            finally
            {
                LogManager.Configuration = null;
                if (Directory.Exists(tempPath))
                    Directory.Delete(tempPath, true);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(NLogConfigurationException))]
        public void IncludeNotExistingIgnoredTest()
        {
            string tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempPath);

            try
            {
                using (StreamWriter fs = File.CreateText(Path.Combine(tempPath, "main.nlog")))
                {
                    fs.Write(@"<nlog>
                    <include file='included-notpresent.nlog' />
                    <targets><target name='debug' type='Debug' layout='${message}' /></targets>
                    <rules>
                        <logger name='*' minlevel='Debug' writeTo='debug' />
                    </rules>
                </nlog>");
                }

                LogManager.Configuration = new XmlLoggingConfiguration(Path.Combine(tempPath, "main.nlog"));
                LogManager.GetLogger("A").Debug("aaa");
                AssertDebugLastMessage("debug", "aaa");
            }
            finally
            {
                LogManager.Configuration = null;
                if (Directory.Exists(tempPath))
                    Directory.Delete(tempPath, true);
            }
        }
#endif
    }
}