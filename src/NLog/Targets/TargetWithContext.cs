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

namespace NLog.Targets
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Text;
    using NLog.Config;
    using NLog.Internal;
    using NLog.Layouts;

    /// <summary>
    /// Represents target that supports context capture of <see cref="ScopeContext"/> Properties + Nested-states
    /// </summary>
    /// <remarks>
    /// <a href="https://github.com/NLog/NLog/wiki/How-to-write-a-custom-target-for-structured-logging">See NLog Wiki</a>
    /// </remarks>
    /// <example><code>
    /// [Target("MyFirst")]
    /// public sealed class MyFirstTarget : TargetWithContext
    /// {
    ///    public MyFirstTarget()
    ///    {
    ///        this.Host = "localhost";
    ///    }
    ///     
    ///    [RequiredParameter]
    ///    public Layout Host { get; set; }
    ///
    ///    protected override void Write(LogEventInfo logEvent) 
    ///    {
    ///        string logMessage = this.RenderLogEvent(this.Layout, logEvent);
    ///        string hostName = this.RenderLogEvent(this.Host, logEvent);
    ///        return SendTheMessageToRemoteHost(hostName, logMessage);
    ///    }
    ///
    ///    private void SendTheMessageToRemoteHost(string hostName, string message)
    ///    {
    ///        // To be implemented
    ///    }
    /// }
    /// </code></example>
    /// <seealso href="https://github.com/NLog/NLog/wiki/How-to-write-a-custom-target-for-structured-logging">Documentation on NLog Wiki</seealso>
    public abstract class TargetWithContext : TargetWithLayout, IIncludeContext
    {
        /// <inheritdoc/>
        /// <docgen category='Layout Options' order='1' />
        public sealed override Layout Layout
        {
            get => _contextLayout;
            set
            {
                if (_contextLayout is null)
                    _contextLayout = new TargetWithContextLayout(this, value);
                else
                    _contextLayout.TargetLayout = value;
            }
        }
        private TargetWithContextLayout _contextLayout;

        /// <summary>
        /// Gets or sets the option to include all properties from the log events
        /// </summary>
        /// <docgen category='Layout Options' order='10' />
        public bool IncludeEventProperties { get => _contextLayout.IncludeEventProperties; set => _contextLayout.IncludeEventProperties = value; }

        /// <summary>
        /// Gets or sets whether to include the contents of the <see cref="ScopeContext"/> properties-dictionary.
        /// </summary>
        /// <docgen category='Layout Options' order='10' />
        public bool IncludeScopeProperties { get => _contextLayout.IncludeScopeProperties; set => _contextLayout.IncludeScopeProperties = value; }

        /// <summary>
        /// Gets or sets whether to include the contents of the <see cref="ScopeContext"/> nested-state-stack.
        /// </summary>
        /// <docgen category='Layout Options' order='10' />
        public bool IncludeScopeNested { get => _contextLayout.IncludeScopeNested; set => _contextLayout.IncludeScopeNested = value; }

        /// <summary>
        /// Obsolete and replaced by <see cref="IncludeScopeProperties"/> with NLog v5.
        /// Gets or sets whether to include the contents of the <see cref="MappedDiagnosticsContext"/>-dictionary.
        /// </summary>
        /// <docgen category='Layout Options' order='10' />
        [Obsolete("Replaced by IncludeScopeProperties. Marked obsolete on NLog 5.0")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool IncludeMdc { get => _contextLayout.IncludeMdc; set => _contextLayout.IncludeMdc = value; }

        /// <summary>
        /// Obsolete and replaced by <see cref="IncludeScopeNested"/> with NLog v5.
        /// Gets or sets whether to include the contents of the <see cref="NestedDiagnosticsContext"/>-stack.
        /// </summary>
        /// <docgen category='Layout Options' order='10' />
        [Obsolete("Replaced by IncludeScopeNested. Marked obsolete on NLog 5.0")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool IncludeNdc { get => _contextLayout.IncludeNdc; set => _contextLayout.IncludeNdc = value; }

        /// <summary>
        /// Obsolete and replaced by <see cref="IncludeScopeProperties"/> with NLog v5.
        /// Gets or sets whether to include the contents of the <see cref="MappedDiagnosticsLogicalContext"/>-properties.
        /// </summary>
        /// <docgen category='Layout Options' order='10' />
        [Obsolete("Replaced by IncludeScopeProperties. Marked obsolete on NLog 5.0")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool IncludeMdlc { get => _contextLayout.IncludeMdlc; set => _contextLayout.IncludeMdlc = value; }

        /// <summary>
        /// Obsolete and replaced by <see cref="IncludeScopeNested"/> with NLog v5.
        /// Gets or sets whether to include the contents of the <see cref="NestedDiagnosticsLogicalContext"/>-stack.
        /// </summary>
        /// <docgen category='Layout Options' order='10' />
        [Obsolete("Replaced by IncludeScopeNested. Marked obsolete on NLog 5.0")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool IncludeNdlc { get => _contextLayout.IncludeNdlc; set => _contextLayout.IncludeNdlc = value; }

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

        /// <summary>
        /// List of property names to exclude when <see cref="IncludeEventProperties"/> is true
        /// </summary>
        /// <docgen category='Layout Options' order='50' />
#if !NET35
        public ISet<string> ExcludeProperties { get; set; }
#else
        public HashSet<string> ExcludeProperties { get; set; }        
#endif

        /// <summary>
        /// Constructor
        /// </summary>
        protected TargetWithContext()
        {
            _contextLayout = _contextLayout ?? new TargetWithContextLayout(this, base.Layout);
            ExcludeProperties = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Check if logevent has properties (or context properties)
        /// </summary>
        /// <param name="logEvent"></param>
        /// <returns>True if properties should be included</returns>
        protected bool ShouldIncludeProperties(LogEventInfo logEvent)
        {
            return IncludeGdc
            || IncludeScopeProperties
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

            if (IncludeScopeProperties && !CombineProperties(logEvent, _contextLayout.ScopeContextPropertiesLayout, ref combinedProperties))
            {
                combinedProperties = CaptureScopeContextProperties(logEvent, combinedProperties);
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
            return AllPropertiesDictionary.GetAllProperties(this, logEvent) ?? GetAllProperties(logEvent, null);
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
                bool checkExcludeProperties = ExcludeProperties?.Count > 0;
                using (var propertyEnumerator = logEvent.CreateOrUpdatePropertiesInternal().GetPropertyEnumerator())
                {
                    while (propertyEnumerator.MoveNext())
                    {
                        var property = propertyEnumerator.CurrentProperty;
                        if (string.IsNullOrEmpty(property.Key))
                            continue;

                        if (checkExcludeProperties && ExcludeProperties.Contains(property.Key))
                            continue;

                        AddContextProperty(logEvent, property.Key, property.Value, checkForDuplicates, combinedProperties);
                    }
                }
            }
            combinedProperties = GetContextProperties(logEvent, combinedProperties);
            return combinedProperties ?? CreateNewDictionary(0);
        }

        private static IDictionary<string, object> CreateNewDictionary(int initialCapacity)
        {
            return new Dictionary<string, object>(initialCapacity < 3 ? 0 : initialCapacity, StringComparer.Ordinal);
        }

        /// <summary>
        /// Yields all properties collected from the <paramref name="logEvent"/>
        /// </summary>
        /// <param name="logEvent"></param>
        /// <returns>Collection of properties from logEvent</returns>
        /// <remarks>Skips the dictionary allocation upfront, but no protection against duplicate property names.</remarks>
        protected IEnumerable<KeyValuePair<string, object>> GetAllPropertiesList(LogEventInfo logEvent)
        {
            var scopeProperties = GetScopePropertiesList(logEvent);

            if (IncludeEventProperties && logEvent.HasProperties)
            {
                var eventProperties = logEvent.CreateOrUpdatePropertiesInternal();
                return YieldAllProperties(logEvent, eventProperties, scopeProperties);
            }

            if (ContextProperties?.Count > 0 || !(scopeProperties is null) || IncludeGdc)
            {
                return YieldAllProperties(logEvent, null, scopeProperties);
            }

            return ArrayHelper.Empty<KeyValuePair<string, object>>();
        }

        private IEnumerable<KeyValuePair<string, object>> GetScopePropertiesList(LogEventInfo logEvent)
        {
            if (IncludeScopeProperties)
            {
                if (logEvent.TryGetCachedLayoutValue(_contextLayout.ScopeContextPropertiesLayout, out object value))
                {
                    if (value is IDictionary<string, object> scopeProperties && scopeProperties.Count > 0)
                    {
                        return scopeProperties;
                    }
                }
                else
                {
                    var scopeProperties = ScopeContext.GetAllProperties();
                    if (scopeProperties is ICollection<KeyValuePair<string, object>> scopeCollection)
                    {
                        if (scopeCollection.Count > 0)
                        {
                            return scopeCollection;
                        }
                    }
                    else
                    {
                        return scopeProperties;
                    }
                }
            }

            return null;
        }

        private IEnumerable<KeyValuePair<string, object>> YieldAllProperties(LogEventInfo logEvent, PropertiesDictionary eventProperties, IEnumerable<KeyValuePair<string, object>> scopeProperties)
        {
            bool checkExcludeProperties = ExcludeProperties?.Count > 0;

            if (eventProperties != null)
            {
                using (var propertyEnumerator = eventProperties.GetPropertyEnumerator())
                {
                    while (propertyEnumerator.MoveNext())
                    {
                        var property = propertyEnumerator.CurrentProperty;
                        if (string.IsNullOrEmpty(property.Key))
                            continue;

                        if (checkExcludeProperties && ExcludeProperties.Contains(property.Key))
                            continue;

                        yield return property;
                    }
                }
            }

            if (scopeProperties != null)
            {
                foreach (var property in scopeProperties)
                {
                    var propertyName = property.Key;
                    if (string.IsNullOrEmpty(propertyName))
                        continue;

                    if (checkExcludeProperties && ExcludeProperties.Contains(propertyName))
                        continue;

                    object propertyValue = property.Value;
                    if (SerializeScopeContextProperty(logEvent, propertyName, propertyValue, out var serializedValue))
                    {
                        yield return new KeyValuePair<string, object>(propertyName, serializedValue);
                    }
                }
            }

            if (IncludeGdc)
            {
                var gdcKeys = GlobalDiagnosticsContext.GetNames();
                if (gdcKeys.Count > 0)
                {
                    foreach (string propertyName in gdcKeys)
                    {
                        if (string.IsNullOrEmpty(propertyName))
                            continue;

                        if (checkExcludeProperties && ExcludeProperties.Contains(propertyName))
                            continue;

                        var propertyValue = GlobalDiagnosticsContext.GetObject(propertyName);
                        if (SerializeItemValue(logEvent, propertyName, propertyValue, out var serializedValue))
                        {
                            yield return new KeyValuePair<string, object>(propertyName, serializedValue);
                        }
                    }
                }
            }

            if (ContextProperties?.Count > 0)
            {
                for (int i = 0; i < ContextProperties.Count; ++i)
                {
                    var contextProperty = ContextProperties[i];
                    if (string.IsNullOrEmpty(contextProperty?.Name) || contextProperty.Layout is null)
                        continue;

                    if (TryGetContextPropertyValue(logEvent, contextProperty, out var propertyValue))
                    {
                        yield return new KeyValuePair<string, object>(contextProperty.Name, propertyValue);
                    }
                }
            }
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
            return PropertiesDictionary.GenerateUniquePropertyName(itemName, combinedProperties, (newKey, props) => props.ContainsKey(newKey));
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

        private void AddContextProperty(LogEventInfo logEvent, string propertyName, object propertyValue, bool checkForDuplicates, IDictionary<string, object> combinedProperties)
        {
            if (checkForDuplicates && combinedProperties.ContainsKey(propertyName))
            {
                propertyName = GenerateUniqueItemName(logEvent, propertyName, propertyValue, combinedProperties);
                if (propertyName is null)
                    return;
            }

            combinedProperties[propertyName] = propertyValue;
        }

        /// <summary>
        /// Obsolete and replaced by <see cref="GetScopeContextProperties"/> with NLog v5.
        /// Returns the captured snapshot of <see cref="MappedDiagnosticsContext"/> for the <see cref="LogEventInfo"/>
        /// </summary>
        /// <param name="logEvent"></param>
        /// <returns>Dictionary with MDC context if any, else null</returns>
        [Obsolete("Replaced by GetScopeContextProperties. Marked obsolete on NLog 5.0")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        protected IDictionary<string, object> GetContextMdc(LogEventInfo logEvent)
        {
            if (logEvent.TryGetCachedLayoutValue(_contextLayout.ScopeContextPropertiesLayout, out object value))
            {
                return value as IDictionary<string, object>;
            }
            return CaptureContextMdc(logEvent, null);
        }

        /// <summary>
        /// Returns the captured snapshot of <see cref="ScopeContext"/> dictionary for the <see cref="LogEventInfo"/>
        /// </summary>
        /// <param name="logEvent"></param>
        /// <returns>Dictionary with ScopeContext properties if any, else null</returns>
        protected IDictionary<string, object> GetScopeContextProperties(LogEventInfo logEvent)
        {
            if (logEvent.TryGetCachedLayoutValue(_contextLayout.ScopeContextPropertiesLayout, out object value))
            {
                return value as IDictionary<string, object>;
            }
            return CaptureScopeContextProperties(logEvent, null);
        }

        /// <summary>
        /// Obsolete and replaced by <see cref="GetScopeContextProperties"/> with NLog v5.
        /// Returns the captured snapshot of <see cref="MappedDiagnosticsLogicalContext"/> for the <see cref="LogEventInfo"/>
        /// </summary>
        /// <param name="logEvent"></param>
        /// <returns>Dictionary with MDLC context if any, else null</returns>
        [Obsolete("Replaced by GetScopeContextProperties. Marked obsolete on NLog 5.0")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        protected IDictionary<string, object> GetContextMdlc(LogEventInfo logEvent)
        {
            if (logEvent.TryGetCachedLayoutValue(_contextLayout.ScopeContextPropertiesLayout, out object value))
            {
                return value as IDictionary<string, object>;
            }
            return CaptureContextMdlc(logEvent, null);
        }

        /// <summary>
        /// Obsolete and replaced by <see cref="GetScopeContextNested"/> with NLog v5.
        /// Returns the captured snapshot of <see cref="NestedDiagnosticsContext"/> for the <see cref="LogEventInfo"/>
        /// </summary>
        /// <param name="logEvent"></param>
        /// <returns>Collection with NDC context if any, else null</returns>
        [Obsolete("Replaced by GetScopeContextNested. Marked obsolete on NLog 5.0")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        protected IList<object> GetContextNdc(LogEventInfo logEvent)
        {
            if (logEvent.TryGetCachedLayoutValue(_contextLayout.ScopeContextNestedStatesLayout, out object value))
            {
                return value as IList<object>;
            }
            return CaptureContextNdc(logEvent);
        }

        /// <summary>
        /// Returns the captured snapshot of nested states from <see cref="ScopeContext"/> for the <see cref="LogEventInfo"/>
        /// </summary>
        /// <param name="logEvent"></param>
        /// <returns>Collection of nested state objects if any, else null</returns>
        protected IList<object> GetScopeContextNested(LogEventInfo logEvent)
        {
            if (logEvent.TryGetCachedLayoutValue(_contextLayout.ScopeContextNestedStatesLayout, out object value))
            {
                return value as IList<object>;
            }
            return CaptureScopeContextNested(logEvent);
        }

        /// <summary>
        /// Obsolete and replaced by <see cref="GetScopeContextNested"/> with NLog v5.
        /// Returns the captured snapshot of <see cref="NestedDiagnosticsLogicalContext"/> for the <see cref="LogEventInfo"/>
        /// </summary>
        /// <param name="logEvent"></param>
        /// <returns>Collection with NDLC context if any, else null</returns>
        [Obsolete("Replaced by GetScopeContextNested. Marked obsolete on NLog 5.0")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        protected IList<object> GetContextNdlc(LogEventInfo logEvent)
        {
            if (logEvent.TryGetCachedLayoutValue(_contextLayout.ScopeContextNestedStatesLayout, out object value))
            {
                return value as IList<object>;
            }
            return CaptureContextNdlc(logEvent);
        }

        private IDictionary<string, object> CaptureContextProperties(LogEventInfo logEvent, IDictionary<string, object> combinedProperties)
        {
            combinedProperties = combinedProperties ?? CreateNewDictionary(ContextProperties.Count);
            for (int i = 0; i < ContextProperties.Count; ++i)
            {
                var contextProperty = ContextProperties[i];
                if (string.IsNullOrEmpty(contextProperty?.Name) || contextProperty.Layout is null)
                    continue;

                if (TryGetContextPropertyValue(logEvent, contextProperty, out var propertyValue))
                {
                    combinedProperties[contextProperty.Name] = propertyValue;
                }
            }

            return combinedProperties;
        }

        private bool TryGetContextPropertyValue(LogEventInfo logEvent, TargetPropertyWithContext contextProperty, out object propertyValue)
        {
            try
            {
                propertyValue = contextProperty.RenderValue(logEvent);
                if (!contextProperty.IncludeEmptyValue && (propertyValue is null || string.Empty.Equals(propertyValue)))
                {
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                if (ex.MustBeRethrownImmediately())
                    throw;

                Common.InternalLogger.Warn(ex, "{0}: Failed to add context property {1}", this, contextProperty.Name);
                propertyValue = null;
                return false;
            }           
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
            bool checkExcludeProperties = ExcludeProperties.Count > 0;
            foreach (string propertyName in globalNames)
            {
                if (string.IsNullOrEmpty(propertyName))
                    continue;

                if (checkExcludeProperties && ExcludeProperties.Contains(propertyName))
                    continue;

                var propertyValue = GlobalDiagnosticsContext.GetObject(propertyName);
                if (SerializeItemValue(logEvent, propertyName, propertyValue, out propertyValue))
                {
                    AddContextProperty(logEvent, propertyName, propertyValue, checkForDuplicates, contextProperties);
                }
            }

            return contextProperties;
        }

        /// <summary>
        /// Obsolete and replaced by <see cref="CaptureScopeContextProperties"/> with NLog v5.
        /// Takes snapshot of <see cref="MappedDiagnosticsContext"/> for the <see cref="LogEventInfo"/>
        /// </summary>
        /// <param name="logEvent"></param>
        /// <param name="contextProperties">Optional pre-allocated dictionary for the snapshot</param>
        /// <returns>Dictionary with MDC context if any, else null</returns>
        [Obsolete("Replaced by CaptureScopeContextProperties. Marked obsolete on NLog 5.0")]
        [EditorBrowsable(EditorBrowsableState.Never)]
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
        /// Obsolete and replaced by <see cref="SerializeScopeContextProperty"/> with NLog v5.
        /// Take snapshot of a single object value from <see cref="MappedDiagnosticsContext"/>
        /// </summary>
        /// <param name="logEvent">Log event</param>
        /// <param name="name">MDC key</param>
        /// <param name="value">MDC value</param>
        /// <param name="serializedValue">Snapshot of MDC value</param>
        /// <returns>Include object value in snapshot</returns>
        [Obsolete("Replaced by SerializeScopeContextProperty. Marked obsolete on NLog 5.0")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        protected virtual bool SerializeMdcItem(LogEventInfo logEvent, string name, object value, out object serializedValue)
        {
            if (string.IsNullOrEmpty(name))
            {
                serializedValue = null;
                return false;
            }

            return SerializeItemValue(logEvent, name, value, out serializedValue);
        }

        /// <summary>
        /// Obsolete and replaced by <see cref="CaptureScopeContextProperties"/> with NLog v5.
        /// Takes snapshot of <see cref="MappedDiagnosticsLogicalContext"/> for the <see cref="LogEventInfo"/>
        /// </summary>
        /// <param name="logEvent"></param>
        /// <param name="contextProperties">Optional pre-allocated dictionary for the snapshot</param>
        /// <returns>Dictionary with MDLC context if any, else null</returns>
        [Obsolete("Replaced by CaptureScopeContextProperties. Marked obsolete on NLog 5.0")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        protected virtual IDictionary<string, object> CaptureContextMdlc(LogEventInfo logEvent, IDictionary<string, object> contextProperties)
        {
            return CaptureScopeContextProperties(logEvent, contextProperties);
        }

        /// <summary>
        /// Takes snapshot of <see cref="ScopeContext"/> dictionary for the <see cref="LogEventInfo"/>
        /// </summary>
        /// <param name="logEvent"></param>
        /// <param name="contextProperties">Optional pre-allocated dictionary for the snapshot</param>
        /// <returns>Dictionary with ScopeContext properties if any, else null</returns>
        protected virtual IDictionary<string, object> CaptureScopeContextProperties(LogEventInfo logEvent, IDictionary<string, object> contextProperties)
        {
            using (var scopeEnumerator = ScopeContext.GetAllPropertiesEnumerator())
            {
                bool checkForDuplicates = contextProperties?.Count > 0;
                bool checkExcludeProperties = ExcludeProperties.Count > 0;
                while (scopeEnumerator.MoveNext())
                {
                    var scopeProperty = scopeEnumerator.Current;
                    var propertyName = scopeProperty.Key;
                    if (string.IsNullOrEmpty(propertyName))
                        continue;

                    if (checkExcludeProperties && ExcludeProperties.Contains(propertyName))
                        continue;

                    contextProperties = contextProperties ?? CreateNewDictionary(0);

                    object propertyValue = scopeProperty.Value;
                    if (SerializeScopeContextProperty(logEvent, propertyName, propertyValue, out var serializedValue))
                    {
                        AddContextProperty(logEvent, propertyName, serializedValue, checkForDuplicates, contextProperties);
                    }
                }
            }

            return contextProperties;
        }

        /// <summary>
        /// Obsolete and replaced by <see cref="SerializeScopeContextProperty"/> with NLog v5.
        /// Take snapshot of a single object value from <see cref="MappedDiagnosticsLogicalContext"/>
        /// </summary>
        /// <param name="logEvent">Log event</param>
        /// <param name="name">MDLC key</param>
        /// <param name="value">MDLC value</param>
        /// <param name="serializedValue">Snapshot of MDLC value</param>
        /// <returns>Include object value in snapshot</returns>
        [Obsolete("Replaced by SerializeScopeContextProperty. Marked obsolete on NLog 5.0")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        protected bool SerializeMdlcItem(LogEventInfo logEvent, string name, object value, out object serializedValue)
        {
            return SerializeScopeContextProperty(logEvent, name, value, out serializedValue);
        }

        /// <summary>
        /// Take snapshot of a single object value from <see cref="ScopeContext"/> dictionary
        /// </summary>
        /// <param name="logEvent">Log event</param>
        /// <param name="name">ScopeContext Dictionary key</param>
        /// <param name="value">ScopeContext Dictionary value</param>
        /// <param name="serializedValue">Snapshot of ScopeContext property-value</param>
        /// <returns>Include object value in snapshot</returns>
        protected virtual bool SerializeScopeContextProperty(LogEventInfo logEvent, string name, object value, out object serializedValue)
        {
            if (string.IsNullOrEmpty(name))
            {
                serializedValue = null;
                return false;
            }

            return SerializeItemValue(logEvent, name, value, out serializedValue);
        }

        /// <summary>
        /// Obsolete and replaced by <see cref="CaptureScopeContextNested"/> with NLog v5.
        /// Takes snapshot of <see cref="NestedDiagnosticsContext"/> for the <see cref="LogEventInfo"/>
        /// </summary>
        /// <param name="logEvent"></param>
        /// <returns>Collection with NDC context if any, else null</returns>
        [Obsolete("Replaced by CaptureScopeContextNested. Marked obsolete on NLog 5.0")]
        [EditorBrowsable(EditorBrowsableState.Never)]
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
                    if (filteredStack is null)
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
        /// Obsolete and replaced by <see cref="SerializeScopeContextNestedState"/> with NLog v5.
        /// Take snapshot of a single object value from <see cref="NestedDiagnosticsContext"/>
        /// </summary>
        /// <param name="logEvent">Log event</param>
        /// <param name="value">NDC value</param>
        /// <param name="serializedValue">Snapshot of NDC value</param>
        /// <returns>Include object value in snapshot</returns>
        [Obsolete("Replaced by SerializeScopeContextNestedState. Marked obsolete on NLog 5.0")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        protected virtual bool SerializeNdcItem(LogEventInfo logEvent, object value, out object serializedValue)
        {
            return SerializeItemValue(logEvent, null, value, out serializedValue);
        }

        /// <summary>
        /// Obsolete and replaced by <see cref="CaptureScopeContextNested"/> with NLog v5.
        /// Takes snapshot of <see cref="NestedDiagnosticsLogicalContext"/> for the <see cref="LogEventInfo"/>
        /// </summary>
        /// <param name="logEvent"></param>
        /// <returns>Collection with NDLC context if any, else null</returns>
        [Obsolete("Replaced by CaptureScopeContextNested. Marked obsolete on NLog 5.0")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        protected virtual IList<object> CaptureContextNdlc(LogEventInfo logEvent)
        {
            return CaptureScopeContextNested(logEvent);
        }

        /// <summary>
        /// Takes snapshot of nested states from <see cref="ScopeContext"/> for the <see cref="LogEventInfo"/>
        /// </summary>
        /// <param name="logEvent"></param>
        /// <returns>Collection with <see cref="ScopeContext"/> stack items if any, else null</returns>
        protected virtual IList<object> CaptureScopeContextNested(LogEventInfo logEvent)
        {
            var stack = ScopeContext.GetAllNestedStateList();
            if (stack.Count == 0)
                return stack;

            IList<object> filteredStack = null;
            for (int i = 0; i < stack.Count; ++i)
            {
                var ndcValue = stack[i];
                if (SerializeScopeContextNestedState(logEvent, ndcValue, out var serializedValue))
                {
                    if (filteredStack != null)
                        filteredStack.Add(serializedValue);
                    else
                        stack[i] = serializedValue;
                }
                else
                {
                    if (filteredStack is null)
                    {
                        filteredStack = new List<object>(stack.Count);
                        for (int j = 0; j < i; ++j)
                            filteredStack.Add(stack[j]);
                    }
                }
            }
            return filteredStack ?? stack;
        }

        /// <summary>
        /// Obsolete and replaced by <see cref="SerializeScopeContextNestedState"/> with NLog v5.
        /// Take snapshot of a single object value from <see cref="NestedDiagnosticsLogicalContext"/>
        /// </summary>
        /// <param name="logEvent">Log event</param>
        /// <param name="value">NDLC value</param>
        /// <param name="serializedValue">Snapshot of NDLC value</param>
        /// <returns>Include object value in snapshot</returns>
        [Obsolete("Replaced by SerializeScopeContextNestedState. Marked obsolete on NLog 5.0")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        protected virtual bool SerializeNdlcItem(LogEventInfo logEvent, object value, out object serializedValue)
        {
            return SerializeScopeContextNestedState(logEvent, value, out serializedValue);
        }

        /// <summary>
        /// Take snapshot of a single object value from <see cref="ScopeContext"/> nested states
        /// </summary>
        /// <param name="logEvent">Log event</param>
        /// <param name="value"><see cref="ScopeContext"/> nested state value</param>
        /// <param name="serializedValue">Snapshot of <see cref="ScopeContext"/> stack item value</param>
        /// <returns>Include object value in snapshot</returns>
        protected virtual bool SerializeScopeContextNestedState(LogEventInfo logEvent, object value, out object serializedValue)
        {
            return SerializeItemValue(logEvent, null, value, out serializedValue);
        }

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
            if (value is null)
            {
                serializedValue = null;
                return true;
            }

            if (value is string || Convert.GetTypeCode(value) != TypeCode.Object || value.GetType().IsValueType())
            {
                serializedValue = value;    // Already immutable, snapshot is not needed
                return true;
            }

            // Make snapshot of the context value
            serializedValue = Convert.ToString(value, logEvent.FormatProvider ?? LoggingConfiguration?.DefaultCultureInfo);
            return true;
        }

        [System.Diagnostics.DebuggerDisplay("Count = {Count}")]
        private sealed class AllPropertiesDictionary : IDictionary<string, object>, IDictionary
#if !NET35 && !NET40
    , IReadOnlyDictionary<string, object>
#endif
        {
            private readonly TargetWithContext _target;
            private readonly LogEventInfo _logEvent;
            private IDictionary<string, object> _inner;

            private IDictionary<string, object> Inner => _inner ?? (_inner = CreateDictionary());

            private AllPropertiesDictionary(TargetWithContext target, LogEventInfo logEvent)
            {
                _target = target;
                _logEvent = logEvent;
            }

            public static IDictionary<string, object> GetAllProperties(TargetWithContext target, LogEventInfo logEvent)
            {
                if (target.IncludeGdc || target.IncludeScopeProperties)
                    return null;

                bool checkContextProperties = target.ContextProperties?.Count > 0;
                var eventProperties = (target.IncludeEventProperties && logEvent.HasProperties) ? logEvent.CreateOrUpdatePropertiesInternal() : null;
                if (eventProperties?.Count > 0)
                {
                    bool checkExcludeProperties = target.ExcludeProperties?.Count > 0;
                    if (checkExcludeProperties || checkContextProperties)
                    {
                        if (eventProperties.Count * (target.ContextProperties?.Count ?? 1) > 20)
                            return null;

                        using (var propertyEnumerator = logEvent.CreateOrUpdatePropertiesInternal().GetPropertyEnumerator())
                        {
                            while (propertyEnumerator.MoveNext())
                            {
                                var property = propertyEnumerator.CurrentProperty;
                                if (string.IsNullOrEmpty(property.Key))
                                    continue;

                                if (checkExcludeProperties && target.ExcludeProperties.Contains(property.Key))
                                {
                                    return null;
                                }

                                if (checkContextProperties && !HasUniqueContextPropertyNames(target, property.Key))
                                {
                                    return null;
                                }
                            }
                        }
                    }
                }
                else if (!checkContextProperties)
                {
                    return null;
                }

                return new AllPropertiesDictionary(target, logEvent);
            }

            private static bool HasUniqueContextPropertyNames(TargetWithContext target, string propertyName)
            {
                for (int i = 0; i < target.ContextProperties.Count; ++i)
                {
                    var contextProperty = target.ContextProperties[i];
                    if (string.Equals(propertyName, contextProperty.Name, StringComparison.OrdinalIgnoreCase))
                    {
                        return false;
                    }
                }

                return true;
            }

            private IDictionary<string, object> CreateDictionary()
            {
                return Count == 0 ? new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase) : new Dictionary<string, object>(this, StringComparer.OrdinalIgnoreCase);
            }

            public bool TryGetValue(string key, out object value)
            {
                if (_inner is null)
                {
                    if (_target.ContextProperties?.Count > 0)
                    {
                        for (int i = 0; i < _target.ContextProperties.Count; ++i)
                        {
                            var contextProperty = _target.ContextProperties[i];
                            if (string.Equals(key, contextProperty.Name, StringComparison.OrdinalIgnoreCase))
                            {
                                if (_target.TryGetContextPropertyValue(_logEvent, contextProperty, out value))
                                {
                                    return true;
                                }
                            }
                        }
                    }

                    if (_logEvent.HasProperties)
                    {
                        if (_logEvent.Properties.TryGetValue(key, out value))
                        {
                            return true;
                        }

                        if (_logEvent.Properties.TryGetValue(new PropertiesDictionary.IgnoreCasePropertyKey(key), out value))
                        {
                            return true;
                        }
                    }

                    value = null;
                    return false;
                }
                else
                {
                    return _inner.TryGetValue(key, out value);
                }
            }

            public int Count
            {
                get
                {
                    if (_inner is null)
                    {
                        return (_logEvent.HasProperties ? _logEvent.Properties.Count : 0) + (_target.ContextProperties?.Count ?? 0);
                    }
                    else
                    {
                        return _inner.Count;
                    }
                }
            }

            public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
            {
                if (_inner is null)
                {
                    if (_target.ContextProperties?.Count > 0 || _logEvent.HasProperties)
                    {
                        return YieldProperties().GetEnumerator();
                    }

                    return System.Linq.Enumerable.Empty<KeyValuePair<string, object>>().GetEnumerator();
                }
                else
                {
                    return _inner.GetEnumerator();
                }
            }

            IEnumerable<KeyValuePair<string, object>> YieldProperties()
            {
                if (_logEvent.HasProperties)
                {
                    using (var propertyEnumerator = _logEvent.CreateOrUpdatePropertiesInternal().GetPropertyEnumerator())
                    {
                        while (propertyEnumerator.MoveNext())
                        {
                            var eventProperty = propertyEnumerator.CurrentProperty;
                            yield return eventProperty;
                        }
                    }
                }

                if (_target.ContextProperties?.Count > 0)
                {
                    for (int i = 0; i < _target.ContextProperties.Count; ++i)
                    {
                        var contextProperty = _target.ContextProperties[i];
                        if (_target.TryGetContextPropertyValue(_logEvent, contextProperty, out object propertyValue))
                        {
                            yield return new KeyValuePair<string, object>(contextProperty.Name, propertyValue);
                        }
                    }
                }
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
            IDictionaryEnumerator IDictionary.GetEnumerator() => ((IDictionary)Inner).GetEnumerator();
            bool IDictionary.IsReadOnly => false;
            bool ICollection<KeyValuePair<string, object>>.IsReadOnly => false;
            ICollection<string> IDictionary<string, object>.Keys => _inner is null && Count == 0 ? ArrayHelper.Empty<string>() : Inner.Keys;
            ICollection<object> IDictionary<string, object>.Values => _inner is null && Count == 0 ? ArrayHelper.Empty<object>() : Inner.Values;
#if !NET35 && !NET40
            IEnumerable<string> IReadOnlyDictionary<string, object>.Keys => _inner is null && Count == 0 ? ArrayHelper.Empty<string>() : Inner.Keys;
            IEnumerable<object> IReadOnlyDictionary<string, object>.Values => _inner is null && Count == 0 ? ArrayHelper.Empty<object>() : Inner.Values;
#endif
            object ICollection.SyncRoot => this;
            bool ICollection.IsSynchronized => false;
            ICollection IDictionary.Keys => ((IDictionary)Inner).Keys;
            ICollection IDictionary.Values => ((IDictionary)Inner).Values;
            bool IDictionary.IsFixedSize => false;
            public object this[string key]
            {
                get
                {
                    if (TryGetValue(key, out object value))
                        return value;
                    throw new KeyNotFoundException();
                }
                set => Inner[key] = value;
            }
            object IDictionary.this[object key] { get => ((IDictionary)Inner)[key]; set => ((IDictionary)Inner)[key] = value; }
            void IDictionary<string, object>.Add(string key, object value) => Inner.Add(key, value);
            void ICollection<KeyValuePair<string, object>>.Add(KeyValuePair<string, object> item) => Inner.Add(item);
            void IDictionary.Add(object key, object value) => ((IDictionary)Inner).Add(key, value);
            public void Clear()
            {
                if (Count != 0)
                {
                    if (_inner is null)
                        _inner = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                    else
                        _inner.Clear();
                }
            }
            public bool ContainsKey(string key) => TryGetValue(key, out var _);
            bool ICollection<KeyValuePair<string, object>>.Contains(KeyValuePair<string, object> item) => ContainsKey(item.Key) && Inner.Contains(item);
            bool IDictionary.Contains(object key) => key is string stringKey ? ContainsKey(stringKey) : ((IDictionary)Inner).Contains(key);
            bool IDictionary<string, object>.Remove(string key)
            {
                if (_inner is null && !ContainsKey(key))
                    return false;
                return Inner.Remove(key);
            }
            bool ICollection<KeyValuePair<string, object>>.Remove(KeyValuePair<string, object> item)
            {
                if (_inner is null && !ContainsKey(item.Key))
                    return false;
                return Inner.Remove(item);
            }
            void IDictionary.Remove(object key) => ((IDictionary)Inner).Remove(key);
            void ICollection<KeyValuePair<string, object>>.CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
            {
                Guard.ThrowIfNull(array);
                if (arrayIndex < 0)
                    throw new ArgumentOutOfRangeException(nameof(arrayIndex));

                if (Count != 0)
                {
                    foreach (var propertyItem in this)
                    {
                        array[arrayIndex++] = propertyItem;
                    }
                }
            }
            void ICollection.CopyTo(Array array, int index) => ((ICollection)Inner).CopyTo(array, index);
        }


        [ThreadAgnostic]
        internal sealed class TargetWithContextLayout : Layout, IIncludeContext, IUsesStackTrace
        {
            public Layout TargetLayout { get => _targetLayout; set => _targetLayout = ReferenceEquals(this, value) ? _targetLayout : value; }
            private Layout _targetLayout;

            /// <summary>Internal Layout that allows capture of <see cref="ScopeContext"/> properties-dictionary</summary>
            internal LayoutScopeContextProperties ScopeContextPropertiesLayout { get; }
            /// <summary>Internal Layout that allows capture of <see cref="ScopeContext"/> nested-states-stack</summary>
            internal LayoutScopeContextNestedStates ScopeContextNestedStatesLayout { get; }

            public bool IncludeEventProperties { get; set; }
            public bool IncludeCallSite { get; set; }
            public bool IncludeCallSiteStackTrace { get; set; }

            public bool IncludeScopeProperties
            {
                get => _includeScopeProperties ?? ScopeContextPropertiesLayout.IsActive;
                set => _includeScopeProperties = ScopeContextPropertiesLayout.IsActive = value;
            }
            private bool? _includeScopeProperties;

            public bool IncludeScopeNested
            {
                get => _includeScopeNested ?? ScopeContextNestedStatesLayout.IsActive;
                set => _includeScopeNested = ScopeContextNestedStatesLayout.IsActive = value;
            }
            private bool? _includeScopeNested;

            [Obsolete("Replaced by IncludeScopeProperties. Marked obsolete on NLog 5.0")]
            public bool IncludeMdc
            {
                get => _includeMdc ?? false;
                set
                {
                    _includeMdc = value;
                    ScopeContextPropertiesLayout.IsActive = _includeScopeProperties ?? (_includeMdlc == true || value);
                }
            }
            private bool? _includeMdc;

            [Obsolete("Replaced by IncludeScopeProperties. Marked obsolete on NLog 5.0")]
            public bool IncludeMdlc
            {
                get => _includeMdlc ?? false;
                set
                {
                    _includeMdlc = value;
                    ScopeContextPropertiesLayout.IsActive = _includeScopeProperties ?? (_includeMdc == true || value);
                }
            }
            private bool? _includeMdlc;

            [Obsolete("Replaced by IncludeScopeNested. Marked obsolete on NLog 5.0")]
            public bool IncludeNdc
            {
                get => _includeNdc ?? false;
                set
                {
                    _includeNdc = value;
                    ScopeContextNestedStatesLayout.IsActive = _includeScopeNested ?? (_includeNdlc == true || value);
                }
            }
            private bool? _includeNdc;

            [Obsolete("Replaced by IncludeScopeNested. Marked obsolete on NLog 5.0")]
            public bool IncludeNdlc
            {
                get => _includeNdlc ?? false;
                set
                {
                    _includeNdlc = value;
                    ScopeContextNestedStatesLayout.IsActive = _includeScopeNested ?? (_includeNdc == true || value);
                }
            }
            private bool? _includeNdlc;

            StackTraceUsage IUsesStackTrace.StackTraceUsage
            {
                get
                {
                    if (IncludeCallSiteStackTrace)
                    {
                        return StackTraceUsage.Max;
                    }

                    if (IncludeCallSite)
                    {
                        return StackTraceUsage.WithCallSite | StackTraceUsage.WithCallSiteClassName;
                    }
                    return StackTraceUsage.None;
                }
            }

            public TargetWithContextLayout(TargetWithContext owner, Layout targetLayout)
            {
                TargetLayout = targetLayout;

                ScopeContextPropertiesLayout = new LayoutScopeContextProperties(owner);
                ScopeContextNestedStatesLayout = new LayoutScopeContextNestedStates(owner);
            }

            protected override void InitializeLayout()
            {
                base.InitializeLayout();
                if (IncludeScopeProperties || IncludeScopeNested)
                    ThreadAgnostic = false;
                if (IncludeEventProperties)
                    ThreadAgnosticImmutable = true;   // TODO Need to convert Properties to an immutable state
            }

            public override string ToString()
            {
                return TargetLayout?.ToString() ?? base.ToString();
            }

            public override void Precalculate(LogEventInfo logEvent)
            {
                if (TargetLayout?.ThreadAgnostic == false || TargetLayout?.ThreadAgnosticImmutable == true)
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
                if (TargetLayout?.ThreadAgnostic == false || TargetLayout?.ThreadAgnosticImmutable == true)
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
                if (IncludeScopeProperties)
                    ScopeContextPropertiesLayout.Precalculate(logEvent);
                if (IncludeScopeNested)
                    ScopeContextNestedStatesLayout.Precalculate(logEvent);
            }

            protected override string GetFormattedMessage(LogEventInfo logEvent)
            {
                return TargetLayout?.Render(logEvent) ?? string.Empty;
            }

            protected override void RenderFormattedMessage(LogEventInfo logEvent, StringBuilder target)
            {
                TargetLayout?.Render(logEvent, target);
            }

            public class LayoutScopeContextProperties : Layout
            {
                private readonly TargetWithContext _owner;

                public bool IsActive { get; set; }

                public LayoutScopeContextProperties(TargetWithContext owner)
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
                    if (IsActive && !logEvent.TryGetCachedLayoutValue(this, out var _))
                    {
                        var scopeContextProperties = _owner.CaptureScopeContextProperties(logEvent, null);
                        logEvent.AddCachedLayoutValue(this, scopeContextProperties);
                    }
                }
            }

            public class LayoutScopeContextNestedStates : Layout
            {
                private readonly TargetWithContext _owner;

                public bool IsActive { get; set; }

                public LayoutScopeContextNestedStates(TargetWithContext owner)
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
                    if (IsActive && !logEvent.TryGetCachedLayoutValue(this, out var _))
                    {
                        var nestedContext = _owner.CaptureScopeContextNested(logEvent);
                        logEvent.AddCachedLayoutValue(this, nestedContext);
                    }
                }
            }
        }
    }
}
