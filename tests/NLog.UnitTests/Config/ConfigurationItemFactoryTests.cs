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
    using NUnit.Framework;

#if !NUNIT
    using SetUp = Microsoft.VisualStudio.TestTools.UnitTesting.TestInitializeAttribute;
    using TestFixture = Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute;
    using Test = Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute;
    using TearDown =  Microsoft.VisualStudio.TestTools.UnitTesting.TestCleanupAttribute;
#endif
    using NLog.Config;
    using NLog.Targets;

    [TestFixture]
    public class ConfigurationItemFactoryTests : NLogTestBase
    {
#if !SILVERLIGHT && !NET_CF
        // this is just to force reference to NLog.Extended.dll
        public Type ForceExtendedReference = typeof(MessageQueueTarget).DeclaringType;

        [Test]
        public void ExtendedTargetTest()
        {
            var targets = ConfigurationItemFactory.Default.Targets;

            AssertInstance(targets, "MSMQ", "MessageQueueTarget");
            AssertInstance(targets, "AspNetTrace", "AspNetTraceTarget");
            AssertInstance(targets, "AspNetBufferingWrapper", "AspNetBufferingTargetWrapper");
        }

        [Test]
        public void ExtendedLayoutRendererTest()
        {
            var layoutRenderers = ConfigurationItemFactory.Default.LayoutRenderers;

            AssertInstance(layoutRenderers, "aspnet-application", "AspNetApplicationValueLayoutRenderer");
            AssertInstance(layoutRenderers, "aspnet-request", "AspNetRequestValueLayoutRenderer");
            AssertInstance(layoutRenderers, "aspnet-sessionid", "AspNetSessionIDLayoutRenderer");
            AssertInstance(layoutRenderers, "aspnet-session", "AspNetSessionValueLayoutRenderer");
            AssertInstance(layoutRenderers, "aspnet-user-authtype", "AspNetUserAuthTypeLayoutRenderer");
            AssertInstance(layoutRenderers, "aspnet-user-identity", "AspNetUserIdentityLayoutRenderer");
        }

        private static void AssertInstance<T1, T2>(INamedItemFactory<T1, T2> targets, string itemName, string expectedTypeName)
            where T1 : class
        {
            Assert.AreEqual(expectedTypeName, targets.CreateInstance(itemName).GetType().Name);
        }
#endif
    }
}