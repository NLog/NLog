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
    using System.Text;
    using NLog.Config;
    using NLog.Targets;
    using Xunit;

    public class ServiceRepositoryTests : NLogTestBase
    {
        [Fact]
        public void SideBySideLogFactoryExternalInterfaceTest()
        {
            // Arrange
            var logFactory1 = new LogFactory();
            const string name1 = "name1";
            logFactory1.ServiceRepository.RegisterService(typeof(IMyPrettyInterface), new MyPrettyImplementation() { Test = name1 });

            var logFactory2 = new LogFactory();
            const string name2 = "name2";
            logFactory2.ServiceRepository.RegisterService(typeof(IMyPrettyInterface), new MyPrettyImplementation { Test = name2 });

            // Act
            var logFactoryService1 = logFactory1.ServiceRepository.ResolveService<IMyPrettyInterface>();
            var logFactoryService2 = logFactory2.ServiceRepository.ResolveService<IMyPrettyInterface>();

            // Assert
            Assert.Equal(name1, logFactoryService1.ToString());
            Assert.Equal(name2, logFactoryService2.ToString());
        }

        [Fact]
        public void SideBySideLogFactoryInternalInterfaceTest()
        {
            // Arrange
            var logFactory1 = new LogFactory();
            const string name1 = "name1";
            InitializeLogFactoryJsonConverter(logFactory1, name1, out var logger1, out var target1);

            var logFactory2 = new LogFactory();
            const string name2 = "name2";
            InitializeLogFactoryJsonConverter(logFactory2, name2, out var logger2, out var target2);

            // Act
            logger1.Info("Hello {user}", "Kenny");
            logger2.Info("Hello {user}", "Kenny");

            // Assert
            Assert.Equal("Kenny" + "_" + name1, target1.LastMessage);
            Assert.Equal("Kenny" + "_" + name2, target2.LastMessage);
        }

        [Fact]
        public void HandleDelayedInjectDependenciesFailure()
        {
            using (new NoThrowNLogExceptions())
            {
                // Arrange
                var logFactory = new LogFactory();
                logFactory.ThrowConfigExceptions = true;
                var logConfig = new LoggingConfiguration(logFactory);
                var logTarget = new TargetWithMissingDependency() { Name = "NeedDependency" };
                logConfig.AddRuleForAllLevels(logTarget);

                // Act
                logFactory.Configuration = logConfig;
                logFactory.GetLogger("Test").Info("Test");

                // Assert
                Assert.Null(logTarget.LastLogEvent);
            }
        }

        [Fact]
        public void HandleDelayedInjectDependenciesSuccess()
        {
            using (new NoThrowNLogExceptions())
            {
                // Arrange
                var logFactory = new LogFactory();
                logFactory.ThrowConfigExceptions = true;
                var logConfig = new LoggingConfiguration(logFactory);
                var logTarget = new TargetWithMissingDependency() { Name = "NeedDependency" };
                logConfig.AddRuleForAllLevels(logTarget);

                // Act
                logFactory.Configuration = logConfig;
                logFactory.GetLogger("Test").Info("Test");
                logFactory.ServiceRepository.RegisterSingleton<IMisingDependencyClass>(new MisingDependencyClass());
                logFactory.GetLogger("Test").Info("Test Again");

                // Assert
                Assert.NotNull(logTarget.LastLogEvent);
            }
        }

        [Fact]
        public void HandleLayoutRendererDependency()
        {
            // Arrange
            var logFactory = new LogFactory().Setup().SetupExtensions(ext =>
            {
                ext.RegisterLayoutRenderer<LayoutRendererUsingDependency>();
                ext.RegisterServiceProvider(new ExternalServiceRepository(t => t == typeof(IMisingDependencyClass) ? new MisingDependencyClass() : null));
            }).LoadConfiguration(builder =>
            {
                builder.ForLogger().WriteTo(new MemoryTarget() { Layout = "${NeedDependency}" });
            }).LogFactory;

            // Act
            logFactory.GetLogger("Test").Info("Test");

            // Assert
            Assert.Equal("Success", (logFactory.Configuration.AllTargets[0] as MemoryTarget).Logs[0]);
        }

        [Fact]
        public void ResolveShouldCheckExternalServiceProvider()
        {
            // Arrange
            var logFactory = new LogFactory().Setup().SetupExtensions(ext =>
            {
                ext.RegisterSingletonService<IMyPrettyInterface>(new MyPrettyImplementation());
                ext.RegisterSingletonService(typeof(IMyPrettyInterface), new MyPrettyImplementation());
                ext.RegisterServiceProvider(new ExternalServiceRepository(t => t == typeof(IMisingDependencyClass) ? new MisingDependencyClass() : null));
            }).LogFactory;

            // Act
            var missingDependency = logFactory.ServiceRepository.ResolveService<IMisingDependencyClass>(false);
            var otherDependency = logFactory.ServiceRepository.ResolveService<IMyPrettyInterface>(false);

            // Assert
            Assert.NotNull(missingDependency);
            Assert.NotNull(otherDependency);
        }

        private static void InitializeLogFactoryJsonConverter(LogFactory logFactory, string testValue, out Logger logger, out DebugTarget target)
        {
            logFactory.ServiceRepository.RegisterService(typeof(IJsonConverter), new MySimpleJsonConverter { Test = testValue });

            var xmlConfig = @"<nlog><targets><target type='debug' name='test' layout='${event-properties:user:format=@}'/></targets><rules><logger name='*' minLevel='Debug' writeTo='test'/></rules></nlog>";
            logFactory.Configuration = XmlLoggingConfiguration.CreateFromXmlString(xmlConfig, logFactory);
            logger = logFactory.GetLogger(nameof(logFactory));
            target = logFactory.Configuration.FindTargetByName("test") as DebugTarget;
        }

        private interface IMyPrettyInterface
        {
            string Test { get; }
        }

        private class MyPrettyImplementation : IMyPrettyInterface
        {
            public string Test { get; set; }
            public override string ToString()
            {
                return Test;
            }
        }

        private class MySimpleJsonConverter : IJsonConverter
        {
            public string Test { get; set; }

            public bool SerializeObject(object value, StringBuilder builder)
            {
                builder.Append(string.Concat(value.ToString(), "_", Test));
                return true;
            }
        }

        private class TargetWithMissingDependency : Target
        {
            public LogEventInfo LastLogEvent { get; private set; }

            protected override void InitializeTarget()
            {
                var wantedDependency = ResolveService<IMisingDependencyClass>();
                base.InitializeTarget();
            }

            protected override void Write(LogEventInfo logEvent)
            {
                LastLogEvent = logEvent;
            }
        }

        [NLog.LayoutRenderers.LayoutRenderer("needdependency")]
        private class LayoutRendererUsingDependency : NLog.LayoutRenderers.LayoutRenderer
        {
            private object _wantedDependency;

            protected override void InitializeLayoutRenderer()
            {
                _wantedDependency = ResolveService<IMisingDependencyClass>();
                base.InitializeLayoutRenderer();
            }

            protected override void Append(StringBuilder builder, LogEventInfo logEvent)
            {
                builder.Append(_wantedDependency != null ? "Success" : "Failed");
            }
        }

        private interface IMisingDependencyClass
        {

        }

        private class MisingDependencyClass : IMisingDependencyClass
        {

        }

        private class ExternalServiceRepository : IServiceProvider
        {
            private readonly Func<Type, object> _serviceResolver;

            public ExternalServiceRepository(Func<Type, object> serviceResolver)
            {
                _serviceResolver = serviceResolver;
            }

            public object GetService(Type serviceType)
            {
                return _serviceResolver(serviceType);
            }
        }
    }
}
