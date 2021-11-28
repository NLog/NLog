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

namespace NLog.Config
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using NLog.Common;
    using NLog.Conditions;
    using NLog.Filters;
    using NLog.Internal;
    using NLog.LayoutRenderers;
    using NLog.Layouts;
    using NLog.Targets;
    using NLog.Time;

    /// <summary>
    /// Provides registration information for named items (targets, layouts, layout renderers, etc.) managed by NLog.
    /// 
    /// Everything of an assembly could be loaded by <see cref="RegisterItemsFromAssembly(System.Reflection.Assembly)"/>
    /// </summary>
    public class ConfigurationItemFactory
    {
        private static ConfigurationItemFactory _defaultInstance;

        private readonly ServiceRepository _serviceRepository;
        private readonly IFactory[] _allFactories;
        private readonly Factory<Target, TargetAttribute> _targets;
        private readonly Factory<Filter, FilterAttribute> _filters;
        private readonly LayoutRendererFactory _layoutRenderers;
        private readonly Factory<Layout, LayoutAttribute> _layouts;
        private readonly MethodFactory _conditionMethods;
        private readonly Factory<LayoutRenderer, AmbientPropertyAttribute> _ambientProperties;
        private readonly Factory<TimeSource, TimeSourceAttribute> _timeSources;

        /// <summary>
        /// Called before the assembly will be loaded.
        /// </summary>
        public static event EventHandler<AssemblyLoadingEventArgs> AssemblyLoading;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationItemFactory"/> class.
        /// </summary>
        /// <param name="assemblies">The assemblies to scan for named items.</param>
        public ConfigurationItemFactory(params Assembly[] assemblies)
            :this(LogManager.LogFactory.ServiceRepository, null, assemblies)
        {
        }

        internal ConfigurationItemFactory(ServiceRepository serviceRepository, ConfigurationItemFactory globalDefaultFactory, params Assembly[] assemblies)
        {
            CreateInstance = FactoryHelper.CreateInstance;
            _serviceRepository = serviceRepository ?? throw new ArgumentNullException(nameof(serviceRepository));
            _targets = new Factory<Target, TargetAttribute>(this, globalDefaultFactory?._targets);
            _filters = new Factory<Filter, FilterAttribute>(this, globalDefaultFactory?._filters);
            _layoutRenderers = new LayoutRendererFactory(this, globalDefaultFactory?._layoutRenderers);
            _layouts = new Factory<Layout, LayoutAttribute>(this, globalDefaultFactory?._layouts);
            _conditionMethods = new MethodFactory(globalDefaultFactory?._conditionMethods, classType => MethodFactory.ExtractClassMethods<ConditionMethodsAttribute, ConditionMethodAttribute>(classType));
            _ambientProperties = new Factory<LayoutRenderer, AmbientPropertyAttribute>(this, globalDefaultFactory?._ambientProperties);
            _timeSources = new Factory<TimeSource, TimeSourceAttribute>(this, globalDefaultFactory?._timeSources);
            _allFactories = new IFactory[]
            {
                _targets,
                _filters,
                _layoutRenderers,
                _layouts,
                _conditionMethods,
                _ambientProperties,
                _timeSources,
            };

            foreach (var asm in assemblies)
            {
                RegisterItemsFromAssembly(asm);
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
            get => _defaultInstance ?? (_defaultInstance = BuildDefaultFactory());
            set => _defaultInstance = value;
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
        public INamedItemFactory<Target, Type> Targets => _targets;

        /// <summary>
        /// Gets the <see cref="Filter"/> factory.
        /// </summary>
        /// <value>The filter factory.</value>
        public INamedItemFactory<Filter, Type> Filters => _filters;

        /// <summary>
        /// gets the <see cref="LayoutRenderer"/> factory
        /// </summary>
        /// <remarks>not using <see cref="_layoutRenderers"/> due to backwards-compatibility.</remarks>
        /// <returns></returns>
        internal LayoutRendererFactory GetLayoutRenderers()
        {
            return _layoutRenderers;
        }

        /// <summary>
        /// Gets the <see cref="LayoutRenderer"/> factory.
        /// </summary>
        /// <value>The layout renderer factory.</value>
        public INamedItemFactory<LayoutRenderer, Type> LayoutRenderers => _layoutRenderers;

        /// <summary>
        /// Gets the <see cref="LayoutRenderer"/> factory.
        /// </summary>
        /// <value>The layout factory.</value>
        public INamedItemFactory<Layout, Type> Layouts => _layouts;

        /// <summary>
        /// Gets the ambient property factory.
        /// </summary>
        /// <value>The ambient property factory.</value>
        public INamedItemFactory<LayoutRenderer, Type> AmbientProperties => _ambientProperties;

        /// <summary>
        /// Gets or sets the JSON serializer to use with <see cref="JsonLayout"/>
        /// </summary>
        [Obsolete("Instead use LogFactory.ServiceRepository.ResolveInstance(typeof(IJsonConverter)). Marked obsolete on NLog 5.0")]
        public IJsonConverter JsonConverter
        {
            get => _serviceRepository.GetService<IJsonConverter>();
            set => _serviceRepository.RegisterJsonConverter(value);
        }

        /// <summary>
        /// Gets or sets the string serializer to use with <see cref="LogEventInfo.MessageTemplateParameters"/>
        /// </summary>
        [Obsolete("Instead use LogFactory.ServiceRepository.ResolveInstance(typeof(IValueFormatter)). Marked obsolete on NLog 5.0")]
        public IValueFormatter ValueFormatter
        {
            get => _serviceRepository.GetService<IValueFormatter>();
            set => _serviceRepository.RegisterValueFormatter(value);
        }

        /// <summary>
        /// Gets or sets the parameter converter to use with <see cref="TargetWithContext"/> or <see cref="Layout{T}"/>
        /// </summary>
        [Obsolete("Instead use LogFactory.ServiceRepository.ResolveInstance(typeof(IPropertyTypeConverter)). Marked obsolete on NLog 5.0")]
        public IPropertyTypeConverter PropertyTypeConverter
        {
            get => _serviceRepository.GetService<IPropertyTypeConverter>();
            set => _serviceRepository.RegisterPropertyTypeConverter(value);
        }

        /// <summary>
        /// Perform message template parsing and formatting of LogEvent messages (True = Always, False = Never, Null = Auto Detect)
        /// </summary>
        /// <remarks>
        /// - Null (Auto Detect) : NLog-parser checks <see cref="LogEventInfo.Message"/> for positional parameters, and will then fallback to string.Format-rendering.
        /// - True: Always performs the parsing of <see cref="LogEventInfo.Message"/> and rendering of <see cref="LogEventInfo.FormattedMessage"/> using the NLog-parser (Allows custom formatting with <see cref="ValueFormatter"/>)
        /// - False: Always performs parsing and rendering using string.Format (Fastest if not using structured logging)
        /// </remarks>
        public bool? ParseMessageTemplates
        {
            get => _serviceRepository.ResolveMessageTemplateParser();
            set => _serviceRepository.RegisterMessageTemplateParser(value);
        }

        /// <summary>
        /// Gets the time source factory.
        /// </summary>
        /// <value>The time source factory.</value>
        public INamedItemFactory<TimeSource, Type> TimeSources => _timeSources;

        /// <summary>
        /// Gets the condition method factory.
        /// </summary>
        /// <value>The condition method factory.</value>
        public INamedItemFactory<MethodInfo, MethodInfo> ConditionMethods => _conditionMethods;

        /// <summary>
        /// Gets the condition method factory (precompiled)
        /// </summary>
        /// <value>The condition method factory.</value>
        internal MethodFactory ConditionMethodDelegates => _conditionMethods;

        /// <summary>
        /// Registers named items from the assembly.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        public void RegisterItemsFromAssembly(Assembly assembly)
        {
            RegisterItemsFromAssembly(assembly, string.Empty);
        }

        /// <summary>
        /// Registers named items from the assembly.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <param name="itemNamePrefix">Item name prefix.</param>
        public void RegisterItemsFromAssembly(Assembly assembly, string itemNamePrefix)
        {
            if (AssemblyLoading != null)
            {
                var args = new AssemblyLoadingEventArgs(assembly);
                AssemblyLoading.Invoke(null, args);
                if (args.Cancel)
                {
                    InternalLogger.Info("Loading assembly '{0}' is canceled", assembly.FullName);
                    return;
                }
            }

            InternalLogger.Debug("ScanAssembly('{0}')", assembly.FullName);
            var typesToScan = assembly.SafeGetTypes();
            if (typesToScan?.Length > 0)
            {
                var assemblyName = new AssemblyName(assembly.FullName).Name;
                PreloadAssembly(typesToScan);
                foreach (IFactory f in _allFactories)
                {
                    f.ScanTypes(typesToScan, assemblyName, itemNamePrefix);
                }
            }
        }

        /// <summary>
        /// Call Preload for NLogPackageLoader
        /// </summary>
        /// <remarks>
        /// Every package could implement a class "NLogPackageLoader" (namespace not important) with the public static method "Preload" (no arguments)
        /// This method will be called just before registering all items in the assembly.
        /// </remarks>
        /// <param name="typesToScan"></param>
        public void PreloadAssembly(Type[] typesToScan)
        {
            var types = typesToScan.Where(t => t.Name.Equals("NLogPackageLoader", StringComparison.OrdinalIgnoreCase));

            foreach (var type in types)
            {
                CallPreload(type);
            }
        }

        /// <summary>
        /// Call the Preload method for <paramref name="type"/>. The Preload method must be static.
        /// </summary>
        /// <param name="type"></param>
        private void CallPreload(Type type)
        {
            if (type is null)
            {
                return;
            }

            InternalLogger.Debug("Found for preload'{0}'", type.FullName);
            var preloadMethod = type.GetMethod("Preload");
            if (preloadMethod != null)
            {
                if (preloadMethod.IsStatic)
                {
                    InternalLogger.Debug("NLogPackageLoader contains Preload method");

                    //only static, so first param null
                    try
                    {
                        var parameters = CreatePreloadParameters(preloadMethod, this);

                        preloadMethod.Invoke(null, parameters);
                        InternalLogger.Debug("Preload successfully invoked for '{0}'", type.FullName);
                    }
                    catch (Exception e)
                    {
                        InternalLogger.Warn(e, "Invoking Preload for '{0}' failed", type.FullName);
                    }
                }
                else
                {
                    InternalLogger.Debug("NLogPackageLoader contains a preload method, but isn't static");
                }
            }
            else
            {
                InternalLogger.Debug("{0} doesn't contain Preload method", type.FullName);
            }
        }

        private static object[] CreatePreloadParameters(MethodInfo preloadMethod, ConfigurationItemFactory configurationItemFactory)
        {
            var firstParam = preloadMethod.GetParameters().FirstOrDefault();
            object[] parameters = null;
            if (firstParam?.ParameterType == typeof(ConfigurationItemFactory))
            {
                parameters = new object[] {configurationItemFactory};
            }

            return parameters;
        }

        /// <summary>
        /// Clears the contents of all factories.
        /// </summary>
        public void Clear()
        {
            foreach (IFactory f in _allFactories)
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
            foreach (IFactory f in _allFactories)
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
            var nlogAssembly = typeof(LogFactory).GetAssembly();
            var factory = new ConfigurationItemFactory(LogManager.LogFactory.ServiceRepository, null, nlogAssembly);
            factory.RegisterExternalItems();
            return factory;
        }

        internal static void ScanForAutoLoadExtensions(LogFactory logFactory)
        {
#if !NETSTANDARD1_3
            try
            {
                var factory = ConfigurationItemFactory.Default;
                var nlogAssembly = typeof(LogFactory).GetAssembly();
                var assemblyLocation = string.Empty;
                var extensionDlls = ArrayHelper.Empty<string>();
                var fileLocations = GetAutoLoadingFileLocations();
                foreach (var fileLocation in fileLocations)
                {
                    if (string.IsNullOrEmpty(fileLocation.Key))
                        continue;

                    if (string.IsNullOrEmpty(assemblyLocation))
                        assemblyLocation = fileLocation.Key;

                    extensionDlls = GetNLogExtensionFiles(fileLocation.Key);
                    if (extensionDlls.Length > 0)
                    {
                        assemblyLocation = fileLocation.Key;
                        break;
                    }
                }

                InternalLogger.Debug("Start auto loading, location: {0}", assemblyLocation);
                var alreadyRegistered = LoadNLogExtensionAssemblies(factory, nlogAssembly, extensionDlls);
                RegisterAppDomainAssemblies(factory, nlogAssembly, alreadyRegistered);
            }
            catch (System.Security.SecurityException ex)
            {
                InternalLogger.Warn(ex, "Seems that we do not have permission");
                if (ex.MustBeRethrown())
                {
                    throw;
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                InternalLogger.Warn(ex, "Seems that we do not have permission");
                if (ex.MustBeRethrown())
                {
                    throw;
                }
            }
            InternalLogger.Debug("Auto loading done");
#else
            // Nothing to do for Sonar Cube
#endif
        }

#if !NETSTANDARD1_3
        private static HashSet<string> LoadNLogExtensionAssemblies(ConfigurationItemFactory factory, Assembly nlogAssembly, string[] extensionDlls)
        {
            HashSet<string> alreadyRegistered = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                {
                    nlogAssembly.FullName
                };

            foreach (var extensionDll in extensionDlls)
            {
                InternalLogger.Info("Auto loading assembly file: {0}", extensionDll);
                var success = false;
                try
                {
                    var extensionAssembly = AssemblyHelpers.LoadFromPath(extensionDll);
                    InternalLogger.LogAssemblyVersion(extensionAssembly);
                    factory.RegisterItemsFromAssembly(extensionAssembly);
                    alreadyRegistered.Add(extensionAssembly.FullName);
                    success = true;
                }
                catch (Exception ex)
                {
                    if (ex.MustBeRethrownImmediately())
                    {
                        throw;
                    }

                    InternalLogger.Warn(ex, "Auto loading assembly file: {0} failed! Skipping this file.", extensionDll);
                    if (ex.MustBeRethrown())
                    {
                        throw;
                    }
                }
                if (success)
                {
                    InternalLogger.Info("Auto loading assembly file: {0} succeeded!", extensionDll);
                }
            }

            return alreadyRegistered;
        }

        private static void RegisterAppDomainAssemblies(ConfigurationItemFactory factory, Assembly nlogAssembly, HashSet<string> alreadyRegistered)
        {
            alreadyRegistered.Add(nlogAssembly.FullName);

#if !NETSTANDARD1_5
            var allAssemblies = LogFactory.DefaultAppEnvironment.GetAppDomainRuntimeAssemblies();
#else
            var allAssemblies = new [] { nlogAssembly };
#endif
            foreach (var assembly in allAssemblies)
            {
                if (assembly.FullName.StartsWith("NLog.", StringComparison.OrdinalIgnoreCase) && !alreadyRegistered.Contains(assembly.FullName))
                {
                    factory.RegisterItemsFromAssembly(assembly);
                }

                if (IncludeAsHiddenAssembly(assembly.FullName))
                {
                    LogManager.AddHiddenAssembly(assembly);
                }
            }
        }

        private static bool IncludeAsHiddenAssembly(string assemblyFullName)
        {
            if (assemblyFullName.StartsWith("NLog.Extensions.Logging,", StringComparison.OrdinalIgnoreCase))
                return true;

            if (assemblyFullName.StartsWith("NLog.Web,", StringComparison.OrdinalIgnoreCase))
                return true;

            if (assemblyFullName.StartsWith("NLog.Web.AspNetCore,", StringComparison.OrdinalIgnoreCase))
                return true;

            if (assemblyFullName.StartsWith("Microsoft.Extensions.Logging,", StringComparison.OrdinalIgnoreCase))
                return true;

            if (assemblyFullName.StartsWith("Microsoft.Extensions.Logging.Abstractions,", StringComparison.OrdinalIgnoreCase))
                return true;

            if (assemblyFullName.StartsWith("Microsoft.Extensions.Logging.Filter,", StringComparison.OrdinalIgnoreCase))
                return true;

            if (assemblyFullName.StartsWith("Microsoft.Logging,", StringComparison.OrdinalIgnoreCase))
                return true;

            return false;
        }

        internal static IEnumerable<KeyValuePair<string, string>> GetAutoLoadingFileLocations()
        {
            var nlogAssembly = typeof(LogFactory).GetAssembly();
            var nlogAssemblyLocation = PathHelpers.TrimDirectorySeparators(AssemblyHelpers.GetAssemblyFileLocation(nlogAssembly));
            InternalLogger.Debug("Auto loading based on NLog-Assembly found location: {0}", nlogAssemblyLocation);
            if (!string.IsNullOrEmpty(nlogAssemblyLocation))
                yield return new KeyValuePair<string, string>(nlogAssemblyLocation, nameof(nlogAssemblyLocation));

            var entryAssemblyLocation = PathHelpers.TrimDirectorySeparators(LogFactory.DefaultAppEnvironment.EntryAssemblyLocation);
            InternalLogger.Debug("Auto loading based on GetEntryAssembly-Assembly found location: {0}", entryAssemblyLocation);
            if (!string.IsNullOrEmpty(entryAssemblyLocation) && !string.Equals(entryAssemblyLocation, nlogAssemblyLocation, StringComparison.OrdinalIgnoreCase))
                yield return new KeyValuePair<string, string>(entryAssemblyLocation, nameof(entryAssemblyLocation));

            // TODO Consider to prioritize AppDomain.PrivateBinPath
            var baseDirectory = PathHelpers.TrimDirectorySeparators(LogFactory.DefaultAppEnvironment.AppDomainBaseDirectory);
            InternalLogger.Debug("Auto loading based on AppDomain-BaseDirectory found location: {0}", baseDirectory);
            if (!string.IsNullOrEmpty(baseDirectory) && !string.Equals(baseDirectory, nlogAssemblyLocation, StringComparison.OrdinalIgnoreCase))
                yield return new KeyValuePair<string, string>(baseDirectory, nameof(baseDirectory));
        }

        private static string[] GetNLogExtensionFiles(string assemblyLocation)
        {
            try
            {
                InternalLogger.Debug("Search for auto loading files in location: {0}", assemblyLocation);
                if (string.IsNullOrEmpty(assemblyLocation))
                {
                    return ArrayHelper.Empty<string>();
                }

                var extensionDlls = Directory.GetFiles(assemblyLocation, "NLog*.dll")
                .Select(Path.GetFileName)
                .Where(x => !x.Equals("NLog.dll", StringComparison.OrdinalIgnoreCase))
                .Where(x => !x.Equals("NLog.UnitTests.dll", StringComparison.OrdinalIgnoreCase))
                .Select(x => Path.Combine(assemblyLocation, x));
                return extensionDlls.ToArray();
            }
            catch (DirectoryNotFoundException ex)
            {
                InternalLogger.Warn(ex, "Skipping auto loading location because assembly directory does not exist: {0}", assemblyLocation);
                if (ex.MustBeRethrown())
                {
                    throw;
                }
                return ArrayHelper.Empty<string>();
            }
            catch (System.Security.SecurityException ex)
            {
                InternalLogger.Warn(ex, "Skipping auto loading location because access not allowed to assembly directory: {0}", assemblyLocation);
                if (ex.MustBeRethrown())
                {
                    throw;
                }
                return ArrayHelper.Empty<string>();
            }
            catch (UnauthorizedAccessException ex)
            {
                InternalLogger.Warn(ex, "Skipping auto loading location because access not allowed to assembly directory: {0}", assemblyLocation);
                if (ex.MustBeRethrown())
                {
                    throw;
                }
                return ArrayHelper.Empty<string>();
            }
        }
#endif

        /// <summary>
        /// Registers items in using late-bound types, so that we don't need a reference to the dll.
        /// </summary>
        private void RegisterExternalItems()
        {
#if !NET35 && !NET40
            _layoutRenderers.RegisterNamedType("configsetting", "NLog.Extensions.Logging.ConfigSettingLayoutRenderer, NLog.Extensions.Logging");
            _layoutRenderers.RegisterNamedType("microsoftconsolelayout", "NLog.Extensions.Logging.MicrosoftConsoleLayoutRenderer, NLog.Extensions.Logging");
            _layoutRenderers.RegisterNamedType("microsoftconsolejsonlayout", "NLog.Extensions.Logging.MicrosoftConsoleJsonLayout, NLog.Extensions.Logging");
#endif
            _layoutRenderers.RegisterNamedType("performancecounter", "NLog.LayoutRenderers.PerformanceCounterLayoutRenderer, NLog.PerformanceCounter");
            _layoutRenderers.RegisterNamedType("registry", "NLog.LayoutRenderers.RegistryLayoutRenderer, NLog.WindowsRegistry");
            _layoutRenderers.RegisterNamedType("windows-identity", "NLog.LayoutRenderers.WindowsIdentityLayoutRenderer, NLog.WindowsIdentity");
            _targets.RegisterNamedType("database", "NLog.Targets.DatabaseTarget, NLog.Database");
#if NETSTANDARD
            _targets.RegisterNamedType("eventlog", "NLog.Targets.EventLogTarget, NLog.WindowsEventLog");
#endif
            _targets.RegisterNamedType("impersonatingwrapper", "NLog.Targets.Wrappers.ImpersonatingTargetWrapper, NLog.WindowsIdentity");
            _targets.RegisterNamedType("logreceiverservice", "NLog.Targets.LogReceiverWebServiceTarget, NLog.Wcf");
            _targets.RegisterNamedType("outputdebugstring", "NLog.Targets.OutputDebugStringTarget, NLog.OutputDebugString");
            _targets.RegisterNamedType("performancecounter", "NLog.Targets.PerformanceCounterTarget, NLog.PerformanceCounter");
        }
    }
}
