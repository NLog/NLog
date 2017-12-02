// 
// Copyright (c) 2004-2017 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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
    /// 
    /// Everything of an assembly could be loaded by <see cref="RegisterItemsFromAssembly(System.Reflection.Assembly)"/>
    /// </summary>
    public class ConfigurationItemFactory
    {
        private static ConfigurationItemFactory defaultInstance = null;

        private readonly IList<object> _allFactories;
        private readonly Factory<Target, TargetAttribute> _targets;
        private readonly Factory<Filter, FilterAttribute> _filters;
        private readonly LayoutRendererFactory _layoutRenderers;
        private readonly Factory<Layout, LayoutAttribute> _layouts;
        private readonly MethodFactory<ConditionMethodsAttribute, ConditionMethodAttribute> _conditionMethods;
        private readonly Factory<LayoutRenderer, AmbientPropertyAttribute> _ambientProperties;
        private readonly Factory<TimeSource, TimeSourceAttribute> _timeSources;

        private IJsonConverter _jsonSerializer = DefaultJsonSerializer.Instance;

        /// <summary>
        /// Called before the assembly will be loaded.
        /// </summary>
        public static event EventHandler<AssemblyLoadingEventArgs> AssemblyLoading;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationItemFactory"/> class.
        /// </summary>
        /// <param name="assemblies">The assemblies to scan for named items.</param>
        public ConfigurationItemFactory(params Assembly[] assemblies)
        {
            CreateInstance = FactoryHelper.CreateInstance;
            _targets = new Factory<Target, TargetAttribute>(this);
            _filters = new Factory<Filter, FilterAttribute>(this);
            _layoutRenderers = new LayoutRendererFactory(this);
            _layouts = new Factory<Layout, LayoutAttribute>(this);
            _conditionMethods = new MethodFactory<ConditionMethodsAttribute, ConditionMethodAttribute>();
            _ambientProperties = new Factory<LayoutRenderer, AmbientPropertyAttribute>(this);
            _timeSources = new Factory<TimeSource, TimeSourceAttribute>(this);
            _allFactories = new List<object>
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
            get
            {
                if (defaultInstance == null)
                    defaultInstance = BuildDefaultFactory();
                return defaultInstance;
            }
            set => defaultInstance = value;
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
        /// <remarks>not using <see cref="_layoutRenderers"/> due to backwardscomp.</remarks>
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
        /// Legacy interface, no longer used by the NLog engine
        /// </summary>
        [Obsolete("Use JsonConverter property instead. Marked obsolete on NLog 4.5")]
        public IJsonSerializer JsonSerializer
        {
            get => _jsonSerializer as IJsonSerializer;
            set => _jsonSerializer = value != null ? (IJsonConverter)new JsonConverterLegacy(value) : DefaultJsonSerializer.Instance;
        }

        /// <summary>
        /// Gets or sets the JSON serializer to use with <see cref="WebServiceTarget"/> or <see cref="JsonLayout"/>
        /// </summary>
        public IJsonConverter JsonConverter
        {
            get => _jsonSerializer;
            set => _jsonSerializer = value ?? DefaultJsonSerializer.Instance;
        }

        /// <summary>
        /// Gets or sets the string serializer to use with <see cref="LogEventInfo.MessageTemplateParameters"/>
        /// </summary>
        public IValueSerializer ValueSerializer
        {
            get => MessageTemplates.ValueSerializer.Instance;
            set => MessageTemplates.ValueSerializer.Instance = value;
        }

        /// <summary>
        /// Perform mesage template parsing and formatting of LogEvent messages (True = Always, False = Never, Null = Auto Detect)
        /// </summary>
        /// <remarks>
        /// - Null (Auto Detect) : NLog-parser checks <see cref="LogEventInfo.Message"/> for positional parameters, and will then fallback to string.Format-rendering.
        /// - True: Always performs the parsing of <see cref="LogEventInfo.Message"/> and rendering of <see cref="LogEventInfo.FormattedMessage"/> using the NLog-parser (Allows custom formatting with <see cref="ValueSerializer"/>)
        /// - False: Always performs parsing and rendering using string.Format (Fastest if not using structured logging)
        /// </remarks>
        public bool? ParseMessageTemplates
        {
            get
            {
                if (ReferenceEquals(LogEventInfo.DefaultMessageFormatter, LogEventInfo.StringFormatMessageFormatter))
                {
                    return false;
                }
                else if (ReferenceEquals(LogEventInfo.DefaultMessageFormatter, LogMessageTemplateFormatter.Default.MessageFormatter))
                {
                    return true;
                }
                else
                {
                    return null;
                }
            }
            set => LogEventInfo.SetDefaultMessageFormatter(value);
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
                AssemblyLoading.Invoke(this,args);
                if (args.Cancel)
                {
                    InternalLogger.Info("Loading assembly '{0}' is canceled", assembly.FullName);
                    return;
                }
            }

            InternalLogger.Debug("ScanAssembly('{0}')", assembly.FullName);
            var typesToScan = assembly.SafeGetTypes();
            PreloadAssembly(typesToScan);
            foreach (IFactory f in _allFactories)
            {
                f.ScanTypes(typesToScan, itemNamePrefix);
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
        private static void CallPreload(Type type)
        {
            if (type != null)
            {
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
                            preloadMethod.Invoke(null, null);
                            InternalLogger.Debug("Preload succesfully invoked for '{0}'", type.FullName);
                        }
                        catch (Exception e)
                        {
                            InternalLogger.Warn(e,"Invoking Preload for '{0}' failed", type.FullName);
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
            var nlogAssembly = typeof(ILogger).GetAssembly();
            var factory = new ConfigurationItemFactory(nlogAssembly);
            factory.RegisterExtendedItems();

#if !SILVERLIGHT && !WINDOWS_UWP
            try
            {
                var assemblyLocation = GetAssemblyFileLocation(nlogAssembly);
                var extensionDlls = GetNLogExtensionFiles(assemblyLocation);
                if (extensionDlls.Length==0)
                {
                    var entryAssembly = Assembly.GetEntryAssembly();
                    if (!string.IsNullOrEmpty(entryAssembly?.CodeBase))
                    {
                        if (!string.Equals(entryAssembly?.CodeBase, nlogAssembly.CodeBase, StringComparison.OrdinalIgnoreCase))
                        {
                            assemblyLocation = GetAssemblyFileLocation(entryAssembly);
                            extensionDlls = GetNLogExtensionFiles(assemblyLocation);
                        }
                    }
                    else
                    {
                        // TODO Consider to prioritize AppDomain.PrivateBinPath
                        var appDomainBaseDirectory = LogFactory.CurrentAppDomain.BaseDirectory;
                        if (!string.IsNullOrEmpty(appDomainBaseDirectory))
                        {
                            if (!string.Equals(appDomainBaseDirectory, assemblyLocation, StringComparison.OrdinalIgnoreCase))
                            {
                                assemblyLocation = appDomainBaseDirectory;
                                extensionDlls = GetNLogExtensionFiles(appDomainBaseDirectory);
                            }
                        }
                    }
                }

                InternalLogger.Debug("Start auto loading, location: {0}", assemblyLocation);
                foreach (var extensionDll in extensionDlls)
                {
                    InternalLogger.Info("Auto loading assembly file: {0}", extensionDll);
                    var success = false;
                    try
                    {
                        var extensionAssembly = AssemblyHelpers.LoadFromPath(extensionDll);
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
#endif
            return factory;
        }

#if !SILVERLIGHT && !WINDOWS_UWP
        private static string GetAssemblyFileLocation(Assembly assembly)
        {
            try
            {
                Uri assemblyCodeBase;
                if (!Uri.TryCreate(assembly.CodeBase, UriKind.RelativeOrAbsolute, out assemblyCodeBase))
                {
                    InternalLogger.Warn("No auto loading because assembly code base is unknown");
                    return string.Empty;
                }

                var assemblyLocation = Path.GetDirectoryName(assemblyCodeBase.LocalPath);
                if (assemblyLocation == null)
                {
                    InternalLogger.Warn("No auto loading because Nlog.dll location is unknown");
                    return string.Empty;
                }
                if (!Directory.Exists(assemblyLocation))
                {
                    InternalLogger.Warn("No auto loading because '{0}' doesn't exists", assemblyLocation);
                    return string.Empty;
                }

                return assemblyLocation;
            }
            catch (System.Security.SecurityException ex)
            {
                InternalLogger.Warn(ex, "Seems that we do not have permission");
                if (ex.MustBeRethrown())
                {
                    throw;
                }
                return string.Empty;
            }
            catch (UnauthorizedAccessException ex)
            {
                InternalLogger.Warn(ex, "Seems that we do not have permission");
                if (ex.MustBeRethrown())
                {
                    throw;
                }
                return string.Empty;
            }
        }

        private static string[] GetNLogExtensionFiles(string assemblyLocation)
        {
            try
            {
                if (string.IsNullOrEmpty(assemblyLocation))
                {
                    return ArrayHelper.Empty<string>();
                }

                InternalLogger.Debug("Search for auto loading files, location: {0}", assemblyLocation);
                var extensionDlls = Directory.GetFiles(assemblyLocation, "NLog*.dll")
                .Select(Path.GetFileName)
                .Where(x => !x.Equals("NLog.dll", StringComparison.OrdinalIgnoreCase))
                .Where(x => !x.Equals("NLog.UnitTests.dll", StringComparison.OrdinalIgnoreCase))
                .Where(x => !x.Equals("NLog.Extended.dll", StringComparison.OrdinalIgnoreCase))
                .Select(x => Path.Combine(assemblyLocation, x));
                return extensionDlls.ToArray();
            }
            catch (System.Security.SecurityException ex)
            {
                InternalLogger.Warn(ex, "Seems that we do not have permission");
                if (ex.MustBeRethrown())
                {
                    throw;
                }
                return ArrayHelper.Empty<string>();
            }
            catch (UnauthorizedAccessException ex)
            {
                InternalLogger.Warn(ex, "Seems that we do not have permission");
                if (ex.MustBeRethrown())
                {
                    throw;
                }
                return ArrayHelper.Empty<string>();
            }
        }
#endif

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
                _targets.RegisterNamedType("AspNetTrace", targetsNamespace + ".AspNetTraceTarget" + suffix);
                _targets.RegisterNamedType("MSMQ", targetsNamespace + ".MessageQueueTarget" + suffix);
                _targets.RegisterNamedType("AspNetBufferingWrapper", targetsNamespace + ".Wrappers.AspNetBufferingTargetWrapper" + suffix);

                // register layout renderers
                string layoutRenderersNamespace = typeof(MessageLayoutRenderer).Namespace;
                _layoutRenderers.RegisterNamedType("appsetting", layoutRenderersNamespace + ".AppSettingLayoutRenderer" + suffix);
                _layoutRenderers.RegisterNamedType("aspnet-application", layoutRenderersNamespace + ".AspNetApplicationValueLayoutRenderer" + suffix);
                _layoutRenderers.RegisterNamedType("aspnet-request", layoutRenderersNamespace + ".AspNetRequestValueLayoutRenderer" + suffix);
                _layoutRenderers.RegisterNamedType("aspnet-sessionid", layoutRenderersNamespace + ".AspNetSessionIDLayoutRenderer" + suffix);
                _layoutRenderers.RegisterNamedType("aspnet-session", layoutRenderersNamespace + ".AspNetSessionValueLayoutRenderer" + suffix);
                _layoutRenderers.RegisterNamedType("aspnet-user-authtype", layoutRenderersNamespace + ".AspNetUserAuthTypeLayoutRenderer" + suffix);
                _layoutRenderers.RegisterNamedType("aspnet-user-identity", layoutRenderersNamespace + ".AspNetUserIdentityLayoutRenderer" + suffix);
            }
        }
    }
}
