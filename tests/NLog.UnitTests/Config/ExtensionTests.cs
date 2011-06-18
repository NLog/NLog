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
#endif
    using MyExtensionNamespace;
    using NLog.Filters;
    using NLog.Layouts;
    using NLog.Targets;

    [TestFixture]
    public class ExtensionTests : NLogTestBase
    {
#if !WINDOWS_PHONE
        private string extensionAssemblyName1 = "SampleExtensions";
#if SILVERLIGHT || NET_CF
        private string extensionAssemblyFullPath1 = "SampleExtensions.dll";
#else
        private string extensionAssemblyFullPath1 = Path.GetFullPath("SampleExtensions.dll");
#endif
        
        [Test]
        public void ExtensionTest1()
        {
            Assert.IsNotNull(typeof(FooLayout));

            var configuration = CreateConfigurationFromString(@"
<nlog throwExceptions='true'>
    <extensions>
        <add assemblyFile='" + this.extensionAssemblyFullPath1 + @"' />
    </extensions>

    <targets>
        <target name='t' type='MyTarget' />
        <target name='d1' type='Debug' layout='${foo}' />
        <target name='d2' type='Debug'>
            <layout type='FooLayout' x='1'>
            </layout>
        </target>
    </targets>

    <rules>
      <logger name='*' writeTo='t'>
        <filters>
           <whenFoo x='44' action='Ignore' />
        </filters>
      </logger>
    </rules>
</nlog>");

            Target myTarget = configuration.FindTargetByName("t");
            Assert.AreEqual("MyExtensionNamespace.MyTarget", myTarget.GetType().FullName);
            
            var d1Target = (DebugTarget)configuration.FindTargetByName("d1");
            var layout = d1Target.Layout as SimpleLayout;
            Assert.IsNotNull(layout);
            Assert.AreEqual(1, layout.Renderers.Count);
            Assert.AreEqual("MyExtensionNamespace.FooLayoutRenderer", layout.Renderers[0].GetType().FullName);

            var d2Target = (DebugTarget)configuration.FindTargetByName("d2");
            Assert.AreEqual("MyExtensionNamespace.FooLayout", d2Target.Layout.GetType().FullName);

            Assert.AreEqual(1, configuration.LoggingRules[0].Filters.Count);
            Assert.AreEqual("MyExtensionNamespace.WhenFooFilter", configuration.LoggingRules[0].Filters[0].GetType().FullName);
        }

        [Test]
        public void ExtensionTest2()
        {
            var configuration = CreateConfigurationFromString(@"
<nlog throwExceptions='true'>
    <extensions>
        <add assembly='" + this.extensionAssemblyName1 + @"' />
    </extensions>

    <targets>
        <target name='t' type='MyTarget' />
        <target name='d1' type='Debug' layout='${foo}' />
        <target name='d2' type='Debug'>
            <layout type='FooLayout' x='1'>
            </layout>
        </target>
    </targets>

    <rules>
      <logger name='*' writeTo='t'>
        <filters>
           <whenFoo x='44' action='Ignore' />
           <when condition='myrandom(10)==3' action='Log' />
        </filters>
      </logger>
    </rules>
</nlog>");

            Target myTarget = configuration.FindTargetByName("t");
            Assert.AreEqual("MyExtensionNamespace.MyTarget", myTarget.GetType().FullName);

            var d1Target = (DebugTarget)configuration.FindTargetByName("d1");
            var layout = d1Target.Layout as SimpleLayout;
            Assert.IsNotNull(layout);
            Assert.AreEqual(1, layout.Renderers.Count);
            Assert.AreEqual("MyExtensionNamespace.FooLayoutRenderer", layout.Renderers[0].GetType().FullName);

            var d2Target = (DebugTarget)configuration.FindTargetByName("d2");
            Assert.AreEqual("MyExtensionNamespace.FooLayout", d2Target.Layout.GetType().FullName);

            Assert.AreEqual(2, configuration.LoggingRules[0].Filters.Count);
            Assert.AreEqual("MyExtensionNamespace.WhenFooFilter", configuration.LoggingRules[0].Filters[0].GetType().FullName);
            var cbf = configuration.LoggingRules[0].Filters[1] as ConditionBasedFilter;
            Assert.IsNotNull(cbf);
            Assert.AreEqual("(myrandom(10) == 3)", cbf.Condition.ToString());
        }

        [Test]
        public void ExtensionWithPrefixTest()
        {
            var configuration = CreateConfigurationFromString(@"
<nlog throwExceptions='true'>
    <extensions>
        <add prefix='myprefix' assemblyFile='" + this.extensionAssemblyFullPath1 + @"' />
    </extensions>

    <targets>
        <target name='t' type='myprefix.MyTarget' />
        <target name='d1' type='Debug' layout='${myprefix.foo}' />
        <target name='d2' type='Debug'>
            <layout type='myprefix.FooLayout' x='1'>
            </layout>
        </target>
    </targets>

    <rules>
      <logger name='*' writeTo='t'>
        <filters>
           <myprefix.whenFoo x='44' action='Ignore' />
        </filters>
      </logger>
    </rules>
</nlog>");

            Target myTarget = configuration.FindTargetByName("t");
            Assert.AreEqual("MyExtensionNamespace.MyTarget", myTarget.GetType().FullName);

            var d1Target = (DebugTarget)configuration.FindTargetByName("d1");
            var layout = d1Target.Layout as SimpleLayout;
            Assert.IsNotNull(layout);
            Assert.AreEqual(1, layout.Renderers.Count);
            Assert.AreEqual("MyExtensionNamespace.FooLayoutRenderer", layout.Renderers[0].GetType().FullName);

            var d2Target = (DebugTarget)configuration.FindTargetByName("d2");
            Assert.AreEqual("MyExtensionNamespace.FooLayout", d2Target.Layout.GetType().FullName);

            Assert.AreEqual(1, configuration.LoggingRules[0].Filters.Count);
            Assert.AreEqual("MyExtensionNamespace.WhenFooFilter", configuration.LoggingRules[0].Filters[0].GetType().FullName);
        }
#endif

        [Test]
        public void ExtensionTest4()
        {
            Assert.IsNotNull(typeof(FooLayout));

            var configuration = CreateConfigurationFromString(@"
<nlog throwExceptions='true'>
    <extensions>
        <add type='" + typeof(MyTarget).AssemblyQualifiedName + @"' />
        <add type='" + typeof(FooLayout).AssemblyQualifiedName + @"' />
        <add type='" + typeof(FooLayoutRenderer).AssemblyQualifiedName + @"' />
        <add type='" + typeof(WhenFooFilter).AssemblyQualifiedName + @"' />
    </extensions>

    <targets>
        <target name='t' type='MyTarget' />
        <target name='d1' type='Debug' layout='${foo}' />
        <target name='d2' type='Debug'>
            <layout type='FooLayout' x='1'>
            </layout>
        </target>
    </targets>

    <rules>
      <logger name='*' writeTo='t'>
        <filters>
           <whenFoo x='44' action='Ignore' />
        </filters>
      </logger>
    </rules>
</nlog>");

            Target myTarget = configuration.FindTargetByName("t");
            Assert.AreEqual("MyExtensionNamespace.MyTarget", myTarget.GetType().FullName);

            var d1Target = (DebugTarget)configuration.FindTargetByName("d1");
            var layout = d1Target.Layout as SimpleLayout;
            Assert.IsNotNull(layout);
            Assert.AreEqual(1, layout.Renderers.Count);
            Assert.AreEqual("MyExtensionNamespace.FooLayoutRenderer", layout.Renderers[0].GetType().FullName);

            var d2Target = (DebugTarget)configuration.FindTargetByName("d2");
            Assert.AreEqual("MyExtensionNamespace.FooLayout", d2Target.Layout.GetType().FullName);

            Assert.AreEqual(1, configuration.LoggingRules[0].Filters.Count);
            Assert.AreEqual("MyExtensionNamespace.WhenFooFilter", configuration.LoggingRules[0].Filters[0].GetType().FullName);
        }

    }
}
