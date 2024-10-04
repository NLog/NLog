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
    public class ConfigurationItemFactory
    {
        private static ConfigurationItemFactory _defaultInstance;

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

        private struct ItemFactory
        {
            public readonly Func<Dictionary<string, PropertyInfo>> ItemProperties;
            public readonly Func<object> ItemCreator;

            public ItemFactory(Func<Dictionary<string, PropertyInfo>> itemProperties, Func<object> itemCreator)
            {
                ItemProperties = itemProperties;
                ItemCreator = itemCreator;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationItemFactory"/> class.
        /// </summary>
        public ConfigurationItemFactory()
            : this(LogManager.LogFactory.ServiceRepository, null)
        {
        }

        internal ConfigurationItemFactory(ServiceRepository serviceRepository, ConfigurationItemFactory globalDefaultFactory)
        {
            _serviceRepository = Guard.ThrowIfNull(serviceRepository);
            _targets = new Factory<Target, TargetAttribute>(this);
            _filters = new Factory<Filter, FilterAttribute>(this);
            _layoutRenderers = new LayoutRendererFactory(this, globalDefaultFactory?._layoutRenderers);
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
        /// Gets the <see cref="Target"/> factory.
        /// </summary>
        public IFactory<Target> TargetFactory => _targets;
        /// <summary>
        /// Gets the <see cref="Layout"/> factory.
        /// </summary>
        public IFactory<Layout> LayoutFactory => _layouts;
        /// <summary>
        /// Gets the <see cref="LayoutRenderer"/> factory.
        /// </summary>
        public IFactory<LayoutRenderer> LayoutRendererFactory => _layoutRenderers;
        /// <summary>
        /// Gets the ambient property factory.
        /// </summary>
        public IFactory<LayoutRenderer> AmbientRendererFactory => _ambientProperties;
        /// <summary>
        /// Gets the <see cref="Filter"/> factory.
        /// </summary>
        public IFactory<Filter> FilterFactory => _filters;
        /// <summary>
        /// Gets the <see cref="TimeSource"/> factory.
        /// </summary>
        public IFactory<TimeSource> TimeSourceFactory => _timeSources;
        internal MethodFactory ConditionMethodFactory => _conditionMethods;

        internal Factory<Target, TargetAttribute> GetTargetFactory() => _targets;
        internal Factory<Layout, LayoutAttribute> GetLayoutFactory() => _layouts;
        internal LayoutRendererFactory GetLayoutRendererFactory() => _layoutRenderers;
        internal ICollection<Type> ItemTypes
        {
            get
            {
                lock (SyncRoot)
                    return new List<Type>(_itemFactories.Keys);
            }
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
        /// - Null (Auto Detect) : NLog-parser checks <see cref="LogEventInfo.Message"/> for positional parameters, and will then fallback to string.Format-rendering.
        /// - True: Always performs the parsing of <see cref="LogEventInfo.Message"/> and rendering of <see cref="LogEventInfo.FormattedMessage"/> using the NLog-parser (Allows custom formatting with <see cref="IValueFormatter"/>)
        /// - False: Always performs parsing and rendering using string.Format (Fastest if not using structured logging)
        /// </remarks>
        public bool? ParseMessageTemplates
        {
            get => _serviceRepository.ResolveParseMessageTemplates();
            set => _serviceRepository.ParseMessageTemplates(value);
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
        public void RegisterType([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.PublicMethods)] Type type, string itemNamePrefix)
        {
            lock (SyncRoot)
            {
                foreach (IFactory f in _allFactories)
                {
                    f.RegisterType(type, itemNamePrefix);
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

        internal void RegisterTypeProperties<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.PublicProperties)] TType>(Func<object> itemCreator)
        {
            lock (SyncRoot)
            {
                if (!_itemFactories.ContainsKey(typeof(TType)))
                {
                    Dictionary<string, PropertyInfo> properties = null;
                    var itemProperties = new Func<Dictionary<string, PropertyInfo>>(() => properties ?? (properties = typeof(TType).GetProperties(BindingFlags.Public | BindingFlags.Instance).ToDictionary(p => p.Name, StringComparer.OrdinalIgnoreCase)));
                    var itemFactory = new ItemFactory(itemProperties, itemCreator);
                    _itemFactories[typeof(TType)] = itemFactory;
                }
            }
        }

        internal void RegisterTypeProperties([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.PublicProperties)] Type itemType, Func<object> itemCreator)
        {
            lock (SyncRoot)
            {
                if (!_itemFactories.ContainsKey(itemType))
                {
                    Dictionary<string, PropertyInfo> properties = null;
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
            return ResolveTypePropertiesLegacy(itemType);
#pragma warning restore CS0618 // Type or member is obsolete
        }

        [UnconditionalSuppressMessage("Trimming - Ignore since obsolete", "IL2067")]
        [UnconditionalSuppressMessage("Trimming - Ignore since obsolete", "IL2070")]
        [UnconditionalSuppressMessage("Trimming - Ignore since obsolete", "IL2072")]
        [Obsolete("Instead use RegisterType<T>, as dynamic Assembly loading will be moved out. Marked obsolete with NLog v5.2")]
        private Dictionary<string, PropertyInfo> ResolveTypePropertiesLegacy(Type itemType)
        {
            InternalLogger.Debug("Object reflection needed to configure instance of type: {0}", itemType);
            Dictionary<string, PropertyInfo> properties = itemType.GetProperties(BindingFlags.Public | BindingFlags.Instance).ToDictionary(p => p.Name, StringComparer.OrdinalIgnoreCase);
            lock (SyncRoot)
            {
                _itemFactories[itemType] = new ItemFactory(() => properties, () => Activator.CreateInstance(itemType));
            }
            return properties;
        }

        internal bool TryCreateInstance(Type itemType, out object instance)
        {
            Func<object> itemCreator = null;

            lock (SyncRoot)
            {
                if (_itemFactories.TryGetValue(itemType, out var itemFactory))
                    itemCreator = itemFactory.ItemCreator;
            }

            if (itemCreator is null)
            {
#pragma warning disable CS0618 // Type or member is obsolete
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
        private object ResolveCreateInstanceLegacy(Type itemType)
        {
            InternalLogger.Debug("Object reflection needed to create instance of type: {0}", itemType);
            Dictionary<string, PropertyInfo> properties = null;
            var itemProperties = new Func<Dictionary<string, PropertyInfo>>(() => properties ?? (properties = itemType.GetProperties(BindingFlags.Public | BindingFlags.Instance).ToDictionary(p => p.Name, StringComparer.OrdinalIgnoreCase)));
            var itemFactory = new ItemFactory(itemProperties, () => Activator.CreateInstance(itemType));
            lock (SyncRoot)
            {
                _itemFactories[itemType] = itemFactory;
            }

            return itemFactory.ItemCreator.Invoke();
        }

        /// <summary>
        /// Builds the default configuration item factory.
        /// </summary>
        /// <returns>Default factory.</returns>
        private static ConfigurationItemFactory BuildDefaultFactory()
        {
            var factory = new ConfigurationItemFactory(LogManager.LogFactory.ServiceRepository, null);
            lock (SyncRoot)
            {
                AssemblyExtensionTypes.RegisterTypes(factory);
#pragma warning disable CS0618 // Type or member is obsolete
                factory.RegisterExternalItems();
#pragma warning restore CS0618 // Type or member is obsolete
            }
            return factory;
        }

        /// <summary>
        /// Registers items in using late-bound types, so that we don't need a reference to the dll.
        /// </summary>
        [Obsolete("Instead use RegisterType<T>, as dynamic Assembly loading will be moved out. Marked obsolete with NLog v5.2")]
        private void RegisterExternalItems()
        {
#if !NET35 && !NET40
            _layouts.RegisterNamedType("microsoftconsolejsonlayout", "NLog.Extensions.Logging.MicrosoftConsoleJsonLayout, NLog.Extensions.Logging");
            _layoutRenderers.RegisterNamedType("configsetting", "NLog.Extensions.Logging.ConfigSettingLayoutRenderer, NLog.Extensions.Logging");
            _layoutRenderers.RegisterNamedType("microsoftconsolelayout", "NLog.Extensions.Logging.MicrosoftConsoleLayoutRenderer, NLog.Extensions.Logging");
#endif
            _layoutRenderers.RegisterNamedType("performancecounter", "NLog.LayoutRenderers.PerformanceCounterLayoutRenderer, NLog.PerformanceCounter");
            _layoutRenderers.RegisterNamedType("registry", "NLog.LayoutRenderers.RegistryLayoutRenderer, NLog.WindowsRegistry");
            _layoutRenderers.RegisterNamedType("windows-identity", "NLog.LayoutRenderers.WindowsIdentityLayoutRenderer, NLog.WindowsIdentity");
            _layoutRenderers.RegisterNamedType("rtblink", "NLog.Windows.Forms.RichTextBoxLinkLayoutRenderer, NLog.Windows.Forms");
            _layoutRenderers.RegisterNamedType("activity", "NLog.LayoutRenderers.ActivityTraceLayoutRenderer, NLog.DiagnosticSource");
            _targets.RegisterNamedType("diagnosticlistener", "NLog.Targets.DiagnosticListenerTarget, NLog.DiagnosticSource");
            _targets.RegisterNamedType("database", "NLog.Targets.DatabaseTarget, NLog.Database");
#if NETSTANDARD
            _targets.RegisterNamedType("eventlog", "NLog.Targets.EventLogTarget, NLog.WindowsEventLog");
#endif
            _targets.RegisterNamedType("impersonatingwrapper", "NLog.Targets.Wrappers.ImpersonatingTargetWrapper, NLog.WindowsIdentity");
            _targets.RegisterNamedType("logreceiverservice", "NLog.Targets.LogReceiverWebServiceTarget, NLog.Wcf");
            _targets.RegisterNamedType("outputdebugstring", "NLog.Targets.OutputDebugStringTarget, NLog.OutputDebugString");
            _targets.RegisterNamedType("performancecounter", "NLog.Targets.PerformanceCounterTarget, NLog.PerformanceCounter");
            _targets.RegisterNamedType("richtextbox", "NLog.Windows.Forms.RichTextBoxTarget, NLog.Windows.Forms");
            _targets.RegisterNamedType("messagebox", "NLog.Windows.Forms.MessageBoxTarget, NLog.Windows.Forms");
            _targets.RegisterNamedType("formcontrol", "NLog.Windows.Forms.FormControlTarget, NLog.Windows.Forms");
            _targets.RegisterNamedType("toolstripitem", "NLog.Windows.Forms.ToolStripItemTarget, NLog.Windows.Forms");
        }
    }
}
