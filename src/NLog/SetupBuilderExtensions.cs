// 
// Copyright (c) 2004-2021 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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
    using System.Collections.Generic;
    using System.Linq;
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
        /// Configures general options for NLog LogFactory before loading NLog config
        /// </summary>
        public static ISetupBuilder SetupLogFactory(this ISetupBuilder setupBuilder, Action<ISetupLogFactoryBuilder> logfactoryBuilder)
        {
            logfactoryBuilder(new SetupLogFactoryBuilder(setupBuilder.LogFactory));
            return setupBuilder;
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
                // New config has already been assigned or unchanged, check if refresh is needed
                if (!ReferenceEquals(config, newConfig) || !ReferenceEquals(newConfig, null))
                {
                    setupBuilder.LogFactory.ReconfigExistingLoggers();
                }
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
        /// <param name="setupBuilder">Fluent interface parameter.</param>
        /// <param name="configFile">Explicit configuration file to be read (Default NLog.config from candidates paths)</param>
        /// <param name="optional">Whether to allow application to run when NLog config is not available</param>
        public static ISetupBuilder LoadConfigurationFromFile(this ISetupBuilder setupBuilder, string configFile = null, bool optional = true)
        {
            setupBuilder.LogFactory.LoadConfiguration(configFile, optional);
            return setupBuilder;
        }

        /// <summary>
        /// Loads NLog config from file-paths <paramref name="candidateFilePaths"/> if provided, else fallback to scanning for NLog.config
        /// </summary>
        /// <param name="setupBuilder">Fluent interface parameter.</param>
        /// <param name="candidateFilePaths">Candidates file paths (including filename) where to scan for NLog config files</param>
        /// <param name="optional">Whether to allow application to run when NLog config is not available</param>
        public static ISetupBuilder LoadConfigurationFromFile(this ISetupBuilder setupBuilder, IEnumerable<string> candidateFilePaths, bool optional = true)
        {
#pragma warning disable CS0618 // Type or member is obsolete
            if (candidateFilePaths is null)
            {
                setupBuilder.LogFactory.ResetCandidateConfigFilePath();
            }
            else if (optional)
            {
                var originalFilePaths = setupBuilder.LogFactory.GetCandidateConfigFilePaths();
                var uniqueFilePaths = new HashSet<string>(candidateFilePaths, StringComparer.OrdinalIgnoreCase);
                var orderedFilePaths = new List<string>(candidateFilePaths);
                foreach (var filePath in originalFilePaths)
                {
                    if (uniqueFilePaths.Add(filePath))
                        orderedFilePaths.Add(filePath);
                }
                setupBuilder.LogFactory.SetCandidateConfigFilePaths(orderedFilePaths);
            }
            else
            {
                setupBuilder.LogFactory.SetCandidateConfigFilePaths(candidateFilePaths);
            }
#pragma warning restore CS0618 // Type or member is obsolete
            return setupBuilder.LoadConfigurationFromFile(default(string), optional);
        }

        /// <summary>
        /// Loads NLog config from XML in <paramref name="configXml"/>
        /// </summary>
        public static ISetupBuilder LoadConfigurationFromXml(this ISetupBuilder setupBuilder, string configXml)
        {
            setupBuilder.LogFactory.Configuration = XmlLoggingConfiguration.CreateFromXmlString(configXml, setupBuilder.LogFactory);
            return setupBuilder;
        }

        /// <summary>
        /// Loads NLog config located in embedded resource from main application assembly.
        /// </summary>
        /// <param name="setupBuilder">Fluent interface parameter.</param>
        /// <param name="applicationAssembly">Assembly for the main Application project with embedded resource</param>
        /// <param name="resourceName">Name of the manifest resource for NLog config XML</param>
        public static ISetupBuilder LoadConfigurationFromAssemblyResource(this ISetupBuilder setupBuilder, System.Reflection.Assembly applicationAssembly, string resourceName = "NLog.config")
        {
            Guard.ThrowIfNull(applicationAssembly);
            Guard.ThrowIfNullOrEmpty(resourceName);

            var resourcePaths = applicationAssembly.GetManifestResourceNames().Where(x => x.EndsWith(resourceName, StringComparison.OrdinalIgnoreCase)).ToList();
            if (resourcePaths.Count == 1)
            {
                var nlogConfigStream = applicationAssembly.GetManifestResourceStream(resourcePaths[0]);
                if (nlogConfigStream?.Length > 0)
                {
                    NLog.Common.InternalLogger.Info("Loading NLog XML config from assembly embedded resource '{0}'", resourceName);
                    using (var xmlReader = System.Xml.XmlReader.Create(nlogConfigStream))
                    {
                        setupBuilder.LoadConfiguration(new XmlLoggingConfiguration(xmlReader, null, setupBuilder.LogFactory));
                    }
                }
                else
                {
                    NLog.Common.InternalLogger.Debug("No NLog config loaded. Empty Embedded resource '{0}' found in assembly: {1}", resourceName, applicationAssembly.FullName);
                }
            }
            else if (resourcePaths.Count == 0)
            {
                NLog.Common.InternalLogger.Debug("No NLog config loaded. No matching embedded resource '{0}' found in assembly: {1}", resourceName, applicationAssembly.FullName);
            }
            else
            {
                NLog.Common.InternalLogger.Error("No NLog config loaded. Multiple matching embedded resource '{0}' found in assembly: {1}", resourceName, applicationAssembly.FullName);
            }
            return setupBuilder;
        }

        /// <summary>
        /// Reloads the current logging configuration and activates it
        /// </summary>
        /// <remarks>Logevents produced during the configuration-reload can become lost, as targets are unavailable while closing and initializing.</remarks>
        public static ISetupBuilder ReloadConfiguration(this ISetupBuilder setupBuilder)
        {
            var newConfig = setupBuilder.LogFactory._config?.Reload();
            if (newConfig is null || (newConfig as IInitializeSucceeded)?.InitializeSucceeded == false)
                return setupBuilder;

            setupBuilder.LogFactory.Configuration = newConfig;
            return setupBuilder;
        }
    }
}
