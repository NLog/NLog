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

namespace NLog.Targets
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Text;
    using NLog.Config;
    using NLog.Internal;
    using NLog.Layouts;

    /// <summary>
    /// Represents target that supports context capture using MDLC, MDC, NDLC and NDC
    /// </summary>
    public abstract class TargetWithContext : TargetWithLayout, IIncludeContext
    {
        /// <inheritdoc/>
        /// <docgen category='Layout Options' order='1' />
        public sealed override Layout Layout
        {
            get => _contextLayout;
            set
            {
                if (_contextLayout != null)
                    _contextLayout.TargetLayout = value;
                else
                    _contextLayout = new TargetWithContextLayout(this, value);
            }
        }
        private TargetWithContextLayout _contextLayout;

        /// <inheritdoc/>
        bool IIncludeContext.IncludeAllProperties { get => IncludeEventProperties; set => IncludeEventProperties = value; }

        /// <docgen category='Layout Options' order='10' />
        public bool IncludeEventProperties { get => _contextLayout.IncludeAllProperties; set => _contextLayout.IncludeAllProperties = value; }

        /// <inheritdoc/>
        /// <docgen category='Layout Options' order='10' />
        public bool IncludeMdc { get => _contextLayout.IncludeMdc; set => _contextLayout.IncludeMdc = value; }

        /// <inheritdoc/>
        /// <docgen category='Layout Options' order='10' />
        public bool IncludeNdc { get => _contextLayout.IncludeNdc; set => _contextLayout.IncludeNdc = value; }

#if !SILVERLIGHT
        /// <inheritdoc/>
        /// <docgen category='Layout Options' order='10' />
        public bool IncludeMdlc { get => _contextLayout.IncludeMdlc; set => _contextLayout.IncludeMdlc = value; }

        /// <inheritdoc/>
        /// <docgen category='Layout Options' order='10' />
        public bool IncludeNdlc { get => _contextLayout.IncludeNdlc; set => _contextLayout.IncludeNdlc = value; }
#endif

        /// <summary>
        /// Gets or sets a value indicating whether to include contents of the <see cref="GlobalDiagnosticsContext"/> dictionary
        /// </summary>
        /// <docgen category='Layout Options' order='10' />
        public bool IncludeGdc { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to include call site (class and method name) in the <see cref="LogEventInfo" />
        /// </summary>
        /// <docgen category='Layout Options' order='10' />
        public bool IncludeCallSite { get => _contextLayout.IncludeCallSite; set => _contextLayout.IncludeCallSite = value; }

        /// <summary>
        /// Gets or sets a value indicating whether to include source info (file name and line number) in the <see cref="LogEventInfo" />
        /// </summary>
        /// <docgen category='Layout Options' order='10' />
        public bool IncludeCallSiteStackTrace { get => _contextLayout.IncludeCallSiteStackTrace; set => _contextLayout.IncludeCallSiteStackTrace = value; }

        /// <summary>
        /// Gets the array of custom attributes to be passed into the logevent context
        /// </summary>
        /// <docgen category='Layout Options' order='10' />
        [ArrayParameter(typeof(TargetPropertyWithContext), "contextproperty")]
        public virtual IList<TargetPropertyWithContext> ContextProperties { get; } = new List<TargetPropertyWithContext>();

        private IPropertyTypeConverter PropertyTypeConverter
        {
            get => _propertyTypeConverter ?? (_propertyTypeConverter = ConfigurationItemFactory.Default.PropertyTypeConverter);
            set => _propertyTypeConverter = value;
        }
        private IPropertyTypeConverter _propertyTypeConverter;

        /// <summary>
        /// Constructor
        /// </summary>
        protected TargetWithContext()
        {
            _contextLayout = _contextLayout ?? new TargetWithContextLayout(this, base.Layout);
            OptimizeBufferReuse = true;
        }

        /// <inheritdoc/>
        protected override void CloseTarget()
        {
            PropertyTypeConverter = null;
            base.CloseTarget();
        }

        /// <summary>
        /// Check if logevent has properties (or context properties)
        /// </summary>
        /// <param name="logEvent"></param>
        /// <returns>True if properties should be included</returns>
        protected bool ShouldIncludeProperties(LogEventInfo logEvent)
        {
            return IncludeGdc
            || IncludeMdc
#if !SILVERLIGHT
            || IncludeMdlc
#endif
            || (IncludeEventProperties && (logEvent?.HasProperties ?? false));
        }

        /// <summary>
        /// Checks if any context properties, and if any returns them as a single dictionary
        /// </summary>
        /// <param name="logEvent"></param>
        /// <returns>Dictionary with any context properties for the logEvent (Null if none found)</returns>
        protected IDictionary<string, object> GetContextProperties(LogEventInfo logEvent)
        {
            return GetContextProperties(logEvent, null);
        }

        /// <summary>
        /// Checks if any context properties, and if any returns them as a single dictionary
        /// </summary>
        /// <param name="logEvent"></param>
        /// <param name="combinedProperties">Optional prefilled dictionary</param>
        /// <returns>Dictionary with any context properties for the logEvent (Null if none found)</returns>
        protected IDictionary<string, object> GetContextProperties(LogEventInfo logEvent, IDictionary<string, object> combinedProperties)
        {
            if (ContextProperties?.Count > 0)
            {
                combinedProperties = CaptureContextProperties(logEvent, combinedProperties);
            }

#if !SILVERLIGHT
            if (IncludeMdlc && !CombineProperties(logEvent, _contextLayout.MdlcLayout, ref combinedProperties))
            {
                combinedProperties = CaptureContextMdlc(logEvent, combinedProperties);
            }
#endif

            if (IncludeMdc && !CombineProperties(logEvent, _contextLayout.MdcLayout, ref combinedProperties))
            {
                combinedProperties = CaptureContextMdc(logEvent, combinedProperties);
            }

            if (IncludeGdc)
            {
                combinedProperties = CaptureContextGdc(logEvent, combinedProperties);
            }

            return combinedProperties;
        }

        /// <summary>
        /// Creates combined dictionary of all configured properties for logEvent
        /// </summary>
        /// <param name="logEvent"></param>
        /// <returns>Dictionary with all collected properties for logEvent</returns>
        protected IDictionary<string, object> GetAllProperties(LogEventInfo logEvent)
        {
            return GetAllProperties(logEvent, null);
        }

        /// <summary>
        /// Creates combined dictionary of all configured properties for logEvent
        /// </summary>
        /// <param name="logEvent"></param>
        /// <param name="combinedProperties">Optional prefilled dictionary</param>
        /// <returns>Dictionary with all collected properties for logEvent</returns>
        protected IDictionary<string, object> GetAllProperties(LogEventInfo logEvent, IDictionary<string, object> combinedProperties)
        {
            if (IncludeEventProperties && logEvent.HasProperties)
            {
                // TODO Make Dictionary-Lazy-adapter for PropertiesDictionary to skip extra Dictionary-allocation
                combinedProperties = combinedProperties ?? CreateNewDictionary(logEvent.Properties.Count + (ContextProperties?.Count ?? 0));
                bool checkForDuplicates = combinedProperties.Count > 0;
                foreach (var property in logEvent.Properties)
                {
                    string propertyKey = property.Key.ToString();
                    if (string.IsNullOrEmpty(propertyKey))
                        continue;

                    AddContextProperty(logEvent, propertyKey, property.Value, checkForDuplicates, combinedProperties);
                }
            }
            combinedProperties = GetContextProperties(logEvent, combinedProperties);
            return combinedProperties ?? new Dictionary<string, object>();
        }

        private static IDictionary<string, object> CreateNewDictionary(int initialCapacity)
        {
            return new Dictionary<string, object>(Math.Max(initialCapacity, 3));
        }

        /// <summary>
        /// Generates a new unique name, when duplicate names are detected
        /// </summary>
        /// <param name="logEvent">LogEvent that triggered the duplicate name</param>
        /// <param name="itemName">Duplicate item name</param>
        /// <param name="itemValue">Item Value</param>
        /// <param name="combinedProperties">Dictionary of context values</param>
        /// <returns>New (unique) value (or null to skip value). If the same value is used then the item will be overwritten</returns>
        protected virtual string GenerateUniqueItemName(LogEventInfo logEvent, string itemName, object itemValue, IDictionary<string, object> combinedProperties)
        {
            itemName = itemName ?? string.Empty;

            int newNameIndex = 1;
            var newItemName = string.Concat(itemName, "_1");
            while (combinedProperties.ContainsKey(newItemName))
            {
                newItemName = string.Concat(itemName, "_", (++newNameIndex).ToString());
            }

            return newItemName;
        }

        private bool CombineProperties(LogEventInfo logEvent, Layout contextLayout, ref IDictionary<string, object> combinedProperties)
        {
            if (!logEvent.TryGetCachedLayoutValue(contextLayout, out object value))
            {
                return false;
            }

            if (value is IDictionary<string, object> contextProperties)
            {
                if (combinedProperties != null)
                {
                    bool checkForDuplicates = combinedProperties.Count > 0;
                    foreach (var property in contextProperties)
                    {
                        AddContextProperty(logEvent, property.Key, property.Value, checkForDuplicates, combinedProperties);
                    }
                }
                else
                {
                    combinedProperties = contextProperties;
                }
            }
            return true;
        }

        private void AddContextProperty(LogEventInfo logEvent, string itemName, object itemValue, bool checkForDuplicates, IDictionary<string, object> combinedProperties)
        {
            if (checkForDuplicates && combinedProperties.ContainsKey(itemName))
            {
                itemName = GenerateUniqueItemName(logEvent, itemName, itemValue, combinedProperties);
                if (itemName == null)
                    return;
            }

            combinedProperties[itemName] = itemValue;
        }

        /// <summary>
        /// Returns the captured snapshot of <see cref="MappedDiagnosticsContext"/> for the <see cref="LogEventInfo"/>
        /// </summary>
        /// <param name="logEvent"></param>
        /// <returns>Dictionary with MDC context if any, else null</returns>
        protected IDictionary<string, object> GetContextMdc(LogEventInfo logEvent)
        {
            if (logEvent.TryGetCachedLayoutValue(_contextLayout.MdcLayout, out object value))
            {
                return value as IDictionary<string, object>;
            }
            return CaptureContextMdc(logEvent, null);
        }

#if !SILVERLIGHT
        /// <summary>
        /// Returns the captured snapshot of <see cref="MappedDiagnosticsLogicalContext"/> for the <see cref="LogEventInfo"/>
        /// </summary>
        /// <param name="logEvent"></param>
        /// <returns>Dictionary with MDLC context if any, else null</returns>
        protected IDictionary<string, object> GetContextMdlc(LogEventInfo logEvent)
        {
            if (logEvent.TryGetCachedLayoutValue(_contextLayout.MdlcLayout, out object value))
            {
                return value as IDictionary<string, object>;
            }
            return CaptureContextMdlc(logEvent, null);
        }
#endif

        /// <summary>
        /// Returns the captured snapshot of <see cref="NestedDiagnosticsContext"/> for the <see cref="LogEventInfo"/>
        /// </summary>
        /// <param name="logEvent"></param>
        /// <returns>Dictionary with NDC context if any, else null</returns>
        protected IList<object> GetContextNdc(LogEventInfo logEvent)
        {
            if (logEvent.TryGetCachedLayoutValue(_contextLayout.NdcLayout, out object value))
            {
                return value as IList<object>;
            }
            return CaptureContextNdc(logEvent);
        }

#if !SILVERLIGHT
        /// <summary>
        /// Returns the captured snapshot of <see cref="NestedDiagnosticsLogicalContext"/> for the <see cref="LogEventInfo"/>
        /// </summary>
        /// <param name="logEvent"></param>
        /// <returns>Dictionary with NDLC context if any, else null</returns>
        protected IList<object> GetContextNdlc(LogEventInfo logEvent)
        {
            if (logEvent.TryGetCachedLayoutValue(_contextLayout.NdlcLayout, out object value))
            {
                return value as IList<object>;
            }
            return CaptureContextNdlc(logEvent);
        }
#endif

        private IDictionary<string, object> CaptureContextProperties(LogEventInfo logEvent, IDictionary<string, object> combinedProperties)
        {
            combinedProperties = combinedProperties ?? CreateNewDictionary(ContextProperties.Count);
            for (int i = 0; i < ContextProperties.Count; ++i)
            {
                var contextProperty = ContextProperties[i];
                if (string.IsNullOrEmpty(contextProperty?.Name) || contextProperty.Layout == null)
                    continue;

                try
                {
                    if (TryGetContextPropertyValue(logEvent, contextProperty, out var propertyValue))
                    {
                        combinedProperties[contextProperty.Name] = propertyValue;
                    }
                }
                catch (Exception ex)
                {
                    if (ex.MustBeRethrownImmediately())
                        throw;

                    Common.InternalLogger.Warn(ex, "{0}(Name={1}): Failed to add context property {2}", GetType(), Name, contextProperty.Name);
                }
            }

            return combinedProperties;
        }

        private bool TryGetContextPropertyValue(LogEventInfo logEvent, TargetPropertyWithContext contextProperty, out object propertyValue)
        {
            var propertyType = contextProperty.PropertyType ?? typeof(string);

            var isStringType = propertyType == typeof(string);
            if (!isStringType && contextProperty.Layout.TryGetRawValue(logEvent, out var rawValue))
            {
                if (propertyType == typeof(object))
                {
                    propertyValue = rawValue;
                    return contextProperty.IncludeEmptyValue || propertyValue != null;
                }
                else if (rawValue?.GetType() == propertyType)
                {
                    propertyValue = rawValue;
                    return true;
                }
            }

            var propertyStringValue = RenderLogEvent(contextProperty.Layout, logEvent) ?? string.Empty;
            if (!contextProperty.IncludeEmptyValue && string.IsNullOrEmpty(propertyStringValue))
            {
                propertyValue = null;
                return false;
            }

            if (isStringType)
            {
                propertyValue = propertyStringValue;
                return true;
            }

            if (string.IsNullOrEmpty(propertyStringValue) && propertyType.IsValueType())
            {
                propertyValue = Activator.CreateInstance(propertyType);
                return true;
            }

            propertyValue = PropertyTypeConverter.Convert(propertyStringValue, propertyType, null, CultureInfo.InvariantCulture);
            return true;
        }

        /// <summary>
        /// Takes snapshot of <see cref="GlobalDiagnosticsContext"/> for the <see cref="LogEventInfo"/>
        /// </summary>
        /// <param name="logEvent"></param>
        /// <param name="contextProperties">Optional pre-allocated dictionary for the snapshot</param>
        /// <returns>Dictionary with GDC context if any, else null</returns>
        protected virtual IDictionary<string, object> CaptureContextGdc(LogEventInfo logEvent, IDictionary<string, object> contextProperties)
        {
            var globalNames = GlobalDiagnosticsContext.GetNames();
            if (globalNames.Count == 0)
                return contextProperties;

            contextProperties = contextProperties ?? CreateNewDictionary(globalNames.Count);
            bool checkForDuplicates = contextProperties.Count > 0;
            foreach (string propertyName in globalNames)
            {
                var propertyValue = GlobalDiagnosticsContext.GetObject(propertyName);
                if (SerializeItemValue(logEvent, propertyName, propertyValue, out propertyValue))
                {
                    AddContextProperty(logEvent, propertyName, propertyValue, checkForDuplicates, contextProperties);
                }
            }

            return contextProperties;
        }

        /// <summary>
        /// Takes snapshot of <see cref="MappedDiagnosticsContext"/> for the <see cref="LogEventInfo"/>
        /// </summary>
        /// <param name="logEvent"></param>
        /// <param name="contextProperties">Optional pre-allocated dictionary for the snapshot</param>
        /// <returns>Dictionary with MDC context if any, else null</returns>
        protected virtual IDictionary<string, object> CaptureContextMdc(LogEventInfo logEvent, IDictionary<string, object> contextProperties)
        {
            var names = MappedDiagnosticsContext.GetNames();
            if (names.Count == 0)
                return contextProperties;

            contextProperties = contextProperties ?? CreateNewDictionary(names.Count);
            bool checkForDuplicates = contextProperties.Count > 0;
            foreach (var name in names)
            {
                object value = MappedDiagnosticsContext.GetObject(name);
                if (SerializeMdcItem(logEvent, name, value, out var serializedValue))
                {
                    AddContextProperty(logEvent, name, serializedValue, checkForDuplicates, contextProperties);
                }
            }
            return contextProperties;
        }

        /// <summary>
        /// Take snapshot of a single object value from <see cref="MappedDiagnosticsContext"/>
        /// </summary>
        /// <param name="logEvent">Log event</param>
        /// <param name="name">MDC key</param>
        /// <param name="value">MDC value</param>
        /// <param name="serializedValue">Snapshot of MDC value</param>
        /// <returns>Include object value in snapshot</returns>
        protected virtual bool SerializeMdcItem(LogEventInfo logEvent, string name, object value, out object serializedValue)
        {
            if (string.IsNullOrEmpty(name))
            {
                serializedValue = null;
                return false;
            }

            return SerializeItemValue(logEvent, name, value, out serializedValue);
        }

#if !SILVERLIGHT
        /// <summary>
        /// Takes snapshot of <see cref="MappedDiagnosticsLogicalContext"/> for the <see cref="LogEventInfo"/>
        /// </summary>
        /// <param name="logEvent"></param>
        /// <param name="contextProperties">Optional pre-allocated dictionary for the snapshot</param>
        /// <returns>Dictionary with MDLC context if any, else null</returns>
        protected virtual IDictionary<string, object> CaptureContextMdlc(LogEventInfo logEvent, IDictionary<string, object> contextProperties)
        {
            var names = MappedDiagnosticsLogicalContext.GetNames();
            if (names.Count == 0)
                return contextProperties;

            contextProperties = contextProperties ?? CreateNewDictionary(names.Count);
            bool checkForDuplicates = contextProperties.Count > 0;
            foreach (var name in names)
            {
                object value = MappedDiagnosticsLogicalContext.GetObject(name);
                if (SerializeMdlcItem(logEvent, name, value, out var serializedValue))
                {
                    AddContextProperty(logEvent, name, serializedValue, checkForDuplicates, contextProperties);
                }
            }
            return contextProperties;
        }

        /// <summary>
        /// Take snapshot of a single object value from <see cref="MappedDiagnosticsLogicalContext"/>
        /// </summary>
        /// <param name="logEvent">Log event</param>
        /// <param name="name">MDLC key</param>
        /// <param name="value">MDLC value</param>
        /// <param name="serializedValue">Snapshot of MDLC value</param>
        /// <returns>Include object value in snapshot</returns>
        protected virtual bool SerializeMdlcItem(LogEventInfo logEvent, string name, object value, out object serializedValue)
        {
            if (string.IsNullOrEmpty(name))
            {
                serializedValue = null;
                return false;
            }

            return SerializeItemValue(logEvent, name, value, out serializedValue);
        }
#endif

        /// <summary>
        /// Takes snapshot of <see cref="NestedDiagnosticsContext"/> for the <see cref="LogEventInfo"/>
        /// </summary>
        /// <param name="logEvent"></param>
        /// <returns>Dictionary with NDC context if any, else null</returns>
        protected virtual IList<object> CaptureContextNdc(LogEventInfo logEvent)
        {
            var stack = NestedDiagnosticsContext.GetAllObjects();
            if (stack.Length == 0)
                return stack;

            IList<object> filteredStack = null;
            for (int i = 0; i < stack.Length; ++i)
            {
                var ndcValue = stack[i];
                if (SerializeNdcItem(logEvent, ndcValue, out var serializedValue))
                {
                    if (filteredStack != null)
                        filteredStack.Add(serializedValue);
                    else
                        stack[i] = serializedValue;
                }
                else
                {
                    if (filteredStack == null)
                    {
                        filteredStack = new List<object>(stack.Length);
                        for (int j = 0; j < i; ++j)
                            filteredStack.Add(stack[j]);
                    }
                }
            }
            return filteredStack ?? stack;
        }

        /// <summary>
        /// Take snapshot of a single object value from <see cref="NestedDiagnosticsContext"/>
        /// </summary>
        /// <param name="logEvent">Log event</param>
        /// <param name="value">NDC value</param>
        /// <param name="serializedValue">Snapshot of NDC value</param>
        /// <returns>Include object value in snapshot</returns>
        protected virtual bool SerializeNdcItem(LogEventInfo logEvent, object value, out object serializedValue)
        {
            return SerializeItemValue(logEvent, null, value, out serializedValue);
        }

#if !SILVERLIGHT
        /// <summary>
        /// Takes snapshot of <see cref="NestedDiagnosticsLogicalContext"/> for the <see cref="LogEventInfo"/>
        /// </summary>
        /// <param name="logEvent"></param>
        /// <returns>Dictionary with NDLC context if any, else null</returns>
        protected virtual IList<object> CaptureContextNdlc(LogEventInfo logEvent)
        {
            var stack = NestedDiagnosticsLogicalContext.GetAllObjects();
            if (stack.Length == 0)
                return stack;

            IList<object> filteredStack = null;
            for (int i = 0; i < stack.Length; ++i)
            {
                var ndcValue = stack[i];
                if (SerializeNdlcItem(logEvent, ndcValue, out var serializedValue))
                {
                    if (filteredStack != null)
                        filteredStack.Add(serializedValue);
                    else
                        stack[i] = serializedValue;
                }
                else
                {
                    if (filteredStack == null)
                    {
                        filteredStack = new List<object>(stack.Length);
                        for (int j = 0; j < i; ++j)
                            filteredStack.Add(stack[j]);
                    }
                }
            }
            return filteredStack ?? stack;
        }

        /// <summary>
        /// Take snapshot of a single object value from <see cref="NestedDiagnosticsLogicalContext"/>
        /// </summary>
        /// <param name="logEvent">Log event</param>
        /// <param name="value">NDLC value</param>
        /// <param name="serializedValue">Snapshot of NDLC value</param>
        /// <returns>Include object value in snapshot</returns>
        protected virtual bool SerializeNdlcItem(LogEventInfo logEvent, object value, out object serializedValue)
        {
            return SerializeItemValue(logEvent, null, value, out serializedValue);
        }
#endif

        /// <summary>
        /// Take snapshot of a single object value
        /// </summary>
        /// <param name="logEvent">Log event</param>
        /// <param name="name">Key Name (null when NDC / NDLC)</param>
        /// <param name="value">Object Value</param>
        /// <param name="serializedValue">Snapshot of value</param>
        /// <returns>Include object value in snapshot</returns>
        protected virtual bool SerializeItemValue(LogEventInfo logEvent, string name, object value, out object serializedValue)
        {
            if (value == null)
            {
                serializedValue = null;
                return true;
            }

            if (value is string || Convert.GetTypeCode(value) != TypeCode.Object || value is Guid || value is TimeSpan || value is DateTimeOffset)
            {
                serializedValue = value;    // Already immutable, snapshot is not needed
                return true;
            }

            // Make snapshot of the context value
            serializedValue = Convert.ToString(value, logEvent.FormatProvider ?? LoggingConfiguration?.DefaultCultureInfo);
            return true;
        }

        [ThreadSafe]
        [ThreadAgnostic]
        private class TargetWithContextLayout : Layout, IIncludeContext, IUsesStackTrace
        {
            public Layout TargetLayout { get => _targetLayout; set => _targetLayout = ReferenceEquals(this, value) ? _targetLayout : value; }
            private Layout _targetLayout;

            /// <summary>Internal Layout that allows capture of MDC context</summary>
            internal LayoutContextMdc MdcLayout { get; }
            /// <summary>Internal Layout that allows capture of NDC context</summary>
            internal LayoutContextNdc NdcLayout { get; }
#if !SILVERLIGHT
            /// <summary>Internal Layout that allows capture of MDLC context</summary>
            internal LayoutContextMdlc MdlcLayout { get; }
            /// <summary>Internal Layout that allows capture of NDLC context</summary>
            internal LayoutContextNdlc NdlcLayout { get; }
#endif

            public bool IncludeAllProperties { get; set; }
            public bool IncludeCallSite { get; set; }
            public bool IncludeCallSiteStackTrace { get; set; }

            public bool IncludeMdc { get => MdcLayout.IsActive; set => MdcLayout.IsActive = value; }
            public bool IncludeNdc { get => NdcLayout.IsActive; set => NdcLayout.IsActive = value; }

#if !SILVERLIGHT
            public bool IncludeMdlc { get => MdlcLayout.IsActive; set => MdlcLayout.IsActive = value; }
            public bool IncludeNdlc { get => NdlcLayout.IsActive; set => NdlcLayout.IsActive = value; }
#endif

            StackTraceUsage IUsesStackTrace.StackTraceUsage
            {
                get
                {
                    if (IncludeCallSiteStackTrace)
                    {
#if !SILVERLIGHT
                        return StackTraceUsage.WithSource;
#else
                        return StackTraceUsage.Max;
#endif
                    }

                    if (IncludeCallSite)
                    {
                        return StackTraceUsage.WithoutSource;
                    }
                    return StackTraceUsage.None;
                }
            }

            public TargetWithContextLayout(TargetWithContext owner, Layout targetLayout)
            {
                TargetLayout = targetLayout;

                MdcLayout = new LayoutContextMdc(owner);
                NdcLayout = new LayoutContextNdc(owner);
#if !SILVERLIGHT
                MdlcLayout = new LayoutContextMdlc(owner);
                NdlcLayout = new LayoutContextNdlc(owner);
#endif
            }

            protected override void InitializeLayout()
            {
                base.InitializeLayout();
                if (IncludeMdc || IncludeNdc)
                    ThreadAgnostic = false;
#if !SILVERLIGHT
                if (IncludeMdlc || IncludeNdlc)
                    ThreadAgnostic = false;
#endif
                if (IncludeAllProperties)
                    MutableUnsafe = true;   // TODO Need to convert Properties to an immutable state
            }

            public override string ToString()
            {
                return TargetLayout?.ToString() ?? base.ToString();
            }

            public override void Precalculate(LogEventInfo logEvent)
            {
                if (!(TargetLayout?.ThreadAgnostic ?? true) || (TargetLayout?.MutableUnsafe ?? false))
                {
                    TargetLayout.Precalculate(logEvent);
                    if (logEvent.TryGetCachedLayoutValue(TargetLayout, out var cachedLayout))
                    {
                        // Also cache the result as belonging to this Layout, for fast lookup
                        logEvent.AddCachedLayoutValue(this, cachedLayout);
                    }
                }

                PrecalculateContext(logEvent);
            }

            internal override void PrecalculateBuilder(LogEventInfo logEvent, StringBuilder target)
            {
                if (!(TargetLayout?.ThreadAgnostic ?? true) || (TargetLayout?.MutableUnsafe ?? false))
                {
                    TargetLayout.PrecalculateBuilder(logEvent, target);
                    if (logEvent.TryGetCachedLayoutValue(TargetLayout, out var cachedLayout))
                    {
                        // Also cache the result as belonging to this Layout, for fast lookup
                        logEvent.AddCachedLayoutValue(this, cachedLayout);
                    }
                }

                PrecalculateContext(logEvent);
            }

            private void PrecalculateContext(LogEventInfo logEvent)
            {
                if (IncludeMdc)
                    MdcLayout.Precalculate(logEvent);
                if (IncludeNdc)
                    NdcLayout.Precalculate(logEvent);
#if !SILVERLIGHT
                if (IncludeMdlc)
                    MdlcLayout.Precalculate(logEvent);
                if (IncludeNdlc)
                    NdlcLayout.Precalculate(logEvent);
#endif
            }

            protected override string GetFormattedMessage(LogEventInfo logEvent)
            {
                return TargetLayout?.Render(logEvent) ?? string.Empty;
            }

            protected override void RenderFormattedMessage(LogEventInfo logEvent, StringBuilder target)
            {
                TargetLayout?.RenderAppendBuilder(logEvent, target, false);
            }

            [ThreadSafe]
            public class LayoutContextMdc : Layout
            {
                private readonly TargetWithContext _owner;

                public bool IsActive { get; set; }

                public LayoutContextMdc(TargetWithContext owner)
                {
                    _owner = owner;
                }

                protected override string GetFormattedMessage(LogEventInfo logEvent)
                {
                    CaptureContext(logEvent);
                    return string.Empty;
                }

                public override void Precalculate(LogEventInfo logEvent)
                {
                    CaptureContext(logEvent);
                }

                private void CaptureContext(LogEventInfo logEvent)
                {
                    if (IsActive)
                    {
                        var contextMdc = _owner.CaptureContextMdc(logEvent, null);
                        logEvent.AddCachedLayoutValue(this, contextMdc);
                    }
                }
            }

#if !SILVERLIGHT
            [ThreadSafe]
            public class LayoutContextMdlc : Layout
            {
                private readonly TargetWithContext _owner;

                public bool IsActive { get; set; }

                public LayoutContextMdlc(TargetWithContext owner)
                {
                    _owner = owner;
                }

                protected override string GetFormattedMessage(LogEventInfo logEvent)
                {
                    CaptureContext(logEvent);
                    return string.Empty;
                }

                public override void Precalculate(LogEventInfo logEvent)
                {
                    CaptureContext(logEvent);
                }

                private void CaptureContext(LogEventInfo logEvent)
                {
                    if (IsActive)
                    {
                        var contextMdlc = _owner.CaptureContextMdlc(logEvent, null);
                        logEvent.AddCachedLayoutValue(this, contextMdlc);
                    }
                }
            }
#endif
            [ThreadSafe]
            public class LayoutContextNdc : Layout
            {
                private readonly TargetWithContext _owner;

                public bool IsActive { get; set; }

                public LayoutContextNdc(TargetWithContext owner)
                {
                    _owner = owner;
                }

                protected override string GetFormattedMessage(LogEventInfo logEvent)
                {
                    CaptureContext(logEvent);
                    return string.Empty;
                }

                public override void Precalculate(LogEventInfo logEvent)
                {
                    CaptureContext(logEvent);
                }

                private void CaptureContext(LogEventInfo logEvent)
                {
                    if (IsActive)
                    {
                        var contextNdc = _owner.CaptureContextNdc(logEvent);
                        logEvent.AddCachedLayoutValue(this, contextNdc);
                    }
                }
            }

#if !SILVERLIGHT
            [ThreadSafe]
            public class LayoutContextNdlc : Layout
            {
                private readonly TargetWithContext _owner;

                public bool IsActive { get; set; }

                public LayoutContextNdlc(TargetWithContext owner)
                {
                    _owner = owner;
                }

                protected override string GetFormattedMessage(LogEventInfo logEvent)
                {
                    CaptureContext(logEvent);
                    return string.Empty;
                }

                public override void Precalculate(LogEventInfo logEvent)
                {
                    CaptureContext(logEvent);
                }

                private void CaptureContext(LogEventInfo logEvent)
                {
                    if (IsActive)
                    {
                        var contextNdlc = _owner.CaptureContextNdlc(logEvent);
                        logEvent.AddCachedLayoutValue(this, contextNdlc);
                    }
                }
            }
#endif
        }
    }
}
