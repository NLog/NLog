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

using System;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using NLog.Common;
using NLog.Config;
using Xunit.Extensions;

namespace NLog.UnitTests.Config
{
    using System.IO;
    using MyExtensionNamespace;
    using NLog.Filters;
    using NLog.Layouts;
    using NLog.Targets;
    using Xunit;

    public class ExtensionTests : NLogTestBase
    {
        private string extensionAssemblyName1 = "SampleExtensions";
        private string extensionAssemblyFullPath1 = Path.GetFullPath("SampleExtensions.dll");

        private string GetExtensionAssemblyFullPath()
        {
#if NETSTANDARD
            Assert.NotNull(typeof(FooLayout));
            return typeof(FooLayout).GetTypeInfo().Assembly.Location;
#else
            return extensionAssemblyFullPath1;
#endif
        }

        [Fact]
        public void ExtensionTest1()
        {
            Assert.NotNull(typeof(FooLayout));

            var configuration = new LogFactory().Setup().LoadConfigurationFromXml(@"
<nlog throwExceptions='true'>
    <extensions>
        <add assemblyFile='" + GetExtensionAssemblyFullPath() + @"' />
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
</nlog>").LogFactory.Configuration;

            Target myTarget = configuration.FindTargetByName("t");
            Assert.Equal("MyExtensionNamespace.MyTarget", myTarget.GetType().FullName);

            var d1Target = (DebugTarget)configuration.FindTargetByName("d1");
            var layout = d1Target.Layout as SimpleLayout;
            Assert.NotNull(layout);
            Assert.Single(layout.Renderers);
            Assert.Equal("MyExtensionNamespace.FooLayoutRenderer", layout.Renderers[0].GetType().FullName);

            var d2Target = (DebugTarget)configuration.FindTargetByName("d2");
            Assert.Equal("MyExtensionNamespace.FooLayout", d2Target.Layout.GetType().FullName);

            Assert.Equal(1, configuration.LoggingRules[0].Filters.Count);
            Assert.Equal("MyExtensionNamespace.WhenFooFilter", configuration.LoggingRules[0].Filters[0].GetType().FullName);
        }

        [Fact]
        public void ExtensionTest2()
        {
            var configuration = new LogFactory().Setup().LoadConfigurationFromXml(@"
<nlog throwExceptions='true'>
    <extensions>
        <add assembly='" + extensionAssemblyName1 + @"' />
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
</nlog>").LogFactory.Configuration;

            Target myTarget = configuration.FindTargetByName("t");
            Assert.Equal("MyExtensionNamespace.MyTarget", myTarget.GetType().FullName);

            var d1Target = (DebugTarget)configuration.FindTargetByName("d1");
            var layout = d1Target.Layout as SimpleLayout;
            Assert.NotNull(layout);
            Assert.Single(layout.Renderers);
            Assert.Equal("MyExtensionNamespace.FooLayoutRenderer", layout.Renderers[0].GetType().FullName);

            var d2Target = (DebugTarget)configuration.FindTargetByName("d2");
            Assert.Equal("MyExtensionNamespace.FooLayout", d2Target.Layout.GetType().FullName);

            Assert.Equal(2, configuration.LoggingRules[0].Filters.Count);
            Assert.Equal("MyExtensionNamespace.WhenFooFilter", configuration.LoggingRules[0].Filters[0].GetType().FullName);
            var cbf = configuration.LoggingRules[0].Filters[1] as ConditionBasedFilter;
            Assert.NotNull(cbf);
            Assert.Equal("(myrandom(10) == 3)", cbf.Condition.ToString());
        }

        [Fact]
        public void ExtensionWithPrefixTest()
        {
            var configuration = new LogFactory().Setup().LoadConfigurationFromXml(@"
<nlog throwExceptions='true'>
    <extensions>
        <add prefix='myprefix' assemblyFile='" + GetExtensionAssemblyFullPath() + @"' />
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
</nlog>").LogFactory.Configuration;

            Target myTarget = configuration.FindTargetByName("t");
            Assert.Equal("MyExtensionNamespace.MyTarget", myTarget.GetType().FullName);

            var d1Target = (DebugTarget)configuration.FindTargetByName("d1");
            var layout = d1Target.Layout as SimpleLayout;
            Assert.NotNull(layout);
            Assert.Single(layout.Renderers);
            Assert.Equal("MyExtensionNamespace.FooLayoutRenderer", layout.Renderers[0].GetType().FullName);

            var d2Target = (DebugTarget)configuration.FindTargetByName("d2");
            Assert.Equal("MyExtensionNamespace.FooLayout", d2Target.Layout.GetType().FullName);

            Assert.Equal(1, configuration.LoggingRules[0].Filters.Count);
            Assert.Equal("MyExtensionNamespace.WhenFooFilter", configuration.LoggingRules[0].Filters[0].GetType().FullName);
        }

        [Fact]
        public void ExtensionTest4()
        {
            Assert.NotNull(typeof(FooLayout));

            var configuration = new LogFactory().Setup().LoadConfigurationFromXml(@"
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
</nlog>").LogFactory.Configuration;

            Target myTarget = configuration.FindTargetByName("t");
            Assert.Equal("MyExtensionNamespace.MyTarget", myTarget.GetType().FullName);

            var d1Target = (DebugTarget)configuration.FindTargetByName("d1");
            var layout = d1Target.Layout as SimpleLayout;
            Assert.NotNull(layout);
            Assert.Single(layout.Renderers);
            Assert.Equal("MyExtensionNamespace.FooLayoutRenderer", layout.Renderers[0].GetType().FullName);

            var d2Target = (DebugTarget)configuration.FindTargetByName("d2");
            Assert.Equal("MyExtensionNamespace.FooLayout", d2Target.Layout.GetType().FullName);

            Assert.Equal(1, configuration.LoggingRules[0].Filters.Count);
            Assert.Equal("MyExtensionNamespace.WhenFooFilter", configuration.LoggingRules[0].Filters[0].GetType().FullName);
        }

        [Fact]
        public void ExtensionTest_extensions_not_top_and_used()
        {
            Assert.NotNull(typeof(FooLayout));

            var configuration = new LogFactory().Setup().LoadConfigurationFromXml(@"
<nlog throwExceptions='true'>
    
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

    <extensions>
        <add assemblyFile='" + GetExtensionAssemblyFullPath() + @"' />
    </extensions>

</nlog>").LogFactory.Configuration;

            Target myTarget = configuration.FindTargetByName("t");
            Assert.Equal("MyExtensionNamespace.MyTarget", myTarget.GetType().FullName);

            var d1Target = (DebugTarget)configuration.FindTargetByName("d1");
            var layout = d1Target.Layout as SimpleLayout;
            Assert.NotNull(layout);
            Assert.Single(layout.Renderers);
            Assert.Equal("MyExtensionNamespace.FooLayoutRenderer", layout.Renderers[0].GetType().FullName);

            var d2Target = (DebugTarget)configuration.FindTargetByName("d2");
            Assert.Equal("MyExtensionNamespace.FooLayout", d2Target.Layout.GetType().FullName);

            Assert.Equal(1, configuration.LoggingRules[0].Filters.Count);
            Assert.Equal("MyExtensionNamespace.WhenFooFilter", configuration.LoggingRules[0].Filters[0].GetType().FullName);
        }

        [Fact]
        public void ExtensionShouldThrowNLogConfiguratonExceptionWhenRegisteringInvalidType()
        {
            var configXml = @"
<nlog throwConfigExceptions='true'>
    <extensions>
                <add type='some_type_that_doesnt_exist'/>
</extensions>
</nlog>";
            Assert.Throws<NLogConfigurationException>(() => XmlLoggingConfiguration.CreateFromXmlString(configXml));
        }

        [Fact]
        public void ExtensionShouldThrowNLogConfiguratonExceptionWhenRegisteringInvalidAssembly()
        {
            var configXml = @"
<nlog throwConfigExceptions='true'>
    <extensions>
        <add assembly='some_assembly_that_doesnt_exist'/>
    </extensions>
</nlog>";
            Assert.Throws<NLogConfigurationException>(() => XmlLoggingConfiguration.CreateFromXmlString(configXml));
        }

        [Fact]
        public void ExtensionShouldThrowNLogConfiguratonExceptionWhenRegisteringInvalidAssemblyFile()
        {
            var configXml = @"
<nlog throwConfigExceptions='true'>
    <extensions>
                <add assemblyfile='some_file_that_doesnt_exist'/>
</extensions>
</nlog>";
            Assert.Throws<NLogConfigurationException>(() => XmlLoggingConfiguration.CreateFromXmlString(configXml));
        }

        [Fact]
        public void ExtensionShouldNotThrowWhenRegisteringInvalidTypeIfThrowConfigExceptionsFalse()
        {
            var configXml = @"
<nlog throwConfigExceptions='false'>
    <extensions>
                <add type='some_type_that_doesnt_exist'/>
                <add assembly='NLog'/>
</extensions>
</nlog>";
            XmlLoggingConfiguration.CreateFromXmlString(configXml);
        }

        [Fact]
        public void ExtensionShouldNotThrowWhenRegisteringInvalidAssemblyIfThrowConfigExceptionsFalse()
        {
            var configXml = @"
<nlog throwConfigExceptions='false'>
    <extensions>
        <add assembly='some_assembly_that_doesnt_exist'/>
    </extensions>
</nlog>";
            XmlLoggingConfiguration.CreateFromXmlString(configXml);
        }

        [Fact]
        public void ExtensionShouldNotThrowWhenRegisteringInvalidAssemblyFileIfThrowConfigExceptionsFalse()
        {
            var configXml = @"
<nlog throwConfigExceptions='false'>
    <extensions>
                <add assemblyfile='some_file_that_doesnt_exist'/>
</extensions>
</nlog>";
            XmlLoggingConfiguration.CreateFromXmlString(configXml);
        }

        [Fact]
        public void CustomXmlNamespaceTest()
        {
            var configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
<nlog throwExceptions='true' xmlns:foo='http://bar'>
    <targets>
        <target name='d' type='foo:Debug' />
    </targets>
</nlog>");

            var d1Target = (DebugTarget)configuration.FindTargetByName("d");
            Assert.NotNull(d1Target);
        }

        [Fact]
        public void Extension_should_be_auto_loaded_when_following_NLog_dll_format()
        {
            var fileLocations = ConfigurationItemFactory.GetAutoLoadingFileLocations().ToArray();
            Assert.NotEmpty(fileLocations);
            Assert.NotNull(fileLocations[0].Key);
            Assert.NotNull(fileLocations[0].Value); // Primary search location is NLog-assembly
            Assert.Equal(fileLocations.Length, fileLocations.Select(f => f.Key).Distinct().Count());

            var logFactory = new LogFactory().Setup().LoadConfigurationFromXml(@"
<nlog throwExceptions='true' autoLoadExtensions='true'>
<targets>
    <target name='t' type='AutoLoadTarget' />
</targets>

<rules>
    <logger name='*' writeTo='t' />
</rules>
</nlog>").LogFactory;

            var autoLoadedTarget = logFactory.Configuration.FindTargetByName("t");
            Assert.Equal("NLogAutloadExtension.AutoLoadTarget", autoLoadedTarget.GetType().ToString());
        }

        [Fact]
        public void ExtensionTypeWithAssemblyNameCanLoad()
        {
            var logFactory = new LogFactory().Setup().LoadConfigurationFromXml(@"
<nlog throwExceptions='true'>
<targets>
    <target name='t' type='AutoLoadTarget,  NLogAutoLoadExtension' />
</targets>
<rules>
    <logger name='*' writeTo='t' />
</rules>
</nlog>").LogFactory;

            var autoLoadedTarget = logFactory.Configuration.FindTargetByName("t");
            Assert.Equal("NLogAutloadExtension.AutoLoadTarget", autoLoadedTarget.GetType().ToString());
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Extension_loading_could_be_canceled(bool cancel)
        {
            EventHandler<AssemblyLoadingEventArgs> onAssemblyLoading = (sender, e) =>
            {
                if (e.Assembly.FullName.Contains("NLogAutoLoadExtension"))
                {
                    e.Cancel = cancel;
                }
            };

            try
            {
                ConfigurationItemFactory.AssemblyLoading += onAssemblyLoading;

                using(new NoThrowNLogExceptions())
                {
                    var logFactory = new LogFactory().Setup().LoadConfigurationFromXml(@"
<nlog throwExceptions='false' autoLoadExtensions='true'>
    <targets>
        <target name='t' type='AutoLoadTarget' />
    </targets>

    <rules>
      <logger name='*' writeTo='t' />
    </rules>
</nlog>").LogFactory;

                    var autoLoadedTarget = logFactory.Configuration.FindTargetByName("t");

                    if (cancel)
                    {
                        Assert.Null(autoLoadedTarget);
                    }
                    else
                    {
                        Assert.Equal("NLogAutloadExtension.AutoLoadTarget", autoLoadedTarget.GetType().ToString());
                    }
                }
            }
            finally
            {
                //cleanup
                ConfigurationItemFactory.AssemblyLoading -= onAssemblyLoading;
            }
        }

        [Fact]
        public void Extensions_NLogPackageLoader_should_beCalled()
        {
            try
            {

                var writer = new StringWriter();
                InternalLogger.LogWriter = writer;
                InternalLogger.LogLevel = LogLevel.Debug;

                var logFactory = new LogFactory().Setup().LoadConfigurationFromXml(@"
<nlog throwExceptions='true'>
<extensions>
 <add assembly='PackageLoaderTestAssembly' />
</extensions>

</nlog>");


                var logs = writer.ToString();
                Assert.Contains("Preload successfully invoked for 'LoaderTestInternal.NLogPackageLoader'", logs);
                Assert.Contains("Preload successfully invoked for 'LoaderTestPublic.NLogPackageLoader'", logs);
                Assert.Contains("Preload successfully invoked for 'LoaderTestPrivateNestedStatic.SomeType+NLogPackageLoader'", logs);
                Assert.Contains("Preload successfully invoked for 'LoaderTestPrivateNested.SomeType+NLogPackageLoader'", logs);

                //4 times successful
                Assert.Equal(4, Regex.Matches(logs, Regex.Escape("Preload successfully invoked for '")).Count);

            }
            finally
            {
                InternalLogger.Reset();
            }
        }

        [Fact]
        public void ImplicitConversionOperatorTest()
        {
            var config = XmlLoggingConfiguration.CreateFromXmlString(@"
            <nlog throwExceptions='true'>
    <extensions>
        <add assemblyFile='" + GetExtensionAssemblyFullPath() + @"' />
    </extensions>
                <targets>
                    <target name='myTarget' type='MyTarget' layout='123' />
                </targets>
                <rules>
                    <logger name='*' level='Debug' writeTo='myTarget' />
                </rules>
            </nlog>");

            var target = config.FindTargetByName<MyTarget>("myTarget");
            Assert.NotNull(target);
            Assert.Equal(123, target.Layout.X);
        }

        [Fact]
        public void LoadExtensionFromAppDomain()
        {
            try
            {
                // ...\NLog\tests\NLog.UnitTests\bin\Debug\netcoreapp2.0\nlog.dll
                var nlogDirectory = new DirectoryInfo(ConfigurationItemFactory.GetAutoLoadingFileLocations().First().Key);
                var configurationDirectory = nlogDirectory.Parent;
                var testsDirectory = configurationDirectory.Parent.Parent.Parent;
                var manuallyLoadedAssemblyPath = Path.Combine(testsDirectory.FullName, "ManuallyLoadedExtension", "bin", configurationDirectory.Name,
#if NETSTANDARD
                    "netstandard2.0",
#elif NET35 || NET40 || NET45
                    "net461",
#else
                    nlogDirectory.Name,
#endif
                    "ManuallyLoadedExtension.dll");
                Assembly.LoadFrom(manuallyLoadedAssemblyPath);

                InternalLogger.LogLevel = LogLevel.Trace;
                var writer = new StringWriter();
                InternalLogger.LogWriter = writer;

                var configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
<nlog throwExceptions='true'>
    <extensions>
        <add assembly='ManuallyLoadedExtension' />
    </extensions>

    <targets>
        <target name='t' type='ManuallyLoadedTarget' />
    </targets>
</nlog>");

                // We get Exception for normal Assembly-Load only in net452.
#if !NETSTANDARD && !MONO
                var logs = writer.ToString();
                Assert.Contains("Try find 'ManuallyLoadedExtension' in current domain", logs);
#endif

                // Was AssemblyLoad successful?
                var autoLoadedTarget = configuration.FindTargetByName("t");
                Assert.Equal("ManuallyLoadedExtension.ManuallyLoadedTarget", autoLoadedTarget.GetType().FullName);
            }
            finally
            {
                InternalLogger.Reset();
            }
        }
    }
}