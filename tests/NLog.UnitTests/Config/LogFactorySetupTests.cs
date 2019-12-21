// 
// Copyright (c) 2004-2019 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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
using NLog.Config;
using NLog.Targets;
using Xunit;

namespace NLog.UnitTests.Config
{
    public class LogFactorySetupTests
    {
        [Fact]
        public void SetupAutoShutdownWithAppDomainTest()
        {
            // Arrange
            var logFactory = new LogFactory();
            // Act
            logFactory.Setup(b => b.AutoShutdownWithAppDomain(false));
            // Assert
            Assert.False(logFactory.AutoShutdown);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void SetupExtensionsAutoLoadExtensionsTest(bool autoLoadAssemblies)
        {
            try
            {
                // Arrange
                var logFactory = new LogFactory();

                // Act
                logFactory.Setup().SetupExtensions(ext => ext.AutoLoadAssemblies(autoLoadAssemblies));
                Func<LogFactory, LoggingConfiguration> buildConfig = (f) => new XmlLoggingConfiguration(@"<nlog throwExceptions='true'>
                    <targets>
                        <target name='t' type='AutoLoadTarget' />
                    </targets>
                    <rules>
                      <logger name='*' writeTo='t'>
                      </logger>
                    </rules>
                </nlog>", null, f);

                // Assert
                if (autoLoadAssemblies)
                {
                    logFactory.Configuration = buildConfig(logFactory);
                    Assert.NotNull(logFactory.Configuration.FindTargetByName("t"));
                }
                else
                {
                    Assert.Throws<NLogConfigurationException>(() => buildConfig(logFactory));
                }
            }
            finally
            {
                ConfigurationItemFactory.Default = null;    // Restore global default
            }
        }

        [Fact]
        public void SetupExtensionsRegisterAssemblyNameTest()
        {
            try
            {
                // Arrange
                var logFactory = new LogFactory();

                // Act
                logFactory.Setup().SetupExtensions(ext => ext.AutoLoadAssemblies(false).RegisterAssembly("NLogAutoLoadExtension"));
                logFactory.Configuration = new XmlLoggingConfiguration(@"<nlog throwExceptions='true'>
                    <targets>
                        <target name='t' type='AutoLoadTarget' />
                    </targets>
                    <rules>
                      <logger name='*' writeTo='t'>
                      </logger>
                    </rules>
                </nlog>", null, logFactory);

                // Assert
                Assert.NotNull(logFactory.Configuration.FindTargetByName("t"));
            }
            finally
            {
                ConfigurationItemFactory.Default = null;    // Restore global default
            }
        }

        [Fact]
        public void SetupExtensionsRegisterAssemblyTest()
        {
            try
            {
                // Arrange
                var logFactory = new LogFactory();

                // Act
                logFactory.Setup().SetupExtensions(ext => ext.AutoLoadAssemblies(false).RegisterAssembly(typeof(MyExtensionNamespace.MyTarget).Assembly));
                logFactory.Configuration = new XmlLoggingConfiguration(@"<nlog throwExceptions='true'>
                    <targets>
                        <target name='t' type='MyTarget' />
                    </targets>
                    <rules>
                      <logger name='*' writeTo='t'>
                      </logger>
                    </rules>
                </nlog>", null, logFactory);

                // Assert
                Assert.NotNull(logFactory.Configuration.FindTargetByName<MyExtensionNamespace.MyTarget>("t"));
            }
            finally
            {
                ConfigurationItemFactory.Default = null;    // Restore global default
            }
        }

        [Fact]
        public void SetupExtensionsRegisterTargetTest()
        {
            try
            {
                // Arrange
                var logFactory = new LogFactory();

                // Act
                logFactory.Setup().SetupExtensions(ext => ext.AutoLoadAssemblies(false).RegisterTarget<MyExtensionNamespace.MyTarget>("MyTarget"));
                logFactory.Configuration = new XmlLoggingConfiguration(@"<nlog throwExceptions='true'>
                    <targets>
                        <target name='t' type='MyTarget' />
                    </targets>
                    <rules>
                      <logger name='*' writeTo='t'>
                      </logger>
                    </rules>
                </nlog>", null, logFactory);

                // Assert
                Assert.NotNull(logFactory.Configuration.FindTargetByName<MyExtensionNamespace.MyTarget>("t"));
            }
            finally
            {
                ConfigurationItemFactory.Default = null;    // Restore global default
            }
        }

        [Fact]
        public void SetupExtensionsRegisterTargetTypeTest()
        {
            try
            {
                // Arrange
                var logFactory = new LogFactory();

                // Act
                logFactory.Setup().SetupExtensions(ext => ext.AutoLoadAssemblies(false).RegisterTarget("MyTarget", typeof(MyExtensionNamespace.MyTarget)));
                logFactory.Configuration = new XmlLoggingConfiguration(@"<nlog throwExceptions='true'>
                    <targets>
                        <target name='t' type='MyTarget' />
                    </targets>
                    <rules>
                      <logger name='*' writeTo='t'>
                      </logger>
                    </rules>
                </nlog>", null, logFactory);

                // Assert
                Assert.NotNull(logFactory.Configuration.FindTargetByName<MyExtensionNamespace.MyTarget>("t"));
            }
            finally
            {
                ConfigurationItemFactory.Default = null;    // Restore global default
            }
        }

        [Fact]
        public void SetupExtensionsRegisterLayoutMethodTest()
        {
            try
            {
                // Arrange
                var logFactory = new LogFactory();

                // Act
                logFactory.Setup().SetupExtensions(ext => ext.RegisterLayoutRenderer("mylayout", (l) => "42"));
                logFactory.Configuration = new XmlLoggingConfiguration(@"<nlog throwExceptions='true'>
                    <targets>
                        <target name='debug' type='Debug' layout='${mylayout}' />
                    </targets>
                    <rules>
                      <logger name='*' writeTo='debug'>
                      </logger>
                    </rules>
                </nlog>", null, logFactory);
                logFactory.GetLogger("Hello").Info("World");

                // Assert
                Assert.Equal("42", logFactory.Configuration.FindTargetByName<DebugTarget>("debug").LastMessage);
            }
            finally
            {
                ConfigurationItemFactory.Default = null;    // Restore global default
            }
        }

        [Fact]
        public void SetupExtensionsRegisterLayoutRendererTest()
        {
            try
            {
                // Arrange
                var logFactory = new LogFactory();

                // Act
                logFactory.Setup().SetupExtensions(ext => ext.RegisterLayoutRenderer<MyExtensionNamespace.FooLayoutRenderer>("foo"));
                logFactory.Configuration = new XmlLoggingConfiguration(@"<nlog throwExceptions='true'>
                    <targets>
                        <target name='debug' type='Debug' layout='${foo}' />
                    </targets>
                    <rules>
                      <logger name='*' writeTo='debug'>
                      </logger>
                    </rules>
                </nlog>", null, logFactory);
                logFactory.GetLogger("Hello").Info("World");

                // Assert
                Assert.Equal("foo", logFactory.Configuration.FindTargetByName<DebugTarget>("debug").LastMessage);
            }
            finally
            {
                ConfigurationItemFactory.Default = null;    // Restore global default
            }
        }

        [Fact]
        public void SetupExtensionsRegisterLayoutRendererTypeTest()
        {
            try
            {
                // Arrange
                var logFactory = new LogFactory();

                // Act
                logFactory.Setup().SetupExtensions(ext => ext.RegisterLayoutRenderer("foo", typeof(MyExtensionNamespace.FooLayoutRenderer)));
                logFactory.Configuration = new XmlLoggingConfiguration(@"<nlog throwExceptions='true'>
                    <targets>
                        <target name='debug' type='Debug' layout='${foo}' />
                    </targets>
                    <rules>
                      <logger name='*' writeTo='debug'>
                      </logger>
                    </rules>
                </nlog>", null, logFactory);
                logFactory.GetLogger("Hello").Info("World");

                // Assert
                Assert.Equal("foo", logFactory.Configuration.FindTargetByName<DebugTarget>("debug").LastMessage);
            }
            finally
            {
                ConfigurationItemFactory.Default = null;    // Restore global default
            }
        }
    }
}
