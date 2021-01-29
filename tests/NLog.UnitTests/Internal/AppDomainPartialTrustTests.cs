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

#if !NETSTANDARD && !MONO

using System;
using System.IO;
using System.Security;
using System.Security.Permissions;
using System.Text;
using System.Threading;
using NLog.Config;
using Xunit;

namespace NLog.UnitTests.Internal
{
    public class AppDomainPartialTrustTests : NLogTestBase
    {
        [Fact]
        public void MediumTrustWithExternalClass()
        {
            var fileWritePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            try
            {
                int times = 25;

                RunAppDomainTestMethod(fileWritePath, times, true);

                // this also checks that thread-volatile layouts
                // such as ${threadid} are properly cached and not recalculated
                // in logging threads.

                var threadID = Thread.CurrentThread.ManagedThreadId.ToString();

                Assert.False(File.Exists(Path.Combine(fileWritePath, "Trace.txt")));

                AssertFileContents(Path.Combine(fileWritePath, "Debug.txt"),
                    StringRepeat(times, "aaa " + threadID + "\n"), Encoding.UTF8);

                AssertFileContents(Path.Combine(fileWritePath, "Info.txt"),
                    StringRepeat(times, "bbb " + threadID + "\n"), Encoding.UTF8);

                AssertFileContents(Path.Combine(fileWritePath, "Warn.txt"),
                    StringRepeat(times, "ccc " + threadID + "\n"), Encoding.UTF8);

                AssertFileContents(Path.Combine(fileWritePath, "Error.txt"),
                    StringRepeat(times, "ddd " + threadID + "\n"), Encoding.UTF8);

                AssertFileContents(Path.Combine(fileWritePath, "Fatal.txt"),
                    StringRepeat(times, "eee " + threadID + "\n"), Encoding.UTF8);

            }
            finally
            {
                if (Directory.Exists(fileWritePath))
                    Directory.Delete(fileWritePath, true);
            }
        }

        [Fact]
        public void MediumTrustWithExternalClassNoAutoFlush()
        {
            var fileWritePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            try
            {
                int times = 5;
                RunAppDomainTestMethod(fileWritePath, times, false);

                Assert.False(File.Exists(Path.Combine(fileWritePath, "Trace.txt")));
                Assert.False(File.Exists(Path.Combine(fileWritePath, "Debug.txt")));
                Assert.False(File.Exists(Path.Combine(fileWritePath, "Warn.txt")));
                Assert.False(File.Exists(Path.Combine(fileWritePath, "Error.txt")));
                Assert.False(File.Exists(Path.Combine(fileWritePath, "Fatal.txt")));
            }
            finally
            {
                if (Directory.Exists(fileWritePath))
                    Directory.Delete(fileWritePath, true);
            }
        }

        private static void RunAppDomainTestMethod(string fileWritePath, int times, bool autoShutdown)
        {
            // ClassUnderTest must extend MarshalByRefObject
            AppDomain partialTrusted = MediumTrustContext.CreatePartialTrustDomain(fileWritePath);
            var classUnderTest = (ClassUnderTest)partialTrusted.CreateInstanceAndUnwrap(typeof(ClassUnderTest).Assembly.FullName, typeof(ClassUnderTest).FullName);

            using (ScopeContext.PushProperty("Winner", new { Hero = "Zero" }))
            using (ScopeContext.PushNestedState(new { Hello = "World" }))
            {
                partialTrusted.DoCallBack(HelloWorld);
                classUnderTest.PartialTrustSuccess(times, fileWritePath, autoShutdown);
            }
            AppDomain.Unload(partialTrusted);
        }

#pragma warning disable xUnit1013 // Public method should be marked as test
        public static void HelloWorld()
#pragma warning restore xUnit1013 // Public method should be marked as test
        {
            Console.WriteLine("Hello World");
        }
    }

    [Serializable]
    public class ClassUnderTest : MarshalByRefObject
    {
        public void PartialTrustSuccess(int times, string fileWritePath, bool autoShutdown)
        {
            var filePath = Path.Combine(fileWritePath, "${level}.txt");

            // NOTE Using BufferingWrapper to validate that DomainUnload remembers to perform flush
            var configXml = $@"
            <nlog throwExceptions='false' autoShutdown='{autoShutdown}'>
                <targets async='true'> 
                    <target name='file' type='BufferingWrapper' bufferSize='10000'>
                        <target name='filewrapped' type='file' layout='${{message}} ${{threadid}}' filename='{
                    filePath
                }' LineEnding='lf' />
                    </target>
                </targets>
                <rules>
                    <logger name='*' minlevel='Debug' appendto='file'>
                    </logger>
                </rules>
            </nlog>";

            using (new NLogTestBase.NoThrowNLogExceptions())
            {
                LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(configXml);

                //this method gave issues
                LogFactory.LogConfigurationInitialized();

                ILogger logger = LogManager.GetLogger("NLog.UnitTests.Targets.FileTargetTests");

                for (var i = 0; i < times; ++i)
                {
                    logger.Trace("@@@");
                    logger.Debug("aaa");
                    logger.Info("bbb");
                    logger.Warn("ccc");
                    logger.Error("{d:l}", "ddd");
                    logger.Fatal("eee");
                }
            }
        }
    }

    internal static class MediumTrustContext
    {
        public static AppDomain CreatePartialTrustDomain(string fileWritePath)
        {
            var setup = new AppDomainSetup { ApplicationBase = AppDomain.CurrentDomain.BaseDirectory };
            var permissions = new PermissionSet(null);
            permissions.AddPermission(new SecurityPermission(SecurityPermissionFlag.Execution));
            permissions.AddPermission(new ReflectionPermission(ReflectionPermissionFlag.RestrictedMemberAccess));
            permissions.AddPermission(new FileIOPermission(FileIOPermissionAccess.AllAccess, fileWritePath));
            return AppDomain.CreateDomain("Partial Trust AppDomain: " + DateTime.Now.Ticks, null, setup, permissions);
        }
    }
}

#endif