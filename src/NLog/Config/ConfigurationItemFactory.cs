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

namespace NLog.Config
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reflection;
    using NLog.Common;
    using NLog.Filters;
    using NLog.Internal;
    using NLog.LayoutRenderers;
    using NLog.Layouts;
    using NLog.Targets;
    using NLog.Time;

    /// <summary>
    /// Provides registration information for named items (targets, layouts, layout renderers, etc.)
    ///
    /// Supports creating item-instance from their type-alias, when parsing NLog configuration
    /// </summary>
    public sealed class ConfigurationItemFactory
    {
        private static ConfigurationItemFactory? _defaultInstance;

        internal static readonly object SyncRoot = new object();

        private readonly ServiceRepository _serviceRepository;
#pragma warning disable CS0618 // Type or member is obsolete
        internal IAssemblyExtensionLoader AssemblyLoader { get; } = new AssemblyExtensionLoader();
#pragma warning restore CS0618 // Type or member is obsolete
        private readonly IFactory[] _allFactories;
        private readonly Factory<Target, TargetAttribute> _targets;
        private readonly Factory<Filter, FilterAttribute> _filters;
        private readonly LayoutRendererFactory _layoutRenderers;
        private readonly Factory<Layout, LayoutAttribute> _layouts;
        private readonly MethodFactory _conditionMethods;
        private readonly Factory<LayoutRenderer, AmbientPropertyAttribute> _ambientProperties;
        private readonly Factory<TimeSource, TimeSourceAttribute> _timeSources;
        private readonly Dictionary<Type, ItemFactory> _itemFactories = new Dictionary<Type, ItemFactory>(256);

        private
#if !NETFRAMEWORK
        readonly
#endif
        struct ItemFactory
        {
            public readonly Func<Dictionary<string, PropertyInfo>> ItemProperties;
            public readonly Func<object?> ItemCreator;

            public ItemFactory(Func<Dictionary<string, PropertyInfo>> itemProperties, Func<object?> itemCreator)
            {
                ItemProperties = itemProperties;
                ItemCreator = itemCreator;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationItemFactory"/> class.
        /// </summary>
        public ConfigurationItemFactory()
            : this(LogManager.LogFactory.ServiceRepository)
        {
        }

        internal ConfigurationItemFactory(ServiceRepository serviceRepository)
        {
            _serviceRepository = Guard.ThrowIfNull(serviceRepository);
            _targets = new Factory<Target, TargetAttribute>(this);
            _filters = new Factory<Filter, FilterAttribute>(this);
            _layoutRenderers = new LayoutRendererFactory(this);
            _layouts = new Factory<Layout, LayoutAttribute>(this);
            _conditionMethods = new MethodFactory();
            _ambientProperties = new Factory<LayoutRenderer, AmbientPropertyAttribute>(this);
            _timeSources = new Factory<TimeSource, TimeSourceAttribute>(this);
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
            RegisterType<LoggingRule>();
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
            get => _defaultInstance ?? (_defaultInstance = new ConfigurationItemFactory(LogManager.LogFactory.ServiceRepository));
            set => _defaultInstance = value;
        }

        /// <summary>
        /// Gets the <see cref="Target"/> factory.
        /// </summary>
        public IFactory<Target> TargetFactory
        {
            get
            {
                if (!_targets.Initialized)
                {
                    _targets.Initialize(checkTypeExists => RegisterAllTargets(checkTypeExists));
                    // Targets can depend on filters
                    if (!_filters.Initialized)
                        _filters.Initialize(checkTypeExists => RegisterAllFilters(checkTypeExists));
                    // Targets can depend on conditions
                    if (!_conditionMethods.Initialized)
                        _conditionMethods.Initialize(checkTypeExists => RegisterAllConditionMethods(checkTypeExists));
                    // Targets can depend on layouts
                    if (!_layouts.Initialized)
                        _layouts.Initialize(checkTypeExists => RegisterAllLayouts(checkTypeExists));
                    // Targets can depend on layoutrenderers
                    if (!_layoutRenderers.Initialized)
                        _layoutRenderers.Initialize(checkTypeExists => RegisterAllLayoutRenderers(checkTypeExists));
                }
                return _targets;
            }
        }

        /// <summary>
        /// Gets the <see cref="Layout"/> factory.
        /// </summary>
        public IFactory<Layout> LayoutFactory
        {
            get
            {
                if (!_layouts.Initialized)
                {
                    _layouts.Initialize(checkTypeExists => RegisterAllLayouts(checkTypeExists));
                    // Layout can depend on layoutrenderers
                    if (!_layoutRenderers.Initialized)
                        _layoutRenderers.Initialize(checkTypeExists => RegisterAllLayoutRenderers(checkTypeExists));
                    // When-LayoutRenderers depends on conditions
                    if (!_conditionMethods.Initialized)
                        _conditionMethods.Initialize(checkTypeExists => RegisterAllConditionMethods(checkTypeExists));
                }
                return _layouts;
            }
        }

        /// <summary>
        /// Gets the <see cref="LayoutRenderer"/> factory.
        /// </summary>
        public IFactory<LayoutRenderer> LayoutRendererFactory
        {
            get
            {
                if (!_layoutRenderers.Initialized)
                    _layoutRenderers.Initialize(checkTypeExists => RegisterAllLayoutRenderers(checkTypeExists));
                // When-LayoutRenderers depends on conditions
                if (!_conditionMethods.Initialized)
                    _conditionMethods.Initialize(checkTypeExists => RegisterAllConditionMethods(checkTypeExists));
                return _layoutRenderers;
            }
        }

        /// <summary>
        /// Gets the ambient property factory.
        /// </summary>
        public IFactory<LayoutRenderer> AmbientRendererFactory
        {
            get
            {
                if (!_layoutRenderers.Initialized)
                    _layoutRenderers.Initialize(checkTypeExists => RegisterAllLayoutRenderers(checkTypeExists));
                // When-LayoutRenderers depends on conditions
                if (!_conditionMethods.Initialized)
                    _conditionMethods.Initialize(checkTypeExists => RegisterAllConditionMethods(checkTypeExists));
                return _ambientProperties;
            }
        }

        /// <summary>
        /// Gets the <see cref="Filter"/> factory.
        /// </summary>
        public IFactory<Filter> FilterFactory
        {
            get
            {
                if (!_filters.Initialized)
                    _filters.Initialize(checkTypeExists => RegisterAllFilters(checkTypeExists));
                // Filters can depend on conditions
                if (!_conditionMethods.Initialized)
                    _conditionMethods.Initialize(checkTypeExists => RegisterAllConditionMethods(checkTypeExists));
                return _filters;
            }
        }

        /// <summary>
        /// Gets the <see cref="TimeSource"/> factory.
        /// </summary>
        public IFactory<TimeSource> TimeSourceFactory
        {
            get
            {
                if (!_timeSources.Initialized)
                    _timeSources.Initialize(checkTypeExists => RegisterAllTimeSources(checkTypeExists));
                return _timeSources;
            }
        }

        internal MethodFactory ConditionMethodFactory
        {
            get
            {
                if (!_conditionMethods.Initialized)
                    _conditionMethods.Initialize(checkTypeExists => RegisterAllConditionMethods(checkTypeExists));
                return _conditionMethods;
            }
        }

        internal Factory<Target, TargetAttribute> GetTargetFactory() => _targets;
        internal Factory<Layout, LayoutAttribute> GetLayoutFactory() => _layouts;
        internal LayoutRendererFactory GetLayoutRendererFactory() => _layoutRenderers;
        internal Factory<LayoutRenderer, AmbientPropertyAttribute> GetAmbientPropertyFactory() => _ambientProperties;
        internal Factory<Filter, FilterAttribute> GetFilterFactory() => _filters;
        internal Factory<TimeSource, TimeSourceAttribute> GetTimeSourceFactory() => _timeSources;
        internal MethodFactory GetConditionMethodFactory() => _conditionMethods;

        internal ICollection<Type> GetRegisteredItemTypes()
        {
            lock (SyncRoot)
                return new List<Type>(_itemFactories.Keys);
        }

        /// <summary>
        /// Obsolete and replaced by <see cref="NLog.SetupSerializationBuilderExtensions.RegisterJsonConverter"/> with NLog v5.2.
        /// Gets or sets the JSON serializer to use with <see cref="JsonLayout"/>
        /// </summary>
        [Obsolete("Instead use NLog.LogManager.Setup().SetupSerialization(s => s.RegisterJsonConverter()) or ResolveService<IJsonConverter>(). Marked obsolete on NLog 5.0")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public IJsonConverter JsonConverter
        {
            get => _serviceRepository.GetService<IJsonConverter>();
            set => _serviceRepository.RegisterJsonConverter(value);
        }

        /// <summary>
        /// Perform message template parsing and formatting of LogEvent messages (True = Always, False = Never, Null = Auto Detect)
        /// </summary>
        /// <remarks>
        /// - <see langword="null"/> (Auto Detect) : NLog-parser checks <see cref="LogEventInfo.Message"/> for positional parameters, and will then fallback to string.Format-rendering.
        /// - <see langword="true"/>: Always performs the parsing of <see cref="LogEventInfo.Message"/> and rendering of <see cref="LogEventInfo.FormattedMessage"/> using the NLog-parser (Allows custom formatting with <see cref="IValueFormatter"/>)
        /// - <see langword="false"/>: Always performs parsing and rendering using string.Format (Fastest if not using structured logging)
        /// </remarks>
        public bool? ParseMessageTemplates
        {
            get => _serviceRepository.ResolveParseMessageTemplates();
            set => _serviceRepository.ParseMessageTemplates(LogManager.LogFactory, value);
        }

        /// <summary>
        /// Obsolete since dynamic assembly loading is not compatible with publish as trimmed application.
        /// Registers named items from the assembly.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        [Obsolete("Instead use NLog.LogManager.Setup().SetupExtensions(). Marked obsolete with NLog v5.2")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void RegisterItemsFromAssembly(Assembly assembly)
        {
            AssemblyLoader.LoadAssembly(this, assembly, string.Empty);
        }

        /// <summary>
        /// Obsolete since dynamic assembly loading is not compatible with publish as trimmed application.
        /// Registers named items from the assembly.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <param name="itemNamePrefix">Item name prefix.</param>
        [Obsolete("Instead use NLog.LogManager.Setup().SetupExtensions(). Marked obsolete with NLog v5.2")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void RegisterItemsFromAssembly(Assembly assembly, string itemNamePrefix)
        {
            AssemblyLoader.LoadAssembly(this, assembly, itemNamePrefix);
        }

        /// <summary>
        /// Clears the contents of all factories.
        /// </summary>
        public void Clear()
        {
            lock (SyncRoot)
            {
                foreach (IFactory f in _allFactories)
                {
                    f.Clear();
                }
            }
        }

        /// <summary>
        /// Obsolete since dynamic type loading is not compatible with publish as trimmed application.
        /// Registers the type.
        /// </summary>
        /// <param name="type">The type to register.</param>
        /// <param name="itemNamePrefix">The item name prefix.</param>
        [Obsolete("Instead use NLog.LogManager.Setup().SetupExtensions(). Marked obsolete with NLog v5.2")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void RegisterType([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.PublicMethods)] Type type, string? itemNamePrefix)
        {
            lock (SyncRoot)
            {
                foreach (IFactory f in _allFactories)
                {
                    f.RegisterType(type, itemNamePrefix ?? string.Empty);
                }
            }
        }

        internal void RegisterType<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.PublicMethods)] TType>() where TType : class, new()
        {
            lock (SyncRoot)
            {
                RegisterTypeProperties<TType>(() => new TType());

                foreach (IFactory f in _allFactories)
                {
                    f.RegisterType(typeof(TType), string.Empty);
                }
            }
        }

        internal void RegisterTypeProperties<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.PublicProperties)] TType>(Func<object?> itemCreator)
        {
            lock (SyncRoot)
            {
                if (!_itemFactories.ContainsKey(typeof(TType)))
                {
                    Dictionary<string, PropertyInfo>? properties = null;
                    var itemProperties = new Func<Dictionary<string, PropertyInfo>>(() => properties ?? (properties = typeof(TType).GetProperties(BindingFlags.Public | BindingFlags.Instance).ToDictionary(p => p.Name, StringComparer.OrdinalIgnoreCase)));
                    var itemFactory = new ItemFactory(itemProperties, itemCreator);
                    _itemFactories[typeof(TType)] = itemFactory;
                }
            }
        }

        internal void RegisterTypeProperties([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.PublicProperties)] Type itemType, Func<object?> itemCreator)
        {
            lock (SyncRoot)
            {
                if (!_itemFactories.ContainsKey(itemType))
                {
                    Dictionary<string, PropertyInfo>? properties = null;
                    var itemProperties = new Func<Dictionary<string, PropertyInfo>>(() => properties ?? (properties = itemType.GetProperties(BindingFlags.Public | BindingFlags.Instance).ToDictionary(p => p.Name, StringComparer.OrdinalIgnoreCase)));
                    var itemFactory = new ItemFactory(itemProperties, itemCreator);
                    _itemFactories[itemType] = itemFactory;
                }
            }
        }

        internal Dictionary<string, PropertyInfo> TryGetTypeProperties(Type itemType)
        {
            lock (SyncRoot)
            {
                if (_itemFactories.TryGetValue(itemType, out var itemFactory))
                {
                    return itemFactory.ItemProperties.Invoke();
                }
            }

            if (itemType.IsAbstract)
                return new Dictionary<string, PropertyInfo>();

            if (itemType.IsGenericType && itemType.GetGenericTypeDefinition() == typeof(Layout<>))
                return new Dictionary<string, PropertyInfo>();

#pragma warning disable CS0618 // Type or member is obsolete
            InternalLogger.Debug("Object reflection needed to configure instance of type: {0}", itemType);
            return ResolveTypePropertiesLegacy(itemType);
#pragma warning restore CS0618 // Type or member is obsolete
        }

        [UnconditionalSuppressMessage("Trimming - Ignore since obsolete", "IL2067")]
        [UnconditionalSuppressMessage("Trimming - Ignore since obsolete", "IL2070")]
        [UnconditionalSuppressMessage("Trimming - Ignore since obsolete", "IL2072")]
        [Obsolete("Instead use RegisterType<T>, as dynamic Assembly loading will be moved out. Marked obsolete with NLog v5.2")]
        private Dictionary<string, PropertyInfo> ResolveTypePropertiesLegacy(Type itemType)
        {
            Dictionary<string, PropertyInfo> properties = itemType.GetProperties(BindingFlags.Public | BindingFlags.Instance).ToDictionary(p => p.Name, StringComparer.OrdinalIgnoreCase);
            lock (SyncRoot)
            {
                _itemFactories[itemType] = new ItemFactory(() => properties, () => Activator.CreateInstance(itemType));
            }
            return properties;
        }

        internal bool TryCreateInstance(Type itemType, out object? instance)
        {
            Func<object?>? itemCreator = null;

            lock (SyncRoot)
            {
                if (_itemFactories.TryGetValue(itemType, out var itemFactory))
                    itemCreator = itemFactory.ItemCreator;
            }

            if (itemCreator is null)
            {
#pragma warning disable CS0618 // Type or member is obsolete
                InternalLogger.Debug("Object reflection needed to create instance of type: {0}", itemType);
                instance = ResolveCreateInstanceLegacy(itemType);
#pragma warning restore CS0618 // Type or member is obsolete
            }
            else
            {
                instance = itemCreator.Invoke();
            }

            return !(instance is null);
        }

        [UnconditionalSuppressMessage("Trimming - Ignore since obsolete", "IL2067")]
        [UnconditionalSuppressMessage("Trimming - Ignore since obsolete", "IL2070")]
        [UnconditionalSuppressMessage("Trimming - Ignore since obsolete", "IL2072")]
        [Obsolete("Instead use RegisterType<T>, as dynamic Assembly loading will be moved out. Marked obsolete with NLog v5.2")]
        private object? ResolveCreateInstanceLegacy(Type itemType)
        {
            Dictionary<string, PropertyInfo>? properties = null;
            var itemProperties = new Func<Dictionary<string, PropertyInfo>>(() => properties ?? (properties = itemType.GetProperties(BindingFlags.Public | BindingFlags.Instance).ToDictionary(p => p.Name, StringComparer.OrdinalIgnoreCase)));
            var itemFactory = new ItemFactory(itemProperties, () => Activator.CreateInstance(itemType));
            lock (SyncRoot)
            {
                _itemFactories[itemType] = itemFactory;
            }

            return itemFactory.ItemCreator.Invoke();
        }

        private void RegisterAllTargets(bool checkTypeExists)
        {
            AssemblyExtensionTypes.RegisterTargetTypes(this, checkTypeExists);
#pragma warning disable CS0618 // Type or member is obsolete
            _targets.RegisterNamedType("diagnosticlistener", "NLog.Targets.DiagnosticListenerTarget, NLog.DiagnosticSource", checkTypeExists);
            _targets.RegisterNamedType("database", "NLog.Targets.DatabaseTarget, NLog.Database", checkTypeExists);
            _targets.RegisterNamedType("atomfile", "NLog.Targets.AtomicFileTarget, NLog.Targets.AtomicFile", checkTypeExists);
            _targets.RegisterNamedType("atomicfile", "NLog.Targets.AtomicFileTarget, NLog.Targets.AtomicFile", checkTypeExists);
#if !NETFRAMEWORK
            _targets.RegisterNamedType("eventlog", "NLog.Targets.EventLogTarget, NLog.WindowsEventLog", checkTypeExists);
#endif
            _targets.RegisterNamedType("gzipfile", "NLog.Targets.GZipFileTarget, NLog.Targets.GZipFile", checkTypeExists);
            _targets.RegisterNamedType("impersonatingwrapper", "NLog.Targets.Wrappers.ImpersonatingTargetWrapper, NLog.WindowsIdentity", checkTypeExists);
            _targets.RegisterNamedType("logreceiverservice", "NLog.Targets.LogReceiverWebServiceTarget, NLog.Wcf", checkTypeExists);
            _targets.RegisterNamedType("outputdebugstring", "NLog.Targets.OutputDebugStringTarget, NLog.OutputDebugString", checkTypeExists);
            _targets.RegisterNamedType("network", "NLog.Targets.NetworkTarget, NLog.Targets.Network", checkTypeExists);
            _targets.RegisterNamedType("log4jxml", "NLog.Targets.Log4JXmlTarget, NLog.Targets.Network", checkTypeExists);
            _targets.RegisterNamedType("chainsaw", "NLog.Targets.Log4JXmlTarget, NLog.Targets.Network", checkTypeExists);
            _targets.RegisterNamedType("nlogviewer", "NLog.Targets.Log4JXmlTarget, NLog.Targets.Network", checkTypeExists);
            _targets.RegisterNamedType("syslog", "NLog.Targets.SyslogTarget, NLog.Targets.Network", checkTypeExists);
            _targets.RegisterNamedType("gelf", "NLog.Targets.GelfTarget, NLog.Targets.Network", checkTypeExists);
            _targets.RegisterNamedType("mail", "NLog.Targets.MailTarget, NLog.Targets.Mail", checkTypeExists);
            _targets.RegisterNamedType("email", "NLog.Targets.MailTarget, NLog.Targets.Mail", checkTypeExists);
            _targets.RegisterNamedType("smtp", "NLog.Targets.MailTarget, NLog.Targets.Mail", checkTypeExists);
            _targets.RegisterNamedType("mailkit", "NLog.MailKit.MailTarget, NLog.MailKit", checkTypeExists);
            _targets.RegisterNamedType("performancecounter", "NLog.Targets.PerformanceCounterTarget, NLog.PerformanceCounter", checkTypeExists);
            _targets.RegisterNamedType("richtextbox", "NLog.Windows.Forms.RichTextBoxTarget, NLog.Windows.Forms", checkTypeExists);
            _targets.RegisterNamedType("messagebox", "NLog.Windows.Forms.MessageBoxTarget, NLog.Windows.Forms", checkTypeExists);
            _targets.RegisterNamedType("formcontrol", "NLog.Windows.Forms.FormControlTarget, NLog.Windows.Forms", checkTypeExists);
            _targets.RegisterNamedType("toolstripitem", "NLog.Windows.Forms.ToolStripItemTarget, NLog.Windows.Forms", checkTypeExists);
            _targets.RegisterNamedType("trace", "NLog.Targets.TraceTarget, NLog.Targets.Trace", checkTypeExists);
            _targets.RegisterNamedType("tracesystem", "NLog.Targets.TraceTarget, NLog.Targets.Trace", checkTypeExists);
            _targets.RegisterNamedType("webservice", "NLog.Targets.WebServiceTarget, NLog.Targets.WebService", checkTypeExists);
#pragma warning restore CS0618 // Type or member is obsolete
        }

        private void RegisterAllLayouts(bool checkTypeExists)
        {
            AssemblyExtensionTypes.RegisterLayoutTypes(this, checkTypeExists);
#pragma warning disable CS0618 // Type or member is obsolete
#if !NET35 && !NET40
            _layouts.RegisterNamedType("microsoftconsolejsonlayout", "NLog.Extensions.Logging.MicrosoftConsoleJsonLayout, NLog.Extensions.Logging", checkTypeExists);
#endif
            _layouts.RegisterNamedType("sysloglayout", "NLog.Targets.SyslogLayout, NLog.Targets.Network", checkTypeExists);
            _layouts.RegisterNamedType("log4jxmllayout", "NLog.Targets.Log4JXmlEventLayout, NLog.Targets.Network", checkTypeExists);
            _layouts.RegisterNamedType("log4jxmleventlayout", "NLog.Targets.Log4JXmlEventLayout, NLog.Targets.Network", checkTypeExists);
            _layouts.RegisterNamedType("gelflayout", "NLog.Targets.GelfLayout, NLog.Targets.Network", checkTypeExists);
#pragma warning restore CS0618 // Type or member is obsolete
        }

        private void RegisterAllLayoutRenderers(bool checkTypeExists)
        {
            AssemblyExtensionTypes.RegisterLayoutRendererTypes(this, checkTypeExists);
#pragma warning disable CS0618 // Type or member is obsolete
#if !NET35 && !NET40
            _layoutRenderers.RegisterNamedType("configsetting", "NLog.Extensions.Logging.ConfigSettingLayoutRenderer, NLog.Extensions.Logging", checkTypeExists);
            _layoutRenderers.RegisterNamedType("microsoftconsolelayout", "NLog.Extensions.Logging.MicrosoftConsoleLayoutRenderer, NLog.Extensions.Logging", checkTypeExists);
            _layoutRenderers.RegisterNamedType("hostappname", "NLog.Extensions.Hosting.HostAppNameLayoutRenderer, NLog.Extensions.Hosting", checkTypeExists);
            _layoutRenderers.RegisterNamedType("hostenvironment", "NLog.Extensions.Hosting.HostEnvironmentLayoutRenderer, NLog.Extensions.Hosting", checkTypeExists);
            _layoutRenderers.RegisterNamedType("hostrootdir", "NLog.Extensions.Hosting.HostRootDirLayoutRenderer, NLog.Extensions.Hosting", checkTypeExists);
#endif
            _layoutRenderers.RegisterNamedType("localip", "NLog.LayoutRenderers.LocalIpAddressLayoutRenderer, NLog.Targets.Network", checkTypeExists);
            _layoutRenderers.RegisterNamedType("performancecounter", "NLog.LayoutRenderers.PerformanceCounterLayoutRenderer, NLog.PerformanceCounter", checkTypeExists);
            _layoutRenderers.RegisterNamedType("registry", "NLog.LayoutRenderers.RegistryLayoutRenderer, NLog.WindowsRegistry", checkTypeExists);
            _layoutRenderers.RegisterNamedType("regexreplace", "NLog.LayoutRenderers.RegexReplaceLayoutRendererWrapper, NLog.RegEx", checkTypeExists);
            _layoutRenderers.RegisterNamedType("windowsidentity", "NLog.LayoutRenderers.WindowsIdentityLayoutRenderer, NLog.WindowsIdentity", checkTypeExists);
            _layoutRenderers.RegisterNamedType("rtblink", "NLog.Windows.Forms.RichTextBoxLinkLayoutRenderer, NLog.Windows.Forms", checkTypeExists);
            _layoutRenderers.RegisterNamedType("activity", "NLog.LayoutRenderers.ActivityTraceLayoutRenderer, NLog.DiagnosticSource", checkTypeExists);
            _layoutRenderers.RegisterNamedType("activityid", "NLog.LayoutRenderers.TraceActivityIdLayoutRenderer, NLog.Targets.Trace", checkTypeExists);
#pragma warning restore CS0618 // Type or member is obsolete
        }

        private void RegisterAllFilters(bool checkTypeExists)
        {
            AssemblyExtensionTypes.RegisterFilterTypes(this, checkTypeExists);
        }

        private void RegisterAllConditionMethods(bool checkTypeExists)
        {
            AssemblyExtensionTypes.RegisterConditionTypes(this, checkTypeExists);
        }

        private void RegisterAllTimeSources(bool checkTypeExists)
        {
            AssemblyExtensionTypes.RegisterTimeSourceTypes(this, checkTypeExists);
        }
    }
}
