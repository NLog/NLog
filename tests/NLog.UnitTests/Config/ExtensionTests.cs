//
// Copyright (c) 2004-2024 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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
    using System.Linq;
    using System.Reflection;
    using MyExtensionNamespace;
    using NLog.Common;
    using NLog.Config;
    using NLog.Filters;
    using NLog.Layouts;
    using NLog.Targets;
    using Xunit;

    public class ExtensionTests : NLogTestBase
    {
        private readonly static string extensionAssemblyName1 = "SampleExtensions";
        private readonly static string extensionAssemblyFullPath1 = Path.GetFullPath("SampleExtensions.dll");

        private static string GetExtensionAssemblyFullPath()
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
           <whenFoo x='44' action='Log' />
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

            Assert.Single(configuration.LoggingRules[0].Filters);
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
        public void ExtensionWithPrefixLoadTwiceTest()
        {
            var configuration = new LogFactory().Setup().SetupExtensions(ext => ext.RegisterAssembly(extensionAssemblyName1))
                .LoadConfigurationFromXml(@"
<nlog throwExceptions='true'>
    <extensions>
        <add assembly='" + extensionAssemblyName1 + @"' prefix='twice' />
    </extensions>

    <targets>
        <target name='t' type='twice.MyTarget' />
        <target name='d1' type='Debug' layout='${foo}' />
        <target name='d2' type='Debug'>
            <layout type='twice.FooLayout' x='1'>
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
           <myprefix.whenFoo x='44' action='Log' />
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

            Assert.Single(configuration.LoggingRules[0].Filters);
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
           <whenFoo x='44' action='Log' />
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

            Assert.Single(configuration.LoggingRules[0].Filters);
            Assert.Equal("MyExtensionNamespace.WhenFooFilter", configuration.LoggingRules[0].Filters[0].GetType().FullName);
        }

        [Fact]
        [Obsolete("Instead override type-creation by calling NLog.LogManager.Setup().SetupExtensions(). Marked obsolete with NLog v5.2")]
        public void RegisterNamedTypeLessTest()
        {
            Assert.NotNull(typeof(FooLayout));
            var configurationItemFactory = new ConfigurationItemFactory();
            configurationItemFactory.GetLayoutFactory().RegisterNamedType("foo", typeof(FooLayout).ToString() + "," + typeof(FooLayout).Assembly.GetName().Name);
            Assert.NotNull(configurationItemFactory.LayoutFactory.CreateInstance("foo"));
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
           <whenFoo x='44' action='Log' />
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

            Assert.Single(configuration.LoggingRules[0].Filters);
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
            var result = XmlLoggingConfiguration.CreateFromXmlString(configXml);
            Assert.NotNull(result);
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
            var result = XmlLoggingConfiguration.CreateFromXmlString(configXml);
            Assert.NotNull(result);
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
            var result = XmlLoggingConfiguration.CreateFromXmlString(configXml);
            Assert.NotNull(result);
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
        [Obsolete("Instead use RegisterType<T>, as dynamic Assembly loading will be moved out. Marked obsolete with NLog v5.2")]
        public void Extension_should_be_auto_loaded_when_following_NLog_dll_format()
        {
            var fileLocations = AssemblyExtensionLoader.GetAutoLoadingFileLocations().ToArray();
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
        [Obsolete("Instead use RegisterType<T>, as dynamic Assembly loading will be moved out. Marked obsolete with NLog v5.2")]
        public void LoadExtensionFromAppDomain()
        {
            try
            {
                LoadManuallyLoadedExtensionDll();

                InternalLogger.LogLevel = LogLevel.Trace;
                var writer = new StringWriter();
                InternalLogger.LogWriter = writer;

                var configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
<nlog throwExceptions='true'>
    <extensions>
        <add assembly='Manually-Loaded-Extension' />
    </extensions>

    <targets>
        <target name='t' type='ManuallyLoadedTarget' />
    </targets>
</nlog>");

                // We get Exception for normal Assembly-Load only in net452.
#if NETFRAMEWORK && !MONO
                var logs = writer.ToString();
                Assert.Contains("Try find 'Manually-Loaded-Extension' in current domain", logs);
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

        [Fact]
        [Obsolete("Instead use RegisterType<T>, as dynamic Assembly loading will be moved out. Marked obsolete with NLog v5.2")]
        public void FullyQualifiedExtensionTest()
        {
            // Arrange

            LoadManuallyLoadedExtensionDll();

            // Act
            var logFactory = new LogFactory().Setup().LoadConfigurationFromXml(@"<nlog throwConfigExceptions='true'>
                <targets>
                    <target name='t' type='ManuallyLoadedTarget, Manually-Loaded-Extension' />
                </targets>
            </nlog>").LogFactory;


            // Assert
            Assert.NotNull(logFactory.Configuration.FindTargetByName("t"));
        }

        [Theory]
        [InlineData((string)null, (string)null)]
        [InlineData("", (string)null)]
        [InlineData("ManuallyLoadedTarget", "ManuallyLoadedExtension.ManuallyLoadedTarget")]
        [InlineData("ManuallyLoaded-Target", "ManuallyLoadedExtension.ManuallyLoadedTarget")]
        [InlineData(", Manually-Loaded-Extension", (string)null)] // border case
        [InlineData("ManuallyLoadedTarget,", (string)null)] // border case
        [Obsolete("Instead use RegisterType<T>, as dynamic Assembly loading will be moved out. Marked obsolete with NLog v5.2")]
        public void NormalizeNameTest(string input, string expected)
        {
            // Arrange
            var assembly = LoadManuallyLoadedExtensionDll();
            var configFactory = new ConfigurationItemFactory();
            configFactory.AssemblyLoader.LoadAssembly(configFactory, assembly, string.Empty);

            // Act
            var foundInstance = configFactory.TargetFactory.TryCreateInstance(input, out var outputInstance);
            var instance = (foundInstance || expected != null) ? configFactory.GetTargetFactory().CreateInstance(input) : null;

            // Assert
            Assert.Equal(expected != null, foundInstance);
            Assert.Equal(expected, instance?.GetType().ToString());
            Assert.Equal(expected, outputInstance?.GetType().ToString());
        }

        [Obsolete("Instead use RegisterType<T>, as dynamic Assembly loading will be moved out. Marked obsolete with NLog v5.2")]
        private static Assembly LoadManuallyLoadedExtensionDll()
        {
            // ...\NLog\tests\NLog.UnitTests\bin\Debug\netcoreapp2.0\nlog.dll
            var nlogDirectory = new DirectoryInfo(AssemblyExtensionLoader.GetAutoLoadingFileLocations().First().Key);
            var configurationDirectory = nlogDirectory.Parent;
            var testsDirectory = configurationDirectory.Parent.Parent.Parent;
            var manuallyLoadedAssemblyPath = Path.Combine(testsDirectory.FullName, "ManuallyLoadedExtension", "bin", configurationDirectory.Name,
#if NETSTANDARD
                "netstandard2.0",
#elif NET35 || NET40 || NET45
                "net462",
#else
                nlogDirectory.Name,
#endif
                "Manually-Loaded-Extension.dll");
            return Assembly.LoadFrom(manuallyLoadedAssemblyPath);

        }
    }
}
