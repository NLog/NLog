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

#if !SILVERLIGHT

namespace NLog.UnitTests.LayoutRenderers
{
    using System.Security.Principal;
    using System.Threading;
#if(__IOS__)
		using NUnit.Framework;
#else
    using Xunit;
#endif

    public class IdentityTests : NLogTestBase
    {
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

        [Fact]
        public void IdentityTest2()
        {
            var oldPrincipal = Thread.CurrentPrincipal;

            Thread.CurrentPrincipal = new GenericPrincipal(new NotAuthenticatedIdentity(), new []{"role1"});
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
            public override bool IsAuthenticated
            {
                get { return false; }
            }

            #endregion
        }
    }
}

#endif