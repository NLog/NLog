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

using System;
using System.Collections.Generic;
using NLog.Internal;

namespace NLog
{
    /// <summary>
    /// <see cref="ScopeContext"/> allows one to store state in the thread execution context. All LogEvents created
    /// within a scope can include the scope state when wanted. The logical context scope supports both
    /// scope-properties and scope-nested-state-stack (Similar to log4j2 ThreadContext)
    /// </summary>
    /// <remarks>
    /// <see cref="MappedDiagnosticsLogicalContext"/> (MDLC), <see cref="MappedDiagnosticsContext"/> (MDC), <see cref="NestedDiagnosticsLogicalContext"/> (NDLC)
    /// and <see cref="NestedDiagnosticsContext"/> (NDC) has been deprecated and replaced by <see cref="ScopeContext"/>.
    /// 
    /// .NetCore (and .Net46) uses AsyncLocal for handling the thread execution context. Older .NetFramework uses System.Runtime.Remoting.CallContext
    /// </remarks>
    public static class ScopeContext
    {
#if !NET35 && !NET40
        /// <summary>
        /// Pushes new state on the logical context scope stack, and includes the provided properties
        /// </summary>
        /// <param name="nestedState">Value to added to the scope stack</param>
        /// <param name="properties">Properties being added to the scope dictionary</param>
        /// <returns>A disposable object that pops the nested scope state on dispose (including properties).</returns>
        /// <remarks>Scope dictionary keys are case-insensitive</remarks>
        public static IDisposable PushNestedStateProperties(object nestedState, IReadOnlyList<KeyValuePair<string, object>> properties)
        {
#if !NET45
            var parent = GetThreadLocal();
            IScopeContext current = null;
            if (properties?.Count > 0)
                current = new ScopeContextProperties(parent, properties, nestedState);
            else 
                current = new ScopeContextNestedState<object>(parent, nestedState);
            SetThreadLocal(current);
            return current;
#else
            if (properties?.Count > 0)
            {
                var mldcScope = MappedDiagnosticsLogicalContext.PushProperties(properties);
                var ndlcScope = NestedDiagnosticsLogicalContext.PushNestedState(nestedState);
                return new ScopeContextNestedStateProperties(ndlcScope, mldcScope);
            }
            else
            {
                return NestedDiagnosticsLogicalContext.PushNestedState(nestedState);
            }
#endif
        }
#endif

#if !NET35 && !NET40
        /// <summary>
        /// Updates the logical scope context with provided properties
        /// </summary>
        /// <param name="properties">Properties being added to the scope dictionary</param>
        /// <returns>A disposable object that removes the properties from logical context scope on dispose.</returns>
        /// <remarks>Scope dictionary keys are case-insensitive</remarks>
        public static IDisposable PushProperties(IReadOnlyList<KeyValuePair<string, object>> properties)
        {
#if !NET45
            var parent = GetThreadLocal();
            
            var scopeProperties = ScopeContextPropertiesCollapsed.BuildCollapsedDictionary(parent, properties.Count);
            if (scopeProperties != null)
            {
                // Collapse all 3 property-scopes into a collapsed scope, and return bookmark that can restore original parent (Avoid huge object-graphs)
                for (int i = 0; i < properties.Count; ++i)
                {
                    var property = properties[i];
                    scopeProperties[property.Key] = property.Value;
                }
                var collapsedState = new ScopeContextProperties(parent.Parent.Parent, scopeProperties);
                SetThreadLocal(collapsedState);
                return new ScopeContextPropertiesCollapsed(parent, collapsedState);
            }

            var current = new ScopeContextProperties(parent, properties, null);
            SetThreadLocal(current);
            return current;
#else
            return MappedDiagnosticsLogicalContext.PushProperties(properties);
#endif
        }
#endif

        /// <summary>
        /// Updates the logical scope context with provided property
        /// </summary>
        /// <param name="key">Name of property</param>
        /// <param name="value">Value of property</param>
        /// <returns>A disposable object that removes the properties from logical context scope on dispose.</returns>
        /// <remarks>Scope dictionary keys are case-insensitive</remarks>
        public static IDisposable PushProperty<T>(string key, T value)
        {
#if !NET35 && !NET40 && !NET45
            var parent = GetThreadLocal();

            var scopeProperties = ScopeContextPropertiesCollapsed.BuildCollapsedDictionary(parent, 1);
            if (scopeProperties != null)
            {
                // Collapse all 3 property-scopes into a collapsed scope, and return bookmark that can restore original parent (Avoid huge object-graphs)
                scopeProperties[key] = value;

                var collapsedState = new ScopeContextProperties(parent.Parent.Parent, scopeProperties);
                SetThreadLocal(collapsedState);
                return new ScopeContextPropertiesCollapsed(parent, collapsedState);
            }

            var current = new ScopeContextProperty<T>(parent, key, value);
            SetThreadLocal(current);
            return current;
#else
            return MappedDiagnosticsLogicalContext.PushProperty(key, value);
#endif
        }

        /// <summary>
        /// Updates the logical scope context with provided property
        /// </summary>
        /// <param name="key">Name of property</param>
        /// <param name="value">Value of property</param>
        /// <returns>A disposable object that removes the properties from logical context scope on dispose.</returns>
        /// <remarks>Scope dictionary keys are case-insensitive</remarks>
        public static IDisposable PushProperty(string key, object value)
        {
            return PushProperty<object>(key, value);
        }

        /// <summary>
        /// Pushes new state on the logical context scope stack
        /// </summary>
        /// <param name="nestedState">Value to added to the scope stack</param>
        /// <returns>A disposable object that pops the nested scope state on dispose.</returns>
        /// <remarks>Skips casting of <paramref name="nestedState"/> to check for scope-properties</remarks>
        public static IDisposable PushNestedState<T>(T nestedState)
        {
#if !NET35 && !NET40 && !NET45
            var parent = GetThreadLocal();
            var current = new ScopeContextNestedState<T>(parent, nestedState);
            SetThreadLocal(current);
            return current;
#else
            return NestedDiagnosticsLogicalContext.PushNestedState(nestedState);
#endif
        }

        /// <summary>
        /// Pushes new state on the logical context scope stack
        /// </summary>
        /// <param name="nestedState">Value to added to the scope stack</param>
        /// <returns>A disposable object that pops the nested scope state on dispose.</returns>
        public static IDisposable PushNestedState(object nestedState)
        {
            return PushNestedState<object>(nestedState);
        }

        /// <summary>
        /// Clears all the entire logical context scope, and removes any properties and nested-states
        /// </summary>
        public static void Clear()
        {
#if !NET35 && !NET40 && !NET45
            SetThreadLocal(null);
#else
            MappedDiagnosticsLogicalContext.ClearMappedContext();
            NestedDiagnosticsLogicalContext.ClearNestedContext();
#endif
        }

        /// <summary>
        /// Retrieves all properties stored within the logical context scopes
        /// </summary>
        /// <returns>Collection of all properties</returns>
        public static IEnumerable<KeyValuePair<string, object>> GetAllProperties()
        {
#if !NET35 && !NET40 && !NET45
            var contextState = GetThreadLocal();
            return contextState?.CaptureContextProperties(0, out var _) ?? ArrayHelper.Empty<KeyValuePair<string, object>>();
#else
            return MappedDiagnosticsLogicalContext.GetAllProperties();
#endif
        }

        /// <summary>
        /// Lookup single property stored within the logical context scopes
        /// </summary>
        /// <param name="key">Name of property</param>
        /// <param name="value">When this method returns, contains the value associated with the specified key</param>
        /// <returns>Returns true when value is found with the specified key</returns>
        /// <remarks>Scope dictionary keys are case-insensitive</remarks>
        public static bool TryGetProperty(string key, out object value)
        {
#if !NET35 && !NET40 && !NET45
            var contextState = GetThreadLocal();
            if (contextState != null)
            {
                var mappedContext = contextState?.CaptureContextProperties(0, out var _);
                if (mappedContext != null)
                {
                    if (mappedContext is IDictionary<string, object> dictionary)
                    {
                        return dictionary.TryGetValue(key, out value);
                    }
                    else
                    {
                        return TryFindPropertyValue(mappedContext, key, out value);
                    }
                }
            }
            value = null;
            return false;
#else
            return MappedDiagnosticsLogicalContext.TryGetProperty(key, out value);
#endif
        }

        /// <summary>
        /// Retrieves all nested states inside the logical context scope stack
        /// </summary>
        /// <returns>Array of nested state objects.</returns>
        public static object[] GetAllNestedStates()
        {
#if !NET35 && !NET40 && !NET45
            var parent = GetThreadLocal();
            return parent?.CaptureNestedContext(0, out var _) ?? ArrayHelper.Empty<object>();
#else
            return NestedDiagnosticsLogicalContext.GetAllNestedStates();
#endif
        }

        /// <summary>
        /// Peeks the top value from the logical context scope stack
        /// </summary>
        /// <returns>Value from the top of the stack.</returns>
        public static object PeekNestedState()
        {
#if !NET35 && !NET40 && !NET45
            var parent = GetThreadLocal();
            while (parent != null)
            {
                var nestedContext = parent.NestedState;
                if (nestedContext != null)
                    return nestedContext;

                parent = parent.Parent;
            }
            return null;
#else
            return NestedDiagnosticsLogicalContext.PeekNestedState();
#endif
        }

        /// <summary>
        /// Peeks the inner state (newest) from the logical context scope stack, and returns its running duration
        /// </summary>
        /// <returns>Scope Duration Time</returns>
        internal static TimeSpan? PeekInnerNestedDuration()
        {
#if !NET35 && !NET40 && !NET45
            var stopwatchNow = GetNestedContextTimestampNow(); // Early timestamp to reduce chance of measuring NLog time
            var parent = GetThreadLocal();
            while (parent != null)
            {
                var scopeTimestamp = parent.NestedStateTimestamp;
                if (scopeTimestamp != 0)
                {
                    return GetNestedStateDuration(scopeTimestamp, stopwatchNow);
                }

                parent = parent.Parent;
            }
            return null;
#else
            return default(TimeSpan?);  // Delay timing only supported when using AsyncLocal. CallContext is very sensitive.
#endif
        }

        /// <summary>
        /// Peeks the outer state (oldest) from the logical context scope stack, and returns its running duration
        /// </summary>
        /// <returns>Scope Duration Time</returns>
        internal static TimeSpan? PeekOuterNestedDuration()
        {
#if !NET35 && !NET40 && !NET45
            var stopwatchNow = GetNestedContextTimestampNow(); // Early timestamp to reduce chance of measuring NLog time
            var parent = GetThreadLocal();
            var scopeTimestamp = 0L;
            while (parent != null)
            {
                if (parent.NestedStateTimestamp != 0)
                    scopeTimestamp = parent.NestedStateTimestamp;
                parent = parent.Parent;
            }

            if (scopeTimestamp != 0L)
            {
                return GetNestedStateDuration(scopeTimestamp, stopwatchNow);
            }

            return null;
#else
            return default(TimeSpan?);  // Delay timing only supported when using AsyncLocal. CallContext is very sensitive.
#endif
        }

#if !NET35 && !NET40 && !NET45
        internal static void RemoveMappedContextLegacy(string key)
        {
            if (TryGetProperty(key, out var _))
            {
                // Replace with new legacy-scope, the legacy-scope can be discarded when previous parent scope is restored
                var contextState = GetThreadLocal();
                CaptureLegacyContext(contextState, out var scopeProperties, out var scopeNestedStates, out var scopeNestedStateTimestamp);
                scopeProperties.Remove(key);

                var legacyScope = new LegacyScopeContext(scopeProperties, scopeNestedStates, scopeNestedStateTimestamp);
                SetThreadLocal(legacyScope);
            }
        }

        internal static object PopNestedContextLegacy()
        {
            var contextState = GetThreadLocal();
            if (contextState != null)
            {
                if ((contextState.Parent == null && contextState is LegacyScopeContext) || contextState.NestedState == null)
                {
                    var nestedContext = contextState?.CaptureNestedContext(0, out var _) ?? ArrayHelper.Empty<object>();
                    if (nestedContext.Length == 0)
                        return null;    // Nothing to pop, just leave scope alone

                    // Replace with new legacy-scope, the legacy-scope can be discarded when previous parent scope is restored
                    var stackTopValue = nestedContext[0];
                    var scopeProperties = contextState?.CaptureContextProperties(0, out var _) ?? ArrayHelper.Empty<KeyValuePair<string, object>>();
                    if (nestedContext.Length == 1)
                    {
                        nestedContext = ArrayHelper.Empty<object>();
                    }
                    else
                    {
                        var newScope = new object[nestedContext.Length - 1];
                        for (int i = 0; i < newScope.Length; ++i)
                            newScope[i] = nestedContext[i + 1];
                        nestedContext = newScope;
                    }

                    var legacyScope = new LegacyScopeContext(scopeProperties, nestedContext, nestedContext.Length > 0 ? GetNestedContextTimestampNow() : 0L);
                    SetThreadLocal(legacyScope);
                    return stackTopValue;
                }
                else
                {
                    SetThreadLocal(contextState.Parent);
                    return contextState?.NestedState;
                }
            }

            return null;
        }

        internal static void ClearMappedContextLegacy()
        {
            var contextState = GetThreadLocal();
            if (contextState != null)
            {
                CaptureLegacyContext(contextState, out var scopeProperties, out var nestedContext, out var nestedContextTimestamp);
                if (nestedContext?.Length > 0)
                {
                    if (scopeProperties?.Count > 0)
                    {
                        var legacyScope = new LegacyScopeContext(null, nestedContext, nestedContextTimestamp);
                        SetThreadLocal(legacyScope);
                    }
                }
                else
                {
                    SetThreadLocal(null);
                }
            }
        }

        internal static void ClearNestedContextLegacy()
        {
            var contextState = GetThreadLocal();
            if (contextState != null)
            {
                CaptureLegacyContext(contextState, out var scopeProperties, out var nestedContext, out var nestedContextTimestamp);
                if (scopeProperties?.Count > 0)
                {
                    if (nestedContext?.Length > 0)
                    {
                        var legacyScope = new LegacyScopeContext(scopeProperties, ArrayHelper.Empty<object>(), 0L);
                        SetThreadLocal(legacyScope);
                    }
                }
                else
                {
                    SetThreadLocal(null);
                }
            }
        }

        private static bool TryFindPropertyValue(IEnumerable<KeyValuePair<string, object>> scopeProperties, string key, out object value)
        {
            if (scopeProperties is IReadOnlyList<KeyValuePair<string, object>> mappedList)
            {
                for (int i = 0; i < mappedList.Count; ++i)
                {
                    var item = mappedList[i];
                    if (DefaultComparer.Equals(item.Key, key))
                    {
                        value = item.Value;
                        return true;
                    }
                }
            }
            else
            {
                foreach (var item in scopeProperties)
                {
                    if (DefaultComparer.Equals(item.Key, key))
                    {
                        value = item.Value;
                        return true;
                    }
                }
            }

            value = null;
            return false;
        }

        private static void CaptureLegacyContext(IScopeContext contextState, out Dictionary<string, object> scopeDictionary, out object[] nestedContext, out long nestedContextTimestamp)
        {
            nestedContext = contextState?.CaptureNestedContext(0, out var _) ?? ArrayHelper.Empty<object>();
            scopeDictionary = null;
            var scopeProperties = contextState?.CaptureContextProperties(0, out scopeDictionary) ?? ArrayHelper.Empty<KeyValuePair<string, object>>();
            if (scopeDictionary == null)
            {
                scopeDictionary = new Dictionary<string, object>(scopeProperties.Count, DefaultComparer);
                foreach (var scopeProperty in scopeProperties)
                    scopeDictionary[scopeProperty.Key] = scopeProperty.Value;
            }

            nestedContextTimestamp = 0L;
            if (nestedContext?.Length > 0)
            {
                var parent = contextState;
                while (parent != null)
                {
                    if (parent.NestedStateTimestamp != 0L)
                        nestedContextTimestamp = parent.NestedStateTimestamp;
                    parent = parent.Parent;
                }

                if (nestedContextTimestamp == 0L)
                    nestedContextTimestamp = GetNestedContextTimestampNow();
            }
        }

        private static long GetNestedContextTimestampNow()
        {
            if (System.Diagnostics.Stopwatch.IsHighResolution)
                return System.Diagnostics.Stopwatch.GetTimestamp();
            else
                return System.Environment.TickCount;
        }

        private static TimeSpan GetNestedStateDuration(long scopeTimestamp, long currentTimestamp)
        {
            if (System.Diagnostics.Stopwatch.IsHighResolution)
                return TimeSpan.FromTicks((currentTimestamp - scopeTimestamp) * TimeSpan.TicksPerSecond / System.Diagnostics.Stopwatch.Frequency);
            else
                return TimeSpan.FromMilliseconds((int)currentTimestamp - (int)scopeTimestamp);
        }

        private static Dictionary<string, object> CloneParentContextDictionary(IEnumerable<KeyValuePair<string, object>> parentContext, int initialCapacity)
        {
            if (parentContext is IReadOnlyList<KeyValuePair<string, object>> parentPropertyList)
            {
                var scopeProperties = new Dictionary<string, object>(parentPropertyList.Count + initialCapacity, DefaultComparer);
                for (int i = 0; i < parentPropertyList.Count; ++i)
                {
                    var item = parentPropertyList[i];
                    scopeProperties[item.Key] = item.Value;
                }
                return scopeProperties;
            }
            else if (parentContext is Dictionary<string, object> parentPropertyDictionary)
            {
                var scopeProperties = new Dictionary<string, object>(parentPropertyDictionary.Count + initialCapacity, DefaultComparer);
                foreach (var item in parentPropertyDictionary)
                {
                    scopeProperties[item.Key] = item.Value;
                }
                return scopeProperties;
            }
            else
            {
                var scopeProperties = new Dictionary<string, object>(1 + initialCapacity, DefaultComparer);
                foreach (var item in parentContext)
                {
                    scopeProperties[item.Key] = item.Value;
                }
                return scopeProperties;
            }
        }

        interface IScopeContext : IDisposable
        {
            IScopeContext Parent { get; }
            object NestedState { get; }
            long NestedStateTimestamp { get; }
            IReadOnlyCollection<KeyValuePair<string, object>> CaptureContextProperties(int initialCapacity, out Dictionary<string, object> scopeProperties);
            object[] CaptureNestedContext(int initialCapacity, out object[] nestedContext);
        }

        interface IPropertyScopeContext : IScopeContext
        {
        }

        /// <summary>
        /// Special bookmark that can restore original parent, after scopes has been collapsed
        /// </summary>
        private sealed class ScopeContextPropertiesCollapsed : IDisposable
        {
            private readonly IScopeContext _parent;
            private readonly IPropertyScopeContext _collapsed;
            private bool _disposed;

            public ScopeContextPropertiesCollapsed(IScopeContext parent, IPropertyScopeContext collapsed)
            {
                _parent = parent;
                _collapsed = collapsed;
            }

            public static Dictionary<string, object> BuildCollapsedDictionary(IScopeContext parent, int initialCapacity)
            {
                if (parent is IPropertyScopeContext parentProperties && parentProperties.Parent is IPropertyScopeContext grandParentProperties)
                {
                    if (parentProperties.NestedState == null && grandParentProperties.NestedState == null)
                    {
                        var scopeProperties = parentProperties.CaptureContextProperties(initialCapacity, out var scopeDictionary) ?? ArrayHelper.Empty<KeyValuePair<string, object>>();
                        if (scopeDictionary == null)
                        {
                            scopeDictionary = new Dictionary<string, object>(scopeProperties.Count + initialCapacity, DefaultComparer);
                            foreach (var scopeProperty in scopeProperties)
                                scopeDictionary[scopeProperty.Key] = scopeProperty.Value;
                        }

                        return scopeDictionary;
                    }
                }

                return null;
            }

            public void Dispose()
            {
                if (_disposed)
                    return;

                _disposed = true;
                SetThreadLocal(_parent);
            }

            public override string ToString()
            {
                return _collapsed.ToString();
            }
        }

        private sealed class ScopeContextNestedState<T> : IScopeContext
        {
            private readonly T _value;
            private bool _disposed;

            public ScopeContextNestedState(IScopeContext parent, T state)
            {
                Parent = parent;
                NestedStateTimestamp = GetNestedContextTimestampNow();
                _value = state;
            }

            public IScopeContext Parent { get; }

            object IScopeContext.NestedState => _value;

            public long NestedStateTimestamp { get; }

            object[] IScopeContext.CaptureNestedContext(int initialCapacity, out object[] nestedContext)
            {
                nestedContext = null;
                Parent?.CaptureNestedContext(initialCapacity + 1, out nestedContext);
                if (nestedContext == null)
                    nestedContext = new object[initialCapacity + 1];
                nestedContext[initialCapacity] = _value;
                return nestedContext;
            }

            IReadOnlyCollection<KeyValuePair<string, object>> IScopeContext.CaptureContextProperties(int initialCapacity, out Dictionary<string, object> scopeProperties)
            {
                scopeProperties = null;
                return Parent?.CaptureContextProperties(initialCapacity, out scopeProperties) ?? scopeProperties;
            }

            public override string ToString()
            {
                return _value?.ToString() ?? "null";
            }

            public void Dispose()
            {
                if (!_disposed)
                {
                    SetThreadLocal(Parent);
                    _disposed = true;
                }
            }
        }

        private sealed class ScopeContextProperty<T> : IPropertyScopeContext
        {
            public IScopeContext Parent { get; }
            long IScopeContext.NestedStateTimestamp => 0;
            object IScopeContext.NestedState => null;
            public string Name { get; }
            public T Value { get; }
            private IReadOnlyCollection<KeyValuePair<string, object>> _scopeDictionary;
            private bool _disposed;

            public ScopeContextProperty(IScopeContext parent, string name, T value)
            {
                Parent = parent;
                Name = name;
                Value = value;
            }

            object[] IScopeContext.CaptureNestedContext(int initialCapacity, out object[] nestedContext)
            {
                nestedContext = null;
                return Parent?.CaptureNestedContext(initialCapacity, out nestedContext) ?? ArrayHelper.Empty<object>();
            }

            IReadOnlyCollection<KeyValuePair<string, object>> IScopeContext.CaptureContextProperties(int initialCapacity, out Dictionary<string, object> scopeProperties)
            {
                scopeProperties = null;
                if (_scopeDictionary != null)
                {
                    return _scopeDictionary;
                }
                else
                {
                    var parentContext = Parent?.CaptureContextProperties(initialCapacity + 1, out scopeProperties);
                    if (scopeProperties == null)
                    {
                        if (parentContext == null)
                        {
                            // No more parent-context, build scope-property-collection starting from this scope
                            if (initialCapacity == 0)
                                return _scopeDictionary = new[] { new KeyValuePair<string, object>(Name, Value) };
                            else
                                scopeProperties = new Dictionary<string, object>(initialCapacity + 1, DefaultComparer);
                        }
                        else
                        {
                            // Build scope-property-collection from parent-context
                            scopeProperties = CloneParentContextDictionary(parentContext, initialCapacity + 1);
                        }
                    }

                    scopeProperties[Name] = Value;
                    if (initialCapacity == 0)
                        _scopeDictionary = scopeProperties; // Immutable since no more scope-properties
                    return scopeProperties;
                }
            }

            public override string ToString()
            {
                return $"{Name}={Value?.ToString() ?? "null"}";
            }

            public void Dispose()
            {
                if (!_disposed)
                {
                    SetThreadLocal(Parent);
                    _disposed = true;
                }
            }
        }

        private sealed class ScopeContextProperties : IPropertyScopeContext
        {
            public IScopeContext Parent { get; }
            public long NestedStateTimestamp { get; }
            public object NestedState { get; }
            public IReadOnlyList<KeyValuePair<string, object>> ScopeProperties { get; }
            private IReadOnlyCollection<KeyValuePair<string, object>> _scopeDictionary;
            private bool _disposed;

            public ScopeContextProperties(IScopeContext parent, Dictionary<string, object> scopeDictionary)
            {
                Parent = parent;
                _scopeDictionary = scopeDictionary;
            }

            public ScopeContextProperties(IScopeContext parent, IReadOnlyList<KeyValuePair<string, object>> scopeProperties, object nestedState)
            {
                Parent = parent;
                ScopeProperties = scopeProperties;
                NestedState = nestedState;
                NestedStateTimestamp = nestedState != null ? GetNestedContextTimestampNow() : 0;
            }

            object[] IScopeContext.CaptureNestedContext(int initialCapacity, out object[] nestedContext)
            {
                nestedContext = null;
                int extraCount = (NestedState != null ? 1 : 0);
                Parent?.CaptureNestedContext(initialCapacity + extraCount, out nestedContext);
                if (extraCount > 0)
                {
                    if (nestedContext == null)
                        nestedContext = new object[initialCapacity + extraCount];
                    nestedContext[initialCapacity] = NestedState;
                }
                return nestedContext;
            }

            IReadOnlyCollection<KeyValuePair<string, object>> IScopeContext.CaptureContextProperties(int initialCapacity, out Dictionary<string, object> scopeProperties)
            {
                scopeProperties = null;
                if (_scopeDictionary != null)
                {
                    return _scopeDictionary;
                }
                else
                {
                    var parentContext = Parent?.CaptureContextProperties(initialCapacity + ScopeProperties.Count, out scopeProperties);
                    if (scopeProperties == null)
                    {
                        if (parentContext == null)
                        {
                            // No more parent-context, build scope-property-collection starting from this scope
                            if (initialCapacity == 0)
                                return _scopeDictionary = EnsureCollectionWithUniqueKeys();
                            else
                                scopeProperties = new Dictionary<string, object>(initialCapacity + ScopeProperties.Count, DefaultComparer);
                        }
                        else
                        {
                            // Build scope-property-collection from parent-context
                            scopeProperties = CloneParentContextDictionary(parentContext, initialCapacity + ScopeProperties.Count);
                        }
                    }

                    AppendScopeProperties(scopeProperties);
                    if (initialCapacity == 0)
                        _scopeDictionary = scopeProperties; // Immutable since no more scope-properties
                    return scopeProperties;
                }
            }

            private IReadOnlyCollection<KeyValuePair<string, object>> EnsureCollectionWithUniqueKeys()
            {
                if (ScopeProperties.Count > 10)
                {
                    var mappedContext = new Dictionary<string, object>(ScopeProperties.Count, DefaultComparer);
                    AppendScopeProperties(mappedContext);
                    return mappedContext;
                }

                for (int i = 0; i < ScopeProperties.Count - 1; ++i)
                {
                    var left = ScopeProperties[i];
                    for (int j = i + 1; j < ScopeProperties.Count; ++j)
                    {
                        var right = ScopeProperties[j];
                        if (DefaultComparer.Equals(left.Key, right.Key))
                        {
                            var mappedContext = new Dictionary<string, object>(ScopeProperties.Count, DefaultComparer);
                            AppendScopeProperties(mappedContext);
                            return mappedContext;
                        }
                    }
                }

                return ScopeProperties;
            }

            private void AppendScopeProperties(Dictionary<string, object> mappedContext)
            {
                for (int i = 0; i < ScopeProperties.Count; ++i)
                {
                    var item = ScopeProperties[i];
                    mappedContext[item.Key] = item.Value;
                }
            }

            public override string ToString()
            {
                return NestedState?.ToString() ?? base.ToString();
            }

            public void Dispose()
            {
                if (!_disposed)
                {
                    SetThreadLocal(Parent);
                    _disposed = true;
                }
            }
        }

        private sealed class LegacyScopeContext : IScopeContext
        {
            public IScopeContext Parent => null;    // Always top parent
            public object[] NestedContext { get; }
            public IReadOnlyCollection<KeyValuePair<string, object>> MappedContext { get; }
            public long NestedStateTimestamp { get; }
            private bool _disposed;

            public LegacyScopeContext(IReadOnlyCollection<KeyValuePair<string, object>> mappedContext, object[] nestedContext, long nestedContextTimestamp)
            {
                MappedContext = mappedContext;
                NestedContext = nestedContext;
                NestedStateTimestamp = nestedContextTimestamp;
            }

            object IScopeContext.NestedState => NestedContext?.Length > 0 ? NestedContext[0] : null;

            object[] IScopeContext.CaptureNestedContext(int initialCapacity, out object[] nestedContext)
            {
                nestedContext = null;
                int extraCount = NestedContext?.Length ?? 0;
                Parent?.CaptureNestedContext(initialCapacity + extraCount, out nestedContext);
                if (extraCount > 0)
                {
                    if (nestedContext == null)
                    {
                        if (initialCapacity == 0)
                            return NestedContext;
                        else
                            nestedContext = new object[initialCapacity + extraCount];
                    }

                    for (int i = 0; i < extraCount; ++i)
                        nestedContext[initialCapacity + i] = NestedContext[i];
                }
                return nestedContext;
            }

            IReadOnlyCollection<KeyValuePair<string, object>> IScopeContext.CaptureContextProperties(int initialCapacity, out Dictionary<string, object> scopeProperties)
            {
                scopeProperties = null;
                return MappedContext;
            }

            public override string ToString()
            {
                if (NestedContext?.Length > 0)
                    return NestedContext[NestedContext.Length - 1]?.ToString() ?? "null";
                else
                    return base.ToString();
            }

            public void Dispose()
            {
                if (!_disposed)
                {
                    SetThreadLocal(Parent);
                    _disposed = true;
                }
            }
        }

        private static void SetThreadLocal(IScopeContext newValue)
        {
            AsyncNestedDiagnosticsContext.Value = newValue;
        }

        private static IScopeContext GetThreadLocal()
        {
            return AsyncNestedDiagnosticsContext.Value;
        }

        private static IEqualityComparer<string> DefaultComparer = StringComparer.OrdinalIgnoreCase;                

        private static readonly System.Threading.AsyncLocal<IScopeContext> AsyncNestedDiagnosticsContext = new System.Threading.AsyncLocal<IScopeContext>();
#endif

#if NET45
        private sealed class ScopeContextNestedStateProperties : IDisposable
        {
            private readonly IDisposable _mldcScope;
            private readonly IDisposable _ndlcScope;

            public ScopeContextNestedStateProperties(IDisposable ndlcScope, IDisposable mldcScope)
            {
                _ndlcScope = ndlcScope;
                _mldcScope = mldcScope;
            }

            public void Dispose()
            {
                try
                {
                    _mldcScope?.Dispose();
                }
                catch (Exception ex)
                {
                    NLog.Common.InternalLogger.Debug(ex, "Exception in BeginScope dispose MappedDiagnosticsLogicalContext");
                }

                try
                {
                    _ndlcScope?.Dispose();
                }
                catch (Exception ex)
                {
                    NLog.Common.InternalLogger.Debug(ex, "Exception in BeginScope dispose NestedDiagnosticsLogicalContext");
                }
            }
        }
#endif

        internal struct ScopePropertiesEnumerator : IEnumerator<KeyValuePair<string, object>>
        {
            readonly IEnumerator<KeyValuePair<string, object>> _scopeEnumerator;
#if !NET35 && !NET40
            readonly IReadOnlyList<KeyValuePair<string, object>> _scopeList;
            int _scopeIndex;
#endif
            Dictionary<string, object>.Enumerator _dicationaryEnumerator;

            public ScopePropertiesEnumerator(IEnumerable<KeyValuePair<string, object>> scopeProperties)
            {
                if (scopeProperties is Dictionary<string, object> scopeDictionary)
                {
                    _scopeEnumerator = null;
#if !NET35 && !NET40
                    _scopeList = null;
                    _scopeIndex = 0;
#endif
                    _dicationaryEnumerator = scopeDictionary.GetEnumerator();
                }
#if !NET35 && !NET40
                else if (scopeProperties is IReadOnlyList<KeyValuePair<string, object>> scopeList)
                {
                    _scopeEnumerator = null;
                    _scopeList = scopeList;
                    _scopeIndex = -1;
                    _dicationaryEnumerator = default(Dictionary<string, object>.Enumerator);
                }
#endif
                else
                {
                    _scopeEnumerator = scopeProperties.GetEnumerator();
#if !NET35 && !NET40
                    _scopeList = null;
                    _scopeIndex = 0;
#endif
                    _dicationaryEnumerator = default(Dictionary<string, object>.Enumerator);
                }
            }

            public KeyValuePair<string, object> Current
            {
                get
                {
#if !NET35 && !NET40
                    if (_scopeList != null)
                    {
                        return _scopeList[_scopeIndex];
                    }
                    else
#endif
                    if (_scopeEnumerator != null)
                    {
                        return _scopeEnumerator.Current;
                    }
                    else
                    {
                        return _dicationaryEnumerator.Current;
                    }
                }
            }

            object System.Collections.IEnumerator.Current => Current;

            public void Dispose()
            {
                if (_scopeEnumerator != null)
                    _scopeEnumerator.Dispose();
                else
#if !NET35 && !NET40
                if (_scopeList == null)
#endif
                    _dicationaryEnumerator.Dispose();
            }

            public bool MoveNext()
            {
#if !NET35 && !NET40
                if (_scopeList != null)
                {
                    if (_scopeIndex < _scopeList.Count - 1)
                    {
                        ++_scopeIndex;
                        return true;
                    }
                    return false;
                }
                else
#endif
                if (_scopeEnumerator != null)
                {
                    return _scopeEnumerator.MoveNext();
                }
                else
                {
                    return _dicationaryEnumerator.MoveNext();
                }
            }

            public void Reset()
            {
#if !NET35 && !NET40
                if (_scopeList != null)
                {
                    _scopeIndex = -1;
                }
                else
#endif
                if (_scopeEnumerator != null)
                {
                    _scopeEnumerator.Reset();
                }
                else
                {
                    _dicationaryEnumerator = default(Dictionary<string, object>.Enumerator);
                }
            }
        }
    }
}
