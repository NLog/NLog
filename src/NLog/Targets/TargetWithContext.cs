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
                bool checkExcludeProperties = ExcludeProperties.Count > 0;
                foreach (var property in logEvent.Properties)
                {
                    string propertyName = property.Key.ToString();
                    if (string.IsNullOrEmpty(propertyName))
                        continue;

                    if (checkExcludeProperties && ExcludeProperties.Contains(propertyName))
                        continue;

                    AddContextProperty(logEvent, propertyName, property.Value, checkForDuplicates, combinedProperties);
                }
            }
            combinedProperties = GetContextProperties(logEvent, combinedProperties);
            return combinedProperties ?? new Dictionary<string, object>(StringComparer.Ordinal);
        }

        private static IDictionary<string, object> CreateNewDictionary(int initialCapacity)
        {
            return new Dictionary<string, object>(Math.Max(initialCapacity, 3), StringComparer.Ordinal);
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

                    Common.InternalLogger.Warn(ex, "{0}: Failed to add context property {1}", this, contextProperty.Name);
                }
            }

            return combinedProperties;
        }

        private static bool TryGetContextPropertyValue(LogEventInfo logEvent, TargetPropertyWithContext contextProperty, out object propertyValue)
        {
            propertyValue = contextProperty.RenderValue(logEvent);
            if (!contextProperty.IncludeEmptyValue && (propertyValue is null || string.Empty.Equals(propertyValue)))
            {
                return false;
            }

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
