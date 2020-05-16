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

namespace NLog
{
    using System;
    using System.Runtime.CompilerServices;
    using NLog.Config;
    using NLog.Internal;

    /// <summary>
    /// Extension methods to setup LogFactory options
    /// </summary>
    public static class SetupBuilderExtensions
    {
        /// <summary>
        /// Gets the logger with the full name of the current class, so namespace and class name.
        /// </summary>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static Logger GetCurrentClassLogger(this ISetupBuilder setupBuilder)
        {
            return setupBuilder.LogFactory.GetLogger(StackTraceUsageUtils.GetClassFullName());
        }

        /// <summary>
        /// Gets the specified named logger.
        /// </summary>
        public static Logger GetLogger(this ISetupBuilder setupBuilder, string name)
        {
            return setupBuilder.LogFactory.GetLogger(name);
        }

        /// <summary>
        /// Configures loading of NLog extensions for Targets and LayoutRenderers
        /// </summary>
        public static ISetupBuilder SetupExtensions(this ISetupBuilder setupBuilder, Action<ISetupExtensionsBuilder> extensionsBuilder)
        {
            extensionsBuilder(new SetupExtensionsBuilder(setupBuilder.LogFactory));
            return setupBuilder;
        }

        /// <summary>
        /// Configures the output of NLog <see cref="Common.InternalLogger"/> for diagnostics / troubleshooting
        /// </summary>
        public static ISetupBuilder SetupInternalLogger(this ISetupBuilder setupBuilder, Action<ISetupInternalLoggerBuilder> internalLoggerBuilder)
        {
            internalLoggerBuilder(new SetupInternalLoggerBuilder(setupBuilder.LogFactory));
            return setupBuilder;
        }

        /// <summary>
        /// Configures serialization and transformation of LogEvents
        /// </summary>
        public static ISetupBuilder SetupSerialization(this ISetupBuilder setupBuilder, Action<ISetupSerializationBuilder> serializationBuilder)
        {
            serializationBuilder(new SetupSerializationBuilder(setupBuilder.LogFactory));
            return setupBuilder;
        }

        /// <summary>
        /// Loads NLog config created by the method <paramref name="configBuilder"/>
        /// </summary>
        public static ISetupBuilder LoadConfiguration(this ISetupBuilder setupBuilder, Action<ISetupLoadConfigurationBuilder> configBuilder)
        {
            var config = setupBuilder.LogFactory._config;
            var setupConfig = new SetupLoadConfigurationBuilder(setupBuilder.LogFactory, config);
            configBuilder(setupConfig);
            var newConfig = setupConfig._configuration;
            bool configHasChanged = !ReferenceEquals(config, setupBuilder.LogFactory._config);

            if (ReferenceEquals(newConfig, setupBuilder.LogFactory._config))
            {
                setupBuilder.LogFactory.ReconfigExistingLoggers();
            }
            else if (!configHasChanged || !ReferenceEquals(config, newConfig))
            {
                setupBuilder.LogFactory.Configuration = newConfig;
            }

            return setupBuilder;
        }

        /// <summary>
        /// Loads NLog config provided in <paramref name="loggingConfiguration"/>
        /// </summary>
        public static ISetupBuilder LoadConfiguration(this ISetupBuilder setupBuilder, LoggingConfiguration loggingConfiguration)
        {
            setupBuilder.LogFactory.Configuration = loggingConfiguration;
            return setupBuilder;
        }

        /// <summary>
        /// Loads NLog config from filename <paramref name="configFile"/> if provided, else fallback to scanning for NLog.config
        /// </summary>
        public static ISetupBuilder LoadConfigurationFromFile(this ISetupBuilder setupBuilder, string configFile = null, bool optional = true)
        {
            setupBuilder.LogFactory.LoadConfiguration(configFile, optional);
            return setupBuilder;
        }

        /// <summary>
        /// Loads NLog config from XML in <paramref name="configXml"/>
        /// </summary>
        public static ISetupBuilder LoadConfigurationFromXml(this ISetupBuilder setupBuilder, string configXml)
        {
            setupBuilder.LogFactory.Configuration = XmlLoggingConfiguration.CreateFromXmlString(configXml, setupBuilder.LogFactory);
            return setupBuilder;
        }
    }
}
