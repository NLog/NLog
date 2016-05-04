// 
// Copyright (c) 2004-2016 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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

using System.IO;
using System.Linq;
using NLog.Common;

namespace NLog.Config
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using NLog.Conditions;
    using NLog.Filters;
    using NLog.Internal;
    using NLog.LayoutRenderers;
    using NLog.Layouts;
    using NLog.Targets;
    using NLog.Time;

    /// <summary>
    /// Provides registration information for named items (targets, layouts, layout renderers, etc.) managed by NLog.
    /// </summary>
    public class ConfigurationItemFactory
    {
        private static ConfigurationItemFactory defaultInstance = null;

        private readonly IList<object> allFactories;
        private readonly Factory<Target, TargetAttribute> targets;
        private readonly Factory<Filter, FilterAttribute> filters;
        private readonly Factory<LayoutRenderer, LayoutRendererAttribute> layoutRenderers;
        private readonly Factory<Layout, LayoutAttribute> layouts;
        private readonly MethodFactory<ConditionMethodsAttribute, ConditionMethodAttribute> conditionMethods;
        private readonly Factory<LayoutRenderer, AmbientPropertyAttribute> ambientProperties;
        private readonly Factory<TimeSource, TimeSourceAttribute> timeSources;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationItemFactory"/> class.
        /// </summary>
        /// <param name="assemblies">The assemblies to scan for named items.</param>
        public ConfigurationItemFactory(params Assembly[] assemblies)
        {
            this.CreateInstance = FactoryHelper.CreateInstance;
            this.targets = new Factory<Target, TargetAttribute>(this);
            this.filters = new Factory<Filter, FilterAttribute>(this);
            this.layoutRenderers = new Factory<LayoutRenderer, LayoutRendererAttribute>(this);
            this.layouts = new Factory<Layout, LayoutAttribute>(this);
            this.conditionMethods = new MethodFactory<ConditionMethodsAttribute, ConditionMethodAttribute>();
            this.ambientProperties = new Factory<LayoutRenderer, AmbientPropertyAttribute>(this);
            this.timeSources = new Factory<TimeSource, TimeSourceAttribute>(this);
            this.allFactories = new List<object>
            {
                this.targets,
                this.filters,
                this.layoutRenderers,
                this.layouts,
                this.conditionMethods,
                this.ambientProperties,
                this.timeSources,
            };

            foreach (var asm in assemblies)
            {
                this.RegisterItemsFromAssembly(asm);
            }
        }

        /// <summary>
        /// Gets or sets default singleton instance of <see cref="ConfigurationItemFactory"/>.
        /// </summary>
        /// <remarks>
        /// This property implements lazy instantiation so that the <see cref="ConfigurationItemFactory"/> is not built before 
        /// the internal logger is configured.
        /// </remarks>
        public static ConfigurationItemFactory Default
        {
            get
            {
                if (defaultInstance == null)
                    defaultInstance = BuildDefaultFactory();
                return defaultInstance;
            }
            set { defaultInstance = value; }
        }

        /// <summary>
        /// Gets or sets the creator delegate used to instantiate configuration objects.
        /// </summary>
        /// <remarks>
        /// By overriding this property, one can enable dependency injection or interception for created objects.
        /// </remarks>
        public ConfigurationItemCreator CreateInstance { get; set; }

        /// <summary>
        /// Gets the <see cref="Target"/> factory.
        /// </summary>
        /// <value>The target factory.</value>
        public INamedItemFactory<Target, Type> Targets
        {
            get { return this.targets; }
        }

        /// <summary>
        /// Gets the <see cref="Filter"/> factory.
        /// </summary>
        /// <value>The filter factory.</value>
        public INamedItemFactory<Filter, Type> Filters
        {
            get { return this.filters; }
        }

        /// <summary>
        /// Gets the <see cref="LayoutRenderer"/> factory.
        /// </summary>
        /// <value>The layout renderer factory.</value>
        public INamedItemFactory<LayoutRenderer, Type> LayoutRenderers
        {
            get { return this.layoutRenderers; }
        }

        /// <summary>
        /// Gets the <see cref="LayoutRenderer"/> factory.
        /// </summary>
        /// <value>The layout factory.</value>
        public INamedItemFactory<Layout, Type> Layouts
        {
            get { return this.layouts; }
        }

        /// <summary>
        /// Gets the ambient property factory.
        /// </summary>
        /// <value>The ambient property factory.</value>
        public INamedItemFactory<LayoutRenderer, Type> AmbientProperties
        {
            get { return this.ambientProperties; }
        }

        /// <summary>
        /// Gets the time source factory.
        /// </summary>
        /// <value>The time source factory.</value>
        public INamedItemFactory<TimeSource, Type> TimeSources
        {
            get { return this.timeSources; }
        }

        /// <summary>
        /// Gets the condition method factory.
        /// </summary>
        /// <value>The condition method factory.</value>
        public INamedItemFactory<MethodInfo, MethodInfo> ConditionMethods
        {
            get { return this.conditionMethods; }
        }

        /// <summary>
        /// Registers named items from the assembly.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        public void RegisterItemsFromAssembly(Assembly assembly)
        {
            this.RegisterItemsFromAssembly(assembly, string.Empty);
        }

        /// <summary>
        /// Registers named items from the assembly.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <param name="itemNamePrefix">Item name prefix.</param>
        public void RegisterItemsFromAssembly(Assembly assembly, string itemNamePrefix)
        {
            InternalLogger.Debug("ScanAssembly('{0}')", assembly.FullName);
            var typesToScan = assembly.SafeGetTypes();
            foreach (IFactory f in this.allFactories)
            {
                f.ScanTypes(typesToScan, itemNamePrefix);
            }
        }

        /// <summary>
        /// Clears the contents of all factories.
        /// </summary>
        public void Clear()
        {
            foreach (IFactory f in this.allFactories)
            {
                f.Clear();
            }
        }

        /// <summary>
        /// Registers the type.
        /// </summary>
        /// <param name="type">The type to register.</param>
        /// <param name="itemNamePrefix">The item name prefix.</param>
        public void RegisterType(Type type, string itemNamePrefix)
        {
            foreach (IFactory f in this.allFactories)
            {
                f.RegisterType(type, itemNamePrefix);
            }
        }

        /// <summary>
        /// Builds the default configuration item factory.
        /// </summary>
        /// <returns>Default factory.</returns>
        private static ConfigurationItemFactory BuildDefaultFactory()
        {
            var nlogAssembly = typeof(ILogger).Assembly;
            var factory = new ConfigurationItemFactory(nlogAssembly);
            factory.RegisterExtendedItems();
#if !SILVERLIGHT

            var assemblyLocation = Path.GetDirectoryName(new Uri(nlogAssembly.CodeBase).LocalPath);
            if (assemblyLocation == null)
            {
                InternalLogger.Warn("No auto loading because Nlog.dll location is unknown");
                return factory;
            }
            if (!Directory.Exists(assemblyLocation))
            {
                InternalLogger.Warn("No auto loading because '{0}' doesn't exists", assemblyLocation);
                return factory;
            }

            var extensionDlls = Directory.GetFiles(assemblyLocation, "NLog*.dll")
                .Select(Path.GetFileName)
                .Where(x => !x.Equals("NLog.dll", StringComparison.OrdinalIgnoreCase))
                .Where(x => !x.Equals("NLog.UnitTests.dll", StringComparison.OrdinalIgnoreCase))
                .Where(x => !x.Equals("NLog.Extended.dll", StringComparison.OrdinalIgnoreCase))
                .Select(x => Path.Combine(assemblyLocation, x));

            InternalLogger.Debug("Start auto loading, location: {0}", assemblyLocation);
            foreach (var extensionDll in extensionDlls)
            {
                InternalLogger.Info("Auto loading assembly file: {0}", extensionDll);
                var success = false;
                try
                {
                    var extensionAssembly = Assembly.LoadFrom(extensionDll);
                    InternalLogger.LogAssemblyVersion(extensionAssembly);
                    factory.RegisterItemsFromAssembly(extensionAssembly);
                    success = true;
                }
                catch (Exception ex)
                {
                    if (ex.MustBeRethrownImmediately())
                    {
                        throw;
                    }

                    InternalLogger.Warn(ex, "Auto loading assembly file: {0} failed! Skipping this file.", extensionDll);
                    //TODO NLog 5, check MustBeRethrown()
                }
                if (success)
                {
                    InternalLogger.Info("Auto loading assembly file: {0} succeeded!", extensionDll);
                }

            }
            InternalLogger.Debug("Auto loading done");
#endif

            return factory;
        }

        /// <summary>
        /// Registers items in NLog.Extended.dll using late-bound types, so that we don't need a reference to NLog.Extended.dll.
        /// </summary>
        private void RegisterExtendedItems()
        {
            string suffix = typeof(ILogger).AssemblyQualifiedName;
            string myAssemblyName = "NLog,";
            string extendedAssemblyName = "NLog.Extended,";
            int p = suffix.IndexOf(myAssemblyName, StringComparison.OrdinalIgnoreCase);
            if (p >= 0)
            {
                suffix = ", " + extendedAssemblyName + suffix.Substring(p + myAssemblyName.Length);

                // register types
                string targetsNamespace = typeof(DebugTarget).Namespace;
                this.targets.RegisterNamedType("AspNetTrace", targetsNamespace + ".AspNetTraceTarget" + suffix);
                this.targets.RegisterNamedType("MSMQ", targetsNamespace + ".MessageQueueTarget" + suffix);
                this.targets.RegisterNamedType("AspNetBufferingWrapper", targetsNamespace + ".Wrappers.AspNetBufferingTargetWrapper" + suffix);

                // register layout renderers
                string layoutRenderersNamespace = typeof(MessageLayoutRenderer).Namespace;
                this.layoutRenderers.RegisterNamedType("appsetting", layoutRenderersNamespace + ".AppSettingLayoutRenderer" + suffix);
                this.layoutRenderers.RegisterNamedType("aspnet-application", layoutRenderersNamespace + ".AspNetApplicationValueLayoutRenderer" + suffix);
                this.layoutRenderers.RegisterNamedType("aspnet-request", layoutRenderersNamespace + ".AspNetRequestValueLayoutRenderer" + suffix);
                this.layoutRenderers.RegisterNamedType("aspnet-sessionid", layoutRenderersNamespace + ".AspNetSessionIDLayoutRenderer" + suffix);
                this.layoutRenderers.RegisterNamedType("aspnet-session", layoutRenderersNamespace + ".AspNetSessionValueLayoutRenderer" + suffix);
                this.layoutRenderers.RegisterNamedType("aspnet-user-authtype", layoutRenderersNamespace + ".AspNetUserAuthTypeLayoutRenderer" + suffix);
                this.layoutRenderers.RegisterNamedType("aspnet-user-identity", layoutRenderersNamespace + ".AspNetUserIdentityLayoutRenderer" + suffix);
            }
        }
    }
}
