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
    using NUnit.Framework;

#if !NUNIT
    using SetUp = Microsoft.VisualStudio.TestTools.UnitTesting.TestInitializeAttribute;
    using TestFixture = Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute;
    using Test = Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute;
    using TearDown =  Microsoft.VisualStudio.TestTools.UnitTesting.TestCleanupAttribute;
#endif
    using NLog.LayoutRenderers;
    using NLog.Layouts;
    using NLog.Targets;

    [TestFixture]
    public class VariableTests : NLogTestBase
    {
        [Test]
        public void VariablesTest1()
        {
            var configuration = CreateConfigurationFromString(@"
<nlog throwExceptions='true'>
    <variable name='prefix' value='[[' />
    <variable name='suffix' value=']]' />

    <targets>
        <target name='d1' type='Debug' layout='${prefix}${message}${suffix}' />
    </targets>
</nlog>");

            var d1 = configuration.FindTargetByName("d1") as DebugTarget;
            Assert.IsNotNull(d1);
            var layout = d1.Layout as SimpleLayout;
            Assert.IsNotNull(layout);
            Assert.AreEqual(3, layout.Renderers.Count);
            var lr1 = layout.Renderers[0] as LiteralLayoutRenderer;
            var lr2 = layout.Renderers[1] as MessageLayoutRenderer;
            var lr3 = layout.Renderers[2] as LiteralLayoutRenderer;
            Assert.IsNotNull(lr1);
            Assert.IsNotNull(lr2);
            Assert.IsNotNull(lr3);
            Assert.AreEqual("[[", lr1.Text);
            Assert.AreEqual("]]", lr3.Text);
        }
    }
}