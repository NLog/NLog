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

namespace NLog.UnitTests.LayoutRenderers
{
    using System;
    using System.Security.Principal;
    using System.Threading;
    using NLog.Common;
    using NLog.Config;
    using NLog.Targets.Wrappers;
    using NLog.UnitTests.Common;
    using Xunit;

    public class IdentityTests : NLogTestBase
    {
#if MONO
        [Fact(Skip = "MONO on Travis not supporting WindowsIdentity")]
#else
        [Fact]
#endif
        public void WindowsIdentityTest()
        {
#if NETSTANDARD
            if (IsTravis())
            {
                Console.WriteLine("[SKIP] IdentityTests.WindowsIdentityTest NetStandard on Travis not supporting WindowsIdentity");
                return; // NetCore on Travis not supporting WindowsIdentity
            }
#endif

            var userDomainName = Environment.GetEnvironmentVariable("USERDOMAIN") ?? string.Empty;
            var userName = Environment.GetEnvironmentVariable("USERNAME") ?? string.Empty;
            if (!string.IsNullOrEmpty(userDomainName))
                userName = userDomainName + "\\" + userName;

            NLog.Layouts.Layout layout = "${windows-identity}";
            var result = layout.Render(LogEventInfo.CreateNullEvent());
            if (!string.IsNullOrEmpty(result) || !IsAppVeyor())
                Assert.Equal(userName, result);
        }

        [Fact]
        public void IdentityTest1()
        {
            var oldPrincipal = Thread.CurrentPrincipal;

            Thread.CurrentPrincipal = new GenericPrincipal(new GenericIdentity("SOMEDOMAIN\\SomeUser", "CustomAuth"), new[] { "Role1", "Role2" });
            try
            {
                AssertLayoutRendererOutput("${identity}", "auth:CustomAuth:SOMEDOMAIN\\SomeUser");
                AssertLayoutRendererOutput("${identity:authtype=false}", "auth:SOMEDOMAIN\\SomeUser");
                AssertLayoutRendererOutput("${identity:authtype=false:isauthenticated=false}", "SOMEDOMAIN\\SomeUser");
                AssertLayoutRendererOutput("${identity:fsnormalize=true}", "auth_CustomAuth_SOMEDOMAIN_SomeUser");
            }
            finally
            {
                Thread.CurrentPrincipal = oldPrincipal;
            }
        }

        /// <summary>
        /// Test writing ${identity} async
        /// </summary>
        [Fact]
        public void IdentityTest1Async()
        {
            var oldPrincipal = Thread.CurrentPrincipal;


            try
            {

                ConfigurationItemFactory.Default.Targets
                            .RegisterDefinition("CSharpEventTarget", typeof(CSharpEventTarget));


                LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"<?xml version='1.0' encoding='utf-8' ?>
<nlog xmlns='http://www.nlog-project.org/schemas/NLog.xsd'
      xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'
 
      internalLogLevel='Debug'
      throwExceptions='true' >

  <targets async='true'>
    <target name='target1' xsi:type='CSharpEventTarget' layout='${identity}' />
  </targets>

  <rules>
    <logger name='*' writeTo='target1' />
  </rules>
</nlog>
");

                try
                {
                    var continuationHit = new ManualResetEvent(false);
                    string rendered = null;
                    var threadId = Thread.CurrentThread.ManagedThreadId;
                    var asyncThreadId = threadId;
                    LogEventInfo lastLogEvent = null;

                    var asyncTarget = LogManager.Configuration.FindTargetByName<AsyncTargetWrapper>("target1");
                    Assert.NotNull(asyncTarget);
                    var target = asyncTarget.WrappedTarget as CSharpEventTarget;
                    Assert.NotNull(target);
                    target.BeforeWrite += (logevent, rendered1, asyncThreadId1) =>
                    {
                        //clear in current thread before write
                        Thread.CurrentPrincipal = new GenericPrincipal(new GenericIdentity("ANOTHER user", "type"), null);
                    };

                    target.EventWritten += (logevent, rendered1, asyncThreadId1) =>
                    {
                        rendered = rendered1;
                        asyncThreadId = asyncThreadId1;
                        lastLogEvent = logevent;
                        continuationHit.Set();
                    };


                    Thread.CurrentPrincipal = new GenericPrincipal(new GenericIdentity("SOMEDOMAIN\\SomeUser", "CustomAuth"), new[] { "Role1", "Role2" });

                    var logger = LogManager.GetCurrentClassLogger();
                    logger.Debug("test write");


                    Assert.True(continuationHit.WaitOne());
                    Assert.NotNull(lastLogEvent);
                    //should be written in another thread.
                    Assert.NotEqual(threadId, asyncThreadId);


                    Assert.Equal("auth:CustomAuth:SOMEDOMAIN\\SomeUser", rendered);



                }
                finally
                {
                    LogManager.Configuration.Close();
                }
            }
            finally
            {
                InternalLogger.Reset();
                Thread.CurrentPrincipal = oldPrincipal;
            }
        }

        [Fact]
        public void IdentityTest2()
        {
            var oldPrincipal = Thread.CurrentPrincipal;

            Thread.CurrentPrincipal = new GenericPrincipal(new NotAuthenticatedIdentity(), new[] { "role1" });
            try
            {
                AssertLayoutRendererOutput("${identity}", "notauth::");
            }
            finally
            {
                Thread.CurrentPrincipal = oldPrincipal;
            }
        }

        /// <summary>
        /// Mock object for IsAuthenticated property.
        /// </summary>
        private class NotAuthenticatedIdentity : GenericIdentity
        {

            public NotAuthenticatedIdentity()
                : base("")
            {
            }

#region Overrides of GenericIdentity

            /// <summary>
            /// Gets a value indicating whether the user has been authenticated.
            /// </summary>
            /// <returns>
            /// true if the user was has been authenticated; otherwise, false.
            /// </returns>
            public override bool IsAuthenticated => false;

#endregion
        }
    }
}
